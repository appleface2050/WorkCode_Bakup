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
	/// Interaction logic for CustomSlider.xaml
	/// </summary>
	public partial class CustomSlider : UserControl
	{

		private string label = string.Empty;
		public string Label
		{
			get
			{
				return label;
			}

			set
			{
				label = value;
				mlbl.Content = value;
			}
		}


		private string postFix = string.Empty;
		public string PostFix
		{
			get
			{
				return postFix;
			}

			set
			{
				postFix = value;
				lblValue.ContentStringFormat = "{0}"+value;
			}
		}

		private int maximum = 10;
		public int Maximum
		{
			get
			{
				return maximum;
			}

			set
			{
				maximum = value;
				mSlider.Maximum = value;
			}
		}
		private int minimum = 0;
		public int Minimum
		{
			get
			{
				return minimum;
			}

			set
			{
				minimum = value;
				mSlider.Minimum = value;
			}
		}

		public double Value
		{
			get
			{
				return mSlider.Value;
			}

			set
			{
				mSlider.Value = value;
			}
		}




		public CustomSlider()
		{
			InitializeComponent();
			
		}

		private void UserControl_IsEnabledChanged(object sender, DependencyPropertyChangedEventArgs e)
		{
			((LinearGradientBrush)this.FindResource("mBrushRepeatButton1")).GradientStops[1].Color = (Color)ColorConverter.ConvertFromString("#717171"); 
			((LinearGradientBrush)this.FindResource("mBrushRepeatButton1")).GradientStops[2].Color = (Color)ColorConverter.ConvertFromString("#4F4F4F"); 
		}

	}
}
