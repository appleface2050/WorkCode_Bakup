/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Id Generator
 */

using System;
using System.ComponentModel;
using System.IO;
using System.Management;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Security.Cryptography;
using System.Text;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.IdManager
{
	class Id
	{
		private const uint HKEY_LOCAL_MACHINE = 0x80000002;
		private const uint KEY_READ = 0x20019;
		private const uint KEY_WOW64_64KEY = 0x00000100;

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern int RegOpenKeyEx(IntPtr hKey,
				String lpSubKey, uint ulOptions, uint samDesired,
				ref UIntPtr phkResult);

		[DllImport("advapi32.dll", SetLastError = true)]
		static extern int RegQueryValueEx(
				UIntPtr hKey,
				string lpValueName,
				int lpReserved,
				ref RegistryValueKind lpType,
				IntPtr lpData,
				ref int lpcbData);

		[DllImport("advapi32.dll", SetLastError = true)]
		private static extern int RegCloseKey(UIntPtr hKey);

		public static string GenerateID()
		{
			string id = "";
			bool allFs = true;
			try
			{
				byte[] commandArray = {
					Convert.ToByte('w'),
					Convert.ToByte('m'),
					Convert.ToByte('i'),
					Convert.ToByte('c')
				};
				string command = Encoding.UTF8.GetString(commandArray);

				byte[] argsArray = {
				Convert.ToByte('c'),
				Convert.ToByte('s'),
				Convert.ToByte('p'),
				Convert.ToByte('r'),
				Convert.ToByte('o'),
				Convert.ToByte('d'),
				Convert.ToByte('u'),
				Convert.ToByte('c'),
				Convert.ToByte('t'),
				Convert.ToByte(' '),
				Convert.ToByte('g'),
				Convert.ToByte('e'),
				Convert.ToByte('t'),
				Convert.ToByte(' '),
				Convert.ToByte('U'),
				Convert.ToByte('U'),
				Convert.ToByte('I'),
				Convert.ToByte('D')
			};

				string args = Encoding.UTF8.GetString(argsArray);

				byte[] replaceStringArray = {
				Convert.ToByte('U'),
				Convert.ToByte('U'),
				Convert.ToByte('I'),
				Convert.ToByte('D')
			};
				string replaceString = Encoding.UTF8.GetString(replaceStringArray);

				id = Utils.RunCmdNoLog(command, args, 3000);
				id = id.Replace(replaceString, "").Trim();
				id = id.Replace("\n", "");
				id = id.Replace("\r", "");
				id = id.Replace("\t", "");
				id = id.Replace(" ", "");

				foreach (char c in id)
				{
					if (c != 'F' && c != '-')
					{
						allFs = false;
						break;
					}
				}

			}
			catch
			{
				Logger.Error("Unable to query intended string");
			}

			if (id != String.Empty && allFs == true)
			{
				try
				{
					byte[] commandArray = {
					Convert.ToByte('w'),
					Convert.ToByte('m'),
					Convert.ToByte('i'),
					Convert.ToByte('c')
				};
					string command = Encoding.UTF8.GetString(commandArray);

					byte[] argsArray = {
					Convert.ToByte('b'),
					Convert.ToByte('i'),
					Convert.ToByte('o'),
					Convert.ToByte('s'),
					Convert.ToByte(' '),
					Convert.ToByte('g'),
					Convert.ToByte('e'),
					Convert.ToByte('t'),
					Convert.ToByte(' '),
					Convert.ToByte('s'),
					Convert.ToByte('e'),
					Convert.ToByte('r'),
					Convert.ToByte('i'),
					Convert.ToByte('a'),
					Convert.ToByte('l'),
					Convert.ToByte('n'),
					Convert.ToByte('u'),
					Convert.ToByte('m'),
					Convert.ToByte('b'),
					Convert.ToByte('e'),
					Convert.ToByte('r')
				};

					string args = Encoding.UTF8.GetString(argsArray);

					byte[] replaceStringArray = {
					Convert.ToByte('S'),
					Convert.ToByte('e'),
					Convert.ToByte('r'),
					Convert.ToByte('i'),
					Convert.ToByte('a'),
					Convert.ToByte('l'),
					Convert.ToByte('N'),
					Convert.ToByte('u'),
					Convert.ToByte('m'),
					Convert.ToByte('b'),
					Convert.ToByte('e'),
					Convert.ToByte('r')
				};
					string replaceString = Encoding.UTF8.GetString(replaceStringArray);

					id = Utils.RunCmdNoLog(command, args, 3000);
					id = id.Replace(replaceString, "").Trim();
					id = id.Replace("\n", "");
					id = id.Replace("\r", "");
					id = id.Replace("\t", "");
					id = id.Replace(" ", "");
				}
				catch
				{
					Logger.Error("Unable to query another intended string");
				}
			}
			if (id == String.Empty)
			{
				id = FallBackID();
			}
			return id;
		}

		private static string FallBackID()
		{
			IntPtr rootKey = (IntPtr)unchecked((int)HKEY_LOCAL_MACHINE);
			UIntPtr hKey = UIntPtr.Zero;
			byte[] regPathArray = {
			Convert.ToByte('S'),
			Convert.ToByte('o'),
			Convert.ToByte('f'),
			Convert.ToByte('t'),
			Convert.ToByte('w'),
			Convert.ToByte('a'),
			Convert.ToByte('r'),
			Convert.ToByte('e'),
			Convert.ToByte('\\'),
			Convert.ToByte('M'),
			Convert.ToByte('i'),
			Convert.ToByte('c'),
			Convert.ToByte('r'),
			Convert.ToByte('o'),
			Convert.ToByte('s'),
			Convert.ToByte('o'),
			Convert.ToByte('f'),
			Convert.ToByte('t'),
			Convert.ToByte('\\'),
			Convert.ToByte('C'),
			Convert.ToByte('r'),
			Convert.ToByte('y'),
			Convert.ToByte('p'),
			Convert.ToByte('t'),
			Convert.ToByte('o'),
			Convert.ToByte('g'),
			Convert.ToByte('r'),
			Convert.ToByte('a'),
			Convert.ToByte('p'),
			Convert.ToByte('h'),
			Convert.ToByte('y')
		};
			string regPath = Encoding.UTF8.GetString(regPathArray);
			int error = RegOpenKeyEx(rootKey, regPath, 0,
					KEY_READ | KEY_WOW64_64KEY, ref hKey);
			if (error != 0)
				throw new ApplicationException(
						@"Cannot open 64-bit HKLM\Software",
						new Win32Exception(error));

			int size = 0;
			RegistryValueKind type = RegistryValueKind.Unknown;

			byte[] keyNameArray = {
			Convert.ToByte('M'),
			Convert.ToByte('a'),
			Convert.ToByte('c'),
			Convert.ToByte('h'),
			Convert.ToByte('i'),
			Convert.ToByte('n'),
			Convert.ToByte('e'),
			Convert.ToByte('G'),
			Convert.ToByte('u'),
			Convert.ToByte('i'),
			Convert.ToByte('d')
		};
			string keyName = Encoding.UTF8.GetString(keyNameArray);

			error = RegQueryValueEx(hKey, keyName, 0, ref type, IntPtr.Zero, ref size);
			string machineGuid = "";

			IntPtr pResult = Marshal.AllocHGlobal(size);
			error = RegQueryValueEx(hKey, keyName, 0, ref type, pResult, ref size);

			if (error != 0)
				throw new ApplicationException(
						@"Cannot read 64-bit registry",
						new Win32Exception(error));

			machineGuid = Marshal.PtrToStringAnsi(pResult);
			if (pResult != IntPtr.Zero)
			{
				Marshal.FreeHGlobal(pResult);
			}
			RegCloseKey(hKey);

			return machineGuid;
		}
	}
}
