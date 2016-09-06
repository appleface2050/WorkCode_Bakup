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
using System.IO;
using System.Runtime.InteropServices;
using System.Runtime.InteropServices.ComTypes;
using System.Threading;
using BlueStacks.hyperDroid.Common;
using System.Collections.Generic;

namespace BlueStacks.hyperDroid.VideoCapture
{
	public enum SupportedColorFormat
	{
		YUV2,
		RGB24,
		LAST
	};

	public class DeviceEnumerator : IDisposable
	{
		private IMoniker m_Moniker;
		private string m_FriendlyName;

		public static List<DeviceEnumerator> ListDevices(Guid filterType)
		{
			ErrorHandler hr;
			ICreateDevEnum pSysDevEnum = null;
			IEnumMoniker pEnumCat;
			List<DeviceEnumerator> devices = new List<DeviceEnumerator>();
			DeviceEnumerator device = null;

			pSysDevEnum = (ICreateDevEnum)new CreateDevEnum();
			hr = pSysDevEnum.CreateClassEnumerator(filterType, out pEnumCat, 0);

			if (pEnumCat != null)
			{
				try
				{
					IMoniker[] pMoniker = null;
					try
					{
						while (true)
						{
							pMoniker = new IMoniker[1];
							if (pEnumCat.Next(1, pMoniker, IntPtr.Zero) != 0)
							{
								Logger.Info("Breaking out of loop..");
								break;
							}
							device = new DeviceEnumerator();
							device.m_Moniker = pMoniker[0];
							device.m_FriendlyName = device.getProperty("FriendlyName");
							String devicePath = device.getProperty("DevicePath");
							if (!devicePath.Contains("\\usb#vid") &&
									!devicePath.Contains("\\pci#ven"))
							{
								continue;
							}
							devices.Add(device);
							Logger.Info("Camera device {0}", device.m_FriendlyName);
						}
					}
					catch (Exception e)
					{
						if (pMoniker != null)
							Marshal.ReleaseComObject(pMoniker[0]);
						devices = null;
						Logger.Error("Failed to enumerate Video input devices: {0}", e.ToString());
						throw;
					}
				}
				finally
				{
					Marshal.ReleaseComObject(pEnumCat);
				}
			}
			else
			{
				Logger.Error("Cannot enumerate the device");
				devices = null;

			}
			return devices;
		}

		public void Dispose()
		{
			if (m_Moniker != null)
			{
				Marshal.ReleaseComObject(m_Moniker);
				m_Moniker = null;
			}
			m_FriendlyName = null;
		}

		public string FriendlyName
		{
			get
			{
				return m_FriendlyName;
			}
		}

		public IMoniker Moniker
		{
			get
			{
				return m_Moniker;
			}
		}

		public Guid ClassGUID
		{
			get
			{
				Guid guid;

				m_Moniker.GetClassID(out guid);
				return guid;
			}
		}

		public string GetDisplayName
		{
			get
			{
				string name = null;
				try
				{
					m_Moniker.GetDisplayName(null, null, out name);
				}
				catch (Exception e)
				{
					Logger.Error(e.ToString());
				}
				return name;
			}
		}

		public string getProperty(string sProperty)
		{
			object prop = null;
			IPropertyBag pProp = null;
			string retProp = null;
			Guid pGuid = typeof(IPropertyBag).GUID;
			ErrorHandler hr;
			try
			{
				object name;
				m_Moniker.BindToStorage(null, null, ref pGuid, out prop);
				pProp = (IPropertyBag)prop;
				hr = pProp.Read(sProperty, out name, null);
				retProp = (string)name;
			}
			catch (Exception e)
			{
				Logger.Error("Failed to fetch Property: {0}", sProperty);
				Logger.Error(e.ToString());
			}
			finally
			{
				prop = null;
				if (pProp != null)
				{
					Marshal.ReleaseComObject(pProp);
					pProp = null;
				}
			}
			return retProp;
		}
	}

	public class ColorFormatNotSupported : Exception
	{
		public ColorFormatNotSupported() : base() { }
		public ColorFormatNotSupported(string message) : base(message) { }
		public ColorFormatNotSupported(string message, Exception inner) : base(message, inner) { }
	}

