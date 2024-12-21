// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
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
public sealed class SizeHelper {
    /// <summary>
    /// Represents the number of bytes in one kibibyte (KiB).
    /// This is calculated as 1024 bytes.
    /// </summary>
    public const long UnitKb = 1024;

    /// <summary>
    /// Represents the number of bytes in one mebibyte (MiB).
    /// This is calculated as 1024 kibibytes.
    /// </summary>
    public const long UnitMb = 1_048_576;

    /// <summary>
    /// Represents the number of bytes in one gibibyte (GiB).
    /// This is calculated as 1024 mebibytes.
    /// </summary>
    public const long UnitGb = 1_073_741_824;

    /// <summary>
    /// Represents the number of bytes in one tebibyte (TiB).
    /// This is calculated as 1024 gibibytes.
    /// </summary>
    public const long UnitTb = 1_099_511_627_776;

    /// <summary>
    /// Represents the number of bytes in one pebibyte (PiB).
    /// This is calculated as 1024 tebibytes.
    /// </summary>
    public const long UnitPb = 1_125_899_906_842_624;

    /// <summary>
    /// Represents the number of bytes in one exibibyte (EiB).
    /// This is calculated as 1024 pebibytes.
    /// </summary>
    public const long UnitEb = 1_152_921_504_606_846_976;

    /// <summary>
    /// Converts a byte count into a human-readable string representation.
    /// </summary>
    /// <param name="byteCount">The total number of bytes to convert.</param>
    /// <returns>A string representing the byte count in a human-readable format.</returns>
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string HumanReadableSize ( long byteCount ) => HumanReadableSize ( (double) byteCount );

    /// <summary>
    /// Converts a byte count into a human-readable string representation.
    /// </summary>
    /// <param name="byteCount">The total number of bytes to convert.</param>
    /// <returns>A string representing the byte count in a human-readable format.</returns>
    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string HumanReadableSize ( double byteCount ) {
        if (byteCount < UnitKb) {
            return $"{byteCount:n0} bytes";
        }
        else if (byteCount > UnitKb && byteCount <= UnitMb) {
            return $"{byteCount / UnitKb:n2} KB";
        }
        else if (byteCount > UnitMb && byteCount <= UnitGb) {
            return $"{byteCount / UnitMb:n2} MB";
        }
        else if (byteCount > UnitGb && byteCount <= UnitTb) {
            return $"{byteCount / UnitGb:n2} GB";
        }
        else if (byteCount > UnitTb && byteCount <= UnitPb) {
            return $"{byteCount / UnitTb:n2} TB";
        }
        else if (byteCount > UnitPb && byteCount <= UnitEb) {
            return $"{byteCount / UnitTb:n2} PB";
        }
        else {
            return $"{byteCount / UnitEb:n2} EB";
        }
    }
}
