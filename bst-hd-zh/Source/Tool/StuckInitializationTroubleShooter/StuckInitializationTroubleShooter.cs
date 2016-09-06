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
using System.Text;
using System.Text.RegularExpressions;
namespace BlueStacks.hyperDroid.Tool
{
    public class StuckInitializationTroubleShooter
    {
        static void Main(String[] args)
        {
            Logger.InitLog(null, "Stuck at Initialization Error TroubleShooter");
            Logger.Info("StuckInitializationTroubleShooter starting  ::::::: {0}", DateTime.Now);
            Logger.Info("StuckInitializationTroubleShooter: Starting agent PID {0}", Process.GetCurrentProcess().Id);
            Logger.Info("StuckInitializationTroubleShooter: CLR version {0}", Environment.Version);
            Logger.Info("StuckInitializationTroubleShooter: CurrentDirectory: {0}", Directory.GetCurrentDirectory());
            StuckInitializationTroubleShooter stuckInitializationTroubleShooter = new StuckInitializationTroubleShooter();

        }

        public StuckInitializationTroubleShooter()
        {
            try
            {
                Logger.Info("In Method StuckInitializationTroubleShooter");
                string mInstallDir;
                using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
                {
                    mInstallDir = (String)key.GetValue("InstallDir");
                }
                string adbPath = Path.Combine(mInstallDir, "HD-Adb.exe");
                int adbPort = Utils.GetAdbPort();

                String adbHost = String.Format("localhost:{0}", adbPort);
                

                while (true)
                {
                    //need to run multiple cases here 
                    //1. java.lang.RuntimeException: Unable to find android system package
                    //2. No SyncManager created!
                    //3. BOOT FAILURE or ServiceManager: service 'sensorservice' died     multiple times (5)
                    //4. lowmemorykiller: Killing  (android dumpstate) 30 times
                    //ServiceManager: Waiting for service SurfaceFlinger...
                    //Attempting to create new SOCKET connection
                    //Unable to read AndroidManifest.xml 10 times
                    //StringBuilder grepString = new StringBuilder("grep -irn java.lang.RuntimeException: Unable to find android system package");
                    //string logFilePath = Path.Combine(Path.GetTempPath(), "DumpStateLogs");
                    //StringBuilder args = new StringBuilder("-s " + adbHost + " shell dumpstate -d | grep ssafsafa");
                    //CmdRes cmdRes = RunCmd(adbPath, args, null);
                    //string output = cmdRes.StdOut;
                    //Logger.Info("Command outout: " + output);
                    //File.WriteAllText(logFilePath, output);

                    Logger.Info(string.Format("Dumping Android Logs at {0}:{1}",
            DateTime.Now.Minute, DateTime.Now.Second) + "  ::::::: {0}", DateTime.Now);

                    string logcatFilePath = Path.Combine(Path.GetTempPath(), "Logcat.log");
                    string dumpsysFilePath = Path.Combine(Path.GetTempPath(), "Dumpsys.log");
                    if (File.Exists(logcatFilePath))
                    {
                        Logger.Info("Logcat file already exists, deleting it");
                        File.Delete(logcatFilePath);
                    }

                    if (File.Exists(dumpsysFilePath))
                    {
                        Logger.Info("Dumpsys file already exists, deleting it");
                        File.Delete(dumpsysFilePath);
                    }

                    Thread dumpstateCollector = new Thread(delegate()
                    {
                        //adb -s localhost:5555  shell dumpsys content > logcat && adb -s localhost:5555 shell logcat -d >> logcat

                        Logger.Info("starting HD-ADB  ::::::: {0}", DateTime.Now);
                        Utils.RunCmdAsync(adbPath, "start-server");
                        Logger.Info("Started HD-ADB  ::::::: {0}", DateTime.Now);
                        Utils.RunCmdAsync(adbPath, String.Format("connect {0}", adbHost));

                        RunCmdWithList(adbPath,
                            new String[] { "-s", adbHost, "shell", "logcat -v threadtime -d *:v" },
                            logcatFilePath);

                        RunCmdWithList(adbPath,
                            new String[] { "-s", adbHost, "shell", "dumpsys content" },
                            dumpsysFilePath);

                        if (File.Exists(logcatFilePath))
                        {
                            Logger.Info("Logcat Dump file created");
                        }
                        else
                        {
                            Logger.Info("Logcat Dump failed");
                        }

                        if (File.Exists(dumpsysFilePath))
                        {
                            Logger.Info("DumpSys Content Dump file created");
                        }
                        else
                        {
                            Logger.Info("DumpSys Content Dump failed");
                        }
                    });

                    Logger.Info("DumpState collector starting  ::::::: {0}", DateTime.Now);
                    dumpstateCollector.Start();
                    dumpstateCollector.Join();
                    string logcatFileContent = File.ReadAllText(logcatFilePath);
                    string dumpsysFileContent = File.ReadAllText(dumpsysFilePath);
                    if (string.IsNullOrEmpty(logcatFileContent))
                    {
                        Logger.Info("Empty Logcat dump file");
                        break;
                    }
                    else
                    {
                        string javaLangRunTimeError = "java.lang.RuntimeException: Unable to find android system package";//logcat -v threadtime -d *:v
                        string noSyncManager = "No SyncManager created";//shell dumpsys content command
                        string unableReadAndroidManifest = "Unable to read AndroidManifest.xml"; //10 times logcat -v threadtime -d *:v
                        string bootFailure = "BOOT FAILURE";//logcat -v threadtime -d *:v
                        string sensorServiceDied = "ServiceManager: service 'sensorservice' died"; //5 times logcat -v threadtime -d *:v
                        string waitingForSurfaceFinger = "ServiceManager: Waiting for service SurfaceFlinger"; //10 tectimes logcat -v threadtime -d *:v
                        string incorrectMagicNo = "dalvikvm:\\s*DexOpt:\\s*incorrect opt magic number"; //logcat -v threadtime -d *:v
                        if ((FindStringInDumpLogs(logcatFileContent, javaLangRunTimeError) == true) ||
                            (FindStringInDumpLogs(logcatFileContent, unableReadAndroidManifest) == true) ||
                            (FindStringInDumpLogs(logcatFileContent, bootFailure) == true) ||
                            (FindStringInDumpLogs(logcatFileContent, sensorServiceDied) == true && (FindStringInDumpLogs(logcatFileContent, waitingForSurfaceFinger) == true)) ||
                            (Regex.IsMatch(logcatFileContent, incorrectMagicNo) == true) ||
                            DetectHeldbyTID(logcatFileContent)
                            )
                        {
                            DoAction();
                            break;

                        }
                        else
                        {
                            if (string.IsNullOrEmpty(dumpsysFileContent))
                            {
                                Logger.Info("Empty DumpSys Dump file");
                                break;
                            }
                            else
                            {
                                if ((FindStringInDumpLogs(dumpsysFileContent, noSyncManager) == true))
                                {
                                    DoAction();
                                    break;
                                }
                                else
                                {
                                    Logger.Info("Stuck at Initialization problem not occured");
                                }
                            }
                        }

                    }
                    //if (!string.IsNullOrEmpty(output))
                    //{
                    //    DoAction();
                    //    break;
                    //}
                    //else
                    //{
                    //    grepString.Remove(0, grepString.Length);
                    //    args.Remove(0, args.Length);
                    //    grepString.Append("grep -irn No SyncManager created");
                    //    args.Append("-s " + adbHost + " shell logcat -d -v threadtime | " + grepString);

                    //    cmdRes = RunCmd(adbPath, args, null);
                    //    output = cmdRes.StdOut;
                    //    Logger.Info("Command outout: " + output);
                    //    if (!string.IsNullOrEmpty(output))
                    //    {
                    //        DoAction();
                    //        break;
                    //    }
                    //    else
                    //    {
                    //        grepString.Remove(0, grepString.Length);
                    //        args.Remove(0, args.Length);
                    //        grepString.Append("grep -irn Unable to read AndroidManifest.xml | wc -l");
                    //        args.Append("-s " + adbHost + " shell logcat -d -v threadtime | " + grepString);

                    //        cmdRes = RunCmd(adbPath, args, null);
                    //        output = cmdRes.StdOut;
                    //        Logger.Info("Command outout: " + output);
                    //        int count = 0;
                    //        if (!string.IsNullOrEmpty(output) && int.TryParse(output, out count) && count >= 10)
                    //        {
                    //            DoAction();
                    //            break;
                    //        }
                    //        else
                    //        {
                    //            grepString.Remove(0, grepString.Length);
                    //            args.Remove(0, args.Length);
                    //            grepString.Append("grep -irn BOOT FAILURE");
                    //            args.Append("-s " + adbHost + " shell logcat -d -v threadtime | " + grepString);

                    //            cmdRes = RunCmd(adbPath, args, null);
                    //            output = cmdRes.StdOut;
                    //            Logger.Info("Command outout: " + output);
                    //            if (!string.IsNullOrEmpty(output))
                    //            {
                    //                DoAction();
                    //                break;
                    //            }
                    //            else
                    //            {
                    //                grepString.Remove(0, grepString.Length);
                    //                args.Remove(0, args.Length);
                    //                grepString.Append("grep -irn ServiceManager: service 'sensorservice' died | wc -l");
                    //                args.Append("-s " + adbHost + " shell logcat -d -v threadtime | " + grepString);

                    //                cmdRes = RunCmd(adbPath, args, null);
                    //                output = cmdRes.StdOut;
                    //                Logger.Info("Command outout: " + output);
                    //                count = 0;
                    //                //ServiceManager: service 'sensorservice' died 5 times
                    //                if (!string.IsNullOrEmpty(output) && int.TryParse(output, out count) && count >= 5)
                    //                {
                    //                    DoAction();
                    //                    break;
                    //                }
                    //                else
                    //                {
                    //                    grepString.Remove(0, grepString.Length);
                    //                    args.Remove(0, args.Length);
                    //                    grepString.Append("grep -irn ServiceManager: Waiting for service SurfaceFlinger | wc -l");
                    //                    args.Append("-s " + adbHost + " shell logcat -d -v threadtime | " + grepString);

                    //                    cmdRes = RunCmd(adbPath, args, null);
                    //                    output = cmdRes.StdOut;
                    //                    Logger.Info("Command outout: " + output);
                    //                    count = 0;
                    //                    //ServiceManager: Waiting for service SurfaceFlinger 5 times
                    //                    if (!string.IsNullOrEmpty(output) && int.TryParse(output, out count) && count >= 10)
                    //                    {
                    //                        DoAction();
                    //                        break;
                    //                    }
                    //                    else
                    //                    {
                    //                        grepString.Remove(0, grepString.Length);
                    //                        args.Remove(0, args.Length);
                    //                        grepString.Append("grep -irn ServiceManager: Waiting for service SurfaceFlinger | wc -l");
                    //                        args.Append("-s " + adbHost + " shell logcat -d -v threadtime | " + grepString);
                    //                    }
                    //                }
                    //            }
                    //        }
                    //    }
                    //}

                    //Thread.Sleep(10000);
                }
                Logger.Info("Exiting StuckInitializationTroubleShooter :::: {0}", DateTime.Now);
                Environment.Exit(0);
            }
            catch (Exception ex)
            {
                Logger.Error("Error occured, Err: {0}", ex.ToString());
                Environment.Exit(-1);
            }
        }

