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
using System.Text;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Common.Interop
{
	public class Window
	{
		public const int WM_CLOSE = 0x0010;
		public const int WM_USER = 0x0400;
		public const int WM_USER_SHOW_WINDOW = WM_USER + 0x1;
		public const int WM_USER_SWITCH_TO_LAUNCHER = WM_USER + 0x2;
		public const int WM_USER_RESIZE_WINDOW = WM_USER + 0x3;
		public const int WM_USER_FE_STATE_CHANGE = WM_USER + 0x4;
		public const int WM_USER_FE_APP_DISPLAYED = WM_USER + 0x5;
		public const int WM_USER_FE_ORIENTATION_CHANGE = WM_USER + 0x6;
		public const int WM_USER_FE_RESIZE = WM_USER + 0x7;
		public const int WM_USER_INSTALL_COMPLETED = WM_USER + 0x8;
		public const int WM_USER_UNINSTALL_COMPLETED = WM_USER + 0x9;
		public const int WM_USER_APP_CRASHED = WM_USER + 0xA;
		public const int WM_USER_EXE_CRASHED = WM_USER + 0xB;
		public const int WM_USER_UPGRADE_FAILED = WM_USER + 0xC;
		public const int WM_USER_BOOT_FAILURE = WM_USER + 0xD;
		public const int WM_USER_FE_SHOOTMODE_STATE = WM_USER + 0xE;

		public const int WM_USER_TOGGLE_FULLSCREEN = WM_USER + 0x20;
		public const int WM_USER_GO_BACK = WM_USER + 0x21;
		public const int WM_USER_SHOW_GUIDANCE = WM_USER + 0x22;
		public const int WM_USER_AUDIO_MUTE = WM_USER + 0x23;
		public const int WM_USER_AUDIO_UNMUTE = WM_USER + 0x24;
		public const int WM_USER_AT_HOME = WM_USER + 0x25;
		public const int WM_USER_ACTIVATE = WM_USER + 0x26;
		public const int WM_USER_HIDE_WINDOW = WM_USER + 0x27;
		public const int WM_USER_VMX_BIT_ON = WM_USER + 0x28;
		public const int WM_USER_DEACTIVATE = WM_USER + 0x29;
		public const int WM_USER_LOGS_REPORTING = WM_USER + 0x30;

		public const int WM_NCHITTEST = 0x0084;
		public const int WM_MOUSEMOVE = 0x0200;
		public const int WM_MOUSEWHEEL = 0x020A;

		public const int WM_LBUTTONDOWN = 0x0201;
		public const int WM_LBUTTONUP = 0x0202;
		public const int WM_LBUTTONDBLCLK = 0x0203;
		public const int WM_DISPLAYCHANGE = 0x007E;

		public const int WM_INPUTLANGCHANGEREQUEST = 0x0050;

		public const int WM_IME_ENDCOMPOSITION = 0x0010E;
		public const int WM_IME_COMPOSITION = 0x0010F;
		public const int WM_IME_CHAR = 0x00286;
		public const int WM_CHAR = 0x00102;
		public const int WM_IME_NOTIFY = 0x00282;
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;
		public const int WM_IME_SETCONTEXT = 0x00281;

		[StructLayout(LayoutKind.Sequential)]
		public struct COMPOSITIONFORM
		{
			public int dwStyle;
			public System.Drawing.Point ptCurrentPos;
			public RECT rcArea;
		}

		[DllImport("Imm32.dll", CharSet = CharSet.Auto)]
		public static extern IntPtr ImmGetContext(IntPtr hWnd);

		[DllImport("imm32.dll")]
		public static extern bool ImmSetCompositionWindow(IntPtr hIMC, out COMPOSITIONFORM lpptPos);

		[DllImport("user32.dll")]
		public static extern int GetSystemMetrics(int which);

		[DllImport("user32.dll")]
		public static extern bool SetWindowPos(IntPtr hwnd,
			IntPtr hwndInsertAfter, int x, int y, int w, int h, uint flags);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool AdjustWindowRect(out RECT lpRect, int dwStyle, bool bMenu);

		[StructLayout(LayoutKind.Sequential)]
		public struct RECT
		{
			public int left, top, right, bottom;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct COPYDATASTRUCT
		{
			public IntPtr dwData;
			public int cbData;
			public string lpData;
		}

		[StructLayout(LayoutKind.Sequential)]
		public struct COPYGAMEPADDATASTRUCT
		{
			public IntPtr dwData;
			public int size;
			public IntPtr lpData;
		}

		public const int WM_COPYDATA = 0x004A;

		public const int SM_CXSCREEN = 0;
		public const int SM_CYSCREEN = 1;
		public const int SWP_FRAMECHANGED = 32;
		public const int SWP_SHOWWINDOW = 64;
		public const int SWP_NOZORDER = 0x0004;
		public const int SWP_NOACTIVATE = 0x0010;

		public const int WS_OVERLAPPED = 0x00000000;
		public const int WS_CAPTION = 0x00C00000;
		public const int WS_SYSMENU = 0x00080000;
		public const int WS_THICKFRAME = 0x00040000;
		public const int WS_MINIMIZEBOX = 0x00020000;
		public const int WS_MAXIMIZEBOX = 0x00010000;
		public const int WS_OVERLAPPEDWINDOW = WS_OVERLAPPED | WS_CAPTION |
			WS_SYSMENU | WS_THICKFRAME | WS_MINIMIZEBOX | WS_MAXIMIZEBOX;

		public enum GetAncestorFlags
		{
			GetParent = 1,
			GetRoot = 2,
			GetRootOwner = 3
		}

		[DllImport("gdi32.dll")]
		private static extern IntPtr CreateDC(String driver, String name,
			String output, IntPtr mode);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteDC(IntPtr hdc);

		[DllImport("gdi32.dll")]
		private static extern Int32 GetDeviceCaps(IntPtr hdc, Int32 index);

		private const int LOGPIXELSX = 88;

		public static IntPtr HWND_TOP = IntPtr.Zero;

		public static int ScreenWidth
		{
			get { return GetSystemMetrics(SM_CXSCREEN); }
		}

		public static int ScreenHeight
		{
			get { return GetSystemMetrics(SM_CYSCREEN); }
		}

		public static void SetFullScreen(IntPtr hwnd)
		{
			SetFullScreen(hwnd, 0, 0, ScreenWidth, ScreenHeight);
		}

		public static void SetFullScreen(IntPtr hwnd, int X, int Y, int cx, int cy)
		{
			if (!SetWindowPos(hwnd, HWND_TOP, X, Y,
				cx, cy, SWP_SHOWWINDOW))
				throw new SystemException(
					"Cannot call SetWindowPos()",
					new Win32Exception(Marshal.GetLastWin32Error()));
		}

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

		[DllImport("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, ref COPYGAMEPADDATASTRUCT cds);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Unicode)]
		public static extern IntPtr FindWindow(String cls, String name);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern bool SetForegroundWindow(IntPtr handle);

		[DllImport("user32.dll")]
		public static extern bool ShowWindow(IntPtr handle, int cmd);

		[DllImport("user32.dll", ExactSpelling = true)]
		public static extern IntPtr GetAncestor(IntPtr hwnd, GetAncestorFlags flags);

		[DllImport("user32.dll")]
		public static extern IntPtr GetParent(IntPtr handle);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern IntPtr SetParent(IntPtr hWndChild, IntPtr hWndNewParent);

		[DllImport("user32.dll")]
		public static extern IntPtr FindWindowEx(IntPtr hwndParent, IntPtr hwndChildAfter, string lpszClass, string lpszWindow);

		[DllImport("kernel32.dll")]
		public static extern bool FreeConsole();

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		public static extern bool IsWindowVisible(IntPtr hWnd);

		[DllImport("user32.dll")]
		public static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll")]
		public static extern uint GetWindowThreadProcessId(IntPtr hWnd, ref uint ProcessId);

		[DllImport("kernel32.dll")]
		public static extern uint GetCurrentThreadId();

		[DllImport("user32.dll")]
		public static extern bool AttachThreadInput(uint idAttach, uint idAttachTo, bool fAttach);

		[DllImport("user32.dll", SetLastError = true)]
		public static extern int GetWindowLong(IntPtr hWnd, int nIndex);

		[DllImport("user32.dll")]
		public static extern int SetWindowLong(IntPtr hWnd, int nIndex, int dwNewLong);

		[DllImport("user32.dll", SetLastError = true, CharSet = CharSet.Auto)]
		public static extern int GetWindowTextLength(IntPtr hWnd);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		public static extern int GetWindowText(IntPtr hWnd, StringBuilder lpString, int nMaxCount);

		[DllImport("user32.dll")]
		public static extern bool GetWindowRect(IntPtr hwnd, ref RECT rect);

		public const int SW_HIDE = 0;
		public const int SW_SHOWMAXIMIZED = 3;
		public const int SW_SHOW = 5;
		public const int SW_MINIMIZE = 6;
		public const int SW_SHOWNA = 8;
		public const int SW_RESTORE = 9;
		public const int SW_SHOWNORMAL = 1;
		public const int GWL_STYLE = (-16);
		public const uint WS_POPUP = 0x80000000;
		public const uint WS_CHILD = 0x40000000;

		public static IntPtr MinimizeWindow(String name)
		{
			IntPtr handle = FindWindow(null, name);
			if (handle == IntPtr.Zero)
				throw new SystemException(
					"Cannot find window '" + name + "'",
					new Win32Exception(Marshal.GetLastWin32Error()));
			ShowWindow(handle, SW_MINIMIZE);
			return handle;
		}

		public static IntPtr BringWindowToFront(String name, bool fullScreen)
		{
			IntPtr handle = FindWindow(null, name);
			if (handle == IntPtr.Zero)
				throw new SystemException(
					"Cannot find window '" + name + "'",
					new Win32Exception(Marshal.GetLastWin32Error()));

			if (!SetForegroundWindow(handle))
				throw new SystemException(
					"Cannot set foreground window",
					new Win32Exception(Marshal.GetLastWin32Error()));

			if (fullScreen)
				ShowWindow(handle, SW_SHOW);

			ShowWindow(handle, SW_SHOW);

			return handle;
		}

		public static IntPtr GetWindowHandle(String name)
		{
			IntPtr handle = FindWindow(null, name);
			if (handle == IntPtr.Zero)
				throw new SystemException(
					"Cannot find window '" + name + "'",
					new Win32Exception(Marshal.GetLastWin32Error()));

			return handle;
		}

		public static bool ForceSetForegroundWindow(IntPtr h)
		{
			if (h == IntPtr.Zero)
				return false;

			IntPtr fgHWND = GetForegroundWindow();

			if (fgHWND == IntPtr.Zero)
				return SetForegroundWindow(h);

			if (h == fgHWND)
				return true;

			uint dummy = 0;
			uint fgThreadId = GetWindowThreadProcessId(fgHWND, ref dummy);
			uint selfThreadId = GetCurrentThreadId();

			if (selfThreadId == fgThreadId)
				return SetForegroundWindow(h);

			if (fgThreadId != 0)
			{
				if (!AttachThreadInput(selfThreadId, fgThreadId, true))
					return false;

				if (!SetForegroundWindow(h))
				{
					AttachThreadInput(selfThreadId, fgThreadId, false);
					return false;
				}

				AttachThreadInput(selfThreadId, fgThreadId, false);
			}

			return true;
		}

		public static int GetScreenDpi()
		{
			IntPtr hdc = CreateDC("DISPLAY", null, null, IntPtr.Zero);
			if (hdc == IntPtr.Zero)
				return -1;

			int dpi = GetDeviceCaps(hdc, LOGPIXELSX);
			if (dpi == 0)
				dpi = 96;

			DeleteDC(hdc);
			return dpi;
		}
	}

}
