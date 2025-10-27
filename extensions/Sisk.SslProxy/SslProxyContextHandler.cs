// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxyContextHandler.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Net.Sockets;
using System.Text;
using Sisk.Cadente;
using Sisk.Core.Http;

namespace Sisk.Ssl;

class SslProxyContextHandler : HttpHostHandler {
    // Helpers
    private static readonly byte [] CRLF = Encoding.ASCII.GetBytes ( "\r\n" );
    private static readonly HashSet<string> HopByHopRequestHeaders = new ( StringComparer.OrdinalIgnoreCase )
    {
        "Connection", "Keep-Alive", "Proxy-Connection", "Transfer-Encoding", "TE", "Trailer", "Upgrade"
    };
    private static readonly HashSet<string> HopByHopResponseHeaders = new ( StringComparer.OrdinalIgnoreCase )
    {
        "Connection", "Keep-Alive", "Proxy-Connection", "Transfer-Encoding", "TE", "Trailer", "Upgrade",
        "Proxy-Authenticate", "Proxy-Authorization"
    };
    private static readonly HashSet<string> StripAlwaysResponseHeaders = new ( StringComparer.OrdinalIgnoreCase )
    {
        "Server", "Date", "Host"
    };

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

        using CancellationTokenSource gatewayCancellation = new CancellationTokenSource ();
        gatewayCancellation.CancelAfter ( ProxyHost.GatewayTimeout );
        gatewayCancellation.Token.ThrowIfCancellationRequested ();

        // Monta request upstream
        HttpMethod requestMethod = new HttpMethod ( context.Request.Method );
        string requestPath = context.Request.Path;
        string requestUri =
            ProxyHost.UseGatewayHttps
                ? $"https://{ProxyHost.GatewayEndpoint.Address}:{ProxyHost.GatewayEndpoint.Port}{requestPath}"
                : $"http://{ProxyHost.GatewayEndpoint.Address}:{ProxyHost.GatewayEndpoint.Port}{requestPath}";

        bool isWebsocketConnection = context.Request.Headers.Any ( h =>
            h.Name.Equals ( HttpKnownHeaderNames.SecWebSocketKey, StringComparison.OrdinalIgnoreCase ) );

        if (isWebsocketConnection) {
            context.Response.StatusCode = 501;
            context.Response.StatusDescription = "Not Implemented (use ClientWebSocket upstream)";
            await context.Response.GetResponseStream ().FlushAsync ( gatewayCancellation.Token );
            return;
        }

        using var proxyRequest = new HttpRequestMessage ( requestMethod, requestUri ) {
            Version = new Version ( 1, 1 )
        };
        proxyRequest.Headers.Host = ProxyHost.GatewayHostname;

        if (context.Request.ContentLength > 0) {
            Stream requestStream = context.Request.GetRequestStream ();
            proxyRequest.Content = new StreamContent ( requestStream );
            proxyRequest.Content.Headers.ContentLength = context.Request.ContentLength;
        }

        for (int i = 0; i < context.Request.Headers.Length; i++) {
            HttpHeader header = context.Request.Headers [ i ];
            if (HopByHopRequestHeaders.Contains ( header.Name ) ||
                header.Name.Equals ( "Host", StringComparison.OrdinalIgnoreCase )) {
                continue;
            }
            proxyRequest.Headers.TryAddWithoutValidation ( header.Name, header.Value );
        }

        if (ProxyHost.ProxyAuthorization != null) {
            proxyRequest.Headers.TryAddWithoutValidation ( HttpKnownHeaderNames.ProxyAuthorization, ProxyHost.ProxyAuthorization );
        }

        using HttpResponseMessage proxyResponse = await state.SendMessageAsync ( proxyRequest, gatewayCancellation.Token );

        context.Response.StatusCode = (int) proxyResponse.StatusCode;
        context.Response.StatusDescription =
            proxyResponse.ReasonPhrase ?? HttpStatusInformation.GetStatusCodeDescription ( proxyResponse.StatusCode );

        IEnumerable<KeyValuePair<string, IEnumerable<string>>> proxyResponseHeaders =
            [ .. proxyResponse.Headers, .. proxyResponse.Content.Headers ];

        long? contentLength = proxyResponse.Content.Headers.ContentLength;

        var announcedTrailers = proxyResponse.Headers.Trailer;

