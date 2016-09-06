using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Linq;
using System.Windows;

using Microsoft.Win32;
using System.Threading;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    /// 
    public partial class App : Application
    {
        public static RegistryKey sConfigKey;
        public static int sBlueStacksTVPort = 2885;
        public static string sApplicationBaseUrl = "http://localhost:2881/static/";

        public static string sServerRootDir = null;
        public static string sApplicationServerPort = null;
        static private Mutex sBTVLock;

        [STAThread]
        public static void Main()
        {
            Init();

            if (Oem.Instance.OEM == "gamemanager" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "bluestacks")
            {
                sConfigKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath, true);
                int gmPort = (int)sConfigKey.GetValue("PartnerServerPort", 2871);
                sApplicationServerPort = gmPort.ToString();
                sServerRootDir = Common.Strings.GameManagerHomeDir;
                StreamWindowUtility.CheckIfGMIsRunning();
            }
            else
            {
                sConfigKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath, true);
                int partnerPort = (int)sConfigKey.GetValue("PartnerServerPort", 2881);
                sApplicationServerPort = partnerPort.ToString();
                sServerRootDir = Common.Strings.BTVDir;
            }
            sApplicationBaseUrl = "http://localhost:" + sApplicationServerPort + "/static/";

            Thread httpThread = new Thread(SetupHTTPServer);
            httpThread.IsBackground = true;
            httpThread.Start();

            StreamWindowUtility.UpdateLocalUrls();
            //FilterUtility.FilterThemesThread();
            FilterUtility.CheckNewFiltersAvailable();
            var application = new App();
            application.InitializeComponent();
            application.Run();
        }

        private static void Init()
        {
            Logger.InitLog("BlueStacksTV", "bluestackstv");
            Logger.Info("Starting BlueStacksTV PID {0}", Process.GetCurrentProcess().Id);
            Logger.Info("BlueStacksTV: CLR version {0}", Environment.Version);
            Logger.Info("IsAdministrator: {0}", User.IsAdministrator());

            System.Windows.Forms.Application.ThreadException += delegate (Object obj,
                    System.Threading.ThreadExceptionEventArgs evt)
            {
                Logger.Error("BlueStacksTV: Unhandled Exception:");
                Logger.Error(evt.Exception.ToString());
                sConfigKey.Close();
                Environment.Exit(1);
            };

            System.Windows.Forms.Application.SetUnhandledExceptionMode(
                    System.Windows.Forms.UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += delegate (
                    Object obj, UnhandledExceptionEventArgs evt)
            {
                Logger.Error("BlueStacksTV: Unhandled Exception:");
                Logger.Error(evt.ExceptionObject.ToString());
                sConfigKey.Close();
                Environment.Exit(1);
            };

            try
            {
                if (Common.Utils.IsAlreadyRunning(Common.Strings.BlueStacksTVLockName, out sBTVLock))
                {
                    Logger.Info("BTV already running.... exiting");
                    Environment.Exit(0);
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to check whether btv is running or not... Err : " + ex.ToString());
            }
        }

        private static void SetupHTTPServer()
        {
            Logger.Info("In setup btv http server");
            Dictionary<String, Common.HTTP.Server.RequestHandler> routes =
                new Dictionary<String, Common.HTTP.Server.RequestHandler>();

            routes.Add("/initstream", HTTPHandler.InitStreamHandler);
            routes.Add("/startstream", HTTPHandler.StartStreamHandler);
            routes.Add("/stopstream", HTTPHandler.StopStreamHandler);
            routes.Add("/startrecord", HTTPHandler.StartRecordHandler);
            routes.Add("/stoprecord", HTTPHandler.StopRecordHandler);
            routes.Add("/showobs", HTTPHandler.ShowObsHandler);
            routes.Add("/hideobs", HTTPHandler.HideObsHandlerHandler);
            routes.Add("/movewebcam", HTTPHandler.MoveWebCamHandler);
            routes.Add("/enablewebcam", HTTPHandler.EnableWebCamHandler);
            routes.Add("/disablewebcam", HTTPHandler.DisableWebCamHandler);
            routes.Add("/setconfig", HTTPHandler.SetConfigHandler);
            routes.Add("/startreplaybuffer", HTTPHandler.StartReplayBufferHandler);
            routes.Add("/stopreplaybuffer", HTTPHandler.StopReplayBufferHandler);
            routes.Add("/savereplaybuffer", HTTPHandler.SaveReplayBufferHandler);
            routes.Add("/setsystemvolume", HTTPHandler.SetSystemVolumeHandler);
            routes.Add("/setmicvolume", HTTPHandler.SetMicVolumeHandler);
            routes.Add("/obsstatus", HTTPHandler.ObsStatusHandler);
            routes.Add("/shutdown", HTTPHandler.ShutDownObsHandler);
            routes.Add("/replaybuffersaved", HTTPHandler.ReplayBufferSavedHandler);
            routes.Add("/reportobserror", HTTPHandler.ReportObsErrorHandler);
            routes.Add("/windowresized", HTTPHandler.ResizeStreamHandler);
            routes.Add("/resetflvstream", HTTPHandler.ResetFlvStreamHandler);
            routes.Add("/ping", HTTPHandler.PingHandler);
            routes.Add("/setclrbrowserconfig", HTTPHandler.SetClrBrowserConfigHandler);
            routes.Add("/enableclrbrowser", HTTPHandler.EnableClrBrowserHandler);
            routes.Add("/disableclrbrowser", HTTPHandler.DisableClrBrowserHandler);
            routes.Add("/setfrontendposition", HTTPHandler.SetFrontendPositionHandler);
            routes.Add("/showstreamwindow", HTTPHandler.ShowStreamWindowHandler);
            routes.Add("/hidestreamwindow", HTTPHandler.HideStreamWindowHandler);
            routes.Add("/sessionswitch", HTTPHandler.SessionSwitchHandler);
            routes.Add("/closebtv", HTTPHandler.CloseBTVHandler);
            routes.Add("/tabchangeddata", HTTPHandler.TabChangedDataHandler);
            routes.Add("/receiveAppInstallStatus", HTTPHandler.ReceiveAppInstallStatusHandler);
            routes.Add("/checknewfilters", HTTPHandler.CheckNewFiltersHandler);
            Common.HTTP.Server server;
            for (int port = 2885; port < 2891; port++)
            {
                try
                {
                    server = new Common.HTTP.Server(port, routes, sServerRootDir);
                    sBlueStacksTVPort = port;
                    server.Start();
                    Logger.Info("BlueStacksTV server listening on port: " + server.Port);

                    /* write server port to the registry */
                    if (Oem.Instance.OEM == "gamemanager" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "bluestacks")
                    {
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
                        key.SetValue("BlueStacksTVServerPort", server.Port, RegistryValueKind.DWord);
                        key.Close();
                    }
                    else
                    {
                        RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath, true);
                        key.SetValue("BlueStacksTVServerPort", server.Port, RegistryValueKind.DWord);
                        key.Close();
                    }

                    server.Run();

                    break;
                }
                catch (Exception ex)
                {
                    Logger.Error("failed in running server... Err : " + ex.ToString());
                }
            }
        }

    }
}
