// THIS CODE AND INFORMATION IS PROVIDED "AS IS" WITHOUT WARRANTY OF
// ANY KIND, EITHER EXPRESSED OR IMPLIED, INCLUDING BUT NOT LIMITED TO
// THE IMPLIED WARRANTIES OF MERCHANTABILITY AND/OR FITNESS FOR A
// PARTICULAR PURPOSE.
//
// Copyright (c) Microsoft Corporation. All rights reserved.

// Turning this on should help if windows only sends updates in WM_TOUCH
// messages.
//#define SUPPORT_WM_TOUCH_DELTAS

using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Windows.Forms;
using System.Runtime.InteropServices;
using System.Diagnostics;
using System.Security.Permissions;

namespace Microsoft.Samples.TabletPC.MTScratchpad.WMTouch
{
	public class WMTouchUserControl : UserControl
	{
		///////////////////////////////////////////////////////////////////////
		// Public interface

		public delegate void LoggerCallback(String msg);

		// Constructor
		[SecurityPermission(SecurityAction.Demand)]
		public WMTouchUserControl(int maxInputs, LoggerCallback loggerCallback)
		{
			this.loggerCallback = loggerCallback;

			// Setup handlers
			try
			{
				Load += new System.EventHandler(this.OnLoadHandler);
			}
			catch (Exception exception)
			{
				Log("ERROR: Could not add form load handler");
				Log(exception.ToString());
			}

			//
			// Pre-allocate the arrays used to receive touch inputs from
			// Windows and to pass touch points to the event handler.
			//

			this.touchInputArray = new TOUCHINPUT[maxInputs];

			for (int ndx = 0; ndx < maxInputs; ndx++)
				this.touchInputArray[ndx] = new TOUCHINPUT();

			this.touchPointArray = new TouchPoint[maxInputs];

			for (int ndx = 0; ndx < maxInputs; ndx++)
				this.touchPointArray[ndx] = new TouchPoint(ndx);

#if SUPPORT_WM_TOUCH_DELTAS

	    this.touchPointIdMap	= new Dictionary<int, TouchPoint>();
	    this.touchPointFreeList	= new Stack<TouchPoint>(maxInputs);

	    for (int ndx = 0; ndx < maxInputs; ndx++)
		this.touchPointFreeList.Push(this.touchPointArray[ndx]);

#endif  // SUPPORT_WM_TOUCH_DELTAS

			//
			// Pre-allocate our touch event argument.
			//

			this.touchEventArgs = new WMTouchEventArgs(this);

			// GetTouchInputInfo need to be
			// passed the size of the structure it will be filling
			// we get the sizes upfront so they can be used later.
			touchInputSize = Marshal.SizeOf(new TOUCHINPUT());
		}

		///////////////////////////////////////////////////////////////////////
		// Protected members, for derived classes.

		protected event EventHandler<WMTouchEventArgs> TouchEvent;

		protected class TouchPoint
		{
			private int x;
			private int y;
			private int id;
			private int slot;

			public int X
			{
				get { return this.x; }
				set { this.x = value; }
			}

			public int Y
			{
				get { return this.y; }
				set { this.y = value; }
			}

			public int Id
			{
				get { return this.id; }
				set { this.id = value; }
			}

			public int Slot
			{
				get { return this.slot; }
			}

			public TouchPoint(int slot)
			{
				Clear();
				this.slot = slot;
			}

			public void Clear()
			{
				this.x = -1;
				this.y = -1;
				this.id = -1;
			}
		}

		protected class WMTouchEventArgs : System.EventArgs
		{
			WMTouchUserControl form;

			public int GetPointCount()
			{
				return form.touchPointArray.Length;
			}

			public TouchPoint GetPoint(int ndx)
			{
				return form.touchPointArray[ndx];
			}

			public WMTouchEventArgs(WMTouchUserControl form)
			{
				this.form = form;
			}
		}

		///////////////////////////////////////////////////////////////////////
		// Private class definitions, structures, attributes and native fn's
		//Exercise1-Task2-Step2 

		// Touch event window message constants [winuser.h]
		private const int WM_TOUCHMOVE = 0x0240;
		private const int WM_TOUCHDOWN = 0x0241;
		private const int WM_TOUCHUP = 0x0242;

