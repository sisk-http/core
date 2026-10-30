using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text;
using System.Text.Json;
using Sisk.Core.Helpers;
using Sisk.Core.Http;
using Sisk.Core.Routing;
using TinyComponents;

namespace Sisk.Monitoring;

/// <summary>
/// Provides an embedded web dashboard for monitoring application counters, log streams and server health.
/// </summary>
public class ApplicationMonitor : IDisposable, IAsyncDisposable {

    const string InFlightBagKey = "Sisk.Monitoring.InFlight";

    string? currentRoutePrefix = null;
    IRequestHandler []? routeHandlers;
    long httpInFlight;
    readonly HttpMetrics httpMetrics = new ();

    // arrow-left-s-line from Remix Icon
    const string ArrowLeftIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M10.8284 12.0007L15.7782 16.9504L14.364 18.3646L8 12.0007L14.364 5.63672L15.7782 7.05093L10.8284 12.0007Z"></path></svg>
        """;

    // arrow-right-s-line from Remix Icon
    const string ArrowRightIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M13.1717 12.0007L8.22192 7.05093L9.63614 5.63672L16.0001 12.0007L9.63614 18.3646L8.22192 16.9504L13.1717 12.0007Z"></path></svg>
        """;

    // dashboard-3-line from Remix Icon
    const string DashboardIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M3 13H11V3H3V13ZM3 21H11V15H3V21ZM13 21H21V11H13V21ZM13 3V9H21V3H13Z"></path></svg>
        """;

    // terminal-line from Remix Icon
    const string LogStreamIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M11.9998 2C17.5226 2 21.9998 6.47715 21.9998 12C21.9998 17.5228 17.5226 22 11.9998 22C6.47691 22 1.99976 17.5228 1.99976 12C1.99976 6.47715 6.47691 2 11.9998 2ZM7.99976 8L5.99976 12L7.99976 16H9.99976L7.99976 12L9.99976 8H7.99976ZM13.9998 8L11.9998 12L13.9998 16H15.9998L13.9998 12L15.9998 8H13.9998Z"></path></svg>
        """;

    // bar-chart-2-line from Remix Icon
    const string CounterIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M2 13H8V21H2V13ZM16 8H22V21H16V8ZM9 3H15V21H9V3ZM4 15V19H6V15H4ZM11 5V19H13V5H11ZM18 10V19H20V10H18Z"></path></svg>
        """;

    // heart-pulse-line from Remix Icon
    const string HealthIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M16.5 3C19.5376 3 22 5.5 22 9C22 16 14.5 20 12 21.5C9.5 20 2 16 2 9C2 5.5 4.5 3 7.5 3C9.36 3 11 4 12 5C13 4 14.64 3 16.5 3ZM12.9339 10.5H17V8.5H14.0654L12.9339 10.5ZM7 8.5V10.5H9.9346L11.0661 8.5H7ZM11.5 12L10 15H7V17H9.9346L12 13L14.0654 17H17V15H14L12.5 12H11.5Z"></path></svg>
        """;

    // sun-line from Remix Icon
    const string LightThemeIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M12 18C8.68629 18 6 15.3137 6 12C6 8.68629 8.68629 6 12 6C15.3137 6 18 8.68629 18 12C18 15.3137 15.3137 18 12 18ZM12 16C14.2091 16 16 14.2091 16 12C16 9.79086 14.2091 8 12 8C9.79086 8 8 9.79086 8 12C8 14.2091 9.79086 16 12 16ZM11 1H13V4H11V1ZM11 20H13V23H11V20ZM3.51472 4.92893L4.92893 3.51472L7.05025 5.63604L5.63604 7.05025L3.51472 4.92893ZM16.9497 18.364L18.364 16.9497L20.4853 19.0711L19.0711 20.4853L16.9497 18.364ZM19.0711 3.51472L20.4853 4.92893L18.364 7.05025L16.9497 5.63604L19.0711 3.51472ZM5.63604 16.9497L7.05025 18.364L4.92893 20.4853L3.51472 19.0711L5.63604 16.9497ZM23 11V13H20V11H23ZM4 11V13H1V11H4Z"></path></svg>
        """;

    // moon-line from Remix Icon
    const string DarkThemeIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M10 7C10 10.866 13.134 14 17 14C18.9584 14 20.729 13.1957 21.9995 11.8995C22 11.933 22 11.9665 22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2C12.0335 2 12.067 2 12.1005 2.00049C10.8043 3.27098 10 5.04157 10 7ZM4 12C4 16.4183 7.58172 20 12 20C15.0583 20 17.7158 18.2839 19.062 15.7621C18.3945 15.9187 17.7035 16 17 16C12.0294 16 8 11.9706 8 7C8 6.29648 8.08133 5.60547 8.2379 4.938C5.71611 6.28423 4 8.9417 4 12Z"></path></svg>
        """;

    // computer-line from Remix Icon
    const string SystemThemeIcon = """
        <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M4 16H20V5H4V16ZM13 18V20H17V22H7V20H11V18H2.9918C2.44405 18 2 17.5511 2 16.9925V4.00748C2 3.45107 2.45531 3 2.9918 3H21.0082C21.556 3 22 3.44892 22 4.00748V16.9925C22 17.5489 21.5447 18 21.0082 18H13Z"></path></svg>
        """;

    List<MonitoringDefinition<LogStream>> capturingLogStreams = new ();
    List<MonitoringDefinition<Counter>> counters = new ();
    List<MonitoringDefinition<Meter>> meters = new ();
    readonly Dictionary<string, int> logStreamBufferLineCounts = new ( StringComparer.OrdinalIgnoreCase );
    readonly List<HealthSnapshot> healthSnapshots = new ();
    readonly object healthSnapshotsSync = new ();
    CancellationTokenSource? storeFlushCancellation;
    Task? storeFlushTask;
    CancellationTokenSource? healthSamplingCancellation;
    Task? healthSamplingTask;
    int storeFlushRunning;
    bool storeStateLoaded;
    bool disposed;

    /// <summary>
    /// Gets or sets the title displayed in the dashboard header and browser tab.
    /// </summary>
    public string PageTitle { get; set; } = "Monitoring";

    /// <summary>
    /// Gets or sets a function that validates user credentials for accessing the monitoring dashboard.
    /// </summary>
    public Func<NetworkCredential, ValueTask<bool>>? CredentialValidator { get; set; } = null;

    /// <summary>
    /// Gets or sets the optional store used to persist and restore monitoring state.
    /// </summary>
    public IMonitoringStore? Store { get; set; }

    /// <summary>
    /// Gets or sets the interval used to flush monitoring state to <see cref="Store"/>.
    /// </summary>
    public TimeSpan StoreFlushInterval { get; set; } = TimeSpan.FromSeconds ( 10 );

    /// <summary>
    /// Registers a counter for monitoring within the dashboard.
    /// </summary>
    /// <param name="counter">The counter definition to capture.</param>
    public void CaptureCounter ( MonitoringDefinition<Counter> counter ) {
        counters.Add ( counter );
    }

    /// <summary>
    /// Registers a meter for monitoring within the dashboard.
    /// </summary>
    /// <param name="meter">The meter definition to capture.</param>
    public void CaptureMeter ( MonitoringDefinition<Meter> meter ) {
        meters.Add ( meter );
    }

    /// <summary>
    /// Registers a log stream for monitoring and starts buffering its output.
    /// </summary>
    /// <param name="logStream">The log stream definition to capture.</param>
    /// <param name="bufferLineCount">The maximum number of lines to buffer; defaults to 500.</param>
    public void CaptureLogStream ( MonitoringDefinition<LogStream> logStream, int bufferLineCount = 500 ) {
        capturingLogStreams.Add ( logStream );
        logStreamBufferLineCounts [ logStream.StorageKey ] = bufferLineCount;
        if (!logStream.Instance.IsBuffering)
            logStream.Instance.StartBuffering ( bufferLineCount );
    }

    /// <summary>
    /// Flushes the current monitoring state to <see cref="Store"/>.
    /// </summary>
    /// <returns><see langword="true"/> when a store exists and the state was saved; otherwise, <see langword="false"/>.</returns>
    public bool FlushStore () {
        return FlushStoreAsync ().GetAwaiter ().GetResult ();
    }

    /// <summary>
    /// Flushes the current monitoring state to <see cref="Store"/> asynchronously.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns><see langword="true"/> when a store exists and the state was saved; otherwise, <see langword="false"/>.</returns>
    public async Task<bool> FlushStoreAsync ( CancellationToken cancellationToken = default ) {
        if (Store is null || disposed)
            return false;

        if (Interlocked.Exchange ( ref storeFlushRunning, 1 ) == 1)
            return false;

        try {
            await Store.SaveAsync ( CreateMonitoringSnapshot (), cancellationToken ).ConfigureAwait ( false );
            return true;
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
            return false;
        }
        catch {
            return false;
        }
        finally {
            Volatile.Write ( ref storeFlushRunning, 0 );
        }
    }

