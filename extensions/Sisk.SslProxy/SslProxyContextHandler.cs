// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxyContextHandler.cs
// Repository:  https://github.com/sisk-http/core

using System.Buffers;
using System.Globalization;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using Sisk.Cadente;
using Sisk.Core.Http;

namespace Sisk.Ssl;

class SslProxyContextHandler : HttpHostHandler {
    // Helpers
    private static readonly FieldInfo ConnectionStreamField = typeof ( HttpHostContext ).GetField ( "_connectionStream", BindingFlags.Instance | BindingFlags.NonPublic )
        ?? throw new InvalidOperationException ( "Unable to access the underlying client stream." );
    private static readonly byte [] HeaderTerminator = new byte [] { (byte) '\r', (byte) '\n', (byte) '\r', (byte) '\n' };
    private static readonly byte [] CRLF = Encoding.ASCII.GetBytes ( "\r\n" );
    private static readonly HashSet<string> HopByHopRequestHeaders = new ( StringComparer.OrdinalIgnoreCase )
    {
        "Host", "Connection", "Keep-Alive", "Proxy-Connection", "Transfer-Encoding", "TE", "Trailer", "Upgrade"
    };
    private static readonly HashSet<string> HopByHopResponseHeaders = new ( StringComparer.OrdinalIgnoreCase )
    {
        "Connection", "Keep-Alive", "Proxy-Connection", "Transfer-Encoding", "TE", "Trailer", "Upgrade",
        "Proxy-Authenticate", "Proxy-Authorization"
    };
    private static readonly HashSet<string> StripAlwaysRequestHeaders = new ( StringComparer.OrdinalIgnoreCase )
    {
        "Content-Type"
    };
    private static readonly HashSet<string> StripAlwaysResponseHeaders = new ( StringComparer.OrdinalIgnoreCase )
    {
        "Server", "Date", "Host"
    };


    private sealed record UpstreamHandshake ( int StatusCode, string ReasonPhrase, List<HttpHeader> Headers, byte [] RemainingBytes );

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

        bool isWebsocketConnection = context.Request.Headers.Any ( h =>
            h.Name.Equals ( HttpKnownHeaderNames.SecWebSocketKey, StringComparison.OrdinalIgnoreCase ) );

        if (isWebsocketConnection) {
            bool handled = await TryHandleWebSocketAsync ( context, gatewayCancellation ).ConfigureAwait ( false );
            if (handled) {
                return;
            }
        }

