/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * This file implements interfaces exported for showing BlueStacks
 * announcements.
 */

using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Drawing.Imaging;
using System.Windows.Forms;
using System.Diagnostics;
using System.Globalization;
using System.Net;
using Microsoft.Win32;
using System.IO.Ports;
using System.Collections.Generic;

using CodeTitans.JSon;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Agent
{
	class AnnouncementMessage
	{
		private Image	m_Image;
		private string	m_Title;
		private string	m_Msg;
		private string	m_Action;
		private string	m_PkgName;
		private string	m_ActionURL;
		private string	m_FileName;

		public Image Image { get { return m_Image; }}

		public string Title { get { return m_Title; }}

		public string Msg { get { return m_Msg; }}

		public string Action { get { return m_Action; }}

		public string PkgName { get { return m_PkgName; }}

		public string ActionURL { get { return m_ActionURL; }}

		public string FileName { get { return m_FileName; } set { m_FileName = value; }}

		public AnnouncementMessage(Image image,
				string title,
				string msg,
				string action,
				string pkgName,
				string actionURL,
				string fileName)
		{
			m_Image		= image;
			m_Title		= title;
			m_Msg		= msg;
			m_Action	= action;
			m_PkgName	= pkgName;
			m_ActionURL	= actionURL;
			m_FileName	= fileName;
		}

		public AnnouncementMessage(string title,
				string msg,
				string action,
				string pkgName,
				string actionURL,
				string fileName)
			:this(CloudAnnouncement.ProductLogo,
					title,
					msg,
					action,
					pkgName,
					actionURL,
					fileName)
		{

		}

		public AnnouncementMessage(Image image, IJSonObject o)
			:this(image,
					o["title"].StringValue.Trim(),
					o["msg"].StringValue.Trim(),
					o["action"].StringValue.Trim(),
					o["pkgName"].StringValue.Trim(),
					o["actionUrl"].StringValue.Trim(),
					o["fileName"].StringValue.Trim())
		{

		}
	}

	class CloudAnnouncement
	{
		private static string		s_announcementDir	= Path.Combine(Common.Strings.BstUserDataDir, "Announcements");
		private static string		s_appsDir		= Path.Combine(Common.Strings.LibraryDir, Common.Strings.MyAppsDir);

		private static string		s_productLogo		= Path.Combine(HDAgent.s_InstallDir, "ProductLogo.png");

		private static int		s_msgId			= -1;
		private static bool		s_uploadStats		= true;

		private static string		s_configPath		= Common.Strings.HKLMAndroidConfigRegKeyPath;
		private static string		s_hostKeyPath		= Common.Strings.CloudRegKeyPath;

		public static string Dir { get { return s_announcementDir; }}

		public static Image ProductLogo { get { return Image.FromFile(s_productLogo); }}

		private static string configString = 
			@"<?xml version=""1.0"" encoding=""utf-8"" ?>
				<configuration>
					<startup>
						<supportedRuntime version=""v4.0"" sku="".NETFramework,Version=v4.0""/>
						<supportedRuntime version=""v2.0.50727"" />
					</startup>
					<system.net>
						<defaultProxy useDefaultCredentials=""true""/>
					</system.net>
				</configuration>";

		public static bool ShowAnnouncement ()
		{
			if (Common.Features.IsFeatureEnabled(Common.Features.BROADCAST_MESSAGES) == false)
			{
				Logger.Debug("Broadcast message feature disabled. Ignoring...");
				return false;
			}

			int		lastAnnouncementId	= -1;
			string		resp			= null;
			string		reason			= "";
			string		result			= "false";
			RegistryKey	key			= Registry.LocalMachine.CreateSubKey(s_configPath);
			RegistryKey	hostKey			= Registry.LocalMachine.OpenSubKey(s_hostKeyPath);

			Logger.Info("Checking for announcement");

			try
			{
				if (Directory.Exists(s_announcementDir))
				{
					String[] files = Directory.GetFiles(s_announcementDir);
					for (int i = 0; i < files.Length; i++)
					{
						try
						{
							if (File.Exists(files[i]))
							{
								File.Delete(files[i]);
							}
						}
						catch (Exception e)
						{
							Logger.Error("Failed to delete file. err: " + e.Message);
						}
					}
				}
				else
				{
					Directory.CreateDirectory(s_announcementDir);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to delete/create announcement dir. err: " + ex.Message);
				// In case of any exception above, just create the dir and continue.
				if (!Directory.Exists(s_announcementDir))
					Directory.CreateDirectory(s_announcementDir);
			}

			try
			{
				lastAnnouncementId = (int)key.GetValue("LastAnnouncementId");
			}
			catch
			{
//				Logger.Error("Failed to get AnnouncementId. Error: " + ex.ToString() + " Showing welcome message.");
				Logger.Info("LastAnnouncementId not available. Using -1");
				lastAnnouncementId = -1;
			}

			string hostURL = (string)hostKey.GetValue("Host");

			string url = String.Format("{0}/getAnnouncement", hostURL);

			Dictionary<String, String> reqHeaders = new Dictionary<String, String>();
			reqHeaders.Add("x_last_msg_id", Convert.ToString(lastAnnouncementId));
			reqHeaders.Add("x_locale", CultureInfo.CurrentCulture.Name.ToLower());

			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String installType = (String)prodKey.GetValue("InstallType", "");
			reqHeaders.Add("x_install_type", installType);

			resp = Common.HTTP.Client.Get(url, reqHeaders, false);
			if (resp == null)
			{
				Logger.Error("Failed to get announcement data.");
				return false;
			}

			Logger.Info("Announcement get resp: " + resp);

			string jsonString = resp;
			JSonReader readjson = new JSonReader();
			IJSonObject jsonResp = readjson.ReadAsJSonObject(jsonString);

			result			= jsonResp["success"].StringValue.Trim();
			reason			= jsonResp["reason"].StringValue.Trim();

			if (String.Compare(result, "false", true) == 0)
			{
				Logger.Info("Could not get announcement msg: " + reason);
				return false;
			}

			lastAnnouncementId	= Convert.ToInt32(jsonResp["msgId"].StringValue.Trim());
			Logger.Info("Last Announcement ID: " + lastAnnouncementId);

			s_msgId			= lastAnnouncementId;

			string imageURL = jsonResp["imageUrl"].StringValue.Trim();

			Image image = DownloadDisplayImage(imageURL);	// This is blocking

			AnnouncementMessage msg = new AnnouncementMessage(image, jsonResp);

			// If no file name is specified, treat this as an executable file
			if (msg.FileName.Length < 3)
				msg.FileName = "downloadedFile.exe";

			try
			{
				s_uploadStats = true;
				ShowFetchedMsg(msg);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to fetch announcement message. error: " + ex.ToString());
				return false;
			}

			Logger.Info("Updating announcement ID to: " + lastAnnouncementId);
			key.SetValue("LastAnnouncementId", lastAnnouncementId, RegistryValueKind.DWord);

			return true;
		}

		private static void InstallApp (
				string		appURL,
				string		pkgName,
				string		storeType
				)
		{
			string			url		= String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort,
					Common.Strings.AppInstallUrl);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("url", appURL);
			data.Add("package", pkgName);
			data.Add("storeType", storeType);

			Thread installThread = new Thread(delegate() {
					try
					{
						string prog = Path.Combine(HDAgent.s_InstallDir, "HD-RunApp.exe");
						Process.Start(prog);

						Logger.Info("sending app click resp for: {0} to url: {1}", pkgName, url);
						Common.HTTP.Client.Post(url, data, null, false);
					}
					catch (Exception ex)
					{
					Logger.Error("Exception installing store app: " + ex.ToString());
					}
					});
			installThread.IsBackground = true;
			installThread.Start();
		}

		public static void ShowNotification (
				string action,
				string title,
				string message,
				string actionURL,
				string fileName,
				string imageURL
				)
		{
			Image image;

			if (imageURL != null)
				image = DownloadDisplayImage(imageURL);
			else
				image = CloudAnnouncement.ProductLogo;

			AnnouncementMessage msg = new AnnouncementMessage(image, title, message, action, "", actionURL, fileName);

			s_uploadStats	= false;

			ShowFetchedMsg(msg);
		}

		private static void ShowFetchedMsg (AnnouncementMessage m)
		{
			Logger.Info("ShowFetchedMsg called for: " + m.Action);
			switch (m.Action)
			{
				case "None":
					CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false, null);
					break;

				case "Amazon App":
					CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false,
							delegate(Object o, EventArgs e)
							{
							InstallApp(m.ActionURL, m.PkgName, "amz");
							UpdateClickStats();
							});
					break;

				case "Opera App":
					CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false,
							delegate(Object o, EventArgs e)
							{
							InstallApp(m.ActionURL, m.PkgName, "opera");
							UpdateClickStats();
							});
					break;

				case "Web URL GM":
					String	tmpDir = Environment.GetEnvironmentVariable("TEMP");
					String imagePath = Path.Combine(tmpDir, Path.GetRandomFileName());
					if (m.Image != null)
						m.Image.Save(imagePath);
					CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false,
							delegate(Object o, EventArgs e)
							{
							Logger.Info("Announcement msg clicked. Opening tab: " + m.ActionURL);
							Dictionary<string, string> data = new Dictionary<string, string>();
							if (File.Exists(imagePath))
							{
							data.Add("image", imagePath);
							}
							data.Add("url", m.ActionURL);
							data.Add("name", m.Title);
							Logger.Info("Will open tab for url: " + m.ActionURL);
							string url = String.Format(
								"http://127.0.0.1:{0}/{1}",
								Common.Utils.GetPartnerServerPort(),
								Common.Strings.GMLaunchWebTab
								);

							if (Oem.Instance.IsWebTabPushNotificationEnabled == true)
							Common.HTTP.Client.Post(url, data, null, false);
							});
					break;

				case "Web URL":
					CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false,
							delegate(Object o, EventArgs e)
							{
							Logger.Info("Announcement msg clicked. Opening url: " + m.ActionURL);
							Process.Start(m.ActionURL);
							UpdateClickStats();
							});
					break;

				case "Download and Execute":
					CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false,
							delegate(Object o, EventArgs e)
							{
							UpdateClickStats();

							Thread downloadThread = new Thread(delegate() {
								Random	random = new Random();
								m.FileName += " "; // append an extra ' ', in case of no args, it will handle it

								string fileName = m.FileName.Substring(0, m.FileName.IndexOf(' '));
								string args = m.FileName.Substring(m.FileName.IndexOf(' ') + 1);

								fileName = String.Format("{0}_{1}", random.Next(), fileName); // random name
								fileName = Path.Combine(s_announcementDir, fileName);

								try
								{
								WebClient client = new WebClient();
								client.DownloadFile(m.ActionURL, fileName);

								Thread.Sleep(2000);

								Process proc = new Process();
								proc.StartInfo.UseShellExecute = true;
								proc.StartInfo.CreateNoWindow = true;

								if (fileName.ToLowerInvariant().EndsWith(".msi") ||
										fileName.ToLowerInvariant().EndsWith(".exe"))
								{
									if (!Common.Utils.IsSignedByBlueStacks(fileName))
									{
										Logger.Info("Not executing unsigned binary " + fileName);
										return;
									}
								}

								if (fileName.ToLowerInvariant().EndsWith(".msi"))
								{
									proc.StartInfo.FileName = "msiexec";
									args = String.Format("/i {0} {1}", fileName, args);
									proc.StartInfo.Arguments = args;
								}
								else
								{
									proc.StartInfo.FileName = fileName;
									proc.StartInfo.Arguments = args;
								}

								Logger.Info("Starting process: {0} {1}", proc.StartInfo.FileName, args);
								proc.Start();
								}
								catch (Exception ex)
								{
								Logger.Error("Failed to download and execute. err: " + ex.ToString());
								}
								});

							downloadThread.IsBackground = true;
							downloadThread.Start();
							});
					break;

					// Since no field for specifying 'activity name' exists in previous verions, using 'Filename' field
					// to fetch both package and activity name in a single field
				case "Start Android App":
					CustomAlert.ShowCloudAnnouncement(m.Image, m.Title, m.Msg, false,
							delegate(Object o, EventArgs e)
							{
							UpdateClickStats();
							try
							{
							String prog = HDAgent.s_InstallDir + @"\HD-RunApp.exe";

							String[] pkgname_activityname = m.FileName.Split(' ');
							Logger.Info("Broadcast: Starting RunApp: {0} with args: -p {1} -a {2} -nl", prog, pkgname_activityname[0], pkgname_activityname[1]);

							Process proc = Process.Start(prog, String.Format("-p {0} -a {1} -nl", pkgname_activityname[0], pkgname_activityname[1]));
							}
							catch (Exception ex)
							{
							Logger.Error("Failed to start android app: {0}. Error: {1}", m.FileName, ex.ToString());
							}
							});
					break;

				case "Silent Install":
					Logger.Info("Got update request. Initializing silent install...");
					Thread updateThread = new Thread(delegate() {
							Random	random = new Random();
							m.FileName += " "; // append an extra ' ', in case of no args, it will handle it

							string fileName = m.FileName.Substring(0, m.FileName.IndexOf(' '));
							string args = m.FileName.Substring(m.FileName.IndexOf(' ') + 1);

							fileName = String.Format("{0}_{1}", random.Next(), fileName); // random name
							fileName = Path.Combine(s_announcementDir, fileName);

							try
							{
							WebClient client = new WebClient();
							client.DownloadFile(m.ActionURL, fileName);

							Thread.Sleep(2000);

							Process proc = new Process();
							proc.StartInfo.UseShellExecute = true;
							proc.StartInfo.CreateNoWindow = true;

							if (fileName.ToLowerInvariant().EndsWith(".msi") ||
									fileName.ToLowerInvariant().EndsWith(".exe"))
							{
								if (!Common.Utils.IsSignedByBlueStacks(fileName))
								{
									Logger.Info("Not executing unsigned binary " + fileName);
									return;
								}
							}

							if (fileName.ToLowerInvariant().EndsWith(".msi"))
							{
								proc.StartInfo.FileName = "msiexec";
								args = String.Format("/i {0} {1}", fileName, args);
								proc.StartInfo.Arguments = args;
							}
							else
							{
								Logger.Info("Creating file: " + fileName + ".config");
								try
								{
									System.IO.File.WriteAllText(fileName + ".config", configString);
								}
								catch (Exception ex)
								{
									Logger.Error("Exception in create config file: " + ex.ToString());
								}
								proc.StartInfo.FileName = fileName;
								proc.StartInfo.Arguments = args;
							}

							Logger.Info("Starting process: {0} {1}", proc.StartInfo.FileName, args);
							proc.Start();
							}
					catch (Exception ex)
					{
						Logger.Error("Silent install failed.");
						Logger.Error("Failed to download and execute. err: " + ex.ToString());
					}
					});

					updateThread.IsBackground = true;
					updateThread.Start();

					break;

				case "Free App":
					Logger.Info("Free App notification recvd. Starting tray animation.");
					try
					{
						SysTray.StartTrayAnimation(m.Title, m.Msg);
//						SysTray.ShowInfoLong(m.Title, m.Msg);
					}
					catch (Exception ex)
					{
						Logger.Error("Exception in 'Free App' notification: " + ex.ToString());
						// Ignore
					}

					break;

				case "Silent LogCollect":
					Logger.Info("Starting Silent LogCollection");
					Thread silentLogCollectorThread = new Thread(delegate() {
						try
						{
							string prog = Path.Combine(HDAgent.s_InstallDir, "HD-logCollector.exe");
							Process.Start(prog, "-silent");
						}
						catch (Exception ex)
						{
							Logger.Error("Exception in starting HD-logCollector.exe: " + ex.ToString());
						}
					});
					silentLogCollectorThread.IsBackground = true;
					silentLogCollectorThread.Start();

					break;

				default:
					Logger.Error("Announcement: Invalid msg type rcvd: " + m.Action);
					break;
			}
		}

		private static Image DownloadDisplayImage (string imageURL)
		{
			WebClient	client	= new WebClient();
			Stream		stream	= client.OpenRead(imageURL);
			Bitmap		bitmap	= new Bitmap(stream);

			stream.Flush();
			stream.Close();
			return bitmap;
		}

		public static void UpdateClickStats ()
		{
			if (!s_uploadStats)
				return;

			Thread statsThread = new Thread(delegate() {
					try
					{
					string			resp		= null;

					RegistryKey		hostKey		= Registry.LocalMachine.OpenSubKey(s_hostKeyPath);

					string hostURL = (string)hostKey.GetValue("Host");

					string url = String.Format("{0}/updateAnnouncementStats", hostURL);

					Dictionary<String, String> reqHeaders = new Dictionary<String, String>();
					reqHeaders.Add("x_last_msg_id", Convert.ToString(s_msgId));

					resp = Common.HTTP.Client.Get(url, reqHeaders, false);
					if (resp == null)
					{
					Logger.Info("Could not send click stats.");
					}
					}
					catch (Exception ex)
					{
						Logger.Error("Failed to send click stats: " + ex.ToString());
					}
			});

			statsThread.IsBackground = true;
			statsThread.Start();
		}
	}
}

