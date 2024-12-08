// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
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
public sealed class PathHelper
{
    /// <summary>
    /// Combines the specified URL paths into one.
    /// </summary>
    /// <param name="paths">The string array which contains parts that will be combined.</param>
    public static string CombinePaths(params string[] paths)
    {
        return PathUtility.CombinePaths(paths);
    }
    
    /// <summary>
    /// Normalizes and combines the specified file-system paths into one.
    /// </summary>
    /// <param name="allowRelativeReturn">Specifies if relative paths should be merged and ".." returns should be respected.</param>
    /// <param name="separator">Specifies the path separator character.</param>
    /// <param name="paths">Specifies the array of paths to combine.</param>
    public static string FilesystemCombinePaths(bool allowRelativeReturn, char separator, params string[] paths)
    {
        return PathUtility.NormalizedCombine(allowRelativeReturn, separator, paths);
    }

    /// <summary>
    /// Normalizes and combines the specified file-system paths into one.
    /// </summary>
    /// <param name="allowRelativeReturn">Specifies if relative paths should be merged and ".." returns should be respected.</param>
    /// <param name="separator">Specifies the path separator character.</param>
    /// <param name="paths">Specifies the array of paths to combine.</param>
    public static string FilesystemCombinePaths(bool allowRelativeReturn, char separator, ReadOnlySpan<string> paths)
    {
        return PathUtility.NormalizedCombine(allowRelativeReturn, separator, paths);
    }

    /// <summary>
    /// Normalizes and combines the specified file-system paths into one, using the default environment directory separator char.
    /// </summary>
    /// <param name="paths">Specifies the array of paths to combine.</param>
    public static string FilesystemCombinePaths(params string[] paths)
    {
        return PathUtility.NormalizedCombine(false, Path.DirectorySeparatorChar, paths);
    }

    /// <summary>
    /// Normalize the given path to use the specified directory separator, trim the last separator and
    /// remove empty entries.
    /// </summary>
    /// <param name="path">The path to normalize.</param>
    /// <param name="directorySeparator">The directory separator.</param>
    public static string NormalizePath(string path, char directorySeparator = '/')
    {
        string[] parts = path.Split(new char[] { '/', '\\' }, StringSplitOptions.RemoveEmptyEntries);
        string result = string.Join(directorySeparator, parts);
        if (path.StartsWith('/') || path.StartsWith('\\'))
            result = directorySeparator + result;
        return result;
    }
}
