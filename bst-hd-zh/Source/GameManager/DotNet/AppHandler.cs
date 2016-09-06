using BlueStacks.hyperDroid;
using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;

namespace BlueStacks.hyperDroid.GameManager
{
    public class AppHandler
    {
        private const string PLAYSTOREAPPINFO = "com.android.vending/com.google.android.finsky.activities.MainActivity";
        private const string PLAYLOGINAPPINFO = "com.google.android.gsf.login";
        public const string PLAYLOCATIONINFO = "com.google.android.gms";
        public const string STOREPACKAGE = "com.android.vending";
        public const string OTSPACKAGEINFO = "com.google.android.setupwizard";
        private const string BSTCMDPROCESSOR = "com.bluestacks.BstCommandProcessor";
        public const string BSTSERVICES = "com.bluestacks.home";
        private const string KEYMAPPINGTOOL = "com.bluestacks.keymappingtool";
        private const string APPFINDER = "com.bluestacks.appfinder";
        public const string GAMEPOPHOME = "com.bluestacks.gamepophome";
        private const string RESOLVERACTIVITY = "com.android.internal.app.ResolverActivity";
        private const string CHOOSERACTIVITY = "com.android.internal.app.ChooserActivity";
        public const string GOPROPACKAGE = "com.pop.store";
        public const string GOPROACTIVITY = "com.pop.store.UpgradeFromWindowsApiActivity";

        public static bool mAppDisplayedOccured = false;
        public static string mLastShownAppInfo = "";
        internal static string mLastAppLaunched = "";
        public static string mLastAppDisplayed = "";

        private static string mLastTopPackage;
        public static bool mCreateVendingTab = false;
        public static bool mDontCreateAppTab = false;

        public static string sAppToInstall;
        public static string sPackageToInstall;


        private static string mUrlToLaunchOnHomeAppDisplayed;
        private static Object sHomeAppDisplayedLock = new Object();

        public static string UrlToLaunchOnHomeAppDisplayed
        {
            get
            {
                lock (sHomeAppDisplayedLock)
                {
                    return mUrlToLaunchOnHomeAppDisplayed;
                }
            }
            set
            {
                lock (sHomeAppDisplayedLock)
                {
                    mUrlToLaunchOnHomeAppDisplayed = value;
                }
            }
        }

        public static string mDefaultLauncher;
        public static void Init()
        {
            InitStorePackages();
            InitPrebundledAppsList();
            InitIAPPackages();
            RegistryKey configKey = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.GMConfigPath);
            mDefaultLauncher = (string)configKey.GetValue("DefaultLauncher", GAMEPOPHOME);
        }


        private static List<string> mStorePackagesList = new List<string>();
        private static void InitStorePackages()
        {
            mStorePackagesList.Add("com.android.vending");
            mStorePackagesList.Add("com.amazon.venezia");
            mStorePackagesList.Add("com.qihoo.gameunion");
            mStorePackagesList.Add("com.baidu.appsearch");
            mStorePackagesList.Add("com.baidu.hao123");
            mStorePackagesList.Add("com.mappn.pad.gfan");
            mStorePackagesList.Add("com.hiapk.marketpho");
            mStorePackagesList.Add("com.tencent.mobileqq");
            mStorePackagesList.Add("com.pop.store");  // temporary fix for launching go pro activity in a new tab...fix in next iteration

        }

        private static List<String> mPrebundledApps;
        private static void InitPrebundledAppsList()
        {
            mPrebundledApps = new List<String>();
            mPrebundledApps.Add(GAMEPOPHOME);
            mPrebundledApps.Add("com.android.settings");
            mPrebundledApps.Add("com.bluestacks.home");
            mPrebundledApps.Add(STOREPACKAGE);
            mPrebundledApps.Add("com.baidu.appsearch");
        }
        public static bool IsAppInstalled(string package)
        {
            GMAppsManager iam = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
            bool isInstalled = mPrebundledApps.Contains(package) || iam.IsAppInstalled(package);
            Logger.Info("IsAppInstalled({0}): {1}", package, isInstalled);
            return isInstalled;
        }

