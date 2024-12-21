// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpSession.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.ManagedHttpListener.HttpSerializer;
using Sisk.ManagedHttpListener.Streams;

namespace Sisk.ManagedHttpListener;

public sealed class HttpSession {
    public HttpRequest Request { get; }
    public HttpResponse Response { get; }

    public bool KeepAlive { get; set; } = true;

    internal HttpSession ( HttpRequestBase baseRequest, Stream contentStream ) {
        HttpRequestStream requestStream = new HttpRequestStream ( contentStream, baseRequest );
        this.Request = new HttpRequest ( baseRequest, requestStream );
        this.Response = new HttpResponse ();
    }
}
