using System.Net;
using System.Net.Http.Headers;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;

namespace Sisk.Monitoring.Tests;

[TestClass]
public sealed class MonitoringStoreTests {

    static readonly TimeSpan NeverFlush = TimeSpan.FromHours ( 1 );

    static int GetFreePort () {
        using var listener = new TcpListener ( IPAddress.Loopback, 0 );
        listener.Start ();
        return ((IPEndPoint) listener.LocalEndpoint).Port;
    }

    static HttpServerHostContext BuildServer ( int port, Action<ApplicationMonitor> configure ) {
        return HttpServer.CreateBuilder ()
            .UseListeningPort ( $"http://127.0.0.1:{port}/" )
            .UseConfiguration ( configuration => {
                configuration.AccessLogsStream = null;
                configuration.ErrorsLogsStream = null;
            } )
            .UseMonitoring ( "/monitoring", monitor => {
                monitor.CredentialValidator = _ => ValueTask.FromResult ( true );
                monitor.StoreFlushInterval = NeverFlush;
                configure ( monitor );
            } )
            .Build ();
    }

    static Task<HttpResponseMessage> GetAuthorizedAsync ( HttpClient client, string path, HttpCompletionOption completion = HttpCompletionOption.ResponseContentRead ) {
        var request = new HttpRequestMessage ( HttpMethod.Get, path );
        request.Headers.Authorization = new AuthenticationHeaderValue ( "Basic", Convert.ToBase64String ( Encoding.UTF8.GetBytes ( "test:test" ) ) );
        return client.SendAsync ( request, completion );
    }

    static string CreateTemporaryDirectory () {
        string directory = Path.Combine ( Path.GetTempPath (), "sisk-monitoring-tests", Guid.NewGuid ().ToString ( "N" ) );
        Directory.CreateDirectory ( directory );
        return directory;
    }

    [TestMethod]
    public async Task FileStore_RoundTripsAndReplacesWithoutLeavingTemporaryFiles () {
        string directory = CreateTemporaryDirectory ();
        try {
            var store = new FileMonitoringStore ( Path.Combine ( directory, "state.json" ) );
            DateTime now = DateTime.Now;

            await store.SaveAsync ( new MonitoringSnapshot ( now, [ new CounterSnapshot ( "a", TimeSpan.FromHours ( 1 ), [ new CounterIncrementSnapshot ( 1, now.AddHours ( 1 ) ) ] ) ], [], [] ) );
            await store.SaveAsync ( new MonitoringSnapshot ( now, [ new CounterSnapshot ( "b", TimeSpan.FromHours ( 2 ), [ new CounterIncrementSnapshot ( 7, now.AddHours ( 1 ) ) ] ) ], [], [] ) );

            MonitoringSnapshot? loaded = await store.LoadAsync ();

            Assert.IsNotNull ( loaded );
            Assert.AreEqual ( "b", loaded.Counters.Single ().Key );
            Assert.AreEqual ( 7, loaded.Counters.Single ().Increments.Single ().Value );
            CollectionAssert.AreEqual ( new [] { "state.json" }, Directory.GetFiles ( directory ).Select ( Path.GetFileName ).ToArray () );
        }
        finally {
            Directory.Delete ( directory, recursive: true );
        }
    }

    [TestMethod]
    public async Task FileStore_RemovesTemporaryFileWhenSaveFails () {
        string directory = CreateTemporaryDirectory ();
        try {
            var store = new FileMonitoringStore ( Path.Combine ( directory, "state.json" ) );
            using var cancellation = new CancellationTokenSource ();
            cancellation.Cancel ();

            await Assert.ThrowsExceptionAsync<TaskCanceledException> (
                () => store.SaveAsync ( new MonitoringSnapshot ( DateTime.Now, [], [], [] ), cancellation.Token ) );

            Assert.AreEqual ( 0, Directory.GetFiles ( directory ).Length );
        }
        finally {
            Directory.Delete ( directory, recursive: true );
        }
    }

