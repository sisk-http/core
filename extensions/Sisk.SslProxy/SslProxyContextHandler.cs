// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxyContextHandler.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Diagnostics;
using System.Net.Sockets;
using System.Reflection;
using Sisk.Cadente;
using Sisk.Core.Http;

namespace Sisk.Ssl;

class SslProxyContextHandler : HttpHostHandler {

    internal readonly static byte [] ChunkedEOF = "0\r\n\r\n"u8.ToArray ();
    internal readonly static string [] UnnalowedProxiedHeaders = [ "Server", "Date", "Host", "Connection" ];
    readonly SslProxy ProxyHost;

    public SslProxyContextHandler ( SslProxy proxy ) {
        ProxyHost = proxy;
    }

    public override Task OnClientConnectedAsync ( HttpHost host, HttpHostClient client ) {
        client.State = new ProxyGateway ( ProxyHost.GatewayEndpoint );
        return Task.CompletedTask;
    }

    public override Task OnClientDisconnectedAsync ( HttpHost host, HttpHostClient client ) {
        (client.State as IDisposable)?.Dispose ();
        return Task.CompletedTask;
    }

    public override async Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
        ProxyGateway state = (ProxyGateway) context.Client.State!;

        CancellationTokenSource gatewayCancellation = new CancellationTokenSource ();
        gatewayCancellation.CancelAfter ( ProxyHost.GatewayTimeout );

        HttpMethod requestMethod = new HttpMethod ( context.Request.Method );
        string requestPath = context.Request.Path;
        string requestUri =
            ProxyHost.UseGatewayHttps ?
                $"https://{ProxyHost.GatewayEndpoint.Address}:{ProxyHost.GatewayEndpoint.Port}{requestPath}" :
                $"http://{ProxyHost.GatewayEndpoint.Address}:{ProxyHost.GatewayEndpoint.Port}{requestPath}";

        bool isWebsocketConnection = context.Request.Headers.Any ( c => c.Name.Equals ( HttpKnownHeaderNames.SecWebSocketKey, StringComparison.OrdinalIgnoreCase ) );

        HttpRequestMessage proxyRequest = new HttpRequestMessage ( requestMethod, requestUri );
        proxyRequest.Headers.Host = ProxyHost.GatewayHostname;

        if (context.Request.ContentLength > 0) {
            Stream requestStream = context.Request.GetRequestStream ();
            proxyRequest.Content = new StreamContent ( requestStream );
            proxyRequest.Content.Headers.ContentLength = context.Request.ContentLength;
        }

        for (int i = 0; i < context.Request.Headers.Length; i++) {
            HttpHeader header = context.Request.Headers [ i ];

            if (UnnalowedProxiedHeaders.Contains ( header.Name, StringComparer.OrdinalIgnoreCase )) {
                continue;
            }
            else {
                proxyRequest.Headers.TryAddWithoutValidation ( header.Name, header.Value );
            }
        }

        if (isWebsocketConnection) {
            proxyRequest.Headers.Connection.Add ( "Upgrade" );
        }
        if (ProxyHost.ProxyAuthorization != null) {
            proxyRequest.Headers.TryAddWithoutValidation ( HttpKnownHeaderNames.ProxyAuthorization, ProxyHost.ProxyAuthorization );
        }

        HttpResponseMessage proxyResponse = await state.SendMessageAsync ( proxyRequest, gatewayCancellation.Token );

        context.Response.StatusCode = (int) proxyResponse.StatusCode;
        context.Response.StatusDescription = proxyResponse.ReasonPhrase ?? HttpStatusInformation.GetStatusCodeDescription ( proxyResponse.StatusCode );

        IEnumerable<KeyValuePair<string, IEnumerable<string>>> proxyResponseHeaders =
            [ .. proxyResponse.Headers, .. proxyResponse.Content.Headers ];

        foreach (var header in proxyResponseHeaders) {
            if (UnnalowedProxiedHeaders.Contains ( header.Key, StringComparer.OrdinalIgnoreCase ))
                continue;

            foreach (var headerValue in header.Value)
                context.Response.Headers.Add ( new HttpHeader ( header.Key, headerValue ) );
        }

