#region License
/*
    Copyright (c) 2010, Paweł Hofman (CodeTitans)
    All Rights Reserved.

    Licensed under the Apache License version 2.0.
    For more information please visit:

    http://codetitans.codeplex.com/license
        or
    http://www.apache.org/licenses/


    For latest source code, documentation, samples
    and more information please visit:

    http://codetitans.codeplex.com/
*/
#endregion

using System;
using System.Diagnostics;

namespace CodeTitans.Diagnostics
{
    /// <summary>
    /// Common class for writing logs inside whole CodeTitans libraries.
    /// </summary>
    internal static class DebugLog
    {
        /// <summary>
        /// Writes debug log message.
        /// </summary>
        [Conditional("TRACE")]
        public static void WriteLine(string category, string message)
        {
            //Debug.WriteLine(message != null && message.Length > 1024 ? message.Substring(0, 1024) : message);
            Debug.WriteLine(message);
            Console.WriteLine(message);
        }

        /// <summary>
        /// Writes exception info to the debug log.
        /// </summary>
        [Conditional("TRACE")]
        public static void WriteException(string category, Exception ex)
        {
            Debug.WriteLine("### " + ex.Message);
            Debug.WriteLine(ex.StackTrace);
            Console.WriteLine("### " + ex.Message);
            Console.WriteLine(ex.StackTrace);
        }

        /// <summary>
        /// Writes general debug log message.
        /// </summary>
        [Conditional("TRACE")]
        public static void Log(string message)
        {
            WriteLine("General", message);
        }

        /// <summary>
        /// Writes general exception message to the log.
        /// </summary>
        [Conditional("TRACE")]
        public static void Log(Exception ex)
        {
            WriteException("General", ex);
        }

        /// <summary>
        /// Writes Bayeux debug message.
        /// </summary>
        [Conditional("TRACE")]
        public static void WriteBayeuxLine(string message)
        {
            WriteLine("Bayeux", message);
        }

        /// <summary>
        /// Writes info about exception in Bayeux module.
        /// </summary>
        [Conditional("TRACE")]
        public static void WriteBayeuxException(Exception ex)
        {
            WriteException("Bayeux", ex);
        }

        /// <summary>
        /// Writes Core debug message.
        /// </summary>
        [Conditional("TRACE")]
        public static void WriteCoreLine(string message)
        {
            WriteLine("Core", message);
        }

        /// <summary>
        /// Writes info about exception in Core module.
        /// </summary>
        [Conditional("TRACE")]
        public static void WriteCoreException(Exception ex)
        {
            WriteException("Core", ex);
        }
    }
}
