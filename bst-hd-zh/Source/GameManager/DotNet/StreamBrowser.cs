using System;
using System.Windows.Forms;
using System.Threading;

using Microsoft.Win32;

using Gecko;
using Gecko.Events;

using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.GameManager
{
	public class StreamBrowser : Browser
	{
		private StreamManager mStreamManager;

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
			if (mStreamManager == null)
			{
				mStreamManager = new StreamManager(this);
			}
			return mStreamManager;
		}

		public void StartObs(string callbackFunction)
		{
			if (GameManager.sGameManager.mFilterDownloader != null)
			{
				if (GameManager.sGameManager.mStreamWindow.mIsOBSReParented)
					GameManager.sGameManager.mFilterDownloader.mReParentOBS = true;
				GameManager.sGameManager.mStreamWindow.UnSetOBSParentWindow();
				GameManager.sGameManager.mFilterDownloader.LaunchUI(callbackFunction);
			}
			else
			{
				EvaluateJS(callbackFunction + "('true');");
			}
		}

		public void StartStream(string key, string location,
				string callbackStreamStatus, string callbackTabChanged)
		{
			GameManager.sGameManager.mStreamManager = InitStreamManager();

			if (mStreamManager.mReplayBufferEnabled)
				mStreamManager.StartReplayBuffer();


			Logger.Info("Got StartStream");
			if (GameManager.sGameManager.mFilterWindow == null)
				GameManager.sGameManager.mStreamWindow.UnSetOBSParentWindow();
			mStreamManager.StartStream(key, location, callbackStreamStatus, callbackTabChanged);
		}

		public void StopStream()
		{
			Logger.Info("Got StopStream");
			mStreamManager.StopStream();
		}

		public void StartRecord()
		{
			GameManager.sGameManager.mStreamManager = InitStreamManager();

			if (mStreamManager.mReplayBufferEnabled)
				mStreamManager.StartReplayBuffer();

			Logger.Info("Got StartRecord");
			mStreamManager.StartRecord();
			GameManager.sGameManager.mStreamWindow.ReParentOBSWindow();
		}

		public void StopRecord()
		{
			if (mStreamManager != null)
			{
				Logger.Info("Got StopRecord");
				if (GameManager.sGameManager.mFilterWindow == null)
					GameManager.sGameManager.mStreamWindow.UnSetOBSParentWindow();
				mStreamManager.StopRecord(true);
			}
		}

		public void SetSystemVolume(string level)
		{
			mStreamManager.SetSystemVolume(level);
		}

		public void SetMicVolume(string level)
		{
			mStreamManager.SetMicVolume(level);
		}

		public void EnableWebcam(string width, string height, string position)
		{
			Logger.Info("Got EnableWebcam");
			mStreamManager.EnableWebcam(width, height, position);
		}

		public void DisableWebcam()
		{
			Logger.Info("Got DisableWebcam");
			mStreamManager.DisableWebcam();
		}

		public void MoveWebcam(string horizontal, string vertical)
		{
			mStreamManager.MoveWebcam(horizontal, vertical);
		}

		public string GetStreamConfig()
		{
			GameManager.sGameManager.mStreamManager = InitStreamManager();

			Logger.Info("In GetStreamConfig");
			return mStreamManager.GetStreamConfig();
		}

		public void SetStreamName(string name)
		{
			Logger.Info("Got SetStreamName: " + name);
			mStreamManager.SetStreamName(name);
		}

		public void SetServerLocation(string location)
		{
			Logger.Info("Got SetServerLocation: " + location);
			mStreamManager.SetServerLocation(location);
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
