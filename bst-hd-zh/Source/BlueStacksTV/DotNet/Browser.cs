using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using Gecko;
using Gecko.Events;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms;
using System.Windows.Forms.Integration;
using System.Windows.Media;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class Browser : GeckoWebBrowser
	{
		private static string sProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

		private static string sInstallDir = StreamWindowUtility.BluestacksGameManager;
		public static string sHomeDataDir = Path.Combine(sInstallDir, @"UserData\Home");
		public static bool sUserInput = false;

		public string mUrl;
		private string mFailedUrl;

		private bool mUrlSet = false;
		private string tabName = "";
		private int progressPercentage = 0;
		private string mfileDownloadPath = "";

		public WebClient webClient = null;
		public Downloader mDownloader = null;
		private object mFilterWindowLock = new object();

		static Browser()
		{
			CultureInfo ci = Thread.CurrentThread.CurrentCulture;
			string lang = ci.Name;

			Gecko.Xpcom.ProfileDirectory = Path.Combine(sInstallDir, @"UserData\Cache");
			Gecko.Xpcom.Initialize(Path.Combine(sInstallDir, @"xulrunner-sdk"));
			string sUserAgent = string.Format("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0 lang/{0} BlueStacks/{1}", lang, Version.STRING);
			Gecko.GeckoPreferences.User["general.useragent.override"] = sUserAgent;
			Gecko.GeckoPreferences.User["dom.disable_beforeunload"] = true;
			GeckoPreferences.User["browser.xul.error_pages.enabled"] = true;
			GeckoPreferences.Default["extensions.blocklist.enabled"] = false;
			Gecko.GeckoPreferences.User.SetBoolPref("browser.cache.disk.enable", true);
			Gecko.GeckoPreferences.User.SetBoolPref("browser.cache.memory.enable", true);
			GeckoPreferences.User["Browser.cache.check doc frequency"] = 3;
			GeckoPreferences.User["Browser.cache.disk.capacity"] = 50000;
			GeckoPreferences.User["Browser.cache.memory.capacity()"] = -1;
			GeckoPreferences.Default["extensions.blocklist.enabled"] = false;
			GeckoPreferences.Default["general.useragent.locale"] = lang;
			if (lang != null && !lang.ToLower().Equals("en-us"))
				GeckoPreferences.User["intl.accept_languages"] = lang.ToLower() + ",en-us,en";
			GeckoPreferences.User["full-screen-api.enabled"] = true;
			GeckoPreferences.User["gfx.font_rendering.graphite.enabled"] = true;

			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
			int devEnv = (int)prodKey.GetValue("BTVDevEnv", 0);
			if (devEnv != 0)
				StartDebugServer();

			int obsDevEnv = (int) prodKey.GetValue("OBSDevEnv", 0);
			if (obsDevEnv > 0)
			{
				StreamWindowUtility.sOBSDevEnv = true;
			}
		}

		public void DialogClickHandler(string jsonString)
		{
			JSonReader jsonReader = new JSonReader();
			IJSonObject obj = jsonReader.ReadAsJSonObject(jsonString);

			bool isAppView = false;
			if (obj.Contains("isAppView"))
				isAppView = obj["isAppView"].BooleanValue;
			LayoutWindow.Instance.ChangeLayout(obj["layoutTheme"].ToString(), isAppView);
		}

		public void CloseDialog(string jsonString)
		{
			ReInitSideBarVideo();
			if (!StreamManager.Instance.mIsStreaming)
				StreamWindowUtility.ReParentOBSWindow();
			LayoutWindow.Instance.CloseDialog(jsonString);
			LayoutWindow.Instance = null;
		}

		public void ReInitSideBarVideo()
		{
			object[] args = { };
			StreamWindow.Instance.mBrowser.CallJs("closeFilter", args);
		}

		private static void RegisterChromeDir(string dir)
		{
			nsIFile chromeDir = (nsIFile)Xpcom.NewNativeLocalFile(dir);
			nsIFile chromeFile = chromeDir.Clone();
			chromeFile.Append(new nsAString("chrome.manifest"));
			Xpcom.ComponentRegistrar.AutoRegister(chromeFile);
			Xpcom.ComponentManager.AddBootstrappedManifestLocation(chromeDir);
		}

		private static void StartDebugServer()
		{
			GeckoPreferences.User["devtools.debugger.remote-enabled"] = true;

			//see <geckofx_src>/chrome dir
			RegisterChromeDir(Path.GetFullPath(Path.Combine(sInstallDir, @"chrome")));

			/*Can be used to if want to hide Debug Server Mode On Tab 

			var browser = new GeckoWebBrowser();
			browser.NavigationError += (s, e) =>
			{
				Console.Error.WriteLine("StartDebugServer error: 0x" + e.ErrorCode.ToString("X"));
				browser.Dispose();
			};
			browser.DocumentCompleted += (s, e) =>
			{
				Console.WriteLine("StartDebugServer completed");
				browser.Dispose();
			};
			browser.Navigate("chrome://geckofx/content/debugger-server.html");
			*/

			Window w = new Window();
			Grid stackPanel = new Grid();
			WindowsFormsHost wfh = new WindowsFormsHost();
			Browser bro = new Browser("chrome://geckofx/content/debugger-server.html");
			wfh.Child = bro;
			bro.Navigate("chrome://geckofx/content/debugger-server.html");
			stackPanel.Children.Add(wfh);
			w.Content = stackPanel;
			w.Show();
			//see <geckofx_src>/chrome/debugger-server.html
			//TabButtons.Instance.AddWebTab("Debug Server Mode On", "chrome://geckofx/content/debugger-server.html", null, true);

			//browser.Navigate("chrome://geckofx/content/debugger-server.html");
		}

		public Browser(string url)
		{
			mUrl = url;

			this.Navigate("about:blank");
			this.Name = "mWebBrowserHome";
			this.TabIndex = 0;
			this.NoDefaultContextMenu = true;
			this.Focus();
			this.AddMessageEventListener("MessageEvent", this.ReceiveJSFunctionCall);
			this.NavigationError += this.NavigateErrorHandler;
			this.CreateWindow += this.BrowserCreateWindow;

			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
			int devEnv = (int)prodKey.GetValue("DevEnv", 0);
			if (devEnv == 0)
			{
				Logger.Info("Live build detected. Supressing js warnings");
				this.NoDefaultContextMenu = true;
			}
		}

		private void BrowserCreateWindow(object sender, GeckoCreateWindowEventArgs e)
		{
			e.InitialWidth = this.Width / 2;
			e.InitialHeight = this.Height / 2;
		}

		public string inTabBar()
		{
			return "false";
		}

		void ReceiveJSFunctionCall(string dataStr)
		{
			JSonReader readjson = new JSonReader();
			IJSonObject fullJson = readjson.ReadAsJSonObject(dataStr);
			IJSonObject dataObject = fullJson["data"];

			object[] argsArray = null;
			if (dataObject.ToString() != null)
			{
				argsArray = new object[dataObject.Length];
				for (int i = 0; i < dataObject.Length; i++)
				{
					argsArray[i] = (object)dataObject[i].ToString();
				}
			}

			try
			{
				object response = this.GetType().GetMethod(fullJson["calledFunction"].ToString()).Invoke(this, argsArray);
				if (fullJson["callbackFunction"].ToString() != null)
				{
					string script;
					if (response == null)
						script = fullJson["callbackFunction"].ToString() + "()";
					else
					{
						Logger.Info("response: " + response.ToString());
						script = fullJson["callbackFunction"].ToString() + "('" +
							response.ToString().Replace("'", "&#39;").Replace("%27", "&#39;") + "')";
					}

					using (Gecko.AutoJSContext runner = new Gecko.AutoJSContext(this.Window.JSContext))
					{
						runner.PushCompartmentScope((nsISupports)this.Window.DomWindow);
						/*
						 * EvaluateScript executes JavaScript callbacks without full security privileges.
						 * This causes certain APIs, such as console.log() to fail, throwing a security exception.
						 * EvaluateScriptBypassingSomeSecurityRestrictions performs an aynchronous
						 * dispatch of the callback, and this seems to resolve the issue
						 * A later version of GeckoFX might take care of this, but for now
						 * we will need to use this method
						 */
						runner.EvaluateScriptBypassingSomeSecurityRestrictions(script);
					}
				}
			}
			catch (Exception e)
			{
				Logger.Info("Error in ReceiveJSFunctionCall: " + e.ToString());
			}

		}

		public void EvaluateJS(string script)
		{
			using (Gecko.AutoJSContext runner = new Gecko.AutoJSContext(this.Window.JSContext))
			{
				runner.PushCompartmentScope((nsISupports)this.Window.DomWindow);
				runner.EvaluateScriptBypassingSomeSecurityRestrictions(script);
			}
		}

		public void LoadUrl(string url)
		{
			mUrl = url;
			this.Navigate(url);
		}

		public void LoadUrl()
		{
			this.Navigate(mUrl);
		}

		public string getUserName()
		{
			return StreamWindowUtility.getUserName();
		}

		public void setUserName(Object param)
		{
			string name = param.ToString();
			StreamWindowUtility.setUserName(name);
		}

		public string getInterests()
		{
			return StreamWindowUtility.getInterests();
		}

		public void setInterests(Object param)
		{
			string name = param.ToString();
			StreamWindowUtility.setInterests(name);
		}


		public string GetCurrentAppInfo()
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "getcurrentappinfo");
				string resp = Common.HTTP.Client.Get(url, null, false);

				JSonReader reader = new JSonReader();
				IJSonObject obj = reader.ReadAsJSonObject(resp);
				string type = obj["type"].ToString();
				string name = obj["name"].ToString();
				string data = obj["data"].ToString();

				JSonWriter info = new JSonWriter();
				info.WriteObjectBegin();
				info.WriteMember("type", type);
				info.WriteMember("name", name);
				info.WriteMember("data", data);
				info.WriteObjectEnd();
				return info.ToString();
			}
			catch (Exception ex)
			{
				Logger.Error("Error in GetCurrentAppInfo... Err : " + ex.ToString());
			}
			return null;
		}

		public void LaunchFilterWindow(string channel, string sessionId)
		{
			lock (mFilterWindowLock)
			{
				if (FilterWindow.Instance != null)
					return;

				bool showFilterScreen = false;
				string appPkg = FilterUtility.GetCurrentAppPkg();
				if (appPkg != null)
				{
					FilterWindow.sCurrentAppPkg = appPkg;
					if (FilterUtility.IsFilterApplicableApp(appPkg))
					{
						string currentTheme = FilterUtility.GetCurrentTheme(appPkg);
						if (currentTheme == null)
						{
							//TODO if needed
						}

						if (!StreamManager.Instance.mCLRBrowserRunning)
							StreamManager.Instance.InitCLRBrowser(appPkg, currentTheme);

						showFilterScreen = true;
					}
				}
				StreamWindowUtility.UnSetOBSParentWindow();
				FilterWindow.Instance = new FilterWindow();
				FilterWindow.Instance.Setup(channel, sessionId, showFilterScreen);
				FilterWindow.Instance.ShowDialog();// to be done later
			}
		}
		public void ChangeFilterTheme(string theme)
		{
			string appPkg = FilterWindow.sCurrentAppPkg;
			FilterThemeConfig filterThemeConfig = StreamManager.Instance.ChangeTheme(appPkg, theme);

			if (filterThemeConfig != null)
			{
				object[] args = { filterThemeConfig.mFilterThemeSettings.ToJsonString() };
				this.CallJs("setSettings", args);
			}
		}

		public void UpdateThemeSettings(string currentTheme, string settingsJson)
		{
			string appPkg = FilterWindow.sCurrentAppPkg;
			FilterThemeConfig filterThemeConfig = FilterUtility.GetFilterThemeConfig(appPkg, currentTheme);
			filterThemeConfig.mFilterThemeSettings = new FilterThemeSettings(settingsJson);

			IJSonReader jsonReader = new JSonReader();
			IJSonObject obj = jsonReader.ReadAsJSonObject(settingsJson);
			bool turnOnWebCam = obj["webcam"].StringValue.ToLower().Equals("true");

			if (turnOnWebCam != filterThemeConfig.mFilterThemeSettings.mIsWebCamOn)
			{
				if (turnOnWebCam)
				{
					StreamManager.Instance.SetAndEnableWebCamPosition(filterThemeConfig);
				}
				else
				{
					StreamManager.Instance.DisableWebcamV2("{}");
				}
				StreamWindow.Instance.ChangeWebCamState();
			}

			RegistryKey filterKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath + @"\" + appPkg, true);
			filterKey.SetValue(currentTheme, filterThemeConfig.ToJsonString(), RegistryValueKind.String);
			filterKey.Close();

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("settings", settingsJson);
			FilterUtility.SendRequestToCLRBrowser("updatesettings", data);
		}
		public void CloseFilterWindow(string jsonArray)
		{
			if (FilterWindow.Instance == null)
			{
				Logger.Error("CloseFilterWindow called without initializing it. Ignoring...");
				return;
			}
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				ReInitSideBarVideo();
				if (!StreamManager.Instance.mIsStreaming)
					StreamWindowUtility.ReParentOBSWindow();
				FilterWindow.Instance.CloseFilterWindow(jsonArray);
				FilterWindow.Instance = null;

				//Thread.Sleep(200);

			}));
		}

		public string GetAppInfo(string package)
		{
			JSonWriter json = new JSonWriter();
			json.WriteObjectBegin();

			string name, img, activity, version;
			GMAppsManager iam = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
			bool isInstalled = iam.GetAppInfoFromPackageName(package, out name, out img, out activity, out version);
			json.WriteMember("installed", isInstalled);
			if (isInstalled)
			{
				json.WriteMember("name", name);
				json.WriteMember("activity", activity);
				json.WriteMember("img", img);
				json.WriteMember("version", version);
			}

			json.WriteObjectEnd();
			return json.ToString();
		}

		public void SendOBSRequest(string request, string jsonString)
		{
			JSonReader readjson = new JSonReader();
			IJSonObject fullJson = readjson.ReadAsJSonObject(jsonString);

			if (jsonString != null)
			{
				Dictionary<string, string> dataDictionary = new Dictionary<string, string>();
				foreach (string name in fullJson.Names)
				{
					dataDictionary.Add(name, fullJson[name].ToString());
				}
				StreamManager.Instance.SendObsRequest(request, dataDictionary, null, null, 0);
			}
			else
				StreamManager.Instance.SendObsRequest(request, null, null, null, 0);

		}

		public void showWebPage(string title, string webUrl)
		{
			Logger.Info("sending web app stats for " + title);
			Common.Stats.SendAppStats(title, title, BlueStacks.hyperDroid.Version.STRING, "", Common.Stats.AppType.web);

			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, "showwebpage");
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("title", title);
				data.Add("url", webUrl);

				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to open web page... Err : " + ex.ToString());
			}
		}

		public void StartStreamViewStatsRecorder(string label, string jsonString)
		{
			StreamWindowUtility.AddNewStreamViewKey(label, jsonString);
		}


		public void ShowWebPageInBrowser(string url)
		{
			Logger.Info("Showing " + url + " in default browser");
			Process.Start(url);
		}

		public void ReportProblem()
		{
			StreamWindowUtility.ReportProblem();
		}

		public void ExecuteExe(string path, string args)
		{
			Process proc = new Process();
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.FileName = path;
			proc.StartInfo.Arguments = args;

			Logger.Info("Running: {0} {1}", path, args);

			proc.Start();
			proc.WaitForExit();
		}

		public void InstallOnDevice(string pkgName)
		{
			Logger.Info("Called installOnDevice for: " + pkgName);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String installDir = (string)key.GetValue("InstallDir");
			string adbFilePath = Path.Combine(installDir, "HD-Adb.exe");
			string args = "connect 127.0.0.1:5555";

			ExecuteExe(adbFilePath, args);

			args = "-s 127.0.0.1:5555 shell am startservice -a com.bluestacks.s2p.INSTALL_ON_DEVICE -e pkg " + pkgName;

			ExecuteExe(adbFilePath, args);
		}

		public void CallJs(String methodName, object[] args)
		{
			Thread thread = new Thread(delegate ()
			{
				try
				{
					if (args.Length == 1)
					{
						string arg = args[0].ToString();
						arg = arg.Replace("%27", "&#39;").Replace("'", "&#39;");
						string cmd = String.Format("javascript:{0}('{1}')", methodName, arg);
						Logger.Info("calling " + methodName);
						this.Navigate(cmd);
					}
					else if (args.Length == 0)
					{
						string cmd = String.Format("javascript:{0}()", methodName);
						Logger.Info("calling " + methodName);
						this.Navigate(cmd);
					}
					else
						Logger.Error("Error: function supported for one length array object to be changed later");
				}
				catch (Exception e)
				{
					Logger.Error(e.ToString());
				}
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public void makeWebCall(string url, string scriptToInvoke)
		{

			HttpWebRequest req = WebRequest.Create(url) as HttpWebRequest;
			req.Method = "GET";

			req.AutomaticDecompression = DecompressionMethods.GZip;
			req.Headers.Add(HttpRequestHeader.AcceptEncoding, "gzip");

			req.UserAgent = "Mozilla/5.0 (compatible; MSIE 8.0; Windows NT 6.1; Trident/4.0; GTB7.4; InfoPath.2; SV1; .NET CLR 3.3.69573; WOW64; en-US)";
			Uri u = new Uri(url);

			try
			{
				Logger.Info("making a webcall at url=" + url);

				String ret = null;

				using (HttpWebResponse res = req.GetResponse() as HttpWebResponse)
				{
					using (Stream s = res.GetResponseStream())
					{
						using (StreamReader r = new StreamReader(s, Encoding.UTF8))
						{
							ret = r.ReadToEnd();
						}
					}
				}
				object[] args = { ret };

				CallJs(scriptToInvoke, args);
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				string empty = "";
				object[] args = { empty };
				CallJs(scriptToInvoke, args);
			}
			return;
		}

		public void reloadFailedUrl()
		{
			Logger.Info("in reloadFailedUrl()");
			Logger.Info("Will navigate to failed url " + mFailedUrl);
			this.Navigate(mFailedUrl);
		}

		public void showMyAppsLocal()
		{
			Logger.Info("in showMyAppsLocal()");
			this.Navigate(StreamWindowUtility.sLocalMyAppsHtml);
		}

		//public void showMyAccount()
		//{
		//    Logger.Info("opening my account page");
		//    string myAccountUrl = "https://bluestacks-cloud.appspot.com/myaccount";
		//    myAccountUrl += "?guid=" + User.GUID;
		//    string label = "My Accounts";
		//    TabButtons.Instance.ShowWebPage(label, myAccountUrl, null);
		//}

		public void launchHelpApp()
		{
			LaunchApp(
					"Help",
					"com.bluestacks.help",
					"com.bluestacks.help.com.bluestacks.help.HelpActivity");
		}

		public void launchLanguageInputApp()
		{
			LaunchApp(
					"Input Settings",
					"com.android.settings",
					"com.android.settings.LanguageSettings");
		}

		private void LaunchApp(string title, string package, string activity)
		{
			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}", App.sApplicationServerPort, Common.Strings.ShowAppUrl);
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("title", title);
				data.Add("package", package);
				data.Add("activity", activity);
				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception e)
			{
				Logger.Error("Failed to launch app {0}... Err : {1}", title, e.ToString());
			}
		}

		private void NavigateErrorHandler(object sender, GeckoNavigationErrorEventArgs e)
		{
			Logger.Info(e.ToString());
			Logger.Info("Cannot navigate to " + e.Uri.ToString());
			Logger.Info("StatusCode: " + e.ErrorCode);
			//TopBar.Instance.mLoadingButton.Visibility = Visibility.Hidden;

			/*
			if (e.StatusCode == 200 || e.StatusCode == 204 || e.StatusCode == 404)
				return;
			*/

			mFailedUrl = mUrl;
			if (mFailedUrl != StreamWindowUtility.sNoWifiHtml && e.ErrorCode.ToString() == GeckoError.NS_ERROR_UNKNOWN_HOST.ToString())
			{
				Logger.Info("Set mFailedUrl to " + mFailedUrl);
				if (StreamWindowUtility.sIsOBSReParented)
					StreamWindowUtility.UnSetOBSParentWindow();
				this.Navigate(StreamWindowUtility.sNoWifiHtml);
			}
		}

		public void ShowWaitPage(string progressStyle, string message)
		{
			string waitUrl = StreamWindowUtility.sWaitHtml;
			string param = String.Format(@"?style={0}&message={1}", progressStyle, message);
			waitUrl = waitUrl + param; //HttpUtility.UrlEncode(param);
			this.Navigate(waitUrl);
		}

		private void HideProgressBar()
		{
			object[] args = { };
			CallJs("hideProgress", args);
		}

		public void SetProgressBarText(string text)
		{
			object[] args = { text };
			CallJs("setMessage", args);
		}

		public void SetProgressBarStyle(string style)
		{
			object[] args = { style };
			CallJs("setStyle", args);
		}

		public void SetFileDownloadProgress(string percent)
		{
			object[] args = { percent };
			CallJs("setProgress", args);
		}

		public void HideModalAlert()
		{
			object[] args = { };
			this.Navigate("javascript:gmHideAlert()");
		}

		public void SendSearchStats(string searchValue)
		{
			Logger.Info("In SendSearchStats, search value = {0}", searchValue);
			Thread searchStatsSender = new Thread(delegate ()
			{
				Common.Stats.SendSearchAppStats(searchValue);
			});
			searchStatsSender.IsBackground = true;
			searchStatsSender.Start();
		}

		public string GetLocaleName()
		{
			return StreamWindowUtility.GetLocaleName();
		}

		public string GetAvailableLocaleName()
		{
			return StreamWindowUtility.GetAvailableLocaleName();
		}
		public string GetCurrentTheme()
		{
			Logger.Info("in GetCurrentTheme()");
			string currentTheme = StreamWindowUtility.getCurrentTheme();
			Logger.Info("current Theme = " + currentTheme);
			return currentTheme;
		}

		//needs to be theme-specific
		public void LaunchHome()
		{
			//string homeUrl = this.Navigate(GameManager.sGameManager.mGameManagerUrls[(int)GameManager.GameManagerStage.HOME]);
			this.Navigate(StreamWindowUtility.getCurrentThemeHomeUrl());
		}

		//needs to be theme specific
		public void LaunchSearch(string searchString)
		{
			//get the search_result url for the current theme
			string searchUrl = StreamWindowUtility.getCurrentThemeSearchUrl() + "?searchstring=" + searchString;
			Logger.Info("searchUrl = " + searchUrl);
			this.Navigate(searchUrl);
		}

		public void LaunchThemesPage()
		{
			this.Navigate(StreamWindowUtility.getCurrentThemeThemesUrl());
		}

		public string GetChannelNamesJson()
		{
			return StreamWindowUtility.GetChannelNamesJson();
		}

		public string GetChannelAppsJson(string channelId, string subCategory)
		{
			return StreamWindowUtility.GetChannelAppsJson(channelId, subCategory);
		}

		public string GetChannelAppsJsonWithHTMLClass(string htmlClass, string channelId, string subCategory)
		{
			return "{\"html_class\":\"" + htmlClass + "\", \"apps\":" + StreamWindowUtility.GetChannelAppsJson(channelId, subCategory) + "}";
		}

		public string searchAppHandler(string searchString)
		{
			this.SendSearchStats(searchString);

			JSonReader readjson = new JSonReader();
			IJSonObject fullJson = readjson.ReadAsJSonObject(this.GetInstalledAppsJson(false).Replace("'", "&#39;"));

			string installed_apps_json_string = fullJson.ToString();
			string channel_apps_json_string = StreamWindowUtility.GetChannelAppsJson().Replace("'", "&#39;").Replace("\r\n", "");

			return "{\"search_string\":\"" + searchString + "\",\"installed_apps\":" + installed_apps_json_string + ",\"channel_apps\":" + channel_apps_json_string + "}";
		}

		public string GetInstalledAppsJsonforJS()
		{
			return GetInstalledAppsJson(false);
		}

		private String GetInstalledAppsJson(bool escapeQuotes)
		{
			string appsJson = Path.Combine(sHomeDataDir, StreamWindowUtility.sInstalledAppsJson);
			StreamReader jsonFile = new StreamReader(appsJson);
			string fileData = jsonFile.ReadToEnd();
			jsonFile.Close();

			if (escapeQuotes)
			{
				fileData = fileData.Replace("\"", "\\\"");
			}

			fileData = fileData.Replace("\n", "");
			fileData = fileData.Replace("\r", "");
			fileData = Regex.Replace(fileData, @"\s+", " ", RegexOptions.Multiline);

			return fileData;
		}
		public bool isBluestacksInstalled()
		{
			// In some legacy code we used this method to chceck if bluestacks is installer.
			// Now it is always true. Just to make sure the functionality does not break
			// when it is called from any webapi not deleting this method.
			return true;
		}

		public string GetUserGUID()
		{
			return User.GUID;
		}

		public string GetSystemRAM()
		{
			return Device.Profile.RAM;
		}

		public string GetSystemCPU()
		{
			return Device.Profile.CPU;
		}

		public string GetSystemGPU()
		{
			return Device.Profile.GPU;
		}

		public string GetSystemOS()
		{
			return Device.Profile.OS;
		}

		public string GetClientId()
		{
			string guid = User.GUID;
			string key = "_BSTK_";
			string timestamp = DateTime.Now.ToString();
			string id = String.Format("{0}{1}{2}", guid, key, timestamp);
			id = Utils.GetMD5HashFromString(id);

			JSonWriter info = new JSonWriter();
			info.WriteObjectBegin();
			info.WriteMember("guid", guid);
			info.WriteMember("name", timestamp);
			info.WriteMember("id", id);
			info.WriteObjectEnd();

			return info.ToString();
		}

		public string GetGMPort()
		{
			Logger.Info("In GetGMPort");
			return App.sApplicationServerPort.ToString();
		}

		public void ShowStreamWindow()
		{
			StreamWindowUtility.ShowStreamWindow();
		}

		public void ShowViewTab()
		{
			//TabButtons.Instance.ShowViewTab();
			//to be done later
		}

		public void LogInfo(string info)
		{
			Logger.Info("HtmlLog: " + info);
		}

		public void CreateNewBrowserWindow(string title, string url, string widthStr, string heightStr)
		{
			CreateNewBrowserWindowV2(title, url, widthStr, heightStr, "true", "true");
		}

		public void CreateNewBrowserWindowV2(string title, string url, string widthStr, string heightStr,
				string enableMaximize, string enableMinimize)
		{
			int width = Convert.ToInt32(widthStr);
			int height = Convert.ToInt32(heightStr);
			bool maximize = Convert.ToBoolean(enableMaximize);
			bool minimize = Convert.ToBoolean(enableMinimize);

			Form newForm = new Form();
			newForm.Text = title;
			newForm.ClientSize = new System.Drawing.Size(width, height);
			newForm.Icon = Utils.GetApplicationIcon();
			newForm.MaximizeBox = maximize;
			newForm.MinimizeBox = minimize;

			if (!maximize)
				newForm.FormBorderStyle = FormBorderStyle.FixedSingle;

			Browser browser = new Browser(url);
			browser.Size = newForm.ClientSize;
			newForm.Controls.Add(browser);
			newForm.Resize += delegate (Object sender, EventArgs e)
			{
				browser.Size = newForm.ClientSize;
			};
			newForm.Show();
			browser.Navigate(url);
		}

		public void SetClipboardText(string text)
		{
			Logger.Info("ClipboardText: {0}", text);
			System.Windows.Forms.Clipboard.SetText(text);
		}

		public bool ApiExists(Object api)
		{
			bool result = false;
			string methodName = api.ToString();
			try
			{
				var type = GetType();
				result = type.GetMethod(methodName) != null;
			}
			catch (System.Reflection.AmbiguousMatchException)
			{
				//More than one match
				result = true;
			}
			Logger.Info("Api {0} exists: {1}", methodName, result);
			return result;
		}

		public bool IsGuestBooted()
		{
			Logger.Info("Received request for IsGuestBooted");
			return Common.Utils.IsGuestBooted();
		}
	}
}
