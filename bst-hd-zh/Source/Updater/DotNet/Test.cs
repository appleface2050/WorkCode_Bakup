using System;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Updater {
class Test : Form {
	public Test() {
		Icon = Icon.ExtractAssociatedIcon(Application.ExecutablePath);
	}

	protected override void OnLoad(EventArgs e) {
		Updater.Manager.Start("http://bluestacks-com.s3.amazonaws.com/updates/manifest_2.0.1.1048.ini");
		Updater.Manager.ManifestDownloaded += new OnManifestDownloaded(ManifestDownloaded);
	}

	private static void ManifestDownloaded() {
		DialogResult res = Common.UI.MessageBox.ShowMessageBox("Updates Available!", 
				String.Format("New version ({0}) of BlueStacks is available", Manifest.Version),
				"Download",
				"Remind Me Later", 
				null);
		switch (res) {
			case DialogResult.OK:
				System.Diagnostics.Process.Start(Manifest.URL);
				break;
			default:
				break;
		}
	}

	/* Test */
	[STAThread]
	public static void Main(string[] args) {
		Test test = new Test();
		Application.EnableVisualStyles();
		Application.Run(test);
	}
}
}
