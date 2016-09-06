/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * Camera Capture support
 */

using System;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;

namespace BlueStacks.hyperDroid.VideoCapture
{
	/*
	 * Define all needed guid's based on directshow sdk 7.1 v1 uuids.h
	 */
	public class Guids
	{
		/*
		 * Video device category
		 */
		public static readonly Guid VideoInputDeviceCategory = new Guid(0x860BB310, 0x5D01, 0x11d0, 0xBD, 0x3B, 0x00, 0xA0, 0xC9, 0x11, 0xCE, 0x86);

		/*
		 * Pin Category
		 */
		public static readonly Guid PinCategoryCapture = new Guid(0xfb6c4281, 0x0353, 0x11d1, 0x90, 0x5f, 0x00, 0x00, 0xc0, 0xcc, 0x16, 0xba);
		public static readonly Guid PinCategoryPreview = new Guid(0xfb6c4282, 0x0353, 0x11d1, 0x90, 0x5f, 0x00, 0x00, 0xc0, 0xcc, 0x16, 0xba);

		/*
		 * Media types, We need video only
		 */
		public static readonly Guid MediaTypeVideo = new Guid(0x73646976, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);

		/*
		 * MediaSubTypes, We intersted in RGB565, RGB24 and YUY formats
		 */
		public static readonly Guid MediaSubtypeRGB24 = new Guid(0xe436eb7d, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);
		public static readonly Guid MediaSubtypeYUY2 = new Guid(0x32595559, 0x0000, 0x0010, 0x80, 0x00, 0x00, 0xaa, 0x00, 0x38, 0x9b, 0x71);
		public static readonly Guid MediaSubtypeRGB565 = new Guid(0xe436eb7b, 0x524f, 0x11ce, 0x9f, 0x53, 0x00, 0x20, 0xaf, 0x0b, 0xa7, 0x70);

		/*
		 * FormatTypes, We intersted in Video only
		 */
		public static readonly Guid FormatTypesVideoInfo = new Guid(0x05589f80, 0xc356, 0x11ce, 0xbf, 0x01, 0x00, 0xaa, 0x00, 0x55, 0x59, 0x5a);

	}

