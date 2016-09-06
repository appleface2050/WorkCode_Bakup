using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Windows.Forms;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{
	public abstract class TabStyleProvider : Component
	{
		protected Image	mTabClose;
		protected Image	mTabCloseClick;
		protected Image	mTabCloseHover;

		public TabStyleProvider(CustomTabControl tabControl)
		{
			mTabClose = Image.FromFile(Path.Combine(GameManager.sAssetsDir, "tab_close.png"));
			mTabCloseClick = Image.FromFile(Path.Combine(GameManager.sAssetsDir, "tab_close_click.png"));
			mTabCloseHover = Image.FromFile(Path.Combine(GameManager.sAssetsDir, "tab_close_hover.png"));

			this._TabControl = tabControl;
			this.Padding = new Point(6,3);
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
			protected CustomTabControl _TabControl;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
			protected Point _Padding;

		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1051:DoNotDeclareVisibleInstanceFields")]
			protected int _Radius = 1;

		public abstract void AddTabBorder(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Rectangle tabBounds, Graphics graphics, int index);
		//{
		//	path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
		//	path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
		//	path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);
		//};

		public virtual Rectangle GetTabRect(int index)
		{
			if (index < 0)
			{
				return new Rectangle();
			}

			Rectangle tabBounds = this._TabControl.GetTabRect(index);
			bool firstTabinRow = this._TabControl.IsFirstTabInRow(index);
			tabBounds.Height -= 1 + GameManager.mTabBarExtraHeight;

			if (firstTabinRow)
			{
				// shifting the home tab header to right to have space for back button
				tabBounds.Width -= 2;
				//nitin -- all other tabs are shifted by 1 pixel to the left to give spacing between tabs. 
				// so subracting 2 pixel 1 for the shift of adjacent tab to left and 1 for the spacing between them
			}

			//	Adjust first tab in the row to align with tabpage
			this.EnsureFirstTabIsInView(ref tabBounds, index);
			tabBounds.Y = 0 + GameManager.mTabBarExtraHeight;//nitin - leaving zero pixel space from top
			return tabBounds;
		}

		public virtual int GetConstantTabWidth()
		{
			Rectangle rect = GetTabRect(0);
			return rect.Width;
		}


		[System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1045:DoNotPassTypesByReference", MessageId = "0#")]
			protected virtual void EnsureFirstTabIsInView(ref Rectangle tabBounds, int index)
			{

				if (index < 0)
				{
					return;
				}
				//	Adjust first tab in the row to align with tabpage
				//	Make sure we only reposition visible tabs, as we may have scrolled out of view.

				bool firstTabinRow = this._TabControl.IsFirstTabInRow(index);

				if (firstTabinRow && tabBounds.Right > 0)
				{
					int tabPageX = this._TabControl.GetPageBounds(index).X;
					if (tabBounds.X < tabPageX)
					{
						tabBounds.Width -= (tabPageX - tabBounds.X);
						tabBounds.X = tabPageX;
					}
				}
			}

		protected virtual Brush GetTabBackgroundBrush(int index)
		{
			Rectangle tabBounds = this.GetTabRect(index);
			LinearGradientBrush fillBrush = null;

			Color gradient1 = GMColors.inactiveTabGradientColor1;
			Color gradient2 = GMColors.inactiveTabGradientColor2;
			/*
			Color gradient3 = GMColors.inactiveTabGradientColor3;
			Color gradient4 = GMColors.inactiveTabGradientColor4;
			Color gradient5 = GMColors.inactiveTabGradientColor5;

			float position1 = GMColors.inactiveTabGradientPosition1;
			float position2 = GMColors.inactiveTabGradientPosition2;
			float position3 = GMColors.inactiveTabGradientPosition3;
			float position4 = GMColors.inactiveTabGradientPosition4;
			float position5 = GMColors.inactiveTabGradientPosition5;
			*/

			if (this._TabControl.SelectedIndex == index)		// selected, or currently active
			{
				gradient1 = GMColors.selectedTabGradientColor1;
				gradient2 = GMColors.selectedTabGradientColor2;
				/*
				gradient3 = GMColors.selectedTabGradientColor3;
				gradient4 = GMColors.selectedTabGradientColor4;
				gradient5 = GMColors.selectedTabGradientColor5;

				position1 = GMColors.selectedTabGradientPosition1;
				position2 = GMColors.selectedTabGradientPosition2;
				position3 = GMColors.selectedTabGradientPosition3;
				position4 = GMColors.selectedTabGradientPosition4;
				position5 = GMColors.selectedTabGradientPosition5;
				*/
			}
			else if (!this._TabControl.TabPages[index].Enabled)		// disabled
			{
				gradient1 = GMColors.disabledTabGradientColor1;
				gradient2 = GMColors.disabledTabGradientColor2;
				/*
				gradient3 = GMColors.disabledTabGradientColor3;
				gradient4 = GMColors.disabledTabGradientColor4;
				gradient5 = GMColors.disabledTabGradientColor5;

				position1 = GMColors.disabledTabGradientPosition1;
				position2 = GMColors.disabledTabGradientPosition2;
				position3 = GMColors.disabledTabGradientPosition3;
				position4 = GMColors.disabledTabGradientPosition4;
				position5 = GMColors.disabledTabGradientPosition5;
				*/
			}
			else if (index == this._TabControl.ActiveIndex)		// mouse over
			{
				gradient1 = GMColors.tabMouseOverGradientColor1;
				gradient2 = GMColors.tabMouseOverGradientColor2;
				/*
				gradient3 = GMColors.tabMouseOverGradientColor3;
				gradient4 = GMColors.tabMouseOverGradientColor4;
				gradient5 = GMColors.tabMouseOverGradientColor5;

				position1 = GMColors.tabMouseOverGradientPosition1;
				position2 = GMColors.tabMouseOverGradientPosition2;
				position3 = GMColors.tabMouseOverGradientPosition3;
				position4 = GMColors.tabMouseOverGradientPosition4;
				position5 = GMColors.tabMouseOverGradientPosition5;
				*/
			}
			else		// inactive
			{
				// use the initialized colors.
			}

			fillBrush = new LinearGradientBrush(tabBounds, gradient1, gradient2, LinearGradientMode.Vertical);
			/*
			fillBrush = new LinearGradientBrush(tabBounds, gradient1, gradient5, LinearGradientMode.Vertical);

			ColorBlend blend = new ColorBlend();
			blend.Positions = new float[] {position1, position2, position3, position4, position5};
			blend.Colors = new Color[] {gradient1, gradient2, gradient3, gradient4, gradient5};
			fillBrush.InterpolationColors = blend;
			*/

			return fillBrush;
		}

		[Category("Appearance")]
			public Point Padding
			{
				get
				{
					return this._Padding;
				}
				set
				{
					this._Padding = value;
					//	This line will trigger the handle to recreate, therefore invalidating the control
					if (value.X + (int)(this._Radius/2) < -6)
					{
						((TabControl)this._TabControl).Padding = new Point(0, value.Y);
					} else {
						((TabControl)this._TabControl).Padding = new Point(value.X + (int)(this._Radius/2) + 6, value.Y);
					}
				}
			}


		public int Radius
		{
			get { return this._Radius; }
		}

		public void PaintTab(int index, Graphics graphics)
		{
			using (GraphicsPath tabpath = this.GetTabBorder(index, graphics))
			{
				using (Brush fillBrush = this.GetTabBackgroundBrush(index))
				{
					graphics.FillPath(fillBrush, tabpath);
					this.DrawTabCloser(index, graphics);
				}
			}
		}

		protected void DrawTabCloser(int index, Graphics graphics)
		{
			if (index == 0 || index == 1)
			{
				return;
			}

			Rectangle closerRect = this._TabControl.GetTabCloserRect(index, graphics);
			//Rectangle closerRect = GetTabCloserRectFinal(index);
			graphics.SmoothingMode = SmoothingMode.AntiAlias;
			int yPadding = closerRect.Height / 5;
			closerRect.Y += yPadding;
			closerRect.Height -= 2 * yPadding;
			closerRect.Width = closerRect.Height;

			if (closerRect.Contains(this._TabControl.MousePosition))
			{
				if (CustomTabControl.MouseButtons == MouseButtons.Left)
					graphics.DrawImage(mTabCloseClick, closerRect);
				else
					graphics.DrawImage(mTabCloseHover, closerRect);
			}
			else
			{
				graphics.DrawImage(mTabClose, closerRect);
			}
		}

		protected static GraphicsPath GetCloserPath(Rectangle closerRect)
		{
			GraphicsPath closerPath = new GraphicsPath();
			closerPath.AddLine(closerRect.X, closerRect.Y, closerRect.Right, closerRect.Bottom);
			closerPath.CloseFigure();
			closerPath.AddLine(closerRect.Right, closerRect.Y, closerRect.X, closerRect.Bottom);
			closerPath.CloseFigure();

			return closerPath;
		}

		private Blend GetBackgroundBlend()
		{
			float[] relativeIntensities = new float[]{0f, 0.5f, 1f, 1f};
			float[] relativePositions = new float[]{0f, 0.5f, 0.51f, 1f};

			Blend blend = new Blend();
			blend.Factors = relativeIntensities;
			blend.Positions = relativePositions;

			return blend;
		}

		public virtual Brush GetPageBackgroundBrush(int index)
		{
			Color light = Color.FromArgb(207, 207, 207);

			if (this._TabControl.SelectedIndex == index)
			{
				light = SystemColors.Window;
			}
			else if (index == this._TabControl.ActiveIndex)
			{
				light = Color.FromArgb(234, 246, 253);
			}

			return new SolidBrush(light);
		}

		public GraphicsPath GetTabBorder(int index, Graphics graphics)
		{
			GraphicsPath path = new GraphicsPath();
			Rectangle tabBounds = this.GetTabRect(index);
			this.AddTabBorder(path, tabBounds, graphics, index);
			return path;
		}
	}
}
