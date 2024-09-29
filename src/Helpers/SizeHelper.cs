// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SizeHelper.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides useful size-dedicated helper members.
/// </summary>
public sealed class SizeHelper
{
    /// <summary>
    /// Represents the number of bytes in one kilobyte (KB).
    /// </summary>
    public const long UnitKb = 1024;

    /// <summary>
    /// Represents the number of bytes in one megabyte (MB).
    /// This is calculated as 1024 kilobytes.
    /// </summary>
    public const long UnitMb = UnitKb * 1024;

    /// <summary>
    /// Represents the number of bytes in one gigabyte (GB).
    /// This is calculated as 1024 megabytes.
    /// </summary>
    public const long UnitGb = UnitMb * 1024;

    /// <summary>
    /// Represents the number of bytes in one terabyte (TB).
    /// This is calculated as 1024 gigabytes.
    /// </summary>
    public const long UnitTb = UnitGb * 1024;

    /// <summary>
    /// Represents the number of bytes in one exabyte (TB).
    /// This is calculated as 1024 terabytes.
    /// </summary>
    public const long UnitEb = UnitTb * 1024;

    /// <summary>
    /// Converts a byte count into a human-readable string representation.
    /// </summary>
    /// <param name="byteCount">The total number of bytes to convert.</param>
    /// <returns>A string representing the byte count in a human-readable format.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string HumanReadableSize(long byteCount) => HumanReadableSize((double)byteCount);

    /// <summary>
    /// Converts a byte count into a human-readable string representation.
    /// </summary>
    /// <param name="byteCount">The total number of bytes to convert.</param>
    /// <returns>A string representing the byte count in a human-readable format.</returns>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string HumanReadableSize(double byteCount)
    {
        if (byteCount < UnitKb)
        {
            return $"{byteCount:n2} bytes";
        }
        else if (byteCount > UnitKb && byteCount <= UnitMb)
        {
            return $"{byteCount / UnitKb:n2} KB";
        }
        else if (byteCount > UnitMb && byteCount <= UnitGb)
        {
            return $"{byteCount / UnitMb:n2} MB";
        }
        else if (byteCount > UnitGb && byteCount <= UnitTb)
        {
            return $"{byteCount / UnitGb:n2} GB";
        }
        else if (byteCount > UnitTb)
        {
            return $"{byteCount / UnitTb:n2} TB";
        }
        else
        {
            return $"{byteCount / UnitEb:n2} EB";
        }
    }
}
