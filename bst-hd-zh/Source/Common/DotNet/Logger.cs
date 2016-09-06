/*
 * Copyright 2011 BlueStack Systems, Inc.
 * All Rights Reserved
 *
 * THIS IS UNPUBLISHED PROPRIETARY SOURCE CODE OF BLUESTACK SYSTEMS, INC.
 * The copyright notice above does not evidence any actual or intended
 * publication of such source code.
 *
 * BlueStacks hyperDroid Common Library
 */

using System;
using System.Diagnostics;
using System.IO;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using BlueStacks.hyperDroid.Common;

namespace BlueStacks
{
	namespace hyperDroid
	{
		namespace Common
		{

			public class Logger
			{

				public const String LOG_TO_CONSOLE = "-";

				public const int LOG_LEVEL_FATAL = 1;
				public const int LOG_LEVEL_ERROR = 2;
				public const int LOG_LEVEL_WARNING = 3;
				public const int LOG_LEVEL_DEBUG = 4;
				public const int LOG_LEVEL_INFO = 5;

				private const int HDLOG_PRIORITY_FATAL = 0;
				private const int HDLOG_PRIORITY_ERROR = 1;
				private const int HDLOG_PRIORITY_WARNING = 2;
				private const int HDLOG_PRIORITY_INFO = 3;
				private const int HDLOG_PRIORITY_DEBUG = 4;

				private const String DEFAULT_FILE_NAME = "BlueStacksUsers";

				/* Log levels string */
				private const String LOG_STRING_FATAL = "FATAL";
				private const String LOG_STRING_ERROR = "ERROR";
				private const String LOG_STRING_WARNING = "WARNING";
				private const String LOG_STRING_INFO = "INFO";
				private const String LOG_STRING_DEBUG = "DEBUG";

				private static String s_logLevels = null;

				public delegate void WriterDelegate(String line);

				private static WriterDelegate s_WriterDelegate = null;

				private static WriterDelegate s_ErrorNotifyCallback = null;
				private static WriterDelegate s_WarningNotifyCallback = null;

				private enum WriterType
				{
					None,
					Core,
					Delegate,
				}

				private static WriterType s_WriterType = WriterType.None;

				public delegate void HdLoggerCallback(int prio, uint tid,
					String tag, String msg);

				private static HdLoggerCallback s_HdLoggerCallback;

				private class Native
				{
					[DllImport("HD-Logger-Native.dll",
						CharSet = CharSet.Unicode)]
					public static extern void LoggerDllInit(String prog,
						String file, bool toConsole);

					[DllImport("HD-Logger-Native.dll")]
					public static extern void LoggerDllReinit();

					[DllImport("HD-Logger-Native.dll",
						CharSet = CharSet.Unicode)]
					public static extern void LoggerDllPrint(String line);
				};

				public static String GetLogDir()
				{
					Object userDataDirObj = null;
					Object dataDirObj = null;
					String dir;

					RegistryKey key = Registry.LocalMachine.OpenSubKey(
							Common.Strings.RegBasePath);

					if (key != null)
					{
						dataDirObj = (String)key.GetValue("DataDir");
						userDataDirObj = (String)key.GetValue("UserDataDir");
						key.Close();
					}

					if (dataDirObj != null)
					{
						dir = (String)dataDirObj;
					}
					else if (userDataDirObj != null)
					{
						dir = (String)userDataDirObj;
					}
					else
					{
						dir = Environment.GetFolderPath(
								Environment.SpecialFolder.LocalApplicationData);
						dir = Path.Combine(dir, "Bluestacks");
					}

					return Path.Combine(dir, "Logs");
				}

				/*
				 * Initialize the logging subsystem so that log messages
				 * are handled by a caller specified delegate.
				 *
				 * This method should only be used by code that cannot
				 * utilize the core logger, such as MSI custom actions.
				 */
				public static void InitLog(WriterDelegate writer)
				{
					s_WriterDelegate = writer;
					s_WriterType = WriterType.Delegate;

					LogLevelsInit();
				}

				public static void InitUserLog()
				{
					InitLog(null, null);
				}

