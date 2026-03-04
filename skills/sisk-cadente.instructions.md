---
applyTo: '**/cadente/**/*.cs'
---

# Sisk.Cadente

Cadente is an **experimental** fully managed HTTP/1.1 server for Sisk, replacing the default `System.Net.HttpListener`. Key advantages: native SSL/TLS on all platforms, consistent cross-platform behavior, no dependency on `HttpListener`.

> **Status: Beta** — API may change. Not recommended for critical production without testing.

Reference: https://docs.sisk-framework.org/docs/cadente  
Packages: `Sisk.Cadente` (standalone), `Sisk.Cadente.CoreEngine` (Sisk integration)

---

## Using as Sisk Engine

The recommended way to use Cadente is via `CadenteHttpServerEngine` from `Sisk.Cadente.CoreEngine`:

```csharp
using Sisk.Core.Http;
using Sisk.Cadente.CoreEngine;

using var host = HttpServer.CreateBuilder()
    .UseEngine<CadenteHttpServerEngine>()
    .UseListeningPort(5000)
    .Build();

await host.StartAsync();
```

**With SSL:**
```csharp
using var host = HttpServer.CreateBuilder()
    .UseEngine<CadenteHttpServerEngine>()
    .UseSsl(certificate: CertificateHelper.CreateTrustedDevelopmentCertificate("localhost"))
    .Build();
```

**With advanced host configuration:**
```csharp
using var engine = new CadenteHttpServerEngine(host =>
{
    host.TimeoutManager.ClientReadTimeout  = TimeSpan.FromSeconds(60);
    host.TimeoutManager.ClientWriteTimeout = TimeSpan.FromSeconds(60);
    host.TimeoutManager.SslHandshakeTimeout = TimeSpan.FromSeconds(10);
});

using var sisk = HttpServer.CreateBuilder()
    .UseEngine(engine)
    .Build();
```

- `CadenteHttpServerEngine(Action<HttpHost>)` — the action is called for every `HttpHost` created (one per listening port).
- `IdleConnectionTimeout` on the engine defaults to 90 s; controls both read and write timeouts applied to all hosts.

---

## Standalone Usage

Use `HttpHost` + `HttpHostHandler` directly without Sisk, similar to `HttpListener`:

```csharp
using Sisk.Cadente;

var host = new HttpHost(15000)
{
    Handler = new MyHandler()
};

host.Start();
Thread.Sleep(-1); // keep alive

class MyHandler : HttpHostHandler
{
    public override async Task OnContextCreatedAsync(HttpHost host, HttpHostContext context)
    {
        context.Response.StatusCode = 200;
        context.Response.StatusDescription = "OK";
        context.Response.Headers.Set(new HttpHeader("Content-Type", "text/plain"));

        // chunked — no need to set Content-Length
        await using var stream = await context.Response.GetResponseStreamAsync(chunked: true);
        await using var writer = new StreamWriter(stream, leaveOpen: true);
        await writer.WriteLineAsync("Hello, world!");
    }
}
```

**Constructors:**
```csharp
new HttpHost(int port)                // listens on 127.0.0.1:<port>
new HttpHost(IPEndPoint endpoint)     // full control over IP and port
```

---

## HttpHost

| Member | Description |
|---|---|
| `Handler` | `HttpHostHandler` instance. Set before `Start()`. |
| `HttpsOptions` | Assign an `HttpsOptions` to enable TLS. `null` = plain HTTP. |
| `TimeoutManager` | `HttpHostTimeoutManager` — per-host timeout settings. |
| `Endpoint` | The `IPEndPoint` the host listens on (read-only). |
| `IsDisposed` | Whether the host has been disposed. |
| `ServerNameHeader` | Static. Default `"Sisk"`. Sets the `Server` response header. |
| `Start()` | Begins accepting connections. Call once. |
| `Dispose()` | Stops accepting and releases resources. |

---

## HttpHostHandler

Override virtual methods to hook into the connection lifecycle:

```csharp
public class AppHandler : HttpHostHandler
{
    // Called for each HTTP request (one per message on a keep-alive connection)
    public override async Task OnContextCreatedAsync(HttpHost host, HttpHostContext context)
    {
        // read request, write response here
    }

    // Called once when a TCP client connects
    public override Task OnClientConnectedAsync(HttpHost host, HttpHostClient client)
    {
        client.State = new ClientSession(); // attach arbitrary state
        return Task.CompletedTask;
    }

    // Called once when the TCP client disconnects
    public override Task OnClientDisconnectedAsync(HttpHost host, HttpHostClient client)
    {
        (client.State as ClientSession)?.Dispose();
        return Task.CompletedTask;
    }
}
```

- `OnContextCreatedAsync` is called for **every request** on the connection (HTTP/1.1 keep-alive reuses TCP).
- `OnClientConnectedAsync` / `OnClientDisconnectedAsync` bracket the full TCP connection lifetime.
- `client.State` is a free `object?` slot — useful for associating per-connection data (sessions, auth state, etc.).

---

## HttpHostContext

Represents a single HTTP request–response exchange:

```csharp
public override async Task OnContextCreatedAsync(HttpHost host, HttpHostContext context)
{
    HttpHostContext.HttpRequest  req = context.Request;
    HttpHostContext.HttpResponse res = context.Response;
    HttpHostClient client            = context.Client;

    context.KeepAlive = true;  // default true; set false to close after this response

    // abort the TCP connection immediately without sending a response
    context.Abort();
}
```

---

## HttpHostContext.HttpRequest

```csharp
string method      = req.Method;         // "GET", "POST", etc.
string path        = req.Path;           // "/api/users?id=1"
long   length      = req.ContentLength;
bool   hasBody     = req.HasBody;        // true if ContentLength > 0 or chunked

HttpHeaderList headers = req.Headers;    // read-only

// reading the body
Stream bodyStream = req.GetRequestStream();
using var reader  = new StreamReader(bodyStream);
string body       = await reader.ReadToEndAsync();
```

- `GetRequestStream()` handles both fixed-length and chunked bodies transparently.
- `HasBody` is `true` for chunked requests even if the actual body is empty — always check before reading.
- Automatically sends `100 Continue` if the client sent `Expect: 100-continue`.

---

## HttpHostContext.HttpResponse

```csharp
res.StatusCode        = 200;
res.StatusDescription = "OK";
res.Headers.Set(new HttpHeader("Content-Type", "application/json"));
res.Headers.Set(new HttpHeader("Content-Length", body.Length.ToString()));

await using var stream = await res.GetResponseStreamAsync(); // fixed-length
await stream.WriteAsync(Encoding.UTF8.GetBytes(body));
```

**Chunked response (no Content-Length needed):**
```csharp
res.Headers.Set(new HttpHeader("Content-Type", "text/plain"));

await using var stream = await res.GetResponseStreamAsync(chunked: true);
await using var writer = new StreamWriter(stream, leaveOpen: true);
await writer.WriteLineAsync("chunk 1");
await writer.WriteLineAsync("chunk 2");
```

- `GetResponseStreamAsync(chunked: false)` — **requires** `Content-Length` to be set beforehand, or throws `InvalidOperationException`.
- `GetResponseStreamAsync(chunked: true)` — sets `Transfer-Encoding: chunked` automatically and removes `Content-Length`.
- Headers are sent on the first call to `GetResponseStreamAsync`; adding headers after that has no effect.
- Default headers added automatically: `Date`, `Server`.

---

## HttpHostClient

```csharp
IPEndPoint         ep      = client.ClientEndpoint;    // remote IP:port
X509Certificate?   cert    = client.ClientCertificate; // null unless mTLS configured
CancellationToken  dc      = client.DisconnectToken;   // raised on disconnection
object?            state   = client.State;             // free slot for connection-scoped data
```

---

## HttpHeaderList

Used for both request (read-only) and response (read-write) headers:

```csharp
// add without replacing
headers.Add(new HttpHeader("X-Custom", "value"));

// add or replace (by name, case-insensitive)
headers.Set(new HttpHeader("Content-Type", "application/json"));

// remove all with a given name
headers.Remove(HttpHeaderName.ContentLength);

// check existence
bool has = headers.Contains(HttpHeaderName.ContentType);

// get value(s)
string value = headers.Get(HttpHeaderName.ContentType); // first match or ""
```

Use `HttpHeaderName` constants (`HttpHeaderName.ContentType`, `HttpHeaderName.ContentLength`, `HttpHeaderName.TransferEncoding`, etc.) instead of raw strings to avoid typos.

---

## HttpsOptions

Enables TLS for an `HttpHost`:

```csharp
var cert = X509Certificate2.CreateFromPemFile("cert.pem", "key.pem");

host.HttpsOptions = new HttpsOptions(cert)
{
    AllowedProtocols          = SslProtocols.Tls12 | SslProtocols.Tls13, // default
    ClientCertificateRequired = false,                                     // mTLS
    CheckCertificateRevocation = false
};
```

---

## HttpHostTimeoutManager

Configure per-host timeouts:

```csharp
host.TimeoutManager.ClientReadTimeout    = TimeSpan.FromSeconds(30); // default
host.TimeoutManager.ClientWriteTimeout   = TimeSpan.FromSeconds(30); // default
host.TimeoutManager.SslHandshakeTimeout  = TimeSpan.FromSeconds(5);  // default
host.TimeoutManager.HeaderParsingTimeout = TimeSpan.FromSeconds(30); // default
host.TimeoutManager.BodyDrainTimeout     = TimeSpan.FromSeconds(30); // default
```

---

## Do's and Don'ts

- **Do** set `Content-Length` before calling `GetResponseStreamAsync()` for non-chunked responses.
- **Do** use `chunked: true` when the response size is not known upfront (streaming, SSE-like flows).
- **Do** use `context.KeepAlive = false` when you want to close the connection after the response (e.g., on protocol errors).
- **Do** use `client.State` in `OnClientConnectedAsync` to attach session/auth state and clean it up in `OnClientDisconnectedAsync`.
- **Don't** set headers after `GetResponseStreamAsync()` is called — they won't be sent.
- **Don't** use `using` on the stream returned by `GetResponseStreamAsync()` with `chunked: true` inside a scoped block if you need to write to it after — the `await using` closes the chunked stream properly, flushing the terminal chunk.
- **Don't** share `HttpHostContext` instances across threads or requests.
