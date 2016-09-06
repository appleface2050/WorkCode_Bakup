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

namespace CodeTitans.JSon.Objects
{
    /// <summary>
    /// Internal wrapper class that describes JSON boolean value.
    /// </summary>
    internal sealed class JSonBooleanObject : JSonDecimalObject, IJSonWritable
    {
        private readonly bool _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonBooleanObject(bool value)
        {
            _data = value;
        }

        protected override string GetStringValue()
        {
            return _data ? JSonReader.TrueString : JSonReader.FalseString;
        }

        protected override int GetInt32Value()
        {
            return _data ? 1: 0;
        }

        protected override uint GetUInt32Value()
        {
            return _data ? 1u : 0u;
        }

        protected override long GetInt64Value()
        {
            return _data ? 1L : 0L;
        }

        protected override ulong GetUInt64Value()
        {
            return _data ? 1UL: 0UL;
        }

        protected override float GetSingleValue()
        {
            return _data ? 1.0f : 0.0f;
        }

        protected override double GetDoubleValue()
        {
            return _data ? 1.0d : 0.0d;
        }

        protected override bool GetBooleanValue()
        {
            return _data;
        }

        protected override object GetObjectValue()
        {
            return _data;
        }

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            // writes current decimal value as boolean:
            output.WriteValue(_data);
        }

        #endregion
    }
}
