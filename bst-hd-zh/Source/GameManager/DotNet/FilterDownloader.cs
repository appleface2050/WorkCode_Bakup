using System;
using System.IO;
using Microsoft.Win32;
using System.Collections.Generic;
using System.Windows.Forms;
using System.Net;
using System.Threading;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using System.ComponentModel;

namespace BlueStacks.hyperDroid.GameManager
{
	public class FilterZipInfo
	{
		public string downloadPath;
		public int version;
		public string url;
		public bool deleteDir = false;
		public string deleteDirPath;
		public string appPkg;
	}

	public class FilterDownloader
	{
		private bool mIsDownloading = false;
		private bool mIsDownloaded = false;
		private bool mFreeMemoryOnClose = false;
		private string mCallBackFunction;
		private bool mIsFirstTimeDownload = false;
		private bool mSkipFolderNotAvailable = false;
		public bool mReParentOBS = false;
		private BackgroundWorker mBackgroundWorker;
		private IJSonObject mCloudJSONData;
		private IJSonObject mLocalJSONData;
		private List<FilterZipInfo> mFilterZipInfo;
		private int mDownloadCount = 0;
		private String mFilterThemesLocalPath;
		private String mFilterThemesCloudPath;
		private FilterDownloadProgress mFilterDownloadProgress;

		public static bool sUpdateLater = false;
		public static bool sStopBackgroundWorker = false;
		
		public FilterDownloader()
		{}

		public void IsFilterUpdateAvailable()
		{
			Logger.Info("FilterUpdateUtility: in IsFilterUpdateAvailable");
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			
			if (key == null)
			{
				RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath, true);
				baseKey.CreateSubKey("Filter");
				baseKey.Close();

				key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			}

			//adding by default Clash royale
			if (key.GetValue("FilterAvailableForApps", null) == null)
				key.SetValue("FilterAvailableForApps", "[\"com.supercell.clashroyale\", \"com.nianticlabs.pokemongo\"]", RegistryValueKind.String);
			string filterThemesCloudURL = (string) key.GetValue("FilterThemeUrl", null);
			key.Close();

			if (filterThemesCloudURL == null)
				filterThemesCloudURL = "http://cdn.bluestacks.com/public/btv/resources/filters/filterthemes.js";

			mFilterThemesCloudPath = Path.Combine(FilterUtility.GetFilterDir(), @"newfilterthemes.js");
			mFilterThemesLocalPath = Path.Combine(FilterUtility.GetFilterDir(), @"filterthemes.js");

			//if code dies in Extracting state filters will come in unstable state
			if (CheckStatus("Downloaded") || CheckStatus("Extracting"))
			{
				if (PopulateZipInfoList())
					StartUpdating("Downloaded");
				else
				{
					Logger.Info("FilterUpdateUtility: populatezip info returned false");
					SetUpdateStatus("PopulateZipError");
					if (mFilterDownloadProgress != null)
						SuccessfullyCloseUI();
					else
						FilterDownloader.FreeMemory();
				}
			}
			else if (!DownloadFilterThemeJson(filterThemesCloudURL, mFilterThemesCloudPath))
			{
				Logger.Info("FilterUpdateUtility: unable to download filter js");
				if (mFilterDownloadProgress != null)
					SuccessfullyCloseUI();
				else
					FilterDownloader.FreeMemory();
			}
			else
				StartUpdating("Downloading");
		}

		private bool CheckStatus(string status)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			string value = (string) key.GetValue("UpdateStatus", null);
			key.Close();

			if (value != null)
			{
				Logger.Info("FilterUpdateUtility: CheckStatus value {0}", value);
				if (value.Equals(status))
					return true;
			}
			else	
				Logger.Info("FilterUpdateUtility: CheckStatus value null");