		// Touch event flags ((TOUCHINPUT.dwFlags) [winuser.h]
		private const int TOUCHEVENTF_MOVE = 0x0001;
		private const int TOUCHEVENTF_DOWN = 0x0002;
		private const int TOUCHEVENTF_UP = 0x0004;
		private const int TOUCHEVENTF_INRANGE = 0x0008;
		private const int TOUCHEVENTF_PRIMARY = 0x0010;
		private const int TOUCHEVENTF_NOCOALESCE = 0x0020;
		private const int TOUCHEVENTF_PEN = 0x0040;

		// Touch input mask values (TOUCHINPUT.dwMask) [winuser.h]
		private const int TOUCHINPUTMASKF_TIMEFROMSYSTEM = 0x0001; // the dwTime field contains a system generated value
		private const int TOUCHINPUTMASKF_EXTRAINFO = 0x0002; // the dwExtraInfo field is valid
		private const int TOUCHINPUTMASKF_CONTACTAREA = 0x0004; // the cxContact and cyContact fields are valid

		// Touch window flags [winuser.h]
		private const int TWF_FINETOUCH = 0x0001;
		private const int TWF_WANTPALM = 0x0002;

		// Touch API defined structures [winuser.h]
		//Exercise1-Task2-Step4 
		[StructLayout(LayoutKind.Sequential)]
		private struct TOUCHINPUT
		{
			public int x;
			public int y;
			public System.IntPtr hSource;
			public int dwID;
			public int dwFlags;
			public int dwMask;
			public int dwTime;
			public System.IntPtr dwExtraInfo;
			public int cxContact;
			public int cyContact;
		}

		[StructLayout(LayoutKind.Sequential)]
		private struct POINTS
		{
			public short x;
			public short y;
		}

		// Currently touch/multitouch access is done through unmanaged code
		// We must p/invoke into user32 [winuser.h]
		//Exercise1-Task2-Step3 
		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool RegisterTouchWindow(System.IntPtr hWnd, ulong ulFlags);

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool GetTouchInputInfo(System.IntPtr hTouchInput, int cInputs, [In, Out] TOUCHINPUT[] pInputs, int cbSize);

		[DllImport("user32")]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern void CloseTouchInputHandle(System.IntPtr lParam);

		private LoggerCallback loggerCallback;
		private TOUCHINPUT[] touchInputArray;
		private TouchPoint[] touchPointArray;

#if SUPPORT_WM_TOUCH_DELTAS
		private Dictionary<int, TouchPoint> touchPointIdMap;
		private Stack<TouchPoint> touchPointFreeList;
#endif // SUPPORT_WM_TOUCH_DELTAS

		private WMTouchEventArgs touchEventArgs;

		// Attributes
		private int touchInputSize;        // size of TOUCHINPUT structure

		///////////////////////////////////////////////////////////////////////
		// Private methods

		// OnLoad window event handler: Registers the form for multi-touch input.
		// in:
		//      sender      object that has sent the event
		//      e           event arguments
		private void OnLoadHandler(Object sender, EventArgs e)
		{
			ulong ulFlags = TWF_WANTPALM;
			try
			{
				if (!RegisterTouchWindow(this.Handle, ulFlags))
				{
					Log("ERROR: Could not register window for touch");
				}
			}
			catch (Exception exception)
			{
				Log("ERROR: RegisterTouchWindow API not available");
				Log(exception.ToString());
			}
		}

		private void Log(String fmt, params Object[] args)
		{
			this.loggerCallback(String.Format(fmt, args));
		}

		// Window procedure. Receives WM_ messages.
		// Translates WM_TOUCH window messages to touch events.
		// Normally, touch events are sufficient for a derived class,
		// but the window procedure can be overriden, if needed.
		// in:
		//      m       message
		[PermissionSet(SecurityAction.Demand, Name = "FullTrust")]
		protected override void WndProc(ref Message m)
		{
			// Decode and handle WM_TOUCH* message.
			bool handled;
			switch (m.Msg)
			{
				case WM_TOUCHDOWN:
				case WM_TOUCHMOVE:
				case WM_TOUCHUP:
					handled = DecodeTouch(ref m);
					break;
				default:
					handled = false;
					break;
			}

			// Call parent WndProc for default message processing.
			base.WndProc(ref m);

			if (handled)
			{
				// Acknowledge event if handled.
				try
				{
					m.Result = new System.IntPtr(1);
				}
				catch (Exception exception)
				{
					Log("ERROR: Could not allocate result ptr");
					Log(exception.ToString());
				}
			}
		}

		// Extracts lower 16-bit word from an 32-bit int.
		// in:
		//      number      int
		// returns:
		//      lower word
		private static int LoWord(int number)
		{
			return number & 0xffff;
		}

