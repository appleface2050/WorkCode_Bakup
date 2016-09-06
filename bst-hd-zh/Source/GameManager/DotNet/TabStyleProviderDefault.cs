using System;
using System.ComponentModel;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
	public class TabStyleProviderDefault : TabStyleProvider
	{
		public TabStyleProviderDefault(CustomTabControl tabControl): base(tabControl)
		{
			this._TabControl = tabControl;
			this.Padding = new Point(6,3);
		}

		public override void AddTabBorder(System.Drawing.Drawing2D.GraphicsPath path, System.Drawing.Rectangle tabBounds, Graphics graphics, int index)
		{
			path.AddLine(tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
			path.AddLine(tabBounds.X, tabBounds.Y, tabBounds.Right, tabBounds.Y);
			path.AddLine(tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);

			if (graphics != null)
			{
				int leftControlBarWidth = GameManager.sGameManager.mControlBarLeft.Width;
				Pen blackPen = new System.Drawing.Pen(Color.Black, 2);
				graphics.DrawLine(blackPen, leftControlBarWidth, tabBounds.Bottom, leftControlBarWidth, tabBounds.Y);
//				graphics.DrawLine(blackPen, tabBounds.X, tabBounds.Bottom, tabBounds.X, tabBounds.Y);
				graphics.DrawLine(blackPen, tabBounds.Right, tabBounds.Y, tabBounds.Right, tabBounds.Bottom);
			}
		}
	}
}
