using System;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using System.Runtime.InteropServices;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Threading;

namespace BlueStacks.hyperDroid.GameManager
{
	public partial class SettingsForm : Form
	{
		const int CS_DROPSHADOW = 0x00020000;
		protected override CreateParams CreateParams
		{
			get
			{
				CreateParams cp = base.CreateParams;
				cp.ClassStyle |= CS_DROPSHADOW;
				return cp;

			}
		}

		private TabBar mTabBar;
		private GameManager mGameManager;

		private const int WM_NCHITTEST = 0x84;
		private const int HTCLIENT = 0x1;
		private const int HTCAPTION = 0x2;
		private const int WM_NCLBUTTONDBLCLK = 0x00A3;

		bool visited = false;

		///
		/// Handling the window messages
		///
		protected override void WndProc(ref Message message)
		{
			//Disabling double click on title bar
			if (message.Msg == WM_NCLBUTTONDBLCLK)
			{
				message.Result = IntPtr.Zero;
				return;
			}

			base.WndProc(ref message);

			if (message.Msg == WM_NCHITTEST)
				message.Result = (IntPtr)(HTCAPTION);
		}

		public SettingsForm()
		{
			InitializeComponent();
			this.panel1.BackColor = GMColors.TabBarGradientBottom;
			this.BackColor = GMColors.SettingsFormBackGroundColor;
			this.treeView1.BackColor = this.BackColor;
			this.ForeColor = GMColors.GreyColor;
			this.treeView1.ForeColor = this.ForeColor;
			pictureBox1.Image = mCloseButtonImage;
			pictureBox1.SizeMode = PictureBoxSizeMode.StretchImage;
			this.Paint += new PaintEventHandler(PaintHandler);
			//this.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
			// this.MouseDown += new System.Windows.Forms.MouseEventHandler(this.FormMouseDown);
		}

