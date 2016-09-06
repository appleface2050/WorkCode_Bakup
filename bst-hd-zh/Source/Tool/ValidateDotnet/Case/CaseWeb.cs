using System;
using System.Collections.Generic;
using System.Text;

namespace CaseDotnet
{
    class CaseWeb
    {
        [CaseDotnet]
        public bool Http()
        {
            {
                Type t = typeof(System.Web.HttpRequest);
                t.GetMethods();
            }
            {
                Type t = typeof(System.Web.HttpResponse);
                t.GetMethods();
            }
            {
                System.Net.HttpListener listener = new System.Net.HttpListener();

                listener.Close();
            }
            return true;
        }
    }
}
