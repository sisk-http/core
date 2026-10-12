using System.Runtime.InteropServices;
using System.Text.Json.Serialization;

namespace Sisk.Monitoring;

internal sealed class HttpMetrics {

    const int MaxTrackedRoutes = 200;
    const int MaxRecentExceptions = 20;
    const string OtherRoutesKey = "(other routes)";

    static readonly TimeSpan Retention = TimeSpan.FromDays ( 7 );
    static readonly TimeSpan GlobalBucketDuration = TimeSpan.FromMinutes ( 1 );
    static readonly TimeSpan RouteBucketDuration = TimeSpan.FromMinutes ( 15 );
    static readonly HttpTotalsPayload EmptyTotals = new ( 0, 0, 0, 0, 0 );

    readonly object sync = new ();
    readonly DateTime startedAt = DateTime.Now;
    readonly Dictionary<long, HttpMetricsBucket> globalBuckets = new ();
    readonly Dictionary<string, Dictionary<long, HttpMetricsBucket>> routeBuckets = new ( StringComparer.Ordinal );
    readonly Queue<HttpExceptionPayload> recentExceptions = new ();
    long lastPruneMinute;

    public void Record ( string routeKey, int statusCode, double elapsedMs, Exception? exception ) {
        DateTime now = DateTime.Now;
        long minute = now.Ticks / GlobalBucketDuration.Ticks;

        lock (sync) {
            if (minute != lastPruneMinute) {
                lastPruneMinute = minute;
                long thresholdTicks = (now - Retention).Ticks;

                foreach (long key in globalBuckets.Keys) {
                    if (key * GlobalBucketDuration.Ticks < thresholdTicks)
                        globalBuckets.Remove ( key );
                }

                foreach (var (route, expiringBuckets) in routeBuckets) {
                    foreach (long key in expiringBuckets.Keys) {
                        if (key * RouteBucketDuration.Ticks < thresholdTicks)
                            expiringBuckets.Remove ( key );
                    }

                    if (expiringBuckets.Count == 0)
                        routeBuckets.Remove ( route );
                }
            }

            ref var globalBucket = ref CollectionsMarshal.GetValueRefOrAddDefault ( globalBuckets, minute, out _ );
            (globalBucket ??= new ()).Add ( statusCode, elapsedMs );

            if (routeBuckets.Count >= MaxTrackedRoutes && !routeBuckets.ContainsKey ( routeKey ))
                routeKey = OtherRoutesKey;

            ref var buckets = ref CollectionsMarshal.GetValueRefOrAddDefault ( routeBuckets, routeKey, out _ );
            buckets ??= new ();

            ref var routeBucket = ref CollectionsMarshal.GetValueRefOrAddDefault ( buckets, now.Ticks / RouteBucketDuration.Ticks, out _ );
            (routeBucket ??= new ()).Add ( statusCode, elapsedMs );

            if (exception is not null) {
                recentExceptions.Enqueue ( new HttpExceptionPayload (
                    now,
                    routeKey,
                    exception.GetType ().FullName ?? exception.GetType ().Name,
                    exception.Message ) );

                if (recentExceptions.Count > MaxRecentExceptions)
                    recentExceptions.Dequeue ();
            }
        }
    }

    public HttpMetricsPayload Read ( TimeSpan period, TimeSpan step, long inFlight ) {
        DateTime now = DateTime.Now;
        DateTime start = now - period > startedAt ? now - period : startedAt;
        long firstGlobalKey = start.Ticks / GlobalBucketDuration.Ticks;
        long firstRouteKey = start.Ticks / RouteBucketDuration.Ticks;

        var total = new HttpMetricsBucket ();
        var steps = new Dictionary<long, HttpMetricsBucket> ();
        HttpRoutePayload [] routes;
        HttpExceptionPayload [] exceptions;

        lock (sync) {
            foreach (var (key, bucket) in globalBuckets) {
                if (key < firstGlobalKey)
                    continue;

                total.Merge ( bucket );

                ref var stepBucket = ref CollectionsMarshal.GetValueRefOrAddDefault ( steps, key * GlobalBucketDuration.Ticks / step.Ticks, out _ );
                (stepBucket ??= new ()).Merge ( bucket );
            }

            routes = routeBuckets
                .Select ( route => (
                    Route: route.Key,
                    Bucket: route.Value
                        .Where ( item => item.Key >= firstRouteKey )
                        .Aggregate ( new HttpMetricsBucket (), ( merged, item ) => merged.Merge ( item.Value ) ) ) )
                .Where ( route => route.Bucket.Requests > 0 )
                .Select ( route => new HttpRoutePayload ( route.Route, route.Bucket.ToTotals (), route.Bucket.ToLatency () ) )
                .ToArray ();

            exceptions = recentExceptions.Reverse ().ToArray ();
        }

        var readings = new List<HttpReadingPayload> ();
        for (long key = start.Ticks / step.Ticks; key <= now.Ticks / step.Ticks; key++) {
            steps.TryGetValue ( key, out var bucket );
            readings.Add ( new HttpReadingPayload (
                new DateTime ( key * step.Ticks, DateTimeKind.Local ),
                bucket?.ToTotals () ?? EmptyTotals,
                bucket?.ToLatency () ) );
        }

        HttpTotalsPayload totals = total.ToTotals ();
        double elapsedMinutes = Math.Max ( 1, (now - start).TotalMinutes );

        return new HttpMetricsPayload (
            inFlight,
            Math.Round ( totals.Requests / elapsedMinutes, 3 ),
            totals.Requests > 0 ? Math.Round ( totals.ServerError * 100.0 / totals.Requests, 3 ) : 0,
            totals,
            total.ToLatency (),
            readings.ToArray (),
            routes,
            exceptions );
    }
}

