using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using System.Windows.Forms;

using Microsoft.Win32;
using System.Linq;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using System.Globalization;

namespace BlueStacks.hyperDroid.GameManager
{
	public class StreamManager
	{
		public static string sObsServerBaseURL		= "http://localhost:2891";

		private Queue<ObsCommand> mObsCommandQueue;
		private Object mObsCommandQueueObject		= new Object();
		private Object mObsSendRequestObject		= new Object();
		private Object mInitOBSLock			= new Object();
		private EventWaitHandle mObsCommandEventHandle;

		private Browser	mBrowser;
		public static StreamManager sStreamManager;
		public static bool sStopInitOBSQueue = false;
		public string	mCallbackStreamStatus;
		public string	mCallbackAppInfo;
		public bool	mIsObsRunning			= false;
		public bool	mIsStreaming			= false;
		public bool	mStoppingOBS			= false;
		private bool	mIsReconnecting			= false;
		private bool	mIsStreamStarted		= false;
		private string	mFailureReason;
		private int	mMicVolume;
		private int	mSystemVolume;
		public bool    mCLRBrowserRunning              = false;
		public string    mCurrentFilterAppPkg;
		private object stoppingOBSLock = new object();

		public string	mCamStatus;
		public bool	mReplayBufferEnabled		= false;

		public StreamManager(Browser browser)
		{
			mBrowser = browser;
			sStreamManager = this;
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string replayBufferEnabled = (string)configKey.GetValue("ReplayBufferEnabled", "false");
			mReplayBufferEnabled = (replayBufferEnabled == "true");
			mCamStatus = (string)configKey.GetValue("CamStatus", "false");
			sStopInitOBSQueue = false;

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
			if (Utils.IsOSWinXP())
				return;

			Utils.KillProcessByName("HD-OBS");

			if (Utils.FindProcessByName("HD-OBS") == false && !sStopInitOBSQueue)
				StartOBS();

			if (sStopInitOBSQueue)
				return;

			try
			{
				SendObsRequestInternal("ping", null);
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

			Thread commandQueueThread = new Thread(delegate()
			{
				ProcessObsCommandQueue();
			});
			commandQueueThread.IsBackground = true;
			commandQueueThread.Start();
	
			UIHelper.RunOnUIThread(GameManager.sGameManager, delegate () { SetHwnd(); });
			SetSavePath();

			if (mReplayBufferEnabled)
				SetReplayBufferSavePath();

			SetConfig();
			GetVolumes();

			string appPkg = FilterUtility.GetCurrentAppPkg();
			if (appPkg != null && FilterUtility.IsFilterApplicableApp(appPkg))
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

			Thread pollingThread = new Thread(delegate()
			{
				StartPollingOBS();
			});
			pollingThread.IsBackground = true;
			pollingThread.Start();
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
			
			string url = GameManager.sBaseUrl + "filters/theme/" + appPkg + "/" + currentTheme + "/index.html?" + queryParam; 

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
				DisableWebcam();

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
			if (GameManager.sGameManager.FullScreen)
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
			SendObsRequest("setcameraposition", data, null, null, 0, false);
			EnableWebcamInternal(cameraWidth.ToString(), cameraHeight.ToString(), "0");
		}
			
		public void SetStreamDimension(out int startX, out int startY, out int width, out int height)
		{
			if (GameManager.sGameManager.FullScreen)
			{
				startX = 0;
				width = GameManager.sGameManager.Width;
			}
			else
			{
				startX = GameManager.mBorderWidth + GameManager.TransparentBox.Width;
				width = GameManager.sGameManager.Width - 2 * GameManager.mBorderWidth -
					GameManager.TransparentBox.Width;
			}

			startY = GameManager.mBorderWidth + GameManager.sTabBarHeight + GameManager.mCenterBorderHeight;
			height = GameManager.sGameManager.Height - 2 * GameManager.mBorderWidth -
				GameManager.sTabBarHeight - GameManager.mCenterBorderHeight;
		}

		private void StartOBS()
		{
			Process proc = new Process();
			proc.StartInfo.FileName = @"OBS\HD-OBS.exe";
			proc.StartInfo.Arguments = "";
			proc.Start();
			proc.WaitForInputIdle();
			Thread.Sleep(1000);
		}

		private void SetHwnd()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("hwnd", GameManager.sGameManager.Handle.ToString());
			SendObsRequest("sethwnd", data, null, null, 0, false);
		}

		private void SetSavePath()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			string savePath = Path.Combine(GameManager.sServerRootDir, "stream.flv");
			data.Add("savepath", savePath);
			SendObsRequest("setsavepath", data, null, null, 0, false);
		}

