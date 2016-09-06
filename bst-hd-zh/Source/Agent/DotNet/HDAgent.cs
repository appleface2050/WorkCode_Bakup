using System;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net.Security;
using System.Windows.Forms;
using System.Management;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using CodeTitans.JSon;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common.Interop;

namespace BlueStacks.hyperDroid.Agent {
public class HDAgent : ApplicationContext {

	[DllImport("HD-ShortcutHandler.dll", CharSet=CharSet.Auto)]
	public static extern int CreateShortcut(
			string target,
			string shortcutName,
			string desc,
			string iconPath,
			string targetArgs,
			int initializeCom);


	[DllImport("HD-GpsLocator-Native.dll", CallingConvention=CallingConvention.StdCall)]
	private static extern void HdLoggerInit(Logger.HdLoggerCallback cb);

	[DllImport("HD-GpsLocator-Native.dll", CallingConvention=CallingConvention.StdCall)]
	public static extern int LaunchGpsLocator();

	public static bool isPartialUpdate = false;
	public const uint BST_DISABLE_S2P = 0x00100000;
	private static ManagementEventWatcher sManagementEventWatcher;
	private static Dictionary<string, string> sPowerValues = new Dictionary<string, string>();

	public static void InitPowerEvents()
	{
		try
		{
			WqlEventQuery q = new WqlEventQuery();
			ManagementScope scope = new ManagementScope("root\\CIMV2");

			q.EventClassName = "Win32_PowerManagementEvent";
			sManagementEventWatcher = new ManagementEventWatcher(scope, q);
			sManagementEventWatcher.EventArrived += PowerEventArrive;
			sManagementEventWatcher.Start();
		}
		catch (Exception ex)
		{
			Logger.Error("An error occured while capturing power events...Err : " + ex.ToString());
		}
	}
	private static void PowerEventArrive(object sender, EventArrivedEventArgs e)
	{
		try
		{
			foreach (PropertyData pd in e.NewEvent.Properties)
			{
				if (pd == null || pd.Value == null) continue;
				string name = sPowerValues.ContainsKey(pd.Value.ToString())
					? sPowerValues[pd.Value.ToString()]
					: pd.Value.ToString();
				Logger.Info("PowerEvent:" + name);

				if (String.Compare(name, "Resume from Suspend", true) == 0)
				{
					long utcMilliseconds = (DateTime.UtcNow.Ticks - 621355968000000000) / TimeSpan.TicksPerMillisecond;
					String cmd = String.Format("settime {0}", utcMilliseconds);
					Logger.Info("Number of ticks in milliseconds since epoch : " + utcMilliseconds.ToString());
					string resp = Common.VmCmdHandler.RunCommand(cmd);

					Logger.Info("Response from bstcmdprocessor for time correction after sleep/hibernate : " + resp);
				}
			}
		}
		catch (Exception ex)
		{
			Logger.Error("An error ocured while setting time....Err : " + ex.ToString());
		}
	}