		static Image mCloseButtonImage = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "tool_close.png"));
		static Image mCloseButtonClickedImage = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "tool_close_click.png"));
		static Image mCloseButtonHoverImage = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "tool_close_hover.png"));

		public void PaintHandler(object sender, PaintEventArgs e)
		{

			//ControlPaint.DrawBorder(e.Graphics, e.ClipRectangle, Color.Red, ButtonBorderStyle.Solid);
		}

		public void SetUp(TabBar tabBar, ContextMenuStrip settingsMenu)
		{
			mTabBar = tabBar;
			mGameManager = tabBar.mGameManager;

			int gmWidth = GameManager.sGameManager.Width;
			int gmHeight = GameManager.sGameManager.Height;
			float fontSize = (float)(GameManager.sGameManager.GetTabBarHeight() * 10) / 20;
			if (fontSize < 11)
			{
				fontSize = 11;
			}
			FontFamily family = GameManager.sFontCollection.Families[0];
			this.Font = new Font(family, fontSize, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));
			treeView1.Font = new Font(family, fontSize+2, FontStyle.Bold, GraphicsUnit.Pixel, ((byte)(0)));
			generalSettings1.Font = this.Font;
            themeSettings1.Font = this.Font;
			int width = Convert.ToInt32(gmWidth * .90);
			int height = Convert.ToInt32(gmHeight * .80);
			this.ClientSize = new Size(width, height);
			this.StartPosition = FormStartPosition.CenterParent;

			SetUpUI();

			themeSettings1.SetUp(tabBar, settingsMenu);
			generalSettings1.SetUp();
		}

		private void SetUpUI()
		{
			this.lblPreferences.Text = String.Format("BlueStacks {0}", GameManager.sLocalizedString.ContainsKey("Preferences") ? GameManager.sLocalizedString["Preferences"] : "Preferences");
			//treeView1.Nodes[0].Text = GameManager.sLocalizedString.ContainsKey("General") ? GameManager.sLocalizedString["General"] : "General";
			//treeView1.Nodes[1].Text = GameManager.sLocalizedString.ContainsKey("Themes") ? GameManager.sLocalizedString["Themes"] : "Themes";
		}

		private void SettingsFrom_Load(object sender, EventArgs e)
		{

		}

		private void treeView1_AfterSelect(object sender, TreeViewEventArgs e)
		{
			if (e.Node.Name == "nThemes")
			{
				Logger.Info("Theme node selected");
				if (!visited)
				{
					themeSettings1.BringToFront();
					//themeSettings1.groupBoxStyle.Select();
					using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
					{
						string parentStyleName = (String)regKey.GetValue("ParentStyleTheme", "Em");
						string themeStyleName = (String)regKey.GetValue("TabStyleTheme", "Default");
						Logger.Info("Current Style name: " + parentStyleName);
						Logger.Info("Current Theme name: " + themeStyleName);
						Stats.SendStyleAndThemeInfoStats(Strings.StyleThemeRender, parentStyleName, themeStyleName, null);
					}
				}
			}
			else
			{
				visited = false;
				if (themeSettings1.themeApplied != null && themeSettings1.cmbTheme != null && themeSettings1.cmbTheme.SelectedItem != null)
				{
					string themeName = themeSettings1.cmbTheme.SelectedItem.ToString();
					if (themeSettings1.themeApplied[themeName] == false)
					{
						ShakeWindow();
						Logger.Info("last visited unsaved theme: " + themeName);
						var confirmResult = GetConfirmMessageResult(themeName);
						Logger.Info("Confirmation result: " + confirmResult);
						if (confirmResult == DialogResult.No)
						{
							visited = true;
							treeView1.SelectedNode = treeNode2;
							themeSettings1.BringToFront();
							themeSettings1.Applybtn.Select();
						}
						else
						{
							themeSettings1.themeApplied[themeName] = true;
							if (e.Node.Name == "nGeneral")
							{
								generalSettings1.BringToFront();
							}
						}
					}
					else
					{
						Logger.Info("Last visited theme saved");
						if (e.Node.Name == "nGeneral")
						{
							generalSettings1.BringToFront();
						}
					}
				}
				else
				{
					if (e.Node.Name == "nGeneral")
					{
						generalSettings1.BringToFront();
					}
				}

			}
		}

		private void PictureBoxControlBarButtonMouseEnter(object sender, EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Cursor = Cursors.Hand;
				button.Image = mCloseButtonHoverImage;
			}

		}

		private void PictureBoxControlBarButtonMouseLeave(object sender, EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Cursor = Cursors.Default;
				button.Image = mCloseButtonImage;
			}
		}

		private void PictureBoxButtonMouseDown(object sender, EventArgs e)
		{
			Logger.Info("Settings form Close button click");
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Image = mCloseButtonClickedImage;
			}
			if (themeSettings1.themeApplied != null && themeSettings1.cmbTheme != null && themeSettings1.cmbTheme.SelectedItem != null)
			{
				string themeName = themeSettings1.cmbTheme.SelectedItem.ToString();
				if (themeSettings1.themeApplied[themeName] == false)
				{

					//switch (pictureBox.Name)
					//{
					//	case "Yes":
					ShakeWindow();
					Logger.Info("Unsaved theme: " + themeName);
					var confirmResult = GetConfirmMessageResult(themeName);
					if (confirmResult == DialogResult.Yes)
					{
						Logger.Info("Closing Settings form");
						this.Close();
					}
					else
					{
						themeSettings1.Applybtn.Select();
					}
				}
				else
				{
					Logger.Info("Closing Settings form");
					this.Close();
				}
			}

		}

		private static System.Windows.Forms.DialogResult GetConfirmMessageResult(string themeName)
		{
			Logger.Info("Getting confirmation message for unsaved theme");
			string confirm = GameManager.sLocalizedString.ContainsKey("Confirm") ? GameManager.sLocalizedString["Confirm"] : "Confirm Action";
			//string tempThemeName = "{DO_NOT_LOCALIZE}";
			string confirmMsgOnThemeChange = "Theme not applied! Do you want to continue without saving the changes?";
			//string confirmMsgOnThemeChange = "Theme not applied! Do you want to continue without setting theme " + tempThemeName + " as current theme?";
			confirmMsgOnThemeChange = GameManager.sLocalizedString.ContainsKey("ConfirmMsgOnThemeChange") ? GameManager.sLocalizedString["ConfirmMsgOnThemeChange"] : confirmMsgOnThemeChange;
			//confirmMsgOnThemeChange = confirmMsgOnThemeChange.Replace(tempThemeName, themeName);
			var confirmResult = MessageBox.Show(confirmMsgOnThemeChange, confirm, MessageBoxButtons.YesNo);
			return confirmResult;
		}

		private void ShakeWindow()
		{
			Logger.Info("Shaking window");
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

				this.Left = this.Left + dx;
				Thread.Sleep(5);
			}
		}

		private void PictureBoxControlBarButtonMouseUp(object sender, MouseEventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (button.Enabled)
			{
				button.Image = mCloseButtonHoverImage;
			}
		}
	}
	public class NativeTreeView : System.Windows.Forms.TreeView
	{
		[DllImport("uxtheme.dll", CharSet = CharSet.Unicode)]
		private extern static int SetWindowTheme(IntPtr hWnd, string pszSubAppName, string pszSubIdList);

		SolidBrush brush = new SolidBrush(GMColors.GreyColor);
		public TreeNode previousSelectedNode = null;
		protected override void CreateHandle()
		{
			base.CreateHandle();
			SetWindowTheme(this.Handle, "explorer", null);
			this.DrawMode = TreeViewDrawMode.OwnerDrawAll;
			this.DrawNode += NativeTreeView_DrawNode;
			this.BeforeCollapse += NativeTreeView_BeforeCollapse;
			this.ExpandAll();
		//	this.HideSelection = false;
		}

		void NativeTreeView_BeforeCollapse(object sender, TreeViewCancelEventArgs e)
		{
			e.Cancel = true;
		}
		
		private void NativeTreeView_DrawNode(object sender, DrawTreeNodeEventArgs e)
		{
			try
			{
				if (e.Node.IsSelected||e.State==TreeNodeStates.Focused)
				{
					//SizeF size = e.Graphics.MeasureString(s, new Font("Arial", 24));
					//int x = (int)(e.Node.Bounds.Location.X * .8);
					//int y = (int)(e.Bounds.Location.Y);
					//int width = (int)(e.Graphics.MeasureString(e.Node.Text, e.Node.TreeView.Font).Width * 1.4);
					//int height = (int)(e.Bounds.Height);
					//e.Graphics.FillRectangle(brush, new Rectangle(x, y, width, height));
					TextRenderer.DrawText(e.Graphics, e.Node.Text, e.Node.TreeView.Font, e.Node.Bounds, GMColors.WhiteColor);
				}
				else
				{
					e.DrawDefault = true;
				}
			}
			catch(Exception ex)
			{
				Logger.Error("Exception at : NativeTreeView_DrawNode " + ex.ToString());
			}
		}
	}
}
