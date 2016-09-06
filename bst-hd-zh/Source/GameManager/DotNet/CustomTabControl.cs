/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Drawing.Imaging;
using System.Security.Permissions;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{

	public class CustomTabControl : TabControl
	{
		public			Bitmap			mBackImage;
		private			Bitmap			mBackBuffer;
		private			Graphics		mBackBufferGraphics;
		private			Bitmap			mTabBuffer;
		private			Graphics		mTabBufferGraphics;

		private			TabStyleProvider	mStyleProvider;

		private static		bool			sIsFullScreen			= false;
		private 		int			mHeightToPaint;

		public CustomTabControl()
		{
			mHeightToPaint = GameManager.sGameManager.GetTabBarHeight() + 1;	// +1 for the bottom border

			this.SetStyle(ControlStyles.UserPaint | ControlStyles.AllPaintingInWmPaint
					| ControlStyles.Opaque | ControlStyles.ResizeRedraw , true);

			this.mBackBuffer = new Bitmap(this.Width, mHeightToPaint);
			this.mBackBufferGraphics = Graphics.FromImage(this.mBackBuffer);
			this.mTabBuffer = new Bitmap(this.Width, mHeightToPaint);
			this.mTabBufferGraphics = Graphics.FromImage(this.mTabBuffer);
			string parentStyleTheme = "Em";
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", parentStyleTheme);

			if(parentStyleTheme == "Em")
				this.mStyleProvider = new TabStyleProviderDefault(this);
			else
				this.mStyleProvider = new TabStyleProviderNew(this);
		}

		protected override void OnCreateControl()
		{
			base.OnCreateControl();
			this.OnFontChanged(EventArgs.Empty);
		}

		protected override void Dispose(bool disposing)
		{
			base.Dispose(disposing);
			if (disposing)
			{
				if (this.mBackImage != null)
				{
					this.mBackImage.Dispose();
				}
				if (this.mBackBufferGraphics != null)
				{
					this.mBackBufferGraphics.Dispose();
				}
				if (this.mBackBuffer != null)
				{
					this.mBackBuffer.Dispose();
				}
				if (this.mTabBufferGraphics != null)
				{
					this.mTabBufferGraphics.Dispose();
				}
				if (this.mTabBuffer != null)
				{
					this.mTabBuffer.Dispose();
				}
			}
		}


		public bool FullScreen
		{
			get
			{
				return sIsFullScreen;
			}
			set
			{
				sIsFullScreen = value;
			}
		}

		public override Rectangle DisplayRectangle
		{
			get
			{
				int itemHeight = this.ItemSize.Height;

				return new Rectangle(0, itemHeight, Width, Height - itemHeight);
			}
		}

		public int ActiveIndex
		{
			get
			{
				NativeMethods.TCHITTESTINFO hitTestInfo = new NativeMethods.TCHITTESTINFO(this.PointToClient(Control.MousePosition));
				int index = NativeMethods.SendMessage(this.Handle, NativeMethods.TCM_HITTEST, IntPtr.Zero, NativeMethods.ToIntPtr(hitTestInfo)).ToInt32();
				if (index == -1)
				{
					return -1;
				}
				else
				{
					if (this.TabPages[index].Enabled)
					{
						return index;
					}
					else
					{
						return -1;
					}
				}
			}
		}

		public TabPage ActiveTab
		{
			get
			{
				int activeIndex = this.ActiveIndex;
				if (activeIndex > -1)
				{
					return this.TabPages[activeIndex];
				}
				else
				{
					return null;
				}
			}
		}

		[Category("Action")] public event EventHandler<TabControlCancelEventArgs> TabClosing;

		protected override void OnFontChanged(EventArgs e)
		{
			IntPtr hFont = this.Font.ToHfont();
			NativeMethods.SendMessage(this.Handle, NativeMethods.WM_SETFONT, hFont, (IntPtr)(-1));
			NativeMethods.SendMessage(this.Handle, NativeMethods.WM_FONTCHANGE, IntPtr.Zero, IntPtr.Zero);
			this.UpdateStyles();
			if (this.Visible)
			{
				this.Invalidate();
			}
		}

		protected override void OnResize(EventArgs e)
		{
			mHeightToPaint = GameManager.sGameManager.GetTabBarHeight() + 1;	// +1 for the bottom border

			//	Recreate the buffer for manual double buffering
			if (this.Width > 0 && this.Height > 0)
			{
				if (this.mBackImage != null)
				{
					this.mBackImage.Dispose();
					this.mBackImage = null;
				}
				if (this.mBackBufferGraphics != null)
				{
					this.mBackBufferGraphics.Dispose();
				}
				if (this.mBackBuffer != null)
				{
					this.mBackBuffer.Dispose();
				}

				this.mBackBuffer = new Bitmap(this.Width, mHeightToPaint);
				this.mBackBufferGraphics = Graphics.FromImage(this.mBackBuffer);

				if (this.mTabBufferGraphics != null)
				{
					this.mTabBufferGraphics.Dispose();
				}
				if (this.mTabBuffer != null)
				{
					this.mTabBuffer.Dispose();
				}

				this.mTabBuffer = new Bitmap(this.Width, mHeightToPaint);
				this.mTabBufferGraphics = Graphics.FromImage(this.mTabBuffer);

				if (this.mBackImage != null)
				{
					this.mBackImage.Dispose();
					this.mBackImage = null;
				}

			}
			base.OnResize(e);
		}

		protected override void OnParentBackColorChanged(EventArgs e)
		{
			if (this.mBackImage != null)
			{
				this.mBackImage.Dispose();
				this.mBackImage = null;
			}
			base.OnParentBackColorChanged(e);
		}

		protected override void OnParentBackgroundImageChanged(EventArgs e)
		{
			if (this.mBackImage != null)
			{
				this.mBackImage.Dispose();
				this.mBackImage = null;
			}
			base.OnParentBackgroundImageChanged(e);
		}

		private void OnParentResize(object sender, EventArgs e)
		{
			if (this.Visible)
			{
				this.Invalidate();
			}
		}


		protected override void OnParentChanged(EventArgs e)
		{
			base.OnParentChanged(e);
			if (this.Parent != null)
			{
				this.Parent.Resize += this.OnParentResize;
			}
		}

		protected override void OnMove(EventArgs e)
		{
			if (this.Width > 0 && this.Height > 0)
			{
				if (this.mBackImage != null)
				{
					this.mBackImage.Dispose();
					this.mBackImage = null;
				}
			}
			base.OnMove(e);
			this.Invalidate();
		}

		protected override void OnControlAdded(ControlEventArgs e)
		{
			base.OnControlAdded(e);
			if (this.Visible)
			{
				this.Invalidate();
			}
		}

		protected override void OnControlRemoved(ControlEventArgs e)
		{
			base.OnControlRemoved(e);
			if (this.Visible)
			{
				this.Invalidate();
			}
		}


		[UIPermission(SecurityAction.LinkDemand, Window = UIPermissionWindow.AllWindows)]
			protected override bool ProcessMnemonic(char charCode)
			{
				foreach (TabPage page in this.TabPages)
				{
					if (IsMnemonic(charCode, page.Text))
					{
						this.SelectedTab = page;
						return true;
					}
				}
				return base.ProcessMnemonic(charCode);
			}

		protected override void OnMouseClick(MouseEventArgs e)
		{
			int index = this.ActiveIndex;
			if (index > -1 && (this.GetTabCloserRect(index, null).Contains(this.MousePosition) || e.Button == System.Windows.Forms.MouseButtons.Middle))
			{
				//	If we are clicking on a closer then remove the tab instead of raising the standard mouse click event
				//	But raise the tab closing event first
				TabPage tab = this.ActiveTab;
				TabControlCancelEventArgs args = new TabControlCancelEventArgs(tab, index, false, TabControlAction.Deselecting);
				this.OnTabClosing(args);
				if (!args.Cancel)
				{
					this.TabPages.Remove(tab);
					tab.Dispose();
				}
			}
			else
			{
				base.OnMouseClick(e);
			}
		}

        protected override void  OnMouseDown(MouseEventArgs e)
        {
            if ((e.Button == MouseButtons.Left) && !GameManager.sGameManager.FullScreen)
            {
                int index = this.ActiveIndex;
			    if (index < 0 || (!this.GetTabCloserRect(index, null).Contains(this.MousePosition)))
                {
                    GameManager.ReleaseCapture();
                    GameManager.SendMessage(GameManager.sGameManager.Handle,
                            GameManager.WM_NCLBUTTONDOWN, GameManager.HT_CAPTION, 0);
                }
               
            }
 	         base.OnMouseDown(e);
        }

		protected override void OnDeselecting(TabControlCancelEventArgs e)
		{
            var temp = this.ActiveIndex;
			if (this.GetTabCloserRect(this.ActiveIndex, null).Contains(this.MousePosition))
			{
                //exclude Back button
                if (!GameManager.sGameManager.mControlBarLeft.DisplayRectangle.Contains(this.MousePosition))
                {
                    if (SelectedIndex != temp)
                    {
                        e.Cancel = true;
                    }
                }
			}

			base.OnDeselecting(e);
		}

		protected virtual void OnTabClosing(TabControlCancelEventArgs e)
		{
			if (this.TabClosing != null)
			{
				this.TabClosing(this, e);
			}
		}		

		protected override void OnMouseMove(MouseEventArgs e)
		{
			base.OnMouseMove(e);
			Rectangle tabRect = this.mStyleProvider.GetTabRect(this.ActiveIndex);
			if (tabRect.Contains(this.MousePosition))
			{
				this.Invalidate();
			}
		}

		protected override void OnPaint(PaintEventArgs e)
		{
//			base.OnPaint(e);
			this.CustomPaint(e.Graphics);
		}

		public void CustomPaint(Graphics screenGraphics)
		{
			//	We render into a bitmap that is then drawn in one shot rather than using
			//	double buffering built into the control as the built in buffering
			// 	messes up the background painting.
			//	Equally the .Net 2.0 BufferedGraphics object causes the background painting
			//	to mess up, which is why we use this .Net 1.1 buffering technique.

			//	Buffer code from Gil. Schmidt http://www.codeproject.com/KB/graphics/DoubleBuffering.aspx
			if (this.mBackImage == null)
			{
				//	Cached Background Image
				Logger.Info("heightToPoint: " + mHeightToPaint);
				Logger.Info("Rectangle height: " + this.ClientRectangle.Height);
				this.mBackImage = new Bitmap(this.Width, mHeightToPaint);
				Graphics backGraphics = Graphics.FromImage(this.mBackImage);
				backGraphics.Clear(Color.Transparent);
				this.PaintTransparentBackground(backGraphics, this.ClientRectangle);

				this.mBackBufferGraphics.Clear(Color.Transparent);
				this.mBackBufferGraphics.DrawImageUnscaled(this.mBackImage, 0, 0);

				this.mTabBufferGraphics.Clear(Color.Transparent);
			}
			if (this.TabCount > 0)
			{
				// Draw each tabpage from right to left.
				for (int index = this.TabCount - 1; index >= 0; index--)
				{
					if (index != this.SelectedIndex)
					{
						this.DrawTabPage(index, this.mTabBufferGraphics);
					}
				}
				if (this.SelectedTab != null)
				{
					// The selected tab must be drawn last so it appears on top.
					this.DrawTabPage(this.SelectedIndex, this.mTabBufferGraphics);
				}
			}
			this.mTabBufferGraphics.Flush();

			//	Paint the tabs on top of the background

			this.mBackBufferGraphics.DrawImage(this.mTabBuffer, new Point(0, 0));
			this.mBackBufferGraphics.Flush();

			//	Now paint this to the screen
			//	We want to paint the whole tabstrip and border every time
			//	so that the hot areas update correctly.
			try
			{
				screenGraphics.DrawImageUnscaled(this.mBackBuffer, 0, 0);
			}
			catch (AccessViolationException ex)
			{
				// http://stackoverflow.com/questions/5510115/randomly-occuring-accessviolationexception-in-gdi
				// sometimes it throws access voilation exception so it is try catched
				Logger.Info(ex.ToString());
			}
		}

		protected void PaintTransparentBackground(Graphics graphics, Rectangle clipRect)
		{
			if ((this.Parent != null))
			{
				//	Set the cliprect to be relative to the parent
				clipRect.Offset(this.Location);

				//	Save the current state before we do anything.
				GraphicsState state = graphics.Save();

				//	Set the graphicsobject to be relative to the parent
				graphics.TranslateTransform((float)-this.Location.X, (float)-this.Location.Y);
				graphics.SmoothingMode = SmoothingMode.HighSpeed;

				//	Paint the parent
				PaintEventArgs e = new PaintEventArgs(graphics, clipRect);
				try
				{
					this.InvokePaintBackground(this.Parent, e);
					this.InvokePaint(this.Parent, e);
				}
				finally
				{
					//	Restore the graphics state and the clipRect to their original locations
					graphics.Restore(state);
					clipRect.Offset(-this.Location.X, -this.Location.Y);
				}
			}
		}

		private void DrawTabPage(int index, Graphics graphics)
		{
			if (index < 0)
			{
				return;
			}
			graphics.SmoothingMode = SmoothingMode.HighSpeed;

			using (GraphicsPath tabPageBorderPath = this.GetTabPageBorder(index, graphics))
			{
				this.mStyleProvider.PaintTab(index, graphics);
				this.DrawTabImage(index, graphics);
				this.DrawTabText(index, graphics);
			}
		}

		private void DrawTabBorder(GraphicsPath path, int index, Graphics graphics)
		{
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			Color borderColor = Color.Transparent;

			using (Pen borderPen = new Pen(borderColor))
			{
				graphics.DrawPath(borderPen, path);
			}
		}

		private void DrawTabText(int index, Graphics graphics)
		{
			graphics.SmoothingMode = SmoothingMode.HighQuality;
			StringFormat stringFormat = GetStringFormat();
			graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
			Rectangle tabBounds = this.GetTabTextRect(index, graphics);

			Rectangle textRect = new Rectangle(tabBounds.Left, tabBounds.Top,
					tabBounds.Width, tabBounds.Height);
			textRect.X += 5;
			textRect.Width -= 5;

			if (index == 0)
			{
				int buttonWidth = ControlBar.GetButtonWidth(GameManager.sControlBarHeight);
				textRect.X += buttonWidth;
				textRect.Width -= buttonWidth;
			}

			if (this.SelectedIndex == index)
			{
				// Active tab text
				using (Brush textBrush = new SolidBrush(GMColors.SelectedTabTextColor))
				{
					graphics.DrawString(this.TabPages[index].Text, this.Font, textBrush, textRect,
							stringFormat);
				}
			}
			else
			{
				string parentStyleTheme = "Em";
				using (RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
				{
					parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", parentStyleTheme);
				}
				if(parentStyleTheme == "Em")
					this.mStyleProvider = new TabStyleProviderDefault(this);
				else
					this.mStyleProvider = new TabStyleProviderNew(this);
				// Hover tab text
				if (index == this.ActiveIndex)
				{
					using (Brush textBrush = new SolidBrush(GMColors.MouseOverTabTextColor))
					{
						graphics.DrawString(this.TabPages[index].Text, this.Font, textBrush, textRect,
								stringFormat);
					}
				}
				else
				{
					// Inactive tab text
					using (Brush textBrush = new SolidBrush(GMColors.InactiveTabTextColor))
					{
						graphics.DrawString(this.TabPages[index].Text, this.Font, textBrush, textRect,
								stringFormat);
					}
				}
			}
		}

		private void DrawTabImage(int index, Graphics graphics)
		{
			Image tabImage = null;
			if (this.TabPages[index].ImageIndex > -1 && this.ImageList != null && this.ImageList.Images.Count > this.TabPages[index].ImageIndex)
			{
				tabImage = this.ImageList.Images[this.TabPages[index].ImageIndex];
			}
			else if ((!string.IsNullOrEmpty(this.TabPages[index].ImageKey) && !this.TabPages[index].ImageKey.Equals("(none)", StringComparison.OrdinalIgnoreCase))
					&& this.ImageList != null && this.ImageList.Images.ContainsKey(this.TabPages[index].ImageKey))
			{
				tabImage = this.ImageList.Images[this.TabPages[index].ImageKey];
			}

			if (tabImage != null)
			{
				Rectangle imageRect = this.GetTabImageRect(index, graphics);
				graphics.InterpolationMode = InterpolationMode.HighQualityBicubic;
				graphics.SmoothingMode  = SmoothingMode.HighQuality;
				graphics.CompositingQuality = CompositingQuality.HighQuality;
				graphics.PixelOffsetMode = PixelOffsetMode.HighQuality;  
				graphics.DrawImage(tabImage, imageRect);
			}
		}

		private StringFormat GetStringFormat()
		{
			StringFormat format = new StringFormat();

			format.Trimming = StringTrimming.EllipsisCharacter;
			format.Alignment = StringAlignment.Near;
			format.LineAlignment = StringAlignment.Center;
			format.HotkeyPrefix = System.Drawing.Text.HotkeyPrefix.Show;

			format.FormatFlags = format.FormatFlags | StringFormatFlags.NoWrap;
			return format;
		}

		private GraphicsPath GetTabPageBorder(int index, Graphics graphics)
		{

			GraphicsPath path = new GraphicsPath();
			Rectangle pageBounds = this.GetPageBounds(index);
			Rectangle tabBounds = this.mStyleProvider.GetTabRect(index);
			this.mStyleProvider.AddTabBorder(path, tabBounds, graphics, index);
			this.AddPageBorder(path, pageBounds, tabBounds, graphics);

			return path;
		}

		public Rectangle GetPageBounds(int index)
		{
			Rectangle pageBounds = this.TabPages[index].Bounds;
			pageBounds.Width += 1;
			pageBounds.Height += 1;
			pageBounds.X -= 1;
			pageBounds.Y -= 1;

			if (pageBounds.Bottom > this.Height - 4)
			{
				pageBounds.Height -= (pageBounds.Bottom - this.Height + 4);
			}
			return pageBounds;
		}

		private Rectangle GetTabTextRect(int index, Graphics graphics)
		{
			Rectangle textRect = new Rectangle();
			using (GraphicsPath path = this.mStyleProvider.GetTabBorder(index, graphics))
			{
				RectangleF tabBounds = path.GetBounds();

				textRect = new Rectangle((int)tabBounds.X, (int)tabBounds.Y, (int)tabBounds.Width, (int)tabBounds.Height);

				textRect.Y += 4;
				textRect.Height -= 6;

				//	If there is an image allow for it
				if (this.ImageList != null && (this.TabPages[index].ImageIndex > -1 
							|| (!string.IsNullOrEmpty(this.TabPages[index].ImageKey)
								&& !this.TabPages[index].ImageKey.Equals("(none)", StringComparison.OrdinalIgnoreCase))))
				{
					Rectangle imageRect = this.GetTabImageRect(index, graphics);
					textRect.X = imageRect.Right + 4;
					textRect.Width -= (textRect.Right - (int)tabBounds.Right);

					Rectangle closerRect = this.GetTabCloserRect(index, graphics);
					textRect.Width -= ((int)tabBounds.Right - closerRect.X + 4);
				}
				else
				{
					Rectangle closerRect = this.GetTabCloserRect(index, graphics);
					if (index != 0)
					{
						textRect.Width -= ((int)tabBounds.Right - closerRect.X + 4);
					}
				}


				while (!path.IsVisible(textRect.Right, textRect.Y) && textRect.Width > 0)
				{
					textRect.Width -= 1;
				}
				while (!path.IsVisible(textRect.X, textRect.Y) && textRect.Width > 0)
				{
					textRect.X += 1;
					textRect.Width -= 1;
				}
			}
			return textRect;
		}

		public int GetTabRow(int index)
		{
			Rectangle rect = this.GetTabRect(index);

			int row = (rect.Y - 2)/rect.Height;

			return row;
		}

		public Point GetTabPosition(int index)
		{
			return new Point(0, index);
		}

		public virtual int GetConstantTabWidth()
		{
			return this.mStyleProvider.GetConstantTabWidth();
		}

		public bool IsFirstTabInRow(int index)
		{
			if (index == 0)
				return true;
			else
				return false;
		}

		private void AddPageBorder(GraphicsPath path, Rectangle pageBounds, Rectangle tabBounds, Graphics graphics)
		{
			//outer border
			path.AddLine(tabBounds.Right, pageBounds.Y, pageBounds.Right, pageBounds.Y); //top-right
			path.AddLine(pageBounds.Right, pageBounds.Y, pageBounds.Right, pageBounds.Bottom); //right
			path.AddLine(pageBounds.Right, pageBounds.Bottom, pageBounds.X, pageBounds.Bottom); //bottom
			path.AddLine(pageBounds.X, pageBounds.Bottom, pageBounds.X, pageBounds.Y);	//left
			path.AddLine(pageBounds.X, pageBounds.Y, tabBounds.X, pageBounds.Y);	// top-left
		}

		private Rectangle GetTabImageRect(int index, Graphics graphics)
		{
			using (GraphicsPath tabBorderPath = this.mStyleProvider.GetTabBorder(index, graphics))
			{
				return this.GetTabImageRect(tabBorderPath);
			}
		}

		private Rectangle GetTabImageRect(GraphicsPath tabBorderPath)
		{
			Rectangle imageRect = new Rectangle();
			RectangleF rect = tabBorderPath.GetBounds();

			rect.X += 4;
			rect.Y += 3;
			rect.Height += 3;
			rect.Width += 20;

			imageRect = new Rectangle(1, (int)rect.Y + 2, (int)rect.Height-11, (int)rect.Height-11);
			while (!tabBorderPath.IsVisible(imageRect.X, imageRect.Y))
			{
				imageRect.X += 1;	
			}
			imageRect.X += 4;

			return imageRect;
		}

		public Rectangle GetTabCloserRect(int index, Graphics graphics)
		{
			Rectangle closerRect = new Rectangle();
			using (GraphicsPath path = this.mStyleProvider.GetTabBorder(index, graphics))
			{
				RectangleF rect = path.GetBounds();

				rect.Y += 1;
				rect.Height -= 2;

				closerRect = new Rectangle((int)rect.Right, (int)rect.Y + (int)Math.Floor((double)((int)rect.Height - 6)/2), 6, 6);
				while (!path.IsVisible(closerRect.Right, closerRect.Y) && closerRect.Right > -6)
				{
					closerRect.X -= 1;	
				}
				closerRect.X -= 4;

				//rect values had been modified above, getting back original rect dimensions and using it to give the closerRect position, this closerRect will adjust automatically on the basis of screen resolution now
				rect = path.GetBounds();
				closerRect.Height = (int)rect.Height;
				closerRect.Width = ((int)closerRect.Height)*29/40;
				closerRect.X = (int)rect.Width + (int)rect.X - closerRect.Width;
				closerRect.Y = ((int)rect.Height  - closerRect.Height) / 2 + (int)rect.Y ; 
			}


			return closerRect;
		}

		public new Point MousePosition
		{
			get
			{
				Point loc = this.PointToClient(Control.MousePosition);
				if (this.RightToLeftLayout)
				{
					loc.X = (this.Width - loc.X);
				}			
				return loc;
			}
		}
	}
}
