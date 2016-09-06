using System;
using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend.Interop
{

	public class Animate
	{

		public const int AW_HOR_POSITIVE = 0X00000001;
		public const int AW_VER_POSITIVE = 0x00000004;
		public const int AW_VER_NEGATIVE = 0x00000008;
		public const int AW_CENTER = 0X00000010;
		public const int AW_HIDE = 0x00010000;
		public const int AW_ACTIVATE = 0X00020000;
		public const int AW_SLIDE = 0X00040000;
		public const int AW_BLEND = 0x00080000;

		[DllImport("User32.dll")]
		public static extern bool AnimateWindow(IntPtr hwnd, int dwTime,
			int dwFlags);
	}

}
