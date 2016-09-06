using System;
using System.IO;
using System.Collections.Generic;
using System.Diagnostics;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.Agent {
public class AppUninstaller {

	public static void AppUninstalled(string packageName, string vmName, string source)
	{
		string name = RemoveFromJson(packageName);
		if (BlueStacks.hyperDroid.Common.Oem.Instance.IsPostUrlOnAppUninstalled)
		{
			try
			{
				RemoveFromGameManagerJson(packageName);

				int port = Common.Utils.GetPartnerServerPort();

				string url = string.Format("http://127.0.0.1:{0}/{1}",
						port, Common.Strings.AppUninstalledUrl);

				Dictionary<string, string> data = new Dictionary<string, string>();
				data.Add("package", packageName);

				Logger.Info("Sending request to: " + url);
				Common.HTTP.Client.Post(url, data, null, false);
			}
			catch (Exception exc)
			{
				Logger.Error("Exception in AppUninstalled: " + exc.ToString());
			}
		}

		Logger.Info("Sending App Install stats");
		string version = HDAgent.GetVersionFromPackage(packageName, vmName);
		Common.Stats.SendAppInstallStats(name, packageName, version, Common.Stats.AppUninstall, "false", source);

		if (name == "")
			name = packageName;

		String uninstallMsg = String.Format("{0} {1}", name, Locale.Strings.UninstallSuccess);
		//SysTray.ShowUninstallAlert(Locale.Strings.BalloonTitle, uninstallMsg);
		if (Common.Features.IsFeatureEnabled(Common.Features.UNINSTALL_NOTIFICATIONS))
		{
			SysTray.ShowInfoShort(Locale.Strings.BalloonTitle, uninstallMsg);
		}
	}

	public static int SilentUninstallApp(string appName, string package, bool nolookup, string vmName, out string reason)
	{
		reason = "";
		s_originalJson = JsonParser.GetAppList();
		string image, name, activity, appstore;

		Logger.Info("nolookup: " + nolookup);
		if (!nolookup)
		{
			JsonParser.GetAppInfoFromPackageName(
					package,
					out name,
					out image,
					out activity,
					out appstore
					);

			Logger.Info("AppUninstaller: Got image name: " + image);

			if (image == null)
			{
				Logger.Info("AppUninstaller: App not found");
				return -1;
			}
		}

		string version;
		if (Utils.IsAppInstalled(package, vmName, out version) == false)
		{
			Logger.Info("App not installed, removing entry from apps.json if there");
			RemoveFromJson(package);
			reason = "App Not Installed";
			return 1;
		}


		int res = UninstallApp(package, vmName);
		if (res == 0)
		{
			Logger.Info("AppUninstaller: Uninstallation successful");
		}
		else
			Logger.Info("AppUninstaller: Uninstallation failed");

		return res;
	}

	public static int UninstallApp(string packageName, string vmName)
	{
		try {
			Logger.Info("AppUninstaller: In uninstall app");

			Dictionary<string, string> data = new Dictionary<string, string>();
			data.Add("pkg", packageName);

			string r;
			try {
				r = HTTPHandler.Post(Common.VmCmdHandler.s_ServerPort, HDAgent.s_UninstallPath, data, vmName);
			} catch (Exception exc) {
				Logger.Error("Exception when sending uninstall post request");
				Logger.Error(exc.ToString());
				return 1;
			}

			IJSonReader json = new JSonReader();
			IJSonObject res = json.ReadAsJSonObject(r);
			if (res["result"].StringValue == "ok")
				return 0;
			else
				return 1;
		} catch(Exception e) {
			Logger.Error(e.ToString());
			return 1;
		}
	}

	public static void RemoveFromGameManagerJson(string packageName)
	{
		GMAppsManager iam = new GMAppsManager(GMAppsManager.JSON_TYPE_INSTALLED_APPS);
		iam.RemoveFromJson(packageName);
	}

	public static string RemoveFromJson(string packageName)
	{
		Logger.Info("AppUninstaller: Removing app from json: " + packageName);

		s_originalJson = JsonParser.GetAppList();

		int count = 0;
		string name = "";
		for (int k=0; k<s_originalJson.Length; k++)
		{
			if (s_originalJson[k].package == packageName)
			{
				name = s_originalJson[k].name;
				count++;
			}
		}

		AppInfo[] newJson = new AppInfo[s_originalJson.Length-count];
		for (int i=0,j=0; i<s_originalJson.Length; i++)
		{
			if (s_originalJson[i].package == packageName)
			{
				RemoveIcon(s_originalJson[i].img);
				RemoveFromLibrary(s_originalJson[i].name, s_originalJson[i].package, s_originalJson[i].img);
				RemoveAppTile(s_originalJson[i].package);

				continue;
			}
			newJson[j] = s_originalJson[i];
			j++;
		}

		JsonParser.WriteJson(newJson);
		return name;
	}

	private static void RemoveIcon(string imageFile)
	{
		Logger.Info("AppUninstaller: Removing icon " + imageFile);
		string imageFilePath = Path.Combine(Common.Strings.GadgetDir, imageFile);
		if (File.Exists(imageFilePath))
			File.Delete(imageFilePath);
	}

	private static void RemoveFromLibrary(string appName, string packageName, string img)
	{
		Logger.Info("Removing {0} from library", appName);
		string appsDir = Path.Combine(Common.Strings.LibraryDir, Common.Strings.MyAppsDir);
		string iconsDir = Path.Combine(Common.Strings.LibraryDir, Common.Strings.IconsDir);
		string storesDir = Path.Combine(Common.Strings.LibraryDir, Common.Strings.StoreAppsDir);
		string shortcutName = appName + ".lnk";
		string ext = img.Substring(img.LastIndexOf("."));
		string iconName = img.Substring(0, img.Length-ext.Length) + ".ico";

		foreach (string app in Directory.GetFiles(appsDir))
		{
			if (Path.GetFileName(app) == shortcutName)
			{
				Logger.Info("Deleting {0}", app);
				File.Delete(app);
			}
		}

		try {
			foreach (string app in Directory.GetFiles(storesDir))
			{
				if (Path.GetFileName(app) == shortcutName)
				{
					Logger.Info("Deleting {0}", app);
					File.Delete(app);
				}
			}
		} catch (Exception e) {
			Logger.Error("Exception when deleting from {0}", storesDir);
			Logger.Error(e.Message);
		}

		foreach (string icon in Directory.GetFiles(iconsDir))
		{
			if (Path.GetFileName(icon) == iconName)
			{
				Logger.Info("Deleting {0}", icon);
				File.Delete(icon);
			}
		}

	}

	private static void RemoveAppTile(string packageName)
	{
		//we do not 
		//support tiles any longer
		//in case of user upgrade to latest bluestacks version
		//when he uninstalls the app
		//if tile is present it should get removed
		string localAppData = Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);
		string shortcutDir = Path.Combine(localAppData, @"Microsoft\Windows\Application Shortcuts\BlueStacks\");
		string linkFileName = packageName + ".lnk";
		string appTilePath = Path.Combine(shortcutDir, linkFileName);
		if (File.Exists(appTilePath))
		{
			Logger.Info("AppUninstaller: Removing app tile " + appTilePath);
			File.Delete(appTilePath);
		}
	}

	static string s_appsDotJsonFile	= Path.Combine(Common.Strings.GadgetDir, "apps.json");

	public static int 		s_systemApps 	= 0;
	public static AppInfo[] 	s_originalJson	= null;
}

public class UninstallerForm : Form {

	public UninstallerForm()
	{
		InitializeComponents();

		AppUninstaller.s_originalJson = JsonParser.GetAppList();

		m_AppList.BeginUpdate();
		m_AppList.Items.Add("Select App to Uninstall");
		m_AppList.SelectedIndex = 0;
		for (int i = AppUninstaller.s_systemApps; i < AppUninstaller.s_originalJson.Length; i++)
		{
			m_AppList.Items.Add(AppUninstaller.s_originalJson[i].name);
		}
		m_AppList.EndUpdate();
	}

	private void InitializeComponents()
	{
		int height = 100;
		int width = 200;

		SuspendLayout();
		this.StartPosition	= FormStartPosition.CenterScreen;
		this.Icon		= System.Drawing.Icon.ExtractAssociatedIcon(Application.ExecutablePath);
		this.SizeGripStyle 	= SizeGripStyle.Hide;
		this.ShowIcon		= true;
		this.MaximizeBox	= false;
		this.MinimizeBox	= false;
		this.ShowInTaskbar	= true;
		this.FormBorderStyle	= FormBorderStyle.FixedDialog;
		this.ClientSize		= new System.Drawing.Size(width,height);
		this.Text		= Locale.Strings.UninstallWindowTitle;

		m_Label = new Label();
		m_Label.Location = new System.Drawing.Point(0,5);
		m_Label.Size = new System.Drawing.Size(width, 25);
		m_Label.Text = "Select an App to Uninstall";

		m_AppList = new ComboBox();
		m_AppList.Location = new System.Drawing.Point(5,35);
		m_AppList.Size = new System.Drawing.Size(width-10, 35);
		m_AppList.DropDownStyle = ComboBoxStyle.DropDownList;

		m_ProgressBar = new ProgressBar();
		m_ProgressBar.Location = new System.Drawing.Point(width/4, height/2 - 10);
		m_ProgressBar.Size = new System.Drawing.Size(width/2, 20);
		m_ProgressBar.Style = ProgressBarStyle.Marquee;
		m_ProgressBar.MarqueeAnimationSpeed = 25;
		m_ProgressBar.Visible = false;


		m_okButton = new Button();
		m_okButton.Text = "Ok";
		m_okButton.DialogResult = DialogResult.OK;
		m_okButton.Width = 60;
		m_okButton.Height = 25;
		m_okButton.Location = new System.Drawing.Point(30, 70);
		m_okButton.Click += new EventHandler(delegate (Object o, EventArgs a) {
					Uninstall();
					});

		m_cancelButton = new Button();
		m_cancelButton.Text = "Cancel";
		m_cancelButton.DialogResult = DialogResult.Cancel;
		m_cancelButton.Width = 60;
		m_cancelButton.Height = 25;
		m_cancelButton.Location = new System.Drawing.Point(110, 70);
		m_okButton.Click += new EventHandler(delegate (Object o, EventArgs a) {
					this.Dispose();
					});

		this.Controls.Add(m_Label);
		this.Controls.Add(m_AppList);
		this.Controls.Add(m_ProgressBar);
		this.Controls.Add(m_okButton);
		this.Controls.Add(m_cancelButton);
		ResumeLayout(false);
		PerformLayout();

		Logger.Info("AppUninstaller: Components Initialized");
	}
	private void Uninstall()
	{
		int selected = m_AppList.SelectedIndex;
		Logger.Info("AppUninstaller: Uninstalling item " + selected.ToString());
		if (selected > 0)
		{
			this.ClientSize	= new System.Drawing.Size(200,40);
			m_Label.Text = Locale.Strings.UninstallingWait;
			m_AppList.Visible = false;
			m_okButton.Visible = false;
			m_cancelButton.Visible = false;
			this.Refresh();

			string name = AppUninstaller.s_originalJson[selected-1+AppUninstaller.s_systemApps].name;
			string package = AppUninstaller.s_originalJson[selected-1+AppUninstaller.s_systemApps].package;
			Logger.Info("AppUninstaller: Uninstalling " + package);
			int res = AppUninstaller.UninstallApp(package, Common.Strings.VMName);
			this.Visible = false;
			if (res == 0)
				MessageBox.Show(String.Format("{0} {1}", name, Locale.Strings.UninstallSuccess),
						this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
			else
				MessageBox.Show(Locale.Strings.UninstallFailed, this.Text, MessageBoxButtons.OK, MessageBoxIcon.None);
			this.Dispose();
		}
		this.Dispose();
	}

	private Label		m_Label;
	private ComboBox	m_AppList;
	private ProgressBar	m_ProgressBar;
	private Button		m_okButton;
	private Button		m_cancelButton;
}
}
