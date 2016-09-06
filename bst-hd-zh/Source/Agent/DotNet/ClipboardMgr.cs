using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Threading;
using System.Windows.Forms;
using Microsoft.Win32;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Agent
{
	public class ClipboardMgr : Form
	{
		private const int WM_DRAWCLIPBOARD = 0x308;
		private const int WM_CHANGECBCHAIN = 0x030D;

		[DllImport("User32.dll")]
		private static extern int SetClipboardViewer(int hWndNewViewer);

		[DllImport("User32.dll", CharSet=CharSet.Auto)]
		private static extern bool ChangeClipboardChain(IntPtr hWndRemove,
				IntPtr hWndNewNext);

		[DllImport("user32.dll", CharSet=CharSet.Auto)]
		private static extern int SendMessage(IntPtr hwnd, int wMsg,
				IntPtr wParam, IntPtr lParam);

		public ClipboardMgr()
		{
			this.WindowState = FormWindowState.Minimized;
			this.Load += OnLoad;
			RegisterForClipBoardNotifications();
		}

		protected override void OnActivated(EventArgs args)
		{
			this.Hide();
		}

		private void OnLoad(object sender, EventArgs e)
		{
			this.Hide();
		}

		protected override void Dispose( bool disposing )
		{
			ChangeClipboardChain(this.Handle, m_NextClipboardViewer);
		}

		private void RegisterForClipBoardNotifications()
		{
			m_NextClipboardViewer = (IntPtr)SetClipboardViewer((int)this.Handle);
		}


		public bool CheckIfGuestFinishedBooting()
		{
		    try {
			Logger.Info("Check if android is booted ");

			string url = String.Format("http://127.0.0.1:{0}/{1}",
				Common.VmCmdHandler.s_ServerPort, Common.VmCmdHandler.s_PingPath);
			Common.HTTP.Client.Get(url, null, false, 1000);
			Logger.Info("Guest finished booting");
			this.guestFinishedBooting = true;
			return true;
		    } catch (Exception e) {
			Logger.Error("Guest not booted yet");
			Logger.Error(e.Message);
			return false;
		    }
		}

		private void ProcessClipboardData()
		{
			if (Clipboard.ContainsText())
			{
				//If andorid is not booted don't send Clipboard
				//data ...
				if (this.guestFinishedBooting != true) {
					if (CheckIfGuestFinishedBooting() != true) {
						return;
					}
				}

				string clipboardText = Clipboard.GetText();

				Logger.Info("ClipboardMgr: Got clipboardText");
				// If data arrived is same as cached data in
				//ClipboardClient, then don't sen it to android
				if (String.Compare(this.CachedText, clipboardText)
						== 0 ) {
					return;
				}

				try {
					Dictionary<string, string> data = new Dictionary<string, string>();
					data.Add("text", clipboardText);

					string url = String.Format("http://127.0.0.1:{0}/{1}",
							Common.VmCmdHandler.s_ServerPort,
							HDAgent.s_ClipboardDataPath);
					Logger.Info("ClipboardMgr: Sending post request to {0}", url);

					string res = Common.HTTP.Client.Post(url, data, null, false);
					Logger.Info("ClipboardMgr: Got response: {0}", res);
				} catch (Exception exc) {
					Logger.Error("Exception in Sending Clipboard" +
							"Command {0}",exc.ToString());
				}
			}
		}

		[System.Security.Permissions.PermissionSet(System.Security.Permissions.SecurityAction.Demand, Name = "FullTrust")]
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_DRAWCLIPBOARD:
					ProcessClipboardData();
					SendMessage(m_NextClipboardViewer, m.Msg, m.WParam,
							m.LParam);
					break;

				case WM_CHANGECBCHAIN:
					if (m.WParam == m_NextClipboardViewer)
						m_NextClipboardViewer = m.LParam;
					else
						SendMessage(m_NextClipboardViewer, m.Msg, m.WParam,
								m.LParam);
					break;

				default:
					base.WndProc(ref m);
					break;
			}
		}

		public void SetCachedText(String text) {
		    this.CachedText = text;
		}

		public String GetCachedText() {
		    return this.CachedText;
		}

		private IntPtr m_NextClipboardViewer;
		private bool guestFinishedBooting = false;
		private  String CachedText = "";
	}
}

