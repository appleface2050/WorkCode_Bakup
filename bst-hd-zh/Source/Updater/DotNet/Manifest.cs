using System;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Updater {
public static class Manifest {
	/*
	   [updaterMgr]
	   version  = 1
	   [latest]
	   version = 0.9.1.5
	   md5 = 42097dae96469c69eeca4cd7a50e1524 
	   sha1 = b901852076986017413856150af96ec6ce60ffdf 
	   size = 204130816 
	   url = http://cdn.bluestacks.com/downloads/BlueStacks_HD_setup_0.9.1.5_REL.msi
	*/
	private static string REG_PATH = Common.Strings.HKLMManifestRegKeyPath;

	public static string Version {
		get { 
			RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH);
			s_Version = (String)key.GetValue("Version");
			return s_Version;
		}
		set { 
			s_Version = value;
			RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH);
			key.SetValue("Version", s_Version, RegistryValueKind.String);
		}
	}

	public static string MD5 {
		get { 
			RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_PATH);
			s_MD5 = (String)key.GetValue("MD5");
			return s_MD5;
		}
		set { 
			s_MD5 = value;
			RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH);
			key.SetValue("MD5", s_MD5, RegistryValueKind.String);
		}
	}

	public static string SHA1 {
		get { 
			RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_PATH);
			s_SHA1 = (String)key.GetValue("SHA1");
			return s_SHA1;
		}
		set { 
			s_SHA1 = value;
			RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH);
			key.SetValue("SHA1", s_SHA1, RegistryValueKind.String);
		}
	}

	public static string Size {
		get { 
			RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_PATH);
			s_Size = (String)key.GetValue("Size");
			return s_Size;
		}
		set { 
			s_Size = value;
			RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH);
			key.SetValue("Size", s_Size, RegistryValueKind.String);
		}
	}

	public static string URL {
		get { 
			RegistryKey key = Registry.LocalMachine.OpenSubKey(REG_PATH);
			s_URL = (String)key.GetValue("URL");
			return s_URL;
		}
		set { 
			s_URL = value;
			RegistryKey key = Registry.LocalMachine.CreateSubKey(REG_PATH);
			key.SetValue("URL", s_URL, RegistryValueKind.String);
		}
	}

	private static string s_Version;
	private static string s_MD5;
	private static string s_SHA1;
	private static string s_Size;
	private static string s_URL;
}
}
