// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseStreamManager.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using Sisk.Core.Helpers;

namespace Sisk.Core.Http.Streams;

/// <summary>
/// Represents a way to manage HTTP requests with their output streams, without relying on synchronous content.
/// </summary>
public sealed class HttpResponseStreamManager {
    internal HttpListenerResponse listenerResponse;
    private bool hasSentData;

    // calculated on chunked encoding, but set on SetContentLength
    internal long calculatedLength;

    internal HttpResponseStreamManager ( HttpListenerResponse listenerResponse, HttpListenerRequest listenerRequest, HttpRequest host ) {
        this.listenerResponse = listenerResponse ?? throw new ArgumentNullException ( nameof ( listenerResponse ) );
        ResponseStream = new ResponseStreamWriter ( listenerResponse.OutputStream, this );

        if (host.Context.MatchedRoute?.UseCors == true)
            HttpServer.SetCorsHeaders ( listenerRequest, host.Context.ListeningHost?.CrossOriginResourceSharingPolicy, listenerResponse );
    }

    /// <summary>
    /// Gets or sets whether this HTTP response stream should use chunked transfer encoding.
    /// </summary>
    public bool SendChunked {
        get {
            return listenerResponse.SendChunked;
        }
        set {
            listenerResponse.SendChunked = value;
            if (listenerResponse.ContentLength64 > 0)
                listenerResponse.ContentLength64 = 0;
        }
    }

    /// <summary>
    /// Gets the <see cref="Stream"/> that represents the HTTP response output stream.
    /// </summary>
    public Stream ResponseStream { get; private set; }

    /// <summary>
    /// Sets the Content-Length header of this response stream. If this response stream is using chunked transfer encoding, this method
    /// will do nothing.
    /// </summary>
    /// <param name="contentLength">The length in bytes of the content stream.</param>
    public void SetContentLength ( long contentLength ) {
        if (SendChunked)
            SendChunked = false;

        listenerResponse.ContentLength64 = contentLength;
    }

    /// <summary>
    /// Sets the specific HTTP header into this response stream.
    /// </summary>
    /// <remarks>
    /// Headers are sent immediately, along with the HTTP response code, after starting to send content or closing this stream.
    /// </remarks>
    /// <param name="headerName">The HTTP header name.</param>
    /// <param name="value">The HTTP header value.</param>
    public void SetHeader ( string headerName, object? value ) {
        string? _value = value?.ToString ();
        if (_value is null) {
            return;
        }
        if (hasSentData)
            throw new InvalidOperationException ( SR.Httpserver_Commons_HeaderAfterContents );
        if (string.Equals ( headerName, HttpKnownHeaderNames.ContentLength, StringComparison.OrdinalIgnoreCase )) {
            SetContentLength ( long.Parse ( _value, null ) );
        }
        else if (string.Equals ( headerName, HttpKnownHeaderNames.ContentType, StringComparison.OrdinalIgnoreCase )) {
            listenerResponse.ContentType = _value;
        }
        else {
            listenerResponse.AddHeader ( headerName, _value );
        }
    }

