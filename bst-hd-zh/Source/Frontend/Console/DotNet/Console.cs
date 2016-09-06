/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Console Frontend
 */

using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Reflection;
using System.Security.Cryptography;
using System.Security.Permissions;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Windows.Forms;
using System.ServiceProcess;
using System.Runtime.InteropServices;
using System.Globalization;
using Microsoft.Samples.TabletPC.MTScratchpad.WMTouch;
using Microsoft.Win32;
using Microsoft.VisualBasic.FileIO;
using CodeTitans.JSon;

using BlueStacks.hyperDroid.Core.VMCommand;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using VideoCapture = BlueStacks.hyperDroid.VideoCapture;
using WindowInterop = BlueStacks.hyperDroid.Common.Interop.Window;

namespace BlueStacks.hyperDroid.Frontend
{

	/*
     * Ideally this should be in the Console class scope, but C# only supports type
     * aliases in namespace scope.
     */

	public class Console : WMTouchUserControl
	{


		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetProcessDPIAware();

		[DllImport("kernel32.dll")]
		static extern void OutputDebugString(string lpOutputString);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SystemParametersInfo(int uiAction, int uiParam, ref uint pvParam, int fWinIni);

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SystemParametersInfo(int uiAction, int uiParam, uint pvParam, int fWinIni);

		[DllImport("user32.dll", ExactSpelling = true, CharSet = CharSet.Auto)]
		public static extern IntPtr GetParent(IntPtr hWnd);

		private String REG_CONFIG = Common.Strings.HKLMAndroidConfigRegKeyPath;

		public class Win32
		{
			[DllImport("User32.Dll")]
			public static extern long SetCursorPos(int x, int y);

			[DllImport("User32.Dll")]
			public static extern bool ClientToScreen(IntPtr hWnd, ref POINT point);

			[StructLayout(LayoutKind.Sequential)]
			public struct POINT
			{
				public int x;
				public int y;
			}
		}
		public bool IsMouseDownHandled = false;
		private static string dateFormat = "yyyy-MM-dd HH:mm";

		/*
         * Frontend Window Sizing
         */

		private Size mConfiguredDisplaySize;
		private Size mConfiguredGuestSize;
		private Size mCurrentDisplaySize;
		private Rectangle mScaledDisplayArea;
		public bool mEmulatedPortraitMode = false;
		private bool mRotateGuest180 = false;
		internal bool mFullScreen = true;
		private bool originalFullScreenState = false;
		private FullScreenToast mFullScreenToast;
		/*
         * Shoot Mode Handling
         */

		public static bool mDisableMouseMovement = false;
		private bool mBurstModeOn = false;
		private Object mShootLockObject = new Object();

		public static bool mShootAttackClick = false;
		private static int sMouseGuestX = 0;
		private static int sMouseGuestY = 0;
		private static int sOriginGuestX = 0;
		private static int sOriginGuestY = 0;
		private static Point mScreenPos = new Point();
		private static Point mMouseMovePos = new Point();
		private static Point mMoveOriginPos = new Point();
		private static Point mShootOriginPos = new Point();
		private static bool mDisableShootMouse = true;
		private static bool mShootMouseSwitch = false;
		private static bool mShootMouseDown = false;
		public static bool restartFrontend = false;
		private static string sArgs = "";

		/*
         * Touch Handling
         * Windows send random mouse move event after tap/touch
         * within 1 sec therefore ignoring mouse move events
         * within 1 sec of touch event.
         */

		private static int sLastTouchTime = 0;
		/*
         * Left Mouse Button Handling
         */
		private static bool sMouseDown = false;

		/*
         * PCIME 
         */
		private IntPtr m_hImc;
		private static WindowInterop.COMPOSITIONFORM Composition;
		private bool mIsTextInputBoxInFocus = false;
		private static bool isUsePcImeWorkflow = false;

		/*
         * MUTE/UNMUTE global state
         */
		private bool mMute = false;
		/*
         * MultiIntance
         */

		private bool otherInstanceServiceStopDone = false;

		private const int VM_EVENT_PORT = 9998;


		private StateMachine mStateMachine;

		public const int GUEST_ABS_MAX_X = 0x8000;
		public const int GUEST_ABS_MAX_Y = 0x8000;

		/* SHOOTING_MODE_1 GAME:default 	(Not rectangle, multi touch)
         * SHOOTING_MODE_2 GAME:QMQZ  		(Not rectangle, single touch)
         * SHOOTING_MODE_3 GAME:Tencent CF  (Rectangle,single touch)
         */
		public const int SHOOTING_MODE_1 = 1;
		public const int SHOOTING_MODE_2 = 2;
		public const int SHOOTING_MODE_3 = 3;

		public const uint NONE_DIRECTION_BIT = 0;
		public const uint UP_DIRECTION_BIT = 0x1;
		public const uint LEFT_DIRECTION_BIT = 0x2;
		public const uint DOWN_DIRECTION_BIT = 0x4;
		public const uint RIGHT_DIRECTION_BIT = 0x8;
		public const uint SPEED_UP_MODIFIER_BIT = 0x10;
		private uint mPokemonDirectionValue = 0x0;

		public const int TOUCH_POINTS_MAX = 16;
		private const int SWIPE_TOUCH_POINTS_MAX = 1;


		private String vmName;
		private EventWaitHandle glReadyEvent;   /* prevent GC */

		private DateTime mLastFrontendStatusUpdateTime;
		private Keyboard keyboard;
		private Mouse mouse;
		private BstCursor mCursor;
		public InputMapper mInputMapper;
		private Gps.Manager mGps;
		private OpenSensor mOpenSensor;
		private GamePad mGamePad;
		private Interop.Monitor.TouchPoint[] touchPoints;

		private SensorDevice mSensorDevice = new SensorDevice();

		private Dictionary<int, String> mControllerMap;

		private const int WM_QUERYENDSESSION = 0x11;
		private const int WM_SYSCOMMAND = 0x112;

		private const int SC_MAXIMIZE = 0xF030;
		private const int SC_MAXIMIZE2 = 0xF032;
		private const int SC_RESTORE = 0xF120;
		private const int SC_RESTORE2 = 0xF122;
		private System.Windows.Forms.Timer afterSleepTimer;
		private System.Windows.Forms.Timer triggerMemoryTrimTimer;

		private bool cannotStartVm = false;
		private bool guestFinishedBooting = false;
		private bool isGuestReady = false;
		private bool appLaunched = false;
		private bool glInitFailed = false;

		private System.Windows.Forms.Timer timer;

		private bool grabKeyboard = true;
		private bool frontendNoClose = false;
		private bool stopZygoteOnClose = false;
		private bool isMinimized = false;

		private static bool sFrontendActive = true;
		private bool frontendReadyActionDone = false;

		private const long LWIN_TIMEOUT_TICKS = 100 * 10000;
		private long lastLWinTimestamp = 0;

		public static String sInstallDir;

		private LoadingScreen loadingScreen = null;
		private bool atLoadingScreen = false;

		private Toast snapshotErrorToast = null;
		private bool snapshotErrorShown = false;

		private bool lockdownDisabled = false;

		private bool disableDwm = false;
		private bool userInteracted = false;
		private bool frontendMinimized = false;
		private bool sessionEnding = false;
		private bool checkingIfBooted = false;
		private bool checkingIfGuestReady = false;

		private SynchronizationContext mUiContext;

		public static Console s_Console;
		public static bool s_UserKeyMappingEnabled = true;
		public static bool s_AutoKeyMappingEnabled = true;

		public static float s_ShootOriginXPos = 0;
		public static float s_ShootOriginYPos = 0;
		public static int s_ShootTriggerXPos = 0;
		public static int s_ShootTriggerYPos = 0;
		public static int s_ShootSensitivity = 6;
		public static bool s_IsSpaceShooterModeEnabled = false;
		public static bool s_IsSingleTouchShootModeEnabled = false;
		public static int s_touchShootModeType = 0;
		public static int s_ClipRectangleX = 55;
		public static int s_ClipRectangleY = 10;
		public static int s_ClipRectangleWidth = 40;
		public static int s_ClipRectangleHeight = 80;
		public static uint sOriginalMouseSpeed = 10;
		private const int SPI_GETMOUSESPEED = 0x70;
		private const int SPI_SETMOUSESPEED = 0x71;
		public static string sVName = "Android";

		private static DateTime sFrontendLaunchTime;

		private static bool s_KeyMapTeachMode = false;

		private static ToolTip s_KeyMapToolTip = new ToolTip();

		private String mCurrentAppPackage;
		private String mCurrentAppActivity;

		private static Queue<Char> ImeCharsQueue = new Queue<Char>();
		private Object imeLockObject = new Object();
		private Object lockObject = new Object();
		private Object sendkeyslock = new Object();
		public static Object sCurrentAppDisplayedLockObject = new Object();

		private enum GlWindowAction
		{
			None,
			Show,
			Hide,
		};

		/*
         * events for sending message from frontend to window
         */

		public event EventHandler FrontendClose;

		protected virtual void OnFrontendClose()
		{
			if (FrontendClose != null)
			{
				EventArgs e = new EventArgs();
				FrontendClose(this, e);
			}
		}

		private GlWindowAction glWindowAction = GlWindowAction.None;

		private static string sAppName = "";
		private static Image sAppIconImage;
		public static string sAppPackage = "";
		private static string sAppIconName = "";
		private static int sCurrentOrientation = 0;

		private static string sDriverUpdateUrl = "";

		private static bool sAppLaunch = false;

		private static void SetInstallDir()
		{
			String path = Common.Strings.RegBasePath;
			RegistryKey key;

			using (key = Registry.LocalMachine.OpenSubKey(path))
			{
				sInstallDir = (String)key.GetValue("InstallDir");
			}
		}
		private VideoCapture.Manager camManager;

		public static bool sAppPackageInfoSent = true;
		public static bool sAppLaunchedFromRunApp = false;
		public static bool sHideMode = false;
		public static bool sPgaInitDone = false;
		public static bool sS2PScreenShown = false;

		public static string sLastAppDisplayed = "";
		public static bool sSendTopAppChangeEvents = false;


		public bool guestHasStopped = false;
		public bool forceVideoModeChange = false;

		private static bool sReLayoutInProgress = false;

		private static Dictionary<Keys, int> sKeyStateSet = new Dictionary<Keys, int>();

		public bool AppLaunched
		{
			get
			{
				return appLaunched;
			}
		}

		public static bool IsFrontendActive
		{
			get
			{
				return sFrontendActive;
			}
		}

		public State FrontendState
		{
			get
			{
				return mStateMachine.CurrentState;
			}
			set
			{
				mStateMachine.CurrentState = value;
			}
		}
		private static Dictionary<String, String[]> sOemWindowMapper;

		public static Dictionary<String, String[]> OemWindowMapper
		{
			get
			{
				if (sOemWindowMapper == null)
				{
					Utils.AddMessagingSupport(out sOemWindowMapper);

				}
				return sOemWindowMapper;
			}

		}

		public void UpdateStateToParentWindow()
		{
			if (OemWindowMapper.ContainsKey(Common.Oem.Instance.OEM))
			{
				WindowMessages.NotifyStateToParentWindow(FrontendState);
			}
		}

		public void ChangeImeMode(bool enableIme)
		{
			Logger.Info("change enable ime called");
			if (enableIme && Utils.IsLatinImeSelected())
			{
				Logger.Info("ime mode enabled");
				UIHelper.RunOnUIThread(s_Console, delegate ()
				{
					ImeMode = ImeMode.On;
					mIsTextInputBoxInFocus = true;
				});
			}
			else
			{
				Logger.Info("ime mode disabled");
				UIHelper.RunOnUIThread(s_Console, delegate ()
				{
					this.ImeMode = ImeMode.Disable;
					mIsTextInputBoxInFocus = false;
				});
			}
		}

		public void ExecuteExe(string path, string args)
		{
			Process proc = new Process();
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.FileName = path;
			proc.StartInfo.Arguments = args;

			Logger.Info("Running: {0} {1}", path, args);

			proc.Start();
			proc.WaitForExit();
		}

		/*
           ---- curl localhost:9999/customstartactivity -d component="com.bluestacks.keymappingtool/.BlueStacksKeyMapper" -d extras="{"foo":"bar"}"

           ---- curl localhost:9999/customstartactivity -d component="com.bluestacks.keymappingtool/.BlueStacksKeyMapper" -d extras=""

           ---- curl localhost:9999/customstartactivity -d component="com.bluestacks.keymappingtool/.BlueStacksKeyMapper" 

         */
		/*
           ---- curl localhost:9999/customstartservice -d component="com.bluestacks.keymappingtool/.ServiceKeyMapper" -d extras="{"foo":"bar"}"

           ---- curl localhost:9999/customstartservice -d component="com.bluestacks.keymappingtool/.ServiceKeyMapper" -d extras=""

           ---- curl localhost:9999/customstartservice -d component="com.bluestacks.keymappingtool/.ServiceKeyMapper"

           9999 is the default port. (We should retrieve it from config regkey under the name bstandroidport)

        */
		public void LaunchBlueStacksKeyMapper()
		{
			try
			{
				int port = Utils.GetBstCommandProcessorPort(Common.Strings.VMName);

				string customService = "customstartservice"; //To launch a custom Service
															 // string customActivity = "customstartactivity"; //To launch a custom Activity
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("action", "com.bluestacks.keymappingtool.BLUESTACKS_KEYMAPPER");

				String url = String.Format("http://127.0.0.1:{0}/{1}", port, customService);
				Logger.Info("the url being hit is {0}", url);

				string res = Common.HTTP.Client.PostWithRetries(url, data, null, false, 10, 500, Common.Strings.VMName);

				Logger.Info("the response we get is " + res);

				JSonReader readjson = new JSonReader();
				IJSonObject resJson = readjson.ReadAsJSonObject(res);
				string result = resJson["result"].StringValue.Trim();
				if (result == "ok")
				{
					Logger.Info("the key mapping tool successfully launched");
				}
				else
				{
					Logger.Error("the key mapping tool could not be launched , got the response : {0}", res);
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error occured in trying to launch key mapping tool, Err: {0}", e.ToString()));
			}
		}

		protected override bool CanEnableIme
		{
			get
			{
				try
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
					int disablePcIme = (int)key.GetValue("DisablePcIme");
					if (disablePcIme == 0)
						return true;
					else
						return false;
				}
				catch (Exception ex)
				{
					Logger.Error("got exception in accessing registry ex :{0}", ex.ToString());
					return true;
				}
			}
		}

		private static void StoreArgs(String[] args)
		{
			foreach (String arg in args)
			{
				sArgs += "\"" + arg + "\" ";
			}
		}

		private void startStayAwakeService()
		{
			bool toggle = true;
			while (true)
			{
				try
				{
					if (Oem.Instance.IsAndroidToBeStayAwake)
					{
						if (toggle)
						{
							mInputMapper.EmulatePinch(0.5f, 0.5f, true);
						}
						else
						{
							mInputMapper.EmulatePinch(0.5f, 0.5f, false);
						}
						toggle = !toggle;
					}
				}
				catch (Exception)
				{
				}
				Thread.Sleep(60000);
			}
		}

		public static void ValidateArgs(String[] args)
		{
			if (args.Length < 1)
				Usage();
		}

		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
		{
			return true;
		}

#if BUILD_HYBRID
		private static bool CheckAndroidFilesIntegrity()
		{
			Logger.Info("Inside CheckAndroidFilesIntegrity check");
			string androidPath = Common.Strings.BstAndroidDir;
			string prebundledVdiFile = Path.Combine(androidPath, "Prebundled.vdi");
			string rootVdiFile = Path.Combine(androidPath, "Root.vdi");
			string kernelElfFile = Path.Combine(androidPath, "kernel.elf");
			string initrdImgFile = Path.Combine(androidPath, "initrd.img");
			string dataVdiPath = Path.Combine(androidPath, "Data.vdi");
			string sdCardVdiPath = Path.Combine(androidPath, "SDCard.vdi");

			try
			{
				if (Utils.IsFileNullOrMissing(prebundledVdiFile) ||
						Utils.IsFileNullOrMissing(rootVdiFile) ||
						Utils.IsFileNullOrMissing(kernelElfFile) ||
						Utils.IsFileNullOrMissing(initrdImgFile) ||
						Utils.IsFileNullOrMissing(dataVdiPath) ||
						Utils.IsFileNullOrMissing(sdCardVdiPath))
					return false;
				else
					return true;
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				/*
                 * Don't assume installation is corrupt in case of an exception here
                 */
				return true;
			}
		}
#else
	private static bool CheckAndroidFilesIntegrity()
	{
			string androidPath = Common.Strings.BstAndroidDir;
			string prebundledFsFile = Path.Combine(androidPath, "Prebundled.fs");
			string rootFsFile = Path.Combine(androidPath, "Root.fs");
			string kernelElfFile = Path.Combine(androidPath, "kernel.elf");
			string initrdImgFile = Path.Combine(androidPath, "initrd.img");
			string dataFSPath = Path.Combine(androidPath, "Data.sparsefs");
			string sdCardFSPath = Path.Combine(androidPath, "SDCard.sparsefs");

			try
			{
				if (Utils.IsFileNullOrMissing(prebundledFsFile) ||
						Utils.IsFileNullOrMissing(rootFsFile) ||
						Utils.IsFileNullOrMissing(kernelElfFile) ||
						Utils.IsFileNullOrMissing(initrdImgFile) ||
						Utils.IsFileNullOrMissing(Path.Combine(dataFSPath, "Map")) ||
						!File.Exists(Path.Combine(dataFSPath, "Store")) ||
						Utils.IsFileNullOrMissing(Path.Combine(sdCardFSPath, "Map")) ||
						!File.Exists(Path.Combine(sdCardFSPath, "Store")))
							return false;
				else
					return true;
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				/*
			 	 * Don't assume installation is corrupt in case of an exception here
			 	 */
				return true;
			}
		}
#endif

		public void RefreshKeyMapping()
		{
			Logger.Info("Refresh Keymapping");
			if (String.IsNullOrEmpty(mCurrentAppPackage) == false)
			{
				InputMapper.Instance().SetPackage(mCurrentAppPackage);
			}
		}

		public void MuteEngine()
		{
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				Audio.Manager.Mute();
			}
			else
			{
				SendMuteEventToGuest(true);
			}
		}

