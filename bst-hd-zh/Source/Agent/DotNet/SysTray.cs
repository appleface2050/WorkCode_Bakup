using System;
using System.IO;
using System.Net;
using System.Reflection;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Diagnostics;
using System.ServiceProcess;
using System.Threading;
using System.Net.NetworkInformation;
using System.Collections.Generic;

using BlueStacks.hyperDroid.Locale;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Updater;   
using BlueStacks.hyperDroid.Cloud.Services;

using CodeTitans.JSon;  

namespace BlueStacks.hyperDroid.Agent {
class SysTray {

	private static string s_AgentOnlineText			= "App Player online";

	private static NotifyIcon s_SysTrayIcon;
	private static ContextMenuStrip s_ContextMenuStrip = null;

	private static bool	s_TrayAnimationStarted	= false;
	private static string	s_NotificationTitle	= "";
	private static string	s_NotificationMsg	= "";

	public static void Init()
	{
		if (Common.Features.IsFeatureEnabled(Common.Features.SYS_TRAY_SUPPORT) == false)
		{
			Logger.Info("Disabling systray support because feature is disabled.");
			return;
		}

		s_AgentOnlineText	 = "App Player online";
		s_AgentOnlineText	 = BlueStacks.hyperDroid.Common.Oem.Instance.GetTitle(s_AgentOnlineText);

		s_SysTrayIcon = new NotifyIcon();

		s_SysTrayIcon.BalloonTipClicked += new EventHandler(AppAndExplorerLauncher);

		s_SysTrayIcon.Icon = Utils.GetApplicationIcon();
		s_SysTrayIcon.Text = s_AgentOnlineText;
		
		s_SysTrayIcon.Text += " (" + Version.STRING + ")";

		s_SysTrayIcon.MouseDown += OnSysTrayMouseDown;
		s_SysTrayIcon.MouseUp += OnSysTrayMouseUp;

		RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		String installType = (String)prodKey.GetValue("InstallType");
		if (String.Compare(installType, "uninstalled", true) != 0)
		{
			s_SysTrayIcon.Visible = true;
		}
		else
		{
			Logger.Info("Not showing tray icon for intallType: " + installType);
		}

		if (!Features.IsFeatureEnabled(Features.SHOW_AGENT_ICON_IN_SYSTRAY))
		{
			s_SysTrayIcon.Visible = false;
		}
	}

	public static void StartTrayAnimation(string title, string msg)
	{
		title =	BlueStacks.hyperDroid.Common.Oem.Instance.GetTitle(title);
		s_NotificationTitle = title;
		s_NotificationMsg = msg;

		if (s_TrayAnimationStarted == true)
			return;

		s_TrayAnimationStarted = true;

		Thread t = new Thread(delegate() {
				StartAnimation();
				});
		t.IsBackground = true;
		t.Start();
	}

	public static void StopTrayAnimation()
	{
		s_TrayAnimationStarted = false;
		s_SysTrayIcon.Icon = Utils.GetApplicationIcon();
	}

	private static void StartAnimation()
	{
		ShowInfoLong(s_NotificationTitle, s_NotificationMsg);

		string installDir = HDAgent.s_InstallDir;
		string iconFile;
		Icon icon;
		while (true)
		{
			if (s_TrayAnimationStarted == false)
				break;

			for (int i = 1; i <= 6; i++)
			{
				iconFile = Path.Combine(installDir, string.Format("trayIcon{0}.ico", i));
				icon = new Icon(iconFile);
				s_SysTrayIcon.Icon = icon;
				Thread.Sleep(300);
			}

			Thread.Sleep(2000);	// animate every two seconds, untill stopped
		}

		s_SysTrayIcon.Icon = Utils.GetApplicationIcon();
	}

	private static void HandleTrayAnimationClick()
	{
		ShowInfoLong(s_NotificationTitle, s_NotificationMsg);
	}

	public static void SetTrayIconVisibility(bool visible)
	{
		if (Features.IsFeatureEnabled(Features.SHOW_AGENT_ICON_IN_SYSTRAY))
		{
			s_SysTrayIcon.Visible = visible;
		}
	}

