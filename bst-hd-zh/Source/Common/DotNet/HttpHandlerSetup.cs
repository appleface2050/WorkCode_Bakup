using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Net;
using System.Net.Security;
using BlueStacks.hyperDroid.Common.HTTP;

namespace BlueStacks.hyperDroid.Common
{
    public class HttpHandlerSetup
    {
        public static Common.HTTP.Server Server;
        public static void InitHTTPServer(Dictionary<String, Common.HTTP.Server.RequestHandler> routes, string homeDir, bool isPartneratSamePort)
        {
            int port = 2871;

            for (; port < 2880; port++)
            {
                try
                {
                    Server = new Common.HTTP.Server(port, routes, homeDir);
                    Server.Start();
                    Logger.Info("Frontend server listening on port: " + Server.Port);
                }
                catch (Exception e)
                {
                    Logger.Error(String.Format("Error Occured, Err: {0}", e.ToString()));
                    continue;
                }

                SetFrontendPortInBootParams(port);

                Thread fqdnThread = new Thread(SendFqdn);
                fqdnThread.IsBackground = true;
                fqdnThread.Start(port);

                /* write server port to the registry */
                RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
                key.SetValue("FrontendServerPort", Server.Port, RegistryValueKind.DWord);
                key.Close();

                if (isPartneratSamePort)
                {
                    key = Registry.LocalMachine.CreateSubKey(Common.Strings.HKLMConfigRegKeyPath);
                    key.SetValue("PartnerServerPort", Server.Port, RegistryValueKind.DWord);
                    key.Close();

                    Thread fqdnThreadGM = new Thread(SendFqdnGM);
                    fqdnThreadGM.IsBackground = true;
                    fqdnThreadGM.Start(Server.Port);
                }

                Server.Run();
                break;
            }

            if (port == 2880)
            {
                Logger.Error("No free port available");
                Environment.Exit(2);
            }
        }

        private static void SetFrontendPortInBootParams(int frontendPort)
        {
            RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.AndroidKeyBasePath);
            if (key != null)
            {
                string bootPrms = (string)key.GetValue("BootParameters", "");
                string[] paramParts = bootPrms.Split(' ');
                string newBootParams = "";
                string fullAddr = string.Format("10.0.2.2:{0}", frontendPort);

                if (bootPrms.IndexOf(Common.Strings.FrontendPortBootParam) == -1)
                {
                    newBootParams = bootPrms + " " + Common.Strings.FrontendPortBootParam + "=" + fullAddr;
                }
                else
                {
                    foreach (string param in paramParts)
                    {
                        if (param.IndexOf(Common.Strings.FrontendPortBootParam) != -1)
                        {
                            if (!String.IsNullOrEmpty(newBootParams))
                            {
                                newBootParams += " ";
                            }
                            newBootParams += Common.Strings.FrontendPortBootParam + "=" + fullAddr;
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

        private static void SendFqdn(object frontendPort)
        {
            RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);

            while (true)
            {
                if (Common.VmCmdHandler.FqdnSend((int)frontendPort, "Frontend") != null)
                    break;
                Thread.Sleep(2000);
            }
        }


        private static void SendFqdnGM(object gmPort)
        {
            try
            {
                while (true)
                {
                    if (VmCmdHandler.FqdnSend((int)gmPort, "GameManager") != null)
                    {
                        break;
                    }

                    Thread.Sleep(2000);
                }
            }
            catch (Exception e)
            {
                Logger.Info(e.ToString());
            }
        }
    }

}