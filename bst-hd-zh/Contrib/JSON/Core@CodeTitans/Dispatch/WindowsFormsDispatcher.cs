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
#if !WINDOWS_PHONE && !DISABLE_WINDOWS_FORMS
    /// <summary>
    /// Dispatcher class that calls methods on Windows.Forms GUI owned thread.
    /// </summary>
    internal class WindowsFormsDispatcher : IEventDispatcher
    {
        private readonly System.Windows.Forms.Control _control;

#if NET_2_COMPATIBLE
        private delegate void Action<T1, T2, T3>(T1 t1, T2 t2, T3 t3) where T1 : class where T2 : class where T3 : class;
#endif

        /// <summary>
        /// Init constructor.
        /// </summary>
        public WindowsFormsDispatcher(System.Windows.Forms.Control control)
        {
            if (control == null)
                throw new ArgumentNullException("control");

            _control = control;
        }

        /// <summary>
        /// Invoke an event on a thread owned by underlaying dispatcher.
        /// </summary>
        public void Invoke<T>(EventHandler<T> eventHandler, object sender, T e) where T : EventArgs
        {
#if !PocketPC
            if (_control.IsDisposed)
                return;
#endif

            if (_control.InvokeRequired)
            {
                _control.Invoke(new Action<EventHandler<T>, object, T>(Event.Invoke), eventHandler, sender, e);
            }
            else
            {
                Event.Invoke(eventHandler, sender, e);
            }
        }
    }
#endif
}
