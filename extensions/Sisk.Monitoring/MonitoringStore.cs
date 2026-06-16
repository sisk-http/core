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

        string temporaryFile = FilePath + $".tmp_{DateTimeOffset.UtcNow.ToUnixTimeMilliseconds ()}";
        await using (var stream = File.Create ( temporaryFile )) {
            await JsonSerializer.SerializeAsync ( stream, snapshot, MonitoringStoreJsonContext.Default.MonitoringSnapshot, cancellationToken ).ConfigureAwait ( false );
        }

        if (File.Exists ( FilePath )) {
            File.Delete ( FilePath );
        }

        File.Move ( temporaryFile, FilePath );
    }

    /// <inheritdoc/>
    public async Task<MonitoringSnapshot?> LoadAsync ( CancellationToken cancellationToken = default ) {
        if (!File.Exists ( FilePath ))
            return null;

        try {
            await using var stream = File.OpenRead ( FilePath );
            return await JsonSerializer.DeserializeAsync ( stream, MonitoringStoreJsonContext.Default.MonitoringSnapshot, cancellationToken ).ConfigureAwait ( false );
        }
        catch (IOException) {
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
public sealed record MonitoringSnapshot (
    DateTime SavedAt,
    CounterSnapshot [] Counters,
    MeterSnapshot [] Meters,
    MonitoringHealthSnapshot [] Health );

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
/// <param name="BucketKey">The internal bucket timestamp key.</param>
/// <param name="Value">The aggregated bucket value.</param>
public sealed record MeterBucketSnapshot ( long BucketKey, double Value );

/// <summary>
/// Represents a persisted server health reading.
/// </summary>
/// <param name="Timestamp">The local date and time when the reading was collected.</param>
/// <param name="CpuPercent">The process CPU usage percentage.</param>
/// <param name="DiskPercent">The current drive used-space percentage.</param>
/// <param name="MemoryPercent">The process memory usage percentage against the available GC memory.</param>
public sealed record MonitoringHealthSnapshot ( DateTime Timestamp, double CpuPercent, double DiskPercent, double MemoryPercent );
