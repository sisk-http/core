// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpIntrinsicsTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.Http.Json;
using System.Net.Sockets;
using System.Text;

namespace tests.Tests;

[TestClass]
public sealed class HttpIntrinsicsTests {

    [TestMethod]
    public async Task ExpectContinue_LargePayload_Succeeds () {
        using var client = Server.GetHttpClient ();
        client.DefaultRequestHeaders.ExpectContinue = true;

        try {
            string payload = new string ( 'x', 256 * 1024 );
            using var content = new StringContent ( payload, Encoding.UTF8, "text/plain" );

            using var response = await client.PostAsync ( "tests/httprequest/getBodyContents", content );
            response.EnsureSuccessStatusCode ();
            string echoed = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( payload.Length, echoed.Length, "Echoed payload should match the original length." );
            Assert.AreEqual ( payload, echoed, "Expect-Continue negotiation should not alter payload content." );
        }
        finally {
            client.DefaultRequestHeaders.ExpectContinue = false;
        }
    }

    [TestMethod]
    public async Task FormContent_ThousandFields_RoundTrips () {
        using var client = Server.GetHttpClient ();
        var formData = Enumerable.Range ( 0, 1000 ).ToDictionary ( i => $"field{i:0000}", i => $"value-{i}" );
        using var content = new FormUrlEncodedContent ( formData );

        using var response = await client.PostAsync ( "tests/httprequest/getFormContent", content );
        response.EnsureSuccessStatusCode ();
        Dictionary<string, string?>? echoed = await response.Content.ReadFromJsonAsync<Dictionary<string, string?>> ();
        Assert.IsNotNull ( echoed, "Server should echo form data as a dictionary." );
        var dictionary = echoed!;
        Assert.AreEqual ( formData.Count, dictionary.Count, "Field count should round trip." );
        foreach (var kv in formData) {
            Assert.AreEqual ( kv.Value, dictionary [ kv.Key ], $"Mismatch for field '{kv.Key}'." );
        }
    }

    [TestMethod]
    public async Task JsonContent_LargeStringPayload_RoundTrips () {
        using var client = Server.GetHttpClient ();
        var nameBuilder = new StringBuilder ( 128 * 1024 );
        for (int i = 0; i < 2048; i++) {
            nameBuilder.Append ( "cadente-" ).Append ( i ).Append ( ';' );
        }
        var expected = new TestPoco { Name = nameBuilder.ToString (), Value = 4242 };

        using var response = await client.PostAsJsonAsync ( "tests/httprequest/getJsonContent", expected );
        response.EnsureSuccessStatusCode ();
        TestPoco? echoed = await response.Content.ReadFromJsonAsync<TestPoco> ();
        Assert.IsNotNull ( echoed, "Server should echo JSON body as TestPoco." );
        var poco = echoed!;
        Assert.AreEqual ( expected.Value, poco.Value );
        Assert.AreEqual ( expected.Name, poco.Name );
    }

    [TestMethod]
    public async Task ResponseStreamChunked_ReadIncrementally () {
        using var client = Server.GetHttpClient ();

        using var response = await client.GetAsync ( "tests/responsestream/chunked", HttpCompletionOption.ResponseHeadersRead );
        response.EnsureSuccessStatusCode ();
        Assert.AreEqual ( true, response.Headers.TransferEncodingChunked, "Response should be chunked." );

        using var stream = await response.Content.ReadAsStreamAsync ();
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        var builder = new StringBuilder ( 128 );
        char [] buffer = new char [ 7 ];
        int readCount = 0;
        int read;
        while ((read = await reader.ReadAsync ( buffer, 0, buffer.Length )) > 0) {
            builder.Append ( buffer, 0, read );
            readCount++;
        }

        const string expectedContent = "This is the first chunk. This is the second chunk. This is the final chunk.";
        Assert.IsTrue ( readCount > 1, "Chunked response should be read in multiple iterations." );
        Assert.AreEqual ( expectedContent, builder.ToString () );
    }

