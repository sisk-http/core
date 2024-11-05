// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PathUtility.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Core.Internal;

internal static class PathUtility
{
    public static bool IsFullyRootedUri(string uri)
    {
        return uri.StartsWith("http://")
            || uri.StartsWith("https://")
            || uri.StartsWith("ws://")
            || uri.StartsWith("wss://");
    }

    public static string CombinePaths(params string[] paths)
    {
        StringBuilder sb = new StringBuilder();
        bool hadFullyRootedUri = false;

        foreach (string part in paths)
        {
            if (IsFullyRootedUri(part))
            {
                hadFullyRootedUri = true;
                sb.Clear();
            }

            if (sb.Length > 0 && !part.StartsWith('?'))
                sb.Append('/');

            sb.Append(part.Trim('/'));
        }

        if (!hadFullyRootedUri)
            sb.Insert(0, '/');

        return sb.ToString();
    }

    public static string NormalizedCombine(bool allowReturn, char environmentPathChar, ReadOnlySpan<string> paths)
    {
        if (paths.Length == 0) return "";

        bool startsWithSepChar = paths[0].StartsWith('/') || paths[0].StartsWith('\\');
        List<string> tokens = new List<string>();

        for (int ip = 0; ip < paths.Length; ip++)
        {
            string path = paths[ip]
                ?? throw new ArgumentNullException($"The path string at index {ip} is null.");

            string normalizedPath = path
                .Replace('/', environmentPathChar)
                .Replace('\\', environmentPathChar)
                .Trim(environmentPathChar);

            string[] pathIdentities = normalizedPath.Split(
                environmentPathChar,
                StringSplitOptions.RemoveEmptyEntries
            );

            tokens.AddRange(pathIdentities);
        }

        Stack<int> insertedIndexes = new Stack<int>();
        StringBuilder pathBuilder = new StringBuilder();
        foreach (string token in tokens)
        {
            if (token == ".")
            {
                continue;
            }
            else if (token == "..")
            {
                if (allowReturn)
                {
                    pathBuilder.Length = insertedIndexes.Pop();
                }
            }
            else
            {
                insertedIndexes.Push(pathBuilder.Length);
                pathBuilder.Append(token);
                pathBuilder.Append(environmentPathChar);
            }
        }

        if (startsWithSepChar)
            pathBuilder.Insert(0, environmentPathChar);

        return pathBuilder.ToString().TrimEnd(environmentPathChar);
    }
}
