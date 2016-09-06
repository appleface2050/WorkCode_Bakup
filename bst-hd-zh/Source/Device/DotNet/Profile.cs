using System;
using System.IO;
using System.Management;
using System.Windows.Forms;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using System.Runtime.InteropServices;
using System.Diagnostics;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Device
{

	enum HD_VIRT_TYPE
	{
		HD_VIRT_TYPE_LEGACY = 0,
		HD_VIRT_TYPE_VMX = 1,
		HD_VIRT_TYPE_SVM = 2,
	};

	public class Profile
	{

		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool IsWow64Process(IntPtr proc, ref bool isWow);
		private static Dictionary<String, String> s_Info;
		private static string s_glVendor = "";
		private static string s_glRenderer = "";
		private static string s_glVersion = "";

		public static String OEM
		{
			get
			{
				String keyPath = Common.Strings.HKLMConfigRegKeyPath;
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
				{
					if (key == null)
					{
						string tagFileDir = AppDomain.CurrentDomain.BaseDirectory;
						Logger.Debug("the tag File  directory is " + tagFileDir);
						string tagFilePath = Path.Combine(tagFileDir, "tag.txt");
						if (File.Exists(tagFilePath))
						{
							Logger.Debug("the tag file exists");
							string oemTag = File.ReadAllText(tagFilePath);
							if (oemTag.StartsWith("_"))
								oemTag = oemTag.Substring(1);

							Logger.Debug("using the oem name " + oemTag);
							return oemTag;
						}
						return Oem.Instance.OEM;
					}
					else
					{
						return (string)key.GetValue("OEM", Oem.Instance.OEM);
					}
				}
			}
			set
			{
				String keyPath = Common.Strings.HKLMConfigRegKeyPath;
				using (RegistryKey key = Registry.LocalMachine.CreateSubKey(keyPath))
				{
					key.SetValue("OEM", value);
					key.Flush();
				}
			}
		}

		public static String VirtType
		{
			get
			{
				String virtType = "legacy";
				String keyPath = Common.Strings.AndroidKeyBasePath;
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
				{
					if (key != null)
						virtType = (string)key.GetValue("VirtType", "legacy");
				}
				Logger.Info(keyPath + "\\VirtType = " + virtType);
				return virtType;
			}
		}

		public static String GlVendor
		{
			get { return s_glVendor; }
			set { s_glVendor = value; }
		}

		public static String GlRenderer
		{
			get { return s_glRenderer; }
			set { s_glRenderer = value; }
		}

		public static String GlVersion
		{
			get { return s_glVersion; }
			set { s_glVersion = value; }
		}

		private static bool IsOs64Bit()
		{
			if (IntPtr.Size == 8 || (IntPtr.Size == 4 && Is32BitProcessOn64BitProcessor()))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		private static bool Is32BitProcessOn64BitProcessor()
		{
			bool retVal = false;
			IsWow64Process(Process.GetCurrentProcess().Handle, ref retVal);
			return retVal;
		}

		public static Dictionary<string, string> Info()
		{
			if (s_Info != null)
				return s_Info;

			Dictionary<String, String> info = new Dictionary<string, string>();
			info.Add("ProcessorId", GetSysInfo("Select processorID from Win32_Processor"));
			info.Add("Processor", CPU);
			string numOfProcessors = GetSysInfo("Select NumberOfLogicalProcessors from Win32_Processor");
			Logger.Info("the length of numOfProcessor string is {0}", numOfProcessors.Length.ToString());
			info.Add("NumberOfProcessors", numOfProcessors);
			info.Add("GPU", GPU);
			info.Add("GPUDriver", GetSysInfo("Select DriverVersion from Win32_VideoController"));
			info.Add("OS", OS);
			string osVersion = String.Format("{0}.{1}",
					Environment.OSVersion.Version.Major, Environment.OSVersion.Version.Minor);
			info.Add("OSVersion", osVersion);
			info.Add("RAM", RAM);

			RegistryKey key;
			string bstVersion;
			try
			{
				key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				bstVersion = (string)key.GetValue("Version");
				info.Add("BlueStacksVersion", bstVersion);
			}
			catch
			{
				bstVersion = "";
			}

			int glMode;
			try
			{
				String keyPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
				key = Registry.LocalMachine.OpenSubKey(keyPath);
				glMode = (int)key.GetValue("GlMode");
			}
			catch
			{
				glMode = -1;
			}
			info.Add("GlMode", glMode.ToString());

			int glRenderMode;
			try
			{
				String keyPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
				key = Registry.LocalMachine.OpenSubKey(keyPath);
				glRenderMode = (int)key.GetValue("GlRenderMode");
			}
			catch
			{
				glRenderMode = -1;
			}
			info.Add("GlRenderMode", glRenderMode.ToString());

			string oemInfo = "";
			try
			{
				key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\OEMInformation");
				string manufacturer = (string)key.GetValue("Manufacturer", "");
				string model = (string)key.GetValue("Model", "");
				oemInfo = String.Format("{0} {1}", manufacturer, model);
			}
			catch
			{
			}
			info.Add("OEMInfo", oemInfo);

			int width = Screen.PrimaryScreen.Bounds.Width;
			int height = Screen.PrimaryScreen.Bounds.Height;
			info.Add("ScreenResolution", (width.ToString() + "x" + height.ToString()));

			try
			{
				key = Registry.LocalMachine.OpenSubKey(Common.Strings.FrameBufferRegKeyPath);
				width = (int)key.GetValue("WindowWidth");
				height = (int)key.GetValue("WindowHeight");
				info.Add("BlueStacksResolution", (width.ToString() + "x" + height.ToString()));
			}
			catch
			{
				width = 0;
				height = 0;
			}

			string dotNetVersion = "";
			try
			{
				key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\NET Framework Setup\NDP");
				string[] dotNetKeys = key.GetSubKeyNames();
				RegistryKey versionKey;
				foreach (string version in dotNetKeys)
				{
					if (version.StartsWith("v"))
					{
						versionKey = key.OpenSubKey(version);
						if (versionKey.GetValue("Install") != null)
							if ((int)versionKey.GetValue("Install") == 1)
								dotNetVersion = (string)versionKey.GetValue("Version");
						if (version == "v4")
						{
							RegistryKey v4VersionKey = versionKey.OpenSubKey("Client");
							if (v4VersionKey != null && (int)v4VersionKey.GetValue("Install") == 1)
								dotNetVersion = ((string)v4VersionKey.GetValue("Version")) + " Client";
							v4VersionKey = versionKey.OpenSubKey("Full");
							if (v4VersionKey != null && (int)v4VersionKey.GetValue("Install") == 1)
								dotNetVersion = ((string)v4VersionKey.GetValue("Version")) + " Full";
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Got exception when checking dot net version,err: {0}", ex.ToString());
			}
			info.Add("DotNetVersion", dotNetVersion);

			if (IsOs64Bit())
			{
				info.Add("OSVERSIONTYPE", "64 bit");
			}
			else
			{
				info.Add("OSVERSIONTYPE", "32 bit");
			}

			s_Info = info;
			return s_Info;
		}

		public static Dictionary<string, string> InfoForGraphicsDriverCheck()
		{
			Dictionary<String, String> info = new Dictionary<string, string>();

			info.Add("os_version", GetSysInfo("Select Caption from Win32_OperatingSystem"));
			info.Add("os_arch", GetSysInfo("Select OSArchitecture from Win32_OperatingSystem"));
			info.Add("processor_vendor", GetSysInfo("Select Manufacturer from Win32_Processor"));
			info.Add("processor", GetSysInfo("Select Name from Win32_Processor"));
			string gpu = GetSysInfo("Select Caption from Win32_VideoController");
			string gpu_vendor = "";
			string[] gpus = gpu.Split(new string[] { Environment.NewLine, "\r\n", "\n" },
					StringSplitOptions.RemoveEmptyEntries);
			if (!String.IsNullOrEmpty(gpu))
			{
				foreach (string vendor in gpus)
				{
					gpu_vendor += vendor.Substring(0, vendor.IndexOf(" ")) + "\r\n";
				}

				gpu_vendor = gpu_vendor.Trim();
			}
			string driver_version = GetSysInfo("Select DriverVersion from Win32_VideoController");
			string driver_date = GetSysInfo("Select DriverDate from Win32_VideoController");

			string[] gpu_vendors = gpu_vendor.Split(new string[] { Environment.NewLine, "\r\n", "\n" },
					StringSplitOptions.RemoveEmptyEntries);
			string[] driver_versions = driver_version.Split(new string[] { Environment.NewLine, "\r\n", "\n" },
					StringSplitOptions.RemoveEmptyEntries);
			string[] driver_dates = driver_date.Split(new string[] { Environment.NewLine, "\r\n", "\n" },
					StringSplitOptions.RemoveEmptyEntries);

			for (int i = 0; i < gpus.Length; i++)
			{
				if (gpus[i] == GlRenderer || GlVendor.Contains(gpu_vendors[i]))
				{
					gpu = gpus[i];
					gpu_vendor = gpu_vendors[i];
					driver_version = driver_versions[i];
					driver_date = driver_dates[i];

					break;
				}
			}

			info.Add("gpu", gpu);
			info.Add("gpu_vendor", gpu_vendor);
			info.Add("driver_version", driver_version);
			info.Add("driver_date", driver_date);

			RegistryKey key;

			string manufacturer = "";
			using (key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\OEMInformation"))
			{
				if (key != null)
					manufacturer = (string)key.GetValue("Manufacturer", "");
			}
			info.Add("oem_manufacturer", manufacturer);

			string model = "";
			using (key = Registry.LocalMachine.OpenSubKey(@"Software\Microsoft\Windows\CurrentVersion\OEMInformation"))
			{
				if (key != null)
					model = (string)key.GetValue("Model", "");
			}
			info.Add("oem_model", model);

			info.Add("bst_oem", OEM);


			return info;
		}

		private static String ToUpper(String id)
		{
			return id.ToUpperInvariant();
		}

		public static String RAM
		{
			get
			{
				int ramInMB = 0;
				try
				{
					string ram = GetSysInfo("Select TotalPhysicalMemory from Win32_ComputerSystem");
					UInt64 ramInBytes = Convert.ToUInt64(ram);
					ramInMB = (int)(ramInBytes / (1024 * 1024));
				}
				catch (Exception ex)
				{
					Logger.Error("Exception when finding ram");
					Logger.Error(ex.ToString());
				}

				return ramInMB.ToString();
			}
		}

		public static String CPU
		{
			get
			{
				return GetSysInfo("Select Name from Win32_Processor");
			}
		}


		public static String GPU
		{
			get
			{
				return GetSysInfo("Select Caption from Win32_VideoController");
			}
		}

		public static String OS
		{
			get
			{
				return GetSysInfo("Select Caption from Win32_OperatingSystem");
			}
		}

		public static string GetSysInfo(string query)
		{
			ManagementObjectSearcher searcher;
			int i = 0;
			string res = "";
			try
			{
				searcher = new ManagementObjectSearcher(query);
				foreach (ManagementObject obj in searcher.Get())
				{
					i++;
					PropertyDataCollection searcherProperties =
						obj.Properties;
					foreach (PropertyData sp in searcherProperties)
						res += sp.Value.ToString() + '\n';
				}
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
			return res.Trim();
		}
	}
}
