/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * Horizontal mouse wheel support for BlueStacks hyperDroid Console Frontend
 */

using System;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{
			namespace Interop
			{

				public class MouseHWheel
				{
					public delegate void MouseHWheelCallback(Int32 x, Int32 y, Int32 keyState, Int32 delta);

					private static MouseHWheelCallback s_MouseHWheelCallback = null;
					private int keyEnableSynaptic = 0;

					public MouseHWheel()
					{
					}

					public MouseHWheel(MouseHWheelCallback cb)
					{
						setMousehWheelCallback(cb);
					}

					public bool setMousehWheelCallback(MouseHWheelCallback cb)
					{
						if (cb == null)
							return false;
						/*
						 * Check for registry key
						 */

						RegistryKey key = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMAndroidConfigRegKeyPath);
						try
						{
							this.keyEnableSynaptic = (int)key.GetValue("HScroll");
						}
						catch
						{
							this.keyEnableSynaptic = 0;
						}

						if (keyEnableSynaptic != 1)
						{
							Logger.Info("Horizontal Mouse Wheel support is Disabled");
							return false;
						}
						s_MouseHWheelCallback = new MouseHWheelCallback(cb);
						try
						{
							if (!Frontend.Modules.Module.SetMouseHWheelCallback(s_MouseHWheelCallback))
							{
								/*
								* It seems there is no synaptic hardware present on host
								* which is not critical, log and continue.
								*/
								Logger.Info("Horizontal scrolling disabled, no synaptic device found");
							}
							return true;
						}
						catch (Exception ex)
						{
							/*
							* Some exception has ocicurred underlying synaptic h/w failed or not present
							* which is not critical, log and continue
							*/
							Logger.Error("Continue with MouseHWheel error:");
							Logger.Error(ex.ToString());
						}

						return false;
					}
				}
			}
		}
	}
}


