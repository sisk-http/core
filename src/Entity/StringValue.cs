// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StringValue.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an instance that hosts a string value and allows conversion to common types.
/// </summary>
/// <definition>
/// public class StringValue
/// </definition>
/// <type>
/// Class
/// </type>
public class StringValue
{
    private string? _ref;
    private string argName;
    private string argType;

    internal StringValue(string name, string type, string? data)
    {
        _ref = data;
        argName = name;
        argType = type;
    }

    /// <summary>
    /// Gets the name of the property that hosts this <see cref="StringValue"/>.
    /// </summary>
    /// <definition>
    /// public string Name { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string Name { get => argName; }

    /// <summary>
    /// Gets the value slot of this <see cref="StringValue"/>.
    /// </summary>
    /// <definition>
    /// public string? Value { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public string? Value { get => _ref; }

    /// <summary>
    /// Gets an boolean indicating if this object value is null or an empty string.
    /// </summary>
    /// <definition>
    /// public bool IsNullOrEmpty { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public bool IsNullOrEmpty { get => string.IsNullOrEmpty(_ref); }

    /// <summary>
    /// Gets an boolean indicating if this object value is null.
    /// </summary>
    /// <definition>
    /// public bool IsNull { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public bool IsNull { get => _ref == null; }

    /// <summary>
    /// Returns a self-reference to this object when its value is not null.
    /// </summary>
    /// <definition>
    /// public StringValue? MaybeNull()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public StringValue? MaybeNull()
    {
        if (IsNull)
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
    /// <returns>An non-null short value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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

    /// <summary>
    /// Gets a <see cref="double"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <returns>An non-null double value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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
    /// <returns>An non-null DateTime value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
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

    void ThrowIfNull()
    {
        if (IsNull)
        {
            throw new NullReferenceException(string.Format(SR.ValueItem_ValueNull, argName, argType));
        }
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public static implicit operator string?(StringValue i)
    {
        return i.Value;
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public static bool operator ==(StringValue i, string? other)
    {
        return i.Value == other;
    }

    /// <inheritdoc/>
    /// <nodocs/>
    public static bool operator !=(StringValue i, string? other)
    {
        return i.Value != other;
    }
}
