using System.Runtime.CompilerServices;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents a mutable type for boxing objects by value or reference in a response from a router.
/// </summary>
/// <typeparam name="T">The type of object to be boxed.</typeparam>
/// <definition>
/// public sealed class ValueResult{{T}}
/// </definition>
/// <type>
/// Class
/// </type>
/// <since>0.15</since>
public class ValueResult<T>
{
    protected ValueResult()
    {
    }

    /// <summary>
    /// Implicitly gets the <typeparamref name="T"/> value from a given <see cref="ValueResult{T}"/> instance.
    /// </summary>
    /// <param name="box">The input <see cref="ValueResult{T}"/> instance.</param>
    /// <nodocs/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(ValueResult<T> box)
    {
        return (T)(object)box;
    }

    /// <summary>
    /// Implicitly creates a new <see cref="ValueResult{T}"/> instance from a given <typeparamref name="T"/> value.
    /// </summary>
    /// <param name="value">The input <typeparamref name="T"/> value to wrap.</param>
    /// <nodocs/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValueResult<T>(T value)
    {
        if (value == null) throw new NullReferenceException("Router response objects or references cannot be null.");
        return Unsafe.As<ValueResult<T>>(value)!;
    }
}