    [TestMethod]
    public void Restore_ToleratesDuplicateKeysAndAssignsSnapshotsInOrder () {
        DateTime now = DateTime.Now;
        var store = new MemoryMonitoringStore ( new MonitoringSnapshot ( now, [
            new CounterSnapshot ( "Requests", TimeSpan.FromHours ( 1 ), [ new CounterIncrementSnapshot ( 3, now.AddHours ( 1 ) ) ] ),
            new CounterSnapshot ( "requests", TimeSpan.FromHours ( 1 ), [ new CounterIncrementSnapshot ( 5, now.AddHours ( 1 ) ) ] )
        ], [], [] ) );

        var first = new Counter ();
        var second = new Counter ();
        int port = GetFreePort ();

        using var server = BuildServer ( port, monitor => {
            monitor.Store = store;
            monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", "api", first ) );
            monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", "web", second ) );
        } );

        server.Start ( verbose: false, preventHault: false );

        Assert.AreEqual ( 3, first.Current );
        Assert.AreEqual ( 5, second.Current );

        server.HttpServer.Stop ();
    }

    [TestMethod]
    public void Restore_FailureDoesNotPreventStartupNorOverwriteStoredState () {
        var counter = new Counter ();
        counter.Increment ( 2 );
        var store = new MemoryMonitoringStore ( null ) { LoadException = new UnauthorizedAccessException () };
        int port = GetFreePort ();

        using var server = BuildServer ( port, monitor => {
            monitor.Store = store;
            monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", counter ) );
        } );

        server.Start ( verbose: false, preventHault: false );
        server.HttpServer.Stop ();

        Assert.AreEqual ( 2, counter.Current );
        Assert.AreEqual ( 0, store.SaveCount );
    }

    [TestMethod]
    public async Task FileStore_LoadReportsUnreadableFileInsteadOfEmptyState () {
        string directory = CreateTemporaryDirectory ();
        try {
            string path = Path.Combine ( directory, "state.json" );
            var store = new FileMonitoringStore ( path );

            Assert.IsNull ( await store.LoadAsync () );

            await store.SaveAsync ( new MonitoringSnapshot ( DateTime.Now, [], [], [] ) );
            using (new FileStream ( path, FileMode.Open, FileAccess.ReadWrite, FileShare.None )) {
                await Assert.ThrowsExceptionAsync<IOException> ( () => store.LoadAsync () );
            }
        }
        finally {
            Directory.Delete ( directory, recursive: true );
        }
    }

    [TestMethod]
    public void ServerStop_FlushesFinalStateAndRestartRestoresIt () {
        var store = new MemoryMonitoringStore ( null );
        var counter = new Counter ();
        var meter = new Meter ();
        int port = GetFreePort ();

        using (var server = BuildServer ( port, monitor => {
            monitor.Store = store;
            monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", counter ) );
            monitor.CaptureMeter ( new MonitoringDefinition<Meter> ( "Bytes", meter ) );
        } )) {
            server.Start ( verbose: false, preventHault: false );
            counter.Increment ( 4 );
            meter.Increment ( 9 );
            server.HttpServer.Stop ();
        }

        Assert.AreEqual ( 1, store.SaveCount );

        var restoredCounter = new Counter ();
        var restoredMeter = new Meter ();
        int restartPort = GetFreePort ();

        using var restarted = BuildServer ( restartPort, monitor => {
            monitor.Store = store;
            monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", restoredCounter ) );
            monitor.CaptureMeter ( new MonitoringDefinition<Meter> ( "Bytes", restoredMeter ) );
        } );

        restarted.Start ( verbose: false, preventHault: false );

        Assert.AreEqual ( 4, restoredCounter.Current );
        Assert.AreEqual ( 9, restoredMeter.Read ().Sum ( reading => reading.Value ) );

        restarted.HttpServer.Stop ();
    }

