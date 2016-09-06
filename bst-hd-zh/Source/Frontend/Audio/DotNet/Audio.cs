using System;
using System.Windows.Forms;
using System.IO;
using NAudio.Wave;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;
using BlueStacks.hyperDroid.Common;
using BlueStacks.hyperDroid.Frontend.Interop;

namespace BlueStacks.hyperDroid.Audio
{
	public class Manager
	{

		/* Playback delegates */
		private unsafe delegate void PlaybackStreamCallback(IntPtr buff,
				Int32 size);
		private delegate void PlaybackConfigCallback(int rate, int bits,
				int channel);

		private static PlaybackStreamCallback s_PlaybackStreamCallback;
		private static PlaybackConfigCallback s_PlaybackConfigCallback;

		/* Capture delegates */
		private delegate void CaptureConfigCallback(int rate, int bits,
				int channel);
		private delegate void CaptureStartCallback();
		private delegate void CaptureStopCallback();

		private static CaptureConfigCallback s_CaptureConfigCallback;
		private static CaptureStartCallback s_CaptureStartCallback;
		private static CaptureStopCallback s_CaptureStopCallback;


		private const String NATIVE_DLL = "HD-Audio-Native.dll";

		[DllImport("kernel32.dll")]
		private static extern bool CloseHandle(IntPtr handle);

		[DllImport(NATIVE_DLL)]
		private static extern void SetPlaybackStreamCallback(
					PlaybackStreamCallback func);

		[DllImport(NATIVE_DLL)]
		private static extern void SetPlaybackConfigCallback(
					PlaybackConfigCallback func);

		[DllImport(NATIVE_DLL)]
		private static extern void SetCaptureConfigCallback(
					CaptureConfigCallback func);

		[DllImport(NATIVE_DLL)]
		private static extern void SetCaptureStartCallback(
					CaptureStartCallback func);

		[DllImport(NATIVE_DLL)]
		private static extern void SetCaptureStopCallback(
					CaptureStopCallback func);

		[DllImport(NATIVE_DLL, SetLastError = true)]
		private static extern IntPtr AudioIoAttach(uint vmId);

		[DllImport(NATIVE_DLL)]
		private static extern int AudioIoProcessMessages(IntPtr ioHandle);

		static Monitor s_Monitor;

		/* Capture */
		static WaveIn s_WaveIn;
		static int s_CaptureRate = 44100;
		static int s_CaptureBitsPerSample = 16;
		static int s_CaptureNrChannels = 1;
		static bool s_IsRecording = false;
		static bool s_CaptureEnabled = true;

		/* Playback */
		static IWavePlayer s_WaveOut;
		static WaveFormat s_WaveOutFormat;
		static BufferedWaveProvider s_WaveProvider;
		static int s_PlaybackRate = 44100;
		static int s_PlaybackBitsPerSample = 16;
		static int s_PlaybackNrChannels = 2;

		static IntPtr s_IoHandle = IntPtr.Zero;
		static Object s_IoHandleLock = new Object();

		static bool s_AudioMuted = false;

		/* Max size of msg that can be read in one read call. */
		private const int BST_CO_MSG_MAX_SIZE = 0x10000;

		/* buffer for manipulating play back stream */
		static byte[] s_Samples = new byte[BST_CO_MSG_MAX_SIZE];

		private static void BstPlaybackConfigCallback(
				int rate,
				int bits,
				int channels)
		{
			if (s_PlaybackRate == rate && s_PlaybackBitsPerSample == bits &&
					s_PlaybackNrChannels == channels)
				return;

			/* Dispose current interface and create a new one
			 * with new sample rate and num chanels.*/
			s_PlaybackRate = rate;
			s_PlaybackBitsPerSample = bits;
			s_PlaybackNrChannels = channels;

			PlaybackDeviceFini();
			PlaybackDeviceInit(rate, bits, channels);

			return;
		}

		private unsafe static void BstPlaybackStreamCallback(
				IntPtr buff,
				Int32 size)
		{
			if (s_AudioMuted)
				return;

			Marshal.Copy(buff, s_Samples, 0, size);
			s_WaveProvider.AddSamples(s_Samples, 0, size);
		}

		private static int PlaybackDeviceInit(
				int rate,
				int bits,
				int channels)
		{
			s_WaveOut = new DirectSoundOut(46 * 2);
			//s_WaveOut		= new WaveOut(WaveCallbackInfo.FunctionCallback());

			s_WaveOutFormat = new WaveFormat(rate, bits, channels);
			s_WaveProvider = new BufferedWaveProvider(s_WaveOutFormat);

			s_WaveProvider.DiscardOnBufferOverflow = true;
			s_WaveProvider.BufferLength = 5 * 1024 * 1024;

			s_WaveOut.Init(s_WaveProvider);
			s_WaveOut.Play();

			return 0;
		}

		private static void PlaybackDeviceFini()
		{
			/* stop the current play back if any and dispose the handle. */
			s_WaveOut.Stop();
			s_WaveOut.Dispose();
		}

		private static void BstCaptureConfigCallback(
				int rate,
				int bits,
				int channels)
		{
			if (s_CaptureRate == rate && s_CaptureNrChannels == channels &&
					bits == s_CaptureBitsPerSample)
				return;

			s_CaptureRate = rate;
			s_CaptureBitsPerSample = bits;
			s_CaptureNrChannels = channels;

			s_WaveIn.WaveFormat = new WaveFormat(rate, bits, channels);
			//CaptureDeviceFini();
			//CaptureDeviceInit(rate, channels);

			return;
		}

