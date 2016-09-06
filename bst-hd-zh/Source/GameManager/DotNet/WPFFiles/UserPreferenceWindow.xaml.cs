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
using System.Windows.Shapes;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for UserPreferenceWindow.xaml
	/// </summary>
	public partial class UserPreferenceWindow : Window
	{
		UserControl visibleControl = null;
		public UserPreferenceWindow()
		{
			InitializeComponent();
			BringtoFront(GeneralSettings.Instance);
			Width = GameManagerWindow.Instance.ActualWidth * .80;
			Height = GameManagerWindow.Instance.ActualHeight * .80;
			SetControlProperties();
		}
		private void SetControlProperties()
		{
			mbtnDataSettings.Content = Locale.Strings.GetLocalizedString("DataButtonText");
			mbtnGeneralSettings.Content = Locale.Strings.GetLocalizedString("GeneralButtonText");
			mbtnSystemSettings.Content = Locale.Strings.GetLocalizedString("SystemButtonText");
			mLblApplicationRestartMessage.Content = string.Format("*{0}",Locale.Strings.GetLocalizedString("ChangesRequireApplicationRestartMessage"));
			mLblBluestacksPreferences.Content = Locale.Strings.GetLocalizedString("BlueStacksPreferencesWindowHeader");
			mCloseButton.Content = Locale.Strings.GetLocalizedString("CloseButtonText");
		}

		public static void SetDefaultRegistry()
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPreferencesPath);
				if (key == null)
				{
					RegistryKey preferencesKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMPreferencesPath);
					preferencesKey.SetValue("HideBTVWindow", 0, RegistryValueKind.DWord);
					preferencesKey.SetValue("Hidetabs", 1, RegistryValueKind.DWord);
					preferencesKey.SetValue("ShowLeftToolBar", 1, RegistryValueKind.DWord);
					preferencesKey.SetValue("BlueStacksCloseWarning", 1, RegistryValueKind.DWord);
					preferencesKey.SetValue("AndroidDPI", (int)Preferences.AndroidDPIEnum.DPI_LOW, RegistryValueKind.DWord);
					preferencesKey.SetValue("VCPUs", 2, RegistryValueKind.DWord);

				}
			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}
		}



		private void GeneralButton_Click(object sender, RoutedEventArgs e)
		{
			BringtoFront(GeneralSettings.Instance);
		}

		private void BringtoFront(UserControl control)
		{
			if (visibleControl != null)
			{
				visibleControl.Visibility = Visibility.Hidden;
			}
			control.Visibility = Visibility.Visible;
			visibleControl = control;
		}

		private void DataButton_Click(object sender, RoutedEventArgs e)
		{
			BringtoFront(DataSettingsControl.Instance);
		}

		private void SystemButton_Click(object sender, RoutedEventArgs e)
		{
			BringtoFront(SystemSettings.Instance);
		}

		private void CustomPictureBox_MouseDown(object sender, MouseButtonEventArgs e)
		{
			this.Close();
		}


		private void CloseButton_Click(object sender, RoutedEventArgs e)
		{
			this.Close();
		}

		private void Close_MouseUp(object sender, MouseButtonEventArgs e)
		{
			CloseButton_Click(null, null);
		}
		private void Close_MouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void TitleBar_MouseDown(object sender, MouseButtonEventArgs e)
		{
			try { this.DragMove(); }
			catch (Exception ex) { }
		}
	}
}
