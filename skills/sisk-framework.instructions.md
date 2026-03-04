---
applyTo: '**/*.cs'
---

# Sisk Framework — Core

Sisk is an HTTP framework for .NET that uses `HttpServer`, `Router`, `Route`, `HttpRequest`, and `HttpResponse` as fundamental blocks. Always prefer the fluent API (`.With*`) and C#'s functional patterns when working with Sisk.

Full reference: https://docs.sisk-framework.org/docs/summary.g

---

## Routing

The `Router` maps URLs to callbacks of type `RouteAction`. Ways to define routes:

```csharp
// Inline with Map*
mainRouter.MapGet("/hello/<name>", req => {
    string name = req.RouteParameters["name"].GetString();
    return new HttpResponse($"Hello, {name}");
});

// SetRoute
mainRouter.SetRoute(RouteMethod.Get, "/user/<id>", req => {
    Guid id = req.RouteParameters["id"].GetGuid();
    return new HttpResponse(200);
});

// Route.* helper
mainRouter += Route.Get("/image.png", req => {
    return new HttpResponse { Content = new StreamContent(File.OpenRead("image.png")) };
});
```

**Attributes in classes:**
```csharp
[RoutePrefix("/api/users")]
public class UsersController : RouterModule
{
    [RouteGet]               // GET /api/users
    public HttpResponse List(HttpRequest req) { ... }

    [RouteGet("/<id>")]      // GET /api/users/<id>
    public HttpResponse Get(HttpRequest req) { ... }

    [RoutePost]              // POST /api/users
    public HttpResponse Create(HttpRequest req) { ... }

    [RoutePatch("/<id>")]    // PATCH /api/users/<id>
    public HttpResponse Edit(HttpRequest req) { ... }

    [RouteDelete("/<id>")]   // DELETE /api/users/<id>
    public HttpResponse Delete(HttpRequest req) { ... }
}

mainRouter.SetObject(new UsersController());
```

**Routes with Regex:**
```csharp
mainRouter.SetRoute(new RegexRoute(RouteMethod.Get, @"/uploads/(?<filename>.*\.(jpg|png))", req => {
    string filename = req.RouteParameters["filename"].GetString();
    return new HttpResponse($"File: {filename}");
}));
```

**Router configurations:**
```csharp
mainRouter.MatchRoutesIgnoreCase = true;

mainRouter.NotFoundErrorHandler = () =>
    new HttpResponse(404) { Content = new StringContent("Not found") };

mainRouter.MethodNotAllowedErrorHandler = (ctx) =>
    new HttpResponse(405) { Content = new StringContent("Method not allowed") };

mainRouter.CallbackErrorHandler = (ex, ctx) =>
    new HttpResponse(500) { Content = new StringContent(ex.Message) };
```

**Wildcard routes:**
```csharp
mainRouter.SetRoute(RouteMethod.Any, "/", handler);           // any method
mainRouter.SetRoute(RouteMethod.Post, Route.AnyPath, handler); // any path
```

- `RouteParameters` and `Query` return `StringValueCollection` — each value is `StringValue` with helpers: `.GetString()`, `.GetGuid()`, `.GetInteger()`, etc.
- Trailing slash is ignored by default. To force: flag `HttpServerFlags.ForceTrailingSlash`.
- Sisk detects route collisions automatically when defining.

---

## Fundamentals — HttpRequest / HttpResponse

### HttpRequest

```csharp
HttpMethod method    = request.Method;
string path          = request.Path;
string fullUrl       = request.FullUrl;
string query         = request.QueryString;
var queryParams      = request.Query;           // StringValueCollection
var routeParams      = request.RouteParameters; // StringValueCollection

// Body
string body          = request.Body;
byte[] rawBody       = request.RawBody;
Stream stream        = request.GetRequestStream(); // single read

bool hasContent      = request.HasContents;
bool loaded          = request.IsContentAvailable;

// Form
var form             = request.GetFormContent();         // NameValueCollection
var multipart        = request.GetMultipartFormContent(); // MultipartObject[]

// Headers
string? auth         = request.Headers.Authorization;

// Context
HttpContext ctx       = request.Context;
var bag              = request.Bag;            // TypedValueDictionary

// Disconnection
CancellationToken dc = request.DisconnectToken;
```

- Configurable size limit in `HttpServerConfiguration.MaximumContentLength` — exceeding returns 413.
- `GetRequestStream()` can only be read once; after that `Body` and `RawBody` become unavailable.
- `HttpContext.Current` / `HttpContext.GetCurrentContext()` gets the current thread's context.
- `request.Bag.Set<T>()` / `request.Bag.Get<T>()` to pass typed data between handlers.

### HttpResponse

