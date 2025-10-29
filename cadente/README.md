# Cadente - The Sisk managed HTTP listener

This folder contains the code for the implementation of the Sisk HTTP/1.1 listener, called Project Cadente. It is a managed alternative to the [HttpListener](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=net-9.0) of .NET. This implementation exists because:

- We do not know how long Microsoft will want to maintain HttpListener as a component of .NET.
- Kestrel is good, but you don't wanna use the entire ASP.NET.
- It is not possible to use HttpListener with SSL natively, only with a reverse proxy or with IIS on Windows.
- The implementation of HttpListener outside of Windows is slow, unpredictable, but somewhat stable. This implementation is an attempt to create something more performant, closer or even better than Kestrel speeds.

## How to use

For now, this implementation does not even have a package or is used in Sisk.HttpServer, but it can be used with:

```csharp
internal class Program
{
    static async Task Main(string[] args)
    {
        var cadenteHost = new HttpHost(15000)
        {
            Handler = new HostHandler()
        };

        cadenteHost.Start();
        Thread.Sleep(-1);
    }
}

sealed class HostHandler : HttpHostHandler
{
    public override async Task OnContextCreatedAsync(HttpHost host, HttpHostContext context)
    {
        context.Response.StatusCode = 200;
        using var contentStream = context.Response.GetResponseStream(chunked: true);
        using var writer = new StreamWriter(contentStream);

        await writer.WriteLineAsync("Hello, world!");
    }
}
```

## Roadmap

This package is expected to supersede Sisk.SslProxy and deprecate it.

The current status of the implementation is:

| Resource | Status | Notes |
| ------- | ------ | ----------- |
| Base HTTP/1.1 Reader | ✅ OK | |
| HTTPS/SSL | ✅ OK | |
| Chunked-transfers responses | ✅ OK | |
| Chunked-transfers requests | ❌ Not implemented | |
| Handle Expect-100 | ✅ OK | |
| Compressed transfer encoding (gzip, brotli, etc) | ✅ OK/External | |
| SSE/Response content streaming | ✅ OK/External  |  |
| Web Sockets | ✅ OK/External |  |
| Trailer headers | ⛔ Discarded |  |
| Pipelining | ⛔ Discarded |  |

Everything in this project is still super experimental and should never be used in production.

Legends:
- ✅ **OK** - means it is done and implemented directly in the Sisk.Cadente code.
- ✅ **OK/External** - means it is implemented in the Sisk.Core package and will not be implemented directly in Sisk.Cadente.
- ⛔ **Discarded** - means it will not be implemented at the moment or just not in the radar.
- ❌ **Not implemented** - means it has not yet been implemented or planned.

## Why not HTTP/2 or QUIC?

Because they're hard to implement and nobody wants to do it (nobody = me).

And because HTTP/1.1 is still capable of doing everything they do. Most applications and clients doens't need all the features they have. Overall, the HTTP protocol itself is awful, a mess, just like the entire web infrastructure, but it is the most used and capable internet transport we have today (for application-level protocols, not network protocols).

The truth is that HTTP/2 and QUIC are solutions to problems invented by their creators. But, if you disagree with me, you still can use Sisk with [HTTP/2 or QUIC](https://learn.microsoft.com/en-us/iis/manage/configuring-security/how-to-set-up-ssl-on-iis) in Windows.

## License

The same as the Sisk project (MIT).