	public class CaptureGraph : ISampleGrabberCB, IDisposable
	{
		private int m_Unit;
		private int m_Width;
		private int m_Height;
		private int m_FrameRate;
		private int m_Stride;
		private int m_DroppedFrame;
		private IntPtr m_Buffer = IntPtr.Zero;
		private ManualResetEvent m_Evt = null;
		private bool m_bGraphRunning = false;
		private volatile bool m_bGrabFrame = false;
		private SupportedColorFormat m_color;

		//Graph builder
		private IFilterGraph2 m_FilterGraph = null;
		private IMediaControl m_mediaCtrl = null;


		[DllImport("Kernel32.dll", EntryPoint = "RtlMoveMemory")]
		private static extern void CopyMemory(IntPtr Destination, IntPtr Source, int Length);

		public CaptureGraph(int unit, int width, int height, int framerate, SupportedColorFormat color)
		{
			m_Unit = unit;
			m_Width = width;
			m_Height = height;
			m_FrameRate = framerate;
			m_DroppedFrame = 0;
			m_color = color;
			m_Evt = new ManualResetEvent(false);
			m_bGraphRunning = false;
			Logger.Info("Building graph");
			try
			{
				BuildGraph();
			}
			catch (Exception e)
			{
				Logger.Error("Failed to build graph. Exception: {0}", e.ToString());
				Dispose();
				throw;
			}

		}

		~CaptureGraph()
		{
			if (m_Buffer != IntPtr.Zero)
			{
				Marshal.FreeCoTaskMem(m_Buffer);
				m_Buffer = IntPtr.Zero;
			}
			Dispose();
		}

