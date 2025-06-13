// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Collections.Specialized;
using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Internal;

namespace Sisk.Core.Entity;

internal sealed class MultipartFormReader {
    readonly byte [] boundaryBytes;
    readonly byte [] bytes;
    readonly Encoding encoding;
    readonly bool debugEnabled;

    const byte CR = 0x0D;
    const byte LF = 0x0A;
    const byte COLON = 0x3A;
    const byte SPACE = 0x20;

    public MultipartFormReader ( byte [] inputBytes, byte [] boundaryBytes, Encoding baseEncoding, bool debugEnabled ) {
        this.boundaryBytes = boundaryBytes;
        encoding = baseEncoding;
        bytes = inputBytes;
        this.debugEnabled = debugEnabled;
    }

    public MultipartObject [] Read ( CancellationToken cancellation = default ) {

        Memory<byte> memory = new Memory<byte> ( bytes );
        ReadOnlySequence<byte> byteSequence = new ReadOnlySequence<byte> ( memory );
        SequenceReader<byte> reader = new SequenceReader<byte> ( byteSequence );

        List<MultipartObject> objects = new List<MultipartObject> ();

        while (reader.TryReadTo ( out ReadOnlySpan<byte> boundary, [ CR, LF ], advancePastDelimiter: true )) {

            // read headers
            NameValueCollection headers = new NameValueCollection ();
            while (reader.TryReadTo ( out ReadOnlySpan<byte> headerLine, [ CR, LF ], advancePastDelimiter: true )) {

                var headerSeparatorIndex = headerLine.IndexOf ( COLON );

                if (headerSeparatorIndex == -1) {
                    break;
                }

                var headerName = headerLine [ 0..headerSeparatorIndex ];
                var headerValue = headerLine [ (headerSeparatorIndex + 1).. ].Trim ( SPACE );

                headers.Add ( encoding.GetString ( headerName ), encoding.GetString ( headerValue ) );
            }

            // read content
            if (reader.TryReadTo ( out ReadOnlySpan<byte> content, boundary, advancePastDelimiter: false )) {

                objects.Add ( new MultipartObject ( headers, content [ 0..^2 ].ToArray (), encoding ) );
            }
        }

        return objects.ToArray ();
    }
}