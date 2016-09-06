/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * This file contains wrapper functions for getting localized strings.
 */
using System;
using System.IO;
using System.Threading;
using Microsoft.Win32;
using System.Globalization;
using System.Collections.Generic;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Locale
{
	public class Strings
	{
		public static string sLocale;
		public static string sResourceLocation;
		public static Dictionary<string, string> sLocalizedString = null;

		public static Dictionary<string, string> InitLocalization(string localeDir)
		{
			if (localeDir == null)
			{
				sResourceLocation = Path.Combine(Common.Strings.BstCommonAppData, @"Locales");
			}
			else
			{
				sResourceLocation = localeDir;
			}

			sLocalizedString = new Dictionary<string, string>();
			CultureInfo ci = Thread.CurrentThread.CurrentCulture;
			sLocale = ci.Name;

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			if (key != null)
			{
				sLocale = (string)key.GetValue("Locale", sLocale).ToString();
			}
			if (PopulateLocaleStrings("en-US"))
				Logger.Info("Successfully populated English strings");
			if ((string.Compare(sLocale, "en-US") != 0) && PopulateLocaleStrings(sLocale))
				Logger.Info("Successfully populated localized strings for locale: " + sLocale);
			return sLocalizedString;
		}

		private static bool PopulateLocaleStrings(string locale)
		{
			try
			{
				string fileName = String.Format("i18n.{0}.txt", locale);
				string filePath = Path.Combine(sResourceLocation, fileName);
				Logger.Info("localized strings file path: " + filePath);
				if (!File.Exists(filePath))
				{
					Logger.Info(string.Format("File does not exist: {0}", filePath));
					return false;
				}

				FillDictionary(filePath, sLocalizedString);

				return true;
			}
			catch (Exception e)
			{
				Logger.Error("Could not populate localizes strings. Error: " + e.ToString());
				return false;
			}
		}

		private static void FillDictionary(string filePath, Dictionary<string, string> dict)
		{
			try
			{
				string[] fileLines = File.ReadAllLines(filePath);
				string[] keyValue;
				foreach (string line in fileLines)
				{
					if (line.IndexOf("=") == -1)
						continue;
					keyValue = line.Split('=');
					dict[keyValue[0].Trim()] = keyValue[1].Trim();
				}
			}
			catch (Exception e)
			{
				throw;
			}
		}

		public static string GetLocalizedString(
				string id
				)
		{
			string str = id;
			try
			{
				if (sLocalizedString == null)
				{
					InitLocalization(null);
				}
				str = sLocalizedString[id];
			}
			catch (Exception)
			{
				Logger.Warning("Localized string not available for: " + id);
			}
			return str;
		}

		public static ProblemCategory GetLocalizationForProblemCategory()
		{
			ProblemCategory problemCategory = new ProblemCategory();
			CultureInfo ci = Thread.CurrentThread.CurrentCulture;
			string locale = ci.Name;

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			if (key != null)
			{
				locale = (string)key.GetValue("Locale", locale).ToString();
			}
			if (PopulateLocaleProblemCategories("en-US", problemCategory))
				Logger.Info("Successfully populated English strings for Problem Categories");
			if ((string.Compare(locale, "en-US") != 0) && PopulateLocaleProblemCategories(locale, problemCategory))
				Logger.Info("Successfully populated localized strings for Problem Categories for locale: " + locale);
			return problemCategory;
		}

		private static bool PopulateLocaleProblemCategories(string locale, ProblemCategory problemCategory)
		{
			try
			{
				Logger.Info("In Method PopulateLocaleProblemCategories");
				string problemCategoryPath = Path.Combine(Common.Strings.BstCommonAppData, @"Locales\ProblemCategories");
				string fileName = String.Format("ReportProblemCategories.{0}.Json", locale);
				string filePath = Path.Combine(problemCategoryPath, fileName);
				Logger.Info("localized strings file path for Problem Categories: " + filePath);
				if (!File.Exists(filePath))
				{
					Logger.Info(string.Format("File does not exist for Problem Categories: {0}", filePath));
					return false;
				}

				string jsonString = File.ReadAllText(filePath);
				if (string.IsNullOrEmpty(jsonString))
				{
					Logger.Info("Invalid json");
					return false;
				}
				Logger.Info("Found Json: " + jsonString);
				JSonReader json = new JSonReader();
				IJSonObject input = json.ReadAsJSonObject(jsonString);
				int noOfCategories = 0;
				foreach (KeyValuePair<string, IJSonObject> categories in input.ObjectItems)
				{
					string key = categories.Key;
					noOfCategories = categories.Value.Length;
					if (noOfCategories == 0)
					{
						Logger.Info("No Categories found in Json");
						return false;
					}
					else
					{
						problemCategory.category = new List<Category>();
					}
					for (int i = 0; i < categories.Value.Length; i++)
					{
						IJSonObject objCategory = categories.Value[i];
						Category category = new Category();
						category.categoryId = objCategory["id"].StringValue;
						category.categoryValue = objCategory["value"].StringValue;
						category.showdropdown = objCategory["showdropdown"].StringValue;
						category.subcategory = new List<Subcategory>();
						if (objCategory.Contains("subcategory"))
						{
							IJSonObject subCatList = objCategory["subcategory"];
							for (int countSubCategory = 0; countSubCategory < subCatList.Length; countSubCategory++)
							{
								IJSonObject subCatObj = subCatList[countSubCategory];
								Subcategory subcategory = new Subcategory();
								subcategory.subcategoryId = subCatObj["id"].StringValue;
								subcategory.subcategoryValue = subCatObj["value"].StringValue;
								subcategory.showdropdown = subCatObj["showdropdown"].StringValue;
								category.subcategory.Add(subcategory);
							}
						}
						else
						{
							Logger.Info("No Subcategories found in Category: " + category.categoryId);
						}
						problemCategory.category.Add(category);
					}
				}

				//FillDictionary(filePath, sLocalizedString);

				return true;
			}
			catch (Exception e)
			{
				Logger.Error("Could not populate localizes strings for Problem Categories. Error: " + e.ToString());
				return false;
			}
		}

		// List of strings to be localized
		public static string RestoreFactorySettings
		{
			get { return GetLocalizedString("RestoreFactorySettings"); }
		}

		public static string StartBlueStacks
		{
			get { return GetLocalizedString("StartBlueStacks"); }
		}

		public static string RestartBlueStacks
		{
			get { return GetLocalizedString("RestartBlueStacks"); }
		}

		public static string QuitBlueStacks
		{
			get { return GetLocalizedString("QuitBlueStacks"); }
		}

		public static string StopBlueStacks
		{
			get { return GetLocalizedString("StopBlueStacks"); }
		}

		public static string RotatePortraitApps
		{
			get { return GetLocalizedString("RotatePortraitApps"); }
		}

		public static string UploadDebugLogs
		{
			get { return GetLocalizedString("UploadDebugLogs"); }
		}

		public static string FreeDuringBeta
		{
			get { return GetLocalizedString("FreeDuringBeta"); }
		}

		public static string SMSSetupMenu
		{
			get { return GetLocalizedString("SMSSetupMenu"); }
		}

		public static string PauseSync
		{
			get { return GetLocalizedString("PauseSync"); }
		}

		public static string ResumeSync
		{
			get { return GetLocalizedString("ResumeSync"); }
		}

		public static string CheckForUpdates
		{
			get { return GetLocalizedString("CheckForUpdates"); }
		}

		public static string DownloadingUpdates
		{
			get { return GetLocalizedString("DownloadingUpdates"); }
		}

		public static string InstallUpdates
		{
			get { return GetLocalizedString("InstallUpdates"); }
		}

		public static string Apps
		{
			get { return GetLocalizedString("Apps"); }
		}

		public static string App
		{
			get { return GetLocalizedString("App"); }
		}

		public static string Installed
		{
			get { return GetLocalizedString("Installed"); }
		}

		public static string DiskUsageUnavailable
		{
			get { return GetLocalizedString("DiskUsageUnavailable"); }
		}

		public static string Of
		{
			get { return GetLocalizedString("Of"); }
		}

		public static string DiskUsed
		{
			get { return GetLocalizedString("DiskUsed"); }
		}

		public static string BackButtonToolTip
		{
			get { return GetLocalizedString("BackButtonToolTip"); }
		}

		public static string MenuButtonToolTip
		{
			get { return GetLocalizedString("MenuButtonToolTip"); }
		}

		public static string CloseButtonToolTip
		{
			get { return GetLocalizedString("CloseButtonToolTip"); }
		}

		public static string FullScreenButtonToolTip
		{
			get { return GetLocalizedString("FullScreenButtonToolTip"); }
		}

		public static string ZoomOutButtonToolTip
		{
			get { return GetLocalizedString("ZoomOutButtonToolTip"); }
		}

		public static string ZoomInButtonToolTip
		{
			get { return GetLocalizedString("ZoomInButtonToolTip"); }
		}

		public static string SettingsButtonToolTip
		{
			get { return GetLocalizedString("SettingsButtonToolTip"); }
		}

		public static string HomeButtonToolTip
		{
			get { return GetLocalizedString("HomeButtonToolTip"); }
		}

		public static string ShareButtonToolTip
		{
			get { return GetLocalizedString("ShareButtonToolTip"); }
		}

		public static string ResizeMessageBoxCaption
		{
			get { return GetLocalizedString("ResizeMessageBoxCaption"); }
		}

		public static string ResizeMessageBoxText
		{
			get { return GetLocalizedString("ResizeMessageBoxText"); }
		}

		public static string Initializing
		{
			get { return GetLocalizedString("Initializing"); }
		}

		public static string InitializingGame
		{
			get { return GetLocalizedString("InitializingGame"); }
		}

		public static string DownloadingGameData
		{
			get { return GetLocalizedString("DownloadingGameData"); }
		}

		public static string CanNotStart
		{
			get { return GetLocalizedString("CanNotStart"); }
		}

		public static string NetworkAvailableIconText
		{
			get { return GetLocalizedString("NetworkAvailableIconText"); }
		}
		public static string NetworkUnavailableIconText
		{
			get { return GetLocalizedString("NetworkUnavailableIconText"); }
		}

		public static string PostToWallLink
		{
			get { return GetLocalizedString("PostToWallLink"); }
		}

		public static string PostToWallPicture
		{
			get { return GetLocalizedString("PostToWallPicture"); }
		}

		public static string PostToWallName
		{
			get { return GetLocalizedString("PostToWallName"); }
		}

		public static string PostToWallCaption
		{
			get { return GetLocalizedString("PostToWallCaption"); }
		}

		public static string PostToWallDescription
		{
			get { return GetLocalizedString("PostToWallDescription"); }
		}

		public static string FacebookWindowTitle
		{
			get { return GetLocalizedString("FacebookWindowTitle"); }
		}

		public static string FacebookLoginWindowText
		{
			get { return GetLocalizedString("FacebookLoginWindowText"); }
		}

		public static string PostToFacebookWindowText
		{
			get { return GetLocalizedString("PostToFacebookWindowText"); }
		}

		public static string NoInternetDuringFBConnect
		{
			get { return GetLocalizedString("NoInternetDuringFBConnect"); }
		}

		public static string BalloonTitle
		{
			get { return GetLocalizedString("BalloonTitle"); }
		}

		public static string MessageBoxTitle
		{
			get { return GetLocalizedString("MessageBoxTitle"); }
		}

		public static string MessageBoxText
		{
			get { return GetLocalizedString("MessageBoxText"); }
		}

		public static string OKButtonText
		{
			get { return GetLocalizedString("OKButtonText"); }
		}

		public static string CancelButtonText
		{
			get { return GetLocalizedString("CancelButtonText"); }
		}

		public static string UninstallWindowTitle
		{
			get { return GetLocalizedString("UninstallWindowTitle"); }
		}

		public static string InstallSuccess
		{
			get { return GetLocalizedString("InstallSuccess"); }
		}

		public static string UninstallSuccess
		{
			get { return GetLocalizedString("UninstallSuccess"); }
		}

		public static string UninstallFailed
		{
			get { return GetLocalizedString("UninstallFailed"); }
		}

		public static string UninstallingWait
		{
			get { return GetLocalizedString("UninstallingWait"); }
		}

		public static string GpsWindowTitle
		{
			get { return GetLocalizedString("GpsWindowTitle"); }
		}

		public static string CloudConnectTitle
		{
			get { return GetLocalizedString("CloudConnectTitle"); }
		}

		public static string CloudConnectedMsg
		{
			get { return GetLocalizedString("CloudConnectedMsg"); }
		}

		public static string CloudDisconnectedMsg
		{
			get { return GetLocalizedString("CloudDisconnectedMsg"); }
		}

		public static string InsufficientStorageMessage
		{
			get { return GetLocalizedString("InsufficientStorageMessage"); }
		}

		public static string InstallFail
		{
			get { return GetLocalizedString("InstallFail"); }
		}

		public static string UserWaitText
		{
			get { return GetLocalizedString("UserWaitText"); }
		}
		public static string FullScreenToastText
		{
			get { return GetLocalizedString("FullScreenToastText"); }
		}
		public static string SnapshotErrorToastText
		{
			get { return GetLocalizedString("SnapshotErrorToastText"); }
		}
		public static string GraphicsDriverOutdatedError
		{
			get { return GetLocalizedString("GraphicsDriverOutdatedError"); }
		}
		public static string GraphicsDriverOutdatedWarning
		{
			get { return GetLocalizedString("GraphicsDriverOutdatedWarning"); }
		}
		public static string GraphicsDriverUpdatedMessage
		{
			get { return GetLocalizedString("GraphicsDriverUpdatedMessage"); }
		}
		public static string NC_ONLY_FORM_TEXT
		{
			get { return GetLocalizedString("NC_ONLY_FORM_TEXT"); }
		}
		public static string FORM_TEXT
		{
			get { return GetLocalizedString("FORM_TEXT"); }
		}
		public static string LoadingScreenAppTitle
		{
			get { return GetLocalizedString("LoadingScreenAppTitle"); }
		}
		public static string BUTTON_TEXT
		{
			get { return GetLocalizedString("BUTTON_TEXT"); }
		}
		public static string EMAIL_LABEL
		{
			get { return GetLocalizedString("EMAIL_LABEL"); }
		}
		public static string ZENDESK_ID_TEXT
		{
			get { return GetLocalizedString("ZENDESK_ID_TEXT"); }
		}
		public static string DESCRIPTION_LABEL
		{
			get { return GetLocalizedString("DESCRIPTION_LABEL"); }
		}
		public static string STATUS_INITIAL
		{
			get { return GetLocalizedString("STATUS_INITIAL"); }
		}
		public static string STATUS_SENDING
		{
			get { return GetLocalizedString("STATUS_SENDING"); }
		}
		public static string STATUS_COLLECTING_PRODUCT
		{
			get { return GetLocalizedString("STATUS_COLLECTING_PRODUCT"); }
		}
		public static string STATUS_COLLECTING_HOST
		{
			get { return GetLocalizedString("STATUS_COLLECTING_HOST"); }
		}
		public static string STATUS_COLLECTING_GUEST
		{
			get { return GetLocalizedString("STATUS_COLLECTING_GUEST"); }
		}
		public static string STATUS_ARCHIVING
		{
			get { return GetLocalizedString("STATUS_ARCHIVING"); }
		}
		public static string APP_NAME
		{
			get { return GetLocalizedString("APP_NAME"); }
		}
		public static string FINISH_CAPT
		{
			get { return GetLocalizedString("FINISH_CAPT"); }
		}
		public static string FINISH_TEXT
		{
			get { return GetLocalizedString("FINISH_TEXT"); }
		}
		public static string PROMPT_TEXT
		{
			get { return GetLocalizedString("PROMPT_TEXT"); }
		}
		public static string DESC_MISSING_TEXT
		{
			get { return GetLocalizedString("DESC_MISSING_TEXT"); }
		}
		public static string ZENDESK_INVALID_ID_TEXT
		{
			get { return GetLocalizedString("ZENDESK_INVALID_ID_TEXT"); }
		}
		public static string SELECT_CATEGORY_TEXT
		{
			get { return GetLocalizedString("SELECT_CATEGORY_TEXT"); }
		}
		public static string EMAIL_MISSING_TEXT
		{
			get { return GetLocalizedString("EMAIL_MISSING_TEXT"); }
		}
		public static string RPC_FORM_TEXT
		{
			get { return GetLocalizedString("RPC_FORM_TEXT"); }
		}
		public static string WORK_DONE_TEXT
		{
			get { return GetLocalizedString("WORK_DONE_TEXT"); }
		}
		public static string PROGRESS_TEXT
		{
			get { return GetLocalizedString("PROGRESS_TEXT"); }
		}
		public static string TROUBLESHOOTER_TEXT
		{
			get { return GetLocalizedString("TROUBLESHOOTER_TEXT"); }
		}
		public static string LOGCOLLECTOR_RUNNING_TEXT
		{
			get { return GetLocalizedString("LOGCOLLECTOR_RUNNING_TEXT"); }
		}
		public static string LOGCOLLECTOR_PROBLEMS
		{
			get { return GetLocalizedString("LOGCOLLECTOR_PROBLEMS"); }
		}
		public static string APP_MISSING_TEXT
		{
			get { return GetLocalizedString("APP_MISSING_TEXT"); }
		}
		public static string ZIP_NAME
		{
			get { return GetLocalizedString("ZIP_NAME"); }
		}
		public static string STUCK_AT_INITIALIZING_FORM_TEXT
		{
			get { return GetLocalizedString("STUCK_AT_INITIALIZING_FORM_TEXT"); }
		}
		public static string RESTART_UTILITY_TITLE_TEXT
		{
			get { return GetLocalizedString("RESTART_UTILITY_TITLE_TEXT"); }
		}
		public static string RESTART_UTILITY_USAGE_TEXT
		{
			get { return GetLocalizedString("RESTART_UTILITY_USAGE_TEXT"); }
		}
		public static string RESTART_UTILITY_RESTARTING_TEXT
		{
			get { return GetLocalizedString("RESTART_UTILITY_RESTARTING_TEXT"); }
		}
		public static string RESTART_UTILITY_CANNOT_STOP_TEXT
		{
			get { return GetLocalizedString("RESTART_UTILITY_CANNOT_STOP_TEXT"); }
		}
		public static string RESTART_UTILITY_CANNOT_START_TEXT
		{
			get { return GetLocalizedString("RESTART_UTILITY_CANNOT_START_TEXT"); }
		}
		public static string RESTART_UTILITY_EXIT_TEXT
		{
			get { return GetLocalizedString("RESTART_UTILITY_EXIT_TEXT"); }
		}
		public static string RESTART_UTILITY_UNHANDLED_EXCEPTION_TEXT
		{
			get { return GetLocalizedString("RESTART_UTILITY_UNHANDLED_EXCEPTION_TEXT"); }
		}
		public static string RESTART_UTILITY_CANCEL_TEXT
		{
			get { return GetLocalizedString("CancelButtonText"); }
		}
		public static string ApkHandlerAlreadyRunning
		{
			get { return GetLocalizedString("APKINSTALLER_ALREADY_RUNNING"); }
		}
		public static string UPDATER_UTILITY_NO_UPDATE_TITLE
		{
			get { return GetLocalizedString("UPDATER_UTILITY_NO_UPDATE_TITLE"); }
		}
		public static string UPDATER_UTILITY_NO_UPDATE_TEXT
		{
			get { return GetLocalizedString("UPDATER_UTILITY_NO_UPDATE_TEXT"); }
		}
		public static string UPDATER_UTILITY_ASK_TO_INSTALL_TITLE
		{
			get { return GetLocalizedString("UPDATER_UTILITY_ASK_TO_INSTALL_TITLE"); }
		}
		public static string UPDATER_UTILITY_ASK_TO_INSTALL_TEXT
		{
			get { return GetLocalizedString("UPDATER_UTILITY_ASK_TO_INSTALL_TEXT"); }
		}
		public static string UPDATER_UTILITY_ASK_TO_INSTALL_NOW
		{
			get { return GetLocalizedString("UPDATER_UTILITY_ASK_TO_INSTALL_NOW"); }
		}
		public static string UPDATER_UTILITY_ASK_TO_INSTALL_REMIND_LATER
		{
			get { return GetLocalizedString("UPDATER_UTILITY_ASK_TO_INSTALL_REMIND_LATER"); }
		}
		public static string UpdateSuccess
		{
			get { return GetLocalizedString("UpdateSuccess"); }
		}
		public static string BlueStacksApkHandlerTitle
		{
			get { return GetLocalizedString("BlueStacksApkHandlerTitle"); }
		}
		public static string CommonAppTitleText
		{
			get { return GetLocalizedString("CommonAppTitleText"); }
		}
		public static string SnapShotShareString
		{
			get { return GetLocalizedString("SnapShotShareString"); }
		}
		public static string DefaultTitle
		{
			get { return GetLocalizedString("DefaultTitle"); }
		}
		public static string DesktopShortcutFileName
		{
			get { return GetLocalizedString("DesktopShortcutFileName"); }
		}
		public static string AppTitle
		{
			get { return GetLocalizedString("AppTitle"); }
		}
		public static string VMXError
		{
			get { return GetLocalizedString("VMX_IN_USE"); }
		}
		public static string HyperVEnabledError
		{
			get { return GetLocalizedString("HYPERV_ENABLED"); }
		}
		public static string SystemUpgradedError
		{
			get { return GetLocalizedString("SYSTEM_UPGRADED_ERROR"); }
		}
		public static string AndroidDataBackUpFailed
		{
			get { return GetLocalizedString("ANDROID_DATA_BACKUP_FAILED"); }
		}
		public static string UninstallProductConfirmation
		{
			get { return GetLocalizedString("UNINSTALLATION_CONFIRMATION"); }
		}
		public static string PreserveBSDataOnUninstall
		{
			get { return GetLocalizedString("PRESERVE_BSDATA_ON_UNINSTALL"); }
		}
		public static string UninstallationFormMsg
		{
			get { return GetLocalizedString("UNINSTALLATION_FORM_MSG"); }
		}
		public static string GroupResolutionTitle
		{
			get { return GetLocalizedString("GroupResolutionTitle"); }
		}
		public static string GroupBossKeyTitle
		{
			get { return GetLocalizedString("GroupBossKeyTitle"); }
		}
		public static string LandscapeMode960
		{
			get { return GetLocalizedString("LandscapeMode960"); }
		}
		public static string LandscapeMode1280
		{
			get { return GetLocalizedString("LandscapeMode1280"); }
		}
		public static string LandscapeMode1440
		{
			get { return GetLocalizedString("LandscapeMode1440"); }
		}
		public static string PortraitMode720
		{
			get { return GetLocalizedString("PortraitMode720"); }
		}
		public static string PortraitMode960
		{
			get { return GetLocalizedString("PortraitMode960"); }
		}
		public static string PortraitMode900
		{
			get { return GetLocalizedString("PortraitMode900"); }
		}
		public static string CustomizeResolutionSetting
		{
			get { return GetLocalizedString("CustomizeResolutionSetting"); }
		}
		public static string CustomizeResolutionSettingPrompt
		{
			get { return GetLocalizedString("CustomizeResolutionSettingPrompt"); }
		}
		public static string CustomerServiceQQ
		{
			get { return GetLocalizedString("CustomerServiceQQ"); }
		}
		public static string CustomerServiceQQGroup
		{
			get { return GetLocalizedString("CustomerServiceQQGroup"); }
		}
		public static string BossKeyLableName
		{
			get { return GetLocalizedString("BossKeyLableName"); }
		}
		public static string BossKeyInputPrompt
		{
			get { return GetLocalizedString("BossKeyInputPrompt"); }
		}
		public static string PromptSetBosskey
		{
			get { return GetLocalizedString("PromptSetBosskey"); }
		}
		public static string CustomizeResolutionNotNULL
		{
			get { return GetLocalizedString("CustomizeResolutionNotNULL"); }
		}
		public static string WidthOrHightNotRight
		{
			get { return GetLocalizedString("WidthOrHightNotRight"); }
		}
		public static string ResoutionCauseDeformation
		{
			get { return GetLocalizedString("ResoutionCauseDeformation"); }
		}

		public static string RebootAfterChangeResolution
		{
			get { return GetLocalizedString("RebootAfterChangeResolution"); }
		}
		public static string RestartNowBtn
		{
			get { return GetLocalizedString("RestartNowBtn"); }
		}
		public static string RestartLaterBtn
		{
			get { return GetLocalizedString("RestartLaterBtn"); }
		}
		public static string BosskeyRegisterFailed
		{
			get { return GetLocalizedString("BosskeyRegisterFailed"); }
		}
		public static string Settings
		{
			get { return GetLocalizedString("Settings"); }
		}
		public static string Location
		{
			get { return GetLocalizedString("Location"); }
		}

		public static string BLACKSCREEN_FORM_TEXT
		{
			get { return GetLocalizedString("BLACKSCREEN_FORM_TEXT"); }
		}
		public static string SELECT_APP_NAME
		{
			get { return GetLocalizedString("SELECT_APP_NAME"); }
		}
		public static string APP_NOT_SELECTED_TEXT
		{
			get { return GetLocalizedString("APP_NOT_SELECTED_TEXT"); }
		}
		public static string SELECT
		{
			get { return GetLocalizedString("SELECT"); }
		}
		public static string InstallationCorruptMessage
		{
			get { return GetLocalizedString("InstallationCorruptMessage"); }
		}
		public static string InstallationCorruptTitle
		{
			get { return GetLocalizedString("InstallationCorruptTitle"); }
		}
		public static string SUBCATEGORY
		{
			get { return GetLocalizedString("Subcategory"); }
		}
		public static string SELECT_SUBCATEGORY_TEXT
		{
			get { return GetLocalizedString("SELECT_SUBCATEGORY_TEXT"); }
		}
		public static string CATEGORY
		{
			get { return GetLocalizedString("Category"); }
		}

		public static string HyperVAlertMessage
		{
			get { return GetLocalizedString("HyperVAlertMessage"); }
		}
	}

	public class Subcategory
	{
		private string SubcategoryId;
		private string SubcategoryValue;
		private string ShowDropdown;

		public string subcategoryId
		{
			get { return SubcategoryId; }
			set { SubcategoryId = value; }
		}

		public string subcategoryValue
		{
			get { return SubcategoryValue; }
			set { SubcategoryValue = value; }
		}

		public string showdropdown
		{
			get { return ShowDropdown; }
			set { ShowDropdown = value; }
		}
	}

	public class Category
	{
		private string CategoryId;
		private string CategoryValue;
		private string ShowDropdown;
		private List<Subcategory> Subcategory;

		public List<Subcategory> subcategory
		{
			get { return Subcategory; }
			set { Subcategory = value; }
		}

		public string categoryId
		{
			get { return CategoryId; }
			set { CategoryId = value; }
		}
		public string categoryValue
		{
			get { return CategoryValue; }
			set { CategoryValue = value; }
		}
		public string showdropdown
		{
			get { return ShowDropdown; }
			set { ShowDropdown = value; }
		}
	}

	public class ProblemCategory
	{
		private List<Category> Category;
		public List<Category> category
		{
			get { return Category; }
			set { Category = value; }
		}
	}
}
