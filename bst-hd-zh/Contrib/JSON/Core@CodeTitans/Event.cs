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
using System.Threading;

namespace CodeTitans.Core
{
    /// <summary>
    /// Helper class for invoking events in thread-safe manner.
    /// </summary>
    public static class Event
    {
        /// <summary>
        /// Invokes event with given parameter.
        /// </summary>
        public static void Invoke<T>(EventHandler<T> eventHandler, object sender, T e) where T : EventArgs
        {
#if !DISABLE_INTERLOCKED
            EventHandler<T> eh = Interlocked.CompareExchange(ref eventHandler, null, null);
#else
            EventHandler<T> eh = eventHandler;
#endif
            if (eh != null)
                eh(sender, e);
        }
    }
}