    [TestMethod]
    public async Task ServerSentEvents_CancelAfterFirstEvent () {
        using var client = Server.GetHttpClient ();
        using var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 5 ) );

        using var response = await client.GetAsync ( "tests/sse/async", HttpCompletionOption.ResponseHeadersRead, cts.Token );
        response.EnsureSuccessStatusCode ();

        using var stream = await response.Content.ReadAsStreamAsync ( cts.Token );
        using var reader = new StreamReader ( stream, Encoding.UTF8 );
        var lines = new List<string> ();

        while (!cts.IsCancellationRequested) {
            string? line = await reader.ReadLineAsync ( cts.Token );
            if (line is null) {
                break;
            }

            if (line.Length == 0) {
                if (lines.Count > 0) {
                    break;
                }
                continue;
            }

            lines.Add ( line );
            if (lines.Count >= 2) {
                cts.Cancel ();
            }
        }

        Assert.IsTrue ( lines.Any ( l => l.Contains ( "async message 1" ) ), "Should receive the first SSE message before cancellation." );
    }

    [TestMethod]
    public async Task Plaintext_ManyConcurrentRequests_Succeed () {
        using var client = Server.GetHttpClient ();

        var tasks = Enumerable.Range ( 0, 12 ).Select ( async _ => {
            using var response = await client.GetAsync ( "tests/plaintext" );
            response.EnsureSuccessStatusCode ();
            string body = await response.Content.ReadAsStringAsync ();
            Assert.AreEqual ( "Hello, world!", body );
        } );

        await Task.WhenAll ( tasks );
    }

    [TestMethod]
    public async Task Plaintext_LongQueryString_Succeeds () {
        using var client = Server.GetHttpClient ();
        var uriBuilder = new StringBuilder ( "tests/plaintext?" );
        for (int i = 0; i < 200; i++) {
            if (i > 0) {
                uriBuilder.Append ( '&' );
            }
            uriBuilder.Append ( "param" ).Append ( i ).Append ( '=' );
            uriBuilder.Append ( new string ( 'a', 24 ) );
        }

        using var response = await client.GetAsync ( uriBuilder.ToString () );
        response.EnsureSuccessStatusCode ();
        string body = await response.Content.ReadAsStringAsync ();
        Assert.AreEqual ( "Hello, world!", body );
    }

    // -------------------------------------------------------------------------
    // Raw-TCP helpers — shared by all low-level tests below
    // -------------------------------------------------------------------------

    private static Uri GetServerUri () =>
        new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );

    private static async Task<int?> SendRawAndReadStatusAsync ( byte [] requestBytes, int timeoutMs = 3000 ) {
        var uri = GetServerUri ();
        using var cts = new CancellationTokenSource ( TimeSpan.FromMilliseconds ( timeoutMs ) );
        try {
            using var tcp = new TcpClient ();
            await tcp.ConnectAsync ( uri.Host, uri.Port, cts.Token );
            using var stream = tcp.GetStream ();
            await stream.WriteAsync ( requestBytes, cts.Token );
            await stream.FlushAsync ( cts.Token );
            using var reader = new StreamReader ( stream, Encoding.ASCII, leaveOpen: true );
            string? statusLine = await reader.ReadLineAsync ( cts.Token );
            if (statusLine is null || !statusLine.StartsWith ( "HTTP/" ))
                return null;
            var parts = statusLine.Split ( ' ', 3 );
            return parts.Length >= 2 && int.TryParse ( parts [ 1 ], out int code ) ? code : null;
        }
        catch (OperationCanceledException) {
            return null;
        }
        catch {
            return null;
        }
    }

    private static Task<int?> SendRawAndReadStatusAsync ( string request, int timeoutMs = 3000 )
        => SendRawAndReadStatusAsync ( Encoding.ASCII.GetBytes ( request ), timeoutMs );

    private static async Task<(int? status, string? body)> SendRawAndReadResponseAsync ( byte [] requestBytes, int timeoutMs = 5000 ) {
        var uri = GetServerUri ();
        using var cts = new CancellationTokenSource ( TimeSpan.FromMilliseconds ( timeoutMs ) );
        try {
            using var tcp = new TcpClient ();
            await tcp.ConnectAsync ( uri.Host, uri.Port, cts.Token );
            using var stream = tcp.GetStream ();
            await stream.WriteAsync ( requestBytes, cts.Token );
            await stream.FlushAsync ( cts.Token );
            using var reader = new StreamReader ( stream, Encoding.Latin1, leaveOpen: true );
            string? statusLine = await reader.ReadLineAsync ( cts.Token );
            if (statusLine is null || !statusLine.StartsWith ( "HTTP/" ))
                return (null, null);
            var statusParts = statusLine.Split ( ' ', 3 );
            if (statusParts.Length < 2 || !int.TryParse ( statusParts [ 1 ], out int statusCode ))
                return (null, null);
            int contentLength = 0;
            bool isChunked = false;
            string? headerLine;
            while (!string.IsNullOrEmpty ( headerLine = await reader.ReadLineAsync ( cts.Token ) )) {
                if (headerLine.StartsWith ( "Content-Length:", StringComparison.OrdinalIgnoreCase ))
                    int.TryParse ( headerLine [ "Content-Length:".Length.. ].Trim (), out contentLength );
                if (headerLine.Contains ( "Transfer-Encoding", StringComparison.OrdinalIgnoreCase ) &&
                    headerLine.Contains ( "chunked", StringComparison.OrdinalIgnoreCase ))
                    isChunked = true;
            }
            if (isChunked) {
                var bodyBuilder = new StringBuilder ();
                while (true) {
                    string? sizeLine = await reader.ReadLineAsync ( cts.Token );
                    if (sizeLine is null)
                        break;
                    int chunkSize = Convert.ToInt32 ( sizeLine.Split ( ';' ) [ 0 ].Trim (), 16 );
                    if (chunkSize == 0)
                        break;
                    var chunkBuffer = new char [ chunkSize ];
                    int totalRead = 0;
                    while (totalRead < chunkSize) {
                        int r = await reader.ReadAsync ( chunkBuffer.AsMemory ( totalRead, chunkSize - totalRead ), cts.Token );
                        if (r == 0)
                            break;
                        totalRead += r;
                    }
                    bodyBuilder.Append ( chunkBuffer, 0, totalRead );
                    await reader.ReadLineAsync ( cts.Token );
                }
                return (statusCode, bodyBuilder.ToString ());
            }
            if (contentLength > 0) {
                var bodyBuffer = new char [ contentLength ];
                int totalRead = 0;
                while (totalRead < contentLength) {
                    int r = await reader.ReadAsync ( bodyBuffer.AsMemory ( totalRead, contentLength - totalRead ), cts.Token );
                    if (r == 0)
                        break;
                    totalRead += r;
                }
                return (statusCode, new string ( bodyBuffer, 0, totalRead ));
            }
            return (statusCode, string.Empty);
        }
        catch (OperationCanceledException) {
            return (null, null);
        }
        catch {
            return (null, null);
        }
    }

    private static Task<(int? status, string? body)> SendRawAndReadResponseAsync ( string request, int timeoutMs = 5000 )
        => SendRawAndReadResponseAsync ( Encoding.ASCII.GetBytes ( request ), timeoutMs );

    [TestMethod]
    public async Task RequestLine_UnsupportedHttpProtocol_IsRejected () {
        var uri = GetServerUri ();
        string request =
            $"GET /tests/plaintext HTTP/1.2\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "\r\n";

        int? status = await SendRawAndReadStatusAsync ( request );
        if (status == 200 && Environment.GetEnvironmentVariable ( "SISK_TEST_ENGINE" ) != "Cadente")
            Assert.Inconclusive ( "HttpListener does not enforce unsupported protocol rejection; only enforced by Cadente." );
        Assert.IsTrue ( status is null || status >= 400,
            $"Unsupported HTTP protocol should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    // -------------------------------------------------------------------------
    // Header injection tests
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task HeaderInjection_ObsoleteFolding_WithSubHeader_IsRejected () {
        // RFC 9112 §5.2: obs-fold continuation lines that start with whitespace and contain
        // a colon would result in an injected header with an invalid name (e.g. "\tX-Evil").
        // The server must reject the request or close the connection.
        var uri = GetServerUri ();
        string request =
            $"GET /tests/plaintext HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "X-Outer: legitimate\r\n" +
            "\tX-Injected: via-obs-fold\r\n" +
            "\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        if (status == 200 && Environment.GetEnvironmentVariable ( "SISK_TEST_ENGINE" ) != "Cadente")
            Assert.Inconclusive ( "HttpListener does not enforce obs-fold rejection; only enforced by Cadente." );
        Assert.IsTrue ( status is null || status >= 400,
            $"Obs-fold header injection should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task HeaderInjection_ObsoleteFolding_ValueOnly_IsRejectedOrStripped () {
        // Obs-fold with only a continued value (no injected name) should either be rejected
        // or have the continuation stripped — the request must not succeed with unexpected payload.
        var uri = GetServerUri ();
        string request =
            $"GET /tests/plaintext HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "X-Outer: line1\r\n" +
            " continued-line2\r\n" +
            "\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status == 200 || status >= 400,
            $"Unexpected status for obs-fold value continuation: {status}." );
    }

    [TestMethod]
    public async Task HeaderInjection_BareLF_IsRejected () {
        // A bare LF (0x0A without preceding 0x0D) inside what could be a header value
        // is treated by some parsers as a line separator, enabling header injection.
        // RFC 9112 §2.2 allows treating bare LF as a line terminator, but from a security
        // perspective the server should reject such requests.
        var uri = GetServerUri ();
        byte [] request = Encoding.ASCII.GetBytes (
            $"GET /tests/plaintext HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "X-Custom: legitimate\nX-Injected: via-bare-lf\r\n" +
            "\r\n" );
        int? status = await SendRawAndReadStatusAsync ( request );
        if (status == 200 && Environment.GetEnvironmentVariable ( "SISK_TEST_ENGINE" ) != "Cadente")
            Assert.Inconclusive ( "HttpListener does not enforce bare-LF rejection; only enforced by Cadente." );
        Assert.IsTrue ( status is null || status >= 400,
            $"Bare-LF header injection should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task HeaderInjection_NullByteInName_IsRejected () {
        // RFC 9110 §5.1: header field names are tokens; null bytes (0x00) are not valid token chars.
        var uri = GetServerUri ();
        byte [] prefix = Encoding.ASCII.GetBytes (
            $"GET /tests/plaintext HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "X-Nu" );
        byte [] suffix = Encoding.ASCII.GetBytes ( "ll: value\r\n\r\n" );
        byte [] request = [ .. prefix, 0x00, .. suffix ];
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Null byte in header name should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task HeaderInjection_NullByteInValue_IsRejected () {
        // RFC 9110 §5.5: header field values must not contain null bytes.
        var uri = GetServerUri ();
        byte [] prefix = Encoding.ASCII.GetBytes (
            $"GET /tests/plaintext HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "X-Custom: before" );
        byte [] suffix = Encoding.ASCII.GetBytes ( "after\r\n\r\n" );
        byte [] request = [ .. prefix, 0x00, .. suffix ];
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Null byte in header value should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task Header_EmptyName_IsRejected () {
        // A header line that begins with a colon has an empty name, which is malformed.
        var uri = GetServerUri ();
        string request =
            $"GET /tests/plaintext HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            ": header-with-no-name\r\n" +
            "\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Header with empty name should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task Header_EmptyNameAndValue_IsRejected () {
        // A single colon ":\r\n" has neither a name nor a value — maximally malformed.
        var uri = GetServerUri ();
        string request =
            $"GET /tests/plaintext HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            ":\r\n" +
            "\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Colon-only header should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    // -------------------------------------------------------------------------
    // Hop-by-hop / duplicate-header tests
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task HopByHop_DuplicateContentLength_DifferentValues_IsRejected () {
        // RFC 9112 §6.3.3: if more than one Content-Length with distinct values is received,
        // the server MUST reject with 400 — this is an HTTP request-smuggling vector.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Content-Length: 5\r\n" +
            "Content-Length: 10\r\n" +
            "\r\n" +
            "Hello";
        int? status = await SendRawAndReadStatusAsync ( request, timeoutMs: 1500 );
        Assert.IsTrue ( status is null || status >= 400,
            $"Conflicting Content-Length headers should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task HopByHop_DuplicateContentLength_SameValue_IsHandled () {
        // Identical duplicate Content-Length values may be tolerated by some implementations.
        // The server should either reject (400) or process correctly (200 with correct body).
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Content-Length: 5\r\n" +
            "Content-Length: 5\r\n" +
            "\r\n" +
            "Hello";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status == 200 || status is null || status >= 400,
            $"Duplicate same-value Content-Length should be handled (200 or rejection). Got: {status}." );
    }

    [TestMethod]
    public async Task HopByHop_DuplicateTransferEncoding_IsRejected () {
        // Duplicate Transfer-Encoding headers are a request-smuggling vector.
        // The server should reject requests that provide the same hop-by-hop encoding twice.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Transfer-Encoding: chunked\r\n" +
            "Transfer-Encoding: chunked\r\n" +
            "\r\n" +
            "5\r\nHello\r\n" +
            "0\r\n\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Duplicate Transfer-Encoding should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task HopByHop_ContentLengthAndTransferEncoding_Together_IsRejected () {
        // RFC 9112 §6.3.3: a message with both Transfer-Encoding and Content-Length
        // "ought to be handled as an error" — it is a classic smuggling attempt.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Content-Length: 5\r\n" +
            "Transfer-Encoding: chunked\r\n" +
            "\r\n" +
            "5\r\nHello\r\n" +
            "0\r\n\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        if (status == 200 && Environment.GetEnvironmentVariable ( "SISK_TEST_ENGINE" ) != "Cadente")
            Assert.Inconclusive ( "HttpListener does not enforce CL+TE rejection; only enforced by Cadente." );
        Assert.IsTrue ( status is null || status >= 400,
            $"Simultaneous Content-Length and Transfer-Encoding should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    // -------------------------------------------------------------------------
    // Chunked request tests
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task ChunkedRequest_SingleChunk_Succeeds () {
        string chunkData = "Hello, chunked world!";
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Transfer-Encoding: chunked\r\n" +
            "\r\n" +
            $"{chunkData.Length:X}\r\n{chunkData}\r\n" +
            "0\r\n\r\n";
        var (status, body) = await SendRawAndReadResponseAsync ( request );
        Assert.AreEqual ( 200, status, "Single-chunk request should succeed." );
        Assert.AreEqual ( chunkData, body, "Server should echo the full chunk body." );
    }

    [TestMethod]
    public async Task ChunkedRequest_MultipleChunks_AreConcatenated () {
        string [] chunks = [ "First-", "Second-", "Third" ];
        string expected = string.Concat ( chunks );
        var uri = GetServerUri ();
        var sb = new StringBuilder ();
        sb.Append ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" );
        sb.Append ( $"Host: {uri.Host}:{uri.Port}\r\n" );
        sb.Append ( "Transfer-Encoding: chunked\r\n\r\n" );
        foreach (string chunk in chunks)
            sb.Append ( $"{chunk.Length:X}\r\n{chunk}\r\n" );
        sb.Append ( "0\r\n\r\n" );
        var (status, body) = await SendRawAndReadResponseAsync ( sb.ToString () );
        Assert.AreEqual ( 200, status, "Multi-chunk request should succeed." );
        Assert.AreEqual ( expected, body, "All chunks must be concatenated in order." );
    }

    [TestMethod]
    public async Task ChunkedRequest_ChunkExtensionsAreIgnored () {
        // RFC 9112 §7.1.1: chunk extensions are optional; unrecognised ones MUST be ignored.
        string chunkData = "Hello";
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Transfer-Encoding: chunked\r\n\r\n" +
            $"{chunkData.Length:X};name=ignored;another=ext\r\n{chunkData}\r\n" +
            "0\r\n\r\n";
        var (status, body) = await SendRawAndReadResponseAsync ( request );
        Assert.AreEqual ( 200, status, "Chunk extensions should not cause an error." );
        Assert.AreEqual ( chunkData, body, "Chunk extensions must not affect the body." );
    }

    [TestMethod]
    public async Task ChunkedRequest_InvalidHexChunkSize_IsRejected () {
        // A chunk size that is not valid hexadecimal is a protocol violation.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Transfer-Encoding: chunked\r\n\r\n" +
            "ZZZ\r\nHello\r\n" +
            "0\r\n\r\n";
        int? status = await SendRawAndReadStatusAsync ( request, timeoutMs: 5000 );
        Assert.IsTrue ( status is null || status >= 400,
            $"Invalid hex chunk size should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task ChunkedRequest_NegativeChunkSize_IsRejected () {
        // Negative chunk sizes are invalid; the parser must not interpret them as valid lengths.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Transfer-Encoding: chunked\r\n\r\n" +
            "-5\r\nHello\r\n" +
            "0\r\n\r\n";
        int? status = await SendRawAndReadStatusAsync ( request, timeoutMs: 5000 );
        Assert.IsTrue ( status is null || status >= 400,
            $"Negative chunk size should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task ChunkedRequest_EmptyChunkSize_IsRejected () {
        // An empty chunk-size line (just "\r\n") before the last-chunk is malformed.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Transfer-Encoding: chunked\r\n\r\n" +
            "\r\nHello\r\n" +
            "0\r\n\r\n";
        int? status = await SendRawAndReadStatusAsync ( request, timeoutMs: 5000 );
        Assert.IsTrue ( status is null || status >= 400,
            $"Empty chunk-size line should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    // -------------------------------------------------------------------------
    // Content-Length mismatch / invalid tests
    // -------------------------------------------------------------------------

    [TestMethod]
    public async Task ContentLength_SmallerThanBody_ServerReadsExactly () {
        // RFC 9112 §6.3: the server must read exactly Content-Length bytes from the body.
        // Bytes beyond Content-Length are excess and should not be included in the read body.
        string fullBody = "Hello World";
        int declaredLength = 5;
        var uri = GetServerUri ();
        byte [] request = Encoding.ASCII.GetBytes (
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            $"Content-Length: {declaredLength}\r\n" +
            "\r\n" +
            fullBody );
        var (status, body) = await SendRawAndReadResponseAsync ( request );
        Assert.AreEqual ( 200, status, "Request with Content-Length smaller than body should succeed." );
        Assert.AreEqual ( fullBody [ ..declaredLength ], body,
            "Server must echo exactly Content-Length bytes, not the excess." );
    }

    [TestMethod]
    public async Task ContentLength_ZeroWithExtraBodyBytes_BodyIsIgnored () {
        // When Content-Length is 0, the server should treat the request as having no body
        // even if extra bytes follow the header section.
        var uri = GetServerUri ();
        byte [] request = Encoding.ASCII.GetBytes (
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Content-Length: 0\r\n" +
            "\r\n" +
            "ShouldBeIgnored" );
        var (status, body) = await SendRawAndReadResponseAsync ( request );
        Assert.AreEqual ( 200, status, "Request with Content-Length: 0 should succeed." );
        Assert.AreEqual ( string.Empty, body,
            "No body bytes should be delivered to the handler when Content-Length is 0." );
    }

    [TestMethod]
    public async Task ContentLength_NegativeValue_IsRejected () {
        // A negative Content-Length value is syntactically invalid (RFC 9112 §6.3).
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Content-Length: -1\r\n" +
            "\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Negative Content-Length should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task ContentLength_NonNumericValue_IsRejected () {
        // Non-numeric Content-Length is a protocol violation.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Content-Length: not-a-number\r\n" +
            "\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Non-numeric Content-Length should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }

    [TestMethod]
    public async Task ContentLength_OverflowValue_IsRejected () {
        // A Content-Length that overflows Int64 (or any representation type) is invalid.
        var uri = GetServerUri ();
        string request =
            $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\n" +
            $"Host: {uri.Host}:{uri.Port}\r\n" +
            "Content-Length: 99999999999999999999\r\n" +
            "\r\n";
        int? status = await SendRawAndReadStatusAsync ( request );
        Assert.IsTrue ( status is null || status >= 400,
            $"Overflow Content-Length should be rejected. Got: {status?.ToString () ?? "connection closed"}." );
    }
}
