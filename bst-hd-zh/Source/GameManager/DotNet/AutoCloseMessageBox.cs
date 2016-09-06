using System;
using System.Windows.Forms;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Runtime.InteropServices;
using System.IO;

namespace BlueStacks.hyperDroid.GameManager
{
    public class AutoCloseMessageBox : Form
    {
        private Panel msgBoxPanel;
        private Label msgBoxLabel;
        private PictureBox portraitToLandscape;

        private static System.Threading.Timer sTimeoutTimer;
        private static System.Threading.Timer sShowOnTopTimer;
        private const int WM_CLOSE = 0x0010;
        private const int sTimeout = 10000;
        private const int sShowOnTopTimeout = 2000;
        private const string sCaption = "AutoCloseMessageBox";

        [DllImport("user32.dll", SetLastError = true)]
        static extern IntPtr FindWindow(string lpClassName, string lpWindowName);
        [DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        static extern IntPtr SendMessage(IntPtr hWnd, UInt32 Msg, IntPtr wParam, IntPtr lParam);

        public AutoCloseMessageBox()
        {

            this.msgBoxPanel = new Panel();
            this.msgBoxLabel = new Label();
            this.portraitToLandscape = new PictureBox();
            this.msgBoxPanel.SuspendLayout();
            this.SuspendLayout();
            // 
            // msgBoxPanel
            // 
            this.msgBoxPanel.Controls.Add(this.portraitToLandscape);
            this.msgBoxPanel.Controls.Add(this.msgBoxLabel);
            this.msgBoxPanel.Dock = DockStyle.Fill;
            this.msgBoxPanel.Location = new Point(0, 0);
            this.msgBoxPanel.Name = "MsgBoxPanel";
            this.msgBoxPanel.Size = new Size(648, 131);
            this.msgBoxPanel.TabIndex = 0;
            // 
            // portraitToLandscape
            // 
            this.portraitToLandscape.Image = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "portraitToLandscape.png"));
            this.portraitToLandscape.Location = new System.Drawing.Point(15, 20);
            this.portraitToLandscape.Name = "portraitToLandscape";
            this.portraitToLandscape.Size = new Size(130, 107);
            this.portraitToLandscape.TabStop = false;
            // 
            // msgBoxLabel
            // 
            this.msgBoxLabel.AutoSize = true;
            this.msgBoxLabel.Font = new Font("Microsoft Sans Serif", 14.25F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
            this.msgBoxLabel.Location = new Point(150, 50);
            this.msgBoxLabel.Name = "MsgBoxLabel";
            this.msgBoxLabel.Size = new Size(60, 24);
            this.msgBoxLabel.TabIndex = 0;
            // 
            // CustomMsgBox
            // 
            this.AutoScaleDimensions = new SizeF(6F, 13F);
            this.AutoScaleMode = AutoScaleMode.Font;
            this.BackColor = Color.FromArgb(235, 235, 235);
            this.ClientSize = new Size(600, 120);
            this.Controls.Add(this.msgBoxPanel);
            this.FormBorderStyle = FormBorderStyle.None;
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "AutoCloseMessageBox";
            this.Text = "AutoCloseMessageBox";
            this.ShowInTaskbar = false;
            this.TopMost = true;
            this.msgBoxPanel.ResumeLayout(false);
            this.msgBoxPanel.PerformLayout();
            this.StartPosition = FormStartPosition.CenterParent;
            ((System.ComponentModel.ISupportInitialize)(this.portraitToLandscape)).EndInit();
            this.ResumeLayout(false);
        }

        public void ShowMsgBox(string text)
        {
            this.msgBoxLabel.Text = text;
            sTimeoutTimer = new System.Threading.Timer(OnTimerElapsed,
                        null, sTimeout, System.Threading.Timeout.Infinite);
            sShowOnTopTimer = new System.Threading.Timer(OnShowOnTopTimerElapsed,
                        null, sShowOnTopTimeout, System.Threading.Timeout.Infinite);
            this.Show();
        }

        private void OnTimerElapsed(object state)
        {
            IntPtr mbWnd = FindWindow(null, sCaption);
            if (mbWnd != IntPtr.Zero)
                SendMessage(mbWnd, WM_CLOSE, IntPtr.Zero, IntPtr.Zero);
            sTimeoutTimer.Dispose();
        }

        private void OnShowOnTopTimerElapsed(object sender)
        {
            this.TopMost = false;
        }

        static AutoCloseMessageBox mAutoCloseMessageBox = null;
        internal static void ShowMsg()
        {
            if (mAutoCloseMessageBox == null)
            {
                mAutoCloseMessageBox = new AutoCloseMessageBox();
            }
            mAutoCloseMessageBox.ShowMsgBox("Please rotate the screen for optimal experience");
        }
        internal static void HideBox()
        {
            if (mAutoCloseMessageBox != null)
            {
                mAutoCloseMessageBox.Hide();
            }
        }
    }
}
