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
    public const int RESERVED_BUFFER_SIZE = 8192;

    internal readonly Stream networkStream;
    internal byte [] sharedPool;

    public HttpConnection ( HttpHostClient client, Stream connectionStream, HttpHost host, IPEndPoint endpoint ) {
        _client = client;
        _host = host;
        _endpoint = endpoint;

        networkStream = connectionStream;
        sharedPool = ArrayPool<byte>.Shared.Rent ( RESERVED_BUFFER_SIZE );
    }

    [MethodImpl ( MethodImplOptions.AggressiveOptimization )]
    public async Task<HttpConnectionState> HandleConnectionEventsAsync () {
        bool connectionCloseRequested = false;

        while (!disposedValue) {

            using HttpRequestBase? nextRequest = await HttpRequestReader.TryReadHttpRequestAsync ( networkStream ).ConfigureAwait ( false );

            if (nextRequest is null) {
                return HttpConnectionState.ConnectionClosed;
            }

            HttpHostContext managedSession = new HttpHostContext ( _host, this, nextRequest, _client );
            await _host.InvokeContextCreated ( managedSession ).ConfigureAwait ( false );

            if (!managedSession.KeepAlive || !nextRequest.CanKeepAlive) {
                connectionCloseRequested = true;
                managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.Connection, "close" ) );
            }

            if (managedSession.ResponseHeadersAlreadySent == false) {
                managedSession.Response.Headers.Set ( new HttpHeader ( HttpHeaderName.ContentLength, "0" ) );

                if (!await managedSession.WriteHttpResponseHeadersAsync ())
                    return HttpConnectionState.ResponseWriteException;
            }

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
                ArrayPool<byte>.Shared.Return ( sharedPool );
            }

            disposedValue = true;
        }
    }

    public void Dispose () {
        Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
