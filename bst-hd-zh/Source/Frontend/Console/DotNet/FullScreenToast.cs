using System;
using System.Windows.Forms;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class FullScreenToast
	{

		private Control mParent;
		private Toast mToast;
		private System.Windows.Forms.Timer mTimer;

		public FullScreenToast(Control parent)
		{
			mParent = parent;

			mTimer = new System.Windows.Forms.Timer();
			mTimer.Interval = 5000;
			mTimer.Tick += Timeout;
		}

		public void Show()
		{
			Hide();

			mToast = new Toast(mParent,
				Locale.Strings.FullScreenToastText);

			int flags =
				Interop.Animate.AW_SLIDE |
				Interop.Animate.AW_VER_POSITIVE;

			Interop.Animate.AnimateWindow(mToast.Handle, 500, flags);
			mToast.Show();

			mTimer.Start();
		}

		public void Hide()
		{
			mTimer.Stop();

			if (mToast != null)
			{
				mToast.Hide();
				mToast = null;
			}
		}

		private void Timeout(Object obj, EventArgs evt)
		{
			mTimer.Stop();

			int flags =
				Interop.Animate.AW_HIDE |
				Interop.Animate.AW_SLIDE |
				Interop.Animate.AW_VER_NEGATIVE;

			if (mToast != null)
			{
				Interop.Animate.AnimateWindow(mToast.Handle, 500,
					flags);
				mToast.Hide();
			}
		}
	}

}
