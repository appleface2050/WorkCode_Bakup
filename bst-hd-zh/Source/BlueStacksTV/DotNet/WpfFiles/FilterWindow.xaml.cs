using BlueStacks.hyperDroid.Common;
using CodeTitans.JSon;
using Microsoft.Win32;
using System;
using System.Collections.Generic;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace BlueStacks.hyperDroid.BlueStacksTV
{
	/// <summary>
	/// Interaction logic for FilterWindow.xaml
	/// </summary>
	public partial class FilterWindow : Window
	{
		internal static FilterWindow Instance = null;

		private string FILTER_HOME_URL = "filters/home/index.html";
		private string FILTER_NA_URL = "filters/home/NotAvailable.html";

		private Browser mFilterBrowser = null;
		private System.Windows.Forms.Panel mOBSRenderFrame = new System.Windows.Forms.Panel();
		private bool mFilterAvailable = false;

		public static string sCurrentAppPkg;

		public IntPtr mOBSHandle = IntPtr.Zero;

		public FilterWindow()
		{
			InitializeComponent();
		}

		public void Setup(string channel, string sessionId, bool showFilterScreen)
		{
			Instance = this;

			string filterUrl = App.sApplicationBaseUrl + FILTER_HOME_URL;
			if (!showFilterScreen)
				filterUrl = App.sApplicationBaseUrl + FILTER_NA_URL;

			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			filterUrl = (string)configKey.GetValue("FilterUrl", filterUrl);

			filterUrl += "?channel=" + channel;
			if (showFilterScreen)
			{
				string appPkg = FilterUtility.GetCurrentAppPkg();
				filterUrl += "&session_id=" + sessionId + "&guid=" + User.GUID;
				filterUrl += "&" + FilterUtility.GetQueryStringForTheme(appPkg, FilterUtility.GetCurrentTheme(appPkg));
			}
			mFilterBrowser = new Browser(filterUrl);
			mFilterBrowser.BackColor = System.Drawing.Color.Yellow;
			mFilterBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			mFilterBrowser.GetMarkupDocumentViewer().SetFullZoomAttribute((float)(this.Width / 865));

			if (showFilterScreen)
			{
				mOBSRenderFrame = new System.Windows.Forms.Panel();
				mOBSRenderFrame.Dock = System.Windows.Forms.DockStyle.Fill;
				mOBSRenderFrame.BackColor = System.Drawing.Color.Black;
				mFilterAvailable = true;
			}
			BrowserHost.Child = mFilterBrowser;

			if (showFilterScreen)
			{
				ObsHost.Child = mOBSRenderFrame;
				mOBSRenderFrame.BringToFront();
			}


			RegistryKey filterKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMFilterPath);
			filterKey.SetValue("ChannelName", channel);
			filterKey.Close();
			mFilterBrowser.Navigate(filterUrl);
		}

		public void CloseFilterWindow(string jsonArray)
		{
			ReportToCloud(jsonArray);

			this.Hide();

			if (mOBSHandle != IntPtr.Zero)
			{
				Common.Interop.Window.ShowWindow(mOBSHandle, Common.Interop.Window.SW_HIDE);
				Common.Interop.Window.SetParent(mOBSHandle, IntPtr.Zero);
			}

			sCurrentAppPkg = null;

			if (mFilterBrowser != null)
			{
				mFilterBrowser.Dispose();
			}

			if (mFilterAvailable && StreamWindow.Instance != null)
				StreamWindow.Instance.EvaluateJS("filter_added();");

			this.Close();
		}

		public void ReParentOBSWindow()
		{
			mOBSHandle = Common.Interop.Window.FindWindow("OBSWindowClass", null);
			IntPtr panelHandle = mOBSRenderFrame.Handle;

			if (mOBSHandle != IntPtr.Zero)
			{
				Logger.Info("OBS Handle: {0}", mOBSHandle.ToString());

				if (StreamWindowUtility.sOBSDevEnv)
					Common.Interop.Window.SetWindowLong(mOBSHandle, Common.Interop.Window.GWL_STYLE, Common.Interop.Window.GetWindowLong(mOBSHandle, Common.Interop.Window.GWL_STYLE) | Convert.ToInt32(Common.Interop.Window.WS_CHILD));
				else
					Common.Interop.Window.SetWindowLong(mOBSHandle, Common.Interop.Window.GWL_STYLE, Convert.ToInt32(Common.Interop.Window.WS_CHILD));
				Common.Interop.Window.SetParent(mOBSHandle, panelHandle);

				Common.Interop.Window.SetWindowPos(mOBSHandle, (IntPtr)0, 0, 0, mOBSRenderFrame.Width, mOBSRenderFrame.Height, Common.Interop.Window.SWP_NOACTIVATE | Common.Interop.Window.SWP_SHOWWINDOW);
				Common.Interop.Window.ShowWindow(mOBSHandle, Common.Interop.Window.SW_SHOW);
			}
		}
		public void ReportToCloud(string jsonArray)
		{
			JSonReader reader = new JSonReader();
			IJSonObject objArray = reader.ReadAsJSonObject(jsonArray);

			for (int i = 0; i < objArray.Length; i++)
			{
				IJSonObject obj = (IJSonObject)objArray[i];

				Dictionary<string, string> data = new Dictionary<string, string>();
				foreach (string key in obj.Names)
				{
					if (!obj[key].IsNull)
					{
						//no way to detect Boolean value
						if (obj[key].ToString().ToLower().Equals("true") ||
							obj[key].ToString().ToLower().Equals("false"))
							data.Add(key, obj[key].ToString().ToLower());
						else
							data.Add(key, obj[key].ToString());
					}
				}

				Thread thread = new Thread(delegate ()
				{
					string url = Common.Utils.GetHostUrl() + "/stats/btvfunnelstats";
					try
					{
						string result = Common.HTTP.Client.Post(url, data, null, false);
						Logger.Info("FilterWindow ReportToCloud response: {0}", result);
					}
					catch (Exception ex)
					{
						Logger.Error(ex.ToString());
						Logger.Error("Post failed. url = {0}, data = {1}", url, data);
					}
				});
				thread.IsBackground = true;
				thread.Start();
			}
		}

		private void Window_Loaded(object sender, RoutedEventArgs e)
		{
			ReParentOBSWindow();
		}
	}
}