	private static void AddContextMenus()
	{
		if (s_ContextMenuStrip != null)
			s_ContextMenuStrip.Dispose();

		s_ContextMenuStrip = new ContextMenuStrip();
		s_SysTrayIcon.ContextMenuStrip = s_ContextMenuStrip;

		AddAppPlayerMenuItems();
	}

	private static void AddAppPlayerMenuItems()
	{
		if (BlueStacks.hyperDroid.Common.Oem.Instance.IsOnlyStopButtonToBeAddedInContextMenuOFSysTray)
		{
			AddStopContextMenu();
			return;
		}

		ToolStripSeparator menuSeparator1 = new ToolStripSeparator();
		ToolStripSeparator menuSeparator2 = new ToolStripSeparator();

		AddZipLogsContextMenu();

		if (BlueStacks.hyperDroid.Common.Oem.Instance.IsAddProblemReportMenuInSysTray)
		{
			AddNetEaseProblemReportMenu();
		}

		if (Common.Features.IsFeatureEnabled(Common.Features.SHOW_RESTART) == true)
		{
			AddRestartBlueStacksContextMenu();
		}

		AddPortraitModeContextMenu();

		if (Common.Features.IsFeatureEnabled(Common.Features.SHOW_USAGE_STATS) == true)
		{
			AddBstUsageContextMenu();
		}

		if (Common.Features.IsFeatureEnabled(Common.Features.OTA_SUPPORT))
		{
			AddUpdatesContextMenu();
		}

		s_ContextMenuStrip.Items.Add(menuSeparator1);

		if (AddOptionalContextMenu())
			s_ContextMenuStrip.Items.Add(menuSeparator2);

		AddStopContextMenu();

		s_ContextMenuStrip.ShowCheckMargin = false;
		s_ContextMenuStrip.ShowImageMargin = false;
	}

	private static void OnSysTrayMouseDown(Object sender, MouseEventArgs e)
	{
		if (e.Button == MouseButtons.Right)
		{
			s_SysTrayIcon.ContextMenuStrip = null;
		}
	}

	private static void OnSysTrayMouseUp(Object sender, MouseEventArgs e)
	{
		if (s_TrayAnimationStarted == true)
		{
			HandleTrayAnimationClick();
			return;
		}
		if (e.Button == MouseButtons.Left && Oem.Instance.IsLefClickOnTrayIconLaunchFrontend)
		{
			Logger.Info("Starting Frontend on systray icon click");
			Utils.StartFrontend();
		}
		else if (e.Button == MouseButtons.Left && Oem.Instance.IsLefClickOnTrayIconLaunchPartner)
		{
			Logger.Info("Starting Partner on systray icon click");
			Utils.StartExe(Utils.GetPartnerExecutablePath());
		}
		else
		{
			AddContextMenus();
			MethodInfo oMethodInfo = typeof(NotifyIcon).GetMethod("ShowContextMenu", BindingFlags.Instance | BindingFlags.NonPublic);
			oMethodInfo.Invoke(s_SysTrayIcon, null);
		}
	}

	public static void ShowInfoShort(String title, String message)
	{
		title =	BlueStacks.hyperDroid.Common.Oem.Instance.GetTitle(title);
		ShowTrayStatus(System.Windows.Forms.ToolTipIcon.Info, title, message, 1000);
	}

	public static void ShowInstallAlert(String appName, String imagePath, String title, String installMsg)
	{
		CustomAlert.ShowInstallAlert(imagePath, title, installMsg, delegate (Object o, EventArgs e) {
			Logger.Info("Clicked on InstallAlert");

			//String vmName = "Android";
			String packageName;
			String imageName;
			String activityName;

			if (!JsonParser.GetAppInfoFromAppName(appName, out packageName, out imageName, out activityName))
			{
			Logger.Error("Failed to launch app: {0}. No info found in json", appName);
			return;
			}

			String prog = HDAgent.s_InstallDir + @"\HD-RunApp.exe";

			Process proc = Process.Start(prog, String.Format("-p {0} -a {1}", packageName, activityName));
		});
	}

	public static void ShowUninstallAlert(String title, String uninstallMsg)
	{
		String imagePath = Path.Combine(HDAgent.s_InstallDir, "ProductLogo.png");
		CustomAlert.ShowUninstallAlert(imagePath, title, uninstallMsg);
	}

