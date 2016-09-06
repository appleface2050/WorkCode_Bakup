using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Text;
using System.Net.Security;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Cloud.Services;
using CodeTitans.JSon;
using Gecko;
using System.Drawing.Imaging;

namespace BlueStacks.hyperDroid.GameManager
{
	public class GameManagerSelectTheme : Form
	{
		public Size mWindowSize;
		public static int sFrontendWidth;
		public static int sFrontendHeight;

		private string mPackage;
		private string mActivity;	

		public GameManagerSelectTheme(string package, string activity)
		{
			try
			{
				this.mPackage = package;
				this.mActivity = activity;
				int width =  Convert.ToInt32(GameManager.sFrontendWidth * .5);
				int height = Convert.ToInt32(GameManager.sFrontendHeight * .5);
				int topPadding		= Convert.ToInt32(height*.15);
				int bottomPadding	= Convert.ToInt32(height*.2);
				int leftPadding		= Convert.ToInt32(width*.02);
				int rightPadding	= Convert.ToInt32(width*.02);
				int imageSpacing	= Convert.ToInt32(width*.04);

				int textFontSize = Convert.ToInt32(height*.05);
				Label label = new Label();
                label.Text = GameManager.sLocalizedString.ContainsKey("SelectTheme") ? GameManager.sLocalizedString["SelectTheme"] : "Select Theme";
				label.Font = new Font("arial", textFontSize, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
				label.Width = width - leftPadding - rightPadding;
				label.Height = Convert.ToInt32(height * .1);
				label.Location = new Point(leftPadding, topPadding);
				label.ForeColor = ColorTranslator.FromHtml("#8d9ba3");
				label.TextAlign = ContentAlignment.TopCenter;
				string path = Directory.GetCurrentDirectory();
                Image oldThemeImage = Image.FromFile(Path.Combine(GameManager.sAssetsCommonDataDir, "oldTheme.png"));
                Image newThemeImage = Image.FromFile(Path.Combine(GameManager.sAssetsCommonDataDir, "newTheme.png"));
				PictureBox oldTheme = new PictureBox();
				PictureBox newTheme = new PictureBox();

				oldTheme.Size = new Size(Convert.ToInt32(width * .45), Convert.ToInt32(height * .45));
				newTheme.Size = new Size(Convert.ToInt32(width * .45), Convert.ToInt32(height * .45));

				oldTheme.Image = oldThemeImage;
				oldTheme.SizeMode = PictureBoxSizeMode.StretchImage;
				oldTheme.Click += new System.EventHandler(this.btnOldTheme_Clicked);
				oldTheme.Location = new Point(
						leftPadding,
						height - oldTheme.Height - bottomPadding);
				oldTheme.MouseEnter += new EventHandler(this.ButtonMouseEnter);
				oldTheme.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
				oldTheme.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
				oldTheme.MouseLeave += new EventHandler(this.ButtonMouseLeave);

                newTheme.Image = newThemeImage;
				newTheme.SizeMode = PictureBoxSizeMode.StretchImage;
				newTheme.Click += new System.EventHandler(this.btnNewTheme_Clicked);
				newTheme.Location = new Point(
						oldTheme.Right + imageSpacing,
						height - newTheme.Height - bottomPadding);
				newTheme.MouseEnter += new EventHandler(this.ButtonMouseEnter);
				newTheme.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
				newTheme.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
				newTheme.MouseLeave += new EventHandler(this.ButtonMouseLeave);

				mWindowSize = new Size(width, height);
				this.ClientSize = mWindowSize;
				Logger.Info("mWindowsize = "+mWindowSize.Width+" x "+mWindowSize.Height);

                CustomizedToolTip toolTip = new CustomizedToolTip();
                newTheme.Tag = new Bitmap(newThemeImage);
                oldTheme.Tag = new Bitmap(oldThemeImage); ;
                toolTip.AutoSize = false;
                toolTip.Size = new Size(Convert.ToInt32(this.Width * .8), Convert.ToInt32(this.Height * .8));
                toolTip.SetToolTip(newTheme, " ");
                toolTip.SetToolTip(oldTheme, " ");

				this.Controls.Add(label);
				this.Controls.Add(oldTheme);
				this.Controls.Add(newTheme);

				this.Icon = Utils.GetApplicationIcon();
				this.StartPosition = FormStartPosition.CenterScreen;
				this.Text = GameManager.sWindowTitle;
				this.BackColor = Color.White;
				this.MinimizeBox = false;
				this.MaximizeBox = false;
                this.MaximumSize = this.MinimumSize = mWindowSize;
				this.Paint += new System.Windows.Forms.PaintEventHandler(this.FormGradientPaint);

			}
			catch(Exception Ex)
			{
				MessageBox.Show(Ex.ToString());	
			}
		}

		private void btnOldTheme_Clicked(object sender, EventArgs e)
		{
			Logger.Info("Old theme selected");
			using (RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath))
			{
				configKey.SetValue("TabStyleTheme", "Default");
				configKey.SetValue("ParentStyleTheme", "Em");
			}

			GameManager gameManager = new GameManager(this.mPackage, this.mActivity);
			this.Hide();
			gameManager.Show();
		}

