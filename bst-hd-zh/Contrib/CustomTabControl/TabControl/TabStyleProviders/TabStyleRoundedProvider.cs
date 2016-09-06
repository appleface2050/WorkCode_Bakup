/*
 * This code is provided under the Code Project Open Licence (CPOL)
 * See http://www.codeproject.com/info/cpol10.aspx for details
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace System.Windows.Forms
{

	[System.ComponentModel.ToolboxItem(false)]
		public class TabStyleRoundedProvider : TabStyleProvider
	{
		private Image	mTabClose = Image.FromFile("Assets\\tab_close.png");
		private Image	mTabCloseClick = Image.FromFile("Assets\\tab_close_click.png");
		private Image	mTabCloseHover = Image.FromFile("Assets\\tab_close_hover.png");

		public TabStyleRoundedProvider(CustomTabControl tabControl) : base(tabControl)
		{
			this._Overlap = 0;
			this._Radius = 10;
			//	Must set after the _Radius as this is used in the calculations of the actual padding
			this.Padding = new Point(6, 3);
		}

		public override void AddTabBorder(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Rectangle tabBounds)
		{

			switch (this._TabControl.Alignment)
			{
				case TabAlignment.Top:
					path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
					//	path.AddArc(tabBounds.X, tabBounds.Y, this._Radius * 2, this._Radius * 2, 180, 90);
					path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
					//	path.AddArc(tabBounds.Right - this._Radius * 2, tabBounds.Y, this._Radius * 2, this._Radius * 2, 270, 90);
					path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);
					break;
				case TabAlignment.Bottom:
					path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom - this._Radius);
					path.AddArc(tabBounds.Right - this._Radius * 2, tabBounds.Bottom - this._Radius * 2, this._Radius * 2, this._Radius * 2, 0, 90);
					path.AddLine(tabBounds.Right - this._Radius, tabBounds.Bottom, tabBounds.X + this._Radius, tabBounds.Bottom);
					path.AddArc(tabBounds.X, tabBounds.Bottom - this._Radius * 2, this._Radius * 2, this._Radius * 2, 90, 90);
					path.AddLine(tabBounds.X, tabBounds.Bottom - this._Radius, tabBounds.X, tabBounds.Y);
					break;
				case TabAlignment.Left:
					path.AddLine(tabBounds.Right, tabBounds.Bottom, tabBounds.X + this._Radius, tabBounds.Bottom);
					path.AddArc(tabBounds.X, tabBounds.Bottom - this._Radius * 2, this._Radius * 2, this._Radius * 2, 90, 90);
					path.AddLine(tabBounds.X, tabBounds.Bottom - this._Radius, tabBounds.X, tabBounds.Y + this._Radius);
					path.AddArc(tabBounds.X, tabBounds.Y, this._Radius * 2, this._Radius * 2, 180, 90);
					path.AddLine(tabBounds.X + this._Radius, tabBounds.Y, tabBounds.Right, tabBounds.Y);
					break;
				case TabAlignment.Right:
					path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right - this._Radius, tabBounds.Y);
					path.AddArc(tabBounds.Right - this._Radius * 2, tabBounds.Y, this._Radius * 2, this._Radius * 2, 270, 90);
					path.AddLine(tabBounds.Right, tabBounds.Y + this._Radius, tabBounds.Right, tabBounds.Bottom - this._Radius);
					path.AddArc(tabBounds.Right - this._Radius * 2, tabBounds.Bottom - this._Radius * 2, this._Radius * 2, this._Radius * 2, 0, 90);
					path.AddLine(tabBounds.Right - this._Radius, tabBounds.Bottom, tabBounds.X, tabBounds.Bottom);
					break;
			}
		}

		protected override void DrawTabCloser(int index, Graphics graphics)
		{
			if (index == 0)
			{
				return;
			}

			if (this._ShowTabCloser)
			{
				Rectangle closerRect = this._TabControl.GetTabCloserRect(index);
				//Rectangle closerRect = GetTabCloserRectFinal(index);
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				int yPadding = closerRect.Height/5;
				closerRect.X -= 5;
				closerRect.Y += yPadding;
				closerRect.Height -= 2*yPadding;
				closerRect.Width = closerRect.Height;

				if (closerRect.Contains(this._TabControl.MousePosition))
				{
					if (CustomTabControl.MouseButtons == MouseButtons.Left)
						graphics.DrawImage(mTabCloseClick, closerRect);
					else
						graphics.DrawImage(mTabCloseHover, closerRect);

					//commenting, as now we are using images
					/*
					   using (GraphicsPath closerPath = GetCloserButtonPath(closerRect))
					   {
					   using (SolidBrush closerBrush = new SolidBrush(Color.FromArgb(193, 53, 53)))
					   {
					   graphics.FillPath(closerBrush, closerPath);
					   }
					   }
					   using (GraphicsPath closerPath = GetCloserPath(closerRect))
					   {
					   using (Pen closerPen = new Pen(this._CloserColorActive))
					   {
					   graphics.DrawPath(closerPen, closerPath);
					   }
					   }*/
				}
				else
				{
					graphics.DrawImage(mTabClose, closerRect);

					//commenting as now we are using images
					/*
					   using (GraphicsPath closerPath = GetCloserPath(closerRect))
					   {
					   using (Pen closerPen = new Pen(Color.Black, 1))
					   {
					   graphics.DrawPath(closerPen, closerPath);
					   }
					   }*/
				}
			}
		}

		private Rectangle GetTabCloserRectFinal(int index)
		{
			Rectangle closerRect = this._TabControl.GetTabCloserRect(index);
			closerRect.X -= 7;
			closerRect.Y -= 5;

			closerRect.Width += 5;
			closerRect.Height += 5;

			return closerRect;
		}

		private static GraphicsPath GetCloserButtonPath(Rectangle closerRect){
			GraphicsPath closerPath = new GraphicsPath();
			closerPath.AddRectangle(new Rectangle(closerRect.X - 3, closerRect.Y - 3, closerRect.Width + 6, closerRect.Height + 6));
			closerPath.CloseFigure();
			return closerPath;
		}


		protected override Brush GetTabBackgroundBrush(int index)
		{
			Rectangle tabBounds = this.GetTabRect(index);
			LinearGradientBrush fillBrush = null;

			Color gradient1 = GMColors.inactiveTabGradientColor1;
			Color gradient2 = GMColors.inactiveTabGradientColor2;
			Color gradient3 = GMColors.inactiveTabGradientColor3;
			Color gradient4 = GMColors.inactiveTabGradientColor4;
			Color gradient5 = GMColors.inactiveTabGradientColor5;

			float position1 = GMColors.inactiveTabGradientPosition1;
			float position2 = GMColors.inactiveTabGradientPosition2;
			float position3 = GMColors.inactiveTabGradientPosition3;
			float position4 = GMColors.inactiveTabGradientPosition4;
			float position5 = GMColors.inactiveTabGradientPosition5;

			if (this._TabControl.SelectedIndex == index)		// selected, or currently active
			{
				gradient1 = GMColors.selectedTabGradientColor1;
				gradient2 = GMColors.selectedTabGradientColor2;
				gradient3 = GMColors.selectedTabGradientColor3;
				gradient4 = GMColors.selectedTabGradientColor4;
				gradient5 = GMColors.selectedTabGradientColor5;

				position1 = GMColors.selectedTabGradientPosition1;
				position2 = GMColors.selectedTabGradientPosition2;
				position3 = GMColors.selectedTabGradientPosition3;
				position4 = GMColors.selectedTabGradientPosition4;
				position5 = GMColors.selectedTabGradientPosition5;
			}
			else if (!this._TabControl.TabPages[index].Enabled)		// enabled
			{
				gradient1 = GMColors.disabledTabGradientColor1;
				gradient2 = GMColors.disabledTabGradientColor2;
				gradient3 = GMColors.disabledTabGradientColor3;
				gradient4 = GMColors.disabledTabGradientColor4;
				gradient5 = GMColors.disabledTabGradientColor5;

				position1 = GMColors.disabledTabGradientPosition1;
				position2 = GMColors.disabledTabGradientPosition2;
				position3 = GMColors.disabledTabGradientPosition3;
				position4 = GMColors.disabledTabGradientPosition4;
				position5 = GMColors.disabledTabGradientPosition5;
			}
			else if (this.HotTrack && index == this._TabControl.ActiveIndex)		// mouse over
			{
				gradient1 = GMColors.tabMouseOverGradientColor1;
				gradient2 = GMColors.tabMouseOverGradientColor2;
				gradient3 = GMColors.tabMouseOverGradientColor3;
				gradient4 = GMColors.tabMouseOverGradientColor4;
				gradient5 = GMColors.tabMouseOverGradientColor5;

				position1 = GMColors.tabMouseOverGradientPosition1;
				position2 = GMColors.tabMouseOverGradientPosition2;
				position3 = GMColors.tabMouseOverGradientPosition3;
				position4 = GMColors.tabMouseOverGradientPosition4;
				position5 = GMColors.tabMouseOverGradientPosition5;
			}
			else		// inactive
			{
				// use the initialized colors.
			}

			//	Get the correctly aligned gradient
			switch (this._TabControl.Alignment)
			{
				case TabAlignment.Top:
					fillBrush = new LinearGradientBrush(tabBounds, gradient1, gradient5, LinearGradientMode.Vertical);
					break;
				case TabAlignment.Bottom:
					fillBrush = new LinearGradientBrush(tabBounds, gradient1, gradient5, LinearGradientMode.Vertical);
					break;
				case TabAlignment.Left:
					fillBrush = new LinearGradientBrush(tabBounds, gradient5, gradient1, LinearGradientMode.Horizontal);
					break;
				case TabAlignment.Right:
					fillBrush = new LinearGradientBrush(tabBounds, gradient1, gradient5, LinearGradientMode.Horizontal);
					break;
			}

			ColorBlend blend = new ColorBlend();
			blend.Positions = new[] {position1, position2, position3, position4, position5};
			blend.Colors = new[] {gradient1, gradient2, gradient3, gradient4, gradient5};
			fillBrush.InterpolationColors = blend;

			//	Add the blend
			//fillBrush.Blend = this.GetBackgroundBlend(index);

			return fillBrush;
		}
	}
}
