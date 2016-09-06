using System;
using System.Collections.Generic;
using System.Text;
using Microsoft.Win32;
using System.Text.RegularExpressions;
using BlueStacks.hyperDroid.Common;

namespace SetUserGuid
{
    class Program
    {
        static void Main(string[] args)
        {
            const string nameUserGuid = "USER_GUID";
            const string nameBootParameters = "BootParameters";
            const string itemAndroid = "Guests\\Android";
            
            try
            {
                Logger.InitUserLog();
                Logger.Info("HDSetUserGuid: Starting agent PID {0}", System.Diagnostics.Process.GetCurrentProcess().Id);
                Logger.Info("HDSetUserGuid: CLR version {0}", Environment.Version);
                Logger.Info("HDSetUserGuid: IsAdministrator: {0}", User.IsAdministrator());
                string regBase = Strings.RegBasePath;

                string vUserGuid = string.Empty;
                using (RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(regBase, false))
                {
                    if (null != baseKey)
                    {
                        object otemp = baseKey.GetValue(nameUserGuid);
                        if (null != otemp)
                        {
                            vUserGuid = otemp.ToString();
                        }
                        else
                        {
                            Logger.Error("can not read registry value: {0}", nameUserGuid);
                        }
                    }
                    else
                    {
                        Logger.Error("can not open registry key: {0}", regBase);
                    }


                    #region read BootParameters, then replace GUID with  USER_GUID
                    if (!string.IsNullOrEmpty(vUserGuid))
                    {
                        using (RegistryKey androidKey = baseKey.OpenSubKey(itemAndroid, true))
                        {
                            if (null != androidKey)
                            {
                                object otemp = androidKey.GetValue(nameBootParameters);
                                if (null != otemp)
                                {
                                    Regex regex = new Regex(" GUID=\\S+\\s");
                                    string oldValue = otemp.ToString();
                                    string newGuid = " GUID=" + vUserGuid + " ";
                                    string newValue = regex.Replace(oldValue, newGuid);
                                    if (newValue != oldValue)
                                    {
                                        androidKey.SetValue(nameBootParameters, newValue);
                                        Logger.Info("Set GUID: {0}", vUserGuid);
                                    }
                                    else
                                    {
                                        Logger.Info("GUID is same: {0}", vUserGuid);
                                    }
                                }
                                else
                                {
                                    Logger.Error("can not read registry value: {0}", nameBootParameters);
                                }
                            }
                            else
                            {
                                Logger.Error("can not open registry key: {0}", itemAndroid);
                            }
                        }
                    }
                    #endregion
                }
            }
            catch (Exception e)
            {
                Logger.Error(e.Message);
            }

            Logger.Info("Exiting HDSetUserGuid PID {0}", System.Diagnostics.Process.GetCurrentProcess().Id);
        }
    }
}
