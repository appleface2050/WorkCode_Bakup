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
using System.Runtime.InteropServices;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{
			namespace Interop
			{

				public class DWM
				{
					private const uint DWM_EC_DISABLECOMPOSITION = 0;
					private const uint DWM_EC_ENABLECOMPOSITION = 1;

					[DllImport("dwmapi.dll")]
					private static extern uint DwmIsCompositionEnabled(ref bool enabled);

					[DllImport("dwmapi.dll")]
					private static extern uint DwmEnableComposition(uint action);

					public static bool CompositionEnabled
					{
						get
						{
							bool enabled = false;

							uint hr = DwmIsCompositionEnabled(ref enabled);
							if (hr != 0)
								throw new SystemException("Cannot check " +
									"if DWM composition is enabled: 0x" +
									hr.ToString("x"));

							return enabled;
						}
					}

					public static void DisableComposition()
					{
						if (!CompositionEnabled)
							return;

						uint hr = DwmEnableComposition(DWM_EC_DISABLECOMPOSITION);
						if (hr != 0)
							throw new SystemException("Cannot disable " +
								"DWM composition: 0x" + hr.ToString("x"));
					}
				}

			}
		}
	}
}
