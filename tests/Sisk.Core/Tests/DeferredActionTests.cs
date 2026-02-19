using System.Collections.Specialized;
using System.Net;
using System.Net.WebSockets;
using System.Reflection;
using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Http.Engine;
using Sisk.Core.Routing;
using tests.TestUtils;

namespace tests.Tests;

[TestClass]
public sealed class DeferredActionTests {

    private static string NewId () => Guid.NewGuid ().ToString ( "N" );

    [TestMethod]
    public async Task EnqueueDeferredAction_NormalRequest_ExecutesAfterHandlerReturns () {
        string id = NewId ();
        var state = DeferredActionsTestState.GetOrCreate ( id );

        try {
            using var client = Server.GetHttpClient ();

            string response = await client.GetStringAsync ( $"/tests/deferred/plain?id={id}" );
            Assert.AreEqual ( "ok", response );

            _ = await state.DeferredExecuted.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );

            var log = state.Log.ToArray ();
            CollectionAssert.AreEqual ( new [] { "handler-start", "handler-before-return", "deferred" }, log );
        }
        finally {
            DeferredActionsTestState.Remove ( id );
        }
    }

    [TestMethod]
    public async Task EnqueueDeferredAction_ResponseStreaming_ExecutesOnlyAfterStreamIsClosed () {
        string id = NewId ();
        var state = DeferredActionsTestState.GetOrCreate ( id );

        try {
            using var client = Server.GetHttpClient ();
            using var req = new HttpRequestMessage ( HttpMethod.Get, $"/tests/deferred/responsestream/wait?id={id}" );

            using var res = await client.SendAsync ( req, HttpCompletionOption.ResponseHeadersRead );
            res.EnsureSuccessStatusCode ();

            await using var body = await res.Content.ReadAsStreamAsync ();
            using var reader = new StreamReader ( body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true );

            string? line1 = await reader.ReadLineAsync ().WaitAsync ( TimeSpan.FromSeconds ( 5 ) );
            Assert.AreEqual ( "part1", line1 );

            Assert.IsFalse ( state.DeferredExecuted.Task.IsCompleted, "Deferred action executed while response stream was still open." );

            state.AllowFinish.TrySetResult ( true );

            string? line2 = await reader.ReadLineAsync ().WaitAsync ( TimeSpan.FromSeconds ( 5 ) );
            Assert.AreEqual ( "part2", line2 );

            _ = await state.DeferredExecuted.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );

            var log = state.Log.ToArray ();
            int wrotePart2Index = Array.IndexOf ( log, "wrote-part2" );
            int deferredIndex = Array.IndexOf ( log, "deferred" );

            Assert.IsTrue ( wrotePart2Index >= 0, "Missing wrote-part2 marker." );
            Assert.IsTrue ( deferredIndex > wrotePart2Index, "Deferred action should run after stream was closed." );
        }
        finally {
            DeferredActionsTestState.Remove ( id );
        }
    }

    [TestMethod]
    public async Task EnqueueDeferredAction_Sse_ExecutesOnlyAfterConnectionIsClosed () {
        string id = NewId ();
        var state = DeferredActionsTestState.GetOrCreate ( id );

        try {
            using var client = Server.GetHttpClient ();

            using var req = new HttpRequestMessage ( HttpMethod.Get, $"/tests/deferred/sse/wait?id={id}" );
            req.Headers.Accept.ParseAdd ( "text/event-stream" );

            using var res = await client.SendAsync ( req, HttpCompletionOption.ResponseHeadersRead );
            res.EnsureSuccessStatusCode ();

            await using var body = await res.Content.ReadAsStreamAsync ();
            using var reader = new StreamReader ( body, Encoding.UTF8, detectEncodingFromByteOrderMarks: false, bufferSize: 1024, leaveOpen: true );

            string? firstDataLine = await ReadUntilAsync ( reader, static line => line.StartsWith ( "data: part1", StringComparison.Ordinal ), TimeSpan.FromSeconds ( 5 ) );
            Assert.IsNotNull ( firstDataLine );

            Assert.IsFalse ( state.DeferredExecuted.Task.IsCompleted, "Deferred action executed while SSE connection was still open." );

            state.AllowFinish.TrySetResult ( true );

            string? secondDataLine = await ReadUntilAsync ( reader, static line => line.StartsWith ( "data: part2", StringComparison.Ordinal ), TimeSpan.FromSeconds ( 5 ) );
            Assert.IsNotNull ( secondDataLine );

            _ = await state.DeferredExecuted.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );

            var log = state.Log.ToArray ();
            int sentPart2Index = Array.IndexOf ( log, "sent-part2" );
            int deferredIndex = Array.IndexOf ( log, "deferred" );

            Assert.IsTrue ( sentPart2Index >= 0, "Missing sent-part2 marker." );
            Assert.IsTrue ( deferredIndex > sentPart2Index, "Deferred action should run after SSE was closed." );
        }
        finally {
            DeferredActionsTestState.Remove ( id );
        }
    }

    [TestMethod]
    public async Task EnqueueDeferredAction_WebSocket_ExecutesOnlyAfterSocketIsClosed () {
        string id = NewId ();
        var state = DeferredActionsTestState.GetOrCreate ( id );

        try {
            var uri = GetWebSocketServerUri ( "/tests/deferred/ws/wait", $"id={id}" );
            using var cts = new CancellationTokenSource ( TimeSpan.FromSeconds ( 10 ) );

            using var client = new ClientWebSocket ();
            await client.ConnectAsync ( uri, cts.Token );

            string ready = await ReceiveTextAsync ( client, cts.Token );
            Assert.AreEqual ( "ready", ready );

            await client.SendAsync ( Encoding.UTF8.GetBytes ( "ping" ), WebSocketMessageType.Text, true, cts.Token );

            await Task.Delay ( 200, cts.Token );
            Assert.IsFalse ( state.DeferredExecuted.Task.IsCompleted, "Deferred action executed while WebSocket was still open." );

            await client.CloseAsync ( WebSocketCloseStatus.NormalClosure, "bye", cts.Token );

            _ = await state.DeferredExecuted.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );
        }
        finally {
            DeferredActionsTestState.Remove ( id );
        }
    }

    [TestMethod]
    public async Task EnqueueDeferredAction_Order_MixedSyncAndAsync_IsFifo () {
        string id = NewId ();
        var state = DeferredActionsTestState.GetOrCreate ( id );

        try {
            using var client = Server.GetHttpClient ();
            string response = await client.GetStringAsync ( $"/tests/deferred/order?id={id}" );
            Assert.AreEqual ( "ok", response );

            _ = await state.DeferredExecuted.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );

            var log = state.Log.ToArray ();
            CollectionAssert.AreEqual ( new [] { "1", "2", "3", "4" }, log );
        }
        finally {
            DeferredActionsTestState.Remove ( id );
        }
    }

    [TestMethod]
    public async Task EnqueueDeferredAction_CancellationToken_IsRespected () {
        string id = NewId ();
        var state = DeferredActionsTestState.GetOrCreate ( id );

        try {
            using var client = Server.GetHttpClient ();
            string response = await client.GetStringAsync ( $"/tests/deferred/cancel?id={id}" );
            Assert.AreEqual ( "ok", response );

            _ = await state.DeferredExecuted.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );

            var log = state.Log.ToArray ();
            CollectionAssert.Contains ( log, "noncancelled" );
            CollectionAssert.DoesNotContain ( log, "cancelled-action-ran" );
        }
        finally {
            DeferredActionsTestState.Remove ( id );
        }
    }

    private static async Task<string?> ReadUntilAsync ( StreamReader reader, Func<string, bool> predicate, TimeSpan timeout ) {
        using var cts = new CancellationTokenSource ( timeout );

        while (true) {
            string? line;
            try {
                line = await reader.ReadLineAsync ().WaitAsync ( cts.Token );
            }
            catch (OperationCanceledException) {
                return null;
            }

            if (line is null) {
                return null;
            }

            if (predicate ( line )) {
                return line;
            }
        }
    }

    private static Uri GetWebSocketServerUri ( string path, string? query = null ) {
        var serverUri = new Uri ( Server.Instance.HttpServer.ListeningPrefixes [ 0 ] );
        return new UriBuilder ( serverUri.Scheme == "https" ? "wss" : "ws", serverUri.Host, serverUri.Port, path ) {
            Query = query
        }.Uri;
    }

    private static async Task<string> ReceiveTextAsync ( ClientWebSocket client, CancellationToken cancellation ) {
        var buffer = new byte [ 4096 ];
        var segment = new ArraySegment<byte> ( buffer );
        var result = await client.ReceiveAsync ( segment, cancellation );
        return Encoding.UTF8.GetString ( buffer, 0, result.Count );
    }
}

