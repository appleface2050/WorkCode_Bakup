using BlueStacks.hyperDroid.Common;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

using BlueStacks.hyperDroid.Frontend;
using System.ComponentModel;

namespace BlueStacks.hyperDroid.GameManager
{
    /// <summary>
    /// Interaction logic for ToolBar.xaml
    /// </summary>
    public partial class ToolBar : UserControl
    {
        Dictionary<ToolBarButtons, System.Windows.Controls.Image> dictButtons = new Dictionary<ToolBarButtons, System.Windows.Controls.Image>();
        enum ToolBarButtons
        {
            golive,
            chat,
            rotate,
            shake,
            camera,
            location,
            installapk,
            folder,
            copy,
            paste,
            sound_on,
            sound_off,
            help,
        };

        [DllImport("winmm.dll")]
        public static extern int waveOutGetVolume(IntPtr h, out uint dwVolume);

        [DllImport("winmm.dll")]
        public static extern int waveOutSetVolume(IntPtr h, uint dwVolume);
        private uint mSavedVolumeLevel = 0xFFFFFFFF;

        public static ToolBar Instance = null;
        public ToolBar()
        {
            Instance = this;
            InitializeComponent();
            if (!Oem.Instance.IsAddGoLiveButton)
            {
                rowGoLive.Height = new GridLength(0);
            }
            if (!Oem.Instance.IsAddChatButton)
            {
                rowChat.Height = new GridLength(0);
            }
            SetControlProperties();
            if (!IsOneTimeSetupComplete())
            {
                EnableOTSButtons(false);
            }

        }
		private void UserControl_Loaded(object sender, RoutedEventArgs e)
		{
			if (!DesignerProperties.GetIsInDesignMode(this))
			{
				CheckToolBar();
			}
		}
		public static void CheckToolBar()
		{
			GameManagerWindow.Instance.HideToolBar(false);
		}
        internal bool IsOneTimeSetupComplete()
        {
            RegistryKey regKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            string oneTimeSetupDone = (string)regKey.GetValue("OneTimeSetupDone", "no");

            if (string.Equals(oneTimeSetupDone, "yes", StringComparison.OrdinalIgnoreCase))
                return true;
            else
                return false;
        }


        internal void EnableAppTabButtons(bool isEnable)
        {
            EnableGenericAppTabButtons(isEnable);
            EnableToggleAppTabButton(isEnable);
        }

        private void SetControlProperties()
        {
            mGoLiveButton.MouseDown += GoLiveButton_MouseDown;
            mGoLiveButton.MouseEnter += PictureBoxMouseEnter;
            mGoLiveButton.MouseLeave += PictureBoxMouseLeave;
            mGoLiveButton.MouseUp += PictureBoxMouseUp;
            mGoLiveButton.Tag = ToolBarButtons.golive;
            mGoLiveButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarWatchVideos");
            dictButtons.Add(ToolBarButtons.golive, mGoLiveButton);

            mChat.MouseDown += ChatButton_MouseDown;
            mChat.ToolTip = Locale.Strings.GetLocalizedString("Chat");
            dictButtons.Add(ToolBarButtons.chat, mChat);

            mOrientationChangeButton.MouseDown += OrientationChangeButton_MouseDown;
            mOrientationChangeButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarChangeAppSize");
            dictButtons.Add(ToolBarButtons.rotate, mOrientationChangeButton);

            mShakeButton.MouseDown += ShakeButton_MouseDown;
            mShakeButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarShake");
            dictButtons.Add(ToolBarButtons.shake, mShakeButton);

            mCameraButton.MouseDown += CameraButton_MouseDown;
            mCameraButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarCamera");
            dictButtons.Add(ToolBarButtons.camera, mCameraButton);

            mLocationButton.MouseDown += LocationButton_MouseDown;
            mLocationButton.ToolTip = Locale.Strings.GetLocalizedString("LocationText");
            dictButtons.Add(ToolBarButtons.location, mLocationButton);

            mApkInstallButton.MouseDown += ApkInstallButton_MouseDown;
            mApkInstallButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarInstallApk");
            dictButtons.Add(ToolBarButtons.installapk, mApkInstallButton);

            mFileButton.MouseDown += FileButton_MouseDown;
            mFileButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarCopyFromWindows");
            dictButtons.Add(ToolBarButtons.folder, mFileButton);

            mCopyButton.MouseDown += CopyButton_MouseDown;
            mCopyButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarCopy");
            dictButtons.Add(ToolBarButtons.copy, mCopyButton);

            mPasteButton.MouseDown += PasteButton_MouseDown;
            mPasteButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarPaste");
            dictButtons.Add(ToolBarButtons.paste, mPasteButton);

            mSoundButton.MouseDown += SoundButton_MouseDown;
            mSoundButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarVolume");
            dictButtons.Add(ToolBarButtons.sound_on, mSoundButton);

            mHelpButton.MouseDown += HelpButton_MouseDown;
            mHelpButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarHelp");
            dictButtons.Add(ToolBarButtons.help, mHelpButton);

        }

