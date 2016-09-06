using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Diagnostics;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using System.IO;

namespace BlueStacks.hyperDroid.Tool
{
    static class DataManagerUtils
    {

		public static void QuitBlueStacks()
		{
			Logger.Info("Quit bluestacks called");
			ProgressForm.percentDone = (int)ProgressForm.States.QuittingBlueStacks;
			RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			string installDir = (string)regKey.GetValue("InstallDir");

			string quitExePath = Path.Combine(installDir, "HD-Quit.exe");

			Logger.Info("quit exe path -- " + quitExePath);
			Process proc = new Process();
			proc.StartInfo.FileName = quitExePath;
			proc.Start();
			proc.WaitForExit();
		}

        public static void BackupData(string srcPath, string destPath)
        {
            Logger.Info("Taking data backup");

            int randomNumber = new Random().Next(0, int.MaxValue - 1 );
            string dateTime = DateTime.Now.ToString();
            string dirName = String.Format("BlueStacksBackup_{0}", randomNumber);
            string dirPath = Path.Combine(destPath, dirName);
            try
            {
				Logger.Info("Creating directory ----> " + dirPath);
                Directory.CreateDirectory(dirPath);
                //android
				Logger.Info("Copying android files");
				ProgressForm.percentDone = (int)ProgressForm.States.BackingupAndroidData;
                BackupAndroidUserData(Path.Combine(srcPath, "Android"), Path.Combine(dirPath, "Android"));

                //gadget
				ProgressForm.percentDone = (int)ProgressForm.States.BackingupUserData;
                CopyGadgetUserData(Path.Combine(srcPath, "UserData"), Path.Combine(dirPath, "UserData"));

                //gamemanager
				ProgressForm.percentDone = (int)ProgressForm.States.BackingupGameManagerData;
                CopyGameManagerUserData(Path.Combine(srcPath, "BluestacksGameManager"), Path.Combine(dirPath, "BluestacksGameManager"));

				ProgressForm.percentDone = 100;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to take backup... Err : " + ex.ToString());
                MessageBox.Show(Locale.Strings.GetLocalizedString("BackupFailedText"));
                Directory.Delete(dirPath, true);
                Environment.Exit(1);
            }
        }

        public static void BackupAndroidUserData(string srcPath, string destPath)
        {
            string srcDataVdi = Path.Combine(srcPath, "Data.vdi");
            string srcSDCardVdi = Path.Combine(srcPath, "SDCard.vdi");

            MakeDirectory(destPath);
            string destDataVdi = Path.Combine(destPath, "Data.vdi");
            string destSDCardVdi = Path.Combine(destPath, "SDCard.vdi");

            File.Copy(srcDataVdi, destDataVdi, true);
            File.Copy(srcSDCardVdi, destSDCardVdi, true);
        }

        public static void CopyGadgetUserData(string srcPath, string destPath)
        {
			Logger.Info("Copying gadget data files");
            string srcInputMapperFolderDir = Path.Combine(srcPath, "InputMapper");
            string srcGadgetDataDir = Path.Combine(srcPath, "Gadget");
            string srcAppSyncDataDir = Path.Combine(srcPath, "AppSync");
            string srcLibraryDir = Path.Combine(srcPath, "Library");
            string srcSharedFolderDir = Path.Combine(srcPath, "SharedFolder");

            MakeDirectory(destPath);
            string destInputMapperFolderDir = Path.Combine(destPath, "InputMapper");
            string destGadgetDataDir = Path.Combine(destPath, "Gadget");
            string destAppSyncDataDir = Path.Combine(destPath, "AppSync");
            string destLibraryDir = Path.Combine(destPath, "Library");
            string destSharedFolderDir = Path.Combine(destPath, "SharedFolder");

            CopyRecursive(srcInputMapperFolderDir, destInputMapperFolderDir);
            CopyRecursive(srcGadgetDataDir, destGadgetDataDir);
            CopyRecursive(srcAppSyncDataDir, destAppSyncDataDir);
            CopyRecursive(srcLibraryDir, destLibraryDir);
            CopyRecursive(srcSharedFolderDir, destSharedFolderDir);
        }

        public static void CopyGameManagerUserData(string srcPath, string destPath)
        {
			Logger.Info("Copying gamemanager data files");
            string srcGMUserData = Path.Combine(srcPath, "UserData");

            MakeDirectory(destPath);
            string destGMUserData = Path.Combine(destPath, "UserData");

            CopyRecursive(srcGMUserData, destGMUserData);
        }

