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
namespace BlueStacks {
namespace hyperDroid {
namespace Frontend {
namespace Interop {

public class Manager {
	[DllImport(Common.FRONTEND_DLL, SetLastError=true)]
	private static extern IntPtr ManagerOpen();

	[DllImport(Common.FRONTEND_DLL, SetLastError=true)]
	private static extern int ManagerList(IntPtr handle,
	    uint[] list, int count);

	[DllImport(Common.FRONTEND_DLL, SetLastError=true)]
	private static extern bool ManagerAttach(IntPtr handle,
	    uint id);

	[DllImport(Common.FRONTEND_DLL, SetLastError=true)]
	private static extern bool ManagerIsVmxActive();

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(IntPtr handle);

		[DllImport(Common.FRONTEND_DLL, SetLastError=true)]
	private static extern void ManagerSetLogger(LoggerCallback logger);

	private IntPtr handle = IntPtr.Zero;

	private Manager(IntPtr handle)
	{
		this.handle = handle;
	}

	public static Manager Open()
	{
		IntPtr handle = ManagerOpen();
		if (handle == IntPtr.Zero)
			Common.ThrowLastWin32Error(
			    "Cannot open hyperDroid manager");

		return new Manager(handle);
	}

	public void Close()
	{
		CloseHandle(this.handle);
	}

	public uint[] List()
	{
		int count, res;
		uint[] monitors;

		do {
			count = ManagerList(this.handle, null, 0);
			if (count == -1)
				Common.ThrowLastWin32Error(
				    "Cannot get monitor count");

			monitors = new uint[count];

			res = ManagerList(this.handle, monitors, count);
			if (res == -1)
				Common.ThrowLastWin32Error(
				    "Cannot get monitor list");

		} while (res != count);

		return monitors;
	}

	public Monitor Attach(uint id, Monitor.ExitHandler exitHandler)
	{
		if (!ManagerAttach(this.handle, id))
			Common.ThrowLastWin32Error(
			    "Cannot attach to monitor " + id);

		return new Monitor(this.handle, id, exitHandler);
	}

	public static bool IsVmxActive()
	{
		return ManagerIsVmxActive();
	}

	private delegate void LoggerCallback(String msg);
	private static LoggerCallback mLoggerCallback;		/* prevent GC */
	public static void SetLogger()
	{
		mLoggerCallback = new LoggerCallback(delegate(String msg)
		{
		BlueStacks.hyperDroid.Common.Logger.Info("VmxChecker GetLast Error: " + msg);
		});
		ManagerSetLogger(mLoggerCallback);
	}
}

}
}
}
}
