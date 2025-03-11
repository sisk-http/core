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

    public ProxyGateway ( IPEndPoint endpoint ) {
        client = new HttpClient ();
    }

    public Task<HttpResponseMessage> SendMessageAsync ( HttpRequestMessage requestMessage ) {
        return client.SendAsync ( requestMessage, HttpCompletionOption.ResponseHeadersRead );
    }

    public void Dispose () {
        client.Dispose ();
    }
}
