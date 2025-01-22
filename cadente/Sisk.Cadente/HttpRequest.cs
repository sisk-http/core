// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequest.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Cadente.HttpSerializer;
using Sisk.Cadente.Streams;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP request.
/// </summary>
public sealed class HttpRequest {

    private HttpRequestBase _baseRequest;

    /// <summary>
    /// Gets the HTTP method (e.g., GET, POST) of the request.
    /// </summary>
    public string Method { get => this._baseRequest.Method; }

    /// <summary>
    /// Gets the path of the requested resource.
    /// </summary>
    public string Path { get => this._baseRequest.Path; }

    /// <summary>
    /// Gets the content length of the request.
    /// </summary>
    public long ContentLength { get; }

    /// <summary>
    /// Gets the headers associated with the request.
    /// </summary>
    public HttpHeader [] Headers { get; }

    /// <summary>
    /// Gets the stream containing the content of the request.
    /// </summary>
    public Stream ContentStream { get; }

    internal HttpRequest ( HttpRequestBase request, HttpRequestStream requestStream ) {
        this.ContentLength = requestStream.Length;

        this._baseRequest = request;
        this.Headers = this._baseRequest.Headers.ToArray ();
        this.ContentStream = requestStream;
    }
}