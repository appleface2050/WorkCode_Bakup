using System;
using System.Drawing;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.IO;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using System.Linq;

namespace BlueStacks.hyperDroid.GameManager
{
	public class ControlBar : UserControl
	{
		private static int sButtonWidth;
		private static int sButtonHeight;
		private static int sDividerWidth = 1;
		private static int sBorderWidth = GameManager.mBorderWidth;

		private const int BUTTON_WIDTH = 67;
		private const int BUTTON_HEIGHT = 46;
		private const int HOME_BUTTON_WIDTH = 141;

		private static int sButtonSpacing;
		private static int sTopPadding = 0;

		public TabBar mTabBar;
		public GameManager mGameManager;

		public IntPtr mFrontendHandle
		{
			get { return mGameManager.mFrontendHandle; }
			set { mGameManager.mFrontendHandle = value; }
		}

		private const int TOTAL_BUTTONS_RIGHT = 6;
		private const int TOTAL_BUTTONS_LEFT = 1;
		private int mTotalButtons;

		// Left control bar buttons
		private PictureBox mBackButton;

		// Right control bar buttons
		private PictureBox mMinimizeButton;
		private PictureBox mCloseButton;
		private PictureBox mToggleScreenButton;
		private PictureBox mShowTabPages;
		//		private PictureBox	mNotificationButton;
		//		private PictureBox	mHelpButton;
		private PictureBox mSettingsButton;
		private PictureBox mGuidanceButton;

		private PictureBox mPBLoading;

		public static int CONTROL_BAR_POSITION_LEFT = 0;
		public static int CONTROL_BAR_POSITION_RIGHT = 1;
		private int mControlBarPosition;

		private ContextMenuStrip mSettingsMenu;

		private string SETTINGS_MENU_REPORT_PROBLEM = GameManager.sLocalizedString["ReportProblemText"];
		//		private string	SETTINGS_MENU_THEMES		= GameManager.sLocalizedString["ThemesText"];
		private string SETTINGS_MENU_RESTART_ANDROID = GameManager.sLocalizedString["RestartAndroidText"];
		private string SETTINGS_MENU_UPDATES = GameManager.sLocalizedString["CheckForUpdatesText"];
		private string SETTINGS_MENU_LANG_AND_INPUT = GameManager.sLocalizedString["LanguageAndInputSettingsText"];
		private string SETTINGS_MENU_ALL_SETTINGS = GameManager.sLocalizedString["SettingsText"];
		private string SETTINGS_MENU_ANDROID_SETTINGS = GameManager.sLocalizedString["AndroidSetingsText"];
		private string SETTINGS_MENU_MY_ACCOUNT = GameManager.sLocalizedString["MyAccountText"];
		//		private string	SETTINGS_MENU_HELP		= GameManager.sLocalizedString["HelpText"];
		private string SETTINGS_MENU_LOCATION = GameManager.sLocalizedString["LocationText"];

		public static int GetButtonWidth(int height)
		{
			return (int)(height * ((float)BUTTON_WIDTH) / BUTTON_HEIGHT);
		}

