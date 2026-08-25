using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;

namespace Sisk.Monitoring.Tests;

[TestClass]
public sealed class MonitoringBuilderExtensionsTests {

    static HttpRequestMessage CreateAuthorizedRequest ( string path ) {
        var request = new HttpRequestMessage ( HttpMethod.Get, path );
        request.Headers.Authorization = new AuthenticationHeaderValue ( "Basic", Convert.ToBase64String ( Encoding.UTF8.GetBytes ( "test:test" ) ) );
        return request;
    }

    static HttpServerHostContextBuilder CreateBuilder ( int port ) {
        var builder = HttpServer.CreateBuilder ()
            .UseListeningPort ( $"http://127.0.0.1:{port}/" )
            .UseConfiguration ( configuration => {
                configuration.AccessLogsStream = null;
                configuration.ErrorsLogsStream = null;
            } );

        return builder;
    }

    static int GetFreePort () {
        using var listener = new TcpListener ( IPAddress.Loopback, 0 );
        listener.Start ();
        return ((IPEndPoint) listener.LocalEndpoint).Port;
    }

    [DataTestMethod]
    [DataRow ( false )]
    [DataRow ( true )]
    public async Task UseMonitoring_ServesStaticDashboardAndCountsHttpResponsesByStatusClass ( bool useCustomMonitorFactory ) {
        int port = GetFreePort ();
        var builder = CreateBuilder ( port );

        if (useCustomMonitorFactory) {
            builder.UseMonitoring ( "/monitoring", () => new TestApplicationMonitor {
                CredentialValidator = _ => ValueTask.FromResult ( true )
            } );
        }
        else {
            builder.UseMonitoring ( "/monitoring", monitor => {
                monitor.CredentialValidator = _ => ValueTask.FromResult ( true );
            } );
        }

        using var server = builder
            .UseRouter ( router => {
                router.MapGet ( "/success", _ => new HttpResponse ( 200 ) );
                router.MapGet ( "/client-error", _ => new HttpResponse ( 404 ) );
                router.MapGet ( "/server-error", _ => new HttpResponse ( 500 ) );
            } )
            .Build ();

        server.Start ( verbose: false, preventHault: false );

        using var client = new HttpClient {
            BaseAddress = new Uri ( $"http://127.0.0.1:{port}/" ),
            Timeout = TimeSpan.FromSeconds ( 3 )
        };

        using HttpResponseMessage successResponse = await client.GetAsync ( "/success" );
        using HttpResponseMessage clientErrorResponse = await client.GetAsync ( "/client-error" );
        using HttpResponseMessage serverErrorResponse = await client.GetAsync ( "/server-error" );

        Assert.AreEqual ( HttpStatusCode.OK, successResponse.StatusCode );
        Assert.AreEqual ( HttpStatusCode.NotFound, clientErrorResponse.StatusCode );
        Assert.AreEqual ( HttpStatusCode.InternalServerError, serverErrorResponse.StatusCode );

        using (HttpResponseMessage dashboardResponse = await client.SendAsync ( CreateAuthorizedRequest ( "/monitoring/" ) )) {
            string dashboard = await dashboardResponse.Content.ReadAsStringAsync ();

            Assert.AreEqual ( HttpStatusCode.OK, dashboardResponse.StatusCode );
            StringAssert.Contains ( dashboard, "HTTP Responses" );
            StringAssert.Contains ( dashboard, "class=\"http-response-chart\"" );
            StringAssert.Contains ( dashboard, "class=\"http-response-chart-container\"" );
            StringAssert.Contains ( dashboard, "data-http-responses-endpoint=\"/monitoring/data\"" );
            StringAssert.Contains ( dashboard, "data-http-total=\"success\"" );
            StringAssert.Contains ( dashboard, "data-http-percent=\"clientError\"" );
            StringAssert.Contains ( dashboard, "aria-label=\"HTTP responses chart\"" );
            Assert.IsFalse ( dashboard.Contains ( "data-http-responses-readings=\"" ), "Dashboard shell must not embed live readings." );
        }

        using (HttpResponseMessage dataResponse = await client.SendAsync ( CreateAuthorizedRequest ( "/monitoring/data" ) )) {
            string dataJson = await dataResponse.Content.ReadAsStringAsync ();

            Assert.AreEqual ( HttpStatusCode.OK, dataResponse.StatusCode );
            StringAssert.Contains ( dataResponse.Content.Headers.ContentType?.ToString (), "application/json" );
            StringAssert.Contains ( dataJson, "\"totals\":{\"success\":2,\"clientError\":1,\"serverError\":1}" );
            StringAssert.Contains ( dataJson, "\"readings\":" );
            StringAssert.Contains ( dataJson, "\"counters\":[" );
        }
    }

