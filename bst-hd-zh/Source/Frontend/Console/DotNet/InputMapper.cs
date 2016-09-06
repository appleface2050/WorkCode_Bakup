using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class InputMapper
	{

		public const int CURSOR_SLOTS = 4;

		private const int GAMEPAD_AXIS_MAX = 1000;
		private const int CURSOR_MOVE_FACTOR = 10;

		private const String TEMPLATE = "TEMPLATE.cfg";
		private const String DEFAULT = "DEFAULT.cfg";

		private const int MAX_X = Console.GUEST_ABS_MAX_X - 1;
		private const int MAX_Y = Console.GUEST_ABS_MAX_Y - 1;

		private const int MOUSE_SLOT = 4;
		private const int MOUSE_SHOOT_SLOT = 5;

		public static bool s_IsKeyMappingEnabled = true;

		[StructLayout(LayoutKind.Sequential)]
		public struct TouchPoint
		{
			public float X;
			public float Y;
			public bool Down;
		};

		public enum Direction
		{
			None = 0,
			Up,
			Down,
			Left,
			Right,
		};

		public enum GamepadEvent
		{
			None = 0,
			Attach,
			Detach,
			GuidancePress,
			GuidanceRelease,
		};

		public delegate void ModeHandlerNet(String mode);

		private delegate void KeyHandler(IntPtr context, byte code);
		private delegate void TouchHandler(IntPtr context, IntPtr list,
			int count, int offset);
		private delegate void TiltHandler(IntPtr context, float x, float y,
			float z);
		private delegate void ModeHandler(IntPtr context, String mode);
		private delegate void MoveHandler(IntPtr context, int identity,
			int x, int y);
		private delegate void ClickHandler(IntPtr context, int identity,
			int down);
		private delegate void SpecialHandler(IntPtr context, String cmd);
		private delegate void GamepadHandler(IntPtr context, int identity,
			GamepadEvent evt, String layout);

		private delegate void LogHandler(String msg);

		private const String INPUT_MAPPER_DLL = "HD-InputMapper-Native.dll";

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperInit(
			KeyHandler keyHandler, IntPtr keyContext,
			TouchHandler touchHandler, IntPtr touchContext,
			TiltHandler tiltHandler, IntPtr tiltContext,
			ModeHandler modeHandler, IntPtr modeContext,
			MoveHandler moveHandler, IntPtr moveContext,
			ClickHandler clickHandler, IntPtr clickContext,
			SpecialHandler specialHandler, IntPtr specialContext,
			GamepadHandler gamepadHandler, IntPtr gamepadContext,
			String defaultConfig, LogHandler logger, int verbose);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperSetLocale(String locale);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperSetEmulatedSwipeKnobs(
			float length, int duration);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperSetEmulatedPinchKnobs(
			float split, float lengthIn, float lengthOut, int duration);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperIsLocationUpdationWithKeyMapEnabled();

		[DllImport(INPUT_MAPPER_DLL, CharSet = CharSet.Unicode)]
		private static extern int InputMapperIsShootingModeEnabled(
				ref float triggerXPos,
				ref float triggerYPos,
				ref float originXPos,
				ref float originYPos,
				ref int sensitivity,
				ref int touchShootModeType,
				ref bool isSpaceShooterModeEnabled,
				ref bool isSingleTouchShootModeEnabled
				);

		[DllImport(INPUT_MAPPER_DLL, CharSet = CharSet.Unicode)]
		private static extern int GetParserVersion();

		[DllImport(INPUT_MAPPER_DLL, CharSet = CharSet.Unicode)]
		private static extern int InputMapperLoadConfig(
			[MarshalAs(UnmanagedType.LPWStr)] String path,
			[MarshalAs(UnmanagedType.LPWStr)] String userPath,
			[MarshalAs(UnmanagedType.LPStr)] String name);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperHandleKey(uint code, int down);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperSetKeyMappingState(int state);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperSetShootingModeControlsState(int state);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperHandleController(int identity,
			uint button, int down);

		[DllImport(INPUT_MAPPER_DLL, CharSet = CharSet.Unicode)]
		private static extern int InputMapperHandleGamePadAttach(
			int identity, int vendor, int product, String dbPath);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperHandleGamePadDetach(
			int identity);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperHandleGamePadUpdate(
			int identity, ref Common.GamePad gamepad);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperHandleCharacter(char ch);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperEmulateSwipe(float x, float y,
			Direction direction);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperEmulateSwipeXY(float x1, float y1,
			float x2, float y2);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern int InputMapperEmulatePinch(float x, float y,
			int zoomIn);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperScrollBegin(float x, float y);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperScrollUpdate(float dx,
			float dy);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperScrollEnd();

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperZoomBegin(float x, float y);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperZoomUpdate(float dz);

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperZoomEnd();

		[DllImport(INPUT_MAPPER_DLL)]
		private static extern void InputMapperHandleShakeAction(float x, float y, float z);

		[DllImport(INPUT_MAPPER_DLL)]
		public static extern void InputMapperUpdateMouseCoordinates(float x, float y, bool down, int delay);

		[DllImport(INPUT_MAPPER_DLL)]
		public static extern void InputMapperUpdateShootCoordinates(float x, float y, bool down, int delay);

		[DllImport("User32.dll")]
		private static extern uint MapVirtualKey(uint code, uint mapType);

		private static InputMapper sInstance = new InputMapper();

		private Console mConsole;

		private String mFolder;
		private String mUserFolder;
		private Interop.Monitor mMonitor;
		private Interop.Monitor.TouchPoint[] mTouchPoints;
		private bool mEmulatedPortraitMode = false;
		private bool mRotateGuest180 = false;
		private String mCurrentPackage;
		private int mIsShootingApp;
		private int mIsLocationSetterApp;
		private bool mEmulatedGestureInProgress;

		private SerialWorkQueue mSerialQueue;

		private SensorDevice mSensor;
		private BstCursor mCursor;
		private IControlHandler mControlHandler;
		private ModeHandlerNet mModeHandlerNet;

		private System.Threading.Timer mCursorTimer;
		private Object mCursorLock = new Object();
		private Point[] mCursorDeltas;

		private KeyHandler mKeyHandler;     /* to prevent GC */
		private TouchHandler mTouchHandler;     /* to prevent GC */
		private TiltHandler mTiltHandler;       /* to prevent GC */
		private ModeHandler mModeHandler;       /* to prevent GC */
		private MoveHandler mMoveHandler;       /* to prevent GC */
		private ClickHandler mClickHandler;     /* to prevent GC */
		private SpecialHandler mSpecialHandler; /* to prevent GC */
		private GamepadHandler mGamepadHandler; /* to prevent GC */
		private LogHandler mLogger;     /* to prevent GC */

		public float mSoftControlBarHeightLandscape;
		public float mSoftControlBarHeightPortrait;
		public bool mSoftControlEnabled;

		private static uint[] sMapableKeyArray = new uint[] {
		0x02,
			0x03,
			0x04,
			0x05,
			0x06,
			0x07,
			0x08,
			0x09,
			0x0a,
			0x0b,
			0x10,
			0x11,
			0x12,
			0x13,
			0x14,
			0x15,
			0x16,
			0x17,
			0x18,
			0x19,
			0x1e,
			0x1f,
			0x20,
			0x21,
			0x22,
			0x23,
			0x24,
			0x25,
			0x26,
			0x2c,
			0x2d,
			0x2e,
			0x2f,
			0x30,
			0x31,
			0x32,
			0x39,
			0x1c,
			0xe048,
			0xe050,
			0xe04b,
			0xe04d
	};

		private static Dictionary<uint, int> sMapableKeySet = null;

		public static InputMapper Instance()
		{
			return sInstance;
		}

		public static bool IsMapableKey(uint scanCode)
		{
			if (sMapableKeySet == null)
			{
				sMapableKeySet = new Dictionary<uint, int>();
				for (int i = 0; i < sMapableKeyArray.Length; i++)
					sMapableKeySet.Add(sMapableKeyArray[i], 1);
			}
			return sMapableKeySet.ContainsKey(scanCode);
		}

		public static string GetKeyMappingParserVersion()
		{
			int parserVersion = GetParserVersion();
			Logger.Info("the parserVersion returned is {0}", parserVersion);
			return parserVersion.ToString();
		}

		public void Init(String folder, bool verbose, SensorDevice sensor,
			BstCursor cursor)
		{
			mFolder = folder;
			mUserFolder = Path.Combine(mFolder, "UserFiles");
			mSensor = sensor;
			mCursor = cursor;

			/*
			 * Initialize the touch point array we'll use to deliver
			 * touch events to the monitor.
			 */

			mTouchPoints =
				new Interop.Monitor.TouchPoint[Console.TOUCH_POINTS_MAX];

			for (int ndx = 0; ndx < mTouchPoints.Length; ndx++)
				mTouchPoints[ndx] =
					new Interop.Monitor.TouchPoint(0xffff, 0xffff);

			mCursorDeltas = new Point[CURSOR_SLOTS];

			/*
			 * Create our handlers and call the DLL init routine.
			 */

			mKeyHandler = new KeyHandler(KeyHandlerImpl);
			mTouchHandler = new TouchHandler(TouchHandlerImpl);
			mTiltHandler = new TiltHandler(TiltHandlerImpl);
			mModeHandler = new ModeHandler(ModeHandlerImpl);
			mMoveHandler = new MoveHandler(MoveHandlerImpl);
			mClickHandler = new ClickHandler(ClickHandlerImpl);
			mSpecialHandler = new SpecialHandler(SpecialHandlerImpl);
			mGamepadHandler = new GamepadHandler(GamepadHandlerImpl);

			mLogger = new LogHandler(delegate (String msg)
			{
				Logger.Info("InputMapper: " + msg);
			});

			InputMapperInit(
				mKeyHandler, IntPtr.Zero,
				mTouchHandler, IntPtr.Zero,
				mTiltHandler, IntPtr.Zero,
				mModeHandler, IntPtr.Zero,
				mMoveHandler, IntPtr.Zero,
				mClickHandler, IntPtr.Zero,
				mSpecialHandler, IntPtr.Zero,
				mGamepadHandler, IntPtr.Zero,
				mFolder + @"\" + DEFAULT,
				mLogger, verbose ? 1 : 0);

			/*
			 * Set the locale used by InputMapper.
			 */

			InputMapperSetLocale(CultureInfo.CurrentCulture.Name);

			/*
			 * Spin up the work queue that we'll use to serialize
			 * operations.
			 */

			mSerialQueue = new SerialWorkQueue();
			mSerialQueue.Start();

			/*
			 * Fire up the cursor movement timer.
			 */

			mCursorTimer = new System.Threading.Timer(MoveHandlerTick, null, 0, 15);
		}

		public void SetSoftControlBarHeight(float landscape,
			float portrait)
		{
			Logger.Info("SetSoftControlBarHeight({0}, {1})",
				landscape, portrait);

			mSoftControlBarHeightLandscape = landscape;
			mSoftControlBarHeightPortrait = portrait;
		}

		public void OverrideLocale(String locale)
		{
			InputMapperSetLocale(locale);
		}

		public void SetConsole(Console console)
		{
			mConsole = console;
		}

		public void SetControlHandler(IControlHandler handler)
		{
			mControlHandler = handler;
		}

		public void SetModeHandler(ModeHandlerNet handler)
		{
			mModeHandlerNet = handler;
		}

		public void SetEmulatedSwipeKnobs(float length, int duration)
		{
			InputMapperSetEmulatedSwipeKnobs(length, duration);
		}

		public void SetEmulatedPinchKnobs(float split, float lengthIn,
			float lengthOut, int duration)
		{
			InputMapperSetEmulatedPinchKnobs(split, lengthIn, lengthOut,
				duration);
		}

		public void SetMonitor(Interop.Monitor monitor)
		{
			mMonitor = monitor;
		}

		public void SetDisplay(bool emulatedPortraitMode, bool rotateGuest180)
		{
			mEmulatedPortraitMode = emulatedPortraitMode;
			mRotateGuest180 = rotateGuest180;
		}

		public void SetPackage(String package)
		{
			mCurrentPackage = package;
			String name = package + ".cfg";
			String path = Path.Combine(mFolder, name);
			String userPath = Path.Combine(mUserFolder, name);

			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperLoadConfig(path, userPath, package);

				mIsLocationSetterApp = InputMapperIsLocationUpdationWithKeyMapEnabled();

				if (mIsLocationSetterApp == 1)
				{
					Logger.Info("Location setting with KeyMapping enabled for package {0}", package);
					Console.s_Console.UpdateGpsLocation(true);
				}

				float triggerXPos = 0, triggerYPos = 0, originXPos = 0, originYPos = 0;
				mIsShootingApp = InputMapperIsShootingModeEnabled(
					ref triggerXPos,
					ref triggerYPos,
					ref originXPos,
					ref originYPos,
					ref Console.s_ShootSensitivity,
					ref Console.s_touchShootModeType,
					ref Console.s_IsSpaceShooterModeEnabled,
					ref Console.s_IsSingleTouchShootModeEnabled
					);

				if (mIsShootingApp == 0)
				{
					return;
				}

				Console.s_ShootOriginXPos = originXPos;
				Console.s_ShootOriginYPos = originYPos;
				Console.s_ShootTriggerXPos = (int)(triggerXPos * MAX_X);
				Console.s_ShootTriggerYPos = (int)(triggerYPos * MAX_Y);
				Logger.Info("ShootTriggerXPos = {0}, ShootTriggerYPos = {1}, s_ShootOriginXPos = {2}, s_ShootOriginYPos = {3},ShootSensitivity = {4}, SpaceShooterModeEnabled = {5}, TouchShootModeExEnabled = {6}, SingleTouchShootModeEnabled = {7}",
					Console.s_ShootTriggerXPos,
					Console.s_ShootTriggerYPos,
					Console.s_ShootOriginXPos,
					Console.s_ShootOriginYPos,
					Console.s_ShootSensitivity,
					Console.s_touchShootModeType,
					Console.s_IsSpaceShooterModeEnabled,
					Console.s_IsSingleTouchShootModeEnabled
					);
			});
		}

		internal int IsLocationUpdationWithKeyMapEnabled()
		{
			return mIsLocationSetterApp;
		}
		public int IsShootingModeEnabled()
		{
			return mIsShootingApp;
		}

		public void ShowConfigDialog()
		{
			String package = mCurrentPackage;

			InputMapperForm form = new InputMapperForm(package,
				EditHandler, ManageHandler);
			form.ShowDialog();
		}

		private void EditHandler(String package)
		{
			String name = package + ".cfg";
			String temp = Path.Combine(mFolder, TEMPLATE);
			String path = Path.Combine(mFolder, name);
			String userPath = Path.Combine(mUserFolder, name);

			if (!File.Exists(userPath) && File.Exists(path))
				File.Copy(path, userPath);

			path = userPath;
			Logger.Info("Editing input mapper file '{0}'", path);

			try
			{
				if (!File.Exists(path))
				{
					File.Copy(temp, path);
				}

				Process proc = new Process();
				proc.StartInfo.FileName = "notepad.exe";
				proc.StartInfo.Arguments = "\"" + path + "\"";
				proc.Start();

			}
			catch (Exception exc)
			{

				Logger.Error("Cannot edit input mapper file: " +
					exc.ToString());
			}
		}

		private void ManageHandler(string package)
		{
			String name = package + ".cfg";
			String userPath = Path.Combine(mUserFolder, name);
			String openFolder = mFolder;
			if (File.Exists(userPath))
				openFolder = mUserFolder;
			try
			{
				Process proc = new Process();
				proc.StartInfo.FileName = openFolder;
				proc.Start();

			}
			catch (Exception exc)
			{

				Logger.Error("Cannot open input mapper folder: " +
					exc.ToString());
			}
		}

		public void DispatchKeyboardEvent(uint code, bool down)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperHandleKey(code, down ? 1 : 0);
			});
		}

		public void SetKeyMappingState(bool state)
		{
			Logger.Info("Setting Key Mapping State = {0}", state);
			mSerialQueue.Enqueue(delegate ()
			{
				s_IsKeyMappingEnabled = state;
				InputMapperSetKeyMappingState(state ? 1 : 0);
			});
		}

		public void DispatchCharacter(char ch)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperHandleCharacter(ch);
			});
		}

		public void DispatchControllerEvent(int identity, uint button,
			int down)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperHandleController(identity, button, down);
			});
		}

		public void DispatchGamePadAttach(int identity, int vendor,
			int product)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperHandleGamePadAttach(identity, vendor,
					product, Path.Combine(mFolder, "GamePads.db"));
			});
		}

		public void DispatchGamePadDetach(int identity)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperHandleGamePadDetach(identity);
			});
		}

		public void DispatchGamePadUpdate(int identity, Common.GamePad gamepad)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperHandleGamePadUpdate(identity,
					ref gamepad);
			});
		}

		public void EmulateSwipeXY(float x1, float y1, float x2, float y2)
		{
			if (mEmulatedGestureInProgress)
				return;

			mEmulatedGestureInProgress = true;

			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperEmulateSwipeXY(x1, y1, x2, y2);
				mEmulatedGestureInProgress = false;
			});

		}
		public void EmulateSwipe(float x, float y, Direction direction)
		{
			if (mEmulatedGestureInProgress)
				return;

			mEmulatedGestureInProgress = true;

			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperEmulateSwipe(x, y, direction);
				mEmulatedGestureInProgress = false;
			});
		}

		public void EmulatePinch(float x, float y, bool zoomIn)
		{
			if (mEmulatedGestureInProgress)
				return;

			mEmulatedGestureInProgress = true;

			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperEmulatePinch(x, y, zoomIn ? 1 : 0);
				mEmulatedGestureInProgress = false;
			});
		}

		public void ScrollBegin(float x, float y)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperScrollBegin(x, y);
			});
		}

		public void ScrollUpdate(float dx, float dy)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperScrollUpdate(dx, dy);
			});
		}

		public void SetShootingModeControls(bool state)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperSetShootingModeControlsState(state ? 1 : 0);
			});
		}

		public void ScrollEnd()
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperScrollEnd();
			});
		}

		public void ZoomBegin(float x, float y)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperZoomBegin(x, y);
			});
		}

		public void ZoomUpdate(float dz)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperZoomUpdate(dz);
			});
		}

		public void ZoomEnd()
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperZoomEnd();
			});
		}

		private void KeyHandlerImpl(IntPtr context, byte code)
		{
			try
			{
				mMonitor.SendScanCode(code);

			}
			catch (Exception exc)
			{
				Logger.Error("Cannot send keyboard scan code: " +
					exc.ToString());
			}
		}

		private void TouchHandlerImpl(IntPtr context, IntPtr array,
			int count, int offset)
		{
			TouchHandlerImpl(array, count, offset, true);
		}

		public void TouchHandlerImpl(IntPtr array, int count, int offset)
		{
			TouchHandlerImpl(array, count, offset, true);
		}

		public void TouchHandlerImpl(IntPtr array, int count, int offset,
			bool adjustForControlBar)
		{
			try
			{
				TouchHandlerImplInternal(array, count, offset,
					adjustForControlBar);

			}
			catch (Exception exc)
			{
				Logger.Error("Cannot send mapped touch points: " +
					exc.ToString());
			}
		}

		public void TouchHandlerImplInternal(IntPtr array, int count,
			int offset, bool adjustForControlBar)
		{
			TouchPoint[] points = new TouchPoint[count];
			int size = Marshal.SizeOf(typeof(TouchPoint));

			for (int ndx = 0; ndx < count; ndx++)
			{

				IntPtr ptr = new IntPtr(array.ToInt64() + ndx * size);
				points[ndx] = (TouchPoint)
					Marshal.PtrToStructure(ptr, typeof(TouchPoint));
			}

			TouchHandlerImpl(points, offset, adjustForControlBar);
		}

		public void TouchHandlerImpl(TouchPoint[] points, int offset,
			bool adjustForControlBar)
		{
			for (int ndx = 0;
				ndx + offset < mTouchPoints.Length && ndx < points.Length;
				ndx++)
			{

				TouchPoint input = points[ndx];
				if (!input.Down)
				{
					mTouchPoints[ndx + offset].PosX = -1;
					mTouchPoints[ndx + offset].PosY = -1;
					continue;
				}

				int x = (int)(input.X * MAX_X);
				int y = (int)(input.Y * MAX_Y);
				float barHeight = 0;

				if (!mEmulatedPortraitMode)
				{

					if (adjustForControlBar)
						barHeight =
							mSoftControlBarHeightLandscape;

					if ((ndx + offset) != MOUSE_SLOT || (ndx + offset) != MOUSE_SHOOT_SLOT)
						y = (int)(y * (1 - barHeight));

					if (!mRotateGuest180)
					{

						mTouchPoints[ndx + offset].PosX = x;
						mTouchPoints[ndx + offset].PosY = y;

					}
					else
					{

						mTouchPoints[ndx + offset].PosX =
							(int)(MAX_X - x);
						mTouchPoints[ndx + offset].PosY =
							(int)(MAX_Y - y);
					}
				}
				else
				{

					if (adjustForControlBar)
						barHeight =
							mSoftControlBarHeightPortrait;

					if ((ndx + offset) != MOUSE_SLOT || (ndx + offset) != MOUSE_SHOOT_SLOT)
						y = (int)(y * (1 - barHeight));

					if (!mRotateGuest180)
					{

						mTouchPoints[ndx + offset].PosX =
							(int)(MAX_Y - y);
						mTouchPoints[ndx + offset].PosY = x;

					}
					else
					{

						mTouchPoints[ndx + offset].PosX = y;
						mTouchPoints[ndx + offset].PosY =
							(int)(MAX_X - x);
					}
				}
			}

			if (mMonitor != null)
				mMonitor.SendTouchState(mTouchPoints);
		}

		public void EmulateShake(float x, float y, float z)
		{
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperHandleShakeAction(x, y, z);
			});
		}

		public void UpdateMouseCoordinates(int guestX, int guestY, bool down)
		{
			float x = guestX / (float)MAX_X;
			float y = guestY / (float)MAX_Y;
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperUpdateMouseCoordinates(x, y, down, 0);
			});
		}

		public void UpdateMouseCoordinates(int guestX, int guestY, bool down, uint delay)
		{
			float x = guestX / (float)MAX_X;
			float y = guestY / (float)MAX_Y;
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperUpdateMouseCoordinates(x, y, down, (int)delay);
			});
		}

		public void UpdateShootCoordinates(int guestX, int guestY, bool down)
		{
			float x = guestX / (float)MAX_X;
			float y = guestY / (float)MAX_Y;
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperUpdateShootCoordinates(x, y, down, 0);
			});
		}

		public void UpdateShootCoordinates(int guestX, int guestY, bool down, uint delay)
		{
			float x = guestX / (float)MAX_X;
			float y = guestY / (float)MAX_Y;
			mSerialQueue.Enqueue(delegate ()
			{
				InputMapperUpdateShootCoordinates(x, y, down, (int)delay);
			});
		}

		private void TiltHandlerImpl(IntPtr context, float x, float y,
			float z)
		{
			mSensor.SetAccelerometerVector(x, y, z);
		}

		private void ModeHandlerImpl(IntPtr context, String mode)
		{
			mModeHandlerNet(mode);
		}

		private void MoveHandlerImpl(IntPtr context, int identity,
			int x, int y)
		{
			if (identity < 0 || identity >= CURSOR_SLOTS)
				return;

			lock (mCursorLock)
			{
				mCursorDeltas[identity].X = x;
				mCursorDeltas[identity].Y = y;
			}
		}

		private void MoveHandlerTick(Object obj)
		{
			Point[] points = new Point[CURSOR_SLOTS];
			int ndx;

			lock (mCursorLock)
			{
				for (ndx = 0; ndx < CURSOR_SLOTS; ndx++)
					points[ndx] = mCursorDeltas[ndx];
			}

			for (ndx = 0; ndx < CURSOR_SLOTS; ndx++)
			{

				if (points[ndx].X == 0 && points[ndx].Y == 0)
					continue;

				float dx = (float)points[ndx].X / GAMEPAD_AXIS_MAX *
					CURSOR_MOVE_FACTOR;
				float dy = (float)points[ndx].Y / GAMEPAD_AXIS_MAX *
					CURSOR_MOVE_FACTOR;

				mCursor.Move(ndx, dx, dy, false);
			}
		}

		private void ClickHandlerImpl(IntPtr context, int identity,
			int down)
		{
			mCursor.Click(identity, down != 0);
		}

		private void SpecialHandlerImpl(IntPtr context, String cmd)
		{
			//Logger.Info("InputMapper.SpecialHandlerImpl -> " + cmd);

			if (cmd == "Back")
				mControlHandler.Back();
			else if (cmd == "Menu")
				mControlHandler.Menu();
			else if (cmd == "Home")
				mControlHandler.Home();
		}

		private void GamepadHandlerImpl(IntPtr context, int identity,
			GamepadEvent evt, String layout)
		{
			Logger.Info("InputMapper.GamepadHandlerImpl {0} {1} {2}",
				identity, evt, layout);

			switch (evt)
			{

				case GamepadEvent.Attach:
					mCursor.Attach(identity);
					mConsole.HandleControllerAttach(true, identity,
						layout);
					break;

				case GamepadEvent.Detach:
					mCursor.Detach(identity);
					mConsole.HandleControllerAttach(false, identity,
						layout);
					break;

				case GamepadEvent.GuidancePress:
					mConsole.HandleControllerGuidance(true, identity,
						layout);
					break;

				case GamepadEvent.GuidanceRelease:
					mConsole.HandleControllerGuidance(false, identity,
						layout);
					break;
			}
		}
	}

}
