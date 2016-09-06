using System;
using System.Collections;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;

namespace BlueStacks.hyperDroid.GameManager
{
	public class TabBar : CustomTabControl
	{
		[DllImport("winmm.dll")]
		public static extern int waveOutGetVolume(IntPtr h, out uint dwVolume);

		[DllImport("winmm.dll")]
		public static extern int waveOutSetVolume(IntPtr h, uint dwVolume);


		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int wMsg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int SetFocus(IntPtr hWnd);

		private	uint mSavedVolumeLevel = 0xFFFFFFFF;

		public static TabBar sTabBar;
		public GameManager mGameManager;

		public bool mAppDisplayed = false;
		public bool mHomeUnresolved = false;
		public Tab mCurrentTab = null;
		public string mLastAppTabName = "";
		public Stack mTabHistory;


		private bool mRotateInProgress = false;

		private const int WM_KEYDOWN = 0x0100;
		private const int WM_KEYUP = 0x0101;
		private const int VK_TAB = 0x09;


		private IntPtr mCurrentParentHandle = IntPtr.Zero;
		private bool mFrontendMuted = false;

		private IntPtr mFrontendHandle {
			get { return mGameManager.mFrontendHandle; }
			set { mGameManager.mFrontendHandle = value; }
		}

		private ImageList mImageList;

		private int mTabBorderWidth = 0;

		public Tab GetCurrentTab()
		{
			return mCurrentTab;
		}

		public string GetCurrentTabType()
		{
			return mCurrentTab.mTabType;
		}

