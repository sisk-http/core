using CacheStorage;

namespace Sisk.Monitoring;

/// <summary>
/// Provides a sliding-window counter that aggregates <see cref="MeterReading"/> values over a configurable time span.
/// </summary>
public sealed class Meter {
    private TimeSpan _slidingWindowDuration;
    MemoryCacheList<MeterReading> _readings;

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
        _slidingWindowDuration = slidingWindowDuration;
        _readings = CachePoolingContext.Shared.Collect ( new MemoryCacheList<MeterReading> () { DefaultExpiration = slidingWindowDuration } );
    }

    /// <summary>
    /// Increments the current reading by the specified value.
    /// </summary>
    /// <param name="by">The amount to increment.</param>
    public void Increment ( double by ) {
        var reading = new MeterReading ( by, DateTime.Now );
        _readings.Add ( reading );
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
        _readings.Clear ();
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
        DateTime target = now;

        DateTime cursor = initial;
        while (cursor < target) {
            DateTime slidingStart = cursor;
            DateTime slidingEnd = cursor + tick;

            double sum = _readings
                .Where ( r => r.Timestamp >= slidingStart && r.Timestamp < slidingEnd )
                .Sum ( r => r.Value );

            cursor += tick;
            yield return new MeterReading ( sum, slidingEnd );
        }
    }
}

/// <summary>
/// Represents a single measurement value recorded at a specific point in time.
/// </summary>
/// <param name="Value">The measurement value.</param>
/// <param name="Timestamp">The UTC date and time when the reading was recorded.</param>
public record struct MeterReading ( double Value, DateTime Timestamp );