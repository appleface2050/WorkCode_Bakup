using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{
	public class TabStyleProviderNew : TabStyleProvider
	{
		public TabStyleProviderNew(CustomTabControl tabControl): base(tabControl)
		{
			this._TabControl = tabControl;
			this.Padding = new Point(6,3);
			this._Radius = 10;
		}

		public override void AddTabBorder(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Rectangle tabBounds, Graphics graphics, int index)
		{
			int leftControlBarWidth = GameManager.sGameManager.mControlBarLeft.Width;
			int BUTTON_WIDTH		= 67;
			int BUTTON_HEIGHT		= 46;
			int shiftTabPosition		= (int)(GameManager.sControlBarHeight * ((float)BUTTON_WIDTH)/BUTTON_HEIGHT);
			switch (this._TabControl.Alignment) {
				case TabAlignment.Top:
					Pen myPen;
					myPen = new Pen(GMColors.TabBorderColor);
					if(graphics != null)
					{
						int endOfTabs = this._TabControl.GetTabRect(this._TabControl.TabCount -1).Right - 1;
						graphics.DrawLine(myPen, endOfTabs, tabBounds.Bottom, this._TabControl.Width + GameManager.sGameManager.mControlBarRight.Width, tabBounds.Bottom);
					}
					myPen = new Pen(GMColors.TabBorderColor);
					int offset = 2;
					if(index == 1)
						offset = 0;
					path.AddLine(tabBounds.X + offset, tabBounds.Bottom, tabBounds.X + offset, tabBounds.Y + this._Radius);
					//if(index != 1)
					//{
					if(graphics != null)
						graphics.DrawLine(myPen, tabBounds.X + offset, tabBounds.Bottom, tabBounds.X + offset, tabBounds.Y + this._Radius);
					//}
					path.AddArc(tabBounds.X + offset, tabBounds.Y, this._Radius * 2, this._Radius * 2, 180, 90);
					if(graphics != null)
						graphics.DrawArc(myPen, tabBounds.X + offset, tabBounds.Y, this._Radius * 2, this._Radius * 2, 180, 90);
					path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
					if(graphics != null)
					{
						if(index == 0)
							graphics.DrawLine(myPen, leftControlBarWidth + this._Radius, tabBounds.Y, tabBounds.Right - this._Radius, tabBounds.Y);
						else
							graphics.DrawLine(myPen, tabBounds.X + this._Radius, tabBounds.Y, tabBounds.Right - this._Radius, tabBounds.Y);
					}
					offset = 3;
					//if(index == 0)
					//offset = 2;
					path.AddArc(tabBounds.Right - this._Radius * 2 - offset, tabBounds.Y, this._Radius * 2, this._Radius * 2, 270, 90);
					if(graphics != null)
						graphics.DrawArc(myPen, tabBounds.Right - this._Radius * 2 - offset, tabBounds.Y, this._Radius * 2, this._Radius * 2, 270, 90);

					path.AddLine(tabBounds.Right - offset, tabBounds.Y + this._Radius, tabBounds.Right - offset, tabBounds.Bottom);
					if(graphics != null)
						graphics.DrawLine(myPen, tabBounds.Right - offset, tabBounds.Y + this._Radius, tabBounds.Right - offset, tabBounds.Bottom);
					if(index == 1)
						offset = 2;

					if(graphics != null)
					{
						if (this._TabControl.SelectedIndex == index)
						{
							myPen = new Pen(GMColors.selectedTabGradientColor2, 2);
							graphics.DrawLine(myPen, tabBounds.X + offset - 1, tabBounds.Bottom, tabBounds.Right - 4, tabBounds.Bottom);
							myPen = new Pen(GMColors.TabBorderColor);
						}
						else
						{
							graphics.DrawLine(myPen, tabBounds.X + offset - 1, tabBounds.Bottom, tabBounds.Right, tabBounds.Bottom);

							myPen = new Pen(GMColors.TabBorderColor);
							graphics.DrawLine(myPen, tabBounds.X + this._Radius, tabBounds.Y, tabBounds.Right - this._Radius, tabBounds.Y);
						}

						graphics.DrawLine(myPen, tabBounds.Right -4, tabBounds.Bottom, tabBounds.Right + 4, tabBounds.Bottom);
					}

					myPen = new Pen(GMColors.TabBorderColor);

					int numberOfIntervals = 1; //or change to whatever you want.
					var interval_R = (GMColors.TabBarGradientBottom.R - GMColors.TabBarGradientTop.R) / numberOfIntervals;
					var interval_G = (GMColors.TabBarGradientBottom.G - GMColors.TabBarGradientTop.G) / numberOfIntervals;
					var interval_B = (GMColors.TabBarGradientBottom.B - GMColors.TabBarGradientTop.B) / numberOfIntervals;

					var current_R = GMColors.TabBarGradientTop.R;
					var current_G = GMColors.TabBarGradientTop.G;
					var current_B = GMColors.TabBarGradientTop.B;

					for (var x = 0; x <= numberOfIntervals; x++)
					{
						var color = Color.FromArgb(current_R, current_G, current_B);
						//do something with color.

						//increment.
						current_R += (byte)interval_R;
						current_G += (byte)interval_G;
						current_B += (byte)interval_B;
					}

					//Draw region on 1st Tab
					if(index == 0 && graphics != null)
					{
						Rectangle rect = new Rectangle(shiftTabPosition, tabBounds.Y, this._Radius, this._Radius);
						using (Region region = new Region(rect))
						{
							using (GraphicsPath pathPie = new GraphicsPath())
							{
								pathPie.AddPie(shiftTabPosition, tabBounds.Y,(float) (this._Radius * 2), (float) (this._Radius * 2), 180, 90);
								region.Exclude(pathPie);
								Color customColor = Color.FromArgb(current_R, current_G, current_B);
								graphics.FillRegion(new SolidBrush(GMColors.TabBarGradientTop), region);

								graphics.DrawLine(myPen, 0, tabBounds.Bottom, leftControlBarWidth, tabBounds.Bottom);
								graphics.DrawLine(myPen, leftControlBarWidth , tabBounds.Bottom, leftControlBarWidth, tabBounds.Y + this._Radius - 1);
								graphics.DrawArc(myPen, leftControlBarWidth, tabBounds.Y, this._Radius * 2, this._Radius * 2, 180, 90);
								graphics.DrawLine(myPen, leftControlBarWidth + this._Radius, tabBounds.Y, tabBounds.Right - this._Radius, tabBounds.Y);
							}
						}
					}

					if(graphics != null)
					{
						graphics.DrawLine(myPen, 0, tabBounds.Bottom, leftControlBarWidth, tabBounds.Bottom);
						graphics.DrawLine(myPen, leftControlBarWidth, tabBounds.Bottom, leftControlBarWidth, tabBounds.Y + this._Radius - 1);
						//graphics.DrawArc(myPen, i, tabBounds.Y + this._Radius * 2, i + this._Radius * 2, tabBounds.Y, 180, 90);
						//graphics.DrawArc(myPen, i, tabBounds.Y, this._Radius * 2, this._Radius * 2, 180, 90);

					}

					myPen.Dispose();
					myPen = null;
					//path.AddLine(i , tabBounds.Bottom, i, tabBounds.Y + this._Radius);

					break;
			}
		}

		protected override Brush GetTabBackgroundBrush(int index)
		{
			Rectangle tabBounds = this.GetTabRect(index);
			LinearGradientBrush fillBrush = null;

			Color gradient1 = GMColors.inactiveTabGradientColor1;
			Color gradient2 = GMColors.inactiveTabGradientColor2;
			//Color gradient3 = GMColors.inactiveTabGradientColorDarkTheme3;
			//Color gradient4 = GMColors.inactiveTabGradientColorDarkTheme4;
			//Color gradient5 = GMColors.inactiveTabGradientColorDarkTheme5;

			//float position1 = GMColors.inactiveTabGradientPosition1;
			//float position2 = GMColors.inactiveTabGradientPosition2;
			//float position3 = GMColors.inactiveTabGradientPosition3;
			//float position4 = GMColors.inactiveTabGradientPosition4;
			//float position5 = GMColors.inactiveTabGradientPosition5;

			if (this._TabControl.SelectedIndex == index)		// selected, or currently active
			{
				gradient1 = GMColors.selectedTabGradientColor1;
				gradient2 = GMColors.selectedTabGradientColor2;
				//gradient3 = GMColors.selectedTabGradientColorDarkTheme3;
				//gradient4 = GMColors.selectedTabGradientColorDarkTheme4;
				//gradient5 = GMColors.selectedTabGradientColorDarkTheme5;

				//position1 = GMColors.selectedTabGradientPosition1;
				//position2 = GMColors.selectedTabGradientPosition2;
				//position3 = GMColors.selectedTabGradientPosition3;
				//position4 = GMColors.selectedTabGradientPosition4;
				//position5 = GMColors.selectedTabGradientPosition5;
			}
			else if (!this._TabControl.TabPages[index].Enabled)		// disabled
			{
				gradient1 = GMColors.disabledTabGradientColor1;
				gradient2 = GMColors.disabledTabGradientColor2;
				//gradient3 = GMColors.disabledTabGradientColor3;
				//gradient4 = GMColors.disabledTabGradientColor4;
				//gradient5 = GMColors.disabledTabGradientColor5;

				//position1 = GMColors.disabledTabGradientPosition1;
				//position2 = GMColors.disabledTabGradientPosition2;
				//position3 = GMColors.disabledTabGradientPosition3;
				//position4 = GMColors.disabledTabGradientPosition4;
				//position5 = GMColors.disabledTabGradientPosition5;
			}
			else if (index == this._TabControl.ActiveIndex)		// mouse over
			{
				gradient1 = GMColors.tabMouseOverGradientColor1;
				gradient2 = GMColors.tabMouseOverGradientColor2;
				//gradient3 = GMColors.tabMouseOverGradientColor3;
				//gradient4 = GMColors.tabMouseOverGradientColor4;
				//gradient5 = GMColors.tabMouseOverGradientColor5;

				//position1 = GMColors.tabMouseOverGradientPosition1;
				//position2 = GMColors.tabMouseOverGradientPosition2;
				//position3 = GMColors.tabMouseOverGradientPosition3;
				//position4 = GMColors.tabMouseOverGradientPosition4;
				//position5 = GMColors.tabMouseOverGradientPosition5;
			}
			else		// inactive
			{
				// use the initialized colors.
			}

			fillBrush = new LinearGradientBrush(tabBounds, gradient1, gradient2, LinearGradientMode.Vertical);

			//ColorBlend blend = new ColorBlend();
			//blend.Positions = new float[] {position1, position2, position3, position4, position5};
			//blend.Colors = new Color[] {gradient1, gradient2, gradient3, gradient4, gradient5};
			//fillBrush.InterpolationColors = blend;

			return fillBrush;
		}
	}
}
