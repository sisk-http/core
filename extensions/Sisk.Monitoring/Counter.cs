using CacheStorage;

namespace Sisk.Monitoring;

/// <summary>
/// Represents a thread-safe counter that automatically expires values after a specified duration.
/// </summary>
public sealed class Counter {

    private List<CachedObject<double>> _counts;

    /// <summary>
    /// Initializes a new instance of the <see cref="Counter"/> class with default settings.
    /// </summary>
    public Counter () {
        _counts = new ();
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Counter"/> class with an initial value and duration.
    /// </summary>
    /// <param name="initialValue">The initial value of the counter.</param>
    /// <param name="initialDuration">The duration for which the initial value remains valid.</param>
    public Counter ( double initialValue, TimeSpan initialDuration ) {
        if (initialValue == 0) {
            _counts = new ();
        }
        else {
            _counts = new () { new CachedObject<double> ( initialValue, initialDuration ) };
        }
        DefaultDuration = initialDuration;
    }

    /// <summary>
    /// Gets or sets the default duration for which counter increments remain valid.
    /// </summary>
    /// <value>The default expiration duration. The default is one hour.</value>
    public TimeSpan DefaultDuration { get; set; } = TimeSpan.FromHours ( 1 );

    /// <summary>
    /// Gets the current sum of all non-expired counter increments.
    /// </summary>
    /// <value>The total count of all valid increments.</value>
    public double Current => _counts.Sum ( c => c.Value );

    /// <summary>
    /// Increments the counter by the specified value using the specified duration.
    /// </summary>
    /// <param name="by">The amount to increment the counter.</param>
    /// <param name="duration">The duration for which this increment remains valid.</param>
    public void Increment ( double by, TimeSpan duration ) {
        var count = new CachedObject<double> ( by, duration );
        _counts.Add ( count );
    }

    /// <summary>
    /// Increments the counter by the specified value using the default duration.
    /// </summary>
    /// <param name="by">The amount to increment the counter.</param>
    public void Increment ( double by ) {
        Increment ( by, DefaultDuration );
    }

    /// <summary>
    /// Increments the counter by one using the default duration.
    /// </summary>
    public void Increment () {
        Increment ( 1 );
    }

    /// <summary>
    /// Resets the counter by clearing all increments.
    /// </summary>
    public void Reset () {
        _counts.Clear ();
    }
}