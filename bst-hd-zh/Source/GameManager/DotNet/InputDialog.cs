using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Drawing;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
    public class InputDialog : Form
    {
        private const int CS_DROPSHADOW = 0x00020000;
        protected Label lblMessage;
        protected TextBox txtInput;
        protected string _txtInput;
        protected bool _txtPaintInvalidated = false;

        public InputDialog()
        {
            System.Windows.Size sizeOfGM = GameManagerWindow.Instance.RenderSize;
            this.Width = Convert.ToInt32(sizeOfGM.Width * .25);
            this.Height = Convert.ToInt32(sizeOfGM.Height * .17);
            this.MinimumSize = new Size(300, 122);

            Panel pl = new Panel();
            pl.Dock = DockStyle.Fill;

            FlowLayoutPanel flp = new FlowLayoutPanel();
            flp.Dock = DockStyle.Fill;

            lblMessage = new Label();
            lblMessage.Font = new Font("Microsoft Sans Serif", 8.5F);
            lblMessage.ForeColor = Color.White;
            lblMessage.AutoSize = true;
            lblMessage.TextAlign = ContentAlignment.MiddleCenter;
            //lblMessage.Text = "Please enter theme name";

            Panel txtPl = new Panel();
            txtPl.BorderStyle = BorderStyle.None;
            txtPl.Width = Convert.ToInt32(sizeOfGM.Width * .23);
            txtPl.Height = Convert.ToInt32(sizeOfGM.Height * .03);
            //txtPl.Padding = new Padding(5);
            txtPl.BackColor = Color.White;
            txtPl.Margin = new Padding(0, Convert.ToInt32(sizeOfGM.Height * .02), 0, 0);
            txtPl.Paint += txtPl_Paint;
            txtPl.MinimumSize = new Size(300, 23);

            txtInput = new TextBox();
            txtInput.Dock = DockStyle.Fill;
            txtInput.BorderStyle = BorderStyle.None;
            txtInput.Font = new Font("Microsoft Sans Serif", 8F);
            txtInput.KeyDown += txtInput_KeyDown;
            txtInput.BackColor = Color.FromArgb(240, 240, 240);
            txtInput.MaxLength = 50;
            txtInput.Multiline = false;
            txtInput.TabIndex = 0;
            txtPl.Controls.Add(txtInput);

            FlowLayoutPanel flpButtons = new FlowLayoutPanel();
            flpButtons.Dock = DockStyle.Bottom;
            flpButtons.FlowDirection = FlowDirection.RightToLeft;
            flpButtons.Height = Convert.ToInt32(sizeOfGM.Height * .05) > 38 ? Convert.ToInt32(sizeOfGM.Height * .05) : 38;

            Button btnCancel = new Button();
            btnCancel.Text = Locale.Strings.GetLocalizedString("NoText");
            btnCancel.ForeColor = Color.FromArgb(170, 170, 170);
            btnCancel.Font = new Font("Microsoft Sans Serif", 8F);
            //btnCancel.Padding = new Padding(3);
            btnCancel.FlatStyle = FlatStyle.Flat;
            btnCancel.Width = Convert.ToInt32(sizeOfGM.Height * .08) > 60 ? Convert.ToInt32(sizeOfGM.Height * .08) : 60;
            btnCancel.Height = Convert.ToInt32(sizeOfGM.Height * .04) > 30 ? Convert.ToInt32(sizeOfGM.Height * .04) : 30;
            //btnCancel.Width = Convert.ToInt32(sizeOfGM.Height * .07);
            btnCancel.TabIndex = 2;
            btnCancel.Click += btnCancel_Click;

            Button btnOK = new Button();
            btnOK.Text = Locale.Strings.GetLocalizedString("YesText");
            btnOK.ForeColor = Color.FromArgb(170, 170, 170);
            btnOK.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            //btnOK.Padding = new Padding(3);
            btnOK.FlatStyle = FlatStyle.Flat;
            btnOK.Width = Convert.ToInt32(sizeOfGM.Height * .08) > 60 ? Convert.ToInt32(sizeOfGM.Height * .08) : 60;
            btnOK.Height = Convert.ToInt32(sizeOfGM.Height * .04) > 30 ? Convert.ToInt32(sizeOfGM.Height * .04) : 30;
            //btnOK.Width = Convert.ToInt32(sizeOfGM.Height * .07);
            btnOK.TabIndex = 1;
            btnOK.Click += btnOK_Click;

            flpButtons.Controls.Add(btnCancel);
            flpButtons.Controls.Add(btnOK);

            flp.Controls.Add(lblMessage);
            flp.SetFlowBreak(lblMessage, true);
            flp.Controls.Add(txtPl);
            flp.SetFlowBreak(txtPl, true);
            //flp.Controls.Add(flpButtons);
            pl.Controls.Add(flp);

            this.Controls.Add(pl);
            this.Controls.Add(flpButtons);
            this.FormBorderStyle = FormBorderStyle.None;
            this.BackColor = Color.FromArgb(45, 45, 48);
            this.StartPosition = FormStartPosition.CenterScreen;
            this.Padding = new Padding(Convert.ToInt32(sizeOfGM.Height * .015));
        }

        void txtInput_KeyDown(object sender, KeyEventArgs e)
        {

            //TextBox txt = (TextBox)sender;
            //if (e.KeyCode == Keys.Enter)
            //{
            //    //_txtInput = txt.Text;
            //    //this.Dispose();
            //}
            //else
            //{
            //    if (txt.Text.Length > 60)
            //    {
            //        txt.Parent.Height = 80;

            //        if (!_txtPaintInvalidated)
            //        {
            //            txt.Parent.Invalidate();
            //            _txtPaintInvalidated = true;
            //        }
            //    }

            //    if (txt.Text.Length < 60)
            //    {
            //        txt.Parent.Height = 38;

            //        if (_txtPaintInvalidated)
            //        {
            //            txt.Parent.Invalidate();
            //            _txtPaintInvalidated = false;
            //        }
            //    }
            //}
        }

        void txtPl_Paint(object sender, PaintEventArgs e)
        {
            Panel pl = (Panel)sender;
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(new Point(0, 0), new Size(pl.Width - 1, pl.Height - 1));
            Pen pen = new Pen(Color.FromArgb(0, 151, 251));
            pen.Width = 3;
            g.FillRectangle(new SolidBrush(Color.FromArgb(240, 240, 240)), rect);
            g.DrawRectangle(pen, rect);
        }

        void btnCancel_Click(object sender, EventArgs e)
        {
            _txtInput = "";
            this.Dispose();
        }

        void btnOK_Click(object sender, EventArgs e)
        {
            _txtInput = txtInput.Text;
            this.Dispose();
        }

        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;
            }
        }

        public static string Show(string message)
        {
            InputDialog dialog = new InputDialog();
            dialog.lblMessage.Text = message;
            dialog.ShowDialog();

            return dialog._txtInput;
        }

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);

            Graphics g = e.Graphics;
            Rectangle rect = new Rectangle(new Point(0, 0), new Size(this.Width - 1, this.Height - 1));
            Pen pen = new Pen(Color.FromArgb(0, 151, 251));

            g.DrawRectangle(pen, rect);

        }
    }
}
