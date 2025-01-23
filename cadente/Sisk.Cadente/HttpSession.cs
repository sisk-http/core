// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpSession.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Cadente.HttpSerializer;
using Sisk.Cadente.Streams;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP session that manages the request and response for a single connection.
/// </summary>
public sealed class HttpSession {

    private Stream _connectionStream;
    internal bool ResponseHeadersAlreadySent = false;

    internal ValueTask<bool> WriteHttpResponseHeaders () {
        if (this.ResponseHeadersAlreadySent) {
            return ValueTask.FromResult ( true );
        }

        this.ResponseHeadersAlreadySent = true;
        return HttpResponseSerializer.WriteHttpResponseHeaders ( this._connectionStream, this.Response );
    }

    /// <summary>
    /// Gets the HTTP request associated with this session.
    /// </summary>
    public HttpSessionRequest Request { get; }

    /// <summary>
    /// Gets the HTTP response associated with this session.
    /// </summary>
    public HttpSessionResponse Response { get; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection should be kept alive.
    /// </summary>
    public bool KeepAlive { get; set; } = true;

    internal HttpSession ( HttpRequestBase baseRequest, Stream connectionStream ) {
        this._connectionStream = connectionStream;

        HttpRequestStream requestStream = new HttpRequestStream ( connectionStream, baseRequest );
        this.Request = new HttpSessionRequest ( baseRequest, requestStream );
        this.Response = new HttpSessionResponse ( this, connectionStream );
    }
}