	public static void ShowAndroidNotification(String msg, String name, String package, String activity, String imagePath)
	{
		CustomAlert.ShowAndroidNotification(imagePath, name, msg, delegate (Object o, EventArgs e) {
			//String vmName = "Android";
			String prog = HDAgent.s_InstallDir + @"\HD-RunApp.exe";

			Logger.Info("Starting RunApp");
			Process proc = Process.Start(prog, String.Format("-p {0} -a {1}",package, activity));
		});
	}

	public static void ShowInfoLong(String title, String message)
	{
		ShowTrayStatus(System.Windows.Forms.ToolTipIcon.Info, title, message, 2000);
	}

	public static void ShowTrayStatus(System.Windows.Forms.ToolTipIcon icon, String title, String message, int timeout)
	{
		title =	BlueStacks.hyperDroid.Common.Oem.Instance.GetTitle(title);
		Logger.Info(string.Format("icon type = {0}", icon));
		int retries = 30;
		while (retries > 0)
		{
			if (s_SysTrayIcon == null)
			{
				Thread.Sleep(1000);
				retries--;
				continue;
			}
			else
			{
				break;
			}
		}

		lock (s_SysTrayIcon)
		{
			s_SysTrayIcon.BalloonTipTitle = title;
			s_SysTrayIcon.BalloonTipIcon = icon; //System.Windows.Forms.ToolTipIcon.Warning;
			s_SysTrayIcon.BalloonTipText  = message;
			s_SysTrayIcon.ShowBalloonTip(timeout);
		}
	}

	public static void LaunchExplorer(string message)
	{
		try
		{
			string[] files = message.Split('\n');
			string copyDir = Directory.GetParent(files[0]).FullName;
			string cmd = "explorer.exe";
			string args;
			if(files.Length == 1)
				args = string.Format("/Select, {0}", files[0]);
			else
				args = copyDir;
			Process.Start(cmd, args);
		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
		}
	}

	public static void AppAndExplorerLauncher(object sender, EventArgs e)
	{
		Logger.Info("Clicked on BalloonTip");

		if (s_TrayAnimationStarted)
		{
			CloudAnnouncement.UpdateClickStats();
		}

		StopTrayAnimation();

		try
		{
			string installMsg = Locale.Strings.InstallSuccess;
			string updateMsg = Locale.Strings.InstallUpdates;
			string uninstallMsg = Locale.Strings.UninstallSuccess;
			string senderTitle = ((NotifyIcon) sender).BalloonTipTitle;
			string senderMessage = ((NotifyIcon) sender).BalloonTipText;
			string appName = "";
			if (senderMessage.Contains(installMsg) || senderMessage.Contains(updateMsg))
				appName = senderMessage.Substring(0, senderMessage.LastIndexOf(installMsg) - 1);
			else if (senderMessage.Contains(uninstallMsg))
				return;
			else
				appName = senderTitle;

			if (string.Compare(appName, "Successfully copied files:", true) == 0
					|| string.Compare(appName, "Cannot copy files:", true) == 0)
			{
				LaunchExplorer(senderMessage);
				return;
			}

			if (appName.Contains("Graphics Driver Checker"))
			{
				UpdateGraphicsDrivers();
				return;
			}

			Logger.Info("Launching " + appName);

			String packageName = "com.bluestacks.appmart";	// default package to launch
			String activityName = "com.bluestacks.appmart.StartTopAppsActivity";
			String imageName;

			String prog = HDAgent.s_InstallDir + @"\HD-RunApp.exe";

			if (!JsonParser.GetAppInfoFromAppName(appName, out packageName, out imageName, out activityName))
			{
				Logger.Error("Failed to launch app: {0}. No info found in json. Starting home app", appName);
				StopTrayAnimation();

				Process.Start(prog, String.Format("-p {0} -a {1}", packageName, activityName));
				return;
			}

			Process proc = Process.Start(prog, String.Format("-p {0} -a {1}", packageName, activityName));
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
		}
	}

