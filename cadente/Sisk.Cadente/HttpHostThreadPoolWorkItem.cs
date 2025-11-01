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

namespace Sisk.Cadente;
internal class HttpHostThreadPoolWorkItem : IThreadPoolWorkItem {

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

            connectionStream.ReadTimeout = clientReadTimeoutMs;
            connectionStream.WriteTimeout = clientWriteTimeoutMs;

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

                        var message = GetBadRequestMessage ( "SSL/TLS Handshake failed." );
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

    byte [] GetBadRequestMessage ( string message ) {
        string content = $"""
            <HTML>
                <HEAD>
                    <TITLE>400 - Bad Request</TITLE>
                </HEAD>
                <BODY>
                    <H1>400 - Bad Request</H1>
                    <P>{message}</P>
                    <HR>
                    <P><EM>Cadente</EM></P>
                </BODY>
            </HTML>
            """;

        string html =
            $"HTTP/1.1 400 Bad Request\r\n" +
            $"Content-Type: text/html\r\n" +
            $"Content-Length: {content.Length}\r\n" +
            $"Connection: close\r\n" +
            $"\r\n" +
            content;

        return Encoding.ASCII.GetBytes ( html );
    }
}
