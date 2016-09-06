/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Library.
 * This file implements interfaces for computing Blustacks usage stats
 * and uploading it to Cloud.
 */

using System;
using System.IO;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Globalization;
using System.Threading;
using Microsoft.Win32;
using System.ServiceProcess;
using System.Collections.Generic;

using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;

using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Common
{
	enum HD_VIRT_TYPE
	{
		HD_VIRT_TYPE_LEGACY = 0,
		HD_VIRT_TYPE_VMX = 1,
		HD_VIRT_TYPE_SVM = 2,
	};

	public class Stats
	{
		[DllImport("HD-SystemDeviceInfo.dll", CharSet = CharSet.Auto)]
		public static extern int GetOrientationInfo(
					int initializeCom);

		[DllImport("HD-SystemDeviceInfo.dll", CharSet = CharSet.Auto)]
		public static extern int GetLocationInfo(
					int initializeCom);

		[DllImport("HD-SystemDeviceInfo.dll", CharSet = CharSet.Auto)]
		public static extern int GetMotionInfo(
					int initializeCom);

		public enum AppType
		{
			app,
			market,
			suggestedapps,
			web
		};

		public const string AppInstall = "true";
		public const string AppUninstall = "false";
		private static string sSessionId;

		private static string SessionId
		{
			get
			{
				if (sSessionId == null)
				{
					ResetSessionId();
				}
				return sSessionId;
			}
			set { sSessionId = value; }
		}

		public static string GetSessionId()
		{
			return SessionId;
		}

		public static string ResetSessionId()
		{
			SessionId = Timestamp;
			return SessionId;
		}

		public static void GetUsageStats(
				out string diskUsage,
				out string appUsage
				)
		{
			// Get installed app count by parsing apps.json
			int installedAppCount = JsonParser.GetInstalledAppCount();
			appUsage = String.Format("{0} {1} {2}",
					installedAppCount,
					(installedAppCount > 1 ? Locale.Strings.Apps : Locale.Strings.App),
					Locale.Strings.Installed);

			diskUsage = Locale.Strings.DiskUsageUnavailable;

			ServiceController sc = new ServiceController(Common.Strings.AndroidServiceName);
			if (sc.Status != ServiceControllerStatus.Running)
				return;

			// Get disk usage
			string url = String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, Common.Strings.GetDiskUsage);
			string res = null;
			try
			{
				res = Common.HTTP.Client.Get(url, null, false, 500);
			}
			catch (Exception e)
			{
				Logger.Error("Exception in GetUsageStats: {0}", e.Message);
			}
			if (res == null)
			{
				Logger.Error("Failed to getUsageStats.");
				return;
			}

			try
			{
				JSonReader readjson = new JSonReader();
				IJSonObject jsonResp = readjson.ReadAsJSonObject(res);

				string error = jsonResp["result"].StringValue;
				if (String.Compare(error, "ok", true) == 0)
				{
					// all sizes in MB
					double sdCardAvailSpace = Convert.ToDouble(jsonResp["diskUsage"][0]["sdCardAvailSize"].StringValue) / (1024 * 1024);
					double sdCardTotalSize = Convert.ToDouble(jsonResp["diskUsage"][0]["sdCardTotalSize"].StringValue) / (1024 * 1024);
					double dataFSAvailSpace = Convert.ToDouble(jsonResp["diskUsage"][1]["dataFSAvailSize"].StringValue) / (1024 * 1024);
					double dataFSTotalSize = Convert.ToDouble(jsonResp["diskUsage"][1]["dataFSTotalSize"].StringValue) / (1024 * 1024);

					double totalDiskSize = sdCardTotalSize + dataFSTotalSize;
					double totalDiskAvailable = sdCardAvailSpace + dataFSAvailSpace;

					double diskUsagePercent = ((totalDiskSize - totalDiskAvailable) / totalDiskSize) * 100;

					diskUsage = String.Format("{0}% {1} {2} MB {3}",
							Convert.ToInt32(diskUsagePercent),
							Locale.Strings.Of,
							Convert.ToInt32(totalDiskSize),
							Locale.Strings.DiskUsed);
				}
			}
			catch (Exception e)
			{
				Logger.Error("Exception in GetUsageStats");
				Logger.Error(e.ToString());
			}
		}

		public static void UploadCrashReport(
				string packageName,
				string versionCode,
				string versionName
				)
		{
			Thread uploadThread = new Thread(delegate ()
			{
				try
				{
					UploadCrashReportHelper(packageName, versionCode, versionName);
				}
				catch (Exception ex)
				{
					Logger.Error("Failed to upload crash report. err: " + ex.ToString());
					// Ignore and continue...
				}
			});
			uploadThread.IsBackground = true;
			uploadThread.Start();
		}

		public static void UploadCrashReportHelper(
				string packageName,
				string versionCode,
				string versionName
				)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Strings.HKLMAndroidConfigRegKeyPath);
			RegistryKey hostKey = Registry.LocalMachine.OpenSubKey(Strings.HKCURegKeyPath);

			string userEmail;

			userEmail = (string)hostKey.GetValue("Email", "null");

			string hostURL = (string)hostKey.GetValue("Host");
			string url = String.Format("{0}/{1}", hostURL, Common.Strings.UploadCrashUrl);

			Dictionary<String, String> postData = new Dictionary<String, String>();
			postData.Add("email", userEmail);
			postData.Add("package_name", packageName);
			postData.Add("version_code", versionCode);
			postData.Add("version_name", versionName);

			HTTP.Client.Post(url, postData, null, true);
		}

		public static void SendAppStats(
				string appName,
				string packageName,
				string appVersion,
				string homeVersion,
				AppType appType
				)
		{
			Thread appStatsThread = new Thread(delegate ()
			{
				try
				{
					string url = String.Format("{0}/{1}", Service.Host, Common.Strings.AppClickStatsUrl);

					RegistryKey key = Registry.LocalMachine.OpenSubKey(Strings.HKLMAndroidConfigRegKeyPath);
					string fbUserID = (string)key.GetValue("FacebookUserId", "0");

					Dictionary<String, String> data = new Dictionary<String, String>();
					data.Add("email", GetURLSafeBase64String(Email));
					data.Add("app_name", GetURLSafeBase64String(appName));
					data.Add("app_pkg", GetURLSafeBase64String(packageName));
					data.Add("app_ver", GetURLSafeBase64String(appVersion));
					data.Add("home_app_ver", GetURLSafeBase64String(homeVersion));
					data.Add("user_time", GetURLSafeBase64String(Timestamp));
					data.Add("app_type", GetURLSafeBase64String(appType.ToString()));

					Logger.Info("Sending App Stats for: {0}", appName);
					string res = HTTP.Client.Post(url, data, null, false);
					Logger.Info("Got App Stat response: {0}", res);
				}
				catch (Exception e)
				{
					Logger.Error("Failed to send app stats. error: " + e.ToString());
				}
			});
			appStatsThread.IsBackground = true;
			appStatsThread.Start();
		}

		public static void SendWebAppChannelStats(
				string appName,
				string packageName,
				string homeVersion
				)
		{
			Thread appStatsThread = new Thread(delegate ()
			{
				string url = String.Format("{0}/{1}", Service.Host, Common.Strings.WebAppChannelClickStatsUrl);

				Dictionary<String, String> data = new Dictionary<String, String>();
				data.Add("app_name", GetURLSafeBase64String(appName));
				data.Add("app_pkg", GetURLSafeBase64String(packageName));
				data.Add("home_app_ver", GetURLSafeBase64String(homeVersion));
				data.Add("user_time", GetURLSafeBase64String(Timestamp));
				data.Add("email", GetURLSafeBase64String(Email));

				try
				{
					string res;
					Logger.Info("Sending Channel App Stats for: {0}", appName);
					res = HTTP.Client.Post(url, data, null, false);
					Logger.Info("Got Channel App Stat response: {0}", res);
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
				}
			});
			appStatsThread.IsBackground = true;
			appStatsThread.Start();
		}

		public static void SendSearchAppStats(
				string keyword
				)
		{
			Thread appStatsThread = new Thread(delegate ()
			{
				string url = String.Format("{0}/{1}", Service.Host, Common.Strings.SearchAppStatsUrl);

				Dictionary<String, String> data = new Dictionary<String, String>();
				data.Add("keyword", keyword);

				try
				{
					string res;
					Logger.Info("Sending Search App Stats for: {0}", keyword);
					res = HTTP.Client.Post(url, data, null, false);
					Logger.Info("Got Search App Stat response: {0}", res);
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
				}
			});
			appStatsThread.IsBackground = true;
			appStatsThread.Start();
		}

		public static void SendAppInstallStats(
				string appName,
				string packageName,
				string appVersion,
				string appInstall,
				string isUpdate,
				string source
				)
		{
			Thread appStatsThread = new Thread(delegate ()
			{
				string url = String.Format("{0}/{1}", Service.Host, Common.Strings.AppInstallStatsUrl);

				Dictionary<String, String> data = new Dictionary<String, String>();
				data.Add("email", GetURLSafeBase64String(Email));
				data.Add("app_name", GetURLSafeBase64String(appName));
				data.Add("app_pkg", GetURLSafeBase64String(packageName));
				data.Add("app_ver", GetURLSafeBase64String(appVersion));
				data.Add("is_install", GetURLSafeBase64String(appInstall));
				data.Add("is_update", GetURLSafeBase64String(isUpdate));
				data.Add("user_time", GetURLSafeBase64String(Timestamp));
				data.Add("install_source", GetURLSafeBase64String(source));

				try
				{
					Logger.Info("Sending App Install Stats for: {0}", appName);
					string res = HTTP.Client.Post(url, data, null, false);
					Logger.Info("Got App Install Stat response: {0}", res);
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
				}
			});
			appStatsThread.IsBackground = true;
			appStatsThread.Start();
		}

		public static void SendSystemInfoStats()
		{
			SendSystemInfoStatsAsync(null, true, null, null);
		}

		public static void SendSystemInfoStatsAsync(
				string host,
				bool createRegKey,
				Dictionary<string, string> dataInfo,
				string guid)
		{
			Thread systemStatsThread = new Thread(delegate ()
			{
				SendSystemInfoStatsSync(host, createRegKey, dataInfo, guid);
			});
			systemStatsThread.IsBackground = true;
			systemStatsThread.Start();
		}
		public static string SendSystemInfoStatsSync(
				string host,
				bool createRegKey,
				Dictionary<string, string> dataInfo,
				string guid)
		{
			return SendSystemInfoStatsSync(host, createRegKey, dataInfo, guid, null);
		}
		public static string SendSystemInfoStatsSync(
				string host,
				bool createRegKey,
				Dictionary<string, string> dataInfo,
				string guid, string programFilesDir)
		{
			string res = "not sent";
			try
			{
				Dictionary<string, string> deviceInfo = Device.Profile.Info();

				Logger.Info("Got Device Profile Info:");
				foreach (KeyValuePair<string, string> info in deviceInfo)
				{
					Logger.Info(info.Key + " " + info.Value);
				}

				if (host == null)
					host = Service.Host;
				string url = String.Format("{0}/{1}", host, Common.Strings.SystemInfoStatsUrl);

				Dictionary<String, String> data = new Dictionary<String, String>();
				data.Add("p", GetURLSafeBase64String(deviceInfo["Processor"]));
				data.Add("nop", GetURLSafeBase64String(deviceInfo["NumberOfProcessors"]));
				data.Add("g", GetURLSafeBase64String(deviceInfo["GPU"]));
				data.Add("gd", GetURLSafeBase64String(deviceInfo["GPUDriver"]));
				data.Add("o", GetURLSafeBase64String(deviceInfo["OS"]));
				data.Add("osv", GetURLSafeBase64String(deviceInfo["OSVersion"]));
				data.Add("sr", GetURLSafeBase64String(deviceInfo["ScreenResolution"]));
				data.Add("dnv", GetURLSafeBase64String(deviceInfo["DotNetVersion"]));
				data.Add("osl", GetURLSafeBase64String(CultureInfo.CurrentCulture.Name.ToLower()));
				data.Add("oem_info", GetURLSafeBase64String(deviceInfo["OEMInfo"]));
				data.Add("ram", GetURLSafeBase64String(deviceInfo["RAM"]));
				data.Add("machine_type", GetURLSafeBase64String(deviceInfo["OSVERSIONTYPE"]));

				if (dataInfo != null)
				{
					data.Add("glmode", GetURLSafeBase64String(dataInfo["GlMode"]));
					data.Add("glrendermode", GetURLSafeBase64String(dataInfo["GlRenderMode"]));
					data.Add("gl_vendor", GetURLSafeBase64String(dataInfo["GlVendor"]));
					data.Add("gl_renderer", GetURLSafeBase64String(dataInfo["GlRenderer"]));
					data.Add("gl_version", GetURLSafeBase64String(dataInfo["GlVersion"]));
					data.Add("bstr", GetURLSafeBase64String(dataInfo["BlueStacksResolution"]));
					if (dataInfo.ContainsKey("gl_check"))
						data.Add("gl_check", GetURLSafeBase64String(dataInfo["gl_check"]));
				}
				else
				{
					data.Add("bstr", GetURLSafeBase64String(deviceInfo["BlueStacksResolution"]));
					data.Add("glmode", GetURLSafeBase64String(deviceInfo["GlMode"]));
					data.Add("glrendermode", GetURLSafeBase64String(deviceInfo["GlRenderMode"]));
				}

				if (string.IsNullOrEmpty(programFilesDir))
				{
					try
					{
						RegistryKey regKey = Registry.LocalMachine.CreateSubKey(Common.Strings.RegBasePath);
						programFilesDir = (string)regKey.GetValue("InstallDir");
					}
					catch
					{
						Logger.Info("Assuming called from msi ignoring registry not found error");
						programFilesDir = "";
					}
				}
				try
				{
					string glVendor = "", glRenderer = "", glVersion = "";
					int dxCheck9 = Utils.GetGraphicsInfo(programFilesDir + "\\HD-GLCheck.exe", "2",
							out glVendor, out glRenderer, out glVersion, false);
					int dxCheck11 = Utils.GetGraphicsInfo(programFilesDir + "\\HD-GLCheck.exe", "3",
							out glVendor, out glRenderer, out glVersion, false);
					int glCheck = Utils.GetGraphicsInfo(programFilesDir + "\\HD-GLCheck.exe", "1",
							out glVendor, out glRenderer, out glVersion, false);

					string dx9 = "", dx11 = "", gl = "";
					if (dxCheck9 == 0)
						dx9 = "1";
					else
						dx9 = "0";

					if (dxCheck11 == 0)
						dx11 = "1";
					else
						dx11 = "0";

					if (glCheck == 0)
						gl = "1";
					else
						gl = "0";
					data.Add("dx9check", GetURLSafeBase64String(dx9));
					data.Add("dx11check", GetURLSafeBase64String(dx11));
					data.Add("gl_check", GetURLSafeBase64String(gl));
				}
				catch (Exception ex)
				{
					Logger.Error("got exception when checking dxcheck and glcheck for sending to systeminfostats ex:{0}", ex.ToString());
				}
				int iOrientation = GetOrientationInfo(0);
				if (iOrientation != 0)
				{
					Logger.Info("No Orientation Device Found");
				}
				int iLocation = GetLocationInfo(0);
				if (iLocation != 0)
				{
					Logger.Info("No Location Device Found");
				}
				int iMotion = GetMotionInfo(0);
				if (iMotion != 0)
				{
					Logger.Info("No Motion Device Found");
				}

				bool bTwoCamera = false;
				if (false == Utils.CheckTwoCameraPresentOnDevice(ref bTwoCamera))
				{
					Logger.Error("Check for Two Camera Present on Device Failed");
				}
				Logger.Info("Two Camera present on Device: " + bTwoCamera);

				data.Add("location_sensor", iLocation == 0 ? "1" : "0");
				data.Add("orientation_sensor", iOrientation == 0 ? "1" : "0");
				data.Add("motion_sensor", iMotion == 0 ? "1" : "0");
				data.Add("two_camera", bTwoCamera ? "1" : "0");

				Logger.Info("LocationSensor Value: " + data["location_sensor"]);
				Logger.Info("OrientationSensor Value: " + data["orientation_sensor"]);
				Logger.Info("MotionSensor Value: " + data["motion_sensor"]);
				Logger.Info("TwoCamera Value: " + data["two_camera"]);
				if (guid != null)
				{
					data.Add("guid", GetURLSafeBase64String(guid));
				}

				Logger.Info("Sending System Info Stats");
				res = HTTP.Client.Post(url, data, null, false, 10000);
				Logger.Info("Got System Info  response: {0}", res);
				if (createRegKey)
				{
					RegistryKey key = Registry.LocalMachine.CreateSubKey(
							Strings.HKLMAndroidConfigRegKeyPath);
					key.SetValue("SystemStats", 1);
					key.Flush();
					key.Close();
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}

			return res;
		}

		public static void SendFrontendStatusUpdate(string evt)
		{
			Logger.Info("SendFrontendStatusUpdate: evt {0}", evt);
			Thread thr = new Thread(delegate ()
			{
				try
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey(
						Common.Strings.HKLMConfigRegKeyPath);

					string agentUrl = string.Format("http://127.0.0.1:{0}", (int)key.GetValue("AgentServerPort", 2861));
					string url = String.Format("{0}/{1}", agentUrl, Common.Strings.FrontendStatusUpdateUrl);

					Dictionary<String, String> data = new Dictionary<String, String>();
					data.Add("event", evt);

					Dictionary<string, string> headers = new Dictionary<string, string>();
					if (!Common.Strings.VMName.Equals("Android"))
					{
						headers.Add("vmid", Common.Strings.VMName.Split(new Char[] { '_' })[1]);
					}
					Logger.Info("Sending FrontendStatusUpdate to {0}", url);
					string res = HTTP.Client.PostWithRetries(url, data, headers, false, 10, 1000, Common.Strings.VMName);
					Logger.Info("Got FrontendStatusUpdate response: {0}", res);
				}
				catch (Exception e)
				{
					Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
				}
			});
			thr.IsBackground = true;
			thr.Start();

			/*
			 * Hack to handle missing frontend-closed events.  Process may
			 * exit before completing HTTP call to Agent.  Make sure to
			 * wait a while so that this does not happen.
			 *
			 * A small wait should be good enough because this connection
			 * is local and there are no latencies other than CPU computation
			 * and process scheduling.
			 */
			if (string.Compare(evt, "frontend-closed", true) == 0)
				thr.Join(200);
		}

		public static void SendTimelineStats(
				long agent_timestamp,
				long sequence,
				string evt,
				long duration,
				string s1,
				string s2,
				string s3,
				string timezone,
				string locale,
				long from_timestamp,
				long to_timestamp,
				long from_ticks,
				long to_ticks
				)
		{
			string url = String.Format("{0}/{1}", Service.Host, Common.Strings.TimelineStatsUrl);

			Dictionary<String, String> data = new Dictionary<String, String>();
			data.Add("agent_timestamp", GetURLSafeBase64String(agent_timestamp.ToString()));
			data.Add("sequence", GetURLSafeBase64String(sequence.ToString()));
			data.Add("event", GetURLSafeBase64String(evt));
			data.Add("duration", GetURLSafeBase64String(duration.ToString()));
			data.Add("s1", GetURLSafeBase64String(s1));
			data.Add("s2", GetURLSafeBase64String(s2));
			data.Add("s3", GetURLSafeBase64String(s3));
			data.Add("timezone", GetURLSafeBase64String(timezone));
			data.Add("locale", GetURLSafeBase64String(locale));
			data.Add("from_timestamp", GetURLSafeBase64String(from_timestamp.ToString()));
			data.Add("to_timestamp", GetURLSafeBase64String(to_timestamp.ToString()));
			data.Add("from_ticks", GetURLSafeBase64String(from_ticks.ToString()));
			data.Add("to_ticks", GetURLSafeBase64String(to_ticks.ToString()));

			try
			{
				/*
				   Logger.Info("Sending TimelineStats to {0}: " +
				   "agent_timestamp {1}, sequence {2}, " +
				   "event {3}, duration {4}, s1 {5}, s2 {6}, s3 {7} " +
				   "from_timestamp {8}, to_timestamp {9}, " +
				   "from_ticks {10}, to_ticks {11}",
				   url, agent_timestamp, sequence,
				   evt, duration, s1, s2, s3,
				   from_timestamp, to_timestamp,
				   from_ticks, to_ticks);
				   */

				string res = HTTP.Client.Post(url, data, null, false);
				//Logger.Info("Got TimelineStats response: sequence {0} {1}", sequence, res);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}

		public static void SendBootStats(
				string type,
				bool booted,
				bool wait
				)
		{
			Thread bootStatsThread = new Thread(delegate ()
			{
				string url = String.Format("{0}/{1}", Service.Host, Common.Strings.BootStatsUrl);

				Dictionary<String, String> data = new Dictionary<String, String>();
				data.Add("type", GetURLSafeBase64String(type));
				data.Add("booted", GetURLSafeBase64String(booted.ToString()));

				try
				{
					Logger.Info("Sending Boot Stats to {0}", url);
					string res = HTTP.Client.Post(url, data, null, false);
					Logger.Info("Got Boot Stats response: {0}", res);
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
				}
			});
			bootStatsThread.IsBackground = true;
			bootStatsThread.Start();

			if (wait)
			{
				if (!bootStatsThread.Join(5000))
					bootStatsThread.Abort();
			}
		}

		public static void SendHomeScreenDisplayedStats()
		{
			Thread homeScreenStatsThread = new Thread(delegate ()
			{
				string url = String.Format("{0}/{1}", Service.Host, Common.Strings.HomeScreenStatsUrl);

				try
				{
					Logger.Info("Sending Home Screen Displayed Stats to {0}", url);
					string res = HTTP.Client.Get(url, null, false);
					Logger.Info("Got Home Screen Displayed Stats response: {0}", res);
				}
				catch (Exception ex)
				{
					Logger.Error(ex.ToString());
				}
			});
			homeScreenStatsThread.IsBackground = true;
			homeScreenStatsThread.Start();
		}

		public static void SendBtvFunnelStats(
				string statEvent,
				string statDataKey,
				string statDataValue,
				bool createNewId
				)
		{
			Thread btvFunnelStatsThread = new Thread(delegate ()
			{
				SendBtvFunnelStatsSync(statEvent, statDataKey, statDataValue, createNewId);
			});
			btvFunnelStatsThread.IsBackground = true;
			btvFunnelStatsThread.Start();
		}

		public static void SendBtvFunnelStatsSync(
				string statEvent,
				string statDataKey,
				string statDataValue,
				bool createNewId
				)
		{
			string url = String.Format("{0}/{1}", Service.Host, Common.Strings.BtvFunnelStatsUrl);

			Dictionary<String, String> data = new Dictionary<String, String>();
			data.Add("session_id", SessionId);
			data.Add("event_type", statEvent);
			if (statDataKey != null)
				data.Add(statDataKey, statDataValue);

			try
			{
				Logger.Info("Sending Btv Funnel Stats to {0}", url);
				string res = HTTP.Client.Post(url, data, null, false);
				Logger.Info("Sent Btv Funnel Stats");
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}

		public static void SendStyleAndThemeInfoStats(string actionName,
				string styleName,
				string themeName,
				string optionalParam)
		{
			SendStyleAndThemeInfoStatsAsync(actionName, styleName, themeName, optionalParam);
		}

		public static void SendStyleAndThemeInfoStatsAsync(
				string actionName,
				string styleName,
				string themeName,
				string optionalParam)
		{
			Thread styleAndThemeStatsThread = new Thread(delegate ()
					{
						SendStyleAndThemeInfoStatsSync(actionName, styleName, themeName, optionalParam);
					});
			styleAndThemeStatsThread.IsBackground = true;
			styleAndThemeStatsThread.Start();
		}

		public static void SendStyleAndThemeInfoStatsSync(
				string actionName,
				string styleName,
				string themeName,
				string optionalParam)
		{
			try
			{
				Logger.Info("Sending Style and Theme Stats");
				Dictionary<String, String> data = CollectStyleAndThemeData(actionName, styleName, themeName, optionalParam);
				foreach (KeyValuePair<string, string> info in data)
				{
					Logger.Info(info.Key + " " + info.Value);
				}
				string url = String.Format("{0}/{1}", Utils.HostUrl, Common.Strings.MiscellaneousStatsUrl);
				SendData(url, data);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}

		}

		public static void SendMiscellaneousStatsSync(
				string tag,
				string arg1,
				string arg2,
				string arg3,
				string arg4,
				string arg5)
		{
			try
			{
				Logger.Info("Sending miscellaneous Stats");
				Dictionary<String, String> data = new Dictionary<string, string>();
				data.Add("tag", tag);
				data.Add("arg1", arg1);
				data.Add("arg2", arg2);
				data.Add("arg3", arg3);
				data.Add("arg4", arg4);
				data.Add("arg5", arg5);
				foreach (KeyValuePair<string, string> info in data)
				{
					Logger.Info(info.Key + " " + info.Value);
				}
				string url = String.Format("{0}/{1}", Utils.HostUrl, Common.Strings.MiscellaneousStatsUrl);
				SendData(url, data);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}

		}

		public static void SendData(string url, Dictionary<string, string> data)
		{
			Logger.Info("Sending stats to " + url);
			try
			{
				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}

			Logger.Info("Sent stats");
		}

		private static Dictionary<string, string> CollectStyleAndThemeData(string actionName, string styleName, string themeName, string optionalParam)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();

			data.Add("tag", Strings.StyleThemeStatsTag);
			data.Add("arg1", User.GUID);
			data.Add("arg2", actionName);
			data.Add("arg3", styleName);
			data.Add("arg4", themeName);
			data.Add("arg5", optionalParam);

			return data;
		}
		private static String Timestamp
		{
			get
			{
				//Find unix timestamp (seconds since 01/01/1970)
				long ticks = DateTime.Now.Ticks - DateTime.Parse("01/01/1970 00:00:00").Ticks;
				ticks /= 10000000; //Convert windows ticks to seconds
				string timestamp = ticks.ToString();
				return timestamp;
			}
		}

		private static String Email
		{
			get
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.CloudRegKeyPath);
				String email = (String)key.GetValue("Email", "");
				key.Close();
				return email;
			}
		}

		private static string GetURLSafeBase64String(string originalString)
		{
			string base64String = System.Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(originalString));
			return base64String;
		}
	}
}


