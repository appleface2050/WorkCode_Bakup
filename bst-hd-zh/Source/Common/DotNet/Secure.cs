using System;
using System.Text;
using System.Security.Cryptography;

namespace BlueStacks.hyperDroid.Common
{
	public class SecureUserData
	{
		public class ESecure : Exception
		{
			public ESecure(String reason)
				: base(reason)
			{
			}
		}

		private static byte[] s_Entropy = new byte[] { 0x7a, 0x69, 0x6e, 0x67, 0x6d, 0x70, 0x65, 0x67 };
		public static byte[] Encrypt(String data)
		{
			if (data == null)
				throw new ESecure("Cannot encrypt null string");

			if (data.Length == 0)
				throw new ESecure("Cannot encrypt empty string");

			byte[] buff = Encoding.UTF8.GetBytes(data);

			return ProtectedData.Protect(buff, s_Entropy, DataProtectionScope.CurrentUser);
		}

		public static String Decrypt(byte[] data)
		{
			byte[] buff = ProtectedData.Unprotect(data, s_Entropy, DataProtectionScope.CurrentUser);
			return Encoding.UTF8.GetString(buff);
		}
	}
}
