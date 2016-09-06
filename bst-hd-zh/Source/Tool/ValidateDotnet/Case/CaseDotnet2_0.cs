using System;
using System.Collections.Generic;
using System.Text;
using System.Net;
using System.IO;
using System.Runtime.InteropServices;
using System.Diagnostics;

namespace CaseDotnet
{
    class CaseDotnet2_0
    {
        [DllImport("kernel32.dll")]
        public static extern int SetProcessWorkingSetSize(IntPtr handle, long minWorkingSetSize, long maxWorkingSetSize);

        [CaseDotnet]
        public bool CaseHttpWebRequest()
        {
            return true;
        }
        [CaseDotnet]
        public bool CaseUnexpectedly()
        {
            return true;
        }

        [CaseDotnet]
        public bool CaseSetSize()
        {
            SetProcessWorkingSetSize(Process.GetCurrentProcess().Handle, -1, -1);
            return true;
        }

        /// <summary>
        /// the property CanEnableIme is not  visible in .net2.0
        /// so if the .net is 2.0, the code is not work
        /// </summary>
        class CaseForm : System.Windows.Forms.Form
        {
            protected override bool CanEnableIme
            {
                get
                {
                    return base.CanEnableIme;
                }
            }
        }
    }

    
     
}
