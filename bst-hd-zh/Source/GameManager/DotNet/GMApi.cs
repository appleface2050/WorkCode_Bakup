using System;
using System.Collections.Generic;
using System.IO;
using System.Net;
using System.Data;
using Microsoft.Win32;
using System.Diagnostics;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Threading;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using BlueStacks.hyperDroid.Frontend;

using CodeTitans.JSon;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
    public class GMApi
    {
        [DllImport("HD-ShortcutHandler.dll", CharSet = CharSet.Auto)]
        public static extern int CreateShortcut(
                    string target,
                    string shortcutName,
                    string desc,
                    string iconPath,
                    string targetArgs,
                    int initializeCom);

        private static string sCurrentInstallStatus = InstallStatus.DOWNLOADING;
        private static string sCurrentStatusString = "";
        public const String JSON_BASE_URL = "http://cdn.bluestacks.com/public/gamemanager/content/bluestacks/json2/";

        public static String CHANNEL_NAMES_JSON_URL = JSON_BASE_URL + "channel_names.json";
        public static String CHANNEL_APPS_JSON_URL = JSON_BASE_URL + "channel_apps.json";
        public static String WEB_APPS_JSON_URL = JSON_BASE_URL + "web_apps.json";
        public static String THEMES_JSON_URL = JSON_BASE_URL + "themes.json";
        public static class InstallStatus
        {
            public static string INSTALLING = "installing";
            public static string INSTALLED = "installed";
            public static string FAILED = "failed";
            public static string DOWNLOADING = "downloading";
        }

        public static void setInstallStatus(string status, string statusString)
        {
            sCurrentInstallStatus = status;
            sCurrentStatusString = statusString;
        }

        public static string getInstallStatus()
        {

            return sCurrentInstallStatus + "##" + sCurrentStatusString;
        }

        public static string GetLocaleName()
        {
            return CultureInfo.CurrentCulture.ToString();
        }

        public static string GetAvailableLocaleName()
        {
            string baseName = "en-US";
            try
            {
                baseName = CultureInfo.CurrentCulture.ToString();
                string localeDir = GameManagerUtilities.GetCurrentThemeLocalDir();
                string filePath = Path.Combine(localeDir, "i18n");
                filePath = Path.Combine(filePath, baseName + ".json");
                Logger.Info("Checking for localized file: " + filePath);
                if (File.Exists(filePath))
                {
                    return baseName;
                }
                else
                {
                    baseName = "en-US";
                }
            }
            catch (Exception e)
            {
                Logger.Error("Failed to check for locale file. error: " + e.ToString());
                baseName = "en-US";
            }

            return baseName;
        }

        public static string GetThemesJson()
        {
            string defaultData = "[]";
            string path = Strings.GameManagerHomeDir + @"\" + GMUtils.sThemesJson;
            string fileData = GMUtils.GetJson(THEMES_JSON_URL, path, defaultData);
            return fileData;
        }

        public static string GetWebAppsJson()
        {
            string defaultData = "[]";
            string path = Strings.GameManagerHomeDir + @"\" + GMUtils.sWebAppsJson;
            string fileData = GMUtils.GetJson(WEB_APPS_JSON_URL, path, defaultData);
            return fileData;
        }

        public static string GetChannelNamesJson()
        {
            string defaultData = "[]";
            string path = Strings.GameManagerHomeDir + @"\" + GMUtils.sChannelNamesJson;
            string fileData = GMUtils.GetJson(CHANNEL_NAMES_JSON_URL, path, defaultData);
            return fileData;
        }

        public static string GetChannelAppsJson()
        {
            string defaultData = "[]";
            string path = Strings.GameManagerHomeDir + @"\" + GMUtils.sChannelAppsJson;
            string fileData = GMUtils.GetJson(CHANNEL_APPS_JSON_URL, path, defaultData);
            return fileData;
        }

        public static string GetChannelAppsJson(string channelId, string subCategory)
        {
            String data = "[]";
            bool filter = true;
            bool filter2 = true;

            if (channelId.Equals(""))
                return data;
            if (channelId.Equals("null"))
                filter = false;
            if (subCategory.Equals("") || subCategory.Equals("null"))
                filter2 = false;

            string fullJsonString = GetAllAppsJsonString();
            JSonReader readjson = new JSonReader();
            IJSonObject fullJson = readjson.ReadAsJSonObject(fullJsonString);

            try
            {
                //flushing data
                data = "";

                JSonWriter writer = new JSonWriter(true);
                writer.WriteArrayBegin();

                if (filter == true)
                {
                    for (int i = 0; i < fullJson.Length; i++)
                    {
                        IJSonObject channelIds = fullJson[i]["channelIds"];
                        for (int j = 0; j < channelIds.Length; j++)
                        {
                            if (channelIds[j].ToString().Equals(channelId))
                            {
                                if (filter2 == true)
                                {
                                    //do some more filtering
                                    try
                                    {
                                        if (fullJson[i]["category"].ToString().Equals(subCategory))
                                        {
                                            writer.Write(fullJson[i]);
                                        }
                                    }
                                    catch
                                    {
                                    }
                                }
                                else
                                {
                                    writer.Write(fullJson[i]);
                                }
                                break;
                            }
                        }
                    }
                }
                else if (filter2 == true)
                {
                    for (int i = 0; i < fullJson.Length; i++)
                    {
                        try
                        {
                            if (fullJson[i]["category"].ToString() == subCategory)
                            {
                                writer.Write(fullJson[i]);
                            }
                        }
                        catch
                        {
                        }
                    }
                }
                else //no filter is applied hence return the data as it is
                {
                    return fullJsonString;
                }

                writer.WriteArrayEnd();
                data = writer.ToString();
                //Logger.Info(data);
                return data;
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
                return "[]";
            }
        }

        private static string GetAllAppsJsonString()
        {
            string channelAppsJsonString = GetChannelAppsJson();
            JSonReader readjson = new JSonReader();
            IJSonObject channelAppsJson = readjson.ReadAsJSonObject(channelAppsJsonString);

            string webAppsJsonString = GetWebAppsJson();
            readjson = new JSonReader();
            IJSonObject webAppsJson = readjson.ReadAsJSonObject(webAppsJsonString);

            JSonWriter writer = new JSonWriter(true);
            writer.WriteArrayBegin();
            for (int i = 0; i < channelAppsJson.Length; i++)
            {
                writer.Write(channelAppsJson[i]);
            }

            for (int i = 0; i < webAppsJson.Length; i++)
            {
                writer.Write(webAppsJson[i]);
            }

            writer.WriteArrayEnd();
            string data = writer.ToString();
            return data;
        }

        public static bool uninstallApp(string pkg)
        {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string installDir = (string)reg.GetValue("InstallDir");

            ProcessStartInfo psi = new ProcessStartInfo();
            psi.FileName = Path.Combine(installDir, "HD-ApkHandler.exe");
            psi.Arguments = String.Format("-u -p \"{0}\"", pkg);
            psi.UseShellExecute = false;
            psi.CreateNoWindow = true;
            Process apkUnInstaller = Process.Start(psi);
            apkUnInstaller.WaitForExit();
            Logger.Info("Apk Uninstaller exit code: {0}", apkUnInstaller.ExitCode);
            if (apkUnInstaller.ExitCode != 0)
            {
                return false;
            }

            return true;
        }

        public static bool createAppShortcut(string pkg)
        {
            if (string.IsNullOrEmpty(pkg))
            {
                return false;
            }
            string appName, image, activity, appstore;
            if (false == JsonParser.GetAppInfoFromPackageName(pkg, out appName, out image, out activity, out appstore))
            {
                return false;
            }

            RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
            string installDir = (string)reg.GetValue("InstallDir");

            string desktopDir = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
            string shortcutPath = Path.Combine(desktopDir, appName + ".lnk");
            RegistryKey gameManagerReg = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
            string gameManagerDir = (string)gameManagerReg.GetValue("InstallDir");
            string gameManagerFile = Path.Combine(gameManagerDir, "BlueStacks.exe");

            string iconsDir = Path.Combine(Common.Strings.LibraryDir, Common.Strings.IconsDir);
            string png2ico = Path.Combine(installDir, "HD-png2ico.exe");
            string iconFile = Path.Combine(installDir, "BlueStacks.ico");
            string imagePath = Path.Combine(Common.Strings.GadgetDir, image);

            Utils.ResizeImage(imagePath);
            string icon = Utils.ConvertToIco(png2ico, imagePath, iconsDir);

            if (!File.Exists(icon))
                icon = iconFile;

            string args = String.Format("-p {0} -a {1}", pkg, activity);

            int res = CreateShortcut(gameManagerFile, shortcutPath, "", icon, args, 0);
            if (res != 0)
                Logger.Error("Couldn't create shortcut {0}", shortcutPath);
            else
                Logger.Info("Created shortcut {0}", shortcutPath);

            return res == 0 ? true : false;
        }

        public static void setUserName(string name)
        {
            RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
            key.SetValue("UserName", name);
        }

        public static void LaunchUrlIntentActivity(String hitUrl)
        {
            Thread launchIntentThread = new Thread(delegate ()
            {
                try
                {
                    Logger.Info("Launching {0} in guest", hitUrl);
                    int port = Utils.GetBstCommandProcessorPort(Common.Strings.VMName);

                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("action", "android.intent.action.VIEW");
                    data.Add("data", hitUrl);

                    String url = String.Format("http://127.0.0.1:{0}/{1}", port, Common.Strings.AndroidCustomActivityLaunchApi);
                    Logger.Info("the url being hit is {0}", url);

                    string res = Common.HTTP.Client.Post(url, data, null, false);

                    Logger.Info("the response we get is " + res);

                    JSonReader readjson = new JSonReader();
                    IJSonObject resJson = readjson.ReadAsJSonObject(res);
                    string result = resJson["result"].StringValue.Trim();
                    if (result == "ok")
                    {
                        Logger.Info("Successfully requested Intent");
                    }
                    else
                    {
                        Logger.Error("Error occurred at guest, got the response : {0}", res);
                    }
                }
                catch (Exception e)
                {
                    Logger.Error(string.Format("Error occured, Err: {0}", e.ToString()));
                }
            });
            launchIntentThread.IsBackground = true;
            launchIntentThread.Start();
        }

        public static string getUserName()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            return key.GetValue("UserName", "").ToString();
        }

        public static void setInterests(string interestString)
        {
            RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.GMConfigPath);
            key.SetValue("Interests", interestString);
        }

        public static string getInterests()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            return key.GetValue("Interests", "").ToString();
        }

        public static bool isAppInstalled(string package)
        {
            if (package != null)
            {
                return JsonParser.IsAppInstalled(package);
            }

            return false;
        }

        public static string getAppDownloadProgress(string pkg)
        {
            RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
            return prodKey.GetValue("DownloadProgress_" + pkg, "").ToString();
        }

        public static void ReportProblem()
        {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string installDir = (string)reg.GetValue("InstallDir");
            ProcessStartInfo proc = new ProcessStartInfo();
            proc.FileName = installDir + "HD-LogCollector.exe";
            Logger.Info("SysTray: Starting " + proc.FileName);
            Process.Start(proc);

        }

        public static void RestartAndroidPlugin()
        {
            if (TabButtons.Instance.SelectedTab.TabType == EnumTabType.web)
            {
                TabButtons.Instance.GoToTab(1);
            }
            ContentControl.Instance.ShowWaitControl();
            RegistryKey reg = Registry.LocalMachine.OpenSubKey("Software\\BlueStacks");
            string installDir = (string)reg.GetValue("InstallDir");
            TabButtons.Instance.mLastAppTabName = "";
            FrontendHandler.RestartFrontend();
            if (GameManagerUtilities.sHomeType != "html")
            {
                TabButtons.Instance.SelectedTab.PerformTabAction(true, true);
            }
        }

        public static void CheckForUpdates()
        {
            string res = GMHTTPHandler.SendUpdateRequest(null, Common.Strings.UpdaterRequestUrl);
            try
            {
                JSonReader reader = new JSonReader();
                IJSonObject obj = reader.ReadAsJSonObject(res);
                bool success = Convert.ToBoolean(obj[0]["success"].StringValue);
                if (success)
                {
                    bool status = Convert.ToBoolean(obj[0]["status"].StringValue);
                    if (!status)
                    {
                        NoUpdatesAvailable();
                    }
                    else
                    {
                        UpdateAvailable();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Error while checking for upgrades... Err : " + ex.ToString());
                NoUpdatesAvailable();
            }
        }
        public static void NoUpdatesAvailable()
        {
            Logger.Info("No updates available");

            String capt = "BlueStacks Updater";
            String text = "No new updates available";

            try
            {
                capt = Locale.Strings.GetLocalizedString("UpdateProgressTitleText");
                text = Locale.Strings.GetLocalizedString("UPDATER_UTILITY_NO_UPDATE_TEXT");
            }
            catch
            {
                //Logger.Info("locale string for no update box not found");
            }
            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                MessageBox.Show(text, capt, MessageBoxButtons.OK);
            }));
        }

        public static void UpdateAvailable()
        {
            String capt = "BlueStacks Updater";
            String text = "Update is available. Do you want to install now?";

            try
            {
                capt = Locale.Strings.GetLocalizedString("UpdateProgressTitleText");
                text = Locale.Strings.GetLocalizedString("UpdateAvailableText");
            }
            catch
            {
                //Logger.Info("locale string for no update box not found");
            }

            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                DialogResult result = MessageBox.Show(text,
                        capt,
                        MessageBoxButtons.YesNo);
                if (result == DialogResult.Yes)
                {
                    RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
                    string installDir = (string)reg.GetValue("InstallDir");
                    ProcessStartInfo proc = new ProcessStartInfo();
                    proc.FileName = installDir + "HD-UpdateHelper.exe";
                    Process.Start(proc);
                    GMHTTPHandler.StartUpdateRequest(null, "/installupdate");
                }
            }));
        }
    }
}