		public TabBar(GameManager manager)
		{
			mGameManager = manager;
			sTabBar = this;

			if (!Utils.IsOSWinXP())
			{
				waveOutSetVolume(IntPtr.Zero, mSavedVolumeLevel);
			}

			mTabHistory = new Stack();

			//this.MouseClick += HandleTabMouseClick;
			this.SelectedIndexChanged += SelectedIndexChangedHandler;
			this.Deselecting += HandleTabDeselecting;
			this.TabClosing += HandleTabClosing;
			this.KeyDown += HandleKeyDown;
			this.GotFocus += new System.EventHandler(this.FocusHandler);

			int tabBarHeight = manager.GetTabBarHeight();
			float fontSize = (float) (tabBarHeight * 10)/20;
			if (fontSize < 11)
			{
				fontSize = 11;
			}
			FontFamily family = GameManager.sFontCollection.Families[0];
			this.Font = new Font(family, fontSize, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
			mImageList = new ImageList();
			this.ImageList = mImageList;
			//we need to specify the color depth and the imagesize, as by default the ImageList takes the lowest possible values
			this.ImageList.ColorDepth = ColorDepth.Depth32Bit;
			ImageList.ImageSize = new Size(50,50);
			this.ShowToolTips = true;

			string parentStyleTheme = "Em";
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", parentStyleTheme);

			if (parentStyleTheme != "Em")
			{
				mTabBorderWidth = 1;
			}
		}

		public Tab AddEmptyTab()
		{
			string label = Guid.NewGuid().ToString().Replace("-", string.Empty).Substring(0, 8);
			string url = "http://www.test.com";

			Tab newTab = new Tab(label, url);
			newTab.Text = label;
			newTab.Size = new Size(1, 1);

			Random randonGen = new Random();
			Color randomColor = Color.FromArgb(randonGen.Next(255), randonGen.Next(255), 
					randonGen.Next(255));
			newTab.BackColor = randomColor;

			this.TabPages.Add(newTab);
			AdjustTabWidth();

			return newTab;
		}

		private void FocusHandler(object sender, EventArgs e)
		{
			if (FrontendVisible())
			{
				Logger.Info("setting focus to frontend window");
				SetFocus(mFrontendHandle);
			}
		}
		public Tab AddAllDownloadTab(String label, String url, String imagePath, bool switchToThis, string e_url, string filePath, int parentTabIndex)
		{
			if (url == null)
				return null;

			Tab newTab = new Tab(label, url);
			newTab.Text = label;
			newTab.Size = new Size(1, 1);;

			if (imagePath != null)
			{
				String imageKey = Path.GetFileName(imagePath);
				mImageList.Images.Add(imageKey, Image.FromFile(imagePath));
				newTab.ImageKey = imageKey;
			}

			Browser browser = new Browser(url);
			browser.AddAllDownloadHandler(e_url, filePath, newTab.Name, parentTabIndex);
			browser.Dock = DockStyle.Fill;
			newTab.Controls.Add(browser);
			newTab.mBrowser = browser;

			this.TabPages.Add(newTab);
			AdjustTabWidth();
			if (switchToThis)
			{
				this.SelectedTab = newTab;
				mTabHistory.Push(newTab);
			}

			return newTab;
		}

		public Tab AddDownloadTab(String label, String url, String imagePath, bool switchToThis, string e_url, int parentTabIndex)
		{
			if (url == null)
				return null;

			Logger.Info("Downloading " + e_url);

			Tab newTab = new Tab(label, url);
			newTab.Text = label;
			newTab.Size = new Size(1, 1);;

			if (imagePath != null)
			{
				String imageKey = Path.GetFileName(imagePath);
				mImageList.Images.Add(imageKey, Image.FromFile(imagePath));
				newTab.ImageKey = imageKey;
			}

			Browser browser = new Browser(url);
			browser.mParentTab = newTab;

			if (parentTabIndex == -1)
				browser.InstallApp(label, e_url);
			else
				browser.AddDownloadHandler(e_url, newTab.Name, parentTabIndex);

			browser.Dock = DockStyle.Fill;
			newTab.Controls.Add(browser);
			newTab.mBrowser = browser;

			this.TabPages.Add(newTab);
			AdjustTabWidth();
			if (switchToThis)
			{
				this.SelectedTab = newTab;
				mTabHistory.Push(newTab);
			}
			return newTab;
		}

		public Tab AddWebTab(String label, String url, String imagePath, bool switchToThis)
		{
			if (url == null)
				return null;

			Tab newTab = new Tab(label, url);
			newTab.Text = label;
			newTab.Size = new Size(1, 1);;

			if (imagePath != null)
			{
				String imageKey = Path.GetFileName(imagePath);
				mImageList.Images.Add(imageKey, Image.FromFile(imagePath));
				newTab.ImageKey = imageKey;
			}

			Browser browser = new Browser(url);
			browser.mParentTab = newTab;
			browser.Dock = DockStyle.Fill;
			newTab.Controls.Add(browser);
			newTab.mBrowser = browser;

			this.TabPages.Add(newTab);
			AdjustTabWidth();
			if (switchToThis)
			{
				this.SelectedTab = newTab;
				mTabHistory.Push(newTab);
			}

			return newTab;
		}

		public Tab AddAppTab(String displayName, String packageName, String activityName, String apkUrl,
				String imagePath, bool switchToThis, bool launchApp)
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
			Tab newTab = new Tab(appName, packageName, activityName, apkUrl);
			//Replacing newline with space
			appName = appName.Replace('\n', ' ');
			newTab.Text = appName;
			newTab.Size = new Size(1, 1);;
			newTab.mLaunchApp = launchApp;

			if (imagePath != null)
			{
				String imageKey = Path.GetFileName(imagePath);
				mImageList.Images.Add(imageKey, Image.FromFile(imagePath));
				newTab.ImageKey = imageKey;
			}

			this.TabPages.Add(newTab);
			AdjustTabWidth();
			if (switchToThis)
			{
				this.SelectedTab = newTab;
				mTabHistory.Push(newTab);
			}

			return newTab;
		}

		public Tab AddErrorTab(String name, String reason)
		{
			string errorMessage = "Sorry! " + name + " could not be installed";
			string param = String.Format(@"?error={0}&reason={1}", errorMessage, reason);

			string baseUrl			= String.Format("http://localhost:{0}/static/themes", GameManager.sGameManagerPort);
			string themeUrl			= String.Format("{0}/{1}/", baseUrl, GameManager.sDefaultTheme);
			string url			= themeUrl + "install-error.html" + param;
			/*
			   string url = String.Format(@"file:///{0}\install-error.html", mGameManager.GetCurrentThemeLocalDir());
			   url = url + HttpUtility.UrlEncode(param);
			   */

			return AddWebTab("Install Failed", url, null, true);
		}

