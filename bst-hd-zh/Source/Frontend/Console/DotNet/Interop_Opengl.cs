/* vim: set tabstop=8 shiftwidth=4: */
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
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.ServiceProcess;
using System.Text;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Win32;
using Microsoft.Win32.SafeHandles;

using BlueStacks.hyperDroid.Core.VMCommand;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Device;
using BlueStacks.hyperDroid.IdManager;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{
			namespace Interop
			{

				public class Opengl
				{

					public delegate void GlReadyHandler();
					public delegate void GlInitFailedHandler();
					private static String adbPath;

					private static int glMode;
					private static bool initialized = false;

					private static EventWaitHandle glReadyEvent;    /* prevent GC */

					private const int GL_MODE_SOFT = 0;
					private const int GL_MODE_SYS = 1;
					private const int GL_MODE_SYS_OLD = 2;

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern void HdLoggerInit(Logger.HdLoggerCallback cb);

					[DllImport("sys_renderer.dll")]
					private static extern void sys_renderer_logger_thread(Logger.HdLoggerCallback cb,
						SafeWaitHandle evt);

					[DllImport("sys_renderer.dll")]
					private static extern int sys_renderer_init(IntPtr h, int x, int y,
						int width, int height, SafeWaitHandle evt);

					[DllImport("sys_renderer.dll")]
					private static extern IntPtr sys_renderer_get_subwindow();

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern int PgaUtilsIsHotAttach();

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern int PgaServerInit(IntPtr h, int x, int y, int width, int height, SafeWaitHandle evt);

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern void PgaServerHandleCommand(int scancode);

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern IntPtr PgaServerGetSubwindow();

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern IntPtr PgaServerHandleOrientation(float hscale,
						float vscale, int orientation);

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern IntPtr PgaServerHandleAppActivity(
						String package, String activity);

					[DllImport("HD-OpenGl-Native.dll")]
					private static extern int GetPgaServerInitStatus(StringBuilder glVendor, StringBuilder glRenderer, StringBuilder glVersion);

					public static bool Init(String vmName, IntPtr h, int x, int y, int width, int height, GlReadyHandler glReadyHandler, GlInitFailedHandler glInitFailedHandler)
					{
						/*
						 * Fetch the configuration required to spin up GL.
						 */

						String rootPath = BlueStacks.hyperDroid.Common.Strings.RegBasePath;
						String confPath = String.Format(@"{0}\Guests\{1}\Config", rootPath,
							vmName);

						RegistryKey rootKey = Registry.LocalMachine.OpenSubKey(rootPath);
						RegistryKey confKey = Registry.LocalMachine.OpenSubKey(confPath);

						adbPath = (String)rootKey.GetValue("InstallDir") + @"HD-Adb.exe";
						glMode = (int)confKey.GetValue("GlMode");

						rootKey.Close();
						confKey.Close();

						Logger.Info("glMode: " + glMode);


						/*
						 * Load and initlialize the correct dll depending on the mode.
						 * Depend on DllNotFoundException to flag error.
						 */
						if (glMode == GL_MODE_SOFT)
						{
							Console.sPgaInitDone = true;
							glReadyHandler();
							SignalGlReady(vmName);
							return true;
						}

						if (glMode == GL_MODE_SYS || glMode == GL_MODE_SYS_OLD)
						{
							/*
							 * Init GL logging
							 */

							HdLoggerInit(Logger.GetHdLoggerCallback());

							/*
							 * Create a thread that asks the VM to stop Zygote, sets up
							 * OpenGL and then asks the VM to start Zygote.
							 */

							EventWaitHandle evt1 = new EventWaitHandle(false,
							  EventResetMode.ManualReset);

							Thread thr1 = new Thread(delegate ()
							{

								if (hyperDroid.Common.Strings.IsEngineLegacy() &&
									ToGenerateId() == true)
								{
									SetId(vmName);
								}

								if (PgaUtilsIsHotAttach() == 0)
								{
									Logger.Info("Stopping Zygote");
									StopZygote(vmName);
								}

								Logger.Info("Initializing System Renderer");

								if (glMode == GL_MODE_SYS_OLD)
									sys_renderer_init(h, x, y, width, height,
									evt1.SafeWaitHandle);

								if (glMode == GL_MODE_SYS)
									PgaServerInit(h, 0, 0, width, height,
									evt1.SafeWaitHandle);

								evt1.WaitOne();
								initialized = true;

								if (!GetPgaServerInitStatus())
								{
									glInitFailedHandler();
									return;
								}

								if (PgaUtilsIsHotAttach() == 0)
								{
									Logger.Info("Starting Zygote");
									StartZygote(vmName);
								}

								glReadyHandler();
								SignalGlReady(vmName);
							});

							thr1.IsBackground = true;
							thr1.Start();

							return true;
						}

						/*
						 * If we reached here the GlMode is bad.
						 */
						throw new SystemException("Unsupported GlMode " + glMode);
					}

					private static bool ToGenerateId()
					{
						RegistryKey key = Registry.LocalMachine.OpenSubKey(Strings.HKLMAndroidConfigRegKeyPath);
						int generateId = (int)key.GetValue(Strings.GenerateIDKeyName, 0);
						return (generateId != 0);
					}

					private static bool GetPgaServerInitStatus()
					{
						StringBuilder glVendor = new StringBuilder(512);
						StringBuilder glRenderer = new StringBuilder(512);
						StringBuilder glVersion = new StringBuilder(512);
						Logger.Info("Calling GetPgaServerInitStatus");
						int pgaServerInitStatus = -1;
						try
						{
							pgaServerInitStatus = GetPgaServerInitStatus(glVendor, glRenderer, glVersion);
						}
						catch (AccessViolationException e)
						{
							Logger.Info("Error Occured" + e.ToString());
						}
						Console.sPgaInitDone = true;

						if (pgaServerInitStatus != 0)
						{
							Logger.Info("PgaServerInit failed");
							return false;
						}

						Profile.GlVendor = glVendor.ToString();
						Profile.GlRenderer = glRenderer.ToString();
						Profile.GlVersion = glVersion.ToString();

						Logger.Info("GlVendor: " + Profile.GlVendor);
						Logger.Info("GlRenderer: " + Profile.GlRenderer);
						Logger.Info("GlVersion: " + Profile.GlVersion);

						return true;
					}

					private static IntPtr GetSubWindow()
					{
						if (!initialized)
							return IntPtr.Zero;

						if (glMode == GL_MODE_SYS_OLD)
							return sys_renderer_get_subwindow();

						if (glMode == GL_MODE_SYS)
							return PgaServerGetSubwindow();

						return IntPtr.Zero;
					}

					public static bool ShowSubWindow()
					{
						IntPtr subWindow = GetSubWindow();
						if (subWindow == IntPtr.Zero)
							return false;

						Window.ShowWindow(subWindow, Window.SW_SHOWNA);
						return true;
					}

					public static bool HideSubWindow()
					{
						IntPtr subWindow = GetSubWindow();
						if (subWindow == IntPtr.Zero)
							return false;

						Window.ShowWindow(subWindow, Window.SW_HIDE);
						return true;
					}

					public static bool IsSubWindowVisible()
					{
						IntPtr subWindow = GetSubWindow();
						if (subWindow == IntPtr.Zero)
							return false;

						return Window.IsWindowVisible(subWindow);
					}

					public static bool ResizeSubWindow(int x, int y, int cx, int cy)
					{
						Window.SetWindowPos(GetSubWindow(), IntPtr.Zero, x, y, cx, cy, Window.SWP_NOZORDER);
						return true;
					}

					public static bool DrawFB(int cx, int cy, IntPtr buffer, bool ConsoleAccess)
					{
						/*
						 * For Soft GL framebuffer must be drawn by the Frontend.
						 */
						if (glMode == GL_MODE_SOFT)
							return false;

						/*
						 * For system wide mode framebuffer is drawn via gralloc/GL.
						 * No need for Frontend to do anything.
						 */
						if (glMode == GL_MODE_SYS || glMode == GL_MODE_SYS_OLD)
						{
							if ((ConsoleAccess) && (!IsSubWindowVisible()))
								return false;
							else
								return true;
						}

						return true;
					}

					public static void HandleOrientation(float hscale, float vscale, int orientation)
					{
						if (glMode == GL_MODE_SYS)
							PgaServerHandleOrientation(hscale, vscale, orientation);
					}

					public static void HandleCommand(int scancode)
					{
						if (glMode == GL_MODE_SYS)
							PgaServerHandleCommand(scancode);
					}

					public static void HandleAppActivity(String package, String activity)
					{
						PgaServerHandleAppActivity(package, activity);
					}

					public static void StopZygote(String vmName)
					{
						/*
						 * Loop, trying to shutdown Zygote.
						 */

						if (hyperDroid.Common.Strings.IsEngineLegacy())
						{
							while (true)
							{

								try
								{
									Command cmd = new Command();
									cmd.Attach(vmName);

									cmd.SetOutputHandler(delegate (String line)
									{
										Logger.Info("OUT: " + line);
									});

									cmd.SetErrorHandler(delegate (String line)
									{
										Logger.Info("ERR: " + line);
									});

									int res = cmd.Run(new String[] {
				"/system/bin/stop",
				});

									if (res != 0)
										throw new ApplicationException("VM command failed: " +
											res);

								}
								catch (Exception exc)
								{
									Logger.Debug("Cannot stop Zygote: " + exc.ToString());
									Thread.Sleep(500);
									Logger.Debug("Retrying...");
									continue;
								}

								break;
							}
						}
						else
						{
							while (true)
							{
								Interop.Manager manager = null;
								Interop.Monitor monitor = null;
								try
								{
									/* Note that we seek the vmID from MonitorLocator in
									 * the while() loop because sometimes, the vmID/PID of the service
									 * is not yet updated in the registry by the the Plus-Service. In those
									 * cases, we fail to Attach to the Monitor, i.e. we fail to open the bstinput
									 * pipe. So, we should retry to get the update PID and then attach.
									 */
									uint vmId = MonitorLocator.Lookup(vmName);
									manager = Interop.Manager.Open();
									monitor = manager.Attach(vmId, true);
								}
								catch (Exception exc)
								{
									Logger.Debug("Cannot attach to the monitor: " + exc.ToString());
									Thread.Sleep(500);
									Logger.Debug("Retrying...");
									continue;
								}
								try
								{
									monitor.SendControl(Monitor.BstInputControlType.BST_INPUT_CONTROL_TYPE_STOP);
								}
								catch (Exception exc)
								{
									Logger.Debug("Cannot stop Zygote: " + exc.ToString());
									Thread.Sleep(500);
									Logger.Debug("Retrying...");
									continue;
								}
								if (monitor != null)
									monitor.Close();
								break;
							}

						}

					}

					public static void SetId(String vmName)
					{
						Logger.Info("Starting id manager");
						bool loop = true;
						String ID = Id.GenerateID();
						while (true)
						{
							Thread.Sleep(50);
							loop = false;
							try
							{
								Command cmd = new Command();
								cmd.Attach(vmName);

								int res = cmd.Run(new String[] {
			"iSetId", ID ,
			});

								if (res != 0)
								{
									Logger.Info("Failed to set Id:" +
										res);
									loop = true;
								}

							}
							catch
							{
								Logger.Info("Retrying to set Id");
								loop = true;
							}
							if (loop == false)
								break;
						}
						Logger.Info("Set Id success!");
					}


					public static void StartZygote(String vmName)
					{
						if (hyperDroid.Common.Strings.IsEngineLegacy())
						{
							/*
							 * Loop, trying to start Zygote.
							 */

							while (true)
							{

								try
								{
									Command cmd = new Command();
									cmd.Attach(vmName);

									cmd.SetOutputHandler(delegate (String line)
									{
										Logger.Info("OUT: " + line);
									});

									cmd.SetErrorHandler(delegate (String line)
									{
										Logger.Info("ERR: " + line);
									});

									int res = cmd.Run(new String[] {
			"/system/bin/start",
			});

									if (res != 0)
										throw new ApplicationException("Cannot start Zygote: " + res);

								}
								catch (Exception exc)
								{
									Logger.Debug("Cannot start Zygote: " + exc.ToString());
									Thread.Sleep(500);
									Logger.Debug("Retrying...");
									continue;
								}

								break;
							}
						}
						else
						{
							while (true)
							{
								uint vmId = MonitorLocator.Lookup(vmName);
								Interop.Manager manager = null;
								Interop.Monitor monitor = null;
								manager = Interop.Manager.Open();
								try
								{
									monitor = manager.Attach(vmId, true);
								}
								catch (Exception exc)
								{
									Logger.Debug("Cannot attach to the monitor: " + exc.ToString());
									Thread.Sleep(500);
									Logger.Debug("Retrying...");
									continue;
								}
								try
								{
									monitor.SendControl(Monitor.BstInputControlType.BST_INPUT_CONTROL_TYPE_START);
								}
								catch (Exception exc)
								{
									Logger.Debug("Cannot start Zygote: " + exc.ToString());
									Thread.Sleep(500);
									Logger.Debug("Retrying...");
									continue;
								}
								if (monitor != null)
									monitor.Close();
								break;
							}
						}

					}

					private static void SignalGlReady(String vmName)
					{
						String evtName = String.Format("Global\\BlueStacks_Frontend_Gl_Ready_{0}",
							vmName);

						glReadyEvent = new EventWaitHandle(false,
							EventResetMode.ManualReset, evtName);
						glReadyEvent.Set();
					}
				}
			}
		}
	}
}
