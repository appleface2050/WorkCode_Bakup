using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;



namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class FilterUtility
	{
		public static String[] sSupportedPackages;
		private static object sSupportedPackageLock = new object();

		private static string sClientId = "";
		public static string ClientId
		{
			get { return sClientId; }
			set { sClientId = value; }
		}

		public static string sDefaultTheme = "basic";

		/*
		 * Gives the current package based on
		 * the selected tab
		 * return null if its a web tab
		 * or mPackage is null
		 */
		public static string GetCurrentAppPkg()
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "getcurrentapppkg");
				string resp = Common.HTTP.Client.Get(url, null, false);
				JSonReader reader = new JSonReader();
				IJSonObject currentPkg = reader.ReadAsJSonObject(resp);

				return currentPkg["package"].ToString();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to find current app package... Err : " + ex.ToString());
				return null;
			}
		}
		public static bool sRunFilterDownloaderAgain = false;
		public static object sFilterDownloaderLock = new object();
		public static FilterDownloader mFilterDownloader;

		public static void CheckNewFiltersAvailable()
		{
			Thread thread = new Thread(delegate ()
			{
				if (Common.Utils.IsOSWinXP())
					return;

				lock (sFilterDownloaderLock)
				{
					if (FilterDownloader.Instance != null)
					{
						sRunFilterDownloaderAgain = true;
						return;
					}
				}

				FilterDownloader.Instance = new FilterDownloader();
				FilterDownloader.Instance.IsFilterUpdateAvailable();

				//if (FilterDownloader.Instance != null)
				//	FilterDownloader.Instance.LaunchUI(null);
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public static void UpdateLayout(string keyStr, string valueStr)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
			key.SetValue(keyStr, valueStr, RegistryValueKind.String);
			key.Close();
		}

		public static void UpdateAppViewLayoutRegistry(bool valueStr)
		{
			int keyValue = 0;
			if (valueStr)
				keyValue = 1;
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
			key.SetValue("AppViewLayout", keyValue, RegistryValueKind.DWord);
			key.Close();
		}

		public static FilterThemeConfig GetInitialConfigForTheme(string appPkg, string theme)
		{
			string jsPath = GetFilterDir() + @"\filterthemes.js";
			string text = GetDefaultConfig(jsPath);
			if (text == null)
				new FilterThemeConfig("{}");

			JSonReader reader = new JSonReader();
			IJSonObject config = reader.ReadAsJSonObject(text);
			Logger.Info("GetInitialConfig {0} {1}", appPkg, theme);
			IJSonObject themesObject = config["themes"][appPkg][theme];

			if (theme.Equals(themesObject["dir_name"].ToString()))
			{
				if (themesObject.Contains("initial_config"))
					text = themesObject["initial_config"].ToString();
				else
					text = "{}";
				return new FilterThemeConfig(text);
			}

			return new FilterThemeConfig("{}");
		}

		/*
		 * Generate a query string for the theme url
		 * after reading from registry and create
		 * empty registry if not found
		 */
		public static string GetQueryStringForTheme(string appPkg, string theme)
		{
			Logger.Info("In GetQueryStringForTheme");
			string queryParam = "theme=" + theme;
			queryParam += "&appPkg=" + appPkg;

			FilterThemeConfig filterThemeConfig = GetFilterThemeConfig(appPkg, theme);
			if (filterThemeConfig != null)
			{
				//queryParam += "&webcam=" + filterThemeConfig.mFilterThemeSettings.mIsWebCamOn.ToString().ToLower();
				//queryParam += "&chat=" + filterThemeConfig.mFilterThemeSettings.mIsChatOn.ToString().ToLower();
				foreach (string key in
						filterThemeConfig.mFilterThemeSettings.mOtherFields.Keys)
					queryParam += "&" + key + "=" + filterThemeConfig.mFilterThemeSettings.mOtherFields[key];
			}

			return queryParam;

		}

		public static FilterThemeConfig GetFilterThemeConfig(string appPkg, string theme)
		{
			RegistryKey filterAppKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath + @"\" + appPkg, true);
			if (filterAppKey != null)
			{
				filterAppKey.SetValue("CurrentTheme", theme.ToLower(), RegistryValueKind.String);
				string themeConfig = (string)filterAppKey.GetValue(theme, null);
				FilterThemeConfig filterThemeConfig;
				if (themeConfig == null)
				{
					Logger.Info("Setting Initial Config for {0}", theme);
					filterThemeConfig = GetInitialConfigForTheme(appPkg, theme);
				}
				else
				{
					Logger.Info("Config theme for {0}: {1}", theme, themeConfig);
					filterThemeConfig = new FilterThemeConfig(themeConfig);
				}
				return filterThemeConfig;
			}
			return null;
		}

		public static string GetQueryStringForTheme(string theme)
		{
			string appPkg = GetCurrentAppPkg();

			if (appPkg == null)
				return String.Empty;

			return GetQueryStringForTheme(GetCurrentAppPkg(), theme);
		}

		/*
		 * returns currentTheme for a apppkg
		 * should be called with filter Applicable
		 * app only
		 */
		public static string GetCurrentTheme(string appPkg)
		{
			if (appPkg == null)
				return null;

			RegistryKey filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			RegistryKey filterAppKey = filterKey.OpenSubKey(appPkg, true);
			if (filterAppKey == null)
			{
				filterKey.CreateSubKey(appPkg);
				filterKey.Close();
				return sDefaultTheme;
			}

			string currentTheme = (string)filterAppKey.GetValue("CurrentTheme", sDefaultTheme);

			Logger.Info("Current theme for appPkg: {0} value {1}", appPkg, currentTheme);
			filterAppKey.Close();
			return currentTheme;
		}

		public static string GetFilterDir()
		{
			return Path.Combine(StreamWindowUtility.InstallDir, @"UserData\Home\filters");
		}

		public static bool IsFilterApplicableApp()
		{
			return IsFilterApplicableApp(GetCurrentAppPkg());
		}

		public static string GetDefaultConfig(string filePath)
		{
			string text = null;
			try
			{
				text = System.IO.File.ReadAllText(filePath);
				int index = text.IndexOf("{");
				text = text.Substring(index, text.Length - index).Trim();
				if (text.EndsWith(";"))
					text = text.Substring(0, text.Length - 1);
			}
			catch (Exception e)
			{
				Logger.Error("Failed to read filterApps.json. Error: {0}", e.ToString());
			}
			return text;
		}

		public static void UpdateSupportedPackages()
		{
			lock (sSupportedPackageLock)
			{
				string jsPath = GetFilterDir() + @"\filterthemes.js";
				string text = GetDefaultConfig(jsPath);

				if (text == null)
					return;

				JSonReader reader = new JSonReader();
				IJSonObject fullJson = reader.ReadAsJSonObject(text);
				IJSonObject dataObject = fullJson["supported_packages"];

				sSupportedPackages = new String[dataObject.Length];
				Logger.Info("{0} supported apps are:", dataObject.Length);
				for (int i = 0; i < dataObject.Length; i++)
				{
					sSupportedPackages[i] = dataObject[i].ToString();
					Logger.Info(sSupportedPackages[i]);
				}
			}
		}

		public static bool IsFilterApplicableApp(string appPkg)
		{
			if (appPkg == null)
				return false;

			if (sSupportedPackages == null)
			{
				UpdateSupportedPackages();
				if (sSupportedPackages == null)
					return false;
			}

			lock (sSupportedPackageLock)
			{
				for (int i = 0; i < sSupportedPackages.Length; i++)
				{
					if (appPkg.Equals(sSupportedPackages[i]))
					{
						Logger.Info("{0} is a filtered app", appPkg);
						return IsFilterAvailableForThisApp(appPkg);
					}
				}
			}

			Logger.Info("{0} is not filtered app", appPkg);
			return false;
		}

		public static bool IsFilterAvailableForThisApp(string appPkg)
		{
			RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath);
			string appsListStr = (string)baseKey.GetValue("FilterAvailableForApps", "[]");
			baseKey.Close();

			JSonReader read = new JSonReader();
			IJSonObject obj = read.ReadAsJSonObject(appsListStr);

			for (int i = 0; i < obj.Length; i++)
			{
				if (appPkg.Equals(obj[i].ToString()))
					return true;
			}
			return false;
		}

		public static void SendRequestToCLRBrowser(string path, Dictionary<string, string> data)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			int clrBrowserPort = (int)key.GetValue("CLRBrowserServerPort", 2911);
			string url = string.Format("http://127.0.0.1:{0}/{1}", clrBrowserPort, path);
			Logger.Info("Sending request to: " + url);
			if (data == null)
				Common.HTTP.Client.Get(url, null, false);
			else
				Common.HTTP.Client.Post(url, data, null, false);
		}

		public static void SetCurrentTheme(string appPkg, string theme)
		{
			if (appPkg == null)
				return;

			RegistryKey filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath + @"\" + appPkg, true);
			if (filterKey != null)
			{
				filterKey.SetValue("CurrentTheme", theme, RegistryValueKind.String);
				filterKey.Close();
			}
		}

		public static string GetChannelName()
		{
			RegistryKey filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath);
			return (string)filterKey.GetValue("ChannelName", String.Empty);
		}

		public static void SetChannelName(string channelName)
		{
			RegistryKey filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			if (filterKey != null)
			{
				filterKey.SetValue("ChannelName", channelName, RegistryValueKind.String);
				filterKey.Close();
			}
			else
			{
				RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath, true);
				regKey.CreateSubKey("Filter");
				regKey.Close();

				filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
				filterKey.SetValue("ChannelName", channelName, RegistryValueKind.String);
				filterKey.Close();
			}
		}
	}

	public class FilterThemeConfig
	{
		public FilterThemeSettings mFilterThemeSettings;
		public FilterThemeCameraSettings mFilterThemeCameraSettings;

		public FilterThemeConfig(string themeConfig)
		{
			Logger.Info("In FilterThemeConfig Constructor");
			JSonReader reader = new JSonReader();
			IJSonObject themeConfigObj = reader.ReadAsJSonObject(themeConfig);

			string settings = "{}";
			if (themeConfigObj.Contains("settings"))
			{
				settings = themeConfigObj["settings"].ToString();
			}
			mFilterThemeSettings = new FilterThemeSettings(settings);

			string cameraSettings = "{}";
			if (themeConfigObj.Contains("camera"))
			{
				cameraSettings = themeConfigObj["camera"].ToString();
			}
			mFilterThemeCameraSettings = new FilterThemeCameraSettings(cameraSettings);
		}

		public String ToJsonString()
		{
			JSonWriter writer = new JSonWriter();
			writer.WriteObjectBegin();
			writer.WriteMember("settings");
			writer.Write(new JSonReader().Read(mFilterThemeSettings.ToJsonString()));
			writer.WriteMember("camera");
			writer.Write(new JSonReader().Read(mFilterThemeCameraSettings.ToJsonString()));
			writer.WriteObjectEnd();

			return writer.ToString();
		}
	}

	public class FilterThemeCameraSettings
	{
		public int width;
		public int height;
		public int x;
		public int y;

		public FilterThemeCameraSettings(string cameraSettings)
		{
			Logger.Info("In FilterThemeCameraSettings Constructor");
			JSonReader reader = new JSonReader();
			IJSonObject settingsObj = reader.ReadAsJSonObject(cameraSettings);

			if (settingsObj.Contains("width"))
			{
				Logger.Info("Camera width is {0}", settingsObj["width"].Int32Value);
				width = settingsObj["width"].Int32Value;
			}

			if (settingsObj.Contains("height"))
			{
				Logger.Info("Camera height is {0}", settingsObj["height"].Int32Value);
				height = settingsObj["height"].Int32Value;
			}

			if (settingsObj.Contains("x"))
			{
				Logger.Info("Camera x is {0}", settingsObj["x"].Int32Value);
				x = settingsObj["x"].Int32Value;
			}

			if (settingsObj.Contains("y"))
			{
				Logger.Info("Camera y is {0}", settingsObj["y"].Int32Value);
				y = settingsObj["y"].Int32Value;
			}
		}

		public string ToJsonString()
		{
			JSonWriter writer = new JSonWriter();
			writer.WriteObjectBegin();
			writer.WriteMember("width", width);
			writer.WriteMember("height", height);
			writer.WriteMember("x", x);
			writer.WriteMember("y", y);
			writer.WriteObjectEnd();

			return writer.ToString();
		}
	}

	public class FilterThemeSettings
	{
		public bool mIsWebCamOn = false;
		public bool mIsChatOn = false;
		public bool mIsAnimate = true;
		public Dictionary<string, string> mOtherFields = new Dictionary<string, string>();

		public FilterThemeSettings(string settings)
		{
			Logger.Info("In FilterThemeSettings Constructor");
			JSonReader reader = new JSonReader();
			IJSonObject settingsObj = reader.ReadAsJSonObject(settings);

			Logger.Info("reading sidebar cam status");
			if (settingsObj.Contains("webcam") && StreamManager.Instance != null && String.Compare(StreamManager.mCamStatus, "true", true) == 0)
			{
				Logger.Info("WebCam status is true");
				mIsWebCamOn = true;
			}

			if (settingsObj.Contains("chat") && settingsObj["chat"].BooleanValue)
			{
				Logger.Info("Chat status is true");
				mIsChatOn = true;
			}

			if (settingsObj.Contains("animate") && !settingsObj["animate"].BooleanValue)
			{
				Logger.Info("animate status is false");
				mIsAnimate = false;
			}

			foreach (string key in settingsObj.Names)
			{
				//Exception handling for camera status
				if (key.Equals("webcam"))
					mOtherFields.Add(key, mIsWebCamOn.ToString().ToLower());
				else
					mOtherFields.Add(key, settingsObj[key].StringValue);
			}
		}

		public string ToJsonString()
		{
			JSonWriter writer = new JSonWriter();
			writer.WriteObjectBegin();

			foreach (string key in mOtherFields.Keys)
			{
				if (!key.Equals("webcam") && !key.Equals("chat")
						&& !key.Equals("animate"))
					writer.WriteMember(key, mOtherFields[key]);
			}
			writer.WriteMember("webcam", mIsWebCamOn);
			writer.WriteMember("animate", mIsAnimate);
			writer.WriteMember("chat", mIsChatOn);
			writer.WriteObjectEnd();

			return writer.ToString();
		}
	}
}