		public ControlBar(TabBar tabBar)
		{
			try
			{
				mTabBar = tabBar;
				mGameManager = tabBar.mGameManager;
				this.BackColor = Color.Transparent;
				this.MouseDown += new MouseEventHandler(FormMouseDown);
				this.GotFocus += new System.EventHandler(this.FocusHandler);
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		public void IntializeLayout(int parentWidth, int height, int pos)
		{
			mControlBarPosition = pos;

			sButtonHeight = height;
			sButtonWidth = GetButtonWidth(height);
			sButtonSpacing = sButtonWidth;
			int xPos = 0;
			int mControlBarWidth;
			if (mControlBarPosition == CONTROL_BAR_POSITION_RIGHT)
			{
				mTotalButtons = TOTAL_BUTTONS_RIGHT;
				mControlBarWidth = mTotalButtons * sButtonSpacing;
				xPos = parentWidth - mControlBarWidth - sBorderWidth - GameManager.TransparentBox.Width;
			}
			else
			{
				mTotalButtons = TOTAL_BUTTONS_LEFT;
				mControlBarWidth = sButtonSpacing;
				xPos = sBorderWidth;
			}

			string parentStyleTheme = "Em";
			using (RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
			{
				parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", parentStyleTheme);
			}

			if (parentStyleTheme != "Em")
			{
				this.Size = new Size(mControlBarWidth, height + GameManager.mTabBarExtraHeight - 1);
				this.MaximumSize = this.Size;
				this.MinimumSize = this.Size;
			}
			else
			{
				this.Size = new Size(mControlBarWidth, height + GameManager.mTabBarExtraHeight);
				this.MaximumSize = this.Size;
				this.MinimumSize = this.Size;
			}

			this.Location = new Point(xPos, GameManager.mBorderWidth);
		}

		private void FocusHandler(object sender, EventArgs e)
		{
			if (mTabBar != null)
				mTabBar.Focus();
		}

		private void FormMouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				GameManager.ReleaseCapture();
				GameManager.SendMessage(mGameManager.Handle, GameManager.WM_NCLBUTTONDOWN, GameManager.HT_CAPTION, 0);
			}
		}

		/*
		   public PictureBox getNewDivider()
		   {
		   PictureBox divider = new PictureBox();
		   divider.Image = mAllImages["tool_divider"];
		   divider.SizeMode = PictureBoxSizeMode.StretchImage;
		   divider.Size = new Size(sDividerWidth, sDividerHeight);
		   divider.BackColor = GMColors.TransparentColor;
		   return divider;
		   }
		   */

		public void Init()
		{
			if (mControlBarPosition == CONTROL_BAR_POSITION_RIGHT)
			{
				InitRight();
			}
			else if (mControlBarPosition == CONTROL_BAR_POSITION_LEFT)
			{
				InitLeft();
			}
		}

		public void InitLeft()
		{
			this.SuspendLayout();
			if (mBackButton == null)
			{
				mBackButton = new PictureBox();
				mBackButton.Tag = "tool_leftarrow";
				mBackButton.MouseClick += HandleBack;
				SetButtonProperties(mBackButton);

				ToolTip backToolTip = new ToolTip();
				backToolTip.ShowAlways = true;
				backToolTip.SetToolTip(mBackButton, GameManager.sLocalizedString["BackTooltip"]);
			}
			DisableButton(mBackButton);
			SetButtonSize(mBackButton);
			mBackButton.Location = new Point(0, sTopPadding + GameManager.mTabBarExtraHeight);

			AddControls(mBackButton);

			this.ResumeLayout(false);
		}

		public void InitRight()
		{
			this.SuspendLayout();

			sButtonSpacing = sButtonWidth + 2 * sDividerWidth;
			int offsetTabBar = GameManager.mTabBarExtraHeight;

			if (mCloseButton == null)
			{
				mCloseButton = new PictureBox();
				mCloseButton.Tag = "tool_close";
				mCloseButton.MouseClick += HandleClose;
				SetButtonProperties(mCloseButton);

				ToolTip closeToolTip = new ToolTip();
				closeToolTip.ShowAlways = true;
				closeToolTip.SetToolTip(mCloseButton, GameManager.sLocalizedString["CloseTooltip"]);
			}
			mCloseButton.Location = new Point(this.Width - sButtonWidth, sTopPadding + offsetTabBar);
			SetButtonSize(mCloseButton);

			if (mToggleScreenButton == null)
			{
				mToggleScreenButton = new PictureBox();
				mToggleScreenButton.MouseClick += HandleFullScreen;
				SetButtonProperties(mToggleScreenButton);

				ToolTip toggleScreenToolTip = new ToolTip();
				toggleScreenToolTip.ShowAlways = true;
				toggleScreenToolTip.SetToolTip(mToggleScreenButton, GameManager.sLocalizedString["ToggleScreenTooltip"]);
			}
			mToggleScreenButton.Location = new Point(mCloseButton.Left - sButtonWidth, sTopPadding + offsetTabBar);
			if (mGameManager.FullScreen)
				mToggleScreenButton.Tag = "tool_shrink";
			else
				mToggleScreenButton.Tag = "tool_fullscreen";
			mToggleScreenButton.Image = Assets.mAllImagesDict[(String)mToggleScreenButton.Tag];
			SetButtonSize(mToggleScreenButton);
			DisableButton(mToggleScreenButton);

			if (mMinimizeButton == null)
			{
				mMinimizeButton = new PictureBox();
				mMinimizeButton.Tag = "tool_minimize";
				mMinimizeButton.MouseClick += HandleMinimize;
				SetButtonProperties(mMinimizeButton);

				ToolTip minimizeToolTip = new ToolTip();
				minimizeToolTip.ShowAlways = true;
				minimizeToolTip.SetToolTip(mMinimizeButton, GameManager.sLocalizedString["MinimizeTooltip"]);
			}
			mMinimizeButton.Location = new Point(mToggleScreenButton.Left - sButtonWidth, sTopPadding + offsetTabBar);
			SetButtonSize(mMinimizeButton);

			if (mSettingsButton == null)
			{
				mSettingsButton = new PictureBox();
				mSettingsButton.Tag = "tool_settings";
				mSettingsButton.Name = "tool_settings";
				mSettingsButton.MouseClick += HandleSettings;
				SetButtonProperties(mSettingsButton);

				ToolTip settingsToolTip = new ToolTip();
				settingsToolTip.ShowAlways = true;
				settingsToolTip.SetToolTip(mSettingsButton, GameManager.sLocalizedString["SettingsTooltip"]);
			}
			mSettingsButton.Location = new Point(mMinimizeButton.Left - sButtonSpacing, sTopPadding + offsetTabBar);
			SetButtonSize(mSettingsButton);

			if (mGuidanceButton == null)
			{
				mGuidanceButton = new PictureBox();
				mGuidanceButton.Tag = "tool_key";
				mGuidanceButton.MouseClick += HandleGuidance;
				SetButtonProperties(mGuidanceButton);

				ToolTip guidanceToolTip = new ToolTip();
				guidanceToolTip.ShowAlways = true;
				guidanceToolTip.SetToolTip(mGuidanceButton, GameManager.sLocalizedString["GuidanceTooltip"]);
			}
			mGuidanceButton.Location = new Point(mSettingsButton.Left - sButtonSpacing, sTopPadding + offsetTabBar);
			SetButtonSize(mGuidanceButton);

			//////////////////
			if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
			{
				if (mShowTabPages == null)
				{
					mShowTabPages = new PictureBox();
					mShowTabPages.Tag = "tool_showtabpages";
					mShowTabPages.MouseClick += HandleShowTabPages;
					SetButtonProperties(mShowTabPages);

					ToolTip closeToolTip = new ToolTip();
					closeToolTip.ShowAlways = true;
					//closeToolTip.SetToolTip(mShowTabPages, GameManager.sLocalizedString["ShowtabpagesTooltip"]);//todo
				}
				mShowTabPages.Location = new Point(mGuidanceButton.Left - sButtonSpacing, sTopPadding + offsetTabBar);
				SetButtonSize(mShowTabPages);

				AddControls(mShowTabPages);
			}
			////////////////////

			if (mPBLoading == null)
			{
				mPBLoading = new PictureBox();
				mPBLoading.BackColor = GMColors.TransparentColor;
				mPBLoading.Parent = this;
			}
			mPBLoading.SizeMode = PictureBoxSizeMode.Zoom;
			mPBLoading.Image = Assets.mAllImagesDict["loading"];
			mPBLoading.Size = new Size(sButtonWidth, sButtonHeight);
			mPBLoading.Location = new Point(mGuidanceButton.Left - sButtonSpacing, sTopPadding + offsetTabBar);
			mPBLoading.Visible = false;


			AddControls(mCloseButton);
			AddControls(mMinimizeButton);
			AddControls(mToggleScreenButton);
			AddControls(mSettingsButton);
			AddControls(mGuidanceButton);
			//			AddControls(mHelpButton);
			AddControls(mPBLoading);

			this.ResumeLayout(false);
		}
		private void AddControls(PictureBox pb)
		{
			if (!this.Controls.Contains(pb))
			{
				this.Controls.Add(pb);
			}
		}

		public void GlowKeyMappingButton()
		{
			mGuidanceButton.Tag = "tool_key_glow";
			EnableButton(mGuidanceButton);
		}

		public void ShowDefaultKeyMappingButton()
		{
			mGuidanceButton.Tag = "tool_key";
			EnableButton(mGuidanceButton);
		}

		public void EnableAllButtons()
		{
			if (mControlBarPosition == CONTROL_BAR_POSITION_LEFT)
			{
				EnableButton(mBackButton);
			}
			else if (mControlBarPosition == CONTROL_BAR_POSITION_RIGHT)
			{
				EnableButton(mToggleScreenButton);
				EnableButton(mSettingsButton);
				EnableButton(mGuidanceButton);
			}
		}

		public void DisableBackButton()
		{
			if (mControlBarPosition == CONTROL_BAR_POSITION_LEFT)
			{
				DisableButton(mBackButton);
			}
		}

		public void EnableBackButton()
		{
			if (mControlBarPosition == CONTROL_BAR_POSITION_LEFT)
			{
				EnableButton(mBackButton);
			}
		}

		public void SetFullScreenButton()
		{
			mToggleScreenButton.Tag = "tool_fullscreen";
			UpdateButton(mToggleScreenButton);
		}

		public void SetShrinkScreenButton()
		{
			mToggleScreenButton.Tag = "tool_shrink";
			UpdateButton(mToggleScreenButton);
		}

		private void SetButtonProperties(PictureBox button)
		{
			button.MouseEnter += new EventHandler(this.ControlBarButtonMouseEnter);
			button.MouseDown += new MouseEventHandler(this.ControlBarButtonMouseDown);
			button.MouseUp += new MouseEventHandler(this.ControlBarButtonMouseUp);
			button.MouseLeave += new EventHandler(this.ControlBarButtonMouseLeave);
			button.BackColor = GMColors.TransparentColor;
		}

		private static void SetButtonSize(PictureBox button)
		{
			button.Image = Assets.mAllImagesDict[(String)button.Tag];
			button.SizeMode = PictureBoxSizeMode.StretchImage;
			button.Size = new Size(sButtonWidth, sButtonHeight);

		}

		private void ControlBarButtonMouseEnter(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Cursor = Cursors.Hand;
				button.Image = Assets.mAllImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ControlBarButtonMouseDown(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Image = Assets.mAllImagesDict[(String)button.Tag + "_click"];
			}
		}

		private void ControlBarButtonMouseUp(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Image = Assets.mAllImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ControlBarButtonMouseLeave(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Cursor = Cursors.Default;
				button.Image = Assets.mAllImagesDict[(String)button.Tag];
			}
		}

		private void HandleBack(Object sender, MouseEventArgs e)
		{
			if (!mTabBar.Visible)
			{
				if (GameManager.sGameManager.mSetupScreenBrowser != null)
					GameManager.sGameManager.mSetupScreenBrowser.GoBack();
			}
			else
			{
				if (mTabBar.SelectedTab != null)
				{
					Tab selectedTab = (Tab)mTabBar.SelectedTab;
					if (selectedTab.mTabType == "app")
					{
						Logger.Info("Back Button Clicked");
						Thread thread = new Thread(delegate ()
								{
									if (Utils.IsGuestBooted())
										VmCmdHandler.RunCommand("back");
								});
						thread.IsBackground = true;
						thread.Start();
					}
					else if (selectedTab.mTabType == "web")
					{
						if (selectedTab.mBrowser != null)
							selectedTab.mBrowser.GoBack();
					}
				}
			}
		}

		private void HandleShowTabPages(Object sender, MouseEventArgs e)
		{
			if (mGameManager.mTabBar.TabPages.Count > 0)
			{
				ContextMenuStrip mShowTabPagesMenu;
				mShowTabPagesMenu = new ContextMenuStrip();
				mShowTabPagesMenu.ForeColor = GMColors.ContextMenuForeColor;
				mShowTabPagesMenu.BackColor = GMColors.ContextMenuBackColor;
				mShowTabPagesMenu.ShowImageMargin = false;
				mShowTabPagesMenu.ShowCheckMargin = true;
				mShowTabPagesMenu.Font = new Font(mShowTabPagesMenu.Font.Name, 15F);
				mShowTabPagesMenu.Cursor = Cursors.Hand;
				mShowTabPagesMenu.Renderer = new CustomRenderer();

				Tab cutab = mGameManager.mTabBar.GetCurrentTab();
				Image itemImage = Image.FromFile(Path.Combine(GameManager.sAssetsDir, "tab_close.png"));
				for (int i = 0; i < mGameManager.mTabBar.TabPages.Count; ++i)
				{
					Tab tab = mGameManager.mTabBar.TabPages[i] as Tab;
					if (tab != null)
					{
						ToolStripMenuItem item = mShowTabPagesMenu.Items.Add(tab.Text) as ToolStripMenuItem;
						item.BackColor = GMColors.ContextMenuBackColor;
						item.ForeColor = GMColors.ContextMenuForeColor;
						if (tab.mTabType != "web" && (!tab.mIsHome))
						{
							item.Image = itemImage;
						}

						if (tab == cutab)
						{
							item.Checked = true;
							item.CheckState = CheckState.Checked;
						}
						item.MouseUp += new MouseEventHandler(delegate (object senderitem, MouseEventArgs eitem)
						{
							ToolStripMenuItem toolItem = senderitem as ToolStripMenuItem;
							Rectangle iconRect = CustomRenderer.CloseRectangle(toolItem);

							if (iconRect.Contains(eitem.Location))
							{
								mGameManager.mTabBar.CloseTab(tab);
							}
						});

					}
				}

				mShowTabPagesMenu.ItemClicked += new ToolStripItemClickedEventHandler((object tsender, ToolStripItemClickedEventArgs te) =>
				{

					ToolStripItem item = te.ClickedItem;
					String itemName = item.ToString();
					for (int i = 0; i < mGameManager.mTabBar.TabPages.Count; ++i)
					{
						Tab tab = mGameManager.mTabBar.TabPages[i] as Tab;
						if (tab != null && tab.Text == itemName)
						{
							mGameManager.mTabBar.GoToTab(tab);
							break;
						}
					}

					mShowTabPagesMenu.Dispose();

				});
				mShowTabPagesMenu.Show(mShowTabPages,
						new Point(-1 * mShowTabPages.Width + mShowTabPages.Width,
							GameManager.sControlBarHeight));
				mShowTabPagesMenu.LostFocus += new EventHandler((object tsender, EventArgs te) =>
				{
					mShowTabPagesMenu.Dispose();
				});

			}
		}
		private void HandleClose(Object sender, MouseEventArgs e)
		{
			mGameManager.HandleClose();
		}

		private void HandleMinimize(Object sender, MouseEventArgs e)
		{
			mGameManager.HandleMinimize();
		}

		private void HandleSettings(Object sender, MouseEventArgs e)
		{
			if (mSettingsMenu == null)
			{
				mSettingsMenu = new ContextMenuStrip();
				mSettingsMenu.ForeColor = GMColors.ContextMenuForeColor;
				mSettingsMenu.BackColor = GMColors.ContextMenuBackColor;
				mSettingsMenu.ShowImageMargin = false;
				mSettingsMenu.ShowCheckMargin = false;
				mSettingsMenu.Font = new Font(mSettingsMenu.Font.Name, 15F);
				mSettingsMenu.Cursor = Cursors.Hand;
				mSettingsMenu.Renderer = new CustomRenderer();

				mSettingsMenu.Items.Add(SETTINGS_MENU_REPORT_PROBLEM);
				mSettingsMenu.Items.Add(SETTINGS_MENU_RESTART_ANDROID);
				mSettingsMenu.Items.Add(SETTINGS_MENU_UPDATES);
				mSettingsMenu.Items.Add(SETTINGS_MENU_ALL_SETTINGS);
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
				{
					mSettingsMenu.Items.Add(SETTINGS_MENU_ANDROID_SETTINGS);
				}
				//				mSettingsMenu.Items.Add(SETTINGS_MENU_LOCATION);
				//				mSettingsMenu.Items.Add(SETTINGS_MENU_LANG_AND_INPUT);
				//				mSettingsMenu.Items.Add(SETTINGS_MENU_MY_ACCOUNT);
				//				mSettingsMenu.Items.Add(SETTINGS_MENU_HELP);
				// mSettingsMenu.Items.Add(SETTINGS_MENU_THEMES);

				mSettingsMenu.ItemClicked += new ToolStripItemClickedEventHandler(SettingsSubMenuClicked);
				mSettingsMenu.Show(mSettingsButton,
						new Point(-1 * mSettingsMenu.Width + mSettingsButton.Width,
							GameManager.sControlBarHeight));
			}
			else
			{
				mSettingsMenu.Show(mSettingsButton,
						new Point(-1 * mSettingsMenu.Width + mSettingsButton.Width,
							GameManager.sControlBarHeight));
			}
		}

		void SettingsSubMenuClicked(object sender, ToolStripItemClickedEventArgs e)
		{
			ToolStripItem item = e.ClickedItem;
			String itemName = item.ToString();

			if (itemName == SETTINGS_MENU_REPORT_PROBLEM)
			{
				GMApi.ReportProblem();
			}
			/*
			   else if (itemName == SETTINGS_MENU_THEMES)
			   {
			// This menu item is not shown right now
			}
			*/
			else if (itemName == SETTINGS_MENU_RESTART_ANDROID)
			{
				GMApi.RestartAndroidPlugin();
			}
			else if (itemName == SETTINGS_MENU_UPDATES)
			{
				Thread t = new Thread(delegate ()
						{
							GMApi.CheckForUpdates();
						});
				t.IsBackground = true;
				t.Start();
			}
			else if (itemName == SETTINGS_MENU_ALL_SETTINGS)
			{
				RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
				string enableStyleTheme = (string)configKey.GetValue("EnableStyleTheme", null);
				if (string.IsNullOrEmpty(enableStyleTheme))
				{
					{
						//setting show language
						GameManager.sGameManager.ShowApp(Locale.Strings.Settings,
								"com.bluestacks.settings",
								"com.bluestacks.settings.SettingsActivity",
								"",
								true);
					}
				}
				else
				{
					SettingsForm settingsForm = new SettingsForm();
					settingsForm.SetUp(mTabBar, mSettingsMenu);
					settingsForm.ShowDialog(this);
				}

			}
			else if (itemName == SETTINGS_MENU_ANDROID_SETTINGS)
			{
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
				{
					//UserSettingsForm
					UserSettingsForm userSettingsForm = new UserSettingsForm(mGameManager);
					userSettingsForm.Show(this);
				}
			}
			else if (itemName == SETTINGS_MENU_LANG_AND_INPUT)
			{
				GameManager.sGameManager.ShowApp("Input Settings",
						"com.android.settings",
						"com.android.settings.LanguageSettings",
						"",
						true);
			}
			else if (itemName == SETTINGS_MENU_LOCATION)
			{
				GameManager.sGameManager.ShowApp(Locale.Strings.Location,
						"com.uncube.locationprovider",
						"com.uncube.locationprovider.MapsActivity",
						"",
						true);
			}
			else if (itemName == SETTINGS_MENU_MY_ACCOUNT)
			{
				Logger.Info("opening my account app");
				GameManager.sGameManager.ShowApp("My BlueStacks",
						"store.bluestacks.billing",
						"com.bluestacks.bsaccountmanager.AccountHomeActivity_",
						null,
						true);
				/*
                   string myAccountUrl = "https://bluestacks-cloud.appspot.com/myaccount";
                   myAccountUrl += "?guid=" + User.GUID;
                   string label = "My Accounts";
                   GameManager.sGameManager.ShowWebPage(label, myAccountUrl, null);
                   */
			}
			/*
			   else if (itemName == SETTINGS_MENU_HELP)
			   {
			   GameManager.sGameManager.ShowApp("Help",
			   "com.bluestacks.help",
			   "com.bluestacks.help.com.bluestacks.help.HelpActivity",
			   "",
			   true);
			   }
			   */
		}

		private void HandleGuidance(Object sender, MouseEventArgs e)
		{
			Logger.Info("HandleGuidance");

			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				int frontendPort = (int)key.GetValue("FrontendServerPort", 2862);
				string url = String.Format("http://127.0.0.1:{0}/{1}", frontendPort, "keymap");

				Logger.Info("Sending get request to {0}", url);
				string res = Common.HTTP.Client.Get(url, null, false);
				Logger.Info("Got response for {0}: {1}", url, res);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}

		private void HandleFullScreen(Object sender, MouseEventArgs e)
		{
			Logger.Info("HandleFullScreen. FullScreen: " + mGameManager.FullScreen);
			mGameManager.ToggleFullScreen();
			Tab selectedTab = (Tab)mTabBar.GetCurrentTab();
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
		}

		public void ToggleButtons()
		{
			if (mTabBar == null)
				return;

			Tab selectedTab = (Tab)mTabBar.SelectedTab;
			if (selectedTab.mTabType == "app")
			{
				EnableButton(mGuidanceButton);
			}
			else if (selectedTab.mTabType == "web")
			{
				mGuidanceButton.Tag = "tool_key";
				DisableButton(mGuidanceButton);
			}
		}

		private void DisableButton(PictureBox button)
		{
			try
			{
				button.Image = Assets.mAllImagesDict[(String)button.Tag + "_disable"];
			}
			catch (Exception)
			{
				button.Image = Assets.mAllImagesDict[(String)button.Tag + "_hover"];
			}
			button.Enabled = false;
		}

		private void EnableButton(PictureBox button)
		{
			button.Image = Assets.mAllImagesDict[(String)button.Tag];
			button.Enabled = true;
		}

		private void UpdateButton(PictureBox button)
		{
			button.Image = Assets.mAllImagesDict[(String)button.Tag];
		}

		public void ShowLoading()
		{
			mPBLoading.Visible = true;
		}

		public void HideLoading()
		{
			mPBLoading.Visible = false; ;
		}

		public void EnableFullScreenButton()
		{
			if (mTabBar == null)
				return;

			EnableButton(mToggleScreenButton);
		}

		public void DisableFullScreenButton()
		{
			if (mTabBar == null)
				return;

			DisableButton(mToggleScreenButton);
		}

		private void FormClosing(object sender, FormClosingEventArgs e)
		{
			e.Cancel = true;
		}

	}