[TestClass]
public sealed class DeferredActionExceptionTests {

    [TestMethod]
    public void EnqueueDeferredAction_ThrowsFromSyncAction_WhenThrowExceptionsIsTrue () {
        HttpServer server = HttpServer.Emit ( 12111, out var config, out _, out var router );
        config.ThrowExceptions = true;
        config.AccessLogsStream = null;
        config.ErrorsLogsStream = null;

        router.SetRoute ( RouteMethod.Get, "/tests/deferred/throw/sync", ( HttpRequest req ) => {
            req.Context.EnqueueDeferredAction ( () => throw new InvalidOperationException ( "boom" ) );
            return new HttpResponse ( "ok" );
        } );

        var context = new FakeEngineContext ( new Uri ( "http://localhost:12111/tests/deferred/throw/sync" ) );

        Assert.ThrowsException<InvalidOperationException> ( () => InvokeProcessRequest ( server, context ) );
    }

    [TestMethod]
    public void EnqueueDeferredAction_ThrowsFromAsyncAction_WhenThrowExceptionsIsTrue () {
        HttpServer server = HttpServer.Emit ( 12112, out var config, out _, out var router );
        config.ThrowExceptions = true;
        config.AccessLogsStream = null;
        config.ErrorsLogsStream = null;

        router.SetRoute ( RouteMethod.Get, "/tests/deferred/throw/async", ( HttpRequest req ) => {
            req.Context.EnqueueDeferredAction ( async () => {
                await Task.Yield ();
                throw new InvalidOperationException ( "boom" );
            } );
            return new HttpResponse ( "ok" );
        } );

        var context = new FakeEngineContext ( new Uri ( "http://localhost:12112/tests/deferred/throw/async" ) );

        Assert.ThrowsException<InvalidOperationException> ( () => InvokeProcessRequest ( server, context ) );
    }

