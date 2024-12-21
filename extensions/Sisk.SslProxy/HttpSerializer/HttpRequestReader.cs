// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using Sisk.Core.Http;

namespace Sisk.Ssl.HttpSerializer;

ref struct HttpRequestReaderSpan {
    public required Span<byte> MethodBuffer;
    public required Span<byte> PathBuffer;
    public required Span<byte> ProtocolBuffer;
    public required Span<byte> PsHeaderName;
    public required Span<byte> PsHeaderValue;
}

static class HttpRequestReader {
    public static bool TryReadHttp1Request (
                            int clientId,
                            Stream inboundStream,
                      scoped HttpRequestReaderSpan readMemory,
                            string? replaceHostName,
        [NotNullWhen ( true )] out string? method,
        [NotNullWhen ( true )] out string? path,
        [NotNullWhen ( true )] out string? proto,
                            out string? forwardedFor,
                            out long contentLength,
                            out List<(string, string)> headers,
                            out bool expectContinue ) {
        contentLength = 0;
        expectContinue = false;
        forwardedFor = null;

        try {
            ReadOnlySpan<byte> _method = SerializerUtils.ReadUntil ( readMemory.MethodBuffer, inboundStream, Constants.CH_SPACE );
            if (_method.Length == 0)
                goto ret;
            method = SerializerUtils.DecodeString ( _method );

            ReadOnlySpan<byte> _path = SerializerUtils.ReadUntil ( readMemory.PathBuffer, inboundStream, Constants.CH_SPACE );
            if (_path.Length == 0)
                goto ret;
            path = SerializerUtils.DecodeString ( _path );

            ReadOnlySpan<byte> _protocol = SerializerUtils.ReadUntil ( readMemory.ProtocolBuffer, inboundStream, Constants.CH_RETURN );
            if (_protocol.Length == 0)
                goto ret;
            proto = SerializerUtils.DecodeString ( _protocol );

            inboundStream.ReadByte (); // \n

            List<(string, string)> headerList = new List<(string, string)> ( Constants.HEADER_LINE_ALLOCATION );
            while (inboundStream.CanRead) {
                char? firstReadChar;
                int c = inboundStream.ReadByte ();
                if (c == Constants.CH_RETURN) {
                    inboundStream.ReadByte (); // \n
                    break;
                }
                else {
                    firstReadChar = (char) c;
                }

                bool fwHeader = true;

                ReadOnlySpan<byte> headerName = SerializerUtils.ReadUntil ( readMemory.PsHeaderName, inboundStream, Constants.CH_HSEP );
                if (headerName.Length == 0)
                    goto ret;

                ReadOnlySpan<byte> headerValue = SerializerUtils.ReadUntil ( readMemory.PsHeaderValue, inboundStream, Constants.CH_RETURN );
                if (headerValue.Length == 0)
                    goto ret;

                inboundStream.ReadByte (); // \n

                string hName = firstReadChar + SerializerUtils.DecodeString ( headerName ).Trim ();
                string hValue = SerializerUtils.DecodeString ( headerValue ).TrimStart ();

                if (string.Compare ( hName, HttpKnownHeaderNames.ContentLength, true ) == 0) {
                    contentLength = long.Parse ( hValue );
                }
                else if (string.Compare ( hName, HttpKnownHeaderNames.Host, true ) == 0 && replaceHostName is not null) {
                    hValue = replaceHostName;
                }
                else if (string.Compare ( hName, HttpKnownHeaderNames.Expect, true ) == 0 && hValue == "100-continue") {
                    expectContinue = true;
                }
                else if (string.Compare ( hName, HttpKnownHeaderNames.XForwardedFor, true ) == 0) {
                    fwHeader = false;
                    forwardedFor = hValue;
                }

                if (fwHeader)
                    headerList.Add ( (hName, hValue) );
            }

            headers = headerList;
            return true;
        }
        catch (Exception ex) {
            Logger.LogInformation ( $"#{clientId}: Couldn't read HTTP request from {inboundStream.GetType ().Name}: {ex.Message}" );
        }

ret:
        method = null;
        path = null;
        proto = null;
        headers = new ();
        return false;
    }
}