	private static void UpdateGraphicsDrivers()
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
		int frontendPort = (int)key.GetValue("FrontendServerPort", 2862);
		string url = String.Format("http://127.0.0.1:{0}/{1}", frontendPort, "updategraphicsdrivers");

		/*
		 * We cannot block here.  Do the HTTP post in a
		 * background thread.
		 */

		Thread thread = new Thread(delegate() {

				try {
					Common.HTTP.Client.Get(url, null, false);

				} catch (Exception exc) {
					Logger.Error(exc.ToString());
				}
				});

		thread.IsBackground = true;
		thread.Start();
	}

	private static void NoUpdatesAvailable()
	{
		Logger.Info("No updates available");
		String capt = BlueStacks.hyperDroid.Common.Oem.Instance.BlueStacksUpdaterTitle;
		String text = Locale.Strings.UPDATER_UTILITY_NO_UPDATE_TEXT;
		if (BlueStacks.hyperDroid.Common.Oem.Instance.IsHideMessageBoxIconInTaskBar)
		{
			MessageBox.Show(new Form(), text, capt, MessageBoxButtons.OK);
		}
		else
		{
			MessageBox.Show(text, capt, MessageBoxButtons.OK);
		}
	}

	public static void CheckForUpdate() 
	{
		string res = HTTPHandler.SendUpdateRequest(null, Common.Strings.UpdaterRequestUrl);
		JSonReader reader = new JSonReader();
		IJSonObject obj = reader.ReadAsJSonObject(res);
		bool success = Convert.ToBoolean(obj[0]["success"].StringValue);
		if (success)
		{
			bool status = Convert.ToBoolean(obj[0]["status"].StringValue);
			if (!status) 
			{
				NoUpdatesAvailable();
			}
			else  
			{
				DialogResult result = MessageBox.Show("Update is available. Do you want to install now?", 
						"BlueStacks",
						MessageBoxButtons.YesNo);
				if (result == DialogResult.Yes)
				{
					Process.Start("HD-UpdateHelper.exe");
					HTTPHandler.StartUpdateRequest(null, "/installupdate");
				}

			}
		}
	}

