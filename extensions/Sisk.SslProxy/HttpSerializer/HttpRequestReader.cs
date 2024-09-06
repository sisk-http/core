// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;
using Sisk.Core.Http;

namespace Sisk.Ssl.HttpSerializer;

ref struct HttpRequestReaderSpan
{
    public required Span<byte> MethodBuffer;
    public required Span<byte> PathBuffer;
    public required Span<byte> ProtocolBuffer;
    public required Span<byte> PsHeaderName;
    public required Span<byte> PsHeaderValue;
}

static class HttpRequestReader
{
    public static bool TryReadHttp1Request(
                            int clientId,
                            Stream inboundStream,
                      scoped HttpRequestReaderSpan readMemory,
                            string? replaceHostName,
                            TcpClient client,
        [NotNullWhen(true)] out string? method,
        [NotNullWhen(true)] out string? path,
        [NotNullWhen(true)] out string? proto,
                            out int contentLength,
                            out List<(string, string)> headers)
    {
        contentLength = 0;
        try
        {
            Span<byte> _method = SerializerUtils.ReadUntil(readMemory.MethodBuffer, inboundStream, Constants.CH_SPACE);
            method = SerializerUtils.DecodeString(_method);

            Span<byte> _path = SerializerUtils.ReadUntil(readMemory.PathBuffer, inboundStream, Constants.CH_SPACE);
            path = SerializerUtils.DecodeString(_path);

            Span<byte> _protocol = SerializerUtils.ReadUntil(readMemory.ProtocolBuffer, inboundStream, Constants.CH_RETURN);
            proto = SerializerUtils.DecodeString(_protocol);

            inboundStream.ReadByte(); // \n

            List<(string, string)> headerList = new List<(string, string)>();
            while (inboundStream.CanRead)
            {
                char? firstReadChar;
                int c = inboundStream.ReadByte();
                if (c == Constants.CH_RETURN)
                {
                    inboundStream.ReadByte(); // \n
                    break;
                }
                else
                {
                    firstReadChar = (char)c;
                }

                Span<byte> headerName = SerializerUtils.ReadUntil(readMemory.PsHeaderName, inboundStream, Constants.CH_HSEP);
                Span<byte> headerValue = SerializerUtils.ReadUntil(readMemory.PsHeaderValue, inboundStream, Constants.CH_RETURN);

                inboundStream.ReadByte(); // \n

                string hName = firstReadChar + SerializerUtils.DecodeString(headerName).Trim();
                string hValue = SerializerUtils.DecodeString(headerValue).TrimStart();

                if (string.Compare(hName, HttpKnownHeaderNames.ContentLength, true) == 0)
                {
                    contentLength = int.Parse(hValue);
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.Host, true) == 0 && replaceHostName is not null)
                {
                    hValue = replaceHostName;
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.XForwardedFor, true) == 0)
                {
                    string? remoteAddr = (client.Client.RemoteEndPoint as IPEndPoint)?.Address.ToString();
                    if (remoteAddr is not null)
                        hValue = hValue + ", " + remoteAddr;
                }

                headerList.Add((hName, hValue));
            }

            headers = headerList;
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"#{clientId}: Couldn't read HTTP request from {inboundStream.GetType().Name}: {ex.Message}");
            method = null;
            path = null;
            proto = null;
            headers = new();
            return false;
        }
    }


}