	public class CustomRenderer : ToolStripProfessionalRenderer
	{
		protected override void OnRenderMenuItemBackground(ToolStripItemRenderEventArgs e)
		{
			if (!e.Item.Selected)
			{
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
				{
					SolidBrush brush = new SolidBrush(GMColors.ContextMenuBackColor);
					Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
					e.Graphics.FillRectangle(brush, rc);
				}
				else
				{
					base.OnRenderMenuItemBackground(e);
					e.Item.BackColor = GMColors.ContextMenuBackColor;
				}

			}
			else
			{
				SolidBrush brush = new SolidBrush(GMColors.ContextMenuHoverColor);
				Rectangle rc = new Rectangle(Point.Empty, e.Item.Size);
				e.Graphics.FillRectangle(brush, rc);
				//e.Graphics.DrawRectangle(Pens.Black, 1, 0, rc.Width - 2, rc.Height - 1);	// border
			}
		}

		protected override void OnRenderItemCheck(ToolStripItemImageRenderEventArgs e)
		{
			if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
			{
				if (e.Item.Selected)
				{
					Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
					SolidBrush b = new SolidBrush(GMColors.ContextMenuHoverColor);

					e.Graphics.FillRectangle(b, rect);
					e.Graphics.DrawImage(e.Image, new Point(5, 3));
				}
				else
				{
					Rectangle rect = new Rectangle(Point.Empty, e.Item.Size);
					SolidBrush b = new SolidBrush(GMColors.ContextMenuBackColor);

					e.Graphics.FillRectangle(b, rect);
					e.Graphics.DrawImage(e.Image, new Point(5, 3));
				}
			}
			else
			{
				base.OnRenderItemCheck(e);
			}
		}

		protected override void OnRenderItemText(ToolStripItemTextRenderEventArgs e)
		{
			base.OnRenderItemText(e);
			{
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
				{
					Image close = e.Item.Image;
					if (close != null)
					{
						e.Graphics.DrawImage(close, CustomRenderer.CloseRectangle(e.Item));
					}
				}
			}
		}

		public static Rectangle CloseRectangle(ToolStripItem item)
		{
			Rectangle re = new Rectangle(item.ContentRectangle.Location.X + item.ContentRectangle.Width - 30, item.ContentRectangle.Y + 1, 24, 24);

			return re;
		}
	}
}