	private static void AddUpdatesContextMenu()
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegUpdaterPath);
		string displayString = (String)key.GetValue("Status", Locale.Strings.CheckForUpdates);
		ToolStripMenuItem updatesMenu = new ToolStripMenuItem(displayString);

		if (displayString == Locale.Strings.CheckForUpdates)
		{
			updatesMenu.Click += new EventHandler(delegate (Object o, EventArgs a) {
					Thread updaterThread = new Thread(delegate() {
							//Updater.Manager.DoWorkflow(true);
							CheckForUpdate();
							});

					updaterThread.IsBackground = true;
					updaterThread.Start();
					});
		}
		else if (displayString == Locale.Strings.InstallUpdates)
		{
			string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string setupDir = String.Format(@"{0}\BlueStacksSetup", programData);
			string filename = Path.GetFileName((new Uri(Manifest.URL)).LocalPath);
			string setupPath = Path.Combine(setupDir, filename);

			if (File.Exists(setupPath))
			{
				updatesMenu.Click += new EventHandler(delegate (Object o, EventArgs a) {
						Updater.Manager.UpdateBlueStacks(setupPath);
						});
			}
			else
			{
				Logger.Info("{0} not found. Adding default context menu", setupPath);
				updatesMenu.Text = Locale.Strings.CheckForUpdates;
				updatesMenu.Click += new EventHandler(delegate (Object o, EventArgs a) {
						Thread updaterThread = new Thread(delegate() {
								Updater.Manager.DoWorkflow(true);
								});

						updaterThread.IsBackground = true;
						updaterThread.Start();
						});
			}
		}
		else
		{
			updatesMenu.Enabled = false;
		}

		s_ContextMenuStrip.Items.Add(updatesMenu);
	}

	private static void AddBstUsageContextMenu()
	{
		ToolStripSeparator menuSeparator1 = new ToolStripSeparator();
		s_ContextMenuStrip.Items.Add(menuSeparator1);

		ToolStripMenuItem usageSubMenuAppCount;

		// Get installed app count by parsing apps.json
		int installedAppCount		= JsonParser.GetInstalledAppCount();
		string appUsage = String.Format("{0} {1} {2}",
				installedAppCount,
				(installedAppCount > 1 ? Locale.Strings.Apps : Locale.Strings.App),
				Locale.Strings.Installed);

		usageSubMenuAppCount		= new ToolStripMenuItem(appUsage);
		usageSubMenuAppCount.Enabled	= false;

		//(s_ContextMenuStrip.Items[3] as ToolStripMenuItem).DropDownItems.Add(usageSubMenuDisk);
		s_ContextMenuStrip.Items.Add(usageSubMenuAppCount);
	}

	private static void AddRestartBlueStacksContextMenu()
	{
		string displayString = Locale.Strings.RestartBlueStacks;
		ToolStripMenuItem restartBlueStacks = new ToolStripMenuItem(displayString);

		restartBlueStacks.Click += new EventHandler(delegate (Object o, EventArgs a) {
				RestartBlueStacks();
				});
		s_ContextMenuStrip.Items.Add(restartBlueStacks);
	}

	private static void RestartBlueStacks()
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		string installDir = (string)key.GetValue("InstallDir");
		ProcessStartInfo proc = new ProcessStartInfo();
		proc.FileName = installDir + "HD-Restart.exe";
		proc.Arguments = "Android";
		Logger.Info("SysTray: Starting " + proc.FileName);
		Process.Start(proc);
	}

	private static void AddPortraitModeContextMenu()
	{
		String display = Locale.Strings.RotatePortraitApps;
		ToolStripMenuItem item = new ToolStripMenuItem(display);

		String keyPath = Common.Strings.AndroidKeyBasePath +
		    @"FrameBuffer\0";
		String name = "EmulatePortraitMode";

		RegistryKey key = Registry.LocalMachine.OpenSubKey(
		    keyPath, true);

		/*
		 * Automatic Menu Item
		 */

		ToolStripMenuItem autoItem = new ToolStripMenuItem(
		    Locale.Strings.GetLocalizedString(
		    "RotatePortraitAppsAutomatic"));

		autoItem.Checked = key.GetValue(name) == null;

		autoItem.Click += delegate(Object obj, EventArgs evt) {
			key.DeleteValue(name, false);
		};

		/*
		 * Enabled Menu Item
		 */

		ToolStripMenuItem enabledItem = new ToolStripMenuItem(
		    Locale.Strings.GetLocalizedString(
		    "RotatePortraitAppsEnabled"));

		enabledItem.Checked = (int)key.GetValue(name, 0) != 0;

		enabledItem.Click += delegate(Object obj, EventArgs evt) {
			key.SetValue(name, 1, RegistryValueKind.DWord);
		};

		/*
		 * Disabled Menu Item
		 */

		ToolStripMenuItem disabledItem = new ToolStripMenuItem(
		    Locale.Strings.GetLocalizedString(
		    "RotatePortraitAppsDisabled"));

		disabledItem.Checked = (int)key.GetValue(name, 1) == 0;

		disabledItem.Click += delegate(Object obj, EventArgs evt) {
			key.SetValue(name, 0, RegistryValueKind.DWord);
		};

		/*
		 * Populate Menu
		 */

		item.DropDown.Items.Add(autoItem);
		item.DropDown.Items.Add(enabledItem);
		item.DropDown.Items.Add(disabledItem);

		s_ContextMenuStrip.Items.Add(item);

		/*
		 * Don't close the registry key, as we need to keep it
		 * open for our click callbacks.  It will be closed at
		 * some point in the future when it is disposed by the
		 * garbage collector.
		 */
	}

	private static void AddZipLogsContextMenu()
	{
		string displayString = Locale.Strings.UploadDebugLogs;
		ToolStripMenuItem zipLogs = new ToolStripMenuItem(displayString);

		zipLogs.Click += new EventHandler(delegate (Object o, EventArgs a) {
				ZipLogsToEmail();
				});
		s_ContextMenuStrip.Items.Add(zipLogs);
	}

	private static void AddNetEaseProblemReportMenu()
	{
		string displayString = Common.Strings.NetEaseOpenBrowserString;
		ToolStripMenuItem openBrower = new ToolStripMenuItem(displayString);

		openBrower.Click += new EventHandler(delegate (Object o, EventArgs a) {
				try
				{
				Process.Start(Common.Strings.NetEaseReportProblemUrl);
				}
				catch (Exception e)
				{
				Logger.Error("Error Occurred, Err: {0}", e.ToString());
				}
				});
		s_ContextMenuStrip.Items.Add(openBrower);
	}

	private static void ZipLogsToEmail()
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		string installDir = (string)key.GetValue("InstallDir");
		ProcessStartInfo proc = new ProcessStartInfo();
		proc.FileName = installDir + "HD-LogCollector.exe";
		Logger.Info("SysTray: Starting " + proc.FileName);
		Process.Start(proc);
	}

	private static bool AddOptionalContextMenu()
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);

		bool ret = false;
		Object optKey;

		optKey = key.GetValue("User1");
		if (optKey != null)
		{
			string val = (string)optKey;
			AddOption("User1", val);
			ret = true;
		}

		optKey = key.GetValue("User2");
		if (optKey != null)
		{
			string val = (string)optKey;
			AddOption("User2", val);
			ret = true;
		}

		optKey = key.GetValue("User3");
		if (optKey != null)
		{
			string val = (string)optKey;
			AddOption("User3", val);
			ret = true;
		}

		optKey = key.GetValue("User4");
		if (optKey != null)
		{
			string val = (string)optKey;
			AddOption("User4", val);
			ret = true;
		}

		optKey = key.GetValue("User5");
		if (optKey != null)
		{
			string val = (string)optKey;
			AddOption("User5", val);
			ret = true;
		}

		return ret;
	}

	private static void AddOption(string file, string val)
	{
		ToolStripMenuItem option = new ToolStripMenuItem(val);
		option.Click += new EventHandler(delegate (Object o, EventArgs a) {
				RunBatchFile(file);
				});
		s_ContextMenuStrip.Items.Add(option);

	}

	private static void AddStopContextMenu()
	{
		string displayString = Locale.Strings.QuitBlueStacks;
		ToolStripMenuItem quit = new ToolStripMenuItem(displayString);

		quit.Click += new EventHandler(delegate (Object o, EventArgs a) {
				Quit(false);
				});
		s_ContextMenuStrip.Items.Add(quit);
	}

	private static void RunBatchFile(string file)
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		string installDir = (string)key.GetValue("InstallDir");
		Logger.Info(String.Format("Trying to launch: \"{0}{1}.bat\"", installDir, file));

		Process runFile = new Process();
		runFile.StartInfo.CreateNoWindow = true;
		runFile.StartInfo.WindowStyle = ProcessWindowStyle.Hidden;
		runFile.StartInfo.FileName = String.Format("\"{0}{1}.bat\"", installDir, file);
		runFile.Start();
		runFile.WaitForExit();
	}

	public static void DisposeIcon()
	{
		if(s_SysTrayIcon != null)
		{
			s_SysTrayIcon.Dispose();
		}
	}

	private static void Quit(bool exitSelfProcess)
	{
		Logger.Info("SysTray: Exiting BlueStacks");

		try
		{
			Logger.Info("Setting HDQuitStatus Registry");
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath, true);
			key.SetValue("HDQuitStatus", "running", RegistryValueKind.String);
			key.Close();
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
		}

		HTTPHandler.SendGameManagerRequest(null, Common.Strings.QuitGameManagerUrl);

		string frontendExe = "HD-Frontend";

		Utils.KillProcessesByName(new String[] {
				"HD-ApkHandler",
				"HD-Adb",
				frontendExe,	
				"HD-Restart",
				"HD-RunApp",
				"HD-OBS",
				"BlueStacksTV"
				});

		Utils.StopService(Common.Strings.AndroidServiceName);

		try
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath, true);
			key.DeleteValue("HDQuitStatus");
			key.Close();
		}
		catch (Exception ex)
		{
			Logger.Error(ex.ToString());
		}

		if (exitSelfProcess)
		{
			s_SysTrayIcon.Dispose();
			Application.Exit();
		}
		else
		{
			// Just hide the tray icon and keep running in background.
			s_SysTrayIcon.Visible = false;
		}
	}
}
}
