using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading;
using System.ServiceProcess;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Agent {

public class ApkInstall {

	// Don't show notifications for these packages
	public static string[] sIgnoreEvents = {
		"com.bluestacks.chartapp"
	};

	public static int InitApkInstall()
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		s_installDir = (string)key.GetValue("InstallDir");

		return 0;
	}
	
	private static bool IsDiskFull(Exception ex)
	{
		//This function will set the IErrorInfo of the current thread which can cause unexpected results for methods like the ThrowExceptionForHR etc. which default to using the IErrorInfo of the current thread if it is set.
		const int ERROR_HANDLE_DISK_FULL = 0x27;
		const int ERROR_DISK_FULL = 0x70;

		int win32ErrorCode = System.Runtime.InteropServices.Marshal.GetHRForException(ex) & 0xFFFF;
		return win32ErrorCode == ERROR_HANDLE_DISK_FULL || win32ErrorCode == ERROR_DISK_FULL;
	}



	public static string InstallApk(string apk, string vmName)
	{
		string newPath = "";
		Logger.Info("apk to be installed is " + apk);		
		try {
			string configPath = Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName);
			RegistryKey key = Registry.LocalMachine.OpenSubKey(configPath);
			int fileSystem = (int)key.GetValue("FileSystem", 0);

			string r = null;
			if (fileSystem == 1)
			{
				try {
					string sharedFolderPath = Common.Strings.RegSharedFolderPath;
					key = Registry.LocalMachine.OpenSubKey(sharedFolderPath);
					string name = (string)key.GetValue("Name");
					string path = (string)key.GetValue("Path");
					if (String.IsNullOrEmpty(name) || String.IsNullOrEmpty(path))
					{
						Logger.Error("Name or Path missing in sharedfolder regkey");
						fileSystem = 0;
					}
					else
					{
						newPath = Path.Combine(path,
								"Bst" + "-" +
								DateTime.Now.Ticks.ToString() + Path.GetExtension(apk));
						Logger.Info("apk = {0}, newPath = {1}", apk, newPath);
						try
						{
							File.Copy(apk, newPath, true);
						}
						catch (Exception e)
						{
							if (IsDiskFull(e) == true)
							{
								Logger.Info("Disk Space is full");
								return "INSTALL_FAILED_INSUFFICIENT_STORAGE_HOST";
							}
							else
							{
								Logger.Error("Unable to copy file" + e.ToString());
								fileSystem = 0;
								throw new Exception("File_Not_Copied");
							}
						}
						File.SetAttributes(newPath, FileAttributes.Normal);

						string androidPath = "/mnt/sdcard/windows/" +
							name + "/" + Path.GetFileName(newPath);
						Logger.Info("androidPath: " + androidPath);

						Dictionary<string, string> data = new Dictionary<string, string>();
						data.Add("path", androidPath);
						string serviceName = Common.Strings.GetAndroidServiceName(vmName);
						ServiceController sc = new ServiceController(serviceName);
						if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending)
						{
							Logger.Info("The Android Service was not running , starting it now");
							sc.Start();
						}

						string url = String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, HDAgent.s_InstallPath);

						if (Common.HTTP.Client.UrlForBstCommandProcessor(url) && !Common.Utils.IsUIProcessAlive())
						{
							Logger.Info("Starting Frontend in hidden mode.");
							Common.Utils.StartHiddenFrontend(vmName);
						}

						Thread t = new Thread(delegate() {
								// wait for frontend to start
								if(!Common.Utils.WaitForFrontendPingResponse(vmName))
								{
									Logger.Info("Frontend not responding to ping response for 50 seconds, aborting installtion with FRONTEND_NOT_STARTING error");
									HTTPHandler.sApkInstallResult = "FRONTEND_NOT_STARTING";
									HTTPHandler.sApkInstallThread.Abort();
									return;
								}
								else
								{
									Logger.Info("Frontend is running gave ping response");
								}

								// check if frontend is running
								while (true)
								{
									if (HTTPHandler.sApkInstallThread == null)
									{
										return;
									}
									Logger.Info("frontend lock name is : " + Common.Strings.GetFrontendLockName(vmName));
									if (Utils.IsUIProcessAlive())
									{
										Thread.Sleep(500);
										continue;
									}

									Logger.Info("Frontend Process Stopped, aborting Installation with FRONTEND_NOT_RUNNING error");
									HTTPHandler.sApkInstallResult = "FRONTEND_NOT_RUNNING";
									HTTPHandler.sApkInstallThread.Abort();
									break;
								}
						});
						t.IsBackground = true;
						t.Start();

						String evtName = String.Format(
								"Global\\BlueStacks_Frontend_Ready_Android");
						EventWaitHandle evt = null;

						Logger.Info("Trying to open event {0}", evtName);

						DateTime startTime = DateTime.Now;
						RegistryKey configRegKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
						bool firstBoot = ((int)configRegKey.GetValue("ConfigSynced", 0)) == 0 ? true : false;
						TimeSpan timeoutPeriod = new TimeSpan(0, 2, 30);

						if (firstBoot == true)
						{
							timeoutPeriod = new TimeSpan(0, 4, 0);
						}

						while (true && DateTime.Compare(startTime + timeoutPeriod, DateTime.Now) >= 0) 
						{
							try {
								evt = EventWaitHandle.OpenExisting(evtName);
							} catch (WaitHandleCannotBeOpenedException exc) {
								string suppressCompilerWarning = exc.Message;
								Thread.Sleep(200);
								continue;
							} 
							break;
						}

						if (DateTime.Compare(startTime + timeoutPeriod, DateTime.Now) < 0)
						{
							Logger.Error("Its a boot complete failure, sending boot failure error");
							return "ANDROID_BOOT_FAILURE";
						}
						Logger.Info("Waiting on event {0}", evtName);
						evt.WaitOne();

						//wait for 30 seconds
						if (Utils.CheckIfGuestReady(30) == false)
						{
							string reason = "";
							if (Utils.IsUIProcessAlive() == false)
							{
								reason = "FRONTEND_NOT_RUNNING";
							}
							if (Utils.IsServiceRunning(Common.Strings.GetAndroidServiceName(vmName)) == false)
							{
								reason = "ANDROID_SERVICE_NOT_RUNNING";
							}
							else
							{
								reason = "GUEST_NOT_READY_YET";
							}
							Logger.Error("Its a CheckIfGuestReady failure. Reason : {0}", reason);
							return reason;
						}

						if(Utils.WaitForSyncConfig(vmName) == false)
						{
							Logger.Info("Config still not sycned");
							return "CONFIG_NOT_SYNCED";
						}
						Logger.Info("HTTPHandler: Sending post request to {0}", url);

						r = Common.HTTP.Client.PostWithoutBootCheck(url, data, null, false, 5 * 60 * 1000, null);

						int retries = 60;
						/*
						 * if INSTALL_FAILED_INVALID_URI error occurs then response comes quickly
						 */
						while (r.Contains("INSTALL_FAILED_INVALID_URI") &&
								retries > 0)
						{
							retries--;
							r = Common.HTTP.Client.Post(url, data, null, false, 3 * 60 * 1000);
							Thread.Sleep(1000);
						}

						if (r.Contains("INSTALL_FAILED_INSUFFICIENT_STORAGE") ||
								r.Contains("INSTALL_FAILED_INVALID_URI"))
							fileSystem = 0;

						if (File.Exists(newPath)) //This file is sometimes already deleted by guest, so the condition
						{
							try
							{
								File.Delete(newPath);
							}
							catch(Exception e)
							{
								Logger.Error("Unable to delete file {0}, Error: {1}", newPath, e.ToString());
							}
						}
					}
				}
				catch (Exception e)
				{
					Logger.Error(e.ToString());
					if (!e.Message.Equals("File_Not_Copied"))
					{
						if (File.Exists(newPath)) //This file is sometimes already deleted by guest, so the condition
						{
							Logger.Info("Deleting copied apk from sharedfolder");
							try
							{
								File.Delete(newPath);
							}
							catch(Exception e2)
							{
								Logger.Error("Unable to delete file {0}, Error: {1}", newPath, e2.ToString());
							}
						}
					}
				}
			}

			try {
				if (fileSystem == 0)
				{
					/* no shared folders. send apk to android */
					Logger.Info("Sending apk");
					r = HTTPHandler.PostFile(Common.VmCmdHandler.s_ServerPort,
							HDAgent.s_InstallPath, apk, vmName);
					if (r == null)
					{
						Logger.Error("No response received yet.");
						return "INSTALL_FAILED_UPLOAD_APK_ERROR";
					}
				}
			} catch (Exception exc) {
				Logger.Error("Exception when sending install post request");
				Logger.Error(exc.ToString());
				return "INSTALL_FAILED_SERVER_ERROR";
			}

			IJSonReader json = new JSonReader();
			IJSonObject res = json.ReadAsJSonObject(r);
			if (res["result"].StringValue == "ok")
				s_returnString = "Success";
			else
				s_returnString = res["reason"].StringValue;

			return s_returnString;
		} catch(Exception e) {
			Logger.Error(e.ToString());
			return "Exception";
		}
	}

	public static void AppInstalled(string name, string package, string activity, string img, string version, string isUpdate, string vmName, string source)
	{
		Logger.Info("Replacing invalid characters, if any, with whitespace");
		s_appName = Regex.Replace(name, @"[\x22\\\/:*?|<>]", " ");

		s_packageName = package;
		s_launchableActivityName = activity;
		s_appIcon = img;
		s_version = version;

		s_originalJson = JsonParser.GetAppList();
		AddToJson();
		if(Common.Features.IsFeatureEnabled(Features.CREATE_LIBRARY))
			MakeLibraryChanges();

		string gadgetIconBaseName = String.Format("{0}.{1}.png", package, activity);
		string gadgetIconPath = Path.Combine(Common.Strings.GadgetDir, gadgetIconBaseName);
		string apktoexeIconBaseName = String.Format("{0}.{1}.png", package, ".Main");
		string apktoexeIconPath = Path.Combine(Common.Strings.GadgetDir, apktoexeIconBaseName);
		if (File.Exists(apktoexeIconPath))
			File.Copy(apktoexeIconPath, gadgetIconPath, true);
		else
		{
			Thread iconDownloader = new Thread(delegate() {
					Utils.DownloadIcon(gadgetIconPath, package);
					});
			iconDownloader.IsBackground = true;
			iconDownloader.Start();
		}

		if(Oem.Instance.IsOemWithGameManagerData)
		{
			Thread imageDownloader = new Thread(delegate() {
					string url = String.Format("http://cdn.bluestacks.com/public/appsettings/app-back-images/{0}.png", package);

					string dir = Common.Strings.GameManagerBannerImageDir;
					if (Directory.Exists(dir) == false)
					{
					Directory.CreateDirectory(dir);
					}

					string backImageName = package + ".png";
					string backImagePath = Path.Combine(dir, backImageName);

					string backImageTempName = backImageName + ".tmp";
					string backImageTempPath = Path.Combine(dir, backImageTempName);

					try
					{
					Logger.Info("Will download {0} to {1}", url, backImageTempPath);
					if (File.Exists(backImageTempPath))
					{
					File.Delete(backImageTempPath);
					}
					WebClient webClient = new WebClient();
					webClient.DownloadFile(url, backImageTempPath);

					if (File.Exists(backImagePath))
					{
						File.Delete(backImagePath);
					}
					File.Move(backImageTempPath, backImagePath);
					}
			catch (Exception e)
			{
				Logger.Error("Error when downloading from " + url);
				Logger.Error(e.ToString());
			}
			});
			imageDownloader.IsBackground = true;
			imageDownloader.Start();
		}
		if (BlueStacks.hyperDroid.Common.Oem.Instance.IsAppInstalledEntryToBeMadeInJson)
		{
			int pos	= Array.IndexOf(sIgnoreEvents, s_packageName);
			if (pos == -1)	// package is not in above list
			{
				AddToGameManagerJson();
			}
		}

		Logger.Info("InstallApk: Got AppName: {0}", ApkInstall.s_appName);

		Logger.Info("Sending App Install stats");
//		string version = HDAgent.GetVersionFromPackage(package);

		Common.Stats.SendAppInstallStats(name, package, version, Common.Stats.AppInstall, isUpdate, source);

		string installMsg = s_appName + " ";
		if (String.Compare(isUpdate, "true", true) == 0)
			installMsg += Locale.Strings.UpdateSuccess;
		else
			installMsg += Locale.Strings.InstallSuccess;
//		installMsg += "\nClick here to launch.";

		string imagePath = Path.Combine(Common.Strings.GadgetDir, s_appIcon);
		//SysTray.ShowInstallAlert(s_appName, imagePath, Locale.Strings.BalloonTitle, installMsg);

		RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
		try
		{
			if (Common.Features.IsFeatureEnabled(Common.Features.INSTALL_NOTIFICATIONS))
			{
				int pos	= Array.IndexOf(sIgnoreEvents, package);
				if (pos == -1)	// package is not in above list
				{
					SysTray.ShowInfoShort(Locale.Strings.BalloonTitle, installMsg);
				}
				else
				{
					Logger.Debug("Not showing notification for: " + package);
				}
			}
			else
			{
				Logger.Info("Not showing install notification...");
			}
		}
		catch (Exception ex)
		{
			key.SetValue("InstallNotificationThreshold", 0);
			Logger.Error("Failed to get ShowInstallerNotification value. err: " + ex.Message);
			// Ignore
		}
	}

	private static void AddToJson()
	{
		Logger.Info("InstallApk: Adding app to json: " + s_appName);
		AppInfo[] newJson = new AppInfo[s_originalJson.Length+1];
		int i;
		int appCount = 1;
		string origName = s_appName;
		for (i=0; i<s_originalJson.Length; i++)
		{
			if (s_originalJson[i].name == s_appName)
			{
				if (s_originalJson[i].package == s_packageName && s_originalJson[i].activity == s_launchableActivityName)
				{
					s_appName = s_originalJson[i].name;
					return;
				}
				s_appName = origName + "-" + appCount;
				appCount++;
				i = 0;
			}
			newJson[i] = s_originalJson[i];
		}

		newJson[i] = new Common.AppInfo(s_appName, s_appIcon, s_packageName, s_launchableActivityName, "0", "no", s_version);

		JsonParser.WriteJson(newJson);
	}

	private static void MakeLibraryChanges()
	{
		Logger.Info("Making Library Changes");
		string appsDir = Path.Combine(Common.Strings.LibraryDir, Common.Strings.MyAppsDir);
		string iconsDir = Path.Combine(Common.Strings.LibraryDir, Common.Strings.IconsDir);
		string png2ico = Path.Combine(s_installDir, "HD-png2ico.exe");
		string iconFile = Path.Combine(s_installDir, "BlueStacks.ico");
		string imagePath = Path.Combine(Common.Strings.GadgetDir, s_appIcon);

		Utils.ResizeImage(imagePath);
		string icon = Utils.ConvertToIco(png2ico, imagePath, iconsDir);

		if (!File.Exists(icon))
			icon = iconFile;

		string arguments = String.Format("-p {0} -a {1} -v {2}", s_packageName, s_launchableActivityName, Common.Strings.VMName);
		CreateAppShortcut(Path.Combine(appsDir, s_appName + ".lnk"), arguments, icon, appsDir, imagePath);
	}

	private static void CreateAppShortcut(string fileName, string arguments, string imglocation, string fileLocation, string imagePath)
	{
		String runAppFile = Path.Combine(s_installDir, "HD-RunApp.exe");
		int res = HDAgent.CreateShortcut(runAppFile, fileName, "", imglocation, arguments, 0);
		if (res != 0)
			Logger.Error("Couldn't create shorcut for " + arguments);
		else
			Logger.Info("Created shorcut {0} at {1}", fileName, fileLocation);
	}

	private static void AddToGameManagerJson()
	{
		GMAppInfo	newAppInfo	= null;

		string imageName = s_packageName + ".png";
		string imagePath = Path.Combine(Common.Strings.GadgetDir, s_appIcon);
		string targetPath = Path.Combine(Common.Strings.GameManagerHomeDir, imageName);
		File.Copy(imagePath, targetPath, true);
		string imgUri = Utils.GetFileURI(targetPath);

		newAppInfo = new GMAppInfo(s_appName, imgUri, s_packageName, s_launchableActivityName, "", "", s_version);

		Logger.Debug("Adding to installedApps json");
		if (newAppInfo != null)
		{
			GMAppsManager iam = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
			iam.AddToJson(newAppInfo);
			iam.UpdateAppTimeStamp(newAppInfo.package);
		}
	}

	private static string	s_installDir		= null;
	private static string	s_appsDotJsonFile	= Path.Combine(Common.Strings.GadgetDir, "apps.json");
	public static AppInfo[]	s_originalJson		= null;

	public static string	s_packageName;
	public static string	s_appName = null;
	public static string	s_appIcon;
	public static string	s_launchableActivityName;
	public static string	s_version;

	private static string	s_returnString = null;
}
}

