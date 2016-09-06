/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Console Library
 */

#include <windows.h>
#include <winioctl.h>
#include <stdio.h>
#include <stdlib.h>

#include <linux/hd.h>
#include <Hypervisor/Hypervisor.h>
typedef void (WINAPI *LOGGER_CALLBACK)(char *msg);
static LOGGER_CALLBACK			sLoggerCallback;

__declspec(dllexport) HANDLE WINAPI
ManagerOpen(void)
{
	HANDLE handle;

	handle = CreateFile(
	    BST_HD_DEVICE_PATH,
	    GENERIC_READ | GENERIC_WRITE,
	    0,
	    NULL,
	    OPEN_EXISTING,
	    FILE_ATTRIBUTE_NORMAL | FILE_FLAG_OVERLAPPED,
	    NULL);
	if (handle == INVALID_HANDLE_VALUE)
		return NULL;

	return handle;
}

/*
 * Fetch the list of running monitors, storing the results to the caller
 * allocated array array <list>, which is expected to be of length
 * <count>, and returning the monitor count.  To just obtain the number
 * of running monitors, pass <count> as zero.
 */
__declspec(dllexport) int WINAPI
ManagerList(HANDLE handle, unsigned int *list, int count)
{
	BST_HD_IOCTL_LIST_PARAMS params = { 0 };
	DWORD result;
	int ndx;

	if (!DeviceIoControl(
	    handle,
	    BST_HD_IOCTL_LIST,
	    &params,
	    sizeof(params),
	    &params,
	    sizeof(params),
	    &result,
	    NULL))
		return -1;

	if (count < params.Count && list != NULL) {
		SetLastError(ERROR_BUFFER_OVERFLOW);
		return params.Count;
	}

	if (list != NULL)
		for (ndx = 0; ndx < count; ndx++)
			list[ndx] = params.List[ndx];

	return params.Count;
}

static void
Log(const char *fmt, ...)
{
	char buf[128];
	va_list ap;

	va_start(ap, fmt);
	_vsnprintf(buf, sizeof(buf), fmt, ap);
	va_end(ap);

	sLoggerCallback(buf);
}

/*
 * We never actually receive any messages destined for the host class.
 * Instead, this gives us a convenient way to detect when the guest
 * exits.  A guest exit will fail reads with ERROR_BROKEN_PIPE.
 */
__declspec(dllexport) BOOL WINAPI
ManagerAttach(HANDLE handle, unsigned int id)
{
	BST_HD_IOCTL_ATTACH_PARAMS params = { 0 };
	DWORD result;

	params.MonitorId		= id;
	params.ListenerCount		= 1;
	params.ListenerList[0].Class	= HD_CLASS_HOST;
	params.ListenerList[0].Unit	= 0;

	return DeviceIoControl(
	    handle,
	    BST_HD_IOCTL_ATTACH,
	    &params,
	    sizeof(params),
	    &params,
	    sizeof(params),
	    &result,
	    NULL);
}

__declspec(dllexport) BOOL WINAPI
ManagerAttachWithListener(HANDLE handle, unsigned int id, unsigned int cls)
{
	BST_HD_IOCTL_ATTACH_PARAMS params = { 0 };
	DWORD result;

	params.MonitorId		= id;
	params.ListenerCount		= 1;
	params.ListenerList[0].Class	= cls;
	params.ListenerList[0].Unit	= HD_UNIT_ALL;

	return DeviceIoControl(
	    handle,
	    BST_HD_IOCTL_ATTACH,
	    &params,
	    sizeof(params),
	    &params,
	    sizeof(params),
	    &result,
	    NULL);
}

__declspec(dllexport) BOOL WINAPI
ManagerIsVmxActive(void)
{
	HANDLE handle;
	DWORD result;
	DWORD r;
	BOOL active = FALSE;

	handle = ManagerOpen();
	if (handle == NULL)
		return FALSE;

	if (!DeviceIoControl(
	    handle,
	    BST_HD_IOCTL_VMX_CHECK,
	    NULL,
	    0,
	    NULL,
	    0,
	    &result,
	    NULL))
		active = TRUE;
	else
	{
		r = GetLastError();
		if (r != 0)
		{
			Log("Error in device control io: %d", GetLastError());
		}
	}
	CloseHandle(handle);
	return active;
}
__declspec(dllexport) void WINAPI
ManagerSetLogger(LOGGER_CALLBACK logger)
{
	sLoggerCallback	= logger;
}

