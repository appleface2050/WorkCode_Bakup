using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Tool
{

public class newRunApp {

	private static string	s_AppName		= "";
	private static string	s_AppIcon		= "";
	private static string	s_AppPackage		= "";
	private static string	s_AppActivity		= "";
	private static string	s_ApkUrl		= "";
	private static string	sVName 			= "";
	private static string	s_appsDotJsonFile	= Path.Combine(Common.Strings.GadgetDir, "apps.json");

	private static int s_AgentPort;
	private static string s_RunAppPath = "runapp";
	private static Dictionary<string, string> data = new Dictionary<string, string>();


	public class Opt : GetOpt
	{
	    public String p="";//packageName
	    public String a="";//activityName
	    public bool nl=false;//nolookup
	    public String url="";//apkUrl
	    public String name="";//appName
	    public bool h=false;//hidemode
	    public bool t=false;//launch from win8 tile
	    public String v="";//vName
	}

	private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
	{
		return true;
	}

	public static int Main(String[] args)
	{
		Opt opt = new Opt();
		opt.Parse(args);
	
		sVName = opt.v;
		if (!sVName.Contains("Android"))
			sVName = "Android";	
		Common.Strings.VMName = sVName;
		Logger.InitUserLog();
		Logger.Info("VM Name : {0}", sVName); 
		InitExceptionHandlers();
		Utils.LogParentProcessDetails();

		Logger.Info("RunApp: Starting RunApp PID {0}", Process.GetCurrentProcess().Id);
		Logger.Info("IsAdministrator: {0}", User.IsAdministrator());

		for (int i = 0; i < args.Length; i++)
		{
			Logger.Info("CMD: arg{0}: {1}", i, args[i]);
		}

		Logger.Debug("nolookup = " + opt.nl);
		Logger.Debug("pkg name = " + opt.p);
		Logger.Debug("app name = " + opt.name);
		Logger.Debug("app url = " + opt.url);
		Logger.Debug("activity = " + opt.a);
		Logger.Debug("hidemode = " + opt.h);
		Logger.Debug("fromtile = " + opt.t);
		Logger.Debug("vName = " + opt.v);

		string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
		Logger.Info("the exe path is " + exePath);
		string folderPath = Directory.GetParent(exePath).ToString();

		if(Features.IsFeatureEnabled(Features.MULTI_INSTANCE_SUPPORT))
		{
			Logger.Info("calling HD-QuitMultiInstance to kill other instances services and processes if running");
			Utils.QuitMultiInstance(folderPath);
		}

		SilentUpdaterHandling(args);
		ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

		if (Utils.IsBlueStacksInstalled() == false)
		{
			MessageBox.Show(Common.Strings.BlueStacksNotFound,
					"BlueStacks runtime could not be detected.",
					MessageBoxButtons.OK,
					MessageBoxIcon.Error);
			// return zero even in case of errors to avoid "This program might not have installed correctly" dialog box.
			Environment.Exit(0);
		}
		
		//setting up the required variables
		s_AppName = opt.name;
		s_AppPackage = opt.p;
		s_AppActivity = opt.a;
		s_ApkUrl = opt.url;
		
		if (BlueStacks.hyperDroid.Common.Oem.Instance.IsGameManagerToBeStartedOnRunApp)
		{
			if (s_AppPackage != "" && s_AppActivity != "")
			{
				StartGameManager(s_AppPackage, s_AppActivity);
				Environment.Exit(0);
			}
			if (args.Length >= 1 && args[0].StartsWith("bluestacks:"))
			{
				if (args.Length >= 3)
				{
					s_AppPackage 	= args[1];
					s_AppActivity 	= args[2];
					StartGameManager(s_AppPackage, s_AppActivity);
					Environment.Exit(0);
				}
			}
			else if (!opt.h)
			{
				StartGameManager(s_AppPackage, s_AppActivity);
				Environment.Exit(0);
			}
		}

		try
		{
			//setting up the required variables
			s_AppName = opt.name;
			s_AppPackage = opt.p;
			s_AppActivity = opt.a;
			s_ApkUrl = opt.url;

			//support for backward compatibility (older types of arguments)
			if(args.Length >0 && args[0].Equals("Android"))
			{
				if(args.Length <= 2 || (args.Length == 3 && args[2].Equals("nolookup")))
				{
					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMessageBoxToBeDisplayed)
					{
						MessageBox.Show("Invalid Call",
								"BlueStacks Error",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
					}
					Logger.Info("RunApp arguments: ");
					foreach (string arg in args)
						Logger.Info("arg: " + arg);
					Logger.Info("backward compatibility arguments not in correct form. Exiting RunApp");
					Environment.Exit(1);

				}

				s_AppPackage 	= args[1];
				s_AppActivity 	= args[2];
				if(args.Length >= 4)
				opt.nl		= args[3].Equals("nolookup") ? true : false;
			}

			//nolookup check
			if (!opt.nl && !String.IsNullOrEmpty(s_AppActivity) && !String.IsNullOrEmpty(s_AppPackage))	//do lookup
			{
				bool valid = JsonParser.GetAppData(s_AppPackage, s_AppActivity, out s_AppName, out s_AppIcon);
				if(!valid && !opt.nl)
				{
					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMessageBoxToBeDisplayed)
					{
						MessageBox.Show("This app is not installed. Please install the app and try again.",
								"BlueStacks Error",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
					}
					Logger.Info("RunApp arguments: ");
					foreach (string arg in args)
						Logger.Info("arg: " + arg);
					Logger.Info("App not found. Exiting RunApp");
					Environment.Exit(1);
				}
			}

			LaunchFrontend(opt.h);
			
			Logger.Info("appname: " + s_AppName);
			
			if(!String.IsNullOrEmpty(s_AppPackage) && !String.IsNullOrEmpty(s_AppActivity))
			{
				data.Clear();
				data.Add("package", s_AppPackage);
				data.Add("activity", s_AppActivity);

				Logger.Info("package: " + s_AppPackage);
				Logger.Info("activity: " + s_AppActivity);


				
				if(s_ApkUrl != "")
					data.Add("apkUrl", s_ApkUrl);

				RegistryKey configKey		= Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
				s_AgentPort			= (int)configKey.GetValue("AgentServerPort", 2861);

				String url = String.Format("http://127.0.0.1:{0}/{1}", s_AgentPort, s_RunAppPath);
				Logger.Info("RunApp: Sending post request to {0}", url);

				string res = Common.HTTP.Client.PostWithRetries(url, data, null, false, 10, 500, sVName);

				IJSonReader json = new JSonReader();
				IJSonObject obj = json.ReadAsJSonObject(res);
				if (obj["success"].BooleanValue)
					return 0;
				else
					return 1;
			}
		}
		catch (Exception exc)
		{
			Logger.Error("Got Exception");
			Logger.Error(exc.ToString());
		}
		Environment.Exit(1);
		return 1;
	}

	private static void StartGameManager(string package, string activity)
	{
		Logger.Info("RunApp: Starting Game Manager");
		String gameManagerFile = Common.Utils.GetPartnerExecutablePath();
		String args = "";
		if (package != "")
			args = String.Format("-p {0} -a {1}", package, activity);
		Logger.Info("Launching {0} with args {1}", gameManagerFile, args);
		Process.Start(gameManagerFile, args);
	}

	private static void LaunchFrontend(bool hidemode)
	{
		Logger.Info("In LaunchFrontend");

		String regPath = Common.Strings.RegBasePath;
		String installDir;
		RegistryKey key;

		using (key = Registry.LocalMachine.OpenSubKey(regPath)) {
			installDir = (String)key.GetValue("InstallDir");
		}

		/*
		 * Set ServiceStoppedGracefully
		 * as this code path will be reached only on user interaction
		 */
		String cfgPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
		using (key = Registry.LocalMachine.OpenSubKey(
					cfgPath, true)) {
			key.SetValue("ServiceStoppedGracefully", 1,
					RegistryValueKind.DWord);
			key.Flush();
		}

		string frontendExe = "HD-Frontend";
		String prog = installDir + "\\" + frontendExe + ".exe";

		Process proc;
		if (hidemode)
		{
			Logger.Info("Starting hidden frontend");
			proc = Process.Start(prog, String.Format("{0} -h", sVName));
		}
		else
		{
			Logger.Info("Starting visible frontend");
			string args = sVName;
			if (s_AppName != "" && s_AppIcon == "")
			{
				s_AppIcon = s_AppPackage + "." + s_AppActivity + ".png";
				args = String.Format("{0} -n \"{1}\" -i \"{2}\"", sVName, s_AppName, s_AppIcon);
			}


			if (s_AppPackage != "" && s_ApkUrl != "")
			{
				args = String.Format("{0} -pkg \"{1}\" -url \"{2}\"", args, s_AppPackage, s_ApkUrl);
				if (!IsAppInstalled(s_AppPackage))
				{
					string serviceName = Common.Strings.GetAndroidServiceName(sVName);
					ServiceController sc = new ServiceController(serviceName);
					if (sc.Status == ServiceControllerStatus.Running)
						Utils.KillProcessByName(frontendExe);
				}
			}
			else if (s_AppPackage != "")
			{
				args = String.Format("{0} -pkg \"{1}\"", args, s_AppPackage);
			}

			Logger.Info("String Frontend with args : {0}", args);
			proc = Process.Start(prog, args);
		}

		/*
		 * Wait for the frontend to signal that it is ready.
		 */

		String evtName = String.Format(
		    "Global\\BlueStacks_Frontend_Ready_{0}", sVName);
		EventWaitHandle evt;

		Logger.Info("Trying to open event {0}", evtName);

		while (true) {

			try {
				evt = EventWaitHandle.OpenExisting(evtName);
			} catch (WaitHandleCannotBeOpenedException exc) {
				string suppressCompilerWarning = exc.Message;
				Thread.Sleep(100);
				continue;
			} catch (UnauthorizedAccessException exc) {
				Logger.Error(string.Format(
							"Error Occured, Err : {0}", exc.ToString()));
				return;
			}

			break;
		}

		Logger.Info("Waiting on event {0}", evtName);
		evt.WaitOne();
	}




	private static void SilentUpdaterHandling(string[] args)
	{
		try
		{
			Logger.Info("Checking if Silent Updater running(Installing)");
			RegistryKey silentKey = Registry.LocalMachine.OpenSubKey(
					Common.Strings.HKLMManifestRegKeyPath);
			if(silentKey != null && silentKey.GetValue("Status") != null &&
					(string.Compare((string)silentKey.GetValue("Status"), "Installing") == 0 ||
					 string.Compare((string)silentKey.GetValue("Status"), "RollBack") == 0))
			{
				string currentFileName = (string)Process.GetCurrentProcess().MainModule.FileName;
				string installDir = (string)Registry.LocalMachine.OpenSubKey(
							Common.Strings.RegBasePath).GetValue("InstallDir");
				string installDirFileName = Path.Combine(installDir, "HD-RunApp.exe");
				if(String.Compare(currentFileName, installDirFileName, true) == 0)
				{
					string tempDir = Environment.GetEnvironmentVariable("TEMP");

					string tempRunAppExe = Path.Combine(tempDir, "HD-RunAppTemp.exe");
					if (File.Exists(tempRunAppExe))
						File.Delete(tempRunAppExe);
					File.Copy(Path.Combine(installDir, "HD-RunApp.exe"), tempRunAppExe);

					string tempRunAppExeConfig = Path.Combine(tempDir, "HD-RunAppTemp.exe.config");
					if (File.Exists(tempRunAppExeConfig))
						File.Delete(tempRunAppExeConfig);
					File.Copy(Path.Combine(installDir, "HD-RunApp.exe.config"), tempRunAppExeConfig);

                    string tempHdLoggerNative = Path.Combine(tempDir, "HD-Logger-Native.dll");
                    if (File.Exists(tempHdLoggerNative))
                        File.Delete(tempHdLoggerNative);

                    File.Copy(Path.Combine(installDir, "HD-Logger-Native.dll"), tempHdLoggerNative);
					using (Process proc = new Process())
					{
						proc.StartInfo.FileName  = Path.Combine(tempDir, "HD-RunAppTemp.exe");
						proc.StartInfo.Arguments = String.Join(" ", args);

						Logger.Info("Calling {0} {1}", proc.StartInfo.FileName, proc.StartInfo.Arguments);

						proc.StartInfo.UseShellExecute = false;
						proc.StartInfo.CreateNoWindow = true;

						proc.Start();
					}

					Environment.Exit(0);
				}
				else
				{
					Common.UI.ProgressBar silentUpdaterBox = new Common.UI.ProgressBar("Update in progress. Please wait");
					Thread waitForUpdate = new Thread(delegate() {
							try
							{
							Logger.Info("Checking for silent updater running");
							Application.EnableVisualStyles();
							Application.Run(silentUpdaterBox);
							}
							catch(Exception e)
							{
							Logger.Info("Error while showing progress UI. Err: {0}", e.ToString());
							}
							});
					Logger.Info("Showing progress UI Dialog");
					waitForUpdate.Start();
					while(silentKey != null && silentKey.GetValue("Status") != null &&
							(string.Compare((string)silentKey.GetValue("Status"), "Installing") == 0 ||
							 string.Compare((string)silentKey.GetValue("Status"), "RollBack") == 0))
					{
						Thread.Sleep(2000);
						Logger.Info("Silent Updater is still running. Waiting again.");
					}
					silentUpdaterBox.Hide();

					using (Process proc = new Process())
					{
						proc.StartInfo.FileName  = Path.Combine(installDir, "HD-RunApp.exe");
						proc.StartInfo.Arguments = String.Join(" ", args);

						Logger.Info("Calling {0} {1}", proc.StartInfo.FileName, proc.StartInfo.Arguments);

						proc.StartInfo.UseShellExecute = false;
						proc.StartInfo.CreateNoWindow = true;

						proc.Start();
					}

					Environment.Exit(0);
				}
			}

			Logger.Info("Silent Updater not installing");
		}
		catch (Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			Environment.Exit(1);
		}

	}


	private static bool IsAppInstalled(string appPackage)
	{
		bool installed;

		try
		{
			string url = String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort,
					Common.VmCmdHandler.s_PingPath);
			Common.HTTP.Client.Get(url, null, false, 3000);
			Logger.Info("Guest booted. Will send request.");

			url = String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort,
					Common.Strings.IsPackageInstalledUrl);
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("package", appPackage);
			string r = Common.HTTP.Client.Post(url, data, null, false);
			JSonReader readjson = new JSonReader();
			IJSonObject resJson = readjson.ReadAsJSonObject(r);
			string result = resJson["result"].StringValue.Trim();
			if (result == "ok")
			{
				Logger.Info("App installed");
				installed = true;
			}
			else
			{
				Logger.Info("App not installed");
				installed = false;
			}
		}
		catch (Exception)
		{
			Logger.Info("Guest not booted. Will read from apps.json");

			string version;
			if (JsonParser.IsAppInstalled(appPackage, out version))
			{
				Logger.Info("Found in apps.json");
				installed = true;
			}
			else
			{
				Logger.Info("Not found in apps.json");
				installed = false;
			}
		}

		return installed;
	}

	private static void InitExceptionHandlers()
	{
		Application.ThreadException += delegate(Object obj,
				System.Threading.ThreadExceptionEventArgs evt)
		{
			Logger.Error("RunApp: Unhandled Exception:");
			Logger.Error(evt.Exception.ToString());
			Environment.Exit(1);
		};

		Application.SetUnhandledExceptionMode(
				UnhandledExceptionMode.CatchException);

		AppDomain.CurrentDomain.UnhandledException += delegate(
				Object obj, UnhandledExceptionEventArgs evt)
		{
			Logger.Error("RunApp: Unhandled Exception:");
			Logger.Error(evt.ExceptionObject.ToString());
			Environment.Exit(1);
		};
	}



}
}
