# Stream ping

Proposed to version 0.15.

Status: included in production release 0.15.

This proposal allows sending periodic packets automatically in web sockets or server sent events. The main motivation for this feature is to be able to keep the connection open and bypass TCP connection inactivity.

```cs
[RouteGet("/")]
public static HttpResponse Index(HttpRequest request)
{
    var sse = request.GetEventSource("my-connection");
    sse.WithPing(ping =>
    {
        ping.Interval = TimeSpan.FromSeconds(3);
        ping.DataMessage = "ping";
        ping.Start();
    });
    sse.WaitForFail(TimeSpan.FromSeconds(30));

    return sse.Close();
}
```

In the above code, in the interval of 1 second the server will send the "ping" message to the client. The same goes for Web sockets.