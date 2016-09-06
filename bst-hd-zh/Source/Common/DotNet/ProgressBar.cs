using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common.UI
{
	class ProgressBar : Form
	{
		static public DialogResult ShowProgressBar(string message)
		{

			using (ProgressBar pBox = new ProgressBar(message))
			{
				DialogResult res = pBox.ShowDialog();
				return res;
			}
		}

		public ProgressBar(string message)
		{

			this.Size = new System.Drawing.Size(320, 120);
			this.KeyPreview = true;
			this.ShowIcon = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ControlBox = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = SizeGripStyle.Hide;
			this.StartPosition = FormStartPosition.CenterScreen;
			this.FormBorderStyle = FormBorderStyle.FixedDialog;
			this.TopLevel = false;

			Label label = new Label();
			label.Text = message;
			label.AutoSize = true;
			label.Location = new System.Drawing.Point(20, 15);

			System.Windows.Forms.ProgressBar progressControl = new System.Windows.Forms.ProgressBar();
			progressControl.Style = ProgressBarStyle.Marquee;
			progressControl.Value = 100;
			progressControl.Width = 260;
			progressControl.Location = new System.Drawing.Point(20, 45);

			this.Controls.Add(label);
			this.Controls.Add(progressControl);
		}

		private int GetTextWidth(String text)
		{
			using (Graphics gfx = this.CreateGraphics())
			{
				SizeF size = gfx.MeasureString(text, this.Font);
				return (int)size.Width;
			}
		}
	}
}
