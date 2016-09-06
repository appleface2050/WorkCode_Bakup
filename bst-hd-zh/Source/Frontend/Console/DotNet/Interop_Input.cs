using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks.hyperDroid.Frontend
{
	namespace Interop
	{

		class Input
		{
			private const uint MOUSEEVENTF_FROMTOUCH = 0xFF515780;
			private const uint MOUSEEVENTF_FROMPEN = 0xFF515700;
			private const uint MOUSEEVENTF_MASK = 0xFFFFFF80;

			[DllImport("user32.dll")]
			private static extern uint GetMessageExtraInfo();

			public static bool IsEventFromTouch()
			{
				return (GetMessageExtraInfo() & MOUSEEVENTF_MASK) ==
					MOUSEEVENTF_FROMTOUCH;
			}

			[DllImport("kernel32.dll", SetLastError = true)]
			private static extern ushort GlobalAddAtom(String str);

			[DllImport("user32.dll", SetLastError = true)]
			private static extern bool SetProp(IntPtr wind, String str,
				IntPtr data);

			public static void DisablePressAndHold(IntPtr hWnd)
			{
				String atom = "MicrosoftTabletPenServiceProperty";
				ushort atomId;

				atomId = GlobalAddAtom(atom);
				if (atomId == 0)
					throw new SystemException("Cannot add global atom",
						new Win32Exception(Marshal.GetLastWin32Error()));

				if (!SetProp(hWnd, atom, (IntPtr)1))
					throw new SystemException("Cannot set property",
						new Win32Exception(Marshal.GetLastWin32Error()));
			}

			/*
			 * Keyboard hooks to prevent Windows from interpreting
			 * interesting keys while we are running.
			 */

			private const int WM_KEYDOWN = 0x0100;
			private const int WM_KEYUP = 0x0100;
			private const int WM_SYSKEYDOWN = 0x0104;
			private const int WM_SYSKEYUP = 0x0105;

			private const int WH_KEYBOARD_LL = 13;

			private const int HC_ACTION = 0;

			public const int VK_LWIN = 0x5b;

			[DllImport("user32.dll", SetLastError = true)]
			private static extern int SetWindowsHookEx(int type,
				HookProc callback, IntPtr module, uint threadId);

			[DllImport("user32.dll")]
			private static extern int CallNextHookEx(int handle, int code,
				uint wparam, IntPtr lparam);

			[DllImport("user32.dll")]
			private static extern bool UnhookWindowsHookEx(int handle);

			[DllImport("kernel32.dll")]
			private static extern IntPtr GetModuleHandle(IntPtr name);

#pragma warning disable 0649
			struct HookData
			{
				public UInt32 vkCode;
				public UInt32 scanCode;
				public UInt32 flags;
				public UInt32 time;
				public IntPtr dwExtraInfo;
			};
#pragma warning restore 0649

			/*
			 * The keyboard callback must return true if the keypress should
			 * be passed on to Windows for regular processing and false
			 * otherwise.
			 */
			public delegate bool KeyboardCallback(bool pressed, uint key);

			private delegate int HookProc(int code, uint wparam, IntPtr lparam);

			private static int sHookHandle = 0;
			private static HookProc sHookProc = null;   /* prevents GC */

			public static void HookKeyboard(KeyboardCallback cb)
			{
				sHookProc = delegate (int code, uint wparam, IntPtr lparam)
				{

					/*
					 * If the code is less than zero, we're supposed
					 * to just call the next hook in the chain and
					 * return.
					 */

					if (code < 0)
						return CallNextHookEx(sHookHandle, code,
							wparam, lparam);

					/*
					 * Just pass it on if this is a system key.
					 */

					if (wparam == WM_SYSKEYDOWN || wparam == WM_SYSKEYUP)
						return CallNextHookEx(sHookHandle, code,
							wparam, lparam);

					/*
					 * Marshal the keyboard data into a managed
					 * structure.
					 */

					HookData data = (HookData)Marshal.PtrToStructure(
						lparam, typeof(HookData));

					/*
					 * Call our callback function and then call the
					 * next hook in the chain if we are told to.
					 */

					bool pressed = (wparam == WM_KEYDOWN);

					bool callNext = cb(pressed, data.vkCode);
					if (callNext)
						return CallNextHookEx(sHookHandle, code,
							wparam, lparam);
					else
						return 1;
				};

				if (sHookHandle != 0)
					throw new SystemException("Keyboard hook is " +
						"already set");

				IntPtr mod = GetModuleHandle(IntPtr.Zero);

				sHookHandle = SetWindowsHookEx(WH_KEYBOARD_LL, sHookProc,
					mod, 0);
				if (sHookHandle == 0)
					throw new SystemException("Cannot set hooks",
						new Win32Exception(Marshal.GetLastWin32Error()));
			}

			public static void UnhookKeyboard()
			{
				if (sHookHandle != 0)
				{
					UnhookWindowsHookEx(sHookHandle);
					sHookHandle = 0;
				}
			}
		}

	}
}
