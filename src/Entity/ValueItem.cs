// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ValueItem.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Entity;

public class ValueItem
{
    private string? _ref;
    private string argName;
    private string argType;

    internal ValueItem(string name, string type, string? data)
    {
        _ref = data;
        argName = name;
        argType = type;
    }

    public string Name { get => argName; }
    public string? Value { get => _ref; }
    public bool IsNullOrEmpty { get => string.IsNullOrEmpty(_ref); }
    public bool IsNull { get => _ref == null; }

    public ValueItem? MaybeNull()
    {
        if (IsNull)
        {
            return null;
        }
        return this;
    }

    public string GetString()
    {
        ThrowIfNull();
        return _ref!;
    }

    public int GetInteger()
    {
        ThrowIfNull();
        try
        {
            return int.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "integer"));
        }
    }

    public int GetByte()
    {
        ThrowIfNull();
        try
        {
            return byte.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "byte"));
        }
    }

    public long GetLong()
    {
        ThrowIfNull();
        try
        {
            return long.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "long"));
        }
    }

    public short GetShort()
    {
        ThrowIfNull();
        try
        {
            return short.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "short"));
        }
    }

    public double GetDouble()
    {
        ThrowIfNull();
        try
        {
            return double.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "double"));
        }
    }

    public bool GetBoolean()
    {
        ThrowIfNull();
        try
        {
            return bool.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "boolean"));
        }
    }

    public DateTime GetDateTime()
    {
        ThrowIfNull();
        try
        {
            return DateTime.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "DateTime"));
        }
    }

    public Guid GetGuid()
    {
        ThrowIfNull();
        try
        {
            return Guid.Parse(_ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "GUID"));
        }
    }

    void ThrowIfNull()
    {
        if (IsNull)
        {
            throw new NullReferenceException(string.Format(SR.ValueItem_ValueNull, argName, argType));
        }
    }

    public static implicit operator string?(ValueItem i)
    {
        return i.Value;
    }
}
