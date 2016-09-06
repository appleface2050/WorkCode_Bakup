using System;
using System.IO;
using System.Threading;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Text;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{
	public class InputMapperTool : Form
	{
		private System.Windows.Forms.TabControl tabControl1;
		private System.Windows.Forms.TabPage tabPage1;
		private System.Windows.Forms.TabPage tabPage2;
		private System.Windows.Forms.TabPage tabPage3;
		private System.Windows.Forms.GroupBox groupBox1;

		private System.Windows.Forms.Label swipeLeftLabel;
		private System.Windows.Forms.Label swipeRightLabel;
		private System.Windows.Forms.Label swipeUpLabel;
		private System.Windows.Forms.Label swipeDownLabel;

		private System.Windows.Forms.ComboBox swipeLeftKeyCombo;
		private System.Windows.Forms.ComboBox swipeRightKeyCombo;
		private System.Windows.Forms.ComboBox swipeUpKeyCombo;
		private System.Windows.Forms.ComboBox swipeDownKeyCombo;

		private System.Windows.Forms.GroupBox groupBox2;
		private System.Windows.Forms.HScrollBar DownHScroll;
		private System.Windows.Forms.Label DownTiltLabel;
		private System.Windows.Forms.ComboBox TiltDownKeyCombo;
		private System.Windows.Forms.Label TiltDownLabel;
		private System.Windows.Forms.HScrollBar UpHScroll;
		private System.Windows.Forms.Label UpTiltLabel;
		private System.Windows.Forms.ComboBox TiltUpKeyCombo;
		private System.Windows.Forms.Label TiltUpLabel;
		private System.Windows.Forms.HScrollBar RightHScroll;
		private System.Windows.Forms.Label RightTiltLabel;
		private System.Windows.Forms.ComboBox TiltRightKeyCombo;
		private System.Windows.Forms.Label TiltRightLabel;
		private System.Windows.Forms.HScrollBar LeftHScroll;
		private System.Windows.Forms.Label LeftTiltLabel;
		private System.Windows.Forms.ComboBox TiltLeftKeyCombo;
		private System.Windows.Forms.Label TiltLeftLabel;
		private System.Windows.Forms.GroupBox groupBox3;
		private DevComponents.DotNetBar.ButtonX buttonX1;
		private System.Windows.Forms.Label[] TapAtLocLabel;
		private System.Windows.Forms.PictureBox[] ConnectingArrowsPBox;
		private System.Windows.Forms.ComboBox[] TapAtLocKeyCombo;
		private DevComponents.DotNetBar.ButtonX buttonX2;
		private DevComponents.DotNetBar.ButtonX buttonX3;
		private Panel MainPanel;
		private Label tapHelpTextLabel;

		public static string sCurrentAppPackage = "com.bluestacks.FOO"; // Ignore mappings for our apps

		System.Windows.Forms.ToolTip tapLocationTooltip = new System.Windows.Forms.ToolTip();

		private System.ComponentModel.IContainer components = null;

		private int mLeft;
		private int mTop;
		private int mWidth;
		private int mHeight;

		private int mOrigWidth = 465;
		private int mOrigHeight = 450;

		private int mTapLocMappingIndex = 0;
		private int mMaxMappingsSupported = 6;

		private bool mIsModifyingMapping = false;
		private float mTapX = 0;
		private float mTapY = 0;

		public InputMapperTool(int left, int top, int width, int height)
		{
			if (sCurrentAppPackage == null || sCurrentAppPackage.StartsWith("com.bluestacks"))
			{
				MessageBox.Show("Please start the App for which you want to set keyboard mappings.");
				return;
			}

			Logger.Error("pkgName = " + sCurrentAppPackage);

			mLeft = left;
			mTop = top;
			mWidth = width;
			mHeight = height;

			InitializeComponent();
		}

		protected override void Dispose(bool disposing)
		{
			if (disposing && (components != null))
			{
				components.Dispose();
			}
			base.Dispose(disposing);
		}

		private void InitializeComponent()
		{
			this.tabControl1 = new System.Windows.Forms.TabControl();
			this.tabPage1 = new System.Windows.Forms.TabPage();
			this.groupBox3 = new System.Windows.Forms.GroupBox();
			this.buttonX1 = new DevComponents.DotNetBar.ButtonX();
			this.buttonX2 = new DevComponents.DotNetBar.ButtonX();
			this.buttonX3 = new DevComponents.DotNetBar.ButtonX();
			this.TapAtLocLabel = new System.Windows.Forms.Label[mMaxMappingsSupported];
			this.TapAtLocKeyCombo = new System.Windows.Forms.ComboBox[mMaxMappingsSupported];
			this.ConnectingArrowsPBox = new System.Windows.Forms.PictureBox[mMaxMappingsSupported];
			this.tabPage2 = new System.Windows.Forms.TabPage();
			this.groupBox1 = new System.Windows.Forms.GroupBox();

			this.swipeLeftLabel = new Label();
			this.swipeRightLabel = new Label();
			this.swipeUpLabel = new Label();
			this.swipeDownLabel = new Label();

			this.swipeLeftKeyCombo = new System.Windows.Forms.ComboBox();
			this.swipeRightKeyCombo = new System.Windows.Forms.ComboBox();
			this.swipeUpKeyCombo = new System.Windows.Forms.ComboBox();
			this.swipeDownKeyCombo = new System.Windows.Forms.ComboBox();

			this.tabPage3 = new System.Windows.Forms.TabPage();
			this.groupBox2 = new System.Windows.Forms.GroupBox();
			this.DownHScroll = new System.Windows.Forms.HScrollBar();
			this.DownTiltLabel = new System.Windows.Forms.Label();
			this.TiltDownKeyCombo = new System.Windows.Forms.ComboBox();
			this.TiltDownLabel = new System.Windows.Forms.Label();
			this.UpHScroll = new System.Windows.Forms.HScrollBar();
			this.UpTiltLabel = new System.Windows.Forms.Label();
			this.TiltUpKeyCombo = new System.Windows.Forms.ComboBox();
			this.TiltUpLabel = new System.Windows.Forms.Label();
			this.RightHScroll = new System.Windows.Forms.HScrollBar();
			this.RightTiltLabel = new System.Windows.Forms.Label();
			this.TiltRightKeyCombo = new System.Windows.Forms.ComboBox();
			this.TiltRightLabel = new System.Windows.Forms.Label();
			this.LeftHScroll = new System.Windows.Forms.HScrollBar();
			this.LeftTiltLabel = new System.Windows.Forms.Label();
			this.TiltLeftKeyCombo = new System.Windows.Forms.ComboBox();
			this.TiltLeftLabel = new System.Windows.Forms.Label();
			this.tapHelpTextLabel = new Label();
			this.tabControl1.SuspendLayout();
			this.tabPage1.SuspendLayout();
			this.groupBox3.SuspendLayout();
			this.tabPage2.SuspendLayout();
			this.groupBox1.SuspendLayout();
			this.tabPage3.SuspendLayout();
			this.groupBox2.SuspendLayout();
			this.SuspendLayout();

			// Main panel
			MainPanel = new Panel();
			MainPanel.Location = new System.Drawing.Point(0, 0);
			MainPanel.Size = new System.Drawing.Size(mWidth, mHeight);
			MainPanel.Visible = false;
			MainPanel.Click += MainPanelClick;
			//MainPanel.Cursor = new Cursor(@"C:\users\vikram\desktop\YellowHandCursor.cur");
			MainPanel.Cursor = Cursors.Hand;

			// Tap Help text label
			tapHelpTextLabel.Text = "Tap the area for which you want to add keyboard mapping";
			tapHelpTextLabel.Location = new Point(mWidth / 2 - 200, 10);
			tapHelpTextLabel.Size = new Size(450, 100);
			tapHelpTextLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point,
					((byte)(0)));
			tapHelpTextLabel.Visible = false;

			// 
			// tabControl1
			// 
			this.tabControl1.Controls.Add(this.tabPage1);
			this.tabControl1.Controls.Add(this.tabPage2);
			this.tabControl1.Controls.Add(this.tabPage3);
			this.tabControl1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.tabControl1.Location = new System.Drawing.Point(0, 0);
			this.tabControl1.Name = "tabControl1";
			this.tabControl1.SelectedIndex = 0;
			this.tabControl1.Size = new System.Drawing.Size(560, 399);
			this.tabControl1.TabIndex = 0;
			// 
			// tabPage1
			// 
			this.tabPage1.Controls.Add(this.groupBox3);
			this.tabPage1.Font = new Font("Microsoft Sans Serif", 9F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
			this.tabPage1.Location = new System.Drawing.Point(4, 29);
			this.tabPage1.Name = "tabPage1";
			this.tabPage1.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage1.Size = new System.Drawing.Size(552, 266);
			this.tabPage1.TabIndex = 0;
			this.tabPage1.Text = "Tap";
			this.tabPage1.ToolTipText = "Set keyboard mappings for TAP action on screen";
			this.tabPage1.UseVisualStyleBackColor = true;
			// 
			// groupBox3
			// 
			this.groupBox3.Controls.Add(this.buttonX1);
			this.groupBox3.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
			this.groupBox3.Location = new System.Drawing.Point(8, 16);
			this.groupBox3.Name = "groupBox3";
			this.groupBox3.Size = new System.Drawing.Size(441, 334);
			this.groupBox3.TabIndex = 0;
			this.groupBox3.TabStop = false;
			this.groupBox3.Text = "Tap Screen Actions";
			// 
			// buttonX1
			// 
			this.buttonX1.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
			this.buttonX1.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
			this.buttonX1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.buttonX1.Location = new System.Drawing.Point(277, 293);
			this.buttonX1.Name = "buttonX1";
			this.buttonX1.Size = new System.Drawing.Size(148, 33);
			this.buttonX1.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
			this.buttonX1.TabIndex = 26;
			this.buttonX1.Text = "Add New Mapping";
			this.buttonX1.Click += AddNewMappingBtnClick;
			// 
			// tabPage2
			// 
			this.tabPage2.Controls.Add(this.groupBox1);
			this.tabPage2.Location = new System.Drawing.Point(4, 29);
			this.tabPage2.Name = "tabPage2";
			this.tabPage2.Padding = new System.Windows.Forms.Padding(3);
			this.tabPage2.Size = new System.Drawing.Size(552, 266);
			this.tabPage2.TabIndex = 1;
			this.tabPage2.Text = "Swipe";
			this.tabPage2.ToolTipText = "Set keyboard mappings for SWIPE actions";
			this.tabPage2.UseVisualStyleBackColor = true;
			// 
			// groupBox1
			// 
			this.groupBox1.Controls.Add(this.swipeLeftLabel);
			this.groupBox1.Controls.Add(this.swipeRightLabel);
			this.groupBox1.Controls.Add(this.swipeUpLabel);
			this.groupBox1.Controls.Add(this.swipeDownLabel);

			this.groupBox1.Controls.Add(this.swipeLeftKeyCombo);
			this.groupBox1.Controls.Add(this.swipeRightKeyCombo);
			this.groupBox1.Controls.Add(this.swipeUpKeyCombo);
			this.groupBox1.Controls.Add(this.swipeDownKeyCombo);

			this.groupBox1.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox1.Location = new System.Drawing.Point(8, 16);
			this.groupBox1.Name = "groupBox1";
			this.groupBox1.Size = new System.Drawing.Size(441, 242);
			this.groupBox1.TabIndex = 0;
			this.groupBox1.TabStop = false;
			this.groupBox1.Text = "Emulate swipe actions";

			this.swipeLeftKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.swipeLeftKeyCombo.FormattingEnabled = true;
			this.swipeLeftKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});
			this.swipeLeftKeyCombo.Location = new System.Drawing.Point(242, 42);
			this.swipeLeftKeyCombo.Name = "swipeLeftKeyCombo";
			this.swipeLeftKeyCombo.Size = new System.Drawing.Size(158, 28);

			this.swipeRightKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.swipeRightKeyCombo.FormattingEnabled = true;
			this.swipeRightKeyCombo.Location = new System.Drawing.Point(242, 85);
			this.swipeRightKeyCombo.Name = "swipeRightKeyCombo";
			this.swipeRightKeyCombo.Size = new System.Drawing.Size(158, 28);
			this.swipeRightKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});

			this.swipeUpKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.swipeUpKeyCombo.FormattingEnabled = true;
			this.swipeUpKeyCombo.Location = new System.Drawing.Point(242, 130);
			this.swipeUpKeyCombo.Name = "swipeUpKeyCombo";
			this.swipeUpKeyCombo.Size = new System.Drawing.Size(158, 28);
			this.swipeUpKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});

			this.swipeDownKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.swipeDownKeyCombo.FormattingEnabled = true;
			this.swipeDownKeyCombo.Location = new System.Drawing.Point(242, 175);
			this.swipeDownKeyCombo.Name = "swipeDownKeyCombo";
			this.swipeDownKeyCombo.Size = new System.Drawing.Size(158, 28);
			this.swipeDownKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});

			this.swipeLeftLabel.Text = "Swipe Left";
			this.swipeLeftLabel.Location = new System.Drawing.Point(62, 45);
			this.swipeLeftLabel.Name = "SwipeLeftLabel";
			this.swipeLeftLabel.Size = new System.Drawing.Size(121, 28);
			this.swipeLeftLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));

			this.swipeRightLabel.Text = "Swipe Right";
			this.swipeRightLabel.Location = new System.Drawing.Point(62, 90);
			this.swipeRightLabel.Name = "SwipeRightLabel";
			this.swipeRightLabel.Size = new System.Drawing.Size(121, 28);
			this.swipeRightLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));

			this.swipeUpLabel.Text = "Swipe Up";
			this.swipeUpLabel.Location = new System.Drawing.Point(62, 135);
			this.swipeUpLabel.Name = "SwipeUpLabel";
			this.swipeUpLabel.Size = new System.Drawing.Size(121, 28);
			this.swipeUpLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));

			this.swipeDownLabel.Text = "Swipe Down";
			this.swipeDownLabel.Location = new System.Drawing.Point(62, 180);
			this.swipeDownLabel.Name = "SwipeDownLabel";
			this.swipeDownLabel.Size = new System.Drawing.Size(121, 28);
			this.swipeDownLabel.Font = new Font("Microsoft Sans Serif", 12F, FontStyle.Bold, GraphicsUnit.Point, ((byte)(0)));

			// 
			// tabPage3
			// 
			this.tabPage3.Controls.Add(this.groupBox2);
			this.tabPage3.Location = new System.Drawing.Point(4, 29);
			this.tabPage3.Name = "tabPage3";
			this.tabPage3.Size = new System.Drawing.Size(552, 266);
			this.tabPage3.TabIndex = 2;
			this.tabPage3.Text = "Tilt";
			this.tabPage3.ToolTipText = "Set keyboard settings for TILT actions";
			this.tabPage3.UseVisualStyleBackColor = true;
			// 
			// groupBox2
			// 
			this.groupBox2.Controls.Add(this.DownHScroll);
			this.groupBox2.Controls.Add(this.DownTiltLabel);
			this.groupBox2.Controls.Add(this.TiltDownKeyCombo);
			this.groupBox2.Controls.Add(this.TiltDownLabel);
			this.groupBox2.Controls.Add(this.UpHScroll);
			this.groupBox2.Controls.Add(this.UpTiltLabel);
			this.groupBox2.Controls.Add(this.TiltUpKeyCombo);
			this.groupBox2.Controls.Add(this.TiltUpLabel);
			this.groupBox2.Controls.Add(this.RightHScroll);
			this.groupBox2.Controls.Add(this.RightTiltLabel);
			this.groupBox2.Controls.Add(this.TiltRightKeyCombo);
			this.groupBox2.Controls.Add(this.TiltRightLabel);
			this.groupBox2.Controls.Add(this.LeftHScroll);
			this.groupBox2.Controls.Add(this.LeftTiltLabel);
			this.groupBox2.Controls.Add(this.TiltLeftKeyCombo);
			this.groupBox2.Controls.Add(this.TiltLeftLabel);
			this.groupBox2.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.groupBox2.Location = new System.Drawing.Point(8, 16);
			this.groupBox2.Name = "groupBox2";
			this.groupBox2.Size = new System.Drawing.Size(441, 254);
			this.groupBox2.TabIndex = 0;
			this.groupBox2.TabStop = false;
			this.groupBox2.Text = "Device Tilt Actions";
			// 
			// DownHScroll
			// 
			this.DownHScroll.Location = new System.Drawing.Point(108, 190);
			this.DownHScroll.Maximum = 98;
			this.DownHScroll.Name = "DownHScroll";
			this.DownHScroll.Size = new System.Drawing.Size(106, 17);
			this.DownHScroll.TabIndex = 38;
			this.DownHScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.DownHScroll_Scroll);
			// 
			// DownTiltLabel
			// 
			this.DownTiltLabel.AutoSize = true;
			this.DownTiltLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.DownTiltLabel.Location = new System.Drawing.Point(222, 190);
			this.DownTiltLabel.Name = "DownTiltLabel";
			this.DownTiltLabel.Size = new System.Drawing.Size(41, 20);
			this.DownTiltLabel.TabIndex = 37;
			this.DownTiltLabel.Text = "0 Degrees";
			// 
			// TiltDownKeyCombo
			// 
			this.TiltDownKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.TiltDownKeyCombo.FormattingEnabled = true;
			this.TiltDownKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});
			this.TiltDownKeyCombo.Location = new System.Drawing.Point(319, 187);
			this.TiltDownKeyCombo.Name = "TiltDownKeyCombo";
			this.TiltDownKeyCombo.Size = new System.Drawing.Size(116, 28);
			this.TiltDownKeyCombo.TabIndex = 36;
			// 
			// TiltDownLabel
			// 
			this.TiltDownLabel.AutoSize = true;
			this.TiltDownLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TiltDownLabel.Location = new System.Drawing.Point(7, 187);
			this.TiltDownLabel.Name = "TiltDownLabel";
			this.TiltDownLabel.Size = new System.Drawing.Size(74, 20);
			this.TiltDownLabel.TabIndex = 35;
			this.TiltDownLabel.Text = "Tilt Down";
			// 
			// UpHScroll
			// 
			this.UpHScroll.Location = new System.Drawing.Point(108, 141);
			this.UpHScroll.Maximum = 98;
			this.UpHScroll.Name = "UpHScroll";
			this.UpHScroll.Size = new System.Drawing.Size(106, 17);
			this.UpHScroll.TabIndex = 34;
			this.UpHScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.UpHScroll_Scroll);
			// 
			// UpTiltLabel
			// 
			this.UpTiltLabel.AutoSize = true;
			this.UpTiltLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.UpTiltLabel.Location = new System.Drawing.Point(222, 141);
			this.UpTiltLabel.Name = "UpTiltLabel";
			this.UpTiltLabel.Size = new System.Drawing.Size(41, 20);
			this.UpTiltLabel.TabIndex = 33;
			this.UpTiltLabel.Text = "0 Degrees";
			// 
			// TiltUpKeyCombo
			// 
			this.TiltUpKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.TiltUpKeyCombo.FormattingEnabled = true;
			this.TiltUpKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});
			this.TiltUpKeyCombo.Location = new System.Drawing.Point(319, 138);
			this.TiltUpKeyCombo.Name = "TiltUpKeyCombo";
			this.TiltUpKeyCombo.Size = new System.Drawing.Size(116, 28);
			this.TiltUpKeyCombo.TabIndex = 32;
			// 
			// TiltUpLabel
			// 
			this.TiltUpLabel.AutoSize = true;
			this.TiltUpLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TiltUpLabel.Location = new System.Drawing.Point(8, 138);
			this.TiltUpLabel.Name = "TiltUpLabel";
			this.TiltUpLabel.Size = new System.Drawing.Size(54, 20);
			this.TiltUpLabel.TabIndex = 31;
			this.TiltUpLabel.Text = "Tilt Up";
			// 
			// RightHScroll
			// 
			this.RightHScroll.Location = new System.Drawing.Point(108, 90);
			this.RightHScroll.Maximum = 98;
			this.RightHScroll.Name = "RightHScroll";
			this.RightHScroll.Size = new System.Drawing.Size(106, 17);
			this.RightHScroll.TabIndex = 30;
			this.RightHScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.RightHScroll_Scroll);
			// 
			// RightTiltLabel
			// 
			this.RightTiltLabel.AutoSize = true;
			this.RightTiltLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.RightTiltLabel.Location = new System.Drawing.Point(222, 90);
			this.RightTiltLabel.Name = "RightTiltLabel";
			this.RightTiltLabel.Size = new System.Drawing.Size(41, 20);
			this.RightTiltLabel.TabIndex = 29;
			this.RightTiltLabel.Text = "0 Degrees";
			// 
			// TiltRightKeyCombo
			// 
			this.TiltRightKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.TiltRightKeyCombo.FormattingEnabled = true;
			this.TiltRightKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});
			this.TiltRightKeyCombo.Location = new System.Drawing.Point(319, 87);
			this.TiltRightKeyCombo.Name = "TiltRightKeyCombo";
			this.TiltRightKeyCombo.Size = new System.Drawing.Size(116, 28);
			this.TiltRightKeyCombo.TabIndex = 28;
			// 
			// TiltRightLabel
			// 
			this.TiltRightLabel.AutoSize = true;
			this.TiltRightLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TiltRightLabel.Location = new System.Drawing.Point(7, 93);
			this.TiltRightLabel.Name = "TiltRightLabel";
			this.TiltRightLabel.Size = new System.Drawing.Size(71, 20);
			this.TiltRightLabel.TabIndex = 27;
			this.TiltRightLabel.Text = "Tilt Right";
			// 
			// LeftHScroll
			// 
			this.LeftHScroll.Location = new System.Drawing.Point(108, 45);
			this.LeftHScroll.Maximum = 98;
			this.LeftHScroll.Name = "LeftHScroll";
			this.LeftHScroll.Size = new System.Drawing.Size(106, 17);
			this.LeftHScroll.TabIndex = 26;
			this.LeftHScroll.Scroll += new System.Windows.Forms.ScrollEventHandler(this.LeftHScroll_Scroll);
			// 
			// LeftTiltLabel
			// 
			this.LeftTiltLabel.AutoSize = true;
			this.LeftTiltLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.LeftTiltLabel.Location = new System.Drawing.Point(222, 40);
			this.LeftTiltLabel.Name = "LeftTiltLabel";
			this.LeftTiltLabel.Size = new System.Drawing.Size(41, 20);
			this.LeftTiltLabel.TabIndex = 25;
			this.LeftTiltLabel.Text = "0 Degrees";
			// 
			// TiltLeftKeyCombo
			// 
			this.TiltLeftKeyCombo.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			this.TiltLeftKeyCombo.FormattingEnabled = true;
			this.TiltLeftKeyCombo.Items.AddRange(new object[] {
					"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});
			this.TiltLeftKeyCombo.Location = new System.Drawing.Point(319, 37);
			this.TiltLeftKeyCombo.Name = "TiltLeftKeyCombo";
			this.TiltLeftKeyCombo.Size = new System.Drawing.Size(116, 28);
			this.TiltLeftKeyCombo.TabIndex = 24;
			// 
			// TiltLeftLabel
			// 
			this.TiltLeftLabel.AutoSize = true;
			this.TiltLeftLabel.Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			this.TiltLeftLabel.Location = new System.Drawing.Point(7, 42);
			this.TiltLeftLabel.Name = "TiltLeftLabel";
			this.TiltLeftLabel.Size = new System.Drawing.Size(61, 20);
			this.TiltLeftLabel.TabIndex = 23;
			this.TiltLeftLabel.Text = "Tilt Left";
			// 
			// buttonX2
			// 
			this.buttonX2.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
			this.buttonX2.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
			this.buttonX2.Location = new System.Drawing.Point(371, 408);
			this.buttonX2.Name = "buttonX2";
			this.buttonX2.Size = new System.Drawing.Size(85, 33);
			this.buttonX2.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
			this.buttonX2.TabIndex = 27;
			this.buttonX2.Text = "Save";
			this.buttonX2.Click += SaveBtnClick;
			// 
			// buttonX3
			// 
			this.buttonX3.AccessibleRole = System.Windows.Forms.AccessibleRole.PushButton;
			this.buttonX3.ColorTable = DevComponents.DotNetBar.eButtonColor.OrangeWithBackground;
			this.buttonX3.Location = new System.Drawing.Point(272, 408);
			this.buttonX3.Name = "buttonX3";
			this.buttonX3.Size = new System.Drawing.Size(85, 33);
			this.buttonX3.Style = DevComponents.DotNetBar.eDotNetBarStyle.StyleManagerControlled;
			this.buttonX3.TabIndex = 28;
			this.buttonX3.Text = "Cancel";
			this.buttonX3.Click += CancelBtnClick;
			// 
			// Form1
			// 
			this.AutoScaleDimensions = new System.Drawing.SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.ClientSize = new System.Drawing.Size(mOrigWidth, mOrigHeight);
			this.Controls.Add(this.buttonX3);
			this.Controls.Add(this.buttonX2);
			this.Controls.Add(this.tabControl1);
			this.Controls.Add(this.tapHelpTextLabel);
			this.Controls.Add(this.MainPanel);
			this.Name = "InputMapperForm";
			this.Text = "Input Mapper";
			this.Load += new System.EventHandler(this.FormLoad);
			this.tabControl1.ResumeLayout(false);
			this.tabPage1.ResumeLayout(false);
			this.groupBox3.ResumeLayout(false);
			this.groupBox3.PerformLayout();
			this.tabPage2.ResumeLayout(false);
			this.groupBox1.ResumeLayout(false);
			this.tabPage3.ResumeLayout(false);
			this.groupBox2.ResumeLayout(false);
			this.groupBox2.PerformLayout();

			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ShowInTaskbar = false;
			//this.FormBorderStyle		= FormBorderStyle.None;

			this.ResumeLayout(false);

		}

		private void FormLoad(object sender, EventArgs e)
		{
			swipeLeftKeyCombo.SelectedIndex = 0;
			swipeRightKeyCombo.SelectedIndex = 0;
			swipeUpKeyCombo.SelectedIndex = 0;
			swipeDownKeyCombo.SelectedIndex = 0;

			TiltLeftKeyCombo.SelectedIndex = 0;
			TiltRightKeyCombo.SelectedIndex = 0;
			TiltUpKeyCombo.SelectedIndex = 0;
			TiltDownKeyCombo.SelectedIndex = 0;
		}

		private void LeftHScroll_Scroll(object sender, ScrollEventArgs e)
		{
			LeftTiltLabel.Text = e.NewValue + " Degrees";
		}

		private void RightHScroll_Scroll(object sender, ScrollEventArgs e)
		{
			RightTiltLabel.Text = e.NewValue + " Degrees";
		}

		private void UpHScroll_Scroll(object sender, ScrollEventArgs e)
		{
			UpTiltLabel.Text = e.NewValue + " Degrees";
		}

		private void DownHScroll_Scroll(object sender, ScrollEventArgs e)
		{
			DownTiltLabel.Text = e.NewValue + " Degrees";
		}

		private void AddNewMappingBtnClick(object sender, EventArgs e)
		{
			tabControl1.Visible = false;
			buttonX2.Visible = false;
			buttonX3.Visible = false;
			MainPanel.Visible = true;
			tapHelpTextLabel.Visible = true;
			this.FormBorderStyle = FormBorderStyle.None;
			this.ClientSize = new System.Drawing.Size(mWidth, mHeight);
			this.Location = new Point(mLeft, mTop);
			this.Opacity = 0.70;
		}

		private void MainPanelClick(object sender, EventArgs e)
		{
			MouseEventArgs me = e as MouseEventArgs;
			float x = (me.X * 100 / mWidth);
			float y = (me.Y * 100 / mHeight);

			tabControl1.Visible = true;
			buttonX2.Visible = true;
			buttonX3.Visible = true;
			MainPanel.Visible = false;
			tapHelpTextLabel.Visible = false;
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.ClientSize = new System.Drawing.Size(mOrigWidth, mOrigHeight);
			this.Opacity = 1.0;

			mTapX = x;
			mTapY = y;

			if (mIsModifyingMapping == false)
			{
				AddNewMapping(x, y);
			}
			mIsModifyingMapping = false;
		}

		private void CancelBtnClick(object sender, EventArgs e)
		{
			this.Dispose();
		}

		private void AddNewMapping(float x, float y)
		{
			int pos = mTapLocMappingIndex;
			if (pos >= mMaxMappingsSupported)
			{
				MessageBox.Show("Only " + mMaxMappingsSupported + " new mappings are supported");
				return;
			}

			TapAtLocLabel[pos] = new Label();
			TapAtLocLabel[pos].AutoSize = true;
			TapAtLocLabel[pos].Font = new System.Drawing.Font("Microsoft Sans Serif", 12F, System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			TapAtLocLabel[pos].Location = new Point(30, 15 + 20 * (pos + 1) + (20 * pos));
			TapAtLocLabel[pos].Name = "TapAtLoc" + pos + "Label";
			TapAtLocLabel[pos].Size = new System.Drawing.Size(110, 20);
			TapAtLocLabel[pos].Text = "Tap at (" + x + "," + y + ")";
			TapAtLocLabel[pos].Cursor = Cursors.Hand;
			TapAtLocLabel[pos].Click += ChangeTapLocation;

			tapLocationTooltip.SetToolTip(TapAtLocLabel[pos], "Click to change tap position");

			ConnectingArrowsPBox[pos] = new PictureBox();
			ConnectingArrowsPBox[pos].Location = new Point(TapAtLocLabel[pos].Right + 30, TapAtLocLabel[pos].Top);
			ConnectingArrowsPBox[pos].Size = new Size(100, 20);
			Image img = Image.FromFile(@"C:\arrows.png");
			ConnectingArrowsPBox[pos].Visible = true;
			ConnectingArrowsPBox[pos].Image = img;
			ConnectingArrowsPBox[pos].BackgroundImage = img;
			ConnectingArrowsPBox[pos].SizeMode = PictureBoxSizeMode.Zoom;

			TapAtLocKeyCombo[pos] = new ComboBox();
			TapAtLocKeyCombo[pos].DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
			TapAtLocKeyCombo[pos].FormattingEnabled = true;
			TapAtLocKeyCombo[pos].Location = new Point(289, 15 + 20 * (pos + 1) + 20 * pos);
			TapAtLocKeyCombo[pos].Name = "TapAtLoc" + pos + "KeyCombo";
			TapAtLocKeyCombo[pos].Size = new System.Drawing.Size(116, 20);
			TapAtLocKeyCombo[pos].Items.AddRange(new object[] {
				"Disabled", "1", "2", "3", "4", "5", "6", "7", "8", "9", "0", "A", "B", "C", "D", "E", "F", "G", "H", "I", "J", "K", "L", "M", "N", "O", "P", "Q", "R", "S", "T", "U", "V", "W", "X", "Y", "Z", "Space", "Enter", "Up-Arrow", "Down-Arrow", "Left-Arrow", "Right-Arrow"});
			TapAtLocKeyCombo[pos].SelectedIndex = 0;

			groupBox3.Controls.Add(TapAtLocKeyCombo[pos]);
			groupBox3.Controls.Add(ConnectingArrowsPBox[pos]);
			groupBox3.Controls.Add(TapAtLocLabel[pos]);

			mTapLocMappingIndex++;
		}

		private void ChangeTapLocation(object sender, EventArgs e)
		{
			mIsModifyingMapping = true;
			AddNewMappingBtnClick(sender, e);

			Label clickedLabel = sender as Label;
			clickedLabel.Text = "Tap at (" + mTapX + "," + mTapY + ")";
		}

		private string mKeysTemplate = "[keys]";
		private string mOpenSensorTemplate = "[opensensor]";
		private void SaveBtnClick(object sender, EventArgs e)
		{
			string fileName = sCurrentAppPackage + ".cfg";
			string filePath = Path.Combine(Common.Strings.InputMapperFolder, fileName);

			string tapMappings = GetNewTapMappings();
			string tiltMappings = GetNewTiltMappings();
			string swipeMappings = GetNewSwipeMappings();

			// Generate config file
			StreamWriter file = new StreamWriter(filePath);
			file.WriteLine(mKeysTemplate);
			file.WriteLine(tapMappings);
			file.WriteLine(tiltMappings);
			file.WriteLine(swipeMappings);

			file.WriteLine(mOpenSensorTemplate);

			file.Close();

			Console.s_Console.SetupInputMapper();
			// XXX: can we get rid of app restart?
			MessageBox.Show("Keyboard mappings saved. Please restart the app for the changes to take effect");
			this.Dispose();
		}

		private string GetNewTapMappings()
		{
			string result = "\r\n";

			if (TapAtLocLabel == null)
				return result;

			int pos = 0;
			for (pos = 0; pos < mMaxMappingsSupported; pos++)
			{
				try
				{
					if (TapAtLocLabel[pos] != null &&
							TapAtLocKeyCombo[pos].SelectedItem.ToString() != "Disabled")
					{
						string coordinates = TapAtLocLabel[pos].Text.Substring(
								TapAtLocLabel[pos].Text.IndexOf('('));
						result += string.Format("{0}\t= Tap {1}\r\n",
								TapAtLocKeyCombo[pos].SelectedItem.ToString(),
								coordinates);
					}
					else
					{
						break;
					}
					pos++;
				}
				catch (Exception ex)
				{
					Logger.Error("Failed to add Tap mappings for index {0}. err: {1}", pos, ex.ToString());
				}
			}

			return result.Replace("-Arrow", "");
		}

		private string GetNewTiltMappings()
		{
			string result = "\r\n";
			string degrees = "";

			string tiltLeftKey = (string)TiltLeftKeyCombo.SelectedItem.ToString();
			string tiltRightKey = (string)TiltRightKeyCombo.SelectedItem.ToString();
			string tiltUpKey = (string)TiltUpKeyCombo.SelectedItem.ToString();
			string tiltDownKey = (string)TiltDownKeyCombo.SelectedItem.ToString();

			if (tiltLeftKey != "Disabled")
			{
				degrees = LeftTiltLabel.Text.Substring(0, LeftTiltLabel.Text.IndexOf(' '));
				result += string.Format("{0}\t= Tilt Absolute (0,-{1}) Return\r\n", tiltLeftKey, degrees);
			}

			if (tiltRightKey != "Disabled")
			{
				degrees = RightTiltLabel.Text.Substring(0, RightTiltLabel.Text.IndexOf(' '));
				result += string.Format("{0}\t= Tilt Absolute (0,{1}) Return\r\n", tiltRightKey, degrees);
			}
			if (tiltUpKey != "Disabled")
			{
				degrees = UpTiltLabel.Text.Substring(0, UpTiltLabel.Text.IndexOf(' '));
				result += string.Format("{0}\t= Tilt Absolute (-{1},0) Return\r\n", tiltUpKey, degrees);
			}
			if (tiltDownKey != "Disabled")
			{
				degrees = DownTiltLabel.Text.Substring(0, DownTiltLabel.Text.IndexOf(' '));
				result += string.Format("{0}\t= Tilt Absolute ({1},0) Return\r\n", tiltDownKey, degrees);
			}

			return result.Replace("-Arrow", "");
		}

		private string GetNewSwipeMappings()
		{
			string result = "\r\n";

			string swipeLeftKey = (string)swipeLeftKeyCombo.SelectedItem.ToString();
			string swipeRightKey = (string)swipeRightKeyCombo.SelectedItem.ToString();
			string swipeUpKey = (string)swipeUpKeyCombo.SelectedItem.ToString();
			string swipeDownKey = (string)swipeDownKeyCombo.SelectedItem.ToString();

			if (swipeLeftKey != "Disabled")
			{
				result += string.Format("{0}\t= Swipe Left\r\n", swipeLeftKey);
			}

			if (swipeRightKey != "Disabled")
			{
				result += string.Format("{0}\t= Swipe Right\r\n", swipeRightKey);
			}

			if (swipeUpKey != "Disabled")
			{
				result += string.Format("{0}\t= Swipe Up\r\n", swipeUpKey);
			}

			if (swipeDownKey != "Disabled")
			{
				result += string.Format("{0}\t= Swipe Down\r\n", swipeDownKey);
			}

			return result.Replace("-Arrow", "");
		}

		private void GetExistingMappings()
		{
			string fileName = sCurrentAppPackage + ".cfg";
			string filePath = Path.Combine(Common.Strings.InputMapperFolder, fileName);

			if (!File.Exists(filePath))
			{
			}
		}
	}
}
