using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;
using System.Threading;

namespace BlueStacks.hyperDroid.Common.UI
{
	class AdvancedProgressBar : Form
	{

		private Label label = new Label();
		private System.Windows.Forms.ProgressBar progressControl;
		private System.Threading.Timer closingTimer;
		private bool willHide;
		private SynchronizationContext mUiContext;

		public bool WillHide
		{
			get
			{
				return willHide;
			}
			set
			{
				willHide = value;
			}
		}

		public AdvancedProgressBar(string message)
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

			label = new Label();
			label.Text = message;
			label.AutoSize = true;
			label.Location = new System.Drawing.Point(20, 15);

			progressControl = new System.Windows.Forms.ProgressBar();
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

		protected override void OnShown(EventArgs e)
		{
			Logger.Info("Form Created");
			this.mUiContext = WindowsFormsSynchronizationContext.Current;
			closingTimer = new System.Threading.Timer(
					CloseDialog,
					null,
					1000,
					Timeout.Infinite
					);
			willHide = false;
		}

		public void CloseDialog(Object s)
		{
			if (!WillHide)
			{
				closingTimer.Change(1000, Timeout.Infinite);
				return;
			}
			SendOrPostCallback cb = new SendOrPostCallback(delegate (Object obj)
					{
						this.Hide();
						this.Close();
						Logger.Info(string.Format("Form Closed, Disposing = {0}", this.Disposing));
					});
			try
			{
				mUiContext.Send(cb, null);
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}
		}
	}
}
