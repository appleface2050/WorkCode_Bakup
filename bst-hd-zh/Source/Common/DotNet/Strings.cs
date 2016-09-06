using System;
using System.IO;
using Microsoft.Win32;
namespace BlueStacks.hyperDroid.Common
{
	public class Strings
	{
		private static string s_AppTitle = null;
		private static string s_VMName = null;
		static string engine = string.Empty;

		public static string AppTitle
		{
			get
			{
				if (s_AppTitle != null)
				{
					return s_AppTitle;
				}
				else
				{
					return DefaultWindowTitle;
				}
			}
			set { s_AppTitle = value; }
		}
		public static string OEM
		{
			get
			{
				return GetOemTag();
			}
		}

		public static string VMName
		{
			get
			{
				if (s_VMName == null)
					return "Android";
				return s_VMName;
			}
			set { s_VMName = value; }
		}


		public static string TmpSparseDataBackUpName
		{
			get
			{
				return "BST_Data.sparsefs_backup";
			}
		}

		public static string TmpSparseSDCardBackUpName
		{
			get
			{
				return "BST_SDCard.sparsefs_backup";
			}
		}
		public static string TmpVdiData
		{
			get
			{
				return "Data.vdi_backup";
			}
		}
		public static string TmpVdiSDCard
		{
			get
			{
				return "SDCard.vdi_backup";
			}
		}

		public static string GetHDAndroidServiceName(string vmName)
		{
			return string.Format("BstHd{0}Svc{1}", vmName, OEM);
		}
		public static string GetOemTag()
		{
			string exePath = "";
			exePath = AppDomain.CurrentDomain.BaseDirectory;
			string oemTag = "";
			string filePath = Path.Combine(exePath, "tag.txt");
			if (File.Exists(filePath))
			{
				oemTag = File.ReadAllText(filePath);
			}
			return oemTag;
		}

		public static string ControlPanelUninstallDisplayName
		{
			get { return "BlueStacks App Player"; }
		}
		public static string ControlPanelNotificationCenterUninstallDisplayName
		{
			get { return "BlueStacks Notification Center"; }
		}

		public static string GetHDPlusAndroidServiceName(string vmName)
		{
			return string.Format("BstHdPlus{0}Svc{1}", vmName, OEM);
		}
		public static string BluestacksLibraryHandlerDll
		{
			get
			{
				return "HD-LibraryHandler.dll";
			}
		}
		public static string BluestacksLoggerNativeDllName
		{
			get
			{
				return "HD-Logger-Native.dll";
			}
		}
		public static string BluestacksInstallerExeName
		{
			get
			{
				return "Bluestacks-Installer";
			}
		}
		public static string BluestacksUninstallerExeName
		{
			get
			{
				return "BluestacksUninstaller.exe";
			}
		}
		public static string BluestacksUninstallerExeConfigName
		{
			get
			{
				return "BluestacksUninstaller.exe.config";
			}
		}
		public static string GetHDAndroidServiceName()
		{
			return string.Format("BstHd{0}Svc{1}", VMName, OEM);
		}
		public static string ComDllFailure
		{
			get
			{
				return "RegisterComDll failed";
			}
		}
		public static string GetHDPlusAndroidServiceName()
		{
			return string.Format("BstHdPlus{0}Svc{1}", VMName, OEM);
		}
		public static string GetAndroidServiceName(string vmName)
		{
			if (IsEngineLegacy())
				return GetHDAndroidServiceName(vmName);
			else
				return GetHDPlusAndroidServiceName(vmName);
		}

		public static string AndroidServiceName
		{
			get
			{
				return GetAndroidServiceName(VMName);
			}
		}

		public static string AndroidServiceDisplayName
		{
			get
			{
				return string.Format("BlueStacks {0} Service {1}", VMName, OEM);
			}
		}
		public static string AndroidPlusServiceDisplayName
		{
			get
			{
				return string.Format("BlueStacks Plus {0} Service {1}", VMName, OEM);
			}
		}
		public static string OldBstUpdaterServiceName
		{
			get { return "BstHdUpdaterSvc" + OEM; }
		}
		public static string BstLogRotatorServiceName
		{
			get { return "BstHdLogRotatorSvc" + OEM; }
		}
		public static string BstLogRotatorServiceDisplayName
		{
			get
			{
				if (OEM == "")
					return "BlueStacks Log Rotator Service";
				else

					return "BlueStacks Log Rotator Service" + " " + OEM;
			}
		}
		public static bool IsEngineLegacy()
		{
			try
			{
				if (string.IsNullOrEmpty(engine))
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
					engine = (string)key.GetValue("Engine", "plus");
				}
				if (engine.Equals("legacy"))
					return true;
				return false;
			}
			catch
			{
				return true;
			}
		}

