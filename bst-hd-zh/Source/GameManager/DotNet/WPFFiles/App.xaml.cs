using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Management;
using System.Net;
using System.Net.NetworkInformation;
using System.Net.Security;
using System.Runtime.InteropServices;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Windows;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : System.Windows.Application
    {
        private static bool mChangeDisplaySettings = true;
        [STAThread]
        public static void Main()
        {
            Init();
            Utils.LogParentProcessDetails();
            WriteTagFile();
            Locale.Strings.InitLocalization(null);
            //check for update if force update available exit
            Utils.ExitIfForceUpdateAvailable();

            Directory.SetCurrentDirectory(GameManagerUtilities.InstallDir);
            if (Features.IsFeatureEnabled(Features.MULTI_INSTANCE_SUPPORT))
            {
                Logger.Info("calling HD-QuitMultiInstance to kill other instances services and processes if running");
                Utils.QuitMultiInstance(GameManagerUtilities.InstallDir);
            }
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			String dataDir = (String)key.GetValue("DataDir");
			String androidBstkPath = System.IO.Path.Combine(dataDir, @"Android\Android.bstk");
			String tempBstkPath= androidBstkPath + ".tmp";
			String backUpBstkPath = androidBstkPath + "-bck";
			if (File.Exists(tempBstkPath))
			{
				File.Replace(tempBstkPath, androidBstkPath,backUpBstkPath);
			}
            var application = new App();
            application.Startup += Application_Startup;
            application.Exit += Application_Exit;
            application.InitializeComponent();
            application.Run();
        }

        private static void WriteTagFile()
        {
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
            String gmDirectory = (String)key.GetValue("InstallDir");
            String tagFilePath = Path.Combine(gmDirectory, @"OBS\tag.txt");
            String oem = Oem.Instance.OEM;
            if (!oem.Equals("gamemanager"))
                File.WriteAllText(tagFilePath, "_" + oem);
        }

        private static void Application_Startup(object sender, StartupEventArgs e)
        {
            GameManagerUtilities.FillArguments(e);
            CheckIfAlreadyRunning();

            ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

            Utils.KillProcessByName("HD-Frontend");

            Utils.KillProcessByName("HD-OBS");
            Utils.KillProcessByName("HD-RPCErrorTroubleShooter");
            ServicePointManager.DefaultConnectionLimit = 1000;

            BackgroundWorker CheckForStuckAtIntialization = new BackgroundWorker();
            CheckForStuckAtIntialization.DoWork += CheckForStuckAtIntialization_DoWork;
            //CheckForStuckAtIntialization.RunWorkerAsync();

            AppHandler.Init();
            GameManagerUtilities.UpdateLocalUrls();
            InitPowerEvents();

            SendPendingStats();
            GameManagerUtilities.Init();
            TimelineStatsSender.Init();
        }

        private static void SendPendingStats()
        {
            RegistryKey pendingStatsKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMPendingStats, true);
            if (pendingStatsKey != null && pendingStatsKey.GetValueNames().Length > 0)
            {
                StartStatsReportExe();
            }
            else if (pendingStatsKey == null)
            {
                RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath, true);
                baseKey.CreateSubKey("Stats");
                baseKey.Close();
            }
        }

        private static void StartStatsReportExe()
        {
            Logger.Info("in StartStatsReportExe()");
            RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
            string installDir = (string)key.GetValue("InstallDir");

            Process proc = new Process();
            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;
            proc.StartInfo.FileName = Path.Combine(installDir, "HD-CloudPost.exe");
            proc.StartInfo.Arguments = "\"" + Common.Strings.GMPendingStats + "\"";
            proc.Start();
        }

        private static void CheckForStuckAtIntialization_DoWork(object sender, DoWorkEventArgs e)
        {
            //Check for HandleAppDisplayed for first 5 mins in every 10 secs
            //If appdisplayed occurs do nothing else keep polling
            Logger.Info("CheckForStuckAtInitialization");
            int count = 0;

            while (count < 30)
            {
                if (AppHandler.mAppDisplayedOccured)
                {
                    Logger.Info("HandleAppDisplayed received, no stuck at loading");
                    return;
                }
                count += 1;
                Thread.Sleep(10000);
            }

            if (AppHandler.mAppDisplayedOccured == false)
            {
                TroubleShootStuckAtInitialization();
            }
        }

        private static void TroubleShootStuckAtInitialization()
        {
            Logger.Info("Stuck at Initialization Error detected");

            DialogResult result = System.Windows.Forms.MessageBox.Show(Locale.Strings.TROUBLESHOOTER_TEXT,
                    Locale.Strings.STUCK_AT_INITIALIZING_FORM_TEXT, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                Logger.Info("User clicked yes");
                GameManagerUtilities.RunTroubleShooterExe("HD-Restart.exe",
                        "Android",
                        Locale.Strings.WORK_DONE_TEXT,
                        Locale.Strings.STUCK_AT_INITIALIZING_FORM_TEXT);
            }
            else
            {
                Logger.Info("User clicked No");
            }
        }

        private static void Application_Exit(object sender, ExitEventArgs e)
        {
            GameManagerUtilities.sGameManagerLock.Close();
        }

        private static void Init()
        {
            Logger.InitLog("GameManager", "gamemanager");
            Logger.Info("Starting GameManager PID {0}", Process.GetCurrentProcess().Id);
            Logger.Info("GameManager: CLR version {0}", Environment.Version);

            System.Windows.Forms.Application.ThreadException += Application_ThreadException;

            System.Windows.Forms.Application.SetUnhandledExceptionMode(UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += CurrentDomain_UnhandledException;
           
        }

       

        private static void InitPowerEvents()
        {
            try
            {
                //log-on event
                SystemEvents.SessionSwitch += HandleSessionSwitch;

                //system suspend event
                WqlEventQuery q = new WqlEventQuery("Win32_PowerManagementEvent");
                ManagementScope scope = new ManagementScope("root\\CIMV2");
                ManagementEventWatcher suspendWatcher = new ManagementEventWatcher(scope, q);
                suspendWatcher.EventArrived += PowerEventArrive;
                Microsoft.Win32.SystemEvents.DisplaySettingsChanged += HandleDisplaySettingsChanged;
                NetworkChange.NetworkAvailabilityChanged += new NetworkAvailabilityChangedEventHandler(OnNetworkAvailabilityChanged);
                suspendWatcher.Start();
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to init power event... Err : " + ex.ToString());
            }
        }

        private static void OnNetworkAvailabilityChanged(object sender, NetworkAvailabilityEventArgs e)
        {
            if (e.IsAvailable)
            {
                Logger.Info("Network is available");
            }
            else
            {
                Logger.Info("Network is NOT available");
            }
        }
        private static void HandleDisplaySettingsChanged(Object sender, EventArgs evt)
        {
            Logger.Info("HandleDisplaySettingsChanged()");
            if (mChangeDisplaySettings && GameManagerWindow.Instance != null)
            {
                GameManagerWindow.Instance.GameManagerResized();

                if (IsPortrait())
                {
                    AutoCloseMessageBox.ShowMsg();
                }
                {
                    AutoCloseMessageBox.HideBox();
                }
            }
            mChangeDisplaySettings = true;
        }

        private static bool IsPortrait()
        {
            ScreenOrientation so = SystemInformation.ScreenOrientation;

            return (so == ScreenOrientation.Angle90) ||
                (so == ScreenOrientation.Angle270);
        }

        private static void HandleSessionSwitch(object sender, SessionSwitchEventArgs e)
        {
            Logger.Info("switch user event has arrived....stopping stream.....isstreaming = " + BTVManager.sStreaming);
            mChangeDisplaySettings = false;
            BTVManager.Stop();
        }

        private static void PowerEventArrive(object sender, EventArrivedEventArgs e)
        {
            try
            {
                foreach (PropertyData pd in e.NewEvent.Properties)
                {
                    if (pd == null || pd.Value == null) continue;
                    string powerEventValue = pd.Value.ToString();
                    Logger.Info("PowerEvent : " + powerEventValue);

                    if (String.Compare(powerEventValue, "4", true) == 0)
                    {
                        Logger.Info("power event has arrived...stopping stream....isstreaming = " + BTVManager.sStreaming);
                        BTVManager.Stop();
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("An error ocured while setting time....Err : " + ex.ToString());
            }
        }
        private static void CheckIfAlreadyRunning()
        {
            if (Common.Utils.IsAlreadyRunning(Common.Strings.GameManagerLockName, out GameManagerUtilities.sGameManagerLock))
            {
                Logger.Info("GameManager already running");

                /*
				 * Try to bring the GameManager window to the foreground.
				 */

                try
                {
                    IntPtr handle = Common.Interop.Window.FindWindow(null, GameManagerUtilities.WindowTitle);
                    if (handle == IntPtr.Zero)
                    {
                        Logger.Error("Cannot find window '" + GameManagerUtilities.WindowTitle + "'");
                    }

                    if (!Common.Interop.Window.SetForegroundWindow(handle))
                    {
                        Logger.Error("Cannot set foreground window" + Marshal.GetLastWin32Error());
                    }

                    Common.Interop.Window.ShowWindow(handle, Common.Interop.Window.SW_SHOW);

                    Logger.Info("Sending WM_USER_SHOW_WINDOW to GameManager handle {0}", handle);
                    int res = Common.Interop.Window.SendMessage(handle,
                            Common.Interop.Window.WM_USER_SHOW_WINDOW,
                            IntPtr.Zero,
                            IntPtr.Zero);
                    /*
					 * Check if the message sent has been handled
					 * res will be 1 in case it is
					 */
                    if (handle != IntPtr.Zero && res != 1)
                    {
                        string url = String.Format("http://127.0.0.1:{0}/{1}", GameManagerUtilities.GameManagerPort, Common.Strings.ShowWindowUrl);
                        Dictionary<string, string> data = new Dictionary<string, string>();
                        Logger.Info("Sending request to: " + url);

                        try
                        {
                            string result = Common.HTTP.Client.Post(url, data, null, false);
                            Logger.Info("showwindow result: " + result);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.ToString());
                            Logger.Error("Post failed. url = {0}, data = {1}", url, data);
                        }
                    }

                    if (handle != IntPtr.Zero)
                    {
                        string url = String.Format("http://127.0.0.1:{0}/{1}", GameManagerUtilities.GameManagerPort, Common.Strings.ShowAppUrl);
                        Dictionary<string, string> data = new Dictionary<string, string>();
                        data.Add("package", GameManagerUtilities.args.p);
                        data.Add("activity", GameManagerUtilities.args.a);
                        Logger.Info("Sending request to: " + url);

                        try
                        {
                            string result = Common.HTTP.Client.Post(url, data, null, false);
                            Logger.Info("showwindow result: " + result);
                        }
                        catch (Exception ex)
                        {
                            Logger.Error(ex.ToString());
                            Logger.Error("Post failed. url = {0}, data = {1}", url, data);
                        }
                    }

                }
                catch (Exception exc)
                {
                    Logger.Error(exc.ToString());
                }
                Environment.Exit(0);
            }
        }

        private static void ExceptionHandlerCallback(Exception e)
        {
            Logger.Error("Unhandled Thread Exception:");
            Logger.Error(e.ToString());

            //StreamViewTimeStats.HandleWindowCrashSession();
            //System.Windows.Forms.MessageBox.Show("BlueStacks App Player.\nError: " + e.Message);
            //SendLogsAndExit(e);
        }

        private static void Application_ThreadException(object sender, ThreadExceptionEventArgs e)
        {
            Logger.Error("Unhandled Thread Exception:");
            Logger.Error(e.Exception.ToString());

            StreamViewTimeStats.HandleWindowCrashSession();
            System.Windows.Forms.MessageBox.Show("BlueStacks App Player.\nError: " + e.Exception.Message);
            SendLogsAndExit(e.Exception);
        }

        private static void CurrentDomain_UnhandledException(object sender, UnhandledExceptionEventArgs e)
        {
            Logger.Error("Unhandled Application Exception:");
            Logger.Error(e.ExceptionObject.ToString());

            StreamViewTimeStats.HandleWindowCrashSession();
            System.Windows.Forms.MessageBox.Show("BlueStacks App Player.\nError: " + ((Exception)(e.ExceptionObject)).ToString());

            SendLogsAndExit((Exception)e.ExceptionObject);
        }

        private static void SendLogsAndExit(Exception evt)
        {
            RegistryKey reg = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
            string installDir = (string)reg.GetValue("InstallDir");
            Utils.KillProcessByName("HD-Frontend");

            GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                GameManagerWindow.Instance.Hide();
            }));

            GameManagerUtilities.sGameManagerLock.Close();

            try
            {
                UploadCrashLogs(evt.ToString());
            }
            catch (Exception e)
            {
                Logger.Error(e.ToString());
            }

            Environment.Exit(1);
        }

        private static void UploadCrashLogs(string errorMsg)
        {
            string url = String.Format("{0}/{1}", Service.Host, Common.Strings.GMCrashReportUrl);
            Dictionary<String, String> postData = new Dictionary<String, String>();
            postData.Add("error", errorMsg);
            Common.HTTP.Client.Post(url, postData, null, true);
        }

        private static bool ValidateRemoteCertificate(object sender, X509Certificate cert,
                X509Chain chain, SslPolicyErrors policyErrors)
        {
            return true;
        }
    }

}