    [TestMethod]
    public void FlushStore_SkipsUnchangedState () {
        var store = new MemoryMonitoringStore ( null );
        var counter = new Counter ();
        ApplicationMonitor? capturedMonitor = null;
        int port = GetFreePort ();

        using var server = BuildServer ( port, monitor => {
            capturedMonitor = monitor;
            monitor.Store = store;
            monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", counter ) );
        } );

        server.Start ( verbose: false, preventHault: false );

        Assert.IsTrue ( capturedMonitor!.FlushStore () );
        Assert.IsTrue ( capturedMonitor.FlushStore () );
        Assert.AreEqual ( 1, store.SaveCount );

        counter.Increment ();
        Assert.IsTrue ( capturedMonitor.FlushStore () );
        Assert.AreEqual ( 2, store.SaveCount );

        server.HttpServer.Stop ();
        Assert.AreEqual ( 2, store.SaveCount );
    }

    [TestMethod]
    public void FlushStore_StandaloneRestoresBeforeSaving () {
        DateTime now = DateTime.Now;
        var store = new MemoryMonitoringStore ( new MonitoringSnapshot ( now, [
            new CounterSnapshot ( "Requests", TimeSpan.FromHours ( 1 ), [ new CounterIncrementSnapshot ( 6, now.AddHours ( 1 ) ) ] )
        ], [], [] ) );
        var counter = new Counter ();

        using var monitor = new ApplicationMonitor { Store = store };
        monitor.CaptureCounter ( new MonitoringDefinition<Counter> ( "Requests", counter ) );
        counter.Increment ();

        Assert.IsTrue ( monitor.FlushStore () );
        Assert.AreEqual ( 6, counter.Current );
        Assert.AreEqual ( 6, store.Saved?.Counters.Single ().Increments.Sum ( increment => increment.Value ) );
    }

    [TestMethod]
    public void FlushStore_RoundTripsMeterSeries () {
        var store = new MemoryMonitoringStore ( null );
        var meter = new Meter { Aggregation = MeterAggregation.Average };
        meter.Increment ( "p50", 10 );
        meter.Increment ( "p50", 30 );
        meter.Increment ( "p99", 80 );

        using (var monitor = new ApplicationMonitor { Store = store }) {
            monitor.CaptureMeter ( new MonitoringDefinition<Meter> ( "Latency", meter ) );
            Assert.IsTrue ( monitor.FlushStore () );
        }

        var restoredMeter = new Meter { Aggregation = MeterAggregation.Average };
        using var restoredMonitor = new ApplicationMonitor { Store = store };
        restoredMonitor.CaptureMeter ( new MonitoringDefinition<Meter> ( "Latency", restoredMeter ) );
        Assert.IsTrue ( restoredMonitor.FlushStore () );

        CollectionAssert.AreEqual ( new [] { "p50", "p99" }, restoredMeter.Series.ToArray () );
        Assert.AreEqual ( 20, restoredMeter.Read ( "p50", TimeSpan.FromHours ( 1 ), TimeSpan.FromHours ( 1 ) ).Single ().Value );
        Assert.AreEqual ( 80, restoredMeter.Read ( "p99", TimeSpan.FromHours ( 1 ), TimeSpan.FromHours ( 1 ) ).Single ().Value );
    }

    [TestMethod]
    public void FlushStore_RestoresMeterSnapshotSavedBeforeSeriesExisted () {
        string directory = CreateTemporaryDirectory ();
        try {
            string path = Path.Combine ( directory, "state.json" );
            long legacyBucketKey = DateTime.Now.Ticks / TimeSpan.TicksPerSecond;
            File.WriteAllText ( path, $$"""{"SavedAt":"2026-01-01T00:00:00","Counters":[],"Meters":[{"Key":"Bytes","Buckets":[{"BucketKey":{{legacyBucketKey}},"Value":9}]}],"Health":[]}""" );

            var meter = new Meter ();
            using var monitor = new ApplicationMonitor { Store = new FileMonitoringStore ( path ) };
            monitor.CaptureMeter ( new MonitoringDefinition<Meter> ( "Bytes", meter ) );
            Assert.IsTrue ( monitor.FlushStore () );

            Assert.AreEqual ( 9, meter.Read ().Sum ( reading => reading.Value ) );
            CollectionAssert.AreEqual ( new [] { string.Empty }, meter.Series.ToArray () );
        }
        finally {
            Directory.Delete ( directory, recursive: true );
        }
    }

