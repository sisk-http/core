using System.Text.Json;
using System.Text.Json.Serialization;
using Sisk.Core.Helpers;

namespace Sisk.Monitoring;

/// <summary>
/// Persists and restores monitoring state snapshots.
/// </summary>
public interface IMonitoringStore {

    /// <summary>
    /// Saves the specified monitoring snapshot.
    /// </summary>
    /// <param name="snapshot">The snapshot to persist.</param>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    Task SaveAsync ( MonitoringSnapshot snapshot, CancellationToken cancellationToken = default );

    /// <summary>
    /// Loads the latest persisted monitoring snapshot, if one exists.
    /// </summary>
    /// <param name="cancellationToken">The cancellation token for the operation.</param>
    /// <returns>The loaded snapshot, or <see langword="null"/> when no snapshot is available.</returns>
    /// <remarks>
    /// Throw when an existing snapshot is temporarily unreadable. The monitor then skips saving for the current run,
    /// so the stored state is not replaced by an empty one. Return <see langword="null"/> when the snapshot is missing
    /// or permanently invalid, which lets the monitor replace it.
    /// </remarks>
    Task<MonitoringSnapshot?> LoadAsync ( CancellationToken cancellationToken = default );
}

/// <summary>
/// Persists monitoring snapshots to a JSON file.
/// </summary>
public sealed class FileMonitoringStore : IMonitoringStore {
    /// <summary>
    /// Initializes a new instance of the <see cref="FileMonitoringStore"/> class.
    /// </summary>
    /// <param name="filePath">The JSON file path used to persist monitoring state.</param>
    public FileMonitoringStore ( string filePath ) {
        ArgumentException.ThrowIfNullOrWhiteSpace ( filePath );
        FilePath = Path.GetFullPath ( filePath );
    }

    /// <summary>
    /// Gets the JSON file path used to persist monitoring state.
    /// </summary>
    public string FilePath { get; }

