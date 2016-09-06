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

namespace BlueStacks.hyperDroid.GameManager
{
	public class GameManager : Form
	{

		int width =  Convert.ToInt32(GameManager.sFrontendWidth * .6);
		int height = Convert.ToInt32(GameManager.sFrontendHeight * .6);
		int topPadding		= Convert.ToInt32(height*.08);
		int bottomPadding	= Convert.ToInt32(height*.1);
		int leftPadding		= Convert.ToInt32(width*.1);
		int rightPadding	= Convert.ToInt32(width*.1);
		int imageSpacing	= Convert.ToInt32(width*.1);

		int textFontSize = Convert.ToInt32(height*.05);
		mChangeThemeForm.ClientSize = new Size(width, height);
		mChangeThemeForm.FormClosing += new FormClosingEventHandler(FormClosing);
		mChangeThemeForm.Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		mChangeThemeForm.StartPosition = FormStartPosition.CenterScreen;
		mChangeThemeForm.Text = "Change Theme";
		mChangeThemeForm.BackColor = Color.White;
		mChangeThemeForm.MinimizeBox = false;
		mChangeThemeForm.MaximizeBox = false;

		int gmWidth = GameManager.sGameManager.Width;
		int gmHeight = GameManager.sGameManager.Height;
		//int buttonFontSize = Convert.ToInt32(gmWidth*.010);

		//FontFamily family = GameManager.sFontCollection.Families[0];
		//Font font = new Font(family, buttonFontSize, FontStyle.Regular, GraphicsUnit.Pixel, ((byte)(0)));

		//Change GameManager Border
		Button ChangeGameManagerBorderBtn = new Button();
		ChangeGameManagerBorderBtn.Text = "Change GameManager Border";
		ChangeGameManagerBorderBtn.Name = "ChangeGameManagerBorderBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeGameManagerBorderBtn.Width = Convert.ToInt32(width *.2);
		ChangeGameManagerBorderBtn.Height = Convert.ToInt32(height *.1);
		ChangeGameManagerBorderBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeGameManagerBorderBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		//ChangeGameManagerBorderBtn.Location = new Point(
		//		leftPadding,
		//		this.ClientSize.Height - ChangeGameManagerBorderBtn.Height - bottomPadding);
		ChangeGameManagerBorderBtn.FlatStyle = FlatStyle.Flat;
		ChangeGameManagerBorderBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeGameManagerBorderBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeGameManagerBorderBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeGameManagerBorderBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeGameManagerBorderBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeGameManagerBorderBtn);

