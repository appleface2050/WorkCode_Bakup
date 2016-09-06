using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common.UI
{
	class MessageBox : Form
	{
		static public DialogResult ShowMessageBox(string title,
				string message, string leftBtnLbl, string rightBtnLbl,
				Image pic)
		{

			using (MessageBox mBox = new MessageBox(title,
					message,
					leftBtnLbl,
					rightBtnLbl,
					pic))
			{
				DialogResult res = mBox.ShowDialog();
				return res;
			}
		}

		private MessageBox(string title,
				string message, string leftBtnLbl, string rightBtnLbl,
				Image pic)
		{

			this.Size = new System.Drawing.Size(360, 160);
			this.KeyPreview = true;
			this.ShowIcon = false;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			this.SizeGripStyle = SizeGripStyle.Hide;
			this.StartPosition = FormStartPosition.CenterScreen;

			this.Text = title;

			Button cancelButton = new Button();
			cancelButton.Text = rightBtnLbl;
			cancelButton.DialogResult = DialogResult.Cancel;
			cancelButton.Width = GetTextWidth(rightBtnLbl) + 20;
			cancelButton.Location = new System.Drawing.Point(
				this.ClientSize.Width - cancelButton.Width - 10,
				this.ClientSize.Height - cancelButton.Height - 10);

			Button okButton = new Button();
			okButton.Text = leftBtnLbl;
			okButton.DialogResult = DialogResult.OK;
			okButton.Width = GetTextWidth(leftBtnLbl) + 20;
			okButton.Location = new System.Drawing.Point(
				cancelButton.Left - okButton.Width - 10,
				this.ClientSize.Height - okButton.Height - 10);

			Label label = new Label();
			label.Text = message;
			label.Width = this.ClientSize.Width - 60;
			label.Height = cancelButton.Top - 30;
			label.Location = new System.Drawing.Point(30, 30);

			this.Controls.Add(cancelButton);
			this.Controls.Add(okButton);
			this.Controls.Add(label);
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
