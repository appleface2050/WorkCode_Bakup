using System;
using System.Collections.Generic;
using System.Text;
using System.Drawing;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
    static class GMColors
    {
        public static Color inactiveTabGradientColor1;
        public static Color inactiveTabGradientColor2;
        public static Color selectedTabGradientColor1;
        public static Color selectedTabGradientColor2;
        public static Color InactiveTabTextColor;
        public static Color MouseOverTabTextColor;
        public static Color TabBackColor;
        public static Color TabBarGradientTop;
        public static Color TabBarGradientBottom;
        public static Color ControlBarGradientTop;
        public static Color ControlBarGradientBottom;
        public static Color SettingsFormBackGroundColor;
        public static Color SettingsFormTitleBarColor;
        public static Color WhiteColor = ColorTranslator.FromHtml("#FFFFFF");
        public static Color GreyColor = ColorTranslator.FromHtml("#999999");
        public static Color TabBorderColor = ColorTranslator.FromHtml("#000000");
        public static Color disabledTabGradientColor1 = ColorTranslator.FromHtml("#3e3d46");
        public static Color disabledTabGradientColor2 = ColorTranslator.FromHtml("#292832");

        public static Color tabMouseOverGradientColor1 = ColorTranslator.FromHtml("#6C697F");
        public static Color tabMouseOverGradientColor2 = ColorTranslator.FromHtml("#464459");

        // Tab text
        public static Color SelectedTabTextColor = ColorTranslator.FromHtml("#FFFFFF");

        public static Color ContextMenuBackColor = ColorTranslator.FromHtml("#31303A");
        public static Color ContextMenuForeColor = ColorTranslator.FromHtml("#FFFFFF");

        public static Color ToolTipBackColor = ColorTranslator.FromHtml("#31303A");
        public static Color ToolTipForeColor = ColorTranslator.FromHtml("#FFFFFF");

        public static Color StreamWindowForeColor = ColorTranslator.FromHtml("#FFFFFF");

        public static Color SettingsFormForeColor = ColorTranslator.FromHtml("#ffffff");

        public static Color ContextMenuHoverColor = ColorTranslator.FromHtml("#484658");

        public static Color TransparentColor = Color.Transparent;

        public static Color FormBackColor = ColorTranslator.FromHtml("#0B0D11");

        public static Color StreamWindowBackColor = ColorTranslator.FromHtml("#2F3B5A");

        public static Color StreamWindowTitleForeColor = ColorTranslator.FromHtml("#b4cbe9");

        public static Color TopBarBorderPenColor = FormBackColor;

        //public static Color	controlBarBorderColorDarkTheme	= ColorTranslator.FromHtml("#3a29d6");

        // Left control box
        public static Color ToolBoxDividerGradientTop = ColorTranslator.FromHtml("#73CFFF");
        public static Color ToolBoxDividerGradientBottom = ColorTranslator.FromHtml("#5C58F2");

        public static Color ToolBoxGradientTop = ColorTranslator.FromHtml("#3E3D46");
        public static Color ToolBoxGradientBottom = ColorTranslator.FromHtml("#292832");

        public static Color ToolBoxBorderColor = ColorTranslator.FromHtml("#0B0D11");

        public static Color InnerBorderColor = ColorTranslator.FromHtml("#000000");

        static GMColors()
        {
            using (RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
            {
                string parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", "Em");
                string tabStyleTheme = (string)configKey.GetValue("TabStyleTheme", "Default");

                if ((string.IsNullOrEmpty(parentStyleTheme) || string.IsNullOrEmpty(tabStyleTheme)) || (parentStyleTheme == "Em" && tabStyleTheme == "Default"))
                {
                    inactiveTabGradientColor1 = ColorTranslator.FromHtml("#3E3D46");
                    inactiveTabGradientColor2 = ColorTranslator.FromHtml("#292832");

                    selectedTabGradientColor1 = ColorTranslator.FromHtml("#8A87A1");
                    selectedTabGradientColor2 = ColorTranslator.FromHtml("#65627D");

                    InactiveTabTextColor = ColorTranslator.FromHtml("#9C9DA9");
                    MouseOverTabTextColor = ColorTranslator.FromHtml("#9C9DA9");

                    TabBackColor = ColorTranslator.FromHtml("#727374");

                    TabBarGradientTop = ColorTranslator.FromHtml("#51505C");
                    TabBarGradientBottom = ColorTranslator.FromHtml("#3E3C4C");

                    SettingsFormBackGroundColor = ColorTranslator.FromHtml("#51505C");
                    SettingsFormTitleBarColor = ColorTranslator.FromHtml("#3E3C4C");

                    ControlBarGradientTop = TabBarGradientTop;
                    ControlBarGradientBottom = TabBarGradientBottom;
                }
                else
                {
                    IniFile ini = new IniFile(Common.Strings.GMAssetDir + "\\" + "ThemeConfig.ini");
                    string selectedTabTextColor = ini.IniReadValue("Color", "SelectedTabTextColor");
                    if (!string.IsNullOrEmpty(selectedTabTextColor))
                    {
                        SelectedTabTextColor = ColorTranslator.FromHtml(selectedTabTextColor);
                    }
                    string inactiveTabTopColor = ini.IniReadValue("Color", "InactiveTabTopColor");
                    if (!string.IsNullOrEmpty(inactiveTabTopColor))
                    {
                        inactiveTabGradientColor1 = ColorTranslator.FromHtml(inactiveTabTopColor);
                    }
                    else
                    {
                        inactiveTabGradientColor1 = ColorTranslator.FromHtml("#5D6B78");
                    }

                    string inactiveTabBottomColor = ini.IniReadValue("Color", "InactiveTabBottomColor");
                    if (!string.IsNullOrEmpty(inactiveTabBottomColor))
                    {
                        inactiveTabGradientColor2 = ColorTranslator.FromHtml(inactiveTabBottomColor);
                    }
                    else
                    {
                        inactiveTabGradientColor2 = ColorTranslator.FromHtml("#5D6B78");
                    }
                    string tabBackColor = ini.IniReadValue("Color", "SelectedTabBottomColor");
                    if (!string.IsNullOrEmpty(tabBackColor))
                    {
                        TabBackColor = ColorTranslator.FromHtml(tabBackColor);//ColorTranslator.FromHtml("#6B869F");
                    }
                    else
                    {
                        TabBackColor = ColorTranslator.FromHtml("#6B869F");
                    }
                    string selectedTabTopColor = ini.IniReadValue("Color", "SelectedTabTopColor");
                    if (!string.IsNullOrEmpty(selectedTabTopColor))
                    {
                        selectedTabGradientColor1 = ColorTranslator.FromHtml(selectedTabTopColor);
                    }
                    else
                    {
                        selectedTabGradientColor1 = TabBackColor;
                    }
                    selectedTabGradientColor2 = TabBackColor;

                    string inactiveTabTextColor = ini.IniReadValue("Color", "InactiveTabTextColor");
                    if (!string.IsNullOrEmpty(inactiveTabTextColor))
                    {
                        InactiveTabTextColor = ColorTranslator.FromHtml(inactiveTabTextColor);
                    }
                    else
                    {
                        InactiveTabTextColor = ColorTranslator.FromHtml("#FFFFFF");
                    }

                    string mouseOverTabTextColor = ini.IniReadValue("Color", "MouseOverTabTextColor");
                    if (!string.IsNullOrEmpty(mouseOverTabTextColor))
                    {
                        MouseOverTabTextColor = ColorTranslator.FromHtml(mouseOverTabTextColor);
                    }
                    else
                    {
                        MouseOverTabTextColor = ColorTranslator.FromHtml("#FFFFFF");
                    }

                    string tabBarTopColor = ini.IniReadValue("Color", "TabBarTopColor");
                    if (!string.IsNullOrEmpty(tabBarTopColor))
                    {
                        TabBarGradientTop = ColorTranslator.FromHtml(tabBarTopColor);
                    }
                    else
                    {
                        TabBarGradientTop = ColorTranslator.FromHtml("#3A4149");
                    }

                    string tabBarBottomColor = ini.IniReadValue("Color", "TabBarBottomColor");
                    if (!string.IsNullOrEmpty(tabBarBottomColor))
                    {
                        TabBarGradientBottom = ColorTranslator.FromHtml(tabBarBottomColor);
                    }
                    else
                    {
                        TabBarGradientBottom = ColorTranslator.FromHtml("#3A4149");
                    }
                    string settingsFormBackGroundtColor = ini.IniReadValue("Color", "SettingsFormBackGroundtColor");
                    if (!string.IsNullOrEmpty(settingsFormBackGroundtColor))
                    {
                        SettingsFormBackGroundColor = ColorTranslator.FromHtml(settingsFormBackGroundtColor);
                        SettingsFormTitleBarColor = ColorTranslator.FromHtml(settingsFormBackGroundtColor);
                    }
                    else
                    {
                        SettingsFormBackGroundColor = ColorTranslator.FromHtml("#3A4149");
                        SettingsFormTitleBarColor = ColorTranslator.FromHtml("#3A4149");
                    }

                    string controlBarTopColor = ini.IniReadValue("Color", "ControlBarTopColor");
                    if (!string.IsNullOrEmpty(controlBarTopColor))
                    {
                        ControlBarGradientTop = ColorTranslator.FromHtml(controlBarTopColor);
                    }
                    else
                    {
                        ControlBarGradientTop = ColorTranslator.FromHtml("#3A4149");
                    }

                    string controlBarBottomColor = ini.IniReadValue("Color", "ControlBarBottomColor");
                    if (!string.IsNullOrEmpty(controlBarBottomColor))
                    {
                        ControlBarGradientBottom = ColorTranslator.FromHtml(controlBarBottomColor);
                    }
                    else
                    {
                        ControlBarGradientBottom = ColorTranslator.FromHtml("#3A4149");
                    }
                    //need to add if color not found or null

                    string formBackColor = ini.IniReadValue("Color", "GameManagerBorder");
                    if (!string.IsNullOrEmpty(formBackColor))
                    {
                        FormBackColor = ColorTranslator.FromHtml(formBackColor);
                    }

                    TopBarBorderPenColor = ColorTranslator.FromHtml("#ADA3E0");// FormBackColor;
                    string tabBorderColor = ini.IniReadValue("Color", "TabBorderColor");
                    if (!string.IsNullOrEmpty(tabBorderColor))
                    {
                        TabBorderColor = ColorTranslator.FromHtml(tabBorderColor); //ColorTranslator.FromHtml("#000000");
                    }

                    string tabMouseOverTopColor = ini.IniReadValue("Color", "TabMouseOverTopColor");
                    if (!string.IsNullOrEmpty(tabMouseOverTopColor))
                    {
                        tabMouseOverGradientColor1 = ColorTranslator.FromHtml(tabMouseOverTopColor);
                    }

                    string tabMouseOverBottomColor = ini.IniReadValue("Color", "TabMouseOverBottomColor");
                    if (!string.IsNullOrEmpty(tabMouseOverBottomColor))
                    {
                        tabMouseOverGradientColor2 = ColorTranslator.FromHtml(tabMouseOverBottomColor);
                    }

                    string innerBorderColor = ini.IniReadValue("Color", "InnerBorderColor");
                    if (!string.IsNullOrEmpty(innerBorderColor))
                    {
                        InnerBorderColor = ColorTranslator.FromHtml(innerBorderColor);
                    }

                    string contextMenuBackColor = ini.IniReadValue("Color", "ContextMenuBackColor");
                    if (!string.IsNullOrEmpty(contextMenuBackColor))
                    {
                        ContextMenuBackColor = ColorTranslator.FromHtml(contextMenuBackColor);
                    }

                    string contextMenuForeColor = ini.IniReadValue("Color", "ContextMenuForeColor");
                    if (!string.IsNullOrEmpty(contextMenuForeColor))
                    {
                        ContextMenuForeColor = ColorTranslator.FromHtml(contextMenuForeColor);
                    }

                    string strStreamWindowForeColor = ini.IniReadValue("Color", "BtvClosingPromptForeColor");
                    if (!string.IsNullOrEmpty(strStreamWindowForeColor))
                    {
                        StreamWindowForeColor = ColorTranslator.FromHtml(strStreamWindowForeColor);
                    }

                    string settingsFormForeColor = ini.IniReadValue("Color", "SettingsFormForeColor");
                    if (!string.IsNullOrEmpty(settingsFormForeColor))
                    {
                        SettingsFormForeColor = ColorTranslator.FromHtml(settingsFormForeColor);
                    }

                    string contextMenuHoverColor = ini.IniReadValue("Color", "ContextMenuHoverColor");
                    if (!string.IsNullOrEmpty(contextMenuHoverColor))
                    {
                        ContextMenuHoverColor = ColorTranslator.FromHtml(contextMenuHoverColor);
                    }

                    string toolBoxGradientTop = ini.IniReadValue("Color", "ToolBoxGradientTopColor");
                    if (!string.IsNullOrEmpty(toolBoxGradientTop))
                    {
                        ToolBoxGradientTop = ColorTranslator.FromHtml(toolBoxGradientTop);
                    }
                    string toolBoxGradientBottom = ini.IniReadValue("Color", "ToolBoxGradientBottomColor");
                    if (!string.IsNullOrEmpty(toolBoxGradientBottom))
                    {
                        ToolBoxGradientBottom = ColorTranslator.FromHtml(toolBoxGradientBottom);
                    }

                    string toolBoxBackColor = ini.IniReadValue("Color", "ToolBoxBorderColor");
                    if (!string.IsNullOrEmpty(toolBoxBackColor))
                    {
                        ToolBoxBorderColor = ColorTranslator.FromHtml(toolBoxBackColor);
                    }
                }
            }

        }
    }
}
