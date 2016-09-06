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

				public class Common
				{

					public static void ThrowLastWin32Error(String msg)
					{
						throw new SystemException(msg,
							new Win32Exception(Marshal.GetLastWin32Error()));
					}
				}

			}
		}
	}
}
