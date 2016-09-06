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

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for ChatWindow.xaml
    /// </summary>
    public partial class ChatWindow : Window
    {
        public static ChatWindow Instance = null;
        public static Browser mBrowser = null;

        public ChatWindow()
        {
            InitializeComponent();
            SetControlProperties();
            this.Closing += HandleCloseEvent;
            this.MouseLeftButtonDown += TitleBar_MouseLeftButtonDown;
            WpfUtils.SetWindowSizeAndLocation(this,"Chat");

            string chatProdUrl = String.Format("{0}/{1}", Service.Host, Common.Strings.ChatApplicationUrl);
            RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            string chatUrl = (string)urlKey.GetValue(Common.Strings.GMChatUrlKeyName, chatProdUrl);

            mBrowser = new Browser(chatUrl);
            mBrowser.Size = new System.Drawing.Size(200, 290);
            mBrowser.GetMarkupDocumentViewer().SetFullZoomAttribute((float)(this.Width / 320));

            mWinFormHost.Child = mBrowser;

            mBrowser.Navigate(chatUrl);
        }

        private void TitleBar_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            this.DragMove();
        }

        internal static void ShowWindow()
        {
            if (Instance == null)
            {
                AddWindow();
            }
            else
            {
                if (Instance.WindowState == System.Windows.WindowState.Minimized)
                {
                    Instance.WindowState = System.Windows.WindowState.Normal;
                }
                Instance.Activate();
                Instance.Topmost = true;
                Instance.Topmost = false;
                Instance.Focus();
            }
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
            Logger.Info("ChatWindow: HandleCloseEvent");

            ChatWindow.Instance.Dispatcher.Invoke(new Action(() =>
            {
                WpfUtils.SaveWindowSizeAndLocation(this,"Chat");
            }));
            this.Hide();
            ChatWindow.Instance = null;
            DisposeBrowser();
        }

        internal static void AddWindow()
        {
            Instance = new ChatWindow();
            Instance.Show();
        }

        public void DisposeBrowser()
        {
            if (mBrowser != null)
            {
                mBrowser.Dispose();
            }
        }

   
    }
}
