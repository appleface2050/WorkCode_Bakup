using System;
using System.Drawing;
using System.Windows.Forms;
using System.IO;
using BlueStacks.hyperDroid.Common;
using System.Linq;
using Microsoft.Win32;
using System.Diagnostics;
using System.ComponentModel;
using System.Threading;
using System.Runtime.InteropServices;
using System.Collections.Generic;
namespace BlueStacks.hyperDroid.GameManager
{
    public partial class ThemeSettings : UserControl
    {
        private TabBar mTabBar;
        private GameManager mGameManager;
        public ContextMenuStrip mSettingsMenu;
        public string mInputThemeName = null;
        public Dictionary<string, bool> themeApplied = new Dictionary<string, bool>();
        public bool firstTime = true;
        public ToolTip toolTipDropDown = new ToolTip();
        //PictureBox leftArrow;

        public static Cursor ActuallyLoadCursor(String path)
        {
            return new Cursor(LoadCursorFromFile(path));
        }

        [DllImport("user32.dll")]
        private static extern IntPtr LoadCursorFromFile(string fileName);

        public ThemeSettings()
        {
            InitializeComponent();

            //leftArrow = new PictureBox();
            //leftArrow.Size = new Size(Convert.ToInt32(this.Width * .08), Convert.ToInt32(this.Width * .06));
            //string toobDir = Directory.GetParent(Common.Strings.GMAssetDir).FullName + "\\" + "Toob";
            ////leftArrow.Location = new Point(Convert.ToInt32(this.Width * .42), Convert.ToInt32(radioButtonNewStyle.Location.Y + Convert.ToInt32(radioButtonNewStyle.Height - leftArrow.Height)));
            //leftArrow.Image = Image.FromFile(toobDir + "\\" + "android_icon.png");
            //leftArrow.SizeMode = PictureBoxSizeMode.StretchImage;
            //leftArrow.BackColor = GMColors.TransparentColor;

            //this.Controls.Add(leftArrow);
        }

        private void ButtonMouseEnter(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            /*
                        switch (button.Name)
                        {
                            case "Yes":
                                button.BackColor = ColorTranslator.FromHtml("#55e0cc");
                                break;

                        case "No":
                            button.BackColor = ColorTranslator.FromHtml("#f66a4c");
                            break;

                        case "Cancel":
                            button.BackColor = ColorTranslator.FromHtml("#88a3b3");
                            break;
                    }
                     */
        }

        private void ButtonMouseDown(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            /*
            switch (button.Name)
            {
                case "Yes":
                    button.BackColor = ColorTranslator.FromHtml("#a9efe1");
                    break;

                case "No":
                    button.BackColor = ColorTranslator.FromHtml("#fbb5a6");
                    break;

                case "Cancel":
                    button.BackColor = ColorTranslator.FromHtml("#c4d1d9");
                    break;
            } */
        }

        private void ButtonMouseUp(object sender, MouseEventArgs e)
        {
            Button button = (Button)sender;
            /*
            switch (button.Name)
            {
                case "Yes":
                    button.BackColor = ColorTranslator.FromHtml("#55e0cc");
                    break;

                case "No":
                    button.BackColor = ColorTranslator.FromHtml("#f66a4c");
                    break;

                case "Cancel":
                    button.BackColor = ColorTranslator.FromHtml("#88a3b3");
                    break;
            } */
        }

        private void ButtonMouseLeave(object sender, EventArgs e)
        {
            Button button = (Button)sender;
            /*
            switch (button.Name)
            {
                case "Yes":
                    button.BackColor = ColorTranslator.FromHtml("#84d2e4");
                    break;

                case "No":
                    button.BackColor = ColorTranslator.FromHtml("#f64c4c");
                    break;

                case "Cancel":
                    button.BackColor = ColorTranslator.FromHtml("#c1cfd8");
                    break;
            }*/
        }

