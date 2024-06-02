// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StringValue.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an instance that hosts a string value and allows conversion to common types.
/// </summary>
public struct StringValue : ICloneable, IEquatable<StringValue>, IComparable<StringValue>
{
    private readonly string? _ref;
    private readonly string argName;
    private readonly string argType;

    internal StringValue(string name, string type, string? data)
    {
        _ref = data;
        argName = name;
        argType = type;
    }

    /// <summary>
    /// Gets the name of the property that hosts this <see cref="StringValue"/>.
    /// </summary>
    public string Name { get => argName; }

    /// <summary>
    /// Gets the value slot of this <see cref="StringValue"/>.
    /// </summary>
    public string? Value { get => _ref; }

    /// <summary>
    /// Gets an boolean indicating if this object value is null or an empty string.
    /// </summary>
    public bool IsNullOrEmpty { get => string.IsNullOrEmpty(_ref); }

    /// <summary>
    /// Gets an boolean indicating if this object value is null.
    /// </summary>
    public bool IsNull { get => _ref is null; }

    /// <summary>
    /// Returns a self-reference to this object when it's value is not null.
    /// </summary>
    public StringValue? MaybeNull()
    {
        if (IsNull)
        {
            return null;
        }
        return this;
    }

    /// <summary>
    /// Returns a self-reference to this object when it's value is not null or an empty string.
    /// </summary>
    public StringValue? MaybeNullOrEmpty()
    {
        if (IsNullOrEmpty)
        {
            return null;
        }
        return this;
    }

    /// <summary>
    /// Gets a non-null string from this <see cref="StringValue"/>. This method will throw an <see cref="NullReferenceException"/> if
    /// the value stored in this instance is null.
    /// </summary>
    /// <returns>An non-null string value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    public string GetString()
    {
        ThrowIfNull();
        return _ref!;
    }

    /// <summary>
    /// Gets a <see cref="char"/> from this <see cref="StringValue"/>. This method will throw an <see cref="NullReferenceException"/> if
    /// the value stored in this instance is null.
    /// </summary>
    /// <returns>An non-null char value.</returns>
    public char GetChar()
    {
        ThrowIfNull();
        if (_ref!.Length != 1)
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "char"));

        return _ref[0];
    }

    /// <summary>
    /// Gets a <see cref="Int32"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <returns>An non-null Int32 value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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

    /// <summary>
    /// Gets a <see cref="Byte"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <returns>An non-null byte value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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

    /// <summary>
    /// Gets a <see cref="Int64"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <returns>An non-null long value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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

    /// <summary>
    /// Gets a <see cref="Int16"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <param name="fmtProvider">Optional. Specifies the culture-specific format information.</param>
    /// <returns>An non-null short value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
    public short GetShort(IFormatProvider? fmtProvider = null)
    {
        ThrowIfNull();
        try
        {
            return short.Parse(_ref!, fmtProvider);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "short"));
        }
    }

    /// <summary>
    /// Gets a <see cref="double"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <param name="fmtProvider">Optional. Specifies the culture-specific format information.</param>
    /// <returns>An non-null double value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
    public double GetDouble(IFormatProvider? fmtProvider = null)
    {
        ThrowIfNull();
        try
        {
            return double.Parse(_ref!, fmtProvider);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "double"));
        }
    }

    /// <summary>
    /// Gets a <see cref="bool"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <returns>An non-null boolean value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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

    /// <summary>
    /// Gets a <see cref="DateTime"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <param name="fmtProvider">Optional. Specifies the culture-specific format information.</param>
    /// <returns>An non-null DateTime value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
    public DateTime GetDateTime(IFormatProvider? fmtProvider = null)
    {
        ThrowIfNull();
        try
        {
            return DateTime.Parse(_ref!, fmtProvider);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, _ref, argName, "DateTime"));
        }
    }

    /// <summary>
    /// Gets a <see cref="Guid"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <returns>An non-null Guid value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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

    /// <summary>
    /// Gets an not null value from the specified <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    public T Get<T>() where T : struct
    {
        ThrowIfNull();
        object result = Internal.Parseable.ParseInternal<T>(_ref!);
        return Unsafe.Unbox<T>(result);
    }

    void ThrowIfNull()
    {
        if (IsNull)
        {
            throw new NullReferenceException(string.Format(SR.ValueItem_ValueNull, argName, argType));
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static implicit operator string?(StringValue i) => i._ref;

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator ==(StringValue i, object? other)
    {
        return i.Equals(other);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator !=(StringValue i, object? other)
    {
        return !i.Equals(other);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public override bool Equals(object? obj)
    {
        if (obj is null)
        {
            return IsNull;
        }
        else if (obj is StringValue sv)
        {
            return _ref?.Equals(sv._ref) == true;
        }
        else if (obj is string ss)
        {
            return _ref?.Equals(ss) == true;
        }
        else
        {
            return false;
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public override int GetHashCode()
    {
        return _ref?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public object Clone()
    {
        return new StringValue((string)argName.Clone(), (string)argType.Clone(), (string?)_ref?.Clone());
    }

    /// <inheritdoc/>
    /// <exclude/>
    public override string? ToString()
    {
        return _ref?.ToString();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Equals(StringValue other)
    {
        return Equals(other);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public int CompareTo(StringValue other)
    {
        return string.Compare(this._ref, other._ref);
    }
}
