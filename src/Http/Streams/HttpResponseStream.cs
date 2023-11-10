// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseStream.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Net;

namespace Sisk.Core.Http.Streams;

/// <summary>
/// Represents a way to manage HTTP requests with their output streams, without relying on synchronous content.
/// </summary>
/// <definition>
/// public class HttpResponseStream
/// </definition>
/// <type>
/// Class
/// </type>
public sealed class HttpResponseStream : CookieHelper
{
    private HttpListenerResponse listenerResponse;
    private bool hasSentData = false;

    internal HttpResponseStream(HttpListenerResponse listenerResponse, HttpListenerRequest listenerRequest, HttpRequest host)
    {
        this.listenerResponse = listenerResponse ?? throw new ArgumentNullException(nameof(listenerResponse));
        this.ResponseStream = listenerResponse.OutputStream;
        HttpServer.SetCorsHeaders(host.baseServer.ServerConfiguration.Flags, listenerRequest, host.hostContext.CrossOriginResourceSharingPolicy, listenerResponse);
    }

    /// <summary>
    /// Gets or sets whether this HTTP response stream should use chunked transfer encoding.
    /// </summary>
    /// <definition>
    /// public bool SendChunked { get; set; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public bool SendChunked
    {
        get
        {
            return listenerResponse.SendChunked;
        }
        set
        {
            listenerResponse.SendChunked = value;
        }
    }

    /// <summary>
    /// Gets the <see cref="Stream"/> that represents the HTTP response output stream.
    /// </summary>
    /// <definition>
    /// public Stream ResponseStream { get; }
    /// </definition>
    /// <type>
    /// Property
    /// </type>
    public Stream ResponseStream { get; private set; }

    /// <summary>
    /// Sets the specific HTTP header into this response stream.
    /// </summary>
    /// <remarks>
    /// Headers are sent immediately, along with the HTTP response code, after starting to send content or closing this stream.
    /// </remarks>
    /// <param name="headerName">The HTTP header name.</param>
    /// <param name="value">The HTTP header value.</param>
    /// <definition>
    /// public void SetHeader(string headerName, string value)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetHeader(string headerName, string value)
    {
        if (hasSentData) throw new InvalidOperationException(SR.Httpserver_Commons_HeaderAfterContents);
        if (string.Compare(headerName, "Content-Length", true) == 0)
        {
            if (SendChunked)
                return;

            listenerResponse.ContentLength64 = long.Parse(value);
        }
        else if (string.Compare(headerName, "Content-Type", true) == 0)
        {
            listenerResponse.ContentType = value;
        }
        else
        {
            listenerResponse.AddHeader(headerName, value);
        }
    }

    /// <summary>
    /// Sets the HTTP status code for this response stream.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <definition>
    /// public void SetStatus(int httpStatusCode)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetStatus(int httpStatusCode)
    {
        if (hasSentData) throw new InvalidOperationException(SR.Httpserver_Commons_HeaderAfterContents);
        listenerResponse.StatusCode = httpStatusCode;
    }

    /// <summary>
    /// Sets the HTTP status code for this response stream.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    /// <definition>
    /// public void SetStatus(int httpStatusCode)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetStatus(HttpStatusCode statusCode)
    {
        if (hasSentData) throw new InvalidOperationException(SR.Httpserver_Commons_HeaderAfterContents);
        listenerResponse.StatusCode = (int)statusCode;
    }

    /// <summary>
    /// Sets the HTTP status code and description for this response stream.
    /// </summary>
    /// <param name="statusCode">The custom HTTP status code information.</param>
    /// <definition>
    /// public void SetStatus(HttpStatusInformation statusCode)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void SetStatus(HttpStatusInformation statusCode)
    {
        if (hasSentData) throw new InvalidOperationException(SR.Httpserver_Commons_HeaderAfterContents);
        listenerResponse.StatusCode = statusCode.StatusCode;
        listenerResponse.StatusDescription = statusCode.Description;
    }

    /// <summary>
    /// Writes an sequence of bytes to the HTTP response stream.
    /// </summary>
    /// <param name="buffer">The read only memory that includes the buffer which will be written to the HTTP response.</param>
    /// <definition>
    /// public void Write(ReadOnlySpan&lt;byte&gt; buffer)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Write(ReadOnlySpan<byte> buffer)
    {
        hasSentData = true;
        listenerResponse.OutputStream.Write(buffer);
    }

    /// <summary>
    /// Writes an sequence of bytes to the HTTP response stream.
    /// </summary>
    /// <param name="buffer">The byte array that includes the buffer which will be written to the HTTP response.</param>
    /// <definition>
    /// public void Write(byte[] buffer)
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public void Write(byte[] buffer)
    {
        hasSentData = true;
        listenerResponse.OutputStream.Write(buffer);
    }

    /// <summary>
    /// Closes this HTTP response stream connection between the server and the client and returns an empty <see cref="HttpResponse"/> to
    /// finish the HTTP server context.
    /// </summary>
    /// <definition>
    /// public HttpResponse Close()
    /// </definition>
    /// <type>
    /// Method
    /// </type>
    public HttpResponse Close()
    {
        return new HttpResponse(HttpResponse.HTTPRESPONSE_SERVER_CLOSE);
    }

    /// <inheritdoc/>
    protected override void SetCookieHeader(string name, string value)
    {
        SetHeader(name, value);
    }
}