    /// <inheritdoc/>
    public async Task SaveAsync ( MonitoringSnapshot snapshot, CancellationToken cancellationToken = default ) {
        PathHelper.EnsureFileDirectoryExistance ( FilePath );

        string temporaryFile = $"{FilePath}.{Guid.NewGuid ():N}.tmp";
        try {
            await using (var stream = File.Create ( temporaryFile )) {
                await JsonSerializer.SerializeAsync ( stream, snapshot, MonitoringStoreJsonContext.Default.MonitoringSnapshot, cancellationToken ).ConfigureAwait ( false );
            }

            File.Move ( temporaryFile, FilePath, overwrite: true );
        }
        catch {
            try {
                File.Delete ( temporaryFile );
            }
            catch (IOException) {
            }
            catch (UnauthorizedAccessException) {
            }
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task<MonitoringSnapshot?> LoadAsync ( CancellationToken cancellationToken = default ) {
        if (!File.Exists ( FilePath ))
            return null;

        try {
            await using var stream = File.OpenRead ( FilePath );
            return await JsonSerializer.DeserializeAsync ( stream, MonitoringStoreJsonContext.Default.MonitoringSnapshot, cancellationToken ).ConfigureAwait ( false );
        }
        catch (FileNotFoundException) {
            return null;
        }
        catch (JsonException) {
            return null;
        }
    }
}

[JsonSourceGenerationOptions ( WriteIndented = false )]
[JsonSerializable ( typeof ( MonitoringSnapshot ) )]
internal sealed partial class MonitoringStoreJsonContext : JsonSerializerContext {
}

/// <summary>
/// Represents a persisted snapshot of the monitoring dashboard state.
/// </summary>
/// <param name="SavedAt">The local date and time when the snapshot was created.</param>
/// <param name="Counters">The persisted counter states.</param>
/// <param name="Meters">The persisted meter states.</param>
/// <param name="Health">The persisted server health history.</param>
/// <param name="Issues">The persisted issues of the error console.</param>
public sealed record MonitoringSnapshot (
    DateTime SavedAt,
    CounterSnapshot [] Counters,
    MeterSnapshot [] Meters,
    MonitoringHealthSnapshot [] Health,
    MonitoringIssueSnapshot []? Issues = null );

/// <summary>
/// Represents a persisted issue, which groups the incidents of an exception type raised at a location.
/// </summary>
/// <param name="Id">The identifier derived from the exception type and the location.</param>
/// <param name="Type">The full name of the exception type.</param>
/// <param name="Location">Where the exception was raised.</param>
/// <param name="FirstSeen">The local date and time of the first incident.</param>
/// <param name="LastSeen">The local date and time of the most recent incident.</param>
/// <param name="Count">The number of incidents.</param>
/// <param name="Resolved">Whether the issue was resolved by the user or archived automatically.</param>
/// <param name="Trend">The number of incidents per hour over the last 24 hours.</param>
/// <param name="Occurrences">The most recent incidents, newest first.</param>
public sealed record MonitoringIssueSnapshot (
    string Id,
    string Type,
    string Location,
    DateTime FirstSeen,
    DateTime LastSeen,
    long Count,
    bool Resolved,
    MonitoringIssueTrendSnapshot [] Trend,
    MonitoringIssueOccurrenceSnapshot [] Occurrences );

/// <summary>
/// Represents the number of incidents of an issue within one hour.
/// </summary>
/// <param name="Hour">The local date and time when the hour starts.</param>
/// <param name="Count">The number of incidents.</param>
public sealed record MonitoringIssueTrendSnapshot ( DateTime Hour, long Count );

/// <summary>
/// Represents a persisted incident of an issue.
/// </summary>
/// <param name="Timestamp">The local date and time of the incident.</param>
/// <param name="Message">The exception message.</param>
/// <param name="Details">The exception text, including inner exceptions and stack traces.</param>
/// <param name="Request">The HTTP method and route of the related request, if any.</param>
/// <param name="Sections">The custom sections recorded with the incident.</param>
public sealed record MonitoringIssueOccurrenceSnapshot (
    DateTime Timestamp,
    string Message,
    string Details,
    string? Request,
    MonitoringIssueSectionSnapshot [] Sections );

/// <summary>
/// Represents a persisted custom section of an incident.
/// </summary>
/// <param name="Title">The title of the section.</param>
/// <param name="Html">The rendered HTML of the section.</param>
public sealed record MonitoringIssueSectionSnapshot ( string Title, string Html );

/// <summary>
/// Represents a persisted counter state.
/// </summary>
/// <param name="Key">The resource key.</param>
/// <param name="DefaultDuration">The counter default increment duration.</param>
/// <param name="Increments">The counter increments that had not expired when saved.</param>
public sealed record CounterSnapshot ( string Key, TimeSpan DefaultDuration, CounterIncrementSnapshot [] Increments );

/// <summary>
/// Represents a persisted counter increment.
/// </summary>
/// <param name="Value">The increment value.</param>
/// <param name="ExpiresAt">The local date and time when the increment expires.</param>
public sealed record CounterIncrementSnapshot ( double Value, DateTime ExpiresAt );

/// <summary>
/// Represents a persisted meter state.
/// </summary>
/// <param name="Key">The resource key.</param>
/// <param name="Buckets">The meter buckets inside the meter's sliding window.</param>
public sealed record MeterSnapshot ( string Key, MeterBucketSnapshot [] Buckets );

/// <summary>
/// Represents a persisted meter bucket.
/// </summary>
/// <param name="BucketKey">The bucket start, in seconds elapsed since <see cref="DateTime.MinValue"/> in local time.</param>
/// <param name="Value">The aggregated bucket value.</param>
/// <param name="Series">The series the bucket belongs to, or <see langword="null"/> for the unnamed series.</param>
/// <param name="Count">The number of values aggregated into the bucket.</param>
public sealed record MeterBucketSnapshot ( long BucketKey, double Value, string? Series = null, long Count = 1 );

/// <summary>
/// Represents a persisted server health reading.
/// </summary>
/// <param name="Timestamp">The local date and time when the reading was collected.</param>
/// <param name="CpuPercent">The process CPU usage percentage.</param>
/// <param name="DiskPercent">The current drive used-space percentage.</param>
/// <param name="MemoryPercent">The process memory usage percentage against the available GC memory.</param>
public sealed record MonitoringHealthSnapshot ( DateTime Timestamp, double CpuPercent, double DiskPercent, double MemoryPercent );
