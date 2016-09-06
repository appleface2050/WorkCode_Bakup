using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for GameManagerWindow.xaml
	/// </summary>
	public partial class GameManagerWindow : Window
	{
		public static GameManagerWindow Instance = null;
		public bool mForceClose = false;
		bool mSessionEnding = false;

		public static bool IsCheckForSlideOut = false;

		public GameManagerWindow()
		{
			Instance = this;

			/*if (Utils.CheckHyperVWithNestedVirtualizationEnabled())
			{
				System.Windows.Forms.MessageBox.Show(Locale.Strings.HyperVAlertMessage, "Warning", MessageBoxButtons.OK, MessageBoxIcon.Warning);
				Process.Start("https://msdn.microsoft.com/en-us/virtualization/hyperv_on_windows/quick_start/walkthrough_install");
				Environment.Exit(0);
			}*/
			InitializeComponent();

			WpfUtils.SetWindowSizeAndLocation(this, "GM", true);
			this.FontFamily = GameManagerUtilities.GetFont();
			this.Closing += GameManagerWindow_Closing;
			this.SourceInitialized += Window_SourceInitialized;
			this.Title = GameManagerUtilities.WindowTitle;
			this.Loaded += GameManagerWindow_Loaded;
			this.LocationChanged += GameManagerWindow_LocationChanged;
			this.Activated += GameManagerWindow_Activated;
			this.Deactivated += GameManagerWindow_Deactivated;
			this.IsVisibleChanged += GameManagerWindow_IsVisibleChanged;
			this.SizeChanged += GameManagerWindow_SizeChanged;
			this.StateChanged += GameManagerWindow_StateChanged;
			this.PreviewKeyDown += GameManagerWindow_PreviewKeyDown;
			mTopBar.MouseLeave += TopBar_MouseLeave;

			timer.Elapsed += Timer_Elapsed;
			Logger.Info("program data {0}", Browser.sHomeDataDir);//Required to call browser static contructor
			if (!Oem.Instance.IsTabsEnabled)
			{
				Grid.SetColumn(mTopBar, 0);
				Grid.SetRow(mToolBar, 1);
				Grid.SetRow(divider, 1);
			}
		}



		System.Timers.Timer timer = new System.Timers.Timer(50);
		private void Timer_Elapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			Dispatcher.Invoke(new Action(() =>
			{
				if (MainGrid.Margin.Top != -31)
				{
					MainGrid.Margin = new Thickness(1, MainGrid.Margin.Top - 1, 1, 1);
				}
				else
				{
					timer.Enabled = false;
				}
			}));
		}

		internal void GameManagerWindow_StateChanged(object sender, EventArgs e)
		{
			int hideTabs = Preferences.ReadFromRegistry("Hidetabs");
			if (hideTabs == 1)
			{
				if (this.WindowState == WindowState.Maximized)
				{
					SlideOutTopBar();
				}
				if (this.WindowState == WindowState.Normal)
				{
					SlideInTopBar();
					IsCheckForSlideOut = false;
				}
			}
		}
		internal void SlideInTopBar()
		{
			if (ResizeManager.IsCheckForSlideIn)
			{
				Grid.SetRow(mContent, 1);
				ResizeManager.IsCheckForSlideIn = false;
				((Storyboard)this.FindResource("SlideIn")).Begin();
				IsCheckForSlideOut = true;
			}
		}

		private void SlideOutTopBar()
		{
			((Storyboard)this.FindResource("SlideOut")).Begin();
			ResizeManager.IsCheckForSlideIn = true;
		}

		private void DoubleAnimation_Completed(object sender, EventArgs e)
		{
			Grid.SetRow(mContent, 0);
		}

		private void TopBar_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (IsCheckForSlideOut)
			{
				IsCheckForSlideOut = false;
				SlideOutTopBar();
			}
		}

		private void GameManagerWindow_PreviewKeyDown(object sender, System.Windows.Input.KeyEventArgs e)
		{
			if (e.Key == Key.F11)
			{
				TopBar.Instance.PbMaximizeButton_MouseUp(null, null);
				e.Handled = true;
			}
			FrontendHandler.SendTabKeyDown(e);
		}

		private void SetToolBarButtonState()
		{
			if (ToolBar.Instance.IsOneTimeSetupComplete())
			{
				if (TabButtons.Instance.SelectedTab.TabType == EnumTabType.app)
				{
					mToolBar.EnableGenericAppTabButtons(true);
					if (GameManagerUtilities.ToggleDisableAppList.Contains(TabButtons.Instance.SelectedTab.mPackageName)
						|| TabButtons.Instance.SelectedTab.mPackageName != AppHandler.OTSPACKAGEINFO)
					{
						mToolBar.EnableToggleAppTabButton(false);
					}
					else
					{
						mToolBar.EnableToggleAppTabButton(true);
					}
				}
				else
				{
					mToolBar.EnableAppTabButtons(true);
				}
			}
		}

		private void GameManagerWindow_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			Logger.Info("GM: HandleResizeEvent, this.width = {0}; this.Height = {1}",
					this.Width, this.Height);


			if (this.WindowState == WindowState.Minimized)
			{

				StreamViewTimeStats.NotifyToAllTabStats(StreamViewStatsEventName.WindowHidden);
			}
			else if (this.WindowState != WindowState.Minimized)
			{

				StreamViewTimeStats.NotifyToAllTabStats(StreamViewStatsEventName.WindowVisible);
			}
			GameManagerResized();
		}

		private void GameManagerWindow_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			if (this.mSessionEnding)
			{
				return;
			}

			if (this.IsVisible)
			{
				if (Utils.FindProcessByName("BlueStacksTV"))
					BTVManager.ShowStreamWindow();
			}
			else
			{
				if (Utils.FindProcessByName("BlueStacksTV"))
					BTVManager.HideStreamWindow();
			}
		}

		private void GameManagerWindow_Deactivated(object sender, EventArgs e)
		{
			Logger.Info("HandleDeactivateEvent");
			StreamViewTimeStats.NotifyToAllTabStats(StreamViewStatsEventName.WindowDeactivated);

			if (FrontendHandler.IsFrontendOnTop)
				FrontendHandler.HandleFrontendDeactivated();
		}

		private void GameManagerWindow_Activated(object sender, EventArgs e)
		{
			Logger.Info("HandleActivatedEvent");
			StreamViewTimeStats.NotifyToAllTabStats(StreamViewStatsEventName.WindowActivated);

			if (FrontendHandler.IsFrontendOnTop)
				FrontendHandler.HandleFrontendActivated();
		}

		private void GameManagerWindow_LocationChanged(object sender, EventArgs e)
		{

			if (this.WindowState == WindowState.Minimized)
			{
				return;
			}
		}


		private void GameManagerWindow_Loaded(object sender, RoutedEventArgs e)
		{
			GameManagerUtilities.IsUserPro();
			ShowHome();

			int hideBTVWindow = Preferences.ReadFromRegistry("HideBTVWindow");
			if (Oem.Instance.IsBTVBuild && hideBTVWindow == 0)
			{
				BTVManager.ShowStreamWindow();
			}
			if (Oem.Instance.IsSendBTVFunnelStats)
			{
				Stats.SendBtvFunnelStats("saw_go_live_button", null, null, true);
			}
			//TabButtons.Instance.SelectedTab = TabButtons.Instance.mDictTabs.ElementAt(0).Value;
			//if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
			//{
			//	Thread td = new Thread(new ThreadStart(InitBossHotKey));
			//	td.Start();
			//}
			if (GameManagerUtilities.IsFirstLaunch)
			{
				GameManagerUtilities.LaunchWelcomeNote();
			}
		}

		private void ShowHome()
		{
			GameManagerUtilities.UpdateLocalUrls();

			TopBar.Instance.EnableAllButtons();

			RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)reg.GetValue("InstallDir");
			string agentFile = System.IO.Path.Combine(installDir, @"HD-Agent.exe");
			Process.Start(agentFile);

			reg.Close();

			/*
			 * The above function call causes focus to move away from Game Manager
			 * To get focus back reliably, use this code instead:
			 Process proc = Utils.StartHiddenFrontend();
			 proc.WaitForInputIdle();
			 Window.SetForegroundWindow(this.Handle);
			 this.Focus();
			 * The problem with using this is that WaitForInputIdle() may take some time
			 * and the UI thread will be blocked the call.
			 */
			TabButtons.Instance.AddStartupTabs();
			SetToolBarButtonState();

			if (!String.IsNullOrEmpty(GameManagerUtilities.args.p))
			{
				AppHandler.ShowApp(null, GameManagerUtilities.args.p, GameManagerUtilities.args.a, "", true);
			}
		}

		private void GameManagerWindow_Closing(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Logger.Info("In OnFormClosing. mSessionEnding = " + mSessionEnding);
			ResizeManager.DisableResizng();

			WpfUtils.SaveWindowSizeAndLocation(this, "GM");
			WpfUtils.SaveControlSize(mContent, "Window");
			if (mSessionEnding == true)
			{
				StreamViewTimeStats.HandleWindowCloseSession();
				Environment.Exit(1);
			}
			else
			{
				int showPrompt = Preferences.ReadFromRegistry("BlueStacksCloseWarning");
				if (showPrompt == 1 && !mForceClose)
				{
					DialogResult keepGMRunning = CustomMessageBox.ShowMessageBox(this, "BlueStacks",
							Locale.Strings.GetLocalizedString("ClosePopupText"),
							Locale.Strings.GetLocalizedString("YesText"),
							Locale.Strings.GetLocalizedString("NoText"),
							Locale.Strings.GetLocalizedString("CancelText"),
							Locale.Strings.GetLocalizedString("RememberChoiceText"),
							true);

					if (keepGMRunning == System.Windows.Forms.DialogResult.Yes)
						Utils.UpdateRegistry(Common.Strings.GMPreferencesPath, "KeepGMRunning", "true", RegistryValueKind.String);
					else if (keepGMRunning == System.Windows.Forms.DialogResult.No)
						Utils.UpdateRegistry(Common.Strings.GMPreferencesPath, "KeepGMRunning", "false", RegistryValueKind.String);
					else
					{
						Logger.Info("User chose cancel...not quiting");
						e.Cancel = true;
						return;
					}

					int choice = GameManagerUtilities.sRememberClosingPopupChoice == true ? 0 : 1;
					Preferences.WriteToregistry("BlueStacksCloseWarning", choice);
					e.Cancel = true;
				}
				this.CloseGameManager();
			}
		}

		private void CloseGameManager()
		{
			if (ChatWindow.Instance != null)
			{
				ChatWindow.Instance.Close();
			}

			StreamViewTimeStats.HandleWindowCloseSession();
			this.Hide();

			BTVManager.CloseBTV();

			foreach (var item in TabButtons.Instance.mDictTabs.Keys.ToList())
			{
				TabButtons.Instance.CloseTab(item);
			}

			try
			{
				Gecko.Xpcom.Shutdown();
			}
			catch (Exception e)
			{
				Logger.Error("Xpcom shutdown failed: " + e.ToString());
			}

			if (BTVManager.sStreaming)
			{
				Stats.SendBtvFunnelStats("stream_ended",
					"stream_ended_reason",
					"app_player_closed",
					false);
			}

			string keepRunning = Utils.GetValueFromRegistry(Common.Strings.GMPreferencesPath, "KeepGMRunning", "false");
			if (string.Compare(keepRunning, "false", true) == 0 || mForceClose)
			{
				FrontendHandler.FormClosing(null, null);
				GameManagerUtilities.sGameManagerLock.Close();
				HttpHandlerSetup.Server.Stop();
				GmClose();
			}
		}

		private void GmClose()
		{
			Logger.Info("GmClose()");
			//mStopRpcTroubleShooter = true;
			Utils.KillProcessByName("HD-RPCErrorTroubleShooter");
			Logger.Info("Exiting");
			Environment.Exit(0);
		}

		private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			const int WM_QUERYENDSESSION = 0x11;
			switch (msg)
			{
				case Common.Interop.Window.WM_USER_SHOW_WINDOW:
					Logger.Info("Received message WM_USER_SHOW_WINDOW");
					this.Show();
					return new System.IntPtr(1);
				case WM_QUERYENDSESSION:
					Logger.Info("Received message WM_QUERYENDSESSION");
					mSessionEnding = true;
					break;
				default:
					break;
			}
			return IntPtr.Zero;
		}


		internal IntPtr Handle
		{
			get
			{
				return new WindowInteropHelper(this).Handle;
			}
		}




		private void Window_SourceInitialized(object sender, EventArgs ea)
		{
			HwndSource source = PresentationSource.FromVisual(this) as HwndSource;
			source.AddHook(WndProc);

			ResizeManager.Init();
		}

		internal void MinimizeWindow()
		{
			WindowState = WindowState.Minimized;
		}

		internal void MaximizeWindow()
		{
			HideToolBar(true);
			WindowState = WindowState.Maximized;
			FrontendHandler.ResizezBanner();
			if (BTVManager.sRecording)
			{
				GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = false;
				ResizeManager.DisableResizng();
				BTVManager.sFullScreenClicked = true;

				Thread thread = new Thread(delegate ()
				{
					BTVManager.SendBTVRequest("stoprecord", null);
					BTVManager.SetConfig();
					BTVManager.SendBTVRequest("startrecord", null);
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}

		internal void RestoreWindow()
		{
			HideToolBar(false);
			WindowState = WindowState.Normal;
			FrontendHandler.ResizezBanner();
			if (BTVManager.sRecording)
			{
				GameManagerWindow.Instance.mTopBar.mMaximizeButton.IsEnabled = false;
				ResizeManager.DisableResizng();
				BTVManager.sFullScreenClicked = true;

				Thread thread = new Thread(delegate ()
				{
					BTVManager.SendBTVRequest("stoprecord", null);
					BTVManager.SetConfig();
					BTVManager.SendBTVRequest("startrecord", null);
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}
		public void HideToolBar(bool isHide)
		{
			int showLeftToolBar = Preferences.ReadFromRegistry("ShowLeftToolBar");
			if (isHide || showLeftToolBar == 0)
			{
				ToolBarColumn.Width = new GridLength(0);
				DividerColumn.Width = new GridLength(0);
			}
			else
			{
				ToolBarColumn.Width = new GridLength(57);
				DividerColumn.Width = new GridLength(2);
			}
		}



		public void GameManagerResized()
		{
			if (this.WindowState == WindowState.Minimized)
			{
				return;
			}
		}



		public void ShakeWindow()
		{
			ShakeWindow(10);
		}

		public void ShakeWindow(int numSteps)
		{
			int movement = 5;
			int dx = 0;
			int step = 0;
			while (numSteps > 0)
			{

				if (step == 0)
				{
					dx = movement;
				}
				else if (step == 1)
				{
					dx = movement * -1;
				}
				else if (step == 2)
				{
					dx = movement * -1;
				}
				else if (step == 3)
				{
					dx = movement;
				}

				step++;
				if (step == 4)
				{
					step = 0;
					numSteps--;
				}

				this.Left = this.Left + dx;
				Thread.Sleep(30);
			}
		}

		internal void ForceClose()
		{
			if (CustomMessageBox.sIsCloseMessageBoxUp)
			{
				Environment.Exit(0);
			}
			else
			{
				GameManagerWindow.Instance.Dispatcher.Invoke((Action)(() =>
							{
								mForceClose = true;
								this.Close();
							}));
			}
		}

		#region BlinkTaskbarIcon
		/* Flash taskbar icon for notifications: starts */
		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		static extern bool FlashWindowEx(ref FLASHWINFO pwfi);
		//Flash both the window caption and taskbar button.
		//This is equivalent to setting the FLASHW_CAPTION | FLASHW_TRAY flags.
		public const UInt32 FLASHW_ALL = 3;

		// Flash continuously until the window comes to the foreground.
		public const UInt32 FLASHW_TIMERNOFG = 12;
		[StructLayout(LayoutKind.Sequential)]
		public struct FLASHWINFO
		{
			public UInt32 cbSize;
			public IntPtr hwnd;
			public UInt32 dwFlags;
			public UInt32 uCount;
			public UInt32 dwTimeout;
		}
		public void BlinkTaskbarIcon()
		{
			IntPtr hWnd = this.Handle;
			FLASHWINFO fInfo = new FLASHWINFO();

			fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
			fInfo.hwnd = hWnd;
			fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
			fInfo.uCount = UInt32.MaxValue;
			fInfo.dwTimeout = 0;

			FlashWindowEx(ref fInfo);
		}
		#endregion
	}
}
