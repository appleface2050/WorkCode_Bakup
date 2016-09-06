using System;
using System.Collections.Generic;
using System.Text;
using System.Management;

namespace CaseDotnet
{
    class CaseManagement
    {
        [CaseDotnet]
        public bool Management()
        {
            ManagementObjectSearcher searcher = new ManagementObjectSearcher("test");
            searcher.Get();
            return true;
        }
    }
}
