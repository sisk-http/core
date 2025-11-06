// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseSerializer.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Buffers.Binary;
using System.Buffers.Text;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text;

namespace Sisk.Cadente.HttpSerializer;

internal class HttpResponseSerializer {

    private static readonly ASCIIEncoding _strictAscii = new ();

    private static ReadOnlySpan<byte> Http11Prefix => "HTTP/1.1 "u8;
    private const ushort ColonSpacePacked = 0x203A; // ": "
    private const ushort CrLfPacked = 0x0A0D; // "\r\n"

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static ReadOnlySpan<byte> GetKnownReasonPhrase ( int statusCode ) => statusCode switch {
        100 => "Continue"u8,
        101 => "Switching Protocols"u8,
        102 => "Processing"u8,
        200 => "OK"u8,
        201 => "Created"u8,
        202 => "Accepted"u8,
        203 => "Non-Authoritative Information"u8,
        204 => "No Content"u8,
        205 => "Reset Content"u8,
        206 => "Partial Content"u8,
        300 => "Multiple Choices"u8,
        301 => "Moved Permanently"u8,
        302 => "Found"u8,
        303 => "See Other"u8,
        304 => "Not Modified"u8,
        307 => "Temporary Redirect"u8,
        308 => "Permanent Redirect"u8,
        400 => "Bad Request"u8,
        401 => "Unauthorized"u8,
        402 => "Payment Required"u8,
        403 => "Forbidden"u8,
        404 => "Not Found"u8,
        405 => "Method Not Allowed"u8,
        406 => "Not Acceptable"u8,
        407 => "Proxy Authentication Required"u8,
        408 => "Request Timeout"u8,
        409 => "Conflict"u8,
        410 => "Gone"u8,
        411 => "Length Required"u8,
        412 => "Precondition Failed"u8,
        413 => "Payload Too Large"u8,
        414 => "URI Too Long"u8,
        415 => "Unsupported Media Type"u8,
        416 => "Range Not Satisfiable"u8,
        417 => "Expectation Failed"u8,
        421 => "Misdirected Request"u8,
        422 => "Unprocessable Content"u8,
        426 => "Upgrade Required"u8,
        428 => "Precondition Required"u8,
        429 => "Too Many Requests"u8,
        431 => "Request Header Fields Too Large"u8,
        451 => "Unavailable For Legal Reasons"u8,
        500 => "Internal Server Error"u8,
        501 => "Not Implemented"u8,
        502 => "Bad Gateway"u8,
        503 => "Service Unavailable"u8,
        504 => "Gateway Timeout"u8,
        505 => "HTTP Version Not Supported"u8,
        _ => ReadOnlySpan<byte>.Empty
    };

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static void WriteTwoBytes ( ref byte destination, int index, ushort packedValue ) {
        if (!BitConverter.IsLittleEndian) {
            packedValue = BinaryPrimitives.ReverseEndianness ( packedValue );
        }

        Unsafe.WriteUnaligned ( ref Unsafe.Add ( ref destination, index ), packedValue );
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    private static int WriteReasonPhrase ( HttpHostContext.HttpResponse response, Span<byte> destination ) {

        ReadOnlySpan<byte> knownPhrase = GetKnownReasonPhrase ( response.StatusCode );
        if (!knownPhrase.IsEmpty) {
            knownPhrase.CopyTo ( destination );
            return knownPhrase.Length;
        }
        else if (!string.IsNullOrEmpty ( response.StatusDescription )) {
            return _strictAscii.GetBytes ( response.StatusDescription.AsSpan (), destination );
        }
        else {
            return 0;
        }
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization | MethodImplOptions.AggressiveInlining )]
    public static int GetResponseHeadersBytes ( Span<byte> buffer, HttpHostContext.HttpResponse response ) {

        ref byte destination = ref MemoryMarshal.GetReference ( buffer );
        int position = 0;

        Http11Prefix.CopyTo ( buffer );
        position += Http11Prefix.Length;

        if (!Utf8Formatter.TryFormat ( response.StatusCode, buffer [ position.. ], out int statusWritten )) {
            throw new OutOfMemoryException ();
        }

        position += statusWritten;

        buffer [ position++ ] = (byte) ' ';

        int reasonWritten = WriteReasonPhrase ( response, buffer [ position.. ] );
        position += reasonWritten;

        WriteTwoBytes ( ref destination, position, CrLfPacked );
        position += 2;

        var headersSpan = CollectionsMarshal.AsSpan ( response.Headers._headers );
        if (!headersSpan.IsEmpty) {
            ref HttpHeader headerPtr = ref MemoryMarshal.GetReference ( headersSpan );

            for (int i = 0; i < headersSpan.Length; i++) {
                ref HttpHeader header = ref Unsafe.Add ( ref headerPtr, i );
                if (header.IsEmpty)
                    continue;

                header.NameBytes.Span.CopyTo ( buffer [ position.. ] );
                position += header.NameBytes.Length;

                WriteTwoBytes ( ref destination, position, ColonSpacePacked );
                position += 2;

                header.ValueBytes.Span.CopyTo ( buffer [ position.. ] );
                position += header.ValueBytes.Length;

                WriteTwoBytes ( ref destination, position, CrLfPacked );
                position += 2;
            }
        }

        WriteTwoBytes ( ref destination, position, CrLfPacked );
        position += 2;

        return position;
    }

    public static async Task<bool> WriteHttpResponseHeaders ( Memory<byte> buffer, Stream outgoingStream, HttpHostContext.HttpResponse response ) {
        try {
            int headerSize = GetResponseHeadersBytes ( buffer.Span, response );
            await outgoingStream.WriteAsync ( buffer [ 0..headerSize ] );

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

    public static byte [] GetRawMessage ( string message, int statusCode, string statusReason ) {
        string content = $"""
            <HTML>
                <HEAD>
                    <TITLE>{statusCode} - {statusReason}</TITLE>
                </HEAD>
                <BODY>
                    <H1>{statusCode} - {statusReason}</H1>
                    <P>{message}</P>
                    <HR>
                    <P><EM>Cadente</EM></P>
                </BODY>
            </HTML>
            """;

        string html =
            $"HTTP/1.1 {statusCode} {statusReason}\r\n" +
            $"Content-Type: text/html\r\n" +
            $"Content-Length: {content.Length}\r\n" +
            $"Connection: close\r\n" +
            $"\r\n" +
            content;

        return Encoding.ASCII.GetBytes ( html );
    }
}
