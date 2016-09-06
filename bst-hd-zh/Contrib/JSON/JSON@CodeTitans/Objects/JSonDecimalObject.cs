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
using System.Collections.Generic;
using System.Globalization;
using System.Collections;

namespace CodeTitans.JSon.Objects
{
    /// <summary>
    /// Internal wrapper class that describes numeric type and provides <see cref="IJSonObject"/> access interface.
    /// </summary>
    internal abstract class JSonDecimalObject : IJSonObject
    {
        #region Protected IJSonObject Members

        protected abstract string GetStringValue();
        protected abstract int GetInt32Value();
        protected abstract uint GetUInt32Value();
        protected abstract long GetInt64Value();
        protected abstract ulong GetUInt64Value();
        protected abstract float GetSingleValue();
        protected abstract double GetDoubleValue();
        protected abstract bool GetBooleanValue();
        protected abstract object GetObjectValue();

        #endregion

        #region IJSonObject Members

        string IJSonObject.StringValue
        {
            get { return GetStringValue(); }
        }

        int IJSonObject.Int32Value
        {
            get { return GetInt32Value(); }
        }

        uint IJSonObject.UInt32Value
        {
            get { return GetUInt32Value(); }
        }

        long IJSonObject.Int64Value
        {
            get { return GetInt64Value(); }
        }

        ulong IJSonObject.UInt64Value
        {
            get { return GetUInt64Value(); }
        }

        float IJSonObject.SingleValue
        {
            get { return GetSingleValue(); }
        }

        double IJSonObject.DoubleValue
        {
            get { return GetDoubleValue(); }
        }

        DateTime IJSonObject.DateTimeValue
        {
            get { return new DateTime(GetInt64Value(), DateTimeKind.Utc); }
        }

        TimeSpan IJSonObject.TimeSpanValue
        {
            get { return new TimeSpan(GetInt64Value()); }
        }

        bool IJSonObject.BooleanValue
        {
            get { return GetBooleanValue(); }
        }

        object IJSonObject.ObjectValue
        {
            get { return GetObjectValue(); }
        }

        Guid IJSonObject.GuidValue
        {
            get { throw new InvalidOperationException(); }
        }

        bool IJSonObject.IsNull
        {
            get { return false; }
        }

        bool IJSonObject.IsTrue
        {
            get { return GetBooleanValue(); }
        }

        bool IJSonObject.IsFalse
        {
            get { return !GetBooleanValue(); }
        }

        bool IJSonObject.IsEnumerable
        {
            get { return false; }
        }

        /// <summary>
        /// Gets the value of given JSON object.
        /// </summary>
        object IJSonObject.ToValue(Type t)
        {
            return JSonObjectConverter.ToObject(this, t);
        }

        /// <summary>
        /// Get the value of given JSON object.
        /// </summary>
        T IJSonObject.ToObjectValue<T>()
        {
            return (T)JSonObjectConverter.ToObject(this, typeof(T));
        }

        int IJSonObject.Length
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[int index]
        {
            get { throw new InvalidOperationException(); }
        }

