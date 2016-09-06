/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Console Frontend
 */

using System;
using System.Net;
using System.Text;
using System.Threading;
using System.Net.Sockets;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using System.Collections.Generic;
using CodeTitans.JSon;

using BlueStacks.hyperDroid.Cloud.Services;

namespace BlueStacks.hyperDroid.Common
{
	public class VmCmdHandler
	{
		static string s_Received = null;
		public static string s_AgentServerPortPath = "setwindowsagentaddr";
		public static string s_FrontendServerPortPath = "setwindowsfrontendaddr";
		public static string s_GameManagerServerPortPath = "setgamemanageraddr";
		public static string s_PingPath = "ping";
		public static string s_CheckGuestReadyPath = "checkifguestready";
		static public ushort s_ServerPort
		{
			get
			{
				return (ushort)Utils.GetBstCommandProcessorPort(Common.Strings.VMName);
			}
		}

		static public void SyncConfig(string keyMapParserVersion)
		{
			long utcMilliseconds = (DateTime.UtcNow.Ticks - 621355968000000000) / TimeSpan.TicksPerMillisecond;
			String cmd = String.Format("settime {0}", utcMilliseconds);
			RunCommand(cmd);

			String standardName = TimeZone.CurrentTimeZone.StandardName;
			TimeSpan utcOffsetTimeSpan = TimeZone.CurrentTimeZone.GetUtcOffset(DateTime.Now);
			String utcOffset = utcOffsetTimeSpan.ToString();
			if (utcOffset[0] != '-')
				utcOffset = String.Format("GMT+{0}", utcOffset);
			else
				utcOffset = String.Format("GMT{0}", utcOffset);

			String isDaylightSavingTime = TimeZone.CurrentTimeZone.IsDaylightSavingTime(DateTime.Now).ToString();
			String daylightBias = Device.Profile.GetSysInfo("Select DaylightBias from Win32_TimeZone");
			String baseUtcOffset;
			if (isDaylightSavingTime == "True" && daylightBias != "")
			{
				baseUtcOffset = utcOffsetTimeSpan.Add(new TimeSpan(0, (Convert.ToInt32(daylightBias)), 0)).ToString();
				if (baseUtcOffset[0] != '-')
					baseUtcOffset = String.Format("GMT+{0}", baseUtcOffset);
				else
					baseUtcOffset = String.Format("GMT{0}", baseUtcOffset);
			}
			else
			{
				baseUtcOffset = utcOffset;
			}

			if (Features.IsFeatureEnabled(Features.SET_CHINA_LOCALE_AND_TIMEZONE))
			{
				baseUtcOffset = "GMT+08:00:00";
				isDaylightSavingTime = "False";
				daylightBias = "-60";
				utcOffset = "GMT+08:00:00";
				standardName = "中国标准时间";
			}

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("baseUtcOffset", baseUtcOffset);
			data.Add("isDaylightSavingTime", isDaylightSavingTime);
			data.Add("daylightBias", daylightBias);
			data.Add("utcOffset", utcOffset);
			data.Add("standardName", standardName);

			string path = "settz";

			SendRequest(path, data);

			string locale = CultureInfo.CurrentCulture.Name.ToLower();

			if (Features.IsFeatureEnabled(Features.SET_CHINA_LOCALE_AND_TIMEZONE))
			{
				locale = "zh-CN";
			}

			cmd = String.Format("setlocale {0}", locale);
			if (RunCommand(cmd) == null)
			{
				Logger.Error("Set locale did not work, will try again on frontend restart");
				return;
			}

			cmd = String.Format("setkeymappingparserversion {0}", keyMapParserVersion);
			if (RunCommand(cmd) == null)
			{
				Logger.Error("setkeymappingparserversion did not work, will try again on frontend restart");
				return;
			}

			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			key.SetValue("ConfigSynced", 1);
			key.Flush();
			key.Close();
		}

		static public void SetMachineType(bool isDesktop)
		{
			string cmd;
			if (isDesktop)
				cmd = String.Format("isdesktop");
			else
				cmd = String.Format("istablet");
			RunCommand(cmd);

		}

