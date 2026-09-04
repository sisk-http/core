// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadentePipeliningTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Sockets;
using System.Text;
using Sisk.Cadente;

namespace tests.Tests;

[TestClass]
public sealed class CadentePipeliningTests {
    private const int HttpConnectionReservedBufferSize = 8 * 1024;
    private static readonly TimeSpan IoTimeout = TimeSpan.FromSeconds ( 5 );

    [TestMethod]
    public async Task PipelinedGets_InSingleWrite_ReturnResponsesInOrder () {
        int port = GetFreePort ();
        using var host = CreateHost ( port );

        string response = await SendAndReadAsync ( port,
            $"GET /a HTTP/1.1\r\nHost: localhost:{port}\r\n\r\n" +
            $"GET /b HTTP/1.1\r\nHost: localhost:{port}\r\n\r\n" +
            $"GET /c HTTP/1.1\r\nHost: localhost:{port}\r\nConnection: close\r\n\r\n" );

        AssertResponseOrder ( response, "response-a", "response-b", "response-c" );
    }

    [TestMethod]
    public async Task ContentLengthBody_FollowedByPipelinedGet_PreservesNextRequest () {
        int port = GetFreePort ();
        using var host = CreateHost ( port );

        string response = await SendAndReadAsync ( port,
            $"POST /body HTTP/1.1\r\nHost: localhost:{port}\r\nContent-Length: 5\r\n\r\nHello" +
            $"GET /after HTTP/1.1\r\nHost: localhost:{port}\r\nConnection: close\r\n\r\n" );

        AssertResponseOrder ( response, "body-Hello", "response-after" );
    }

    [TestMethod]
    public async Task LargeContentLengthBody_FollowedByPipelinedGet_DoesNotOverRead () {
        int port = GetFreePort ();
        using var host = CreateHost ( port );
        string requestBody = new ( 'x', HttpConnectionReservedBufferSize + 257 );

        string response = await SendAndReadAsync ( port,
            $"POST /body-length HTTP/1.1\r\nHost: localhost:{port}\r\nContent-Length: {requestBody.Length}\r\n\r\n{requestBody}" +
            $"GET /after HTTP/1.1\r\nHost: localhost:{port}\r\nConnection: close\r\n\r\n" );

        AssertResponseOrder ( response, $"body-length-{requestBody.Length}", "response-after" );
    }

    [TestMethod]
    public async Task ExpectContinue_WithBodyInSameWrite_PreservesBodyAndNextRequest () {
        int port = GetFreePort ();
        using var host = CreateHost ( port );

        string response = await SendAndReadAsync ( port,
            $"POST /body HTTP/1.1\r\nHost: localhost:{port}\r\nExpect: 100-continue\r\nContent-Length: 5\r\n\r\nHello" +
            $"GET /after HTTP/1.1\r\nHost: localhost:{port}\r\nConnection: close\r\n\r\n" );

        StringAssert.Contains ( response, "HTTP/1.1 100 Continue" );
        AssertResponseOrder ( response, "body-Hello", "response-after" );
    }

    [TestMethod]
    public async Task ChunkedBody_FollowedByPipelinedGet_PreservesNextRequest () {
        int port = GetFreePort ();
        using var host = CreateHost ( port );

        string response = await SendAndReadAsync ( port,
            $"POST /body HTTP/1.1\r\nHost: localhost:{port}\r\nTransfer-Encoding: chunked\r\n\r\n" +
            "2\r\nHe\r\n3\r\nllo\r\n0\r\nX-Trailer: accepted\r\n\r\n" +
            $"GET /after HTTP/1.1\r\nHost: localhost:{port}\r\nConnection: close\r\n\r\n" );

        AssertResponseOrder ( response, "body-Hello", "response-after" );
    }

    [TestMethod]
    public async Task FragmentedHeaders_AreCombinedWithBufferedNextRequest () {
        int port = GetFreePort ();
        using var host = CreateHost ( port );
        using var client = new TcpClient ();
        using var connectCts = new CancellationTokenSource ( IoTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, connectCts.Token );

        await using var stream = client.GetStream ();
        await stream.WriteAsync ( Encoding.ASCII.GetBytes ( $"GET /fragmented HTTP/1.1\r\nHost: localhost:{port}\r\n\r" ), connectCts.Token );
        await stream.WriteAsync ( Encoding.ASCII.GetBytes (
            $"\nGET /after HTTP/1.1\r\nHost: localhost:{port}\r\nConnection: close\r\n\r\n" ), connectCts.Token );

        string response = await ReadUntilClosedAsync ( stream, connectCts.Token );
        AssertResponseOrder ( response, "response-fragmented", "response-after" );
    }

