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
using System.Collections;

namespace CodeTitans.JSon.Objects
{
    /// <summary>
    /// Internal wrapper class that describes array of IJSonObjects and provides <see cref="IJSonObject"/> access interface.
    /// </summary>
    internal sealed class JSonArray : IJSonObject, IJSonWritable
    {
        private readonly IJSonObject[] _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonArray(List<object> data)
        {
            _data = new IJSonObject[data.Count];

            // convert and copy data as an array:
            int i = 0;
            foreach (object d in data)
                _data[i++] = (IJSonObject)d;
        }

        #region IJSonObject Members

        string IJSonObject.StringValue
        {
            get { throw new InvalidOperationException(); }
        }

        int IJSonObject.Int32Value
        {
            get { throw new InvalidOperationException(); }
        }

        uint IJSonObject.UInt32Value
        {
            get { throw new InvalidOperationException(); }
        }

        long IJSonObject.Int64Value
        {
            get { throw new InvalidOperationException(); }
        }

        ulong IJSonObject.UInt64Value
        {
            get { throw new InvalidOperationException(); }
        }

        float IJSonObject.SingleValue
        {
            get { throw new InvalidOperationException(); }
        }

        double IJSonObject.DoubleValue
        {
            get { throw new InvalidOperationException(); }
        }

        DateTime IJSonObject.DateTimeValue
        {
            get { throw new InvalidOperationException(); }
        }

        TimeSpan IJSonObject.TimeSpanValue
        {
            get { throw new InvalidOperationException(); }
        }

        bool IJSonObject.BooleanValue
        {
            get { throw new InvalidOperationException(); }
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
            get { return false; }
        }

        bool IJSonObject.IsFalse
        {
            get { return true; }
        }

        bool IJSonObject.IsEnumerable
        {
            get { return true; }
        }

        object IJSonObject.ObjectValue
        {
            get
            {
                object[] result = new object[_data.Length];

                for (int i = 0; i < _data.Length; i++)
                    result[i] = _data[i].ObjectValue;

                return result;
            }
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
            get { return _data.Length; }
        }

        IJSonObject IJSonObject.this[int index]
        {
            get { return _data[index]; }
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

        IJSonObject IJSonObject.this[String name, String defaultValue, Boolean asJSonSerializedObject]
        {
            get { throw new InvalidOperationException(); }
        }

        IJSonObject IJSonObject.this[String name, String defaultValue]
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
            get { return null; }
        }

        IEnumerable<IJSonObject> IJSonObject.ArrayItems
        {
            get { return _data; }
        }

        #endregion

        #region IJSonWritable Members

        void IJSonWritable.Write(IJSonWriter output)
        {
            output.Write(_data);
        }

        #endregion

        public override string ToString()
        {
            using (IJSonWriter writer = new JSonWriter(true))
            {
                writer.Write(_data);
                return writer.ToString();
            }
        }
    }
}
