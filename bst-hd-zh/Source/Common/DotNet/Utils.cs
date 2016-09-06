using System;
using System.IO;
using System.Net;
using System.Text;
using Microsoft.Win32;
using System.Threading;
using System.Drawing;
using System.Net.Sockets;
using System.Diagnostics;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.ServiceProcess;
using System.Drawing.Imaging;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Management;
using System.Security.Cryptography.X509Certificates;
using System.Security;
using System.Security.Permissions;
using System.Security.AccessControl;
using System.Security.Principal;
using BlueStacks.hyperDroid.Device;

using WindowInterop = BlueStacks.hyperDroid.Common.Interop.Window;

using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Common
{

	class ExtendedWebClient : WebClient
	{
		private int mTimeout;

		public ExtendedWebClient(int timeout)
		{
			this.mTimeout = timeout;
		}

		protected override WebRequest GetWebRequest(Uri address)
		{
			WebRequest result = base.GetWebRequest(address);
			result.Timeout = this.mTimeout;
			return result;
		}
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct GamePad
	{
		public int X;
		public int Y;
		public int Z;
		public int Rx;
		public int Ry;
		public int Rz;
		public int Hat;
		public uint Mask;
	}

	public class ProcessDetails
	{
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
		private struct PROCESSENTRY32
		{
			const int MAX_PATH = 260;
			internal UInt32 dwSize;
			internal UInt32 cntUsage;
			internal UInt32 th32ProcessID;
			internal IntPtr th32DefaultHeapID;
			internal UInt32 th32ModuleID;
			internal UInt32 cntThreads;
			internal UInt32 th32ParentProcessID;
			internal Int32 pcPriClassBase;
			internal UInt32 dwFlags;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = MAX_PATH)]
			internal string szExeFile;
		}

		[Flags]
		private enum SnapshotFlags : uint
		{
			HeapList = 0x00000001,
			Process = 0x00000002,
			Thread = 0x00000004,
			Module = 0x00000008,
			Module32 = 0x00000010,
			Inherit = 0x80000000,
			All = 0x0000001F,
			NoHeaps = 0x40000000
		}

		[DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		static extern IntPtr CreateToolhelp32Snapshot([In]UInt32 dwFlags, [In]UInt32 th32ProcessID);

		[DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		static extern bool Process32First([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("kernel32", SetLastError = true, CharSet = System.Runtime.InteropServices.CharSet.Auto)]
		static extern bool Process32Next([In]IntPtr hSnapshot, ref PROCESSENTRY32 lppe);

		[DllImport("kernel32", SetLastError = true)]
		[return: MarshalAs(UnmanagedType.Bool)]
		private static extern bool CloseHandle([In] IntPtr hObject);

		public static int? GetParentProcessId(int pid)
		{
			Process parentProcess = GetParentProcess(pid);
			if (parentProcess == null)
				return null;
			return parentProcess.Id;
		}

		public static Process GetParentProcess(int pid)
		{
			Process parentProc = null;
			IntPtr handleToSnapshot = IntPtr.Zero;
			try
			{
				PROCESSENTRY32 procEntry = new PROCESSENTRY32();
				procEntry.dwSize = (UInt32)Marshal.SizeOf(typeof(PROCESSENTRY32));
				handleToSnapshot = CreateToolhelp32Snapshot((uint)SnapshotFlags.Process, 0);
				if (Process32First(handleToSnapshot, ref procEntry))
				{
					do
					{
						if (pid == procEntry.th32ProcessID)
						{
							parentProc = Process.GetProcessById((int)procEntry.th32ParentProcessID);
							break;
						}
					} while (Process32Next(handleToSnapshot, ref procEntry));
				}
				else
				{
					throw new ApplicationException(string.Format("Failed with win32 error code {0}", Marshal.GetLastWin32Error()));
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Can't get the process.", ex.ToString());
			}
			finally
			{
				// Must clean up the snapshot object!
				CloseHandle(handleToSnapshot);
			}

			return parentProc;
		}

		public static Process CurrentProcessParent
		{
			get
			{
				return GetParentProcess(Process.GetCurrentProcess().Id);
			}
		}

		public static int? CurrentProcessParentId
		{
			get
			{
				return GetParentProcessId(Process.GetCurrentProcess().Id);
			}
		}

		public static int CurrentProcessId
		{
			get
			{
				return Process.GetCurrentProcess().Id;
			}
		}

		public static int? GetNthParentPid(int pid, int order)
		{
			int? queryPid = pid;

			while (order > 0 && queryPid.HasValue == true)
			{
				queryPid = GetParentProcessId(queryPid.Value);
				order--;
			}

			return queryPid;
		}
	}

	public class Utils
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		private static extern bool IsWow64Process(IntPtr proc, ref bool isWow);

		[DllImport(@"urlmon.dll", CharSet = CharSet.Auto)]
		private extern static System.UInt32 FindMimeFromData(
					System.UInt32 pBC,
					[MarshalAs(UnmanagedType.LPStr)] System.String pwzUrl,
					[MarshalAs(UnmanagedType.LPArray)] byte[] pBuffer,
					System.UInt32 cbSize,
					[MarshalAs(UnmanagedType.LPStr)] System.String pwzMimeProposed,
					System.UInt32 dwMimeFlags,
					out System.UInt32 ppwzMimeOut,
					System.UInt32 dwReserverd
					);

		[DllImport("user32.dll")]
		private extern static int GetSystemMetrics(int smIndex);

		[DllImport("hid.dll", SetLastError = true)]
		public static extern void HidD_GetHidGuid(
					ref Guid lpHidGuid
					);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiGetClassDevs(
					ref Guid lpGuid,
					IntPtr Enumerator,
					IntPtr hwndParent,
					ClassDevsFlags Flags
					);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiGetClassDevs(
					IntPtr guid,
					IntPtr Enumerator,
					IntPtr hwndParent,
					ClassDevsFlags Flags
					);

		[StructLayout(LayoutKind.Sequential)]
		public struct SP_DEVINFO_DATA
		{
			public int cbSize;
			public Guid ClassGuid;
			public int DevInst;
			public int Reserved;
		}

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiEnumDeviceInfo(
					int DeviceInfoSet,
					int Index,
					ref SP_DEVINFO_DATA DeviceInfoData
					);

		[Flags]
		public enum ClassDevsFlags
		{
			DIGCF_DEFAULT = 0x00000001,
			DIGCF_PRESENT = 0x00000002,
			DIGCF_ALLCLASSES = 0x00000004,
			DIGCF_PROFILE = 0x00000008,
			DIGCF_DEVICEINTERFACE = 0x00000010,
		}

		// Device interface data
		[StructLayout(LayoutKind.Sequential)]
		public struct SP_DEVICE_INTERFACE_DATA
		{
			public int cbSize;
			public Guid InterfaceClassGuid;
			public int Flags;
			public int Reserved;
		}

		// Device interface detail data
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct PSP_DEVICE_INTERFACE_DETAIL_DATA
		{
			public int cbSize;
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 256)]
			public string DevicePath;
		}

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiEnumDeviceInterfaces(
					int DeviceInfoSet,
					int DeviceInfoData,
					ref Guid lpHidGuid,
					int MemberIndex,
					ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiGetDeviceInterfaceDetail(
					int DeviceInfoSet,
					ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData,
					IntPtr aPtr,
					int detailSize,
					ref int requiredSize,
					IntPtr bPtr);

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiGetDeviceInterfaceDetail(
					int DeviceInfoSet,
					ref SP_DEVICE_INTERFACE_DATA lpDeviceInterfaceData,
					ref PSP_DEVICE_INTERFACE_DETAIL_DATA myPSP_DEVICE_INTERFACE_DETAIL_DATA,
					int detailSize,
					ref int requiredSize,
					IntPtr bPtr);

		public enum GamePadEventType
		{
			TYPE_GAMEPAD_ATTACH,
			TYPE_GAMEPAD_DETACH,
			TYPE_GAMEPAD_UPDATE,
		}

		public enum RegPropertyType
		{
			SPDRP_DEVICEDESC = 0x00000000, // DeviceDesc (R/W)
			SPDRP_HARDWAREID = 0x00000001, // HardwareID (R/W)
			SPDRP_COMPATIBLEIDS = 0x00000002, // CompatibleIDs (R/W)
			SPDRP_UNUSED0 = 0x00000003, // unused
			SPDRP_SERVICE = 0x00000004, // Service (R/W)
			SPDRP_UNUSED1 = 0x00000005, // unused
			SPDRP_UNUSED2 = 0x00000006, // unused
			SPDRP_CLASS = 0x00000007, // Class (R--tied to ClassGUID)
			SPDRP_CLASSGUID = 0x00000008, // ClassGUID (R/W)
			SPDRP_DRIVER = 0x00000009, // Driver (R/W)
			SPDRP_CONFIGFLAGS = 0x0000000A, // ConfigFlags (R/W)
			SPDRP_MFG = 0x0000000B, // Mfg (R/W)
			SPDRP_FRIENDLYNAME = 0x0000000C, // FriendlyName (R/W)
			SPDRP_LOCATION_INFORMATION = 0x0000000D,// LocationInformation (R/W)
			SPDRP_PHYSICAL_DEVICE_OBJECT_NAME = 0x0000000E, // PhysicalDeviceObjectName (R)
			SPDRP_CAPABILITIES = 0x0000000F, // Capabilities (R)
			SPDRP_UI_NUMBER = 0x00000010, // UiNumber (R)
			SPDRP_UPPERFILTERS = 0x00000011, // UpperFilters (R/W)
			SPDRP_LOWERFILTERS = 0x00000012, // LowerFilters (R/W)
			SPDRP_BUSTYPEGUID = 0x00000013, // BusTypeGUID (R)
			SPDRP_LEGACYBUSTYPE = 0x00000014, // LegacyBusType (R)
			SPDRP_BUSNUMBER = 0x00000015, // BusNumber (R)
			SPDRP_ENUMERATOR_NAME = 0x00000016, // Enumerator Name (R)
			SPDRP_SECURITY = 0x00000017, // Security (R/W, binary form)
			SPDRP_SECURITY_SDS = 0x00000018, // Security (W, SDS form)
			SPDRP_DEVTYPE = 0x00000019, // Device Type (R/W)
			SPDRP_EXCLUSIVE = 0x0000001A, // Device is exclusive-access (R/W)
			SPDRP_CHARACTERISTICS = 0x0000001B, // Device Characteristics (R/W)
			SPDRP_ADDRESS = 0x0000001C, // Device Address (R)
			SPDRP_UI_NUMBER_DESC_FORMAT = 0x0000001E, // UiNumberDescFormat (R/W)
			SPDRP_MAXIMUM_PROPERTY = 0x0000001F  // Upper bound on ordinals
		}

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiGetDeviceRegistryProperty(
					int DeviceInfoSet,
					ref SP_DEVINFO_DATA DeviceInfoData,
					RegPropertyType Property,
					IntPtr PropertyRegDataType,
					IntPtr PropertyBuffer,
					int PropertyBufferSize,
					ref int RequiredSize
					);

		// Device interface detail data
		[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Ansi)]
		public struct DATA_BUFFER
		{
			[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 1024)]
			public string Buffer;
		}

		[DllImport("setupapi.dll", SetLastError = true)]
		public static extern int SetupDiGetDeviceRegistryProperty(
					int DeviceInfoSet,
					ref SP_DEVINFO_DATA DeviceInfoData,
					RegPropertyType Property,
					IntPtr PropertyRegDataType,
					ref DATA_BUFFER PropertyBuffer,
					int PropertyBufferSize,
					ref int RequiredSize
					);


		[DllImport("gdi32.dll", CharSet = CharSet.Auto, SetLastError = true, ExactSpelling = true)]
		public static extern int GetDeviceCaps(IntPtr hDC, int nIndex);

		public enum DeviceCap
		{
			/// <summary>
			/// Logical pixels inch in X
			/// </summary>
			LOGPIXELSX = 88,
			/// <summary>
			/// Logical pixels inch in Y
			/// </summary>
			LOGPIXELSY = 90,

			VERTRES = 10,
			DESKTOPVERTRES = 117
		}


		private const int SM_TABLETPC = 86;
		private const int TASKBAR_HEIGHT = 48;
		public const int BTV_RIGHT_PANEL_WIDTH = 320;

		private static string s_BstHKLMPath = Common.Strings.RegBasePath;
		private static string s_BstHKCUCloudPath = Common.Strings.CloudRegKeyPath;

		private const int SM_CXSCREEN = 0;
		private const int SM_CYSCREEN = 1;

		private static Object sLogFailureLogRegLock = new Object();
		private static Object sWaitForBootCompleteLock = new Object();
		public static bool sIsWaitLockExist = false;

		public const int DEFAULT_DPI = 96;
		static int currentDPI = int.MinValue;
		public static int CurrentDPI
		{
			get
			{
				if (currentDPI.Equals(int.MinValue))
				{
					currentDPI = GetDPI();
				}
				return currentDPI;
			}

		}

		public static void NotifyBootFailureToParentWindow(String className, String windowName, int exitCode)
		{
			Logger.Info("Sending BOOT_FAILURE message to class = {0}, window = {1}", className, windowName);
			IntPtr handle = Common.Interop.Window.FindWindow(className, windowName);
			try
			{
				if (handle == IntPtr.Zero)
				{
					Logger.Info("Unable to find window : {0}", className);
					return;
				}
				uint wparam = (uint)exitCode;
				Logger.Info("Sending wparam : {0}", wparam);
				Common.Interop.Window.SendMessage(handle, Common.Interop.Window.WM_USER_BOOT_FAILURE, (IntPtr)wparam, IntPtr.Zero);
				Logger.Info("Sent BOOT_FAILURE message");
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}
		}

		public static bool IsDesktopPC()
		{
			return (GetSystemMetrics(SM_TABLETPC) == 0);
		}

		public static void RunDxSetup(string exePath)
		{
			Logger.Info("Running dxsetup");
			string dxSetupPath = Path.Combine(exePath, "DXSETUP.exe");
			Process DirectXInstaller = new Process();
			DirectXInstaller.StartInfo.FileName = dxSetupPath;
			DirectXInstaller.StartInfo.Arguments = "/silent";
			DirectXInstaller.Start();
			DirectXInstaller.WaitForExit();
			Logger.Info("dxsetup exe process finished");
		}

		public static void CopyRecursive(String srcPath, String dstPath)
		{
			if (!Directory.Exists(dstPath))
				Directory.CreateDirectory(dstPath);

			DirectoryInfo src = new DirectoryInfo(srcPath);

			foreach (FileInfo file in src.GetFiles())
			{
				Logger.Info(file.FullName + " {0}", DateTime.Now);
				file.CopyTo(Path.Combine(dstPath, file.Name), true);
			}

			foreach (DirectoryInfo dir in src.GetDirectories())
			{
				Logger.Info(dir.FullName + " {0}", DateTime.Now);
				CopyRecursive(Path.Combine(srcPath, dir.Name),
						Path.Combine(dstPath, dir.Name));
			}
		}

		public static bool IsOSWinXP()
		{
			return Environment.OSVersion.Version.Major == 5;
		}

		public static bool IsOSVista()
		{
			return (Environment.OSVersion.Version.Major == 6
					&& Environment.OSVersion.Version.Minor == 0);
		}

		public static bool IsOSWin7()
		{
			return (Environment.OSVersion.Version.Major == 6
					&& Environment.OSVersion.Version.Minor == 1);
		}

		public static bool IsOSWin8()
		{
			return (Environment.OSVersion.Version.Major == 6
					&& Environment.OSVersion.Version.Minor == 2);
		}

		public static bool IsOSWin81()
		{
			return (Environment.OSVersion.Version.Major == 6
					&& Environment.OSVersion.Version.Minor == 3);
		}

		private static bool IsOSWin10()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows NT\CurrentVersion");
			string productName = (string)key.GetValue("ProductName");
			return productName.Contains("Windows 10");
		}

		public static bool RenameKernelAndInitrdFile()
		{
			Logger.Info("In RenameKernelAndInitrdFile");
			int logicalProcessorsCount = 0;
			try
			{
				logicalProcessorsCount = Environment.ProcessorCount;
			}
			catch (Exception ex)
			{
				Logger.Error("got error when checking processorCount , exception :{0}", ex.ToString());
			}

			Logger.Info("the number of Logger.Infoical processors are" + logicalProcessorsCount);

			string commonAppDataDir;
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
			{
				commonAppDataDir = (string)key.GetValue("DataDir");
			}
			string uniProcessorKernelFile = Path.Combine(commonAppDataDir, @"Android\kernel.elf.uni");
			string multiProcessorKernelFile = Path.Combine(commonAppDataDir, @"Android\kernel.elf");
			string uniProcessorInitrdFile = Path.Combine(commonAppDataDir, @"Android\initrd.img.uni");
			string multiProcessorInitrdFile = Path.Combine(commonAppDataDir, @"Android\initrd.img");

			if (logicalProcessorsCount >= 3)
			{
				Logger.Info("removing kernel.elf.uni and initrd.img.uni");
				if (File.Exists(uniProcessorInitrdFile))
				{
					File.Delete(uniProcessorInitrdFile);
					Logger.Info(uniProcessorInitrdFile + " file is deleted");
				}
				if (File.Exists(uniProcessorKernelFile))
				{
					File.Delete(uniProcessorKernelFile);
					Logger.Info(uniProcessorKernelFile + " file is deleted");
				}
				return true;
			}
			// deleting EnableVMSmpMode value
			RegistryKey enableVmSmpModekey = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			enableVmSmpModekey.DeleteValue("EnableVMSmpMode", false);
			Logger.Info("the enableVmSmpMode registry value is deleted");

			if (File.Exists(uniProcessorKernelFile))
			{
				Logger.Info("trying to move " + uniProcessorKernelFile + "to  " + multiProcessorKernelFile);
				if (File.Exists(multiProcessorKernelFile))
				{
					Logger.Info("deleting the multiProcessorKernelFile " + multiProcessorKernelFile);
					File.Delete(multiProcessorKernelFile);
				}

				File.Move(uniProcessorKernelFile, multiProcessorKernelFile);
			}

			if (File.Exists(uniProcessorInitrdFile))
			{
				Logger.Info("Moving " + uniProcessorInitrdFile + " to  " + multiProcessorInitrdFile);
				if (File.Exists(multiProcessorInitrdFile))
				{
					Logger.Info("deleting the multiProcessorInitrdFile " + multiProcessorInitrdFile);
					File.Delete(multiProcessorInitrdFile);
				}
				File.Move(uniProcessorInitrdFile, multiProcessorInitrdFile);
			}
			return true;
		}

		public static bool ConvertSparseToVdi(string blockDeviceToolPath, string srcSparseFile, string destVdiFile, string UUID)
		{
			string cmd = blockDeviceToolPath;
			string args = string.Format("sparse-to-vdi {0} \"{1}\" \"{2}\"", UUID, srcSparseFile, destVdiFile);

			if (File.Exists(destVdiFile))
				File.Delete(destVdiFile);

			File.Create(destVdiFile).Dispose();

			try
			{
				CmdRes res = RunCmd(cmd, args, null);
				if (res.ExitCode != 0)
					return false;
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while converting from sparse to vdi file." + ex.ToString());
				return false;
			}
			return true;
		}

		public static bool ConvertFlatToVdi(string blockDeviceToolPath, string srcFlatFile, string destVdiFile, string UUID)
		{
			string cmd = blockDeviceToolPath;
			string args = string.Format("flat-to-vdi {0} \"{1}\" \"{2}\"", UUID, srcFlatFile, destVdiFile);

			Logger.Info("cmd " + cmd);
			Logger.Info("args " + args);
			if (File.Exists(destVdiFile))
				File.Delete(destVdiFile);

			File.Create(destVdiFile).Dispose();

			try
			{
				Process proc = new Process();
				proc.StartInfo.FileName = cmd;
				proc.StartInfo.Arguments = args;
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.CreateNoWindow = true;

				proc.Start();
				proc.WaitForExit();

				int exitCode = proc.ExitCode;
				if (exitCode != 0)
					return false;
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while converting from flat to vdi file." + ex.ToString());
				return false;
			}
			return true;
		}

		public static void QuitMultiInstance(string folderPath)
		{
			Process p = new Process();

			string quitMultiInstancePath = Path.Combine(folderPath, "HD-QuitMultiInstance.exe");
			Logger.Info("the MultiInstance exe Path is" + quitMultiInstancePath);
			p.StartInfo.FileName = quitMultiInstancePath;
			p.Start();
			p.WaitForExit();
			if (p.ExitCode == 2)
			{
				Logger.Info("HD-QuitMultiInstance requires higher priviliges to quit processes");
				Process pAdmin = new Process();
				pAdmin.StartInfo.FileName = quitMultiInstancePath;

				if (System.Environment.OSVersion.Version.Major >= 6)
				{
					pAdmin.StartInfo.Verb = "runas";
				}

				pAdmin.StartInfo.UseShellExecute = true;
				pAdmin.Start();
				pAdmin.WaitForExit();
			}
		}


		public static bool IsProxyEnabled(out string proxy)
		{
			Uri testUri = new Uri("http://www.bluestacks.com/");
			IWebProxy defaultProxy = WebRequest.GetSystemWebProxy();
			bool isByPassed = defaultProxy.IsBypassed(testUri);
			Uri proxyUri = defaultProxy.GetProxy(testUri);
			proxy = proxyUri.ToString();
			return !isByPassed;
		}

		public static bool GetOSInfo(
				out string osName,
				out string servicePack,
				out string osArch
				)
		{
			osName = "";
			servicePack = "";
			osArch = "";

			OperatingSystem os = Environment.OSVersion;
			System.Version vs = os.Version;

			if (os.Platform == PlatformID.Win32Windows)
			{
				//This is a pre-NT version of Windows
				switch (vs.Minor)
				{
					case 0:
						osName = "95";
						break;
					case 10:
						if (vs.Revision.ToString() == "2222A")
							osName = "98SE";
						else
							osName = "98";
						break;
					case 90:
						osName = "Me";
						break;
					default:
						break;
				}
			}
			else if (os.Platform == PlatformID.Win32NT)
			{
				switch (vs.Major)
				{
					case 3:
						osName = "NT 3.51";
						break;
					case 4:
						osName = "NT 4.0";
						break;
					case 5:
						if (vs.Minor == 0)
							osName = "2000";
						else
							osName = "XP";
						break;
					case 6:
						if (vs.Minor == 0)
							osName = "Vista";

						else if (vs.Minor == 1)
							osName = "7";

						else if (vs.Minor == 2)
							osName = "8";

						else if (vs.Minor == 3)
							osName = "8.1";

						break;

					case 10:
						osName = "10";
						break;
					default:
						break;
				}
			}

			string operatingSystem = osName;

			//Make sure we actually got something in our OS check
			//We don't want to just return " Service Pack 2" or " 32-bit"
			//That information is useless without the OS version.
			if (operatingSystem != "")
			{
				//Got something.  Let's prepend "Windows" and get more info.
				operatingSystem = "Windows " + operatingSystem;
				//See if there's a service pack installed.
				if (os.ServicePack != "")
				{
					//Append it to the OS name.  i.e. "Windows XP Service Pack 3"
					servicePack = os.ServicePack.Substring(os.ServicePack.LastIndexOf(' ') + 1);
					operatingSystem += " " + os.ServicePack;
				}
				//Append the OS architecture.  i.e. "Windows XP Service Pack 3 32-bit"
				osArch = getOSArchitecture().ToString() + "-bit";
				operatingSystem += " " + osArch;
			}
			else
			{
				return false;
			}

			Logger.Info("Operating system details: " + operatingSystem);

			return true;
		}

		public static int GetAndroidVMMemory()
		{
			int size = 768;
			try
			{
				Logger.Info("Checking for physical memory...");
				string ramString = Device.Profile.GetSysInfo("Select TotalPhysicalMemory from Win32_ComputerSystem");
				long ram = Int64.Parse(ramString);

				size = (int)((ram * 0.4) / (1024 * 1024));
				Logger.Info("Ram = {0}, size = {1}", ram, size);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to check physical memory. err: " + ex.ToString());
				size = 768;
			}
			if (size > 1856)
				size = 1856;
			else if (size < 768)
				size = 768;

			Logger.Info("Using Memory Size = {0}", size);

			return size;
		}

		public static bool IsOs64Bit()
		{
			Process proc = Process.GetCurrentProcess();
			bool isWow = false;

			if (!IsWow64Process(proc.Handle, ref isWow))
			{
				throw new Exception("Could not get os arch info.");
			}

			return isWow;
		}

		public static int getOSArchitecture()
		{
			string pa = Environment.GetEnvironmentVariable("PROCESSOR_ARCHITECTURE", EnvironmentVariableTarget.Machine);
			return ((String.IsNullOrEmpty(pa) || String.Compare(pa, 0, "x86", 0, 3, true) == 0) ? 32 : 64);
		}


		public static bool FindProcessByName(String name)
		{
			Process[] procList;
			procList = Process.GetProcessesByName(name);
			return (procList.Length != 0);
		}

		public static void KillProcessById(int pid)
		{
			try
			{
				Logger.Info("Killing Process with PID {0}", pid);
				Process proc = Process.GetProcessById(pid);
				proc.Kill();
			}
			catch (Exception ex)
			{
				Logger.Error(ex.ToString());
			}
		}

		public static string GetAlreadyDownloadedVersion()
		{
			string alreadyDownloadedVersion = null;

			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegUpdaterPath))
				{
					alreadyDownloadedVersion = (string)key.GetValue("NewVersion", null);
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Ignoring Exception: {0}", ex.ToString());
			}

			return alreadyDownloadedVersion;
		}

		private static void CleanUpUpdateRegistryValues()
		{
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
							Common.Strings.RegUpdaterPath, true))
			{
				if (key.GetValue("Forced", null) != null) key.DeleteValue("Forced");
				if (key.GetValue("NewVersion", null) != null) key.DeleteValue("NewVersion");
				if (key.GetValue("Executable", null) != null) key.DeleteValue("Executable");
			}
		}

		private static void StartUpdateInstallerExecuatbleAndExit()
		{
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
							Common.Strings.RegUpdaterPath))
			{
				string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
				string downloadDir = Path.Combine(programData, @"BlueStacksSetup");
				string execuatableFileName = (string)key.GetValue("Executable", null);
				string execuatablePath = string.Empty;

				if (execuatableFileName != null)
					execuatablePath = Path.Combine(downloadDir, execuatableFileName);

				if (execuatableFileName == null || !File.Exists(execuatablePath))
				{
					Logger.Info("No executable exist");
					CleanUpUpdateRegistryValues();
					//return as this case should not arise or if came will delete all values
					//and launch the frontend
					return;
				}

				Process Proc = new Process();
				Proc.StartInfo.UseShellExecute = false;
				Proc.StartInfo.FileName = execuatablePath;
				Proc.Start();

				CleanUpUpdateRegistryValues();

				Environment.Exit(0);
			}
		}

		/*
		 * Always checks for update from manifestUrl
		 * Starts HD-Updater if update exist
		 * Should be called from thread as it might provide 1-2 sec delay in startup
		 */
		public static void StartUpdaterIfAvailable()
		{
			Logger.Info("Checking for update");

			/* check for update once in 24 hrs */
			string dateFormat = "yyyy-MM-dd HH:mm";
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegUpdaterPath, true);
			string date = (string)key.GetValue("LastCheckDateUTC", null);
			if (date != null)
			{
				DateTime firstRun = Convert.ToDateTime(date);
				TimeSpan ts = DateTime.UtcNow - firstRun;
				int differenceDays = ts.Days;
				if (differenceDays == 0)
				{
					Logger.Info("Skipping update check as differenceDays value is {0}", differenceDays);
					return;
				}
			}

			DateTime now = DateTime.UtcNow;
			key.SetValue("LastCheckDateUTC", now.ToString(dateFormat), RegistryValueKind.String);
			key.Flush();
			key.Close();

			string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
			string downloadDir = Path.Combine(programData, @"BlueStacksSetup");
			string manifestFileUrl = ((string)Registry.LocalMachine.OpenSubKey(Common.Strings.RegUpdaterPath).GetValue("ManifestURL")) + ".ini";
			Logger.Info("manifestFileUrl: {0}", manifestFileUrl);
			if (Features.IsFeatureEnabled(Features.CHINA_CLOUD))
			{
				manifestFileUrl = manifestFileUrl.Replace("cdn.bluestacks.com", "cdn.bluestacks.cn");
				Logger.Info("updated manifestFileUrl: {0}", manifestFileUrl);
			}

			Thread updateThread = new Thread(delegate ()
			{
				WebClient webClient = new WebClient();
				try
				{
					Logger.Info("Downloading update manifest from: " + manifestFileUrl);
					string manifestIniPath = Path.Combine(downloadDir, "update.ini");
					webClient.DownloadFile(manifestFileUrl, manifestIniPath);

					string downloadZipUrl, newVersion, isForced, updateExecutableName;
					PopulateUpdateIniVariables(manifestIniPath, out downloadZipUrl,
							out newVersion, out isForced, out updateExecutableName);
					string installDir = GetInstallDir();

					string alreadyDownloadedVersion = GetAlreadyDownloadedVersion();
					if (alreadyDownloadedVersion != null && alreadyDownloadedVersion.Equals(newVersion))
					{
						Logger.Info("already Downloaded exe for version {0}", alreadyDownloadedVersion);
						return;
					}

					Process updaterProc = new Process();
					updaterProc.StartInfo.UseShellExecute = false;
					updaterProc.StartInfo.CreateNoWindow = true;
					updaterProc.StartInfo.FileName = Path.Combine(installDir, "HD-Updater.exe");
					updaterProc.StartInfo.Arguments = "\"" + manifestIniPath + "\"";
					updaterProc.Start();
				}
				catch (Exception exc)
				{
					Logger.Error("Exception in check update: " + exc.ToString());
				}
			});
			updateThread.IsBackground = true;
			updateThread.Start();
		}

		/*
		 * Checks Forced key in Updater Registry
		 * if Forced is 1 Show Popup for update and exit if selected No
		 * if Forced is 0 Show Popup for update run bluestacks if selected No
		 * Don't do anything if no registry exist
		 */
		public static void ExitIfForceUpdateAvailable()
		{
			DialogResult updateDialog;
			string updateAvailableText, updateProgressTitleText;
			Logger.Info("if update available for install");
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
							Common.Strings.RegUpdaterPath))
			{
				int isForced = (int)key.GetValue("Forced", -1);
				Logger.Info("Forced value: {0}", isForced);

				if (isForced > -1)
				{
					Logger.Info("Checking for certificate for file downloaded");

					string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
					string downloadDir = Path.Combine(programData, @"BlueStacksSetup");
					string executableFileName = (string)key.GetValue("Executable", "");
					string executablePath = Path.Combine(downloadDir, executableFileName);
					Logger.Info("the executablePath is {0}", executablePath);
					if (!IsSignedByBlueStacks(executablePath))
					{
						Logger.Info("cleaning up registry");
						CleanUpUpdateRegistryValues();
						Logger.Info("the executable exe is not signed by bluestacks, returning");
						return;
					}
				}

				switch (isForced)
				{
					case -1:
						Logger.Info("No Forced Update Available");
						break;
					case 0:

						updateAvailableText = Locale.Strings.GetLocalizedString("UpdateAvailableText");
						if (updateAvailableText.Equals("UpdateAvailableText"))
							updateAvailableText = "Update is available. Do you want to install now?";

						updateProgressTitleText = Locale.Strings.GetLocalizedString("UpdateProgressTitleText");
						if (updateProgressTitleText.Equals("UpdateProgressTitleText"))
							updateProgressTitleText = "BlueStacks Updater";

						updateDialog = MessageBox.Show(updateAvailableText, updateProgressTitleText,
								MessageBoxButtons.YesNo);
						if (updateDialog == DialogResult.Yes)
						{
							Logger.Info("Run update installer");
							StartUpdateInstallerExecuatbleAndExit();
						}
						break;
					case 1:

						updateAvailableText = Locale.Strings.GetLocalizedString("ForcedUpdateAvailableText");
						if (updateAvailableText.Equals("ForcedUpdateAvailableText"))
							updateAvailableText = "Forced update is available. Do you want to install now?";

						updateProgressTitleText = Locale.Strings.GetLocalizedString("UpdateProgressTitleText");
						if (updateProgressTitleText.Equals("UpdateProgressTitleText"))
							updateProgressTitleText = "BlueStacks Updater";

						updateDialog = MessageBox.Show(updateAvailableText, updateProgressTitleText,
								MessageBoxButtons.YesNo);
						if (updateDialog == DialogResult.Yes)
						{
							Logger.Info("Run update installer");
							StartUpdateInstallerExecuatbleAndExit();
						}
						else
						{
							Logger.Info("Exiting as it is forced Download");
							//Killing process which may be left running as calling from Frontend
							KillProcessByName("HD-RunApp");
							Environment.Exit(0);
						}
						break;
				}
			}
		}

		public static void PopulateUpdateIniVariables(string manifestIniPath, out string downloadZipUrl,
				out string newVersion, out string isForced, out string updateExecutableName)
		{
			IniFile iniFile = new IniFile(manifestIniPath);
			downloadZipUrl = iniFile.GetValue("update", "url");
			newVersion = iniFile.GetValue("update", "version");
			isForced = iniFile.GetValue("update", "force");
			updateExecutableName = iniFile.GetValue("update", "filename");

			Logger.Info("update.ini values are url: {0}, version: {1}, force: {2}, upadateExecutableName: {3}",
					   downloadZipUrl, newVersion, isForced, updateExecutableName);
		}

		/*
		 *	@param dirPath set to the filepath to kill process which has the same file path, set to empty otherwise
		 *  @param ignoreDirPath set to the filepath that has to be ignored for killing a process, set to empty otherwise
		 */
		public static void KillProcessByName(string name, string dirPath, string ignoreDirPath)
		{
			Process[] procList;

			procList = Process.GetProcessesByName(name);
			string processFilePath = String.Empty;
			string processDirectory = String.Empty;
			foreach (Process proc in procList)
			{
				try
				{
					processFilePath = proc.MainModule.FileName;
					processDirectory = Path.GetDirectoryName(processFilePath);
					Logger.Debug("the processDirectory is {0}", processDirectory);
				}
				catch (Exception ex)
				{
					Logger.Error("Got exception in finding processdirectorypath ex:{0}", ex.ToString());
				}

				if (String.IsNullOrEmpty(ignoreDirPath) == false
						&& String.Compare(Path.GetFullPath(ignoreDirPath).TrimEnd('\\'),
							Path.GetFullPath(processDirectory).TrimEnd('\\'),
								StringComparison.InvariantCultureIgnoreCase) == 0)
				{
					Logger.Info("processfile path for proc {0} is in ignoreDir:{1}", proc.Id, ignoreDirPath);
					continue;
				}

				if (String.IsNullOrEmpty(dirPath) == false
						&& String.Compare(Path.GetFullPath(dirPath).TrimEnd('\\'),
							Path.GetFullPath(processDirectory).TrimEnd('\\'),
								StringComparison.InvariantCultureIgnoreCase) != 0)
				{
					Logger.Info("Process {0} not killed as dirPath {1} and processPath are NOT same", proc.Id, dirPath);
					continue;
				}

				Logger.Info("Killing PID " + proc.Id + " -> " + proc.ProcessName);

				/*
				 * Kill the process and then wait for
				 * it to exit.
				 */

				try
				{
					proc.Kill();
				}
				catch (Exception exc)
				{
					/* bummer */
					Logger.Error(exc.ToString());
					continue;
				}

				if (!proc.WaitForExit(5000))
				{
					Logger.Info("Timeout waiting for process to die");
				}
			}
		}

		public static void KillProcessByName(String name)
		{
			Process[] procList;

			procList = Process.GetProcessesByName(name);
			foreach (Process proc in procList)
			{
				try
				{
					proc.Kill();
					if (!proc.WaitForExit(5000))
					{
						Logger.Info("Timeout waiting for process to die");
					}
				}
				catch (Exception exc)
				{
					/* bummer */
					Logger.Error(exc.ToString());
					continue;
				}
			}
		}

		public static void KillProcessesByName(String[] nameList)
		{
			foreach (String name in nameList)
			{
				KillProcessByName(name);
			}
		}

		/* HARD-CODING THE IP TO BE SENT TO ANDROID TO 10.0.2.2/
		/*
		public static string GetIPAddress() {
		IPHostEntry host = Dns.GetHostEntry("");
		for (int i=0; i<host.AddressList.Length; i++)
		{
		if(host.AddressList[i].AddressFamily.ToString() == ProtocolFamily.InterNetwork.ToString() &&
		!host.AddressList[i].ToString().StartsWith("127"))
		return host.AddressList[i].ToString();
		}
		return "10.0.2.2";
		}
		*/

		/*
		 * return false if service wasnt running
		 * return true if service was running
		 */

		public static bool StartServiceIfNeeded(string vmName)
		{
			return StartServiceIfNeeded(false, vmName);
		}

		public static void StartServiceIgnoreAlreadyRunningException(ServiceController sc, string serviceName, string startType)
		{
			try
			{
				Utils.EnableService(serviceName, startType);
				sc.Start();
			}
			catch (Exception exc)
			{
				sc.Refresh();
				if (sc.Status != ServiceControllerStatus.Running &&
						sc.Status != ServiceControllerStatus.StartPending)
				{
					Logger.Error("Failed to start {0}", serviceName);
					Logger.Error("{0} status = {1}", serviceName, sc.Status);
					Logger.Error(exc.ToString());
					throw exc;
				}
				else
				{
					Logger.Info("{0} is already running", serviceName);
				}
			}
		}

		public static bool StartServiceIfNeeded(bool waitForRunAppExit, string vmName)
		{
			string serviceName = Common.Strings.GetAndroidServiceName(vmName);
			ServiceController sc = new ServiceController(serviceName);
			if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending)
			{
				String cfgPath = Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName);
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
							cfgPath, true))
				{
					key.SetValue("ServiceStoppedGracefully", 1,
							RegistryValueKind.DWord);
					key.Flush();
				}
				Logger.Info("Utils: Starting service");
				StartServiceIgnoreAlreadyRunningException(sc, serviceName, "auto");
				sc.WaitForStatus(ServiceControllerStatus.Running);
				if (!IsGlHotAttach(vmName))
				{
					/*
					 * Also need to start the frontend because full android boot
					 * requires the frontend to be running at boot time
					 * This is required only in case GlHotAttach is disabled
					 */
					Process proc = StartHiddenFrontend(vmName);
					if (waitForRunAppExit)
					{
						proc.WaitForExit(60);
					}
				}
				return false;
			}
			else
			{
				if (!IsGuestBooted() && !IsUIProcessAlive() &&
						!IsGlHotAttach(vmName))
				{
					Process proc = StartHiddenFrontend(vmName);
					if (waitForRunAppExit)
					{
						proc.WaitForExit(60);
					}
				}

				return true;
			}
		}

		public static bool CheckIfGuestReady()
		{
			//we will try for 3 minutes before 
			return CheckIfGuestReady(180);
		}

		public static bool CheckIfGuestReady(int retries)
		{
			while (retries > 0)
			{
				retries--;
				try
				{
					string url = String.Format("http://127.0.0.1:{0}/{1}",
							Common.VmCmdHandler.s_ServerPort, Common.VmCmdHandler.s_CheckGuestReadyPath);
					string r = Common.HTTP.Client.Get(url, null, false, 1000);

					IJSonReader json = new JSonReader();
					IJSonObject res = json.ReadAsJSonObject(r);
					string sReceived = res["result"].StringValue;
					if (sReceived.Equals("ok"))
					{
						Logger.Info("guest finished complete booting, Guest is ready now");
						return true;
					}

					Thread.Sleep(1000);
				}
				catch (Exception e)
				{
					Logger.Error("got error while checking if Guest is booted, err: " + e.Message);
					Thread.Sleep(1000);
				}
			}
			return false;
		}

		public static void EnableService(string name, string startType)
		{
			string cmd = "sc";
			string args = String.Format("config {0} start= {1}", name, startType);
			RunCmd(cmd, args, null);
		}

		public static void StopServiceNoWait(string name)
		{
			StopServiceByName(name, true);
		}

		public static void StopService(string name)
		{
			StopServiceByName(name, false);
		}

		public static void StopServiceByName(string name, bool noWait)
		{
			try
			{
				ServiceController sc = new ServiceController(name);
				if (sc.Status == ServiceControllerStatus.Running)
				{
					Logger.Info("Stopping " + name);
					sc.Stop();
					if (!noWait)
					{
						sc.WaitForStatus(ServiceControllerStatus.Stopped, new TimeSpan(0, 0, 10));
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to stop service {0}", name);
				Logger.Error(ex.ToString());
			}
		}

		public static bool IsServiceRunning(string svcName)
		{
			ServiceController sc = new ServiceController(svcName);
			if (sc.Status == ServiceControllerStatus.Running || sc.Status == ServiceControllerStatus.StartPending)
			{
				return true;
			}
			else
			{
				return false;
			}
		}
		public static bool IsAlreadyRunning(string name)
		{
			Mutex lck;
			if (!IsAlreadyRunning(name, out lck))
			{
				lck.Close();
				return false;
			}
			return true;
		}


		public static bool IsAlreadyRunning(string name, out Mutex lck)
		{
			bool owned = false;

			try
			{
				lck = new Mutex(true, name, out owned);
			}
			catch (System.UnauthorizedAccessException exc)
			{
				lck = null;
				Logger.Error(exc.Message);
				return true;
			}

			if (!owned)
			{
				lck.Close();
				lck = null;
			}

			return !owned;
		}

		public static void MakeUpdateFsRegistryEntry()
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.AndroidKeyBasePath);
				string bootParams = "";
				if (key != null)
				{
					bootParams = (string)key.GetValue("BootParameters");
					string[] bootParamsValues = bootParams.Split(' ');
					foreach (string bootParam in bootParamsValues)
					{
						if (bootParam.Equals("UPGRADE=/dev/sde1", StringComparison.CurrentCultureIgnoreCase))
						{
							return;
						}
					}

					string upgradeValue = "UPGRADE=/dev/sde1";
					string newBootParams = bootParams + " " + upgradeValue;
					key.SetValue("BootParameters", newBootParams);
					key.Close();

					String blockDevicePath = String.Format(@"{0}\BlockDevice\4", Common.Strings.AndroidKeyBasePath);
					String updateFsPath = Path.Combine(Common.Strings.BstAndroidDir, "Update.fs");
					RegistryKey blockDeviceKey = Registry.LocalMachine.CreateSubKey(blockDevicePath);
					blockDeviceKey.SetValue("Name", "sde1");
					blockDeviceKey.SetValue("Path", updateFsPath);
					blockDeviceKey.Close();
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Got Error {0}", ex.ToString());
			}
		}


		public static String UserAgent(String tag, string version)
		{
			string userAgent = "";
			if (version == null)
				userAgent = String.Format("{0}/{1}/{2}", Version.PRODUCT, Version.STRING, tag);
			else
				userAgent = String.Format("{0}/{1}/{2}", Version.PRODUCT, version, tag);

			userAgent += " gzip";
			Logger.Debug("UserAgent = " + userAgent);

			return userAgent;
		}

		public static String UserAgent(String tag)
		{
			return UserAgent(tag, null);
		}

		public static Process StartHiddenFrontend(string vmName)
		{
			// Although the name of the function is StartHiddenFrontend, but we
			// will start it using HD-RunApp.exe
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String installDir = (string)key.GetValue("InstallDir");
			string runAppFile = Path.Combine(installDir, @"HD-RunApp.exe");

			Process runAppProc = new Process();
			runAppProc.StartInfo.UseShellExecute = false;
			runAppProc.StartInfo.CreateNoWindow = true;
			runAppProc.StartInfo.FileName = runAppFile;
			runAppProc.StartInfo.Arguments = "-v " + vmName + " -h";    // XXX: Hardcode name
			Logger.Info("Sending RunApp for vm calling {0}", vmName);

			Logger.Info("Utils: Starting hidden Frontend");
			runAppProc.Start();
			return runAppProc;
		}

		public static Process StartFrontend()
		{
			// Although the name of the function is StartFrontend, but we
			// will start it using HD-RunApp.exe
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String installDir = (string)key.GetValue("InstallDir");
			string runAppFile = Path.Combine(installDir, @"HD-RunApp.exe");

			Process runAppProc = new Process();
			runAppProc.StartInfo.UseShellExecute = false;
			runAppProc.StartInfo.CreateNoWindow = true;
			runAppProc.StartInfo.FileName = runAppFile;

			Logger.Info("Utils: Starting Frontend");
			runAppProc.Start();
			return runAppProc;
		}

		public static string GetMD5HashFromFile(string fileName)
		{
			FileStream file = new FileStream(fileName, FileMode.Open);
			MD5 md5 = new MD5CryptoServiceProvider();
			byte[] retVal = md5.ComputeHash(file);
			file.Close();

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < retVal.Length; i++)
			{
				sb.Append(retVal[i].ToString("x2"));
			}
			return sb.ToString();
		}

		public static string GetMD5HashFromString(string input)
		{
			MD5 md5 = MD5.Create();
			byte[] inputBytes = Encoding.UTF8.GetBytes(input);
			byte[] hash = md5.ComputeHash(inputBytes);

			StringBuilder sb = new StringBuilder();
			for (int i = 0; i < hash.Length; i++)
			{
				sb.Append(hash[i].ToString("X2"));
			}

			return sb.ToString();
		}

		public static string GetSystemFontName()
		{
			try
			{
				Font font = new Font("Arial", 8, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
				return "Arial";
			}
			catch (Exception)
			{
				Label lbl = new Label();
				try
				{
					Font systemFont = new Font(lbl.Font.Name, 8, FontStyle.Regular, GraphicsUnit.Point, ((byte)(0)));
				}
				catch (Exception)
				{
					if (Oem.Instance.IsMessageBoxToBeDisplayed)
					{
						MessageBox.Show("Failed to load Font set.",
								"BlueStacks instance failed.",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
					}
					Environment.Exit(-1);
				}
				return lbl.Font.Name;
			}
		}

		public static Int64 GetContentSize(
				string downloadURL
				)
		{
			HttpWebRequest request = WebRequest.Create(downloadURL) as HttpWebRequest;
			request.Method = "HEAD";

			HttpWebResponse response = request.GetResponse() as HttpWebResponse;
			Int64 length = response.ContentLength;

			response.Close();

			return length;
		}

		public static bool IsResumeSupported(
				string downloadURL
				)
		{
			HttpWebRequest request = WebRequest.Create(downloadURL) as HttpWebRequest;
			request.AddRange(0);

			request.Method = "HEAD";

			HttpWebResponse response = request.GetResponse() as HttpWebResponse;
			HttpStatusCode status = response.StatusCode;

			response.Close();

			if (status == HttpStatusCode.PartialContent)
				return true;
			else
				return false;
		}

		public static bool IsBlueStacksInstalled()
		{
			try
			{
				//				Logger.Info("Checking for existing BlueStacks installation...");
				using (RegistryKey HKLMregistry = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
				{
					string version = (string)HKLMregistry.GetValue("Version", "");
					if (String.IsNullOrEmpty(version) == false)
					{
						//						Logger.Info("BlueStacks installed");
						return true;
					}
				}
			}
			catch (Exception)
			{
				// Ignore
			}

			Logger.Info("BlueStacks not installed");
			return false;
		}

		public static int GlMode
		{
			get
			{
				String keyPath;
				keyPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
				{
					if (key == null)
						return 0;
					else
						return (int)key.GetValue("GlMode", 0);
				}
			}
		}

		public static void UpdateOEM(string newOEM)
		{
			// Update BootParameters
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.AndroidKeyBasePath);

			string val = (string)key.GetValue("BootParameters");
			string newPattern = String.Format("OEM={0}", newOEM);
			Regex regex = new Regex(@"OEM=\w+");

			string newVal = regex.Replace(val, newPattern);

			key.SetValue("BootParameters", newVal);
			key.Flush();
			key.Close();

			// Update OEM
			RegistryKey configKey = Registry.LocalMachine.CreateSubKey(Strings.HKLMConfigRegKeyPath);
			configKey.SetValue("OEM", newOEM);
			configKey.Flush();
			configKey.Close();
		}

		public static void UpdateDpi(int newDpi)
		{
			try
			{
				string dpi = newDpi.ToString();
				RegistryKey androidKey = Registry.LocalMachine.CreateSubKey(Common.Strings.AndroidKeyBasePath);
				string bootParams = (string)androidKey.GetValue("BootParameters");

				string newPattern = String.Format("DPI={0}", dpi);
				Regex regex = new Regex(@"DPI=\w+");

				string newVal = regex.Replace(bootParams, newPattern);

				androidKey.SetValue("BootParameters", newVal);
				androidKey.Close();
			}
			catch (Exception e)
			{
				Logger.Error("Failed to update dpi. Error: " + e.ToString());
			}
		}

		public static void SetGuestSize(int width, int height)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.FrameBufferRegKeyPath);
				key.SetValue("GuestWidth", width);
				key.SetValue("GuestHeight", height);
				key.Close();
			}
			catch (Exception e)
			{
				Logger.Error("Failed to set guest size. Error: " + e.ToString());
			}
		}

		public static void SetWindowSize(int width, int height)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.FrameBufferRegKeyPath);
				key.SetValue("WindowWidth", width);
				key.SetValue("WindowHeight", height);
				key.Close();
			}
			catch (Exception e)
			{
				Logger.Error("Failed to set window size. Error: " + e.ToString());
			}
		}

		public static string GetLogoFile()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)key.GetValue(Common.Strings.InstallDirKeyName);
			string logoFile = Path.Combine(installDir, Common.Strings.ProductLogoIconFile);

			if (File.Exists(logoFile) == false)
			{
				logoFile = Path.Combine(
						installDir,
						Common.Strings.ProductLogoDefaultIconFile
						);
			}
			return logoFile;
		}

		public static void AddUploadTextToImage(string inputImage, string outputImage)
		{
			Image image1 = Image.FromFile(inputImage);

			int totalWidth = image1.Width;
			int totalHeight = image1.Height + 100;

			Bitmap bmp = new Bitmap(totalWidth, totalHeight);

			Graphics finalImage = Graphics.FromImage(bmp);
			finalImage.DrawImage(image1,
					new Rectangle(0, 0, image1.Width, image1.Height),
					new Rectangle(0, 0, image1.Width, image1.Height),
					GraphicsUnit.Pixel);

			string logoFile = GetLogoFile();
			Image logoImage = Image.FromFile(logoFile);
			finalImage.DrawImage(logoImage,
					new Rectangle(65, image1.Height, 40, 40),
					new Rectangle(80, 0, logoImage.Width, 40),
					GraphicsUnit.Pixel);

			SolidBrush drawBrush = new SolidBrush(Color.White);

			float width = (float)image1.Width;
			float height = 80;  // Add some blank space at bottom to avoid Facebook's like, tag etc buttons

			RectangleF drawRect = new RectangleF(120, image1.Height + 7, width, height);
			Pen blackPen = new Pen(Color.Black);
			finalImage.DrawRectangle(blackPen, 120, image1.Height, width, height);

			string shareString = Oem.Instance.SnapShotShareString;

			finalImage.DrawString(shareString, new Font("Arial", 14), drawBrush, drawRect);
			finalImage.Save();
			image1.Dispose();

			bmp.Save(outputImage, ImageFormat.Jpeg);
		}

		public static void KillAnotherFrontendInstance()
		{
			int myId = Process.GetCurrentProcess().Id;
			string frontendExe = "HD-Frontend";
			Process[] allProcesses = Process.GetProcessesByName(frontendExe);

			foreach (Process proc in allProcesses)
			{
				if (proc.Id == myId)
				{
					Logger.Info("Ignoring process id {0}", proc.Id);
					continue;
				}

				KillProcessById(proc.Id);

			}
		}

		public static bool IsP2DMEnabled()
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Strings.AndroidKeyBasePath);
				string val = (string)key.GetValue("BootParameters");
				if (val.Contains("P2DM=1"))
				{
					return true;
				}
			}
			catch (Exception)
			{
				// Ignore
			}

			return false;
		}

		public class CmdRes
		{
			public String StdOut = "";
			public String StdErr = "";
			public int ExitCode;
		}

		public static CmdRes RunCmd(String prog, String args, String outPath)
		{
			try
			{
				return RunCmdInternal(prog, args, outPath, true);

			}
			catch (Exception exc)
			{

				Logger.Error(exc.ToString());
			}

			return new CmdRes();
		}

		public static CmdRes RunCmdNoLog(String prog, String args, String outPath)
		{
			try
			{
				return RunCmdInternal(prog, args, outPath, false);

			}
			catch (Exception exc)
			{

				Logger.Error(exc.ToString());
			}

			return new CmdRes();
		}

		public static string RunCmdNoLog(String prog, String args, int timeout)
		{
			Process proc = new Process();
			CmdRes res = new CmdRes();
			string output = "";

			proc.StartInfo.FileName = prog;
			proc.StartInfo.Arguments = args;

			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;

			proc.StartInfo.RedirectStandardInput = true;
			proc.StartInfo.RedirectStandardOutput = true;

			proc.OutputDataReceived += delegate (object obj,
					DataReceivedEventArgs line)
			{
				string stdout = line.Data;
				if (stdout != null && (stdout = stdout.Trim()) != String.Empty)
				{
					output += stdout + "\n";
				}
			};

			proc.Start();
			proc.BeginOutputReadLine();
			proc.WaitForExit(timeout);

			return output;
		}

		private static CmdRes RunCmdInternal(String prog, String args, String outPath, bool enableLog)
		{
			StreamWriter writer = null;
			Process proc = new Process();

			Logger.Info("Running Command");
			Logger.Info("    prog: " + prog);
			Logger.Info("    args: " + args);
			Logger.Info("    out:  " + outPath);

			CmdRes res = new CmdRes();

			proc.StartInfo.FileName = prog;
			proc.StartInfo.Arguments = args;

			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;

			if (outPath != null)
			{
				writer = new StreamWriter(outPath);
			}

			proc.StartInfo.RedirectStandardInput = true;
			proc.StartInfo.RedirectStandardOutput = true;
			proc.StartInfo.RedirectStandardError = true;

			proc.OutputDataReceived += delegate (object obj,
					DataReceivedEventArgs line)
			{
				if (outPath != null)
				{
					writer.WriteLine(line.Data);
				}
				string stdout = line.Data;
				if (stdout != null && (stdout = stdout.Trim()) != String.Empty)
				{
					if (enableLog)
						Logger.Info(proc.Id + " OUT: " + stdout);
					res.StdOut += stdout + "\n";
				}
			};

			proc.ErrorDataReceived += delegate (object obj,
					DataReceivedEventArgs line)
			{
				if (outPath != null)
				{
					writer.WriteLine(line.Data);
				}
				if (enableLog)
					Logger.Error(proc.Id + " ERR: " + line.Data);
				res.StdErr += line.Data + "\n";
			};

			proc.Start();
			proc.BeginOutputReadLine();
			proc.BeginErrorReadLine();
			proc.WaitForExit();
			res.ExitCode = proc.ExitCode;

			if (enableLog)
				Logger.Info(proc.Id + " ExitCode: " + proc.ExitCode);

			if (outPath != null)
			{
				writer.Close();
			}

			return res;
		}

		public static void RunCmdAsync(String prog, String args)
		{
			try
			{
				RunCmdAsyncInternal(prog, args);

			}
			catch (Exception exc)
			{

				Logger.Error(exc.ToString());
			}
		}

		private static void RunCmdAsyncInternal(String prog, String args)
		{
			Process proc = new Process();

			Logger.Info("Running Command Async");
			Logger.Info("    prog: " + prog);
			Logger.Info("    args: " + args);

			proc.StartInfo.FileName = prog;
			proc.StartInfo.Arguments = args;

			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;

			proc.Start();
		}

		public static int GetAgentServerPort()
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
				return (int)key.GetValue(Common.Strings.AgentPortKeyName, 2861);
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
				return 2861;
			}
		}

		public static int GetFrontendServerPort(string vmName)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				return (int)key.GetValue(Common.Strings.FrontendPortKeyName, 2871);
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
				return 2871;
			}
		}
		
		public static int GetBTVServerPort()
		{
			try
			{
				RegistryKey btvKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
				return (int)btvKey.GetValue("BlueStacksTVServerPort", 2885);
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
				return 2885;
			}
		}

		public static int GetPartnerServerPort()
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
				return (int)key.GetValue(Common.Strings.PartnePortKeyName, 2881);
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
				return 2881;
			}
		}

		static string partnerExePath = string.Empty;
		public static string GetPartnerExecutablePath()
		{
			try
			{
				if (string.IsNullOrEmpty(partnerExePath))
				{
					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
					partnerExePath = (string)key.GetValue(Common.Strings.PartneExePathKeyName, string.Empty);
					if (string.IsNullOrEmpty(partnerExePath))
					{
						throw new System.InvalidOperationException("Partner Exe not found");
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				partnerExePath = Path.Combine(((string)key.GetValue("InstallDir")), @"BlueStacks.exe");
			}
			return partnerExePath;
		}

		public static Process StartExe(string exePath)
		{

			Process runAppProc = new Process();
			runAppProc.StartInfo.UseShellExecute = false;
			runAppProc.StartInfo.CreateNoWindow = true;
			runAppProc.StartInfo.FileName = exePath;

			Logger.Info("Utils: Starting Frontend");
			runAppProc.Start();
			return runAppProc;
		}

		public static int Unzip(
				string filePath,
				string targetPath
				)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String installDir = (string)key.GetValue("InstallDir");

			// Start unzip to dump file info. No extracting here...
			Process dumpInfoProc = new Process();
			dumpInfoProc.StartInfo.UseShellExecute = false;
			dumpInfoProc.StartInfo.CreateNoWindow = true;
			dumpInfoProc.StartInfo.RedirectStandardOutput = true;
			dumpInfoProc.StartInfo.RedirectStandardError = true;
			dumpInfoProc.StartInfo.FileName = String.Format("\"{0}HD-unzip.exe\"", installDir);
			dumpInfoProc.StartInfo.Arguments = String.Format("-ov \"{0}\" -d \"{1}\"", filePath, targetPath);

			dumpInfoProc.OutputDataReceived += new DataReceivedEventHandler(
					delegate (object sender, DataReceivedEventArgs line)
					{
						Logger.Info(dumpInfoProc.Id + " Unzip info OUT: " + line.Data);
					});

			dumpInfoProc.ErrorDataReceived += new DataReceivedEventHandler(
					delegate (object sender, DataReceivedEventArgs line)
					{
						Logger.Error(dumpInfoProc.Id + " Unzip info ERR: " + line.Data);
					});

			Logger.Info("Starting unzip: " + dumpInfoProc.StartInfo.FileName + " " + dumpInfoProc.StartInfo.Arguments);
			dumpInfoProc.Start();
			dumpInfoProc.BeginOutputReadLine();
			dumpInfoProc.BeginErrorReadLine();
			dumpInfoProc.WaitForExit();

			Logger.Info(String.Format("{0} {1} ExitCode = {2}", dumpInfoProc.StartInfo.FileName,
						dumpInfoProc.StartInfo.Arguments, dumpInfoProc.ExitCode));

			// Start process for extracting file
			Process unzipProc = new Process();
			unzipProc.StartInfo.UseShellExecute = false;
			unzipProc.StartInfo.CreateNoWindow = true;
			unzipProc.StartInfo.RedirectStandardOutput = true;
			unzipProc.StartInfo.RedirectStandardError = true;
			unzipProc.StartInfo.FileName = String.Format("\"{0}HD-unzip.exe\"", installDir);
			unzipProc.StartInfo.Arguments = String.Format("-o \"{0}\" -d \"{1}\"", filePath, targetPath);

			Logger.Info("Starting unzip: " + unzipProc.StartInfo.FileName + " " + unzipProc.StartInfo.Arguments);

			unzipProc.OutputDataReceived += new DataReceivedEventHandler(
					delegate (object sender, DataReceivedEventArgs line)
					{
						Logger.Info(unzipProc.Id + " Unzip extract OUT: " + line.Data);
					});

			unzipProc.ErrorDataReceived += new DataReceivedEventHandler(
					delegate (object sender, DataReceivedEventArgs line)
					{
						Logger.Error(unzipProc.Id + " Unzip extract ERR: " + line.Data);
					});

			Logger.Info("Starting unzip: " + unzipProc.StartInfo.FileName + " " + unzipProc.StartInfo.Arguments);

			unzipProc.Start();
			unzipProc.BeginOutputReadLine();
			unzipProc.BeginErrorReadLine();
			unzipProc.WaitForExit();

			unzipProc.StartInfo.Arguments = String.Format("-o \"{0}\" -d \"{1}\"", filePath, targetPath);

			Logger.Info(String.Format("{0} {1} ExitCode = {2}", unzipProc.StartInfo.FileName, unzipProc.StartInfo.Arguments,
						unzipProc.ExitCode));

			return unzipProc.ExitCode;
		}

		public static void RestartBlueStacks()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)key.GetValue("InstallDir");
			ProcessStartInfo proc = new ProcessStartInfo();
			proc.FileName = installDir + "HD-Restart.exe";
			proc.Arguments = "Android";
			Process.Start(proc);

		}

		public static void GetGuestWidthAndHeight(int sWidth, int sHeight, out int width, out int height)
		{
			sHeight -= TASKBAR_HEIGHT;
			if (sWidth > 1920 && sHeight > 1080)
			{
				width = 1920;
				height = 1080;
			}
			else if (sWidth > 1600 && sHeight > 900)
			{
				width = 1600;
				height = 900;
			}
			else
			{
				width = 1280;
				height = 720;
			}

			GetCustomWidthAndHeight(ref width, ref height);
		}

		public static void GetWindowWidthAndHeight(int sWidth, int sHeight, out int width, out int height)
		{
			sHeight -= TASKBAR_HEIGHT;

			if (sWidth > 2560 && sHeight > 1440)
			{
				width = 2560;
				height = 1440;
			}
			else if (sWidth > 1920 && sHeight > 1080)
			{
				width = 1920;
				height = 1080;
			}
			else if (sWidth > 1600 && sHeight > 900)
			{
				width = 1600;
				height = 900;
			}
			else if (sWidth > 1280 && sHeight > 720)
			{
				width = 1280;
				height = 720;
			}
			else
			{
				width = 960;
				height = 540;
			}
			GetCustomWidthAndHeight(ref width, ref height);
		}

		public static void GetMinimumFEWindowSize(out int width, out int height)
		{
			width = height = -1;
			GetWindowWidthAndHeight(0, 0, out width, out height);
		}

		public static void GetCustomWidthAndHeight(ref int width, ref int height)
		{
			if (Oem.Instance.IsUseCustomResolutionIfLower)
			{
				if (width > 1024 || height > 576)
				{
					Logger.Info("Setting Custom resolution from {0}*{1} to {2}*{3}", width, height, "1024", "576");
					width = 1024;
					height = 576;
				}
			}
			if (Oem.Instance.IsResolution900600)
			{
				Logger.Info("Setting Custom resolution from {0}*{1} to {2}*{3}", width, height, "960", "600");
				width = 960;
				height = 600;
			}
		}

		public static void GetStreamWidthAndHeight(int sWidth, int sHeight, out int width, out int height)
		{
			if (sWidth >= 1920 && sHeight >= 1080)
			{
				width = 1280;
				height = 720;
			}
			else
			{
				width = 960;
				height = 540;
			}
		}

		public static void AddMessagingSupport(out Dictionary<string, string[]> oemWindowMapper)
		{
			oemWindowMapper = new Dictionary<String, String[]>();

			if (string.IsNullOrEmpty(Oem.Instance.MsgWindowClassName) == false ||
					string.IsNullOrEmpty(Oem.Instance.MsgWindowTitle) == false)
			{
				String[] windowDetails = new String[] { Oem.Instance.MsgWindowClassName, Oem.Instance.MsgWindowTitle };
				oemWindowMapper.Add(Oem.Instance.OEM, windowDetails);
			}
		}

		public static string GetURLSafeBase64String(string originalString)
		{
			string base64String = System.Convert.ToBase64String(System.Text.UTF8Encoding.UTF8.GetBytes(originalString));
			return base64String;
		}

		public static bool IsSignedByBlueStacks(string filePath)
		{
			Logger.Info("Checking if {0} is signed", filePath);
			Logger.Info("MD5 of file: " + GetMD5HashFromFile(filePath));
			bool signed = false;
			try
			{
				X509Certificate basicSigner = X509Certificate.CreateFromSignedFile(filePath);
				X509Certificate2 cert = new X509Certificate2(basicSigner);
				string issuer = cert.IssuerName.Name;
				Logger.Debug("Certificate issuer name is: " + issuer);
				string certSubject = cert.SubjectName.Name;
				Logger.Debug("Certificate issued by: " + certSubject);
				CultureInfo culture = CultureInfo.CurrentCulture;
				if (culture.CompareInfo.IndexOf(certSubject, "Bluestack Systems, Inc.", CompareOptions.IgnoreCase) >= 0)
				{
					Logger.Info("File signed by BlueStacks");
					if (cert.Verify())
					{
						Logger.Info("Certificate verified");
						signed = true;
					}
					else
					{
						Logger.Info("Certificate not verified");
					}
				}
				else
				{
					Logger.Info("File not signed by BlueStacks");
				}
			}
			catch (Exception e)
			{
				Logger.Error("File not signed");
				Logger.Error(e.ToString());
			}

			return signed;
		}

		public static string GetMimeFromFile(string filename)
		{
			string mimeType = "";
			if (!File.Exists(filename))
			{
				return mimeType;
			}

			byte[] buffer = new byte[256];
			using (FileStream fs = new FileStream(filename, FileMode.Open))
			{
				if (fs.Length >= 256)
					fs.Read(buffer, 0, 256);
				else
					fs.Read(buffer, 0, (int)fs.Length);
			}
			try
			{
				System.UInt32 mimetype;
				FindMimeFromData(0, null, buffer, 256, null, 0, out mimetype, 0);
				System.IntPtr mimeTypePtr = new IntPtr(mimetype);
				mimeType = Marshal.PtrToStringUni(mimeTypePtr);
				Marshal.FreeCoTaskMem(mimeTypePtr);
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to get mime type. err: " + ex.Message);
			}

			return mimeType;
		}

		public static bool IsSharedFolderEnabled()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			int fileSystem = (int)key.GetValue("FileSystem", 0);
			if (fileSystem == 0)
			{
				Logger.Info("Shared folders disabled");
				return false;
			}

			return true;
		}

		public static bool IsInstallTypeNCOnly()
		{
			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String installType = (String)prodKey.GetValue("InstallType");
			if (String.Compare(installType, "nconly", true) == 0)
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsGlHotAttach(string vmName)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(
					Common.Strings.HKLMAndroidConfigRegKeyPath);

			int value = (int)key.GetValue("GlHotAttach", 0);
			Logger.Debug("GlHotAttach = {0}", value);

			key.Close();
			return value != 0;
		}

		public static bool IsProcessAlive(int pid)
		{
			bool alive = false;
			try
			{
				Process.GetProcessById(pid);
				alive = true;
			}
			catch (ArgumentException)
			{
			}
			return alive;
		}

		public static bool IsProcessAlive(string lockName)
		{
			return IsProcessAlive(lockName, true);
		}
		public static bool IsProcessAlive(string lockName, bool printLog)
		{
			Mutex processLock;
			bool processRunning = IsAlreadyRunning(lockName, out processLock);
			if (processRunning)
			{
				if (printLog)
					Logger.Info(lockName + " running.");

				return true;
			}
			else
			{
				processLock.Close();
				return false;
			}
		}
		public static bool IsUIProcessAlive()
		{
			if (IsAlreadyRunning(Common.Strings.GameManagerLockName) || IsAlreadyRunning(Common.Strings.FrontendLockName))
			{
				return true;
			}
			else
			{
				return false;
			}
		}

		public static bool IsFileNullOrMissing(string file)
		{
			if (!File.Exists(file))
			{
				Logger.Info(file + " does not exist");
				return true;
			}

			FileInfo fileInfo = new FileInfo(file);
			if (fileInfo.Length == 0)
			{
				Logger.Info(file + " is null");
				return true;
			}
			else
				return false;
		}

		public static bool IsDirectoryEmpty(string dir)
		{
			bool isEmpty = true;

			if (!Directory.Exists(dir))
			{
				Logger.Info(dir + " does not exist");
				return isEmpty;
			}

			string[] files = System.IO.Directory.GetFiles(dir);
			if (files.Length == 0)
			{
				Logger.Info(dir + " is empty");
			}
			else
				isEmpty = false;

			string[] dirs = Directory.GetDirectories(dir);
			foreach (string subdir in dirs)
			{
				files = Directory.GetFiles(subdir);
				if (!IsDirectoryEmpty(subdir))
					isEmpty = false;
			}

			return isEmpty;
		}

		public static bool IsGraphicsDriverUptodate(
				out string updateUrl,
				out string msgType,
				string guid)
		{
			Logger.Info("In IsGraphicsDriverUptodate");
			updateUrl = null;
			msgType = null;

			string cfgPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
			int skipGraphicsDriverCheck = 0;
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
						cfgPath, true))
			{
				if (key != null)
					skipGraphicsDriverCheck = (int)key.GetValue("SkipGraphicsDriverCheck", 0);
			}
			switch (skipGraphicsDriverCheck)
			{
				case 0:
					break;
				case 1:
					Logger.Info("Skipping graphics driver version check");
					return true;
			}

			string url = String.Format("{0}/{1}",
					Strings.ChannelsProdUrl,
					Strings.CheckGraphicsDriverUrl);

			Dictionary<string, string> deviceInfo;
			try
			{
				deviceInfo = Profile.InfoForGraphicsDriverCheck();
			}
			catch (Exception e)
			{
				Logger.Error("Error in InfoForGraphicsDriverCheck");
				Logger.Error(e.ToString());
				// Mark driver as up-to-date in case of an exception. No point blocking the user.
				return true;
			}

			if (guid != null)
				deviceInfo.Add("guid", guid);

			Logger.Info("data being posted: ");
			foreach (KeyValuePair<string, string> entry in deviceInfo)
			{
				Logger.Info("Key: " + entry.Key + " Value: " + entry.Value);
			}

			string res;
			try
			{
				Logger.Info("Sending post request to " + url);
				res = Common.HTTP.Client.Post(
						url,
						deviceInfo,
						null,
						false);
				Logger.Info("IsGraphicsDriverUptodate response: " + res);

				IJSonReader json = new JSonReader();
				IJSonObject resjson = json.ReadAsJSonObject(res);
				if (resjson["result"].StringValue == "false")
				{
					Logger.Info("Driver out-of-date");
					updateUrl = resjson["url"].StringValue;
					msgType = resjson["msgtype"].StringValue;

					using (RegistryKey key = Registry.LocalMachine.CreateSubKey(cfgPath))
					{
						key.SetValue("DriverUrl", updateUrl);
						key.SetValue("MsgType", msgType);
					}

					return false;
				}
				else
				{
					using (RegistryKey key = Registry.LocalMachine.CreateSubKey(cfgPath))
					{
						key.DeleteValue("DriverUrl", false);
						key.DeleteValue("MsgType", false);
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Request failed");

				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(cfgPath))
				{
					if (key.GetValue("DriverUrl") != null && key.GetValue("MsgType") != null)
					{
						Logger.Info("Driver out-of-date");
						updateUrl = (string)key.GetValue("DriverUrl");
						msgType = (string)key.GetValue("MsgType");
						return false;
					}
				}

				Logger.Error(e.ToString());
				Logger.Info("Checking local data for newer driver");
				bool foundNewer = CheckLocalGraphicsDriverData(deviceInfo, out updateUrl, out msgType);
				if (foundNewer)
				{
					Logger.Info("Driver out-of-date");
					return false;
				}
			}

			Logger.Info("Could not find a newer driver");
			return true;
		}

		public static string GetUserGUID()
		{
			RegistryKey key;
			String userGUID = null;

			/*
			 * The UserGUID is in HKCU\BlueStacks in older versions
			 * Check if guid present there
			 */
			Logger.Info("Checking if guid present in HKCU");
			Logger.Info("the value of regbase path is " + Common.Strings.RegBasePath);
			using (key = Registry.CurrentUser.OpenSubKey(Common.Strings.RegBasePath))
			{
				if (key != null)
				{
					userGUID = (string)key.GetValue("USER_GUID", null);
					if (userGUID != null)
					{
						Logger.Info("Detected GUID in HKCU: " + userGUID);
						return userGUID;
					}
				}
			}

			Logger.Info("Checking if guid present in HKLM");
			using (key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
			{
				if (key != null)
				{
					userGUID = (String)key.GetValue("USER_GUID", null);
					if (userGUID != null)
					{
						Logger.Info("Detected User GUID in HKLM: " + userGUID);
						return userGUID;
					}
				}
			}

			/*
			 * We now save the guid to a file in %temp% on uninstall
			 * Check to see if this file is present. Use the guid saved in this file.
			 */
			try
			{
				Logger.Info("Checking if guid present in %temp%");
				string tmpDir = Environment.GetEnvironmentVariable("TEMP");
				Logger.Info("%TEMP% = " + tmpDir);
				string filePath = Path.Combine(tmpDir, "Bst_Guid_Backup");
				if (File.Exists(filePath))
				{
					string fileContent = System.IO.File.ReadAllText(filePath);
					if (!String.IsNullOrEmpty(fileContent))
					{
						userGUID = fileContent;
						Logger.Info("Detected User GUID %temp%: " + userGUID);
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}

			return userGUID;
		}

		public static String HostUrl
		{
			get
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey("Software");
				string host = (string)key.GetValue("BstTestUrl", Strings.ChannelsUrl);
				return host;
			}
		}

		public static String UpdatesTestUrl
		{
			get
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey("Software");
				string updaterUrl = (string)key.GetValue("BstUpdatesTestUrl", "");
				return updaterUrl;
			}
		}

		private static string GetOldPCode()
		{
			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePath = Path.Combine(tmpDir, Strings.PCodeBackUpFileName);
			string fileContent = "";

			if (File.Exists(filePath))
			{
				fileContent = System.IO.File.ReadAllText(filePath);
				if (!String.IsNullOrEmpty(fileContent))
				{
					Logger.Info(string.Format("Old PCode = {0}", fileContent));
				}
				try
				{
					File.Delete(filePath);
				}
				catch (Exception e)
				{
					Logger.Info(string.Format("Error Occured, Err: {0}", e.ToString()));
				}
			}
			return fileContent;
		}

		private static bool IsCACodeValid(string caCode)
		{
			string[] cacodesIgnore = new string[] { "4", "20", "5", "14", "8", "2", "9", "36" };
			for (int i = 0; i < cacodesIgnore.Length; i++)
			{
				if (string.Compare(cacodesIgnore[i], caCode, true) == 0)
					return false;
			}
			return true;
		}

		private static string GetOldCaCode()
		{
			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePath = Path.Combine(tmpDir, Strings.CaCodeBackUpFileName);
			string fileContent = "";

			if (File.Exists(filePath))
			{
				Logger.Info("the ca code temp file exists");
				fileContent = System.IO.File.ReadAllText(filePath);
				if (!String.IsNullOrEmpty(fileContent))
				{
					Logger.Info(string.Format("Old CaCode = {0}", fileContent));
				}
				try
				{
					File.Delete(filePath);
				}
				catch (Exception e)
				{
					Logger.Info(string.Format("Error Occured, Err: {0}", e.ToString()));
				}
			}
			if (!IsCACodeValid(fileContent))
				fileContent = "";
			return fileContent;
		}

		private static string GetOldCaSelector()
		{
			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePath = Path.Combine(tmpDir, Strings.CaSelectorBackUpFileName);
			string fileContent = "";

			if (File.Exists(filePath))
			{
				Logger.Info("the ca selector temp file exists");
				fileContent = System.IO.File.ReadAllText(filePath);
				if (!String.IsNullOrEmpty(fileContent))
				{
					Logger.Info(string.Format("Old CaSelector = {0}", fileContent));
				}
				try
				{
					File.Delete(filePath);
				}
				catch (Exception e)
				{
					Logger.Info(string.Format("Error Occured, Err: {0}", e.ToString()));
				}
			}
			return fileContent;
		}

		public static void BackUpGuid(string userGUID)
		{
			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePath = Path.Combine(tmpDir, "Bst_Guid_Backup");

			TextWriter writer = new StreamWriter(filePath);
			writer.Write(userGUID);
			writer.Close();
		}

		public static void BackUpPCode(string pCode)
		{
			Logger.Info(string.Format("backing up pCode = {0}", pCode));
			if (pCode == "")
			{
				Logger.Info("Not backing up empty pCode");
				return;
			}
			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePath = Path.Combine(tmpDir, Strings.PCodeBackUpFileName);

			TextWriter writer = new StreamWriter(filePath);
			writer.Write(pCode);
			writer.Close();
		}

		public static void DeleteCaCodePCodeTempFiles()
		{
			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePathCaCode = Path.Combine(tmpDir, Strings.CaCodeBackUpFileName);
			if (File.Exists(filePathCaCode))
			{
				Logger.Info("ca code back up time file exists");
				File.Delete(filePathCaCode);
				Logger.Info("ca code backup file deleted");
			}
			string filePathPCode = Path.Combine(tmpDir, Strings.PCodeBackUpFileName);
			if (File.Exists(filePathPCode))
			{
				Logger.Info("P code back up time file exists");
				File.Delete(filePathPCode);
				Logger.Info("P code backup file deleted");
			}
			string filePathCaSelector = Path.Combine(tmpDir, Strings.CaSelectorBackUpFileName);
			if (File.Exists(filePathCaSelector))
			{
				Logger.Info("CaSelector back up time file exists");
				File.Delete(filePathCaSelector);
				Logger.Info("CaSelector backup file deleted");
			}
		}

		public static void BackUpCaCode(string caCode)
		{
			Logger.Info(string.Format("backing up caCode = {0}", caCode));
			if (caCode == "")
			{
				Logger.Info("Not backing up empty caCode");
				return;
			}

			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePath = Path.Combine(tmpDir, Strings.CaCodeBackUpFileName);

			TextWriter writer = new StreamWriter(filePath);
			writer.Write(caCode);
			writer.Close();
		}

		public static void BackUpCaSelector(string caSelector)
		{
			Logger.Info(string.Format("backing up caSelector = {0}", caSelector));
			if (caSelector == "")
			{
				Logger.Info("Not backing up empty caSelector");
				return;
			}
			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string filePath = Path.Combine(tmpDir, Strings.CaSelectorBackUpFileName);

			TextWriter writer = new StreamWriter(filePath);
			writer.Write(caSelector);
			writer.Close();
		}

		public static void GetCodesAndCountryInfoForDeployTool(out string code, out string pcode, out string country, string locale, string upgradeDetected)
		{
			code = "840";
			pcode = "icbc";
			country = "US";

			RegistryKey key = Registry.LocalMachine.OpenSubKey("Software");
			string testCA = (string)key.GetValue("BstTestCA", "");
			string testPCode = (string)key.GetValue("BstTestPCode", "");

			if (testCA != "" && testPCode != "")
			{
				code = testCA;
				pcode = testPCode;
			}
			else
			{
				string oldPCode = GetOldPCode();
				string oldCaCode = GetOldCaCode();
				if (oldCaCode != "")
				{
					Logger.Info("the ca code taken from temp file");
					code = oldCaCode;
					pcode = oldPCode;
				}
				else
				{
					if (Oem.Instance.IsLoadDefaultCountryCode)
					{
						code = GetRandomCaCode();
					}
					else
					{
						code = "156";
					}
					Logger.Info("cacode = {0}", code);
					if (upgradeDetected == "")
					{
						pcode = GetRandomPCode();
					}
					else
					{
						pcode = "";
					}
					Logger.Info("pcode = {0}", pcode);
				}
			}

			if (Oem.Instance.IsLoadDefaultCountryCode == false)
			{
				country = "ZH";
			}
		}

		private static string GetRandomCaCode()
		{
			string[] cacodes = new string[] { "392", "036", "410", "826", "554", "372", "276", "840", "124" };
			Random rnd = new Random();
			int r = rnd.Next(cacodes.Length);
			return cacodes[r];
			/*
			   iso_3166_code = {

			   'CA': '124',

			   'US': '840',

			   'GB': '826',

			   'DE': '276',

			   'FR': '250',

			   'TW': '158',

			   'HK': '344',

			   'SG': '702',

			   'JP': '392',

			   'AU': '036',

			   'NZ': '554',

			   'IE': '372',

			   'KR': '410',

			   'BR': '076'

			   }

			   country distributions = { # % of users to be sent this code

			   'default': {
			   'CA': 4,

			   'JP': 20,

			   'AU': 5,

			   'KR': 14,

			   'GB': 8,

			   'NZ': 2,

			   'IE': 2,

			   'DE': 9,

			//'FR': 0,  France not yet to be made live

			'US': 36


			}

			}
			*/
		}

		private static string GetRandomPCode()
		{
			string[] pcodes = new string[] { "ghei", "aegj", "icbc", "ddfa" };
			Random rnd = new Random();
			int r = rnd.Next(pcodes.Length);
			return pcodes[r];
			/*
			   pcodes = {

			   "S2(GT-I9100)":"cddc",

			   "S3(GT-9300/m0)":"bagd",

			   "S4(ja3g)-N9500":"ghei",

			   "S5(klte)-G900F":"aegj",

			   "Note(N7000)":"dbhc",

			   "Note2(T03g)-N7100":"icbc",

			   "Note3(hlteatt)-N900A":"ddfa"

			   }
			 */
		}

		public static IJSonObject JSonResponseFromCloud(string locale)
		{
			string hostUrl = GetHostUrl();
			string url = String.Format("{0}/{1}", hostUrl, Strings.GetCACodeUrl);
			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("locale", locale);
			string resp = "";
			try
			{
				resp = BlueStacks.hyperDroid.Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured while fetching info from cloud...Err : " + ex.ToString());
			}
			Logger.Info("Got resp: " + resp);

			JSonReader readjson = new JSonReader();
			IJSonObject json = readjson.ReadAsJSonObject(resp);
			return json;
		}

		public static void GetCodesAndCountryInfo(out string code, out string pcode, out string country,
				out string caSelector, out string noChangesDroidG, string locale, string upgradeDetected)
		{
			code = "840";
			pcode = "icbc";
			country = "US";
			caSelector = "";
			noChangesDroidG = "";

			RegistryKey key = Registry.LocalMachine.OpenSubKey("Software");
			string testCA = (string)key.GetValue("BstTestCA", "");
			string testPCode = (string)key.GetValue("BstTestPCode", "");
			string testCaSelector = (string)key.GetValue("BstTestCaSelector", "");
			string testNoChangesDroidG = (string)key.GetValue("BstTestNoChangesDroidG", "");

			if (testCA != "" && testPCode != "")
			{
				code = testCA;
				pcode = testPCode;
				caSelector = testCaSelector;
				noChangesDroidG = testNoChangesDroidG;
			}
			else
			{
				string oldPCode = GetOldPCode();
				string oldCaCode = GetOldCaCode();
				string oldCaSelector = GetOldCaSelector();
				if (oldCaCode != "")
				{
					Logger.Info("the ca code taken from temp file");
					code = oldCaCode;
					pcode = oldPCode;
					caSelector = oldCaSelector;

					if (Oem.Instance.IsLoadCACodeFromCloud)
					{
						Logger.Info("noChangesDroidG requested from cloud");
						try
						{
							IJSonObject json = JSonResponseFromCloud(locale);
							string success = json["success"].StringValue.Trim();
							if (success == "true")
							{
								if (json.ToString().Contains("no_changes_droidg"))
									noChangesDroidG = json["no_changes_droidg"].StringValue.Trim();

								if (caSelector == "" && upgradeDetected == "" && json.ToString().Contains("ca_selector"))
									caSelector = json["ca_selector"].StringValue.Trim();
							}
						}
						catch (Exception exc)
						{
							Logger.Error(exc.Message);
						}
					}
				}
				else
				{
					if (Oem.Instance.IsLoadCACodeFromCloud)
					{

						Logger.Info("the cacode, pcode, caSelector and noChangesDroidG requested from cloud");
						try
						{
							IJSonObject json = JSonResponseFromCloud(locale);
							string success = json["success"].StringValue.Trim();
							if (success == "true")
							{
								code = json["code"].StringValue.Trim();

								if (upgradeDetected == "")
								{
									pcode = json["p_code"].StringValue.Trim();
								}
								else
								{
									pcode = "";
								}

								if (json.ToString().Contains("ca_selector"))
									caSelector = json["ca_selector"].StringValue.Trim();
								if (json.ToString().Contains("no_changes_droidg"))
									noChangesDroidG = json["no_changes_droidg"].StringValue.Trim();
							}
						}
						catch (Exception exc)
						{
							Logger.Error(exc.Message);
						}
					}
					else
					{
						if (upgradeDetected == "")
						{
							pcode = GetRandomPCode();
						}
						else
						{
							pcode = "";
						}
						code = "156";
						Logger.Info("cacode = {0} and pcode = {1}", code, pcode);

					} //china: This preprocessor is only present in case of china-msi installer
				}
			}

			if (Oem.Instance.IsCountryChina)
			{
				country = "CN";
			}
			else
			{

				try
				{
					string hostUrl = GetHostUrl();
					string url = String.Format("{0}/{1}", hostUrl, Strings.GetCountryUrl);
					string resp = BlueStacks.hyperDroid.Common.HTTP.Client.Get(url, null, false);
					Logger.Info("Got resp: " + resp);

					JSonReader readjson = new JSonReader();
					IJSonObject json = readjson.ReadAsJSonObject(resp);
					country = json["country"].StringValue.Trim();
				}
				catch (Exception e)
				{
					Logger.Info(e.Message);
				}
			} //china: This preprocessor is only present in case of china-msi installer
		}

		public static string GetHostUrl()
		{
			return BlueStacks.hyperDroid.Common.Utils.HostUrl;
		}

		/*
		 * BST_HIDE_NAVIGATIONBAR = 0x00000001;
		 * BST_HIDE_STATUSBAR = 0x00000002;
		 * BST_HIDE_BACKBUTTON = 0x00000004;
		 * BST_HIDE_HOMEBUTTON = 0x00000008;
		 * BST_HIDE_RECENTSBUTTON = 0x00000010;
		 * BST_HIDE_SCREENSHOTBUTTON = 0x00000020;
		 * BST_HIDE_TOGGLEBUTTON = 0x00000040;
		 * BST_HIDE_CLOSEBUTTON = 0x00000080;
		 * BST_HIDE_KEYMAPPINGBUTTON = 0x00000100;
		 * BST_SHOW_APKINSTALLBUTTON = 0x00000800;

		catering to changes in ROSEN apks (3rd byte)
		 * BST_HIDE_HOMEAPPNEWLOADER = 0x00010000;
		 * BST_SENDLETSGOS2PCLICKREPORT = 0x00020000;
		 * BST_DISABLE_P2DM = 0x00040000;
		 * BST_DISABLE_ARMTIPS = 0x00080000;
		 * BST_DISABLE_S2P = 0x00100000;

		frameworks/base -- related to ime(4th byte -- starting from end)
		 * BST_BAIDUIME = 0x40000000;
		 * BST_QQIME = 0x80000000;
		 * BST_QEMU_3BT_COEXISTENCE_BIT = 0x80000000;
		 */

		public static bool IsAndroidFeatureBitEnabled(uint featureBit)
		{
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.AndroidKeyBasePath);
				string bootParams = (string)key.GetValue("BootParameters");
				uint androidFeatureBits = 0;
				string[] bootParamsParts = bootParams.Split(' ');
				foreach (string eachParam in bootParamsParts)
				{
					string[] keyValue = eachParam.Split('=');
					if (keyValue[0] == "OEMFEATURES")
					{
						androidFeatureBits = Convert.ToUInt32(keyValue[1]);
						break;
					}
				}
				Logger.Info("the android oem feature bits are" + androidFeatureBits.ToString());
				uint featureExists = androidFeatureBits & featureBit;
				if (featureExists == 0)
				{
					return false;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Got error while checking for android bit, err:{0}", e.ToString());
				return false;
			}
			Logger.Info("we are returning true");
			return true;
		}

		public static void SetImeSelectedInReg(string imeSelected)
		{
			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			if (key != null)
			{
				Logger.Info("Setting the following ime in registry {0}", imeSelected);
				key.SetValue("ImeSelected", imeSelected);
				key.Close();
			}
		}

		public static bool IsLatinImeSelected()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
			if (key != null)
			{
				string imeSelected = (string)key.GetValue("ImeSelected", "");
				if (imeSelected.Equals(Common.Strings.LatinImeId, StringComparison.CurrentCultureIgnoreCase) == true)
				{
					Logger.Info("LatinIme is selected");
					return true;
				}
				if (imeSelected == "")
				{
					try
					{
						Logger.Info("the ime selected in registry is null, query currentImeId");
						string path = "getCurrentImeId";
						string url = String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, path);
						Logger.Info("the url is {0}", url);
						string response = Common.HTTP.Client.Get(url, null, false, 1000);
						Logger.Info("response we get for setting LatinIme is {0}", response);
						IJSonReader json = new JSonReader();
						IJSonObject res = json.ReadAsJSonObject(response);
						imeSelected = res["currentIme"].StringValue;
						Logger.Info("The imeselected we get from currentImeId is {0}", imeSelected);
						if (imeSelected.Equals(Common.Strings.LatinImeId, StringComparison.CurrentCultureIgnoreCase) == true)
						{
							SetImeSelectedInReg(imeSelected);
							return true;
						}
					}
					catch (Exception ex)
					{
						Logger.Error("Got exception in checking CurrentImeSelected, ex : {0}", ex.ToString());
					}
				}
			}
			return false;
		}

		public static bool IsForcePcImeForLang(string locale)
		{
			//make a list if more languages are added
			if (locale.Equals("vi-VN"))
			{
				Logger.Info("the system locale is vi-vn, using pcime workflow");
				return true;
			}
			return false;
		}

		public static bool IsEastAsianLanguage(string lang)
		{
			List<string> eastAsianLanguages = new List<string>();
			eastAsianLanguages.Add("zh-CN");
			eastAsianLanguages.Add("ja-JP");
			eastAsianLanguages.Add("ko-KR");
			if (eastAsianLanguages.Contains(lang))
				return true;

			return false;
		}

		public static bool SecurePackagesInstalled(out string securePackages)
		{
			securePackages = "";

			try
			{
				string serviceName = Common.Strings.AndroidServiceName;
				ServiceController sc = new ServiceController(serviceName);
				if (sc.Status == ServiceControllerStatus.Stopped || sc.Status == ServiceControllerStatus.StopPending)
				{
					int serviceStoppedGracefully = 0;
					String cfgPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
					using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
								cfgPath, true))
					{
						serviceStoppedGracefully = (int)key.GetValue("ServiceStoppedGracefully", 0);
						key.Flush();
					}
					if (serviceStoppedGracefully == 0)
					{
						Logger.Info("Service not stopped gracefully. Can't show app list to user. Will upgrade.");
						return false;
					}

					Logger.Info("ThinInstaller: Starting service");
					sc.Start();
					sc.WaitForStatus(ServiceControllerStatus.Running);
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				Logger.Info("Error when trying to start service. Will upgrade.");
				return false;
			}

			bool bootCompleted = Utils.WaitForBootComplete();
			if (!bootCompleted)
			{
				Logger.Info("Taking too long to boot. Will upgrade.");
				return false;
			}

			securePackages = Utils.GetSecurePackages();
			if (securePackages.Trim() == "")
			{
				Logger.Info("No secure packages. Will upgrade");
				return false;
			}

			return true;
		}

		public static int GetAdbPort()
		{
			int adbPort = 5555;
			try
			{
				RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				adbPort = (int)configKey.GetValue("bstadbport", 5555);
				configKey.Close();
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
				Logger.Info("will use 5555 as default");
			}
			return adbPort;
		}

		public static string GetSecurePackages()
		{
			int adbPort = GetAdbPort();

			String adbHost = String.Format("localhost:{0}", adbPort);

			String mInstallDir;
			using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
			{
				mInstallDir = (String)key.GetValue("InstallDir");
			}

			String adbPath = Path.Combine(mInstallDir, "HD-Adb.exe");

			RunCmdAsync(adbPath, "start-server");
			Thread.Sleep(3000);

			RunCmdAsync(adbPath, String.Format("connect {0}", adbHost));
			Thread.Sleep(250);

			string args = String.Format("-s {0} shell pm list packages -f | grep -i mnt | cut -d = -f 2", adbHost);
			string securePackages = "";
			try
			{
				CmdRes cmdRes = RunCmd(adbPath, args, null);
				securePackages = cmdRes.StdOut;
			}
			catch (Exception e)
			{
				Logger.Info(e.ToString());
				throw e;
			}

			return securePackages;
		}

		public static bool WaitForSyncConfig(string vmName)
		{
			int retries = 50;
			while (retries > 0)
			{
				retries--;
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GetHKLMAndroidConfigRegKeyPath(vmName));
				int configSynced = (int)key.GetValue("ConfigSynced", 0);
				if (configSynced == 0)
				{
					Logger.Info("Config not sycned, wait 1 second and try again");
					Thread.Sleep(1000);
				}
				else
				{
					Logger.Info("Config is synced now");
					return true;
				}
			}
			return false;
		}

		public static bool WaitForFrontendPingResponse(string vmName)
		{
			int retries = 50;
			string url;
			while (retries > 0)
			{
				retries--;
				try
				{
					url = String.Format("http://127.0.0.1:{0}/{1}",
							Common.Utils.GetFrontendServerPort(vmName),
							Common.Strings.PingPath);

					Common.HTTP.Client.Get(url, null, false, 1000);
					Logger.Info("Frontend Server Running");
					return true;
				}
				catch (Exception)
				{
					Logger.Info("Frontend Server Not Running.");
					Thread.Sleep(1000);
				}
			}

			return false;
		}

		public static bool WaitForAgentPingResponse()
		{
			int retries = 50;
			string url;
			while (retries > 0)
			{
				retries--;
				try
				{
					url = String.Format("http://127.0.0.1:{0}/{1}",
							Common.Utils.GetAgentServerPort(),
							Common.Strings.PingPath);

					Common.HTTP.Client.Get(url, null, false, 1000);
					Logger.Info("Agent Server Running");
					return true;
				}
				catch (Exception)
				{
					Logger.Info("Agent Server Not Running.");
					Thread.Sleep(1000);
					if (retries <= 40 && IsProcessAlive(Common.Strings.HDAgentLockName) == false)
					{
						return false;
					}
				}
			}

			return false;
		}

		public static bool WaitForBootComplete()
		{
			return WaitForBootComplete(120); //2 minutes
		}

		public static bool WaitForBootComplete(int retries)
		{
			lock (sWaitForBootCompleteLock)
			{
				sIsWaitLockExist = true;
				while (retries > 0)
				{
					retries--;
					if (IsGuestBooted())
					{
						sIsWaitLockExist = false;
						return true;
					}
					else
						Thread.Sleep(1000);
				}
				sIsWaitLockExist = false;
			}

			return false;
		}

		public static bool IsGuestBooted()
		{
			string url = String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort,
					Common.VmCmdHandler.s_PingPath);

			try
			{
				Common.HTTP.Client.Get(url, null, false, 1000);
				Logger.Info("Guest finished booting");
				return true;
			}
			catch (Exception ex)
			{
				Logger.Info("Guest not booted yet." + ex.Message);
			}

			return false;
		}

		private static bool CheckLocalGraphicsDriverData(Dictionary<string, string> deviceInfo, out string updateUrl, out string msgType)
		{
			bool foundNewer = false;
			updateUrl = null;
			msgType = null;

			try
			{
				GraphicsDriverData graphicsDriverData = new GraphicsDriverData();
				foundNewer = graphicsDriverData.FindDriver(deviceInfo, out updateUrl, out msgType);
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}
			return foundNewer;
		}

		public static int ForceVMLegacyMode
		{
			get
			{
				String keyPath;
				keyPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(keyPath))
				{
					if (key == null)
						return 0;
					else
						return (int)key.GetValue("ForceVMLegacyMode", 0);
				}
			}
		}

		public static void ExtractImages(string targetDir, string resourceName)
		{
			try
			{
				// Delete directory to get rid of older images
				Directory.Delete(targetDir, true);
			}
			catch (Exception)
			{
				// Ignore
			}

			if (!Directory.Exists(targetDir))
			{
				Directory.CreateDirectory(targetDir);
			}

			System.Resources.ResourceManager resources;
			try
			{
				resources = new System.Resources.ResourceManager(resourceName, System.Reflection.Assembly.GetExecutingAssembly());
			}
			catch (Exception e)
			{
				Logger.Error("Failed to extract resources. err: " + e.ToString());
				return;
			}

			Image img;

			img = (System.Drawing.Image)(resources.GetObject("bg"));
			img.Save(Path.Combine(targetDir, "bg.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);

			bool homeScreenPresent = true;
			try
			{
				img = (System.Drawing.Image)(resources.GetObject("HomeScreen"));
				img.Save(Path.Combine(targetDir, "HomeScreen.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
			}
			catch (Exception)
			{
				homeScreenPresent = false;
			}

			try
			{
				img = (System.Drawing.Image)(resources.GetObject("ThankYouImage"));
				img.Save(Path.Combine(targetDir, "ThankYouImage.jpg"), System.Drawing.Imaging.ImageFormat.Jpeg);
			}
			catch (Exception)
			{
				// ignore
			}

			// We don't know the number of images, just use an infinite loop to extract all the images
			int index = 0;
			try
			{
				while (true)
				{
					index++;

					img = (System.Drawing.Image)(resources.GetObject("SetupImage" + Convert.ToString(index)));
					img.Save(Path.Combine(targetDir, "SetupImage" + Convert.ToString(index) + ".jpg"),
							System.Drawing.Imaging.ImageFormat.Jpeg);

					if (homeScreenPresent == false && index == 1)
					{
						img.Save(Path.Combine(targetDir, "HomeScreen.jpg"),
								System.Drawing.Imaging.ImageFormat.Jpeg);
					}
				}
			}
			catch (Exception)
			{
				// Logger.Info("Could not extract installer image. Failed at index {0}. Err: {1}", index, ex.Message);
				// Ignore.
			}
		}

		public static string DownloadIcon(string iconPath, string packageName)
		{
			try
			{
				string url = "http://opasanet.appspot.com/op/appinfo?id=" + packageName;
				Logger.Info("Downloading app info from url: " + url);

				string jsonResp = Common.HTTP.Client.Get(url, null, true);

				JSonReader readJson = new JSonReader();
				IJSonObject internalJson = readJson.ReadAsJSonObject(jsonResp);
				string internalJsonString = internalJson["json"].ToString();

				readJson = new JSonReader();
				IJSonObject mainObj = readJson.ReadAsJSonObject(internalJsonString);
				string iconUrl = mainObj["icon_url"].StringValue.Trim();

				Logger.Info("Downloaded app icon from url: " + iconUrl);
				WebClient webClient = new WebClient();
				webClient.DownloadFile(iconUrl, iconPath);
				Logger.Info("Downloaded app icon at: " + iconPath);
			}
			catch (Exception e)
			{
				Logger.Error("Failed to download icon from web. Err: " + e.ToString());
				iconPath = null;
			}

			return iconPath;
		}

		public static string GetDNS2Value(string oem)
		{
			string dns2 = "8.8.8.8";

			if (string.Compare(oem, "tc_dt", true) == 0 ||
					string.Compare(oem, "china", true) == 0 ||
					string.Compare(oem, "china_api", true) == 0 ||
					string.Compare(oem, "ucweb_dt", true) == 0 ||
					string.Compare(oem, "4399", true) == 0 ||
					string.Compare(oem, "anquicafe", true) == 0 ||
					string.Compare(oem, "yy_dt", true) == 0)
			{
				dns2 = "114.114.114.114";
			}

			return dns2;
		}

		public static void ExitAgent()
		{
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			int agentPort = (int)configKey.GetValue("AgentServerPort", 2861);

			string url = String.Format("http://127.0.0.1:{0}/{1}",
					agentPort, Common.Strings.ExitAgentUrl);

			try
			{
				Common.HTTP.Client.Get(url, null, false, 3000);
			}
			catch (Exception e)
			{
				Logger.Error("Exception in ExitAgent");
				Logger.Error(e.ToString());

				KillProcessByName("HD-Agent");
			}
		}

		public static bool IsInstallOrUpgradeRequired()
		{
			if (IsBlueStacksInstalled() == false)
			{
				return true;
			}

			RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string version = (string)prodKey.GetValue("Version", "");
			if (String.IsNullOrEmpty(version) == false)
			{
				// Get rid of revision number for upgrade check by setting it same (ZERO) for both versions
				string version1 = version.Substring(0, version.LastIndexOf('.')) + ".0";
				string version2 = Version.STRING.Substring(0, Version.STRING.LastIndexOf('.')) + ".0";

				System.Version installedVersion = new System.Version(version1);
				System.Version newVersion = new System.Version(version2);

				Logger.Info("Installed Version: {0}, new version: {1}", version, Version.STRING);

				if (newVersion > installedVersion)
				{
					Logger.Info("IMP: lower version: {0} is already installed. Forcing upgrade.", version);
					return true;
				}
				else
				{
					return false;
				}
			}

			return true;
		}

		public static void SendBrowserVersionStats(string version)
		{
			Thread t = new Thread(delegate ()
			{
				try
				{
					string guid = "unknown";
					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
					if (key != null)
					{
						guid = (String)key.GetValue("USER_GUID", "");
					}

					string url = "https://bluestacks-cloud.appspot.com/stats/ieversionstats";

					Dictionary<String, String> data = new Dictionary<String, String>();
					data.Add("ie_ver", version);
					data.Add("guid", guid);
					data.Add("prod_ver", BlueStacks.hyperDroid.Version.STRING);

					Logger.Info("Sending browser version Stats");
					string res = BlueStacks.hyperDroid.Common.HTTP.Client.Post(url, data, null, false);
					Logger.Info("Got browser version stat response: {0}", res);
				}
				catch (Exception e)
				{
					Logger.Error("Failed to send app stats. error: " + e.ToString());
				}
			});
			t.IsBackground = true;
			t.Start();
		}

		public static bool IsRemoteFilePresent(string url)
		{
			bool result = true;
			HttpWebRequest req = (HttpWebRequest)WebRequest.Create(url);
			req.Method = "Head";

			HttpWebResponse res = null;

			try
			{
				res = (HttpWebResponse)req.GetResponse();
				if ((int)res.StatusCode == 404)
				{
					result = false;
				}

				res.Close();
			}
			catch (Exception e)
			{
				result = false;
				Logger.Error("Could not make http request: " + e.ToString());
			}

			return result;
		}

		public static string ConvertToIco(string png2ico, string imagePath, string iconsDir)
		{
			Logger.Info("Converting {0}", imagePath);
			string imgName = Path.GetFileName(imagePath);
			int pos = imgName.LastIndexOf(".");
			string iconName = imgName.Substring(0, pos) + ".ico";
			string icon = Path.Combine(iconsDir, iconName);

			Process installer = new Process();
			installer.StartInfo.UseShellExecute = false;
			installer.StartInfo.CreateNoWindow = true;
			installer.StartInfo.FileName = String.Format("\"{0}\"", png2ico);
			installer.StartInfo.Arguments = String.Format("\"{0}\" \"{1}\"", icon, imagePath);
			Logger.Info(installer.StartInfo.FileName + " " + installer.StartInfo.Arguments);
			installer.Start();
			installer.WaitForExit();

			return icon;
		}

		public static void ResizeImage(string imagePath)
		{
			bool resize = false;
			Image src = Image.FromFile(imagePath);

			int width = src.Width;
			int height = src.Height;

			if (width >= 256)
			{
				int newWidth = 248;
				height = (int)((float)height / ((float)width / newWidth));
				width = newWidth;
				resize = true;
			}
			if (height >= 256)
			{
				int newHeight = 248;
				width = (int)((float)width / ((float)height / newHeight));
				height = newHeight;
				resize = true;
			}
			if (width % 8 != 0)
			{
				width = width - width % 8;
				resize = true;
			}
			if (height % 8 != 0)
			{
				height = height - height % 8;
				resize = true;
			}

			if (!resize)
			{
				src.Dispose();
				return;
			}

			Image dst = new Bitmap(width, height);

			Graphics g = Graphics.FromImage(dst);
			g.SmoothingMode = SmoothingMode.AntiAlias;
			g.InterpolationMode = InterpolationMode.HighQualityBicubic;
			g.DrawImage(src, 0, 0, dst.Width, dst.Height);

			src.Dispose();
			File.Delete(imagePath);
			dst.Save(imagePath);
			dst.Dispose();
		}

		public static DateTime FromUnixEpochToLocal(long secs)
		{
			DateTime epoch = new DateTime(1970, 1, 1, 0, 0, 0, DateTimeKind.Utc);
			return epoch.AddSeconds(secs).ToLocalTime();
		}

		public static string GetDefaultLauncher()
		{
			string defaultLauncher = "com.bluestacks.gamepophome";

			try
			{
				string path = Common.Strings.DefaultLauncherUrl;
				string url = String.Format("http://127.0.0.1:{0}/{1}", VmCmdHandler.s_ServerPort, path);
				Logger.Info("sending get to defaultlauncher");
				string res = Common.HTTP.Client.Get(url, null, false);
				Logger.Info("defaultlauncher res = " + res);
				JSonReader readjson = new JSonReader();
				IJSonObject resJson = readjson.ReadAsJSonObject(res);
				string result = resJson["result"].StringValue.Trim();
				if (result == "ok")
				{
					defaultLauncher = resJson["defaultLauncher"].StringValue.Trim();
				}
				else if (result == "error")
				{
					string reason = resJson["reason"].StringValue.Trim();
					if (reason == "no default launcher")
					{
						defaultLauncher = "none";
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}

			return defaultLauncher;
		}

		public static bool IsPackageInstalled(string package)
		{
			string version = "Unknown";
			bool installed;

			string url = String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort,
					Common.Strings.IsPackageInstalledUrl);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("package", package);
			string r = Common.HTTP.Client.Post(
					url,
					data,
					null,
					false);
			JSonReader readjson = new JSonReader();
			IJSonObject resJson = readjson.ReadAsJSonObject(r);
			string result = resJson["result"].StringValue.Trim();
			if (result == "ok")
			{
				version = resJson["version"].StringValue.Trim();
				installed = true;
			}
			else
			{
				installed = false;
			}

			Logger.Info("IsPackageInstalled({0}): {1}", package, installed);
			return installed;
		}

		public static string GetAppName(string package)
		{
			string appName;

			string url = String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort,
					Common.Strings.GetLaunchActivityNameUrl);

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("package", package);

			if (GetValueFromRequest(url, data, out appName, package) == false)
			{
				url = String.Format("http://127.0.0.1:{0}/{1}",
						Common.VmCmdHandler.s_ServerPort,
						Common.Strings.GetAppNameUrl);

				GetValueFromRequest(url, data, out appName, package);
			}

			return appName;
		}

		public static bool GetValueFromRequest(string url, Dictionary<string, string> data, out string val, string defaultVal)
		{
			string r = Common.HTTP.Client.Post(
					url,
					data,
					null,
					false);
			JSonReader readjson = new JSonReader();
			IJSonObject resJson = readjson.ReadAsJSonObject(r);
			string result = resJson["result"].StringValue.Trim();
			if (result == "ok")
				val = resJson["value"].StringValue.Trim();
			else
				val = defaultVal;

			return (result == "ok");
		}

		public static int GetSystemHeight()
		{
			return (GetSystemMetrics(SM_CYSCREEN));
		}

		public static int GetSystemWidth()
		{
			return (GetSystemMetrics(SM_CXSCREEN));
		}

		public static int GetBstCommandProcessorPort(string vmName)
		{
			int guestPort = 9999;
			try
			{
				RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
				guestPort = (int)configKey.GetValue("bstandroidport", 9999);
				configKey.Close();
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
				Logger.Info("will use 9999 as default");
			}
			return guestPort;
		}

		public static bool IsHomeApp(string appInfo)
		{
			if (appInfo.IndexOf("com.bluestacks.appmart") != -1 ||
					appInfo.IndexOf("com.android.launcher2") != -1 ||
					appInfo.IndexOf("com.uncube.launcher") != -1 ||
					appInfo.IndexOf("com.bluestacks.gamepophome") != -1)
			{
				return true;
			}
			return false;
		}

		public static bool IsValidEmail(string email)
		{
			string patternStrict = @"^(([^<>()[\]\\.,;:\s@\""]+"
						+ @"(\.[^<>()[\]\\.,;:\s@\""]+)*)|(\"".+\""))@"
						+ @"((\[[0-9]{1,3}\.[0-9]{1,3}\.[0-9]{1,3}"
						+ @"\.[0-9]{1,3}\])|(([a-zA-Z\-0-9]+\.)+"
						+ @"[a-zA-Z]{2,}))$";

			Regex reStrict = new Regex(patternStrict);

			return reStrict.IsMatch(email);
		}

		public static string GetFileURI(string path)
		{
			Uri fileUri = new Uri(path);
			return fileUri.AbsoluteUri;
		}

		public static string PostToBstCmdProcessorAfterServiceStart(string path, Dictionary<string, string> data, string vmName)
		{
			return PostToBstCmdProcessorAfterServiceStart(path, data, false, vmName);
		}

		public static string PostToBstCmdProcessorAfterServiceStart(string path, Dictionary<string, string> data, bool waitForRunAppExit, string vmName)
		{
			int port = Utils.GetBstCommandProcessorPort(vmName);
			Logger.Info("HTTPHandler: Sending post request to http://127.0.0.1:{0}/{1}", port, path);
			string url = String.Format("http://127.0.0.1:{0}/{1}", port, path);

			string res = null;
			bool serviceAlreadyRunning = Common.Utils.StartServiceIfNeeded(waitForRunAppExit, vmName);
			if (serviceAlreadyRunning == false)
				Utils.WaitForBootComplete();

			if (!Common.Utils.IsUIProcessAlive())
			{
				Logger.Info("Starting Frontend in hidden mode.");
				Process proc = Common.Utils.StartHiddenFrontend(vmName);
				if (waitForRunAppExit)
				{
					proc.WaitForExit(60);
				}
			}

			try
			{
				res = Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception e)
			{
				Logger.Error("Exception in PostAfterServiceStart");
				Logger.Error(e.Message);
			}
			return res;
		}

		public static string GetToBstCmdProcessorAfterServiceStart(string path, bool waitForRunAppExit, string vmName)
		{
			int port = Utils.GetBstCommandProcessorPort(vmName);
			Logger.Info("HTTPHandler: Sending get request to http://127.0.0.1:{0}/{1}", port, path);
			string url = String.Format("http://127.0.0.1:{0}/{1}", port, path);

			string res = null;
			bool serviceAlreadyRunning = Common.Utils.StartServiceIfNeeded(waitForRunAppExit, vmName);
			if (serviceAlreadyRunning == false)
				Utils.WaitForBootComplete();

			if (!Common.Utils.IsUIProcessAlive())
			{
				Logger.Info("Starting Frontend in hidden mode.");
				Process proc = Common.Utils.StartHiddenFrontend(vmName);
				if (waitForRunAppExit)
				{
					proc.WaitForExit(60);
				}
			}

			try
			{
				res = Common.HTTP.Client.Get(url, null, false);
			}
			catch (Exception e)
			{
				Logger.Error("Exception in GetAfterServiceStart");
				Logger.Error(e.Message);
			}
			return res;
		}

		public static bool IsAppInstalled(string package, string vmName, out string version)
		{
			string failReason = "";
			version = "";
			return IsAppInstalled(package, vmName, out version, out failReason);
		}

		public static bool IsAppInstalled(string package, string vmName, out string version, out string failReason)
		{
			Logger.Info("Utils: IsAppInstalled Called for package {0}", package);
			version = "";
			failReason = "App not installed";
			bool isInstalled = false;
			try
			{
				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("package", package);
				string r = PostToBstCmdProcessorAfterServiceStart(
						Common.Strings.IsPackageInstalledUrl,
						data,
						true,
						vmName);

				Logger.Info("Got response: {0}", r);
				if (String.IsNullOrEmpty(r) == true)
				{
					failReason = "The Api failed to get a response";
				}
				else
				{
					JSonReader readjson = new JSonReader();
					IJSonObject resJson = readjson.ReadAsJSonObject(r);
					string result = resJson["result"].StringValue.Trim();
					if (String.Compare(result, "ok", true) == 0)
					{
						isInstalled = true;
						version = resJson["version"].StringValue.Trim();
					}
					else if (String.Compare(result, "error", true) == 0)
					{
						failReason = resJson["reason"].StringValue.Trim();
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(String.Format("Error Occured, Err: {0}", e.ToString()));
				failReason = e.Message;
			}
			Logger.Info("Installed = {0}", isInstalled);
			return isInstalled;
		}

		private static string FilterSystemApps(IJSonObject packages)
		{
			String[] filterPackagePrefixes = new String[] {
				"com.bluestacks",
				"com.android",
				"com.google"
			};

			String[] filterPackages = new String[] {
				"com.example.android.notepad",
				"appca.st.bigscreen",
				"android",
				"com.pop.store",
				"com.svox.pico",
				"com.location.provider"
			};

			JSonWriter jsonWriter = new JSonWriter();
			jsonWriter.WriteArrayBegin();
			for (int i = 0; i < packages.Length; i++)
			{
				bool include = true;
				string package = packages[i]["package"].StringValue.Trim();
				string version = packages[i]["version"].StringValue.Trim();
				string appname = packages[i]["appname"].StringValue.Trim();
				for (int j = 0; j < filterPackagePrefixes.Length; j++)
				{
					if (package.StartsWith(filterPackagePrefixes[j]) == true)
					{
						include = false;
						break;
					}
				}

				if (include == true)
				{
					for (int j = 0; j < filterPackages.Length; j++)
					{
						if (String.Compare(package, filterPackages[j], true) == 0)
						{
							include = false;
							break;
						}
					}
				}
				if (include == true)
				{
					jsonWriter.WriteObjectBegin();
					jsonWriter.WriteMember("package", package);
					jsonWriter.WriteMember("version", version);
					jsonWriter.WriteMember("appname", appname);
					jsonWriter.WriteObjectEnd();
				}
			}
			jsonWriter.WriteArrayEnd();
			return jsonWriter.ToString();
		}

		public static string GetInstalledPackages(string vmName, out string failReason)
		{
			Logger.Info("Utils: GetInstalledPackages Called for VM: {0}", vmName);
			failReason = "Unable to get list of installed apps";
			string jsonAppList = "";
			try
			{
				string r = GetToBstCmdProcessorAfterServiceStart(
						Common.Strings.GetInstalledPackagesUrl,
						true,
						vmName);

				Logger.Info("Got response: {0}", r);
				if (String.IsNullOrEmpty(r) == true)
				{
					failReason = "The Api failed to get a response";
				}
				else
				{
					JSonReader readjson = new JSonReader();
					IJSonObject resJson = readjson.ReadAsJSonObject(r);
					string result = resJson["result"].StringValue.Trim();
					if (String.Compare(result, "ok", true) == 0)
					{
						failReason = "";
						jsonAppList = FilterSystemApps(resJson["installed_packages"]);
						Logger.Info("Filtered results: {0}", jsonAppList);
					}
					else if (String.Compare(result, "error", true) == 0)
					{
						failReason = resJson["reason"].StringValue.Trim();
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error(String.Format("Error Occured, Err: {0}", e.ToString()));
				failReason = e.Message;
			}
			return jsonAppList;
		}

		public static bool ParseHypervisorLogsForFailures(DateTime launchTime, out string reason, out int exitCode)
		{
			reason = "";
			exitCode = 9999;
			Logger.Info("Checking if failure in hypervisor logs");
			try
			{
				string line;
				int counter = 0;
				DateTime logTime;

				using (FileStream stream = File.Open(Path.Combine(Common.Strings.BstCommonAppData, @"Logs\Hypervisor.log"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (StreamReader hdFileReader = new StreamReader(stream))
					{
						while ((line = hdFileReader.ReadLine()) != null)
						{
							if (line.Length < 26 ||
									DateTime.TryParse(line.Substring(0, 26), out logTime) == false)
							{
								continue;
							}

							if (DateTime.Compare(launchTime, logTime) > 0)
							{
								counter++;
								continue;
							}

							do
							{
								if (line.IndexOf(Common.Strings.VMXBitIsOn, StringComparison.CurrentCultureIgnoreCase) != -1)
								{
									reason = "VMX bit on";
									exitCode = 1;
									hdFileReader.Close();
									return true;
								}
							} while ((line = hdFileReader.ReadLine()) != null);
							break;
						}
					}
				}

				string oldHdFile = Path.Combine(Common.Strings.BstCommonAppData, @"Logs\Hypervisor-0.log");
				if (File.Exists(oldHdFile) && counter == 0)
				{
					using (FileStream stream = File.Open(oldHdFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						using (StreamReader hdFileReader = new StreamReader(stream))
						{
							while ((line = hdFileReader.ReadLine()) != null)
							{
								if (line.Length < 26 ||
										DateTime.TryParse(line.Substring(0, 26), out logTime) == false)
								{
									continue;
								}

								if (DateTime.Compare(launchTime, logTime) > 0)
									continue;

								do
								{
									if (line.IndexOf(Common.Strings.VMXBitIsOn, StringComparison.CurrentCultureIgnoreCase) != -1)
									{
										reason = "VMX bit on";
										exitCode = 1;
										hdFileReader.Close();
										return true;
									}
								} while ((line = hdFileReader.ReadLine()) != null);
								break;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
			}
			return false;
		}

		public static bool UnsupportedProcessor()
		{
			try
			{
				Logger.Info("Checking if Processor Unsupported");
				string[] unsupportedProcessors = new string[] {
					"AMD64 Family 21 Model 16 Stepping 1 AuthenticAMD"
				};

				string tempDir = Path.GetTempPath();
				string tempFile = Path.Combine(tempDir, "SystemInfo.txt");
				if (File.Exists(tempFile))
					File.Delete(tempFile);

				RunCmd("SystemInfo", null, tempFile);
				string sysInfo = File.ReadAllText(tempFile);
				foreach (string processor in unsupportedProcessors)
				{
					if (sysInfo.IndexOf(processor) != -1)
					{
						return true;
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
			}
			return false;
		}

		public static bool IsFrontendCrashReasonknown(DateTime launchTime, out string reason, out int exitCode)
		{
			exitCode = -1;
			bool reasonDetected = ParseBlueStacksUsersLogsForFailures(launchTime, out reason, out exitCode);

			return reasonDetected;
		}

		public static bool IsBootFailureReasonknown(DateTime launchTime, out string reason, out int exitCode)
		{
			exitCode = 9999;
			bool reasonDetected = ParseCoreLogsForFailures(launchTime, out reason, out exitCode);
			if (!reasonDetected)
				reasonDetected = ParseHypervisorLogsForFailures(launchTime, out reason, out exitCode);

			return reasonDetected;
		}

		public static bool ParseBlueStacksUsersLogsForFailures(DateTime launchTime, out string reason, out int exitCode)
		{
			reason = "";
			exitCode = -1;
			Logger.Info("Checking if failure in BlueStacksUser logs");
			try
			{
				string line;
				int counter = 0;
				DateTime logTime;

				using (FileStream stream = File.Open(Path.Combine(Common.Strings.BstCommonAppData, @"Logs\BlueStacksUsers.log"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (StreamReader userFileReader = new StreamReader(stream))
					{
						while ((line = userFileReader.ReadLine()) != null)
						{
							if (line.Length < 26 ||
									DateTime.TryParse(line.Substring(0, 26), out logTime) == false)
							{
								continue;
							}
							Logger.Debug("launchTime = {0} and logTime = {1}, compare = {2}", launchTime, logTime, DateTime.Compare(launchTime, logTime));

							if (DateTime.Compare(launchTime, logTime) > 0)
							{
								counter++;
								continue;
							}

							do
							{
								if (line.IndexOf(Common.Strings.PgaCtlInitFailedString, StringComparison.CurrentCultureIgnoreCase) != -1)
								{
									reason = "PgaCtlInitFailed, Access Violation";
									exitCode = 6;
									userFileReader.Close();
									return true;
								}
							} while ((line = userFileReader.ReadLine()) != null);
							break;
						}
					}
				}

				string oldUserFile = Path.Combine(Common.Strings.BstCommonAppData, @"Logs\BlueStacksUsers-0.log");
				if (File.Exists(oldUserFile) && counter == 0)
				{
					using (FileStream stream = File.Open(oldUserFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						using (StreamReader userFileReader = new StreamReader(stream))
						{
							while ((line = userFileReader.ReadLine()) != null)
							{
								if (line.Length < 26 ||
										DateTime.TryParse(line.Substring(0, 26), out logTime) == false)
								{
									continue;
								}

								if (DateTime.Compare(launchTime, logTime) > 0)
									continue;

								do
								{
									if (line.IndexOf(Common.Strings.PgaCtlInitFailedString, StringComparison.CurrentCultureIgnoreCase) != -1)
									{
										reason = "PgaCtlInitFailed, Access Violation";
										exitCode = 6;
										userFileReader.Close();
										return true;
									}
								} while ((line = userFileReader.ReadLine()) != null);
								break;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
			}

			return false;
		}

		public static void CheckForHDQuitRunning()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath, true);
			string hdQuitRunningStatus = (string)key.GetValue("HDQuitStatus", null);
			if (hdQuitRunningStatus != null)
			{
				//Timeout for 5 sec to make sure hd quit running
				int hdQuitTimeout = 10, count = 0;
				while (hdQuitRunningStatus != null && count < hdQuitTimeout)
				{
					Logger.Info("Waiting for HD-Quit to complete...");
					Thread.Sleep(500);
					hdQuitRunningStatus = (string)key.GetValue("HDQuitStatus", null);
					count++;
				}

				if (count == hdQuitTimeout)
				{
					try
					{
						key.DeleteValue("HDQuitStatus");
					}
					catch (Exception ex)
					{
						Logger.Error(ex.ToString());
					}
				}
			}
			else
			{
				Logger.Info("No HD-Quit running");
			}
		}

		public static bool ParseCoreLogsForFailures(DateTime launchTime, out string reason, out int exitCode)
		{
			reason = "";
			exitCode = 9999;
			Logger.Info("Checking if failure in Core logs");
			try
			{
				string line;
				int counter = 0;
				DateTime logTime;

				using (FileStream stream = File.Open(Path.Combine(Common.Strings.BstCommonAppData, @"Logs\Core.log"), FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
				{
					using (StreamReader coreFileReader = new StreamReader(stream))
					{
						while ((line = coreFileReader.ReadLine()) != null)
						{
							if (line.Length < 26 ||
									DateTime.TryParse(line.Substring(0, 26), out logTime) == false)
							{
								continue;
							}
							Logger.Debug("launchTime = {0} and logTime = {1}, compare = {2}", launchTime, logTime, DateTime.Compare(launchTime, logTime));

							if (DateTime.Compare(launchTime, logTime) > 0)
							{
								counter++;
								continue;
							}

							do
							{
								Logger.Debug("corelog = {0}", line);
								if (line.IndexOf(Common.Strings.KernelPanic, StringComparison.CurrentCultureIgnoreCase) != -1)
								{
									reason = "Kernel Panic";
									exitCode = 2;
									coreFileReader.Close();
									return true;
								}
								else if (line.IndexOf(Common.Strings.InvalidOpCode, StringComparison.CurrentCultureIgnoreCase) != -1 &&
										UnsupportedProcessor() == true)
								{
									reason = "Invalidopcode";
									exitCode = 3;
									coreFileReader.Close();
									return true;
								}
								else if (Regex.IsMatch(line, Common.Strings.Ext4Error) == true)
								{
									Logger.Info("{0} matches the regex", line);
									reason = "Block device corrupted";
									exitCode = 5;
									coreFileReader.Close();
									return true;
								}
							} while ((line = coreFileReader.ReadLine()) != null);
							break;
						}
					}
				}

				string oldCoreFile = Path.Combine(Common.Strings.BstCommonAppData, @"Logs\Core-0.log");
				if (File.Exists(oldCoreFile) && counter == 0)
				{
					using (FileStream stream = File.Open(oldCoreFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
					{
						using (StreamReader coreFileReader = new StreamReader(stream))
						{
							while ((line = coreFileReader.ReadLine()) != null)
							{
								if (line.Length < 26 ||
										DateTime.TryParse(line.Substring(0, 26), out logTime) == false)
								{
									continue;
								}

								if (DateTime.Compare(launchTime, logTime) > 0)
									continue;

								do
								{
									if (line.IndexOf(Common.Strings.KernelPanic, StringComparison.CurrentCultureIgnoreCase) != -1)
									{
										reason = "Kernel Panic";
										exitCode = 2;
										coreFileReader.Close();
										return true;
									}
									else if (line.IndexOf(Common.Strings.InvalidOpCode, StringComparison.CurrentCultureIgnoreCase) != -1 &&
											UnsupportedProcessor() == true)
									{
										reason = "Invalidopcode";
										exitCode = 3;
										coreFileReader.Close();
										return true;
									}
									else if (Regex.IsMatch(line, Common.Strings.Ext4Error) == true)
									{
										reason = "Block device corrupted";
										exitCode = 5;
										coreFileReader.Close();
										return true;
									}
								} while ((line = coreFileReader.ReadLine()) != null);
								break;
							}
						}
					}
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error Occured, Err: {0}", e.ToString());
			}

			return false;
		}
		public static bool ReserveHTTPPorts()
		{
			try
			{
				Logger.Info("ReserveHTTPPorts called");

				string everyone = new System.Security.Principal.SecurityIdentifier(
						"S-1-1-0").Translate(typeof(System.Security.Principal.NTAccount)).ToString();

				string progName = "netsh.exe";

				for (int port = 2861; port <= 2890; port++)
				{
					try
					{
						RunCmd(
								progName,
								String.Format("http add urlacl url=http://*:{0}/ User=\\" + "\"" + everyone + "\"", port),
								null
							  );
					}
					catch (Exception e)
					{
						Logger.Error(String.Format("Error Occured, Err: {0}", e.ToString()));
					}
				}

				//not reserving port 2891 to 2900 as it seems the way we are creating port
				//in obs has issue if set these permission
				for (int port = 2901; port <= 2910; port++)
				{
					try
					{
						RunCmd(
								progName,
								String.Format("http add urlacl url=http://*:{0}/ User=\\" + "\"" + everyone + "\"", port),
								null
							  );
					}
					catch (Exception e)
					{
						Logger.Error(String.Format("Error Occured, Err: {0}", e.ToString()));
					}
				}
				if (Oem.Instance.IsBTVBuild)
				{
					for (int port = 2911; port <= 2920; port++)
					{
						try
						{
							RunCmd(
									progName,
									String.Format("http add urlacl url=http://*:{0}/ User=\\" + "\"" + everyone + "\"", port),
									null
								  );
						}
						catch (Exception e)
						{
							Logger.Error(String.Format("Error Occured, Err: {0}", e.ToString()));
						}
					}
				}
				return true;
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				return false;
			}
		}
		public static bool CheckIfErrorLogsAlreadySent(string category, int exitCode)
		{
			Logger.Info("Checking if logs sent for category: {0} with exitcode: {1}", category, exitCode);

			lock (sLogFailureLogRegLock)
			{
				RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.FailureLogRegPath);
				string logInfo = (string)key.GetValue(category, "");
				if (String.IsNullOrEmpty(logInfo) == false)
				{
					String[] eCodes = logInfo.Split(',');
					foreach (string eCode in eCodes)
					{
						if (String.Compare(eCode, exitCode.ToString()) == 0)
						{
							Logger.Info("Logs already sent");
							return true;
						}
					}
				}
				else
				{
					key.SetValue(category, exitCode.ToString());
					Logger.Info("Logs not sent, will send this time");
					return false;
				}
				key.SetValue(category, logInfo + "," + exitCode.ToString());
				Logger.Info("Logs not sent, will send this time");
				return false;
			}
		}

		public static void RestartService(string serviceName, int timeoutMilliseconds)
		{
			Logger.Info("Restarting {0} service", serviceName);
			ServiceController service = new ServiceController(serviceName);
			try
			{
				int millisec1 = Environment.TickCount;
				TimeSpan timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds);

				service.Stop();
				service.WaitForStatus(ServiceControllerStatus.Stopped, timeout);

				int millisec2 = Environment.TickCount;
				timeout = TimeSpan.FromMilliseconds(timeoutMilliseconds - (millisec2 - millisec1));

				service.Start();
				service.WaitForStatus(ServiceControllerStatus.Running, timeout);
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
			}
		}

		public static bool CheckSupportedGlRenderMode(out int glRenderMode, out string glVendor, out string glRenderer, out string glVersion)
		{
			return CheckSupportedGlRenderMode(out glRenderMode, out glVendor, out glRenderer, out glVersion, "");
		}

		public static bool CheckSupportedGlRenderMode(out int glRenderMode, out string glVendor, out string glRenderer, out string glVersion, string blueStacksProgramFiles)
		{
			Logger.Info("In CheckSupportedGlRenderMode");

			int dxCheck;
			glRenderMode = 4;
			glVersion = "";
			glRenderer = "";
			glVendor = "";

			if (Utils.GpuWithDx9Support())
			{
				Logger.Info("Machine has gpu which runs on Dx 9 only");
				glRenderMode = 2;
				dxCheck = Utils.GetGraphicsInfo(Path.Combine(blueStacksProgramFiles, "HD-GLCheck.exe"), "2", out glVendor, out glRenderer, out glVersion);
			}
			else
			{
				Logger.Info("Checking for glRenderMode 4");
				glRenderMode = 4;
				dxCheck = Utils.GetGraphicsInfo(Path.Combine(blueStacksProgramFiles, "HD-GLCheck.exe"), "4", out glVendor, out glRenderer, out glVersion);
			}

			if (dxCheck != 0)
			{
				Logger.Info("DirectX not supported.");
				glRenderMode = -1;
				return false;
			}

			return true;
		}

		public static bool GpuWithDx9Support()
		{
			string[] gpuList = new string[] {"Mobile Intel(R) 4 Series Express Chipset Family",
				"Mobile Intel(R) 45 Express Chipset Family",
				"Mobile Intel(R) 965 Express Chipset Family",
				"Intel(R) G41 Express Chipset",
				"Intel(R) G45/G43 Express Chipset",
				"Intel(R) Q45/Q43 Express Chipset"};

			string graphicsCard = "";
			try
			{
				ManagementObjectSearcher searcher = new ManagementObjectSearcher(
						"SELECT * FROM Win32_DisplayConfiguration");
				foreach (ManagementObject mo in searcher.Get())
				{
					foreach (PropertyData property in mo.Properties)
					{
						if (property.Name == "Description")
						{
							graphicsCard = property.Value.ToString();
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Exception while runninq query. Err : ", ex.ToString());
			}

			Logger.Info("Graphics Card : " + graphicsCard);
			string cardNameLowerCase = graphicsCard.ToLower();

			if (cardNameLowerCase.Contains("intel") && cardNameLowerCase.Contains("express chipset"))
			{
				Logger.Info("graphicsCard : {0} part of the list of graphics card to be forced to dx9", graphicsCard);
				return true;
			}

			return false;
		}

		public static int GetGraphicsInfo(string prog, string args,
				out string glVendor, out string glRenderer, out string glVersion)
		{
			return GetGraphicsInfo(prog, args, out glVendor, out glRenderer, out glVersion, true);
		}
		public static int GetGraphicsInfo(string prog, string args,
				out string glVendor, out string glRenderer, out string glVersion, bool enableLogging)
		{
			Logger.Info("Will run " + prog + " with args " + args);

			string vendor = "", renderer = "", version = "";

			Process proc = new Process();
			proc.StartInfo.FileName = prog;
			proc.StartInfo.Arguments = args;
			proc.StartInfo.UseShellExecute = false;
			proc.StartInfo.CreateNoWindow = true;
			proc.StartInfo.RedirectStandardOutput = true;

			string tmpDir = Environment.GetEnvironmentVariable("TEMP");
			string outPath = Path.Combine(tmpDir, Common.Strings.InstallTimeTempGraphicsCheckFileName);
			StreamWriter writer = new StreamWriter(outPath);

			proc.OutputDataReceived += new DataReceivedEventHandler(
					delegate (object sender, DataReceivedEventArgs outLine)
					{
						string line = outLine.Data != null ? outLine.Data : "";
						if (enableLogging)
							Logger.Info(proc.Id + " OUT: " + line);

						int index;
						if (line.StartsWith("GL_VENDOR"))
						{
							index = line.IndexOf('=');
							vendor = line.Substring(index + 1).Trim();
							vendor = vendor.Replace(";", ";;");
						}

						if (line.StartsWith("GL_RENDERER"))
						{
							index = line.IndexOf('=');
							renderer = line.Substring(index + 1).Trim();
							renderer = renderer.Replace(";", ";;");
						}

						if (line.StartsWith("GL_VERSION"))
						{
							index = line.IndexOf('=');
							version = line.Substring(index + 1).Trim();
							version = version.Replace(";", ";;");
						}
						writer.WriteLine(line);
					});

			proc.Start();
			proc.BeginOutputReadLine();
			proc.WaitForExit();

			glVendor = vendor;
			glRenderer = renderer;
			glVersion = version;

			string lastLine = proc.Id + " EXIT: " + proc.ExitCode;
			Logger.Info(lastLine);
			writer.WriteLine(lastLine);
			writer.Close();

			return proc.ExitCode;
		}

		public static string GetLogDir()
		{
			string logDir = "";
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
				{
					logDir = (String)key.GetValue(Strings.LogKeyName, "");
					key.Close();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Error Occurred, Err: {0}", e.ToString());
			}
			return logDir;
		}

		public static bool CheckTwoCameraPresentOnDevice(ref bool bBothCamera)
		{
			bool bReturnVal;
			try
			{
				const string Image = "{6bdd1fc6-810f-11d0-bec7-08002be2092f}"; //for Camera
				Guid guid = new Guid("{53F56307-B6BF-11D0-94F2-00A0C91EFB8B}");

				int PnPHandle = SetupDiGetClassDevs(
						ref guid,
						IntPtr.Zero,
						IntPtr.Zero,
						ClassDevsFlags.DIGCF_PRESENT | ClassDevsFlags.DIGCF_ALLCLASSES
						);

				int result = -1;
				int DeviceIndex = 0;
				int iCameraCount = 0;
				while (result != 0)
				{
					SP_DEVINFO_DATA DeviceInfoData = new SP_DEVINFO_DATA();
					DeviceInfoData.cbSize = Marshal.SizeOf(DeviceInfoData);
					result = SetupDiEnumDeviceInfo(PnPHandle, DeviceIndex, ref DeviceInfoData);

					if (result == 1)
					{
						string classGUID = GetRegistryProperty(PnPHandle, ref DeviceInfoData, RegPropertyType.SPDRP_CLASSGUID);
						if (classGUID.Equals(Image))
						{
							iCameraCount++;
							if (iCameraCount == 2)
							{
								bBothCamera = true;
							}
						}
					}

					DeviceIndex++;

					if (bBothCamera)
					{
						Logger.Info("Both Camera present on Device");
						break;
					}
				}

				bReturnVal = true;
			}
			catch (Exception ex)
			{
				bReturnVal = false;
				Logger.Info("Exception when trying to check Camera present on Device");
				Logger.Info(ex.ToString());

			}

			return bReturnVal;
		}

		private static string GetRegistryProperty(int PnPHandle, ref SP_DEVINFO_DATA DeviceInfoData, RegPropertyType Property)
		{
			int RequiredSize = 0;
			DATA_BUFFER Buffer = new DATA_BUFFER();

			int result = SetupDiGetDeviceRegistryProperty(
					PnPHandle,
					ref DeviceInfoData,
					Property,
					IntPtr.Zero,
					ref Buffer,
					1024,
					ref RequiredSize
					);

			return Buffer.Buffer;

		}
		public static void CallApkInstaller(string apkPath, bool isSilentInstall)
		{
			Logger.Info("Installing apk : " + apkPath);
			try
			{
				RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				string installDir = (string)reg.GetValue("InstallDir");

				ProcessStartInfo psi = new ProcessStartInfo();
				psi.FileName = Path.Combine(installDir, "HD-ApkHandler.exe");
				if (isSilentInstall)
				{
					psi.Arguments = String.Format("-apk \"{0}\" -s", apkPath);
				}
				else
				{
					psi.Arguments = String.Format("-apk \"{0}\" ", apkPath);
				}
				psi.UseShellExecute = false;
				psi.CreateNoWindow = true;

				Logger.Info("Console: installer path {0}", psi.FileName);

				Process silentApkInstaller = Process.Start(psi);
				silentApkInstaller.WaitForExit();
				Logger.Info("Console: apk installer exit code: {0}", silentApkInstaller.ExitCode);
			}
			catch (Exception ex)
			{
				Logger.Info("Error Installing Apk : " + ex.ToString());
			}

		}

		public static string GetUserDataDir()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			if (key != null && key.GetValue("UserDefinedDir") != null)
				return (string)key.GetValue("UserDefinedDir");
			else
				return (string)Environment.GetFolderPath(
						Environment.SpecialFolder.CommonApplicationData);
		}

		//Reading from DataDir registry 
		public static string GetDataDir()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			if (key != null)
			{
				string dataDir = (String)key.GetValue("DataDir");
				if (dataDir == null)
				{
					dataDir = (String)key.GetValue("UserDataDir", "");
				}
				return dataDir;
			}
			return "";
		}
		public static String GetInstallStatsUrl()
		{
			return String.Format("{0}/{1}", BlueStacks.hyperDroid.Common.Utils.HostUrl, Strings.BsInstallStatsUrl);
		}
		public static Dictionary<string, string> GetUserData()
		{
			Dictionary<string, string> data = new Dictionary<string, string>();

			string version = "";
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
				{
					version = (string)key.GetValue("Version", "");
				}
			}
			catch (Exception)
			{
			}

			string email = "";
			try
			{
				using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.CloudRegKeyPath))
				{
					email = (string)key.GetValue("Email", "");
				}
			}
			catch (Exception)
			{
			}

			if (email != "")
				data.Add("email", email);

			//Find unix timestamp (seconds since 01/01/1970)
			long ticks = DateTime.UtcNow.Ticks - 621355968000000000;
			ticks /= 10000000; //Convert windows ticks to seconds
			string timestamp = ticks.ToString();
			data.Add("user_time", timestamp);

			return data;
		}

		public static int GetDPI()
		{
			Logger.Info("In Utils:GetDPI");

			Graphics g = Graphics.FromHwnd(IntPtr.Zero);
			IntPtr desktop = g.GetHdc();
			int dpi = GetDeviceCaps(desktop, (int)DeviceCap.LOGPIXELSX);

			int logicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.VERTRES);
			int physicalScreenHeight = GetDeviceCaps(desktop, (int)DeviceCap.DESKTOPVERTRES);
			float screenScalingFactor = (float)logicalScreenHeight / (float)physicalScreenHeight;
			dpi = (int)(dpi * screenScalingFactor);
			Logger.Info("DPI = {0}", dpi);
			return dpi;
		}

		public static void KillProcessByNameIgnoreDirectory(string name, string IgnoreDirectory)
		{
			Process[] procList;

			procList = Process.GetProcessesByName(name);

			foreach (Process proc in procList)
			{
				string procPath = "";
				try
				{
					procPath = proc.MainModule.FileName;
				}
				catch (Win32Exception w)
				{
					Logger.Error("got the excpetion {0}", w.ToString());
					Logger.Info("giving the exit code to start as admin");
					Environment.Exit(2);
				}
				catch (Exception e)
				{
					Logger.Error("got exception: err {0}", e.ToString());
				}
				string procDir = Directory.GetParent(procPath).ToString();

				Logger.Debug("The Process Dir is {0}", procDir);
				if (procDir.Equals(IgnoreDirectory, StringComparison.CurrentCultureIgnoreCase))
				{
					Logger.Debug("process:{0} not killed since the process Dir:{1} and Ignore Dir:{2} are same", proc.ProcessName, procDir, IgnoreDirectory);
					continue;
				}
				/*
				 * Kill the process and then wait for
				 * it to exit.
				 */

				Logger.Info("Killing PID " + proc.Id + " -> " + proc.ProcessName);
				try
				{
					proc.Kill();
				}
				catch (Exception exc)
				{
					/* bummer */
					Logger.Error(exc.ToString());
					continue;
				}

				if (!proc.WaitForExit(5000))
				{
					Logger.Info("Timeout waiting for process to die");
				}
			}
		}

		public static bool IsForegroundApplication()
		{
			bool foregroundApp = false;

			IntPtr hwnd = WindowInterop.GetForegroundWindow();
			if (hwnd != IntPtr.Zero)
			{

				uint pid = 0;

				WindowInterop.GetWindowThreadProcessId(hwnd,
						ref pid);
				if (pid == Process.GetCurrentProcess().Id)
					foregroundApp = true;
			}

			return foregroundApp;
		}


		public static bool CheckWritePermissionForFolder(string DirectoryPath)
		{
			if (string.IsNullOrEmpty(DirectoryPath)) return false;

			//try
			//{

			//FileIOPermission writePermission = new FileIOPermission(FileIOPermissionAccess.Write, DirectoryPath);
			//if (!SecurityManager.IsGranted(writePermission))
			//{
			//	return false;
			//}
			//AuthorizationRuleCollection rules = Directory.GetAccessControl(DirectoryPath).GetAccessRules(true, true, typeof(System.Security.Principal.SecurityIdentifier));
			//WindowsIdentity identity = WindowsIdentity.GetCurrent();

			//foreach (FileSystemAccessRule rule in rules)
			//{
			//	if (identity.Groups.Contains(rule.IdentityReference))
			//	{
			//		if ((FileSystemRights.Write & rule.FileSystemRights) == FileSystemRights.Write)
			//		{
			//			if (rule.AccessControlType == AccessControlType.Deny)
			//				return false;
			//			if (rule.AccessControlType == AccessControlType.Allow)
			//				return true;
			//		}
			//	}
			//}
			//}
			//catch(Exception e)
			//{
			//	Logger.Error("got exception: err {0}", e.ToString());
			//}

			return true;
		}

		public static void UpdateRegistry(string registryKey, string name, object value, RegistryValueKind kind)
		{
			try
			{
				//Logger.Info("Updating Registry" + registryKey +"~~" + name +"~~"+ value);
				RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey, true);
				key.SetValue(name, value, kind);
				key.Close();
				key.Flush();
			}
			catch (Exception ex)
			{
				Logger.Error("Exception occured in UpdateRegistry " + ex.ToString());
				throw ex;
			}
		}

		public static string GetValueFromRegistry(string registryKey, string name, object defaultValue)
		{
			string value = string.Empty;
			try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(registryKey, true);
				value = key.GetValue(name, defaultValue).ToString();
				//Logger.Info("Registry value recived " + registryKey + "~~" + name + "~~" + defaultValue.ToString() + "~~" + value);
			}
			catch (Exception ex)
			{
				value = string.Empty;
				Logger.Error("Exception occured in GetValueFromRegistry " + registryKey + "~~" + name + "~~" + defaultValue.ToString() + "~~" + ex.ToString());
			}
			return value;
		}

		public static Icon GetApplicationIcon()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)key.GetValue("InstallDir", "");
			string iconFile = Path.Combine(installDir, Common.Strings.ProductLogoIconFile);
			if (File.Exists(iconFile))
			{
				return new Icon(iconFile);
			}
			else
			{
				return Icon.ExtractAssociatedIcon(Application.ExecutablePath);
			}
		}

		public static bool IsHDPlusDebugMode()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);

			if (key != null)
			{
				int plusDebug = (int)key.GetValue("PlusDebug", 0);
				return (plusDebug == 1);
			}
			return false;
		}

		public static int GetGMStreamWindowWidth()
		{
			return BTV_RIGHT_PANEL_WIDTH * GetDPI() / DEFAULT_DPI;
		}

		public static void LogParentProcessDetails()
		{
			try
			{
				Process parentProcess = ProcessDetails.CurrentProcessParent;
				if (parentProcess == null)
				{
					Logger.Info("Unable to retrieve information about invoking process");
					return;
				}
				Logger.Info("Invoking Process Details: (Name: {0}, Pid: {1})",
						parentProcess.ProcessName,
						parentProcess.Id
					   );
			}
			catch (Exception e)
			{
				Logger.Error("Unable to get parent process details, Err: {0}", e.ToString());
			}
		}

		public static bool UpgradeAndroidUserData(string dataFsPath, string sdCardFsPath,
							string dataVdiPath, string sdCardVdiPath, bool removeSourceFiles)
		{
			String tmpDir = Environment.GetEnvironmentVariable("TEMP");
			String blockDeviceToolPath = GetInstallDir() + @"\HD-BlockDeviceTool.exe";
			Logger.Info("UpgradeUserData called.");

			bool convertSuccess = true;

			try
			{

				if (!Directory.Exists(dataFsPath) || !Directory.Exists(sdCardFsPath))
				{
					Logger.Error("Sparsfs folder not found");
					return false;
				}
				convertSuccess = Utils.ConvertSparseToVdi(blockDeviceToolPath, dataFsPath, dataVdiPath, Strings.DataVdiUUID);
				if (convertSuccess)
					convertSuccess = Utils.ConvertSparseToVdi(blockDeviceToolPath, sdCardFsPath, sdCardVdiPath, Strings.SDCardVdiUUID);

				if (convertSuccess)
				{
					if (removeSourceFiles)
					{
						try
						{
							Directory.Delete(dataFsPath, true);
						}
						catch (Exception ex)
						{
							Logger.Error("Ignoring Error: {0}", ex.ToString());
						}

						try
						{
							Directory.Delete(sdCardFsPath, true);
						}
						catch (Exception ex)
						{
							Logger.Error("Ignoring Error: {0}", ex.ToString());
						}
					}
				}
				else
				{
					Logger.Error("Failed to convert sparse files into vdi files");
					return false;
				}

			}
			catch (Exception exc)
			{
				Logger.Error(exc.ToString());
				return false;
			}

			Logger.Info("UpgradeUserData end.");
			return true;
		}

		public static bool UpgradeAndroidFiles(string srcRootFs, string srcPrebundledFs, string destRootVdi, string destPrebundledVdi, bool removeSourceFiles)
		{
			String tmpDir = Environment.GetEnvironmentVariable("TEMP");
			String blockDeviceToolPath = GetInstallDir() + @"\HD-BlockDeviceTool.exe";
			Logger.Info("UpgradeAndroidFiles called");

			bool convertSuccess = true;

			try
			{
				if (!File.Exists(srcRootFs) || !File.Exists(srcPrebundledFs))
				{
					Logger.Error("Source flat files not found");
					return false;
				}
				convertSuccess = Utils.ConvertFlatToVdi(blockDeviceToolPath, srcRootFs, destRootVdi, Strings.RootVdiUUID);
				if (convertSuccess)
					convertSuccess = Utils.ConvertFlatToVdi(blockDeviceToolPath, srcPrebundledFs, destPrebundledVdi, Strings.PrebundledVdiUUID);

				if (convertSuccess)
				{
					if (removeSourceFiles)
					{
						try
						{
							File.Delete(srcRootFs);
							File.Delete(srcPrebundledFs);
						}
						catch (Exception ex)
						{
							Logger.Error("Ignoring error...Failed to delete src root fs and prebundled fs...Err : " + ex.ToString());
						}
					}
				}
				else
				{
					Logger.Error("Failed to convert flat files to vdi files...stopping upgrade");
					return false;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed in UpgradeAndroidFiles...Err : " + e.ToString());
				return false;
			}

			Logger.Info("UpgradeAndroidFiles ended");
			return true;
		}

		public static void SetEngineRegistry(string engine)
		{
			if (engine.Equals("legacy"))
			{
				SetGlTransportValue("0");
				RegistryKey key = Registry.LocalMachine.OpenSubKey(
						BlueStacks.hyperDroid.Common.Strings.RegBasePath, true);
				key.SetValue("Engine", engine, RegistryValueKind.String);
				key.Close();
			}
			else
				SetGlTransportValue("3");

		}

		public static void SetGlTransportValue(string glTValue)
		{
			Logger.Info("Setting GlTransport: " + glTValue);
			RegistryKey key = Registry.LocalMachine.CreateSubKey(BlueStacks.hyperDroid.Common.Strings.AndroidKeyBasePath);
			if (key != null)
			{
				string bootPrms = (string)key.GetValue("BootParameters", "");
				string[] paramParts = bootPrms.Split(' ');
				string newBootParams = "";
				string paramName = "GlTransport";

				if (bootPrms.IndexOf(paramName) == -1)
				{
					newBootParams = bootPrms + " " + paramName + "=" + glTValue;
				}
				else
				{
					foreach (string param in paramParts)
					{
						if (param.IndexOf(paramName) != -1)
						{
							if (!String.IsNullOrEmpty(newBootParams))
							{
								newBootParams += " ";
							}
							newBootParams += paramName + "=" + glTValue;
						}
						else
						{
							if (!String.IsNullOrEmpty(newBootParams))
							{
								newBootParams += " ";
							}
							newBootParams += param;
						}
					}
				}
				key.SetValue("BootParameters", newBootParams);
				key.Close();
			}
		}

		public static void SetDeviceCapsRegistry(String legacyReason,
				String engine, bool cpuHvm, bool biosHvm)
		{
			if (!engine.Equals("raw") || biosHvm == true || cpuHvm == false)
			{
				try
				{
					RegistryKey vtxKey = Registry.LocalMachine.OpenSubKey(
							Common.Strings.RegBasePath, true);
					vtxKey.DeleteValue("VtxDisabled");
					vtxKey.Close();
				}
				catch (Exception ex)
				{
					Logger.Info("Key VtxDisabled not found: {0}", ex.Message);
				}
			}

			String deviceCaps = "{ " +
				"\"engine_enabled\": \"" + engine + "\", " +
				"\"legacy_reason\": \"" + legacyReason + "\", " +
				"\"cpu_hvm\": \"" + cpuHvm + "\", " +
				"\"bios_hvm\": \"" + biosHvm +
				"\"} ";
			RegistryKey key = Registry.LocalMachine.OpenSubKey(
					Common.Strings.HKLMConfigRegKeyPath, true);
			String oldDeviceCaps = (String) key.GetValue("DeviceCaps", "");
			if (!oldDeviceCaps.Equals(""))
			{
				//This code is dangerous, change is risky
				int startIndex = oldDeviceCaps.IndexOf("engine_enabled") + 17;
				int endIndex = oldDeviceCaps.IndexOf(",");
				String oldEngine = oldDeviceCaps.Substring(startIndex, endIndex - startIndex);
				Logger.Info("Old engine was {0}", oldEngine);
				if (!oldEngine.Equals(engine))
				{
					key.SetValue("SystemInfoStats2", 1, RegistryValueKind.DWord);
				}
			}
			key.SetValue("DeviceCaps", deviceCaps, RegistryValueKind.String);
			key.Close();
		}

		public static string GetGameManagerPreviousVersionDir()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(@"Software\BluestacksGameManager");
			if (key != null)
			{
				return (string)key.GetValue("InstallDir");
			}
			Logger.Info("the key bluestacks\\gamemanager is null, returning empty as installDir");
			return "";
		}
		public static string GetInstallDir()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			if (key != null)
			{
				return (string)key.GetValue("InstallDir", "");
			}
			return "";
		}

		public static string GetOemFromRegistry()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			string oem = (string)key.GetValue(Strings.OEMKeyName, String.Empty);
			key.Close();
			return oem;
		}

		public static string ToggleDisabledAppListFileUrl
		{
			get
			{
				if (Features.IsFeatureEnabled(Features.CHINA_CLOUD) == true)
				{
					return Strings.ChinaToggleDisabledAppListUrl;
				}
				return Strings.WorldWideToggleDisabledAppListUrl;
			}
		}

		public static string ToggleDisabledAppListLocation
		{
			get
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Strings.GMBasePath);
				if (key == null)
				{
					return String.Empty;
				}

				string installDir = (String)key.GetValue(Strings.GMInstallDirKeyName, String.Empty);
				key.Close();
				return Path.Combine(installDir, Strings.ToggleDisableAppListFileName);
			}
		}

		public static string RegisterComComponents(String installDir)
		{
			Logger.Info("RegisterComComponents()");

			String exePath = Path.Combine(installDir, "BstkSVC.exe");
			String dllPath = Path.Combine(installDir, "BstkC.dll");

			RegisterComExe(exePath, false);
			if (!RegisterComExe(exePath, true))
				return "RegisterComExe failed";

			RegisterComDll(dllPath, false);
			if (!RegisterComDll(dllPath, true))
				return "RegisterComDll failed";

			return null;
		}

		private static bool RegisterComExe(String path, bool register)
		{
			try
			{
				CmdRes res = RunCmd(path, register ? "/RegServer" : "/UnregServer", null);
				return res.ExitCode == 0;
			}
			catch (Exception exc)
			{
				Logger.Error("Command runner raised an exception: " + exc.ToString());
				return false;
			}
		}

		private static bool RegisterComDll(String path, bool register)
		{
			String args = null;
			if (register)
				args = "/s \"" + path + "\"";
			else
				args = "/s /u \"" + path + "\"";

			try
			{
				CmdRes res = RunCmd(Environment.ExpandEnvironmentVariables(@"%windir%\system32\regsvr32"), args, null);
				return res.ExitCode == 0;
			}
			catch (Exception exc)
			{
				Logger.Error("Command runner raised an exception: " + exc.ToString());
				return false;
			}
		}

		public static bool DeregisterComComponents(String installDir)
		{
			Logger.Info("DeregisterComComponents()");

			String exePath = Path.Combine(installDir, "BstkSVC.exe");
			String dllPath = Path.Combine(installDir, "BstkC.dll");

			RegisterComExe(exePath, false);
			RegisterComDll(dllPath, false);

			return true;
		}

		public static string[] ToggleDisableAppList
		{
			get
			{
				String[] toggleDisabledAppList;

				if (File.Exists(ToggleDisabledAppListLocation) == false ||
						File.GetLastWriteTime(ToggleDisabledAppListLocation) < DateTime.Now.AddDays(-1))
				{
					WebClient webClient = new WebClient();
					string tmpDir = Environment.GetEnvironmentVariable("TEMP");
					string tempFileLocation = Path.Combine(tmpDir, Path.GetRandomFileName());
					try
					{
						if (File.Exists(tempFileLocation) == true)
						{
							File.Delete(tempFileLocation);
						}
						/*
						 * Downloading file asynchronously. so that the tab switch time does not increase
						 * in gamemanager. It will cause the ToggleDisabledAppList to not get updated in
						 * some first cases of tab-switch operations but its better than increasing tab-switch
						 * time.
						 */
						Logger.Info("ToggleDisabledAppListFileUrl = {0}, ToggleDisabledAppListLocation = {1}",
								ToggleDisabledAppListFileUrl, ToggleDisabledAppListLocation);
						webClient.DownloadFile(ToggleDisabledAppListFileUrl, tempFileLocation);
						if (File.Exists(tempFileLocation) == false)
						{
							Logger.Info("Unable to download file.");
						}
						String fileContent = File.ReadAllText(tempFileLocation, Encoding.UTF8);
						if (fileContent != null && fileContent.Trim().Length > 0)
						{
							File.Copy(tempFileLocation, ToggleDisabledAppListLocation, true);
						}
					}
					catch (Exception e)
					{
						Logger.Error("Error Occurred while downloading ToggleDisabedAppListFile, Err: {0}", e.ToString());
					}
				}

				if (File.Exists(ToggleDisabledAppListLocation))
				{
					toggleDisabledAppList = File.ReadAllLines(ToggleDisabledAppListLocation, Encoding.UTF8);
				}
				else
				{
					toggleDisabledAppList = new String[] { };
				}
				Array.Resize(ref toggleDisabledAppList, toggleDisabledAppList.Length + 2);
				toggleDisabledAppList[toggleDisabledAppList.Length - 2] = "com.bluestacks.gamepophome";
				toggleDisabledAppList[toggleDisabledAppList.Length - 1] = "com.android.vending";

				return toggleDisabledAppList;
			}
		}

        /// <summary>
        /// System crashes with Win10 Anniversery addition if HyperV enabled, so alert message to be shown to diable HyperV
        /// </summary>
        public static bool CheckHyperVWithNestedVirtualizationEnabled()
        {
            try
            {
                //Check for Win 10
                /*if(!Utils.IsOSWin10)
                  {
                  Logger.Info("Not windows 10");
                  retrun;
                  Environment.Exit(0);
                  }*/
                string hyperVEnabled = "false";
                //string guid = Utils.GetUserGUID();
                Logger.Info("Checking HyperV Version");
                int hyperVVersion = Modes.Mode.CheckHyperV();
                if (hyperVVersion > 0)
                {
                    hyperVEnabled = "true";
                    if (hyperVVersion >= 14361)
                    {
                        Logger.Info("HYPERV with nested virtualization enabled!!!");
                        return true;
                    }
                    else
                    {
                        Logger.Info("HYPERV with nested virtualization not enabled!!!");
                    }
                }
                else
                {
                    Logger.Info("HyperV is disabled");
                }

                Logger.Info("HyperV Enabled: " + hyperVEnabled);
                Logger.Info("HyperV Version: " + hyperVVersion.ToString());

                /*string osName = "";
                  string servicePack = "";
                  string osArch = "";

                  try
                  {
                  Logger.Info("Checking for Supported OS...");
                  if (Common.Utils.GetOSInfo(out osName, out servicePack, out osArch))
                  {
                  Logger.Info("OS info: Name: {0}, Service Pack: {1}, Arch: {2}", osName, servicePack, osArch);
                  int servicePackNum = 0;
                  int.TryParse(servicePack, out servicePackNum);

                  Logger.Info("Checking for 64 bit...");
                  try
                  {
                  if (Common.Utils.IsOs64Bit())
                  {
                  Logger.Info("OS 64 bit");
                  }
                  }
                  catch (Exception e)
                  {
                  Logger.Info(string.Format("Exception Error : {0}", e.ToString()));

                  }

                  }
                  }
                  catch (Exception e)
                  {
                  Logger.Info(string.Format("Exception Error : {0}", e.ToString()));

                  }

                //SendHyperVInfoStatsSync(hyperVEnabled, hyperVVersion.ToString(), osName, servicePack);

                Logger.Info("Stats Sent");
                 */
            }
            catch (Exception e)
            {
                Logger.Info(string.Format("Exception Error : {0}", e.ToString()));
            }

            return false;
        }
    }

	public class Modes
	{
		static IMode mode = null;

		public static IMode Mode
		{
			get
			{
				if (mode == null)
				{
					IntializeModule();
				}
				return mode;
			}
		}

		private static void IntializeModule()
		{
			if (Common.Strings.IsEngineLegacy())
			{
				mode = new LegacyMode();
                Logger.Info("LegacyMode");
			}
			else
			{
				mode = new PlusMode();
                Logger.Info("PlusMode");
            }
		}
	}

	public interface IMode
	{
		string FrontendDllName();
		int CheckHyperV();
	}

	public class PlusMode : IMode
	{
		public const string FRONTEND_DLL = "HD-Plus-Frontend-Native.dll";

		string IMode.FrontendDllName()
		{
			return FRONTEND_DLL;
		}

		[DllImport(FRONTEND_DLL)]
			public static extern int CheckHyperV();

		int IMode.CheckHyperV()
		{
			return CheckHyperV();
		}
	}

	public class LegacyMode : IMode
	{
		public const string FRONTEND_DLL = "HD-Frontend-Native.dll";

		string IMode.FrontendDllName()
		{
			return FRONTEND_DLL;
		}

		[DllImport(FRONTEND_DLL)]
			public static extern int CheckHyperV();

		int IMode.CheckHyperV()
		{
			return CheckHyperV();
		}
	}
}
