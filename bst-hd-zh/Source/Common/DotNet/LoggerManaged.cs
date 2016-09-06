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
using System.IO;
using System.Text;
using System.Threading;
using Microsoft.Win32;
using System.Diagnostics;

namespace BlueStacks {
    namespace hyperDroid {
	namespace Common {

	    public class Logger {
		public  static  int		LOG_LEVEL_FATAL		= 1;
		public  static  int		LOG_LEVEL_ERROR		= 2;
		public  static  int		LOG_LEVEL_WARNING	= 3;
		public  static  int		LOG_LEVEL_DEBUG		= 4;
		public  static  int		LOG_LEVEL_INFO		= 5;

		private static int		HDLOG_PRIORITY_FATAL	= 0;
		private static int		HDLOG_PRIORITY_ERROR	= 1;
		private static int		HDLOG_PRIORITY_WARNING	= 2;
		private static int		HDLOG_PRIORITY_INFO	= 3;
		private static int		HDLOG_PRIORITY_DEBUG	= 4;

		private	static	Object		s_sync			= new Object();
		private	static	TextWriter	writer			= System.Console.Error;
		private	static	int		s_logRotationTime	= 30000; /* 30 sec */
		public	static	int		s_logFileSize		= 10 * 1024 * 1024;
		public	static	int		s_totalLogFileNum	= 5;
		private	static	String		s_logFileName		= "BlueStacks";
		private	static	String		s_logFilePath		= null;
		private static	bool		s_consoleLogging	= false;
		private static	bool		s_loggerInited		= false;
		private static	int		s_processId		= -1;
		private static	String		s_processName		= "Unknown";
		private static  String		s_logLevels		= null;
		private	static	FileStream	s_fileStream;
		private static	string		s_logDir		= null;

		/* Log levels string */
		private static	String		s_logStringFatal	= "FATAL";
		private static	String		s_logStringError	= "ERROR";
		private static	String		s_logStringWarning	= "WARNING";
		private static	String		s_logStringInfo		= "INFO";
		private static	String		s_logStringDebug	= "DEBUG";

		public delegate void HdLoggerCallback(int prio, uint tid, String tag,
				String msg);

		public static HdLoggerCallback s_HdLoggerCallback;

		private static String GetLogDir(
			bool		userSpecificLog
			)
		{
			if (s_logDir != null)
				return s_logDir;

			String logDir;
			RegistryKey key = Registry.LocalMachine.OpenSubKey(Common.Strings.RegBasePath);
			if(key == null || (key.GetValue("DataDir") == null &&
						key.GetValue("UserDataDir") == null))
			{
				logDir = Environment.GetFolderPath(
						Environment.SpecialFolder.LocalApplicationData);
				logDir = Path.Combine(logDir, "Bluestacks");
			}
			else if(key.GetValue("DataDir") != null)
			{
					logDir = (String)key.GetValue("DataDir");
			}
			else
			{
					logDir = (String)key.GetValue("UserDataDir");
			}
			logDir = Path.Combine(logDir, "Logs");
			s_logDir = logDir;

			return logDir;
		}

		public static void SetLogDir(
			string		logDir
			)
		{
			s_logDir = logDir;
		}

		// Wrapper functions for initializing logs
		public static void InitUserLog()
		{
		    InitLog(null, true, false);
		}

		public static void InitUserLogWithRotation()
		{
		    InitLog(null, true, true);
		}

		public static void InitSystemLog()
		{
		    InitLog(null, false, false);
		}

		public static void InitSystemLogWithRotation()
		{
		    InitLog(null, false, true);
		}

		public static void InitConsoleLog()
		{
		    InitLog("-", true, false);
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

			Logger.Print(level, tag, "{0:X8}: {1}", tid, msg);
		}

		/*
		 * doLogRotation should be set by only one process. Currently, HD-Agent would
		 * be doing the log rotation, for rest of the components, this must be 'false'
		 */
		public static void InitLog(
			String		logFileName,
			bool		userSpecificLog,
			bool		doLogRotation
			)
		{
		    s_loggerInited = true;

		    Logger.s_HdLoggerCallback = new HdLoggerCallback(HdLogger);

		    s_processId = Process.GetCurrentProcess().Id;
		    s_processName = Process.GetCurrentProcess().ProcessName;

		    if (logFileName == "-")
		    {
			writer = System.Console.Error;
			s_consoleLogging = true;
		    }
		    else
		    {
			if (logFileName == null)
			    logFileName = s_logFileName;

			if (userSpecificLog)
			    logFileName = logFileName + "Users";

			String logDir = GetLogDir(userSpecificLog);

			String logPath = String.Format("{0}\\{1}.log", logDir, logFileName);

			if (!Directory.Exists(logDir))
			    Directory.CreateDirectory(logDir);

			s_logFilePath = logPath;

			LogLevelsInit();

			if (doLogRotation)
			{
			    Thread logRotationThread = new Thread(
				    delegate() {
				    DoLogRotation();
				    });

			    logRotationThread.IsBackground = true;
			    logRotationThread.Start();
			}
		    }
		}