    [TestMethod]
    public void EnqueueDeferredAction_EnqueuedBeforeRouterThrows_IsNotExecuted () {
        bool deferredCalled = false;

        HttpServer server = HttpServer.Emit ( 12113, out var config, out _, out var router );
        config.ThrowExceptions = true;
        config.AccessLogsStream = null;
        config.ErrorsLogsStream = null;

        router.SetRoute ( RouteMethod.Get, "/tests/deferred/router-throws/sync", ( HttpRequest req ) => {
            req.Context.EnqueueDeferredAction ( () => deferredCalled = true );
            throw new InvalidOperationException ( "router boom" );
        } );

        var context = new FakeEngineContext ( new Uri ( "http://localhost:12113/tests/deferred/router-throws/sync" ) );

        Assert.ThrowsException<InvalidOperationException> ( () => InvokeProcessRequest ( server, context ) );
        Assert.IsFalse ( deferredCalled, "Deferred action should not execute when router throws before response is finished." );
    }

    [TestMethod]
    public void EnqueueDeferredAction_EnqueuedBeforeRouterThrowsAsync_IsNotExecuted () {
        bool deferredCalled = false;

        HttpServer server = HttpServer.Emit ( 12114, out var config, out _, out var router );
        config.ThrowExceptions = true;
        config.AccessLogsStream = null;
        config.ErrorsLogsStream = null;

        router.SetRoute ( RouteMethod.Get, "/tests/deferred/router-throws/async", ( HttpRequest req ) => {
            req.Context.EnqueueDeferredAction ( () => deferredCalled = true );
            return Task.FromException<HttpResponse> ( new InvalidOperationException ( "router boom" ) );
        } );

        var context = new FakeEngineContext ( new Uri ( "http://localhost:12114/tests/deferred/router-throws/async" ) );

        Assert.ThrowsException<InvalidOperationException> ( () => InvokeProcessRequest ( server, context ) );
        Assert.IsFalse ( deferredCalled, "Deferred action should not execute when async router throws before response is finished." );
    }

    private static void InvokeProcessRequest ( HttpServer server, HttpServerEngineContext context ) {
        var method = typeof ( HttpServer ).GetMethod ( "ProcessRequest", BindingFlags.Instance | BindingFlags.NonPublic );
        Assert.IsNotNull ( method, "Could not locate HttpServer.ProcessRequest via reflection." );

        try {
            method.Invoke ( server, new object [] { context } );
        }
        catch (TargetInvocationException ex) when (ex.InnerException is not null) {
            throw ex.InnerException;
        }
    }

