using System;
using System.IO;
using System.Diagnostics;
using System.Windows.Forms;
using System.ServiceProcess;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Launcher
{
	public class StartLauncher
	{
		public static void Main(String[] args)
		{
			bool result;
			string resultText;
			
			string vmName = "Android";
			if (args.Length > 0)
				vmName = args[0];

			Logger.InitUserLog();
			InitExceptionHandlers();
			Utils.LogParentProcessDetails();
			Common.Strings.VMName = vmName;
			Logger.Info("Starting HD-StartLauncher PID {0}", Process.GetCurrentProcess().Id);
			Logger.Info("IsAdministrator: {0}", User.IsAdministrator());

			Logger.Info("no. of arguments = " + args.Length);
			for (int i = 0; i < args.Length; i++)
			{
				Logger.Debug("arg{0}: {1}", i, args[i]);
			}

			// Start agent
			RegistryKey		HKLMregistry	= Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String			installDir	= (string)HKLMregistry.GetValue("InstallDir");

			Logger.Info("Starting HD-Agent");
			string agentFile = Path.Combine(installDir, @"HD-Agent.exe");
			try {
			    Process.Start(agentFile);
			} catch (Exception exc) 
			{
			}

			string runAppFile = Path.Combine(installDir, @"HD-RunApp.exe");
			Logger.Info("Starting HD-RunApp");
			if (args.Length >= 2)
			{
				string appsPackage = args[1];
				string appsActivity = args[2];
				string appsUrl = "";
					
				if (args.Length == 4)
				{
					appsUrl = args[3];
				}

				if(appsUrl != "")
					Process.Start(runAppFile, String.Format("-p {0} -a {1} -url {2} -v {3} -nl", appsPackage, appsActivity, appsUrl, vmName));
				else
					Process.Start(runAppFile, String.Format("-p {0} -a {1} -v {3} -nl", appsPackage, appsActivity, vmName));
			}
			else
			{
				Process.Start(runAppFile, String.Format("-v {0}", vmName));
			}

			Logger.Info("Exiting HD-StartLauncher PID {0}", Process.GetCurrentProcess().Id);
		}

		private static void InitExceptionHandlers()
		{
			Application.ThreadException += delegate(Object obj,
					System.Threading.ThreadExceptionEventArgs evt)
			{
				Logger.Error("StartLauncher: Unhandled Exception:");
				Logger.Error(evt.Exception.ToString());
				Environment.Exit(1);
			};

			Application.SetUnhandledExceptionMode(
					UnhandledExceptionMode.CatchException);

			AppDomain.CurrentDomain.UnhandledException += delegate(
					Object obj, UnhandledExceptionEventArgs evt)
			{
				Logger.Error("StartLauncher: Unhandled Exception:");
				Logger.Error(evt.ExceptionObject.ToString());
				Environment.Exit(1);
			};
		}
	}
}