		static public void SetKeyboard(bool isDesktop)
		{
			string cmd;
			if (isDesktop)
				cmd = String.Format("usehardkeyboard");
			else
				cmd = String.Format("usesoftkeyboard");
			RunCommand(cmd);
		}

		static public string FqdnSend(int port, string serverIn)
		{
			try
			{
				string destinationPath;
				if (String.Compare(serverIn, "agent", true) == 0)
				{
					destinationPath = s_AgentServerPortPath;
				}
				else if (String.Compare(serverIn, "frontend", true) == 0)
				{
					destinationPath = s_FrontendServerPortPath;
				}
				else if (String.Compare(serverIn, "gamemanager", true) == 0)
				{
					destinationPath = s_GameManagerServerPortPath;
				}
				else
				{
					Logger.Error("Unknown server: " + serverIn);
					return null;
				}

				if (port == 0)
				{
					RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
					if (String.Compare(serverIn, "agent", true) == 0)
						port = (int)configKey.GetValue("AgentServerPort", 2861);
					else if (String.Compare(serverIn, "frontend", true) == 0)
						port = (int)key.GetValue("FrontendServerPort", 2862);
					else if (String.Compare(serverIn, "gamemanager", true) == 0)
					{
						key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
						port = Common.Utils.GetPartnerServerPort();
					}
				}

				Dictionary<string, string> data = new Dictionary<string, string>();

				string addr = "10.0.2.2:" + port.ToString();
				string cmd = String.Format("{0} {1}", destinationPath, addr);
				return RunCommand(cmd);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception when sending fqdn post request: " + exc.Message);
				return null;
			}
		}

		static public string RunCommand(String cmd)
		{
			int pos = cmd.IndexOf(' ');
			string path, arg;
			if (pos == -1)
			{
				path = cmd;
				arg = "";
			}
			else
			{
				path = cmd.Substring(0, pos);
				arg = cmd.Substring(pos + 1);
			}

			Logger.Info("Will send command: {0} to {1}", arg, path);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("arg", arg);

			return SendRequest(path, data);
		}

		static public string SendRequest(String path, Dictionary<string, string> data)
		{
			TimeSpan waitTime = new TimeSpan(0, 0, 1);

			int retries = 60;
			int printErrorLog = 3;  // print 3 times and then shut up
			while (retries > 0)
			{
				try
				{
					string url = String.Format("http://127.0.0.1:{0}/{1}", s_ServerPort, path);

					if (printErrorLog != 0)
					{
						printErrorLog--;
						Logger.Info("Sending request to " + url);
					}

					/*
					 * force a timeout of 3 second when posting to runex
					 * and a timeout of 1 second when posting to setwindowsagentaddr
					 */
					string r;
					if (path == "runex" || path == "run" || path == "powerrun")
						r = Common.HTTP.Client.Post(url, data, null, false, 3000);
					else if (path == s_AgentServerPortPath)
						r = Common.HTTP.Client.Post(url, data, null, false, 1000);
					else
						r = Common.HTTP.Client.Post(url, data, null, false);

					Logger.Info("Got response for {0}: {1}", path, r);

					IJSonReader json = new JSonReader();
					IJSonObject res = json.ReadAsJSonObject(r);
					s_Received = res["result"].StringValue;
					if (s_Received != "ok" && s_Received != "error")
						s_Received = null;
				}
				catch (Exception e)
				{
					if (printErrorLog != 0)
						Logger.Error("Exception in SendRequest for {0}: {1}", path, e.Message);
					s_Received = null;
				}

				if (s_Received != null)
					return s_Received;
				else
					Thread.Sleep(waitTime);

				retries--;
			}
			return null;
		}

		static public void RunCommandAsync(String cmd,
			UIHelper.Action continuation, Control control)
		{
			TimerCallback callback = new TimerCallback(
				delegate (Object obj)
				{

					RunCommand(cmd);

					if (continuation != null)
					{
						UIHelper.RunOnUIThread(control, continuation);
					}
				});

			new System.Threading.Timer(callback, null, 0,
				Timeout.Infinite);

		}
	}
}
