using System.Security.Cryptography;
using System.Text;
using Sisk.Core.Http;
using TinyComponents;

namespace Sisk.Monitoring;

/// <summary>
/// Collects the exceptions shown in the errors page of the monitoring dashboard, grouping them into issues by
/// exception type and location.
/// </summary>
public abstract class MonitoringErrorConsole {
    const int MaxIssues = 200;
    const int MaxOccurrences = 5;
    const int MaxDetailsLength = 32_000;

    readonly Dictionary<string, Issue> issues = new ( StringComparer.Ordinal );
    readonly object sync = new ();
    long version;

    /// <summary>
    /// Gets or sets how long an issue can go without new incidents before it is resolved automatically.
    /// Set to <see langword="null"/> to keep issues unresolved until the user resolves them.
    /// </summary>
    public TimeSpan? ArchiveIssuesAfter { get; set; } = TimeSpan.FromDays ( 7 );

    internal long Version {
        get {
            lock (sync) {
                return version;
            }
        }
    }

    /// <summary>
    /// Called for every exception reported to this console. Override it to attach custom sections or to change
    /// where the exception is reported, calling <see cref="WriteException"/> to record it.
    /// </summary>
    /// <param name="exception">The reported exception.</param>
    /// <param name="context">The HTTP context in which the exception was raised, if any.</param>
    protected virtual void OnException ( Exception exception, HttpContext? context ) {
        WriteException ( new MonitoringIssueContext ( exception, context ) );
    }

    /// <summary>
    /// Reports an exception caught by the application, such as one raised by a background job.
    /// </summary>
    /// <param name="exception">The exception to report.</param>
    /// <param name="context">The related HTTP context. When omitted, the context of the current request is used, if any.</param>
    /// <returns><see langword="true"/> when the exception was recorded; otherwise, <see langword="false"/>.</returns>
    public bool TryInformException ( Exception exception, HttpContext? context = null ) {
        try {
            OnException ( exception, context ?? HttpContext.GetCurrentContext () );
            return true;
        }
        catch {
            return false;
        }
    }

    /// <summary>
    /// Records an incident, creating its issue or reopening it when it was resolved.
    /// </summary>
    /// <param name="context">The exception and the details to store with the incident.</param>
    protected void WriteException ( MonitoringIssueContext context ) {
        ArgumentNullException.ThrowIfNull ( context );

        DateTime now = DateTime.Now;
        Exception exception = context.Exception;
        string type = exception.GetType ().FullName ?? exception.GetType ().Name;
        string? request = context.HttpContext is { } httpContext
            ? $"{httpContext.Request.Method.Method} {httpContext.MatchedRoute?.Path ?? httpContext.Request.Path}"
            : null;

        string? throwSite = exception.StackTrace?.Split ( '\n', 2 ) [ 0 ].Trim ();
        if (throwSite is not null) {
            int fileIndex = throwSite.IndexOf ( " in ", StringComparison.Ordinal );
            throwSite = throwSite [ (throwSite.StartsWith ( "at ", StringComparison.Ordinal ) ? 3 : 0)..(fileIndex < 0 ? throwSite.Length : fileIndex) ];
        }

        string location = context.Location
            ?? request
            ?? (string.IsNullOrEmpty ( throwSite ) ? null : throwSite)
            ?? "Unknown location";

        string id = Convert.ToHexString ( SHA256.HashData ( Encoding.UTF8.GetBytes ( $"{type}\n{location}" ) ), 0, 8 ).ToLowerInvariant ();
        string details = exception.ToString ();

        var occurrence = new MonitoringIssueOccurrenceSnapshot (
            Timestamp: now,
            Message: exception.Message,
            Details: details.Length > MaxDetailsLength ? details [ ..MaxDetailsLength ] : details,
            Request: request,
            Sections: context.Sections
                .Select ( section => new MonitoringIssueSectionSnapshot ( section.Title, section.Contents.ToString () ?? string.Empty ) )
                .ToArray () );

        lock (sync) {
            SweepIssues ( now );

            if (!issues.TryGetValue ( id, out Issue? issue )) {
                if (issues.Count >= MaxIssues) {
                    issues.Remove ( issues.Values
                        .OrderBy ( item => item.Resolved ? 0 : 1 )
                        .ThenBy ( item => item.LastSeen )
                        .First ().Id );
                }

                issue = new Issue ( id, type, location, now );
                issues.Add ( id, issue );
            }

            long hour = now.Ticks / TimeSpan.TicksPerHour;

            issue.LastSeen = now;
            issue.Count++;
            issue.Resolved = false;
            issue.Trend [ hour ] = issue.Trend.GetValueOrDefault ( hour ) + 1;
            issue.Occurrences.Insert ( 0, occurrence );

            if (issue.Occurrences.Count > MaxOccurrences)
                issue.Occurrences.RemoveAt ( MaxOccurrences );

            version++;
        }
    }

    internal void Resolve ( string id ) {
        lock (sync) {
            if (issues.TryGetValue ( id, out Issue? issue ) && !issue.Resolved) {
                issue.Resolved = true;
                version++;
            }
        }
    }

    internal MonitoringIssueSnapshot [] ExportState () {
        DateTime now = DateTime.Now;

        lock (sync) {
            SweepIssues ( now );

            return issues.Values
                .Select ( issue => new MonitoringIssueSnapshot (
                    issue.Id,
                    issue.Type,
                    issue.Location,
                    issue.FirstSeen,
                    issue.LastSeen,
                    issue.Count,
                    issue.Resolved,
                    issue.Trend
                        .OrderBy ( item => item.Key )
                        .Select ( item => new MonitoringIssueTrendSnapshot ( new DateTime ( item.Key * TimeSpan.TicksPerHour ), item.Value ) )
                        .ToArray (),
                    issue.Occurrences.ToArray () ) )
                .ToArray ();
        }
    }

