---
applyTo: '**/Sisk.Monitoring/**/*.cs'
---

# Sisk.Monitoring

## Overview

`Sisk.Monitoring` provides an embedded web dashboard for monitoring application counters, meters, and log streams. It integrates directly with `HttpServerHostContextBuilder` and renders an HTML UI accessible via browser.

## Core Types

### `ApplicationMonitor`

The main class. Hosts the dashboard routes and manages the registered resources.

```csharp
var monitor = new ApplicationMonitor();
monitor.PageTitle = "My App";
monitor.CaptureCounter(new MonitoringDefinition<Counter>("Requests", counter));
monitor.CaptureMeter(new MonitoringDefinition<Meter>("Throughput", meter));
monitor.CaptureLogStream(new MonitoringDefinition<LogStream>("App Log", logStream));
```

Key members:
- `PageTitle` — title shown in the dashboard header and browser tab.
- `CredentialValidator` — optional `Func<NetworkCredential, ValueTask<bool>>` for Basic Auth protection.
- `CaptureCounter(MonitoringDefinition<Counter>)` — registers a counter.
- `CaptureMeter(MonitoringDefinition<Meter>)` — registers a meter.
- `CaptureLogStream(MonitoringDefinition<LogStream>, int bufferLineCount = 500)` — registers a log stream and starts buffering it.
- `GetRoutes(string prefix)` — internal; returns the dashboard routes for a given prefix.

Virtual overrides for customization:
- `AuthenticateAccountAsync(string email, string password)` — override to plug in custom auth logic.
- `WriteSidebar(string? activeNavItem)` — override to change sidebar HTML.
- `GetDashboardPageHtmlAsync(HttpRequest)` — override dashboard page.
- `GetCountersPageHtmlAsync(HttpRequest)` — override counters page.
- `GetMetersPageHtmlAsync(HttpRequest)` — override meters page.
- `GetMetersDataAsync(HttpRequest)` — override meters JSON endpoint.
- `GetLogStreamPageHtmlAsync(HttpRequest, MonitoringDefinition<LogStream>)` — override log stream page.

---

### `MonitoringDefinition<T>`

Wraps a monitored resource with metadata.

```csharp
new MonitoringDefinition<Counter>("label", counter)
new MonitoringDefinition<Counter>("label", "GroupName", counter)
```

Properties:
- `Instance` — the monitored object.
- `Label` — display name.
- `Group` — optional group name for sidebar/grid grouping.
- `DashboardPinned` — if `true`, the resource appears on the main dashboard page.
- `SanitizedLabel` — URL-encoded label used in route paths.

---

### `Counter`

Thread-safe, time-expiring accumulator. Each increment has a configurable TTL; `Current` returns the live sum of non-expired increments.

```csharp
var counter = new Counter();
counter.DefaultDuration = TimeSpan.FromHours(1);
counter.Increment();           // +1 with DefaultDuration
counter.Increment(5);          // +5 with DefaultDuration
counter.Increment(1, TimeSpan.FromMinutes(30)); // +1 for 30 min
double value = counter.Current;
counter.Reset();
```

---

### `Meter`

Sliding-window time-series recorder. Accumulates `MeterReading` values; `Read()` aggregates them into buckets.

```csharp
var meter = new Meter();                          // 24-hour window, default
var meter = new Meter(TimeSpan.FromHours(12));    // custom window

meter.Increment();     // +1
meter.Increment(42.5); // +42.5

// Read with 1-hour buckets (default)
foreach (var reading in meter.Read())
    Console.WriteLine($"{reading.Timestamp}: {reading.Value}");

// Read with custom tick
foreach (var reading in meter.Read(TimeSpan.FromMinutes(15)))
    Console.WriteLine($"{reading.Timestamp}: {reading.Value}");

meter.Reset();
```

`MeterReading` is a `record struct(double Value, DateTime Timestamp)`.

---

## Setup with `HttpServerHostContextBuilder`

### Simple setup (default `/monitoring` path)

```csharp
builder.UseMonitoring(monitor => {
    monitor.PageTitle = "My App Monitor";
    monitor.CaptureCounter(new MonitoringDefinition<Counter>("HTTP Requests", requestCounter) {
        DashboardPinned = true
    });
    monitor.CaptureMeter(new MonitoringDefinition<Meter>("Requests/hour", requestMeter) {
        DashboardPinned = true,
        Group = "Traffic"
    });
    monitor.CaptureLogStream(new MonitoringDefinition<LogStream>("Main Log", logStream) {
        DashboardPinned = true
    });
});
```

### Custom base path

```csharp
builder.UseMonitoring("/admin/monitoring", monitor => {
    // configure monitor
});
```

### Custom `ApplicationMonitor` subclass

```csharp
builder.UseMonitoring(() => new MyCustomMonitor());
builder.UseMonitoring("/metrics", () => new MyCustomMonitor());
```

---

## Authentication

Set `CredentialValidator` for Basic Auth on the dashboard:

```csharp
monitor.CredentialValidator = async credential => {
    return credential.UserName == "admin" && credential.Password == "secret";
};
```

Or override `AuthenticateAccountAsync`:

```csharp
public class MyMonitor : ApplicationMonitor {
    protected override ValueTask<bool> AuthenticateAccountAsync(string email, string password) {
        return new ValueTask<bool>(email == "admin@example.com" && password == "pass");
    }
}
```

When no validator is set, the dashboard is publicly accessible.

---

## Dashboard Routes (auto-registered)

| Path | Description |
|------|-------------|
| `{prefix}/` | Dashboard overview (pinned resources) |
| `{prefix}/counters` | All registered counters |
| `{prefix}/meters` | All registered meters with charts |
| `{prefix}/meters/data` | JSON endpoint for meter data (`?name=Label`) |
| `{prefix}/logstream/{name}` | Log stream viewer for a specific stream |
| `{prefix}/health` | Server health information |

---

## Grouping

Use `Group` on `MonitoringDefinition<T>` to organize resources in the sidebar and on pages. Items without a group appear under a default group key.

```csharp
new MonitoringDefinition<Counter>("DB Queries", "Database", dbQueryCounter)
new MonitoringDefinition<Counter>("Cache Hits",  "Cache",    cacheHitCounter)
```

---

## Patterns and Best Practices

- Define counters and meters as singletons (e.g., static fields or DI singletons) so they accumulate values across requests.
- Set `DashboardPinned = true` only for the most important resources you want visible at a glance.
- Keep `Counter.DefaultDuration` aligned with your reporting window (e.g., 1 hour for "requests per hour").
- Use `Meter` for time-series charts; use `Counter` for simple current-value cards.
- Use `Group` to keep the sidebar readable when you have many resources.
- Override `ApplicationMonitor` virtual methods only when you need to customize the HTML output.
- Always call `CaptureLogStream` before the log stream receives any writes — buffering must be started first.
