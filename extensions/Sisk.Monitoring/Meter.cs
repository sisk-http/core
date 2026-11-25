using System.Globalization;
using System.Text.Json.Serialization;

namespace Sisk.Monitoring;

/// <summary>
/// Provides a sliding-window recorder that aggregates values of one or more series over configurable periods.
/// </summary>
public sealed class Meter {
    static readonly TimeSpan [] DefaultPeriods = [ TimeSpan.FromHours ( 1 ), TimeSpan.FromDays ( 1 ), TimeSpan.FromDays ( 7 ), TimeSpan.FromDays ( 30 ) ];

    private readonly List<SeriesBuckets> _series = new ();
    private readonly object _sync = new ();
    private readonly TimeSpan [] _periods = DefaultPeriods;
    private readonly TimeSpan _slidingWindowDuration;
    private long _prunedThreshold = long.MinValue;
    private long _version;

    /// <summary>
    /// Initializes a new instance of the <see cref="Meter"/> class with the default 1-hour, 1-day, 7-day and 30-day periods.
    /// </summary>
    public Meter () {
        Periods = DefaultPeriods;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Meter"/> class with a single period of the specified duration.
    /// </summary>
    /// <param name="slidingWindowDuration">The length of the sliding window used to retain readings.</param>
    public Meter ( TimeSpan slidingWindowDuration ) {
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( slidingWindowDuration.TotalSeconds, nameof ( slidingWindowDuration ) );
        Periods = [ slidingWindowDuration ];
    }

    /// <summary>
    /// Gets the periods that can be selected when visualizing this meter. The first period is selected by default
    /// and the longest one defines for how long readings are retained.
    /// </summary>
    /// <exception cref="ArgumentException">The list is empty or contains a zero or negative period.</exception>
    public IReadOnlyList<TimeSpan> Periods {
        get => _periods;
        init {
            ArgumentNullException.ThrowIfNull ( value );
            if (value.Count == 0 || value.Any ( period => period <= TimeSpan.Zero ))
                throw new ArgumentException ( SR.Meter_InvalidPeriods, nameof ( value ) );

            _periods = value.ToArray ();
            _slidingWindowDuration = _periods.Max ();
            BucketDuration = _slidingWindowDuration > TimeSpan.FromHours ( 1 ) ? TimeSpan.FromMinutes ( 1 ) : TimeSpan.FromSeconds ( 1 );
        }
    }

    /// <summary>
    /// Gets how the values recorded for a series are combined into a single reading.
    /// </summary>
    public MeterAggregation Aggregation { get; init; }

    /// <summary>
    /// Gets or sets how the vertical axis of this meter is scaled and how its values are formatted.
    /// </summary>
    public MeterAxis Axis { get; set; } = new ();

    /// <summary>
    /// Gets the names of the series measured by this meter, in the order they were declared or first recorded.
    /// Declare them upfront to keep a stable order and to show series that have no values yet.
    /// </summary>
    public IReadOnlyList<string> Series {
        get {
            lock (_sync) {
                return _series.Select ( item => item.Name ).ToArray ();
            }
        }
        init {
            ArgumentNullException.ThrowIfNull ( value );
            foreach (string name in value) {
                GetSeries ( name );
            }
        }
    }

    internal TimeSpan BucketDuration { get; private init; }

    internal long Version {
        get {
            lock (_sync) {
                return _version;
            }
        }
    }

    /// <summary>
    /// Records the value 1 on the unnamed series.
    /// </summary>
    public void Increment () {
        Increment ( string.Empty, 1 );
    }

    /// <summary>
    /// Records the specified value on the unnamed series.
    /// </summary>
    /// <param name="by">The value to record.</param>
    public void Increment ( double by ) {
        Increment ( string.Empty, by );
    }

    /// <summary>
    /// Records the value 1 on the specified series.
    /// </summary>
    /// <param name="series">The name of the series.</param>
    public void Increment ( string series ) {
        Increment ( series, 1 );
    }

    /// <summary>
    /// Records the specified value on the specified series.
    /// </summary>
    /// <param name="series">The name of the series.</param>
    /// <param name="by">The value to record.</param>
    public void Increment ( string series, double by ) {
        ArgumentNullException.ThrowIfNull ( series );

        DateTime now = DateTime.Now;
        long key = ToBucketKey ( now );

        lock (_sync) {
            PruneExpiredBuckets ( now );
            AddToBucket ( GetSeries ( series ).Buckets, key, by, count: 1 );
            _version++;
        }
    }

    /// <summary>
    /// Clears all stored readings.
    /// </summary>
    public void Reset () {
        lock (_sync) {
            foreach (var series in _series) {
                series.Buckets.Clear ();
            }
            _version++;
        }
    }

    internal MeterBucketSnapshot [] ExportState () {
        DateTime now = DateTime.Now;
        long bucketSeconds = (long) BucketDuration.TotalSeconds;

        lock (_sync) {
            PruneExpiredBuckets ( now );
            return _series
                .SelectMany ( series => series.Buckets
                    .Select ( item => new MeterBucketSnapshot ( item.Key * bucketSeconds, item.Value.Value, series.Name, item.Value.Count ) ) )
                .ToArray ();
        }
    }

    internal void ImportState ( MeterBucketSnapshot [] buckets ) {
        long bucketSeconds = (long) BucketDuration.TotalSeconds;
        long threshold = ToBucketKey ( DateTime.Now - _slidingWindowDuration );

        lock (_sync) {
            foreach (var series in _series) {
                series.Buckets.Clear ();
            }

            foreach (var bucket in buckets) {
                long key = bucket.BucketKey / bucketSeconds;
                if (key >= threshold) {
                    AddToBucket ( GetSeries ( bucket.Series ?? string.Empty ).Buckets, key, bucket.Value, Math.Max ( 1, bucket.Count ) );
                }
            }
            _version++;
        }
    }

    /// <summary>
    /// Returns aggregated readings of the unnamed series for the default 1-hour tick within the sliding window.
    /// </summary>
    /// <returns>An enumerable sequence of aggregated <see cref="MeterReading"/> values.</returns>
    public IEnumerable<MeterReading> Read () {
        return Read ( TimeSpan.FromHours ( 1 ) );
    }

    /// <summary>
    /// Returns aggregated readings of the unnamed series for the specified tick duration within the sliding window.
    /// </summary>
    /// <param name="tick">The duration of each aggregation bucket. Must be greater than zero.</param>
    /// <returns>An enumerable sequence of aggregated <see cref="MeterReading"/> values.</returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="tick"/> is zero or negative.</exception>
    public IEnumerable<MeterReading> Read ( TimeSpan tick ) {
        return Read ( string.Empty, _slidingWindowDuration, tick );
    }

    /// <summary>
    /// Returns aggregated readings of a series for the specified tick duration within the most recent period.
    /// </summary>
    /// <param name="series">The name of the series, or an empty string for the unnamed series.</param>
    /// <param name="period">How far back to read. Must be greater than zero.</param>
    /// <param name="tick">The duration of each aggregation bucket. Must be greater than zero.</param>
    /// <returns>
    /// An enumerable sequence of aggregated <see cref="MeterReading"/> values. Unless <see cref="Aggregation"/> is
    /// <see cref="MeterAggregation.Sum"/>, a reading without recorded values is <see cref="double.NaN"/>.
    /// </returns>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="period"/> or <paramref name="tick"/> is zero or negative.</exception>
    public IEnumerable<MeterReading> Read ( string series, TimeSpan period, TimeSpan tick ) {
        ArgumentNullException.ThrowIfNull ( series );
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( period.TotalSeconds, nameof ( period ) );
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero ( tick.TotalSeconds, nameof ( tick ) );

        DateTime now = DateTime.Now;
        long initialTicks = (now - period).Ticks;
        long readingCount = (period.Ticks + tick.Ticks - 1) / tick.Ticks;

        KeyValuePair<long, (double Value, long Count)> [] snapshot;
        lock (_sync) {
            PruneExpiredBuckets ( now );
            snapshot = _series.Find ( item => item.Name == series )?.Buckets.ToArray () ?? [];
        }

        Array.Sort ( snapshot, ( left, right ) => left.Key.CompareTo ( right.Key ) );

        int position = 0;
        while (position < snapshot.Length && snapshot [ position ].Key * BucketDuration.Ticks < initialTicks) {
            position++;
        }

        for (long index = 0; index < readingCount; index++) {
            long endTicks = initialTicks + (tick.Ticks * (index + 1));
            bool isLastReading = index == readingCount - 1;

            double value = 0;
            long count = 0;
            while (position < snapshot.Length && (isLastReading || snapshot [ position ].Key * BucketDuration.Ticks < endTicks)) {
                var bucket = snapshot [ position ].Value;
                value = count == 0 ? bucket.Value : Combine ( value, bucket.Value );
                count += bucket.Count;
                position++;
            }

            double reading = Aggregation switch {
                MeterAggregation.Sum => value,
                _ when count == 0 => double.NaN,
                MeterAggregation.Average => value / count,
                _ => value
            };

            yield return new MeterReading ( reading, new DateTime ( endTicks, DateTimeKind.Local ) );
        }
    }

    SeriesBuckets GetSeries ( string name ) {
        foreach (var item in _series) {
            if (item.Name == name)
                return item;
        }

        var series = new SeriesBuckets ( name );
        _series.Add ( series );
        return series;
    }

    void AddToBucket ( Dictionary<long, (double Value, long Count)> buckets, long key, double value, long count ) {
        buckets [ key ] = buckets.TryGetValue ( key, out var existing )
            ? (Combine ( existing.Value, value ), existing.Count + count)
            : (value, count);
    }

    double Combine ( double current, double next ) {
        return Aggregation switch {
            MeterAggregation.Maximum => Math.Max ( current, next ),
            MeterAggregation.Last => next,
            _ => current + next
        };
    }

    void PruneExpiredBuckets ( DateTime now ) {
        long threshold = ToBucketKey ( now - _slidingWindowDuration );
        if (threshold == _prunedThreshold) {
            return;
        }

        _prunedThreshold = threshold;
        foreach (var series in _series) {
            foreach (long key in series.Buckets.Keys) {
                if (key < threshold) {
                    series.Buckets.Remove ( key );
                }
            }
        }
    }

    long ToBucketKey ( DateTime timestamp ) {
        return timestamp.Ticks / BucketDuration.Ticks;
    }

    sealed class SeriesBuckets ( string name ) {
        public readonly string Name = name;
        public readonly Dictionary<long, (double Value, long Count)> Buckets = new ();
    }
}

/// <summary>
/// Represents a single measurement value recorded at a specific point in time.
/// </summary>
/// <param name="Value">The measurement value.</param>
/// <param name="Timestamp">The UTC date and time when the reading was recorded.</param>
public record struct MeterReading ( double Value, DateTime Timestamp );

/// <summary>
/// Specifies how the values recorded by a <see cref="Meter"/> are combined into a single reading.
/// </summary>
public enum MeterAggregation {
    /// <summary>
    /// Adds the values, as when counting events or accumulating amounts.
    /// </summary>
    Sum,

