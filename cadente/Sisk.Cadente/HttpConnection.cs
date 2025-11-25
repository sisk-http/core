// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpConnection.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Net;
using System.Runtime.CompilerServices;
using Sisk.Cadente.HttpSerializer;
using Sisk.Core.Http;

namespace Sisk.Cadente;

sealed class HttpConnection : IDisposable {
    private readonly HttpHost _host;
    private readonly IPEndPoint _endpoint;
    private readonly HttpHostClient _client;
    private bool disposedValue;

#if DEBUG
    public readonly int Id = Random.Shared.Next ( 0, ushort.MaxValue );
#else
    public readonly int Id = 0;
#endif

    // buffer dedicated to headers.
    public const int RESERVED_BUFFER_SIZE = 8 * 1024;
    private readonly HttpRequestBase _requestContext = new HttpRequestBase();
    private readonly HttpHeader[] _headerBuffer = new HttpHeader[64];

    internal readonly Stream networkStream;
    internal IMemoryOwner<byte> requestPool, responsePool;

    public HttpConnection ( HttpHostClient client, Stream connectionStream, HttpHost host, IPEndPoint endpoint ) {
        _client = client;
        _host = host;
        _endpoint = endpoint;

        networkStream = connectionStream;

        requestPool = MemoryPool<byte>.Shared.Rent ( RESERVED_BUFFER_SIZE );
        responsePool = MemoryPool<byte>.Shared.Rent ( RESERVED_BUFFER_SIZE );
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public async Task<HttpConnectionState> HandleConnectionEventsAsync () {
        bool connectionCloseRequested = false;

        // Create the context once and reuse it
        HttpHostContext managedSession = new HttpHostContext ( _host, this, _requestContext, _client );

        while (!disposedValue) {

            bool success = await HttpRequestReader.TryReadHttpRequestAsync ( _requestContext, _headerBuffer, requestPool.Memory, networkStream ).ConfigureAwait ( false );

            if (!success) {
                return HttpConnectionState.ConnectionClosed;
            }

            managedSession.Reset();

            await _host.InvokeContextCreated ( managedSession ).ConfigureAwait ( false );

            if (!managedSession.KeepAlive || !_requestContext.CanKeepAlive) {
                connectionCloseRequested = true;
                managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.Connection, "close" ) );
            }

            if (!managedSession.ResponseHeadersAlreadySent) {
                managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.ContentLength, "0" ) );

                await managedSession.WriteHttpResponseHeadersAsync ();
            }

            // Flush is not necessary if we are using BufferedStream and the buffer is full or if we are closing.
            // But we want to ensure the response is sent.
            // However, with BufferedStream, we should Flush only when we are done with the response.
            await networkStream.FlushAsync ().ConfigureAwait ( false );

            if (connectionCloseRequested) {
                break;
            }
        }

        return HttpConnectionState.ConnectionClosed;
    }

    private void Dispose ( bool disposing ) {
        if (!disposedValue) {
            if (disposing) {
                networkStream.Dispose ();
                requestPool.Dispose ();
                responsePool.Dispose ();
            }

            disposedValue = true;
        }
    }

    public void Dispose () {
        Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