				/*
				 * Initialize the logging subsystem so that log messages
				 * are written to a file via the core logger.
				 *
				 * logFileName
				 *     File name for the log.  Will choose a suitable
				 *     default if null.
				 *
				 * logRotatorTag
				 *     Tag to use while waiting for log rotation events.
				 *     If it is not necessary for the program to reopen its
				 *     logs when a log rotation event occurs, then this can
				 *     be null.
				 */
				public static void InitLog(
					String logFileName,
					String logRotatorTag
					)
				{
					s_WriterType = WriterType.Core;
					s_HdLoggerCallback = new HdLoggerCallback(HdLogger);

					String prog = Process.GetCurrentProcess().ProcessName;
					String path = null;
					bool toConsole = true;

					if (logFileName != LOG_TO_CONSOLE)
					{
						toConsole = false;

						if (logFileName == null)
							logFileName = DEFAULT_FILE_NAME;

						String logDir = GetLogDir();

						path = String.Format("{0}\\{1}.log", logDir,
							logFileName);

						if (!Directory.Exists(logDir))
							Directory.CreateDirectory(logDir);
					}

					Native.LoggerDllInit(prog, path, toConsole);
					LogLevelsInit();

					if (logRotatorTag != null)
						InitLogRotation(logRotatorTag);
				}

				public static void InitLogAtPath(
						String path,
						String logRotatorTag
						)
				{
					s_WriterType = WriterType.Core;
					s_HdLoggerCallback = new HdLoggerCallback(HdLogger);

					String prog = Process.GetCurrentProcess().ProcessName;
					bool toConsole = true;

					toConsole = false;

					String logDir = GetLogDir();

					if (!Directory.Exists((new FileInfo(path)).Directory.FullName))
						Directory.CreateDirectory((new FileInfo(path)).Directory.FullName);

					Native.LoggerDllInit(prog, path, toConsole);
					LogLevelsInit();

					if (logRotatorTag != null)
						InitLogRotation(logRotatorTag);
				}

				private static void LogLevelsInit()
				{
					/* XXXDPR:  Hard coded VM name. */
					String path = Common.Strings.HKLMAndroidConfigRegKeyPath;

					RegistryKey key;

					try
					{
						using (key = Registry.LocalMachine.OpenSubKey(path))
						{
							s_logLevels = (String)key.GetValue("DebugLogs");
						}
					}
					catch (Exception)
					{
						return;
					}

					if (s_logLevels != null)
						s_logLevels = s_logLevels.ToUpper();

				}

				private static void InitLogRotation(String tag)
				{
					String name = @"Global\BlueStacks_LogRotate_" + tag;
					bool created = false;

					Logger.Print("Using event {0} for log rotation",
						name);

					try
					{
						EventWaitHandle handle = new EventWaitHandle(false,
								EventResetMode.AutoReset, name, out created);
						if (!created)
						{
							Logger.Print("Log rotation event for " +
									tag + " already exists");
							return;
						}

						WaitOrTimerCallback callback = delegate (
								Object state, bool timedOut)
						{
							Logger.Print("Reopening log file");
							Native.LoggerDllReinit();
						};

						ThreadPool.RegisterWaitForSingleObject(handle,
								callback, null, -1, false);
					}
					catch (Exception e)
					{
						Logger.Error(e.ToString());
					}
				}

				private static bool IsLogLevelEnabled(String tag, String level)
				{
					/*
					 * Verbose logs are an opt-in experience.  Don't
					 * consider them enabled if nobody has taken the
					 * trouble to configure the DebugLogs registry
					 * value.
					 */
					if (s_logLevels == null)
						return false;

					/*
					 * If DebugLogs string starts with 'ALL', enable
					 * verbose logging for all processes.
					 */
					if (s_logLevels.StartsWith("ALL"))
						return true;

					/*
					 * Expected registry value is "ProcessName:level", or
					 * "LogTag:level".
					 *
					 * XXXDPR:  Wouldn't it make more sense to use a
					 *          MultiString value?  This configuration
					 *          knob is naturally a list of name/value
					 *          pairs.
					 */

					return s_logLevels.Contains((tag + ":" + level).ToUpper());
				}

				public static TextWriter GetWriter()
				{
					return new Writer(delegate (String msg)
					{
						Print(msg);
					});
				}

				private static void Print(int level, String tag, String fmt, params Object[] args)
				{
					int tid = Thread.CurrentThread.ManagedThreadId;
					String s = "UNKNOWN";
					String prefix;

					if (level == LOG_LEVEL_FATAL)
						s = LOG_STRING_FATAL;
					else if (level == LOG_LEVEL_ERROR)
						s = LOG_STRING_ERROR;
					else if (level == LOG_LEVEL_WARNING)
						s = LOG_STRING_WARNING;
					else if (level == LOG_LEVEL_INFO)
						s = LOG_STRING_INFO;
					else if (level == LOG_LEVEL_DEBUG)
						s = LOG_STRING_DEBUG;

					if (level == LOG_LEVEL_DEBUG && !IsLogLevelEnabled(tag, s))
						return;

					if (tag != null)
						prefix = String.Format("{0,5:D0} {1} {2} ", tid,
						tag, s);
					else
						prefix = String.Format("{0,5:D0} {1} ", tid, s);

					WriteMessageToLog(prefix + String.Format(fmt, args));
				}

