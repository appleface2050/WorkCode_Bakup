/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * This file Implements http call handler for frontend
 */

using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Runtime.InteropServices;
using System.Threading;
using BlueStacks.hyperDroid.Frontend;
using System.Web;
using System.Windows.Forms;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using System.Windows.Interop;

namespace BlueStacks.hyperDroid.GameManager
{
    public class GMHTTPHandler
    {
        private static Object sLockObject = new Object();
        private static Object sFrontendPositionRequestLock = new Object();

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

        public static void GMNotificationHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got GMNotificationHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }

                string type = requestData.data["type"];
                string package = requestData.data["package"];
                string content = requestData.data["content"];
                string name, img, activity, version;

                GMAppsManager installedAppsList = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
                if (installedAppsList.GetAppInfoFromPackageName(package, out name, out img, out activity, out version))
                {
                    Logger.Info("type: " + type);

                    Logger.Info("name: " + name);
                    //Logger.Info("img: " + img);
                    //Logger.Info("content: " + content);

                    Logger.Info("package: " + package);
                    Logger.Info("activity: " + activity);

                    Logger.Info("version: " + version);

                    /*
                     * We now have all the information we need to add notification:
                     * content, img, app name to show notification
                     * package, activty to launch app on notification click
                     */
                }

                WriteSuccessJson(res);

            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server GMNotificationHandler");
                Logger.Error(exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void PingHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            WriteSuccessJson(res);
        }

