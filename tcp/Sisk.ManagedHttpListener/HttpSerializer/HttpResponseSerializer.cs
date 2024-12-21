// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseSerializer.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.ManagedHttpListener.HttpSerializer;

internal static class HttpResponseSerializer {
    public static bool WriteHttpResponseHeaders ( Stream outgoingStream, int statusCode, string statusDescription, List<(string, string)> headers ) {
        try {
            string protocolLine = $"HTTP/1.1 {statusCode} {statusDescription}\r\n";
            outgoingStream.Write ( Encoding.ASCII.GetBytes ( protocolLine ) );

            var headerCount = headers.Count;
            for (int i = 0; i < headerCount; i++) {
                var header = headers [ i ];
                string headerLine = $"{header.Item1}: {header.Item2}\r\n";
                outgoingStream.Write ( Encoding.UTF8.GetBytes ( headerLine ) );
            }

            outgoingStream.Write ( Encoding.ASCII.GetBytes ( "\r\n" ) );
            return true;
        }
        catch {
            return false;
        }
    }
}
