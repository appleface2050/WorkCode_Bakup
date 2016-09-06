using System;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlueStacks.hyperDroid.Common;

using Microsoft.Win32;

using WindowInterop = BlueStacks.hyperDroid.Common.Interop.Window;

namespace BlueStacks.hyperDroid.Frontend
{

	public class BstCursor
	{

		private const int COUNT_MAX = 4;

		private const int INITIAL_X = 128;
		private const int INITIAL_Y = 128;

		private class State
		{

			public int SlotId;
			public Bitmap PrimaryImage;
			public Bitmap SecondaryImage;

			public Pointer Pointer;
			public Point Position;
			public bool Clicked;

			public State(int slotId, Bitmap primaryImage,
				Bitmap secondaryImage)
			{
				SlotId = slotId;
				PrimaryImage = primaryImage;
				SecondaryImage = secondaryImage;
			}
		}

		private Console mConsole;
		private InputMapper mInputMapper;

		private State[] mCursors;

		public BstCursor(Console console, String installDir)
		{
			mConsole = console;

			mCursors = new State[COUNT_MAX];

			for (int ndx = 0; ndx < COUNT_MAX; ndx++)
			{

				String primaryPath = String.Format(
					@"{0}\Cursor_{1}_Primary.png", installDir, ndx);
				Bitmap primaryImage = new Bitmap(primaryPath);

				String secondaryPath = String.Format(
					@"{0}\Cursor_{1}_Secondary.png", installDir, ndx);
				Bitmap secondaryImage = new Bitmap(secondaryPath);

				mCursors[ndx] = new State(ndx, primaryImage,
					secondaryImage);
			}
		}

		public void SetInputMapper(InputMapper inputMapper)
		{
			mInputMapper = inputMapper;
		}

		public void Attach(int identity)
		{
			UIHelper.RunOnUIThread(mConsole, delegate ()
			{

				try
				{
					InternalAttach(identity);
				}
				catch (Exception exc)
				{
					Logger.Error(exc.ToString());
				}
			});
		}

		public void Detach(int identity)
		{
			UIHelper.RunOnUIThread(mConsole, delegate ()
			{

				try
				{
					InternalDetach(identity);
				}
				catch (Exception exc)
				{
					Logger.Error(exc.ToString());
				}
			});
		}

		public void Move(int identity, float x, float y, bool absolute)
		{
			UIHelper.RunOnUIThread(mConsole, delegate ()
			{

				try
				{
					InternalMove(identity, x, y, absolute);
				}
				catch (Exception exc)
				{
					Logger.Error(exc.ToString());
				}
			});
		}

		public void Click(int identity, bool down)
		{
			UIHelper.RunOnUIThread(mConsole, delegate ()
			{

				try
				{
					InternalClick(identity, down);
				}
				catch (Exception exc)
				{
					Logger.Error(exc.ToString());
				}
			});
		}

		public void RaiseFocusChange()
		{
			bool foregroundApp = false;

			if (Console.s_Console.IsFrontendReparented() == false &&
					Common.Utils.IsForegroundApplication() == true)
			{
				foregroundApp = true;
			}
			else if (Console.s_Console.IsFrontendReparented() == true &&
					Oem.Instance.IsGamePadEnabled &&
					Console.s_Console.Visible == true)

			{
				foregroundApp = true;
			}

			//Logger.Info("Cursor.RaiseFocusChange()");

			for (int ndx = 0; ndx < COUNT_MAX; ndx++)
			{

				State state = mCursors[ndx];
				if (state.Pointer == null)
					continue;

				if (foregroundApp)
				{
					state.Pointer.Show();
					mConsole.Focus();
				}
				else
				{
					state.Pointer.Hide();
				}
			}
		}

		public void GetNormalizedPosition(int identity, out float x,
			out float y)
		{
			State state = LookupCursor(identity);
			if (state == null)
			{
				x = 0;
				y = 0;
				return;
			}

			Rectangle guestArea = mConsole.GetScaledGuestDisplayArea();

			x = (float)state.Position.X / guestArea.Width;
			y = (float)state.Position.Y / guestArea.Height;
		}

		private void InternalAttach(int identity)
		{
			Logger.Info("Cursor.Attach({0})", identity);

			State state = LookupCursor(identity);
			if (state == null)
			{
				Logger.Warning("Cannot find cursor slot for " +
					"identity {0}", identity);
				return;
			}

			if (state.Pointer != null)
			{
				Logger.Warning("Cursor slot ID %d already " +
					"has a pointer", state.SlotId);
				return;
			}

			Logger.Info("Cursor using slot {0}", state.SlotId);

			state.Position.X = INITIAL_X;
			state.Position.Y = INITIAL_Y;

			state.Clicked = false;

			/*
			 * Create the pointer window.
			 */

			state.Pointer = new Pointer();
			state.Pointer.SetBitmap(state.PrimaryImage);

			/*
			 * Perform a NOP move to display the cursor and then
			 * activate the console in case we caused it to loose
			 * focus.
			 */

			Move(identity, 0, 0, false);
			mConsole.Focus();
		}

