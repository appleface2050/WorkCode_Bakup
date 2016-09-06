using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Diagnostics;
using System.Drawing;
using System.Globalization;
using System.IO;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading;
using System.Web;
using Microsoft.Win32;
using System.Windows.Forms;

using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Cloud.Services;
using CodeTitans.JSon;

namespace BlueStacks.hyperDroid.GameManager {

	public class FoneLinkNotifications
	{
		public class NotificationInfo
		{
			public Image		Icon;
			public string		AppName;
			public string		Package;
			public string		Title;
			public string		Text;
			public string		Content;
			public string		AdditionalText;
			public long 		ReceivedAt;
			public string		Id;
			public string		Tag;
			public int		Priority;
			public NotificationDialog Dialog;
		}

		static List<NotificationInfo> sActiveNotifications = new List<NotificationInfo>();
		static bool sShutdown = false;


		public static void Show(string icon,
				string appName,
				string package,
				string title,
				string text, 
				string content,
				string additionalText,
				string receivedAtMillis,
				string tag,
				string id,
				int priority
				)
		{
			if (sShutdown == true)
				return;

			/*
			 * Check if the package is muted or if we want to ignore
			 * it for other reasons.
			 */
			if (IsMirroringDisabled(package))
				return;

			NotificationInfo n 	= new NotificationInfo();

			n.Icon			= Base64ToImage(icon);
			n.AppName		= appName;
			n.Package		= package;
			n.Title			= title;
			n.Text			= text;
			n.Content		= content;
			n.AdditionalText 	= additionalText;
			n.ReceivedAt		= Convert.ToInt64(receivedAtMillis) / 1000;
			n.Tag			= tag;
			n.Id			= id;
			n.Priority		= priority;

			ShowInternal(n);
		}

		public static void ShowSms(string icon,
			       	string appName,
			       	string package,
			       	string originatingAddress,
			       	string body, 
				string timestampMillis)
		{
			NotificationInfo n = new NotificationInfo();

			n.Icon		= Base64ToImage(icon);
			n.AppName	= appName;
			n.Package	= package;
			n.Title		= originatingAddress;
			n.Text		= body;
			n.ReceivedAt	= Convert.ToInt64(timestampMillis) / 1000;
			n.Tag		= originatingAddress;
			n.Id		= "0";
			n.Priority	= 0;

			ShowInternal(n);
		}

		public static void Shutdown()
		{
			sShutdown = true;
			foreach (NotificationInfo m in sActiveNotifications)
			{
				m.Dialog.Close();
			}
		}

		private static void ShowInternal(NotificationInfo n)
		{

			/*
			 * If and older dialog exists for this (package, tag, id) close it.
			 */
			NotificationInfo oldn = sActiveNotifications.Find(delegate(NotificationInfo m)
					{
					if (m.Package == n.Package &&
						m.Tag == n.Tag &&
						m.Id == n.Id)
					return true;

					return false;
					});

			if (oldn != null)
			{
				oldn.Dialog.Close();
				sActiveNotifications.Remove(oldn);
			}

			CreateNotificationDialog(n);
			sActiveNotifications.Insert(0, n);

			int y = Screen.PrimaryScreen.WorkingArea.Bottom;
			foreach (NotificationInfo m in sActiveNotifications)
			{
				y -= m.Dialog.Height + 8;
				m.Dialog.Top = y;
				m.Dialog.TopMost = true;
			}

		}


		static Image Base64ToImage(string icon)
		{
			byte[] imgBytes = Convert.FromBase64String(icon);
			Image image = null;
			using(MemoryStream stream = new MemoryStream(imgBytes, 0, imgBytes.Length))
			{
				image = Image.FromStream(stream);
			}
			return image;
		}

		static void CreateNotificationDialog(NotificationInfo n)
		{
			Logger.Info("FoneLinkNotifications: CreateNotificationDialog: Package={0}, Tag={1}, Id={2}",
				       	n.Package, n.Tag, n.Id);

			n.Dialog = new NotificationDialog(
					n, 
					delegate (Object o, EventArgs e) {
					Logger.Info("notification-clicked Package={0}, Id={1}", n.Package, n.Id);
				});

			n.Dialog.Show();
		}