    /// <summary>
    /// Sets the HTTP status code for this response stream.
    /// </summary>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    public void SetStatus ( int httpStatusCode ) {
        if (hasSentData)
            throw new InvalidOperationException ( SR.Httpserver_Commons_HeaderAfterContents );
        listenerResponse.StatusCode = httpStatusCode;
    }

    /// <summary>
    /// Sets the HTTP status code for this response stream.
    /// </summary>
    /// <param name="statusCode">The HTTP status code.</param>
    public void SetStatus ( HttpStatusCode statusCode ) {
        if (hasSentData)
            throw new InvalidOperationException ( SR.Httpserver_Commons_HeaderAfterContents );
        listenerResponse.StatusCode = (int) statusCode;
    }

    /// <summary>
    /// Sets the HTTP status code and description for this response stream.
    /// </summary>
    /// <param name="statusCode">The custom HTTP status code information.</param>
    public void SetStatus ( HttpStatusInformation statusCode ) {
        if (hasSentData)
            throw new InvalidOperationException ( SR.Httpserver_Commons_HeaderAfterContents );
        listenerResponse.StatusCode = statusCode.StatusCode;
        listenerResponse.StatusDescription = statusCode.Description;
    }

    /// <summary>
    /// Writes an sequence of bytes to the HTTP response stream.
    /// </summary>
    /// <param name="buffer">The read only memory that includes the buffer which will be written to the HTTP response.</param>
    public void Write ( ReadOnlySpan<byte> buffer ) {
        hasSentData = true;
        listenerResponse.OutputStream.Write ( buffer );
    }

    /// <summary>
    /// Writes an sequence of bytes to the HTTP response stream.
    /// </summary>
    /// <param name="buffer">The byte array that includes the buffer which will be written to the HTTP response.</param>
    public void Write ( byte [] buffer ) {
        hasSentData = true;
        listenerResponse.OutputStream.Write ( buffer );
    }

    /// <summary>
    /// Closes this HTTP response stream connection between the server and the client and returns an empty <see cref="HttpResponse"/> to
    /// finish the HTTP server context.
    /// </summary>
    public HttpResponse Close () {
        return new HttpResponse ( HttpResponse.HTTPRESPONSE_SERVER_CLOSE ) {
            CalculedLength = calculatedLength
        };
    }

    #region Cookie setter helpers
    /// <summary>
    /// Sets a cookie and sends it in the response to be set by the client.
    /// </summary>
    /// <param name="cookie">The cookie object.</param>
    public void SetCookie ( Cookie cookie ) {
        SetHeader ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( cookie ) );
    }

    /// <summary>
    /// Sets a cookie and sends it in the response to be set by the client.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    public void SetCookie ( string name, string value ) {
        SetHeader ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( name, value ) );
    }

    /// <summary>
    /// Sets a cookie and sends it in the response to be set by the client.
    /// </summary>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="expires">The cookie expirity date.</param>
    /// <param name="maxAge">The cookie max duration after being set.</param>
    /// <param name="domain">The domain where the cookie will be valid.</param>
    /// <param name="path">The path where the cookie will be valid.</param>
    /// <param name="secure">Determines if the cookie will only be stored in an secure context.</param>
    /// <param name="httpOnly">Determines if the cookie will be only available in the HTTP context.</param>
    /// <param name="sameSite">The cookie SameSite parameter.</param>
    public void SetCookie ( string name,
        string value,
        DateTime? expires = null,
        TimeSpan? maxAge = null,
        string? domain = null,
        string? path = null,
        bool? secure = null,
        bool? httpOnly = null,
        string? sameSite = null ) {
        SetHeader ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( name, value, expires, maxAge, domain, path, secure, httpOnly, sameSite ) );
    }
    #endregion
}

internal sealed class ResponseStreamWriter : Stream {
    private readonly Stream BaseStream;
    private readonly HttpResponseStreamManager Parent;

    public ResponseStreamWriter ( Stream baseStream, HttpResponseStreamManager parent ) {
        BaseStream = baseStream;
        Parent = parent;
    }

    public override Boolean CanRead => BaseStream.CanRead;

    public override Boolean CanSeek => BaseStream.CanSeek;

    public override Boolean CanWrite => BaseStream.CanWrite;

    public override Int64 Length => BaseStream.Length;

    public override Int64 Position { get => BaseStream.Position; set => BaseStream.Position = value; }

    public override void Flush () {
        BaseStream.Flush ();
    }

    public override Int32 Read ( Byte [] buffer, Int32 offset, Int32 count ) {
        return BaseStream.Read ( buffer, offset, count );
    }

    public override Int64 Seek ( Int64 offset, SeekOrigin origin ) {
        return BaseStream.Seek ( offset, origin );
    }

    public override void SetLength ( Int64 value ) {
        BaseStream.SetLength ( value );
    }

    public override void Write ( Byte [] buffer, Int32 offset, Int32 count ) {
        if (Parent.listenerResponse.ContentLength64 == 0) {
            Parent.SendChunked = true;
        }
        BaseStream.Write ( buffer, offset, count );
        Interlocked.Add ( ref Parent.calculatedLength, buffer.Length );
    }
}