using System;
using System.IO;
using Microsoft.Win32;
using System.Drawing;
using System.Windows.Forms;
using System.Text.RegularExpressions;
using BlueStacks.hyperDroid;
using System.ComponentModel;
using System.Xml.Serialization;
using System.Xml;
using System.Collections.Generic;
using System.Text;
namespace BlueStacks.hyperDroid.Common
{
	public class Oem
	{
		private static volatile Oem sInstance;
		private static object syncRoot = new Object();

		public static Oem Instance
		{
			get
			{
				if (sInstance == null)
				{
					lock (syncRoot)
					{
						if (sInstance == null)
						{
							LoadOem();
						}
					}
				}

				return sInstance;
			}
		}

		private static void LoadOem()
		{
			try
			{
				string fileName = "Oem.cfg";
				string currentFilePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, fileName);
				Logger.Info("Current Dir cfg path: {0}", currentFilePath);
				if (!File.Exists(currentFilePath))
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
					currentFilePath = Path.Combine((string)key.GetValue("DataDir", string.Empty), fileName);
					if (!File.Exists(currentFilePath))
					{
						throw new Exception("Oem.cfg file not found");
					}
				}
				using (FileStream fs = File.OpenRead(currentFilePath))
				{
					Logger.Info("Loading Oem Settings from " + currentFilePath);
					XmlSerializer serializer = new XmlSerializer(typeof(Oem));
					sInstance = (Oem)serializer.Deserialize(fs);
					fs.Flush();
				}

			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured while loading oem config file" + ex.ToString());
				sInstance = new Oem();
			}
		}

		internal void Save()
		{
			try
			{
				string fileName = "Oem.cfg";
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				string currentFilePath = Path.Combine((string)key.GetValue("DataDir", string.Empty), fileName);
				using (XmlTextWriter writer = new XmlTextWriter(currentFilePath, Encoding.UTF8))
				{
					writer.Formatting = Formatting.Indented;
					XmlSerializer serialize = new XmlSerializer(typeof(Oem));
					serialize.Serialize(writer, sInstance);
					writer.Flush();
				}
			}
			catch (Exception ex)
			{
				Logger.Info(ex.ToString());
			}
		}

		public String GetTitle(string title)
		{
			if (DefaultTitle.Equals("DefaultTitle"))
			{
				return title;
			}
			return DefaultTitle;
		}

		#region collection properties

		private BindingList<string> _deleteDesktopShortcutFileNames;
		[Browsable(true)]
		[Category("DesktopShortcut")]
		[DisplayName("DeleteDesktopShortcutFileNames")]
		[Description("We have shortcuts with different names over different versions \n while uninstalling we will delete all \n mention all previous and current shortcut names in this field")]
		public BindingList<string> DeleteDesktopShortcutFileNames
		{
			get
			{
				if (_deleteDesktopShortcutFileNames == null)
					_deleteDesktopShortcutFileNames = new BindingList<string>();

				return _deleteDesktopShortcutFileNames;
			}
			set
			{
				if (_deleteDesktopShortcutFileNames == null)
					_deleteDesktopShortcutFileNames = new BindingList<string>();

				_deleteDesktopShortcutFileNames = value;
			}
		}

		#endregion

		#region Feature bits properties
		private ulong _windowsOEMFeatures;
		[Browsable(true)]
		[Description("Windows Feature Bit Hign and low")]
		[Category("0 Properties For All")]
		[DefaultValue(0)]
		[DisplayName("WindowsOEMFeatures")]
		public ulong WindowsOEMFeatures
		{
			get { return _windowsOEMFeatures; }
			set { _windowsOEMFeatures = value; }
		}

		private ulong _appsOEMFeaturesBits;
		[Browsable(true)]
		[Description("App Feature Bit Hign and low")]
		[Category("0 Properties For All")]
		[DefaultValue(0)]
		[DisplayName("AppsOEMFeaturesBits")]
		public ulong AppsOEMFeaturesBits
		{
			get { return _appsOEMFeaturesBits; }
			set { _appsOEMFeaturesBits = value; }
		}

		private string _msgWindowClassName;
		[Browsable(true)]
		[Description("Message window class name")]
		[Category("Message Window")]
		[DefaultValue(null)]
		[DisplayName("MsgWindowClassName")]
		public string MsgWindowClassName
		{

			get { return _msgWindowClassName; }
			set { _msgWindowClassName = value; }
		}

		private string _msgWindowTitle;
		[Browsable(true)]
		[Description("Message window title")]
		[Category("Message Window")]
		[DefaultValue(null)]
		[DisplayName("MsgWindowTitle")]
		public string MsgWindowTitle
		{
			get { return _msgWindowTitle; }
			set { _msgWindowTitle = value; }
		}

		private uint _androidFeatureBits;
		[Browsable(true)]
		[Description("Android Feature Bit")]
		[Category("0 Properties For All")]
		[DefaultValue(0)]
		[DisplayName("AndroidFeatureBits")]
		public uint AndroidFeatureBits
		{
			get { return _androidFeatureBits; }
			set { _androidFeatureBits = value; }
		}
		#endregion

		#region bool properties

		private bool _isRunDirectoryPermissionOnBlueStacksFolder = true;
		[Browsable(true)]
		[Description("Default true")]
		[Category("Tencent Installer")]
		[DefaultValue(false)]
		[DisplayName("IsRunDirectoryPermissionOnBlueStacksFolder")]
		public bool IsRunDirectoryPermissionOnBlueStacksFolder
		{
			get
			{
				return _isRunDirectoryPermissionOnBlueStacksFolder;
			}
			set
			{
				_isRunDirectoryPermissionOnBlueStacksFolder = value;
			}
		}

		private bool _isDebugMode = false;
		[Browsable(true)]
		[Description("Default false")]
		[Category("Debug")]
		[DefaultValue(false)]
		[DisplayName("IsDebugMode")]
		public bool IsDebugMode
		{
			get
			{
				return _isDebugMode;
			}

			set
			{
				_isDebugMode = value;
			}
		}


		private bool _isUseExtraBootRetries = false;
		[Browsable(true)]
		[Description("Default false")]
		[Category("Tencent")]
		[DefaultValue(false)]
		[DisplayName("IsUseExtraBootRetries")]
		public bool IsUseExtraBootRetries
		{
			get
			{
				return _isUseExtraBootRetries;
			}

			set
			{
				_isUseExtraBootRetries = value;
			}
		}
		private bool _isRunBlueStacksAutoUpgradeMechanism = true;
		[Browsable(true)]
		[Description("Default true make false for 4399")]
		[Category("4399")]
		[DefaultValue(false)]
		[DisplayName("IsRunBlueStacksAutoUpgradeMechanism")]
		public bool IsRunBlueStacksAutoUpgradeMechanism
		{
			get { return _isRunBlueStacksAutoUpgradeMechanism; }
			set { _isRunBlueStacksAutoUpgradeMechanism = value; }
		}

		private bool _isReportExeAppCrashLogs = false;
		[Browsable(true)]
		[Description("Default false")]
		[Category("Tencent")]
		[DefaultValue(false)]
		[DisplayName("IsReportExeAppCrashLogs")]
		public bool IsReportExeAppCrashLogs
		{
			get
			{
				return _isReportExeAppCrashLogs;
			}

			set
			{
				_isReportExeAppCrashLogs = value;
			}
		}

		private bool _isExitMenuToBeDisplayed = true;
		[Browsable(true)]
		[Description("Default true")]
		[Category("VNG")]
		[DefaultValue(true)]
		[DisplayName("IsExitMenuToBeDisplayed")]
		public bool IsExitMenuToBeDisplayed
		{
			get
			{
				return _isExitMenuToBeDisplayed;
			}

			set
			{
				_isExitMenuToBeDisplayed = value;
			}
		}

		private bool _isAppToBeForceKilledOnTabClose = true;
		[Browsable(true)]
		[Description("Default true")]
		[Category("VNG")]
		[DefaultValue(true)]
		[DisplayName("IsAppToBeForceKilledOnTabClose")]
		public bool IsAppToBeForceKilledOnTabClose
		{
			get
			{
				return _isAppToBeForceKilledOnTabClose;
			}

			set
			{
				_isAppToBeForceKilledOnTabClose = value;
			}
		}

		private bool _isGamePadEnabled = true;
		[Browsable(true)]
		[Description("Default true")]
		[Category("VNG")]
		[DefaultValue(true)]
		[DisplayName("IsGamePadEnabled")]
		public bool IsGamePadEnabled
		{
			get
			{
				return _isGamePadEnabled;
			}

			set
			{
				_isGamePadEnabled = value;
			}
		}

		private bool _isSlideUpTabBar = false;
		[Browsable(true)]
		[Description("Default false")]
		[Category("VNG")]
		[DefaultValue(false)]
		[DisplayName("IsSlideUpTabBar")]
		public bool IsSlideUpTabBar
		{
			get
			{
				return _isSlideUpTabBar;
			}

			set
			{
				_isSlideUpTabBar = value;
			}
		}

		private bool _isSideBarVisible = true;
		[Browsable(true)]
		[Description("Default true")]
		[Category("VNG")]
		[DefaultValue(true)]
		[DisplayName("IsSideBarVisible")]
		public bool IsSideBarVisible
		{
			get
			{
				return _isSideBarVisible;
			}

			set
			{
				_isSideBarVisible = value;
			}
		}

		private bool _isAndroidToBeStayAwake = false;
		[Browsable(true)]
		[Description("Default false")]
		[Category("VNG")]
		[DefaultValue(false)]
		[DisplayName("IsAndroidToBeStayAwake")]
		public bool IsAndroidToBeStayAwake
		{
			get
			{
				return _isAndroidToBeStayAwake;
			}

			set
			{
				_isAndroidToBeStayAwake = value;
			}
		}

		private bool _isFrontendToBeHiddenOnGamemanagerClose = true;
		[Browsable(true)]
		[Description("Default true")]
		[Category("Settings Form")]
		[DefaultValue(true)]
		[DisplayName("IsFrontendToBeHiddenOnGamemanagerClose")]
		public bool IsFrontendToBeHiddenOnGamemanagerClose
		{
			get
			{
				return _isFrontendToBeHiddenOnGamemanagerClose;
			}

			set
			{
				_isFrontendToBeHiddenOnGamemanagerClose = value;
			}
		}
		private bool _isUseFrontendBanner = false;
		[Browsable(true)]
		[Description("Default false")]
		[Category("Frontend")]
		[DefaultValue(false)]
		[DisplayName("IsUseFrontendBanner")]
		public bool IsUseFrontendBanner
		{
			get { return _isUseFrontendBanner; }
			set { _isUseFrontendBanner = value; }
		}

		private bool _isUseProgramFilesAsInstallDir;
		[Browsable(true)]
		[Description("For Oems Like IQT set it to true")]
		[Category("IQT")]
		[DefaultValue(false)]
		[DisplayName("IsUseProgramFilesAsInstallDir")]
		public bool IsUseProgramFilesAsInstallDir
		{
			get { return _isUseProgramFilesAsInstallDir; }
			set { _isUseProgramFilesAsInstallDir = value; }
		}

		private bool _isUsePcImeWorkflow = false;
		[Browsable(true)]
		[Description("Default false  \nVNG true")]
		[Category("VNG")]
		[DefaultValue(false)]
		[DisplayName("IsUsePcImeWorkflow")]
		public bool IsUsePcImeWorkflow
		{
			get { return _isUsePcImeWorkflow; }
			set { _isUsePcImeWorkflow = value; }
		}

		private bool _isLoadBluestacksLogoForAndroidTab = false;
		[Browsable(true)]
		[Description("Default false  \nChina true")]
		[Category("China")]
		[DefaultValue(false)]
		[DisplayName("IsLoadBluestacksLogoForAndroidTab")]
		public bool IsLoadBluestacksLogoForAndroidTab
		{
			get { return _isLoadBluestacksLogoForAndroidTab; }
			set { _isLoadBluestacksLogoForAndroidTab = value; }
		}

		private bool _isRestoreDataFailureUpgradeFailure = true;
		[Browsable(true)]
		[Description("During upgrade if restore of data fails, mark it as install failure")]
		[Category("GameManagerType installer True deploytool False")]
		[DefaultValue(false)]
		[DisplayName("IsRestoreDataFailureUpgradeFailure")]
		public bool IsRestoreDataFailureUpgradeFailure
		{
			get { return _isRestoreDataFailureUpgradeFailure; }
			set { _isRestoreDataFailureUpgradeFailure = value; }
		}


		private bool _isCreateControlPanelUninstallEntry = true;
		[Browsable(true)]
		[Description("Will create control panel uninstall entry")]
		[Category("GameManagerType installer True deploytool False")]
		[DefaultValue(false)]
		[DisplayName("IsCreateControlPanelUninstallEntry")]
		public bool IsCreateControlPanelUninstallEntry
		{
			get { return _isCreateControlPanelUninstallEntry; }
			set { _isCreateControlPanelUninstallEntry = value; }
		}

		private bool _isBTVBuild = false;
		[Browsable(true)]
		[Description("Default false  \nBTV true")]
		[Category("BTV True or False")]
		[DefaultValue(false)]
		[DisplayName("IsBTVBuild")]
		public bool IsBTVBuild
		{
			get { return _isBTVBuild; }
			set { _isBTVBuild = value; }
		}
		private bool _isSendBTVFunnelStats = false;
		[Browsable(true)]
		[Description("Default false  \nBTV true")]
		[Category("BTV")]
		[DefaultValue(false)]
		[DisplayName("IsSendBTVFunnelStats")]
		public bool IsSendBTVFunnelStats
		{
			get { return _isSendBTVFunnelStats; }
			set { _isSendBTVFunnelStats = value; }
		}
		private bool _isStreamWindowEnabled = false;
		[Browsable(true)]
		[Description("Default false  \nBTV true")]
		[Category("BTV")]
		[DefaultValue(false)]
		[DisplayName("IsStreamWindowEnabled")]
		public bool IsStreamWindowEnabled
		{
			get { return _isStreamWindowEnabled; }
			set { _isStreamWindowEnabled = value; }
		}
		private bool _isUnmuteOnFrontendActivated = true;
		[Browsable(true)]
		[Description("Default true  \nGamemanager false \nkizi false \nIQT false \nchina false")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(true)]
		[DisplayName("IsUnmuteOnFrontendActivated")]
		public bool IsUnmuteOnFrontendActivated
		{
			get { return _isUnmuteOnFrontendActivated; }
			set { _isUnmuteOnFrontendActivated = value; }
		}
		private bool _isShowUIDuringInstallationUninstallation = true;
		[Browsable(true)]
		[Description("Default true  \nGamemanager true \nkizi true \nIQT true \nchina true \n all deploytool partner false")]
		[Category("GameManagerType installer True deploytool False")]
		[DefaultValue(true)]
		[DisplayName("IsShowUIDuringInstallationUninstallation")]
		public bool IsShowUIDuringInstallationUninstallation
		{
			get { return _isShowUIDuringInstallationUninstallation; }
			set { _isShowUIDuringInstallationUninstallation = value; }
		}
		private bool _isDotnet4Point5Required = true;
		[Browsable(true)]
		[Description("Default true  \n all installers using gamemanager with BTV set to true \n all deploytool partners set to false")]
		[Category("GameManagerType installer True deploytool False")]
		[DefaultValue(true)]
		[DisplayName("IsDotnet4Point5Required")]
		public bool IsDotnet4Point5Required
		{
			get { return _isDotnet4Point5Required; }
			set { _isDotnet4Point5Required = value; }
		}
		private bool _isSendSysTrayNotificationOnFrontendClose = false;
		[Browsable(true)]
		[Description("Default false \n NetEase true")]
		[Category("NetEase True or False")]
		[DefaultValue(false)]
		[DisplayName("IsSendSysTrayNotificationOnFrontendClose")]
		public bool IsSendSysTrayNotificationOnFrontendClose
		{
			get { return _isSendSysTrayNotificationOnFrontendClose; }
			set { _isSendSysTrayNotificationOnFrontendClose = value; }
		}
		private bool _isFrontendFormLocation6 = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsFrontendFormLocation6")]
		public bool IsFrontendFormLocation6
		{
			get { return _isFrontendFormLocation6; }
			set { _isFrontendFormLocation6 = value; }
		}
		private bool _isFullScreenToggleEnabled = true;
		[Browsable(true)]
		[Description("Default true  \nGamemanager false \nkizi false \nIQT false \nchina false")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(true)]
		[DisplayName("IsFullScreenToggleEnabled")]
		public bool IsFullScreenToggleEnabled
		{
			get { return _isFullScreenToggleEnabled; }
			set { _isFullScreenToggleEnabled = value; }
		}
		private bool _isTagTextFilePresentForOem = true;
		[Browsable(true)]
		[Description("Default true  \nGamemanager false \nkizi false \nIQT false \nchina false")]
		[Category("GameManagerType installer false deploytool True")]
		[DefaultValue(true)]
		[DisplayName("IsTagTextFilePresentForOem")]
		public bool IsTagTextFilePresentForOem
		{
			get { return _isTagTextFilePresentForOem; }
			set { _isTagTextFilePresentForOem = value; }
		}
		private bool _isMessageBoxToBeDisplayed = true;
		[Browsable(true)]
		[Description("Default true  \nTencent false")]
		[Category("Tencent True or False")]
		[DefaultValue(true)]
		[DisplayName("IsMessageBoxToBeDisplayed")]
		public bool IsMessageBoxToBeDisplayed
		{
			get { return _isMessageBoxToBeDisplayed; }
			set { _isMessageBoxToBeDisplayed = value; }
		}
		private int _partnerControlBarHeight = 0;
		[Browsable(true)]
		[Description("Default 0 \nTencent 73")]
		[Category("Tencent int or 0")]
		[DefaultValue(0)]
		[DisplayName("PartnerControlBarHeight")]
		public int PartnerControlBarHeight
		{
			get { return _partnerControlBarHeight; }
			set { _partnerControlBarHeight = value; }
		}
		private bool _isStartFrontendCrashDebugging = false;
		[Browsable(true)]
		[Description("Default false \nTencent true")]
		[Category("Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsStartFrontendCrashDebugging")]
		public bool IsStartFrontendCrashDebugging
		{
			get { return _isStartFrontendCrashDebugging; }
			set { _isStartFrontendCrashDebugging = value; }
		}
		private bool _isResizeFrontendWindow = true;
		[Browsable(true)]
		[Description("Default true  \nGamemanager false \nkizi false \nIQT false \nchina false")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(true)]
		[DisplayName("IsResizeFrontendWindow")]
		public bool IsResizeFrontendWindow
		{
			get { return _isResizeFrontendWindow; }
			set { _isResizeFrontendWindow = value; }
		}
		private bool _isUseCustomResolutionIfLower = false;
		[Browsable(true)]
		[Description("Default false \nTencent true")]
		[Category("Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsUseCustomResolutionIfLower")]
		public bool IsUseCustomResolutionIfLower
		{
			get { return _isUseCustomResolutionIfLower; }
			set { _isUseCustomResolutionIfLower = value; }
		}
		private bool _isResolution900600 = false;
		[Browsable(true)]
		[Description("Default false \nNetEase2 true")]
		[Category("NetEase2 True or False")]
		[DefaultValue(false)]
		[DisplayName("IsResolution900600")]
		public bool IsResolution900600
		{
			get { return _isResolution900600; }
			set { _isResolution900600 = value; }
		}
		private bool _isWebTabPushNotificationEnabled = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsWebTabPushNotificationEnabled")]
		public bool IsWebTabPushNotificationEnabled
		{
			get { return _isWebTabPushNotificationEnabled; }
			set { _isWebTabPushNotificationEnabled = value; }
		}
		private bool _isAddMessagingSupport = false;
		[Browsable(true)]
		[Description("Default false \nTencent true")]
		[Category("Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsAddMessagingSupport")]
		public bool IsAddMessagingSupport
		{
			get { return _isAddMessagingSupport; }
			set { _isAddMessagingSupport = value; }
		}
		private bool _isFrontendBorderHidden = false;
		[Browsable(true)]
		[Description("Default false \nTencent true \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china, Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsFrontendBorderHidden")]
		public bool IsFrontendBorderHidden
		{
			get { return _isFrontendBorderHidden; }
			set { _isFrontendBorderHidden = value; }
		}
		private bool _isRemoveFromGameManagerJsonOnApkInstalled = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsRemoveFromGameManagerJsonOnApkInstalled")]
		public bool IsRemoveFromGameManagerJsonOnApkInstalled
		{
			get { return _isRemoveFromGameManagerJsonOnApkInstalled; }
			set { _isRemoveFromGameManagerJsonOnApkInstalled = value; }
		}
		private bool _isSendGameManagerRequestWhenActivityInfoDisplayedOnTopInHttpHandler = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsSendGameManagerRequestWhenActivityInfoDisplayedOnTopInHttpHandler")]
		public bool IsSendGameManagerRequestWhenActivityInfoDisplayedOnTopInHttpHandler
		{
			get { return _isSendGameManagerRequestWhenActivityInfoDisplayedOnTopInHttpHandler; }
			set { _isSendGameManagerRequestWhenActivityInfoDisplayedOnTopInHttpHandler = value; }
		}
		private bool _isGameManagerPropertiesToBeUsedWhileInstalling = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsGameManagerPropertiesToBeUsedWhileInstalling")]
		public bool IsGameManagerPropertiesToBeUsedWhileInstalling
		{
			get { return _isGameManagerPropertiesToBeUsedWhileInstalling; }
			set { _isGameManagerPropertiesToBeUsedWhileInstalling = value; }
		}

		private bool _isNotifyFrontendOrientationChangeToParentWindow = false;
		[Browsable(true)]
		[Description("Default false \nTencent true \nChina true")]
		[Category("Tencent China True or False")]
		[DefaultValue(false)]
		[DisplayName("IsNotifyFrontendOrientationChangeToParentWindow")]
		public bool IsNotifyFrontendOrientationChangeToParentWindow
		{
			get { return _isNotifyFrontendOrientationChangeToParentWindow; }
			set { _isNotifyFrontendOrientationChangeToParentWindow = value; }
		}

		private bool _isNotifyChangesToParentWindow = false;
		[Browsable(true)]
		[Description("Default false \nTencent true")]
		[Category("Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsNotifyChangesToParentWindow")]
		public bool IsNotifyChangesToParentWindow
		{
			get { return _isNotifyChangesToParentWindow; }
			set { _isNotifyChangesToParentWindow = value; }
		}
		private bool _isLookForPartnerFile = false;
		[Browsable(true)]
		[Description("Default false \nTencent true")]
		[Category("Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsLookForPartnerFile")]
		public bool IsLookForPartnerFile
		{
			get { return _isLookForPartnerFile; }
			set { _isLookForPartnerFile = value; }
		}
		private bool _isFrontendToBeHiddenOnRestart = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsFrontendToBeHiddenOnRestart")]
		public bool IsFrontendToBeHiddenOnRestart
		{
			get { return _isFrontendToBeHiddenOnRestart; }
			set { _isFrontendToBeHiddenOnRestart = value; }
		}
		private bool _isPackageToBeRemovedIfNotPresentOnInstallApk = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsPackageToBeRemovedIfNotPresentOnInstallApk")]
		public bool IsPackageToBeRemovedIfNotPresentOnInstallApk
		{
			get { return _isPackageToBeRemovedIfNotPresentOnInstallApk; }
			set { _isPackageToBeRemovedIfNotPresentOnInstallApk = value; }
		}
		private bool _isGMNotificationToBePosted = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsGMNotificationToBePosted")]
		public bool IsGMNotificationToBePosted
		{
			get { return _isGMNotificationToBePosted; }
			set { _isGMNotificationToBePosted = value; }
		}
		private bool _isAppInstalledEntryToBeMadeInJson = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsAppInstalledEntryToBeMadeInJson")]
		public bool IsAppInstalledEntryToBeMadeInJson
		{
			get { return _isAppInstalledEntryToBeMadeInJson; }
			set { _isAppInstalledEntryToBeMadeInJson = value; }
		}
		private bool _isOnlyStopButtonToBeAddedInContextMenuOFSysTray = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsOnlyStopButtonToBeAddedInContextMenuOFSysTray")]
		public bool IsOnlyStopButtonToBeAddedInContextMenuOFSysTray
		{
			get { return _isOnlyStopButtonToBeAddedInContextMenuOFSysTray; }
			set { _isOnlyStopButtonToBeAddedInContextMenuOFSysTray = value; }
		}
		private bool _isAddProblemReportMenuInSysTray = false;
		[Browsable(true)]
		[Description("Default false \n NetEase true")]
		[Category("NetEase True or False")]
		[DefaultValue(false)]
		[DisplayName("IsAddProblemReportMenuInSysTray")]
		public bool IsAddProblemReportMenuInSysTray
		{
			get { return _isAddProblemReportMenuInSysTray; }
			set { _isAddProblemReportMenuInSysTray = value; }
		}
		private bool _isWhiteBlueStackLogoToBeHiddenOnLoadingScreen = false;
		[Browsable(true)]
		[Description("Default false \n NetEase true")]
		[Category("NetEase True or False")]
		[DefaultValue(false)]
		[DisplayName("IsWhiteBlueStackLogoToBeHiddenOnLoadingScreen")]
		public bool IsWhiteBlueStackLogoToBeHiddenOnLoadingScreen
		{
			get { return _isWhiteBlueStackLogoToBeHiddenOnLoadingScreen; }
			set { _isWhiteBlueStackLogoToBeHiddenOnLoadingScreen = value; }
		}
		private bool _isCountryChina = false;
		[Browsable(true)]
		[Description("Default false  ==>true for china only")]
		[Category("China True or False")]
		[DefaultValue(false)]
		[DisplayName("IsCountryChina")]
		public bool IsCountryChina
		{
			get { return _isCountryChina; }
			set { _isCountryChina = value; }
		}
		private bool _isLoadCACodeFromCloud = true;
		[Browsable(true)]
		[Description("Default true  ==>false for china only")]
		[Category("China True or False")]
		[DefaultValue(true)]
		[DisplayName("IsLoadCACodeFromCloud")]
		public bool IsLoadCACodeFromCloud
		{
			get { return _isLoadCACodeFromCloud; }
			set { _isLoadCACodeFromCloud = value; }
		}
		private bool _isQuitingMultiInstanceFromGameManager = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsQuitingMultiInstanceFromGameManager")]
		public bool IsQuitingMultiInstanceFromGameManager
		{
			get { return _isQuitingMultiInstanceFromGameManager; }
			set { _isQuitingMultiInstanceFromGameManager = value; }
		}
		private bool _isStopBstUpdaterServiceOnQuitAll = false;
		[Browsable(true)]
		[Description("Default false \nTencent true")]
		[Category("Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsStopBstUpdaterServiceOnQuitAll")]
		public bool IsStopBstUpdaterServiceOnQuitAll
		{
			get { return _isStopBstUpdaterServiceOnQuitAll; }
			set { _isStopBstUpdaterServiceOnQuitAll = value; }
		}
		private bool _isForceKillHDAgentOnQuitAll = false;
		[Browsable(true)]
		[Description("Default false \nTencent true")]
		[Category("Tencent True or False")]
		[DefaultValue(false)]
		[DisplayName("IsForceKillHDAgentOnQuitAll")]
		public bool IsForceKillHDAgentOnQuitAll
		{
			get { return _isForceKillHDAgentOnQuitAll; }
			set { _isForceKillHDAgentOnQuitAll = value; }
		}
		private bool _isCreateApkHandlingRegistry = true;
		[Browsable(true)]
		[Description("Default true \nYY false")]
		[Category("YY True or False")]
		[DefaultValue(true)]
		[DisplayName("IsCreateApkHandlingRegistry")]
		public bool IsCreateApkHandlingRegistry
		{
			get { return _isCreateApkHandlingRegistry; }
			set { _isCreateApkHandlingRegistry = value; }
		}

		private bool _isWelcomeTabEnabled = false;
		[Browsable(true)]
		[Description("Default false \nExperiment true")]
		[Category("Experiment True or False")]
		[DefaultValue(false)]
		[DisplayName("IsWelcomeTabEnabled")]
		public bool IsWelcomeTabEnabled
		{
			get { return _isWelcomeTabEnabled; }
			set { _isWelcomeTabEnabled = value; }
		}

		private bool _isTabsEnabled = true;
		[Browsable(true)]
		[Description("Default true \nExperiment false")]
		[Category("Experiment True or False")]
		[DefaultValue(true)]
		[DisplayName("IsTabsEnabled")]
		public bool IsTabsEnabled
		{
			get { return _isTabsEnabled; }
			set { _isTabsEnabled = value; }
		}


		private bool _isPostUrlOnAppUninstalled = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsPostUrlOnAppUninstalled")]
		public bool IsPostUrlOnAppUninstalled
		{
			get { return _isPostUrlOnAppUninstalled; }
			set { _isPostUrlOnAppUninstalled = value; }
		}
		private bool _isSendGameManagerRequest = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsSendGameManagerRequest")]
		public bool IsSendGameManagerRequest
		{
			get { return _isSendGameManagerRequest; }
			set { _isSendGameManagerRequest = value; }
		}
		private bool _isFrontEndDPIAware = true;
		[Browsable(true)]
		[Description("Default true \nTencent false")]
		[Category("Tencent True or False")]
		[DefaultValue(true)]
		[DisplayName("IsFrontEndDPIAware")]
		public bool IsFrontEndDPIAware
		{
			get { return _isFrontEndDPIAware; }
			set { _isFrontEndDPIAware = value; }
		}
		private bool _isShowBTVViewTab = false;
		[Browsable(true)]
		[Description("Default false")]
		[Category("BTV True or False")]
		[DefaultValue(false)]
		[DisplayName("IsShowBTVViewTab")]
		public bool IsShowBTVViewTab
		{
			get { return _isShowBTVViewTab; }
			set { _isShowBTVViewTab = value; }
		}
		private bool _isHideMessageBoxIconInTaskBar = false; // Hides the bluestacks icon in taskbar for china release as they dont want to see bluestacks icon
		[Browsable(true)]
		[Description("Default false \nNetEase true \n4399 true")]
		[Category("4399, NetEase True or False")]
		[DefaultValue(false)]
		[DisplayName("IsHideMessageBoxIconInTaskBar")]
		public bool IsHideMessageBoxIconInTaskBar
		{
			get { return _isHideMessageBoxIconInTaskBar; }
			set { _isHideMessageBoxIconInTaskBar = value; }
		}
		private bool _isJsonModelToBePushed = false;
		[Browsable(true)]
		[Description("Default false  \n4399 true")]
		[Category("4399 True or False")]
		[DefaultValue(false)]
		[DisplayName("IsJsonModelToBePushed")]
		public bool IsJsonModelToBePushed
		{
			get { return _isJsonModelToBePushed; }
			set { _isJsonModelToBePushed = value; }
		}
		private bool _isLefClickOnTrayIconLaunchFrontend = false;
		[Browsable(true)]
		[Description("Default false \n NetEase true")]
		[Category("NetEase True or False")]
		[DefaultValue(false)]
		[DisplayName("IsLefClickOnTrayIconLaunchFrontend")]
		public bool IsLefClickOnTrayIconLaunchFrontend
		{
			get { return _isLefClickOnTrayIconLaunchFrontend; }
			set { _isLefClickOnTrayIconLaunchFrontend = value; }
		}
		private bool _isLefClickOnTrayIconLaunchPartner = false;
		[Browsable(true)]
		[Description("Default false \n BCGP true")]
		[Category("BCGP True or False")]
		[DefaultValue(false)]
		[DisplayName("IsLefClickOnTrayIconLaunchPartner")]
		public bool IsLefClickOnTrayIconLaunchPartner
		{
			get { return _isLefClickOnTrayIconLaunchPartner; }
			set { _isLefClickOnTrayIconLaunchPartner = value; }
		}


		private bool _isGameManagerToBeStartedOnRunApp = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsGameManagerToBeStartedOnRunApp")]
		public bool IsGameManagerToBeStartedOnRunApp
		{
			get { return _isGameManagerToBeStartedOnRunApp; }
			set { _isGameManagerToBeStartedOnRunApp = value; }
		}
		private bool _isLibraryShortcutToBeCreated = false;
		[Browsable(true)]
		[Description("Default false  \nEnterprise true ")]
		[Category("Enterprise: True or False")]
		[DefaultValue(false)]
		[DisplayName("IsLibraryShortcutToBeCreated")]
		public bool IsLibraryShortcutToBeCreated
		{
			get { return _isLibraryShortcutToBeCreated; }
			set { _isLibraryShortcutToBeCreated = value; }
		}
		private bool _isSendInstallationStats = true;
		[Browsable(true)]
		[Description("Default true \n for tc_dt make false")]
		[Category("Send Install Stats")]
		[DefaultValue(false)]
		[DisplayName("IsSendInstallationStats")]
		public bool IsSendInstallationStats
		{
			get { return _isSendInstallationStats; }
			set { _isSendInstallationStats = value; }
		}
		private bool _isLoadDefaultCountryCode = false;
		[Browsable(true)]
		[Description("Default false  \nEnterprise true ")]
		[Category("Enterprise: True or False")]
		[DefaultValue(false)]
		[DisplayName("IsLoadDefaultCountryCode")]
		public bool IsLoadDefaultCountryCode
		{
			get { return _isLoadDefaultCountryCode; }
			set { _isLoadDefaultCountryCode = value; }
		}
		private bool _isDisplayMessageAfterPreInstallCheck = false;
		[Browsable(true)]
		[Description("Default false  \nEnterprise true ")]
		[Category("Enterprise: True or False")]
		[DefaultValue(false)]
		[DisplayName("IsDisplayMessageAfterPreInstallCheck")]
		public bool IsDisplayMessageAfterPreInstallCheck
		{
			get { return _isDisplayMessageAfterPreInstallCheck; }
			set { _isDisplayMessageAfterPreInstallCheck = value; }
		}
		private bool _isSetCustomStreamWindowWidth = false;
		[Browsable(true)]
		[Description("Default false \nBTV true")]
		[Category("BTV True or False")]
		[DefaultValue(false)]
		[DisplayName("IsSetCustomStreamWindowWidth")]
		public bool IsSetCustomStreamWindowWidth
		{
			get { return _isSetCustomStreamWindowWidth; }
			set { _isSetCustomStreamWindowWidth = value; }
		}
		private bool _isAppPlayerToBeLaunchedAfterInstallation = true;
		[Browsable(true)]
		[Description("Default true  \nEnterprise false ")]
		[Category("Enterprise: True or False")]
		[DefaultValue(true)]
		[DisplayName("IsAppPlayerToBeLaunchedAfterInstallation")]
		public bool IsAppPlayerToBeLaunchedAfterInstallation
		{
			get { return _isAppPlayerToBeLaunchedAfterInstallation; }
			set { _isAppPlayerToBeLaunchedAfterInstallation = value; }
		}
		private bool _isUninstallationPromptEnabled = false;
		[Browsable(true)]
		[Description("Default false  \nEnterprise true ")]
		[Category("Enterprise: True or False")]
		[DefaultValue(false)]
		[DisplayName("IsUninstallationPromptEnabled")]
		public bool IsUninstallationPromptEnabled
		{
			get { return _isUninstallationPromptEnabled; }
			set { _isUninstallationPromptEnabled = value; }
		}
		private bool _isLocalAppDataToBeCleanedUp = false;
		[Browsable(true)]
		[Description("Default false  \nEnterprise true ")]
		[Category("Enterprise: True or False")]
		[DefaultValue(false)]
		[DisplayName("IsLocalAppDataToBeCleanedUp")]
		public bool IsLocalAppDataToBeCleanedUp
		{
			get { return _isLocalAppDataToBeCleanedUp; }
			set { _isLocalAppDataToBeCleanedUp = value; }
		}
		private bool _isRestoreAndSaveAndroidData = false;
		[Browsable(true)]
		[Description("Default false  \nEnterprise true ")]
		[Category("Enterprise: True or False")]
		[DefaultValue(false)]
		[DisplayName("IsRestoreAndSaveAndroidData")]
		public bool IsRestoreAndSaveAndroidData
		{
			get { return _isRestoreAndSaveAndroidData; }
			set { _isRestoreAndSaveAndroidData = value; }
		}
		private bool _isCustomResolutionToBeSet = true;
		[Browsable(true)]
		[Description("Default true \nAnquiCafe false")]
		[Category("Anqui Cafe True or False")]
		[DefaultValue(true)]
		[DisplayName("IsCustomResolutionToBeSet")]
		public bool IsCustomResolutionToBeSet
		{
			get { return _isCustomResolutionToBeSet; }
			set { _isCustomResolutionToBeSet = value; }
		}
		private bool _isOemWithGameManagerData = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("GameManagerType installer True deploytool False")]
		[DefaultValue(false)]
		[DisplayName("IsOemWithGameManagerData")]
		public bool IsOemWithGameManagerData
		{
			get { return _isOemWithGameManagerData; }
			set { _isOemWithGameManagerData = value; }
		}
		private bool _isAddGoLiveButton = false;
		[Browsable(true)]
		[Description("Default false  make true for gamemanager with btv")]
		[Category("BTV True or False")]
		[DefaultValue(false)]
		[DisplayName("IsAddGoLiveButton")]
		public bool IsAddGoLiveButton
		{
			get { return _isAddGoLiveButton; }
			set { _isAddGoLiveButton = value; }
		}
		private bool _isAddChatButton = false;
		[Browsable(true)]
		[Description("Default false make true for gamemanager with chat")]
		[Category("Chat True or False")]
		[DefaultValue(false)]
		[DisplayName("IsAddChatButton")]
		public bool IsAddChatButton
		{
			get { return _isAddChatButton; }
			set { _isAddChatButton = value; }
		}
		private bool _isMinimizeOnEscapeIfFullscreen = false;
		[Browsable(true)]
		[Description("Default false  \nTC true")]
		[Category("TC: true or false")]
		[DefaultValue(false)]
		[DisplayName("IsMinimizeOnEscapeIfFullscreen")]
		public bool IsMinimizeOnEscapeIfFullscreen
		{
			get { return _isMinimizeOnEscapeIfFullscreen; }
			set { _isMinimizeOnEscapeIfFullscreen = value; }
		}
		private bool _isGameManagerToBeRestartedOnBlackScreen = false;
		[Browsable(true)]
		[Description("Default false  \nGamemanager true \nkizi true \nIQT true \nchina true")]
		[Category("Gamemanager, kizi, IQT, china True or False")]
		[DefaultValue(false)]
		[DisplayName("IsGameManagerToBeRestartedOnBlackScreen")]
		public bool IsGameManagerToBeRestartedOnBlackScreen
		{
			get { return _isGameManagerToBeRestartedOnBlackScreen; }
			set { _isGameManagerToBeRestartedOnBlackScreen = value; }
		}
		#endregion
		#region string properties
		private string _uninstallerExeName = "BluestacksUninstaller.exe";
		[Browsable(true)]
		[Description("Bluestacks Uninstaller exe Name")]
		[Category("Uninstaller Name")]
		[DefaultValue("BluestacksUninstaller.exe")]
		[DisplayName("Bluestacks uninstaller exe Name")]
		public string UninstallerExeName
		{
			get { return _uninstallerExeName; }
			set { _uninstallerExeName = value; }
		}

		private string _oem = "gamemanager";
		[Browsable(true)]
		[Description("OEM name of the release")]
		[Category("0 Properties For All")]
		[DefaultValue("gamemanager")]
		[DisplayName("Oem Name")]
		public String OEM
		{
			get { return _oem; }
			set { _oem = value; }
		}
		private string _dnsValue = "8.8.8.8";
		[Browsable(true)]
		[Description("Default value \"8.8.8.8\" \nFor china use \"114.114.114.114\"")]
		[Category("China Oems")]
		[DefaultValue("8.8.8.8")]
		[DisplayName("DNS Value")]
		public string DNSValue
		{
			get { return _dnsValue; }
			set { _dnsValue = value; }
		}
		private string _openWithApkHandlerText = "Open with BlueStacks Apk Installer";
		[Browsable(true)]
		[Description("Default value \"Open with BlueStacks Apk Installe\" \nFor NetEase use \"使用网易安卓模拟器打开\" \nFor 4399 use \"Open with 4399 Apk Installer\"")]
		[Category("4399, NetEase Text Properties")]
		[DefaultValue("Open with BlueStacks Apk Installer")]
		[DisplayName("OpenWithApkHandlerText")]
		public string OpenWithApkHandlerText
		{
			get { return _openWithApkHandlerText; }
			set { _openWithApkHandlerText = value; }
		}
		private string _defaultTitle = "DefaultTitle";
		[Browsable(true)]
		[Description("\nDefault value \"DefaultTitle\" \nFor 4399 use \"4399手游通\" \nFor NetEase \"网易安卓模拟器\"")]
		[Category("4399, NetEase Text Properties")]
		[DefaultValue("DefaultTitle")]
		[DisplayName("DefaultTitle")]
		public string DefaultTitle
		{
			get
			{
				if (string.IsNullOrEmpty(_defaultTitle) || _defaultTitle.Equals("DefaultTitle"))
				{
					_defaultTitle = Locale.Strings.DefaultTitle;
				}
				return _defaultTitle;
			}
			set { _defaultTitle = value; }
		}
		private string _desktopShortcutFileName = "";
		[Browsable(true)]
		[Description("\nDefault value \"BlueStacks.lnk\" \nFor Kizi \"Kizi Player Powered By BlueStacks.lnk\"  \nFor 4399 \"4399手游通.lnk\" \nFor netease \"网易安卓模拟器.lnk\"  \nFor enterprise \"Start BlueStacks.lnk\"")]
		[Category("DesktopShortcut")]
		[DisplayName("DesktopShortcutFileName")]
		public string DesktopShortcutFileName
		{
			get { return _desktopShortcutFileName; }
			set { _desktopShortcutFileName = value; }

		}
		private string _blueStacksApkHandlerTitle = "BlueStacksApkHandlerTitle";
		[Browsable(true)]
		[Description("\nDefault value \"BlueStacksApkHandlerTitle\" \nFor 4399 \"4399手游通\" \nFor NetEase \"网易安卓模拟器\"")]
		[Category("4399, NetEase Text Properties")]
		[DefaultValue("BlueStacksApkHandlerTitle")]
		[DisplayName("BlueStacksApkHandlerTitle")]
		public string BlueStacksApkHandlerTitle
		{
			get
			{
				if (string.IsNullOrEmpty(_blueStacksApkHandlerTitle) || _blueStacksApkHandlerTitle.Equals("BlueStacksApkHandlerTitle"))
				{
					_blueStacksApkHandlerTitle = Locale.Strings.BlueStacksApkHandlerTitle;
				}
				return _blueStacksApkHandlerTitle;
			}
			set { _blueStacksApkHandlerTitle = value; }
		}
		private string _commonAppTitleText = "CommonAppTitleText";
		[Browsable(true)]
		[Description("\nDefault value \"CommonAppTitleText\" \nFor GameManager \"BlueStacks Android Plugin\" \nFor 4399 \"4399手游通 Powered by BlueStacks\" \nFor NetEase \"网易安卓模拟器\"")]
		[Category("4399, NetEase, Gamemanager, IQT, Kizi, china, Text Properties")]
		[DefaultValue("CommonAppTitleText")]
		[DisplayName("CommonAppTitleText")]
		public string CommonAppTitleText
		{
			get
			{
				if (string.IsNullOrEmpty(_commonAppTitleText) || _commonAppTitleText.Equals("CommonAppTitleText"))
				{
					_commonAppTitleText = Locale.Strings.CommonAppTitleText;
				}
				return _commonAppTitleText;
			}
			set { _commonAppTitleText = value; }
		}
		private string _snapShotShareString = "SnapShotShareString";
		[Browsable(true)]
		[Description("\nDefault value \"SnapShotShareString\" \nFor 4399 \"分享自4399手游通( www.4399.cn/app-emu.html )\"")]
		[Category("4399 Text Properties")]
		[DefaultValue("SnapShotShareString")]
		[DisplayName("SnapShotShareString")]
		public string SnapShotShareString
		{
			get
			{
				if (string.IsNullOrEmpty(_snapShotShareString) || _snapShotShareString.Equals("SnapShotShareString"))
				{
					_snapShotShareString = Locale.Strings.SnapShotShareString;
				}
				return _snapShotShareString;
			}
			set { _snapShotShareString = value; }
		}
		private string _updaterMessageBoxText = "UPDATER_UTILITY_ASK_TO_INSTALL_TEXT";
		[Browsable(true)]
		[Description("\nDefault value \"UPDATER_UTILITY_ASK_TO_INSTALL_TEXT\" \nFor 4399 use \"您需要升级到4399手游通的最新版本吗？\"")]
		[Category("4399 Text Properties")]
		[DefaultValue("UPDATER_UTILITY_ASK_TO_INSTALL_TEXT")]
		[DisplayName("UpdaterMessageBoxText")]
		public string UpdaterMessageBoxText
		{
			get
			{
				if (string.IsNullOrEmpty(_updaterMessageBoxText) || _updaterMessageBoxText.Equals("UPDATER_UTILITY_ASK_TO_INSTALL_TEXT"))
				{
					_updaterMessageBoxText = Locale.Strings.UPDATER_UTILITY_ASK_TO_INSTALL_TEXT;
				}
				return _updaterMessageBoxText;
			}
			set { _updaterMessageBoxText = value; }
		}
		private string _blueStacksRestartUtilityRestartingText = "RESTART_UTILITY_RESTARTING_TEXT";
		[Browsable(true)]
		[Description("\nDefault value \"RESTART_UTILITY_RESTARTING_TEXT\" \nFor 4399 \"重新启动4399手游通\" \nFor NetEase \"网易安卓模拟器\"")]
		[Category("4399, NetEase Text Properties")]
		[DefaultValue("RESTART_UTILITY_RESTARTING_TEXT")]
		[DisplayName("BlueStacksRestartUtilityRestartingText")]
		public string BlueStacksRestartUtilityRestartingText
		{
			get
			{
				if (string.IsNullOrEmpty(_blueStacksRestartUtilityRestartingText) || _blueStacksRestartUtilityRestartingText.Equals("RESTART_UTILITY_RESTARTING_TEXT"))
				{
					_blueStacksRestartUtilityRestartingText = Locale.Strings.RESTART_UTILITY_RESTARTING_TEXT;
				}
				return _blueStacksRestartUtilityRestartingText;
			}
			set { _blueStacksRestartUtilityRestartingText = value; }
		}
		private string _blueStacksUpdaterTitle = "UPDATER_UTILITY_NO_UPDATE_TITLE";
		[Browsable(true)]
		[Description("\nDefault value \"UPDATER_UTILITY_NO_UPDATE_TITLE\" \nFor 4399 \"4399手游通\" \nFor NetEase \"网易安卓模拟器\"")]
		[Category("4399, NetEase Text Properties")]
		[DefaultValue("UPDATER_UTILITY_NO_UPDATE_TITLE")]
		[DisplayName("BlueStacksUpdaterTitle")]
		public string BlueStacksUpdaterTitle
		{
			get
			{
				if (string.IsNullOrEmpty(_blueStacksUpdaterTitle) || _blueStacksUpdaterTitle.Equals("UPDATER_UTILITY_NO_UPDATE_TITLE"))
				{
					_blueStacksUpdaterTitle = Locale.Strings.UPDATER_UTILITY_NO_UPDATE_TITLE;
				}
				return _blueStacksUpdaterTitle;
			}
			set { _blueStacksUpdaterTitle = value; }
		}
		private string _bluestacksLogCollectorText = "FORM_TEXT";
		[Browsable(true)]
		[Description("\nDefault value \"FORM_TEXT\" \nFor 4399 \"4399手游通支持工具\" \nFor NetEase \"网易安卓模拟器\"")]
		[Category("4399, NetEase Text Properties")]
		[DefaultValue("FORM_TEXT")]
		[DisplayName("BluestacksLogCollectorText")]
		public string BluestacksLogCollectorText
		{
			get
			{
				if (string.IsNullOrEmpty(_bluestacksLogCollectorText) || _bluestacksLogCollectorText.Equals("FORM_TEXT"))
				{
					_bluestacksLogCollectorText = Locale.Strings.FORM_TEXT;
				}
				return _bluestacksLogCollectorText;
			}
			set { _bluestacksLogCollectorText = value; }
		}
		private string _loadingScreenAppTitle = "LoadingScreenAppTitle";
		[Browsable(true)]
		[Description("\nDefault value \"LoadingScreenAppTitle\" \nFor 4399 \"DynamicText;4399初始化需要一些时间，如等待时间过长，可重启手游通;4399用户反馈QQ群：485313402;4399兼容99%的安卓游戏;4399使用键盘映射  轻松超神;\" \nFor NetEase \"网易安卓模拟器\"")]
		[Category("4399, NetEase Text Properties")]
		[DefaultValue("LoadingScreenAppTitle")]
		[DisplayName("LoadingScreenAppTitle")]
		public string LoadingScreenAppTitle
		{
			get
			{
				if (string.IsNullOrEmpty(_loadingScreenAppTitle) || _loadingScreenAppTitle.Equals("LoadingScreenAppTitle"))
				{
					_loadingScreenAppTitle = Locale.Strings.LoadingScreenAppTitle;
				}
				return _loadingScreenAppTitle;
			}
			set { _loadingScreenAppTitle = value; }
		}
		private string _homeTabUrl = "default";
		[Browsable(true)]
		[Description("Home tab url of BlueStacks 2 \nDefault value \"default\" \nFor China use  \"http://magazine.bluestacks.cn/\" \nFor kizi use  \"http://kizi.com/bluestacks\"")]
		[Category("Kizi Text Properties")]
		[DefaultValue("default")]
		[DisplayName("HomeTabUrl1")]
		public string HomeTabUrl1
		{
			get { return _homeTabUrl; }
			set { _homeTabUrl = value; }
		}
		[Browsable(false)]
		public string HomeTabUrl
		{
			get
			{
				if (string.IsNullOrEmpty(_homeTabUrl) || _homeTabUrl.Equals("default"))
				{
					return String.Format("{0}?lang={1}", "http://bluestacks-magazine.appspot.com/home", System.Globalization.CultureInfo.CurrentCulture.Name);
				}
				return _homeTabUrl;
			}
		}
		private string _customDPI = string.Empty;
		[Browsable(true)]
		[Description("\nDefault value \"\" \nFor 4399 \"240\"")]
		[Category("4399 Text Properties")]
		[DefaultValue("")]
		[DisplayName("CustomDPI")]
		public string CustomDPI
		{
			get { return _customDPI; }
			set { _customDPI = value; }
		}
		#endregion
	}
}