		public void AdjustTabWidth()
		{
			if (this.TabPages.Count > 0)
			{
				int tabWidth = (this.Parent.Width - mGameManager.mControlBarRight.Width - 5) / (this.TabPages.Count);

				if (tabWidth > mGameManager.mTabWidth)
				{
					tabWidth = mGameManager.mTabWidth;
				}
				if (tabWidth < 0)
				{
					tabWidth = 20;
				}
				this.ItemSize = new Size(tabWidth, GameManager.sTabBarHeight + 1); //+1 for the bottom darkline of the tabbar
			}
		}

		public void GoToTab(Tab tab)
		{
			UIHelper.RunOnUIThread(this, delegate() {
					this.SelectedTab = tab;
					});
		}

		public void GoToTab(int index)
		{
			UIHelper.RunOnUIThread(this, delegate() {
					Logger.Info("GoToTab: " + index);
					if (this.SelectedIndex == index)
						return;

					this.SelectedIndex = index;
					Logger.Info("this.SelectedIndex: " + this.SelectedIndex);
			});
		}

		public bool FindAppTab(String package, out int index)
		{
			return ((index = this.TabPages.IndexOfKey(String.Format("app:{0}", package))) != -1);
		}

		public bool FindWebTab(String label, out int index)
		{
			return ((index = this.TabPages.IndexOfKey(String.Format("web:{0}", label))) != -1);
		}

		public bool IsCurrentTab(Tab tab)
		{
			return (this.SelectedTab == tab);
		}

		public void UpdateTab(int index, String activity)
		{
			Tab tab = (Tab)this.TabPages[index];

			if (tab.mActivity == "S2P")
				return;

			tab.mActivity = activity;
		}

		private void HandleTabClosing(Object sender, TabControlCancelEventArgs e)
		{
			Logger.Info("HandleTabClosing. Index: " + e.TabPageIndex);
			Tab tab = (Tab)e.TabPage;
			if (!(tab.mTabType == "web" && tab.mWebUrl == "Installer"))
			{
				CloseTab(e.TabPageIndex);
			}
			e.Cancel = true;
		}

		public void CloseCurrentTab()
		{
			CloseTab(this.SelectedIndex);
		}

		public void CloseAppTab(string package)
		{
			int index;
			FindAppTab(package, out index);
			if (index != -1)
			{
				CloseTab(index);
			}
		}

		public void CloseTab(int index)
		{
			Logger.Info("Closing tab index: " + index);
			TabPage tabPage = this.TabPages[index];
			if (tabPage != null)
			{
				Tab tab = (Tab)tabPage;
				CloseTab(tab);
			}
		}

		public void CloseTab(String key)
		{
			Logger.Info("Closing tab for key: " + key);
			TabPage tabPage = this.TabPages[key];
			if (tabPage != null)
			{
				Tab tab = (Tab)tabPage;
				CloseTab(tab);
			}
		}

