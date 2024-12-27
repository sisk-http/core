# Cadente - The Sisk managed HTTP listener

This folder contains the code for the implementation of the Sisk HTTP/1.1 listener, called Project Cadente. It is a managed alternative to the [HttpListener](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=net-9.0) of .NET. This implementation exists because:

- We do not know how long Microsoft will want to maintain HttpListener as a component of .NET.
- They don't want to detach Kestrel from ASP.NET.
- It is not possible to use HttpListener with SSL natively, only with a reverse proxy or with IIS on Windows.
- The implementation of HttpListener outside of Windows is slow, but stable. This implementation is an attempt to create something more performant, closer or better than Kestrel speeds.

## How to use

For now, this implementation does not even have a package or is used in Sisk.HttpServer, but it can be used with:

```csharp
static void Main ( string [] args ) {
    using CadenteHttpListener host = new CadenteHttpListener ( 5555, HandleSession );
    
    // optional properties to run SSL
    host.HttpsOptions = new HttpsOptions ( CertificateUtil.CreateTrustedDevelopmentCertificate () );

    Console.WriteLine ( "server running at : http://localhost:5555/" );

    host.Start ();
    Thread.Sleep ( -1 );
}

public static void HandleSession ( HttpSession session ) {
    // handle the request here
}
```

## Implementation status

This package is expected to supersede Sisk.SslProxy and deprecate it.

The current status of the implementation is:

| Resource | Status | Notes |
| ------- | ------ | ----------- |
| Base HTTP/1.1 Reader | OK - Needs testing | |
| HTTPS | OK - Needs testing | |
| Chunked transfer-encoding | OK - Needs testing | Only for responses. |
| SSE/Response content streaming | OK - Needs testing |  |
| Gzip transfer encoding | Not implemented | Implement for both request and response. |
| Deflate transfer encoding | Not implemented | Implement for both request and response. |
| Brotli transfer encoding | Not implemented | Implement for both request and response. |
| Handle Expect-100 | Not implemented | There is already an implementation in Sisk.SslProxy. |
| Web Sockets | Not implemented |  |
| Trailer headers | - | Will not be implemented. |
| Pipelining | - | May be implemented later. |

Everything in this project is still super experimental and should never be used in production.

## Why not HTTP/2 or QUIC?

Because they're hard to implement. And because HTTP/1.1 is still capable of doing everything they do, most applications and clients don't need all the features they have. Overall, the HTTP protocol itself is awful, a mess, just like the entire web infrastructure, but it is the most used and capable internet transport we have today (for application-level protocols, not network protocols).

The truth is that HTTP/2 and QUIC are solutions to problems invented by their creators. But, if you disagree with me, you still can use Sisk with [HTTP/2 or QUIC](https://learn.microsoft.com/en-us/iis/manage/configuring-security/how-to-set-up-ssl-on-iis) in Windows.

## License

The same as the Sisk project (MIT).