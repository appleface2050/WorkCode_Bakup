using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Net;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text.RegularExpressions;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

using CodeTitans.JSon;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;

namespace BlueStacks.hyperDroid.ApkInstaller {

public class HDApkInstaller : Form {

	private static string	s_InstallPath		= "install";
	private static string	s_InstallDir		= null;
	const string		logName 		= "HD-ApkHandler";
	private ProgressBar	m_ProgressBar;
	private Label		m_Label;
	private static Mutex 	s_HDApkInstallerLock;
	private const int	PROC_KILL_TIMEOUT	= 10 * 1000;	/* 10 sec */
	private static string unUsed;

	private static string	s_AppName		= "";
	private static string	s_AppIcon		= "";
	private static string	s_AppPackage		= "";
	private static string	s_ApkPath		= "";
	private static bool 	s_IsSilent=false;
	
	private static int		s_AgentPort;
	private static string		s_UninstallPath	= "uninstall";

	private static DateTime sApkHandlerLaunchTime;

	private static Dictionary<string, string>	data		= new Dictionary<string, string>();

	public static Dictionary<String, String[]> sOemWindowMapper;


	public class Opt : GetOpt
	{
		public bool s=false;//silent
		public bool u=false;//for uninstall
		public String apk="";//apk path
		public String name="";//appName
		public String p="";//packageName
	}

	private static class VMChooser
	{
		public static string vmName = "Android";
		private static ComboBox vmComboBox;
		private static Form vmForm;

		public static string[] ListVMs()
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GuestRegKeyPath, true);
			string[] vmList = (string[])key.GetValue("VMList", null);
			if (vmList == null)
			{
				vmList = new string[] {"Android:Android"};
				key.SetValue("VMList", vmList, RegistryValueKind.MultiString);
				key.Close();
			}
			string[] displayNames = new string[vmList.Length];
			for (int i = 0; i < vmList.Length; ++i)
			{
				displayNames[i] = vmList[i].Split(new char[]{':'})[0];
			}	
			return displayNames;
		}

		private static void ButtonClick(Object sender, EventArgs e)
		{
		       	vmName= vmComboBox.SelectedItem.ToString();
		       	vmForm.Close();
		}

