using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Windows.Forms;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class HTTPHandler
	{
		private static void WriteSuccessJson(HttpListenerResponse res)
		{
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}

		private static void WriteErrorJson(String reason, HttpListenerResponse res)
		{
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", false);
			json.WriteMember("reason", reason);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}

		public static void PingHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got Ping {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				JSonWriter json = new JSonWriter();
				json.WriteArrayBegin();
				json.WriteObjectBegin();
				json.WriteMember("success", true);
				if (StreamManager.Instance != null)
				{
					json.WriteMember("recording", StreamManager.Instance.mIsObsRunning);
					json.WriteMember("streaming", StreamManager.Instance.mIsStreaming);
				}
				else
				{
					json.WriteMember("recording", false);
					json.WriteMember("streaming", false);
				}
				json.WriteObjectEnd();
				json.WriteArrayEnd();
				Common.HTTP.Utils.Write(json.ToString(), res);

			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server Ping");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void ShowStreamWindowHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got ShowStreamWindowHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				StreamWindowUtility.ShowStreamWindow();
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server ShowStreamWindowHandler");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void HideStreamWindowHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got HideStreamWindowHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				if (StreamWindow.Instance != null)
				{
					StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
					{
						StreamWindow.Instance.Visibility = System.Windows.Visibility.Hidden;
					}));
					WriteSuccessJson(res);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in Server HideStreamWindowHandler");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.Message, res);
			}
		}

		public static void SessionSwitchHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got SessionSwitchHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				if (StreamManager.Instance != null)
				{
					StreamManager.Instance.StopStream();
					StreamManager.Instance.StopRecord(true);
					StreamWindow.Instance.mBrowser.LoadUrl(StreamWindowUtility.GetStreamWindowUrl());
				}
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in Server SessionSwitchHandler");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.Message, res);
			}
		}

		public static void CloseBTVHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got CloseBTVHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				WriteSuccessJson(res);
				StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
				{
					if (StreamWindow.Instance != null)
					{
						StreamWindow.Instance.Close();
					}
					else
					{
						StreamWindowUtility.CloseBTV();
					}
				}));
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in Server CloseBTVHandler");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.Message, res);
			}
		}

		public static void TabChangedDataHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got TabChangedDataHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string type = requestData.data["type"];
				string name = requestData.data["name"];
				string data = requestData.data["data"];

				//Handling Tab Change functionality for CLR Browser
				if (StreamManager.Instance != null
						&& StreamManager.Instance.mIsObsRunning)
				{
					StreamManager.Instance.SendAppInfo(type, name, data);

					string appPkg = data;

					if (StreamManager.Instance.mLayoutTheme == null)
					{
						if (appPkg != null && FilterUtility.IsFilterApplicableApp(appPkg))
						{
							if (!StreamManager.Instance.mCLRBrowserRunning)
								StreamManager.Instance.InitCLRBrowser(appPkg,
									FilterUtility.GetCurrentTheme(appPkg));
							else if (StreamManager.Instance.mCurrentFilterAppPkg
										!= appPkg)
							{
								string theme = FilterUtility.GetCurrentTheme(appPkg);
								if (theme != null)
									StreamManager.Instance.ChangeTheme(appPkg, theme);
								else
									StreamManager.Instance.ResetCLRBrowser();
							}
						}
						else if (StreamManager.Instance.mEnableFilter && StreamManager.Instance.mCLRBrowserRunning)
							StreamManager.Instance.ResetCLRBrowser();
					}
				}
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in Server TabChangedDataHandler");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.Message, res);
			}
		}

		public static void ShowObsHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got ShowObsHandler");

			try
			{
				if (StreamManager.Instance != null)
				{
					StreamManager.Instance.ShowObs();
					WriteSuccessJson(res);
				}
			}
			catch (Exception ex)
			{
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void HideObsHandlerHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got HideObsHandler");

			try
			{
				if (StreamManager.Instance != null)
				{
					StreamManager.Instance.HideObs();
					WriteSuccessJson(res);
				}
			}
			catch (Exception ex)
			{
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void ReceiveAppInstallStatusHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got ReceiveAppInstallStatusHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				if (!Oem.Instance.IsBTVBuild)
				{
					WriteSuccessJson(res);
					return;
				}

				RequestData requestData = HTTPUtils.ParseRequest(req);
				foreach (string key in requestData.data.AllKeys)
				{
					Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
				}

				string package = requestData.data["package"];
				Logger.Info("package: " + package);
				string isInstall = requestData.data["isInstall"];
				Logger.Info("isInstall: " + isInstall);

				if (isInstall.Equals("true"))
					FilterUtility.CheckNewFiltersAvailable();
				else
					FilterDownloader.RemoveFilterAvailableForApp(package);
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server ReceiveAppInstallStatusHandler: " + exc.ToString());
				WriteErrorJson(exc.ToString(), res);
			}
		}

		public static void SetFrontendPositionHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			if (StreamManager.Instance != null)
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string width = requestData.data["width"];
				string height = requestData.data["height"];
				string isPortrait = requestData.data["isPortrait"].ToLower();

				Logger.Info("SetFrontendPositionHandler: width x height, portrait {0} x {1}, {2}",
						width, height, isPortrait);

				if (StreamManager.Instance.mLayoutTheme != null)
					StreamManager.Instance.SetSceneConfiguration(StreamManager.Instance.mLayoutTheme);
				else
					StreamManager.Instance.SetFrontendPosition(Int32.Parse(width), Int32.Parse(height));
			}
		}
		public static void MoveWebCamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got MoveWebCam");

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string horizontal = requestData.data["horizontal"];
				string vertical = requestData.data["vertical"];
				StreamManager.Instance.MoveWebcam(horizontal, vertical);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got exception while moving web cam... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void SetConfigHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got SetConfig");
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				int startX = Convert.ToInt32(requestData.data["startX"]);
				int startY = Convert.ToInt32(requestData.data["startY"]);
				int width = Convert.ToInt32(requestData.data["width"]);
				int height = Convert.ToInt32(requestData.data["height"]);
				StreamManager.Instance.SetConfig(startX, startY, width, height);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in SetConfig");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void StartReplayBufferHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got Start Replay Buffer");
			try
			{
				StreamManager.Instance.StartReplayBuffer();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in StartReplayBuffer");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void StopReplayBufferHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got StopReplayBuffer");
			try
			{
				StreamManager.Instance.StopReplayBuffer();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in StopReplayBuffer");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void SaveReplayBufferHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got StopReplayBuffer");
			try
			{
				StreamManager.Instance.SaveReplayBuffer();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in StopReplayBuffer");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void SetSystemVolumeHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got SetSystemVolume");
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string volume = requestData.data["volume"];
				StreamManager.Instance.SetSystemVolume(volume);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in SetSystemVolume");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void SetMicVolumeHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got SetMicVolume");
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string volume = requestData.data["volume"];
				StreamManager.Instance.SetMicVolume(volume);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in SetMicVolume");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		internal static void ObsStatusHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got ObsStatus {0} request from {1}",
					   req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				if (requestData.data.Count > 0 && requestData.data.AllKeys[0] == "Error")
				{
					if (StreamManager.sStopInitOBSQueue)
						return;

					if (requestData.data[0].Equals("OBSAlreadyRunning"))
						StreamManager.sStopInitOBSQueue = true;

					Thread thread = new Thread(delegate ()
					{
						StreamManager.Instance.ReportObsError(requestData.data[0]);
					});
					thread.IsBackground = true;
					thread.Start();
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in ObsStatus");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void ReportObsErrorHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got ReportObsErrorHandler");
			try
			{
				StreamManager.Instance.ReportObsError("obs_error");
				StreamManager.Instance = null;
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in ReportObsHandler");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void ReplayBufferSavedHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Replay buffer saved Handler");
			if (StreamManager.Instance != null)
				StreamManager.Instance.ReplayBufferSaved();
		}

		public static void ResetFlvStreamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("In reset flv stream");

			try
			{
				StreamManager.Instance.ResetFlvStream();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in reseting flv stream... Err : " + ex.ToString());
			}
		}

		public static void ShutDownObsHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got ShutDownObs");
			try
			{
				StreamManager.Instance.Shutdown();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in Obs Shut Down");
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void InitStreamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Logger.Info("Got request for initializing stream");

			string pid = requestData.data["pid"];
			string handle = requestData.data["handle"];

			try
			{
				StreamManager.Instance = new StreamManager(null);
				StreamManager.Instance.Init(handle, pid);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void StartStreamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Logger.Info("Got request for start stream");

			try
			{
				string playPath = requestData.data["playPath"];
				string url = requestData.data["url"];
				StreamManager.Instance.StartStream(playPath, url, null, null);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while starting stream... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void StopStreamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for stop stream");

			try
			{
				StreamManager.Instance.StopStream();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while stopping stream... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void StartRecordHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for start record");

			try
			{
				StreamManager.Instance.StartRecord();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while starting recording... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void StopRecordHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for stop record");

			try
			{
				StreamManager.Instance.StopRecord();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while stopping recording... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void ResizeStreamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for resize stream");

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string width = requestData.data["width"];
				string height = requestData.data["height"];
				StreamManager.Instance.ResizeStream(width, height);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while resizing stream... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void EnableWebCamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Logger.Info("Got request for enable web cam");

			try
			{
				string width = requestData.data["width"];
				string height = requestData.data["height"];
				string position = requestData.data["position"];

				StreamManager.Instance.EnableWebcam(width, height, position);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while enabling webcam... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void DisableWebCamHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for disable web cam");

			try
			{
				StreamManager.Instance.DisableWebcamV2("{}");
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while disabling webcam... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void SetClrBrowserConfigHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Logger.Info("Got request for set clr browser config");

			try
			{
				string width = requestData.data["width"];
				string height = requestData.data["height"];
				string url = requestData.data["url"];
				StreamManager.Instance.SetClrBrowserConfig(width, height, url);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while setting clr browser config... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void EnableClrBrowserHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for enable clr browser");

			try
			{
				StreamManager.Instance.EnableCLRBrowser();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while enabling clr browser... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void DisableClrBrowserHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for disable clr browser");

			try
			{
				StreamManager.Instance.DisableCLRBrowser();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while disabling clr browser... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}

		public static void CheckNewFiltersHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request for check new filters");

			try
			{
				FilterUtility.CheckNewFiltersAvailable();
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Got error while check new filters... Err : " + ex.ToString());
				WriteErrorJson(ex.ToString(), res);
			}
		}
	}
}