        private bool FindStringInDumpLogs(string logFileContent, string stringToBeSearched)
        {
            bool stringFound = logFileContent.IndexOf(stringToBeSearched, StringComparison.OrdinalIgnoreCase) >= 0;
            if (stringFound)
            {
                Logger.Info(stringToBeSearched + " found in dumpstate");
            }
            else
            {
                Logger.Info(stringToBeSearched + " not found in dumpstate");
            }
            return stringFound;
        }

        private void RunCmdWithList(String prog, String[] argList,
        String outPath)
        {
            try
            {
                List<String> quoted = new List<String>();

                foreach (String arg in argList)
                    quoted.Add(String.Format("\"{0}\"", arg));

                RunCmd(prog, String.Join(" ", quoted.ToArray()),
                    outPath);

            }
            catch (Exception exc)
            {

                Console.WriteLine(exc.ToString());
            }
        }

        private void RunCmd(String prog, String args, String outPath)
        {
            try
            {
                RunCmdInternal(prog, args, outPath);

            }
            catch (Exception exc)
            {

                Console.WriteLine(exc.ToString());
            }
        }

        private void RunCmdInternal(String prog, String args, String outPath)
        {
            StreamWriter writer = null;
            Process proc = new Process();

            Console.WriteLine("Running Command  :::::::  {0}", DateTime.Now);
            Console.WriteLine("    prog: " + prog);
            Console.WriteLine("    args: " + args);
            Console.WriteLine("    out:  " + outPath);

            proc.StartInfo.FileName = prog;
            proc.StartInfo.Arguments = args;

            proc.StartInfo.UseShellExecute = false;
            proc.StartInfo.CreateNoWindow = true;

            if (outPath != null)
            {
                writer = new StreamWriter(outPath);

                proc.StartInfo.RedirectStandardInput = true;
                proc.StartInfo.RedirectStandardOutput = true;
                proc.StartInfo.RedirectStandardError = true;

                proc.OutputDataReceived += delegate(object obj,
                    DataReceivedEventArgs line)
                {
                    writer.WriteLine(line.Data);
                };

                proc.ErrorDataReceived += delegate(object obj,
                    DataReceivedEventArgs line)
                {
                    writer.WriteLine(line.Data);
                };
            }

            proc.Start();

            if (outPath != null)
            {
                proc.BeginOutputReadLine();
                proc.BeginErrorReadLine();
            }

            proc.WaitForExit();

            if (outPath != null)
            {
                writer.Close();
            }
        }