	[STAThread]
	static void Main(string[] args)
	{
		Logger.InitLog(null, "Agent");
		InitExceptionHandlers();
		Utils.LogParentProcessDetails();

		sPowerValues.Add("4", "Entering Suspend");
		sPowerValues.Add("7", "Resume from Suspend");
		sPowerValues.Add("10", "Power Status Change");
		sPowerValues.Add("18", "Resume Automatic");
		InitPowerEvents();

		Logger.Info("HDAgent: Starting agent PID {0}", Process.GetCurrentProcess().Id);
		Logger.Info("HDAgent: CLR version {0}", Environment.Version);
		Logger.Info("HDAgent: IsAdministrator: {0}", User.IsAdministrator());

		using (RegistryKey HKLMregistry = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
		{
			s_InstallDir = (string)HKLMregistry.GetValue("InstallDir");
		}
		Directory.SetCurrentDirectory(s_InstallDir);
		Logger.Info("HDAgent: CurrentDirectory: {0}", Directory.GetCurrentDirectory());

		try
		{
			Logger.Info("Checking if Silent Updater running(Installing)");
			RegistryKey silentKey = Registry.LocalMachine.OpenSubKey(
					Common.Strings.HKLMManifestRegKeyPath);
			if(silentKey != null && silentKey.GetValue("Status") != null &&
					(string.Compare((string)silentKey.GetValue("Status"), "Installing")==0 ||
					 string.Compare((string)silentKey.GetValue("Status"), "RollBack")==0))
				return;

			Logger.Info("Silent Updater not installing");
		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			return;
		}

		Locale.Strings.InitLocalization(null);

		bool askedToWait = false;
		if ((args.Length == 1) && (String.Compare(args[0].Trim(), "wait")) == 0)
			askedToWait = true;

		if (askedToWait)
		{
			Logger.Info("Was asked to wait. Sleeping for 60 sec");
			Thread.Sleep(60000);
		}

		string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
		Logger.Info("the exe path is " + exePath);
		string folderPath = Directory.GetParent(exePath).ToString();

		if(Features.IsFeatureEnabled(Features.MULTI_INSTANCE_SUPPORT))
		{
			Logger.Info("calling HD-QuitMultiInstance to kill other instances services and processes if running");
			Utils.QuitMultiInstance(folderPath);
		}
		if (Common.Utils.IsAlreadyRunning(Common.Strings.HDAgentLockName, out s_HDAgentLock))
		{
			HandleAlreadyRunning();
		}

		ServicePointManager.DefaultConnectionLimit = 10;
		ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

		Guid guid;

		if (User.IsFirstTimeLaunch() && User.GUID == "")
		{
			try
			{
				guid = UUID.GenerateUUID(UUID.UUIDTYPE.GLOBAL);
			}
			catch (UUID.EUUIDLocalOnly)
			{
				guid = Guid.NewGuid();
			}
			catch (UUID.EUUID e)
			{
				Logger.Error(e.ToString());
				throw e;
			}

			User.GUID = guid.ToString();
		}

		Application.EnableVisualStyles();

		ApkInstall.InitApkInstall();

		Thread serverThread = new Thread(SetupHTTPServer);
		serverThread.IsBackground = true;
		serverThread.Start();

		Thread performanceCounterThread = new Thread(HDProcessPerformanceCounter);
		performanceCounterThread.IsBackground = true;
		performanceCounterThread.Start();

		try
		{
			//adding try catch here, this was causing hd-agent crash in netbar
			EventLog applicationEventLog = new EventLog("Application");
			applicationEventLog.EntryWritten += new EntryWrittenEventHandler(EventLogWritten);
			applicationEventLog.EnableRaisingEvents = true;

			EventLog systemEventLog = new EventLog("System");
			systemEventLog.EntryWritten += new EntryWrittenEventHandler(EventLogWritten);
			systemEventLog.EnableRaisingEvents = true;
		}
		catch(Exception ex)
		{
			Logger.Error("Got excecption while hooking to event log ex:{0}", ex.ToString());
		}

		/*
		 * Try to copy the library shortcut on Windows 7 and above
		 */
		int majorVersion = Environment.OSVersion.Version.Major;
		int minorVersion = Environment.OSVersion.Version.Minor;
		if (majorVersion == 6 && minorVersion >= 1)
		{
			try
			{
				CopyLibraryIfNeeded();
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}
		}

		Logger.Info("Starting Gps Locator");
		try
		{
			Thread gpsStarter = new Thread(StartGpsLocator);
			gpsStarter.IsBackground = true;
			gpsStarter.Start();
		}
		catch(Exception e)
		{
			Logger.Error("Error Occured, Err: {0}", e.ToString());
		}
		
		GetDefaultBrowserProgId();

		HDAgent agent = new HDAgent();
		Application.Run(agent);
		Logger.Info("Exiting HDAgent PID {0}", Process.GetCurrentProcess().Id);
	}

	private static void GetDefaultBrowserProgId()
	{
		Logger.Info("Reading default browser information");
		string progIdRegPath = @"Software\Microsoft\Windows\Shell\Associations\UrlAssociations\http\UserChoice";
		try
		{
			RegistryKey progIdRegKey = Registry.CurrentUser.OpenSubKey(progIdRegPath);
			if (progIdRegKey == null)
			{
				Logger.Info(progIdRegPath + " not found");
			}
			else
			{
				string progId = (string)progIdRegKey.GetValue("ProgId", "");
				if (progId != "")
				{
					Logger.Info("ProgId: " + progId);
					RegistryKey configKey = Registry.LocalMachine.CreateSubKey(
							Common.Strings.HKLMConfigRegKeyPath);
					configKey.SetValue("ProgId", progId);
					configKey.Close();
				}
			}
		}
		catch (Exception e)
		{
			Logger.Error(e.ToString());
		}

	}

	private static void StartGpsLocator()
	{
		Logger.Info("Inside Start GpsLocator");
		try
		{
			try
			{
				Logger.Info("Checking if Gps Enabled");
				int gpsMode = (int)Registry.LocalMachine.OpenSubKey(
						Common.Strings.HKLMAndroidConfigRegKeyPath).GetValue("GpsMode");
				if(gpsMode == 0)
				{
					Logger.Info("GpsMode is Disabled.");
					return;
				}

			}
			catch(Exception ex)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", ex.ToString()));
			}

			System.Version win8version = new System.Version(6, 2, 9200, 0);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
					Environment.OSVersion.Version >= win8version)
			{
				try
				{
					HdLoggerInit(Logger.GetHdLoggerCallback());
					LaunchGpsLocator();
					Logger.Info("Back from Native Call");
				}
				catch(Exception exp)
				{
					Logger.Error(string.Format("Error Occured, Err: {0}", exp.ToString()));
				}
			}
			else
			{
				Logger.Info("Need Windows 8 or Higher for GpsLocator to work.");
			}

		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
		}
	}

