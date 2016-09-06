using System;
using System.Drawing;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.ServiceProcess;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
namespace BlueStacks.hyperDroid.Tool
{
    public class RPCErrorTroubleShooter
    {
        static void Main(String[] args)
        {
            Logger.InitLog(null, "RPC Error");
            Logger.Info("HDRPCErrorTroubleShooter: Starting agent PID {0}", Process.GetCurrentProcess().Id);
            Logger.Info("HDRPCErrorTroubleShooter: CLR version {0}", Environment.Version);
            Logger.Info("HDRPCErrorTroubleShooter: CurrentDirectory: {0}", Directory.GetCurrentDirectory());

            RPCErrorTroubleShooter rpcTroubleShooter = new RPCErrorTroubleShooter();

        }
        //static private LogCollector sLogCollector;
        public RPCErrorTroubleShooter()
        {
            try
            {
                Logger.Info("In Method RPCErrorTroubleShooter");
                string mInstallDir;
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
                {
                    mInstallDir = (String)key.GetValue("InstallDir");
                }
                string adbPath = Path.Combine(mInstallDir, "HD-Adb.exe");
                int adbPort = Utils.GetAdbPort();

                String adbHost = String.Format("localhost:{0}", adbPort);
                Logger.Info("Starting HD-ADB");
                Utils.RunCmdAsync(adbPath, "start-server");
                Thread.Sleep(3000);
                Logger.Info("Started HD-ADB");
                Utils.RunCmdAsync(adbPath, String.Format("connect {0}", adbHost));
                Thread.Sleep(250);
                
                while (true)
                {
                    string args = "-s " + adbHost + " shell logcat -d -v threadtime | grep -rn RPC:S";
                    
                    CmdRes cmdRes = RunCmd(adbPath, args, null);
                    string output = cmdRes.StdOut;
                    Logger.Info("Command outout: " + output);
                    if (!string.IsNullOrEmpty(output))
                    {
                        Logger.Info("RPC Error detected");
                        DialogResult result = MessageBox.Show(Locale.Strings.TROUBLESHOOTER_TEXT,
                    Locale.Strings.RPC_FORM_TEXT, MessageBoxButtons.OKCancel);
                        if (result == DialogResult.OK)
                        {
                            Logger.Info("User clicked yes");
                            RunTroubleShooterExe("HD-GuestCommandRunner.exe",
                                    "",
                        Locale.Strings.WORK_DONE_TEXT,
                        Locale.Strings.RPC_FORM_TEXT);
                            //Once We r done, exit this process
                        }
                        else
                        {
                            Logger.Info("User clicked No");
                        }
                        break;
                    }

                    Thread.Sleep(10000);
                }

                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Error("Error occured, Err: {0}", ex.ToString());
                Environment.Exit(-1);
            }
        }

        private void RunTroubleShooterExe(string fileName, string args, string text, string title)
        {
            try
            {
                Logger.Info("In Method RunTroubleShooterExe");
                RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
                string filePath = Path.Combine(
                        (string)key.GetValue("InstallDir"),
                        fileName);

                Process proc = new Process();
                proc.StartInfo.FileName = filePath;
                proc.StartInfo.Arguments = args;
                proc.EnableRaisingEvents = true;

                proc.Exited += new EventHandler(delegate(object sender, EventArgs e)
                {
                    MessageBox.Show(text, title, MessageBoxButtons.OK);
                });
                proc.Start();
                proc.WaitForExit();
                proc.Close();
            }
            catch (Exception e)
            {
                Logger.Error("Error occured, Err: {0}", e.ToString());
            }
        }

        public static CmdRes RunCmd(String prog, String args, String outPath)
        {
            try
            {
                return RunCmdInternal(prog, args, outPath, true);

            }
            catch (Exception exc)
            {

                Logger.Error(exc.ToString());
            }

            return new CmdRes();
        }

        private static CmdRes RunCmdInternal(String prog, String args, String outPath, bool enableLog)
        {
            StreamWriter writer = null;
            Process proc = new Process();

            Logger.Info("Running Command");
            Logger.Info("    prog: " + prog);
            Logger.Info("    args: " + args);
            Logger.Info("    out:  " + outPath);

            CmdRes res = new CmdRes();

            proc.StartInfo.FileName = prog;
            proc.StartInfo.Arguments = args;

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            if (outPath != null)
            {
                writer = new StreamWriter(outPath);
            }

            proc.StartInfo.RedirectStandardInput = true;
            proc.StartInfo.RedirectStandardOutput = true;
            proc.StartInfo.RedirectStandardError = true;

            proc.OutputDataReceived += delegate(object obj,
                    DataReceivedEventArgs line)
            {
                if (outPath != null)
                {
                    writer.WriteLine(line.Data);
                }
                string stdout = line.Data;
                if (stdout != null && (stdout = stdout.Trim()) != String.Empty)
                {
                    if (enableLog)
                        Logger.Info(proc.Id + " OUT: " + stdout);
                    res.StdOut += stdout + "\n";
                }
            };

            proc.ErrorDataReceived += delegate(object obj,
                    DataReceivedEventArgs line)
            {
                if (outPath != null)
                {
                    writer.WriteLine(line.Data);
                }
                if (enableLog)
                    Logger.Error(proc.Id + " ERR: " + line.Data);
                res.StdErr += line.Data + "\n";
            };

            proc.Start();
            proc.BeginOutputReadLine();
            proc.BeginErrorReadLine();
            proc.WaitForExit();
            res.ExitCode = proc.ExitCode;

            if (enableLog)
                Logger.Info(proc.Id + " ExitCode: " + proc.ExitCode);

            if (outPath != null)
            {
                writer.Close();
            }

            Logger.Info("RPC TroubleShooter Error: " + res.StdErr);
            Logger.Info("RPC TroubleShooter Output: " + res.StdOut);

            return res;
        }

        public class CmdRes
        {
            public String StdOut = "";
            public String StdErr = "";
            public int ExitCode;
        }
    }
    
}
