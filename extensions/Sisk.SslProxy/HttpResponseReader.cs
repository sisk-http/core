// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using Sisk.Core.Http;

namespace Sisk.Ssl;

ref struct HttpResponseReaderSpan
{
    public required Span<byte> ProtocolBuffer;
    public required Span<byte> StatusCodeBuffer;
    public required Span<byte> StatusReasonBuffer;
    public required Span<byte> PsHeaderName;
    public required Span<byte> PsHeaderValue;
}

static class HttpResponseReader
{
    public static bool TryReadHttp1Response(Stream inboundStream,
                     scoped HttpResponseReaderSpan memory,
        [NotNullWhen(true)] out string? statusCode,
        [NotNullWhen(true)] out string? statusDescription,
                            out List<(string, string)> headers,
                            out int contentLength,
                            out bool isChunkedEncoding,
                            out bool isConnectionKeepAlive,
                            out bool isWebSocket)
    {
        contentLength = 0;
        isChunkedEncoding = false;
        isConnectionKeepAlive = true;
        isWebSocket = false;

        try
        {
            Span<byte> _proto = SerializerUtils.ReadUntil(memory.ProtocolBuffer, inboundStream, Constants.CH_SPACE);
            _ = SerializerUtils.DecodeString(_proto); //protocol

            Span<byte> _statusCode = SerializerUtils.ReadUntil(memory.StatusCodeBuffer, inboundStream, Constants.CH_SPACE);
            statusCode = SerializerUtils.DecodeString(_statusCode);

            Span<byte> _reason = SerializerUtils.ReadUntil(memory.StatusReasonBuffer, inboundStream, Constants.CH_RETURN);
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

                Span<byte> headerName = SerializerUtils.ReadUntil(memory.PsHeaderName, inboundStream, Constants.CH_HSEP);
                Span<byte> headerValue = SerializerUtils.ReadUntil(memory.PsHeaderValue, inboundStream, Constants.CH_RETURN);

                inboundStream.ReadByte(); // \n

                bool forwardHeader = true;
                string hName = firstReadChar + SerializerUtils.DecodeString(headerName).Trim();
                string hValue = SerializerUtils.DecodeString(headerValue).TrimStart();

                if (string.Compare(hName, HttpKnownHeaderNames.ContentLength, true) == 0)
                {
                    contentLength = int.Parse(hValue);
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.TransferEncoding, true) == 0 && hValue == "chunked")
                {
                    isChunkedEncoding = true;
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.Connection, true) == 0 && hValue == "close")
                {
                    forwardHeader = false;
                    isConnectionKeepAlive = false;
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.Upgrade, true) == 0 && hValue == "websocket")
                {
                    isWebSocket = true;
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