        private void PictureBoxMouseEnter(object sender, EventArgs e)
        {
            if (BTVManager.sStreaming)
            {
                mGoLiveButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarLiveStreaming");
            }
            else
            {
                mGoLiveButton.ToolTip = Locale.Strings.GetLocalizedString("ToolBarWatchVideos");
            }
            if (mGoLiveButton.IsEnabled)
            {
                mGoLiveButton.Cursor = Cursors.Hand;
                CustomPictureBox.SetBitmapImage(mGoLiveButton, mGoLiveButton.Tag.ToString() + "_hover");
            }
        }

        private void PictureBoxMouseLeave(object sender, EventArgs e)
        {
            mGoLiveButton.Cursor = null;
            if (BTVManager.sStreaming)
                CustomPictureBox.SetBitmapImage(mGoLiveButton, mGoLiveButton.Tag.ToString() + "_live");
            else if (Utils.IsProcessAlive(Strings.BlueStacksTVLockName))
                CustomPictureBox.SetBitmapImage(mGoLiveButton, mGoLiveButton.Tag.ToString() + "_on");
            else
                CustomPictureBox.SetBitmapImage(mGoLiveButton, mGoLiveButton.Tag.ToString());
        }

        private void PictureBoxMouseUp(object sender, MouseEventArgs e)
        {
            CustomPictureBox.SetBitmapImage(mGoLiveButton, mGoLiveButton.Tag.ToString() + "_hover");
        }

        private void GoLiveButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            CustomPictureBox.SetBitmapImage(mGoLiveButton, mGoLiveButton.Tag.ToString() + "_click");
            if (BTVManager.sStreaming)
            {
                BTVManager.ShowStreamWindow();
            }
            else
            {
                BTVManager.ShowStreamWindow();
                Stats.SendBtvFunnelStats("clicked_go_live_button", null, null, false);
            }
            e.Handled = true;
        }

        public void HandleGoLiveButton(string status)
        {
            if (rowGoLive.Height.Value > 0)
            {
                if (!string.IsNullOrEmpty(status))
                {
                    status = String.Format("_{0}", status);
                }
                if (mGoLiveButton.IsEnabled)
                {
                    mGoLiveButton.Cursor = Cursors.Hand;
                    CustomPictureBox.SetBitmapImage(mGoLiveButton, mGoLiveButton.Tag.ToString() + status);
                }
            }
        }