        private void ButtonMouseClick(object sender, MouseEventArgs e)
        {
            ColorDialog colorDialog = new ColorDialog();
            // Show the color dialog.
            DialogResult result = colorDialog.ShowDialog();
            // See if user pressed ok.
            if (result == DialogResult.OK)
            {
                IniFile ini = new IniFile(Common.Strings.GMAssetDir + "\\" + "ThemeConfig.ini");

                Button button = (Button)sender;
                Logger.Info(button.Name + "Clicked");
                Logger.Info("Color: " + HexConverter(colorDialog.Color) + "applied");
                if (cmbTheme.SelectedItem != null && themeApplied.ContainsKey(cmbTheme.SelectedItem.ToString()))
                    themeApplied[cmbTheme.SelectedItem.ToString()] = false;
                string optionalParam = "";
                switch (button.Name)
                {
                    case "ChangeGameManagerBorderBtn":
                        ini.IniWriteValue("Color", "GameManagerBorder", HexConverter(colorDialog.Color));
                        GMColors.FormBackColor = colorDialog.Color;
                        mGameManager.BackColor = colorDialog.Color;
                        optionalParam = "AppPlayer Border";
                        break;
                    case "ChangeTabBorderBtn":
                        ini.IniWriteValue("Color", "TabBorderColor", HexConverter(colorDialog.Color));
                        GMColors.TabBorderColor = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Tab Border";
                        break;
                    case "ChangeSelectedTabTopColorBtn":
                        ini.IniWriteValue("Color", "SelectedTabTopColor", HexConverter(colorDialog.Color));
                        //GMColors.TabBackColor = colorDialog.Color;
                        GMColors.selectedTabGradientColor1 = colorDialog.Color;
                        //GMColors.selectedTabGradientColor2 = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Selected Tab Top";
                        //foreach (TabPage tabPage in mTabBar.TabPages)
                        //{
                        //    if (tabPage != null)
                        //    {
                        //        Tab tab = (Tab)tabPage;
                        //        tab.BackColor = colorDialog.Color;
                        //    }
                        //}
                        break;
                    case "ChangeSelectedTabBottomColorBtn":
                        ini.IniWriteValue("Color", "SelectedTabBottomColor", HexConverter(colorDialog.Color));
                        GMColors.TabBackColor = colorDialog.Color;
                        //GMColors.selectedTabGradientColor1 = colorDialog.Color;
                        GMColors.selectedTabGradientColor2 = colorDialog.Color;
                        mTabBar.Invalidate();
                        foreach (TabPage tabPage in mTabBar.TabPages)
                        {
                            if (tabPage != null)
                            {
                                Tab tab = (Tab)tabPage;
                                tab.BackColor = colorDialog.Color;
                            }
                        }
                        optionalParam = "Selected Tab Bottom";
                        break;
                    case "ChangeTabBarTopColorBtn":
                        ini.IniWriteValue("Color", "TabBarTopColor", HexConverter(colorDialog.Color));
                        GMColors.TabBarGradientTop = colorDialog.Color;
                        //GMColors.TabBarGradientBottom = colorDialog.Color;
                        mGameManager.Invalidate();
                        mTabBar.mBackImage = null;
                        mTabBar.Invalidate();
                        optionalParam = "TabBar Top";
                        break;
                    case "ChangeTabBarBottomColorBtn":
                        ini.IniWriteValue("Color", "TabBarBottomColor", HexConverter(colorDialog.Color));
                        //GMColors.TabBarGradientTop = colorDialog.Color;
                        GMColors.TabBarGradientBottom = colorDialog.Color;
                        mGameManager.Invalidate();
                        mTabBar.mBackImage = null;
                        mTabBar.Invalidate();
                        optionalParam = "TabBar Bottom";
                        break;
                    case "ChangeControlBarTopColorBtn":
                        ini.IniWriteValue("Color", "ControlBarTopColor", HexConverter(colorDialog.Color));
                        GMColors.ControlBarGradientTop = colorDialog.Color;
                        //GMColors.ControlBarGradientBottom = colorDialog.Color;
                        mGameManager.mControlBarLeft.Invalidate();
                        mGameManager.mControlBarRight.Invalidate();
                        optionalParam = "ControlBar Top";
                        break;
                    case "ChangeControlBarBottomColorBtn":
                        ini.IniWriteValue("Color", "ControlBarBottomColor", HexConverter(colorDialog.Color));
                        //GMColors.ControlBarGradientTop = colorDialog.Color;
                        GMColors.ControlBarGradientBottom = colorDialog.Color;
                        mGameManager.mControlBarLeft.Invalidate();
                        mGameManager.mControlBarRight.Invalidate();
                        optionalParam = "ControlBar Bottom";
                        break;
                    case "ChangeInactiveTabTopColorBtn":
                        ini.IniWriteValue("Color", "InactiveTabTopColor", HexConverter(colorDialog.Color));
                        //GMColors.TabBackColor = colorDialog.Color;
                        GMColors.inactiveTabGradientColor1 = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Inactive Tab Top";
                        break;
                    case "ChangeInactiveTabBottomColorBtn":
                        ini.IniWriteValue("Color", "InactiveTabBottomColor", HexConverter(colorDialog.Color));
                        GMColors.TabBackColor = colorDialog.Color;
                        GMColors.inactiveTabGradientColor2 = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Inactive Tab Bottom";
                        break;
                    case "ChangeTabTopColorMouseOverBtn":
                        ini.IniWriteValue("Color", "TabMouseOverTopColor", HexConverter(colorDialog.Color));
                        //GMColors.TabBackColor = colorDialog.Color;
                        GMColors.tabMouseOverGradientColor1 = colorDialog.Color;
                        //GMColors.tabMouseOverGradientColor2 = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Tab Top Color MouseOver";
                        break;
                    case "ChangeTabBottomColorMouseOverBtn":
                        ini.IniWriteValue("Color", "TabMouseOverBottomColor", HexConverter(colorDialog.Color));
                        GMColors.TabBackColor = colorDialog.Color;
                        //GMColors.tabMouseOverGradientColor1 = colorDialog.Color;
                        GMColors.tabMouseOverGradientColor2 = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Tab Bottom Color MouseOver";
                        break;
                    case "ChangeInnerBorderColorBtn":
                        ini.IniWriteValue("Color", "InnerBorderColor", HexConverter(colorDialog.Color));
                        GMColors.InnerBorderColor = colorDialog.Color;
                        foreach (TabPage tabPage in mTabBar.TabPages)
                        {
                            if (tabPage != null)
                            {
                                Tab tab = (Tab)tabPage;
                                tab.Invalidate();
                            }
                        }
                        optionalParam = "Inner Border";
                        break;
                    case "ChangeContextMenuOverColorBtn":
                        ini.IniWriteValue("Color", "ContextMenuOverColor", HexConverter(colorDialog.Color));
                        GMColors.ContextMenuHoverColor = colorDialog.Color;
                        this.mSettingsMenu.Invalidate();
                        optionalParam = "Settings Menu MouseOver";
                        break;
                    case "ChangeContextMenuForeColorBtn":
                        ini.IniWriteValue("Color", "ContextMenuForeColor", HexConverter(colorDialog.Color));
                        this.mSettingsMenu.ForeColor = colorDialog.Color;
                        GMColors.ContextMenuForeColor = colorDialog.Color;
                        optionalParam = "Settings Menu Fore Color";
                        break;
                    case "ChangeContextMenuBackColorBtn":
                        ini.IniWriteValue("Color", "ContextMenuBackColor", HexConverter(colorDialog.Color));
                        this.mSettingsMenu.BackColor = colorDialog.Color;
                        GMColors.ContextMenuBackColor = colorDialog.Color;
                        optionalParam = "Settings Menu Back Color";
                        break;
                    case "ChangeInactiveTabTextColorBtn":
                        ini.IniWriteValue("Color", "InactiveTabTextColor", HexConverter(colorDialog.Color));
                        GMColors.InactiveTabTextColor = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Inactive Tab Text";
                        break;
                    case "ChangeMouseOverTabTextColorBtn":
                        ini.IniWriteValue("Color", "MouseOverTabTextColor", HexConverter(colorDialog.Color));
                        GMColors.MouseOverTabTextColor = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "MouseOver Tab Text";
                        break;
                    case "ChangeSelectedTabTextColorBtn":
                        ini.IniWriteValue("Color", "SelectedTabTextColor", HexConverter(colorDialog.Color));
                        GMColors.SelectedTabTextColor = colorDialog.Color;
                        mTabBar.Invalidate();
                        optionalParam = "Selected Tab Text";
                        break;
                    case "ChangeToolBarTopColorBtn":
                        ini.IniWriteValue("Color", "ToolBoxGradientTopColor", HexConverter(colorDialog.Color));
                        GMColors.ToolBoxGradientTop = colorDialog.Color;
                        mGameManager.mToolBarForm.mPanelToolBox.mStartColor = colorDialog.Color;
                        mGameManager.mToolBarForm.mPanelToolBox.Invalidate();
                        optionalParam = "SideBar Top";
                        //mGameManager.mToolBarForm.Invalidate();
                        break;
                    case "ChangeToolBarBottomColorBtn":
                        ini.IniWriteValue("Color", "ToolBoxGradientBottomColor", HexConverter(colorDialog.Color));
                        GMColors.ToolBoxGradientBottom = colorDialog.Color;
                        mGameManager.mToolBarForm.mPanelToolBox.mEndColor = colorDialog.Color;
                        mGameManager.mToolBarForm.mPanelToolBox.Invalidate();
                        optionalParam = "SideBar Bottom";
                        break;
                    case "ChangeToolBarBorderColorBtn":
                        ini.IniWriteValue("Color", "ToolBoxBorderColor", HexConverter(colorDialog.Color));
                        GMColors.ToolBoxBorderColor = colorDialog.Color;
                        mGameManager.mToolBarForm.BackColor = colorDialog.Color;
                        optionalParam = "SideBar Border";
                        //GameManager.mBorderWidth = 10;
                        break;
                }

                using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
                {
                    string parentStyleName = (String)regKey.GetValue("ParentStyleTheme", "Em");
                    string themeStyleName = (String)regKey.GetValue("TabStyleTheme", "Default");
                    Stats.SendStyleAndThemeInfoStats(Strings.ChangingColor, parentStyleName, themeStyleName, optionalParam);
                }
                button.BackColor = colorDialog.Color;
            }
        }


        private static String HexConverter(Color color)
        {
            return "#" + color.R.ToString("X2") + color.G.ToString("X2") + color.B.ToString("X2");
        }

