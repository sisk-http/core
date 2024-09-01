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
/// Represents an option/monad item that wraps an string value and allows conversion to most common types.
/// </summary>
public readonly struct StringValue : ICloneable, IEquatable<StringValue>, IComparable<StringValue>
{
    private readonly string? _ref;
    private readonly string argName;
    private readonly string argType;

    internal StringValue(string name, string type, string? data)
    {
        this._ref = data;
        this.argName = name;
        this.argType = type;
    }

    /// <summary>
    /// Creates an new empty value of the <see cref="StringValue"/> with no predefined value.
    /// </summary>
    /// <param name="name">The <see cref="StringValue"/> name.</param>
    public StringValue(string name)
    {
        this._ref = null;
        this.argName = name;
        this.argType = "StringValue";
    }

    /// <summary>
    /// Creates an new value of the <see cref="StringValue"/>.
    /// </summary>
    /// <param name="name">The <see cref="StringValue"/> name.</param>
    /// <param name="value">The <see cref="StringValue"/> value.</param>
    public StringValue(string name, string? value)
    {
        this._ref = value;
        this.argName = name;
        this.argType = "StringValue";
    }

    /// <summary>
    /// Gets the name of the property that hosts this <see cref="StringValue"/>.
    /// </summary>
    public string Name { get => this.argName; }

    /// <summary>
    /// Gets the value of the current <see cref="StringValue"/> string if it has been assigned a valid underlying value.
    /// </summary>
    public string? Value { get => this._ref; }

    /// <summary>
    /// Gets an boolean indicating if this object value is null or an empty string.
    /// </summary>
    public bool IsNullOrEmpty { get => string.IsNullOrEmpty(this._ref); }

    /// <summary>
    /// Gets an boolean indicating if this object value is null.
    /// </summary>
    public bool IsNull { get => this._ref is null; }

    /// <summary>
    /// Returns a self-reference to this object when it's value is not null.
    /// </summary>
    public StringValue? MaybeNull()
    {
        if (this.IsNull)
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
        if (this.IsNullOrEmpty)
        {
            return null;
        }
        return this;
    }

    /// <summary>
    /// Gets an <see cref="Enum"/> object representation from this <see cref="StringValue"/>, parsing the current string expression into an value of
    /// <typeparamref name="TEnum"/>. This method will throw an <see cref="NullReferenceException"/> if
    /// the value stored in this instance is null.
    /// </summary>
    /// <typeparam name="TEnum">The <see cref="Enum"/> type.</typeparam>
    public TEnum GetEnum<TEnum>() where TEnum : struct, Enum
    {
        this.ThrowIfNull();
        return Enum.Parse<TEnum>(this._ref!, true);
    }

    /// <summary>
    /// Gets a non-null string from this <see cref="StringValue"/>. This method will throw an <see cref="NullReferenceException"/> if
    /// the value stored in this instance is null.
    /// </summary>
    /// <returns>An non-null string value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    public string GetString()
    {
        this.ThrowIfNull();
        return this._ref!;
    }

    /// <summary>
    /// Gets a <see cref="char"/> from this <see cref="StringValue"/>. This method will throw an <see cref="NullReferenceException"/> if
    /// the value stored in this instance is null.
    /// </summary>
    /// <returns>An non-null char value.</returns>
    public char GetChar()
    {
        this.ThrowIfNull();
        if (this._ref!.Length != 1)
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "char"));

        return this._ref[0];
    }

    /// <summary>
    /// Gets a <see cref="Int32"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <returns>An non-null Int32 value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
    public int GetInteger()
    {
        this.ThrowIfNull();
        try
        {
            return int.Parse(this._ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "integer"));
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
        this.ThrowIfNull();
        try
        {
            return byte.Parse(this._ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "byte"));
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
        this.ThrowIfNull();
        try
        {
            return long.Parse(this._ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "long"));
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
        this.ThrowIfNull();
        try
        {
            return short.Parse(this._ref!, fmtProvider);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "short"));
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
        this.ThrowIfNull();
        try
        {
            return double.Parse(this._ref!, fmtProvider);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "double"));
        }
    }

    /// <summary>
    /// Gets a <see cref="float"/> from this <see cref="StringValue"/>.
    /// </summary>
    /// <param name="fmtProvider">Optional. Specifies the culture-specific format information.</param>
    /// <returns>An non-null double value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    /// <exception cref="FormatException">Thrown when the value stored in this instance is not parseable to the desired type.</exception>
    public double GetSingle(IFormatProvider? fmtProvider = null)
    {
        this.ThrowIfNull();
        try
        {
            return float.Parse(this._ref!, fmtProvider);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "float"));
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
        this.ThrowIfNull();
        try
        {
            return bool.Parse(this._ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "boolean"));
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
        this.ThrowIfNull();
        try
        {
            return DateTime.Parse(this._ref!, fmtProvider);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "DateTime"));
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
        this.ThrowIfNull();
        try
        {
            return Guid.Parse(this._ref!);
        }
        catch (Exception ex) when (ex is FormatException || ex is InvalidCastException)
        {
            throw new FormatException(string.Format(SR.ValueItem_CastException, this._ref, this.argName, "GUID"));
        }
    }

#if NET7_0_OR_GREATER
    /// <summary>
    /// Gets the current value parsed by the provided <see cref="IParsable{TSelf}"/> at <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type where the conversion will result into.</typeparam>
    /// <param name="fmtProvider">Optional. An object that provides culture-specific formatting information about the current value.</param>
    /// <returns>The result of parsing the current string value.</returns>
    public T GetParsable<T>(IFormatProvider? fmtProvider = null) where T : IParsable<T>
    {
        this.ThrowIfNull();
        return T.Parse(this._ref!, fmtProvider);
    }
#endif

    /// <summary>
    /// Gets an not null value from the specified <typeparamref name="T"/>.
    /// </summary>
    /// <typeparam name="T">The type to convert the value to.</typeparam>
    public T Get<T>() where T : struct
    {
        this.ThrowIfNull();
        object result = Internal.Parseable.ParseInternal<T>(this._ref!);
        return Unsafe.Unbox<T>(result);
    }

    void ThrowIfNull()
    {
        if (this.IsNull)
        {
            throw new NullReferenceException(string.Format(SR.ValueItem_ValueNull, this.argName, this.argType));
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
            return this.IsNull;
        }
        else if (obj is StringValue sv)
        {
            return this._ref?.Equals(sv._ref) == true;
        }
        else if (obj is string ss)
        {
            return this._ref?.Equals(ss) == true;
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
        return this._ref?.GetHashCode() ?? 0;
    }

    /// <inheritdoc/>
    public object Clone()
    {
        return new StringValue((string)this.argName.Clone(), (string)this.argType.Clone(), (string?)this._ref?.Clone());
    }

    /// <inheritdoc/>
    /// <exclude/>
    public override string? ToString()
    {
        return this._ref?.ToString();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Equals(StringValue other)
    {
        return this.Equals(other);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public int CompareTo(StringValue other)
    {
        return string.Compare(this._ref, other._ref);
    }
}
