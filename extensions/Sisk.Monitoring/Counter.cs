namespace Sisk.Monitoring;

/// <summary>
/// Represents a thread-safe counter that automatically expires values after a specified duration.
/// </summary>
public sealed class Counter {

    private readonly List<CounterEntry> _counts;
    private readonly object _sync = new ();

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
            _counts = new () { new CounterEntry ( initialValue, DateTime.Now + initialDuration ) };
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
    public double Current {
        get {
            lock (_sync) {
                PruneExpiredEntries ();
                return _counts.Sum ( c => c.Value );
            }
        }
    }

    /// <summary>
    /// Increments the counter by the specified value using the specified duration.
    /// </summary>
    /// <param name="by">The amount to increment the counter.</param>
    /// <param name="duration">The duration for which this increment remains valid.</param>
    public void Increment ( double by, TimeSpan duration ) {
        lock (_sync) {
            PruneExpiredEntries ();
            _counts.Add ( new CounterEntry ( by, DateTime.Now + duration ) );
        }
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
        lock (_sync) {
            _counts.Clear ();
        }
    }

    internal CounterIncrementSnapshot [] ExportState () {
        lock (_sync) {
            PruneExpiredEntries ();
            return _counts
                .Select ( entry => new CounterIncrementSnapshot ( entry.Value, entry.ExpiresAt ) )
                .ToArray ();
        }
    }

    internal void ImportState ( CounterIncrementSnapshot [] increments ) {
        DateTime now = DateTime.Now;

        lock (_sync) {
            _counts.Clear ();
            _counts.AddRange ( increments
                .Where ( increment => increment.ExpiresAt > now )
                .Select ( increment => new CounterEntry ( increment.Value, increment.ExpiresAt ) ) );
        }
    }

    void PruneExpiredEntries () {
        DateTime now = DateTime.Now;
        _counts.RemoveAll ( entry => entry.ExpiresAt <= now );
    }

    record struct CounterEntry ( double Value, DateTime ExpiresAt );
}