    internal void ImportState ( MonitoringIssueSnapshot [] snapshots ) {
        lock (sync) {
            issues.Clear ();

            foreach (var snapshot in snapshots) {
                if (snapshot is not { Id: not null, Type: not null, Location: not null })
                    continue;

                var issue = new Issue ( snapshot.Id, snapshot.Type, snapshot.Location, snapshot.FirstSeen ) {
                    LastSeen = snapshot.LastSeen,
                    Count = snapshot.Count,
                    Resolved = snapshot.Resolved
                };

                foreach (var reading in snapshot.Trend ?? []) {
                    issue.Trend [ reading.Hour.Ticks / TimeSpan.TicksPerHour ] = reading.Count;
                }

                issue.Occurrences.AddRange ( snapshot.Occurrences ?? [] );
                issues [ snapshot.Id ] = issue;
            }

            version++;
        }
    }

    void SweepIssues ( DateTime now ) {
        long oldestHour = (now.Ticks / TimeSpan.TicksPerHour) - 23;

        foreach (var issue in issues.Values) {
            foreach (long hour in issue.Trend.Keys) {
                if (hour < oldestHour)
                    issue.Trend.Remove ( hour );
            }

            if (!issue.Resolved && ArchiveIssuesAfter is { } archiveAfter && issue.LastSeen + archiveAfter <= now) {
                issue.Resolved = true;
                version++;
            }
        }
    }

    sealed class Issue ( string id, string type, string location, DateTime firstSeen ) {
        public readonly string Id = id;
        public readonly string Type = type;
        public readonly string Location = location;
        public readonly DateTime FirstSeen = firstSeen;
        public readonly Dictionary<long, long> Trend = new ();
        public readonly List<MonitoringIssueOccurrenceSnapshot> Occurrences = new ();
        public DateTime LastSeen = firstSeen;
        public long Count;
        public bool Resolved;
    }
}

internal sealed class DefaultMonitoringErrorConsole : MonitoringErrorConsole {
}

/// <summary>
/// Describes an incident to be recorded by a <see cref="MonitoringErrorConsole"/>.
/// </summary>
public sealed class MonitoringIssueContext {

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitoringIssueContext"/> class.
    /// </summary>
    /// <param name="exception">The exception that caused the incident.</param>
    /// <param name="httpContext">The HTTP context in which the exception was raised, if any.</param>
    /// <param name="sections">Custom sections displayed with the incident.</param>
    public MonitoringIssueContext ( Exception exception, HttpContext? httpContext = null, params MonitoringErrorSection [] sections ) {
        ArgumentNullException.ThrowIfNull ( exception );
        ArgumentNullException.ThrowIfNull ( sections );

        Exception = exception;
        HttpContext = httpContext;
        Sections = [ .. sections ];
    }

    /// <summary>
    /// Gets the exception that caused the incident.
    /// </summary>
    public Exception Exception { get; }

    /// <summary>
    /// Gets the HTTP context in which the exception was raised, if any.
    /// </summary>
    public HttpContext? HttpContext { get; }

    /// <summary>
    /// Gets or sets where the incident happened. Incidents of the same exception type and location belong to the
    /// same issue. When <see langword="null"/>, the method and route of the request are used, or the method that
    /// threw the exception when there is no request.
    /// </summary>
    public string? Location { get; set; }

    /// <summary>
    /// Gets the custom sections displayed with the incident.
    /// </summary>
    public IList<MonitoringErrorSection> Sections { get; }
}

/// <summary>
/// Represents a custom block of content displayed with an incident in the errors page.
/// </summary>
public sealed class MonitoringErrorSection {

    /// <summary>
    /// Initializes a new instance of the <see cref="MonitoringErrorSection"/> class.
    /// </summary>
    /// <param name="title">The title of the section.</param>
    /// <param name="contents">The contents of the section.</param>
    public MonitoringErrorSection ( string title, HtmlElement contents ) {
        ArgumentNullException.ThrowIfNull ( title );
        ArgumentNullException.ThrowIfNull ( contents );

        Title = title;
        Contents = contents;
    }

    /// <summary>
    /// Gets or sets the title of the section.
    /// </summary>
    public string Title { get; set; }

    /// <summary>
    /// Gets or sets the contents of the section. They are rendered when the incident is recorded.
    /// </summary>
    public HtmlElement Contents { get; set; }

    /// <summary>
    /// Creates a section whose contents are built by the specified action.
    /// </summary>
    /// <param name="title">The title of the section.</param>
    /// <param name="builder">The action that fills the element holding the contents.</param>
    /// <returns>The created section.</returns>
    public static MonitoringErrorSection Create ( string title, Action<HtmlElement> builder ) {
        return new MonitoringErrorSection ( title, new HtmlElement ( "div", builder ) );
    }

    /// <summary>
    /// Creates a section that displays a table of names and values.
    /// </summary>
    /// <param name="title">The title of the section.</param>
    /// <param name="values">The names and values to display.</param>
    /// <returns>The created section.</returns>
    public static MonitoringErrorSection Create ( string title, IEnumerable<KeyValuePair<string, string?>> values ) {
        return Create ( title, wrapper => {
            wrapper.ClassList.Add ( "http-table-wrapper" );

            wrapper += new HtmlElement ( "table", table => {
                table.ClassList.Add ( "http-table" );

                foreach (var (name, value) in values) {
                    table += new HtmlElement ( "tr", row => {
                        row += new HtmlElement ( "td", name );
                        row += new HtmlElement ( "td", value ?? string.Empty );
                    } );
                }
            } );
        } );
    }
}
