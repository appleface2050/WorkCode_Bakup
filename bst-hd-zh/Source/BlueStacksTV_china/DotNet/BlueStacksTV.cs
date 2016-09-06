using System;
using System.IO;
using System.Net;
using System.Text;
using System.Timers;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Reflection;
using System.Net.Sockets;
using System.Diagnostics;
using System.Net.Security;
using System.Windows.Forms;
using System.Management;
using System.ComponentModel;
using System.Collections.Generic;
using Microsoft.Win32.SafeHandles;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Security.Cryptography.X509Certificates;

using CodeTitans.JSon;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class BlueStacksTV : ApplicationContext
	{

		public static RegistryKey sConfigKey;
        public static int sBlueStacksTVPort = 2885;
        public static string sApplicationBaseUrl = "http://localhost:2881/static/";

        public static string sServerRootDir = null;
        public static string sApplicationServerPort = null;
        static private Mutex sBTVLock;
		public static BlueStacksTV Instance;

		[STAThread]
		public static void Main(String [] args)
		{
			Init();

            if (Oem.Instance.OEM == "gamemanager" || Oem.Instance.OEM == "btv" || Oem.Instance.OEM == "bluestacks")
            {
                sConfigKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
                int gmPort = (int)sConfigKey.GetValue("GameManagerServerPort", 2871);
                sApplicationServerPort = gmPort.ToString();
                sServerRootDir = Common.Strings.GameManagerHomeDir;
            }
            else
            {
                sConfigKey = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath, true);
                int partnerPort = (int)sConfigKey.GetValue("PartnerServerPort", 2881);
                sApplicationServerPort = partnerPort.ToString();
                sServerRootDir = Common.Strings.BTVDir;
            }
            sApplicationBaseUrl = "http://localhost:" + sApplicationServerPort + "/static/";
			//http server
			Thread thread = new Thread(SetupHTTPServer);
			thread.IsBackground = true;
			thread.Start();

			Instance = new BlueStacksTV();
			Application.Run(Instance);
		}

		private static void Init()
        {
            Logger.InitLog("BlueStacksTV", "bluestackstv");
            Logger.Info("Starting BlueStacksTV PID {0}", Process.GetCurrentProcess().Id);
            Logger.Info("BlueStacksTV: CLR version {0}", Environment.Version);
            Logger.Info("IsAdministrator: {0}", User.IsAdministrator());

            System.Windows.Forms.Application.ThreadException += delegate(Object obj,
                    System.Threading.ThreadExceptionEventArgs evt)
            {
                Logger.Error("BlueStacksTV: Unhandled Exception:");
                Logger.Error(evt.Exception.ToString());
				HandleClose();
            };

            System.Windows.Forms.Application.SetUnhandledExceptionMode(
                    System.Windows.Forms.UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += delegate(
                    Object obj, UnhandledExceptionEventArgs evt)
            {
                Logger.Error("BlueStacksTV: Unhandled Exception:");
                Logger.Error(evt.ExceptionObject.ToString());
				HandleClose();
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

		private static void HandleClose()
		{
			sConfigKey.Close();
			sBTVLock.Close();
			try
			{
				Common.Utils.KillProcessByName("HD-OBS");
			}
			catch (Exception ex)
			{
				Logger.Error("Failed to kill obs... Err : " + ex.ToString());
			}
			Environment.Exit(1);
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
            routes.Add("/setcameraposition", HTTPHandler.SetCameraPositionHandler);

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
