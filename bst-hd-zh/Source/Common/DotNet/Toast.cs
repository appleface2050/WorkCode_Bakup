using System;
using System.Drawing;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
	class Toast : Form
	{

		[DllImport("user32.dll", SetLastError = true)]
		static extern bool SetProcessDPIAware();

		[DllImport("gdi32.dll")]
		private static extern System.IntPtr CreateRoundRectRgn(int nLeftRect,
				int nTopRect,
				int nRightRect,
				int nBottomRect,
				int nWidthEllipse,
				int nHeightEllipse);

		[DllImport("gdi32.dll")]
		private static extern bool DeleteObject(System.IntPtr hObject);

		const int WS_EX_TOOLWINDOW = 0x00000080;
		const int WS_EX_NOACTIVATE = 0x08000000;
		const int WS_CHILD = 0x40000000;

		private Font font = new Font(Utils.GetSystemFontName(), 12);
		private SizeF stringSize;
		private String toastText;

		public Toast(Control parent, string toastText)
		{
			this.toastText = toastText;
			Graphics g = CreateGraphics();
			stringSize = g.MeasureString(this.toastText, font);

			this.StartPosition = FormStartPosition.Manual;
			this.FormBorderStyle = FormBorderStyle.None;
			this.ShowInTaskbar = false;
			this.Paint += ShowToast;
			this.Width = ((int)stringSize.Width + 20);
			this.Height = ((int)stringSize.Height + 20);

			int left = parent.Left + (parent.Width - this.Width) / 2;
			int top = parent.Top + 5;
			this.Location = new Point(left, top);
			//this.Owner		= parent;

			System.IntPtr roundedRect = CreateRoundRectRgn(0, 0, this.Width, this.Height, 5, 5);
			this.Region = System.Drawing.Region.FromHrgn(roundedRect);
			DeleteObject(roundedRect);
		}

		private void ShowToast(object sender, PaintEventArgs e)
		{
			RectangleF drawRect = new RectangleF(0, 0, this.Width, this.Height);
			Pen rectangleBorder = new Pen(Color.Black);
			e.Graphics.DrawRectangle(rectangleBorder, 0, 0, this.Width, this.Height);

			SolidBrush rectFillColor = new SolidBrush(Color.White);
			e.Graphics.FillRectangle(rectFillColor, drawRect);

			float left = (this.Width - stringSize.Width) / 2;
			float top = (this.Height - stringSize.Height) / 2;
			RectangleF textRect = new RectangleF(left, top, stringSize.Width, stringSize.Height);
			SolidBrush textColor = new SolidBrush(Color.Black);
			e.Graphics.DrawString(this.toastText, font, textColor, textRect);

			//this.Owner.Focus();
		}

		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.Style = WS_CHILD;
				cp.ExStyle |= WS_EX_NOACTIVATE | WS_EX_TOOLWINDOW;
				return cp;
			}
		}
	}
}

