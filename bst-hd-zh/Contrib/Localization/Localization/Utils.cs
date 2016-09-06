using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Windows.Forms;
using System.Xml;

namespace Localization
{
	public class Utils
	{

		internal static Dictionary<string, string> GetFileDictionary(string file)
		{
			Dictionary<string, string> dict = new Dictionary<string, string>();
			try
			{
				List<string> lines = File.ReadAllLines(file).ToList();
				foreach (string line in lines)
				{
					if (line.Contains('='))
					{
						dict.Add(line.Split('=')[0].Trim(), line.Split('=')[1].Trim());
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return dict;
		}

		internal static void WriteFiles(Dictionary<string, Dictionary<string, string>> dictOutput, string folderName)
		{
			try
			{
				foreach (KeyValuePair<string, Dictionary<string, string>> kvp in dictOutput)
				{
					if (kvp.Value.Count > 0)
					{
						StringBuilder s = new StringBuilder();
						foreach (KeyValuePair<string, string> kvplocale in kvp.Value)
						{
							s.Append(kvplocale.Key).Append(" = ").Append(kvplocale.Value).Append(Environment.NewLine).Append(Environment.NewLine).Append(Environment.NewLine);
						}
						File.WriteAllText(Path.Combine(folderName, kvp.Key), s.ToString());
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		internal static Dictionary<string, XmlNode> GetXMLFileDictionary(XmlDocument xmldoc)
		{
			Dictionary<string, XmlNode> dict = null;
			try
			{
				XmlNodeList xmlnode = xmldoc.GetElementsByTagName("string");
				dict = new Dictionary<string, XmlNode>();
				for (int i = 0; i <= xmlnode.Count - 1; i++)
				{
					if (!dict.ContainsKey(xmlnode[i].Attributes[0].InnerText))
					{
						dict.Add(xmlnode[i].Attributes[0].InnerText, xmlnode[i]);
					}
				}
				xmlnode = xmldoc.GetElementsByTagName("string-array");
				for (int i = 0; i <= xmlnode.Count - 1; i++)
				{
					if (!dict.ContainsKey(xmlnode[i].Attributes[0].InnerText))
					{
						dict.Add(xmlnode[i].Attributes[0].InnerText, xmlnode[i]);
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return dict;
		}

	}
}
