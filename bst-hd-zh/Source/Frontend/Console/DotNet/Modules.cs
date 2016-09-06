using Microsoft.Win32.SafeHandles;
using System;
using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Frontend.Interop;
using BlueStacks.hyperDroid.Gps;
using BlueStacks.hyperDroid.VideoCapture;
using Microsoft.Samples.TabletPC.MTScratchpad.WMTouch;

namespace BlueStacks.hyperDroid.Frontend
{
	public class Modules
	{
		static IModule module = null;

		public static IModule Module
		{
			get
			{
				if (module == null)
				{
					IntializeModule();
				}
				return module;
			}
		}

		private static void IntializeModule()
		{
			if (Common.Strings.IsEngineLegacy())
			{
				module = new HDModule();
			}
			else
			{
				module = new HDPlusModule();
			}
		}
	}
	public interface IModule
	{
		string CameraDllName();

		string FrontendDllName();

		IntPtr ManagerOpen();

		SafeFileHandle ManagerOpenSafe();

		void SetStartStopCamerCB(VideoCapture.Manager.fpStartStopCamera func);

		int CameraIoProcessMessages(IntPtr ioHandle);

		bool CameraSendCaptureStream(IntPtr handle, IntPtr stream, int size, int width, int height, int stride);

		bool MonitorSendCaptureStream(IntPtr handle, IntPtr stream, int size, IntPtr over, int width, int height, int stride);

		IntPtr CameraIoAttach(uint vmId);

		IntPtr MonitorCreateOverWrite();

		bool convertRGB24toYUV422(IntPtr src, int w, int h, IntPtr dst);

		bool ManagerAttachWithListener(SafeFileHandle handle, uint id, uint cls);

		bool MonitorSendMesg(SafeFileHandle handle, IntPtr msg);

		bool MonitorRecvMesg(SafeFileHandle handle, Monitor.ReceiverCallback callback, SafeWaitHandle wakeupEvent);

		bool SensorSendUdpMesg(IntPtr handle, IntPtr msg, bool loopBack);

		bool SensorRecvUdpMesg(IntPtr handle, Monitor.ReceiverCallback callback);

		int ManagerList(IntPtr handle, uint[] list, int count);

		bool ManagerAttach(IntPtr handle, uint id);

		bool ManagerIsVmxActive();

		void MonitorSetLogger(Interop.Monitor.LoggerCallback callback);

		void CameraSetLogger(Interop.Monitor.LoggerCallback callback);

		SafeFileHandle MonitorAttach(uint id, bool verbose);

		IntPtr MonitorVideoAttach(IntPtr handle);

		IntPtr MonitorVideoAttach(uint id, bool verbose);

		bool MonitorVideoDetach(IntPtr addr);

		bool MonitorSendScanCode(IntPtr handle, byte code);

		bool MonitorSendScanCode(SafeFileHandle handle, byte code);

		bool MonitorSendMouseState(IntPtr handle, uint x, uint y, uint mask);

		bool MonitorSendMouseState(SafeFileHandle handle, uint x, uint y, uint mask);

		bool MonitorSendTouchState(SafeFileHandle handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick);

		bool MonitorSendTouchState(IntPtr handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick);

		bool MonitorSendControl(SafeFileHandle handle, Interop.Monitor.BstInputControlType type);

		bool MonitorSendLocation(SafeFileHandle handle, [MarshalAs(UnmanagedType.Struct)]Gps.Manager.GpsLocation location);

		bool MonitorSendCaptureStream(IntPtr handle, IntPtr streamBuf, int size);

		bool VideoCheckMagic(IntPtr addr, ref uint magic);

		void VideoGetMode(IntPtr addr, ref uint width, ref uint height, ref uint depth);

		bool VideoGetAndClearDirty(IntPtr addr);

		bool SetMouseHWheelCallback(Interop.MouseHWheel.MouseHWheelCallback func);
	}

	public class HDPlusModule : IModule
	{
		public const string CAMERA_DLL = "HD-Plus-Camera-Native.dll";
		public const string FRONTEND_DLL = "HD-Plus-Frontend-Native.dll";

		string IModule.CameraDllName()
		{
			return CAMERA_DLL;
		}
		string IModule.FrontendDllName()
		{
			return FRONTEND_DLL;
		}

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		private static extern IntPtr ManagerOpen();

		[DllImport(CAMERA_DLL)]
		public static extern void SetStartStopCamerCB(VideoCapture.Manager.fpStartStopCamera func);

