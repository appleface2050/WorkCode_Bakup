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
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
    public class Browser : GeckoWebBrowser
    {
		public PopupWindow mParentWindow = null;
        private static String sProgramData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);

        private static String sInstallDir = GameManagerUtilities.BluestacksGameManager;
        public static String sHomeDataDir = Path.Combine(sInstallDir, @"UserData\Home");
        public static bool sUserInput = false;

        public String mUrl;
        private String mFailedUrl;
        private object mFilterWindowLock = new object();

        private bool mUrlSet = false;
        private string tabName = "";
        private int parentTabIndex = 0;
        private int progressPercentage = 0;
        private string mfileDownloadPath = "";
        private DateTime mLastCheckTime;

        public WebClient webClient = null;
        public TabButton mParentTab;
        public Downloader mDownloader = null;
        public object mDownloadUpdateLock = new object();
        public bool mDocumentLoadCompleted = false;

        static Browser()
        {
            CultureInfo ci = Thread.CurrentThread.CurrentCulture;
            string lang = ci.Name;

            Gecko.Xpcom.ProfileDirectory = Path.Combine(sInstallDir, @"UserData\Cache");
            Gecko.Xpcom.Initialize(Path.Combine(sInstallDir, @"xulrunner-sdk"));
            string sUserAgent = String.Format("Mozilla/5.0 (Windows NT 6.1; WOW64; rv:39.0) Gecko/20100101 Firefox/39.0 lang/{0} BlueStacks/{1}", lang, Version.STRING);
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
            int devEnv = (int)prodKey.GetValue("DevEnv", 0);
            if (devEnv != 0)
            {
                StartDebugServer();
            }

            int obsDevEnv = (int)prodKey.GetValue("OBSDevEnv", 0);
            if (obsDevEnv != 0)
            {
                GameManagerUtilities.sOBSDevEnv = true;
            }
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

            //see <geckofx_src>/chrome/debugger-server.html
            TabButtons.Instance.AddWebTab("Debug Server Mode On", "chrome://geckofx/content/debugger-server.html", null, true);

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
            this.DocumentTitleChanged += this.DocumentTitleChangedHandler;
            this.DocumentCompleted += this.DocumentCompletedHandler;
            this.NavigationError += this.NavigateErrorHandler;
            this.Navigating += this.OnWebBrowserNavigating;
            this.CreateWindow += this.BrowserCreateWindow;

            LauncherDialog.Download += this.LauncherDialog_Download;

            RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
            int devEnv = (int)prodKey.GetValue("DevEnv", 0);
            if (devEnv == 0)
            {
				Logger.Info("Live build detected. Supressing js warnings");
				this.NoDefaultContextMenu = true;
			}
		}

        private void OnWebBrowserNavigating(object sender, GeckoNavigatingEventArgs e)
        {
            Logger.Info("Navigating to: {0}", e.Uri.Host);
        }

        private void BrowserCreateWindow(object sender, GeckoCreateWindowEventArgs e)
        {
            if (e.Flags == GeckoWindowFlags.All)
            {
                string url = e.Uri.ToString();
                string label = "Popup Window";
                if (url.StartsWith("http://"))
                {
                    string[] words = url.Split('/');
                    label = words[2];
                }
                else if (url.StartsWith("https://"))
                {
                    string[] words = url.Split('/');
                    label = words[2];
                }

                e.WebBrowser = TabButtons.Instance.AddWebTab(label, url, null, true).mBrowser;

            }
            else
            {
                e.InitialWidth = this.Width / 2;
                e.InitialHeight = this.Height / 2;
            }

        }

        public void AddDownloadHandler(string url, string tabName)
        {
            progressPercentage = 0;

            this.tabName = tabName;

            string fileName = Path.GetRandomFileName();
            string filePath = Path.Combine(GameManagerUtilities.SetupDir, fileName);

            webClient = new WebClient();
            mfileDownloadPath = filePath;
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(DownloadCompleted);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            webClient.DownloadFileAsync(new Uri(url), filePath);
        }

        public void AddAllDownloadHandler(string url, string filePath, string tabName)
        {
            progressPercentage = 0;

            this.tabName = tabName;

            webClient = new WebClient();
            mfileDownloadPath = filePath;
            webClient.DownloadFileCompleted += new AsyncCompletedEventHandler(AllDownloadCompleted);
            webClient.DownloadProgressChanged += new DownloadProgressChangedEventHandler(DownloadProgressChanged);
            webClient.DownloadFileAsync(new Uri(url), filePath);
        }

        private void DownloadProgressChanged(object sender, DownloadProgressChangedEventArgs e)
        {
            int timeSplit = 3;
            if (this.progressPercentage + timeSplit <= e.ProgressPercentage)
            {
                this.progressPercentage = e.ProgressPercentage;
                this.SetFileDownloadProgress(e.ProgressPercentage.ToString());
            }
        }

        private void DownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            SetProgressBarText("Installing app...");
            SetProgressBarStyle("marquee");
            InstallApk(mfileDownloadPath, this.tabName);
            mfileDownloadPath = "";
        }

        private void AllDownloadCompleted(object sender, AsyncCompletedEventArgs e)
        {
            if (e.Cancelled)
                return;
            SetProgressBarText("Download Completed...");
            TabButtons.Instance.CloseTab(this.tabName);
        }

        void LauncherDialog_Download(object sender, LauncherDialogEvent e)
        {
            if (TabButtons.Instance.SelectedTab.mBrowser != this)
            {
                return;
            }
            nsACString mimeType = new nsACString("");
            e.Mime.GetMIMETypeAttribute(mimeType);

            //? is used once to defferentiate querystring and url
            string hostname = null;

            try
            {
                hostname = e.Url.Split('?')[0];
            }
            catch (Exception ex)
            {
                Logger.Info(ex.Message);
            }

            if (mimeType.ToString() == "application/vnd.android.package-archive" || hostname.EndsWith(".apk") || e.Filename.EndsWith(".apk"))
            {
                string param = String.Format(@"?style={0}&message={1}", "continuous", "Downloading app...");
                string waitUrl = GameManagerUtilities.sWaitHtml + param;

                //if e.Filename dont matches with package name we might end up opening two tabs for same app
                TabButtons.Instance.AddDownloadTab(e.Filename, null, waitUrl, null, true, e.Url, true);
            }
            else
            {
                System.Windows.Forms.SaveFileDialog saveFileDialog = new System.Windows.Forms.SaveFileDialog();

                saveFileDialog.Filter = "All files (*.*)|*.*";
                saveFileDialog.FilterIndex = 1;
                saveFileDialog.RestoreDirectory = true;
                saveFileDialog.FileName = e.Filename;
                string pathDownload = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                saveFileDialog.InitialDirectory = pathDownload;

                if (saveFileDialog.ShowDialog() == DialogResult.OK)
                {
                    string filePath = saveFileDialog.FileName;
                    string param = String.Format(@"?style={0}&message={1}", "continuous", "Downloading ...");
                    string waitUrl = GameManagerUtilities.sWaitHtml + param;
                    string fileName = Path.GetFileName(filePath);

                    TabButtons.Instance.AddAllDownloadTab(fileName, waitUrl, null, true, e.Url, filePath);
                }
            }
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

        public void LaunchUrlIntentActivity(Object param)
        {
            if (AppHandler.mLastAppLaunched.Contains(Common.Strings.HomeAppPackageName) == true ||
                    AppHandler.mLastAppLaunched.Contains(Common.Strings.SharerActivityPackage) == true)
            {
                TabButtons.Instance.GoToHomeTab();
                GMApi.LaunchUrlIntentActivity(param.ToString());
            }
            else
            {
                //Handle Intent launch on HomeAppDisplayed later
                AppHandler.UrlToLaunchOnHomeAppDisplayed = param.ToString();
                Logger.Info("UrlToLaunchOnHomeAppDisplayed = {0}", AppHandler.UrlToLaunchOnHomeAppDisplayed);
                TabButtons.Instance.GoToHomeTab();
            }
        }

        public string getUserName()
        {
            return GMApi.getUserName();
        }

        public void setUserName(Object param)
        {
            string name = param.ToString();
            GMApi.setUserName(name);
        }

        public string getInterests()
        {
            return GMApi.getInterests();
        }

        public void setInterests(Object param)
        {
            string name = param.ToString();
            GMApi.setInterests(name);
        }

        public string getInstallStatus()
        {
            return GMApi.getInstallStatus();
        }

        public void CloseCurrentTab()
        {
            TabButtons.Instance.SelectedTab.Close();
        }

        public string inTabBar()
        {
            if (this.mParentTab is TabButton)
                return "true";
            else
                return "false";
        }

        public void GoHome()
        {
            TabButtons.Instance.GoToHomeTab();
        }

        public void relaunchApp(string displayName, string package, string activity, string apkUrl)
        {
            if (String.Compare(package, "") == 0)
            {
                return;
            }

            TabButtons.Instance.RelaunchApp(displayName, package, activity, apkUrl);
        }

        public void stageCompleted(Object param)
        {
            string stage = param.ToString();
            //			GameManager.sGameManager.StageCompleted(stage);
        }

        public bool isAppInstalled(string package)
        {
            return GMApi.isAppInstalled(package);
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

        public void showWebPage(string title, string webUrl)
        {
            Logger.Info("sending web app stats for " + title);
            Common.Stats.SendAppStats(title, title, BlueStacks.hyperDroid.Version.STRING, "", Common.Stats.AppType.web);
            GameManagerWindow.Instance.Show();
            TabButtons.Instance.ShowWebPage(title, webUrl, null);
        }

        public void PlayUserStreamHook(string label)
        {
            StreamViewTimeStats.HandleStreamViewStatsEvent(label,
                        StreamViewStatsEventName.VideoPlay);
        }

        public void PauseUserStreamHook(string label)
        {
            StreamViewTimeStats.HandleStreamViewStatsEvent(label,
                        StreamViewStatsEventName.VideoPause);
        }

        public void StartStreamViewStatsRecorder(string label, string jsonString)
        {
            if (!StreamViewTimeStats.sStreamViewTimeStatsList.ContainsKey(label))
                new StreamViewTimeStats(label, jsonString);
        }


        public void ShowWebPageInBrowser(string url)
        {
            Logger.Info("Showing " + url + " in default browser");
            Process.Start(url);
        }

        public void gmHandleAppDisplayed(string jsonString)
        {
            JSonReader readjson = new JSonReader();
            IJSonObject fullJson = readjson.ReadAsJSonObject(jsonString);
            bool isAppInstalled = this.isAppInstalled(fullJson["pkg"].ToString());

            this.Navigate("javascript:gmHandleAppDisplayed('" + isAppInstalled.ToString() + "','" + jsonString.Replace("'", "&#39;") + "')");
        }

        public void ShowApp(String displayName, String package, String activity, String apkUrl)
        {
            Logger.Info("Calling GameManager.ShowApp() from the ShowApp wrapper in Browser");
            GameManagerWindow.Instance.Show();
            StartAppHandling(displayName, package, activity, apkUrl);
        }

        public void LaunchApp(String displayName, String package)
        {
            Logger.Info("LaunchApp({0}, {1})", displayName, package);
            StartAppHandling(displayName, package, ".Main", "");
        }

        public void InstallApp(String appName, String apkUrl)
        {
            Logger.Info("InstallApp({0})", apkUrl);

            if (mParentTab != null)
            {
                mParentTab.DownloadApk(apkUrl, appName, true, true);
            }
        }

        public void ReportProblem()
        {
            GMApi.ReportProblem();
        }

        public void RestartAndroidPlugin()
        {
            GMApi.RestartAndroidPlugin();
        }

        public void CheckForUpdates()
        {
            GMApi.CheckForUpdates();
        }

        public void startCDNAppDownload(string pkg, string apkUrl)
        {
            TabButtons.Instance.SelectedTab.DownloadApkForPackage(apkUrl, pkg, true, false);
        }

        public void startAppDownload(string pkg, string apkUrl)
        {
            TabButtons.Instance.SelectedTab.DownloadApkForPackage(apkUrl, pkg, true, true);
        }

        public string getAppDownloadProgress(string pkg)
        {
            return GMApi.getAppDownloadProgress(pkg);
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

        public bool uninstallApp(string pkg)
        {
            if (GMApi.uninstallApp(pkg))
            {
                AppHandler.AppUninstalled(pkg);
                return true;
            }
            return false;
        }

        public bool createAppShortcut(string pkg)
        {
            Thread t = new Thread(delegate ()
            {
                GMApi.createAppShortcut(pkg);
            });
            t.IsBackground = true;
            t.Start();

            return true;
        }

        public void showMyAppsLocal()
        {
            Logger.Info("in showMyAppsLocal()");
            this.Navigate(GameManagerUtilities.sLocalMyAppsHtml);
        }

        public void showMyAccount()
        {
            Logger.Info("opening my account page");
            string myAccountUrl = "https://bluestacks-cloud.appspot.com/myaccount";
            myAccountUrl += "?guid=" + User.GUID;
            string label = "My Accounts";
            TabButtons.Instance.ShowWebPage(label, myAccountUrl, null);
        }

        public void launchHelpApp()
        {
            AppHandler.ShowApp(
                    "Help",
                    "com.bluestacks.help",
                    "com.bluestacks.help.com.bluestacks.help.HelpActivity",
                    "",
                    true
                    );
        }

        public void launchLanguageInputApp()
        {
            AppHandler.ShowApp(
                    "Input Settings",
                    "com.android.settings",
                    "com.android.settings.LanguageSettings",
                    "",
                    true
                    );

        }




        private void DocumentTitleChangedHandler(object sender, EventArgs e)
        {
            if (mParentTab != null)
            {
                mParentTab.UpdateTabText(this.DocumentTitle);
            }
        }

        public void UpdateTabTitle(string title)
        {
            mParentTab.UpdateTabText(title);
        }

        private void DocumentCompletedHandler(object sender, GeckoDocumentCompletedEventArgs e)
        {
            if (!this.mUrlSet)
            {
                this.Navigate(mUrl);
                this.Focus();
                this.mUrlSet = true;
                return;
            }
            mDocumentLoadCompleted = true;
        }

        private void NavigateErrorHandler(object sender, GeckoNavigationErrorEventArgs e)
        {
            Logger.Info(e.ToString());
            Logger.Info("Cannot navigate to " + e.Uri.ToString());
            Logger.Info("StatusCode: " + e.ErrorCode);

            /*
			if (e.StatusCode == 200 || e.StatusCode == 204 || e.StatusCode == 404)
				return;
			*/
            mFailedUrl = mUrl;
            if (mFailedUrl != GameManagerUtilities.sNoWifiHtml && e.ErrorCode.ToString() == GeckoError.NS_ERROR_UNKNOWN_HOST.ToString())
            {
                Logger.Info("Set mFailedUrl to " + mFailedUrl);
                this.Navigate(GameManagerUtilities.sNoWifiHtml);
            }
        }

        private void DownloadAndInstallApk(string url)
        {
            TabButton parent = null;
            if (inTabBar() == "true")
                parent = this.mParentTab;

            string fileName = Path.GetRandomFileName();
            string filePath = Path.Combine(GameManagerUtilities.SetupDir, fileName);
            Logger.Info("Will download " + url + " to " + filePath);

            Thread dl = new Thread(delegate ()
            {
                if (parent != null)
                    parent.mIsDownloading = true;

                mDownloader = new Downloader(1, url, filePath);
                mDownloader.Download(
                    delegate (int percent)
                    {
                        Logger.Info("percent: " + percent);
                        SetFileDownloadProgress(percent.ToString());
                    },
                    delegate (String file)
                    {
                        Logger.Info("file downloaded to: " + file);
                        if (parent != null)
                            parent.mIsDownloading = false;

                        if (file.EndsWith(".apk"))
                        {
                            SetProgressBarText("Installing app...");
                            SetProgressBarStyle("marquee");
                            InstallApk(file, "");
                        }
                    },
                    delegate (Exception exc)
                    {
                        Logger.Error(exc.ToString());
                        if (parent != null)
                            parent.mIsDownloading = false;
                    },
                    delegate (String contentType)
                    {
                        if (contentType == "application/vnd.android.package-archive")
                        {
                            ShowWaitPage("continuous", "Downloading app...");
                            return true;
                        }
                        else
                        {
                            if (parent != null)
                                parent.mIsDownloading = false;
                            return false;
                        }
                    }
                );
            });

            dl.IsBackground = true;
            dl.Start();
        }

        public void ShowWaitPage(string progressStyle, string message)
        {
            string waitUrl = GameManagerUtilities.sWaitHtml;
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
            lock (mDownloadUpdateLock)
            {
                object[] args = { percent };
                if (mLastCheckTime == null)
                {
                    mLastCheckTime = DateTime.Now;
                }
                else if ((DateTime.Now - mLastCheckTime).TotalSeconds >= 1)
                {
                    mLastCheckTime = DateTime.Now;
                    if (mDocumentLoadCompleted)
                        CallJs("setProgress", args);
                }
                else
                {
                    //Logger.Info("skipping update");
                }
            }
        }

        private void InstallApk(string apkPath, string tabName)
        {
            Logger.Info("Installing apk: {0}", apkPath);

            bool eventSet = GameManagerUtilities.sAppInstallEvent.WaitOne(0, false);
            Logger.Info("eventSet: " + eventSet);
            if (!eventSet)
            {
                Logger.Info("Waiting for event");
                GameManagerUtilities.sAppInstallEvent.WaitOne();
            }

            Thread apkInstall = new Thread(delegate ()
            {
                RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
                string installDir = (string)reg.GetValue("InstallDir");

                ProcessStartInfo psi = new ProcessStartInfo();
                psi.FileName = Path.Combine(installDir, "HD-ApkHandler.exe");
                psi.Arguments = String.Format("\"{0}\" silent", apkPath);
                psi.UseShellExecute = false;
                psi.CreateNoWindow = true;

                Logger.Info("Installer path {0}", psi.FileName);

                Process silentApkInstaller = Process.Start(psi);

                silentApkInstaller.WaitForExit();
                Logger.Info("Apk installer exit code: {0}", silentApkInstaller.ExitCode);

                Logger.Info("Setting event");
                GameManagerUtilities.sAppInstallEvent.Set();

                TabButton parent = this.mParentTab;
                if (silentApkInstaller.ExitCode == 0)
                {
                    Logger.Info("Installation successful.");
                    webClient = null;
                    File.Delete(apkPath);
                    GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                    {
                        TabButtons.Instance.CloseTab(parent.mKey);
                    }));
                }
                else
                {
                    String reason = "Error code: " + silentApkInstaller.ExitCode + ", " + ((Common.InstallerCodes)silentApkInstaller.ExitCode).ToString();
                    GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                    {
                        TabButtons.Instance.CloseTab(tabName);
                        TabButtons.Instance.AddErrorTab("Install Error", reason);

                    }));
                }
            });

            apkInstall.IsBackground = true;
            apkInstall.Start();
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

        private void StartAppHandling(string displayName, string package, string activity, string apkUrl)
        {
            if (String.Compare(package, "") == 0)
            {
                return;
            }

            GMAppsManager installedAppsList = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
            installedAppsList.UpdateAppTimeStamp(package);

            /*if (String.Compare(apkUrl, "") == 0)
			{
				apkUrl = "http://cdn.bluestacks.com/public/appsettings/GamePop/apps/" + package + ".apk";
			}*/

            if (!AppHandler.IsAppInstalled(package))
            {
                Logger.Info("apkUrl: " + apkUrl);
                if (String.IsNullOrEmpty(apkUrl) == false)
                {
                    string statusText = String.Format("Downloading {0}...", displayName);
                    string param = String.Format(@"?style={0}&message={1}&title={2}", "continuous", statusText, displayName);
                    string waitUrl = GameManagerUtilities.sWaitHtml + param;

                    TabButton tab = TabButtons.Instance.AddDownloadTab(displayName, package, waitUrl, null, true, apkUrl, true);

                    tab.mPackageName = package;
                    tab.mActivity = activity;
                }
                else
                {
                    /*
					 * Android will launch vending for this app on receiving runex
					 * GM will create a tab for the next vending launch based on this flag
					 */
                    AppHandler.mCreateVendingTab = true;
                    AppHandler.mDontCreateAppTab = true;
                    AppHandler.ShowApp(displayName, package, activity, apkUrl, true);
                }
            }
            else
                AppHandler.ShowApp(displayName, package, activity, apkUrl, true);
        }

        public string GetLocaleName()
        {
            return GMApi.GetLocaleName();
        }

        public string GetAvailableLocaleName()
        {
            return GMApi.GetAvailableLocaleName();
        }
        public string GetCurrentTheme()
        {
            Logger.Info("in GetCurrentTheme()");
            string currentTheme = GameManagerUtilities.getCurrentTheme();
            Logger.Info("current Theme = " + currentTheme);
            return currentTheme;
        }

        //needs to be theme-specific
        public void LaunchHome()
        {
            //string homeUrl = this.Navigate(GameManager.sGameManager.mGameManagerUrls[(int)GameManager.GameManagerStage.HOME]);
            this.Navigate(GameManagerUtilities.getCurrentThemeHomeUrl());
        }

        //needs to be theme specific
        public void LaunchSearch(string searchString)
        {
            //get the search_result url for the current theme
            string searchUrl = GameManagerUtilities.getCurrentThemeSearchUrl() + "?searchstring=" + searchString;
            Logger.Info("searchUrl = " + searchUrl);
            this.Navigate(searchUrl);
        }


        public void LaunchThemesPage()
        {
            this.Navigate(GameManagerUtilities.getCurrentThemeThemesUrl());
        }

        public string GetChannelNamesJson()
        {
            return GMApi.GetChannelNamesJson();
        }

        public string GetChannelAppsJson(string channelId, string subCategory)
        {
            return GMApi.GetChannelAppsJson(channelId, subCategory);
        }

        public string GetChannelAppsJsonWithHTMLClass(string htmlClass, string channelId, string subCategory)
        {
            return "{\"html_class\":\"" + htmlClass + "\", \"apps\":" + GMApi.GetChannelAppsJson(channelId, subCategory) + "}";
        }

        public string searchAppHandler(string searchString)
        {
            this.SendSearchStats(searchString);

            JSonReader readjson = new JSonReader();
            IJSonObject fullJson = readjson.ReadAsJSonObject(this.GetInstalledAppsJson(false).Replace("'", "&#39;"));

            string installed_apps_json_string = fullJson.ToString();
            string channel_apps_json_string = GMApi.GetChannelAppsJson().Replace("'", "&#39;").Replace("\r\n", "");

            return "{\"search_string\":\"" + searchString + "\",\"installed_apps\":" + installed_apps_json_string + ",\"channel_apps\":" + channel_apps_json_string + "}";
        }

        public string GetInstalledAppsJsonforHTMLElement(string htmlClass)
        {
            return "{\"htmlClass\":\"" + htmlClass + "\",\"recentApps\":" + GetInstalledAppsJson(false) + "}";
        }

        public string GetInstalledAppsJsonforJS()
        {
            return GetInstalledAppsJson(false);
        }

        public string GetThemesJson()
        {
            return GMApi.GetThemesJson();
        }

        private String GetInstalledAppsJson(bool escapeQuotes)
        {
            string appsJson = Path.Combine(sHomeDataDir, GMUtils.sInstalledAppsJson);
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
            return GameManagerUtilities.GameManagerPort.ToString();
        }

        public string GetCurrentAppInfo()
        {
            string[] currentTabData = TabButtons.Instance.GetCurrentTabData();

            JSonWriter info = new JSonWriter();
            info.WriteObjectBegin();
            info.WriteMember("type", currentTabData[0]);
            info.WriteMember("name", currentTabData[1]);
            info.WriteMember("data", currentTabData[2]);
            info.WriteObjectEnd();

            return info.ToString();
        }

        public void ShowStreamWindow()
        {
            BTVManager.ShowStreamWindow();
        }

        public void ShowViewTab()
        {
            TabButtons.Instance.ShowViewTab();
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
            int givenWidth = Convert.ToInt32(widthStr);
            int givenHeight = Convert.ToInt32(heightStr);
            Logger.Info("CreateNewBrowserWindowV2 Given: widthxheight {0}x{1}", givenWidth, givenHeight);

            int width = (int)(givenWidth * Utils.GetDPI() * 1.0f / 96);
            int height = (int)(givenHeight * Utils.GetDPI() * 1.0f / 96);

            if (Utils.GetSystemWidth() < width)
                width = Utils.GetSystemWidth() - 50;

            int bottomTaskBarPadding = (int)(100 * Utils.GetDPI() * 1.0f / 96);
            if ((Utils.GetSystemHeight() - bottomTaskBarPadding) < height)
                height = Utils.GetSystemHeight() - bottomTaskBarPadding;

            Logger.Info("CreateNewBrowserWindowV2 New: widthxheight {0}x{1}", width, height);

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
            newForm.StartPosition = FormStartPosition.CenterScreen;
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

		public void DoNotShowThisMessageAgain(string isChecked)
		{
			if(mParentWindow!=null)
			{
				mParentWindow.UpdateTagInRegistry(isChecked);
			}
		}

		public void EnableVtClicked()
		{
			Logger.Info("Enable vt clicked");
			ThreadPool.QueueUserWorkItem(delegate(Object stateInfo)
					{
					try
					{
					Common.Stats.SendMiscellaneousStatsSync("EnableVtx",
						"Enable vt clicked", User.GUID, null, null, null);
					}
					catch(Exception e)
					{
					Logger.Error("Could not send enable vt clicked stats: {0}", e.Message);
					}
					});
		}

		public void PopupClose()
		{
			Logger.Info("Close button clicked");
			if (PopupWindow.Instance != null)
			{
				PopupWindow.Instance.Close();
			}
		}

		public void ShowHiddenPopup()
		{
			Logger.Info("Html successfully loaded, showing hidden popup");
			if (PopupWindow.Instance != null)
			{
				if (PopupWindow.DimBackground)
					new DimWindow(PopupWindow.Instance);
				else
					PopupWindow.Instance.ShowDialog();
			}
		}
	}
}