        int IJSonObject.Count
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[string name]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, IJSonObject defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, String defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, String defaultValue, Boolean asJSonSerializedObject]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, Int32 defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, Int64 defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, Single defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, Double defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, DateTime defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, TimeSpan defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, Guid defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, Boolean defaultValue]
        {
            get { throw new InvalidOperationException(); }
        }

        bool IJSonObject.Contains(string name)
        {
            return false;
        }

        ICollection<string> IJSonObject.Names
        {
            get { return null; }
        }

        IEnumerable<KeyValuePair<string, IJSonObject>> IJSonObject.ObjectItems
        {
            get { throw new NotImplementedException(); }
        }

        IEnumerable<IJSonObject> IJSonObject.ArrayItems
        {
            get { throw new NotImplementedException(); }
        }

        #endregion

        public override string ToString()
        {
            return GetStringValue();
        }
    }

    internal sealed class JSonDecimalInt32Object : JSonDecimalObject, IJSonWritable
    {
        private readonly Int32 _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonDecimalInt32Object(Int32 data)
        {
            _data = data;
        }

        protected override string GetStringValue()
        {
            return _data.ToString(CultureInfo.InvariantCulture);
        }

        protected override int GetInt32Value()
        {
            return _data;
        }

        protected override uint GetUInt32Value()
        {
            return (uint)_data;
        }

        protected override long GetInt64Value()
        {
            return _data;
        }

        protected override ulong GetUInt64Value()
        {
            return (ulong)_data;
        }

        protected override float GetSingleValue()
        {
            return _data;
        }

        protected override double GetDoubleValue()
        {
            return _data;
        }

        protected override bool GetBooleanValue()
        {
            return _data != 0;
        }

        protected override object GetObjectValue()
        {
            return _data;
        }

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            output.WriteValue(_data);
        }

        #endregion
    }

    internal sealed class JSonDecimalInt64Object : JSonDecimalObject, IJSonWritable
    {
        private readonly Int64 _data;
        private readonly string _stringRepresentation;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonDecimalInt64Object(Int64 data)
        {
            _data = data;
        }

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonDecimalInt64Object(Int64 data, string stringRepresentation)
        {
            _data = data;
            _stringRepresentation = stringRepresentation;
        }

        protected override string GetStringValue()
        {
            if (_stringRepresentation != null)
                return _stringRepresentation;

            return _data.ToString(CultureInfo.InvariantCulture);
        }

        protected override int GetInt32Value()
        {
            return (int)_data;
        }

        protected override uint GetUInt32Value()
        {
            return (uint)_data;
        }

        protected override long GetInt64Value()
        {
            return _data;
        }

        protected override ulong GetUInt64Value()
        {
            return (ulong)_data;
        }

        protected override float GetSingleValue()
        {
            return _data;
        }

        protected override double GetDoubleValue()
        {
            return _data;
        }

        protected override bool GetBooleanValue()
        {
            return _data != 0;
        }

        protected override object GetObjectValue()
        {
            return _data;
        }

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            output.WriteValue(_data);
        }

        #endregion
    }

    internal sealed class JSonDecimalUInt64Object : JSonDecimalObject, IJSonWritable
    {
        private readonly UInt64 _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonDecimalUInt64Object(UInt64 data)
        {
            _data = data;
        }

        protected override string GetStringValue()
        {
            return _data.ToString(CultureInfo.InvariantCulture);
        }

        protected override int GetInt32Value()
        {
            return (int)_data;
        }

        protected override uint GetUInt32Value()
        {
            return (uint)_data;
        }

        protected override long GetInt64Value()
        {
            return (long)_data;
        }

        protected override ulong GetUInt64Value()
        {
            return _data;
        }

        protected override float GetSingleValue()
        {
            return _data;
        }

        protected override double GetDoubleValue()
        {
            return _data;
        }

        protected override bool GetBooleanValue()
        {
            return _data != 0;
        }

        protected override object GetObjectValue()
        {
            return _data;
        }

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            output.WriteValue(_data);
        }

        #endregion
    }

    internal sealed class JSonDecimalSingleObject : JSonDecimalObject, IJSonWritable
    {
        private readonly Single _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonDecimalSingleObject(Single data)
        {
            _data = data;
        }

        protected override string GetStringValue()
        {
            return _data.ToString(CultureInfo.InvariantCulture);
        }

        protected override int GetInt32Value()
        {
            return (int)_data;
        }

        protected override uint GetUInt32Value()
        {
            return (uint)_data;
        }

        protected override long GetInt64Value()
        {
            return (long)_data;
        }

        protected override ulong GetUInt64Value()
        {
            return (ulong)_data;
        }

        protected override float GetSingleValue()
        {
            return _data;
        }

        protected override double GetDoubleValue()
        {
            return _data;
        }

        protected override bool GetBooleanValue()
        {
            return _data != 0;
        }

        protected override object GetObjectValue()
        {
            return _data;
        }

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            output.WriteValue(_data);
        }

        #endregion
    }

    internal sealed class JSonDecimalDoubleObject : JSonDecimalObject, IJSonWritable
    {
        private readonly Double _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonDecimalDoubleObject(Double data)
        {
            _data = data;
        }

        protected override string GetStringValue()
        {
            return _data.ToString(CultureInfo.InvariantCulture);
        }

        protected override int GetInt32Value()
        {
            return (int)_data;
        }

        protected override uint GetUInt32Value()
        {
            return (uint)_data;
        }

        protected override long GetInt64Value()
        {
            return (long)_data;
        }

        protected override ulong GetUInt64Value()
        {
            return (ulong)_data;
        }

        protected override float GetSingleValue()
        {
            return (float)_data;
        }

        protected override double GetDoubleValue()
        {
            return _data;
        }

        protected override bool GetBooleanValue()
        {
            return _data != 0;
        }

        protected override object GetObjectValue()
        {
            return _data;
        }

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            output.WriteValue(_data);
        }

        #endregion
    }
}
