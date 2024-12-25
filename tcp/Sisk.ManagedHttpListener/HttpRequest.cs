// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequest.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.ManagedHttpListener.HttpSerializer;
using Sisk.ManagedHttpListener.Streams;

namespace Sisk.ManagedHttpListener;

public sealed class HttpRequest {
    public string Method { get; }
    public string Path { get; }
    public long ContentLength { get; }
    public HttpHeader [] Headers { get; }
    public Stream ContentStream { get; }

    internal HttpRequest ( HttpRequestBase request, HttpRequestStream requestStream ) {
        this.ContentLength = requestStream.Length;

        this.Method = request.Method;
        this.Path = request.Path;
        this.Headers = request.Headers;
        this.ContentStream = requestStream;
    }
}