using System.Diagnostics;

namespace Sisk.Monitoring.Tests;

[TestClass]
public sealed class CounterMeterTests {

    static double ReadSummary ( Meter meter, string series ) {
        return meter.Read ( series, TimeSpan.FromHours ( 1 ), TimeSpan.FromHours ( 1 ) ).Single ().Value;
    }

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

    [TestMethod]
    public void Meter_KeepsSeriesApart () {
        var meter = new Meter { Series = [ "2xx", "5xx" ] };

        meter.Increment ( "2xx", 4 );
        meter.Increment ( "2xx" );
        meter.Increment ( "4xx", 2 );

        CollectionAssert.AreEqual ( new [] { "2xx", "5xx", "4xx" }, meter.Series.ToArray () );
        Assert.AreEqual ( 5, ReadSummary ( meter, "2xx" ) );
        Assert.AreEqual ( 0, ReadSummary ( meter, "5xx" ) );
        Assert.AreEqual ( 2, ReadSummary ( meter, "4xx" ) );
        Assert.AreEqual ( 0, meter.Read ().Sum ( reading => reading.Value ), "Named series must not leak into the unnamed one." );

        meter.Reset ();
        Assert.AreEqual ( 0, ReadSummary ( meter, "2xx" ) );
        Assert.AreEqual ( 3, meter.Series.Count, "Reset clears the readings, not the series." );
    }

    [DataTestMethod]
    [DataRow ( MeterAggregation.Sum, 12d, 0d )]
    [DataRow ( MeterAggregation.Average, 4d, double.NaN )]
    [DataRow ( MeterAggregation.Maximum, 7d, double.NaN )]
    [DataRow ( MeterAggregation.Last, 3d, double.NaN )]
    public void Meter_CombinesValuesAccordingToAggregation ( MeterAggregation aggregation, double expected, double expectedWhenEmpty ) {
        var meter = new Meter { Aggregation = aggregation, Series = [ "idle" ] };

        meter.Increment ( "busy", 2 );
        meter.Increment ( "busy", 7 );
        meter.Increment ( "busy", 3 );

        Assert.AreEqual ( expected, ReadSummary ( meter, "busy" ) );
        Assert.AreEqual ( expectedWhenEmpty, ReadSummary ( meter, "idle" ) );

        MeterReading [] readings = meter.Read ( "busy", TimeSpan.FromHours ( 1 ), TimeSpan.FromMinutes ( 1 ) ).ToArray ();
        Assert.AreEqual ( 60, readings.Length );
        Assert.AreEqual ( expected, readings [ ^1 ].Value, "Recent values belong to the newest reading." );
    }

    [TestMethod]
    public void Meter_OffersDefaultPeriodsAndRejectsInvalidOnes () {
        CollectionAssert.AreEqual (
            new [] { TimeSpan.FromHours ( 1 ), TimeSpan.FromDays ( 1 ), TimeSpan.FromDays ( 7 ), TimeSpan.FromDays ( 30 ) },
            new Meter ().Periods.ToArray () );
        CollectionAssert.AreEqual (
            new [] { TimeSpan.FromSeconds ( 2 ) },
            new Meter ( TimeSpan.FromSeconds ( 2 ) ).Periods.ToArray () );

        Assert.ThrowsException<ArgumentException> ( () => new Meter { Periods = [] } );
        Assert.ThrowsException<ArgumentException> ( () => new Meter { Periods = [ TimeSpan.FromHours ( 1 ), TimeSpan.Zero ] } );
    }
}
