// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteSecurityTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Text;
using Sisk.Cadente;
using Sisk.Core.Helpers;

namespace tests.Tests;

[TestClass]
public sealed class CadenteSecurityTests {
    private static readonly TimeSpan HeaderTimeout = TimeSpan.FromMilliseconds ( 200 );
    private static readonly TimeSpan CloseWaitTimeout = TimeSpan.FromSeconds ( 2 );
    private static readonly TimeSpan PollDelay = TimeSpan.FromMilliseconds ( 25 );
    private const int SocketPollMicroseconds = 1000;

    [TestMethod]
    public async Task HeaderParsingTimeout_ClosesIdleConnection () {
        int port = GetFreePort ();
        using var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = new EmptyHandler ()
        };
        host.TimeoutManager.HeaderParsingTimeout = HeaderTimeout;
        host.TimeoutManager.ClientReadTimeout = TimeSpan.FromSeconds ( 5 );
        host.Start ();

        using var client = new TcpClient ();
        using var connectCts = new CancellationTokenSource ( CloseWaitTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, connectCts.Token );

        bool closed = await WaitUntilClosedAsync ( client.Client, CloseWaitTimeout );

        Assert.IsTrue ( closed, "Cadente should close an idle connection when header parsing exceeds HeaderParsingTimeout." );
    }

    [TestMethod]
    public async Task SecureConnectionState_IsSetForTlsConnections () {
        int port = GetFreePort ();
        var handler = new SecureStateHandler ();
        using var certificate = CertificateHelper.CreateDevelopmentCertificate ( "localhost" );
        using var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = handler,
            HttpsOptions = new HttpsOptions ( certificate )
        };
        host.TimeoutManager.SslHandshakeTimeout = TimeSpan.FromSeconds ( 5 );
        host.Start ();

        using var client = new TcpClient ();
        using var connectCts = new CancellationTokenSource ( CloseWaitTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, connectCts.Token );

        await using var sslStream = new SslStream (
            client.GetStream (),
            leaveInnerStreamOpen: false,
            userCertificateValidationCallback: static ( _, _, _, _ ) => true );

        using var sslCts = new CancellationTokenSource ( CloseWaitTimeout );
        await sslStream.AuthenticateAsClientAsync ( new SslClientAuthenticationOptions {
            TargetHost = "localhost",
            EnabledSslProtocols = SslProtocols.Tls12 | SslProtocols.Tls13
        }, sslCts.Token );

        byte [] requestBytes = Encoding.ASCII.GetBytes (
            $"GET / HTTP/1.1\r\n" +
            $"Host: localhost:{port}\r\n" +
            "Connection: close\r\n" +
            "\r\n" );
        await sslStream.WriteAsync ( requestBytes, sslCts.Token );
        await sslStream.FlushAsync ( sslCts.Token );

        bool isSecure = await handler.SecureState.Task.WaitAsync ( CloseWaitTimeout );

        Assert.IsTrue ( isSecure, "Cadente should expose TLS transport state independently of client certificates." );
    }

    private static int GetFreePort () {
        using var listener = new TcpListener ( IPAddress.Loopback, 0 );
        listener.Start ();

        return ((IPEndPoint) listener.LocalEndpoint).Port;
    }

    private static async Task<bool> WaitUntilClosedAsync ( Socket socket, TimeSpan timeout ) {
        DateTime deadline = DateTime.UtcNow + timeout;

        while (DateTime.UtcNow < deadline) {
            try {
                if (socket.Poll ( microSeconds: SocketPollMicroseconds, SelectMode.SelectRead ) && socket.Available == 0)
                    return true;
            }
            catch (SocketException) {
                return true;
            }
            catch (ObjectDisposedException) {
                return true;
            }

            await Task.Delay ( PollDelay );
        }

        return false;
    }

    private sealed class EmptyHandler : HttpHostHandler { }

    private sealed class SecureStateHandler : HttpHostHandler {
        public TaskCompletionSource<bool> SecureState { get; } = new ( TaskCreationOptions.RunContinuationsAsynchronously );

        public override Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
            SecureState.TrySetResult ( context.Client.IsSecureConnection );
            context.Response.Headers.Set ( new HttpHeader ( "Content-Length", "0" ) );
            return Task.CompletedTask;
        }
    }
}