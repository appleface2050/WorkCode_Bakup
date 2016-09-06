using System;
using System.IO;

using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Tool
{

    public class InstallChecker
    {

        public static string s_CommonAppData;
        public enum InstallCheckCode
        {
            SUCCESS = 0,
            WINXP64_UNSUPPORTED = 1,
            WINXPSP3_REQUIRED = 2,
            WINVISTASP2_REQUIRED = 3,
            INSUFFICIENT_DISKSPACE = 4,
            INSUFFICIENT_PHYSICALMEMORY = 5
        };

        /*
		public static int Main()
		{
			s_CommonAppData = (string)Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath).GetValue("UserDefinedDir");
			Logger.InitUserLog();

			string reason;
			int install = (int)PreInstallChecks(out reason);
			Logger.Info(install + ": " + reason);

			RegistryKey compatibilityKey = Registry.CurrentUser.CreateSubKey(Common.Strings.RegBasePath);
			compatibilityKey.SetValue("Compatible", install);
			compatibilityKey.Flush();
			compatibilityKey.Close();

			return install;
		}
		*/

        public static int PreInstallChecks(
                out string reason
                )
        {
            reason = "";
            int checkInstall = (int)InstallCheckCode.SUCCESS;

            // Operating system check
            string osName = "";
            string servicePack = "";
            string osArch = "";

            try
            {
                Logger.Info("Checking for Supported OS...");

                if (Common.Utils.GetOSInfo(out osName, out servicePack, out osArch))
                {
                    Logger.Info("OS info: Name: {0}, Service Pack: {1}, Arch: {2}", osName, servicePack, osArch);

                    int servicePackNum;
                    try
                    {
                        servicePackNum = Convert.ToInt32(servicePack);
                    }
                    catch (Exception e)
                    {
                        servicePackNum = 0;
                        Logger.Error("Error when trying to convert service pack to string: " + e.Message);
                    }

                    // XP service pack 3 is required
                    if (String.Compare(osName, "XP", true) == 0)
                    {
                        Logger.Info("Checking for 64 bit...");
                        try
                        {
                            if (Common.Utils.IsOs64Bit())
                            {
                                reason += "OS not supported. BlueStacks does not support Windows XP X64\n";
                                SetBit(ref checkInstall, (int)InstallCheckCode.WINXP64_UNSUPPORTED - 1);
                            }
                        }
                        catch (Exception ex)
                        {
                            Logger.Error("Failed to get OS Arch. err: " + ex.ToString());
                            Logger.Error("Ignoring Arch check");
                            // Ignore
                        }


                        Logger.Info("Checking for service pack 3...");
                        if (servicePackNum < 3)
                        {
                            reason += "OS not supported. BlueStacks requires windows XP SP3\n";
                            SetBit(ref checkInstall, (int)InstallCheckCode.WINXPSP3_REQUIRED - 1);
                        }
                    }

                    // Vista requiers service pack 2 or greater
                    if (String.Compare(osName, "VISTA", true) == 0)
                    {
                        if (servicePackNum < 2)
                        {
                            reason += "OS not supported. BlueStacks requires Window Vista SP2 or greater\n";
                            SetBit(ref checkInstall, (int)InstallCheckCode.WINVISTASP2_REQUIRED - 1);
                        }
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check for OS. err: " + ex.ToString());
            }

            // Disk space check
            try
            {
                Logger.Info("Checking for disk space...");

                DriveInfo driveInfo = new DriveInfo(s_CommonAppData);
                long freeSpace = (driveInfo.AvailableFreeSpace) / (1024 * 1024 * 1024);

                if (freeSpace < 2)
                {
                    reason += "Not enough disk space available. BlueStacks requires 2 GB of free disk space.\n";
                    SetBit(ref checkInstall, (int)InstallCheckCode.INSUFFICIENT_DISKSPACE - 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check disk space. err: " + ex.ToString());
            }

            // Physical Memory check
            try
            {
                Logger.Info("Checking for physical memory...");
                string ramString = Device.Profile.GetSysInfo("Select TotalPhysicalMemory from Win32_ComputerSystem");
                long ram = Int64.Parse(ramString);
                long minimumRamRequired = 1L * 1024 * 1024 * 1024;
                if (ram < minimumRamRequired)
                {
                    reason += "BlueStacks needs at least 2 GB of physical memory\n";
                    SetBit(ref checkInstall, (int)InstallCheckCode.INSUFFICIENT_PHYSICALMEMORY - 1);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check physical memory. err: " + ex.ToString());
            }

            if (checkInstall == (int)InstallCheckCode.SUCCESS)
                reason = "SUCCESS";

            return checkInstall;
        }

        private static void SetBit(ref int val, int position)
        {
            val |= (1 << position);
        }
    }
}