```csharp
// Basic construction
var res = new HttpResponse(200) {
    Content = new StringContent(json, Encoding.UTF8, "application/json")
};

// Fluent API
return new HttpResponse()
    .WithStatus(HttpStatusCode.Created)
    .WithHeader("Location", $"/users/{id}")
    .WithContent(JsonContent.Create(user))
    .WithCookie("session", token, expiresAt: DateTime.UtcNow.AddDays(7));

// Redirect
return new HttpResponse(301).WithHeader("Location", "/login");

// Chunked
var res = new HttpResponse { SendChunked = true };

// Response stream (large files)
var responseStream = request.GetResponseStream();
responseStream.SendChunked = true;
responseStream.SetStatus(200);
responseStream.SetHeader(HttpKnownHeaderNames.ContentType, "application/zip");
File.OpenRead("file.zip").CopyTo(responseStream.ResponseStream);
return responseStream.Close();
```

**Implicit return types** — configure the router to convert custom objects:
```csharp
router.RegisterValueHandler<MyResult>(result =>
    new HttpResponse { Content = JsonContent.Create(result) });

// fallback for any type (must be registered last)
router.RegisterValueHandler<object>(obj =>
    new HttpResponse { Content = JsonContent.Create(obj) });
```

- `HttpHeaderCollection.Add()` adds without replacing; `.Set()` / indexer replaces.
- `response.GetHeaderValue("Content-Type")` searches in `Headers` and `Content.Headers`.

---

## Request Handlers (Middlewares)

Implement `IRequestHandler` (or inherit `RequestHandler`) to create middlewares:

```csharp
public class AuthHandler : IRequestHandler
{
    public RequestHandlerExecutionMode ExecutionMode { get; init; }
        = RequestHandlerExecutionMode.BeforeResponse;

    public HttpResponse? Execute(HttpRequest request, HttpContext context)
    {
        if (request.Headers.Authorization is null)
            return new HttpResponse(HttpStatusCode.Unauthorized);

        context.RequestBag.Add("User", ResolveUser(request));
        return null; // continue cycle
    }
}
```

- Returning `null` → continues cycle.
- Returning `HttpResponse` → interrupts and responds immediately.
- `ExecutionMode.AfterResponse` → overwrites router's response if non-null is returned.

**Association:**

```csharp
// per route
mainRouter.SetRoute(RouteMethod.Get, "/", IndexPage, "", new IRequestHandler[] {
    new AuthHandler(),
    new RateLimitHandler(),
    // ↑ IndexPage executes here
    new AuditHandler()  // AfterResponse
});

// global (all routes)
mainRouter.GlobalRequestHandlers = new IRequestHandler[] { new AuthHandler() };

// by attribute
[RouteGet("/")]
[RequestHandler<AuthHandler>]
public HttpResponse Index(HttpRequest req) { ... }

// attribute with constructor arguments
[RequestHandler<RateLimitHandler>(100, TimeSpan.FromMinutes(1))]
public HttpResponse Limited(HttpRequest req) { ... }
```

**Bypassing global handler:**
```csharp
var auth = new AuthHandler();
mainRouter.GlobalRequestHandlers = new IRequestHandler[] { auth };

mainRouter.SetRoute(new Route(RouteMethod.Get, "/public", ...) {
    BypassGlobalRequestHandlers = new IRequestHandler[] { auth } // same instance
});
```

> Always use the **same instance** for bypass; creating a new instance won't work.

---

## Response Compression

**Manual per route:**
```csharp
router.MapGet("/hello", req =>
    new HttpResponse {
        Content = new GZipContent(new StringContent("hello")),
        // or BrotliContent, DeflateContent
    });

// with stream
router.MapGet("/archive", req =>
    new HttpResponse {
        Content = new GZipContent(File.OpenRead("/path/to/file.zip"))
        // don't use "using" here — server discards after sending
    });
```

**Automatic (recommended):**
```csharp
config.EnableAutomaticResponseCompression = true;
```

Priority order by `Accept-Encoding`: Brotli (`br`) → GZip (`gzip`) → Deflate (`deflate`).  
Responses that already inherit from `CompressedContent` are not compressed again.

---

## Web Sockets

```csharp
router.MapGet("/ws", async req =>
{
    using var ws = await req.GetWebSocketAsync();

    while (await ws.ReceiveMessageAsync(timeout: TimeSpan.FromSeconds(30)) is { } msg)
    {
        string text = msg.GetString();
        await ws.SendAsync($"Echo: {text}");
    }

    return await ws.CloseAsync();
});
```

- `ReceiveMessageAsync` returns `null` on timeout, cancellation, or disconnection.
- It's not possible to read and write simultaneously.

**Ping policy (keep connection alive):**
```csharp
ws.PingPolicy.Start(
    dataMessage: "ping",
    interval: TimeSpan.FromSeconds(10));
```

