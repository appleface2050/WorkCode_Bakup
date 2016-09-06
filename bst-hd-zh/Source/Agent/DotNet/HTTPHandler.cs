using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Win32;
using System.Windows.Forms;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Agent {

public class AndroidNotification {
	private string mPackageName;
	private string mAppName;
	private string mMessage;
	private bool mNotificationSent;
	private DateTime mNotificationTime;
	public AndroidNotification(string pkg, string name, string msg)
	{
		mPackageName = pkg;
		mAppName = name;
		mMessage = msg;
		mNotificationSent = false;
		mNotificationTime = DateTime.Now;
	}
	public bool NotificationSent {
		get { return mNotificationSent; }
		set { mNotificationSent = value; }
	}
	public bool OldNotificationFlag {
		get {
		  TimeSpan spentTimeSpan = DateTime.Now.Subtract(mNotificationTime);	
		  TimeSpan thresholdTimeSpan = new TimeSpan(0, 0, 5);
		  if (TimeSpan.Compare(spentTimeSpan, thresholdTimeSpan) > -1)
			  return true;

		  return false;
		}
	}
	public string Package {
		get { return mPackageName; }
	}
	public string AppName {
		get { return mAppName; }
	}
	public string Message {
		get { return mMessage; }
	}
}

public class HTTPHandler {

	//private static string s_FileSharerPackageName = "com.bluestacks.windowsfilemanager";
	private static LinkedList<AndroidNotification> s_PendingNotifications = new LinkedList<AndroidNotification>();
	private static System.Threading.Timer s_NotificationTimer = null;
	private static int s_NotificationTimeout = 20 * 1000;
	private static bool[] s_NotificationLockHelper = new bool[2] {false, false}; //For implementing Peterson's algo to handle notifications
	private static int s_LockForTurn = 0; //For implementing Peterson's algo to handle notifications
	private static Object s_NotificationLock = new Object();

	public static Thread sApkInstallThread = null;
	public static bool sAppUninstallationInProgress = false;
	public static string sApkInstallResult = "";
	private static Mutex sAppCrashInfoWriteLock;

	private static string sCurrentAppPackageFromRunApp = "";

	public static string Get(int port, string path, string vmName)
	{
		Logger.Info("HTTPHandler: Sending get request to http://127.0.0.1:{0}/{1}", port, path);
		string url = String.Format("http://127.0.0.1:{0}/{1}", port, path);

		string res = null;
		if (port == Common.VmCmdHandler.s_ServerPort)
		{
			bool serviceAlreadyRunning = Common.Utils.StartServiceIfNeeded(vmName);
			if (serviceAlreadyRunning == false)
				Utils.WaitForBootComplete();

			int retries = 30;
			while (retries > 0)
			{
				try {
					res = Common.HTTP.Client.Get(url, null, false);
					break;
				} catch (Exception e) {
					Logger.Error("Exception in get request");
					Logger.Error(e.Message);
				}
				retries--;
				Thread.Sleep(2000);
			}
		}
		else
			res = Common.HTTP.Client.Get(url, null, false);

		Logger.Debug("HTTPHandler: Got response: " + res);
		return res;
	}

	public static string Post(int port, string path, Dictionary<string, string> data, string vmName)
	{
		Logger.Info("HTTPHandler: Sending post request to http://127.0.0.1:{0}/{1}", port, path);
		string url = String.Format("http://127.0.0.1:{0}/{1}", port, path);

		string res = null;
		if (port == Common.VmCmdHandler.s_ServerPort)
		{
			bool serviceAlreadyRunning = Common.Utils.StartServiceIfNeeded(vmName);
			if (serviceAlreadyRunning == false)
				Utils.WaitForBootComplete();

			if (Common.HTTP.Client.UrlForBstCommandProcessor(url) && !Common.Utils.IsUIProcessAlive() && !Utils.IsGlHotAttach(vmName))
			{
				Logger.Info("Starting Frontend in hidden mode.");
				Common.Utils.StartHiddenFrontend(vmName);
			}

			int retries = 30;
			while (retries > 0)
			{
				try {
					res = Common.HTTP.Client.Post(url, data, null, false);
					/*
					 * There is a bug in package manager due to which installation fails
					 * intermittently when started just after starting service.
					 * This happens because the shared folder path being posted is not immediately
					 * available to Android during startup
					 * So we retry installing apk
					 */
					if (res.Contains("INSTALL_FAILED_INSUFFICIENT_STORAGE") && !serviceAlreadyRunning)
						Logger.Info("Got response: {0}", res);
					else
						break;
				} catch (Exception e) {
					Logger.Error("Exception in post request");
					Logger.Error(e.Message);

					if (retries <= 27 && !Common.Utils.IsUIProcessAlive() && !Utils.IsGlHotAttach(vmName))
					{
						Logger.Info("Starting Frontend. BstCommandProcessor not running but service is running.");
						Common.Utils.StartHiddenFrontend(vmName);
					}
				}
				retries--;
				Thread.Sleep(2000);
			}
		}
		else
			res = Common.HTTP.Client.Post(url, data, null, false);

		Logger.Info("HTTPHandler: Got response for " + path + ": " + res);
		return res;
	}

	public static string PostFile(int port, string path, string file, string vmName)
	{
		Logger.Info("HTTPHandler: Uploading {0} to http://127.0.0.1:{1}/{2}", file, port, path);
		ExtendedWebClient client = new ExtendedWebClient(200 * 1000);
		string url = String.Format("http://127.0.0.1:{0}/{1}", port, path);

		string res = null;
		if (port == Common.VmCmdHandler.s_ServerPort)
		{
			bool serviceAlreadyRunning = Common.Utils.StartServiceIfNeeded(vmName);
			if (serviceAlreadyRunning == false)
				Utils.WaitForBootComplete();

			int retries = 1;
			while (retries > 0)
			{
				try {
					byte[] r = client.UploadFile(url, file);
					res = Encoding.UTF8.GetString(r);
					break;
				} catch (WebException e) {
					Logger.Error("Exception in post file");
					HttpWebResponse response = (HttpWebResponse) e.Response;
					Logger.Error(e.Message);
					if (response != null && response.StatusCode == HttpStatusCode.InternalServerError)
					{
						if (retries == 1)
							throw e;
						retries = 2;
					}
				}
				retries--;
				if (retries > 0)
					Thread.Sleep(2000);
			}
		}
		else
		{
			byte[] r = client.UploadFile(url, file);
			res = Encoding.UTF8.GetString(r);
		}


		Logger.Info("HTTPHandler: Got response for " + path + ": " + res);
		return res;
	}

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

	public static void ShowTileInterface(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ShowTileInterface {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);			
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			Common.KeyboardSend.KeyDown(Keys.LWin);
			Common.KeyboardSend.KeyUp(Keys.LWin);

			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in ShowTileInterface: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void CheckForUpdate(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got CheckForUpdate {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try 
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);			
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			SysTray.CheckForUpdate();
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in CheckForUpdate: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void SendGameManagerRequest(Dictionary<string, string> data, string path)
	{
		Logger.Info("Will send {0} request", path);
		try
		{
			int port = Common.Utils.GetPartnerServerPort();
			string url = string.Format("http://127.0.0.1:{0}/{1}", port, path);
			Logger.Info("Sending request to: " + url);

			if (data == null)
				Common.HTTP.Client.Get(url, null, false);
			else
				Common.HTTP.Client.Post(url, data, null, false);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in SendGameManagerRequest: " + exc.ToString());
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


	public static void PostHttpUrl(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got PostHttpUrl {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try
		{
			bool result = false;
			string resp = String.Empty;
			RequestData requestData = HTTPUtils.ParseRequest(req);
			try
			{
				string url = String.Empty;
				Dictionary<string,string> data = new Dictionary<string,string>();
				foreach (string col in requestData.data.AllKeys)
				{
					Logger.Info(col + " = " + requestData.data[col]);
					if (col.Equals("url", StringComparison.InvariantCultureIgnoreCase))
					{
						url = requestData.data[col];
					}
					else
					{
						data.Add(col, requestData.data[col]);
					}
				}
				resp = BlueStacks.hyperDroid.Common.HTTP.Client.Post(url, data, null, false);
				result = true;
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while fetching info from cloud...Err : " + ex.ToString());
			}

			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", result);
			json.WriteMember("response", resp);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in PostHtpUrl: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void ClearAppDataHandler(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ClearAppData {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try
		{
			bool paramPresent = false;
			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
				vmName += "_" + requestData.headers["vmid"].ToString();

			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
				if (String.Compare(key, "package", true) == 0)
				{
					string package = requestData.data[key];

					bool installed = false;
					string version = "", failReason = "";
					installed = Utils.IsAppInstalled(package, vmName, out version, out failReason);

					if (installed == false)
					{
						WriteErrorJson(failReason, res);
						return;
					}

					paramPresent = true;
					string args = string.Format("clearappdata {0}", requestData.data[key]);
					string result = Common.VmCmdHandler.RunCommand(args);
					if (String.Compare(result, "ok", true) != 0)
					{
						WriteErrorJson( string.Format("Unable to clear the app data", package), res);
						return;
					}
				}
			}

			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", paramPresent);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in ClearAppDataInterface: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void StopAppHandler(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got StopApp {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		RequestData requestData = HTTPUtils.ParseRequest(req);
		string vmName = "Android";
		if (requestData.headers["vmid"] != null)
			vmName += "_" + requestData.headers["vmid"].ToString();
		try
		{
			if(!Utils.IsUIProcessAlive())
			{
				Logger.Info("Frontend not running");
				WriteErrorJson("Frontend Not Running", res);
				return;
			}
			bool paramPresent = false;

			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
				if (String.Compare(key, "pkg", true) == 0)
				{
					string package = requestData.data[key];

					bool installed = false;
					string version = "", failReason = "";
					installed = Utils.IsAppInstalled(package, vmName, out version, out failReason);

					if (installed == false)
					{
						WriteErrorJson(failReason, res);
						return;
					}

					paramPresent = true;
					string args = string.Format("StopApp {0}", requestData.data[key]);
					string result = Common.VmCmdHandler.RunCommand(args);
					if (String.Compare(result, "ok", true) == 0)
					{
						string url = string.Format("http://127.0.0.1:{0}/{1}",
								Utils.GetFrontendServerPort(vmName), Common.Strings.StopAppInfo);

						Dictionary<string, string> data = new Dictionary<string, string>();
						data.Add("appPackage", package);

						if(Utils.WaitForFrontendPingResponse(vmName) == true)
						{
							Logger.Info("Sending StopAppInfo request to: " + url);
							Common.HTTP.Client.Post(url, data, null, false);
						}
						else
						{
							Logger.Error("Frontend not responding to ping response");
							WriteErrorJson("Frontend Server not running", res);
							return;
						}
					}
					else
					{
						WriteErrorJson( string.Format("Unable to stop the app: {0}", package), res);
						return;
					}
				}
			}

			sCurrentAppPackageFromRunApp = "";
			Logger.Info("Assigning value empty to sCurrentAppPackageFromRunApp");
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", paramPresent);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in StopAppInterface: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * request for making changes after an app has been installed
	 * receive the icon(s) and json info of the installed app
	 */
	public static void ApkInstalled(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ApkInstalled {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		bool isS2P = false;
		bool showMsgBox = false;

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
				vmName += "_" + requestData.headers["vmid"].ToString();
			
			Logger.Info("Data:");
			String source = String.Empty;
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);

				string jsonString = requestData.data[key];
				Logger.Info(jsonString);
				JSonReader readjson = new JSonReader();
				IJSonObject fullJson = readjson.ReadAsJSonObject(jsonString);

				TimelineStatsSender.HandleAppInstallEvents(fullJson);
				source = fullJson["source"].StringValue.Trim();
				if (string.Compare(source, "s2p") == 0)
				{
					foreach (string fileKey in requestData.files.AllKeys)
					{
						string filePath = requestData.files[fileKey];
						if (File.Exists(filePath))
							File.Delete(filePath);
					}

					WriteSuccessJson(res);
					isS2P = true;
				}

				string isUpdate = fullJson["update"].StringValue.Trim();
				string package = fullJson["package"].StringValue.Trim();
				Logger.Info("package: {0}", package);
				string version = fullJson["version"].StringValue.Trim();

				if (package == "com.android.vending")
				{
					Logger.Info("HTTPHandler: Not creating shortcut for " + package);
					break;
				}

				if (isS2P == false)
				{
					/*
					 * Removing package from json if currently present.
					 * Done to handle app upgrades where the activity name has been changed
					 */
					Logger.Info("Removing package if present");
					lock (s_sync)
					{
						AppUninstaller.RemoveFromJson(package);
						if (BlueStacks.hyperDroid.Common.Oem.Instance.IsRemoveFromGameManagerJsonOnApkInstalled)
						{
							AppUninstaller.RemoveFromGameManagerJson(package);
						}
					}

					Logger.Info("Files:");
					foreach (string filekey in requestData.files.AllKeys)
					{
						Logger.Info("Key: {0}, Value: {1}", filekey,
								requestData.files[filekey]);
						string filePath = requestData.files[filekey];
						string fileName = Path.GetFileName(filePath);
						string newFilePath = Path.Combine(Common.Strings.GadgetDir, fileName);
						try {
							if (File.Exists(newFilePath))
								File.Delete(newFilePath);
							File.Move(filePath, newFilePath);
						} catch (Exception e) {
							Logger.Error("Exception when handling app icons");
							Logger.Error(e.ToString());
						}
					}
				}

				string activities = fullJson["activities"].StringValue.Trim();
				Logger.Info(activities);
				readjson = new JSonReader();
				IJSonObject activitiesJson = readjson.ReadAsJSonObject(activities);	

				for (int j=0; j<activitiesJson.Length; j++)
				{
					string img = activitiesJson[j]["img"].StringValue.Trim();
					string activity = activitiesJson[j]["activity"].StringValue.Trim();
					string appname = activitiesJson[j]["name"].StringValue.Trim();

					if (isS2P == true)
					{
						Common.Stats.SendAppInstallStats(appname, package, version,
								Common.Stats.AppInstall, isUpdate, source);
						return;
					}

					lock (s_sync)
					{
						Agent.ApkInstall.AppInstalled(
								appname,
								package,
								activity,
								img,
								version,
								isUpdate,
								vmName,
								source);	
					}
				}

				if (Oem.Instance.IsBTVBuild && !Utils.IsOSWinXP())
				{
					ReportAppInstallStatusToGameManager(package, true);
				}

				/*
				 * Can't display a message box here for all apps without a launchable activity
				 * as it will cause prompts for each Google Apk installed when executing the google market msi.
				 * The extra check will ensure apks containing 'android' as part of their
				 * package name will not cause a message box to be shown
				 */
				if (activitiesJson.Length == 0 && !package.Contains("android"))	// the apk doesn't have a launchable activity.
				{
					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMessageBoxToBeDisplayed)
						showMsgBox = true;
				}
				else
					showMsgBox = false;
			}

			Logger.Info("returning from appinstalled");
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);

			if (showMsgBox)
			{
				MessageBox.Show("App has been installed");
			}

		} catch (Exception exc) {
			Logger.Error("Exception in Server ApkInstalled");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * request for making changes after an app has been uninstalled
	 * receive package name of the uninstalled app
	 */
	public static void AppUninstalled(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got AppUninstalled {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
				vmName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data:");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);

				string jsonString = requestData.data[key];
				JSonReader readjson = new JSonReader();
				IJSonObject jsonObj = readjson.ReadAsJSonObject(jsonString);

				TimelineStatsSender.HandleAppUninstallEvents(jsonObj);
				string package = jsonObj["package"].StringValue.Trim();
				string source = jsonObj["source"].StringValue.Trim();

				if (Oem.Instance.IsBTVBuild && !Utils.IsOSWinXP())
				{
					ReportAppInstallStatusToGameManager(package, false);
				}

				if (string.Compare(source, "s2p") == 0)
				{
					string version = HDAgent.GetVersionFromPackage(package, vmName);
					string name = JsonParser.GetAppNameFromPackage(package);
					Common.Stats.SendAppInstallStats(name, package, version, Common.Stats.AppUninstall, "false", source);
					WriteSuccessJson(res);
					return;
				}

				Logger.Info("package: {0}", package);

				lock (s_sync)
				{
					AppUninstaller.AppUninstalled(package, vmName, source);
				}
			}

			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server AppUninstalled");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void ReportAppInstallStatusToGameManager(string package, bool isInstall)
	{
		Thread thread = new Thread(delegate() {
			try
			{
				string url = string.Format("http://127.0.0.1:{0}/receiveAppInstallStatus",
					Common.Utils.GetBTVServerPort());

				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("package", package);
				data.Add("isInstall", isInstall.ToString().ToLower());

				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch(Exception ex)
			{
				Logger.Error("Error: {0}", ex.Message);
			}
		});
		thread.IsBackground = true;
		thread.Start();
        }

	/*
	 * Returns apps.json or more_apps.json
	 * in the form:
	 * {"json": "<apps.json>"}
	 */
	public static void GetAppList(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got GetAppList {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

		Logger.Info("QueryString:");
		foreach (string key in requestData.queryString.AllKeys)
		{
			Logger.Info("Key: {0}, Value: {1}", key, requestData.queryString[key]);
		}

		string failReason;
		string appList = Utils.GetInstalledPackages(Common.Strings.VMName, out failReason);

		if (String.IsNullOrEmpty(failReason) == true) {
			Common.HTTP.Utils.Write(appList, res);
		} else {
			WriteErrorJson(failReason, res);
		}
	}

	/*
	 * Returns the image associated with an app
	 * the image name needs to be specified in the query string
	 * e.g. image?foo.png
	 */
	public static void GetAppImage(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got GetAppList {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

		Logger.Info("QueryString:");
		foreach (string key in requestData.queryString.AllKeys)
		{
			Logger.Info("Key: {0}, Value: {1}", key, requestData.queryString[key]);
		}

		string image = requestData.queryString["image"];
		string filePath = Path.Combine(Common.Strings.GadgetDir, image);

		if (File.Exists(filePath))
		{
			byte[] content = File.ReadAllBytes(filePath);
			res.Headers.Add("Cache-Control: max-age=2592000"); // 30 days
			res.OutputStream.Write(content, 0, content.Length);
		}
		else
		{
			res.StatusCode = 404;
			res.StatusDescription = "Not Found.";
		}
	}

	public static void ReleasApkInstallThread(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ReleasApkInstallThread {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		if(sApkInstallThread != null)
		{
			Logger.Info("sApkInstallThread  is not null, making it null now");
			sApkInstallThread = null;
		}
		else
		{
			Logger.Info("sApkInstallThread already marked null");
		}
	}
	/*
	 * request for installing an app
	 * receive path of the apk to be installed
	 */
	public static void ApkInstall(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ApkInstall {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		string result = "";

		if (sApkInstallThread != null)
		{
			result = "APK_INSTALLATION_IN_PROGRESS";
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteMember("reason", result);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
			return;
		}
		if (sAppUninstallationInProgress == true)
		{
			result = "APP_UNINSTALLATION_IN_PROGRESS";
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteMember("reason", result);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
			return;
		}

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
			{
				Logger.Info("got vmid as " + requestData.headers["vmid"].ToString());
				vmName += "_" + requestData.headers["vmid"].ToString();
			}
			Logger.Info("Data:");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			sApkInstallThread = new Thread(delegate() {
					result = Agent.ApkInstall.InstallApk(requestData.data["path"], vmName);
					});
			sApkInstallThread.IsBackground = true;
			sApkInstallThread.Start();

			sApkInstallThread.Join();
			Logger.Info("Apk Install thread has returned");

			if(result == "")
			{
				Logger.Info("the apkinstallationresult is {0}", sApkInstallResult);
				result = sApkInstallResult;
			}

			sApkInstallResult = "";
			sApkInstallThread = null;
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteMember("reason", result);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc) {
			Logger.Error("Exception in Server AppInstall");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
		sApkInstallResult = "";
		sApkInstallThread = null;
	}

	/*
	 * request for uninstalling an app
	 * receive package and name of the app to be uninstalled
	 */
	public static void AppUninstall(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got AppUninstall {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		string reason = "";
		if (sApkInstallThread != null)
		{
			reason = "APK_INSTALLATION_IN_PROGRESS";
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteMember("reason", reason);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
			return;
		}
		if (sAppUninstallationInProgress == true)
		{
			reason = "APP_UNINSTALLATION_IN_PROGRESS";
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteMember("reason", reason);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
			return;
		}
		try {
			sAppUninstallationInProgress = true;
			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
				vmName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data:");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			String package = requestData.data["package"];
			Logger.Info("package: {0}", package);
			String name = requestData.data["name"];
			Logger.Info("name: {0}", name);
			string nolookup = requestData.data["nolookup"];
			Logger.Info("nolookup: {0}", nolookup);

			int r = AppUninstaller.SilentUninstallApp(name, package, (nolookup != null), vmName, out reason);
			sAppUninstallationInProgress = false;

			bool status;
			if (r == 0)
				status = true;
			else
				status = false;

			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", status);

			if (!String.IsNullOrEmpty(reason))
			{
				json.WriteMember("reason", reason);
			}
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server AppUninstall");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * Request for running an app
	 * data should contain 2 keys: package and activity
	 */
	public static void RunApp(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got RunApp {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		try {
			bool success = true;

			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
				vmName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data:");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			sCurrentAppPackageFromRunApp = "";
			string package = requestData.data["package"];
			string activity = requestData.data["activity"];
			string apkUrl = requestData.data["apkUrl"];
			string json = requestData.data["json"];

			if (req.UserAgent.Contains("BlueStacks") == false)
			{
				if (String.IsNullOrEmpty(package) == true)
				{
					WriteErrorJson("Please give a non-empty package name", res);
					return;
				}
				else if (String.IsNullOrEmpty(activity) == true)
				{
					WriteErrorJson("Please give a non-empty activity name", res);
					return;
				}

				bool installed = false;
				string version = "", failReason = "";

				/*
				 * Adding this call to ensure that BootFailure logs
				 * are reported in case of tencent, as FE is responsible
				 * for sending them.
				 */
				Utils.StartHiddenFrontend(vmName);

				installed = Utils.IsAppInstalled(package, vmName, out version, out failReason);

				if (installed == false)
				{
					WriteErrorJson(failReason, res);
					return;
				}

				string url = string.Format("http://127.0.0.1:{0}/{1}",
						Utils.GetFrontendServerPort(vmName), Common.Strings.RunAppInfo);

				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("appPackage", package);

				if(Utils.WaitForFrontendPingResponse(vmName) == true)
				{
					Logger.Info("Sending RunAppInfo request to: " + url);
					Common.HTTP.Client.Post(url, data, null, false);
				}
				else
				{
					Logger.Error("Frontend not responding to ping response");
					WriteErrorJson("Frontend Server not running", res);
					return;
				}
			}

			if(package.Equals("com.tencent.tmgp.rxcq", StringComparison.CurrentCultureIgnoreCase) == true)
			{
				/*
				 * Adding 15 second sleep before launching Tencent new App
				 * this hack seems to make the app lauch in time
				 */
				Logger.Info("Before 15 second sleep for package : {0}", package);
				Thread.Sleep(15000);
				Logger.Info("After 15 second sleep for package : {0}", package);
			}

			if (String.IsNullOrEmpty(json))
			{
				string request = String.Format("runex {0}/{1}", package, activity);
				if (apkUrl != null)
					request = String.Format("{0} {1}", request, apkUrl);
				success = HDAgent.DoRunCmd(request, vmName);
				if(success == true)
				{
					Logger.Info("Assigning value {0} to sCurrentAppPackageFromRunApp", package);
					sCurrentAppPackageFromRunApp = package;
				}
			}
			else
			{
				int port = Utils.GetBstCommandProcessorPort(vmName);
				Dictionary<string, string> data= new Dictionary<string, string>();
				data.Add("component", package + "/" + activity);

				data.Add("extras", json);

				String url = String.Format("http://127.0.0.1:{0}/{1}", port, Common.Strings.AndroidCustomActivityLaunchApi);
				Logger.Info("the url being hit is {0}", url);

				string response = Common.HTTP.Client.Post(url, data, null, false);

				Logger.Info("the response we get is " + response);

				JSonReader readjson = new JSonReader();
				IJSonObject resJson = readjson.ReadAsJSonObject(response);
				string result = resJson["result"].StringValue.Trim();
				if (result == "ok")
				{
					success = true;
					Logger.Info("Assigning value {0} to sCurrentAppPackageFromRunApp", package);
					sCurrentAppPackageFromRunApp = package;
				}
				else
				{
					success = false;
				}
			}

			JSonWriter jsonWriter = new JSonWriter();
			jsonWriter.WriteObjectBegin();
			jsonWriter.WriteMember("success", success);
			jsonWriter.WriteObjectEnd();
			Common.HTTP.Utils.Write(jsonWriter.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server RunApp");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void InstallAppByURL(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got InstallAppByURL {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
				vmName += "_" + requestData.headers["vmid"].ToString();

			string		appURL		= requestData.data["appURL"];
			string		storeType	= requestData.data["storeType"];

			string prog = Path.Combine(HDAgent.s_InstallDir, "HD-RunApp.exe");
			Process.Start(prog);

			string		installerPath;

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("url", appURL);

			switch (storeType)
			{
				case "amz":
					Logger.Info("Trying to install an Amazon app: {0}", appURL);
					installerPath = Common.Strings.AppInstallUrl;
					break;

				case "opera":
					Logger.Info("Trying to install an Opera app: {0}", appURL);
					installerPath = HDAgent.s_BrowserInstallPath;
					break;

				default:
					Logger.Error("Invalid storeType: " + storeType);
					return;
			}

			try
			{
				HTTPHandler.Post(Common.VmCmdHandler.s_ServerPort, installerPath, data, vmName);

			}
			catch (Exception exc)
			{
				Logger.Error("Exception when sending post request");
				Logger.Error(exc.ToString());
			}

			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in InstallAppByURL");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * restart request
	 */
	public static void Restart(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got Restart {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		JSonWriter json = new JSonWriter();
		json.WriteArrayBegin();
		json.WriteObjectBegin();
		json.WriteMember("success", true);
		json.WriteObjectEnd();
		json.WriteArrayEnd();
		Common.HTTP.Utils.Write(json.ToString(), res);
		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

		Process.Start(Path.Combine(HDAgent.s_InstallDir, "HD-Restart.exe"), "Android");

	}

	/*
	 * ping request
	 */
	public static void Ping(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got Ping {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		try {
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(CheckForJsonp(json.ToString(), req), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server Ping");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * request for receiving app failure info
	 * this info is then sent to the cloud
	 */
	public static void AppCrashedInfo(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got AppCrashedInfo {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		string shortPackageName	= "";
		string packageName = "";
		string versionCode = "";
		string versionName = "";

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data:");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);

				string jsonString = requestData.data[key];
				JSonReader readjson = new JSonReader();
				IJSonObject infoJson = readjson.ReadAsJSonObject(jsonString);

				shortPackageName = infoJson["shortPackageName"].StringValue.Trim();
				Logger.Info("shortPackageName: {0}", shortPackageName);
				/*
				 * This try-catch block to handle cases where only the short package name is received
				 * This will happen in cases where the short package name is not present in pm list
				 */
				try {
					packageName	= infoJson["packageName"].StringValue.Trim();
					Logger.Info("packageName: {0}", packageName);
					versionCode	= infoJson["versionCode"].StringValue.Trim();
					Logger.Info("versionCode: {0}", versionCode);
					versionName	= infoJson["versionName"].StringValue.Trim();
					Logger.Info("versionName: {0}", versionName);
				} catch {
					Logger.Error("Only shortPackageName received");
				}

				if (Features.IsFeatureEnabled(Features.WRITE_APP_CRASH_LOGS) == true)
				{
					int retries = 10;
					while (retries-- > 0)
					{
						//Write App Crash Logs
						if (Common.Utils.IsAlreadyRunning(Common.Strings.AppCrashInfoFile, out sAppCrashInfoWriteLock) == false)
						{
							try {
								string fileContent = "";
								string logFilePath = Path.Combine(Utils.GetLogDir(), Strings.AppCrashInfoFile);
								if (File.Exists(logFilePath))
								{
									fileContent = File.ReadAllText(logFilePath, Encoding.UTF8);
									fileContent += "\n";
								}
								JSonWriter writer = new JSonWriter();
								writer.WriteObjectBegin();
								writer.WriteMember("shortPackageName", shortPackageName);
								writer.WriteMember("packageName", packageName);
								writer.WriteMember("versionName", versionName);
								writer.WriteMember("versionCode", versionCode);
								writer.WriteMember("time", DateTime.Now.ToString());
								writer.WriteObjectEnd();
								File.WriteAllText(logFilePath, fileContent + writer.ToString(), Encoding.UTF8);
							} catch (Exception e) {
								Logger.Error("Error Occurred, Err: {0}", e.ToString());
							}
							sAppCrashInfoWriteLock.Close();
							break;
						}
						else
							Thread.Sleep(1000);
					}
				}
			}

			try
			{
				Logger.Info("Closing app-----------" + packageName);
				int port = Common.Utils.GetPartnerServerPort();
				string url = String.Format("http://127.0.0.1:{0}/{1}", port, "closecrashedapptab");
				Logger.Info("the url hit is " + url);

				Dictionary<string, string> data = new Dictionary<string, string>();

				data.Add("package", packageName);

				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception ex)
			{
				Logger.Error("There occured an exception while trying to post data " + ex.ToString());
			}

			if (HDAgent.sOemWindowMapper.ContainsKey(Oem.Instance.OEM) == true &&
					packageName.IndexOf(Common.Strings.BlueStacksPackagePrefix) == -1)
			{
				if(shortPackageName.Equals(sCurrentAppPackageFromRunApp, StringComparison.CurrentCultureIgnoreCase))
				{
					Logger.Info("notifying TGP of App Crash for package {0}", shortPackageName);
					HDAgent.NotifyAppCrashToParentWindow(HDAgent.sOemWindowMapper[Oem.Instance.OEM][0],
							HDAgent.sOemWindowMapper[Oem.Instance.OEM][1]);
				}
				else
				{
					Logger.Info("Not sending AppCrash Info for package {0} as currentAppPackagename from runapp is {1}", shortPackageName, sCurrentAppPackageFromRunApp); 
				}
			}

			Logger.Info("Files:");
			string filePath, fileName;
			foreach (string key in requestData.files.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.files[key]);
				filePath = requestData.files[key];
				fileName = Path.GetFileName(filePath);
			}

			/*
			 * Send app failure info and file to cloud
			 */
			StartLogCollection("APP_CRASHED",  packageName);
			JSonWriter json = new JSonWriter();
			json.WriteArrayBegin();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			json.WriteArrayEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server AppCrashedInfo");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void StartLogCollection(string error, string detail)
	{
		if (Oem.Instance.IsReportExeAppCrashLogs)
		{
			NotifyLogReportingToParentWindow(0);
			Logger.Info("starting the logging of apk installation failure");
			Process proc =  new Process();
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)key.GetValue("InstallDir");
			proc.StartInfo.FileName = Path.Combine(installDir, "HD-LogCollector.exe");
			string arguments = "-ReportCrashLogs" +" " + error + " " + detail;
			Logger.Info("The arguments being passed to log collector is :{0}", arguments);
			proc.StartInfo.Arguments = arguments;
			proc.Start();
			proc.WaitForExit();
			NotifyLogReportingToParentWindow(1);
		}
	}

	public static void NotifyLogReportingToParentWindow(int param)
	{
		try
		{
			Dictionary<String, String[]> oemWindowMapper = new Dictionary<string, string[]>();
			Utils.AddMessagingSupport(out oemWindowMapper);
			if (oemWindowMapper != null && oemWindowMapper.ContainsKey(Common.Oem.Instance.OEM))
			{
				string className = oemWindowMapper[Common.Oem.Instance.OEM][0];
				string windowName = oemWindowMapper[Common.Oem.Instance.OEM][1];
				Logger.Info("Sending WM_USER_LOGS_REPORTING message to class = {0}, window = {1}", className, windowName);
				IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				uint wparam = (uint)param;
				Logger.Info("Sending wparam : {0}", wparam);
				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_USER_LOGS_REPORTING, (IntPtr)wparam, IntPtr.Zero);
			}
		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
		}
	}

	/*
	 * request for performing some action
	 */
	public static void DoAction(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got DoAction {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.queryString[key]);
			}

			if (requestData.data["action"] == "openforgotpassword")
			{
				RegistryKey regkey = Registry.LocalMachine.OpenSubKey(s_CloudRegKey);
				string url = Service.Host + "/?forgotpassword=1";
				Process.Start(url);
			}

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server DoAction");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * request for getting guid, email address, product version and culture of the user
	 */
	public static void GetUserData(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got GetUserData {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		try {
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.CloudRegKeyPath);
			String email = (String) key.GetValue("Email", "");
			key.Close();

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("guid", User.GUID);
			json.WriteMember("email", email);
			json.WriteMember("version", Version.STRING);
			json.WriteMember("culture", CultureInfo.CurrentCulture.Name.ToLower());
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(CheckForJsonp(json.ToString(), req), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server GetUserData");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * request for showing notification
	 * the following data can be present:
	 * msgAction: the action to be performed by the notification. can be among:
	 * 		None, Amazon App, Opera App, Web URL, Download and Execute, Start Android App
	 * displayTitle: the title of the notification
	 * displayMsg: the message to be shown
	 * actionURL:
	 * 	url of the app to be installed in case of Amazon App and Opera App;
	 * 	url of the page to be opened in case of Web URL;
	 * 	url of the file to be downloaded in case of Download and Execute;
	 * 	ignored in case of Start Android App
	 * fileName:
	 * 	full filename of the file being run in Download and Execute e.g. foo.exe
	 * 	name of the app to be run in case of Start Android App
	 * imageURL: if present, gives the url of the image to be used
	 */
	public static void ShowNotification(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ShowNotification {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string msgAction	= requestData.data["msgAction"];
			string displayTitle	= requestData.data["displayTitle"];
			string displayMsg	= requestData.data["displayMsg"];
			string actionURL	= requestData.data["actionURL"];
			string fileName		= requestData.data["fileName"];
			string imageURL		= requestData.data["imageURL"];

			CloudAnnouncement.ShowNotification(msgAction, displayTitle, displayMsg, actionURL, fileName, imageURL);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server ShowNotification");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void ShowFeNotification(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ShowFeNotification {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		RequestData requestData = HTTPUtils.ParseRequest(req);
		string vmName = "Android";
		if (requestData.headers["vmid"] != null)
			vmName += "_" + requestData.headers["vmid"].ToString();
		try
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
			int port = (int)key.GetValue("FrontendServerPort");

			string url = string.Format("http://127.0.0.1:{0}/{1}", port, Common.Strings.ShowFeNotificationUrl);

			JSonReader readjson = new JSonReader();
			IJSonObject jsonData = readjson.ReadAsJSonObject(requestData.data["data"]);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("data", jsonData.ToString());

			Logger.Info("Sending Fe-notification request to: " + url);
			Common.HTTP.Client.Post(url, data, null, false);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in ShowFeNotification: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void SendAppDataToFE(String package, String activity, String callingPackage, string vmName)
	{
		Logger.Info("HTTPHandler:SendAppDataToFE(\"{0}\",\"{1}\")", package, activity);

		try
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
			int port = (int)key.GetValue("FrontendServerPort");

			string url = string.Format("http://127.0.0.1:{0}/{1}",
					port, Common.Strings.AppDataFEUrl);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("package", package);
			data.Add("activity", activity);
			data.Add("callingPackage", callingPackage);

			Logger.Info("Sending SendAppDataToFE request to: " + url);
			Common.HTTP.Client.Post(url, data, null, false);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in SendAppDataToFE: " +
					exc.ToString());
		}
	}

	public static void SwitchToLauncher(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got SwitchToLauncher {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		RequestData requestData = HTTPUtils.ParseRequest(req);
		string vmName = "Android";
		if (requestData.headers["vmid"] != null)
			vmName += "_" + requestData.headers["vmid"].ToString();
		try
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
			int port = (int)key.GetValue("FrontendServerPort");

			string url = string.Format("http://127.0.0.1:{0}/{1}", port, Common.Strings.SwitchToLauncherUrl);

			Logger.Info("Sending SwitchToLauncher request to: " + url);
			Common.HTTP.Client.Get(url, null, false);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in SwitchToLauncher: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void SwitchToWindows(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got SwitchToWindows {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		RequestData requestData = HTTPUtils.ParseRequest(req);
		string vmName = "Android";
		if (requestData.headers["vmid"] != null)
			vmName += "_" + requestData.headers["vmid"].ToString();
		try
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
			int port = (int)key.GetValue("FrontendServerPort");

			string url = string.Format("http://127.0.0.1:{0}/{1}", port, Common.Strings.SwitchToWindowsUrl);

			Logger.Info("Sending SwitchToWindows request to: " + url);
			Common.HTTP.Client.Get(url, null, false);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in SwitchToWindows: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void ShowSysTrayNotification(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ShowSysTrayNotification {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		try
		{
			Logger.Debug("Tray notification Data:");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string message		= requestData.data["message"];
			string title		= requestData.data["title"];
			int timeout		= Convert.ToInt32(requestData.data["timeout"]);
			if(requestData.data["status"] != null && requestData.data["status"].Equals("error"))
				SysTray.ShowTrayStatus(System.Windows.Forms.ToolTipIcon.Error, title, message, timeout);
			else
				SysTray.ShowTrayStatus(System.Windows.Forms.ToolTipIcon.Info, title, message, timeout);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in ShowSysTrayNotification: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void ExitAgent(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got ExitAgent {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		try
		{
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);

			SysTray.DisposeIcon();
			Environment.Exit(0);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in ExitAgent: " + exc.ToString());
			WriteErrorJson(exc.Message, res);

			Environment.Exit(-1);
		}
	}

	public static void QuitFrontend(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got QuitFrontend {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		RequestData requestData = HTTPUtils.ParseRequest(req);
		string vmName = "Android";
		if (requestData.headers["vmid"] != null)
			vmName += "_" + requestData.headers["vmid"].ToString();
		try
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
			int port = (int)key.GetValue("FrontendServerPort");

			string url = string.Format("http://127.0.0.1:{0}/{1}", port, Common.Strings.QuitFrontend);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("reason", "app_exiting");

			Logger.Info("Sending Quit request to: " + url);
			Common.HTTP.Client.Post(url, data, null, false);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in QuitFrontend: " + exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * request for adding a new app
	 * app_type must be present in the data and can be one of these:
	 * app:		a normal app which will be shown in My Apps
	 * store:	a store app which will be shown in App Stores
	 * featured:	a featured app which will be shown in Suggested Downladds
	 *
	 * for all app_types, the image for the shortcut needs to be sent as well
	 *
	 * for app_type 'app' and 'store', the following data is required:
	 * name, package, activity
	 * for app_type 'app' and 'store', the apk needs to be already installed for the shortcuts to work
	 *
	 * for app_type 'featured', the following data is required:
	 * name, version, package
	 * the following data can also be present for featured apps:
	 * asin: in case of amazon apps
	 * org_name: name of the developer
	 * url: a download url
	 */
	public static void AddApp(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got AddApp {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string filePath = "";
			string fileName = "";

			Logger.Info("Files:");
			foreach (string key in requestData.files.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.files[key]);
				filePath = requestData.files[key];
				fileName = Path.GetFileName(filePath);
			}

			string appType	= requestData.data["app_type"];
			string name;
			string img, package, activity, version;
			string icon;
			string shortcutName, runAppFile, imagePath, arguments;

			switch(appType)
			{
				case "app":
					name		= requestData.data["name"];
					img		= fileName;
					package		= requestData.data["package"];
					activity	= requestData.data["activity"];

					try
					{
						version		= requestData.data["version"];
					}
					catch
					{
						version		= "Unknown";
					}

					imagePath = Path.Combine(Common.Strings.GadgetDir, fileName);
					if (File.Exists(imagePath))
						File.Delete(imagePath);
					File.Move(filePath, imagePath);

					name = Regex.Replace(name, @"[\x22\\\/:*?|<>]", " ");
					lock (s_sync)
					{
						JsonParser.AddToJson(new AppInfo(
									name,
									img,
									package,
									activity,
									"0",
									"no",
									version
									));
					}


					Utils.ResizeImage(imagePath);
					icon = Utils.ConvertToIco(s_Png2ico, imagePath, s_IconsDir);

					if (!File.Exists(icon))
						icon = s_IconFile;

					runAppFile = Path.Combine(HDAgent.s_InstallDir,
							"HD-RunApp.exe");
					shortcutName = Path.Combine(s_MyAppsDir, name) + ".lnk";
					arguments = String.Format("-p {0} -a {1}", package, activity);
					if (HDAgent.CreateShortcut(runAppFile, shortcutName,
							"", icon, arguments, 0) != 0)
						Logger.Error("Couldn't create shortcut {0}", shortcutName);
					else
						Logger.Info("Created shortcut {0}", shortcutName);

					break;

				case "store":
					name		= requestData.data["name"];
					img		= fileName;
					package		= requestData.data["package"];
					activity	= requestData.data["activity"];

					try
					{
						version		= requestData.data["version"];
					}
					catch
					{
						version		= "Unknown";
					}

					imagePath = Path.Combine(Common.Strings.GadgetDir, fileName);
					if (File.Exists(imagePath))
						File.Delete(imagePath);
					File.Move(filePath, imagePath);

					name = Regex.Replace(name, @"[\x22\\\/:*?|<>]", " ");
					lock (s_sync)
					{
						JsonParser.AddToJson(new AppInfo(
									name,
									img,
									package,
									activity,
									"0",
									"yes",
									version
									));
					}

					Utils.ResizeImage(imagePath);
					icon = Utils.ConvertToIco(s_Png2ico, imagePath, s_IconsDir);

					if (!File.Exists(icon))
						icon = s_IconFile;

					runAppFile = Path.Combine(HDAgent.s_InstallDir,
							"HD-RunApp.exe");
					shortcutName = Path.Combine(s_AppStoresDir, name) + ".lnk";
					arguments = String.Format("-p {0} -a {1}", package, activity);
					if (HDAgent.CreateShortcut(runAppFile, shortcutName,
							"", icon, arguments, 0) != 0)
						Logger.Error("Couldn't create shorcut {0}", shortcutName);
					else
						Logger.Info("Created shorcut {0}", shortcutName);

					break;
			}

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server AddApp");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * API to log click events from Android. The API needs to:
	 *  POST to HDA/logappclick:
	 *  PARAMS:
	 *    - package
	 *    - activity
	 *    - clickloc
	 *
	 * clickloc is the location from which these apps clicks have originated. Say for Home screen apps, it can simply be "Home"
	 *
	 * Curl example:
	 * curl 127.0.0.1:2862/logappclick -d "package=com.foo.bar" -d "activity=MainActivity" -d "clickloc=Home"
	 */
	public static void LogAndroidClickEvent(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got LogAndroidClickEvent {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			Logger.Info("Data");

			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string package			= requestData.data["package"];
			string clickLocation	= requestData.data["clickloc"];
			string appName			= requestData.data["appname"];
			string version			= requestData.data["appver"];

			if ((string.Compare(package, "com.bluestacks.home", true) != 0) &&
					(string.Compare(package, "com.bluestacks.setup", true) != 0) &&
					(string.Compare(package, "mpi.v23", true) != 0) &&
					(string.Compare(package, "com.android.systemui", true) != 0) &&
					(string.Compare(package, "com.bluestacks.s2p", true) != 0) &&
					(string.Compare(package, "com.bluestacks.gamepophome", true) != 0))
			{
				Common.Stats.SendAppStats(appName, package, version, clickLocation, Common.Stats.AppType.app);
			}

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server LogAndroidClickEvent");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void HandleFrontendStatusUpdate(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got HandleFrontendStatusUpdate {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
			{
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			}
		    	TimelineStatsSender.HandleFrontendStatusUpdate(requestData);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);

		} catch (Exception exc) {
			Logger.Error("Exception in HandleFrontendStatusUpdate");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	} 

	public static void HandleS2PEvents(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got HandleS2PEvents {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		    	TimelineStatsSender.HandleS2PEvents (requestData);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);

		} catch (Exception exc) {
			Logger.Error("Exception in HandleS2PEvents");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void SetNewLocation(HttpListenerRequest req,HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got SetNewLocation {0} request from {1}",
				req.HttpMethod,req.RemoteEndPoint.ToString());

		try{
			RequestData requestData=HTTPUtils.ParseRequest(req);
			Common.Strings.VMName="Android";
			if(requestData.headers["vmid"] != null)
				Common.Strings.VMName +="_" + requestData.headers["vmid"].ToString();

			double latitude = Convert.ToDouble(requestData.data["latitude"]);
			double longitude = Convert.ToDouble(requestData.data["longitude"]);
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("latitude",  latitude);
			json.WriteMember("longitude", longitude);
			json.WriteObjectEnd();
			Logger.Info("latitude={0} longitude={1}",latitude,longitude);
			string setLocationUrl = string.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, "setNewLocation");
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("data",json.ToString());
			string resp = Common.HTTP.Client.Post(setLocationUrl, data, null, false);
			Logger.Info("the response is {0}", resp);
			JSonReader readjson = new JSonReader();
			IJSonObject infoJson = readjson.ReadAsJSonObject(resp);
			if(infoJson["result"].StringValue=="ok")
				WriteSuccessJson(res);
			else
				WriteErrorJson(resp,res);

		} catch(Exception exc){
			Logger.Error("Exception in SetNewLocation");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message,res);
		}
	}

	public static void HandleAdEvents(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got HandleAdEvents {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		    	TimelineStatsSender.HandleAdEvents (requestData);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);

		} catch (Exception exc) {
			Logger.Error("Exception in HandleAdEvents");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	} 

	/*
	 * API to log app channel click events from Android. The API needs to:
	 *  POST to HDA/logwebappchannelclick:
	 *  PARAMS:
	 *    - package
	 *    - activity
	 *    - clickloc
	 *
	 * clickloc is the location from which these apps clicks have originated. Say for Home screen apps, it can simply be "Home"
	 *
	 * Curl example:
	 * curl 127.0.0.1:2862/logappclick -d "package=com.foo.bar" -d "activity=MainActivity" -d "clickloc=Home"
	 */
	public static void LogWebAppChannelClickEvent(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got LogWebAppChannelClickEvent {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			Logger.Info("Data");

			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string package		= requestData.data["package"];
			string clickLocation	= requestData.data["clickloc"];
			string appName		= requestData.data["appname"];

			Common.Stats.SendWebAppChannelStats(appName, package, clickLocation);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server LogWebAppChannelClickEvent");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * API to log app searches from Android. The API needs to:
	 *  POST to HDA/logappsearch:
	 *  PARAMS:
	 *    - keyword
	 *
	 * Curl example:
	 * curl 127.0.0.1:2862/logappclick -d "keyword=Angry Birds"
	 */
	public static void LogAndroidSearchEvent(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got LogAndroidSearchEvent {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			Logger.Info("Data");

			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string keyword = requestData.data["keyword"];

			Common.Stats.SendSearchAppStats(keyword);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server LogAndroidSearchEvent");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	/*
	 * request for setting clipboard data on windows to match data put on clipboard on android
	 */
	public static void SetClipboardData (HttpListenerRequest req,
		HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got SetClipboardData {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string clipboardText = requestData.data["text"];
			Logger.Debug("ClipboradText {0}", clipboardText);
			Clipboard.SetText(clipboardText);

			HDAgent.clipboardClient.SetCachedText(clipboardText);
			Logger.Debug("CachedText {0}",HDAgent.clipboardClient.GetCachedText());

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);

		} catch (Exception exc) {
			Logger.Error("Exception in Server SetClipboardData");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void SendPendingNotifications(Object obj)
	{
		Logger.Info("Inside SendPendingNotifications");
		s_NotificationLockHelper[1] = true;
		s_LockForTurn = 0;
		if (s_NotificationLockHelper[0] && s_LockForTurn == 0)
		{
			Logger.Info(string.Format("Critical resources are being used, returning"));
			s_NotificationLockHelper[1] = false;
			return;
		}

		try
		{
			while(s_PendingNotifications.Count > 0)
			{
				if (s_PendingNotifications.First.Value.NotificationSent == true)
				{
					s_PendingNotifications.RemoveFirst();
					continue;
				}
				string pkg = s_PendingNotifications.First.Value.Package;
				string name = s_PendingNotifications.First.Value.AppName;
				string msg = s_PendingNotifications.First.Value.Message;
				SysTray.ShowInfoShort(name, msg);
				s_PendingNotifications.RemoveFirst();
				if (s_PendingNotifications.Count == 0)
				{
					s_PendingNotifications.AddFirst(new AndroidNotification(pkg, name, msg));
					s_PendingNotifications.First.Value.NotificationSent = true;
				}
				break;
			}
			if (s_PendingNotifications.Count == 0)
			{
				s_NotificationTimer.Dispose();
				s_NotificationTimer = null;
			}
		}
		catch(Exception e)
		{
			Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
		}
		s_NotificationLockHelper[1] = false;
	}

	/*
	 * Handles android notifications
	 */
	public static void NotificationHandler(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got NotificationHandler {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		if (!Common.Features.IsFeatureEnabled(Common.Features.ANDROID_NOTIFICATIONS))
		{
			Logger.Info("Android notifications disabled. Not showing.");

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);

			return;
		}

		s_NotificationLockHelper[0] = true;
		s_LockForTurn = 1;
		while(s_NotificationLockHelper[1] && s_LockForTurn == 1)
		{
			Thread.Sleep(100);
		}

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req, false);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();

			Logger.Info("Data");
			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Debug("Key: {0}, Value: {1}", key, requestData.data[key]);

				string jsonString = requestData.data[key];
				JSonReader readjson = new JSonReader();
				IJSonObject infoJson = readjson.ReadAsJSonObject(jsonString);

				string pkg		= infoJson["pkg"].StringValue.Trim();
				string id		= infoJson["id"].StringValue.Trim();
				string content		= infoJson["content"].StringValue.Trim();
				string tickerText	= infoJson["tickerText"].StringValue.Trim();
				content = content.Replace("*bst*", "\n");

				string msg = (content == "" ? tickerText : content);

				String name, image, activity, store;
				if (!JsonParser.GetAppInfoFromPackageName(pkg, out name, out image, out activity, out store))
				{
					Logger.Error("Systray: Notifying app {0} not found!", pkg);
					continue;
				}

				if (pkg == "bn.ereader" ||
						pkg == "com.amazon.venezia" ||
						pkg == "getjar.android.client" ||
						pkg == "me.onemobile.android" ||
						pkg == "com.movend.gamebox" ||
						pkg == "com.android.vending")
				{
					Logger.Info("HTTPHandler: Not showing notification for " + pkg);
					continue;
				}
				else if (content.Contains("%"))
				{
					Logger.Info("HTTPHandler: Not showing notification for {0} because the content seems to show download info", pkg);
					continue;
				}
				else
					Logger.Info("HTTPHandler: Showing notification for " + pkg);

				String	imagePath	= Path.Combine(Common.Strings.GadgetDir, image);

				lock(s_NotificationLock)
				{
					bool pendingNewNotification = false;
					while(s_PendingNotifications.Count > 0 &&
							String.Compare(s_PendingNotifications.Last.Value.AppName, name, true) == 0 &&
							s_PendingNotifications.Last.Value.NotificationSent == false)
					{
						if (!s_PendingNotifications.First.Value.OldNotificationFlag)
							pendingNewNotification = true;
						s_PendingNotifications.RemoveLast();
					}
					if (pendingNewNotification)
					{
						s_PendingNotifications.AddLast(new AndroidNotification(pkg, name, msg));
						s_PendingNotifications.Last.Value.NotificationSent = true;
					}
					s_PendingNotifications.AddLast(new AndroidNotification(pkg, name, msg));

					//SysTray.ShowAndroidNotification(msg, name, pkg, activity, imagePath);
					if (s_PendingNotifications.Count == 1)
					{
						SysTray.ShowInfoShort(name, msg);
						SendGMNotification(pkg, msg);
						s_PendingNotifications.Last.Value.NotificationSent = true;
						if (s_NotificationTimer == null)
						{
							s_NotificationTimer = new System.Threading.Timer(
									SendPendingNotifications,
									null,
									s_NotificationTimeout,
									s_NotificationTimeout);
						}
						else
						{
							s_NotificationTimer.Change(s_NotificationTimeout, s_NotificationTimeout);
						}
					}
					else
					{
						if (s_PendingNotifications.Count > 0 &&
								s_PendingNotifications.First.Value.NotificationSent == true)
							s_PendingNotifications.RemoveFirst();
						while (s_PendingNotifications.Count > 0 &&
								String.Compare(s_PendingNotifications.First.Value.AppName, name, true) != 0)
						{
							SysTray.ShowInfoShort(s_PendingNotifications.First.Value.AppName,
									s_PendingNotifications.First.Value.Message);
							SendGMNotification(s_PendingNotifications.First.Value.Package,
									s_PendingNotifications.First.Value.Message);
							s_PendingNotifications.RemoveFirst();
						}
					}
				}
			}

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server NotificationHandler");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
		s_NotificationLockHelper[0] = false;
	}

	private static void SendGMNotification(string package, string content)
	{
		if (BlueStacks.hyperDroid.Common.Oem.Instance.IsGMNotificationToBePosted)
		{
			try
			{
				int port = Common.Utils.GetPartnerServerPort();

				string url = string.Format("http://127.0.0.1:{0}/{1}",
						port, Common.Strings.GMNotificationUrl);

				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("type", "android");
				data.Add("package", package);
				data.Add("content", content);

				Logger.Info("Sending request to: " + url);
				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in SendGMNotification: " + exc.ToString());
			}
		}
	}

	/*
	 * Checks if a specific package is installed
	 */
	public static void IsAppInstalled(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got IsAppInstalled {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			RequestData requestData = HTTPUtils.ParseRequest(req);
			string vmName = "Android";
			if (requestData.headers["vmid"] != null)
				vmName += "_" + requestData.headers["vmid"].ToString();

			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key, requestData.data[key]);
			}

			string package = requestData.data["package"];
			string version = "Unknown";
			string failReason = "";

			bool installed;

			/*
			 * Adding this call to ensure that BootFailure logs
			 * are reported in case of tencent, as FE is responsible
			 * for sending them.
			 */
			Utils.StartHiddenFrontend(vmName);

			installed = Utils.IsAppInstalled(package, vmName, out version, out failReason);

			if (installed == false &&
					String.IsNullOrEmpty(failReason) == false &&
					String.Compare(failReason, Common.Strings.AppNotInstalledString, true) != 0)
			{
				WriteErrorJson(failReason, res);
				return;
			}

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();

			if (installed)
				json.WriteMember("version", version);

			json.WriteMember("success", true);
			json.WriteMember("installed", installed);

			json.WriteObjectEnd();
			Logger.Info("Sending response: " + json.ToString());
			Common.HTTP.Utils.Write(json.ToString(), res);
			Logger.Info("Sent response");
		} catch (Exception exc) {
			Logger.Error("Exception in Server IsAppInstalled");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	public static void RestartAgent(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got RestartAgent {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		RequestData requestData = HTTPUtils.ParseRequest(req);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		string bstInstallDir = (string)Registry.LocalMachine.OpenSubKey(
				Common.Strings.RegBasePath).GetValue("InstallDir");
		string agentPath = Path.Combine(bstInstallDir, "HD-Agent.exe");

		Logger.Info(string.Format("Agent Path {0}", agentPath));
		string tempFilePath = Path.Combine(Path.GetTempPath(), "BstBatFile.bat");
		Logger.Info(string.Format("Temp File Path {0}", tempFilePath));
		if (!File.Exists(tempFilePath))
		{
			using (FileStream fs = File.Create(tempFilePath))
			{
				fs.Close();
			}
		}	
		Logger.Info(string.Format("Temp File {0} Created", tempFilePath));
		using (StreamWriter sw = new StreamWriter(tempFilePath))
		{
			sw.WriteLine("ping 192.0.2.2 -n 1 -w 100000 > nul");
			sw.WriteLine("call \"" + agentPath + "\"");
		}
		Logger.Info(string.Format("Temp File {0} Data Written", tempFilePath));
		using (Process proc = new Process())
		{
			proc.StartInfo.FileName  = "cmd.exe";
			proc.StartInfo.Arguments  = "/c \"" + tempFilePath + "\"";

			Logger.Info(string.Format("Calling {0} {1}", proc.StartInfo.FileName, proc.StartInfo.Arguments));


			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;

			proc.Start();
		} 
		Logger.Info("Bat File Called");

		JSonWriter json = new JSonWriter();
		json.WriteObjectBegin();
		json.WriteMember("success", true);
		json.WriteObjectEnd();
		Common.HTTP.Utils.Write(json.ToString(), res);

		Environment.Exit(0);
	}

	public static void SystrayVisibility(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got SystrayVisibility {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try
		{
			RequestData requestData = HTTPUtils.ParseRequest(req);
			Common.Strings.VMName = "Android";
			if (requestData.headers["vmid"] != null)
				Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
			string visibility	= requestData.data["visible"];
			if (string.Compare(visibility, "true") == 0)
				SysTray.SetTrayIconVisibility(true);
			else
				SysTray.SetTrayIconVisibility(false);
		}
		catch (Exception exc)
		{
			Logger.Error("Exception in Server SystrayVisibility");
			Logger.Error(exc.ToString());
		}
	}

	public static void TopActivityInfo(HttpListenerRequest req,
			HttpListenerResponse res)
	{
		/*
		 * Return if frontend is not running
		 * Log file is being flooded by these logs when the frontend is not running
		 * because the home app keeps crashing and getting restarted
		 * This method only sends app data to the frontend

		*/
		RequestData requestData = HTTPUtils.ParseRequest(req);
		string vmName = "Android";
		if (requestData.headers["vmid"] != null)
			vmName += "_" + requestData.headers["vmid"].ToString();
		if (!Utils.IsUIProcessAlive())
		{
			return;
		}
		Logger.Info("HTTPHandler: Got TopActivityInfo {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());

		try {
			Logger.Info("Data");

			foreach (string key in requestData.data.AllKeys)
			{
				Logger.Info("Key: {0}, Value: {1}", key,
						requestData.data[key]);
			}

			string package		= requestData.data["packageName"];
			string activity		= requestData.data["activityName"];
			string callingPackage	= requestData.data["callingPackage"];

			Logger.Info("packageName = {0}, activityName = {1}",
					package, activity);

			TimelineStatsSender.HandleTopActivityInfo(requestData);

			SendAppDataToFE(package, activity, callingPackage, vmName);

			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();
			json.WriteMember("success", true);
			json.WriteObjectEnd();
			Common.HTTP.Utils.Write(json.ToString(), res);
		} catch (Exception exc) {
			Logger.Error("Exception in Server TopActivityInfo");
			Logger.Error(exc.ToString());
			WriteErrorJson(exc.Message, res);
		}
	}

	private static string CheckForJsonp(String json, HttpListenerRequest req)
	{

		RequestData requestData = HTTPUtils.ParseRequest(req, false);
		Common.Strings.VMName = "Android";
		if (requestData.headers["vmid"] != null)
			Common.Strings.VMName += "_" + requestData.headers["vmid"].ToString();
		String callback = requestData.queryString["callback"];
		if (callback == null)
			return json;
		else
			return String.Format("{0}({1});", callback, json);
	}

	public static void RestartGameManager(HttpListenerRequest req, HttpListenerResponse res)
	{
		Logger.Info("HTTPHandler: Got Restart {0} request from {1}",
				req.HttpMethod, req.RemoteEndPoint.ToString());
		JSonWriter json = new JSonWriter();
		json.WriteArrayBegin();
		json.WriteObjectBegin();
		json.WriteMember("success", true);
		json.WriteObjectEnd();
		json.WriteArrayEnd();
		Common.HTTP.Utils.Write(json.ToString(), res);
		string processName = Path.GetFileNameWithoutExtension(Utils.GetPartnerExecutablePath());
		Logger.Info("Aman Process name " + processName);
		Thread t = new Thread(delegate() {
				while (Utils.FindProcessByName(processName))
				{
				Thread.Sleep(200);
				}
				Utils.StartExe(Utils.GetPartnerExecutablePath());
				});
		t.IsBackground = true;
		t.Start();
	}
	private static string	s_AppsDotJsonFile	= Path.Combine(Common.Strings.GadgetDir, "apps.json");
	private static string	s_AppStoresDir		= Path.Combine(Common.Strings.LibraryDir, Common.Strings.StoreAppsDir);
	private static string	s_MyAppsDir		= Path.Combine(Common.Strings.LibraryDir, Common.Strings.MyAppsDir);
	private static string	s_IconsDir		= Path.Combine(Common.Strings.LibraryDir, Common.Strings.IconsDir) + "\\";
	private static string	s_IconFile		= Path.Combine(HDAgent.s_InstallDir, "BlueStacks.ico");

	private static string	s_Png2ico		= Path.Combine(HDAgent.s_InstallDir, "HD-png2ico.exe");

	public static Object	s_sync			= new Object();
	private static string	s_CloudRegKey		= Common.Strings.CloudRegKeyPath;
}
}