		private static bool DisableMirroring(string package)
		{
			int numItems = 0;
			string[] packageList;
			RegistryKey prodKey = Registry.LocalMachine.CreateSubKey(Common.Strings.GMBasePath);
			try
			{
				packageList = (string[])prodKey.GetValue("MirroringDisabled");
				numItems = packageList.Length;
				for (int i = 0; i < numItems; i++)
				{
					if (String.Compare(packageList[i], package, true) == 0)
					{
						Logger.Info("{0} already in MirroringDisabled list. Ignoring", package);
						return false;
					}
				}
			}
			catch (Exception)
			{
				packageList = new string[1];	// to supress unassigned variable error
				// Ignore
			}

			string[] newList = new string[numItems + 1];
			for (int i = 0; i < numItems; i++)
			{
				newList[i] = packageList[i];
			}

			newList[numItems] = package;
			prodKey.SetValue("MirroringDisabled", newList);
			return true;
		}

		private static bool IsMirroringDisabled(string package)
		{
			try
			{
				RegistryKey prodKey = Registry.LocalMachine.OpenSubKey(Common.Strings.GMBasePath);
				string[] packageList = (string[])prodKey.GetValue("MirroringDisabled");
				for (int i = 0; i < packageList.Length; i++)
				{
					if (String.Compare(packageList[i], package, true) == 0)
					{
						return true;
					}
				}
			}
			catch (Exception)
			{
				// Ignore
			}

			return false;
		}

		public class NotificationDialog: Form
		{
			private const int WS_EX_TOPMOST		= 0x00000008;
			private const int WS_EX_TOOLWINDOW	= 0x00000080;
			private const int CS_DROPSHADOW		= 0x00020000;

			private NotificationInfo mInfo;

			protected override CreateParams CreateParams
			{
				get
				{
					CreateParams cp = base.CreateParams;
					cp.ExStyle |= WS_EX_TOOLWINDOW | WS_EX_TOPMOST;
					cp.ClassStyle |= CS_DROPSHADOW;
					return cp;
				}
			}

			private void ClickHandler(object sender, EventArgs e)
			{
				this.Close();
				Logger.Info("notification-clicked package={0}, id={1}",
						mInfo.Package, mInfo.Id);

			}

			private void DismissHandler(object sender, EventArgs e)
			{
				this.Close();
				Logger.Info("notification-dismissed package={0}, id={1}",
						mInfo.Package, mInfo.Id);
			}

			private void FormClosedHandler(object sender, FormClosedEventArgs args)
			{
				if (sShutdown == true)
					return;

				sActiveNotifications.Remove(mInfo);

				int y = Screen.PrimaryScreen.WorkingArea.Bottom;
				foreach (NotificationInfo m in sActiveNotifications)
				{
					y -= m.Dialog.Height + 8;
					m.Dialog.Top = y;
					m.Dialog.TopMost = true;
				}
			}

			public NotificationDialog(
					NotificationInfo	n,
					System.EventHandler 	clickHandler
					)
			{
				mInfo 			= n;
				this.SuspendLayout();

				this.ShowInTaskbar	= false;
				this.TopMost		= true;
				this.FormBorderStyle	= FormBorderStyle.None;
				this.FormClosed		+= FormClosedHandler;

				/*
				 * Main Panel
				 */
				Panel mainPanel		= new Panel();
				mainPanel.BackColor 	= Color.FromArgb(251, 251, 251);
				mainPanel.Location	= new Point(0, 0);
				mainPanel.Width		=  (int)(Screen.PrimaryScreen.WorkingArea.Width*(1.0/6));
				mainPanel.BorderStyle	= BorderStyle.None;
				mainPanel.Click		+= ClickHandler;

				this.Controls.Add(mainPanel);

				/*
				 * Small Icon
				 */
				PictureBox smallIcon	= new PictureBox();
				smallIcon.BackColor	= Color.Transparent;
				smallIcon.Image		= n.Icon;
				smallIcon.Size		= new Size(48, 48);
				smallIcon.Location	= new Point(8, 8);
				smallIcon.SizeMode	= PictureBoxSizeMode.Zoom;
				smallIcon.Click		+= ClickHandler;

				mainPanel.Controls.Add(smallIcon);

				/*
				 * Close Button
				 */
				PictureBox closeButton	= new PictureBox();
				closeButton.BackColor	= Color.Transparent;
				closeButton.Image	= Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "dismiss.png"));
				closeButton.Size	= new Size(32, 32);
				closeButton.Location	= new Point(mainPanel.Width-8-closeButton.Width, 8);
				closeButton.SizeMode	= PictureBoxSizeMode.CenterImage;
				closeButton.Click	+= DismissHandler;

				mainPanel.Controls.Add(closeButton);

