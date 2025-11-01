// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestBase.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Text;

namespace Sisk.Cadente.HttpSerializer;

sealed class HttpRequestBase : IDisposable {

    private string? _method;
    private string? _path;
    private HttpHeader []? _headers;

    public bool IsExpecting100;
    public bool IsChunked;

    public long ContentLength;
    public bool CanKeepAlive;

    public IMemoryOwner<byte>? BufferOwner;
    public required ReadOnlyMemory<byte> RawBuffer;
    public required ReadOnlyMemory<byte> BufferedContent;

    public required ReadOnlyMemory<byte> MethodRef;
    public required ReadOnlyMemory<byte> PathRef;

    public required ReadOnlyMemory<HttpHeader> Headers;

    public string Method {
        get {
            _method ??= Encoding.ASCII.GetString ( MethodRef.Span );
            return _method;
        }
    }

    public string Path {
        get {
            _path ??= Encoding.ASCII.GetString ( PathRef.Span );
            return _path;
        }
    }

    public HttpHeader [] HeadersAR {
        get {
            _headers ??= Headers.ToArray ();
            return _headers;
        }
    }

    public void Dispose () {
        BufferOwner?.Dispose ();
    }
}
