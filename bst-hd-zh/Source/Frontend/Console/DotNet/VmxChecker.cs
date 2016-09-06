using System;
using System.Windows.Forms;
using System.Threading;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class VmxChecker
	{

		private Thread mThread;
		private Control mParent;
		private String mTitle;
		private String mText;

		public VmxChecker(Control parent, String title, String text)
		{
			mThread = new Thread(ThreadEntry);
			mThread.IsBackground = true;

			mParent = parent;
			mTitle = title;
			mText = text;
		}

		public void Start()
		{
			mThread.Start();
		}

		private void ThreadEntry()
		{
			try
			{
				ThreadEntryInternal();

			}
			catch (ObjectDisposedException)
			{
			}
		}

		private void ThreadEntryInternal()
		{
			Logger.Info("Starting VMX checker thread");

			Thread.Sleep(5000);

			while (true)
			{

				/*
				if (!IsParentVisible()) {
					Thread.Sleep(1000);
					continue;
				}
				*/

				if (Interop.Manager.IsVmxActive())
				{
					Logger.Info("VMX is active");
					if (BlueStacks.hyperDroid.Common.Oem.Instance.IsNotifyChangesToParentWindow)
					{
						WindowMessages.NotifyVMXBitOnToParentWindow();
					}
					else
					{
						WarnAndQuit();
					}
					break;
				}

				Thread.Sleep(1000);
			}
		}

		/*
		private bool IsParentVisible()
		{
			bool visible = false;

			UIHelper.RunOnUIThread(mParent,
				delegate() {
				visible = mParent.Visible;
			});

			return visible;
		}
		*/

		private void WarnAndQuit()
		{
			UIHelper.RunOnUIThread(mParent,
					delegate ()
					{
						MessageBox.Show(mText, mTitle);
					//mParent.Close();
				});
		}
	}

}
