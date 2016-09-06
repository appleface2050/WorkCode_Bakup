using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using System.Windows.Media.Imaging;
using CodeTitans.JSon;
using System.Net;
using System.Globalization;
using System.Text.RegularExpressions;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public static class StreamWindowUtility
	{
		public const int STREAM_WINDOW_WIDTH = 320;
		public static int sDpi = Utils.GetDPI();
		private const int mGrip = 5;

		public static Panel mOBSRenderFrame;
		public static IntPtr mOBSHandle = IntPtr.Zero;
		public static bool sOBSDevEnv = false;
		public static bool isCheckThreadAlreadyRunning = false;

		public static bool sIsOBSReParented = false;

		public const string THEMES_HTML = "themes.html";
		public const string HOME_HTML = "home.html";
		public const string SEARCH_HTML = "search-results.html";
		public const string LOCAL_MY_APPS_HTML = "local-my-apps.html";


		public static string sChannelNamesJson = "channel_names.json";
		public static string sChannelAppsJson = "channel_apps.json";
		public static string sWebAppsJson = "web_apps.json";
		public static string sInstalledAppsJson = "installedApps.json";
		public static string sThemesJson = "themes.json";

		public static string sDefaultTheme = "default_theme";
		public static String sLocalMyAppsHtml;
		public static String sNoWifiHtml;
		public static String sWaitHtml;
		public static String sStreamWindowHtml;
		public static String sStreamWindowProdHtml;
		public static String sStreamWindowProd2Html;
		public static String sStreamWindowQAHtml;
		public static String sStreamWindowStagingHtml;
		public static String sStreamWindowDevHtml;
		public const String JSON_BASE_URL = "http://cdn.bluestacks.com/public/gamemanager/content/bluestacks/json2/";
		public static OBSRenderFrameSpecs sOBSRenderFrameSpecs;

		public static String CHANNEL_NAMES_JSON_URL = JSON_BASE_URL + "channel_names.json";
		public static String CHANNEL_APPS_JSON_URL = JSON_BASE_URL + "channel_apps.json";
		public static String WEB_APPS_JSON_URL = JSON_BASE_URL + "web_apps.json";
		public static String THEMES_JSON_URL = JSON_BASE_URL + "themes.json";
		public static string mCurrentThemeUrl;

		private static Object sLockObject = new Object();

		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int which);
		private const int SM_CXSCREEN = 0;
		private const int SM_CYSCREEN = 1;

		private static System.Drawing.Size? sSize = null;
		public static System.Drawing.Size ScreenSize
		{
			get
			{
				if (!sSize.HasValue)
				{
					sSize = new System.Drawing.Size(GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
				}
				return sSize.Value;
			}
		}

		static string sProgramData = string.Empty;
		public static string ProgramData
		{
			get
			{
				if (string.IsNullOrEmpty(sProgramData))
				{
					sProgramData = (string)Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
					Logger.Info("the value of gmconfipath is {0}", Common.Strings.GMBasePath);
				}
				return sProgramData;
			}

			set
			{
				sProgramData = value;
			}
		}

		static string sSetupDir = string.Empty;
		public static string SetupDir
		{
			get
			{
				if (string.IsNullOrEmpty(sSetupDir))
				{
					sSetupDir = Path.Combine(ProgramData, "BlueStacksSetup");
				}
				return sSetupDir;
			}

			set
			{
				sSetupDir = value;
			}
		}

		static string sInstallDir = string.Empty;

		public static string InstallDir
		{
			get
			{
				if (string.IsNullOrEmpty(sInstallDir))
				{
					RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
					sInstallDir = (string)reg.GetValue("InstallDir");
					Logger.Info("the installdir path is " + sInstallDir);
				}
				return sInstallDir;
			}

			set
			{
				sInstallDir = value;
			}
		}


		static string sBluestacksGameManager = string.Empty;

		public static string BluestacksGameManager
		{
			get
			{
				if (string.IsNullOrEmpty(sBluestacksGameManager))
				{
					RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
					sBluestacksGameManager = (string)reg.GetValue("InstallDir");
					Logger.Info("the sBluestacksGameManager path is " + sBluestacksGameManager);
				}
				return sBluestacksGameManager;
			}

			set
			{
				sBluestacksGameManager = value;
			}
		}

		public static void UpdateLocalUrls()
		{
			string baseUrl = String.Format("http://localhost:{0}/static/themes", App.sBlueStacksTVPort);
			string themeUrl = String.Format("{0}/{1}/", baseUrl, getCurrentTheme());
			sLocalMyAppsHtml = themeUrl + "local-my-apps.html";
			sNoWifiHtml = themeUrl + "no-wifi.html";
			sWaitHtml = themeUrl + "wait.html";
			sStreamWindowHtml = "http://bluestacks-tv.appspot.com/home";
			sStreamWindowProdHtml = "http://bluestacks-tv-prod.appspot.com/home";
			sStreamWindowProd2Html = "http://bluestacks-tv2-prod.appspot.com/home";
			sStreamWindowQAHtml = "http://bluestacks-tv-qa.appspot.com/home";
			sStreamWindowStagingHtml = "http://bluestacks-tv-staging.appspot.com/home";
			sStreamWindowDevHtml = "http://bluestacks-tv-dev.appspot.com/home";
		}


		public static void UnSetOBSParentWindow()
		{
			if (mOBSHandle != IntPtr.Zero)
			{
				Window.ShowWindow(mOBSHandle, Window.SW_HIDE);
				Window.SetParent(mOBSHandle, IntPtr.Zero);
			}
			//mOBSRenderFrame.Hide();
			sIsOBSReParented = false;
			StreamWindow.Instance.HideGrid();
		}

		public static void ReParentOBSWindow()
		{
			if (sOBSRenderFrameSpecs != null)
				ReParentOBSWindow(sOBSRenderFrameSpecs);
		}

		public static void ReParentOBSWindow(OBSRenderFrameSpecs obsRenderFrameSpecs)
		{
			if (sIsOBSReParented)
			{
				Logger.Info("skipping reparenting as already reparented");
				return;
			}

			sOBSRenderFrameSpecs = obsRenderFrameSpecs;

			int actualWidth = (int)(StreamWindow.Instance.BrowserGrid.RenderSize.Width) * sOBSRenderFrameSpecs.width / 100;
			int actualHeight = (int)(StreamWindow.Instance.BrowserGrid.RenderSize.Height) * sOBSRenderFrameSpecs.height / 100;
			int actualY = (int)(StreamWindow.Instance.BrowserGrid.RenderSize.Height) * sOBSRenderFrameSpecs.yPosition / 100;
			int actualX = sOBSRenderFrameSpecs.xPosition;

			if (sOBSRenderFrameSpecs.xPosition != -1)
				actualX = (int)(StreamWindow.Instance.BrowserGrid.RenderSize.Width) * sOBSRenderFrameSpecs.xPosition / 100;
			Point p = new Point(actualWidth, actualHeight);
			Logger.Info("GridSizeForRenderFrame {0} x {1}", p.X, p.Y);

			Size renderFrameSize = new Size((int)(p.X * sDpi / Utils.DEFAULT_DPI),
						(int)(p.Y * sDpi / Utils.DEFAULT_DPI));
			if (mOBSRenderFrame == null)
			{
				mOBSRenderFrame = new Panel();
				mOBSRenderFrame.BackColor = System.Drawing.Color.Black;
			}

			mOBSRenderFrame.Size = renderFrameSize;
			StreamWindow.Instance.AddPanel(mOBSRenderFrame);
			if (!sIsOBSReParented)
				StreamWindow.Instance.HideGrid();
			IntPtr panelHandle = mOBSRenderFrame.Handle;
			mOBSRenderFrame.BringToFront();

			Thread d = new Thread(delegate ()
			{
				mOBSHandle = Window.FindWindow("OBSWindowClass", null);

				int requiredHeight = renderFrameSize.Height;
				try
				{
					if (obsRenderFrameSpecs.preserveAspectRatio)
					{
						requiredHeight = ((obsRenderFrameSpecs.heightRatio * renderFrameSize.Width) / obsRenderFrameSpecs.widthRatio);
						Logger.Info("RenderFrame {0} x {1}", renderFrameSize.Width, renderFrameSize.Height);
					}
				}
				catch (Exception ex)
				{
					Logger.Error("ReParentOBSWindow Error : {0}", ex.ToString());
				}

				while (mOBSHandle == IntPtr.Zero)
				{
					Thread.Sleep(100);
					mOBSHandle = Window.FindWindow("OBSWindowClass", null);
				}

				StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
				{
					mOBSRenderFrame.Size = new Size(renderFrameSize.Width, requiredHeight);

					StreamWindow.Instance.BrowserGrid.RowDefinitions[0].Height = new System.Windows.GridLength(
						actualY, System.Windows.GridUnitType.Pixel);
					StreamWindow.Instance.BrowserGrid.RowDefinitions[1].Height = new System.Windows.GridLength(
						requiredHeight * Utils.DEFAULT_DPI / sDpi, System.Windows.GridUnitType.Pixel);
					StreamWindow.Instance.BrowserGrid.ColumnDefinitions[1].Width = new System.Windows.GridLength(
						renderFrameSize.Width * Utils.DEFAULT_DPI / sDpi, System.Windows.GridUnitType.Pixel);

					int xPosition = actualX;
					if (xPosition == -1)
					{
						xPosition = ((int)(StreamWindow.Instance.BrowserGrid.RenderSize.Width) - actualWidth) / 2;
						StreamWindow.Instance.BrowserGrid.ColumnDefinitions[2].Width = new System.Windows.GridLength(
							xPosition, System.Windows.GridUnitType.Pixel);
					}

					StreamWindow.Instance.BrowserGrid.ColumnDefinitions[0].Width = new System.Windows.GridLength(
							xPosition, System.Windows.GridUnitType.Pixel);

					StreamWindow.Instance.ShowGrid();
					Logger.Info("OBS Handle: {0}", mOBSHandle.ToString());

					Logger.Info("mOBSRenderFrame {0} x {1}", renderFrameSize.Width, requiredHeight);
					if (sOBSDevEnv)
						Window.SetWindowLong(mOBSHandle, Window.GWL_STYLE, Window.GetWindowLong(mOBSHandle, Window.GWL_STYLE) | Convert.ToInt32(Window.WS_CHILD));
					else
						Window.SetWindowLong(mOBSHandle, Window.GWL_STYLE, Convert.ToInt32(Window.WS_CHILD));
					Window.SetParent(mOBSHandle, panelHandle);

					//+4 height for removing white patches visible due to scaling issue
					Window.SetWindowPos(
								mOBSHandle,
								(IntPtr)0,
								-1,
								-1,
								renderFrameSize.Width + 2,
								requiredHeight + 4,
								Window.SWP_NOACTIVATE | Window.SWP_SHOWWINDOW
							);
					Window.ShowWindow(mOBSHandle, Window.SW_SHOW);
					sIsOBSReParented = true;
				}));
			});
			d.IsBackground = true;
			d.Start();
		}

		public static void CheckIfGMIsRunning()
		{
			Logger.Info("In check if gm is running");

			RegistryKey gmConfigKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			int gmPort = (int)gmConfigKey.GetValue("PartnerServerPort", 2871);

			Thread gmCheckThread = new Thread(delegate ()
			{
				while (true)
				{
					string url = String.Format("http://127.0.0.1:{0}/{1}", gmPort.ToString(), "ping");

					try
					{
						string r = Common.HTTP.Client.Get(url, null, false, 1000);
						if (r.Contains("true") || r.Contains("ok"))
						{
							Thread.Sleep(2000);
							continue;
						}
						Logger.Info("GameManager not running...");
						CloseBTV();
						break;
					}
					catch (Exception ex)
					{
						Logger.Error("GameManager not running... Err : " + ex.ToString());
						CloseBTV();
						break;
					}
				}
			});
			if (!isCheckThreadAlreadyRunning)
			{
				Logger.Info("starting gm check thread");
				isCheckThreadAlreadyRunning = true;
				gmCheckThread.IsBackground = true;
				gmCheckThread.Start();
			}
		}

		public static void CloseBTV()
		{
			try
			{
				Logger.Info("Exiting OBS and closing btv exe");
				if (StreamWindow.Instance != null)
				{
					StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
					{
						StreamWindow.Instance.Close();
					}));
				}
				else
				{
					Utils.KillProcessByName("HD-OBS");
				}
			}
			catch (Exception e)
			{
				Logger.Info("Failed to kill HD-OBS.exe...Err : " + e.ToString());
			}
			App.sConfigKey.Close();
			Environment.Exit(1);
		}

		public static bool IsGMFullScreen()
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "getwindowstate");
				string resp = Common.HTTP.Client.Get(url, null, false);

				JSonReader reader = new JSonReader();
				IJSonObject obj = reader.ReadAsJSonObject(resp);
				string windowState = obj["windowstate"].ToString();

				if (string.Equals(windowState, "fullscreen", StringComparison.OrdinalIgnoreCase))
					return true;
			}
			catch (Exception ex)
			{
				Logger.Info("Failed in getting GM window state... Err : " + ex.ToString());
			}
			return false;
		}

		public static void AddNewStreamViewKey(string label, string jsonString)
		{

			string url = string.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "addstreamviewkey");
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("label", label);
			data.Add("jsonString", jsonString);

			try
			{
				string resp = Common.HTTP.Client.Post(url, data, null, false);
				Logger.Info("response for api addstreamkey : " + resp);
			}
			catch (Exception ex)
			{
				Logger.Info("Failed to send addstreamviewkey... Err : " + ex.ToString());
			}
		}

		public static string getCurrentTheme()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			return (string)key.GetValue("theme", sDefaultTheme);
		}

		private static string mThemesDir = "themes\\";
		public static string GetCurrentThemeLocalDir()
		{
			return Path.Combine(Common.Strings.GameManagerHomeDir, mThemesDir + getCurrentTheme());
		}

		public static string getCurrentThemeHomeUrl()
		{
			string result = Path.Combine(mCurrentThemeUrl, HOME_HTML);
			Logger.Info("getCurrentThemeHomeUrl(): " + result);
			return result;
		}

		public static string getCurrentThemeSearchUrl()
		{
			string result = Path.Combine(mCurrentThemeUrl, SEARCH_HTML);
			Logger.Info("getCurrentThemeSearchUrl(): " + result);
			return result;
		}


		public static string getCurrentThemeThemesUrl()
		{
			string result = Path.Combine(mCurrentThemeUrl, THEMES_HTML);
			Logger.Info("getCurrentThemeThemesUrl(): " + result);
			return result;
		}

		public static string GetChannelNamesJson()
		{
			string defaultData = "[]";
			string path = Strings.GameManagerHomeDir + @"\" + sChannelNamesJson;
			string fileData = GetJson(CHANNEL_NAMES_JSON_URL, path, defaultData);
			return fileData;
		}

		public static string GetChannelAppsJson(string channelId, string subCategory)
		{
			String data = "[]";
			bool filter = true;
			bool filter2 = true;

			if (channelId.Equals(""))
				return data;
			if (channelId.Equals("null"))
				filter = false;
			if (subCategory.Equals("") || subCategory.Equals("null"))
				filter2 = false;

			string fullJsonString = GetAllAppsJsonString();
			JSonReader readjson = new JSonReader();
			IJSonObject fullJson = readjson.ReadAsJSonObject(fullJsonString);

			try
			{
				//flushing data
				data = "";

				JSonWriter writer = new JSonWriter(true);
				writer.WriteArrayBegin();

				if (filter == true)
				{
					for (int i = 0; i < fullJson.Length; i++)
					{
						IJSonObject channelIds = fullJson[i]["channelIds"];
						for (int j = 0; j < channelIds.Length; j++)
						{
							if (channelIds[j].ToString().Equals(channelId))
							{
								if (filter2 == true)
								{
									//do some more filtering
									try
									{
										if (fullJson[i]["category"].ToString().Equals(subCategory))
										{
											writer.Write(fullJson[i]);
										}
									}
									catch
									{
									}
								}
								else
								{
									writer.Write(fullJson[i]);
								}
								break;
							}
						}
					}
				}
				else if (filter2 == true)
				{
					for (int i = 0; i < fullJson.Length; i++)
					{
						try
						{
							if (fullJson[i]["category"].ToString() == subCategory)
							{
								writer.Write(fullJson[i]);
							}
						}
						catch
						{
						}
					}
				}
				else //no filter is applied hence return the data as it is
				{
					return fullJsonString;
				}

				writer.WriteArrayEnd();
				data = writer.ToString();
				//Logger.Info(data);
				return data;
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				return "[]";
			}
		}

		public static string GetWebAppsJson()
		{
			string defaultData = "[]";
			string path = Strings.GameManagerHomeDir + @"\" + sWebAppsJson;
			string fileData = GetJson(WEB_APPS_JSON_URL, path, defaultData);
			return fileData;
		}

		public static string GetChannelAppsJson()
		{
			string defaultData = "[]";
			string path = Strings.GameManagerHomeDir + @"\" + sChannelAppsJson;
			string fileData = GetJson(CHANNEL_APPS_JSON_URL, path, defaultData);
			return fileData;
		}

		public static String GetJson(string url, string path, string defaultData)
		{
			if (File.Exists(path) && !ValidJsonFile(path))
				File.Delete(path);

			string fileData;
			lock (sLockObject)
			{
				fileData = GetDataFromUrl(url, path, defaultData);
			}

			if (!File.Exists(path) || !ValidJsonFile(path))
				return defaultData;

			return fileData;
		}

		public static String GetDataFromUrl(String url, String path, String defaultData)
		{
			try
			{
				if (File.Exists(path) == false)
				{
					DownloadFile(url, path);
				}
				else if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(path)) > TimeSpan.FromDays(1))
				{
					Thread t = new Thread(delegate ()
					{
						DownloadFile(url, path);
					});
					t.IsBackground = true;
					t.Start();
				}

				string fileData;
				if (File.Exists(path))
				{
					StreamReader sr = new StreamReader(path);
					fileData = sr.ReadToEnd();
					sr.Close();
				}
				else
				{
					fileData = defaultData;
				}

				return fileData;
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Failed to get json data. Err: {0}", e.ToString()));
				return defaultData;
			}
		}

		private static void DownloadFile(string url, string path)
		{
			try
			{
				string newPath = path + ".new";

				if (File.Exists(newPath))
				{
					File.Delete(newPath);
				}

				Logger.Info("Downloading latest json: " + newPath);

				WebClient w = new WebClient();
				w.DownloadFile(url, newPath);

				if (ValidJsonFile(newPath))
				{
					if (File.Exists(path))
						File.Delete(path);
					File.Move(newPath, path);
				}
				else
				{
					Logger.Error("Downloaded json is not valid");
				}
			}
			catch
			{
			}
		}

		public static bool ValidJsonFile(string path)
		{
			try
			{
				StreamReader sr = new StreamReader(path);
				string fileData = sr.ReadToEnd();
				sr.Close();

				JSonReader readjson = new JSonReader();
				IJSonObject fullJson = readjson.ReadAsJSonObject(fileData);
				return true;
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				return false;
			}
		}
		private static string GetAllAppsJsonString()
		{
			string channelAppsJsonString = GetChannelAppsJson();
			JSonReader readjson = new JSonReader();
			IJSonObject channelAppsJson = readjson.ReadAsJSonObject(channelAppsJsonString);

			string webAppsJsonString = GetWebAppsJson();
			readjson = new JSonReader();
			IJSonObject webAppsJson = readjson.ReadAsJSonObject(webAppsJsonString);

			JSonWriter writer = new JSonWriter(true);
			writer.WriteArrayBegin();
			for (int i = 0; i < channelAppsJson.Length; i++)
			{
				writer.Write(channelAppsJson[i]);
			}

			for (int i = 0; i < webAppsJson.Length; i++)
			{
				writer.Write(webAppsJson[i]);
			}

			writer.WriteArrayEnd();
			string data = writer.ToString();
			return data;
		}

		public static string GetLocaleName()
		{
			return CultureInfo.CurrentCulture.ToString();
		}

		public static string GetAvailableLocaleName()
		{
			string baseName = "en-US";
			try
			{
				baseName = CultureInfo.CurrentCulture.ToString();
				string localeDir = GetCurrentThemeLocalDir();
				string filePath = Path.Combine(localeDir, "i18n");
				filePath = Path.Combine(filePath, baseName + ".json");
				Logger.Info("Checking for localized file: " + filePath);
				if (File.Exists(filePath))
				{
					return baseName;
				}
				else
				{
					baseName = "en-US";
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed to check for locale file. error: " + e.ToString());
				baseName = "en-US";
			}

			return baseName;
		}

		public static void setInterests(string interestString)
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			key.SetValue("Interests", interestString);
		}

		public static string getInterests()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			return key.GetValue("Interests", "").ToString();
		}

		public static void ReportProblem()
		{
			RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
			string installDir = (string)reg.GetValue("InstallDir");
			ProcessStartInfo proc = new ProcessStartInfo();
			proc.FileName = installDir + "HD-LogCollector.exe";
			Logger.Info("SysTray: Starting " + proc.FileName);
			Process.Start(proc);

		}
		public static void setUserName(string name)
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			key.SetValue("UserName", name);
		}

		public static string getUserName()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			return key.GetValue("UserName", "").ToString();
		}

		public static void ReLaunchStreamWindow()
		{
			RegistryKey gmConfigKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			int gmPort = (int)gmConfigKey.GetValue("PartnerServerPort", 2871);
			string url = String.Format("http://127.0.0.1:{0}/{1}", gmPort.ToString(), "relaunchstreamwindow");

			try
			{
				string r = Common.HTTP.Client.Get(url, null, false, 1000);
				Logger.Info(r);
			}
			catch (Exception ex)
			{
				Logger.Error("Error Relaunching stream window... Err : " + ex.ToString());
				Environment.Exit(0);
			}
		}

		public static bool isAddStreamindow = false;
		internal static void ShowStreamWindow()
		{
			if (StreamWindow.Instance != null)
			{
				StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
				{
					if (StreamWindow.Instance.WindowState == System.Windows.WindowState.Minimized)
					{
						StreamWindow.Instance.WindowState = System.Windows.WindowState.Normal;
					}
					StreamWindow.Instance.Activate();
					StreamWindow.Instance.Topmost = true;
					StreamWindow.Instance.Topmost = false;
					StreamWindow.Instance.Focus();
				}));
			}
		
		}

		internal static string GetStreamWindowUrl()
		{
			string url = sStreamWindowProd2Html;
			string mShowLogo = "null";
			string mNumRecVideos = "null";
			string mGoLiveEnabled = "null";

			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			mShowLogo = (string)configKey.GetValue("ShowLogo", mShowLogo);
			mNumRecVideos = (string)configKey.GetValue("NumRecVideos", mNumRecVideos);
			mGoLiveEnabled = (string)configKey.GetValue("GoLiveEnabled", mGoLiveEnabled);
			url = String.Format("{0}?logo={1}&videos={2}&streaming={3}",
					url, mShowLogo, mNumRecVideos, mGoLiveEnabled);

			if (configKey.GetValue("StreamWindowUrl", null) != null)
				url = (string) configKey.GetValue("StreamWindowUrl");

			return url;
		}

		public static string GetChannelName()
		{
			RegistryKey filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath);
			return (string)filterKey.GetValue("channelName");
		}

		public static void ClosingWebTab(string tabTitle)
		{
			Logger.Info("Closing tab: " + tabTitle);
			object[] args = { tabTitle };
			StreamWindow.Instance.mBrowser.CallJs("tabClosing", args);
		}
	}
}

