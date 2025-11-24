// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PathHelper.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;

namespace Sisk.Core.Helpers;

/// <summary>
/// Provides useful path-dedicated helper members.
/// </summary>
public sealed class PathHelper {
    /// <summary>
    /// Splits the specified path into its individual segments, removing empty entries and trimming whitespace.
    /// </summary>
    /// <param name="path">The path to split.</param>
    /// <returns>An array of path segments.</returns>
    public static string [] Split ( string path ) {
        return path.Split ( [ '/', '\\' ], StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries );
    }

    /// <summary>
    /// Removes the last segment from the specified path.
    /// </summary>
    /// <param name="path">The path to process.</param>
    /// <returns>The path without its final segment, or <see cref="string.Empty"/> if no segments remain.</returns>
    public static string Pop ( string path ) {
        var segments = Split ( path );
        if (segments.Length == 0)
            return string.Empty;
        return string.Join ( '/', segments, 0, segments.Length - 1 );
    }

    /// <summary>
    /// Combines the specified URL paths into one.
    /// </summary>
    /// <param name="paths">The string array which contains parts that will be combined.</param>
    public static string CombinePaths ( params string [] paths ) {
        return PathUtility.CombinePaths ( paths );
    }

    /// <summary>
    /// Normalizes and combines the specified file-system paths into one.
    /// </summary>
    /// <param name="allowRelativeReturn">Specifies if relative paths should be merged and ".." returns should be respected.</param>
    /// <param name="separator">Specifies the path separator character.</param>
    /// <param name="paths">Specifies the array of paths to combine.</param>
    public static string FilesystemCombinePaths ( bool allowRelativeReturn, char separator, params string [] paths ) {
        return PathUtility.NormalizedCombine ( allowRelativeReturn, separator, paths );
    }

    /// <summary>
    /// Normalizes and combines the specified file-system paths into one.
    /// </summary>
    /// <param name="allowRelativeReturn">Specifies if relative paths should be merged and ".." returns should be respected.</param>
    /// <param name="separator">Specifies the path separator character.</param>
    /// <param name="paths">Specifies the array of paths to combine.</param>
    public static string FilesystemCombinePaths ( bool allowRelativeReturn, char separator, ReadOnlySpan<string> paths ) {
        return PathUtility.NormalizedCombine ( allowRelativeReturn, separator, paths );
    }

    /// <summary>
    /// Normalizes and combines the specified file-system paths into one, using the default environment directory separator char.
    /// </summary>
    /// <param name="paths">Specifies the array of paths to combine.</param>
    public static string FilesystemCombinePaths ( params string [] paths ) {
        return PathUtility.NormalizedCombine ( false, Path.DirectorySeparatorChar, paths );
    }

    private static readonly char [] pathNormalizationChars = new char [] { '/', '\\' };

    /// <summary>
    /// Normalize the given path to use the specified directory separator, trim the last separator and
    /// remove empty entries.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <param name="directorySeparator">The directory separator.</param>
    /// <param name="surroundWithDelimiters">
    /// <see langword="true"/> to ensure the result starts and ends with <paramref name="directorySeparator"/>;
    /// otherwise, <see langword="false"/>. Defaults to <see langword="false"/>.
    /// </param>
    public static string NormalizePath ( string path, char directorySeparator = '/', bool surroundWithDelimiters = false ) {
        string [] parts = path.Split ( pathNormalizationChars, StringSplitOptions.RemoveEmptyEntries );
        string result = string.Join ( directorySeparator, parts );
        if (path.StartsWith ( '/' ) || path.StartsWith ( '\\' ))
            result = directorySeparator + result;
        if (surroundWithDelimiters)
            result = directorySeparator + result.Trim ( '/', '\\' ) + directorySeparator;
        return result;
    }
}
