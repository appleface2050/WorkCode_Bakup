/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Library
 */

using System;
using System.Drawing;
using System.Diagnostics;
using System.Collections;
using System.Collections.Generic;
using System.ComponentModel;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using DevComponents.DotNetBar;
using System.Threading;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Common
{
	class CustomAlert : DevComponents.DotNetBar.Balloon
	{
		private DevComponents.DotNetBar.Controls.ReflectionImage reflectionImage1;
		private DevComponents.DotNetBar.Bar displayBar;
		private DevComponents.DotNetBar.ButtonItem buttonItem3;
		private DevComponents.DotNetBar.LabelX lblTitle;
		private DevComponents.DotNetBar.LabelX lblMsg;
		private static int s_numAlerts = 0;

		public static Rectangle s_screenSize = Screen.PrimaryScreen.WorkingArea;

		private static string s_FontName = Utils.GetSystemFontName();

		private static Image ResizeImage(Image src)
		{
			int width = 64;
			int height = 64;

			Image dst = new Bitmap(width, height);

			Graphics g = Graphics.FromImage(dst);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(src, 0, 0, dst.Width, dst.Height);

			src.Dispose();
			return dst;
		}

		public CustomAlert(
				Image image,
				string title,
				string displayMsg,
				bool autoClose,
				System.EventHandler clickHandler
				)
		{
			string displayTitle = "<b>" + title + "</b>";

			this.ShowInTaskbar = false;
			this.FormBorderStyle = FormBorderStyle.FixedToolWindow;

			this.reflectionImage1 = new DevComponents.DotNetBar.Controls.ReflectionImage();
			this.displayBar = new DevComponents.DotNetBar.Bar();
			this.buttonItem3 = new DevComponents.DotNetBar.ButtonItem();
			this.lblTitle = new DevComponents.DotNetBar.LabelX();
			this.lblMsg = new DevComponents.DotNetBar.LabelX();

			((System.ComponentModel.ISupportInitialize)(this.displayBar)).BeginInit();
			this.SuspendLayout();
			// 
			// reflectionImage1
			// 
			this.reflectionImage1.BackColor = System.Drawing.Color.Transparent;
			this.reflectionImage1.Image = ResizeImage(image);
			this.reflectionImage1.Location = new System.Drawing.Point(8, 8);
			this.reflectionImage1.Name = "reflectionImage1";
			this.reflectionImage1.Size = new System.Drawing.Size(64, 100);
			this.reflectionImage1.TabIndex = 0;
			// 
			// displayBar
			// 
			this.displayBar.BackColor = System.Drawing.Color.Transparent;
			this.displayBar.Dock = System.Windows.Forms.DockStyle.Bottom;
			this.displayBar.Location = new System.Drawing.Point(0, 111);
			this.displayBar.Name = "displayBar";
			this.displayBar.Size = new System.Drawing.Size(280, 25);
			this.displayBar.Stretch = true;
			this.displayBar.Style = DevComponents.DotNetBar.eDotNetBarStyle.Office2007;
			this.displayBar.TabIndex = 1;
			this.displayBar.TabStop = false;
			this.displayBar.Text = "displayBar";
			// 
			// lblTitle
			// 
			this.lblTitle.BackColor = System.Drawing.Color.Transparent;
			this.lblTitle.Location = new System.Drawing.Point(80, 20);
			this.lblTitle.Name = "lblTitle";
			this.lblTitle.Size = new System.Drawing.Size(260, 60);
			this.lblTitle.TabIndex = 2;
			this.lblTitle.Text = displayTitle;
			this.lblMsg.WordWrap = true;
			this.lblTitle.Font = new System.Drawing.Font(s_FontName, 12F,
					System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// lblMsg
			// 
			this.lblMsg.BackColor = System.Drawing.Color.Transparent;
			this.lblMsg.Location = new System.Drawing.Point(80, 78);
			this.lblMsg.Name = "lblMsg";
			this.lblMsg.Size = new System.Drawing.Size(260, 75);
			this.lblMsg.TabIndex = 3;
			this.lblMsg.Text = displayMsg;
			this.lblMsg.TextLineAlignment = System.Drawing.StringAlignment.Near;
			this.lblMsg.WordWrap = true;
			this.lblMsg.Font = new System.Drawing.Font(s_FontName, 10F,
					System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, ((byte)(0)));
			// 
			// CustomAlert
			// 
			this.AutoScaleBaseSize = new System.Drawing.Size(5, 13);
			this.BackColor = System.Drawing.Color.FromArgb(((System.Byte)(227)), ((System.Byte)(239)), ((System.Byte)(255)));
			this.BackColor2 = System.Drawing.Color.FromArgb(((System.Byte)(175)), ((System.Byte)(210)), ((System.Byte)(255)));
			this.BorderColor = System.Drawing.Color.FromArgb(((System.Byte)(101)), ((System.Byte)(147)), ((System.Byte)(207)));
			this.CaptionFont = new System.Drawing.Font(s_FontName, 12F, System.Drawing.FontStyle.Bold);
			this.ClientSize = new System.Drawing.Size(350, 160);

			this.Controls.AddRange(new System.Windows.Forms.Control[] {
					this.lblMsg,
					this.lblTitle,
					this.displayBar,
					this.reflectionImage1});

			foreach (Control control in this.Controls)
			{
				if (clickHandler != null)
					control.Click += clickHandler;
				control.Click += delegate (Object o, System.EventArgs e) { this.Close(); };
			}

			this.TopMost = true;
			this.ForeColor = System.Drawing.Color.FromArgb(((System.Byte)(8)), ((System.Byte)(55)), ((System.Byte)(114)));
			this.Location = new System.Drawing.Point(0, 0);
			this.Name = "CustomAlert";
			this.Style = DevComponents.DotNetBar.eBallonStyle.Office2007Alert;
			((System.ComponentModel.ISupportInitialize)(this.displayBar)).EndInit();
			this.ResumeLayout(false);

			this.FormClosing += new System.Windows.Forms.FormClosingEventHandler(this.AlertFormClosing);

			if (autoClose == true)
			{
				this.AutoCloseTimeOut = 10; // 10 sec
			}
			else
			{
				// if 'AutoClose' is set to false, the 'Close'
				// button doesn't work. set a very high time out period.
				this.AutoCloseTimeOut = 24 * 60 * 60;   // 1 day
			}

			this.AutoClose = true;
			this.AlertAnimation = eAlertAnimation.RightToLeft;
			this.AlertAnimationDuration = 300;
			s_numAlerts++;
			this.Location = new Point(s_screenSize.Right - this.Width,
					s_screenSize.Bottom - (this.Height * s_numAlerts));
			this.Show(false);
		}

		public CustomAlert(
				string imagePath,
				string title,
				string displayMsg,
				bool autoClose,
				System.EventHandler clickHandler
				)
			: this(Image.FromFile(imagePath),
					title,
					displayMsg,
					autoClose,
					clickHandler)
		{
		}

		private void AlertFormClosing(object sender, FormClosingEventArgs e)
		{
			s_numAlerts--;
		}

		public static void ShowAlert(
				string imagePath,
				string title,
				string displayMsg,
				bool autoClose,
				System.EventHandler clickHandler
				)
		{
			ShowAlert(Image.FromFile(imagePath), title, displayMsg, autoClose, clickHandler);
		}

		public static void ShowAlert(
				Image image,
				string title,
				string displayMsg,
				bool autoClose,
				System.EventHandler clickHandler
				)
		{
			Thread dialogThread = new Thread(delegate ()
			{
				Application.Run(new CustomAlert(image, title, displayMsg, autoClose, clickHandler));
			});
			dialogThread.IsBackground = true;
			dialogThread.Start();
		}

		public static void ShowInstallAlert(
				string imagePath,
				string title,
				string displayMsg,
				System.EventHandler clickHandler
				)
		{
			if (BlueStacks.hyperDroid.Common.Features.IsFeatureEnabled(BlueStacks.hyperDroid.Common.Features.INSTALL_NOTIFICATIONS))
				ShowAlert(imagePath, title, displayMsg, true, clickHandler);
		}

		public static void ShowUninstallAlert(
				string imagePath,
				string title,
				string displayMsg
				)
		{
			if (BlueStacks.hyperDroid.Common.Features.IsFeatureEnabled(BlueStacks.hyperDroid.Common.Features.UNINSTALL_NOTIFICATIONS))
				ShowAlert(imagePath, title, displayMsg, true, null);
		}

		public static void ShowCloudConnectedAlert(
				string imagePath,
				string title,
				string displayMsg
				)
		{
			ShowAlert(imagePath, title, displayMsg, true, null);
		}

		public static void ShowCloudDisconnectedAlert(
				string imagePath,
				string title,
				string displayMsg,
				System.EventHandler clickHandler
				)
		{
			ShowAlert(imagePath, title, displayMsg, true, clickHandler);
		}

		public static void ShowCloudAnnouncement(
				string imagePath,
				string title,
				string displayMsg,
				bool autoClose,
				System.EventHandler clickHandler
				)
		{
			ShowAlert(imagePath, title, displayMsg, autoClose, clickHandler);
		}

		public static void ShowCloudAnnouncement(
				Image image,
				string title,
				string displayMsg,
				bool autoClose,
				System.EventHandler clickHandler
				)
		{
			ShowAlert(image, title, displayMsg, autoClose, clickHandler);
		}

		public static void ShowSMSMessage(
				string imagePath,
				string title,
				string displayMsg
				)
		{
			ShowAlert(imagePath, title, displayMsg, false, null);
		}

		public static void ShowAndroidNotification(
				string imagePath,
				string title,
				string displayMsg,
				System.EventHandler clickHandler
				)
		{
			if (s_numAlerts >= 1)
			{
				Logger.Info("Another alert already being displayed. Not showing another one.", title);
				return;
			}

			ShowAlert(imagePath, title, displayMsg, true, clickHandler);
		}

	}
}
