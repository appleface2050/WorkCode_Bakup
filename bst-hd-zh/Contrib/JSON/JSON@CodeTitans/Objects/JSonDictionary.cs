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
    /// Internal wrapper class that describes JSON object and provides <see cref="IJSonObject"/> access interface.
    /// </summary>
    internal sealed class JSonDictionary : IJSonObject, IJSonWritable
    {
        private readonly Dictionary<string, IJSonObject> _data;

        /// <summary>
        /// Init constructor.
        /// </summary>
        public JSonDictionary(Dictionary<string, object> data)
        {
            _data = new Dictionary<string, IJSonObject>();

            foreach (KeyValuePair<string, object> d in data)
                _data.Add(d.Key, (IJSonObject)d.Value);
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
                Dictionary<string, object> result = new Dictionary<string, object>();

                foreach (KeyValuePair<string, IJSonObject> v in _data)
                {
                    result.Add(v.Key, v.Value.ObjectValue);
                }

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
            get { return _data.Count; }
        }

        IJSonObject IJSonObject.this[int index]
        {
            get { throw new InvalidOperationException(); }
        }

        int IJSonObject.Count
        {
            get { return _data.Count; }
        }

        IJSonObject IJSonObject.this[string name]
        {
            get { return _data[name]; }
        }

        IJSonObject IJSonObject.this[String name, IJSonObject defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                if (defaultValue != null)
                    return defaultValue;

                return null;
            }
        }

        IJSonObject IJSonObject.this[String name, String defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonStringObject(defaultValue);
            }
        }

        IJSonObject IJSonObject.this[String name, String defaultValue, Boolean asJSonSerializedObject]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                if (!asJSonSerializedObject)
                    return new JSonStringObject(defaultValue);

                JSonReader reader = new JSonReader();
                return reader.ReadAsJSonObject(defaultValue);
            }
        }

        IJSonObject IJSonObject.this[String name, Int32 defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonDecimalInt32Object(defaultValue);
            }
        }

        IJSonObject IJSonObject.this[String name, Int64 defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonDecimalInt64Object(defaultValue);
            }
        }

        IJSonObject IJSonObject.this[String name, Single defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonDecimalSingleObject(defaultValue);
            }
        }

        IJSonObject IJSonObject.this[String name, Double defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonDecimalDoubleObject(defaultValue);
            }
        }

        IJSonObject IJSonObject.this[String name, DateTime defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonDecimalInt64Object(defaultValue.ToUniversalTime().Ticks, JSonWriter.ToString(defaultValue));
            }
        }

        IJSonObject IJSonObject.this[String name, TimeSpan defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonDecimalInt64Object(defaultValue.Ticks, JSonWriter.ToString(defaultValue));
            }
        }

        IJSonObject IJSonObject.this[String name, Guid defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonStringObject(JSonWriter.ToString(defaultValue));
            }
        }

        IJSonObject IJSonObject.this[String name, Boolean defaultValue]
        {
            get
            {
                IJSonObject result;

                if (_data.TryGetValue(name, out result))
                    return result;

                return new JSonBooleanObject(defaultValue);
            }
        }

        bool IJSonObject.Contains(string name)
        {
            return _data.ContainsKey(name);
        }

        ICollection<string> IJSonObject.Names
        {
            get { return _data.Keys; }
        }

        IEnumerable<KeyValuePair<string, IJSonObject>> IJSonObject.ObjectItems
        {
            get { return _data; }
        }

        IEnumerable<IJSonObject> IJSonObject.ArrayItems
        {
            get { return _data.Values; }
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
