// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SizeHelper.cs
// Repository:  https://github.com/sisk-http/core

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text.RegularExpressions;

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides useful size-dedicated helper members.
/// </summary>
public sealed class SizeHelper {

    static readonly Regex parserRegex = new Regex ( @"^\s*([\d,\.]+)\s*([kmgtpe]?b?)?\s*$", RegexOptions.IgnoreCase | RegexOptions.Compiled );

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
    /// Parses a human-readable size string (e.g., "10 KB", "2.5 MB") into a long representing the number of bytes.
    /// </summary>
    /// <param name="humanReadableSize">The human-readable size string to parse.</param>
    /// <returns>The size in bytes, represented as a long.</returns>
    /// <exception cref="ArgumentNullException">Thrown if <paramref name="humanReadableSize"/> is null or whitespace.</exception>
    /// <exception cref="ArgumentException">Thrown if <paramref name="humanReadableSize"/> is not in a valid format.</exception>
    public static long Parse ( string humanReadableSize ) {
        ArgumentException.ThrowIfNullOrWhiteSpace ( humanReadableSize, nameof ( humanReadableSize ) );

        var match = parserRegex.Match ( humanReadableSize );

        if (!match.Success) {
            throw new ArgumentException ( SR.SizeHelper_InvalidParsingString, nameof ( humanReadableSize ) );
        }

        var numberString = match.Groups [ 1 ].Value;
        var unitString = match.Groups [ 2 ].Value.ToLowerInvariant ();

        numberString = numberString.Replace ( " ", null );

        if (!double.TryParse ( numberString, CultureInfo.InvariantCulture, out double number )) {
            throw new ArgumentException ( SR.SizeHelper_InvalidParsingString, nameof ( humanReadableSize ) );
        }

        switch (unitString) {
            case "k":
            case "kb":
                return (long) (number * UnitKb);
            case "m":
            case "mb":
                return (long) (number * UnitMb);
            case "g":
            case "gb":
                return (long) (number * UnitGb);
            case "t":
            case "tb":
                return (long) (number * UnitTb);
            case "p":
            case "pb":
                return (long) (number * UnitPb);
            case "e":
            case "eb":
                return (long) (number * UnitEb);
        }

        return (long) number;
    }

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