    [TestMethod]
    public async Task UseMonitoring_ServesCounterAndLogStreamDataEndpoints () {
        int port = GetFreePort ();
        var counter = new Counter ();
        counter.Increment ();
        counter.Increment ( 3 );

        var logStream = new LogStream ();

        using var server = CreateBuilder ( port )
            .UseMonitoring ( "/monitoring", monitor => {
                monitor.CredentialValidator = _ => ValueTask.FromResult ( true );
                monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", counter ) { DashboardPinned = true } );
                monitor.CaptureLogStream ( new MonitoringDefinition<LogStream> ( "AppLog", logStream ) );
            } )
            .Build ();

        server.Start ( verbose: false, preventHault: false );

        logStream.WriteLine ( "2026-01-01 10:00:00 [Info] line one happened" );
        logStream.WriteLine ( "2026-01-01 10:00:01 [Info] line two happened" );
        logStream.Flush ();

        using var client = new HttpClient {
            BaseAddress = new Uri ( $"http://127.0.0.1:{port}/" ),
            Timeout = TimeSpan.FromSeconds ( 3 )
        };

        using (HttpResponseMessage countersResponse = await client.SendAsync ( CreateAuthorizedRequest ( "/monitoring/counters/data" ) )) {
            string countersJson = await countersResponse.Content.ReadAsStringAsync ();

            Assert.AreEqual ( HttpStatusCode.OK, countersResponse.StatusCode );
            StringAssert.Contains ( countersResponse.Content.Headers.ContentType?.ToString (), "application/json" );
            StringAssert.Contains ( countersJson, "\"id\":\"Requests\"" );
            StringAssert.Contains ( countersJson, "\"value\":4" );
        }

        using (HttpResponseMessage pageResponse = await client.SendAsync ( CreateAuthorizedRequest ( "/monitoring/logstream/AppLog" ) )) {
            string pageHtml = await pageResponse.Content.ReadAsStringAsync ();

            Assert.AreEqual ( HttpStatusCode.OK, pageResponse.StatusCode );
            StringAssert.Contains ( pageHtml, "data-logstream-endpoint=\"/monitoring/logstream/AppLog/data\"" );
            Assert.IsFalse ( pageHtml.Contains ( "line one happened" ), "Log stream page must be a static shell." );
        }

        using (HttpResponseMessage logResponse = await client.SendAsync ( CreateAuthorizedRequest ( "/monitoring/logstream/AppLog/data" ) )) {
            string logJson = await logResponse.Content.ReadAsStringAsync ();

            Assert.AreEqual ( HttpStatusCode.OK, logResponse.StatusCode );
            StringAssert.Contains ( logJson, "\"lines\":" );
            Assert.IsTrue ( logJson.IndexOf ( "line one happened" ) < logJson.IndexOf ( "line two happened" ), "Lines must be returned oldest first." );
        }

        using (HttpResponseMessage missingResponse = await client.SendAsync ( CreateAuthorizedRequest ( "/monitoring/logstream/Unknown/data" ) )) {
            Assert.AreEqual ( HttpStatusCode.NotFound, missingResponse.StatusCode );
        }
    }

    [TestMethod]
    public async Task UseMonitoring_ServerHealthSupportsSelectableAggregatedPeriods () {
        int port = GetFreePort ();
        DateTime now = DateTime.Now;
        var health = Enumerable.Range ( 0, 31 * 24 * 60 )
            .Select ( minute => new MonitoringHealthSnapshot (
                now - TimeSpan.FromMinutes ( minute + 1 ),
                minute % 100,
                40,
                20 ) )
            .OrderBy ( snapshot => snapshot.Timestamp )
            .ToArray ();
        var store = new MemoryMonitoringStore ( new MonitoringSnapshot ( now, [], [], health ) );

        using var server = CreateBuilder ( port )
            .UseMonitoring ( "/monitoring", monitor => {
                monitor.CredentialValidator = _ => ValueTask.FromResult ( true );
                monitor.Store = store;
            } )
            .Build ();

        server.Start ( verbose: false, preventHault: false );

        using var client = new HttpClient {
            BaseAddress = new Uri ( $"http://127.0.0.1:{port}/" ),
            Timeout = TimeSpan.FromSeconds ( 3 )
        };

        using (HttpResponseMessage pageResponse = await client.SendAsync ( CreateAuthorizedRequest ( "/monitoring/health" ) )) {
            string pageHtml = await pageResponse.Content.ReadAsStringAsync ();

            Assert.AreEqual ( HttpStatusCode.OK, pageResponse.StatusCode );
            foreach (string period in new [] { "1h", "1d", "7d", "30d" })
                StringAssert.Contains ( pageHtml, $"data-health-period=\"{period}\"" );
        }

        foreach ((string period, int maximumReadings) in new [] { ("1h", 61), ("1d", 289), ("7d", 337), ("30d", 361) }) {
            using HttpResponseMessage dataResponse = await client.SendAsync ( CreateAuthorizedRequest ( $"/monitoring/health/data?period={period}" ) );
            using JsonDocument payload = JsonDocument.Parse ( await dataResponse.Content.ReadAsStringAsync () );
            int readingCount = payload.RootElement.GetProperty ( "readings" ).GetArrayLength ();

            Assert.AreEqual ( HttpStatusCode.OK, dataResponse.StatusCode );
            Assert.IsTrue ( readingCount > 1, $"Period {period} must contain a useful history." );
            Assert.IsTrue ( readingCount <= maximumReadings, $"Period {period} must be downsampled." );
        }
    }

    sealed class MemoryMonitoringStore ( MonitoringSnapshot snapshot ) : IMonitoringStore {

        MonitoringSnapshot snapshot = snapshot;

        public Task SaveAsync ( MonitoringSnapshot snapshot, CancellationToken cancellationToken = default ) {
            this.snapshot = snapshot;
            return Task.CompletedTask;
        }

        public Task<MonitoringSnapshot?> LoadAsync ( CancellationToken cancellationToken = default ) {
            return Task.FromResult<MonitoringSnapshot?> ( snapshot );
        }
    }

    sealed class TestApplicationMonitor : ApplicationMonitor;
}
