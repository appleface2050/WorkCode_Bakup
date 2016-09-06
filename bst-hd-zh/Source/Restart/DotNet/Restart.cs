using System;
using System.Diagnostics;
using System.Drawing;
using System.IO;
using System.Threading;
using System.Windows.Forms;
using System.ServiceProcess;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Locale;

namespace BlueStacks.hyperDroid {

public class Restart : Form {

	private SynchronizationContext	uiContext;
	private String			vmName;
	private ServiceController	svc;
	private String			svcName;
	private Label			statusControl;
	private ProgressBar		progressControl;
	private Button			exitControl;
	private static bool		sHideMode = false;

	[STAThread]
	public static void Main(String[] args)
	{
		if (Common.Utils.IsAlreadyRunning(Common.Strings.RestartLockName, out s_RestartLock))
		{
			Environment.Exit(1);
		}

		Locale.Strings.InitLocalization(null);
		if (args.Length == 0)
			Usage();

		foreach (String arg in args)
		{
			if (arg.Equals("hidemode"))
			{
				sHideMode = true;
				break;
			}
		}

		InitLog();
		Utils.LogParentProcessDetails();
		Logger.Info("HD-Restart started");
		Logger.Info("IsAdministrator: {0}", User.IsAdministrator());
		Logger.Info("args.Length: {0}", args.Length);

		Application.EnableVisualStyles();
		Application.Run(new Restart(args[0]));
	}

	private static void Usage()
	{
		String prog = Process.GetCurrentProcess().ProcessName;

		String capt = Locale.Strings.RESTART_UTILITY_TITLE_TEXT;
		String text = "";

		text += String.Format("{0}\n", Locale.Strings.RESTART_UTILITY_USAGE_TEXT);
		text += String.Format("    {0} <vm name>\n", prog);

		MessageBox.Show(text, capt);
		Environment.Exit(1);
	}

	private static void InitLog()
	{
		Logger.InitUserLog();

		System.Console.SetOut(Logger.GetWriter());
		System.Console.SetError(Logger.GetWriter());

		Application.ThreadException += delegate(Object obj,
		    System.Threading.ThreadExceptionEventArgs evt) {
			Logger.Error("Unhandled Exception:");
			Logger.Error(evt.Exception.ToString());
		};

		Application.SetUnhandledExceptionMode(
		    UnhandledExceptionMode.CatchException);

		AppDomain.CurrentDomain.UnhandledException += delegate(
		    Object obj, UnhandledExceptionEventArgs evt) {
			Logger.Error("Unhandled Exception:");
			Logger.Error(evt.ExceptionObject.ToString());

			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsMessageBoxToBeDisplayed)
			{
				String capt = Locale.Strings.RESTART_UTILITY_TITLE_TEXT;
				String text = Locale.Strings.RESTART_UTILITY_UNHANDLED_EXCEPTION_TEXT;
				MessageBox.Show(text, capt);
			}
			Environment.Exit(1);
		};
	}

	private Restart(String vmName)
	{
		Utils.KillProcessByName("HD-Frontend");
		Utils.KillProcessByName("Bluestacks");

		this.Icon = Utils.GetApplicationIcon();

		this.uiContext = WindowsFormsSynchronizationContext.Current;
		this.vmName = vmName;

		this.svcName = String.Format(Common.Strings.GetAndroidServiceName(vmName));
		this.svc = new ServiceController(this.svcName);
		Logger.Info("the service name is " + this.svcName);
		/*
		 * Setup the form.
		 */
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		String installDir = (string)key.GetValue("InstallDir");	
		this.statusControl = new Label();
			this.statusControl.Text = BlueStacks.hyperDroid.Common.Oem.Instance.BlueStacksRestartUtilityRestartingText;
			this.Text = BlueStacks.hyperDroid.Common.Oem.Instance.BlueStacksRestartUtilityRestartingText;

		this.ClientSize = new Size(300, 120);


		this.statusControl.AutoSize = true;
		this.statusControl.Location = new Point(20, 15);

		this.progressControl = new ProgressBar();
		this.progressControl.Style = ProgressBarStyle.Marquee;
		this.progressControl.Value = 100;
		this.progressControl.Width = 260;
		this.progressControl.Location = new Point(20, 45);

		this.exitControl = new Button();
		this.exitControl.Text = Locale.Strings.RESTART_UTILITY_CANCEL_TEXT;
		this.exitControl.Location = new Point(
		    280 - this.exitControl.Width, 85);

		this.exitControl.Click += delegate(Object obj,
		    EventArgs evt) {
			Environment.Exit(0);
		};

		this.Controls.Add(this.statusControl);
		this.Controls.Add(this.progressControl);
		this.Controls.Add(this.exitControl);

		this.Shown += new System.EventHandler(this.FormShown);

		/*
		 * Fire off another thread to stop the service.
		 */

		Thread thread = new Thread(delegate() {
			StopService();
		});
		thread.Start();
	}

	private void FormShown(object sender, EventArgs e)
	{
		Logger.Info("Form Shown called");
		if (sHideMode)
		{
			Logger.Info("Not showing form in hidemode");
			this.Hide();
		}
	}

