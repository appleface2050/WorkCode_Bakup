using System;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.Diagnostics;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Tool
{
	public class DataManager : Form
	{
		public static Mutex sDataManagerLock;
		public static string type = null;

		[STAThread]
		public static void Main(String[] args)
		{
			if (Common.Utils.IsAlreadyRunning(Strings.DataManagerLock, out sDataManagerLock))
			{
				Logger.Info(Process.GetCurrentProcess().ProcessName + " already running");
				Environment.Exit(0);
			}

			InitExceptionHandlers();
			
			if (args.Length > 0)
			{
				if (string.Compare(args[0], "backup", StringComparison.OrdinalIgnoreCase) == 0)
					type = "backup";
				else if (string.Compare(args[0], "restore", StringComparison.OrdinalIgnoreCase) == 0)
					type = "restore";
				else
				{
					Logger.Info("Invalid request....type = " + args[0]);
					Environment.Exit(0);
				}
			}
			else
			{
				Logger.Info("Invalid request....type = " + args[0]);
				Environment.Exit(0);
			}

			type = type.ToLower();

			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);

			BackgroundWorker bgwThread = new BackgroundWorker();
			bgwThread.DoWork += new DoWorkEventHandler(CreateOrRestoreData);
			bgwThread.RunWorkerAsync();

			ProgressForm.Instance = new ProgressForm(type);

			Application.Run(ProgressForm.Instance);
		}

		private static void InitExceptionHandlers()
		{
			Logger.InitUserLog();

			Logger.Info("CLR version {0}", Environment.Version);
			Logger.Info("IsAdministrator: {0}", User.IsAdministrator());
			Logger.Info("BlueStacks Patch : Starting process at PID {0}", Process.GetCurrentProcess().Id);

			Application.ThreadException += delegate(Object obj,
					System.Threading.ThreadExceptionEventArgs evt)
			{
				Logger.Error("BlueStacks Downloader : Unhandled Exception");
				Logger.Error(evt.Exception.ToString());
				Environment.Exit(1);
			};

			Application.SetUnhandledExceptionMode(
					UnhandledExceptionMode.CatchException);

			AppDomain.CurrentDomain.UnhandledException += delegate(
					Object obj, UnhandledExceptionEventArgs evt)
			{
				Logger.Error("BlueStacks Downloader : Unhandled Exception");
				Logger.Error(evt.ExceptionObject.ToString());
				Environment.Exit(1);
			};
		}

		private static void CreateOrRestoreData(object sender, DoWorkEventArgs e)
		{
			Thread.Sleep(1000);
			DataManagerUtils.QuitBlueStacks();

			string dirPath = null;
			RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string dataDir = (string)regKey.GetValue("DataDir");
			if (type == "backup")
			{
				Common.UIHelper.RunOnUIThread(ProgressForm.Instance, delegate(){
						FolderBrowserDialog fbd = new FolderBrowserDialog();
						fbd.Description = Locale.Strings.GetLocalizedString("SelectBackupText");
						DialogResult result = fbd.ShowDialog();
						if (result == DialogResult.Cancel)
							Environment.Exit(0);
						dirPath = fbd.SelectedPath;
				});

				if (!string.IsNullOrEmpty(dirPath))
				{
					DataManagerUtils.BackupData(dataDir, dirPath);
				}
			}
			else
			{
				Common.UIHelper.RunOnUIThread(ProgressForm.Instance, delegate(){
						FolderBrowserDialog fbd = new FolderBrowserDialog();
						fbd.Description = Locale.Strings.GetLocalizedString("SelectRestoreText");
						DialogResult result = fbd.ShowDialog();
						if (result == DialogResult.Cancel)
							Environment.Exit(0);
						dirPath = fbd.SelectedPath;
				});

				if (!string.IsNullOrEmpty(dirPath))
				{
					DataManagerUtils.RestoreData(dirPath, dataDir);
				}
			}
		}

	}
}
