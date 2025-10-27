// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ProxyGateway.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;

namespace Sisk.Ssl;

class ProxyGateway : IDisposable {
    HttpClient client;
    HttpClientHandler httpHandler;

    public IPEndPoint GatewayEndpoint { get; }

    public ProxyGateway ( IPEndPoint endpoint ) {
        httpHandler = new HttpClientHandler () {
            AllowAutoRedirect = false,
            AutomaticDecompression = DecompressionMethods.None,
            ServerCertificateCustomValidationCallback = ( message, cert, chain, errors ) => {
                return true;
            },
        };

        client = new HttpClient ( httpHandler );
        GatewayEndpoint = endpoint;
    }

    public Task<HttpResponseMessage> SendMessageAsync ( HttpRequestMessage requestMessage, CancellationToken cancellationToken = default ) {
        return client.SendAsync ( requestMessage, HttpCompletionOption.ResponseHeadersRead, cancellationToken );
    }

    public void Dispose () {
        httpHandler.Dispose ();
        client.Dispose ();
    }
}
