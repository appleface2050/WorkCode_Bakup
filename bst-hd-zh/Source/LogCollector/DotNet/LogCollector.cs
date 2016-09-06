using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Management;
using System.Net;
using System.Net.Security;
using System.Security.AccessControl;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using System.Globalization;
using System.Collections;
using System.Text;
using System.Timers;
using System.ServiceProcess;

using BlueStacks.hyperDroid.Cloud.Services;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;
using System.Runtime.InteropServices;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.LogCollector
{

	public class LogCollector : Form
	{

		private Button mStartControl;
		private TextBox mEmailControl;
		private Label mEmailLabelControl;
		private ComboBox mCategoryControl;
		private ComboBox mSubCategoryControl;
		private PictureBox mLoadingControlBox;
		private Bitmap mLoadingImage;
		private Label mAppNameLabelControl;
		private ComboBox mAppNameControlDropDown;
		private TextBox mAppNameTextBox;
		private TextBox mDescriptionControl;
		private Label mDescriptionLabelControl;
		private Label mRPCProgressBarLabelControl;
		private ProgressBar mRPCProgressBarControl;
		private static Hashtable sStringConversions;
		private static Dictionary<string, string> sCategoryShowDropDownMapping;
		public ToolTip toolTipDropDown = new ToolTip();

		private BackgroundWorker mCategoryDownloader;

		static private Mutex sLogCollectorRunning;
		static private string[] sProblemCategories;
		static private Dictionary<string, Dictionary<string, string>> sCategorySubCategoryMapping;
		static private Dictionary<string, Dictionary<string, string>> sCategorySubCategoryMappingWithShowDropdown;
		static private string sRootFolderUrl = Common.Strings.CDNAppSettingsUrl;
		static private string sFilePath = Path.Combine(Path.GetTempPath(), "ProblemCategories.txt");
		static private LogCollector sLogCollector;

		private const string LOADING_IMAGE_LOGCOLLECTOR = "loading_logcollector.gif";

		private static void InitExceptionHandlers()
		{
			Application.ThreadException += delegate(Object obj,
					System.Threading.ThreadExceptionEventArgs evt)
			{
				Logger.Error("LogCollector: Unhandled Exception:");
				Logger.Error(evt.Exception.ToString());

				Environment.Exit(1);
			};

			Application.SetUnhandledExceptionMode(
					UnhandledExceptionMode.CatchException);

			AppDomain.CurrentDomain.UnhandledException += delegate(
					Object obj, UnhandledExceptionEventArgs evt)
			{
				Logger.Error("LogCollector: Unhandled Exception:");
				Logger.Error(evt.ExceptionObject.ToString());

				Environment.Exit(1);
			};
		}

		public static void Main(String[] args)
		{
			Logger.InitUserLog();
			InitExceptionHandlers();
			Utils.LogParentProcessDetails();
			Locale.Strings.InitLocalization(null);
			Application.EnableVisualStyles();
			ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

			bool showUI = false;

			for (int i = 0; i < args.Length; i++)
			{
				Logger.Info("args[{0}] = {1}", i, args[i]);
			}

			if (Common.Utils.IsAlreadyRunning(Common.Strings.LogCollectorLockName, out sLogCollectorRunning))
			{
				bool isSilent = false;
				Logger.Info("LogCollector already running.");
#if SILENT
				return;
#else
				for (int i = 0; i < args.Length; i++)
				{
					if (String.Compare(args[i], "-boot", true) == 0 ||
							String.Compare(args[i], "-apk", true) == 0 ||
							String.Compare(args[i], "-d", true) == 0 ||
							String.Compare(args[i], "-thin", true) == 0 ||
							String.Compare(args[i], "-silent", true) == 0)
					{
						isSilent = true;
					}
				}
				if (isSilent == false)
				{
					MessageBox.Show(Locale.Strings.LOGCOLLECTOR_RUNNING_TEXT,
							Oem.Instance.BluestacksLogCollectorText, MessageBoxButtons.OK);
				}
				return;
#endif
			}

			for (int i = 0; i < args.Length; i++)
			{
				/*
				 * This menu is for selective collection of logs.
				 */
				if (String.Compare(args[i], "-extra", true) == 0)
				{
					CollectLogs.s_StartLogcat = true;
					break;
				}
				else if (String.Compare(args[i], "-silent", true) == 0)
				{
					CollectLogs.s_SilentCollector = true;
					bool uploadZip = true;
					string dest = Path.GetTempPath();
					CollectLogs logCollector = new CollectLogs(dest, uploadZip);
					logCollector.StartSilentLogCollection(showUI);
					return;
				}
				else if (String.Compare(args[i], "-thin", true) == 0)
				{
					CollectLogs.s_ThinCollector = true;
					bool uploadZip = true;
					string dest = Path.GetTempPath();
					CollectLogs logCollector = new CollectLogs(dest, uploadZip);
					logCollector.StartSilentLogCollection(showUI);
					return;
				}
				else if (String.Compare(args[i], "-boot", true) == 0)
				{
					CollectLogs.s_BootFailureLogs = true;
					bool uploadZip = true;

					string errorReason = "Generic", errorCode = "-1"; ;
					if (args.Length > (i + 2))
					{
						errorReason = args[i + 1];
						errorCode = args[i + 2];
					}

					string dest = Path.GetTempPath();
					CollectLogs logCollector = new CollectLogs(dest, uploadZip, errorReason, errorCode);
					logCollector.StartSilentLogCollection(showUI);
					return;
				}
				else if (String.Compare(args[i], "-startwithparam", true) == 0)
				{
					if (args.Length > i + 1)
					{
						string mail = "", category = "", appName = "", desc = "", subCategory="";
						string param = args[i + 1];
						string[] list = param.Split('&');
						if (list.Length > 0)
							mail = list[0];
						if (list.Length > 1)
							category = list[1];
						if (list.Length > 2)
							appName = list[2];
						if (list.Length > 3)
							desc = list[3];
						if (list.Length > 4)
							subCategory = list[4];

						CollectLogs logCollector = new CollectLogs(mail, category, appName, desc, subCategory);

						Application.Run(logCollector);
					}

					return;
				}
				else if (String.Compare(args[i], "-apk", true) == 0)
				{
					bool uploadZip = true;
					CollectLogs.s_ApkInstallFailureLogCollector = true;
					string errorReason = "Apk-Generic", errorCode = "-1"; ;
					if (args.Length >= i + 3)
					{
						errorCode = args[i + 1];
						errorReason = args[i + 2];
						CollectLogs.s_InstallFailedApkName = args[i + 3];
					}
					string dest = Path.GetTempPath();
					CollectLogs logCollector = new CollectLogs(dest, uploadZip, errorReason, errorCode);
					logCollector.StartSilentLogCollection(showUI);
					return;
				}
				else if (String.Equals(args[i], "-ReportCrashLogs"))
				{
					bool uploadZip = true;
					string dest = Path.GetTempPath();
					string errorCode = "-1";
					string errorReason = args[i + 1];
					CollectLogs.s_CrashLogs = true;
					string errorDetails = args[i + 2];
					CollectLogs logCollector = new CollectLogs(dest, uploadZip, errorReason, errorDetails, errorCode);
					logCollector.StartSilentLogCollection(showUI);
					return;

				}
				// s_StartLogCat s_ThinCollector and s_ApkInstallFailureLogCollector cannot be true altogether for now. 
			}

#if SILENT
			bool silentUploadZip = true;
			string destination = Path.GetTempPath();
			CollectLogs silentLogCollector = new CollectLogs(destination, silentUploadZip);
			silentLogCollector.StartSilentLogCollection(showUI);
#else
			if (args.Length > 0)
			{
				int dPos = -1;
				for (int i = 0; i < args.Length; i++)
				{
					if (String.Compare(args[i], "-d", true) == 0)
					{
						dPos = i;
					}
				}

				if (dPos != -1)
				{
					bool uploadZip = false;
					string dest = Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
					if ((args.Length > (dPos + 1)) && String.Compare(args[dPos + 1], "-extra", true) != 0)
					{
						dest = args[dPos + 1];
					}
					if (!Directory.Exists(dest))
					{
						Directory.CreateDirectory(dest);
					}
					CollectLogs logCollector = new CollectLogs(dest, uploadZip);
					logCollector.StartSilentLogCollection(showUI);
				}
				else
				{
					Application.Run(new LogCollector());
				}
			}
			else
			{
				Application.Run(new LogCollector());
			}
#endif
		}

		private void DownloadCategories(Object sender, DoWorkEventArgs doWorkEventArgs)
		{
			Logger.Info("In method DownCategories");
			SetProblemCategories();

			if (sProblemCategories != null)
			{
				foreach (string problem in sProblemCategories)
					Logger.Info(problem);
				AddCategories();
			}
		}

		private void ShowCategories(Object sender, RunWorkerCompletedEventArgs workCompletedEventArgs)
		{
			mCategoryControl.Visible = true;
			mLoadingControlBox.Visible = false;
		}

		private void ShowSubCategories(Object sender, RunWorkerCompletedEventArgs workCompletedEventArgs)
		{
			mSubCategoryControl.Visible = true;
		}

		private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
		{
			return true;
		}

		private static void SetProblemCategories()
		{
			Logger.Info("In method SetProblemCategories");
			WebClient webClient = new WebClient();
			sStringConversions = new Hashtable();
			sCategoryShowDropDownMapping = new Dictionary<string,string>();
			string[] possibleFileUrls = PossibleCategoryFilesHelper(sRootFolderUrl);
			foreach (string url in possibleFileUrls)
			{
				Logger.Info("Start fetching file {0}", url);

				try
				{
					webClient.DownloadFile(new Uri(url), sFilePath);
					break;
				}
				catch (Exception exc)
				{
					Logger.Error(string.Format("sDownloadFile failed: {0}", exc.ToString()));
				}
			}
			if (!File.Exists(sFilePath))
			{
				Logger.Info("Unable to download problem categories");
			}
			SetConversions();
		}

		private static string[] PossibleCategoryFilesHelper(string url)
		{

			Logger.Info("In method PossibleCategoryFilesHelper");
			string current_Locale = CultureInfo.CurrentCulture.Name.ToLower();
			string[] fileUrls = new string[6];
			fileUrls[5] = url + "ReportProblemCategories" + ".Json";
			fileUrls[4] = url + "ReportProblemCategories_" + Oem.Instance.OEM + ".Json";
			fileUrls[3] = url + "ReportProblemCategories_" + current_Locale + ".Json";
			fileUrls[2] = url + "ReportProblemCategories_" + Version.STRING + ".Json";
			fileUrls[1] = url + "ReportProblemCategories_" + Version.STRING + "_" + Oem.Instance.OEM + ".Json";
			fileUrls[0] = url + "ReportProblemCategories_" + Version.STRING + "_" + current_Locale + ".Json";
			return fileUrls;
		}

		private static void SetDefaultCategories()
		{
			Logger.Info("In method SetDefaultCategories");
			string[] problems = Locale.Strings.LOGCOLLECTOR_PROBLEMS.Split(',');
			sProblemCategories = new String[problems.Length];

			for (int i = 0; i < problems.Length; i++)
			{
				if (problems[i].IndexOf(":") != -1)
				{
					sProblemCategories[i] = problems[i].Trim().Split(':')[1].Trim();
					sStringConversions.Add(problems[i].Trim().Split(':')[1].Trim(),
							problems[i].Trim().Split(':')[0].Trim());
				}
			}
		}

		private static void SetDefaultCategoriesFromJson()
		{
			Logger.Info("In method SetDefaultCategoriesFromJson");
			ProblemCategory problemCategory = Locale.Strings.GetLocalizationForProblemCategory();
			if (problemCategory != null && problemCategory.category != null && problemCategory.category.Count > 0)
			{
				List<Category> lsCategory = new List<Category>(problemCategory.category);
				sProblemCategories = new String[lsCategory.Count];
				sCategorySubCategoryMapping = new Dictionary<string,Dictionary<string,string>>();
				sCategorySubCategoryMappingWithShowDropdown = new Dictionary<string, Dictionary<string, string>>();
				for (int i = 0; i < lsCategory.Count; i++)
				{
					sProblemCategories[i] = lsCategory[i].categoryValue;
					sStringConversions.Add(lsCategory[i].categoryValue,
							lsCategory[i].categoryId);
					sCategoryShowDropDownMapping.Add(lsCategory[i].categoryId, lsCategory[i].showdropdown);
					if (lsCategory[i].subcategory != null && lsCategory[i].subcategory.Count > 0)
					{
						Dictionary<string, string> subCategoryIdValueMapping = new Dictionary<string, string>();
						Dictionary<string, string> subCategoryIdShowDropdownMapping = new Dictionary<string, string>();
						List<Subcategory> lsSubCategory = new List<Subcategory>(lsCategory[i].subcategory);
						for (int countSubCategory = 0; countSubCategory < lsSubCategory.Count; countSubCategory++)
						{
							subCategoryIdValueMapping.Add(lsSubCategory[countSubCategory].subcategoryId, lsSubCategory[countSubCategory].subcategoryValue);
							subCategoryIdShowDropdownMapping.Add(lsSubCategory[countSubCategory].subcategoryId, lsSubCategory[countSubCategory].showdropdown);
						}
						sCategorySubCategoryMapping.Add(lsCategory[i].categoryId, subCategoryIdValueMapping);
						sCategorySubCategoryMappingWithShowDropdown.Add(lsCategory[i].categoryId, subCategoryIdShowDropdownMapping);
					}
				}
			}
		}

		private static void SetConversions()
		{
			try
			{
				if (!File.Exists(sFilePath))
				{
					SetDefaultCategoriesFromJson();
					return;
				}
				//string[] fileLines = File.ReadAllLines(sFilePath, Encoding.UTF8);
				string jsonString = File.ReadAllText(sFilePath);
				Logger.Info("Downloaded Json: " + jsonString);
				JSonReader json = new JSonReader();
				IJSonObject input = json.ReadAsJSonObject(jsonString);
				int noOfCategories = 0;
				foreach (KeyValuePair<string, IJSonObject> categories in input.ObjectItems)
				{
					string key = categories.Key;
					noOfCategories = categories.Value.Length;
					if (noOfCategories == 0)
					{
						sProblemCategories = null;
						return;
					}
					else
					{
						sProblemCategories = new string[noOfCategories];
						sCategorySubCategoryMapping = new Dictionary<string, Dictionary<string, string>>();
						sCategorySubCategoryMappingWithShowDropdown = new Dictionary<string, Dictionary<string, string>>();
					}
					for (int i = 0; i < categories.Value.Length; i++)
					{
						IJSonObject objCategory = categories.Value[i];
						string categoryId = objCategory["id"].StringValue;
						string categoryValue = objCategory["value"].StringValue;
						if (objCategory.Contains("showdropdown"))
						{
							string categoryShowDropdown = objCategory["showdropdown"].StringValue;
							sCategoryShowDropDownMapping.Add(categoryId, categoryShowDropdown);                            
						}

						sStringConversions.Add(categoryValue, categoryId);
						sProblemCategories[i] = categoryValue;
						Dictionary<string, string> subCategoryIdValueMapping = new Dictionary<string, string>();
						Dictionary<string, string> subCategoryIdShowDropdownMapping = new Dictionary<string, string>();
						if (objCategory.Contains("subcategory"))
						{
							IJSonObject subCatList = objCategory["subcategory"];
							for (int countSubCategory = 0; countSubCategory < subCatList.Length; countSubCategory++)
							{
								IJSonObject subCatObj = subCatList[countSubCategory];
								string subCatId = subCatObj["id"].StringValue;
								string subCatVal = subCatObj["value"].StringValue;
								if (subCatObj.Contains("showdropdown"))
								{
									string subCatShowDropdown = subCatObj["showdropdown"].StringValue;
									subCategoryIdShowDropdownMapping.Add(subCatId, subCatShowDropdown);
								}
								subCategoryIdValueMapping.Add(subCatId, subCatVal);                                
							}
						}
						sCategorySubCategoryMapping.Add(categoryId, subCategoryIdValueMapping);
						sCategorySubCategoryMappingWithShowDropdown.Add(categoryId, subCategoryIdShowDropdownMapping);
					}
				}

				//sProblemCategories = new string[fileLines.Length];
				//for (int i = 0; i < fileLines.Length; i++)
				//{
				//    if ((fileLines[i].Trim().Length > 0) && (fileLines[i].IndexOf("=") != -1))
				//    {
				//        sStringConversions.Add(fileLines[i].Split('=')[0], fileLines[i].Split('=')[1]);
				//        sProblemCategories[i] = fileLines[i].Split('=')[0];
				//    }
				//}
			}
			catch (Exception e)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
			}

			if (sStringConversions.Count == 0)
				sProblemCategories = null;

		}

		private void ShowProgress(string text)
		{
			mStartControl.Hide();
			mEmailControl.Hide();
			mEmailLabelControl.Hide();
			mCategoryControl.Hide();
			mSubCategoryControl.Hide();
			mLoadingControlBox.Hide();
			mDescriptionControl.Hide();
			mDescriptionLabelControl.Hide();
			mRPCProgressBarLabelControl.Visible = true;
			mRPCProgressBarLabelControl.Text = text;
			mRPCProgressBarControl.Visible = true;
			this.ClientSize = new Size(300, 165);
		}

		private void DoSubCategoryRelatedTask(object sender, System.EventArgs e)
		{
			try
			{
				Logger.Info("In method DoSubCategoryRelatedTask");
				string category = mCategoryControl.SelectedItem.ToString();
				category = (String)sStringConversions[category];
				string showCategoryDropdown = (String)sCategoryShowDropDownMapping[category];
				string subCategory = mSubCategoryControl.SelectedItem.ToString();
				if(string.IsNullOrEmpty(subCategory) || mSubCategoryControl.SelectedIndex < 1)
				{
					return;
				}
				if (mCategoryControl.SelectedIndex > 0 && (string.IsNullOrEmpty(showCategoryDropdown) || showCategoryDropdown == "0"))
				{
					Dictionary<string, string> subCategoryIdValueMapping = new Dictionary<string, string>();
					subCategoryIdValueMapping = sCategorySubCategoryMapping[category];
					subCategory = FindKey(subCategory, subCategoryIdValueMapping);
					Dictionary<string, string> subCategoryIdShowDropdownMapping = new Dictionary<string, string>();
					subCategoryIdShowDropdownMapping = sCategorySubCategoryMappingWithShowDropdown[category];

					string showDropdown = (String)subCategoryIdShowDropdownMapping[subCategory];
					if (!string.IsNullOrEmpty(showDropdown) && showDropdown == "1")
					{
						AbleDisableAppName(true);
					}
					else
					{
						AbleDisableAppName(false);
					}
				}

				RunTroubleShooter(subCategory);
			}
			catch (Exception ex)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", ex.ToString()));            
			}
		}

		private void RunTroubleShooter(string Category)
		{
			Logger.Info("In method RunTroubleShooter");
			if (Category.Contains("RPCError"))
			{
				Logger.Info("RPC Error detected");
				DialogResult result = MessageBox.Show(Locale.Strings.TROUBLESHOOTER_TEXT,
						Locale.Strings.RPC_FORM_TEXT, MessageBoxButtons.OKCancel);

				if (result == DialogResult.OK)
				{
					ShowProgress(Locale.Strings.PROGRESS_TEXT);

					RunTroubleShooterExe("HD-GuestCommandRunner.exe",
							"",
							Locale.Strings.WORK_DONE_TEXT,
							Locale.Strings.RPC_FORM_TEXT);
				}
			}
			else if (Category.Contains("StuckOnLoading"))
			{
				Logger.Info("Stuck on loading detected");
				DialogResult result = MessageBox.Show(Locale.Strings.TROUBLESHOOTER_TEXT,
						Locale.Strings.STUCK_AT_INITIALIZING_FORM_TEXT, MessageBoxButtons.OKCancel);

				if (result == DialogResult.OK)
				{

					sLogCollector.Hide();
					RunTroubleShooterExe("HD-Restart.exe",
							"Android",
							Locale.Strings.WORK_DONE_TEXT,
							Locale.Strings.STUCK_AT_INITIALIZING_FORM_TEXT);
				}
			}
		}

		private void DoCategoryRelatedTask(object sender, System.EventArgs e)
		{
			try
			{
				Logger.Info("In method DoCategoryRelatedTask");
				ComboBox categoryBox = (ComboBox)sender;
				string selectedCategory = (string)sStringConversions[categoryBox.SelectedItem];
				if (string.IsNullOrEmpty(selectedCategory))
				{
					mSubCategoryControl.Items.Clear();
					mSubCategoryControl.Hide();
					mAppNameLabelControl.Hide();
					mAppNameControlDropDown.Hide();
					return;
				}
				else
				{
					if (sCategorySubCategoryMapping.ContainsKey(selectedCategory))
					{
						Dictionary<string, string> subCategoryIdValueMapping = new Dictionary<string, string>();
						Dictionary<string, string> subCategoryIdShowDropdownMapping = new Dictionary<string, string>();
						if (sCategorySubCategoryMapping[selectedCategory] != null)
						{
							subCategoryIdValueMapping = sCategorySubCategoryMapping[selectedCategory];
							subCategoryIdShowDropdownMapping = sCategorySubCategoryMappingWithShowDropdown[selectedCategory];
							string[] subCategories = new string[subCategoryIdValueMapping.Count];
							int index = 0;
							foreach (string values in subCategoryIdValueMapping.Values)
							{
								subCategories[index++] = values;
							}
							if (subCategories.Length > 0)
							{
								AddSubCategories(subCategories);
							}
							else
							{
								mSubCategoryControl.Items.Clear();
								mSubCategoryControl.Hide();
							}
						}
					}
				}

				string showDropdown = (String)sCategoryShowDropDownMapping[selectedCategory];

				if (!string.IsNullOrEmpty(showDropdown) && showDropdown == "1")
				{
					AbleDisableAppName(true);
				}
				else
				{
					AbleDisableAppName(false);
				}

			}
			catch (Exception ex)
			{ 
				Logger.Error("Error occured, Err: {0}", ex.ToString());
			}
		}

		private void RunTroubleShooterExe(string fileName, string args, string text, string title)
		{
			try
			{
				Logger.Info("In method RunTroubleShooterexe");
				RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
				string filePath = Path.Combine(
						(string)key.GetValue("InstallDir"),
						fileName);

				Process proc = new Process();
				proc.StartInfo.FileName = filePath;
				proc.StartInfo.Arguments = args;
				proc.EnableRaisingEvents = true;

				proc.Exited += new EventHandler(delegate(object sender, EventArgs e)
						{
						UIHelper.RunOnUIThread(
							sLogCollector,
							delegate()
							{
							Hide();
							Form form = null;
							if (BlueStacks.hyperDroid.Common.Oem.Instance.IsHideMessageBoxIconInTaskBar)
							{
							form = sLogCollector;
							}
							MessageBox.Show(form, text, title, MessageBoxButtons.OK);
							Close();
							}
							);
						});
				proc.Start();
			}
			catch (Exception e)
			{
				Logger.Error("Error occured, Err: {0}", e.ToString());
			}
		}

		private void AbleDisableAppName(bool showDropdown)
		{
			Logger.Info("In Method AbleDisableAppName");
			Logger.Info("Show apps list dropdown: " + showDropdown);
			if (showDropdown)
			{
				mAppNameControlDropDown.Visible = true;
				mAppNameLabelControl.Visible = true;
				mAppNameControlDropDown.Items.Clear();
				if (false == AddAppNames())
				{
					Logger.Info("AddAppNames returns false");
					mAppNameTextBox.Visible = true;
					mAppNameTextBox.Text = "";
					mAppNameControlDropDown.Visible = false;
					mAppNameLabelControl.Text = Locale.Strings.APP_NAME;
					//Show TextBox instead of DropDown
				}
				else
				{
					Logger.Info("AddAppNames returns true");
					mAppNameTextBox.Visible = false;
					mAppNameTextBox.Text = "";
					mAppNameControlDropDown.Visible = true;
					mAppNameLabelControl.Text = Locale.Strings.SELECT_APP_NAME;
					mAppNameControlDropDown.SelectedIndex = 0;
				}
			}
			else
			{
				Logger.Info("User not selected App not working");
				mAppNameControlDropDown.Text = "";
				mAppNameControlDropDown.Visible = false;
				mAppNameLabelControl.Visible = false;
				mAppNameTextBox.Visible = false;
			}
		}

		private void AbleDisableAppName(ComboBox categoryBox)
		{
			Logger.Info("In Method AbleDisableAppName");
			if ((String.Compare((string)sStringConversions[categoryBox.SelectedItem], "App not working", true) == 0) || (String.Compare((string)sStringConversions[categoryBox.SelectedItem], "Laggy game play", true) == 0))
			{
				mAppNameControlDropDown.Visible = true;
				mAppNameLabelControl.Visible = true;
				mAppNameControlDropDown.Items.Clear();
				if (false == AddAppNames())
				{
					Logger.Info("AddAppNames returns false");
					mAppNameTextBox.Visible = true;
					mAppNameTextBox.Text = "";
					mAppNameControlDropDown.Visible = false;
					mAppNameLabelControl.Text = Locale.Strings.APP_NAME;
					//Show TextBox instead of DropDown
				}
				else
				{
					Logger.Info("AddAppNames returns true");
					mAppNameTextBox.Visible = false;
					mAppNameTextBox.Text = "";
					mAppNameControlDropDown.Visible = true;
					mAppNameLabelControl.Text = Locale.Strings.SELECT_APP_NAME;
					mAppNameControlDropDown.SelectedIndex = 0;
				}
			}
			else
			{
				Logger.Info("User not selected App not working");
				mAppNameControlDropDown.Text = "";
				mAppNameControlDropDown.Visible = false;
				mAppNameLabelControl.Visible = false;
				mAppNameTextBox.Visible = false;
			}
		}

		private void InitializeBackgroundWorker()
		{
			mCategoryDownloader = new BackgroundWorker();
			mCategoryDownloader.WorkerSupportsCancellation = true;
			mCategoryDownloader.DoWork += new DoWorkEventHandler(DownloadCategories);
			mCategoryDownloader.RunWorkerCompleted += new RunWorkerCompletedEventHandler(ShowCategories);
			mCategoryDownloader.RunWorkerAsync();
		}

		private LogCollector()
		{
			InitializeBackgroundWorker();

			this.Icon = Utils.GetApplicationIcon();

			RegistryKey baseKey = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);

			string installStatus = (string)baseKey.GetValue("InstallType");
			if ((string.Compare(installStatus, "nconly", true) == 0))
			{
				this.Text = Locale.Strings.NC_ONLY_FORM_TEXT;
			}
			else
			{
				this.Text = BlueStacks.hyperDroid.Common.Oem.Instance.BluestacksLogCollectorText;
			}
			this.FormBorderStyle = FormBorderStyle.FixedSingle;
			this.MaximizeBox = false;
			this.MinimizeBox = false;
			this.ClientSize = new Size(300, 435);
			sLogCollector = this;

			RegistryKey key = Registry.LocalMachine.CreateSubKey(Common.Strings.CloudRegKeyPath);
			String regEmail = (String)key.GetValue("Email", "");

			mEmailLabelControl = new Label();
			mEmailLabelControl.Text = Locale.Strings.EMAIL_LABEL;
			mEmailLabelControl.AutoSize = true;
			mEmailLabelControl.Location = new Point(20, 15);

			mEmailControl = new TextBox();
			mEmailControl.Text = regEmail;
			mEmailControl.AutoSize = true;
			mEmailControl.Location = new Point(20, mEmailLabelControl.Bottom + 5);
			mEmailControl.Size = new Size(260, 15);

			string loadingImagePath;
			using (key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath))
			{
				string installDir = (String)key.GetValue("InstallDir");
				loadingImagePath = Path.Combine(installDir, LOADING_IMAGE_LOGCOLLECTOR);
			}
			mLoadingImage = new Bitmap(loadingImagePath);

			mLoadingControlBox = new PictureBox();
			mLoadingControlBox.AutoSize = false;
			mLoadingControlBox.Location = new Point(22, 120);
			mLoadingControlBox.Size = new Size(40, 15);
			mLoadingControlBox.Visible = false;
			mLoadingControlBox.SizeMode = PictureBoxSizeMode.StretchImage;
			mLoadingControlBox.Image = mLoadingImage;

			mCategoryControl = new ComboBox();
			mCategoryControl.AutoSize = true;
			mCategoryControl.Location = new Point(20, 90);
			mCategoryControl.Size = new Size(260, 15);
			mCategoryControl.DropDownWidth = 150;
			mCategoryControl.DropDownStyle = ComboBoxStyle.DropDownList;
			mCategoryControl.DrawMode = DrawMode.OwnerDrawFixed;
			mCategoryControl.DrawItem += CategoryDrawItem;
			mCategoryControl.DropDownClosed += CategoryDropDownClosed;
			mCategoryControl.MouseLeave += new EventHandler(CategoryMouseLeave);

			mSubCategoryControl = new ComboBox();
			mSubCategoryControl.AutoSize = true;
			mSubCategoryControl.Location = new Point(20, 120);
			mSubCategoryControl.Size = new Size(260, 15);
			mSubCategoryControl.DropDownWidth = 150;
			mSubCategoryControl.DropDownStyle = ComboBoxStyle.DropDownList;
			mSubCategoryControl.DrawMode = DrawMode.OwnerDrawFixed;
			mSubCategoryControl.DrawItem += SubcategoryDrawItem;
			mSubCategoryControl.DropDownClosed += SubcategoryDropDownClosed;
			mSubCategoryControl.MouseLeave += new EventHandler(SubcategoryMouseLeave);
			mSubCategoryControl.SelectedIndexChanged += new System.EventHandler(DoSubCategoryRelatedTask);

			if (sProblemCategories != null)
			{
				AddCategories();
			}
			else
			{
				mCategoryControl.Visible = false;
				mLoadingControlBox.Visible = true;
				mSubCategoryControl.Visible = false;
			}

			mAppNameLabelControl = new Label();
			mAppNameLabelControl.Text = Locale.Strings.SELECT_APP_NAME;
			mAppNameLabelControl.AutoSize = true;
			mAppNameLabelControl.Location = new Point(20, 175);
			mAppNameLabelControl.Visible = false;

			mAppNameControlDropDown = new ComboBox();
			mAppNameControlDropDown.AutoSize = true;
			mAppNameControlDropDown.Location = new Point(20, 195);
			mAppNameControlDropDown.Visible = false;
			mAppNameControlDropDown.Size = new Size(260, 15);
			mAppNameControlDropDown.DropDownWidth = 150;
			mAppNameControlDropDown.DropDownStyle = ComboBoxStyle.DropDownList;

			mAppNameTextBox = new TextBox();
			mAppNameTextBox.AutoSize = true;
			mAppNameTextBox.Location = new Point(20, 195);
			mAppNameTextBox.Visible = false;
			mAppNameTextBox.Size = new Size(260, 15);

			mDescriptionLabelControl = new Label();
			mDescriptionLabelControl.Text = Locale.Strings.DESCRIPTION_LABEL;
			mDescriptionLabelControl.AutoSize = true;
			mDescriptionLabelControl.Location = new Point(20, 230);

			mDescriptionControl = new TextBox();
			mDescriptionControl.AutoSize = true;
			mDescriptionControl.Location = new Point(20, 250);
			mDescriptionControl.Size = new Size(260, 140);
			mDescriptionControl.Multiline = true;

			mStartControl = new Button();
			mStartControl.Text = Locale.Strings.BUTTON_TEXT;
			mStartControl.Size = new Size(80, 25);
			mStartControl.Location = new Point(
					(300 - mStartControl.Width) / 2, 400);

			mRPCProgressBarLabelControl = new Label();
			mRPCProgressBarLabelControl.AutoSize = true;
			mRPCProgressBarLabelControl.Location = new Point(20, 15);
			mRPCProgressBarLabelControl.Visible = false;

			mRPCProgressBarControl = new ProgressBar();
			mRPCProgressBarControl.Style = ProgressBarStyle.Marquee;
			mRPCProgressBarControl.Value = 100;
			mRPCProgressBarControl.Width = 260;
			mRPCProgressBarControl.Location = new Point(20, 75);
			mRPCProgressBarControl.Visible = false;


			this.Controls.Add(mRPCProgressBarLabelControl);
			this.Controls.Add(mRPCProgressBarControl);
			this.Controls.Add(mStartControl);
			this.Controls.Add(mEmailLabelControl);
			this.Controls.Add(mEmailControl);
			this.Controls.Add(mCategoryControl);
			this.Controls.Add(mSubCategoryControl);
			this.Controls.Add(mLoadingControlBox);
			this.Controls.Add(mAppNameLabelControl);
			this.Controls.Add(mAppNameControlDropDown);
			this.Controls.Add(mDescriptionLabelControl);
			this.Controls.Add(mDescriptionControl);
			this.Controls.Add(mAppNameTextBox);

			mStartControl.Click += delegate(Object obj, EventArgs evt)
			{
				try
				{
					String email = mEmailControl.Text;
					String desc = mDescriptionControl.Text;
					string selectedAppPkgName = "";
					if (mAppNameControlDropDown.SelectedItem != null)
					{
						ComboboxItem item = (ComboboxItem)mAppNameControlDropDown.SelectedItem;
						selectedAppPkgName = item.Value;
						Logger.Info("Selected Package Name: " + selectedAppPkgName);
					}
					String appPkgName = mAppNameControlDropDown.Visible == true ? selectedAppPkgName : mAppNameTextBox.Text;
					int categoryIndex = 0;
					String category = null;
					string subCategory = "";
					int subCategoryIndex = mSubCategoryControl !=null ? mSubCategoryControl.SelectedIndex : 0;
					if (sProblemCategories != null)
					{
						categoryIndex = mCategoryControl.SelectedIndex;
						category = mCategoryControl.SelectedItem.ToString();
						if ((appPkgName == null || appPkgName == "" || appPkgName == "Select"))
						{
							if (mAppNameControlDropDown.Visible || mAppNameTextBox.Visible)
							{
								MessageBox.Show(mAppNameControlDropDown.Visible ? Locale.Strings.APP_NOT_SELECTED_TEXT : Locale.Strings.APP_MISSING_TEXT,
										Oem.Instance.BluestacksLogCollectorText, MessageBoxButtons.OK);
								return;
							}	
						}
						else
						{
							Logger.Info("Selected App Package Name: " + appPkgName);
						}
					}

					if (!ValidateEmail(email))
					{
						MessageBox.Show(Locale.Strings.EMAIL_MISSING_TEXT,
								Oem.Instance.BluestacksLogCollectorText, MessageBoxButtons.OK);
					}
					else if (sProblemCategories != null &&
							categoryIndex == 0)
					{
						MessageBox.Show(Locale.Strings.SELECT_CATEGORY_TEXT,
								Oem.Instance.BluestacksLogCollectorText, MessageBoxButtons.OK);
					}
					else if (mSubCategoryControl != null && mSubCategoryControl.Visible &&
							subCategoryIndex == 0)
					{
						MessageBox.Show(Locale.Strings.SELECT_SUBCATEGORY_TEXT,
								Oem.Instance.BluestacksLogCollectorText, MessageBoxButtons.OK);
					}
					else if (String.IsNullOrEmpty(desc.Trim()))
					{
						MessageBox.Show(Locale.Strings.DESC_MISSING_TEXT,
								Oem.Instance.BluestacksLogCollectorText, MessageBoxButtons.OK);
					}
					else
					{
						try
						{
							mStartControl.Enabled = false;

							if (sProblemCategories != null)
								category = (String)sStringConversions[category];

							if (sCategorySubCategoryMapping[category] != null && mSubCategoryControl.Visible && mSubCategoryControl.SelectedItem != null)
							{
								subCategory = mSubCategoryControl.SelectedItem.ToString();
								Dictionary<string, string> subCategoryIdValueMapping = new Dictionary<string, string>();
								subCategoryIdValueMapping = sCategorySubCategoryMapping[category];
								subCategory= FindKey(subCategory, subCategoryIdValueMapping);
							}

							CollectLogs collectLogsForm = new CollectLogs(email, category, appPkgName, desc, subCategory);
							this.Hide();
							collectLogsForm.ShowDialog(this);
						}
						catch (Exception e)
						{
							Logger.Error(string.Format("Error Occured, Err : {0}", e.ToString()));
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error(string.Format("Error Occured, Err : {0}", ex.ToString()));
				}
			};
		}

		public string FindKey(string Value, Dictionary<string, string> HT)
		{
			Logger.Info("In method FindKey, finding key for: " + Value + " in dictionary");
			foreach (KeyValuePair<string, string> keyValPair in HT)
			{
				Logger.Info("Key: " + keyValPair.Key + "Value: " + keyValPair.Value);
			}
			string Key = "";
			IDictionaryEnumerator e = HT.GetEnumerator();
			while (e.MoveNext())
			{
				if (e.Value.ToString().Equals(Value))
				{
					Key = e.Key.ToString();
				}
			}
			return Key;
		}

		private void AddCategories()
		{
			Logger.Info("In method AddCategories");
			mCategoryControl.Items.Clear();
			ComboboxItem select = new ComboboxItem();
			select.Text = Locale.Strings.CATEGORY;
			select.Value = "Category";
			mCategoryControl.Items.Add(select);
			mCategoryControl.Items.AddRange(sProblemCategories);
			mCategoryControl.SelectedIndex = 0;
			mCategoryControl.SelectedIndexChanged += new System.EventHandler(DoCategoryRelatedTask);
		}

		private void AddSubCategories(string[] subCategories)
		{
			Logger.Info("In method AddSubCategories");
			mSubCategoryControl.Items.Clear();
			ComboboxItem select = new ComboboxItem();
			select.Text = Locale.Strings.SUBCATEGORY;
			select.Value = "Subcategory";
			mSubCategoryControl.Items.Add(select);
			mSubCategoryControl.Items.AddRange(subCategories);
			mSubCategoryControl.SelectedIndex = 0;
			mSubCategoryControl.Show();
			//mSubCategoryControl.SelectedIndexChanged += new System.EventHandler(DoSubCategoryRelatedTask);
		}

		private bool ValidateEmail(String email)
		{
			string pattern = @"\w+([-+.]\w+)*@\w+([-.]\w+)*\.\w+([-.]\w+)*";
			Regex valid = new Regex(pattern);
			bool isValid = valid.IsMatch(email);
			return isValid;
		}

		private bool AddAppNames()
		{
			try
			{
				Logger.Info("In Method AddAppNames");
				string strAppList = "";
				Thread thread = new Thread(delegate()
						{
						Logger.Info("Getting installed apps");
						int agentPort = Utils.GetAgentServerPort();
						string restartUrl = String.Format("http://127.0.0.1:{0}/{1}",
							agentPort,
							"getapplist");
						Logger.Info("Requesting Agent");
						strAppList = Common.HTTP.Client.Get(restartUrl, null, false);
						});

				thread.IsBackground = true;
				thread.Start();
				Thread.Sleep(500);
				if (string.IsNullOrEmpty(strAppList))
				{
					Logger.Info("Empty app list");
					return false;
				}
				JSonReader reader = new JSonReader();
				IJSonObject input = reader.ReadAsJSonObject(strAppList);
				AppInfo[] appInfo = new AppInfo[input.Length];
				if (appInfo == null || appInfo.Length == 0)
				{
					Logger.Info("AppInfo null");
					return false;
				}
				for (int i = 0; i < input.Length; i++)
				{
					appInfo[i] = new AppInfo(input[i]);
					appInfo[i].name = Regex.Replace(appInfo[i].name, @"\t|\n|\r", string.Empty);
				}

				foreach (AppInfo app in appInfo)
				{
					if (string.IsNullOrEmpty(app.name) || string.IsNullOrEmpty(app.package))
					{
						Logger.Info("Empty app name or package");
						return false;
					}
					else
					{
						Logger.Info("App Package Name: " + app.package + " App Name: " + app.name);
					}
				}

				Array.Sort(appInfo,
						delegate(AppInfo x, AppInfo y) { return x.name.CompareTo(y.name); });

				ComboboxItem select = new ComboboxItem();
				select.Text = Locale.Strings.SELECT;
				select.Value = "Select";
				mAppNameControlDropDown.Items.Add(select);
				foreach (AppInfo app in appInfo)
				{
					ComboboxItem item = new ComboboxItem();
					item.Text = Regex.Replace(app.name, @"\t|\n|\r", string.Empty);
					item.Value = app.package;
					mAppNameControlDropDown.Items.Add(item);
				}

				ComboboxItem other = new ComboboxItem();
				other.Text = "Other";
				other.Value = "Other";
				mAppNameControlDropDown.Items.Add(other);

				return true;
			}
			catch (Exception ex)
			{
				Logger.Error(string.Format("Error Occured, Err: {0}", ex.ToString()));
				return false;
			}
		}

		void CategoryMouseLeave(object sender, EventArgs e)
		{
			toolTipDropDown.Hide(mCategoryControl);
		}

		private void CategoryDropDownClosed(object sender, EventArgs e)
		{
			toolTipDropDown.Hide(mCategoryControl);
		}

		private void CategoryDrawItem(object sender, DrawItemEventArgs e)
		{
			if (e.Index < 0) { return; }
			string text = mCategoryControl.GetItemText(mCategoryControl.Items[e.Index]);
			e.DrawBackground();
			using (SolidBrush br = new SolidBrush(e.ForeColor))
			{ e.Graphics.DrawString(text, e.Font, br, e.Bounds); }
			if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
			{ toolTipDropDown.Show(text, mCategoryControl, e.Bounds.Right, e.Bounds.Bottom, 2000); }
			e.DrawFocusRectangle();
		}

		void SubcategoryMouseLeave(object sender, EventArgs e)
		{
			toolTipDropDown.Hide(mSubCategoryControl);
		}

		private void SubcategoryDropDownClosed(object sender, EventArgs e)
		{
			toolTipDropDown.Hide(mSubCategoryControl);
		}

		private void SubcategoryDrawItem(object sender, DrawItemEventArgs e)
		{
			try
			{
				if (e.Index < 0) { return; }
				//if (mSubCategoryControl.Items.Count == 0) { return; }
				string text = mSubCategoryControl.GetItemText(mSubCategoryControl.Items[e.Index]);
				e.DrawBackground();
				using (SolidBrush br = new SolidBrush(e.ForeColor))
				{ e.Graphics.DrawString(text, e.Font, br, e.Bounds); }
				if ((e.State & DrawItemState.Selected) == DrawItemState.Selected)
				{ toolTipDropDown.Show(text, mSubCategoryControl, e.Bounds.Right, e.Bounds.Bottom, 2000); }
				e.DrawFocusRectangle();
			}
			catch (Exception ex)
			{ 

				Logger.Error(string.Format("Error Occured, Err: {0}", ex.ToString()));
			}
		}

		public class CollectLogs : Form
		{

			[DllImport("USER32.DLL")]
				public static extern bool SetForegroundWindow(IntPtr hWnd);

			[DllImport("user32.dll", EntryPoint = "FindWindow", CharSet = CharSet.Unicode)]
				private static extern IntPtr FindWindow(string sClass, string sWindow);


			private const int PROGRESS_COUNT = 12;

			private string CFG_ROOT = Common.Strings.RegBasePath;

			private TextWriter mOrigOut;

			private SynchronizationContext mUiContext;
			private System.Windows.Forms.ProgressBar mProgressControl;
			private Label mStatusControl;
			private Button mExitControl;

			private DirectoryInfo mTmpDir;

			private String mInstallDir;
			private String mUserDataDir;
			private String mPerUserDir;

			private bool mUploadSuccessful;

			private BackgroundWorker mGuestInfoCollector;
			private BackgroundWorker mHostInfoCollector;
			private bool mGuestInfoAvailable = false;
			private bool mHostInfoAvailable = false;
			private String mEmail;
			private String mCategory;
			private String mAppName;
			private String mErrorDetails;
			private String mDescription;
			private String mErrorReason;
			private String mErrorCode;
			private String mSubcategory;
			private System.Timers.Timer mQuitTimer;

			private static String s_DestinationFolder = Path.GetTempPath();
			private static bool s_UploadZip = true;
			private static bool s_ShowUI = true;

			public static bool s_StartLogcat = false;
			public static bool s_ThinCollector = false;
			public static bool s_SilentCollector = false;
			public static bool s_ApkInstallFailureLogCollector = false;
			public static String s_InstallFailedApkName = "";
			public static bool s_BootFailureLogs = false;
			public static bool s_CrashLogs = false;
			public CollectLogs(string dest, bool uploadZip)
			{
				s_DestinationFolder = dest;
				s_UploadZip = uploadZip;
				CreateStagingDir();

				EnableDebugPrivilege();
				GetConfig();
			}

			public CollectLogs(string dest, bool uploadZip, string errorReason, string errorDetails, string errorCode)
			{
				mErrorDetails = errorDetails;
				s_DestinationFolder = dest;
				s_UploadZip = uploadZip;
				mErrorReason = errorReason;
				mErrorCode = errorCode;
				CreateStagingDir();

				EnableDebugPrivilege();
				GetConfig();
			}

			public CollectLogs(string dest, bool uploadZip, string errorReason, string errorCode)
			{
				s_DestinationFolder = dest;
				s_UploadZip = uploadZip;
				mErrorReason = errorReason;
				mErrorCode = errorCode;
				CreateStagingDir();

				EnableDebugPrivilege();
				GetConfig();
			}

			public void StartSilentLogCollection(bool showUI)
			{
				s_ShowUI = showUI;
				OpenLog();
				CollectGuestArtifacts(null, null);
				mGuestInfoAvailable = true;
				CollectHostArtifacts(null, null);
				mHostInfoAvailable = true;
				DoAfterLogCollection(null, null);
			}

			public CollectLogs(String email, String category, String appName, String desc, String subcategory)
			{
				mEmail = email;
				mCategory = category;
				mAppName = appName;
				mDescription = desc;
				mSubcategory = subcategory;

				this.Icon = Utils.GetApplicationIcon();
				mUiContext = WindowsFormsSynchronizationContext.Current;

				/*
				 * Setup the form.
				 */
				this.Text = BlueStacks.hyperDroid.Common.Oem.Instance.BluestacksLogCollectorText;
				this.FormBorderStyle = FormBorderStyle.FixedSingle;
				this.MaximizeBox = false;
				this.MinimizeBox = false;
				this.ClientSize = new Size(300, 120);

				mStatusControl = new Label();
				mStatusControl.Text = Locale.Strings.STATUS_INITIAL;
				mStatusControl.AutoSize = true;
				mStatusControl.Location = new Point(20, 15);

				mProgressControl = new System.Windows.Forms.ProgressBar();
				mProgressControl.Value = 0;
				mProgressControl.Minimum = 0;
				mProgressControl.Maximum = PROGRESS_COUNT;
				mProgressControl.Width = 260;
				mProgressControl.Location = new Point(20, 45);

				mExitControl = new Button();
				mExitControl.Text = "Cancel";
				mExitControl.Location = new Point(
						280 - mExitControl.Width, 85);

				mExitControl.Click += delegate(Object obj, EventArgs evt)
				{
					Application.Exit();
				};

				this.Controls.Add(mStatusControl);
				this.Controls.Add(mProgressControl);
				this.Controls.Add(mExitControl);

				mQuitTimer = new System.Timers.Timer(5 * 60 * 1000);
				mQuitTimer.Elapsed += SendLogsTimeout;
				mQuitTimer.Enabled = true;


				/*
				 * Fire off another thread to perform log collection.
				 */

				Thread thread = new Thread(delegate()
						{

						CreateStagingDir();
						UpdateProgress();

						OpenLog();
						UpdateProgress();

						EnableDebugPrivilege();
						GetConfig();
						UpdateProgress();

						CollectArtifacts();

						});

				thread.IsBackground = true;
				thread.Start();
			}

			private void SendLogsTimeout(Object source, ElapsedEventArgs e)
			{
				try
				{
					mQuitTimer.Dispose();
					mHostInfoAvailable = true;
					mGuestInfoAvailable = true;
					DoAfterLogCollection(null, null);
				}
				catch (Exception exp)
				{
					Logger.Error(string.Format("Error Occured, Err: {0}", exp.ToString()));
				}
				Environment.Exit(1);
			}

			private void DoAfterLogCollection(Object sender, RunWorkerCompletedEventArgs workerCompletedArgs)
			{
				if (!mHostInfoAvailable || !mGuestInfoAvailable)
				{
					Console.WriteLine(string.Format("Guest Info Available = {0}, Host Info Available = {1}, ::::::: {2}", mGuestInfoAvailable, mHostInfoAvailable, DateTime.Now));
					return;
				}
				else
				{
					/*
					 * To ensure that we send logs only once.
					 */
					mHostInfoAvailable = false;
					mGuestInfoAvailable = false;
				}
				CloseLog();

				if (s_ShowUI)
					UpdateProgress();
				try
				{
					CreateZipFile();
				}
				catch (Exception e)
				{
					Logger.Error("Exception in createzipfile");
					Logger.Error(e.ToString());
				}
				if (s_ShowUI == true)
					UpdateProgress();
				if (s_UploadZip)
				{
					if (s_ShowUI)
					{
						UploadZipFile(mEmail, mCategory, mAppName, mDescription, mSubcategory);
					}
					else
					{
						if (s_ApkInstallFailureLogCollector)
						{
							string category = "apk install failure";
							string description = mErrorCode + "-->" + mErrorReason;
							string subcategory = ""; //Need to ask Vikram
							UploadZipFile("developer@bluestacks.com", category, s_InstallFailedApkName, description, subcategory);
						}
						else
						{
							string category = "", description = "silent debug logs";
							if (s_ThinCollector == true)
							{
								category = "ThinLogCollector Logs";
								description = "silent thin debug logs";
							}
							else if (s_BootFailureLogs == true)
							{
								category = "Boot Failure";
								description = mErrorReason;
							}
							else if (s_SilentCollector == true)
							{
								category = "Silent Log Collection";
								description = "silent debug logs";
							}
							//Need to ask Vikram
							string subcategory = "";
							UploadZipFile("developer@bluestacks.com", category, "", description, subcategory);
						}
					}
				}
				if (mQuitTimer != null)
					mQuitTimer.Enabled = false;

				if (s_ShowUI == true)
				{
					UpdateProgress();
					ShowFinish();
				}
				Environment.Exit(0);
			}

			private void EnableDebugPrivilege()
			{
				try
				{
					Process.EnterDebugMode();

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}
			}

			private void CreateStagingDir()
			{
				String name = Path.GetRandomFileName();
				String path = Path.Combine(Path.GetTempPath(), name);

				Console.WriteLine("Creating temporary directory , {0}  ::::::: {1}", path, DateTime.Now);
				mTmpDir = Directory.CreateDirectory(path);

				this.FormClosing += delegate(Object obj,
						FormClosingEventArgs evt)
				{
					Console.WriteLine("Deleting temporary directory,  ::::::: {0}", DateTime.Now);
					mTmpDir.Delete(true);
				};
			}

			/*
			 * Open a log file in our temporary directory and use it to
			 * redirect the standard output stream.
			 */
			private void OpenLog()
			{
				StreamWriter writer = new StreamWriter(
						Path.Combine(mTmpDir.FullName, "LogCollector.log"));

				writer.AutoFlush = true;

				mOrigOut = Console.Out;
				Console.SetOut(writer);

				this.FormClosing += delegate(Object obj,
						FormClosingEventArgs evt)
				{
					writer.Close();
				};
			}

			/*
			 * Close our log file and restore our original standard output
			 * stream.
			 */
			private void CloseLog()
			{
				Console.Out.Close();
				Console.SetOut(mOrigOut);
			}

			private void GetConfig()
			{
				RegistryKey key;

				using (key = Registry.LocalMachine.OpenSubKey(CFG_ROOT))
				{
					mInstallDir = (String)key.GetValue("InstallDir");
					mUserDataDir = (String)key.GetValue("DataDir");
				}

				mPerUserDir = Common.Strings.BstUserDataDir;

				Console.WriteLine("Install Dir:   " + mInstallDir + "  ::::::: {0}", DateTime.Now);
				Console.WriteLine("User Data Dir: " + mUserDataDir + "  ::::::: {0}", DateTime.Now);
				Console.WriteLine("Per User Dir:  " + mPerUserDir + "  ::::::: {0}", DateTime.Now);
			}

			private bool StartAndroidService()
			{
				ServiceController[] services = ServiceController.GetServices();
				foreach (ServiceController service in services)
				{
					if (service.ServiceName == Common.Strings.AndroidServiceName)
					{
						if (service.Status == ServiceControllerStatus.Running)
						{
							Console.WriteLine(string.Format("the service:{0} is already running", service.ServiceName) + "  ::::::: {0}", DateTime.Now);
							return true;
						}
						else
						{
							Console.WriteLine(string.Format("trying to start the service:{0}, current status = {1}", service.ServiceName, service.Status) + "  ::::::: {0}", DateTime.Now);
							try
							{
								service.Start();
								service.WaitForStatus(ServiceControllerStatus.Running, TimeSpan.FromSeconds(10));
								Console.WriteLine(string.Format("Android service:{0} is running now", service.ServiceName) + " :::::::  {0}", DateTime.Now);
								return true;
							}
							catch (Exception e)
							{
								Console.WriteLine(string.Format("Service : {0} could not be started, Exception:{1}", service.ServiceName, e.ToString()) + " :::::::  {0}", DateTime.Now);
							}
						}
					}
				}
				return false;
			}
			private void CollectGuestArtifacts(Object sender, DoWorkEventArgs doWorkEventArgs)
			{
				Console.WriteLine("Starting CollectGuestArtifacts  :::::::  {0}", DateTime.Now);

				if (!StartAndroidService())
				{
					Console.WriteLine("Android service not running, Android-dump State logs will not be created, returing  :::::::  {0}", DateTime.Now);
					return;
				}

				/*
				 * Launch frontend if not running. It helps bring extra logs.
				 */
				if (Utils.IsUIProcessAlive() == false)
				{
					string frontendExe = "HD-Frontend.exe";
					string fePath = Path.Combine(mInstallDir, frontendExe);
					Process proc = Process.Start(fePath, String.Format("{0} -h", "Android"));
					Console.WriteLine(frontendExe + " process not running, starting in hidden mode");
					if (s_ThinCollector == false &&
							s_ApkInstallFailureLogCollector == false &&
							s_BootFailureLogs == false)		//Wait for frontend to come up only when LogCollector is run normally
					{
						proc.WaitForInputIdle();
						Utils.WaitForBootComplete();
					}
				}

				String adbHost;
				String adbPath;

				int adbPort;

				/*
				 * Find the appropriate host name to use when running
				 * ADB.
				 */

				adbPort = BlueStacks.hyperDroid.Common.Utils.GetAdbPort();

				adbHost = String.Format("127.0.0.1:{0}", adbPort);

				/*
				 * Spin up the ADB server now, so we don't have to wait
				 * for it later.
				 */

				adbPath = Path.Combine(mInstallDir, "HD-Adb.exe");

				//RunCmdAsync(adbPath, "start-server");
				//Thread.Sleep(250);


				UpdateMessage(Locale.Strings.STATUS_COLLECTING_GUEST);
				UpdateProgress();

				Console.WriteLine("Fetching Guest Information  :::::::  {0}", DateTime.Now);

				/*
				 * Connect to the guest.
				 */

				RunCmdWithList(adbPath, new String[] { "connect", adbHost });

				/*
				 * Extra Debugging
				 */
				if (s_StartLogcat == true)
				{
					RunCmdWithList(adbPath, new String[] {
							"-s", adbHost, "shell", "/system/xbin/bstk/su", "-c", "stop" }, Path.Combine(mTmpDir.FullName, "stop.txt"));

					RunCmdWithList(adbPath, new String[] {
							"-s", adbHost, "shell", "/system/xbin/bstk/su", "-c", "logcat", "-c" }, null);

					RunCmdWithList(adbPath, new String[] {
							"-s", adbHost, "shell", "/system/xbin/bstk/su", "-c", "start", "logcat" }, null);

					RunCmdWithList(adbPath, new String[] {
							"-s", adbHost, "shell", "/system/xbin/bstk/su", "-c", "start" }, Path.Combine(mTmpDir.FullName, "start.txt"));

					Thread.Sleep(30 * 1000);
				}

				/*
				 * Capture the output of dumpstate
				 */

				Console.WriteLine(string.Format("Dumping Android Logs at {0}:{1}",
							DateTime.Now.Minute, DateTime.Now.Second) + "  ::::::: {0}", DateTime.Now);

				Thread dumpstateCollector = new Thread(delegate()
						{
						string dumpstateFilePath = Path.Combine(mTmpDir.FullName, "Android-DumpState.log");
						RunCmdWithList(adbPath,
							new String[] { "-s", adbHost, "shell", "dumpstate" },
							dumpstateFilePath);
						if (File.Exists(dumpstateFilePath))
						{
						FileInfo dumpStateFile = new FileInfo(dumpstateFilePath);
						long fileSize = dumpStateFile.Length;
						if (fileSize < 10 * 1024)
						{
						Console.WriteLine("Dumpstate file size less than 10KB, kill server and start server and collect dumpstate  ::::::: {0}", DateTime.Now);
						RunCmd(adbPath, "kill-server", null);
						Console.WriteLine("kill-server done  ::::::: {0}", DateTime.Now);
						RunCmd(adbPath, "start-server", null);
						Console.WriteLine("start-server done  ::::::: {0}", DateTime.Now);
						RunCmdWithList(adbPath,
							new String[] { "-s", adbHost, "shell", "dumpstate" },
							dumpstateFilePath);
						}
						}
						});
				dumpstateCollector.Start();

				if (s_ApkInstallFailureLogCollector == false &&
						s_BootFailureLogs == false)
				{
					/*
					 * Run some simple network tests in the guest.
					 */

					RunCmdWithList(adbPath,
							new String[] { "-s", adbHost, "shell", "nslookup",
							"www.google.com" },
							Path.Combine(mTmpDir.FullName, "Guest-nslookup.txt"));

					RunCmdWithList(adbPath,
							new String[] { "-s", adbHost, "shell", "wget", "-O",
							"/dev/null", "http://www.google.com" },
							Path.Combine(mTmpDir.FullName, "Guest-wget.txt"));
					/*
					 *	and fetch the ARM logs.
					 */
					RunCmdWithList(adbPath, new String[] {
							"-s", adbHost, "pull", "/mnt/sdcard/arm-logs",
							Path.Combine(mTmpDir.FullName, "ArmLogs") });

					RunCmdWithList(adbPath, new String[] {
							"-s", adbHost, "pull", "/data/anr/traces.txt",
							Path.Combine(mTmpDir.FullName, "anr-traces.txt") },
							Path.Combine(mTmpDir.FullName, "anr-out.txt"));

					/*
					 * This stuff is only for debugging purposes of additional issues
					 *
					 string[] apps = new string[] {
					 "/data/downloads/bluestacksServices.apk",
					 "/data/downloads/settings.apk",
					 "/data/downloads/S2P.apk",
					 "/data/downloads/bluestacksHome.apk",
					 "/data/priv-downloads/com.google.android.gsf.apk",
					 "/data/priv-downloads/com.google.android.gsf.login.apk",
					 "/data/priv-downloads/com.google.android.gms.apk"
					 };

					 for (int i = 0; i < apps.Length; i++)
					 {
					 RunCmdWithList(adbPath, new String[] {
					 "-s", adbHost, "shell", "/system/xbin/bstk/su", "-c", "md5", apps[i] },
					 Path.Combine(mTmpDir.FullName, string.Format("md5-{0}.txt", i)));
					 }
					 */

					if (Utils.IsUIProcessAlive())
					{
						string pngFilePath = "/mnt/sdcard/" +
							(new DateTime()).ToString().Replace(" ", "_").Replace("/", "_").Replace(":", "_") +
							".png";
						RunCmdWithList(adbPath,
								new String[] { "-s", adbHost, "shell", "screencap", "-p", pngFilePath },
								null);

						RunCmdWithList(adbPath,
								new String[] { "-s", adbHost, "pull", pngFilePath,
								Path.Combine(mTmpDir.FullName, "guest_screenshot.png")},
								null);


					}
				}
				dumpstateCollector.Join();

				Console.WriteLine("Done CollectGuestArtifacts  :::::::  {0}", DateTime.Now);
				mGuestInfoAvailable = true;
			}

			private void CollectArtifacts()
			{
				mGuestInfoCollector = new BackgroundWorker();
				mGuestInfoCollector.DoWork += new DoWorkEventHandler(CollectGuestArtifacts);
				mGuestInfoCollector.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoAfterLogCollection);
				mGuestInfoCollector.RunWorkerAsync();

				mHostInfoCollector = new BackgroundWorker();
				mHostInfoCollector.DoWork += new DoWorkEventHandler(CollectHostArtifacts);
				mHostInfoCollector.RunWorkerCompleted += new RunWorkerCompletedEventHandler(DoAfterLogCollection);
				mHostInfoCollector.RunWorkerAsync();
			}

			private void CollectHostArtifacts(object sender, DoWorkEventArgs doEventArgs)
			{
				Console.WriteLine("Starting CollectHostArtifacts  :::::::  {0}", DateTime.Now);
				String args, path;
				const string dataDirListing = "DataDirListing.txt";
				const string installDirListing = "InstallDirListing.txt";
				UpdateMessage(Locale.Strings.STATUS_COLLECTING_PRODUCT);
				Console.WriteLine("Copying Product Logs  :::::::  {0}", DateTime.Now);

				/*
				 * Ask the HD service's to dump additional information
				 * to the logs.
				 */

				DumpServiceInfo();

				Thread eventLogCollectorThread = null;

				try
				{

					if (Utils.IsUIProcessAlive())
					{
						DumpPendingGlCalls();
					}

					CopyRecursive(Path.Combine(mUserDataDir, "Logs"),
							Path.Combine(mTmpDir.FullName, "Logs"));

					string oemCfgFilePath = Path.Combine(mUserDataDir, "Oem.cfg");
					if(File.Exists(oemCfgFilePath))
					{
						File.Copy(oemCfgFilePath, Path.Combine(mTmpDir.FullName, "Oem.cfg"));
						Console.WriteLine("Oem.cfg file successfully copied from path {0}", oemCfgFilePath);
					}
					else
					{
						Console.WriteLine("Oem config file does not exists at location {0}", oemCfgFilePath);
					}

#if BUILD_HYBRID
					try
					{
						if(Directory.Exists(Path.Combine(mUserDataDir, "Android\\Logs")))
							CopyRecursive(Path.Combine(mUserDataDir, "Android\\Logs"),
									Path.Combine(mTmpDir.FullName, "Android\\Logs"));

						if(Directory.Exists(Path.Combine(mUserDataDir, "Manager")))
							CopyRecursive(Path.Combine(mUserDataDir, "Manager"),
									Path.Combine(mTmpDir.FullName, "Manager"));
					}
					catch(Exception ex)
					{
						Logger.Error(ex.ToString());
					}

					try
					{
						string androidVboxFile = Path.Combine(mUserDataDir, "Android\\Android.bstk");
						string androidVboxPrevFile = Path.Combine(mUserDataDir, "Android\\Android.bstk-prev");

						if (!Directory.Exists(Path.Combine(mTmpDir.FullName, "Android")))
							Directory.CreateDirectory(Path.Combine(mTmpDir.FullName, "Android"));

						if (File.Exists(androidVboxFile))
						{
							File.Copy(androidVboxFile, Path.Combine(mTmpDir.FullName, "Android\\Android.bstk"));
						}
						if (File.Exists(androidVboxPrevFile))
						{
							File.Copy(androidVboxPrevFile, Path.Combine(mTmpDir.FullName, "Android\\Android.bstk-prev"));
						}
					}
					catch(Exception ex)
					{
						Logger.Error(ex.ToString());
					}
#endif

					if (s_BootFailureLogs == false)
					{
						string appsJson = "apps.json";
						string appsJsonDir = Path.Combine(mPerUserDir, "Gadget");
						string appsJsonPath = Path.Combine(appsJsonDir, appsJson);
						string tmpAppsJsonLocation = Path.Combine(mTmpDir.FullName, appsJson);
						if (File.Exists(appsJsonPath))
							File.Copy(appsJsonPath, tmpAppsJsonLocation);
					}

					RegistryKey preInstallCheckerKey = Registry.CurrentUser.OpenSubKey("Software\\Bluestacks");
					if (preInstallCheckerKey != null)
					{
						int compatibiltyValue = (int)preInstallCheckerKey.GetValue("Compatible", -1);
						Console.WriteLine("the PREINSTALLCHECKER return value is :::::::: {0}", compatibiltyValue);
						string preInstallCheckerLogFilePath = (string)preInstallCheckerKey.GetValue("logfilepath", "");
						if (preInstallCheckerLogFilePath != "")
						{
							Console.WriteLine("the preinstallercherker log file path is ::::::: {0}", preInstallCheckerLogFilePath);
							string tmpPreInstallCheckerPath = Path.Combine(mTmpDir.FullName, "PreInstallChecker.log");
							if (File.Exists(preInstallCheckerLogFilePath))
								File.Copy(preInstallCheckerLogFilePath, tmpPreInstallCheckerPath);
						}
					}
					else
					{
						Console.WriteLine("the preInstallerChecker Regisry Keys do not exists");
					}
					Console.WriteLine("Dumping Product Configuration  :::::::  {0}", DateTime.Now);

					if (s_ThinCollector == false &&
							s_BootFailureLogs == false &&
							s_ApkInstallFailureLogCollector == false)
					{
						eventLogCollectorThread = new Thread(delegate()
								{
								try
								{
								path = Path.Combine(mTmpDir.FullName,
									"ApplicationEvents.txt");

								DumpEventLogs("Application", path);

								path = Path.Combine(mTmpDir.FullName,
									"SystemEvents.txt");

								DumpEventLogs("System", path);

								}
								catch (Exception exc)
								{

								Console.WriteLine(exc.ToString());
								}
								});
						eventLogCollectorThread.Start();
					}

					try
					{
						UpdateProgress();
						Console.WriteLine("Dumping Process Information  :::::::  {0}", DateTime.Now);

						try
						{
							path = Path.Combine(mTmpDir.FullName, "TaskList.txt");

							DumpProcessList(path);

						}
						catch (Exception exc)
						{

							Console.WriteLine(exc.ToString());
						}

						if (s_BootFailureLogs == false &&
								s_ApkInstallFailureLogCollector == false)
						{
							Console.WriteLine("Dumping Driver Query  :::::::  {0}", DateTime.Now);

							try
							{
								path = Path.Combine(mTmpDir.FullName, "DriverQuery.txt");
								RunCmd("driverquery.exe", "/V", path);

							}
							catch (Exception exc)
							{

								Console.WriteLine(exc.ToString());
							}
						}
					}
					catch (Exception e)
					{
						Logger.Error(e.ToString());
					}

					try
					{
						args = String.Format(
								"EXPORT HKLM\\{0} \"{1}\"", Common.Strings.RegBasePath,
								Path.Combine(mTmpDir.FullName, "RegHKLM.txt"));

						RunCmd("reg.exe", args, null);

						args = String.Format(
								"EXPORT HKLM\\System\\CurrentControlSet\\services\\{0} \"{1}\"", Common.Strings.GetHDAndroidServiceName(),
								Path.Combine(mTmpDir.FullName, "RegBstHdAndroidSvc.txt"));

						RunCmd("reg.exe", args, null);

						args = String.Format(
								"EXPORT HKLM\\System\\CurrentControlSet\\services\\{0} \"{1}\"", Common.Strings.BstHypervisorDrvName,
								Path.Combine(mTmpDir.FullName, "RegBstHdDrv.txt"));

						RunCmd("reg.exe", args, null);
					}
					catch (Exception e)
					{
						Logger.Error(e.ToString());
					}

#if BUILD_HYBRID
					try 
					{
						args = String.Format(
								"EXPORT HKLM\\System\\CurrentControlSet\\services\\{0} \"{1}\"", Common.Strings.GetHDPlusAndroidServiceName(),
								Path.Combine(mTmpDir.FullName, "RegBstHdPlusAndroidSvc.txt"));

						RunCmd("reg.exe", args, null);
					}
					catch(Exception e)
					{
						Logger.Info("Ignoring Error of PlusAndroidSvc since may be case of no plus component has been installed");
						Logger.Error(e.ToString());
					}

					try
					{
						args = String.Format(
								"EXPORT HKLM\\System\\CurrentControlSet\\services\\{0} \"{1}\"", Common.Strings.BstHDPlusDrvName,
								Path.Combine(mTmpDir.FullName, "RegBstkDrv.txt"));

						RunCmd("reg.exe", args, null);
					}
					catch(Exception e)
					{
						Logger.Info("Ignoring Error of BstkDrv since may be case of no plus component has been installed");
						Logger.Error(e.ToString());
					}
#endif

					if (s_ApkInstallFailureLogCollector == true)
					{

						Console.WriteLine("Dumping File listing  :::::::  {0}", DateTime.Now);

						args = String.Format("/c dir \"{0}\" /s", mInstallDir);
						RunCmd("cmd", args, Path.Combine(mTmpDir.FullName, installDirListing));

						args = String.Format("/c dir \"{0}\" /s", mUserDataDir);
						RunCmd("cmd", args, Path.Combine(mTmpDir.FullName, dataDirListing));

						mHostInfoAvailable = true;
						Console.WriteLine("returning after copying log dir log files and appsjson ::::::  {0}", DateTime.Now);
						return;
					}

					try
					{
						Console.WriteLine("Dumping System Information  :::::::  {0}", DateTime.Now);
						RunCmd("SystemInfo", null, Path.Combine(mTmpDir.FullName, "SystemInfo.txt"));
					}
					catch (Exception e)
					{
						Logger.Error(e.ToString());
					}

					try
					{
						string fileName = Path.Combine(mTmpDir.FullName, "FreeDiskSpace.txt");
						DriveInfo[] allDrives = DriveInfo.GetDrives();
						using (System.IO.StreamWriter file = new System.IO.StreamWriter(fileName))
						{
							foreach (DriveInfo d in allDrives)
							{
								file.WriteLine("Drive {0}", d.Name);
								file.WriteLine("  Drive type: {0}", d.DriveType);
								if (d.IsReady == true)
								{
									file.WriteLine("  Volume label: {0}", d.VolumeLabel);
									file.WriteLine("  File system: {0}", d.DriveFormat);
									file.WriteLine("  Available space to current user:{0, 15} bytes", d.AvailableFreeSpace);
									file.WriteLine("  Total available space:          {0, 15} bytes", d.TotalFreeSpace);
									file.WriteLine("  Total size of drive:            {0, 15} bytes ", d.TotalSize);
								}
							}
						}
					}
					catch (Exception e)
					{
						Logger.Error(e.ToString());
					}

					if (s_BootFailureLogs == true)
					{
						Console.WriteLine("Dumping File listing  :::::::  {0}", DateTime.Now);

						args = String.Format("/c dir \"{0}\" /s", mInstallDir);
						RunCmd("cmd", args, Path.Combine(mTmpDir.FullName, installDirListing));

						args = String.Format("/c dir \"{0}\" /s", mUserDataDir);
						RunCmd("cmd", args, Path.Combine(mTmpDir.FullName, dataDirListing));

						mHostInfoAvailable = true;
						Console.WriteLine("returning after copying log dir log files ::::::  {0}", DateTime.Now);
						return;
					}
					string localAppDataDir = Environment.GetFolderPath(
							Environment.SpecialFolder.LocalApplicationData);

					string localAppDataLogDir = Path.Combine(
							localAppDataDir, @"BlueStacks");
					if (Directory.Exists(localAppDataLogDir))
						CopyRecursive(localAppDataLogDir,
								Path.Combine(mTmpDir.FullName, "Installer Logs"));

					string obsLogDir = Path.Combine(Common.Strings.GameManagerDir, @"OBS\logs");
					string obsCrashDumpsDir = Path.Combine(Common.Strings.GameManagerDir, @"OBS\crashDumps");
					if (Directory.Exists(obsLogDir))
					{
						CopyRecursive(obsLogDir, Path.Combine(mTmpDir.FullName, @"OBS\logs"));
					}

					if (Directory.Exists(obsCrashDumpsDir))
					{
						CopyRecursive(obsCrashDumpsDir, Path.Combine(mTmpDir.FullName, @"OBS\crashDumps"));
					}

					string obsLogFile = Path.Combine(Common.Strings.GameManagerDir, @"OBS\OBS.log");
					if (File.Exists(obsLogFile))
					{
						File.Copy(obsLogFile, Path.Combine(mTmpDir.FullName, "OBS.log"));
					}

					string programData = Environment.GetFolderPath(Environment.SpecialFolder.CommonApplicationData);
					string installLogDir = Path.Combine(programData, @"BlueStacksSetup");

					string bstInstallLog = "bstInstall.log";
					string installLogLocation = Path.Combine(installLogDir, bstInstallLog);
					string tmpLogLocation = Path.Combine(mTmpDir.FullName, bstInstallLog);
					if (File.Exists(installLogLocation))
						File.Copy(installLogLocation, tmpLogLocation);

#if __notyet__
					string tmpAppCfgFileLocation = Path.Combine(mTmpDir.FullName, Common.Strings.AppCfgFileName);
					if (File.Exists(Common.Strings.AppCfgFile))
						File.Copy(Common.Strings.AppCfgFile, tmpAppCfgFileLocation);
#endif	// __notyet__

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}

				Console.WriteLine("trying to get host nslookup  :::::::  {0}", DateTime.Now);

				RunCmdInternal("nslookup", "www.google.com",
						Path.Combine(mTmpDir.FullName, "Host-nslookup.txt"));

				Console.WriteLine("trying to get host ipconfig  :::::::  {0}", DateTime.Now);
				RunCmdInternal("ipconfig", "/all",
						Path.Combine(mTmpDir.FullName, "Host-ipconfig.txt"));

				Console.WriteLine("trying to get host netstat  :::::::  {0}", DateTime.Now);
				RunCmdInternal("netstat", "-aon",
						Path.Combine(mTmpDir.FullName, "Host-netstat.txt"));

				Console.WriteLine("trying to get host net statistics workstation  :::::::  {0}", DateTime.Now);
				RunCmdInternal("net", "statistics workstation",
						Path.Combine(mTmpDir.FullName, "Host-netstatistics.txt"));

				UpdateProgress();


				/*
				 * Copy all signature files to the zip
				 */

				try
				{
					string bstAndroidDir = Common.Strings.BstAndroidDir;
					foreach (string file in Directory.GetFiles(bstAndroidDir))
					{
						if (file.EndsWith(".signature"))
						{
							string fileName = Path.GetFileName(file);
							string newPath = Path.Combine(mTmpDir.FullName, fileName);
							File.Copy(file, newPath, true);
						}
					}

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}


				UpdateMessage(Locale.Strings.STATUS_COLLECTING_HOST);



				UpdateProgress();

				Console.WriteLine("Dumping Startup Programs  :::::::  {0}", DateTime.Now);

				try
				{
					path = Path.Combine(mTmpDir.FullName, "Startup.txt");

					DumpStartupPrograms(path);

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}

				Console.WriteLine("Dumping Installed Programs  :::::::  {0}", DateTime.Now);

				try
				{
					path = Path.Combine(mTmpDir.FullName, "InstalledPrograms.txt");

					DumpInstalledPrograms(path);

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}

				string graphicsCheckBinary = "HD-DXCheck.exe";
				args = "2";
				string outputFileName = "CurrentDXCheck.txt";

				try
				{
					using (RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath))
					{
						if (key != null)
						{
							int glRenderMode = (int)key.GetValue(Common.Strings.GLRenderModeKeyName, -1);
							if (glRenderMode == 3)
							{
								args = "3";
							}
							else if (glRenderMode == 1)
							{
								args = null;
								graphicsCheckBinary = "HD-GLCheck.exe";
								outputFileName = "CurrentGLCheck.txt";
							}
						}
					}
				}
				catch (Exception e)
				{
					Logger.Error("Error Occured, Err: {0}", e.ToString());
				}

				Console.WriteLine("Dumping {0} output  :::::::  {1}", graphicsCheckBinary, DateTime.Now);

				try
				{
					path = Path.Combine(mTmpDir.FullName, outputFileName);
					string graphicsCheckBinaryPath = Path.Combine(mInstallDir, graphicsCheckBinary);
					RunCmd(graphicsCheckBinaryPath, args, path);

					string installTimeGraphicsCheckPath = Path.Combine(mUserDataDir, Common.Strings.InstallTimeGraphicsCheckFileName);
					string installTimeGraphicsCheckPathTmp = Path.Combine(mTmpDir.FullName, installTimeGraphicsCheckPath);
					if (File.Exists(installTimeGraphicsCheckPath))
						File.Copy(installTimeGraphicsCheckPath, installTimeGraphicsCheckPathTmp, true);

				}
				catch (Exception exc)
				{


					Console.WriteLine(exc.ToString());
				}

				UpdateProgress();

				Console.WriteLine("Dumping File listing  :::::::  {0}", DateTime.Now);

				args = String.Format("/c dir \"{0}\" /s", mInstallDir);
				RunCmd("cmd", args, Path.Combine(mTmpDir.FullName, installDirListing));

				args = String.Format("/c dir \"{0}\" /s", mUserDataDir);
				RunCmd("cmd", args, Path.Combine(mTmpDir.FullName, dataDirListing));

				args = String.Format("/c dir \"{0}\" /s", Common.Strings.GameManagerDir);
				RunCmd("cmd", args, Path.Combine(mTmpDir.FullName, "GMDirListing.txt"));

				UpdateProgress();

				Console.WriteLine(string.Format("Done CollectHostArtifacts at {0}:{1}",
							DateTime.Now.Minute, DateTime.Now.Second));
				mHostInfoAvailable = true;
			}

			private void DumpPendingGlCalls()
			{
				try
				{
					String regPath = Common.Strings.RegBasePath;
					RegistryKey key = Registry.LocalMachine.OpenSubKey(regPath);
					string installDir = (String)key.GetValue("InstallDir");
					string feTitle = Oem.Instance.CommonAppTitleText;

					IntPtr feHandle = FindWindow(null, feTitle);

					Logger.Info("FE title = {1}, FE handle = {0}", feHandle, feTitle);
					if (feHandle != IntPtr.Zero)
					{
						SetForegroundWindow(feHandle);
						SendKeys.SendWait("^%(G)");
					}
				}
				catch (Exception exp)
				{
					Logger.Error("Error Occured while trying to send alt+ctrl+G keys to Frontend");
				}
			}

			private void DumpServiceInfo()
			{
				try
				{
					EventWaitHandle evt = EventWaitHandle.OpenExisting(
							@"Global\BlueStacks_Core_Service_Info_Event",
							EventWaitHandleRights.Modify);

					evt.Set();
					Thread.Sleep(2000);

				}
				catch (Exception exc)
				{

					Console.WriteLine("Cannot dump service info: :::::::  {0}", DateTime.Now);
				}
			}

			private void CopyRecursive(String srcPath, String dstPath)
			{
				if (!Directory.Exists(dstPath))
					Directory.CreateDirectory(dstPath);

				DirectoryInfo src = new DirectoryInfo(srcPath);

				foreach (FileInfo file in src.GetFiles())
				{
					Console.WriteLine(file.FullName + " {0}", DateTime.Now);
					file.CopyTo(Path.Combine(dstPath, file.Name), true);
				}

				foreach (DirectoryInfo dir in src.GetDirectories())
				{
					Console.WriteLine(dir.FullName + " {0}", DateTime.Now);
					CopyRecursive(Path.Combine(srcPath, dir.Name),
							Path.Combine(dstPath, dir.Name));
				}
			}

			private void DumpProcessList(String outPath)
			{
				ManagementObjectSearcher searcher;
				ManagementObjectCollection list;

				StreamWriter writer = new StreamWriter(outPath);
				String query = "SELECT * FROM Win32_Process";

				searcher = new ManagementObjectSearcher(query);
				list = searcher.Get();

				foreach (ManagementObject obj in list)
				{
					writer.WriteLine("");
					DumpProcess(obj, writer);
				}

				writer.Close();
			}

			private void DumpProcess(ManagementObject o, StreamWriter w)
			{
				w.WriteLine("Name:        " + GetP(o, "Name"));
				w.WriteLine("Path:        " + GetP(o, "ExecutablePath"));
				w.WriteLine("Command:     " + GetP(o, "CommandLine"));
				w.WriteLine("PID:         " + GetP(o, "ProcessId"));
				w.WriteLine("User:        " + GetProcessOwner(o));
				w.WriteLine("Session:     " + GetP(o, "SessionId"));
				w.WriteLine("Threads:     " + GetP(o, "ThreadCount"));
				w.WriteLine("Handles:     " + GetP(o, "HandleCount"));
				w.WriteLine("Memory (KB): " +
						(Int64.Parse(GetP(o, "WorkingSetSize")) / 1024));
				w.WriteLine("Peak (KB):   " + GetP(o, "PeakWorkingSetSize"));
				w.WriteLine("User (ms):   " +
						(Int64.Parse(GetP(o, "UserModeTime")) / 10000));
				w.WriteLine("Kernel (ms): " +
						(Int64.Parse(GetP(o, "KernelModeTime")) / 10000));
			}

			private String GetP(ManagementObject obj, String name)
			{
				Object val = obj.GetPropertyValue(name);
				if (val == null)
					return "";

				return val.ToString();
			}

			private String GetProcessOwner(ManagementObject obj)
			{
				try
				{
					String[] args = new String[] { "", "" };
					int val = Convert.ToInt32(obj.InvokeMethod("GetOwner", args));
					if (val == 0)
						return args[1] + @"\" + args[0];
				}
				catch (Exception e)
				{
					Logger.Error(string.Format("Error Occured, Err: {0}", e.ToString()));
				}

				return "";
			}

			private void DumpStartupPrograms(String outPath)
			{
				ManagementClass mangnmt = new ManagementClass("Win32_StartupCommand");

				ManagementObjectCollection mcol = mangnmt.GetInstances();

				StreamWriter writer = new StreamWriter(outPath);
				foreach (ManagementObject strt in mcol)
				{
					writer.WriteLine("Application Name: " + strt["Name"].ToString());
					writer.WriteLine("Application Location: " + strt["Location"].ToString());
					writer.WriteLine("Application Command: " + strt["Command"].ToString());
					writer.WriteLine("User: " + strt["User"].ToString());
					writer.WriteLine("");
				}

				writer.Close();
			}

			private void DumpInstalledPrograms(String outPath)
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(@"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall");
				string[] uninstallKeys = key.GetSubKeyNames();
				RegistryKey uninstallKey;

				StreamWriter writer = new StreamWriter(outPath);
				foreach (string program in uninstallKeys)
				{
					uninstallKey = key.OpenSubKey(program);
					writer.WriteLine("Key: " + program);
					Object name = uninstallKey.GetValue("DisplayName");
					Object version = uninstallKey.GetValue("DisplayVersion");
					if (name != null)
					{
						writer.WriteLine("Application Name: " + (string)name);
						writer.WriteLine("Application Version: " + (string)version);
					}
					writer.WriteLine("");
				}

				writer.Close();
			}

			private void DumpEventLogs(String name, String outPath)
			{
				StreamWriter writer = new StreamWriter(outPath);
				EventLog log = new EventLog(name);
				int iTotalCount = log.Entries.Count;
				int iOffset = 0;
				if (iTotalCount > 2000)
				{
					iOffset = iTotalCount - 2000;
				}

				for (int index = iOffset; index < iTotalCount; index++)
				{
					writer.WriteLine("Event[{0}]:", index);
					writer.WriteLine(" Log Name: {0}", name);
					writer.WriteLine(" Source: {0}", log.Entries[index].Source);
					writer.WriteLine(" Date: {0}", log.Entries[index].TimeGenerated);
					writer.WriteLine(" Event ID: {0}", log.Entries[index].InstanceId);
					writer.WriteLine(" User: {0}", log.Entries[index].UserName);
					writer.WriteLine(" Description:");
					writer.WriteLine("{0}", log.Entries[index].Message);
					writer.WriteLine("");
				}
				writer.Close();
			}

			private void DumpHotfixInfo(String outPath)
			{
				ManagementObjectSearcher searcher;
				ManagementObjectCollection list;

				StreamWriter writer = new StreamWriter(outPath);
				String query = "SELECT * FROM Win32_QuickFixEngineering";

				searcher = new ManagementObjectSearcher(query);
				list = searcher.Get();

				writer.WriteLine("{0} Hotfixes Applied", list.Count);

				foreach (ManagementObject obj in list)
				{
					DumpHotfixDetails(obj, writer);
				}

				writer.Close();
			}

			private void DumpHotfixDetails(ManagementObject obj,
					StreamWriter writer)
			{
				writer.WriteLine("{0} - {1}",
						GetP(obj, "HotFixID"), GetP(obj, "Description"));
			}

			private void CreateZipFile()
			{
				Console.WriteLine("Creating zip file  :::::::  {0}", DateTime.Now);
				String prog, curDir, dst;

				UpdateMessage(Locale.Strings.STATUS_ARCHIVING);

				curDir = Environment.CurrentDirectory;
				Environment.CurrentDirectory = mTmpDir.FullName;

				dst = Path.Combine(s_DestinationFolder, Locale.Strings.ZIP_NAME);

				try
				{
					prog = Path.Combine(mInstallDir, "HD-zip.exe");

					RunCmd(prog, "-r archive.zip *", null);

					if (File.Exists(dst))
						File.Delete(dst);

					File.Move("archive.zip", dst);
					Console.WriteLine("Zip at {0}", dst);

				}
				finally
				{
					Environment.CurrentDirectory = curDir;
					Directory.Delete(mTmpDir.FullName, true);
				}
			}

			private void UploadZipFile(String email, String category, String appName, String desc, String subcategory)
			{
				UpdateMessage(Locale.Strings.STATUS_SENDING);

				String zip = Path.Combine(Path.GetTempPath(), Locale.Strings.ZIP_NAME);
				if (String.IsNullOrEmpty(s_DestinationFolder) == false)
				{
					zip = Path.Combine(s_DestinationFolder, Locale.Strings.ZIP_NAME);
				}
				String url = String.Format("{0}/{1}", Service.Host, Common.Strings.UploadDebugLogsUrl);
				if (s_ApkInstallFailureLogCollector == true)
				{
					url = String.Format("{0}/{1}", Service.Host, Common.Strings.UploadDebugLogsApkInstallFailureUrl);
				}
				else if (s_BootFailureLogs == true)
				{
					url = String.Format("{0}/{1}", Service.Host, Common.Strings.UploadDebugLogsBootFailureUrl);
				}
				else if (s_CrashLogs == true)
				{
					url = String.Format("{0}/{1}", Service.Host, Common.Strings.UploadDebugLogsCrashUrl);
				}

				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("email", email);
				if (category != null)
					data.Add("category", category);
				if (subcategory != null)
				{
					data.Add("subcategory", subcategory);
				}
				if (appName != null && appName.Length > 0)
					data.Add("app", appName);
				data.Add("desc", desc);
				data.Add("culture", CultureInfo.CurrentCulture.Name.ToLower());

				if (s_BootFailureLogs == true)
				{
					data.Add("error", mErrorReason);
					data.Add("ecode", mErrorCode);

					RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMAndroidConfigRegKeyPath);
					bool firstBoot = ((int)key.GetValue("ConfigSynced", 0)) == 0 ? true : false;
					data.Add("firstboot", firstBoot.ToString());
				}
				else if (s_ApkInstallFailureLogCollector == true)
				{
					data.Add("error", mErrorReason);
					data.Add("ecode", mErrorCode);
					data.Add("apk", s_InstallFailedApkName);
				}
				else if (s_CrashLogs == true)
				{
					Logger.Info("the error reason is {0}", mErrorReason);
					data.Add("crash_type", mErrorReason);
					data.Add("ecode", mErrorCode);
					data.Add("package", mErrorDetails);
				}

				try
				{
					string version = User.RegVersion;
					if (version == null)
					{
						Console.WriteLine("Version string is empty  :::::::  {0}", DateTime.Now);
					}
					string res = BlueStacks.hyperDroid.Common.HTTP.Client.HTTPGaeFileUploader(
							url, data, null, zip, "application/zip", false, version);
					mUploadSuccessful = true;
					File.Delete(zip);
				}
				catch
				{
					mUploadSuccessful = false;
					String dst = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Desktop),
							Locale.Strings.ZIP_NAME);

					if (File.Exists(dst))
						File.Delete(dst);

					File.Move(zip, dst);
				}
			}

			private void ShowFinish()
			{
				SendOrPostCallback cb = new SendOrPostCallback(
						delegate(Object obj)
						{
						this.Hide();
						if (BlueStacks.hyperDroid.Common.Oem.Instance.IsHideMessageBoxIconInTaskBar)
						{
						if (mUploadSuccessful)
						MessageBox.Show(new Form(), Locale.Strings.FINISH_TEXT,
							Locale.Strings.FINISH_CAPT, MessageBoxButtons.OK);
						else
						MessageBox.Show(new Form(), Locale.Strings.PROMPT_TEXT,
							Locale.Strings.FINISH_CAPT, MessageBoxButtons.OK);
						}
						else
						{
						if (mUploadSuccessful)
						MessageBox.Show(Locale.Strings.FINISH_TEXT,
							Locale.Strings.FINISH_CAPT, MessageBoxButtons.OK);
						else
						MessageBox.Show(Locale.Strings.PROMPT_TEXT,
							Locale.Strings.FINISH_CAPT, MessageBoxButtons.OK);
						}
						});

				try
				{
					mUiContext.Send(cb, null);
				}
				catch (Exception)
				{
				}
			}

			private void RunCmd(String prog, String args, String outPath)
			{
				try
				{
					RunCmdInternal(prog, args, outPath);

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}
			}

			private void RunCmdWithList(String prog, String[] argList,
					String outPath)
			{
				try
				{
					List<String> quoted = new List<String>();

					foreach (String arg in argList)
						quoted.Add(String.Format("\"{0}\"", arg));

					RunCmd(prog, String.Join(" ", quoted.ToArray()),
							outPath);

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}
			}

			private void RunCmdWithList(String prog, String[] argList)
			{
				RunCmdWithList(prog, argList, null);
			}

			private void RunCmdInternal(String prog, String args, String outPath)
			{
				StreamWriter writer = null;
				Process proc = new Process();

				Console.WriteLine("Running Command  :::::::  {0}", DateTime.Now);
				Console.WriteLine("    prog: " + prog);
				Console.WriteLine("    args: " + args);
				Console.WriteLine("    out:  " + outPath);

				proc.StartInfo.FileName = prog;
				proc.StartInfo.Arguments = args;

				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.CreateNoWindow = true;

				if (outPath != null)
				{
					writer = new StreamWriter(outPath);

					proc.StartInfo.RedirectStandardInput = true;
					proc.StartInfo.RedirectStandardOutput = true;
					proc.StartInfo.RedirectStandardError = true;

					proc.OutputDataReceived += delegate(object obj,
							DataReceivedEventArgs line)
					{
						writer.WriteLine(line.Data);
					};

					proc.ErrorDataReceived += delegate(object obj,
							DataReceivedEventArgs line)
					{
						writer.WriteLine(line.Data);
					};
				}

				proc.Start();

				if (outPath != null)
				{
					proc.BeginOutputReadLine();
					proc.BeginErrorReadLine();
				}

				proc.WaitForExit();

				if (outPath != null)
				{
					writer.Close();
				}
			}

			private void RunCmdAsync(String prog, String args)
			{
				try
				{
					RunCmdAsyncInternal(prog, args);

				}
				catch (Exception exc)
				{

					Console.WriteLine(exc.ToString());
				}
			}

			private void RunCmdAsyncInternal(String prog, String args)
			{
				Process proc = new Process();

				Console.WriteLine("Running Command Async  :::::::  {0}", DateTime.Now);
				Console.WriteLine("    prog: " + prog);
				Console.WriteLine("    args: " + args);

				proc.StartInfo.FileName = prog;
				proc.StartInfo.Arguments = args;

				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.CreateNoWindow = true;

				proc.Start();
			}

			private void UpdateMessage(String msg)
			{
				SendOrPostCallback cb = new SendOrPostCallback(
						delegate(Object obj)
						{
						mStatusControl.Text = msg;
						});

				try
				{
					mUiContext.Send(cb, null);
				}
				catch (Exception)
				{
				}
			}

			private void UpdateProgress()
			{
				SendOrPostCallback cb = new SendOrPostCallback(
						delegate(Object obj)
						{
						mProgressControl.Increment(1);
						});

				try
				{
					mUiContext.Send(cb, null);
				}
				catch (Exception)
				{
				}
			}
		}

		public class ComboboxItem
		{
			private string text;
			public string Text
			{
				get
				{
					return text;
				}
				set
				{
					text = value;
				}
			}

			private string itemValue;
			public string Value
			{
				get
				{
					return itemValue;
				}
				set
				{
					itemValue = value;
				}
			}

			public override string ToString()
			{
				return Text;
			}
		}

		public class AppInfo
		{
			public string name, package, version;

			public AppInfo(IJSonObject app)
			{
				try
				{
					name = app["appname"].StringValue;
					package = app["package"].StringValue;
					version = app["version"].StringValue;
				}
				catch
				{
					name = "";
					package = "";
					version = "";
				}
			}

			public AppInfo(string InName, string InPackage, string InVersion)
			{
				name = InName;
				package = InPackage;
				version = InVersion;
			}
		}
	}
}
