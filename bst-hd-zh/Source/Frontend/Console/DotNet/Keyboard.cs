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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Windows.Forms;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{

			public class Keyboard
			{

				[DllImport("user32.dll")]
				private static extern uint MapVirtualKey(uint code, uint mapType);

				private Dictionary<Keys, bool> escapeSet;

				public Keyboard()
				{
					this.escapeSet = new Dictionary<Keys, bool>();

					this.escapeSet.Add(Keys.LWin, true);
					this.escapeSet.Add(Keys.RWin, true);
					this.escapeSet.Add(Keys.Apps, true);
					this.escapeSet.Add(Keys.Home, true);
					this.escapeSet.Add(Keys.End, true);
					this.escapeSet.Add(Keys.PageUp, true);
					this.escapeSet.Add(Keys.PageDown, true);
					this.escapeSet.Add(Keys.Left, true);
					this.escapeSet.Add(Keys.Right, true);
					this.escapeSet.Add(Keys.Up, true);
					this.escapeSet.Add(Keys.Down, true);
				}

				public uint NativeToScanCodes(Keys key)
				{
					uint keyCode = (uint)(key & Keys.KeyCode);
					uint scanCode = MapVirtualKey(keyCode, 0);

					if (NeedEscape(key))
						return 0xe000 | scanCode;
					else
						return scanCode;
				}

				private bool NeedEscape(Keys key)
				{
					return this.escapeSet.ContainsKey(key);
				}

				public bool IsAltDepressed()
				{
					if ((Control.ModifierKeys & Keys.Alt) == Keys.Alt)
						return true;
					else
						return false;
				}
			}

		}
	}
}
