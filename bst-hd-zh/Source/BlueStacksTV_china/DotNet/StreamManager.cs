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
		public static string sObsServerBaseURL		= "http://localhost:2891";

		private static Queue<ObsCommand> mObsCommandQueue;
		private Object mObsCommandQueueObject		= new Object();
		private Object mObsSendRequestObject		= new Object();
		private static EventWaitHandle sObsCommandEventHandle;

        public string mCallbackStreamStatus;
        public string mCallbackAppInfo;
		public bool	mIsObsRunning			= false;
		public bool	mIsStreaming			= false;
		public bool	mIsReconnecting			= false;
		public bool	mIsRecording			= false;
		private string	mFailureReason = "";
		private int	mMicVolume;
		private int	mSystemVolume;

		private string mCamStatus;
		private string mAppHandle                   = "";
		private string mAppPid						= "";
        public bool mCLRBrowserRunning = false;
        public string mCurrentFilterAppPkg;

        public static StreamManager Instance = null;

		public StreamManager()
		{
            RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			mCamStatus = (string)configKey.GetValue("CamStatus", "false");

			sObsCommandEventHandle = new EventWaitHandle(false, EventResetMode.AutoReset);
		}

		private void InitObs()
		{
			if (Utils.IsOSWinXP())
				return;

			Utils.KillProcessByName("HD-OBS");

			if (Utils.FindProcessByName("HD-OBS") == false)
				StartOBS();

			try
			{
				SendObsRequestInternal("ping", null);
				mIsObsRunning = true;
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				Thread.Sleep(100);
				InitObs();
				return;
			}
			//if (Oem.Instance.OEM == "gamemanager" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "bluestacks")
			//	App.CheckIfGMIsRunning();

			Thread commandQueueThread = new Thread(delegate()
			{
				ProcessObsCommandQueue();
			});
			commandQueueThread.IsBackground = true;
			commandQueueThread.Start();

            SetHwnd(mAppHandle);
			SetSavePath();

			GetVolumes();

			Thread pollingThread = new Thread(delegate()
			{
				StartPollingOBS();
			});
			pollingThread.IsBackground = true;
			pollingThread.Start();
		}

		private void StartOBS()
		{
			RegistryKey gmKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
			string gmDir = (string)gmKey.GetValue("InstallDir");
			Process proc = new Process();
			if (Oem.Instance.OEM == "gamemanager" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "bluestacks")
				proc.StartInfo.FileName = Path.Combine(gmDir, @"OBS\HD-OBS.exe");
			else
				proc.StartInfo.FileName = Path.Combine(BlueStacksTV.sServerRootDir, @"OBS\HD-OBS.exe");
			proc.StartInfo.Arguments = "";
			proc.Start();
			proc.WaitForInputIdle();
			Thread.Sleep(1000);
		}

		private void SetHwnd(string handle)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("hwnd", handle);
			SendObsRequest("sethwnd", data, null, null, 0);
		}

		private void SetSavePath()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			string savePath = Path.Combine(BlueStacksTV.sServerRootDir, "stream.flv");
			data.Add("savepath", savePath);
			SendObsRequest("setsavepath", data, null, null, 0);
		}

		public void SetFrontendPosition(string width, string height, string x, string y)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width);
			data.Add("height", height);
			data.Add("y", y);
			data.Add("x", x);
			SendObsRequest("setfrontendposition", data, null, null, 0);
		}

		public void DisableCLRBrowser()
		{
			SendObsRequest("disableclrbrowser", null, null, null, 0);
		}

        public void SetStreamDimension(out int startX, out int startY, out int width, out int height)
        {
            try
            {
                string url = String.Format("http://127.0.0.1:{0}/{1}", BlueStacksTV.sApplicationServerPort, "streamdimension");
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

		public void SetCameraPosition(string width, string height, string x, string y)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width.ToString());
			data.Add("height", height.ToString());
			data.Add("x", x.ToString());
			data.Add("y", y.ToString());
			SendObsRequest("setcameraposition", data, null, null, 0);
		}

		public void ObsErrorStatus(string erroReason)
		{
			mIsStreaming = false;
			mFailureReason = "Error starting stream : " + erroReason;
			SendStreamStatus(false, false);
		}

		public void ReportObsError()
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", BlueStacksTV.sApplicationServerPort, "reportobserror");
				string res = Common.HTTP.Client.Post(url, null, null, false);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to report obs error.. Err : " + ex.ToString());
			}
		}

		private void GetVolumes()
		{
			SendObsRequest("getmicvolume", null, "SetMicVolumeLocal", null, 0);
			SendObsRequest("getsystemvolume", null, "SetSystemVolumeLocal", null, 0);
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
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width);
			data.Add("height", height);
			data.Add("position", position);
			SendObsRequest("enablewebcam", data, "WebcamConfigured", null, 0);
		}

		public void DisableWebcam()
		{
			SendObsRequest("disablewebcam", null, "WebcamConfigured", null, 0);
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
					RegistryKey key = BlueStacksTV.sConfigKey;
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

        public void SetConfig(int startX, int startY, int width, int height)
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
			SendObsRequest("setconfig", data, null, null, 0);
		}

        public void StartStream(string playPath, string url)
        {
            SetStreamSettings(playPath, url);
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
				string url = String.Format("http://127.0.0.1:{0}/{1}", BlueStacksTV.sApplicationServerPort, path);
				string res = Common.HTTP.Client.Post(url, data, null, false);
				Logger.Info("Successfully sent status for {0}", path);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to send post request for {0}... Err : {1}", path, ex.ToString());
			}
		}

		public void StartRecord(bool reInitAfterStart)
		{
			mIsRecording = true;

			SendObsRequest("startrecord", null, "RecordStarted", null, 0);
		}

		public void StopRecord()
		{
			StopRecord(false);
		}

		public void StopRecord(bool immediate)
		{
			mIsRecording = false;
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("immediate", immediate ? "1" : "0");
			SendObsRequest("stoprecord", data, "RecordStopped", null, 0);
		}

		private void StreamStarted(string response)
		{
			SendStatus("streamstarted", null);
			mIsStreaming = true;
		}

		private void StreamStopped(string response)
		{
			SendStatus("streamstopped", null);
            mIsStreaming = false;
		}

		private void RecordStarted(string response)
		{
			SendStatus("recordstarted", null);
			mIsRecording = true;
		}

		private void RecordStopped(string response)
		{
			SendStatus("recordstopped", null);
			mIsRecording = false;
		}

		public void StartReplayBuffer()
		{
			SendObsRequest("startreplaybuffer", null, null, null, 0);
		}

		public void StopReplayBuffer()
		{
			SendObsRequest("stopreplaybuffer", null, null, null, 2000);
		}

		private void SetStreamSettings(string playPath, string url)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
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

					if (streaming != mIsStreaming || reconnecting != mIsReconnecting)
					{
						SendStreamStatus(streaming, reconnecting);
					}

					mIsStreaming = streaming;
					mIsReconnecting = reconnecting;
					Dictionary<string, string> data = new Dictionary<string, string>();
					data.Add("isstreaming", Convert.ToString(mIsStreaming));
					data.Add("isrecording", Convert.ToString(mIsRecording));
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
						UpdateFailureReason();
						SendStreamStatus(false, false);
						InitObs();
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
					logFilesDir = Path.Combine(BlueStacksTV.sServerRootDir, @"OBS\Logs\");
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

                string url = String.Format("http://127.0.0.1:{0}/{1}", BlueStacksTV.sApplicationServerPort, "streamstatuscallback");

                Common.HTTP.Client.Post(url, data, null, false);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to send stream status... Err : " + ex.ToString());
            }

        }

		public void ResizeStream(string width, string height)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("width", width);
			data.Add("height", height);
			SendObsRequest("windowresized", data, null, null, 0);
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
			mIsObsRunning = false;
			SendObsRequest("close", null, null, "CloseFailed", 0);
		}

		public void CloseFailed()
		{
			Utils.KillProcessByName("HD-OBS");
            StreamManager.Instance = null;
		}

		public void SaveReplayBuffer()
		{
			SendObsRequest("savereplaybuffer", null, null, null, 0);
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
			Logger.Info("got obs request: " + request);
			if (mObsCommandQueue == null)
			{
				mObsCommandQueue = new Queue<ObsCommand>();
			}

			Thread commandQueueThread = new Thread(delegate()
			{
				ObsCommand command = new ObsCommand(request, data,
					responseCallback, failureCallback, pauseTime);

				lock (mObsCommandQueueObject)
				{
					mObsCommandQueue.Enqueue(command);
					sObsCommandEventHandle.Set();
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
				sObsCommandEventHandle.WaitOne();
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

		public void Init(string appHandle, string pid)
		{
            Logger.Info("App Handle : {0} and Process Id : {1}", appHandle, pid);
			if (mAppHandle == "" && mAppPid == "")
			{
				mAppHandle = appHandle;
				mAppPid = pid;
				InitObs();
			}
            else if (appHandle == mAppHandle && pid == mAppPid)
            {
                InitObs();
            }
            else
            {
                ReportObsError();
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