		// Decodes and handles WM_TOUCH* messages.
		// Unpacks message arguments and invokes appropriate touch events.
		// in:
		//      m           window message
		// returns:
		//      flag whether the message has been handled
		private bool DecodeTouch(ref Message m)
		{
			if (TouchEvent == null)
			{
				return false;
			}

			//
			// Determine the number of touch inputs associated with this
			// message, but make sure that we don't try asking for more
			// inputs than will fit in our input list.
			//

			int inputCount = LoWord(m.WParam.ToInt32());
			if (inputCount > this.touchInputArray.Length)
				inputCount = this.touchInputArray.Length;

			//Log("Touch inputs {0}", inputCount);

			//
			// Ask Windows for the details of each touch input associated
			// with this message.
			//

			if (!GetTouchInputInfo(m.LParam, inputCount,
		this.touchInputArray, touchInputSize))
			{
				return false;
			}

#if SUPPORT_WM_TOUCH_DELTAS

	    //
	    // Iterate over the input list once, processing touch down
	    // and touch move events.
	    //

	    for (int ndx = 0; ndx < inputCount; ndx++)
	    {
		TOUCHINPUT input = this.touchInputArray[ndx];

		if ((input.dwFlags & TOUCHEVENTF_DOWN) == 0 &&
		    (input.dwFlags & TOUCHEVENTF_MOVE) == 0)
		{
		    continue;
		}

		//
		// Try to find a touch point with this touch ID.  If
		// we can't, then fetch a new one from the free list
		// and prepare it for use.
		//

		TouchPoint point;

		if (!this.touchPointIdMap.TryGetValue(input.dwID,
		    out point))
		{
		    if (this.touchPointFreeList.Count == 0)
		    {
			Log("Touch point free list is empty");
			continue;
		    }

		    point = this.touchPointFreeList.Pop();
		    point.Id = input.dwID;

		    this.touchPointIdMap[input.dwID] = point;
		}

		//
		// TOUCHINFO coordinates are measured in 100th's of a
		// pixel.  Scale and convert to form relative
		// coordinates.
		//

		Point tmp = PointToClient(new Point(input.x / 100,
		    input.y / 100));

		point.X = tmp.X;
		point.Y = tmp.Y;

		//Log("Slot {0} down", point.Slot);
	    }

	    //
	    // Iterate a second time, processing touch up events.
	    // Processing up events in a separate loop makes sure that
	    // we always see an empty slot when the user lifts a finger.
	    //

	    for (int ndx = 0; ndx < inputCount; ndx++)
	    {
		TOUCHINPUT input = this.touchInputArray[ndx];

		if ((input.dwFlags & TOUCHEVENTF_UP) == 0)
		    continue;

		//
		// Find this point in the point ID map.
		//

		TouchPoint point;

		if (!this.touchPointIdMap.TryGetValue(input.dwID,
		    out point))
		{
		    Log("Touch point not found in map");
		    continue;
		}

		//
		// Remove it from the map and add it to the free list.
		//

		//Log("Slot {0} up", point.Slot);

		this.touchPointIdMap.Remove(input.dwID);
		point.Clear();

		this.touchPointFreeList.Push(point);
	    }

#else  // !SUPPORT_WM_TOUCH_DELTAS

			//
			// Clear the point array.
			//

			for (int ndx = 0; ndx < this.touchPointArray.Length; ndx++)
			{
				this.touchPointArray[ndx].Clear();
			}

			//
			// Populate the now empty point array with our touch input
			// array.
			//

			for (int ndx = 0; ndx < inputCount; ndx++)
			{
				TOUCHINPUT input = this.touchInputArray[ndx];
				TouchPoint point = this.touchPointArray[ndx];

				if ((input.dwFlags & TOUCHEVENTF_DOWN) == 0 &&
					(input.dwFlags & TOUCHEVENTF_MOVE) == 0)
					continue;

				//
				// TOUCHINFO coordinates are measured in 100th's of a
				// pixel.  Scale and convert to form relative
				// coordinates.
				//

				Point tmp = PointToClient(new Point(input.x / 100,
					input.y / 100));

				point.Id = input.dwID;
				point.X = tmp.X;
				point.Y = tmp.Y;
			}

#endif // !SUPPORT_WM_TOUCH_DELTAS

			//
			// Call our handler and cleanup.
			//

			TouchEvent(this, this.touchEventArgs);

			CloseTouchInputHandle(m.LParam);
			return true;
		}
	}
}
