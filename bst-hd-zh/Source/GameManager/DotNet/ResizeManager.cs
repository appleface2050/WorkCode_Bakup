using System;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Media;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend;

namespace BlueStacks.hyperDroid.GameManager
{
	internal static class ResizeManager
	{
		private static double _aspectRatio;
		private static bool? _adjustingHeight = null;
		private static object sResizeLock = new object();
		static bool Resizing = false;

		internal enum WM
		{
			WINDOWPOSCHANGING = 0x0046,
			EXITSIZEMOVE = 0x0232,
			WM_MOUSEMOVE = 0x0200,
		}
		internal enum SWP
		{
			NOMOVE = 0x0002
		}
		[StructLayout(LayoutKind.Sequential)]
		internal struct WINDOWPOS
		{
			public IntPtr hwnd;
			public IntPtr hwndInsertAfter;
			public int x;
			public int y;
			public int cx;
			public int cy;
			public int flags;
		}

		[DllImport("user32.dll")]
		[return: MarshalAs(UnmanagedType.Bool)]
		internal static extern bool GetCursorPos(ref Win32Point pt);

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern IntPtr SendMessage(IntPtr hWnd, uint Msg, IntPtr wParam, IntPtr lParam);

		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();

		[StructLayout(LayoutKind.Sequential)]
		internal struct Win32Point
		{
			public Int32 X;
			public Int32 Y;
		};
		internal static void Init()
		{
			HwndSource source = PresentationSource.FromVisual(GameManagerWindow.Instance) as HwndSource;
			source.AddHook(DragHook);

			_aspectRatio = (double)16 / 9;
			EnableResizing();
		}

		private static void StartResizeHook()
		{
			if (_hookID == IntPtr.Zero)
			{
				//_hookID = SetHook(_proc);
			}
		}

		private static IntPtr DragHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
		{
			switch ((WM)msg)
			{
				case WM.WINDOWPOSCHANGING:
					{
						if (GameManagerWindow.Instance.WindowState == WindowState.Normal)
						{
							WINDOWPOS pos = (WINDOWPOS)Marshal.PtrToStructure(lParam, typeof(WINDOWPOS));

							if ((pos.flags & (int)SWP.NOMOVE) != 0)
								return IntPtr.Zero;

							Window wnd = (Window)HwndSource.FromHwnd(hwnd).RootVisual;
							if (wnd == null)
								return IntPtr.Zero;

							// determine what dimension is changed by detecting the mouse position relative to the 
							// window bounds. if gripped in the corner, either will work.
							if (!_adjustingHeight.HasValue)
							{
								Point p = GetMousePosition();

								double diffWidth = Math.Min(Math.Abs(p.X - pos.x), Math.Abs(p.X - pos.x - pos.cx));
								double diffHeight = Math.Min(Math.Abs(p.Y - pos.y), Math.Abs(p.Y - pos.y - pos.cy));

								_adjustingHeight = diffHeight > diffWidth;
							}

							if (_adjustingHeight.Value)
								pos.cy = (int)(pos.cx / _aspectRatio); // adjusting height to width change
							else
								pos.cx = (int)(pos.cy * _aspectRatio); // adjusting width to heigth change

							Marshal.StructureToPtr(pos, lParam, true);
							handled = true;
						}
					}
					break;
				case WM.EXITSIZEMOVE:
					{
						_adjustingHeight = null; // reset adjustment dimension and detect again next time window is resized
					}
					break;

			}
			return IntPtr.Zero;
		}

		public static Point GetMousePosition() // mouse position relative to screen
		{
			Win32Point w32Mouse = new Win32Point();
			GetCursorPos(ref w32Mouse);
			return new Point(w32Mouse.X, w32Mouse.Y);
		}


		private static readonly LowLevelMouseProc _proc = HookCallback;
		private static IntPtr _hookID = IntPtr.Zero;


