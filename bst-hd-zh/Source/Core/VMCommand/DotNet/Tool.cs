using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.InteropServices;

using BlueStacks.hyperDroid.Common;

namespace BlueStacks.hyperDroid.Core.VMCommand
{

	public class Tool
	{

		public static void Main(String[] args)
		{
			if (args.Length < 2)
				Usage();

			String vmName = args[0];

			String[] new_argv = new String[args.Length - 1];
			Array.Copy(args, 1, new_argv, 0, args.Length - 1);

			Command cmd = new Command();
			cmd.Attach(vmName);

			cmd.SetOutputHandler(delegate (String line)
			{
				Console.WriteLine(line);
			});

			cmd.SetErrorHandler(delegate (String line)
			{
				Console.Error.WriteLine(line);
			});

			ConsoleControl.SetHandler(
				delegate (ConsoleControl.CtrlType ctrl)
				{
					cmd.Kill();
					return true;
				});

			try
			{
				int res = cmd.Run(new_argv);
				Environment.Exit(res);

			}
			catch (Exception exc)
			{

				String msg = exc.Message;

				if (exc.InnerException != null)
					msg += " --> " + exc.InnerException.Message;

				Console.WriteLine("Cannot run VM command: " + msg);
				Environment.Exit(1);
			}
		}

		private static void Usage()
		{
			String prog = Process.GetCurrentProcess().ProcessName;

			Console.Error.WriteLine(
				"Usage: {0} <vm name> <cmd> [args ...]", prog);

			Environment.Exit(1);
		}
	}

}
