using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using System.Runtime.InteropServices;

using Microsoft.Win32;
using System.Linq;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using System.Globalization;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class StreamManager
	{
		public static string sObsServerBaseURL = "http://localhost:2891";

		public static string DEFAULT_NETWORK = "twitch";
		public static bool DEFAULT_ENABLE_FILTER = true;
		public static bool DEFAULT_SQUARE_THEME = false;
		public static string DEFAULT_LAYOUT_THEME = null;

		private static Queue<ObsCommand> mObsCommandQueue;
		private Object mObsCommandQueueObject = new Object();
		private Object mObsSendRequestObject = new Object();
		private Object mInitOBSLock = new Object();
		private EventWaitHandle mObsCommandEventHandle;

		public static bool sStopInitOBSQueue = false;
		public string mCallbackStreamStatus;
		public string mCallbackAppInfo;
		public bool mIsObsRunning = false;
		public bool mIsInitCalled = false;
		public bool mIsStreaming = false;
		public bool mStoppingOBS = false;
		public bool mIsReconnecting = false;
		private bool mIsStreamStarted = false;
		private string mFailureReason = "";
		private static int mMicVolume;

		private string mNetwork = DEFAULT_NETWORK;
		public bool mSquareTheme = DEFAULT_SQUARE_THEME;
		public string mLayoutTheme = DEFAULT_LAYOUT_THEME;
		public string mLastCameraLayoutTheme = DEFAULT_LAYOUT_THEME;
		public bool mAppViewLayout = false;
		public bool mEnableFilter = DEFAULT_ENABLE_FILTER;
		private static int mSystemVolume;
		public static Dictionary<string, string> sLocalizedString = new Dictionary<string, string>();

		public static string mCamStatus;
		public bool mReplayBufferEnabled = false;
		private string mAppHandle = "";
		private string mAppPid = "";
		public bool mCLRBrowserRunning = false;
		public string mCurrentFilterAppPkg;
		private object stoppingOBSLock = new object();

		private Browser mBrowser;
		public static StreamManager Instance = null;

		public StreamManager(Browser browser)
		{
			Instance = this;
			mBrowser = browser;
			sLocalizedString = Locale.Strings.InitLocalization(null);
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string replayBufferEnabled = (string)configKey.GetValue("ReplayBufferEnabled", "false");
			mReplayBufferEnabled = (replayBufferEnabled == "true");

			RegistryKey key = App.sConfigKey;
			mCamStatus = (string)key.GetValue("CamStatus", "false");

			RegistryKey filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath);
			if (filterKey == null)
			{
				RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath, true);
				regKey.CreateSubKey("Filter");
				regKey.Close();
			}
			else
				filterKey.Close();

			mObsCommandEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
		}

		private void InitObs()
		{
			mIsInitCalled = true;
			if (Utils.IsOSWinXP())
				return;

			Utils.KillProcessByName("HD-OBS");

			if (Utils.FindProcessByName("HD-OBS") == false && !sStopInitOBSQueue)
				StartOBS();

			if (sStopInitOBSQueue)
				return;

			try
			{
				string response = SendObsRequestInternal("ping", null);
				Logger.Info("reponse for ping is {0}", response);
				mIsObsRunning = true;
			}
			catch (Exception e)
			{
				if (sStopInitOBSQueue)
					return;
				Logger.Error(e.ToString());
				Thread.Sleep(100);
				InitObs();
				return;
			}

			Thread commandQueueThread = new Thread(delegate ()
			{
				ProcessObsCommandQueue();
			});
			commandQueueThread.IsBackground = true;
			commandQueueThread.Start();

			SetHwnd(mAppHandle);
			SetSavePath();

			if (mReplayBufferEnabled)
				SetReplayBufferSavePath();

			GetVolumes();
			int startX, startY, width, height;
			StreamManager.Instance.SetStreamDimension(out startX, out startY, out width, out height);

			if (mSquareTheme)
				StreamManager.Instance.SetSquareConfig(startX, startY, width, height);
			else
				StreamManager.Instance.SetConfig(startX, startY, width, height);

			EnableSource("BlueStacks");
			SetSceneConfiguration(mLayoutTheme);

			if (mLayoutTheme == null)
			{
				string appPkg = FilterUtility.GetCurrentAppPkg();
				if (appPkg != null && mEnableFilter && FilterUtility.IsFilterApplicableApp(appPkg))
				{
					string currentTheme = FilterUtility.GetCurrentTheme(appPkg);
					if (currentTheme != null)
						InitCLRBrowser(appPkg, currentTheme);
					else
						ResetCLRBrowser();
				}
				else
				{
					ResetCLRBrowser();
				}
			}

			Thread pollingThread = new Thread(delegate ()
			{
				StartPollingOBS();
			});
			pollingThread.IsBackground = true;
			pollingThread.Start();
		}

		public void SetSceneConfiguration(string layoutTheme)
		{
			mAppViewLayout = false;
			mLayoutTheme = layoutTheme;

			if (layoutTheme == null)
			{
				EnableSource("BlueStacks");
				SendObsRequest("resettooriginalscene", null, null, null, 0, false);
			}
			else
			{
				FilterUtility.UpdateLayout("LayoutTheme", layoutTheme);
				FilterUtility.UpdateAppViewLayoutRegistry(mAppViewLayout);
				SetFrontendPosition();
				DisableCLRBrowser();
				try
				{
					JSonReader reader = new JSonReader();
					IJSonObject obj = reader.ReadAsJSonObject(layoutTheme);
					Logger.Info(layoutTheme);

					bool isPortraitApp = IsPortraitApp();
					if (obj.Contains("isPortrait") && !obj["isPortrait"].IsNull)
						isPortraitApp = Convert.ToBoolean(obj["isPortrait"].StringValue);

					IJSonObject requiredObj = null;
					if (isPortraitApp)
						requiredObj = (IJSonObject) obj["portrait"];
					else
						requiredObj = (IJSonObject) obj["landscape"];

					Dictionary<string, string> data = null;
					if (requiredObj.Contains("BlueStacksWebcam"))
					{
						bool enableWebCam = Convert.ToBoolean(requiredObj["BlueStacksWebcam"]["enableWebCam"].StringValue);
						if (enableWebCam)
						{
							int cameraStartX = Convert.ToInt32(requiredObj["BlueStacksWebcam"]["x"].StringValue);
							int cameraStartY = Convert.ToInt32(requiredObj["BlueStacksWebcam"]["y"].StringValue);
							int cameraWidth = Convert.ToInt32(requiredObj["BlueStacksWebcam"]["width"].StringValue);
							int cameraHeight = Convert.ToInt32(requiredObj["BlueStacksWebcam"]["height"].StringValue);
							int cameraActualWidth = Convert.ToInt32(requiredObj["BlueStacksWebcam"]["actualWidth"].StringValue);
							int cameraActualHeight = Convert.ToInt32(requiredObj["BlueStacksWebcam"]["actualHeight"].StringValue);

							data = new Dictionary<string, string>();
							data.Add("x", cameraStartX.ToString());
							data.Add("y", cameraStartY.ToString());
							data.Add("width", cameraWidth.ToString());
							data.Add("height", cameraHeight.ToString());
							data.Add("actualWidth", cameraActualWidth.ToString());
							data.Add("actualHeight", cameraActualHeight.ToString());
							data.Add("isPercentage", "1");
							data.Add("render", enableWebCam ? "1" : "0");
							SendObsRequest("setcameraposition", data, "WebcamConfigured", null, 0, false);
						}
						else
						{
							DisableWebcamInternal();
						}
					}

					if (requiredObj.Contains("BlueStacks"))
					{
						int blueStacksStartX = Convert.ToInt32(requiredObj["BlueStacks"]["x"].StringValue);
						int blueStacksStartY = Convert.ToInt32(requiredObj["BlueStacks"]["y"].StringValue);
						int blueStacksWidth = Convert.ToInt32(requiredObj["BlueStacks"]["width"].StringValue);
						int blueStacksHeight = Convert.ToInt32(requiredObj["BlueStacks"]["height"].StringValue);

						data = new Dictionary<string, string>();
						data.Add("x", blueStacksStartX.ToString());
						data.Add("y", blueStacksStartY.ToString());
						data.Add("width", blueStacksWidth.ToString());
						data.Add("height", blueStacksHeight.ToString());
						data.Add("source", "BlueStacks");
						SendObsRequest("setsourceposition", data, null, null, 0, false);

						EnableSource("BlueStacks");
					}
					else
					{
						DisableSource("BlueStacks");
					}
					
					string order = obj["order"].StringValue;
					string logo = obj["logo"].StringValue;

					data = new Dictionary<string, string>();
					data.Add("order", order);
					data.Add("logo", logo);
					SendObsRequest("setorderandlogo", data, null, null, 0, false);
				}
				catch (Exception ex)
				{
					Logger.Error("SetSceneConfiguration: Error {0}",ex);
				}
			}
		}

		public bool IsPortraitApp()
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "isportraitmode");
				string res = Common.HTTP.Client.Post(url, null, null, false);

				IJSonReader jsonReader = new JSonReader();
				IJSonObject obj = jsonReader.ReadAsJSonObject(res);

				if (obj.Contains("isPortrait"))
				{
					bool isPortrait = obj["isPortrait"].StringValue.Equals("true");
					Logger.Info("isPortrait mode: {0}", isPortrait);
					return isPortrait;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("IsPortaitApp Error: {0}", ex);
			}

			//will run in case there is error
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath);
			int frontendWidth = (int)configKey.GetValue("GuestOpenGlWidth", -1);
			int frontendHeight = (int)configKey.GetValue("GuestOpenGlHeight", -1);

			//simple method for detecting portrait app
			if (frontendWidth > frontendHeight)
				return false;
			else
				return true;
		}

		private void StartOBS()
		{
			RegistryKey gmKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
			string gmDir = (string)gmKey.GetValue("InstallDir");
			Process proc = new Process();
			if (Oem.Instance.OEM == "gamemanager" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "bluestacks")
				proc.StartInfo.FileName = Path.Combine(gmDir, @"OBS\HD-OBS.exe");
			else
				proc.StartInfo.FileName = Path.Combine(App.sServerRootDir, @"OBS\HD-OBS.exe");
			proc.StartInfo.Arguments = "";
			proc.Start();
			proc.WaitForInputIdle();
			Thread.Sleep(1000);
		}

		private void SetHwnd(string handle)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("hwnd", handle);
			SendObsRequest("sethwnd", data, null, null, 0, false);
		}

		private void SetSavePath()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			string savePath = Path.Combine(App.sServerRootDir, "stream.flv");
			data.Add("savepath", savePath);
			SendObsRequest("setsavepath", data, null, null, 0, false);
		}

		private void SetReplayBufferSavePath()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			string savePath = Path.Combine(App.sServerRootDir, "replay.flv");
			data.Add("savepath", savePath);
			SendObsRequest("setreplaybuffersavepath", data, null, null, 0, false);
		}

		public void DisableCLRBrowser()
		{
			SendObsRequest("disableclrbrowser", null, null, null, 0, false);
		}

		public void SetStreamDimension(out int startX, out int startY, out int width, out int height)
		{
			try
			{
				// Handling not done for full screen
				// not needed until side bar is not removed from full screen
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "streamdimension");
				string resp = Common.HTTP.Client.Get(url, null, false);
				JSonReader reader = new JSonReader();
				IJSonObject streamDim = reader.ReadAsJSonObject(resp);

				startX = Convert.ToInt32(streamDim["startX"].ToString());
				startY = Convert.ToInt32(streamDim["startY"].ToString());
				width = Convert.ToInt32(streamDim["width"].ToString());
				height = Convert.ToInt32(streamDim["height"].ToString());
			}
			catch (Exception ex)
			{
				Logger.Error("Got Exception in getting stream dimension... Err : " + ex.ToString());
				startX = startY = width = height = 0;
			}
		}

		public void SetFrontendPosition()
		{
			int startX, startY, width, height;
			SetStreamDimension(out startX, out startY, out width, out height);

			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath);
			if ((mEnableFilter && FilterUtility.IsFilterApplicableApp()) || mLayoutTheme != null)
			{
				int frontendWidth = (int)configKey.GetValue("GuestOpenGlWidth", width);
				int frontendHeight = (int)configKey.GetValue("GuestOpenGlHeight", height);
				startX = startX + (width - frontendWidth) / 2;
				SetFrontendPosition(startX, startY, frontendWidth, frontendHeight);
			}
			else
			{
				SetFrontendPosition(startX, startY, width, height);
			}
		}

		public void SetFrontendPosition(int width, int height)
		{
			int startX, startY, windowWidth, windowHeight;
			SetStreamDimension(out startX, out startY, out windowWidth, out windowHeight);
			if (FilterUtility.IsFilterApplicableApp())
				startX = startX + (windowWidth - width) / 2;
			SetFrontendPosition(startX, startY, width, height);
		}

		public void SetFrontendPosition(int startX, int startY, int width, int height)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width.ToString());
			data.Add("height", height.ToString());
			data.Add("y", startY.ToString());
			data.Add("x", startX.ToString());

			SendObsRequest("setfrontendposition", data, null, null, 0, false);
		}

		public void ResetCLRBrowser()
		{
			DisableCLRBrowser();
			SetFrontendPosition();

			if (String.Compare(mCamStatus, "true", true) == 0)
				EnableWebcamInternal("320", "240", "3");
			else
				DisableWebcamInternal();
			mCLRBrowserRunning = false;
			mCurrentFilterAppPkg = null;
		}

		public void InitCLRBrowser(string appPkg, string currentTheme)
		{
			mCurrentFilterAppPkg = appPkg;
			string channel = FilterUtility.GetChannelName();
			string queryParam = "channel=" + channel + "&client_id=" + FilterUtility.ClientId;

			mCLRBrowserRunning = true;
			FilterUtility.SetCurrentTheme(appPkg, currentTheme);

			queryParam += "&" + FilterUtility.GetQueryStringForTheme(appPkg, currentTheme);

			string url = App.sApplicationBaseUrl + "filters/theme/" + appPkg + "/" + currentTheme + "/index.html?" + queryParam;

			int startX, startY, width, height;
			SetStreamDimension(out startX, out startY, out width, out height);

			//Special Handling for camera
			FilterThemeConfig filterThemeConfig = FilterUtility.GetFilterThemeConfig(appPkg, currentTheme);
			if (String.Compare(mCamStatus, "true", true) == 0)
			{
				filterThemeConfig.mFilterThemeSettings.mIsWebCamOn = true;
				SetAndEnableWebCamPosition(filterThemeConfig, width, height);
			}
			else
			{
				filterThemeConfig.mFilterThemeSettings.mIsWebCamOn = false;
				DisableWebcamInternal();
			}

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width.ToString());
			data.Add("height", height.ToString());
			data.Add("url", url);
			SendObsRequest("setclrbrowserconfig", data, null, null, 0, false);

			Thread.Sleep(200);
			EnableCLRBrowser();
			SetFrontendPosition();
		}

		public FilterThemeConfig ChangeTheme(string appPkg, string theme)
		{
			if (!mCLRBrowserRunning)
				return null;

			mCurrentFilterAppPkg = appPkg;
			string channel = FilterUtility.GetChannelName();

			string queryParam = "channel=" + channel + "&client_id=" + FilterUtility.ClientId + "&";
			queryParam += FilterUtility.GetQueryStringForTheme(appPkg, theme);

			FilterUtility.SetCurrentTheme(appPkg, theme);
			FilterThemeConfig filterThemeConfig = FilterUtility.GetFilterThemeConfig(appPkg, theme);

			if (filterThemeConfig.mFilterThemeSettings.mIsWebCamOn)
				SetAndEnableWebCamPosition(filterThemeConfig);
			else
				DisableWebcamV2("{}");

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("queryParam", queryParam);
			data.Add("theme", theme);
			data.Add("appPkg", appPkg);
			FilterUtility.SendRequestToCLRBrowser("changetheme", data);

			return filterThemeConfig;
		}

		public void SetAndEnableWebCamPosition(FilterThemeConfig filterThemeConfig)
		{
			int startX, startY, width, height;
			SetStreamDimension(out startX, out startY, out width, out height);

			SetAndEnableWebCamPosition(filterThemeConfig, width, height);
		}

		public void SetAndEnableWebCamPosition(FilterThemeConfig filterThemeConfig, int windowWidth, int windowHeight)
		{
			//default camera position for filters
			//special handling for fullscreen window
			if (StreamWindowUtility.IsGMFullScreen())
			{
				int requiredWindowWidth = windowHeight * 16 / 9;
				if (windowWidth < requiredWindowWidth)
					windowWidth = requiredWindowWidth;
			}

			int cameraWidth = 320;
			int cameraHeight = 240;
			int cameraX = windowWidth - cameraWidth;
			int cameraY = windowHeight - cameraHeight;
			filterThemeConfig.mFilterThemeSettings.mIsWebCamOn = true;

			if (filterThemeConfig.mFilterThemeCameraSettings.width != 0
					&& filterThemeConfig.mFilterThemeCameraSettings.height != 0)
			{
				cameraWidth = (int)(filterThemeConfig.mFilterThemeCameraSettings.width * windowWidth * 0.01);
				cameraHeight = (int)(filterThemeConfig.mFilterThemeCameraSettings.height * windowHeight * 0.01);
				cameraX = (int)(filterThemeConfig.mFilterThemeCameraSettings.x * windowWidth * 0.01);
				cameraY = (int)(filterThemeConfig.mFilterThemeCameraSettings.y * windowHeight * 0.01);
			}
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", cameraWidth.ToString());
			data.Add("height", cameraHeight.ToString());
			data.Add("x", cameraX.ToString());
			data.Add("y", cameraY.ToString());
			data.Add("actualWidth", "320");
			data.Add("actualHeight", "240");
			data.Add("isPercentage", "0");
			data.Add("render", "1");
			SendObsRequest("setcameraposition", data, null, null, 0, false);
			EnableWebcamInternal(cameraWidth.ToString(), cameraHeight.ToString(), "0");
		}

		public void EnableCLRBrowser()
		{
			SendObsRequest("enableclrbrowser", null, null, null, 0, false);
		}

		public void SetClrBrowserConfig(string width, string height, string url)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width);
			data.Add("height", height);
			data.Add("url", url);
			SendObsRequest("setclrbrowserconfig", data, null, null, 0);
		}

		public void ObsErrorStatus(string erroReason)
		{
			mIsStreaming = false;
			mFailureReason = "Error starting stream : " + erroReason;
			SendStreamStatus(false, false);
		}

		public void ReportObsError(string errorReason)
		{
			try
			{
				string eventType = "stream_interrupted_error";
				Dictionary<string, string> data = new Dictionary<string, string>();
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "reportobserror");

				if (errorReason.Equals("ConnectionSuccessfull"))
				{
					if (!mIsStreamStarted)
					{
						mIsStreamStarted = true;
						eventType = "obs_connected";
					}
					else
					{
						eventType = "stream_resumed";
					}
				}
				else if (!mIsStreamStarted)
				{
					eventType = "went_live_error";
				}

				if (errorReason.Equals("OBSAlreadyRunning"))
				{
					eventType = "obs_already_running";
					ReportStreamStatsToCloud(eventType, errorReason);
					StreamManager.sStopInitOBSQueue = false;
					data.Add("reason", eventType);
					string res = Common.HTTP.Client.Post(url, data, null, false);
				}
				else if (errorReason.StartsWith("AccessDenied") || errorReason.StartsWith("ConnectServerError") || errorReason.Equals("obs_error"))
				{
					errorReason = "Error starting stream : " + errorReason;
					ReportStreamStatsToCloud(eventType, errorReason);
					data.Add("reason", "obs_error");
					string res = Common.HTTP.Client.Post(url, data, null, false);
				}
				else if (errorReason.Equals("ConnectionSuccessfull"))
				{
					ReportStreamStatsToCloud(eventType, errorReason);
				}
				else
				{
					errorReason = "Error starting stream : " + errorReason;
					ReportStreamStatsToCloud(eventType, errorReason);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to report obs error.. Err : " + ex.ToString());
			}
		}


		private void ReportStreamStatsToCloud(string eventType, string reason)
		{
			Logger.Info("StreamStats eventType: {0}, reason: {1}", eventType, reason);
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);

			string installDir = (string)key.GetValue("InstallDir");
			string url = Common.Utils.GetHostUrl() + "/stats/btvfunnelstats";
			string subkey = Common.Strings.GMPendingStats;
			string randomString = Guid.NewGuid().ToString();

			JSonWriter status = new JSonWriter();
			status.WriteObjectBegin();
			status.WriteMember("event_type", eventType);
			status.WriteMember("error_code", reason);
			status.WriteMember("guid", User.GUID);
			status.WriteMember("session_id", Stats.GetSessionId());
			status.WriteMember("prod_ver", Version.STRING);
			status.WriteMember("created_at", DateTime.UtcNow.ToString("yyyy-MM-dd HH:mm:ss"));
			status.WriteObjectEnd();

			JSonWriter writer = new JSonWriter();
			writer.WriteObjectBegin();
			writer.WriteMember("url", url);
			writer.WriteMember("isArray", false);
			writer.WriteMember("data", status.ToString());
			writer.WriteObjectEnd();

			RegistryKey statsKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPendingStats, true);
			if (statsKey == null)
			{
				RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath, true);
				baseKey.CreateSubKey("Stats");
				baseKey.Close();
				statsKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPendingStats, true);
			}

			statsKey.SetValue(randomString, writer.ToString(), RegistryValueKind.String);
			statsKey.Close();

			//Logger.Info("Params: " + "\"" + subkey + "\" \"" + randomString + "\"");

			Process proc = new Process();
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.FileName = Path.Combine(installDir, "HD-CloudPost.exe");
			proc.StartInfo.Arguments = "\"" + subkey + "\" \"" + randomString + "\"";
			proc.Start();
		}

		private void GetVolumes()
		{
			SendObsRequest("getmicvolume", null, "SetMicVolumeLocal", null, 0, false);
			SendObsRequest("getsystemvolume", null, "SetSystemVolumeLocal", null, 0, false);
		}

		private void SetMicVolumeLocal(string volumeResponse)
		{
			try
			{
				IJSonReader json = new JSonReader();
				IJSonObject res = json.ReadAsJSonObject(volumeResponse);
				mMicVolume = res["volume"].Int32Value;
			}
			catch (Exception e)
			{
				Logger.Error("Error in SetMicVolumeLocal. response: " + volumeResponse);
				Logger.Error(e.ToString());
			}
		}

		private void SetSystemVolumeLocal(string volumeResponse)
		{
			try
			{
				IJSonReader json = new JSonReader();
				IJSonObject res = json.ReadAsJSonObject(volumeResponse);
				mSystemVolume = res["volume"].Int32Value;
			}
			catch (Exception e)
			{
				Logger.Error("Error in SetSystemVolumeLocal. response: " + volumeResponse);
				Logger.Error(e.ToString());
			}
		}

		/*
		 * position of webcam can be
		 * 1 - top left
		 * 2 - top right
		 * 3 - bottom left
		 * 4 - bottom right
		 */
		public void EnableWebcam(string width, string height, string position)
		{
			if (mCLRBrowserRunning)
			{
				string currentTheme = FilterUtility.GetCurrentTheme(mCurrentFilterAppPkg);
				FilterThemeConfig filterThemeConfig = FilterUtility.GetFilterThemeConfig(mCurrentFilterAppPkg, currentTheme);
				SetAndEnableWebCamPosition(filterThemeConfig);

				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("settings", filterThemeConfig.mFilterThemeSettings.ToJsonString());
				FilterUtility.SendRequestToCLRBrowser("updatesettings", data);
			}
			else if (mLayoutTheme == null)
				EnableWebcamInternal(width, height, position);
			else
			{
				SetSceneConfiguration(mLastCameraLayoutTheme);
				StreamWindow.Instance.EvaluateJS("currentActiveLayout('" + LayoutWindow.GetLayoutName(mLayoutTheme) + "');");
			}
		}

		public void DisableSource(string source)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("source", source);
			data.Add("render", "0");
			
			SendObsRequest("setrender", data, null, null, 0, false);
		}

		public void EnableSource(string source)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("source", source);
			data.Add("render", "1");
			
			SendObsRequest("setrender", data, null, null, 0, false);
		}

		public void EnableWebcamInternal(string width, string height, string position)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width);
			data.Add("height", height);
			data.Add("position", position);
			SendObsRequest("enablewebcam", data, "WebcamConfigured", null, 0, false);
		}

		public void DisableWebcamV2(string jsonString)
		{
			if (mLayoutTheme != null)
			{
				mLastCameraLayoutTheme = mLayoutTheme;
				FilterUtility.UpdateLayout("LastCameraLayoutTheme", mLastCameraLayoutTheme);
				SetSceneConfiguration(jsonString);
				mAppViewLayout = true;
				FilterUtility.UpdateAppViewLayoutRegistry(mAppViewLayout);
				StreamWindow.Instance.EvaluateJS("currentActiveLayout('" + LayoutWindow.GetLayoutName(mLayoutTheme) + "');");
			}
			else
			{
				DisableWebcamInternal();
			}

			if (mCLRBrowserRunning)
			{
				string currentTheme = FilterUtility.GetCurrentTheme(mCurrentFilterAppPkg);
				FilterThemeConfig filterThemeConfig = FilterUtility.GetFilterThemeConfig(mCurrentFilterAppPkg, currentTheme);
				filterThemeConfig.mFilterThemeSettings.mIsWebCamOn = false;
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("settings", filterThemeConfig.mFilterThemeSettings.ToJsonString());
				FilterUtility.SendRequestToCLRBrowser("updatesettings", data);
			}
		}

		public void DisableWebcamInternal()
		{
			SendObsRequest("disablewebcam", null, "WebcamConfigured", null, 0, false);
		}

		private void WebcamConfigured(string response)
		{
			try
			{
				IJSonReader json = new JSonReader();
				IJSonObject res = json.ReadAsJSonObject(response);
				bool success = res["success"].BooleanValue;

				if (success)
				{
					bool camStatus = res["webcam"].BooleanValue;
					mCamStatus = camStatus.ToString().ToLower();
					RegistryKey key = App.sConfigKey;
					key.SetValue("CamStatus", mCamStatus);
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error in SetWebcamReg. response: " + response);
				Logger.Error(e.ToString());
			}
		}

		public void ResetFlvStream()
		{
			SendObsRequest("resetflvstream", null, null, null, 0);
		}

		public void SetSquareConfig(int startX, int startY, int width, int height)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();

			Logger.Info("Window size: ({0}, {1})", width, height);

			int streamWidth, streamHeight;
			Utils.GetStreamWidthAndHeight(width, height, out streamWidth, out streamHeight);
			Logger.Info("Stream size: ({0}, {1})", streamWidth, streamHeight);

			string x264Profile;
			int maxBitrate;
			if (streamHeight == 540)
			{
				x264Profile = "main";
				maxBitrate = 1200;
				width = 480;
				height = 480;
			}
			else if (streamHeight == 720)
			{
				x264Profile = "main";
				maxBitrate = 2500;
				width = 720;
				height = 720;
			}
			else
			{
				x264Profile = "high";
				maxBitrate = 3500;
				width = 720;
				height = 720;
			}

			//fixing downscale to 1
			float downscale = 1.0f;

			Logger.Info("x : " + startX);
			Logger.Info("y : " + startY);
			Logger.Info("width : " + width);
			Logger.Info("height : " + height);
			data.Clear();
			data.Add("startX", startX.ToString());
			data.Add("startY", startY.ToString());
			data.Add("width", width.ToString());
			data.Add("height", height.ToString());
			data.Add("x264Profile", x264Profile);
			data.Add("maxBitrate", maxBitrate.ToString());
			data.Add("downscale", downscale.ToString());
			SendObsRequest("setconfig", data, null, null, 0, false);
		}

		public void SetConfig(int startX, int startY, int width, int height)
		{
			if (mSquareTheme)
				SetSquareConfig(startX, startY, width, height);
			else
				SetDefaultConfig(startX, startY, width, height);
		}

		public void SetDefaultConfig(int startX, int startY, int width, int height)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();

			Logger.Info("Window size: ({0}, {1})", width, height);

			int streamWidth, streamHeight;
			Utils.GetStreamWidthAndHeight(width, height, out streamWidth, out streamHeight);
			Logger.Info("Stream size: ({0}, {1})", streamWidth, streamHeight);

			string x264Profile;
			int maxBitrate;
			if (streamHeight == 540)
			{
				x264Profile = "main";
				maxBitrate = 1200;
			}
			else if (streamHeight == 720)
			{
				x264Profile = "main";
				maxBitrate = 2500;
			}
			else
			{
				x264Profile = "high";
				maxBitrate = 3500;
			}

			float downscale = (float)height / streamHeight;

			Logger.Info("x : " + startX);
			Logger.Info("y : " + startY);
			Logger.Info("width : " + width);
			Logger.Info("height : " + height);
			data.Clear();
			data.Add("startX", startX.ToString());
			data.Add("startY", startY.ToString());
			data.Add("width", width.ToString());
			data.Add("height", height.ToString());
			data.Add("x264Profile", x264Profile);
			data.Add("maxBitrate", maxBitrate.ToString());
			data.Add("downscale", downscale.ToString());
			SendObsRequest("setconfig", data, null, null, 0, false);
		}
		
		public void StartStream(string key, string location,
				string callbackStreamStatus, string callbackAppInfo)
		{
			string service = "1";
			mCallbackStreamStatus = callbackStreamStatus;
			mCallbackAppInfo = callbackAppInfo;

			SetStreamSettings(service, key, location);
			SendObsRequest("startstream", null, "StreamStarted", null, 0);
		}

		public void StartStream(string jsonString,
				string callbackStreamStatus, string callbackAppInfo)
		{
			Logger.Info(jsonString);
			IJSonReader json = new JSonReader();
			IJSonObject res = json.ReadAsJSonObject(jsonString);
			String key = res["key"].StringValue;
			String service = res["service"].StringValue;
			String streamUrl = res["streamUrl"].StringValue;

			mCallbackStreamStatus = callbackStreamStatus;
			mCallbackAppInfo = callbackAppInfo;

			SetStreamSettings(service, key, streamUrl);
			SendObsRequest("startstream", null, "StreamStarted", null, 0);
		}

		public void StopStream()
		{
			SendObsRequest("stopstream", null, "StreamStopped", null, 0);
		}

		private void SendStatus(string path, Dictionary<string, string> data)
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, path);
				string res = Common.HTTP.Client.Post(url, data, null, false);
				Logger.Info("Successfully sent status for {0}", path);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to send post request for {0}... Err : {1}", path, ex.ToString());
			}
		}

		public void StartRecord()
		{
			StartRecord(mNetwork, mEnableFilter, mSquareTheme, mLayoutTheme);
		}

		public void StartRecord(string network, bool enableFilter, bool squareTheme, string layoutTheme)
		{
			lock (stoppingOBSLock)
			{
				mEnableFilter = enableFilter;
				mSquareTheme = squareTheme;
				if (layoutTheme != null)
				{
					RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
					mLayoutTheme = (string) configKey.GetValue("LayoutTheme", layoutTheme);
					mLastCameraLayoutTheme = (string) configKey.GetValue("LastCameraLayoutTheme", null);
					mAppViewLayout =  ((int)configKey.GetValue("AppViewLayout", 0) == 1);
					if (mLastCameraLayoutTheme == null)
						mLastCameraLayoutTheme = layoutTheme;
				}
				else
					mLayoutTheme = layoutTheme;
				mNetwork = network;

				if (mIsObsRunning)
				{
					int startX, startY, width, height;
					StreamManager.Instance.SetStreamDimension(out startX, out startY, out width, out height);
					SetConfig(startX, startY, width, height);

					SetSceneConfiguration(mLayoutTheme);
					if (layoutTheme == null)
					{
						string appPkg = FilterUtility.GetCurrentAppPkg();
						if (appPkg != null && mEnableFilter && FilterUtility.IsFilterApplicableApp(appPkg))
						{
							string currentTheme = FilterUtility.GetCurrentTheme(appPkg);
							InitCLRBrowser(appPkg, currentTheme);
						}
						else
						{
							ResetCLRBrowser();
						}
					}
				}
				SendObsRequest("startrecord", null, "RecordStarted", null, 0);
			}
		}

		public void StopRecord()
		{
			StopRecord(false);
		}

		public void StopRecord(bool immediate)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("immediate", immediate ? "1" : "0");
			SendObsRequest("stoprecord", data, "RecordStopped", null, 0);
		}

		public void SendAppInfo(string type, string name, string data)
		{
			if (mCallbackAppInfo == null)
				return;

			JSonWriter info = new JSonWriter();
			info.WriteObjectBegin();
			info.WriteMember("type", type);
			info.WriteMember("name", name);
			info.WriteMember("data", data);
			info.WriteObjectEnd();

			object[] args = { info.ToString() };
			mBrowser.CallJs(mCallbackAppInfo, args);
		}

		public static string GetStreamConfig()
		{
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string streamName = (string)configKey.GetValue("StreamName", "");
			string serverLocation = (string)configKey.GetValue("ServerLocation", "");

			JSonWriter config = new JSonWriter();
			config.WriteObjectBegin();
			config.WriteMember("streamName", streamName);
			config.WriteMember("camStatus", Convert.ToBoolean(mCamStatus));
			config.WriteMember("micVolume", mMicVolume);
			config.WriteMember("systemVolume", mSystemVolume);
			config.WriteMember("serverLocation", serverLocation);
			config.WriteObjectEnd();

			Logger.Info("GetStreamConfig: " + config.ToString());
			return config.ToString();

		}

		private void StreamStarted(string response)
		{
			SendStatus("streamstarted", null);
			mIsStreaming = true;
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				if (StreamWindow.Instance != null)
					StreamWindow.Instance.StreamStarted();
			}));
		}

		private void StreamStopped(string response)
		{
			SendStatus("streamstopped", null);
			mIsStreaming = false;
			mIsStreamStarted = false;
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				if (StreamWindow.Instance != null)
					StreamWindow.Instance.StreamEnded();
			}));
			SendObsRequest("close", null, null, null, 0);

			Thread thread = new Thread(delegate() {
				KillOBS();
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public void KillOBS()
		{
			if (mStoppingOBS)
				return;

			lock (stoppingOBSLock)
			{
				mStoppingOBS = true;
				try
				{
					/*
					 * Wait for HD-OBS to stop gracefully
					 * for 4 sec
					 */
					int retry = 0;
					int RETRY_MAX = 20;
					while (retry < RETRY_MAX)
					{
						if (Process.GetProcessesByName("HD-OBS").Length == 0)
						{
							break;
						}
						retry++;
						if (retry < RETRY_MAX)
						{
							Logger.Info("Waiting for HD-OBS to quit gracefully, retry: {0}", retry);
							Thread.Sleep(200);
						}
					}
					if (retry >= RETRY_MAX)
						Utils.KillProcessByName("HD-OBS");

					StartOBS();
				}
				catch (Exception ex)
				{
					Logger.Info("Failed to kill HD-OBS.exe...Err : " + ex.ToString());
				}
				mStoppingOBS = false;
			}
		}

		private void RecordStarted(string response)
		{
			SendStatus("recordstarted", null);
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				StreamWindow.Instance.EvaluateJS("getStreamConfigJson('" + GetStreamConfig() + "');");
			}));
		}

		private void RecordStopped(string response)
		{
			SendStatus("recordstopped", null);
		}

		public void StartReplayBuffer()
		{
			SendObsRequest("startreplaybuffer", null, null, null, 0);
		}

		public void StopReplayBuffer()
		{
			SendObsRequest("stopreplaybuffer", null, null, null, 2000);
		}

		private void SetStreamSettings(String service, string playPath, string url)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("service", service);
			data.Add("playPath", playPath);
			data.Add("url", url);

			SendObsRequest("setstreamsettings", data, null, null, 0);
		}

		public void SetSystemVolume(string level)
		{
			mSystemVolume = Convert.ToInt32(level);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("volume", level);

			SendObsRequest("setsystemvolume", data, null, null, 0);
		}

		public void SetMicVolume(string level)
		{
			mMicVolume = Convert.ToInt32(level);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("volume", level);

			SendObsRequest("setmicvolume", data, null, null, 0);
		}

		private void StartPollingOBS()
		{
			while (mIsObsRunning)
			{
				try
				{
					string response = SendObsRequestInternal("getstatus", null);
					response = Regex.Replace(response, @"\r\n?|\n", "");

					IJSonReader json = new JSonReader();
					IJSonObject res = json.ReadAsJSonObject(response);
					bool streaming = res["streaming"].BooleanValue;
					bool reconnecting = res["reconnecting"].BooleanValue;
					if (!streaming)
					{
						try
						{
							mFailureReason = res["reason"].StringValue;
						}
						catch
						{
						}
					}

					if (streaming != mIsStreaming)
					{
						SendStreamStatus(streaming, reconnecting);
					}

					mIsStreaming = streaming;
					mIsReconnecting = reconnecting;
					Dictionary<string, string> data = new Dictionary<string, string>();
					data.Add("isstreaming", Convert.ToString(mIsStreaming));
					SendStatus("streamstatus", data);
				}
				catch (Exception e)
				{
					Logger.Error(e.ToString());
					if (Utils.FindProcessByName("HD-OBS") == false)
					{
						mIsObsRunning = false;
						mIsStreaming = false;
						mIsReconnecting = false;
						mCLRBrowserRunning = false;
						mIsStreamStarted = false;
						if (!mStoppingOBS)
						{
							UpdateFailureReason();
						}
						SendStreamStatus(false, false);
						InitObs();
						mStoppingOBS = false;
						break;
					}
				}

				Thread.Sleep(5000);
			}
		}

		private void UpdateFailureReason()
		{
			if (string.IsNullOrEmpty(mFailureReason))
			{
				RegistryKey gmKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
				string gmDir = (string)gmKey.GetValue("InstallDir");
				string s = "";
				string format = "yyyy-MM-dd-HHmm-ss";
				DateTime dt = DateTime.MinValue;
				string logFilesDir = null;
				if (Oem.Instance.OEM == "gamemanager" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "bluestacks")
					logFilesDir = Path.Combine(gmDir, @"OBS\Logs\");
				else
					logFilesDir = Path.Combine(App.sServerRootDir, @"OBS\Logs\");
				foreach (string path in Directory.GetFiles(logFilesDir))
				{
					DateTime dtTemp;
					s = Path.GetFileNameWithoutExtension(path);
					if (DateTime.TryParseExact(s, format, CultureInfo.InvariantCulture, DateTimeStyles.None, out dtTemp))
					{
						if (dt < dtTemp)
						{
							dt = dtTemp;
						}
					}
				}
				if (!dt.Equals(DateTime.MinValue))
				{
					string fileName = Path.Combine(gmDir, @"OBS\Logs\") + dt.ToString("yyyy-MM-dd-HHmm-ss") + ".log";
					s = File.ReadAllLines(fileName).Last();
				}
				mFailureReason = "OBS crashed: " + s;
				ReportObsError(mFailureReason);
			}
		}

		private void SendStreamStatus(bool streaming, bool reconnecting)
		{
			Logger.Info("Sending stream status with data :: " +
					"streaming : " + streaming +
					", reconnecting : " + reconnecting +
					", obsRunning : " + mIsObsRunning +
					", failureReason : " + mFailureReason);
			try
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("obs", mIsObsRunning.ToString());
				data.Add("streaming", streaming.ToString());
				data.Add("reconnecting", reconnecting.ToString());
				data.Add("reason", mFailureReason.ToString());

				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "streamstatuscallback");

				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to send stream status... Err : " + ex.ToString());
			}

		}

		public void ResizeStream(string width, string height)
		{
			if (mObsCommandQueue != null)
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("width", width);
				data.Add("height", height);
				SendObsRequest("windowresized", data, null, null, 0);
			}
		}

		public void ShowObs()
		{
			SendObsRequest("show", null, null, null, 0);
		}

		public void HideObs()
		{
			SendObsRequest("hide", null, null, null, 0);
		}

		/*
		 * expected args for each edge:
		 * left: -1, 0
		 * top: 0, -1
		 * right: 1,0
		 * bottom: 0, 1
		 */
		public void MoveWebcam(string horizontal, string vertical)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("horizontal", horizontal);
			data.Add("vertical", vertical);
			SendObsRequest("movewebcam", data, null, null, 0);
		}

		public void Shutdown()
		{
			if (mObsCommandQueue != null)
			{
				mIsObsRunning = false;
				mIsStreamStarted = false;
				SendObsRequest("close", null, null, "CloseFailed", 0);
			}
		}

		public void CloseFailed()
		{
			Utils.KillProcessByName("HD-OBS");
		}

		public static void StopOBS()
		{
			//StreamWindow.Instance.Close();
			StreamWindow.Instance.Hide();

			StreamManager.Instance.mStoppingOBS = true;
			sStopInitOBSQueue = true;

			StreamManager.Instance.Shutdown();

			int count = 0;
			while (Process.GetProcessesByName("HD-OBS").Length != 0 && count < 20)
			{
				Thread.Sleep(500);
				count ++;
			}

			if (count == 20)
			{
				Logger.Info("Killing hd-obs as normal close failed");
				StreamManager.Instance.CloseFailed();
			}
		}

		public void SaveReplayBuffer()
		{
			SendObsRequest("savereplaybuffer", null, null, null, 0);
		}

		public void SetStreamName(string name)
		{
			RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			configKey.SetValue("StreamName", name);
			configKey.Close();
		}

		public void SetServerLocation(string location)
		{
			RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			configKey.SetValue("ServerLocation", location);
			configKey.Close();
		}

		public void ReplayBufferSaved()
		{
			SendStatus("replaybuffersaved", null);
		}

		public void SendObsRequest(string request,
				Dictionary<string, string> data,
				string responseCallback,
				string failureCallback,
				int pauseTime)
		{
			SendObsRequest(request, data, responseCallback, failureCallback, pauseTime, true);
		}

		public void SendObsRequest(string request,
				Dictionary<string, string> data,
				string responseCallback,
				string failureCallback,
				int pauseTime,
				bool waitForInit)
		{
			Logger.Info("got obs request: " + request);
			if (data != null)
				data.Add("randomVal", "0");

			Thread commandQueueThread = new Thread(delegate ()
			{
				ObsCommand command = new ObsCommand(request, data,
					responseCallback, failureCallback, pauseTime);

				if (mObsCommandQueue == null)
				{
					mObsCommandQueue = new Queue<ObsCommand>();
					lock (mInitOBSLock)
					{
						if (!mIsInitCalled)
						{
							InitObs();
						}
					}
				}

				if (waitForInit)
				{
					lock (mInitOBSLock)
					{
						lock (mObsCommandQueueObject)
						{
							mObsCommandQueue.Enqueue(command);
							mObsCommandEventHandle.Set();
						}
					}
				}
				else
				{
					lock (mObsCommandQueueObject)
					{
						mObsCommandQueue.Enqueue(command);
						mObsCommandEventHandle.Set();
					}
				}
			});
			commandQueueThread.IsBackground = true;
			commandQueueThread.Start();
		}

		private string SendObsRequestInternal(string request, Dictionary<string, string> data)
		{
			Logger.Info("waiting to send request: " + request);
			lock (mObsSendRequestObject)
			{
				string url = String.Format("{0}/{1}", sObsServerBaseURL, request);
				string response = Common.HTTP.Client.Post(url, data, null, false);
				return response;
			}
		}

		private void ProcessObsCommandQueue()
		{
			while (mIsObsRunning)
			{
				mObsCommandEventHandle.WaitOne();
				while (mObsCommandQueue.Count != 0)
				{
					ObsCommand command;
					lock (mObsCommandQueueObject)
					{
						if (mObsCommandQueue.Count == 0)
							break;

						command = mObsCommandQueue.Dequeue();
					}

					string response = "";
					try
					{
						response = SendObsRequestInternal(command.mRequest, command.mData);
						Logger.Info("Got response {0} for {1}", response, command.mRequest);
						if (command.mResponseCallback != null)
						{
							MethodInfo method = this.GetType().GetMethod(command.mResponseCallback,
									BindingFlags.Public |
									BindingFlags.NonPublic |
									BindingFlags.Instance);
							method.Invoke(this, new object[] { response });
						}
					}
					catch (Exception e)
					{
						Logger.Error("Exception when sending " + command.mRequest);
						Logger.Error(e.ToString());

						if (command.mFailureCallback != null)
						{
							MethodInfo method = this.GetType().GetMethod(command.mFailureCallback,
									BindingFlags.Public |
									BindingFlags.NonPublic |
									BindingFlags.Instance);
							method.Invoke(this, new object[] { });
						}
					}

					Thread.Sleep(command.mPauseTime);
				}
			}
		}

		public void Init(string appHandle, string pid)
		{
			Logger.Info("App Handle : {0} and Process Id : {1}", appHandle, pid);
			if (mAppHandle == "" && mAppPid == "")
			{
				mAppHandle = appHandle;
				mAppPid = pid;
				//InitObs();
			}
			else if (appHandle == mAppHandle && pid == mAppPid)
			{
				//InitObs();
			}
			else
			{
				//ReportObsError("CallingProcessMismatch");
			}
		}

		class ObsCommand
		{
			public string mRequest;
			public Dictionary<string, string> mData;
			public string mResponseCallback;
			public string mFailureCallback;
			public int mPauseTime;

			public ObsCommand(string request, Dictionary<string, string> data,
					string responseCallback, string failureCallback, int pauseTime)
			{
				mRequest = request;
				mData = data;
				mResponseCallback = responseCallback;
				mFailureCallback = failureCallback;
				mPauseTime = pauseTime;
			}
		}
	}
}
