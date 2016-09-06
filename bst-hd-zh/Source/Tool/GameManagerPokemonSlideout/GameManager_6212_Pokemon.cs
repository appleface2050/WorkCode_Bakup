using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Management;
using System.Diagnostics;
using System.Drawing.Text;
using System.Net.Security;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Frontend;
using CodeTitans.JSon;
using Gecko;

namespace BlueStacks.hyperDroid.GameManager
{
	public class GameManager : Form
	{
		// Interop stuff
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;

		public const string THEMES_HTML		= "themes.html";
		public const string HOME_HTML		= "home.html";
		public const string SEARCH_HTML		= "search-results.html";
		public const string LOCAL_MY_APPS_HTML	= "local-my-apps.html";
		public static float sScale = 1.0f;

		[DllImport("user32.dll")]
		private static extern int GetSystemMetrics(int which);

		[DllImportAttribute("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();

		[DllImport("user32.dll", SetLastError=true)]
		public static extern bool SetProcessDPIAware();

		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

		public enum DeviceCap
		{
			/// <summary>
			/// Logical pixels inch in X
			/// </summary>
			LOGPIXELSX = 88,
			/// <summary>
			/// Logical pixels inch in Y
			/// </summary>
			LOGPIXELSY = 90,

			VERTRES = 10,
			DESKTOPVERTRES = 117
		}

		private const int SM_CXSCREEN = 0;
		private const int SM_CYSCREEN = 1;

		public static GameManager sGameManager;
		public ToolBar mToolBarForm = null;
		public bool mUseToolBar = true;

		public static int sGameManagerPort = 2881;

		public static bool rememberChoice = false;
		public static bool sForceClose = false;
		public static bool sLaunchTwitchWindow = true;
		public static string	sSetupDir;

		public static int sTabBarHeight	= 35;
		public static int sControlBarHeight = 46;

		public TabBar	    mTabBar;

		public System.Windows.Forms.Timer mTimerTopBarSlide ;
		bool mIsSlideUp = true;

		public ControlBar  mControlBarLeft;
		public ControlBar  mControlBarRight;
		public AutoCloseMessageBox mAutoCloseMessageBox = null;

		public static int sFrontendWidth;
		public static int sFrontendHeight;
		public int mTabWidth	= 178;
		public double mTabWidthHeightRatio = 5;

		private Point mWindowedLocation;

		public const int mBorderWidth		= 1;
		public const int mCenterBorderHeight	= 1;
		public static int mContentBorderWidth	= 0;
		public static int mTabBarExtraHeight	= 0;
		public const int mGrip			= 5;

		private bool	mInstallingAppPlayer	= false;
		public bool	mCreateVendingTab	= false;
		public bool	mDontCreateAppTab	= false;

		public static String		sLocalMyAppsHtml;
		public static String		sNoWifiHtml;
		public static String		sWaitHtml;
		public static String		sStreamWindowHtml;
		public static String		sStreamWindowProdHtml;
		public static String		sStreamWindowQAHtml;
		public static String		sStreamWindowStagingHtml;
		public static String		sStreamWindowDevHtml;

		private string	mShowLogo;
		private string	mNumRecVideos;
		private string	mGoLiveEnabled;

		public static String		sHomeType;

		public static Common.HTTP.Server	sServer;

		public static Dictionary<string, string> sLocalizedString = new Dictionary<string, string>();

		public	Browser		mSetupScreenBrowser;

		public Size		mWindowSize;
		public Size		mMinimumFESize;
		public int		mCurrentWidth;
		public int		mCurrentHeight;

		public	static Size	sStreamWindowDefaultContentSize;
		public	static Point	sStreamWindowDefaultLocation;
		public	static int	sStreamWindowDefaultChromeHeight;

		public static int	sShowFonelink	= 0;

		private bool mFullScreen = false;
		public bool FullScreen { get { return mFullScreen; } }

		public IntPtr	mFrontendHandle = IntPtr.Zero;

		public static string sInstallDir;
		public static string sAssetsDir;
		public static string sAssetsCommonDataDir;

		public string	mCurrentDir	= Directory.GetCurrentDirectory();

		public string	mLastShownAppInfo = "";
		private string	mLastAppLaunched = "";
		public string	mLastAppDisplayed = "";

		private String mHomeUrl;

		private String mPackageName;
		private String mActivityName;

		public static string sAppToInstall;
		public static string sPackageToInstall;

		private static Mutex 	sGameManagerLock;
		public static String	sWindowTitle = "Bluestacks App Player";

		private List<String> mPrebundledApps;

		public static EventWaitHandle	sAppInstallEvent;

		public static bool sConfigShown = false;

		public static PrivateFontCollection	sFontCollection;
		public static FontFamily		sFontFamily;

		private Frontend.GamePad		mGamePad;
		private Frontend.InputMapper    mInputMapper;

		private static Object sAccessToggleAppListLock = new Object();
		public static string[] sToggleAppList = new String[] {
			"com.bluestacks.gamepophome",
			"com.android.vending"
		};

		public static string[] ToggleDisableAppList
		{
			get
			{
				lock(sAccessToggleAppListLock)
				{
					return sToggleAppList;
				}
			}
			set
			{
				lock(sAccessToggleAppListLock)
				{
					sToggleAppList = value;
				}
			}
		}

		delegate void InitBossHotKeyCallBack();
		public gamemanager.SystemHotKey mSystemHotKey;
		List<IntPtr> ptrList = new List<IntPtr>();

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr handle, int cmd);
		[DllImport("user32.dll", EntryPoint = "FindWindowEx", SetLastError = true)]
		static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		public class Opt : GetOpt
		{
			public String name="";	//appName
			public String p="";	// packageName
			public String a="";	// activityName
		}

		public const string	FONELINK_URL_PROD = "https://bluestacks-cloud.appspot.com/fonelink/";
		public const string	FONELINK_URL_MY_HOME = "http://192.168.1.8:8080/fonelink/";
		public const string	FONELINK_URL_MY_OFFICE = "http://10.0.5.140:8080/fonelink/";
		//public const string 	FONELINK_URL = FONELINK_URL_MY_OFFICE;
		public const string 	FONELINK_URL = FONELINK_URL_PROD;

		public const string 	DISCOVER_TAB_NAME = "Discover";
		public const string 	FONELINK_TAB_NAME = "FoneLink";

		private	string		mThemesDir		= "themes\\";
		public	static string	sSetupTheme		= "setup";
		public	static string	sDefaultTheme		= "default_theme";
		public	static string	sDefaultThemeBlack	= "default_black";

		public static string	sBaseUrl		= "http://localhost:2881/static/";

		public static int	sDpi			= Utils.DEFAULT_DPI;

		public string mDefaultLauncher;
		private string mLastTopPackage;

		private string mCurrentThemeUrl;

		private static Tab sFoneLinkTab = null;

		private Tab mHomeTab = null;

		public StreamWindow mStreamWindow;
		public TwitchWindow mTwitchWindow;
		public StreamManager mStreamManager;

		public TriangularPictureBox ResizeGameManagerBtn = new TriangularPictureBox();
		bool allowResize = false;
		bool isResizeOnFirstClick = true;
		public Dictionary<string, Image> mResizeBtnImagesDict = null;

		public static bool sWritingToFile
		{
			set
			{
				Common.HTTP.Server.s_FileWriteComplete = !value;
			}
		}

		public static bool sStreaming = false;
		public static bool sRecording = false;
		public static bool sWasRecording = false;

		public string GetCurrentThemeLocalDir()
		{
			return Path.Combine(Common.Strings.GameManagerHomeDir, mThemesDir + getCurrentTheme());
		}

		public string GetThemesDir()
		{
			return Path.Combine(Common.Strings.GameManagerHomeDir, mThemesDir);
		}

		public static string sServerRootDir = Common.Strings.GameManagerHomeDir;

		public void applyTheme(string name, string themeBaseUrl)
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\BlueStacksGameManager\Config");
			key.SetValue("theme", name);
			key.SetValue("themeUrl", themeBaseUrl);
			key.Close();

			UIHelper.RunOnUIThread(this, delegate() {
					Tab homeTab = (Tab)mTabBar.SelectedTab;
					homeTab.Controls.Remove(homeTab.mBrowser);
					homeTab.mBrowser.Dispose();

					string homePageUrl = themeBaseUrl + HOME_HTML;
					//important: set the CurrentThemeUrl to new Theme's BaseUrl
					mCurrentThemeUrl = themeBaseUrl;

					homeTab.mBrowser = new Browser(homePageUrl);
					homeTab.mBrowser.Size = new Size(homeTab.Width, homeTab.Height);
					homeTab.Controls.Add(homeTab.mBrowser);
			});
		}
        public void OnHotkey(int HotkeyID)
        {
            if (HotkeyID == mSystemHotKey.BossKey_HotKey)
            {
                if (this.WindowState != FormWindowState.Minimized)
                {
                    //hide apkinstaller
                    Process[] apk = System.Diagnostics.Process.GetProcessesByName("HD-ApkHandler");
                    if (apk.Length != 0)
                    {
                        ptrList.Add(apk[0].MainWindowHandle);
                        ShowWindow(apk[0].MainWindowHandle, 0);
                    }
                    //hide logcollector
                    Process[] log = System.Diagnostics.Process.GetProcessesByName("HD-LogCollector");
                    if (log.Length != 0)
                    {
                        ptrList.Add(log[0].MainWindowHandle);
                        ShowWindow(log[0].MainWindowHandle, 0);
                    }

                    this.WindowState = FormWindowState.Minimized;
                    this.ShowInTaskbar = false;

                }
                else
                {
                    this.WindowState = FormWindowState.Normal;
                    this.ShowInTaskbar = true;

                    this.TopMost = true;
                    this.Activate();
                    this.TopMost = false;

                    foreach (var item in ptrList)
                    {
                        ShowWindow(item, 1);
                    }
                    ptrList.Clear();
                }
                mSystemHotKey.UpdateBossHotKey(this.Handle);
            }
        }

		public string getCurrentThemeHomeUrl()
		{
			string result = Path.Combine(mCurrentThemeUrl, HOME_HTML);
			Logger.Info("getCurrentThemeHomeUrl(): " + result);
			return result;
		}

		public string getCurrentThemeSearchUrl()
		{
			string result = Path.Combine(mCurrentThemeUrl, SEARCH_HTML);
			Logger.Info("getCurrentThemeThemesUrl(): " + result);
			return result;
		}

		public string getCurrentThemeThemesUrl()
		{
			string result = Path.Combine(mCurrentThemeUrl, THEMES_HTML);
			Logger.Info("getCurrentThemeThemesUrl(): " + result);
			return result;
		}

		public string GetDefaultThemesBaseUrl(String themeName)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software");
			string baseUrl = (string)key.GetValue("gmurl", sBaseUrl);
			baseUrl = baseUrl.TrimEnd('/');
			baseUrl = baseUrl + '/';
			baseUrl += "themes/" + themeName;

			return baseUrl;
		}

