namespace Sisk.Monitoring;

/// <summary>
/// Provides a sliding-window counter that aggregates <see cref="MeterReading"/> values over a configurable time span.
/// </summary>
public sealed class Meter {
    static readonly TimeSpan BucketDuration = TimeSpan.FromSeconds ( 1 );

    private readonly TimeSpan _slidingWindowDuration;
    private readonly Dictionary<long, double> _buckets = new ();
    private readonly object _sync = new ();

    /// <summary>
    /// Initializes a new instance of the <see cref="Meter"/> class with a default 24-hour sliding window.
    /// </summary>
    public Meter () : this ( TimeSpan.FromHours ( 24 ) ) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Meter"/> class with the specified sliding-window duration.
    /// </summary>
    /// <param name="slidingWindowDuration">The length of the sliding window used to retain readings.</param>
    public Meter ( TimeSpan slidingWindowDuration ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( slidingWindowDuration.TotalSeconds, nameof ( slidingWindowDuration ) );
        _slidingWindowDuration = slidingWindowDuration;
    }

    /// <summary>
    /// Increments the current reading by the specified value.
    /// </summary>
    /// <param name="by">The amount to increment.</param>
    public void Increment ( double by ) {
        DateTime now = DateTime.Now;
        long key = ToBucketKey ( now );

        lock (_sync) {
            PruneExpiredBuckets ( now );
            _buckets.TryGetValue ( key, out double existing );
            _buckets[key] = existing + by;
        }
    }

    /// <summary>
    /// Increments the current reading by 1.
    /// </summary>
    public void Increment () {
        Increment ( 1 );
    }

    /// <summary>
    /// Clears all stored readings.
    /// </summary>
    public void Reset () {
        lock (_sync) {
            _buckets.Clear ();
        }
    }

    internal MeterBucketSnapshot [] ExportState () {
        DateTime now = DateTime.Now;

        lock (_sync) {
            PruneExpiredBuckets ( now );
            return _buckets
                .Select ( item => new MeterBucketSnapshot ( item.Key, item.Value ) )
                .ToArray ();
        }
    }

    internal void ImportState ( MeterBucketSnapshot [] buckets ) {
        DateTime now = DateTime.Now;
        long threshold = ToBucketKey ( now - _slidingWindowDuration );

        lock (_sync) {
            _buckets.Clear ();
            foreach (var bucket in buckets.Where ( item => item.BucketKey >= threshold )) {
                _buckets [ bucket.BucketKey ] = bucket.Value;
            }
        }
    }

    /// <summary>
    /// Returns aggregated readings for the default 1-hour tick within the sliding window.
    /// </summary>
    /// <returns>An enumerable sequence of aggregated <see cref="MeterReading"/> values.</returns>
    public IEnumerable<MeterReading> Read () {
        return Read ( TimeSpan.FromHours ( 1 ) );
    }

    /// <summary>
    /// Returns aggregated readings for the specified tick duration within the sliding window.
    /// </summary>
    /// <param name="tick">The duration of each aggregation bucket. Must be greater than zero.</param>
    /// <returns>An enumerable sequence of aggregated <see cref="MeterReading"/> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tick"/> is zero or negative.</exception>
    public IEnumerable<MeterReading> Read ( TimeSpan tick ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( tick.TotalSeconds, nameof ( tick ) );

        DateTime now = DateTime.Now;
        DateTime initial = now - _slidingWindowDuration;

        Dictionary<long, double> snapshot;
        lock (_sync) {
            PruneExpiredBuckets ( now );
            snapshot = new ( _buckets );
        }

        DateTime cursor = initial;
        while (cursor < now) {
            DateTime slidingStart = cursor;
            DateTime slidingEnd = cursor + tick;

            long startBucket = ToBucketKey ( slidingStart );
            long endBucket = ToBucketKey ( slidingEnd.AddTicks ( -1 ) );

            double sum = 0;
            for (long bucket = startBucket; bucket <= endBucket; bucket++) {
                snapshot.TryGetValue ( bucket, out double value );
                sum += value;
            }

            cursor += tick;
            yield return new MeterReading ( sum, slidingEnd );
        }
    }

    void PruneExpiredBuckets ( DateTime now ) {
        long threshold = ToBucketKey ( now - _slidingWindowDuration );

        foreach (long key in _buckets.Keys.Where ( k => k < threshold ).ToArray ()) {
            _buckets.Remove ( key );
        }
    }

    static long ToBucketKey ( DateTime timestamp ) {
        return timestamp.Ticks / BucketDuration.Ticks;
    }
}

/// <summary>
/// Represents a single measurement value recorded at a specific point in time.
/// </summary>
/// <param name="Value">The measurement value.</param>
/// <param name="Timestamp">The UTC date and time when the reading was recorded.</param>
public record struct MeterReading ( double Value, DateTime Timestamp );
