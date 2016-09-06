using System.Drawing;
using System.Windows.Forms;
namespace BlueStacks.hyperDroid.GameManager
{
    partial class ThemeSettings
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
            this.panel1 = new System.Windows.Forms.Panel();
            this.grpControlBar = new System.Windows.Forms.GroupBox();
            this.ChangeControlBarBottomColorLbl = new System.Windows.Forms.Label();
            this.ChangeControlBarBottomColorBtn = new System.Windows.Forms.Button();
            this.ChangeControlBarTopColorLbl = new System.Windows.Forms.Label();
            this.ChangeControlBarTopColorBtn = new System.Windows.Forms.Button();
            this.grpBorder = new System.Windows.Forms.GroupBox();
            this.ChangeInnerBorderColorLbl = new System.Windows.Forms.Label();
            this.ChangeTabBorderLbl = new System.Windows.Forms.Label();
            this.ChangeToolBarBorderColorLbl = new System.Windows.Forms.Label();
            this.ChangeGameManagerBorderLbl = new System.Windows.Forms.Label();
            this.ChangeToolBarBorderColorBtn = new System.Windows.Forms.Button();
            this.ChangeInnerBorderColorBtn = new System.Windows.Forms.Button();
            this.ChangeTabBorderBtn = new System.Windows.Forms.Button();
            this.ChangeGameManagerBorderBtn = new System.Windows.Forms.Button();
            this.grpTab = new System.Windows.Forms.GroupBox();
            this.ChangeTabTopColorMouseOverLbl = new System.Windows.Forms.Label();
            this.ChangeInactiveTabBottomColorLbl = new System.Windows.Forms.Label();
            this.ChangeTabBottomColorMouseOverLbl = new System.Windows.Forms.Label();
            this.ChangeInactiveTabTopColorLbl = new System.Windows.Forms.Label();
            this.ChangeSelectedTabBottomColorLbl = new System.Windows.Forms.Label();
            this.ChangeSelectedTabTopColorLbl = new System.Windows.Forms.Label();
            this.ChangeTabTopColorMouseOverBtn = new System.Windows.Forms.Button();
            this.ChangeTabBottomColorMouseOverBtn = new System.Windows.Forms.Button();
            this.ChangeInactiveTabBottomColorBtn = new System.Windows.Forms.Button();
            this.ChangeSelectedTabBottomColorBtn = new System.Windows.Forms.Button();
            this.ChangeSelectedTabTopColorBtn = new System.Windows.Forms.Button();
            this.ChangeInactiveTabTopColorBtn = new System.Windows.Forms.Button();
            this.grpText = new System.Windows.Forms.GroupBox();
            this.ChangeSelectedTabTextColorLbl = new System.Windows.Forms.Label();
            this.ChangeMouseOverTabTextColorLbl = new System.Windows.Forms.Label();
            this.ChangeInactiveTabTextColorLbl = new System.Windows.Forms.Label();
            this.ChangeMouseOverTabTextColorBtn = new System.Windows.Forms.Button();
            this.ChangeSelectedTabTextColorBtn = new System.Windows.Forms.Button();
            this.ChangeInactiveTabTextColorBtn = new System.Windows.Forms.Button();
            this.grpTabBar = new System.Windows.Forms.GroupBox();
            this.ChangeTabBarBottomColorLbl = new System.Windows.Forms.Label();
            this.ChangeTabBarTopColorLbl = new System.Windows.Forms.Label();
            this.ChangeTabBarBottomColorBtn = new System.Windows.Forms.Button();
            this.ChangeTabBarTopColorBtn = new System.Windows.Forms.Button();
            this.grpLeftSideBar = new System.Windows.Forms.GroupBox();
            this.ChangeToolBarBottomColorLbl = new System.Windows.Forms.Label();
            this.ChangeToolBarTopColorLbl = new System.Windows.Forms.Label();
            this.ChangeToolBarBottomColorBtn = new System.Windows.Forms.Button();
            this.ChangeToolBarTopColorBtn = new System.Windows.Forms.Button();
            this.grpSettingsMenu = new System.Windows.Forms.GroupBox();
            this.ChangeContextMenuOverColorLbl = new System.Windows.Forms.Label();
            this.ChangeContextMenuForeColorLbl = new System.Windows.Forms.Label();
            this.ChangeContextMenuBackColorLbl = new System.Windows.Forms.Label();
            this.ChangeContextMenuOverColorBtn = new System.Windows.Forms.Button();
            this.ChangeContextMenuForeColorBtn = new System.Windows.Forms.Button();
            this.ChangeContextMenuBackColorBtn = new System.Windows.Forms.Button();
            this.grpChooseProfile = new System.Windows.Forms.GroupBox();
            this.NewThemeBtn = new System.Windows.Forms.Button();
            this.lblSelectTheme = new System.Windows.Forms.Label();
            this.cmbTheme = new System.Windows.Forms.ComboBox();
            this.groupBoxStyle = new System.Windows.Forms.GroupBox();
            this.pictureBoxSelected = new System.Windows.Forms.PictureBox();
            this.pictureBoxNewStyle = new System.Windows.Forms.PictureBox();
            this.pictureBoxOldStyle = new System.Windows.Forms.PictureBox();
            this.Applybtn = new System.Windows.Forms.Button();
            this.panel1.SuspendLayout();
            this.grpControlBar.SuspendLayout();
            this.grpBorder.SuspendLayout();
            this.grpTab.SuspendLayout();
            this.grpText.SuspendLayout();
            this.grpTabBar.SuspendLayout();
            this.grpLeftSideBar.SuspendLayout();
            this.grpSettingsMenu.SuspendLayout();
            this.grpChooseProfile.SuspendLayout();
            this.groupBoxStyle.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelected)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNewStyle)).BeginInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOldStyle)).BeginInit();
            this.SuspendLayout();
            // 
            // panel1
            // 
            this.panel1.AutoScroll = true;
            this.panel1.AutoSize = true;
            this.panel1.Controls.Add(this.grpControlBar);
            this.panel1.Controls.Add(this.grpBorder);
            this.panel1.Controls.Add(this.grpTab);
            this.panel1.Controls.Add(this.grpText);
            this.panel1.Controls.Add(this.grpTabBar);
            this.panel1.Controls.Add(this.grpLeftSideBar);
            this.panel1.Controls.Add(this.grpSettingsMenu);
            this.panel1.Controls.Add(this.grpChooseProfile);
            this.panel1.Controls.Add(this.groupBoxStyle);
            this.panel1.Controls.Add(this.Applybtn);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.panel1.Location = new System.Drawing.Point(0, 0);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(1101, 1489);
            this.panel1.TabIndex = 0;
            // 
            // grpControlBar
            // 
            this.grpControlBar.Controls.Add(this.ChangeControlBarBottomColorLbl);
            this.grpControlBar.Controls.Add(this.ChangeControlBarBottomColorBtn);
            this.grpControlBar.Controls.Add(this.ChangeControlBarTopColorLbl);
            this.grpControlBar.Controls.Add(this.ChangeControlBarTopColorBtn);
            this.grpControlBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpControlBar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grpControlBar.ForeColor = System.Drawing.Color.White;
            this.grpControlBar.Location = new System.Drawing.Point(0, 1078);
            this.grpControlBar.Name = "grpControlBar";
            this.grpControlBar.Size = new System.Drawing.Size(1101, 98);
            this.grpControlBar.TabIndex = 27;
            this.grpControlBar.TabStop = false;
            this.grpControlBar.Text = "grpControlBar";
            this.grpControlBar.Visible = false;
            // 
            // ChangeControlBarBottomColorLbl
            // 
            this.ChangeControlBarBottomColorLbl.AutoSize = true;
            this.ChangeControlBarBottomColorLbl.Location = new System.Drawing.Point(622, 43);
            this.ChangeControlBarBottomColorLbl.Name = "ChangeControlBarBottomColorLbl";
            this.ChangeControlBarBottomColorLbl.Size = new System.Drawing.Size(222, 32);
            this.ChangeControlBarBottomColorLbl.TabIndex = 23;
            this.ChangeControlBarBottomColorLbl.Text = "Bottom Gradient";
            // 
            // ChangeControlBarBottomColorBtn
            // 
            this.ChangeControlBarBottomColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeControlBarBottomColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeControlBarBottomColorBtn.Location = new System.Drawing.Point(1003, 38);
            this.ChangeControlBarBottomColorBtn.Name = "ChangeControlBarBottomColorBtn";
            this.ChangeControlBarBottomColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeControlBarBottomColorBtn.TabIndex = 7;
            this.ChangeControlBarBottomColorBtn.UseVisualStyleBackColor = true;
            this.ChangeControlBarBottomColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeControlBarTopColorLbl
            // 
            this.ChangeControlBarTopColorLbl.AutoSize = true;
            this.ChangeControlBarTopColorLbl.Location = new System.Drawing.Point(51, 46);
            this.ChangeControlBarTopColorLbl.Name = "ChangeControlBarTopColorLbl";
            this.ChangeControlBarTopColorLbl.Size = new System.Drawing.Size(181, 32);
            this.ChangeControlBarTopColorLbl.TabIndex = 22;
            this.ChangeControlBarTopColorLbl.Text = "Top Gradient";
            // 
            // ChangeControlBarTopColorBtn
            // 
            this.ChangeControlBarTopColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeControlBarTopColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeControlBarTopColorBtn.Location = new System.Drawing.Point(450, 40);
            this.ChangeControlBarTopColorBtn.Name = "ChangeControlBarTopColorBtn";
            this.ChangeControlBarTopColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeControlBarTopColorBtn.TabIndex = 6;
            this.ChangeControlBarTopColorBtn.UseVisualStyleBackColor = true;
            this.ChangeControlBarTopColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // grpBorder
            // 
            this.grpBorder.Controls.Add(this.ChangeInnerBorderColorLbl);
            this.grpBorder.Controls.Add(this.ChangeTabBorderLbl);
            this.grpBorder.Controls.Add(this.ChangeToolBarBorderColorLbl);
            this.grpBorder.Controls.Add(this.ChangeGameManagerBorderLbl);
            this.grpBorder.Controls.Add(this.ChangeToolBarBorderColorBtn);
            this.grpBorder.Controls.Add(this.ChangeInnerBorderColorBtn);
            this.grpBorder.Controls.Add(this.ChangeTabBorderBtn);
            this.grpBorder.Controls.Add(this.ChangeGameManagerBorderBtn);
            this.grpBorder.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpBorder.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grpBorder.ForeColor = System.Drawing.Color.White;
            this.grpBorder.Location = new System.Drawing.Point(0, 926);
            this.grpBorder.Name = "grpBorder";
            this.grpBorder.Size = new System.Drawing.Size(1101, 152);
            this.grpBorder.TabIndex = 22;
            this.grpBorder.TabStop = false;
            this.grpBorder.Text = "grpBorder";
            this.grpBorder.Visible = false;
            // 
            // ChangeInnerBorderColorLbl
            // 
            this.ChangeInnerBorderColorLbl.AutoSize = true;
            this.ChangeInnerBorderColorLbl.Location = new System.Drawing.Point(622, 98);
            this.ChangeInnerBorderColorLbl.Name = "ChangeInnerBorderColorLbl";
            this.ChangeInnerBorderColorLbl.Size = new System.Drawing.Size(171, 32);
            this.ChangeInnerBorderColorLbl.TabIndex = 20;
            this.ChangeInnerBorderColorLbl.Text = "Inner Border";
            // 
            // ChangeTabBorderLbl
            // 
            this.ChangeTabBorderLbl.AutoSize = true;
            this.ChangeTabBorderLbl.Location = new System.Drawing.Point(51, 93);
            this.ChangeTabBorderLbl.Name = "ChangeTabBorderLbl";
            this.ChangeTabBorderLbl.Size = new System.Drawing.Size(156, 32);
            this.ChangeTabBorderLbl.TabIndex = 19;
            this.ChangeTabBorderLbl.Text = "Tab Border";
            // 
            // ChangeToolBarBorderColorLbl
            // 
            this.ChangeToolBarBorderColorLbl.AutoSize = true;
            this.ChangeToolBarBorderColorLbl.Location = new System.Drawing.Point(622, 46);
            this.ChangeToolBarBorderColorLbl.Name = "ChangeToolBarBorderColorLbl";
            this.ChangeToolBarBorderColorLbl.Size = new System.Drawing.Size(207, 32);
            this.ChangeToolBarBorderColorLbl.TabIndex = 18;
            this.ChangeToolBarBorderColorLbl.Text = "ToolBar Border";
            // 
            // ChangeGameManagerBorderLbl
            // 
            this.ChangeGameManagerBorderLbl.AutoSize = true;
            this.ChangeGameManagerBorderLbl.Location = new System.Drawing.Point(51, 46);
            this.ChangeGameManagerBorderLbl.Name = "ChangeGameManagerBorderLbl";
            this.ChangeGameManagerBorderLbl.Size = new System.Drawing.Size(296, 32);
            this.ChangeGameManagerBorderLbl.TabIndex = 17;
            this.ChangeGameManagerBorderLbl.Text = "GameManager Border";
            // 
            // ChangeToolBarBorderColorBtn
            // 
            this.ChangeToolBarBorderColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeToolBarBorderColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeToolBarBorderColorBtn.Location = new System.Drawing.Point(1003, 40);
            this.ChangeToolBarBorderColorBtn.Name = "ChangeToolBarBorderColorBtn";
            this.ChangeToolBarBorderColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeToolBarBorderColorBtn.TabIndex = 23;
            this.ChangeToolBarBorderColorBtn.UseVisualStyleBackColor = true;
            this.ChangeToolBarBorderColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeInnerBorderColorBtn
            // 
            this.ChangeInnerBorderColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeInnerBorderColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeInnerBorderColorBtn.Location = new System.Drawing.Point(1003, 88);
            this.ChangeInnerBorderColorBtn.Name = "ChangeInnerBorderColorBtn";
            this.ChangeInnerBorderColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeInnerBorderColorBtn.TabIndex = 25;
            this.ChangeInnerBorderColorBtn.UseVisualStyleBackColor = true;
            this.ChangeInnerBorderColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeTabBorderBtn
            // 
            this.ChangeTabBorderBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeTabBorderBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeTabBorderBtn.Location = new System.Drawing.Point(450, 93);
            this.ChangeTabBorderBtn.Name = "ChangeTabBorderBtn";
            this.ChangeTabBorderBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeTabBorderBtn.TabIndex = 24;
            this.ChangeTabBorderBtn.UseVisualStyleBackColor = true;
            this.ChangeTabBorderBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeGameManagerBorderBtn
            // 
            this.ChangeGameManagerBorderBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeGameManagerBorderBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeGameManagerBorderBtn.Location = new System.Drawing.Point(450, 40);
            this.ChangeGameManagerBorderBtn.Name = "ChangeGameManagerBorderBtn";
            this.ChangeGameManagerBorderBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeGameManagerBorderBtn.TabIndex = 22;
            this.ChangeGameManagerBorderBtn.UseVisualStyleBackColor = true;
            this.ChangeGameManagerBorderBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // grpTab
            // 
            this.grpTab.Controls.Add(this.ChangeTabTopColorMouseOverLbl);
            this.grpTab.Controls.Add(this.ChangeInactiveTabBottomColorLbl);
            this.grpTab.Controls.Add(this.ChangeTabBottomColorMouseOverLbl);
            this.grpTab.Controls.Add(this.ChangeInactiveTabTopColorLbl);
            this.grpTab.Controls.Add(this.ChangeSelectedTabBottomColorLbl);
            this.grpTab.Controls.Add(this.ChangeSelectedTabTopColorLbl);
            this.grpTab.Controls.Add(this.ChangeTabTopColorMouseOverBtn);
            this.grpTab.Controls.Add(this.ChangeTabBottomColorMouseOverBtn);
            this.grpTab.Controls.Add(this.ChangeInactiveTabBottomColorBtn);
            this.grpTab.Controls.Add(this.ChangeSelectedTabBottomColorBtn);
            this.grpTab.Controls.Add(this.ChangeSelectedTabTopColorBtn);
            this.grpTab.Controls.Add(this.ChangeInactiveTabTopColorBtn);
            this.grpTab.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTab.ForeColor = System.Drawing.Color.White;
            this.grpTab.Location = new System.Drawing.Point(0, 723);
            this.grpTab.Name = "grpTab";
            this.grpTab.Size = new System.Drawing.Size(1101, 203);
            this.grpTab.TabIndex = 21;
            this.grpTab.TabStop = false;
            this.grpTab.Text = "grpTab";
            this.grpTab.Visible = false;
            // 
            // ChangeTabTopColorMouseOverLbl
            // 
            this.ChangeTabTopColorMouseOverLbl.AutoSize = true;
            this.ChangeTabTopColorMouseOverLbl.Location = new System.Drawing.Point(622, 147);
            this.ChangeTabTopColorMouseOverLbl.Name = "ChangeTabTopColorMouseOverLbl";
            this.ChangeTabTopColorMouseOverLbl.Size = new System.Drawing.Size(273, 32);
            this.ChangeTabTopColorMouseOverLbl.TabIndex = 25;
            this.ChangeTabTopColorMouseOverLbl.Text = "Tab Top MouseOver";
            // 
            // ChangeInactiveTabBottomColorLbl
            // 
            this.ChangeInactiveTabBottomColorLbl.AutoSize = true;
            this.ChangeInactiveTabBottomColorLbl.Location = new System.Drawing.Point(622, 101);
            this.ChangeInactiveTabBottomColorLbl.Name = "ChangeInactiveTabBottomColorLbl";
            this.ChangeInactiveTabBottomColorLbl.Size = new System.Drawing.Size(266, 32);
            this.ChangeInactiveTabBottomColorLbl.TabIndex = 26;
            this.ChangeInactiveTabBottomColorLbl.Text = "Inactive Tab Bottom";
            // 
            // ChangeTabBottomColorMouseOverLbl
            // 
            this.ChangeTabBottomColorMouseOverLbl.AutoSize = true;
            this.ChangeTabBottomColorMouseOverLbl.Location = new System.Drawing.Point(51, 150);
            this.ChangeTabBottomColorMouseOverLbl.Name = "ChangeTabBottomColorMouseOverLbl";
            this.ChangeTabBottomColorMouseOverLbl.Size = new System.Drawing.Size(314, 32);
            this.ChangeTabBottomColorMouseOverLbl.TabIndex = 24;
            this.ChangeTabBottomColorMouseOverLbl.Text = "Tab Bottom MouseOver";
            // 
            // ChangeInactiveTabTopColorLbl
            // 
            this.ChangeInactiveTabTopColorLbl.AutoSize = true;
            this.ChangeInactiveTabTopColorLbl.Location = new System.Drawing.Point(51, 101);
            this.ChangeInactiveTabTopColorLbl.Name = "ChangeInactiveTabTopColorLbl";
            this.ChangeInactiveTabTopColorLbl.Size = new System.Drawing.Size(225, 32);
            this.ChangeInactiveTabTopColorLbl.TabIndex = 25;
            this.ChangeInactiveTabTopColorLbl.Text = "Inactive Tab Top";
            // 
            // ChangeSelectedTabBottomColorLbl
            // 
            this.ChangeSelectedTabBottomColorLbl.AutoSize = true;
            this.ChangeSelectedTabBottomColorLbl.Location = new System.Drawing.Point(622, 48);
            this.ChangeSelectedTabBottomColorLbl.Name = "ChangeSelectedTabBottomColorLbl";
            this.ChangeSelectedTabBottomColorLbl.Size = new System.Drawing.Size(280, 32);
            this.ChangeSelectedTabBottomColorLbl.TabIndex = 24;
            this.ChangeSelectedTabBottomColorLbl.Text = "Selected Tab Bottom";
            // 
            // ChangeSelectedTabTopColorLbl
            // 
            this.ChangeSelectedTabTopColorLbl.AutoSize = true;
            this.ChangeSelectedTabTopColorLbl.Location = new System.Drawing.Point(51, 51);
            this.ChangeSelectedTabTopColorLbl.Name = "ChangeSelectedTabTopColorLbl";
            this.ChangeSelectedTabTopColorLbl.Size = new System.Drawing.Size(239, 32);
            this.ChangeSelectedTabTopColorLbl.TabIndex = 23;
            this.ChangeSelectedTabTopColorLbl.Text = "Selected Tab Top";
            // 
            // ChangeTabTopColorMouseOverBtn
            // 
            this.ChangeTabTopColorMouseOverBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeTabTopColorMouseOverBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeTabTopColorMouseOverBtn.Location = new System.Drawing.Point(1003, 148);
            this.ChangeTabTopColorMouseOverBtn.Name = "ChangeTabTopColorMouseOverBtn";
            this.ChangeTabTopColorMouseOverBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeTabTopColorMouseOverBtn.TabIndex = 9;
            this.ChangeTabTopColorMouseOverBtn.UseVisualStyleBackColor = true;
            this.ChangeTabTopColorMouseOverBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeTabBottomColorMouseOverBtn
            // 
            this.ChangeTabBottomColorMouseOverBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeTabBottomColorMouseOverBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeTabBottomColorMouseOverBtn.Location = new System.Drawing.Point(450, 145);
            this.ChangeTabBottomColorMouseOverBtn.Name = "ChangeTabBottomColorMouseOverBtn";
            this.ChangeTabBottomColorMouseOverBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeTabBottomColorMouseOverBtn.TabIndex = 8;
            this.ChangeTabBottomColorMouseOverBtn.UseVisualStyleBackColor = true;
            this.ChangeTabBottomColorMouseOverBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeInactiveTabBottomColorBtn
            // 
            this.ChangeInactiveTabBottomColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeInactiveTabBottomColorBtn.Location = new System.Drawing.Point(1003, 96);
            this.ChangeInactiveTabBottomColorBtn.Name = "ChangeInactiveTabBottomColorBtn";
            this.ChangeInactiveTabBottomColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeInactiveTabBottomColorBtn.TabIndex = 21;
            this.ChangeInactiveTabBottomColorBtn.UseVisualStyleBackColor = true;
            this.ChangeInactiveTabBottomColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeSelectedTabBottomColorBtn
            // 
            this.ChangeSelectedTabBottomColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeSelectedTabBottomColorBtn.Location = new System.Drawing.Point(1003, 46);
            this.ChangeSelectedTabBottomColorBtn.Name = "ChangeSelectedTabBottomColorBtn";
            this.ChangeSelectedTabBottomColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeSelectedTabBottomColorBtn.TabIndex = 19;
            this.ChangeSelectedTabBottomColorBtn.UseVisualStyleBackColor = true;
            this.ChangeSelectedTabBottomColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeSelectedTabTopColorBtn
            // 
            this.ChangeSelectedTabTopColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeSelectedTabTopColorBtn.Location = new System.Drawing.Point(450, 46);
            this.ChangeSelectedTabTopColorBtn.Name = "ChangeSelectedTabTopColorBtn";
            this.ChangeSelectedTabTopColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeSelectedTabTopColorBtn.TabIndex = 18;
            this.ChangeSelectedTabTopColorBtn.UseVisualStyleBackColor = true;
            this.ChangeSelectedTabTopColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeInactiveTabTopColorBtn
            // 
            this.ChangeInactiveTabTopColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeInactiveTabTopColorBtn.Location = new System.Drawing.Point(450, 96);
            this.ChangeInactiveTabTopColorBtn.Name = "ChangeInactiveTabTopColorBtn";
            this.ChangeInactiveTabTopColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeInactiveTabTopColorBtn.TabIndex = 20;
            this.ChangeInactiveTabTopColorBtn.UseVisualStyleBackColor = true;
            this.ChangeInactiveTabTopColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // grpText
            // 
            this.grpText.Controls.Add(this.ChangeSelectedTabTextColorLbl);
            this.grpText.Controls.Add(this.ChangeMouseOverTabTextColorLbl);
            this.grpText.Controls.Add(this.ChangeInactiveTabTextColorLbl);
            this.grpText.Controls.Add(this.ChangeMouseOverTabTextColorBtn);
            this.grpText.Controls.Add(this.ChangeSelectedTabTextColorBtn);
            this.grpText.Controls.Add(this.ChangeInactiveTabTextColorBtn);
            this.grpText.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpText.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grpText.ForeColor = System.Drawing.Color.White;
            this.grpText.Location = new System.Drawing.Point(0, 575);
            this.grpText.Name = "grpText";
            this.grpText.Size = new System.Drawing.Size(1101, 148);
            this.grpText.TabIndex = 20;
            this.grpText.TabStop = false;
            this.grpText.Text = "grpText";
            this.grpText.Visible = false;
            // 
            // ChangeSelectedTabTextColorLbl
            // 
            this.ChangeSelectedTabTextColorLbl.AutoSize = true;
            this.ChangeSelectedTabTextColorLbl.Location = new System.Drawing.Point(51, 91);
            this.ChangeSelectedTabTextColorLbl.Name = "ChangeSelectedTabTextColorLbl";
            this.ChangeSelectedTabTextColorLbl.Size = new System.Drawing.Size(245, 32);
            this.ChangeSelectedTabTextColorLbl.TabIndex = 22;
            this.ChangeSelectedTabTextColorLbl.Text = "Selected Tab Text";
            // 
            // ChangeMouseOverTabTextColorLbl
            // 
            this.ChangeMouseOverTabTextColorLbl.AutoSize = true;
            this.ChangeMouseOverTabTextColorLbl.Location = new System.Drawing.Point(622, 40);
            this.ChangeMouseOverTabTextColorLbl.Name = "ChangeMouseOverTabTextColorLbl";
            this.ChangeMouseOverTabTextColorLbl.Size = new System.Drawing.Size(279, 32);
            this.ChangeMouseOverTabTextColorLbl.TabIndex = 21;
            this.ChangeMouseOverTabTextColorLbl.Text = "MouseOver Tab Text";
            // 
            // ChangeInactiveTabTextColorLbl
            // 
            this.ChangeInactiveTabTextColorLbl.AutoSize = true;
            this.ChangeInactiveTabTextColorLbl.Location = new System.Drawing.Point(51, 46);
            this.ChangeInactiveTabTextColorLbl.Name = "ChangeInactiveTabTextColorLbl";
            this.ChangeInactiveTabTextColorLbl.Size = new System.Drawing.Size(231, 32);
            this.ChangeInactiveTabTextColorLbl.TabIndex = 20;
            this.ChangeInactiveTabTextColorLbl.Text = "Inactive Tab Text";
            // 
            // ChangeMouseOverTabTextColorBtn
            // 
            this.ChangeMouseOverTabTextColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeMouseOverTabTextColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeMouseOverTabTextColorBtn.Location = new System.Drawing.Point(1003, 29);
            this.ChangeMouseOverTabTextColorBtn.Name = "ChangeMouseOverTabTextColorBtn";
            this.ChangeMouseOverTabTextColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeMouseOverTabTextColorBtn.TabIndex = 16;
            this.ChangeMouseOverTabTextColorBtn.UseVisualStyleBackColor = true;
            this.ChangeMouseOverTabTextColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeSelectedTabTextColorBtn
            // 
            this.ChangeSelectedTabTextColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeSelectedTabTextColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeSelectedTabTextColorBtn.Location = new System.Drawing.Point(450, 91);
            this.ChangeSelectedTabTextColorBtn.Name = "ChangeSelectedTabTextColorBtn";
            this.ChangeSelectedTabTextColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeSelectedTabTextColorBtn.TabIndex = 17;
            this.ChangeSelectedTabTextColorBtn.UseVisualStyleBackColor = true;
            this.ChangeSelectedTabTextColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeInactiveTabTextColorBtn
            // 
            this.ChangeInactiveTabTextColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeInactiveTabTextColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeInactiveTabTextColorBtn.Location = new System.Drawing.Point(450, 40);
            this.ChangeInactiveTabTextColorBtn.Name = "ChangeInactiveTabTextColorBtn";
            this.ChangeInactiveTabTextColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeInactiveTabTextColorBtn.TabIndex = 15;
            this.ChangeInactiveTabTextColorBtn.UseVisualStyleBackColor = true;
            this.ChangeInactiveTabTextColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // grpTabBar
            // 
            this.grpTabBar.Controls.Add(this.ChangeTabBarBottomColorLbl);
            this.grpTabBar.Controls.Add(this.ChangeTabBarTopColorLbl);
            this.grpTabBar.Controls.Add(this.ChangeTabBarBottomColorBtn);
            this.grpTabBar.Controls.Add(this.ChangeTabBarTopColorBtn);
            this.grpTabBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpTabBar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grpTabBar.ForeColor = System.Drawing.Color.White;
            this.grpTabBar.Location = new System.Drawing.Point(0, 482);
            this.grpTabBar.Name = "grpTabBar";
            this.grpTabBar.Size = new System.Drawing.Size(1101, 93);
            this.grpTabBar.TabIndex = 17;
            this.grpTabBar.TabStop = false;
            this.grpTabBar.Text = "grpTabBar";
            this.grpTabBar.Visible = false;
            // 
            // ChangeTabBarBottomColorLbl
            // 
            this.ChangeTabBarBottomColorLbl.AutoSize = true;
            this.ChangeTabBarBottomColorLbl.Location = new System.Drawing.Point(622, 45);
            this.ChangeTabBarBottomColorLbl.Name = "ChangeTabBarBottomColorLbl";
            this.ChangeTabBarBottomColorLbl.Size = new System.Drawing.Size(222, 32);
            this.ChangeTabBarBottomColorLbl.TabIndex = 21;
            this.ChangeTabBarBottomColorLbl.Text = "Bottom Gradient";
            // 
            // ChangeTabBarTopColorLbl
            // 
            this.ChangeTabBarTopColorLbl.AutoSize = true;
            this.ChangeTabBarTopColorLbl.Location = new System.Drawing.Point(51, 46);
            this.ChangeTabBarTopColorLbl.Name = "ChangeTabBarTopColorLbl";
            this.ChangeTabBarTopColorLbl.Size = new System.Drawing.Size(181, 32);
            this.ChangeTabBarTopColorLbl.TabIndex = 20;
            this.ChangeTabBarTopColorLbl.Text = "Top Gradient";
            // 
            // ChangeTabBarBottomColorBtn
            // 
            this.ChangeTabBarBottomColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeTabBarBottomColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeTabBarBottomColorBtn.Location = new System.Drawing.Point(1003, 38);
            this.ChangeTabBarBottomColorBtn.Name = "ChangeTabBarBottomColorBtn";
            this.ChangeTabBarBottomColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeTabBarBottomColorBtn.TabIndex = 5;
            this.ChangeTabBarBottomColorBtn.UseVisualStyleBackColor = true;
            this.ChangeTabBarBottomColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeTabBarTopColorBtn
            // 
            this.ChangeTabBarTopColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeTabBarTopColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeTabBarTopColorBtn.Location = new System.Drawing.Point(450, 40);
            this.ChangeTabBarTopColorBtn.Name = "ChangeTabBarTopColorBtn";
            this.ChangeTabBarTopColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeTabBarTopColorBtn.TabIndex = 4;
            this.ChangeTabBarTopColorBtn.UseVisualStyleBackColor = true;
            this.ChangeTabBarTopColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // grpLeftSideBar
            // 
            this.grpLeftSideBar.Controls.Add(this.ChangeToolBarBottomColorLbl);
            this.grpLeftSideBar.Controls.Add(this.ChangeToolBarTopColorLbl);
            this.grpLeftSideBar.Controls.Add(this.ChangeToolBarBottomColorBtn);
            this.grpLeftSideBar.Controls.Add(this.ChangeToolBarTopColorBtn);
            this.grpLeftSideBar.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpLeftSideBar.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grpLeftSideBar.ForeColor = System.Drawing.Color.White;
            this.grpLeftSideBar.Location = new System.Drawing.Point(0, 388);
            this.grpLeftSideBar.Name = "grpLeftSideBar";
            this.grpLeftSideBar.Size = new System.Drawing.Size(1101, 94);
            this.grpLeftSideBar.TabIndex = 19;
            this.grpLeftSideBar.TabStop = false;
            this.grpLeftSideBar.Text = "grpLeftSideBar";
            this.grpLeftSideBar.Visible = false;
            // 
            // ChangeToolBarBottomColorLbl
            // 
            this.ChangeToolBarBottomColorLbl.AutoSize = true;
            this.ChangeToolBarBottomColorLbl.Location = new System.Drawing.Point(622, 35);
            this.ChangeToolBarBottomColorLbl.Name = "ChangeToolBarBottomColorLbl";
            this.ChangeToolBarBottomColorLbl.Size = new System.Drawing.Size(222, 32);
            this.ChangeToolBarBottomColorLbl.TabIndex = 21;
            this.ChangeToolBarBottomColorLbl.Text = "Bottom Gradient";
            // 
            // ChangeToolBarTopColorLbl
            // 
            this.ChangeToolBarTopColorLbl.AutoSize = true;
            this.ChangeToolBarTopColorLbl.Location = new System.Drawing.Point(51, 40);
            this.ChangeToolBarTopColorLbl.Name = "ChangeToolBarTopColorLbl";
            this.ChangeToolBarTopColorLbl.Size = new System.Drawing.Size(181, 32);
            this.ChangeToolBarTopColorLbl.TabIndex = 20;
            this.ChangeToolBarTopColorLbl.Text = "Top Gradient";
            // 
            // ChangeToolBarBottomColorBtn
            // 
            this.ChangeToolBarBottomColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeToolBarBottomColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeToolBarBottomColorBtn.Location = new System.Drawing.Point(1003, 38);
            this.ChangeToolBarBottomColorBtn.Name = "ChangeToolBarBottomColorBtn";
            this.ChangeToolBarBottomColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeToolBarBottomColorBtn.TabIndex = 14;
            this.ChangeToolBarBottomColorBtn.UseVisualStyleBackColor = true;
            this.ChangeToolBarBottomColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeToolBarTopColorBtn
            // 
            this.ChangeToolBarTopColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeToolBarTopColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeToolBarTopColorBtn.Location = new System.Drawing.Point(450, 38);
            this.ChangeToolBarTopColorBtn.Name = "ChangeToolBarTopColorBtn";
            this.ChangeToolBarTopColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeToolBarTopColorBtn.TabIndex = 13;
            this.ChangeToolBarTopColorBtn.UseVisualStyleBackColor = true;
            this.ChangeToolBarTopColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // grpSettingsMenu
            // 
            this.grpSettingsMenu.Controls.Add(this.ChangeContextMenuOverColorLbl);
            this.grpSettingsMenu.Controls.Add(this.ChangeContextMenuForeColorLbl);
            this.grpSettingsMenu.Controls.Add(this.ChangeContextMenuBackColorLbl);
            this.grpSettingsMenu.Controls.Add(this.ChangeContextMenuOverColorBtn);
            this.grpSettingsMenu.Controls.Add(this.ChangeContextMenuForeColorBtn);
            this.grpSettingsMenu.Controls.Add(this.ChangeContextMenuBackColorBtn);
            this.grpSettingsMenu.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpSettingsMenu.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.grpSettingsMenu.ForeColor = System.Drawing.Color.White;
            this.grpSettingsMenu.Location = new System.Drawing.Point(0, 234);
            this.grpSettingsMenu.Name = "grpSettingsMenu";
            this.grpSettingsMenu.Size = new System.Drawing.Size(1101, 154);
            this.grpSettingsMenu.TabIndex = 18;
            this.grpSettingsMenu.TabStop = false;
            this.grpSettingsMenu.Text = "grpSettingsMenu";
            this.grpSettingsMenu.Visible = false;
            // 
            // ChangeContextMenuOverColorLbl
            // 
            this.ChangeContextMenuOverColorLbl.AutoSize = true;
            this.ChangeContextMenuOverColorLbl.Location = new System.Drawing.Point(51, 97);
            this.ChangeContextMenuOverColorLbl.Name = "ChangeContextMenuOverColorLbl";
            this.ChangeContextMenuOverColorLbl.Size = new System.Drawing.Size(259, 32);
            this.ChangeContextMenuOverColorLbl.TabIndex = 28;
            this.ChangeContextMenuOverColorLbl.Text = "Context Menu Over";
            // 
            // ChangeContextMenuForeColorLbl
            // 
            this.ChangeContextMenuForeColorLbl.AutoSize = true;
            this.ChangeContextMenuForeColorLbl.Location = new System.Drawing.Point(622, 42);
            this.ChangeContextMenuForeColorLbl.Name = "ChangeContextMenuForeColorLbl";
            this.ChangeContextMenuForeColorLbl.Size = new System.Drawing.Size(331, 32);
            this.ChangeContextMenuForeColorLbl.TabIndex = 27;
            this.ChangeContextMenuForeColorLbl.Text = "Context Menu Fore Color";
            // 
            // ChangeContextMenuBackColorLbl
            // 
            this.ChangeContextMenuBackColorLbl.AutoSize = true;
            this.ChangeContextMenuBackColorLbl.Location = new System.Drawing.Point(51, 46);
            this.ChangeContextMenuBackColorLbl.Name = "ChangeContextMenuBackColorLbl";
            this.ChangeContextMenuBackColorLbl.Size = new System.Drawing.Size(336, 32);
            this.ChangeContextMenuBackColorLbl.TabIndex = 26;
            this.ChangeContextMenuBackColorLbl.Text = "Context Menu Back Color";
            // 
            // ChangeContextMenuOverColorBtn
            // 
            this.ChangeContextMenuOverColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeContextMenuOverColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeContextMenuOverColorBtn.Location = new System.Drawing.Point(450, 97);
            this.ChangeContextMenuOverColorBtn.Name = "ChangeContextMenuOverColorBtn";
            this.ChangeContextMenuOverColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeContextMenuOverColorBtn.TabIndex = 12;
            this.ChangeContextMenuOverColorBtn.UseVisualStyleBackColor = true;
            this.ChangeContextMenuOverColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeContextMenuForeColorBtn
            // 
            this.ChangeContextMenuForeColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeContextMenuForeColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeContextMenuForeColorBtn.Location = new System.Drawing.Point(1003, 40);
            this.ChangeContextMenuForeColorBtn.Name = "ChangeContextMenuForeColorBtn";
            this.ChangeContextMenuForeColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeContextMenuForeColorBtn.TabIndex = 11;
            this.ChangeContextMenuForeColorBtn.UseVisualStyleBackColor = true;
            this.ChangeContextMenuForeColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // ChangeContextMenuBackColorBtn
            // 
            this.ChangeContextMenuBackColorBtn.AutoSizeMode = System.Windows.Forms.AutoSizeMode.GrowAndShrink;
            this.ChangeContextMenuBackColorBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.ChangeContextMenuBackColorBtn.Location = new System.Drawing.Point(450, 40);
            this.ChangeContextMenuBackColorBtn.Name = "ChangeContextMenuBackColorBtn";
            this.ChangeContextMenuBackColorBtn.Size = new System.Drawing.Size(50, 37);
            this.ChangeContextMenuBackColorBtn.TabIndex = 10;
            this.ChangeContextMenuBackColorBtn.UseVisualStyleBackColor = true;
            this.ChangeContextMenuBackColorBtn.MouseClick += new System.Windows.Forms.MouseEventHandler(this.ButtonMouseClick);
            // 
            // grpChooseProfile
            // 
            this.grpChooseProfile.AutoSize = true;
            this.grpChooseProfile.Controls.Add(this.NewThemeBtn);
            this.grpChooseProfile.Controls.Add(this.lblSelectTheme);
            this.grpChooseProfile.Controls.Add(this.cmbTheme);
            this.grpChooseProfile.Dock = System.Windows.Forms.DockStyle.Top;
            this.grpChooseProfile.ForeColor = System.Drawing.Color.White;
            this.grpChooseProfile.Location = new System.Drawing.Point(0, 111);
            this.grpChooseProfile.Name = "grpChooseProfile";
            this.grpChooseProfile.Size = new System.Drawing.Size(1101, 123);
            this.grpChooseProfile.TabIndex = 16;
            this.grpChooseProfile.TabStop = false;
            this.grpChooseProfile.Text = "grpChooseProfile";
            // 
            // NewThemeBtn
            // 
            this.NewThemeBtn.AutoSize = true;
            this.NewThemeBtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.NewThemeBtn.ForeColor = System.Drawing.Color.White;
            this.NewThemeBtn.Location = new System.Drawing.Point(829, 41);
            this.NewThemeBtn.Name = "NewThemeBtn";
            this.NewThemeBtn.Size = new System.Drawing.Size(178, 44);
            this.NewThemeBtn.TabIndex = 3;
            this.NewThemeBtn.Text = "New Theme";
            this.NewThemeBtn.UseVisualStyleBackColor = true;
            this.NewThemeBtn.Click += new System.EventHandler(this.NewThemeBtn_Click);
            // 
            // lblSelectTheme
            // 
            this.lblSelectTheme.AutoSize = true;
            this.lblSelectTheme.ForeColor = System.Drawing.Color.White;
            this.lblSelectTheme.Location = new System.Drawing.Point(38, 46);
            this.lblSelectTheme.Name = "lblSelectTheme";
            this.lblSelectTheme.Size = new System.Drawing.Size(190, 32);
            this.lblSelectTheme.TabIndex = 1;
            this.lblSelectTheme.Text = "Select Theme";
            // 
            // cmbTheme
            // 
            this.cmbTheme.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cmbTheme.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.cmbTheme.FormattingEnabled = true;
            this.cmbTheme.Location = new System.Drawing.Point(234, 44);
            this.cmbTheme.Name = "cmbTheme";
            this.cmbTheme.Size = new System.Drawing.Size(245, 40);
            this.cmbTheme.TabIndex = 2;
            this.cmbTheme.SelectedIndexChanged += new System.EventHandler(this.cmbTheme_SelectedIndexChanged);
            // 
            // groupBoxStyle
            // 
            this.groupBoxStyle.AutoSize = true;
            this.groupBoxStyle.Controls.Add(this.pictureBoxSelected);
            this.groupBoxStyle.Controls.Add(this.pictureBoxNewStyle);
            this.groupBoxStyle.Controls.Add(this.pictureBoxOldStyle);
            this.groupBoxStyle.Dock = System.Windows.Forms.DockStyle.Top;
            this.groupBoxStyle.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.groupBoxStyle.ForeColor = System.Drawing.Color.White;
            this.groupBoxStyle.Location = new System.Drawing.Point(0, 0);
            this.groupBoxStyle.Name = "groupBoxStyle";
            this.groupBoxStyle.Size = new System.Drawing.Size(1101, 111);
            this.groupBoxStyle.TabIndex = 15;
            this.groupBoxStyle.TabStop = false;
            this.groupBoxStyle.Text = "Style";
            // 
            // pictureBoxSelected
            // 
            this.pictureBoxSelected.BackColor = System.Drawing.Color.Transparent;
            this.pictureBoxSelected.Location = new System.Drawing.Point(501, 13);
            this.pictureBoxSelected.Name = "pictureBoxSelected";
            this.pictureBoxSelected.Size = new System.Drawing.Size(50, 40);
            this.pictureBoxSelected.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxSelected.TabIndex = 0;
            this.pictureBoxSelected.TabStop = false;
            // 
            // pictureBoxNewStyle
            // 
            this.pictureBoxNewStyle.Location = new System.Drawing.Point(658, 44);
            this.pictureBoxNewStyle.Name = "pictureBoxNewStyle";
            this.pictureBoxNewStyle.Size = new System.Drawing.Size(92, 29);
            this.pictureBoxNewStyle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxNewStyle.TabIndex = 1;
            this.pictureBoxNewStyle.TabStop = false;
            this.pictureBoxNewStyle.Text = "Toob";
            this.pictureBoxNewStyle.Click += new System.EventHandler(this.pictureBoxStyle_Click);
            this.pictureBoxNewStyle.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            this.pictureBoxNewStyle.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            // 
            // pictureBoxOldStyle
            // 
            this.pictureBoxOldStyle.Location = new System.Drawing.Point(44, 44);
            this.pictureBoxOldStyle.Name = "pictureBoxOldStyle";
            this.pictureBoxOldStyle.Size = new System.Drawing.Size(111, 29);
            this.pictureBoxOldStyle.SizeMode = System.Windows.Forms.PictureBoxSizeMode.StretchImage;
            this.pictureBoxOldStyle.TabIndex = 0;
            this.pictureBoxOldStyle.TabStop = false;
            this.pictureBoxOldStyle.Text = "Em";
            this.pictureBoxOldStyle.Click += new System.EventHandler(this.pictureBoxStyle_Click);
            this.pictureBoxOldStyle.MouseEnter += new System.EventHandler(this.pictureBox_MouseEnter);
            this.pictureBoxOldStyle.MouseLeave += new System.EventHandler(this.pictureBox_MouseLeave);
            // 
            // Applybtn
            // 
            this.Applybtn.AutoSize = true;
            this.Applybtn.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.Applybtn.Location = new System.Drawing.Point(3, 183);
            this.Applybtn.Name = "Applybtn";
            this.Applybtn.Size = new System.Drawing.Size(152, 44);
            this.Applybtn.TabIndex = 26;
            this.Applybtn.Text = "Apply";
            this.Applybtn.UseVisualStyleBackColor = true;
            this.Applybtn.Visible = false;
            this.Applybtn.Click += new System.EventHandler(this.ApplyButtonClick);
            // 
            // ThemeSettings
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(16F, 32F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.Name = "ThemeSettings";
            this.Size = new System.Drawing.Size(1101, 1489);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            this.grpControlBar.ResumeLayout(false);
            this.grpControlBar.PerformLayout();
            this.grpBorder.ResumeLayout(false);
            this.grpBorder.PerformLayout();
            this.grpTab.ResumeLayout(false);
            this.grpTab.PerformLayout();
            this.grpText.ResumeLayout(false);
            this.grpText.PerformLayout();
            this.grpTabBar.ResumeLayout(false);
            this.grpTabBar.PerformLayout();
            this.grpLeftSideBar.ResumeLayout(false);
            this.grpLeftSideBar.PerformLayout();
            this.grpSettingsMenu.ResumeLayout(false);
            this.grpSettingsMenu.PerformLayout();
            this.grpChooseProfile.ResumeLayout(false);
            this.grpChooseProfile.PerformLayout();
            this.groupBoxStyle.ResumeLayout(false);
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxSelected)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxNewStyle)).EndInit();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBoxOldStyle)).EndInit();
            this.ResumeLayout(false);
            this.PerformLayout();

        }
        
        #endregion

        private System.Windows.Forms.Panel panel1;
        public System.Windows.Forms.Button Applybtn;
        private System.Windows.Forms.GroupBox grpBorder;
        private System.Windows.Forms.Label ChangeInnerBorderColorLbl;
        private System.Windows.Forms.Label ChangeTabBorderLbl;
        private System.Windows.Forms.Label ChangeToolBarBorderColorLbl;
        private System.Windows.Forms.Label ChangeGameManagerBorderLbl;
        private System.Windows.Forms.Button ChangeToolBarBorderColorBtn;
        private System.Windows.Forms.Button ChangeInnerBorderColorBtn;
        private System.Windows.Forms.Button ChangeTabBorderBtn;
        private System.Windows.Forms.Button ChangeGameManagerBorderBtn;
        private System.Windows.Forms.GroupBox grpTab;
        private System.Windows.Forms.Label ChangeInactiveTabBottomColorLbl;
        private System.Windows.Forms.Label ChangeInactiveTabTopColorLbl;
        private System.Windows.Forms.Label ChangeSelectedTabBottomColorLbl;
        private System.Windows.Forms.Label ChangeSelectedTabTopColorLbl;
        private System.Windows.Forms.Button ChangeInactiveTabBottomColorBtn;
        private System.Windows.Forms.Button ChangeSelectedTabBottomColorBtn;
        private System.Windows.Forms.Button ChangeSelectedTabTopColorBtn;
        private System.Windows.Forms.Button ChangeInactiveTabTopColorBtn;
        private System.Windows.Forms.GroupBox grpText;
        private System.Windows.Forms.Label ChangeSelectedTabTextColorLbl;
        private System.Windows.Forms.Label ChangeMouseOverTabTextColorLbl;
        private System.Windows.Forms.Label ChangeInactiveTabTextColorLbl;
        private System.Windows.Forms.Button ChangeMouseOverTabTextColorBtn;
        private System.Windows.Forms.Button ChangeSelectedTabTextColorBtn;
        private System.Windows.Forms.Button ChangeInactiveTabTextColorBtn;
        private System.Windows.Forms.GroupBox grpLeftSideBar;
        private System.Windows.Forms.Label ChangeToolBarBottomColorLbl;
        private System.Windows.Forms.Label ChangeToolBarTopColorLbl;
        private System.Windows.Forms.Button ChangeToolBarBottomColorBtn;
        private System.Windows.Forms.Button ChangeToolBarTopColorBtn;
        private System.Windows.Forms.GroupBox grpSettingsMenu;
        private System.Windows.Forms.Label ChangeContextMenuOverColorLbl;
        private System.Windows.Forms.Label ChangeContextMenuForeColorLbl;
        private System.Windows.Forms.Label ChangeContextMenuBackColorLbl;
        private System.Windows.Forms.Button ChangeContextMenuOverColorBtn;
        private System.Windows.Forms.Button ChangeContextMenuForeColorBtn;
        private System.Windows.Forms.Button ChangeContextMenuBackColorBtn;
        private System.Windows.Forms.GroupBox grpTabBar;
        private System.Windows.Forms.Label ChangeTabTopColorMouseOverLbl;
        private System.Windows.Forms.Label ChangeTabBottomColorMouseOverLbl;
        private System.Windows.Forms.Label ChangeControlBarBottomColorLbl;
        private System.Windows.Forms.Label ChangeControlBarTopColorLbl;
        private System.Windows.Forms.Label ChangeTabBarBottomColorLbl;
        private System.Windows.Forms.Label ChangeTabBarTopColorLbl;
        private System.Windows.Forms.Button ChangeTabBottomColorMouseOverBtn;
        private System.Windows.Forms.Button ChangeControlBarBottomColorBtn;
        private System.Windows.Forms.Button ChangeTabBarBottomColorBtn;
        private System.Windows.Forms.Button ChangeTabTopColorMouseOverBtn;
        private System.Windows.Forms.Button ChangeControlBarTopColorBtn;
        private System.Windows.Forms.Button ChangeTabBarTopColorBtn;
        private System.Windows.Forms.GroupBox grpChooseProfile;
        private System.Windows.Forms.Button NewThemeBtn;
        private System.Windows.Forms.Label lblSelectTheme;
        public System.Windows.Forms.ComboBox cmbTheme;
        public System.Windows.Forms.GroupBox groupBoxStyle;
        private System.Windows.Forms.PictureBox pictureBoxNewStyle;
        public System.Windows.Forms.PictureBox pictureBoxOldStyle;
        private System.Windows.Forms.PictureBox pictureBoxSelected;
        private GroupBox grpControlBar;
    }
}
