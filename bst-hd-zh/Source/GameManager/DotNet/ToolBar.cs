using System;
using System.IO;
using System.Net;
using System.Text;
using System.Data;
using System.Drawing;
using Microsoft.Win32;
using System.Threading;
using System.Diagnostics;
using System.Drawing.Text;
using System.Net.Security;
using System.Globalization;
using System.Windows.Forms;
using System.ComponentModel;
using System.Drawing.Drawing2D;
using System.Collections.Generic;
using System.Net.NetworkInformation;
using System.Runtime.InteropServices;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.GameManager
{
	public class ToolBar : UserControl
	{
		[DllImport("winmm.dll")]
		public static extern int waveOutGetVolume(IntPtr h, out uint dwVolume);

		[DllImport("winmm.dll")]
		public static extern int waveOutSetVolume(IntPtr h, uint dwVolume);

		private const int WM_QUERYENDSESSION = 0x11;

		private static int mDividerWidth
		{ get { return GetDividerWidth(); } }

		private static int mToolBoxWidth
		{
			get { return 60 * GameManager.sDpi / Utils.DEFAULT_DPI; }
		}

		private static int mButtonWidth
		{
			get
			{

				if (sToolBar.Height < (Enum.GetNames(typeof(EnumGameManagerToolBarButtonOrder)).Length * mButtonHeight))
				{
					int newHeight = sToolBarHeight / Enum.GetNames(typeof(EnumGameManagerToolBarButtonOrder)).Length;
					return  (58 * GameManager.sDpi / Utils.DEFAULT_DPI * newHeight) / mButtonHeight;
				}
				else
				{
					return 58 * GameManager.sDpi / Utils.DEFAULT_DPI;
				}
			}
		}
		private static int mButtonHeight
		{
			get
			{
				if (sToolBar.Height < (Enum.GetNames(typeof(EnumGameManagerToolBarButtonOrder)).Length * 48 * GameManager.sDpi / Utils.DEFAULT_DPI))
				{
					return sToolBarHeight / Enum.GetNames(typeof(EnumGameManagerToolBarButtonOrder)).Length;
				}
				else
				{
					return 48 * GameManager.sDpi / Utils.DEFAULT_DPI;
				}
			}
		}
		private static int mButtonPosX
		{
			get { return (mToolBoxWidth - mButtonWidth) / 2; }
		}

		public TableLayoutPanel mButtonPanel = null;
		public TableLayoutPanel mPanel = null;
		private static int				mVerticalSpacing	= 10;

		private static int				mBorderSize		= 1;

		private Dictionary<string, Image>	mImagesDict		= null;

		private GradientPanel			mPanelDivider;
		public GradientPanel			mPanelToolBox;

		private PictureBox[]			mPBToolButtons;

		public static int			sToolBarWidth
		{
			get { return mDividerWidth + mToolBoxWidth + mBorderSize; }
		}
		public static int sToolBarHeight
		{
			get { return GameManager.sGameManager.Height - GameManager.TransparentBox.Height ; }
		}

		private int				mButtonCount		= 0;

		private	uint				mSavedVolumeLevel	= 0xFFFFFFFF;
		private bool				mIsSoundMuted		= false;

		public static bool			sMakeVisible		= false;
		private bool				mTempDisableHandler	= false;
		public enum EnumGameManagerToolBarButtonOrder
		{
			golive,
			chat,
			rotate,
			shake,
			camera,
			location,
			installapk,
			folder,         // copy files from Windows to Android
			copy,
			paste,
			sound_on,
			help
		};
		private static List<string> lstToolBarButtonOrder = null;

		public static List<string> slstToolBarButtonOrder
		{
			get
			{
				if (lstToolBarButtonOrder == null)
				{
					InitToolBarButtonsList();
				}
				return lstToolBarButtonOrder;
			}
		}

		//This tool tip is made global because it's changed dynamically.
		private ToolTip goliveToolTip;


		public static void InitToolBarButtonsList()
		{
            lstToolBarButtonOrder = new List<string>();
			if (Oem.Instance.IsAddGoLiveButton)
			{
				lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.golive.ToString());
			}
			if (Oem.Instance.IsAddChatButton)
			{
				lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.chat.ToString());
			}
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.rotate.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.shake.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.camera.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.location.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.installapk.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.folder.ToString());	// copy files from Windows to Android
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.copy.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.paste.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.sound_on.ToString());
			lstToolBarButtonOrder.Add(EnumGameManagerToolBarButtonOrder.help.ToString());
		}

		public static ToolBar sToolBar = null;
		public ToolBar()
		{
			sToolBar = this;
			this.AutoScaleDimensions = new SizeF(6F, 13F);
			this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
			this.BackColor = GMColors.ToolBoxBorderColor;
			this.Dock = DockStyle.Fill;

			mButtonCount = slstToolBarButtonOrder.Count;
			mPBToolButtons = new PictureBox[mButtonCount];


			if (mImagesDict == null)
			{
				mImagesDict = new Dictionary<string, Image>();

				InitImagesDict();
				AddToImagesDict("sound_off");
			}

			InitUi();
			AddButtons();
		}

		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_QUERYENDSESSION:
					Logger.Info("Received message WM_QUERYENDSESSION in ToolBar");
					GameManager.sGameManager.mSessionEnding = true;
					GameManager.sGameManager.Close();
					break;
			}

			base.WndProc(ref m);
		}

		public static int GetDividerWidth()
		{
			int mDividerWidth=0;
			string parentStyleTheme = "Em";
			using (RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath))
			{
				parentStyleTheme = (string)configKey.GetValue("ParentStyleTheme", parentStyleTheme);
			}
			if (parentStyleTheme == "Em")
			{
				mDividerWidth = 5;
			}
			// adjust for dpi
			return mDividerWidth * GameManager.sDpi / Utils.DEFAULT_DPI;
		}

		protected override void SetVisibleCore(bool value)
		{
			if (sMakeVisible)
				base.SetVisibleCore(true);
			else
				base.SetVisibleCore(false);
		}

		private void InitImagesDict()
		{
			foreach (string btn in slstToolBarButtonOrder)
			{
				AddToImagesDict(btn);
			}
		}

		private void AddToImagesDict(string name)
		{
			string baseName = Path.Combine(GameManager.sAssetsDir, name);
			string tag = "toolbar_" + name;

			mImagesDict.Add(tag, Image.FromFile(baseName + ".png"));
			mImagesDict.Add(tag + "_click", Image.FromFile(baseName + "_click.png"));
			mImagesDict.Add(tag + "_hover", Image.FromFile(baseName + "_hover.png"));
			mImagesDict.Add(tag + "_dis", Image.FromFile(baseName + "_dis.png"));

			if (string.Compare(name, "golive", true) == 0)
			{
				mImagesDict.Add(tag + "_live", Image.FromFile(baseName + "_live.gif"));
				mImagesDict.Add(tag + "_on", Image.FromFile(baseName + "_on.png"));
			}
		}

		public void EnableToggleAppTabButton() {
			EnableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.rotate.ToString()));
		}

		public void DisableToggleAppTabButton() {
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.rotate.ToString()));
		}

		public void DisableAppTabButtons() {
			DisableGenericAppTabButtons();
			DisableToggleAppTabButton();
		}

		public void EnableGenericAppTabButtons()
		{
			EnableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.shake.ToString()));
			EnableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.copy.ToString()));
			EnableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.paste.ToString()));
			EnableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.camera.ToString()));
			EnableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.folder.ToString()));
		}

		public void DisableGenericAppTabButtons()
		{
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.shake.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.copy.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.paste.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.camera.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.folder.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.shake.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.copy.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.paste.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.camera.ToString()));
			DisableButton((int)slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.folder.ToString()));
		}

		private void EnableButton(int index)
		{
			string tag = slstToolBarButtonOrder[index];
			tag = "toolbar_" + tag;
			mPBToolButtons[index].Enabled = true;
			mPBToolButtons[index].Image = mImagesDict[tag];
		}

		private void DisableButton(int index)
		{
			string tag = slstToolBarButtonOrder[index];
			tag = "toolbar_" + tag + "_dis";
			mPBToolButtons[index].Enabled = false;
			mPBToolButtons[index].Image = mImagesDict[tag];
		}

		private void AddButtons()
		{
			this.SuspendLayout();

			int firstY = mVerticalSpacing;
			int pos = 0;
			foreach (string btn in slstToolBarButtonOrder)
			{
				int posY = firstY + pos * (mVerticalSpacing + mButtonHeight);

				mPBToolButtons[pos] = new PictureBox();
				mPBToolButtons[pos].Tag = "toolbar_" + btn.ToString();
				mPBToolButtons[pos].Location = new Point(mButtonPosX, posY);
				mPBToolButtons[pos].MouseClick += HandleToolButtonClick;
				mPBToolButtons[pos].Enabled = true;
				mPBToolButtons[pos].SizeMode = PictureBoxSizeMode.Zoom;
				SetButtonProperties(mPBToolButtons[pos]);
				switch(btn.ToString())
				{
					case "golive":
						goliveToolTip = new ToolTip();
						goliveToolTip.ShowAlways = true;
						goliveToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarWatchVideos"]);
						break;
					case "rotate" :
						ToolTip rotateToolTip = new ToolTip();
						rotateToolTip.ShowAlways = true;
						rotateToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarChangeAppSize"]);
						break;
					case "shake" :
						ToolTip shakeToolTip = new ToolTip();
						shakeToolTip.ShowAlways = true;
						shakeToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarShake"]);
						break;
					case "camera" :
						ToolTip cameraToolTip = new ToolTip();
						cameraToolTip.ShowAlways = true;
						cameraToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarCamera"]);
						break;
					case "location" :
						ToolTip locationToolTip = new ToolTip();
						locationToolTip.ShowAlways = true;
						locationToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["LocationText"]);
						break;
					case "installapk" :
						ToolTip installapkToolTip = new ToolTip();
						installapkToolTip.ShowAlways = true;
						installapkToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarInstallApk"]);
						break;
					case "folder" :
						ToolTip folderToolTip = new ToolTip();
						folderToolTip.ShowAlways = true;
						folderToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarCopyFromWindows"]);
						break;
					case "copy" :
						ToolTip copyToolTip = new ToolTip();
						copyToolTip.ShowAlways = true;
						copyToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarCopy"]);
						break;
					case "paste" :
						ToolTip pasteToolTip = new ToolTip();
						pasteToolTip.ShowAlways = true;
						pasteToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarPaste"]);
						break;
					case "sound_on" :
						ToolTip soundToolTip = new ToolTip();
						soundToolTip.ShowAlways = true;
						soundToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarVolume"]);
						break;
					case "help" :
						ToolTip helpToolTip = new ToolTip();
						helpToolTip.ShowAlways = true;
						helpToolTip.SetToolTip(mPBToolButtons[pos],
								GameManager.sLocalizedString["ToolBarHelp"]);
						break;
					case "chat" :
						ToolTip chatToolTip = new ToolTip();
						chatToolTip.ShowAlways = true;
						chatToolTip.SetToolTip(mPBToolButtons[pos],
								"Chat");
						break;
				}
				mButtonPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 1F));
				mButtonPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 50F));
				mButtonPanel.Controls.Add(mPBToolButtons[pos], 0, pos+1);

				pos++;
			}

			this.ResumeLayout(false);
		}

		private void HandleToolButtonClick(Object sender, MouseEventArgs e)
		{
			string senderTag = (string)(((PictureBox)sender).Tag);
			string tag = senderTag.Substring(senderTag.IndexOf('_') + 1);

			// special handling for sound_off tag as we don't have a separate button for it
			if (String.Compare(tag, "sound_off", true) == 0)
			{
				tag = "sound_on";
			}

			if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.golive.ToString(), true) == 0)
			{
				HandleGoLiveClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.rotate.ToString(), true) == 0)
			{
				HandleRotateClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.shake.ToString(), true) == 0)
			{
				HandleShakeClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.camera.ToString(), true) == 0)
			{
				HandleCameraClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.location.ToString(), true) == 0)
			{
				HandleLocationClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.installapk.ToString(), true) == 0)
			{
				HandleInstallApkClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.folder.ToString(), true) == 0)
			{
				HandleFolderClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.copy.ToString(), true) == 0)
			{
				HandleCopyClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.paste.ToString(), true) == 0)
			{
				HandlePasteClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.sound_on.ToString(), true) == 0)
			{
				HandleSoundClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.help.ToString(), true) == 0)
			{
				HandleHelpClicked();
			}
			else if (String.Compare(tag, EnumGameManagerToolBarButtonOrder.chat.ToString(), true) == 0)
			{
				HandleChatClicked();
			}
		}

		private void SetButtonProperties(PictureBox button)
		{
			button.Image = mImagesDict[(String)button.Tag];
			button.Anchor = AnchorStyles.None;
			button.Size = new Size(mButtonWidth,mButtonHeight);
			button.MouseEnter += new EventHandler(this.ToolBarButtonMouseEnter);
			button.MouseDown += new MouseEventHandler(this.ToolBarButtonMouseDown);
			button.MouseUp += new MouseEventHandler(this.ToolBarButtonMouseUp);
			button.MouseLeave += new EventHandler(this.ToolBarButtonMouseLeave);
			button.BackColor = GMColors.TransparentColor;
		}

		private void ToolBarButtonMouseEnter(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;


			if (string.Compare((String)button.Tag, "toolbar_golive", true) == 0)
			{
				if (StreamManager.sStreaming)
					goliveToolTip.SetToolTip(button, GameManager.sLocalizedString["ToolBarLiveStreaming"]);
				else
					goliveToolTip.SetToolTip(button, GameManager.sLocalizedString["ToolBarWatchVideos"]);

				if (mTempDisableHandler)
				return;
			}
			if (button.Enabled)
			{
				button.Cursor = Cursors.Hand;
				button.Image = mImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ToolBarButtonMouseDown(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (string.Compare((String)button.Tag, "toolbar_golive", true) == 0 && mTempDisableHandler)
				return;
			if (button.Enabled)
			{
				button.Image = mImagesDict[(String)button.Tag + "_click"];
			}
		}

		private void ToolBarButtonMouseUp(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (string.Compare((String)button.Tag, "toolbar_golive", true) == 0 && mTempDisableHandler)
				return;
			if (button.Enabled)
			{
				button.Image = mImagesDict[(String)button.Tag + "_hover"];
			}
		}

		private void ToolBarButtonMouseLeave(object sender, System.EventArgs e)
		{
			PictureBox button = (PictureBox)sender;
			if (string.Compare((String)button.Tag, "toolbar_golive", true) == 0 && mTempDisableHandler)
				return;
			if (button.Enabled)
			{
				button.Cursor = Cursors.Default;
				button.Image = mImagesDict[(String)button.Tag];
			}
		}

		private void InitUi()
		{

			mPanelToolBox = new GradientPanel(GMColors.ToolBoxGradientTop, GMColors.ToolBoxGradientBottom, LinearGradientMode.Vertical);
			mPanelToolBox.Dock = DockStyle.Fill;
			mPanelToolBox.Margin = new Padding(0);
			mButtonPanel = new TableLayoutPanel();
			mButtonPanel.ColumnCount = 1;
			mButtonPanel.Dock = DockStyle.Fill;
			mButtonPanel.Margin = new Padding(0);
			mButtonPanel.BackColor = Color.Transparent;
			mButtonPanel.RowStyles.Add(new RowStyle(SizeType.Absolute, GameManager.TransparentBox.Height));
			mPanelToolBox.Controls.Add(mButtonPanel);

			mPanelDivider = new GradientPanel(GMColors.ToolBoxDividerGradientTop, GMColors.ToolBoxDividerGradientBottom, LinearGradientMode.Vertical);
			mPanelDivider.Dock = DockStyle.Fill;
			mPanelDivider.Margin = new Padding(0);

			mPanel = new TableLayoutPanel();
			mPanel.ColumnCount = 2;
			mPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, mToolBoxWidth));
			mPanel.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, mDividerWidth));
			mPanel.Controls.Add(this.mPanelToolBox, 0, 0);
			mPanel.Controls.Add(this.mPanelDivider, 1, 0);
			mPanel.Dock = DockStyle.Fill;
			mPanel.Margin = new Padding(0);
			mPanel.RowCount = 1;
			mPanel.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
			this.Controls.Add(mPanel);
		}

		private void HandleChatClicked()
		{
			GameManager.sGameManager.ShowChatWindow();
		}

		private void HandleGoLiveClicked()
		{
			mTempDisableHandler = true;

			if (StreamManager.sStreaming)
			{
				mTempDisableHandler = true;
				GameManager.sGameManager.ShowStreamWindow();
			}
			else
			{
				HandleGoLiveButton("on");
				GameManager.sGameManager.ShowStreamWindow();
				Stats.SendBtvFunnelStats("clicked_go_live_button", null, null, false);
			}
		}

		public void HandleGoLiveButton(string status)
		{
			if (slstToolBarButtonOrder.Contains(EnumGameManagerToolBarButtonOrder.golive.ToString()))
			{
				int buttonIndex = slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.golive.ToString());

				PictureBox button = mPBToolButtons[buttonIndex];


				if (string.IsNullOrEmpty(status))
					mTempDisableHandler = false;
				else
				{
					mTempDisableHandler = true;
					status = String.Format("_{0}", status);
				}
				if (button.Enabled)
				{
					button.Cursor = Cursors.Hand;
					button.Image = mImagesDict[(String)button.Tag + status];
				}
			}
		}

		private void HandleRotateClicked()
		{
			TabBar.sTabBar.HandleRotate();
			SendCommandToBstCmdProcessor("toggle");
		}

		private void HandleShakeClicked()
		{
			SendCommandToFrontend("shake");
			GameManager.sGameManager.ShakeWindow();
		}

		private void HandleCameraClicked()
		{
			TabBar.sTabBar.GetCurrentTab().ShareScreenshot();
		}

		private void HandleLocationClicked()
		{
			GameManager.sGameManager.ShowApp(Locale.Strings.Location,
					"com.location.provider",
					"com.location.provider.MapsActivity.java",
					"",
					true);
		}

		private void HandleInstallApkClicked()
		{
			try
			{
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Filter = "Android Files (.apk)|*.apk";
				dialog.RestoreDirectory = true;
				DialogResult dialogResult = dialog.ShowDialog();
				if (dialogResult == DialogResult.OK)
				{
					Logger.Info("File Selected : "+ dialog.FileName);
					string apkPath = dialog.FileName;
					Logger.Info ("Console: Installing apk: {0}", apkPath);

					Thread apkInstall = new Thread(delegate()
					{
						Utils.CallApkInstaller(apkPath, false);
					});
					apkInstall.IsBackground = true;
					apkInstall.Start();
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed to get install apk. Error: " + e.ToString());
			}
		}

		private void HandleFolderClicked()
		{
			try
			{
				OpenFileDialog dialog = new OpenFileDialog();
				dialog.Filter = "All Files (*.*)|*.*";
				dialog.Multiselect = true;
				DialogResult dialogResult = dialog.ShowDialog();
				string sharedFolder = Common.Strings.SharedFolderDir;
				string sharedFolderName = Common.Strings.SharedFolderName;
				string origFilePath;
				string fileBaseName;
				string[] files = dialog.FileNames;
				if(dialogResult == DialogResult.OK)
				{
					Logger.Info("File Selected : "+ dialog.FileName);
					origFilePath 	= dialog.FileName;

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

				string	url	= String.Format("http://127.0.0.1:{0}/{1}",Common.VmCmdHandler.s_ServerPort, Common.Strings.SharePicUrl);
				foreach (string file in files)
				{
					fileBaseName = Path.GetFileName(file);
					string destFilePath = Path.Combine(sharedFolder, fileBaseName);
					string androidPath = "/mnt/sdcard/windows/" +
						sharedFolderName + "/" + fileBaseName;
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
						File.Copy(file, destFilePath);
				}
			}
			catch(Exception e)
			{
				Logger.Error("Failed to copy file. Error: " + e.ToString());
			}
		}

		private void HandleCopyClicked()
		{
			SendCommandToBstCmdProcessor("copyfromappplayer");

		}

		private void HandlePasteClicked()
		{
			SendCommandToBstCmdProcessor("pastetoappplayer");
		}

		private void HandleSoundClicked()
		{
			int buttonIndex = slstToolBarButtonOrder.IndexOf(EnumGameManagerToolBarButtonOrder.sound_on.ToString());
			string tag = "toolbar_sound_on";
			if (mIsSoundMuted)
			{
				if (!Utils.IsOSWinXP())
				{
					waveOutSetVolume(IntPtr.Zero, mSavedVolumeLevel);
					VolumeMixer.SetVolume(null, 50f);
				}

				TabBar.sTabBar.UnmuteFrontend();
			}
			else
			{
				if (!Utils.IsOSWinXP())
				{
					waveOutGetVolume(IntPtr.Zero, out mSavedVolumeLevel);
					waveOutSetVolume(IntPtr.Zero, 0);
					VolumeMixer.SetVolume(null, 0f);
				}

				TabBar.sTabBar.MuteFrontend();
				tag = "toolbar_sound_off";
			}

			mPBToolButtons[buttonIndex].Tag = tag;
			mPBToolButtons[buttonIndex].Image = mImagesDict[tag];
			GameManager.sGameManager.Activate();

			mIsSoundMuted = !mIsSoundMuted;
		}

		private void HandleHelpClicked()
		{
			RegistryKey urlKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			string url = (string)urlKey.GetValue(Common.Strings.GMSupportUrlKeyName,
					"http://bluestacks-cloud.appspot.com/static/support_page/index.html");

			GameManager.sGameManager.ShowWebPage("Help", url, null);
		}

		private void SendCommandToFrontend(string command)
		{
			Thread t = new Thread(delegate() {
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
			catch(Exception ex)
			{
				Logger.Error("An error occured. Err :" + ex.ToString());
			}
		}

	}

	public class GradientPanel : Panel
	{
		public Color mStartColor;
		public Color mEndColor;
		LinearGradientMode mGradientMode;

		public GradientPanel(Color startColor, Color endColor, LinearGradientMode mode)
		{
			mStartColor = startColor;
			mEndColor = endColor;
			mGradientMode = mode;
			this.ResizeRedraw = true;
		}

		protected override void OnPaintBackground(PaintEventArgs e)
		{
			try
			{
				using (var brush = new LinearGradientBrush(this.ClientRectangle,
						mStartColor, mEndColor, mGradientMode))
				{
					e.Graphics.FillRectangle(brush, this.ClientRectangle);
				}
			}
			catch (AccessViolationException ex)
			{
				// http://stackoverflow.com/questions/5510115/randomly-occuring-accessviolationexception-in-gdi
				// sometimes it throws access voilation exception so it is try catched
				Logger.Info(ex.ToString());
			}
		}

		protected override void OnScroll(ScrollEventArgs se)
		{
			this.Invalidate();
			base.OnScroll(se);
		}
	}
}
