using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Threading;
using System.IO;

using BlueStacks.hyperDroid.Frontend;

using Microsoft.Win32;
using BlueStacks.hyperDroid.Locale;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.FrontendForm
{
   public static class Program
    {
        [DllImport("user32.dll", SetLastError=true)]
	    static extern bool SetProcessDPIAware();

	    public static  Mutex	sFrontendLock;		/* prevent GC */

        private static DateTime sFrontendLaunchTime;
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main(string[] args)
        {
            ValidateArgs(args);

            InitLog(args[0]);
	    Utils.LogParentProcessDetails();
            string vmName = args[0];
            Opt opt = new Opt();
            opt.Parse(args);
            bool hideMode = false;
            if (opt.h)
            {
                hideMode = true;
            }
            if (BlueStacks.hyperDroid.Common.Utils.IsAlreadyRunning(BlueStacks.hyperDroid.Common.Strings.FrontendLockName, out sFrontendLock))
            {
                Logger.Info("Frontend already running");

                RegistryKey key = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.HKLMAndroidConfigRegKeyPath);
                int frontendPort = (int)key.GetValue("FrontendServerPort");
                key.Close();


                IntPtr handle = IntPtr.Zero;
                if (!hideMode)
                {
                    handle = BringToFront(args[0]);
                }

                Logger.Info("Sending WM_USER_SHOW_WINDOW to Frontend handle {0}", handle);
                int res = BlueStacks.hyperDroid.Common.Interop.Window.SendMessage(handle,
                        BlueStacks.hyperDroid.Common.Interop.Window.WM_USER_SHOW_WINDOW,
                        IntPtr.Zero,
                        IntPtr.Zero);
                /*
                 * Check if the message sent has been handled
                 * res will be 1 in case it is
                 */
                if (handle != IntPtr.Zero && res != 1)
                {
                    string url = String.Format("http://127.0.0.1:{0}/{1}",
                            frontendPort, BlueStacks.hyperDroid.Common.Strings.ShowWindowUrl);
                    Dictionary<string, string> data = new Dictionary<string, string>();
                    Logger.Info("Sending request to: " + url);

                    try
                    {
                        string result = BlueStacks.hyperDroid.Common.HTTP.Client.Post(url, data, null, false);
                        Logger.Info("showwindow result: " + result);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                        Logger.Error("Post failed. url = {0}, data = {1}", url, data);
                    }
                }
                Environment.Exit(0);

            }
            

            if (!Utils.IsOSWinXP())
            {
                SetProcessDPIAware();
            }
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            //FrontendForm frontendForm = new FrontendForm(vmName, hideMode);
            //Application.Run();

			Application.Run(new FrontendForm(vmName, hideMode));
        }

        private static IntPtr BringToFront(string vmName)
        {
            /*
             * Is the frontend running fullscreen?
             */

            String path = String.Format(@"{0}\{1}\FrameBuffer\0",
                    BlueStacks.hyperDroid.Common.Strings.GuestRegKeyPath, vmName);
            bool success = true;

            RegistryKey key;
            bool fullScreen;

            using (key = Registry.LocalMachine.OpenSubKey(path))
            {
                fullScreen = (int)key.GetValue("FullScreen", 0) != 0;
            }

            /*
             * Try to bring the frontend window to the foreground.
             */

            BlueStacks.hyperDroid.Common.Logger.Info(String.Format("Starting BlueStacks {0} Frontend",
                        vmName));
			String name = Oem.Instance.CommonAppTitleText;

            IntPtr handle = IntPtr.Zero;

            try
            {
                handle = BlueStacks.hyperDroid.Common.Interop.Window.BringWindowToFront(name, fullScreen);
            }
            catch (Exception exc)
            {

                Logger.Error("Cannot bring existing frontend " +
                        "window for VM {0} to the foreground", vmName);
                Logger.Error(exc.ToString());
                success = false;
            }
            
            return handle;
        }
        public class Opt : BlueStacks.hyperDroid.Common.GetOpt
        {
            public String n = "";//AppName
            public String i = "";//AppIconName
            public String pkg = "";//AppPackage
            public String url = "";//ApkUrl
            public bool h = false;//hidemode
            public bool dbgWait = false;	// wait for debugger before starting frontend
            public bool dontStartVm = false; // for testing only, hd-service will not be started
        }
        private static void Usage()
        {
            String prog = Process.GetCurrentProcess().ProcessName;
            String capt = "BlueStacks Frontend";
            String text = "";

            text += String.Format("Usage:\n");
            text += String.Format("    {0} <vm name>\n", prog);

            if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMessageBoxToBeDisplayed)
            {
                Logger.Info("Displaying Message box");
                MessageBox.Show(text, capt);
                Logger.Info("Displayed Message box");
            }
            Environment.Exit(1);
        }

        public static void ValidateArgs(String[] args)
        {
            if (args.Length < 1)
                Usage();
        }


        private static void StartFrontendCrashDebugging()
        {
            string reason = "";
            int exitCode = -1;
            try
            {
                Logger.Info("In StartFrontendCrashDebugging");
                if (Utils.IsFrontendCrashReasonknown(sFrontendLaunchTime, out reason, out exitCode) == true)
                {
                    Logger.Info("Reason for fe crash: {0}", reason);
                    bool logsSent = Utils.CheckIfErrorLogsAlreadySent(BlueStacks.hyperDroid.Common.Strings.BootFailureCategory, exitCode);

                    if (logsSent == false)
                    {
                        string installDir;
                        using (RegistryKey key = Registry.LocalMachine.OpenSubKey(BlueStacks.hyperDroid.Common.Strings.RegBasePath))
                        {
                            installDir = (String)key.GetValue("InstallDir");
                        }
                        Process.Start(Path.Combine(installDir, "HD-LogCollector.exe"),
                                string.Format("-boot \"{0}\" {1}", reason, exitCode));
                    }

                    WindowMessages.NotifyBootFailureToParentWindow(exitCode);
                }
                else
                {
                    Logger.Info("Reason Unknown");
                    WindowMessages.NotifyExeCrashToParentWindow();
                }
            }
            catch (Exception e)
            {
                Logger.Error("Error occured in StartFrontendCrashDebugging, Err: {0}", e.ToString());
            }
        }
        
        private static void InitLog(String vmName)
        {
            Logger.InitLog(null, "Frontend");

            System.Console.SetOut(Logger.GetWriter());
            System.Console.SetError(Logger.GetWriter());

            AppDomain.CurrentDomain.ProcessExit += delegate(
                object sender, EventArgs evtArgs)
            {
                Logger.Info("Exiting frontend PID {0}", Process.GetCurrentProcess().Id);
            };

            Logger.Info("Starting frontend PID {0}",
                Process.GetCurrentProcess().Id);

            Logger.Info("CLR version {0}", Environment.Version);
            Logger.Info("IsAdministrator: {0}", User.IsAdministrator());

            Application.ThreadException += delegate(Object obj,
                System.Threading.ThreadExceptionEventArgs evt)
            {
                Logger.Error("Unhandled Exception:");
                Logger.Error(evt.Exception.ToString());

                if (BlueStacks.hyperDroid.Common.Oem.Instance.IsStartFrontendCrashDebugging)
                {
                    StartFrontendCrashDebugging();
                }

                Environment.Exit(1);
            };

            Application.SetUnhandledExceptionMode(
                UnhandledExceptionMode.CatchException);

            AppDomain.CurrentDomain.UnhandledException += delegate(
                Object obj, UnhandledExceptionEventArgs evt)
            {
                Logger.Error("Unhandled Exception:");
                Logger.Error(evt.ExceptionObject.ToString());

                if (BlueStacks.hyperDroid.Common.Oem.Instance.IsStartFrontendCrashDebugging)
                {
                    StartFrontendCrashDebugging();
                }

                Environment.Exit(1);
            };
        }

    }
}