    [TestMethod]
    public async Task StreamResponses_FromPipelinedRequests_RemainOrdered () {
        int port = GetFreePort ();
        using var host = CreateHost ( port );

        string response = await SendAndReadAsync ( port,
            $"GET /stream-a HTTP/1.1\r\nHost: localhost:{port}\r\n\r\n" +
            $"GET /stream-b HTTP/1.1\r\nHost: localhost:{port}\r\nConnection: close\r\n\r\n" );

        AssertResponseOrder ( response, "stream-response-a", "stream-response-b" );
    }

    private static HttpHost CreateHost ( int port ) {
        var host = new HttpHost ( new IPEndPoint ( IPAddress.Loopback, port ) ) {
            Handler = new PipeliningHandler ()
        };
        host.Start ();
        return host;
    }

    private static async Task<string> SendAndReadAsync ( int port, string request ) {
        using var client = new TcpClient ();
        using var ioCts = new CancellationTokenSource ( IoTimeout );
        await client.ConnectAsync ( IPAddress.Loopback, port, ioCts.Token );

        await using var stream = client.GetStream ();
        await stream.WriteAsync ( Encoding.ASCII.GetBytes ( request ), ioCts.Token );
        return await ReadUntilClosedAsync ( stream, ioCts.Token );
    }

    private static async Task<string> ReadUntilClosedAsync ( NetworkStream stream, CancellationToken cancellationToken ) {
        using var response = new MemoryStream ();
        byte [] buffer = new byte [ 1024 ];

        while (true) {
            int read = await stream.ReadAsync ( buffer, cancellationToken );
            if (read == 0)
                return Encoding.ASCII.GetString ( response.ToArray () );

            response.Write ( buffer, 0, read );
        }
    }

    private static void AssertResponseOrder ( string response, params string [] bodies ) {
        Assert.AreEqual ( bodies.Length, response.Split ( "HTTP/1.1 200 OK", StringSplitOptions.None ).Length - 1 );

        int previousIndex = -1;
        foreach (string body in bodies) {
            int index = response.IndexOf ( body, StringComparison.Ordinal );
            Assert.IsTrue ( index > previousIndex, $"Response body '{body}' was missing or out of order. Raw response: {response}" );
            previousIndex = index;
        }
    }

    private static int GetFreePort () {
        using var listener = new TcpListener ( IPAddress.Loopback, 0 );
        listener.Start ();
        return ((IPEndPoint) listener.LocalEndpoint).Port;
    }

    private sealed class PipeliningHandler : HttpHostHandler {
        public override async Task OnContextCreatedAsync ( HttpHost host, HttpHostContext context ) {
            string path = context.Request.Path;
            byte [] body;

            if (path is "/body" or "/body-length") {
                using var bodyBuffer = new MemoryStream ();
                context.Request.GetRequestStream ().CopyTo ( bodyBuffer );
                body = path == "/body"
                    ? Encoding.ASCII.GetBytes ( $"body-{Encoding.ASCII.GetString ( bodyBuffer.ToArray () )}" )
                    : Encoding.ASCII.GetBytes ( $"body-length-{bodyBuffer.Length}" );
            }
            else if (path.StartsWith ( "/stream-", StringComparison.Ordinal )) {
                body = Encoding.ASCII.GetBytes ( $"stream-response-{path [ ^1 ]}" );
                context.Response.Headers.Set ( new HttpHeader ( "Content-Length", body.Length.ToString () ) );
                await using Stream responseStream = await context.Response.GetResponseStreamAsync ( chunked: false );
                await responseStream.WriteAsync ( body );
                return;
            }
            else {
                body = Encoding.ASCII.GetBytes ( $"response-{path [ 1.. ]}" );
            }

            context.Response.Headers.Set ( new HttpHeader ( "Content-Length", body.Length.ToString () ) );
            context.Response.WriteInlineContent ( body );
        }
    }
}
