using System;
using System.Drawing;
using System.IO;
using System.Runtime.InteropServices;
using System.Security.Permissions;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.GameManager
{
	//http://stackoverflow.com/questions/24641829/formborderstyle-none-removes-the-native-open-effect-of-windows-8
	public class AeroForm : Form
	{
		bool isClearCache = true;
		public new FormWindowState WindowState
		{
			get { return base.WindowState; }
			set
			{
				if (value == FormWindowState.Normal)
				{
					Size = cachedSize;
					isClearCache = true;
				}
				else
				{
					cachedSize = Size;
					isClearCache = false;
				}
				base.WindowState = value;
			}
		}
		public new Size ClientSize
		{
			get { return base.ClientSize; }
			set
			{
				if (value != base.ClientSize)
				{
					cachedSize = value;
                    base.ClientSize = value;
				}
			}
		}

		public Size cachedSize = new Size();
		bool aero = false;
		[StructLayout(LayoutKind.Sequential)]
		struct MARGINS { public int Left, Right, Top, Bottom; }


		[DllImport("dwmapi.dll", PreserveSig = false)]
		static extern bool DwmIsCompositionEnabled();

		public AeroForm()
		{
			aero = IsCompositionEnabled();
		}

		private const int WM_SYSCOMMAND = 0x0112;
		private const int SC_MINIMIZE = 0xF020;
		private const int SC_MAXIMIZE = 0xF030;
		private const int SC_RESTORE = 0xF120;
		private const int WM_NCCALCSIZE = 0x0083;

		[SecurityPermission(SecurityAction.LinkDemand, Flags = SecurityPermissionFlag.UnmanagedCode)]
		protected override void WndProc(ref Message m)
		{
			switch (m.Msg)
			{
				case WM_SYSCOMMAND:
					int command = m.WParam.ToInt32() & 0xfff0;
					//When you minimize and restore, the size will change.
					//this override is for preventing this unwanted resize.
					if (command == SC_MINIMIZE)
					{
						cachedSize = Size;
					}
					if (command == SC_MAXIMIZE)
					{
						cachedSize = Size;
					}
					if (command == SC_RESTORE)
					{
						RestoreSize();
					}
					break;
				case WM_NCCALCSIZE:
					if (aero)
						return;
					break;
				case 174:
					if (this.WindowState == FormWindowState.Minimized)
					{
						cachedSize = Size;
					}
					break;
				case 133:
					RestoreSize();
					break;
			}
			base.WndProc(ref m);
		}

		private void RestoreSize()
		{
			if (cachedSize != new Size())
			{
				Size = cachedSize;
				if (isClearCache)
				{
					cachedSize = new Size();
				}
			}
		}

		//this is for checking the OS's functionality.
		//Windows XP does not have dwmapi.dll
		//also, This corrupts the designer... 
		//so i used the Release/Debug configuration
		bool IsCompositionEnabled()
		{
			return File.Exists(Environment.SystemDirectory + "\\dwmapi.dll")
				 && DwmIsCompositionEnabled();
		}

		//this one is used for a shadow when aero is not available
		protected override CreateParams CreateParams
		{
			get
			{
				const int CS_DROPSHADOW = 0x00020000;
				const int WS_MINIMIZEBOX = 0x20000;
				const int CS_DBLCLKS = 0x8;
				CreateParams cp = base.CreateParams;
				//if (!aero)
				cp.ClassStyle |= CS_DROPSHADOW;
				cp.Style |= WS_MINIMIZEBOX;
				cp.ClassStyle |= CS_DBLCLKS;
				return cp;
			}
		}

		//this is for aero shadow and border configurations
		protected override void OnLoad(EventArgs e)
		{
			base.OnLoad(e);
			if (aero)
			{
				MARGINS _glassMargins = new MARGINS()
				{
					Top = 5,
					Left = 5,
					Bottom = 5,
					Right = 5
				};
			}
			else
				FormBorderStyle = FormBorderStyle.None;
		}
		protected override void OnActivated(EventArgs e)
		{
			base.OnActivated(e);
			if (aero)
			{
				FormBorderStyle = FormBorderStyle.FixedSingle;
				ControlBox = false;
				MinimizeBox = false;
				MaximizeBox = false;
			}
		}
	}
}