    [TestMethod]
    public void Counter_GroupsIncrementsWithoutLosingValue () {
        var counter = new Counter ();

        for (int i = 0; i < 10_000; i++)
            counter.Increment ( 0.5 );

        Assert.AreEqual ( 5_000, counter.Current );
    }

    [DataTestMethod]
    [DataRow ( 3600, 60, 60 )]
    [DataRow ( 2, 3600, 1 )]
    [DataRow ( 7, 2, 4 )]
    public void Meter_ReadKeepsEveryBucketExactlyOnce ( int windowSeconds, int tickSeconds, int expectedReadings ) {
        var meter = new Meter ( TimeSpan.FromSeconds ( windowSeconds ) );
        meter.Increment ( 3 );
        meter.Increment ( 4 );

        MeterReading [] readings = meter.Read ( TimeSpan.FromSeconds ( tickSeconds ) ).ToArray ();

        Assert.AreEqual ( expectedReadings, readings.Length );
        Assert.AreEqual ( 7, readings.Sum ( reading => reading.Value ) );
        Assert.AreEqual ( 7, readings [ ^1 ].Value, "Recent increments belong to the newest reading." );
    }

    [TestMethod]
    public async Task LogStreamData_ReadsTailOfLargeFileWhileItIsWritable () {
        string directory = CreateTemporaryDirectory ();
        string logFile = Path.Combine ( directory, "app.log" );
        string padding = new ( '-', 160 );
        File.WriteAllLines ( logFile, Enumerable.Range ( 1, 200_000 ).Select ( line => $"2026-01-01 {padding} entry number {line:D6}" ) );

        try {
            using var logStream = new LogStream ( logFile );
            int port = GetFreePort ();

            using var server = BuildServer ( port, monitor =>
                monitor.CaptureLogStream ( new MonitoringDefinition<LogStream> ( "AppLog", logStream ), bufferLineCount: 3 ) );

            server.Start ( verbose: false, preventHault: false );

            using var client = new HttpClient {
                BaseAddress = new Uri ( $"http://127.0.0.1:{port}/" ),
                Timeout = TimeSpan.FromSeconds ( 3 )
            };

            using (HttpResponseMessage response = await GetAuthorizedAsync ( client, "/monitoring/logstream/AppLog/data" )) {
                using JsonDocument payload = JsonDocument.Parse ( await response.Content.ReadAsStringAsync () );
                string [] lines = payload.RootElement.GetProperty ( "lines" ).EnumerateArray ().Select ( line => line.GetString ()! ).ToArray ();

                CollectionAssert.AreEqual (
                    new [] { 199_998, 199_999, 200_000 }.Select ( line => $"2026-01-01 {padding} entry number {line:D6}" ).ToArray (),
                    lines );
            }

            using (HttpResponseMessage download = await GetAuthorizedAsync ( client, "/monitoring/logstream/AppLog/download", HttpCompletionOption.ResponseHeadersRead )) {
                using Stream body = await download.Content.ReadAsStreamAsync ();
                byte [] head = new byte [ 16 ];
                await body.ReadExactlyAsync ( head );

                File.AppendAllText ( logFile, "appended while downloading" + Environment.NewLine );
                await body.CopyToAsync ( Stream.Null );

                Assert.AreEqual ( HttpStatusCode.OK, download.StatusCode );
                Assert.AreEqual ( "attachment", download.Content.Headers.ContentDisposition?.DispositionType );
                Assert.AreEqual ( "utf-8", download.Content.Headers.ContentType?.CharSet );
            }

            server.HttpServer.Stop ();
        }
        finally {
            Directory.Delete ( directory, recursive: true );
        }
    }

