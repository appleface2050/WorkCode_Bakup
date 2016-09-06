using System;
using System.Diagnostics;
using System.Net;
using System.Windows.Forms;
using System.Threading;
using System.ComponentModel;
using System.IO;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;

namespace BlueStacks.hyperDroid.Updater {
class Manager {
	public static void DoWorkflow() {
		DoWorkflow(false);
	}

	public static void DoWorkflow(bool userClicked) {
		s_UserClicked = userClicked;

		Locale.Strings.InitLocalization(null);

		if (UpdateNeeded())
		{
			DownloadAndInstall(Manifest.URL);
			return;
		}

		RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.RegUpdaterPath);
		key.DeleteValue("Status", false);
		string manifestURL 	= (String)key.GetValue(@"ManifestURL");
		Logger.Info("manifestURL = {0}", manifestURL);
		Manager.Start(manifestURL);
	}

	private static void Start(string url) {
		s_ManifestPath = String.Format(@"{0}\{1}", Common.Strings.BstUserDataDir, "manifest.ini");

		s_Client = new WebClient();

		s_Client.Headers.Add("User-Agent", "BlueStacks");
		string[] possibleManifests = Manager.PossibleManifestsHelper(url);
		string actualUrl = url;
		
		foreach(string url_man in possibleManifests)
		{
			actualUrl = String.Format("{0}?user_guid={1}", url_man, User.GUID);
			Logger.Info("Start fetching manifest {0}", actualUrl);

			try {
				s_Client.DownloadFile(new Uri(actualUrl), s_ManifestPath);
				ManifestDownloadComplete();
				return;
			} catch (Exception exc) {
				Logger.Error("DownloadFile failed: {0}", exc.ToString());
			}
		}

		Logger.Info("s_UserClicked = {0}", s_UserClicked.ToString());
		if (s_UserClicked)
		{
			NoUpdatesAvailable();
		}
	}

	private static string[] PossibleManifestsHelper(string url) {
		string current_OEM = Oem.Instance.OEM;
		string[] manifestUrls;

		int index = 0;
		if (Features.IsFeatureEnabled(Features.CHINA_CLOUD))
		{
			manifestUrls = new String[4];
			manifestUrls[index++] = url.Replace("cdn.bluestacks.com", "www.bluestacks.cn") + "_" + Oem.Instance.OEM + ".ini";
			manifestUrls[index++] = url.Replace("cdn.bluestacks.com", "www.bluestacks.cn") + ".ini";
		}
		else
		{
			manifestUrls = new String[2];
		}

		manifestUrls[index++] = url + "_" + Oem.Instance.OEM + ".ini";
		manifestUrls[index++] = url + ".ini";
		return manifestUrls;
	}



	private static void ManifestDownloadComplete()
	{
		Logger.Info("Manifest downloaded successfully to {0}", s_ManifestPath);
		IniFile iniFile = new IniFile(s_ManifestPath);
		Manifest.Version	= iniFile.GetValue("update", "version");
		Manifest.MD5		= iniFile.GetValue("update", "md5");
		Manifest.SHA1		= iniFile.GetValue("update", "sha1");
		Manifest.Size		= iniFile.GetValue("update", "size");
		Manifest.URL		= iniFile.GetValue("update", "url");
		Logger.Info("Manifest:\n\tversion = {0}\n\tmd5 = {1}\n\tsha1 = {2}\n\tsize = {3}\n\turl = {4}",
				Manifest.Version, Manifest.MD5, Manifest.SHA1, Manifest.Size, Manifest.URL);
		CheckAndInstallUpdate();
	}

	private static void CheckAndInstallUpdate()
	{
		if (UpdateNeeded())
		{
			DownloadAndInstall(Manifest.URL);
		}
		else if (s_UserClicked)
		{
			NoUpdatesAvailable();
		}
	}

	private static bool UpdateNeeded()
	{
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		System.Version currentVersion	= new System.Version((String)key.GetValue("Version"));
		if (Manifest.Version != null && Manifest.Version != "")
		{
			System.Version newVersion = new System.Version(Manifest.Version);
			if (newVersion > currentVersion)
			{
				Logger.Info("Update needed");
				return true;
			}
		}
		Logger.Info("Update not needed");
		return false;
	}

