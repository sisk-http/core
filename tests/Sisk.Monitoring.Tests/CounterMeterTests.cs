using System.Diagnostics;

namespace Sisk.Monitoring.Tests;

[TestClass]
public sealed class CounterMeterTests {

    [TestMethod]
    public void Counter_SumsLiveIncrementsAndDropsExpiredOnes () {
        var counter = new Counter ();

        counter.Increment ( 5, TimeSpan.FromMilliseconds ( 50 ) );
        counter.Increment ( 3, TimeSpan.FromHours ( 1 ) );
        counter.Increment ( 2, TimeSpan.FromHours ( 2 ) );
        Assert.AreEqual ( 10, counter.Current );

        Thread.Sleep ( 150 );
        Assert.AreEqual ( 5, counter.Current );

        counter.Reset ();
        Assert.AreEqual ( 0, counter.Current );
    }

    [TestMethod]
    public void Counter_ExpiresIncrementsOutOfInsertionOrder () {
        var counter = new Counter ();

        counter.Increment ( 1, TimeSpan.FromHours ( 1 ) );
        counter.Increment ( 10, TimeSpan.FromMilliseconds ( 50 ) );
        counter.Increment ( 100, TimeSpan.FromHours ( 1 ) );

        Thread.Sleep ( 150 );
        Assert.AreEqual ( 101, counter.Current );
    }

    [TestMethod]
    public void Counter_IncrementCostDoesNotGrowWithLiveEntries () {
        const int increments = 100_000;
        var counter = new Counter ();

        var stopwatch = Stopwatch.StartNew ();
        for (int i = 0; i < increments; i++) {
            counter.Increment ();
        }
        stopwatch.Stop ();

        Assert.AreEqual ( increments, counter.Current );
        Assert.IsTrue (
            stopwatch.Elapsed < TimeSpan.FromSeconds ( 1 ),
            $"{increments} increments took {stopwatch.Elapsed.TotalMilliseconds:N0} ms; per-increment cost must not grow with the number of live entries." );
    }

    [TestMethod]
    public void Meter_SumsRecentIncrementsAndDropsExpiredBuckets () {
        var meter = new Meter ( TimeSpan.FromSeconds ( 2 ) );

        meter.Increment ( 4 );
        meter.Increment ();
        Assert.AreEqual ( 5, meter.Read ( TimeSpan.FromSeconds ( 2 ) ).Sum ( reading => reading.Value ) );

        Thread.Sleep ( 3200 );
        Assert.AreEqual ( 0, meter.Read ( TimeSpan.FromSeconds ( 2 ) ).Sum ( reading => reading.Value ) );

        meter.Increment ( 7 );
        Assert.AreEqual ( 7, meter.Read ( TimeSpan.FromSeconds ( 2 ) ).Sum ( reading => reading.Value ) );
    }
}
