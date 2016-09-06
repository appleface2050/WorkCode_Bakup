using System;
using System.Windows.Forms;
using System.Diagnostics;
using System.Threading;
using Microsoft.Win32;

using Gecko;
using Gecko.Events;

using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using System.Windows.Interop;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class StreamBrowser : Browser
	{

		public StreamBrowser(String url) : base(url)
		{
			this.Navigating += this.OnWebBrowserNavigating;
			this.NoDefaultContextMenu = true;
		}

		private void OnWebBrowserNavigating(object sender, GeckoNavigatingEventArgs e)
		{
			if (String.Compare(e.Uri.Host, "www.twitch.tv") == 0)
				e.Cancel = true;
		}

		public StreamManager InitStreamManager()
		{
			if (StreamManager.Instance == null)
			{
				StreamManager.Instance = new StreamManager(this);

				string handle, pid;
				GetStreamConfig(out handle, out pid);
				StreamManager.Instance.Init(handle, pid);
			}
			return StreamManager.Instance;
		}

		public void HidePreview()
		{
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				try
				{
					StreamWindow.Instance.HideGrid();
				}
				catch(Exception ex)
				{
					Logger.Error("HidePreview Error: {0}", ex);
				}
			}));
		}

		public void ShowPreview()
		{
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				try
				{
					StreamWindow.Instance.ShowGrid();
				}
				catch(Exception ex)
				{
					Logger.Error("ShowPreview Error: {0}", ex);
				}
			}));
		}

		public void StartObs(string callbackFunction)
		{
			if (FilterDownloader.Instance != null)
			{
				if (StreamWindowUtility.sIsOBSReParented)
					FilterDownloader.Instance.mReParentOBS = true;
				StreamWindowUtility.UnSetOBSParentWindow();
				FilterDownloader.Instance.LaunchUI(callbackFunction);
			}
			else
			{
				EvaluateJS(callbackFunction + "('true');");
			}
		}

		private void GetStreamConfig(out string handle, out string pid)
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "initstream");
				string resp = Common.HTTP.Client.Get(url, null, false);
				Logger.Info("response ----> " + resp);

				JSonReader reader = new JSonReader();
				IJSonObject config = reader.ReadAsJSonObject(resp);
				handle = config["handle"].ToString();
				pid = config["pid"].ToString();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to get window handle and process id... Err : " + ex.ToString());
				handle = pid = null;
				StreamWindow.Instance.Close();
			}
		}

		public void LaunchDialog(string jsonString)
		{
			JSonReader reader = new JSonReader();
			IJSonObject obj = reader.ReadAsJSonObject(jsonString);

			string paramsStr = "";
			if (obj.Contains("parameter"))
				paramsStr = obj["parameter"].StringValue;

			LayoutWindow.LaunchWindow(paramsStr);
		}

		public void StartStreamV2(string jsonString,
				string callbackStreamStatus, string callbackTabChanged)
		{
			InitStreamManager();

			if (StreamManager.Instance.mReplayBufferEnabled)
				StreamManager.Instance.StartReplayBuffer();


			Logger.Info("Got StartStream");
			if (FilterWindow.Instance == null)
				StreamWindowUtility.UnSetOBSParentWindow();

			StreamManager.Instance.StartStream(jsonString, callbackStreamStatus, callbackTabChanged);
		}

		public void StopStream()
		{
			Logger.Info("Got StopStream");
			StreamManager.Instance.StopStream();
		}

		public void StartRecord()
		{
			StartRecordV2("{}");
		}

		public void StartRecordV2(string jsonString)
		{
			JSonReader jsonReader = new JSonReader();
			IJSonObject obj = jsonReader.ReadAsJSonObject(jsonString);

			//default values
			string network = StreamManager.DEFAULT_NETWORK;
			bool enableFilter = StreamManager.DEFAULT_ENABLE_FILTER;
			string layoutTheme = StreamManager.DEFAULT_LAYOUT_THEME;
			bool squareTheme = StreamManager.DEFAULT_SQUARE_THEME;

			if (obj.Contains("network"))
				network = obj["network"].StringValue;

			OBSRenderFrameSpecs obsRenderFrameSpecs = new OBSRenderFrameSpecs(jsonString);

			if (obj.Contains("enableFilter"))
				enableFilter = Convert.ToBoolean(obj["enableFilter"].StringValue);
			if (obj.Contains("layoutTheme") && !obj["layoutTheme"].IsNull)
				layoutTheme = obj["layoutTheme"].ToString();
			if (obj.Contains("squareTheme"))
				squareTheme = Convert.ToBoolean(obj["squareTheme"].StringValue);

			Logger.Info("network = {0}, position = {1}x{2}, size = {3}x{4}", network,
					obsRenderFrameSpecs.xPosition, obsRenderFrameSpecs.yPosition,
					obsRenderFrameSpecs.width, obsRenderFrameSpecs.height);
			Logger.Info("enableFilter: {0}, layoutTheme: {1}", enableFilter, layoutTheme);

			InitStreamManager();

			if (StreamManager.Instance.mReplayBufferEnabled)
				StreamManager.Instance.StartReplayBuffer();

			Logger.Info("Got StartRecord");
			StreamManager.Instance.StartRecord(network, enableFilter, squareTheme, layoutTheme);
			StreamWindowUtility.ReParentOBSWindow(obsRenderFrameSpecs);
		}

		public void StopRecord()
		{
			if (StreamManager.Instance != null)
			{
				Logger.Info("Got StopRecord");
				if (FilterWindow.Instance == null)
					StreamWindowUtility.UnSetOBSParentWindow();
				StreamManager.Instance.StopRecord(true);
			}
		}

		public void SetSystemVolume(string level)
		{
			StreamManager.Instance.SetSystemVolume(level);
		}

		public void SetMicVolume(string level)
		{
			StreamManager.Instance.SetMicVolume(level);
		}

		public void EnableWebcam(string width, string height, string position)
		{
			Logger.Info("Got EnableWebcam");
			StreamManager.Instance.EnableWebcam(width, height, position);
		}

		public void DisableWebcamV2(string jsonString)
		{
			Logger.Info("Got DisableWebcamV2");
			StreamManager.Instance.DisableWebcamV2(jsonString);
		}

		/*public void DisableWebcam()
		{
			Logger.Info("Got DisableWebcam");
			StreamManager.Instance.DisableWebcam();
		}*/

		public void MoveWebcam(string horizontal, string vertical)
		{
			StreamManager.Instance.MoveWebcam(horizontal, vertical);
		}

		public string GetStreamConfig()
		{
			//InitStreamManager();

			Logger.Info("In GetStreamConfig");
			return StreamManager.GetStreamConfig();
		}

		public void SetStreamName(string name)
		{
			Logger.Info("Got SetStreamName: " + name);
			StreamManager.Instance.SetStreamName(name);
		}

		public void SetServerLocation(string location)
		{
			Logger.Info("Got SetServerLocation: " + location);
			StreamManager.Instance.SetServerLocation(location);
		}

		public string GetCurrentSessionId()
		{
			Logger.Info("In GetCurrentSessionId");
			return Stats.GetSessionId();
		}

		public string ResetSessionId()
		{
			Logger.Info("In ResetSessionId");
			return Stats.ResetSessionId();
		}

		public void SetChannelName(string channelName)
		{
			FilterUtility.SetChannelName(channelName);
		}

		public void SetClientId(string clientId)
		{
			FilterUtility.ClientId = clientId;
		}
	}
}
