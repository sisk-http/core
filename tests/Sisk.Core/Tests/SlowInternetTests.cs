// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SlowInternetTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.Sockets;
using System.Text;
using System.Web;

namespace tests.Tests;

[TestClass]
public sealed class SlowInternetTests {

    private async Task SendBytesSlowly ( NetworkStream stream, byte [] data, int chunkSize, int delayMs ) {
        for (int i = 0; i < data.Length; i += chunkSize) {
            int remaining = data.Length - i;
            int currentChunkSize = Math.Min ( chunkSize, remaining );
            await stream.WriteAsync ( data, i, currentChunkSize );
            await stream.FlushAsync ();
            await Task.Delay ( delayMs );
        }
    }

    [TestMethod]
    public async Task LargeHeaders_SlowSend_Succeeds () {
        using var client = new TcpClient ();
        var uri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        await client.ConnectAsync ( uri.Host, uri.Port );
        using var stream = client.GetStream ();

        var bigParam = string.Join ( string.Empty, Enumerable.Range ( 0, 30 ).Select ( s => Guid.NewGuid () ) );

        var sb = new StringBuilder ();
        sb.Append ( $"GET /tests/plaintext?q={HttpUtility.UrlEncode ( bigParam )} HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        // Add large headers ~4KB
        for (int i = 0; i < 40; i++) {
            sb.Append ( $"X-Large-Header-{i}: {new string ( 'a', 100 )}\r\n" );
        }
        sb.Append ( "\r\n" );

        byte [] requestBytes = Encoding.UTF8.GetBytes ( sb.ToString () );
        await SendBytesSlowly ( stream, requestBytes, 100, delayMs: 15 );

        // Read response
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        string? statusLine = await reader.ReadLineAsync ();
        Assert.IsNotNull ( statusLine );
        Assert.IsTrue ( statusLine.Contains ( "200 OK" ) );
    }

    [TestMethod]
    public async Task SmallContent_ContentLength_SlowSend_Succeeds () {
        using var client = new TcpClient ();
        var uri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        await client.ConnectAsync ( uri.Host, uri.Port );
        using var stream = client.GetStream ();

        string body = "Small content body";
        var sb = new StringBuilder ();
        sb.Append ( "POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        sb.Append ( $"Content-Length: {body.Length}\r\n" );
        sb.Append ( "\r\n" );
        sb.Append ( body );

        byte [] requestBytes = Encoding.UTF8.GetBytes ( sb.ToString () );
        await SendBytesSlowly ( stream, requestBytes, 5, 50 ); // Very slow send

        // Read response
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        string? statusLine = await reader.ReadLineAsync ();
        Assert.IsNotNull ( statusLine );
        Assert.IsTrue ( statusLine.Contains ( "200 OK" ) );

        // Read until body
        string? line;
        while (!string.IsNullOrEmpty ( line = await reader.ReadLineAsync () )) { }

        char [] buffer = new char [ body.Length ];
        await reader.ReadAsync ( buffer, 0, body.Length );
        string responseBody = new string ( buffer );
        Assert.AreEqual ( body, responseBody );
    }

    [TestMethod]
    public async Task SmallContent_Chunked_SlowSend_Succeeds () {
        using var client = new TcpClient ();
        var uri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        await client.ConnectAsync ( uri.Host, uri.Port );
        using var stream = client.GetStream ();

        var sb = new StringBuilder ();
        sb.Append ( "POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        sb.Append ( "Transfer-Encoding: chunked\r\n" );
        sb.Append ( "\r\n" );

        // Send headers first
        byte [] headerBytes = Encoding.UTF8.GetBytes ( sb.ToString () );
        await stream.WriteAsync ( headerBytes );

        // Send chunks slowly
        string [] chunks = { "Chunk1", "Chunk2", "Chunk3" };
        foreach (var chunk in chunks) {
            byte [] chunkBytes = Encoding.UTF8.GetBytes ( $"{chunk.Length:X}\r\n{chunk}\r\n" );
            await SendBytesSlowly ( stream, chunkBytes, 1, 50 );
        }
        byte [] endChunk = Encoding.UTF8.GetBytes ( "0\r\n\r\n" );
        await stream.WriteAsync ( endChunk );

        // Read response
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        string? statusLine = await reader.ReadLineAsync ();
        Assert.IsNotNull ( statusLine );
        Assert.IsTrue ( statusLine.Contains ( "200 OK" ) );

        // Read until body
        string? line;
        while (!string.IsNullOrEmpty ( line = await reader.ReadLineAsync () )) { }

        string expectedBody = string.Join ( "", chunks );
        char [] buffer = new char [ expectedBody.Length ];
        await reader.ReadAsync ( buffer, 0, expectedBody.Length );
        string responseBody = new string ( buffer );
        Assert.AreEqual ( expectedBody, responseBody );
    }

    [TestMethod]
    public async Task MediumContent_ContentLength_SlowSend_Succeeds () {
        using var client = new TcpClient ();
        var uri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        await client.ConnectAsync ( uri.Host, uri.Port );
        using var stream = client.GetStream ();

        string body = new string ( 'a', 10 * 1024 ); // 10KB
        var sb = new StringBuilder ();
        sb.Append ( "POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        sb.Append ( $"Content-Length: {body.Length}\r\n" );
        sb.Append ( "\r\n" );

        byte [] headerBytes = Encoding.UTF8.GetBytes ( sb.ToString () );
        await stream.WriteAsync ( headerBytes );

        byte [] bodyBytes = Encoding.UTF8.GetBytes ( body );
        await SendBytesSlowly ( stream, bodyBytes, 1024, 20 ); // Send 1KB every 20ms

        // Read response
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        string? statusLine = await reader.ReadLineAsync ();
        Assert.IsNotNull ( statusLine );
        Assert.IsTrue ( statusLine.Contains ( "200 OK" ) );

        // Read until body
        string? line;
        while (!string.IsNullOrEmpty ( line = await reader.ReadLineAsync () )) { }

        char [] buffer = new char [ body.Length ];
        int totalRead = 0;
        while (totalRead < body.Length) {
            int read = await reader.ReadAsync ( buffer, totalRead, body.Length - totalRead );
            if (read == 0)
                break;
            totalRead += read;
        }
        string responseBody = new string ( buffer );
        Assert.AreEqual ( body, responseBody );
    }

    [TestMethod]
    public async Task MediumContent_Chunked_SlowSend_Succeeds () {
        using var client = new TcpClient ();
        var uri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        await client.ConnectAsync ( uri.Host, uri.Port );
        using var stream = client.GetStream ();

        var sb = new StringBuilder ();
        sb.Append ( "POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        sb.Append ( "Transfer-Encoding: chunked\r\n" );
        sb.Append ( "\r\n" );

        await stream.WriteAsync ( Encoding.UTF8.GetBytes ( sb.ToString () ) );

        string chunkData = new string ( 'a', 1024 ); // 1KB chunks
        for (int i = 0; i < 10; i++) {
            byte [] chunkBytes = Encoding.UTF8.GetBytes ( $"{chunkData.Length:X}\r\n{chunkData}\r\n" );
            await SendBytesSlowly ( stream, chunkBytes, 100, 10 );
        }
        byte [] endChunk = Encoding.UTF8.GetBytes ( "0\r\n\r\n" );
        await stream.WriteAsync ( endChunk );

        // Read response
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        string? statusLine = await reader.ReadLineAsync ();
        Assert.IsNotNull ( statusLine );
        Assert.IsTrue ( statusLine.Contains ( "200 OK" ) );

        // Read until body
        string? line;
        while (!string.IsNullOrEmpty ( line = await reader.ReadLineAsync () )) { }

        string expectedBody = new string ( 'a', 10 * 1024 );
        char [] buffer = new char [ expectedBody.Length ];
        int totalRead = 0;
        while (totalRead < expectedBody.Length) {
            int read = await reader.ReadAsync ( buffer, totalRead, expectedBody.Length - totalRead );
            if (read == 0)
                break;
            totalRead += read;
        }
        string responseBody = new string ( buffer );
        Assert.AreEqual ( expectedBody, responseBody );
    }

    [TestMethod]
    public async Task SegmentedHeaders_SlowSend_Succeeds () {
        using var client = new TcpClient ();
        var uri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        await client.ConnectAsync ( uri.Host, uri.Port );
        using var stream = client.GetStream ();

        var sb = new StringBuilder ();
        sb.Append ( "GET /tests/plaintext HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        sb.Append ( "X-Custom-Header: value\r\n" );
        sb.Append ( "\r\n" );

        byte [] requestBytes = Encoding.UTF8.GetBytes ( sb.ToString () );
        await SendBytesSlowly ( stream, requestBytes, 10, 50 ); // Send 10 bytes every 50ms

        // Read response
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        string? statusLine = await reader.ReadLineAsync ();
        Assert.IsNotNull ( statusLine );
        Assert.IsTrue ( statusLine.Contains ( "200 OK" ) );
    }

    [TestMethod]
    public async Task RepeatedRequestHeaders_Succeeds () {
        using var client = new TcpClient ();
        var uri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        await client.ConnectAsync ( uri.Host, uri.Port );
        using var stream = client.GetStream ();

        var sb = new StringBuilder ();
        sb.Append ( "GET /tests/plaintext HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        sb.Append ( "X-Repeated: value1\r\n" );
        sb.Append ( "X-Repeated: value2\r\n" );
        sb.Append ( "\r\n" );

        byte [] requestBytes = Encoding.UTF8.GetBytes ( sb.ToString () );
        await stream.WriteAsync ( requestBytes );

        // Read response
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        string? statusLine = await reader.ReadLineAsync ();
        Assert.IsNotNull ( statusLine );
        Assert.IsTrue ( statusLine.Contains ( "200 OK" ) );
    }

    [TestMethod]
    public async Task RepeatedResponseHeaders_Succeeds () {
        using var client = Server.GetHttpClient ();

        using var response = await client.GetAsync ( "tests/headers/repeated" );
        response.EnsureSuccessStatusCode ();

        IEnumerable<string>? values;
        Assert.IsTrue ( response.Headers.TryGetValues ( "X-Custom-Header", out values ) );
        Assert.IsNotNull ( values );
        var list = values!.ToList ();
        Assert.AreEqual ( 2, list.Count );
        Assert.IsTrue ( list.Contains ( "value1" ) );
        Assert.IsTrue ( list.Contains ( "value2" ) );
    }
}