	private void StopService()
	{
		/*
		 * Stop the service.
		 */

		try
		{
			Logger.Info("Restart: Stopping service");
			this.svc.Stop();
		}
		catch (Exception)
		{
		}

		/*
		 * Sleep for some time to let everything settle and
		 * give the user the * impression that something is happening.
		 * Check if the service has started after the wait.
		 * Try this a few times before giving up.
		 */

		for (int i = 0; i < 10; i++)
		{
			this.svc.Refresh();
			if (this.svc.Status == ServiceControllerStatus.Stopped) {
				Logger.Info("Restart: Service stopped");
				break;
			}
			else {
				Logger.Info("Restart: Service not stopped yet");
				Thread.Sleep(2000);
			}
		}

		/*
		 * Execute our completion routine on the UI thread.
		 */

		SendOrPostCallback cb = new SendOrPostCallback(
		    delegate(Object obj) {
			StopServiceCompletion();
		});

		uiContext.Send(cb, null);
	}

	private void StopServiceCompletion()
	{
		this.svc.Refresh();

		if (this.svc.Status == ServiceControllerStatus.Stopped) {
				this.statusControl.Text = BlueStacks.hyperDroid.Common.Oem.Instance.BlueStacksRestartUtilityRestartingText;
			Thread thread = new Thread(delegate() {
				SetServiceStoppedGracefully();
				StartService();
			});

			thread.Start();

		} else {

			Logger.Info("Cannot stop service");

			this.statusControl.Text = Locale.Strings.RESTART_UTILITY_CANNOT_STOP_TEXT;
			this.exitControl.Text = Locale.Strings.RESTART_UTILITY_EXIT_TEXT;

			this.Controls.Remove(this.progressControl);
		}
	}

	private void SetServiceStoppedGracefully()
	{
		/*
		 * Set ServiceStoppedGracefully
		 * as this code path will be reached only on user interaction
		 */
		String cfgPath = Common.Strings.HKLMAndroidConfigRegKeyPath;
		using (RegistryKey key = Registry.LocalMachine.OpenSubKey(
					cfgPath, true)) {
			key.SetValue("ServiceStoppedGracefully", 1,
					RegistryValueKind.DWord);
			key.Flush();
		}
	}

	private void StartService()
	{
		/*
		 * Start the service.
		 */

		try
		{
			Logger.Info("Restart: Starting service");
			Utils.EnableService(this.svcName, "demand");
			this.svc.Start();
		}
		catch (Exception)
		{
		}

		/*
		 * Sleep for some time to give the user the
		 * impression that something is happening.
		 * Check if the service has started after the wait.
		 * Try this a few times before giving up.
		 */

		for (int i = 0; i < 10; i++)
		{
			this.svc.Refresh();
			if (this.svc.Status == ServiceControllerStatus.Running) {
				Logger.Info("Restart: Service started");
				break;
			}
			else {
				Logger.Info("Restart: Service not started yet");
				Thread.Sleep(2000);
			}
		}

		/*
		 * Execute our completion routine on the UI thread.
		 */

		SendOrPostCallback cb = new SendOrPostCallback(
		    delegate(Object obj) {
			StartServiceCompletion();
		});

		uiContext.Send(cb, null);
	}

	private void StartFrontend()
	{
		Logger.Info("Restart: Starting Frontend");
		// Although the name of the function is StartFrontend, but we
		// will start it using HD-RunApp.exe
		RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
		String installDir = (string)key.GetValue("InstallDir");
		string runAppFile = Path.Combine(installDir, @"HD-RunApp.exe");

		Process runAppProc = new Process();
		runAppProc.StartInfo.UseShellExecute = false;
		runAppProc.StartInfo.CreateNoWindow = true;
		runAppProc.StartInfo.FileName = runAppFile;
		if (sHideMode)
			runAppProc.StartInfo.Arguments = String.Format("-h");
		else
			runAppProc.StartInfo.Arguments = String.Format("");

		runAppProc.Start();
	}

	private void StartGameManager()
	{
		Logger.Info("Restart: Starting Game Manager");
		String gameManagerFile = Common.Utils.GetPartnerExecutablePath();;
		Logger.Info("Launching " + gameManagerFile);
		Process.Start(gameManagerFile);
	}

	private void StartServiceCompletion()
	{
		this.svc.Refresh();

		if (this.svc.Status == ServiceControllerStatus.Running) {
			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsGameManagerToBeStartedOnRunApp)
			{
				StartGameManager();
			}
			else
			{
			if (BlueStacks.hyperDroid.Common.Oem.Instance.IsFrontendToBeHiddenOnRestart) {
				Utils.StartHiddenFrontend(Common.Strings.VMName);
			} else {
				StartFrontend();
			}
			}

			/* Successful.  Just exit. */
			Environment.Exit(0);

		} else {

			Logger.Info("Cannot start service");

			this.statusControl.Text = Locale.Strings.RESTART_UTILITY_CANNOT_START_TEXT;
			this.exitControl.Text = Locale.Strings.RESTART_UTILITY_EXIT_TEXT;

			this.Controls.Remove(this.progressControl);
		}
	}

	private static Mutex s_RestartLock;
}

}