		public string getCurrentTheme()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\BlueStacksGameManager\Config");
			return (string)key.GetValue("theme", sDefaultTheme);
		}

		private const string PLAYSTOREAPPINFO = "com.android.vending/com.google.android.finsky.activities.MainActivity";
		private const string PLAYLOGINAPPINFO = "com.google.android.gsf.login";
		public const string PLAYLOCATIONINFO = "com.google.android.gms";
		private const string STOREPACKAGE = "com.android.vending";
		private const string BSTCMDPROCESSOR = "com.bluestacks.BstCommandProcessor";
		public const string BSTSERVICES = "com.bluestacks.home";
		private const string KEYMAPPINGTOOL = "com.bluestacks.keymappingtool";
		private const string APPFINDER = "com.bluestacks.appfinder";
		public const string GAMEPOPHOME = "com.bluestacks.gamepophome";
		private const string RESOLVERACTIVITY = "com.android.internal.app.ResolverActivity";

		private List<string>	mStorePackagesList = new List<string>();
		private List<string>	mIAPPackagesList = new List<string>();

		/* Minimize handling on clicking taskbar icon: starts */
		const int WS_MINIMIZEBOX = 0x20000;
		const int CS_DBLCLKS = 0x8;
		const int CS_DROPSHADOW = 0x00020000;
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.Style |= WS_MINIMIZEBOX;
				cp.ClassStyle |= CS_DBLCLKS;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;

			}
		}
		/* Minimize handling on clicking taskbar icon: ends */

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
		/* Flash taskbar icon for notifications: ends */

		public bool mSessionEnding = false;
		private const int WM_QUERYENDSESSION = 0x11;
        private const int WM_HOTKEY = 0x0312;

		[STAThread]
		static int Main(String[] args)
		{
			Init();
			MouseKeyboardHook.StartKeyBoardHook();
			Utils.LogParentProcessDetails();
			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			int useScaling = (int)prodKey.GetValue("UseScaling", 0);
			if (useScaling == 0)
			{
				if (!Utils.IsOSWinXP())
					SetProcessDPIAware();
			}
			Opt opt = new Opt();
			opt.Parse(args);
			Logger.Info("pkg name = " + opt.p);
			Logger.Info("activity = " + opt.a);

			RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String installDir = (string)reg.GetValue("InstallDir");
			Logger.Info("the installdir path is " + installDir);

			string	programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			sInstallDir = Path.Combine(programData, @"BlueStacksGameManager");
			sAssetsDir = Path.Combine(GameManager.sInstallDir, "Assets");
			sAssetsCommonDataDir = Path.Combine(GameManager.sInstallDir, "Assets\\Common");
			sSetupDir = Path.Combine(programData, "BlueStacksSetup");
			Directory.SetCurrentDirectory(sInstallDir);

			if(Features.IsFeatureEnabled(Features.MULTI_INSTANCE_SUPPORT))
			{
				Logger.Info("calling HD-QuitMultiInstance to kill other instances services and processes if running");
				Utils.QuitMultiInstance(installDir);
			}

			int screenWidth  = GetSystemMetrics(SM_CXSCREEN);
			int screenHeight = GetSystemMetrics(SM_CYSCREEN);

			sDpi = Utils.GetDPI();
			Logger.Info("screenWidth = " + screenWidth + "; screenHeight = " + screenHeight);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath);
			sFrontendWidth  = (int)key.GetValue("WindowWidth");
			sFrontendHeight = (int)key.GetValue("WindowHeight");
			Logger.Info("sFrontendWidth = " + sFrontendWidth + "; sFrontendHeight" + sFrontendHeight);

			key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			sGameManagerPort = (int)key.GetValue(Common.Strings.GameManagerPortKeyName, sGameManagerPort);

			if (Common.Utils.IsAlreadyRunning(Common.Strings.GameManagerLockName, out sGameManagerLock))
			{
				Logger.Info("GameManager already running");

				/*
				 * Try to bring the GameManager window to the foreground.
				 */

				try {
					IntPtr handle = Window.FindWindow(null, sWindowTitle);
					if (handle == IntPtr.Zero)
					{
						Logger.Error("Cannot find window '" + sWindowTitle + "'");
					}

					if (!Window.SetForegroundWindow(handle))
					{
						Logger.Error("Cannot set foreground window" + Marshal.GetLastWin32Error());
					}

					Window.ShowWindow(handle, Window.SW_SHOW);

					Logger.Info("Sending WM_USER_SHOW_WINDOW to GameManager handle {0}", handle);
					int res = Window.SendMessage(handle,
							Window.WM_USER_SHOW_WINDOW,
							IntPtr.Zero,
							IntPtr.Zero);
					/*
					 * Check if the message sent has been handled
					 * res will be 1 in case it is
					 */
					if (handle != IntPtr.Zero && res != 1)
					{
						string	url = String.Format("http://127.0.0.1:{0}/{1}",
								sGameManagerPort, Common.Strings.ShowWindowUrl);
						Dictionary<string, string> data = new Dictionary<string, string>();
						Logger.Info("Sending request to: " + url);

						try
						{
							string result = Common.HTTP.Client.Post(url, data, null, false);
							Logger.Info("showwindow result: " + result);
						}
						catch (Exception ex)
						{
							Logger.Error(ex.ToString());
							Logger.Error("Post failed. url = {0}, data = {1}", url, data);
						}
					}

					if (handle != IntPtr.Zero)
					{
						string	url = String.Format("http://127.0.0.1:{0}/{1}",
								sGameManagerPort, Common.Strings.ShowAppUrl);
						Dictionary<string, string> data = new Dictionary<string, string>();
						data.Add("package", opt.p);
						data.Add("activity", opt.a);
						Logger.Info("Sending request to: " + url);

						try
						{
							string result = Common.HTTP.Client.Post(url, data, null, false);
							Logger.Info("showwindow result: " + result);
						}
						catch (Exception ex)
						{
							Logger.Error(ex.ToString());
							Logger.Error("Post failed. url = {0}, data = {1}", url, data);
						}
					}

				} catch (Exception exc) {
					Logger.Error(exc.ToString());
				}
				Environment.Exit(0);
			}

			ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

			if (Common.Strings.IsEngineLegacy())
				Utils.KillProcessByName("HD-Frontend");
			else
				Utils.KillProcessByName("HD-Plus-Frontend");

			Utils.KillProcessByName("HD-OBS");

			ServicePointManager.DefaultConnectionLimit = 1000;
			Application.EnableVisualStyles();
			Application.SetCompatibleTextRenderingDefault(false);
			string tabStyleTheme = null;
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string enableStyleTheme = (string)configKey.GetValue("EnableStyleTheme", null);
			if(string.IsNullOrEmpty(enableStyleTheme))
			{
				using (RegistryKey tempKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath))
				{
					tempKey.SetValue("TabStyleTheme", "Default");
					tempKey.SetValue("ParentStyleTheme", "Em");
				}
				Application.Run(new GameManager(opt.p, opt.a));
			}
			else
			{
				tabStyleTheme = (string)configKey.GetValue("TabStyleTheme", tabStyleTheme);
				if (string.IsNullOrEmpty(tabStyleTheme))
				{
					Application.Run(new GameManagerSelectTheme(opt.p, opt.a));
				}
				else
				{
					Application.Run(new GameManager(opt.p, opt.a));
				}
			}
			return 0;
		}

		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert,
				X509Chain chain, SslPolicyErrors policyErrors)
		{
			return true;
		}

		public GameManager(String package, String activity)
		{
            
			string path = Directory.GetCurrentDirectory();
			sGameManager = this;
			string parentStyleTheme = "Em";
			string tabStyleTheme = "Default";
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", parentStyleTheme);
			tabStyleTheme = (string)configKey.GetValue("TabStyleTheme", tabStyleTheme);

			Logger.Info("Parent style: " + parentStyleTheme);
			Logger.Info("Theme style: " + tabStyleTheme);
			if(parentStyleTheme == "Em")
			{
				sAssetsDir = sAssetsDir + "\\" + parentStyleTheme + "\\" + tabStyleTheme;

			}
			else if (parentStyleTheme == "Toob")
			{
				mContentBorderWidth = 8;
				int  tabBarExtraHeight = 0;
				mTabBarExtraHeight = (int)configKey.GetValue("TabBarExtraHeight", tabBarExtraHeight);
				sAssetsDir = sAssetsDir + "\\" + parentStyleTheme + "\\" + tabStyleTheme;
			}
			else
			{
				sAssetsDir = sAssetsDir + "\\" + "Em" + "\\" + "Default";
			}

            Assets.Init();

			InitStorePackages();
			InitIAPPackages();
			UpdateLocalUrls();
			InitPowerEvents();

			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			sHomeType = (string)prodKey.GetValue("homeType", "gphome");

			if (String.Compare(sHomeType, "html", true) == 0)
			{
				DownloadJsonFiles();
			}

			string currentThemeName = getCurrentTheme();
			string defaultThemeBaseUrl = GetDefaultThemesBaseUrl(sDefaultTheme);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software");
			string baseUrl = (string)key.GetValue("gmurl", "");

			if (String.Compare(baseUrl, "") != 0)		// use gmurl
			{
				baseUrl = baseUrl.TrimEnd('/');
				baseUrl = baseUrl + '/';
				baseUrl += "themes/" + currentThemeName;
				mCurrentThemeUrl = baseUrl;
			}
			else
			{
				mCurrentThemeUrl = (string)configKey.GetValue("themeUrl", defaultThemeBaseUrl); 
				mCurrentThemeUrl = mCurrentThemeUrl.TrimEnd('/');
			}

			mCurrentThemeUrl = mCurrentThemeUrl + '/';
			defaultThemeBaseUrl =  defaultThemeBaseUrl + '/';

			string setupBaseUrl = String.Format("{0}themes/{1}/", sBaseUrl, sSetupTheme);

			Uri prepareHomeUri = new Uri(new Uri(setupBaseUrl), "wait.html");

			Uri themeBaseUri = new Uri(mCurrentThemeUrl);
			Uri homeUri = new Uri(themeBaseUri, HOME_HTML);
			mHomeUrl = homeUri.ToString();

			mPackageName = package;
			mActivityName = activity;

			mDefaultLauncher = (string)configKey.GetValue("DefaultLauncher", GAMEPOPHOME);

			if (!Directory.Exists(sSetupDir))
			{
				Directory.CreateDirectory(sSetupDir);
			}

			sLocalizedString = Locale.Strings.InitLocalization(null);

			sTabBarHeight = GetTabBarHeight();
			mTabWidth = (int)(sTabBarHeight * mTabWidthHeightRatio);
			sControlBarHeight = GetControlBarHeight();

			mWindowSize = GetGMSizeGivenFESize(sFrontendWidth, sFrontendHeight);

			Logger.Info("mWindowsize = "+mWindowSize.Width+" x "+mWindowSize.Height);

			sFontCollection = new PrivateFontCollection();
			sFontCollection.AddFontFile("Lato.ttf");
			sFontCollection.AddFontFile("Roboto-Medium.ttf");
			sFontFamily = sFontCollection.Families[0];

			this.Icon = Utils.GetApplicationIcon();

			if (mResizeBtnImagesDict == null)
			{
				mResizeBtnImagesDict = new Dictionary<string, Image>();
				mResizeBtnImagesDict.Add("resize_tool",
						Image.FromFile(Path.Combine(sAssetsCommonDataDir, "resize_tool.png")));
				mResizeBtnImagesDict.Add("resize_tool_click",
						Image.FromFile(Path.Combine(sAssetsCommonDataDir, "resize_tool_click.png")));
				mResizeBtnImagesDict.Add("resize_tool_hover",
						Image.FromFile(Path.Combine(sAssetsCommonDataDir, "resize_tool_hover.png")));
			}

			string value = Registry.LocalMachine.OpenSubKey(Strings.GMConfigPath).GetValue("IsHideTabBarMaximized", false.ToString()).ToString();

			if (value.Equals(true.ToString(),StringComparison.InvariantCultureIgnoreCase ))
			{
				mTimerTopBarSlide = new System.Windows.Forms.Timer();
				mTimerTopBarSlide.Interval = 10;
				mTimerTopBarSlide.Tick += HandleTopBarSlide_TimerTick;
			}

			//
			// pictureBox1
			//
			this.ResizeGameManagerBtn.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Bottom | System.Windows.Forms.AnchorStyles.Right)));
			this.ResizeGameManagerBtn.Cursor = System.Windows.Forms.Cursors.SizeNWSE;
			this.ResizeGameManagerBtn.Tag = "resize_tool";
			this.ResizeGameManagerBtn.Image = mResizeBtnImagesDict[(String)ResizeGameManagerBtn.Tag];
			this.ResizeGameManagerBtn.Name = "ResizeGameManagerBtn";
			this.ResizeGameManagerBtn.Size = new System.Drawing.Size(Convert.ToInt32(sControlBarHeight * .8), Convert.ToInt32(sControlBarHeight * .8));
			this.ResizeGameManagerBtn.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
			this.ResizeGameManagerBtn.TabIndex = 1;
			this.ResizeGameManagerBtn.TabStop = false;
			this.ResizeGameManagerBtn.MouseUp += new System.Windows.Forms.MouseEventHandler(this.ResizeButtonMouseUp);
			this.ResizeGameManagerBtn.MouseMove += new System.Windows.Forms.MouseEventHandler(this.ResizeButtonMouseMove);
			this.ResizeGameManagerBtn.MouseDown += new System.Windows.Forms.MouseEventHandler(this.ResizeButtonMouseDown);
			this.ResizeGameManagerBtn.MouseLeave += new System.EventHandler(this.ResizeButtonMouseLeave);
			this.ResizeGameManagerBtn.MouseEnter += new EventHandler(this.ResizeButtonMouseEnter);
			this.ResizeGameManagerBtn.BackColor = Color.Transparent;

			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = mWindowSize;
			this.ResizeGameManagerBtn.Location = new System.Drawing.Point(this.ClientSize.Width - ResizeGameManagerBtn.Size.Width, this.ClientSize.Height - ResizeGameManagerBtn.Size.Height);
			this.Text = sWindowTitle;
			this.BackColor = GMColors.FormBackColor;
			this.StartPosition = FormStartPosition.Manual;
			this.Load += new System.EventHandler(this.FormLoad);
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormGradientPaint);

			this.LocationChanged += new System.EventHandler(this.HandleLocationChanged);
			this.Activated += HandleActivatedEvent;
			this.Deactivate += HandleDeactivateEvent;
			this.KeyPreview = true;
			this.VisibleChanged += this.FormVisibleChanged;
			this.Resize += HandleResizeEvent;
			this.ResizeBegin += HandleResizeBeginEvent;
			this.ResizeEnd += HandleResizeEndEvent;

			int minWidth, minHeight;
			Utils.GetMinimumFEWindowSize(out minWidth, out minHeight);
			if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
			{
				if (sFrontendWidth > sFrontendHeight)
				{
					minWidth = minWidth * sFrontendHeight * 8 / (sFrontendWidth * 10);
				}
				else
				{
					minWidth = minWidth * sFrontendWidth * 8 / (sFrontendHeight * 10);
				}
			}

			this.Controls.Add(this.ResizeGameManagerBtn);

			InitTabBar();
			this.MinimumSize =  GetGMSizeGivenFESize(minWidth, minHeight);

			mCurrentWidth = this.Width;
			mCurrentHeight = this.Height;

			this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMouseDown);

			NetworkChange.NetworkAvailabilityChanged += 
				new NetworkAvailabilityChangedEventHandler(OnNetworkAvailabilityChanged);

			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
			
			InitControlBars(true, false);

			InitPrebundledAppsList();

			sAppInstallEvent = new EventWaitHandle(true, EventResetMode.AutoReset);

			Thread httpThread = new Thread(SetupHTTPServer);
			httpThread.IsBackground = true;
			httpThread.Start();

			TimelineStatsSender.Init();

			if (mUseToolBar)
			{
				ToolBar.SetToolBarParams();
				AddToolBar();
			}

			Point location = GetDefaultGMWindowPosition();
			Logger.Info("GM Location = ({0} x {1})", location.X, location.Y);

			int regX = (int)configKey.GetValue("WindowLeft", -1);
			int regY = (int)configKey.GetValue("WindowTop", -1);

			if (regX >= 0 && regX < GetSystemMetrics(SM_CXSCREEN) &&
					regY >= 0 && regY < GetSystemMetrics(SM_CYSCREEN))
			{
				location.X = regX;
				location.Y = regY;
			}

			mInputMapper = InputMapper.Instance();

			this.Location = location;

			Thread toolBarSetupThread = new Thread(delegate(){
					ToggleDisableAppList = Utils.ToggleDisableAppList;
					SetToolBarButtonState();
					});
			toolBarSetupThread.IsBackground = true;
			toolBarSetupThread.Start();
        }

        private void InitBossHotKey()
        {
            if (this.InvokeRequired)
            {
                InitBossHotKeyCallBack cb = new InitBossHotKeyCallBack(InitBossHotKey);
                this.Invoke(cb, new object[] { });
            }
            else
            {
                //regist hotkey 
                mSystemHotKey = new gamemanager.SystemHotKey(this.Handle);
                mSystemHotKey.OnHotkey += new gamemanager.HotkeyEventHandler(OnHotkey);
                mSystemHotKey.UpdateBossHotKey(this.Handle);
            }
        }

		private Point GetDefaultGMWindowPosition()
		{
			if (Oem.Instance.IsStreamWindowEnabled) {
				SetDefaultStreamWindowParams();
			}
			Point location = new Point();
			int gameUsableScreenWidth  = GetSystemMetrics(SM_CXSCREEN) - sStreamWindowDefaultContentSize.Width;
			int screenWidth = GetSystemMetrics(SM_CXSCREEN);
			int screenHeight  = GetSystemMetrics(SM_CYSCREEN);
			int gameUsableWidth, gameUsableHeight;
			Utils.GetWindowWidthAndHeight(gameUsableScreenWidth, screenHeight, out gameUsableWidth, out gameUsableHeight);
			Size gameUsableWindowSize = GetGMSizeGivenFESize(gameUsableWidth, gameUsableHeight);

			location.X = (gameUsableScreenWidth - gameUsableWindowSize.Width)/2 +
				ToolBar.sToolBarWidth/2;

			if (location.X - ToolBar.sToolBarWidth < 0)
				location.X = ToolBar.sToolBarWidth;

			location.Y = (screenHeight - gameUsableWindowSize.Height)/2;

			return location;
		}
		private void ResizeButtonMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Logger.Info("Resize Btn MouseDown");
			allowResize = true;
			isResizeOnFirstClick = false;
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Image = mResizeBtnImagesDict[(String)button.Tag + "_click"];
			}
		}

		private void ResizeButtonMouseUp(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Logger.Info("Resize Btn MouseUp");
			allowResize = false;
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Image = mResizeBtnImagesDict[(String)button.Tag + "_hover"];
			}
		}
		private void ResizeButtonMouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			Logger.Info("Resize Btn MouseMove");
			Logger.Info("Allow resize: " + allowResize);
			if (allowResize && !isResizeOnFirstClick)
			{
				isResizeOnFirstClick = true;
				return;
			}
			if (allowResize)
			{
				this.Height = ResizeGameManagerBtn.Top + e.Y;
				this.Width = ResizeGameManagerBtn.Left + e.X;

				ResizeGameManagerBtn.Top = this.Height - ResizeGameManagerBtn.Height;
				ResizeGameManagerBtn.Left = this.Width - ResizeGameManagerBtn.Width;
			}
		}

		/*
		 * As the borderstyle is none, currently we have same GM window size
		 * as GM Client Window Size.
		 */
		private Size GetGMSizeGivenFESize(int feWidth, int feHeight)
		{
			return new Size(feWidth + 2 * mBorderWidth + 2 * mContentBorderWidth,
					feHeight + 2 * mBorderWidth + sTabBarHeight + mCenterBorderHeight + 2 * mContentBorderWidth + mTabBarExtraHeight);
		}

        private Size GetFESizeHorizontalBySreen()
        {

            double d = 1;
            if (sFrontendWidth > sFrontendHeight)
            {
                d = (double)sFrontendHeight / sFrontendWidth;
            }
            else
            {
                d = (double)sFrontendWidth / sFrontendHeight;
            }

            Size temp = new Size(SystemInformation.WorkingArea.Width, (int)(SystemInformation.WorkingArea.Width * d));
            Size s = gamemanager.UserSettingsData.CountSize(temp, 2 * GameManager.mBorderWidth + 2 * GameManager.mContentBorderWidth, 2 * GameManager.mBorderWidth + GameManager.sTabBarHeight + GameManager.mCenterBorderHeight +
                2 * GameManager.mContentBorderWidth + GameManager.mTabBarExtraHeight);
            return s;
        }

        private Size GetFESizeVerticalBySreen()
        {
            double d = 1;
            if (sFrontendWidth > sFrontendHeight)
            {
                d = (double)sFrontendHeight / sFrontendWidth;
            }
            else
            {
                d = (double)sFrontendWidth / sFrontendHeight;
            }

            Size temp = new Size((int)(SystemInformation.WorkingArea.Height * d), SystemInformation.WorkingArea.Height);
            Size s = gamemanager.UserSettingsData.CountSize(temp, 2 * GameManager.mBorderWidth + 2 * GameManager.mContentBorderWidth, 2 * GameManager.mBorderWidth + GameManager.sTabBarHeight + GameManager.mCenterBorderHeight +
                2 * GameManager.mContentBorderWidth + GameManager.mTabBarExtraHeight);

            return s;
        }

        public void WebResizeWindow()
        {
            if (!this.FullScreen)
            {
                Size s;
                if (sFrontendWidth > sFrontendHeight)
                {
                    s = this.GetGMSizeGivenFESize(sFrontendWidth, sFrontendHeight);
                }
                else
                {
                    s = this.GetGMSizeGivenFESize(sFrontendHeight , sFrontendWidth);
                }
                this.ClientSize = s;
            }
        }

        public void AppResizeWindowIfNeed()
        {
            if (!this.FullScreen)
            {
                Size s;
                if (sFrontendWidth < sFrontendHeight)
                {
                    s = this.GetGMSizeGivenFESize(sFrontendWidth, sFrontendHeight);
                    this.ClientSize = s;
                }
            }
        }

		public void SetupGamePad()
		{
			if (mGamePad == null)
			{
				Logger.Info("Setting up Gamepad");
				mGamePad = new Frontend.GamePad();
				mGamePad.Setup(mInputMapper, this.Handle);
			}
		}

		private void InitStorePackages()
		{
			mStorePackagesList.Add("com.android.vending");
			mStorePackagesList.Add("com.amazon.venezia");
			mStorePackagesList.Add("com.qihoo.gameunion");
			mStorePackagesList.Add("com.baidu.appsearch");
			mStorePackagesList.Add("com.baidu.hao123");
			mStorePackagesList.Add("com.mappn.pad.gfan");
			mStorePackagesList.Add("com.hiapk.marketpho");
			mStorePackagesList.Add("com.tencent.mobileqq");

		}

		private bool IsStorePackage(string packageName)
		{
			if (mStorePackagesList.Contains(packageName))
				return true;

			return false;
		}

		private void InitIAPPackages()
		{
			mIAPPackagesList.Add("com.pop.store");
		}

		private bool IsIAPPackage(string packageName)
		{
			if (mIAPPackagesList.Contains(packageName))
				return true;

			return false;
		}

		private static void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
		{
			if (e.IsAvailable)
			{
				Logger.Info("Network is available");
				if (sFoneLinkTab != null)
				{
					UIHelper.RunOnUIThread(sGameManager, delegate() {
							sFoneLinkTab.GetBrowser().Navigate("javascript:onLineHandler()");
							});
				}
			}
			else
			{
				Logger.Info("Network is NOT available");
			}
		}

		private void HandleDisplaySettingsChanged(Object sender, EventArgs evt)
		{
			Logger.Info("HandleDisplaySettingsChanged()");
			ResizeGameManager();
		}

		public void ShakeWindow()
		{
			int movement = 5;
			int dx = 0;
			int numSteps = 10;
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

		private bool mUserResize = true;

		private void ResizeGameManager()
		{
			mUserResize = false;
			if (mFullScreen)
			{
				ResizeGameManagerBtn.Visible = false;
				Logger.Info("Setting Resize Btn visible: false");
				ResizeGameManagerFullScreen();
			}
			else
			{
				ResizeGameManagerBtn.Visible = true;
				Logger.Info("Setting Resize Btn visible: true");
				ResizeGameManagerWindowed();
			}

			GameManagerResized();
			mUserResize = true;
		}

		public void HandleResizeEvent(Object sender, EventArgs e)
		{
			Logger.Info("GM: HandleResizeEvent, this.width = {0}; this.Height = {1}",
					this.Width, this.Height);
			if (mUserResize)
				GameManagerResized();
		}

		private void GameManagerResized()
		{
			if (this.WindowState == FormWindowState.Minimized)
			{
				return;
			}

			Logger.Info("this.width: {0}, this.height: {1}", this.Width, this.Height);
			Logger.Info("frontendwidth: {0}, frontendheight: {1}", sFrontendWidth, sFrontendHeight);

			if (mTimerTopBarSlide != null)
			{
				if (mFullScreen)
				{
					//slide down effect of tab bar
					mTabBar.Size = new Size(this.Width - 2 * mBorderWidth, this.Height - 2 * mBorderWidth - mTabBarExtraHeight + mTabBar.ItemSize.Height);
				}
				else
				{
					mTabBar.Size = new Size(this.Width - 2 * mBorderWidth, this.Height - 2 * mBorderWidth - mTabBarExtraHeight);
				}
			}
			else
			{
				mTabBar.Size = new Size(this.Width - 2 * mBorderWidth, this.Height - 2 * mBorderWidth - mTabBarExtraHeight);
			}

			mTabBar.Location = new Point(mBorderWidth, mBorderWidth + mTabBarExtraHeight);
			mTabBar.mTabPageSize = new Size(mTabBar.Width, mTabBar.Height - sTabBarHeight - mCenterBorderHeight);

			if (mTabBar.TabPages.Count != 0)
				mTabBar.AdjustTabWidth();

			if (mContentBorderWidth > 0)
			{
				foreach (TabPage tabPage in mTabBar.TabPages)
				{
					Tab tab = (Tab)tabPage;
					if(tab.mBrowser != null)
					{
						tab.mBrowser.Size = new Size(tab.Size.Width - 2 * mContentBorderWidth - 1, tab.Size.Height - 2 * mContentBorderWidth - 1);
						tab.mBrowser.Location = new System.Drawing.Point(mContentBorderWidth + 1, mContentBorderWidth + 1);
					}
				}
			}

			if (mStreamManager != null)
			{
				mStreamManager.ResizeStream();
			}

			if (mToolBarForm != null)
			{
				ToolBar.sToolBarHeight = this.Height - sControlBarHeight - mBorderWidth - mTabBarExtraHeight;
				mToolBarForm.Height = ToolBar.sToolBarHeight;
			}

			UpdateControlBar();

			SaveWindowSize();

			if (IsPortrait())
			{
				if (mAutoCloseMessageBox == null || mAutoCloseMessageBox.IsDisposed)
				{
					mAutoCloseMessageBox = new AutoCloseMessageBox();
				}
				mAutoCloseMessageBox.StartPosition = FormStartPosition.CenterParent;
				mAutoCloseMessageBox.ShowMsgBox("Please rotate the screen for optimal experience");
			}
			else if (mAutoCloseMessageBox != null)
			{
				mAutoCloseMessageBox.Hide();
			}
		}

		public void HandleResizeBeginEvent(Object sender, EventArgs e)
		{
			if (mStreamManager != null)
			{
				if (mStreamManager.mReplayBufferEnabled)
					mStreamManager.StopReplayBuffer();

				if (sRecording)
				{
					mStreamManager.StopRecord(true);
					sWasRecording = true;
				}
				else
					sWasRecording = false;
			}
		}

		public void HandleResizeEndEvent(Object sender, EventArgs e)
		{
			if (mStreamManager != null)
			{
				mStreamManager.SetConfig();

				if (mStreamManager.mReplayBufferEnabled)
					mStreamManager.StartReplayBuffer();

				if (sWasRecording)
					mStreamManager.StartRecord(true);
			}
			Logger.Info("Setting allowResize: false");
			allowResize = false;
		}

		private void SaveWindowSize()
		{
			if (!mFullScreen && mTabBar != null && mTabBar.TabPages.Count > 0)
			{
				Tab currentTab = mTabBar.GetCurrentTab();

				int screenWidth  = GetSystemMetrics(SM_CXSCREEN);
				int screenHeight = GetSystemMetrics(SM_CYSCREEN);
				if (currentTab != null &&
						currentTab.mContentWidth < screenWidth &&
						currentTab.mContentHeight < screenHeight &&
						this.mMinimumFESize.Width <= currentTab.mContentWidth &&
						this.mMinimumFESize.Height <= currentTab.mContentHeight)
				{
					if (!Features.IsFeatureEnabled(Features.IS_CHINA_UI))
                    {
                        RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.FrameBufferRegKeyPath);
                        key.SetValue("WindowWidth", currentTab.mContentWidth);
                        key.SetValue("WindowHeight", currentTab.mContentHeight);
                        key.Close();
                    }
					mWindowSize = this.Size;
				}
			}

			mCurrentWidth = this.Width;
			mCurrentHeight = this.Height;
		}

		private void SaveWindowLocation()
		{
			if (!mFullScreen)
			{
				RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
				configKey.SetValue("WindowLeft", this.Left);
				configKey.SetValue("WindowTop", this.Top);
				configKey.Close();
			}
		}

		private void ResizeGameManagerFullScreen()
		{
			mWindowedLocation = this.Location;
			Common.Interop.Window.SetFullScreen(this.Handle);
		}

		private void ResizeGameManagerWindowed()
		{
			Size newSize = new Size();
			if (!IsPortrait())
			{
				newSize.Width  = mWindowSize.Width;
				newSize.Height = mWindowSize.Height;
				this.ClientSize = newSize;
			}
			else
			{
				newSize.Width  = mWindowSize.Height;
				newSize.Height = mWindowSize.Width;
				this.ClientSize = newSize;
			}

			this.Location = mWindowedLocation;
		}

		private bool IsPortrait()
		{
			ScreenOrientation so = SystemInformation.ScreenOrientation;

			return (so == ScreenOrientation.Angle90) ||
				(so == ScreenOrientation.Angle270);
		}

		public void UpdateLocalUrls()
		{
			string baseUrl			= String.Format("http://localhost:{0}/static/themes", sGameManagerPort);
			string themeUrl			= String.Format("{0}/{1}/", baseUrl, getCurrentTheme());
			sLocalMyAppsHtml		= themeUrl + "local-my-apps.html";
			sNoWifiHtml		    	= themeUrl + "no-wifi.html";
			sWaitHtml          		= themeUrl + "wait.html";
			sStreamWindowHtml		= "http://bluestacks-tv.appspot.com/home";
			sStreamWindowProdHtml		= "http://bluestacks-tv-prod.appspot.com/home";
			sStreamWindowQAHtml		= "http://bluestacks-tv-qa.appspot.com/home";
			sStreamWindowStagingHtml	= "http://bluestacks-tv-staging.appspot.com/home";
			sStreamWindowDevHtml		= "http://bluestacks-tv-dev.appspot.com/home";
		}

		private void InitPowerEvents()
		{
			try
			{
				//log-on event
				var interval = new TimeSpan(0, 0, 1);
				const string isWin32Process = "TargetInstance isa \"Win32_Process\"";
				WqlEventQuery stopQuery = new WqlEventQuery("__InstanceDeletionEvent", interval, isWin32Process);
				ManagementEventWatcher logonWatcher = new ManagementEventWatcher(stopQuery);
				logonWatcher.EventArrived += LogOnEventArrived;
				logonWatcher.Start();

				//system suspend event
				WqlEventQuery q = new WqlEventQuery("Win32_PowerManagementEvent");
				ManagementScope scope = new ManagementScope("root\\CIMV2");
				ManagementEventWatcher suspendWatcher = new ManagementEventWatcher(scope, q);
				suspendWatcher.EventArrived += PowerEventArrive;
				suspendWatcher.Start();
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to init power event... Err : " + ex.ToString());
			}
		}

		private void LogOnEventArrived(object sender, EventArrivedEventArgs e)
		{
			var o = (ManagementBaseObject)e.NewEvent[ "TargetInstance" ];
			string name = (string)o[ "Name" ];

			if (String.Compare(name, "LogonUI.exe", true) == 0)
			{
				Logger.Info("switch user event has arrived....stopping stream");
				if (sStreaming)
				{
					mStreamManager.StopStream();
					mStreamManager.StopRecord(true);
					mStreamWindow.mBrowser.LoadUrl(sStreamWindowProdHtml);
				}
			}
		}

		private void PowerEventArrive(object sender, EventArrivedEventArgs e)
		{
			try
			{
				foreach (PropertyData pd in e.NewEvent.Properties)
				{
					if (pd == null || pd.Value == null) continue;
					string powerEventValue = pd.Value.ToString();
					Logger.Info("PowerEvent : " + powerEventValue);

					if (String.Compare(powerEventValue, "4", true) == 0)
					{
						Logger.Info("power event has arrived...stopping stream");
						if (sStreaming)
						{
							mStreamManager.StopStream();
							mStreamManager.StopRecord(true);
							mStreamWindow.mBrowser.LoadUrl(sStreamWindowProdHtml);
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("An error ocured while setting time....Err : " + ex.ToString());
			}
		}

		private void DownloadJsonFiles()
		{
			Thread thr = new Thread(delegate() {
					try
					{
						GMApi.GetChannelNamesJson();
						GMApi.GetChannelAppsJson();
						GMApi.GetWebAppsJson();
						GMApi.GetThemesJson();
					}
					catch(Exception e)
					{
						Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
					}
					});
			thr.IsBackground = true;
			thr.Start();
		}

		private void FormVisibleChanged(object sender, EventArgs e)
		{
			if (this.mSessionEnding)
			{
				return;
			}

			if (this.Visible)
			{
				if (mToolBarForm != null)
					mToolBarForm.Show();

				if (mStreamWindow != null)
					mStreamWindow.Show();
			}
			else
			{
				if (mToolBarForm != null)
					mToolBarForm.Hide();

				if (mStreamWindow != null)
					mStreamWindow.Hide();
			}
		}

		public void ShowStreamWindow()
		{
			if (mStreamWindow == null)
			{
				if (sLaunchTwitchWindow)
				{
					Logger.Info("Launching twitch.tv silently");
					AddTwitchWindow();
					sLaunchTwitchWindow = false;
				}
				AddStreamWindow();
			}
			else
			{
				mStreamWindow.WindowState = FormWindowState.Normal;
				mStreamWindow.BringToFront();
			}

		}

		private void SetupHTTPServer()
		{
			Dictionary<String, Common.HTTP.Server.RequestHandler> routes =
				new Dictionary<String, Common.HTTP.Server.RequestHandler>();

			routes.Add("/" + Common.Strings.GMNotificationUrl, HTTPHandler.GMNotificationHandler);
			routes.Add("/" + Common.Strings.AppDisplayedUrl, HTTPHandler.AppDisplayedHandler);
			routes.Add("/" + Common.Strings.AppLaunchedUrl, HTTPHandler.AppLaunchedHandler);
			routes.Add("/" + Common.Strings.ShowAppUrl, HTTPHandler.ShowAppHandler);
			routes.Add("/" + Common.Strings.ShowWindowUrl, HTTPHandler.ShowWindowHandler);
			routes.Add("/" + Common.Strings.IsGMVisibleUrl, HTTPHandler.IsVisibleHandler);
			routes.Add("/" + Common.Strings.S2PConfiguredUrl, HTTPHandler.S2PConfiguredHandler);
			routes.Add("/" + Common.Strings.AppUninstalledUrl, HTTPHandler.AppUninstalledHandler);
			routes.Add("/" + Common.Strings.GMLaunchWebTab, HTTPHandler.GMLaunchWebTab);
			routes.Add("/ping", HTTPHandler.PingHandler);
			routes.Add("/quit", HTTPHandler.ForceQuitHandler);
			routes.Add("/showobs", HTTPHandler.ShowObsHandler);
			routes.Add("/hideobs", HTTPHandler.HideObsHandler);
			routes.Add("/google", HTTPHandler.OpenGoogleHandler);
			routes.Add("/replaybuffersaved", HTTPHandler.ReplayBufferSavedHandler);
			routes.Add("/closecrashedapptab", HTTPHandler.AppCrashedHandler);
			routes.Add("/reportobserror", HTTPHandler.ReportObsErrorHandler);

			for (int port = 2881; port < 2891; port++)
			{
				try
				{
					sServer	= new Common.HTTP.Server(port, routes, sServerRootDir);
					sServer.Start();
					sGameManagerPort = port;
					Logger.Info("GameManager server listening on port: " + sServer.Port);

					/* write server port to the registry */
					RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\BlueStacksGameManager\Config");
					key.SetValue(Common.Strings.GameManagerPortKeyName, sServer.Port, RegistryValueKind.DWord);
					key.Close();

					Thread fqdnThread = new Thread(SendFqdn);
					fqdnThread.IsBackground = true;
					fqdnThread.Start();

					sServer.Run();

					sBaseUrl = "http://localhost:" + sGameManagerPort + "/static/";
					this.UpdateLocalUrls();

					break;
				}
				catch (Exception)
				{
				}
			}
		}

		private void SendFqdn()
		{
			try
			{
				while (true)
				{
					if (VmCmdHandler.FqdnSend(sGameManagerPort, "GameManager") != null)
					{
						break;
					}

					Thread.Sleep(2000);
				}
			}
			catch (Exception e)
			{
				Logger.Info(e.ToString());
			}
		}

		private void AddToolBar()
		{
			int SideBarVisible = Convert.ToInt32(Utils.GetValueFromRegistry(Common.Strings.GMConfigPath, "SideBarVisible", -1));
			if (SideBarVisible == -1)
			{
				Thread t = new Thread(delegate()
						{
						Thread.Sleep(2000);

						UIHelper.RunOnUIThread(sGameManager, delegate()
							{
							mToolBarForm = new ToolBar();
							ToolBar.sMakeVisible = true;
							mToolBarForm.Show();
							mToolBarForm.Width = ToolBar.sToolBarWidth;
							mToolBarForm.Height = ToolBar.sToolBarHeight;
							mToolBarForm.Left = this.Left - ToolBar.sToolBarWidth;
							mToolBarForm.Top = this.Top + mBorderWidth + sControlBarHeight + mTabBarExtraHeight;
							mToolBarForm.Owner = this;
							SetToolBarButtonState();
							if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
                            {
                                Tab tab = sGameManager.mTabBar.GetCurrentTab();
                                if (tab != null && tab.mTabType == "web")
                                {
                                    sGameManager.WebResizeWindow();
                                }
                            }
							});
						});
				t.IsBackground = true;
				t.Start();
			}
		}

		private void SetToolBarButtonState()
		{
			if (mTabBar.TabPages.Count != 0 && GameManager.sGameManager.mToolBarForm != null)
			{
				Tab selectedTab = (Tab)(mTabBar.SelectedTab);
				if (mTabBar.GetCurrentTabType() == "app") {
					mToolBarForm.EnableGenericAppTabButtons();
					lock(sAccessToggleAppListLock)
					{
						if (Array.IndexOf(ToggleDisableAppList, selectedTab.mPackage) == -1) {
							mToolBarForm.EnableToggleAppTabButton();
						} else {
							mToolBarForm.DisableToggleAppTabButton();
						}
					}
				} else {
					GameManager.sGameManager.mToolBarForm.DisableAppTabButtons();
				}
			}
		}

		public void UserAtHome()
		{
			Logger.Info("User reached home");
			try
			{
				Tab tab = (Tab)mTabBar.SelectedTab;
				Logger.Info("mRunAppRequestPending: {0}", tab.mRunAppRequestPending);

				if (tab.mRunAppRequestPending == false)
				{
					UIHelper.RunOnUIThread(sGameManager, delegate() {
						mTabBar.CloseCurrentTab();
					});

				}
			}
			catch (Exception e)
			{
				// A very small window, but if the tab was closed after getting the
				// selected tab from tabBar and before doing further operations, this will happen.
				// No way to prevent it, so ignore it...
				Logger.Error("Failed in UserAtHome. Ignoring. Error: " + e.ToString());
			}
		}

		internal int GetTabBarHeight()
		{
			return GetControlBarHeight();
		}

		private int GetControlBarHeight()
		{
			int screenHeight = System.Windows.Forms.Screen.PrimaryScreen.Bounds.Height;
//			Logger.Info("getcontrolbarheight , screen.height = " + screenHeight);
			if (mFullScreen)
			{
				return (int)((5 * screenHeight)/100) - mBorderWidth;
			}
			else
			{
				return (int)( (5 * sFrontendHeight)/(100-6.3)) - mBorderWidth;
			}
		}

		public void HandleTopBarSlide_TimerTick(object sender, EventArgs e)
		{
			if (mIsSlideUp)
			{
				if (mTabBar.ItemSize.Height == (mTabBar.Location.Y * -1))
				{
					mTimerTopBarSlide.Enabled = false;
					mIsSlideUp = false;
					MouseKeyboardHook.StartMouseHook((int)(this.Height * .01), false);
				}
				else
				{
					mTabBar.Location = new Point(mTabBar.Location.X, mTabBar.Location.Y - 1);
					mControlBarLeft.Location = new Point(mControlBarLeft.Location.X, mControlBarLeft.Location.Y - 1);
					mControlBarRight.Location = new Point(mControlBarRight.Location.X, mControlBarRight.Location.Y - 1);
				}
			}
			else
			{
				if ((mTabBar.Location.Y * -1) < 5)
				{
					mTimerTopBarSlide.Enabled = false;
					mIsSlideUp = true;
					MouseKeyboardHook.StartMouseHook((int)(this.Height * .06), true);
					mTabBar.Location = new Point(mTabBar.Location.X, 1);
					mControlBarLeft.Location = new Point(mControlBarLeft.Location.X, 1);
					mControlBarRight.Location = new Point(mControlBarRight.Location.X, 1);
				}
				else
				{
					mTabBar.Location = new Point(mTabBar.Location.X, mTabBar.Location.Y + 5);
					mControlBarLeft.Location = new Point(mControlBarLeft.Location.X, mControlBarLeft.Location.Y + 5);
					mControlBarRight.Location = new Point(mControlBarRight.Location.X, mControlBarRight.Location.Y + 5);
				}
			}
		}

		private void MouseHook_MouseAction(object sender, EventArgs e)
		{
			if (mTimerTopBarSlide != null)
			{
				mTimerTopBarSlide.Enabled = true;
			}
		}

		public string GetHomeUrl()
		{
			return mHomeUrl;
		}

		public void BlinkTaskbarIcon()
		{
			IntPtr hWnd = sGameManager.Handle;
			FLASHWINFO fInfo = new FLASHWINFO();

			fInfo.cbSize = Convert.ToUInt32(Marshal.SizeOf(fInfo));
			fInfo.hwnd = hWnd;
			fInfo.dwFlags = FLASHW_ALL | FLASHW_TIMERNOFG;
			fInfo.uCount = UInt32.MaxValue;
			fInfo.dwTimeout = 0;

			FlashWindowEx(ref fInfo);
		}

		private static void Init()
		{
			Logger.InitLog("GameManager", "gamemanager");
			Logger.Info("Starting GameManager PID {0}", Process.GetCurrentProcess().Id);
			Logger.Info("GameManager: CLR version {0}", Environment.Version);

			Application.ThreadException += delegate(Object obj, ThreadExceptionEventArgs evt)
			{
				Logger.Error("Unhandled Thread Exception:");
				Logger.Error(evt.Exception.ToString());

				MessageBox.Show("BlueStacks App Player.\nError: " + evt.Exception.Message);

				RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				String installDir = (string)reg.GetValue("InstallDir");
				if (Common.Strings.IsEngineLegacy())
					Utils.KillProcessByName("HD-Frontend");
				else
					Utils.KillProcessByName("HD-Plus-Frontend");

				UIHelper.RunOnUIThread(sGameManager, delegate() {
						sGameManager.Hide();
				});

				sGameManagerLock.Close();

				try
				{
					UploadCrashLogs(evt.Exception.ToString());
				}
				catch (Exception e)
				{
					Logger.Error(e.ToString());
				}

				Environment.Exit(1);
			};

			Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

			AppDomain.CurrentDomain.UnhandledException += delegate(Object obj, UnhandledExceptionEventArgs evt)
			{
				Logger.Error("Unhandled Application Exception:");
				Logger.Error(evt.ExceptionObject.ToString());

				MessageBox.Show("BlueStacks App Player.\nError: " + ((Exception)(evt.ExceptionObject)).Message);

				RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				String installDir = (string)reg.GetValue("InstallDir");
				if (Common.Strings.IsEngineLegacy())
					Utils.KillProcessByName("HD-Frontend");
				else
					Utils.KillProcessByName("HD-Plus-Frontend");

				UIHelper.RunOnUIThread(sGameManager, delegate() {
						sGameManager.Hide();
				});

				sGameManagerLock.Close();

				try
				{
					UploadCrashLogs(evt.ExceptionObject.ToString());
				}
				catch (Exception e)
				{
					Logger.Error(e.ToString());
				}

				Environment.Exit(1);
			};
		}

		private static void UploadCrashLogs(String errorMsg)
		{
			string url = String.Format("{0}/{1}", Service.Host, Common.Strings.GMCrashReportUrl);
			Dictionary<String, String> postData = new Dictionary<String, String>();
			postData.Add("error", errorMsg);
			Common.HTTP.Client.Post(url, postData, null, true);
		}

		private void FormMouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (!mFullScreen && e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}

		private void FormGradientPaint(object sender, PaintEventArgs e)
		{
			Logger.Info("In FormGradientPaint sTabBarheight=" + sTabBarHeight);
			if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0)
				return;

			int tabBarWidth = this.Width - mControlBarRight.mControlBarWidth;

			Rectangle rec = new Rectangle();
			rec.Location = new Point(mBorderWidth, mTabBarExtraHeight);
			rec.Width = tabBarWidth - 2 * mBorderWidth;
			rec.Height = sTabBarHeight + mTabBarExtraHeight;

			Rectangle topRect = new Rectangle();
			topRect.Location = new Point(mBorderWidth, mBorderWidth);
			topRect.Width = tabBarWidth - 2 * mBorderWidth;
			topRect.Height =  mTabBarExtraHeight; 
			using(SolidBrush brush = new SolidBrush(GMColors.TabBarGradientTop))
			{
				e.Graphics.FillRectangle(brush, topRect);
			}

			using (LinearGradientBrush gradBrush = new LinearGradientBrush(rec, GMColors.TabBarGradientTop, GMColors.TabBarGradientBottom, LinearGradientMode.Vertical))
			{
				e.Graphics.FillRectangle(gradBrush, mBorderWidth, (mTabBarExtraHeight == 0) ? mBorderWidth: mTabBarExtraHeight, tabBarWidth, sTabBarHeight + mTabBarExtraHeight);
			}
		}

		private void HandleLocationChanged(object sender, EventArgs e)
		{
			if (this.WindowState == FormWindowState.Minimized)
				return;

			if (mToolBarForm != null)
			{
				mToolBarForm.Left = this.Left - ToolBar.sToolBarWidth;
				mToolBarForm.Top = this.Top + sControlBarHeight + mBorderWidth + mTabBarExtraHeight;
			}

			if (this.Left <= 0 || this.Left >= GetSystemMetrics(SM_CXSCREEN) ||
					this.Top <= 0 || this.Top >= GetSystemMetrics(SM_CYSCREEN))
				return;

			SaveWindowLocation();
		}

		private void HandleActivatedEvent(Object o, EventArgs e)
		{
			Logger.Info("HandleActivatedEvent");
			if (mTabBar != null && mTabBar.FrontendVisible())
			{
				int res = Window.SendMessage(mFrontendHandle, Window.WM_USER_ACTIVATE, IntPtr.Zero, IntPtr.Zero);
				Logger.Info("WM_USER_ACTIVATE: " + res);
			}
		}

		private void HandleDeactivateEvent(Object o, EventArgs e)
		{
			Logger.Info("HandleDeactivateEvent");
			if (mTabBar != null && mTabBar.FrontendVisible())
			{
				int res = Window.SendMessage(mFrontendHandle, Window.WM_USER_DEACTIVATE, IntPtr.Zero, IntPtr.Zero);
				Logger.Info("WM_USER_DEACTIVATE: " + res);
			}
		}

		private void FormLoad(object sender, EventArgs e)
		{
			ShowHome();
			if (Oem.Instance.IsSendBTVFunnelStats)
			{
				Stats.SendBtvFunnelStats("saw_go_live_button", null, null, true);
				ShowStreamWindow();
			}
			if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
            {
                Thread td = new Thread(new ThreadStart(InitBossHotKey));
                td.Start();
            }
		}

		private void SetStreamWindowSettings()
		{
			mShowLogo = "null";
			mNumRecVideos = "null";
			mGoLiveEnabled = "null";

			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			mShowLogo = (string)configKey.GetValue("ShowLogo", mShowLogo);
			mNumRecVideos = (string)configKey.GetValue("NumRecVideos", mNumRecVideos);
			mGoLiveEnabled = (string)configKey.GetValue("GoLiveEnabled", mGoLiveEnabled);
		}

		private string GetStreamWindowUrl()
		{
			string url = sStreamWindowProdHtml;
			url = String.Format("{0}?logo={1}&videos={2}&streaming={3}",
					url, mShowLogo, mNumRecVideos, mGoLiveEnabled);
			return url;
		}

		private void AddTwitchWindow()
		{
			UIHelper.RunOnUIThread(this, delegate() {
				mTwitchWindow = new TwitchWindow();
				mTwitchWindow.Show();
				mTwitchWindow.Visible = false;
			});
		}

		private void AddStreamWindow()
		{
			SetStreamWindowSettings();
			string defaultUrl = GetStreamWindowUrl();
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string url = (string)configKey.GetValue("StreamWindowUrl", defaultUrl);
			Logger.Info("StreamWindow url: " + url);

			Logger.Info("Stream Window Default Params:");
			Logger.Info("Location: ({0}, {1})",
					sStreamWindowDefaultLocation.X,
					sStreamWindowDefaultLocation.Y);
			Logger.Info("Size: {0} x {1}, ChromeHeight = {2}",
					sStreamWindowDefaultContentSize.Width,
					sStreamWindowDefaultContentSize.Height,
					sStreamWindowDefaultChromeHeight);

			mStreamWindow = new StreamWindow(
					sStreamWindowDefaultLocation,
					sStreamWindowDefaultContentSize.Width,
					sStreamWindowDefaultContentSize.Height,
					sStreamWindowDefaultChromeHeight,
					url
					);
			Thread thr = new Thread(delegate() {
					UIHelper.RunOnUIThread(this, delegate() {
						
						mStreamWindow.Show();
						});
					mStreamManager = mStreamWindow.InitStreamManager();

					if (mStreamManager.mReplayBufferEnabled)
						mStreamManager.StartReplayBuffer();
					});
			thr.IsBackground = true;
			thr.Start();
		}

		private void SetDefaultStreamWindowParams()
		{
			/*
			 * gameUsable prefixed variables contain those values which would have
			 * been assigned to location and size variables if the screenwidth had
			 * been lesser than it actually is by amount sStreamWindowDefaultContentSize.Width
			 */

			int gameUsableScreenWidth  = GetSystemMetrics(SM_CXSCREEN);
			int screenWidth  = GetSystemMetrics(SM_CXSCREEN);
			int screenHeight  = GetSystemMetrics(SM_CYSCREEN);
			int gameUsableWidth, gameUsableHeight;

			Utils.GetWindowWidthAndHeight(gameUsableScreenWidth, screenHeight, out gameUsableWidth, out gameUsableHeight);
			Size gameUsableWindowSize = GetGMSizeGivenFESize(gameUsableWidth, gameUsableHeight);

			sScale = (gameUsableHeight * 1.0f / 540) * (96.0f / Utils.GetDPI());
			int streamWindowWidth = (int)Math.Ceiling(320 * (gameUsableHeight * 1.0f / 540));

			int retries = 5;
			while (((ToolBar.sToolBarWidth + gameUsableWindowSize.Width + streamWindowWidth) > screenWidth) &&
					retries > 0) {

				gameUsableScreenWidth = screenWidth - streamWindowWidth;
				Utils.GetWindowWidthAndHeight(gameUsableScreenWidth, screenHeight, out gameUsableWidth, out gameUsableHeight);
				gameUsableWindowSize = GetGMSizeGivenFESize(gameUsableWidth, gameUsableHeight);

				sScale = (gameUsableHeight * 1.0f / 540) * (96.0f / Utils.GetDPI());
				streamWindowWidth = (int)Math.Ceiling(320 * (gameUsableHeight * 1.0f / 540));
				retries--;
			}

			gameUsableScreenWidth = screenWidth - streamWindowWidth;
			sStreamWindowDefaultLocation.X = (gameUsableScreenWidth - gameUsableWindowSize.Width)/2 +
				gameUsableWindowSize.Width + ToolBar.sToolBarWidth/2;

			if (sStreamWindowDefaultLocation.X + streamWindowWidth > screenWidth)
				sStreamWindowDefaultLocation.X = screenWidth - streamWindowWidth;
			sStreamWindowDefaultLocation.Y = (screenHeight - gameUsableWindowSize.Height)/2;

			sStreamWindowDefaultContentSize.Width = streamWindowWidth;
			sStreamWindowDefaultContentSize.Height = gameUsableHeight;
			sStreamWindowDefaultChromeHeight = gameUsableWindowSize.Height - gameUsableHeight;
		}

		private void InitTabBar()
		{
			mTabBar = new TabBar(this);
			mTabBar.SizeMode = TabSizeMode.Fixed;
			mTabBar.ItemSize = new Size(mTabWidth, GetTabBarHeight() + 1 - mTabBarExtraHeight);  //+1 for the bottom darkline of the tabbar
			mTabBar.Visible = false;
			mTabBar.Location = new Point(1, 1 + mTabBarExtraHeight);
			this.Controls.Add(mTabBar);
		}

		private void InitControlBars(bool enableLeftButtons, bool enableRightButtons)
		{
			if (mControlBarLeft == null)
			{
				mControlBarLeft = new ControlBar(mTabBar);
			}
			if (mControlBarRight == null)
			{
				mControlBarRight = new ControlBar(mTabBar);
			}
			mControlBarLeft.IntializeLayout(this.Width, sControlBarHeight, ControlBar.CONTROL_BAR_POSITION_LEFT);
			mControlBarRight.IntializeLayout(this.Width, sControlBarHeight, ControlBar.CONTROL_BAR_POSITION_RIGHT);
			mControlBarLeft.Init();
			mControlBarRight.Init();

			if (!this.Controls.Contains(mControlBarLeft))
			{
				if (this.InvokeRequired)
				{
					this.Invoke((MethodInvoker)delegate {
							this.Controls.Add(mControlBarLeft);
							});
				}
				else
					this.Controls.Add(mControlBarLeft);
			}
			if (!this.Controls.Contains(mControlBarRight))
			{
				if (this.InvokeRequired)
				{
					this.Invoke((MethodInvoker)delegate {
							this.Controls.Add(mControlBarRight);
							});
				}
				else
					this.Controls.Add(mControlBarRight);
			}

			this.Controls.SetChildIndex(mControlBarLeft, 0);
			this.Controls.SetChildIndex(mControlBarRight, 0);

			if (enableLeftButtons)
			{
				mControlBarLeft.EnableAllButtons();
			}

			if (enableRightButtons)
			{
				mControlBarRight.EnableAllButtons();
			}
		}

		public void DisableBackButton()
		{
			mControlBarLeft.DisableBackButton();
		}

		public void EnableBackButton()
		{
			mControlBarLeft.EnableBackButton();
		}

		private void InitPrebundledAppsList()
		{
			mPrebundledApps = new List<String>();
			mPrebundledApps.Add(GAMEPOPHOME);
			mPrebundledApps.Add("com.android.settings");
			mPrebundledApps.Add("com.bluestacks.home");
			mPrebundledApps.Add(STOREPACKAGE);
			mPrebundledApps.Add("com.baidu.appsearch");
		}

		public void UpdateControlBar()
		{
			InitControlBars(true, true);
		}

		private void ShowHome()
		{
			UpdateLocalUrls();

			mTabBar.Visible = true;
			mControlBarLeft.EnableAllButtons();
			mControlBarRight.EnableAllButtons();

			RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
			String installDir = (string)reg.GetValue("InstallDir");
			String agentFile = Path.Combine(installDir, @"HD-Agent.exe");
			Process.Start(agentFile);
			reg.Close();

			Thread thr = new Thread(delegate() {
					Utils.StartHiddenFrontend(Common.Strings.VMName);
					});
			thr.IsBackground = true;
			thr.Start();
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

			AddHtmlTab();
			AddAndroidTab();

			if (Oem.Instance.IsShowBTVViewTab)
			{
				ShowViewTab();
			}
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);

			sShowFonelink = (int)configKey.GetValue("Fonelink", 0);
			if (sShowFonelink == 1)
			{
				sFoneLinkTab = AddFoneLinkTab();
			}

			if (!String.IsNullOrEmpty(mPackageName))
			{
				ShowApp(null, mPackageName, mActivityName, "", true);
			}
		}

		private void AddHtmlTab()
		{
			string name = sLocalizedString["Welcome"];

			RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string url = (string)urlKey.GetValue("gmHomeUrl", "");

			if (String.Compare(url, "") == 0)
			{
				url = (string)urlKey.GetValue("welcomePageUrl", "");
			}

			if (String.Compare(url, "") == 0)
			{
				url = Oem.Instance.HomeTabUrl;
			}

			Tab tab = mTabBar.AddWebTab(name, url, null, true);
			tab.mIsHome = true;
			mTabBar.mCurrentTab = tab;
		}

		public void ShowViewTab()
		{
			string name = "BlueStacks TV";
			string url = "http://bluestacks-tv-prod.appspot.com/web";

			int index;
			if (mTabBar.FindWebTab(name, out index))
			{
				mTabBar.GoToTab(index);
			}
			else
			{
				Tab tab = mTabBar.AddWebTab(name, url, null, false);
				tab.mIsHome = false;
			}
		}

		private void AddAndroidTab()
		{
			bool switchToTab = false;

			Tab tab = null;
			if (String.Compare(sHomeType, "gphome", true) == 0)
			{
				string name = sLocalizedString["Android"];
				string androidIconPath = Path.Combine(sAssetsDir, "android_icon.png");
				if (Oem.Instance.IsLoadBluestacksLogoForAndroidTab)
				{
					RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
					string installDir = (string)reg.GetValue("InstallDir");
					androidIconPath = Path.Combine(installDir, "BlueStacks.png");
				}
				tab = mTabBar.AddAppTab(name, mDefaultLauncher, ".Main",
						null, androidIconPath, switchToTab, true);
				mHomeTab = tab;

				tab.PerformTabAction(false, false);
			}
			else if (String.Compare(sHomeType, "html", true) == 0)
			{
				RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
				string url = (string)urlKey.GetValue("gmHomeUrl", "");

				if (String.Compare(url, "") == 0)
				{
					url = mHomeUrl;
				}
				tab = mTabBar.AddWebTab("Home", url, null, switchToTab);
			}
			else
			{
				MessageBox.Show("Invalid homeType. Aborting");
				Environment.Exit(-1);
			}

			tab.mIsHome = true;
			if (switchToTab)
				mTabBar.mCurrentTab = tab;

			ToggleControlBarButtons();
		}

		private Tab AddFoneLinkTab()
		{
			return mTabBar.AddWebTab(FONELINK_TAB_NAME, GetFoneLinkUrl(), null, false);
		}

		public void ToggleControlBarButtons()
		{
			Thread t = new Thread(delegate() {
					Thread.Sleep(1000);
					UIHelper.RunOnUIThread(this, delegate() {
						mControlBarRight.ToggleButtons();
					});
			});
			t.IsBackground = true;
			t.Start();
		}

		public void HandleMinimize()
		{
			this.WindowState = FormWindowState.Minimized;
		}

		public void HandleClose()
		{
			Logger.Info("User Closed the form");
			this.Close();
		}

		public void ToggleFullScreen()
		{
			Logger.Info("In ToggleFullScreen()");

			if (sStreaming)
			{
				Logger.Info("Ongoing stream. returning.");
				return;
			}

			mTabBar.Visible = false;
			if (mTabBar != null)
			{
				mTabBar.FullScreen = !mTabBar.FullScreen;
			}

			if (mFullScreen)
			{
				if (mTimerTopBarSlide != null)
				{
					MouseKeyboardHook.MouseAction -= MouseHook_MouseAction;
					mTimerTopBarSlide.Enabled = false;
				}
				Logger.Info("Going windowed");
				mFullScreen = false;
				ResizeGameManager();
                if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
                {
                    Tab tab = mTabBar.GetCurrentTab();
                    if (tab != null && !this.FullScreen)
                    {
                        if (tab.DeselectedGMClientSize != null)
                        {
                            ClientSize = tab.DeselectedGMClientSize.Value;
                        }
                    }
                }
			}
			else
			{
				Logger.Info("Going fullscreen");
				mFullScreen = true;
				if (mTimerTopBarSlide != null)
				{
					mTimerTopBarSlide.Enabled = true;
					MouseKeyboardHook.MouseAction += MouseHook_MouseAction;
				}
				ResizeGameManager();
			}
			mTabBar.Visible = true;

		}

		public void AppLaunched(String package, String activity, String callingPackage)
		{
			string activityName = activity;
			int pos = activity.IndexOf('/');
			if (pos != -1)
			{
				Logger.Info("AppLaunched: got invalid format for activity: {0}. Fixing it.", activity);
				activityName = activity.Substring(pos + 1);
			}

			if (mLastAppLaunched != package)
			{
				mLastAppLaunched = package;
				mLastAppDisplayed = "";
			}

			Thread thread = new Thread(delegate(){
					mDefaultLauncher = Utils.GetDefaultLauncher();
					Logger.Info("mDefaultLauncher: {0}", mDefaultLauncher);

					if (mDefaultLauncher != "none")
						mTabBar.mHomeUnresolved = false;
					else if (activity.Contains(RESOLVERACTIVITY))
					{
						mTabBar.mHomeUnresolved = true;
						if (mTabBar.GetCurrentTab().mIsHome == false &&
							mTabBar.GetCurrentTabType() == "app")
							GoToHomeTab();
					}

					RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
					configKey.SetValue("DefaultLauncher", mDefaultLauncher);
					configKey.Close();

					mHomeTab.mPackage = mDefaultLauncher;
					mHomeTab.Name = String.Format("app:{0}", mDefaultLauncher);
					});
			thread.IsBackground = true;
			thread.Start();
			/*
			 * If we don't wait for the thread to finish, the side effect could be that
			 * the first app that is launched might show in the same tab as home tab.
			 * This will not happen with our launcher but will happen only if the user
			 * changes the default launcher. However, if we wait for the thread to finish,
			 * the UI thread freezes for some time and the user is unable to launch an app
			 * for the first 4-5 seconds.
			 */
			// thread.Join();

			Logger.Info("AppLaunched: {0}/{1}", package, activity);
			Logger.Info("mLastTopPackage: {0}", mLastTopPackage);

			if (package == mDefaultLauncher)
			{
				UserAtHome();
			}

			/*
			 * Special handling for case where user goes to gp from an app other than the default
			 * launcher. This will happen in in-app-purchase scenarios where gp activity is started
			 * by the app itself. We don't want to switch tabs in this case.
			 */
			bool allowSwitch = true;
			if ((IsStorePackage(package) || IsIAPPackage(package))
					&& mLastTopPackage != mDefaultLauncher)
				allowSwitch = false;
			Tab selectedTab = (Tab)(mTabBar.SelectedTab);
			if (package == mDefaultLauncher && selectedTab.mTabType == "app" &&
					selectedTab.mIsHome == true)
			{
				selectedTab.mPackage = package;
				selectedTab.mActivity = activityName;
			}

			int index;
			if (mTabBar.FindAppTab(package, out index))
			{
				Logger.Info("Found tab. Index: " + index);
				Tab tab = (Tab)mTabBar.TabPages[index];
				if (package != mDefaultLauncher && allowSwitch)
					mTabBar.GoToTab(index);
                HandlePokemonGo(package);
                mTabBar.UpdateTab(index, activityName);
			}

			if (mLastTopPackage == package && selectedTab.mTabType == "app" &&
					selectedTab.mIsHome == false)
				return;
			if (package == KEYMAPPINGTOOL || package == APPFINDER || activity.Contains(RESOLVERACTIVITY))
				return;	
			if (package == mDefaultLauncher)
			{
				mLastTopPackage = package;
				return;
			}

			string appName = Utils.GetAppName(package);
			Logger.Info("Got appName: " + appName);

			/*
			 * Handling for case where user clicked on a not-installed app
			 * in the Welome tab
			 */
			if (package == STOREPACKAGE && mCreateVendingTab == true)
			{
				ShowApp(appName, package, activity, null, false);
				mCreateVendingTab = false;
				return;
			}

			if (mLastTopPackage == mDefaultLauncher || IsStorePackage(mLastTopPackage))
			{
				if (mTabBar.FindAppTab(package, out index))
				{
					if (index != mTabBar.SelectedIndex && allowSwitch)
					{
						Tab tab = (Tab)mTabBar.TabPages[index];
						tab.mLaunchApp = false;
						mTabBar.GoToTab(index);
					}
				}
				else
				{
					GMAppsManager iam = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
					bool isInstalled = iam.IsAppInstalled(package);
					if (callingPackage == mDefaultLauncher ||
							callingPackage == BSTCMDPROCESSOR ||
							callingPackage == BSTSERVICES ||
							IsStorePackage(callingPackage))
					{
						GMAppInfo info = new GMAppInfo(appName, null, package, activityName, "", "", "");
						if (!isInstalled)
							iam.AddToJson(info);

						ShowApp(appName, package, activity, null, false);
					}
					else
					{
						if (isInstalled)
							ShowApp(appName, package, activity, null, false);
					}
				}
			}
			/*
			 * handling for cases:
			 * - one time setup launches gp
			 * - user goes back in the activity stack after launching an app from gp
			 *   This second case will break because of the special case handling added above
			 *   Need to figure out a way to handle both cases.
			 */
			else if (IsStorePackage(package))
			{
				if (mTabBar.FindAppTab(package, out index))
				{
					if (index != mTabBar.SelectedIndex && allowSwitch)
					{
						Tab tab = (Tab)mTabBar.TabPages[index];
						tab.mLaunchApp = false;
						mTabBar.GoToTab(index);
					}
				}
				else
				{
					if (callingPackage == BSTCMDPROCESSOR || callingPackage == BSTSERVICES)
						ShowApp(appName, package, activity, null, false);
				}
			}

			mLastTopPackage = package;
		}
        private void HandlePokemonGo(string package)
        {
            if (package.ToLower().Equals("com.nianticlabs.pokemongo"))
            {
                RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
                int i = (int)key.GetValue("PokemonGuide", 0);
                if (i <= 1)
                {
                    int indx = 0;
                    if (!mTabBar.FindWebTab("Pokemon Go Guide", out indx))
                    {
                        mTabBar.AddWebTab("Pokemon Go Guide", "https://bluestacks.zendesk.com/hc/en-us/articles/211770923-How-to-play-Pokemon-Go-on-BlueStacks-", null, true);
                        key.SetValue("PokemonGuide", ++i, RegistryValueKind.DWord);
                    }
                }
            }
        }

        public void HandleAppDisplayed(string token)
		{
			Logger.Info("HandleAppDisplayed: {0}", token);

			try
			{
				String[] args = token.Split(' ');
				string appInfo = args[3];

				if (appInfo.Contains(mLastAppLaunched))
					mLastAppDisplayed = appInfo;

				Tab selectedTab = (Tab)(mTabBar.SelectedTab);
				string package = selectedTab.mPackage;
				if (mLastShownAppInfo != "" && mLastShownAppInfo.Contains(package) &&
						mLastShownAppInfo.IndexOf(PLAYSTOREAPPINFO) == -1)
				{
					Logger.Info("Not sending AppDisplayed request for last shown appInfo: " + appInfo);
					return;
				}

				int pos = token.IndexOf(' ');
				string appToken = token.Substring(pos + 1);
				UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {
						bool shown = AppDisplayed(appToken);
						
						if (shown)
						{
							mLastShownAppInfo = appInfo;
						}
				});
			}
			catch (Exception exc)
			{
				Logger.Error("GameManager: HandleAppDisplayed: " + exc.ToString());
			}
		}

		public bool AppDisplayed(String appToken)
		{
			try
			{
				Logger.Info("AppDisplayed: {0}", appToken);
				Tab selectedTab = (Tab)(mTabBar.SelectedTab);
				string toSearch = String.Format("{0}/", selectedTab.mPackage);
				Logger.Info("toSearch: {0}", toSearch);

				if (mDefaultLauncher == "none" && appToken.Contains(RESOLVERACTIVITY))
				{
					mTabBar.mHomeUnresolved = true;
					if (selectedTab.mTabType == "app")
					{
						selectedTab.PerformTabAction(false, true);
						selectedTab.mRunAppRequestPending = false;
						return true;
					}
				}

				if (selectedTab.mTabType == "app")
				{
					if (selectedTab.mIsHome == true &&
							selectedTab.mTabType == "app" &&
							appToken.Contains(RESOLVERACTIVITY))
					{
						selectedTab.PerformTabAction(false, true);
						selectedTab.mRunAppRequestPending = false;
						return true;
					}

					Logger.Info("mDefaultLauncher: " + mDefaultLauncher);
					if (selectedTab.mIsHome == true &&
							selectedTab.mTabType == "app" &&
							(selectedTab.mPackage == GAMEPOPHOME ||
							 selectedTab.mPackage == mDefaultLauncher) &&
							(appToken.Contains(GAMEPOPHOME) ||
							 appToken.Contains(mDefaultLauncher)))
					{
						selectedTab.PerformTabAction(false, true);
						selectedTab.mRunAppRequestPending = false;
						selectedTab.TakeScreenshot();
						return true;
					}

					if (appToken.Contains(toSearch)||
							appToken.IndexOf(PLAYSTOREAPPINFO) != -1 ||
							appToken.IndexOf(PLAYLOGINAPPINFO) != -1 ||
							appToken.IndexOf(PLAYLOCATIONINFO) != -1)
					{
						selectedTab.PerformTabAction(false, true);
						selectedTab.mRunAppRequestPending = false;
						return true;
					}
				}

			}
			catch (Exception e)
			{
				// A very small window, but if the tab was closed after getting the
				// selected tab from tabBar and before doing further operations, this will happen.
				// No way to prevent it, so ignore it...
				Logger.Error("Failed in AppDisplayed. Ignoring. Error: " + e.ToString());
			}

			return false;
		}

		public void AppUninstalled(String package)
		{
			Logger.Info("AppUninstalled: {0}", package);
			mTabBar.CloseAppTab(package);
		}

		public void RelaunchApp(String displayName, String package, String activity, String apkUrl)
		{
			mTabBar.CloseCurrentTab();
			ShowApp(displayName, package, activity, apkUrl, true);
		}

		public void ShowApp(String displayName, String package, String activity, String apkUrl, bool launchApp)
		{
			Logger.Info("ShowApp: " + package + "/" + activity + "/" + apkUrl);
			int index;
			if (mTabBar.FindAppTab(package, out index))
			{
				mTabBar.GoToTab(index);
			}
			else
			{
				if (mDontCreateAppTab == true)
				{
					mDontCreateAppTab = false;
					if (package.Contains("search::"))
						SendSearchPlayRequestAsync(package, activity);
					else
						SendRunAppRequestAsync(package, activity);
					return;
				}

				if (package == mDefaultLauncher)
					return;

				String imageFile = Path.Combine(Common.Strings.GameManagerHomeDir, package + ".png");
				if (!File.Exists(imageFile))
					imageFile = null;
				mTabBar.AddAppTab(displayName, package, activity, apkUrl, imageFile, true, launchApp);
				mTabBar.Focus();
			}
		}

		public void ShowWebPage(String label, String url, String imagePath)
		{
			Logger.Info("ShowWebPage: " + label + "/" + url);
			int index;
			if (mTabBar.FindWebTab(label, out index))
			{
				mTabBar.GoToTab(index);
			}
			else
			{
				mTabBar.AddWebTab(label, url, imagePath, true);
			}
		}

		private void SendRunAppRequestAsync(string package, string activity)
		{
			Thread thread = new Thread(delegate()
					{
					string cmd = String.Format("runex {0}/{1}", package, activity);
					Common.VmCmdHandler.RunCommand(cmd);
					});

			thread.IsBackground = true;
			thread.Start();
		}

		private void SendSearchPlayRequestAsync(string package, string activity)
		{
			Thread thread = new Thread(delegate()
					{
					if (package.Contains("search::"))
					{
					package = package.Remove(0, 8);
					}
					string cmd = String.Format("searchplay {0}", package);
					Logger.Info("Package: " + package + "Activity: " + activity);
					Common.VmCmdHandler.RunCommand(cmd);
					});

			thread.IsBackground = true;
			thread.Start();
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case Window.WM_USER_SHOW_WINDOW:
					Logger.Info("Received message WM_USER_SHOW_WINDOW");
					this.ShowWindow();
					m.Result = new System.IntPtr(1);
					break;

				case WM_QUERYENDSESSION:
					Logger.Info("Received message WM_QUERYENDSESSION");
					mSessionEnding = true;
					break;

				case Window.WM_NCHITTEST:
//					Logger.Info("Received message WM_NCHITTEST");
					Point pos = new Point(m.LParam.ToInt32());
					pos = this.PointToClient(pos);
					int result = 0;

					if (mFullScreen)
					{
						Logger.Info("Full Screen");
						Logger.Info("Setting Resize Btn visible: false");
						ResizeGameManagerBtn.Visible = false;
						// We should not add resizing support in fullscreen mode
						break;
					}

					if (sStreaming)
					{
						Logger.Info("Streaming enabled");
						Logger.Info("Setting Resize Btn visible: false");
						ResizeGameManagerBtn.Visible = false;
						// We will not allow resizing when streaming
						break;
					}

					if (pos.Y <= mGrip)
					{
						//if (pos.X <= mGrip)
						//    result = 13;				// HTTOPLEFT
						//else if (pos.X >= this.ClientSize.Width - mGrip)
						//    result = 14;				// HTTOPRIGHT
						//else
						//    result = 12;				// HTTOP;
					}
					else if (pos.Y >= this.ClientSize.Height - mGrip)
					{
						if (pos.X <= mGrip)
						{
							// No support for resizing from the bottom-left corner
							// result = 16;				// HTBOTTOMLEFT
						}
						else if (pos.X >= this.ClientSize.Width - mGrip)
							result = 17;				// HTBOTTOMRIGHT
						//else
						//result = 15;				// HTBOTTOM
					}
					else if (pos.X <= mGrip)
					{
						// No support for resizing from the left border
						// result = 10;					// HTLEFT
					}
					else if (pos.X >= this.ClientSize.Width - mGrip)
					{
						//result = 11;					// HTRIGHT
					}

					if (result != 0)
					{
						m.Result = (IntPtr)result;
						return;
					}

					break;
                case Window.WM_USER_FE_ORIENTATION_CHANGE:
                    if (Features.IsFeatureEnabled(Features.IS_CHINA_UI) && this.mFrontendHandle != IntPtr.Zero)
                    {
                        if ((int)m.WParam ==1)
                        {
                            if (!this.FullScreen)
                            {

                                if (sFrontendWidth > sFrontendHeight)
                                {
                                    Size s = this.GetFESizeVerticalBySreen();
                                    this.ClientSize = GetGMSizeGivenFESize(s.Width, s.Height);
                                }
                                else
                                {
                                    Size s = this.GetFESizeHorizontalBySreen();
                                    this.ClientSize = GetGMSizeGivenFESize(sFrontendHeight, sFrontendWidth);
                                }
                            }
                        }
                        else
                        {
                            if (!this.FullScreen)
                            {
                                //if (sFrontendWidth > sFrontendHeight)
                                {
                                    this.ClientSize = GetGMSizeGivenFESize(sFrontendWidth, sFrontendHeight);
                                }
                            }
                        }
                    }
                    break;
				default:
					break;
			}

			base.WndProc(ref m);
		}

		public void ShowWindow()
		{
			this.Show();
			this.WindowState = FormWindowState.Normal;

			if (mToolBarForm != null)
			{
				ToolBar.sMakeVisible = true;
				mToolBarForm.Show();
			}
		}

		public void GoToHomeTab()
		{
			Logger.Info("Going to home");
			mTabBar.GoToTab(mHomeTab);
		}

		public void CloseCurrentTab()
		{
			mTabBar.CloseCurrentTab();
		}

		private void RememberNotificationChoice(int enableNotification)
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(@"Software\BlueStacksGameManager\Config");
			key.SetValue("NotificationEnabled", enableNotification);
			key.Close();
		}

		// should be called CloseGameManager
		private void CloseFrontend(int notificationEnabled)
		{
			if (notificationEnabled == 1)
			{
				ToolBar.sMakeVisible = false;
				if (mToolBarForm != null)
				{
					mToolBarForm.Hide();
				}

				this.Hide();
				mTabBar.mLastAppTabName = "";
				VmCmdHandler.RunCommand("home"); 
				for (int i = 1; i < mTabBar.TabPages.Count; i++)
				{
					mTabBar.CloseTab(i);
				}
			}
			else
			{
				FoneLinkNotifications.Shutdown();
				this.mSessionEnding = true;
				this.Hide();
				sServer.Stop();
				for (int i = 1; i < mTabBar.TabPages.Count; i++)
				{
					mTabBar.CloseTab(i);
				}

				if (mStreamWindow != null)
				{
					mStreamWindow.DisposeBrowser();
				}

				if (mTwitchWindow != null)
				{
					mTwitchWindow.Close();
				}

				try
				{
					Gecko.Xpcom.Shutdown();
				}
				catch (Exception e)
				{
					Logger.Error("Xpcom shutdown failed: " + e.ToString());
				}

				sGameManagerLock.Close();

				if (sStreaming)
					SendBtvFunnelStats(GmClose);
				else
					GmClose();
			}
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			Logger.Info("In OnFormClosing. mSessionEnding = " + mSessionEnding);
			if (mSessionEnding == true)
			{
				Environment.Exit(1);
			}
			
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\BlueStacksGameManager\Config");
			int notificationEnabled = (int)key.GetValue("NotificationEnabled", -1);

			if (notificationEnabled == -1 && !sForceClose)
			{
				DialogResult restart = CustomMessageBox.ShowMessageBox(
					"BlueStacks",
					sLocalizedString["ClosePopupText"],
					sLocalizedString["YesText"],
					sLocalizedString["NoText"],
					sLocalizedString["CancelText"],
					sLocalizedString["RememberChoiceText"],
					null);

				if (restart == DialogResult.Yes)
				{
					ToolBar.sMakeVisible = false;

					if (mToolBarForm != null)
						mToolBarForm.Hide();

					if (mStreamWindow != null)
						mStreamWindow.Hide();

					if (rememberChoice)
						this.RememberNotificationChoice(1);

					this.CloseFrontend(1);
				}
				else if (restart == DialogResult.No)
				{
					ToolBar.sMakeVisible = false;

					if (mToolBarForm != null)
						mToolBarForm.Hide();

					if (mStreamWindow != null)
						mStreamWindow.Hide();

					if (rememberChoice)
						this.RememberNotificationChoice(0);

					this.CloseFrontend(0);
				}
				else
				{
					Logger.Info("got DialogResult.Cancel, hence do not exit");
				}
			}
			else if (sForceClose == true)
			{
				this.CloseFrontend(0);
			}
			else
			{
				if (notificationEnabled == 1)
					this.CloseFrontend(1);
				else if (notificationEnabled == 0)
					this.CloseFrontend(0);
			}

			if (e != null)
			{
				e.Cancel = true;
			}
		}
		
		public static void NoUpdatesAvailable()
		{
			Logger.Info("No updates available");

			String capt = "BlueStacks Updater";
			String text = "No new updates available";

			try 
			{
				capt = sLocalizedString["UpdateProgressTitleText"];
				text = sLocalizedString["UPDATER_UTILITY_NO_UPDATE_TEXT"];
			}
			catch
			{
				//Logger.Info("locale string for no update box not found");
			}
				UIHelper.RunOnUIThread(sGameManager, delegate() {
					MessageBox.Show(text, capt, MessageBoxButtons.OK);
					});
		}

		public static void UpdateAvailable()
		{
			String capt = "BlueStacks Updater";
			String text = "Update is available. Do you want to install now?";

			try 
			{
				capt = sLocalizedString["UpdateProgressTitleText"];
				text = sLocalizedString["UpdateAvailableText"];
			}
			catch
			{
				//Logger.Info("locale string for no update box not found");
			}

			UIHelper.RunOnUIThread(sGameManager, delegate() {
				DialogResult result = MessageBox.Show(text, 
						capt,
						MessageBoxButtons.YesNo);
				if (result == DialogResult.Yes)
				{
					RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
					string installDir = (string)reg.GetValue("InstallDir");
					ProcessStartInfo proc = new ProcessStartInfo();
					proc.FileName = installDir + "HD-UpdateHelper.exe";
					Process.Start(proc);
					HTTPHandler.StartUpdateRequest(null, "/installupdate");
				}
			});
		}

		private void GmClose()
		{
			Logger.Info("GmClose()");
			CloseFrontend();
			Utils.KillProcessByName("HD-OBS");
			Logger.Info("Exiting");
			Environment.Exit(0);
		}

		private void CloseFrontend()
		{
			if (Utils.IsProcessAlive(Common.Strings.FrontendLockName) == false)
			{
				Logger.Info("Frontend not running");
				return;
			}

			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				int port = (int)key.GetValue("FrontendServerPort");
				Logger.Info("sending closescreen request to frontend");
				string url = String.Format("http://127.0.0.1:{0}/closescreen", port);
				Common.HTTP.Client.Post(url, null, null, false);
			}
			catch (Exception e)
			{
				Logger.Error("Exception in CloseFrontend()");
				Logger.Error(e.ToString());
			}
		}

		public bool IsAppInstalled(string package)
		{
			GMAppsManager iam = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
			bool isInstalled = mPrebundledApps.Contains(package) || iam.IsAppInstalled(package);
			Logger.Info("IsAppInstalled({0}): {1}", package, isInstalled);
			return isInstalled;
		}

		public delegate void ActionAfterStatusUpdate();
		public static void SendBtvFunnelStats(ActionAfterStatusUpdate action)
		{
			Thread thr = new Thread(delegate() {
					try
					{
						Stats.SendBtvFunnelStatsSync("stream_ended",
							"stream_ended_reason",
							"app_player_closed",
							false);

						if (action != null)
							action();
					}
					catch(Exception e)
					{
						Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
					}
					});
			thr.IsBackground = true;
			thr.Start();
		}

		public static string GetFoneLinkUrl()
		{
			return GameManager.FONELINK_URL + "?app_player_guid=" + User.GUID;
		}

        public void Restart()
        {
            string file = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName() + ".bat");
            string batScript = "\r\n";
            {

                RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
                String installDir = (string)reg.GetValue("InstallDir");
                batScript +=  string.Format("\"{0}\"\r\n", Path.Combine(installDir, "HD-Quit.exe"));
                batScript += string.Format("\"{0}\"\r\n", Application.ExecutablePath);
                batScript += string.Format("del \"{0}\" /Q\r\n", file);
            }

            System.IO.File.WriteAllText(file, batScript);

            Utils.RunCmdAsync(file, "");
            System.Environment.Exit(0);

            
        }

		private void ResizeButtonMouseEnter(object sender, System.EventArgs e)
		{
			Logger.Info("Resize Btn MouseEnter");
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Cursor = Cursors.Hand;
				button.Image = mResizeBtnImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ResizeButtonMouseLeave(object sender, System.EventArgs e)
		{
			Logger.Info("Resize Btn MouseLeave");
			allowResize = false;
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Cursor = Cursors.Default;
				button.Image = mResizeBtnImagesDict[(String)button.Tag];
			}
		}
	}
	public static class MouseKeyboardHook
	{
		public static event EventHandler MouseAction = delegate { };

		static bool _isBelowThisPoint = true;
		static int _point = 0;
		public static void StartMouseHook(int point, bool isBelowThisPoint)
		{
			_isBelowThisPoint = isBelowThisPoint;
			_point = point;
			_hookID = SetHook(_procMouse);
		}
		public static void StartKeyBoardHook()
		{
			_keyBoardHookID = SetHook(_procKeyBoard);
		}
		public static void Stop()
		{
			UnhookWindowsHookEx(_hookID);
		}

		private static LowLevelMouseProc _procMouse = HookCallback;
		private static LowLevelKeyboardProc _procKeyBoard = HookCallback;
		private static IntPtr _hookID = IntPtr.Zero;
		private static IntPtr _keyBoardHookID = IntPtr.Zero;

		private static IntPtr SetHook(LowLevelMouseProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_MOUSE_LL, proc,
				  GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private static IntPtr SetHook(LowLevelKeyboardProc proc)
		{
			using (Process curProcess = Process.GetCurrentProcess())
			using (ProcessModule curModule = curProcess.MainModule)
			{
				return SetWindowsHookEx(WH_KEYBOARD_LL, proc,
				  GetModuleHandle(curModule.ModuleName), 0);
			}
		}

		private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);
		private delegate IntPtr LowLevelKeyboardProc(int nCode, IntPtr wParam, IntPtr lParam);
		private static IntPtr HookCallback(
		  int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (ApplicationIsActivated())
			{
				//Mouse hook
				if (nCode >= 0 && MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam)
				{
					MSLLHOOKSTRUCT hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));

					if (_isBelowThisPoint)
					{
						if (hookStruct.pt.y >= _point)
						{
							Stop();
							MouseAction(null, new EventArgs());
						}
					}
					else
					{
						if (hookStruct.pt.y <= _point)
						{
							Stop();
							MouseAction(null, new EventArgs());
						}
					}
				}
				//KeyBoard hook

				if (nCode >= 0 && wParam == (IntPtr)WM_KEYDOWN)
				{
					int vkCode = Marshal.ReadInt32(lParam);
					if (vkCode.ToString() == "122")
					{
						Console.WriteLine("Aman");
						Logger.Info("GameManager KeyBoardHook : got f11");
						GameManager.sGameManager.ToggleFullScreen();
					}
				}
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);

		public static bool ApplicationIsActivated()
		{
			var activatedHandle = GetForegroundWindow();
			if (activatedHandle == IntPtr.Zero)
			{
				return false;       // No window is currently activated
			}

			var procId = Process.GetCurrentProcess().Id;
			int activeProcId;
			GetWindowThreadProcessId(activatedHandle, out activeProcId);

			return activeProcId == procId;
		}

		private const int WH_MOUSE_LL = 14;
		private const int WH_KEYBOARD_LL = 13;

		const int WM_KEYDOWN = 0x100;

		private enum MouseMessages
		{
			WM_LBUTTONDOWN = 0x0201,
			WM_LBUTTONUP = 0x0202,
			WM_MOUSEMOVE = 0x0200,
			WM_MOUSEWHEEL = 0x020A,
			WM_RBUTTONDOWN = 0x0204,
			WM_RBUTTONUP = 0x0205
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MSLLHOOKSTRUCT
		{
			public POINT pt;
			public uint mouseData;
			public uint flags;
			public uint time;
			public IntPtr dwExtraInfo;
		}

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook,
		  LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);
		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook,
		  LowLevelKeyboardProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
		  IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);


	}
}
