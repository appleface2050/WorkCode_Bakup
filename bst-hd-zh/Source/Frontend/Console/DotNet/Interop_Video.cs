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
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Frontend
		{
			namespace Interop
			{

				public class Video
				{
					public class Mode
					{
						public int width;
						public int height;
						public int depth;

						public Mode(int width, int height, int depth)
						{
							this.width = width;
							this.height = height;
							this.depth = depth;
						}

						public int Width
						{
							get { return this.width; }
						}

						public int Height
						{
							get { return this.height; }
						}

						public int Depth
						{
							get { return this.depth; }
						}
					};

					private const uint OFFSET_MAGIC = 0x00;
					private const uint OFFSET_LENGTH = 0x04;
					private const uint OFFSET_OFFSET = 0x08;
					private const uint OFFSET_MODE = 0x0c;
					private const uint OFFSET_STRIDE = 0x10;
					private const uint OFFSET_DIRTY = 0x14;

					private IntPtr addr;
					private unsafe byte* raw;

					public unsafe Video(IntPtr addr)
					{
						this.addr = addr;
						this.raw = (byte*)addr;
					}

					/*
					 * Check the magic field in the video header.  Throws an
					 * exception if the magic field contains in invalid value.
					 */
					public void CheckMagic()
					{
						uint magic = 0;
						bool success;

						success = Frontend.Modules.Module.VideoCheckMagic(this.addr, ref magic);
						if (!success)
							throw new SystemException("Bad magic 0x" +
								magic.ToString("x"));
					}

					public unsafe Mode GetMode()
					{
						uint width = 0;
						uint height = 0;
						uint depth = 0;

						Frontend.Modules.Module.VideoGetMode(this.addr, ref width, ref height,
							ref depth);
						return new Mode((int)width, (int)height, (int)depth);
					}

					public bool GetAndClearDirty()
					{
						return Frontend.Modules.Module.VideoGetAndClearDirty(this.addr);
					}

					public unsafe uint GetStride()
					{
						ushort* stride = (ushort*)(this.raw + OFFSET_STRIDE);
						return *stride;
					}

					public unsafe IntPtr GetBufferAddr()
					{
						uint* offset = (uint*)(this.raw + OFFSET_OFFSET);
						uint* start = (uint*)(this.raw + *offset);

						return (IntPtr)start;
					}

					public unsafe IntPtr GetBufferEnd()
					{
						uint* offset = (uint*)(this.raw + OFFSET_LENGTH);
						uint* end = (uint*)(this.raw + *offset);

						return (IntPtr)end;
					}

					public unsafe uint GetBufferSize()
					{
						int diff = (int)GetBufferEnd() - (int)GetBufferAddr();
						if (diff < 0)
							throw new SystemException("Buffer size is negative");

						return (uint)diff;
					}
				}

			}
		}
	}
}
