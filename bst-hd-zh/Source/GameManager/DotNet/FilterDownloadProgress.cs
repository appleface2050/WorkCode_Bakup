using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.IO;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{
	public class FilterDownloadProgress : Form
	{
		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int which);

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

		[DllImport("user32.dll", SetLastError=true)]
		public static extern bool SetProcessDPIAware();

		public enum DeviceCap
		{
			LOGPIXELSX = 88,
			LOGPIXELSY = 90,

			VERTRES = 10,
			DESKTOPVERTRES = 117
		}

		private	static float 				sScaleMultiplier		= 1;
		private static bool				sStarted				= false;

		private ProgressBar				mProgressBar;
		private Label					mLblProgress;
		private Button					mButtonApply;
		private Button					mButtonLater;
		private bool					mCallBackStatus;

		public FilterDownloadProgress()
		{
			InitializeComponent();
		}

		public static int RearrangePixel(int pixels)
		{
			return (int)(sScaleMultiplier * pixels);
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			if (GameManager.sGameManager.mFilterDownloader != null)
			{
				GameManager.sGameManager.mFilterDownloader.ExecuteCallBack(mCallBackStatus);
			}
		}

		public void UpdateProgress(string text)
		{
			UIHelper.RunOnUIThread(this, delegate() {
				this.mLblProgress.Text = text;
			});
		}

		public void EnableButtons()
		{
			UIHelper.RunOnUIThread(this, delegate() {
				mButtonLater.Enabled = true;
				mButtonApply.Enabled = true;
				mProgressBar.Visible = false;
			});
		}

		private void ApplyClickHandler(Object sender, EventArgs e)
		{
			mButtonApply.Enabled = false;
			mButtonApply.Enabled = false;
			mProgressBar.Visible = true;

			FilterDownloader.sUpdateLater = false;
			FilterDownloader.sStopBackgroundWorker = false;
		}

		private void LaterClickHandler(Object sender, EventArgs e)
		{
			FilterDownloader.sUpdateLater = true;
			FilterDownloader.sStopBackgroundWorker = false;
			mCallBackStatus = true;

			this.Close();

			GameManager.sGameManager.mFilterDownloader = null;
		}

		private void InitializeComponent()
		{
			if (!Utils.IsOSWinXP())
				SetProcessDPIAware();

			Graphics g = Graphics.FromHwnd(IntPtr.Zero);
			IntPtr desktop = g.GetHdc();
			int Dpi = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSX);

			int logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
			int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
			float screenScalingFactor = (float)logicalScreenHeight / (float)physicalScreenHeight;
			Dpi = (int)(Dpi * screenScalingFactor);
			sScaleMultiplier = Dpi / (96 * 1.0F);

			int width = RearrangePixel(460);
			int height = RearrangePixel(180);

			this.SuspendLayout();
			// 
			// mLblProgress
			// 
			this.mLblProgress = new System.Windows.Forms.Label();
			this.mLblProgress.AutoSize = true;
			this.mLblProgress.Location = new System.Drawing.Point(RearrangePixel(18), RearrangePixel(30));
			this.mLblProgress.Name = "mLblProgress";
			this.mLblProgress.Size = new System.Drawing.Size(width - RearrangePixel(40), RearrangePixel(30));
			this.mLblProgress.TabIndex = 1;
			this.mLblProgress.Text = GameManager.sLocalizedString["INITIALIZING_TEXT"];
			this.mLblProgress.TextAlign = ContentAlignment.BottomLeft;
			this.mLblProgress.Font = new Font(this.mLblProgress.Font.Name, RearrangePixel(16), FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));	
			// 
			// mProgressBar
			//
			this.mProgressBar = new System.Windows.Forms.ProgressBar();
			this.mProgressBar.Location = new System.Drawing.Point(RearrangePixel(18), this.mLblProgress.Bottom + RearrangePixel(3));
			this.mProgressBar.Size = new System.Drawing.Size(width - RearrangePixel(40), RearrangePixel(30));
			this.mProgressBar.Name = "mProgressBar";
		        this.mProgressBar.Style = ProgressBarStyle.Marquee;
			this.mProgressBar.MarqueeAnimationSpeed = 70;
			// 
			// Form1
			//
			this.mButtonLater = new System.Windows.Forms.Button();
			this.mButtonLater.Name = "Later";
			this.mButtonLater.Text = GameManager.sLocalizedString["LATER_BUTTON_TEXT"];
			this.mButtonLater.Size = new System.Drawing.Size(RearrangePixel(80), RearrangePixel(24));
			this.mButtonLater.Location = new System.Drawing.Point((width/2) + RearrangePixel(30), this.mProgressBar.Bottom + RearrangePixel(30));
			this.mButtonLater.Font = new Font(this.mLblProgress.Font.Name, RearrangePixel(14), FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));	
			this.mButtonLater.Enabled = false;
			this.mButtonLater.Click += LaterClickHandler;
			
			this.mButtonApply = new System.Windows.Forms.Button();
			this.mButtonApply.Name = "Apply";
			this.mButtonApply.Text = GameManager.sLocalizedString["APPLY_BUTTON_TEXT"];
			this.mButtonApply.TabIndex = 0;
			this.mButtonApply.Size = new System.Drawing.Size(RearrangePixel(80), RearrangePixel(24));
			this.mButtonApply.Location = new System.Drawing.Point((width/2) - this.mButtonApply.Width - RearrangePixel(30), this.mProgressBar.Bottom + RearrangePixel(30));
			this.mButtonApply.Font = new Font(this.mLblProgress.Font.Name, RearrangePixel(14), FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
			this.mButtonApply.Enabled = false;	
			this.mButtonApply.Click += ApplyClickHandler;
				

			this.ClientSize = new System.Drawing.Size(width, height);
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.Controls.Add(this.mLblProgress);
			this.Controls.Add(this.mProgressBar);
			this.Controls.Add(this.mButtonLater);
			this.Controls.Add(this.mButtonApply);
			this.Name = "BlueStacksFilterUpdater";
			this.Text = GameManager.sLocalizedString["FILTER_UPDATE_TITLE"];
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.ResumeLayout(false);
			this.Icon = Utils.GetApplicationIcon();
			this.PerformLayout();
			this.StartPosition = FormStartPosition.CenterScreen;
		}
	}
}
