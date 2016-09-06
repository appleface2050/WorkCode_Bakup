using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BlueStacks.hyperDroid.Common
{
	public static class WpfUtils
	{
		public static void SetWindowSizeAndLocation(Window window, string prefix, bool isGMWindow = false)
		{
			try
			{
				double aspectRatio = (double)16 / 9;
				if (Oem.Instance.IsStreamWindowEnabled || !isGMWindow)
				{
					aspectRatio = (double)64 / 27;
				}
				bool isCustomCalculate = true;
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
				if ((int)key.GetValue(prefix + "Width", int.MinValue) != int.MinValue)
				{
					try
					{
						window.Width = (int)key.GetValue(prefix + "Width");
						window.Height = (int)key.GetValue(prefix + "Height");

						RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
						window.Left = (int)configKey.GetValue(prefix + "Left");
						window.Top = (int)configKey.GetValue(prefix + "Top");
						isCustomCalculate = false;
						if (!IsParametersValid(window))
						{
							isCustomCalculate = true;
						}
					}
					catch (Exception ex)
					{
						Logger.Info("Exception in geting value from reg" + ex.ToString());
						isCustomCalculate = true;
					}
				}
				if (isCustomCalculate)
				{
					int defaultWidth = int.MinValue;
					if (SystemParameters.PrimaryScreenWidth * 0.90 / aspectRatio <= SystemParameters.PrimaryScreenHeight * 0.90)
					{
						defaultWidth = (int)(SystemParameters.PrimaryScreenWidth * 0.90);
					}
					else
					{
						defaultWidth = (int)(SystemParameters.PrimaryScreenHeight * 0.90 * aspectRatio);
					}

					double GMWidth;
					double GMLeft;
					if (Oem.Instance.IsStreamWindowEnabled || !isGMWindow)
					{
						GMWidth = defaultWidth / 4 * 3;
						GMLeft = (int)(SystemParameters.PrimaryScreenWidth - defaultWidth) / 2;
					}
					else
					{
						GMWidth = defaultWidth;
						GMLeft = (int)(SystemParameters.PrimaryScreenWidth - defaultWidth) / 2;
					}
					if (GMWidth < 912)
					{
						GMWidth = 912;
						GMLeft = 20;
					}

					double GMHeight = (int)GMWidth / 16 * 9;
					double GMRight = GMLeft + GMWidth;
					double GMTop = (int)(SystemParameters.PrimaryScreenHeight - GMHeight) / 2;

					if (isGMWindow)
					{
						window.Left = GMLeft;
						window.Top = GMTop;
						window.Height = GMHeight;
						window.Width = GMWidth;
						SaveControlSize(GMWidth, GMHeight, "DefaultGM");
					}
					else
					{
						window.Left = GMRight;
						window.Top = GMTop;
						window.Height = GMHeight;
						window.Width = (window.Height - 33) / 27 * 16;
						if ((window.Left + window.Width) > SystemParameters.PrimaryScreenWidth)
						{
							window.Left = SystemParameters.PrimaryScreenWidth - window.Width - 20;
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Info("Exception getting size" + ex.ToString());
			}
		}

		private static bool IsParametersValid(Window window)
		{
			try
			{
				//check if window is out of bounds
				if (window.Left < 0 || window.Left > SystemParameters.VirtualScreenWidth || window.Top < 0 || window.Top > SystemParameters.VirtualScreenHeight)
				{
					return false;
				}

				//check if the window visible is far too hidden
				if ((SystemParameters.VirtualScreenWidth - window.Left) < (window.Width / 10)
					|| (SystemParameters.VirtualScreenHeight - window.Top) < (window.Height / 10))
				{
					return false;
				}

				//Check if window size resolution is changed
				RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
				int width = (int)configKey.GetValue("ScreenWidth");
				int height = (int)configKey.GetValue("ScreenHeight");
				if (Math.Abs(width - SystemParameters.VirtualScreenWidth) > 100 || Math.Abs(height - SystemParameters.VirtualScreenHeight) > 100)
				{
					return false;
				}
			}
			catch (Exception ex)
			{
				Logger.Info("Exception calculating size" + ex.ToString());
				return false;
			}
			return true;
		}

		public static void SaveWindowSizeAndLocation(Window window, string prefix)
		{
			try
			{
				if (window.WindowState == WindowState.Normal)
				{
					if (window != null)
					{
						SaveControlSize(window.ActualWidth, window.ActualHeight, prefix);
					}
				}
				if (window.Left >= 0 && window.Left <= SystemParameters.VirtualScreenWidth &&
				   window.Top >= 0 && window.Top <= SystemParameters.VirtualScreenHeight)
				{
					RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
					configKey.SetValue(prefix + "Left", window.Left, RegistryValueKind.DWord);
					configKey.SetValue(prefix + "Top", window.Top, RegistryValueKind.DWord);
					configKey.Close();
				}
			}
			catch (Exception ex)
			{
				Logger.Info("Exception saving size" + ex.ToString());
			}
		}

		private static void SaveControlSize(double width, double height, string prefix)
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
			key.SetValue(prefix + "Width", width, RegistryValueKind.DWord);
			key.SetValue(prefix + "Height", height, RegistryValueKind.DWord);
			key.Close();
		}

		public static void SaveControlSize(UserControl control, string prefix)
		{
			if (control != null)
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.FrameBufferRegKeyPath);
				key.SetValue(prefix + "Width", (int)control.ActualWidth, RegistryValueKind.DWord);
				key.SetValue(prefix + "Height", (int)control.ActualHeight, RegistryValueKind.DWord);
				key.Close();

				RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
				configKey.SetValue("ScreenWidth", (int)SystemParameters.VirtualScreenWidth, RegistryValueKind.DWord);
				configKey.SetValue("ScreenHeight", (int)SystemParameters.VirtualScreenHeight, RegistryValueKind.DWord);
				configKey.Close();
			}

		}
	}
}
