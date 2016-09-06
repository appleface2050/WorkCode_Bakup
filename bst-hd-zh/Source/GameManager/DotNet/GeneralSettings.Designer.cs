namespace BlueStacks.hyperDroid.GameManager
{
    partial class GeneralSettings
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

        #region Component Designer generated code

        /// <summary> 
        /// Required method for Designer support - do not modify 
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
			this.pbExitMenu = new System.Windows.Forms.PictureBox();
			this.pbSideBar = new System.Windows.Forms.PictureBox();
			this.cmbBxChangeResolution = new System.Windows.Forms.ComboBox();
			this.lblShowSideBar = new System.Windows.Forms.Label();
			this.lblShowExitMenu = new System.Windows.Forms.Label();
			this.lblChangeResolution = new System.Windows.Forms.Label();
			this.lblNote = new System.Windows.Forms.Label();
			this.pbStayAwake = new System.Windows.Forms.PictureBox();
			this.lblStayAwake = new System.Windows.Forms.Label();
			this.lblLanguage = new System.Windows.Forms.Label();
			this.cmbBxLanguage = new System.Windows.Forms.ComboBox();
			this.pbAutoStart = new System.Windows.Forms.PictureBox();
			this.lblAutoStart = new System.Windows.Forms.Label();
			this.pbForceKill = new System.Windows.Forms.PictureBox();
			this.lblForceKill = new System.Windows.Forms.Label();
			this.pbGamePad = new System.Windows.Forms.PictureBox();
			this.lblConfigureGamePad = new System.Windows.Forms.Label();
			this.grpbxBluestacksPreferences = new System.Windows.Forms.GroupBox();
			this.tableLayoutPanel1 = new System.Windows.Forms.TableLayoutPanel();
			this.pbHidTabBar = new System.Windows.Forms.PictureBox();
			this.lblHideTopBar = new System.Windows.Forms.Label();
			((System.ComponentModel.ISupportInitialize)(this.pbExitMenu)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbSideBar)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbStayAwake)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbAutoStart)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbForceKill)).BeginInit();
			((System.ComponentModel.ISupportInitialize)(this.pbGamePad)).BeginInit();
			this.grpbxBluestacksPreferences.SuspendLayout();
			this.tableLayoutPanel1.SuspendLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbHidTabBar)).BeginInit();
			this.SuspendLayout();
			// 
			// pbExitMenu
			// 
			this.pbExitMenu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbExitMenu.Location = new System.Drawing.Point(7, 291);
			this.pbExitMenu.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.pbExitMenu.Name = "pbExitMenu";
			this.pbExitMenu.Size = new System.Drawing.Size(41, 28);
			this.pbExitMenu.TabIndex = 4;
			this.pbExitMenu.TabStop = false;
			this.pbExitMenu.Click += new System.EventHandler(this.ExitMenuPictureBoxClicked);
			// 
			// pbSideBar
			// 
			this.pbSideBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbSideBar.Location = new System.Drawing.Point(7, 120);
			this.pbSideBar.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.pbSideBar.Name = "pbSideBar";
			this.pbSideBar.Size = new System.Drawing.Size(41, 28);
			this.pbSideBar.TabIndex = 3;
			this.pbSideBar.TabStop = false;
			this.pbSideBar.Click += new System.EventHandler(this.SideBarPictureBoxClicked);
			// 
			// cmbBxChangeResolution
			// 
			this.cmbBxChangeResolution.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbBxChangeResolution.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbBxChangeResolution.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmbBxChangeResolution.FormattingEnabled = true;
			this.cmbBxChangeResolution.Items.AddRange(new object[] {
            "1280 * 720",
            "1600 * 900",
            "1920 * 1080",
            "2560 * 1440"});
			this.cmbBxChangeResolution.Location = new System.Drawing.Point(274, 6);
			this.cmbBxChangeResolution.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.cmbBxChangeResolution.Name = "cmbBxChangeResolution";
			this.cmbBxChangeResolution.Size = new System.Drawing.Size(358, 40);
			this.cmbBxChangeResolution.TabIndex = 2;
			this.cmbBxChangeResolution.SelectedIndexChanged += new System.EventHandler(this.cmbBxChangeResolutionSelectedIndexChanged);
			// 
			// lblShowSideBar
			// 
			this.lblShowSideBar.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblShowSideBar, 3);
			this.lblShowSideBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblShowSideBar.Location = new System.Drawing.Point(62, 114);
			this.lblShowSideBar.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblShowSideBar.Name = "lblShowSideBar";
			this.lblShowSideBar.Size = new System.Drawing.Size(822, 40);
			this.lblShowSideBar.TabIndex = 6;
			this.lblShowSideBar.Text = "Show side bar";
			this.lblShowSideBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblShowExitMenu
			// 
			this.lblShowExitMenu.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblShowExitMenu, 3);
			this.lblShowExitMenu.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblShowExitMenu.Location = new System.Drawing.Point(62, 285);
			this.lblShowExitMenu.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblShowExitMenu.Name = "lblShowExitMenu";
			this.lblShowExitMenu.Size = new System.Drawing.Size(822, 40);
			this.lblShowExitMenu.TabIndex = 5;
			this.lblShowExitMenu.Text = "Show Exit Menu";
			this.lblShowExitMenu.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblChangeResolution
			// 
			this.lblChangeResolution.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblChangeResolution, 2);
			this.lblChangeResolution.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblChangeResolution.Location = new System.Drawing.Point(7, 0);
			this.lblChangeResolution.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblChangeResolution.Name = "lblChangeResolution";
			this.lblChangeResolution.Size = new System.Drawing.Size(253, 40);
			this.lblChangeResolution.TabIndex = 7;
			this.lblChangeResolution.Text = "Window size";
			this.lblChangeResolution.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblNote
			// 
			this.lblNote.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblNote, 3);
			this.lblNote.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblNote.Location = new System.Drawing.Point(7, 513);
			this.lblNote.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblNote.Name = "lblNote";
			this.lblNote.Size = new System.Drawing.Size(625, 46);
			this.lblNote.TabIndex = 8;
			this.lblNote.Text = "Settings will require application restart.";
			this.lblNote.TextAlign = System.Drawing.ContentAlignment.BottomLeft;
			// 
			// pbStayAwake
			// 
			this.pbStayAwake.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbStayAwake.Location = new System.Drawing.Point(7, 177);
			this.pbStayAwake.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.pbStayAwake.Name = "pbStayAwake";
			this.pbStayAwake.Size = new System.Drawing.Size(41, 28);
			this.pbStayAwake.TabIndex = 9;
			this.pbStayAwake.TabStop = false;
			this.pbStayAwake.Click += new System.EventHandler(this.StayAwakePictureBoxClicked);
			// 
			// lblStayAwake
			// 
			this.lblStayAwake.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblStayAwake, 3);
			this.lblStayAwake.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblStayAwake.Location = new System.Drawing.Point(62, 171);
			this.lblStayAwake.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblStayAwake.Name = "lblStayAwake";
			this.lblStayAwake.Size = new System.Drawing.Size(822, 40);
			this.lblStayAwake.TabIndex = 12;
			this.lblStayAwake.Text = "Stay awake";
			this.lblStayAwake.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// lblLanguage
			// 
			this.lblLanguage.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblLanguage, 2);
			this.lblLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblLanguage.Location = new System.Drawing.Point(7, 57);
			this.lblLanguage.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblLanguage.Name = "lblLanguage";
			this.lblLanguage.Size = new System.Drawing.Size(253, 40);
			this.lblLanguage.TabIndex = 11;
			this.lblLanguage.Text = "Language";
			this.lblLanguage.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// cmbBxLanguage
			// 
			this.cmbBxLanguage.Dock = System.Windows.Forms.DockStyle.Fill;
			this.cmbBxLanguage.DropDownHeight = 200;
			this.cmbBxLanguage.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.cmbBxLanguage.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
			this.cmbBxLanguage.FormattingEnabled = true;
			this.cmbBxLanguage.IntegralHeight = false;
			this.cmbBxLanguage.Items.AddRange(new object[] {
            "ar-EG",
            "ar-IL",
            "cs-CZ",
            "da-DK",
            "de-DE",
            "el-GR",
            "en-US",
            "es-ES",
            "fi-FI",
            "fr-FR",
            "hr-HR",
            "hu-HU",
            "in-ID",
            "it-IT",
            "ja-JP",
            "ko-KR",
            "nb-NO",
            "nl-BE",
            "nl-NL",
            "pl-PL",
            "pt-BR",
            "pt-PT",
            "ro-RO",
            "ru-RU",
            "sk-SK",
            "sl-SI",
            "sv-SE",
            "th-TH",
            "tr-TR",
            "vi-VN",
            "zh-CN",
            "zh-TW"});
			this.cmbBxLanguage.Location = new System.Drawing.Point(274, 63);
			this.cmbBxLanguage.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.cmbBxLanguage.Name = "cmbBxLanguage";
			this.cmbBxLanguage.Size = new System.Drawing.Size(358, 40);
			this.cmbBxLanguage.TabIndex = 13;
			this.cmbBxLanguage.SelectedIndexChanged += new System.EventHandler(this.cmbBxLanguageSelectedIndexChanged);
			// 
			// pbAutoStart
			// 
			this.pbAutoStart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbAutoStart.Location = new System.Drawing.Point(7, 234);
			this.pbAutoStart.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.pbAutoStart.Name = "pbAutoStart";
			this.pbAutoStart.Size = new System.Drawing.Size(41, 28);
			this.pbAutoStart.TabIndex = 14;
			this.pbAutoStart.TabStop = false;
			this.pbAutoStart.Click += new System.EventHandler(this.AutoStartPictureBoxClicked);
			// 
			// lblAutoStart
			// 
			this.lblAutoStart.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblAutoStart, 3);
			this.lblAutoStart.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblAutoStart.Location = new System.Drawing.Point(62, 228);
			this.lblAutoStart.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblAutoStart.Name = "lblAutoStart";
			this.lblAutoStart.Size = new System.Drawing.Size(822, 40);
			this.lblAutoStart.TabIndex = 15;
			this.lblAutoStart.Text = "Auto start on windows start";
			this.lblAutoStart.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pbForceKill
			// 
			this.pbForceKill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbForceKill.Location = new System.Drawing.Point(7, 348);
			this.pbForceKill.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.pbForceKill.Name = "pbForceKill";
			this.pbForceKill.Size = new System.Drawing.Size(41, 28);
			this.pbForceKill.TabIndex = 16;
			this.pbForceKill.TabStop = false;
			this.pbForceKill.Click += new System.EventHandler(this.ForceKillPictureBoxClicked);
			// 
			// lblForceKill
			// 
			this.lblForceKill.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblForceKill, 3);
			this.lblForceKill.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblForceKill.Location = new System.Drawing.Point(62, 342);
			this.lblForceKill.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblForceKill.Name = "lblForceKill";
			this.lblForceKill.Size = new System.Drawing.Size(822, 40);
			this.lblForceKill.TabIndex = 17;
			this.lblForceKill.Text = "Force kill on tab close";
			this.lblForceKill.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// pbGamePad
			// 
			this.pbGamePad.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbGamePad.Location = new System.Drawing.Point(7, 405);
			this.pbGamePad.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.pbGamePad.Name = "pbGamePad";
			this.pbGamePad.Size = new System.Drawing.Size(41, 28);
			this.pbGamePad.TabIndex = 18;
			this.pbGamePad.TabStop = false;
			this.pbGamePad.Click += new System.EventHandler(this.GamePadPictureBoxClicked);
			// 
			// lblConfigureGamePad
			// 
			this.lblConfigureGamePad.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblConfigureGamePad, 3);
			this.lblConfigureGamePad.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblConfigureGamePad.Location = new System.Drawing.Point(62, 399);
			this.lblConfigureGamePad.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblConfigureGamePad.Name = "lblConfigureGamePad";
			this.lblConfigureGamePad.Size = new System.Drawing.Size(822, 40);
			this.lblConfigureGamePad.TabIndex = 19;
			this.lblConfigureGamePad.Text = "Enable GamePad";
			this.lblConfigureGamePad.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// grpbxBluestacksPreferences
			// 
			this.grpbxBluestacksPreferences.Controls.Add(this.tableLayoutPanel1);
			this.grpbxBluestacksPreferences.Dock = System.Windows.Forms.DockStyle.Fill;
			this.grpbxBluestacksPreferences.Location = new System.Drawing.Point(0, 0);
			this.grpbxBluestacksPreferences.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.grpbxBluestacksPreferences.Name = "grpbxBluestacksPreferences";
			this.grpbxBluestacksPreferences.Padding = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.grpbxBluestacksPreferences.Size = new System.Drawing.Size(905, 603);
			this.grpbxBluestacksPreferences.TabIndex = 8;
			this.grpbxBluestacksPreferences.TabStop = false;
			this.grpbxBluestacksPreferences.Text = "BlueStacks Preferences";
			// 
			// tableLayoutPanel1
			// 
			this.tableLayoutPanel1.AutoScroll = true;
			this.tableLayoutPanel1.ColumnCount = 4;
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 55F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 25.3819F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 44.53584F));
			this.tableLayoutPanel1.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.Controls.Add(this.lblConfigureGamePad, 1, 14);
			this.tableLayoutPanel1.Controls.Add(this.lblLanguage, 0, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblForceKill, 1, 12);
			this.tableLayoutPanel1.Controls.Add(this.pbGamePad, 0, 14);
			this.tableLayoutPanel1.Controls.Add(this.lblAutoStart, 1, 8);
			this.tableLayoutPanel1.Controls.Add(this.lblShowExitMenu, 1, 10);
			this.tableLayoutPanel1.Controls.Add(this.pbSideBar, 0, 4);
			this.tableLayoutPanel1.Controls.Add(this.pbAutoStart, 0, 8);
			this.tableLayoutPanel1.Controls.Add(this.pbForceKill, 0, 12);
			this.tableLayoutPanel1.Controls.Add(this.lblNote, 0, 18);
			this.tableLayoutPanel1.Controls.Add(this.lblShowSideBar, 1, 4);
			this.tableLayoutPanel1.Controls.Add(this.pbStayAwake, 0, 6);
			this.tableLayoutPanel1.Controls.Add(this.pbExitMenu, 0, 10);
			this.tableLayoutPanel1.Controls.Add(this.lblStayAwake, 1, 6);
			this.tableLayoutPanel1.Controls.Add(this.cmbBxChangeResolution, 2, 0);
			this.tableLayoutPanel1.Controls.Add(this.cmbBxLanguage, 2, 2);
			this.tableLayoutPanel1.Controls.Add(this.lblChangeResolution, 0, 0);
			this.tableLayoutPanel1.Controls.Add(this.pbHidTabBar, 0, 16);
			this.tableLayoutPanel1.Controls.Add(this.lblHideTopBar, 1, 16);
			this.tableLayoutPanel1.Dock = System.Windows.Forms.DockStyle.Fill;
			this.tableLayoutPanel1.Location = new System.Drawing.Point(7, 38);
			this.tableLayoutPanel1.Name = "tableLayoutPanel1";
			this.tableLayoutPanel1.RowCount = 19;
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, 30F));
			this.tableLayoutPanel1.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, 40F));
			this.tableLayoutPanel1.Size = new System.Drawing.Size(891, 559);
			this.tableLayoutPanel1.TabIndex = 20;
			this.tableLayoutPanel1.Paint += new System.Windows.Forms.PaintEventHandler(this.tableLayoutPanel1_Paint);
			// 
			// pbHidTabBar
			// 
			this.pbHidTabBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.pbHidTabBar.Location = new System.Drawing.Point(7, 462);
			this.pbHidTabBar.Margin = new System.Windows.Forms.Padding(7, 6, 7, 6);
			this.pbHidTabBar.Name = "pbHidTabBar";
			this.pbHidTabBar.Size = new System.Drawing.Size(41, 28);
			this.pbHidTabBar.TabIndex = 20;
			this.pbHidTabBar.TabStop = false;
			this.pbHidTabBar.Click += new System.EventHandler(this.HideTabBarPictureBoxClicked);
			// 
			// lblHideTopBar
			// 
			this.lblHideTopBar.AutoSize = true;
			this.tableLayoutPanel1.SetColumnSpan(this.lblHideTopBar, 3);
			this.lblHideTopBar.Dock = System.Windows.Forms.DockStyle.Fill;
			this.lblHideTopBar.Location = new System.Drawing.Point(62, 456);
			this.lblHideTopBar.Margin = new System.Windows.Forms.Padding(7, 0, 7, 0);
			this.lblHideTopBar.Name = "lblHideTopBar";
			this.lblHideTopBar.Size = new System.Drawing.Size(822, 40);
			this.lblHideTopBar.TabIndex = 21;
			this.lblHideTopBar.Text = "Hide tabs in full screen";
			this.lblHideTopBar.TextAlign = System.Drawing.ContentAlignment.MiddleLeft;
			// 
			// GeneralSettings
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 32F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.Controls.Add(this.grpbxBluestacksPreferences);
			this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
			this.Margin = new System.Windows.Forms.Padding(1, 2, 1, 2);
			this.Name = "GeneralSettings";
			this.Size = new System.Drawing.Size(905, 603);
			this.Load += new System.EventHandler(this.GeneralSettings_Load);
			((System.ComponentModel.ISupportInitialize)(this.pbExitMenu)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbSideBar)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbStayAwake)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbAutoStart)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbForceKill)).EndInit();
			((System.ComponentModel.ISupportInitialize)(this.pbGamePad)).EndInit();
			this.grpbxBluestacksPreferences.ResumeLayout(false);
			this.tableLayoutPanel1.ResumeLayout(false);
			this.tableLayoutPanel1.PerformLayout();
			((System.ComponentModel.ISupportInitialize)(this.pbHidTabBar)).EndInit();
			this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.PictureBox pbExitMenu;
        private System.Windows.Forms.PictureBox pbSideBar;
        private System.Windows.Forms.ComboBox cmbBxChangeResolution;
        private System.Windows.Forms.Label lblShowSideBar;
        private System.Windows.Forms.Label lblShowExitMenu;
        private System.Windows.Forms.Label lblChangeResolution;
        private System.Windows.Forms.Label lblNote;
        private System.Windows.Forms.PictureBox pbStayAwake;
        private System.Windows.Forms.Label lblStayAwake;
        private System.Windows.Forms.Label lblLanguage;
        private System.Windows.Forms.ComboBox cmbBxLanguage;
        private System.Windows.Forms.PictureBox pbAutoStart;
        private System.Windows.Forms.Label lblAutoStart;
        private System.Windows.Forms.PictureBox pbForceKill;
        private System.Windows.Forms.Label lblForceKill;
        private System.Windows.Forms.PictureBox pbGamePad;
        private System.Windows.Forms.Label lblConfigureGamePad;
        private System.Windows.Forms.GroupBox grpbxBluestacksPreferences;
        private System.Windows.Forms.TableLayoutPanel tableLayoutPanel1;
		private System.Windows.Forms.PictureBox pbHidTabBar;
		private System.Windows.Forms.Label lblHideTopBar;


    }
}