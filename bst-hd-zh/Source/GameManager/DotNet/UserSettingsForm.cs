using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.IO;
using BlueStacks.hyperDroid.GameManager.gamemanager;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.GameManager
{
    public partial class UserSettingsForm : Form
    {
        Image mCloseButtonImage = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "tool_close.png"));
        Image mCloseButtonClickedImage = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "tool_close_click.png"));
        Image mCloseButtonHoverImage = Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "tool_close_hover.png"));

        GameManager gameManager;
        UserSettingsData mSettingsData = new UserSettingsData();
        UserSettingsData mLastSettingData = new UserSettingsData();

        public UserSettingsForm(GameManager gm)
        {
            InitializeComponent();
            InitData(gm);
            SetStrings();

            this.Owner = gm;
            pbClose.Image = mCloseButtonImage;
            pbClose.SizeMode = PictureBoxSizeMode.StretchImage;
            btnOK.Enabled = false;
        }

        private void SetStrings()
        {
            gbResolution.Text = Locale.Strings.GroupResolutionTitle;
            gbBossKey.Text = Locale.Strings.GroupBossKeyTitle;
            rbLMode960.Text = Locale.Strings.LandscapeMode960;
            rbLMode1280.Text = Locale.Strings.LandscapeMode1280;
            rbLMode1440.Text = Locale.Strings.LandscapeMode1440;
            rbPMode720.Text = Locale.Strings.PortraitMode720;
            rbPMode960.Text = Locale.Strings.PortraitMode960;
            rbPMode900.Text = Locale.Strings.PortraitMode900;

            rbCustomize.Text = Locale.Strings.CustomizeResolutionSetting;
            lblCustomizeResolutionPrompt.Text = Locale.Strings.CustomizeResolutionSettingPrompt;
            lblBossKeyName.Text = Locale.Strings.BossKeyLableName;
            lblInputPrompt.Text = Locale.Strings.BossKeyInputPrompt;

            btnOK.Text = Locale.Strings.OKButtonText;
            btnCancel.Text = Locale.Strings.CancelButtonText;

            System.Globalization.CultureInfo ci = System.Threading.Thread.CurrentThread.CurrentCulture;
            string lang = ci.Name;

            RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
            if (key != null)
            {
                lang = (string)key.GetValue("Localization", lang).ToString();
            }
            if (lang == "zh-CN")
            {
                lblQQ.Visible = true;
                lblQQGroup.Visible = true;

                lblQQ.Text = Locale.Strings.CustomerServiceQQ;
                lblQQGroup.Text = Locale.Strings.CustomerServiceQQGroup;
            }
            else
            {
                btnOK.Location = new Point(btnOK.Location.X, btnOK.Location.Y - 15);
                btnCancel.Location = new Point(btnCancel.Location.X, btnCancel.Location.Y - 15);
            }
        }

        private void InitData(GameManager gm)
        {
            gameManager = gm;
            mSettingsData.Init();
            Size srcSize = mSettingsData.GuestSize;
            tbBossKey.Text = mSettingsData.keyString;

            string size = string.Format("{0}*{1}", srcSize.Width, srcSize.Height);
            switch (size)
            {
                case "960*720":
                    rbLMode960.Checked = true;
                    break;
                case "1280*960":
                    rbLMode1280.Checked = true;
                    break;
                case "1440*900":
                    rbLMode1440.Checked = true;
                    break;
                case "720*960":
                    rbPMode720.Checked = true;
                    break;
                case "960*1280":
                    rbPMode960.Checked = true;
                    break;
                case "900*1440":
                    rbPMode900.Checked = true;
                    break;
                default:
                    rbCustomize.Checked = true;
                    tbHeight.Text = srcSize.Height.ToString();
                    tbWide.Text = srcSize.Width.ToString();
                    break;
            }

            mLastSettingData.GuestSize = mSettingsData.GuestSize;
            mLastSettingData.keyString = mSettingsData.keyString;

            if (rbCustomize.Checked)
            {
                mLastSettingData.GuestSize = srcSize;
                mSettingsData.GuestSize = srcSize;
            }
        }
        private void pbClose_Click(object sender, EventArgs e)
        {
            pbClose.Image = mCloseButtonClickedImage;
            this.Close();
        }

        private void pbClose_MouseHover(object sender, EventArgs e)
        {
            pbClose.Image = mCloseButtonHoverImage;
        }

        private void pbClose_MouseLeave(object sender, EventArgs e)
        {
            pbClose.Image = mCloseButtonImage;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        public Size GetGMSizeGivenFESize(int feWidth, int feHeight)
        {
            return new Size(feWidth + 2 * GameManager.mBorderWidth + 2 * GameManager.mContentBorderWidth,
                    feHeight + 2 * GameManager.mBorderWidth + GameManager.sTabBarHeight + GameManager.mCenterBorderHeight + 
                    2 * GameManager.mContentBorderWidth + GameManager.mTabBarExtraHeight);
        }

        private void btnOK_Click(object sender, EventArgs e)
        {
            string str = this.tbBossKey.Text.TrimEnd();
            int len = str.Length;
            if (len >= 1 && str.Substring(str.Length - 1) == "+")
            {
                this.tbBossKey.Text = "";
                PromptForm confirmFrm = new PromptForm(Locale.Strings.PromptSetBosskey);
                confirmFrm.ShowDialog(gameManager);
                return;
            }

            try
            {
                if (rbCustomize.Checked)
                {
                    if (string.IsNullOrEmpty(tbWide.Text) || string.IsNullOrEmpty(tbHeight.Text))
                    {
                        PromptForm form = new PromptForm(Locale.Strings.CustomizeResolutionNotNULL);
                        form.ShowDialog(gameManager);
                        return;
                    }

                    int w;
                    if (!int.TryParse(tbWide.Text, out w))
                    {
                        tbWide.Text = "";
                        tbWide.Focus();
                        return;
                    }

                    int h;
                    if (!int.TryParse(this.tbHeight.Text, out h))
                    {
                        tbHeight.Text = "";
                        tbHeight.Focus();
                        return;
                    }

                    Size temp = UserSettingsData.CountSize(new Size(w,h), 2 * GameManager.mBorderWidth + 2 * GameManager.mContentBorderWidth, 2 * GameManager.mBorderWidth + GameManager.sTabBarHeight + GameManager.mCenterBorderHeight +
                                    2 * GameManager.mContentBorderWidth + GameManager.mTabBarExtraHeight);

                    double s = (w > h) ? (double)h * 1.0 / w : (double)w * 1.0 / h;
                    if (s < 0.4)
                    {
                        PromptForm form = new PromptForm(Locale.Strings.WidthOrHightNotRight);
                        form.ShowDialog(gameManager);
                        tbWide.Focus();
                        return;
                    }

                    if (temp.Width < 330)
                    {
                        PromptForm form = new PromptForm(Locale.Strings.WidthOrHightNotRight);
                        form.ShowDialog(gameManager);
                        tbWide.Focus();
                        return;
                    }

                    if (mSettingsData.GuestSize != mLastSettingData.GuestSize)
                    {
                        //check scale 
                        double defaultSacle = 1280.0 / 720;
                        double currentSacle = 1.0 * w / h;
                        if (defaultSacle != currentSacle)
                        {
                            PromptForm confirmFrm = new PromptForm(Locale.Strings.ResoutionCauseDeformation,
                                Locale.Strings.OKButtonText, Locale.Strings.CancelButtonText);
                            DialogResult dr = confirmFrm.ShowDialog(gameManager);
                            if (dr != DialogResult.OK)
                            {
                                return;
                            }
                        }
                    }

                    mSettingsData.GuestSize = new Size(w, h);
                }

                mSettingsData.GMSize = UserSettingsData.CountSize(mSettingsData.GuestSize, 2 * GameManager.mBorderWidth + 2 * GameManager.mContentBorderWidth, 2 * GameManager.mBorderWidth + GameManager.sTabBarHeight + GameManager.mCenterBorderHeight +
                    2 * GameManager.mContentBorderWidth + GameManager.mTabBarExtraHeight);


                if (string.IsNullOrEmpty(str))
                {
                    mSettingsData.keyString = "";
                    mSettingsData.keyValue = 0;
                }

                mSettingsData.SaveToReg();

                if (!string.IsNullOrEmpty(str))
                {
                    gameManager.mSystemHotKey.UpdateBossHotKey(gameManager.Handle);
                }

                if (mSettingsData.GuestSize != mLastSettingData.GuestSize)
	            {
                    PromptForm rebootFrm = new PromptForm(Locale.Strings.RebootAfterChangeResolution,
                             Locale.Strings.RestartNowBtn, Locale.Strings.RestartLaterBtn);
                    DialogResult dr = rebootFrm.ShowDialog(gameManager);
                    if (dr == DialogResult.OK)
                    {
                        this.gameManager.Restart();
                        return;
                    }
	            }

                this.Close();
            }
            catch (Exception)
            {
            }
        }

        private void SetRadioBtnFareColor(object sender)
        {
            RadioButton btn = (RadioButton)sender;
            if (btn.Checked)
            {
                btn.ForeColor = SystemColors.Control;
            }
            else
            {
                btn.ForeColor = SystemColors.ControlDark;
            }
        }
        private void CheckChangesAndEnableBtn()
        {
            if (mSettingsData.GuestSize != mLastSettingData.GuestSize ||
                mSettingsData.keyString != mLastSettingData.keyString)
            {
                btnOK.Enabled = true;
            }
            else
            {
                btnOK.Enabled = false;
            }
        }

        private void rbPMode960_CheckedChanged(object sender, EventArgs e)
        {
            SetRadioBtnFareColor(sender);
            mSettingsData.GuestSize = new Size(960,720);
            CheckChangesAndEnableBtn();
        }

        private void rbPMode1280_CheckedChanged(object sender, EventArgs e)
        {
            SetRadioBtnFareColor(sender);
            mSettingsData.GuestSize = new Size(1280, 960);
            CheckChangesAndEnableBtn();
        }

        private void rbPMode1440_CheckedChanged(object sender, EventArgs e)
        {
            SetRadioBtnFareColor(sender);
            mSettingsData.GuestSize = new Size(1440, 900);
            CheckChangesAndEnableBtn();
        }

        private void rbCustomize_CheckedChanged(object sender, EventArgs e)
        {
            RadioButton btn = (RadioButton)sender;
            if (btn.Checked)
            {
                btnOK.Enabled = true;
                btn.ForeColor = SystemColors.Control;
                tbHeight.Enabled = true;
                tbWide.Enabled = true;
                tbWide.Focus();

                int w = 0;
                int.TryParse(tbWide.Text, out w);
                int h = 0;
                int.TryParse(this.tbHeight.Text, out h);
                mSettingsData.GuestSize = new Size(w, h);
                CheckChangesAndEnableBtn();
            }
            else
            {
                btn.ForeColor = SystemColors.ControlDark;
                tbWide.Enabled = false;
                tbHeight.Enabled = false;
            }
        }

        private void rbLMode720_CheckedChanged(object sender, EventArgs e)
        {
            SetRadioBtnFareColor(sender);
            mSettingsData.GuestSize = new Size(720, 960);
            CheckChangesAndEnableBtn();
        }

        private void rbLMode960_CheckedChanged(object sender, EventArgs e)
        {
            SetRadioBtnFareColor(sender);
            mSettingsData.GuestSize = new Size(960, 1280);
            CheckChangesAndEnableBtn();
        }

        private void rbLMode900_CheckedChanged(object sender, EventArgs e)
        {
            SetRadioBtnFareColor(sender);
            mSettingsData.GuestSize = new Size(900, 1440);
            CheckChangesAndEnableBtn();
        }

        private void tbBossKey_KeyDown(object sender, KeyEventArgs e)
        {
            int keyCode = 0;
            StringBuilder keyValue = new StringBuilder();
            keyValue.Length = 0;
            keyValue.Append("");
            if (e.Modifiers != 0)
            {
                if (e.Control)
                {
                    keyValue.Append("Ctrl + ");
                    keyCode += (int)SystemHotKey.KeyFlags.Ctrl;
                }
                if (e.Alt)
                {
                    keyValue.Append("Alt + ");
                    keyCode += (int)SystemHotKey.KeyFlags.Alt;
                }
                if (e.Shift)
                {
                    keyValue.Append("Shift + ");
                    keyCode += (int)SystemHotKey.KeyFlags.Shift;
                }
            }
            else
            {
                return;
            }

            if ((e.KeyValue >= 33 && e.KeyValue <= 40) ||
                (e.KeyValue >= 65 && e.KeyValue <= 90) ||   //a-z/A-Z
                (e.KeyValue >= 112 && e.KeyValue <= 123))   //F1-F12
            {
                keyValue.Append(e.KeyCode);
            }
            else if ((e.KeyValue >= 48 && e.KeyValue <= 57))    //0-9
            {
                keyValue.Append(e.KeyCode.ToString().Substring(1));
            }

            mSettingsData.keyValue = (keyCode << 16 | e.KeyValue);
            mSettingsData.keyString = keyValue.ToString();
            this.ActiveControl.Text = "";
            //设置当前活动控件的文本内容
            this.ActiveControl.Text = keyValue.ToString();
            tbBossKey.SelectionStart = tbBossKey.Text.Length;
        }

        private void tbBossKey_KeyUp(object sender, KeyEventArgs e)
        {
            string str = this.ActiveControl.Text.TrimEnd();
            int len = str.Length;
            if (len >= 1 && str.Substring(str.Length - 1) == "+")
            {
                this.ActiveControl.Text = "";
            }
            CheckChangesAndEnableBtn();
        }

        private void tbBossKey_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == (char)8)
            {
                this.ActiveControl.Text = "";
                mSettingsData.keyString = "";
            }
            else
            {
                e.Handled = true;
            }
        }

        private void tbWide_KeyPress(object sender, KeyPressEventArgs e)
        {
            IsValidNumber(e, tbWide);
        }

        private void tbHeight_KeyPress(object sender, KeyPressEventArgs e)
        {
            IsValidNumber(e, tbHeight);
        }

        private void IsValidNumber(KeyPressEventArgs e, TextBox tb)
        {
            if (tb.SelectedText != "")
            {
                if ((e.KeyChar >= '0') && (e.KeyChar <= '9'))
                {
                    return;
                }
            }
            if (e.KeyChar != '\b')
            {
                if ((e.KeyChar >= '0') && (e.KeyChar <= '9'))
                {
                    try
                    {
                        int intOut = int.Parse(tb.Text + e.KeyChar.ToString());
                        if (intOut < 1 || intOut > 9999)
                        {
                            e.Handled = true;
                        }
                    }
                    catch
                    {
                        e.KeyChar = (char)0;   //处理非法字符
                    }
                }
                else
                {
                    e.Handled = true;
                }
            }
        }

        private void UserSettingsForm_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == MouseButtons.Left)
            {
                GameManager.ReleaseCapture();
                GameManager.SendMessage(this.Handle,
                GameManager.WM_NCLBUTTONDOWN, GameManager.HT_CAPTION, 0);
            }
        }

        private void tbWide_TextChanged(object sender, EventArgs e)
        {
            int width = 0;
            int.TryParse(tbWide.Text, out width);
            Size temp = new Size(width, mSettingsData.GuestSize.Height);
            mSettingsData.GuestSize = temp;
            CheckChangesAndEnableBtn();
        }

        private void tbHeight_TextChanged(object sender, EventArgs e)
        {
            int height = 0;
            int.TryParse(tbHeight.Text, out height);
            Size temp = new Size(mSettingsData.GuestSize.Width,height);
            mSettingsData.GuestSize = temp;
            CheckChangesAndEnableBtn();
        }
    }
}
 