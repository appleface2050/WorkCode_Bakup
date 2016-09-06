using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for CustomComboBox.xaml
	/// </summary>
	public partial class CustomComboBox : ComboBox
	{
		public CustomComboBox()
		{
			InitializeComponent();
		}

		private void Chrome_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!this.IsDropDownOpen)
			{
				this.IsDropDownOpen = true;
			}
		}

		private void Image_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}
	}
}