	/*
	 * Define all needed interface based on directshow sdk 7.1 based on oaidl.h
	 */

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("3127CA40-446E-11CE-8135-00AA004BB851"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IErrorLog
	{
		int AddError(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
			[In] System.Runtime.InteropServices.ComTypes.EXCEPINFO pExcepInfo);

	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("55272A00-42CB-11CE-8135-00AA004BB851"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPropertyBag
	{
		int Read(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
			[Out, MarshalAs(UnmanagedType.Struct)] out object pVar,
			[In] IErrorLog pErrorLog
			);

		int Write(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszPropName,
			[In, MarshalAs(UnmanagedType.Struct)] ref object pVar
			);
	}

	/*
	 * based on uuids.h and strmif.h sdk 7.1
	 */
	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("29840822-5B84-11D0-BD3B-00A0C911CE86"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ICreateDevEnum
	{
		int CreateClassEnumerator(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pType,
			[Out] out IEnumMoniker ppEnumMoniker,
			[In, MarshalAs(UnmanagedType.I4)] int dwFlags);
	}

	/*
	 * CLSID_SystemDeviceEnum
	 */
	[ComImport, Guid("62BE5D10-60EB-11d0-BD3B-00A0C911CE86")]
	public class CreateDevEnum
	{
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a8689a-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMediaSample
	{
		int GetPointer([Out] out IntPtr ppBuffer);
		int GetSize();
		int GetTime(
			[Out] out long pTimeStart,
			[Out] out long pTimeEnd
			);
		int SetTime(
			[In] long pTimeStart,
			[In] long pTimeEnd
			);
		int IsSyncPoint();
		int SetSyncPoint([In, MarshalAs(UnmanagedType.Bool)] bool bIsSyncPoint);
		int IsPreroll();
		int SetPreroll([In, MarshalAs(UnmanagedType.Bool)] bool bIsPreroll);
		int GetActualDataLength();
		int SetActualDataLength([In] int len);
		int GetMediaType([Out, MarshalAs(UnmanagedType.LPStruct)] out AMMediaType ppMediaType);
		int SetMediaType([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pMediaType);
		int IsDiscontinuity();
		int SetDiscontinuity([In, MarshalAs(UnmanagedType.Bool)] bool bDiscontinuity);
		int GetMediaTime(
			[Out] out long pTimeStart,
			[Out] out long pTimeEnd
			);
		int SetMediaTime(
			[In] long pTimeStart,
			[In] long pTimeEnd
			);
	}

	/*
	 * Sample grabber orginaly from qedit.h headrs shipped with directx 8.1
	 * now this header file is removed though com dll is still shipped with windows 7
	 * So, we can safely use these guids
	 */
	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("6B652FFF-11FE-4fce-92AD-0266B5D7C78F"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISampleGrabber
	{
		int SetOneShot(
			[In, MarshalAs(UnmanagedType.Bool)] bool OneShot);
		int SetMediaType(
			[In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);
		int GetConnectedMediaType(
			[Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);
		int SetBufferSamples(
			[In, MarshalAs(UnmanagedType.Bool)] bool BufferThem);
		int GetCurrentBuffer(ref int pBufferSize, IntPtr pBuffer);
		int GetCurrentSample(out IMediaSample ppSample);
		int SetCallback(ISampleGrabberCB pCallback, int WhichMethodToCallback);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("0579154A-2B53-4994-B0D0-E773148EFF85"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ISampleGrabberCB
	{
		/*
		 * Callee releasse pSample
		 */
		//Without [PreserveSig] it runtime system throws error, its strange.
		[PreserveSig]
		int SampleCB(double SampleTime, IMediaSample pSample);
		[PreserveSig]
		int BufferCB(double SampleTime, IntPtr pBuffer, int BufferLen);
	}

	/*
	 * CLSID_FilterGraph based on sdk 7.1 uuids.h
	 */
	[ComImport, Guid("e436ebb3-524f-11ce-9f53-0020af0ba770")]
	public class FilterGraph
	{
	}

	/*
	 * CLSID_CaptureGraphBuilder2 based on sdk 7.1 uuids.h
	 */
	[ComImport, Guid("BF87B6E1-8C27-11d0-B3F0-00AA003761C5")]
	public class CaptureGraphBuilder2
	{
	}

	/*
	 * sdk7.1, ObjIdl.h
	 */
	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("0000010c-0000-0000-C000-000000000046"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPersist
	{
		int GetClassID([Out] out Guid pClassID);
	}

	/*
	 * sdk 7.1 strmif.h
	 */
	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a86897-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IReferenceClock
	{
		int GetTime([Out] out long pTime);
		int AdviseTime(
			[In] long baseTime,
			[In] long streamTime,
			[In] IntPtr hEvent, // System.Threading.WaitHandle?
			[Out] out int pdwAdviseCookie
			);
		int AdvisePeriodic(
			[In] long startTime,
			[In] long periodTime,
			[In] IntPtr hSemaphore, // System.Threading.WaitHandle?
			[Out] out int pdwAdviseCookie
			);
		int Unadvise([In] int dwAdviseCookie);
	}

	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct PinInfo
	{
		[MarshalAs(UnmanagedType.Interface)]
		public IBaseFilter filter;
		public PinDirection dir;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string name;
	}

	/*
	 * From FILTER_INFO sdk 7.1 strmif.h
	 */
	[StructLayout(LayoutKind.Sequential, CharSet = CharSet.Unicode)]
	public struct FilterInfo
	{
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst = 128)]
		public string achName;
		[MarshalAs(UnmanagedType.Interface)]
		public IFilterGraph pGraph;
	}

	/*
	 * sdk 7.1 strmif.h
	 */
	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a86893-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumFilters
	{
		int Next(
			[In] int cFilters,
			[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IBaseFilter[] ppFilter,
			[In] IntPtr pcFetched
			);
		int Skip([In] int cFilters);
		int Reset();
		int Clone([Out] out IEnumFilters ppEnum);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a8689f-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IFilterGraph
	{
		int AddFilter(
			[In] IBaseFilter pFilter,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pName
			);
		int RemoveFilter([In] IBaseFilter pFilter);
		int EnumFilters([Out] out IEnumFilters ppEnum);
		int FindFilterByName(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pName,
			[Out] out IBaseFilter ppFilter
			);
		int ConnectDirect(
			[In] IPin ppinOut,
			[In] IPin ppinIn,
			[In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
			);
		int Reconnect([In] IPin ppin);
		int Disconnect([In] IPin ppin);
		int SetDefaultSyncSource();
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a86899-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IMediaFilter : IPersist
	{
		new int GetClassID(
			[Out] out Guid pClassID);
		int Stop();
		int Pause();
		int Run([In] long tStart);
		int GetState(
			[In] int dwMilliSecsTimeout,
			[Out] out FilterState State
			);
		int SetSyncSource([In] IReferenceClock pClock);
		int GetSyncSource([Out] out IReferenceClock pClock);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("89c31040-846b-11ce-97d3-00aa0055595a"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumMediaTypes
	{
		int Next(
			[In] int cMediaTypes,
			//[In, Out, MarshalAs(UnmanagedType.CustomMarshaler, MarshalTypeRef = typeof(EMTMarshaler), SizeParamIndex = 0)] AMMediaType[] ppMediaTypes,
			[In, Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] AMMediaType[] ppMediaTypes,
			[In] IntPtr pcFetched
			);
		int Skip([In] int cMediaTypes);
		int Reset();
		int Clone([Out] out IEnumMediaTypes ppEnum);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a86891-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IPin
	{
		int Connect(
			[In] IPin pReceivePin,
			[In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
			);
		int ReceiveConnection(
			[In] IPin pReceivePin,
			[In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
			);
		int Disconnect();
		int ConnectedTo(
			[Out] out IPin ppPin);
		int ConnectionMediaType(
			[Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);
		int QueryPinInfo([Out] out PinInfo pInfo);
		int QueryDirection(out PinDirection pPinDir);
		int QueryId([Out, MarshalAs(UnmanagedType.LPWStr)] out string Id);
		int QueryAccept([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);
		int EnumMediaTypes([Out] out IEnumMediaTypes ppEnum);
		int QueryInternalConnections(
			[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 1)] IPin[] ppPins,
			[In, Out] ref int nPin
			);
		int EndOfStream();
		int BeginFlush();
		int EndFlush();
		int NewSegment(
			[In] long tStart,
			[In] long tStop,
			[In] double dRate
			);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a86892-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IEnumPins
	{
		int Next(
			[In] int cPins,
			[Out, MarshalAs(UnmanagedType.LPArray, SizeParamIndex = 0)] IPin[] ppPins,
			[In] IntPtr pcFetched
			);
		int Skip([In] int cPins);
		int Reset();
		int Clone([Out] out IEnumPins ppEnum);
	}


	/*
	 * sdk 7.1 strmif.h
	 */
	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a86895-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IBaseFilter : IMediaFilter
	{
		new int GetClassID(
			[Out] out Guid pClassID);
		new int Stop();
		new int Pause();
		new int Run(long tStart);
		new int GetState([In] int dwMilliSecsTimeout, [Out] out FilterState filtState);
		new int SetSyncSource([In] IReferenceClock pClock);
		new int GetSyncSource([Out] out IReferenceClock pClock);
		int EnumPins([Out] out IEnumPins ppEnum);
		int FindPin(
			[In, MarshalAs(UnmanagedType.LPWStr)] string Id,
			[Out] out IPin ppPin
			);
		int QueryFilterInfo([Out] out FilterInfo pInfo);
		int JoinFilterGraph(
			[In] IFilterGraph pGraph,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pName
			);
		int QueryVendorInfo([Out, MarshalAs(UnmanagedType.LPWStr)] out string pVendorInfo);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("a2104830-7c70-11cf-8bce-00aa00a3f1a6"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IFileSinkFilter
	{
		int SetFileName(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pszFileName,
			[In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
			);
		int GetCurFile(
			[Out, MarshalAs(UnmanagedType.LPWStr)] out string pszFileName,
			[Out, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt
			);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("670d1d20-a068-11d0-b3f0-00aa003761c5"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAMCopyCaptureFileProgress
	{

		int Progress(int iProgress);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a868a9-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IGraphBuilder : IFilterGraph
	{
		new int AddFilter(
			[In] IBaseFilter pFilter,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pName
			);
		new int RemoveFilter([In] IBaseFilter pFilter);
		new int EnumFilters([Out] out IEnumFilters ppEnum);
		new int FindFilterByName(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pName,
			[Out] out IBaseFilter ppFilter
			);
		new int ConnectDirect(
			[In] IPin ppinOut,
			[In] IPin ppinIn,
			[In, MarshalAs(UnmanagedType.LPStruct)]
			AMMediaType pmt
			);
		new int Reconnect([In] IPin ppin);
		new int Disconnect([In] IPin ppin);
		new int SetDefaultSyncSource();
		int Connect(
			[In] IPin ppinOut,
			[In] IPin ppinIn
			);
		int Render([In] IPin ppinOut);
		int RenderFile(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList
			);
		int AddSourceFilter(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
			[Out] out IBaseFilter ppFilter
			);
		int SetLogFile(IntPtr hFile);
		int Abort();
		int ShouldOperationContinue();
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("93E5A4E0-2D50-11d2-ABFA-00A0C9C6E38D"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface ICaptureGraphBuilder2
	{
		int SetFiltergraph([In] IGraphBuilder pfg);
		int GetFiltergraph([Out] out IGraphBuilder ppfg);
		int SetOutputFileName(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pType,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpstrFile,
			[Out] out IBaseFilter ppbf,
			[Out] out IFileSinkFilter ppSink
			);
		int FindInterface(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pCategory,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pType,
			[In] IBaseFilter pf,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid riid,
			[Out, MarshalAs(UnmanagedType.IUnknown)] out object ppint
			);
		int RenderStream(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pCategory,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pType,
			[In, MarshalAs(UnmanagedType.IUnknown)] object pSource,
			[In] IBaseFilter pCompressor,
			[In] IBaseFilter pRenderer
			);
		int ControlStream(
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pCategory,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pType,
			[In, MarshalAs(UnmanagedType.Interface)] IBaseFilter pFilter,
			[In] long pstart,
			[In] long pstop,
			[In] short wStartCookie,
			[In] short wStopCookie
			);
		int AllocCapFile(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpstr,
			[In] long dwlSize
			);
		int CopyCaptureFile(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpwstrOld,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpwstrNew,
			[In, MarshalAs(UnmanagedType.Bool)] bool fAllowEscAbort,
			[In] IAMCopyCaptureFileProgress pCallback
			);
		int FindPin(
			[In, MarshalAs(UnmanagedType.IUnknown)] object pSource,
			[In] PinDirection pindir,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pCategory,
			[In, MarshalAs(UnmanagedType.LPStruct)] Guid pType,
			[In, MarshalAs(UnmanagedType.Bool)] bool fUnconnected,
			[In] int num,
			[Out, MarshalAs(UnmanagedType.Interface)] out IPin ppPin
			);
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("36b73882-c2c8-11cf-8b46-00805f6cef60"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IFilterGraph2 : IGraphBuilder
	{
		new int AddFilter(
			[In] IBaseFilter pFilter,
			[In, MarshalAs(UnmanagedType.LPWStr)] string pName
			);
		new int RemoveFilter([In] IBaseFilter pFilter);
		new int EnumFilters([Out] out IEnumFilters ppEnum);
		new int FindFilterByName(
			[In, MarshalAs(UnmanagedType.LPWStr)] string pName,
			[Out] out IBaseFilter ppFilter
			);
		new int ConnectDirect(
			[In] IPin ppinOut,
			[In] IPin ppinIn,
			[In, MarshalAs(UnmanagedType.LPStruct)]
			AMMediaType pmt
			);
		new int Reconnect([In] IPin ppin);
		new int Disconnect([In] IPin ppin);
		new int SetDefaultSyncSource();
		new int Connect(
			[In] IPin ppinOut,
			[In] IPin ppinIn
			);
		new int Render([In] IPin ppinOut);
		new int RenderFile(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFile,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrPlayList
			);
		new int AddSourceFilter(
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFileName,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
			[Out] out IBaseFilter ppFilter
			);
		new int SetLogFile(IntPtr hFile);
		new int Abort();
		new int ShouldOperationContinue();
		int AddSourceFilterForMoniker(
			[In] IMoniker pMoniker,
			[In] IBindCtx pCtx,
			[In, MarshalAs(UnmanagedType.LPWStr)] string lpcwstrFilterName,
			[Out] out IBaseFilter ppFilter
			);
		int ReconnectEx(
			[In] IPin ppin,
			[In] AMMediaType pmt
			);
		int RenderEx(
			[In] IPin pPinOut,
			[In] int dwFlags,
			[In] IntPtr pvContext
			);
	}

	/*
	 * sdk 7.1 control.h
	 */
	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("56a868b1-0ad4-11ce-b03a-0020af0ba770"),
	InterfaceType(ComInterfaceType.InterfaceIsDual)]
	public interface IMediaControl
	{
		int Run();
		int Pause();
		int Stop();
		int GetState([In] int msTimeout, [Out] out FilterState pfs);
		int RenderFile([In, MarshalAs(UnmanagedType.BStr)] string strFilename);

		[Obsolete("MSDN: Intended for Visual Basic 6.0; not documented here.", false)]
		int AddSourceFilter(
			[In, MarshalAs(UnmanagedType.BStr)] string strFilename,
			[Out, MarshalAs(UnmanagedType.IDispatch)] out object ppUnk
			);

		[Obsolete("MSDN: Intended for Visual Basic 6.0; not documented here.", false)]
		int get_FilterCollection([Out, MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);

		[Obsolete("MSDN: Intended for Visual Basic 6.0; not documented here.", false)]
		int get_RegFilterCollection([Out, MarshalAs(UnmanagedType.IDispatch)] out object ppUnk);
		int StopWhenReady();
	}

	/*
	 * CLSID_SampleGrabber
	 */
	[ComImport, Guid("C1F400A0-3F08-11d3-9F0B-006008039E37")]
	public class SampleGrabber
	{
	}

	[ComImport, System.Security.SuppressUnmanagedCodeSecurity,
	Guid("C6E13340-30AC-11d0-A18C-00A0C9118956"),
	InterfaceType(ComInterfaceType.InterfaceIsIUnknown)]
	public interface IAMStreamConfig
	{
		//Without [PreserveSig] it runtime system throws error, its strange.
		[PreserveSig]
		int SetFormat([In, MarshalAs(UnmanagedType.LPStruct)] AMMediaType pmt);

		[PreserveSig]
		int GetFormat([Out] out AMMediaType pmt);

		[PreserveSig]
		int GetNumberOfCapabilities(out int piCount, out int piSize);

		[PreserveSig]
		int GetStreamCaps(
			[In] int iIndex,
			[Out] out AMMediaType ppmt,
			[In] IntPtr pSCC
			);
	}

	[StructLayout(LayoutKind.Sequential, Pack = 2)]
	public class BitmapInfoHeader
	{
		public int Size;
		public int Width;
		public int Height;
		public short Planes;
		public short BitCount;
		public int Compression;
		public int ImageSize;
		public int XPelsPerMeter;
		public int YPelsPerMeter;
		public int ClrUsed;
		public int ClrImportant;
	}

	[StructLayout(LayoutKind.Sequential)]
	public struct RECT
	{
		int left;
		int top;
		int right;
		int bottom;
	}

	[StructLayout(LayoutKind.Sequential)]
	public class VideoInfoHeader
	{
		public RECT SrcRect;
		public RECT TargetRect;
		public int BitRate;
		public int BitErrorRate;
		public long AvgTimePerFrame;
		public BitmapInfoHeader BmiHeader;
	}

	/*
	 * _AMMediaType typedef as AM_MEDIA_TYPE based on strmif.h
	 */
	[StructLayout(LayoutKind.Sequential)]
	public class AMMediaType
	{
		public Guid majorType;
		public Guid subType;
		[MarshalAs(UnmanagedType.Bool)]
		public bool fixedSizeSamples;
		[MarshalAs(UnmanagedType.Bool)]
		public bool temporalCompression;
		public int sampleSize;
		public Guid formatType;
		public IntPtr pUnk;
		public int cbFormat;
		public IntPtr pbFormat; // Pointer to a buff determined by formatType
	}

	/*
	 * From PIN_DIRECTION
	 */
	public enum PinDirection
	{
		PIN_Input,
		PIN_Output
	}

	/*
	 * sdk 7.1 strmif.h
	 */
	public enum FilterState
	{
		State_Stopped = 0,
		State_Paused = (State_Stopped + 1),
		State_Running = (State_Paused + 1)
	}

}