    [TestMethod]
    public void ErrorConsole_GroupsIncidentsArchivesAndReopensIssuesAcrossRestarts () {
        static Exception Catch ( Action action ) {
            try {
                action ();
            }
            catch (Exception ex) {
                return ex;
            }
            throw new InvalidOperationException ( "The action must throw." );
        }

        Action invalidOperation = () => throw new InvalidOperationException ( "boom" );
        Action invalidArgument = () => throw new ArgumentException ( "bad" );

        var store = new MemoryMonitoringStore ( null );
        var console = new SectionErrorConsole ();

        using (var monitor = new ApplicationMonitor { Store = store, ErrorConsole = console }) {
            for (int i = 0; i < 3; i++)
                Assert.IsTrue ( console.TryInformException ( Catch ( invalidOperation ) ) );
            Assert.IsTrue ( console.TryInformException ( Catch ( invalidArgument ) ) );
            Assert.IsFalse ( console.TryInformException ( null! ) );
            Assert.IsTrue ( monitor.FlushStore () );

            MonitoringIssueSnapshot [] issues = store.Saved!.Issues!;
            MonitoringIssueSnapshot issue = issues.Single ( item => item.Type == "System.InvalidOperationException" );

            Assert.AreEqual ( 2, issues.Length, "Incidents are grouped by exception type and location." );
            Assert.AreEqual ( 3, issue.Count );
            Assert.AreEqual ( 3, issue.Trend.Sum ( reading => reading.Count ) );
            Assert.IsFalse ( issue.Resolved );
            Assert.AreEqual ( "boom", issue.Occurrences [ 0 ].Message );
            Assert.AreEqual ( "Tenant", issue.Occurrences [ 0 ].Sections.Single ().Title );
            StringAssert.Contains ( issue.Occurrences [ 0 ].Sections.Single ().Html, "acme &amp; co" );

            console.ArchiveIssuesAfter = TimeSpan.Zero;
            Assert.IsTrue ( console.TryInformException ( Catch ( invalidArgument ) ) );
            Assert.IsTrue ( monitor.FlushStore () );
            Assert.IsTrue ( store.Saved!.Issues!.All ( item => item.Resolved ), "Issues without new incidents are archived." );
        }

        var restoredConsole = new SectionErrorConsole ();
        using var restoredMonitor = new ApplicationMonitor { Store = store, ErrorConsole = restoredConsole };
        Assert.IsTrue ( restoredMonitor.FlushStore () );

        Assert.IsTrue ( restoredConsole.TryInformException ( Catch ( invalidOperation ) ) );
        Assert.IsTrue ( restoredMonitor.FlushStore () );

        MonitoringIssueSnapshot reopened = store.Saved!.Issues!.Single ( item => item.Type == "System.InvalidOperationException" );
        Assert.AreEqual ( 4, reopened.Count );
        Assert.IsFalse ( reopened.Resolved, "A new incident reopens a resolved issue." );
    }

