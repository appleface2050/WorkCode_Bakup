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
	/// Interaction logic for LayoutWindow.xaml
	/// </summary>
	public partial class LayoutWindow : Window
	{
		internal static LayoutWindow Instance = null;

		public static object mLayoutWindowLock = new object();
		
		private string LAYOUT_HOME_URL = "filters/home/layout/index.html";
		private string LAYOUT_NA_URL = "filters/home/layout/NotAvailable.html";

		private Browser mLayoutBrowser = null;
		private System.Windows.Forms.Panel mOBSRenderFrame = new System.Windows.Forms.Panel();
		private string mCurrentLayout;
		private bool mIsCurrentAppViewLayout;
		private string mCurrentLastCameraLayout;
		public object mChangeLayoutLock = new object();

		public IntPtr mOBSHandle = IntPtr.Zero;

		public LayoutWindow()
		{
			InitializeComponent();
		}

		public static string GetLayoutName(string layoutTheme)
		{
			JSonReader jsonReader = new JSonReader();
			IJSonObject obj = jsonReader.ReadAsJSonObject(layoutTheme);

			string name = obj["name"].StringValue;

			string[] nameSplit = name.Split(new string[]{"_"}, StringSplitOptions.RemoveEmptyEntries);

			if (StreamManager.Instance.IsPortraitApp())
				return "portrait_" + nameSplit[1];
			else
				return "landscape_" + nameSplit[1];
		}

		public void ChangeLayout(string layoutTheme, bool isAppView)
		{
			lock (mChangeLayoutLock)
			{
				if (isAppView)
				{
					if (String.Compare(StreamManager.mCamStatus, "true", true) == 0)
						StreamWindow.Instance.ChangeWebCamState();
				}
				else
				{
					if (String.Compare(StreamManager.mCamStatus, "true", true) != 0)
						StreamWindow.Instance.ChangeWebCamState();
				}
				StreamManager.Instance.SetSceneConfiguration(layoutTheme);
				StreamManager.Instance.mAppViewLayout = isAppView;
				FilterUtility.UpdateAppViewLayoutRegistry(isAppView);
			}
		}

		public void Setup(string layoutTheme, string paramsStr)
		{
			Instance = this;

			string layoutUrl = App.sApplicationBaseUrl + LAYOUT_HOME_URL;

			RegistryKey configKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMConfigPath);
			layoutUrl = (string)configKey.GetValue("LayoutUrl", layoutUrl);
			mCurrentLayout = layoutTheme;
			mIsCurrentAppViewLayout = StreamManager.Instance.mAppViewLayout;
			mCurrentLastCameraLayout = StreamManager.Instance.mLastCameraLayoutTheme;

			layoutUrl += "?activeMode="+LayoutWindow.GetLayoutName(layoutTheme);
			if (StreamManager.Instance.mIsStreaming)
				layoutUrl += "&live=true";
			layoutUrl += paramsStr;

			mLayoutBrowser = new Browser(layoutUrl);
			mLayoutBrowser.BackColor = System.Drawing.Color.Yellow;
			mLayoutBrowser.Dock = System.Windows.Forms.DockStyle.Fill;
			mLayoutBrowser.GetMarkupDocumentViewer().SetFullZoomAttribute((float)(this.Width / 573));

			mOBSRenderFrame = new System.Windows.Forms.Panel();
			mOBSRenderFrame.Dock = System.Windows.Forms.DockStyle.Fill;
			mOBSRenderFrame.BackColor = System.Drawing.Color.Black;

			BrowserHost.Child = mLayoutBrowser;

			ObsHost.Child = mOBSRenderFrame;
			mOBSRenderFrame.BringToFront();

			mLayoutBrowser.Navigate(layoutUrl);
		}

		public static void LaunchWindow(string paramsStr)
		{
			lock (mLayoutWindowLock)
			{
				if (LayoutWindow.Instance != null)
					return;
				
				StreamWindowUtility.UnSetOBSParentWindow();
				LayoutWindow.Instance = new LayoutWindow();
				LayoutWindow.Instance.Setup(StreamManager.Instance.mLayoutTheme, paramsStr);
				LayoutWindow.Instance.ShowDialog();
			}
		}

		public void UpdateRegistry()
		{
			if (mCurrentLastCameraLayout != null)
			{
				FilterUtility.UpdateLayout("LastCameraLayoutTheme", mCurrentLastCameraLayout);
				StreamManager.Instance.mLastCameraLayoutTheme = mCurrentLastCameraLayout;
			}
			else
			{
				FilterUtility.UpdateLayout("LastCameraLayoutTheme", mCurrentLayout);
				StreamManager.Instance.mLastCameraLayoutTheme = mCurrentLayout;
			}
			FilterUtility.UpdateLayout("LayoutTheme", mCurrentLayout);
			FilterUtility.UpdateAppViewLayoutRegistry(mIsCurrentAppViewLayout);
		}

		public void CloseDialog(string jsonString)
		{
			JSonReader reader = new JSonReader();
			IJSonObject obj = reader.ReadAsJSonObject(jsonString);

			try
			{
				ReportToCloud(obj["stats"].ToString());
			}
			catch (Exception ex)
			{
				Logger.Error("CloseDialog: {0}", ex);
			}

			this.Hide();

			if (mOBSHandle != IntPtr.Zero)
			{
				Common.Interop.Window.ShowWindow(mOBSHandle, Common.Interop.Window.SW_HIDE);
				Common.Interop.Window.SetParent(mOBSHandle, IntPtr.Zero);
			}

			if (mLayoutBrowser != null)
			{
				mLayoutBrowser.Dispose();
			}

			if (obj.Contains("type") && obj["type"].StringValue.Equals("close") &&
					!StreamManager.Instance.mIsStreaming)
			{
				UpdateRegistry();
				ChangeLayout(mCurrentLayout, mIsCurrentAppViewLayout);
			}

			if (StreamWindow.Instance != null)
				StreamWindow.Instance.EvaluateJS("layoutWindowClosed();");

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
						Logger.Info("LayoutWindow ReportToCloud response: {0}", result);
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
