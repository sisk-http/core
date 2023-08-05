# Input Streams

Proposed to version 0.15.

Status: requires more testing.

Allows the user to read the input content stream inline.

It's a way to read the request input from the input stream. It can use this to send responses through GetResponseStream() as well.
Furthermore, it is possible to read the input stream even in an IRequestHandler with BeforeContents is active on the matched route.

```cs
// in HttpRequest.cs

/// <summary>
/// Gets the HTTP request content stream.
/// </summary>
public Stream InputStream { get => listenerRequest.InputStream; }
```