		private void InternalDetach(int identity)
		{
			Logger.Info("Cursor.Detach({0})", identity);

			State state = LookupCursor(identity);
			if (state == null)
			{
				Logger.Warning("Cannot find cursor slot for " +
					"identity {0}", identity);
				return;
			}

			state.Pointer.Close();
			state.Pointer = null;

			state.Position.X = 0;
			state.Position.Y = 0;

			state.Clicked = false;
		}

		private void InternalMove(int identity, float x, float y,
			bool absolute)
		{
			//Logger.Info("Cursor.Move({0}, {1}, {2}, {3})", identity,
			//    x, y, absolute);

			State state = LookupCursor(identity);
			if (state == null)
			{
				Logger.Warning("Cannot find cursor slot for " +
					"identity {0}", identity);
				return;
			}

			/*
			 * Force the cursor to be within the bounds of our guest
			 * display area.
			 */

			Rectangle guestArea = mConsole.GetScaledGuestDisplayArea();

			state.Position.X += (int)x;
			if (state.Position.X < 0)
				state.Position.X = 0;
			else if (state.Position.X > guestArea.Width)
				state.Position.X = guestArea.Width;

			state.Position.Y += (int)y;
			if (state.Position.Y < 0)
				state.Position.Y = 0;
			else if (state.Position.Y > guestArea.Height)
				state.Position.Y = guestArea.Height;

			/*
			 * Draw the cursor or hide it depending on whether or not
			 * we are the foreground application.
			 */
			bool isForegroundApp = false;

			if (Console.s_Console.IsFrontendReparented() == false &&
					Common.Utils.IsForegroundApplication() == true)
			{
				isForegroundApp = true;
			}
			else if (Console.s_Console.IsFrontendReparented() == true &&
					Oem.Instance.IsGamePadEnabled &&
					Console.s_Console.Visible == true)

			{
				isForegroundApp = true;
			}

			if (isForegroundApp == true)
			{

				Rectangle screenRect = mConsole.RectangleToScreen(
					guestArea);

				int left = state.Position.X + screenRect.Left -
					state.Pointer.GetBitmap().Width / 2;
				int right = state.Position.Y + screenRect.Top -
					state.Pointer.GetBitmap().Height / 2;

				state.Pointer.Update(left, right);
				state.Pointer.Show();

			}
			else
			{

				state.Pointer.Hide();
			}

			/*
			 * Send the appropriate touch event to the InputMapper.
			 */

			InputMapper.TouchPoint[] touchPoints =
				new InputMapper.TouchPoint[] {
			new InputMapper.TouchPoint(),
			};

			touchPoints[0].X = (float)state.Position.X / guestArea.Width;
			touchPoints[0].Y = (float)state.Position.Y / guestArea.Height;
			touchPoints[0].Down = state.Clicked;

			mInputMapper.TouchHandlerImpl(touchPoints, state.SlotId * 4,
				false);
		}

		private void InternalClick(int identity, bool down)
		{
			//Logger.Info("Cursor.Click({0}, {1})", identity, down);

			State state = LookupCursor(identity);
			if (state == null)
			{
				Logger.Warning("Cannot find cursor slot for " +
					"identity {0}", identity);
				return;
			}

			/*
			 * Record the click state in our cursor slot.
			 */

			state.Clicked = down;

			/*
			 * Switch to the appropriate image based on the new click
			 * state followed by a NOP move to show the result and
			 * dispatch the appropriate touch events.
			 */

			if (!down)
				state.Pointer.SetBitmap(state.PrimaryImage);
			else
				state.Pointer.SetBitmap(state.SecondaryImage);

			Move(identity, 0, 0, false);
		}

		private State LookupCursor(int identity)
		{
			int ndx = -1;

			if (identity >= 0 && identity < COUNT_MAX)
				ndx = COUNT_MAX - 1 - identity;
			else if (identity >= OpenSensor.IDENTITY_OFFSET)
				ndx = identity - OpenSensor.IDENTITY_OFFSET;

			if (ndx >= 0 && ndx < COUNT_MAX)
				return mCursors[ndx];
			else
				return null;
		}

		private class Pointer : Form
		{

			const Int32 WS_EX_TRANSPARENT = 0x00000020;
			const Int32 WS_EX_TOOLWINDOW = 0x00000080;
			const Int32 WS_EX_LAYERED = 0x00080000;
			const byte AC_SRC_OVER = 0x00;
			const byte AC_SRC_ALPHA = 0x01;
			const Int32 ULW_ALPHA = 0x02;


