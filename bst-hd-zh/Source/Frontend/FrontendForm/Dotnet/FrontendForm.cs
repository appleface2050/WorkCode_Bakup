using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using System.Runtime.InteropServices;

using Microsoft.Win32;

using BlueStacks.hyperDroid.Frontend;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.FrontendForm
{
    public partial class FrontendForm : Form
    {

        BlueStacks.hyperDroid.Frontend.Console frontendConsole;
        public FrontendForm(string vmName, bool hideMode)
        {
            InitializeComponent();

			frontendConsole = Frontend.FrontendHandler.StartFrontend(vmName, hideMode);

            Thread httpThread = new Thread(delegate ()
		   {
			   HttpHandlerSetup.InitHTTPServer(HTTPHandler.GetRoutes(), null, false);

		   });
            httpThread.IsBackground = true;
            httpThread.Start();

			InitForm();
            Controls.Add(frontendConsole);

			SetupEventHooks();
        }

		private void InitForm()
		{
			/* Set form icon */
			AssignFormIcon();

			/* Set form title */
			this.Text = Oem.Instance.CommonAppTitleText;
			/* when we launch frontend in hidden mode
			 * if border is not set to none, the frontend appears then disappears
			 * using borderstyle none, fixes the issue
			 */
			this.FormBorderStyle = FormBorderStyle.None;

			this.MinimizeBox = true;
			this.MaximizeBox = false;
			this.DoubleBuffered = true;
			this.BackColor = Color.Black;
			this.ForeColor = Color.LightGray;
			this.ClientSize = FrontendHandler.GetConfiguredDisplaySize();
            frontendConsole.Dock = DockStyle.Fill;
		}

        private void SetupEventHooks()
        {
            this.Activated += FrontendForm_Activated;
            this.Deactivate += FrontendForm_Deactivate;
			this.InputLanguageChanged += FrontendForm_InputLanguageChanged;
            this.FormClosing += FrontendForm_FormClosing;

			frontendConsole.FrontendClose += frontendConsole_FrontendClose;
        }

		void frontendConsole_FrontendClose(object sender, EventArgs e)
		{
			Environment.Exit(0);
		}

		void FrontendForm_InputLanguageChanged(object sender, InputLanguageChangedEventArgs e)
		{
			FrontendHandler.FrontendLanguageChange(sender, e);
		}

		void FrontendForm_Deactivate(object sender, EventArgs e)
		{
			FrontendHandler.HandleFrontendDeactivated();
		}

		void FrontendForm_Activated(object sender, EventArgs e)
		{
			FrontendHandler.HandleFrontendActivated();
		}

        private void FrontendForm_FormClosing(object sender, FormClosingEventArgs e)
        {
			int ret = FrontendHandler.FormClosing(sender, e);
			Environment.Exit(ret);
        }

		private void AssignFormIcon()
		{
			this.Icon = Utils.GetApplicationIcon();
		}

		#region wmdProcUserMessageHandling
		private void HandleWMUserResizeWindow(int wParam, int lParam)
		{
			if (FrontendHandler.IsFrontendFullScreen())
			{
				Logger.Info("In fullscreen mode. Not resizing.");
				return;
			}

			int width, height;
			if (wParam != 0 && lParam != 0)
			{
				width = wParam;
				height = lParam;
			}
			else
			{
				Size displaySize = FrontendHandler.GetConfiguredDisplaySize();
				width = displaySize.Width;
				height = displaySize.Height;
			}

			this.ClientSize = new Size(width, height);

		}

		private void HandleWMUserHideWindow()
		{
			this.Hide();
			FrontendHandler.SetFrontendHideModeValue(true);
		}

		private void HandleWMUserShowWindow()
		{
			FrontendHandler.SetFrontendHideModeValue(false);
			FrontendHandler.HandleShowFrontend();
			this.Show();
			this.WindowState = FormWindowState.Normal;
		}

		private void HandleFrontendActivated()
		{
			FrontendHandler.HandleFrontendActivated();
		}

		private void HandleFrontendDeactivated()
		{
			FrontendHandler.HandleFrontendDeactivated();
		}

		private void HandleFrontendMute()
		{
			FrontendHandler.MuteFrontend();
		}

		private void HandleFrontendUnMute()
		{
			FrontendHandler.UnmuteFrontend();
		}

		private void HandleToggleFullScreen()
		{
			FrontendHandler.ToggleFrontendFullScreen();
		}

		private void HandleWMUserGoBack()
		{
			FrontendHandler.FrontendUserGoBack();
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case Common.Interop.Window.WM_USER_HIDE_WINDOW:
					Logger.Info("Received message WM_USER_HIDE_WINDOW");
					HandleWMUserHideWindow();
					break;

				case Common.Interop.Window.WM_USER_SHOW_WINDOW:
					Logger.Info("Received message WM_USER_SHOW_WINDOW");
					HandleWMUserShowWindow();
					break;

			case Common.Interop.Window.WM_USER_RESIZE_WINDOW:
					Logger.Info("Received message WM_USER_RESIZE_WINDOW");
					int wParam = m.WParam.ToInt32();
					int lParam = m.LParam.ToInt32();
					Logger.Info("WParam: " + wParam);
					Logger.Info("LParam: " + lParam);
					HandleWMUserResizeWindow(wParam, lParam);
					break;

				case Common.Interop.Window.WM_USER_ACTIVATE:
					Logger.Info("Received message WM_USER_ACTIVATE");
					HandleFrontendActivated();
					break;

				case Common.Interop.Window.WM_USER_DEACTIVATE:
					Logger.Info("Received message WM_USER_DEACTIVATE");
					HandleFrontendDeactivated();
					break;

				case Common.Interop.Window.WM_USER_AUDIO_MUTE:
					Logger.Info("Received message WM_USER_AUDIO_MUTE");
					HandleFrontendMute();
					break;

				case Common.Interop.Window.WM_USER_AUDIO_UNMUTE:
					Logger.Info("Received message WM_USER_AUDIO_UNMUTE");
					HandleFrontendUnMute();
					break;

				case Common.Interop.Window.WM_USER_SHOW_GUIDANCE:
					Logger.Info("Received message WM_USER_SHOW_GUIDANCE");
					Common.VmCmdHandler.RunCommand(Common.Strings.ShowGuidanceUrl);
					break;

				case Common.Interop.Window.WM_USER_TOGGLE_FULLSCREEN:
					Logger.Info("Received message WM_USER_TOGGLE_FULLSCREEN");
					HandleToggleFullScreen();
					break;

				case Common.Interop.Window.WM_USER_GO_BACK:
					Logger.Info("Received message WM_USER_GO_BACK");
					HandleWMUserGoBack();
					break;
			}
			base.WndProc(ref m);
		}
		#endregion
	}
}