		public void CloseTab(Tab tab)
		{
			if (tab != null && tab.mBrowser != null && tab.mBrowser.webClient != null)
			{
				tab.mBrowser.webClient.CancelAsync();
			}

			Logger.Info("Closing " + tab.Name);
			if (tab.mIsHome)
			{
				Logger.Info("Not closing");
				return;
			}

			if (tab.mTabType == "web" && tab.mLabel != null)
				StreamViewTimeStats.HandleStreamViewStatsEvent(tab.mLabel,
						StreamViewStatsEventName.TabCloseSessionEnd);

			if (tab.Handle == mCurrentParentHandle)
			{
				UnsetParentFrontend();
			}
			else
			{
				Logger.Info("Not unsetting for index = " + this.TabPages.IndexOf(tab) + ", currentIndex = "
						+ this.SelectedIndex);
			}

			for (int i=0; i<tab.Controls.Count; i++)
			{
				Control control = tab.Controls[i];
				if (control is Browser)
				{
					if (control.Name == Tab.sWaitBrowserName)
					{
						Logger.Info("Removing wait browser from " + tab.Name);
						tab.Controls.Remove(control);
					}
					else
					{
						control.Dispose();
						tab.Controls.Remove(control);
						control = null;
					}
				}
				else if (control is PictureBox)
				{
					Logger.Info("Removing picturebox from " + tab.Name);
					tab.Controls.Remove(control);
				}
				else
				{
					control.Dispose();
					tab.Controls.Remove(control);
					control = null;
				}
			}

			if (tab.mScreenshot != null)
			{
				if (tab.mScreenshot.Image != null)
				{
					Logger.Info("Disposing picturebox image from " + tab.Name);
					tab.mScreenshot.Image.Dispose();
				}
				tab.mScreenshot.Dispose();
				tab.mScreenshot = null;
			}

			if (tab.mHasLock)
			{
				Logger.Info("Setting event");
				GameManager.sAppInstallEvent.Set();
			}

			if (tab.mIsDownloading)
			{
				tab.mIsTabClosing = true;
				tab.AbortApkDownload();
			}

			try
			{
				if (tab.Name == this.SelectedTab.Name &&
						mTabHistory.Count > 1 && this.TabPages.Count > 1)
				{
					mTabHistory.Pop();
					Tab lastTab = (Tab)mTabHistory.Pop();
					while (this.TabPages.IndexOf(lastTab) == -1 ||
							lastTab.Name == tab.Name)
					{
						lastTab = (Tab)mTabHistory.Pop();
					}
					this.SelectedTab = lastTab;
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}

			if (mLastAppTabName == tab.Name)
				mLastAppTabName = "";

			if (tab.mTabType.Equals("app") && mRotateInProgress == false)
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
				if (key != null)
				{
					if (Oem.Instance.IsAppToBeForceKilledOnTabClose)
					{
						Thread thread = new Thread(delegate()
								{
                                    Tab tt = tab;
                                    if (null != tt)//sometime the tab value is null, I do not know why
                                    {
                                        HTTPHandler.StopAppRequest(tt.mPackage);
                                    }
								});
						thread.IsBackground = true;
						thread.Start();
					}
				}
			}
			else if (mRotateInProgress == true)
			{
				mRotateInProgress = false;
			}

			if (tab.mTabType.Equals("web"))
			{
				if (GameManager.sGameManager.mStreamWindow != null)
					GameManager.sGameManager.mStreamWindow.ClosingWebTab(tab.Text);
			}

			//If Tab Name is Play Store then Kill RPC TroubleShooter process
			if (tab != null && (tab.Name == Utils.GetAppName("com.android.vending")))
			{
				GameManager.sGameManager.mStopRpcTroubleShooter = true;
				Utils.KillProcessByName("HD-RPCErrorTroubleShooter");
			}

			this.TabPages.Remove(tab);
			tab.Dispose();
			tab = null;

			//handling the situation where Selected index was not resequencing after tab removal and we were getting index out of bounds error:
			if (this.SelectedIndex >= this.TabPages.Count)
			{
				this.SelectedIndex = 0;
			}

			AdjustTabWidth();
		}

		public void HandleKeyDown(Object sender, KeyEventArgs e)
		{
			if (e.KeyCode == Keys.Left || e.KeyCode == Keys.Right)
			{
				e.Handled = true;
				return;
			}
		}

		protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
		{
			if (keyData == Keys.Tab)
			{
				SendMessage(mFrontendHandle, WM_KEYDOWN, (IntPtr)keyData, (IntPtr)0);
				SendMessage(mFrontendHandle, WM_KEYUP, (IntPtr)VK_TAB, (IntPtr)0);
				return true;
			}
			return base.ProcessCmdKey(ref msg, keyData);
		}

		private void HandleTabDeselecting(Object sender, TabControlCancelEventArgs e)
		{
			Logger.Info("HandleTabDeselecting. Index: " + e.TabPageIndex);

			Tab tab = (Tab)(e.TabPage);
			if (tab != null && tab.mTabType == "app")
			{
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
                {
                    if (!mGameManager.FullScreen)
                    {
                        tab.DeselectedGMClientSize = mGameManager.ClientSize;
                    }
                }
				if (e.TabPageIndex != 0)
					tab.TakeScreenshot();

				if (tab.mScreenshot != null)
					tab.mScreenshot.Show();
			}
			else if (tab != null && tab.mTabType == "web")
			{
				StreamViewTimeStats.HandleStreamViewStatsEvent(tab.mLabel,
						StreamViewStatsEventName.TabVisibleEnd);
			}
		}

		public void SelectedIndexChangedHandler(Object sender, EventArgs e)
		{
			HandleTabChanged();
			/*
			   Thread t = new Thread(delegate() {
			   HandleTabChanged();
			   });
			   t.IsBackground = true;
			   t.Start();
			   */
		}

