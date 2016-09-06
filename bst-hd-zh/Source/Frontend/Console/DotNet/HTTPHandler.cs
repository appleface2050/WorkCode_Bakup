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
using System.Web;
using System.Windows.Forms;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Frontend
{
	public class HTTPHandler
	{

		[DllImport("winmm.dll")]
		public static extern int waveOutGetVolume(IntPtr hwo, out uint dwVolume);

		[DllImport("winmm.dll")]
		public static extern int waveOutSetVolume(IntPtr hwo, uint dwVolume);

		public static void QuitFrontend(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got QuitFrontend {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				WriteSuccessJson(res);
				Environment.Exit(0);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in QuitFrontend: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static Dictionary<String, Common.HTTP.Server.RequestHandler> GetRoutes()
		{
			Dictionary<String, Common.HTTP.Server.RequestHandler> routes = new Dictionary<string, Common.HTTP.Server.RequestHandler>();

			routes.Add("/quitfrontend", HTTPHandler.QuitFrontend);
			routes.Add("/updatetitle", HTTPHandler.UpdateWindowTitle);
			routes.Add("/switchtolauncher", HTTPHandler.SwitchToLauncher);
			routes.Add("/switchtowindows", HTTPHandler.SwitchToWindows);
			routes.Add("/switchorientation", HTTPHandler.SwitchOrientation);
			routes.Add("/showwindow", HTTPHandler.ShowWindow);
			routes.Add("/sharescreenshot", HTTPHandler.ShareScreenshot);
			routes.Add("/togglescreen", HTTPHandler.ToggleScreen);
			routes.Add("/goback", HTTPHandler.GoBack);
			routes.Add("/closescreen", HTTPHandler.CloseScreen);
			routes.Add("/softControlBarEvent", HTTPHandler.HandleSoftControlBarEvent);
			routes.Add("/ping", HTTPHandler.PingHandler);
			routes.Add("/pingvm", HTTPHandler.PingVMHandler);
			routes.Add("/showtileinterface", HTTPHandler.ShowTileInterface);
			routes.Add("/copyfiles", HTTPHandler.SendFilesToWindows);
			routes.Add("/getwindowsfiles", HTTPHandler.PickFilesFromWindows);
			routes.Add("/updategraphicsdrivers", HTTPHandler.UpdateGraphicsDrivers);
			routes.Add("/gpscoordinates", HTTPHandler.UpdateGpsCoordinates);
			routes.Add("/getvolume", HTTPHandler.GetProductVolume);
			routes.Add("/setvolume", HTTPHandler.SetProductVolume);
			routes.Add("/s2pscreen", HTTPHandler.S2PScreenShown);
			routes.Add("/" + Common.Strings.AppDataFEUrl, HTTPHandler.SetCurrentAppData);
			routes.Add("/topDisplayedActivityInfo", HTTPHandler.TopDisplayedActivityInfo);
			routes.Add("/appdisplayed", HTTPHandler.GetAppDisplayedInfo);
			routes.Add("/gohome", HTTPHandler.GoHome);
			routes.Add("/iskeyboardenabled", HTTPHandler.IsKeyboardEnabled);
			routes.Add("/setkeymappingstate", HTTPHandler.SetKeyMappingState);
			routes.Add("/keymap", HTTPHandler.KeyMappingHandler);
			routes.Add("/refreshkeymap", HTTPHandler.RefreshKeyMappingHandler);
			routes.Add("/setfrontendvisibility", HTTPHandler.SetFrontendVisibility);
			routes.Add("/getfesize", HTTPHandler.GetFESize);
			routes.Add("/mute", HTTPHandler.MuteHandler);
			routes.Add("/unmute", HTTPHandler.UnmuteHandler);
			routes.Add("/getcurrentkeymappingstatus", HTTPHandler.IsKeyMappingEnabled);
			routes.Add("/shake", HTTPHandler.ShakeHandler);
			routes.Add("/iskeynamefocussed", HTTPHandler.IsKeyNameFocussed);
			routes.Add("/androidimeselected", HTTPHandler.AndroidImeSelected);
			routes.Add("/isgpssupported", HTTPHandler.IsGPSSupported);
			routes.Add("/" + Common.Strings.RunAppInfo, HTTPHandler.RunAppInfo);
			routes.Add("/" + Common.Strings.StopAppInfo, HTTPHandler.StopAppInfo);
			routes.Add("/zoom", HTTPHandler.ZoomHandler);
			routes.Add("/installapk", HTTPHandler.InstallApk);
			routes.Add("/resizewindow", HTTPHandler.ResizeWindowHandler);
			routes.Add("/injectcopy", HTTPHandler.InjectCopyHandler);
			routes.Add("/injectpaste", HTTPHandler.InjectPasteHandler);
			routes.Add("/tgploginappstatus", HTTPHandler.TGPLoginAppStatus);
			routes.Add("/stopzygote", HTTPHandler.StopZygote);
			routes.Add("/startzygote", HTTPHandler.StartZygote);
			routes.Add("/restartfrontend", HTTPHandler.RestartFrontend);
			routes.Add("/getkeymappingparserversion", HTTPHandler.KeyMappingParserVersion);
			routes.Add("/vibratehostwindow", HTTPHandler.VibrateHostWindowHandler);
			return routes;
		}


		public static void GetProductVolume(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got GetProductVolume {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			uint currVol;
			int result = waveOutGetVolume(IntPtr.Zero, out currVol);
			if (result != 0)
			{
				WriteErrorJson("", res);
				return;
			}
			ushort left_channel_volume = (ushort)(currVol & 0xffff);
			Logger.Info("left_channel_volume = {0}", left_channel_volume);

			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteMember("volume", left_channel_volume);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}

		public static void SetProductVolume(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got SetProductVolume {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			RequestData requestData = HTTPUtils.ParseRequest(req);
			uint newVol = Convert.ToUInt32(requestData.data["vol"]);

			if (newVol > 65535)
			{
				WriteErrorJson("Please give volume between 0 and 65535", res);
				return;
			}

			newVol = (newVol | (newVol << 16));

			int result = waveOutSetVolume(IntPtr.Zero, newVol);
			if (result != 0)
			{
				WriteErrorJson("Unable to set volume", res);
				return;
			}
			WriteSuccessJson(res);
		}

		public static void ShowTileInterface(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got ShowTileInterface {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				/*
				 * Commenting for now. showtileinterface caller needs to be fixed -
				 * call being sent at every app install
				RegistryKey prodKey = Registry.LocalMachine.CreateSubKey(Common.Strings.RegBasePath);
				string installDir = (string)prodKey.GetValue("InstallDir");
				Process.Start(Path.Combine(installDir, "HD-Agent"));

				Thread.Sleep(500);

				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				int port = (int)key.GetValue("AgentServerPort");
				string url = string.Format("http://127.0.0.1:{0}/{1}", port, Common.Strings.ShowTileInterfaceUrl);
				Common.HTTP.Client.Get(url, null, false);
				*/

				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in ShowTileInterface: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void UpdateWindowTitle(HttpListenerRequest req, HttpListenerResponse res)
		{
			return;
			/*
			Logger.Info("HTTPHandler: Got UpdateWindowTitle {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string title = requestData.data["title"];
				Console.UpdateTitle(title);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in UpdateWindowTitle: " + exc.ToString());
				Console.UpdateTitle(Common.Strings.DefaultWindowTitle);
			}
			*/
		}

		public static void MuteHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Audio.Manager.Mute();
		}

		public static void UnmuteHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("UnmuteHandler");
			Audio.Manager.Unmute();
		}

		public static void SetCurrentAppData(HttpListenerRequest req,
			HttpListenerResponse res)
		{
			Logger.Info("SetCurrentAppData");
			Console.s_Console.SetCurrentAppData(req, res);
		}

		public static void GetAppDisplayedInfo(HttpListenerRequest req,
				HttpListenerResponse res)
		{
			Logger.Info("GetAppDisplayedInfo");
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteMember("LastAppDisplayed", Console.sLastAppDisplayed);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}

		static bool IsWinXP()
		{
			OperatingSystem OS = Environment.OSVersion;
			Logger.Info("OS Major = {0}, Minor = {1}, Patform = {2}", OS.Version.Major, OS.Version.Minor, OS.Platform);
			return (OS.Platform == PlatformID.Win32NT) && (OS.Version.Major == 5 && (OS.Version.Minor == 1 || OS.Version.Minor == 2));
		}


		public static void TopDisplayedActivityInfo(HttpListenerRequest req,
				HttpListenerResponse res)
		{
			Logger.Info("TopDisplayedActivityInfo");

			string appToken = "", appDisplayed = "";
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				appToken = requestData.data["appToken"];
				Logger.Info("appToken = " + appToken);

				if (appToken.IndexOf("com.bluestacks.keymappingtool") == -1)
				{
					string[] seperator = new String[] { "ActivityRecord" };
					appDisplayed = appToken.Split(seperator, StringSplitOptions.None)[1];
					seperator = new String[] { "u0 " };

					appDisplayed = appDisplayed.Split(seperator, StringSplitOptions.None)[1].Replace("}", "");
					appDisplayed = appDisplayed.Split(' ')[0];

					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsSendGameManagerRequestWhenActivityInfoDisplayedOnTopInHttpHandler)
					{
						Dictionary<string, string> data = new Dictionary<string, string>();
						data.Add("token", appToken);
						Console.s_Console.SendGameManagerRequest(data, Common.Strings.AppDisplayedUrl);
					}
				}

				if (!String.IsNullOrEmpty(appDisplayed) &&
						String.Compare(appDisplayed, Console.sLastAppDisplayed, true) != 0)
				{
					Logger.Info("appDisplayed = {0}, s_Console.sLastAppDisplayed= {1}", appDisplayed, Console.sLastAppDisplayed);

					lock (Console.sCurrentAppDisplayedLockObject)
					{
						bool isHome = Utils.IsHomeApp(appDisplayed);
						string newPackageName = appDisplayed.Split('/')[0];
						string oldPackageName = Console.sLastAppDisplayed.Split('/')[0];

						Console.sLastAppDisplayed = appDisplayed;
						Logger.Info("sSendTopAppChangeEvents = {0}, sAppPackage = {1}, sAppPackageInfoSent = {2}", Console.sSendTopAppChangeEvents, Console.sAppPackage, Console.sAppPackageInfoSent);
						if (Oem.Instance.IsNotifyChangesToParentWindow == true &&
								Console.sSendTopAppChangeEvents == true &&
								((String.IsNullOrEmpty(Console.sAppPackage) == false &&
								  appDisplayed.ToUpper().Contains(Console.sAppPackage.ToUpper())) ||
								 (Console.sAppPackageInfoSent == true &&
								  (newPackageName.Contains("com.bluestacks.gamepophome") ||
								   newPackageName.Contains("com.bluestacks.appmart")))))
						{
							WindowMessages.NotifyAppDisplayedToParentWindow(isHome);
							Console.sAppPackageInfoSent = true;
							Console.sAppPackage = "";

							if (isHome)
							{
								Console.sSendTopAppChangeEvents = false;
								Console.sAppPackageInfoSent = false;
							}
						}
					}
				}
				Common.HTTP.Utils.Write("true", res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server TopDisplayedActivityInfo appToken = {0} : {1}", appToken, exc.ToString());
				WriteErrorJson(exc.Message, res);
			}

		}

		public static void ShakeHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got ShakeHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string time = requestData.data["time"];

				string strX = requestData.data["x"];
				string strY = requestData.data["y"];
				string strZ = requestData.data["z"];
				float x = 0, y = 0, z = 0;

				if (!float.TryParse(strX, out x) || !float.TryParse(strY, out y) || !float.TryParse(strZ, out z) || x <= 0 || y <= 0 || z <= 0)
				{
					x = y = z = 5;
				}

				Logger.Info("Using x = {0}, y = {1} and z = {2}", x, y, z);

				float seconds;
				if (!float.TryParse(time, out seconds))
					seconds = 2;

				int retries = (int)(seconds * 1000 / (50 + 50));

				BlueStacks.hyperDroid.Frontend.InputMapper inputMapper = BlueStacks.hyperDroid.Frontend.InputMapper.Instance();
				while (retries > 0)
				{
					inputMapper.EmulateShake(x, y, z);
					Thread.Sleep(50);
					inputMapper.EmulateShake(0, 0, 0);
					Thread.Sleep(50);
					inputMapper.EmulateShake(-1 * x, -1 * y, -1 * z);
					retries--;
				}
				inputMapper.EmulateShake(0, 0, 0);
				WriteSuccessJson(res);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Erro: Occured, Err: {0}", e.ToString()));
				WriteErrorJson("Error in api", res);
			}
		}

		public static void InjectCopyHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got InjectCopy {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				SendKeys.SendWait("^C");
			}
			catch (Exception e)
			{
				Logger.Info("Failed to send ctrl + c. Error: " + e.ToString());
			}
		}

		public static void InjectPasteHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got InjectPaste {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				SendKeys.SendWait("^V");
			}
			catch (Exception e)
			{
				Logger.Info("Failed to send ctrl + v. Error: " + e.ToString());
			}
		}

		public static void IsKeyNameFocussed(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Inside keyname focussed");
			Logger.Info("HTTPHandler: Got IsKeyNameFocussed {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string focussedState = requestData.data["state"];
				Logger.Info("The focussed state is " + focussedState);
				if (focussedState == "true")
					Console.s_Console.ChangeImeMode(false);
				else
					Console.s_Console.ChangeImeMode(true);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}

		public static void GoHome(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("GoHome Handler");
			try
			{
				Common.VmCmdHandler.RunCommand("home");
				Common.HTTP.Utils.Write("true", res);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Erro: Occured, Err: {0}", e.ToString()));
				WriteErrorJson("unable to go home", res);
			}
		}

		public static void SwitchToLauncher(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got SwitchToLauncher {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				IntPtr handle = Common.Interop.Window.FindWindow(null, Common.Strings.AppTitle);
				Logger.Info("Sending WM_USER_SWITCH_TO_LAUNCHER to Frontend handle {0}", handle);
				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_USER_SWITCH_TO_LAUNCHER, IntPtr.Zero, IntPtr.Zero);
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server SwitchToLauncher: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void ResizeWindowHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got ResizeWindow {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData reqData = HTTPUtils.ParseRequest(req);
				String strX = reqData.data["x"];
				String strY = reqData.data["y"];
				int x, y;


				Logger.Info("x={0}, y={1}", strX, strY);
				if (!int.TryParse(strX, out x) || !int.TryParse(strY, out y) || x <= 0 || y <= 0)
				{
					int screenWidth = Screen.PrimaryScreen.Bounds.Width;
					int screenHeight = Screen.PrimaryScreen.Bounds.Height;

					Utils.GetWindowWidthAndHeight(screenWidth, screenHeight, out x, out y);
					Logger.Info("Recieved invalid parameters, setting (x, y) to ({0}, {1})", x, y);
				}
				//Peace run in ui thread
				UIHelper.RunOnUIThread(Console.s_Console,
					delegate ()
					{
						Console.s_Console.ResizeClientWindow(x, y);
					});
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server ResizeWindow: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void SwitchToWindows(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got SwitchToWindows {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				IntPtr handle = Common.Interop.Window.FindWindow(null, Common.Strings.AppTitle);
				Logger.Info("Sending WM_CLOSE to Frontend handle {0}", handle);
				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server SwitchToWindows: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void IsKeyMappingEnabled(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got IsKeyMappingEnabled {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				JSonWriter json = new JSonWriter();
				json.WriteObjectBegin();
				json.WriteMember("success", true);
				json.WriteMember("keymapping", Console.s_UserKeyMappingEnabled);
				json.WriteObjectEnd();
				Common.HTTP.Utils.Write(json.ToString(), res);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				WriteErrorJson(e.Message, res);
			}
		}

		public static void SetKeyMappingState(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got SetKeyMappingState {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);

				string keymappingstate = requestData.data["keymapping"];
				if (String.Compare(keymappingstate, "true", true) == 0)
				{
					Console.s_UserKeyMappingEnabled = true;
					Console.s_Console.SetKeyMappingState(Console.s_AutoKeyMappingEnabled);
				}
				else
				{
					Console.s_UserKeyMappingEnabled = false;
					Console.s_Console.SetKeyMappingState(false);
				}
				JSonWriter json = new JSonWriter();
				json.WriteObjectBegin();
				json.WriteMember("success", true);
				json.WriteObjectEnd();
				Common.HTTP.Utils.Write(json.ToString(), res);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				WriteErrorJson(e.Message, res);
			}
		}

		public static void IsKeyboardEnabled(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got IsKeyboardEnabled {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);

				string isInput = requestData.data["isinput"];
				if (String.Compare(isInput, "true", true) == 0)
				{
					Logger.Info("calling change ime with true");
					Console.s_Console.ChangeImeMode(true);
					Console.s_AutoKeyMappingEnabled = false;
				}
				else
				{
					Logger.Info("calling change ime with false");
					Console.s_Console.ChangeImeMode(false);
					Console.s_AutoKeyMappingEnabled = true;
				}

				if (Console.s_UserKeyMappingEnabled == false)
				{
					WriteErrorJson("Cannot entertain this request as the user/keymappingtool has disabled keymapping, Will force this value when user enables keymapping", res);
					return;
				}
				else
				{
					Console.s_Console.SetKeyMappingState(Console.s_AutoKeyMappingEnabled);
					JSonWriter json = new JSonWriter();
					json.WriteObjectBegin();
					json.WriteMember("success", true);
					json.WriteObjectEnd();
					Common.HTTP.Utils.Write(json.ToString(), res);
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				WriteErrorJson(e.Message, res);
			}
		}

		public static void RefreshKeyMappingHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got RefreshKeyMappingHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				Console.s_Console.RefreshKeyMapping();
				WriteSuccessJson(res);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				WriteErrorJson(e.Message, res);
			}
		}

		public static void KeyMappingHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got KeyMappingHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				Console.s_Console.LaunchBlueStacksKeyMapper();
				WriteSuccessJson(res);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				WriteErrorJson(e.Message, res);
			}
		}

		public static void SwitchOrientation(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got SwitchOrientation {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				string orientation = req.QueryString["orientation"];
				try
				{
					int orien = int.Parse(orientation);
					Logger.Info("Orientation change to {0}", orien);
					Console.s_Console.OrientationHandler(orien);

					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyFrontendOrientationChangeToParentWindow)
					{
						WindowMessages.NotifyOrientationChangeToParentWindow(Console.s_Console.mEmulatedPortraitMode);
					}
					
				}
				catch (Exception exc)
				{
					Logger.Info("Got exec in orientation change");
					Logger.Info(exc.ToString());
				}
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in SwitchOrientation: " + exc.ToString());
			}
		}

		public static void TGPLoginAppStatus(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got TGPLoginAppStatus {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				foreach (string key in requestData.data.AllKeys)
				{
					Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);

					WindowMessages.ForwardStatusToParentWindow(requestData.data[key]);
				}
				JSonWriter json = new JSonWriter();
				json.WriteObjectBegin();
				json.WriteMember("success", true);
				json.WriteObjectEnd();
				Common.HTTP.Utils.Write(json.ToString(), res);
			}
			catch (Exception exc)
			{
				Logger.Error("Got exception in Notifying TGP window the we chat login app status {0}", exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void GetFESize(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got GetFESize {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				JSonWriter json = new JSonWriter();
				json.WriteArrayBegin();
				json.WriteObjectBegin();
				json.WriteMember("success", true);
				json.WriteMember("Height", Console.s_Console.Size.Height);
				json.WriteMember("Width", Console.s_Console.Size.Width);
				json.WriteMember("ClientHeight", Console.s_Console.ClientSize.Height);
				json.WriteMember("ClientWidth", Console.s_Console.ClientSize.Width);
				json.WriteObjectEnd();
				json.WriteArrayEnd();
				Common.HTTP.Utils.Write(json.ToString(), res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server SystrayVisibility");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void StopAppInfo(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got StopAppInfo {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string appPackage = requestData.data["appPackage"];

				Logger.Info("Received stop app package = {0}", appPackage);

				if (String.IsNullOrEmpty(appPackage) == false &&
						Console.sLastAppDisplayed.Contains(appPackage) == true)
				{
					if (Console.s_Console.mInputMapper.IsLocationUpdationWithKeyMapEnabled() == 1)
					{
						Console.s_Console.StopGpsLocationProvider();
					}
					Console.sLastAppDisplayed = "";
					Logger.Info("assigned empty value to sLastAppDisplayed");
				}
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in StopAppInfo");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void RunAppInfo(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got RunAppInfo {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string appPackage = requestData.data["appPackage"];

				Logger.Info("Received appPackage = {0}", appPackage);

				if (String.IsNullOrEmpty(appPackage) == false)
				{
					Console.sAppPackage = appPackage;
					Console.sAppPackageInfoSent = false;
					Console.sSendTopAppChangeEvents = true;
				}
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in RunAppInfo");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void SetFrontendVisibility(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got SetFrontendVisibility {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string visibility = requestData.data["visible"];
				string appPackage = requestData.data["appPackage"];

				Logger.Info("Received visible = {0} and appPackage = {1}", visibility, appPackage);

				if (String.IsNullOrEmpty(appPackage) == false)
				{
					Console.sAppPackage = appPackage;
					Console.sAppPackageInfoSent = false;
					Console.sSendTopAppChangeEvents = true;
				}

				if (string.Compare(visibility, "false") == 0)
				{
					UIHelper.RunOnUIThread(Console.s_Console,
						delegate ()
						{
							Logger.Info("Hiding frontend");
							Console.sHideMode = false;
							if (Console.s_Console.ParentForm != null)
							{
								Console.s_Console.ParentForm.Hide();
							}
						});
				}
				else
				{
					UIHelper.RunOnUIThread(Console.s_Console,
						delegate ()
						{
							Logger.Info("Showing frontend");
							Console.sHideMode = false;
							if (Console.s_Console.ParentForm != null)
							{
								Console.s_Console.ParentForm.Show();
							}
						});
				}
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in SetFrontendVisibility");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void ShowWindow(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got ShowWindow {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				UIHelper.RunOnUIThread(Console.s_Console,
						delegate ()
						{
							Console.sHideMode = false;
							Console.s_Console.UserShowWindow();
						});

				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server ShowWindow: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void ShareScreenshot(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got ShareSnapshot {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				Console.s_Console.HandleShareButtonClicked();

				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server ShareSnapshot: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void ToggleScreen(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got ToggleScreen {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				UIHelper.RunOnUIThread(Console.s_Console,
						delegate ()
						{
							Console.s_Console.ToggleFullScreen();
						});

				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server ToggleScreen: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void GoBack(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got BackPress {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				Common.VmCmdHandler.RunCommand("back");
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server BackPress: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void CloseScreen(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got CloseScreen {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				UIHelper.RunOnUIThread(Console.s_Console,
						delegate ()
						{
							//Console.s_Console.Close();
						});

				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server CloseScreen: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void RestartFrontend(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got RestartFrontend {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				Common.Stats.SendMiscellaneousStatsSync("PlusFailureReason", 
						requestData.data["PlusFailureReason"],
						User.GUID, null, null, null);
				Thread thread = new Thread(delegate() {
						try 
						{
						RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
						string installDir = (string)key.GetValue("InstallDir");
						string progName = Path.Combine(installDir, "HD-Restart.exe");
						Process proc = new Process();
						proc.StartInfo.FileName = progName;
						proc.StartInfo.Arguments = "Android hidemode";
						proc.Start();
						} catch (Exception exc) {
						Logger.Error("Failed to restart: " + exc.ToString());
						}
						});

				thread.IsBackground = true;
				thread.Start();

				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in RestartFrontend: " + ex.ToString());
				WriteErrorJson(ex.Message, res);
			}
		}

		public static void VibrateHostWindowHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler : Got VibrateHostWindowHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			int partnerServerPort = Utils.GetPartnerServerPort();
			try
			{
				string url = string.Format("http://127.0.0.1:{0}/{1}", partnerServerPort, "ping");
				Common.HTTP.Client.Get(url, null, false);
			}
			catch
			{
				Logger.Info("gamemanager port not running");
				return;
			}

			try
			{
				Dictionary<string, string> data = new Dictionary<string,string>();
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string isForce = requestData.data["isForce"];
				if (isForce != null && isForce.Equals("true", StringComparison.OrdinalIgnoreCase))
				{
					Logger.Info("isforce flag is set to true");
					data.Add("isForce", "true");
				}

				string url = string.Format("http://127.0.0.1:{0}/{1}", partnerServerPort, "vibratenotification");
				Common.HTTP.Client.Post(url, data, null, false);
				WriteSuccessJson(res);
			}
			catch (Exception ex)
			{
				Logger.Info("Caught exception when sending shake to partner window err:{0}", ex.ToString());
			}

		}

		public static void KeyMappingParserVersion(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got KeyMappingParserVersion {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				string parserVersion = InputMapper.GetKeyMappingParserVersion();
				JSonWriter json = new JSonWriter();
				json.WriteObjectBegin();
				json.WriteMember("parserversion", parserVersion);
				json.WriteMember("success", true);
				json.WriteObjectEnd();
				Common.HTTP.Utils.Write(json.ToString(), res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server Ping");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void HandleSoftControlBarEvent(HttpListenerRequest req, HttpListenerResponse res)
		{
			try
			{
				String visible = req.QueryString["visible"];
				if (visible != null)
				{
					Console.s_Console.SoftControlBarVisible(visible != "0");
				}

			}
			catch (Exception exc)
			{
				Logger.Error("Exception in HandleSoftControlBarEvent: " + exc.ToString());
			}
		}

		public static void PingHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got Ping {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server Ping");
				Logger.Error(exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}
		/// <summary>
		///  ping the bstcommandprocessor
		/// </summary>
		/// <param name="req"></param>
		/// <param name="res"></param>
		/// <returns></returns>
		public static void PingVMHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got pingvm {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}",
					  Common.VmCmdHandler.s_ServerPort,
					  Common.VmCmdHandler.s_CheckGuestReadyPath);

				string strRet = Common.HTTP.Client.Get(url, null, false, 1000);

				IJSonReader json = new JSonReader();
				IJSonObject ret = json.ReadAsJSonObject(strRet);
				string sReceived = ret["result"].StringValue;
				if (sReceived.Equals("ok"))
				{
					WriteSuccessJson(res);
					return;
				}
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server PingVM");
				Logger.Error(exc.ToString());
			}
			WriteErrorJson("", res);
		}

		public static void UpdateGraphicsDrivers(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got UpdateGraphicsDrivers {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			try
			{
				WriteSuccessJson(res);

				UIHelper.RunOnUIThread(Console.s_Console,
						delegate ()
						{
							Console.s_Console.UpdateGraphicsDrivers();
						});
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server UpdateGraphicsDrivers: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void AndroidImeSelected(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got AndroidImeSelected {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				string imeSelected = requestData.data["result"];
				if (!imeSelected.Equals(Common.Strings.LatinImeId))
				{
					bool enableImeMode = false;
					Logger.Info("Android Ime Selected in not latinIme");
					Console.s_Console.ChangeImeMode(enableImeMode);
				}
				Common.Utils.SetImeSelectedInReg(imeSelected);
			}
			catch (Exception ex)
			{
				Logger.Error("Exception in AndroidImeSelected : " + ex.ToString());
			}
		}

		public static void WriteSuccessJson(HttpListenerResponse res)
		{
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}

		public static void WriteErrorJson(String reason, HttpListenerResponse res)
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

		public static void SendSysTrayNotification(string title, string status, string message)
		{
			Logger.Info("Sending Notifications for files sent to windows");

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			int agentPort = (int)key.GetValue("AgentServerPort", 2861);
			string url = String.Format("http://127.0.0.1:{0}/{1}", agentPort, "showtraynotification");

			Dictionary<String, String> data =
				new Dictionary<String, String>();

			data.Add("message", message);
			data.Add("title", title);
			data.Add("status", status);

			/*
			 * We cannot block here.  Do the HTTP post in a
			 * background thread.
			 */

			Thread thread = new Thread(delegate ()
			{

				try
				{
					Common.HTTP.Client.Post(url, data, null,
						false);

				}
				catch (Exception exc)
				{
					Logger.Error(
						"Cannot send orientation to guest: " +
						exc.ToString());
				}
			});

			thread.IsBackground = true;
			thread.Start();
		}

		private static bool IsWindows7AndBelow()
		{
			System.Version win8version = new System.Version(6, 2, 9200, 0);
			if (Environment.OSVersion.Platform == PlatformID.Win32NT &&
					Environment.OSVersion.Version >= win8version)
			{
				return false;
			}
			else
			{
				return true;
			}
		}

		public static void SendSetFrontendPositionRequest(int width, int height)
		{
			Logger.Info("Sending SetFrontendPosition request to gamemanager");

			int gamemanagerPort = Utils.GetPartnerServerPort();
			string url = String.Format("http://127.0.0.1:{0}/{1}", gamemanagerPort, "setfrontendposition");

			Dictionary<String, String> data =
				new Dictionary<String, String>();

			data.Add("width", width.ToString());
			data.Add("height", height.ToString());

			Thread thread = new Thread(delegate ()
			{

				try
				{
					Common.HTTP.Client.Post(url, data, null,
						false);

				}
				catch (Exception exc)
				{
					Logger.Error(
						"Cannot send request to gamemanager: " +
						exc.ToString());
				}
			});

			thread.IsBackground = true;
			thread.Start();
		}

		public static void UpdateGpsCoordinates(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Inside UpdateGpsCoordinates\nHTTPHandler:  {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				int gpsMode = (int)key.GetValue("GpsMode", 0);
				int gpsSource = (int)key.GetValue("GpsSource", 0);
				string gpsLatitude = (string)key.GetValue("GpsLatitude", null);
				string gpsLongitude = (string)key.GetValue("GpsLongitude", null);

				if (gpsMode == 0 || gpsSource == 0 || (gpsSource != 8 && IsWindows7AndBelow()))
				{
					Logger.Info(string.Format("Stopping Gps Service, gpsMode = {0}, gpsSource = {1}, IsWindows7AndBelow() = {2}", gpsMode, gpsSource, IsWindows7AndBelow()));
					Logger.Info("No Coordinates Available so far");
					Common.HTTP.Utils.Write("", res);
					return;
				}

				Common.HTTP.Utils.Write(gpsLatitude + "," + gpsLongitude, res);
				return;
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err : ", e.ToString()));
				Common.HTTP.Utils.Write("exception", res);
			}
		}

		public static void PickFilesFromWindows(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Inside PickFilesFromWindows\nHTTPHandler: Got Pick Files To From Windows {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			RequestData requestData = HTTPUtils.ParseRequest(req);

			try
			{
				foreach (string key in requestData.data.AllKeys)
					Logger.Info(string.Format("Key = {0}, Value = {1}", key, requestData.data[key]));

				string bstSharedFolder = Common.Strings.SharedFolderDir;
				string[] outFiles;
				string filters = "";
				OpenFileDialog fileChooserDialog = new OpenFileDialog();

				if (String.Compare(requestData.data["filesNo"].ToUpper(), "MULTIPLE") == 0)
					fileChooserDialog.Multiselect = true;
				fileChooserDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);

				if (requestData.data["mimeType"].ToUpper().Contains("VIDEO") ||
						requestData.data["mimeType"].ToUpper().Contains("AUDIO"))
				{
					filters = "Video & Audio Files | *.dat; *.wmv; *.3g2; *.3gp; *.3gp2; *.3gpp; *.amv; *.asf;" +
						"*.avi; *.bin; *.cue; *.divx; *.dv; *.flv; *.gxf; *.iso; *.m1v;" +
						"*.m2v; *.m2t; *.m2ts; *.m4v; *.mkv; *.mov; *.mp2; *.mp2v; *.mp4;" +
						"*.mp4v; *.mpa; *.mpe; *.mpeg; *.mpeg1; *.mpeg2; *.mpeg4; *.mpg;" +
						"*.mpv2; *.mts; *.nsv; *.nuv; *.ogg; *.ogm; *.ogv; *.ogx; *.ps; *.rec;" +
						"*.rm; *.rmvb; *.tod; *.ts; *.tts; *.vob; *.vro; *.webm; *.mp3";
				}

				else if (requestData.data["mimeType"].ToUpper().Contains("IMAGE"))
				{
					filters = "Image Files|*.jpg;*.jpeg;*.png;*.gif";
					fileChooserDialog.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyPictures);
				}

				else if (filters.Length < 1)
				{
					filters = "All Files|*.*";
				}

				fileChooserDialog.Filter = filters;

				if (fileChooserDialog.ShowDialog(Console.s_Console) == DialogResult.OK)
				{
					outFiles = new String[fileChooserDialog.SafeFileNames.Length];
					for (int i = 0; i < fileChooserDialog.SafeFileNames.Length; i++)
					{
						outFiles[i] = fileChooserDialog.SafeFileNames[i];
						if (File.Exists(Path.Combine(bstSharedFolder, fileChooserDialog.SafeFileNames[i])))
							outFiles[i] = NextAvailableFileName(bstSharedFolder, fileChooserDialog.SafeFileNames[i]);
						Logger.Info(string.Format("Will Copy : {0}  to {1}",
									fileChooserDialog.FileNames[i],
									Path.Combine(bstSharedFolder, outFiles[i])));
					}

					for (int i = 0; i < outFiles.Length; i++)
					{
						Logger.Info(string.Format("Copying : {0}  to {1}",
									fileChooserDialog.FileNames[i],
									Path.Combine(bstSharedFolder, outFiles[i])));
						File.Copy(fileChooserDialog.FileNames[i], Path.Combine(bstSharedFolder, outFiles[i]));
					}

					Common.HTTP.Utils.Write(string.Join("\t", outFiles), res);
					Logger.Info("Response Sent: Copied");
				}
				else
				{
					Logger.Info("Response Sent: false");
					Common.HTTP.Utils.Write("false", res);
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
				Common.HTTP.Utils.Write("false", res);
			}
		}

		private static string NextAvailableFileName(string sharedFolder, string fileName)
		{
			Logger.Info("Inside NextAvailableFileName");
			int i = 1;
			string outFileName = fileName;
			string[] fileParts = new string[] { Path.GetFileNameWithoutExtension(fileName), Path.GetExtension(fileName) };
			Logger.Info(string.Format("fileName = {0}, fileExtension = {1}", fileParts[0], fileParts[1]));
			while (File.Exists(Path.Combine(sharedFolder, outFileName)))
			{
				outFileName = fileParts[0] + i.ToString() + fileParts[1];
				i++;
			}
			return outFileName;
		}

		public static void SendMultipleFilesToWindows(RequestData requestData, HttpListenerResponse res)
		{
			Logger.Info("Multiple Files");
			DialogResult result;
			string responseStringSuccess = null, responseStringFailure = null;
			string bstSharedFolder = Common.Strings.SharedFolderDir;
			string userCopyDir;

			FolderBrowserDialog setCopyDir = new FolderBrowserDialog();
			setCopyDir.ShowNewFolderButton = true;
			setCopyDir.Description = "Choose folder to copy files";
			setCopyDir.RootFolder = Environment.SpecialFolder.MyComputer;
			result = setCopyDir.ShowDialog(Console.s_Console);

			if (result.Equals(DialogResult.OK))
			{
				userCopyDir = (string)setCopyDir.SelectedPath;
				Logger.Debug(string.Format("User Select {0} Directory", userCopyDir));
			}
			else
			{
				Logger.Info("User cancelled browser dialog");
				foreach (string key in requestData.data.AllKeys)
				{
					try
					{
						File.Delete(Path.Combine(bstSharedFolder, requestData.data[key].Trim()));
					}
					catch (Exception e)
					{
						Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
					}
				}
				Common.HTTP.Utils.Write("false", res);
				return;
			}

			foreach (string key in requestData.data.AllKeys)
			{
				try
				{
					Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
					if (File.Exists(Path.Combine(bstSharedFolder, requestData.data[key].Trim())))
					{
						if (String.Compare(userCopyDir, bstSharedFolder, false) != 0)
						{
							if (File.Exists(Path.Combine(userCopyDir, requestData.data[key].Trim())))
							{
								result = MessageBox.Show(
										Console.s_Console,
										string.Format("Overwrite {0}?", requestData.data[key].Trim()),
										"File already exists",
										MessageBoxButtons.YesNo,
										MessageBoxIcon.Information);
								if (result == DialogResult.No)
								{
									File.Delete(Path.Combine(bstSharedFolder, requestData.data[key].Trim()));
									continue;
								}
								if (File.Exists(Path.Combine(userCopyDir, requestData.data[key].Trim())))
									File.Delete(Path.Combine(userCopyDir, requestData.data[key].Trim()));
							}
							File.Move(Path.Combine(bstSharedFolder, requestData.data[key].Trim()),
									Path.Combine(userCopyDir, requestData.data[key].Trim()));
						}

						if (responseStringSuccess == null)
							responseStringSuccess = Path.Combine(userCopyDir, requestData.data[key].Trim());
						else
							responseStringSuccess += "\n" + Path.Combine(userCopyDir, requestData.data[key].Trim());
					}
					else
					{
						if (responseStringFailure == null)
							responseStringFailure = Path.Combine(userCopyDir, requestData.data[key].Trim());
						else
							responseStringFailure += "\n" + Path.Combine(userCopyDir, requestData.data[key].Trim());
						Logger.Error(string.Format("{0} does not exist in sharedfolder", requestData.data[key]));
					}
				}
				catch (Exception e)
				{
					Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
					if (responseStringFailure == null)
						responseStringFailure = Path.Combine(userCopyDir, requestData.data[key].Trim());
					else
						responseStringFailure += "\n" + Path.Combine(userCopyDir, requestData.data[key].Trim());
				}
			}
			if (responseStringSuccess != null)
				SendSysTrayNotification("Successfully copied files:", "success", responseStringSuccess);
			if (responseStringFailure != null)
				SendSysTrayNotification("Cannot copy files:", "error", responseStringFailure);

			Common.HTTP.Utils.Write("true", res);
		}

		public static void SendSingleFileToWindows(RequestData requestData, HttpListenerResponse res)
		{
			Logger.Info("Single File");
			DialogResult result = DialogResult.Cancel;
			SaveFileDialog fileSaver;

			string fileKey = null;
			foreach (string key in requestData.data.AllKeys)
			{
				fileKey = key;
				break;
			}

			string responseStringSuccess = null, responseStringFailure = null;
			string bstSharedFolder = Common.Strings.SharedFolderDir;
			string userDefinedName;
			string fileName = null;

			if (fileKey == null)
				Common.HTTP.Utils.Write("false", res);

			string ext = Path.GetExtension(requestData.data[fileKey]).Replace(".", "");
			UIHelper.RunOnUIThread(Console.s_Console, delegate ()
					{
					Console.s_Console.DisableShootingModeIfEnabled();

					Logger.Debug(string.Format("File Extension = {0}", ext));
					fileSaver = new SaveFileDialog();
					fileSaver.Filter = ext + " files (*." + ext + ")| *." + ext;
					fileSaver.AddExtension = true;
					fileSaver.Title = "Save File";
					fileSaver.AutoUpgradeEnabled = true;
					fileSaver.CheckPathExists = true;
					fileSaver.DefaultExt = ext;
					fileSaver.InitialDirectory = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
					fileSaver.FileName = requestData.data[fileKey];
					fileSaver.OverwritePrompt = true;
					fileSaver.ValidateNames = true;
					result = fileSaver.ShowDialog(Console.s_Console);
					fileName = (string)fileSaver.FileName;
					});

			if (result.Equals(DialogResult.OK))
			{
				userDefinedName = fileName;
				Logger.Info(string.Format("User Selected {0} Path", userDefinedName));
			}
			else
			{
				Logger.Info("User cancelled save file dialog");
				try
				{
					File.Delete(Path.Combine(bstSharedFolder, requestData.data[fileKey].Trim()));
				}
				catch (Exception e)
				{
					Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
				}
				Common.HTTP.Utils.Write("false", res);
				return;
			}

			try
			{
				Logger.Info("Key: {0}, Value: {1}", fileKey, requestData.data[fileKey]);
				if (File.Exists(Path.Combine(bstSharedFolder, requestData.data[fileKey].Trim())))
				{
					if (String.Compare(userDefinedName, Path.Combine(bstSharedFolder, requestData.data[fileKey]), false) != 0)
					{
						if (File.Exists(userDefinedName))
							File.Delete(userDefinedName);
						File.Move(Path.Combine(bstSharedFolder, requestData.data[fileKey].Trim()),
								userDefinedName);
					}
					responseStringSuccess = userDefinedName;
				}
				else
				{
					responseStringFailure = userDefinedName;
					Logger.Error(string.Format("{0} does not exist in sharedfolder", requestData.data[fileKey]));
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				responseStringFailure = userDefinedName;
			}
			if (responseStringSuccess != null)
				SendSysTrayNotification("Successfully copied files:", "success", responseStringSuccess);
			else if (responseStringFailure != null)
				SendSysTrayNotification("Cannot copy files:", "error", responseStringFailure);

			Common.HTTP.Utils.Write("true", res);
		}

		public static void SendFilesToWindows(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Inside SendFilesToWindows\nHTTPHandler: Got Send Files To Windows {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			RequestData requestData = HTTPUtils.ParseRequest(req);

			if (requestData.data.Count > 1)
			{
				SendMultipleFilesToWindows(requestData, res);
			}
			else
			{
				SendSingleFileToWindows(requestData, res);
			}
		}

		public static void S2PScreenShown(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: Got S2PScreenShown {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());

			Console.s_Console.S2PScreenShown();
			WriteSuccessJson(res);
		}

		public static void ZoomHandler(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("HTTPHandler: ZoomHandler {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				RequestData reqData = HTTPUtils.ParseRequest(req);
				String direction = reqData.data["direction"];
				String strX = reqData.data["x"];
				String strY = reqData.data["y"];
				Logger.Info("direction={0}, x={1}, y={2}", direction, strX, strY);
				float x = 0.5f;
				float y = 0.5f;
				bool zoomIn = false;
				{
					if (!float.TryParse(strX, out x))
					{
						x = 0.5f;
					}
					if (!float.TryParse(strY, out y))
					{
						y = 0.5f;
					}

					if (direction.ToLower() == "in")
					{
						zoomIn = true;
					}
				}
				BlueStacks.hyperDroid.Frontend.InputMapper mInputMapper = BlueStacks.hyperDroid.Frontend.InputMapper.Instance();
				Logger.Info("zoomIn={0}, x={1}, y={2}", zoomIn, x, y);
				mInputMapper.EmulatePinch(x, y, zoomIn);
				WriteSuccessJson(res);

			}
			catch (Exception exc)
			{
				Logger.Error("Exception in ZoomHandler: " + exc.ToString());
				WriteErrorJson(exc.Message, res);
			}
		}

		public static void IsGPSSupported(HttpListenerRequest req, HttpListenerResponse res)
		{
			Logger.Info("Got request IsGPSSupported :  {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
			try
			{
				if (Utils.IsOSWinXP() || Utils.IsOSWin7() || Utils.IsOSVista())
				{
					WriteErrorJson("not supported", res);
					return;
				}
				else
				{
					RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
					int isSupported = (int)regKey.GetValue("GPSAvailable", 0);
					if (isSupported == 1)
					{
						WriteSuccessJson(res);
					}
					else
					{
						WriteErrorJson("not supported", res);
					}
					return;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed to get GPS device status. Error: " + e.ToString());
				WriteErrorJson("not supported", res);
				return;
			}
		}

		public static void InstallApk(HttpListenerRequest req, HttpListenerResponse res)
		{
			UIHelper.RunOnUIThread(Console.s_Console, delegate ()
			{
				Logger.Info("Got request InstallAPk :  {0} request from {1}",
					req.HttpMethod, req.RemoteEndPoint.ToString());
				try
				{
					OpenFileDialog dialog = new OpenFileDialog();

					dialog.Filter = "Android Files (.apk)|*.apk";
					DialogResult dialogResult = dialog.ShowDialog(Console.s_Console);
					if (dialogResult == DialogResult.OK)
					{
						Logger.Info("File Selected : " + dialog.FileName);
						string apkPath = dialog.FileName;
						Logger.Info("Console: Installing apk: {0}", apkPath);

						Thread apkInstall = new Thread(delegate ()
							{
								Utils.CallApkInstaller(apkPath, false);
							});
						apkInstall.IsBackground = true;
						apkInstall.Start();

						Logger.Info("Got out of Installing APK");
					}
				}
				catch (Exception e)
				{
					Logger.Error("Failed to get install apk. Error: " + e.ToString());
					WriteErrorJson(e.ToString(), res);
					return;
				}
				Logger.Info("Operation Performed");
			});
		}

		public static void StopZygote(HttpListenerRequest req, HttpListenerResponse res)
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Logger.Info("Got request for stopzygote for vm : " + requestData.data["vmName"]);
			string vmName = requestData.data["vmName"];
			try
			{
				BlueStacks.hyperDroid.Frontend.Interop.Opengl.StopZygote(vmName);

			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}

		public static void StartZygote(HttpListenerRequest req, HttpListenerResponse res)
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Logger.Info("Got request for startzygote for vm : " + requestData.data["vmName"]);
			string vmName = requestData.data["vmName"];
			try
			{
				BlueStacks.hyperDroid.Frontend.Interop.Opengl.StartZygote(vmName);

			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}
	}
}
