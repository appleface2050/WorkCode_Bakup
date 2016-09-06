using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.IO;
using Microsoft.Win32;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;

using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Common.Interop;

using Gecko;
using Gecko.Events;
using System.Windows;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	public class TwitchWindow : Form
	{
		public static TwitchWindow Instance = null;
		GeckoWebBrowser mBrowser;
		static bool isFormToBeCreated = true;

		int mRetry = 0;
		int RETRY_MAX = 30;
		Thread mThread;

		public TwitchWindow()
		{
			Instance = this;
			this.SuspendLayout();
			this.Size = new System.Drawing.Size(100, 100);
			this.Text = "Initialising BlueStacks TV";

			mBrowser = new GeckoWebBrowser();
			mBrowser.Dock = DockStyle.Fill;
			mBrowser.Navigate("http://www.twitch.tv");
			mBrowser.DocumentCompleted += DocumentCompletedHandler;
			mBrowser.Visible = false;

			this.Visible = false;
			this.Icon = Utils.GetApplicationIcon();
			this.Controls.Add(mBrowser);
			this.ResumeLayout(false);
			this.Show();
			this.Visible = false;

			mThread = new Thread(delegate ()
			{

				Thread.Sleep(1000);

				while (mRetry < RETRY_MAX)
				{
					mRetry++;

					if (TwitchWindow.Instance == null)
						break;

					UIHelper.RunOnUIThread(TwitchWindow.Instance, delegate ()
					{
						if (CheckForLanguageCookie())
						{
							UpdateRegistry();
							this.Close();
						}
					});
					Thread.Sleep(1000);
				}

				UIHelper.RunOnUIThread(TwitchWindow.Instance, delegate ()
				{
					Logger.Info("CheckForLanguageCookie: Closing the twitch form after {0} attempt", mRetry);
					UpdateRegistry();
					this.Close();
				});
			});
			mThread.IsBackground = true;
			mThread.Start();
		}

		private void DocumentCompletedHandler(object sender, GeckoDocumentCompletedEventArgs e)
		{
			UpdateRegistry();
			Logger.Info("CheckForLanguageCookie: Closing Twitch Window on DocumentCompletedHandler");
			this.Close();
		}

		private void UpdateRegistry()
		{
			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath, true);
			configKey.SetValue("OpenTwitchWindow", 0, RegistryValueKind.DWord);
			configKey.Close();
		}

		private bool CheckForLanguageCookie()
		{
			try
			{
				string result = "";
				if (mBrowser == null || mBrowser.Window == null)
					return true;

				using (Gecko.AutoJSContext runner = new Gecko.AutoJSContext(mBrowser.Window.JSContext))
				{
					var script = "document.cookie;";
					runner.PushCompartmentScope((nsISupports)mBrowser.Window.DomWindow);
					runner.EvaluateScript(script, out result);

					if (result == null || result.Length == 0)
						return false;

					string[] stringSeparators = new string[] { ";" };
					string[] keyValPairs = result.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);

					if (keyValPairs.Length != 0)
					{
						stringSeparators = new string[] { "=" };
						foreach (string keyValPair in keyValPairs)
						{
							string[] keyVal = keyValPair.Split(stringSeparators, StringSplitOptions.RemoveEmptyEntries);
							if (keyVal[0].Trim().Equals("language"))
							{
								Logger.Info("CheckForLanguageCookie: found language: {0} in retry: {1}", keyVal[1], mRetry);
								return true;
							}
						}
					}
				}
			}
			catch (Exception ex)
			{
				Logger.Error("CheckForLanguageCookie Error: {0}", ex.ToString());
			}
			return false;
		}

		protected override void OnFormClosing(FormClosingEventArgs e)
		{
			mThread.Abort();
			mBrowser.Dispose();
		}

		internal static void AddTwitchWindowIfNeeded()
		{
			if (isFormToBeCreated)
			{
				RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
				int openTwitchWindow = (int)configKey.GetValue("OpenTwitchWindow", 1);
				Logger.Info("openTwitchWindow Value: {0}", openTwitchWindow);

				if (openTwitchWindow == 1)
				{
					/*
					 * Setting the OpenTwitchWindow value to 0
					 * on DocumentCompleted Event of Twitch Window
					 */
					StreamWindow.Instance.Dispatcher.Invoke(new Action(() =>
					{
						Logger.Info("opening hidden twitch window");
						new TwitchWindow();
					}));
				}
				else
				{
					Logger.Info("Skipping opening of hidden twitch window");
				}
				isFormToBeCreated = false;
			}
		}

		internal static void CloseWindow()
		{
			if (Instance != null)
			{
				Instance.Close();
			}
		}
	}
}