		public void HandleTabChanged()
		{
			Logger.Info("HandleTabChanged. SelectedIndex: " + this.SelectedIndex);

			if (this.SelectedIndex < 0)
			{
				return;
			}

			mGameManager.mLastShownAppInfo = "";

			Tab selectedTab = (Tab)this.SelectedTab;

			if (mCurrentTab != null)
			{
				if (mCurrentTab.Controls.Contains(Tab.sProgressBarControl))
				{
					Logger.Info("Removing wait browser from " + mCurrentTab.Name);
					Tab.ShowWaitControl(false);
				}
			}

			mCurrentTab = (Tab)this.SelectedTab;
			Logger.Info("mCurrentTab: " + mCurrentTab.Name);
			mTabHistory.Push(mCurrentTab);

			selectedTab = mCurrentTab;
			selectedTab.mFrontendHandle = IntPtr.Zero;

			string[] tabChangedData = new string[3];
			tabChangedData[0] = selectedTab.mTabType;
			tabChangedData[1] = selectedTab.mLabel;

			//Handling Tab Change functionality for CLR Browser
			if (StreamManager.sStreamManager != null 
					&& StreamManager.sStreamManager.mIsObsRunning)
			{

				string appPkg = selectedTab.mPackage;
				if (appPkg != null && FilterUtility.IsFilterApplicableApp(appPkg))
				{
					if (!StreamManager.sStreamManager.mCLRBrowserRunning)
						StreamManager.sStreamManager.InitCLRBrowser(appPkg,
							FilterUtility.GetCurrentTheme(appPkg));
					else if (StreamManager.sStreamManager.mCurrentFilterAppPkg
								!= appPkg)
					{
						string theme = FilterUtility.GetCurrentTheme(appPkg);
						if (theme != null)
							StreamManager.sStreamManager.ChangeTheme(appPkg, theme);
						else
							StreamManager.sStreamManager.ResetCLRBrowser();
					}
				}
				else if (StreamManager.sStreamManager.mCLRBrowserRunning)
					StreamManager.sStreamManager.ResetCLRBrowser();
			}

			if (selectedTab.mTabType == "web")
			{
				StreamViewTimeStats.HandleStreamViewStatsEvent(selectedTab.mLabel,
						StreamViewStatsEventName.TabVisibleStart);

				tabChangedData[2] = selectedTab.mWebUrl;

				if (!Utils.IsOSWinXP())
				{
					waveOutSetVolume(IntPtr.Zero, mSavedVolumeLevel);
				}

				mGameManager.ToggleControlBarButtons();
				if (mGameManager.mToolBarForm != null) {
					mGameManager.mToolBarForm.DisableAppTabButtons();
				}

				mAppDisplayed = false;
				Logger.Info("mAppDisplayed: " + mAppDisplayed);
				UnsetParentFrontend();

				if (selectedTab.mBrowser != null)
					selectedTab.mBrowser.Focus();
				else
					Logger.Error("TabBar: selectedTab browser is null. No window to show");

				//resize window
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
				{
					mGameManager.WebResizeWindow();
				}
			}
			else if (selectedTab.mTabType == "app")
			{
				tabChangedData[2] = selectedTab.mPackage;

				if (!Utils.IsOSWinXP())
				{
					waveOutGetVolume(IntPtr.Zero, out mSavedVolumeLevel);
					waveOutSetVolume(IntPtr.Zero, 0);
				}

				string package = selectedTab.mPackage;
				string activity = selectedTab.mActivity;

				mGameManager.ToggleControlBarButtons();
				if (mGameManager.mToolBarForm != null) {
					mGameManager.mToolBarForm.EnableGenericAppTabButtons();
					if (Array.IndexOf(GameManager.ToggleDisableAppList, selectedTab.mPackage) == -1) {
						mGameManager.mToolBarForm.EnableToggleAppTabButton();
					} else {
						mGameManager.mToolBarForm.DisableToggleAppTabButton();
					}
				}

				bool relaunch, show;
				Logger.Info("mLastAppTabName: {0}, selectedTab.Name: {1}, selectedTab.mLaunchApp: {2}",
						mLastAppTabName, selectedTab.Name, selectedTab.mLaunchApp);

				if (mLastAppTabName == selectedTab.Name)
				{
					relaunch = false;
					show = true;
				}
				else if (selectedTab.mLaunchApp == false)
				{
					relaunch = false;
					show = false;
				}
				else
				{
					relaunch = true;
					show = false;
				}

				if (selectedTab.mIsHome)
				{
					Thread thread = new Thread(delegate()
							{
							Common.VmCmdHandler.RunCommand("home");
							});

					thread.IsBackground = true;
					thread.Start();
				}

				if (mHomeUnresolved || selectedTab.mPackage == "none")
				{
					if (GetCurrentTab().mIsHome)
					{
						relaunch = false;
						show = true;
					}
				}

				/*
				 * It is possible that we get AppDisplayed for the launched app even before
				 * its tab is created and the tab action performed.
				 * mLastAppDisplayed will save the AppDisplayed data, if any, for the last
				 * launched app.
				 * We will check this var while creating a tab and show frontend
				 * without waiting for AppDisplayed in case it matches the tab package.
				 */
				if (mGameManager.mLastAppDisplayed.Contains(package))
					show = true;

				/*
				 * We want to switch to android when the location popup is visible,
				 * irrespective of what tab the user switches to
				 */
				if (mGameManager.mLastAppDisplayed.Contains(GameManager.PLAYLOCATIONINFO))
					show = true;

				selectedTab.mLaunchApp = true;

				selectedTab.PerformTabAction(relaunch, show);

				mLastAppTabName = selectedTab.Name;

				RegistryKey regKey = Registry.LocalMachine.OpenSubKey(@"Software\BlueStacks");
				string programDataDir = (string)regKey.GetValue("DataDir");
				string configDir = Path.Combine(programDataDir, @"UserData\InputMapper");
				string configFile = Path.Combine(configDir, selectedTab.mPackage + ".cfg");
				string userConfigFile = Path.Combine(configDir, "UserFiles\\" + selectedTab.mPackage + ".cfg");

				if (File.Exists(configFile) || File.Exists(userConfigFile))
				{
					mGameManager.mControlBarRight.GlowKeyMappingButton();
				}
				else
				{
					mGameManager.mControlBarRight.ShowDefaultKeyMappingButton();
				}

				//
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
				{
					Tab tab = selectedTab;
					if (tab != null && !this.FullScreen)
					{
						if (tab.DeselectedGMClientSize != null)
						{
							mGameManager.ClientSize = tab.DeselectedGMClientSize.Value;
                            mGameManager.Refresh();
						}
						else
						{
							//if (relaunch)
							//{
							Size appSize = GameManager.GetFEAppWindowSize(GameManager.sGameManager.mFrontendHandle);
							if (appSize.Width > 0 && appSize.Height > 0)
							{
								Size tempSize = mGameManager.GetFESizeVerticalBySreen();
								if (appSize.Width < appSize.Height)
								{
									if (GameManager.sFrontendWidth < GameManager.sFrontendHeight)
									{
										tempSize = new Size(GameManager.sFrontendWidth, GameManager.sFrontendHeight);
									}
								}
								else
								{
									tempSize = mGameManager.GetFESizeHorizontalBySreen();
									if (GameManager.sFrontendWidth > GameManager.sFrontendHeight)
									{
										tempSize = new Size(GameManager.sFrontendWidth, GameManager.sFrontendHeight);
									}
								}
								tempSize = mGameManager.GetGMSizeGivenFESize(tempSize.Width, tempSize.Height);
								mGameManager.ClientSize = tempSize;
                                mGameManager.Refresh();
							}
							else
							{
								mGameManager.AppResizeWindowIfNeed();
							}
							//}
							//else
							//{
							//    mGameManager.AppResizeWindowIfNeed();
							//}
						}
					}
				}
			}
			if (Oem.Instance.IsStreamWindowEnabled)
			{
				if (GameManager.sGameManager.mStreamManager != null)
				{
					GameManager.sGameManager.mStreamManager.TabChanged(tabChangedData);
				}
			}

			Logger.Info("HandleTabChanged. done SelectedIndex: " + this.SelectedIndex);
		}

