using System;
using System.Collections.Generic;
using System.Text;

namespace CaseDotnet
{
    class CaseXml
    {
        [CaseDotnet]
        public bool Xml()
        {
            System.Xml.XmlDocument xml = new System.Xml.XmlDocument();
            return true;
        }
    }
}