		//Change Tab Border
		Button ChangeTabBorderBtn = new Button();
		ChangeTabBorderBtn.Text = "Change Tab Border";
		ChangeTabBorderBtn.Name = "ChangeTabBorderBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeTabBorderBtn.Width = Convert.ToInt32(width * .2);
		ChangeTabBorderBtn.Height = Convert.ToInt32(height * .1);
		ChangeTabBorderBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeTabBorderBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeTabBorderBtn.Location = new Point(
				300, 0);
		ChangeTabBorderBtn.FlatStyle = FlatStyle.Flat;
		ChangeTabBorderBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeTabBorderBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeTabBorderBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeTabBorderBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeTabBorderBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeTabBorderBtn);

		//Change Selected Tab Color
		Button ChangeSelectedTabColorBtn = new Button();
		ChangeSelectedTabColorBtn.Text = "Change Selected Tab Color";
		ChangeSelectedTabColorBtn.Name = "ChangeSelectedTabColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeSelectedTabColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeSelectedTabColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeSelectedTabColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeSelectedTabColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeSelectedTabColorBtn.Location = new Point(
				600, 0);
		ChangeSelectedTabColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeSelectedTabColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeSelectedTabColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeSelectedTabColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeSelectedTabColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeSelectedTabColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeSelectedTabColorBtn);

		//Change TabBar Color
		Button ChangeTabBarColorBtn = new Button();
		ChangeTabBarColorBtn.Text = "Change TabBar Color";
		ChangeTabBarColorBtn.Name = "ChangeTabBarColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeTabBarColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeTabBarColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeTabBarColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeTabBarColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeTabBarColorBtn.Location = new Point(
				900, 0);
		ChangeTabBarColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeTabBarColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeTabBarColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeTabBarColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeTabBarColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeTabBarColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeTabBarColorBtn);

		//Change ControlBar Color
		Button ChangeControlBarColor = new Button();
		ChangeControlBarColor.Text = "Change Control Bar Color";
		ChangeControlBarColor.Name = "ChangeControlBarColor";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeControlBarColor.Width = Convert.ToInt32(width * .2);
		ChangeControlBarColor.Height = Convert.ToInt32(height * .1);
		ChangeControlBarColor.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeControlBarColor.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeControlBarColor.Location = new Point(
				0, 200);
		ChangeControlBarColor.FlatStyle = FlatStyle.Flat;
		ChangeControlBarColor.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeControlBarColor.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeControlBarColor.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeControlBarColor.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeControlBarColor.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeControlBarColor);

		//Change Inactive Tab Color
		Button ChangeInactiveTabColorBtn = new Button();
		ChangeInactiveTabColorBtn.Text = "Change Inactive Tab Color";
		ChangeInactiveTabColorBtn.Name = "ChangeInactiveTabColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeInactiveTabColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeInactiveTabColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeInactiveTabColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeInactiveTabColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeInactiveTabColorBtn.Location = new Point(
				300, 200);
		ChangeInactiveTabColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeInactiveTabColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeInactiveTabColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeInactiveTabColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeInactiveTabColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeInactiveTabColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeInactiveTabColorBtn);

		//Change Tab Color on Mouse Over
		Button ChangeTabColorMouseOverBtn = new Button();
		ChangeTabColorMouseOverBtn.Text = "Change Tab Color on Mouse Over";
		ChangeTabColorMouseOverBtn.Name = "ChangeTabColorMouseOverBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeTabColorMouseOverBtn.Width = Convert.ToInt32(width * .2);
		ChangeTabColorMouseOverBtn.Height = Convert.ToInt32(height * .1);
		ChangeTabColorMouseOverBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeTabColorMouseOverBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeTabColorMouseOverBtn.Location = new Point(
				600, 200);
		ChangeTabColorMouseOverBtn.FlatStyle = FlatStyle.Flat;
		ChangeTabColorMouseOverBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeTabColorMouseOverBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeTabColorMouseOverBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeTabColorMouseOverBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeTabColorMouseOverBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeTabColorMouseOverBtn);

		//Change Inner border Color
		Button ChangeInnerBorderColorBtn = new Button();
		ChangeInnerBorderColorBtn.Text = "Change Inner Border Color";
		ChangeInnerBorderColorBtn.Name = "ChangeInnerBorderColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeInnerBorderColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeInnerBorderColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeInnerBorderColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeInnerBorderColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeInnerBorderColorBtn.Location = new Point(
				900, 200);
		ChangeInnerBorderColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeInnerBorderColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeInnerBorderColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeInnerBorderColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeInnerBorderColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeInnerBorderColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeInnerBorderColorBtn);

		//Change Selected Tab Text Color
		Button ChangeSelectedTabTextColorBtn = new Button();
		ChangeSelectedTabTextColorBtn.Text = "Change Selected Tab Text Color";
		ChangeSelectedTabTextColorBtn.Name = "ChangeSelectedTabTextColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeSelectedTabTextColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeSelectedTabTextColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeSelectedTabTextColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeSelectedTabTextColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeSelectedTabTextColorBtn.Location = new Point(
				0, 400);
		ChangeSelectedTabTextColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeSelectedTabTextColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeSelectedTabTextColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeSelectedTabTextColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeSelectedTabTextColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeSelectedTabTextColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeSelectedTabTextColorBtn);
		//Change Inactive Tab Text Color
		Button ChangeInactiveTabTextColorBtn = new Button();
		ChangeInactiveTabTextColorBtn.Text = "Change Inactive Tab Text Color";
		ChangeInactiveTabTextColorBtn.Name = "ChangeInactiveTabTextColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeInactiveTabTextColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeInactiveTabTextColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeInactiveTabTextColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeInactiveTabTextColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeInactiveTabTextColorBtn.Location = new Point(
				300, 400);
		ChangeInactiveTabTextColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeInactiveTabTextColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeInactiveTabTextColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeInactiveTabTextColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeInactiveTabTextColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeInactiveTabTextColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeInactiveTabTextColorBtn);

		//Change MouseOver Tab Text Color
		Button ChangeMouseOverTabTextColorBtn = new Button();
		ChangeMouseOverTabTextColorBtn.Text = "Change MouseOver Tab Text Color";
		ChangeMouseOverTabTextColorBtn.Name = "ChangeMouseOverTabTextColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeMouseOverTabTextColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeMouseOverTabTextColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeMouseOverTabTextColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeMouseOverTabTextColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeMouseOverTabTextColorBtn.Location = new Point(
				600, 400);
		ChangeMouseOverTabTextColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeMouseOverTabTextColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeMouseOverTabTextColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeMouseOverTabTextColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeMouseOverTabTextColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeMouseOverTabTextColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeMouseOverTabTextColorBtn);

		//Change Context Menu Back Color
		Button ChangeContextMenuBackColorBtn = new Button();
		ChangeContextMenuBackColorBtn.Text = "Change Context Menu Back Color";
		ChangeContextMenuBackColorBtn.Name = "ChangeContextMenuBackColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeContextMenuBackColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeContextMenuBackColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeContextMenuBackColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeContextMenuBackColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeContextMenuBackColorBtn.Location = new Point(
				0, 600);
		ChangeContextMenuBackColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeContextMenuBackColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeContextMenuBackColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeContextMenuBackColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeContextMenuBackColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeContextMenuBackColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeContextMenuBackColorBtn);

		//Change Context Menu Fore Color
		Button ChangeContextMenuForeColorBtn = new Button();
		ChangeContextMenuForeColorBtn.Text = "Change Context Menu Fore Color";
		ChangeContextMenuForeColorBtn.Name = "ChangeContextMenuForeColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeContextMenuForeColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeContextMenuForeColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeContextMenuForeColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeContextMenuForeColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeContextMenuForeColorBtn.Location = new Point(
				300, 600);
		ChangeContextMenuForeColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeContextMenuForeColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeContextMenuForeColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeContextMenuForeColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeContextMenuForeColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeContextMenuForeColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeContextMenuForeColorBtn);

		//ChangeContextMenuOverColorBtn
		Button ChangeContextMenuOverColorBtn = new Button();
		ChangeContextMenuOverColorBtn.Text = "Change Context Menu Over Color";
		ChangeContextMenuOverColorBtn.Name = "ChangeContextMenuOverColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeContextMenuOverColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeContextMenuOverColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeContextMenuOverColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeContextMenuOverColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeContextMenuOverColorBtn.Location = new Point(
				600, 600);
		ChangeContextMenuOverColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeContextMenuOverColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeContextMenuOverColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeContextMenuOverColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeContextMenuOverColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeContextMenuOverColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeContextMenuOverColorBtn);

		//Change ToolBar Top Color
		Button ChangeToolBarTopColorBtn = new Button();
		ChangeToolBarTopColorBtn.Text = "Change ToolBar Top Color";
		ChangeToolBarTopColorBtn.Name = "ChangeToolBarTopColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeToolBarTopColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeToolBarTopColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeToolBarTopColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeToolBarTopColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeToolBarTopColorBtn.Location = new Point(
				0, 800);
		ChangeToolBarTopColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeToolBarTopColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeToolBarTopColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeToolBarTopColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeToolBarTopColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeToolBarTopColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeToolBarTopColorBtn);

		//Change ToolBar Bottom Color
		Button ChangeToolBarBottomColorBtn = new Button();
		ChangeToolBarBottomColorBtn.Text = "Change ToolBar Bottom Color";
		ChangeToolBarBottomColorBtn.Name = "ChangeToolBarBottomColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeToolBarBottomColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeToolBarBottomColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeToolBarBottomColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeToolBarBottomColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeToolBarBottomColorBtn.Location = new Point(
				300, 800);
		ChangeToolBarBottomColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeToolBarBottomColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeToolBarBottomColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeToolBarBottomColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeToolBarBottomColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeToolBarBottomColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeToolBarBottomColorBtn);

		//Change ToolBar Border Color
		Button ChangeToolBarBorderColorBtn = new Button();
		ChangeToolBarBorderColorBtn.Text = "Change ToolBar Border Color";
		ChangeToolBarBorderColorBtn.Name = "ChangeToolBarBorderColorBtn";
		//ChangeGameManagerBorderBtn.Font = font;
		ChangeToolBarBorderColorBtn.Width = Convert.ToInt32(width * .2);
		ChangeToolBarBorderColorBtn.Height = Convert.ToInt32(height * .1);
		ChangeToolBarBorderColorBtn.BackColor = ColorTranslator.FromHtml("#84d2e4");
		ChangeToolBarBorderColorBtn.ForeColor = ColorTranslator.FromHtml("#ffffff");
		ChangeToolBarBorderColorBtn.Location = new Point(
				600, 800);
		ChangeToolBarBorderColorBtn.FlatStyle = FlatStyle.Flat;
		ChangeToolBarBorderColorBtn.MouseEnter += new EventHandler(this.ButtonMouseEnter);
		ChangeToolBarBorderColorBtn.MouseDown += new MouseEventHandler(this.ButtonMouseDown);
		ChangeToolBarBorderColorBtn.MouseUp += new MouseEventHandler(this.ButtonMouseUp);
		ChangeToolBarBorderColorBtn.MouseLeave += new EventHandler(this.ButtonMouseLeave);
		ChangeToolBarBorderColorBtn.Click += new System.EventHandler(this.ChangeGameManagerBorderBtn_Click);
		mChangeThemeForm.Controls.Add(ChangeToolBarBorderColorBtn);
	}
}	
