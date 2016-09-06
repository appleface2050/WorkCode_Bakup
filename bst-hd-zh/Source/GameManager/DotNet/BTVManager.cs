using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

using BlueStacks.hyperDroid.Common;
using System.Windows;
using CodeTitans.JSon;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
    class BTVManager
    {
        public static bool sStreaming = false;
        public static bool sRecording = false;
        public static bool sFullScreenClicked = false;
        public static bool sWasRecording = false;
        public static bool sStopPingBTVThread = false;
        public static object sPingBTVLock = new object();

        public static bool sWritingToFile
        {
            set
            {
                Common.HTTP.Server.s_FileWriteComplete = !value;
            }
        }

        public static void StartBlueStacksTV()
        {
            Process proc = new Process();
            RegistryKey gmKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
            string gmDir = (String)gmKey.GetValue("InstallDir");
            proc.StartInfo.FileName = Path.Combine(gmDir, "BlueStacksTV.exe");
            proc.StartInfo.Arguments = "-u";
            proc.Start();
            Thread.Sleep(1000);

            Thread thread = new Thread(StartPingBTVThread);
            thread.IsBackground = true;
            thread.Start();
        }

        public static void StartPingBTVThread()
        {
            lock (sPingBTVLock)
            {
                Logger.Info("Starting btv ping thread");
                while (true)
                {
                    PingBTV();

                    if (sStopPingBTVThread)
                    {
                        GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                        {
                            ResizeManager.EnableResizing();
                            GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = true;
                            GameManagerWindow.Instance.mToolBar.HandleGoLiveButton("");
                        }));
                        break;
                    }
                    Thread.Sleep(5000);
                }
            }
        }

        public static void ShowStreamWindow()
        {
			if (!Utils.FindProcessByName("BlueStacksTV"))
            {
                StartBlueStacksTV();
            }
            else
            {
                SendBTVRequest("showstreamwindow", null);
            }
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                ToolBar.Instance.HandleGoLiveButton("on");
            }));
        }

        public static void HideStreamWindow()
        {
            if (Utils.FindProcessByName("BlueStacksTV"))
            {
                SendBTVRequest("hidestreamwindow", null);
            }
        }

        public static void GetStreamDimensionInfo(out int startX, out int startY,
                out int width, out int height)
        {
            Point p = new Point();
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                p = ContentControl.Instance.TranslatePoint(new System.Windows.Point(0, 0), GameManagerWindow.Instance);
            }));
            startX = ((Convert.ToInt32(p.X)) * GameManagerUtilities.sDpi) / Utils.DEFAULT_DPI;
            startY = (Convert.ToInt32(p.Y) * GameManagerUtilities.sDpi) / Utils.DEFAULT_DPI;
            width = ((int)(ContentControl.Instance.ActualWidth) * GameManagerUtilities.sDpi) / Utils.DEFAULT_DPI;
            height = ((int)(ContentControl.Instance.ActualHeight) * GameManagerUtilities.sDpi) / Utils.DEFAULT_DPI;
        }

        public static void PingBTV()
        {
            bool recording = false;
            bool streaming = false;
            try
            {
                string res = SendBTVRequest("ping", null);
                JSonReader reader = new JSonReader();
                IJSonObject obj = reader.ReadAsJSonObject(res);

                if (obj[0]["success"].BooleanValue)
                {
                    recording = obj[0]["recording"].BooleanValue;
                    streaming = obj[0]["streaming"].BooleanValue;
                }
                Logger.Info("Ping BTV response recording: {0}, streaming: {1}", recording, streaming);
                sStopPingBTVThread = false;
            }
            catch (Exception ex)
            {
                sStopPingBTVThread = true;
                Logger.Error("PingBTV : {0}", ex.Message);
            }

            sRecording = recording;
            sStreaming = streaming;
            if (!recording)
            {
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    sFullScreenClicked = false;
                    ResizeManager.EnableResizing();
                    GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = true;
                }));
            }
        }

        public static void SetFrontendPosition(int width, int height, bool isPortrait)
        {
            if (Utils.FindProcessByName("BlueStacksTV"))
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("width", width.ToString());
                data.Add("height", height.ToString());
                data.Add("isPortrait", isPortrait.ToString());
                SendBTVRequest("setfrontendposition", data);
            }
        }

        public static void StreamStarted()
        {
            sWritingToFile = true;
            sRecording = true;
            sStreaming = true;
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = false;
                ResizeManager.DisableResizng();
                GameManagerWindow.Instance.mToolBar.HandleGoLiveButton("live");
            }));
        }

        public static void StreamStopped()
        {
            sWritingToFile = false;
            sStreaming = false;
            sRecording = false;
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                ResizeManager.EnableResizing();
                GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = true;
                GameManagerWindow.Instance.mToolBar.HandleGoLiveButton("on");
            }));
        }

        public static void RecordStarted()
        {
            sWritingToFile = true;
            sRecording = true;
            sWasRecording = true;

            if (sFullScreenClicked)
            {
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    sFullScreenClicked = false;
                    ResizeManager.EnableResizing();
                    GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = true;
                }));
            }
        }

        public static void SetConfig()
        {
            int startX, startY, width, height;
            GetStreamDimensionInfo(out startX, out startY, out width, out height);

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("startX", startX.ToString());
            data.Add("startY", startY.ToString());
            data.Add("width", width.ToString());
            data.Add("height", height.ToString());

            SendBTVRequest("setconfig", data);
        }

        public static void RecordStopped()
        {
            sWritingToFile = false;
            sRecording = false;
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                if (!BTVManager.sFullScreenClicked)
                    GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = true;
            }));
        }

        public static void SendTabChangeData(string[] tabChangedData)
        {
            Thread d = new Thread(delegate ()
                {
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("type", tabChangedData[0]);
                    data.Add("name", tabChangedData[1]);
                    data.Add("data", tabChangedData[2]);
                    SendBTVRequest("tabchangeddata", data);
                }
              );
            d.IsBackground = true;
            d.Start();
        }

        public static void ReplayBufferSaved()
        {
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

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
            }));
        }

        public static void Stop()
        {
            if (sStreaming || sRecording)
            {
                SendBTVRequest("sessionswitch", null);
                sWasRecording = false;
            }
        }

        public static void CloseBTV()
        {
            if (sStopPingBTVThread)
            {
                //since if btv is already close we get some timeout exception
                //which blocks the ui thread
                Thread thread = new Thread(delegate() {
                    SendBTVRequest("closebtv", null);
                });
                thread.IsBackground = true;
                thread.Start();
            }
            else
                SendBTVRequest("closebtv", null);
            sWasRecording = false;
        }

        public static void CheckNewFiltersAvailable()
        {
            SendBTVRequest("checknewfilters", null);
        }

        public static string SendBTVRequest(string request, Dictionary<string, string> data)
        {
            try
            {
                Logger.Info("Sending btv request for : " + request);
                RegistryKey btvKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
                int btvPort = Utils.GetBTVServerPort();
                string url = String.Format("http://127.0.0.1:{0}/{1}", btvPort.ToString(), request);
                string res = Common.HTTP.Client.Post(url, data, null, false);
                return res;
            }
            catch (Exception ex)
            {
                Logger.Error("An unexpected error occured in request {0}... Err : {1}", request, ex.ToString());
                return null;
            }
        }
    }
}