    void RestoreStoreState () {
        if (storeStateLoaded || Store is null)
            return;

        storeStateLoaded = true;
        MonitoringSnapshot? snapshot = Store.LoadAsync ().ConfigureAwait ( false ).GetAwaiter ().GetResult ();
        if (snapshot is null)
            return;

        var counterSnapshots = (snapshot.Counters ?? []).ToDictionary ( item => item.Key, StringComparer.OrdinalIgnoreCase );
        foreach (var counter in counters) {
            if (counterSnapshots.TryGetValue ( counter.StorageKey, out var counterSnapshot )) {
                counter.Instance.DefaultDuration = counterSnapshot.DefaultDuration;
                counter.Instance.ImportState ( counterSnapshot.Increments );
            }
        }

        var meterSnapshots = (snapshot.Meters ?? []).ToDictionary ( item => item.Key, StringComparer.OrdinalIgnoreCase );
        foreach (var meter in meters) {
            if (meterSnapshots.TryGetValue ( meter.StorageKey, out var meterSnapshot )) {
                meter.Instance.ImportState ( meterSnapshot.Buckets );
            }
        }

        DateTime threshold = DateTime.Now - TimeSpan.FromDays ( 30 );
        lock (healthSnapshotsSync) {
            healthSnapshots.Clear ();
            healthSnapshots.AddRange ( (snapshot.Health ?? [])
                .Where ( item => item.Timestamp >= threshold )
                .Select ( item => new HealthSnapshot ( item.Timestamp, item.CpuPercent, item.DiskPercent, item.MemoryPercent, 0, 0, 0, string.Empty, DateTime.MinValue ) ) );
        }
    }

    void StartStoreFlushLoop () {
        if (Store is null || storeFlushTask is not null)
            return;

        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( StoreFlushInterval.TotalMilliseconds, nameof ( StoreFlushInterval ) );

        storeFlushCancellation = new CancellationTokenSource ();
        storeFlushTask = RunStoreFlushLoopAsync ( storeFlushCancellation.Token );
    }

    async Task RunStoreFlushLoopAsync ( CancellationToken cancellationToken ) {
        try {
            using var timer = new PeriodicTimer ( StoreFlushInterval );

            while (await timer.WaitForNextTickAsync ( cancellationToken ).ConfigureAwait ( false )) {
                await FlushStoreAsync ( cancellationToken ).ConfigureAwait ( false );
            }
        }
        catch (OperationCanceledException) when (cancellationToken.IsCancellationRequested) {
        }
    }

    MonitoringSnapshot CreateMonitoringSnapshot () {
        var countersSnapshot = counters
            .Select ( counter => new CounterSnapshot (
                counter.StorageKey,
                counter.Instance.DefaultDuration,
                counter.Instance.ExportState () ) )
            .ToArray ();

        var metersSnapshot = meters
            .Select ( meter => new MeterSnapshot (
                meter.StorageKey,
                meter.Instance.ExportState () ) )
            .ToArray ();

        MonitoringHealthSnapshot [] healthSnapshot;
        lock (healthSnapshotsSync) {
            healthSnapshot = healthSnapshots
                .Select ( item => new MonitoringHealthSnapshot ( item.Timestamp, item.CpuPercent, item.DiskPercent, item.MemoryPercent ) )
                .ToArray ();
        }

        return new MonitoringSnapshot ( DateTime.Now, countersSnapshot, metersSnapshot, healthSnapshot );
    }

    /// <summary>
    /// Authenticates a user account against the configured credential validator.
    /// </summary>
    /// <param name="userEmail">The user's email address.</param>
    /// <param name="userPassword">The user's password.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> that yields <see langword="true"/> when authentication succeeds; otherwise, <see langword="false"/>.</returns>
    protected virtual ValueTask<bool> AuthenticateAccountAsync ( string userEmail, string userPassword ) {
        if (CredentialValidator is not null) {
            return CredentialValidator ( new NetworkCredential ( userEmail, userPassword ) );
        }
        return new ValueTask<bool> ( false );
    }

    string PrefixPath ( string relativePath ) {
        return PathHelper.CombinePaths ( currentRoutePrefix ?? "/", relativePath );
    }

    string BuildPageHtml ( HtmlElement bodyContent, string? activeNavItem = null ) {
        var html = new HtmlElement ( "html" );
        html.Attributes [ "lang" ] = "en";

        html += new HtmlElement ( "head", head => {
            head += new HtmlElement ( "meta" )
                .WithAttribute ( "charset", "UTF-8" )
                .SelfClosed ();
            head += new HtmlElement ( "meta" )
                .WithAttribute ( "name", "viewport" )
                .WithAttribute ( "content", "width=device-width, initial-scale=1.0" )
                .SelfClosed ();
            head += new HtmlElement ( "meta" )
                .WithAttribute ( "name", "color-scheme" )
                .WithAttribute ( "content", "light dark" )
                .SelfClosed ();
            head += new HtmlElement ( "title", PageTitle );
            head += new HtmlElement ( "script", RenderableText.Raw ( Assets.ThemeBootstrapScript ) );
            head += new HtmlElement ( "style", RenderableText.Raw ( Assets.DefaultStyles ) );
        } );

        html += new HtmlElement ( "body", body => {
            body += new HtmlElement ( "div", wrapper => {
                wrapper.ClassList.Add ( "page-wrapper" );

                body += new HtmlElement ( "div", overlay => {
                    overlay.ClassList.Add ( "sidebar-overlay" );
                } );

                var sidebar = WriteSidebar ( activeNavItem );
                sidebar += new HtmlElement ( "div", switcher => {
                    switcher.ClassList.Add ( "theme-switcher" );
                    switcher.Attributes [ "role" ] = "group";
                    switcher.Attributes [ "aria-label" ] = "Color theme";

                    foreach ((string value, string label, string icon) in new [] {
                        ("light", "Light", LightThemeIcon),
                        ("dark", "Dark", DarkThemeIcon),
                        ("system", "System", SystemThemeIcon)
                    }) {
                        switcher += new HtmlElement ( "button", button => {
                            button.ClassList.Add ( "theme-option" );
                            button.Attributes [ "type" ] = "button";
                            button.Attributes [ "data-theme-option" ] = value;
                            button.Attributes [ "title" ] = label;
                            button.Attributes [ "aria-label" ] = $"{label} theme";
                            button.Attributes [ "aria-pressed" ] = "false";
                            button += RenderableText.Raw ( icon );
                        } );
                    }
                } );
                wrapper += sidebar;

                body += new HtmlElement ( "button", menuBtn => {
                    menuBtn.ClassList.Add ( "mobile-menu-btn" );
                    menuBtn.Attributes [ "aria-label" ] = "Open menu";
                    menuBtn += "\u2630";
                } );

                wrapper += new HtmlElement ( "main", main => {
                    main += bodyContent;
                } );
            } );

            body += new HtmlElement ( "script", RenderableText.Raw ( Assets.DefaultScript ) );
        } );

        return "<!DOCTYPE html>\n" + html.ToString ();
    }

