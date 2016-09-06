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
    internal interface IObjectFactory
    {
        object CreateArray(List<Object> data);
        object CreateObject(Dictionary<String, Object> data);
        object CreateKeyword(TokenDataString keyword);
        object CreateString(String data);
        object CreateDecimal(Int64 data);
        object CreateDecimal(UInt64 data);
        object CreateDecimal(Double data);
    }
}