		public static string BstHypervisorDrvName
		{
			get
			{
				return "BstHdDrv" + OEM;
			}
		}
		public static string BstHDPlusDrvName
		{
			get
			{
				return "BstkDrv" + OEM;
			}
		}
		public static string BstHyperVisorDrvDisplayName
		{
			get
			{
				if (OEM == "")
					return "BlueStacks Hypervisor";
				else

					return "BlueStacks Hypervisor" + " " + OEM;
			}
		}
		public static string BstHDPlusDrvDisplayName
		{
			get
			{
				if (OEM == "")
					return "BlueStacks Plus Hypervisor";
				else

					return "BlueStacks Plus Hypervisor" + " " + OEM;
			}
		}
		public static string ThinInstallerTitle
		{
			get { return "BlueStacks Download Manager"; }
		}
		public static string ThinInstallerInitLbl
		{
			get { return "Installing BlueStacks"; }
		}
		public static string ThinInstallerInstallStateTitle
		{
			get { return "Installing BlueStacks"; }
		}
		public static string RuntimeDisplayName
		{
			get { return "BlueStacks Android Plugin"; }
		}
		public static string AppPlayerDisplayName
		{
			get { return "BlueStacks App Player"; }
		}
		public static string NetEaseAndroidSimulator
		{
			get { return "网易安卓模拟器"; }
		}
		public static string NetEaseFrontendExitMessage
		{
			get { return "网易安卓模拟器已关闭，可在此处重新启动"; }
		}
		public static string GetFrontendLockName(string vmName)
		{
			return string.Format("Global\\BlueStacks_{0}_Frontend_Lock", vmName);
		}
		public static string FrontendLockName
		{
			get { return GetFrontendLockName(VMName); }
		}
		public static string MultiInsLockName
		{
			get { return "Global\\BlueStacks_MULTI_INS_Frontend_Lock"; }
		}
		public static string DeployToolLockName
		{
			get { return "Global\\BlueStacks_Android_DeployTool_Lock"; }
		}
		public static string LogCollectorLockName
		{
			get { return "Global\\BlueStacks_Log_Collector_Lock"; }
		}
		public static string BlueStacksTVLockName
		{
			get { return "Global\\BlueStacks_BTV_Lock"; }
		}
		public static string HDAgentLockName
		{
			get { return "Global\\BlueStacks_HDAgent_Lock"; }
		}
		public static string GetHDApkInstallerLockName(string vmName)
		{
			return string.Format("Global\\BlueStacks_HDApkInstaller_{0}_Lock", vmName);
		}
		public static string HDApkInstallerLockName
		{
			get { return GetHDApkInstallerLockName(VMName); }
		}
		public static string HDQuitMultiInstanceLockName
		{
			get { return "Global\\BlueStacks_HDQuitMultiInstace_Lock"; }
		}
		public static string RestartLockName
		{
			get { return "Global\\BlueStacks_Restart_Lock"; }
		}
		public static string LogRotateLockName
		{
			get { return "Global\\BlueStacks_LogRotate_Lock"; }
		}
		public static string ApkThinInstallerLockName
		{
			get { return "Global\\BlueStacks_ApkThinInstaller_Lock"; }
		}
		public static string InstallerLockName
		{
			get { return "Global\\BlueStacks_ThinInstaller_Lock"; }
		}
		public static string GameManagerLockName
		{
			get { return GetGameManagerLockName(VMName); }
		}
		public static string GetGameManagerLockName(string vmName)
		{
			return string.Format("Global\\BlueStacks_GameManager_{0}_Lock", vmName);
		}
		public static string LogFailureRegLockName
		{
			get { return "Global\\BlueStacks_Log_Failure_Lock"; }
		}
		public static string UpdaterLockName
		{
			get { return "Global\\BlueStacks_Updater_Lock"; }
		}
		public static string DownloaderLockName
		{
			get { return "Global\\BlueStacks_Downloader_Lock"; }
		}
		public static string DataManagerLock
		{
			get { return "Global\\BlueStacks_Downloader_Lock"; }
		}
		public static string AnotherInstanceRunning
		{
			get
			{
				return "Access is denied. " + "You probably have another instance of BlueStacks running from another user account.";
			}
		}
		public static string RegBasePath
		{
			get
			{
				if (OEM == "")
					return @"Software\BlueStacks";
				else
					return @"Software\BlueStacks" + OEM;
			}

		}
		public static string RegWow64BasePath
		{
			get
			{
				if (OEM == "")
					return @"Software\Wow6432Node\BlueStacks";
				else
					return @"Software\Wow6432Node\BlueStacks" + OEM;
			}

		}
		public static string RegSharedFolderPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\SharedFolder\\0\\", RegBasePath, VMName); }
		}

		public static string RegBlueStacksOemPath
		{
			get { return @"Software\BlueStacksOem"; }
		}
		public static string MsiInstallerClassesRootRegPath
		{
			get { return @"Installer\Products\"; }
		}
		public static string MsiInstallerProductRegPath
		{
			get { return @"SOFTWARE\Classes\Installer\Products\"; }
		}
		public static string UninstallCurrentVersionRegPath
		{
			get { return @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall"; }
		}
		public static string ClassesInstallerProductRegPath
		{
			get { return @"SOFTWARE\Classes\Installer\Products"; }
		}
		public static string GMBasePath
		{
			get { return string.Format("{0}\\BlueStacksGameManager", RegBasePath); }
		}
		public static string GMConfigPath
		{
			get { return string.Format("{0}\\BlueStacksGameManager\\Config", RegBasePath); }
		}
		public static string GMFilterPath
		{
			get { return string.Format("{0}\\BlueStacksGameManager\\Filter", RegBasePath); }
		}
		public static string GMPreferencesPath
		{
			get { return string.Format("{0}\\BlueStacksGameManager\\Preferences", RegBasePath); }
		}
		public static string RegUpdaterPath
		{
			get { return RegBasePath + @"\Updater\"; }
		}
		public static string HKLMConfigRegKeyPath
		{
			get { return RegBasePath + @"\Config\"; }
		}
		public static string GetHKLMAndroidConfigRegKeyPath(string vmName)
		{
			return string.Format("{0}\\Guests\\{1}\\Config\\", RegBasePath, vmName);
		}
		public static string HKLMAndroidConfigRegKeyPath
		{
			get { return GetHKLMAndroidConfigRegKeyPath(VMName); }
		}
		public static string GuestRegKeyPath
		{
			get { return Path.Combine(RegBasePath, "Guests\\"); }
		}
		public static string CloudRegKey
		{
			get { return RegBasePath + @"\Agent\Cloud"; }
		}
		public static string AgentRegKeyPath
		{
			get { return RegBasePath + @"\Agent\"; }
		}
		public static string FrameBufferRegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\FrameBuffer\\0", RegBasePath, VMName); }
		}
		public static string Network0RegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\Network\\0", RegBasePath, VMName); }
		}
		public static string SharedFolder0RegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\SharedFolder\\0", RegBasePath, VMName); }
		}
		public static string SharedFolder1RegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\SharedFolder\\1", RegBasePath, VMName); }
		}
		public static string SharedFolder2RegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\SharedFolder\\2", RegBasePath, VMName); }
		}
		public static string SharedFolder3RegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\SharedFolder\\3", RegBasePath, VMName); }
		}
		public static string SharedFolder4RegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\SharedFolder\\4", RegBasePath, VMName); }
		}
		public static string SharedFolder5RegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\SharedFolder\\5", RegBasePath, VMName); }
		}
		public static string LogRotatorRegKeyPath
		{
			get { return string.Format("{0}\\LogRotator", RegBasePath); }
		}
		public static string LogRotatorServiceRegKeyPath
		{
			get { return string.Format("{0}\\LogRotatorService", RegBasePath); }
		}
		public static string MonitorsRegKeyPath
		{
			get { return string.Format("{0}\\Monitors", RegBasePath); }
		}
		public static string NetworkRedirectRegKeyPath
		{
			get { return string.Format("{0}\\Guests\\{1}\\Network\\Redirect", RegBasePath, VMName); }
		}
		public static string GetAndroidKeyBasePath(string vmName)
		{
			return string.Format("{0}\\Guests\\{1}\\", RegBasePath, vmName);
		}
		public static string AndroidKeyBasePath
		{
			get { return GetAndroidKeyBasePath(VMName); }
		}
		public static string AndroidMemoryKeyName
		{
			get { return @"Memory"; }
		}
		public static string HKCURegKeyPath
		{
			get { return RegBasePath + @"\Agent\Cloud\"; }
		}
		public static string BlockDevice0RegKeyPath
		{
			get { return RegBasePath + @"\Guests\Android\BlockDevice\0"; }
		}
		public static string BlockDevice1RegKeyPath
		{
			get { return RegBasePath + @"\Guests\Android\BlockDevice\1"; }
		}
		public static string BlockDevice2RegKeyPath
		{
			get { return RegBasePath + @"\Guests\Android\BlockDevice\2"; }
		}
		public static string BlockDevice3RegKeyPath
		{
			get { return RegBasePath + @"\Guests\Android\BlockDevice\3"; }
		}
		public static string BlockDevice4RegKeyPath
		{
			get { return RegBasePath + @"\Guests\Android\BlockDevice\4"; }
		}
		public static string CloudRegKeyPath
		{
			get { return RegBasePath + @"\Agent\Cloud\"; }
		}
		public static string AppSyncRegKeyPath
		{
			get { return RegBasePath + @"\Agent\AppSync\"; }
		}
		public static string HKLMManifestRegKeyPath
		{
			get { return RegUpdaterPath + @"\Manifest\"; }
		}
		public static string GMPendingStats
		{
			get { return string.Format("{0}\\Stats", GMBasePath); }
		}
		public static string GetDiskUsage
		{
			get { return "getdiskusage"; }
		}
		public static string SystrayVisibilityUrl
		{
			get { return "systrayvisibility"; }
		}
		public static string RestartGameManagerUrl
		{
			get { return "restartgamemanager"; }
		}
		public static string ShowEnableVtPopupUrl
		{
			get { return "showenablevtpopup"; }
		}
		public static string RestartAgentUrl
		{
			get { return "restartagent"; }
		}
		public static string ExitAgentUrl
		{
			get { return "exitagent"; }
		}
		public static string ShowNotificationsUrl
		{
			get { return "usernotifications"; }
		}
		public static string SharePicUrl
		{
			get { return "sharepic"; }
		}
		public static string FileDropUrl
		{
			get { return "filedrop"; }
		}
		public static string ShowGuidanceUrl
		{
			get { return "controller_guidance_pressed"; }
		}
		public static string DefaultLauncherUrl
		{
			get { return "getdefaultlauncher"; }
		}
		public static string HostOrientationUrl
		{
			get { return "hostorientation"; }
		}
		public static string UploadCrashUrl
		{
			get { return "stats/uploadcrashreport"; }
		}
		public static string AgentCrashReportUrl
		{
			get { return "stats/agentcrashreport"; }
		}
		public static string GMCrashReportUrl
		{
			get { return "stats/gmcrashreport"; }
		}
		public static string UploadUsageUrl
		{
			get { return "stats/uploadusagestats"; }
		}
		public static string UploadUsageCountUrl
		{
			get { return "stats/uploadusagecountstats"; }
		}
		public static string AppClickStatsUrl
		{
			get { return "stats/appclickstats"; }
		}
		public static string WebAppChannelClickStatsUrl
		{
			get { return "stats/webappchannelclickstats"; }
		}
		public static string SearchAppStatsUrl
		{
			get { return "stats/searchappstats"; }
		}
		public static string AppInstallStatsUrl
		{
			get { return "stats/appinstallstats"; }
		}
		public static string AVGInstallStatsUrl
		{
			get { return "stats/avginstallstats"; }
		}
		public static string SystemInfoStatsUrl
		{
			get { return "stats/systeminfostats"; }
		}
		public static string BootStatsUrl
		{
			get { return "stats/bootstats"; }
		}
		public static string TimelineStatsUrl
		{
			get { return "stats/timelinestats2"; }
		}
		public static string TiDebugLogsUrl
		{
			get { return "stats/uploadscreenshot"; }
		}
		public static string HomeScreenStatsUrl
		{
			get { return "stats/homescreenstats"; }
		}
		public static string BinaryCrashStatsUrl
		{
			get { return "stats/bincrashstats"; }
		}
		public static string BsInstallStatsUrl
		{
			get { return "stats/bsinstallstats"; }
		}
		public static string GMStageStatsUrl
		{
			get { return "stats/gmstagestats"; }
		}
		public static string MiscellaneousStatsUrl
		{
			get { return "/stats/miscellaneousstats"; }
		}
		public static string BtvFunnelStatsUrl
		{
			get { return "stats/btvfunnelstats"; }
		}
		public static string GetCACodeUrl
		{
			get { return "api/getcacode"; }
		}
		public static string GetCountryUrl
		{
			get { return "api/getcountryforip"; }
		}
		public static string RegisterEmailUrl
		{
			get { return "api/auth/registeremail"; }
		}
		public static string SignUpUrl
		{
			get { return "api/auth/signup"; }
		}
		public static string LoginUrl
		{
			get { return "api/auth/login"; }
		}
		public static string ForgotPasswordUrl
		{
			get { return "forgotpassword"; }
		}
		public static string UploadDebugLogsUrl
		{
			get { return "uploaddebuglogs"; }
		}
		public static string UploadDebugLogsApkInstallFailureUrl
		{
			get { return "logs/appinstallfailurelog"; }
		}
		public static string UploadDebugLogsBootFailureUrl
		{
			get { return "logs/bootfailurelog"; }
		}
		public static string UploadDebugLogsCrashUrl
		{
			get { return "logs/crashlog"; }
		}
		public static string SlideoutMetricsUrl
		{
			get { return "UpdateSlideOutMetrics"; }
		}
		public static string IsPackageInstalledUrl
		{
			get { return "ispackageinstalled"; }
		}
		public static string GetInstalledPackagesUrl
		{
			get { return "installedpackages"; }
		}
		public static string GetLaunchActivityNameUrl
		{
			get { return "getlaunchactivityname"; }
		}
		public static string GetAppNameUrl
		{
			get { return "getappname"; }
		}
		public static string ShowFeNotificationUrl
		{
			get { return "showfenotification"; }
		}
		public static string AppDataFEUrl
		{
			get { return "appdatafeurl"; }
		}
		public static string QuitFrontend
		{
			get { return "quitfrontend"; }
		}
		public static string SwitchToLauncherUrl
		{
			get { return "switchtolauncher"; }
		}
		public static string SwitchToWindowsUrl
		{
			get { return "switchtowindows"; }
		}
		public static string ShowWindowUrl
		{
			get { return "showwindow"; }
		}
		public static string IsGMVisibleUrl
		{
			get { return "isvisible"; }
		}
		public static string S2PConfiguredUrl
		{
			get { return "s2pconfigured"; }
		}
		public static string LocaleResourceUrl
		{
			get { return "downloadlocale"; }
		}
		public static string AppInstallUrl
		{
			get { return "amzinstall"; }
		}
		public static string CheckGAUrl
		{
			get { return "gaurl"; }
		}
		public static string CheckGraphicsDriverUrl
		{
			get { return "checkgraphicsdriver"; }
		}
		public static string CommandLineArgsUrl
		{
			get { return "commandlineargs"; }
		}
		public static string ShowTileInterfaceUrl
		{
			get { return "showtileinterface"; }
		}
		public static string UserDataDir
		{
			get { return "UserData"; }
		}
		public static string MyAppsDir
		{
			get { return "My Apps"; }
		}
		public static string StoreAppsDir
		{
			get { return "App Stores"; }
		}
		public static string IconsDir
		{
			get { return "Icons"; }
		}
		public static string ChannelsUrl
		{
			get { return ChannelsProdUrl; }
		}
		public static string ChannelsEC2Url
		{
			get { return "https://23.23.194.123"; }
		}
		public static string ChannelsProdUrl
		{
			get { return "https://bluestacks-cloud.appspot.com"; }
		}
		public static string ChannelsProdUpdateUrl
		{
			get { return (ChannelsProdUrl + "/checkupgrade"); }
		}
		public static string ChannelsQaUrl
		{
			get { return "https://bluestacks-cloud-qa.appspot.com"; }
		}
		public static string ChannelsDevUrl
		{
			get { return "https://bluestacks-cloud-dev.appspot.com"; }
		}
		public static string CDNDownloadUrl
		{
			get { return "http://cdn.bluestacks.com/downloads"; }
		}
		public static string CDNAppSettingsUrl
		{
			get { return "http://cdn.bluestacks.com/public/appsettings/ProblemCategories/"; }
		}
		public static string WebPlayerUrl
		{
			get { return "http://bluestacks-wap.appspot.com/play"; }
		}
		public static string FrontendStatusUpdateUrl
		{
			get { return "FrontendStatusUpdate"; }
		}
		public static string GLUnsupportedError
		{
			get { return "BlueStacks currently doesn't recognize your graphics card.\nIt is possible your Graphics Drivers may need to be updated. Please update them and try installing again."; }
		}
		public static string GLUnsupportedErrorForApkToExe
		{
			get { return "Your graphics hardware or drivers do not support apps that need high performance graphics.\nYou may update the graphics drivers and re-install the app to try to resolve this limitation."; }
		}
		public static string BitdefenderFoundError
		{
			get { return "You seem to have Bitdefender antivirus installed. BlueStacks is currently not compatible with Bitdefender. Installation will now abort."; }
		}
		public static string UninstallMessage
		{
			get { return "Do you want to keep all your apps and data?\n\nNote: This might take some space on your system depending upon app data size you currently have."; }
		}
		public static string UninstallDependentAppsMessage
		{
			get { return "Some programs require Notification Center to work properly. If you uninstall it, you will not be able to use those dependent programs.\n\n Are you sure want to uninstall Notification Center?"; }
		}
		public static string GraphicsDriverOutdatedError
		{
			get { return "BlueStacks could not be installed. Your Graphics Drivers seem to be out-of-date. We recommend you update your drivers and try installing again. Update now?"; }
		}
		public static string UpgradeNotSupported
		{
			get { return "Sorry.. This upgrade is not supported for the installed version.\n"; }
		}
		public static string UpgradeFromGingerbreadWarning1
		{
			get { return "NOTE: While we have taken utmost care to preserve your existing Apps and Data, some Apps may not work after the Upgrade. Please reinstall apps that fail to launch.\n"; }
		}
		public static string UpgradeFromGingerbreadWarning2
		{
			get { return "\nWe have detected the following apps which will need to be reinstalled:\n"; }
		}
		public static string UpgradeFromGingerbreadWarning3
		{
			get { return "\nContinue with upgrade?"; }
		}
		/*
		   public static string GraphicsDriverOutdated {
		   get { return "Your Graphics Drivers seem to be out-of-date. BlueStacks requires updated drivers to run. Update now?"; }
		   }
		   */
		public static string GAUserAccountDefault
		{
			get { return "UA-32186883-1"; }
		}
		public static string GAUserAccountAppClicks
		{
			get { return "UA-30694866-1"; }
		}
		public static string GAUserAccountInstaller
		{
			get { return "UA-30705638-1"; }
		}
		public static string GAUserAccountSuggestedApps
		{
			get { return "UA-30698067-1"; }
		}
		public static string GAUserAccountGameManager
		{
			get { return "UA-59230446-1"; }
		}
		public static string GACategorySuggestedApps
		{
			get { return "suggestedapps"; }
		}
		public static string GACategoryInstaller
		{
			get { return "installer"; }
		}
		public static string GACategoryAppClicks
		{
			get { return "appclicks"; }
		}
		public static string DefaultWindowTitle
		{
			get { return "App Player"; }
		}
		public static string LibraryName
		{
			get { return "Apps"; }
		}
		public static string StartLauncherShortcutName
		{
			get { return "Start BlueStacks"; }
		}
		public static string ManagerLauncherShortcutName
		{
			get { return "BlueStacks Manager"; }
		}
		public static string Four399Name
		{
			get { return "4399手游通"; }
		}
		public static string Four399StatusControlText
		{
			get { return "重新启动4399手游通"; }
		}
		public static string Four399LogCollectorText
		{
			get { return "4399手游通支持工具"; }
		}
		public static string BlueStacksNotFound
		{
			get { return "No BlueStacks installation found. Please download and install BlueStacks runtime from www.bluestacks.com"; }
		}
		public static string CompanyName
		{
			get { return "BlueStack Systems, Inc."; }
		}
		public static string GameManagerName
		{
			get { return "BlueStacks App Player"; }
		}
		public static string UninstallKey
		{
			get { return @"Software\Microsoft\Windows\CurrentVersion\Uninstall"; }
		}
		public static string BstPrefix
		{
			get { return "Bst-"; }
		}
		public static string UninstallKeyPrefix
		{
			get { return UninstallKey + "\\" + BstPrefix; }
		}
		public static string CommonAppData
		{
			get
			{
				string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				RegistryKey key = Registry.LocalMachine.OpenSubKey(RegBasePath);
				if (key != null)
				{
					programData = (string)key.GetValue("UserDefinedDir", programData);
				}
				return programData;
			}
		}
		public static string BstCommonAppData
		{
			get
			{
				string bstCommonAppData = Path.Combine(CommonAppData, "BlueStacks");
				RegistryKey key = Registry.LocalMachine.OpenSubKey(RegBasePath);
				if (key != null)
				{
					bstCommonAppData = (string)key.GetValue("DataDir", bstCommonAppData);
				}
				return bstCommonAppData;
			}
		}
		public static string BstAndroidDir
		{
			get { return Path.Combine(BstCommonAppData, VMName); }
		}
		public static string BstUserDataDir
		{
			get { return Path.Combine(BstCommonAppData, UserDataDir); }
		}
		public static string BstLogsDir
		{
			get { return Path.Combine(BstCommonAppData, LogDirName); }
		}
		public static string GadgetDir
		{
			get { return Path.Combine(BstUserDataDir, "Gadget"); }
		}
		public static string GameManagerDirName
		{
			get { return "BlueStacksGameManager"; }
		}

		public static string GameManagerDir
		{
			get
			{
				RegistryKey gameManagerReg = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
				string gameManagerDir = (string)gameManagerReg.GetValue("InstallDir");
				return gameManagerDir;
			}
		}
		static string mGMAssetDir;
		public static string GMAssetDir
		{
			get
			{
				if (string.IsNullOrEmpty(mGMAssetDir))
				{
					mGMAssetDir = Path.Combine(GameManagerDir, "Assets");
				}
				return mGMAssetDir;
			}
			set
			{
				mGMAssetDir = value;
			}
		}
		public static string GameManagerHomeDir
		{
			get { return Path.Combine(GameManagerDir, @"UserData\Home"); }
		}
		public static string GameManagerImageDir
		{
			get { return Path.Combine(GameManagerDir, @"UserData\AppImages"); }
		}
		public static string GameManagerBannerImageDir
		{
			get { return Path.Combine(GameManagerHomeDir, @"images"); }
		}
		public static string LibraryDir
		{
			get { return Path.Combine(BstUserDataDir, "Library"); }
		}
		public static string SharedFolderDir
		{
			get { return Path.Combine(BstUserDataDir, "SharedFolder"); }
		}
		public static string SharedFolderName
		{
			get { return "BstSharedFolder"; }
		}
		public static string InputMapperFolderName
		{
			get { return "InputMapper"; }
		}
		public static string InputMapperFolder
		{
			get { return Path.Combine(BstUserDataDir, InputMapperFolderName); }
		}
		public static string CampaignKeyName
		{
			get { return "campaign"; }
		}
		public static string CampaignKeyHeaderName
		{
			get { return "X_BST_UTM"; }
		}
		public static string CampaignIdentifier
		{
			get { return "UTM_"; }
		}
		public static string GMPreInstallCheckFailed
		{
			get { return "PreInstallCheckFailed"; }
		}
		public static string GMNotificationUrl
		{
			get { return "gmnotification"; }
		}
		public static string AppDisplayedUrl
		{
			get { return "appdisplayed"; }
		}
		public static string QuitGameManagerUrl
		{
			get { return "quit"; }
		}
		public static string UpdaterRequestUrl
		{
			get { return "checkforupdate"; }
		}
		public static string AppLaunchedUrl
		{
			get { return "applaunched"; }
		}
		public static string AppUninstalledUrl
		{
			get { return "appuninstalled"; }
		}

		public static string ShowAppUrl
		{
			get { return "showapp"; }
		}
		public static string UserAtHomeUrl
		{
			get { return "userathome"; }
		}
		public static string ExpiryDateFileName
		{
			get { return "ExpiryDate.txt"; }
		}
		public static string CaCodeBackUpFileName
		{
			get { return "Bst_CaCode_Backup"; }
		}
		public static string PCodeBackUpFileName
		{
			get { return "Bst_PCode_Backup"; }
		}
		public static string CaSelectorBackUpFileName
		{
			get { return "Bst_CaSelector_Backup"; }
		}
		public static string BlueStacksPackagePrefix
		{
			get
			{
				return "com.bluestacks";
			}
		}
		public static string SnapShotShareString
		{
			get
			{
				return "shared via BlueStacks App Player (www.bluestacks.com)";
			}
		}
		public static string SnapShot4399ShareString
		{
			get
			{
				return " 分享自4399手游通( www.4399.cn/app-emu.html )";
			}
		}
		public static string Four399ModelJsonFileName
		{
			get
			{
				return "model.json";
			}
		}
		public static string LatinImeId
		{
			get
			{
				return "com.android.inputmethod.latin/.LatinIME";
			}
		}
		public static string BluestacksGameManagerFolderName
		{
			get { return "BluestacksGameManager"; }
		}
		public static string FrontendPortBootParam
		{
			get
			{
				return "WINDOWSFRONTEND";
			}
		}
		public static string AgentPortBootParam
		{
			get
			{
				return "WINDOWSAGENT";
			}
		}
		public static string ProductLogoIconFile
		{
			get
			{
				return "ProductLogo.ico";
			}
		}
		public static string ProductLogoDefaultIconFile
		{
			get
			{
				return "bluestacks.ico";
			}
		}
		public static string PostInstallScriptName
		{
			get
			{
				return "PostInstall.bat";
			}
		}
		public static string VersionKeyName
		{
			get
			{
				return "Version";
			}
		}
		public static string VMXBitIsOn
		{
			get
			{
				return "Cannot run guest while VMX is in use";
			}
		}
		public static string InvalidOpCode
		{
			get
			{
				return "invalid_op";
			}
		}
		public static string KernelPanic
		{
			get
			{
				return "Kernel panic";
			}
		}
		public static string ArmBinarySupportAddedString
		{
			get
			{
				return "ARM binary format support added successfully";
			}
		}
		public static string Ext4Error
		{
			get
			{
				return ".*EXT4-fs error \\(device sd[a-b]1\\): (mb_free_blocks|ext4_mb_generate_buddy|ext4_lookup|.*deleted inode referenced):";
			}
		}
		public static string PingPath
		{
			get
			{
				return "ping";
			}
		}
		public static string PgaCtlInitFailedString
		{
			get
			{
				return "BlueStacks.hyperDroid.Frontend.Interop.Opengl.GetPgaServerInitStatus()";
			}
		}
		public static string BootFailureCategory
		{
			get
			{
				return "BootFailure";
			}
		}
		public static string FailureLogRegPath
		{
			get { return AndroidKeyBasePath + @"\FailureLogsInfo\"; }
		}
		public static string StopAppInfo
		{
			get
			{
				return "stopappinfo";
			}
		}
		public static string ClearAppDataInfo
		{
			get
			{
				return "clearappdata";
			}
		}
		public static string RunAppInfo
		{
			get
			{
				return "runappinfo";
			}
		}
		public static string AndroidCustomActivityLaunchApi
		{
			get
			{
				return "customstartactivity";
			}
		}
		public static string AppCrashInfoFile
		{
			get
			{
				return "App_C_Info.txt";
			}
		}
		public static string LogKeyName
		{
			get
			{
				return "LogDir";
			}
		}
		public static string LogDirName
		{
			get
			{
				return "Logs";
			}
		}
		public static string GLRenderModeKeyName
		{
			get
			{
				return "GlRenderMode";
			}
		}
		public static string AgentPortKeyName
		{
			get
			{
				return "AgentServerPort";
			}
		}
		public static string FrontendPortKeyName
		{
			get
			{
				return "FrontendServerPort";
			}
		}
		public static string PartnePortKeyName
		{
			get
			{
				return "PartnerServerPort";
			}
		}
		public static string PartneExePathKeyName
		{
			get
			{
				return "PartnerExePath";
			}
		}
		public static string GMLaunchWebTab
		{
			get
			{
				return "launchwebtab";
			}
		}
		public static string InstallTimeTempGraphicsCheckFileName
		{
			get
			{
				return "Bst_InstallTimeGraphicsCheck.txt";
			}
		}
		public static string InstallTimeGraphicsCheckFileName
		{
			get
			{
				return "InstallTimeGraphicsCheck.txt";
			}
		}
		public static string AppNotInstalledString
		{
			get
			{
				return "package not installed";
			}
		}

		public static string OEMNameNetEase
		{
			get
			{
				return "nt_dt";
			}
		}
		public static string OEMNameNetEase2
		{
			get
			{
				return "nt2_dt";
			}
		}
		public static string NetEaseReportProblemUrl
		{
			get
			{
				return "http://gh.163.com/m";
			}
		}
		public static string NetEaseOpenBrowserString
		{
			get
			{
				return "问题咨询";
			}
		}
		public static string OEMName4399
		{
			get
			{
				return "4399";
			}
		}
		public static string OEMName360
		{
			get
			{
				return "360";
			}
		}
		public static string OEMNameTencent
		{
			get
			{
				return "tc_dt";
			}
		}
		public static string OEMNameAcer
		{
			get
			{
				return "Acer";
			}
		}
		public static string OEMNameLenovo
		{
			get
			{
				return "Lenovo";
			}
		}
		public static string OEMNameNineOne
		{
			get
			{
				return "nineone_dt";
			}
		}
		public static string OEMNameGameManager
		{
			get
			{
				return "gamemanager";
			}
		}
		public static string OEMNameKizi
		{
			get
			{
				return "kizi";
			}
		}
		public static string OEMNameChina
		{
			get
			{
				return "china";
			}
		}
		public static string OEMNameUcWeb
		{
			get
			{
				return "ucweb";
			}
		}
		public static string OEMNameWildTangent
		{
			get
			{
				return "wildtangent";
			}
		}
		public static string OEMNameAnquiCafe
		{
			get
			{
				return "anquicafe";
			}
		}
		public static string OEMNameYY
		{
			get
			{
				return "yy_dt";
			}
		}
		public static string OEMNameChinaApi
		{
			get
			{
				return "china_api";
			}
		}
		public static string OEMNameUCWebDT
		{
			get
			{
				return "ucweb_dt";
			}
		}
		public static string OEMName7F
		{
			get
			{
				return "sf_dt";
			}
		}
		public static string OEMNameIQT
		{
			get
			{
				return "IQT";
			}
		}
		public static string OEMNameWuji
		{
			get
			{
				return "wuji_dt";
			}
		}
		public static string OEMNameBtv
		{
			get
			{
				return "btv";
			}
		}
		public static string OEMNameBluestacks
		{
			get
			{
				return "bluestacks";
			}
		}
		public static string InstallDirKeyName
		{
			get
			{
				return "InstallDir";
			}
		}
		public static string GMInstallDirKeyName
		{
			get
			{
				return "InstallDir";
			}
		}
		public static string DataVdiUUID
		{
			get
			{
				return "e99dab6a-1e34-4579-ae6b-4a7e520933c5";
			}
		}
		public static string SDCardVdiUUID
		{
			get
			{
				return "282c22bd-9c58-44ea-8223-2e64ed82fe82";
			}
		}
		public static string RootVdiUUID
		{
			get
			{
				return "fca296ce-8268-4ed7-a57f-d32ec11ab304";
			}
		}
		public static string PrebundledVdiUUID
		{
			get
			{
				return "c15de548-a277-48b1-97f4-4871ef2c2b8a";
			}
		}
		public static string StyleThemeStatsTag
		{
			get
			{
				return "StyleAndThemeData";
			}
		}
		public static string StyleThemeRender
		{
			get
			{
				return "ThemeSettings FormRender";
			}
		}
		public static string NewThemeCreated
		{
			get
			{
				return "New Theme";
			}
		}
		public static string ApplyingTheme
		{
			get
			{
				return "Applying Theme";
			}
		}
		public static string ChangingColor
		{
			get
			{
				return "Changing Color";
			}
		}
		public static string ChangingStyle
		{
			get
			{
				return "Changing Style";
			}
		}
		public static string GMStreamWindowLeftLocationKeyName
		{
			get
			{
				return "StreamWindowLeft";
			}
		}
		public static string GMStreamWindowTopLocationKeyName
		{
			get
			{
				return "StreamWindowTop";
			}
		}
		public static string GMChatWindowLeftLocationKeyName
		{
			get
			{
				return "ChatWindowLeft";
			}
		}
		public static string GMChatWindowTopLocationKeyName
		{
			get
			{
				return "ChatWindowTop";
			}
		}
		public static string PinToTaskBar
		{
			get
			{
				return "0";
			}
		}
		public static string UnpinFromTaskBar
		{
			get
			{
				return "1";
			}
		}
		public static string PinToStartMenu
		{
			get
			{
				return "2";
			}
		}
		public static string UnpinFromStartMenu
		{
			get
			{
				return "3";
			}
		}
		public static string OEMKeyName
		{
			get
			{
				return "OEM";
			}
		}
		public static string ChinaToggleDisabledAppListUrl
		{
			get
			{
				return "http://cdn.bluestacks.cn/public/appsettings/ToggleDisableAppList.cfg";
			}
		}
		public static string WorldWideToggleDisabledAppListUrl
		{
			get
			{
				return "http://cdn.bluestacks.com/public/appsettings/ToggleDisableAppList.cfg";
			}
		}
		public static string ToggleDisableAppListFileName
		{
			get
			{
				return "ToggleDisabledAppList.cfg";
			}
		}

		public static string GMOldBasePath
		{
			get { return "Software\\BluestacksGameManager"; }
		}

		public static string TombStoneFilePrefix
		{
			get { return "tombstone"; }
		}

		public static string TempFileCheckName
		{
			get { return "BST_Temp_File_Check"; }
		}

		public static string HomeAppPackageName
		{
			get { return "com.bluestacks.gamepophome"; }
		}

		public static string SharerActivityPackage
		{
			get { return "com.android.internal.app.ResolverActivity"; }
		}
		public static string GenerateIDKeyName
		{
			get { return "UpdatedVersion"; }
		}
		public static string GenerateIdFileName
		{
			get { return "UpdatedVersion.txt"; }
		}
		public static string GMSupportUrlKeyName
		{
			get { return "supporturl"; }
		}
		public static string GMChatUrlKeyName
		{
			get { return "chaturl"; }
		}
		public static string ChatApplicationUrl
		{
			get { return "chat"; }
		}
		public static string BTVDir
		{
			get
			{
				RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				string dataDir = (string)regKey.GetValue("DataDir");
				return Path.Combine(dataDir, "BlueStacksGameManager");
			}
		}
		public static string DownloadingInstallerTag
		{
			get
			{
				return "DownloadingInstaller";
			}
		}
		public static string FirstLaunchDateTimeRegistryKey
		{
			get
			{
				return "FirstLaunchDateTime";
			}
		}
	}
}
