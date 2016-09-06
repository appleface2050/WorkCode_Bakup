using System;
using System.IO;
using System.Text;
using System.Threading;
using System.ComponentModel;
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace BlueStacks.hyperDroid.Common.Pipe
{
	class ConnectedClient
	{
		public IntPtr handle;
	}

	class Common
	{
		public const int MSG_HEADER_SZ = 4;

		public static void WriteMsg(IntPtr handle, String msg)
		{
			uint bytesWritten;
			byte[] buffer = UTF8Encoding.UTF8.GetBytes(msg);

			/* Writing header: msg size */
			if (!Interop.WriteFile(handle,
					BitConverter.GetBytes(buffer.Length),
					Common.MSG_HEADER_SZ,
					out bytesWritten,
					0))
			{
				throw new SystemException("WriteFile failed",
						new Win32Exception(Marshal.GetLastWin32Error()));
			}

			/* Writing msg */
			if (!Interop.WriteFile(handle,
					buffer,
					(uint)buffer.Length,
					out bytesWritten,
					0))
			{
				throw new SystemException("WriteFile failed",
						new Win32Exception(Marshal.GetLastWin32Error()));
			}

			Interop.FlushFileBuffers(handle);
		}

		public static String ReadMsg(IntPtr handle)
		{
			byte[] msg = null;
			uint msgLen;
			byte[] header = new byte[Common.MSG_HEADER_SZ];
			uint bytesRead;
			bool ret;

			/* Reading header: msg size */
			ret = Interop.ReadFile(handle,
					header,
					Common.MSG_HEADER_SZ,
					out bytesRead,
					0);

			if (ret == false || bytesRead == 0)
			{
				Logger.Error("Marshal.GetLastWin32Error = {0}", Marshal.GetLastWin32Error());
				if ((ulong)Marshal.GetLastWin32Error() == Interop.ERROR_BROKEN_PIPE)
					Logger.Error("Peer disconnected");
				throw new SystemException("ReadFile failed",
						new Win32Exception(Marshal.GetLastWin32Error()));
			}

			msgLen = BitConverter.ToUInt32(header, 0);
			msg = new byte[msgLen];

			/* Reading msg */
			ret = Interop.ReadFile(handle,
					msg,
					msgLen,
					out bytesRead,
					0);

			if (ret == false || bytesRead == 0)
			{
				Logger.Error("Marshal.GetLastWin32Error = {0}", Marshal.GetLastWin32Error());
				if ((ulong)Marshal.GetLastWin32Error() == Interop.ERROR_BROKEN_PIPE)
					Logger.Error("Peer disconnected");
				throw new SystemException("ReadFile failed",
						new Win32Exception(Marshal.GetLastWin32Error()));
			}

			String response = UTF8Encoding.UTF8.GetString(msg,
								0,
								(int)bytesRead);
			return response;
		}
	}

	class Interop
	{
		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateNamedPipe(
				String lpName,
				uint dwOpenMode,
				uint dwPipeMode,
				uint nMaxInstances,
				uint nOutBufferSize,
				uint nInBufferSize,
				uint nDefaultTimeOut,
				IntPtr lpSecurityAttributes);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ConnectNamedPipe(
				IntPtr hNamedPipe,
				IntPtr lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool SetNamedPipeHandleState(
				IntPtr hNamedPipe,
				ref uint lpMode,
				IntPtr lpMaxCollectionCount,
				IntPtr lpCollectDataTimeout);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WaitNamedPipe(
				String lpNamedPipeName,
				int nTimeOut);


		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern IntPtr CreateFile(
				String lpFileName,
				uint dwDesiredAccess,
				uint dwShareMode,
				IntPtr lpSecurityAttributes,
				uint dwCreationDisposition,
				uint dwFlagsAndAttributes,
				IntPtr hTemplateFile);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool ReadFile(
				IntPtr hHandle,
				byte[] lpBuffer,
				uint nNumberOfBytesToRead,
				out uint lpNumberOfBytesRead,
				uint lpOverlapped);


		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool WriteFile(
				IntPtr hHandle,
				byte[] lpBuffer,
				uint nNumberOfBytesToWrite,
				out uint lpNumberOfBytesWritten,
				uint lpOverlapped);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool FlushFileBuffers(IntPtr hHandle);

		[DllImport("kernel32.dll", SetLastError = true)]
		public static extern bool CloseHandle(IntPtr hHandle);

		public const int INVALID_HANDLE_VALUE = -1;

		public const uint PIPE_ACCESS_DUPLEX = 0x00000003;
		public const uint PIPE_UNLIMITED_INSTANCES = 255;
		public const uint PIPE_TYPE_MESSAGE = 0x00000004;
		public const uint PIPE_READMODE_MESSAGE = 0x00000002;
		public const uint PIPE_WAIT = 0x00000000;

		public const uint GENERIC_READ = 0x80000000;
		public const uint GENERIC_WRITE = 0x40000000;
		public const uint OPEN_EXISTING = 3;
		public const ulong ERROR_PIPE_CONNECTED = 535L;
		public const ulong ERROR_BROKEN_PIPE = 109L;
	}

	class Server
	{
		public delegate void RequestHandler(ConnectedClient client, String request);
		public event RequestHandler RequestReceived;

		public const int BUFFER_SIZE = 512;

		public Server()
		{
			m_ConnectedClients = new List<ConnectedClient>();
		}

		public string PipeName
		{
			get { return m_PipeName; }
			set { m_PipeName = value; }
		}

		public bool Running
		{
			get { return m_Running; }
		}

		public void Start()
		{
			m_ListenerThread = new Thread(Listener);
			m_ListenerThread.IsBackground = true;
			m_ListenerThread.Start();
			m_Running = true;
		}

		private void Listener()
		{
			try
			{
				while (true)
				{
					ConnectedClient client = new ConnectedClient();
					client.handle = Interop.CreateNamedPipe(
							m_PipeName,
							Interop.PIPE_ACCESS_DUPLEX,
							Interop.PIPE_TYPE_MESSAGE
							| Interop.PIPE_READMODE_MESSAGE
							| Interop.PIPE_WAIT,
							Interop.PIPE_UNLIMITED_INSTANCES,
							BUFFER_SIZE,
							BUFFER_SIZE,
							0,
							IntPtr.Zero);

					if (client.handle.ToInt32() == Interop.INVALID_HANDLE_VALUE)
						throw new SystemException("CreateNamedPipe failed",
								new Win32Exception(Marshal.GetLastWin32Error()));

					if ((Interop.ConnectNamedPipe(client.handle, IntPtr.Zero) == false) &&
							(ulong)Marshal.GetLastWin32Error() != Interop.ERROR_PIPE_CONNECTED)
					{
						Interop.CloseHandle(client.handle);
						throw new SystemException("ConnectNamedPipe failed",
								new Win32Exception(Marshal.GetLastWin32Error()));
					}

					lock (m_ConnectedClients)
						m_ConnectedClients.Add(client);

					Thread reader = new Thread(Reader);
					reader.IsBackground = true;
					reader.SetApartmentState(ApartmentState.STA);
					reader.Start(client);
				}
			}
			catch (Exception exc)
			{
				Logger.Error(exc.ToString());
			}
		}

		private void Reader(Object o)
		{
			ConnectedClient client = (ConnectedClient)o;
			String msg;

			Logger.Info("Reader started");
			try
			{
				while (true)
				{
					msg = Common.ReadMsg(client.handle);
					if (this.RequestReceived != null)
						this.RequestReceived(client, msg);
				}
			}
			catch (Exception exc)
			{
				Logger.Error(exc.ToString());
			}

			Interop.CloseHandle(client.handle);
			lock (m_ConnectedClients)
				m_ConnectedClients.Remove(client);
		}

		public void Send(String response)
		{
			lock (m_ConnectedClients)
				foreach (ConnectedClient client in m_ConnectedClients)
					Send(client.handle, response);
		}

		public void Send(ConnectedClient client, String response)
		{
			Common.WriteMsg(client.handle, response);
		}

		private void Send(IntPtr handle, String response)
		{
			Common.WriteMsg(handle, response);
		}

		private string m_PipeName;
		private Thread m_ListenerThread;
		private bool m_Running;
		private List<ConnectedClient> m_ConnectedClients;
	}

	class Client
	{
		public void Connect()
		{
			m_Handle = Interop.CreateFile(
					m_PipeName,
					Interop.GENERIC_READ | Interop.GENERIC_WRITE,
					0,
					IntPtr.Zero,
					Interop.OPEN_EXISTING,
					0,
					IntPtr.Zero);

			if (m_Handle.ToInt32() == Interop.INVALID_HANDLE_VALUE)
				throw new SystemException("CreateFile failed",
						new Win32Exception(Marshal.GetLastWin32Error()));

			uint mode = Interop.PIPE_READMODE_MESSAGE | Interop.PIPE_WAIT;
			if (Interop.SetNamedPipeHandleState(m_Handle,
						ref mode,
						IntPtr.Zero,
						IntPtr.Zero) == false)
				throw new SystemException("SetNamedPipeHandleState failed",
						new Win32Exception(Marshal.GetLastWin32Error()));

			m_Connected = true;
		}

		public static bool AccessDenied(SystemException exc)
		{
			int ERROR_ACCESS_DENIED = 0x5;
			Exception inner = exc.InnerException;

			if (inner != null && inner.GetType() == typeof(Win32Exception))
			{
				Win32Exception win32 = (Win32Exception)inner;
				if (win32.NativeErrorCode == ERROR_ACCESS_DENIED)
				{
					return true;
				}
			}

			return false;
		}

		public bool Connected
		{
			get { return m_Connected; }
		}

		public string PipeName
		{
			get { return m_PipeName; }
			set { m_PipeName = value; }
		}


		public void Close()
		{
			m_Connected = false;
			Interop.CloseHandle(m_Handle);
		}

		public void Send(string request)
		{
			if (m_Connected == false)
			{
				Logger.Info("Not connected");
				return;
			}
			Common.WriteMsg(m_Handle, request);
		}

		public String Recv()
		{
			String msg = null;
			if (m_Connected == false)
			{
				Logger.Info("Not connected");
				return msg;
			}
			msg = Common.ReadMsg(m_Handle);
			return msg;
		}

		private string m_PipeName;
		private IntPtr m_Handle;
		private bool m_Connected;
	}
}
