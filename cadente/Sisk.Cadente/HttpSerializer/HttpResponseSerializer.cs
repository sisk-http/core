// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseSerializer.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sisk.Cadente.HttpSerializer;

internal static class HttpResponseSerializer {

    static Encoding _headerDataEncoding = Encoding.ASCII;

    const byte _H = (byte) 'H';
    const byte _T = (byte) 'T';
    const byte _P = (byte) 'P';
    const byte _1 = (byte) '1';
    const byte _DOT = (byte) '.';
    const byte _SPACE = (byte) ' ';
    const byte _CR = (byte) '\r';
    const byte _LF = (byte) '\n';
    const byte _COLON = (byte) ':';
    const byte _SLASH = (byte) '/';

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static int GetResponseHeadersBytes ( scoped Span<byte> buffer, HttpHostContext.HttpResponse response ) {

        // HTTP/1.1
        int position = 0;

        buffer [ position++ ] = _H;
        buffer [ position++ ] = _T;
        buffer [ position++ ] = _T;
        buffer [ position++ ] = _P;
        buffer [ position++ ] = _SLASH;
        buffer [ position++ ] = _1;
        buffer [ position++ ] = _DOT;
        buffer [ position++ ] = _1;
        buffer [ position++ ] = _SPACE;

        int statusCodeCount = _headerDataEncoding.GetBytes ( response.StatusCode.ToString (), buffer [ position.. ] );
        position += statusCodeCount;

        buffer [ position++ ] = _SPACE;

        int statusReasonCode = _headerDataEncoding.GetBytes ( response.StatusDescription, buffer [ position.. ] );
        position += statusReasonCode;

        buffer [ position++ ] = _CR;
        buffer [ position++ ] = _LF;

        var headersSpan = CollectionsMarshal.AsSpan ( response.Headers );
        ref HttpHeader headerPointer = ref MemoryMarshal.GetReference ( headersSpan );
        for (int i = 0; i < headersSpan.Length; i++) {
            ref HttpHeader header = ref Unsafe.Add ( ref headerPointer, i );

            if (header.IsEmpty)
                continue;

            header.NameBytes.Span.CopyTo ( buffer [ position.. ] );
            position += header.NameBytes.Length;

            buffer [ position++ ] = _COLON;
            buffer [ position++ ] = _SPACE;

            header.ValueBytes.Span.CopyTo ( buffer [ position.. ] );
            position += header.ValueBytes.Length;

            buffer [ position++ ] = _CR;
            buffer [ position++ ] = _LF;
        }

        buffer [ position++ ] = _CR;
        buffer [ position++ ] = _LF;

        return position;
    }

    public static bool WriteHttpResponseHeaders ( Stream outgoingStream, HttpHostContext.HttpResponse response ) {
        try {
            Span<byte> responseBuffer = stackalloc byte [ HttpConnection.RESPONSE_BUFFER_SIZE ];

            int headerSize = GetResponseHeadersBytes ( responseBuffer, response );
            outgoingStream.Write ( responseBuffer [ 0..headerSize ] );

            return true;
        }
        catch (Exception) {
            return false;
        }
    }

    public static bool WriteExpectationContinue ( Stream outgoingStream ) {
        try {
            outgoingStream.Write ( "HTTP/1.1 100 Continue\r\n\r\n"u8 );

            return true;
        }
        catch (Exception) {
            return false;
        }
    }
}
