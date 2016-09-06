/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Service
 */

using System;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Core.VMCommand
{

	public class ConsoleControl
	{
		[DllImport("Kernel32")]
		private static extern bool SetConsoleCtrlHandler(Handler handler,
			bool Add);

		public delegate bool Handler(CtrlType ctrlType);

		public enum CtrlType
		{
			CTRL_C_EVENT = 0,
			CTRL_BREAK_EVENT,
			CTRL_CLOSE_EVENT,
			CTRL_LOGOFF_EVENT = 5,
			CTRL_SHUTDOWN_EVENT,
		};

		public static void SetHandler(Handler handler)
		{
			SetConsoleCtrlHandler(handler, true);
		}
	};

}
