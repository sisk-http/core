using System.Diagnostics;
using System.Globalization;
using System.Net;
using System.Text.RegularExpressions;
using Sisk.Core.Helpers;
using Sisk.Core.Http;
using Sisk.Core.Routing;
using TinyComponents;

namespace Sisk.Monitoring;

/// <summary>
/// Provides an embedded web dashboard for monitoring application counters, log streams and server health.
/// </summary>
public class ApplicationMonitor {

    static readonly Regex dateTokenRegex = new Regex (
        @"\b(\d{4}[-/]\d{1,2}[-/]\d{1,2}(?:[ T]\d{1,2}:\d{2}(?::\d{2})?(?:\s?(?:Z|[+-]\d{2}:?\d{2}|[+-]?\d{4}))?)?|\d{1,2}/(?:\d{1,2}|[A-Za-z]{3,9})/\d{2,4}(?:\s+\d{1,2}:\d{2}(?::\d{2})?)?(?:\s+[+-]?\d{4})?)\b",
        RegexOptions.Compiled | RegexOptions.CultureInvariant | RegexOptions.IgnoreCase );

    static readonly Regex numberTokenRegex = new Regex (
        @"(?<![A-Za-z])[-+]?\d+(?:[\.,]\d+)?(?![A-Za-z])",
        RegexOptions.Compiled | RegexOptions.CultureInvariant );

    static readonly Regex bracketTokenRegex = new Regex (
        @"\[[^\]\r\n]+\]",
        RegexOptions.Compiled | RegexOptions.CultureInvariant );

    string? currentRoutePrefix = null;

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

    List<MonitoringDefinition<LogStream>> capturingLogStreams = new ();
    List<MonitoringDefinition<Counter>> counters = new ();

    public string PageTitle { get; set; } = "Monitoring";

    public Func<NetworkCredential, ValueTask<bool>>? CredentialValidator { get; set; } = null;

    public void CaptureCounter ( MonitoringDefinition<Counter> counter ) {
        counters.Add ( counter );
    }

    public void CaptureLogStream ( MonitoringDefinition<LogStream> logStream, int bufferLineCount = 500 ) {
        capturingLogStreams.Add ( logStream );
        if (!logStream.Instance.IsBuffering)
            logStream.Instance.StartBuffering ( bufferLineCount );
    }

