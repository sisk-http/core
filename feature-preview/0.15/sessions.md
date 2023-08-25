# Sessions

Proposed to version 0.15.

Status: refused.

### Refused

This proposal was rejected because sessions are more complicated than it seems and the complexity of implementation is beyond the scope of the project, in bringing a very general solution to very canonical and non-specific problems.

-----

Sessions are a way to track and identify a user throughout the application and take control of session storage on the server. This allows storing data intended for a session created on the HTTP client in the Sisk application.

The operation is similar to what exists in PHP, where it creates a session ID and associates it with the client. This ID is generated as a GUID and sent to the client via a Cookie. The lifespan of this ID is controlled by the server and not the client. The session storage is flexible, allowing the user to implement their own storage method. This proposal includes two primitive forms of session storage: memory-based and file-based. The memory-based option is faster and controlled by the RuntimeCache's cache. The file-based option may be slightly slower due to file read/write operations, but it can be more useful for persisting data using physical storage.

This proposal demonstrates a way to store only one session on the client, controlled by a single storage controller. The idea of having multiple hybrid sessions for the same client starts to create complexity and goes beyond the scope of the implementation.

The session implementation model is imperceptible to the user and straightforward to accomplish. In the server configuration, a session configuration object controls how sessions will be used throughout the Sisk application:

```cs
HttpServerConfiguration config = new HttpServerConfiguration();
config.SessionConfiguration.SessionController = new MemorySessionController();
config.SessionConfiguration.Enabled = true;
```

With this, a functional property within the HTTP context is made available to the user:

```cs
[RouteGet("/")]
public static HttpResponse Index(HttpRequest request)
{
    var session = request.Context.Session!;

    if (session["dice-roll"] == null)
        session["dice-roll"] = Random.Shared.Next(1, 6);

    return new HttpResponse()
    {
        Content = new StringContent("Your dice number is " + session["dice-roll"])
    };
}
```

In the above code, unless the client manually removes their session cookie, the "dice-roll" value will be maintained for every request. For new sessions, a new "dice-roll" will be generated.

The session garbage collection (GC) is manual and should be controlled by the user, either through scheduling or some other means. The garbage collection implementation is also done by the user, which involves comparing which sessions should be deleted and which ones are still in use. For the FileSystemSessionController, the algorithm compares the last access date of the session file with the current time and the configured session validity to determine which sessions should be deleted.

Whenever the HTTP server starts with HttpServer.Start() and there is an active session controller, the garbage collection (GC) is called on the controller. This is the only time that the Sisk framework natively invokes the GC for sessions. After the initial GC, it is up to the user to manage the garbage collection manually or schedule it as needed for subsequent sessions.

The code snippet below demonstrates a programmable example of how to call the Garbage Collector (GC). In this code, every request has a 1% chance of invoking the GC.

```cs
[RouteGet("/")]
public static HttpResponse Index(HttpRequest request)
{
    if (Random.Shared.Next(0, 100) == 1)
    {
        Console.WriteLine("Running GC!");
        request.Context.HttpServer.ServerConfiguration.SessionConfiguration.SessionController.RunSessionGC();
    }

    var session = request.Context.Session!;

    if (session["dice-roll"] == null)
        session["dice-roll"] = Random.Shared.Next(1, 6);

    return new HttpResponse()
    {
        Content = new StringContent("Your dice number is " + session["dice-roll"])
    };
}
```

> If you are going to follow something like the code above, it is recommended to create a global RequestHandler for the router, which runs after the execution of the routes.