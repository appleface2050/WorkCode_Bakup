/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 */

using System;
using System.IO;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Common
{
	public class Features
	{
		private static string s_ConfigPath = Common.Strings.HKLMAndroidConfigRegKeyPath;

		// List of all features
		// Lower 31 bits
		public const ulong BROADCAST_MESSAGES = 0x00000001;
		public const ulong INSTALL_NOTIFICATIONS = 0x00000002;
		public const ulong UNINSTALL_NOTIFICATIONS = 0x00000004;
		public const ulong CREATE_APP_SHORTCUTS = 0x00000008;
		public const ulong LAUNCH_SETUP_APP = 0x00000010;
		public const ulong SHOW_USAGE_STATS = 0x00000020;
		public const ulong SYS_TRAY_SUPPORT = 0x00000040;
		public const ulong SUGGESTED_APPS_SUPPORT = 0x00000080;
		public const ulong OTA_SUPPORT = 0x00000100;
		public const ulong SHOW_RESTART = 0x00000200;
		public const ulong ANDROID_NOTIFICATIONS = 0x00000400;
		public const ulong RIGHT_ALIGN_PORTRAIT_MODE = 0x00000800;
		public const ulong LAUNCH_FRONTEND_AFTER_INSTALLTION = 0x00001000;
		public const ulong CREATE_LIBRARY = 0x00002000;
		public const ulong SHOW_AGENT_ICON_IN_SYSTRAY = 0x00004000;
		public const ulong IS_HOME_BUTTON_ENABLED = 0x00008000;
		public const ulong IS_GRAPHICS_DRIVER_REMINDER_ENABLED = 0x00010000;
		public const ulong EXIT_ON_HOME = 0x00020000;
		public const ulong MULTI_INSTANCE_SUPPORT = 0x00040000;
		public const ulong UPDATE_FRONTEND_APP_TITLE = 0x00080000;
		public const ulong USE_DEFAULT_NETWORK_TEXT = 0x00100000;
		public const ulong IS_FULL_SCREEN_TOGGLE_ENABLED = 0x00200000;
		public const ulong SET_CHINA_LOCALE_AND_TIMEZONE = 0x00400000;
		public const ulong SHOW_TOGGLE_BUTTON_IN_LOADING_SCREEN = 0x00800000;
		public const ulong ENABLE_ALT_CTRL_I_SHORTCUTS = 0x01000000;
		public const ulong CREATE_LIBRARY_SHORTCUT_AT_DESKTOP = 0x02000000;
		public const ulong CREATE_START_LAUNCHER_SHORTCUT = 0x04000000;
		public const ulong WRITE_APP_CRASH_LOGS = 0x10000000;
		public const ulong CHINA_CLOUD = 0x20000000;
		public const ulong FORCE_DESKTOP_MODE = 0x40000000;
		public const ulong NOT_TO_BE_USED = 0x80000000;     // Do not use this bit

		public const ulong ENABLE_ALT_CTRL_M_SHORTCUTS = 0x0000000100000000;
		public const ulong COLLECT_APK_HANDLER_LOGS = 0x0000000200000000;
		public const ulong SHOW_FRONTEND_FULL_SCREEN_TOAST = 0x0000000400000000;
		public const ulong IS_CHINA_UI = 0x0000000800000000;

		// Higher 31 bits
		public const ulong NOT_TO_BE_USED_2 = 0x8000000000000000;   // Do not use this bit

		public const ulong ALL_FEATURES = 0x7FFFFFFF7FFFFFFF;


		public const uint BST_HIDE_NAVIGATIONBAR = 0x00000001;
		public const uint BST_HIDE_STATUSBAR = 0x00000002;
		public const uint BST_HIDE_BACKBUTTON = 0x00000004;
		public const uint BST_HIDE_HOMEBUTTON = 0x00000008;
		public const uint BST_HIDE_RECENTSBUTTON = 0x00000010;
		public const uint BST_HIDE_SCREENSHOTBUTTON = 0x00000020;
		public const uint BST_HIDE_TOGGLEBUTTON = 0x00000040;
		public const uint BST_HIDE_CLOSEBUTTON = 0x00000080;

		//for hiding location button in bottom bar
		public const uint BST_HIDE_GPS = 0x00000200;
		public const uint BST_SHOW_APKINSTALLBUTTON = 0x00000800;

		// catering to changes in ROSEN apks (3rd byte)
		public const uint BST_HIDE_HOMEAPPNEWLOADER = 0x00010000;
		public const uint BST_SENDLETSGOS2PCLICKREPORT = 0x00020000;
		public const uint BST_DISABLE_P2DM = 0x00040000;
		public const uint BST_DISABLE_ARMTIPS = 0x00080000;
		public const uint BST_DISABLE_S2P = 0x00100000;

		// frameworks/base -- related to ime(4th byte -- starting from end)
		public const uint BST_SOGOUIME = 0x10000000;
		public const uint BST_BAIDUIME = 0x40000000;
		public const uint BST_QQIME = 0x80000000;
		public const uint BST_QEMU_3BT_COEXISTENCE_BIT = 0x20000000;

		// used to determine if you want to show s2p/search/baidu apps in home_app_new (Apps.apk)
		public const uint BST_HIDE_S2P_SEARCH_BAIDU_IN_HOMEAPPNEW = 0x00400000;

		// launch an app as a new task. also causes runex to use recent apps list to bring app to front
		public const uint BST_NEW_TASK_ON_HOME = 0x00200000;

		// for some chinese oems, don't reinstall if version if already there and return as successful.
		public const uint BST_NO_REINSTALL = 0x04000000;

		//for some chinese oems, don't show app guidance scree.
		public const int BST_HIDE_GUIDANCESCREEN = 0x00000400;

		//for chinese oems, start using chinese cdn.
		public const int BST_USE_CHINESE_CDN = 0x00001000;
		public const int BST_ENALBE_ABOUT_PHONE_OPTION = 0x01000000;
		public const int BST_ENABLE_SECURITY_OPTION = 0x02000000;

		//app features
		public const uint BST_SKIP_S2P_WHILE_LAUNCHING_APP = 0x00000800;

		public static ulong GetEnabledFeatures()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(s_ConfigPath, true);
			if (key == null)
			{
				return 0;
			}

			ulong feature1 = Convert.ToUInt32(key.GetValue("Features", 0));
			ulong feature2 = 0;

			if (key.GetValue("FeaturesHigh") == null)
			{
				key.SetValue("FeaturesHigh", 0);
				key.Flush();
			}
			else
			{
				feature2 = Convert.ToUInt32(key.GetValue("FeaturesHigh", 0));
			}

			ulong features = feature2 << 32 | feature1;
			return features;
		}

		public static void SetEnabledFeatures(
				ulong feature
				)
		{
			uint featuresLow, featuresHigh;
			GetHighLowFeatures(feature, out featuresHigh, out featuresLow);
			RegistryKey key = Registry.LocalMachine.OpenSubKey(s_ConfigPath, true);
			key.SetValue("Features", featuresLow);
			key.SetValue("FeaturesHigh", featuresHigh);
			key.Flush();
			key.Close();
		}

		public static void GetHighLowFeatures(
				ulong features,
				out uint featuresHigh,
				out uint featuresLow
				)
		{
			featuresLow = (uint)(features & 0xffffffff);
			featuresHigh = (uint)(features >> 32);
		}

		public static bool IsFeatureEnabled(
				ulong featureMask
				)
		{
			ulong features = GetEnabledFeatures();
			if (features == 0)
				features = Oem.Instance.WindowsOEMFeatures;
			return IsFeatureEnabled(featureMask, features);
		}

		public static bool IsFeatureEnabled(
				ulong featureMask,
				ulong features
				)
		{
			try
			{
				if ((features & featureMask) != 0)
					return true;
				else
					return false;
			}
			catch (Exception exc)
			{
				Logger.Error(exc.ToString());
				return false;
			}
		}

		public static void DisableFeature(
				ulong featureMask
				)
		{
			ulong features = GetEnabledFeatures();

			if ((features & featureMask) == 0)
				return;     // already disabled

			ulong newMask = features & ~featureMask;
			SetEnabledFeatures(newMask);
		}

		public static void EnableFeature(
				ulong featureMask
				)
		{
			ulong features = GetEnabledFeatures();

			if ((features & featureMask) != 0)
				return;     // already enabled

			ulong newMask = features | featureMask;
			SetEnabledFeatures(newMask);
		}

		public static void EnableAllFeatures()
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(s_ConfigPath);
			SetEnabledFeatures(ALL_FEATURES);
		}

		public static void EnableFeaturesOfOem()
		{
			ulong features = Oem.Instance.WindowsOEMFeatures;
			SetEnabledFeatures(features);
		}

		/*
		 * The following are not implemented by feature bits.  They
		 * don't really belong here, but need to live somewhere
		 * until we find an appropriate home.
		 */

		public static bool IsFullScreenToggleEnabled()
		{
			return IsFeatureEnabled(IS_FULL_SCREEN_TOGGLE_ENABLED);
		}

		public static bool IsHomeButtonEnabled()
		{
			return IsFeatureEnabled(IS_HOME_BUTTON_ENABLED);
		}

		public static bool IsShareButtonEnabled()
		{
			return false;
		}

		public static bool IsGraphicsDriverReminderEnabled()
		{
			return IsFeatureEnabled(IS_GRAPHICS_DRIVER_REMINDER_ENABLED);
		}

		public static bool IsSettingsButtonEnabled()
		{
			return false;
		}

		public static bool IsBackButtonEnabled()
		{
			return false;
		}

		public static bool IsMenuButtonEnabled()
		{
			return false;
		}

		public static bool ExitOnHome()
		{
			return IsFeatureEnabled(EXIT_ON_HOME);
		}

		public static bool UpdateFrontendAppTitle()
		{
			return IsFeatureEnabled(UPDATE_FRONTEND_APP_TITLE);
		}

		public static bool UseDefaultNetworkText()
		{
			return IsFeatureEnabled(USE_DEFAULT_NETWORK_TEXT);
		}
	}
}


