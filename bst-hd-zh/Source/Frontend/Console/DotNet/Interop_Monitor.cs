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

//#define VERBOSE_LOGGING

using Microsoft.Win32.SafeHandles;
using System;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{
			namespace Interop
			{

				public class Monitor
				{

					public delegate void LoggerCallback(String msg);

					public delegate void ExitHandler();

					private delegate void ReadCallback();

					[StructLayout(LayoutKind.Sequential, Pack = 1)]
					public struct TouchPoint
					{
						public int PosX;
						public int PosY;

						public TouchPoint(int x, int y)
						{
							this.PosX = x;
							this.PosY = y;
						}
					}

					public enum BstInputControlType
					{
						BST_INPUT_CONTROL_TYPE_NONE = 0,
						BST_INPUT_CONTROL_TYPE_SHUTDOWN,
						BST_INPUT_CONTROL_TYPE_STOP,
						BST_INPUT_CONTROL_TYPE_START,
					};

					private static LoggerCallback sLoggerCallback;
					private static LoggerCallback camLoggerCallback;
					private SafeFileHandle mHandle;
					private uint mId;

					private IntPtr handle;
					private uint id;

					public Monitor(IntPtr handle, uint id, ExitHandler exitHandler)
					{
						this.handle = handle;
						this.id = id;

						Thread thread = new Thread(delegate ()
						{
							while (Utils.IsProcessAlive(Convert.ToInt32(id)))
								Thread.Sleep(1000);

							exitHandler();
						});

						thread.IsBackground = true;
						thread.Start();
					}

					static Monitor()
					{
						/*
						 * Prevent garbage collection by holding a static
						 * reference.
						 */

						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
						}
						else
						{
							sLoggerCallback = delegate (String msg)
							{
								Logger.Info("Monitor: " + msg);
							};

							Frontend.Modules.Module.MonitorSetLogger(sLoggerCallback);
							camLoggerCallback = delegate (String msg)
							{
								Logger.Info("Camera: " + msg);
							};
							Frontend.Modules.Module.CameraSetLogger(camLoggerCallback);
						}
					}

					public Monitor(uint id, bool verbose)
					{
						mId = id;
						mHandle = Attach(verbose);
					}

					public Monitor(uint id)
					{
						mId = id;
					}

					public void Close()
					{
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
						}
						else
						{
							mHandle.Close();
						}
					}

					private SafeFileHandle Attach(bool verbose)
					{
						SafeFileHandle handle = Frontend.Modules.Module.MonitorAttach(mId, verbose);
						if (handle.IsInvalid)
							Common.ThrowLastWin32Error(String.Format("FATAL ERROR: Cannot attach to monitor: {0}",
										Marshal.GetLastWin32Error()));

						return handle;
					}

					public Video VideoAttach(bool verbose)
					{
						IntPtr addr = IntPtr.Zero;

						addr = Frontend.Modules.Module.MonitorVideoAttach(mId, verbose);
						if (addr == IntPtr.Zero)
							Common.ThrowLastWin32Error(String.Format("FATAL ERROR: Cannot attach to monitor video: {0}",
										Marshal.GetLastWin32Error()));

						//Logger.Info("Video memory at 0x{0:X8}", addr.ToString("x"));

						Video video = new Video(addr);

						try
						{
							video.CheckMagic();

						}
						catch (Exception exc)
						{
							Frontend.Modules.Module.MonitorVideoDetach(addr);
							throw exc;
						}
						Logger.Info("Video Attached");

						return video;
					}

					public Video VideoAttach()
					{
						int retries = 5;
						IntPtr addr = IntPtr.Zero;

						while (retries > 0)
						{
							addr = Frontend.Modules.Module.MonitorVideoAttach(this.handle);
							if (addr == IntPtr.Zero)
							{
								Logger.Error(String.Format("FATAL ERROR: Cannot attach to monitor video. err: {0}", Marshal.GetLastWin32Error()));
								Utils.KillAnotherFrontendInstance();
								Thread.Sleep(1000);
								retries--;
								continue;
							}
							else
							{
								break;
							}
						}

						if (retries == 0)
						{
							Common.ThrowLastWin32Error(String.Format("FATAL ERROR: Cannot attach to monitor video. err: {0}",
										Marshal.GetLastWin32Error()));
						}

						Logger.Info("Video memory at 0x{0:X8}", addr.ToString("x"));

						Video video = new Video(addr);
						video.CheckMagic();

						return video;
					}

					public void SendScanCode(byte code)
					{
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							if (!Frontend.Modules.Module.MonitorSendScanCode(this.handle, code))
								Common.ThrowLastWin32Error(
								"Cannot send keyboard scan code");
						}
						else
						{
							if (!Frontend.Modules.Module.MonitorSendScanCode(mHandle, code))
								Common.ThrowLastWin32Error(
								"Cannot send keyboard scan code");
						}
					}

					public void SendLocation(Gps.Manager.GpsLocation location)
					{
						if (!Frontend.Modules.Module.MonitorSendLocation(mHandle, location))
							Common.ThrowLastWin32Error(
									"Cannot send GPS location update");
					}

					public void SendMouseState(uint x, uint y, uint mask)
					{
						//Logger.Info("SendMouseState(0x{0}, 0x{1}, 0x{2})",
						//    x.ToString("x"),
						//    y.ToString("x"),
						//    mask.ToString("x"));

						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							if (!Frontend.Modules.Module.MonitorSendMouseState(this.handle, x, y, mask))
								Common.ThrowLastWin32Error("Cannot send mouse state");
						}
						else
						{
							if (!Frontend.Modules.Module.MonitorSendMouseState(mHandle, x, y, mask))
								Common.ThrowLastWin32Error("Cannot send mouse state");
						}
					}

					public void SendControl(BstInputControlType type)
					{
						if (!Frontend.Modules.Module.MonitorSendControl(mHandle, type))
							Common.ThrowLastWin32Error("Cannot send control state state");
					}

					public void SendTouchState(TouchPoint[] points)
					{
						if (points == null)
							points = new TouchPoint[0];

#if VERBOSE_LOGGING

		Logger.Info("SendTouchState");

		for (int ndx = 0; ndx < points.Length; ndx++) {

			TouchPoint point = points[ndx];

			if (point.PosX == 0xffff || point.PosY == 0xffff)
				continue;

			Logger.Info("    SLOT = {0}, X = {1}, Y = {2}",
			    ndx, point.PosX, point.PosY);
		}

#endif   // VERBOSE_LOGGING

						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							if (!Frontend.Modules.Module.MonitorSendTouchState(this.handle, points,
							points.Length, Console.mShootAttackClick && Console.mDisableMouseMovement))
								Common.ThrowLastWin32Error("Cannot send touch state");
						}
						else
						{
							if (!Frontend.Modules.Module.MonitorSendTouchState(mHandle, points,
								points.Length, Console.mShootAttackClick && Console.mDisableMouseMovement))
								Common.ThrowLastWin32Error("Cannot send touch state");
						}
						Console.mShootAttackClick = false;
					}

					public void SendAudioCaptureStream(byte[] streamBuf, int size)
					{
						Logger.Warning("{0}", MethodBase.GetCurrentMethod().Name);
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							IntPtr buf = Marshal.AllocHGlobal(size);

							try
							{
								Marshal.Copy(streamBuf, 0, buf, size);

								Frontend.Modules.Module.MonitorSendCaptureStream(this.handle, buf, size);
							}
							finally
							{
								Marshal.FreeHGlobal(buf);
							}
						}
					}
				}
			}
		}
	}
}