        gatewayCancellation.Token.ThrowIfCancellationRequested ();
        await HandleHttpProxyAsync ( context, state, gatewayCancellation.Token ).ConfigureAwait ( false );
    }

    private async Task HandleHttpProxyAsync ( HttpHostContext context, ProxyGateway state, CancellationToken cancellationToken ) {
        HttpMethod requestMethod = new HttpMethod ( context.Request.Method );
        string requestPath = context.Request.Path;
        string requestUri =
            ProxyHost.UseGatewayHttps
                ? $"https://{ProxyHost.GatewayEndpoint.Address}:{ProxyHost.GatewayEndpoint.Port}{requestPath}"
                : $"http://{ProxyHost.GatewayEndpoint.Address}:{ProxyHost.GatewayEndpoint.Port}{requestPath}";

        using var proxyRequest = new HttpRequestMessage ( requestMethod, requestUri ) {
            Version = new Version ( 1, 1 )
        };

        proxyRequest.Headers.Host = ProxyHost.GatewayHostname;

        if (context.Request.ContentLength > 0) {
            Stream requestStream = context.Request.GetRequestStream (); // peeks original body stream to forward upstream
            proxyRequest.Content = new StreamContent ( requestStream );
            proxyRequest.Content.Headers.ContentLength = context.Request.ContentLength;
        }

        for (int i = 0; i < context.Request.Headers.Length; i++) {
            HttpHeader header = context.Request.Headers [ i ];
            if (HopByHopRequestHeaders.Contains ( header.Name ) || StripAlwaysRequestHeaders.Contains ( header.Name )) {
                continue;
            }
            if (!proxyRequest.Headers.TryAddWithoutValidation ( header.Name, header.Value )) {
                proxyRequest.Content?.Headers.TryAddWithoutValidation ( header.Name, header.Value );
            }
        }

        if (ProxyHost.ProxyAuthorization != null) {
            proxyRequest.Headers.TryAddWithoutValidation ( HttpKnownHeaderNames.ProxyAuthorization, ProxyHost.ProxyAuthorization );
        }

        using HttpResponseMessage proxyResponse = await state.SendMessageAsync ( proxyRequest, cancellationToken );

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

        await using Stream contentStream = await proxyResponse.Content.ReadAsStreamAsync ( cancellationToken );
        Stream responseStream = context.Response.GetResponseStream ();

        if (contentLength.HasValue) {
            context.Response.Headers.Add ( new HttpHeader ( "Content-Length", contentLength.Value.ToString ( CultureInfo.InvariantCulture ) ) );

            await CopyRawAsync ( contentStream, responseStream, cancellationToken );
        }
        else {
            context.Response.Headers.Add ( new HttpHeader ( "Transfer-Encoding", "chunked" ) );
            await CopyAsChunkedAsync ( contentStream, responseStream, cancellationToken );

            if (proxyResponse.TrailingHeaders != null && proxyResponse.TrailingHeaders.Any ()) {
                // Trailing headers are handled by the caller when chunked payload is requested.
            }
        }

        await responseStream.FlushAsync ( cancellationToken );
    }

    private async Task<bool> TryHandleWebSocketAsync ( HttpHostContext context, CancellationTokenSource gatewayCancellation ) {
        using CancellationTokenSource handshakeCancellation = CancellationTokenSource.CreateLinkedTokenSource ( gatewayCancellation.Token, context.Client.DisconnectToken );
        CancellationToken handshakeToken = handshakeCancellation.Token;

        TcpClient gatewayClient = new TcpClient { NoDelay = true };
        Stream? upstreamStream = null;
        SslStream? sslStream = null;

        try {
            await gatewayClient.ConnectAsync ( ProxyHost.GatewayEndpoint, handshakeToken ).ConfigureAwait ( false );
            upstreamStream = gatewayClient.GetStream ();

            string targetHost = ResolveGatewayHostHeader ( context );
            if (ProxyHost.UseGatewayHttps) {
                sslStream = new SslStream ( upstreamStream, leaveInnerStreamOpen: false, ( _, _, _, _ ) => true );
                await sslStream.AuthenticateAsClientAsync ( targetHost ).ConfigureAwait ( false );
                upstreamStream = sslStream;
            }

            byte [] handshakeBuffer = BuildWebSocketRequestBuffer ( context, targetHost );
            await upstreamStream!.WriteAsync ( handshakeBuffer, handshakeToken ).ConfigureAwait ( false );
            await upstreamStream.FlushAsync ( handshakeToken ).ConfigureAwait ( false );

            UpstreamHandshake handshake = await ReadWebSocketHandshakeAsync ( upstreamStream, handshakeToken ).ConfigureAwait ( false );

            if (handshake.StatusCode != 101) {
                return false;
            }

            ApplyHandshakeToContext ( context, handshake );
            Stream clientStream = context.Response.GetResponseStream ();
            await clientStream.FlushAsync ( handshakeToken ).ConfigureAwait ( false );

            gatewayCancellation.CancelAfter ( Timeout.InfiniteTimeSpan );

            await PipeBidirectionalAsync ( clientStream, upstreamStream, handshake.RemainingBytes, context.Client.DisconnectToken ).ConfigureAwait ( false );
            return true;
        }
        catch (OperationCanceledException) when (gatewayCancellation.IsCancellationRequested || context.Client.DisconnectToken.IsCancellationRequested) {
            PrepareErrorResponse ( context, 504, "Gateway Timeout", "WebSocket handshake timed out." );
            return true;
        }
        catch (Exception ex) {
            PrepareErrorResponse ( context, 502, "Bad Gateway", $"WebSocket proxy error: {ex.Message}" );
            return true;
        }
        finally {
            try {
                sslStream?.Dispose ();
            }
            catch { }

            try {
                upstreamStream?.Dispose ();
            }
            catch { }

            gatewayClient.Dispose ();
        }
    }

    private string ResolveGatewayHostHeader ( HttpHostContext context ) {
        if (!string.IsNullOrEmpty ( ProxyHost.GatewayHostname )) {
            return ProxyHost.GatewayHostname;
        }

        HttpHeader originalHostHeader = context.Request.Headers.FirstOrDefault ( h =>
            h.Name.Equals ( HttpKnownHeaderNames.Host, StringComparison.OrdinalIgnoreCase ) );

        if (!originalHostHeader.IsEmpty) {
            return originalHostHeader.Value;
        }

        bool isDefaultPort = (!ProxyHost.UseGatewayHttps && ProxyHost.GatewayEndpoint.Port == 80) ||
                             (ProxyHost.UseGatewayHttps && ProxyHost.GatewayEndpoint.Port == 443);

        string address = ProxyHost.GatewayEndpoint.Address.ToString ();
        return isDefaultPort ? address : $"{address}:{ProxyHost.GatewayEndpoint.Port}";
    }

    private byte [] BuildWebSocketRequestBuffer ( HttpHostContext context, string hostHeader ) {
        StringBuilder builder = new StringBuilder ( 256 );
        builder.Append ( context.Request.Method )
               .Append ( ' ' )
               .Append ( context.Request.Path )
               .Append ( " HTTP/1.1\r\n" );

        builder.Append ( "Host: " )
               .Append ( hostHeader )
               .Append ( "\r\n" );

        for (int i = 0; i < context.Request.Headers.Length; i++) {
            HttpHeader header = context.Request.Headers [ i ];
            if (header.Name.Equals ( HttpKnownHeaderNames.Host, StringComparison.OrdinalIgnoreCase )) {
                continue;
            }

            if (header.Name.Equals ( HttpKnownHeaderNames.ProxyAuthorization, StringComparison.OrdinalIgnoreCase )) {
                continue;
            }

            builder.Append ( header.Name )
                   .Append ( ": " )
                   .Append ( header.Value )
                   .Append ( "\r\n" );
        }

        if (!string.IsNullOrEmpty ( ProxyHost.ProxyAuthorization )) {
            builder.Append ( HttpKnownHeaderNames.ProxyAuthorization )
                   .Append ( ": " )
                   .Append ( ProxyHost.ProxyAuthorization )
                   .Append ( "\r\n" );
        }

        builder.Append ( "\r\n" );
        return Encoding.ASCII.GetBytes ( builder.ToString () );
    }

    private async Task<UpstreamHandshake> ReadWebSocketHandshakeAsync ( Stream upstreamStream, CancellationToken cancellationToken ) {
        using MemoryStream headerBuffer = new MemoryStream ( 512 );
        byte [] buffer = ArrayPool<byte>.Shared.Rent ( 4096 );

        try {
            while (true) {
                int read = await upstreamStream.ReadAsync ( buffer.AsMemory ( 0, buffer.Length ), cancellationToken ).ConfigureAwait ( false );
                if (read <= 0) {
                    throw new IOException ( "Unexpected end of stream while reading handshake response." );
                }

                headerBuffer.Write ( buffer, 0, read );

                if (TryLocateHeaderTerminator ( headerBuffer, out int headerLength )) {
                    byte [] rawBytes = headerBuffer.ToArray ();
                    int headerTextLength = headerLength - HeaderTerminator.Length;
                    string headerText = Encoding.ASCII.GetString ( rawBytes, 0, headerTextLength );

                    string [] lines = headerText.Split ( new [] { "\r\n" }, StringSplitOptions.None );
                    if (lines.Length == 0 || string.IsNullOrWhiteSpace ( lines [ 0 ] )) {
                        throw new InvalidOperationException ( "Invalid handshake response from upstream server." );
                    }

                    string statusLine = lines [ 0 ];
                    string [] statusParts = statusLine.Split ( ' ', 3, StringSplitOptions.RemoveEmptyEntries );
                    if (statusParts.Length < 2 || !int.TryParse ( statusParts [ 1 ], out int statusCode )) {
                        throw new InvalidOperationException ( "Unable to parse status code from upstream handshake." );
                    }

                    string reasonPhrase = statusParts.Length >= 3
                        ? statusParts [ 2 ]
                        : HttpStatusInformation.GetStatusCodeDescription ( (HttpStatusCode) statusCode );

                    List<HttpHeader> headers = new List<HttpHeader> ( lines.Length - 1 );
                    for (int i = 1; i < lines.Length; i++) {
                        string line = lines [ i ];
                        if (string.IsNullOrEmpty ( line )) {
                            continue;
                        }

                        int separatorIndex = line.IndexOf ( ':' );
                        if (separatorIndex <= 0) {
                            continue;
                        }

                        string name = line.Substring ( 0, separatorIndex );
                        string value = line.Substring ( separatorIndex + 1 ).Trim ();

                        if (StripAlwaysResponseHeaders.Contains ( name )) {
                            continue;
                        }

                        headers.Add ( new HttpHeader ( name, value ) );
                    }

                    int remainingCount = rawBytes.Length - headerLength;
                    byte [] remaining = remainingCount > 0 ? rawBytes [ headerLength.. ] : Array.Empty<byte> ();

                    return new UpstreamHandshake ( statusCode, reasonPhrase, headers, remaining );
                }
            }
        }
        finally {
            ArrayPool<byte>.Shared.Return ( buffer );
        }
    }

    private static bool TryLocateHeaderTerminator ( MemoryStream buffer, out int headerLength ) {
        ReadOnlySpan<byte> span = buffer.GetBuffer ().AsSpan ( 0, (int) buffer.Length );
        int index = span.IndexOf ( HeaderTerminator );
        if (index >= 0) {
            headerLength = index + HeaderTerminator.Length;
            return true;
        }

        headerLength = -1;
        return false;
    }

    private void ApplyHandshakeToContext ( HttpHostContext context, UpstreamHandshake handshake ) {
        context.Response.StatusCode = handshake.StatusCode;
        context.Response.StatusDescription = handshake.ReasonPhrase;
        context.Response.SendChunked = false;
        context.Response.Headers = new List<HttpHeader> ( handshake.Headers );
    }

    private static async Task PipeBidirectionalAsync ( Stream clientStream, Stream upstreamStream, byte [] initialUpstreamData, CancellationToken disconnectToken ) {
        using CancellationTokenSource relayCancellation = CancellationTokenSource.CreateLinkedTokenSource ( disconnectToken );
        CancellationToken relayToken = relayCancellation.Token;

        Task upstreamToClient = RelayStreamAsync ( upstreamStream, clientStream, relayToken, initialUpstreamData );
        Task clientToUpstream = RelayStreamAsync ( clientStream, upstreamStream, relayToken, Array.Empty<byte> () );

        Task completed = await Task.WhenAny ( upstreamToClient, clientToUpstream ).ConfigureAwait ( false );
        _ = completed;
        relayCancellation.Cancel ();

        await Task.WhenAll (
            SuppressExceptionsAsync ( upstreamToClient ),
            SuppressExceptionsAsync ( clientToUpstream ) ).ConfigureAwait ( false );
    }

    private static async Task RelayStreamAsync ( Stream source, Stream destination, CancellationToken cancellationToken, byte [] initialPayload ) {
        byte [] buffer = ArrayPool<byte>.Shared.Rent ( 81920 );
        try {
            if (initialPayload.Length > 0) {
                await destination.WriteAsync ( initialPayload, cancellationToken ).ConfigureAwait ( false );
                await destination.FlushAsync ( cancellationToken ).ConfigureAwait ( false );
            }

            while (true) {
                int read = await source.ReadAsync ( buffer.AsMemory ( 0, buffer.Length ), cancellationToken ).ConfigureAwait ( false );
                if (read <= 0) {
                    break;
                }

                await destination.WriteAsync ( buffer.AsMemory ( 0, read ), cancellationToken ).ConfigureAwait ( false );
                await destination.FlushAsync ( cancellationToken ).ConfigureAwait ( false );
            }
        }
        catch (OperationCanceledException) { }
        catch (IOException) { }
        catch (SocketException) { }
        catch (ObjectDisposedException) { }
        finally {
            ArrayPool<byte>.Shared.Return ( buffer );
        }
    }

    private static async Task SuppressExceptionsAsync ( Task task ) {
        try {
            await task.ConfigureAwait ( false );
        }
        catch { }
    }

    private static void PrepareErrorResponse ( HttpHostContext context, int statusCode, string statusDescription, string message ) {
        byte [] bodyBytes = Encoding.UTF8.GetBytes ( message );
        context.Response.StatusCode = statusCode;
        context.Response.StatusDescription = statusDescription;
        context.Response.SendChunked = false;
        context.Response.Headers = new List<HttpHeader>
        {
            new HttpHeader ( HttpKnownHeaderNames.ContentType, "text/plain; charset=utf-8" ),
            new HttpHeader ( HttpKnownHeaderNames.ContentLength, bodyBytes.Length.ToString ( CultureInfo.InvariantCulture ) )
        };

        context.Response.ResponseStream = new MemoryStream ( bodyBytes );
        context.KeepAlive = false;
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