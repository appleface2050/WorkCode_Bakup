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
using System.IO;
using System.Text.RegularExpressions;
using BlueStacks.hyperDroid.Tool;
using System.Diagnostics;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for SystemSettings.xaml
	/// </summary>
	public partial class SystemSettings : UserControl
	{
		public static SystemSettings Instance = null;
		public SystemSettings()
		{
			Instance = this;
			InitializeComponent();
			this.Loaded += SystemSettings_Loaded;
			SetControlProoperties();
		}

		void SystemSettings_Loaded(object sender, RoutedEventArgs e)
		{
			PopulateCurrentPreferences();
		}
		private void SetControlProoperties()
		{
			mRamSlider.Label=string.Format("*{0}",Locale.Strings.GetLocalizedString("RamSliderText"));
			//mDiskSlider.Label = string.Format("*{0}",Locale.Strings.GetLocalizedString("DiskSliderText"));
			mCPUSlider.Label = string.Format("*{0}",Locale.Strings.GetLocalizedString("CPUSliderText"));
			mlblAndroidDpi.Content=string.Format("*{0}",Locale.Strings.GetLocalizedString("AndroidDPI"));
			mLowDPI.Content=Locale.Strings.GetLocalizedString("LowButtonText");
			mHighDPI.Content = Locale.Strings.GetLocalizedString("HighButtonText");
		}
		private void PopulateCurrentPreferences()
		{
			try
			{
				int androidDPI = Preferences.ReadFromRegistry("AndroidDPI");
				if (androidDPI == (int)Preferences.AndroidDPIEnum.DPI_LOW)
				{
					mLowDPI.IsSelected = true;
				}
				else
				{
					mHighDPI.IsSelected = true;
				}
				PopulateRAMSlider();
				PopulateCPUSlider();

				//RegistryKey regBaseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				//string engineMode = (string)regBaseKey.GetValue("Engine");
				//if (engineMode == "plus")
				//{
				//	mPlusEngine.IsSelected = true;
				//}
				//else if (engineMode == "legacy")
				//{
				//	mLegacyEngine.IsSelected = true;
				//}
				//else if(engineMode=="raw")
				//{
				//	mAutoEngine.IsSelected = true;
				//}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}
		private void PopulateRAMSlider()
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.AndroidKeyBasePath);
				int memoryValue = (int)key.GetValue("Memory");
				mRamSlider.Value = memoryValue;
				int systemRAM;
				bool check = Int32.TryParse(Device.Profile.RAM, out systemRAM);
				mRamSlider.Maximum = (int)(systemRAM * (.5));
				if(mRamSlider.Maximum>4096)
				{
					mRamSlider.Maximum = 4096;
				}
				key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				if (Common.Strings.IsEngineLegacy())
				{
					mRamSlider.IsEnabled = false;
				}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}
		private void PopulateCPUSlider()
		{
			int CPU = Preferences.ReadFromRegistry("VCPUs");
			mCPUSlider.Maximum = Environment.ProcessorCount;
			mCPUSlider.Value = CPU;
			if (Common.Strings.IsEngineLegacy())
			{
				mCPUSlider.IsEnabled= false;
			}
		}

		private void HighDPI_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			string bootparameterString = Preferences.ReadAndroidRegistry("BootParameters");
			if (mHighDPI.IsSelected == false)
			{
				Preferences.WriteToregistry("AndroidDPI", (int)Preferences.AndroidDPIEnum.DPI_HIGH);
				SetDPIParameters(bootparameterString, "240");
				mHighDPI.IsSelected = true;
			}
		}


		private void LowDPI_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			string bootparameterString = Preferences.ReadAndroidRegistry("BootParameters");
			if (mLowDPI.IsSelected == false)
			{
				Preferences.WriteToregistry("AndroidDPI", (int)Preferences.AndroidDPIEnum.DPI_LOW);
				SetDPIParameters(bootparameterString, "160");
				mLowDPI.IsSelected = true;
			}
		}

		//private void mAutoEngine_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		//{
		//	string bootparameters = Preferences.ReadAndroidRegistry("BootParameters");
		//	if (mAutoEngine.IsSelected == false)
		//	{
		//		Preferences.WriteToBluestacksRegistry("Engine", 1);
		//		SetEngineMode(bootparameters, "1");
		//	}
		//}

		//private void mPlusEngine_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		//{
		//	string bootparameters = Preferences.ReadAndroidRegistry("BootParameters");
		//	if (mPlusEngine.IsSelected == false)
		//	{
		//		Preferences.WriteToBluestacksRegistry("Engine", 2);
		//		SetEngineMode(bootparameters, "3");
		//	}
		//}

		//private void mLegacyEngine_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		//{
		//	string bootparameters = Preferences.ReadAndroidRegistry("BootParameters");
		//	if (mLegacyEngine.IsSelected == false)
		//	{
		//		Preferences.WriteToBluestacksRegistry("Engine", 0);
		//		SetEngineMode(bootparameters, "0");
		//	}
		//}
		private void SetDPIParameters(string bootParameterString, string updatedValue)
		{
			string[] bootParameters = bootParameterString.Split(' ');
			string dpi = null;
			foreach (string androidParameters in bootParameters)
			{
				if (androidParameters.StartsWith("DPI=", StringComparison.OrdinalIgnoreCase))
				{
					dpi = androidParameters.Split('=')[0];
					string dpiValue = androidParameters.Split('=')[1];
					if (dpiValue != updatedValue)
					{
						string replaceParameter = string.Format("DPI={0}", updatedValue);
						string previousParameter = string.Format("DPI={0}", dpiValue);
						string newBootParameterValue = bootParameterString.Replace(previousParameter, replaceParameter);
						Preferences.WriteAndroidRegistry("BootParameters", newBootParameterValue);
					}
				}
			}
			if (dpi == null)
			{
				string appendParameter = string.Format("DPI={0}", updatedValue);
				string newBootParameterValue = string.Format("{0} {1}", bootParameterString, appendParameter);
				Preferences.WriteAndroidRegistry("BootParameters", newBootParameterValue);
			}
		}


		//private void SetEngineMode(string bootParameterString , string updatedValue)
		//{
		//	string glTransport=null;
		//	string[] bootParameters = bootParameterString.Split(' ');
		//	foreach (string androidParameters in bootParameters)
		//	{
		//		if(androidParameters.StartsWith("GlTransport=", StringComparison.OrdinalIgnoreCase))
		//		{
		//			glTransport=androidParameters.Split('=')[0];
		//			string engineMode= androidParameters.Split('=')[1];
		//			if (engineMode != updatedValue)
		//			{
		//				string replaceParameter =string.Format("GlTransport={0}", updatedValue);
		//				string previousParameter = string.Format("GlTransport={0}", engineMode);
		//				string correctParameter = bootParameterString.Replace(previousParameter, replaceParameter);
		//			}
		//		}
		//	}
		//	if(glTransport==null)
		//	{
		//		string appendParameter = string.Format("GlTransport={0}", updatedValue);
		//		string newBootParameterValue = string.Format("{0} {1}",bootParameterString, appendParameter);
		//		Preferences.WriteAndroidRegistry("BootParameters", newBootParameterValue);
		//	}
		//}
		private void RamSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			try
			{
				RegistryKey androidkey = Registry.LocalMachine.OpenSubKey(Common.Strings.AndroidKeyBasePath, true);
				int currentRam = (int)androidkey.GetValue("Memory");
				if ((int)mRamSlider.Value != currentRam)
				{
					SetRam((int)(mRamSlider.Value), currentRam);
				}
			}
			catch (Exception ex)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", ex.ToString()));
			}
		}
		private void CPUSlider_PreviewMouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			int currentCPU=Preferences.ReadFromRegistry("VCPUs");
			if((int)(mCPUSlider.Value)!=currentCPU)
			{
				SetCPU((int)(mCPUSlider.Value), currentCPU);
			}
		}
		private void SetCPU(int newCPU, int currentCPU)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath, true);
				System.Windows.Forms.DialogResult res = CustomMessageBox.ShowMessageBox(GameManagerWindow.Instance, "Bluestacks",
						Locale.Strings.GetLocalizedString("QuitBluestacksMessageToResetCPU"),
						Locale.Strings.GetLocalizedString("YesText"),
						Locale.Strings.GetLocalizedString("NoText"),
						Locale.Strings.GetLocalizedString("CancelText"),
						Locale.Strings.GetLocalizedString("RememberChoiceText"),
						false);

				if (res == System.Windows.Forms.DialogResult.Yes)
				{
					key.SetValue("VCPUs", (int)(mCPUSlider.Value), RegistryValueKind.DWord);
					Preferences.WriteToregistry("VCPUs", (int)(mCPUSlider.Value));
					QuitBlueStacks();
				}
				if (res == System.Windows.Forms.DialogResult.No || res == System.Windows.Forms.DialogResult.Cancel)
				{
					mCPUSlider.Value = currentCPU;
				}
			}
			catch (Exception ex)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", ex.ToString()));
			}
		}
		private void SetRam(int newSize,int currentRam)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				RegistryKey androidkey = Registry.LocalMachine.OpenSubKey(Common.Strings.AndroidKeyBasePath, true);
				String dataDir = (String)key.GetValue("DataDir");
				String androidBstkPath = System.IO.Path.Combine(dataDir, @"Android\Android.bstk");
				String tempBsktPath = androidBstkPath + ".tmp";
				if (File.Exists(androidBstkPath))
				{
					string allText = File.ReadAllText(androidBstkPath);
					string pattern = "Memory RAMSize=\\\"\\d*\\\"";
					string replacement = String.Format("Memory RAMSize=\"{0}\"", newSize);
					string replacedText = Regex.Replace(allText, pattern, replacement);
					File.WriteAllText(tempBsktPath, replacedText);
					System.Windows.Forms.DialogResult res = CustomMessageBox.ShowMessageBox(GameManagerWindow.Instance, "Bluestacks",
						Locale.Strings.GetLocalizedString("QuitBluestacksMessageToResetRAM"),
						Locale.Strings.GetLocalizedString("YesText"),
						Locale.Strings.GetLocalizedString("NoText"),
						Locale.Strings.GetLocalizedString("CancelText"),
						Locale.Strings.GetLocalizedString("RememberChoiceText"),
						false);
					if (res == System.Windows.Forms.DialogResult.Yes)
					{
						androidkey.SetValue("Memory", (int)mRamSlider.Value, RegistryValueKind.DWord);
						QuitBlueStacks();
					}
					if (res == System.Windows.Forms.DialogResult.No || res == System.Windows.Forms.DialogResult.Cancel)
					{
						mRamSlider.Value = currentRam;
					}
				}
				else
				{
					Logger.Error("Bstk file path does not exist");
				}
			}
			catch (Exception ex)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", ex.ToString()));
			}

		}
		public static void QuitBlueStacks()
		{
			Logger.Info("Quit bluestacks called");
			RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)regKey.GetValue("InstallDir");

			string quitExePath = System.IO.Path.Combine(installDir, "HD-Quit.exe");

			Logger.Info("quit exe path -- " + quitExePath);
			Process proc = new Process();
			proc.StartInfo.FileName = quitExePath;
			proc.Start();
		}
	}
}