		private static void BstSendCaptureSamples(
				object sender,
				WaveInEventArgs samples)
		{
			if (s_IsRecording)
			{
				Monitor.SendAudioCaptureStream(samples.Buffer,
						samples.BytesRecorded);
			}
		}

		private static int CaptureDeviceInit(
				int rate,
				int bits,
				int channels)
		{
			//s_WaveIn		= new WaveIn();
			s_WaveIn = new WaveIn(WaveCallbackInfo.FunctionCallback());
			s_WaveIn.WaveFormat = new WaveFormat(rate, bits, channels);

			s_WaveIn.BufferMilliseconds = 75;
			s_WaveIn.DataAvailable += new EventHandler<WaveInEventArgs>
				(BstSendCaptureSamples);

			return 0;
		}

		private static void CaptureDeviceFini()
		{
			if (s_IsRecording)
			{
				s_IsRecording = false;
				s_WaveIn.StopRecording();
			}

			if (s_WaveIn != null)
				s_WaveIn.Dispose();
		}

		private static void BstCaptureStartCallback()
		{
			if (!s_CaptureEnabled || s_AudioMuted)
				return;

			try
			{
				s_WaveIn.StartRecording();
				s_IsRecording = true;
			}
			catch (Exception e)
			{
				Logger.Error("Audio: Excetpion during recording: {0}.\n",
						e.Message);
				s_CaptureEnabled = false;
			}
		}

		private static void BstCaptureStopCallback()
		{
			if (!s_IsRecording)
				return;

			s_IsRecording = false;
			s_WaveIn.StopRecording();
		}

		public static void Mute()
		{
			Logger.Debug("Audio: volume muted");
			s_AudioMuted = true;
		}

		public static void Unmute()
		{
			Logger.Debug("Audio: volume unmuted");
			s_AudioMuted = false;
		}

		public static Monitor Monitor
		{
			get
			{
				return s_Monitor;
			}

			set
			{
				s_Monitor = value;
			}
		}

		public static void Main(String[] args)
		{
			if (args.Length != 1)
				Usage();

			String vmName = args[0];

			/*
			 * Look up the VM identifier using the VM name.
			 */

			uint vmId = MonitorLocator.Lookup(vmName);

			/*
			 * Attach to the VM.
			 */

			lock (s_IoHandleLock)
			{
				if (s_IoHandle != IntPtr.Zero)
					throw new SystemException("I/O handle is already open");

				Logger.Debug("Attaching to monitor ID {0}", vmId);
				s_IoHandle = AudioIoAttach(vmId);
				if (s_IoHandle == IntPtr.Zero)
					throw new SystemException("Cannot attach for I/O",
							new Win32Exception(Marshal.GetLastWin32Error()));
			}

			/*
			 * Initialize audio device for playback.
			 */
			int err = PlaybackDeviceInit(s_PlaybackRate,
					s_PlaybackBitsPerSample, s_PlaybackNrChannels);
			if (err != 0)
			{
				throw new SystemException("Failed to init playback device.",
						new Win32Exception(err));
			}

			/*
			 * Initialize audio device for capture.
			 */
			err = CaptureDeviceInit(s_CaptureRate, s_CaptureBitsPerSample,
					s_CaptureNrChannels);
			if (err != 0)
			{
				throw new SystemException("Failed to init capture device.",
						new Win32Exception(err));
			}

			s_PlaybackStreamCallback = new PlaybackStreamCallback(
					BstPlaybackStreamCallback);
			s_PlaybackConfigCallback = new PlaybackConfigCallback(
					BstPlaybackConfigCallback);

			s_CaptureConfigCallback = new CaptureConfigCallback(
					BstCaptureConfigCallback);
			s_CaptureStartCallback = new CaptureStartCallback(
					BstCaptureStartCallback);
			s_CaptureStopCallback = new CaptureStopCallback(
					BstCaptureStopCallback);

			/* set required callbacks */
			SetPlaybackStreamCallback(s_PlaybackStreamCallback);
			SetPlaybackConfigCallback(s_PlaybackConfigCallback);
			SetCaptureConfigCallback(s_CaptureConfigCallback);
			SetCaptureStartCallback(s_CaptureStartCallback);
			SetCaptureStopCallback(s_CaptureStopCallback);

			/*
			 * Loop, processing messages on each iteration.  Note
			 * that IoProcessMessages() will block until it receives
			 * a message from the monitor.
			 */

			Logger.Debug("Waiting for Audio messages...");
			System.Threading.Thread audioThread = new
				System.Threading.Thread(delegate ()
				{
					while (true)
					{
						try
						{
							int error = AudioIoProcessMessages(s_IoHandle);
							if (error != 0)
								throw new SystemException("Cannot process VM messages",
									new Win32Exception(error));
						}
						catch (Exception e)
						{
							Logger.Error(e.ToString());
							Logger.Error("Audio: Exiting thread.");
							return;
						}
					}
				});

			audioThread.IsBackground = true;
			audioThread.Start();
			Application.Run();
		}

		private static void Usage()
		{
			String prog = Process.GetCurrentProcess().ProcessName;

			//Console.Error.WriteLine("Usage: {0} <vm name>", prog);
			Environment.Exit(1);
		}

		public static void Shutdown()
		{
			PlaybackDeviceFini();
			CaptureDeviceFini();

			System.Threading.Thread.Sleep(500);

			lock (s_IoHandleLock)
			{
				if (s_IoHandle != IntPtr.Zero)
				{
					CloseHandle(s_IoHandle);
					s_IoHandle = IntPtr.Zero;
				}
			}
		}

	}
}
