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

    private Stream _connectionStream;
    internal bool ResponseHeadersAlreadySent = false;

    internal Task<bool> WriteHttpResponseHeaders () {
        if (this.ResponseHeadersAlreadySent) {
            return Task.FromResult ( true );
        }

        this.ResponseHeadersAlreadySent = true;
        return HttpResponseSerializer.WriteHttpResponseHeaders ( this._connectionStream, this.Response );
    }


    public HttpRequest Request { get; }
    public HttpResponse Response { get; }

    public bool KeepAlive { get; set; } = true;

    internal HttpSession ( HttpRequestBase baseRequest, Stream connectionStream ) {
        this._connectionStream = connectionStream;

        HttpRequestStream requestStream = new HttpRequestStream ( connectionStream, baseRequest );
        this.Request = new HttpRequest ( baseRequest, requestStream );
        this.Response = new HttpResponse ( this, connectionStream );
    }
}
