using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.GameManager.gamemanager
{
	public partial class PromptForm : Form
	{
		public const int WM_NCLBUTTONDOWN = 0xA1;
		public const int HT_CAPTION = 0x2;

		[DllImportAttribute("user32.dll")]
		public static extern int SendMessage(IntPtr hWnd, int Msg, int wParam, int lParam);

		[DllImportAttribute("user32.dll")]
		public static extern bool ReleaseCapture();
		public PromptForm(string msg, string leftBtnLbl, string rightBtnLbl)
		{
			InitializeComponent();
			this.lblMsg.Text = msg;
			this.btnOK.Text = leftBtnLbl;
			this.btnCancel.Text = rightBtnLbl;
			this.ShowInTaskbar = false;
		}

		public PromptForm(string msg)
		{
			InitializeComponent();
			this.lblMsg.Text = msg;
			this.btnConform.Visible = true;
			this.btnOK.Visible = false;
			this.btnCancel.Visible = false;
			this.ShowInTaskbar = false;
		}

		private void btnOK_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private void btnCancel_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.Cancel;
		}

		private void btnConform_Click(object sender, EventArgs e)
		{
			this.DialogResult = System.Windows.Forms.DialogResult.OK;
		}

		private void lblMsg_MouseDown(object sender, MouseEventArgs e)
		{
			if (e.Button == MouseButtons.Left)
			{
				ReleaseCapture();
				SendMessage(this.Handle, WM_NCLBUTTONDOWN, HT_CAPTION, 0);
			}
		}
	}
}
