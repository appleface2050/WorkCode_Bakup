using System;
using System.Collections.Generic;
using System.Text;
using System.Security.Cryptography;

namespace CaseDotnet
{
    class CaseSecurity
    {
        private static byte[] s_Entropy = new byte[] { 0x7a, 0x69, 0x6e, 0x67, 0x6d, 0x70, 0x65, 0x67 };
        [CaseDotnet]
        public bool Secrity()
        {
            byte[] buff = Encoding.UTF8.GetBytes("test");
            ProtectedData.Protect(buff, s_Entropy, DataProtectionScope.CurrentUser);
            return true;
        }
    }
}
