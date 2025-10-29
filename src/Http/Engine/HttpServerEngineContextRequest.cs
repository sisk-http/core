// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerEngineContextRequest.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Sisk.Core.Http.Engine;

/// <summary>
/// Provides an abstract base class for HTTP requests.
/// </summary>
public abstract class HttpServerEngineContextRequest {
    /// <summary>
    /// Gets a value indicating whether the request is from the local machine.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the request is from the local machine; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool IsLocal { get; }

    /// <summary>
    /// Gets the raw URL of the request.
    /// </summary>
    /// <value>
    /// The raw URL of the request.
    /// </value>
    public abstract string? RawUrl { get; }

    /// <summary>
    /// Gets the query string collection.
    /// </summary>
    /// <value>
    /// The <see cref="NameValueCollection"/> containing the query string parameters.
    /// </value>
    public abstract NameValueCollection QueryString { get; }

    /// <summary>
    /// Gets the HTTP protocol version.
    /// </summary>
    /// <value>
    /// The <see cref="Version"/> representing the HTTP protocol version.
    /// </value>
    public abstract Version ProtocolVersion { get; }

    /// <summary>
    /// Gets the host name of the user.
    /// </summary>
    /// <value>
    /// The host name of the user.
    /// </value>
    public abstract string UserHostName { get; }

    /// <summary>
    /// Gets the URL of the request.
    /// </summary>
    /// <value>
    /// The <see cref="Uri"/> representing the URL of the request.
    /// </value>
    public abstract Uri? Url { get; }

    /// <summary>
    /// Gets the HTTP method of the request.
    /// </summary>
    /// <value>
    /// The HTTP method of the request.
    /// </value>
    public abstract string HttpMethod { get; }

    /// <summary>
    /// Gets the local endpoint of the request.
    /// </summary>
    /// <value>
    /// The <see cref="IPEndPoint"/> representing the local endpoint.
    /// </value>
    public abstract IPEndPoint LocalEndPoint { get; }

    /// <summary>
    /// Gets the remote endpoint of the request.
    /// </summary>
    /// <value>
    /// The <see cref="IPEndPoint"/> representing the remote endpoint.
    /// </value>
    public abstract IPEndPoint RemoteEndPoint { get; }

    /// <summary>
    /// Gets the request trace identifier.
    /// </summary>
    /// <value>
    /// The <see cref="Guid"/> representing the request trace identifier.
    /// </value>
    public abstract Guid RequestTraceIdentifier { get; }

    /// <summary>
    /// Gets the HTTP headers.
    /// </summary>
    /// <value>
    /// The <see cref="WebHeaderCollection"/> containing the HTTP headers.
    /// </value>
    public abstract NameValueCollection Headers { get; }

    /// <summary>
    /// Gets the input stream of the request.
    /// </summary>
    /// <value>
    /// The <see cref="Stream"/> representing the input stream.
    /// </value>
    public abstract Stream InputStream { get; }

    /// <summary>
    /// Gets the content length of the request.
    /// </summary>
    /// <value>
    /// The content length of the request.
    /// </value>
    public abstract long ContentLength64 { get; }

    /// <summary>
    /// Gets a value indicating whether the connection is secure.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if the connection is secure; otherwise, <see langword="false"/>.
    /// </value>
    public abstract bool IsSecureConnection { get; }

    /// <summary>
    /// Gets the content encoding of the request.
    /// </summary>
    /// <value>
    /// The <see cref="Encoding"/> representing the content encoding.
    /// </value>
    public abstract Encoding ContentEncoding { get; }
}