using System;
using System.Windows.Forms;


namespace BlueStacks.hyperDroid.Common
{
	public class CustomIDocHostUIHandler : IDocHostUIHandler
	{
		WebBrowser mBrowserObj;

		public CustomIDocHostUIHandler(WebBrowser browserObj)
		{
			mBrowserObj = browserObj;
		}

		int IDocHostUIHandler.ShowContextMenu(uint dwID, ref tagPOINT pt, object pcmdtReserved, object pdispReserved)
		{
			/*
			 * return S_OK to disable context menu
			 * return S_FALSE to enable context menu
			 */
			return Hresults.S_OK;
		}

		private DOCHOSTUIFLAG m_DocHostUiFlags = DOCHOSTUIFLAG.NO3DBORDER |
			DOCHOSTUIFLAG.FLAT_SCROLLBAR | DOCHOSTUIFLAG.THEME | DOCHOSTUIFLAG.DPI_AWARE;
		int IDocHostUIHandler.GetHostInfo(ref DOCHOSTUIINFO info)
		{
			//		MessageBox.Show("GetHostInfo");
			//Default, selecttext+no3dborder+flatscrollbars+themes(xp look)
			//Size has be set
			//		info.cbSize = (uint)Marshal.SizeOf(info);
			//		info.dwDoubleClick = (uint)m_DocHostUiDbClkFlag;
			info.dwFlags = (uint)m_DocHostUiFlags;
			return Hresults.S_OK;
		}

		int IDocHostUIHandler.ShowUI(int dwID, IOleInPlaceActiveObject activeObject, IOleCommandTarget commandTarget, IOleInPlaceFrame frame, IOleInPlaceUIWindow doc)
		{
			return Hresults.S_OK;
		}

		int IDocHostUIHandler.HideUI()
		{
			return Hresults.S_OK;
		}

		int IDocHostUIHandler.UpdateUI()
		{
			return Hresults.S_OK;
		}

		int IDocHostUIHandler.EnableModeless(bool fEnable)
		{
			return Hresults.E_NOTIMPL;
		}

		int IDocHostUIHandler.OnDocWindowActivate(bool fActivate)
		{
			return Hresults.E_NOTIMPL;
		}

		int IDocHostUIHandler.OnFrameWindowActivate(bool fActivate)
		{
			return Hresults.E_NOTIMPL;
		}

		int IDocHostUIHandler.ResizeBorder(ref tagRECT rect, IOleInPlaceUIWindow doc, bool fFrameWindow)
		{
			return Hresults.E_NOTIMPL;
		}

		int IDocHostUIHandler.TranslateAccelerator(ref tagMSG msg, ref Guid group, uint nCmdID)
		{
			return Hresults.S_FALSE;
		}

		int IDocHostUIHandler.GetOptionKeyPath(out string pbstrKey, uint dw)
		{
			pbstrKey = null;
			return Hresults.E_NOTIMPL;
		}

		int IDocHostUIHandler.GetDropTarget(IDropTarget pDropTarget, out IDropTarget ppDropTarget)
		{
			int hret = Hresults.E_NOTIMPL;
			ppDropTarget = null;
			return hret;
		}

		int IDocHostUIHandler.GetExternal(out object ppDispatch)
		{
			ppDispatch = mBrowserObj;
			return Hresults.S_OK;
		}

		int IDocHostUIHandler.TranslateUrl(uint dwTranslate, string strURLIn, out string pstrURLOut)
		{
			pstrURLOut = null;
			return Hresults.E_NOTIMPL;
		}

		int IDocHostUIHandler.FilterDataObject(System.Runtime.InteropServices.ComTypes.IDataObject pDO, out System.Runtime.InteropServices.ComTypes.IDataObject ppDORet)
		{
			ppDORet = null;
			return Hresults.E_NOTIMPL;
		}


		// ICustomQueryInterface

		/*
		public CustomQueryInterfaceResult GetInterface(ref Guid iid, out IntPtr ppv)
		{
			if (iid == typeof(IDocHostUIHandler).GUID)
			{
				ppv = Marshal.GetComInterfaceForObject(this, typeof(IDocHostUIHandler), CustomQueryInterfaceMode.Ignore);
			}
			else
			{
				ppv = IntPtr.Zero;
				return CustomQueryInterfaceResult.NotHandled;
			}
			return CustomQueryInterfaceResult.Handled;
		}   
		*/
	}
}
