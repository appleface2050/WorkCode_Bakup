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
using System.Windows.Forms;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{

			public class Mouse
			{
				private uint x;
				private uint y;

				private bool b0;
				private bool b1;
				private bool b2;

				public Mouse()
				{
					this.x = 0;
					this.y = 0;

					this.b0 = false;
					this.b1 = false;
					this.b2 = false;
				}

				public void UpdateCursor(uint x, uint y)
				{
					this.x = x;
					this.y = y;
				}

				public void UpdateButton(uint x, uint y, MouseButtons button,
					bool pressed)
				{
					this.x = x;
					this.y = y;

					if (button == MouseButtons.Left)
						this.b0 = pressed;
					else if (button == MouseButtons.Right)
						this.b1 = pressed;
					else if (button == MouseButtons.Middle)
						this.b2 = pressed;
				}

				public uint X
				{
					get { return this.x; }
				}

				public uint Y
				{
					get { return this.y; }
				}

				public uint Mask
				{
					get
					{
						uint mask = 0;

						if (this.b0)
							mask |= 0x01;
						if (this.b1)
							mask |= 0x02;
						if (this.b2)
							mask |= 0x04;

						return mask;
					}
				}
			}

		}
	}
}
