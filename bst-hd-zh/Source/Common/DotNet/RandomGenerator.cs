using System;
using System.Security.Cryptography;

namespace BlueStacks.hyperDroid.Common
{
	public static class RandomGenerator
	{
		private static RNGCryptoServiceProvider s_RandomProvider = new RNGCryptoServiceProvider();

		[ThreadStatic]
		private static Random s_RandomPerThread;

		public static int Next(int maxValue)
		{
			if (s_RandomPerThread == null)
			{
				byte[] buff = new byte[4];
				s_RandomProvider.GetBytes(buff);
				s_RandomPerThread = new Random(BitConverter.ToInt32(buff, 0));
			}
			return s_RandomPerThread.Next(maxValue);
		}
	}
}
