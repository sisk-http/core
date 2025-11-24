// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpFileRangedContentStream.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Globalization;
using System.Net;
using Sisk.Core.Helpers;
using Sisk.Core.Http;
using Sisk.Core.Http.FileSystem;

abstract class HttpFileRangedContentStream : HttpFileServerFileConverter {

    public virtual int ChunkSize => 5 * (int) SizeHelper.UnitMb;

    public override HttpResponse Convert ( FileInfo file, HttpRequest request ) {

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( ChunkSize );

        using var fs = file.OpenRead ();
        long length = fs.Length;

        string? rangeHeader = request.Headers.Range;

        bool hasRange = TryGetSingleRange (
            rangeHeader,
            length,
            out long rangeStart,
            out long rangeEnd,
            out bool isRangeSatisfiable
        );

        if (hasRange && !isRangeSatisfiable) {
            return new HttpResponse ( HttpStatusCode.RequestedRangeNotSatisfiable )
                .WithHeader ( HttpKnownHeaderNames.ContentRange, $"bytes */{length}" );
        }

        if (!hasRange) {
            var resStream = request.GetResponseStream ();

            resStream.SetStatus ( HttpStatusCode.OK );
            resStream.SetHeader ( HttpKnownHeaderNames.AcceptRanges, "bytes" );
            resStream.SetHeader ( HttpKnownHeaderNames.ContentType, MimeHelper.GetMimeType ( file.Extension ) );
            resStream.SetContentLength ( length );

            var buffer = ArrayPool<byte>.Shared.Rent ( ChunkSize );
            try {
                int read;
                while ((read = fs.Read ( buffer, 0, buffer.Length )) > 0) {
                    resStream.ResponseStream.Write ( buffer, 0, read );
                }
            }
            finally {
                ArrayPool<byte>.Shared.Return ( buffer );
            }

            return resStream.Close ();
        }

        long requestedLength = rangeEnd - rangeStart + 1;

        long maxLength = ChunkSize > 0
            ? Math.Min ( requestedLength, ChunkSize )
            : requestedLength;

        long actualEnd = rangeStart + maxLength - 1;

        var partialResStream = request.GetResponseStream ();

        partialResStream.SetStatus ( HttpStatusCode.PartialContent );
        partialResStream.SetHeader ( HttpKnownHeaderNames.AcceptRanges, "bytes" );
        partialResStream.SetHeader ( HttpKnownHeaderNames.ContentType, MimeHelper.GetMimeType ( file.Extension ) );
        partialResStream.SetHeader (
            HttpKnownHeaderNames.ContentRange,
            $"bytes {rangeStart}-{actualEnd}/{length}"
        );
        partialResStream.SetContentLength ( maxLength );

        fs.Position = rangeStart;

        // Buffer de leitura (no máximo ChunkSize, e nunca mais do que o que falta enviar)
        var rangeBuffer = ArrayPool<byte>.Shared.Rent ( (int) Math.Min ( ChunkSize, maxLength ) );
        try {
            long remaining = maxLength;
            while (remaining > 0) {
                int read = fs.Read ( rangeBuffer, 0, (int) Math.Min ( rangeBuffer.Length, remaining ) );
                if (read <= 0)
                    break;

                if (request.Method != HttpMethod.Head)
                    partialResStream.ResponseStream.Write ( rangeBuffer, 0, read );
                remaining -= read;
            }
        }
        finally {
            ArrayPool<byte>.Shared.Return ( rangeBuffer );
        }

        return partialResStream.Close ();
    }

    private static bool TryGetSingleRange ( string? rangeHeader, long entityLength, out long rangeStart, out long rangeEnd, out bool isSatisfiable ) {
        rangeStart = 0;
        rangeEnd = 0;
        isSatisfiable = false;

        if (string.IsNullOrWhiteSpace ( rangeHeader ))
            return false;

        rangeHeader = rangeHeader.Trim ();

        if (!rangeHeader.StartsWith ( "bytes=", StringComparison.OrdinalIgnoreCase ))
            return false;

        var rangeSpec = rangeHeader.Substring ( "bytes=".Length ).Trim ();
        if (string.IsNullOrEmpty ( rangeSpec ))
            return false;

        if (rangeSpec.Contains ( ',' ))
            return false;

        var parts = rangeSpec.Split ( '-', 2 );
        if (parts.Length != 2)
            return false;

        string startPart = parts [ 0 ].Trim ();
        string endPart = parts [ 1 ].Trim ();

        // Caso 1: bytes=start-end
        if (startPart.Length > 0 && endPart.Length > 0) {
            if (!long.TryParse ( startPart, NumberStyles.None, CultureInfo.InvariantCulture, out var start ) ||
                !long.TryParse ( endPart, NumberStyles.None, CultureInfo.InvariantCulture, out var end )) {
                return false;
            }

            if (start < 0 || end < start)
                return false;

            if (start >= entityLength) {
                isSatisfiable = false;
                return true;
            }

            rangeStart = start;
            rangeEnd = Math.Min ( end, entityLength - 1 );
            isSatisfiable = true;
            return true;
        }

        if (startPart.Length > 0 && endPart.Length == 0) {
            if (!long.TryParse ( startPart, NumberStyles.None, CultureInfo.InvariantCulture, out var start ))
                return false;

            if (start < 0)
                return false;

            if (start >= entityLength) {
                isSatisfiable = false;
                return true;
            }

            rangeStart = start;
            rangeEnd = entityLength - 1;
            isSatisfiable = true;
            return true;
        }

        if (startPart.Length == 0 && endPart.Length > 0) {
            if (!long.TryParse ( endPart, NumberStyles.None, CultureInfo.InvariantCulture, out var suffixLength ))
                return false;

            if (suffixLength <= 0)
                return false;

            if (entityLength == 0) {
                isSatisfiable = false;
                return true;
            }

            if (suffixLength >= entityLength) {
                rangeStart = 0;
            }
            else {
                rangeStart = entityLength - suffixLength;
            }

            rangeEnd = entityLength - 1;
            isSatisfiable = true;
            return true;
        }

        return false;
    }
}