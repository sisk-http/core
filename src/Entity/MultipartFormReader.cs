// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Internal;

namespace Sisk.Core.Entity;

internal sealed class MultipartFormReader {
    readonly byte [] boundaryBytes;
    readonly byte [] bytes;
    readonly byte [] nlbytes;
    int position;
    readonly Encoding encoder;
    readonly bool debugEnabled;

    public MultipartFormReader ( byte [] inputBytes, byte [] boundaryBytes, Encoding baseEncoding, bool debugEnabled ) {
        this.boundaryBytes = boundaryBytes;
        encoder = baseEncoding;
        bytes = inputBytes;
        position = 0;
        nlbytes = baseEncoding.GetBytes ( "\r\n" );
        this.debugEnabled = debugEnabled;
    }

    void ThrowDataException ( string message ) {
        if (debugEnabled) {
            throw new InvalidDataException ( SR.Format ( SR.MultipartFormReader_InvalidData, position, message ) );
        }
    }

    bool CanRead { get => position < bytes.Length; }

    int ReadByte () {
        if (CanRead)
            return bytes [ position++ ];

        return -1;
    }

    void ReadNewLine () {
        position += nlbytes.Length;
    }

    int Read ( Span<byte> buffer, CancellationToken cancellation ) {
        int read = 0;
        for (int i = 0; i < buffer.Length; i++) {
            cancellation.ThrowIfCancellationRequested ();
            if (ReadByte () is > 0 and int b) {
                buffer [ read++ ] = (byte) b;
            }
            else
                break;
        }
        return read;
    }

    public MultipartObject [] Read ( CancellationToken cancellation = default ) {
        List<MultipartObject> objects = new List<MultipartObject> ();
        while (CanRead) {
            cancellation.ThrowIfCancellationRequested ();
            ReadNextBoundary ( cancellation );
            NameValueCollection headers = ReadHeaders ();

            if (!CanRead)
                break;

            byte [] content = ReadContent ( cancellation ).ToArray ();

            ReadNewLine ();

            string? contentDisposition = headers [ HttpKnownHeaderNames.ContentDisposition ];
            if (contentDisposition is null) {
                ThrowDataException ( "The Content-Disposition header is empty or missing." );
                continue;
            }

            var cdispositionValues = StringKeyStoreCollection.FromCookieString ( contentDisposition );

            string? formItemName = cdispositionValues [ "name" ]?.Trim ( SharedChars.DoubleQuote );
            string? formFilename = cdispositionValues [ "filename" ]?.Trim ( SharedChars.DoubleQuote );

            if (string.IsNullOrEmpty ( formItemName )) {
                ThrowDataException ( "The Content-Disposition \"name\" parameter is empty or missing." );
                continue;
            }

            MultipartObject resultObj = new MultipartObject ( headers, formFilename, formItemName, content, encoder );

            objects.Add ( resultObj );
        }

        return objects.ToArray ();
    }

    string ReadLine () {
        Span<byte> line = stackalloc byte [ 2048 ];
        int read,
            n = 0,
            lnbytelen = nlbytes.Length;

        while ((read = ReadByte ()) > 0) {
            if (n == line.Length) {
                ThrowDataException ( $"Header line was too long (> {line.Length} bytes allocated)." );
                break;
            }

            line [ n++ ] = (byte) read;

            if (n >= lnbytelen) {
                if (line [ (n - lnbytelen)..n ].SequenceEqual ( nlbytes )) {
                    break;
                }
            }
        }

        return encoder.GetString ( line [ 0..n ] );
    }

    Span<byte> ReadContent ( CancellationToken cancellation = default ) {
        var boundarySpan = boundaryBytes.AsSpan ();
        int boundaryLen = boundaryBytes.Length;
        int istart = position;

        while (CanRead) {
            position++;
            cancellation.ThrowIfCancellationRequested ();

            if ((position - istart) > boundaryLen) {
                if (bytes [ (position - boundaryLen)..position ].AsSpan ().SequenceCompareTo ( boundarySpan ) == 0) {
                    break;
                }
            }
        }

        position -= boundaryLen + nlbytes.Length + 2 /* +2 represents the boundary "--" construct */;

        return bytes.AsSpan () [ istart..position ];
    }

    NameValueCollection ReadHeaders () {
        NameValueCollection headers = new NameValueCollection ();
        string? line;
        while (!string.IsNullOrEmpty ( line = ReadLine () )) {
            int sepIndex = line.IndexOf ( ':', StringComparison.Ordinal );
            if (sepIndex == -1)
                break;

            string hname = line [ ..sepIndex ];
            string hvalue = line [ (sepIndex + 1).. ].Trim ();

            headers.Add ( hname, hvalue );
        }

        return headers;
    }

    void ReadNextBoundary ( CancellationToken cancellation ) {
        Span<byte> boundaryBlock = stackalloc byte [ boundaryBytes.Length + 2 ];
        int nextLine = Read ( boundaryBlock, cancellation );

        ReadNewLine ();

        if (nextLine != boundaryBlock.Length) {
            ThrowDataException ( $"Boundary expected at byte {position}." );
        }
        if (!boundaryBlock [ 2.. ].SequenceEqual ( boundaryBytes )) {
            ThrowDataException ( $"The provided boundary string does not match the request boundary string." );
        }
    }
}