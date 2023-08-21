# Dynamic request bag

Proposed to version 0.15.

Status: approved.

This proposal lets you store objects in the `HttpContext.Bag` without the need to pack/unpack or define a key for them. The motivation is to remove the need for converting the object and having to look for objects with the same name as the one that was stored.

The `HttpRequest.GetContextBag<>() ` and `HttpRequest.SetContextBag<>()` methods allow you to get and store objects in the `HttpContext.Bag` by type name, eliminating type conversion.

Objects are stored in the bag by their full type name. These dynamic objects cannot be null.

```cs
public class MyRequestBag
{
    public string Message { get; set; } = "";
}

public class InitializeMyRequestBag : IRequestHandler
{
    public RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeContents;

    public HttpResponse? Execute(HttpRequest request, HttpContext context)
    {
        var bag = new MyRequestBag();
        bag.Message = "Hello, world!";
        request.SetContextBag(bag);

        return null;
    }
}

public class Test
{
    [RouteGet("/")]
    [RequestHandler(typeof(InitializeMyRequestBag))]
    public string Index(HttpRequest request)
    {
        MyRequestBag bag = request.GetContextBag<MyRequestBag>();
        return bag.Message;
    }
}
```