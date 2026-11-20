using System.Runtime.InteropServices;

namespace Sisk.Monitoring;

/// <summary>
/// Represents a thread-safe counter that automatically expires values after a specified duration.
/// </summary>
public sealed class Counter {

    const long ExpirationPrecisionDivisor = 1000;

    private readonly Dictionary<long, double> _buckets = new ();
    private readonly PriorityQueue<long, long> _expirations = new ();
    private readonly object _sync = new ();
    private long _version;

    /// <summary>
    /// Initializes a new instance of the <see cref="Counter"/> class with default settings.
    /// </summary>
    public Counter () {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="Counter"/> class with an initial value and duration.
    /// </summary>
    /// <param name="initialValue">The initial value of the counter.</param>
    /// <param name="initialDuration">The duration for which the initial value remains valid.</param>
    public Counter ( double initialValue, TimeSpan initialDuration ) {
        if (initialValue != 0) {
            Increment ( initialValue, initialDuration );
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
                return _buckets.Values.Sum ();
            }
        }
    }

    internal long Version {
        get {
            lock (_sync) {
                return _version;
            }
        }
    }

    /// <summary>
    /// Increments the counter by the specified value using the specified duration.
    /// </summary>
    /// <param name="by">The amount to increment the counter.</param>
    /// <param name="duration">The duration for which this increment remains valid.</param>
    /// <remarks>
    /// Increments are grouped by expiration time with a precision of one thousandth of <paramref name="duration"/>,
    /// so an increment may remain valid slightly longer than requested, but never shorter.
    /// </remarks>
    public void Increment ( double by, TimeSpan duration ) {
        long precision = Math.Max ( 1, duration.Ticks / ExpirationPrecisionDivisor );
        long expiresAt = (DateTime.Now + duration).Ticks;
        long remainder = expiresAt % precision;
        if (remainder != 0)
            expiresAt += precision - remainder;

        lock (_sync) {
            PruneExpiredEntries ();
            AddToBucket ( expiresAt, by );
            _version++;
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
            _buckets.Clear ();
            _expirations.Clear ();
            _version++;
        }
    }

    internal CounterIncrementSnapshot [] ExportState () {
        lock (_sync) {
            PruneExpiredEntries ();
            return _buckets
                .Select ( bucket => new CounterIncrementSnapshot ( bucket.Value, new DateTime ( bucket.Key, DateTimeKind.Local ) ) )
                .ToArray ();
        }
    }

    internal void ImportState ( CounterIncrementSnapshot [] increments ) {
        long now = DateTime.Now.Ticks;

        lock (_sync) {
            _buckets.Clear ();
            _expirations.Clear ();

            foreach (var increment in increments) {
                if (increment.ExpiresAt.Ticks > now)
                    AddToBucket ( increment.ExpiresAt.Ticks, increment.Value );
            }

            _version++;
        }
    }

    void AddToBucket ( long expiresAt, double value ) {
        ref double bucket = ref CollectionsMarshal.GetValueRefOrAddDefault ( _buckets, expiresAt, out bool exists );
        bucket += value;

        if (!exists)
            _expirations.Enqueue ( expiresAt, expiresAt );
    }

    void PruneExpiredEntries () {
        long now = DateTime.Now.Ticks;

        while (_expirations.TryPeek ( out long expiresAt, out _ ) && expiresAt <= now) {
            _expirations.Dequeue ();
            _buckets.Remove ( expiresAt );
        }
    }
}
