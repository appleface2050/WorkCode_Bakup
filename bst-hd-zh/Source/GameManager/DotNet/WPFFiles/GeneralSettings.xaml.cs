using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common.Interop;
using System.IO;
using System.Globalization;
using System.Threading;
using System.ComponentModel;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for GeneralSettings.xaml
	/// </summary>
	public partial class GeneralSettings : UserControl
	{
		bool inintalized = false;
		public static GeneralSettings Instance = null;
		Dictionary<string, string> dictLocale = new Dictionary<string, string>();

		public GeneralSettings()
		{
			Instance = this;
			InitializeComponent();
			this.Loaded += GeneralSettings_Loaded;
			SetControlProperties();
		}

		void GeneralSettings_Loaded(object sender, RoutedEventArgs e)
		{
			PopulateCurrentPreferences();
		}

		private void SetControlProperties()
		{
			mlblLanguage.Content = string.Format("*{0}", Locale.Strings.GetLocalizedString("LanguageText"));
			mlblResolution.Content = Locale.Strings.GetLocalizedString("ResolutionText");
			mHideBTVWindow.Content = string.Format("*{0}", Locale.Strings.GetLocalizedString("HideBluestacksTVAtLaunch"));
			mHideTabs.Content = string.Format("*{0}", Locale.Strings.GetLocalizedString("HideTabs"));
			mBlueStacksClosewarning.Content = Locale.Strings.GetLocalizedString("BluestacksCloseWarning");
			//mAutomaticUpgrade.Content = Locale.Strings.GetLocalizedString("AutomaticUpgrade");
			mShowLeftToolbar.Content = string.Format("*{0}", Locale.Strings.GetLocalizedString("ShowLeftToolBar"));
			//mlblWorkingMode.Content = Locale.Strings.GetLocalizedString("WorkingModeText");
			//mDesktopMode.Content = Locale.Strings.GetLocalizedString("DesktopButtonText");
			//mTabletMode.Content = Locale.Strings.GetLocalizedString("TabletButtonText");
		}
		public void PopulateCurrentPreferences()
		{
			inintalized = false;
			try
			{
				UpdateResolutionCombo();
				UpdateLanguageCombo();
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPreferencesPath);
				int hideBTVWindow = (int)key.GetValue("HideBTVWindow");
				DisplayCheck(hideBTVWindow, mHideBTVWindow);
				int showLeftToolBar = (int)key.GetValue("ShowLeftToolBar");
				DisplayCheck(showLeftToolBar, mShowLeftToolbar);
				int hideTabs = (int)key.GetValue("Hidetabs");
				DisplayCheck(hideTabs, mHideTabs);
				int blueStacksCloseWarning = (int)key.GetValue("BlueStacksCloseWarning");
				DisplayCheck(blueStacksCloseWarning, mBlueStacksClosewarning);
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
			inintalized = true;
		}



		private void UpdateResolutionCombo()
		{
			string prefix = "GM";
			string size;
			size = Convert.ToInt32(GameManagerWindow.Instance.ActualWidth) + " X " + Convert.ToInt32(GameManagerWindow.Instance.ActualHeight);
			mResolutionMenu.Text = size;
			if (mResolutionMenu.SelectedItem == null)
			{
				CustomComboBoxItem item = new CustomComboBoxItem();
				item.Content = size;
				mResolutionMenu.Items.Add(item);
				mResolutionMenu.SelectedItem = item;
			}

			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			if ((int)key.GetValue("DefaultGMWidth", int.MinValue) != int.MinValue)
			{
				string defaultSize = string.Format("{0} X {1} ({2})", key.GetValue("DefaultGMWidth"), key.GetValue("DefaultGMHeight"), Locale.Strings.GetLocalizedString("Recommended"));
				if (defaultSize.Contains(size))
				{
					((CustomComboBoxItem)mResolutionMenu.SelectedItem).Content = defaultSize;
				}
				else
				{
					CustomComboBoxItem item = new CustomComboBoxItem();
					item.Content = defaultSize;
					mResolutionMenu.Items.Add(item);
				}
			}
			SortDescription sd = new SortDescription("Content", ListSortDirection.Ascending);
			mResolutionMenu.Items.SortDescriptions.Add(sd);
		}


		private void UpdateLanguageCombo()
		{
			foreach (var item in Directory.GetFiles(Locale.Strings.sResourceLocation))
			{
				string s = System.IO.Path.GetFileNameWithoutExtension(item).Replace("i18n.", string.Empty);
				string displayName = s;
				CustomComboBoxItem cmbitem = new CustomComboBoxItem();
				try
				{
					CultureInfo culture = CultureInfo.GetCultureInfo(s);
					displayName = culture.DisplayName;
				}
				catch (Exception EX)
				{
					Logger.Info("LOCALE " + displayName + " DESCRIPTION NOT FOUND " + EX.ToString());
				}
				cmbitem.Content = displayName;
				mLanguageMenu.Items.Add(cmbitem);
				dictLocale.Add(displayName, s);
			}

			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			string localeName = key.GetValue("Locale", string.Empty).ToString();
			if (string.IsNullOrEmpty(localeName))
			{
				CultureInfo ci = Thread.CurrentThread.CurrentCulture;
				localeName = ci.Name;
			}
			if (!dictLocale.ContainsValue(localeName))
			{
				localeName = "en-US";
			}

			mLanguageMenu.Text = CultureInfo.GetCultureInfo(localeName).DisplayName;
			SortDescription sd = new SortDescription("Content", ListSortDirection.Ascending);
			mLanguageMenu.Items.SortDescriptions.Add(sd);
		}

		public void DisplayCheck(int check, CustomCheckbox option)
		{
			if (check == 1)
				option.IsChecked = true;
			else
				option.IsChecked = false;
		}

		private void HideBTVWindow_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			UpdatePreference("HideBTVWindow", mHideBTVWindow);
		}

		private void ShowLeftToolbar_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			UpdatePreference("ShowLeftToolBar", mShowLeftToolbar);
		}

		private void BlueStacksClosewarning_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			UpdatePreference("BlueStacksCloseWarning", mBlueStacksClosewarning);
		}

		//private void AutomaticUpgrade_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		//{
		//	UpdatePreference("AutomaticUpdate", mAutomaticUpgrade);
		//}

		private void HideTabs_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			UpdatePreference("Hidetabs", mHideTabs);
		}

		private void UpdatePreference(String registryName, CustomCheckbox option)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPreferencesPath, true);
				if ((bool)option.IsChecked)
				{
					key.SetValue(registryName, 0);
				}
				else
				{
					key.SetValue(registryName, 1);
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Failed to update pref: {0} to value: {1}. Err: {2}", registryName, (bool)option.IsChecked, e.ToString()));
				// TODO: prompt user that this failed and toggle back the checkbox.
			}
		}
		//private void DesktopMode_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		//{
		//	if (mDesktopMode.IsSelected == false)
		//	{
		//		Preferences.WriteToregistry("IsWorkingModeDesktop", 1);
		//		mDesktopMode.IsSelected = true;
		//	}
		//}

		//private void TabletMode_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		//{
		//	if (mTabletMode.IsSelected == false)
		//	{
		//		Preferences.WriteToregistry("IsWorkingModeDesktop", 0);
		//		mTabletMode.IsSelected = true;
		//	}
		//}

		private void ResolutionMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (inintalized)
			{
				List<string> resolution = ((CustomComboBoxItem)mResolutionMenu.SelectedValue).Content.ToString().Split(new string[] { "X", " ", "((Recommended))" }, StringSplitOptions.RemoveEmptyEntries).ToList();
				Preferences.NewGMSize.Width = Int32.Parse(resolution[0].Trim());
				Preferences.NewGMSize.Height = Int32.Parse(resolution[1].Trim());
			}
		}

		private void LanguageMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
		{
			if (inintalized)
			{
				string selectedValue = ((CustomComboBoxItem)mLanguageMenu.SelectedValue).Content.ToString();
				string language = dictLocale[selectedValue];
				Preferences.WriteLanguageRegistry("Locale", language);

				//send command to bst cmd processor
				string cmd = String.Format("setlocale {0}", language);
				if (VmCmdHandler.RunCommand(cmd) == null)
				{
					Logger.Info("Failed to set locale.");
				}
			}
		}

	}
}
