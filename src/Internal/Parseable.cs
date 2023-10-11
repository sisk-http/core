// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Parseable.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal;

internal class Parseable
{
    public static object? ParseInternal<T>(string value) where T : struct
    {
        Type type = typeof(T);

        if (type == typeof(Byte)) return Byte.Parse(value);
        if(type == typeof(Char)) return Char.Parse(value);
        if(type == typeof(DateOnly)) return DateOnly.Parse(value);
        if(type == typeof(DateTime)) return DateTime.Parse(value);
        if (type == typeof(DateTimeOffset)) return DateTimeOffset.Parse(value);
        if (type == typeof(Decimal)) return Decimal.Parse(value);
        if (type == typeof(Double)) return Double.Parse(value);
        if (type == typeof(Guid)) return Guid.Parse(value);
        if (type == typeof(Half)) return Half.Parse(value);
        if (type == typeof(Int16)) return Int16.Parse(value);
        if (type == typeof(Int32)) return Int32.Parse(value);
        if (type == typeof(Int64)) return Int64.Parse(value);
        if (type == typeof(IntPtr)) return IntPtr.Parse(value);
        if (type == typeof(BigInteger)) return BigInteger.Parse(value);
        if (type == typeof(SByte)) return SByte.Parse(value);
        if (type == typeof(Single)) return Single.Parse(value);
        if (type == typeof(TimeOnly)) return TimeOnly.Parse(value);
        if (type == typeof(TimeSpan)) return TimeSpan.Parse(value);
        if (type == typeof(UInt16)) return UInt16.Parse(value);
        if (type == typeof(UInt32)) return UInt32.Parse(value);
        if (type == typeof(UInt64)) return UInt64.Parse(value);
        if (type == typeof(UIntPtr)) return UIntPtr.Parse(value);

        throw new InvalidCastException($"Cannot cast string to {type.FullName}.");
    }
}