    protected virtual ValueTask<bool> AuthenticateAccountAsync ( string userEmail, string userPassword ) {
        if (CredentialValidator is not null) {
            return CredentialValidator ( new NetworkCredential ( userEmail, userPassword ) );
        }
        return new ValueTask<bool> ( true );
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
            head += new HtmlElement ( "title", PageTitle );
            head += new HtmlElement ( "style", RenderableText.Raw ( Style.DefaultStyles ) );
        } );

        html += new HtmlElement ( "body", body => {
            body += new HtmlElement ( "div", wrapper => {
                wrapper.ClassList.Add ( "page-wrapper" );

                body += new HtmlElement ( "div", overlay => {
                    overlay.ClassList.Add ( "sidebar-overlay" );
                } );

                wrapper += WriteSidebar ( activeNavItem );

                body += new HtmlElement ( "button", menuBtn => {
                    menuBtn.ClassList.Add ( "mobile-menu-btn" );
                    menuBtn.Attributes [ "aria-label" ] = "Open menu";
                    menuBtn += "\u2630";
                } );

                wrapper += new HtmlElement ( "main", main => {
                    main += bodyContent;
                } );
            } );

            body += new HtmlElement ( "script", RenderableText.Raw ( Style.DefaultScript ) );
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

            nav += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "sidebar-header" );
                header += new HtmlElement ( "h1", PageTitle );
            } );

            nav += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "nav-section" );

                section += new HtmlElement ( "div", "Dashboard" ).WithClass ( "nav-section-title" );

                section += WriteNavItem ( PrefixPath ( "/" ), "Dashboard", DashboardIcon, activeNavItem == "dashboard" );
                section += WriteNavItem ( PrefixPath ( "/health" ), "Server Health", HealthIcon, activeNavItem == "health" );

                //if (counters.Count > 0) {
                //    section += new HtmlElement ( "div", "Counters" ).WithClass ( "nav-section-title" );
                //    foreach (var counter in counters) {
                //        section += WriteNavItem ( null, counter.Label, CounterIcon, false );
                //    }
                //}

                if (capturingLogStreams.Count > 0) {
                    section += new HtmlElement ( "div", "Log Streams" ).WithClass ( "nav-section-title" );
                    foreach (var logStream in capturingLogStreams) {
                        string href = PrefixPath ( $"/logstream/{logStream.SanitizedLabel}" );
                        bool isActive = activeNavItem == logStream.SanitizedLabel;
                        section += WriteNavItem ( href, logStream.Label, LogStreamIcon, isActive );
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

            if (counters.Count > 0) {
                fragment += new HtmlElement ( "div", section => {
                    section.ClassList.Add ( "section" );
                    section += new HtmlElement ( "h2", "Counters" );

                    section += new HtmlElement ( "div", grid => {
                        grid.ClassList.Add ( "cards-grid" );

                        foreach (var counter in counters) {
                            string currentText;
                            double current = counter.Instance.Current;

                            if (current == 0) {
                                currentText = "-";
                            }
                            else if (current > 0 && current < 0.01) {
                                currentText = "~ 0.01";
                            }
                            else {
                                currentText = Math.Round ( current, 2 ).ToString ( "N2", CultureInfo.InvariantCulture );
                            }

                            grid += new HtmlElement ( "div", card => {
                                card.ClassList.Add ( "card" );
                                card += new HtmlElement ( "div", counter.Label ).WithClass ( "card-label" );
                                card += new HtmlElement ( "div", currentText ).WithClass ( "card-value" );
                            } );
                        }
                    } );
                } );
            }

            if (capturingLogStreams.Count > 0) {
                fragment += new HtmlElement ( "div", section => {
                    section.ClassList.Add ( "section" );
                    section += new HtmlElement ( "h2", "Log Streams" );

                    section += new HtmlElement ( "div", list => {
                        list.ClassList.Add ( "stream-list" );

                        foreach (var logStream in capturingLogStreams) {
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

            if (counters.Count == 0 && capturingLogStreams.Count == 0) {
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
    /// Generates the HTML for a specific log stream page.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <param name="logStream">The log stream definition to display.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with log stream HTML.</returns>
    protected virtual ValueTask<HttpResponse> GetLogStreamPageHtmlAsync ( HttpRequest request, MonitoringDefinition<LogStream> logStream ) {
        string [] logContent = logStream.Instance.PeekEntries ();

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

                toolbar += new HtmlElement ( "div", meta => {
                    meta.ClassList.Add ( "log-meta" );
                    meta += new HtmlElement ( "span", $"Buffering: {(logStream.Instance.IsBuffering ? "active" : "inactive")}" )
                        .WithClass ( "log-meta-item" );
                } );

                toolbar += new HtmlElement ( "div", actions => {
                    actions.ClassList.Add ( "log-toolbar-actions" );

                    actions += new HtmlElement ( "button", btn => {
                        btn.ClassList.Add ( "toolbar-btn" );
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

                    actions += new HtmlElement ( "button", btn => {
                        btn.ClassList.Add ( "toolbar-btn" );
                        btn.Id = "btn-expand";
                        btn.Attributes [ "type" ] = "button";
                        btn += "Expand";
                    } );
                } );
            } );

            fragment += new HtmlElement ( "div", logContainer => {
                logContainer.ClassList.Add ( "log-content" );
                logContainer.Id = "log-content";

                if (!logContent.Any ()) {
                    logContainer.ClassList.Add ( "log-empty" );
                    logContainer += "No log entries yet.";
                    return;
                }

                foreach (string line in logContent) {
                    logContainer += CreateLogLineElement ( line );
                }
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
    /// Generates the HTML for the server health page displaying disk, memory and CPU metrics.
    /// </summary>
    /// <param name="request">The incoming HTTP request.</param>
    /// <returns>A <see cref="ValueTask{TResult}"/> yielding the HTTP response with server health HTML.</returns>
    protected virtual ValueTask<HttpResponse> GetServerHealthPageHtmlAsync ( HttpRequest request ) {

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
            cpuUsage = cpuUsedMs / (Environment.ProcessorCount * totalMs) * 100.0;
        }

        string currentDir = Environment.CurrentDirectory;
        string rootPath = Path.GetPathRoot ( currentDir ) ?? currentDir;
        var driveInfo = new DriveInfo ( rootPath );

        long diskTotal = driveInfo.TotalSize;
        long diskFree = driveInfo.AvailableFreeSpace;
        long diskUsed = diskTotal - diskFree;
        double diskPercent = diskTotal > 0 ? (double) diskUsed / diskTotal * 100.0 : 0;

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

    HtmlElement CreateLogLineElement ( string line ) {
        return new HtmlElement ( "div", lineElement => {
            lineElement.ClassList.Add ( "log-line" );

            if (line.Length == 0) {
                lineElement += "\u00a0";
                return;
            }

            foreach (object token in GetHighlightedTokens ( line )) {
                lineElement += token;
            }
        } );
    }

    IEnumerable<object> GetHighlightedTokens ( string line ) {
        int currentIndex = 0;

        foreach (Match dateMatch in dateTokenRegex.Matches ( line )) {
            if (dateMatch.Index > currentIndex) {
                foreach (object token in GetBracketAndNumberTokens ( line [ currentIndex..dateMatch.Index ] )) {
                    yield return token;
                }
            }

            yield return new HtmlElement ( "span", dateMatch.Value ).WithClass ( "log-token-date" );
            currentIndex = dateMatch.Index + dateMatch.Length;
        }

        if (currentIndex < line.Length) {
            foreach (object token in GetBracketAndNumberTokens ( line [ currentIndex.. ] )) {
                yield return token;
            }
        }
    }

    IEnumerable<object> GetBracketAndNumberTokens ( string segment ) {
        int currentIndex = 0;

        foreach (Match bracketMatch in bracketTokenRegex.Matches ( segment )) {
            if (bracketMatch.Index > currentIndex) {
                foreach (object token in GetNumberTokens ( segment.Substring ( currentIndex, bracketMatch.Index - currentIndex ) )) {
                    yield return token;
                }
            }

            yield return CreateBracketTokenElement ( bracketMatch.Value );
            currentIndex = bracketMatch.Index + bracketMatch.Length;
        }

        if (currentIndex < segment.Length) {
            foreach (object token in GetNumberTokens ( segment [ currentIndex.. ] )) {
                yield return token;
            }
        }
    }

    HtmlElement CreateBracketTokenElement ( string token ) {
        int hue = GetTokenHueCaseSensitive ( token );

        return new HtmlElement ( "span", token )
            .WithClass ( "log-token-tag" )
            .WithStyle ( new {
                color = $"hsl({hue}, 72%, 38%)",
                backgroundColor = $"hsla({hue}, 90%, 55%, 0.18)"
            } );
    }

    static int GetTokenHueCaseSensitive ( string token ) {
        unchecked {
            uint hash = 2166136261;
            foreach (char ch in token) {
                hash ^= ch;
                hash *= 16777619;
            }
            return (int) (hash % 360);
        }
    }

    IEnumerable<object> GetNumberTokens ( string segment ) {
        int currentIndex = 0;

        foreach (Match numberMatch in numberTokenRegex.Matches ( segment )) {
            if (numberMatch.Index > currentIndex) {
                yield return segment.Substring ( currentIndex, numberMatch.Index - currentIndex );
            }

            yield return new HtmlElement ( "span", numberMatch.Value ).WithClass ( "log-token-number" );
            currentIndex = numberMatch.Index + numberMatch.Length;
        }

        if (currentIndex < segment.Length) {
            yield return segment [ currentIndex.. ];
        }
    }

    HtmlElement WriteHealthCard ( string label, string value ) {
        return new HtmlElement ( "div", card => {
            card.ClassList.Add ( "card" );
            card += new HtmlElement ( "div", label ).WithClass ( "card-label" );
            card += new HtmlElement ( "div", value ).WithClass ( "card-value" );
        } );
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

    internal IEnumerable<Route> GetRoutes ( string prefix = "/" ) {

        if (currentRoutePrefix is not null)
            throw new InvalidOperationException ( $"Route prefix cannot be changed once set. Current prefix: '{currentRoutePrefix}', attempted new prefix: '{prefix}'. Please, use a new instance of the ApplicationMonitor class for multiple servers." );

        currentRoutePrefix = prefix;
        IRequestHandler [] handlers = [ new AuthorizationRequestHandler ( this ) ];

        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetDashboardPageHtmlAsync ( request );
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
        yield return new Route ( RouteMethod.Get, PathHelper.CombinePaths ( prefix, "/health" ), null, async ( HttpRequest request ) => {
            request.Context.LogMode = LogOutput.ErrorLog;
            return await GetServerHealthPageHtmlAsync ( request );
        }, handlers );
    }

    class AuthorizationRequestHandler ( ApplicationMonitor monitor ) : IRequestHandler {

        ApplicationMonitor monitor = monitor;

        public RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeResponse;

        static HttpResponse UnauthorizedResponse => new HttpResponse ( HttpStatusInformation.Unauthorized ) {
            Content = new HtmlContent ( DefaultMessagePage.Instance.CreateMessageHtml ( "Unauthorized", "Authenticate using your credentials to access this page." ) ),
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
            string decodedCredentials = System.Text.Encoding.UTF8.GetString ( Convert.FromBase64String ( encodedCredentials ) );

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
}