		private void SetReplayBufferSavePath()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			string savePath = Path.Combine(GameManager.sServerRootDir, "replay.flv");
			data.Add("savepath", savePath);
			SendObsRequest("setreplaybuffersavepath", data, null, null, 0, false);
		}

		internal void ReportObsError(string errorReason)
		{
			string eventType = "stream_interrupted_error";

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
				UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {

					if (GameManager.sGameManager.mStreamWindow != null)
						GameManager.sGameManager.mStreamWindow.CloseWindow();
					MessageBox.Show(
						GameManager.sLocalizedString["OBS_ALREADY_RUNNING"],
						"BlueStacks Error",
						MessageBoxButtons.OK,
						MessageBoxIcon.Error
					);
					StreamManager.sStopInitOBSQueue = false;
					GameManager.sGameManager.mStreamWindow = null;
					GameManager.sGameManager.ShowStreamWindow();
				});
			}
			else if (errorReason.StartsWith("AccessDenied") || errorReason.StartsWith("ConnectServerError"))
			{
				errorReason = "Error starting stream : " + errorReason;
				ReportStreamStatsToCloud(eventType, errorReason);
				UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {
					GameManager.sGameManager.mStreamWindow.CloseWindow();
					DialogResult result = MessageBox.Show(
						GameManager.sLocalizedString["OBS_ERROR_TEXT"],
						"BlueStacks Error",
						MessageBoxButtons.YesNo,
						MessageBoxIcon.Error
					);
					if (result == DialogResult.Yes)
						GameManager.sGameManager.ShowStreamWindow();
				});
			}
			else if (errorReason.Equals("ConnectionSuccessfull"))
			{
				errorReason = errorReason;
				ReportStreamStatsToCloud(eventType, errorReason);
			}
			else
			{
				errorReason = "Error starting stream : " + errorReason;
				ReportStreamStatsToCloud(eventType, errorReason);
			}
		}

		private void GetVolumes()
		{
			SendObsRequest("getmicvolume", null, "SetMicVolumeLocal", null, 0, false);
			SendObsRequest("getsystemvolume", null, "SetSystemVolumeLocal", null, 0, false);
		}

		public void DisableCLRBrowser()
		{
			SendObsRequest("disableclrbrowser", null, null, null, 0, false);
		}
		
		public void EnableCLRBrowser()
		{
			SendObsRequest("enableclrbrowser", null, null, null, 0, false);
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
		 * 0 - to use default position
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
			else
				EnableWebcamInternal(width, height, position);
		}

		public void EnableWebcamInternal(string width, string height, string position)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width);
			data.Add("height", height);
			data.Add("position", position);
			SendObsRequest("enablewebcam", data, "WebcamConfigured", null, 0, false);
		}

		public void DisableWebcam()
		{
			DisableWebcamInternal();
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
					mCamStatus = camStatus.ToString();
					RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
					key.SetValue("CamStatus", mCamStatus);
					key.Close();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error in SetWebcamReg. response: " + response);
				Logger.Error(e.ToString());
			}
		}

		public void SetConfig()
		{
			Dictionary<string, string> data = GetObsArgs();
			SendObsRequest("setconfig", data, null, null, 0, false);
		}

		public void SetFrontendPosition()
		{
			int startX, startY, width, height;
			SetStreamDimension(out startX, out startY, out width, out height);

			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath);
			if (FilterUtility.IsFilterApplicableApp())
			{
				int frontendWidth = (int) configKey.GetValue("GuestOpenGlWidth", width);
				int frontendHeight = (int) configKey.GetValue("GuestOpenGlHeight", height);
				startX = startX + (width-frontendWidth)/2;
				SetFrontendPosition(startX, startY, frontendWidth, frontendHeight);
			}
			else
				SetFrontendPosition(startX, startY, width, height);
		}

		public void SetFrontendPosition(int width, int height)
		{
			int startX, startY, windowWidth, windowHeight;
			SetStreamDimension(out startX, out startY, out windowWidth, out windowHeight);
			if (FilterUtility.IsFilterApplicableApp())
			{
				startX = startX + (windowWidth-width)/2;
				SetFrontendPosition(startX, startY, width, height);
			}
			else
				SetFrontendPosition(startX, startY, windowWidth, windowHeight);
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

		public void StartStream(string key, string location,
				string callbackStreamStatus, string callbackAppInfo)
		{
			mCallbackStreamStatus = callbackStreamStatus;
			mCallbackAppInfo = callbackAppInfo;

			SetStreamSettings(key, location);
			SendObsRequest("startstream", null, "StreamStarted", null, 0);
		}

		public void StopStream()
		{
			Logger.Info("StopStream called");
			SendObsRequest("stopstream", null, "StreamStopped", null, 0);
			GameManager.sGameManager.mToolBarForm.HandleGoLiveButton("on");
		}

		public void StartRecord()
		{
			lock (stoppingOBSLock)
			{
				if (mIsObsRunning)
				{
					string appPkg = FilterUtility.GetCurrentAppPkg();
					if (appPkg != null && FilterUtility.IsFilterApplicableApp(appPkg))
					{
						string currentTheme = FilterUtility.GetCurrentTheme(appPkg);
						InitCLRBrowser(appPkg, currentTheme);
					}
					else
					{
						ResetCLRBrowser();
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

		private void StreamStarted(string response)
		{
			GameManager.sRecording = true;
			GameManager.sStreaming = true;
			GameManager.sGameManager.mControlBarRight.DisableFullScreenButton();
			GameManager.sGameManager.mStreamWindow.StreamStarted();

			string[] currentTabData = TabBar.sTabBar.GetCurrentTabData();
			SendAppInfo(currentTabData[0], currentTabData[1], currentTabData[2]);
		}

		private void StreamStopped(string response)
		{
			GameManager.sStreaming = false;
			GameManager.sRecording = false;
			mIsStreaming = false;
			mIsStreamStarted = false;

			GameManager.sGameManager.mControlBarRight.EnableFullScreenButton();
			GameManager.sGameManager.EnableGameManagerResizeButton();
			GameManager.sGameManager.mStreamWindow.StreamEnded();

			mStoppingOBS = true;
			SendObsRequest("close", null, "KillOBS", "KillOBS", 0);
		}

		public void KillOBS(string response)
		{
			lock (stoppingOBSLock)
			{
				try
				{
					/*
					 * Wait for HD-OBS to stop gracefully
					 * for 3 sec
					 */
					int retry = 0;
					int RETRY_MAX = 15;
					while (retry < RETRY_MAX)
					{
						if (Process.GetProcessesByName("HD-OBS").Length == 0)
						{
							break;
						}
						retry ++;
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
			GameManager.sRecording = true;

			if (GameManager.sEnableFullScreenButton)
			{
				GameManager.sGameManager.mControlBarRight.EnableFullScreenButton();
				GameManager.sEnableFullScreenButton = false;
			}

			UIHelper.RunOnUIThread(GameManager.sGameManager.mStreamWindow, delegate() {
				GameManager.sGameManager.mStreamWindow.EvaluateJS("getStreamConfigJson('" +GetStreamConfig() + "');");
			});
		}

		private void RecordStopped(string response)
		{
			GameManager.sRecording = false;
		}

		public void StartReplayBuffer()
		{
			SendObsRequest("startreplaybuffer", null, null, null, 0);
		}

		public void StopReplayBuffer()
		{
			SendObsRequest("stopreplaybuffer", null, null, null, 2000);
		}

		private void SetStreamSettings(string key, string location)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("playPath", key);
			data.Add("url", location);

			SendObsRequest("setstreamsettings", data, null, null, 0);
		}

		private Dictionary<string, string> GetObsArgs()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();

			int startX = 0;
			int width = GameManager.sGameManager.Width;

			if (!GameManager.sGameManager.FullScreen)
			{
				startX = GameManager.mBorderWidth + GameManager.TransparentBox.Width;
				width = GameManager.sGameManager.Width - 2 * GameManager.mBorderWidth -
					GameManager.TransparentBox.Width;
			}

			int startY = GameManager.mBorderWidth + GameManager.sTabBarHeight + GameManager.mCenterBorderHeight;
			int height = GameManager.sGameManager.Height - 2 * GameManager.mBorderWidth -
				GameManager.sTabBarHeight - GameManager.mCenterBorderHeight;
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

			float downscale = (float)height/streamHeight;

			data.Clear();
			data.Add("startX", startX.ToString());
			data.Add("startY", startY.ToString());
			data.Add("width", width.ToString());
			data.Add("height", height.ToString());
			data.Add("x264Profile", x264Profile);
			data.Add("maxBitrate", maxBitrate.ToString());
			data.Add("downscale", downscale.ToString());

			return data;
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
						if (streaming)
						{
							GameManager.sGameManager.mToolBarForm.HandleGoLiveButton("live");
						}
						else
						{
							GameManager.sGameManager.mToolBarForm.HandleGoLiveButton("on");
						}
					}

					mIsStreaming = streaming;
					mIsReconnecting = reconnecting;
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
						if (!mStoppingOBS) {
							UpdateFailureReason();
						}
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
				string s = "";
				string format = "yyyy-MM-dd-HHmm-ss";
				DateTime dt = DateTime.MinValue;
				foreach (string path in Directory.GetFiles(@"OBS\Logs\"))
				{
					DateTime dtTemp;
					s = Path.GetFileNameWithoutExtension(path.Replace(@"OBS\Logs\", string.Empty));
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
					string fileName = @"OBS\Logs\" + dt.ToString("yyyy-MM-dd-HHmm-ss") + ".log";
					s = File.ReadAllLines(fileName).Last();
				}
				mFailureReason = "OBS crashed: " + s;
				ReportObsError(mFailureReason);
			}
		}

		/*private void SendStreamStatus(bool streaming, bool reconnecting)
		{
			if (mCallbackStreamStatus == null)
				return;

			JSonWriter status = new JSonWriter();
			status.WriteObjectBegin();
			status.WriteMember("obs", mIsObsRunning);
			status.WriteMember("streaming", streaming);
			status.WriteMember("reconnecting", reconnecting);
			status.WriteMember("reason", mFailureReason);
			status.WriteObjectEnd();

			object[] args = {status.ToString()};
			mBrowser.CallJs(mCallbackStreamStatus, args);
		}*/

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

                        CustomLogger.Info("Params: "+"\""+subkey+"\" \""+randomString+"\"");

                        Process proc = new Process();
                        proc.StartInfo.UseShellExecute = false;
                        proc.StartInfo.CreateNoWindow = true;
                        proc.StartInfo.FileName = Path.Combine(installDir, "HD-CloudPost.exe");
                        proc.StartInfo.Arguments = "\""+subkey+"\" \""+randomString+"\"";
                        proc.Start();
		}

		public void ResizeStream()
		{
			if (mObsCommandQueue != null)
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("width", GameManager.sGameManager.Width.ToString());
				data.Add("height", GameManager.sGameManager.Height.ToString());
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
			if (GameManager.sGameManager.mToolBarForm != null)
			{
				GameManager.sGameManager.mToolBarForm.HandleGoLiveButton(string.Empty);
			}
		}

		public static void StopOBS()
		{
			GameManager.sGameManager.mStreamWindow.Close();

			while (Process.GetProcessesByName("HD-OBS").Length != 0)
			{
				Thread.Sleep(500);
			}

		}

		public void CloseFailed()
		{
			Utils.KillProcessByName("HD-OBS");
		}

		public void TabChanged(string[] tabChangedData)
		{
			SendAppInfo(tabChangedData[0], tabChangedData[1], tabChangedData[2]);
		}

		private void SendAppInfo(string type, string name, string data)
		{
			if (mCallbackAppInfo == null)
				return;

			JSonWriter info = new JSonWriter();
			info.WriteObjectBegin();
			info.WriteMember("type", type);
			info.WriteMember("name", name);
			info.WriteMember("data", data);
			info.WriteObjectEnd();

			object[] args = {info.ToString()};
			mBrowser.CallJs(mCallbackAppInfo, args);
		}

		public string GetStreamConfig()
		{
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string streamName = (string)configKey.GetValue("StreamName", "Streaming from BlueStacks");
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

		public void SaveReplayBuffer()
		{
			SendObsRequest("savereplaybuffer", null, null, null, 0);
		}

		public void ReplayBufferSaved()
		{
			UIHelper.RunOnUIThread(TabBar.sTabBar, delegate() {
					SaveFileDialog saveFileDialog = new SaveFileDialog();

					saveFileDialog.Filter = "Flash Video (*.flv)|*.flv";
					saveFileDialog.FilterIndex = 1;
					saveFileDialog.RestoreDirectory = true;
					saveFileDialog.FileName = "Replay";

					if (saveFileDialog.ShowDialog() == DialogResult.OK)
					{
						string filePath = saveFileDialog.FileName;

						string replayFileName = "replay.flv";
						string replayFilePath = Path.Combine(Common.Strings.GameManagerHomeDir, replayFileName);

						File.Copy(replayFilePath, filePath);
					}
				});
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
			if (data!=null)
				data.Add("randomVal", "0");
			Thread commandQueueThread = new Thread(delegate()
			{
				ObsCommand command = new ObsCommand(request, data,
					responseCallback, failureCallback, pauseTime);

				if (mObsCommandQueue == null)
				{
					mObsCommandQueue = new Queue<ObsCommand>();
					lock (mInitOBSLock)
					{
						InitObs();
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
				string response = Common.HTTP.Client.Post(url, data, null, false, 5000);
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
							method.Invoke(this, new object[]{response});
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
							method.Invoke(this, new object[]{});
						}
					}

					Thread.Sleep(command.mPauseTime);
				}
			}
		}

		class ObsCommand
		{
			public string				mRequest;
			public Dictionary<string, string>	mData;
			public string				mResponseCallback;
			public string				mFailureCallback;
			public int				mPauseTime;

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