    /// <summary>
    /// Creates the sidebar navigation HTML element.
    /// </summary>
    /// <param name="activeNavItem">Optional identifier of the currently active navigation item.</param>
    /// <returns>An <see cref="HtmlElement"/> representing the sidebar.</returns>
    protected virtual HtmlElement WriteSidebar ( string? activeNavItem ) {
        return new HtmlElement ( "nav", nav => {
            nav.ClassList.Add ( "sidebar" );

            nav += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "nav-section" );

                section += new HtmlElement ( "div", "Pages" ).WithClass ( "nav-section-title" );

                section += WriteNavItem ( PrefixPath ( "/" ), "Dashboard", DashboardIcon, activeNavItem == "dashboard" );
                section += WriteNavItem ( PrefixPath ( "/health" ), "Server Health", HealthIcon, activeNavItem == "health" );
                if (counters.Count > 0)
                    section += WriteNavItem ( PrefixPath ( "/counters" ), "Counters", CounterIcon, activeNavItem == "counters" );
                if (meters.Count > 0)
                    section += WriteNavItem ( PrefixPath ( "/meters" ), "Meters", CounterIcon, activeNavItem == "meters" );

                //if (counters.Count > 0) {
                //    section += new HtmlElement ( "div", "Counters" ).WithClass ( "nav-section-title" );
                //    foreach (var counter in counters) {
                //        section += WriteNavItem ( null, counter.Label, CounterIcon, false );
                //    }
                //}

                if (capturingLogStreams.Count > 0) {
                    section += new HtmlElement ( "div", "Log Streams" ).WithClass ( "nav-section-title" );
                    foreach (var streamGroup in GroupDefinitionsByGroup ( capturingLogStreams )) {
                        section += new HtmlElement ( "div", streamGroup.Key ).WithClass ( "nav-group-title" );

                        foreach (var logStream in streamGroup) {
                            string href = PrefixPath ( $"/logstream/{logStream.SanitizedLabel}" );
                            bool isActive = activeNavItem == logStream.SanitizedLabel;
                            section += WriteNavItem ( href, logStream.Label, LogStreamIcon, isActive );
                        }
                    }
                }
            } );
        } );
    }

    HtmlElement WriteNavItem ( string? href, string label, string icon, bool active ) {
        return new HtmlElement ( "a", item => {
            item.ClassList.Add ( "nav-item" );
            if (active)
                item.ClassList.Add ( "active" );
            if (href is not null)
                item.Attributes [ "href" ] = href;
            item += new HtmlElement ( "span", RenderableText.Raw ( icon ) ).WithClass ( "nav-icon" );
            item += new HtmlElement ( "span", label );
        } );
    }

    HtmlElement WriteAutoRefreshToolbar () {
        return new HtmlElement ( "div", toolbar => {
            toolbar.ClassList.Add ( "page-toolbar" );

            toolbar += new HtmlElement ( "button", btn => {
                btn.ClassList.Add ( "toolbar-btn" );
                btn.ClassList.Add ( "active" );
                btn.Id = "btn-toggle-refresh";
                btn.Attributes [ "type" ] = "button";
                btn += "Stop refresh";
            } );
        } );
    }

    /// <summary>
    /// Generates the HTML for the dashboard page.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with dashboard HTML.</returns>
    protected virtual ValueTask<HttpResponse> GetDashboardPageHtmlAsync ( HttpRequest request ) {

        var content = new HtmlElement ( "", fragment => {

            fragment += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "content-header" );
                header += new HtmlElement ( "h1", "Dashboard" );
                header += new HtmlElement ( "p", "Overview of monitored resources." ).WithClass ( "description" );
            } );

            fragment += WriteAutoRefreshToolbar ();

            fragment += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "section" );
                section.Attributes [ "data-http-endpoint" ] = PrefixPath ( "/data" );

                section += new HtmlElement ( "div", header => {
                    header.ClassList.Add ( "usage-history-header" );
                    header += new HtmlElement ( "h2", "HTTP Traffic" );
                    header += new HtmlElement ( "div", periods => {
                        periods.ClassList.Add ( "usage-history-periods" );
                        periods.Attributes [ "role" ] = "group";
                        periods.Attributes [ "aria-label" ] = "HTTP traffic period";

                        foreach (string period in new [] { "1h", "1d", "7d" }) {
                            periods += new HtmlElement ( "button", button => {
                                button.ClassList.Add ( "toolbar-btn" );
                                if (period == "1h")
                                    button.ClassList.Add ( "active" );
                                button.Attributes [ "type" ] = "button";
                                button.Attributes [ "data-http-period" ] = period;
                                button.Attributes [ "aria-pressed" ] = period == "1h" ? "true" : "false";
                                button += period;
                            } );
                        }
                    } );
                } );

                section += new HtmlElement ( "div", grid => {
                    grid.ClassList.Add ( "http-stats-grid" );

                    foreach ((string label, string key) in new [] {
                        ("Requests", "requests"),
                        ("Requests / min", "rpm"),
                        ("Error rate (5xx)", "errorRate"),
                        ("In flight", "inFlight"),
                        ("Latency p50", "p50"),
                        ("Latency p95", "p95"),
                        ("Latency p99", "p99"),
                        ("Latency max", "max")
                    }) {
                        grid += new HtmlElement ( "div", card => {
                            card.ClassList.Add ( "card" );
                            card.ClassList.Add ( "http-stat-card" );
                            card += new HtmlElement ( "div", label ).WithClass ( "card-label" );
                            card += new HtmlElement ( "div", "-" )
                                .WithClass ( "card-value" )
                                .WithAttribute ( "data-http-stat", key );
                        } );
                    }
                } );

                section += new HtmlElement ( "div", charts => {
                    charts.ClassList.Add ( "http-charts" );

                    foreach ((string title, string chartKey) in new [] { ("Responses", "responses"), ("Latency (ms)", "latency") }) {
                        charts += new HtmlElement ( "div", block => {
                            block.ClassList.Add ( "http-chart-block" );
                            block += new HtmlElement ( "h3", title ).WithClass ( "group-title" );
                            block += new HtmlElement ( "div", chart => {
                                chart.ClassList.Add ( "health-chart-container" );
                                chart.Attributes [ "data-http-chart" ] = chartKey;
                                chart.Attributes [ "role" ] = "img";
                                chart.Attributes [ "aria-label" ] = $"HTTP {chartKey} chart";
                            } );
                        } );
                    }
                } );

                section += new HtmlElement ( "div", tables => {
                    tables.ClassList.Add ( "http-tables" );

                    foreach ((string title, string tableKey) in new [] { ("Slowest routes (p95)", "slowest"), ("Routes with most errors", "errors") }) {
                        tables += new HtmlElement ( "div", block => {
                            block.ClassList.Add ( "http-table-block" );
                            block += new HtmlElement ( "h3", title ).WithClass ( "group-title" );
                            block += new HtmlElement ( "div", wrapper => {
                                wrapper.ClassList.Add ( "http-table-wrapper" );
                                wrapper.Attributes [ "data-http-table" ] = tableKey;
                                wrapper += new HtmlElement ( "p", "No requests recorded." ).WithClass ( "http-table-empty" );
                            } );
                        } );
                    }
                } );

                section += new HtmlElement ( "div", block => {
                    block.ClassList.Add ( "http-table-block" );
                    block += new HtmlElement ( "h3", "Recent exceptions" ).WithClass ( "group-title" );
                    block += new HtmlElement ( "div", list => {
                        list.ClassList.Add ( "http-exceptions" );
                        list.Attributes [ "data-http-exceptions" ] = "";
                        list += new HtmlElement ( "p", "No exceptions recorded." ).WithClass ( "http-table-empty" );
                    } );
                } );
            } );

            var pinnedCounters = counters.Where ( c => c.DashboardPinned ).ToArray ();
            if (pinnedCounters.Length > 0) {
                fragment += new HtmlElement ( "div", section => {
                    section.ClassList.Add ( "section" );
                    section += new HtmlElement ( "h2", "Counters" );

                    foreach (var counterGroup in GroupDefinitionsByGroup ( pinnedCounters )) {
                        section += new HtmlElement ( "div", groupBlock => {
                            groupBlock.ClassList.Add ( "group-block" );
                            groupBlock += new HtmlElement ( "h3", counterGroup.Key ).WithClass ( "group-title" );

                            groupBlock += new HtmlElement ( "div", grid => {
                                grid.ClassList.Add ( "cards-grid" );

                                foreach (var counter in counterGroup) {
                                    grid += WriteCounterCard ( counter );
                                }
                            } );
                        } );
                    }
                } );
            }

            var dashboardMeters = meters.Where ( m => m.DashboardPinned ).ToArray ();
            if (dashboardMeters.Length > 0) {
                fragment += new HtmlElement ( "div", section => {
                    section.ClassList.Add ( "section" );
                    section += new HtmlElement ( "h2", "Meters" );

                    foreach (var meterGroup in GroupDefinitionsByGroup ( dashboardMeters )) {
                        section += new HtmlElement ( "div", groupBlock => {
                            groupBlock.ClassList.Add ( "group-block" );
                            groupBlock += new HtmlElement ( "h3", meterGroup.Key ).WithClass ( "group-title" );

                            groupBlock += new HtmlElement ( "div", grid => {
                                grid.ClassList.Add ( "cards-grid" );
                                grid.ClassList.Add ( "meters-grid" );
                                grid.Attributes [ "data-meters-endpoint" ] = PrefixPath ( "/meters/data" );

                                foreach (var meter in meterGroup) {
                                    grid += WriteMeterCard ( meter );
                                }
                            } );
                        } );
                    }
                } );
            }

            var pinnedLogStreams = capturingLogStreams.Where ( s => s.DashboardPinned ).ToArray ();
            if (pinnedLogStreams.Length > 0) {
                fragment += new HtmlElement ( "div", section => {
                    section.ClassList.Add ( "section" );
                    section += new HtmlElement ( "h2", "Log Streams" );

                    foreach (var streamGroup in GroupDefinitionsByGroup ( pinnedLogStreams )) {
                        section += new HtmlElement ( "div", groupBlock => {
                            groupBlock.ClassList.Add ( "group-block" );
                            groupBlock += new HtmlElement ( "h3", streamGroup.Key ).WithClass ( "group-title" );

                            groupBlock += new HtmlElement ( "div", list => {
                                list.ClassList.Add ( "stream-list" );

                                foreach (var logStream in streamGroup) {
                                    list += new HtmlElement ( "a", item => {
                                        item.ClassList.Add ( "stream-item" );
                                        item.Attributes [ "href" ] = PrefixPath ( $"/logstream/{logStream.SanitizedLabel}" );

                                        item += new HtmlElement ( "div", left => {
                                            left += new HtmlElement ( "span", logStream.Label ).WithClass ( "stream-item-label" );
                                        } );
                                        item += new HtmlElement ( "span", RenderableText.Raw ( ArrowRightIcon ) ).WithClass ( "stream-arrow" );
                                    } );
                                }
                            } );
                        } );
                    }
                } );
            }

            if (!pinnedCounters.Any () && !pinnedLogStreams.Any () && !dashboardMeters.Any ()) {
                fragment += new HtmlElement ( "div", empty => {
                    empty.ClassList.Add ( "empty-state" );
                    empty += new HtmlElement ( "p", "No monitored resources configured." );
                } );
            }
        } );

        string html = BuildPageHtml ( content, "dashboard" );
        return new ValueTask<HttpResponse> (
            new HttpResponse ( 200 ) {
                Content = new HtmlContent ( html )
            }
        );
    }

    /// <summary>
    /// Generates the HTML for the dedicated counters page.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with counters HTML.</returns>
    protected virtual ValueTask<HttpResponse> GetCountersPageHtmlAsync ( HttpRequest request ) {
        var content = new HtmlElement ( "", fragment => {

            fragment += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "content-header" );
                header += new HtmlElement ( "h1", "Counters" );
                header += new HtmlElement ( "p", "Current values for captured counters." ).WithClass ( "description" );
            } );

            fragment += WriteAutoRefreshToolbar ();

            if (counters.Count == 0) {
                fragment += new HtmlElement ( "div", empty => {
                    empty.ClassList.Add ( "empty-state" );
                    empty += new HtmlElement ( "p", "No counters configured." );
                } );
                return;
            }

            foreach (var counterGroup in GroupDefinitionsByGroup ( counters )) {
                fragment += new HtmlElement ( "div", section => {
                    section.ClassList.Add ( "section" );
                    section += new HtmlElement ( "h2", counterGroup.Key );

                    section += new HtmlElement ( "div", grid => {
                        grid.ClassList.Add ( "cards-grid" );
                        grid.Attributes [ "data-counters-endpoint" ] = PrefixPath ( "/counters/data" );

                        foreach (var counter in counterGroup) {
                            grid += WriteCounterCard ( counter );
                        }
                    } );
                } );
            }
        } );

        string html = BuildPageHtml ( content, "counters" );
        return new ValueTask<HttpResponse> (
            new HttpResponse ( 200 ) {
                Content = new HtmlContent ( html )
            }
        );
    }

    /// <summary>
    /// Generates the HTML for the dedicated meters page.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with meters HTML.</returns>
    protected virtual ValueTask<HttpResponse> GetMetersPageHtmlAsync ( HttpRequest request ) {
        var content = new HtmlElement ( "", fragment => {

            fragment += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "content-header" );
                header += new HtmlElement ( "h1", "Meters" );
                header += new HtmlElement ( "p", "Timeline of aggregated meter readings." ).WithClass ( "description" );
            } );

            fragment += WriteAutoRefreshToolbar ();

            if (meters.Count == 0) {
                fragment += new HtmlElement ( "div", empty => {
                    empty.ClassList.Add ( "empty-state" );
                    empty += new HtmlElement ( "p", "No meters configured." );
                } );
                return;
            }

            foreach (var meterGroup in GroupDefinitionsByGroup ( meters )) {
                fragment += new HtmlElement ( "div", section => {
                    section.ClassList.Add ( "section" );
                    section += new HtmlElement ( "h2", meterGroup.Key );

                    section += new HtmlElement ( "div", grid => {
                        grid.ClassList.Add ( "cards-grid" );
                        grid.ClassList.Add ( "meters-grid" );
                        grid.Attributes [ "data-meters-endpoint" ] = PrefixPath ( "/meters/data" );

                        foreach (var meter in meterGroup) {
                            grid += WriteMeterCard ( meter );
                        }
                    } );
                } );
            }
        } );

        string html = BuildPageHtml ( content, "meters" );
        return new ValueTask<HttpResponse> (
            new HttpResponse ( 200 ) {
                Content = new HtmlContent ( html )
            }
        );
    }

    /// <summary>
    /// Generates the JSON payload containing all meter points and computed statistics.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding an HTTP response with meter data in JSON format.</returns>
    protected virtual ValueTask<HttpResponse> GetMetersDataAsync ( HttpRequest request ) {
        string? requestedName = request.Query [ "name" ].Value;

        var selectedMeters = meters.AsEnumerable ();
        if (!string.IsNullOrWhiteSpace ( requestedName )) {
            selectedMeters = selectedMeters.Where ( m =>
                m.Label.Equals ( requestedName, StringComparison.OrdinalIgnoreCase )
                || m.SanitizedLabel.Equals ( requestedName, StringComparison.OrdinalIgnoreCase ) );
        }

        string json = SerializeMetersPayload ( selectedMeters );

        return new ValueTask<HttpResponse> (
            new HttpResponse ( new StringContent ( json, Encoding.UTF8, "application/json" ) )
        );
    }

    /// <summary>
    /// Generates the JSON payload containing the server health history.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding an HTTP response with health data in JSON format.</returns>
    protected virtual ValueTask<HttpResponse> GetServerHealthDataAsync ( HttpRequest request ) {
        string? requestedPeriod = request.Query [ "period" ].Value;
        (TimeSpan period, TimeSpan bucket) = requestedPeriod switch {
            "1d" => (TimeSpan.FromDays ( 1 ), TimeSpan.FromMinutes ( 5 )),
            "7d" => (TimeSpan.FromDays ( 7 ), TimeSpan.FromMinutes ( 30 )),
            "30d" => (TimeSpan.FromDays ( 30 ), TimeSpan.FromHours ( 2 )),
            _ => (TimeSpan.FromHours ( 1 ), TimeSpan.FromMinutes ( 1 ))
        };

        HealthSnapshot [] history = ReadHealthSnapshots ( period, bucket );
        HealthSnapshot snapshot = ReadCurrentHealthSnapshot ();
        string json = SerializeHealthPayload ( snapshot, history );

        return new ValueTask<HttpResponse> (
            new HttpResponse ( new StringContent ( json, Encoding.UTF8, "application/json" ) )
        );
    }

    /// <summary>
    /// Generates the JSON payload consumed by the dashboard page, containing HTTP traffic, latency, per-route statistics,
    /// recent exceptions and pinned counters.
    /// </summary>
    /// <param name="request">The incoming HTTP request. The <c>period</c> query parameter accepts <c>1h</c>, <c>1d</c> or <c>7d</c>.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding an HTTP response with dashboard data in JSON format.</returns>
    protected virtual ValueTask<HttpResponse> GetDashboardDataAsync ( HttpRequest request ) {
        (TimeSpan period, TimeSpan step) = request.Query [ "period" ].Value switch {
            "1d" => (TimeSpan.FromDays ( 1 ), TimeSpan.FromMinutes ( 15 )),
            "7d" => (TimeSpan.FromDays ( 7 ), TimeSpan.FromHours ( 2 )),
            _ => (TimeSpan.FromHours ( 1 ), TimeSpan.FromMinutes ( 1 ))
        };

        long inFlightExcludingCurrent = Math.Max ( 0, Interlocked.Read ( ref httpInFlight ) - 1 );
        HttpMetricsPayload metrics = httpMetrics.Read ( period, step, inFlightExcludingCurrent );

        string json = new StringBuilder ()
            .Append ( "{\"http\":" )
            .Append ( JsonSerializer.Serialize ( metrics, HttpMetricsJsonContext.Default.HttpMetricsPayload ) )
            .Append ( ",\"counters\":" )
            .Append ( SerializeCountersPayload ( counters.Where ( c => c.DashboardPinned ) ) )
            .Append ( '}' )
            .ToString ();

        return new ValueTask<HttpResponse> (
            new HttpResponse ( new StringContent ( json, Encoding.UTF8, "application/json" ) )
        );
    }

    /// <summary>
    /// Generates the JSON payload containing the current values of all captured counters.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding an HTTP response with counter data in JSON format.</returns>
    protected virtual ValueTask<HttpResponse> GetCountersDataAsync ( HttpRequest request ) {
        string json = SerializeCountersPayload ( counters );

        return new ValueTask<HttpResponse> (
            new HttpResponse ( new StringContent ( json, Encoding.UTF8, "application/json" ) )
        );
    }

    /// <summary>
    /// Generates the JSON payload containing the current buffered lines of a log stream.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="logStream">The log stream definition whose buffered lines will be returned.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding an HTTP response with log lines in JSON format.</returns>
    protected virtual ValueTask<HttpResponse> GetLogStreamDataAsync ( HttpRequest request, MonitoringDefinition<LogStream> logStream ) {
        string [] logContent = logStream.Instance.PeekEntries ();
        if (logContent.Length == 0 && logStream.Instance.FilePath is { } filePath) {
            logStreamBufferLineCounts.TryGetValue ( logStream.StorageKey, out int lineCount );
            logContent = ReadLogFileTail ( filePath, lineCount > 0 ? lineCount : 500 );
        }

        string json = SerializeLogStreamPayload ( logContent );

        return new ValueTask<HttpResponse> (
            new HttpResponse ( new StringContent ( json, Encoding.UTF8, "application/json" ) )
        );
    }

    /// <summary>
    /// Generates the HTML for a specific log stream page.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="logStream">The log stream definition to display.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with log stream HTML.</returns>
    protected virtual ValueTask<HttpResponse> GetLogStreamPageHtmlAsync ( HttpRequest request, MonitoringDefinition<LogStream> logStream ) {
        var content = new HtmlElement ( "", fragment => {

            fragment += new HtmlElement ( "a", back => {
                back.ClassList.Add ( "back-link" );
                back.Attributes [ "href" ] = PrefixPath ( "/" );
                back += RenderableText.Raw ( ArrowLeftIcon );
                back += "Dashboard";
            } );

            fragment += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "log-viewer-header" );
                header += new HtmlElement ( "h1", logStream.Label );
            } );

            fragment += new HtmlElement ( "div", toolbar => {
                toolbar.ClassList.Add ( "log-toolbar" );

                toolbar += new HtmlElement ( "div", actions => {
                    actions.ClassList.Add ( "log-toolbar-actions" );

                    actions += new HtmlElement ( "button", btn => {
                        btn.ClassList.Add ( "toolbar-btn" );
                        btn.ClassList.Add ( "active" );
                        btn.Id = "btn-tail";
                        btn.Attributes [ "type" ] = "button";
                        btn += "Tail";
                    } );

                    actions += new HtmlElement ( "button", btn => {
                        btn.ClassList.Add ( "toolbar-btn" );
                        btn.Id = "btn-refresh";
                        btn.Attributes [ "type" ] = "button";
                        btn += "Refresh";
                    } );

                    actions += new HtmlElement ( "button", btn => {
                        btn.ClassList.Add ( "toolbar-btn" );
                        btn.ClassList.Add ( "active" );
                        btn.Id = "btn-toggle-refresh";
                        btn.Attributes [ "type" ] = "button";
                        btn += "Stop refresh";
                    } );

                    if (logStream.Instance.FilePath is not null) {
                        actions += new HtmlElement ( "a", btn => {
                            btn.ClassList.Add ( "toolbar-btn" );
                            btn.Attributes [ "href" ] = PrefixPath ( $"/logstream/{logStream.SanitizedLabel}/download" );
                            btn += "Download";
                        } );
                    }

                } );
            } );

            fragment += new HtmlElement ( "div", logContainer => {
                logContainer.ClassList.Add ( "log-content" );
                logContainer.ClassList.Add ( "log-expanded" );
                logContainer.ClassList.Add ( "log-empty" );
                logContainer.Id = "log-content";
                logContainer.Attributes [ "data-logstream-endpoint" ] = PrefixPath ( $"/logstream/{logStream.SanitizedLabel}/data" );
                logContainer += "No log entries yet.";
            } );
        } );

        string html = BuildPageHtml ( content, logStream.SanitizedLabel );
        return new ValueTask<HttpResponse> (
            new HttpResponse ( 200 ) {
                Content = new HtmlContent ( html )
            }
        );
    }

    /// <summary>
    /// Serves the log file linked to a log stream as a downloadable attachment.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="logStream">The log stream definition whose file will be downloaded.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with the file content, or 404 if no file is linked.</returns>
    protected virtual ValueTask<HttpResponse> GetLogStreamFileDownloadAsync ( HttpRequest request, MonitoringDefinition<LogStream> logStream ) {
        string? filePath = logStream.Instance.FilePath;
        if (filePath is null || !File.Exists ( filePath ))
            return new ValueTask<HttpResponse> ( new HttpResponse ( 404 ) );

        return new ValueTask<HttpResponse> ( new HttpResponse ( 200 ) {
            Content = new FileContent ( filePath )
        } );
    }

    /// <summary>
    /// Generates the HTML for the server health page displaying disk, memory and CPU metrics.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with server health HTML.</returns>
    protected virtual ValueTask<HttpResponse> GetServerHealthPageHtmlAsync ( HttpRequest request ) {

        var snapshot = ReadCurrentHealthSnapshot ();
        var history = ReadHealthSnapshots ( TimeSpan.FromHours ( 1 ), TimeSpan.FromMinutes ( 1 ) );

        long appMemory = snapshot.AppMemoryBytes;
        TimeSpan uptime = DateTime.Now - snapshot.ProcessStartTime;
        double cpuUsage = snapshot.CpuPercent;
        string rootPath = snapshot.DriveRoot;
        long diskTotal = snapshot.DiskTotalBytes;
        long diskFree = snapshot.DiskFreeBytes;
        long diskUsed = diskTotal - diskFree;
        double diskPercent = snapshot.DiskPercent;

        var content = new HtmlElement ( "", fragment => {

            fragment += new HtmlElement ( "a", back => {
                back.ClassList.Add ( "back-link" );
                back.Attributes [ "href" ] = PrefixPath ( "/" );
                back += RenderableText.Raw ( ArrowLeftIcon );
                back += "Dashboard";
            } );

            fragment += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "content-header" );
                header += new HtmlElement ( "h1", "Server Health" );
                header += new HtmlElement ( "p", $"Drive: {rootPath}" ).WithClass ( "description" );
            } );

            fragment += WriteAutoRefreshToolbar ();

            fragment += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "section" );
                section += new HtmlElement ( "div", header => {
                    header.ClassList.Add ( "usage-history-header" );
                    header += new HtmlElement ( "h2", "Usage History" );
                    header += new HtmlElement ( "div", periods => {
                        periods.ClassList.Add ( "usage-history-periods" );
                        periods.Attributes [ "role" ] = "group";
                        periods.Attributes [ "aria-label" ] = "Usage history period";

                        foreach ((string value, string label) in new [] { ("1h", "1h"), ("1d", "1d"), ("7d", "7d"), ("30d", "30d") }) {
                            periods += new HtmlElement ( "button", button => {
                                button.ClassList.Add ( "toolbar-btn" );
                                if (value == "1h")
                                    button.ClassList.Add ( "active" );
                                button.Attributes [ "type" ] = "button";
                                button.Attributes [ "data-health-period" ] = value;
                                button.Attributes [ "aria-pressed" ] = value == "1h" ? "true" : "false";
                                button += label;
                            } );
                        }
                    } );
                } );

                section += new HtmlElement ( "div", chart => {
                    chart.ClassList.Add ( "health-chart-container" );
                    chart.Attributes [ "data-health-endpoint" ] = PrefixPath ( "/health/data" );
                    chart.Attributes [ "data-health-readings" ] = SerializeHealthReadingsPayload ( history );
                } );
            } );

            fragment += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "section" );
                section += new HtmlElement ( "h2", "Uptime" );

                section += new HtmlElement ( "div", grid => {
                    grid.ClassList.Add ( "cards-grid" );
                    grid += WriteHealthCard ( "Server Uptime", FormatUptime ( uptime ) );
                    grid += WriteHealthCard ( "Started At", snapshot.ProcessStartTime.ToString ( "yyyy-MM-dd HH:mm:ss", CultureInfo.InvariantCulture ) );
                } );
            } );

            fragment += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "section" );
                section += new HtmlElement ( "h2", "Disk" );

                section += new HtmlElement ( "div", grid => {
                    grid.ClassList.Add ( "cards-grid" );
                    grid += WriteHealthCard ( "Total", SizeHelper.HumanReadableSize ( diskTotal ) );
                    grid += WriteHealthCard ( "Used", SizeHelper.HumanReadableSize ( diskUsed ) );
                    grid += WriteHealthCard ( "Free", SizeHelper.HumanReadableSize ( diskFree ) );
                } );

                section += WriteProgressBar ( diskPercent );
            } );

            fragment += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "section" );
                section += new HtmlElement ( "h2", "Memory" );

                section += new HtmlElement ( "div", grid => {
                    grid.ClassList.Add ( "cards-grid" );
                    grid += WriteHealthCard ( "App Memory", SizeHelper.HumanReadableSize ( appMemory ) );

                    if (OperatingSystem.IsLinux ()) {
                        try {
                            string memInfo = File.ReadAllText ( "/proc/meminfo" );
                            long memFree = ParseProcMemValue ( memInfo, "MemAvailable" );
                            grid += WriteHealthCard ( "Host Free", SizeHelper.HumanReadableSize ( memFree ) );
                        }
                        catch { }
                    }
                } );
            } );

            fragment += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "section" );
                section += new HtmlElement ( "h2", "CPU" );

                section += new HtmlElement ( "div", grid => {
                    grid.ClassList.Add ( "cards-grid" );
                    grid += WriteHealthCard ( "Process CPU", $"{cpuUsage:n1}%" );
                    grid += WriteHealthCard ( "Logical Cores", Environment.ProcessorCount.ToString () );
                } );

                section += WriteProgressBar ( cpuUsage );
            } );
        } );

        string html = BuildPageHtml ( content, "health" );
        return new ValueTask<HttpResponse> (
            new HttpResponse ( 200 ) {
                Content = new HtmlContent ( html )
            }
        );
    }

    HealthSnapshot CreateHealthSnapshot () {
        var process = Process.GetCurrentProcess ();
        long appMemory = process.WorkingSet64;

        double cpuUsage;
        {
            var startTime = DateTime.UtcNow;
            var startCpuUsage = process.TotalProcessorTime;
            Thread.Sleep ( 100 );
            process.Refresh ();
            var endTime = DateTime.UtcNow;
            var endCpuUsage = process.TotalProcessorTime;
            double cpuUsedMs = (endCpuUsage - startCpuUsage).TotalMilliseconds;
            double totalMs = (endTime - startTime).TotalMilliseconds;
            cpuUsage = totalMs > 0 ? cpuUsedMs / (Environment.ProcessorCount * totalMs) * 100.0 : 0;
        }

        string currentDir = Environment.CurrentDirectory;
        string rootPath = Path.GetPathRoot ( currentDir ) ?? currentDir;
        var driveInfo = new DriveInfo ( rootPath );

        long diskTotal = driveInfo.TotalSize;
        long diskFree = driveInfo.AvailableFreeSpace;
        double diskPercent = diskTotal > 0 ? (double) (diskTotal - diskFree) / diskTotal * 100.0 : 0;
        double memoryPercent = 0;

        long totalAvailableMemory = GC.GetGCMemoryInfo ().TotalAvailableMemoryBytes;
        if (totalAvailableMemory > 0)
            memoryPercent = Math.Clamp ( (double) appMemory / totalAvailableMemory * 100.0, 0, 100 );

        return new HealthSnapshot ( DateTime.Now, cpuUsage, diskPercent, memoryPercent, appMemory, diskTotal, diskFree, rootPath, process.StartTime );
    }

    void TrackHealthSnapshot ( HealthSnapshot snapshot ) {
        DateTime threshold = snapshot.Timestamp - TimeSpan.FromDays ( 30 );

        lock (healthSnapshotsSync) {
            HealthSnapshot? latest = healthSnapshots.Count > 0 ? healthSnapshots [ ^1 ] : null;
            if (latest is not { } existing || snapshot.Timestamp - existing.Timestamp >= TimeSpan.FromMinutes ( 1 )) {
                healthSnapshots.Add ( snapshot );
            }
            else {
                healthSnapshots [ ^1 ] = snapshot with { Timestamp = existing.Timestamp };
            }

            healthSnapshots.RemoveAll ( item => item.Timestamp < threshold );
        }
    }

    HealthSnapshot ReadCurrentHealthSnapshot () {
        lock (healthSnapshotsSync) {
            return healthSnapshots.Count > 0 ? healthSnapshots [ ^1 ] : CreateHealthSnapshot ();
        }
    }

    HealthSnapshot [] ReadHealthSnapshots ( TimeSpan period, TimeSpan bucket ) {
        DateTime threshold = DateTime.Now - period;

        lock (healthSnapshotsSync) {
            return healthSnapshots
                .Where ( item => item.Timestamp >= threshold )
                .GroupBy ( item => item.Timestamp.Ticks / bucket.Ticks )
                .Select ( group => {
                    HealthSnapshot latest = group.Last ();
                    return latest with {
                        CpuPercent = group.Average ( item => item.CpuPercent ),
                        DiskPercent = group.Average ( item => item.DiskPercent ),
                        MemoryPercent = group.Average ( item => item.MemoryPercent )
                    };
                } )
                .ToArray ();
        }
    }

    internal void SetHealthSampling ( bool enabled ) {
        if (enabled) {
            if (healthSamplingTask is not null)
                return;

            var cancellation = new CancellationTokenSource ();
            healthSamplingCancellation = cancellation;
            TrackHealthSnapshot ( CreateHealthSnapshot () );
            healthSamplingTask = Task.Run ( async () => {
                try {
                    using var timer = new PeriodicTimer ( TimeSpan.FromMinutes ( 1 ) );
                    while (await timer.WaitForNextTickAsync ( cancellation.Token ).ConfigureAwait ( false ))
                        TrackHealthSnapshot ( CreateHealthSnapshot () );
                }
                catch (OperationCanceledException) when (cancellation.IsCancellationRequested) {
                }
            } );
            return;
        }

        healthSamplingCancellation?.Cancel ();
        healthSamplingTask?.ConfigureAwait ( false ).GetAwaiter ().GetResult ();
        healthSamplingCancellation?.Dispose ();
        healthSamplingCancellation = null;
        healthSamplingTask = null;
    }

    internal void TrackHttpRequestOpen ( HttpRequest request ) {
        Interlocked.Increment ( ref httpInFlight );
        request.Context.RequestBag [ InFlightBagKey ] = true;
    }

    internal void TrackHttpRequestClose ( HttpServerExecutionResult result ) {
        HttpContext context = result.Context;
        if (context.RequestBag.Remove ( InFlightBagKey ))
            Interlocked.Decrement ( ref httpInFlight );

        Route? route = context.MatchedRoute;
        if (route is not null && ReferenceEquals ( route.RequestHandlers, routeHandlers ))
            return;

        // unhandled exceptions carry an internal placeholder response whose public status is 200,
        // so the execution status must be inspected before the response status code
        int? statusCode = result.Status switch {
            HttpServerExecutionStatus.ConnectionClosed or HttpServerExecutionStatus.RemoteRequestDropped => null,
            HttpServerExecutionStatus.ExceptionThrown or HttpServerExecutionStatus.UncaughtExceptionThrown => 500,
            HttpServerExecutionStatus.MalformedRequest or HttpServerExecutionStatus.DnsUnknownHost => 400,
            HttpServerExecutionStatus.ContentTooLarge => 413,
            HttpServerExecutionStatus.ListeningHostNotReady => 503,
            HttpServerExecutionStatus.NoResponse => 204,
            _ => result.Response?.Status.StatusCode
        };

        if (statusCode is not { } code)
            return;

        string routeKey = route is null ? "(unmatched)" : $"{context.Request.Method.Method} {route.Path}";
        httpMetrics.Record ( routeKey, code, result.Elapsed.TotalMilliseconds, result.ServerException );
    }

    static string FormatUptime ( TimeSpan uptime ) {
        if (uptime.TotalDays >= 1)
            return $"{(int) uptime.TotalDays}d {uptime.Hours:D2}h {uptime.Minutes:D2}m {uptime.Seconds:D2}s";
        if (uptime.TotalHours >= 1)
            return $"{uptime.Hours:D2}h {uptime.Minutes:D2}m {uptime.Seconds:D2}s";
        if (uptime.TotalMinutes >= 1)
            return $"{uptime.Minutes:D2}m {uptime.Seconds:D2}s";
        return $"{uptime.Seconds}s";
    }

    static long ParseProcMemValue ( string memInfo, string key ) {
        foreach (var line in memInfo.Split ( '\n' )) {
            if (line.StartsWith ( key, StringComparison.OrdinalIgnoreCase )) {
                string value = line [ (key.Length + 1).. ].Trim ().Replace ( "kB", "" ).Trim ();
                if (long.TryParse ( value, out long kb ))
                    return kb * 1024;
            }
        }
        return 0;
    }

    static string [] ReadLogFileTail ( string filePath, int lineCount ) {
        try {
            if (!File.Exists ( filePath ))
                return [];

            return File.ReadLines ( filePath )
                .TakeLast ( lineCount )
                .ToArray ();
        }
        catch {
            return [];
        }
    }

    HtmlElement WriteHealthCard ( string label, string value ) {
        return new HtmlElement ( "div", card => {
            card.ClassList.Add ( "card" );
            card += new HtmlElement ( "div", label ).WithClass ( "card-label" );
            card += new HtmlElement ( "div", value ).WithClass ( "card-value" );
        } );
    }

    HtmlElement WriteCounterCard ( MonitoringDefinition<Counter> counter ) {
        return new HtmlElement ( "div", card => {
            card.ClassList.Add ( "card" );
            card.Attributes [ "data-counter-id" ] = counter.SanitizedLabel;
            card += new HtmlElement ( "div", counter.Label ).WithClass ( "card-label" );
            card += new HtmlElement ( "div", "-" ).WithClass ( "card-value" );
        } );
    }

    HtmlElement WriteMeterCard ( MonitoringDefinition<Meter> meter ) {
        var readings = meter.Instance.Read ().ToArray ();
        var values = readings.Select ( r => r.Value ).ToArray ();

        double total = values.Sum ();
        string currentText = FormatMeasuredValue ( total );
        string readingsJson = SerializeReadingsPayload ( readings );

        return new HtmlElement ( "div", card => {
            card.ClassList.Add ( "card" );
            card.ClassList.Add ( "meter-card" );
            card.Attributes [ "data-meter-id" ] = meter.SanitizedLabel;
            card.Attributes [ "data-meter-label" ] = meter.Label;
            card.Attributes [ "data-meter-group" ] = meter.Group ?? string.Empty;

            card += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "meter-card-header" );
                header += new HtmlElement ( "div", meter.Label ).WithClass ( "card-label" );

                header += new HtmlElement ( "button", btn => {
                    btn.ClassList.Add ( "toolbar-btn" );
                    btn.ClassList.Add ( "meter-expand-btn" );
                    btn.Attributes [ "type" ] = "button";
                    btn.Attributes [ "data-meter-id" ] = meter.SanitizedLabel;
                    btn += "Expand";
                } );
            } );

            card += new HtmlElement ( "div", currentText ).WithClass ( "card-value meter-current-value" );

            card += new HtmlElement ( "div", chart => {
                chart.ClassList.Add ( "meter-chart-container" );
                chart.Attributes [ "data-meter-id" ] = meter.SanitizedLabel;
                chart.Attributes [ "data-readings" ] = readingsJson;
            } );
        } );
    }

    static string FormatMeasuredValue ( double value ) {
        if (value == 0)
            return "-";
        if (value > 0 && value < 0.01)
            return "~ 0.01";

        return Math.Round ( value, 2 ).ToString ( "N2", CultureInfo.InvariantCulture );
    }

    static IEnumerable<IGrouping<string, MonitoringDefinition<T>>> GroupDefinitionsByGroup<T> ( IEnumerable<MonitoringDefinition<T>> definitions ) where T : notnull {
        return definitions
            .OrderBy ( d => string.IsNullOrWhiteSpace ( d.Group ) ? 1 : 0 )
            .ThenBy ( d => d.Group ?? string.Empty, StringComparer.OrdinalIgnoreCase )
            .ThenBy ( d => d.Label, StringComparer.OrdinalIgnoreCase )
            .GroupBy ( d => string.IsNullOrWhiteSpace ( d.Group ) ? "Ungrouped" : d.Group! );
    }

    HtmlElement WriteProgressBar ( double percent ) {
        double clamped = Math.Clamp ( percent, 0, 100 );
        string color = clamped switch {
            > 90 => "var(--danger)",
            > 70 => "var(--warning)",
            _ => "var(--accent)"
        };

        return new HtmlElement ( "div", bar => {
            bar.ClassList.Add ( "progress-bar" );
            bar += new HtmlElement ( "div", fill => {
                fill.ClassList.Add ( "progress-fill" );
                fill.Style = new { width = $"{clamped:n1}%", backgroundColor = color };
            } );
            bar += new HtmlElement ( "span", $"{clamped:n1}%" ).WithClass ( "progress-label" );
        } );
    }

    static string SerializeMetersPayload ( IEnumerable<MonitoringDefinition<Meter>> definitions ) {
        var builder = new StringBuilder ();
        builder.Append ( '[' );

        bool isFirstMeter = true;
        foreach (var definition in definitions) {
            if (!isFirstMeter)
                builder.Append ( ',' );
            isFirstMeter = false;

            var readings = definition.Instance.Read ().ToArray ();
            var values = readings.Select ( r => r.Value ).ToArray ();
            double current = values.LastOrDefault ();
            double min = values.Length > 0 ? values.Min () : 0;
            double max = values.Length > 0 ? values.Max () : 0;
            double avg = values.Length > 0 ? values.Average () : 0;
            double total = values.Sum ();

            builder.Append ( '{' );

            builder.Append ( "\"id\":\"" );
            builder.Append ( EscapeJsonString ( definition.SanitizedLabel ) );
            builder.Append ( "\"," );

            builder.Append ( "\"label\":\"" );
            builder.Append ( EscapeJsonString ( definition.Label ) );
            builder.Append ( "\"," );

            builder.Append ( "\"group\":" );
            if (definition.Group is null) {
                builder.Append ( "null" );
            }
            else {
                builder.Append ( '"' );
                builder.Append ( EscapeJsonString ( definition.Group ) );
                builder.Append ( '"' );
            }
            builder.Append ( ',' );

            builder.Append ( "\"dashboardPinned\":" );
            builder.Append ( definition.DashboardPinned ? "true" : "false" );
            builder.Append ( ',' );

            builder.Append ( "\"current\":" );
            builder.Append ( JsonNumber ( current ) );
            builder.Append ( ',' );

            builder.Append ( "\"min\":" );
            builder.Append ( JsonNumber ( min ) );
            builder.Append ( ',' );

            builder.Append ( "\"max\":" );
            builder.Append ( JsonNumber ( max ) );
            builder.Append ( ',' );

            builder.Append ( "\"avg\":" );
            builder.Append ( JsonNumber ( avg ) );
            builder.Append ( ',' );

            builder.Append ( "\"total\":" );
            builder.Append ( JsonNumber ( total ) );
            builder.Append ( ',' );

            builder.Append ( "\"readings\":" );
            builder.Append ( SerializeReadingsPayload ( readings ) );

            builder.Append ( '}' );
        }

        builder.Append ( ']' );
        return builder.ToString ();
    }

    static string SerializeReadingsPayload ( MeterReading [] readings ) {
        var builder = new StringBuilder ();
        builder.Append ( '[' );

        for (int index = 0; index < readings.Length; index++) {
            if (index > 0)
                builder.Append ( ',' );

            MeterReading reading = readings [ index ];
            builder.Append ( "{\"timestamp\":\"" );
            builder.Append ( reading.Timestamp.ToString ( "O", CultureInfo.InvariantCulture ) );
            builder.Append ( "\",\"value\":" );
            builder.Append ( JsonNumber ( reading.Value ) );
            builder.Append ( '}' );
        }

        builder.Append ( ']' );
        return builder.ToString ();
    }

    static string SerializeHealthPayload ( HealthSnapshot snapshot, HealthSnapshot [] history ) {
        var builder = new StringBuilder ();
        builder.Append ( '{' );

        builder.Append ( "\"cpuPercent\":" );
        builder.Append ( JsonNumber ( snapshot.CpuPercent ) );
        builder.Append ( ',' );

        builder.Append ( "\"diskPercent\":" );
        builder.Append ( JsonNumber ( snapshot.DiskPercent ) );
        builder.Append ( ',' );

        builder.Append ( "\"memoryPercent\":" );
        builder.Append ( JsonNumber ( snapshot.MemoryPercent ) );
        builder.Append ( ',' );

        builder.Append ( "\"appMemoryBytes\":" );
        builder.Append ( snapshot.AppMemoryBytes.ToString ( CultureInfo.InvariantCulture ) );
        builder.Append ( ',' );

        builder.Append ( "\"diskTotalBytes\":" );
        builder.Append ( snapshot.DiskTotalBytes.ToString ( CultureInfo.InvariantCulture ) );
        builder.Append ( ',' );

        builder.Append ( "\"diskFreeBytes\":" );
        builder.Append ( snapshot.DiskFreeBytes.ToString ( CultureInfo.InvariantCulture ) );
        builder.Append ( ',' );

        builder.Append ( "\"readings\":" );
        builder.Append ( SerializeHealthReadingsPayload ( history ) );

        builder.Append ( '}' );
        return builder.ToString ();
    }

    static string SerializeHealthReadingsPayload ( HealthSnapshot [] history ) {
        var builder = new StringBuilder ();
        builder.Append ( '[' );

        for (int index = 0; index < history.Length; index++) {
            if (index > 0)
                builder.Append ( ',' );

            HealthSnapshot reading = history [ index ];
            builder.Append ( "{\"timestamp\":\"" );
            builder.Append ( reading.Timestamp.ToString ( "O", CultureInfo.InvariantCulture ) );
            builder.Append ( "\",\"cpu\":" );
            builder.Append ( JsonNumber ( reading.CpuPercent ) );
            builder.Append ( ",\"disk\":" );
            builder.Append ( JsonNumber ( reading.DiskPercent ) );
            builder.Append ( ",\"memory\":" );
            builder.Append ( JsonNumber ( reading.MemoryPercent ) );
            builder.Append ( '}' );
        }

        builder.Append ( ']' );
        return builder.ToString ();
    }

    static string SerializeCountersPayload ( IEnumerable<MonitoringDefinition<Counter>> definitions ) {
        var builder = new StringBuilder ();
        builder.Append ( '[' );

        bool first = true;
        foreach (var definition in definitions) {
            if (!first)
                builder.Append ( ',' );
            first = false;

            builder.Append ( "{\"id\":\"" )
                .Append ( EscapeJsonString ( definition.SanitizedLabel ) )
                .Append ( "\",\"label\":\"" )
                .Append ( EscapeJsonString ( definition.Label ) )
                .Append ( "\",\"group\":" );

            if (definition.Group is null) {
                builder.Append ( "null" );
            }
            else {
                builder.Append ( '"' )
                    .Append ( EscapeJsonString ( definition.Group ) )
                    .Append ( '"' );
            }

            builder.Append ( ",\"value\":" )
                .Append ( JsonNumber ( definition.Instance.Current ) )
                .Append ( '}' );
        }

        builder.Append ( ']' );
        return builder.ToString ();
    }

    static string SerializeLogStreamPayload ( IEnumerable<string> lines ) {
        var builder = new StringBuilder ();
        builder.Append ( "{\"lines\":[" );

        bool first = true;
        foreach (string line in lines) {
            if (!first)
                builder.Append ( ',' );
            first = false;

            builder.Append ( '"' )
                .Append ( EscapeJsonString ( line ) )
                .Append ( '"' );
        }

        builder.Append ( "]}" );
        return builder.ToString ();
    }

    static string EscapeJsonString ( string value ) {
        var builder = new StringBuilder ( value.Length + 8 );

        foreach (char character in value) {
            switch (character) {
                case '\\':
                    builder.Append ( "\\\\" );
                    break;
                case '"':
                    builder.Append ( "\\\"" );
                    break;
                case '\b':
                    builder.Append ( "\\b" );
                    break;
                case '\f':
                    builder.Append ( "\\f" );
                    break;
                case '\n':
                    builder.Append ( "\\n" );
                    break;
                case '\r':
                    builder.Append ( "\\r" );
                    break;
                case '\t':
                    builder.Append ( "\\t" );
                    break;
                default:
                    if (character < 0x20) {
                        builder.Append ( "\\u" );
                        builder.Append ( ((int) character).ToString ( "x4", CultureInfo.InvariantCulture ) );
                    }
                    else {
                        builder.Append ( character );
                    }
                    break;
            }
        }

        return builder.ToString ();
    }

    static string JsonNumber ( double value ) {
        if (!double.IsFinite ( value ))
            return "0";

        return value.ToString ( "0.################", CultureInfo.InvariantCulture );
    }

    internal IEnumerable<Route> GetRoutes ( string prefix = "/" ) {

        if (currentRoutePrefix is not null)
            throw new InvalidOperationException ( SR.Format ( SR.ApplicationMonitor_RoutePrefixChanged, currentRoutePrefix, prefix ) );

        currentRoutePrefix = prefix;
        RestoreStoreState ();
        StartStoreFlushLoop ();
        IRequestHandler [] handlers = [ new AuthorizationRequestHandler ( this ) ];
        routeHandlers = handlers;

        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetDashboardPageHtmlAsync ( request );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/data" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetDashboardDataAsync ( request );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/counters" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetCountersPageHtmlAsync ( request );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/counters/data" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetCountersDataAsync ( request );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/meters" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetMetersPageHtmlAsync ( request );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/meters/data" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetMetersDataAsync ( request );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/health/data" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetServerHealthDataAsync ( request );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/logstream/<name>" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;

            string logstreamName = request.RouteParameters [ "name" ].GetString ();
            var matchedLogStream = capturingLogStreams
                .FirstOrDefault ( f => f.Label.Equals ( logstreamName, StringComparison.OrdinalIgnoreCase ) );

            if (matchedLogStream is null)
                return new HttpResponse ( 404 );

            return await GetLogStreamPageHtmlAsync ( request, matchedLogStream );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/logstream/<name>/data" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;

            string logstreamDataName = request.RouteParameters [ "name" ].GetString ();
            var matchedDataLogStream = capturingLogStreams
                .FirstOrDefault ( f => f.Label.Equals ( logstreamDataName, StringComparison.OrdinalIgnoreCase ) );

            if (matchedDataLogStream is null)
                return new HttpResponse ( 404 );

            return await GetLogStreamDataAsync ( request, matchedDataLogStream );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/logstream/<name>/download" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;

            string logstreamName = request.RouteParameters [ "name" ].GetString ();
            var matchedLogStream = capturingLogStreams
                .FirstOrDefault ( f => f.Label.Equals ( logstreamName, StringComparison.OrdinalIgnoreCase ) );

            if (matchedLogStream is null)
                return new HttpResponse ( 404 );

            return await GetLogStreamFileDownloadAsync ( request, matchedLogStream );
        }, handlers );
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/health" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetServerHealthPageHtmlAsync ( request );
        }, handlers );
    }

    /// <inheritdoc/>
    public void Dispose () {
        if (disposed)
            return;

        disposed = true;
        storeFlushCancellation?.Cancel ();
        SetHealthSampling ( enabled: false );

        try {
            storeFlushTask?.ConfigureAwait ( false ).GetAwaiter ().GetResult ();
        }
        finally {
            storeFlushCancellation?.Dispose ();

            if (Store is IAsyncDisposable asyncDisposableStore) {
                ValueTask disposeTask = asyncDisposableStore.DisposeAsync ();
                if (disposeTask.IsCompletedSuccessfully) {
                    disposeTask.GetAwaiter ().GetResult ();
                }
                else {
                    disposeTask.AsTask ().ConfigureAwait ( false ).GetAwaiter ().GetResult ();
                }
            }
            else {
                (Store as IDisposable)?.Dispose ();
            }
        }

        GC.SuppressFinalize ( this );
    }

    /// <inheritdoc/>
    public async ValueTask DisposeAsync () {
        if (disposed)
            return;

        disposed = true;

        if (storeFlushCancellation is not null)
            await storeFlushCancellation.CancelAsync ().ConfigureAwait ( false );
        SetHealthSampling ( enabled: false );

        try {
            if (storeFlushTask is not null)
                await storeFlushTask.ConfigureAwait ( false );
        }
        finally {
            storeFlushCancellation?.Dispose ();

            if (Store is IAsyncDisposable asyncDisposableStore) {
                await asyncDisposableStore.DisposeAsync ().ConfigureAwait ( false );
            }
            else {
                (Store as IDisposable)?.Dispose ();
            }
        }

        GC.SuppressFinalize ( this );
    }

    class AuthorizationRequestHandler ( ApplicationMonitor monitor ) : IRequestHandler {

        ApplicationMonitor monitor = monitor;

        public RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeResponse;

        static HttpResponse UnauthorizedResponse => new HttpResponse ( HttpStatusInformation.Unauthorized ) {
            Content = new HtmlContent ( DefaultMessagePage.Instance.CreateMessageHtml ( "Unauthorized", SR.ApplicationMonitor_AuthenticationRequired ) ),
            Headers = new () {
                WWWAuthenticate = "Basic realm=\"Credentials required to access this page.\""
            }
        };

        public HttpResponse? Execute ( HttpRequest request, HttpContext context ) {
            string? authorization = request.Headers.Authorization;
            if (authorization is null || !authorization.StartsWith ( "Basic " )) {
                return UnauthorizedResponse;
            }

            string encodedCredentials = authorization [ "Basic ".Length.. ];
            string decodedCredentials;
            try {
                decodedCredentials = System.Text.Encoding.UTF8.GetString ( Convert.FromBase64String ( encodedCredentials ) );
            }
            catch (FormatException) {
                return UnauthorizedResponse;
            }

            int separatorIndex = decodedCredentials.IndexOf ( ':' );
            if (separatorIndex < 0) {
                return UnauthorizedResponse;
            }

            string userEmail = decodedCredentials [ ..separatorIndex ];
            string userPassword = decodedCredentials [ (separatorIndex + 1).. ];

            bool isAuthenticated = monitor.AuthenticateAccountAsync ( userEmail, userPassword ).GetAwaiter ().GetResult ();
            if (!isAuthenticated) {
                return UnauthorizedResponse;
            }

            return null;
        }
    }

    record struct HealthSnapshot ( DateTime Timestamp, double CpuPercent, double DiskPercent, double MemoryPercent, long AppMemoryBytes, long DiskTotalBytes, long DiskFreeBytes, string DriveRoot, DateTime ProcessStartTime );

}
