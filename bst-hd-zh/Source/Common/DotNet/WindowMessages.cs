using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Win32;
using System.Runtime.InteropServices;

using BlueStacks.hyperDroid.Frontend;

namespace BlueStacks.hyperDroid.Common
{
	public class WindowMessages
	{
		[StructLayout(LayoutKind.Sequential)]
		struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int size;
			public IntPtr lpData;
		}

		private static string className = Oem.Instance.MsgWindowClassName;
		private static string windowName = Oem.Instance.MsgWindowTitle;

		public static void NotifyOrientationChangeToParentWindow(bool mEmulatedPortraitMode)
		{
			Logger.Info("Sending FE_ORIENTATION_CHANGE message to class = {0}, window = {1}", className, windowName);
			try
			{
				IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}

				uint temp = mEmulatedPortraitMode ? 1u : 0u;
				Common.Interop.Window.SendMessage(
						handle,
						Common.Interop.Window.WM_USER_FE_ORIENTATION_CHANGE,
						(IntPtr)temp,
						IntPtr.Zero
						);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}
		public static void NotifyBootFailureToParentWindow(int exitCode)
		{
			Logger.Info("Sending BOOT_FAILURE message to class = {0}, window = {1}", className, windowName);
			IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
			try
			{
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				uint wparam = (uint)exitCode;
				Logger.Info("Sending wparam : {0}", wparam);
				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_USER_BOOT_FAILURE, (IntPtr)wparam, IntPtr.Zero);
				Logger.Info("Sent BOOT_FAILURE message");
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}
		public static void NotifyVMXBitOnToParentWindow()
		{
			Logger.Info("Sending VMXBitON message to class = {0}, window = {1}", className, windowName);
			IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
			try
			{
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_USER_VMX_BIT_ON, IntPtr.Zero, IntPtr.Zero);
				Logger.Info("Sent VMX Bit On message");
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured While sending VMX bit on Message, Err: {0}", e.ToString()));
			}
		}
		public static void NotifyStateToParentWindow(Frontend.State frontendState)
		{
			Logger.Info("Sending FE_STATE_CHANGE message to class = {0}, window = {1}", className, windowName);
			IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
			try
			{
				uint state = 0;
				if (frontendState == State.Initial)
				{
					state = 1;
				}
				else if (frontendState == State.Connected)
				{
					state = 2;
				}
				else if ((BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy() && frontendState == State.CannotStart)
						|| (!BlueStacks.hyperDroid.Common.Strings.IsEngineLegacy() && frontendState == State.Error))
				{
					state = 3;
				}
				else if (frontendState == State.Quitting)
				{
					state = 4;
				}

				if (state != 0)
				{
					if (handle == IntPtr.Zero)
					{
						Logger.Info("Unable to find window : {0}", className);
						return;
					}
					Logger.Info("Sending wparam : {0} as FE_STATE", state);
					Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_USER_FE_STATE_CHANGE, (IntPtr)state, IntPtr.Zero);
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}
		public static void NotifyExeCrashToParentWindow(String className, String windowName)
		{
			Logger.Info("Sending WM_USER_EXE_CRASHED message to class = {0}, window = {1}", className, windowName);
			try
			{
				IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				Common.Interop.Window.SendMessage(
						handle,
						Common.Interop.Window.WM_USER_EXE_CRASHED,
						IntPtr.Zero,
						IntPtr.Zero
						);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}
		public static void NotifyShootingModeStateToParentWindow(bool isShootingMode)
		{
			Logger.Info("Sending FE_SHOOTMODE_STATE message to class = {0}, window = {1}", className, windowName);
			try
			{
				IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				uint wparam = (uint)(isShootingMode ? 1 : 0);
				Logger.Info("Sending wparam value: {0}", wparam);
				Common.Interop.Window.SendMessage(
						handle,
						Common.Interop.Window.WM_USER_FE_SHOOTMODE_STATE,
						(IntPtr)wparam,
						IntPtr.Zero
						);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}
		public static void NotifyFrontendResizeToParentWindow(bool isFullScreen)
		{
			Logger.Info("Sending FE_RESIZE message to class = {0}, window = {1}", className, windowName);
			try
			{
				IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				uint wparam = (uint)(isFullScreen ? 1 : 0);
				Common.Interop.Window.SendMessage(
						handle,
						Common.Interop.Window.WM_USER_FE_RESIZE,
						(IntPtr)wparam,
						IntPtr.Zero
						);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}

		public static void NotifyAppDisplayedToParentWindow(bool isHome)
		{
			Logger.Info("Sending FE_APP_DISPLAYED message to class = {0}, window = {1}", className, windowName);
			try
			{
				IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				uint wparam = (uint)(isHome ? 0 : 1);
				Logger.Info("Sending wparam : {0} as HomeFlag = {1}", wparam, isHome);
				Common.Interop.Window.SendMessage(
						handle,
						Common.Interop.Window.WM_USER_FE_APP_DISPLAYED,
						(IntPtr)wparam,
						IntPtr.Zero
						);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}

		public static void NotifyExeCrashToParentWindow()
		{
			Logger.Info("Sending WM_USER_EXE_CRASHED message to class = {0}, window = {1}", className, windowName);
			try
			{
				IntPtr handle = BlueStacks.hyperDroid.Common.Interop.Window.FindWindow(className, windowName);
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				BlueStacks.hyperDroid.Common.Interop.Window.SendMessage(
						handle,
						BlueStacks.hyperDroid.Common.Interop.Window.WM_USER_EXE_CRASHED,
						IntPtr.Zero,
						IntPtr.Zero
						);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}

		public static void ForwardStatusToParentWindow(string status)
		{
			IntPtr ptrStatusData = IntPtr.Zero;
			try
			{
				Logger.Info("We will forwarding the status {0} to parent window", status);

				IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);

				COPYDATASTRUCT statusData = new COPYDATASTRUCT();
				statusData.dwData = new IntPtr(1);
				statusData.size = (status.Length + 1);
				statusData.lpData = Marshal.StringToHGlobalAnsi(status);

				ptrStatusData = Marshal.AllocCoTaskMem(Marshal.SizeOf(statusData));
				Marshal.StructureToPtr(statusData, ptrStatusData, false);

				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_COPYDATA, IntPtr.Zero, ptrStatusData);
				Logger.Info("the message has been forwarded");

			}
			catch (Exception ex)
			{
				Logger.Error("got exception in forwarding message to parent window ex:{0}", ex.ToString());
			}
			finally
			{
				if (ptrStatusData != IntPtr.Zero)
					Marshal.FreeCoTaskMem(ptrStatusData);
			}
		}

	}
}