		public void BuildGraph()
		{
			ErrorHandler hr;
			ICaptureGraphBuilder2 capGraph = null;
			IBaseFilter capFilter = null;
			ISampleGrabber grabber = null;
			List<DeviceEnumerator> devices = null;
			DeviceEnumerator device = null;
			AMMediaType media;
			try
			{
				Logger.Info("Creating List of devices");
				devices = DeviceEnumerator.ListDevices(Guids.VideoInputDeviceCategory);
			}
			catch (Exception e)
			{
				Logger.Error("No Video device found : {0}", e.ToString());
			}
			if (devices == null || devices.Count == 0)
			{
				Logger.Info("CAMERA: Could not find a camera device!");
				return;
			}
			try
			{
				Logger.Info("found {0} Camera, Opening {1}", devices.Count, m_Unit);
				if (m_Unit < devices.Count)
				{
					device = devices[m_Unit];
				}
				else
				{
					device = devices[0];
				}

				m_FilterGraph = (IFilterGraph2)new FilterGraph();

				//Query interface for medaiControl
				m_mediaCtrl = m_FilterGraph as IMediaControl;

				//Craete a graph builder
				capGraph = (ICaptureGraphBuilder2)new CaptureGraphBuilder2();

				//get BaseFilter
				//capFilter = (IBaseFilter)new FilterGraph();

				//Create SampleGrabber
				grabber = (ISampleGrabber)new SampleGrabber();

				//Build the graph
				hr = capGraph.SetFiltergraph(m_FilterGraph);
				//TODO: error handling
				if (hr.GetError() != 0)
				{
					Logger.Error("SetFiltergraph failed with {0:X}..", hr.GetError());
				}

				//Add video device
				hr = m_FilterGraph.AddSourceFilterForMoniker(device.Moniker, null, "Video input", out capFilter);

				if (hr.GetError() != 0)
				{
					Logger.Error("AddSourceFilterForMoniker failed with {0:X}", hr.GetError());
				}
				media = new AMMediaType();
				media.majorType = Guids.MediaTypeVideo;
				//Defines video encoding
				if (m_color == SupportedColorFormat.YUV2)
					media.subType = Guids.MediaSubtypeYUY2;
				else if (m_color == SupportedColorFormat.RGB24)
					media.subType = Guids.MediaSubtypeRGB24;
				else
					throw new Exception("Unsupported color format");

				media.formatType = Guids.FormatTypesVideoInfo;

				hr = grabber.SetMediaType(media);

				FreeAMMedia(media);

				hr = grabber.SetCallback(this, 1);
				if (hr.GetError() != 0)
				{
					Logger.Error("Grabber setcallback failed with {0:X}", hr.GetError());
				}

				IBaseFilter baseFilter = (IBaseFilter)grabber;
				hr = m_FilterGraph.AddFilter(baseFilter, "FrameGrabber");

				if (hr.GetError() != 0)
				{
					Logger.Error("AddFilter failed with {0:X}", hr.GetError());
				}
				object obj;
				hr = capGraph.FindInterface(Guids.PinCategoryCapture, Guids.MediaTypeVideo, capFilter, typeof(IAMStreamConfig).GUID, out obj);

				if (hr.GetError() != 0)
				{
					Logger.Error("FindInterface failed with {0:X}", hr.GetError());
				}
				IAMStreamConfig conf = obj as IAMStreamConfig;
				if (conf == null)
					throw new Exception("Stream config Error");
				hr = conf.GetFormat(out media);

				//For format type: FORMAT_VideoInfo, pbBuffer should be VIDEOINFOHEADER 
				VideoInfoHeader vInfo = new VideoInfoHeader();
				Marshal.PtrToStructure(media.pbFormat, vInfo);
				//Average display time of the video frames, in 100-nanosecond units
				vInfo.AvgTimePerFrame = 10000000 / m_FrameRate;
				vInfo.BmiHeader.Width = m_Width;
				vInfo.BmiHeader.Height = m_Height;
				Marshal.StructureToPtr(vInfo, media.pbFormat, false);

				hr = conf.SetFormat(media);
				if (hr.GetError() != 0)
				{
					Logger.Error("conf.setformat failed with {0:X}", hr.GetError());
				}
				FreeAMMedia(media);

				hr = capGraph.RenderStream(Guids.PinCategoryCapture, Guids.MediaTypeVideo, capFilter, null, baseFilter);
				if (hr.GetError() != 0)
				{
					Logger.Error("RenderStream failed with {0:X}", hr.GetError());
				}
				//check whether config param set correctly
				media = new AMMediaType();

				hr = grabber.GetConnectedMediaType(media);
				if (media.formatType != Guids.FormatTypesVideoInfo)
					throw new ColorFormatNotSupported("Not able to connect to Video Media");
				if (media.pbFormat == IntPtr.Zero)
					throw new Exception("Format Array is null");

				vInfo = (VideoInfoHeader)Marshal.PtrToStructure(media.pbFormat, typeof(VideoInfoHeader));
				m_Width = vInfo.BmiHeader.Width;
				m_Height = vInfo.BmiHeader.Height;
				m_Stride = m_Width * (vInfo.BmiHeader.BitCount / 8);
				if (m_Buffer == IntPtr.Zero)
					m_Buffer = Marshal.AllocCoTaskMem(m_Stride * m_Height);
				FreeAMMedia(media);

			}
			catch
			{
				//Logger.Error("Failed to build Graph. Exception: {0}", e.ToString());
				throw;
			}
			finally
			{
				if (capFilter != null)
				{
					Marshal.ReleaseComObject(capFilter);
					capFilter = null;
				}
				if (grabber != null)
				{
					Marshal.ReleaseComObject(grabber);
					grabber = null;
				}
				if (capGraph != null)
				{
					Marshal.ReleaseComObject(capGraph);
					capGraph = null;
				}
			}
		}

		public void Dispose()
		{
			TearDownCom();
			if (m_Evt != null)
			{
				m_Evt.Close();
				m_Evt = null;
			}

		}

		private void FreeAMMedia(AMMediaType m)
		{
			if (m != null)
			{
				if (m.cbFormat != 0)
				{
					Marshal.FreeCoTaskMem(m.pbFormat);
					m.cbFormat = 0;
					m.pbFormat = IntPtr.Zero;
				}
				if (m.pUnk != IntPtr.Zero)
				{
					Marshal.Release(m.pUnk);
					m.pUnk = IntPtr.Zero;
				}
			}
			m = null;
		}

		int ISampleGrabberCB.SampleCB(double time, IMediaSample pSample)
		{
			Marshal.ReleaseComObject(pSample);
			return 0;
		}

		int ISampleGrabberCB.BufferCB(double time, IntPtr pBuffer, int len)
		{
			if (m_bGrabFrame)
			{
				if (len <= m_Stride * m_Height)
				{
					CopyMemory(m_Buffer, pBuffer, len);
				}
				m_bGrabFrame = false;
				m_Evt.Set();
			}
			else
			{
				m_DroppedFrame++;
			}
			return 0;
		}

