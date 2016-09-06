using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using System.Windows;

namespace BlueStacks.hyperDroid.GameManager
{
	class Preferences
	{

		public static  Size NewGMSize = new Size();

		public enum AndroidDPIEnum
		{
			DPI_LOW = 0,
			DPI_MEDIUM,
			DPI_HIGH
		}

		public enum EngineTypeEnum
		{
			ENGINE_TYPE_LEGACY = 0,
			ENGINE_TYPE_RAW,
			ENGINE_TYPE_PLUS
		}

		public static int ReadFromRegistry(string option)
		{
			int check=-1;
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPreferencesPath);
				check = (int)key.GetValue(option);

			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}
			return check;
		}
		public static int ReadFromBluestacksRegistry(string option)
		{
			int check=-1;
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				check = (int)key.GetValue(option);

			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}
			return check;
		}
		public static void WriteToBluestacksRegistry(string option,int value)
		{
			
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath,true);
				key.SetValue(option,value,RegistryValueKind.DWord);

			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}
			
		}
		public static void WriteToregistry(string option, int value)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPreferencesPath, true);
				key.SetValue(option, value, RegistryValueKind.DWord);

			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}
		}
		public static string ReadAndroidRegistry(string option)
		{
			string GuestParameters = null;
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.AndroidKeyBasePath);
				GuestParameters = (string)key.GetValue(option);
			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}
			return GuestParameters;
		}
		public static void WriteAndroidRegistry(string option, string value)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.AndroidKeyBasePath, true);
				key.SetValue(option, value);
			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}

		}
		public static int ReadFrameBufferRegistry(string option)
		{
			int GuestParameters = -1;
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format("{0}\\FrameBuffer\\0", Common.Strings.AndroidKeyBasePath));
				GuestParameters = (int)key.GetValue(option);
			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}
			return GuestParameters;
		}
		public static void WriteFrameBufferRegistry(string option, int value)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(string.Format("{0}\\FrameBuffer\\0",Common.Strings.AndroidKeyBasePath), true);
				key.SetValue(option, value);
			}
			catch (Exception ex)
			{
				Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
			}

		}

        public static void WriteLanguageRegistry(string key, string value)
        {
            try
            {
                RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath, true);
                regKey.SetValue(key, value);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
            }
        }

        public static void WriteGMSizeRegistry(string option, int value)
        {
            try
            {
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
                key.SetValue(option, value);
            }
            catch (Exception ex)
            {
                Logger.Error("There was an error in setting default preferences registry... Err : " + ex.ToString());
            }
        }
	}
}

