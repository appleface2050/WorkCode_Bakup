using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;
using BlueStacks.hyperDroid.Device;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Cloud.Services;

namespace BlueStacks.hyperDroid.Tool
{
	class ProgressForm : Form
	{
		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int which);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

		public enum DeviceCap
		{
			LOGPIXELSX = 88,
			LOGPIXELSY = 90,

			VERTRES = 10,
			DESKTOPVERTRES = 117
		}

		public static int sDpi = 96;
		public static float sScaleMultiplier = 1.0F;

		private BackgroundWorker		mBackgroundWorker;
		private ProgressBar				mProgressBar;
		private Label					mLblProgress;
		private Button					mCancelButton;
		public static int percentDone = 0;
		public static ProgressForm Instance = null;
		public static string sType = null;

		public enum States
		{
			QuittingBlueStacks = 0,
			BackingupAndroidData = 25,
			BackingupUserData = 50,
			BackingupGameManagerData = 75,
			RestoringAndroidData = 25,
			RestoringUserData = 50,
			RestoringGameManagerData = 75,
		};

		public static int RearrangePixel(int pixels)
		{
			return (int)(sScaleMultiplier * pixels);
		}

		public ProgressForm(string type)
		{
			sType = type;
			InitializeComponent();
			if (type == "backup")
				this.mLblProgress.Text = "Creating Backup...";
			else
				this.mLblProgress.Text = "Restoring Data...";
			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.DoWork += new DoWorkEventHandler(UpdateProgress);

			this.mBackgroundWorker.WorkerReportsProgress = true;
			mBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(UpdateProgressBar);
			mBackgroundWorker.RunWorkerAsync();
		}

		public void UpdateProgress(object sender, DoWorkEventArgs e)
		{
			while (true)
			{
				mBackgroundWorker.ReportProgress(percentDone);
				if (percentDone == 100)
				{
					CloseAndRestart();
					break;
				}
				Thread.Sleep(200);
			}
		}

		private void UpdateProgressBar(object sender, ProgressChangedEventArgs e)
		{
			// Change the value of the ProgressBar to the BackgroundWorker progress.
			mProgressBar.Value = e.ProgressPercentage;
			// Set the text.

			switch(percentDone)
			{
				case 0 : 
					this.mLblProgress.Text = Locale.Strings.GetLocalizedString("QuittingText");
					break;
				case 25 :
					if (sType == "backup")
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("AndroidBackupText");
					else
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("AndroidRestoreText");
					break;
				case 50 :
					if (sType == "backup")
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("UserDataBackupText");
					else
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("UserDataRestoreText");
					break;
				case 75 :
					if (sType == "backup")
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("GameManagerBackupText");
					else
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("GameManagerRestoreText");
					break;
				case 100 :
					if (sType == "backup")
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("BackupCompleteText");
					else
						this.mLblProgress.Text = Locale.Strings.GetLocalizedString("RestoreCompleteText");
					break;
			}
		}

		public static void CloseAndRestart()
		{
			Common.UIHelper.RunOnUIThread(ProgressForm.Instance, delegate() {
					MessageBox.Show(Locale.Strings.GetLocalizedString("RestartText"));
					ProgressForm.Instance.Hide();
					RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
					string installDir = (string)regKey.GetValue("InstallDir");
					regKey.Flush();
					regKey.Close();

					Process proc = new Process();
					proc.StartInfo.FileName = Path.Combine(installDir, "BlueStacks.exe");
					proc.StartInfo.UseShellExecute = false;
					proc.Start();

					Application.Exit();
					});
		}

		private void HandleFormClosingEvent(object sender, FormClosingEventArgs e)
		{
			try
			{
			}
			catch (Exception ex)
			{
				Logger.Error("Err : " + ex.ToString());
			}
		}

		private void InitializeComponent()
		{
			Graphics g = Graphics.FromHwnd(IntPtr.Zero);
			IntPtr desktop = g.GetHdc();
			sDpi = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSX);

			int logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
			int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
			float screenScalingFactor = (float)logicalScreenHeight / (float)physicalScreenHeight;
			sDpi = (int)(sDpi * screenScalingFactor);
			Logger.Info("sDpi: {0}", sDpi);
			sScaleMultiplier = sDpi / (96 * 1.0F);
			
			int width = RearrangePixel(600);
			int height = RearrangePixel(200);

			this.SuspendLayout();
			// 
			// mLblProgress
			// 
			this.mLblProgress = new System.Windows.Forms.Label();
			this.mLblProgress.AutoSize = true;
			this.mLblProgress.Location = new System.Drawing.Point(RearrangePixel(20), RearrangePixel(30));
			this.mLblProgress.Name = "mLblProgress";
			this.mLblProgress.Size = new System.Drawing.Size(width - RearrangePixel(40), RearrangePixel(45));
			this.mLblProgress.TabIndex = 1;
			this.mLblProgress.Text = Locale.Strings.GetLocalizedString("DataManagerText");
			this.mLblProgress.TextAlign = ContentAlignment.TopLeft;
			this.mLblProgress.Font = new Font(this.mLblProgress.Font.Name, RearrangePixel(27), FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));	
			// 
			// mProgressBar
			//
			this.mProgressBar = new System.Windows.Forms.ProgressBar();
			this.mProgressBar.Location = new System.Drawing.Point(RearrangePixel(20), this.mLblProgress.Bottom + RearrangePixel(25));
			this.mProgressBar.Size = new System.Drawing.Size(width - RearrangePixel(40), RearrangePixel(55));
			this.mProgressBar.Name = "mProgressBar";
			//
			// mCancelButton
			// 
			//this.mCancelButton = new System.Windows.Forms.Button();
			//this.mCancelButton.Location = new System.Drawing.Point(RearrangePixel(340), this.mProgressBar.Bottom + RearrangePixel(20));
			//this.mCancelButton.Size = new System.Drawing.Size(RearrangePixel(100), RearrangePixel(30));
			//this.mCancelButton.Name = "CancelButton";
			//this.mCancelButton.Text = sLocalizedString["CancelButtonText"];
			//this.mCancelButton.TabIndex = 2;
			//this.mCancelButton.Click += new System.EventHandler(this.HandleCancelEvent);
			// Form1
			// 
			this.ClientSize = new System.Drawing.Size(width, height);
			this.ControlBox = true;
			this.Controls.Add(this.mLblProgress);
			this.Controls.Add(this.mProgressBar);
			//this.Controls.Add(this.mCancelButton);
			this.Name = "BlueStacksDataManager";
			this.Text = Locale.Strings.GetLocalizedString("DataManagerText");
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = true;
			this.ResumeLayout(false);
			this.Icon = System.Drawing.Icon.ExtractAssociatedIcon("BlueStacks.ico");
			this.PerformLayout();
			//this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.HandleFormClosingEvent);
			this.StartPosition = FormStartPosition.CenterScreen;
		}

	}
}
