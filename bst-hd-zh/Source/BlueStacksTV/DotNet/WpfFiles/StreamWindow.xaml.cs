using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Windows;
using System.Diagnostics;
using System.Threading;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;
using System.Windows.Forms;

using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Windows.Interop;
using System.Windows.Forms.Integration;

namespace BlueStacks.hyperDroid.BlueStacksTV
{

    public partial class StreamWindow : Window
    {
        public static string BROWSER_NAME = "StreamBrowser";
        public StreamBrowser mBrowser;
        public static StreamWindow Instance = null;

        public static Grid sGrid;

        public StreamWindow()
        {
            Instance = this;
            InitializeComponent();

            this.Closing += HandleCloseEvent;
            SetControlProperties();

            string url = StreamWindowUtility.GetStreamWindowUrl();
            WpfUtils.SetWindowSizeAndLocation(this, "BTV");
            if (url != null)
            {
                mBrowser = new StreamBrowser(url);
                mBrowser.Name = BROWSER_NAME;
                mBrowser.Size = new System.Drawing.Size(200, 290);
                mBrowser.GetMarkupDocumentViewer().SetFullZoomAttribute((float)(this.Width / 320));
                mBrowser.Location = new System.Drawing.Point(1, 1);

                Grid grid = new Grid();
                WindowsFormsHost host = new WindowsFormsHost();
                host.Child = mBrowser;
                Grid.SetColumnSpan(grid, 3);
                Grid.SetRowSpan(grid, 3);
                grid.Children.Add(host);
                BrowserGrid.Children.Add(grid);
                mBrowser.Navigate(url);
            }
            this.Activated += StreamWindow_Activated;
            this.MouseLeftButtonDown += StreamWindow_MouseLeftButtonDown;
            TwitchWindow.AddTwitchWindowIfNeeded();
        }

        private void StreamWindow_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }
	
	private void StreamWindow_Activated(object sender, EventArgs e)
        {
            Logger.Info("HandleActivatedEvent");
	    if (FilterWindow.Instance != null)
		    FilterWindow.Instance.Activate();

	    if (LayoutWindow.Instance != null)
		    LayoutWindow.Instance.Activate();
        }





        public void HandleCloseEvent(object sender, System.ComponentModel.CancelEventArgs e)
        {
            Logger.Info("StreamWindow: HandleCloseEvent");

            StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                WpfUtils.SaveWindowSizeAndLocation(this, "BTV");
            }));

	    if (FilterWindow.Instance != null)
                FilterWindow.Instance.CloseFilterWindow("[]");

	    if (LayoutWindow.Instance != null)
            {
                if (StreamManager.Instance != null)
			LayoutWindow.Instance.UpdateRegistry();
                LayoutWindow.Instance.Close();
            }

            if (StreamManager.Instance != null)
            {
                StreamManager.Instance.Shutdown();
                StreamManager.Instance.mIsStreaming = false;
            }

            Instance = null;
            StreamManager.Instance = null;
            DisposeBrowser(); 

            Stats.ResetSessionId();

            KillOBS();
            Logger.Info("Exiting");
            Environment.Exit(0);
        }

        private static void KillOBS()
        {
            try
            {
                /*
                 * Wait for HD-OBS to stop gracefully
                 * for 5 sec
                 */
                int retry = 0;
                int RETRY_MAX = 25;
                while (retry < RETRY_MAX)
                {
                    if (Process.GetProcessesByName("HD-OBS").Length == 0)
                    {
                        break;
                    }
                    retry++;
                    if (retry < RETRY_MAX)
                    {
                        Logger.Info("Waiting for HD-OBS to quit gracefully, retry: {0}", retry);
                        Thread.Sleep(200);
                    }
                }
                if (retry >= RETRY_MAX)
                    Utils.KillProcessByName("HD-OBS");
            }
            catch (Exception ex)
            {
                Logger.Info("Failed to kill HD-OBS.exe...Err : " + ex.ToString());
            }
        }

        public void DisposeBrowser()
        {
            if (mBrowser != null)
                mBrowser.Dispose();
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

        private void HandleMouseDown(object sender, MouseButtonEventArgs e)
        {
            e.Handled = true;//disables drag move
        }

        private void CloseButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            if (StreamManager.Instance != null && StreamManager.Instance.mIsStreaming)
            {
                Stats.SendBtvFunnelStats("stream_ended",
                        "stream_ended_reason",
                        "stream_window_closed",
                        false);

                string body = Locale.Strings.GetLocalizedString("CloseMessagePrompt");
                string leftButton = Locale.Strings.GetLocalizedString("EndStreaming");
                string rightButton = Locale.Strings.GetLocalizedString("KeepStreaming");
                body = body.Replace("\\n", Environment.NewLine);
                MessageBoxResult result = CustomMessageBoxWindow.Show(body, leftButton, rightButton);

                if (result == MessageBoxResult.Yes)
                {
                    this.Close();
                }
            }
            else
                this.Close();
        }

        public void EvaluateJS(string script)
        {
            mBrowser.EvaluateJS(script);
        }

        public void ChangeWebCamState()
        {
            mBrowser.EvaluateJS("toggle_webcam_via_filter();");
        }

        private void MinimizeButton_MouseUp(object sender, MouseButtonEventArgs e)
        {
            this.WindowState = WindowState.Minimized;
        }

        public void AddPanel(System.Windows.Forms.Panel panel)
        {
            sGrid = new Grid();
            System.Windows.Forms.Integration.WindowsFormsHost host = new System.Windows.Forms.Integration.WindowsFormsHost();
            host.Child = panel;
            sGrid.Children.Add(host);
            Grid.SetColumn(sGrid, 1);
            Grid.SetRow(sGrid, 1);

            this.BrowserGrid.Children.Add(sGrid);
        }

        public void ShowGrid()
        {
            sGrid.Visibility = Visibility.Visible;
        }

        public void HideGrid()
        {
            sGrid.Visibility = Visibility.Hidden;
        }

        public void StreamStarted()
        {
            mCloseButton.ToolTip = Locale.Strings.GetLocalizedString("CloseWhileStreamingTooltip");
        }

        public void StreamEnded()
        {
            mCloseButton.ToolTip = Locale.Strings.GetLocalizedString("CloseTooltip");
        }

    }
}