        private bool DetectHeldbyTID(string logFileContent)
        {
            string waitingForSurfaceFinger = "at android.view.SurfaceControl.nativeCreate(Native Method)";
            int index = logFileContent.IndexOf(waitingForSurfaceFinger, StringComparison.OrdinalIgnoreCase);
            if (index >= 0)
            {
                //get 1st string from line and then search for held by tid
                string heldByTID = "held by tid";
                List<int> listOfIndex = new List<int>();
                listOfIndex = AllIndexesOf(logFileContent, heldByTID);
                int max = -1;
                foreach (int item in listOfIndex)
                {
                    if (index > item)
                    {
                        max = Math.Max(item, max);
                    }
                }
                if (listOfIndex == null || listOfIndex.Count == 0)
                {
                    return false;
                }

                string substring = logFileContent.Substring(max, (index - max));
                //if TID value is found then retrun true else false
                string tidValue = "";
                if (substring.IndexOf(tidValue) >= -1)
                {
                    return true;
                }
                else
                {
                    return false;
                }

            }
            else
            {
                return false;
            }
        }

        private List<int> AllIndexesOf(string str, string value)
        {
            List<int> indexes = new List<int>();
            for (int index = 0; ; index += value.Length)
            {
                index = str.IndexOf(value, index);
                if (index == -1)
                    return indexes;
                indexes.Add(index);
            }
        }

