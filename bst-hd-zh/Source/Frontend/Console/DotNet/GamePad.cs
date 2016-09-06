using System;
using System.Runtime.InteropServices;
using System.Windows.Forms;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Frontend
{

	public class GamePad
	{

		private delegate void LoggerCallback(String msg);

		private delegate void AttachCallback(int identity, int vendor,
			int product);
		private delegate void DetachCallback(int identity);
		private delegate void UpdateCallback(int identity,
			ref Common.GamePad gamepad);

		[DllImport("kernel32.dll")]
		public static extern IntPtr LoadLibrary(string dllToLoad);

		[DllImport("kernel32.dll")]
		public static extern IntPtr GetProcAddress(IntPtr hModule, string procedureName);

		[UnmanagedFunctionPointer(CallingConvention.Cdecl)]
		private delegate void GamePadSetup(LoggerCallback logger,
			AttachCallback attach, DetachCallback detach,
			UpdateCallback update, IntPtr windowHandle);

		private InputMapper mInputMapper;

		private LoggerCallback mLoggerCallback;     /* prevent GC */
		private AttachCallback mAttachCallback;     /* prevent GC */
		private DetachCallback mDetachCallback;     /* prevent GC */
		private UpdateCallback mUpdateCallback;     /* prevent GC */

		public void Setup(InputMapper inputMapper, IntPtr windowHandle)
		{
			Logger.Info("GamePad.Setup()");

			mInputMapper = inputMapper;

			mLoggerCallback = new LoggerCallback(delegate (String msg)
			{
				Logger.Info("GamePad: " + msg);
			});

			mAttachCallback = new AttachCallback(
				delegate (int identity, int vendor, int product)
				{
					mInputMapper.DispatchGamePadAttach(identity,
					vendor, product);
				});

			mDetachCallback = new DetachCallback(delegate (int identity)
			{
				mInputMapper.DispatchGamePadDetach(identity);
			});

			mUpdateCallback = new UpdateCallback(
				delegate (int identity, ref Common.GamePad gamepad)
				{
					mInputMapper.DispatchGamePadUpdate(identity,
					gamepad);
				});


			String nativeDllName = "HD-Frontend-Native.dll";

			IntPtr pDll = LoadLibrary(nativeDllName);
			if (pDll == IntPtr.Zero)
			{
				Logger.Info("Failed to {0} dll", nativeDllName);
				return;
			}

			IntPtr GamePadSetupFnPtr = GetProcAddress(pDll, "GamePadSetup");
			if (GamePadSetupFnPtr == IntPtr.Zero)
			{
				Logger.Info("function pointer is null");
				return;
			}


			GamePadSetup GamePadSetupFn = (GamePadSetup)Marshal.GetDelegateForFunctionPointer(GamePadSetupFnPtr,
																						typeof(GamePadSetup));
			GamePadSetupFn(mLoggerCallback, mAttachCallback, mDetachCallback, mUpdateCallback, windowHandle);
		}
	}

}