		public void ResizeFrontend()
		{
			Tab selectedTab = (Tab)this.SelectedTab;

			if (FrontendVisible())
			{
				Logger.Info("Resizing to ({0}, {1})", selectedTab.Width, selectedTab.Height);
				int r = Window.SendMessage(mFrontendHandle,
						Window.WM_USER_RESIZE_WINDOW,
						(IntPtr)selectedTab.Width, (IntPtr)selectedTab.Height);
				Logger.Info("WM_USER_RESIZE_WINDOW: " + r);
			}
		}

		public IntPtr SetParentFrontend(IntPtr parentHandle, bool showFrontend,string procName,string title)
		{
			Logger.Info("SetParentFrontend called");

			if (mFrontendHandle == IntPtr.Zero || GetWindowText(mFrontendHandle) == "")
			{
				mFrontendHandle = Window.FindWindow(null, title);
				if (mFrontendHandle == IntPtr.Zero)
				{
					Logger.Info("Could not find window");
					string frontendName = procName;
					if (!Utils.FindProcessByName(frontendName))
						LaunchFrontend();
					while (mFrontendHandle == IntPtr.Zero)
					{
						mFrontendHandle = Window.FindWindow(null, title);
						Thread.Sleep(100);
					}
				}
			}

			Logger.Info("parentHandle: " + parentHandle);
			Logger.Info("mFrontendHandle: " + mFrontendHandle);

			//Remove WS_OVERLAPPED style and add WS_CHILD style
			int style = Window.GetWindowLong(mFrontendHandle, Window.GWL_STYLE);
			style = (int)(style & ~Window.WS_OVERLAPPED);
			style = (int)(style | Window.WS_CHILD);
			Window.SetWindowLong(mFrontendHandle, Window.GWL_STYLE, style);

			UIHelper.RunOnUIThread(this, delegate() {
					IntPtr h = Window.SetParent(mFrontendHandle, parentHandle);
					mCurrentParentHandle = parentHandle;
					SetFocus(mFrontendHandle);
					Logger.Info("SetParent(): " + h);
					Logger.Info("LastError: " + Marshal.GetLastWin32Error());
					});

			Window.SetWindowPos(
					mFrontendHandle,
					(IntPtr)0,
					GameManager.mContentBorderWidth + mTabBorderWidth,
					GameManager.mContentBorderWidth + mTabBorderWidth,
					this.SelectedTab.Width - 2 * GameManager.mContentBorderWidth - mTabBorderWidth,
					this.SelectedTab.Height - 2 * GameManager.mContentBorderWidth - mTabBorderWidth,
					Window.SWP_NOACTIVATE | Window.SWP_SHOWWINDOW
					);

			if (showFrontend)
			{
				bool ret = Window.ShowWindow(mFrontendHandle, Window.SW_SHOW);
				Logger.Info("ShowWindow(SW_SHOW): " + ret);

				int res = Window.SendMessage(mFrontendHandle,
						Window.WM_USER_SHOW_WINDOW,
						IntPtr.Zero, IntPtr.Zero);
				Logger.Info("WM_USER_SHOW_WINDOW: " + res);

				if (mFrontendMuted)
				{
					int r = Window.SendMessage(mFrontendHandle, Window.WM_USER_AUDIO_MUTE, IntPtr.Zero, IntPtr.Zero);
					Logger.Info("WM_USER_AUDIO_MUTE: " + r);
				}
				else
				{
					res = Window.SendMessage(mFrontendHandle, Window.WM_USER_AUDIO_UNMUTE, IntPtr.Zero, IntPtr.Zero);
					Logger.Info("WM_USER_AUDIO_UNMUTE: " + res);
				}

				res = Window.SendMessage(mFrontendHandle, Window.WM_USER_ACTIVATE, IntPtr.Zero, IntPtr.Zero);
				Logger.Info("WM_USER_ACTIVATE: " + res);
			}
			else
			{
				bool ret = Window.ShowWindow(mFrontendHandle, Window.SW_HIDE);
				Logger.Info("ShowWindow(SW_HIDE): " + ret);

				int r = Window.SendMessage(mFrontendHandle, Window.WM_USER_AUDIO_MUTE, IntPtr.Zero, IntPtr.Zero);
				Logger.Info("WM_USER_AUDIO_MUTE: " + r);
			}

			mGameManager.SetupGamePad();

			return mFrontendHandle;
		}

