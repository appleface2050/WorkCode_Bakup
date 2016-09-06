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

namespace CodeTitans.JSon.ReaderHelpers
{
    /// <summary>
    /// Class that wraps operation on creating default .NET types based on read data.
    /// </summary>
    internal sealed class FclObjectFactory : IObjectFactory
    {
        public object CreateArray(List<Object> data)
        {
            return data.ToArray();
        }

        public object CreateObject(Dictionary<String, Object> data)
        {
            return data;
        }

        public object CreateKeyword(TokenDataString keyword)
        {
            return keyword.Value;
        }

        public object CreateString(String data)
        {
            return data;
        }

        public object CreateDecimal(Int64 data)
        {
            return data;
        }

        public object CreateDecimal(UInt64 data)
        {
            return data;
        }

        public object CreateDecimal(Double data)
        {
            return data;
        }
    }
}