			return false;
		}

		private static void FreeMemory()
		{
			lock (GameManager.sFilterDownloaderLock)
			{
				if (GameManager.sRunFilterDownloaderAgain)
				{
					GameManager.sRunFilterDownloaderAgain = false;
					GameManager.sGameManager.mFilterDownloader = null;			
					GameManager.sGameManager.CheckNewFiltersAvailable();
				}
				else
					GameManager.sGameManager.mFilterDownloader = null;			

			}
		}

		private void StartUpdating(string status)
		{
			Logger.Info("FilterUpdateUtility: in StartUpdating with arg {0}", status);
			
			JSonReader readjson = new JSonReader();
			string cloudFileJSONData = FilterUtility.GetDefaultConfig(mFilterThemesCloudPath);
			if (cloudFileJSONData == null)
			{
				FilterDownloader.FreeMemory();
				return;
			}
			mCloudJSONData = readjson.ReadAsJSonObject(cloudFileJSONData);
			
			if (status.Equals("Downloading"))
			{
				try
				{
					string localFileJSONData = null;
	
					if (File.Exists(mFilterThemesLocalPath))
					{
						localFileJSONData = FilterUtility.GetDefaultConfig(mFilterThemesLocalPath);

						mLocalJSONData = readjson.ReadAsJSonObject(localFileJSONData);

						int currentFilterVersion = int.Parse(mLocalJSONData["version"].StringValue);
						int nextFilterVersion = int.Parse(mCloudJSONData["version"].StringValue);
						Logger.Info("FilterUpdateUtility: currentFilterVersion: {0}, nextFilterVersion: {1}", currentFilterVersion, nextFilterVersion);
						//if (nextFilterVersion > currentFilterVersion)
							mIsDownloading = true;
						//else
						//{
						//	Logger.Info("FilterUpdateUtility: Same Version filter themes available");
						//	FilterDownloader.FreeMemory();
						//	return;
						//}
					}
					else
					{
						Logger.Info("FilterUpdateUtility: No local filterthemes.js exist");
						mIsDownloading = true;
						mIsFirstTimeDownload = true;
						mIsDownloaded = false;
					}	
				}
				catch (Exception ex)
				{
					Logger.Error("FilterUpdateUtility: Error in update filtertheme. Error: {0}", ex.ToString());
					SetUpdateStatus("Error");
					FilterDownloader.FreeMemory();
					return;
				}
			}
			else if (status.Equals("Downloaded"))
			{
				if (CheckStatus("Extracting"))
				{
					mSkipFolderNotAvailable = true;
					SetUpdateStatus("Extracting");
				}
				else
					SetUpdateStatus("Downloaded");

				mIsDownloading = false;
				mIsDownloaded = true;
			}
			else
			{
				FilterDownloader.FreeMemory();
				return;
			}

			mBackgroundWorker = new BackgroundWorker();
			mBackgroundWorker.WorkerSupportsCancellation = true;
			mBackgroundWorker.WorkerReportsProgress = true;

			mBackgroundWorker.DoWork += new DoWorkEventHandler(DoWork);
			//mBackgroundWorker.ProgressChanged += new ProgressChangedEventHandler(ProgressChanged);
			//mBackgroundWorker.RunWorkerCompleted += new RunWorkerCompletedEventHandler(RunWorkerCompleted);
			mBackgroundWorker.RunWorkerAsync();
		}

		private void DoWork(object sender, DoWorkEventArgs e)
		{
			BackgroundWorker worker = (BackgroundWorker) sender;
			
			if (mIsDownloading)
			{
				if (PopulateUpdateCount())
				{
					if (mFilterZipInfo.Count == 0)
					{
						Logger.Info("FilterUpdateUtility: Nothing is available for update");
						if (mFilterDownloadProgress != null)
							SuccessfullyCloseUI();
						else
							FilterDownloader.FreeMemory();
						return;
					}

					SetUpdateStatus("Downloading");
					if (DownloadAllUpdates() == 0)
					{
						mIsDownloaded = true;
						SavePopulateUpdateCount();
						SetUpdateStatus("Downloaded");
						CheckAndExtractAllFiles();
					}
					else
					{
						SetUpdateStatus("Error");
					}
				}
				else
				{
					SetUpdateStatus("Error");
				}
			}
			else if (mIsDownloaded)
			{
				CheckAndExtractAllFiles();
			}
		}

		public void SetUpdateStatus(string status)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			
			if (status.Equals("Error"))
			{
				if (mFilterDownloadProgress != null)
				{
					UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {
						MessageBox.Show(GameManager.sLocalizedString["DOWNLOAD_ERROR_TEXT"]);

						SuccessfullyCloseUI();
					});
				}
			}

			string currentStatus = (string) key.GetValue("UpdateStatus", null);
			if (status.Equals("PopulateZipError"))
				key.SetValue("UpdateStatus", "Error", RegistryValueKind.String);
			else if (status.Equals("Completed"))
				key.SetValue("UpdateStatus", status, RegistryValueKind.String);
			else if (currentStatus == null || (!currentStatus.Equals("Downloaded")
					&& !currentStatus.Equals("Extracting")))
				key.SetValue("UpdateStatus", status, RegistryValueKind.String);
			
			key.Close();

			if (status.Equals("Error"))
				FilterDownloader.FreeMemory();
		}

		public void ExecuteCallBack(bool callBackStatus)
		{
			UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {

				if (mCallBackFunction != null)
					GameManager.sGameManager.mStreamWindow.EvaluateJS(mCallBackFunction + "('"+ callBackStatus.ToString().ToLower() +"');");
				if (mReParentOBS)
					GameManager.sGameManager.mStreamWindow.ReParentOBSWindow();

				mFilterDownloadProgress = null;
				
				if (mFreeMemoryOnClose)
					FilterDownloader.FreeMemory();
			});
		}

		public void CheckAndExtractAllFiles()
		{
			if (StreamManager.sStreamManager != null &&
				StreamManager.sStreamManager.mIsObsRunning)
			{
				if (mFilterDownloadProgress != null)
				{
					UpdateUI("Downloaded");
					sStopBackgroundWorker = true;

					while(sStopBackgroundWorker)
					{
						Thread.Sleep(100);
					}
								
					if (!sUpdateLater)
						KillOBSAndExtractUpdates();
				}
				else
					FilterDownloader.FreeMemory();
			}
			else
			{
				if (ExtractAllUpdates() == 0)
					FinishUpdate();
				else
					SetUpdateStatus("Error");
			}
		}

		public void SavePopulateUpdateCount()
		{
			JSonWriter writer = new JSonWriter();
			writer.WriteArrayBegin();
			foreach (FilterZipInfo filterZipInfo in mFilterZipInfo)
			{
				writer.WriteObjectBegin();
				writer.WriteMember("version", filterZipInfo.version);
				writer.WriteMember("downloadPath", filterZipInfo.downloadPath);
				writer.WriteMember("url", filterZipInfo.url);
				writer.WriteMember("deleteDir", filterZipInfo.deleteDir);
				writer.WriteMember("deleteDirPath", filterZipInfo.deleteDirPath);
				writer.WriteMember("appPkg", filterZipInfo.appPkg);
				writer.WriteObjectEnd();
			}
			writer.WriteArrayEnd();

			if (mFilterZipInfo.Count > 0)
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
				key.SetValue("FilterZipList", writer.ToString(), RegistryValueKind.String);
				key.Close();
			}
		}

		private bool PopulateZipInfoList()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath);
			string filterZipList = (string) key.GetValue("FilterZipList", null);

			mFilterZipInfo = new List<FilterZipInfo>();
			if (filterZipList != null)
			{
				JSonReader readJson = new JSonReader();
				IJSonObject obj = readJson.ReadAsJSonObject(filterZipList);

				List<string> installedApps = GetInstalledAppsList();
				for (int i=0; i<obj.Length; i++)
				{
					FilterZipInfo filterZipInfo = new FilterZipInfo();
					if (!obj[i]["appPkg"].IsNull)
					{
						filterZipInfo.appPkg = obj[i]["appPkg"].StringValue;
						if (!installedApps.Contains(filterZipInfo.appPkg))
							continue;
					}

					if (!obj[i]["version"].IsNull)
						filterZipInfo.version = obj[i]["version"].Int32Value;
					if (!obj[i]["downloadPath"].IsNull)
						filterZipInfo.downloadPath = obj[i]["downloadPath"].StringValue;
					if (!obj[i]["url"].IsNull)
						filterZipInfo.url = obj[i]["url"].StringValue;
					if (!obj[i]["deleteDir"].IsNull)
						filterZipInfo.deleteDir = obj[i]["deleteDir"].BooleanValue;
					if (!obj[i]["deleteDirPath"].IsNull)
						filterZipInfo.deleteDirPath = obj[i]["deleteDirPath"].StringValue;

					mFilterZipInfo.Add(filterZipInfo);
				}
				return true;
			}
			key.Close();
			return false;
		}

		private void KillOBSAndExtractUpdates()
		{
			if (StreamManager.sStreamManager != null &&
					StreamManager.sStreamManager.mIsStreaming)
			{
				UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {
					DialogResult dialogResult = MessageBox.Show(GameManager.sLocalizedString["STOP_STREAMING_REQUIRED"],
						GameManager.sLocalizedString["FILTER_UPDATE_TITLE"], MessageBoxButtons.YesNo);
				
					if(dialogResult == DialogResult.Yes)
					{
						StreamManager.StopOBS();
						mCallBackFunction = null;
						mReParentOBS = false;

						if (ExtractAllUpdates() == 0)
							FinishUpdate();
						else
						{
							SetUpdateStatus("Error");
						}

						GameManager.sGameManager.ShowStreamWindow();
					}
					else
					{
						FilterDownloader.FreeMemory();
					}
				});
			}
			else
			{
				
				UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {
					
					StreamManager.StopOBS();
					
					mCallBackFunction = null;
					mReParentOBS = false;
					if (ExtractAllUpdates() == 0)
						FinishUpdate();
					else
					{
						SetUpdateStatus("Error");
					}
				
					GameManager.sGameManager.ShowStreamWindow();
				});
			}
		}

		private void FinishUpdate()
		{
			try
			{
				File.Copy(mFilterThemesCloudPath, mFilterThemesLocalPath, true);

				UpdateFilterRegistry();

				UpdateAppList();
				SetUpdateStatus("Completed");

				FilterUtility.UpdateSupportedPackages();
				foreach (FilterZipInfo filterZipInfo in mFilterZipInfo)
				{
					try
					{
						File.Delete(filterZipInfo.downloadPath);
					}
					catch (Exception ex)
					{
						Logger.Error("FilterUpdateUtility: Error {0}", ex);
					}
				}

			}
			catch (Exception ex)
			{
				SetUpdateStatus("Error");
				Logger.Error("FilterUpdateUtility: Error {0}", ex.ToString());
				return;
			}

			if (mFilterDownloadProgress != null)
			{
				SuccessfullyCloseUI();
			}
			else
				FilterDownloader.FreeMemory();
		}

		private void SuccessfullyCloseUI()
		{
			UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {

				if (mCallBackFunction != null)
					GameManager.sGameManager.mStreamWindow.EvaluateJS(mCallBackFunction + "('true');");
				mCallBackFunction = null;
				mFreeMemoryOnClose = true;
				mFilterDownloadProgress.Close();
			});
		}

		private void UpdateAppList()
		{
			RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			
			IJSonObject obj = mCloudJSONData["supported_packages"];
			
			List<string> installedApps = GetInstalledAppsList();

			JSonWriter writer = new JSonWriter();
			writer.WriteArrayBegin();
			for (int i=0; i<obj.Length; i++)
			{
				if (installedApps.Contains(obj[i].ToString()))
					writer.WriteValue(obj[i].ToString());
			}
			writer.WriteArrayEnd();

			baseKey.SetValue("FilterAvailableForApps", writer.ToString(), RegistryValueKind.String);
			baseKey.Close();
		}

		public static void RemoveFilterAvailableForApp(string appPkg)
		{
			RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			string appList = (string) baseKey.GetValue("FilterAvailableForApps", null);
			if (appList == null || appPkg.Equals("com.supercell.clashroyale")
					|| appPkg.Equals("com.nianticlabs.pokemongo"))
				return;

			JSonReader jsonRead = new JSonReader();
			IJSonObject obj = jsonRead.ReadAsJSonObject(appList);
			
			JSonWriter writer = new JSonWriter();
			writer.WriteArrayBegin();
			bool deleteDir = false;
			for (int i=0; i<obj.Length; i++)
			{
				if (!obj[i].ToString().Equals(appPkg))
					writer.WriteValue(obj[i].ToString());
				else
				{
					deleteDir = true;
				}
			}
			writer.WriteArrayEnd();

			baseKey.SetValue("FilterAvailableForApps", writer.ToString(), RegistryValueKind.String);

			if (deleteDir)
			{
				try
				{
					Directory.Delete(FilterUtility.GetFilterDir() + @"\theme\" + appPkg, true);
				}
				catch (Exception ex)
				{
					Logger.Error("FilterUpdateUtility: {0}", ex.ToString());
				}

				try
				{
					if (Array.IndexOf(baseKey.GetSubKeyNames(), appPkg) > -1)
						baseKey.DeleteSubKey(appPkg);
				}
				catch (Exception ex)
				{
					Logger.Error("FilterUpdateUtility: {0}", ex.ToString());
				}
			}
			baseKey.Close();
		}

		private List<string> GetInstalledAppsList()
		{
			List<string> installedApps = new List<string>();

			GMAppInfo[] gmAppInfoList = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS).GetAppInfoList();
			for (int i=0; i<gmAppInfoList.Length; i++)
			{
				if (gmAppInfoList[i] != null && gmAppInfoList[i].package != null)
					installedApps.Add(gmAppInfoList[i].package);
			}

			if (!installedApps.Contains("com.supercell.clashroyale"))
				installedApps.Add("com.supercell.clashroyale");

			if (!installedApps.Contains("com.nianticlabs.pokemongo"))
				installedApps.Add("com.nianticlabs.pokemongo");

			return installedApps;
		}

		public void ShowMessageBox(string data)
		{
			UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {
				MessageBox.Show(data);
				});
		}

		private bool PopulateUpdateCount()
		{
			try
			{
				mFilterZipInfo = new List<FilterZipInfo>();
				IJSonObject cloudthemes = mCloudJSONData["themes"];

				IJSonObject localthemes = null;
				if (mLocalJSONData != null)
					localthemes = mLocalJSONData["themes"];
	
				//for home dir
				bool shouldUpdate = false;
				if (mIsFirstTimeDownload)
				{
					shouldUpdate = true;
				}
				else
				{
					int cloudVersion = mCloudJSONData["home"]["version"].Int32Value;
					int localVersion = mLocalJSONData["home"]["version"].Int32Value;

					if (cloudVersion > localVersion)
						shouldUpdate = true;
				}
				if (shouldUpdate)
				{
					FilterZipInfo zip = new FilterZipInfo();
					zip.version = mCloudJSONData["home"]["version"].Int32Value; 
					zip.url =  mCloudJSONData["home"]["zip_url"].StringValue;
					zip.downloadPath = FilterUtility.GetFilterDir() + @"\" + mCloudJSONData["home"]["dir_name"] + "_V" + zip.version.ToString() + ".zip";
					mFilterZipInfo.Add(zip);
				}

				foreach (string appPkg in GetInstalledAppsList())
				{
					Logger.Info("FilterUpdateUtility: installed package {0}", appPkg);
					if (cloudthemes.Contains(appPkg))
					{
						string pkgDir = FilterUtility.GetFilterDir() + @"\theme\" + appPkg;
						if (!Directory.Exists(pkgDir))
							Directory.CreateDirectory(pkgDir);

						foreach (string theme in cloudthemes[appPkg].Names)
						{
							Logger.Info("FilterUpdateUtility: {0} : {1}", appPkg, theme);
							shouldUpdate = false;
							if (mIsFirstTimeDownload)
								shouldUpdate = true;
							else if (localthemes.Contains(appPkg) && FilterUtility.IsFilterAvailableForThisApp(appPkg))
							{
								int cloudVersion = cloudthemes[appPkg][theme]["version"].Int32Value;
								if (localthemes[appPkg].Contains(theme))
								{
									int localVersion = localthemes[appPkg][theme]["version"].Int32Value;
									if (cloudVersion > localVersion)
										shouldUpdate = true;
								}
								else
									shouldUpdate = true;

							}
							else
							{
								shouldUpdate = true;
							}

							if (shouldUpdate)
							{
								FilterZipInfo zip = new FilterZipInfo();
								zip.version = cloudthemes[appPkg][theme]["version"].Int32Value; 
								zip.appPkg = appPkg;
								string dir = FilterUtility.GetFilterDir() + @"\" + "theme" + @"\" + appPkg + @"\";
								if (cloudthemes[appPkg][theme].Contains("delete"))
								{
									zip.deleteDir = true;
									zip.deleteDirPath = dir + cloudthemes[appPkg][theme]["dir_name"];
								}
								else
								{
									zip.url =  cloudthemes[appPkg][theme]["zip_url"].StringValue;
									zip.downloadPath = dir + cloudthemes[appPkg][theme]["dir_name"] + "_V" + zip.version.ToString() + ".zip";
								}
								mFilterZipInfo.Add(zip);
							}

						}
					}	
				}
			}
			catch(Exception ex)
			{
				Logger.Error(ex.ToString());
				return false;
			}
			return true;
		}	

		public void LaunchUI(string callBackFunction)
		{
			UIHelper.RunOnUIThread(GameManager.sGameManager, delegate() {
				mCallBackFunction = callBackFunction;

				if (mFilterDownloadProgress != null)
					return;

				mFilterDownloadProgress = new FilterDownloadProgress();
				if (mIsDownloaded)
					UpdateUI("Downloaded");
				mFilterDownloadProgress.ShowDialog();
			});
		}

		private void UpdateUI(string status)
		{
			if (mFilterDownloadProgress != null)
			{
				string text;
				if (status.Equals("Downloading"))
				{
					if (mDownloadCount > mFilterZipInfo.Count)
						text = GameManager.sLocalizedString["DOWNLOADED_TEXT"];
					else
						text = String.Format(GameManager.sLocalizedString["DOWNLOADING_TEXT"], mDownloadCount, mFilterZipInfo.Count);
				}
				else if (status.Equals("Downloaded"))
				{
					text = GameManager.sLocalizedString["DOWNLOADED_TEXT"];
					mFilterDownloadProgress.EnableButtons();
				}
				else
					text = GameManager.sLocalizedString["APPLYING_TEXT"];
				mFilterDownloadProgress.UpdateProgress(text);
			}
		}

		private int DownloadAllUpdates()
		{
			int exitCode = 0;
			mDownloadCount = 1;
			foreach (FilterZipInfo filterZipInfo in mFilterZipInfo)
			{
				if (filterZipInfo.deleteDir)
				{
					mDownloadCount ++;
					UpdateUI("Downloading");
					continue;
				}

				if (File.Exists(filterZipInfo.downloadPath))
				{
					mDownloadCount ++;
					UpdateUI("Downloading");
				}
				else
				{
					Downloader downloader = new Downloader(1, filterZipInfo.url, filterZipInfo.downloadPath);
					downloader.Download(
						delegate(int percent)
						{
							UpdateUI("Downloading");
							Logger.Info("FilterUpdateUtility: percent: " + percent);
						},
						delegate(String file)
						{
							Logger.Info("FilterUpdateUtility: file downloaded to: " + file);
							mDownloadCount ++;
							UpdateUI("Downloading");
						},
						delegate(Exception ex)
						{
							Logger.Error("FilterUpdateUtility: {0}", ex.ToString());
							exitCode = -1;
						}
					);
				}
			}
			return exitCode;
		}

		private int ExtractAllUpdates()
		{
			UpdateUI("Extracting");
			SetUpdateStatus("Extracting");
			int exitCode = 0;
			if (StreamManager.sStreamManager != null &&
				StreamManager.sStreamManager.mIsObsRunning)
			{
				exitCode = -1;
				Logger.Info("FilterUpdateUtility: Unable to update obs already running");
			}
			else
			{
				foreach (FilterZipInfo filterZipInfo in mFilterZipInfo)
				{	
					if (filterZipInfo.deleteDir)
					{
						try
						{
							if (Directory.Exists(filterZipInfo.deleteDirPath))
								Directory.Delete(filterZipInfo.deleteDirPath, true);
						}
						catch(Exception ex)
						{
							Logger.Error("FilterUpdateUtility: {0}", ex.ToString());
						}
					}
					else
					{
						if (mSkipFolderNotAvailable)
						{
							if (!File.Exists(filterZipInfo.downloadPath))
								continue;
						}

						string targetDir = Path.GetDirectoryName(filterZipInfo.downloadPath);
						exitCode = Utils.Unzip(filterZipInfo.downloadPath, targetDir);
						if (exitCode != 0)
							break;
					}
				}
			}
			return exitCode;
		}

		private bool DownloadFilterThemeJson(string cloudThemeURL, string cloudThemeFilePath)
		{
			Logger.Info("FilterUpdateUtility: Downloading file at location {0}", cloudThemeFilePath);
			WebClient cl = new WebClient();
			try
			{
				cl.DownloadFile(cloudThemeURL, cloudThemeFilePath);
				return true;
			}
			catch (Exception e)
			{
				Logger.Error("FilterUpdateUtility: Failed to Download file on url. Error: {0}", e.ToString());
			}
			return false;
		}

		public void UpdateFilterRegistry()
		{
			IJSonObject cloudThemesJson = (IJSonObject) mCloudJSONData["themes"];

			RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath, true);
			if (baseKey == null)
				return;

			foreach (string appPkgRegKeyName in baseKey.GetSubKeyNames())
			{
				bool found = false;
				foreach (string appPkg in cloudThemesJson.Names)
				{
					if (appPkg.Equals(appPkgRegKeyName))
						found = true;
				}

				if (found)
				{
					Logger.Info("FilterUpdateUtility: updating reg for appPkg {0}", appPkgRegKeyName);
					
					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GMFilterPath + @"\" + appPkgRegKeyName, true);
					if (key == null)
						continue;
					
					string currentTheme = (string) key.GetValue("CurrentTheme", null);
					foreach (string regKeyName in key.GetValueNames())
					{
						if (!cloudThemesJson[appPkgRegKeyName].Contains(regKeyName))
							continue;

						foreach (string theme in cloudThemesJson[appPkgRegKeyName].Names)
						{
							if (theme.ToLower().Equals(regKeyName.ToLower()))
							{
								FilterThemeConfig oldFilterThemeConfig = new FilterThemeConfig((string) key.GetValue(theme));
								FilterThemeConfig newFilterThemeConfig = new FilterThemeConfig(cloudThemesJson[appPkgRegKeyName][theme]["initial_config"].ToString());


								newFilterThemeConfig.mFilterThemeSettings.mIsWebCamOn = oldFilterThemeConfig.mFilterThemeSettings.mIsWebCamOn;
								newFilterThemeConfig.mFilterThemeSettings.mIsChatOn = oldFilterThemeConfig.mFilterThemeSettings.mIsChatOn;
								newFilterThemeConfig.mFilterThemeSettings.mIsAnimate = oldFilterThemeConfig.mFilterThemeSettings.mIsAnimate;
								key.SetValue(theme, newFilterThemeConfig.ToJsonString(), RegistryValueKind.String);
							}
						}
					}

					if (currentTheme != null)
					{
						if (!cloudThemesJson[appPkgRegKeyName].Contains(currentTheme) ||
								cloudThemesJson[appPkgRegKeyName][currentTheme].Contains("delete"))
						{
							key.DeleteValue("CurrentTheme");
						}
					}

					key.Close();
				}
				else
				{
					baseKey.DeleteSubKey(appPkgRegKeyName);
				}
			}
			
			baseKey.Close();
		}
	}

}	
