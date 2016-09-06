/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Console Frontend
 */

using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using System.Net;
using System.Threading;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{
	public class FrontendBanner
	{
		const string url = "http://cdn.bluestacks.com/public/appsettings/app-back-images/{0}.png";
		public static void HandleBackgroundBannerImage(string packageName, UserControl form)
		{
			form.Tag = null;
			string backImagePath = Path.Combine(Common.Strings.GameManagerBannerImageDir, packageName + ".png");
			UIHelper.RunOnUIThread(form, delegate ()
			{
				form.BackgroundImage = null;
			});

			if (File.Exists(backImagePath))
			{
				Image backImage = new Bitmap(backImagePath);
				SetImageAsBackground(backImage, form);
				CheckForNewBannerFile(packageName, form);
			}
			else
			{
				// Set the default file for the time being downloaded
				backImagePath = Path.Combine(Common.Strings.GameManagerBannerImageDir, "Bluestacks_Default" + ".png");
				if (File.Exists(backImagePath))
				{
					Image backImage = new Bitmap(backImagePath);
					SetImageAsBackground(backImage, form);
					//check for new default file if there
					CheckForNewBannerFile("Bluestacks_Default", form);
				}
				// download app file
				CheckForNewBannerFile(packageName, form);
			}
		}

		private static void CheckForNewBannerFile(string packageName, UserControl form)
		{
			try
			{
				string filePath = Path.Combine(Common.Strings.GameManagerBannerImageDir, packageName + ".png");
				FileInfo fileInfo = null;
				if (File.Exists(filePath))
				{
					fileInfo = new FileInfo(filePath);
				}
				if (fileInfo == null || (DateTime.Now - fileInfo.CreationTime).TotalDays > 7)
				{
					string serverFilePath = String.Format(url, packageName);
					WebRequest req = HttpWebRequest.Create(serverFilePath);
					req.Method = "HEAD";
					using (System.Net.WebResponse resp = req.GetResponse())
					{
						int ContentLength;
						if (int.TryParse(resp.Headers.Get("Content-Length"), out ContentLength))
						{
							if (fileInfo == null || fileInfo.Length != ContentLength)
							{
								if (DownloadFile(packageName))
								{
									Image backnewImage = new Bitmap(Path.Combine(Common.Strings.GameManagerBannerImageDir, packageName + ".png"));
									SetImageAsBackground(backnewImage, form);
								}
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("Error downloading file from server" + ex.ToString());
				//Check for default file
				if (!packageName.Equals("Bluestacks_Default"))
				{
					CheckForNewBannerFile("Bluestacks_Default", form);
				}
			}
		}

		public static bool DownloadFile(string packageName)
		{
			string serverFilePath = String.Format(url, packageName);
			string dir = Common.Strings.GameManagerBannerImageDir;
			if (!Directory.Exists(dir))
			{
				Directory.CreateDirectory(dir);
			}

			string backImageName = packageName + ".png";
			string backImagePath = Path.Combine(dir, backImageName);

			string backImageTempName = backImageName + ".tmp";
			string backImageTempPath = Path.Combine(dir, backImageTempName);

			try
			{
				Logger.Info("Will download {0} to {1}", serverFilePath, backImageTempPath);
				if (File.Exists(backImageTempPath))
				{
					File.Delete(backImageTempPath);
				}
				WebClient webClient = new WebClient();
				webClient.DownloadFile(serverFilePath, backImageTempPath);

				if (File.Exists(backImagePath))
				{
					File.Delete(backImagePath);
				}
				File.Move(backImageTempPath, backImagePath);
				return true;
			}
			catch (Exception e)
			{
				Logger.Error("Error when downloading from " + serverFilePath);
				Logger.Error(e.ToString());
				return false;
			}
		}

		public static void SetImageAsBackground(Image srcImage, UserControl form)
		{
			form.Tag = srcImage.Clone();
			if (form.ClientSize.Height > 0)
			{
				//UIHelper.RunOnUIThread(form, delegate ()
				//{
				form.BackgroundImage = null;
				////});
				Image backImage = ResizeImageToWindow(srcImage, form);
				srcImage.Dispose();

				////UIHelper.RunOnUIThread(form, delegate ()
				//{
				form.BackgroundImage = backImage;
				form.Tag = backImage.Clone();
				form.BackgroundImageLayout = ImageLayout.None;
				//});
			}
		}

		/// <summary>
		/// 	
		/// Given image is resized so that it follows the rules:
		/// Make sure the image covers the entire window while preserving aspect ratio.
		/// Pin the upper lefthand corner of the image to the upper lefthand corner of the App Player window.
		/// When the image aspect ratio is greater than the window aspect ratio, the left side of the image is
		/// aligned with the left side of the window.The right side of the image is cropped off.
		/// When the window aspect ratio is greater than the image aspect ratio, the top of the image is
		/// aligned with the top of the window and the bottom of the image is cropped off.
		///
		/// The caller needs to dispose the original image
		/// </summary>
		/// <param name="src"></param>
		/// <returns></returns>
		private static Image ResizeImageToWindow(Image src, UserControl form)
		{
			try
			{
				int srcWidth = src.Width;
				int srcHeight = src.Height;

				int dstWidth = form.ClientSize.Width;
				int dstHeight = form.ClientSize.Height;

				float windowAspectRatio = (float)form.ClientSize.Width / form.ClientSize.Height;
				float imageAspectRatio = (float)srcWidth / srcHeight;

				if (windowAspectRatio == imageAspectRatio)
				{
					dstWidth = form.ClientSize.Width;
					dstHeight = form.ClientSize.Height;
				}
				else if (windowAspectRatio > imageAspectRatio)
				{
					dstWidth = form.ClientSize.Width;
					dstHeight = (int)((float)dstWidth / imageAspectRatio);
				}
				else if (windowAspectRatio < imageAspectRatio)
				{
					dstHeight = form.ClientSize.Height;
					dstWidth = (int)((float)dstHeight * imageAspectRatio);
				}

				Logger.Info("BackgroundImage size: ({0}, {1})", dstWidth, dstHeight);
				Image dst = new Bitmap(dstWidth, dstHeight);

				using (Graphics g = Graphics.FromImage(dst))
				{
					g.SmoothingMode = SmoothingMode.AntiAlias;
					g.InterpolationMode = InterpolationMode.HighQualityBicubic;
					g.DrawImage(src, 0, 0, dst.Width, dst.Height);
				}
				return dst;
			}
			catch (Exception ex)
			{
				return new Bitmap(src);
			}
		}
	}
}
