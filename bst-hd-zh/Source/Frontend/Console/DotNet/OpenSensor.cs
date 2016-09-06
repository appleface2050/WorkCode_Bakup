using System;
using System.Drawing;
using System.Management;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class OpenSensor
	{

		public const int IDENTITY_OFFSET = 16;

		private String CFG_KEY =
			Common.Strings.HKLMAndroidConfigRegKeyPath;
		private const String OPENSENSOR_DEVICEID_KEY =
			"OpenSensorDeviceId";

		private enum GestureEvent
		{
			Begin = 0,
			Update,
			End,
		};

		private delegate void ConnectHandler(IntPtr context, int identity,
			int connected, int clientCount);
		private delegate void TrackPadMoveHandler(IntPtr context,
			int identity, float x, float y, int absolute);
		private delegate void TrackPadClickHandler(IntPtr context,
			int identity, int down);
		private delegate void TrackPadScrollHandler(IntPtr context,
			int identity, float dx, float dy, GestureEvent evt);
		private delegate void TrackPadZoomHandler(IntPtr context,
			int identity, float dz, GestureEvent evt);
		private delegate void TouchHandler(IntPtr context, int identity,
			IntPtr list, int count);
		private delegate void KeyboardHandler(IntPtr context, int identity,
			char ch);
		private delegate void ControllerHandler(IntPtr context,
			int identity, int button, int down);
		private delegate void AccelerometerHandler(IntPtr context,
			int identity, float x, float y, float z);
		private delegate void SpecialHandler(IntPtr context, int identity,
			String cmd);

		private delegate void LoggerCallback(String msg);

		private const String OPEN_SENSOR_DLL = "HD-OpenSensor-Native.dll";

		[DllImport(OPEN_SENSOR_DLL)]
		private static extern int OpenSensorInit(
			LoggerCallback logger, int beaconPort, int beaconInterval,
			ConnectHandler connectHandler, IntPtr connectContext,
			TrackPadMoveHandler moveHandler, IntPtr moveContext,
			TrackPadClickHandler clickHandler, IntPtr clickContext,
			TrackPadScrollHandler scrollHandler, IntPtr scrollContext,
			TrackPadZoomHandler zoomHandler, IntPtr zoomContext,
			TouchHandler touchHandler, IntPtr touchContext,
			KeyboardHandler keyboardHandler, IntPtr keyboardContext,
			ControllerHandler ctrlHandler, IntPtr ctrlContext,
			AccelerometerHandler accelHandler, IntPtr accelContext,
			SpecialHandler specialHandler, IntPtr specialContext,
			String deviceType, String deviceId, int verbose);

		[DllImport(OPEN_SENSOR_DLL)]
		private static extern void OpenSensorRunServerInet();

		[DllImport(OPEN_SENSOR_DLL)]
		private static extern void OpenSensorRunServerBluetooth();

		[DllImport(OPEN_SENSOR_DLL)]
		private static extern void OpenSensorRunBeacon();

		[DllImport(OPEN_SENSOR_DLL)]
		private static extern void OpenSensorAdvertiseBeacon(String service,
			int port);

		[DllImport(OPEN_SENSOR_DLL)]
		private static extern void OpenSensorSetMode(String mode);

		private InputMapper mInputMapper;
		private int mBeaconPort;
		private int mBeaconInterval;
		private String mDeviceType;
		private SensorDevice mSensorDevice;
		private BstCursor mCursor;
		private bool mVerbose;

		private Console mConsole;
		private IControlHandler mControlHandler;

		private ConnectHandler mConnectHandler; /* no GC */
		private TrackPadMoveHandler mTrackPadMoveHandler;   /* no GC */
		private TrackPadClickHandler mTrackPadClickHandler; /* no GC */
		private TrackPadScrollHandler mTrackPadScrollHandler;   /* no GC */
		private TrackPadZoomHandler mTrackPadZoomHandler;   /* no GC */
		private TouchHandler mTouchHandler;     /* no GC */
		private KeyboardHandler mKeyboardHandler;   /* no GC */
		private ControllerHandler mControllerHandler;   /* no GC */
		private AccelerometerHandler mAccelerometerHandler; /* no GC */
		private SpecialHandler mSpecialHandler; /* no GC */

		private LoggerCallback mLoggerCallback; /* prevent GC */

		public OpenSensor(InputMapper inputMapper, int beaconPort,
			int beaconInterval, String deviceType,
			SensorDevice sensorDevice, BstCursor cursor, bool verbose)
		{
			mInputMapper = inputMapper;
			mBeaconPort = beaconPort;
			mBeaconInterval = beaconInterval;
			mDeviceType = deviceType;
			mSensorDevice = sensorDevice;
			mCursor = cursor;
			mVerbose = verbose;

			mLoggerCallback = new LoggerCallback(delegate (String msg)
			{
				Logger.Info("OpenSensor: " + msg);
			});
		}

		public void SetConsole(Console console)
		{
			mConsole = console;
		}

		public void SetControlHandler(IControlHandler handler)
		{
			mControlHandler = handler;
		}

		public void Start()
		{
			mConnectHandler =
				new ConnectHandler(ConnectHandlerImpl);
			mTrackPadMoveHandler =
				new TrackPadMoveHandler(TrackPadMoveHandlerImpl);
			mTrackPadClickHandler =
				new TrackPadClickHandler(TrackPadClickHandlerImpl);
			mTrackPadScrollHandler =
				new TrackPadScrollHandler(TrackPadScrollHandlerImpl);
			mTrackPadZoomHandler =
				new TrackPadZoomHandler(TrackPadZoomHandlerImpl);
			mTouchHandler =
				new TouchHandler(TouchHandlerImpl);
			mKeyboardHandler =
				new KeyboardHandler(KeyboardHandlerImpl);
			mControllerHandler =
				new ControllerHandler(ControllerHandlerImpl);
			mAccelerometerHandler =
				new AccelerometerHandler(AccelerometerHandlerImpl);
			mSpecialHandler =
				new SpecialHandler(SpecialHandlerImpl);

			if (OpenSensorInit(
				mLoggerCallback, mBeaconPort, mBeaconInterval,
				mConnectHandler, IntPtr.Zero,
				mTrackPadMoveHandler, IntPtr.Zero,
				mTrackPadClickHandler, IntPtr.Zero,
				mTrackPadScrollHandler, IntPtr.Zero,
				mTrackPadZoomHandler, IntPtr.Zero,
				mTouchHandler, IntPtr.Zero,
				mKeyboardHandler, IntPtr.Zero,
				mControllerHandler, IntPtr.Zero,
				mAccelerometerHandler, IntPtr.Zero,
				mSpecialHandler, IntPtr.Zero,
				mDeviceType, GetDeviceIdentifier(),
				mVerbose ? 1 : 0) == -1)
				return;

			Thread inetThread = new Thread(OpenSensorRunServerInet);
			inetThread.IsBackground = true;
			inetThread.Start();

			Thread btThread = new Thread(OpenSensorRunServerBluetooth);
			btThread.IsBackground = true;
			btThread.Start();

			Thread beaconThread = new Thread(OpenSensorRunBeacon);
			beaconThread.IsBackground = true;
			beaconThread.Start();

			mInputMapper.SetModeHandler(delegate (String mode)
			{
				OpenSensorSetMode(mode);
			});
		}

		private void ConnectHandlerImpl(IntPtr context, int identity,
			int connected, int clientCount)
		{
			Logger.Info("OpenSensor client {0} {1}", identity,
				connected != 0 ? "connected" : "disconnected");
			Logger.Info("OpenSensor now has {0} clients", clientCount);

			if (connected != 0)
				mCursor.Attach(identity + IDENTITY_OFFSET);
			else
				mCursor.Detach(identity + IDENTITY_OFFSET);

			mConsole.HandleControllerAttach(connected != 0,
				identity + IDENTITY_OFFSET, "OpenSensor");
		}

		private void TrackPadMoveHandlerImpl(IntPtr context, int identity,
			float x, float y, int absolute)
		{
			mCursor.Move(identity + IDENTITY_OFFSET, x, y,
				absolute != 0);
		}

		private void TrackPadClickHandlerImpl(IntPtr context, int identity,
			int down)
		{
			mCursor.Click(identity + IDENTITY_OFFSET, down != 0);
		}

		private void TrackPadScrollHandlerImpl(IntPtr context, int identity,
			float dx, float dy, GestureEvent evt)
		{
			//Logger.Info("TrackPadScrollHandlerImpl {0} {1} {2}",
			//    dx, dy, evt);

			if (evt == GestureEvent.Begin)
			{

				float x = 0;
				float y = 0;

				mCursor.GetNormalizedPosition(
					identity + IDENTITY_OFFSET, out x, out y);

				mInputMapper.ScrollBegin(x, y);

			}
			else if (evt == GestureEvent.Update)
			{

				Size size = GetWindowSize();
				mInputMapper.ScrollUpdate(dx / size.Width,
					dy / size.Height);

			}
			else if (evt == GestureEvent.End)
			{

				mInputMapper.ScrollEnd();
			}
		}

		private void TrackPadZoomHandlerImpl(IntPtr context, int identity,
			float dz, GestureEvent evt)
		{
			//Logger.Info("TrackPadZoomHandlerImpl {0} {1}", dz, evt);

			if (evt == GestureEvent.Begin)
			{

				float x = 0;
				float y = 0;

				mCursor.GetNormalizedPosition(
					identity + IDENTITY_OFFSET, out x, out y);

				mInputMapper.ZoomBegin(x, y);

			}
			else if (evt == GestureEvent.Update)
			{

				mInputMapper.ZoomUpdate(dz);

			}
			else if (evt == GestureEvent.End)
			{

				mInputMapper.ZoomEnd();
			}
		}

		private void TouchHandlerImpl(IntPtr context, int identity,
			IntPtr list, int count)
		{
			mInputMapper.TouchHandlerImpl(list, count,
				identity * count);
		}

		private void KeyboardHandlerImpl(IntPtr context, int identity,
			char ch)
		{
			mInputMapper.DispatchCharacter(ch);
		}

		private void ControllerHandlerImpl(IntPtr context, int identity,
			int button, int down)
		{
			mInputMapper.DispatchControllerEvent(identity,
				(uint)button, down);
		}

		private void AccelerometerHandlerImpl(IntPtr context, int identity,
			float x, float y, float z)
		{
			/* XXXDPR:  Cannot do anything with identity. */
			mSensorDevice.SetAccelerometerVector(x, y, z);
		}

		private void SpecialHandlerImpl(IntPtr context, int identity,
			String cmd)
		{
			Logger.Info("OpenSensor.SpecialHandlerImpl -> " + cmd);

			if (cmd == "Back")
				mControlHandler.Back();
			else if (cmd == "Menu")
				mControlHandler.Menu();
			else if (cmd == "Home")
				mControlHandler.Home();
		}

		private Size GetWindowSize()
		{
			Size size = new Size();

			UIHelper.RunOnUIThread(mConsole, delegate ()
			{

				size = mConsole.Size;
			});

			return size;
		}

		private String GetDeviceIdentifier()
		{
			ManagementObjectCollection col;
			ManagementClass mgt;
			String deviceId;

			/*
			 * Return the stored device identifier if we have one.  This
			 * is much cheaper than querying the identifier via WMI.
			 */

			deviceId = GetStoredDeviceIdentifier();
			if (deviceId != null)
			{
				Logger.Info("Using stored OpenSensor device " +
					"identifier: " + deviceId);
				return deviceId;
			}

			try
			{
				mgt = new ManagementClass(
					"Win32_NetworkAdapterConfiguration");
				col = mgt.GetInstances();

				foreach (ManagementObject obj in col)
				{

					String name = (String)obj["Description"];
					String addr = (String)obj["MACAddress"];

					if (!name.Contains("Bluetooth"))
						continue;

					Logger.Info("Bluetooth device: " + name);

					if (addr == null)
						continue;

					Logger.Info("Bluetooth address: " + addr);

					deviceId = addr.ToUpper().Replace(":", "");
					Logger.Info("Using OpenSensor device " +
						"identifier: " + deviceId);

					SetStoredDeviceIdentifier(deviceId);
					return deviceId;
				}

			}
			catch (Exception exc)
			{
				Logger.Error(exc.ToString());
			}

			/*
			 * Didn't find a viable device identifier.  Return the user
			 * GUID instead.
			 */

			Logger.Info("Using default OpenSensor device identifier: " +
				User.GUID);

			SetStoredDeviceIdentifier(User.GUID);
			return User.GUID;
		}

		private String GetStoredDeviceIdentifier()
		{
			String deviceId = null;
			RegistryKey key;

			using (key = Registry.LocalMachine.OpenSubKey(CFG_KEY))
			{
				Object obj = key.GetValue(OPENSENSOR_DEVICEID_KEY);
				if (obj != null)
					deviceId = (String)obj;
			}

			return deviceId;
		}

		private void SetStoredDeviceIdentifier(String deviceId)
		{
			RegistryKey key;

			using (key = Registry.LocalMachine.OpenSubKey(CFG_KEY, true))
			{
				key.SetValue(OPENSENSOR_DEVICEID_KEY, deviceId);
			}
		}
	}

}