	private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
	{
		return true;
	}

	private static void HandleAlreadyRunning()
	{
		Logger.Info("Agent already running");

		int agentPort = Utils.GetAgentServerPort();
		string url = String.Format("http://127.0.0.1:{0}/{1}", agentPort, Common.Strings.SystrayVisibilityUrl);
		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add("visible", "true");
		try
		{
			Common.HTTP.Client.Post(url, data, null, false);
		}
		catch (Exception e)
		{
			Logger.Error("Exception when sending HTTP message to SystrayVisibilityUrl");
			Logger.Error(e.ToString());
		}

		Environment.Exit(1);
	}

	private static void CopyLibraryIfNeeded()
	{
		string library = Common.Strings.LibraryName;

		string appData = Environment.GetFolderPath(
				Environment.SpecialFolder.ApplicationData);
		string userLibraryPath = Path.Combine(appData, @"Microsoft\Windows\Libraries");

		string publicFolder = Environment.ExpandEnvironmentVariables("%Public%");
		string publicLibraryPath = Path.Combine(publicFolder, "Libraries");

		string libraryIdentifier = library + ".library-ms";
		string libraryShortcut = library + ".lnk";

		userLibraryPath = Path.Combine(userLibraryPath, libraryIdentifier);
		publicLibraryPath = Path.Combine(publicLibraryPath, libraryIdentifier);

		if (!File.Exists(userLibraryPath))
		{
			Logger.Info("Copying library from {0} to {1}", publicLibraryPath, userLibraryPath);
			File.Copy(publicLibraryPath, userLibraryPath, true);
		}
	}

	private static void EventLogWritten(Object source, EntryWrittenEventArgs e)
	{
		Logger.Debug("EventLog written");
		string message = e.Entry.Message;

		Regex regex = new Regex("(HD-.+).exe");
		Match match = regex.Match(message);

		if (!match.Success)
		{
			return;
		}

		string binName = match.Groups[1].Value;

		Logger.Info("Event log for {0} written", binName);
		Logger.Info("Message:\n{0}", message);

		try
		{
			if (sOemWindowMapper.ContainsKey(Oem.Instance.OEM) == true && binName.Equals("HD-Agent", StringComparison.CurrentCultureIgnoreCase) == false)
			{
				HTTPHandler.StartLogCollection("EXE_CRASHED", "binName");
				NotifyExeCrashToParentWindow(sOemWindowMapper[Oem.Instance.OEM][0],
						sOemWindowMapper[Oem.Instance.OEM][1]);
			}
		}
		catch(Exception exp)
		{
			Logger.Error(String.Format("Error Occured, Err: {0}", exp.ToString()));
		}

		string url = String.Format("{0}/{1}", Service.Host, Common.Strings.BinaryCrashStatsUrl);
		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add("binary", GetURLSafeBase64String(binName));
		data.Add("message", GetURLSafeBase64String(message));
		Common.HTTP.Client.Post(url, data, null, false);
	}

