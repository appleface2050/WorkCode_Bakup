using System;
using System.Collections.Generic;
using System.Windows.Forms;
using System.IO;

namespace CaseDotnet
{
    static class Program
    {
        [System.Runtime.InteropServices.DllImport("kernel32.dll")]
        static extern ErrorModes SetErrorMode(ErrorModes uMode);

        [Flags]
        public enum ErrorModes : uint
        {
            SYSTEM_DEFAULT = 0x0,
            SEM_FAILCRITICALERRORS = 0x0001,
            SEM_NOALIGNMENTFAULTEXCEPT = 0x0004,
            SEM_NOGPFAULTERRORBOX = 0x0002,
            SEM_NOOPENFILEERRORBOX = 0x8000
        }
        /// <summary>
        /// The main entry point for the application.
        /// </summary>
        [STAThread]
        static void Main()
        {
            //System.Environment.Exit(1);
            SetErrorMode(SetErrorMode(ErrorModes.SYSTEM_DEFAULT) | ErrorModes.SEM_NOGPFAULTERRORBOX);
            try{

                Console.Out.WriteLine("started the process");
                Application.EnableVisualStyles();
                Application.SetCompatibleTextRenderingDefault(false);
                bool b = CaseUtils.DoCase(null);

                Console.Out.WriteLine("finished the process");
                //throw new Exception("sdfds");

                if (b)
                {
                    System.Console.WriteLine("0");
                    System.Environment.Exit(0);
                }
                else
                {
                    System.Console.WriteLine("1");
                    System.Environment.Exit(1);
                }
            }catch(Exception e)
            {
                Console.Out.WriteLine(e.Message);
            }
        }
    }
}