	private static void NoUpdatesAvailable()
	{
		Logger.Info("No updates available");
		String capt = BlueStacks.hyperDroid.Common.Oem.Instance.BlueStacksUpdaterTitle;
		String text = Locale.Strings.UPDATER_UTILITY_NO_UPDATE_TEXT;
		if(BlueStacks.hyperDroid.Common.Oem.Instance.IsHideMessageBoxIconInTaskBar)
		{
			MessageBox.Show(new Form(), text, capt, MessageBoxButtons.OK);
		}
		else
		{
			MessageBox.Show(text, capt, MessageBoxButtons.OK);
		}
	}

	private static void DownloadAndInstall(string url)
	{
		Logger.Info("Downloading from {0}", url);

		string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
		string setupDir = String.Format(@"{0}\BlueStacksSetup", programData);

		string manifestFileName = Path.GetFileName((new Uri(url)).LocalPath);

		string fileName		= Path.GetFileNameWithoutExtension(manifestFileName);
		string setupPath	= Path.Combine(setupDir, fileName);

		if (!Directory.Exists(setupDir))
			Directory.CreateDirectory(setupDir);

		RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.RegUpdaterPath);
		key.SetValue("Status", Locale.Strings.DownloadingUpdates);

		Logger.Info("Start downloading from {0}", url);

		if (File.Exists(setupPath))
		{
			AskToInstall(setupPath);
		}
		else
		{
			Thread dl = new Thread(delegate()
					{
					int nrWorkers	= 3;
					bool downloaded	= false;
					while (!downloaded)
					{
						SplitDownloader splitDl = new SplitDownloader(url,
										setupDir,
										Utils.UserAgent(User.GUID),
										nrWorkers);

						splitDl.Download(delegate(float percent)
							{
								key = Registry.LocalMachine.CreateSubKey(Common.Strings.RegUpdaterPath);
								key.SetValue("Status", String.Format("{0} {1}%",
										Locale.Strings.DownloadingUpdates, percent));
							},
							delegate(string filePath)
							{
								try
								{
									downloaded = true;
									AskToInstall(filePath);
								}
								catch (Exception ex)
								{
									Logger.Error("Exception in AskToInstall. " + ex.ToString());
									// Ignore
								}
							},
							delegate(Exception ex)
							{
								downloaded = false;
								Logger.Error("Download Not Complete: " + ex.ToString());
								Thread.Sleep(10000);
							});
					}
					});

			dl.IsBackground = true;
			dl.Start();
		}
	}

	private static void AskToInstall(string setupPath)
	{
		string md5 = Utils.GetMD5HashFromFile(setupPath);
		Logger.Info("md5 of downloaded file: " + md5);

		Logger.Info("New version ({0}) of BlueStacks is available", Manifest.Version);
		RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.RegUpdaterPath);
		key.SetValue("Status", Locale.Strings.InstallUpdates);

		string capt = Locale.Strings.UPDATER_UTILITY_ASK_TO_INSTALL_TITLE;
		string text = BlueStacks.hyperDroid.Common.Oem.Instance.UpdaterMessageBoxText;
		string installNowText = Locale.Strings.UPDATER_UTILITY_ASK_TO_INSTALL_NOW;
		string remindLaterText = Locale.Strings.UPDATER_UTILITY_ASK_TO_INSTALL_REMIND_LATER;

		DialogResult res = Common.UI.MessageBox.ShowMessageBox(
				capt, text, installNowText, remindLaterText,
				null);

		if (res == DialogResult.OK)
		{
			Logger.Info("The user clicked ok, ThinInstaller installation will begin");
			UpdateBlueStacks(setupPath);
		}
		else
		{
			Logger.Info("The user clicked on remind me later option");
		}
	}

	public static void UpdateBlueStacks(string setupPath)
	{
		/*
		 * This should ideally be handled by an updater binary
		 * which silently installs the msi
		 */
		Process.Start(setupPath);
	}

	private static string s_ManifestPath;
	private static WebClient s_Client;
	private static bool s_UserClicked;
}
}
