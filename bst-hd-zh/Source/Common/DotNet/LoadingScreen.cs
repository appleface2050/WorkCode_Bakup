using System;
using System.IO;
using System.Drawing;
using System.Drawing.Text;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Collections.Generic;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;

namespace BlueStacks.hyperDroid.Common
{
	class LoadingScreen : UserControl
	{

		private Image splashLogoImage;
		private Image whiteLogoImage;
		private Image closeButtonImage;
		private Image fullScreenButtonImage;
		private NewProgressBar progressBar;
		private Label statusText;
		private Label fullScreenButton;
		private Label closeButton;
		private String installDir;
		private String imageDir;
		private Label appNameText = new AppNameText();
		private Form parentForm;

		public delegate void ToggleFullScreen();

		private bool isFullScreen;
		private List<string> mLstDynamicString;

		public LoadingScreen(Point loadingScreenLocation, Size loadingScreenSize, Image appIconImage,
				string appName, string barType, ToggleFullScreen toggleFullScreen)
		{
			Logger.Info("LoadingScreen({0}, {1}, {2})", loadingScreenSize,
				appIconImage, appName);

			parentForm = this.FindForm();
			SetImageDir();
			LoadImages();

			SetStyle(ControlStyles.AllPaintingInWmPaint | ControlStyles.OptimizedDoubleBuffer, true);

			this.Location = loadingScreenLocation;
			this.Size = loadingScreenSize;
			this.BackColor = Color.FromArgb(35, 147, 213);

			if (this.Width == Screen.PrimaryScreen.Bounds.Width &&
					this.Height == Screen.PrimaryScreen.Bounds.Height)
				isFullScreen = true;
			else
				isFullScreen = false;

			int centerX = loadingScreenSize.Width / 2;
			int centerY = loadingScreenSize.Height / 2;
			Logger.Info("centerX: {0}, centerY: {1}", centerX, centerY);

			/* Add app logo. */

			Label appLogo = new Label();
			if (appName != null && appName.Trim() != "")
			{
				Logger.Info("Using app icon");
				appLogo.BackgroundImage = appIconImage;
			}
			else
			{
				Logger.Info("Using splash logo");
				appLogo.BackgroundImage = this.splashLogoImage;
			}
			appLogo.Width = appLogo.BackgroundImage.Width;
			appLogo.Height = appLogo.BackgroundImage.Height;
			appLogo.BackColor = Color.Transparent;

			String path = Common.Strings.RegBasePath;
			RegistryKey key;
			using (key = Registry.LocalMachine.OpenSubKey(path))
			{
				installDir = (String)key.GetValue("InstallDir");
			}
			if (appName == "")
			{
				appName = Oem.Instance.LoadingScreenAppTitle;
				if (appName.StartsWith("DynamicText"))
				{
					mLstDynamicString = new List<string>(appName.Split(new char[] { ';' }, StringSplitOptions.RemoveEmptyEntries));
					if (mLstDynamicString.Count > 0)
					{
						mLstDynamicString.Remove("DynamicText");
						System.Timers.Timer t = new System.Timers.Timer(5000);
						t.Elapsed += new System.Timers.ElapsedEventHandler(TimerElapsed);
						appName = mLstDynamicString[0];
						t.Start();
					}
				}
			}
			else
				appName = "BlueStacks";

			appNameText.Text = appName;
			appNameText.TextAlign = ContentAlignment.MiddleCenter;
			appNameText.Width = loadingScreenSize.Width;
			appNameText.UseMnemonic = false;
			appNameText.ForeColor = Color.White;
			appNameText.BackColor = Color.Transparent;

			statusText = new StatusText();
			statusText.TextAlign = ContentAlignment.MiddleCenter;
			statusText.Width = loadingScreenSize.Width;
			statusText.UseMnemonic = false;
			statusText.ForeColor = Color.White;
			statusText.BackColor = Color.Transparent;

			/* Add progress bar */

			progressBar = new NewProgressBar(barType);
			progressBar.Width = 336;
			progressBar.Height = 10;
			progressBar.Value = 0;

			if (barType == "Marquee")
			{
				Timer timer = new Timer();
				timer.Interval = 50;
				timer.Tick += delegate (Object obj, EventArgs evt)
				{
					progressBar.Invalidate();
				};
				timer.Start();
			}

			/* Add our logo. */

			Label whiteLogo = new Label();
			whiteLogo.BackgroundImage = this.whiteLogoImage;
			whiteLogo.BackgroundImageLayout = ImageLayout.Stretch;
			whiteLogo.Width = 48;
			whiteLogo.Height = 44;
			whiteLogo.BackColor = Color.Transparent;

			/* Add toggle fullscreen button. */

			fullScreenButton = new Label();
			fullScreenButton.BackgroundImage = this.fullScreenButtonImage;
			fullScreenButton.BackgroundImageLayout = ImageLayout.Stretch;
			fullScreenButton.Width = 24;
			fullScreenButton.Height = 24;
			fullScreenButton.BackColor = Color.Transparent;
			fullScreenButton.Click += delegate (Object obj, EventArgs evt)
			{
				if (toggleFullScreen != null)
				{
					toggleFullScreen();
					FullScreenToggled();
				}
			};
			if (toggleFullScreen == null)
			{
				fullScreenButton.Visible = false;
			}

			/* Add close button. */

			closeButton = new Label();
			closeButton.BackgroundImage = this.closeButtonImage;
			closeButton.BackgroundImageLayout = ImageLayout.Stretch;
			closeButton.Width = 24;
			closeButton.Height = 24;
			closeButton.BackColor = Color.Transparent;
			closeButton.Click += delegate (Object obj, EventArgs evt)
			{
				parentForm.Close();
			};
			if (!isFullScreen)
				closeButton.Visible = false;

			/*
			 * Set locations
			 */
			int heightNeeded = appLogo.Height + 30 + appNameText.Height + 50 + progressBar.Height + 20 + statusText.Height;
			int startPos = centerY - heightNeeded / 2;
			appLogo.Location = new Point(centerX - appLogo.Width / 2, startPos);
			appNameText.Location = new Point(0, appLogo.Bottom + 30);
			progressBar.Location = new Point(centerX - progressBar.Width / 2, appNameText.Bottom + 50);
			statusText.Location = new Point(0, progressBar.Bottom + 20);
			whiteLogo.Location = new Point(centerX - whiteLogo.Width / 2, this.Height - whiteLogo.Height - 20);
			closeButton.Location = new Point(this.Width - closeButton.Width - 30, 30);
			fullScreenButton.Location = new Point(closeButton.Left - 10 - fullScreenButton.Width, 30);

			this.Controls.Add(appLogo);
			this.Controls.Add(appNameText);
			this.Controls.Add(progressBar);
			this.Controls.Add(statusText);
			this.Controls.Add(whiteLogo);
			this.Controls.Add(closeButton);
			this.Controls.Add(fullScreenButton);

			if (Common.Oem.Instance.IsWhiteBlueStackLogoToBeHiddenOnLoadingScreen)
			{
				whiteLogo.Visible = false;
			}

		}

