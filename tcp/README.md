# Sisk managed HTTP listener

This folder contains the code for the implementation of the Sisk HTTP/1.1 listener. It is a managed alternative to the [HttpListener](https://learn.microsoft.com/en-us/dotnet/api/system.net.httplistener?view=net-9.0) of .NET. This implementation exists because:

- We do not know how long Microsoft will want to maintain HttpListener as a component of .NET.
- It is not possible to use HttpListener with SSL natively, only with a reverse proxy or with IIS on Windows.
- The implementation of HttpListener outside of Windows is awful, but stable. This implementation is an attempt to create something more performant, closer or better than Kestrel speeds.

## How to use

For now, this implementation does not even have a package or is used in Sisk.HttpServer, but it can be used with:

```csharp
static void Main ( string [] args ) {
    HttpHost host = new HttpHost ( 5555, HandleSession );
    
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
| Base HTTP/1.1 Reader | Functional | |
| HTTPS | Functional | |
| Expect-100 header | Not implemented | There is already an implementation in Sisk.SslProxy. |
| Chunked transfer-encoding | Not implemented |  |
| SSE/Response content streaming | Not implemented |  |
| Web Sockets | Not implemented |  |

Everything in this project is still super experimental and should never be used in production.

## License

The same as the Sisk project (MIT).