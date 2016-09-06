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
using System.Diagnostics;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for DataSettingsControl.xaml
	/// </summary>
	public partial class DataSettingsControl : UserControl
	{
		public static DataSettingsControl Instance = null;
		public DataSettingsControl()
		{
			Instance = this;
			InitializeComponent();
			SetControlProperties();
		}
		private void SetControlProperties()
		{
			mBackupLbl.Content = Locale.Strings.GetLocalizedString("TakeBackup");
			mRestoreLbl.Content = string.Format("*{0}",Locale.Strings.GetLocalizedString("RestoreFromBackup"));
			mBackupButton.Content = Locale.Strings.GetLocalizedString("BackupButtonText");
			mRestoreButton.Content = Locale.Strings.GetLocalizedString("RestoreButtonText");
		}
        private void Backup_MouseLeftButtonClick(object sender, MouseButtonEventArgs e)
        {
			System.Windows.Forms.DialogResult res = CustomMessageBox.ShowMessageBox(GameManagerWindow.Instance, "BlueStacks",
					Locale.Strings.GetLocalizedString("BackupButtonClickPromptMessage"),
					Locale.Strings.GetLocalizedString("YesText"),
					Locale.Strings.GetLocalizedString("NoText"),
					Locale.Strings.GetLocalizedString("CancelText"),
					Locale.Strings.GetLocalizedString("RememberChoiceText"),
					false);

			if (res == System.Windows.Forms.DialogResult.Yes)
            {
                LaunchDataManager("backup");
            }
        }

        private void Restore_MouseLeftButtonClick(object sender, MouseButtonEventArgs e)
        {
			System.Windows.Forms.DialogResult res = CustomMessageBox.ShowMessageBox(GameManagerWindow.Instance, "BlueStacks",
					Locale.Strings.GetLocalizedString("RestoreButtonClickPromptMessage"),
					Locale.Strings.GetLocalizedString("YesText"),
					Locale.Strings.GetLocalizedString("NoText"),
					Locale.Strings.GetLocalizedString("CancelText"),
					Locale.Strings.GetLocalizedString("RememberChoiceText"),
					false);

			if (res == System.Windows.Forms.DialogResult.Yes)
			{
				LaunchDataManager("restore");
			}
        }

        private void LaunchDataManager(string type)
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
            string installDir = (string)regKey.GetValue("InstallDir");
            Process proc = new Process();
            proc.StartInfo.FileName = System.IO.Path.Combine(installDir, "HD-DataManager.exe");
            proc.StartInfo.Arguments = type;
            proc.Start();
        }
	}
}
