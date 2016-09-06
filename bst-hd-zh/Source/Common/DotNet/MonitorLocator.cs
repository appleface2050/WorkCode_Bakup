/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Library
 */

using System;
using System.IO;
using Microsoft.Win32;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Common
		{

			public class MonitorLocator
			{
				private static String REG_PATH = Path.Combine(Common.Strings.RegBasePath, "Monitors");

				public static void Publish(String vmName, uint vmId)
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey(
						REG_PATH, true);

					/*
					 * Remove any stale entries that include this monitor ID.
					 */

					foreach (String name in key.GetValueNames())
					{

						/* Ignore anything that is the wrong type. */

						RegistryValueKind kind = key.GetValueKind(name);
						if (kind != RegistryValueKind.DWord)
						{
							//Logger.Info("Value {0} has bogus type {1}",
							//    name, kind);
							continue;
						}

						uint value = (uint)(int)key.GetValue(name, 0);
						if (vmId == value)
							key.DeleteValue(name);
					}

					key.SetValue(vmName, vmId, RegistryValueKind.DWord);
				}

				public static uint Lookup(String vmName)
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_PATH);
					return (uint)(int)key.GetValue(vmName, 0);
				}
			}

		}
	}
}
