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

namespace CodeTitans.Core.Dispatch
{
    /// <summary>
    /// Default dispatcher that calls given method on the same thread.
    /// </summary>
    internal class DefaultDispatcher : IEventDispatcher
    {
        /// <summary>
        /// Invoke an event on a thread owned by underlaying dispatcher.
        /// </summary>
        public void Invoke<T>(EventHandler<T> eventHandler, object sender, T e) where T : EventArgs
        {
            Event.Invoke(eventHandler, sender, e);
        }
    }
}
