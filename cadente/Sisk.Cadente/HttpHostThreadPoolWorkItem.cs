// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpHostThreadPoolWorkItem.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using Sisk.Cadente.HttpSerializer;

namespace Sisk.Cadente;
internal class HttpHostThreadPoolWorkItem : IThreadPoolWorkItem {

    static byte [] buffer = new byte [ 1024 ];
    private HttpHost host;
    private TcpClient client;

    public HttpHostThreadPoolWorkItem ( HttpHost host, TcpClient client ) {
        this.host = host;
        this.client = client;
    }

    public async void Execute () {
        try {
            int clientReadTimeoutMs = (int) host.TimeoutManager.ClientReadTimeout.TotalMilliseconds;
            int clientWriteTimeoutMs = (int) host.TimeoutManager.ClientWriteTimeout.TotalMilliseconds;

            client.ReceiveTimeout = clientReadTimeoutMs;
            client.SendTimeout = clientWriteTimeoutMs;

            if (host.Handler is null)
                return;

            Stream connectionStream;
            using Stream clientStream = client.GetStream ();

            if (host.HttpsOptions is not null) {
                connectionStream = new SslStream ( clientStream, leaveInnerStreamOpen: false );
            }
            else {
                connectionStream = clientStream;
            }

            IPEndPoint clientEndpoint = (IPEndPoint) client.Client.RemoteEndPoint!;
            HttpHostClient hostClient = new HttpHostClient ( clientEndpoint, CancellationToken.None );

            using (HttpConnection connection = new HttpConnection ( hostClient, connectionStream, host, clientEndpoint )) {

                if (connectionStream is SslStream sslStream) {
                    try {
                        await sslStream.AuthenticateAsServerAsync (
                            serverCertificate: host.HttpsOptions!.ServerCertificate,
                            clientCertificateRequired: host.HttpsOptions.ClientCertificateRequired,
                            checkCertificateRevocation: host.HttpsOptions.CheckCertificateRevocation,
                            enabledSslProtocols: host.HttpsOptions.AllowedProtocols ).ConfigureAwait ( false );

                        hostClient.ClientCertificate = sslStream.RemoteCertificate;
                    }
                    catch (Exception) {

                        var message = HttpResponseSerializer.GetRawMessage ( "SSL/TLS Handshake failed.", 400, "Bad Request" );
                        await clientStream.WriteAsync ( message, 0, message.Length );
                        return;
                    }
                }

                await host.Handler.OnClientConnectedAsync ( host, hostClient ).ConfigureAwait ( false );
                await connection.HandleConnectionEventsAsync ().ConfigureAwait ( false );
                await host.Handler.OnClientDisconnectedAsync ( host, hostClient ).ConfigureAwait ( false );
            }
        }
        finally {
            client.Dispose ();
        }
    }
}
