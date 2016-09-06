using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Common
{
	public class CustomPopupMessageBox : Form
	{

		[DllImport("user32.dll", CharSet = CharSet.Auto)]
		private static extern bool MessageBeep(uint type);

		[DllImport("Shell32.dll")]
		public extern static int ExtractIconEx(string libName, int iconIndex, IntPtr[] largeIcon, IntPtr[] smallIcon, int nIcons);

		static private IntPtr[] largeIcon;
		static private IntPtr[] smallIcon;

		static public CustomPopupMessageBox newMessageBox;
		static private Label frmTitle;
		static private Label frmMessage;
		static private PictureBox pIcon;
		static private FlowLayoutPanel flpButtons;
		static private Icon frmIcon;

		static private Button btnOK;
		static private Button btnAbort;
		static private Button btnRetry;
		static private Button btnIgnore;
		static private Button btnCancel;
		static private Button btnYes;
		static private Button btnNo;
		static private Button btnLearnMore;

		static private DialogResult ReturnButton;

		public enum Icons
		{
			Error,
			Explorer,
			Find,
			Information,
			Mail,
			Media,
			Print,
			Question,
			RecycleBinEmpty,
			RecycleBinFull,
			Stop,
			User,
			Warning
		}

		public enum Buttons
		{
			AbortRetryIgnore,
			OK,
			OKCancel,
			RetryCancel,
			YesNo,
			YesNoCancel,
			LearnMoreOk
		}

		static private void BuildMessageBox(string title, Size size)
		{
			newMessageBox = new CustomPopupMessageBox();
			newMessageBox.Text = title;
			newMessageBox.Size = size;//new System.Drawing.Size(500, 150);
			newMessageBox.StartPosition = System.Windows.Forms.FormStartPosition.CenterScreen;
			newMessageBox.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			newMessageBox.Paint += new PaintEventHandler(newMessageBox_Paint);
			newMessageBox.BackColor = System.Drawing.Color.White;

			TableLayoutPanel tlp = new TableLayoutPanel();
			tlp.RowCount = 3;
			tlp.ColumnCount = 0;
			tlp.Dock = System.Windows.Forms.DockStyle.Fill;
			tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, (float)(size.Height * .16)));
			tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Percent, (float)(size.Height * .54)));
			tlp.RowStyles.Add(new System.Windows.Forms.RowStyle(System.Windows.Forms.SizeType.Absolute, (float)(size.Height * .3)));
			tlp.BackColor = System.Drawing.Color.Transparent;
			tlp.Padding = new Padding(2, 5, 2, 2);

			frmTitle = new Label();
			frmTitle.Dock = System.Windows.Forms.DockStyle.Fill;
			frmTitle.BackColor = System.Drawing.Color.Transparent;
			frmTitle.ForeColor = System.Drawing.Color.Black;
			frmTitle.Font = new Font("Tahoma", 9, FontStyle.Bold);

			frmMessage = new Label();
			frmMessage.Dock = System.Windows.Forms.DockStyle.Fill;
			frmMessage.BackColor = System.Drawing.Color.White;
			frmMessage.Font = new Font("Tahoma", 9, FontStyle.Regular);
			frmMessage.Text = "";

			largeIcon = new IntPtr[250];
			smallIcon = new IntPtr[250];
			pIcon = new PictureBox();
			ExtractIconEx("shell32.dll", 0, largeIcon, smallIcon, 250);

			flpButtons = new FlowLayoutPanel();
			flpButtons.FlowDirection = System.Windows.Forms.FlowDirection.RightToLeft;
			flpButtons.Padding = new Padding(0, 5, 5, 0);
			flpButtons.Dock = System.Windows.Forms.DockStyle.Fill;
			flpButtons.BackColor = System.Drawing.Color.FromArgb(240, 240, 240);

			TableLayoutPanel tlpMessagePanel = new TableLayoutPanel();
			tlpMessagePanel.BackColor = System.Drawing.Color.White;
			tlpMessagePanel.Dock = System.Windows.Forms.DockStyle.Fill;
			tlpMessagePanel.ColumnCount = 2;
			tlpMessagePanel.RowCount = 0;
			tlpMessagePanel.Padding = new Padding(4, 5, 4, 4);
			tlpMessagePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Absolute, 60));
			tlpMessagePanel.ColumnStyles.Add(new System.Windows.Forms.ColumnStyle(System.Windows.Forms.SizeType.Percent, 100F));
			tlpMessagePanel.Controls.Add(pIcon);
			tlpMessagePanel.Controls.Add(frmMessage);

			tlp.Controls.Add(frmTitle);
			tlp.Controls.Add(tlpMessagePanel);
			tlp.Controls.Add(flpButtons);
			newMessageBox.Controls.Add(tlp);
		}

		/// <summary>
		/// Message: Text to display in the message box.
		/// </summary>
		static public DialogResult Show(string Message, Size size)
		{
			BuildMessageBox("", size);
			frmMessage.Text = Message;
			ShowOKButton();
			newMessageBox.ShowDialog();
			return ReturnButton;
		}

		/// <summary>
		/// Title: Text to display in the title bar of the messagebox.
		/// </summary>
		static public DialogResult Show(string Message, string Title, Size size)
		{
			BuildMessageBox(Title, size);
			frmTitle.Text = Title;
			frmMessage.Text = Message;
			ShowOKButton();
			newMessageBox.ShowDialog();
			return ReturnButton;
		}

		/// <summary>
		/// MButtons: Display Buttons on the message box.
		/// </summary>
		static public DialogResult Show(string Message, string Title, Buttons MButtons, Size size)
		{
			BuildMessageBox(Title, size); // BuildMessageBox method, responsible for creating the MessageBox
			frmTitle.Text = Title; // Set the title of the MessageBox
			frmMessage.Text = Message; //Set the text of the MessageBox
			ButtonStatements(MButtons); // ButtonStatements method is responsible for showing the appropreiate buttons
			newMessageBox.ShowDialog(); // Show the MessageBox as a Dialog.
			return ReturnButton; // Return the button click as an Enumerator
		}

		/// <summary>
		/// MIcon: Display Icon on the message box.
		/// </summary>
		static public DialogResult Show(string Message, string Title, Buttons MButtons, Icons MIcon, Size size)
		{
			BuildMessageBox(Title, size);
			frmTitle.Text = Title;
			frmMessage.Text = Message;
			ButtonStatements(MButtons);
			IconStatements(MIcon);
			Image imageIcon = new Bitmap(frmIcon.ToBitmap(), 50, 50);
			pIcon.Image = imageIcon;
			newMessageBox.ShowDialog();
			return ReturnButton;
		}

		static void btnOK_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.OK;
			newMessageBox.Dispose();
		}

		static void btnAbort_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.Abort;
			newMessageBox.Dispose();
		}

		static void btnRetry_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.Retry;
			newMessageBox.Dispose();
		}

		static void btnIgnore_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.Ignore;
			newMessageBox.Dispose();
		}

		static void btnCancel_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.Cancel;
			newMessageBox.Dispose();
		}

		static void btnYes_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.Yes;
			newMessageBox.Dispose();
		}

		static void btnLearnMore_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.Abort;
			newMessageBox.Dispose();
		}

		static void btnNo_Click(object sender, EventArgs e)
		{
			ReturnButton = DialogResult.No;
			newMessageBox.Dispose();
		}

		static private void ShowOKButton()
		{
			btnOK = new Button();
			btnOK.Text = "OK";
			btnOK.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnOK.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnOK.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnOK.Click += new EventHandler(btnOK_Click);
			flpButtons.Controls.Add(btnOK);
		}

		static private void ShowAbortButton()
		{
			btnAbort = new Button();
			btnAbort.Text = "Abort";
			btnAbort.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnAbort.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnAbort.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnAbort.Click += new EventHandler(btnAbort_Click);
			flpButtons.Controls.Add(btnAbort);
		}

		static private void ShowRetryButton()
		{
			btnRetry = new Button();
			btnRetry.Text = "Retry";
			btnRetry.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnRetry.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnRetry.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnRetry.Click += new EventHandler(btnRetry_Click);
			flpButtons.Controls.Add(btnRetry);
		}

		static private void ShowIgnoreButton()
		{
			btnIgnore = new Button();
			btnIgnore.Text = "Ignore";
			btnIgnore.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnIgnore.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnIgnore.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnIgnore.Click += new EventHandler(btnIgnore_Click);
			flpButtons.Controls.Add(btnIgnore);
		}

		static private void ShowCancelButton()
		{
			btnCancel = new Button();
			btnCancel.Text = "Cancel";
			btnCancel.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnCancel.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnCancel.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnCancel.Click += new EventHandler(btnCancel_Click);
			flpButtons.Controls.Add(btnCancel);
		}

		static private void ShowYesButton()
		{
			btnYes = new Button();
			btnYes.Text = "Yes";
			btnYes.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnYes.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnYes.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnYes.Click += new EventHandler(btnYes_Click);
			flpButtons.Controls.Add(btnYes);
		}

		static private void ShowLearnMoreButton()
		{
			btnLearnMore = new Button();
			btnLearnMore.Text = "Learn More";
			btnLearnMore.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnLearnMore.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnLearnMore.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnLearnMore.Click += new EventHandler(btnLearnMore_Click);
			flpButtons.Controls.Add(btnLearnMore);
		}

		static private void ShowNoButton()
		{
			btnNo = new Button();
			btnNo.Text = "No";
			btnNo.Size = new System.Drawing.Size(Convert.ToInt32(newMessageBox.Width * .18), Convert.ToInt32(newMessageBox.Height * .16));
			btnNo.BackColor = System.Drawing.Color.FromArgb(255, 255, 255);
			btnNo.Font = new Font("Tahoma", 8, FontStyle.Regular);
			btnNo.Click += new EventHandler(btnNo_Click);
			flpButtons.Controls.Add(btnNo);
		}

		static private void ButtonStatements(Buttons MButtons)
		{
			if (MButtons == Buttons.AbortRetryIgnore)
			{
				ShowIgnoreButton();
				ShowRetryButton();
				ShowAbortButton();
			}

			if (MButtons == Buttons.OK)
			{
				ShowOKButton();
			}

			if (MButtons == Buttons.OKCancel)
			{
				ShowCancelButton();
				ShowOKButton();
			}

			if (MButtons == Buttons.RetryCancel)
			{
				ShowCancelButton();
				ShowRetryButton();
			}

			if (MButtons == Buttons.YesNo)
			{
				ShowNoButton();
				ShowYesButton();
			}

			if (MButtons == Buttons.YesNoCancel)
			{
				ShowCancelButton();
				ShowNoButton();
				ShowYesButton();
			}

			if (MButtons == Buttons.LearnMoreOk)
			{
				ShowOKButton();
				ShowLearnMoreButton();
			}
		}

		static private void IconStatements(Icons MIcon)
		{
			if (MIcon == Icons.Error)
			{
				MessageBeep(30);
				frmIcon = Icon.FromHandle(largeIcon[109]);
			}

			if (MIcon == Icons.Explorer)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[220]);
			}

			if (MIcon == Icons.Find)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[22]);
			}

			if (MIcon == Icons.Information)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[221]);
			}

			if (MIcon == Icons.Mail)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[156]);
			}

			if (MIcon == Icons.Media)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[116]);
			}

			if (MIcon == Icons.Print)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[136]);
			}

			if (MIcon == Icons.Question)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[23]);
			}

			if (MIcon == Icons.RecycleBinEmpty)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[31]);
			}

			if (MIcon == Icons.RecycleBinFull)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[32]);
			}

			if (MIcon == Icons.Stop)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[27]);
			}

			if (MIcon == Icons.User)
			{
				MessageBeep(0);
				frmIcon = Icon.FromHandle(largeIcon[170]);
			}

			if (MIcon == Icons.Warning)
			{
				MessageBeep(30);
				frmIcon = Icon.FromHandle(largeIcon[217]);
			}
		}

		static void newMessageBox_Paint(object sender, PaintEventArgs e)
		{
			Graphics g = e.Graphics;
			Rectangle frmTitleL = new Rectangle(0, 0, (newMessageBox.Width), Convert.ToInt32(newMessageBox.Height * .16));
			Rectangle frmTitleR = new Rectangle((newMessageBox.Width / 2), 0, (newMessageBox.Width / 2), Convert.ToInt32(newMessageBox.Height * .16));
			Rectangle frmMessageBox = new Rectangle(0, 0, (newMessageBox.Width - 1), (newMessageBox.Height - 1));
			LinearGradientBrush frmLGBL = new LinearGradientBrush(frmTitleL, Color.FromArgb(87, 148, 160), Color.FromArgb(209, 230, 243), LinearGradientMode.Horizontal);
			//LinearGradientBrush frmLGBR = new LinearGradientBrush(frmTitleR, Color.FromArgb(209, 230, 243), Color.FromArgb(87, 148, 160), LinearGradientMode.Horizontal);
			Pen frmPen = new Pen(Color.FromArgb(63, 119, 143), 1);
			g.FillRectangle(frmLGBL, frmTitleL);
			//g.FillRectangle(frmLGBR, frmTitleR);
			g.DrawRectangle(frmPen, frmMessageBox);
		}
	}
}
