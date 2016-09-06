using System;
using System.Drawing;
using Microsoft.Win32;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.IO;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;
using System.Linq;
using System.Windows.Media.Imaging;

namespace BlueStacks.hyperDroid.GameManager
{
	public static class Assets
	{
		static string assetsDir = GameManagerUtilities.AssetsDir;
		public static Dictionary<string, BitmapImage> mAllImagesDict = null;
		public static void Init()
		{
			if (mAllImagesDict == null)
			{
				mAllImagesDict = new Dictionary<string, BitmapImage>();

				mAllImagesDict.Add("tool_leftarrow",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_leftarrow.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_leftarrow_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_leftarrow_hover.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_leftarrow_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_leftarrow_click.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_leftarrow_disable",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_leftarrow_disable.png"), UriKind.Relative)));

				mAllImagesDict.Add("tool_close",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_close.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_close_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_close_hover.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_close_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_close_click.png"), UriKind.Relative)));
				if (Features.IsFeatureEnabled(Features.IS_CHINA_UI))
				{
					mAllImagesDict.Add("tool_showtabpages",
							new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_showtabpages.png"), UriKind.Relative)));
					mAllImagesDict.Add("tool_showtabpages_hover",
							new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_showtabpages_hover.png"), UriKind.Relative)));
					mAllImagesDict.Add("tool_showtabpages_click",
							new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_showtabpages_click.png"), UriKind.Relative)));
				}

				mAllImagesDict.Add("tool_minimize",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_minimize.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_minimize_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_minimize_hover.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_minimize_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_minimize_click.png"), UriKind.Relative)));

				mAllImagesDict.Add("tool_shrink",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_shrink.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_shrink_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_shrink_hover.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_shrink_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_shrink_click.png"), UriKind.Relative)));

				mAllImagesDict.Add("tool_fullscreen",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_fullscreen.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_fullscreen_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_fullscreen_click.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_fullscreen_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_fullscreen_hover.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_fullscreen_disable",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_fullscreen_disable.png"), UriKind.Relative)));

				mAllImagesDict.Add("tool_key",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_key.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_key_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_key_click.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_key_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_key_hover.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_key_disable",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_key_disable.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_key_glow",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_key_glow.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_key_glow_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_key_glow_hover.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_key_glow_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_key_glow_click.png"), UriKind.Relative)));

				mAllImagesDict.Add("tool_settings",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_settings.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_settings_click",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_settings_click.png"), UriKind.Relative)));
				mAllImagesDict.Add("tool_settings_hover",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "tool_settings_hover.png"), UriKind.Relative)));

				mAllImagesDict.Add("loading",
						new BitmapImage(new Uri(Path.Combine(assetsDir, "loading.gif"), UriKind.Relative)));

			}
		}
	}
}