        private void DoAction()
        {
            Logger.Info("Stuck at Initialization Error detected");

            DialogResult result = MessageBox.Show(Locale.Strings.TROUBLESHOOTER_TEXT,
        Locale.Strings.STUCK_AT_INITIALIZING_FORM_TEXT, MessageBoxButtons.OKCancel);

            if (result == DialogResult.OK)
            {
                Logger.Info("User clicked yes");
                RunTroubleShooterExe("HD-Restart.exe",
                        "Android",
                        Locale.Strings.WORK_DONE_TEXT,
                        Locale.Strings.STUCK_AT_INITIALIZING_FORM_TEXT);
            }
            else
            {
                Logger.Info("User clicked No");
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

        public static CmdRes RunCmd(String prog, StringBuilder args, String outPath)
        {
            try
            {
                return RunCmdInternal(prog, args.ToString(), outPath, true);

            }
            catch (Exception exc)
            {

                Logger.Error(exc.ToString());
            }

            return new CmdRes();
        }

        private static CmdRes RunCmdInternal(String prog, string args, String outPath, bool enableLog)
        {
            StreamWriter writer = null;
            Process proc = new Process();

            Logger.Info("Running Command");
            Logger.Info("    prog: " + prog);
            Logger.Info("    args: " + args);
            Logger.Info("    out:  " + outPath);

            CmdRes res = new CmdRes();

            proc.StartInfo.FileName = prog;
            proc.StartInfo.Arguments = args.ToString();

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

            Logger.Info("Stuck at Initialization TroubleShooter Error: " + res.StdErr);
            Logger.Info("Stuck at Initialization TroubleShooter Output: " + res.StdOut);

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
