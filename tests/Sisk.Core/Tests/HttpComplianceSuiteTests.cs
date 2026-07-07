// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpComplianceSuiteTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.Sockets;
using System.Reflection;
using System.Text;

namespace tests.Tests;

[TestClass]
public sealed class HttpComplianceSuiteTests {
    private const int DefaultTimeoutMs = 2000;
    private const int IncompleteTimeoutMs = 1200;

    public static IEnumerable<object []> ComplianceCases () {
        yield return Case ( "COMP-BASELINE", "Valid GET request confirms server is reachable",
            u => Get ( u, "/tests/plaintext" ),
            e => ExpectStatus ( e, 200 ) );
        yield return Case ( "RFC9112-2.2-BARE-LF-REQUEST-LINE", "Bare LF in request line should be rejected, but may be accepted",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "RFC9112-2.2-BARE-LF-HEADER", "Bare LF in header should be rejected, but may be accepted",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "RFC9112-5.1-OBS-FOLD", "Obs-fold in headers should be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nX-Test: one\r\n two\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9110-5.6.2-SP-BEFORE-COLON", "Whitespace between header name and colon must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nBad : value\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-3-MULTI-SP-REQUEST-LINE", "Multiple spaces between request-line components should be rejected but may be accepted",
            u => Bytes ( $"GET  /tests/plaintext  HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "RFC9112-7.1-MISSING-HOST", "HTTP/1.1 request without Host must be rejected with 400",
            _ => Bytes ( "GET /tests/plaintext HTTP/1.1\r\nConnection: close\r\n\r\n" ),
            ExpectBadRequestOrClosed );
        yield return Case ( "RFC9112-2.3-INVALID-VERSION", "Invalid HTTP version must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.X\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-5-EMPTY-HEADER-NAME", "Empty header name must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\n: value\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-3-CR-ONLY-LINE-ENDING", "CR without LF as line ending must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\rHost: {Authority ( u )}\rConnection: close\r\r" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-3-MISSING-TARGET", "Request line with no target must be rejected",
            u => Bytes ( $"GET  HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-3.2-FRAGMENT-IN-TARGET", "Fragment in request-target must be rejected",
            u => Bytes ( $"GET /tests/plaintext#fragment HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-2.3-HTTP09-REQUEST", "HTTP/0.9 request must be rejected",
            _ => Bytes ( "GET /tests/plaintext\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-5-INVALID-HEADER-NAME", "Header name with invalid characters must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nBad[Name]: value\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-5-HEADER-NO-COLON", "Header line without colon must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nBrokenHeader\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9110-5.4-DUPLICATE-HOST", "Duplicate Host headers with different values must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nHost: attacker.test\r\nConnection: close\r\n\r\n" ),
            ExpectBadRequestOrClosed );
        yield return Case ( "RFC9112-6.1-CL-NON-NUMERIC", "Non-numeric Content-Length must be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nContent-Length: abc\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "RFC9112-6.1-CL-PLUS-SIGN", "Content-Length with plus sign must be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nContent-Length: +5\r\nConnection: close\r\n\r\nHello" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-WHITESPACE-BEFORE-HEADERS", "Whitespace before first header line must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\n Host: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-DUPLICATE-HOST-SAME", "Duplicate Host headers with identical values must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectBadRequestOrClosed );
        yield return Case ( "COMP-HOST-WITH-USERINFO", "Host header with userinfo must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: user@{Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectBadRequestOrClosed );
        yield return Case ( "COMP-HOST-WITH-PATH", "Host header with path component must be rejected",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}/path\r\nConnection: close\r\n\r\n" ),
            ExpectBadRequestOrClosed );
        yield return Case ( "COMP-ASTERISK-WITH-GET", "Asterisk-form request-target with GET must be rejected",
            u => Bytes ( $"GET * HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-OPTIONS-STAR", "OPTIONS * is the only valid asterisk-form request",
            u => Bytes ( $"OPTIONS * HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "COMP-UNKNOWN-TE", "Unknown Transfer-Encoding without Content-Length must be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: gzip\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-UNKNOWN-TE-WITH-CL", "Unknown Transfer-Encoding with Content-Length must be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: gzip\r\nContent-Length: 5\r\nConnection: close\r\n\r\nHello" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-MIXED-TE-GZIP-CHUNKED", "Transfer-Encoding with unsupported coding before chunked must be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: gzip, chunked\r\nConnection: close\r\n\r\n5\r\nHello\r\n0\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-MIXED-TE-CHUNKED-GZIP", "Transfer-Encoding with unsupported final coding must be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: chunked, gzip\r\nConnection: close\r\n\r\n5\r\nHello\r\n0\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-DUPLICATE-CHUNKED-TE", "Transfer-Encoding with repeated chunked codings must be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: chunked, chunked\r\nConnection: close\r\n\r\n5\r\nHello\r\n0\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-LEADING-CRLF", "Leading CRLF before request-line may be ignored",
            u => Bytes ( $"\r\nGET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "COMP-ABSOLUTE-FORM", "Absolute-form request-target should be accepted",
            u => Bytes ( $"GET http://{Authority ( u )}/tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "COMP-METHOD-CASE", "Lowercase method get must not be treated as GET",
            u => Bytes ( $"get /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectNotSuccessful );
        yield return Case ( "COMP-POST-CL-BODY", "POST with Content-Length and matching body must be accepted",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nContent-Length: 5\r\nConnection: close\r\n\r\nHello" ),
            e => ExpectStatusAndBody ( e, 200, "Hello" ) );
        yield return Case ( "COMP-POST-CL-ZERO", "POST with Content-Length: 0 must be accepted",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nContent-Length: 0\r\nConnection: close\r\n\r\n" ),
            e => ExpectStatusAndBody ( e, 200, string.Empty ) );
        yield return Case ( "COMP-POST-NO-CL-NO-TE", "POST with neither Content-Length nor Transfer-Encoding has zero-length body",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            e => ExpectStatusAndBody ( e, 200, string.Empty ) );
        yield return Case ( "COMP-POST-CL-UNDERSEND", "POST undersending declared Content-Length is incomplete",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nContent-Length: 10\r\nConnection: close\r\n\r\nHello" ),
            ExpectIncompleteOrRejected, IncompleteTimeoutMs, true );
        yield return Case ( "COMP-CHUNKED-BODY", "Valid single-chunk POST must be accepted",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: chunked\r\nConnection: close\r\n\r\n5\r\nHello\r\n0\r\n\r\n" ),
            e => ExpectStatusAndBody ( e, 200, "Hello" ) );
        yield return Case ( "COMP-CHUNKED-MULTI", "Valid multi-chunk POST must be accepted",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: chunked\r\nConnection: close\r\n\r\n5\r\nHello\r\n6\r\n World\r\n0\r\n\r\n" ),
            e => ExpectStatusAndBody ( e, 200, "Hello World" ) );
        yield return Case ( "COMP-CHUNKED-EMPTY", "Zero-length chunked body must be accepted",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: chunked\r\nConnection: close\r\n\r\n0\r\n\r\n" ),
            e => ExpectStatusAndBody ( e, 200, string.Empty ) );
        yield return Case ( "COMP-CHUNKED-NO-FINAL", "Chunked body without zero terminator is incomplete",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: chunked\r\nConnection: close\r\n\r\n5\r\nHello\r\n" ),
            ExpectIncompleteOrRejected, IncompleteTimeoutMs, true );
        yield return Case ( "COMP-METHOD-CONNECT", "CONNECT to an origin server must be rejected",
            u => Bytes ( $"CONNECT {Authority ( u )} HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectNotSuccessful );
        yield return Case ( "COMP-EXPECT-UNKNOWN", "Unknown Expect value should be rejected with 417",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nExpect: something-else\r\nContent-Length: 0\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "COMP-GET-WITH-CL-BODY", "GET with Content-Length and body is semantically unusual but body-framed",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nContent-Length: 5\r\nConnection: close\r\n\r\nHello" ),
            e => ExpectStatus ( e, 200 ) );
        yield return Case ( "COMP-CHUNKED-EXTENSION", "Chunk extension should be accepted or may be rejected",
            u => Bytes ( $"POST /tests/httprequest/getBodyContents HTTP/1.1\r\nHost: {Authority ( u )}\r\nTransfer-Encoding: chunked\r\nConnection: close\r\n\r\n5;name=value\r\nHello\r\n0\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "COMP-METHOD-TRACE", "TRACE request should be disabled",
            u => Bytes ( $"TRACE /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectNotSuccessful );
        yield return Case ( "COMP-HOST-EMPTY-VALUE", "Empty Host header value must be rejected",
            _ => Bytes ( "GET /tests/plaintext HTTP/1.1\r\nHost:\r\nConnection: close\r\n\r\n" ),
            ExpectBadRequestOrClosed );
        yield return Case ( "COMP-REQUEST-LINE-TAB", "Tab as request-line delimiter should be rejected but may parse on whitespace",
            u => Bytes ( $"GET\t/tests/plaintext\tHTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
        yield return Case ( "COMP-VERSION-MISSING-MINOR", "HTTP/1 with no minor version digit is invalid",
            u => Bytes ( $"GET /tests/plaintext HTTP/1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-VERSION-LEADING-ZEROS", "Leading zeros in HTTP version digits are invalid",
            u => Bytes ( $"GET /tests/plaintext HTTP/01.01\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-VERSION-WHITESPACE", "Whitespace inside version token is invalid",
            u => Bytes ( $"GET /tests/plaintext HTTP/ 1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectProtocolRejected );
        yield return Case ( "COMP-CONNECTION-CLOSE", "Server must close connection after Connection: close",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.1\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            e => ExpectStatus ( e, 200 ) ?? ExpectClosed ( e ) );
        yield return Case ( "COMP-HTTP10-DEFAULT-CLOSE", "HTTP/1.0 without keep-alive should close after response",
            _ => Bytes ( "GET /tests/plaintext HTTP/1.0\r\n\r\n" ),
            e => ExpectStatus ( e, 200 ) ?? ExpectClosed ( e ) );
        yield return Case ( "COMP-HTTP10-NO-HOST", "HTTP/1.0 without Host header is valid",
            _ => Bytes ( "GET /tests/plaintext HTTP/1.0\r\nConnection: close\r\n\r\n" ),
            e => ExpectStatus ( e, 200 ) );
        yield return Case ( "COMP-HTTP12-VERSION", "HTTP/1.2 should be accepted as HTTP/1.x compatible",
            u => Bytes ( $"GET /tests/plaintext HTTP/1.2\r\nHost: {Authority ( u )}\r\nConnection: close\r\n\r\n" ),
            ExpectAcceptedOrRejected );
    }

    public static string GetCaseDisplayName ( MethodInfo methodInfo, object [] data ) =>
        data [ 0 ] is ComplianceCase testCase ? $"{methodInfo.Name}_{testCase.Id}" : methodInfo.Name;

    [DataTestMethod]
    [DynamicData ( nameof ( ComplianceCases ), DynamicDataSourceType.Method, DynamicDataDisplayName = nameof ( GetCaseDisplayName ) )]
    public async Task Compliance_Rfc9110_Rfc9112 ( ComplianceCase testCase ) {
        if (!IsCadenteTestEngine ())
            Assert.Inconclusive ( "The raw HTTP compliance suite targets the Cadente engine; HttpListener has different parsing semantics." );

        var exchange = await SendRawAsync (
            testCase.RequestFactory ( GetServerUri () ),
            testCase.TimeoutMs,
            testCase.ShutdownSendAfterWrite );

        string? failure = testCase.Validate ( exchange );
        if (failure is { Length: > 0 })
            Assert.Fail ( $"{testCase.Id}: {testCase.Description}. {failure} Raw response: {exchange.RawText}" );
    }

    private static Uri GetServerUri () =>
        new ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );

    private static bool IsCadenteTestEngine () =>
        string.Equals ( Environment.GetEnvironmentVariable ( "SISK_TEST_ENGINE" ), "Cadente", StringComparison.OrdinalIgnoreCase );

    private static object [] Case (
        string id,
        string description,
        Func<Uri, byte []> requestFactory,
        Func<RawHttpExchange, string?> validate,
        int timeoutMs = DefaultTimeoutMs,
        bool shutdownSendAfterWrite = false ) =>
        [ new ComplianceCase ( id, description, requestFactory, validate, timeoutMs, shutdownSendAfterWrite ) ];

    private static byte [] Get ( Uri uri, string path ) =>
        Bytes ( $"GET {path} HTTP/1.1\r\nHost: {Authority ( uri )}\r\nConnection: close\r\n\r\n" );

    private static byte [] Bytes ( string value ) =>
        Encoding.Latin1.GetBytes ( value );

    private static string Authority ( Uri uri ) =>
        $"{uri.Host}:{uri.Port}";

    private static async Task<RawHttpExchange> SendRawAsync ( byte [] requestBytes, int timeoutMs, bool shutdownSendAfterWrite ) {
        var uri = GetServerUri ();
        using var cts = new CancellationTokenSource ( TimeSpan.FromMilliseconds ( timeoutMs ) );
        using var tcp = new TcpClient ();
        var responseBytes = new List<byte> ();
        bool closed = false;
        bool timedOut = false;

        try {
            await tcp.ConnectAsync ( uri.Host, uri.Port, cts.Token );
            await using var stream = tcp.GetStream ();
            await stream.WriteAsync ( requestBytes, cts.Token );
            await stream.FlushAsync ( cts.Token );

            if (shutdownSendAfterWrite)
                tcp.Client.Shutdown ( SocketShutdown.Send );

            byte [] buffer = new byte [ 4096 ];
            while (true) {
                int read = await stream.ReadAsync ( buffer, cts.Token );
                if (read == 0) {
                    closed = true;
                    break;
                }
                responseBytes.AddRange ( buffer [ ..read ] );
            }
        }
        catch (OperationCanceledException) {
            timedOut = true;
        }
        catch (IOException) {
            closed = true;
        }
        catch (SocketException) {
            closed = true;
        }
        catch (ObjectDisposedException) {
            closed = true;
        }

        string rawText = Encoding.Latin1.GetString ( responseBytes.ToArray () );
        return new RawHttpExchange ( rawText, ParseResponses ( rawText ), closed, timedOut );
    }

    private static IReadOnlyList<RawHttpResponse> ParseResponses ( string rawText ) {
        var responses = new List<RawHttpResponse> ();
        int index = 0;

        while (index < rawText.Length) {
            int statusIndex = FindStatusLine ( rawText, index );
            if (statusIndex < 0)
                break;

            int statusLineEnd = rawText.IndexOf ( "\r\n", statusIndex, StringComparison.Ordinal );
            if (statusLineEnd < 0)
                break;

            string statusLine = rawText [ statusIndex..statusLineEnd ];
            string [] statusParts = statusLine.Split ( ' ', 3, StringSplitOptions.RemoveEmptyEntries );
            if (statusParts.Length < 2 || !int.TryParse ( statusParts [ 1 ], out int status ))
                break;

            int headersStart = statusLineEnd + 2;
            int headersEnd = rawText.IndexOf ( "\r\n\r\n", headersStart, StringComparison.Ordinal );
            if (headersEnd < 0) {
                responses.Add ( new RawHttpResponse ( status, string.Empty, statusLine ) );
                break;
            }

            string headersText = rawText [ headersStart..headersEnd ];
            int bodyStart = headersEnd + 4;
            int bodyLength = GetContentLength ( headersText );
            int bodyEnd = Math.Min ( bodyStart + bodyLength, rawText.Length );
            string body = bodyEnd > bodyStart ? rawText [ bodyStart..bodyEnd ] : string.Empty;

            responses.Add ( new RawHttpResponse ( status, body, statusLine ) );
            index = bodyEnd > bodyStart ? bodyEnd : bodyStart;
        }

        return responses;
    }

    private static int FindStatusLine ( string rawText, int startIndex ) {
        int index = rawText.IndexOf ( "HTTP/", startIndex, StringComparison.Ordinal );
        while (index > 0 && rawText [ index - 1 ] != '\n')
            index = rawText.IndexOf ( "HTTP/", index + 1, StringComparison.Ordinal );
        return index;
    }

    private static int GetContentLength ( string headersText ) {
        foreach (string line in headersText.Split ( "\r\n", StringSplitOptions.None )) {
            if (!line.StartsWith ( "Content-Length:", StringComparison.OrdinalIgnoreCase ))
                continue;

            if (int.TryParse ( line [ "Content-Length:".Length.. ].Trim (), out int contentLength ))
                return Math.Max ( 0, contentLength );
        }

        return 0;
    }

    private static string? ExpectStatus ( RawHttpExchange exchange, int expectedStatus ) {
        int? status = exchange.FirstStatus;
        return status == expectedStatus
            ? null
            : $"Expected status {expectedStatus}, got {FormatStatus ( status )}.";
    }

    private static string? ExpectStatusAndBody ( RawHttpExchange exchange, int expectedStatus, string expectedBody ) {
        string? statusFailure = ExpectStatus ( exchange, expectedStatus );
        if (statusFailure is not null)
            return statusFailure;

        string body = exchange.Responses [ 0 ].Body;
        return body == expectedBody
            ? null
            : $"Expected body '{expectedBody}', got '{body}'.";
    }

    private static string? ExpectBadRequestOrClosed ( RawHttpExchange exchange ) =>
        exchange.FirstStatus is null || exchange.FirstStatus == 400
            ? null
            : $"Expected 400 or connection close, got {FormatStatus ( exchange.FirstStatus )}.";

    private static string? ExpectProtocolRejected ( RawHttpExchange exchange ) =>
        exchange.FirstStatus switch {
            null => null,
            400 or 414 or 431 or 501 => null,
            _ => $"Expected protocol rejection or connection close, got {FormatStatus ( exchange.FirstStatus )}."
        };

    private static string? ExpectNotSuccessful ( RawHttpExchange exchange ) =>
        exchange.FirstStatus is null || exchange.FirstStatus >= 400
            ? null
            : $"Expected non-successful status or connection close, got {FormatStatus ( exchange.FirstStatus )}.";

    private static string? ExpectParsedByServer ( RawHttpExchange exchange ) =>
        exchange.FirstStatus is null || exchange.FirstStatus is 400 or 414 or 431
            ? $"Expected request to be parsed by the server, got {FormatStatus ( exchange.FirstStatus )}."
            : null;

    private static string? ExpectAcceptedOrRejected ( RawHttpExchange exchange ) =>
        exchange.FirstStatus is null || exchange.FirstStatus == 200 || exchange.FirstStatus >= 400
            ? null
            : $"Expected acceptance, rejection, or connection close, got {FormatStatus ( exchange.FirstStatus )}.";

    private static string? ExpectIncompleteOrRejected ( RawHttpExchange exchange ) =>
        exchange.TimedOut || exchange.FirstStatus is null || exchange.FirstStatus >= 400
            ? null
            : $"Expected incomplete transfer timeout, rejection, or connection close, got {FormatStatus ( exchange.FirstStatus )}.";

    private static string? ExpectClosed ( RawHttpExchange exchange ) =>
        exchange.Closed
            ? null
            : "Expected the server to close the TCP connection.";

    private static string FormatStatus ( int? status ) =>
        status?.ToString () ?? "connection closed";

    public sealed record ComplianceCase (
        string Id,
        string Description,
        Func<Uri, byte []> RequestFactory,
        Func<RawHttpExchange, string?> Validate,
        int TimeoutMs,
        bool ShutdownSendAfterWrite );

    public sealed record RawHttpExchange (
        string RawText,
        IReadOnlyList<RawHttpResponse> Responses,
        bool Closed,
        bool TimedOut ) {
        public int? FirstStatus => Responses.Count > 0 ? Responses [ 0 ].Status : null;
    }

    public sealed record RawHttpResponse ( int Status, string Body, string StatusLine );
}
