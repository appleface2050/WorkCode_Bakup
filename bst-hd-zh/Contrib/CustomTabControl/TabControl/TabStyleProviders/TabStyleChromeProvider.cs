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
	public class TabStyleChromeProvider : TabStyleProvider
	{
		public TabStyleChromeProvider(CustomTabControl tabControl) : base(tabControl){
			this._Overlap = 16;
			this._Radius = 16;
			this._ShowTabCloser = true;
			this._CloserColorActive = Color.White;
			
			//	Must set after the _Radius as this is used in the calculations of the actual padding
			this.Padding = new Point(7, 5);
		}
		
		public override void AddTabBorder(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Rectangle tabBounds){

			int spread;
			int eigth;
			int sixth;
			int quarter;

			if (this._TabControl.Alignment <= TabAlignment.Bottom){
				spread = (int)Math.Floor((decimal)tabBounds.Height * 2/3);
				eigth = (int)Math.Floor((decimal)tabBounds.Height * 1/8);
				sixth = (int)Math.Floor((decimal)tabBounds.Height * 1/6);
				quarter = (int)Math.Floor((decimal)tabBounds.Height * 1/4);
			} else {
				spread = (int)Math.Floor((decimal)tabBounds.Width * 2/3);
				eigth = (int)Math.Floor((decimal)tabBounds.Width * 1/8);
				sixth = (int)Math.Floor((decimal)tabBounds.Width * 1/6);
				quarter = (int)Math.Floor((decimal)tabBounds.Width * 1/4);
			}
			
			switch (this._TabControl.Alignment) {
				case TabAlignment.Top:
					
					path.AddCurve(new Point[] {  new Point(tabBounds.X, tabBounds.Bottom)
					              		,new Point(tabBounds.X + sixth, tabBounds.Bottom - eigth)
					              		,new Point(tabBounds.X + spread - quarter, tabBounds.Y + eigth)
					              		,new Point(tabBounds.X + spread, tabBounds.Y)});
//					path.AddLine(tabBounds.X + spread, tabBounds.Y, tabBounds.Right - spread, tabBounds.Y);
					path.AddCurve(new Point[] {  new Point(tabBounds.Right - spread, tabBounds.Y)
					              		,new Point(tabBounds.Right - spread + quarter, tabBounds.Y + eigth)
					              		,new Point(tabBounds.Right - sixth, tabBounds.Bottom - eigth)
					              		,new Point(tabBounds.Right, tabBounds.Bottom)});
					break;
				case TabAlignment.Bottom:
					path.AddCurve(new Point[] {  new Point(tabBounds.Right, tabBounds.Y)
					              		,new Point(tabBounds.Right - sixth, tabBounds.Y + eigth)
					              		,new Point(tabBounds.Right - spread + quarter, tabBounds.Bottom - eigth)
					              		,new Point(tabBounds.Right - spread, tabBounds.Bottom)});
//					path.AddLine(tabBounds.Right - spread, tabBounds.Bottom, tabBounds.X + spread, tabBounds.Bottom);
					path.AddCurve(new Point[] {  new Point(tabBounds.X + spread, tabBounds.Bottom)
					              		,new Point(tabBounds.X + spread - quarter, tabBounds.Bottom - eigth)
					              		,new Point(tabBounds.X + sixth, tabBounds.Y + eigth)
					              		,new Point(tabBounds.X, tabBounds.Y)});
					break;
				case TabAlignment.Left:
					path.AddCurve(new Point[] {  new Point(tabBounds.Right, tabBounds.Bottom)
					              		,new Point(tabBounds.Right - eigth, tabBounds.Bottom - sixth)
					              		,new Point(tabBounds.X + eigth, tabBounds.Bottom - spread + quarter)
					              		,new Point(tabBounds.X, tabBounds.Bottom - spread)});
//					path.AddLine(tabBounds.X, tabBounds.Bottom - spread, tabBounds.X ,tabBounds.Y + spread);
					path.AddCurve(new Point[] {  new Point(tabBounds.X, tabBounds.Y + spread)
					              		,new Point(tabBounds.X + eigth, tabBounds.Y + spread - quarter)
					              		,new Point(tabBounds.Right - eigth, tabBounds.Y + sixth)
					              		,new Point(tabBounds.Right, tabBounds.Y)});

					break;
				case TabAlignment.Right:
					path.AddCurve(new Point[] {  new Point(tabBounds.X, tabBounds.Y)
					              		,new Point(tabBounds.X + eigth, tabBounds.Y + sixth)
					              		,new Point(tabBounds.Right - eigth, tabBounds.Y + spread - quarter)
					              		,new Point(tabBounds.Right, tabBounds.Y + spread)});
//					path.AddLine(tabBounds.Right, tabBounds.Y + spread, tabBounds.Right, tabBounds.Bottom - spread);
					path.AddCurve(new Point[] {  new Point(tabBounds.Right, tabBounds.Bottom - spread)
					              		,new Point(tabBounds.Right - eigth, tabBounds.Bottom - spread + quarter)
					              		,new Point(tabBounds.X + eigth, tabBounds.Bottom - sixth)
					              		,new Point(tabBounds.X, tabBounds.Bottom)});
					break;
			}
		}

		protected override void DrawTabCloser(int index, Graphics graphics){
			if (this._ShowTabCloser){
				Rectangle closerRect = this._TabControl.GetTabCloserRect(index);
				//Rectangle closerRect = GetTabCloserRectFinal(index);
				graphics.SmoothingMode = SmoothingMode.AntiAlias;
				if (closerRect.Contains(this._TabControl.MousePosition)){
					using (GraphicsPath closerPath = GetCloserButtonPath(closerRect)){
						using (SolidBrush closerBrush = new SolidBrush(Color.FromArgb(193, 53, 53))){
							graphics.FillPath(closerBrush, closerPath);
						}
					}
					using (GraphicsPath closerPath = GetCloserPath(closerRect)){
						using (Pen closerPen = new Pen(this._CloserColorActive)){
							graphics.DrawPath(closerPen, closerPath);
						}
					}
				} else {
					using (GraphicsPath closerPath = GetCloserPath(closerRect)){
						using (Pen closerPen = new Pen(this._CloserColor, 1)){
							graphics.DrawPath(closerPen, closerPath);
						}
					}
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
			closerPath.AddEllipse(new Rectangle(closerRect.X - 2, closerRect.Y - 2, closerRect.Width + 4, closerRect.Height + 4));
			closerPath.CloseFigure();
			return closerPath;
		}

		protected override Brush GetTabBackgroundBrush(int index){
			LinearGradientBrush fillBrush = null;

			//	Capture the colours dependant on selection state of the tab
			Color dark = Color.FromArgb(10, 107, 220);
			Color light = Color.FromArgb(8, 92, 192);
			
			if (this._TabControl.SelectedIndex == index) {
				dark = Color.White;
				light = Color.White;
			} else if (!this._TabControl.TabPages[index].Enabled){
				light = dark;
			} else if (this.HotTrack && index == this._TabControl.ActiveIndex){
				//	Enable hot tracking
				light = SystemColors.Window;
				dark = Color.FromArgb(166, 203, 248);
			}
			
			//	Get the correctly aligned gradient
			Rectangle tabBounds = this.GetTabRect(index);
			tabBounds.Inflate(3,3);
			tabBounds.X -= 1;
			tabBounds.Y -= 1;
			switch (this._TabControl.Alignment) {
				case TabAlignment.Top:
					fillBrush = new LinearGradientBrush(tabBounds, dark, light, LinearGradientMode.Vertical);
					break;
				case TabAlignment.Bottom:
					fillBrush = new LinearGradientBrush(tabBounds, light, dark, LinearGradientMode.Vertical);
					break;
				case TabAlignment.Left:
					fillBrush = new LinearGradientBrush(tabBounds, dark, light, LinearGradientMode.Horizontal);
					break;
				case TabAlignment.Right:
					fillBrush = new LinearGradientBrush(tabBounds, light, dark, LinearGradientMode.Horizontal);
					break;
			}
			
			//	Add the blend
			fillBrush.Blend = this.GetBackgroundBlend(index);
			
			return fillBrush;
		}

		private Blend GetBackgroundBlend(int index){
			float[] relativeIntensities = new float[]{0f, 0.7f, 1f};
			float[] relativePositions = new float[]{0f, 0.8f, 1f};

			if (this._TabControl.SelectedIndex != index) {
				relativeIntensities = new float[]{0f, 0.3f, 1f};
				relativePositions = new float[]{0f, 0.2f, 1f};
			}
	
			Blend blend = new Blend();
			blend.Factors = relativeIntensities;
			blend.Positions = relativePositions;
			
			return blend;
		}
	}
}
