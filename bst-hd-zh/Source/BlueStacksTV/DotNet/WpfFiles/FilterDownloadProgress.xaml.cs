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
	public partial class FilterDownloadProgress : Window
	{
		public static FilterDownloadProgress Instance = null;
		private bool mCallBackStatus;

		public FilterDownloadProgress()
		{
			Instance = this;
			InitializeComponent();

			this.mLabelText.Content = Locale.Strings.GetLocalizedString("INITIALIZING_TEXT");
			this.mApplyButton.Content = Locale.Strings.GetLocalizedString("APPLY_BUTTON_TEXT");
			this.mLaterButton.Content = Locale.Strings.GetLocalizedString("LATER_BUTTON_TEXT");

			this.Closing += HandleCloseEvent;

			SetControlProperties();
		}

		private void HandleCloseEvent(object sender, System.ComponentModel.CancelEventArgs e)
		{
			if (FilterDownloader.Instance != null)
			{
				FilterDownloader.Instance.ExecuteCallBack(mCallBackStatus);
			}
		}

		public void UpdateProgress(string text)
		{
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				this.mLabelText.Content = text;
			}));
		}

		private void SetControlProperties()
		{
			mCloseButton.ToolTip = Locale.Strings.GetLocalizedString("CloseTooltip");
			mCloseButton.MouseUp += CloseButton_MouseUp;
			mCloseButton.MouseLeftButtonDown += HandleMouseDown;
		}

		private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e)
		{
			this.Close();
		}

		private void HandleMouseDown(object sender, MouseButtonEventArgs e)
		{
			e.Handled = true;
		}

		private void LaterButtonClick(object sender, RoutedEventArgs e)
		{
			FilterDownloader.sUpdateLater = true;
			FilterDownloader.sStopBackgroundWorker = false;
			mCallBackStatus = true;

			this.Close();

			FilterDownloader.Instance = null;
		}

		private void ApplyButtonClick(object sender, RoutedEventArgs e)
		{
			this.mApplyButton.IsEnabled = false;
			this.mLaterButton.IsEnabled = false;
			this.mProgressBar.Visibility = Visibility.Visible;

			FilterDownloader.sUpdateLater = false;
			FilterDownloader.sStopBackgroundWorker = false;
		}

		public void EnableButtons()
		{
			StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
			{
				this.mLaterButton.IsEnabled = true;
				this.mApplyButton.IsEnabled = true;
				this.mProgressBar.Visibility= Visibility.Hidden;
			}));
		}
	}
}