		private void btnNewTheme_Clicked(object sender, EventArgs e)
		{
			Logger.Info("New theme selected");
			using (RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath))
			{
				configKey.SetValue("TabStyleTheme", "Default");
				configKey.SetValue("ParentStyleTheme", "Toob");
			}

			GameManager gameManager = new GameManager(this.mPackage, this.mActivity);
			this.Hide();
			gameManager.Show();
		}

		private void FormGradientPaint(object sender, PaintEventArgs e)
		{
			if (this.ClientSize.Width == 0 || this.ClientSize.Height == 0)
				return;

			//int tabBarWidth = this.Width - mControlBarRight.mControlBarWidth;
			Pen sBorderPen = new Pen(Color.Black);
//			e.Graphics.DrawRectangle(sBorderPen, new Rectangle(0, 0, this.Width -1, this.Height -1));
			e.Graphics.DrawLine(sBorderPen, 0, 0, this.Right, 0);
		}

		private void ButtonMouseEnter(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;

			//switch (pictureBox.Name)
			//{
			//	case "Yes":
			pictureBox.Cursor = Cursors.Hand;
			int movement = 5;
			int dx = 0;
			int numSteps = 4;
			int step = 0;
			while (numSteps > 0)
			{
				if (step == 0)
				{
					dx = movement;
				}
				else if (step == 1)
				{
					dx = movement * -1;
				}
				else if (step == 2)
				{
					dx = movement * -1;
				}
				else if (step == 3)
				{
					dx = movement;
				}

				step++;
				if (step == 4)
				{
					step = 0;
					numSteps--;
				}

				pictureBox.Left = pictureBox.Left + dx;
				Thread.Sleep(5);
			}
			pictureBox.BackColor = ColorTranslator.FromHtml("#55e0cc");
			//		break;

			//	case "No":
			//		button.BackColor = ColorTranslator.FromHtml("#f66a4c");
			//		break;

			//	case "Cancel":
			//		button.BackColor = ColorTranslator.FromHtml("#88a3b3");
			//		break;
			//}
		}


		private void ButtonMouseDown(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;

			//switch (pictureBox.Name)
			//{
			//	case "Yes":
					pictureBox.BackColor = ColorTranslator.FromHtml("#a9efe1");
			//		break;

			//	case "No":
			//		button.BackColor = ColorTranslator.FromHtml("#fbb5a6");
			//		break;
//
//				case "Cancel":
//					button.BackColor = ColorTranslator.FromHtml("#c4d1d9");
//					break;
//			}
		}

		private void ButtonMouseUp(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;

//			switch (pictureBox.Name)
//			{
//				case "Yes":
					pictureBox.BackColor = ColorTranslator.FromHtml("#55e0cc");
//					break;

//				case "No":
//					button.BackColor = ColorTranslator.FromHtml("#f66a4c");
//					break;
//
//				case "Cancel":
//					button.BackColor = ColorTranslator.FromHtml("#88a3b3");
//					break;
//			}
		}

		private void ButtonMouseLeave(object sender, EventArgs e)
		{
			PictureBox pictureBox = (PictureBox)sender;

//			switch (pictureBox.Name)
//			{
//				case "Yes":
			pictureBox.Cursor = Cursors.Default;
			pictureBox.BackColor = ColorTranslator.FromHtml("#84d2e4");
//					break;

//				case "No":
//					button.BackColor = ColorTranslator.FromHtml("#f64c4c");
//					break;

//				case "Cancel":
//					button.BackColor = ColorTranslator.FromHtml("#c1cfd8");
//					break;
//			}
		}

	}
}
