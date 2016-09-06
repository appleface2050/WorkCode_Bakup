using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using Microsoft.Win32;
using System.IO;
using BlueStacks.hyperDroid.Common;
using System.Threading;

namespace BlueStacks.hyperDroid.GameManager
{
	public partial class GeneralSettings : UserControl
	{

		/// <summary>
		/// Code to be commented
		/// </summary>
		static Image sCheckedImagePath = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "checked.png"));
		static Image sUnCheckedImagePath = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "unchecked.png"));

		static string sStartupRegistryPath = @"Software\Microsoft\Windows\CurrentVersion\Run";
		static string sFrameBufferPath = Common.Strings.FrameBufferRegKeyPath;
		static Dictionary<string, string> sLocalizedString = new Dictionary<string, string>();
		bool 	sShouldAutoStart=false;
		public GeneralSettings()
		{
			InitializeComponent();
		}

		private void GeneralSettings_Load(object sender, EventArgs e)
		{
			try
			{
				string resolution = Utils.GetValueFromRegistry(sFrameBufferPath, "WindowWidth", 1280) + " * " + Utils.GetValueFromRegistry(sFrameBufferPath, "WindowHeight", 720);
				cmbBxChangeResolution.SelectedItem = resolution;

				if (cmbBxChangeResolution.SelectedItem == null)
				{
					this.cmbBxChangeResolution.Items.AddRange(new object[] { resolution });
					cmbBxChangeResolution.SelectedItem = resolution;
				}

				string defaultCulture = "en-US";
                string currentCulture = Thread.CurrentThread.CurrentCulture.Name;
                bool isCurrentCultureLocaleFilePresent = false;
                Logger.Info("current culture info for general settings form : " + currentCulture);
                using (RegistryKey baseRegKey = Registry.LocalMachine.CreateSubKey(Strings.RegBasePath))
                {
                    string locale = (string)baseRegKey.GetValue("Locale");
                    if (string.IsNullOrEmpty(locale))
                    {
                        Logger.Info("registry does not exist... creating registry");
                        string dataDir = (string)baseRegKey.GetValue("DataDir", @"C:\ProgramData\BlueStacks");
                        DirectoryInfo localesDir = new DirectoryInfo(Path.Combine(dataDir, "Locales"));

                        foreach (FileInfo files in localesDir.GetFiles())
                        {
                            string fileName = files.Name;
                            if (fileName.Contains(currentCulture))
                            {
                                baseRegKey.SetValue("Locale", currentCulture);
                                isCurrentCultureLocaleFilePresent = true;
                                break;
                            }
                        }
                        if (!isCurrentCultureLocaleFilePresent)
                            baseRegKey.SetValue("Locale", defaultCulture);
                    }
                }
                cmbBxLanguage.SelectedItem = Utils.GetValueFromRegistry(Common.Strings.RegBasePath, "Locale", defaultCulture);

                if (Oem.Instance.IsExitMenuToBeDisplayed)
				{
					pbExitMenu.Image = sCheckedImagePath;
				}
				else
				{
					pbExitMenu.Image = sUnCheckedImagePath;
				}
				if (Oem.Instance.IsSideBarVisible )
				{
					pbSideBar.Image = sCheckedImagePath;
				}
				else
				{
					pbSideBar.Image = sUnCheckedImagePath;
				}
				if (Oem.Instance.IsAppToBeForceKilledOnTabClose )
				{
					pbForceKill.Image = sCheckedImagePath;
				}
				else
				{
					pbForceKill.Image = sUnCheckedImagePath;
				}
				
				if (Oem.Instance.IsGamePadEnabled)
				{
					pbGamePad.Image = sCheckedImagePath;
				}
				else
				{
					pbGamePad.Image = sUnCheckedImagePath;
				}
				if (Oem.Instance.IsSlideUpTabBar)
				{
					pbHidTabBar.Image = sCheckedImagePath;
				}
				else
				{
					pbHidTabBar.Image = sUnCheckedImagePath;
				}

				if (Oem.Instance.IsAndroidToBeStayAwake)
				{
					pbStayAwake.Image = sCheckedImagePath;
				}
				else
				{
					pbStayAwake.Image = sUnCheckedImagePath;
				}

				RegistryKey key = Registry.CurrentUser.OpenSubKey(sStartupRegistryPath);
				if (new List<string>(key.GetValueNames()).Contains("BlueStacks Agent"))
				{
					sShouldAutoStart = true;
					pbAutoStart.Image = sCheckedImagePath;
				}
				else
				{
					sShouldAutoStart = false;
					pbAutoStart.Image = sUnCheckedImagePath;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Cannot update Form registry " + ex.ToString());
			}
		}
        
		private void cmbBxChangeResolutionSelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(cmbBxChangeResolution.Text))
				{
					string selectedItem = cmbBxChangeResolution.Text;
					int width = Convert.ToInt32(selectedItem.Split(new char[] { '*' })[0].Trim());
					int height = Convert.ToInt32(selectedItem.Split(new char[] { '*' })[1].Trim());
					Utils.UpdateRegistry(sFrameBufferPath, "WindowWidth", width, RegistryValueKind.DWord);
					Utils.UpdateRegistry(sFrameBufferPath, "WindowHeight", height, RegistryValueKind.DWord);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in cmbBxChangeResolutionSelectedIndexChanged " + ex.ToString());
			}
		}

		private void cmbBxLanguageSelectedIndexChanged(object sender, EventArgs e)
		{
			try
			{
				if (!string.IsNullOrEmpty(cmbBxLanguage.Text))
				{
					string selectedItem = cmbBxLanguage.Text;
					Utils.UpdateRegistry(Common.Strings.RegBasePath, "Locale", selectedItem, RegistryValueKind.String);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in cmbBxLanguageSelectedIndexChanged " + ex.ToString());
			}
		}

		private void SideBarPictureBoxClicked(object sender, EventArgs e)
		{
			try
			{

				if (Oem.Instance.IsSideBarVisible)
				{
					pbSideBar.Image = sUnCheckedImagePath;
				}
				else
				{
					pbSideBar.Image = sCheckedImagePath;
				}
				Oem.Instance.IsSideBarVisible = !Oem.Instance.IsSideBarVisible;
				Oem.Instance.Save();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in SideBarPictureBoxClicked " + ex.ToString());
			}
		}

		private void StayAwakePictureBoxClicked(object sender, EventArgs e)
		{
			try
			{
				if (Oem.Instance.IsAndroidToBeStayAwake)
				{
					pbStayAwake.Image = sUnCheckedImagePath;
				}
				else
				{
					pbStayAwake.Image = sCheckedImagePath;
				}
				Oem.Instance.IsAndroidToBeStayAwake = !Oem.Instance.IsAndroidToBeStayAwake;
				Oem.Instance.Save();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in StayAwakePictureBoxClicked " + ex.ToString());
			}
		}

		private void AutoStartPictureBoxClicked(object sender, EventArgs e)
		{
			try
			{
				if (sShouldAutoStart)
				{
					sShouldAutoStart = false;
					RegistryKey key = Registry.CurrentUser.OpenSubKey(sStartupRegistryPath, true);
					key.DeleteValue("BlueStacks Agent");
					key.Close();
					key.Flush();
					pbAutoStart.Image = sUnCheckedImagePath;
				}
				else
				{
					sShouldAutoStart = true;
					string installDir = Utils.GetValueFromRegistry(Common.Strings.RegBasePath, "InstallDir", "");
					string agentPath = Path.Combine(installDir, "HD-Agent.exe");
					RegistryKey key = Registry.CurrentUser.OpenSubKey(sStartupRegistryPath, true);
					key.SetValue("BlueStacks Agent", agentPath, RegistryValueKind.String);
					key.Close();
					key.Flush();
					pbAutoStart.Image = sCheckedImagePath;
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in AutoStartPictureBoxClicked " + ex.ToString());
			}
		}

		private void ExitMenuPictureBoxClicked(object sender, EventArgs e)
		{
			try
			{
				if (Oem.Instance.IsExitMenuToBeDisplayed)
				{
					pbExitMenu.Image = sUnCheckedImagePath;
				}
				else
				{
					pbExitMenu.Image = sCheckedImagePath;
				}
				Oem.Instance.IsExitMenuToBeDisplayed = !Oem.Instance.IsExitMenuToBeDisplayed;
				Oem.Instance.Save();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in ExitMenuPictureBoxClicked " + ex.ToString());
			}
		}

		private void ForceKillPictureBoxClicked(object sender, EventArgs e)
		{
			try
			{
				if (Oem.Instance.IsAppToBeForceKilledOnTabClose)
				{
					pbForceKill.Image = sUnCheckedImagePath;
				}
				else
				{
					pbForceKill.Image = sCheckedImagePath;
				}
				Oem.Instance.IsAppToBeForceKilledOnTabClose = !Oem.Instance.IsAppToBeForceKilledOnTabClose;
				Oem.Instance.Save();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in ForceKillPictureBoxClicked " + ex.ToString());
			}
		}

		private void GamePadPictureBoxClicked(object sender, EventArgs e)
		{
			try
			{
				if (Oem.Instance.IsGamePadEnabled)
				{
					pbGamePad.Image = sUnCheckedImagePath;
				}
				else
				{
					pbGamePad.Image = sCheckedImagePath;
				}
				Oem.Instance.IsGamePadEnabled = !Oem.Instance.IsGamePadEnabled;
				Oem.Instance.Save();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in  GamePadPictureBoxClicked" + ex.ToString());
			}
		}


		private void HideTabBarPictureBoxClicked(object sender, EventArgs e)
		{
			try
			{
				if (Oem.Instance.IsSlideUpTabBar)
				{
					pbHidTabBar.Image = sUnCheckedImagePath;
				}
				else
				{
					pbHidTabBar.Image = sCheckedImagePath;
				}
				Oem.Instance.IsSlideUpTabBar = !Oem.Instance.IsSlideUpTabBar;
				Oem.Instance.Save();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in  HideTabBarPictureBoxClicked" + ex.ToString());
			}
		}

		internal void SetUp()
		{
			lblAutoStart.Font = this.Font;
			lblAutoStart.Text = GameManager.sLocalizedString.ContainsKey("'AutoStart") ? GameManager.sLocalizedString["LabelAutoStart"] : lblAutoStart.Text;

			lblForceKill.Font = this.Font;
			lblForceKill.Text = GameManager.sLocalizedString.ContainsKey("LabelForceKill") ? GameManager.sLocalizedString["LabelForceKill"] : lblForceKill.Text;

			lblNote.Font = this.Font;
			lblNote.Text = "*" + (GameManager.sLocalizedString.ContainsKey("LabelRestartNote") ? GameManager.sLocalizedString["LabelRestartNote"] : lblNote.Text);

			lblShowExitMenu.Font = this.Font;
			lblShowExitMenu.Text = GameManager.sLocalizedString.ContainsKey("LabelShowExitMenu") ? GameManager.sLocalizedString["LabelShowExitMenu"] : lblShowExitMenu.Text;

			lblShowSideBar.Font = this.Font;
			lblShowSideBar.Text = "*" + (GameManager.sLocalizedString.ContainsKey("LabelSideBar") ? GameManager.sLocalizedString["LabelSideBar"] : lblShowSideBar.Text);

			lblStayAwake.Font = this.Font;
			lblStayAwake.Text = GameManager.sLocalizedString.ContainsKey("LabelStayAwake") ? GameManager.sLocalizedString["LabelStayAwake"] : lblStayAwake.Text;

			lblConfigureGamePad.Font = this.Font;
			lblConfigureGamePad.Text = "*" + (GameManager.sLocalizedString.ContainsKey("LableConfigureGamePad") ? GameManager.sLocalizedString["LableConfigureGamePad"] : lblConfigureGamePad.Text);

			lblChangeResolution.Font = this.Font;
			lblChangeResolution.Text = "*" + (GameManager.sLocalizedString.ContainsKey("LabelWindowSize") ? GameManager.sLocalizedString["LabelWindowSize"] : lblChangeResolution.Text);

			lblLanguage.Font = this.Font;
			lblLanguage.Text = "*" + (GameManager.sLocalizedString.ContainsKey("LabelLanguage") ? GameManager.sLocalizedString["LabelLanguage"] : lblLanguage.Text);

			lblHideTopBar.Font = this.Font;
			lblHideTopBar.Text = "*" + (GameManager.sLocalizedString.ContainsKey("LabelHideTopBar") ? GameManager.sLocalizedString["LabelHideTopBar"] : lblHideTopBar.Text);

			grpbxBluestacksPreferences.ForeColor = GMColors.GreyColor;
			grpbxBluestacksPreferences.Font = this.Font;
			grpbxBluestacksPreferences.Text = GameManager.sLocalizedString.ContainsKey("GroupBoxBluestacksPreferences") ? GameManager.sLocalizedString["GroupBoxBluestacksPreferences"] : grpbxBluestacksPreferences.Text;

			cmbBxChangeResolution.ForeColor = GMColors.SelectedTabTextColor;
			cmbBxChangeResolution.BackColor = GMColors.TabBarGradientBottom;
			cmbBxChangeResolution.Font = this.Font;
            cmbBxChangeResolution.DrawItem += cmbBxChangeResolution_DrawItem;
            cmbBxChangeResolution.DrawMode = DrawMode.OwnerDrawFixed;

			cmbBxLanguage.ForeColor = GMColors.SelectedTabTextColor;
			cmbBxLanguage.BackColor = GMColors.TabBarGradientBottom;
			cmbBxLanguage.Font = this.Font;
			cmbBxLanguage.DrawItem += cmbBxChangeResolution_DrawItem;
			cmbBxLanguage.DrawMode = DrawMode.OwnerDrawVariable;
			
		}

		void cmbBxChangeResolution_DrawItem(object sender, DrawItemEventArgs e)
		{

			ComboBox comb = (ComboBox)(sender);
			//SizeF size = e.Graphics.MeasureString(s, new Font("Arial", 24));
			int x = (int)(e.Graphics.ClipBounds.Location.X * .8);
			int y = (int)(e.Bounds.Location.Y);
			int width = (int)(e.Graphics.MeasureString(comb.Items[e.Index].ToString(), comb.Font).Width * 1.4);
			int height = (int)(e.Bounds.Height);
			TextFormatFlags flags = TextFormatFlags.Left | TextFormatFlags.VerticalCenter | TextFormatFlags.WordBreak;
			if (e.State == (DrawItemState.Focus | DrawItemState.NoAccelerator | DrawItemState.NoFocusRect | DrawItemState.Selected | DrawItemState.ComboBoxEdit)
				|| e.State == (DrawItemState.Focus | DrawItemState.NoAccelerator | DrawItemState.NoFocusRect | DrawItemState.Selected)
				|| e.State == (DrawItemState.ComboBoxEdit | DrawItemState.NoAccelerator | DrawItemState.NoFocusRect)
				|| e.State == (DrawItemState.Selected | DrawItemState.NoAccelerator | DrawItemState.NoFocusRect))
			{
				TextRenderer.DrawText(e.Graphics, comb.Items[e.Index].ToString(), comb.Font, e.Bounds, GMColors.WhiteColor, flags);
			}
			else
			{
				TextRenderer.DrawText(e.Graphics, comb.Items[e.Index].ToString(), comb.Font, e.Bounds, GMColors.GreyColor, flags);
			}
		}

        private void tableLayoutPanel1_Paint(object sender, PaintEventArgs e)
        {

        }

        class AdvancedComboBox : ComboBox
        {
            new public System.Windows.Forms.DrawMode DrawMode { get; set; }
            public Color HighlightColor { get; set; }

            public AdvancedComboBox()
            {
                base.DrawMode = System.Windows.Forms.DrawMode.OwnerDrawFixed;
                this.HighlightColor = Color.Gray;
                this.DrawItem += new DrawItemEventHandler(AdvancedComboBox_DrawItem);
            }

            void AdvancedComboBox_DrawItem(object sender, DrawItemEventArgs e)
            {
                if (e.Index < 0)
                    return;

                ComboBox combo = sender as ComboBox;
                if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
                    e.Graphics.FillRectangle(new SolidBrush(HighlightColor),
                                             e.Bounds);
                else
                    e.Graphics.FillRectangle(new SolidBrush(combo.BackColor),
                                             e.Bounds);

                e.Graphics.DrawString(combo.Items[e.Index].ToString(), e.Font,
                                      new SolidBrush(combo.ForeColor),
                                      new Point(e.Bounds.X, e.Bounds.Y));

                e.DrawFocusRectangle();
            }
        }
    }
}
