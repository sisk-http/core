# Input Streams

Proposed to version 0.15.

Status: approved, but may be breaking. Advanced use only.

Allows the user to read the input content stream inline.

It's a way to read the request input from the input stream. It can use this to send responses through GetResponseStream() as well.
Furthermore, it is possible to read the input stream even in an IRequestHandler with BeforeContents is active on the matched route.

This method will only be accessible in request handlers with BeforeContents ExecutionMode. This way, the user can peek on the content of the request before it goes to the router. One limitation is that content in HttpRequest.Body and RawBody will not be available as the input stream was read elsewhere.

Old code:

```cs
// in HttpRequest.cs

/// <summary>
/// Gets the HTTP request content stream.
/// </summary>
public Stream InputStream { get => listenerRequest.InputStream; }
```

Since <commit>:

```cs
/// <summary>
/// Gets the HTTP request content stream. This property is only available while the
/// content has not been imported by the HTTP server and will invalidate the body content
/// cached in this object.
/// </summary>
/// <definition>
/// public Stream GetInputStream()
/// </definition>
/// <type>
/// Method
/// </type>
/// <since>0.15</since>
public Stream GetInputStream()
{
    if (isContentAvailable)
    {
        throw new InvalidOperationException("The InputStream property is only accessible by BeforeContents requests handlers.");
    }
    return listenerRequest.InputStream;
}
```