				private static void WriteMessageToLog(String msg)
				{
					Char[] sep = new Char[] { '\n' };
					Char[] cr = new Char[] { '\r' };

					foreach (String rawLine in msg.Split(sep))
					{
						String line = rawLine.Trim(cr);

						if (s_WriterType == WriterType.Core)
							Native.LoggerDllPrint(line);
						else if (s_WriterType == WriterType.Delegate)
							s_WriterDelegate(line);
						else
							Console.Error.WriteLine(line);
					}
				}

				private static void Print(String fmt, params Object[] args)
				{
					Print(LOG_LEVEL_INFO, null, fmt, args);
				}

				private static void Print(String msg)
				{
					Print("{0}", msg);
				}

				public static void Fatal(String fmt, params Object[] args)
				{
					Print(LOG_LEVEL_FATAL, null, fmt, args);
				}

				public static void Fatal(String msg)
				{
					Fatal("{0}", msg);
				}

				public static void Error(String fmt, params Object[] args)
				{
					Print(LOG_LEVEL_ERROR, null, fmt, args);
				}

				public static void Error(String msg)
				{
					Error("{0}", msg);
				}

				public static void SetErrorNotifyCallback(WriterDelegate callback)
				{
					s_ErrorNotifyCallback = callback;
				}

				public static void ErrorNotify(String fmt, params Object[] args)
				{
					ErrorNotify(String.Format(fmt, args));
				}

				public static void ErrorNotify(String msg)
				{
					if (s_ErrorNotifyCallback != null)
						s_ErrorNotifyCallback(msg);

					Error(msg);
				}

				public static void Warning(String fmt, params Object[] args)
				{
					Print(LOG_LEVEL_WARNING, null, fmt, args);
				}

				public static void Warning(String msg)
				{
					Warning("{0}", msg);
				}

				public static void SetWarningNotifyCallback(WriterDelegate callback)
				{
					s_WarningNotifyCallback = callback;
				}

				public static void WarningNotify(String fmt, params Object[] args)
				{
					WarningNotify(String.Format(fmt, args));
				}

				public static void WarningNotify(String msg)
				{
					if (s_WarningNotifyCallback != null)
						s_WarningNotifyCallback(msg);

					Warning(msg);
				}

				public static void Info(String fmt, params Object[] args)
				{
					Print(LOG_LEVEL_INFO, null, fmt, args);
				}

				public static void Info(String msg)
				{
					Info("{0}", msg);
				}

				public static void Debug(String fmt, params Object[] args)
				{
					Print(LOG_LEVEL_DEBUG, null, fmt, args);
				}

				public static void Debug(String msg)
				{
					Debug("{0}", msg);
				}

				private static void HdLogger(int prio, uint tid, String tag,
						String msg)
				{
					int level = 0;

					if (prio == HDLOG_PRIORITY_FATAL)
						level = Logger.LOG_LEVEL_FATAL;

					else if (prio == HDLOG_PRIORITY_ERROR)
						level = Logger.LOG_LEVEL_ERROR;

					else if (prio == HDLOG_PRIORITY_WARNING)
						level = Logger.LOG_LEVEL_WARNING;

					else if (prio == HDLOG_PRIORITY_INFO)
						level = Logger.LOG_LEVEL_INFO;

					else if (prio == HDLOG_PRIORITY_DEBUG)
						level = Logger.LOG_LEVEL_DEBUG;

					/*
					 * No need to print the native thread ID, as it is
					 * already printed by the core logger.
					 */
					Logger.Print(level, tag, "{0}", msg);
				}

				public static HdLoggerCallback GetHdLoggerCallback()
				{
					return s_HdLoggerCallback;
				}

				public class Writer : TextWriter
				{
					public delegate void WriteFunc(String msg);

					private WriteFunc writeFunc;

					public Writer(WriteFunc writeFunc)
					{
						this.writeFunc = writeFunc;
					}

					public override Encoding Encoding
					{
						get { return Encoding.UTF8; }
					}

					public override void WriteLine(String msg)
					{
						writeFunc(msg);
					}

					public override void WriteLine(String fmt, Object obj)
					{
						writeFunc(String.Format(fmt, obj));
					}

					public override void WriteLine(String fmt, Object[] objs)
					{
						writeFunc(String.Format(fmt, objs));
					}
				}
			}

		}
	}
}
