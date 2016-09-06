/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * Camera support for BlueStacks hyperDroid Console Frontend
 */

using System;
using Microsoft.Win32;
using System.Windows.Forms;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using System.Drawing;
using System.Drawing.Imaging;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public class Manager
	{

		public unsafe delegate void fpStartStopCamera(Int32 startStop, Int32 unit, Int32 width, Int32 height, Int32 framerate);
		private fpStartStopCamera s_fpStartStopCamera;

		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr handle);

		static Monitor s_Monitor;
		static IntPtr s_IoHandle = IntPtr.Zero;
		static Object s_IoHandleLock = new Object();
		private IntPtr overWrite;

		private Camera camera;
		private Camera.getFrameCB cb;
		bool bShutDown = false;
		private int unit = 0;
		private int framerate = 30;
		private int width = 640;
		private int height = 480;
		private int jpegQuality = 100;
		private int keyEnableCam = 0;
		private bool cameraStoped = true;
		private SupportedColorFormat m_color = SupportedColorFormat.YUV2;
		private IntPtr m_buffer = IntPtr.Zero;
		private int m_StartCount = 0;


		private unsafe void BstStartStopCamera(Int32 startStop, Int32 unit, Int32 width, Int32 height, Int32 framerate)
		{
			lock (s_IoHandleLock)
			{

				if (this.unit != unit && startStop == 1)
				{
					camStop();
					m_StartCount = 0;
				}
				if (this.unit == unit && startStop == 0)
				{
					m_StartCount--;
					if (m_StartCount == 0)
						camStop();
				}
				if (startStop == 1)
				{
					m_StartCount++;
					camStart(unit, width, height, framerate);
					this.unit = unit;
				}
			}

		}

		public static Monitor Monitor
		{
			get
			{
				return s_Monitor;
			}

			set
			{
				s_Monitor = value;
			}
		}

		public void InitCamera(String[] args)
		{
			if (args.Length != 1)
				throw new SystemException("InitCamera: Should have vmName as one arg");

			String vmName = args[0];

			/*
			 * Check for registry key
			 */

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			try
			{
				this.keyEnableCam = (int)key.GetValue("Camera");
			}
			catch
			{
				keyEnableCam = 0;
			}

			if (keyEnableCam != 1)
			{
				Logger.Info("Camera is Disabled");
				return;
			}

			/*
			 * Look up the VM identifier using the VM name.
			 */

			uint vmId = MonitorLocator.Lookup(vmName);

			/*
			 * Attach to the VM.
			 */

			lock (s_IoHandleLock)
			{
				if (s_IoHandle != IntPtr.Zero)
					throw new SystemException("I/O handle is already open");

				Logger.Info("Attaching to monitor ID {0}", vmId);
				s_IoHandle = Frontend.Modules.Module.CameraIoAttach(vmId);
				if (s_IoHandle == IntPtr.Zero)
					throw new SystemException("Cannot attach for I/O",
					new Win32Exception(Marshal.GetLastWin32Error()));
			}

			if (hyperDroid.Common.Strings.IsEngineLegacy())
			{
				overWrite = Frontend.Modules.Module.MonitorCreateOverWrite();
				if (overWrite == IntPtr.Zero)
					throw new SystemException("Cannot create overlapped structure",
					new Win32Exception(Marshal.GetLastWin32Error()));
			}

			s_fpStartStopCamera = new fpStartStopCamera(BstStartStopCamera);

			/* set required callbacks */
			Frontend.Modules.Module.SetStartStopCamerCB(s_fpStartStopCamera);

			/*
			 * Loop, processing messages on each iteration.  Note
			 * that CameraIoProcessMessages() will block until it receives
			 * a message from the monitor.
			 */

			Logger.Info("Waiting for Camera messages...");
			System.Threading.Thread cameraThread = new
			System.Threading.Thread(delegate ()
			{
				while (true)
				{
					if (bShutDown)
						break;
					int error = Frontend.Modules.Module.CameraIoProcessMessages(s_IoHandle);
					if (error != 0)
					{
						Logger.Error("Camera: Cannot process VM messages. Error: " + error);
						Shutdown();
					}
				}
			});

			cameraThread.IsBackground = true;
			cameraThread.Start();
		}

		public void Shutdown()
		{
			lock (s_IoHandleLock)
			{
				if (keyEnableCam != 1)
					return;
				bShutDown = true;
				if (camera != null || cameraStoped == false)
				{
					camera.StopCamera();
				}
				camera = null;
				if (s_IoHandle != IntPtr.Zero)
				{
					CloseHandle(s_IoHandle);
					s_IoHandle = IntPtr.Zero;
				}
			}
		}

		public void getFrame(IntPtr ip, int width, int height, int stride)
		{
			if (ip == IntPtr.Zero || camera == null || s_IoHandle == IntPtr.Zero || cameraStoped == true)
			{
				return;
			}
			if (hyperDroid.Common.Strings.IsEngineLegacy() && overWrite == IntPtr.Zero)
			{
				return;
			}
			IntPtr tPtr = ip;
			if (m_color == SupportedColorFormat.RGB24)
			{
				if (m_buffer == IntPtr.Zero)
				{
					m_buffer = Marshal.AllocCoTaskMem(width * height * 2);
				}
				Frontend.Modules.Module.convertRGB24toYUV422(ip, width, height, m_buffer);
				tPtr = m_buffer;
			}
			//		Logger.Info("Got CamFrame...\n");
			if (hyperDroid.Common.Strings.IsEngineLegacy())
			{
				Frontend.Modules.Module.MonitorSendCaptureStream(s_IoHandle, tPtr, width * height * 2, overWrite, width, height, stride);
			}
			else
			{
				Frontend.Modules.Module.CameraSendCaptureStream(s_IoHandle, tPtr, width * height * 2, width, height, stride);
			}
		}

		public void camStart(Int32 unit, Int32 w, Int32 h, Int32 f)
		{
			if (camera != null || keyEnableCam != 1 || cameraStoped == false)
				return;
			if (w > 0)
				width = w;
			if (h > 0)
				height = h;
			if (f > 0)
				framerate = f;
			Logger.Info("Starting Camera {0}. Frame width: {1}, height: {2}, framerate: {3}", unit, width, height, framerate);
			cameraStoped = false;
			cb = new Camera.getFrameCB(getFrame);
			//try with all supported colors
			for (int i = 0; i < (int)SupportedColorFormat.LAST; i++)
			{
				if (camera != null)
					break;
				try
				{
					m_color = (SupportedColorFormat)i;
					camera = new Camera(unit, width, height, framerate, jpegQuality, m_color);
				}
				catch (ColorFormatNotSupported e)
				{
					Logger.Info("Trying with other color." + e.ToString());
				}
				catch (Exception e)
				{
					Logger.Error("Failed to initialize the camera", e.ToString());
				}
			}
			if (camera == null)
			{
				Logger.Error("Cannot start the host camera.");
				return;
			}
			camera.registerFrameCB(cb);
			camera.StartCamera();
		}

		public void camStop()
		{
			if (camera == null || cameraStoped == true)
			{
				return;
			}
			Logger.Info("Stoping Camera.");
			cameraStoped = true;
			camera.StopCamera();
			camera = null;
			if (m_buffer != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(m_buffer);
				m_buffer = IntPtr.Zero;
			}
		}

		public void resumeCamera()
		{
			lock (s_IoHandleLock)
			{
				if (keyEnableCam != 1 || cameraStoped == false || camera == null)
					return;
				Logger.Info("Resuming Camera");
				camStart(unit, width, height, framerate);
			}
		}

		public void pauseCamera()
		{
			lock (s_IoHandleLock)
			{
				if (keyEnableCam != 1 || camera == null || cameraStoped == true)
					return;
				Logger.Info("Pausing Camera");
				camStop();
			}
		}

	}
}
