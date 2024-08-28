// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseReader.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Ssl;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.SslProxy;

static class HttpResponseReader
{
    public static bool TryReadHttp1Response(Stream inboundStream,
        [NotNullWhen(true)] out string? statusCode,
        [NotNullWhen(true)] out string? statusDescription,
                            out List<(string, string)> headers,
                            out int contentLength,
                            out bool isChunkedEncoding,
                            out bool isConnectionKeepAlive)
    {
        contentLength = 0;
        isChunkedEncoding = false;
        isConnectionKeepAlive = true;
        try
        {
            byte[] _proto = SerializerUtils.ReadUntil(inboundStream, Constants.CH_SPACE, 16);
            _ = SerializerUtils.DecodeString(_proto); //protocol

            byte[] _statusCode = SerializerUtils.ReadUntil(inboundStream, Constants.CH_SPACE, 4);
            statusCode = SerializerUtils.DecodeString(_statusCode);

            byte[] _reason = SerializerUtils.ReadUntil(inboundStream, Constants.CH_RETURN, 512);
            statusDescription = SerializerUtils.DecodeString(_reason);

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

                byte[] headerName = SerializerUtils.ReadUntil(inboundStream, Constants.CH_HSEP, 256);
                byte[] headerValue = SerializerUtils.ReadUntil(inboundStream, Constants.CH_RETURN, 2048);

                inboundStream.ReadByte(); // \n

                bool forwardHeader = true;
                string hName = firstReadChar + SerializerUtils.DecodeString(headerName).Trim();
                string hValue = SerializerUtils.DecodeString(headerValue).TrimStart();

                if (string.Compare(hName, HttpKnownHeaderNames.ContentLength, true) == 0)
                {
                    contentLength = int.Parse(hValue);
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.TransferEncoding, true) == 0
                         && hValue == "chunked")
                {
                    isChunkedEncoding = true;
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.Connection, true) == 0
                         && hValue == "close")
                {
                    forwardHeader = false;
                    isConnectionKeepAlive = false;
                }

                if (forwardHeader)
                    headerList.Add((hName, hValue));
            }

            headers = headerList;
            return true;
        }
        catch
        {
            statusCode = null;
            statusDescription = null;
            headers = new();
            return false;
        }
    }
}