		public void UnsetParentFrontend()
		{
			Logger.Info("UnsetParentFrontend called");
			if (mFrontendHandle == IntPtr.Zero || GetWindowText(mFrontendHandle) == "")
			{
				return;
			}

			Logger.Info("mFrontendHandle: " + mFrontendHandle);

			int r = Window.SendMessage(mFrontendHandle, Window.WM_USER_HIDE_WINDOW, IntPtr.Zero, IntPtr.Zero);

			bool ret = Window.ShowWindow(mFrontendHandle, Window.SW_HIDE);
			Logger.Info("ShowWindow(SW_HIDE): " + ret);
			IntPtr h = Window.SetParent(mFrontendHandle, IntPtr.Zero);
			Logger.Info("SetParent(): " + h);
			mCurrentParentHandle = IntPtr.Zero;

			//Remove WS_CHILD style and add WS_OVERLAPPED style
			int style = Window.GetWindowLong(mFrontendHandle, Window.GWL_STYLE);
			style = (int)(style & ~Window.WS_CHILD);
			style = (int)(style | Window.WS_OVERLAPPED);
			Window.SetWindowLong(mFrontendHandle, Window.GWL_STYLE, style);

			r = Window.SendMessage(mFrontendHandle, Window.WM_USER_DEACTIVATE, IntPtr.Zero, IntPtr.Zero);
			Logger.Info("WM_USER_DEACTIVATE: " + r);
			r = Window.SendMessage(mFrontendHandle, Window.WM_USER_HIDE_WINDOW, IntPtr.Zero, IntPtr.Zero);
			Logger.Info("WM_USER_HIDE_WINDOW: " + r);

			r = Window.SendMessage(mFrontendHandle, Window.WM_USER_AUDIO_MUTE, IntPtr.Zero, IntPtr.Zero);
			Logger.Info("WM_USER_AUDIO_MUTE: " + r);
		}

