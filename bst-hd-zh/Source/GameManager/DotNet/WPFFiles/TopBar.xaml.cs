using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Automation.Peers;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for TopBar.xaml
    /// </summary>
    public partial class TopBar : UserControl
    {
        public static TopBar Instance = null;
        Dictionary<TopBarButtons, Image> dictButtons = new Dictionary<TopBarButtons, Image>();

        BackgroundWorker bgSubMenuClosing = new BackgroundWorker();
        enum TopBarButtons
        {
            tool_leftArrow,
            tool_key,
            tool_settings,
            tool_minimize,
            tool_shrink,
            tool_fullscreen,
            tool_close,
        };

        public TopBar()
        {
            Instance = this;
            InitializeComponent();
            SetControlProperties();
            bgSubMenuClosing.DoWork += BgSubMenuClosing_DoWork;
        }

        private void SetControlProperties()
        {
            if (!Oem.Instance.IsTabsEnabled)
            {
                BackButtonGrid.Visibility = Visibility.Hidden;
                mTabButtons.Visibility = Visibility.Hidden;
                TitleBarViewBox.Visibility = Visibility.Visible;
            }

            mBackButton.MouseUp += PbBackButton_MouseUp;
            mBackButton.ToolTip = Locale.Strings.GetLocalizedString("BackTooltip");

			mPremiumButton.MouseUp += PbPremiumButton_MouseUp; ;
			mPremiumButton.ToolTip = Locale.Strings.GetLocalizedString("BuyProTooltip");

			mKeyMappingButton.MouseUp += PbKeyMappingButton_MouseUp;
            mKeyMappingButton.ToolTip = Locale.Strings.GetLocalizedString("GuidanceTooltip");

            mSettingsButton.MouseUp += PbSettingsButton_MouseUp;
            mSettingsButton.ToolTip = Locale.Strings.GetLocalizedString("SettingsTooltip");

            mMinimizeButton.MouseUp += PbMinimizeButton_MouseUp;
            mMinimizeButton.ToolTip = Locale.Strings.GetLocalizedString("MinimizeTooltip");

            mMaximizeButton.MouseUp += PbMaximizeButton_MouseUp;
            mMaximizeButton.ToolTip = Locale.Strings.GetLocalizedString("ToggleScreenTooltip");

            mCloseButton.MouseUp += PbCloseButton_MouseUp;
            mCloseButton.ToolTip = Locale.Strings.GetLocalizedString("CloseTooltip");

			mLblReportProblem.Header = Locale.Strings.GetLocalizedString("ReportProblemText");
			mLblPreferences.Header = Locale.Strings.GetLocalizedString("PreferencesButtonText");
			mLblSettings.Header = Locale.Strings.GetLocalizedString("SettingsText");
			mLblCheckForUpdate.Header = Locale.Strings.GetLocalizedString("CheckForUpdatesText");
			mLblRestart.Header = Locale.Strings.GetLocalizedString("RestartBlueStacks");

        }

		private void PbPremiumButton_MouseUp(object sender, MouseButtonEventArgs e)
		{
			if (ToolBar.Instance.IsOneTimeSetupComplete())
				AppHandler.SendRunAppRequestAsync(AppHandler.GOPROPACKAGE, AppHandler.GOPROACTIVITY);
		}

		private void mSettingsMenu_SubmenuOpened(object sender, RoutedEventArgs e)
        {
            mSettingsButton.SetClickedImage();
        }
        private void mSettingsMenu_SubmenuClosed(object sender, RoutedEventArgs e)
        {
            mSettingsButton.SetDefaultImage();
            bgSubMenuClosing.RunWorkerAsync();
        }

        private void BgSubMenuClosing_DoWork(object sender, DoWorkEventArgs e)
        {
            Thread.Sleep(100);
        }
        private void PbBackButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (TabButtons.Instance.SelectedTab != null)
            {

                if (TabButtons.Instance.SelectedTab.TabType == EnumTabType.app)
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
                else
                {
                    if (TabButtons.Instance.SelectedTab.mBrowser != null)
                        TabButtons.Instance.SelectedTab.mBrowser.GoBack();
                }
            }
            e.Handled = true;
        }

        private void PbKeyMappingButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            Logger.Info("HandleGuidance");

            try
            {
                if (ToolBar.Instance.IsOneTimeSetupComplete())
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
                    int frontendPort = (int)key.GetValue("FrontendServerPort", 2862);
                    string url = String.Format("http://127.0.0.1:{0}/{1}", frontendPort, "keymap");

                    Logger.Info("Sending get request to {0}", url);
                    string res = Common.HTTP.Client.Get(url, null, false);
                    Logger.Info("Got response for {0}: {1}", url, res);
                }
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
            }
            e.Handled = true;
        }

        internal void EnableAllButtons()
        {
            mBackButton.IsEnabled = true;
            mKeyMappingButton.IsEnabled = true;
            mSettingsButton.IsEnabled = true;
            mMinimizeButton.IsEnabled = true;
            mMaximizeButton.IsEnabled = true;
            mCloseButton.IsEnabled = true;
        }

        public void ToggleButtons()
        {
            if (TabButtons.Instance != null)
            {
                TabButton selectedTab = TabButtons.Instance.SelectedTab;
                if (selectedTab.TabType == EnumTabType.app)
                {
                    mKeyMappingButton.IsEnabled = true;
                }
                else if (selectedTab.TabType == EnumTabType.web)
                {
                    mKeyMappingButton.ImageName = "tool_key";
                    mKeyMappingButton.IsEnabled = false;
                }
            }
        }

        internal void ShowDefaultKeyMappingButton()
        {
            mKeyMappingButton.ImageName = "tool_key";
        }

        internal void GlowKeyMappingButton()
        {
            mKeyMappingButton.ImageName = "tool_key_glow";
        }

        internal void DisableKeyMappingButton()
        {
            mKeyMappingButton.ImageName = "tool_key_disable";
            mKeyMappingButton.IsEnabled = false;
        }

        internal void ChangePremiumButton(string proState)
        {
            mPremiumButton.ImageName = proState;
            if (proState == "premium")
                mPremiumButton.ToolTip = Locale.Strings.GetLocalizedString("PremiumTooltip");
            else
                mPremiumButton.ToolTip = Locale.Strings.GetLocalizedString("BuyProTooltip");
        }

        private void PbSettingsButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (!bgSubMenuClosing.IsBusy)
            {
                mSettingsMenu.IsSubmenuOpen = !mSettingsMenu.IsSubmenuOpen;
            }
            e.Handled = true;
        }

        private void PbMinimizeButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GameManagerWindow.Instance.MinimizeWindow();
            e.Handled = true;
        }

        internal void PbMaximizeButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (mMaximizeButton.IsEnabled)
            {
                if (GameManagerWindow.Instance.WindowState != WindowState.Maximized)
                {
                    GameManagerWindow.Instance.MaximizeWindow();
                    mMaximizeButton.ImageName = TopBarButtons.tool_shrink.ToString();
                }
                else
                {
                    GameManagerWindow.Instance.RestoreWindow();
                    mMaximizeButton.ImageName = TopBarButtons.tool_fullscreen.ToString();
                }
            }
            if (e != null)
            {
                e.Handled = true;
            }
        }

        private void PbCloseButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            GameManagerWindow.Instance.Close();
            e.Handled = true;
        }


        internal static void DisableBackButton()
        {
            Instance.mBackButton.IsEnabled = false;
        }

        internal static void EnableBackButton()
        {
            Instance.mBackButton.IsEnabled = true;
        }

        private void ReportProblem_Click(object sender, RoutedEventArgs e)
        {
            GMApi.ReportProblem();
        }

        private void Restart_Click(object sender, RoutedEventArgs e)
        {
			Utils.RestartBlueStacks();
		}

        private void CheckForUpdate_Click(object sender, RoutedEventArgs e)
        {
            Thread t = new Thread(delegate ()
            {
                GMApi.CheckForUpdates();
            });
            t.IsBackground = true;
            t.Start();
        }

        private void Settings_Click(object sender, RoutedEventArgs e)
        {
            RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            string oneTimeSetupDone = (string)configKey.GetValue("OneTimeSetupDone", "no");
            if (string.Equals(oneTimeSetupDone, "yes", StringComparison.OrdinalIgnoreCase))
            {
                //setting show language
                AppHandler.ShowApp(Locale.Strings.Settings, "com.bluestacks.settings", "com.bluestacks.settings.SettingsActivity", "", true);
            }
        }

		private void Preferences_Click(object sender, RoutedEventArgs e)
		{
			new DimWindow(new UserPreferenceWindow());
			if(Preferences.NewGMSize!=new Size())
			{
				ResizeManager.ResizeBegin();
				GameManagerWindow.Instance.Width = Preferences.NewGMSize.Width;
				GameManagerWindow.Instance.Height = Preferences.NewGMSize.Height;
				ResizeManager.ResizeEnd();
				Preferences.NewGMSize = new Size();
			}
		}

		
		private void EditTheme_click(object sender, RoutedEventArgs e)
        {
            ThemeWindow window = new ThemeWindow();
            window.Show();
        }

        private void TopBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if(!e.OriginalSource.GetType().Equals(typeof(CustomPictureBox)))
            {
                GameManagerWindow.Instance.DragMove();
            }
        }

    }
}
