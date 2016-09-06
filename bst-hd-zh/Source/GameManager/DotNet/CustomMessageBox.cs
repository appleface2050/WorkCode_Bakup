using System;
using System.Drawing;
using System.Drawing.Text;
using System.Windows;
using System.Windows.Forms;
using System.Drawing.Drawing2D;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{
    class CustomMessageBox : Form
    {
        bool rememberChoice = false;
        const int CS_DROPSHADOW = 0x00020000;
        protected override CreateParams CreateParams
        {
            get
            {
                CreateParams cp = base.CreateParams;
                cp.ClassStyle |= CS_DROPSHADOW;
                return cp;

            }
        }

        static public DialogResult ShowMessageBox(string title, string message, string leftBtnLbl,
            string rightBtnLbl, string cancelBtnLbl, string checkBoxLbl, Image pic)
        {
            using (CustomMessageBox mBox = new CustomMessageBox(title,
                    message,
                    leftBtnLbl,
                    rightBtnLbl,
                    cancelBtnLbl,
                    checkBoxLbl,
                    pic))
            {
                DialogResult res = mBox.ShowDialog();
                return res;
            }
        }

        private CustomMessageBox(string title, string message, string leftBtnLbl,
            string rightBtnLbl, string cancelBtnLbl, string checkBoxLbl, Image pic)
        {
            int gmWidth = (int)(GameManagerWindow.Instance.Width < 1300 ? 1300 : GameManagerWindow.Instance.Width);
            int gmHeight = Convert.ToInt32(gmWidth * .56);

            this.ClientSize = new System.Drawing.Size(Convert.ToInt32(gmWidth * .75), Convert.ToInt32(gmHeight * .60));
            this.KeyPreview = true;
            this.ShowIcon = false;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.ShowInTaskbar = false;
            this.SizeGripStyle = SizeGripStyle.Hide;
            this.StartPosition = FormStartPosition.CenterScreen;
            this.FormBorderStyle = FormBorderStyle.None;
            this.ControlBox = false;
            this.BackColor = ColorTranslator.FromHtml("#ffffff");
            this.Paint += CustomMessageBoxPaint;
            this.Text = title;

            int topPadding = Convert.ToInt32(gmHeight * .1);
            int bottomPadding = Convert.ToInt32(gmHeight * .1);
            int leftPadding = Convert.ToInt32(gmWidth * .05);
            int rightPadding = Convert.ToInt32(gmWidth * .05);
            int buttonSpacing = Convert.ToInt32(gmWidth * .01);
            int checkboxBottomPadding = Convert.ToInt32(this.ClientSize.Height * .02);


            int buttonFontSize = Convert.ToInt32(gmWidth * .010);
            int textFontSize = Convert.ToInt32(gmWidth * .0125);


            Button okButton = new Button();
            okButton.Text = leftBtnLbl;
            okButton.Name = "Yes";
            okButton.DialogResult = DialogResult.Yes;
            okButton.Width = Convert.ToInt32(gmWidth * .1);
            okButton.Height = Convert.ToInt32(gmHeight * .1);
            okButton.BackColor = ColorTranslator.FromHtml("#84d2e4");
            okButton.ForeColor = ColorTranslator.FromHtml("#ffffff");
            okButton.Location = new System.Drawing.Point(
                leftPadding,
                this.ClientSize.Height - okButton.Height - bottomPadding);
            okButton.FlatStyle = FlatStyle.Flat;
            okButton.MouseEnter += new EventHandler(this.ButtonMouseEnter);
            okButton.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
            okButton.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
            okButton.MouseLeave += new EventHandler(this.ButtonMouseLeave);

            Button noButton = new Button();
            noButton.Text = rightBtnLbl;
            noButton.Name = "No";
            noButton.DialogResult = DialogResult.No;
            noButton.Width = Convert.ToInt32(gmWidth * .1);
            noButton.Height = Convert.ToInt32(gmHeight * .1);
            noButton.BackColor = ColorTranslator.FromHtml("#f64c4c");
            noButton.ForeColor = ColorTranslator.FromHtml("#ffffff");
            noButton.Location = new System.Drawing.Point(
                okButton.Right + buttonSpacing,
                this.ClientSize.Height - noButton.Height - bottomPadding);
            noButton.FlatStyle = FlatStyle.Flat;
            noButton.MouseEnter += new EventHandler(this.ButtonMouseEnter);
            noButton.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
            noButton.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
            noButton.MouseLeave += new EventHandler(this.ButtonMouseLeave);

            Button cancelButton = new Button();
            cancelButton.Text = cancelBtnLbl;
            cancelButton.Name = "Cancel";
            cancelButton.DialogResult = DialogResult.Cancel;
            cancelButton.Width = Convert.ToInt32(gmWidth * .1);
            cancelButton.Height = Convert.ToInt32(gmHeight * .1);
            cancelButton.BackColor = ColorTranslator.FromHtml("#c1cfd8");
            cancelButton.ForeColor = ColorTranslator.FromHtml("#ffffff");
            cancelButton.Location = new System.Drawing.Point(
                this.ClientSize.Width - cancelButton.Width - rightPadding,
                this.ClientSize.Height - cancelButton.Height - bottomPadding);
            cancelButton.FlatStyle = FlatStyle.Flat;
            cancelButton.MouseEnter += new EventHandler(this.ButtonMouseEnter);
            cancelButton.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
            cancelButton.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
            cancelButton.MouseLeave += new EventHandler(this.ButtonMouseLeave);

            CheckBox rememberCheckBox = new CheckBox();
            rememberCheckBox.Text = checkBoxLbl;
            rememberCheckBox.Name = "Remember";
            rememberCheckBox.Width = Convert.ToInt32(gmWidth * .3);
            rememberCheckBox.Height = Convert.ToInt32(gmHeight * .1);
            rememberCheckBox.Checked = rememberChoice;
            rememberCheckBox.ForeColor = ColorTranslator.FromHtml("#8d9ba3");
            rememberCheckBox.Location = new System.Drawing.Point(
                leftPadding,
                this.ClientSize.Height - rememberCheckBox.Height - checkboxBottomPadding);
            rememberCheckBox.Click += new EventHandler(this.CheckBoxClicked);

            Label label = new Label();
            label.Text = message;
            label.Width = this.ClientSize.Width - leftPadding - rightPadding;
            label.Height = this.ClientSize.Height - topPadding - bottomPadding;
            label.Location = new System.Drawing.Point(leftPadding, topPadding);
            label.ForeColor = ColorTranslator.FromHtml("#8d9ba3");
            label.TextAlign = ContentAlignment.TopCenter;
            label.AutoSize = false;
            label.Font = new Font("Arial", textFontSize);

            this.Controls.Add(okButton);
            this.Controls.Add(noButton);
            this.Controls.Add(cancelButton);
            this.Controls.Add(rememberCheckBox);
            this.Controls.Add(label);
        }

        private void CustomMessageBoxPaint(object sender, PaintEventArgs e)
        {
            Rectangle rect = this.DisplayRectangle;
            rect.Width -= 1;
            rect.Height -= 1;
            e.Graphics.DrawRectangle(new Pen(Color.Black, 3), rect);
        }

        private int GetTextWidth(String text, Font font)
        {
            using (Graphics gfx = this.CreateGraphics())
            {
                SizeF size = gfx.MeasureString(text, font);
                return (int)size.Width;
            }
        }

        private void CheckBoxClicked(object sender, EventArgs e)
        {
            CheckBox checkbox = (CheckBox)sender;
            if (checkbox.Checked)
                rememberChoice = true;
            else
                rememberChoice = false;

            GameManagerUtilities.sRememberClosingPopupChoice = rememberChoice;
        }

        private void ButtonMouseEnter(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "Yes":
                    button.BackColor = ColorTranslator.FromHtml("#55e0cc");
                    break;

                case "No":
                    button.BackColor = ColorTranslator.FromHtml("#f66a4c");
                    break;

                case "Cancel":
                    button.BackColor = ColorTranslator.FromHtml("#88a3b3");
                    break;
            }
        }

        private void ButtonMouseDown(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "Yes":
                    button.BackColor = ColorTranslator.FromHtml("#a9efe1");
                    break;

                case "No":
                    button.BackColor = ColorTranslator.FromHtml("#fbb5a6");
                    break;

                case "Cancel":
                    button.BackColor = ColorTranslator.FromHtml("#c4d1d9");
                    break;
            }
        }

        private void ButtonMouseUp(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "Yes":
                    button.BackColor = ColorTranslator.FromHtml("#55e0cc");
                    break;

                case "No":
                    button.BackColor = ColorTranslator.FromHtml("#f66a4c");
                    break;

                case "Cancel":
                    button.BackColor = ColorTranslator.FromHtml("#88a3b3");
                    break;
            }
        }

        private void ButtonMouseLeave(object sender, EventArgs e)
        {
            Button button = (Button)sender;

            switch (button.Name)
            {
                case "Yes":
                    button.BackColor = ColorTranslator.FromHtml("#84d2e4");
                    break;

                case "No":
                    button.BackColor = ColorTranslator.FromHtml("#f64c4c");
                    break;

                case "Cancel":
                    button.BackColor = ColorTranslator.FromHtml("#c1cfd8");
                    break;
            }
        }
    }
}
