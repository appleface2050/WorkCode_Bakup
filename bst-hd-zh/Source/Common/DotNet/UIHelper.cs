using System;
using System.Windows.Forms;

namespace BlueStacks.hyperDroid.Common
{

	public static class UIHelper
	{

		public delegate object dispatcher(Delegate method, params object[] args);
		static dispatcher obj = null;
		public static void SetDispatcher(dispatcher gameManagerWindowDispatcher)
		{
			obj = gameManagerWindowDispatcher;
		}
		public delegate void Action();

		public static void RunOnUIThread(Control control, Action action)
		{
			if (obj == null)
			{
				if (control.InvokeRequired)
					control.Invoke(action);
				else
					action.Invoke();
			}
			else
			{
				if (control.InvokeRequired)
					obj.Invoke(action);
			}
		}

		public static void AssertUIThread(Control control)
		{
			if (control.InvokeRequired)
				throw new ApplicationException("Not running on UI thread");
		}
	};

}
