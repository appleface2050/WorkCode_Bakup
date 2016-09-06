using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Cloud.Services;

namespace BlueStacks.hyperDroid.GameManager
{
	public class ChatWindow : Form
	{
		[DllImportAttribute("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();

		public const int	WM_NCLBUTTONDOWN	= 0xA1;
		public const int	HT_CAPTION		= 0x2;

		private ChatBrowser mChatBrowser;
		private string mAppName = "Default";
		private string mRegion = "Default";
		private string mAppPackage = "Default";

		private int mChromeHeight;
		private int mContentWidth;
		private int mContentHeight;

		private int mBorderSize				= 1;
		private int mPadding				= 10;

		private int mButtonWidth;
		private int mButtonHeight;

		private const int mGrip				= 5;

		private PictureBox mPBIcon;

		private PictureBox mPBCloseBtn;
		private PictureBox mPBMinimizeBtn;

		public bool mIsOBSReParented = false;
		private Label mLblBluestacksChat;

		private ToolTip mCloseToolTip;
		private ToolTip mMinimizeToolTip;
		public	IntPtr	mOBSHandle = IntPtr.Zero;
		public  Panel   mOBSRenderFrame;

		private Dictionary<string, Image> mAllImagesDict		= null;

		public static string BROWSER_NAME = "ChatBrowser";
		internal static ChatWindow Instance = null;
		bool allowResize = false;

		const int CS_DROPSHADOW = 0x00020000;
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;

			}
		}

		public ChatWindow(Point defaultLocation, int contentWidth, int contentHeight, int totalHeightDiff)
		{
			Instance = this;
			mButtonHeight		= totalHeightDiff - (2 * mBorderSize);
			//mButtonWidth		= ControlBar.GetButtonWidth(mButtonHeight);
			mPadding		= (totalHeightDiff - mButtonHeight - (2 * mBorderSize))/2;
			mChromeHeight		= mBorderSize + 2 * mPadding + mButtonHeight + mBorderSize;
			mContentWidth           = contentWidth;
			mContentHeight          = contentHeight;

			int formWidth = mContentWidth + 2 * mBorderSize;
			int formHeight = mContentHeight + mChromeHeight;

			if (mAllImagesDict == null)
			{
                //string assetsDir = GameManager.sAssetsDir;

                //mAllImagesDict = new Dictionary<string, Image>();
                //string path = Directory.GetCurrentDirectory();

                //mAllImagesDict.Add("tool_close",
                //        Image.FromFile(Path.Combine(assetsDir, "close_button.png")));
                //mAllImagesDict.Add("tool_close_hover",
                //        Image.FromFile(Path.Combine(assetsDir, "close_button_hover.png")));
                //mAllImagesDict.Add("tool_close_click",
                //        Image.FromFile(Path.Combine(assetsDir, "close_button_click.png")));
                //mAllImagesDict.Add("tool_minimize",
                //        Image.FromFile(Path.Combine(assetsDir, "minimize_button.png")));
                //mAllImagesDict.Add("tool_minimize_hover",
                //        Image.FromFile(Path.Combine(assetsDir, "minimize_button_hover.png")));
                //mAllImagesDict.Add("tool_minimize_click",
                //        Image.FromFile(Path.Combine(assetsDir, "minimize_button_click.png")));
                //mAllImagesDict.Add("tool_icon",
                //        Image.FromFile(Path.Combine(assetsDir, "btv_logo_two.png")));
			}

			this.StartPosition = FormStartPosition.Manual;
			this.Size = new Size(formWidth, formHeight);

			this.Location = GetChatWindowLocation(defaultLocation);
			this.LocationChanged += new System.EventHandler(this.HandleLocationChanged);

			this.BackColor = GMColors.StreamWindowBackColor;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMouseDown);
			this.Icon = Utils.GetApplicationIcon();
			this.FormClosing += HandleCloseEvent;

			Init();
		}
		
		internal static void ShowChatWindow()
		{
			if (Instance == null)
			{
				AddChatWindow();
			}
			else
			{
				Instance.BringToFront();
			}
		}

