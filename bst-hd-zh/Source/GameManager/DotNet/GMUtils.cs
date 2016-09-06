using System;
using System.IO;
using System.Net;
using System.Data;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Windows.Forms;
using System.ComponentModel;
using System.IO.Compression;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Text.RegularExpressions;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;

using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.GameManager
{
    public class GMUtils
    {
        private static string sS2PDonePath = "iss2pmappingdone";
        private static string sLaunchS2PSetupPath = "starts2pgpsetup";

        public static string sChannelNamesJson = "channel_names.json";
        public static string sChannelAppsJson = "channel_apps.json";
        public static string sWebAppsJson = "web_apps.json";
        public static string sInstalledAppsJson = "installedApps.json";
        public static string sThemesJson = "themes.json";

        private static Object sLockObject = new Object();

        public static String GetJson(string url, string path, string defaultData)
        {
            if (File.Exists(path) && !ValidJsonFile(path))
                File.Delete(path);

            string fileData;
            lock (sLockObject)
            {
                fileData = GetDataFromUrl(url, path, defaultData);
            }

            if (!File.Exists(path) || !ValidJsonFile(path))
                return defaultData;

            return fileData;
        }

        public static String GetJsonAsString(string path)
        {
            if (!File.Exists(path))
            {
                Logger.Info("file does not exists = " + path);
                return "[]";
            }

            try
            {
                StreamReader sr = new StreamReader(path);
                string fileData = sr.ReadToEnd();
                sr.Close();
                Logger.Info("fileData = " + fileData);
                return fileData;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
                return "[]";
            }
        }

        public static bool ValidJsonFile(string path)
        {
            try
            {
                StreamReader sr = new StreamReader(path);
                string fileData = sr.ReadToEnd();
                sr.Close();

                JSonReader readjson = new JSonReader();
                IJSonObject fullJson = readjson.ReadAsJSonObject(fileData);
                return true;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
                return false;
            }
        }

        public static String RectifyString(string fileData)
        {
            fileData = fileData.Replace("\"", "\\\"");
            fileData = fileData.Replace("\n", "");
            fileData = fileData.Replace("\r", "");
            fileData = Regex.Replace(fileData, @"\s+", " ", RegexOptions.Multiline);
            return fileData;
        }

        public static String GetDataFromUrl(String url, String path, String defaultData)
        {
            try
            {
                if (File.Exists(path) == false)
                {
                    DownloadFile(url, path);
                }
                else if ((DateTime.UtcNow - File.GetLastWriteTimeUtc(path)) > TimeSpan.FromDays(1))
                {
                    Thread t = new Thread(delegate ()
                    {
                        DownloadFile(url, path);
                    });
                    t.IsBackground = true;
                    t.Start();
                }

                string fileData;
                if (File.Exists(path))
                {
                    StreamReader sr = new StreamReader(path);
                    fileData = sr.ReadToEnd();
                    sr.Close();
                }
                else
                {
                    fileData = defaultData;
                }

                return fileData;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Failed to get json data. Err: {0}", e.ToString()));
                return defaultData;
            }
        }

        private static void DownloadFile(string url, string path)
        {
            try
            {
                string newPath = path + ".new";

                if (File.Exists(newPath))
                {
                    File.Delete(newPath);
                }

                Logger.Info("Downloading latest json: " + newPath);

                WebClient w = new WebClient();
                w.DownloadFile(url, newPath);

                if (ValidJsonFile(newPath))
                {
                    if (File.Exists(path))
                        File.Delete(path);
                    File.Move(newPath, path);
                }
                else
                {
                    Logger.Error("Downloaded json is not valid");
                }
            }
            catch
            {
            }
        }

        public static bool IsGuestBooted()
        {
            string url = String.Format("http://127.0.0.1:{0}/{1}",
                    Common.VmCmdHandler.s_ServerPort, Common.VmCmdHandler.s_PingPath);
            try
            {
                Common.HTTP.Client.Get(url, null, false, 100);
                Logger.Info("Guest booted");
                return true;
            }
            catch
            {
                return false;
            }
        }

        public static bool IsS2PConfigured()
        {
            bool s2pConfigured = false;
            RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMBasePath);
            s2pConfigured = Convert.ToBoolean((string)key.GetValue("S2PConfigured", "false"));
            if (s2pConfigured)
                return s2pConfigured;

            if (IsGuestBooted())
            {
                string url = String.Format("http://127.0.0.1:{0}/{1}",
                        Common.VmCmdHandler.s_ServerPort, sS2PDonePath);
                try
                {
                    string r = Common.HTTP.Client.Get(url, null, false, 100);
                    Logger.Info("Guest booted");

                    IJSonReader json = new JSonReader();
                    IJSonObject res = json.ReadAsJSonObject(r);
                    string result = res["result"].StringValue;
                    if (result == "ok")
                        s2pConfigured = true;
                    else
                        s2pConfigured = false;
                }
                catch
                {
                    s2pConfigured = false;
                }

                key.SetValue("S2PConfigured", s2pConfigured.ToString());
            }
            else
            {
                s2pConfigured = Convert.ToBoolean((string)key.GetValue("S2PConfigured", "false"));
            }

            key.Close();
            return s2pConfigured;
        }

        public static bool ElevatedProcess()
        {
            try
            {
                WindowsIdentity identity = WindowsIdentity.GetCurrent();
                WindowsPrincipal principal = new WindowsPrincipal(identity);
                bool isAdmin = principal.IsInRole(WindowsBuiltInRole.Administrator);
                Logger.Info("Process admin mode ={0}", isAdmin);
                return isAdmin;
            }
            catch (Exception e)
            {
                Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
                return true;
            }
        }

        public static void LaunchS2PSetup(string package, string title)
        {
            string url = String.Format("http://127.0.0.1:{0}/{1}",
                    Common.VmCmdHandler.s_ServerPort, sLaunchS2PSetupPath);
            try
            {
                Dictionary<string, string> data = new Dictionary<string, string>();
                data.Add("pkg", package);
                data.Add("title", title);
                Common.HTTP.Client.PostWithRetries(url, data, null, false, 60, 1000, Common.Strings.VMName);
            }
            catch
            {
            }
        }

        public static int Unzip(string zipFilePath, string outDir)
        {
            try
            {
                if (Directory.Exists(outDir) == false)
                {
                    Directory.CreateDirectory(outDir);
                }
            }
            catch (Exception e)
            {
                Logger.Error("failed to create directory. err: " + e.ToString());
            }

            String FILE_SEPARATOR = "\\";
            ZipStorer zip = ZipStorer.Open(zipFilePath, FileAccess.Read);

            List<ZipStorer.ZipFileEntry> dir = zip.ReadCentralDir();

            foreach (ZipStorer.ZipFileEntry entry in dir)
            {
                String entryName = entry.FilenameInZip;
                String dirName = System.IO.Path.GetDirectoryName(entryName);

                String subDir = FILE_SEPARATOR;
                if (dirName.IndexOf(FILE_SEPARATOR) != -1)
                {
                    subDir = dirName.Substring(dirName.IndexOf("\\") + 1) + FILE_SEPARATOR;
                }

                String fileName = Path.GetFileName(entry.FilenameInZip);
                String fileOutDir = subDir == FILE_SEPARATOR ? outDir : Path.Combine(outDir, subDir);
                String fileOutPath = Path.Combine(fileOutDir, fileName);
                zip.ExtractFile(entry, fileOutPath);
            }
            zip.Close();

            return 0;
        }
    }
}