		private void TimerElapsed(object sender, System.Timers.ElapsedEventArgs e)
		{
			try
			{
				if (mLstDynamicString != null && mLstDynamicString.Count > 0)
				{
					int index = 0;
					if (mLstDynamicString.Contains(appNameText.Text))
					{
						index = mLstDynamicString.IndexOf(appNameText.Text) + 1;
					}
					if (index == mLstDynamicString.Count)
					{
						index = 0;
					}
					appNameText.Text = mLstDynamicString[index];
				}
			}
			catch (Exception ex)
			{
				Logger.Info(ex.Message);
			}
		}

		public void SetStatusText(string text)
		{
			this.statusText.Text = text;
		}

		private void SetImageDir()
		{
			String path = Common.Strings.RegBasePath;

			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
			if (key == null)
			{
				string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				string setupDir = Path.Combine(programData, @"BlueStacksSetup");
				imageDir = Path.Combine(setupDir, @"Images");
			}
			else
			{
				imageDir = (String)key.GetValue("InstallDir");
			}
		}

		private void LoadImages()
		{
			Logger.Info("imageDir = " + imageDir);
			Image splashLogoImage;
			splashLogoImage = new Bitmap(Path.Combine(imageDir, "ProductLogo.png"));
			this.splashLogoImage = new Bitmap(splashLogoImage, new Size(128, 128));
			this.whiteLogoImage = new Bitmap(Path.Combine(imageDir, "WhiteLogo.png"));
			this.closeButtonImage = new Bitmap(Path.Combine(imageDir, "XButton.png"));
			this.fullScreenButtonImage = new Bitmap(Path.Combine(imageDir, "WhiteFullScreen.png"));
		}