		private static void LogLevelsInit()
		{
		    /* XXXDPR:  Hard coded VM name. */
		    String path = @"Software\BlueStacks\Guests\Android\Config";

		    RegistryKey key;

		    try
		    {
			using (key = Registry.LocalMachine.OpenSubKey(path)) {
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

		private static void DoLogRotation()
		{
		    while (true)
		    {
			/* Sleep for 30 sec and check for log file size */
			Thread.Sleep(s_logRotationTime);

			try
			{
			    lock (s_sync)
			    {
				FileInfo	logFileInfo	= new FileInfo(s_logFilePath);

				if (logFileInfo.Length >= s_logFileSize)
				{
				    String newName = s_logFilePath + ".1";
				    String oldestFileName = s_logFilePath + "." + s_totalLogFileNum;

				    if (File.Exists(oldestFileName))
				    {
					File.Delete(oldestFileName);
				    }

				    for (int i = s_totalLogFileNum - 1; i >= 1; i--)
				    {
					String oldFileName = s_logFilePath + "." + i;
					String newFileName = s_logFilePath + "." + (i + 1);

					if (File.Exists(oldFileName))
					{
					    File.Move(oldFileName, newFileName);
					}
				    }

				    File.Move(s_logFilePath, newName);
				}
			    }
			}
			catch (Exception)
			{
			    /* Ignore */
			}
		    }
		}

		private static void Open()
		{
		    if (s_consoleLogging)
		    {
			return;
		    }

		    if (!s_loggerInited)
		    {
			InitLog("-", false, false);
			s_loggerInited = true;
			return;
		    }

		    s_fileStream = new FileStream(s_logFilePath, FileMode.Append, FileAccess.Write, FileShare.ReadWrite | FileShare.Delete);
		    writer = new StreamWriter(s_fileStream, Encoding.UTF8);
		}

		private static void Close()
		{
		    if (s_consoleLogging)
			return;

		    writer.Close();
		    s_fileStream.Dispose();
		    writer.Dispose();
		}

		public static TextWriter GetWriter()
		{
		    return new Writer(delegate(String msg) {
			    Print(msg);
			    });
		}

		private static void Print(int level, String tag, String fmt, params Object[] args)
		{
		    String s = "UNKNOWN";

		    if (level == LOG_LEVEL_FATAL)
			s = s_logStringFatal;
		    else if (level == LOG_LEVEL_ERROR)
			s = s_logStringError;
		    else if (level == LOG_LEVEL_WARNING)
			s = s_logStringWarning;
		    else if (level == LOG_LEVEL_INFO)
			s = s_logStringInfo;
		    else if (level == LOG_LEVEL_DEBUG)
			s = s_logStringDebug;

		    if (level == LOG_LEVEL_DEBUG && !IsLogLevelEnabled(tag, s))
			return;

		    lock (s_sync) {
			Open();
			writer.WriteLine(GetPrefix(tag, s) + fmt, args);
			writer.Flush();
			Close();
		    }
		}

		private static void Print(String fmt, params Object[] args)
		{
		    Print(LOG_LEVEL_INFO, s_processName, fmt, args);
		}

		private static void Print(String msg)
		{
		    Print("{0}", msg);
		}

		public static void Fatal(String fmt, params Object[] args)
		{
		    Print(LOG_LEVEL_FATAL, s_processName, fmt, args);
		}

		public static void Fatal(String msg)
		{
		    Fatal("{0}", msg);
		}

		public static void Error(String fmt, params Object[] args)
		{
		    Print(LOG_LEVEL_ERROR, s_processName, fmt, args);
		}

		public static void Error(String msg)
		{
		    Error("{0}", msg);
		}

		public static void Warning(String fmt, params Object[] args)
		{
		    Print(LOG_LEVEL_WARNING, s_processName, fmt, args);
		}

		public static void Warning(String msg)
		{
		    Warning("{0}", msg);
		}

		public static void Info(String fmt, params Object[] args)
		{
		    Print(LOG_LEVEL_INFO, s_processName, fmt, args);
		}

		public static void Info(String msg)
		{
		    Info("{0}", msg);
		}

		public static void Debug(String fmt, params Object[] args)
		{
		    Print(LOG_LEVEL_DEBUG, s_processName, fmt, args);
		}

		public static void Debug(String msg)
		{
		    Debug("{0}", msg);
		}

		private static String GetPrefix(String tag, String logLevel)
		{
		    int tid = Thread.CurrentThread.ManagedThreadId;
		    DateTime now = DateTime.Now;

		    return String.Format("{0:D4}-{1:D2}-{2:D2} " +
			    "{3:D2}:{4:D2}:{5:D2}.{6:D3} {7}:{8:X8} ({9}). {10}: ",
			    now.Year, now.Month, now.Day, now.Hour, now.Minute,
			    now.Second, now.Millisecond, s_processId, tid, tag, logLevel);
		}

		public class Writer : TextWriter {
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
