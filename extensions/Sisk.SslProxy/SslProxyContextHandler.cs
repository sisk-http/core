// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SslProxyContextHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Cadente;

namespace Sisk.Ssl;

class SslProxyContextHandler : HttpHostHandler {

    readonly string [] UnnalowedProxiedHeaders = [ "Server", "Date", "Host", "Connection" ];
    public SslProxy Proxy { get; }

    public SslProxyContextHandler ( SslProxy proxy ) {
        Proxy = proxy;
    }

    public override Task OnClientConnectedAsync ( HttpHost host, HttpHostClient client ) {
        client.State = new ProxyGateway ( Proxy.GatewayEndpoint );
        return Task.CompletedTask;
    }

    public override Task OnClientDisconnectedAsync ( HttpHost host, HttpHostClient client ) {
        (client.State as IDisposable)?.Dispose ();
        return Task.CompletedTask;
    }

    public override async Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
        ProxyGateway state = (ProxyGateway) context.Client.State!;

        HttpMethod requestMethod = new HttpMethod ( context.Request.Method );
        string requestPath = context.Request.Path;
        string requestUri = $"http://{Proxy.GatewayEndpoint.Address}:{Proxy.GatewayEndpoint.Port}{requestPath}";

        bool isWebsocketConnection = context.Request.Headers.Any ( c => c.Name.Equals ( "Sec-WebSocket-Key", StringComparison.OrdinalIgnoreCase ) );
        if (isWebsocketConnection) {
            // handle ws connection
        }

        HttpRequestMessage proxyRequest = new HttpRequestMessage ( requestMethod, requestUri );
        proxyRequest.Headers.Host = Proxy.GatewayHostname;

        if (context.Request.ContentLength > 0) {
            Stream requestStream = context.Request.GetRequestStream ();
            proxyRequest.Content = new StreamContent ( requestStream );
        }

        for (int i = 0; i < context.Request.Headers.Length; i++) {
            HttpHeader header = context.Request.Headers [ i ];

            if (UnnalowedProxiedHeaders.Contains ( header.Name, StringComparer.OrdinalIgnoreCase )) {
                continue;
            }
            else if (header.Name.Equals ( "Host", StringComparison.OrdinalIgnoreCase ) && Proxy.GatewayHostname != null) {

            }
            else {
                proxyRequest.Headers.TryAddWithoutValidation ( header.Name, header.Value );
            }
        }

        HttpResponseMessage proxyResponse = await state.SendMessageAsync ( proxyRequest );

        context.Response.StatusCode = (int) proxyResponse.StatusCode;
        context.Response.StatusDescription = proxyResponse.ReasonPhrase ?? "Unknown";
        context.Response.ResponseStream = await proxyResponse.Content.ReadAsStreamAsync ();

        IEnumerable<KeyValuePair<string, IEnumerable<string>>> proxyResponseHeaders =
            [ .. proxyResponse.Headers, .. proxyResponse.Content.Headers ];

        foreach (var header in proxyResponseHeaders) {
            if (UnnalowedProxiedHeaders.Contains ( header.Key, StringComparer.OrdinalIgnoreCase ))
                continue;

            foreach (var headerValue in header.Value)
                context.Response.Headers.Add ( new HttpHeader ( header.Key, headerValue ) );
        }
    }
}