		internal static void AddChatWindow()
		{
			Instance = new ChatWindow(new Point(0,0),100,400,30);
                    //sStreamWindowDefaultLocation,
                    //sStreamWindowDefaultContentSize.Width,
                    //sStreamWindowDefaultContentSize.Height,
                    //sStreamWindowDefaultChromeHeight);
			Instance.Show();
		}

		private void FormMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}

		public void SetAppDetails(string appName, string appPackage, string region) 
		{
			
			mAppPackage = appPackage;
			mAppName = appName;
			mRegion = region;
			//We are not going to call js APIs via c# in V1
			//mChatBrowser.ChangeRoom(appPackage);
		}

		public void Init()
		{
			this.SuspendLayout();
			this.Text = "BlueStacks Chat";

			mPBCloseBtn = new PictureBox();
			mPBCloseBtn.Tag = "tool_close";
			mPBCloseBtn.Location = new Point(this.Width - mButtonWidth - mBorderSize - mPadding, mPadding + mBorderSize);
			mPBCloseBtn.MouseClick += HandleCloseMouseClick;
			SetButtonProperties(mPBCloseBtn);

			mPBMinimizeBtn = new PictureBox();
			mPBMinimizeBtn.Tag = "tool_minimize";
			mPBMinimizeBtn.Location = new Point(this.Width - (2 * mButtonWidth) - mBorderSize - (2 * mPadding), mPadding + mBorderSize);
			mPBMinimizeBtn.MouseClick += HandleMinimizeMouseClick;
			SetButtonProperties(mPBMinimizeBtn);

			mPBIcon = new PictureBox();
			mPBIcon.Tag = "tool_icon";
			mPBIcon.Location = new Point(mPadding + mBorderSize, mPadding + mBorderSize);
			//mPBIcon.Image = mAllImagesDict[(String)mPBIcon.Tag];
			mPBIcon.SizeMode = PictureBoxSizeMode.StretchImage;
			mPBIcon.Size = new Size(mButtonWidth, mButtonHeight);
			mPBIcon.BackColor = GMColors.TransparentColor;

			mCloseToolTip = new ToolTip();
			mCloseToolTip.OwnerDraw = true;
			mCloseToolTip.Draw += mMinimizeToolTip_Draw;
			mCloseToolTip.ShowAlways = true;
			mCloseToolTip.SetToolTip(mPBCloseBtn,Locale.Strings.GetLocalizedString("CloseTooltip"));

			mCloseToolTip.BackColor = GMColors.ToolTipBackColor;
			mCloseToolTip.ForeColor = GMColors.ToolTipForeColor;

			mMinimizeToolTip = new ToolTip();
			mMinimizeToolTip.OwnerDraw = true;
			mMinimizeToolTip.Draw+=mMinimizeToolTip_Draw;
			mMinimizeToolTip.ShowAlways = true;
			mMinimizeToolTip.SetToolTip(mPBMinimizeBtn, Locale.Strings.GetLocalizedString("MinimizeTooltip"));

			mMinimizeToolTip.BackColor = GMColors.ToolTipBackColor;
			mMinimizeToolTip.ForeColor = GMColors.ToolTipForeColor;

			this.mLblBluestacksChat = new Label();
			this.mLblBluestacksChat.Text = "BlueStacks Chat";
			this.mLblBluestacksChat.MouseDown += FormMouseDown;

			float fontSize = (float)(mChromeHeight * 10) / 20;
			if (fontSize < 11)
			{
				fontSize = 11;
			}

			this.ForeColor = GMColors.StreamWindowTitleForeColor;
			this.mLblBluestacksChat.AutoSize = true;
			//this.mLblBluestacksChat.Font = new Font( fontSize, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
			this.mLblBluestacksChat.Location = new Point(mChromeHeight / 6, mChromeHeight/6);

			this.Controls.Add(mLblBluestacksChat);
			this.Controls.Add(mPBCloseBtn);
			this.Controls.Add(mPBMinimizeBtn);
			//float zoomFactor = GameManager.sScale;

			string chatProdUrl = String.Format("{0}/{1}", Service.Host, Common.Strings.ChatApplicationUrl);

			RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string chatUrl = (string)urlKey.GetValue(
					Common.Strings.GMChatUrlKeyName,
					chatProdUrl
					);

			mChatBrowser = new ChatBrowser(chatUrl);
			mChatBrowser.Name = BROWSER_NAME;
			mChatBrowser.Size = new Size(mContentWidth, mContentHeight);
			mChatBrowser.Location = new Point(mBorderSize, mChromeHeight);
			//mChatBrowser.GetMarkupDocumentViewer().SetFullZoomAttribute(zoomFactor);

			//Logger.Info("using zoom: " + zoomFactor);

			this.Controls.Add(mChatBrowser);
			this.ResumeLayout(false);
		}

		public void DisposeBrowser()
		{
			if (mChatBrowser != null)
			{
				mChatBrowser.Dispose();
			}
		}

		private void HandleCloseEvent(Object o, CancelEventArgs e)
		{
			Logger.Info("ChatWindow: HandleCloseEvent");

			this.Hide();
			ChatWindow.Instance = null;
			DisposeBrowser();
		}

		void mMinimizeToolTip_Draw(object sender, DrawToolTipEventArgs e)
		{
			e.DrawBackground();
			e.DrawBorder();
			e.DrawText();
		}

		private void SetButtonProperties(PictureBox button)
		{
			//button.Image = mAllImagesDict[(String)button.Tag];
			button.SizeMode = PictureBoxSizeMode.StretchImage;
			button.Size = new Size(mButtonWidth, mButtonHeight);
			button.MouseEnter += new EventHandler(this.ControlBarButtonMouseEnter);
			button.MouseDown += new MouseEventHandler(this.ControlBarButtonMouseDown);
			button.MouseUp += new MouseEventHandler(this.ControlBarButtonMouseUp);
			button.MouseLeave += new EventHandler(this.ControlBarButtonMouseLeave);
			button.BackColor = GMColors.TransparentColor;
		}

		private void ControlBarButtonMouseEnter(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
			}
			if (button.Enabled)
			{
				button.Cursor = Cursors.Hand;
			//	button.Image = mAllImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ControlBarButtonMouseDown(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
			}
			if (button.Enabled)
			{
			//	button.Image = mAllImagesDict[(String)button.Tag + "_click"];
			}
		}

		private void ControlBarButtonMouseUp(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
				if (button == null)
				{
					return;
				}
			}
			if (button.Enabled)
			{
			//	button.Image = mAllImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ControlBarButtonMouseLeave(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
			}
			if (button.Enabled)
			{
				button.Cursor = Cursors.Default;
			//	button.Image = mAllImagesDict[(String)button.Tag];
			}
		}

		private void HandleMinimizeMouseClick(Object sender, MouseEventArgs e)
		{
			this.WindowState = FormWindowState.Minimized;
		}

		private void HandleCloseMouseClick(Object sender, MouseEventArgs e)
		{
			this.Close();
		}

		private Point GetChatWindowLocation(Point defaultLocation)
		{
			RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			Point location = defaultLocation;

			int regX = (int)configKey.GetValue(Strings.GMChatWindowLeftLocationKeyName, -1);
			int regY = (int)configKey.GetValue(Strings.GMChatWindowTopLocationKeyName, -1);

			if (regX >= 0 && regX < Window.GetSystemMetrics(Window.SM_CXSCREEN) &&
					regY >= 0 && regY < Window.GetSystemMetrics(Window.SM_CYSCREEN))
			{
				location.X = regX;
				location.Y = regY;
			}

			return location;
		}

		private void HandleLocationChanged(object sender, EventArgs e)
		{
			if (this.Left <= 0 || this.Left >= (Window.GetSystemMetrics(Window.SM_CXSCREEN)) ||
					this.Top <= 0 || this.Top >= Window.GetSystemMetrics(Window.SM_CYSCREEN))
				return;

			RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			configKey.SetValue(Strings.GMChatWindowLeftLocationKeyName, this.Left);
			configKey.SetValue(Strings.GMChatWindowTopLocationKeyName, this.Top);
			configKey.Close();
		}
	}
}
