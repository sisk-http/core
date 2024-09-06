// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestWriter.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Ssl.HttpSerializer;

static class HttpRequestWriter
{
    public static bool TryWriteHttpV1Request(
        int clientId,
        Stream outboundStream,
        string method,
        string path,
        List<(string, string)> headers,
        int contentLength)
    {
        try
        {
            using var sw = new StringWriter() { NewLine = "\r\n" };
            sw.WriteLine($"{method} {path} HTTP/1.1");
            for (int i = 0; i < headers.Count; i++)
            {
                (string, string) header = headers[i];
                sw.WriteLine($"{header.Item1}: {header.Item2}");
            }
            sw.WriteLine();

            byte[] headerBytes = Encoding.UTF8.GetBytes(sw.ToString());
            outboundStream.Write(headerBytes);
            outboundStream.Flush();

            return true;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"#{clientId}: Couldn't write HTTP request to {outboundStream.GetType().Name}: {ex.Message}");
            return false;
        }
    }
}