        Stream gatewayStream = ResolveRawResponseStream ( await proxyResponse.Content.ReadAsStreamAsync (), out bool isChunked );

        if (isWebsocketConnection) {

            context.Response.Headers.Add ( new HttpHeader ( HttpKnownHeaderNames.Connection, "Upgrade" ) );
            Stream responseStream = context.Response.GetResponseStream ();

            Task copyToProxy = CopyToAsyncUnchecked ( responseStream, gatewayStream, eof: null, gatewayCancellation.Token );
            Task copyFromProxy = CopyToAsyncUnchecked ( gatewayStream, responseStream, eof: null, gatewayCancellation.Token );

            await Task.WhenAny ( copyToProxy, copyFromProxy );
            await gatewayCancellation.CancelAsync ();
            ;

        }
        else {
            Stream responseStream = context.Response.GetResponseStream ();

            byte []? eof = isChunked ? ChunkedEOF : null;

            await CopyToAsyncUnchecked ( gatewayStream, responseStream, eof, gatewayCancellation.Token );
            ;

        }
    }

    static async Task CopyToAsyncUnchecked ( Stream from, Stream to, byte []? eof, CancellationToken cancellationToken ) {
        try {
            if (!from.CanRead) {
                if (to.CanWrite) {
                    throw new Exception ( "@to is not writable" );
                }

                throw new Exception ( "@from is not readable" );
            }

            const int DefaultCopySize = 81920;
            Memory<byte> eofMemory = eof;

            byte [] buffer = ArrayPool<byte>.Shared.Rent ( DefaultCopySize );
            var bufferMemory = new Memory<byte> ( buffer );
            try {
                int bytesRead;
                while ((bytesRead = await from.ReadAsync ( bufferMemory, cancellationToken ).ConfigureAwait ( false )) != 0) {

                    var bufferedResult = bufferMemory [ 0..bytesRead ];
                    await to.WriteAsync ( bufferedResult, cancellationToken ).ConfigureAwait ( false );

                    if (eof != null && bufferedResult [ Index.FromEnd ( eofMemory.Length ).. ].Span.SequenceEqual ( eofMemory.Span )) {
                        break;
                    }
                }
            }
            finally {
                ArrayPool<byte>.Shared.Return ( buffer );
            }
        }
        catch (IOException) {
        }
        catch (SocketException) {
        }
        catch (OperationCanceledException) {
        }
    }

    static FieldInfo? ResolveRawResponseStream__f_connection;
    static FieldInfo? ResolveRawResponseStream__f_stream;

    static Stream ResolveRawResponseStream ( Stream gatewayStream, out bool isChunked ) {

        /*
            The HttpClient places the response stream into a Stream to deserialize the chunked encoding
                and retrieve the deserialized content.

            The problem with this is that the proxy sends the transfer-encoding: chunked header and a
                non-chunked response, which causes a deserialization issue.

            The code below uses reflection to get the underlying NetworkStream of the connection between
                the proxy and the gateway to send a raw response without data decompression,
                to the proxy client.
        */

        Type typeName = gatewayStream.GetType ();
        if (typeName.FullName == "System.Net.Http.HttpConnection+ChunkedEncodingReadStream") {

            ResolveRawResponseStream__f_connection ??= typeName.GetField ( "_connection", BindingFlags.NonPublic | BindingFlags.Instance );
            Debug.Assert ( ResolveRawResponseStream__f_connection != null );

            object? _connection = ResolveRawResponseStream__f_connection.GetValue ( gatewayStream );
            Debug.Assert ( _connection != null );

            ResolveRawResponseStream__f_stream ??= _connection.GetType ()?.GetField ( "_stream", BindingFlags.NonPublic | BindingFlags.Instance );
            Debug.Assert ( ResolveRawResponseStream__f_stream != null );

            object? stream = ResolveRawResponseStream__f_stream.GetValue ( _connection );
            Debug.Assert ( stream != null );

            isChunked = true;

            return (Stream) stream;
        }
        else {
            isChunked = false;

            return gatewayStream;
        }
    }
}
