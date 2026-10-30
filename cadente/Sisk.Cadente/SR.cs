// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Cadente;

internal static class SR {
    public const string HttpHeader_NameEmpty = "Header name cannot be empty.";
    public const string HttpHeader_NameNotAllowedCharacters = "Header name contains not allowed characters.";
    public const string HttpHeader_ValueNotAllowedCharacters = "Header value contains not allowed characters.";

    public const string HttpHeaderList_ReadOnly = "The HttpHeaderList is read-only and cannot be modified.";

    public const string HttpConnection_RequestHeaderFieldsTooLarge = "Request header fields too large.";
    public const string HttpConnection_UnexpectedDrainedBody = "Unexpected: request body already drained and stream is null.";

    public const string HttpHost_TlsHandshakeFailed = "SSL/TLS Handshake failed.";
    public const string HttpHost_FailedSslHandshake = "Failed SSL handshake: {0}";
    public const string HttpHost_ConnectionEventsException = "HandleConnectionEventsAsync/exception: {0}";
    public const string HttpHost_SocketException = "SocketException: {0}";
    public const string HttpHost_IOException = "IOException: {0}";
    public const string HttpHost_Exception = "Exception: {0}";

    public const string HttpHostContext_InputStreamUnavailable = "Unable to obtain the input stream for the request.";
    public const string HttpHostContext_HeadersAlreadySent = "Headers already sent for this HTTP response.";
    public const string HttpHostContext_ContentLengthRequired = "Content-Length header must be set for non-chunked responses.";

    public const string HttpRequestStream_UnexpectedEndOfBody = "Request body ended before the declared Content-Length was fully read.";

    public const string HttpChunkedReadStream_MalformedChunkSize = "Invalid chunked transfer encoding: malformed chunk size.";
    public const string HttpChunkedReadStream_ChunkSizeTooLarge = "Invalid chunked transfer encoding: chunk size is too large.";
    public const string HttpChunkedReadStream_EmptyChunkSize = "Invalid chunked transfer encoding: empty chunk size.";
    public const string HttpChunkedReadStream_NegativeChunkSize = "Invalid chunked transfer encoding: negative chunk size {0}.";
    public const string HttpChunkedReadStream_IncompleteChunkMetadata = "Invalid chunked transfer encoding: incomplete chunk metadata.";
    public const string HttpChunkedReadStream_ChunkMetadataTooLong = "Chunk metadata line too long.";
    public const string HttpChunkedReadStream_ExpectedCrlf = "Invalid chunked transfer encoding: expected CRLF.";
    public const string HttpChunkedReadStream_IncompleteChunkTerminator = "Invalid chunked transfer encoding: incomplete chunk terminator.";
    public const string HttpChunkedReadStream_ExpectedCrlfAfterChunkData = "Invalid chunked transfer encoding: expected CRLF after chunk data.";
    public const string HttpChunkedReadStream_IncompleteTrailerSection = "Invalid chunked transfer encoding: incomplete trailer section.";

    public const string HttpRequestReader_ParseFailed = "failed to parse HTTP request: {0}";
    public const string HttpRequestReader_ParseExceptionFailed = "failed to parse HTTP request (exception): {0}";
    public const string HttpRequestReader_MalformedRequest = "malformed request";
    public const string HttpRequestReader_HeadersTooLarge = "failed to parse HTTP request: headers too large";
    public const string HttpRequestReader_HeaderReadTimeout = "failed to parse HTTP request: header read timeout";
    public const string HttpRequestReader_ConnectionClosed = "failed to parse HTTP request: connection closed";
    public const string HttpRequestReader_OperationCancelled = "failed to parse HTTP request: operation cancelled";
    public const string HttpRequestReader_MissingMethod = "failed to parse HTTP request: missing or empty request method in request line";
    public const string HttpRequestReader_InvalidMethod = "failed to parse HTTP request: request method is not a valid token";
    public const string HttpRequestReader_MissingPath = "failed to parse HTTP request: missing request path in request line";
    public const string HttpRequestReader_MissingTarget = "failed to parse HTTP request: missing request target in request line";
    public const string HttpRequestReader_FragmentNotAllowed = "failed to parse HTTP request: fragment is not allowed in request target";
    public const string HttpRequestReader_InvalidTargetBytes = "failed to parse HTTP request: request target contains invalid bytes";
    public const string HttpRequestReader_MissingProtocolVersion = "failed to parse HTTP request: missing HTTP protocol version in request line";
    public const string HttpRequestReader_UnsupportedProtocolVersion = "failed to parse HTTP request: unsupported HTTP protocol version '{0}'";
    public const string HttpRequestReader_TransferEncodingAndContentLength = "failed to parse HTTP request: both Transfer-Encoding and Content-Length present, request-smuggling vector (RFC 9112 §6.3.3)";
    public const string HttpRequestReader_MissingHost = "failed to parse HTTP request: missing Host header violates RFC 9112 §3.2";
    public const string HttpRequestReader_IncompleteHeaderLine = "incomplete header line: no LF found";
    public const string HttpRequestReader_BareLineFeed = "bare LF without preceding CR in header line violates RFC 9112 §2.2";
    public const string HttpRequestReader_ObsFold = "obs-fold header line violates RFC 9112 §5.2";
    public const string HttpRequestReader_HeaderWithoutColon = "header line without colon violates RFC 9112 §5";
    public const string HttpRequestReader_EmptyHeaderName = "empty header name violates RFC 9110 §5.1";
    public const string HttpRequestReader_HeaderNameNotAllowed = "header name contains not allowed characters";
    public const string HttpRequestReader_HeaderValueNotAllowed = "header value contains not allowed characters";
    public const string HttpRequestReader_ConflictingContentLength = "conflicting duplicate Content-Length headers violate RFC 9112 §6.3.3";
    public const string HttpRequestReader_InvalidContentLength = "invalid Content-Length value '{0}' (RFC 9110 §8.6)";
    public const string HttpRequestReader_DuplicateTransferEncoding = "duplicate Transfer-Encoding header is a request-smuggling vector (RFC 9112 §6.3.3)";
    public const string HttpRequestReader_UnsupportedTransferEncoding = "unsupported Transfer-Encoding value '{0}' (only chunked is supported)";
    public const string HttpRequestReader_DuplicateHost = "duplicate Host header violates RFC 9112 §3.2";
    public const string HttpRequestReader_EmptyHost = "empty Host header violates RFC 9112 §3.2";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