internal sealed class HttpMetricsBucket {

    const double MinBoundMs = 0.5;
    const double Growth = 1.5;
    const int BinCount = 32;

    static readonly double LogGrowth = Math.Log ( Growth );

    readonly long [] histogram = new long [ BinCount ];
    long success;
    long redirect;
    long clientError;
    long serverError;
    double totalMs;
    double maxMs;

    public long Requests { get; private set; }

    public void Add ( int statusCode, double elapsedMs ) {
        Requests++;

        switch (statusCode) {
            case >= 500:
                serverError++;
                break;
            case >= 400:
                clientError++;
                break;
            case >= 300:
                redirect++;
                break;
            case >= 200:
                success++;
                break;
        }

        totalMs += elapsedMs;
        maxMs = Math.Max ( maxMs, elapsedMs );

        int bin = elapsedMs <= MinBoundMs
            ? 0
            : Math.Min ( BinCount - 1, (int) Math.Ceiling ( Math.Log ( elapsedMs / MinBoundMs ) / LogGrowth ) );
        histogram [ bin ]++;
    }

    public HttpMetricsBucket Merge ( HttpMetricsBucket other ) {
        Requests += other.Requests;
        success += other.success;
        redirect += other.redirect;
        clientError += other.clientError;
        serverError += other.serverError;
        totalMs += other.totalMs;
        maxMs = Math.Max ( maxMs, other.maxMs );

        for (int bin = 0; bin < BinCount; bin++)
            histogram [ bin ] += other.histogram [ bin ];

        return this;
    }

    public HttpTotalsPayload ToTotals () {
        return new HttpTotalsPayload ( Requests, success, redirect, clientError, serverError );
    }

    public HttpLatencyPayload ToLatency () {
        return new HttpLatencyPayload (
            Math.Round ( Requests > 0 ? totalMs / Requests : 0, 3 ),
            Percentile ( 0.50 ),
            Percentile ( 0.95 ),
            Percentile ( 0.99 ),
            Math.Round ( maxMs, 3 ) );
    }

    double Percentile ( double quantile ) {
        double rank = quantile * Requests;
        long cumulative = 0;

        for (int bin = 0; bin < BinCount; bin++) {
            long count = histogram [ bin ];
            if (count == 0 || cumulative + count < rank) {
                cumulative += count;
                continue;
            }

            double lower = bin == 0 ? 0 : MinBoundMs * Math.Pow ( Growth, bin - 1 );
            double upper = bin == BinCount - 1 ? maxMs : MinBoundMs * Math.Pow ( Growth, bin );
            double interpolated = lower + ((upper - lower) * ((rank - cumulative) / count));

            return Math.Round ( Math.Min ( maxMs, interpolated ), 3 );
        }

        return Math.Round ( maxMs, 3 );
    }
}

internal sealed record HttpMetricsPayload (
    long InFlight,
    double RequestsPerMinute,
    double ErrorRate,
    HttpTotalsPayload Totals,
    HttpLatencyPayload Latency,
    HttpReadingPayload [] Readings,
    HttpRoutePayload [] Routes,
    HttpExceptionPayload [] Exceptions );

internal sealed record HttpTotalsPayload ( long Requests, long Success, long Redirect, long ClientError, long ServerError );

internal sealed record HttpLatencyPayload ( double Avg, double P50, double P95, double P99, double Max );

internal sealed record HttpReadingPayload ( DateTime Timestamp, HttpTotalsPayload Totals, HttpLatencyPayload? Latency );

internal sealed record HttpRoutePayload ( string Route, HttpTotalsPayload Totals, HttpLatencyPayload Latency );

internal sealed record HttpExceptionPayload ( DateTime Timestamp, string Route, string Type, string Message );

[JsonSourceGenerationOptions ( PropertyNamingPolicy = JsonKnownNamingPolicy.CamelCase )]
[JsonSerializable ( typeof ( HttpMetricsPayload ) )]
internal sealed partial class HttpMetricsJsonContext : JsonSerializerContext {
}
