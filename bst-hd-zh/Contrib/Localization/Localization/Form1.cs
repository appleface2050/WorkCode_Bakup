using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;

namespace Localization
{
	public partial class Form1 : Form
	{

		string localeDirectoryPath = string.Empty;
		string outputDirectoryPath = string.Empty;
		bool isWindows = true;
		bool isExtract = true;
		string subDirectoryPath = string.Empty;
		string xmlFileName = string.Empty;
		string localeFolderName = string.Empty;

		string englishFileName = string.Empty;
		public Form1()
		{
			InitializeComponent();
		}


		private void textBox1_MouseDoubleClick(object sender, MouseEventArgs e)
		{
			try
			{
				localeDirectoryPath = GetFolderPath();
				tbLocaleDirectoryPath.Text = localeDirectoryPath;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void radioButton1_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				radioButton2.Checked = !radioButton1.Checked;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void radioButton2_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				radioButton1.Checked = !radioButton2.Checked;
				label3.Visible = radioButton2.Checked;
				textBox3.Visible = radioButton2.Checked;
				label4.Visible = radioButton2.Checked;
				textBox4.Visible = radioButton2.Checked;
				label5.Visible = radioButton2.Checked;
				textBox5.Visible = radioButton2.Checked;
				if (radioButton2.Checked)
				{
					textBox2.Text = "\\values\\strings.xml";
				}
				else
				{
					textBox2.Text = "i18n.en - US.txt";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void radioButton3_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				radioButton3.Checked = !radioButton4.Checked;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void radioButton4_CheckedChanged(object sender, EventArgs e)
		{
			try
			{
				radioButton4.Checked = !radioButton3.Checked;
				if (radioButton4.Checked)
				{
					button1.Text = "Choose Translated strings directory";
				}
				else
				{
					button1.Text = "Choose Output directory";
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private void button1_Click(object sender, EventArgs e)
		{
			try
			{
				outputDirectoryPath = GetFolderPath();
                localeDirectoryPath = tbLocaleDirectoryPath.Text;
				isWindows = radioButton1.Checked;
				isExtract = radioButton3.Checked;
				englishFileName = textBox2.Text;
				subDirectoryPath = textBox3.Text;
				xmlFileName = textBox4.Text;
				localeFolderName = textBox5.Text;
				backgroundWorker1.RunWorkerAsync();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}

		private string GetFolderPath()
		{
			try
			{
				FolderBrowserDialog dlg = new FolderBrowserDialog();
				DialogResult res = dlg.ShowDialog();
				if (res == DialogResult.OK)
				{
					if (Directory.Exists(dlg.SelectedPath))
					{
						return dlg.SelectedPath;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
			return string.Empty;
		}

		private void backgroundWorker1_DoWork(object sender, DoWorkEventArgs e)
		{
			try
			{
				if (string.IsNullOrEmpty(localeDirectoryPath) || string.IsNullOrEmpty(outputDirectoryPath))
				{
					e.Result = "Path Missing";
					return;
				}
				if (!Directory.Exists(localeDirectoryPath) || !Directory.Exists(outputDirectoryPath))
				{
					e.Result = "Directory not found";
					return;
				}

				#region windows
				if (isWindows)
				{
					Dictionary<string, Dictionary<string, string>> dictLocale = new Dictionary<string, Dictionary<string, string>>();
					Dictionary<string, Dictionary<string, string>> dictOutput = new Dictionary<string, Dictionary<string, string>>();

					List<string> lst = Directory.GetFiles(localeDirectoryPath).ToList();
					foreach (string file in lst)
					{
						dictLocale.Add(Path.GetFileName(file), Utils.GetFileDictionary(file));
					}
					#region extract
					if (isExtract)
					{
						if (dictLocale.ContainsKey(englishFileName))
						{
							Dictionary<string, string> dictEnglish = dictLocale[englishFileName];
							dictLocale.Remove(englishFileName);
							foreach (KeyValuePair<string, Dictionary<string, string>> kvp in dictLocale)
							{
								dictOutput.Add(kvp.Key, dictEnglish.Where(x => !kvp.Value.Keys.Contains(x.Key, StringComparer.InvariantCultureIgnoreCase)).ToDictionary(x => x.Key, x => x.Value));
							}
							Utils.WriteFiles(dictOutput, outputDirectoryPath);
						}
						else
						{
							e.Result = "English Locale file not found.";
							return;
						}
					}
					#endregion

					#region merge
					else
					{
						List<string> llst = Directory.GetFiles(outputDirectoryPath).ToList();
						foreach (string file in llst)
						{
							dictOutput.Add(Path.GetFileName(file), Utils.GetFileDictionary(file));
						}
						foreach (KeyValuePair<string, Dictionary<string, string>> kvp in dictOutput)
						{
							if (dictLocale.ContainsKey(kvp.Key))
							{
								dictLocale[kvp.Key] = dictLocale[kvp.Key].Union(kvp.Value).ToDictionary(x => x.Key, x => x.Value);
							}
							else
							{
								dictLocale.Add(kvp.Key, kvp.Value);
							}
						}
						dictLocale.Remove(englishFileName);
						Utils.WriteFiles(dictLocale, localeDirectoryPath);
					}
					#endregion
				}
				#endregion

				#region android
				else
				{
					List<string> lst = Directory.GetDirectories(Path.Combine(localeDirectoryPath, subDirectoryPath)).ToList()
						.Where(x => x.Contains(localeFolderName) && File.Exists(Path.Combine(x, xmlFileName))).ToList();

					foreach (var directory in lst)
					{
						string file = Path.Combine(directory, xmlFileName);
						if (file.EndsWith(englishFileName))
						{
							englishFileName = directory;
							break;
						}
					}
					lst.Remove(englishFileName);

					#region extract
					if (isExtract)
					{
						if (!File.Exists(Path.Combine(englishFileName, xmlFileName)))
						{
							e.Result = "English Locale file not found.";
							return;
						}
						foreach (var directory in lst)
						{
							XmlDocument doc = new XmlDocument();
							doc.Load(Path.Combine(englishFileName, xmlFileName));
							Dictionary<string, XmlNode> xmlnode = Utils.GetXMLFileDictionary(doc);

							XmlDocument temp = new XmlDocument();
							temp.Load(Path.Combine(directory, xmlFileName));
							Dictionary<string, XmlNode> tempXmlnodes = Utils.GetXMLFileDictionary(temp);

							foreach (KeyValuePair<string, XmlNode> kvp in xmlnode)
							{
								if (tempXmlnodes.ContainsKey(kvp.Key))
								{
									kvp.Value.ParentNode.RemoveChild(kvp.Value);
								}
							}
							string newFilePath = Path.Combine(outputDirectoryPath + directory.Replace(Directory.GetParent(localeDirectoryPath).FullName, string.Empty), xmlFileName);
							if (!Directory.Exists(Directory.GetParent(newFilePath).FullName))
							{
								Directory.CreateDirectory(Directory.GetParent(newFilePath).FullName);
							}
							doc.Save(newFilePath);
						}
					}
					#endregion

					#region merge
					else
					{
						List<string> lstTranslated = Directory.GetDirectories(Path.Combine(outputDirectoryPath, subDirectoryPath)).ToList()
						.Where(x => x.Contains(localeFolderName) && File.Exists(Path.Combine(x, xmlFileName))).ToList();

						foreach (var directory in lstTranslated)
						{
							 
							XmlDocument docLocale = new XmlDocument();
							string localeFileName =localeDirectoryPath+ Path.Combine(directory, xmlFileName).Replace(outputDirectoryPath, string.Empty);
							docLocale.Load(localeFileName);
							Dictionary<string, XmlNode> xmlnode = Utils.GetXMLFileDictionary(docLocale);

							XmlDocument translatedDoc = new XmlDocument();
							translatedDoc.Load(Path.Combine(directory, xmlFileName));
							Dictionary<string, XmlNode> tempXmlnodes = Utils.GetXMLFileDictionary(translatedDoc);

							foreach (XmlNode  childEl in translatedDoc.DocumentElement.ChildNodes)
							{
								var newNode = docLocale.ImportNode(childEl, true);
								docLocale.DocumentElement.AppendChild(newNode);
							}

							if (!Directory.Exists(Directory.GetParent(localeFileName).FullName))
							{
								Directory.CreateDirectory(Directory.GetParent(localeFileName).FullName);
							}
							docLocale.Save(localeFileName);
						}
					}
					#endregion
				}
				#endregion
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}





		private void backgroundWorker1_RunWorkerCompleted(object sender, RunWorkerCompletedEventArgs e)
		{
			try
			{
				if (e.Result != null && !string.IsNullOrEmpty(e.Result.ToString()))
				{
					MessageBox.Show(this, "Something not correct : " + e.Result.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
				}
				else
				{
					MessageBox.Show(this, "Done", "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Information);
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString(), "Bluestacks Localization Tool", MessageBoxButtons.OK, MessageBoxIcon.Error);
			}
		}
	}
}
