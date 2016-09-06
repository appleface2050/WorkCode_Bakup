using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Web;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for TabButtons.xaml
    /// </summary>
    public partial class TabButtons : UserControl
    {
        List<TabButton> mTabHistory = new List<TabButton>();
        public static TabButtons Instance = null;
        static TabButton mSelectedTab = null;
        public bool mHomeUnresolved = false;
        internal bool mRotateInProgress = false;
        public bool mAppDisplayed = false;

        public string mLastAppTabName = "";

        public TabButton mHomeTab = null;

        public TabButton SelectedTab
        {
            get
            {
                if (mSelectedTab == null)
                {
                    return mDictTabs.ElementAt(0).Value;
                }
                return mSelectedTab;
            }
            set
            {
                if (mSelectedTab != null)
                {
                    mSelectedTab.IsSelected = false;
                }
                mSelectedTab = value;
                mSelectedTab.IsSelected = true;
                AppHandler.mLastShownAppInfo = string.Empty;

                if (mTabHistory.Contains(mSelectedTab))
                {
                    mTabHistory.Remove(mSelectedTab);
                }
                mTabHistory.Add(mSelectedTab);

                TopBar.Instance.ToggleButtons();
                string[] tabChangedData = new string[3];
                tabChangedData[0] = mSelectedTab.TabType.ToString();
                tabChangedData[1] = mSelectedTab.mAppName.Text;

                if (mSelectedTab.TabType == EnumTabType.web)
                {
                    tabChangedData[2] = mSelectedTab.mWebUrl;
                    WebTabSelected();
                }
                else
                {
                    tabChangedData[2] = mSelectedTab.mPackageName;
                    AppTabSelected();
                }

                if (Oem.Instance.IsStreamWindowEnabled)
                {
                    if (Utils.FindProcessByName("BlueStacksTV"))
                        BTVManager.SendTabChangeData(tabChangedData);
                }

                Logger.Info("HandleTabChanged. done SelectedIndex: " + TabGrid.ColumnDefinitions.IndexOf(mSelectedTab.ColumnDefinition));
            }
        }

        static RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\BlueStacks");
        static string programDataDir = (string)regKey.GetValue("DataDir");
        static string configDir = System.IO.Path.Combine(programDataDir, @"UserData\InputMapper");

        public Dictionary<string, TabButton> mDictTabs = new Dictionary<string, TabButton>();
        public TabButtons()
        {
            Instance = this;
            InitializeComponent();
            this.MouseLeftButtonDown += TabButtons_MouseLeftButtonDown;

        }

        private void TabButtons_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            GameManagerWindow.Instance.DragMove();
        }

        public void GoToHomeTab()
        {
            Logger.Info("Going to home");
            this.GoToTab(mHomeTab.mKey);
        }
        public void UserAtHome()
        {
            Logger.Info("User reached home");
            try
            {
                TabButton tab = this.SelectedTab;
                Logger.Info("mRunAppRequestPending: {0}", tab.mRunAppRequestPending);

                if (tab.mRunAppRequestPending == false)
                {
                    GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
                    {
                        this.SelectedTab.Close();
                    }));
                }
            }
            catch (Exception e)
            {
                // A very small window, but if the tab was closed after getting the
                // selected tab from tabBar and before doing further operations, this will happen.
                // No way to prevent it, so ignore it...
                Logger.Error("Failed in UserAtHome. Ignoring. Error: " + e.ToString());
            }
        }

        private void WebTabSelected()
        {
            ContentControl.Instance.BringToFront(mSelectedTab.ControlGrid, true);
            ToolBar.Instance.EnableAppTabButtons(false);
            mAppDisplayed = false;

            if (mSelectedTab.mBrowser != null)
            {
                mSelectedTab.mBrowser.Focus();
            }
            else
            {
                Logger.Error("TabBar: selectedTab browser is null. No window to show");
            }
        }

        private void AppTabSelected()
        {
            if (ToolBar.Instance.IsOneTimeSetupComplete())
                ToolBar.Instance.EnableGenericAppTabButtons(true);
            if (GameManagerUtilities.ToggleDisableAppList.Contains(mSelectedTab.mPackageName) || mSelectedTab.mPackageName == AppHandler.OTSPACKAGEINFO)
            {
                ToolBar.Instance.EnableToggleAppTabButton(false);
            }
            else
            {
                ToolBar.Instance.EnableToggleAppTabButton(true);
            }

            if (mSelectedTab.mIsHomeTab)
            {
                Thread thread = new Thread(delegate ()
                {
                    Common.VmCmdHandler.RunCommand("home");
                });

                thread.IsBackground = true;
                thread.Start();
            }

            PerformTabAction();
            mLastAppTabName = mSelectedTab.mKey;

            string configFile = System.IO.Path.Combine(configDir, mSelectedTab.mPackageName + ".cfg");
            string userConfigFile = System.IO.Path.Combine(configDir, "UserFiles\\" + mSelectedTab.mPackageName + ".cfg");

            if (mSelectedTab.mIsHomeTab)
            {
                TopBar.Instance.DisableKeyMappingButton();
            }
            else if (File.Exists(configFile) || File.Exists(userConfigFile))
            {
                TopBar.Instance.GlowKeyMappingButton();
            }
            else
            {
                TopBar.Instance.ShowDefaultKeyMappingButton();
            }

            ContentControl.Instance.BringFrontendInFront();
        }

        private void PerformTabAction()
        {
            bool relaunch, show;
            Logger.Info("mLastAppTabName: {0}, selectedTab.key: {1}, selectedTab.mLaunchApp: {2}",
                    mLastAppTabName, mSelectedTab.mKey, mSelectedTab.mLaunchApp);

            if (mLastAppTabName == mSelectedTab.mKey)
            {
                relaunch = false;
                show = true;
            }
            else if (mSelectedTab.mLaunchApp == false)
            {
                relaunch = false;
                show = false;
            }
            else
            {
                relaunch = true;
                show = false;
            }

            if (mHomeUnresolved || mSelectedTab.mPackageName == "none")
            {
                if (mSelectedTab.mIsHomeTab)
                {
                    relaunch = false;
                    show = true;
                }
            }

            //It is possible that we get AppDisplayed for the launched app even before
            //its tab is created and the tab action performed.
            // mLastAppDisplayed will save the AppDisplayed data, if any, for the last
            // launched app.
            // We will check this var while creating a tab and show frontend
            // without waiting for AppDisplayed in case it matches the tab package.
            if (AppHandler.mLastAppDisplayed.Contains(mSelectedTab.mPackageName))
                show = true;

            // We want to switch to android when the location popup is visible,
            // irrespective of what tab the user switches to
            if (AppHandler.mLastAppDisplayed.Contains(AppHandler.PLAYLOCATIONINFO))
            {
                show = true;
            }

            mSelectedTab.mLaunchApp = true;
            mSelectedTab.PerformTabAction(relaunch, show);
        }
        public TabButton AddWebTab(string label, string url, string imagePath, bool switchToThis)
        {

            TabButton newTab = new TabButton();
            if (url != null)
            {
                if (Oem.Instance.IsTabsEnabled)
                {
                    newTab.Initialize(label, url, true, imagePath);

                    Browser browser = new Browser(url);
                    browser.mParentTab = newTab;
                    browser.Dock = System.Windows.Forms.DockStyle.Fill;
                    newTab.mBrowser = browser;

                    newTab.ControlGrid = ContentControl.Instance.AddControl(browser);
                    this.AddTab(newTab, switchToThis);
                }
                else
                {
                    OpenURlInDefaultBrowser(url);
                }
            }
            return newTab;
        }

        private static void OpenURlInDefaultBrowser(string url)
        {
            try
            {
                if (url.Contains("http://") || url.Contains("https://"))
                    Process.Start(url);
                else

                    Process.Start("http://" + url);
            }
            catch (Win32Exception)
            {
                Process.Start("IExplorer.exe", url);
            }
        }

        private void AddTab(TabButton newTab, bool switchToThis)
        {
            if (mDictTabs.ContainsKey(newTab.mKey))
            {
                this.SelectedTab = mDictTabs[newTab.mKey];
            }
            else
            {
                mDictTabs.Add(newTab.mKey, newTab);

                ColumnDefinition colDef = new ColumnDefinition();
                colDef.Width = new GridLength(1, GridUnitType.Star);
                colDef.MaxWidth = 140;
                TabGrid.ColumnDefinitions.Add(colDef);

                //TabGrid.ColumnDefinitions.Remove(EmptyColumn);//To keep the empty column in left
                //TabGrid.ColumnDefinitions.Add(EmptyColumn);

                TabGrid.Children.Add(newTab);
                newTab.ColumnDefinition = colDef;
                Grid.SetColumn(newTab, TabGrid.ColumnDefinitions.Count - 1);

                if (switchToThis)
                {
                    SelectedTab = newTab;
                }
            }
        }

        internal void RemoveTab(TabButton tabButton)
        {
            mDictTabs.Remove(tabButton.mKey);
            ColumnDefinition colDef = tabButton.ColumnDefinition;
            int index = TabGrid.ColumnDefinitions.IndexOf(colDef);
            foreach (TabButton item in mDictTabs.Values)
            {
                int itemIndex = int.MinValue;
                if (TabGrid.Children.Contains(item))
                {
                    itemIndex = Grid.GetColumn(item);
                }
                if (itemIndex > index)
                {
                    Grid.SetColumn(item, itemIndex - 1);
                }
            }
            TabGrid.ColumnDefinitions.Remove(colDef);
            TabGrid.Children.Remove(tabButton);
            mTabHistory.Remove(tabButton);
            if (SelectedTab == tabButton)
            {
                this.SelectedTab = mTabHistory[mTabHistory.Count - 1];
            }
        }

        internal TabButton AddErrorTab(string name, string reason)
        {
            string errorMessage = "Sorry! " + name + " could not be installed";
            string param = String.Format(@"?error={0}&reason={1}", errorMessage, reason);

            string baseUrl = String.Format("http://localhost:{0}/static/themes", GameManagerUtilities.GameManagerPort);
            string themeUrl = String.Format("{0}/{1}/", baseUrl, GameManagerUtilities.sDefaultTheme);
            string url = themeUrl + "install-error.html" + param;
            /*
			   string url = String.Format(@"file:///{0}\install-error.html", mGameManager.GetCurrentThemeLocalDir());
			   url = url + HttpUtility.UrlEncode(param);
			   */
            return AddWebTab("Install Failed", url, null, true);
        }
        public TabButton AddDownloadTab(String label, String url, String imagePath, bool switchToThis, string e_url)
	{
		return AddDownloadTab(label, null, url, imagePath, switchToThis, e_url, false);
	}
	public TabButton AddDownloadTab(String label, String package, String url, String imagePath, bool switchToThis, string e_url, bool installApk)
        {
            TabButton newTab = new TabButton();
            if (url != null)
            {
                if (Oem.Instance.IsTabsEnabled)
                {
                    Logger.Info("Downloading " + e_url);

                    if (package != null)
                        newTab.Initialize(label, url, true, imagePath, package);
                    else
                        newTab.Initialize(label, url, true, imagePath);

                    Browser browser = new Browser(url);
                    browser.mParentTab = newTab;
                    browser.Dock = System.Windows.Forms.DockStyle.Fill;
                    newTab.mBrowser = browser;

                    if (installApk)
                        browser.InstallApp(label, e_url);
                    else
                        browser.AddDownloadHandler(e_url, newTab.mKey);

                    newTab.ControlGrid = ContentControl.Instance.AddControl(browser);

                    this.AddTab(newTab, switchToThis);
                }
                else
                {
                    OpenURlInDefaultBrowser(url);
                }

            }
            return newTab;
        }
        internal void AddAllDownloadTab(string label, string url, string imagePath, bool switchToThis, string e_url, string filePath)
        {
            if (url != null)
            {
                if (Oem.Instance.IsTabsEnabled)
                {
                    Logger.Info("Downloading " + e_url);

                    TabButton newTab = new TabButton();
                    newTab.Initialize(label, url, true, imagePath);

                    Browser browser = new Browser(url);
                    browser.mParentTab = newTab;
                    browser.Dock = System.Windows.Forms.DockStyle.Fill;
                    newTab.mBrowser = browser;

                    browser.AddAllDownloadHandler(e_url, filePath, newTab.mKey);

                    newTab.ControlGrid = ContentControl.Instance.AddControl(browser);

                    this.AddTab(newTab, switchToThis);
                }
                else
                {
                    OpenURlInDefaultBrowser(url);
                }
            }
        }
        internal TabButton AddAppTab(string displayName, string packageName, string activityName, string apkUrl, string imagePath, bool switchToThis, bool launchApp)
        {
            String appName = displayName;

            if (displayName == null)
            {
                try
                {
                    appName = JsonParser.GetAppNameFromPackageActivity(packageName, activityName);
                    if (appName == String.Empty)
                    {
                        appName = packageName;
                    }
                }
                catch (Exception)
                {
                    appName = packageName;
                }
            }

            if (String.Equals(packageName, "com.bluestacks.home", StringComparison.InvariantCultureIgnoreCase))
            {
                appName = "One Time Setup";
            }

            appName = appName.Replace("&", "&&");



            TabButton newTab = new TabButton();
            newTab.Initialize(appName, packageName, activityName, imagePath, launchApp);

            this.AddTab(newTab, switchToThis);

            return newTab;
        }

        internal void AddStartupTabs()
        {
            AddHtmlTab();
            AddAndroidTab(true);

            if (Oem.Instance.IsShowBTVViewTab)
            {
                ShowViewTab();
            }
        }

        private void AddHtmlTab()
        {
            if (Oem.Instance.IsWelcomeTabEnabled)
            {
                string name = Locale.Strings.GetLocalizedString("Welcome");
                RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
                string url = (string)urlKey.GetValue("gmHomeUrl", "");

                if (String.Compare(url, "") == 0)
                {
                    url = (string)urlKey.GetValue("welcomePageUrl", "");
                }

                if (String.Compare(url, "") == 0)
                {
                    url = Oem.Instance.HomeTabUrl;
                }

                TabButton tab = AddWebTab(name, url, null, true);
                tab.IsTabClosable = false;
                tab.mIsHomeTab = true;
            }
        }

        private void AddAndroidTab(bool switchToTab)
        {
            ContentControl.Instance.AddAndroid();
            TabButton tab = null;
            if (String.Compare(GameManagerUtilities.sHomeType, "gphome", true) == 0)
            {
                string name = Locale.Strings.GetLocalizedString("Android");
                string androidIconPath = "android_icon.png";
                if (Oem.Instance.IsLoadBluestacksLogoForAndroidTab)
                {
                    RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
                    string installDir = (string)reg.GetValue("InstallDir");
                    androidIconPath = System.IO.Path.Combine(installDir, "ProductLogo.png");
                }
                tab = AddAppTab(name, AppHandler.mDefaultLauncher, ".Main",
                        null, androidIconPath, switchToTab, true);
                tab.IsTabClosable = false;
                mHomeTab = tab;

                tab.PerformTabAction(false, false);
            }
            else if (String.Compare(GameManagerUtilities.sHomeType, "html", true) == 0)
            {
                RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
                string url = (string)urlKey.GetValue("gmHomeUrl", "");

                if (String.Compare(url, "") == 0)
                {
                    url = GameManagerUtilities.mHomeUrl;
                }
                tab = AddWebTab("Home", url, null, switchToTab);
            }
            else
            {
                MessageBox.Show("Invalid homeType. Aborting");
                Environment.Exit(-1);
            }

            tab.mIsHomeTab = true;
            if (switchToTab)
            {
                this.SelectedTab = tab;
            }
			ContentControl.Instance.ShowWaitControl();
            TopBar.Instance.ToggleButtons();
        }

        internal void ShowWebPage(string label, string url, string imagePath)
        {
            Logger.Info("ShowWebPage: " + label + "/" + url);
            if (this.FindWebTab(label) != null)
            {
                this.GoToTab(label);
                StreamViewTimeStats.HandleStreamViewStatsEvent(this.SelectedTab.mAppName.Text, StreamViewStatsEventName.TabVisibleStart);
            }
            else
            {
                this.AddWebTab(label, url, imagePath, true);
            }
        }
        public void GoToTab(string tabName)
        {
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                Logger.Info("GoToTab: " + tabName);
                if (SelectedTab.mKey != tabName && mDictTabs.ContainsKey(tabName))
                {
                    SelectedTab = mDictTabs[tabName];
                    SelectedTab.Focus();
                    Logger.Info("this.SelectedName: " + SelectedTab.mKey);
                }
            }));
        }

        public void GoToTab(int index)
        {
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                Logger.Info("GoToTab: " + index);
                if (mDictTabs.Count >= index && mDictTabs.ElementAt(index).Value.mKey != SelectedTab.mKey)
                {
                    SelectedTab = mDictTabs.ElementAt(index).Value;
                    SelectedTab.Focus();
                    Logger.Info("this.SelectedName: " + SelectedTab.mKey);
                }
            }));
        }

        public TabButton FindAppTab(string package)
        {
            if (mDictTabs.Keys.Contains(package))
            {
                return mDictTabs[package];
            }
            return null;
        }

        public TabButton FindWebTab(string label)
        {
            if (mDictTabs.Keys.Contains(label))
            {
                return mDictTabs[label];
            }
            return null;
        }

        internal void CloseTab(string tabName)
        {
            TabButton tab = FindAppTab(tabName);
            if (tab != null && tab.IsTabClosable)
            {
                tab.Close();
            }
        }

        internal string[] GetCurrentTabData()
        {
            TabButton selectedTab = SelectedTab;

            string[] currentTabData = new string[3];
            currentTabData[0] = selectedTab.TabType.ToString();
            currentTabData[1] = selectedTab.mAppName.Text;
            if (selectedTab.TabType == EnumTabType.web)
            {
                currentTabData[2] = selectedTab.mWebUrl;
            }
            else if (selectedTab.TabType == EnumTabType.web)
            {
                currentTabData[2] = selectedTab.mPackageName;
            }

            return currentTabData;
        }

        internal void HandleRotate()
        {
            mRotateInProgress = true;
        }

        internal void UpdateTab(string package, string activityName)
        {
            if (mDictTabs.ContainsKey(package))
            {
                TabButton tab = mDictTabs[package];
                if (tab.mActivity != "S2P")
                {
                    tab.mActivity = activityName;
                }
            }
        }

        internal void RelaunchApp(string displayName, string package, string activity, string apkUrl)
        {
            this.SelectedTab.Close();
            AppHandler.ShowApp(displayName, package, activity, apkUrl, true);
        }

        internal void ShowViewTab()
        {
            string name = "BlueStacks TV";
            string url = "http://bluestacks-tv-prod.appspot.com/web";

            if (FindWebTab(name) != null)
            {
                GoToTab(name);
            }
            else
            {
                TabButton tab = AddWebTab(name, url, null, false);
                tab.mIsHomeTab = false;
            }
        }
    }
}