        public void SetUp(TabBar tabBar, ContextMenuStrip settingsMenu)
        {
            Logger.Info("Theme settings Setup");
            mTabBar = tabBar;
            mGameManager = tabBar.mGameManager;
            mSettingsMenu = settingsMenu;

            CustomizedToolTip toolTip = new CustomizedToolTip();
            Image oldThemeImage = Image.FromFile(GameManager.sAssetsCommonDataDir + "\\" + "oldTheme.png");
            Image newThemeImage = Image.FromFile(GameManager.sAssetsCommonDataDir + "\\" + "newTheme.png");

            pictureBoxNewStyle.Tag = new Bitmap(newThemeImage);
            pictureBoxOldStyle.Tag = new Bitmap(oldThemeImage);

            Logger.Info("Setting Tooltip");
            toolTip.AutoSize = false;
            toolTip.Size = new Size(Convert.ToInt32(this.Width * .8), Convert.ToInt32(this.Height * .8));
            toolTip.SetToolTip(pictureBoxNewStyle, " ");
            toolTip.SetToolTip(pictureBoxOldStyle, " ");
            //radioButtonNewStyle.Appearance = Appearance.Button;
            //radioButtonOldStyle.Appearance = Appearance.Button;

            pictureBoxOldStyle.Image = oldThemeImage;
            pictureBoxNewStyle.Image = newThemeImage;
            pictureBoxOldStyle.AutoSize = false;
            pictureBoxNewStyle.AutoSize = false;
            pictureBoxOldStyle.Location = new Point(Convert.ToInt32(this.Width * .05), pictureBoxOldStyle.Location.Y);
            pictureBoxNewStyle.Location = new Point(Convert.ToInt32(this.Width * .55), pictureBoxNewStyle.Location.Y);
            pictureBoxOldStyle.Size = new Size(Convert.ToInt32(this.Width * .4), Convert.ToInt32(this.Height * .4));
            pictureBoxNewStyle.Size = new Size(Convert.ToInt32(this.Width * .4), Convert.ToInt32(this.Height * .4));

            LocalizeControls();

            NewThemeBtn.Size = new Size(Convert.ToInt32(this.Width * .13), Convert.ToInt32(this.Height * .07));
            Applybtn.Size = new Size(Convert.ToInt32(this.Width * .13), Convert.ToInt32(this.Height * .07));
            cmbTheme.Size = new Size(Convert.ToInt32(this.Width * .21), Convert.ToInt32(this.Height * .06));

            SetColorButtonsSize();

            SetControlsLocation();

            //PictureBox leftArrow = new PictureBox();
            // pictureBoxLeftArrow.Size = new Size(Convert.ToInt32(this.Width * .08), Convert.ToInt32(this.Width * .06));
            //pictureBoxLeftArrow.Location = new Point(Convert.ToInt32(this.Width * .42), Convert.ToInt32(radioButtonOldStyle.Location.Y + Convert.ToInt32((radioButtonOldStyle.Height - pictureBoxLeftArrow.Height) * .5)));

            //pictureBoxRightArrow.Size = new Size(Convert.ToInt32(this.Width * .08), Convert.ToInt32(this.Width * .06));
            //pictureBoxRightArrow.Location = new Point(Convert.ToInt32(pictureBoxLeftArrow.Location.X + pictureBoxLeftArrow.Width), Convert.ToInt32(radioButtonOldStyle.Location.Y + Convert.ToInt32((radioButtonOldStyle.Height - pictureBoxRightArrow.Height) * .5)));

            pictureBoxSelected.Image = Image.FromFile(Common.Strings.GMAssetDir + "\\" + "SelectedStyle.png");

            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
            {
                string parentStyleName = (String)regKey.GetValue("ParentStyleTheme", "Em");
                string currentThemeName = (String)regKey.GetValue("TabStyleTheme", "Default");
                Logger.Info("Current Style: " + parentStyleName);
                Logger.Info("Current Theme: " + currentThemeName);
                if (parentStyleName == "Em")
                {
                    pictureBoxSelected.Parent = pictureBoxOldStyle;
                }
                else
                {
                    pictureBoxSelected.Parent = pictureBoxNewStyle;
                }

                pictureBoxSelected.Location = new Point(0, 0);
                pictureBoxSelected.Size = new Size(Convert.ToInt32(this.Width * .06), Convert.ToInt32(this.Height * .06));

                cmbTheme.DrawMode = DrawMode.OwnerDrawFixed;
                cmbTheme.DrawItem += comboBoxDrawItem;
                cmbTheme.DropDownClosed += comboBoxDropDownClosed;
                cmbTheme.MouseLeave += new EventHandler(comboBoxMouseLeave);


                string[] subdirectoryEntries = Directory.GetDirectories(Directory.GetParent(Common.Strings.GMAssetDir).FullName).Select(path => Path.GetFileName(path)).ToArray();
                foreach (string dirName in subdirectoryEntries)
                {
                    Logger.Info("Directory: " + dirName + " found");
                    cmbTheme.Items.Add(dirName);
                    themeApplied.Add(dirName, true);
                }

                cmbTheme.SelectedItem = (String)regKey.GetValue("TabStyleTheme", "Default");
                //labelCurrentTheme.Location = new Point(Convert.ToInt32((this.Width - labelCurrentTheme.Width) * .5), pictureBoxLeftArrow.Location.Y - 40);
            }
            SetGroupBoxesSize();

            SetApplyBtnLocation();

            pictureBoxOldStyle.Cursor = ActuallyLoadCursor(Common.Strings.GMAssetDir + "\\" + "Cursor.cur");
            pictureBoxNewStyle.Cursor = ActuallyLoadCursor(Common.Strings.GMAssetDir + "\\" + "Cursor.cur");

            pictureBoxOldStyle.MouseEnter += new EventHandler(StyleImageMouseEnter);
            pictureBoxOldStyle.MouseLeave += new EventHandler(StyleImageMouseLeave);

            pictureBoxNewStyle.MouseEnter += new EventHandler(StyleImageMouseEnter);
            pictureBoxNewStyle.MouseLeave += new EventHandler(StyleImageMouseLeave);
        }

        void comboBoxMouseLeave(object sender, EventArgs e)
        {
            toolTipDropDown.Hide(cmbTheme);
        }

        private void comboBoxDropDownClosed(object sender, EventArgs e)
        {
            toolTipDropDown.Hide(cmbTheme);
        }

        private void comboBoxDrawItem(object sender, DrawItemEventArgs e)
        {
            if (e.Index < 0) { return; }
            string text = cmbTheme.GetItemText(cmbTheme.Items[e.Index]);
            e.DrawBackground();
            using (SolidBrush br = new SolidBrush(e.ForeColor))
            { e.Graphics.DrawString(text, e.Font, br, e.Bounds); }
            if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
            { toolTipDropDown.Show(text, cmbTheme, e.Bounds.Right, e.Bounds.Bottom, 2000); }
            e.DrawFocusRectangle();
        }
        private void SetGroupBoxesSize()
        {
            Logger.Info("Setting Groupboxes size");
            this.grpControlBar.Size = new Size(this.grpControlBar.Width, this.ChangeControlBarTopColorBtn.Bottom + Convert.ToInt32(this.Height * .073));
            this.grpBorder.Size = new Size(this.grpBorder.Width, this.ChangeInnerBorderColorBtn.Bottom + Convert.ToInt32(this.Height * .050));
            this.grpTab.Size = new Size(this.grpTab.Width, this.ChangeTabBottomColorMouseOverBtn.Bottom + Convert.ToInt32(this.Height * .050));
            this.grpText.Size = new Size(this.grpText.Width, this.ChangeSelectedTabTextColorBtn.Bottom + Convert.ToInt32(this.Height * .050));
            this.grpLeftSideBar.Size = new Size(this.grpLeftSideBar.Width, this.ChangeToolBarTopColorBtn.Bottom + Convert.ToInt32(this.Height * .073));
            this.grpSettingsMenu.Size = new Size(this.grpSettingsMenu.Width, this.ChangeContextMenuOverColorBtn.Bottom + Convert.ToInt32(this.Height * .050));
            this.grpTabBar.Size = new Size(this.grpTabBar.Width, this.ChangeTabBarTopColorBtn.Bottom + Convert.ToInt32(this.Height * .073));
            this.grpChooseProfile.Size = new Size(this.grpChooseProfile.Width, this.NewThemeBtn.Bottom + Convert.ToInt32(this.Height * .073));
        }

