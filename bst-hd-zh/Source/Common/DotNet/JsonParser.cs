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
using System.Threading;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Common
{
	public class JsonParser
	{
		private static string s_appsDotJsonFile = Path.Combine(Common.Strings.GadgetDir, "apps.json");
		public static AppInfo[] s_originalJson = null;

		public static AppInfo[] GetAppList()
		{
			StreamReader jsonFile = new StreamReader(s_appsDotJsonFile);
			string originalJsonString = jsonFile.ReadToEnd();
			jsonFile.Close();

			JSonReader reader = new JSonReader();
			GetOriginalJson(reader.ReadAsJSonObject(originalJsonString));
			return s_originalJson;
		}

		private static void GetOriginalJson(IJSonObject input)
		{
			s_originalJson = new AppInfo[input.Length];
			for (int i = 0; i < input.Length; i++)
			{
				s_originalJson[i] = new AppInfo(input[i]);
			}
		}

		public static int GetInstalledAppCount()
		{
			GetAppList();

			int count = 0;

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (string.Compare(s_originalJson[i].activity, ".Main", true) == 0)
				{
					// This is an invalid entry added for "AddApp", don't count it as installed app
					continue;
				}

				if (string.Compare(s_originalJson[i].appstore, "yes", true) != 0)
				{
					count++;
				}
			}

			return count;
		}

		// returns true if an entry is found, else returns false
		public static bool GetAppInfoFromAppName(
				string appName,
				out string packageName,
				out string imageName,
				out string activityName
				)
		{
			packageName = null;
			imageName = null;
			activityName = null;

			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].name == appName)
				{
					packageName = s_originalJson[i].package;
					imageName = s_originalJson[i].img;
					activityName = s_originalJson[i].activity;
					return true;
				}
			}

			return false;
		}

		// returns true if an entry is found, else returns false
		public static bool GetAppInfoFromPackageName(
				string packageName,
				out string appName,
				out string imageName,
				out string activityName,
				out string appstore
				)
		{
			appName = "";
			imageName = "";
			activityName = "";
			appstore = "";

			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].package == packageName)
				{
					appName = s_originalJson[i].name;
					imageName = s_originalJson[i].img;
					activityName = s_originalJson[i].activity;
					appstore = s_originalJson[i].appstore;
					return true;
				}
			}

			return false;
		}

		public static String GetAppNameFromPackageActivity(string packageName, string activityName)
		{
			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].package == packageName && s_originalJson[i].activity == activityName)
				{
					return s_originalJson[i].name;
				}
			}

			return String.Empty;
		}

		public static String GetAppNameFromPackage(string packageName)
		{
			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].package == packageName)
				{
					return s_originalJson[i].name;
				}
			}

			return String.Empty;
		}

		public static String GetPackageNameFromActivityName(string activityName)
		{
			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].activity == activityName)
				{
					return s_originalJson[i].package;
				}
			}

			return String.Empty;
		}

		public static String GetActivityNameFromPackageName(string packageName)
		{
			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].package == packageName)
				{
					return s_originalJson[i].activity;
				}
			}

			return String.Empty;
		}

		public static bool IsPackageNameSystemApp(string packageName)
		{
			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].package == packageName)
				{
					if (s_originalJson[i].system == "1")
						return true;
					else
						return false;
				}
			}
			return false;
		}

		public static bool IsAppNameSystemApp(string appName)
		{
			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].name == appName)
				{
					if (s_originalJson[i].system == "1")
						return true;
					else
						return false;
				}
			}
			return false;
		}

		public static bool IsAppInstalled(
				string packageName
				)
		{
			string ver;
			return IsAppInstalled(packageName, out ver);
		}

		public static bool IsAppInstalled(
				string packageName,
				out string version
				)
		{
			GetAppList();

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].package == packageName)
				{
					version = s_originalJson[i].version;
					return true;
				}
			}

			version = "NA";
			return false;
		}

		public static bool GetAppData(string package, string activity, out string name, out string img)
		{
			GetAppList();

			name = "";
			img = "";

			for (int i = 0; i < s_originalJson.Length; i++)
			{
				if (s_originalJson[i].package == package && s_originalJson[i].activity == activity)
				{
					name = s_originalJson[i].name;
					img = s_originalJson[i].img;
					Logger.Info("Got AppName: {0} and AppIcon: {1}", name, img);
					return true;
				}
			}
			return false;
		}

		public static void WriteJson(AppInfo[] json)
		{
			JSonWriter writer = new JSonWriter();

			Logger.Info("JsonParser: Writing json object array to json writer");

			writer.WriteArrayBegin();
			for (int i = 0; i < json.Length; i++)
			{
				writer.WriteObjectBegin();
				writer.WriteMember("img", json[i].img);
				writer.WriteMember("name", json[i].name);
				writer.WriteMember("system", json[i].system);
				writer.WriteMember("package", json[i].package);
				writer.WriteMember("appstore", json[i].appstore);
				writer.WriteMember("activity", json[i].activity);
				writer.WriteMember("version", json[i].version);
				if (json[i].url != null)
					writer.WriteMember("url", json[i].url);
				writer.WriteObjectEnd();
			}
			writer.WriteArrayEnd();

			StreamWriter jsonFile = new StreamWriter(s_appsDotJsonFile + ".tmp");
			jsonFile.Write(writer.ToString());
			jsonFile.Close();
			File.Copy(s_appsDotJsonFile + ".tmp", s_appsDotJsonFile + ".bak", true);
			File.Delete(s_appsDotJsonFile);

			/*
			 * It has been observed that sometimes the apps.json file is not
			 * deleted even after File.Delete command is done with. It is
			 * instead marked for deletion because some other thread is accessing
			 * the file.
			 */
			int retries = 10;
			while (File.Exists(s_appsDotJsonFile) && retries > 0)
			{
				retries--;
				Thread.Sleep(100);
			}

			try
			{
				File.Move(s_appsDotJsonFile + ".tmp", s_appsDotJsonFile);
				return;
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
			}
		}

		public static int AddToJson(AppInfo json)
		{
			GetAppList();

			Logger.Info("Adding to Json");

			AppInfo[] newJson = new AppInfo[s_originalJson.Length + 1];
			int i;
			for (i = 0; i < s_originalJson.Length; i++)
			{
				newJson[i] = s_originalJson[i];
			}
			newJson[i] = json;
			WriteJson(newJson);

			return s_originalJson.Length;
		}
	}

	public class AppInfo
	{
		public string name, img, package, activity, system, url, appstore, version;

		public AppInfo(IJSonObject app)
		{
			name = app["name"].StringValue;
			img = app["img"].StringValue;
			package = app["package"].StringValue;
			activity = app["activity"].StringValue;
			system = app["system"].StringValue;
			try
			{
				url = app["url"].StringValue;
			}
			catch
			{
				url = null;
			}
			try
			{
				appstore = app["appstore"].StringValue;
			}
			catch
			{
				appstore = "Unknown";
			}
			try
			{
				version = app["version"].StringValue;
			}
			catch
			{
				version = "Unknown";
			}
		}

		public AppInfo(string InName, string InImage, string InPackage, string InActivity, string InSystem, string InAppStore, string InVersion)
		{
			name = InName;
			img = InImage;
			package = InPackage;
			activity = InActivity;
			system = InSystem;
			url = null;
			appstore = InAppStore;
			version = InVersion;
		}
	}
}
