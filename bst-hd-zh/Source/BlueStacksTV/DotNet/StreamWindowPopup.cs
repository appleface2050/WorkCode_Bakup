using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	class StreamWindowPopup : Form
	{
		private static int sWidth;
		private static int sHeight;
		private static int sButtonHeight;
		private static int sButtonWidth;
		private static int sButtonFontSize;
		private static int sTextFontSize;

		private static int sLeftButtonWidth;
		private static int sRightButtonWidth;

		private int mBorderSize = 1;
		private int mPadding = 7;
		private int mButtonPadding = 27;
		private int extraHeight = 7;
		private int mCloseButtonWidth;
		private int mCloseButtonHeight;

		private int mMaxLabelWidth = 400;

		private Label lblBodyText;
		private Label lblLeftButton;
		private Label lblRightButton;
		private PictureBox mCloseButton;
		private PictureBox mLeftButton;
		private PictureBox mRightButton;

		private Dictionary<string, Image> mAllImagesDict = null;
		private StreamWindow mStreamWindow;
		public StreamWindowPopup(StreamWindow streamWindow, string bodyText, string leftButtonText, string rightButtonText)
		{
			Logger.Info("Streaming window popup is launcehd");
			this.SuspendLayout();
			mStreamWindow = streamWindow;

			Init();
			ChangeButtonSize(leftButtonText, "left");
			ChangeButtonSize(rightButtonText, "right");

            if ((sLeftButtonWidth + sRightButtonWidth + 2*mButtonPadding + 4*mPadding)
        		    > sWidth)
            {
                 sLeftButtonWidth = (sWidth/2 - 2*mButtonPadding);
                 sRightButtonWidth = sLeftButtonWidth;
                 sButtonHeight = (int)(sButtonHeight * 1.5);
            }
            else
            {
			if (sLeftButtonWidth > sRightButtonWidth)
				sRightButtonWidth = sLeftButtonWidth;
			else
				sLeftButtonWidth = sRightButtonWidth;
            }

			this.Text = "BlueStacks TV";

			this.mCloseButton = new System.Windows.Forms.PictureBox();
			this.mCloseButton.Tag = "tool_close";
			this.mCloseButton.Location = new Point(sWidth - mCloseButtonWidth - 2 * mPadding, 0);
			this.mCloseButton.MouseClick += HandleRightButtonMouseClick;
			this.mCloseButton.Image = mAllImagesDict[(String)this.mCloseButton.Tag];
			this.mCloseButton.SizeMode = PictureBoxSizeMode.StretchImage;
			this.mCloseButton.Size = new Size(mCloseButtonWidth, mCloseButtonHeight);
			this.mCloseButton.MouseEnter += new EventHandler(this.ControlBarButtonMouseEnter);
			this.mCloseButton.MouseDown += new MouseEventHandler(this.ControlBarButtonMouseDown);
			this.mCloseButton.MouseUp += new MouseEventHandler(this.ControlBarButtonMouseUp);
			this.mCloseButton.MouseLeave += new EventHandler(this.ControlBarButtonMouseLeave);
			this.mCloseButton.BackColor = GMColors.TransparentColor;

			this.lblBodyText = new System.Windows.Forms.Label();
			this.lblBodyText.ForeColor = GMColors.StreamWindowForeColor;
			this.lblBodyText.Text = bodyText.Replace("\\n", "\n");
			this.lblBodyText.MaximumSize = new Size(mMaxLabelWidth, 0);
			this.lblBodyText.AutoSize = true;
			this.lblBodyText.BackColor = Color.Transparent;
			this.lblBodyText.Font = new Font(GameManagerUtilities.GetFont(), sTextFontSize, FontStyle.Regular,
					GraphicsUnit.Point, ((byte)(0)));
			this.lblBodyText.Location = new Point(mButtonPadding + mBorderSize, mButtonPadding + mBorderSize + extraHeight);


			this.mRightButton = new System.Windows.Forms.PictureBox();
			this.mRightButton.Tag = "endstream_button";
			this.mRightButton.Image = mAllImagesDict[(String)this.mRightButton.Tag];
			this.mRightButton.Size = new Size(sRightButtonWidth, sButtonHeight);
			this.mRightButton.Location = new Point(sWidth - this.mRightButton.Size.Width - mBorderSize - mButtonPadding - 2 * mPadding, sHeight - sButtonHeight - mBorderSize - mPadding - 2 * mButtonPadding);
			this.mRightButton.SizeMode = PictureBoxSizeMode.StretchImage;
			this.mRightButton.BackColor = GMColors.TransparentColor;

			this.lblRightButton = new System.Windows.Forms.Label();
			this.lblRightButton.Size = this.mRightButton.Size;
			this.lblRightButton.TextAlign = ContentAlignment.MiddleCenter;
			this.lblRightButton.ForeColor = GMColors.ContextMenuForeColor;
			this.lblRightButton.MouseClick += HandleRightButtonMouseClick;
			this.lblRightButton.MouseEnter += new EventHandler(this.ControlBarButtonMouseEnter);
			this.lblRightButton.MouseDown += new MouseEventHandler(this.ControlBarButtonMouseDown);
			this.lblRightButton.MouseUp += new MouseEventHandler(this.ControlBarButtonMouseUp);
			this.lblRightButton.MouseLeave += new EventHandler(this.ControlBarButtonMouseLeave);
			this.lblRightButton.Font = new Font(GameManagerUtilities.GetFont(), sButtonFontSize, FontStyle.Regular,
					GraphicsUnit.Point, ((byte)(0)));

			this.lblRightButton.Text = rightButtonText;
			this.mRightButton.Controls.Add(this.lblRightButton);

			this.mLeftButton = new System.Windows.Forms.PictureBox();
			this.mLeftButton.Tag = "endstream_button";
			this.mLeftButton.Size = new Size(sLeftButtonWidth, sButtonHeight);
			this.mLeftButton.Location = new Point(this.mRightButton.Left - this.mLeftButton.Size.Width - mButtonPadding, sHeight - sButtonHeight - mBorderSize - mPadding - 2 * mButtonPadding);
			this.mLeftButton.Image = mAllImagesDict[(String)this.mLeftButton.Tag];
			this.mLeftButton.SizeMode = PictureBoxSizeMode.StretchImage;
			this.mLeftButton.BackColor = GMColors.TransparentColor;

			this.lblLeftButton = new System.Windows.Forms.Label();
			this.lblLeftButton.Size = this.mLeftButton.Size;
			this.lblLeftButton.TextAlign = ContentAlignment.MiddleCenter;
			this.lblLeftButton.ForeColor = GMColors.StreamWindowForeColor;
			this.lblLeftButton.MouseEnter += new EventHandler(this.ControlBarButtonMouseEnter);
			this.lblLeftButton.MouseDown += new MouseEventHandler(this.ControlBarButtonMouseDown);
			this.lblLeftButton.MouseUp += new MouseEventHandler(this.ControlBarButtonMouseUp);
			this.lblLeftButton.MouseLeave += new EventHandler(this.ControlBarButtonMouseLeave);
			this.lblLeftButton.MouseClick += HandleLeftButtonMouseClick;
			this.lblLeftButton.Font = new Font(GameManagerUtilities.GetFont(), sButtonFontSize, FontStyle.Regular,
					GraphicsUnit.Point, ((byte)(0)));
			this.lblLeftButton.Text = leftButtonText;
			this.mLeftButton.Controls.Add(this.lblLeftButton);

			this.StartPosition = FormStartPosition.CenterScreen;
			this.Size = new Size(sWidth, sHeight);
			this.BackColor = GMColors.StreamWindowBackColor;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			this.Icon = Utils.GetApplicationIcon();
			this.BackgroundImage = mAllImagesDict["endstream_popup_bk"];
			this.BackgroundImageLayout = ImageLayout.Stretch;

			this.Controls.Add(this.mCloseButton);
			this.Controls.Add(this.mLeftButton);
			this.Controls.Add(this.mRightButton);
			this.Controls.Add(this.lblBodyText);
			this.ResumeLayout(false);
		}

		private void Init()
		{
			sWidth = 500;
			sHeight = (int)(sWidth / 2.1);
			sButtonWidth = (int)(sWidth / 2.5);
			sButtonFontSize = 8;
			sTextFontSize = 10;

			mCloseButtonHeight = 27;
			mCloseButtonWidth = (int)(mCloseButtonHeight * 1.2);

			sWidth = sWidth * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			sHeight = sHeight * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			sButtonWidth = sButtonWidth * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			mCloseButtonHeight = mCloseButtonHeight * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			mCloseButtonWidth = mCloseButtonWidth * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			sTextFontSize = sTextFontSize * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			sButtonFontSize = sButtonFontSize * Utils.CurrentDPI / Utils.DEFAULT_DPI;

			sButtonHeight = (int)(sHeight / 6.5);

			mMaxLabelWidth = mMaxLabelWidth * Utils.CurrentDPI / Utils.DEFAULT_DPI;

			mPadding = mPadding * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			mButtonPadding = mButtonPadding * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			extraHeight = extraHeight * Utils.CurrentDPI / Utils.DEFAULT_DPI;
			string assetsDir = Common.Strings.GMAssetDir;
			if (mAllImagesDict == null)
			{
				mAllImagesDict = new Dictionary<string, Image>();
				mAllImagesDict.Add("tool_close",
						Image.FromFile(Path.Combine(assetsDir, "close_button.png")));
				mAllImagesDict.Add("tool_close_hover",
						Image.FromFile(Path.Combine(assetsDir, "close_button_hover.png")));
				mAllImagesDict.Add("tool_close_click",
						Image.FromFile(Path.Combine(assetsDir, "close_button_click.png")));
				mAllImagesDict.Add("endstream_popup_bk",
							Image.FromFile(Path.Combine(assetsDir, "endstream_popup_bk.png")));
				mAllImagesDict.Add("endstream_button",
						Image.FromFile(Path.Combine(assetsDir, "endstream_button.png")));
				mAllImagesDict.Add("endstream_button_click",
						Image.FromFile(Path.Combine(assetsDir, "endstream_button_click.png")));
				mAllImagesDict.Add("endstream_button_hover",
						Image.FromFile(Path.Combine(assetsDir, "endstream_button_hover.png")));
				mAllImagesDict.Add("endstream_button_dis",
						Image.FromFile(Path.Combine(assetsDir, "endstream_button_dis.png")));
			}
		}

		private void ControlBarButtonMouseEnter(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
			}
			if (button.Enabled)
			{
				button.Cursor = Cursors.Hand;
				button.Image = mAllImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ControlBarButtonMouseDown(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
			}
			if (button.Enabled)
			{
				button.Image = mAllImagesDict[(String)button.Tag + "_click"];
			}
		}

		private void ControlBarButtonMouseUp(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
				if (button == null)
				{
					return;
				}
			}
			if (button.Enabled)
			{
				button.Image = mAllImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ControlBarButtonMouseLeave(object sender, System.EventArgs e)
		{
			PictureBox button = sender as PictureBox;
			if (button == null)
			{
				button = (sender as Control).Parent as PictureBox;
			}
			if (button.Enabled)
			{
				button.Cursor = Cursors.Default;
				button.Image = mAllImagesDict[(String)button.Tag];
			}
		}

		private void HandleRightButtonMouseClick(object sender, MouseEventArgs e)
		{
			this.Close();
		}

		private void HandleLeftButtonMouseClick(object sender, MouseEventArgs e)
		{
			mStreamWindow.CloseWindow();
		}

		private void ChangeButtonSize(string text, string buttonPos)
		{
			using (Graphics cg = this.CreateGraphics())
			{
				SizeF size = cg.MeasureString(text, new Font(GameManagerUtilities.GetFont(), sButtonFontSize, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0))));

				if (buttonPos == "left")
				{
                    sLeftButtonWidth = (int)size.Width + 10;
				}
				else
				{
                    sRightButtonWidth = (int)size.Width + 10;
				}
			}
		}
	}

}