        public static void MakeDirectory(string path)
        {
            if (!Directory.Exists(path))
                Directory.CreateDirectory(path);
        }

		public static void CopyRecursive(String srcPath, String dstPath)
		{
			if (!Directory.Exists(dstPath))
				Directory.CreateDirectory(dstPath);

			DirectoryInfo src = new DirectoryInfo(srcPath);

			foreach (FileInfo file in src.GetFiles())
			{
				file.CopyTo(Path.Combine(dstPath, file.Name), true);
			}

			foreach (DirectoryInfo dir in src.GetDirectories())
			{
				CopyRecursive(Path.Combine(srcPath, dir.Name),
						Path.Combine(dstPath, dir.Name));
			}
		}

        public static void RestoreData(string srcPath, string destPath)
        {
			Logger.Info("Restoring data backup");
			Logger.Info("srcPath = {0} and destPath = {1}", srcPath, destPath);
            try
            {
                string srcAndroidUserDataPath = Path.Combine(srcPath, "Android");

                if (!File.Exists(Path.Combine(srcAndroidUserDataPath, "Data.vdi")) || !File.Exists(Path.Combine(srcAndroidUserDataPath, "SDCard.vdi")))
                {
                    Logger.Info("One of the android files is missing... Not restoring");
                    ShowRestoreErrorMsg();
                }

				//android
				Logger.Info("Restoring android files");
				ProgressForm.percentDone = (int)ProgressForm.States.RestoringAndroidData;
                RestoreAndroidUserData(srcAndroidUserDataPath, Path.Combine(destPath, "Android"));

				//gadget
				ProgressForm.percentDone = (int)ProgressForm.States.RestoringUserData;
                RestoreGadgetUserData(Path.Combine(srcPath, "UserData"), Path.Combine(destPath, "UserData"));

				//gamemanager
				ProgressForm.percentDone = (int)ProgressForm.States.RestoringGameManagerData;
                RestoreGameManagerUserData(Path.Combine(srcPath, "BluestacksGameManager"), Path.Combine(destPath, "BluestacksGameManager"));

				ProgressForm.percentDone = 100;
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to restore bluestacks data... Err : " + ex.ToString());
                ShowRestoreErrorMsg();
            }
        }

        public static void ShowRestoreErrorMsg()
        {
            MessageBox.Show(Locale.Strings.GetLocalizedString("RestoreFailedText"));
			Environment.Exit(0);
        }

        public static bool RestoreAndroidUserData(string srcPath, string destPath)
        {
            string srcDataVdi = Path.Combine(srcPath, "Data.vdi");
            string srcSDCardVdi = Path.Combine(srcPath, "SDCard.vdi");

            string destDataVdi = Path.Combine(destPath, "Data.vdi");
            string destSDCardVdi = Path.Combine(destPath, "SDCard.vdi");
            
            string tempPath = Path.GetTempPath();
            try
            {
                //backup dest android data
                File.Copy(destDataVdi, Path.Combine(tempPath, "Data.vdi_backup"), true);
                File.Copy(destSDCardVdi, Path.Combine(tempPath, "SDCard.vdi_backup"), true);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to create android backup in temp... Err : " + ex.ToString());
                ShowRestoreErrorMsg();
                return false;
            }

            try
            {
                File.Copy(srcDataVdi, destDataVdi, true);
                File.Copy(srcSDCardVdi, destSDCardVdi, true);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to copy backed up android data...copying from temp... Err : " + ex.ToString());
                File.Copy(Path.Combine(tempPath, "Data.vdi_backup"), destDataVdi, true);
                File.Copy(Path.Combine(tempPath, "SDCard.vdi_backup"), destSDCardVdi, true);
                ShowRestoreErrorMsg();
                return false;
            }

            return true;
        }

        public static void RestoreGadgetUserData(string srcPath, string destPath)
        {
            try
            {
                CopyGadgetUserData(srcPath, destPath);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to restore gadget user data... Err : " + ex.ToString());
            }
        }

        public static void RestoreGameManagerUserData(string srcPath, string destPath)
        {
            try
            {
                CopyGameManagerUserData(srcPath, destPath);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to backup gamemanager user data... Err : " + ex.ToString());
            }
        }
    }
}
