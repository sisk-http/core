// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseReader.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.Ssl.HttpSerializer;

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
    public static bool TryReadHttp1Response(
                            int clientId,
                            Stream inboundStream,
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
            ReadOnlySpan<byte> _proto = SerializerUtils.ReadUntil(memory.ProtocolBuffer, inboundStream, Constants.CH_SPACE);
            if (_proto.Length == 0) goto ret;
            _ = SerializerUtils.DecodeString(_proto); //protocol

            ReadOnlySpan<byte> _statusCode = SerializerUtils.ReadUntil(memory.StatusCodeBuffer, inboundStream, Constants.CH_SPACE);
            if (_statusCode.Length == 0) goto ret;
            statusCode = SerializerUtils.DecodeString(_statusCode);

            ReadOnlySpan<byte> _reason = SerializerUtils.ReadUntil(memory.StatusReasonBuffer, inboundStream, Constants.CH_RETURN);
            if (_reason.Length == 0) goto ret;
            statusDescription = SerializerUtils.DecodeString(_reason);

            inboundStream.ReadByte(); // \n

            List<(string, string)> headerList = new List<(string, string)>(Constants.HEADER_LINE_ALLOCATION);
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

                ReadOnlySpan<byte> headerName = SerializerUtils.ReadUntil(memory.PsHeaderName, inboundStream, Constants.CH_HSEP);
                if (headerName.Length == 0) goto ret;

                ReadOnlySpan<byte> headerValue = SerializerUtils.ReadUntil(memory.PsHeaderValue, inboundStream, Constants.CH_RETURN);
                if (headerValue.Length == 0) goto ret;

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
        catch (Exception ex)
        {
            Logger.LogInformation($"#{clientId}: Couldn't read HTTP response from {inboundStream.GetType().Name}: {ex.Message}");
        }

    ret:
        statusCode = null;
        statusDescription = null;
        headers = new();
        return false;
    }
}
