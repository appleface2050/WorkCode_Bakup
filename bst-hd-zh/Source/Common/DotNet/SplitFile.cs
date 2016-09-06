using System;
using System.IO;
using System.Text;
using System.Collections.Generic;
using System.Security.Cryptography;

using BlueStacks.hyperDroid.Common;

class SplitFile
{
	public delegate void ProgressCb(String manifest);
	public static void Split(String path, int size, ProgressCb progressCb)
	{
		const int BLOCK_SIZE = 16 * 1024;
		byte[] buffer = new byte[BLOCK_SIZE];

		using (Stream inFs = File.OpenRead(path))
		{
			int partId = 0;
			String manifestFilePath = String.Format(@"{0}.manifest", path);

			while (inFs.Position < inFs.Length)
			{
				String partFilePath = String.Format(@"{0}_part_{1}", path, partId);

				using (Stream outFs = File.Create(partFilePath))
				{
					int remaining = size;
					int bytesRead = 0;

					while (remaining > 0)
					{
						bytesRead = inFs.Read(buffer, 0, Math.Min(remaining, BLOCK_SIZE));

						if (bytesRead == 0)
							break;

						outFs.Write(buffer, 0, bytesRead);
						remaining -= bytesRead;
					}
				}

				String partFileManifest = null;

				using (Stream outFs = File.OpenRead(partFilePath))
				{
					String partFileSha1 = CheckSum(outFs);
					long partFileSize = outFs.Length;
					partFileManifest = String.Format("{0} {1} {2}", Path.GetFileName(partFilePath),
							partFileSize, partFileSha1);
				}

				progressCb(partFileManifest);

				partId++;
			}
		}

	}

	public static string CheckSum(Stream stream)
	{
		/*
		 * Replaced SHA1Managed with SHA1CryptoServiceProvider. SHA1Managed is not FIPS compliant and
		 * its usage throws an InvalidOperationException on machines with the FIPS compliant policy enforced.
		 * Move to a FIPS compliant algorithm.
		 */
		SHA1CryptoServiceProvider sha1 = new SHA1CryptoServiceProvider();
		byte[] hash = sha1.ComputeHash(stream);
		StringBuilder hex = new StringBuilder(hash.Length * 2);

		foreach (byte b in hash)
			hex.AppendFormat("{0:x2}", b);

		return hex.ToString();
	}
}