		public void UnMuteEngine()
		{
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				Audio.Manager.Unmute();
			}
			else
			{
				SendMuteEventToGuest(false);
			}
		}

		public void SetCurrentAppData(HttpListenerRequest req,
				HttpListenerResponse res)
		{
			Logger.Info("SetCurrentAppData");
			try
			{
				RequestData requestData = HTTPUtils.ParseRequest(req);
				if (String.Compare(mCurrentAppPackage, requestData.data["package"]) != 0 &&
						mDisableMouseMovement == true)
				{
					UIHelper.RunOnUIThread(this, delegate ()
					{
						DisableShootingMode();
					});
				}
				mCurrentAppPackage = requestData.data["package"];
				mCurrentAppActivity = requestData.data["activity"];
				Logger.Info("SetCurrentAppData mCurrentAppPackage = " +
						mCurrentAppPackage);
				Logger.Info("SetCurrentAppData mCurrentAppActivity = " +
						mCurrentAppActivity);
				Logger.Info("Looking for: " + sAppPackage);


				if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
				{
					if (!this.guestFinishedBooting &&
						(mCurrentAppPackage != "com.bluestacks.gamepophome" &&
						mCurrentAppPackage != "com.bluestacks.appmart") &&
						(sAppIconName.Contains(mCurrentAppActivity) ||
						sAppPackage == mCurrentAppPackage)
						)
					{
						Logger.Info("Moved away from home");
						this.guestFinishedBooting = true;

						if (!this.checkingIfGuestReady && !this.isGuestReady)
						{
							CheckIfGuestReady();
						}

						this.appLaunched = true;
					}
				}
				else
				{
					if (sAppLaunchedFromRunApp == true ||
										sAppIconName.Contains(mCurrentAppActivity))
					{
						appLaunched = true;
						sAppLaunchedFromRunApp = false;
					}
				}

				if (BlueStacks.hyperDroid.Common.Oem.Instance.IsSendGameManagerRequest)
				{
					string callingPackage = requestData.data["callingPackage"];

					Dictionary<string, string> data = new Dictionary<string, string>();
					data.Add("package", mCurrentAppPackage);
					data.Add("activity", mCurrentAppActivity);
					data.Add("callingPackage", callingPackage);
					SendGameManagerRequest(data, Common.Strings.AppLaunchedUrl);
				}

				if (Features.ExitOnHome() &&
						this.appLaunched &&
						(mCurrentAppPackage == "com.bluestacks.gamepophome" ||
						mCurrentAppPackage == "com.bluestacks.appmart"))
				{
					Logger.Info("Reached home app. Closing frontend.");
					OnFrontendClose();
				}

				Interop.Opengl.HandleAppActivity(mCurrentAppPackage,
					mCurrentAppActivity);

				if (Oem.Instance.IsUseFrontendBanner)
				{
					FrontendBanner.HandleBackgroundBannerImage(mCurrentAppPackage, this);
				}

				InputMapper.Instance().SetPackage(mCurrentAppPackage);
				InputMapperTool.sCurrentAppPackage = mCurrentAppPackage;
				HTTPHandler.WriteSuccessJson(res);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in Server SetCurrentAppData: " + exc.ToString());
				HTTPHandler.WriteErrorJson(exc.Message, res);
			}
		}

		public void SetKeyMappingState(bool state)
		{
			mInputMapper.SetKeyMappingState(state);
		}

		private static void Quit()
		{
			Logger.Info("Exiting BlueStacks. Killing all running processes...");

			Utils.StopServiceNoWait(Common.Strings.AndroidServiceName);

			Utils.KillProcessesByName(new String[] {
				"HD-ApkHandler",
				"HD-Adb",
				"HD-Restart",
				"HD-RunApp"
				});

			Environment.Exit(0);
		}

		private static void Usage()
		{
			String prog = Process.GetCurrentProcess().ProcessName;
			String capt = "BlueStacks Frontend";
			String text = "";

			text += String.Format("Usage:\n");
			text += String.Format("    {0} <vm name>\n", prog);

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMessageBoxToBeDisplayed)
			{
				Logger.Info("Displaying Message box");
				MessageBox.Show(text, capt);
				Logger.Info("Displayed Message box");
			}
			Environment.Exit(1);
		}

		private static Image ResizeImage(string imagePath, int height, int width)
		{
			Image src = Image.FromFile(imagePath);
			Image dst = new Bitmap(width, height);

			Graphics g = Graphics.FromImage(dst);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(src, 0, 0, dst.Width, dst.Height);
			return dst;
		}

		private static void WaitForDebugger()
		{
			while (!Debugger.IsAttached)
			{
				Thread.Sleep(1000);
			}

			Debugger.Break();
		}

		private static void PromptUserForUpdates()
		{
			String capt = "Check for Updates";
			String text = String.Format("Please check whether a new " +
				"version of BlueStacks is available at {0}",
				User.FIRST_TIME_LAUNCH_URL);

			Logger.Info("Displaying Message box");
			DialogResult res = Common.UI.MessageBox.ShowMessageBox(
				capt, text, "Check Now and Launch", "Remind Me Later",
				null);
			Logger.Info("Displayed Message box");
			if (res == DialogResult.OK)
			{
				String url = String.Format(
					"{0}?version={1}&user_guid={2}",
					User.FIRST_TIME_LAUNCH_URL, Version.STRING,
					User.GUID);

				try
				{
					Process.Start(url);
				}
				catch (Exception exc)
				{
					Logger.Error(exc.ToString());
				}
			}
		}

		private static IntPtr BringToFront(string vmName)
		{
			/*
             * Is the frontend running fullscreen?
             */

			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
					Common.Strings.GuestRegKeyPath, vmName);
			bool success = true;

			RegistryKey key;
			bool fullScreen;

			using (key = Registry.LocalMachine.OpenSubKey(path))
			{
				fullScreen = (int)key.GetValue("FullScreen", 0) != 0;
			}

			/*
             * Try to bring the frontend window to the foreground.
             */

			Logger.Info(String.Format("Starting BlueStacks {0} Frontend",
						vmName));
			String name = Oem.Instance.CommonAppTitleText;

			IntPtr handle = IntPtr.Zero;

			try
			{
				handle = Common.Interop.Window.BringWindowToFront(name, fullScreen);
			}
			catch (Exception exc)
			{

				Logger.Error("Cannot bring existing frontend " +
						"window for VM {0} to the foreground", vmName);
				Logger.Error(exc.ToString());
				success = false;
			}

			if (success == false || String.IsNullOrEmpty(sAppPackage) == false)
			{
				Logger.Info("we will try to bring frontend to foreground through setfrontendvisiblity http request");
				string res = SetFrontendVisibility(true);
				Logger.Info("the response we get from setfrontendvisiblity is {0}", res);
				handle = Common.Interop.Window.FindWindow(null, name);
			}
			return handle;
		}

		private void DeleteAppCrashLogFile()
		{
			try
			{
				string logFilePath = Path.Combine(Utils.GetLogDir(), Common.Strings.AppCrashInfoFile);

				Mutex appCrashInfoWriteLock;
				if (File.Exists(logFilePath))
				{
					if (Common.Utils.IsAlreadyRunning(Common.Strings.AppCrashInfoFile, out appCrashInfoWriteLock) == false)
					{
						File.Delete(logFilePath);
						appCrashInfoWriteLock.Close();
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error Occurred. Err: {0}", e.ToString());
			}
		}

		protected override void OnLoad(EventArgs evt)
		{
			Interop.Animate.AnimateWindow(Handle, 500,
				Interop.Animate.AW_BLEND);

			if (Features.IsFeatureEnabled(Features.WRITE_APP_CRASH_LOGS) == true)
				DeleteAppCrashLogFile();

			Common.Stats.SendFrontendStatusUpdate("frontend-launched");
			base.OnLoad(evt);
		}

		private void StartAgentIfNeeded()
		{
			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			if (Utils.IsProcessAlive(Common.Strings.HDAgentLockName) == false)
			{
				Logger.Info("Agent not running, launching agent");
				string installDir = (string)prodKey.GetValue("InstallDir");
				Process.Start(Path.Combine(installDir, "HD-Agent.exe"));
			}
		}

		public Console(String vmName) : this(vmName, false){}

		public Console(String vmName, bool hideMode)
			: base(TOUCH_POINTS_MAX,
				delegate (String msg) { Logger.Info("Touch: " + msg); })
		{
			Locale.Strings.InitLocalization(null);

			if (hideMode)
				sHideMode = hideMode;

			if (Oem.Instance.IsRunBlueStacksAutoUpgradeMechanism)
			{
				if (!Common.Oem.Instance.IsOemWithGameManagerData)
					Common.Utils.ExitIfForceUpdateAvailable();
				Common.Utils.StartUpdaterIfAvailable();
			}

			Utils.LogParentProcessDetails();
			Common.Strings.VMName = vmName;
			Console.sVName = vmName;
			Logger.Info("Engine registry value: {0}",
					(BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy() ? "legacy" : "plus"));

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
			}
			else
			{
				try
				{
					Utils.CheckForHDQuitRunning();
				}
				catch (Exception ex)
				{
					Logger.Error("Ignoring any Exception");
					Logger.Error("Error: {0}", ex.ToString());
				}
			}

			SetInstallDir();

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsFrontEndDPIAware && !Utils.IsOSWinXP())
				SetProcessDPIAware();

			ServicePointManager.DefaultConnectionLimit = 10;

			try
			{
				Common.Strings.AppTitle = BlueStacks.hyperDroid.Common.Oem.Instance.CommonAppTitleText;

				if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
				{
				}
				else
				{
					if (Common.Strings.VMName != "Android")
					{
						//RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GuestRegKeyPath);
						//string[] vmList = (string[]) key.GetValue("VMList", null);
						//string titleSuffix = " ";
						//foreach ( string vmPair in vmList)
						//{
						//    string vmName = vmPair.Split(new char[]{':'})[1];
						//    if (vmName == Common.Strings.VMName)
						//    {
						//        titleSuffix += vmPair.Split(new char[]{':'})[0];
						//        break;
						//    }
						//}
						//Common.Strings.AppTitle += titleSuffix;
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Could not set app player title. err: " + ex.ToString());
				// Ignore. Default title would be used
			}

			try
			{
#if BUILD_HYBRID
				if (Common.Strings.IsEngineLegacy())
					Common.Utils.StopService(Common.Strings.GetHDPlusAndroidServiceName(Console.sVName));
				else
					Common.Utils.StopService(Common.Strings.GetHDAndroidServiceName(Console.sVName));
#endif

				isUsePcImeWorkflow = Oem.Instance.IsUsePcImeWorkflow;

				if (!isUsePcImeWorkflow)
				{
					string locale = CultureInfo.CurrentCulture.Name;
					isUsePcImeWorkflow = Utils.IsForcePcImeForLang(locale);
				}

				Application.EnableVisualStyles();

				ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

				RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				String installType = (String)prodKey.GetValue("InstallType");
				if (String.Compare(installType, "uninstalled", true) == 0)
				{
					Logger.Info("Displaying Message box");
					MessageBox.Show("BlueStacks App Player is not installed on this machine. Please install it and try again. You can download the latest version from www.bluestacks.com", "BlueStacks is not installed.");
					Logger.Info("Displayed Message box");
					Application.Exit();
				}

				if (String.Compare(installType, "nconly", true) != 0 &&
						String.Compare(installType, "split", true) != 0 &&
						!CheckAndroidFilesIntegrity())
				{
					Logger.Info("Displaying Message box");
					MessageBox.Show("The BlueStacks App Player installation is corrupt. Please download and install the latest version from www.bluestacks.com", "BlueStacks installation is corrupt.");
					Logger.Info("Displayed Message box");
					Environment.Exit(1);
				}

				using (RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath, true))
				{
					int systemStatsSent = (int)configKey.GetValue("SystemStats", 0);
					if (systemStatsSent == 0)
						Common.Stats.SendSystemInfoStats();

					if (Oem.Instance.OEM == "bluestacks" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "gamemanager")
					{
						string firstLaunchDate = (string)configKey.GetValue(Common.Strings.FirstLaunchDateTimeRegistryKey);
						if (firstLaunchDate == null)
						{
							Logger.Info("Firstlaunch registry is null");
							configKey.SetValue(Common.Strings.FirstLaunchDateTimeRegistryKey, DateTime.UtcNow.ToString(dateFormat));
							Logger.Info("time set in firstlaunch registry");
						}
						else if (firstLaunchDate == "done")
						{
							Logger.Info("S2p feature already set");
						}
						else
						{
							try
							{
								DateTime firstRun = Convert.ToDateTime(firstLaunchDate);
								TimeSpan ts = DateTime.UtcNow - firstRun;

								if (ts.Days >= 1)
								{
									Thread setS2PFeaturesThread = new Thread(delegate ()
											{
											SetS2PFeatureBitInBootParams();
											});
									setS2PFeaturesThread.IsBackground = true;
									setS2PFeaturesThread.Start();
								}
							}
							catch (Exception ex)
							{
								Logger.Info("got exception in checking lauch time err:{0}", ex.ToString());
								configKey.SetValue(Common.Strings.FirstLaunchDateTimeRegistryKey, DateTime.UtcNow.ToString(dateFormat));
							}
						}
					}
				}

				Logger.Info("console message loop done");

			}
			catch (Exception exc)
			{

				Logger.Error("Unhandled Exception:");
				Logger.Error(exc.ToString());

				Environment.Exit(1);
			}

			//Interop.DWM.DisableComposition();
			Thread stayAwakeThread = new Thread(delegate ()
			{
				startStayAwakeService();
			});
			stayAwakeThread.IsBackground = true;
			stayAwakeThread.Start();

			Interop.Input.DisablePressAndHold(this.Handle);
			Interop.Input.HookKeyboard(HandleKeyboardHook);

			StartAgentIfNeeded();

			s_Console = this;
			m_hImc = Common.Interop.Window.ImmGetContext(this.Handle);
			this.mUiContext = WindowsFormsSynchronizationContext.Current;

			this.vmName = vmName;

			this.keyboard = new Keyboard();
			this.mouse = new Mouse();

			Logger.Info("console constructor :settings capsLock and numsLock to false");

			mControllerMap = new Dictionary<int, String>();

			this.AllowDrop = true;
			this.DragEnter += FileImporter.HandleDragEnter;
			this.DragDrop += FileImporter.MakeDragDropHandler();
			this.Resize += Console_Resize;

			/*
             * Setup a triggerMemoryTrimTimer that periodically clears the working
             * set of the current process
             * and call triggerTrimMemory API of Android
             */

			int timerInterval = 60 * 1000; //time intenval in milliseconds
			int triggerThreshold = 700; //size in MB 

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			if (key != null)
			{
				timerInterval = (int)key.GetValue("triggerMemoryTrimTimerInterval", 60000);
				Logger.Info("the value of time interval in registry is {0}", timerInterval);
				triggerThreshold = (int)key.GetValue("TriggerMemoryTrimThreshold", 700);
				Logger.Info("the value of triggerThreshold in registry is {0}", triggerThreshold);
			}

			this.triggerMemoryTrimTimer = new System.Windows.Forms.Timer();
			this.triggerMemoryTrimTimer.Interval = timerInterval;

			this.triggerMemoryTrimTimer.Tick += delegate (Object obj, EventArgs evt)
			{
				long workingSet = System.Diagnostics.Process.GetCurrentProcess().WorkingSet64;
				int triggerThresholdInBytes = triggerThreshold * 1024 * 1024;

				if (workingSet > triggerThresholdInBytes)
				{
					Logger.Info("Current Process Working set exceeds {0} MB, its {1} now.",
							triggerThreshold,
							(workingSet / (1024 * 1024))
							);
					TriggerMemoryTrimInAndroid();
				}
			};

			this.triggerMemoryTrimTimer.Start();

			/*
             * Preallocate the touch point array so we don't have any
             * unnecessary allocations in the touch code path.
             */

			this.touchPoints =
				new Interop.Monitor.TouchPoint[TOUCH_POINTS_MAX];

			for (int ndx = 0; ndx < this.touchPoints.Length; ndx++)
				this.touchPoints[ndx] =
					new Interop.Monitor.TouchPoint(0xffff, 0xffff);

			mStateMachine = new StateMachine(this, vmName);
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
			}
			else
			{
				SetupStateTransitions();
			}

			InitConfig(vmName);
			InitControl();
			InitScreen();
			mLastFrontendStatusUpdateTime = DateTime.Now;

			sFrontendLaunchTime = DateTime.Now;

			mFullScreenToast = new FullScreenToast(this);

			this.Layout += HandleLayoutEvent;

			ServiceController sc = new ServiceController(Common.Strings.GetAndroidServiceName(vmName));

			if (sc.Status == ServiceControllerStatus.Running)
			{
				this.Paint += HandlePaintEvent;
			}

			if (this.disableDwm)
			{
				try
				{
					Interop.DWM.DisableComposition();
				}
				catch (Exception exc)
				{
					Logger.Error("Cannot disable DWM composition");
					Logger.Error(exc.ToString());
				}
			}

			mCursor = new BstCursor(this, sInstallDir);

			SetupInputMapper();
			Logger.Info("Done InputMapper");

			SetupOpenSensor();
			Logger.Info("Done OpenSensor");

			mCursor.SetInputMapper(mInputMapper);
			SetupSoftControlBar();

			mSensorDevice.StartThreads();

			/*
             * Setup the HTTP server used to receive messages from
             * the guest.
             */

			if (Common.Utils.ForceVMLegacyMode == 1)
			{
				/*
                 * Spin up our VMX checker thread.
                 */

				SetupVmxChecker();
			}

			/*
             * Spin up our graphics driver version checker thread.
             */

			SetupGraphicsDriverVersionChecker();

			/*
             * Hook up our display settings changed handler so we can
             * handle orientation changes.
             */
			Microsoft.Win32.SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				InitOpengl();

				StateExitInitial();
				ContinueStateMachine(vmName);
			}
			else
			{
				StartCompanionProcesses();
				mStateMachine.Start();
			}
		}

		private static void SetS2PFeatureBitInBootParams()
		{
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.AndroidKeyBasePath, true))
				{
					string bootParams = (string)key.GetValue("BootParameters");
					uint appsFeatureBits = 0;
					string[] bootParamsParts = bootParams.Split(' ');
					const string appsFeaturekey = "appsfeatures";
					foreach (string eachParam in bootParamsParts)
					{
						string[] keyValue = eachParam.Split('=');
						if (keyValue[0].Equals(appsFeaturekey))
						{
							string appsFeaturesOld = eachParam;
							appsFeatureBits = Convert.ToUInt32(keyValue[1]);
							Logger.Info("the android apps feature bits are" + appsFeatureBits.ToString());
							appsFeatureBits = appsFeatureBits & ~Features.BST_SKIP_S2P_WHILE_LAUNCHING_APP;
							Logger.Info("the new feature bits with s2p enabled is {0}", appsFeatureBits);
							string appsFeatureNew = string.Format("{0}={1}", appsFeaturekey, appsFeatureBits);
							bootParams = bootParams.Replace(appsFeaturesOld, appsFeatureNew);
							Logger.Info("the new bootParams are {0}", bootParams);
							key.SetValue("BootParameters", bootParams);
							Logger.Info("new bootparams set in registry");

							RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath, true);
							configKey.SetValue(Common.Strings.FirstLaunchDateTimeRegistryKey, "done");
							configKey.Close();
							Logger.Info("setting done in registry");

							Dictionary<string, string> data = new Dictionary<string, string>();
							data.Add(appsFeaturekey, appsFeatureBits.ToString());
							string refreshAppFeaturesUrl = String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, "refreshappsfeatures");
							Logger.Info("The url is {0}", refreshAppFeaturesUrl);
							string r = Common.HTTP.Client.Post(refreshAppFeaturesUrl, data, null, false, 4 * 60 * 1000);
							Logger.Info("the response we get is {0}", r);

						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Got error while setting s2p feature bit, err:{0}", e.ToString());
			}
		}

		void Console_Resize(object sender, EventArgs e)
		{
			FixupGuestDisplay();
		}

		private void StartCompanionProcesses()
		{
			ServiceController sc = null;

			sc = new ServiceController(Common.Strings.BstLogRotatorServiceName);
			Logger.Info("bshdlogrotatorsvc state is " + sc.Status);

			if (sc.Status == ServiceControllerStatus.Stopped ||
					sc.Status == ServiceControllerStatus.StopPending)
				StartLogRotatorServiceAsync();
		}

		private void SetupStateTransitions()
		{
			Logger.Info("Setting up state machine handlers");

			mStateMachine.SetCallbacks(State.Initial, State.Stopping, StateNop, ShowLoadingView);
			mStateMachine.SetCallbacks(State.Initial, State.Starting, StateNop, ShowLoadingViewAndHandleStarting);
			mStateMachine.SetCallbacks(State.Initial, State.Error, StateNop, StateError);
			mStateMachine.SetCallbacks(State.Initial, State.ConnectingVideo, StateNop, ShowLoadingViewAndHandleStarting);

			mStateMachine.SetCallbacks(State.Stopping, State.Starting, StateNop, HandleStarting);
			mStateMachine.SetCallbacks(State.Stopping, State.Error, StateNop, StateError);

			mStateMachine.SetCallbacks(State.Starting, State.ConnectingVideo, StateNop, StateNop);
			mStateMachine.SetCallbacks(State.Starting, State.Error, StateNop, StateError);

			if (HideBootProgress())
			{

				mStateMachine.SetCallbacks(State.ConnectingVideo, State.ConnectingGuest,
					StateNop, StateNop);
				mStateMachine.SetCallbacks(State.ConnectingVideo, State.Error,
					StateNop, StateError);

				mStateMachine.SetCallbacks(State.ConnectingGuest, State.Connected,
					StateNop, ShowConnectedViewAndPerformDeferredSetupAndShowVtxPopup);
				mStateMachine.SetCallbacks(State.ConnectingGuest, State.Error,
					StateNop, StateError);

			}
			else
			{

				mStateMachine.SetCallbacks(State.ConnectingVideo, State.ConnectingGuest,
					StateNop, ShowConnectedView);
				mStateMachine.SetCallbacks(State.ConnectingVideo, State.Error,
					StateNop, StateError);

				mStateMachine.SetCallbacks(State.ConnectingGuest, State.Connected,
					StateNop, StateNop);
				mStateMachine.SetCallbacks(State.ConnectingGuest, State.Error,
					StateNop, StateError);
			}

			mStateMachine.SetCallbacks(State.Connected, State.Error, StateNop, StateError);
			mStateMachine.SetCallbacks(State.Quitting, State.Starting, StateNop, StateNop);
			mStateMachine.SetCallbacks(State.Quitting, State.Connected, StateNop, StateNop);
			mStateMachine.SetCallbacks(State.Quitting, State.ConnectingGuest, StateNop, StateNop);
			mStateMachine.SetCallbacks(State.Quitting, State.ConnectingVideo, StateNop, StateNop);
			mStateMachine.SetCallbacks(State.Quitting, State.Stopping, StateNop, StateNop);
		}

		private void StateNop(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);
		}

		private void StateError(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);

			/*
             * Close the frontend on any error.
             */

			Logger.ErrorNotify("Service stopped unexpectedly");
			Logger.Error(new StackTrace().ToString());

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyChangesToParentWindow)
			{
				int exitCode = -1;
				exitCode = StartBootFailureDebugging();
				WindowMessages.NotifyBootFailureToParentWindow(exitCode);
			}

			Environment.Exit(1);
		}

		private void InitOpengl()
		{
			Logger.Info("Checking for port availablity for GlInit");
			Common.HTTP.Server tempServer;

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath, true);
			int port = 2901;
			bool noPortAvailable = false;

			Dictionary<String, Common.HTTP.Server.RequestHandler> routes = new Dictionary<String, Common.HTTP.Server.RequestHandler>();
			for (; port <= 2910; port++)
			{
				try
				{
					tempServer = new Common.HTTP.Server(port, routes, null);
					tempServer.Start();
					Logger.Info("Successfully started server listening on port: " + port);

					key.SetValue("GlPort", port, RegistryValueKind.DWord);

					Logger.Info("Stopping server listening on port: " + port);
					tempServer.Stop();
					break;
				}
				catch (Exception e)
				{
					Logger.Error(String.Format("Error Occured while listening on port {0}, Err: {1}", port, e.ToString()));
					if (port == 2910)
						noPortAvailable = true;
					continue;
				}
			}
			key.Close();

			if (noPortAvailable)
			{
				MessageBox.Show(String.Format("Server initialization failed! Please stop any server running on port {0} to start using bluestacks", 2901));
				Environment.Exit(-1);
			}
			Logger.Info("Opengl.Init({0}, {1}, {2}, {3}, {4})",
				this.Handle,
				mScaledDisplayArea.X,
				mScaledDisplayArea.Y,
				mConfiguredGuestSize.Width,
				mConfiguredGuestSize.Height);

			Interop.Opengl.Init(
				vmName,
				this.Handle,
				mScaledDisplayArea.X,
				mScaledDisplayArea.Y,
				mConfiguredGuestSize.Width,
				mConfiguredGuestSize.Height,
				GlInitSuccess,
				GlInitFailed);

			Logger.Info("Done Opengl.Init");
		}

		private void HandleStarting(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);
			InitOpengl();
		}

		private void ShowLoadingView(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);

			AddLoadingScreen("Marquee");
		}

		private void ShowLoadingViewAndHandleStarting(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);

			ShowLoadingView(oldState, newState);
			HandleStarting(oldState, newState);
		}

		private void ShowConnectedView(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);

			SignalReady(vmName);
			/*
             * Set the monitor used by the InputMapper early, as it may be used to unwedge stuck keys
             * when we handle frontend activated events.
             */

			mInputMapper.SetMonitor(mStateMachine.Monitor);
			/*
             * Set the monitor used by Gps Manager class, as it is used to
             * periodically send the GPS coordinates.
             */
			mGps = Gps.Manager.Instance();
			mGps.SetMonitor(mStateMachine.Monitor);

			RemoveLoadingScreen();

			Logger.Debug("Raising Layout event");
			OnLayout(new LayoutEventArgs(this, ""));

			this.userInteracted = true;

			if (!Interop.Opengl.IsSubWindowVisible())
			{
				Logger.Info("showing window");
				this.glWindowAction = GlWindowAction.Show;
				this.userInteracted = false;
			}

			FixupGuestDisplay();

			/*
             * Start the display timer.
             */

			this.timer = new System.Windows.Forms.Timer();
			this.timer.Interval = 1000 / 30;

			this.timer.Tick += delegate (Object obj, EventArgs evt)
			{
				HandleDisplayTimeout();
			};

			this.timer.Start();

			/*
             * Wire up the display paint event.
             */

			this.Paint += HandlePaintEvent;

			/*
             * Wire up our event handlers.
             */

			//this.Activated += HandleActivatedEvent;
			//this.Deactivate += HandleDeactivateEvent;

			this.MouseMove += HandleMouseMove;
			this.MouseDown += HandleMouseDown;
			this.MouseUp += HandleMouseUp;

			this.MouseWheel += HandleMouseWheel;

			SystemEvents.PowerModeChanged += new PowerModeChangedEventHandler(SystemEvents_PowerModeChanged);

			this.TouchEvent += HandleTouchEvent;

			this.KeyDown += HandleKeyDown;
			this.KeyUp += HandleKeyUp;

			isGuestReady = true;

			SendLockedKeys();
		}

		private void PerformDeferredSetup(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);

			Common.Stats.SendBootStats("frontend", true, false);

			
			this.guestFinishedBooting = true;

			SendOrientationToGuest();

			/*
             * One-time sync of config of the Host with the VM
             */
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			int configSynced = (int)key.GetValue("ConfigSynced", 0);
			if (configSynced == 0)
			{
				Logger.Info("Config not synced. Syncing now.");
				ThreadPool.QueueUserWorkItem(delegate (Object stateInfo)
				{
					string parserVersion = InputMapper.GetKeyMappingParserVersion();
					VmCmdHandler.SyncConfig(parserVersion);
					VmCmdHandler.SetKeyboard(IsDesktop());
				});
			}
			else
				Logger.Info("Config already synced.");

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsJsonModelToBePushed)
				PushModelJsonIfNeeded();

			/*
             * Send fqdn of the Host with the VM 
             * This is required to handle restart case. Agent will not send
             * port again on restart, so this needs to be updated from here.
             */
			ThreadPool.QueueUserWorkItem(delegate (Object stateInfo)
			{
				Logger.Info("Started fqdnSender thread");
				VmCmdHandler.FqdnSend(0, "Agent");
				Logger.Info("fqdnSender thread exiting");
			});

			ThreadPool.QueueUserWorkItem(delegate (Object stateInfo)
			{
				Common.VmCmdHandler.SetMachineType(IsDesktop());
			});

			/*
             * Initialize our virtual devices before wiring up the event handlers.  Otherwise,
             * we have a race window where event handlers that poke at these devices can throw
             * null reference exceptions.
             */

			GpsAttach();
			mSensorDevice.Start(this.vmName);
			CameraAttach();

			/*
             * Process any pending controller attach events.
             */

			SendControllerEventInternal("controller_flush", delegate ()
			{

				foreach (int identity in mControllerMap.Keys)
				{
					mSensorDevice.ControllerAttach(SensorDevice.Type.Accelerometer);
					SendControllerEvent("attach", identity, mControllerMap[identity]);
				}

				mControllerMap.Clear();
			});

			OutputDebugString("SpawnApps:executeJavascript(\"frontendFinishedBooting();\")");

			ThreadPool.QueueUserWorkItem(delegate (Object stateInfo)
			{
				Logger.Info("Checking for Black Screen Error");

				CheckBlackScreenAndRestartGMifOccurs();
			});
		}

		private void ShowConnectedViewAndPerformDeferredSetupAndShowVtxPopup(State oldState, State newState)
		{
			Logger.Info("Console.{0}({1}, {2})", MethodBase.GetCurrentMethod().Name, oldState, newState);

			ShowConnectedView(oldState, newState);
			PerformDeferredSetup(oldState, newState);

#if BUILD_HYBRID
			ThreadPool.QueueUserWorkItem(delegate(Object stateInfo)
					{
					CheckVtxAndShowPopup();
					});
#endif
		}

		internal void InputLangChanged(Object sender, InputLanguageChangedEventArgs e)
		{
			Logger.Info("The inputlanguage changed");
			if (Oem.Instance.IsUsePcImeWorkflow)
			{
				Logger.Info("PC Ime workflow already specfied in config, returning");
				return;
			}
			string inputLang = e.InputLanguage.Culture.ToString();
			if (inputLang != CultureInfo.CurrentCulture.Name && !Utils.IsEastAsianLanguage(inputLang) && Utils.IsLatinImeSelected())
			{
				Logger.Info("different input language choosen from system language");
				isUsePcImeWorkflow = true;
			}
			else
			{
				Logger.Info("input language and system default language are same");
				isUsePcImeWorkflow = false;
			}
		}

		public static void TriggerMemoryTrimInAndroid()
		{
			Logger.Info("In TriggerMemoryTrimInAndroid");
			Thread triggerMemoryTrimThread = new Thread(delegate ()
					{
						try
						{
							int port = Utils.GetBstCommandProcessorPort(Common.Strings.VMName);
							string path = "triggerMemoryTrim";
							string url = string.Format("http://127.0.0.1:{0}/{1}", port, path);
							Logger.Info("Sending request to: " + url);

							string r = Common.HTTP.Client.Get(url, null, false); //default timeout is 100 seconds
							IJSonReader json = new JSonReader();
							IJSonObject res = json.ReadAsJSonObject(r);
							string result = res["result"].StringValue;

							Logger.Info("the result we get from {0} is {1}", path, result);
						}
						catch (Exception e)
						{
							Logger.Error("Error Occured when calling triggerMemoryTrim API of BstCommandProcessor, Err: {0}", e.ToString());
						}
					});

			triggerMemoryTrimThread.IsBackground = true;
			triggerMemoryTrimThread.Start();
		}

		public void UpdateUserActivityStatus()
		{
			try
			{
				Logger.Debug(string.Format("In UpdateUserActivityStatus, mLastFrontendStatusUpdateTime = {0}", mLastFrontendStatusUpdateTime));
				if (TimeSpan.Compare(DateTime.Now.Subtract(mLastFrontendStatusUpdateTime), new TimeSpan(0, 0, 10)) > 0)
				{
					Common.Stats.SendFrontendStatusUpdate("user-active");
					mLastFrontendStatusUpdateTime = DateTime.Now;
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
			}
		}

		private void ContinueStateMachine(string vmName)
		{
			Logger.Info("Continuing state machine");

			String AndroidSrvName = Common.Strings.GetAndroidServiceName(vmName);
			ServiceController sc = new ServiceController(AndroidSrvName);

			Logger.Info("bsthd" + sVName + "svc state is " + sc.Status);

			if (sc.Status == ServiceControllerStatus.Stopped ||
					sc.Status == ServiceControllerStatus.StopPending)
				StartVmServiceAsync();

			string logRotatorSvcName = Common.Strings.BstLogRotatorServiceName;
			sc = new ServiceController(logRotatorSvcName);
			Logger.Info("bshdlogrotatorsvc state is " + sc.Status);
			if (sc.Status == ServiceControllerStatus.Stopped ||
					sc.Status == ServiceControllerStatus.StopPending)
				StartLogRotatorServiceAsync();


			Logger.Info("Attaching VM");


			/* Try attaching to the desired VM. */
			if (mStateMachine.TryConnectVideo())
			{
				Logger.Info("Attached");
				mInputMapper.SetMonitor(mStateMachine.Monitor);

				if (HideBootProgress())
				{
					if (ConnectingBlankEnabled())
						StateEnterConnectingBlank();
					else
						StateEnterConnecting();
				}
				else
				{
					StateEnterConnected();
				}
			}
			else
			{
				Logger.Info("Attach failed");
				StateEnterStopped(vmName);
			}
		}
		private bool IsAppInstalled(string appPackage)
		{
			bool installed;

			try
			{
				string url = String.Format("http://127.0.0.1:{0}/{1}",
						Common.VmCmdHandler.s_ServerPort,
						Common.VmCmdHandler.s_PingPath);
				Common.HTTP.Client.Get(url, null, false, 3000);
				Logger.Info("Guest booted. Will send request.");

				url = String.Format("http://127.0.0.1:{0}/{1}",
						Common.VmCmdHandler.s_ServerPort,
						Common.Strings.IsPackageInstalledUrl);
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("package", appPackage);
				string r = Common.HTTP.Client.Post(url, data, null, false);
				JSonReader readjson = new JSonReader();
				IJSonObject resJson = readjson.ReadAsJSonObject(r);
				string result = resJson["result"].StringValue.Trim();
				if (result == "ok")
				{
					Logger.Info("App installed");
					installed = true;
				}
				else
				{
					Logger.Info("App not installed");
					installed = false;
				}
			}
			catch (Exception)
			{
				Logger.Info("Guest not booted. Will read from apps.json");

				string version;
				if (JsonParser.IsAppInstalled(appPackage, out version))
				{
					Logger.Info("Found in apps.json");
					installed = true;
				}
				else
				{
					Logger.Info("Not found in apps.json");
					installed = false;
				}
			}

			return installed;
		}

		private static string SetFrontendVisibility(bool visible)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("visible", visible.ToString());

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
			}
			else
			{
				if (String.IsNullOrEmpty(sAppIconName) == false)
					data.Add("AppIcon", sAppIconName);
			}

			if (String.IsNullOrEmpty(sAppPackage) == false)
				data.Add("appPackage", sAppPackage);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			int frontendPort = (int)key.GetValue("FrontendServerPort", 2871);
			string url = String.Format("http://127.0.0.1:{0}/{1}", frontendPort, "setfrontendvisibility");

			Logger.Info("FrontEndVisiblity: Sending post request to {0}", url);
			string res = Common.HTTP.Client.Post(url, data, null, false);
			return res;
		}

		public void RunApp(string package, string activity)
		{
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("package", package);
			data.Add("activity", activity);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			int agentPort = (int)key.GetValue("AgentServerPort", 2861);
			string url = String.Format("http://127.0.0.1:{0}/{1}", agentPort, "runapp");

			Logger.Info("RunApp: Sending post request to {0}", url);
			Common.HTTP.Client.PostWithRetries(url, data, null, false, 10, 500, Common.Strings.VMName);
		}

		private void UpdateDownloadProgressFromReg()
		{
			int progressPercent = 0;

			while (true)
			{
				RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				progressPercent = (int)prodKey.GetValue("DownloadProgress", 0);

				SendOrPostCallback cb = new SendOrPostCallback(delegate (Object obj)
						{
							this.loadingScreen.UpdateProgressBar(progressPercent);
						});

				try
				{
					mUiContext.Send(cb, null);
				}
				catch (Exception)
				{
				}

				if (progressPercent == 100)
				{
					Logger.Info("Download completed 100%");
					break;
				}

				Thread.Sleep(1000);
				continue;
			}
		}

		private void GlInitSuccess()
		{
			Logger.Info("Gl Init success");
			FixupGuestDisplay();

			/*
             * Start a thread which keeps checking if the guest has finished booting
             */

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				if (!this.checkingIfBooted)
					CheckIfGuestFinishedBooting();
			}
		}

		private void GlInitFailed()
		{
			Logger.Error("Gl Init failed");
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				this.glInitFailed = true;
			}
			else
			{
				mStateMachine.SignalFailure();
			}
		}

		public void SetupGraphicsDriverVersionChecker()
		{
			Logger.Info("in SetupGraphicsDriverVersionChecker");

			Thread t = new Thread(delegate ()
			{
				while (!sPgaInitDone)
					Thread.Sleep(100);

				string msgType;
				bool isUptodate = Utils.IsGraphicsDriverUptodate(out sDriverUpdateUrl, out msgType, null);
				if (!isUptodate)
				{
					String msg = Locale.Strings.GraphicsDriverOutdatedWarning;
					switch (msgType)
					{
						case "warning":
							// Default value. No need to set again.
							break;
						case "ignore":
							return;
					}

					if (Features.IsGraphicsDriverReminderEnabled())
						HTTPHandler.SendSysTrayNotification("Graphics Driver Checker", "error", msg);
				}
				Logger.Info("driverVersionChecker done");
			});
			t.IsBackground = true;
			t.Start();
		}

		public void UpdateGraphicsDrivers()
		{
			Logger.Info("User chose to update graphics drivers");

			if (sDriverUpdateUrl.EndsWith("zip"))
			{
				Thread stopZygoteThread = new Thread(
						delegate ()
						{
							Interop.Opengl.StopZygote(vmName);
						});
				stopZygoteThread.IsBackground = true;
				stopZygoteThread.Start();

				GraphicsDriverUpdater graphicsDriverUpdater = new GraphicsDriverUpdater(s_Console);
				graphicsDriverUpdater.Update(sDriverUpdateUrl);
				UIHelper.RunOnUIThread(this, delegate ()
				{
					graphicsDriverUpdater.ShowDialog();
				});
			}
			else
			{
				Process.Start(sDriverUpdateUrl);
				Logger.Info("Exiting frontend");
				Environment.Exit(0);
			}
		}

		public void SetupInputMapper()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(
				REG_CONFIG);

			mInputMapper = InputMapper.Instance();

			int verbose = (int)key.GetValue("InputMapperVerbose", 0);

			mInputMapper.Init(Common.Strings.InputMapperFolder,
				verbose != 0, mSensorDevice, mCursor);
			mInputMapper.SetConsole(this);
			mInputMapper.SetControlHandler(new ControlHandler(this));

			int swipeLength = (int)key.GetValue(
				"InputMapperEmulatedSwipeLength", 20);
			int swipeDuration = (int)key.GetValue(
				"InputMapperEmulatedSwipeDuration", 100);

			mInputMapper.SetEmulatedSwipeKnobs(
				(float)swipeLength / 100,
				swipeDuration);

			int pinchSplit = (int)key.GetValue(
				"InputMapperEmulatedPinchSplit", 20);
			int pinchLengthIn = (int)key.GetValue(
				"InputMapperEmulatedPinchLengthIn", 20);
			int pinchLengthOut = (int)key.GetValue(
				"InputMapperEmulatedPinchLengthOut", 10);
			int pinchDuration = (int)key.GetValue(
				"InputMapperEmulatedPinchDuration", 50);

			mInputMapper.SetEmulatedPinchKnobs(
				(float)pinchSplit / 100,
				(float)pinchLengthIn / 100,
				(float)pinchLengthOut / 100,
				pinchDuration);

			mInputMapper.SetDisplay(mEmulatedPortraitMode,
				mRotateGuest180);

			/*
             * Optionally override the locale setting.
             */

			String locale = (String)key.GetValue("InputMapperLocale", "");
			if (locale != "")
				mInputMapper.OverrideLocale(locale);

			key.Close();
		}

		private void SetupOpenSensor()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(
				REG_CONFIG);

			int verbose = (int)key.GetValue(
				"OpenSensorVerbose", 0);
			int beaconPort = (int)key.GetValue(
				"OpenSensorBeaconPort", 10505);
			int beaconInterval = (int)key.GetValue(
				"OpenSensorBeaconInterval", 2000);
			String deviceType = (String)key.GetValue(
				"OpenSensorDeviceType", "WindowsPC");

			mOpenSensor = new OpenSensor(mInputMapper, beaconPort,
				beaconInterval, deviceType, mSensorDevice, mCursor,
				verbose != 0);
			mOpenSensor.SetConsole(this);
			mOpenSensor.SetControlHandler(new ControlHandler(this));
			mOpenSensor.Start();

			key.Close();
		}

		private void SetupSoftControlBar()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(
				REG_CONFIG);

			if (Utils.IsAndroidFeatureBitEnabled(Features.BST_HIDE_NAVIGATIONBAR) == false)
			{
				Logger.Info("Soft Control Bar Enabled");
				SoftControlBarVisible(true);
			}

			key.Close();
		}

		public void SoftControlBarVisible(bool visible)
		{
			float landscape = 0;
			float portrait = 0;
			mInputMapper.mSoftControlEnabled = visible;

			if (visible)
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(
					REG_CONFIG);
				float cfgLandscape, cfgPortrait;
				if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
				{
					RegistryKey frameBufferKey = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath);

					int guestHeight = (int)frameBufferKey.GetValue("GuestHeight", 1);
					int windowHeight = (int)frameBufferKey.GetValue("WindowHeight", 1);

					frameBufferKey.Close();

					/*
                     * the guest and host resolutions are different
                     * in some machines, when calculating the softControlbarHeight
                     * the guestHostScaling was not getting considered
                     */

					float guestHostScaling = (float)windowHeight / (float)guestHeight;

					cfgLandscape = guestHostScaling * (int)key.GetValue(
						"SoftControlBarHeightLandscape", 0);
					cfgPortrait = guestHostScaling * (int)key.GetValue(
						"SoftControlBarHeightPortrait", 0);
				}
				else
				{
					cfgLandscape = (int)key.GetValue(
						"SoftControlBarHeightLandscape", 0);
					cfgPortrait = (int)key.GetValue(
						"SoftControlBarHeightPortrait", 0);
				}
				landscape = (float)cfgLandscape /
					(float)mConfiguredDisplaySize.Height;
				portrait = (float)cfgPortrait /
					(float)mConfiguredDisplaySize.Width;

				key.Close();
			}

			mInputMapper.SetSoftControlBarHeight(landscape, portrait);
		}

		public void HandleControllerAttach(bool attach, int identity,
			String type)
		{
			/*
             * Handle this attach event on the main thread so we
             * synchronize correctly with the code that fires when
             * we enter the connected state.
             */

			UIHelper.RunOnUIThread(this, delegate ()
			{

				if (FrontendState == State.Connected)
				{

					Logger.Info(
						"Sending controller attach event " +
						"{0} {1} {2}",
						identity, attach, type);

					if (attach)
						mSensorDevice.ControllerAttach(
							SensorDevice.Type.Accelerometer);
					else
						mSensorDevice.ControllerDetach(
							SensorDevice.Type.Accelerometer);

					SendControllerEvent(
						attach ? "attach" : "detach", identity,
						type);

				}
				else
				{

					Logger.Info(
						"Queueing controller attach event " +
						"{0} {1} {2}",
						identity, attach, type);

					if (attach)
						mControllerMap[identity] = type;
					else
						mControllerMap.Remove(identity);
				}
			});
		}

		public void HandleControllerGuidance(bool pressed, int identity,
			String type)
		{
			if (FrontendState != State.Connected)
				return;

			SendControllerEvent(
				pressed ? "guidance_pressed" : "guidance_released",
				identity, type);
		}

		private void SendControllerEvent(String name, int identity,
			String type)
		{
			String cmd = String.Format("controller_{0} {1} {2}",
				name, identity, type);

			SendControllerEventInternal(cmd, null);
		}

		private void SendControllerEventInternal(String cmd,
			UIHelper.Action continuation)
		{
			Logger.Info("Sending controller event " + cmd);

			Common.VmCmdHandler.RunCommandAsync(cmd, continuation,
				this);
		}

		private void SetupVmxChecker()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_CONFIG);

			int check = (int)key.GetValue("CheckVMX", 1);
			if (check != 0)
			{

				String title = "Closing BlueStacks app Player";
				String text = Locale.Strings.VMXError;

				VmxChecker checker = new VmxChecker(this, title,
					text);
				checker.Start();

			}
			else
			{

				Logger.Info("Skipping VMX check...");
			}

			key.Close();
		}

		public void OrientationHandler(int orientation)
		{
			Logger.Info("Got orientation change notification for {0}",
				orientation);

			/*
			 * By listening for orientation handler messages from
			 * the guest, we can rotate the guest display to emulate
			 * portrait mode on devices that don't rotate.
			 */

			bool emulate = ShouldEmulatePortraitMode();
			Logger.Info("ShouldEmulatePortraitMode => " + emulate);

			if (sCurrentOrientation == orientation)
			{
				Logger.Info("Not doing anything as current orientation is same as orientation requested");
				return;
			}
			sCurrentOrientation = orientation;

			if (orientation == 2 || orientation == 3)
				mRotateGuest180 = true;
			else
				mRotateGuest180 = false;

			if (emulate)
			{
				mEmulatedPortraitMode =
					orientation == 1 || orientation == 3;
				mRotateGuest180 =
					orientation == 2 || orientation == 3;
			}
			else
			{
				mEmulatedPortraitMode = false;
			}

			mInputMapper.SetDisplay(mEmulatedPortraitMode,
				mRotateGuest180);
			mSensorDevice.SetDisplay(mEmulatedPortraitMode,
				mRotateGuest180);

			SendOrPostCallback cb = new SendOrPostCallback(
				delegate(Object obj)
				{

					//we need to skip the resizeFrontendWindow() in case of GameManager
					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsResizeFrontendWindow)
					{
						ResizeFrontendWindow();
					}
					Logger.Info("Orientation handler calling fixguest");
					FixupGuestDisplay();
					if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
					{
					}
					else
					{
						OutputDebugString("SpawnApps:executeJavascript(\"orientationChanged();\")");
					}
				});

			try
			{
				this.mUiContext.Send(cb, null);
			}
			catch (Exception)
			{
			}
		}

		private void SendOrientationToGuest()
		{
			Logger.Info("Sending screen orientation to guest: {0}",
				SystemInformation.ScreenOrientation);

			String url = String.Format("http://127.0.0.1:{0}/{1}",
				Common.VmCmdHandler.s_ServerPort,
				Common.Strings.HostOrientationUrl);

			Dictionary<String, String> data =
				new Dictionary<String, String>();

			data.Add("data",
				SystemInformation.ScreenOrientation.ToString());

			/*
             * We cannot block here.  Do the HTTP post in a
             * background thread.
             */

			Thread thread = new Thread(delegate ()
			{

				try
				{
					Common.HTTP.Client.Post(url, data, null,
						false);

				}
				catch (Exception exc)
				{
					Logger.Error(
						"Cannot send orientation to guest: " +
						exc.ToString());
				}
			});

			thread.IsBackground = true;
			thread.Start();
		}

		private void InitConfig(String vmName)
		{
			String path = String.Format(Common.Strings.RegBasePath + @"\Guests\{0}\Config", vmName);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);

			this.grabKeyboard = (int)key.GetValue("GrabKeyboard", 1) != 0;
			this.frontendNoClose = (int)key.GetValue("FrontendNoClose", 0) != 0;
			this.stopZygoteOnClose = (int)key.GetValue("StopZygoteOnClose", 0) != 0;
			this.disableDwm = (int)key.GetValue("DisableDWM", 0) != 0;

			key.Close();

			Thread thr = new Thread(EnableConsoleAccessThreadEntry);
			thr.IsBackground = true;
			thr.Start();
		}

		private void EnableConsoleAccessThreadEntry()
		{
			String path = String.Format(
					Common.Strings.RegBasePath + @"\Guests\{0}\Config", vmName);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);

			while (true)
			{
				/*
                 * Try to make it marginally harder for someone using
                 * RegMon to determine which registry value is used to
                 * disable console lockdown by only reading the
                 * appropriate value if it exists.
                 */

				foreach (String name in key.GetValueNames())
				{
					String match = "EnableConsoleAccess";

					if (name != match)
						continue;

					/*
                     * Enable console access only if the SHA1
                     * digest of the magic string matches.  To
                     * enable console access, the magic string
                     * should read as follows, with correct
                     * capitalization and punctuation:
                     *
                     *     May I please have console access?
                     */

					String magic = (String)key.GetValue(name);
					String digest = ComputeSha1Digest(magic);

					this.lockdownDisabled = (digest ==
						"B4003D4A30C230EB82380DE4AA9697B967FC239F");
				}

				Thread.Sleep(1000);
			}
		}

		private String ComputeSha1Digest(String text)
		{
			SHA1 sha = new SHA1CryptoServiceProvider();
			Encoding enc = new UTF8Encoding(true, true);
			String digest = "";

			try
			{
				Byte[] raw = enc.GetBytes(text);
				Byte[] hash = sha.ComputeHash(raw);

				for (int ndx = 0; ndx < hash.Length; ndx++)
					digest += String.Format("{0:X2}", hash[ndx]);

			}
			catch (Exception exc)
			{
				Logger.Error("Cannot compute digest");
				Logger.Error(exc.ToString());

				return "";
			}

			return digest;
		}

		/*
         * Set the initial screen configuration using the frame buffer
         * settings in the registry.
         */
		private void InitScreen()
		{
			Logger.Info("InitScreen()");

			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
					Common.Strings.GuestRegKeyPath, this.vmName);
			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);

			if (sHideMode)
			{
				originalFullScreenState = (int)key.GetValue("FullScreen", 0) != 0;
				mFullScreen = false;
			}
			else
			{
				mFullScreen = (int)key.GetValue("FullScreen", 0) != 0;
			}

			key.Close();

			this.mConfiguredDisplaySize = GetConfiguredDisplaySize();
			this.mConfiguredGuestSize = GetConfiguredGuestSize();

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsFrontendBorderHidden)
				//this.FormBorderStyle = FormBorderStyle.None;

				ResizeFrontendWindow();
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
			}
			else
			{
				OutputDebugString("SpawnApps:executeJavascript(\"initScreenDone();\")");
			}
		}

		public static void UpdateTitle(string title)
		{
			if (!Features.UpdateFrontendAppTitle())
			{
				return;
			}

			string packageName;
			string activityName;
			string imageName;

			JsonParser.GetAppInfoFromAppName(title, out packageName, out imageName, out activityName);

			string iconFile = Path.Combine(Common.Strings.LibraryDir,
					String.Format("Icons\\{0}.{1}.ico", packageName, activityName));

			Common.Strings.AppTitle = title;
			Logger.Info("Setting new icon: " + iconFile);
			Icon appIcon = new System.Drawing.Icon(iconFile);

			s_Console.Text = title;
			//s_Console.Icon = appIcon;

			BringToFront(sVName);
		}

		private void InitControl()
		{
			this.DoubleBuffered = true;

			this.BackColor = Color.Black;
			this.ForeColor = Color.LightGray;
		}

		private bool HideBootProgress()
		{
			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
				Common.Strings.GuestRegKeyPath, this.vmName);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
			bool hide = (int)key.GetValue("HideBootProgress", 0) != 0;

			key.Close();
			return hide;
		}

		private bool ConnectingBlankEnabled()
		{
			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
				Common.Strings.GuestRegKeyPath, this.vmName);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
			bool val = (int)key.GetValue("ConnectingBlankEnabled", 0) != 0;

			key.Close();
			return val;
		}

		/*
         * State change routines.
         */

		public void StateExitCurrent()
		{
			string methodName = String.Format("StateExit{0}", FrontendState.ToString());
			Logger.Info("Invoking: " + methodName);
			MethodInfo method = this.GetType().GetMethod(methodName,
					BindingFlags.NonPublic | BindingFlags.Instance);
			method.Invoke(this, null);
		}

		private void StateExitInitial()
		{
			Logger.Info("Exiting state Initial");

			if (FrontendState != State.Initial)
				throw new SystemException("Illegal state " +
					FrontendState);
		}

		private void StateEnterStopped()
		{
			StateEnterStopped(Common.Strings.VMName);
		}

		private void StateEnterStopped(string vmName)
		{
			Logger.Info("Entering state Stopped");

			/* Kill the frontend if the service stops for any reason */
			if (FrontendState != State.Initial)
			{
				Logger.Info("Frontend unexpectedly reaached the stopped state. Exiting.");
				if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyChangesToParentWindow)
				{
					int exitCode = 9999;
					exitCode = StartBootFailureDebugging();
					WindowMessages.NotifyBootFailureToParentWindow(exitCode);
				}

				Environment.Exit(1);
			}

			this.ClearControls();

			/*
             * Set up a form that lets the user start the VM or
             * close the frontend.
             */

			int centerX = mScaledDisplayArea.Width / 2;
			int centerY = mScaledDisplayArea.Height / 2;

			/*
             * Reset other state.
             */

			mInputMapper.SetMonitor(null);

			if (mStateMachine.Monitor != null)
			{
				mStateMachine.Monitor.Close();
				mStateMachine.Monitor = null;
			}

			if (mStateMachine.Manager != null)
			{
				mStateMachine.Manager.Close();
				mStateMachine.Manager = null;
			}

			mStateMachine.Video = null;

			this.cannotStartVm = false;
			this.guestFinishedBooting = false;
			this.isGuestReady = false;
			this.guestHasStopped = false;

			/*
             * Setup a timer that polls the VM, trying to attach.
             * The timer can transition us to either the Connecting
             * or Connected states.
             */

			this.timer = new System.Windows.Forms.Timer();
			this.timer.Interval = 1000; /* one second */

			this.timer.Tick += delegate (Object obj, EventArgs evt)
			{
				if (mStateMachine.TryConnectVideo())
				{

					mInputMapper.SetMonitor(mStateMachine.Monitor);

					StateExitStopped();
					if (HideBootProgress())
						StateEnterConnecting();
					else
						StateEnterConnected();
				}
			};

			this.timer.Start();

			/*
             * Finally, set our new state.
             */

			FrontendState = State.Stopped;

			try
			{
				Process.Start(sInstallDir + "HD-Agent.exe");
			}
			catch (Exception)
			{
			}

			if (!this.userInteracted)
			{
				this.Paint += HandlePaintEvent;
				this.userInteracted = true;
			}
			String AndroidSrvName = Common.Strings.GetAndroidServiceName(vmName);
			ServiceController sc = new ServiceController(AndroidSrvName);
			Logger.Info("bsthdandroidsvc state is " + sc.Status);
			if (sc.Status == ServiceControllerStatus.Stopped ||
					sc.Status == ServiceControllerStatus.StopPending)
			{
				Logger.Info("Starting Service in normal mode");
				StartVmServiceAsync();
			}
			StateExitStopped();
			StateEnterStarting();
		}

		private void StateExitStopped()
		{
			Logger.Info("Exiting state Stopped");

			if (FrontendState != State.Stopped)
				throw new SystemException("Illegal state " +
					FrontendState);

			this.timer.Stop();
			this.timer.Dispose();
			this.timer = null;

			this.ClearControls();
		}

		private void ClearControls()
		{
			Logger.Info("Clearing controls");

			for (int i = this.Controls.Count - 1; i >= 0; i--)
			{
				Control control = this.Controls[i];
				if (this.atLoadingScreen && control == this.loadingScreen)
				{
					Logger.Info("Not clearing " + control.ToString());
					continue;
				}
				else
				{
					this.Controls.Remove(control);
					control = null;
				}
			}
		}

		private void RemoveLoadingScreen()
		{
			if (this.loadingScreen != null)
			{
				this.Controls.Remove(this.loadingScreen);
				this.loadingScreen.Controls.Clear();
				this.loadingScreen.Dispose();
				this.loadingScreen = null;
			}
		}

		private void AddLoadingScreen(string barType)
		{
			Logger.Info("AddLoadingScreen: " + barType);

			if (this.Controls.Contains(this.loadingScreen))
			{
				Logger.Info("Already added");
				return;
			}

			Logger.Info("In Loading Screen");
			Size loadingScreenSize = new Size();

			Logger.Info("In Loading Screen");
			loadingScreenSize.Height = this.ClientSize.Height;
			loadingScreenSize.Width = this.ClientSize.Width;

			Logger.Info("In Loading Screen");
			Point loadingScreenLocation = new Point(0, 0);

			Logger.Info("In Loading Screen");
			if (Features.IsFeatureEnabled(Features.SHOW_TOGGLE_BUTTON_IN_LOADING_SCREEN))
				this.loadingScreen = new LoadingScreen(loadingScreenLocation, loadingScreenSize, sAppIconImage, sAppName, barType, ToggleFullScreen);
			else
				this.loadingScreen = new LoadingScreen(loadingScreenLocation, loadingScreenSize, sAppIconImage, sAppName, barType, null);

			Logger.Info("In Loading Screen");
			this.SuspendLayout();
			this.Controls.Add(this.loadingScreen);

			if (sAppName == "")
				this.loadingScreen.SetStatusText(Locale.Strings.Initializing);
			else
				this.loadingScreen.SetStatusText(Locale.Strings.InitializingGame);
			this.ResumeLayout();
		}

		private void StateEnterStarting()
		{
			Logger.Info("Entering state Starting");

			AddLoadingScreen("Marquee");

			/*
             * Setup a timer that checks to make sure that we didn't
             * fail while starting the VM and tries to attach.
             */

			this.timer = new System.Windows.Forms.Timer();
			this.timer.Interval = 1000; /* one second */

			this.timer.Tick += delegate (Object obj, EventArgs evt)
			{
				if (this.cannotStartVm || this.glInitFailed)
				{

					StateExitStarting();
					StateEnterCannotStart();

				}
				else if (mStateMachine.TryConnectVideo())
				{
					mInputMapper.SetMonitor(mStateMachine.Monitor);

					if (HideBootProgress())
						this.atLoadingScreen = true;
					StateExitStarting();
					if (HideBootProgress())
						StateEnterConnecting();
					else
						StateEnterConnected();
				}
			};

			this.timer.Start();

			/*
             * Finally, set our new state.
             */

			FrontendState = State.Starting;
		}

		private void StateExitStarting()
		{
			Logger.Info("Exiting state Starting");

			if (FrontendState != State.Starting)
				throw new SystemException("Illegal state " +
					FrontendState);

			this.timer.Stop();
			this.timer.Dispose();
			this.timer = null;

			this.ClearControls();
		}

		private void StateEnterCannotStart()
		{
			Logger.Info("Entering state CannotStart");

			int centerX = mScaledDisplayArea.Width / 2;
			int centerY = mScaledDisplayArea.Height / 2;

			Image splashLogoImage = new Bitmap(
				Console.sInstallDir + "ProductLogo.png");
			splashLogoImage = new Bitmap(
				splashLogoImage, new Size(128, 128));

			Label logo = new Label();
			logo.BackgroundImage = splashLogoImage;
			logo.Width = logo.BackgroundImage.Width;
			logo.Height = logo.BackgroundImage.Height;
			logo.Location = new Point(centerX - logo.Width / 2,
				centerY - logo.Height + 70);

			Label blurb = new StatusMessage();
			blurb.Text = Locale.Strings.CanNotStart;
			blurb.TextAlign = ContentAlignment.MiddleCenter;
			blurb.Width = mScaledDisplayArea.Width;
			blurb.Location = new Point(0, centerY + 90);

			Button exitButton = new FrontendButton();
			exitButton.Text = "Exit";
			exitButton.Location = new Point(
				centerX - exitButton.Width / 2, centerY + 180);

			exitButton.Click += delegate (Object obj, EventArgs evt)
			{
				OnFrontendClose();
			};

			this.Controls.Add(logo);
			this.Controls.Add(blurb);
			this.Controls.Add(exitButton);

			FrontendState = State.CannotStart;
		}

		private void StateEnterConnectingBlank()
		{
			Logger.Info("Entering state ConnectingBlank");

			/*
             * Don't add anything visible to the form, as this state
             * is supposed to be stealthy.
             */

			/*
             * Setup a timer that checks to see if the VM has
             * finished booting.
             */

			int retryCount = 10;

			this.timer = new System.Windows.Forms.Timer();
			this.timer.Interval = 100;  /* one tenth of a second */

			this.timer.Tick += delegate (Object obj, EventArgs evt)
			{

				/*
                 * If we finished booting, then go to the
                 * connected state.
                 */

				if (this.guestFinishedBooting)
				{
					StateExitConnectingBlank();
					StateEnterConnected();
				}

				/*
                 * If we timeout before the VM is finished
                 * booting, then move to the connecting
                 * state.
                 */

				if (retryCount-- == 0)
				{
					StateExitConnectingBlank();
					StateEnterConnecting();
				}
			};

			this.guestFinishedBooting = false;
			this.isGuestReady = false;
			this.timer.Start();

			/*
             * Finally, reflect the state change.
             */

			FrontendState = State.ConnectingBlank;
		}

		private void StateExitConnectingBlank()
		{
			Logger.Info("Exiting state ConnectingBlank");

			if (FrontendState != State.ConnectingBlank)
				throw new SystemException("Illegal state " +
					FrontendState);

			this.timer.Stop();
			this.timer.Dispose();
			this.timer = null;

			this.ClearControls();
		}

		private void StateEnterConnecting()
		{
			Logger.Info("Entering state Connecting");

			AddLoadingScreen("Marquee");

			/*
             * Setup a timer that checks to see the VM has finished
             * booting.
             */

			this.timer = new System.Windows.Forms.Timer();
			this.timer.Interval = 250;  /* quarter second */

			this.timer.Tick += delegate (Object obj, EventArgs evt)
			{
				if (this.glInitFailed)
				{
					StateExitConnecting();
					StateEnterCannotStart();
				}
				else if (this.guestHasStopped)
				{
					StateExitConnecting();
					StateEnterStopped();
				}
				else if (this.guestFinishedBooting)
				{
					StateExitConnecting();
					Common.Stats.SendBootStats("frontend", true, false);
#if BUILD_HYBRID
					ThreadPool.QueueUserWorkItem(delegate(Object stateInfo)
							{
							CheckVtxAndShowPopup();
							});
#endif
					StateEnterConnected();
				}
			};

			this.guestFinishedBooting = false;
			this.isGuestReady = false;
			this.timer.Start();

			/*
             * Unwedge any stuck modifier keys.
             */

			UnstickKeyboardModifiers();

			/*
             * Send state of keys like caps lock
             */


			/*
             * Finally, reflect the state change.
             */

			FrontendState = State.Connecting;
		}

		private void StateExitConnecting()
		{
			Logger.Info("Exiting state Connecting");

			if (FrontendState != State.Connecting)
				throw new SystemException("Illegal state " +
					FrontendState);

			this.timer.Stop();
			this.timer.Dispose();
			this.timer = null;
			this.atLoadingScreen = false;

			this.ClearControls();
		}

		private void PushModelJsonIfNeeded()
		{
			Logger.Info("In PushModelJsonIfNeeded");
			string modelFile = Path.Combine(Common.Strings.GadgetDir, Common.Strings.Four399ModelJsonFileName);
			if (File.Exists(modelFile))
			{
				int adbPort = Utils.GetAdbPort();
				String prog = Path.Combine(sInstallDir, "HD-Adb.exe");

				Common.Utils.RunCmdAsync(prog, "start-server");
				Thread.Sleep(5000);

				Common.Utils.RunCmd(prog, string.Format("connect localhost:{0}", adbPort), null);
				Utils.CmdRes res = Common.Utils.RunCmd(prog, string.Format("-s localhost:{0} push \"" + modelFile + "\" /mnt/sdcard/model.json", adbPort), null);
				if (res.StdErr.Trim().Length == 0 ||
						res.StdErr.Trim().IndexOf("KB/s") != -1)
				{
					File.Delete(modelFile);
				}
			}
			else
			{
				Logger.Info("Model file not present. Seems like already pushed");
			}
		}

		private void StateEnterConnected()
		{
			Logger.Info("Entering state Connected");

			/*
             * One-time sync of config of the Host with the VM
             */
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			int configSynced = (int)key.GetValue("ConfigSynced", 0);
			if (configSynced == 0)
			{
				Logger.Info("Config not synced. Syncing now.");
				Thread configSyncer = new Thread(delegate ()
				{
					string parserVersion = InputMapper.GetKeyMappingParserVersion();
					VmCmdHandler.SyncConfig(parserVersion);
					VmCmdHandler.SetKeyboard(IsDesktop());
				});

				configSyncer.IsBackground = true;
				configSyncer.Start();
			}
			else
				Logger.Info("Config already synced.");

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsJsonModelToBePushed)
			{
				PushModelJsonIfNeeded();
			}
			/*
             * Send fqdn of the Host with the VM 
             * This is required to handle restart case. Agent will not send
             * port again on restart, so this needs to be updated from here.
             */
			Thread fqdnSender = new Thread(delegate ()
			{
				Logger.Info("Started fqdnSender thread");
				VmCmdHandler.FqdnSend(0, "Agent");
				Logger.Info("fqdnSender thread exiting");
			});

			fqdnSender.IsBackground = true;
			fqdnSender.Start();

			RemoveLoadingScreen();
			if (sAppIconImage != null)
			{
				sAppIconImage.Dispose();
				sAppIconImage = null;
			}

			Logger.Debug("Raising Layout event");
			OnLayout(new LayoutEventArgs(this, ""));
			SendOrientationToGuest();

			Thread machineTypeSyncer = new Thread(delegate ()
			{
				Common.VmCmdHandler.SetMachineType(IsDesktop());
			});

			machineTypeSyncer.IsBackground = true;
			machineTypeSyncer.Start();

			this.userInteracted = true;

			if (!Interop.Opengl.IsSubWindowVisible())
			{
				Logger.Info("Showing subwindow");
				this.glWindowAction = GlWindowAction.Show;
				this.userInteracted = false;
			}

			FixupGuestDisplay();

			/*
             * Start the display timer.
             */

			this.timer = new System.Windows.Forms.Timer();
			this.timer.Interval = 1000 / 30;

			this.timer.Tick += delegate (Object obj, EventArgs evt)
			{
				if (this.guestHasStopped)
				{
					StateExitConnected();
					StateEnterStopped();
				}
				else
				{
					HandleDisplayTimeout();
				}
			};

			this.guestHasStopped = false;
			this.timer.Start();

			/*
             * Initialize our virtual devices before wiring up the
             * event handlers.  Otherwise, we have a race window
             * where event handlers that poke at these devices can
             * throw null reference exceptions.
             */

			AudioAttach();
			GpsAttach();
			mSensorDevice.Start(this.vmName);
			CameraAttach();

			/*
             * Wire up our event handlers.
             */


			//this.Activated += HandleActivatedEvent;
			//this.Deactivate += HandleDeactivateEvent;

			this.MouseMove += HandleMouseMove;
			this.MouseDown += HandleMouseDown;
			this.MouseUp += HandleMouseUp;

			this.MouseWheel += HandleMouseWheel;

			this.TouchEvent += HandleTouchEvent;

			this.KeyDown += HandleKeyDown;
			this.KeyUp += HandleKeyUp;

			/*
             * Start a thread which keeps checking if the guest has finished booting
             */

			if (!this.checkingIfBooted)
				CheckIfGuestFinishedBooting();

			/*
             * Process any pending controller attach events.
             */

			SendControllerEventInternal("controller_flush", delegate ()
			{

				foreach (int identity in mControllerMap.Keys)
				{

					mSensorDevice.ControllerAttach(
						SensorDevice.Type.Accelerometer);

					SendControllerEvent("attach", identity,
						mControllerMap[identity]);
				}

				mControllerMap.Clear();
			});

			/*
             * Finally, reflect our state change.
             */
			FrontendState = State.Connected;

			if (sFrontendActive)
			{
				Logger.Info("Frontend active");
				DoFrontendReadyAction();
				frontendReadyActionDone = true;
			}
			else
			{
				Logger.Info("Frontend Inactive");
			}

		}

		private void DoFrontendReadyAction()
		{
			if (FrontendState != State.Connected)
				return;

			Thread blackScreenChecker = new Thread(delegate ()
			{
				Logger.Info("Checking for Black Screen Error");

				CheckBlackScreenAndRestartGMifOccurs();
			});
			blackScreenChecker.IsBackground = true;
			blackScreenChecker.Start();
		}

		private void CheckBlackScreenAndRestartGMifOccurs()
		{
			int count = 0;
			bool gamemanager = (Oem.Instance == null) ? false : Oem.Instance.IsGameManagerToBeRestartedOnBlackScreen;
			while (CheckBlackScreen() && count < 300)
			{
				count += 1;
				Thread.Sleep(1000);
			}
			if (count >= 300 && gamemanager)
			{
				Logger.Info("Black occurs for 5 mins");

				DialogResult result = MessageBox.Show(Locale.Strings.TROUBLESHOOTER_TEXT,
						Locale.Strings.BLACKSCREEN_FORM_TEXT, MessageBoxButtons.OKCancel);

				if (result == DialogResult.OK)
				{
					Logger.Info("User click Yes, Restartig GameManager");
					Utils.KillProcessByName("BlueStacks");
					Process.Start(Utils.GetPartnerExecutablePath());
				}
				else
				{
					Logger.Info("User clicked No");
				}
			}
			else
			{
				Logger.Info("Frontend launched Successfully");
				Common.Stats.SendFrontendStatusUpdate("frontend-ready");
				Common.Stats.SendHomeScreenDisplayedStats();
			}
		}

		public bool CheckBlackScreen()
		{
			try
			{
				Logger.Debug("Inside CheckBlackScreen");
				Size snapShotSize = new Size((int)(this.ClientSize.Width * 0.993), (int)(this.ClientSize.Height * 0.957));
				double chromeWidthPercent = 0.0035;
				double chromeHeightPercent = 0.0425;
				Bitmap bmp = new Bitmap(this.ClientSize.Width, this.ClientSize.Height);
				using (Graphics g = Graphics.FromImage(bmp))
				{
					g.CopyFromScreen(
							new Point((int)(this.Left + (this.Width * chromeWidthPercent)), (int)(this.Top + (this.Height * chromeHeightPercent))),
							Point.Empty,
							snapShotSize
							);
					Color pixClr;
					for (int x = 0; x < snapShotSize.Width; x++)
					{
						for (int y = 0; y < snapShotSize.Height; y++)
						{
							pixClr = bmp.GetPixel(x, y);
							if (pixClr.A != Color.Black.A ||
									pixClr.R != Color.Black.R ||
									pixClr.G != Color.Black.G ||
									pixClr.B != Color.Black.B)
							{
								Logger.Info(string.Format("Pixel {0},{1} is not black", x, y));
								bmp.Dispose();
								return false;
							}
						}
					}
					bmp.Dispose();
					Logger.Error("Black Screen Detected");
					return true;
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				return false;
			}
		}

		private void CheckVtxAndShowPopup()
		{
			Logger.Info("In CheckVtxAndShowPopup");
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath, true);
			int vtxDisabled = (int) key.GetValue("VtxDisabled", 0);
			int systemInfoStats2 = (int) key.GetValue("SystemInfoStats2", 0);
			if (systemInfoStats2 == 1)
			{
				//send stats
				String deviceCaps = (String) key.GetValue("DeviceCaps", "");
				if (!deviceCaps.Equals(""))
				{
					Logger.Info("Sending DeviceCaps stats");
					Dictionary<string, string> data = new Dictionary<string, string>();
					Logger.Info("DeviceCaps: " + deviceCaps);
					data.Add("data", deviceCaps);
					try
					{
						Common.HTTP.Client.Post(Common.Utils.HostUrl + "/stats/systeminfostats2",
								data, null, false);
						key.DeleteValue("SystemInfoStats2");
					}
					catch (Exception ex)
					{
						Logger.Error(ex.ToString());
					}
				}
			}
			if (vtxDisabled == 1)
			{
				Logger.Info("User shown vtx enable popup");

				Dictionary<String, String> data = new Dictionary<String, String>();
				data.Add("url", "http://bluestacks-cloud.appspot.com/performance_with_vt");
				data.Add("title", "enablevt");
				SendGameManagerRequest(data, Common.Strings.ShowEnableVtPopupUrl);

				try
				{
					Common.Stats.SendMiscellaneousStatsSync("EnableVtx",
							null, User.GUID, "Enable vt popup shown", null, null);
				}
				catch (Exception e)
				{
					Logger.Info("Unable to send enablevtx stats: {0}", e.Message);
				}
			}
			key.Close();
		}

		public void CheckIfGuestReady()
		{
			Logger.Info("In CheckIfGuestReady");

			Thread guestReadyThread = new Thread(delegate ()
			{
				this.checkingIfGuestReady = true;
				int retries = 90;
				while (retries > 0)
				{
					retries--;
					try
					{
						string url = String.Format("http://127.0.0.1:{0}/{1}",
							Common.VmCmdHandler.s_ServerPort, Common.VmCmdHandler.s_CheckGuestReadyPath);
						string r = Common.HTTP.Client.Get(url, null, false, 1000);

						IJSonReader json = new JSonReader();
						IJSonObject res = json.ReadAsJSonObject(r);
						string sReceived = res["result"].StringValue;
						if (sReceived.Equals("ok"))
						{
							this.isGuestReady = true;
							Logger.Info("guest finished complete booting,sendlockedkeys now");
							SendLockedKeys();
							break;
						}

						Thread.Sleep(1000);
					}
					catch (Exception e)
					{
						Logger.Error("Guest not completely booted yet. err: " + e.Message);
						Thread.Sleep(1000);
					}
				}
				this.checkingIfGuestReady = false;
			});
			guestReadyThread.IsBackground = true;
			guestReadyThread.Start();
		}
		public void CheckIfGuestFinishedBooting()
		{
			Logger.Info("in CheckIfGuestFinishedBooting");

			Thread guestFinishedBootingThread = new Thread(delegate ()
				{
					bool logSent = false;
					this.checkingIfBooted = true;
					int retries = 90;

					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsUseExtraBootRetries)
					{
						retries = 150;
						RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
						bool firstBoot = ((int)key.GetValue("ConfigSynced", 0)) == 0 ? true : false;

						if (firstBoot == true)
							retries = 240;
					}

					if (Common.Utils.WaitForBootComplete(retries) == true)
					{
						SignalReady(vmName);
						if (!sAppLaunch)
						{
							this.guestFinishedBooting = true;

							if (!this.checkingIfGuestReady && !this.isGuestReady)
							{
								CheckIfGuestReady();
							}
						}
						/*
                           Thread dumpStateThread = new Thread(delegate() {
                           CollectDumpState("Loaded");
                           });
                           dumpStateThread.IsBackground = true;
                           dumpStateThread.Start();

                           if(!dumpStateThread.Join(15000))
                           dumpStateThread.Abort();
                        */
					}
					else
					{
						RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
						if (key.GetValue("EnableConsoleAccess") != null)
						{
							Logger.Info("EnableConsoleAccess present. Not Aborting.");
							retries = 150;
						}
						else
						{
							Logger.Error("Android could not be started. Collecting dump state.");
							retries = 150;
							Common.Stats.SendBootStats("frontend", false, true);

							Thread dumpStateThread = new Thread(delegate ()
							{
								CollectDumpState("NoLoad");
							});
							dumpStateThread.IsBackground = true;
							dumpStateThread.Start();

							if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyChangesToParentWindow &&
									logSent == false)
							{
								int exitCode = 9999;
								exitCode = StartBootFailureDebugging();
								logSent = true;
								WindowMessages.NotifyBootFailureToParentWindow(exitCode);
							}

							// Quit();
						}
					}

					this.checkingIfBooted = false;
				});
			guestFinishedBootingThread.IsBackground = true;
			guestFinishedBootingThread.Start();
		}

		private int StartBootFailureDebugging()
		{
			string reason = "";
			int exitCode = 9999;
			try
			{
				Logger.Info("In StartBootFailureDebugging");
				if (Utils.IsBootFailureReasonknown(sFrontendLaunchTime, out reason, out exitCode) == true)
				{
					Logger.Info("Reason for not being able to boot: {0}", reason);
					bool logsSent = Utils.CheckIfErrorLogsAlreadySent(Common.Strings.BootFailureCategory, exitCode);
					if (logsSent == false)
					{
						Process.Start(Path.Combine(sInstallDir, "HD-LogCollector.exe"),
								string.Format("-boot \"{0}\" {1}", reason, exitCode));
					}
				}
				else
				{
					Logger.Info("Boot Failure reason not found, will send logs");
					Process.Start(Path.Combine(sInstallDir, "HD-LogCollector.exe"), "-boot");
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error occured in StartBootFailureDebugging, Err: {0}", e.ToString());
			}
			return exitCode;
		}

		private void SignalReady(String vmName)
		{
			String evtName = String.Format("Global\\BlueStacks_Frontend_Ready_{0}",
					vmName);

			bool created;
			glReadyEvent = new EventWaitHandle(false,
					EventResetMode.ManualReset, evtName, out created);
			Logger.Info("Event created: " + created);
			bool set = glReadyEvent.Set();
			Logger.Info("Event set: " + set);
		}

		private void CollectDumpState(string state)
		{
			try
			{
				int adbPort = Utils.GetAdbPort();
				String logDir = Path.Combine(Common.Strings.BstCommonAppData, "Logs");
				String logFileName = String.Format("Frontend-{0}-Android-DumpState.log", state);

				String prog = Path.Combine(sInstallDir, "HD-Adb.exe");

				Common.Utils.RunCmdAsync(prog, "start-server");
				Thread.Sleep(5000);

				Common.Utils.RunCmd(prog, string.Format("connect localhost:{0}", adbPort), null);
				Common.Utils.RunCmdNoLog(prog, string.Format("-s localhost:{0} shell dumpstate", adbPort),
						Path.Combine(logDir, logFileName));
			}
			catch (Exception ex)
			{
				Logger.Info("caught exception in collecting dumpstate ex : {0}", ex.ToString());
			}
		}

		private void StateExitConnected()
		{
			Logger.Info("Exiting state Connected");

			this.glWindowAction = GlWindowAction.Hide;

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				if (FrontendState != State.Connected)
					throw new SystemException("Illegal state " +
						FrontendState);
			}
			else
			{
				if (FrontendState != State.Quitting)
					throw new SystemException("Illegal state " +
						FrontendState);
			}
			/*
             * Shutdown Audio
             */
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				AudioDetach();
			}

			/*
             * Unplug Gps
             */
			GpsDetach();

			try
			{
				mSensorDevice.Stop();
			}
			catch (Exception ex)
			{
				Logger.Info(ex.Message);
			}

			/*
             * Detach Camera
             */
			try
			{
				CameraDetach();
			}
			catch (Exception ex)
			{
				Logger.Info(ex.Message);
			}

			/*
             * Unwire event handlers.
             */

			this.Layout -= HandleLayoutEvent;
			this.Paint -= HandlePaintEvent;

			this.MouseMove -= HandleMouseMove;
			this.MouseDown -= HandleMouseDown;
			this.MouseUp -= HandleMouseUp;
			this.MouseWheel -= HandleMouseWheel;

			this.TouchEvent -= HandleTouchEvent;

			this.KeyDown -= HandleKeyDown;
			this.KeyUp -= HandleKeyUp;

			/*
             * Stop our display timer.
             */

			try
			{
				this.timer.Stop();
				this.timer.Dispose();
				this.timer = null;
			}
			catch (Exception ex)
			{
				Logger.Info(ex.Message);
			}

			/*
             * Close our monitor and manager.
             */

			mStateMachine.Monitor.Close();
			mStateMachine.Monitor = null;
			mStateMachine.Video = null;

			mStateMachine.Manager.Close();
			mStateMachine.Manager = null;

			/*
             * Clear the controls associated with this state.
             */

			this.ClearControls();
			this.Invalidate();
		}

		private void StartVmServiceAsync()
		{
			/*
             * Make the service control manager call from a separate
             * thread.
             */

			Thread thread = new Thread(delegate ()
			{
				lock (lockObject)
				{
					Logger.Info("Starting VM service for {0}",
						this.vmName);

					try
					{
						StartVmService();
					}
					catch (InvalidOperationException exc)
					{
						/*
                         * Our registry symlink breaks when Windows is upgraded from 8.0 to 8.1 and
                         * we get this exception because we unable to start BstHdDrv
                         * In case we get this exception, launch a 64-bit binary which will recreate the symlink
                         */
						Logger.Error("Caught InvalidOperationException");
						Logger.Error(exc.ToString());

						if (Features.IsFeatureEnabled(Features.MULTI_INSTANCE_SUPPORT) && otherInstanceServiceStopDone == false)
						{
							Logger.Info("This is China Oem, other instance services might be running, wait for them to stop");
							otherInstanceServiceStopDone = true;
							WaitOtherInstanceServiceStop();
							StartVmServiceAsync();
							return;
						}
						Logger.Info("Displaying Message box");
						DialogResult res = MessageBox.Show(Locale.Strings.SystemUpgradedError,
							"BlueStacks Installer",
							MessageBoxButtons.OKCancel,
							MessageBoxIcon.Error,
							MessageBoxDefaultButton.Button1);
						Logger.Info("Displayed Message box");
						if (res == DialogResult.OK)
						{
							Process proc = new Process();
							proc.StartInfo.UseShellExecute = true;
							proc.StartInfo.CreateNoWindow = true;
							proc.StartInfo.FileName = Path.Combine(sInstallDir, "HD-CreateSymlink.exe");
							proc.StartInfo.Arguments = "BlueStacks";

							if (!Common.Utils.IsOSWinXP())
							{
								proc.StartInfo.Verb = "runas";
							}

							Logger.Info("Starting {0} with args {1}", proc.StartInfo.FileName, proc.StartInfo.Arguments);
							proc.Start();
							proc.WaitForExit();
							Logger.Info("HD-CreateSymlink done");
							StartVmServiceAsync();
						}
						else
						{
							Logger.Info("User chose not to fix installation. Exiting.");
							Environment.Exit(-1);
						}
					}
					catch (Exception exc)
					{
						Logger.Error("Cannot start VM service for {0}",
							this.vmName);
						Logger.Error(exc.ToString());
						this.cannotStartVm = true;
					}
				}
			});

			this.cannotStartVm = false;
			thread.IsBackground = true;
			thread.Start();
		}

		/*
         * XXXDPR:  This belongs in the frontend state machine.
         */
		private void WaitOtherInstanceServiceStop()
		{
			Logger.Info("Wait For android service of other instance to stop");
			string bluestacksAndroidServicesPrefix = Common.Strings.AndroidServiceName;
			ServiceController[] services = ServiceController.GetServices();
			SearchAndWaitForServiceStop(services, bluestacksAndroidServicesPrefix);
			ServiceController[] drvServices = ServiceController.GetDevices();
			string bluestacksDriverServicePrefix = "BstHdDrv";
			SearchAndWaitForServiceStop(drvServices, bluestacksDriverServicePrefix);
			Logger.Info("Wait for android service of other instance to stop done");
		}

		/*
         * XXXDPR:  This belongs in the frontend state machine.
         */
		private static void SearchAndWaitForServiceStop(ServiceController[] services, string servicePrefix)
		{
			foreach (ServiceController service in services)
			{
				string serviceName = service.ServiceName;
				if (serviceName.StartsWith(servicePrefix, true, null))
				{
					if (Common.Strings.OEM.Equals(""))
					{
						if (serviceName.Contains("_"))
						{
							if (service.Status == ServiceControllerStatus.Running)
							{
								Logger.Info("serviceName: {0} found running", serviceName);
								try
								{
									service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
								}
								catch (Exception ex)
								{
									Logger.Error("Got exception , ex : {0}", ex.ToString());
								}
								Logger.Info("ServiceName : {0} is {1}", serviceName, service.Status.ToString());
							}
						}
					}
					else
					{
						if (!serviceName.EndsWith(Common.Strings.OEM, true, null))
						{
							if (service.Status == ServiceControllerStatus.Running)
							{
								Logger.Info("serviceName: {0} found running", serviceName);
								try
								{
									service.WaitForStatus(ServiceControllerStatus.Stopped, TimeSpan.FromSeconds(10));
								}
								catch (Exception ex)
								{
									Logger.Error("Got exception , ex : {0}", ex.ToString());
								}
								Logger.Info("ServiceName : {0} is {1}", serviceName, service.Status.ToString());
							}
						}
					}
				}
			}
		}

		private void StartLogRotatorServiceAsync()
		{
			Thread thread = new Thread(delegate ()
			{
				Logger.Info("Starting logrotator service");

				try
				{
					StartLogRotatorService();
				}
				catch (Exception exc)
				{
					Logger.Error("Failed to start logrotator service");
					Logger.Error(exc.ToString());
				}
			});

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				this.cannotStartVm = false;
			}
			thread.IsBackground = true;
			thread.Start();
		}
		private void StartLogRotatorService()
		{
			string serviceName = Common.Strings.BstLogRotatorServiceName;
			StartService(serviceName, "auto");
		}
		private void StartVmService()
		{
#if BUILD_HYBRID
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				/*
                 * bug fix for message box your system seems to be ugraded ...
                 * Since issue is intermittent so running this code only for build hybrid for qa
                 * testing later can be included in hd builds also
                 */
				try
				{
					ServiceController sc = new ServiceController(Common.Strings.BstHypervisorDrvName);

					if (sc.Status == ServiceControllerStatus.StopPending)
					{
						Logger.Info("Waiting for {0} driver to stop", Common.Strings.BstHypervisorDrvName);
						sc.WaitForStatus(ServiceControllerStatus.Stopped);
					}
				}
				catch (Exception e)
				{
					Logger.Error("Error in checking status of driver");
					Logger.Error(e.ToString());
				}
                Common.Utils.SetGlTransportValue("0");
				RegistryKey key = Registry.LocalMachine.OpenSubKey(
						Common.Strings.HKLMConfigRegKeyPath, true);
				try
				{
					key.DeleteValue("VtxDisabled");
				}
				catch(Exception e)
				{
					Logger.Info("VtxDisabled registry does not exist");
				}
				String deviceCaps = (String) key.GetValue("DeviceCaps", "");
				key.Close();
				if (!deviceCaps.Equals(""))
				{
					JSonReader jsonReader = new JSonReader();
					IJSonObject obj = jsonReader.ReadAsJSonObject(deviceCaps);
					Logger.Info("OldEngineEnabled: {0}", obj["engine_enabled"].StringValue);
					if (!obj["engine_enabled"].StringValue.Equals("legacy"))
					{
						Common.Utils.SetDeviceCapsRegistry("Hardcoded engine as legacy",
								"legacy", false, false);
					}
				}
			}
