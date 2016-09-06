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
	/// Interaction logic for CustomCheckbox.xaml
	/// </summary>
	public partial class CustomCheckbox : CheckBox
	{

		Image mImage = null;
		public Image MImage
		{
			get
			{
				if (mImage == null)
				{
					mImage = (Image)this.Template.FindName("mImage", this);
				}
				return mImage;
			}
		}

		public CustomCheckbox()
		{
			InitializeComponent();
		}

		private void CheckBox_MouseEnter(object sender, MouseEventArgs e)
		{
			if (!IsChecked.HasValue || !IsChecked.Value)
			{
				Common.CustomPictureBox.SetBitmapImage(MImage, "Checkbox_hover");
			}
		}

		private void CheckBox_MouseLeave(object sender, MouseEventArgs e)
		{
			if (IsChecked.HasValue && IsChecked.Value)
			{
				Common.CustomPictureBox.SetBitmapImage(MImage, "Checkbox_checked");
			}
			else
			{
				Common.CustomPictureBox.SetBitmapImage(MImage, "Checkbox");
			}
		}

		private void CheckBox_Checked(object sender, RoutedEventArgs e)
		{
			Common.CustomPictureBox.SetBitmapImage(MImage, "Checkbox_checked");
		}

		private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
		{
			if (IsMouseOver)
			{
				Common.CustomPictureBox.SetBitmapImage(MImage, "Checkbox_hover");
			}
			else
			{
				Common.CustomPictureBox.SetBitmapImage(MImage, "Checkbox");
			}
		}
	}
}
