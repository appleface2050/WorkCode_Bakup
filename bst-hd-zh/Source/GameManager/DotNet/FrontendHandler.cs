using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend;
using System;
using System.Windows.Forms;
using System.Windows.Input;

namespace BlueStacks.hyperDroid.GameManager
{
    public static class FrontendHandler
    {
        public static bool IsFrontendOnTop = false;

        public static Frontend.Console frontend = null;

        public static Frontend.Console StartFrontend()
        {
            try
            {
                frontend = new Frontend.Console("Android");
            }
            catch (Exception ex)
            {
                Logger.Info("Exception in starting frontend" + ex.ToString());
            }
            return frontend;
        }

        internal static void CloseFrontend()
        {
            int retCode = FrontendEventManager.OnFormClosing(null, null);
            Logger.Info("Frontend Return code" + retCode);
        }

		internal static void ActivateFrontend()
		{
			if(frontend != null)
				FrontendEventManager.OnFrontendActivated(null, null);
		}

		internal static void DeactivateFrontend()
		{
			if(frontend != null)
				FrontendEventManager.OnFrontendDeactivated(null, null);
		}

        internal static void RestartFrontend()
        {
            throw new NotImplementedException();
        }

        internal static void MuteFrontend()
        {
            frontend.MuteEngine();
        }

        internal static void UnmuteFrontend()
        {
            frontend.UnMuteEngine();
        }

        internal static void SendTabKeyDown(System.Windows.Input.KeyEventArgs e)
        {
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

        internal static void ResizezBanner()
        {
            if (frontend.Tag != null && frontend.Visible && Oem.Instance.IsUseFrontendBanner)
            {
                FrontendBanner.SetImageAsBackground((System.Drawing.Image)frontend.Tag, frontend);
            }
        }
    }
}
