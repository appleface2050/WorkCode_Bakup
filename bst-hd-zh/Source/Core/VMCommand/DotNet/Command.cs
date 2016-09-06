using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Text;
using System.Runtime.InteropServices;

using Microsoft.Win32.SafeHandles;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Core.VMCommand
{

	public class Command
	{

		private const String NATIVE_DLL = "HD-VMCommand-Native.dll";

		public delegate void ChunkHandler(String chunk);

		[DllImport(NATIVE_DLL, SetLastError = true)]
		private static extern SafeFileHandle CommandAttach(uint vmId,
			uint unitId);

		[DllImport(NATIVE_DLL)]
		private static extern int CommandPing(SafeFileHandle vmHandle,
			uint unitId);

		[DllImport(NATIVE_DLL)]
		private static extern int CommandRun(SafeFileHandle vmHandle,
			uint unitId, int argc, String[] argv, ChunkHandler outHandler,
			ChunkHandler errHandler, ref int exitCode);

		[DllImport(NATIVE_DLL)]
		private static extern int CommandKill(SafeFileHandle vmHandle,
			uint unitId);

		public delegate void LineHandler(String line);

		private Random random = new Random();

		private SafeFileHandle vmHandle;
		private uint unitId = 0;

		private LineHandler userOutputHandler = null;
		private LineHandler userErrorHandler = null;

		private StringBuilder outputBuffer = new StringBuilder();
		private StringBuilder errorBuffer = new StringBuilder();

		public Command()
		{
		}

		public void Attach(String vmName)
		{
			/*
			 * Look up the VM identifier using the VM name.
			 */

			uint vmId = MonitorLocator.Lookup(vmName);

			/*
			 * Generate a random unit ID.  There is no chance well
			 * get HD_UNIT_ALL, as this method only returns non-
			 * negative integers.
			 */

			this.unitId = (uint)this.random.Next();

			/*
			 * Attach to the VM.
			 */

			this.vmHandle = CommandAttach(vmId, this.unitId);
			if (this.vmHandle.IsInvalid)
			{
				throw new ApplicationException(
					"Cannot attach to monitor: " +
					Marshal.GetLastWin32Error());
			}
		}

		public void SetOutputHandler(LineHandler handler)
		{
			this.userOutputHandler = handler;
		}

		public void SetErrorHandler(LineHandler handler)
		{
			this.userErrorHandler = handler;
		}

		public int Run(String[] argv)
		{
			int exitCode = 0;
			int error;

			error = CommandPing(this.vmHandle, this.unitId);
			if (error != 0)
			{
				throw new ApplicationException(
					"Cannot ping VM", new Win32Exception(error));
			}

			error = CommandRun(this.vmHandle, this.unitId, argv.Length,
				argv, OutputHandler, ErrorHandler, ref exitCode);
			if (error != 0)
			{
				throw new ApplicationException(
					"Cannot run VM command",
					new Win32Exception(error));
			}

			/*
			 * Our string builders may contain a partial line.  Call
			 * the line handler one more time if this is the case.
			 */

			if (this.outputBuffer.Length > 0 &&
				this.userOutputHandler != null)
				this.userOutputHandler(this.outputBuffer.ToString());

			if (this.errorBuffer.Length > 0 &&
				this.userErrorHandler != null)
				this.userErrorHandler(this.errorBuffer.ToString());

			return exitCode;
		}

		private void OutputHandler(String chunk)
		{
			CommonHandler(chunk, this.outputBuffer,
				this.userOutputHandler);
		}

		private void ErrorHandler(String chunk)
		{
			CommonHandler(chunk, this.errorBuffer,
				this.userErrorHandler);
		}

		private static void CommonHandler(String chunk, StringBuilder sb,
			LineHandler handler)
		{
			/*
			 * Append this chunk to the string builder.
			 */

			sb.Append(chunk);

			/*
			 * Call the user handler for all of the complete lines.
			 */

			String buf = sb.ToString();
			String[] lines = buf.Split(new Char[] { '\n' });

			if (lines.Length < 2)
				return;

			for (int ndx = 0; ndx < lines.Length - 1; ndx++)
				if (handler != null)
					handler(lines[ndx]);

			/*
			 * Clear the string builder and initialize it with the
			 * remaining partial line.
			 */

			sb.Remove(0, sb.Length);
			sb.Append(lines[lines.Length - 1]);
		}

		public void Kill()
		{
			CommandKill(this.vmHandle, this.unitId);
		}
	};

}