	private static void HDProcessPerformanceCounter()
	{
		try 
		{
			string frontendProcess = "HD-Frontend";
			string serviceProcess = "";
			if (Common.Strings.IsEngineLegacy())
			{
				serviceProcess = "HD-Service";
			}
			else
			{
				serviceProcess = "HD-Service-VBox";
			}	
			PerformanceCounter frontendProcessCpu = new PerformanceCounter("Process", "% Processor Time", frontendProcess, true);
			PerformanceCounter serviceProcessCpu = new PerformanceCounter("Process", "% Processor Time", serviceProcess, true);

			while(true)
			{
				Process[] pname = Process.GetProcessesByName(frontendProcess);
				if(pname.Length > 0)
				{
					double pct = frontendProcessCpu.NextValue();
					if(pct >= 50.0)
					{
						Logger.Info("****{0} Process usage is more than 50% of cpu****", frontendProcess);
					}
				}
				pname = Process.GetProcessesByName(serviceProcess);
				if(pname.Length > 0)
				{
					double pct = serviceProcessCpu.NextValue();
					if(pct >= 50.0)
					{
						Logger.Info("****{0} Process usage is more than 50% of cpu****", serviceProcess);
					}
				}
				Thread.Sleep(60000);
			}
		}
		catch(Exception ex)
		{
			Logger.Error("Got exception in performance counter thread : ex {0}", ex.ToString());
		}
	}

	private static void SetupHTTPServer()
	{
		Dictionary<String, Common.HTTP.Server.RequestHandler> routes =
			new Dictionary<String, Common.HTTP.Server.RequestHandler>();
		routes.Add("/checkforupdate", HTTPHandler.CheckForUpdate);
		routes.Add("/installed", HTTPHandler.ApkInstalled);
		routes.Add("/uninstalled", HTTPHandler.AppUninstalled);
		routes.Add("/getapplist", HTTPHandler.GetAppList);
		routes.Add("/install", HTTPHandler.ApkInstall);
		routes.Add("/uninstall", HTTPHandler.AppUninstall);
		routes.Add("/runapp", HTTPHandler.RunApp);
		routes.Add("/InstallAppByURL", HTTPHandler.InstallAppByURL);
		routes.Add("/ping", HTTPHandler.Ping);
		routes.Add("/AppCrashedInfo", HTTPHandler.AppCrashedInfo);
		routes.Add("/doaction", HTTPHandler.DoAction);
		routes.Add("/getuserdata", HTTPHandler.GetUserData);
		routes.Add("/shownotification", HTTPHandler.ShowNotification);
		routes.Add("/showfenotification", HTTPHandler.ShowFeNotification);
		routes.Add("/quitfrontend", HTTPHandler.QuitFrontend);
		routes.Add("/addapp", HTTPHandler.AddApp);
		routes.Add("/getappimage", HTTPHandler.GetAppImage);
		routes.Add("/showtraynotification", HTTPHandler.ShowSysTrayNotification);
		routes.Add("/switchtolauncher", HTTPHandler.SwitchToLauncher);
		routes.Add("/switchtowindows", HTTPHandler.SwitchToWindows);
		routes.Add("/restart", HTTPHandler.Restart);
		routes.Add("/logappclick", HTTPHandler.LogAndroidClickEvent);
		routes.Add("/logwebappchannelclick", HTTPHandler.LogWebAppChannelClickEvent);
		routes.Add("/logappsearch", HTTPHandler.LogAndroidSearchEvent);
		routes.Add("/notification", HTTPHandler.NotificationHandler);
		routes.Add("/clipboard", HTTPHandler.SetClipboardData);
		routes.Add("/isappinstalled", HTTPHandler.IsAppInstalled);
		routes.Add("/topActivityInfo", HTTPHandler.TopActivityInfo);
		routes.Add("/systrayvisibility", HTTPHandler.SystrayVisibility);
		routes.Add("/restartagent", HTTPHandler.RestartAgent);
		routes.Add("/showtileinterface", HTTPHandler.ShowTileInterface);
		routes.Add("/FrontendStatusUpdate", HTTPHandler.HandleFrontendStatusUpdate);
		routes.Add("/s2pEvents", HTTPHandler.HandleS2PEvents);
		routes.Add("/setNewLocation",HTTPHandler.SetNewLocation);
		routes.Add("/adevents", HTTPHandler.HandleAdEvents);
		routes.Add("/exitagent", HTTPHandler.ExitAgent);
		routes.Add("/StopApp", HTTPHandler.StopAppHandler);
		routes.Add("/releaseApkInstallThread", HTTPHandler.ReleasApkInstallThread);
		routes.Add("/clearappdata", HTTPHandler.ClearAppDataHandler);
		routes.Add("/restartgamemanager", HTTPHandler.RestartGameManager);
		routes.Add("/PostHttpUrl", HTTPHandler.PostHttpUrl);
		Common.HTTP.Server server;
		int port = 2861;

		for (; port < 2870; port++)
		{
			try
			{
				server = new Common.HTTP.Server(port, routes, s_RootDir);
				server.Start();
				Logger.Info("Server listening on port " + server.Port);
				Logger.Info("Serving static content from " + server.RootDir);
				s_AgentPort = server.Port;
			}
			catch(Exception e)
			{
				Logger.Error(String.Format("Error Occured, Err: {0}", e.ToString()));
				continue;
			}

			SetAgentPortInBootParams();
			/* write agent server port to the registry */
			/*
			 * Don't crash the agent if unable to write to registry
			 * We might not be able to do this on Agent's first launch
			 */
			try
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMConfigRegKeyPath);
				key.SetValue("AgentServerPort", server.Port, RegistryValueKind.DWord);
				key.Flush();
				key.Close();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception when trying to write AgentServerPort to the registry");
				Logger.Error(ex.ToString());
			}