---

## Server-Sent Events (SSE)

```csharp
router.MapGet("/events", async req =>
{
    using var sse = await req.GetEventSourceAsync();

    sse.AppendHeader("X-Custom", "value"); // must come before any Send

    foreach (var item in GetItems())
    {
        await sse.SendAsync(item.ToString());
        await Task.Delay(500);
    }

    return sse.Close();
});
```

**Persistent identified connection (WaitForFail):**
```csharp
router.MapGet("/live", req =>
{
    using var sse = req.GetEventSource("client-" + req.Query["id"]);
    sse.WithPing(p => { p.DataMessage = "ping"; p.Interval = TimeSpan.FromSeconds(5); p.Start(); });
    sse.WaitForFail(TimeSpan.FromMinutes(10));
    return sse.Close();
});

// from elsewhere, send to specific connection
HttpRequestEventSource? conn = server.EventSources.GetByIdentifier("client-42");
conn?.Send("New event!");

// broadcast
foreach (var e in server.EventSources.Find(id => id.StartsWith("client-")))
    e.Send("Broadcast!");
```

- Browsers reconnect automatically after server closure; send a termination message to avoid infinite reconnection.
- SSE supports only GET in most browsers; don't expect other methods or custom headers.
- `server.EventSources.All` lists all active identified connections.

---

## Logging (LogStream)

```csharp
// File
config.AccessLogsStream = new LogStream("logs/access.log");
config.ErrorsLogsStream = new LogStream("logs/error.log");
config.ThrowExceptions  = false; // necessary for errors to go to ErrorsLogsStream

// Console
config.AccessLogsStream = new LogStream(Console.Out);

// Custom application log
var appLog = new LogStream("logs/app.log");
appLog.WriteLine("Server started at {0}", DateTime.Now);
```

**Custom format:**
```csharp
config.AccessLogsFormat = "%dd/%dmm/%dy %tH:%ti:%ts %tz %ls %ri %rm %rs://%ra%rz%rq [%sc %sd] %lin -> %lou in %lmsms";
// Uses HttpServerConfiguration.DefaultAccessLogFormat for default
```

Useful variables: `%ri` (client IP), `%rm` (method), `%rz` (path), `%sc` (status code), `%lms` (ms), `%{header-name}` (request header), `%{:header-name}` (response header).

**Automatic rotation:**
```csharp
new LogStream("logs/access.log")
    .ConfigureRotatingPolicy(
        maximumSize: 64 * SizeHelper.UnitMb,
        dueTime: TimeSpan.FromHours(6));
```

**Custom LogStream:**
```csharp
public class PrefixedLogStream : LogStream
{
    protected override void WriteLineInternal(string line)
        => base.WriteLineInternal($"[{DateTime.Now:g}] {line}");
}
```

- `LogStream` implements `IAsyncDisposable`; ensures pending log writes on dispose.
- File directory is created automatically; parent directory must exist.
- Rotation works only on file-based LogStreams.

---

## Server Handlers (HttpServerHandler)

`HttpServerHandler` provides hooks into the entire server lifecycle — unlike `IRequestHandler`, it can't be limited to specific routes. A single instance is created per type.

```csharp
public class DatabaseHandler : HttpServerHandler
{
    // called when HTTP connection is opened (headers received)
    protected override void OnHttpRequestOpen(HttpRequest request)
    {
        // read content here if needed after closure
        if (request.Headers["Content-Type"]?.Contains("json") == true)
            _ = request.RawBody; // forces read before GC
    }

    // called when HTTP connection is closed (response sent)
    protected override void OnHttpRequestClose(HttpServerExecutionResult result)
    {
        if (result.Request.Bag.IsSet<DbContext>())
            result.Request.Bag.Get<DbContext>().Dispose();
    }
}
```

**Registration:**
```csharp
using var app = HttpServer.CreateBuilder()
    .UseHandler<DatabaseHandler>()
    .Build();

// or after creation
server.RegisterHandler<DatabaseHandler>();
```

**Extension via extension methods:**
```csharp
public static class DbExtensions
{
    public static DbContext GetDb(this HttpRequest request)
        => request.Bag.GetOrAdd<DbContext>(() => new DbContext());
}
```

Available events (override as needed):
- `OnServerStarting` / `OnServerStopping`
- `OnHttpRequestOpen(HttpRequest)` — connection opened, headers available
- `OnHttpRequestClose(HttpServerExecutionResult)` — response sent, cleanup here
- `OnContextBagCreated(HttpContextBag)`

> Use `HttpServerHandler` to manage request-scoped resources (DB connections, sessions, telemetry). Use `IRequestHandler` for conditional logic per route (authentication, rate limit, validation).
