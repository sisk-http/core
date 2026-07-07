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

        await using var stream = client.GetStream ();
        bool closed = await WaitUntilClosedAsync ( stream, CloseWaitTimeout );

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

    [TestMethod]
    public async Task HttpsListener_RedirectsPlainHttpRequest () {
        int port = GetFreePort ();
        using var certificate = CertificateHelper.CreateDevelopmentCertificate ( "localhost" );
        using var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = new EmptyHandler (),
            HttpsOptions = new HttpsOptions ( certificate )
        };
        host.TimeoutManager.SslHandshakeTimeout = CloseWaitTimeout;
        host.Start ();

        using var client = new TcpClient ();
        using var connectCts = new CancellationTokenSource ( CloseWaitTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, connectCts.Token );

        await using var stream = client.GetStream ();
        using var ioCts = new CancellationTokenSource ( CloseWaitTimeout );
        byte [] requestBytes = Encoding.ASCII.GetBytes (
            $"GET /hello?x=1 HTTP/1.1\r\n" +
            $"Host: localhost:{port}\r\n" +
            "Connection: close\r\n" +
            "\r\n" );
        await stream.WriteAsync ( requestBytes, ioCts.Token );
        await stream.FlushAsync ( ioCts.Token );

        using var responseBuffer = new MemoryStream ();
        byte [] buffer = new byte [ 1024 ];
        while (true) {
            int read = await stream.ReadAsync ( buffer, ioCts.Token );
            if (read == 0)
                break;

            responseBuffer.Write ( buffer, 0, read );
        }

        string response = Encoding.ASCII.GetString ( responseBuffer.ToArray () );

        StringAssert.StartsWith ( response, "HTTP/1.1 301 Moved Permanently" );
        StringAssert.Contains ( response, $"Location: https://localhost:{port}/hello?x=1" );

        int headerEnd = response.IndexOf ( "\r\n\r\n", StringComparison.Ordinal );
        Assert.IsTrue ( headerEnd > 0, "Response should terminate headers with CRLF CRLF." );

        string [] headerLines = response [ ..headerEnd ].Split ( "\r\n" );
        for (int i = 1; i < headerLines.Length; i++) {
            StringAssert.Contains ( headerLines [ i ], ":" );
        }
    }

    [TestMethod]
    public async Task FixedLengthResponseStream_WithoutWrite_FlushesHeadersBeforeKeepAliveRead () {
        int port = GetFreePort ();
        using var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = new HeaderOnlyStreamHandler ()
        };
        host.Start ();

        using var client = new TcpClient ();
        using var ioCts = new CancellationTokenSource ( CloseWaitTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, ioCts.Token );

        await using var stream = client.GetStream ();
        byte [] requestBytes = Encoding.ASCII.GetBytes (
            $"GET / HTTP/1.1\r\n" +
            $"Host: localhost:{port}\r\n" +
            "\r\n" );

        await stream.WriteAsync ( requestBytes, ioCts.Token );
        string firstResponse = await ReadHeadersAsync ( stream, ioCts.Token );

        await stream.WriteAsync ( requestBytes, ioCts.Token );
        string secondResponse = await ReadHeadersAsync ( stream, ioCts.Token );

        StringAssert.StartsWith ( firstResponse, "HTTP/1.1 200 OK" );
        StringAssert.Contains ( firstResponse, "Content-Length: 0" );
        StringAssert.StartsWith ( secondResponse, "HTTP/1.1 200 OK" );
        StringAssert.Contains ( secondResponse, "Content-Length: 0" );
    }

    [TestMethod]
    public async Task DisconnectToken_IsCanceledWhenBodyReadDetectsClientDisconnect () {
        int port = GetFreePort ();
        var handler = new DisconnectTokenBodyReadHandler ();
        using var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = handler
        };
        host.Start ();

        using var client = new TcpClient ();
        using var ioCts = new CancellationTokenSource ( CloseWaitTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, ioCts.Token );

        await using var stream = client.GetStream ();
        byte [] requestBytes = Encoding.ASCII.GetBytes (
            $"POST / HTTP/1.1\r\n" +
            $"Host: localhost:{port}\r\n" +
            $"Content-Length: 16\r\n" +
            "\r\n" +
            "partial" );

        await stream.WriteAsync ( requestBytes, ioCts.Token );
        await stream.FlushAsync ( ioCts.Token );
        client.Close ();

        bool tokenCanceled = await handler.DisconnectTokenCanceled.Task.WaitAsync ( CloseWaitTimeout );

        Assert.IsTrue ( tokenCanceled, "Cadente should cancel DisconnectToken when a request body read detects that the client disconnected." );
    }

    [DataTestMethod]
    [DataRow ( "gzip", "Hello" )]
    [DataRow ( "deflate", "Hello" )]
    [DataRow ( "br", "Hello" )]
    [DataRow ( "gzip, chunked", "5\r\nHello\r\n0\r\n\r\n" )]
    [DataRow ( "chunked, gzip", "5\r\nHello\r\n0\r\n\r\n" )]
    [DataRow ( "chunked, chunked", "5\r\nHello\r\n0\r\n\r\n" )]
    public async Task TransferEncoding_RejectsUnsupportedCodingsBeforeHandler ( string transferEncoding, string body ) {
        int port = GetFreePort ();
        var handler = new ContextInvocationHandler ();
        using var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = handler
        };
        host.Start ();

        using var client = new TcpClient ();
        using var ioCts = new CancellationTokenSource ( CloseWaitTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, ioCts.Token );

        await using var stream = client.GetStream ();
        byte [] requestBytes = Encoding.ASCII.GetBytes (
            $"POST / HTTP/1.1\r\n" +
            $"Host: localhost:{port}\r\n" +
            $"Transfer-Encoding: {transferEncoding}\r\n" +
            "Connection: close\r\n" +
            "\r\n" +
            body );

        await stream.WriteAsync ( requestBytes, ioCts.Token );
        await stream.FlushAsync ( ioCts.Token );
        client.Client.Shutdown ( SocketShutdown.Send );

        string response = await ReadHeadersAsync ( stream, ioCts.Token );

        Assert.IsFalse ( handler.ContextCreated.Task.IsCompleted, "Unsupported Transfer-Encoding values must be rejected before user handlers run." );
        Assert.IsTrue (
            response.Length == 0 ||
            response.StartsWith ( "HTTP/1.1 4", StringComparison.Ordinal ) ||
            response.StartsWith ( "HTTP/1.1 5", StringComparison.Ordinal ),
            $"Unsupported Transfer-Encoding values must not produce a successful response. Response: {response}" );
    }

    [TestMethod]
    public async Task ContentEncoding_DoesNotAffectRequestFraming () {
        int port = GetFreePort ();
        var handler = new ContentEncodingCaptureHandler ();
        using var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = handler
        };
        host.Start ();

        using var client = new TcpClient ();
        using var ioCts = new CancellationTokenSource ( CloseWaitTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, ioCts.Token );

        await using var stream = client.GetStream ();
        byte [] requestBytes = Encoding.ASCII.GetBytes (
            $"POST / HTTP/1.1\r\n" +
            $"Host: localhost:{port}\r\n" +
            "Content-Encoding: gzip\r\n" +
            "Content-Length: 5\r\n" +
            "Connection: close\r\n" +
            "\r\n" +
            "Hello" );

        await stream.WriteAsync ( requestBytes, ioCts.Token );
        await stream.FlushAsync ( ioCts.Token );

        CapturedRequest captured = await handler.Captured.Task.WaitAsync ( CloseWaitTimeout );
        string response = await ReadHeadersAsync ( stream, ioCts.Token );

        Assert.AreEqual ( 5, captured.ContentLength );
        Assert.AreEqual ( "Hello", captured.Body );
        Assert.AreEqual ( "gzip", captured.ContentEncoding );
        StringAssert.StartsWith ( response, "HTTP/1.1 200 OK" );
    }

    private static int GetFreePort () {
        using var listener = new TcpListener ( IPAddress.Loopback, 0 );
        listener.Start ();

        return ((IPEndPoint) listener.LocalEndpoint).Port;
    }

    private static async Task<string> ReadHeadersAsync ( NetworkStream stream, CancellationToken cancellationToken ) {
        using var responseBuffer = new MemoryStream ();
        byte [] buffer = new byte [ 256 ];

        while (true) {
            int read = await stream.ReadAsync ( buffer, cancellationToken );
            if (read == 0)
                break;

            responseBuffer.Write ( buffer, 0, read );
            string response = Encoding.ASCII.GetString ( responseBuffer.ToArray () );
            if (response.Contains ( "\r\n\r\n", StringComparison.Ordinal ))
                return response;
        }

        return Encoding.ASCII.GetString ( responseBuffer.ToArray () );
    }

    private static async Task<bool> WaitUntilClosedAsync ( NetworkStream stream, TimeSpan timeout ) {
        byte [] buffer = new byte [ 1 ];

        try {
            int read = await stream.ReadAsync ( buffer ).AsTask ().WaitAsync ( timeout );
            return read == 0;
        }
        catch (TimeoutException) {
            return false;
        }
        catch (IOException) {
            return true;
        }
        catch (SocketException) {
            return true;
        }
        catch (ObjectDisposedException) {
            return true;
        }
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

    private sealed class HeaderOnlyStreamHandler : HttpHostHandler {
        public override async Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
            context.Response.Headers.Set ( new HttpHeader ( "Content-Length", "0" ) );
            _ = await context.Response.GetResponseStreamAsync ( chunked: false );
        }
    }

    private sealed class ContextInvocationHandler : HttpHostHandler {
        public TaskCompletionSource<bool> ContextCreated { get; } = new ( TaskCreationOptions.RunContinuationsAsynchronously );

        public override Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
            ContextCreated.TrySetResult ( true );
            context.Response.Headers.Set ( new HttpHeader ( "Content-Length", "0" ) );
            return Task.CompletedTask;
        }
    }

    private sealed class ContentEncodingCaptureHandler : HttpHostHandler {
        public TaskCompletionSource<CapturedRequest> Captured { get; } = new ( TaskCreationOptions.RunContinuationsAsynchronously );

        public override Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
            using var bodyBuffer = new MemoryStream ();
            context.Request.GetRequestStream ().CopyTo ( bodyBuffer );

            string? contentEncoding = null;
            foreach (HttpHeader header in context.Request.Headers) {
                if (header.Name.Equals ( "Content-Encoding", StringComparison.OrdinalIgnoreCase )) {
                    contentEncoding = header.Value;
                    break;
                }
            }

            Captured.TrySetResult ( new CapturedRequest (
                context.Request.ContentLength,
                Encoding.ASCII.GetString ( bodyBuffer.ToArray () ),
                contentEncoding ) );
            context.Response.Headers.Set ( new HttpHeader ( "Content-Length", "0" ) );

            return Task.CompletedTask;
        }
    }

    private sealed record CapturedRequest ( long ContentLength, string Body, string? ContentEncoding );

    private sealed class DisconnectTokenBodyReadHandler : HttpHostHandler {
        public TaskCompletionSource<bool> DisconnectTokenCanceled { get; } = new ( TaskCreationOptions.RunContinuationsAsynchronously );

        public override Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
            byte [] buffer = new byte [ 16 ];
            Stream bodyStream = context.Request.GetRequestStream ();

            try {
                while (bodyStream.Read ( buffer, 0, buffer.Length ) > 0) {
                }
            }
            catch (IOException) {
            }
            catch (ObjectDisposedException) {
            }

            DisconnectTokenCanceled.TrySetResult ( context.Client.DisconnectToken.IsCancellationRequested );
            context.Response.Headers.Set ( new HttpHeader ( "Content-Length", "0" ) );

            return Task.CompletedTask;
        }
    }
}
