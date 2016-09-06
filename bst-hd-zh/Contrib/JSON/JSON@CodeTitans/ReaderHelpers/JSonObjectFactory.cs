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
using CodeTitans.JSon.Objects;

namespace CodeTitans.JSon.ReaderHelpers
{
    /// <summary>
    /// Class that wraps creation of JSON-specific implementations based on read data.
    /// </summary>
    internal sealed class JSonObjectFactory : IObjectFactory
    {
        public object CreateArray(List<Object> data)
        {
            return new JSonArray(data);
        }

        public object CreateObject(Dictionary<String, Object> data)
        {
            return new JSonDictionary(data);
        }

        public object CreateKeyword(TokenDataString keyword)
        {
            return keyword.ValueAsJSonObject;
        }

        public object CreateString(string data)
        {
            return new JSonStringObject(data);
        }

        public object CreateDecimal(Int64 data)
        {
            return new JSonDecimalInt64Object(data);
        }

        public object CreateDecimal(UInt64 data)
        {
            return new JSonDecimalUInt64Object(data);
        }

        public object CreateDecimal(Double data)
        {
            return new JSonDecimalDoubleObject(data);
        }
    }
}