    /// <summary>
    /// Averages the values, as when measuring durations.
    /// </summary>
    Average,

    /// <summary>
    /// Keeps the highest value.
    /// </summary>
    Maximum,

    /// <summary>
    /// Keeps the most recent value, as when sampling a level such as memory usage.
    /// </summary>
    Last
}

/// <summary>
/// Describes the scale of the vertical axis of a <see cref="Meter"/> and how its values are formatted.
/// </summary>
public sealed class MeterAxis {
    /// <summary>
    /// Gets or sets the lowest value of the axis. When <see langword="null"/>, the axis starts at zero or at the
    /// lowest negative value.
    /// </summary>
    public double? Minimum { get; set; }

    /// <summary>
    /// Gets or sets the highest value of the axis. When <see langword="null"/>, the axis fits the highest value.
    /// </summary>
    public double? Maximum { get; set; }

    /// <summary>
    /// Gets or sets the function that formats the values shown in the dashboard, such as <c>value => $"{value:N0} ms"</c>.
    /// By default, values are formatted as numbers with thousands separators and up to two decimal places.
    /// </summary>
    public Func<double, string> Format { get; set; } = value => value.ToString ( "#,0.##", CultureInfo.InvariantCulture );
}

internal sealed record MeterPayload (
    string Id,
    string Label,
    string? Group,
    bool DashboardPinned,
    MeterAggregation Aggregation,
    string Period,
    MeterAxisPayload Axis,
    MeterSeriesPayload [] Series,
    MeterReadingPayload [] Readings );

internal sealed record MeterAxisPayload ( double Minimum, double Maximum, string [] Ticks );

internal sealed record MeterSeriesPayload ( string Name, double? Summary, string? SummaryText );

internal sealed record MeterReadingPayload ( DateTime Timestamp, double? [] Values, string? [] Texts );

[JsonSourceGenerationOptions ( PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase, UseStringEnumConverter = true )]
[JsonSerializable ( typeof ( MeterPayload [] ) )]
internal sealed partial class MeterJsonContext : JsonSerializerContext {
}
