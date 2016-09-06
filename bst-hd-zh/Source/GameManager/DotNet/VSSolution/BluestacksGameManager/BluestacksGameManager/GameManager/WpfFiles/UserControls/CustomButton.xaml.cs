using BlueStacks.hyperDroid.Common;
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
	/// Interaction logic for CustomButton.xaml
	/// </summary>
	public partial class CustomButton : Button
	{
		static Dictionary<string, CustomButton> dictSelecetedButtons = new Dictionary<string, CustomButton>();

		private string group = string.Empty;
		public string Group
		{
			get
			{
				return group;
			}

			set
			{
				group = value;
			}
		}

		public Image img = new Image();

		private string imageName = string.Empty;
		public string ImageName
		{
			get
			{
				return imageName;
			}

			set
			{
				imageName = value;
				//UpdateLayout(value);
			}
		}

		//private void UpdateLayout(string value)
		//{
		//	((CustomPictureBox)this.Template.FindName("mImage", this)).ImageName = value;
		//	((Viewbox)this.Template.FindName("ViewBox", this)).HorizontalAlignment = HorizontalAlignment.Left;
		//	Grid.SetColumn(((Viewbox)this.Template.FindName("ViewBox", this)), 3);
		//}

		LinearGradientBrush brush = null;

		private bool isButtonHorizontal = true;
		public bool IsButtonHorizontal
		{
			get
			{
				return isButtonHorizontal;
			}

			set
			{
				isButtonHorizontal = value;
				SetBrush();
			}
		}

		private bool isSelected;
		public bool IsSelected
		{
			get
			{
				return isSelected;
			}
			set
			{
				isSelected = value;
				SetBackground();
				if (IsSelected)
				{
					if (!string.IsNullOrEmpty(Group))
					{
						if (dictSelecetedButtons.ContainsKey(Group))
						{
							dictSelecetedButtons[Group].IsSelected = false;
						}
						dictSelecetedButtons[Group] = this;
					}
				}
			}
		}

		private bool isHoverDisabled;
		public bool IsHoverDisabled
		{
			get
			{
				return isHoverDisabled;
			}

			set
			{
				isHoverDisabled = value;
			}
		}


		public CustomButton()
		{
			InitializeComponent();
			SetBrush();
			SetBackground();
		}

		private void SetBrush()
		{
			if (isButtonHorizontal)
			{
				brush = (LinearGradientBrush)this.FindResource("ButtonNormalBackground");
			}
			else
			{
				brush = (LinearGradientBrush)this.FindResource("ButtonNormalBackgroundVertical");
			}
		}

		private void SetBackground()
		{
			if (IsSelected)
			{
				Background = brush;
				CustomPictureBox.SetBitmapImage(img, ImageName + "_click");
			}
			else
			{
				if (isHoverDisabled)
				{
					Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#222527"));
				}
				else
				{
					Button_MouseEvent(null, null);
				}
				CustomPictureBox.SetBitmapImage(img, ImageName);
			}
		}

		private void Button_MouseEvent(object sender, MouseEventArgs e)
		{
			if (!IsSelected && !isHoverDisabled)
			{
				if (IsMouseOver)
				{
					Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#1B1C1E"));
				}
				else
				{
					Background = (SolidColorBrush)(new BrushConverter().ConvertFrom("#383A3D"));
				}
			}
		}


		private void Button_Click(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(group))
			{
				if (!IsSelected)
				{
					IsSelected = true;
				}
			}
		}

		private void Button_PreviewMouseDown(object sender, MouseButtonEventArgs e)
		{
			if (string.IsNullOrEmpty(group))
			{
				IsSelected = true;
			}
		}

		private void Button_PreviewMouseUp(object sender, MouseButtonEventArgs e)
		{
			if (string.IsNullOrEmpty(group))
			{
				IsSelected = false;
				Button_MouseEvent(null, null);
			}
		}

		private void Button_Loaded(object sender, RoutedEventArgs e)
		{
			if (!string.IsNullOrEmpty(imageName))
			{
				((Grid)this.Template.FindName("Grid", this)).Children.Add(img);
				Grid.SetColumn(img, 1);
				SetBackground();
			}
		}
	}
}
