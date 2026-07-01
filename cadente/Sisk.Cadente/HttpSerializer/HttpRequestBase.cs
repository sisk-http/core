// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestBase.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Cadente.HttpSerializer;

sealed class HttpRequestBase {

    private string? _method;
    private string? _path;

    public bool IsExpecting100;
    public bool IsChunked;

    public long ContentLength;
    public bool CanKeepAlive;

    public required ReadOnlyMemory<byte> BufferedContent;
    public required ReadOnlyMemory<byte> MethodRef;
    public required ReadOnlyMemory<byte> PathRef;
    public required ReadOnlyMemory<byte> HeaderBlockRef;

    private HttpHeader []? _headers;

    public string Method {
        get {
            if (_method is { })
                return _method;

            ReadOnlySpan<byte> method = MethodRef.Span;
            if (method.SequenceEqual ( "GET"u8 ))
                return _method = "GET";
            if (method.SequenceEqual ( "POST"u8 ))
                return _method = "POST";
            if (method.SequenceEqual ( "PUT"u8 ))
                return _method = "PUT";
            if (method.SequenceEqual ( "DELETE"u8 ))
                return _method = "DELETE";
            if (method.SequenceEqual ( "PATCH"u8 ))
                return _method = "PATCH";
            if (method.SequenceEqual ( "HEAD"u8 ))
                return _method = "HEAD";
            if (method.SequenceEqual ( "OPTIONS"u8 ))
                return _method = "OPTIONS";

            return _method = Encoding.ASCII.GetString ( method );
        }
    }

    public string Path {
        get {
            if (_path is { })
                return _path;

            ReadOnlySpan<byte> path = PathRef.Span;
            if (path.Length == 1 && path [ 0 ] == (byte) '/')
                return _path = "/";

            return _path = Encoding.ASCII.GetString ( path );
        }
    }

    public ReadOnlyMemory<HttpHeader> Headers {
        get {
            _headers ??= HttpRequestReader.ParseHeaders ( HeaderBlockRef );
            return _headers;
        }
    }
}
