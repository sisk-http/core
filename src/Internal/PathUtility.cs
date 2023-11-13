// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
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
}