#endif

			string serviceName = Common.Strings.AndroidServiceName;
			StartService(serviceName, "demand");
		}

		private void StartService(string serviceName, string startType)
		{
			ServiceController sc;

			/*
			 * If the service is currently stopping, wait for it
			 * before we try starting it again.
			 */

			sc = new ServiceController(serviceName);
			if (sc.Status == ServiceControllerStatus.StopPending)
				sc.WaitForStatus(ServiceControllerStatus.Stopped);

			/*
			 * Now that we are sure the service is stopped, go
			 * ahead and start it.
			 */
			Utils.StartServiceIgnoreAlreadyRunningException(sc, serviceName, startType);
		}

		private void StopService(string serviceName)
		{
			ServiceController sc;

			/*
			 * If the service is currently starting, wait for it
			 * to start before we try stopping it.
			 */

			sc = new ServiceController(serviceName);
			if (sc.Status == ServiceControllerStatus.StartPending)
				sc.WaitForStatus(ServiceControllerStatus.Running);

			/*
			 * Now that we are sure the service is running, go
			 * ahead and stop it.
			 */

			try
			{
				sc.Stop();
			}
			catch (Exception exc)
			{
				sc.Refresh();
				if (sc.Status != ServiceControllerStatus.Stopped &&
						sc.Status != ServiceControllerStatus.StopPending)
				{
					Logger.Error("Failed to stop {0}", serviceName);
					Logger.Error("{0} status = {1}", serviceName, sc.Status);
					Logger.Error(exc.ToString());
				}
				else
				{
					Logger.Info("{0} is already stopped", serviceName);
				}
			}
		}

		private void AudioAttach()
		{
			Thread audioThread = new Thread(delegate ()
					{
					Logger.Info("AudioAttach");

					try
					{
					Audio.Manager.Monitor = mStateMachine.Monitor;
					Audio.Manager.Main(
						new String[] { this.vmName });
					}
					catch (Exception exc)
					{
					Logger.Error(exc.ToString());
					}
					});

			audioThread.Priority = ThreadPriority.Highest;
			audioThread.IsBackground = true;
			audioThread.Start();
		}

		private void AudioDetach()
		{
			Logger.Info("AudioDetach");
			Audio.Manager.Shutdown();
		}

		private void GpsAttach()
		{
			Thread GpsThread = new Thread(delegate ()
					{
					Logger.Info("GpsAttach");

					try
					{
					Gps.Manager.Main(
						new String[] { this.vmName });
					}
					catch (Exception exc)
					{
					Logger.Error(exc.ToString());
					}
					});

			GpsThread.IsBackground = true;
			GpsThread.Start();
		}

		private void GpsDetach()
		{
			Logger.Info("GpsDetach");
			Gps.Manager.Shutdown();
		}

		private void CameraAttach()
		{
			if (camManager != null)
			{
				Logger.Info("cam Manager is already attached");
				return;
			}

			camManager = new VideoCapture.Manager();
			Logger.Info("CameraAttach");
			try
			{
				VideoCapture.Manager.Monitor = mStateMachine.Monitor;
				camManager.InitCamera(new String[] { this.vmName });
			}
			catch (Exception exc)
			{
				Logger.Error(exc.ToString());
			}
		}

		private void CameraDetach()
		{
			if (camManager == null)
			{
				Logger.Info("Cannot detach camera, which is not yet attached");
				return;
			}
			Logger.Info("CameraDetach");
			camManager.Shutdown();
			camManager = null;
		}

		private void HandleDisplayTimeout()
		{
			//Logger.Info("HandleDisplayTimeout");

			/*
			 * Check if we have any deferred OpenGL window actions to
			 * perform.
			 */

			if (this.glWindowAction == GlWindowAction.Show)
			{

				Logger.Info("Showing subwindow");
				if (Interop.Opengl.ShowSubWindow())
					this.glWindowAction = GlWindowAction.None;

			}
			else if (this.glWindowAction == GlWindowAction.Hide)
			{

				Logger.Info("Hiding subwindow");
				if (Interop.Opengl.HideSubWindow())
					this.glWindowAction = GlWindowAction.None;
			}

			/*
			 * Get the current video mode and compare it to the
			 * guest size to see if the screen dimensions changed.
			 */
			Interop.Video.Mode mode;
			mode = mStateMachine.Video.GetMode();


			/*
			 * Short circuit if a mode hasn't been set yet.
			 */

			if (mode.Width == 0 || mode.Height == 0)
				return;

			bool changed =
				(mode.Width != this.mConfiguredGuestSize.Width) ||
				(mode.Height != this.mConfiguredGuestSize.Height) ||
				this.forceVideoModeChange;
			if (changed)
			{
				Logger.Info("mode changed to ({0},{1})", mode.Width, mode.Height);
			}

			this.forceVideoModeChange = false;

			/*
			 * Adjust the window if the video mode changed.
			 */

			if (changed)
			{
				this.mConfiguredGuestSize.Width = mode.Width;
				this.mConfiguredGuestSize.Height = mode.Height;
			}

			/*
			 * Invalidate the window if either there was a frame
			 * buffer update or the screen size changed.
			 */
			bool dirty = mStateMachine.Video.GetAndClearDirty();

			if (dirty || changed)
			{
				this.Invalidate();
				this.Update();
			}

		}

		private void HandleLayoutEvent(Object o, EventArgs e)
		{
			Logger.Info("HandleLayoutEvent()");
			Logger.Info("New client size is {0}x{1}", this.ClientSize.Width,
					this.ClientSize.Height);

			if (this.ParentForm !=null && this.ParentForm.WindowState == FormWindowState.Minimized)
			{
				this.frontendMinimized = true;
				isMinimized = true;
			}
			else
			{
				sReLayoutInProgress = true;
				FixupGuestDisplay();
				FrontendActivated();
				isMinimized = false;
				sReLayoutInProgress = false;
			}
			if (this.Controls.Contains(this.loadingScreen))
			{
				if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
				{
					if (FrontendState == State.Starting || FrontendState == State.Connecting)
					{
						RemoveLoadingScreen();
						AddLoadingScreen("Marquee");
					}
				}
				else
				{
					if (FrontendState == State.Starting || FrontendState == State.ConnectingVideo ||
							FrontendState == State.ConnectingGuest)
					{
						RemoveLoadingScreen();
						AddLoadingScreen("Marquee");
					}
				}
				if (sHideMode)
				{
					Logger.Info("Hiding window");
					if (this.ParentForm != null)
						this.ParentForm.Hide();
				}
				else
				{
					if (this.ParentForm != null && !Oem.Instance.IsFrontendBorderHidden)
						this.ParentForm.FormBorderStyle = FormBorderStyle.FixedSingle;	
				}
			}
		}

		internal int HandleCloseEvent(Object o, CancelEventArgs e)
		{
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
			}
			else
			{
				mStateMachine.Quit();
			}

			Logger.Info("HandleCloseEvent sessionEnding = " + sessionEnding);
			Common.Stats.SendFrontendStatusUpdate("frontend-closed");


			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsSendSysTrayNotificationOnFrontendClose)
			{
				String msg = Common.Strings.NetEaseFrontendExitMessage;
				String title = Common.Strings.NetEaseAndroidSimulator;
				HTTPHandler.SendSysTrayNotification(title, "info", msg);
			}

			Utils.KillProcessByName("HD-RunApp");

			Logger.Info("changing the frontend state to quitting");
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				FrontendState = State.Quitting;

			}
			if (this.sessionEnding == true)
			{
				return 1;
			}

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				Audio.Manager.Mute();
			}
			else
			{
				SendMuteEventToGuest(true);
			}

			if (camManager != null)
				camManager.pauseCamera();
			this.Paint -= HandlePaintEvent;

			if (this.frontendNoClose)
			{
				Interop.Animate.AnimateWindow(Handle, 500,
						Interop.Animate.AW_BLEND | Interop.Animate.AW_HIDE);
				this.Hide();
				e.Cancel = true;
			}
			else if (this.stopZygoteOnClose)
			{
				Logger.Info("Stopping Zygote from interop");
				try
				{
					if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
					{
						Command cmd = new Command();
						cmd.Attach(this.vmName);

						int res = cmd.Run(new String[] {
								"/system/bin/stop",
								});

						if (res != 0)
							Logger.Error("Cannot stop Zygote: " +
									res);
					}
					else
					{
						Interop.Opengl.StopZygote(this.vmName);
					}
				}
				catch (Exception exc)
				{
					Logger.Error("Cannot run command to stop " +
							"Zygote: " + exc.ToString());
				}
			}

