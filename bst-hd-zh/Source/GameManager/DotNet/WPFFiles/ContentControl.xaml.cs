using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend;
using System;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Forms.Integration;

namespace BlueStacks.hyperDroid.GameManager
{
	/// <summary>
	/// Interaction logic for ContentControl.xaml
	/// </summary>
	public partial class ContentControl : System.Windows.Controls.UserControl
	{
		public static ContentControl Instance = null;

		Thread mThreadCheckforActivity = new Thread(CheckForActivity);

		Grid VisibleGrid = null;

		[DllImport("user32.dll", SetLastError = true)]
		static extern IntPtr SetFocus(IntPtr hWnds);

		public ContentControl()
		{
			Instance = this;
			InitializeComponent();
			mThreadCheckforActivity.IsBackground = true;
			mThreadCheckforActivity.Start();
		}

		private static void CheckForActivity()
		{
			while (true)
			{
				try
				{
					Thread.Sleep(1000);
					if (Instance.VisibleGrid == Instance.ProgressBarGrid)
					{
						Logger.Info("Top Grid is Progress Bar");
						string topPackageName = GetTopPackageName();
						Logger.Info("Top ActivityName is " + topPackageName);
						Logger.Info("Selected Tab activity is " + TabButtons.Instance.SelectedTab.mPackageName);
						if (topPackageName.Contains(TabButtons.Instance.SelectedTab.mPackageName))
						{
							Logger.Info("Stuck!!! hiding progress Bar");
							GameManagerWindow.Instance.Dispatcher.Invoke(new Action(() =>
							{
								Instance.HideWaitControl();
							}));
						}
					}
				}
				catch (Exception ex)
				{
					Logger.Error("Error while checking if stuck on progressbar grid " + ex.ToString());
				}
			}
		}
		internal void AddAndroid()
		{
			UIHelper.SetDispatcher(GameManagerWindow.Instance.Dispatcher.Invoke);
			// Create the interop host control.
			AddControl(FrontendHandler.StartFrontend("Android"), FrontendGrid);
			FrontendHandler.frontend.MouseMove += ResizeManager.Frontend_MouseMove;
			FrontendHandler.frontend.MouseLeave += ResizeManager.Frontend_MouseLeave;
			FrontendHandler.frontend.MouseDown += ResizeManager.Frontend_MouseDown;
			GameManagerWindow.Instance.MouseMove += ResizeManager.Instance_MouseMove;
			GameManagerWindow.Instance.MouseLeave += ResizeManager.Instance_MouseLeave;
			GameManagerWindow.Instance.MouseDown += ResizeManager.Instance_MouseDown;
		}



		internal Grid AddControl(Browser control)
		{
			Grid grid = new Grid();
			ControlGrid.Children.Add(grid);
			AddControl(control, grid);
			control.PreviewKeyDown += Control_PreviewKeyDown; ;
			control.DomMouseMove += ResizeManager.Browser_MouseMove;
			control.DomMouseDown += ResizeManager.Browser_MouseDown;
			control.DomMouseOut += ResizeManager.Browser_MouseLeave;
			return grid;
		}

		private void Control_PreviewKeyDown(object sender, System.Windows.Forms.PreviewKeyDownEventArgs e)
		{
			if (e.KeyCode == System.Windows.Forms.Keys.F11)
			{
				TopBar.Instance.PbMaximizeButton_MouseUp(null, null);
			}
		}

		internal void AddControl(System.Windows.Forms.Control control, Grid grid)
		{
			WindowsFormsHost host = new WindowsFormsHost();
			host.Child = control;
			grid.Children.Add(host);
			grid.Visibility = Visibility.Hidden;
		}

		internal void ShowWaitControl()
		{
			if (Oem.Instance.IsTabsEnabled)
			{
				mProgressBarControl.Reset();
				BringToFront(ProgressBarGrid, false);
			}
		}

		private static string GetTopPackageName()
		{

			string command = String.Format("{0} {1}", "getprop", "bst.config.topDisplayedPackage");
			string url = String.Format("http://127.0.0.1:{0}/{1}",
					Common.VmCmdHandler.s_ServerPort, command);

			string result = string.Empty;
			try
			{
				result = Common.HTTP.Client.Get(url, null, false);
				Logger.Info("post command: " + command + ", result: " + result);
			}
			catch (Exception ex)
			{
				Logger.Error("An error occured. Err :" + ex.ToString());
			}
			return result;
		}
		internal void HideWaitControl()
		{
			Logger.Info("Hiding ProgressBar");
			BringToFront(FrontendGrid, true);
		}

		internal void BringFrontendInFront()
		{
			BringToFront(FrontendGrid, false);
		}

		internal void BringToFront(Grid grid, bool removeWaitControl)
		{
			if (grid == FrontendGrid)
			{
				FrontendHandler.IsFrontendOnTop = true;
			}
			else
			{
				FrontendHandler.IsFrontendOnTop = false;
			}
			if (VisibleGrid != ProgressBarGrid || removeWaitControl)
			{
				if (grid != VisibleGrid)
				{
					if (VisibleGrid != null)
					{
						VisibleGrid.Visibility = Visibility.Hidden;
					}
					VisibleGrid = grid;
					WindowsFormsHost h = (grid.Children[0] as WindowsFormsHost);
					if (h != null)
					{
						SetFocus(h.Child.Handle);
					}
					VisibleGrid.Visibility = Visibility.Visible;
				}
			}
		}
	}
}
