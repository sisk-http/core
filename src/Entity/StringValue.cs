// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   StringValue.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Entity;

/// <summary>
/// Represents an option/monad item that wraps an string value and allows conversion to most common types.
/// </summary>
public readonly struct StringValue : ICloneable, IEquatable<StringValue>, IComparable<StringValue> {
    private readonly string? _ref;
    private readonly string argName;
    private readonly string argType;

    internal StringValue ( string name, string type, string? data ) {
        _ref = data;
        argName = name;
        argType = type;
    }

    /// <summary>
    /// Creates an new <see cref="StringValue"/> from the specified string.
    /// </summary>
    /// <param name="value">The string value.</param>
    public static StringValue Create ( string? value ) {
        return new StringValue ( "StringValue", value );
    }

    /// <summary>
    /// Creates an new empty value of the <see cref="StringValue"/> with no predefined value.
    /// </summary>
    /// <param name="name">The <see cref="StringValue"/> name.</param>
    public StringValue ( string name ) {
        _ref = null;
        argName = name;
        argType = "StringValue";
    }

    /// <summary>
    /// Creates an new value of the <see cref="StringValue"/>.
    /// </summary>
    /// <param name="name">The <see cref="StringValue"/> name.</param>
    /// <param name="value">The <see cref="StringValue"/> value.</param>
    public StringValue ( string name, string? value ) {
        _ref = value;
        argName = name;
        argType = "StringValue";
    }

    /// <summary>
    /// Gets the name of the property that hosts this <see cref="StringValue"/>.
    /// </summary>
    public string Name { get => argName; }

    /// <summary>
    /// Gets the value of the current <see cref="StringValue"/> string if it has been assigned a valid underlying value.
    /// </summary>
    public string? Value { get => _ref; }

    /// <summary>
    /// Gets an boolean indicating if this object value is null or an empty string.
    /// </summary>
    public bool IsNullOrEmpty { get => string.IsNullOrEmpty ( _ref ); }

    /// <summary>
    /// Gets an boolean indicating if this object value is null.
    /// </summary>
    public bool IsNull { get => _ref is null; }

    /// <summary>
    /// Returns a self-reference to this object when it's value is not null.
    /// </summary>
    public StringValue? MaybeNull () {
        if (IsNull) {
            return null;
        }
        return this;
    }

    /// <summary>
    /// Returns a self-reference to this object when it's value is not null or an empty string.
    /// </summary>
    public StringValue? MaybeNullOrEmpty () {
        if (IsNullOrEmpty) {
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
    public TEnum GetEnum<TEnum> () where TEnum : struct, Enum {
        return Enum.Parse<TEnum> ( GetString (), true );
    }

    /// <summary>
    /// Gets a non-null string from this <see cref="StringValue"/>. This method will throw an <see cref="NullReferenceException"/> if
    /// the value stored in this instance is null.
    /// </summary>
    /// <returns>An non-null string value.</returns>
    /// <exception cref="NullReferenceException">Thrown when the value stored in this instance is null.</exception>
    public string GetString () {
        if (_ref is null)
            throw new InvalidOperationException ( SR.Format ( SR.ValueItem_ValueNull, argName, argType ) );
        return _ref;
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="char"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <returns>The converted <see cref="char"/>.</returns>
    public char GetChar () {
        string r = GetString ();
        if (r.Length != 1)
            throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, "char" ) );

        return r [ 0 ];
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as an <see cref="int"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="int"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to an <see cref="int"/>.</exception>
    public int GetInteger ( IFormatProvider? formatProvider = null ) {
        if (Int32.TryParse ( GetString (), formatProvider, out Int32 result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Int32 ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="byte"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="byte"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="byte"/>.</exception>
    public int GetByte ( IFormatProvider? formatProvider = null ) {
        if (Byte.TryParse ( GetString (), formatProvider, out Byte result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Byte ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="long"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="long"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="long"/>.</exception>
    public long GetLong ( IFormatProvider? formatProvider = null ) {
        if (Int64.TryParse ( GetString (), formatProvider, out Int64 result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Int64 ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="short"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="short"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="short"/>.</exception>
    public short GetShort ( IFormatProvider? formatProvider = null ) {
        if (Int16.TryParse ( GetString (), formatProvider, out Int16 result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Int16 ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="double"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="double"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="double"/>.</exception>
    public double GetDouble ( IFormatProvider? formatProvider = null ) {
        if (Double.TryParse ( GetString (), formatProvider, out Double result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Double ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="float"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="float"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="float"/>.</exception>
    public float GetSingle ( IFormatProvider? formatProvider = null ) {
        if (Single.TryParse ( GetString (), formatProvider, out Single result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Single ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="decimal"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="decimal"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="decimal"/>.</exception>
    public decimal GetDecimal ( IFormatProvider? formatProvider = null ) {
        if (Decimal.TryParse ( GetString (), formatProvider, out Decimal result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Decimal ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="bool"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <returns>The converted <see cref="bool"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="bool"/>.</exception>
    public bool GetBoolean () {
        if (Boolean.TryParse ( GetString (), out Boolean result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Boolean ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="DateTime"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="DateTime"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="DateTime"/>.</exception>
    public DateTime GetDateTime ( IFormatProvider? formatProvider = null ) {
        if (DateTime.TryParse ( GetString (), formatProvider, out DateTime result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( DateTime ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a <see cref="Guid"/>. Throws an exception
    /// if the value couldn't be parsed to the target type.
    /// </summary>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted <see cref="Guid"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to a <see cref="Guid"/>.</exception>
    public Guid GetGuid ( IFormatProvider? formatProvider = null ) {
        if (Guid.TryParse ( GetString (), formatProvider, out Guid result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, nameof ( Guid ) ) );
    }

    /// <summary>
    /// Parses the value contained in this <see cref="StringValue"/> as a type <typeparamref name="T"/> that implements <see cref="IParsable{T}"/>.
    /// Throws an exception if the value couldn't be parsed to the target type.
    /// </summary>
    /// <typeparam name="T">The type to parse the value to.</typeparam>
    /// <param name="formatProvider">The <see cref="IFormatProvider"/> to use for parsing. Defaults to <see langword="null"/>.</param>
    /// <returns>The converted value of type <typeparamref name="T"/>.</returns>
    /// <exception cref="FormatException">Thrown when the value cannot be parsed to type <typeparamref name="T"/>.</exception>
    public T Get<T> ( IFormatProvider? formatProvider = null ) where T : IParsable<T> {
        if (T.TryParse ( GetString (), formatProvider, out T? result )) {
            return result;
        }
        throw new FormatException ( SR.Format ( SR.ValueItem_CastException, _ref, argName, typeof ( T ).Name ) );
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static implicit operator string? ( StringValue i ) => i._ref;

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator == ( StringValue i, object? other ) {
        return i.Equals ( other );
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator != ( StringValue i, object? other ) {
        return !(i == other);
    }

    /// <inheritdoc/>
    /// <exclude/>
    public override bool Equals ( object? obj ) {
        if (obj is null) {
            return IsNull;
        }
        else if (obj is StringValue sv) {
            return _ref?.Equals ( sv._ref, StringComparison.Ordinal ) == true;
        }
        else if (obj is string ss) {
            return _ref?.Equals ( ss, StringComparison.Ordinal ) == true;
        }
        else {
            return false;
        }
    }

    /// <inheritdoc/>
    /// <exclude/>
    public override int GetHashCode () {
        return _ref?.GetHashCode ( StringComparison.Ordinal ) ?? 0;
    }

    /// <inheritdoc/>
    public object Clone () {
        return new StringValue ( (string) argName.Clone (), (string) argType.Clone (), (string?) _ref?.Clone () );
    }

    /// <inheritdoc/>
    /// <exclude/>
    public override string? ToString () {
        return _ref?.ToString ();
    }

    /// <inheritdoc/>
    /// <exclude/>
    public bool Equals ( StringValue other ) {
        return Equals ( other );
    }

    /// <inheritdoc/>
    /// <exclude/>
    public int CompareTo ( StringValue other ) {
        return string.Compare ( _ref, other._ref, StringComparison.Ordinal );
    }

    /// <summary>
    /// Compares the current object with another object of the same type, using the specified string comparison.
    /// </summary>
    /// <param name="other">The object to compare with the current object.</param>
    /// <param name="stringComparison">One of the <see cref="StringComparison"/> values that specifies the comparison rules to use.</param>
    /// <returns>A value that indicates the relative order of the objects being compared.</returns>
    /// <seealso cref="StringComparison"/>
    public int CompareTo ( StringValue other, in StringComparison stringComparison ) {
        return string.Compare ( _ref, other._ref, stringComparison );
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator < ( StringValue left, StringValue right ) {
        return left.CompareTo ( right ) < 0;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator <= ( StringValue left, StringValue right ) {
        return left.CompareTo ( right ) <= 0;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator > ( StringValue left, StringValue right ) {
        return left.CompareTo ( right ) > 0;
    }

    /// <inheritdoc/>
    /// <exclude/>
    public static bool operator >= ( StringValue left, StringValue right ) {
        return left.CompareTo ( right ) >= 0;
    }
}
