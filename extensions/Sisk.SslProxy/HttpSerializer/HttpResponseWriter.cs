// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseWriter.cs
// Repository:  https://github.com/sisk-http/core


// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseWriter.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using Sisk.Core.Http;

namespace Sisk.Ssl.HttpSerializer;

static class HttpResponseWriter
{
    public static List<(string, string)> GetDefaultHeaders()
    {
        return [
            ("Server", $"Sisk/{HttpServer.SiskVersion.Major}.{HttpServer.SiskVersion.Minor}"),
            ("Date", DateTime.Now.ToUniversalTime().ToString("r"))
        ];
    }

    public static bool TryWriteHttp1Response(
        int clientId,
        Stream outgoingStream,
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
        catch (Exception ex)
        {
            Logger.LogInformation($"#{clientId}: Couldn't write HTTP response to {outgoingStream.GetType().Name}: {ex.Message}");
            return false;
        }
    }

    public static void WriteHttp1DefaultResponse(HttpStatusInformation status, string statusDescription, Stream outgoingStream)
    {
        var html = DefaultMessagePage.CreateDefaultPageHtml(status.Description, statusDescription);
        var htmlBytes = Encoding.UTF8.GetBytes(html);
        var headers = GetDefaultHeaders();
        headers.Add(("Content-Length", htmlBytes.Length.ToString()));
        headers.Add(("Content-Type", "text/html; charset=utf-8"));

        if (TryWriteHttp1Response(0, outgoingStream, status.StatusCode.ToString(), status.Description, headers))
        {
            outgoingStream.Write(htmlBytes);
        }
        outgoingStream.Flush();
    }
}
