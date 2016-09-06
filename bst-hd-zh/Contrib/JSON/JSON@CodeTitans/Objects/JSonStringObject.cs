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
    /// Internal wrapper class that describes string and provides <see cref="IJSonObject"/> access interface.
    /// </summary>
    internal sealed class JSonStringObject : IJSonObject, IJSonWritable
    {
        private readonly string _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonStringObject(string data)
        {
            _data = data;
        }

        #region IJSonObject Members

        string IJSonObject.StringValue
        {
            get { return _data; }
        }

        int IJSonObject.Int32Value
        {
            get { return Int32.Parse(_data, NumberStyles.Integer, CultureInfo.InvariantCulture); }
        }

        uint IJSonObject.UInt32Value
        {
            get { return UInt32.Parse(_data, NumberStyles.Integer, CultureInfo.InvariantCulture); }
        }

        long IJSonObject.Int64Value
        {
            get { return Int64.Parse(_data, NumberStyles.Integer, CultureInfo.InvariantCulture); }
        }

        ulong IJSonObject.UInt64Value
        {
            get { return UInt64.Parse(_data, NumberStyles.Integer, CultureInfo.InvariantCulture); }
        }

        float IJSonObject.SingleValue
        {
            get { return Single.Parse(_data, NumberStyles.Float, CultureInfo.InvariantCulture); }
        }

        double IJSonObject.DoubleValue
        {
            get { return Double.Parse(_data, NumberStyles.Float, CultureInfo.InvariantCulture); }
        }

        DateTime IJSonObject.DateTimeValue
        {
            get { return DateTime.Parse(_data, CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal); }
        }

        TimeSpan IJSonObject.TimeSpanValue
        {
            get { return TimeSpan.Parse(_data); }
        }

        bool IJSonObject.BooleanValue
        {
            get { return Boolean.Parse(_data); }
        }

        Guid IJSonObject.GuidValue
        {
            get { return new Guid(_data); }
        }

        bool IJSonObject.IsNull
        {
            get { return _data == null; }
        }

        bool IJSonObject.IsTrue
        {
            get { return Boolean.Parse(_data); }
        }

        bool IJSonObject.IsFalse
        {
            get { return !Boolean.Parse(_data); }
        }

        bool IJSonObject.IsEnumerable
        {
            get { return false; }
        }

        object IJSonObject.ObjectValue
        {
            get { return _data; }
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
            if (typeof(T) == typeof(IJSonObject))
            {
                JSonReader reader = new JSonReader();
                return (T)reader.ReadAsJSonObject(_data);
            }

            return (T)JSonObjectConverter.ToObject(this, typeof(T));
        }

        int IJSonObject.Length
        {
            get { return _data.Length; }
        }

        IJSonObject IJSonObject.this[int index]
        {
            get { throw new InvalidOperationException(); }
        }

        int IJSonObject.Count
        {
            get { return _data.Length; }
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

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            output.WriteValue(_data);
        }

        #endregion

        public override string ToString()
        {
            return _data;
        }

    }
}