        private static List<string> mIAPPackagesList = new List<string>();
        private static void InitIAPPackages()
        {
            mIAPPackagesList.Add("com.pop.store");
        }

        public static void AppLaunched(String package, String activity, String callingPackage)
        {
            string activityName = activity;
            int pos = activity.IndexOf('/');
            if (pos != -1)
            {
                Logger.Info("AppLaunched: got invalid format for activity: {0}. Fixing it.", activity);
                activityName = activity.Substring(pos + 1);
            }

            if (mLastAppLaunched != package)
            {
                mLastAppLaunched = package;
                mLastAppDisplayed = "";
            }

            Thread thread = new Thread(delegate ()
            {
                mDefaultLauncher = Utils.GetDefaultLauncher();
                Logger.Info("mDefaultLauncher: {0}", mDefaultLauncher);

                if (mDefaultLauncher != "none")
                {
                    TabButtons.Instance.mHomeUnresolved = false;
                }
                else if (activity.Contains(RESOLVERACTIVITY))
                {
                    TabButtons.Instance.mHomeUnresolved = true;
                    if (TabButtons.Instance.SelectedTab.mIsHomeTab == false &&
                        TabButtons.Instance.SelectedTab.TabType == EnumTabType.app)
                        TabButtons.Instance.GoToHomeTab();
                }

                RegistryKey configKey = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.GMConfigPath);
                configKey.SetValue("DefaultLauncher", mDefaultLauncher);
                configKey.Close();

                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    TabButtons.Instance.mHomeTab.mPackageName = mDefaultLauncher;
                    TabButtons.Instance.mHomeTab.mKey = mDefaultLauncher;
                }));
            });
            thread.IsBackground = true;
            thread.Start();
            /*
			 * If we don't wait for the thread to finish, the side effect could be that
			 * the first app that is launched might show in the same tab as home tab.
			 * This will not happen with our launcher but will happen only if the user
			 * changes the default launcher. However, if we wait for the thread to finish,
			 * the UI thread freezes for some time and the user is unable to launch an app
			 * for the first 4-5 seconds.
			 */
            // thread.Join();

            Logger.Info("AppLaunched: {0}/{1}", package, activity);
            Logger.Info("mLastTopPackage: {0}", mLastTopPackage);

            /*
			* This is required for tablet like setup
			* Hardcoded package name because during one time setup default launcher is com.google.android.setupwizard
			* Will not work for any other home launcher
			* */
            if (package == "com.bluestacks.gamepophome" && !ToolBar.Instance.IsOneTimeSetupComplete())
            {
                ToolBar.Instance.EnableOTSButtons(true);
                ToolBar.Instance.EnableToggleAppTabButton(false);
                RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
                configKey.SetValue("OneTimeSetupDone", "yes");

                string welcomePageUrl = (string)configKey.GetValue("welcomePageUrl", string.Empty);
                if (!string.IsNullOrEmpty(welcomePageUrl))
                {
                    GameManagerUtilities.ShowPromotion("campaign", welcomePageUrl);
                }
            }

            if (package == mDefaultLauncher)
            {
                TabButtons.Instance.UserAtHome();
            }

            /*
			 * Special handling for case where user goes to gp from an app other than the default
			 * launcher. This will happen in in-app-purchase scenarios where gp activity is started
			 * by the app itself. We don't want to switch tabs in this case.
			 */
            bool allowSwitch = true;
            if ((mStorePackagesList.Contains(package) || mIAPPackagesList.Contains(package)) && mLastTopPackage != mDefaultLauncher)
            {
                allowSwitch = false;
            }
            TabButton selectedTab = TabButtons.Instance.SelectedTab;
            if (package == mDefaultLauncher && selectedTab.TabType == EnumTabType.app &&
                    selectedTab.mIsHomeTab == true)
            {
                selectedTab.mPackageName = package;
                selectedTab.mActivity = activityName;
            }

            if (TabButtons.Instance.FindAppTab(package) != null)
            {
                Logger.Info("Found tab. package: " + package);
                if (package != mDefaultLauncher && allowSwitch)
                {
                    TabButtons.Instance.GoToTab(package);
                }
                TabButtons.Instance.UpdateTab(package, activityName);
            }

            if (mLastTopPackage == package && selectedTab.TabType == EnumTabType.app &&
                    selectedTab.mIsHomeTab == false)
            {
                return;
            }
            if (package == KEYMAPPINGTOOL || package == APPFINDER || activity.Contains(RESOLVERACTIVITY))
            {
                return;
            }
            if (package == mDefaultLauncher)
            {
                mLastTopPackage = package;
                return;
            }
            if (callingPackage == OTSPACKAGEINFO || mLastTopPackage == OTSPACKAGEINFO)
            {
                return;
            }
            if (activity.Contains(CHOOSERACTIVITY))
            {
                return;
            }
            string appName = Utils.GetAppName(package);
            Logger.Info("Got appName: " + appName);
            /*
             * Handling for case where user clicked on a not-installed app
             * in the Welome tab
             */
            if (package == STOREPACKAGE && mCreateVendingTab == true)
            {
                ShowApp(appName, package, activity, null, false);
                mCreateVendingTab = false;
                return;
            }
            /*
             * In case gsf is launched by our home app,
             * we need to launch it in a new tab
             * irrespective of what the last top package was
             */
            if (package == PLAYLOGINAPPINFO && callingPackage == mDefaultLauncher)
            {
                mLastTopPackage = mDefaultLauncher;
            }
            if (mLastTopPackage == mDefaultLauncher || mStorePackagesList.Contains(mLastTopPackage))
            {
                if (TabButtons.Instance.FindAppTab(package) != null)
                {
                    if (package != TabButtons.Instance.SelectedTab.mKey && allowSwitch)
                    {
                        TabButton tab = TabButtons.Instance.FindAppTab(package);
                        TabButtons.Instance.GoToTab(tab.mKey);
                    }
                }
                else
                {
                    GMAppsManager iam = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
                    bool isInstalled = iam.IsAppInstalled(package);
                    if (callingPackage == mDefaultLauncher ||
                            callingPackage == BSTCMDPROCESSOR ||
                            callingPackage == BSTSERVICES ||
                            mStorePackagesList.Contains(callingPackage))
                    {
                        GMAppInfo info = new GMAppInfo(appName, null, package, activityName, "", "", "");
                        if (!isInstalled)
                        {
                            iam.AddToJson(info);
                            if (Oem.Instance.IsBTVBuild)
                            {
                                BTVManager.CheckNewFiltersAvailable();
                            }
                        }

                        ShowApp(appName, package, activity, null, false);
                    }
                    else
                    {
                        if (isInstalled)
                            ShowApp(appName, package, activity, null, false);
                    }
                }
            }
            /*
			 * handling for cases:
			 * - one time setup launches gp
			 * - user goes back in the activity stack after launching an app from gp
			 *   This second case will break because of the special case handling added above
			 *   Need to figure out a way to handle both cases.
			 */
            else if (mStorePackagesList.Contains(package))
            {
                if (TabButtons.Instance.FindAppTab(package) != null)
                {
                    if (package != TabButtons.Instance.SelectedTab.mKey && allowSwitch)
                    {
                        TabButton tab = TabButtons.Instance.FindAppTab(package);
                        TabButtons.Instance.GoToTab(tab.mKey);
                    }
                }
                else
                {
                    if (callingPackage == BSTCMDPROCESSOR || callingPackage == BSTSERVICES)
                        ShowApp(appName, package, activity, null, false);
                }
            }

            /*In case applaunch comes from notifiaction
			 * lasttoppackage != mDefaultLauncher & lasttoppackage != storepackage
			 * hence this fix*/
            else if (package == callingPackage || mStorePackagesList.Contains(callingPackage))
            {
                TabButton tab = TabButtons.Instance.FindAppTab(package);
                if (tab != null)
                {
                    if (tab != TabButtons.Instance.SelectedTab && allowSwitch)
                    {
                        tab.mLaunchApp = false;
                        TabButtons.Instance.GoToTab(package);
                    }
                }
                else
                {
                    ShowApp(appName, package, activity, null, false);
                }
            }
            mLastTopPackage = package;
        }

        public static void HandleAppDisplayed(string token)
        {
            Logger.Info("HandleAppDisplayed: {0}", token);
            try
            {
                if (token.Contains(Common.Strings.HomeAppPackageName) == true &&
                        UrlToLaunchOnHomeAppDisplayed != null)
                {
                    GMApi.LaunchUrlIntentActivity(UrlToLaunchOnHomeAppDisplayed);
                    UrlToLaunchOnHomeAppDisplayed = null;
                }

                mAppDisplayedOccured = true;
                String[] args = token.Split(' ');
                string appInfo = args[3];

                if (appInfo.Contains(mLastAppLaunched))
                    mLastAppDisplayed = appInfo;

                TabButton selectedTab = TabButtons.Instance.SelectedTab;
                string package = selectedTab.mPackageName;
                if (mLastShownAppInfo != "" && mLastShownAppInfo.Contains(package) &&
                        mLastShownAppInfo.IndexOf(PLAYSTOREAPPINFO) == -1)
                {
                    Logger.Info("Not sending AppDisplayed request for last shown appInfo: " + appInfo);
                    return;
                }

                int pos = token.IndexOf(' ');
                string appToken = token.Substring(pos + 1);
                GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                {
                    bool shown = AppDisplayed(appToken);
                    if (shown)
                    {
                        mLastShownAppInfo = appInfo;
                        #region commented code

                        //Handling RPC Error
                        //Commenting for time being
                        /*if (appInfo.IndexOf("com.android.vending") != -1)
						{
						Logger.Info("Play Store app Displayed");
						bool isExeToBeRun = false;
						Logger.Info("isExeToBeRun" + isExeToBeRun);
						if (isExeToBeRun)
						{
						Logger.Info("Checking if HD-RPCErrorTroubleShooter running");
						if (!Utils.FindProcessByName("HD-RPCErrorTroubleShooter"))
						{
						Logger.Info("HD-RPCErrorTroubleShooter not running");
						//Do polling for RPC Error
						if (false == DoPollingForTroubleShooter("HD-RPCErrorTroubleShooter.exe", isExeToBeRun))
						{
						Logger.Info("Eror in running HD-RPCErrorTroubleShooter");
						}
						}
						else
						{
							Logger.Info("HD-RPCErrorTroubleShooter already running");
						}
						}
						else
						{
							Thread thread = new Thread(delegate()
									{
									if (Monitor.TryEnter(mLockObj))
									{
									try
									{
									//Do polling for RPC Error
									mStopRpcTroubleShooter = false;
									if (false == DoPollingForTroubleShooter("HD-RPCErrorTroubleShooter.exe", isExeToBeRun))
									{
									Logger.Info("Eror in running RPCErrorTroubleShooter");
									}

									}
									finally
									{
									//the lock is released.
									Monitor.Exit(mLockObj);
									}
									}
									else
									{
										Logger.Info("RPCErrorTroubleShooter already running");
									}
									});
							thread.IsBackground = true;
							thread.Start();
						}
						} */
                        /*
							if (appInfo.IndexOf("com.android.vending") != -1)
							{
							Logger.Info("Play Store app Displayed");
							Logger.Info("Checking if HD-RPCErrorTroubleShooter running");
							if (!Utils.FindProcessByName("HD-RPCErrorTroubleShooter"))
							{
							Logger.Info("HD-RPCErrorTroubleShooter not running");
						//Do polling for RPC Error
						if (false == DoPollingForTroubleShooter("HD-RPCErrorTroubleShooter.exe"))
						{
						Logger.Info("Eror in running HD-RPCErrorTroubleShooter");
						}
						}
						else
						{
						Logger.Info("HD-RPCErrorTroubleShooter already running");
						}
						}*/
                        #endregion
                    }
                }));
            }
            catch (Exception exc)
            {
                Logger.Error("GameManager: HandleAppDisplayed: " + exc.ToString());
            }
        }

        internal static void AppUninstalled(string package)
        {
            Logger.Info("AppUninstalled: {0}", package);
            TabButtons.Instance.CloseTab(package);
        }

        public static void ShowApp(String displayName, String package, String activity, String apkUrl, bool launchApp)
        {
            Logger.Info("ShowApp: " + package + "/" + activity + "/" + apkUrl);
            if (TabButtons.Instance.FindAppTab(package) != null)
            {
                TabButtons.Instance.GoToTab(package);
            }
            else
            {
                if (mDontCreateAppTab == true)
                {
                    mDontCreateAppTab = false;
                    if (package.Contains("search::"))
                        SendSearchPlayRequestAsync(package, activity);
                    else
                        SendRunAppRequestAsync(package, activity);
                    return;
                }

                if (package == mDefaultLauncher)
                    return;

                String imageFile = Path.Combine(BlueStacks.hyperDroid.Common.Strings.GameManagerHomeDir, package + ".png");
                if (!File.Exists(imageFile))
                    imageFile = null;
                TabButtons.Instance.AddAppTab(displayName, package, activity, apkUrl, imageFile, true, launchApp);
                TabButtons.Instance.Focus();
            }
        }
        public static bool AppDisplayed(String appToken)
        {
            try
            {
                Logger.Info("AppDisplayed: {0}", appToken);
                TabButton selectedTab = TabButtons.Instance.SelectedTab;
                string toSearch = String.Format("{0}/", selectedTab.mPackageName);
                Logger.Info("toSearch: {0}", toSearch);

                if (mDefaultLauncher == "none" && appToken.Contains(RESOLVERACTIVITY))
                {
                    TabButtons.Instance.mHomeUnresolved = true;
                    if (selectedTab.TabType == EnumTabType.app)
                    {
                        selectedTab.PerformTabAction(false, true);
                        selectedTab.mRunAppRequestPending = false;
                        return true;
                    }
                }

                if (selectedTab.TabType == EnumTabType.app)
                {
                    if (selectedTab.mIsHomeTab == true && appToken.Contains(RESOLVERACTIVITY))
                    {
                        selectedTab.PerformTabAction(false, true);
                        selectedTab.mRunAppRequestPending = false;
                        return true;
                    }

                    Logger.Info("mDefaultLauncher: " + mDefaultLauncher);
                    if (selectedTab.mIsHomeTab == true &&
                            selectedTab.TabType == EnumTabType.app &&
                            (selectedTab.mPackageName == GAMEPOPHOME ||
                             selectedTab.mPackageName == mDefaultLauncher) &&
                            (appToken.Contains(GAMEPOPHOME) ||
                             appToken.Contains(mDefaultLauncher)))
                    {
                        selectedTab.PerformTabAction(false, true);
                        selectedTab.mRunAppRequestPending = false;
                        //selectedTab.TakeScreenshot();
                        return true;
                    }

                    if (appToken.Contains(toSearch) ||
                            appToken.IndexOf(PLAYSTOREAPPINFO) != -1 ||
                            appToken.IndexOf(PLAYLOGINAPPINFO) != -1 ||
                            appToken.IndexOf(PLAYLOCATIONINFO) != -1)
                    {
                        selectedTab.PerformTabAction(false, true);
                        selectedTab.mRunAppRequestPending = false;
                        return true;
                    }
                }

            }
            catch (Exception e)
            {
                // A very small window, but if the tab was closed after getting the
                // selected tab from tabBar and before doing further operations, this will happen.
                // No way to prevent it, so ignore it...
                Logger.Error("Failed in AppDisplayed. Ignoring. Error: " + e.ToString());
            }

            return false;
        }

        private static void SendSearchPlayRequestAsync(string package, string activity)
        {
            Thread thread = new Thread(delegate ()
            {
                if (package.Contains("search::"))
                {
                    package = package.Remove(0, 8);
                }
                string cmd = String.Format("searchplay {0}", package);
                Logger.Info("Package: " + package + "Activity: " + activity);
                Common.VmCmdHandler.RunCommand(cmd);
            });

            thread.IsBackground = true;
            thread.Start();
        }

        public static void SendRunAppRequestAsync(string package, string activity)
        {
            Thread thread = new Thread(delegate ()
            {
                string cmd = String.Format("runex {0}/{1}", package, activity);
                Common.VmCmdHandler.RunCommand(cmd);
            });

            thread.IsBackground = true;
            thread.Start();
        }


    }
}