			Thread fqdnThread = new Thread(SendFqdn);
			fqdnThread.IsBackground = true;
			fqdnThread.Start();

			server.Run();
			break;
		}

		if (port == 2870)
		{
			Logger.Error("No free port available");
			Environment.Exit(2);
		}
	}

	private static void SetAgentPortInBootParams()
	{
		try
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.AndroidKeyBasePath);
			if (key != null)
			{
				string bootPrms = (string)key.GetValue("BootParameters", "");
				string[] paramParts = bootPrms.Split(' ');
				string newBootParams = "";
				string fullAddr = string.Format("10.0.2.2:{0}", s_AgentPort);

				if (bootPrms.IndexOf(Common.Strings.AgentPortBootParam) == -1)
				{
					newBootParams = bootPrms + " " + Common.Strings.AgentPortBootParam + "=" + fullAddr;
				}
				else
				{
					foreach (string param in paramParts)
					{
						if (param.IndexOf(Common.Strings.AgentPortBootParam) != -1)
						{
							if (!String.IsNullOrEmpty(newBootParams))
							{
								newBootParams += " ";
							}
							newBootParams += Common.Strings.AgentPortBootParam + "=" + fullAddr;
						}
						else
						{
							if (!String.IsNullOrEmpty(newBootParams))
							{
								newBootParams += " ";
							}
							newBootParams += param;
						}
					}
				}
				key.SetValue("BootParameters", newBootParams);
				key.Close();
			}
		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
		}
	}

	private static void SendFqdn()
	{
		RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);

		while (true)
		{
			/*
			 *  Added check for installation complete here to prevent unnecessary logs
			 */
			String installType = (String)prodKey.GetValue("InstallType");
			if (String.Compare(installType, "complete", true) == 0 ||
					String.Compare(installType, "full", true) == 0)
			{
				if (Common.VmCmdHandler.FqdnSend(s_AgentPort, "Agent") != null)
					break;
			}
			Thread.Sleep(2000);
		}
	}

	public HDAgent()
	{
		SysTray.Init();
		Utils.AddMessagingSupport(out sOemWindowMapper);

		string proxy = "";
		bool isProxyEnabled = Utils.IsProxyEnabled(out proxy);
		Logger.Info(string.Format("Proxy server enabled = {0} {1}", isProxyEnabled, proxy));

		// Start windows Clipboard handler
		clipboardClient = new ClipboardMgr();
		clipboardClient.Show();

		// Check for any announcements
		CheckAnnouncement();

		if(!Utils.IsAndroidFeatureBitEnabled(BST_DISABLE_S2P))
		{
			Logger.Info("The S2P feature is enabled");
			LaunchS2PAppsIfRequired();
		}

		TimelineStatsSender.Init();
	}

	private static void LaunchS2PAppsIfRequired()
	{
		Thread launchAppsThread = new Thread(delegate() {
				Thread.Sleep(1 * 60 * 1000);	// wait 1 min to get the list from another thread
				while (true)
				{
					LaunchExistingApps();
					Thread.Sleep(10 * 60 * 1000);	// retry after 10 min
				}
				});
		launchAppsThread.IsBackground = true;
		launchAppsThread.Start();

		Thread getAppListThread = new Thread(delegate() {
				while (true)
				{
				try
				{
				bool result = GetS2PAppsList();
				if (result == false)
				{
				// Failed to get s2p apps list. Retry again
				Thread.Sleep(1 * 60 * 60 * 1000);
				continue;
				}
				}
				catch (Exception ex)
				{
				Logger.Info("Failed to get s2p apps list. err: " + ex.ToString());
				// some error occured. Ignore it and sleep for 1 hour before checking again.
				Thread.Sleep(1 * 60 * 60 * 1000);
				continue;
				}

				Thread.Sleep(12 * 60 * 60 * 1000);	// sleep for 12 hours and then check again
				}
		});

		getAppListThread.IsBackground = true;
		getAppListThread.Start();
	}

	private static void LaunchExistingApps()
	{
		Logger.Info("Trying to launch s2p apps");

		if (Utils.IsUIProcessAlive())
		{
			Logger.Info("Frontend is already running. Ignoring s2p launch");
			return;
		}

		RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
		string[] allApps = (string[])configKey.GetValue("S2PSilentLaunch", null);
		if (allApps == null || allApps.Length == 0)
		{
			Logger.Info("No apps to launch");
			return;
		}


		string[]	appData		= allApps[0].Split(new char[] {'#'});

		string		package		= appData[0];
		string		timeOfLaunch	= appData[1];
		string		duration	= appData[2];

		RegistryKey	HKLMregistry	= Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		string		installDir	= (string)HKLMregistry.GetValue("InstallDir");

		String prog = installDir + @"\HD-RunApp.exe";
		String args = "-h";
		Process.Start(prog, args);

		bool isBooted = Utils.WaitForBootComplete();
		if (isBooted == false)
		{
			Logger.Error("Guest not booted. Ignoring S2P launch");
			return;
		}

		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
		int port = (int)key.GetValue("FrontendServerPort");

		Logger.Info("sending mute request to frontend");
		string url = String.Format("http://127.0.0.1:{0}/mute", port);
		Common.HTTP.Client.Post(url, null, null, false);

		string request = String.Format("runex {0}/.Main", package);
		//string request = String.Format("runex {0}/.Main", "com.supercell.boombeach");

		bool result = SendRunCommand(request);
		if (result == false)
		{
			Logger.Error("Failed to send runex request for s2p apps. Aboring");
			Utils.KillProcessByName("HD-Frontend");
			return;
		}

		Logger.Info("S2P app launched silently: " + package);

		string[] newAppsList = new string[allApps.Length - 1];
		for (int i = 0; i < allApps.Length - 1; i++)
		{
			newAppsList[i] = allApps[i + 1];
		}
		configKey.SetValue("S2PSilentLaunch", newAppsList);

		Logger.Info("Sleeping for {0} seconds before killing frontend", duration);
		Thread.Sleep(Convert.ToInt32(duration) * 1000);

		Logger.Info("Killing frontend");
		Utils.KillProcessByName("HD-Frontend");

		/*
		   IntPtr handle = Common.Interop.Window.FindWindow(null, "BlueStacks App Player");
		   if (Window.IsWindowVisible(handle))
		   {
		   Logger.Info("Killing frontend");
		   KillProcessByName("HD-Frontend");
		   }
		   else
		   {
		   Logger.Info("s2p launch: user launched the frontend. not killing it");
		   }
		   */
	}

	private static bool GetS2PAppsList()
	{
		RegistryKey hostKey = Registry.LocalMachine.OpenSubKey(Common.Strings.CloudRegKeyPath);
		string hostURL = (string)hostKey.GetValue("Host");

		string url = String.Format("{0}/api/s2p/launch?guid={1}&prod_ver={2}", hostURL, User.GUID, Version.STRING);

		//			url = "http://bluestacks-cloud.appspot.com/api/s2p/launch?guid=9b61e764-6a6a-11e4-9414-b04c770c4b3d&prod_ver=1.1.2.141&list_of_installed_apps=com.whatsapp,com.kiloo.subwaysurf,com.flipkart.android";

		string resp = Common.HTTP.Client.Get(url, null, false);
		if (resp == null)
		{
			return false;
		}

		Logger.Info("s2pAppsList resp: " + resp);

		string		jsonString	= resp;
		JSonReader	readjson	= new JSonReader();
		IJSonObject	jsonResp	= readjson.ReadAsJSonObject(jsonString);

		string		result		= "false";
		try
		{
			result		= jsonResp["success"].StringValue.Trim();
		}
		catch (Exception e)
		{
			Logger.Error("Could not parse jsonResp. Error: " + e.Message);
			return false;
		}

		if (String.Compare(result, "false", true) == 0)
		{
			Logger.Info("No list available.");
			return false;
		}

		string	appListStr	= jsonResp["appList"].StringValue.Trim();

		readjson = new JSonReader();
		jsonResp = readjson.ReadAsJSonObject(appListStr);

		if (jsonResp.Length == 0)
		{
			Logger.Info("No list available.");
			return false;
		}

		string[] appList = new string[jsonResp.Length];

		for (int i = 0; i < jsonResp.Length; i++)
		{
			IJSonObject app = jsonResp[i];

			appList[i] = app["app"].StringValue + "#"
				+ app["launch_at"].StringValue
				+ "#" + app["duration"].StringValue;
		}

		RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
		configKey.SetValue("S2PSilentLaunch", appList);

		return false;
	}

	public static bool SendRunCommand(String request)
	{
		bool success = false;

		String cmd = request;

		if (Common.VmCmdHandler.RunCommand(cmd) == "ok")
		{
			success = true;
		}

		return success;
	}

	private static void CheckAnnouncement ()
	{
		Thread announcementThread = new Thread(delegate() {
				while (true)
				{
					try
					{
						bool result = CloudAnnouncement.ShowAnnouncement();
						if (result == false)
						{
							Logger.Info("No new announcement to show.");
						}
					}
					catch (Exception ex)
					{
						Logger.Debug("Failed to show announcement. err: " + ex.ToString());
					}

					Thread.Sleep(24 * 60 * 60 * 1000);	// sleep for a day and then check again
				}
				});

		announcementThread.IsBackground = true;
		announcementThread.Start();
	}

	public static bool DoRunCmd(String request, string vmName)
	{
		bool success = false;

		String cmd = request;

		if (Common.VmCmdHandler.RunCommand(cmd) == "ok")
		{
			success = true;
			if (request.Contains("mpi.v23"))
			{
				Logger.Info("starting amidebug. not sending message to frontend.");
				return success;
			}

			/*
			 * On receiving an "ok" response from HCCommandProcessor
			 * send a message to frontend to hide the splash screen
			 */

			String name = Common.Strings.AppTitle;
			IntPtr handle = Common.Interop.Window.FindWindow(null, name);
			if (handle != IntPtr.Zero)
			{
				Logger.Info("Sending WM_USER_SHOW_WINDOW to Frontend Handle {0}", handle);
				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_USER_SHOW_WINDOW, IntPtr.Zero, IntPtr.Zero);
			}
		}

		// Extract app name and add icon for it in launcher's recent app list.
		// request format: "run <App name>"
		string appName = "";
		string packageName = "";
		string activityName = "";
		string imagePath = "";
		string target = "";
		string storeapp = "";

		if (request.StartsWith("runex"))
		{
			Regex regex = new Regex("^runex\\s+");
			packageName = regex.Replace(request, "");
			packageName = packageName.Substring(0, packageName.IndexOf('/'));

			if (!JsonParser.GetAppInfoFromPackageName(packageName, out appName, out imagePath, out activityName, out storeapp))
			{
				Logger.Error("Failed to get App info for: {0}. Not adding in launcher dock.", packageName);
				return success;
			}
		}

		string version = GetVersionFromPackage(packageName, vmName);

		/*if (storeapp == "yes")
			Common.Stats.SendAppStats(appName, packageName, version, "", Common.Stats.AppType.market);
		else
			Common.Stats.SendAppStats(appName, packageName, version, "", Common.Stats.AppType.app);*/

		string	appsDir		= Path.Combine(Common.Strings.LibraryDir, Common.Strings.MyAppsDir);

		target = appsDir + appName + ".lnk";
		imagePath = Common.Strings.GadgetDir + imagePath;

		return success;
	}

	public static string GetVersionFromPackage(string packageName, string vmName)
	{
		string version = "";
		if (s_InstalledPackages == null)
			s_InstalledPackages = new Dictionary<string, int>();

		if(!s_InstalledPackages.ContainsKey(packageName))
			GetInstalledPackages(vmName);

		int ver;
		if (s_InstalledPackages.TryGetValue(packageName, out ver))
			version = Convert.ToString(ver);

		return version;
	}

	private static void GetInstalledPackages(string vmName)
	{
		string res = HTTPHandler.Get(Common.VmCmdHandler.s_ServerPort, HDAgent.s_InstalledPacakgesPath, vmName);
		JSonReader readjson = new JSonReader();
		IJSonObject installedApps = readjson.ReadAsJSonObject(res);

		string result = installedApps["result"].StringValue.Trim();

		if (result != "ok")
		{
			Logger.Error("result: {0}", result);
			return;
		}

		string installedPackages = installedApps["installed_packages"].ToString();
		Logger.Debug(installedPackages);
		readjson = new JSonReader();
		IJSonObject packagesJson = readjson.ReadAsJSonObject(installedPackages);
		for (int i=0; i<packagesJson.Length; i++)
		{
			string package = packagesJson[i]["package"].StringValue.Trim();
			int version = packagesJson[i]["version"].Int32Value;

			try {
				s_InstalledPackages.Add(package, version);
			} catch (Exception) {
			}
		}
	}

	private static string GetURLSafeBase64String(string originalString)
	{
		string base64String = System.Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(originalString));
		return base64String;
	}

	private static void InitExceptionHandlers()
	{
		Application.ThreadException += delegate(Object obj,
				System.Threading.ThreadExceptionEventArgs evt)
		{
			Logger.Error("HDAgent: Unhandled Exception:");
			Logger.Error(evt.Exception.ToString());

			try
			{
				UploadCrashLogs(evt.Exception.ToString());
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}

			Environment.Exit(1);
		};

		Application.SetUnhandledExceptionMode(
				UnhandledExceptionMode.CatchException);

		AppDomain.CurrentDomain.UnhandledException += delegate(
				Object obj, UnhandledExceptionEventArgs evt)
		{
			Logger.Error("HDAgent: Unhandled Exception:");
			Logger.Error(evt.ExceptionObject.ToString());

			try
			{
				UploadCrashLogs(evt.ExceptionObject.ToString());
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}

			Environment.Exit(1);
		};
	}

	public static void NotifyAppCrashToParentWindow(String className, String windowName)
	{
		Logger.Info("Sending WM_USER_APP_CRASHED message to class = {0}, window = {1}", className, windowName);
		try
		{
			IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
			if (handle == IntPtr.Zero)
			{
				Logger.Info("Unable to find window : {0}", className);
				return;
			}
			Common.Interop.Window.SendMessage(
					handle,
					Common.Interop.Window.WM_USER_APP_CRASHED,
					IntPtr.Zero,
					IntPtr.Zero
					);
		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
		}
	}

	public static void NotifyExeCrashToParentWindow(String className, String windowName)
	{
		Logger.Info("Sending WM_USER_EXE_CRASHED message to class = {0}, window = {1}", className, windowName);
		try
		{
			IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
			if (handle == IntPtr.Zero)
			{
				Logger.Info("Unable to find window : {0}", className);
				return;
			}
			Common.Interop.Window.SendMessage(
					handle,
					Common.Interop.Window.WM_USER_EXE_CRASHED,
					IntPtr.Zero,
					IntPtr.Zero
					);
		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
		}
	}

	private static void UploadCrashLogs(String errorMsg)
	{
		RegistryKey	hostKey	= Registry.LocalMachine.OpenSubKey(Common.Strings.HKCURegKeyPath);
		string hostURL = (string)hostKey.GetValue("Host");
		string url = String.Format("{0}/{1}", hostURL, Common.Strings.AgentCrashReportUrl);

		Dictionary<String, String> postData = new Dictionary<String, String>();
		postData.Add("error", errorMsg);

		Common.HTTP.Client.Post(url, postData, null, true);
	}

	public static bool IsWindowOpen(String title)
	{
		IntPtr handle = Common.Interop.Window.FindWindow(null, title);
		if (handle == IntPtr.Zero)
		{
			Logger.Info("{0} not open", title);
			return false;
		}

		Logger.Info("{0} already open", title);
		return true;
	}

	private static Dictionary<string, int> s_InstalledPackages = null;

	private static Mutex 	s_HDAgentLock;

	public static string	s_InstallDir;
	const string		BluestacksExe	= "HD-Frontend.exe";

	public static int	s_AgentPort		= 2861;
	public static string	s_InstallPath		= "install";
	public static string	s_BrowserInstallPath	= "browserinstall";
	public static string	s_UninstallPath		= "uninstall";
	public static string	s_InstalledPacakgesPath = "installedpackages";
	public static string	s_ClipboardDataPath	= "clipboard";
	public static string	s_GetDiskUsage		= Common.Strings.GetDiskUsage; // "getdiskusage";
	public static ClipboardMgr	clipboardClient;

	public static string	s_RootDir		= Path.Combine(Common.Strings.BstUserDataDir, "www");

	public static Dictionary<String, String[]> sOemWindowMapper;
}
}