		public void MuteFrontend()
		{
			if (GetCurrentTabType() == "app")
				Window.SendMessage(mFrontendHandle, Window.WM_USER_AUDIO_MUTE, IntPtr.Zero, IntPtr.Zero);
			mFrontendMuted = true;
		}

		public void UnmuteFrontend()
		{
			if (GetCurrentTabType() == "app")
				Window.SendMessage(mFrontendHandle, Window.WM_USER_AUDIO_UNMUTE, IntPtr.Zero, IntPtr.Zero);
			mFrontendMuted = false;
		}

		public bool FrontendVisible()
		{
			if (mCurrentTab != null && mFrontendHandle != IntPtr.Zero && mAppDisplayed)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private String GetWindowText(IntPtr handle)
		{
			int length = Window.GetWindowTextLength(mFrontendHandle);
			StringBuilder sb = new StringBuilder(length + 1);
			Window.GetWindowText(mFrontendHandle, sb, sb.Capacity);
			String text = sb.ToString();
			Logger.Info("Window Text: " + text);
			return text;
		}

		private void LaunchFrontend()
		{
			String path = String.Format(@"Software\BlueStacks\Guests\{0}\FrameBuffer\0", Common.Strings.VMName);
			RegistryKey key = Registry.LocalMachine.CreateSubKey(path);
			key.SetValue("FullScreen", 0);
			key.Close();

			Logger.Info("Launching frontend");
			key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)key.GetValue("InstallDir");
			string runApp = Path.Combine(installDir, @"HD-RunApp.exe");
			string args = "-v " + Common.Strings.VMName + " -h";
			Process.Start(runApp, args);
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case Window.WM_MOUSEWHEEL:
					if (FrontendVisible())
					{
						int LOWORD = (int)m.LParam & 0xFFFF;    // X
						// int HIWORD = (int)m.LParam >> 16;       // Y
						int pos = LOWORD - mGameManager.Left - GameManager.sFrontendWidth;
						if (pos <= 0)
							Window.SendMessage(mFrontendHandle, (uint)m.Msg, m.WParam, m.LParam);
					}
					break;
			}

			base.WndProc(ref m);
		}


		public string[] GetCurrentTabData()
		{
			Tab selectedTab = (Tab)this.SelectedTab;

			string[] currentTabData = new string[3];
			currentTabData[0] = selectedTab.mTabType;
			currentTabData[1] = selectedTab.mLabel;
			if (selectedTab.mTabType == "web")
			{
				currentTabData[2] = selectedTab.mWebUrl;
			}
			else if (selectedTab.mTabType == "app")
			{
				currentTabData[2] = selectedTab.mPackage;
			}

			return currentTabData;
		}

		public void HandleRotate()
		{
			mRotateInProgress = true;
		}

	}
}

