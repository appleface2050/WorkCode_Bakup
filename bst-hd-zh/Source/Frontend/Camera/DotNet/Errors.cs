/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * Camera Capture support
 */

using System.Text;
using System.Runtime.InteropServices;
using System.Security;

namespace BlueStacks.hyperDroid.VideoCapture
{
	static public class CameraError
	{
		[DllImport("quartz.dll", CharSet = CharSet.Unicode, ExactSpelling = true, EntryPoint = "AMGetErrorTextW"), SuppressUnmanagedCodeSecurity]
		public static extern int AMGetErrorText(int hr, StringBuilder buf, int max);

		public static string GetCameraErrorString(int hr)
		{
			const int MAX_ERROR_TEXT_LEN = 256;
			StringBuilder err = new StringBuilder(MAX_ERROR_TEXT_LEN, MAX_ERROR_TEXT_LEN);
			if (AMGetErrorText(hr, err, MAX_ERROR_TEXT_LEN) > 0)
			{
				return err.ToString();
			}
			return null;
		}
		public static void ThrowCameraError(int hr)
		{
			//Error occured
			if (hr < 0)
			{
				string s = GetCameraErrorString(hr);
				if (s != null)
				{
					throw new COMException(s, hr);
				}
				else
				{
					Marshal.ThrowExceptionForHR(hr);
				}
			}
		}
	}

	public class ErrorHandler
	{
		int hr;
		public ErrorHandler(int hr)
		{
			this.hr = hr;
			CameraError.GetCameraErrorString(hr);
		}
		public ErrorHandler(ErrorHandler err)
		{
			CameraError.ThrowCameraError(err.hr);
		}
		public static implicit operator ErrorHandler(int hr)
		{
			ErrorHandler e = new ErrorHandler(hr);
			return e;
		}

		public int GetError()
		{
			return hr;
		}
	}
}



