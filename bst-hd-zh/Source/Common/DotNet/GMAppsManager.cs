/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Library
 */

using System;
using System.IO;
using CodeTitans.JSon;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Common
{
	public class GMAppsManager
	{
		[DllImport("HD-ShortcutHandler.dll", CharSet = CharSet.Auto)]
		public static extern int VerifyShortcutTargetFile(
					string shortcutPath,
					string expectedTargetFilePath,
					int initializeCom);
		[DllImport("shell32.dll", CharSet = CharSet.Auto)]
		static extern int SHGetFolderPath(IntPtr hwndOwner, int nFolder, IntPtr hToken,
			uint dwFlags, [Out] StringBuilder pszPath);

		String mInstallDir = null;
		String mDataDir = null;

		String mAppsJson = null;

		GMAppInfo[] mAppsList = null;

		static private Object sLock = new Object();

		public static String JSON_TYPE_INSTALLED_APPS = "installedApps.json";
		public static String JSON_TYPE_SUGGESTED_APPS = "suggestedApps.json";
		private const int CSIDL_COMMON_DESKTOPDIRECTORY = 0x0019;
		private const int CSIDL_DESKTOPDIRECTORY = 0x0010;

		public GMAppsManager(String jsonType)
		{
			RegistryKey gameManagerReg = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
			mInstallDir = (string)gameManagerReg.GetValue("InstallDir");
			mDataDir = Path.Combine(mInstallDir, @"UserData\Home");
			mAppsJson = Path.Combine(mDataDir, jsonType);

			lock (sLock)
			{
				PopulateAppList();
			}
		}

		public static string GetFolderPath(int CSIDL)
		{
			StringBuilder sb = new StringBuilder(260);
			SHGetFolderPath(IntPtr.Zero, CSIDL, IntPtr.Zero, 0x0000, sb);
			return sb.ToString();
		}

		private void PopulateAppList()
		{
			if (File.Exists(mAppsJson) == false && File.Exists(mAppsJson + ".bak") == true)
				File.Copy(mAppsJson + ".bak", mAppsJson, true);

			int retries = 20;
			while (!File.Exists(mAppsJson) && retries > 0)
			{
				retries--;
				Thread.Sleep(100);
			}

			StreamReader jsonFile = new StreamReader(mAppsJson);
			string originalJsonString = jsonFile.ReadToEnd();
			jsonFile.Close();

			JSonReader reader = new JSonReader();

			IJSonObject appsJsonObj = reader.ReadAsJSonObject(originalJsonString);

			mAppsList = new GMAppInfo[appsJsonObj.Length];

			for (int i = 0; i < mAppsList.Length; i++)
			{
				mAppsList[i] = new GMAppInfo(appsJsonObj[i]);
			}
		}


		public void DeleteAllAppsShortcutIcons()
		{
			for (int k = 0; k < mAppsList.Length; k++)
			{
				RemoveShortcut(mAppsList[k].name);
			}

		}

		public GMAppInfo[] GetAppInfoList()
		{
			GMAppInfo[] installedAppsList = new GMAppInfo[mAppsList.Length];
			for (int i = 0, j = 0; i < mAppsList.Length; i++)
			{
				if (mAppsList[i].img != null)
				{
					installedAppsList[j] = mAppsList[i];
					j++;
				}
			}

			return installedAppsList;
		}

		private void WriteJson(GMAppInfo[] json)
		{
			lock (sLock)
			{
				JSonWriter writer = new JSonWriter();

				writer.WriteArrayBegin();

				for (int i = 0; i < json.Length; i++)
				{
					writer.WriteObjectBegin();

					writer.WriteMember("title", json[i].name);
					writer.WriteMember("pkgName", json[i].package);
					writer.WriteMember("activity", json[i].activity);
					writer.WriteMember("iconUrl", json[i].img);
					writer.WriteMember("apkUrl", json[i].apkurl);
					writer.WriteMember("helpurl", json[i].helpurl);
					writer.WriteMember("version", json[i].version);
					writer.WriteMember("last_used", json[i].lastUsed);

					writer.WriteObjectEnd();
				}

				writer.WriteArrayEnd();

				StreamWriter jsonFile = new StreamWriter(mAppsJson + ".tmp");
				jsonFile.Write(writer.ToString());
				jsonFile.Close();

				File.Copy(mAppsJson + ".tmp", mAppsJson + ".bak", true);
				File.Delete(mAppsJson);

				/*
				 * It has been observed that sometimes the apps.json file is not
				 * deleted even after File.Delete command is done with. It is
				 * instead marked for deletion because some other thread is accessing
				 * the file.
				 */
				int retries = 10;
				while (File.Exists(mAppsJson) && retries > 0)
				{
					retries--;
					Thread.Sleep(100);
				}

				try
				{
					File.Move(mAppsJson + ".tmp", mAppsJson);
					return;
				}
				catch (Exception e)
				{
					Logger.Error("Error Occurred, Err: {0}", e.ToString());
				}
			}
		}

		public string GetHelpUrl(string package)
		{
			for (int k = 0; k < mAppsList.Length; k++)
			{
				if (String.Compare(mAppsList[k].package, package) == 0)
				{
					return mAppsList[k].helpurl;
				}
			}

			return "";
		}

		// returns true if an entry is found, else returns false
		public bool GetAppInfoFromPackageName(
				string package,
				out string name,
				out string img,
				out string activity,
				out string version
				)
		{
			name = "";
			img = "";
			activity = "";
			version = "";

			for (int k = 0; k < mAppsList.Length; k++)
			{
				if (String.Compare(mAppsList[k].package, package) == 0)
				{
					name = mAppsList[k].name;
					img = mAppsList[k].img;

					activity = mAppsList[k].activity;
					version = mAppsList[k].version;
					return true;
				}
			}

			return false;
		}

		public int AddToJson(GMAppInfo newAppInfo)
		{
			Logger.Info("Adding to Json");

			GMAppInfo[] newJson = new GMAppInfo[mAppsList.Length + 1];
			int i;
			for (i = 0; i < mAppsList.Length; i++)
			{
				newJson[i] = mAppsList[i];
			}

			newJson[i] = newAppInfo;

			WriteJson(newJson);

			return mAppsList.Length;
		}

		public bool IsAppInstalled(String packageName)
		{
			for (int i = 0; i < mAppsList.Length; i++)
			{
				if (String.Compare(mAppsList[i].package, packageName) == 0)
				{
					return true;
				}
			}

			return false;
		}

		public void RemoveFromJson(String packageName)
		{
			Logger.Info("GMAppsManager: Removing app from GameManager json: " + packageName);

			bool found = false;
			int numEntries = 0;
			for (int k = 0; k < mAppsList.Length; k++)
			{
				if (String.Compare(mAppsList[k].package, packageName) == 0)
				{
					found = true;
					numEntries++;
				}
			}


			if (found == false)
			{
				Logger.Info("GMAppsManager: {0} is not in json. Ignoring deletion", packageName);
				return;
			}
			Logger.Info(string.Format("GMAppsManager: There are {0} matching entries", numEntries));

			GMAppInfo[] newJson = new GMAppInfo[mAppsList.Length - numEntries];
			for (int i = 0, j = 0; i < mAppsList.Length; i++)
			{
				if (String.Compare(mAppsList[i].package, packageName) == 0)
				{
					try
					{
						RemoveIcon(mAppsList[i].img);
						RemoveShortcut(mAppsList[i].name);
						Logger.Info("GMAppsManager: Removed {0} from json", packageName);
					}
					catch (Exception e)
					{
						Logger.Error("GMAppsManager: Failed to remove icon/shortcut. Ignoring. Error: "
								+ e.ToString());
					}
					continue;
				}
				else
				{
					Logger.Info(string.Format("GMAppsManager: not removing {0},  as it does not match {1}",
								mAppsList[i].package, packageName));
				}
				newJson[j] = mAppsList[i];
				j++;
			}

			WriteJson(newJson);
		}

		public void UpdateAppTimeStamp(string packageName)
		{
			Logger.Info("Updating app timestamp in json: " + packageName);

			bool found = false;
			int numEntries = 0;
			for (int k = 0; k < mAppsList.Length; k++)
			{
				if (String.Compare(mAppsList[k].package, packageName) == 0)
				{
					found = true;
					numEntries++;
				}
			}


			if (found == false)
			{
				Logger.Info("{0} is not in json. Ignoring updation", packageName);
				return;
			}
			Logger.Info(string.Format("There are {0} matching entries", numEntries));

			GMAppInfo[] newJson = new GMAppInfo[mAppsList.Length];
			for (int i = 0; i < mAppsList.Length; i++)
			{
				newJson[i] = mAppsList[i];
				if (String.Compare(mAppsList[i].package, packageName) == 0)
				{
					newJson[i].UpdateTimeStamp();
				}
			}

			WriteJson(newJson);

		}

		public static void RemoveStartMenuShortCut(string appName)
		{
			Logger.Info("Removing startmenu {0} shortcut", appName);
			string shortcutName = appName + ".lnk";
			string startMenuDir = Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
			string shortcutPath = Path.Combine(startMenuDir, shortcutName);

			try
			{
				Logger.Info("Deleting {0}", shortcutPath);
				if (File.Exists(shortcutPath))
				{
					string gameManagerFile = Utils.GetPartnerExecutablePath();
					int res = VerifyShortcutTargetFile(shortcutPath, gameManagerFile, 0);
					if (res == 0)
					{
						Logger.Info("The shortcut exits and the target file also matches");
						File.Delete(shortcutPath);
					}
					else
					{
						Logger.Info("Not deleting shortCut, the targetfile does not match");
					}
				}
				else
					Logger.Info("{0} does not exist", shortcutPath);
			}
			catch (Exception e)
			{
				Logger.Error("Exception when deleting {0}", shortcutPath);
				Logger.Error(e.Message);
			}


		}

		public static void RemoveShortcut(string appName)
		{
			Logger.Info("Removing {0} shortcut", appName);
			string shortcutName = appName + ".lnk";
			string desktopDir = "";
			string shortcutPath = "";

			try
			{
				Logger.Info("Deleting {0}", shortcutPath);

				if (File.Exists(Path.Combine(GetFolderPath(CSIDL_COMMON_DESKTOPDIRECTORY), shortcutName)) || File.Exists(Path.Combine(GetFolderPath(CSIDL_DESKTOPDIRECTORY), shortcutName)))
				{
					if (File.Exists(Path.Combine(GetFolderPath(CSIDL_COMMON_DESKTOPDIRECTORY), shortcutName)))
					{
						desktopDir = GetFolderPath(CSIDL_COMMON_DESKTOPDIRECTORY);
						shortcutPath = Path.Combine(desktopDir, shortcutName);
					}
					else
					{
						desktopDir = GetFolderPath(CSIDL_DESKTOPDIRECTORY);
						shortcutPath = Path.Combine(desktopDir, shortcutName);
					}

					string gameManagerFile = Utils.GetPartnerExecutablePath();

					int res = VerifyShortcutTargetFile(shortcutPath, gameManagerFile, 0);
					if (res == 0)
					{
						Logger.Info("The shortcut exits and the target file also matches");
						File.Delete(shortcutPath);
					}
					else
					{
						Logger.Info("Not deleting shortCut, the targetfile does not match");
					}
				}
				else
					Logger.Info("{0} does not exist", shortcutPath);
			}
			catch (Exception e)
			{
				Logger.Error("Exception when deleting {0}", shortcutPath);
				Logger.Error(e.Message);
			}
		}

		private static void RemoveIcon(string imageFile)
		{
			Logger.Info("Removing icon " + imageFile);
			string imageFilePath = Path.Combine(Common.Strings.GameManagerHomeDir, imageFile);
			if (File.Exists(imageFilePath))
				File.Delete(imageFilePath);
			else
				Logger.Info("{0} does not exist", imageFilePath);
		}
	}

	public class GMAppInfo
	{
		public string name, img, package, activity, apkurl, helpurl, version, lastUsed;

		public GMAppInfo(IJSonObject app)
		{
			name = app["title"].StringValue;
			package = app["pkgName"].StringValue;
			activity = app["activity"].StringValue;
			if (app.Contains("iconUrl") && !app["iconUrl"].IsNull)
				img = app["iconUrl"].StringValue;
			else
				img = "BlueStacks.png";
			apkurl = app["apkUrl"].StringValue;
			helpurl = app["helpurl"].StringValue;

			/*
			 * This try-catch block needed for backward compatibility.
			 * App json objects in newer versions will always have this field
			 */
			try
			{
				version = app["version"].StringValue;
			}
			catch
			{
				version = "";
			}

			try
			{
				lastUsed = app["last_used"].StringValue;
			}
			catch
			{
			}
		}

		public GMAppInfo(string InName, string InImage, string InPackage, string InActivity, string InApkUrl, string InHelpUrl, string InVersion)
		{
			name = InName;
			img = InImage;
			package = InPackage;
			activity = InActivity;
			apkurl = InApkUrl;
			helpurl = InHelpUrl;
			version = InVersion;
			lastUsed = DateTime.Now.ToString();
		}


		public GMAppInfo(string InName, string InImage, string InPackage, string InActivity, string InApkUrl, string InHelpUrl, string usageTime, string InVersion)
		{
			name = InName;
			img = InImage;
			package = InPackage;
			activity = InActivity;
			apkurl = InApkUrl;
			helpurl = InHelpUrl;
			version = InVersion;
			lastUsed = usageTime;
		}

		public void UpdateTimeStamp()
		{
			lastUsed = DateTime.Now.ToString();
		}
	}
}
