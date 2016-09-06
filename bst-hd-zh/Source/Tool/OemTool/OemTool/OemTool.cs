using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using System.Xml.Serialization;

namespace OemTool
{
	public partial class OemTool : Form
	{
		Oem mOem;
		public OemTool()
		{
			try
			{
				InitializeComponent();
				ChangeDescriptionHeight(propertyGrid1, propertyGrid1.Height / 5);
				BlueStacks.hyperDroid.Locale.Strings.sLocalizedString = new Dictionary<string, string>();
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void btnSave_Click(object sender, EventArgs e)
		{
			try
			{
				DialogResult result = saveFileDialog1.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					using (XmlTextWriter writer = new XmlTextWriter(saveFileDialog1.FileName, Encoding.UTF8))
					{
						writer.Formatting = Formatting.Indented;
						XmlSerializer serialize = new XmlSerializer(typeof(Oem));
						serialize.Serialize(writer, mOem);
						writer.Flush();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		private static void ChangeDescriptionHeight(PropertyGrid grid, int height)
		{
			try
			{
				if (grid == null) throw new ArgumentNullException("grid");

				foreach (Control control in grid.Controls)
				{
					if (control.GetType().Name == "DocComment")
					{
						control.Enabled = true;
						FieldInfo fieldInfo = control.GetType().BaseType.GetField("userSized",
						  BindingFlags.Instance |
						  BindingFlags.NonPublic);
						fieldInfo.SetValue(control, true);
						control.Height = height;
						control.Enabled = true;
						return;
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
		private void btnLoad_Click(object sender, EventArgs e)
		{
			try
			{
				DialogResult result = openFileDialog1.ShowDialog();
				if (result == System.Windows.Forms.DialogResult.OK)
				{
					using (FileStream fs = File.OpenRead(openFileDialog1.FileName))
					{
						XmlSerializer serializer = new XmlSerializer(typeof(Oem));
						mOem = (Oem)serializer.Deserialize(fs);
						fs.Flush();
					}
					propertyGrid1.SelectedObject = mOem;
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}


		private void resetToolStripMenuItem_Click(object sender, EventArgs e)
		{
			try
			{
				if (propertyGrid1.SelectedGridItem != null)
				{
					PropertyDescriptor pd = propertyGrid1.SelectedGridItem.PropertyDescriptor;
					if (pd != null)
					{
						pd.ResetValue(propertyGrid1.SelectedObject);
						propertyGrid1.Refresh();
					}
				}
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}

		private void btnDefault_Click(object sender, EventArgs e)
		{
			try
			{
				mOem = new Oem();
				propertyGrid1.SelectedObject = mOem;
			}
			catch (Exception ex)
			{
				MessageBox.Show(ex.ToString());
			}
		}
	}
}
