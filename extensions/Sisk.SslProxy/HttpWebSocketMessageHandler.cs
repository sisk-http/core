// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpWebSocketMessageHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Cadente;

namespace Sisk.Ssl;

class HttpWebSocketMessageHandler : HttpMessageHandler {

    readonly ProxyGateway _proxy;
    readonly HttpHostContext _hostContext;
    readonly SslProxy _host;

    public HttpResponseMessage? ConnectResponse { get; private set; }

    public HttpWebSocketMessageHandler ( HttpHostContext context, ProxyGateway proxy, SslProxy host ) {
        _hostContext = context;
        _proxy = proxy;
        _host = host;
    }

    protected override async Task<HttpResponseMessage> SendAsync ( HttpRequestMessage request, CancellationToken cancellationToken ) {

        request.Headers.Host = _host.GatewayHostname;

        // proxy other headers
        for (int i = 0; i < _hostContext.Request.Headers.Length; i++) {
            HttpHeader header = _hostContext.Request.Headers [ i ];

            if (SslProxyContextHandler.UnnalowedProxiedHeaders.Contains ( header.Name, StringComparer.OrdinalIgnoreCase )) {
                continue;
            }
            else if (request.Headers.Contains ( header.Name )) {
                continue; // do not add already sent headers
            }
            else {
                request.Headers.TryAddWithoutValidation ( header.Name, header.Value );
            }
        }

        var response = await _proxy.SendMessageAsync ( request, cancellationToken );
        ConnectResponse = response;

        return response;
    }
}