using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Drawing.Text;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using System.Windows;
using System.Net;
using System.Text;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.GameManager
{
    public class GameManagerUtilities
    {
        public const string BSTSERVICES = "com.bluestacks.home";

        public static String WindowTitle = "BlueStacks App Player";
        public static int sDpi = Utils.GetDPI();

        [DllImport("user32.dll")]
        private static extern int GetSystemMetrics(int which);
        private const int SM_CXSCREEN = 0;
        private const int SM_CYSCREEN = 1;

        public static string sHomeType = string.Empty;
        public static bool sOBSDevEnv = false;
        public static Mutex sGameManagerLock;

        public static bool sRememberClosingPopupChoice = false;

        private static System.Drawing.Size? sSize = null;
        public static System.Drawing.Size ScreenSize
        {
            get
            {
                if (!sSize.HasValue)
                {
                    sSize = new System.Drawing.Size(GetSystemMetrics(SM_CXSCREEN), GetSystemMetrics(SM_CYSCREEN));
                }
                return sSize.Value;
            }
        }

        public static EventWaitHandle sAppInstallEvent;

        internal static void Init()
        {
            RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            sHomeType = (string)prodKey.GetValue("homeType", "gphome");

            if (String.Compare(sHomeType, "html", true) == 0)
            {
                DownloadJsonFiles();
            }

            Thread httpThread = new Thread(SetupHTTPServer);
            httpThread.IsBackground = true;
            httpThread.Start();

            if (!Directory.Exists(SetupDir))
            {
                Directory.CreateDirectory(SetupDir);
            }

            sAppInstallEvent = new EventWaitHandle(true, EventResetMode.AutoReset);
			UserPreferenceWindow.SetDefaultRegistry();
        }

        public static Common.HTTP.Server sServer;
        public static string sBaseUrl = "http://localhost:2881/static/";
        private static void SetupHTTPServer()
        {
            Dictionary<String, Common.HTTP.Server.RequestHandler> routes =
                new Dictionary<String, Common.HTTP.Server.RequestHandler>();

            routes.Add("/" + Common.Strings.GMNotificationUrl, GMHTTPHandler.GMNotificationHandler);
            routes.Add("/" + Common.Strings.AppDisplayedUrl, GMHTTPHandler.AppDisplayedHandler);
            routes.Add("/" + Common.Strings.AppLaunchedUrl, GMHTTPHandler.AppLaunchedHandler);
            routes.Add("/" + Common.Strings.ShowAppUrl, GMHTTPHandler.ShowAppHandler);
            routes.Add("/" + Common.Strings.ShowWindowUrl, GMHTTPHandler.ShowWindowHandler);
            routes.Add("/" + Common.Strings.IsGMVisibleUrl, GMHTTPHandler.IsVisibleHandler);
            routes.Add("/" + Common.Strings.S2PConfiguredUrl, GMHTTPHandler.S2PConfiguredHandler);
            routes.Add("/" + Common.Strings.AppUninstalledUrl, GMHTTPHandler.AppUninstalledHandler);
            routes.Add("/" + Common.Strings.GMLaunchWebTab, GMHTTPHandler.GMLaunchWebTab);
            routes.Add("/ping", GMHTTPHandler.PingHandler);
            routes.Add("/quit", GMHTTPHandler.ForceQuitHandler);
            routes.Add("/google", GMHTTPHandler.OpenGoogleHandler);
            routes.Add("/closecrashedapptab", GMHTTPHandler.AppCrashedHandler);
            routes.Add("/reportobserror", GMHTTPHandler.ReportObsErrorHandler);

            routes.Add("/attachfrontend", GMHTTPHandler.AttachFrontend);

            routes.Add("/streamstatus", GMHTTPHandler.StreamStatusHandler);
            routes.Add("/streamstarted", GMHTTPHandler.StreamStartedHandler);
            routes.Add("/streamstopped", GMHTTPHandler.StreamStoppedHandler);
            routes.Add("/recordstarted", GMHTTPHandler.RecordStartedHandler);
            routes.Add("/recordstopped", GMHTTPHandler.RecordStoppedHandler);
            routes.Add("/replaybuffersaved", GMHTTPHandler.ReplayBufferSavedHandler);
            routes.Add("/getcurrentapppkg", GMHTTPHandler.GetCurrentAppPackageHandler);
            routes.Add("/streamdimension", GMHTTPHandler.SetStreamDimensionHandler);
            routes.Add("/initstream", GMHTTPHandler.InitStreamHandler);
            routes.Add("/showwebpage", GMHTTPHandler.ShowWebPageHandler);
            routes.Add("/streamwindowclosed", GMHTTPHandler.StreamWindowClosedHandler);
            routes.Add("/getcurrentappinfo", GMHTTPHandler.GetCurrentAppInfoHandler);
            routes.Add("/closetab", GMHTTPHandler.TabCloseHandler);
            routes.Add("/showwelcometab", GMHTTPHandler.ShowWelcomeTabHandler);
            routes.Add("/vibratenotification", GMHTTPHandler.GameManagerVibrateNotificationHandler);
            routes.Add("/setfrontendposition", GMHTTPHandler.SetFrontendPositionHandler);
            routes.Add("/restartgamemanager", GMHTTPHandler.RestartGameManager);
            routes.Add("/addstreamviewkey", GMHTTPHandler.AddStreamViewKeyHandler);
            routes.Add("/relaunchstreamwindow", GMHTTPHandler.RelaunchStreamWindowHandler);
            routes.Add("/offerUrl", GMHTTPHandler.OfferUrlHandler);
            routes.Add("/showenablevtpopup", GMHTTPHandler.ShowEnableVtPopupHandler);
            routes.Add("/isportraitmode", GMHTTPHandler.IsPortraitModeHandler);
            foreach (var item in Frontend.HTTPHandler.GetRoutes())
            {
                if (!routes.ContainsKey(item.Key))
                {
                    routes.Add(item.Key, item.Value);
                }
            }
            HttpHandlerSetup.InitHTTPServer(routes, Common.Strings.GameManagerHomeDir, true);
        }

        internal static Opt args;
        internal static void FillArguments(StartupEventArgs e)
        {
            args = new Opt();
            args.Parse(e.Args);
            Logger.Info("pkg name = " + args.p);
            Logger.Info("activity = " + args.a);
        }


        public static void StopHTTPServer()
        {
            sServer.Stop();
        }

        private static void DownloadJsonFiles()
        {
            Thread thr = new Thread(delegate ()
            {
                try
                {
                    GMApi.GetChannelNamesJson();
                    GMApi.GetChannelAppsJson();
                    GMApi.GetWebAppsJson();
                    GMApi.GetThemesJson();
                }
                catch (Exception e)
                {
                    Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
                }
            });
            thr.IsBackground = true;
            thr.Start();
        }

        public static string sDefaultTheme = "default_theme";
        public static String sLocalMyAppsHtml;
        public static String sNoWifiHtml;
        public static String sWaitHtml;
        public static String sStreamWindowHtml;
        public static String sStreamWindowProdHtml;
        public static String sStreamWindowQAHtml;
        public static String sStreamWindowStagingHtml;
        public static String sStreamWindowDevHtml;
        public static void UpdateLocalUrls()
        {
            string baseUrl = String.Format("http://localhost:{0}/static/themes", sGameManagerPort);
            string themeUrl = String.Format("{0}/{1}/", baseUrl, getCurrentTheme());
            sLocalMyAppsHtml = themeUrl + "local-my-apps.html";
            sNoWifiHtml = themeUrl + "no-wifi.html";
            sWaitHtml = themeUrl + "wait.html";
            sStreamWindowHtml = "http://bluestacks-tv.appspot.com/home";
            sStreamWindowProdHtml = "http://bluestacks-tv-prod.appspot.com/home";
            sStreamWindowQAHtml = "http://bluestacks-tv-qa.appspot.com/home";
            sStreamWindowStagingHtml = "http://bluestacks-tv-staging.appspot.com/home";
            sStreamWindowDevHtml = "http://bluestacks-tv-dev.appspot.com/home";
        }

        public const string THEMES_HTML = "themes.html";
        public const string HOME_HTML = "home.html";
        public const string SEARCH_HTML = "search-results.html";
        public const string LOCAL_MY_APPS_HTML = "local-my-apps.html";

        public static string mHomeUrl;
        public static string mCurrentThemeUrl;

        public static void applyTheme(string name, string themeBaseUrl)
        {
            RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
            key.SetValue("theme", name);
            key.SetValue("themeUrl", themeBaseUrl);
            key.Close();

            //AMAN commented this code in wpf module
            //UIHelper.RunOnUIThread(this, delegate ()
            //{
            //	Tab homeTab = (Tab)mTabBar.SelectedTab;
            //	homeTab.Controls.Remove(homeTab.mBrowser);
            //	homeTab.mBrowser.Dispose();

            //	string homePageUrl = themeBaseUrl + HOME_HTML;
            //	//important: set the CurrentThemeUrl to new Theme's BaseUrl
            //	mCurrentThemeUrl = themeBaseUrl;

            //	homeTab.mBrowser = new Browser(homePageUrl);
            //	homeTab.mBrowser.Dock = DockStyle.Fill;
            //	homeTab.Controls.Add(homeTab.mBrowser);
            //});
        }


        internal static System.Windows.Media.FontFamily GetFont()
        {
            return new System.Windows.Media.FontFamily(new Uri("pack://application:,,,/Assets/Roboto-Medium.ttf"), "./resources/#Roboto Medium");
        }

        public static bool IsFirstLaunch
        {
            get
            {
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
                string firstTimeLaunch = (string)regKey.GetValue("IsFirstTimeLaunch", "true");

                if (String.Compare(firstTimeLaunch, "true", true) == 0)
                {
                    regKey.SetValue("IsFirstTimeLaunch", "false", RegistryValueKind.String);
                    return true;
                }
                return false;
            }
        }

        static string sInstallDir = string.Empty;

        public static string InstallDir
        {
            get
            {
                if (string.IsNullOrEmpty(sInstallDir))
                {
                    RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
                    sInstallDir = (string)reg.GetValue("InstallDir");
                    Logger.Info("the installdir path is " + sInstallDir);
                }
                return sInstallDir;
            }

            set
            {
                sInstallDir = value;
            }
        }

        static string sBluestacksGameManager = string.Empty;

        public static string BluestacksGameManager
        {
            get
            {
                if (string.IsNullOrEmpty(sBluestacksGameManager))
                {
                    RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
                    sBluestacksGameManager = (string)reg.GetValue("InstallDir");
                    Logger.Info("the sBluestacksGameManager path is " + sBluestacksGameManager);
                }
                return sBluestacksGameManager;
            }

            set
            {
                sBluestacksGameManager = value;
            }
        }



        static string sProgramData = string.Empty;
        public static string ProgramData
        {
            get
            {
                if (string.IsNullOrEmpty(sProgramData))
                {
                    sProgramData = (string)Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
                    Logger.Info("the value of gmconfipath is {0}", Common.Strings.GMBasePath);
                }
                return sProgramData;
            }

            set
            {
                sProgramData = value;
            }
        }


        static string sSetupDir = string.Empty;
        public static string SetupDir
        {
            get
            {
                if (string.IsNullOrEmpty(sSetupDir))
                {
                    sSetupDir = Path.Combine(ProgramData, "BlueStacksSetup");
                }
                return sSetupDir;
            }

            set
            {
                sSetupDir = value;
            }
        }



        static int sGameManagerPort = 2871;
        public static int GameManagerPort
        {
            get
            {
                if (sGameManagerPort == int.MinValue)
                {
                    RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
                    sGameManagerPort = (int)reg.GetValue(Common.Strings.PartnePortKeyName, sGameManagerPort);
                }
                return sGameManagerPort;
            }

            set
            {
                sGameManagerPort = value;
            }
        }
        private static string sAssetsDir = string.Empty;
        public static string AssetsDir
        {
            get
            {
                if (string.IsNullOrEmpty(sAssetsDir))
                {
                    sAssetsDir = Path.Combine(BluestacksGameManager, "Assets");
                }
                return sAssetsDir;
            }
        }


        
        public static void RunTroubleShooterExe(string fileName, string args, string text, string title)
        {
            try
            {
                Logger.Info("In Method RunTroubleShooterExe");
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
                string filePath = Path.Combine(
                        (string)key.GetValue("InstallDir"),
                        fileName);

                Process proc = new Process();
                proc.StartInfo.FileName = filePath;
                proc.StartInfo.Arguments = args;
                proc.EnableRaisingEvents = true;

                proc.Exited += new EventHandler(delegate (object sender, EventArgs e)
                {
                    System.Windows.Forms.MessageBox.Show(text, title, MessageBoxButtons.OK);
                    Logger.Info("Exit StuckAtLoading TroubleShooter");
                });
                proc.Start();
                proc.WaitForExit();
                proc.Close();
            }
            catch (Exception e)
            {
                Logger.Error("Error occured, Err: {0}", e.ToString());
            }
        }

        public static string ToggleDisabledAppListFileUrl
        {
            get
            {
                if (Features.IsFeatureEnabled(Features.CHINA_CLOUD) == true)
                {
                    return Strings.ChinaToggleDisabledAppListUrl;
                }
                return Strings.WorldWideToggleDisabledAppListUrl;
            }
        }

        public static string ToggleDisabledAppListLocation
        {
            get
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Strings.GMBasePath);
                if (key == null)
                {
                    return String.Empty;
                }

                string installDir = (String)key.GetValue(Strings.GMInstallDirKeyName, String.Empty);
                key.Close();
                return Path.Combine(installDir, Strings.ToggleDisableAppListFileName);
            }
        }

        private static Object sAccessToggleAppListLock = new Object();
        static List<string> toggleDisabledAppList;
        public static List<string> ToggleDisableAppList
        {
            get
            {
                lock (sAccessToggleAppListLock)
                {
                    if (toggleDisabledAppList == null)
                    {

                        if (File.Exists(ToggleDisabledAppListLocation) == false ||
                                File.GetLastWriteTime(ToggleDisabledAppListLocation) < DateTime.Now.AddDays(-1))
                        {
                            WebClient webClient = new WebClient();
                            string tmpDir = Environment.GetEnvironmentVariable("TEMP");
                            string tempFileLocation = Path.Combine(tmpDir, Path.GetRandomFileName());
                            try
                            {
                                if (File.Exists(tempFileLocation) == true)
                                {
                                    File.Delete(tempFileLocation);
                                }
                                /*
								 * Downloading file asynchronously. so that the tab switch time does not increase
								 * in gamemanager. It will cause the ToggleDisabledAppList to not get updated in
								 * some first cases of tab-switch operations but its better than increasing tab-switch
								 * time.
								 */
                                Logger.Info("ToggleDisabledAppListFileUrl = {0}, ToggleDisabledAppListLocation = {1}",
                                        ToggleDisabledAppListFileUrl, ToggleDisabledAppListLocation);
                                webClient.DownloadFile(ToggleDisabledAppListFileUrl, tempFileLocation);
                                if (File.Exists(tempFileLocation) == false)
                                {
                                    Logger.Info("Unable to download file.");
                                }
                                String fileContent = File.ReadAllText(tempFileLocation, Encoding.UTF8);
                                if (fileContent != null && fileContent.Trim().Length > 0)
                                {
                                    File.Copy(tempFileLocation, ToggleDisabledAppListLocation, true);
                                }
                            }
                            catch (Exception e)
                            {
                                Logger.Error("Error Occurred while downloading ToggleDisabedAppListFile, Err: {0}", e.ToString());
                            }
                        }

                        if (File.Exists(ToggleDisabledAppListLocation))
                        {
                            toggleDisabledAppList = new List<string>(File.ReadAllLines(ToggleDisabledAppListLocation, Encoding.UTF8));
                        }
                        else
                        {
                            toggleDisabledAppList = new List<string>();
                        }

                        toggleDisabledAppList.Add("com.bluestacks.gamepophome");
                        toggleDisabledAppList.Add("com.android.vending");
                        toggleDisabledAppList.Add("com.google.android.setupwizard");
                    }
                    return toggleDisabledAppList;
                }
            }
        }

        public static string getCurrentThemeHomeUrl()
        {
            string result = Path.Combine(mCurrentThemeUrl, HOME_HTML);
            Logger.Info("getCurrentThemeHomeUrl(): " + result);
            return result;
        }

        public static string getCurrentThemeSearchUrl()
        {
            string result = Path.Combine(mCurrentThemeUrl, SEARCH_HTML);
            Logger.Info("getCurrentThemeSearchUrl(): " + result);
            return result;
        }

        public static string getCurrentThemeThemesUrl()
        {
            string result = Path.Combine(mCurrentThemeUrl, THEMES_HTML);
            Logger.Info("getCurrentThemeThemesUrl(): " + result);
            return result;
        }

        public static string getCurrentTheme()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            return (string)key.GetValue("theme", sDefaultTheme);
        }

        private static string mThemesDir = "themes\\";
        public static string GetCurrentThemeLocalDir()
        {
            return Path.Combine(Common.Strings.GameManagerHomeDir, mThemesDir + getCurrentTheme());
        }

        public static string GetThemesDir()
        {
            return Path.Combine(Common.Strings.GameManagerHomeDir, mThemesDir);
        }

        public static void LaunchWelcomeNote()
        {
            try
            {
                Logger.Info("Launching welcome note");
                string hostUrl = String.Format("{0}/update_notes?prod_ver={1}&oem={2}&campaign=&guid={3}",
                        BlueStacks.hyperDroid.Common.Strings.ChannelsUrl,
                        Version.STRING,
                        Oem.Instance.OEM,
                        User.GUID);
                string responseString = BlueStacks.hyperDroid.Common.HTTP.Client.Get(hostUrl, null, false);
                Logger.Info("Response string from cloud : " + responseString);
                JSonReader readjson = new JSonReader();
                IJSonObject jsonObj = readjson.ReadAsJSonObject(responseString);
                string updateNoteUrl = jsonObj["update_notes_url"].StringValue.Trim();
                ShowPromotion("welcomenote", updateNoteUrl);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to launch welcome note... Err : " + ex.ToString());
            }
        }

        public static void ShowPromotion(string tag, string url)
        {
            Thread thread = new Thread(delegate()
            {
                while (true)
                {
                    if (PopupWindow.Instance == null)
                    {
                        GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                        {
                            Logger.Info("Launching popup for tag : " + tag);
                            PopupWindow.Url = url;
                            PopupWindow.Tag = tag;
                            PopupWindow.DimBackground = true;
                            PopupWindow.ShowWindow();
                        }));
                        break;
                    }
                    else
                    {
                        Thread.Sleep(1000);
                        continue;
                    }
                }
            });
            thread.IsBackground = true;
            thread.Start();
        }

        public static void IsUserPro()
        {
            try
            {
                string url = String.Format("{0}/api/subscription/mybluestacksuserstate?guid={1}", Common.Strings.ChannelsUrl, User.GUID);
                string responseString = Common.HTTP.Client.Get(url, null, false);
                Logger.Info("Response string from cloud : " + responseString);
                JSonReader readjson = new JSonReader();
                IJSonObject jsonObj = readjson.ReadAsJSonObject(responseString);
                string success = jsonObj["success"].StringValue.Trim();
                if (string.Compare(success, "true", true) == 0)
                {
                    string isPaid = jsonObj["state"].StringValue.Trim();
                    if (string.Compare(isPaid, "PAID", true) == 0)
                    {
                        TopBar.Instance.ChangePremiumButton("premium");
                        Utils.UpdateRegistry(Common.Strings.GMConfigPath, "IsUserPro", "true", RegistryValueKind.String);
                    }
                    else
                    {
                        TopBar.Instance.ChangePremiumButton("buypro");
                        Utils.UpdateRegistry(Common.Strings.GMConfigPath, "IsUserPro", "false", RegistryValueKind.String);
                    }
                }
            }
            catch (Exception ex)
            {
                string isPro = Utils.GetValueFromRegistry(Common.Strings.GMConfigPath, "IsUserPro", "false");
                if (string.Compare(isPro, "true", true) == 0)
                    TopBar.Instance.ChangePremiumButton("premium");
                else
                    TopBar.Instance.ChangePremiumButton("buypro");
                Logger.Error("Failed to check if user is pro... Err : " + ex.ToString());
            }
        }
    }

    public class Opt : GetOpt
    {
        public String name = "";    //appName
        public String p = "";   // packageName
        public String a = "";   // activityName
    }
}
