// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ValueResult.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Core.Routing;

/// <summary>
/// Represents a mutable type for boxing objects by value or reference in a response from a router.
/// </summary>
/// <typeparam name="T">The type of object to be boxed.</typeparam>
public class ValueResult<T>
{
    // <exclude />
    // <inheritdocs />
    private ValueResult()
    {
    }

    /// <summary>
    /// Implicitly gets the <typeparamref name="T"/> value from a given <see cref="ValueResult{T}"/> instance.
    /// </summary>
    /// <param name="box">The input <see cref="ValueResult{T}"/> instance.</param>
    /// <exclude/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator T(ValueResult<T> box)
    {
        return (T)(object)box;
    }

    /// <summary>
    /// Implicitly creates a new <see cref="ValueResult{T}"/> instance from a given <typeparamref name="T"/> value.
    /// </summary>
    /// <param name="value">The input <typeparamref name="T"/> value to wrap.</param>
    /// <exclude/>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static implicit operator ValueResult<T>(T value)
    {
        if (value == null) throw new NullReferenceException(SR.ValueResult_Null);
        return Unsafe.As<ValueResult<T>>(value)!;
    }
}