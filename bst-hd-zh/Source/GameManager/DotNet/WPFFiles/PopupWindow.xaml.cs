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
using System.Windows.Forms.Integration;

using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;

using Gecko.Events;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for PopupWindow.xaml
	/// </summary>
	public partial class PopupWindow : Window
	{
		public static PopupWindow Instance = null;
		public static Browser mBrowser = null;
		public static bool mIsShowTitleBar = false;
		bool ignore = false;
		public static bool IsShowTitleBar
		{
			set
			{
				mIsShowTitleBar = value;
			}
		}

		static string url = string.Empty;
		public static string Url
		{
			get
			{
				return url;
			}
			set
			{
				url = value;
			}
		}

		static bool mIsShowMinimizeButton = false;
		public static bool IsShowMinimizeButton
		{
			set
			{
				mIsShowMinimizeButton = value;
			}
		}

		public static bool AllowDragging = false;
		static string tag = string.Empty;
		public static string Tag
		{
			get
			{
				return tag;
			}
			set
			{
				tag = value;
			}
		}

		static bool hidden = false;
		public static bool Hidden
		{
			get
			{
				return hidden;
			}
			set
			{
				hidden = value;
			}
		}

		static bool dimBackground = true;
		public static bool DimBackground
		{
			get
			{
				return dimBackground;
			}
			set
			{
				dimBackground = value;
			}
		}

		private PopupWindow()
		{
			Instance = this;

			Width = GameManagerWindow.Instance.ActualWidth * .80;
			Height = GameManagerWindow.Instance.ActualHeight * .80;

			InitializeComponent();
			SetControlProperties();
			this.Closing += HandleCloseEvent;
			this.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;

			mBrowser = new Browser(url);
			//mBrowser.DocumentCompleted += mBrowser_DocumentCompleted;
			mWinFormHost.Child = mBrowser;
			mBrowser.Navigate(url);
			mBrowser.mParentWindow = this;

			if (mIsShowTitleBar)
			{
				mColTitleBar.Height = new GridLength(31);
				Height += 31;
				MaxHeight += 31;
			}
			else
			{
				mColTitleBar.Height = new GridLength(0);
				Height -= 31;
				MaxHeight -= 31;
			}

			if (mIsShowMinimizeButton)
			{
				mMinimizeButton.Visibility = Visibility.Visible;
			}
			else
			{
				mMinimizeButton.Visibility = Visibility.Hidden;
			}

		}


		private void mBrowser_DocumentCompleted(object sender, GeckoDocumentCompletedEventArgs e)
		{
			if (!ignore && !hidden)    // TODO: Nikhil: This is a hacky fix for now, will have to make sure that we are getting this event for the main url.
			{
				ignore = true;
				if (PopupWindow.Instance != null)
				{
					if (dimBackground)
					{
						new DimWindow(PopupWindow.Instance);
					}
					else
					{
						PopupWindow.Instance.ShowDialog();
					}
				}
			}
		}

		private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
		{
			if (AllowDragging)
			{
				DragMove();
			}
		}

		internal static void ShowWindow()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			string tagExists = (string)key.GetValue(tag, "0");
			if (tagExists.Equals("1"))
			{
				return;
			}
			new PopupWindow();//This will be displayed when browser is completely loaded
		}

		private void SetControlProperties()
		{
			mCloseButton.ToolTip = Locale.Strings.GetLocalizedString("CloseTooltip");
			mCloseButton.MouseUp += CloseButton_MouseUp;
			mCloseButton.MouseLeftButtonDown += HandleMouseDown;

			mMinimizeButton.ToolTip = Locale.Strings.GetLocalizedString("MinimizeTooltip");
			mMinimizeButton.MouseUp += MinimizeButton_MouseUp;
			mMinimizeButton.MouseLeftButtonDown += HandleMouseDown;
		}

		private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e)
		{
			this.Close();
		}

		private void MinimizeButton_MouseUp(object sender, MouseButtonEventArgs e)
		{
			this.WindowState = System.Windows.WindowState.Minimized;
		}

		private void HandleMouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;//disables drag move
		}

		public void HandleCloseEvent(object sender, System.ComponentModel.CancelEventArgs e)
		{
			Logger.Info("PopupWindow: HandleCloseEvent");

			GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				WpfUtils.SaveWindowSizeAndLocation(this, "Popup");
			}));
			this.Hide();
			Instance = null;
			DisposeBrowser();
		}


		public void DisposeBrowser()
		{
			if (mBrowser != null)
			{
				mBrowser.Dispose();
			}
		}

		internal void UpdateTagInRegistry(string isChecked)
		{
			Logger.Info("Do not show this message again checked");
			Logger.Info("PopupWindow.Tag: {0}", Tag);
			Logger.Info("PopupWindow.Url: {0}", Url);
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath, true);
			key.SetValue(Tag, isChecked, RegistryValueKind.String);
			key.Close();
		}
	}
}
