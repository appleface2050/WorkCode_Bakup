using System;
using System.Windows.Forms;
using System.Threading;
using System.IO;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;
using Microsoft.Win32;

namespace BlueStacks.hyperDroid.Gps {
public class Manager {

	private const String NATIVE_DLL = "HD-Gps-Native.dll";

	static	IntPtr			s_IoHandle	= IntPtr.Zero;
	static	Object			s_IoHandleLock	= new Object();
	[StructLayout(LayoutKind.Sequential, Pack = 1)]
	public struct GpsLocation
	{
		public double	latitude;
		public double	longitude;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=16)]
		public string 	country;
		[MarshalAs(UnmanagedType.ByValTStr, SizeConst=64)]
		public string	city;
	}
	public static GpsLocation location;
	
	private static Manager 	sInstance =		new Manager();
	private static Frontend.Interop.Monitor		mMonitor;

	[DllImport("kernel32.dll")]
	private static extern bool CloseHandle(IntPtr handle);

	[DllImport(NATIVE_DLL, SetLastError=true)]
	private static extern IntPtr GpsIoAttach(uint vmId);

	[DllImport(NATIVE_DLL, SetLastError=true)]
	private static extern int GpsIoProcessMessages(IntPtr ioHandle);

	public void SetMonitor(Frontend.Interop.Monitor monitor)
	{
		mMonitor = monitor;
	}

	public static Manager Instance()
	{
		return sInstance;
	}

	public static void Main(String[] args)
	{
	    if (args.Length != 1)
	    {
		Logger.Error("Gps: Invalid invocation. Argument missing.");
		return;
	    }
	    String vmName = args[0];
	    String path = String.Format(Common.Strings.RegBasePath + @"\Guests\{0}\Config", vmName);

	    Logger.Debug("Waiting for Gps messages...");
	   
	    /*
	     * Periodically (P = 1 min) send GPS coordinate info
	     * to the guest
	     */
	    System.Threading.Thread GpsThread = new System.Threading.Thread(delegate () {
		    while (true)
		    {
		    	try
			{
				RegistryKey key = Registry.LocalMachine.OpenSubKey(path);
				location.latitude = Convert.ToDouble(key.GetValue("GpsLatitude"));
				location.longitude = Convert.ToDouble(key.GetValue("GpsLongitude"));
				key.Close();
			}
			catch (Exception e)
			{
				Logger.Error(e.ToString());
				Logger.Error("GPS: Exiting thread.");
				return;
			}
			Logger.Debug("Sending GPS location...");
			mMonitor.SendLocation(location);

			System.Threading.Thread.Sleep(60000);
		    }
    		});

	    GpsThread.IsBackground = true;
	    GpsThread.Start();
	}

	/*
	 * VT: This API is practically not called today.
	 */
	public static void Shutdown ()
	{
	    lock (s_IoHandleLock)
	    {
		if (s_IoHandle != IntPtr.Zero)
		{
		    Logger.Debug("Shutting down gps...\n");
		    CloseHandle(s_IoHandle);
		    s_IoHandle = IntPtr.Zero;
		}
	    }
	}
}
}
