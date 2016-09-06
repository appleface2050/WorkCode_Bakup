using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Text.RegularExpressions;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for TabButton.xaml
	/// </summary>
	public partial class TabButton : UserControl
	{
		public string mKey = string.Empty;
		public string mPackageName = string.Empty;
		public string mActivity = string.Empty;
		private bool mIsTabClosable = true;
		public bool IsTabClosable
		{
			get
			{
				return mIsTabClosable;
			}
			set
			{
				mIsTabClosable = value;
				if (mIsTabClosable)
				{
					CloseImageColumn.Width = new GridLength(18, GridUnitType.Pixel);
					mCloseButton.Visibility = Visibility.Visible;
				}
				else
				{
					CloseImageColumn.Width = new GridLength(0, GridUnitType.Pixel);
					mCloseButton.Visibility = Visibility.Hidden;
				}
			}
		}
		BitmapImage SelectedAppIcon = null;
		BitmapImage UnselectedAppIcon = null;
		private BitmapImage AppIconImage
		{
			set
			{
				if (value == null)
				{
					IconImageColumn.Width = new GridLength(0, GridUnitType.Pixel);
					mAppIcon.Source = null;
				}
				else
				{
					mAppIcon.Source = value;
					SelectedAppIcon = value;
					IconImageColumn.Width = new GridLength(30, GridUnitType.Pixel);
				}
			}
		}


		public bool mIsHomeTab = false;
		private bool isSelected = false;
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				UpdateTabColor();
			}
		}
		public string mWebUrl;
		public bool mLaunchApp = true;
		public Downloader mDownloader = null;

		public bool mIsTabClosing = false;
		private Browser _browser = null;

		public Browser mBrowser
		{
			get
			{
				return _browser;
			}
			set
			{
				_browser = value;
			}
		}

		internal Grid ControlGrid = null;

		public Image mScreenshot;

		public bool mHasLock = false;

		private EnumTabType? mTabType = null;
		public EnumTabType TabType
		{
			get
			{
				if (!mTabType.HasValue)
				{
					if (_browser == null)
					{
						return EnumTabType.app;
					}
					else
					{
						return EnumTabType.web;
					}
				}
				return mTabType.Value;
			}
		}


		public TabButton()
		{
			InitializeComponent();
			AppIconImage = null;
		}

		public void Initialize(string appName, string url, bool isColsable, string imagePath)
		{
			Initialize(appName, url, isColsable, imagePath, null);
		}

		public void Initialize(string appName, string url, bool isColsable, string imagePath, string package)
		{
			mPackageName = appName;
			mWebUrl = url;
			mAppName.Text = mPackageName;
			if (imagePath != null)
			{
				AppIconImage = CustomPictureBox.GetBitmapImage(imagePath);
				UnselectedAppIcon = CustomPictureBox.GetBitmapImage(imagePath.ToLower().Replace(".png", "_unselected.png"));
			}
			IsTabClosable = isColsable;
			this.ToolTip = appName;
			if (package != null)
				this.mKey = package;
			else
				this.mKey = appName;
		}

		internal void Initialize(string appName, string packageName, string activityName, string imagePath, bool launchApp)
		{
			Logger.Info("Creating app tab for: name: {0}, package: {1}, activity: {2}", appName, packageName, activityName);
			mAppName.Text = appName.Replace('\n', ' ');
			mPackageName = packageName;
			mActivity = activityName;
			if (imagePath != null)
			{
				AppIconImage = CustomPictureBox.GetBitmapImage(imagePath);
				UnselectedAppIcon = CustomPictureBox.GetBitmapImage(imagePath.ToLower().Replace(".png", "_unselected.png"));
			}
			mLaunchApp = launchApp;
			this.ToolTip = appName.Replace('\n', ' ');
			this.mKey = mPackageName;
		}

		private void Grid_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (!isSelected)
			{
				firstGradientColor.Color = (Color)ColorConverter.ConvertFromString("#6C697F");
				SecondGradientColor.Color = (Color)ColorConverter.ConvertFromString("#464459");
				mAppName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(("#9c9da9")));
			}
		}

		private void Grid_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			UpdateTabColor();
		}
		private void UpdateTabColor()
		{
			if (isSelected)
			{
				firstGradientColor.Color = (Color)ColorConverter.ConvertFromString("#8A87A1");
				SecondGradientColor.Color = (Color)ColorConverter.ConvertFromString("#65627D");
				mAppName.Foreground = Brushes.White;
				if (SelectedAppIcon != null)
				{
					mAppIcon.Source = SelectedAppIcon;
				}
			}
			else
			{
				firstGradientColor.Color = (Color)ColorConverter.ConvertFromString("#3E3D46");
				SecondGradientColor.Color = (Color)ColorConverter.ConvertFromString("#292832");
				mAppName.Foreground = new SolidColorBrush((Color)ColorConverter.ConvertFromString(("#9c9da9")));
				if (UnselectedAppIcon != null)
				{
					mAppIcon.Source = UnselectedAppIcon;
				}
			}
		}
		private void Grid_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (TabButtons.Instance.TabGrid.Children.Contains(this))
			{
				if (TabButtons.Instance.SelectedTab != this)
				{
					TabButtons.Instance.SelectedTab = this;
					e.Handled = true;
				}
			}
		}
		private void Grid_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (e.ChangedButton == MouseButton.Middle)
			{
				Close();
			}
		}


		internal void Close()
		{
			if (IsTabClosable)
			{
				if (mBrowser != null && mBrowser.webClient != null)
				{
					mBrowser.webClient.CancelAsync();
				}
				if (TabType == EnumTabType.web && mAppName != null)
				{
					StreamViewTimeStats.HandleStreamViewStatsEvent(mAppName.Text, StreamViewStatsEventName.TabCloseSessionEnd);
				}
				if (mBrowser != null)
				{
					mBrowser.Dispose();
				}
				if (mIsDownloading)
				{
					mIsTabClosing = true;
					AbortApkDownload();
				}

				if (TabButtons.Instance.mLastAppTabName == mPackageName)
					TabButtons.Instance.mLastAppTabName = "";

				if (TabType == EnumTabType.app && TabButtons.Instance.mRotateInProgress == false)
				{
					if (Oem.Instance.IsAppToBeForceKilledOnTabClose)
					{
						Thread thread = new Thread(delegate ()
						{

							GMHTTPHandler.StopAppRequest(mPackageName);

						});
						thread.IsBackground = true;
						thread.Start();
					}
				}
				else if (TabButtons.Instance.mRotateInProgress == true)
				{
					TabButtons.Instance.mRotateInProgress = false;
				}
				if (mHasLock)
				{
					Logger.Info("Setting event");
					GameManagerUtilities.sAppInstallEvent.Set();
				}

				//If Tab package is Play Store then Kill RPC TroubleShooter process
				if (this != null && (this.mPackageName == AppHandler.STOREPACKAGE))
				{
					Utils.KillProcessByName("HD-RPCErrorTroubleShooter");
				}
				TabButtons.Instance.RemoveTab(this);
			}
		}
		internal void UpdateTabText(string s)
		{
			mAppName.Text = s;
			this.ToolTip = s;
		}

		public void PerformTabAction(bool launchApp, bool showFrontend)
		{
			Logger.Info("PerformTabAction: name: {0}, activity: {1}", this.mKey, mActivity);
			Logger.Info("mRunAppRequestPending: {0}, launchApp: {1}, showFrontend: {2}",
					mRunAppRequestPending, launchApp, showFrontend);

			if (launchApp)
			{
				if (mActivity == "S2P")
				{
					LaunchS2PSetup(AppHandler.sPackageToInstall, AppHandler.sAppToInstall);
				}
				else if (!mPackageName.Equals(AppHandler.GAMEPOPHOME) && ToolBar.Instance.IsOneTimeSetupComplete())
				{
					GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
					{ SendRunAppRequestAsync(mPackageName, mActivity); }));
				}
			}
			else
			{
				mRunAppRequestPending = false;
			}

			if (showFrontend)
			{
				ContentControl.Instance.BringFrontendInFront();
				ContentControl.Instance.HideWaitControl();
			}
			else
			{
				ContentControl.Instance.ShowWaitControl();
			}
			Logger.Info("mAppDisplayed: " + TabButtons.Instance.mAppDisplayed);
		}

		public void LaunchS2PSetup(string package, string title)
		{
			Logger.Info("In LaunchS2PSetup");

			Thread thread = new Thread(delegate ()
			{
				mRunAppRequestPending = true;

				GMUtils.LaunchS2PSetup(package, title);
				mRunAppRequestPending = false;
			});

			thread.IsBackground = true;
			thread.Start();
		}



		#region SendRunAppRequest
		private static Object sRequestObject = new Object();
		private static bool sRequestQueued = false;
		public bool mRunAppRequestPending = true;
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
		private void SendRunAppRequestAsync(string package, string activity)
		{
			Logger.Info("SendRunAppRequest: " + package + "/" + activity);
			Logger.Info("mRunAppRequestPending: " + mRunAppRequestPending);
			mRunAppRequestPending = true;

			TabButton tab = TabButtons.Instance.SelectedTab;
			if (this.mKey == tab.mKey)
			{
				Thread thread = new Thread(delegate ()
				{
					if (SendRunAppRequestSync(package, activity) == false)
					{
						GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
						{
							tab = TabButtons.Instance.SelectedTab;
							if (this.mKey == tab.mKey)
								TabButtons.Instance.GoToHomeTab();
						}));
					}
					mRunAppRequestPending = false;
					//Logger.Info("mRunAppRequestPending: " + mRunAppRequestPending);

				});
				thread.IsBackground = true;
				thread.Start();
			}
		}



		private bool SendRequest(String url, String arg)
		{
			Logger.Info("Will send {0} to {1}", arg, url);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("arg", arg);

			TimeSpan waitTime = new TimeSpan(0, 0, 1);

			int retries = 60;
			int printErrorLog = 3;  // print 3 times and then shut up
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
		#endregion

		#region DownloadApk
		private Thread mDownloadThread;
		public bool mIsDownloading = false;
		internal ColumnDefinition ColumnDefinition;

		public void DownloadApkForPackage(string apkUrl, string pkg, bool install, bool launchAfterInstall)
		{
			mPackageName = pkg;
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
			string originalPath = System.IO.Path.Combine(GameManagerUtilities.SetupDir, appName);
			string apkFilePath = Regex.Replace(originalPath, @"[\x22\\\/:*?|<>]", " ");

			Logger.Info("Downloading Apk file to: " + apkFilePath);

			mDownloadThread = new Thread(delegate ()
			{
				mIsDownloading = true;

				mDownloader = new Downloader(3, apkUrl, apkFilePath);
				mDownloader.Download(
					delegate (int percent)
					{
						if (mBrowser != null)
						{
							mBrowser.SetFileDownloadProgress(percent.ToString());
						}

					},
					delegate (string filePath)
					{
						mIsDownloading = false;

						if (install)
						{
							InstallApk(appName, filePath, launchAfterInstall);
						}
					},
					delegate (Exception ex)
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

		#endregion

		private void InstallApk(string appName, string apkPath, bool actionAfterInstall)
		{
			Logger.Info("Installing apk: {0}", apkPath);

			bool eventSet = GameManagerUtilities.sAppInstallEvent.WaitOne(0, false);
			Logger.Info("eventSet: " + eventSet);
			if (!eventSet)
			{
				Logger.Info("Waiting for event");
				GameManagerUtilities.sAppInstallEvent.WaitOne();
			}
			mHasLock = true;

			if (mBrowser != null)
			{
				string statusText = String.Format("Installing {0}...", appName);
				mBrowser.SetProgressBarText(statusText);
				mBrowser.SetProgressBarStyle("marquee");
			}

			Thread apkInstall = new Thread(delegate ()
			{
				RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
				string installDir = (string)reg.GetValue("InstallDir");

				ProcessStartInfo psi = new ProcessStartInfo();
				psi.FileName = System.IO.Path.Combine(installDir, "HD-ApkHandler.exe");
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

					this.mTabType = EnumTabType.app;
				}
				else
				{

					String reason = "Error code: " + silentApkInstaller.ExitCode + ", " +
						((Common.InstallerCodes)silentApkInstaller.ExitCode).ToString();

					GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
					{
						TabButtons.Instance.CloseTab(this.mKey);
						TabButtons.Instance.AddErrorTab(mAppName.Text, reason);
					}));
					return;
				}

				if (actionAfterInstall)
				{
					GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
					{
						TabButton currentTab = TabButtons.Instance.SelectedTab;
						if (currentTab.mKey == this.mKey)
						{
							PerformTabAction(true, false);

						}
					}));
				}

				Logger.Info("Setting event");
				GameManagerUtilities.sAppInstallEvent.Set();
				mHasLock = false;

				GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
				{
					GameManagerWindow.Instance.BlinkTaskbarIcon();
				}));
			});

			apkInstall.IsBackground = true;
			apkInstall.Start();
		}
		private void TabButton_SizeChanged(object sender, SizeChangedEventArgs e)
		{
			if (LabelColumn.ActualWidth >= 15)
			{
				mAppName.MaxWidth = LabelColumn.ActualWidth - 15;
			}
		}

		private void CloseButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			Close();
			e.Handled = true;
		}

		private void CloseButton_MouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
			mCloseButton.SetClickedImage();
		}
	}

	public enum EnumTabType
	{
		web,
		app
	}
}

