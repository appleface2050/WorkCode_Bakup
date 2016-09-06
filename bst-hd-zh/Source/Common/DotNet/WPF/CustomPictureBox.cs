using BlueStacks.hyperDroid.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace BlueStacks.hyperDroid.Common
{
	public class CustomPictureBox : Image
	{
		static Dictionary<string, BitmapImage> dictAssets = new Dictionary<string, BitmapImage>();
		private string imagePath = string.Empty;

		public string ImageName
		{
			get
			{
				return imagePath;
			}

			set
			{
				imagePath = value;
				if (!System.ComponentModel.DesignerProperties.GetIsInDesignMode(this))
				{
					SetDefaultImage();
				}
			}
		}

		public CustomPictureBox()
		{
			this.MouseEnter += PictureBox_MouseEnter;
			this.MouseLeave += PictureBox_MouseLeave;
			this.MouseDown += PictureBox_MouseDown;
			this.MouseUp += PictureBox_MouseUp;

		}


		private void PictureBox_MouseEnter(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SetHoverImage();
			//e.Handled = true;
		}

		private void PictureBox_MouseLeave(object sender, System.Windows.Input.MouseEventArgs e)
		{
			SetDefaultImage();
			// e.Handled = true;
		}
		private void PictureBox_MouseDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			SetClickedImage();
			// e.Handled = true;
		}
		private void PictureBox_MouseUp(object sender, System.Windows.Input.MouseButtonEventArgs e)
		{
			if (this.IsMouseOver)
			{
				SetHoverImage();
			}
			else
			{
				SetDefaultImage();
			}
			//e.Handled = true;
		}

		public void SetHoverImage()
		{
			//SetBitmapImage(this, imagePath + "_hover.png");
			try
			{
				Source = new BitmapImage(new Uri("pack://application:,,,/Assets/" + imagePath + "_hover.png"));
			}
			catch (Exception)
			{
				//If Image not found then don't set it.
			}
		}
		public void SetClickedImage()
		{
			try
			{
				Source = new BitmapImage(new Uri("pack://application:,,,/Assets/" + imagePath + "_click.png"));
			}
			catch (Exception)
			{
				//If Image not found then don't set it.
			}
		}
		public void SetDefaultImage()
		{
			try
			{
				Source = new BitmapImage(new Uri("pack://application:,,,/Assets/" + imagePath + ".png"));
			}
			catch (Exception)
			{
				//If Image not found then don't set it.
			}
		}
		internal static BitmapImage GetBitmapImage(string fileName)
		{
			BitmapImage img = null;
			if (dictAssets.ContainsKey(fileName))
			{
				img = dictAssets[fileName];
			}
			else
			{
				try
				{
					img = new BitmapImage(new Uri("pack://application:,,,/Assets/" + Path.GetFileNameWithoutExtension(fileName) + ".png"));
				}
				catch
				{
					Logger.Info("Error at loding image file " + fileName + "Retrying with absolute path");
					try
					{
						img = new BitmapImage(new Uri(fileName));
					}
					catch
					{
						Logger.Error("Error when loading from absolute path too.");
					}
				}
				if (img != null)
				{
					dictAssets.Add(fileName, img);
				}
			}
			return img;
		}

		internal static void SetBitmapImage(Image imgage, string fileName)
		{
			BitmapImage img = GetBitmapImage(fileName);
			if (img != null)
			{
				imgage.Source = img;
			}
		}
	}
}
