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

    public ReadOnlyMemory<byte> BufferedContent;
    public ReadOnlyMemory<byte> MethodRef;
    public ReadOnlyMemory<byte> PathRef;
    public ReadOnlyMemory<HttpHeader> Headers;

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

    public void Reset() {
        _method = null;
        _path = null;
        IsExpecting100 = false;
        IsChunked = false;
        ContentLength = 0;
        CanKeepAlive = false;
        BufferedContent = default;
        MethodRef = default;
        PathRef = default;
        Headers = default;
    }
}
