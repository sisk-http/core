﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHeaderExtensions.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.ManagedHttpListener;

internal static class HttpHeaderExtensions
{
    public static void Set(this List<(string, string)> headers, (string, string) header)
    {
        for (int i = headers.Count - 1; i >= 0; i--)
        {
            if (StringComparer.OrdinalIgnoreCase.Compare(headers[i].Item1, header.Item1) == 0)
            {
                headers.RemoveAt(i);
            }
        }

        headers.Add(header);
    }
}
