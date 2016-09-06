using System;
using System.Collections.Generic;
using System.Text;
using System.ComponentModel;
using System.Windows.Forms;

using BlueStacks.hyperDroid.Common;
using System.Drawing;
using System.Windows.Input;

namespace BlueStacks.hyperDroid.Frontend {

	public static class FrontendHandler
	{
		public static Console frontend = null;

        public static bool IsFrontendOnTop = false;
		public static Console StartFrontend(string vmName)
		{
			return StartFrontend(vmName, false);
		}
		public static Console StartFrontend(string vmName, bool hideMode)
		{
			Logger.Info("StartFrontend start");
			try
			{
				frontend = new Console(vmName, hideMode);
			}
			catch (Exception ex)
			{
                Logger.Info("Exception in starting frontend" + ex.ToString());
			}
			return frontend;
		}

		public static bool IsFrontendFullScreen()
		{
			Logger.Info("IsFrontendFullScreen start");
			return frontend.mFullScreen;
		}

		public static Size GetConfiguredDisplaySize()
		{
			Logger.Info("GetConfiguredDisplaySize start");
			return frontend.GetConfiguredDisplaySize();
		}

		public static void SetFrontendHideModeValue(bool hideMode)
		{
			Logger.Info("SetFrontendHideModeValue start");
			Console.sHideMode = hideMode;
		}

		public static void HandleFrontendActivated()
		{
			Logger.Info("HandleFrontendActivated start");
			frontend.HandleActivatedEvent(null, null);
		}

		public static void HandleFrontendDeactivated()
		{
			Logger.Info("HandleFrontendDeactivated start");
			frontend.HandleDeactivateEvent(null, null);
		}

		public static void HandleShowFrontend()
		{
			Logger.Info("HandleShowFrontend start");
			frontend.UserShowWindow();
		}

		public static void MuteFrontend()
        {
			Logger.Info("MuteFrontend start");
            frontend.MuteEngine();
        }

		public static void UnmuteFrontend()
		{
			Logger.Info("UnmuteFrontend start");
			frontend.UnMuteEngine();
		}

		public static void ToggleFrontendFullScreen()
		{
			Logger.Info("ToggleFrontendFullScreen start");
			frontend.ToggleFullScreen();
		}

		public static void FrontendUserGoBack()
		{
			Logger.Info("FrontendUserGoBack start");
			frontend.UserGoBack();
		}

		public static void FrontendLanguageChange(Object sender, System.Windows.Forms.InputLanguageChangedEventArgs e)
		{
			Logger.Info("FrontendLanguageChange start");
			frontend.InputLangChanged(sender, e);
		}
		public static int FormClosing(object sender, CancelEventArgs e)
		{
			Logger.Info("FormClosing start");
			return frontend.HandleCloseEvent(sender, e);
		}

		public static void RestartFrontend()
        {
            throw new NotImplementedException();
        }

		public static void SendTabKeyDown(System.Windows.Input.KeyEventArgs e)
		{
			Logger.Info("SendTabKeyDown start");
			if (IsFrontendOnTop)
			{
				if (e.Key == Key.Left)
				{
					frontend.HandleKeyDown(null, new System.Windows.Forms.KeyEventArgs(Keys.Left));
					e.Handled = true;
				}
				if (e.Key == Key.Right)
				{
					frontend.HandleKeyDown(null, new System.Windows.Forms.KeyEventArgs(Keys.Right));
					e.Handled = true;
				}
				if (e.Key == Key.Down)
				{
					frontend.HandleKeyDown(null, new System.Windows.Forms.KeyEventArgs(Keys.Down));
					e.Handled = true;
				}
				if (e.Key == Key.Up)
				{
					frontend.HandleKeyDown(null, new System.Windows.Forms.KeyEventArgs(Keys.Up));
					e.Handled = true;
				}
				if (e.Key == Key.Tab)
				{
					frontend.HandleKeyDown(null, new System.Windows.Forms.KeyEventArgs(Keys.Tab));
					e.Handled = true;
				}
			}
		}

		public static void ResizezBanner()
		{
			Logger.Info("ResizezBanner start");
			if (frontend.Tag != null && frontend.Visible && Oem.Instance.IsUseFrontendBanner)
			{
				FrontendBanner.SetImageAsBackground((System.Drawing.Image)frontend.Tag, frontend);
			}
		}
	}
}