        public static void StreamStartedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got StreamStartedHandler {0} request from {1}",
                req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                BTVManager.StreamStarted();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in server StreamStartedHandler... Err : " + ex.ToString());
            }
        }

        public static void StreamStoppedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got StreamStoppedHandler {0} request from {1}",
                req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                BTVManager.StreamStopped();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in server StreamStoppedHandler... Err : " + ex.ToString());
            }
        }

        public static void RecordStartedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got RecordStartedHandler {0} request from {1}",
                req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                BTVManager.RecordStarted();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in server RecordStartedHandler... Err : " + ex.ToString());
            }
        }

	public static void RelaunchStreamWindowHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got RelaunchStreamWindowHandler {0} request from {1}",
                req.HttpMethod, req.RemoteEndPoint.ToString());

	    Thread thread = new Thread(delegate() {
		BTVManager.CloseBTV();
		
		Thread.Sleep(1000);
		if (Process.GetProcessesByName("BlueStacksTV").Length != 0)
			Utils.KillProcessByName("BlueStacksTV");

		BTVManager.ShowStreamWindow();
	    });
	    thread.IsBackground = true;
	    thread.Start();
	}

        public static void RecordStoppedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got RecordStoppedHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                BTVManager.RecordStopped();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in server RecordStoppedHandler... Err : " + ex.ToString());
            }
        }

        public static void GetCurrentAppPackageHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got GetCurrentAppPackageHandler {0} request from {1}",
                req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                JSonWriter json = new JSonWriter();
                json.WriteObjectBegin();

                string pkg = TabButtons.Instance.SelectedTab.mPackageName;
                if (TabButtons.Instance.SelectedTab.TabType == EnumTabType.web || pkg == null)
                {
                    json.WriteMember("package", "");
                    Logger.Info("Current Package is null");
                }
                else
                {
                    json.WriteMember("package", pkg);
                    Logger.Info("Current app package is {0}", pkg);
                }
                json.WriteObjectEnd();
                Common.HTTP.Utils.Write(json.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server GetCurrentAppPackageHandler: " + ex.ToString());
            }
        }

	public static void IsPortraitModeHandler(HttpListenerRequest req, HttpListenerResponse res)
	{
		try
		{
			Logger.Info("Got IsPortraitModeHandler {0} request from {1}",
			    req.HttpMethod, req.RemoteEndPoint.ToString());

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("isPortrait", FrontendHandler.frontend.mEmulatedPortraitMode.ToString().ToLower());
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception ex)
		{
			Logger.Error("Exception in Server IsPortraitModeHandler... Err : " + ex.ToString());
			WriteErrorJson(ex.ToString(), res);
		}
	}

        public static void SetFrontendPositionHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            RequestData requestData = HTTPUtils.ParseRequest(req);
            string width = requestData.data["width"];
            string height = requestData.data["height"];
            bool isPortrait = FrontendHandler.frontend.mEmulatedPortraitMode;

            Logger.Info("SetFrontendPositionHandler: width x height {0} x {1}", width, height);
            Thread thread = new Thread(delegate()
            {
                lock (sFrontendPositionRequestLock)
                {
                    if (BTVManager.sRecording)
                        BTVManager.SetFrontendPosition(Int32.Parse(width), Int32.Parse(height), isPortrait);
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void SetStreamDimensionHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got SetStreamDimensionHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                int startX, startY, width, height;
                BTVManager.GetStreamDimensionInfo(out startX, out startY, out width, out height);

                JSonWriter json = new JSonWriter();
                json.WriteObjectBegin();
                json.WriteMember("startX", startX);
                json.WriteMember("startY", startY);
                json.WriteMember("width", width);
                json.WriteMember("height", height);
                json.WriteObjectEnd();
                Common.HTTP.Utils.Write(json.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server SetStreamDimensionHandler... Err : " + ex.ToString());
                WriteErrorJson(ex.ToString(), res);
            }
        }

        public static void GetCurrentAppInfoHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got GetCurrentAppInfoHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                string[] currentTabData = new string[3];
                TabButtons.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    currentTabData = TabButtons.Instance.GetCurrentTabData();
                }));

                JSonWriter info = new JSonWriter();
                info.WriteObjectBegin();
                info.WriteMember("type", currentTabData[0]);
                info.WriteMember("name", currentTabData[1]);
                info.WriteMember("data", currentTabData[2]);
                info.WriteObjectEnd();
                Common.HTTP.Utils.Write(info.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server GetCurrentAppInfoHandler... Err : " + ex.ToString());
                WriteErrorJson(ex.ToString(), res);
            }
        }

        public static void AddStreamViewKeyHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got AddStreamViewKeyHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string label = requestData.data["label"].ToString();
                string jsonString = requestData.data["jsonString"].ToString();

                if (!StreamViewTimeStats.sStreamViewTimeStatsList.ContainsKey(label))
                    new StreamViewTimeStats(label, jsonString);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server AddStreamViewKeyHandler... Err : " + ex.ToString());
                WriteErrorJson(ex.ToString(), res);
            }
        }

        public static void InitStreamHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got InitStreamHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                //HwndSource hwndSource = System.Windows.PresentationSource.FromVisual((System.Windows.Media.Visual)ContentControl.Instance) as HwndSource;
                IntPtr handle = IntPtr.Zero;
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    handle = GameManagerWindow.Instance.Handle;
                }));

                string gmHandle = handle.ToString();
                Process proc = Process.GetCurrentProcess();
                string pid = proc.Id.ToString();

                JSonWriter json = new JSonWriter();
                json.WriteObjectBegin();
                json.WriteMember("handle", gmHandle);
                json.WriteMember("pid", pid);
                json.WriteObjectEnd();
                Common.HTTP.Utils.Write(json.ToString(), res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server InitStreamHandler... Err : " + ex.ToString());
                WriteErrorJson(ex.ToString(), res);
            }
        }

        public static void StreamWindowClosedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got StreamWindowClosedHandler {0} request from {1}",
                               req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                TopBar.Instance.mMaximizeButton.IsEnabled = true;
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server StreamWindowClosedHandler... Err : " + ex.ToString());
                WriteErrorJson(ex.ToString(), res);
            }
        }

        public static void AppDisplayedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got AppDisplayedHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                /*
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }
                */

                string token = requestData.data["token"];
                lock (sLockObject)
                {
                    AppHandler.HandleAppDisplayed(token);
                }

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server AppDisplayedHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void AppLaunchedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got AppLaunchedHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }

                string package = requestData.data["package"];
                string activity = requestData.data["activity"];
                string callingPackage = requestData.data["callingPackage"];
                Logger.Info("package: " + package);
                Logger.Info("activity: " + activity);
                Logger.Info("callingPackage: " + callingPackage);
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    AppHandler.AppLaunched(package, activity, callingPackage);
                }));

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server AppLaunchedHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void AppCrashedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got AppCrashedHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }

                string package = requestData.data["package"];
                Logger.Info("package: " + package);
                package = String.Format("app:{0}", package);
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    TabButtons.Instance.CloseTab(package);
                }));
                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server AppCrashedHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void TabCloseHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got TabCloseHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }

                string package = requestData.data["package"];
                Logger.Info("package: " + package);
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    try
                    {
                        TabButtons.Instance.CloseTab(package);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error("Error {0}", ex.ToString());
                    }
                }));

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server TabCloseHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void ShowAppHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got ShowAppHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }

                string package = requestData.data["package"];
                string activity = requestData.data["activity"];
                string title = requestData.data["title"];

                Logger.Info("package: " + package);
                Logger.Info("activity: " + activity);
                Logger.Info("title : " + title);

                ThreadStart threadDelegate = new ThreadStart(delegate ()
                {
                    GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                    {
                        if (!String.IsNullOrEmpty(package) &&
                            !String.IsNullOrEmpty(activity))
                        {
                            if (String.IsNullOrEmpty(title))
                                AppHandler.ShowApp(null, package, activity, "", true);
                            else
                                AppHandler.ShowApp(title, package, activity, "", true);
                        }
                    }));
                });
                Thread showAppThread = new Thread(threadDelegate);
                showAppThread.IsBackground = true;
                showAppThread.Start();

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server ShowAppHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void ShowWindowHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got ShowWindowHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    GameManagerWindow.Instance.Show();
                }));

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server ShowWindowHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void IsVisibleHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got IsVisibleHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                if (GameManagerWindow.Instance.IsVisible)
                {
                    WriteSuccessJson(res);
                }
                else
                {
                    WriteErrorJson("unused", res);
                }
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server IsVisibleHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void S2PConfiguredHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got S2PConfiguredHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMBasePath);
                key.SetValue("S2PConfigured", "true");
                key.Close();

                /*
                 * We need to make sure that gp tab has opened and finished loading before closing s2p setup tab.
                 * If we do not wait, there is a race condition where the home tab is made active rather than the gp tab.
                 * If s2p setup tab is closed before gp tab is ready, focus is shifted to the home tab
                 */
                Thread.Sleep(5000);
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    TabButtons.Instance.CloseTab(GameManagerUtilities.BSTSERVICES);
                }));

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server S2PConfiguredHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void AppUninstalledHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got AppUninstalledHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }

                string package = requestData.data["package"];
                Logger.Info("package: " + package);
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    AppHandler.AppUninstalled(package);
                }));

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server AppUninstalledHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void ShowWelcomeTabHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                Logger.Info("Switching to Welcome tab");
                TabButtons.Instance.GoToTab(0);
            }));
        }

        public static void ShowWebPageHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got ShowWebPageHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string title = requestData.data["title"].ToString();
                string webUrl = requestData.data["url"].ToString();

                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    GameManagerWindow.Instance.Show();
                    TabButtons.Instance.ShowWebPage(title, webUrl, null);
                }));
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server ShowWebPageHandler : " + ex.ToString());
            }
        }

        public static void GMLaunchWebTab(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got GMLaunchWebTab {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                foreach (string key in requestData.data.AllKeys)
                {
                    Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
                }

                string url = requestData.data["url"];
                string image = requestData.data["image"];
                string name = requestData.data["name"];
                Logger.Info("url: " + url);
                Logger.Info("name: " + name);
                Logger.Info("image: " + image);
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    TabButtons.Instance.ShowWebPage(name, url, image);
                }));

                WriteSuccessJson(res);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in Server GMLaunchWebTab: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static string SendUpdateRequest(Dictionary<string, string> data, string path)
        {
            Logger.Info("Will send {0} request", path);
            string res = null;
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
                int port = (int)key.GetValue("UpdaterServiceServerPort", 2841);
                string url = string.Format("http://127.0.0.1:{0}/{1}", port, path);
                Logger.Info("Sending request to: " + url);

                if (data == null)
                    res = Common.HTTP.Client.Get(url, null, false);
                else
                    res = Common.HTTP.Client.Post(url, data, null, false);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in SendUpdateRequest: " + exc.ToString());
            }
            Logger.Info(res);
            return res;
        }

        public static string StartUpdateRequest(Dictionary<string, string> data, string path)
        {
            Logger.Info("Will send {0} request", path);
            string res = null;
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
                int port = (int)key.GetValue("UpdaterServiceServerPort", 2841);
                string url = string.Format("http://127.0.0.1:{0}/{1}", port, path);
                Logger.Info("Sending request to: " + url);

                if (data == null)
                    res = Common.HTTP.Client.Get(url, null, false);
                else
                    res = Common.HTTP.Client.Post(url, data, null, false);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in SendUpdateRequest: " + exc.ToString());
            }
            Logger.Info(res);
            return res;
        }

        public static void ForceQuitHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Quiting GameManager");

			GameManagerWindow.Instance.ForceClose();
        }

        public static void OpenGoogleHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Opening web page");
            Random rnd = new Random();
            int num = rnd.Next(100) + 1;
            string tabName = "tab_" + num;

            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                TabButtons.Instance.AddWebTab(tabName, "http://www.google.com", null, true);
            }));
        }

        public static void StreamStatusHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Stream Status Handler");
            RequestData requestData = HTTPUtils.ParseRequest(req);

            try
            {
                string isStreaming = requestData.data["isstreaming"];
                //string isRecording = requestData.data["isrecording"];
                if (String.Compare(isStreaming, "true", true) == 0)
                {
                    BTVManager.sStreaming = true;
                }
                else
                {
                    BTVManager.sStreaming = false;
                }
                /*if (String.Compare(isRecording, "true", true) == 0)
                    BTVManager.sRecording = true;
                else
                {
                    Logger.Info("StreamStatusHandler {0}", isRecording);
                    BTVManager.sRecording = false;
                }*/
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        public static void ReplayBufferSavedHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("In Replay Buffer Saved Handler");
            try
            {
                BTVManager.ReplayBufferSaved();
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
        }

        internal static void StopAppRequest(string packageName)
        {
            Logger.Info("Will send stop {0} request", packageName);
            string result = null;
            try
            {
                int frontendPort = Utils.GetFrontendServerPort(Common.Strings.VMName);
                string stopAppUrl = String.Format("http://127.0.0.1:{0}/{1}", frontendPort, Common.Strings.StopAppInfo);
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("appPackage", packageName);
                string res = Common.HTTP.Client.Post(stopAppUrl, data, null, false);
                Logger.Info("the response we get is {0}", res);

                string args = string.Format("StopApp {0}", packageName);
                result = Common.VmCmdHandler.RunCommand(args);
                Logger.Info(result);
            }
            catch (Exception exc)
            {
                Logger.Error("Exception in SendUpdateRequest: " + exc.ToString());
            }
        }

        public static void GameManagerVibrateNotificationHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got GameManagerVibrateNotificationHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());

            try
            {
                if (ResizeManager.ApplicationIsActivated())
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			bool isForceVibrate =  false;
			string isForce = requestData.data["isForce"];
			if (isForce != null && isForce.Equals("true", StringComparison.OrdinalIgnoreCase))
			{
				Logger.Info("the isForceVibrateFlag is set to {0}", isForceVibrate.ToString());
				isForceVibrate = true;
			}
			if (isForceVibrate)
			{
				GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
				{
					try
					{
						GameManagerWindow.Instance.ShakeWindow(2);
					}
					catch (Exception ex)
					{
						Logger.Error("Error {0}", ex.ToString());
					}
				}));
			}
		}
		else
		{
			GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
						{
						FlashWindowHelper.FlashWindowEx(GameManagerWindow.Instance.Handle);
						}));
		}
		WriteSuccessJson(res);
	    }
	    catch (Exception exc)
            {
                Logger.Error("Exception in Server ShakeGameHandler: " + exc.ToString());
                WriteErrorJson(exc.ToString(), res);
            }
        }

        public static void ReportObsErrorHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Obs reported an error. Will restart stream window.");

            RequestData requestData = HTTPUtils.ParseRequest(req);
            string reason = requestData.data["reason"].ToString();

            if (string.Equals(reason, "obs_error", StringComparison.OrdinalIgnoreCase))
            {
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    BTVManager.CloseBTV();
                    DialogResult result = MessageBox.Show(Locale.Strings.GetLocalizedString("OBS_ERROR_TEXT"),
                        "BlueStacks Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error
                        );
                    if (result == DialogResult.Yes)
                    {
                        BTVManager.ShowStreamWindow();
                    }
                }));
            }
            else if (string.Equals(reason, "obs_already_running", StringComparison.OrdinalIgnoreCase))
            {
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    BTVManager.CloseBTV();
                    DialogResult result = MessageBox.Show(Locale.Strings.GetLocalizedString("OBS_ALREADY_RUNNING"),
                        "BlueStacks Error",
                        MessageBoxButtons.YesNo,
                        MessageBoxIcon.Error
                        );
		    if (result == DialogResult.Yes)
                    {
                        BTVManager.ShowStreamWindow();
		    }
                }));
            }
        }

        public static void AttachFrontend(HttpListenerRequest req, HttpListenerResponse res)
        {
            //        try
            //        {
            //            Logger.Info("Attaching frontend");
            //            Thread thread = new Thread(delegate() {
            //                    GameManager.sGameManager.mFrontendHandle = TabBar.sTabBar.SetParentFrontend(GameManager.sGameManager.Handle, true);
            //                    });
            //            thread.IsBackground = true;
            //            thread.Start();
            //            WriteSuccessJson(res);
            //        }
            //        catch(Exception ex)
            //        {
            //            Logger.Error("Error in attaching frontend: {0}", ex.ToString());
            //WriteErrorJson(ex.ToString(), res);
            //        }
        }

        public static void RestartGameManager(HttpListenerRequest req, HttpListenerResponse res)
        {
            try
            {
                Logger.Info("Restarting GameManager");
                RequestData requestData = HTTPUtils.ParseRequest(req);
                bool showMessage = true;
                if (requestData.data["donotshow"] != null)
                    showMessage = false;
                string restartMessage = "";
                string confirm = "";
                var confirmResult = DialogResult.Yes;
                if (showMessage)
                {
                    Logger.Info("Showing restart popup");
                    restartMessage = Locale.Strings.GetLocalizedString("AppRestartConfirmMsg");
                    confirm = Locale.Strings.GetLocalizedString("Confirm");
                    GameManagerWindow.Instance.Dispatcher.Invoke((Action)(() =>
                    {
                        confirmResult = CustomMessageBox.ShowMessageBox(GameManagerWindow.Instance, "BlueStacks",
                        restartMessage,
                        Locale.Strings.GetLocalizedString("YesText"),
                        Locale.Strings.GetLocalizedString("NoText"),
                        Locale.Strings.GetLocalizedString("CancelText"),
                        Locale.Strings.GetLocalizedString("RememberChoiceText"),
                        false);
                    }));
                }
                else
                    Logger.Info("Not showing restart popup");
                if (confirmResult == DialogResult.Yes)
                {
                    Thread th = new Thread(delegate ()
                            {
                                int agentPort = Utils.GetAgentServerPort();
                                string restartUrl = String.Format("http://127.0.0.1:{0}/{1}",
                                    agentPort,
                                    Common.Strings.RestartGameManagerUrl);
                                Logger.Info("Requesting Agent");
                                Common.HTTP.Client.Get(restartUrl, null, false);
                            });

                    th.IsBackground = true;
                    th.Start();
                    GameManagerWindow.Instance.ForceClose();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server RestartGameManager");
                Logger.Error(ex.ToString());
                WriteErrorJson(ex.ToString(), res);
            }
        }

        public static void OfferUrlHandler(HttpListenerRequest req, HttpListenerResponse res)
        {
            Logger.Info("Got OfferUrlHandler {0} request from {1}",
                    req.HttpMethod, req.RemoteEndPoint.ToString());
            try
            {
                RequestData requestData = HTTPUtils.ParseRequest(req);
                string url = requestData.data["url"].ToString();
                string tabName = requestData.data["title"].ToString();

                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    TabButtons.Instance.AddWebTab(tabName, url, null, true);
                }));
                WriteSuccessJson(res);
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in Server OfferUrlHandler");
                Logger.Error(ex.ToString());
                WriteErrorJson(ex.ToString(), res);
            }
        }

		public static void ShowEnableVtPopupHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got ShowEnableVtPopup {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				String url = requestData.data["url"].ToString();
				String title = requestData.data["title"].ToString();

				GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
							{
							GameManagerUtilities.ShowPromotion(title, url);
							}));
				WriteSuccessJson(res);
			}
			catch(Exception e)
			{
				Logger.Error("Exception in ShowEnableVtPopup");
				Logger.Error(e.ToString());
				WriteErrorJson(e.ToString(), res);
			}
		}
    }
}
