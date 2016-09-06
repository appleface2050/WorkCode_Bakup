using Microsoft.Win32;
using System;
using System.Net;
using System.Net.Sockets;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using Microsoft.Win32.SafeHandles;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class Monitor
	{

		public delegate void SendMessage(IntPtr msg);
		public delegate void ReceiverCallback(IntPtr msg);

		SafeFileHandle mHandle;
		IntPtr mListener;
		UdpClient mUdpClient;
		ReceiverCallback mReceiverCallback;
		Thread mReceiverThread;
		EventWaitHandle mReceiverWakeup;

		public static Int32 lPort; //Listener port for server.
								   //Incoming data from client.
		public static string data = null;

		private Monitor(UdpClient c)
		{
			Logger.Warning("{0} Monitor constructor initing UDP socket",
					MethodBase.GetCurrentMethod().Name);
			mUdpClient = c;
			mListener = mUdpClient.Client.Handle;
		}

		private Monitor(SafeFileHandle handle)
		{
			mHandle = handle;
		}

		public static Monitor Connect(String vmName, uint cls)
		{
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				uint vmId = MonitorLocator.Lookup(vmName);
				if (vmId == 0)
					throw new ApplicationException("Cannot lookup VM");

				SafeFileHandle handle = Frontend.Modules.Module.ManagerOpenSafe();
				if (handle.IsInvalid)
					Interop.Common.ThrowLastWin32Error(
							"Cannot open manager");

				if (!Frontend.Modules.Module.ManagerAttachWithListener(handle, vmId, cls))
					Interop.Common.ThrowLastWin32Error(
							"Cannot attach to guest");

				return new Monitor(handle);
			}
			else
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				lPort = (int)key.GetValue("HostSensorPort");
				Logger.Warning("Host sensor port is {0}", lPort);
				key.Close();
				//Creates an instance of the UdpClient class using a local endpoint.
				IPEndPoint ipLocalEndPoint = new IPEndPoint(IPAddress.Parse("127.0.0.1"), lPort);
				UdpClient c = new UdpClient(ipLocalEndPoint);
				return new Monitor(c);
			}
		}

		public void Close()
		{
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				mHandle.Close();
			}
			else
			{
				mUdpClient.Close();
			}
		}

		public void Send(IntPtr msg)
		{
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				if (!Frontend.Modules.Module.MonitorSendMesg(mHandle, msg))
					Interop.Common.ThrowLastWin32Error(
							"Cannot send message to guest");
			}
			else
			{
				if (!Frontend.Modules.Module.SensorSendUdpMesg(mListener, msg, false))
					Interop.Common.ThrowLastWin32Error(
							"Cannot send message to guest");
			}
		}

		public void SendShutdown(IntPtr msg)
		{
			if (!Frontend.Modules.Module.SensorSendUdpMesg(mListener, msg, true))
				Interop.Common.ThrowLastWin32Error(
						"Cannot send message to guest");
		}

		public void StartReceiver(ReceiverCallback callback)
		{
			mReceiverCallback = callback;
			mReceiverWakeup = new ManualResetEvent(false);

			mReceiverThread = new Thread(delegate ()
			{

				try
				{
					if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
					{
						if (!Frontend.Modules.Module.MonitorRecvMesg(mHandle,
								mReceiverCallback,
								mReceiverWakeup.SafeWaitHandle))
							Interop.Common.ThrowLastWin32Error(
								"Cannot receive monitor message");
					}
					else
					{
						if (!Frontend.Modules.Module.SensorRecvUdpMesg(mListener, mReceiverCallback))
							Interop.Common.ThrowLastWin32Error(
								"Cannot receive monitor message");
					}
				}
				catch (Exception exc)
				{
					Logger.Error("Receiver thread died: " + exc);
				}
			});

			mReceiverThread.IsBackground = true;
			mReceiverThread.Start();
		}

		public void StopReceiver()
		{
			if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
			{
				mReceiverWakeup.Set();
			}
			else
			{
				Logger.Warning("{0} NOP for VBox",
						MethodBase.GetCurrentMethod().Name);
			}

		}
	}
}