        private void SetColorButtonsSize()
        {
            Logger.Info("Setting Color buttons size");
            Size colorBtnSize = new Size(Convert.ToInt32(this.Width * .035), Convert.ToInt32(this.Height * .050));
            ChangeTabBarTopColorBtn.Size = colorBtnSize;
            ChangeControlBarTopColorBtn.Size = colorBtnSize;
            ChangeTabBottomColorMouseOverBtn.Size = colorBtnSize;
            ChangeInactiveTabTextColorBtn.Size = colorBtnSize;
            ChangeSelectedTabTextColorBtn.Size = colorBtnSize;
            ChangeToolBarTopColorBtn.Size = colorBtnSize;
            ChangeContextMenuBackColorBtn.Size = colorBtnSize;
            ChangeContextMenuOverColorBtn.Size = colorBtnSize;
            ChangeSelectedTabTopColorBtn.Size = colorBtnSize;
            ChangeGameManagerBorderBtn.Size = colorBtnSize;
            ChangeTabBorderBtn.Size = colorBtnSize;
            ChangeInnerBorderColorBtn.Size = colorBtnSize;
            ChangeInactiveTabTopColorBtn.Size = colorBtnSize;
            ChangeTabBarBottomColorBtn.Size = colorBtnSize;
            ChangeControlBarBottomColorBtn.Size = colorBtnSize;
            ChangeTabTopColorMouseOverBtn.Size = colorBtnSize;
            ChangeMouseOverTabTextColorBtn.Size = colorBtnSize;
            ChangeToolBarBottomColorBtn.Size = colorBtnSize;
            ChangeContextMenuForeColorBtn.Size = colorBtnSize;
            ChangeSelectedTabBottomColorBtn.Size = colorBtnSize;
            ChangeInactiveTabBottomColorBtn.Size = colorBtnSize;
            ChangeToolBarBorderColorBtn.Size = colorBtnSize;
        }

        void StyleImageMouseEnter(object sender, EventArgs e)
        {
            //mBtnTermsNext.BackgroundImage = Image.FromFile("buttonHover.png");
            //this.Cursor = Cursors.Hand;
            //mBtnTermsNext.ForeColor = Color.FromArgb(132, 210, 228);	// #84d2e4
        }

        void StyleImageMouseLeave(object sender, EventArgs e)
        {
            //mBtnTermsNext.BackgroundImage = Image.FromFile("button.png");
            this.Cursor = Cursors.Default;
            //mBtnTermsNext.ForeColor = Color.White;
        }

        private void SetControlsLocation()
        {
            Logger.Info("Setting controls location");
            NewThemeBtn.Location = new Point(Convert.ToInt32(this.Width - (NewThemeBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));
            lblSelectTheme.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .097));
            cmbTheme.Location = new Point(lblSelectTheme.Location.X + lblSelectTheme.Width + 20, Convert.ToInt32(this.Height * .091));

            ChangeTabBarTopColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .084));
            ChangeControlBarTopColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .084));
            ChangeTabBottomColorMouseOverLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .240));
            ChangeInactiveTabTextColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .084));
            ChangeSelectedTabTextColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .170));
            ChangeToolBarTopColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .084));
            ChangeContextMenuBackColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .084));
            ChangeContextMenuOverColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .170));
            ChangeSelectedTabTopColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .084));
            ChangeInactiveTabTopColorLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .160));
            ChangeTabBorderLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .170));
            ChangeInnerBorderColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .170));
            ChangeGameManagerBorderLbl.Location = new Point(Convert.ToInt32(this.Width * .02), Convert.ToInt32(this.Height * .084));

            ChangeTabBarTopColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeTabBarTopColorBtn.Width), Convert.ToInt32(this.Height * .073));
            ChangeControlBarTopColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeControlBarTopColorBtn.Width), Convert.ToInt32(this.Height * .073));
            ChangeTabBottomColorMouseOverBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeTabBottomColorMouseOverBtn.Width), Convert.ToInt32(this.Height * .240));
            ChangeInactiveTabTextColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeInactiveTabTextColorBtn.Width), Convert.ToInt32(this.Height * .073));
            ChangeSelectedTabTextColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeSelectedTabTextColorBtn.Width), Convert.ToInt32(this.Height * .170));
            ChangeToolBarTopColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeToolBarTopColorBtn.Width), Convert.ToInt32(this.Height * .073));
            ChangeContextMenuBackColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeContextMenuBackColorBtn.Width), Convert.ToInt32(this.Height * .073));
            ChangeContextMenuOverColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeContextMenuOverColorBtn.Width), Convert.ToInt32(this.Height * .170));
            ChangeSelectedTabTopColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeSelectedTabTopColorBtn.Width), Convert.ToInt32(this.Height * .073));
            ChangeGameManagerBorderBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeGameManagerBorderBtn.Width), Convert.ToInt32(this.Height * .073));
            ChangeTabBorderBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeTabBorderBtn.Width), Convert.ToInt32(this.Height * .170));
            ChangeInnerBorderColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeInnerBorderColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .170));
            ChangeInactiveTabTopColorBtn.Location = new Point(Convert.ToInt32(this.Width * .45 - ChangeInactiveTabTopColorBtn.Width), Convert.ToInt32(this.Height * .160));

            ChangeTabBarBottomColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .084));
            ChangeControlBarBottomColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .084));
            ChangeTabTopColorMouseOverLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .240));
            ChangeMouseOverTabTextColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .084));
            ChangeToolBarBottomColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .084));
            ChangeContextMenuForeColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .084));
            ChangeSelectedTabBottomColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .084));
            ChangeToolBarBorderColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .084));
            ChangeInactiveTabBottomColorLbl.Location = new Point(Convert.ToInt32(this.Width * .51), Convert.ToInt32(this.Height * .160));

            ChangeTabBarBottomColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeTabBarBottomColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));
            ChangeControlBarBottomColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeControlBarBottomColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));
            ChangeTabTopColorMouseOverBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeTabTopColorMouseOverBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .240));
            ChangeMouseOverTabTextColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeMouseOverTabTextColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));
            ChangeToolBarBottomColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeToolBarBottomColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));
            ChangeContextMenuForeColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeContextMenuForeColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));
            ChangeSelectedTabBottomColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeSelectedTabBottomColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));
            ChangeInactiveTabBottomColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeInactiveTabBottomColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .160));
            ChangeToolBarBorderColorBtn.Location = new Point(Convert.ToInt32(this.Width - (ChangeToolBarBorderColorBtn.Width + this.Width * .05)), Convert.ToInt32(this.Height * .073));

        }

        private void LocalizeControls()
        {
            Logger.Info("Localizing Controls");
            this.grpText.Text = GameManager.sLocalizedString.ContainsKey("Text") ? GameManager.sLocalizedString["Text"] : "Text";
            this.grpTab.Text = GameManager.sLocalizedString.ContainsKey("Tab") ? GameManager.sLocalizedString["Tab"] : "Tab";
            this.grpLeftSideBar.Text = GameManager.sLocalizedString.ContainsKey("LeftSideBar") ? GameManager.sLocalizedString["LeftSideBar"] : "Left Sidebar";
            this.grpSettingsMenu.Text = GameManager.sLocalizedString.ContainsKey("SettingsMenu") ? GameManager.sLocalizedString["SettingsMenu"] : "Settings Menu";
            this.grpTabBar.Text = GameManager.sLocalizedString.ContainsKey("TabBar") ? GameManager.sLocalizedString["TabBar"] : "TabBar";
            this.grpBorder.Text = GameManager.sLocalizedString.ContainsKey("Border") ? GameManager.sLocalizedString["Border"] : "Border";
            this.groupBoxStyle.Text = GameManager.sLocalizedString.ContainsKey("Style") ? GameManager.sLocalizedString["Style"] : "Style";
            this.grpChooseProfile.Text = GameManager.sLocalizedString.ContainsKey("Themes") ? GameManager.sLocalizedString["Themes"] : "Themes";
            this.grpControlBar.Text = GameManager.sLocalizedString.ContainsKey("ControlBar") ? GameManager.sLocalizedString["ControlBar"] : "ControlBar";

            this.ChangeInactiveTabBottomColorLbl.Text = GameManager.sLocalizedString.ContainsKey("InactiveTabBottom") ? GameManager.sLocalizedString["InactiveTabBottom"] : "Inactive Tab Bottom";
            this.ChangeInactiveTabTopColorLbl.Text = GameManager.sLocalizedString.ContainsKey("InactiveTabTop") ? GameManager.sLocalizedString["InactiveTabTop"] : "Inactive Tab Top";
            this.ChangeSelectedTabBottomColorLbl.Text = GameManager.sLocalizedString.ContainsKey("SelectedTabBottom") ? GameManager.sLocalizedString["SelectedTabBottom"] : "Selected Tab Bottom";
            this.ChangeSelectedTabTopColorLbl.Text = GameManager.sLocalizedString.ContainsKey("SelectedTabTop") ? GameManager.sLocalizedString["SelectedTabTop"] : "Selected Tab Top";
            this.ChangeSelectedTabTextColorLbl.Text = GameManager.sLocalizedString.ContainsKey("SelectedTabText") ? GameManager.sLocalizedString["SelectedTabText"] : "Selected Tab Text";
            this.ChangeMouseOverTabTextColorLbl.Text = GameManager.sLocalizedString.ContainsKey("MouseOverTabText") ? GameManager.sLocalizedString["MouseOverTabText"] : "MouseOver Tab Text";
            this.ChangeInactiveTabTextColorLbl.Text = GameManager.sLocalizedString.ContainsKey("InactiveTabText") ? GameManager.sLocalizedString["InactiveTabText"] : "Inactive Tab Text";
            this.ChangeToolBarBottomColorLbl.Text = GameManager.sLocalizedString.ContainsKey("BottomGradient") ? GameManager.sLocalizedString["BottomGradient"] : "Bottom Gradient";
            this.ChangeToolBarTopColorLbl.Text = GameManager.sLocalizedString.ContainsKey("TopGradient") ? GameManager.sLocalizedString["TopGradient"] : "Top Gradient";
            this.ChangeContextMenuOverColorLbl.Text = GameManager.sLocalizedString.ContainsKey("MouseOverColor") ? GameManager.sLocalizedString["MouseOverColor"] : "MouseOver Color";
            this.ChangeContextMenuForeColorLbl.Text = GameManager.sLocalizedString.ContainsKey("ForeGroundColor") ? GameManager.sLocalizedString["ForeGroundColor"] : "Foreground Color";
            this.ChangeContextMenuBackColorLbl.Text = GameManager.sLocalizedString.ContainsKey("BackGroundColor") ? GameManager.sLocalizedString["BackGroundColor"] : "Background Color";
            this.ChangeTabTopColorMouseOverLbl.Text = GameManager.sLocalizedString.ContainsKey("TabTopColorMouseOver") ? GameManager.sLocalizedString["TabTopColorMouseOver"] : "Tab Top MouseOver";
            this.ChangeTabBottomColorMouseOverLbl.Text = GameManager.sLocalizedString.ContainsKey("TabBottomColorMouseOver") ? GameManager.sLocalizedString["TabBottomColorMouseOver"] : "Tab Bottom MouseOver";
            this.ChangeControlBarBottomColorLbl.Text = GameManager.sLocalizedString.ContainsKey("BottomGradient") ? GameManager.sLocalizedString["BottomGradient"] : "Bottom Gradient";
            this.ChangeControlBarTopColorLbl.Text = GameManager.sLocalizedString.ContainsKey("TopGradient") ? GameManager.sLocalizedString["TopGradient"] : "Top Gradient";
            this.ChangeTabBarBottomColorLbl.Text = GameManager.sLocalizedString.ContainsKey("BottomGradient") ? GameManager.sLocalizedString["BottomGradient"] : "Bottom Gradient";
            this.ChangeTabBarTopColorLbl.Text = GameManager.sLocalizedString.ContainsKey("TopGradient") ? GameManager.sLocalizedString["TopGradient"] : "Top Gradient";
            this.ChangeInnerBorderColorLbl.Text = GameManager.sLocalizedString.ContainsKey("InnerBorder") ? GameManager.sLocalizedString["InnerBorder"] : "Inner Border";
            this.ChangeTabBorderLbl.Text = GameManager.sLocalizedString.ContainsKey("TabBorder") ? GameManager.sLocalizedString["TabBorder"] : "Tab Border";
            this.ChangeToolBarBorderColorLbl.Text = GameManager.sLocalizedString.ContainsKey("ToolBarBorder") ? GameManager.sLocalizedString["ToolBarBorder"] : "Left Sidebar Border";
            this.ChangeGameManagerBorderLbl.Text = GameManager.sLocalizedString.ContainsKey("GameManagerBorder") ? GameManager.sLocalizedString["GameManagerBorder"] : "App Player Border";
            this.lblSelectTheme.Text = GameManager.sLocalizedString.ContainsKey("SelectTheme") ? GameManager.sLocalizedString["SelectTheme"] : "Select Theme";
            this.Applybtn.Text = GameManager.sLocalizedString.ContainsKey("Apply") ? GameManager.sLocalizedString["Apply"] : "Apply";
            this.NewThemeBtn.Text = GameManager.sLocalizedString.ContainsKey("NewTheme") ? GameManager.sLocalizedString["NewTheme"] : "New Theme";
        }

        private void ApplyButtonClick(object sender, EventArgs e)
        {
            Logger.Info("Apply Btn Click");
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
            {
                String existingTheme = (String)regKey.GetValue("TabStyleTheme", "Default");
                string existingStyle = (String)regKey.GetValue("ParentStyleTheme", "Em");
                string selectedThemeName = cmbTheme.SelectedItem.ToString();
                Logger.Info("Current Style: " + existingStyle);
                Logger.Info("Current Theme: " + existingTheme);
                Logger.Info("Selected Theme: " + selectedThemeName);
                //If come across same theme, do nothing
                if ((existingTheme == selectedThemeName))
                {
                    if (themeApplied.ContainsKey(existingTheme))
                        themeApplied[existingTheme] = true;
                    Logger.Info("Theme Applied");
                    return;
                }
                //If Source theme of Existing theme and new theme is different then Restart GM. Else just set the Reg Val
                //bool checkSourceThemesSame = CheckSourceThemesSame(existingTheme, cmbTheme.SelectedItem.ToString());

                //if (!checkSourceThemesSame)
                //{
                //string theme = cmbTheme.SelectedItem.ToString();
                //RestartGameManager(theme);
                //}
                //else
                //{


                string confirm = GameManager.sLocalizedString.ContainsKey("Confirm") ? GameManager.sLocalizedString["Confirm"] : "Confirm Action";
                string tempThemeName = "{DO_NOT_LOCALIZE}";
                string confirmMsgOnApplyTheme = tempThemeName + " Theme will be applied. Do you want to continue?";
                confirmMsgOnApplyTheme = GameManager.sLocalizedString.ContainsKey("ConfirmMsgOnApplyTheme") ? GameManager.sLocalizedString["ConfirmMsgOnApplyTheme"] : confirmMsgOnApplyTheme;
                confirmMsgOnApplyTheme = confirmMsgOnApplyTheme.Replace(tempThemeName, selectedThemeName);
                var confirmResult = MessageBox.Show(confirmMsgOnApplyTheme,
             confirm,
             MessageBoxButtons.YesNo);

                if (confirmResult == DialogResult.Yes)
                {
                    using (RegistryKey tempKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath))
                    {
                        tempKey.SetValue("TabStyleTheme", cmbTheme.SelectedItem.ToString());
                        Stats.SendStyleAndThemeInfoStats(Strings.ApplyingTheme, existingStyle, selectedThemeName, null);
                        if (themeApplied.ContainsKey(selectedThemeName))
                            themeApplied[selectedThemeName] = true;
                    }
                    Logger.Info("Theme Applied");
                }
                //}
            }

        }

        private string GetParentStyleName(string themeName)
        {
            string SourceAssetsDir = Directory.GetParent(Common.Strings.GMAssetDir).FullName + "\\" + (themeName == "Em" ? "Default" : themeName);
            IniFile ini = new IniFile(SourceAssetsDir + "\\" + "ThemeConfig.ini");
            string sourceStyle = ini.IniReadValue("SourceTheme", "Theme");
            return sourceStyle;
        }

        private bool CheckSourceThemesSame(String existingTheme, string newTheme)
        {
            string oldSourceAssetsDir = Directory.GetParent(Common.Strings.GMAssetDir).FullName + "\\" + (existingTheme == "Em" ? "Default" : existingTheme);
            IniFile ini = new IniFile(oldSourceAssetsDir + "\\" + "ThemeConfig.ini");
            string oldSourceTheme = ini.IniReadValue("SourceTheme", "Theme");

            string newSourceAssetsDir = Directory.GetParent(Common.Strings.GMAssetDir).FullName + "\\" + (newTheme);
            ini = new IniFile(newSourceAssetsDir + "\\" + "ThemeConfig.ini");
            string newSourceTheme = ini.IniReadValue("SourceTheme", "Theme");

            return oldSourceTheme == newSourceTheme;
        }

        private void RestartGameManager(string styleName)
        {
            Logger.Info("Restarting GameManager");
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
            {
                string restartMessage = GameManager.sLocalizedString.ContainsKey("AppRestartConfirmMsg") ? GameManager.sLocalizedString["AppRestartConfirmMsg"] : "Application will be restarted. Do you want to continue?";
                string confirm = GameManager.sLocalizedString.ContainsKey("Confirm") ? GameManager.sLocalizedString["Confirm"] : "Confirm Action";
                var confirmResult = MessageBox.Show(restartMessage,
                 confirm,
                 MessageBoxButtons.YesNo);
                if (confirmResult == DialogResult.Yes)
                {
                    using (RegistryKey tempKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath))
                    {
                        tempKey.SetValue("ParentStyleTheme", styleName);
                        tempKey.SetValue("TabStyleTheme", "Default");
                        Stats.SendStyleAndThemeInfoStats(Strings.ChangingStyle, styleName, "Default", null);
                    }

                    int agentPort = Utils.GetAgentServerPort();
                    string restartUrl = String.Format("http://127.0.0.1:{0}/{1}",
                            agentPort,
                            Common.Strings.RestartGameManagerUrl);
                    Logger.Info("Requesting Agent");
                    Common.HTTP.Client.Get(restartUrl, null, false);

                    GameManager.sForceClose = true;
                    GameManager.sGameManager.Close();
                }

            }
        }

        private void NewThemeBtn_Click(object sender, EventArgs e)
        {
            Logger.Info("New theme Button click");
            string enterThemeStr = GameManager.sLocalizedString.ContainsKey("EnterTheme") ? GameManager.sLocalizedString["EnterTheme"] : "Please enter theme name";
            mInputThemeName = InputDialog.Show(enterThemeStr);

            if (!string.IsNullOrEmpty(mInputThemeName))
            {
                Logger.Info("Inout theme name: " + mInputThemeName);
                //create folder with this name
                string targetPath = Directory.GetParent(Common.Strings.GMAssetDir).FullName + "\\" + mInputThemeName;
                bool exists = Directory.Exists(targetPath);
                if (!exists)
                {
                    if (Directory.Exists(Common.Strings.GMAssetDir))
                    {
                        string[] files = Directory.GetFiles(Common.Strings.GMAssetDir);
                        Directory.CreateDirectory(targetPath);
                        foreach (string sourceFile in files)
                        {
                            string fileName = Path.GetFileName(sourceFile);
                            string destFile = Path.Combine(targetPath, fileName);
                            File.Copy(sourceFile, destFile, true);
                        }

                        IniFile ini = new IniFile(Common.Strings.GMAssetDir + "\\" + "ThemeConfig.ini");
                        string parentStyleName = ini.IniReadValue("SourceTheme", "Theme");
                        Common.Strings.GMAssetDir = targetPath;
                        string[] subdirectoryEntries = Directory.GetDirectories(Directory.GetParent(Common.Strings.GMAssetDir).FullName).Select(path => Path.GetFileName(path)).ToArray();

                        if (subdirectoryEntries != null && subdirectoryEntries.Length > 0)
                        {
                            ini = new IniFile(Common.Strings.GMAssetDir + "\\" + "ThemeConfig.ini");
                            ini.IniWriteValue("SourceTheme", "Theme", parentStyleName);

                            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
                            {
                                Stats.SendStyleAndThemeInfoStats(Strings.NewThemeCreated, parentStyleName, mInputThemeName, null);
                            }
                            cmbTheme.Items.Clear();
                            foreach (string dirName in subdirectoryEntries)
                            {
                                cmbTheme.Items.Add(dirName);
                            }

                            themeApplied.Add(mInputThemeName, false);
                            cmbTheme.SelectedItem = mInputThemeName;
                            Logger.Info("Theme: " + mInputThemeName + " created");
                            cmbTheme_SelectedIndexChanged(cmbTheme, EventArgs.Empty);
                        }
                        {
                            Logger.Info("No Directory found");
                        }
                    }
                    else
                    {
                        Logger.Info("Source path does not exist");
                    }

                }
                else
                {
                    string themeAlreadyExists = GameManager.sLocalizedString.ContainsKey("ThemeAlreadyExists") ? GameManager.sLocalizedString["ThemeAlreadyExists"] : "Theme already exists";
                    MessageBox.Show(themeAlreadyExists);
                    Logger.Info("Theme already exists");
                }
            }
        }

        private void cmbTheme_SelectedIndexChanged(object sender, EventArgs e)
        {
            Logger.Info("theme dropdown index change event");

            this.Applybtn.Visible = true;
            string selectedTheme = cmbTheme.SelectedItem.ToString();
            bool enableControls = !(selectedTheme == "Default");
            Logger.Info("Selected theme: " + selectedTheme);
            //if (firstTime)
            //{
            //  firstTime = false;
            //}
            //else
            //{
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
            {
                String existingTheme = (String)regKey.GetValue("TabStyleTheme", "Default");
                Logger.Info("Existing theme: " + existingTheme);
                if (selectedTheme != existingTheme)
                {
                    if (themeApplied.ContainsKey(selectedTheme))
                    {
                        themeApplied[selectedTheme] = false;
                    }
                }
            }
            //}


            ShowControls(enableControls);

            enableControls = SetApplyBtnLocation();

            EnableColorButtonControls(enableControls);

            //if (enableControls)
            //{
            Common.Strings.GMAssetDir = Directory.GetParent(Common.Strings.GMAssetDir).FullName + "\\" + cmbTheme.SelectedItem;
            string iniPath = Common.Strings.GMAssetDir + "\\" + "ThemeConfig.ini";
            IniFile ini = new IniFile(iniPath);

            SetGMColors(ini);

            RefreshTheme(ini);

            ChangeButtonsColor(ini);
            //}
        }

        private bool SetApplyBtnLocation()
        {
            Logger.Info("Setting Apply Btn location");
            bool enableControls = !(cmbTheme.SelectedItem.ToString() == "Default");
            int yCordinate = 0;
            if (grpControlBar.Location.Y > grpBorder.Location.Y)
            {
                yCordinate = grpControlBar.Location.Y + grpControlBar.Height + Convert.ToInt32(this.Height * .073);
            }
            else
            {
                yCordinate = grpBorder.Location.Y + grpBorder.Height + Convert.ToInt32(this.Height * .073);
            }
            int applyBtnY = enableControls ? yCordinate : (grpChooseProfile.Height + grpChooseProfile.Location.Y + Convert.ToInt32(this.Height * .073));
            Applybtn.Location = new Point(Convert.ToInt32(this.Width * .5 - Applybtn.Width * .5), applyBtnY);
            return enableControls;
        }

        private void EnableColorButtonControls(bool enableControl)
        {
            Logger.Info("Enable color buttons: " + enableControl);
            this.ChangeContextMenuBackColorBtn.Enabled = enableControl;
            this.ChangeContextMenuForeColorBtn.Enabled = enableControl;
            this.ChangeContextMenuOverColorBtn.Enabled = enableControl;
            this.ChangeGameManagerBorderBtn.Enabled = enableControl;
            this.ChangeInactiveTabTopColorBtn.Enabled = enableControl;
            this.ChangeInactiveTabTextColorBtn.Enabled = enableControl;
            this.ChangeInnerBorderColorBtn.Enabled = enableControl;
            this.ChangeMouseOverTabTextColorBtn.Enabled = enableControl;
            this.ChangeSelectedTabTopColorBtn.Enabled = enableControl;
            this.ChangeSelectedTabTextColorBtn.Enabled = enableControl;
            this.ChangeTabBarTopColorBtn.Enabled = enableControl;
            this.ChangeTabBorderBtn.Enabled = enableControl;
            this.ChangeTabTopColorMouseOverBtn.Enabled = enableControl;
            this.ChangeToolBarBorderColorBtn.Enabled = enableControl;
            this.ChangeToolBarBottomColorBtn.Enabled = enableControl;
            this.ChangeToolBarTopColorBtn.Enabled = enableControl;
            this.ChangeControlBarTopColorBtn.Enabled = enableControl;
            this.ChangeTabBarBottomColorBtn.Enabled = enableControl;
            this.ChangeSelectedTabBottomColorBtn.Enabled = enableControl;
            this.ChangeInactiveTabBottomColorBtn.Enabled = enableControl;
            this.ChangeControlBarBottomColorBtn.Enabled = enableControl;
            this.ChangeTabBottomColorMouseOverBtn.Enabled = enableControl;
        }

        private void ChangeButtonsColor(IniFile ini)
        {
            Logger.Info("Change buttons color as per ini");
            this.ChangeContextMenuBackColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ContextMenuBackColor"));
            this.ChangeContextMenuForeColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ContextMenuForeColor"));
            this.ChangeContextMenuOverColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ContextMenuOverColor"));
            this.ChangeGameManagerBorderBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "GameManagerBorder"));
            this.ChangeInactiveTabTopColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "InactiveTabTopColor"));
            this.ChangeInactiveTabBottomColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "InactiveTabBottomColor"));
            this.ChangeInactiveTabTextColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "InactiveTabTextColor"));
            this.ChangeInnerBorderColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "InnerBorderColor"));
            this.ChangeMouseOverTabTextColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "MouseOverTabTextColor"));
            this.ChangeSelectedTabTopColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "SelectedTabTopColor"));
            this.ChangeSelectedTabBottomColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "SelectedTabBottomColor"));
            this.ChangeSelectedTabTextColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "SelectedTabTextColor"));
            this.ChangeTabBarTopColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "TabBarTopColor"));
            this.ChangeTabBarBottomColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "TabBarBottomColor"));
            this.ChangeTabBorderBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "TabBorderColor"));
            this.ChangeTabTopColorMouseOverBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "TabMouseOverTopColor"));
            this.ChangeToolBarBorderColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ToolBoxBorderColor"));
            this.ChangeToolBarBottomColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ToolBoxGradientBottomColor"));
            this.ChangeToolBarTopColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ToolBoxGradientTopColor"));
            this.ChangeControlBarTopColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ControlBarTopColor"));
            this.ChangeControlBarBottomColorBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ControlBarBottomColor"));
            this.ChangeTabBottomColorMouseOverBtn.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "TabMouseOverBottomColor"));
        }

        private void RefreshTheme(IniFile ini)
        {
            Logger.Info("Refreshing theme on dropdown index change");
            mGameManager.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "GameManagerBorder"));
            mGameManager.Invalidate();
            mTabBar.mBackImage = null;
            mTabBar.Invalidate();
            foreach (TabPage tabPage in mTabBar.TabPages)
            {
                if (tabPage != null)
                {
                    Tab tab = (Tab)tabPage;
                    tab.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "SelectedTabBottomColor"));
                    tab.Invalidate();
                }
            }
            mGameManager.mControlBarLeft.Invalidate();
            mGameManager.mControlBarRight.Invalidate();
            this.mSettingsMenu.Invalidate();
            this.mSettingsMenu.ForeColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ContextMenuForeColor")); ;
            GMColors.ContextMenuForeColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ContextMenuForeColor"));
            this.mSettingsMenu.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ContextMenuBackColor"));
            if (mGameManager.mToolBarForm != null)
            {
                mGameManager.mToolBarForm.mPanelToolBox.mStartColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ToolBoxGradientTopColor"));
                mGameManager.mToolBarForm.mPanelToolBox.Invalidate();
                mGameManager.mToolBarForm.mPanelToolBox.mEndColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ToolBoxGradientBottomColor"));
                mGameManager.mToolBarForm.mPanelToolBox.Invalidate();
                mGameManager.mToolBarForm.BackColor = ColorTranslator.FromHtml(ini.IniReadValue("Color", "ToolBoxBorderColor"));
            }
        }

        private static void SetGMColors(IniFile ini)
        {
            Logger.Info("Setting GM colors");
            string selectedTabTextColor = ini.IniReadValue("Color", "SelectedTabTextColor");
            if (!string.IsNullOrEmpty(selectedTabTextColor))
            {
                GMColors.SelectedTabTextColor = ColorTranslator.FromHtml(selectedTabTextColor);
            }
            string inactiveTabTopColor = ini.IniReadValue("Color", "InactiveTabTopColor");
            if (!string.IsNullOrEmpty(inactiveTabTopColor))
            {
                GMColors.inactiveTabGradientColor1 = ColorTranslator.FromHtml(inactiveTabTopColor);
            }

            string inactiveTabBottomColor = ini.IniReadValue("Color", "InactiveTabBottomColor");
            if (!string.IsNullOrEmpty(inactiveTabBottomColor))
            {
                GMColors.inactiveTabGradientColor2 = ColorTranslator.FromHtml(inactiveTabBottomColor);
            }

            string tabBackColor = ini.IniReadValue("Color", "SelectedTabBottomColor");
            if (!string.IsNullOrEmpty(tabBackColor))
            {
                GMColors.TabBackColor = ColorTranslator.FromHtml(tabBackColor);//ColorTranslator.FromHtml("#6B869F");
            }

            string selectedTabTopColor = ini.IniReadValue("Color", "SelectedTabTopColor");
            if (!string.IsNullOrEmpty(selectedTabTopColor))
            {
                GMColors.selectedTabGradientColor1 = ColorTranslator.FromHtml(selectedTabTopColor);
            }

            GMColors.selectedTabGradientColor2 = GMColors.TabBackColor;

            string inactiveTabTextColor = ini.IniReadValue("Color", "InactiveTabTextColor");
            if (!string.IsNullOrEmpty(inactiveTabTextColor))
            {
                GMColors.InactiveTabTextColor = ColorTranslator.FromHtml(inactiveTabTextColor);
            }

            string mouseOverTabTextColor = ini.IniReadValue("Color", "MouseOverTabTextColor");
            if (!string.IsNullOrEmpty(mouseOverTabTextColor))
            {
                GMColors.MouseOverTabTextColor = ColorTranslator.FromHtml(mouseOverTabTextColor);
            }

            string tabBarTopColor = ini.IniReadValue("Color", "TabBarTopColor");
            if (!string.IsNullOrEmpty(tabBarTopColor))
            {
                GMColors.TabBarGradientTop = ColorTranslator.FromHtml(tabBarTopColor);
            }

            string tabBarBottomColor = ini.IniReadValue("Color", "TabBarBottomColor");
            if (!string.IsNullOrEmpty(tabBarBottomColor))
            {
                GMColors.TabBarGradientBottom = ColorTranslator.FromHtml(tabBarBottomColor);
            }

            string controlBarTopColor = ini.IniReadValue("Color", "ControlBarTopColor");
            if (!string.IsNullOrEmpty(controlBarTopColor))
            {
                GMColors.ControlBarGradientTop = ColorTranslator.FromHtml(controlBarTopColor);
            }

            string controlBarBottomColor = ini.IniReadValue("Color", "ControlBarBottomColor");
            if (!string.IsNullOrEmpty(controlBarBottomColor))
            {
                GMColors.ControlBarGradientBottom = ColorTranslator.FromHtml(controlBarBottomColor);
            }

            string formBackColor = ini.IniReadValue("Color", "GameManagerBorder");
            if (!string.IsNullOrEmpty(formBackColor))
            {
                GMColors.FormBackColor = ColorTranslator.FromHtml(formBackColor);
            }

            string tabBorderColor = ini.IniReadValue("Color", "TabBorderColor");
            if (!string.IsNullOrEmpty(tabBorderColor))
            {
                GMColors.TabBorderColor = ColorTranslator.FromHtml(tabBorderColor); //ColorTranslator.FromHtml("#000000");
            }

            string tabMouseOverTopColor = ini.IniReadValue("Color", "TabMouseOverTopColor");
            if (!string.IsNullOrEmpty(tabMouseOverTopColor))
            {
                GMColors.tabMouseOverGradientColor1 = ColorTranslator.FromHtml(tabMouseOverTopColor);
            }

            string tabMouseOverBottomColor = ini.IniReadValue("Color", "TabMouseOverBottomColor");
            if (!string.IsNullOrEmpty(tabMouseOverBottomColor))
            {
                GMColors.tabMouseOverGradientColor2 = ColorTranslator.FromHtml(tabMouseOverBottomColor);
            }

            string innerBorderColor = ini.IniReadValue("Color", "InnerBorderColor");
            if (!string.IsNullOrEmpty(innerBorderColor))
            {
                GMColors.InnerBorderColor = ColorTranslator.FromHtml(innerBorderColor);
            }

            string contextMenuBackColor = ini.IniReadValue("Color", "ContextMenuBackColor");
            if (!string.IsNullOrEmpty(contextMenuBackColor))
            {
                GMColors.ContextMenuBackColor = ColorTranslator.FromHtml(contextMenuBackColor);
            }

            string contextMenuForeColor = ini.IniReadValue("Color", "ContextMenuForeColor");
            if (!string.IsNullOrEmpty(contextMenuForeColor))
            {
                GMColors.ContextMenuForeColor = ColorTranslator.FromHtml(contextMenuForeColor);
            }

            string contextMenuHoverColor = ini.IniReadValue("Color", "ContextMenuHoverColor");
            if (!string.IsNullOrEmpty(contextMenuHoverColor))
            {
                GMColors.ContextMenuHoverColor = ColorTranslator.FromHtml(contextMenuHoverColor);
            }

            string toolBoxGradientTop = ini.IniReadValue("Color", "ToolBoxGradientTopColor");
            if (!string.IsNullOrEmpty(toolBoxGradientTop))
            {
                GMColors.ToolBoxGradientTop = ColorTranslator.FromHtml(toolBoxGradientTop);
            }
            string toolBoxGradientBottom = ini.IniReadValue("Color", "ToolBoxGradientBottomColor");
            if (!string.IsNullOrEmpty(toolBoxGradientBottom))
            {
                GMColors.ToolBoxGradientBottom = ColorTranslator.FromHtml(toolBoxGradientBottom);
            }

            string toolBoxBackColor = ini.IniReadValue("Color", "ToolBoxBorderColor");
            if (!string.IsNullOrEmpty(toolBoxBackColor))
            {
                GMColors.ToolBoxBorderColor = ColorTranslator.FromHtml(toolBoxBackColor);
            }
        }

        private void ShowControls(bool showControl)
        {
            Logger.Info("Show control: " + showControl);
            //this.grpControlBar.Size = new Size(1090,95);
            this.grpControlBar.Visible = showControl;
            //this.grpBorder.Size = new Size(1090, 180);
            this.grpBorder.Visible = showControl;
            this.grpTab.Visible = showControl;
            this.grpText.Visible = showControl;
            this.grpLeftSideBar.Visible = showControl;
            this.grpSettingsMenu.Visible = showControl;
            this.grpTabBar.Visible = showControl;
        }

        private void pictureBoxStyle_Click(object sender, EventArgs e)
        {
            Logger.Info("Style image click");
            PictureBox pictureBox = (PictureBox)sender;
            using (RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
            {
                String regValue = (String)regKey.GetValue("ParentStyleTheme", "Em");

                //If Source theme of Existing theme and new theme is different then Restart GM. Else just set the Reg Val
                //bool checkSourceThemesSame = CheckSourceThemesSame(regValue, pictureBox.Text);
                Logger.Info("Current style: " + regValue);
                Logger.Info("Click on : " + pictureBox.Text);
                if (regValue != pictureBox.Text)
                {
                    RestartGameManager(pictureBox.Text);
                }

            }
        }

        private void pictureBox_MouseEnter(object sender, EventArgs e)
        {
            try
            {
                PictureBox picBox = (PictureBox)(sender);
                picBox.BorderStyle = BorderStyle.Fixed3D;
            }
            catch
            {
                return;
            }
        }

        private void pictureBox_MouseLeave(object sender, EventArgs e)
        {
            try
            {
                PictureBox picBox = (PictureBox)(sender);
                picBox.BorderStyle = BorderStyle.None;
            }
            catch
            {
                return;
            }
        }
    }
}