		internal static void EnableResizing()
		{
			IsResizingEnabled = true;
			StartResizeHook();
		}
		internal static void DisableResizng()
		{
			IsResizingEnabled = true;
			//StopResizeHook();
		}
		private static IntPtr SetHook(LowLevelMouseProc proc)
		{
			if (Oem.Instance.IsDebugMode)
			{
				IntPtr hook = SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle("user32"), 0);
				if (hook == IntPtr.Zero)
				{
					throw new System.ComponentModel.Win32Exception();
				}
				return hook;
			}
			else
			{
				using (Process curProcess = Process.GetCurrentProcess())
				using (ProcessModule curModule = curProcess.MainModule)
				{
					return SetWindowsHookEx(WH_MOUSE_LL, proc, GetModuleHandle(curModule.ModuleName), 0);
				}
			}
		}

		private delegate IntPtr LowLevelMouseProc(int nCode, IntPtr wParam, IntPtr lParam);

		[Flags]
		private enum MouseEventFlags
		{
			LEFTDOWN = 0x00000002,
			LEFTUP = 0x00000004,
			MIDDLEDOWN = 0x00000020,
			MIDDLEUP = 0x00000040,
			MOVE = 0x00000001,
			ABSOLUTE = 0x00008000,
			RIGHTDOWN = 0x00000008,
			RIGHTUP = 0x00000010
		}
		private enum MouseMessages
		{
			WM_LBUTTONDOWN = 0x0201,
			WM_LBUTTONUP = 0x0202,
			WM_MOUSEMOVE = 0x0200,
			WM_MOUSEWHEEL = 0x020A,
			WM_RBUTTONDOWN = 0x0204,
			WM_RBUTTONUP = 0x0205
		}

		static bool IsResizingEnabled = false;
		public static bool IsCheckForSlideIn = false;

		private static IntPtr HookCallback(int nCode, IntPtr wParam, IntPtr lParam)
		{
			if (ApplicationIsActivated() && GameManagerWindow.Instance.WindowState == WindowState.Normal && IsResizingEnabled)
			{
				if (nCode >= 0)
				{
					if (MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam && !Resizing)
					{
						var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
						Point p = GameManagerWindow.Instance.PointFromScreen(new Point(hookStruct.pt.x, hookStruct.pt.y));
						CheckPointAndChangeCursor(p);
					}

					if (MouseMessages.WM_LBUTTONDOWN == (MouseMessages)wParam)
					{
						if (CheckCursorAndStartResizing())
						{
							return (IntPtr)1;
						}
					}
				}
			}
			else
			{
				if (ApplicationIsActivated() && GameManagerWindow.Instance.WindowState == WindowState.Maximized && IsCheckForSlideIn)
				{
					if (nCode >= 0)
					{
						if (MouseMessages.WM_MOUSEMOVE == (MouseMessages)wParam && !Resizing)
						{
							var hookStruct = (MSLLHOOKSTRUCT)Marshal.PtrToStructure(lParam, typeof(MSLLHOOKSTRUCT));
							Point p = GameManagerWindow.Instance.PointFromScreen(new Point(hookStruct.pt.x, hookStruct.pt.y));
							if ((GameManagerWindow.Instance.ActualHeight / 100) > p.Y)
							{
								GameManagerWindow.Instance.SlideInTopBar();
							}
						}
					}
				}

				if (currentCursor != CurrentCursor.Normal && IsResizingEnabled)
				{
					currentCursor = CurrentCursor.Normal;
					SystemParametersInfo(0x0057, 0, null, 0);
				}
			}
			return CallNextHookEx(_hookID, nCode, wParam, lParam);
		}

		internal static bool CheckCursorAndStartResizing()
		{
			if (Resizing)
			{
				ReleaseCapture();
			}
			else if (currentCursor != CurrentCursor.Normal)
			{
				Resizing = true;
				Start();
				Resizing = false;
				return true;
			}
			return false;
		}

		internal static void StopResizeHook()
		{
			UnhookWindowsHookEx(_hookID);
			_hookID = IntPtr.Zero;
		}

