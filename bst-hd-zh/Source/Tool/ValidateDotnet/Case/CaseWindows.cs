using System;
using System.Collections.Generic;
using System.Text;

namespace CaseDotnet
{
    class CaseWindows
    {
        [CaseDotnet]
        public bool CaseForm()
        {
            System.Windows.Forms.Form form = new System.Windows.Forms.Form();
            form.Hide();
            return true;
        }
    }
}
