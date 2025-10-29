// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngineContextResponse.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Engine;

/// <summary>
/// Provides an abstract base class for HTTP responses.
/// </summary>
public abstract class HttpServerEngineContextResponse : IDisposable {
    /// <summary>
    /// Gets or sets the HTTP status code.
    /// </summary>
    /// <value>
    /// The HTTP status code.
    /// </value>
    public abstract int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the status description.
    /// </summary>
    /// <value>
    /// The status description.
    /// </value>
    public abstract string StatusDescription { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether the connection should be kept alive.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the connection should be kept alive; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool KeepAlive { get; set; }

    /// <summary>
    /// Gets or sets a value indicating whether chunked transfer encoding is used.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if chunked transfer encoding is used; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool SendChunked { get; set; }

    /// <summary>
    /// Gets or sets the content length of the response.
    /// </summary>
    /// <value>
    /// The content length of the response.
    /// </value>
    public abstract long ContentLength64 { get; set; }

    /// <summary>
    /// Gets or sets the content type of the response.
    /// </summary>
    /// <value>
    /// The content type of the response.
    /// </value>
    public abstract string? ContentType { get; set; }

    /// <summary>
    /// Gets or sets the HTTP headers.
    /// </summary>
    /// <value>
    /// The <see cref="IHttpEngineHeaderList"/> containing the HTTP headers.
    /// </value>
    public abstract IHttpEngineHeaderList Headers { get; }

    /// <summary>
    /// Appends a header to the response.
    /// </summary>
    /// <param name="name">The name of the header.</param>
    /// <param name="value">The value of the header.</param>
    public abstract void AppendHeader ( string name, string value );

    /// <summary>
    /// Aborts the response.
    /// </summary>
    public abstract void Abort ();

    /// <summary>
    /// Closes the response.
    /// </summary>
    public abstract void Close ();

    /// <summary>
    /// Performs application-defined tasks associated with freeing, releasing, or resetting unmanaged resources.
    /// </summary>
    public abstract void Dispose ();

    /// <summary>
    /// Gets the output stream of the response.
    /// </summary>
    /// <value>
    /// The <see cref="Stream"/> representing the output stream.
    /// </value>
    public abstract Stream OutputStream { get; }
}