			[StructLayout(LayoutKind.Sequential)]
			struct Win32Point
			{
				public Int32 X;
				public Int32 Y;

				public Win32Point(Int32 x, Int32 y)
				{
					X = x;
					Y = y;
				}
			}

			[StructLayout(LayoutKind.Sequential)]
			struct Win32Size
			{
				public Int32 Width;
				public Int32 Height;

				public Win32Size(Int32 width, Int32 height)
				{
					Width = width;
					Height = height;
				}
			}

			[StructLayout(LayoutKind.Sequential, Pack = 1)]
			struct BLENDFUNCTION
			{
				public byte BlendOp;
				public byte BlendFlags;
				public byte SourceConstantAlpha;
				public byte AlphaFormat;
			}

			[DllImport("user32.dll", SetLastError = true)]
			private static extern bool UpdateLayeredWindow(IntPtr hwnd,
				IntPtr hdcDst, ref Win32Point pptDst, ref Win32Size psize,
				IntPtr hdcSrc, ref Win32Point pprSrc, Int32 crKey,
				ref BLENDFUNCTION pblend, Int32 dwFlags);

			[DllImport("gdi32.dll", SetLastError = true)]
			private static extern IntPtr CreateCompatibleDC(IntPtr hDC);

			[DllImport("user32.dll", SetLastError = true)]
			private static extern IntPtr GetDC(IntPtr hWnd);

			[DllImport("user32.dll", SetLastError = true)]
			private static extern int ReleaseDC(IntPtr hWnd, IntPtr hDC);

			[DllImport("gdi32.dll", SetLastError = true)]
			private static extern bool DeleteDC(IntPtr hdc);

			[DllImport("gdi32.dll", SetLastError = true)]
			private static extern IntPtr SelectObject(IntPtr hDC,
				IntPtr hObject);

			[DllImport("gdi32.dll", SetLastError = true)]
			private static extern bool DeleteObject(IntPtr hObject);

			private Bitmap mBitmap;

			public Pointer()
			{
				SuspendLayout();

				ShowInTaskbar = false;
				FormBorderStyle = FormBorderStyle.None;
				TopMost = true;

				ResumeLayout();
			}

			protected override CreateParams CreateParams
			{
				get
				{
					CreateParams createParams = base.CreateParams;
					createParams.ExStyle |= WS_EX_TRANSPARENT;
					createParams.ExStyle |= WS_EX_TOOLWINDOW;
					createParams.ExStyle |= WS_EX_LAYERED;
					return createParams;
				}
			}

			public void SetBitmap(Bitmap bitmap)
			{
				if (bitmap.PixelFormat !=
					PixelFormat.Format32bppArgb)
					throw new ApplicationException("Bad bitmap");

				mBitmap = bitmap;
			}

			public Bitmap GetBitmap()
			{
				return mBitmap;
			}

			public void Update(int x, int y)
			{
				//Logger.Info("Cursor.Pointer.Update({0}, {1})", x, y);

				IntPtr screenDc = GetDC(IntPtr.Zero);
				IntPtr memDc = CreateCompatibleDC(screenDc);
				IntPtr hBitmap = IntPtr.Zero;
				IntPtr hOldBitmap = IntPtr.Zero;

				try
				{

					/*
					 * Select the desired bitmap into the
					 * current device context.
					 */

					hBitmap = mBitmap.GetHbitmap(
						Color.FromArgb(0));
					hOldBitmap = SelectObject(memDc, hBitmap);

					/*
					 * Prepare for our call to update the
					 * layered window.
					 */

					Win32Size newSize = new Win32Size(
						mBitmap.Width, mBitmap.Height);
					Win32Point srcLoc = new Win32Point(0, 0);
					Win32Point newLoc = new Win32Point(x, y);

					BLENDFUNCTION blend = new BLENDFUNCTION();
					blend.BlendOp = AC_SRC_OVER;
					blend.BlendFlags = 0;
					blend.SourceConstantAlpha = (byte)255;
					blend.AlphaFormat = AC_SRC_ALPHA;

					if (!UpdateLayeredWindow(
						Handle,
						screenDc,
						ref newLoc,
						ref newSize,
						memDc,
						ref srcLoc,
						0,
						ref blend,
						ULW_ALPHA))
						Interop.Common.ThrowLastWin32Error(
							"Cannot update layered window");

				}
				finally
				{

					ReleaseDC(IntPtr.Zero, screenDc);

					if (hBitmap != IntPtr.Zero)
					{
						SelectObject(memDc, hOldBitmap);
						DeleteObject(hBitmap);
					}

					DeleteDC(memDc);
				}
			}
		}
	}

}
