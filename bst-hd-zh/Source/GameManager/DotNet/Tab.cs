using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Text;
using System.Threading;
using System.Timers;
using System.Windows.Forms;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using System.Drawing.Drawing2D;

using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.GameManager
{
	public class Tab : TabPage
	{
		public String mLabel;
		public String mPackage;
		public String mActivity;
		public String mTabType;
		public String mWebUrl;
		public Browser mBrowser;
		public Downloader mDownloader = null;

		public bool mIsHome = false;

		public static ProgressBarControl sProgressBarControl;
		public static bool sIsProgressBarControl = false;

		public static string	sWaitBrowserName	= "WaitBrowser";

		public PictureBox mScreenshot;

		private static Object sRequestObject = new Object();
		private static bool sRequestQueued = false;

		public bool mHasLock = false;

		public bool mIsDownloading = false;
		public bool mIsTabClosing = false;
		private Thread mDownloadThread;

		public bool mRunAppRequestPending = true;
		public IntPtr mFrontendHandle = IntPtr.Zero;

		public bool mLaunchApp = true;

		private int mDevEnv = 0;

		private int mTabBorderWidth = 0;

        public Nullable<Size> DeselectedGMClientSize { get; set; }

		public Tab()
		{
			string parentStyleTheme = "Em";
			using (RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
			{
				parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", parentStyleTheme);
			}
			if (parentStyleTheme != "Em")
			{
				this.Paint += new PaintEventHandler(HandlePaintEvent);
				mTabBorderWidth = 1;
			}

			this.BackColor = GMColors.TabBackColor;
			this.GotFocus += new System.EventHandler(this.FocusHandler);

			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
			mDevEnv = (int)prodKey.GetValue("DevEnv", 0);
			this.Resize += Tab_Resize;
		}

		private void Tab_Resize(object sender, EventArgs e)
		{
			if (this.mTabType == "app")
			{
				TabBar.sTabBar.ResizeFrontend();
			}
			this.RefreshWaitControl();
		}

		public Tab(String tabName, String tabUrl) : this()
		{
			Logger.Info("Creating web tab for: " + tabName);

			mLabel = tabName;
			mWebUrl = tabUrl;
			mTabType = "web";
			this.Name = String.Format("web:{0}", mLabel);
			this.ToolTipText = tabName;
		}

		public Tab(String appName, String packageName, String activityName, String apkFileUrl) : this()
		{
			Logger.Info("Creating app tab for: name: {0}, package: {1}, activity: {2}", appName, packageName, activityName);

			mLabel = appName;
			mPackage = packageName;
			mActivity = activityName;
			string mApkUrl = apkFileUrl;
			mTabType = "app";
			this.Name = String.Format("app:{0}", mPackage);
			this.ToolTipText = appName;
		}

		public Browser GetBrowser()
		{
			return mBrowser;
		}

		private void UpdateTabName()
		{
			if (mTabType == "app")
				this.Name = String.Format("app:{0}", mPackage);
			else if (mTabType == "web")
				this.Name = String.Format("web:{0}", mLabel);
		}

		public void UpdateTabText(string text)
		{
			this.Text = text;
			this.ToolTipText = text;
		}

		public Size CalculateContentArea()
		{
			Size tabSize = new Size(GameManager.sGameManager.Width - 2 * GameManager.mBorderWidth,
					GameManager.sGameManager.Height - 2 * GameManager.mBorderWidth -
					GameManager.sTabBarHeight - GameManager.mCenterBorderHeight -
					GameManager.mTabBarExtraHeight);

			Size contentArea = new Size(
					tabSize.Width - 2 * GameManager.mContentBorderWidth - mTabBorderWidth,
					tabSize.Height - 2 * GameManager.mContentBorderWidth - mTabBorderWidth);

			return contentArea;
		}

		private void HandlePaintEvent(Object obj, PaintEventArgs e)
		{
			int width = this.Width - 1;	// XXX: IDK why -1?
			int height = this.Height;
			Pen borderPen = new Pen(GMColors.InnerBorderColor, 1);
			e.Graphics.DrawRectangle(borderPen,
					new Rectangle(GameManager.mContentBorderWidth,
						GameManager.mContentBorderWidth,
						width - 2 * GameManager.mContentBorderWidth + mTabBorderWidth,
						height - 2 * GameManager.mContentBorderWidth));
		}

		private void FocusHandler(object sender, EventArgs e)
		{
			TabBar.sTabBar.Focus();
		}

		public void PerformTabAction(bool launchApp, bool showFrontend)
		{
			Logger.Info("PerformTabAction: name: {0}, activity: {1}", this.Name, mActivity);
			Logger.Info("mRunAppRequestPending: {0}, launchApp: {1}, showFrontend: {2}",
					mRunAppRequestPending, launchApp, showFrontend);

			if (launchApp)
			{
				if (mActivity == "S2P")
				{
					LaunchS2PSetup(GameManager.sPackageToInstall, GameManager.sAppToInstall);
				}
				else
				{
					UIHelper.RunOnUIThread(this, delegate () { SendRunAppRequestAsync(mPackage, mActivity); });
				}
			}
			else
			{
				mRunAppRequestPending = false;
			}

			ShowFrontend(showFrontend);
			Logger.Info("mAppDisplayed: " + TabBar.sTabBar.mAppDisplayed);
		}

		private void ShowFrontend(bool showFrontend)
		{
			UIHelper.RunOnUIThread(this, delegate ()
			{
				if (showFrontend)
				{
					Logger.Info("{0} ShowFrontend", this.Name);

					TabBar.sTabBar.mAppDisplayed = true;

					if (mFrontendHandle == IntPtr.Zero)
					{
						mFrontendHandle = TabBar.sTabBar.SetParentFrontend(this.Handle, true,"Bluestacks Android Plugin", "HD-Frontend");

						/*
						 * For some reason, touch does not work the first time time frontend is reparented
						 * Reparenting it again fixes it
						 * Let this code stay till we can figure out why
						 */
						mFrontendHandle = TabBar.sTabBar.SetParentFrontend(this.Handle, true, "Bluestacks Android Plugin", "HD-Frontend");
						mFrontendHandle = TabBar.sTabBar.SetParentFrontend(this.Handle, true, "Form1", "HD-Frontend");
					}

					if (mScreenshot != null)
					{
						mScreenshot.Hide();
					}

					if (sIsProgressBarControl && sProgressBarControl != null)
					{
						ShowWaitControl(false);
					}

					TabBar.sTabBar.ResizeFrontend();
					GameManager.sGameManager.mControlBarRight.HideLoading();
				}
				else

				{
					Logger.Info("{0} HideFrontend", this.Name);
					GameManager.sGameManager.mControlBarRight.ShowLoading();

					//			Logger.Info("mScreenshot = " + mScreenshot);
					if (mScreenshot == null)
					{
						ShowWaitControl(true);
					}
					TabBar.sTabBar.mAppDisplayed = false;
				}
			});
		}

		public void ShareScreenshot()
		{
			Random	random			= new Random();
			int	randomNum		= random.Next(0, 100000);

			string	fileBaseName		= String.Format(@"bstSnapshot_{0}.jpg", randomNum);
			string	finalBaseName		= String.Format(@"final_{0}", fileBaseName);

			string sharedFolder = Common.Strings.SharedFolderDir;
			string sharedFolderName = Common.Strings.SharedFolderName;

			string	origFileName		= Path.Combine(sharedFolder, fileBaseName);
			string	modifiedFileName	= Path.Combine(sharedFolder, finalBaseName);

			Bitmap bitmap = GetScreenshot();
			bitmap.Save(origFileName, ImageFormat.Jpeg);

			try
			{
				Utils.AddUploadTextToImage(origFileName, modifiedFileName);
				File.Delete(origFileName);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to add upload text to snapshot. err: " + ex.ToString());
				finalBaseName = fileBaseName;
				modifiedFileName = origFileName;
			}

			string	url	= String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort, Common.Strings.SharePicUrl);

			string androidPath = "/mnt/sdcard/windows/" +
				sharedFolderName + "/" + Path.GetFileName(finalBaseName);
			Logger.Info("share screenshot: androidPath: " + androidPath);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("data", androidPath);

			Logger.Info("Sending snapshot upload request.");

			string result = "";
			try
			{
				result = Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
				Logger.Error("Post failed. url = {0}, data = {1}", url, data);
			}
			return;
		}

		public void TakeScreenshot()
		{
			Logger.Info("Taking screenshot for: " + this.Name);
			Bitmap bitmap = GetScreenshot();

			if (mDevEnv == 1)
			{
				bitmap.Save(mPackage + ".jpg", ImageFormat.Jpeg);
			}

			if (mScreenshot == null)
			{
				mScreenshot = new PictureBox();
				mScreenshot.Dock = DockStyle.Fill;
				mScreenshot.SizeMode = PictureBoxSizeMode.StretchImage;

			}
			else
			{
				mScreenshot.Image.Dispose();
			}
			mScreenshot.Image = bitmap;
			mScreenshot.Visible = false;

			try
			{
				if (!this.Controls.Contains(mScreenshot))
					this.Controls.Add(mScreenshot);
			}
			catch (Exception e)
			{
				Logger.Error("Error in TakeScreenshot");
				Logger.Error(e.ToString());

				if (mScreenshot.Image != null)
				{
					mScreenshot.Image.Dispose();
				}

				mScreenshot.Dispose();
				mScreenshot = null;
			}
		}

		public Bitmap GetScreenshot()
		{
			Rectangle bounds = this.Bounds;

			Bitmap bitmap;
			try
			{
				bitmap = new Bitmap(this.Width, this.Height);
			}
			catch (Exception ex)
			{
				Logger.Error("Error while creating bitmap. width = " + this.Width + " and height = " + this.Height);
				Logger.Info("recalculating tab page size");
				Size contentArea = this.CalculateContentArea();
				bitmap = new Bitmap(contentArea.Width, contentArea.Height);
			}
			using (Graphics g = Graphics.FromImage(bitmap))
			{
				Point startPoint = this.Parent.PointToScreen(new Point(this.Left, this.Top));
				g.CopyFromScreen(startPoint, Point.Empty, new Size(this.Width, this.Height));
			}
			return bitmap;
		}

		public void RefreshWaitControl()
		{
			if (this.InvokeRequired)
			{
				SendOrPostCallback cb = new SendOrPostCallback(delegate(Object obj)
						{
							RefreshWaitControl();
						});
				this.Invoke(cb, null);
			}
			else
			{
				if (sProgressBarControl != null && sProgressBarControl.IsVisible == true)
				{
					ShowWaitControl(true);
				}
			}
		}

		internal static void ShowWaitControl(bool isVisible)
		{
			try
			{
				if (sProgressBarControl != null)
				{
					if (sProgressBarControl.IsHandleCreated && !sProgressBarControl.Disposing)
					{
						sProgressBarControl.Dispose();
					}
					if(!sProgressBarControl.IsDisposed)
					{
						sProgressBarControl.Dispose();
					}
					sProgressBarControl = null;
				}
				if (isVisible)
				{
					if (TabBar.sTabBar.SelectedTab.InvokeRequired)
					{
						TabBar.sTabBar.SelectedTab.Invoke((MethodInvoker)delegate
						{
							CreateWaitControl();
							TabBar.sTabBar.SelectedTab.Controls.Add(sProgressBarControl);
						});
					}
					else
					{
						CreateWaitControl();
						TabBar.sTabBar.SelectedTab.Controls.Add(sProgressBarControl);
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex.Message);
			}
		}

		private static void CreateWaitControl()
		{
			try
			{
				Logger.Info("Setting Up ProgressBar Control");
				sIsProgressBarControl = true;
				if (sProgressBarControl == null)
				{
					sProgressBarControl = new ProgressBarControl();
					sProgressBarControl.Visible = true;
					Tab currentTab = (Tab)TabBar.sTabBar.SelectedTab;
					sProgressBarControl.Size = new System.Drawing.Size((int)(currentTab.Width * .6), (int)(currentTab.Height * .3));
					sProgressBarControl.Anchor = AnchorStyles.None;
					sProgressBarControl.Location = new System.Drawing.Point(
							(currentTab.Width - sProgressBarControl.Width) / 2,
							(currentTab.Height - sProgressBarControl.Height) / 2);
				}
			}
			catch (Exception ex)
			{
				Logger.Info(ex.Message);
			}
		}

		public void LaunchS2PSetup(string package, string title)
		{
			Logger.Info("In LaunchS2PSetup");

			Thread thread = new Thread(delegate()
					{
					mRunAppRequestPending = true;

					GMUtils.LaunchS2PSetup(package, title);
					mRunAppRequestPending = false;
					});

			thread.IsBackground = true;
			thread.Start();
		}

		private void SendRunAppRequestAsync(string package, string activity)
		{
			Logger.Info("SendRunAppRequest: " + package + "/" + activity);
			Logger.Info("mRunAppRequestPending: " + mRunAppRequestPending);
			mRunAppRequestPending = true;

			Tab currentTab = (Tab)TabBar.sTabBar.SelectedTab;
			if (this.Name == currentTab.Name)
			{
				Thread thread = new Thread(delegate ()
				{
					if (SendRunAppRequestSync(package, activity) == false)
					{
						UIHelper.RunOnUIThread(this, delegate ()
						 {
							 currentTab = (Tab)TabBar.sTabBar.SelectedTab;
							 if (this.Name == currentTab.Name)
								 GameManager.sGameManager.GoToHomeTab();
						 });
					}
					mRunAppRequestPending = false;
					//Logger.Info("mRunAppRequestPending: " + mRunAppRequestPending);

				});
				thread.IsBackground = true;
				thread.Start();
			}
		}

		private bool SendRunAppRequestSync(string package, string activity)
		{
			string path = "runex";
			string url = String.Format("http://127.0.0.1:{0}/{1}", VmCmdHandler.s_ServerPort, path);
			string arg = String.Format("{0}/{1}", package, activity);
			sRequestQueued = true;
			lock (sRequestObject)
			{
				sRequestQueued = false;
				return SendRequest(url, arg);
			}
		}

		private bool SendRequest(String url, String arg)
		{
			Logger.Info("Will send {0} to {1}", arg, url);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("arg", arg);

			TimeSpan waitTime = new TimeSpan(0, 0, 1);

			int retries = 60;
			int printErrorLog = 3;	// print 3 times and then shut up
			while (retries > 0)
			{
				try
				{

					if (printErrorLog != 0)
					{
						printErrorLog--;
						Logger.Info("Sending request to " + url);
					}

					if (sRequestQueued)
						return true;

					/*
					 * force a timeout of 3 second when posting to runex
					 * and a timeout of 1 second when posting to setwindowsagentaddr
					 */
					string r = Common.HTTP.Client.Post(url, data, null, false, 10000);
					Logger.Info("Got response for {0}: {1}", url, r);

					JSonReader readjson = new JSonReader();
					IJSonObject jsonObj = readjson.ReadAsJSonObject(r);
					string result = jsonObj["result"].StringValue.Trim();
					if (result == "error")
					{
						string reason = jsonObj["reason"].StringValue.Trim();
						if (reason == "system not ready")
							return false;
					}

					return true;
				}
				catch (Exception e)
				{
					if (printErrorLog != 0)
						Logger.Error("Exception for {0}: {1}", url, e.Message);
				}

				Thread.Sleep(waitTime);
				retries--;
			}

			return false;
		}

		public void DownloadApkForPackage(string apkUrl, string pkg, bool install, bool launchAfterInstall)
		{
			mPackage = pkg;
			DownloadApk(apkUrl, pkg + ".apk", install, launchAfterInstall);
			Logger.Info("apkUrl = " + apkUrl + "; pkg = " + pkg);
		}

		public void DownloadApk(string apkUrl, string appName)
		{
			DownloadApk(apkUrl, appName, true, true);
			Logger.Info("apkUrl = " + apkUrl + "; appName = " + appName);
		}

		public void DownloadApk(string apkUrl, string appName, bool install, bool launchAfterInstall)
		{
			if (apkUrl == null)
			{
				return;
			}

			string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string originalPath = Path.Combine(GameManager.sSetupDir, appName);
			string apkFilePath = Regex.Replace(originalPath, @"[\x22\\\/:*?|<>]", " ");

			Logger.Info("Downloading Apk file to: " + apkFilePath);

			mDownloadThread = new Thread(delegate()
					{
					mIsDownloading = true;

					mDownloader = new Downloader(3, apkUrl, apkFilePath);
					mDownloader.Download(
						delegate(int percent)
						{
							if (mBrowser != null) {
								mBrowser.SetFileDownloadProgress(percent.ToString());
							}

						},
						delegate(string filePath)
						{
							mIsDownloading = false;

							if (install)
							{
								InstallApk(appName, filePath, launchAfterInstall);
							}
						},
						delegate(Exception ex)
						{
							Logger.Error("Failed to download file: {0}. err: {1}", apkFilePath, ex.Message);
							GMApi.setInstallStatus(GMApi.InstallStatus.FAILED, String.Format("Failed to download file: {0}. err: {1}", apkFilePath, ex.Message));
							if (mIsTabClosing == false)
							{
								DownloadApk(apkUrl, appName, install, launchAfterInstall);
							}
							else
							{
								Logger.Info("Not retrying apk download as the tab is closing");
							}
						});
					});

			mDownloadThread.IsBackground = true;
			mDownloadThread.Start();
		}

		public void AbortApkDownload()
		{
			Logger.Info("Tab closed. Aborting download");
			mDownloader.AbortDownload();
			if (mDownloadThread != null)
				mDownloadThread.Abort();
		}

		private void InstallApk(string appName, string apkPath, bool actionAfterInstall)
		{
			Logger.Info("Installing apk: {0}", apkPath);

			bool eventSet = GameManager.sAppInstallEvent.WaitOne(0, false);
			Logger.Info("eventSet: " + eventSet);
			if (!eventSet)
			{
				Logger.Info("Waiting for event");
				GameManager.sAppInstallEvent.WaitOne();
			}
			mHasLock = true;

			if (mBrowser != null)
			{
				string statusText = String.Format("Installing {0}...", appName);
				mBrowser.SetProgressBarText(statusText);
				mBrowser.SetProgressBarStyle("marquee");
			}

			Thread apkInstall = new Thread(delegate()
					{
					RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
					string installDir = (string)reg.GetValue("InstallDir");

					ProcessStartInfo psi = new ProcessStartInfo();
					psi.FileName = Path.Combine(installDir, "HD-ApkHandler.exe");
					psi.Arguments = String.Format("\"{0}\" silent", apkPath);
					psi.UseShellExecute = false;
					psi.CreateNoWindow = true;

					Logger.Info("Installer path {0}", psi.FileName);

					Process silentApkInstaller = Process.Start(psi);

					silentApkInstaller.WaitForExit();
					Logger.Info("Apk installer exit code: {0}", silentApkInstaller.ExitCode);

					if (silentApkInstaller.ExitCode == 0)
					{
						Logger.Info("Installation successful.");
						File.Delete(apkPath);

						this.mTabType = "app";
						UIHelper.RunOnUIThread(TabBar.sTabBar, delegate() {

							if (this.mBrowser != null)
							{
								this.mBrowser.Hide();
								this.Controls.Remove(this.mBrowser);
								this.mBrowser.Dispose();
								this.mBrowser = null;
							}

							this.UpdateTabName();
						});
					}
					else
					{
						String reason = "Error code: " + silentApkInstaller.ExitCode + ", " +
							((Common.InstallerCodes)silentApkInstaller.ExitCode).ToString();

						UIHelper.RunOnUIThread(TabBar.sTabBar, delegate() {
								TabBar.sTabBar.CloseTab(this.Name);
								TabBar.sTabBar.AddErrorTab(mLabel, reason);
								});
						return;
					}

					if (actionAfterInstall)
					{
						UIHelper.RunOnUIThread(this, delegate() {
								Tab currentTab = TabBar.sTabBar.GetCurrentTab();
								if (currentTab.Name == this.Name)
								{
									PerformTabAction(true, false);
                                    //fix window size
                                    if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
                                    {
                                        Size appSize = GameManager.GetFEAppWindowSize(GameManager.sGameManager.mFrontendHandle);
                                        if (appSize.Width > 0 && appSize.Height > 0 && appSize.Width < appSize.Height)
                                        {
                                            Size s = GameManager.sGameManager.GetFESizeHorizontalBySreen();
                                            GameManager.sGameManager.ClientSize = GameManager.sGameManager.GetGMSizeGivenFESize(s.Width, s.Height);
                                            GameManager.sGameManager.Refresh();
                                        }
                                    }
								}
								});
					}


					Logger.Info("Setting event");
					GameManager.sAppInstallEvent.Set();
					mHasLock = false;

					GameManager.sGameManager.BlinkTaskbarIcon();
					});

			apkInstall.IsBackground = true;
			apkInstall.Start();
		}
	}
}