		[DllImport(CAMERA_DLL)]
		public static extern int CameraIoProcessMessages(IntPtr ioHandle);

		[DllImport(CAMERA_DLL)]
		public static extern bool CameraSendCaptureStream(IntPtr handle, IntPtr stream, int size, int width, int height, int stride);

		[DllImport(CAMERA_DLL)]
		public static extern bool MonitorSendCaptureStream(IntPtr handle, IntPtr stream, int size, IntPtr over, int width, int height, int stride);

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern IntPtr CameraIoAttach(uint vmId);

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern IntPtr MonitorCreateOverWrite();

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern bool convertRGB24toYUV422(IntPtr src, int w, int h, IntPtr dst);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool ManagerAttachWithListener(SafeFileHandle handle, uint id, uint cls);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendMesg(SafeFileHandle handle, IntPtr msg);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorRecvMesg(SafeFileHandle handle, Monitor.ReceiverCallback callback, SafeWaitHandle wakeupEvent);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool SensorSendUdpMesg(IntPtr handle, IntPtr msg, bool loopBack);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool SensorRecvUdpMesg(IntPtr handle, Monitor.ReceiverCallback callback);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern int ManagerList(IntPtr handle, uint[] list, int count);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool ManagerAttach(IntPtr handle, uint id);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool ManagerIsVmxActive();

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern void MonitorSetLogger(Interop.Monitor.LoggerCallback callback);

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern void CameraSetLogger(Interop.Monitor.LoggerCallback callback);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern SafeFileHandle MonitorAttach(uint id, bool verbose);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern IntPtr MonitorVideoAttach(IntPtr handle);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern IntPtr MonitorVideoAttach(uint id, bool verbose);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorVideoDetach(IntPtr addr);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendScanCode(SafeFileHandle handle, byte code);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendScanCode(IntPtr handle, byte code);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendMouseState(IntPtr handle, uint x, uint y, uint mask);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendMouseState(SafeFileHandle handle, uint x, uint y, uint mask);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendTouchState(SafeFileHandle handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendTouchState(IntPtr handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendControl(SafeFileHandle handle, Interop.Monitor.BstInputControlType type);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendLocation(SafeFileHandle handle, [MarshalAs(UnmanagedType.Struct)]Gps.Manager.GpsLocation location);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendCaptureStream(IntPtr handle, IntPtr streamBuf, int size);

		[DllImport(FRONTEND_DLL)]
		public static extern bool VideoCheckMagic(IntPtr addr, ref uint magic);

		[DllImport(FRONTEND_DLL)]
		public static extern void VideoGetMode(IntPtr addr, ref uint width, ref uint height, ref uint depth);

		[DllImport(FRONTEND_DLL)]
		public static extern bool VideoGetAndClearDirty(IntPtr addr);

		[DllImport(FRONTEND_DLL)]
		public static extern bool SetMouseHWheelCallback(Interop.MouseHWheel.MouseHWheelCallback func);

		IntPtr IModule.ManagerOpen()
		{
			return ManagerOpen();
		}

		SafeFileHandle IModule.ManagerOpenSafe()
		{
			return new SafeFileHandle(ManagerOpen(), true);
		}

		IntPtr IModule.CameraIoAttach(uint vmId)
		{
			return CameraIoAttach(vmId);
		}

		int IModule.CameraIoProcessMessages(IntPtr ioHandle)
		{
			return CameraIoProcessMessages(ioHandle);
		}

		bool IModule.CameraSendCaptureStream(IntPtr handle, IntPtr stream, int size, int width, int height, int stride)
		{
			return CameraSendCaptureStream(handle, stream, size, width, height, stride);
		}

		void IModule.CameraSetLogger(Interop.Monitor.LoggerCallback callback)
		{
			CameraSetLogger(callback);
		}

		bool IModule.convertRGB24toYUV422(IntPtr src, int w, int h, IntPtr dst)
		{
			return convertRGB24toYUV422(src, w, h, dst);
		}

		bool IModule.ManagerAttach(IntPtr handle, uint id)
		{
			return ManagerAttach(handle, id);
		}

		bool IModule.ManagerAttachWithListener(SafeFileHandle handle, uint id, uint cls)
		{
			return ManagerAttachWithListener(handle, id, cls);
		}

		bool IModule.ManagerIsVmxActive()
		{
			return ManagerIsVmxActive();
		}

		int IModule.ManagerList(IntPtr handle, uint[] list, int count)
		{
			return ManagerList(handle, list, count);
		}

		SafeFileHandle IModule.MonitorAttach(uint id, bool verbose)
		{
			return MonitorAttach(id, verbose);
		}

		IntPtr IModule.MonitorCreateOverWrite()
		{
			return MonitorCreateOverWrite();
		}

		bool IModule.MonitorRecvMesg(SafeFileHandle handle, Monitor.ReceiverCallback callback, SafeWaitHandle wakeupEvent)
		{
			return MonitorRecvMesg(handle, callback, wakeupEvent);
		}

		bool IModule.MonitorSendCaptureStream(IntPtr handle, IntPtr streamBuf, int size)
		{
			return MonitorSendCaptureStream(handle, streamBuf, size);
		}

		bool IModule.MonitorSendCaptureStream(IntPtr handle, IntPtr stream, int size, IntPtr over, int width, int height, int stride)
		{
			return MonitorSendCaptureStream(handle, stream, size, over, width, height, stride);
		}

		bool IModule.MonitorSendControl(SafeFileHandle handle, Interop.Monitor.BstInputControlType type)
		{
			return MonitorSendControl(handle, type);
		}

		bool IModule.MonitorSendLocation(SafeFileHandle handle, Gps.Manager.GpsLocation location)
		{
			return MonitorSendLocation(handle, location);
		}

		bool IModule.MonitorSendMesg(SafeFileHandle handle, IntPtr msg)
		{
			return MonitorSendMesg(handle, msg);
		}

		bool IModule.MonitorSendMouseState(IntPtr handle, uint x, uint y, uint mask)
		{
			return MonitorSendMouseState(handle, x, y, mask);
		}

		bool IModule.MonitorSendScanCode(SafeFileHandle handle, byte code)
		{
			return MonitorSendScanCode(handle, code);
		}

		bool IModule.MonitorSendTouchState(SafeFileHandle handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick)
		{
			return MonitorSendTouchState(handle, points, count, attackClick);
		}

		bool IModule.MonitorSendTouchState(IntPtr handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick)
		{
			return MonitorSendTouchState(handle, points, count, attackClick);
		}

		void IModule.MonitorSetLogger(Interop.Monitor.LoggerCallback callback)
		{
			MonitorSetLogger(callback);
		}

		IntPtr IModule.MonitorVideoAttach(IntPtr handle)
		{
			return MonitorVideoAttach(handle);
		}

		IntPtr IModule.MonitorVideoAttach(uint id, bool verbose)
		{
			return MonitorVideoAttach(id, verbose);
		}

		bool IModule.MonitorVideoDetach(IntPtr addr)
		{
			return MonitorVideoDetach(addr);
		}

		bool IModule.SensorRecvUdpMesg(IntPtr handle, Monitor.ReceiverCallback callback)
		{
			return SensorRecvUdpMesg(handle, callback);
		}

		bool IModule.SensorSendUdpMesg(IntPtr handle, IntPtr msg, bool loopBack)
		{
			return SensorSendUdpMesg(handle, msg, loopBack);
		}

		bool IModule.SetMouseHWheelCallback(MouseHWheel.MouseHWheelCallback func)
		{
			return SetMouseHWheelCallback(func);
		}

		void IModule.SetStartStopCamerCB(VideoCapture.Manager.fpStartStopCamera func)
		{
			SetStartStopCamerCB(func);
		}

		bool IModule.VideoCheckMagic(IntPtr addr, ref uint magic)
		{
			return VideoCheckMagic(addr, ref magic);
		}

		bool IModule.VideoGetAndClearDirty(IntPtr addr)
		{
			return VideoGetAndClearDirty(addr);
		}

		void IModule.VideoGetMode(IntPtr addr, ref uint width, ref uint height, ref uint depth)
		{
			VideoGetMode(addr, ref width, ref height, ref depth);
		}

		bool IModule.MonitorSendScanCode(IntPtr handle, byte code)
		{
			return MonitorSendScanCode(handle, code);
		}

		bool IModule.MonitorSendMouseState(SafeFileHandle handle, uint x, uint y, uint mask)
		{
			return MonitorSendMouseState(handle, x, y, mask);
		}
	}

	public class HDModule : IModule
	{
		public const string CAMERA_DLL = "HD-Camera-Native.dll";
		public const string FRONTEND_DLL = "HD-Frontend-Native.dll";

		string IModule.CameraDllName()
		{
			return CAMERA_DLL;
		}
		string IModule.FrontendDllName()
		{
			return FRONTEND_DLL;
		}

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		private static extern IntPtr ManagerOpen();

		[DllImport(CAMERA_DLL)]
		public static extern void SetStartStopCamerCB(VideoCapture.Manager.fpStartStopCamera func);

		[DllImport(CAMERA_DLL)]
		public static extern int CameraIoProcessMessages(IntPtr ioHandle);

		[DllImport(CAMERA_DLL)]
		public static extern bool CameraSendCaptureStream(IntPtr handle, IntPtr stream, int size, int width, int height, int stride);

		[DllImport(CAMERA_DLL)]
		public static extern bool MonitorSendCaptureStream(IntPtr handle, IntPtr stream, int size, IntPtr over, int width, int height, int stride);

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern IntPtr CameraIoAttach(uint vmId);

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern IntPtr MonitorCreateOverWrite();

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern bool convertRGB24toYUV422(IntPtr src, int w, int h, IntPtr dst);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool ManagerAttachWithListener(SafeFileHandle handle, uint id, uint cls);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendMesg(SafeFileHandle handle, IntPtr msg);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorRecvMesg(SafeFileHandle handle, Monitor.ReceiverCallback callback, SafeWaitHandle wakeupEvent);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool SensorSendUdpMesg(IntPtr handle, IntPtr msg, bool loopBack);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool SensorRecvUdpMesg(IntPtr handle, Monitor.ReceiverCallback callback);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern int ManagerList(IntPtr handle, uint[] list, int count);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool ManagerAttach(IntPtr handle, uint id);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool ManagerIsVmxActive();

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern void MonitorSetLogger(Interop.Monitor.LoggerCallback callback);

		[DllImport(CAMERA_DLL, SetLastError = true)]
		public static extern void CameraSetLogger(Interop.Monitor.LoggerCallback callback);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern SafeFileHandle MonitorAttach(uint id, bool verbose);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern IntPtr MonitorVideoAttach(IntPtr handle);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern IntPtr MonitorVideoAttach(uint id, bool verbose);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorVideoDetach(IntPtr addr);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendScanCode(SafeFileHandle handle, byte code);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendScanCode(IntPtr handle, byte code);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendMouseState(IntPtr handle, uint x, uint y, uint mask);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendMouseState(SafeFileHandle handle, uint x, uint y, uint mask);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendTouchState(SafeFileHandle handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendTouchState(IntPtr handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendControl(SafeFileHandle handle, Interop.Monitor.BstInputControlType type);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendLocation(SafeFileHandle handle, [MarshalAs(UnmanagedType.Struct)]Gps.Manager.GpsLocation location);

		[DllImport(FRONTEND_DLL, SetLastError = true)]
		public static extern bool MonitorSendCaptureStream(IntPtr handle, IntPtr streamBuf, int size);

		[DllImport(FRONTEND_DLL)]
		public static extern bool VideoCheckMagic(IntPtr addr, ref uint magic);

		[DllImport(FRONTEND_DLL)]
		public static extern void VideoGetMode(IntPtr addr, ref uint width, ref uint height, ref uint depth);

		[DllImport(FRONTEND_DLL)]
		public static extern bool VideoGetAndClearDirty(IntPtr addr);

		[DllImport(FRONTEND_DLL)]
		public static extern bool SetMouseHWheelCallback(Interop.MouseHWheel.MouseHWheelCallback func);

		IntPtr IModule.ManagerOpen()
		{
			return ManagerOpen();
		}
		SafeFileHandle IModule.ManagerOpenSafe()
		{
			return new SafeFileHandle(ManagerOpen(), true);
		}
		IntPtr IModule.CameraIoAttach(uint vmId)
		{
			return CameraIoAttach(vmId);
		}

		int IModule.CameraIoProcessMessages(IntPtr ioHandle)
		{
			return CameraIoProcessMessages(ioHandle);
		}

		bool IModule.CameraSendCaptureStream(IntPtr handle, IntPtr stream, int size, int width, int height, int stride)
		{
			return CameraSendCaptureStream(handle, stream, size, width, height, stride);
		}

		void IModule.CameraSetLogger(Interop.Monitor.LoggerCallback callback)
		{
			CameraSetLogger(callback);
		}

		bool IModule.convertRGB24toYUV422(IntPtr src, int w, int h, IntPtr dst)
		{
			return convertRGB24toYUV422(src, w, h, dst);
		}

		bool IModule.ManagerAttach(IntPtr handle, uint id)
		{
			return ManagerAttach(handle, id);
		}

		bool IModule.ManagerAttachWithListener(SafeFileHandle handle, uint id, uint cls)
		{
			return ManagerAttachWithListener(handle, id, cls);
		}

		bool IModule.ManagerIsVmxActive()
		{
			return ManagerIsVmxActive();
		}

		int IModule.ManagerList(IntPtr handle, uint[] list, int count)
		{
			return ManagerList(handle, list, count);
		}

		SafeFileHandle IModule.MonitorAttach(uint id, bool verbose)
		{
			return MonitorAttach(id, verbose);
		}

		IntPtr IModule.MonitorCreateOverWrite()
		{
			return MonitorCreateOverWrite();
		}

		bool IModule.MonitorRecvMesg(SafeFileHandle handle, Monitor.ReceiverCallback callback, SafeWaitHandle wakeupEvent)
		{
			return MonitorRecvMesg(handle, callback, wakeupEvent);
		}

		bool IModule.MonitorSendCaptureStream(IntPtr handle, IntPtr streamBuf, int size)
		{
			return MonitorSendCaptureStream(handle, streamBuf, size);
		}

		bool IModule.MonitorSendCaptureStream(IntPtr handle, IntPtr stream, int size, IntPtr over, int width, int height, int stride)
		{
			return MonitorSendCaptureStream(handle, stream, size, over, width, height, stride);
		}

		bool IModule.MonitorSendControl(SafeFileHandle handle, Interop.Monitor.BstInputControlType type)
		{
			return MonitorSendControl(handle, type);
		}

		bool IModule.MonitorSendLocation(SafeFileHandle handle, Gps.Manager.GpsLocation location)
		{
			return MonitorSendLocation(handle, location);
		}

		bool IModule.MonitorSendMesg(SafeFileHandle handle, IntPtr msg)
		{
			return MonitorSendMesg(handle, msg);
		}

		bool IModule.MonitorSendMouseState(IntPtr handle, uint x, uint y, uint mask)
		{
			return MonitorSendMouseState(handle, x, y, mask);
		}

		bool IModule.MonitorSendScanCode(SafeFileHandle handle, byte code)
		{
			return MonitorSendScanCode(handle, code);
		}

		bool IModule.MonitorSendTouchState(SafeFileHandle handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick)
		{
			return MonitorSendTouchState(handle, points, count, attackClick);
		}

		bool IModule.MonitorSendTouchState(IntPtr handle, Interop.Monitor.TouchPoint[] points, int count, bool attackClick)
		{
			return MonitorSendTouchState(handle, points, count, attackClick);
		}

		void IModule.MonitorSetLogger(Interop.Monitor.LoggerCallback callback)
		{
			MonitorSetLogger(callback);
		}

		IntPtr IModule.MonitorVideoAttach(IntPtr handle)
		{
			return MonitorVideoAttach(handle);
		}

		IntPtr IModule.MonitorVideoAttach(uint id, bool verbose)
		{
			return MonitorVideoAttach(id, verbose);
		}

		bool IModule.MonitorVideoDetach(IntPtr addr)
		{
			return MonitorVideoDetach(addr);
		}

		bool IModule.SensorRecvUdpMesg(IntPtr handle, Monitor.ReceiverCallback callback)
		{
			return SensorRecvUdpMesg(handle, callback);
		}

		bool IModule.SensorSendUdpMesg(IntPtr handle, IntPtr msg, bool loopBack)
		{
			return SensorSendUdpMesg(handle, msg, loopBack);
		}

		bool IModule.SetMouseHWheelCallback(MouseHWheel.MouseHWheelCallback func)
		{
			return SetMouseHWheelCallback(func);
		}

		void IModule.SetStartStopCamerCB(VideoCapture.Manager.fpStartStopCamera func)
		{
			SetStartStopCamerCB(func);
		}

		bool IModule.VideoCheckMagic(IntPtr addr, ref uint magic)
		{
			return VideoCheckMagic(addr, ref magic);
		}

		bool IModule.VideoGetAndClearDirty(IntPtr addr)
		{
			return VideoGetAndClearDirty(addr);
		}

		void IModule.VideoGetMode(IntPtr addr, ref uint width, ref uint height, ref uint depth)
		{
			VideoGetMode(addr, ref width, ref height, ref depth);
		}
		bool IModule.MonitorSendScanCode(IntPtr handle, byte code)
		{
			return MonitorSendScanCode(handle, code);
		}

		bool IModule.MonitorSendMouseState(SafeFileHandle handle, uint x, uint y, uint mask)
		{
			return MonitorSendMouseState(handle, x, y, mask);
		}
	}

}