				/*
				 * Dialog title - "Via Bluestacks-FoneLink ..."
				 */
				Label dialogTitle	= new Label();
				dialogTitle.BackColor	= Color.Transparent;
				dialogTitle.ForeColor	= Color.Gray;
				dialogTitle.Location	= new Point(smallIcon.Right+8, smallIcon.Top);
				dialogTitle.Size	= new Size(closeButton.Left-smallIcon.Right-8*2, smallIcon.Height);
				dialogTitle.Text	= "Via Bluestacks-FoneLink at " + DateTime.Now.ToString("h:mm tt, MMM dd");
				dialogTitle.Font	= new Font("Segoe UI", 9F);// XXX: What about WinXP/Win8/etc
				dialogTitle.TextAlign	= ContentAlignment.MiddleLeft;
				dialogTitle.Click	+= ClickHandler;

				mainPanel.Controls.Add(dialogTitle);

				/*
				 * Content title
				 */
				Label contentTitle	= new Label();
				contentTitle.BackColor	= Color.Transparent;
				contentTitle.ForeColor	= Color.Black;
				contentTitle.Location	= new Point(8, smallIcon.Bottom+8);
				contentTitle.Size	= new Size(mainPanel.Width-8*2, 32);
				contentTitle.Text	= n.AppName + ": " + n.Title;
				contentTitle.Font	= new Font("Segoe UI", 10F, FontStyle.Bold);// XXX: What about WinXP/Win8/etc
				contentTitle.TextAlign	= ContentAlignment.MiddleLeft;
				contentTitle.Click	+= ClickHandler;

				mainPanel.Controls.Add(contentTitle);

				/*
				 * Content text
				 */
				Label contentText		= new Label();
				contentText.BackColor		= Color.Transparent;
				contentText.ForeColor		= Color.Black;
				contentText.Location		= new Point(8, contentTitle.Bottom+8);
				contentText.Size		= new Size(mainPanel.Width-8*2, 16);
				contentText.MaximumSize		= new Size(contentText.Width, 200);
				contentText.AutoSize		= true;
				contentText.AutoEllipsis	= true;
				contentText.Text		= new StringBuilder(n.Text)
					.AppendLine().Append(n.Content)
					.AppendLine().Append(n.AdditionalText).ToString();
				contentText.Font		= new Font("Segoe UI", 10F);// XXX: What about WinXP/Win8/etc
				contentText.TextAlign		= ContentAlignment.TopLeft;
				contentText.Click		+= ClickHandler;

				mainPanel.Controls.Add(contentText);

				/*
				 * Separator 1
				 */
				Label separator1	= new Label();
				separator1.BackColor	= Color.FromArgb(240, 240, 240);
				separator1.Location	= new Point(0, contentText.Bottom+24);
				separator1.Size		= new Size(mainPanel.Width, 1);

				mainPanel.Controls.Add(separator1);

				/* 
				 * Dismiss Icon
				 */
				PictureBox dismissIcon	= new PictureBox();
				dismissIcon.BackColor	= Color.Transparent;
				dismissIcon.Image	= Image.FromFile(Path.Combine(Common.Strings.GMAssetDir, "dismiss.png"));
				dismissIcon.Size	= new Size(32, 32);
				dismissIcon.Location	= new Point(8, separator1.Bottom+8);
				dismissIcon.SizeMode	= PictureBoxSizeMode.CenterImage;
				dismissIcon.Click	+= DismissHandler;

				mainPanel.Controls.Add(dismissIcon);

				/*
				 * Dismiss Text
				 */
				Label dismissText	= new Label();
				dismissText.BackColor	= Color.Transparent;
				dismissText.ForeColor	= Color.Black;
				dismissText.Location	= new Point(dismissIcon.Right+8, dismissIcon.Top);
				dismissText.Size	= new Size(mainPanel.Width-8*2, dismissIcon.Height);
				dismissText.Text	= "Dismiss";// XXX: I18N
				dismissText.Font	= new Font("Segoe UI", 9F);// XXX: What about WinXP/Win8/etc
				dismissText.TextAlign	= ContentAlignment.MiddleLeft;
				dismissText.Click	+= DismissHandler;

				mainPanel.Controls.Add(dismissText);


				mainPanel.Height	= dismissText.Bottom+8;
				this.ClientSize		= mainPanel.Size;

				this.StartPosition	= FormStartPosition.Manual;
				this.Location	= new Point(
						Screen.PrimaryScreen.WorkingArea.Right - this.Width - 8,
						Screen.PrimaryScreen.WorkingArea.Bottom - this.Height - 8);

				this.ResumeLayout(false);

				Logger.Info("notification-displayed package={0}, id={1}", n.Package, n.Id);
			}

		}
	}

}