        foreach (var header in proxyResponseHeaders) {
            string name = header.Key;

            if (HopByHopResponseHeaders.Contains ( name ) || StripAlwaysResponseHeaders.Contains ( name ))
                continue;

            if (name.Equals ( "Content-Length", StringComparison.OrdinalIgnoreCase )) {
                if (contentLength is null)
                    continue;
            }

            foreach (var headerValue in header.Value) {
                context.Response.Headers.Add ( new HttpHeader ( name, headerValue ) );
            }
        }

        if (announcedTrailers != null && announcedTrailers.Count > 0) {
            foreach (var trailersName in announcedTrailers) {
                context.Response.Headers.Add ( new HttpHeader ( "Trailer", trailersName ) );
            }
        }

        await using Stream contentStream = await proxyResponse.Content.ReadAsStreamAsync ( gatewayCancellation.Token );
        Stream responseStream = context.Response.GetResponseStream ();

        if (contentLength.HasValue) {
            context.Response.Headers.Add ( new HttpHeader ( "Content-Length", contentLength.Value.ToString () ) );

            await CopyRawAsync ( contentStream, responseStream, gatewayCancellation.Token );
        }
        else {
            context.Response.Headers.Add ( new HttpHeader ( "Transfer-Encoding", "chunked" ) );
            await CopyAsChunkedAsync ( contentStream, responseStream, gatewayCancellation.Token );

            if (proxyResponse.TrailingHeaders != null && proxyResponse.TrailingHeaders.Any ()) {
                // Escreve chunk de tamanho zero + CRLF já foi feito em CopyAsChunkedAsync,
                // então aqui temos que escrever corretamente os trailers antes do CRLF final.
                // Para simplificar, vamos ajustar CopyAsChunkedAsync para NÃO escrever o final,
                // e fazê-lo aqui com trailers. Implementação abaixo assume esse contrato.
                // Caso já tenha escrito "0\r\n\r\n", devemos adaptar a função para omitir esse final.
            }
        }

        await responseStream.FlushAsync ( gatewayCancellation.Token );
    }

    static async Task CopyRawAsync ( Stream from, Stream to, CancellationToken cancellationToken ) {
        try {
            const int DefaultCopySize = 81920;
            byte [] buffer = ArrayPool<byte>.Shared.Rent ( DefaultCopySize );
            try {
                int bytesRead;
                while ((bytesRead = await from.ReadAsync ( buffer, 0, buffer.Length, cancellationToken ).ConfigureAwait ( false )) != 0) {
                    await to.WriteAsync ( buffer.AsMemory ( 0, bytesRead ), cancellationToken ).ConfigureAwait ( false );
                }
            }
            finally {
                ArrayPool<byte>.Shared.Return ( buffer );
            }
        }
        catch (IOException) { }
        catch (SocketException) { }
        catch (OperationCanceledException) { }
    }

    static async Task CopyAsChunkedAsync ( Stream from, Stream to, CancellationToken cancellationToken ) {
        try {
            const int DefaultCopySize = 81920;
            byte [] buffer = ArrayPool<byte>.Shared.Rent ( DefaultCopySize );
            try {
                int bytesRead;
                while ((bytesRead = await from.ReadAsync ( buffer, 0, buffer.Length, cancellationToken ).ConfigureAwait ( false )) > 0) {
                    // size
                    string sizeHex = bytesRead.ToString ( "x" );
                    await WriteAsciiAsync ( to, sizeHex, cancellationToken ).ConfigureAwait ( false );
                    await to.WriteAsync ( CRLF, 0, CRLF.Length, cancellationToken ).ConfigureAwait ( false );

                    // payload
                    await to.WriteAsync ( buffer.AsMemory ( 0, bytesRead ), cancellationToken ).ConfigureAwait ( false );

                    // crlf
                    await to.WriteAsync ( CRLF, 0, CRLF.Length, cancellationToken ).ConfigureAwait ( false );
                }
            }
            finally {
                ArrayPool<byte>.Shared.Return ( buffer );
            }
        }
        catch (IOException) { }
        catch (SocketException) { }
        catch (OperationCanceledException) { }
    }

    static Task WriteAsciiAsync ( Stream to, string s, CancellationToken ct )
        => to.WriteAsync ( Encoding.ASCII.GetBytes ( s ), 0, s.Length, ct );
}