		private void FullScreenToggled()
		{
			isFullScreen = !isFullScreen;
			closeButton.Visible = !closeButton.Visible;
		}

		public void UpdateProgressBar(int val)
		{
			this.progressBar.Value = val;
		}

		public class NewProgressBar : ProgressBar
		{
			private enum BarType
			{
				Progress,
				Marquee
			}
			private BarType barType;
			private SolidBrush baseBrush;
			private SolidBrush backBrush;
			private SolidBrush foreBrush;

			private int marqueeStart = 0;

			public NewProgressBar(string type)
			{
				this.SetStyle(ControlStyles.UserPaint, true);
				this.SetStyle(ControlStyles.DoubleBuffer, true);
				this.SetStyle(ControlStyles.AllPaintingInWmPaint, true);
				barType = (BarType)Enum.Parse(typeof(BarType), type, true);

				Color baseColor = Color.FromArgb(35, 147, 213);
				baseBrush = new SolidBrush(baseColor);
				Color backColor = Color.FromArgb(195, 195, 193);
				backBrush = new SolidBrush(backColor);
				Color foreColor = Color.FromArgb(21, 83, 120);
				foreBrush = new SolidBrush(foreColor);
			}

			protected override void OnPaint(PaintEventArgs e)
			{
				Rectangle rec = e.ClipRectangle;

				FillRectangle(e, baseBrush, 0, 0, rec.Width, 1);
				FillRectangle(e, backBrush, 0, 1, rec.Width, rec.Height - 2);
				FillRectangle(e, baseBrush, 0, rec.Height - 1, rec.Width, 1);

				switch (barType)
				{
					case BarType.Progress:
						int progressWidth = (int)(rec.Width * ((double)Value / Maximum));
						FillRectangle(e, foreBrush, 0, 0, progressWidth, rec.Height);

						break;

					case BarType.Marquee:
						FillRectangle(e, foreBrush, marqueeStart, 0, 96, rec.Height);

						marqueeStart += 10;
						if (marqueeStart >= rec.Width - 20)
						{
							marqueeStart = 0;
						}

						break;
				}
			}

			private void FillRectangle(PaintEventArgs e, Brush brush, int x, int y, int width, int height)
			{
				//		Logger.Info("Creating rect ({0}, {1}, {2}, {3})", x, y, width, height);
				e.Graphics.FillRectangle(brush, x, y, width, height);
			}
		}

		public class AppNameText : Label
		{
			public AppNameText()
			{
				this.Font = new Font(Common.Utils.GetSystemFontName(), 18, FontStyle.Bold);
				this.Height = 36;
			}

			protected override void OnPaint(PaintEventArgs evt)
			{
				evt.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				base.OnPaint(evt);
			}
		}

		public class StatusText : Label
		{
			public StatusText()
			{
				this.Font = new Font(Common.Utils.GetSystemFontName(), 12, FontStyle.Regular);
				this.Height = 24;
			}

			protected override void OnPaint(PaintEventArgs evt)
			{
				evt.Graphics.TextRenderingHint = TextRenderingHint.AntiAlias;
				base.OnPaint(evt);
			}
		}

	}
}
