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
using System.Windows.Shapes;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	/// <summary>
	/// Interaction logic for Window1.xaml
	/// </summary>
	public partial class CustomMessageBoxWindow : Window
	{
		public CustomMessageBoxWindow()
		{
			InitializeComponent();
			WindowStartupLocation = System.Windows.WindowStartupLocation.CenterScreen;
		}
		public MessageBoxResult messageBoxResult { get; set; }


		private void Image_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
		{
			messageBoxResult = MessageBoxResult.Yes;
			this.Close();
		}

		public void CustomMessage(string contentBox, string leftBtnLabel, string rightBtnLabel)
		{
			ContentBox.Text = contentBox;
			LeftBtnLabel.Text = leftBtnLabel;
			RightBtnLabel.Text = rightBtnLabel;
		}

		private void Image_MouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}


		private void Image_MouseLeftButtonUpKeepData(object sender, MouseButtonEventArgs e)
		{
			messageBoxResult = MessageBoxResult.Cancel;

			this.Close();
		}

		private void Window_MouseDown(object sender, MouseButtonEventArgs e)
		{
			if (!mHoverImage.IsMouseOver && !mImageleftBtn.IsMouseOver && !mImageRightBtn.IsMouseOver)
				this.DragMove();
		}

		public static MessageBoxResult Show(string contentBox, string leftBtnLabel, string rightBtnLabel)
		{
			CustomMessageBoxWindow msg = new CustomMessageBoxWindow();
			msg.CustomMessage(contentBox, leftBtnLabel, rightBtnLabel);
			msg.ShowDialog();

			return msg.messageBoxResult;
		}

	}
}
