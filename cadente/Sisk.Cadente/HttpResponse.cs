// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponse.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using Sisk.Cadente.HttpSerializer;

namespace Sisk.Cadente;

/// <summary>
/// Represents an HTTP response.
/// </summary>
public sealed class HttpResponse {
    private Stream _baseOutputStream;
    private HttpSession _session;

    /// <summary>
    /// Gets or sets the HTTP status code of the response.
    /// </summary>
    public int StatusCode { get; set; }

    /// <summary>
    /// Gets or sets the status description of the response.
    /// </summary>
    public string StatusDescription { get; set; }

    /// <summary>
    /// Gets or sets the list of headers associated with the response.
    /// </summary>
    public List<HttpHeader> Headers { get; set; }

    /// <summary>
    /// Gets or sets the transfer encoding for the response.
    /// </summary>
    public TransferEncoding TransferEncoding { get; set; }

    // MUST SPECIFY ResponseStream OR ResponseBytes, NOT BOTH
    /// <summary>
    /// Gets or sets the stream for the response content.
    /// </summary>
    public Stream? ResponseStream { get; set; }

    /// <summary>
    /// Asynchronously gets an event stream writer with UTF-8 encoding.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="HttpEventStreamWriter"/> as the result.</returns>
    public Task<HttpEventStreamWriter> GetEventStreamAsync () => this.GetEventStreamAsync ( Encoding.UTF8 );

    /// <summary>
    /// Asynchronously gets an event stream writer with the specified encoding.
    /// </summary>
    /// <param name="encoding">The encoding to use for the event stream.</param>
    /// <returns>A task that represents the asynchronous operation, with a <see cref="HttpEventStreamWriter"/> as the result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to obtain an output stream for the response.</exception>
    public async Task<HttpEventStreamWriter> GetEventStreamAsync ( Encoding encoding ) {
        this.Headers.Set ( new HttpHeader ( "Content-Type", "text/event-stream" ) );
        this.Headers.Set ( new HttpHeader ( "Cache-Control", "no-cache" ) );

        if (await this._session.WriteHttpResponseHeaders () == false) {
            throw new InvalidOperationException ( "Unable to obtain an output stream for the response." );
        }

        return new HttpEventStreamWriter ( this._baseOutputStream, encoding );
    }

    /// <summary>
    /// Asynchronously gets the content stream for the response.
    /// </summary>
    /// <returns>A task that represents the asynchronous operation, with the response content stream as the result.</returns>
    /// <exception cref="InvalidOperationException">Thrown when unable to obtain an output stream for the response.</exception>
    public async Task<Stream> GetContentStream () {
        if (await this._session.WriteHttpResponseHeaders () == false) {
            throw new InvalidOperationException ( "Unable to obtain an output stream for the response." );
        }

        this.ResponseStream = null;
        return this._baseOutputStream;
    }

    internal HttpResponse ( HttpSession session, Stream httpSessionStream ) {
        this._session = session;
        this._baseOutputStream = httpSessionStream;

        this.StatusCode = 200;
        this.StatusDescription = "Ok";

        this.Headers = new List<HttpHeader>
            {
                new HttpHeader ("Date", DateTime.UtcNow.ToString("R")),
                new HttpHeader ("Server", "Sisk")
            };
    }
}