#if BUILD_HYBRID
			try
			{
				if (!this.frontendNoClose)
				{
					if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
					{
					}
					else
					{
						StateExitConnected();
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception stateexitconnected: {0}", ex.ToString());
			}
			finally
			{
				if (restartFrontend)
				{
					restartFrontend = false;

					int port = Common.Utils.GetPartnerServerPort();
					Process.Start(sInstallDir + @"\HD-Frontend.exe", sArgs);
					if (Oem.Instance.IsOemWithGameManagerData)
						Common.HTTP.Client.Post(String.Format("http://127.0.0.1:{0}/attachfrontend", port), null, null, false);
				}
			}
#endif
			return 0;
		}

		internal void HandleActivatedEvent(Object o, EventArgs e)
		{
			Logger.Info("HandleActivatedEvent");
			FrontendActivated();
		}

		internal void HandleDeactivateEvent(Object o, EventArgs e)
		{
			Logger.Info("HandleDeactivateEvent");
			FrontendDeactivated();
		}

		public void FrontendDeactivated()
		{
			sFrontendActive = false;
			Common.Stats.SendFrontendStatusUpdate("frontend-deactivated");

			if (mFullScreen)
			{
				mFullScreenToast.Hide();
			}

			foreach (KeyValuePair<Keys, int> key in sKeyStateSet)
			{
				mInputMapper.DispatchKeyboardEvent(
						this.keyboard.NativeToScanCodes(key.Key), false);
			}
			sKeyStateSet.Clear();

			if (mDisableMouseMovement == true)
				DisableShootingMode();

			if (mInputMapper.IsLocationUpdationWithKeyMapEnabled() != 0)
				StopGpsLocationProvider();

			mCursor.RaiseFocusChange();
		}

		public void FrontendActivated()
		{
			if (this.frontendMinimized)
			{
				this.Paint += HandlePaintEvent;
				this.frontendMinimized = false;
			}

			if (Oem.Instance.IsUnmuteOnFrontendActivated)
			{
				if (!sReLayoutInProgress)
				{
					if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
					{
						Audio.Manager.Unmute();
					}
					else
					{
						SendMuteEventToGuest(false);
					}
				}
			}

			if (camManager != null)
				camManager.resumeCamera();

			sFrontendActive = true;
			Common.Stats.SendFrontendStatusUpdate("frontend-activated");
			if (mGamePad == null &&
					IsFrontendReparented() == false)
			{
				mGamePad = new GamePad();
				mGamePad.Setup(mInputMapper, this.Handle);
			}

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				if (!frontendReadyActionDone)
				{
					DoFrontendReadyAction();
					frontendReadyActionDone = true;
				}
			}

			if (this.keyboard != null && mStateMachine.Monitor != null
					&& isGuestReady)
			{
				UnstickKeyboardModifiers();
				Logger.Info("frontendActivated - sendlockedkeys");
			}



			mCursor.RaiseFocusChange();
		}

		private void HandlePaintEvent(Object obj, PaintEventArgs evt)
		{
			Rectangle rect = evt.ClipRectangle;
			Graphics surface = evt.Graphics;
			Interop.Video.Mode mode;
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				if (FrontendState != State.Connected && key.GetValue("EnableConsoleAccess") == null)
					return;

				if (mStateMachine.Video == null)
					return;
			}
			else
			{
				bool ok = false;

				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath))
				{

					if (FrontendState == State.Connected)
						ok = true;
					else if (FrontendState == State.ConnectingGuest && !HideBootProgress())
						ok = true;
					else if (key.GetValue("EnableConsoleAccess") != null && FrontendState == State.ConnectingGuest)
						ok = true;
				}

				if (!ok)
					return;

				//Logger.Info("HandlePaintEvent ({0}, {1}), ({2}, {3})",
				//    rect.X, rect.Y, rect.Width, rect.Height);

				//DumpFrameBufferInfo();

			}

			mode = mStateMachine.Video.GetMode();

			if (mode.Width == 0 || mode.Height == 0)
				return;

			PixelFormat fmt;

			if (mode.Depth == 24)
				fmt = PixelFormat.Format24bppRgb;
			else if (mode.Depth == 16)
				fmt = PixelFormat.Format16bppRgb565;
			else
				fmt = PixelFormat.Undefined;

			//
			// If Opengl DrawFB is successful we don't need to
			// draw the fraembuffer on our own.
			//
			// For GL mode = 1, and if ConsoleAccess is enabled,
			// don't draw FrameBuffer...
			Bitmap bitmap;
			if (Interop.Opengl.DrawFB(mode.Width, mode.Height,
						mStateMachine.Video.GetBufferAddr(), lockdownDisabled))
				return;

			bitmap = new Bitmap(
					mode.Width,
					mode.Height,
					(int)mStateMachine.Video.GetStride(),
					fmt,
					mStateMachine.Video.GetBufferAddr());

			if (!IsPortrait())
			{
				Point[] destinationPoints = {

					// Upper left
					new Point(
							mScaledDisplayArea.X + 0,
							mScaledDisplayArea.Y + 0),

					// Upper right
					new Point(
							mScaledDisplayArea.X + mScaledDisplayArea.Width - 0,
							mScaledDisplayArea.Y + 0),

					// Lower left
					new Point(
							mScaledDisplayArea.X + 0,
							mScaledDisplayArea.Y + mScaledDisplayArea.Height)
				};

				surface.DrawImage(bitmap, destinationPoints);
			}
			else
			{
				Point[] destinationPoints = {

					// Upper left
					new Point(
							mScaledDisplayArea.X + 0,
							mScaledDisplayArea.Y + mScaledDisplayArea.Height),

					// Upper right
					new Point(
							mScaledDisplayArea.X + 0,
							mScaledDisplayArea.Y + 0),

					// Lower left
					new Point(
							mScaledDisplayArea.X + mScaledDisplayArea.Width - 0,
							mScaledDisplayArea.Y + mScaledDisplayArea.Height)
				};

				surface.DrawImage(bitmap, destinationPoints);
			}
		}

		/*
		 * Mouse Handling Routines
		 */
		private void HandleShootingMode(MouseEventArgs evt)
		{
			int MoveMax = mScaledDisplayArea.Width / 2;
			int LengthNotmal = mScaledDisplayArea.Width / 6;
			float percentX = 1;
			float percentY = 1;
			Cursor.Position = mScreenPos;

			if (mShootMouseSwitch && s_touchShootModeType == SHOOTING_MODE_2)
			{
				/* GAME:QMQZ
				 * Prevent the switched process of the touch position
				 * moving speed too fast caused by misoperation.
				 * Wait mouse to move speed of less than 10, activated again.
				 */
				if (Math.Abs(evt.X - mMoveOriginPos.X) > 10 || Math.Abs(evt.Y - mMoveOriginPos.Y) > 10)
					return;
				else
					mShootMouseSwitch = false;
			}

			if (s_touchShootModeType == SHOOTING_MODE_3)
			{
				/* GAME:Tencent CF
				 * cf game MouseMovePos farther from the originPox moving the faster the.
				 * We need to control the moving speed
				 * Does not control the length in the LengthNotmal and mShootMouseDown in press
				 */
				int x1 = mShootMouseDown ? mShootOriginPos.X : mMoveOriginPos.X;
				int y1 = mShootMouseDown ? mShootOriginPos.Y : mMoveOriginPos.Y;

				int x2 = Math.Abs(mMouseMovePos.X - x1);
				int y2 = Math.Abs(mMouseMovePos.Y - y1);

				if (x1 > LengthNotmal)
					percentX = (float)Math.Max((float)(MoveMax - x2) / MoveMax, 0.1);

				if (y1 > LengthNotmal)
					percentY = (float)Math.Max((float)(MoveMax - y2) / MoveMax, 0.1);
			}

			if (evt.X > mMoveOriginPos.X) //Right
			{
				mMouseMovePos.X += Math.Max((int)((float)(evt.X - mMoveOriginPos.X) * percentX), 1);
			}
			else if (evt.X < mMoveOriginPos.X) //Left
			{
				mMouseMovePos.X -= Math.Max((int)((float)(mMoveOriginPos.X - evt.X) * percentX), 1);
			}

			if (evt.Y > mMoveOriginPos.Y) //Down
			{
				mMouseMovePos.Y += Math.Max((int)((float)(evt.Y - mMoveOriginPos.Y) * percentY), 1);
			}
			else if (evt.Y < mMoveOriginPos.Y) //Up
			{
				mMouseMovePos.Y -= Math.Max((int)((float)(mMoveOriginPos.Y - evt.Y) * percentY), 1);
			}

			if (s_touchShootModeType == SHOOTING_MODE_3)
			{

				Point originPos = mShootMouseDown ? mShootOriginPos : mMoveOriginPos;
				if (Math.Abs(mMouseMovePos.X - originPos.X) > MoveMax
						|| Math.Abs(mMouseMovePos.Y - originPos.Y) > MoveMax)
				{
					if (!mShootMouseDown)
						InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, false);
					else
						InputMapper.Instance().UpdateShootCoordinates(sMouseGuestX, sMouseGuestY, true);
					mMouseMovePos = originPos;
				}
			}

			sMouseGuestX = GetLandscapeGuestX(mMouseMovePos.X, mMouseMovePos.Y);
			sMouseGuestY = GetLandscapeGuestY(mMouseMovePos.X, mMouseMovePos.Y);

			lock (mShootLockObject)
			{
				if (mShootMouseDown && s_touchShootModeType == SHOOTING_MODE_3)
				{
					InputMapper.Instance().UpdateShootCoordinates(sMouseGuestX, sMouseGuestY, true);
					return;
				}

				InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, true);
			}
		}

		private void HandleMouseMove(Object obj, MouseEventArgs evt)
		{
			int x = evt.X;
			int y = evt.Y;

			UpdateUserActivityStatus();

			int currentTime = System.Environment.TickCount;
			if (Math.Abs(currentTime - sLastTouchTime) <= 1000)
				return;
			if (Interop.Input.IsEventFromTouch())
				return;

			if (mStateMachine.Monitor == null)
				return;

			//Logger.Info("{0,-12} {1:D4} {2:D4}", "MouseMove", x, y);
			if (mDisableMouseMovement == true)
			{
				if ((evt.X != mMoveOriginPos.X || evt.Y != mMoveOriginPos.Y) && mDisableShootMouse)
					HandleShootingMode(evt);
				return;
			}

			if (sMouseDown == true)
			{
				InputMapper.Instance().UpdateMouseCoordinates(
						GetLandscapeGuestX(x, y),
						GetLandscapeGuestY(x, y),
						sMouseDown);
			}
			else
			{
				this.mouse.UpdateCursor((uint)GetGuestX(x, y),
						(uint)GetGuestY(x, y));
				mStateMachine.Monitor.SendMouseState(this.mouse.X, this.mouse.Y,
						this.mouse.Mask);
			}

			if (s_KeyMapTeachMode)
			{
				double x1 = 100.0 * GetGuestX(x, y) / GUEST_ABS_MAX_X;
				double y1 = 100.0 * GetGuestY(x, y) / GUEST_ABS_MAX_Y;

				if (mEmulatedPortraitMode)
				{
					double origX = x1;
					x1 = (x1 * (1 - mInputMapper.mSoftControlBarHeightPortrait));
					if (!mRotateGuest180)
					{
						x1 = y1;
						y1 = 100 - origX;

					}
					else
					{
						x1 = 100 - y1;
						y1 = origX;
					}
				}

				if (mInputMapper.mSoftControlEnabled == true)
				{
					double yCoord = (y1 * mScaledDisplayArea.Height / 100.0);
					double appScreenHeight = (mScaledDisplayArea.Height - (mInputMapper.mSoftControlBarHeightLandscape * mConfiguredDisplaySize.Height));
					y1 = (yCoord / appScreenHeight) * 100;

					if (y1 > 100 || y1 < 0)
					{
						s_KeyMapToolTip.Hide(this);
						return;
					}
				}

				string status = String.Format("  [ x={0}%, y={1}% - {2}]",
						Math.Round(x1, 2), Math.Round(y1, 2),
						mCurrentAppPackage);

				s_KeyMapToolTip.Show(status, this, x, y, 100000);
			}
			else
			{
				s_KeyMapToolTip.Hide(this);
			}
		}

		private void HandleMouseDown(Object obj, MouseEventArgs evt)
		{
			if (IsMouseDownHandled)
			{
				IsMouseDownHandled = false;
			}
			else
			{ 
				UpdateUserActivityStatus();

				if (evt.Button == MouseButtons.Left)
				{
					Logger.Debug("left button");
					Logger.Debug("{0},{1}", evt.X, evt.Y);
					Logger.Debug("{0},{1}", this.ClientSize.Width, this.ClientSize.Height);
				}

				if ((Control.ModifierKeys & Keys.Control) == Keys.Control &&
						evt.Button == MouseButtons.Left)
				{
					mInputMapper.EmulatePinch((float)evt.X / this.ClientSize.Width,
							(float)evt.Y / this.ClientSize.Height, false);
				}
				else if ((Control.ModifierKeys & Keys.Control) == Keys.Control &&
						evt.Button == MouseButtons.Right)
				{
					mInputMapper.EmulatePinch((float)evt.X / this.ClientSize.Width,
							(float)evt.Y / this.ClientSize.Height, true);
				}
				else
				{
					bool shootingMode = InputMapper.Instance().IsShootingModeEnabled() == 1 ? true : false;
					if (evt.Button == MouseButtons.Right &&
							shootingMode == true)
					{


						if (mDisableMouseMovement == false)
						{
							if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyChangesToParentWindow)
							{
								WindowMessages.NotifyShootingModeStateToParentWindow(true);
							}
							mDisableMouseMovement = !mDisableMouseMovement;

							int mouseX = (int)((float)mScaledDisplayArea.Width * s_ShootOriginXPos);
							int mouseY = (int)((float)mScaledDisplayArea.Height * s_ShootOriginYPos);
							mMouseMovePos = mMoveOriginPos = new Point(mouseX, mouseY);
							Cursor.Position = mScreenPos = this.PointToScreen(new Point(mouseX, mouseY));
							Cursor.Clip = new Rectangle(this.Location, this.Size);
							sOriginGuestX = GetLandscapeGuestX(mMoveOriginPos.X, mMoveOriginPos.Y);
							sOriginGuestY = GetLandscapeGuestY(mMoveOriginPos.X, mMoveOriginPos.Y);
							InputMapper.Instance().UpdateMouseCoordinates(sOriginGuestX, sOriginGuestY, true);
							Cursor.Hide();
							InputMapper.Instance().SetShootingModeControls(true);

							bool retVal = SystemParametersInfo(SPI_GETMOUSESPEED, 0, ref sOriginalMouseSpeed, 0);
							if (retVal == false)
							{
								Logger.Error("Unable to get current mouse speed");
							}
							else
							{
								Logger.Info("Original Mouse sensitivity = {0}", sOriginalMouseSpeed);
							}
							if (s_ShootSensitivity <= 20 && s_ShootSensitivity >= 0)
							{
								retVal = SystemParametersInfo(SPI_SETMOUSESPEED, 0, (uint)s_ShootSensitivity, 0x1);

								if (retVal == false)
								{
									Logger.Error("Unable to set mouse speed to {0}", s_ShootSensitivity);
								}
								else
								{
									Logger.Info("Set Mouse sensitivity to {0}", s_ShootSensitivity);
								}
							}
						}
						else
						{
							DisableShootingMode();
						}
					}
					else
					{
						HandleMouseButton(evt.X, evt.Y, evt.Button, true, false);
						if (mDisableMouseMovement)
						{
							if (s_touchShootModeType == SHOOTING_MODE_2)
								mShootAttackClick = true;
							mBurstModeOn = true;
							SetShootEvent(true);
						}
					}
				}
			}
		}
		private void SetShootEvent(bool down)
		{
			mDisableShootMouse = false; /*Clicking Not Move*/

			int mouseX = (int)(((float)s_ShootTriggerXPos / GUEST_ABS_MAX_X) * mScaledDisplayArea.Width);
			int mouseY = (int)(((float)s_ShootTriggerYPos / GUEST_ABS_MAX_Y) * mScaledDisplayArea.Height);

			if (s_touchShootModeType == SHOOTING_MODE_3)
			{
				if (down)
				{
					mShootMouseDown = true;
					//reset the mouse move touch position
					InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, false);
					InputMapper.Instance().UpdateMouseCoordinates(sOriginGuestX, sOriginGuestY, true);
					//set shoot touch position
					InputMapper.Instance().UpdateShootCoordinates(s_ShootTriggerXPos, s_ShootTriggerYPos, true);
					mShootOriginPos = mMouseMovePos = new Point(mouseX, mouseY);
				}
				else
				{
					mShootMouseDown = false;
					mMouseMovePos = mMoveOriginPos;
					InputMapper.Instance().UpdateShootCoordinates(sMouseGuestX, sMouseGuestY, false);
				}
			}
			else
			{
				if (s_touchShootModeType == SHOOTING_MODE_2)
				{
					if (down)
					{
						mShootMouseDown = true;
						InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, false, 80);
						InputMapper.Instance().UpdateMouseCoordinates(s_ShootTriggerXPos, s_ShootTriggerYPos, true);
						mMouseMovePos = new Point(mouseX, mouseY);

					}
					else
					{

						mShootMouseDown = false;
						InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, false, 80);
						InputMapper.Instance().UpdateMouseCoordinates(sOriginGuestX, sOriginGuestY, true);
						mMouseMovePos = mMoveOriginPos;
						mShootMouseSwitch = true;
					}
				}
				else
				{
					/*
					 * Normally, Single touch shoot mode is disabled.
					 * It is only enabled for games which do not support
					 * simultaneous rotational movement and gun fire.
					 */
					if (s_IsSingleTouchShootModeEnabled == false)
					{
						mInputMapper.DispatchKeyboardEvent(
								0x101, down);

					}
					else
					{

						if (down == true)
						{
							InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, false);
						}

						InputMapper.Instance().UpdateMouseCoordinates(s_ShootTriggerXPos, s_ShootTriggerYPos, down);

						if (down == false)
						{
							InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, true);
						}
					}
				}
			}
			mDisableShootMouse = true; /*restore move*/
		}

		public void DisableShootingModeIfEnabled()
		{
			Logger.Info("In DisableShootingModeIfEnabled");

			if (mDisableMouseMovement == true)
				DisableShootingMode();

			Logger.Info("Done DisableShootingModeIfEnabled");
		}

		private void DisableShootingMode()
		{
			mDisableMouseMovement = !mDisableMouseMovement;
			Cursor.Show();
			Cursor.Clip = new Rectangle();
			InputMapper.Instance().UpdateMouseCoordinates(sMouseGuestX, sMouseGuestY, false);
			InputMapper.Instance().SetShootingModeControls(false);
			if (sOriginalMouseSpeed != 10 || s_ShootSensitivity != 10)
			{
				bool retVal = SystemParametersInfo(SPI_SETMOUSESPEED, 0, sOriginalMouseSpeed, 0x1);
				if (retVal == false)
				{
					Logger.Error("Unable to set Mouse sensitivity to {0}", sOriginalMouseSpeed);
				}
				else
				{
					Logger.Info("Set Mouse sensitivity back to {0}", sOriginalMouseSpeed);
				}
			}
			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyChangesToParentWindow)
			{
				WindowMessages.NotifyShootingModeStateToParentWindow(false);
			}
		}

		private void HandleMouseUp(Object obj, MouseEventArgs evt)
		{
			UpdateUserActivityStatus();

			if (mDisableMouseMovement == true && mBurstModeOn == true)
			{
				if (s_touchShootModeType == SHOOTING_MODE_2)
					mShootAttackClick = true;
				mBurstModeOn = false;
				SetShootEvent(false);
			}
			HandleMouseButton(evt.X, evt.Y, evt.Button, false, false);
		}

		private void HandleMouseButton(int x, int y, MouseButtons button,
			bool pressed, bool force)
		{
			if (Interop.Input.IsEventFromTouch())
				return;

			if (mStateMachine.Monitor == null)
				return;

			if (mDisableMouseMovement == true && force == false) //for shooting apps
				return;

			Logger.Debug("{3} Mouse {2} at {0}, {1}",
					(int)((float)this.ClientSize.Width),
					(int)((float)this.ClientSize.Height),
					(pressed ? "down" : "up"),
					(button == MouseButtons.Left ? "left" : "right")
					);
			//Logger.Info("{0,-12} {1:D4} {2:D4} {3:s} {4}",
			//    "MouseButton", x, y, button.ToString(), pressed);

			if (button == MouseButtons.Left)
			{
				InputMapper.Instance().UpdateMouseCoordinates(
						GetLandscapeGuestX(x, y),
						GetLandscapeGuestY(x, y),
						pressed);
				sMouseDown = pressed;
			}
			else
			{
				this.mouse.UpdateButton((uint)GetGuestX(x, y),
						(uint)GetGuestY(x, y), button, pressed);
				mStateMachine.Monitor.SendMouseState(this.mouse.X, this.mouse.Y,
						this.mouse.Mask);
			}
		}

		private void HandleMouseWheel(Object obj, MouseEventArgs evt)
		{
			UpdateUserActivityStatus();

			if (Interop.Input.IsEventFromTouch())
				return;

			float x = (float)GetGuestX(evt.X, evt.Y) /
				(float)GUEST_ABS_MAX_X;
			float y = (float)GetGuestY(evt.X, evt.Y) /
				(float)GUEST_ABS_MAX_Y;

			if ((Control.ModifierKeys & Keys.Control) == Keys.Control)
			{

				mInputMapper.EmulatePinch(x, y,
					evt.Delta > 0);

			}
			else
			{

				InputMapper.Direction direction;

				if (evt.Delta < 0)
					direction = InputMapper.Direction.Up;
				else
					direction = InputMapper.Direction.Down;

				mInputMapper.EmulateSwipe(x, y, direction);
			}
		}

		protected override void WndProc(ref Message m)
		{
			if (m.Msg == Common.Interop.Window.WM_IME_SETCONTEXT && m.WParam.ToInt32() == 1)
			{
				Composition.dwStyle = 0x0002;
				Composition.ptCurrentPos.X = 0;
				Composition.ptCurrentPos.Y = base.Height - 15;
				Common.Interop.Window.ImmSetCompositionWindow(m_hImc, out Composition);
			}
			bool handled = false;
			switch (m.Msg)
			{
				case 0x0082:
					Logger.Error("----------------------------> Got WM_NCDESTROY");
					break;

				

				case Common.Interop.Window.WM_USER_SWITCH_TO_LAUNCHER:
					Logger.Info("Received message WM_USER_SWITCH_TO_LAUNCHER");
					break;

				case WM_QUERYENDSESSION:
					Logger.Info("Received message WM_QUERYENDSESSION");
					sessionEnding = true;
					break;

				case WM_SYSCOMMAND:
					Logger.Info("Received message WM_SYSCOMMAND");
					int command = m.WParam.ToInt32();
					if (!HandleWMSysCommand(command))
						return;
					break;

				case Common.Interop.Window.WM_COPYDATA:

					Common.Interop.Window.COPYGAMEPADDATASTRUCT gdStruct =
						(Common.Interop.Window.COPYGAMEPADDATASTRUCT)Marshal.PtrToStructure(m.LParam,
								typeof(Common.Interop.Window.COPYGAMEPADDATASTRUCT));

					byte[] buff = new byte[gdStruct.size];
					Marshal.Copy(gdStruct.lpData, buff, 0, buff.Length);

					int[] data = new int[buff.Length / sizeof(int)];
					for (int i = 0; i < buff.Length; i += sizeof(int))
					{
						data[i / (sizeof(int))] = BitConverter.ToInt32(buff, i);
					}

					switch (m.WParam.ToInt32())
					{
						case (int)Utils.GamePadEventType.TYPE_GAMEPAD_ATTACH:
							mInputMapper.DispatchGamePadAttach(data[0], data[1], data[2]);
							break;

						case (int)Utils.GamePadEventType.TYPE_GAMEPAD_DETACH:
							mInputMapper.DispatchGamePadDetach(data[0]);
							break;

						case (int)Utils.GamePadEventType.TYPE_GAMEPAD_UPDATE:

							Common.GamePad gamepad = new Common.GamePad();
							gamepad.X = data[1];
							gamepad.Y = data[2];
							gamepad.Z = data[3];
							gamepad.Rx = data[4];
							gamepad.Ry = data[5];
							gamepad.Rz = data[6];
							gamepad.Hat = data[7];
							gamepad.Mask = (uint)data[8];

							mInputMapper.DispatchGamePadUpdate(data[0], gamepad);
							break;

						default:
							Logger.Info("Recieved CopyData wParam: {0}", m.WParam);
							break;
					}
					break;

				case Common.Interop.Window.WM_INPUTLANGCHANGEREQUEST:
					Logger.Info("Received message WM_INPUTLANGCHANGEREQUEST");
					break;

				case Common.Interop.Window.WM_IME_CHAR:
					GetImeString(m);
					break;

				case Common.Interop.Window.WM_IME_ENDCOMPOSITION:
					PostCompostionStringToAndroid();
					break;

				case Common.Interop.Window.WM_CHAR:
					if (mIsTextInputBoxInFocus && isUsePcImeWorkflow)
						PassCharToAndroid(m);
					break;
				
				default:
					//Logger.Info("Received unknown message: " + m.Msg);
					handled = false;
					break;
			}

			base.WndProc(ref m);

			if (handled)
			{
				try
				{
					m.Result = new System.IntPtr(1);
				}
				catch (Exception exception)
				{
					Debug.Print("ERROR: Could not allocate result ptr");
					Debug.Print(exception.ToString());
				}
			}
		}

		internal void UserGoBack()
		{
			try
			{
				Logger.Info("Back Button Clicked");
				Thread thread = new Thread(delegate ()
						{
							if (Utils.IsGuestBooted())
								VmCmdHandler.RunCommand("back");
						});
				thread.IsBackground = true;
				thread.Start();
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in HandleWMUserGoBack: " + exc.ToString());
			}
		}

		private void PassCharToAndroid(Message m)
		{
			char c = (char)m.WParam;
			if (char.IsControl(c))
			{
				return;
			}
			Logger.Debug("the ascii value is {0}", m.WParam.ToString());
			string inputCharString = "";
			inputCharString = "start_" + c + "_end";
			Logger.Debug("the inputcharstring  is {0}", inputCharString);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("string", inputCharString);

			string url = String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, "imestring");
			Thread postCharToAndroid = new Thread(delegate ()
					{
						try
						{
							lock (imeLockObject)
							{
								string r = Common.HTTP.Client.Post(url, data, null, false, 5 * 1000);
								Logger.Debug("the response we get is {0}", r);
							}
						}
						catch (Exception ex)
						{
							Logger.Error("got exception when sending char to android ex:{0}", ex.ToString());
						}
					});
			postCharToAndroid.IsBackground = true;
			postCharToAndroid.Start();
		}

		private void GetImeString(Message m)
		{
			char c = (char)m.WParam;
			Logger.Debug("the ascii value is {0}", m.WParam.ToString());
			ImeCharsQueue.Enqueue(c);
		}

		private void PostCompostionStringToAndroid()
		{
			if (ImeCharsQueue.Count == 0)
			{
				//WM_IME_NOTIFY message is also received whenever the form loses/gets back focus 
				return;
			}
			string resultString = "";
			string imeCompositionString = "";

			while (ImeCharsQueue.Count != 0)
			{
				imeCompositionString = imeCompositionString + ImeCharsQueue.Dequeue();
			}
			Logger.Debug("the ime compostion string is:" + imeCompositionString);

			imeCompositionString = "start_" + imeCompositionString + "_end";
			resultString = imeCompositionString.Normalize(NormalizationForm.FormKC);

			Logger.Debug("the ime result string is:" + resultString);
			string imeStringPath = "imestring";
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("string", resultString);
			string url = String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, imeStringPath);
			Logger.Debug("The url is {0}", url);

			Thread postImeString = new Thread(delegate ()
				{
					try
					{
						lock (imeLockObject)
						{
							string r = Common.HTTP.Client.Post(url, data, null, false, 5 * 1000);
							Logger.Debug("the response we get is {0}", r);
						}
					}
					catch (Exception ex)
					{
						Logger.Error("we got an exception when posting IME String {0}", ex.ToString());
					}
				});
			postImeString.IsBackground = true;
			postImeString.Start();

		}

		internal void UserShowWindow()
		{
			if (!this.userInteracted)
			{
				Logger.Info("attaching paint event handler");
				this.Paint += HandlePaintEvent;
				this.userInteracted = true;
			}

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				if (FrontendState == State.Stopped)
				{
					Logger.Info("Starting Service");
					StartVmServiceAsync();
					StateExitStopped();
					StateEnterStarting();
				}
			}
			if (FrontendState == State.Connected)
			{
				Logger.Info("at connected state");
				this.userInteracted = false;
			}
			Interop.Animate.AnimateWindow(Handle, 500,
				Interop.Animate.AW_BLEND);
			Logger.Info("sHideMode = " + sHideMode);
			
			/*
             * In case frontend was earlier running in hidden mode,
             * we will switch to fullscreen mode if required after frontend becomes visible
             */
			if (originalFullScreenState && !mFullScreen)
			{
				ToggleFullScreen();
				originalFullScreenState = false;
			}
		}

		/*
         * returns true in case the command will go to base.WndProc() as well
         * returns false in case we don't want to send the command to base.WndProc()
         */
		private bool HandleWMSysCommand(int command)
		{
			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
					Common.Strings.GuestRegKeyPath, this.vmName);

			if (command == SC_MAXIMIZE || command == SC_MAXIMIZE2 || command == SC_RESTORE || command == SC_RESTORE2)
			{
				Logger.Info("Received MAXIMIZE/RESTORE command");

				if (isMinimized)
					return true;
			}
			return true;
		}

		public void ResizeClientWindow(int width, int height)
		{
			Logger.Info("In ResizeClientWindow");
			mConfiguredDisplaySize.Width = width;
			mConfiguredDisplaySize.Height = height;
			ResizeFrontendWindow(true);
		}

		private void HandleWMCopyData(string messageData)
		{
			String[] args = messageData.Split(' ');
			string package = args[0];
			string activity = ".Main";
			string apkUrl = args[1];
			Logger.Info("package = " + package);
			Logger.Info("apkUrl = " + apkUrl);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("package", package);
			data.Add("activity", activity);
			data.Add("apkUrl", apkUrl);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			int agentPort = (int)key.GetValue("AgentServerPort", 2861);
			string url = String.Format("http://127.0.0.1:{0}/{1}", agentPort, "runapp");
			Logger.Info("Console: Sending post request to {0}", url);

			Common.HTTP.Client.PostWithRetries(url, data, null, false, 10, 500, Common.Strings.VMName);
		}

		/*
         * Get width of the border of the frontend window with
         * style set to WS_OVERLAPPEDWINDOW
         * This is used to set the client size of the window
         */
		private int GetBorderWidth(int width, int height)
		{
			Common.Interop.Window.RECT clientWindow;
			clientWindow.left = 0;
			clientWindow.top = 0;
			clientWindow.right = width;
			clientWindow.bottom = height;

			int windowStyle = Common.Interop.Window.WS_OVERLAPPEDWINDOW;

			if (!Common.Interop.Window.AdjustWindowRect(out clientWindow, windowStyle, false))
				return 18;
			else
			{
				int border = clientWindow.right - clientWindow.left - width;
				Logger.Info("Got border = " + border);
				return border;
			}
		}

		internal Size GetConfiguredDisplaySize()
		{
			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
				Common.Strings.GuestRegKeyPath, this.vmName);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
			int width = (int)key.GetValue("WindowWidth");
			int height = (int)key.GetValue("WindowHeight");
			key.Close();

			return new Size(width, height);
		}

		private Size GetConfiguredGuestSize()
		{
			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
				Common.Strings.GuestRegKeyPath, this.vmName);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
			int width = (int)key.GetValue("GuestWidth");
			int height = (int)key.GetValue("GuestHeight");
			key.Close();

			return new Size(width, height);
		}

		public Rectangle GetScaledGuestDisplayArea()
		{
			return mScaledDisplayArea;
		}


		void SystemEvents_PowerModeChanged(object sender, PowerModeChangedEventArgs e)
		{
			if (e.Mode == PowerModes.Resume)
			{
				this.afterSleepTimer = new System.Windows.Forms.Timer();
				this.afterSleepTimer.Interval = 3000;
				this.afterSleepTimer.Tick += delegate (Object obj, EventArgs evt)
				{
					this.afterSleepTimer.Stop();
				};
				this.afterSleepTimer.Start();
			}

		}

		/*
         * Touch Event Handling
         */

		private void HandleTouchEvent(Object obj, WMTouchEventArgs evt)
		{
			UpdateUserActivityStatus();
			sLastTouchTime = System.Environment.TickCount;

			for (int ndx = 0; ndx < this.touchPoints.Length; ndx++)
			{

				WMTouchUserControl.TouchPoint point = evt.GetPoint(ndx);
				if (point.Id != -1)
				{
					this.touchPoints[ndx].PosX =
						(ushort)GetGuestX(point.X, point.Y);
					this.touchPoints[ndx].PosY =
						(ushort)GetGuestY(point.X, point.Y);
				}
				else
				{
					this.touchPoints[ndx].PosX = -1;
					this.touchPoints[ndx].PosY = -1;
				}
			}

			mStateMachine.Monitor.SendTouchState(this.touchPoints);
		}

		/*
         * Keyboard Event Handling
         */

		private bool HandleKeyboardHook(bool pressed, uint key)
		{
			UpdateUserActivityStatus();
			/*
             * Don't do anything unless we're in the connected
             * state and we are focused.
             */
			if (!this.Focused)
			{
				if (key == (uint)Keys.CapsLock)
				{
					HandleKeyEvent(Keys.CapsLock, pressed);
					return true;
				}
				else if (key == (uint)Keys.NumLock)
				{
					HandleKeyEvent(Keys.NumLock, pressed);
					return true;
				}
				else if (key == (uint)Keys.LShiftKey)
				{
					HandleKeyEvent(Keys.LShiftKey, pressed);
					return true;
				}
			}
			if (FrontendState != State.Connected || !this.Focused)
				return true;
			/*
             * Process any key strokes that we don't want seen by
             * Windows here.  Everything else can be passed on to
             * Windows and handled normally.
             */

			if (this.grabKeyboard && (key == (uint)Keys.LWin || key == (uint)Keys.RWin))
			{

				this.lastLWinTimestamp = DateTime.Now.Ticks;
				HandleKeyEvent(Keys.LWin, pressed);
				return false;

			}
			else if (key == (uint)Keys.D)
			{

				/*
                 * XXXDPR:  The home button on the ViewPad
                 *          should generate an LWIN+D key
                 *          stroke, which Android handles
                 *          gracefully. Sometimes, we see
                 *          LWIN followed by D, which can
                 *          cause undesired behavior.  Hack
                 *          around this case by simply
                 *          ignoring the D key if it occurs
                 *          quickly after a press or release
                 *          event for LWIN.
                 */

				long now = DateTime.Now.Ticks;

				if (now - this.lastLWinTimestamp < LWIN_TIMEOUT_TICKS)
					return false;
				else
					return true;

			}
			else if (key == (uint)Keys.BrowserBack)
			{

				HandleKeyEvent(Keys.Escape, pressed);
				return false;

			}
			else if (key == (uint)Keys.BrowserHome)
			{

				HandleKeyEvent(Keys.F8, pressed);
				return false;

			}
			else if (key == 255)
			{

				/*
                 * XXXDPR:  The menu button on the ViewPad
                 *          sends a virtual key code of 255.
                 *          This is good enough for shipping
                 *          to ViewSonic, but we'll need
                 *          something a little more generic
                 *          to handle other devices.
                 */

				HandleKeyEvent(Keys.Apps, pressed);
				return false;

			}
			else
			{
				return true;
			}
		}

		protected override bool ProcessDialogKey(Keys keyData)
		{
			return false;
		}

		private bool IgnoreKey(KeyEventArgs evt)
		{
			bool ret = false;
			/*
             * A few keys do not make sense to guest just ignore them
             */
			if (evt.KeyCode == Keys.VolumeDown || evt.KeyCode == Keys.VolumeUp || evt.KeyCode == Keys.VolumeMute)
			{
				ret = true; ;
			}
			return ret;
		}

		private void SendMuteEventToGuest(bool mute)
		{
			int guestPort = Utils.GetBstCommandProcessorPort(Common.Strings.VMName);
			string url;

			try
			{
				Thread thread = new Thread(delegate ()
					{
						if (mute)
						{
							url = string.Format("http://127.0.0.1:{0}/muteappplayer", guestPort);
							mMute = true;
						}
						else
						{
							url = string.Format("http://127.0.0.1:{0}/unmuteappplayer", guestPort);
							mMute = false;
						}

						Logger.Info("Sending request to: " + url);

						Common.HTTP.Client.Get(url, null, false);
					});
				thread.IsBackground = true;
				thread.Start();
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in SendMuteEventToGuest: " + exc.ToString());
			}
		}

		public void HandleKeyDown(Object obj, KeyEventArgs evt)
		{
			UpdateUserActivityStatus();

			/*
             * Route various CTRL+ALT key combinations to the
             * appropriate handler.
             */

			if (evt.Alt && evt.Control)
			{
				if (evt.KeyCode == Keys.V ||
					evt.KeyCode == Keys.O ||
					evt.KeyCode == Keys.G ||
					evt.KeyCode == Keys.T ||
					evt.KeyCode == Keys.F)
				{
					uint code = keyboard.NativeToScanCodes(
						evt.KeyCode);
					Interop.Opengl.HandleCommand((int)code);
				}
				else if (evt.KeyCode == Keys.K)
				{
					ToggleKeyMapTeachMode();
				}
				else if (evt.KeyCode == Keys.I &&
						Features.IsFeatureEnabled(Features.ENABLE_ALT_CTRL_I_SHORTCUTS))
				{
					mInputMapper.ShowConfigDialog();
					return;
				}
				else if (evt.KeyCode == Keys.M &&
						Features.IsFeatureEnabled(Features.ENABLE_ALT_CTRL_M_SHORTCUTS))
				{
					LaunchBlueStacksKeyMapper();
					return;
				}
				/* TODO: Remove the following two keycodes post integration
                 * of game-manager in gm-vbox as the trigger for muting and unmuting
                 * would be done by the game manager button on the UI
                 */
				else if (evt.KeyCode == Keys.N)
				{
					if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
					{
					}
					else
					{
						/* Simulate Mute */
						if (mMute == false)
							SendMuteEventToGuest(true);
						else
							SendMuteEventToGuest(false);
					}

					return;
				}
			}

			if (IgnoreKey(evt))
				return;

			/*
             * Swallow this keystroke if it is not allowed by our
             * console lockdown mechanism.
             */

			if (!LockdownIsKeyAllowed(evt.KeyCode))
				return;

			if (lockdownDisabled)
			{
				if (evt.KeyCode == Keys.F1 && evt.Alt)
				{
					Interop.Opengl.HideSubWindow();
				}

				if (evt.KeyCode == Keys.F7 && evt.Alt)
				{
					Interop.Opengl.ShowSubWindow();
				}
			}

			/*
             * Translate keystrokes to emulated gestures:
             *     1.  CTRL+ArrowKey -> swipe gestures
             *     2.  CTRL+PLUS and CTRL+MINUS -> pinch zoom
             */

			if (evt.Control)
			{
				if (evt.KeyCode == Keys.Up)
				{
					mInputMapper.EmulateSwipe(0.5f, 0.5f,
						InputMapper.Direction.Up);
					return;
				}
				else if (evt.KeyCode == Keys.Down)
				{
					mInputMapper.EmulateSwipe(0.5f, 0.5f,
						InputMapper.Direction.Down);
					return;
				}
				else if (evt.KeyCode == Keys.Left)
				{
					mInputMapper.EmulateSwipe(0.5f, 0.5f,
						InputMapper.Direction.Left);
					return;
				}
				else if (evt.KeyCode == Keys.Right)
				{
					mInputMapper.EmulateSwipe(0.5f, 0.5f,
						InputMapper.Direction.Right);
					return;
				}
				else if (evt.KeyCode == Keys.Oemplus)
				{
					mInputMapper.EmulatePinch(0.5f, 0.5f, true);
					return;
				}
				else if (evt.KeyCode == Keys.OemMinus)
				{
					mInputMapper.EmulatePinch(0.5f, 0.5f, false);
					return;
				}
			}

			if (mInputMapper.IsLocationUpdationWithKeyMapEnabled() != 0 && !mIsTextInputBoxInFocus)
			{
				if ((mPokemonDirectionValue & SPEED_UP_MODIFIER_BIT) == 0 && evt.KeyCode == Keys.ShiftKey)
				{
					Logger.Info("pokenmongo got Shift key down");

					if (mPokemonDirectionValue == NONE_DIRECTION_BIT)
					{
						mPokemonDirectionValue = mPokemonDirectionValue | SPEED_UP_MODIFIER_BIT;
					}
					else
					{
						mPokemonDirectionValue = mPokemonDirectionValue | SPEED_UP_MODIFIER_BIT;
						UpdateGpsLocation();
						return;
					}

				}
				if ((mPokemonDirectionValue & UP_DIRECTION_BIT) == 0 && evt.KeyCode == Keys.W)
				{
					Logger.Debug("pokenmongo got UP key down");
					mPokemonDirectionValue = mPokemonDirectionValue | UP_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
				if ((mPokemonDirectionValue & DOWN_DIRECTION_BIT) == 0 && evt.KeyCode == Keys.S)
				{
					Logger.Debug("pokenmongo got DOWN key down");
					mPokemonDirectionValue = mPokemonDirectionValue | DOWN_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
				if ((mPokemonDirectionValue & LEFT_DIRECTION_BIT) == 0 && evt.KeyCode == Keys.A)
				{
					Logger.Debug("pokenmongo got LEFT key down");
					mPokemonDirectionValue = mPokemonDirectionValue | LEFT_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
				if ((mPokemonDirectionValue & RIGHT_DIRECTION_BIT) == 0 && evt.KeyCode == Keys.D)
				{
					Logger.Debug("pokenmongo got RIGHT key down");
					mPokemonDirectionValue = mPokemonDirectionValue | RIGHT_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
			}
			if (mCurrentAppPackage == null)
			{
				Logger.Info("current app package name is null");
			}

			/*
             * Handle full screen toggle via F11.
             */

			//if (BlueStacks.hyperDroid.Common.Oem.Instance.IsFullScreenToggleEnabled)
			//{
			if (evt.KeyCode == Keys.F11 &&
					Features.IsFullScreenToggleEnabled())
				ToggleFullScreen();
			//}

			if (mIsTextInputBoxInFocus == true && isUsePcImeWorkflow == true && IsPrintingKey(evt.KeyCode) && !(evt.Control || evt.Alt))
				return;

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMinimizeOnEscapeIfFullscreen == true &&
					evt.KeyCode == Keys.Escape &&
					mFullScreen == true)
				return;

			HandleKeyEvent(evt.KeyCode, true);

			if (evt.KeyCode == Keys.CapsLock)
			{
				Logger.Info("caps lock pressed while in frontend");
			}

			if (evt.KeyCode == Keys.NumLock)
			{
				Logger.Info("numlock pressed while in frontend");
			}
		}

		public void StopGpsLocationProvider()
		{
			Thread startLocThread = new Thread(delegate ()
				{
					string url = string.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, "stopLocProvider");
					Logger.Info("the data being sent to url:{0}  for stopLocProvider", url);

					int retries = 5;
					while (retries > 0)
					{
						try
						{
							string res = Common.HTTP.Client.Get(url, null, false, 5 * 1000);
							Logger.Info("the response we get is {0}", res);
							JSonReader readjson = new JSonReader();
							IJSonObject fullJson = readjson.ReadAsJSonObject(res);
							if (fullJson["result"].StringValue.Equals("ok", StringComparison.OrdinalIgnoreCase))
							{
								break;
							}
							else
							{
								retries--;
							}
						}
						catch (Exception ex)
						{
							Logger.Info("the exception we get is {0}", ex.ToString());
							retries--;
						}
					}
				});
			startLocThread.IsBackground = true;
			startLocThread.Start();

		}

		public void UpdateGpsLocation()
		{
			UpdateGpsLocation(false);
		}
		public void UpdateGpsLocation(bool doRetries)
		{
			int angle = 0;

			if ((mPokemonDirectionValue & UP_DIRECTION_BIT) != 0 && (mPokemonDirectionValue & RIGHT_DIRECTION_BIT) != 0)
			{
				angle = 45;
			}
			else if ((mPokemonDirectionValue & UP_DIRECTION_BIT) != 0 && (mPokemonDirectionValue & LEFT_DIRECTION_BIT) != 0)
			{
				angle = 315;
			}
			else if ((mPokemonDirectionValue & UP_DIRECTION_BIT) != 0)
			{
				angle = 0;
			}
			else if ((mPokemonDirectionValue & DOWN_DIRECTION_BIT) != 0 && (mPokemonDirectionValue & RIGHT_DIRECTION_BIT) != 0)
			{
				angle = 135;
			}
			else if ((mPokemonDirectionValue & DOWN_DIRECTION_BIT) != 0 && (mPokemonDirectionValue & LEFT_DIRECTION_BIT) != 0)
			{
				angle = 225;
			}
			else if ((mPokemonDirectionValue & DOWN_DIRECTION_BIT) != 0)
			{
				angle = 180;
			}
			else if ((mPokemonDirectionValue & RIGHT_DIRECTION_BIT) != 0)
			{
				angle = 90;
			}
			else if ((mPokemonDirectionValue & LEFT_DIRECTION_BIT) != 0)
			{
				angle = 270;
			}

			Logger.Debug("the value of angle is {0}", angle);

			bool isDown = false;
			float speed = 5;
			if ((mPokemonDirectionValue & SPEED_UP_MODIFIER_BIT) != 0)
			{
				speed = speed * (float)2.5;
			}
			if (mPokemonDirectionValue == NONE_DIRECTION_BIT || mPokemonDirectionValue == SPEED_UP_MODIFIER_BIT)
			{
				isDown = false;
			}
			else
			{
				isDown = true;
			}

			Thread startLocThread = new Thread(delegate ()
				{
					string url = string.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, "startLocProvider");

					Dictionary<String, String> data = new Dictionary<string, string>();

					JSonWriter json = new JSonWriter();
					json.WriteObjectBegin();
					json.WriteMember("enableNetworkProvider", true);
					json.WriteMember("enableGpsProvider", true);
					json.WriteMember("frequency", 1000);
					if (isDown)
					{
						json.WriteMember("speed", speed);
					}
					else
					{
						json.WriteMember("speed", 0);
					}
					json.WriteMember("angle", angle);
					json.WriteObjectEnd();

					data.Add("updatelocation", json.ToString());
					Logger.Debug("the data being sent to url:{0} is {1}", url, json.ToString());

					int retryCount = 1;
					if (doRetries)
					{
						retryCount = 5;
					}
					while (retryCount > 0)
					{
						try
						{
							string res = Common.HTTP.Client.Post(url, data, null, false, 5 * 1000);

							JSonReader readjson = new JSonReader();
							IJSonObject fullJson = readjson.ReadAsJSonObject(res);
							if (fullJson["result"].StringValue.Equals("ok", StringComparison.OrdinalIgnoreCase))
							{
								break;
							}
							else
							{
								Logger.Info("the response for startloc is false, {0}", res);
								retryCount--;
							}
						}
						catch (Exception ex)
						{
							Logger.Info("the exception we get is {0} for sending to url{1}", ex.ToString(), url);
							retryCount--;
						}
					}
				});
			startLocThread.IsBackground = true;
			startLocThread.Start();
		}

		private void HandleKeyUp(Object obj, KeyEventArgs evt)
		{
			UpdateUserActivityStatus();

			if (IgnoreKey(evt))
				return;
			if (mIsTextInputBoxInFocus == true && isUsePcImeWorkflow && IsPrintingKey(evt.KeyCode) && !(evt.Control || evt.Alt))
				return;

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMinimizeOnEscapeIfFullscreen == true &&
					evt.KeyCode == Keys.Escape &&
					mFullScreen == true)
			{
				ToggleFullScreen();
				return;
			}

			if (mInputMapper.IsLocationUpdationWithKeyMapEnabled() != 0 && !mIsTextInputBoxInFocus)
			{
				if (evt.KeyCode == Keys.W)
				{
					Logger.Debug("pokenmongo got key up up");
					mPokemonDirectionValue = mPokemonDirectionValue & ~UP_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
				if (evt.KeyCode == Keys.S)
				{
					Logger.Debug("pokenmongo got key down up");
					mPokemonDirectionValue = mPokemonDirectionValue & ~DOWN_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
				if (evt.KeyCode == Keys.A)
				{
					Logger.Debug("pokenmongo got key left up");
					mPokemonDirectionValue = mPokemonDirectionValue & ~LEFT_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
				if (evt.KeyCode == Keys.D)
				{
					Logger.Debug("pokenmongo got key right up");
					mPokemonDirectionValue = mPokemonDirectionValue & ~RIGHT_DIRECTION_BIT;
					UpdateGpsLocation();
					return;
				}
				if (evt.KeyCode == Keys.ShiftKey)
				{
					Logger.Debug("pokenmongo speed up modifier key up");
					mPokemonDirectionValue = mPokemonDirectionValue & ~SPEED_UP_MODIFIER_BIT;
					if (mPokemonDirectionValue != NONE_DIRECTION_BIT)
					{
						UpdateGpsLocation();
					}
					return;
				}
			}

			HandleKeyEvent(evt.KeyCode, false);
		}

		public void SendGameManagerRequest(Dictionary<string, string> data, string path)
		{
			Logger.Info("Will send {0} request", path);
			try
			{
				int port = Common.Utils.GetPartnerServerPort();
				string url = string.Format("http://127.0.0.1:{0}/{1}", port, path);
				Logger.Info("Sending request to: " + url);

				if (data == null)
					Common.HTTP.Client.Get(url, null, false);
				else
					Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in SendGameManagerRequest: " + exc.ToString());
			}
		}

		private bool IsPrintingKey(Keys key)
		{
			if (key >= Keys.A && key <= Keys.Z)
				return true;

			if (key >= Keys.D0 && key <= Keys.D9)
				return true;

			if (key >= Keys.NumPad0 && key <= Keys.NumPad9)
				return true;

			//checking keycode for ; = , . _  / \ `
			if (key >= (Keys)186 && key <= (Keys)192)
				return true;

			//checking keycode for [ ] ' | 
			if (key >= (Keys)219 && key <= (Keys)222)
				return true;

			//checking keycode for * +
			if (key >= (Keys)106 && key <= (Keys)107)
				return true;

			//checking keycode for - .  /
			if (key >= (Keys)109 && key <= (Keys)111)
				return true;

			//for space
			if (key == (Keys)32)
				return true;

			return false;
		}

		private bool UpdateKeyState(Keys key, bool pressed)
		{
			/*
             * It was observed that when frontend is reparented
             * by some other window then due to worflow differences
             * in some cases, two key up events might be received for
             * a single mapable key's down event. One from our
             * handledeactivatedevent and other from the parent window.
             * so, to handle it, we need to prune extra key up events.
             */
			bool shouldForwardKeyEvent = true;
			if (pressed == true && sKeyStateSet.ContainsKey(key) == false)
				sKeyStateSet.Add(key, 1);
			else if (pressed == false && sKeyStateSet.ContainsKey(key) == true)
				sKeyStateSet.Remove(key);
			else if (pressed == false && sKeyStateSet.ContainsKey(key) == false)
				shouldForwardKeyEvent = false;
			return shouldForwardKeyEvent;
		}

		public void HandleKeyEvent(Keys key, bool pressed)
		{
			UpdateUserActivityStatus();
			//Logger.Info("{0,-12} {1:-12} {2}, Scancode = {3}", "KeyEvent",
			// 	key.ToString(), pressed, this.keyboard.NativeToScanCodes(key));

			if (InputMapper.IsMapableKey(this.keyboard.NativeToScanCodes(key)))
			{
				bool shouldForwardKeyEvent = UpdateKeyState(key, pressed);
				if (shouldForwardKeyEvent == false)
					return;
			}

			mInputMapper.DispatchKeyboardEvent(
				this.keyboard.NativeToScanCodes(key), pressed);
		}

		private bool LockdownIsKeyAllowed(Keys key)
		{
			/*
             * Allow the key if console lockdown is disabled or ALT
             * is not depressed.
             */

			if (this.lockdownDisabled || !keyboard.IsAltDepressed())
				return true;

			/*
             * Do not allow any keys that would enable the user to
             * escape to the console while ALT is depressed.
             */

			if (key == Keys.Left || key == Keys.Right ||
				key == Keys.F1 || key == Keys.F2 ||
				key == Keys.F3 || key == Keys.F4)
				return false;

			/*
             * Allow all other keys.
             */

			return true;
		}

		private void UnstickKeyboardModifiers()
		{
			/*
             * Send key up events for every possible modifier
             * key.  This is used to unwedge modifier keys when
             * the frontend establishes a connection to the VM
             * or it gets focus.
             */

			//Logger.Info("Unsticking modifier keys");

			HandleKeyEvent(Keys.LWin, false);
			HandleKeyEvent(Keys.RWin, false);
			HandleKeyEvent(Keys.Apps, false);
			HandleKeyEvent(Keys.Menu, false);
			HandleKeyEvent(Keys.LMenu, false);

			/*
             * Fixing: Freezing of Guest and Host while keep pressing Alt + Tab.
             * Which boils down to kernel sane behavior for stray KEY_UP events specially for LShiftKey
             */
			//HandleKeyEvent(Keys.LShiftKey, false);
			HandleKeyEvent(Keys.RShiftKey, false);
			HandleKeyEvent(Keys.LControlKey, false);
			HandleKeyEvent(Keys.RControlKey, false);
		}

		private void SendLockedKeys()
		{
			Thread thread = new Thread(delegate ()
			{
				lock (sendkeyslock)
				{
					Logger.Info("in sendLockedKeys -- mCapsLocked - {0} and mNumLocked - {1}", Control.IsKeyLocked(Keys.CapsLock), Control.IsKeyLocked(Keys.NumLock));

					if (!isGuestReady)
					{
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							this.isGuestReady = false;
						}
						Logger.Info("return from sendLockedKeys - guest not booted");
						return;
					}
					Logger.Info("in sendLockedKeys - guest booted");
					if (Control.IsKeyLocked(Keys.CapsLock))
					{
						HandleKeyEvent(Keys.CapsLock, true);
						Logger.Info("in sendLockedKeys - sleeping for 100 ms for capslock toggle");
						Thread.Sleep(100);
						HandleKeyEvent(Keys.CapsLock, false);
					}
					if (Control.IsKeyLocked(Keys.NumLock))
					{
						HandleKeyEvent(Keys.NumLock, true);
						Logger.Info("in sendLockedKeys - sleeping for 100 ms for numLock toggle");
						Thread.Sleep(100);
						HandleKeyEvent(Keys.NumLock, false);
					}
				}
			});
			thread.IsBackground = true;
			thread.Start();

		}

		/*
         * Window Handling Routines
         */

		/*
         * Fires whenever the display settings change.  Go ahead and
         * resize the frontend window.
         */

		private void HandleDisplaySettingsChanged(Object sender,
			EventArgs evt)
		{
			Logger.Info("HandleDisplaySettingsChanged()");
			ResizeFrontendWindow();
			SendOrientationToGuest();
		}

		internal void ToggleFullScreen()
		{
			String path = String.Format(@"{0}\{1}\FrameBuffer\0",
					Common.Strings.GuestRegKeyPath, vmName);
			RegistryKey key = Registry.LocalMachine.OpenSubKey(path,
				true);

			if (!mFullScreen)
			{
				mFullScreen = true;
				ResizeFrontendWindow();
				if (Features.IsFeatureEnabled(Features.SHOW_FRONTEND_FULL_SCREEN_TOAST) == true)
				{
					mFullScreenToast.Show();
				}
			}
			else
			{
				mFullScreen = false;
				ResizeFrontendWindow();
				mFullScreenToast.Hide();
			}

			key.SetValue("FullScreen", mFullScreen ? 1 : 0);
			key.Close();
		}

		private void ToggleKeyMapTeachMode()
		{
			s_KeyMapTeachMode = !s_KeyMapTeachMode;
		}

		private void DrawPaneLine(Object obj, PaintEventArgs evt)
		{
			evt.Graphics.DrawLine(Pens.Gray, 0, 0, 0, this.Height);
		}

		/*
         * Resize the frontend window, considering the fullscreen knob
         * and screen orientation.
         */
		private void ResizeFrontendWindow()
		{
			ResizeFrontendWindow(false);
		}
		private void ResizeFrontendWindow(bool forceResize)
		{
			Logger.Info("ResizeFrontendWindow()");

			Logger.Info("Suspending Layout");
			SuspendLayout();

			/*
             * Resize the frontend window.
             *
             * If this is Windows 8 and we are running in full
             * screen, then we need to hide the control bar.
             */

			if (mFullScreen)
			{
				ResizeFrontendWindow_FullScreen();
			}
			else
			{
				ResizeFrontendWindow_Windowed(forceResize);
			}
			if (this.ParentForm != null)
			{
				if (sHideMode)
				{
					this.ParentForm.WindowState = FormWindowState.Minimized;
					isMinimized = true;
				}
				else if (this.ParentForm.WindowState == FormWindowState.Minimized)
				{
					this.ParentForm.WindowState = FormWindowState.Normal;
				}
			}
			/*
             * Resume the window layout to commit our changes and
             * then fixup the guest display.
             */

			Logger.Info("Resuming Layout");
			ResumeLayout();

			FixupGuestDisplay();

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyChangesToParentWindow)
			{
				WindowMessages.NotifyFrontendResizeToParentWindow(mFullScreen);
			}

			Logger.Info("ResizeFrontendWindow DONE");
		}

		private void ResizeFrontendWindow_FullScreen()
		{
			Logger.Info("ResizeFrontendWindow_FullScreen()");

			Logger.Info("Screen size is {0}x{1}",
					Common.Interop.Window.ScreenWidth,
					Common.Interop.Window.ScreenHeight);

			/*
             * Adjust our guest display sizing area parameters.
             */
			if (this.ParentForm != null)
			{
				this.ParentForm.FormBorderStyle = FormBorderStyle.None;
			}
			Logger.Info("Guest display area is {0}x{1}",
					mCurrentDisplaySize.Width, mCurrentDisplaySize.Height);

			/*
             * Enter full screen mode.
             */

			if (mEmulatedPortraitMode && Common.Features.IsFeatureEnabled(Common.Features.RIGHT_ALIGN_PORTRAIT_MODE))
			{
				float desiredRatio = (float)mConfiguredGuestSize.Width /
					(float)mConfiguredGuestSize.Height;

				Size newSize = new Size();
				newSize.Height = Screen.PrimaryScreen.WorkingArea.Height;

				newSize.Height -= Oem.Instance.PartnerControlBarHeight;
				newSize.Width = (int)((float)newSize.Height / desiredRatio);
				int X = Common.Interop.Window.ScreenWidth - newSize.Width;
				int Y = 0;
				int cx = newSize.Width;
				int cy = newSize.Height;

				Common.Interop.Window.SetFullScreen(this.Handle, X, Y, cx, cy);
			}
			else
			{
				if (this.ParentForm != null)
				{
					Common.Interop.Window.SetFullScreen(this.ParentForm.Handle);
				}
			}

			Logger.Info("New client size is {0}x{1}", ClientSize.Width,
					ClientSize.Height);

			Logger.Info("ResizeFrontendWindow_FullScreen DONE");
		}

		private void ResizeFrontendWindow_Windowed(bool forceResize)
		{
			Logger.Info("ResizeFrontendWindow_Windowed()");
			Logger.Info("mEmulatedPortraitMode: " + mEmulatedPortraitMode);

			Size newSize = new Size();

			int left = 20, top = 20;
			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsFrontendFormLocation6)
			{
				left = 6;
				top = 6;
			}

			if (this.ParentForm != null)
			{
				if(!Oem.Instance.IsFrontendBorderHidden)
					this.ParentForm.FormBorderStyle = FormBorderStyle.FixedSingle;
			}

			int usableScreenHeight = Screen.PrimaryScreen.WorkingArea.Height -
				SystemInformation.CaptionHeight - GetBorderWidth(100, 100);

			if (mEmulatedPortraitMode) //&& Common.Features.IsFeatureEnabled(Common.Features.RIGHT_ALIGN_PORTRAIT_MODE)) 
			{
				newSize.Height = usableScreenHeight;
				newSize.Height -= Oem.Instance.PartnerControlBarHeight;
				float desiredRatio = (float)mConfiguredGuestSize.Width /
					(float)mConfiguredGuestSize.Height;

				if (forceResize)
				{
					float providedAspectRatio = (float)this.Height /
						(float)this.Width;
					if (desiredRatio == providedAspectRatio)
					{
						newSize.Height = this.Height;
					}
				}

				newSize.Width = (int)((float)newSize.Height / desiredRatio);

				left = Screen.PrimaryScreen.WorkingArea.Width -
					newSize.Width -
					GetBorderWidth(100, 100) / 2;
				top = GetBorderWidth(100, 100) / 2;

				Logger.Info("location: ({0}x{1})", left, top);

			}
			else if (!IsPortrait())
			{
				newSize.Width = mConfiguredDisplaySize.Width;
				newSize.Height = mConfiguredDisplaySize.Height;

				/*
                 * Use this for centering the frontend window
                 *
                 left = (Screen.PrimaryScreen.WorkingArea.Width - newSize.Width)/2;
                 top = (usableScreenHeight - newSize.Height)/2;

    */

			}
			else
			{
				newSize.Width = mConfiguredDisplaySize.Height;
				newSize.Height = mConfiguredDisplaySize.Width;
				newSize.Height -= Oem.Instance.PartnerControlBarHeight;
				/*
                 * Use this for centering the frontend window
                 *
                 left = (Screen.PrimaryScreen.WorkingArea.Width - newSize.Width)/2;
                 top = (usableScreenHeight - newSize.Height)/2;
                */
			}

			mCurrentDisplaySize = newSize;

			Logger.Info("Guest display area is {0}x{1}",
					mCurrentDisplaySize.Width, mCurrentDisplaySize.Height);

			Logger.Info("New window size is {0}x{1}",
					newSize.Width, newSize.Height);

			if (this.ParentForm != null)
			{
				if(!Oem.Instance.IsFrontendBorderHidden)
					this.ParentForm.FormBorderStyle = FormBorderStyle.FixedSingle;

				this.ParentForm.StartPosition = FormStartPosition.Manual;
				this.ParentForm.Location = new Point(left, top);
				this.ParentForm.ClientSize = newSize;
				Logger.Info("New client size is {0}x{1}", ParentForm.ClientSize.Width,
					ParentForm.ClientSize.Height);
			}
			/*
             * assigning the form icon again
             * we had a bug where when the appplayer launches in full screen mode
             * and when we go back into window mode, the form icon does not display
             */

			Logger.Info("ResizeFrontendWindow_Windowed DONE");
		}

		private void FixupGuestDisplay()
		{
			Logger.Info("FixupGuestDisplay()");
			FixupGuestDisplay_FixAspectRatio();
			FixupGuestDisplay_FixOpenGLSubwindow();
			Logger.Info("FixupGuestDisplay DONE");
		}

		private void FixupGuestDisplay_FixAspectRatio()
		{
			Logger.Info("FixupGuestDisplay_FixAspectRatio()");

			float desiredRatio;

			if (!IsPortrait() && !mEmulatedPortraitMode)
				desiredRatio = (float)mConfiguredGuestSize.Width /
					(float)mConfiguredGuestSize.Height;
			else
				desiredRatio = (float)mConfiguredGuestSize.Height /
					(float)mConfiguredGuestSize.Width;

			float currentRatio = (float)this.Width /
				(float)this.Height;

			Logger.Info("Current aspect ratio {0}, desired {1}",
				currentRatio, desiredRatio);

			/*
             * If the current aspect ratio is too high, then decrease
             * the guest display width to obtain the desired ratio.
             * Otherwise, decrease the guest display height.
             */

			if (currentRatio > desiredRatio)
			{

				Logger.Info("Decreasing guest display width");

				float w = (float)this.Width /
					currentRatio * desiredRatio;

				mScaledDisplayArea.X =
					(this.Width - (int)w) / 2;
				mScaledDisplayArea.Y = 0;

				mScaledDisplayArea.Width = (int)w;
				mScaledDisplayArea.Height = this.Height;

			}
			else
			{

				Logger.Info("Decreasing guest display height");

				float h = (float)this.Height *
					currentRatio / desiredRatio;

				mScaledDisplayArea.X = 0;
				mScaledDisplayArea.Y =
					(this.Height - (int)h) / 2;

				mScaledDisplayArea.Width = this.Width;
				mScaledDisplayArea.Height = (int)h;
			}

			Logger.Debug("the scalled display area is -> " + mScaledDisplayArea.Width + "x" + mScaledDisplayArea.Height + "and postion is " + 
				mScaledDisplayArea.X + "," + mScaledDisplayArea.Y);
			Logger.Info("FixupGuestDisplay_FixAspectRatio DONE");
		}
		private void FixupGuestDisplay_FixOpenGLSubwindow()
		{
			int mode;

			Logger.Info("FixupGuestDisplay_FixOpenGLSubwindow()");

			if (IsPortrait())
				mode = sCurrentOrientation;
			else if (mEmulatedPortraitMode)
				mode = mRotateGuest180 ? 3 : 1;
			else
				mode = mRotateGuest180 ? 2 : 0;

			Logger.Info("OpenGL at ({0},{1}) size ({2},{3}) mode {4}",
				mScaledDisplayArea.X, mScaledDisplayArea.Y,
				mScaledDisplayArea.Width, mScaledDisplayArea.Height, mode);

			int width = mScaledDisplayArea.Width;
			int height = mScaledDisplayArea.Height;

			if (Common.Oem.Instance.IsBTVBuild && !Utils.IsOSWinXP())
			{
				RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath, true);
				int savedOpenGLValueWidth = (int)configKey.GetValue("GuestOpenGlWidth", 0);
				int savedOpenGLValueHeight = (int)configKey.GetValue("GuestOpenGlHeight", 0);

				if (savedOpenGLValueWidth != width || savedOpenGLValueHeight != height)
				{
					configKey.SetValue("GuestOpenGlWidth", width, RegistryValueKind.DWord);
					configKey.SetValue("GuestOpenGlHeight", height, RegistryValueKind.DWord);
					configKey.Close();

					HTTPHandler.SendSetFrontendPositionRequest(width, height);
				}
			}

			/*
             * DPR:  Resizing the GL subwindow to cover the entire
             *       screen can sometimes prevent any other windows,
             *       including the Windows 8 charm menu, from
             *       displaying.  This behavior has been observed
             *       on the AMD Radeon HD 6670 at a resolution of
             *       1920x1080 pixels with all driver updates.
             *
             *       If the user gets stuck in this state, then
             *       he/she won't be able to bring up our control
             *       charm to exit the frontend or leave fullscreen
             *       mode.
             *
             *       It seems we can prevent this problem by leaving
             *       a one pixel gap at the right and bottom edges
             *       of the screen.  The frontend's main form window
             *       covers the entirety of the screen, so we simply
             *       see black in the gap.
             */

			if (mFullScreen)
			{
				width -= 1;
				height -= 1;
			}

			Interop.Opengl.ResizeSubWindow(
				mScaledDisplayArea.X,
				mScaledDisplayArea.Y,
				width,
				height);

			Interop.Opengl.HandleOrientation(1, 1, mode);

			Logger.Info("FixupGuestDisplay_FixOpenGLSubwindow DONE");
		}

		/*
         * Check if running in portrait mode
         */
		private bool IsPortrait()
		{
			ScreenOrientation so = SystemInformation.ScreenOrientation;

			return (so == ScreenOrientation.Angle90) ||
				(so == ScreenOrientation.Angle270);
		}

		/*
         * Should we emulate portrait in landscape mode?
         */
		private bool ShouldEmulatePortraitMode()
		{
			String keyPath;
			RegistryKey key;

			/*
             * Check if our registry override is set.  If so, just
             * return the override value.
             */

			keyPath = String.Format(
			   Common.Strings.RegBasePath + @"\Guests\{0}\FrameBuffer\0",
				vmName);

			using (key = Registry.LocalMachine.OpenSubKey(keyPath))
			{

				Object val = key.GetValue("EmulatePortraitMode");
				if (val != null)
					return (int)val != 0;
			}

			return IsDesktop();
		}

		private static bool IsDesktop()
		{
			if (Features.IsFeatureEnabled(Features.FORCE_DESKTOP_MODE) == true)
			{
				return true;
			}
			if (Utils.IsDesktopPC())
				return true;

			bool emulate;

			/*
             * Checks for the existence of two cameras on the device.
             * Typical Windows 8 tablet devices have both front and
             * rear facing cameras.  Otherwise, this is probably a
             * fixed orientation device.
             */

			try
			{
				List<VideoCapture.DeviceEnumerator> list = VideoCapture.DeviceEnumerator.ListDevices(
					VideoCapture.Guids.VideoInputDeviceCategory);
				emulate = (list.Count != 2);

				foreach (VideoCapture.DeviceEnumerator dev in list)
					dev.Dispose();

			}
			catch (Exception exc)
			{
				Logger.Info("Cannot enumerate camera devices: " +
					exc);
				emulate = false;
			}

			return emulate;
		}

		/*
         * Control Handler Class
         *
         * Used by both the control bar and the control charm.
         */

		private class ControlHandler : IControlHandler
		{

			private Console mConsole;

			public ControlHandler(Console console)
			{
				mConsole = console;
			}

			public void Back()
			{
				if (mConsole.FrontendState != State.Connected)
					return;

				Logger.Info("Back Button Clicked");
				mConsole.HandleKeyEvent(Keys.Escape, true);
				Thread.Sleep(100);
				mConsole.HandleKeyEvent(Keys.Escape, false);
			}

			public void Menu()
			{
				if (mConsole.FrontendState != State.Connected)
					return;

				Logger.Info("Menu Button Clicked");
				mConsole.HandleKeyEvent(Keys.Apps, true);
				mConsole.HandleKeyEvent(Keys.Apps, false);
			}

			public void Home()
			{
				if (mConsole.FrontendState != State.Connected)
					return;

				Logger.Info("Home Button Clicked");
				Common.VmCmdHandler.RunCommand("home");
			}
		}

		/*
         * Share Button Handling
         */

		public void HandleShareButtonClicked()
		{
			int borderWidth = (this.Width - this.ClientSize.Width) / 2;
			int titlebarHeight = this.Height - this.ClientSize.Height - 2 * borderWidth;

			int captureWidth = this.Width + 2 * borderWidth;
			int captureHeight = this.Height + 2 * borderWidth + titlebarHeight;

			System.Drawing.Bitmap bmp = new System.Drawing.Bitmap(captureWidth, captureHeight);
			Graphics g = Graphics.FromImage(bmp);
			g.CopyFromScreen(new Point(this.Left, this.Top), Point.Empty, new Size(this.Width, this.Height));

			Random random = new Random();
			int randomNum = random.Next(0, 100000);

			string fileBaseName = String.Format(@"bstSnapshot_{0}.jpg", randomNum);
			string finalBaseName = String.Format(@"final_{0}", fileBaseName);

			string configPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
			RegistryKey key = Registry.LocalMachine.OpenSubKey(configPath);
			int fileSystem = (int)key.GetValue("FileSystem", 0);
			if (fileSystem == 0)
			{
				Logger.Info("Shared folders disabled");
				return;
			}

			string sharedFolder = Common.Strings.SharedFolderDir;
			string sharedFolderName = Common.Strings.SharedFolderName;

			string origFileName = Path.Combine(sharedFolder, fileBaseName);
			string modifiedFileName = Path.Combine(sharedFolder, finalBaseName);

			bmp.Save(origFileName, ImageFormat.Jpeg);

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

			string url = String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort, Common.Strings.SharePicUrl);

			string androidPath = "/mnt/sdcard/windows/" +
				sharedFolderName + "/" + Path.GetFileName(finalBaseName);
			Logger.Info("androidPath: " + androidPath);

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

			// if no sharing app is installed, show an error message
			if (result.Contains("error") && this.snapshotErrorShown == false)
			{
				this.snapshotErrorShown = true;
				this.snapshotErrorToast = new Toast(this, Locale.Strings.SnapshotErrorToastText);
				Interop.Animate.AnimateWindow(this.snapshotErrorToast.Handle, 500, Interop.Animate.AW_SLIDE | Interop.Animate.AW_VER_POSITIVE);
				this.snapshotErrorToast.Show();

				Thread t = new Thread(delegate ()
				{
					Thread.Sleep(3000);
					Interop.Animate.AnimateWindow(this.snapshotErrorToast.Handle, 500,
							Interop.Animate.AW_HIDE | Interop.Animate.AW_SLIDE | Interop.Animate.AW_VER_NEGATIVE);
					this.snapshotErrorShown = false;
				});
				t.IsBackground = true;
				t.Start();
			}
		}

		/*
         * Translate a window relative x value to a guest relative x
         * value.
         */

		private int GetLandscapeGuestX(int x, int y)
		{
			int landscapeGuestX = x - mScaledDisplayArea.X;
			if (mScaledDisplayArea.Width == 0)
				return 0;
			landscapeGuestX = (int)((float)landscapeGuestX * GUEST_ABS_MAX_X / mScaledDisplayArea.Width);

			System.Diagnostics.Debug.WriteLine("the value of x:" + x + " and y:" + y + " and landscapeGuestx:" + landscapeGuestX);
			return landscapeGuestX;
		}

		private int GetPortraitGuestX(int x, int y)
		{
			int portraitGuestX = y - mScaledDisplayArea.Y;
			if (mScaledDisplayArea.Height == 0)
				return 0;
			portraitGuestX = (int)((float)portraitGuestX * GUEST_ABS_MAX_Y / mScaledDisplayArea.Height);

			return portraitGuestX;
		}

		private int GetGuestX(int x, int y)
		{
			int guestX = 0;

			int landscapeGuestX = GetLandscapeGuestX(x, y);
			int portraitGuestX = GetPortraitGuestX(x, y);

			if (!IsPortrait() && !mEmulatedPortraitMode)
			{
				if (!mRotateGuest180)
					guestX = landscapeGuestX;
				else
					guestX = GUEST_ABS_MAX_X - landscapeGuestX;
			}
			else
			{
				if (!mRotateGuest180)
					guestX = GUEST_ABS_MAX_Y - portraitGuestX;
				else
					guestX = portraitGuestX;
			}

			return guestX;
		}

		/*
         * Translate a window relative y value to a guest relative y
         * value.
         */

		private int GetLandscapeGuestY(int x, int y)
		{
			int landscapeGuestY = y - mScaledDisplayArea.Y;
			if (mScaledDisplayArea.Height == 0)
				return 0;
			landscapeGuestY = (int)((float)landscapeGuestY * GUEST_ABS_MAX_Y / mScaledDisplayArea.Height);

			return landscapeGuestY;
		}

		private int GetPortraitGuestY(int x, int y)
		{
			int portraitGuestY = x - mScaledDisplayArea.X;
			if (mScaledDisplayArea.Width == 0)
				return 0;
			portraitGuestY = (int)((float)portraitGuestY * GUEST_ABS_MAX_X / mScaledDisplayArea.Width);

			return portraitGuestY;
		}

		private int GetGuestY(int x, int y)
		{
			int guestY = 0;

			int landscapeGuestY = GetLandscapeGuestY(x, y);
			int portraitGuestY = GetPortraitGuestY(x, y);

			if (!IsPortrait() && !mEmulatedPortraitMode)
			{
				if (!mRotateGuest180)
					guestY = landscapeGuestY;
				else
					guestY = GUEST_ABS_MAX_Y - landscapeGuestY;
			}
			else
			{
				if (!mRotateGuest180)
					guestY = portraitGuestY;
				else
					guestY = GUEST_ABS_MAX_X - portraitGuestY;
			}

			return guestY;
		}

		public bool IsFrontendReparented()
		{
			if (GetParent(this.Handle) == IntPtr.Zero)
				return false;
			return true;
		}

		public void S2PScreenShown()
		{
			sS2PScreenShown = true;

			/*
             * XXXDPR:  No clue what this is supposed to do.
             */

			if (!this.guestFinishedBooting)
			{
				this.guestFinishedBooting = true;

				if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
				{
					if (!this.checkingIfGuestReady && !this.isGuestReady)
					{
						CheckIfGuestReady();
					}
				}

			}
		}
	}

}
