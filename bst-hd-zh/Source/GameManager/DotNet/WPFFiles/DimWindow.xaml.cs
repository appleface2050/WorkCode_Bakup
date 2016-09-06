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

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for DimWindow.xaml
	/// </summary>
	public partial class DimWindow : Window
	{
		Window childWindow = null;
		public DimWindow(Window window)
		{
			childWindow = window;
			window.Closed += Window_Closed;
			InitializeComponent();
			Owner = GameManagerWindow.Instance;
			Height = GameManagerWindow.Instance.ActualHeight;
			Width = GameManagerWindow.Instance.ActualWidth;
			Left = GameManagerWindow.Instance.Left;
			Top = GameManagerWindow.Instance.Top;
			ShowDialog();
		}

		private void Window_Closed(object sender, EventArgs e)
		{
			this.Close();
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			if (childWindow != null)
			{
				childWindow.Owner = (Window)sender;
				childWindow.ShowDialog();
			}
		}
	}
}
