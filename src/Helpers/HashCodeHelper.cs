// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HashCodeHelper.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides helper methods for deterministic hash code generation and combination.
/// </summary>
public static class HashCodeHelper {

    /// <summary>
    /// The offset basis used by the FNV‑1a algorithm.
    /// </summary>
    public const ulong FnvHash = 14695981039346656037UL;

    /// <summary>
    /// The prime multiplier used by the FNV‑1a algorithm.
    /// </summary>
    public const ulong FnvPrime = 1099511628211UL;

    /// <summary>
    /// Computes a deterministic 64‑bit hash for the supplied <see cref="ReadOnlySpan{T}"/> of characters
    /// using the specified seed, prime, and step values.
    /// </summary>
    /// <param name="s">The span of characters to hash.</param>
    /// <param name="eseed">The initial seed value for the hash algorithm.</param>
    /// <param name="eprime">The prime multiplier used in the hash algorithm.</param>
    /// <param name="step">The step increment for iterating over the span.</param>
    /// <returns>A 64‑bit hash value representing the input characters.</returns>
    public static ulong DeterministicHash ( ReadOnlySpan<char> s, ulong eseed, ulong eprime, int step ) {
        ulong hash = eseed;
        int len = s.Length;
        for (int i = 0; i < len; i += step) {
            hash ^= s [ i ];
            hash *= eprime;
        }
        return hash;
    }

    /// <summary>
    /// Computes a deterministic 64‑bit hash for the supplied <see cref="ReadOnlySpan{T}"/> of characters.
    /// </summary>
    /// <param name="s">The span of characters to hash.</param>
    /// <returns>A 64‑bit hash value representing the input characters.</returns>
    public static ulong DeterministicHash ( ReadOnlySpan<char> s ) {
        return DeterministicHash ( s, FnvHash, FnvPrime, 1 );
    }

    /// <summary>
    /// Computes a deterministic 64‑bit hash for the supplied <see cref="string"/>.
    /// </summary>
    /// <param name="s">The string to hash. May be <see langword="null"/> or empty.</param>
    /// <returns>
    /// The hash of the string, or <c>0</c> if <paramref name="s"/> is <see langword="null"/> or empty.
    /// </returns>
    public static ulong DeterministicHash ( string? s ) {
        if (s is null || s.Length == 0)
            return 0;

        return DeterministicHash ( s.AsSpan () );
    }

    /// <summary>
    /// Computes a deterministic 64‑bit hash for an arbitrary <see cref="object"/>.
    /// </summary>
    /// <param name="data">The object to hash. May be <see langword="null"/>.</param>
    /// <returns>
    /// If <paramref name="data"/> is <see langword="null"/>, returns <c>0</c>;
    /// if it is a <see cref="string"/>, returns the hash of that string;
    /// otherwise returns the 64‑bit representation of <see cref="object.GetHashCode"/>.
    /// </returns>
    public static ulong DeterministicHash ( object? data ) {
        if (data is null)
            return 0;
        else if (data is string s)
            return DeterministicHash ( s );
        else
            return (ulong) data.GetHashCode ();
    }

    /// <summary>
    /// Combines the deterministic hash codes of a collection of objects into a single 64‑bit hash.
    /// </summary>
    /// <param name="objects">
    /// The objects whose hash codes should be combined. May contain <see langword="null"/> entries.
    /// </param>
    /// <returns>A combined 64‑bit hash value representing the supplied objects.</returns>
#if NET10_0_OR_GREATER

    public static ulong Combine ( params ReadOnlySpan<object?> objects ) {
#else
    public static ulong Combine ( params object? [] objects ) {
#endif
        if (objects.Length == 0)
            return 0;

        ulong hash = FnvHash;
        for (int i = 0; i < objects.Length; i++) {
            ulong objHash = DeterministicHash ( objects [ i ] );
            hash ^= objHash;
            hash *= FnvPrime;
        }
        return hash;
    }
}