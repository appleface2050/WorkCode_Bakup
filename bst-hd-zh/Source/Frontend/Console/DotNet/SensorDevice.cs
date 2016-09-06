using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class SensorDevice
	{

		private const String NATIVE_DLL = "HD-Sensor-Native.dll";

		public enum Type
		{
			Accelerometer = 0,
		};

		private class State
		{
			public bool Enabled = false;
			public uint Period = 0;
			public bool HasPhysical = false;
			public int ControllerCount = 0;
		};

		private class AccelerometerState : State
		{
			public Object Lock = new Object();
			public float X = 0;
			public float Y = 0;
			public float Z = 0;
		}

		private delegate void LoggerCallback(String msg);
		private delegate void AccelerometerCallback(float x, float y,
			float z);

		private delegate void EnableHandler(Type sensor, bool enable);
		private delegate void SetDelayHandler(Type sensor, uint msec);

		[DllImport(NATIVE_DLL)]
		private static extern void LoggerSetCallback(LoggerCallback cb);

		/*
		 * Native DLL entry points for HD message routines
		 */

		[DllImport(NATIVE_DLL)]
		private static extern void MesgInit(EnableHandler enableHandler,
			SetDelayHandler setDelayHandler);

		[DllImport(NATIVE_DLL)]
		private static extern uint MesgGetDeviceClass();

		[DllImport(NATIVE_DLL)]
		private static extern void MesgHandleMessage(IntPtr msg);

		[DllImport(NATIVE_DLL)]
		private static extern void MesgSendReattach(Type sensor,
			Frontend.Monitor.SendMessage handler);

		[DllImport(NATIVE_DLL)]
		private static extern void MesgSendAccelerometerEvent(float x,
			float y, float z, Frontend.Monitor.SendMessage handler);

		[DllImport(NATIVE_DLL)]
		private static extern void MesgSendStopReceiver(Frontend.Monitor.SendMessage handler);
		/*
		 * Native DLL entry points for Windows sensor devices
		 */

		[DllImport(NATIVE_DLL)]
		private static extern bool HostInit();

		[DllImport(NATIVE_DLL)]
		private static extern void HostSetOrientation(
			int orientation);

		[DllImport(NATIVE_DLL)]
		private static extern bool HostSetupAccelerometer(
			AccelerometerCallback callback);

		[DllImport(NATIVE_DLL)]
		private static extern void HostEnableSensor(Type sensor,
			bool enable);

		[DllImport(NATIVE_DLL)]
		private static extern void HostSetSensorPeriod(Type sensor,
			uint msec);

		private Frontend.Monitor mMonitor;
		private LoggerCallback mLogger;

		private bool mRunning = false;

		private Dictionary<Type, State> mStateMap;
		private Thread mAccelerometerThread;

		private bool mEmulatedPortraitMode = false;
		private bool mRotateGuest180 = false;

		private AccelerometerCallback mAccelerometerCallback;   /* no GC */
		private EnableHandler mEnableHandler;       /* no GC */
		private SetDelayHandler mSetDelayHandler;   /* no GC */

		private SerialWorkQueue mSerialQueue;

		public SensorDevice()
		{
			mStateMap = new Dictionary<Type, State>();
			mStateMap[Type.Accelerometer] = new AccelerometerState();
		}

		public void StartThreads()
		{
			mAccelerometerThread = new Thread(AccelerometerThreadEntry);
			mAccelerometerThread.IsBackground = true;
			mAccelerometerThread.Start();
		}

		public void Start(String vmName)
		{
			mRunning = true;

			/*
			 * Wire up the loggers.
			 */

			mLogger = new LoggerCallback(delegate (String msg)
			{
				Logger.Info("SensorDevice: " + msg);
			});

			LoggerSetCallback(mLogger);

			/*
			 * Setup orientation handling.
			 */

			UpdateOrientation(null, null);
			SystemEvents.DisplaySettingsChanged += UpdateOrientation;

			/*
			 * Spin up our serialization queue.
			 */

			mSerialQueue = new SerialWorkQueue();
			mSerialQueue.Start();

			/*
			 * Configure our host sensors on the dedicated COM
			 * thread.
			 */

			EventWaitHandle evt = new ManualResetEvent(false);

			mSerialQueue.Enqueue(delegate ()
			{
				SetupHostSensors();
				evt.Set();
			});

			evt.WaitOne();

			/*
			 * Connect to the monitor and start receiving messages.
			 * Note that any handler that uses COM must be dispatched
			 * on the dedicated COM thread.
			 */

			mEnableHandler = new EnableHandler(EnableHandlerImpl);
			mSetDelayHandler = new SetDelayHandler(SetDelayHandlerImpl);

			MesgInit(mEnableHandler, mSetDelayHandler);

			mMonitor = Monitor.Connect(vmName, MesgGetDeviceClass());
			mMonitor.StartReceiver(MesgHandleMessage);

			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				//VT:???? Not sure if we need it.
				//MesgSendReattach(Type.Accelerometer, SendMessage);
			}
			else
				MesgSendReattach(Type.Accelerometer, SendMessage);
		}

		public void Stop()
		{
			mRunning = false;

			SystemEvents.DisplaySettingsChanged -= UpdateOrientation;
			if (hyperDroid.Common.Strings.IsEngineLegacy())
			{
			}
			else
			{
				try
				{
					MesgSendStopReceiver(SendShutdownMessage);
				}
				catch (Exception exc)
				{
					Logger.Error("Cannot send Stop receiver msg: " + exc);
				}
			}
			mMonitor.StopReceiver();

			mMonitor.Close();
			mMonitor = null;
		}

		public void SetDisplay(bool emulatedPortraitMode,
			bool rotateGuest180)
		{
			mEmulatedPortraitMode = emulatedPortraitMode;
			mRotateGuest180 = rotateGuest180;
		}

		private State LookupState(Type sensor)
		{
			if (mStateMap == null)
				return null;

			if (!mStateMap.ContainsKey(sensor))
				return null;

			return mStateMap[sensor];
		}

		private void SetupHostSensors()
		{
			if (!HostInit())
			{
				Logger.Warning("Cannot initialize host sensors");
				return;
			}

			Logger.Info("Setting up host accelerometer");

			mAccelerometerCallback = new AccelerometerCallback(
				SetAccelerometerVector);

			if (HostSetupAccelerometer(mAccelerometerCallback))
			{

				mStateMap[Type.Accelerometer].HasPhysical = true;

			}
			else
			{

				Logger.Warning("Cannot setup host accelerometer");
			}
		}

		public void ControllerAttach(Type sensor)
		{
			ControllerAttachDetach(sensor, true);
		}

		public void ControllerDetach(Type sensor)
		{
			ControllerAttachDetach(sensor, false);
		}

		private void ControllerAttachDetach(Type sensor, bool attach)
		{
			State state = LookupState(sensor);
			if (state == null)
				return;

			if (sensor != Type.Accelerometer)
			{
				Logger.Warning("Don't know how to do controller " +
					"override for sensor type " + sensor);
				return;
			}

			/*
			 * Manipulate the controller count on the sensor COM thread
			 * so it synchornizes properly with the enable handler.
			 */

			mSerialQueue.Enqueue(delegate ()
			{

				if (attach)
					state.ControllerCount++;
				else
					state.ControllerCount--;

				Logger.Info("Sensor device sees {0} controllers",
					state.ControllerCount);

				if (state.ControllerCount < 0)
				{
					Logger.Error("Bad sensor device " +
						"controller count");
					return;
				}

				if (attach && state.ControllerCount == 1)
				{

					Logger.Info("Switching from host " +
						"accelerometer to controller " +
						"accelerometer");

					if (state.HasPhysical)
						HostEnableSensor(sensor, false);

				}
				else if (!attach && state.ControllerCount == 0)
				{

					Logger.Info("Switching from controller " +
						"accelerometer to host accelerometer");

					if (state.HasPhysical)
						HostEnableSensor(sensor, true);
				}
			});
		}

		public void SetAccelerometerVector(float origX, float origY,
			float origZ)
		{
			AccelerometerState state =
				(AccelerometerState)LookupState(Type.Accelerometer);
			if (state == null)
				return;

			float x;
			float y;

			if (!mEmulatedPortraitMode)
			{
				x = -origX;
				y = -origY;
			}
			else
			{
				x = -origY;
				y = origX;
			}

			if (mRotateGuest180)
			{
				x = -x;
				y = -y;
			}

			lock (state.Lock)
			{
				state.X = x;
				state.Y = y;
				state.Z = origZ;
			}
		}

		private void AccelerometerThreadEntry()
		{
			AccelerometerState state =
				(AccelerometerState)LookupState(Type.Accelerometer);
			long begin;
			long end;
			long wait;
			int msec;
			float x, y, z;

			Logger.Info("Starting accelerometer sensor thread");

			while (true)
			{

				begin = DateTime.Now.Ticks;

				if (mRunning && state != null &&
					state.Enabled && state.Period != 0)
				{

					lock (state.Lock)
					{
						x = state.X;
						y = state.Y;
						z = state.Z;
					}

					SendAccelerometerVector(x, y, z);
					msec = (int)state.Period;

				}
				else
				{

					msec = 200;
				}

				end = DateTime.Now.Ticks;
				wait = msec * 10000 - end + begin;

				if (wait > 0)
					Thread.Sleep((int)(wait / 10000));
			}
		}

		private void SendAccelerometerVector(float x, float y, float z)
		{
			try
			{
				MesgSendAccelerometerEvent(x, y, z, SendMessage);
			}
			catch (Exception exc)
			{
				Logger.Error("Cannot send accelerometer event: " +
					exc);
			}
		}

		private void EnableHandlerImpl(Type sensor, bool enable)
		{
			Logger.Info("SensorDevice.EnableHandlerImpl({0}, {1})",
				sensor, enable);

			State state = LookupState(sensor);
			if (state == null)
			{
				Logger.Error("Enable/disable for invalid sensor " +
					sensor);
				return;
			}

			state.Enabled = enable;

			/*
			 * Enable the host sensor only if it is physically present
			 * and we aren't supposed to favor an emulated sensor,
			 * which injects sensor events itself.
			 *
			 * Perform the controller count check on the sensor COM
			 * thread so it synchronizes properly with the controller
			 * attach/detach handler.
			 */

			mSerialQueue.Enqueue(delegate ()
			{
				if (state.HasPhysical && state.ControllerCount == 0)
					HostEnableSensor(sensor, enable);
			});
		}

		private void SetDelayHandlerImpl(Type sensor, uint msec)
		{
			Logger.Info("SensorDevice.SetDelayHandlerImpl({0}, {1})",
				sensor, msec);

			State state = LookupState(sensor);
			if (state == null)
			{
				Logger.Error("Set delay for invalid sensor " +
					sensor);
				return;
			}

			state.Period = msec;

			mSerialQueue.Enqueue(delegate ()
			{
				HostSetSensorPeriod(sensor, msec);
			});
		}

		private void SendMessage(IntPtr msg)
		{
			mMonitor.Send(msg);
		}
		private void SendShutdownMessage(IntPtr msg)
		{
			mMonitor.SendShutdown(msg);
		}

		private void UpdateOrientation(Object obj, EventArgs evt)
		{
			ScreenOrientation so = SystemInformation.ScreenOrientation;

			switch (so)
			{
				case ScreenOrientation.Angle0:
					HostSetOrientation(0);
					break;

				case ScreenOrientation.Angle90:
					HostSetOrientation(1);
					break;

				case ScreenOrientation.Angle180:
					HostSetOrientation(2);
					break;

				case ScreenOrientation.Angle270:
					HostSetOrientation(3);
					break;
			}
		}
	}

}
