using System.Drawing;
using System;
using System.Windows.Forms;
namespace BlueStacks.hyperDroid.GameManager
{
    partial class SettingsForm
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

        /// <summary>
        /// Required method for Designer support - do not modify
        /// the contents of this method with the code editor.
        /// </summary>
        private void InitializeComponent()
        {
            this.components = new System.ComponentModel.Container();
            System.Windows.Forms.TreeNode treeNode1 = new System.Windows.Forms.TreeNode("General");
            System.Windows.Forms.TreeNode treeNode2 = new System.Windows.Forms.TreeNode("Themes");
            this.imageList1 = new System.Windows.Forms.ImageList(this.components);
            this.panel1 = new System.Windows.Forms.Panel();
            this.pictureBox1 = new System.Windows.Forms.PictureBox();
            this.lblPreferences = new System.Windows.Forms.Label();
            this.splitContainer1 = new System.Windows.Forms.SplitContainer();
            this.treeView1 = new BlueStacks.hyperDroid.GameManager.NativeTreeView();
            this.generalSettings1 = new BlueStacks.hyperDroid.GameManager.GeneralSettings();
            this.themeSettings1 = new BlueStacks.hyperDroid.GameManager.ThemeSettings();
            this.panel1.SuspendLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).BeginInit();
            this.splitContainer1.Panel1.SuspendLayout();
            this.splitContainer1.Panel2.SuspendLayout();
            this.splitContainer1.SuspendLayout();
            this.SuspendLayout();
            // 
            // imageList1
            // 
            this.imageList1.ColorDepth = System.Windows.Forms.ColorDepth.Depth8Bit;
            this.imageList1.ImageSize = new System.Drawing.Size(16, 16);
            this.imageList1.TransparentColor = System.Drawing.Color.Transparent;
            // 
            // panel1
            // 
            this.panel1.Controls.Add(this.pictureBox1);
            this.panel1.Controls.Add(this.lblPreferences);
            this.panel1.Dock = System.Windows.Forms.DockStyle.Top;
            this.panel1.Location = new System.Drawing.Point(4, 8);
            this.panel1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.panel1.Name = "panel1";
            this.panel1.Size = new System.Drawing.Size(738, 44);
            this.panel1.TabIndex = 14;
            // 
            // pictureBox1
            // 
            this.pictureBox1.Dock = System.Windows.Forms.DockStyle.Right;
            this.pictureBox1.Location = new System.Drawing.Point(692, 0);
            this.pictureBox1.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.pictureBox1.Name = "pictureBox1";
            this.pictureBox1.Size = new System.Drawing.Size(46, 44);
            this.pictureBox1.TabIndex = 1;
            this.pictureBox1.TabStop = false;
            this.pictureBox1.Click += new System.EventHandler(this.PictureBoxButtonMouseDown);
            this.pictureBox1.MouseEnter += new System.EventHandler(this.PictureBoxControlBarButtonMouseEnter);
            this.pictureBox1.MouseLeave += new System.EventHandler(this.PictureBoxControlBarButtonMouseLeave);
            this.pictureBox1.MouseUp += new System.Windows.Forms.MouseEventHandler(this.PictureBoxControlBarButtonMouseUp);
            // 
            // lblPreferences
            // 
            this.lblPreferences.AutoSize = true;
            this.lblPreferences.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.lblPreferences.Location = new System.Drawing.Point(10, 9);
            this.lblPreferences.Name = "lblPreferences";
            this.lblPreferences.Size = new System.Drawing.Size(0, 20);
            this.lblPreferences.TabIndex = 0;
            // 
            // splitContainer1
            // 
            this.splitContainer1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.splitContainer1.Location = new System.Drawing.Point(4, 52);
            this.splitContainer1.Name = "splitContainer1";
            // 
            // splitContainer1.Panel1
            // 
            this.splitContainer1.Panel1.Controls.Add(this.treeView1);
            // 
            // splitContainer1.Panel2
            // 
            this.splitContainer1.Panel2.Controls.Add(this.generalSettings1);
            this.splitContainer1.Panel2.Controls.Add(this.themeSettings1);
            this.splitContainer1.Size = new System.Drawing.Size(738, 472);
            this.splitContainer1.SplitterDistance = 130;
            this.splitContainer1.TabIndex = 15;
            // 
            // treeView1
            // 
            this.treeView1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.treeView1.DrawMode = System.Windows.Forms.TreeViewDrawMode.OwnerDrawAll;
            this.treeView1.Location = new System.Drawing.Point(0, 0);
            this.treeView1.Margin = new System.Windows.Forms.Padding(1);
            this.treeView1.Name = "treeView1";
            treeNode1.Name = "nGeneral";
            treeNode1.Text = "General";
            treeNode2.Name = "nThemes";
            treeNode2.Text = "Themes";
            this.treeView1.Nodes.AddRange(new System.Windows.Forms.TreeNode[] {
            treeNode1,
            treeNode2});
            this.treeView1.Size = new System.Drawing.Size(130, 472);
            this.treeView1.TabIndex = 8;
            this.treeView1.AfterSelect += new System.Windows.Forms.TreeViewEventHandler(this.treeView1_AfterSelect);
            // 
            // generalSettings1
            // 
            this.generalSettings1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.generalSettings1.Font = new System.Drawing.Font("Microsoft Sans Serif", 14F);
            this.generalSettings1.Location = new System.Drawing.Point(0, 0);
            this.generalSettings1.Margin = new System.Windows.Forms.Padding(1);
            this.generalSettings1.Name = "generalSettings1";
            this.generalSettings1.Size = new System.Drawing.Size(604, 472);
            this.generalSettings1.TabIndex = 10;
            // 
            // themeSettings1
            // 
            this.themeSettings1.AutoScroll = true;
            this.themeSettings1.Dock = System.Windows.Forms.DockStyle.Fill;
            this.themeSettings1.Font = new System.Drawing.Font("Microsoft Sans Serif", 8F);
            this.themeSettings1.Location = new System.Drawing.Point(0, 0);
            this.themeSettings1.Margin = new System.Windows.Forms.Padding(1);
            this.themeSettings1.Name = "themeSettings1";
            this.themeSettings1.Size = new System.Drawing.Size(604, 472);
            this.themeSettings1.TabIndex = 12;
            // 
            // SettingsForm
            // 
            this.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.BackColor = System.Drawing.SystemColors.ControlLightLight;
            this.ClientSize = new System.Drawing.Size(746, 529);
            this.Controls.Add(this.splitContainer1);
            this.Controls.Add(this.panel1);
            this.Font = new System.Drawing.Font("Microsoft Sans Serif", 16F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Pixel);
            this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            this.Margin = new System.Windows.Forms.Padding(3, 4, 3, 4);
            this.MaximizeBox = false;
            this.MinimizeBox = false;
            this.Name = "SettingsForm";
            this.Padding = new System.Windows.Forms.Padding(4, 8, 4, 5);
            this.ShowIcon = false;
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
            this.Text = "SettingsForm";
            this.Load += new System.EventHandler(this.SettingsFrom_Load);
            this.panel1.ResumeLayout(false);
            this.panel1.PerformLayout();
            ((System.ComponentModel.ISupportInitialize)(this.pictureBox1)).EndInit();
            this.splitContainer1.Panel1.ResumeLayout(false);
            this.splitContainer1.Panel2.ResumeLayout(false);
            this.splitContainer1.ResumeLayout(false);
            this.ResumeLayout(false);

        }

        #endregion

        private System.Windows.Forms.ImageList imageList1;
		private NativeTreeView treeView1;
        private GeneralSettings generalSettings1;
        private ThemeSettings themeSettings1;
        private System.Windows.Forms.Panel panel1;
        private System.Windows.Forms.PictureBox pictureBox1;
        private System.Windows.Forms.Label lblPreferences;
		System.Windows.Forms.TreeNode treeNode1;
		System.Windows.Forms.TreeNode treeNode2;
		private SplitContainer splitContainer1;
    }
}
