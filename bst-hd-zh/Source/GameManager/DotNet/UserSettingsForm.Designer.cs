namespace BlueStacks.hyperDroid.GameManager
{
    partial class UserSettingsForm
    {
        /// <summary>
        /// Required designer variable.
        /// </summary>
        private System.ComponentModel.IContainer components = null;

        /// <summary>
        /// Clean up any resources being used.
        /// </summary>
        /// <param name="disposing">true if managed resources should be disposed; otherwise, false.</param>
        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null))
            {
                components.Dispose();
            }
            base.Dispose(disposing);
        }

        #region Windows Form Designer generated code

        class TextBoxWihtOutCopy : System.Windows.Forms.TextBox
        {
            protected override void WndProc(ref   System.Windows.Forms.Message m)
            {
                if (m.Msg != 0x007B && m.Msg != 0x0301 && m.Msg != 0x0302)
                {
                    base.WndProc(ref m);
                }
            }
        }
        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.gbResolution = new System.Windows.Forms.GroupBox();
            this.lblCustomizeResolutionPrompt = new System.Windows.Forms.Label();
            this.tbHeight = new TextBoxWihtOutCopy();
            this.lblMultiply = new System.Windows.Forms.Label();
            this.tbWide = new TextBoxWihtOutCopy();
            this.rbPMode900 = new System.Windows.Forms.RadioButton();
            this.rbPMode960 = new System.Windows.Forms.RadioButton();
            this.rbPMode720 = new System.Windows.Forms.RadioButton();
            this.rbCustomize = new System.Windows.Forms.RadioButton();
            this.rbLMode1440 = new System.Windows.Forms.RadioButton();
            this.rbLMode1280 = new System.Windows.Forms.RadioButton();
            this.rbLMode960 = new System.Windows.Forms.RadioButton();
            this.gbBossKey = new System.Windows.Forms.GroupBox();
            this.lblInputPrompt = new System.Windows.Forms.Label();
            this.tbBossKey = new TextBoxWihtOutCopy();
            this.lblBossKeyName = new System.Windows.Forms.Label();
            this.lblQQ = new System.Windows.Forms.Label();
            this.lblQQGroup = new System.Windows.Forms.Label();
            this.btnCancel = new System.Windows.Forms.Button();
            this.btnOK = new System.Windows.Forms.Button();
            this.pbClose = new System.Windows.Forms.PictureBox();
            this.gbResolution.SuspendLayout();
            this.gbBossKey.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).BeginInit();
            this.SuspendLayout();
            // 
            // gbResolution
            // 
            this.gbResolution.Controls.Add(this.lblCustomizeResolutionPrompt);
            this.gbResolution.Controls.Add(this.tbHeight);
            this.gbResolution.Controls.Add(this.lblMultiply);
            this.gbResolution.Controls.Add(this.tbWide);
            this.gbResolution.Controls.Add(this.rbPMode900);
            this.gbResolution.Controls.Add(this.rbPMode960);
            this.gbResolution.Controls.Add(this.rbPMode720);
            this.gbResolution.Controls.Add(this.rbCustomize);
            this.gbResolution.Controls.Add(this.rbLMode1440);
            this.gbResolution.Controls.Add(this.rbLMode1280);
            this.gbResolution.Controls.Add(this.rbLMode960);
            this.gbResolution.Font = new System.Drawing.Font("SimSun", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.gbResolution.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.gbResolution.Location = new System.Drawing.Point(32, 34);
            this.gbResolution.Name = "gbResolution";
            this.gbResolution.Size = new System.Drawing.Size(577, 209);
            this.gbResolution.TabIndex = 0;
            this.gbResolution.TabStop = false;
            this.gbResolution.Text = "分辨率设置";
            // 
            // lblCustomizeResolutionPrompt
            // 
            this.lblCustomizeResolutionPrompt.AutoSize = true;
            this.lblCustomizeResolutionPrompt.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblCustomizeResolutionPrompt.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblCustomizeResolutionPrompt.Location = new System.Drawing.Point(348, 187);
            this.lblCustomizeResolutionPrompt.Name = "lblCustomizeResolutionPrompt";
            this.lblCustomizeResolutionPrompt.Size = new System.Drawing.Size(95, 12);
            this.lblCustomizeResolutionPrompt.TabIndex = 11;
            this.lblCustomizeResolutionPrompt.Text = "（宽度 x 高度）";
            // 
            // tbHeight
            // 
            this.tbHeight.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(68)))), ((int)(((byte)(76)))));
            this.tbHeight.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbHeight.Enabled = false;
            this.tbHeight.Font = new System.Drawing.Font("SimSun", 12F);
            this.tbHeight.ForeColor = System.Drawing.SystemColors.Control;
            this.tbHeight.Location = new System.Drawing.Point(414, 160);
            this.tbHeight.MaxLength = 4;
            this.tbHeight.Name = "tbHeight";
            this.tbHeight.Size = new System.Drawing.Size(141, 19);
            this.tbHeight.TabIndex = 9;
            this.tbHeight.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbHeight.TextChanged += new System.EventHandler(this.tbHeight_TextChanged);
            this.tbHeight.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbHeight_KeyPress);
            // 
            // lblMultiply
            // 
            this.lblMultiply.AutoSize = true;
            this.lblMultiply.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblMultiply.Location = new System.Drawing.Point(386, 159);
            this.lblMultiply.Name = "lblMultiply";
            this.lblMultiply.Size = new System.Drawing.Size(22, 21);
            this.lblMultiply.TabIndex = 8;
            this.lblMultiply.Text = "x";
            // 
            // tbWide
            // 
            this.tbWide.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(68)))), ((int)(((byte)(76)))));
            this.tbWide.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbWide.Enabled = false;
            this.tbWide.Font = new System.Drawing.Font("SimSun", 12F);
            this.tbWide.ForeColor = System.Drawing.SystemColors.Control;
            this.tbWide.Location = new System.Drawing.Point(235, 160);
            this.tbWide.MaxLength = 4;
            this.tbWide.Name = "tbWide";
            this.tbWide.Size = new System.Drawing.Size(141, 19);
            this.tbWide.TabIndex = 7;
            this.tbWide.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbWide.TextChanged += new System.EventHandler(this.tbWide_TextChanged);
            this.tbWide.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbWide_KeyPress);
            // 
            // rbPMode900
            // 
            this.rbPMode900.AutoSize = true;
            this.rbPMode900.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbPMode900.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.rbPMode900.Location = new System.Drawing.Point(322, 122);
            this.rbPMode900.Name = "rbPMode900";
            this.rbPMode900.Size = new System.Drawing.Size(188, 23);
            this.rbPMode900.TabIndex = 6;
            this.rbPMode900.TabStop = true;
            this.rbPMode900.Text = "竖屏 900 x 1440";
            this.rbPMode900.UseVisualStyleBackColor = true;
            this.rbPMode900.CheckedChanged += new System.EventHandler(this.rbLMode900_CheckedChanged);
            // 
            // rbPMode960
            // 
            this.rbPMode960.AutoSize = true;
            this.rbPMode960.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbPMode960.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.rbPMode960.Location = new System.Drawing.Point(322, 83);
            this.rbPMode960.Name = "rbPMode960";
            this.rbPMode960.Size = new System.Drawing.Size(188, 23);
            this.rbPMode960.TabIndex = 5;
            this.rbPMode960.TabStop = true;
            this.rbPMode960.Text = "竖屏 960 x 1280";
            this.rbPMode960.UseVisualStyleBackColor = true;
            this.rbPMode960.CheckedChanged += new System.EventHandler(this.rbLMode960_CheckedChanged);
            // 
            // rbPMode720
            // 
            this.rbPMode720.AutoSize = true;
            this.rbPMode720.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbPMode720.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.rbPMode720.Location = new System.Drawing.Point(322, 42);
            this.rbPMode720.Name = "rbPMode720";
            this.rbPMode720.Size = new System.Drawing.Size(177, 23);
            this.rbPMode720.TabIndex = 4;
            this.rbPMode720.TabStop = true;
            this.rbPMode720.Text = "竖屏 720 x 960";
            this.rbPMode720.UseVisualStyleBackColor = true;
            this.rbPMode720.CheckedChanged += new System.EventHandler(this.rbLMode720_CheckedChanged);
            // 
            // rbCustomize
            // 
            this.rbCustomize.AutoSize = true;
            this.rbCustomize.Font = new System.Drawing.Font("SimSun", 14F, System.Drawing.FontStyle.Bold);
            this.rbCustomize.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.rbCustomize.Location = new System.Drawing.Point(34, 158);
            this.rbCustomize.Name = "rbCustomize";
            this.rbCustomize.Size = new System.Drawing.Size(127, 23);
            this.rbCustomize.TabIndex = 3;
            this.rbCustomize.TabStop = true;
            this.rbCustomize.Text = "自定义设置";
            this.rbCustomize.UseVisualStyleBackColor = true;
            this.rbCustomize.CheckedChanged += new System.EventHandler(this.rbCustomize_CheckedChanged);
            // 
            // rbLMode1440
            // 
            this.rbLMode1440.AutoSize = true;
            this.rbLMode1440.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbLMode1440.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.rbLMode1440.Location = new System.Drawing.Point(34, 122);
            this.rbLMode1440.Name = "rbLMode1440";
            this.rbLMode1440.Size = new System.Drawing.Size(188, 23);
            this.rbLMode1440.TabIndex = 2;
            this.rbLMode1440.TabStop = true;
            this.rbLMode1440.Text = "横屏 1440 x 900";
            this.rbLMode1440.UseVisualStyleBackColor = true;
            this.rbLMode1440.CheckedChanged += new System.EventHandler(this.rbPMode1440_CheckedChanged);
            // 
            // rbLMode1280
            // 
            this.rbLMode1280.AutoSize = true;
            this.rbLMode1280.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbLMode1280.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.rbLMode1280.Location = new System.Drawing.Point(34, 83);
            this.rbLMode1280.Name = "rbLMode1280";
            this.rbLMode1280.Size = new System.Drawing.Size(199, 23);
            this.rbLMode1280.TabIndex = 1;
            this.rbLMode1280.TabStop = true;
            this.rbLMode1280.Text = "横屏 1280 x 960 ";
            this.rbLMode1280.UseVisualStyleBackColor = true;
            this.rbLMode1280.CheckedChanged += new System.EventHandler(this.rbPMode1280_CheckedChanged);
            // 
            // rbLMode960
            // 
            this.rbLMode960.AutoSize = true;
            this.rbLMode960.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.rbLMode960.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.rbLMode960.Location = new System.Drawing.Point(34, 42);
            this.rbLMode960.Name = "rbLMode960";
            this.rbLMode960.Size = new System.Drawing.Size(188, 23);
            this.rbLMode960.TabIndex = 0;
            this.rbLMode960.TabStop = true;
            this.rbLMode960.Text = "横屏  960 x 720";
            this.rbLMode960.UseVisualStyleBackColor = true;
            this.rbLMode960.CheckedChanged += new System.EventHandler(this.rbPMode960_CheckedChanged);
            // 
            // gbBossKey
            // 
            this.gbBossKey.Controls.Add(this.lblInputPrompt);
            this.gbBossKey.Controls.Add(this.tbBossKey);
            this.gbBossKey.Controls.Add(this.lblBossKeyName);
            this.gbBossKey.Font = new System.Drawing.Font("SimSun", 15.75F, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.gbBossKey.ForeColor = System.Drawing.SystemColors.ControlLight;
            this.gbBossKey.Location = new System.Drawing.Point(32, 249);
            this.gbBossKey.Name = "gbBossKey";
            this.gbBossKey.Size = new System.Drawing.Size(577, 102);
            this.gbBossKey.TabIndex = 1;
            this.gbBossKey.TabStop = false;
            this.gbBossKey.Text = "老板键设置";
            // 
            // lblInputPrompt
            // 
            this.lblInputPrompt.AutoSize = true;
            this.lblInputPrompt.Font = new System.Drawing.Font("SimSun", 9F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.lblInputPrompt.Location = new System.Drawing.Point(278, 77);
            this.lblInputPrompt.Name = "lblInputPrompt";
            this.lblInputPrompt.Size = new System.Drawing.Size(143, 12);
            this.lblInputPrompt.TabIndex = 2;
            this.lblInputPrompt.Text = "*请使用键盘输入自定义键";
            // 
            // tbBossKey
            // 
            this.tbBossKey.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(69)))), ((int)(((byte)(68)))), ((int)(((byte)(72)))));
            this.tbBossKey.BorderStyle = System.Windows.Forms.BorderStyle.None;
            this.tbBossKey.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.tbBossKey.ForeColor = System.Drawing.SystemColors.Control;
            this.tbBossKey.Location = new System.Drawing.Point(244, 47);
            this.tbBossKey.Name = "tbBossKey";
            this.tbBossKey.Size = new System.Drawing.Size(219, 19);
            this.tbBossKey.TabIndex = 1;
            this.tbBossKey.TextAlign = System.Windows.Forms.HorizontalAlignment.Center;
            this.tbBossKey.KeyDown += new System.Windows.Forms.KeyEventHandler(this.tbBossKey_KeyDown);
            this.tbBossKey.KeyPress += new System.Windows.Forms.KeyPressEventHandler(this.tbBossKey_KeyPress);
            this.tbBossKey.KeyUp += new System.Windows.Forms.KeyEventHandler(this.tbBossKey_KeyUp);
            // 
            // lblBossKeyName
            // 
            this.lblBossKeyName.AutoSize = true;
            this.lblBossKeyName.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblBossKeyName.Location = new System.Drawing.Point(34, 47);
            this.lblBossKeyName.Name = "lblBossKeyName";
            this.lblBossKeyName.Size = new System.Drawing.Size(129, 19);
            this.lblBossKeyName.TabIndex = 0;
            this.lblBossKeyName.Text = "自定义老板键";
            // 
            // lblQQ
            // 
            this.lblQQ.AutoSize = true;
            this.lblQQ.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblQQ.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblQQ.Location = new System.Drawing.Point(42, 368);
            this.lblQQ.Name = "lblQQ";
            this.lblQQ.Size = new System.Drawing.Size(201, 19);
            this.lblQQ.TabIndex = 2;
            this.lblQQ.Text = "客服QQ：3033957406";
            this.lblQQ.Visible = false;
            // 
            // lblQQGroup
            // 
            this.lblQQGroup.AutoSize = true;
            this.lblQQGroup.Font = new System.Drawing.Font("SimSun", 14.25F, System.Drawing.FontStyle.Bold);
            this.lblQQGroup.ForeColor = System.Drawing.SystemColors.ControlDark;
            this.lblQQGroup.Location = new System.Drawing.Point(380, 368);
            this.lblQQGroup.Name = "lblQQGroup";
            this.lblQQGroup.Size = new System.Drawing.Size(199, 19);
            this.lblQQGroup.TabIndex = 3;
            this.lblQQGroup.Text = "官方2群：399011190";
            this.lblQQGroup.Visible = false;
            // 
            // btnCancel
            // 
            this.btnCancel.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(93)))), ((int)(((byte)(101)))));
            this.btnCancel.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnCancel.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnCancel.ForeColor = System.Drawing.SystemColors.Control;
            this.btnCancel.Location = new System.Drawing.Point(509, 400);
            this.btnCancel.Name = "btnCancel";
            this.btnCancel.Size = new System.Drawing.Size(100, 25);
            this.btnCancel.TabIndex = 4;
            this.btnCancel.Text = "取消";
            this.btnCancel.UseVisualStyleBackColor = false;
            this.btnCancel.Click += new System.EventHandler(this.btnCancel_Click);
            // 
            // btnOK
            // 
            this.btnOK.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(94)))), ((int)(((byte)(93)))), ((int)(((byte)(101)))));
            this.btnOK.FlatStyle = System.Windows.Forms.FlatStyle.Popup;
            this.btnOK.Font = new System.Drawing.Font("SimSun", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            this.btnOK.ForeColor = System.Drawing.SystemColors.Control;
            this.btnOK.Location = new System.Drawing.Point(385, 400);
            this.btnOK.Name = "btnOK";
            this.btnOK.Size = new System.Drawing.Size(100, 25);
            this.btnOK.TabIndex = 5;
            this.btnOK.Text = "应用";
            this.btnOK.UseVisualStyleBackColor = false;
            this.btnOK.Click += new System.EventHandler(this.btnOK_Click);
            // 
            // pbClose
            // 
            this.pbClose.Anchor = ((System.Windows.Forms.AnchorStyles)((System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right)));
            this.pbClose.Location = new System.Drawing.Point(592, 2);
            this.pbClose.Name = "pbClose";
            this.pbClose.Padding = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pbClose.Size = new System.Drawing.Size(48, 41);
            this.pbClose.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pbClose.TabIndex = 10;
            this.pbClose.TabStop = false;
            this.pbClose.Click += new System.EventHandler(this.pbClose_Click);
            this.pbClose.MouseLeave += new System.EventHandler(this.pbClose_MouseLeave);
            this.pbClose.MouseHover += new System.EventHandler(this.pbClose_MouseHover);
            // 
            // UserSettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 12F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(41)))), ((int)(((byte)(39)))), ((int)(((byte)(50)))));
            this.ClientSize = new System.Drawing.Size(640, 435);
            this.Controls.Add(this.pbClose);
            this.Controls.Add(this.btnOK);
            this.Controls.Add(this.btnCancel);
            this.Controls.Add(this.lblQQGroup);
            this.Controls.Add(this.lblQQ);
            this.Controls.Add(this.gbBossKey);
            this.Controls.Add(this.gbResolution);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Name = "UserSettingsForm";
            this.ShowInTaskbar = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "取消";
            this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.UserSettingsForm_MouseDown);
            this.gbResolution.ResumeLayout(false);
            this.gbResolution.PerformLayout();
            this.gbBossKey.ResumeLayout(false);
            this.gbBossKey.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pbClose)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }

        #endregion

        private System.Windows.Forms.GroupBox gbResolution;
        private System.Windows.Forms.RadioButton rbLMode1440;
        private System.Windows.Forms.RadioButton rbLMode1280;
        private System.Windows.Forms.RadioButton rbLMode960;
        private System.Windows.Forms.GroupBox gbBossKey;
        private System.Windows.Forms.Label lblMultiply;
        private System.Windows.Forms.RadioButton rbPMode900;
        private System.Windows.Forms.RadioButton rbPMode960;
        private System.Windows.Forms.RadioButton rbPMode720;
        private System.Windows.Forms.RadioButton rbCustomize;
        private System.Windows.Forms.Label lblInputPrompt;
        private System.Windows.Forms.Label lblBossKeyName;
        private System.Windows.Forms.Label lblQQ;
        private System.Windows.Forms.Label lblQQGroup;
        private System.Windows.Forms.Button btnCancel;
        private System.Windows.Forms.Button btnOK;
        private System.Windows.Forms.PictureBox pbClose;
        private System.Windows.Forms.Label lblCustomizeResolutionPrompt;
        private UserSettingsForm.TextBoxWihtOutCopy tbHeight;
        private UserSettingsForm.TextBoxWihtOutCopy tbWide;
        private UserSettingsForm.TextBoxWihtOutCopy tbBossKey;
    }
}