# Fluent HTTP responses

Proposed to version 0.15.

Status: approved.

Allows the user to take advantage of the Fluent Interface for HttpResponse objects and create simpler responses.

Previously, you could only create HTTP responses with:

```cs
[RouteGet("/")]
public static HttpResponse Home(HttpRequest request)
{
    var res = new HttpResponse();
    res.Content = new StringContent("hello");
    res.Headers.Add("header1", "value");
    res.Headers.Add("header2", "value");
    res.Status = System.Net.HttpStatusCode.Accepted;
    return res;
}
```

This proposal allows you to create responses such as:

```cs
[RouteGet("/")]
public static HttpResponse Home(HttpRequest request)
{
    return new HttpResponse()
        .WithContent(new StringContent("hello"))
        .WithHeader("header1", "value")
        .WithHeader("header2", "value")
        .WithStatus(202);
}
```