        private void ChatButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ChatWindow.ShowWindow();
            e.Handled = true;
        }

        private void OrientationChangeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            TabButtons.Instance.HandleRotate();
            SendCommandToBstCmdProcessor("toggle");
            e.Handled = true;
        }
        private void ShakeButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCommandToFrontend("shake");
            GameManagerWindow.Instance.ShakeWindow();
            e.Handled = true;
        }
        private void CameraButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            ShareScreenshot();
            e.Handled = true;
        }

        internal static void ShareScreenshot()
        {
            Random random = new Random();
            int randomNum = random.Next(0, 100000);

            string fileBaseName = String.Format(@"bstSnapshot_{0}.jpg", randomNum);
            string finalBaseName = String.Format(@"final_{0}", fileBaseName);

            string sharedFolder = Common.Strings.SharedFolderDir;
            string sharedFolderName = Common.Strings.SharedFolderName;

            string origFileName = System.IO.Path.Combine(sharedFolder, fileBaseName);
            string modifiedFileName = System.IO.Path.Combine(sharedFolder, finalBaseName);

            System.Drawing.Bitmap bitmap = GetScreenshot();
            bitmap.Save(origFileName, ImageFormat.Jpeg);

            try
            {
                Utils.AddUploadTextToImage(origFileName, modifiedFileName);
                File.Delete(origFileName);
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to add upload text to snapshot. err: " + ex.ToString());
                finalBaseName = fileBaseName;
                modifiedFileName = origFileName;
            }

            string url = String.Format("http://127.0.0.1:{0}/{1}",
                    Common.VmCmdHandler.s_ServerPort, Common.Strings.SharePicUrl);

            string androidPath = "/mnt/sdcard/windows/" +
                sharedFolderName + "/" + System.IO.Path.GetFileName(finalBaseName);
            Logger.Info("share screenshot: androidPath: " + androidPath);

            Dictionary<string, string> data = new Dictionary<string, string>();
            data.Add("data", androidPath);

            Logger.Info("Sending snapshot upload request.");

            string result = "";
            try
            {
                result = Common.HTTP.Client.Post(url, data, null, false);
            }
            catch (Exception ex)
            {
                Logger.Error(ex.ToString());
                Logger.Error("Post failed. url = {0}, data = {1}", url, data);
            }
            return;

        }

        public static System.Drawing.Bitmap GetScreenshot()
        {
            System.Windows.Point p = ContentControl.Instance.PointToScreen(new System.Windows.Point(0, 0));
            System.Drawing.Point startPoint = new System.Drawing.Point(Convert.ToInt32(p.X), Convert.ToInt32(p.Y));
            Point endPoint = ContentControl.Instance.PointToScreen(new Point(ContentControl.Instance.ActualWidth, ContentControl.Instance.ActualHeight));
            System.Drawing.Size size = new System.Drawing.Size(Convert.ToInt32(endPoint.X - startPoint.X), Convert.ToInt32(endPoint.Y - startPoint.Y));
            System.Drawing.Bitmap bitmap = new System.Drawing.Bitmap(size.Width, size.Height);
            using (System.Drawing.Graphics g = System.Drawing.Graphics.FromImage(bitmap))
            {
                g.CopyFromScreen(startPoint, System.Drawing.Point.Empty, size);
            }
            return bitmap;
        }


        private void LocationButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            AppHandler.ShowApp(Locale.Strings.Location, "com.location.provider", "com.location.provider.MapsActivity.java", "", true);
            e.Handled = true;
        }

        private void ApkInstallButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "Android Files (.apk)|*.apk";
                dialog.RestoreDirectory = true;
                System.Windows.Forms.DialogResult dialogResult = dialog.ShowDialog();
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    Logger.Info("File Selected : " + dialog.FileName);
                    string apkPath = dialog.FileName;
                    Logger.Info("Console: Installing apk: {0}", apkPath);

                    Thread apkInstall = new Thread(delegate ()
                    {
                        Utils.CallApkInstaller(apkPath, false);
                    });
                    apkInstall.IsBackground = true;
                    apkInstall.Start();
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to get install apk. Error: " + ex.ToString());
            }
            e.Handled = true;
        }
        private void FileButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                System.Windows.Forms.OpenFileDialog dialog = new System.Windows.Forms.OpenFileDialog();
                dialog.Filter = "All Files (*.*)|*.*";
                dialog.Multiselect = true;
                System.Windows.Forms.DialogResult dialogResult = dialog.ShowDialog();
                string sharedFolder = Common.Strings.SharedFolderDir;
                string sharedFolderName = Common.Strings.SharedFolderName;
                string origFilePath;
                string fileBaseName;
                string[] files = dialog.FileNames;
                if (dialogResult == System.Windows.Forms.DialogResult.OK)
                {
                    Logger.Info("File Selected : " + dialog.FileName);
                    origFilePath = dialog.FileName;
                    fileBaseName = System.IO.Path.GetFileName(origFilePath);

                    string configPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(configPath);
                    int fileSystem = (int)key.GetValue("FileSystem", 0);
                    if (fileSystem == 0)
                    {
                        Logger.Info("Shared folders disabled");
                        return;
                    }
                }
                else
                {
                    Logger.Info("No file selected");
                    return;
                }

                string url = String.Format("http://127.0.0.1:{0}/{1}", Common.VmCmdHandler.s_ServerPort, Common.Strings.SharePicUrl);
                foreach (string file in files)
                {
                    fileBaseName = System.IO.Path.GetFileName(file);
                    string destFilePath = System.IO.Path.Combine(sharedFolder, fileBaseName);

                    string androidPath = "/mnt/sdcard/windows/" + sharedFolderName + "/" + fileBaseName;
                    Logger.Info("androidPath: " + androidPath);

                    Dictionary<string, string> data = new Dictionary<string, string>();
                    data.Add("data", androidPath);

                    Logger.Info("Sending download file request.");

                    string result = "";
                    try
                    {
                        result = Common.HTTP.Client.Post(url, data, null, false);
                    }
                    catch (Exception ex)
                    {
                        Logger.Error(ex.ToString());
                        Logger.Error("Post failed. url = {0}, data = {1}", url, data);
                        return;
                    }
                    if (result != null)
                    {
                        System.IO.File.Copy(file, destFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                Logger.Error("Failed to copy file. Error: " + ex.ToString());
            }
            e.Handled = true;
        }
        private void CopyButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCommandToBstCmdProcessor("copyfromappplayer");
            e.Handled = true;
        }
        private void PasteButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            SendCommandToBstCmdProcessor("pastetoappplayer");
            e.Handled = true;
        }
        private void SoundButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            try
            {
                string imageName = "sound_on";
                if (mSoundButton.ImageName.ToString().Equals(imageName))
                {
                    if (!Utils.IsOSWinXP())
                    {
                        waveOutGetVolume(IntPtr.Zero, out mSavedVolumeLevel);
                        waveOutSetVolume(IntPtr.Zero, 0);
                        VolumeMixer.SetVolume(null, 0f);
                    }
                    FrontendHandler.MuteFrontend();
                    imageName = "sound_off";
                }
                else
                {
                    if (!Utils.IsOSWinXP())
                    {
                        waveOutSetVolume(IntPtr.Zero, mSavedVolumeLevel);
                        VolumeMixer.SetVolume(null, 50f);
                    }
                    FrontendHandler.UnmuteFrontend();
                }

                mSoundButton.ImageName = imageName;
                GameManagerWindow.Instance.Activate();
            }
            catch (Exception ex)
            {
                Logger.Error("Exception in SoundButton_MouseDown" + ex.ToString());
            }
            e.Handled = true;
        }
        private void HelpButton_MouseDown(object sender, MouseButtonEventArgs e)
        {
            string url = "http://bluestacks-cloud.appspot.com/static/support_page/index.html";
            RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
            url = (string)urlKey.GetValue(Common.Strings.GMSupportUrlKeyName, url);
            TabButtons.Instance.ShowWebPage("Help", url, null);
            e.Handled = true;
        }

        private void SendCommandToBstCmdProcessor(string command)
        {
            string url = String.Format("http://127.0.0.1:{0}/{1}",
                    Common.VmCmdHandler.s_ServerPort, command);

            string result = "";
            try
            {
                result = Common.HTTP.Client.Get(url, null, false);
                Logger.Info("post command: " + command + ", result: " + result);
            }
            catch (Exception ex)
            {
                Logger.Error("An error occured. Err :" + ex.ToString());
            }
        }

        private void SendCommandToFrontend(string command)
        {
            Thread t = new Thread(delegate ()
            {
                try
                {
                    RegistryKey key = Registry.LocalMachine.OpenSubKey(
                        Common.Strings.HKLMAndroidConfigRegKeyPath);
                    int frontendPort = (int)key.GetValue("FrontendServerPort", 2871);
                    string url = String.Format("http://127.0.0.1:{0}/{1}",
                        frontendPort, command);

                    Logger.Info("Sending get request to {0}", url);
                    string res = Common.HTTP.Client.Get(url, null, false);
                    Logger.Info("Got response for {0}: {1}", url, res);
                }
                catch (Exception ex)
                {
                    Logger.Error(ex.ToString());
                }
            });
            t.IsBackground = true;
            t.Start();
        }

        public void EnableToggleAppTabButton(bool isEnable)
        {
            EnableButton(mOrientationChangeButton, isEnable);
        }

        private void EnableButton(CustomPictureBox index, bool isEnable)
        {
            if (isEnable)
            {
                index.ImageName = index.ImageName.ToString().Replace("_dis", string.Empty);
            }
            else
            {
                if (!index.ImageName.EndsWith("_dis"))
                {
                    index.ImageName = index.ImageName.ToString() + "_dis";
                }
            }
            index.IsEnabled = isEnable;
        }

        public void EnableGenericAppTabButtons(bool isEnable)
        {
            EnableButton(mShakeButton, isEnable);
            EnableButton(mCopyButton, isEnable);
            EnableButton(mPasteButton, isEnable);
            EnableButton(mCameraButton, isEnable);
            EnableButton(mFileButton, isEnable);
        }

        public void EnableOTSButtons(bool isEnable)
        {
            EnableButton(mShakeButton, isEnable);
            EnableButton(mCopyButton, isEnable);
            EnableButton(mPasteButton, isEnable);
            EnableButton(mCameraButton, isEnable);
            EnableButton(mFileButton, isEnable);
            EnableButton(mOrientationChangeButton, isEnable);
            EnableButton(mLocationButton, isEnable);
            EnableButton(mSoundButton, isEnable);
            EnableButton(mApkInstallButton, isEnable);
        }

		
	}
}

