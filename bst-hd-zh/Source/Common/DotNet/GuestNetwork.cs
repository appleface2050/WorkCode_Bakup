using System;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Common
{

	public class GuestNetwork
	{

		public static int GetHostPort(bool isUdp, int guestPort)
		{
			String name = String.Format("{0}/{1}",
					isUdp ? "udp" : "tcp", guestPort);

			RegistryKey key = Registry.LocalMachine.OpenSubKey(
				Common.Strings.AndroidKeyBasePath +
				@"Network\Redirect");

			if (key != null)
				return (int)key.GetValue(name, -1);

			Logger.Error("CRITICAL: Could not find guestPort");
			return -1;
		}
	}
}
