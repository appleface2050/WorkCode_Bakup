using System;
using System.Text;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Common
{
	public class IniFile
	{
		private const int VALUE_LEN_MAX = 255;

		public IniFile(string path)
		{
			m_Path = path;
		}

		public string GetValue(string section, string key)
		{
			StringBuilder ret = new StringBuilder(VALUE_LEN_MAX);
			int rc = GetPrivateProfileString(section, key, "", ret,
					VALUE_LEN_MAX, m_Path);
			return ret.ToString();
		}

		public void SetValue(string section, string key, string value)
		{
			WritePrivateProfileString(section, key, value, m_Path);
		}

		[DllImport("kernel32", SetLastError = true)]
		private static extern int GetPrivateProfileString(string section,
				string key,
				string defaultValue,
				StringBuilder result,
				int size,
				string fileName);

		[DllImport("kernel32", SetLastError = true)]
		private static extern long WritePrivateProfileString(string section,
				string key,
				string val,
				string path);

		private string m_Path;
	}
}
