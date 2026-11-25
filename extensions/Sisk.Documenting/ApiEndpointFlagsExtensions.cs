// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiEndpointFlagsExtensions.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Documenting;

/// <summary>
/// Provides extension methods for <see cref="ApiEndpointFlags"/> values.
/// </summary>
public static class ApiEndpointFlagsExtensions {

    static readonly ApiEndpointFlags [] DefinedFlags = [
        ApiEndpointFlags.Beta,
        ApiEndpointFlags.Experimental,
        ApiEndpointFlags.Preview,
        ApiEndpointFlags.Deprecated,
        ApiEndpointFlags.Internal
    ];

    /// <summary>
    /// Gets the individual flags that compose the specified <see cref="ApiEndpointFlags"/> value,
    /// in declaration order.
    /// </summary>
    /// <param name="flags">The flags value to decompose.</param>
    /// <returns>An array containing each defined flag present in <paramref name="flags"/>.</returns>
    public static ApiEndpointFlags [] GetDefinedFlags ( this ApiEndpointFlags flags ) {
        return DefinedFlags.Where ( flag => ( flags & flag ) == flag ).ToArray ();
    }
}