// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseWriter.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;

namespace Sisk.Ssl;

static class HttpResponseWriter
{
    public static List<(string, string)> GetDefaultHeaders()
    {
        return [
            ("Server", $"Sisk/{HttpServer.SiskVersion.Major}.{HttpServer.SiskVersion.Minor}"),
            ("Date", DateTime.Now.ToUniversalTime().ToString("r")),
            ("Content-Length", "0"),
        ];
    }

    public static bool TryWriteHttp1Response(Stream outgoingStream,
        string statusCode,
        string statusDescription,
        List<(string, string)> headers)
    {
        try
        {
            using var sw = new StringWriter() { NewLine = "\r\n" };
            sw.WriteLine($"HTTP/1.1 {statusCode} {statusDescription}");

            for (int i = 0; i < headers.Count; i++)
            {
                (string name, string value) header = headers[i];
                if (string.Compare(header.name, HttpKnownHeaderNames.Server, true) == 0)
                {
                    sw.WriteLine($"Server: {Constants.Server}");
                }
                else
                {
                    sw.WriteLine($"{header.name}: {header.value}");
                }
            }

            sw.WriteLine();
            outgoingStream.Write(SerializerUtils.EncodeString(sw.ToString()));
            return true;
        }
        catch
        {
            return false;
        }
    }
}
