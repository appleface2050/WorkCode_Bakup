using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;

namespace BlueStacks.hyperDroid.Frontend
{
	class GraphicsDriverUpdater : Form
	{
		private Console s_Console;
		private Label statusControl;
		private ProgressBar progressControl;
		private Button exitControl;
		private Downloader mDownloader;

		public GraphicsDriverUpdater(Console console)
		{
			s_Console = console;

			this.Icon = Utils.GetApplicationIcon();

			this.Text = String.Format("Graphics Driver Updater");
			this.ClientSize = new Size(300, 120);

			this.statusControl = new Label();
			this.statusControl.AutoSize = true;
			this.statusControl.Location = new Point(20, 15);
			this.statusControl.Text = "Updating Graphics Driver";

			this.progressControl = new ProgressBar();
			this.progressControl.Width = 260;
			this.progressControl.Location = new Point(20, 45);
			this.progressControl.MarqueeAnimationSpeed = 25;

			this.exitControl = new Button();
			this.exitControl.Text = "Cancel";
			this.exitControl.Location = new Point(
				280 - this.exitControl.Width, 85);

			this.exitControl.Click += delegate (Object obj,
				EventArgs evt)
			{
				Environment.Exit(0);
			};

			this.Controls.Add(this.statusControl);
			this.Controls.Add(this.progressControl);
			this.Controls.Add(this.exitControl);
		}

		private void UpdateStatus(string status)
		{
			this.statusControl.Text = status;
		}

		private void SetProgressBarStyle(ProgressBarStyle style)
		{
			this.progressControl.Style = style;
		}

		private void UpdateDownloadProgress(int progress)
		{
			this.progressControl.Value = progress;
		}

		public void Update(string downloadUrl)
		{
			string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string setupDir = Path.Combine(programData, "BlueStacksSetup");
			string fileName = System.IO.Path.GetFileName(new Uri(downloadUrl).LocalPath);
			string filePath = Path.Combine(setupDir, fileName);

			Thread dl = new Thread(delegate ()
			{
				UIHelper.RunOnUIThread(this, delegate ()
				{
					UpdateStatus("Downloading graphics driver");
					SetProgressBarStyle(ProgressBarStyle.Continuous);
				});

				bool downloaded = false;
				int retries = 5;
				while (retries-- > 0 && !downloaded)
				{
					mDownloader = new Downloader(3, downloadUrl, filePath);
					mDownloader.Download(
							delegate (int percent)  // Download progress
							{
								UIHelper.RunOnUIThread(this, delegate ()
								{
									UpdateDownloadProgress(percent);
								});
							},
							delegate (string file)  // Download completed
							{
								try
								{
									InstallDriver(file);
									downloaded = true;
								}
								catch (Exception ex)
								{
									Logger.Error("Exception in CompleteGraphicsDriverSetup: " + ex.ToString());
									Thread.Sleep(10000);
									downloaded = false;
								}
							},
							delegate (Exception ex) // Download error
							{
								downloaded = false;
								// TODO: resume if it was a case of timeout
								Logger.Error("DownloadGraphicsDriver error: " + ex.ToString());
								Thread.Sleep(10000);
							});
				}
			});

			dl.IsBackground = true;
			dl.Start();
		}

		private void InstallDriver(string filePath)
		{
			string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string setupDir = Path.Combine(programData, "BlueStacksSetup");
			string driverDir = Path.Combine(setupDir, "GraphicsDriver");

			// Unzip downloaded runtime to ProgramData
			int exitCode = Utils.Unzip(filePath, driverDir);
			Logger.Info("Unzip runtime exited with error code: " + exitCode);
			if (exitCode != 0)
			{
				Logger.Error("Failed to unzip graphics driver. Aborting");
				try
				{
					Logger.Info("Deleting corrupted downloaded file...");
					File.Delete(filePath);
				}
				catch
				{
				}
				return;
			}

			string setupPath = Path.Combine(driverDir, "Setup.exe");

			Logger.Info("Installing graphics driver: {0}", setupPath);
			UIHelper.RunOnUIThread(this, delegate ()
			{
				UpdateStatus("Installing graphics driver");
				SetProgressBarStyle(ProgressBarStyle.Marquee);
			});

			Thread driverInstaller = new Thread(delegate ()
			{
				string args = "-over4id -nowinsat -s";
				Logger.Info("Launching {0} with args {1}", setupPath, args);
				Process p = Process.Start(setupPath, args);
				p.WaitForExit();
				Logger.Info("Installation completed. ExitCode: {0}", p.ExitCode);
				this.Close();

				DialogResult res = MessageBox.Show(Locale.Strings.GraphicsDriverUpdatedMessage,
					"Graphics Driver Updater",
					MessageBoxButtons.YesNo,
					MessageBoxIcon.Warning);

				Logger.Info("Retrying IsGraphicsDriverUptodate check");
				string updateUrl, msgType;
				bool isDriverUptodate = Utils.IsGraphicsDriverUptodate(out updateUrl, out msgType, null);
				Logger.Info("isDriverUptodate: " + isDriverUptodate);

				if (res == DialogResult.Yes)
				{
					System.Diagnostics.Process.Start("shutdown.exe", "-r -t 0");
				}
				else
				{
					RegistryKey prodKey = Registry.LocalMachine.CreateSubKey(Common.Strings.RegBasePath);
					string installDir = (string)prodKey.GetValue("InstallDir");
					string progName = Path.Combine(installDir, "HD-Restart.exe");
					Process proc = new Process();
					proc.StartInfo.FileName = progName;
					proc.StartInfo.Arguments = "Android";
					proc.Start();

					Environment.Exit(0);
				}
			});
			driverInstaller.IsBackground = true;
			driverInstaller.Start();
		}
	}
}