		private static void Start()
		{
			Thread thread = new Thread(delegate ()
			{
				GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
				{
					ResizeBegin();
					ReleaseCapture();
					HwndSource hwndSource = PresentationSource.FromVisual((Visual)GameManagerWindow.Instance) as HwndSource;
					SendMessage(hwndSource.Handle, 0x112, (IntPtr)61448, IntPtr.Zero);
					ResizeEnd();
				}));
			});
			thread.IsBackground = true;
			thread.Start();
		}

		public static void ResizeBegin()
		{
			if (BTVManager.sRecording)
			{
				Thread thread = new Thread(delegate ()
				{
					lock (sResizeLock)
					{
						BTVManager.SendBTVRequest("stoprecord", null);
						BTVManager.sRecording = false;
						BTVManager.sWasRecording = true;
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}

		}

		public static void ResizeEnd()
		{
			System.Windows.Input.Mouse.OverrideCursor = null;

			if (BTVManager.sWasRecording)
			{
				Thread thread = new Thread(delegate ()
				{
					lock (sResizeLock)
					{
						BTVManager.SetConfig();
						BTVManager.SendBTVRequest("startrecord", null);
						BTVManager.sRecording = true;
						BTVManager.sWasRecording = false;
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
			FrontendHandler.ResizezBanner();
		}


		[DllImport("user32.dll")]
		static extern bool SetSystemCursor(IntPtr hcur, uint id);
		[DllImport("user32.dll")]
		static extern IntPtr LoadCursor(IntPtr hInstance, int lpCursorName);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		static extern bool SystemParametersInfo(uint uiAction, uint uiParam, string pvParam, uint fWinIni);

		private static uint OCR_SIZENWSE = 32642;
		private static uint OCR_SIZENS = 32645;
		private static uint OCR_SIZEWE = 32644;
		//Normal cursor
		private static uint OCR_NORMAL = 32512;

		enum CurrentCursor
		{
			Normal,
			NWSE,
			WE,
			NS
		}
		static CurrentCursor currentCursor = CurrentCursor.Normal;
		public static bool CheckPointAndChangeCursor(Point p)
		{
			Size s = GameManagerWindow.Instance.RenderSize;
			double xdiff = (s.Width - p.X);
			double ydiff = (s.Height - p.Y);
			if (xdiff < 10 && xdiff > 0 && ydiff < 10 && ydiff > 0)
			{
				if (currentCursor != CurrentCursor.NWSE)
				{
					SetSystemCursor(LoadCursor(IntPtr.Zero, (int)OCR_SIZENWSE), OCR_NORMAL);
					currentCursor = CurrentCursor.NWSE;
				}
				return true;
			}
			else if (xdiff < 10 && xdiff > 0 && p.Y > 0 && p.Y < s.Height && !TopBar.Instance.mCloseButton.IsMouseOver)
			{
				if (currentCursor != CurrentCursor.WE)
				{
					SetSystemCursor(LoadCursor(IntPtr.Zero, (int)OCR_SIZEWE), OCR_NORMAL);
					currentCursor = CurrentCursor.WE;
				}
				return true;
			}
			else if (ydiff < 10 && ydiff > 0 && p.X > 0 && p.X < s.Width && !IsMouseOverAndroidButtons(p, ydiff))
			{
				if (currentCursor != CurrentCursor.NS)
				{
					SetSystemCursor(LoadCursor(IntPtr.Zero, (int)OCR_SIZENS), OCR_NORMAL);
					currentCursor = CurrentCursor.NS;
				}
				return true;
			}
			else if (currentCursor != CurrentCursor.Normal)
			{
				ResetCursor();
			}
			return false;
		}

		public static void ResetCursor()
		{
			currentCursor = CurrentCursor.Normal;
			SystemParametersInfo(0x0057, 0, null, 0);
		}

		private static bool IsMouseOverAndroidButtons(Point x, double ydiff)
		{
			if (!Common.Oem.Instance.IsTabsEnabled)
			{
				Point p = GameManagerWindow.Instance.TranslatePoint(x, ContentControl.Instance);
				if (FrontendHandler.frontend.mEmulatedPortraitMode)
				{
					if (p.X > (ContentControl.Instance.ActualWidth / 2.9) && p.X < (ContentControl.Instance.ActualWidth / 2.4) && ydiff > 2)
					{
						return true;
					}
				}
				else
				{
					if (p.X > 0 && p.X < ContentControl.Instance.ActualWidth / 8 && ydiff > 4)
					{
						return true;
					}
				}
			}
			return false;
		}

		private const int WH_MOUSE_LL = 14;

		[StructLayout(LayoutKind.Sequential)]
		private struct POINT
		{
			public int x;
			public int y;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct MSLLHOOKSTRUCT
		{
			public POINT pt;
			public uint mouseData;
			public uint flags;
			public uint time;
			public IntPtr dwExtraInfo;
		}


		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr SetWindowsHookEx(int idHook,
			LowLevelMouseProc lpfn, IntPtr hMod, uint dwThreadId);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool UnhookWindowsHookEx(IntPtr hhk);

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr CallNextHookEx(IntPtr hhk, int nCode,
			IntPtr wParam, IntPtr lParam);

		[DllImport("kernel32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern IntPtr GetModuleHandle(string lpModuleName);


		public static bool ApplicationIsActivated()
		{
			var activatedHandle = GetForegroundWindow();
			if (activatedHandle == IntPtr.Zero)
			{
				return false;       // No window is currently activated
			}

			var procId = Process.GetCurrentProcess().Id;
			int activeProcId;
			GetWindowThreadProcessId(activatedHandle, out activeProcId);

			return activeProcId == procId;
		}


		[DllImport("user32.dll", CharSet = CharSet.Auto, ExactSpelling = true)]
		private static extern IntPtr GetForegroundWindow();

		[DllImport("user32.dll", CharSet = CharSet.Auto, SetLastError = true)]
		private static extern int GetWindowThreadProcessId(IntPtr handle, out int processId);


		public static void Instance_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			ResizeManager.CheckCursorAndStartResizing();
		}
		public static void Frontend_MouseDown(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			if (CheckCursorAndStartResizing())
			{
				FrontendHandler.frontend.IsMouseDownHandled = true;
			}
		}
		public static void Browser_MouseDown(object sender, Gecko.DomMouseEventArgs e)
		{
			if (CheckCursorAndStartResizing())
			{
				e.Handled = true;
			}
		}
		public static void Instance_MouseMove(object sender, System.Windows.Input.MouseEventArgs e)
		{
			if (MouseMoved())
			{
				e.Handled = true;
			}
		}
		public static void Frontend_MouseMove(object sender, System.Windows.Forms.MouseEventArgs e)
		{
			MouseMoved();
		}
		public static void Browser_MouseMove(object sender, Gecko.DomMouseEventArgs e)
		{
			MouseMoved();
		}
		public static void Instance_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			MouseMoved();
		}

		public static void Frontend_MouseLeave(object sender, EventArgs e)
		{
			MouseMoved();
		}
		public static void Browser_MouseLeave(object sender, Gecko.DomMouseEventArgs e)
		{
			MouseMoved();
		}

		private static bool MouseMoved()
		{
			Point p = GameManagerWindow.Instance.PointFromScreen(new System.Windows.Point(System.Windows.Forms.Cursor.Position.X, System.Windows.Forms.Cursor.Position.Y));
			if (GameManagerWindow.Instance.WindowState == WindowState.Normal)
			{
				return ResizeManager.CheckPointAndChangeCursor(p);
			}
			if (GameManagerWindow.Instance.WindowState == WindowState.Maximized && IsCheckForSlideIn)
			{
				if ((GameManagerWindow.Instance.ActualHeight / 100) > p.Y)
				{
					GameManagerWindow.Instance.SlideInTopBar();
				}
			}
			return false;
		}

	}
}