    [TestMethod]
    public async Task ErrorsPage_ListsCapturedExceptionsAndResolvesThem () {
        int port = GetFreePort ();

        using var server = HttpServer.CreateBuilder ()
            .UseListeningPort ( $"http://127.0.0.1:{port}/" )
            .UseConfiguration ( configuration => {
                configuration.AccessLogsStream = null;
                configuration.ErrorsLogsStream = null;
            } )
            .UseMonitoring ( "/monitoring", monitor => {
                monitor.CredentialValidator = _ => ValueTask.FromResult ( true );
                monitor.ErrorConsole = new SectionErrorConsole ();
            } )
            .UseRouter ( router => router.MapGet ( "/throws", HttpResponse ( HttpRequest _ ) => throw new InvalidOperationException ( "boom" ) ) )
            .Build ();

        server.Start ( verbose: false, preventHault: false );

        using var client = new HttpClient ( new HttpClientHandler { AllowAutoRedirect = false } ) {
            BaseAddress = new Uri ( $"http://127.0.0.1:{port}/" ),
            Timeout = TimeSpan.FromSeconds ( 3 )
        };

        using (await client.GetAsync ( "/throws" )) {
        }

        string page = string.Empty;
        for (int attempt = 0; attempt < 40 && !page.Contains ( "System.InvalidOperationException" ); attempt++) {
            await Task.Delay ( 50 );
            using HttpResponseMessage pageResponse = await GetAuthorizedAsync ( client, "/monitoring/errors" );
            page = await pageResponse.Content.ReadAsStringAsync ();
        }

        StringAssert.Contains ( page, "System.InvalidOperationException" );
        StringAssert.Contains ( page, "href=\"/monitoring/errors\"", "The sidebar links to the errors page." );
        string issuePath = System.Text.RegularExpressions.Regex.Match ( page, "href=\"(/monitoring/errors/[0-9a-f]{16})\"" ).Groups [ 1 ].Value;

        using (HttpResponseMessage issueResponse = await GetAuthorizedAsync ( client, issuePath )) {
            string issuePage = await issueResponse.Content.ReadAsStringAsync ();

            Assert.AreEqual ( HttpStatusCode.OK, issueResponse.StatusCode );
            StringAssert.Contains ( issuePage, "boom" );
            StringAssert.Contains ( issuePage, "GET /throws" );
            StringAssert.Contains ( issuePage, "acme &amp; co" );
            StringAssert.Contains ( issuePage, $"action=\"{issuePath}/resolve\"" );
        }

        var resolveRequest = new HttpRequestMessage ( HttpMethod.Post, $"{issuePath}/resolve" );
        resolveRequest.Headers.Authorization = new AuthenticationHeaderValue ( "Basic", Convert.ToBase64String ( Encoding.UTF8.GetBytes ( "test:test" ) ) );
        using (HttpResponseMessage resolveResponse = await client.SendAsync ( resolveRequest )) {
            Assert.AreEqual ( HttpStatusCode.SeeOther, resolveResponse.StatusCode );
            Assert.AreEqual ( "/monitoring/errors", resolveResponse.Headers.Location?.OriginalString );
        }

        using (HttpResponseMessage unresolvedResponse = await GetAuthorizedAsync ( client, "/monitoring/errors" ))
            StringAssert.Contains ( await unresolvedResponse.Content.ReadAsStringAsync (), "No unresolved issues." );

        using (HttpResponseMessage resolvedResponse = await GetAuthorizedAsync ( client, "/monitoring/errors?status=resolved" ))
            StringAssert.Contains ( await resolvedResponse.Content.ReadAsStringAsync (), "System.InvalidOperationException" );

        using (HttpResponseMessage missingResponse = await GetAuthorizedAsync ( client, "/monitoring/errors/0000000000000000" ))
            Assert.AreEqual ( HttpStatusCode.NotFound, missingResponse.StatusCode );

        server.HttpServer.Stop ();
    }

    sealed class SectionErrorConsole : MonitoringErrorConsole {

        protected override void OnException ( Exception exception, HttpContext? context ) {
            WriteException ( new MonitoringIssueContext ( exception, context,
                MonitoringErrorSection.Create ( "Tenant", [ new KeyValuePair<string, string?> ( "Name", "acme & co" ) ] ) ) );
        }
    }

    sealed class MemoryMonitoringStore ( MonitoringSnapshot? snapshot ) : IMonitoringStore {

        public MonitoringSnapshot? Saved { get; private set; } = snapshot;

        public int SaveCount { get; private set; }

        public Exception? LoadException { get; init; }

        public Task SaveAsync ( MonitoringSnapshot snapshot, CancellationToken cancellationToken = default ) {
            Saved = snapshot;
            SaveCount++;
            return Task.CompletedTask;
        }

        public Task<MonitoringSnapshot?> LoadAsync ( CancellationToken cancellationToken = default ) {
            if (LoadException is not null)
                throw LoadException;

            return Task.FromResult ( Saved );
        }
    }
}
