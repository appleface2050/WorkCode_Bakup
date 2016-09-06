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
using System.Runtime.InteropServices;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{
			namespace Interop
			{

				public class Manager
				{

					[DllImport("kernel32.dll")]
					private static extern bool CloseHandle(IntPtr handle);

					private IntPtr handle = IntPtr.Zero;

					private Manager(IntPtr handle)
					{
						this.handle = handle;
					}

					private Manager()
					{
					}

					public static Manager Open()
					{
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							IntPtr handle = Frontend.Modules.Module.ManagerOpen();
							if (handle == IntPtr.Zero)
								Common.ThrowLastWin32Error(
								"Cannot open hyperDroid manager");

							return new Manager(handle);
						}
						else
						{
							return new Manager();
						}
					}

					public void Close()
					{
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							CloseHandle(this.handle);
						}
					}

					public uint[] List()
					{
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							int count, res;
							uint[] monitors;

							do
							{
								count = Frontend.Modules.Module.ManagerList(this.handle, null, 0);
								if (count == -1)
									Common.ThrowLastWin32Error(
									"Cannot get monitor count");

								monitors = new uint[count];

								res = Frontend.Modules.Module.ManagerList(this.handle, monitors, count);
								if (res == -1)
									Common.ThrowLastWin32Error(
									"Cannot get monitor list");

							} while (res != count);
							return monitors;
						}
						else
						{
							return new uint[0];
						}
					}

					public Monitor Attach(uint id, Monitor.ExitHandler exitHandler)
					{
						if (!Frontend.Modules.Module.ManagerAttach(this.handle, id))
							Common.ThrowLastWin32Error(
							"Cannot attach to monitor " + id);

						return new Monitor(this.handle, id, exitHandler);
					}

					public Monitor Attach(uint id, bool verbose)
					{
						return new Monitor(id, verbose);
					}

					public Monitor Attach(uint id, bool verbose, bool isMonAttach)
					{
						if (isMonAttach == true)
							return new Monitor(id, verbose);

						return new Monitor(id);
					}

					public static bool IsVmxActive()
					{
						if (BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy())
						{
							return Frontend.Modules.Module.ManagerIsVmxActive();
						}
						else
						{
							return false;
						}
					}
				}

			}
		}
	}
}