    private sealed class FakeEngineContext : HttpServerEngineContext {
        private readonly FakeRequest _request;
        private readonly FakeResponse _response;

        public FakeEngineContext ( Uri url ) {
            _response = new FakeResponse ();
            _request = new FakeRequest ( url );
        }

        public override HttpServerEngineContextRequest Request => _request;

        public override HttpServerEngineContextResponse Response => _response;

        public override Task<HttpServerEngineWebSocket> AcceptWebSocketAsync ( string? subProtocol ) {
            throw new NotSupportedException ();
        }
    }

    private sealed class FakeRequest : HttpServerEngineContextRequest {
        private readonly NameValueCollection _query;
        private readonly NameValueCollection _headers;

        public FakeRequest ( Uri url ) {
            Url = url;
            RawUrl = url.PathAndQuery;

            _headers = new NameValueCollection ( StringComparer.OrdinalIgnoreCase ) {
                { "Host", url.Authority }
            };

            _query = ParseQueryString ( url.Query );
        }

        public override bool IsLocal => true;

        public override string? RawUrl { get; }

        public override NameValueCollection QueryString => _query;

        public override Version ProtocolVersion => HttpVersion.Version11;

        public override string UserHostName => "localhost";

        public override Uri? Url { get; }

        public override string HttpMethod => "GET";

        public override IPEndPoint LocalEndPoint => new ( IPAddress.Loopback, Url!.Port );

        public override IPEndPoint RemoteEndPoint => new ( IPAddress.Loopback, 34567 );

        public override Guid RequestTraceIdentifier { get; } = Guid.NewGuid ();

        public override NameValueCollection Headers => _headers;

        public override Stream InputStream => Stream.Null;

        public override long ContentLength64 => 0;

        public override bool IsSecureConnection => false;

        public override Encoding ContentEncoding => Encoding.UTF8;

        private static NameValueCollection ParseQueryString ( string query ) {
            var nvc = new NameValueCollection ( StringComparer.OrdinalIgnoreCase );
            if (string.IsNullOrEmpty ( query )) {
                return nvc;
            }

            string q = query;
            if (q.StartsWith ( "?", StringComparison.Ordinal )) {
                q = q [ 1.. ];
            }

            foreach (string part in q.Split ( '&', StringSplitOptions.RemoveEmptyEntries )) {
                int idx = part.IndexOf ( '=' );
                if (idx < 0) {
                    nvc.Add ( Uri.UnescapeDataString ( part ), string.Empty );
                }
                else {
                    string key = Uri.UnescapeDataString ( part [ ..idx ] );
                    string value = Uri.UnescapeDataString ( part [ (idx + 1).. ] );
                    nvc.Add ( key, value );
                }
            }

            return nvc;
        }
    }

    private sealed class FakeResponse : HttpServerEngineContextResponse {
        private readonly MemoryStream _body = new ();
        private readonly FakeHeaderList _headers = new ();

        public override int StatusCode { get; set; } = 200;

        public override string StatusDescription { get; set; } = "OK";

        public override bool KeepAlive { get; set; }

        public override bool SendChunked { get; set; }

        public override long ContentLength64 { get; set; }

        public override string? ContentType { get; set; }

        public override IHttpEngineHeaderList Headers => _headers;

        public override void AppendHeader ( string name, string value ) => _headers.AppendHeader ( name, value );

        public override void Abort () {
        }

        public override void Close () {
        }

        public override void Dispose () {
            _body.Dispose ();
        }

        public override Stream OutputStream => _body;
    }

    private sealed class FakeHeaderList : IHttpEngineHeaderList {
        private readonly Dictionary<string, List<string>> _items = new ( StringComparer.OrdinalIgnoreCase );

        public int Count => _items.Count;

        public string [] DefinedHeaderNames => _items.Keys.ToArray ();

        public void Clear () => _items.Clear ();

        public bool Contains ( string name ) => _items.ContainsKey ( name );

        public void AppendHeader ( string name, string value ) {
            if (!_items.TryGetValue ( name, out var list )) {
                list = new List<string> ();
                _items.Add ( name, list );
            }
            list.Add ( value );
        }

        public void SetHeader ( string name, string value ) {
            _items [ name ] = new List<string> { value };
        }

        public string [] GetHeader ( string name ) {
            return _items.TryGetValue ( name, out var list ) ? list.ToArray () : Array.Empty<string> ();
        }
    }
}