		public void Run()
		{
			if (m_bGraphRunning || m_mediaCtrl == null)
				return;
			ErrorHandler hr = m_mediaCtrl.Run();
			m_bGraphRunning = true;
		}
		/*
		public void Stop()
		{
			if (!m_bGraphRunning)
				return;
			ErrorHandler hr = m_mediaCtrl.Stop();
			m_bGraphRunning = false;
			sw.Close();
		}
		*/
		public void Pause()
		{
			if (!m_bGraphRunning)
				return;
			ErrorHandler hr = m_mediaCtrl.Pause();
			m_bGraphRunning = false;
		}

		public IntPtr getSignleFrame()
		{
			try
			{
				m_Evt.Reset();
				m_bGrabFrame = true;
				Run();
				if (!m_Evt.WaitOne(5000, false))
				{
					Logger.Info("GetSingleFrame Timed out");
				}
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				Marshal.FreeCoTaskMem(m_Buffer);
				m_Buffer = IntPtr.Zero;
			}
			return m_Buffer;
		}

		public int Width
		{
			get
			{
				return m_Width;
			}
		}

		public int Height
		{
			get
			{
				return m_Height;
			}
		}

		public int Stride
		{
			get
			{
				return m_Stride;
			}
		}
		private void TearDownCom()
		{
			ErrorHandler hr;

			try
			{
				if (m_mediaCtrl != null && m_bGraphRunning == true)
				{
					hr = m_mediaCtrl.Stop();
					m_bGraphRunning = false;
				}
				if (m_mediaCtrl != null)
				{
					Marshal.ReleaseComObject(m_mediaCtrl);
					m_mediaCtrl = null;
				}
			}
			catch (Exception e)
			{
				Logger.Error("Failed to Stop Graph, Exception: {0}", e.ToString());
			}

			if (m_FilterGraph != null)
			{
				Marshal.ReleaseComObject(m_FilterGraph);
				m_FilterGraph = null;
			}
		}
	}

	public class Camera : Object
	{
		public IntPtr pFrame = IntPtr.Zero;
		protected Thread previewThread = null;
		private volatile bool m_bStop;
		private CaptureGraph VidCapture = null;
		public delegate void getFrameCB(IntPtr ip, int width, int height, int stride);
		private static getFrameCB s_sendFrame = null;
		private int m_Unit;
		private int m_Width;
		private int m_Height;
		private int m_Framerate;
		private int m_Quality;
		private SupportedColorFormat m_color;

		public bool registerFrameCB(getFrameCB cb)
		{
			if (cb == null)
				return false;
			s_sendFrame = new getFrameCB(cb);
			return true;
		}

		public Camera(int unit, int width, int height, int framerate, int quality, SupportedColorFormat color)
		{
			m_bStop = true;
			m_Unit = unit;
			m_Width = width;
			m_Height = height;
			m_Framerate = framerate;
			m_Quality = quality;
			m_color = color;
			VidCapture = new CaptureGraph(m_Unit, m_Width, m_Height, m_Framerate, m_color);
		}

		public void StartCamera()
		{
			ThreadStart starter = new ThreadStart(Run);
			previewThread = new Thread(new ThreadStart(Run));
			previewThread.Start();
		}

		public void StopCamera()
		{
			if (previewThread != null)
			{
				m_bStop = true;
				previewThread.Join();
				if (VidCapture != null)
				{
					VidCapture.Dispose();
					VidCapture = null;
				}
				if (s_sendFrame != null)
				{
					s_sendFrame = null;
				}

			}
			previewThread = null;
		}

		protected void Run()
		{
			m_bStop = false;
			try
			{
				VidCapture.Run();
				do
				{
					while (true)
					{
						try
						{
							pFrame = IntPtr.Zero;

							pFrame = VidCapture.getSignleFrame();
							if (s_sendFrame != null && pFrame != IntPtr.Zero)
							{
								s_sendFrame(pFrame, VidCapture.Width, VidCapture.Height, VidCapture.Stride);
							}
							if (m_bStop)
								break;

						}
						catch (Exception e)
						{
							Logger.Error("Failed in send frame callback. Exception: {0}", e.ToString());
							throw;
						}
					}
					VidCapture.Pause();
				} while (!m_bStop);
			}
			catch (Exception e)
			{
				Logger.Error("Failed in Graph Run. Exception {0}", e.ToString());
			}
			finally
			{
				if (VidCapture != null)
				{
					VidCapture.Dispose();
					VidCapture = null;
				}
			}
		}
	}

}