		public static void ChooseVM()
		{
#if !MULTI_INS
			return;
#endif
			vmForm = new Form();
			vmForm.FormBorderStyle = FormBorderStyle.FixedSingle;
			vmForm.Text = "Choose Machine..";
			vmForm.StartPosition = FormStartPosition.CenterParent;
			vmComboBox = new ComboBox();
			Button installButton = new Button();
			vmComboBox.Location = new System.Drawing.Point(49, 59);
			vmComboBox.Size = new System.Drawing.Size(197, 28);
			vmComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
			installButton.Location = new System.Drawing.Point(69, 127);
			installButton.Size = new System.Drawing.Size(143, 49);
			installButton.Text = "Install";
			installButton.Click += new EventHandler(ButtonClick);
			vmForm.AutoScaleDimensions = new System.Drawing.SizeF(9F, 20F);
			vmForm.ClientSize = new System.Drawing.Size(290, 225);
            		vmForm.Controls.Add(installButton);
            		vmForm.Controls.Add(vmComboBox);
	    		vmForm.MaximizeBox = false;
			foreach (string name in ListVMs())
				vmComboBox.Items.Add(name);
			vmComboBox.SelectedIndex = 0;
			vmForm.ShowDialog();
		}
	}


	public static void Main(string[] args)
	{
		Logger.InitUserLog();
		Locale.Strings.InitLocalization(null);
		InitExceptionHandlers();
		Utils.LogParentProcessDetails();
		Opt opt = new Opt();
		opt.Parse(args);

		//Assigning silent argument if found while parsing
		s_IsSilent = opt.s;

		string exePath = System.Reflection.Assembly.GetEntryAssembly().Location;
		Logger.Info("the exe path is " + exePath);
		string folderPath = Directory.GetParent(exePath).ToString();

		if(Features.IsFeatureEnabled(Features.MULTI_INSTANCE_SUPPORT))
		{
			Logger.Info("calling HD-QuitMultiInstance to kill other instances services and processes if running");
			Utils.QuitMultiInstance(folderPath);
		}
		
		VMChooser.ChooseVM();
		String installVM = VMChooser.vmName;
		Logger.Info("VM to be installed is " + installVM);
		if (Common.Utils.IsAlreadyRunning(Common.Strings.GetHDApkInstallerLockName(installVM), out s_HDApkInstallerLock))
		{
			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMessageBoxToBeDisplayed)
			{
				String title = BlueStacks.hyperDroid.Common.Oem.Instance.BlueStacksApkHandlerTitle;
				Form apkHandler = new HDApkInstaller(null, "");
				apkHandler.Show();
				MessageBox.Show(apkHandler, Locale.Strings.ApkHandlerAlreadyRunning, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
				apkHandler.Close();
			}
			Logger.Info("HD-ApkHandler already running");
			if(Features.IsFeatureEnabled(Features.COLLECT_APK_HANDLER_LOGS))
			{
				StartLogCollection((int)InstallerCodes.PROCESS_ALREADY_RUNNING, "PROCESS_ALREADY_RUNNING", "");
			}
			Environment.Exit((int)InstallerCodes.PROCESS_ALREADY_RUNNING);
		}

		Logger.Info("IsAdministrator: {0}", User.IsAdministrator());

		Logger.Debug("pkg name = " + opt.p);
		Logger.Debug("app name = " + opt.name);
		Logger.Debug("silentmode = " + opt.s);
		Logger.Debug("apk = " + opt.apk);
		Logger.Debug("uninstallmode = " + opt.u);

		sApkHandlerLaunchTime = DateTime.Now;

		Application.EnableVisualStyles();

		ServicePointManager.ServerCertificateValidationCallback += new RemoteCertificateValidationCallback(ValidateRemoteCertificate);

		//assigning the parsed arguments
		s_AppName = opt.name;
		s_AppPackage = opt.p;
		s_ApkPath = opt.apk;
		if (String.IsNullOrEmpty(s_ApkPath) == false &&
				Path.IsPathRooted(s_ApkPath) == false)
		{
			s_ApkPath = Path.Combine(Directory.GetCurrentDirectory(),
					s_ApkPath);
		}
		
		if(!opt.u) //is in InstallMode
		{
			Logger.Debug("in Install mode");
			
			//for backward compatibility (for older arguments types)
			if(args.Length >= 1 && s_ApkPath.Equals("")) //i.e. if no. of arguments are greater than or equal to 1 and GetOpt fails to recognize the ApkPath
			{
				Logger.Debug("ApkHandler called with older types of arguments");
				s_ApkPath	= args[0];
				if (String.IsNullOrEmpty(s_ApkPath) == false &&
						Path.IsPathRooted(s_ApkPath) == false)
				{
					s_ApkPath = Path.Combine(Directory.GetCurrentDirectory(),
							s_ApkPath);
				}
				if(args.Length == 2)
					s_IsSilent = args[1].Equals("silent")? true : false ;
			}

			if (File.Exists(s_ApkPath) == false)
			{
				Logger.Info("Exiting with exit code {0}", (int)InstallerCodes.APK_FILE_NOT_FOUND);
				if(Features.IsFeatureEnabled(Features.COLLECT_APK_HANDLER_LOGS))
					StartLogCollection((int)InstallerCodes.APK_FILE_NOT_FOUND, "File not found", GetApkNameFromPath(s_ApkPath));

				Environment.Exit((int)InstallerCodes.APK_FILE_NOT_FOUND);
			}

			HDApkInstaller obj = new HDApkInstaller(s_ApkPath, installVM);
			if(!s_IsSilent)
				Application.Run(obj);
		}
		else	//is in apktoexeUninstall Mode
		{
			try{
				RegistryKey configKey		= Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
				s_AgentPort			= (int)configKey.GetValue("AgentServerPort", 2861);


				if(s_AppPackage != "")
					JsonParser.GetAppInfoFromPackageName(s_AppPackage, out s_AppName, out s_AppIcon, out unUsed,out unUsed);
				else if(s_AppName != "")
				{
					JsonParser.GetAppInfoFromAppName(s_AppName, out s_AppPackage, out s_AppIcon, out unUsed);
					CleanUpUninstallEntry();
				}

				if(String.IsNullOrEmpty(s_AppPackage) == true)
				{
					Logger.Error("PackageName can not be null for uninstalling an app");
				}
				else
				{
					int ret = 0;
					bool isSystemApp = JsonParser.IsPackageNameSystemApp(s_AppPackage);
					if (isSystemApp)
					{
						MessageBox.Show("Uninstalling a pre-bundled app is not supported.",
								"BlueStacks Error",
								MessageBoxButtons.OK,
								MessageBoxIcon.Error);
					}
					else
					{
						data.Clear();
						data.Add("package", s_AppPackage);
						data.Add("name", s_AppName);

						string url = String.Format("http://127.0.0.1:{0}/{1}", s_AgentPort, s_UninstallPath);
						Logger.Info("RunApp: Sending post request to {0}", url);
						string res = Common.HTTP.Client.PostWithRetries(url, data, null, false, 10, 500, installVM);

						JSonReader reader = new JSonReader();
						IJSonObject resultJson = reader.ReadAsJSonObject(res);
						bool success = resultJson[0]["success"].BooleanValue;
						if (success)
							ret = 0;
						else
							ret = 1;
					}
					Environment.Exit(ret);
				}
			}
			catch(Exception exc)
			{
				Logger.Error("Got Exception");
				Logger.Error(exc.ToString());
				CleanUpUninstallEntry();
			}
		}
	}
	
	private static bool ValidateRemoteCertificate(object sender, X509Certificate cert, X509Chain chain, SslPolicyErrors policyErrors)
	{
		return true;
	}

	public static void CleanUpUninstallEntry()
	{
		bool		silentUninstall	= false;
		string		entrySuffix = s_AppName;	
		string		path		= Common.Strings.BstPrefix + entrySuffix;

		Logger.Info("Cleaning up uninstall entry for {0}", entrySuffix);

		RegistryKey	key		= Registry.LocalMachine.CreateSubKey(Common.Strings.UninstallKey);

		try
		{
			string		bstPath		= Common.Strings.UninstallKey + "\\" + path;
			RegistryKey	bstKey		= Registry.LocalMachine.OpenSubKey(bstPath);
			string		silent		= (string)bstKey.GetValue("Silent");
			if (silent == "yes")
			{
				silentUninstall = true;
			}
		}
		catch (Exception)
		{
			// Ignore
		}


		Logger.Info("Key: " + key.ToString());
		key.DeleteSubKeyTree(path);
		key.Close();

		string	desktopDir		= Environment.GetFolderPath(Environment.SpecialFolder.Desktop);
		string	shortcutName		= entrySuffix + ".lnk";
		string	shortcutFilePath	= Path.Combine(desktopDir, shortcutName);
		string	startMenuPath		= Environment.GetFolderPath(Environment.SpecialFolder.StartMenu);
		startMenuPath			= Path.Combine(startMenuPath, Common.Strings.LibraryName);
		string	startMenuShortcut	= Path.Combine(startMenuPath, shortcutName);

		try
		{
			Logger.Info("Deleting shortcut file: " + shortcutFilePath);
			File.Delete(shortcutFilePath);
			Logger.Info("Deleting shortcut file: " + startMenuShortcut);
			File.Delete(startMenuShortcut);
		}
		catch (Exception ex)
		{
			Logger.Error("Failed to remove shortcut entry. err: " + ex.ToString());
			// Ignore
		}

		if (!silentUninstall)
		{
			MessageBox.Show(entrySuffix + " has been uninstalled.",
					"App Player",
					MessageBoxButtons.OK,
					MessageBoxIcon.Information);
		}
	}

	private static string GetVMName(string displayName)
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.GuestRegKeyPath, false);
			string[] vmList = (string[])key.GetValue("VMList", null);
			for (int i = 0; i < vmList.Length; ++i)
				if (displayName == vmList[i].Split( new char[] {':'} )[0])
					return vmList[i].Split(new char[] {':'})[1];
			return null;
		}

	private void InstallApk(Object apk, string vmName)
	{
		string apkPath = (string) apk;
		RegistryKey HKLMregistry = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		s_InstallDir = (string)HKLMregistry.GetValue("InstallDir");
		Logger.Info("HDApkInstaller: Installing {0}", apkPath);

		Dictionary<string, string> data = new Dictionary<string, string>();
		data.Add("path", apkPath);

		Dictionary<string, string> headers = new Dictionary<string, string>();
		if (!vmName.Equals("Android"))
		{
			headers.Add("vmid", vmName.Split(new char[] {'_'})[1]);
		}
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
		int port = (int)key.GetValue("AgentServerPort", 2861);

		string url = String.Format("http://127.0.0.1:{0}/{1}", port, s_InstallPath);
		Logger.Info("HDApkInstaller: Sending post request to {0}", url);
		string res = "";
		try
		{
			if (Utils.IsProcessAlive(Common.Strings.HDAgentLockName) == false)
			{
				Process proc = new Process();
				proc.StartInfo.UseShellExecute = false;
				proc.StartInfo.CreateNoWindow = true;
				proc.StartInfo.FileName = Path.Combine(s_InstallDir, "HD-Agent.exe");
				Logger.Info("Utils: Starting Agent");
				proc.Start();
				bool bootComplete = Utils.WaitForAgentPingResponse();
				if (bootComplete == false)
				{
					Logger.Info("Exiting with exit code {0}", (int)InstallerCodes.AGENT_SERVER_NOT_RUNNING);
					if(Features.IsFeatureEnabled(Features.COLLECT_APK_HANDLER_LOGS))
						StartLogCollection((int)InstallerCodes.AGENT_SERVER_NOT_RUNNING, "Agent or agent-server not running", apkPath);

					Environment.Exit((int)InstallerCodes.AGENT_SERVER_NOT_RUNNING);
				}
			}
			res = Common.HTTP.Client.Post(url, data, headers, false, 10 * 60 * 1000);
		}
		catch (WebException e)
		{
			ReleaseApkInstallThread();
			Logger.Error("WebException in install request");
			Logger.Error(e.ToString());
			Logger.Error("WebException Response", e.Response);
			Logger.Info("Exiting with exit code {0}", (int)InstallerCodes.INSTALL_FAILED_APKHANDLER_WEBEXCEPTION);
			if(Features.IsFeatureEnabled(Features.COLLECT_APK_HANDLER_LOGS))
			{
				StartLogCollection(
						(int)InstallerCodes.INSTALL_FAILED_APKHANDLER_WEBEXCEPTION,
						string.Format("status = {0}, error = {1}", e.Status, e.Message),
						apkPath);
			}

			Environment.Exit((int)InstallerCodes.INSTALL_FAILED_APKHANDLER_WEBEXCEPTION);
		}
		catch (Exception e)
		{
			ReleaseApkInstallThread();
			Logger.Error("Exception in install request");
			Logger.Error(e.ToString());
			Logger.Info("Exiting with exit code {0}", (int)InstallerCodes.INSTALL_FAILED_APKHANDLER_EXCEPTION);
			if(Features.IsFeatureEnabled(Features.COLLECT_APK_HANDLER_LOGS))
				StartLogCollection((int)InstallerCodes.INSTALL_FAILED_APKHANDLER_EXCEPTION, e.Message, apkPath);

			Environment.Exit((int)InstallerCodes.INSTALL_FAILED_APKHANDLER_EXCEPTION);
		}
		JSonReader reader = new JSonReader();
		IJSonObject resultJson = reader.ReadAsJSonObject(res);
		string result = resultJson["reason"].StringValue.Trim();
		InstallerCodes retCode = InstallerCodes.SUCCESS_CODE;

		try {
			retCode = (InstallerCodes)Enum.Parse(typeof(InstallerCodes), result);
		} catch {
			Logger.Error("HDApkInstaller: Failed to recognize Installer Codes : " + result);
			if(Features.IsFeatureEnabled(Features.COLLECT_APK_HANDLER_LOGS))
				StartLogCollection((int)InstallerCodes.INSTALL_FAILED_ERROR_AT_GUEST, result, apkPath);

			Logger.Info("Exiting with exit code {0}", (int)InstallerCodes.INSTALL_FAILED_ERROR_AT_GUEST);
			Environment.Exit((int)InstallerCodes.INSTALL_FAILED_ERROR_AT_GUEST);
		}
		
		if (retCode == InstallerCodes.INSTALL_FAILED_INSUFFICIENT_STORAGE_HOST)
		{
			Logger.Error("HDApkInstaller: Installation failed, disk space insufficient in host");
			Logger.Info("Exiting with exit code {0}", (int)InstallerCodes.INSTALL_FAILED_INSUFFICIENT_STORAGE_HOST);
			Environment.Exit((int)InstallerCodes.INSTALL_FAILED_INSUFFICIENT_STORAGE_HOST);
		}

		if (retCode == InstallerCodes.Success)
		{
			Logger.Info("HDApkInstaller: Installation Successful");
			string msg = "Apk" + " " + Locale.Strings.InstallSuccess;

			/*
			if(!s_IsSilent)
				MessageBox.Show(msg, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
			*/
			Logger.Info("HDApkInstaller: Exit with code 0");
			Environment.Exit(0);
		}
		else
		{
			Logger.Info("HDApkInstaller: Installation Failed");
			Logger.Info("HDApkInstaller: Got Error: {0}", result);

			if(!s_IsSilent)
			{
				string errormsg = "Apk" + " " + Locale.Strings.InstallFail + ": " + retCode;
				MessageBox.Show(errormsg, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
			}

			if(Features.IsFeatureEnabled(Features.COLLECT_APK_HANDLER_LOGS))
			{
				StartLogCollection((int)retCode, result, apkPath);

				if(retCode == InstallerCodes.ANDROID_BOOT_FAILURE)
				{
					string reason = "";
					int exitCode = -1;
					try
					{
						if (Utils.IsBootFailureReasonknown(sApkHandlerLaunchTime, out reason, out exitCode) == true)
						{
							Logger.Info("Reason for not being able to boot: {0}", reason);
						}
						Utils.AddMessagingSupport(out sOemWindowMapper);
						Common.Utils.NotifyBootFailureToParentWindow(sOemWindowMapper[Oem.Instance.OEM][0], sOemWindowMapper[Oem.Instance.OEM][1], exitCode);
					}
					catch(Exception ex)
					{
						Logger.Error("caught exception in checking reason for android boot failure ex : {0}", ex.ToString());
					}
				}
			}
			Logger.Info("Exiting with exit code {0}", (int)retCode);
			Environment.Exit((int)retCode);
		}
	}

	private void ReleaseApkInstallThread()
	{
		if(Common.Utils.IsProcessAlive(Common.Strings.HDAgentLockName))
		{
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.HKLMConfigRegKeyPath);
			int port = (int)key.GetValue("AgentServerPort", 2861);
			string releaseApkInstall = "releaseApkInstallThread";
			string url = String.Format("http://127.0.0.1:{0}/{1}", port, releaseApkInstall);
			Common.HTTP.Client.Post(url, null, null, false);
		}

	}

	private static void StartLogCollection(int errorCode, string errorReason, string apkPath)
	{
		Logger.Info("starting the logging of apk installation failure");
		Process proc =  new Process();
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		string installDir = (string)key.GetValue("InstallDir");
		proc.StartInfo.FileName = Path.Combine(installDir, "HD-LogCollector.exe");
		string apkName = GetApkNameFromPath(apkPath);
		string arguments = "-apk" + " " + errorCode + " " + AddQuotes(errorReason) + " " + AddQuotes(apkName);
		Logger.Info("The arguments being passed to log collector is :{0}", arguments);
		proc.StartInfo.Arguments = arguments;
		proc.Start();
	}

	private static string AddQuotes(string value)
	{
		return "\"" + value + "\"";
	}

	private static String GetApkNameFromPath(string apkPath)
	{
		int index = apkPath.LastIndexOf('\\') + 1;
		int length = apkPath.Length - index;
		return apkPath.Substring(index, length);
	}

	private static void InitExceptionHandlers()
	{
		Application.ThreadException += delegate(Object obj,
				System.Threading.ThreadExceptionEventArgs evt)
		{
			StartLogCollection(-1, "Unhandled Exception:", "");
			Logger.Error("HDApkInstaller: Unhandled Exception:");
			Logger.Error(evt.Exception.ToString());
			Environment.Exit(-1);
		};

		Application.SetUnhandledExceptionMode(
				UnhandledExceptionMode.CatchException);

		AppDomain.CurrentDomain.UnhandledException += delegate(
				Object obj, UnhandledExceptionEventArgs evt)
		{
			StartLogCollection(-1, "Unhandled Exception:", "");
			Logger.Error("HDApkInstaller: Unhandled Exception:");
			Logger.Error(evt.ExceptionObject.ToString());
			Environment.Exit(-1);
		};
	} 

	private HDApkInstaller(string apk, string vmName)
	{
		Common.Interop.Window.FreeConsole();

		if(!s_IsSilent)
			InitializeComponents();

		if (apk != null)
			Install(apk, vmName);
	}

	private void Install(string apk, string vmName)
	{
		ThreadStart ts = delegate { InstallApk(apk, vmName); };
		Thread installApk = new Thread(ts) ;
		installApk.Start();
	}

        private void InitializeComponents()
        {
		int height = 70;
		int width = 220;

		SuspendLayout();
		this.StartPosition 	= FormStartPosition.CenterScreen;
		this.Icon			= Utils.GetApplicationIcon();
		this.Text			= BlueStacks.hyperDroid.Common.Oem.Instance.BlueStacksApkHandlerTitle;
		this.SizeGripStyle 	= SizeGripStyle.Hide;
		this.ShowIcon		= true;
		this.MaximizeBox	= false;
		this.MinimizeBox	= false;
		this.ShowInTaskbar	= true;
		this.FormBorderStyle	= FormBorderStyle.FixedDialog;
                this.ClientSize 	= new System.Drawing.Size(width,height);
	
		m_Label = new Label();
		// m_Label
		m_Label.Location = new System.Drawing.Point(width/4,5);
		m_Label.Size = new System.Drawing.Size(width, 35);
		m_Label.Text = Locale.Strings.UserWaitText;

		m_ProgressBar = new ProgressBar();
		// m_ProgressBar
		m_ProgressBar.Location = new System.Drawing.Point(width/4, 40);
		m_ProgressBar.Size = new System.Drawing.Size(width/2, 20);
		m_ProgressBar.Style = ProgressBarStyle.Marquee;
		m_ProgressBar.MarqueeAnimationSpeed = 25;

                // HDApkInstaller
		Controls.Add(this.m_Label);
                Controls.Add(this.m_ProgressBar);
                ResumeLayout(false);
		PerformLayout();

		Logger.Info("HDApkInstaller: Components Initialized");
	}

	protected override void OnClosing(CancelEventArgs evt)
	{
		Environment.Exit((int)InstallerCodes.USER_EXITED);
	}

}

}

