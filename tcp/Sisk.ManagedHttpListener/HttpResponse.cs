// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponse.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using Sisk.ManagedHttpListener.HttpSerializer;

namespace Sisk.ManagedHttpListener;

public sealed class HttpResponse {
    private Stream _baseOutputStream;
    private HttpSession _session;

    public int StatusCode { get; set; }
    public string StatusDescription { get; set; }
    public List<HttpHeader> Headers { get; set; }

    public TransferEncoding TransferEncoding { get; set; }

    // MUST SPECIFY ResponseStream OR ResponseBytes, NOT BOTH
    public Stream? ResponseStream { get; set; }
    public byte []? ResponseBytes { get; set; }

    public Task<HttpEventStreamWriter> GetEventStreamAsync () => this.GetEventStreamAsync ( Encoding.UTF8 );

    public async Task<HttpEventStreamWriter> GetEventStreamAsync ( Encoding encoding ) {
        this.Headers.Set ( new HttpHeader ( "Content-Type", "text/event-stream" ) );
        this.Headers.Set ( new HttpHeader ( "Cache-Control", "no-cache" ) );

        if (await this._session.WriteHttpResponseHeaders () == false) {
            throw new InvalidOperationException ( "Unable to obtain an output stream for the response." );
        }

        return new HttpEventStreamWriter ( this._baseOutputStream, encoding );
    }

    public async Task<Stream> GetContentStream () {
        if (await this._session.WriteHttpResponseHeaders () == false) {
            throw new InvalidOperationException ( "Unable to obtain an output stream for the response." );
        }

        this.ResponseStream = null;
        this.ResponseBytes = null;

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