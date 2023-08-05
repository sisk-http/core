## Sisk 0.15 features release overview:

| Feature | Description | Status |
| - | - | - |
| Stream ping | This proposal allows sending periodic packets automatically in web sockets or server sent events. | Approved |
| Implicit return types | Implicitly typed returns allow callbacks to return any object other than HttpResponse. | Approved |
| Overridable headers | Overloadable headers is a way to ensure that these headers are sent at the end of the response. | Approved |
| Dynamic bag | This proposal lets you store objects in the `HttpContext.Bag` without the need to pack/unpack or define a key for them. | Testing | 
| Input streams | Allows the user to read the input content stream inline. | Testing |
| Sessions | Sessions are a way to track and identify a user throughout the application and take control of session storage on the server. | Testing |

## Other changelog

- https://github.com/sisk-http/core/commit/30de03cdb9df577039d267d14e94016f71cac656
    - Added the HttpRequest.InputStream property.

- https://github.com/sisk-http/core/commit/a445ad3651f910b3fbc6b8cb98ee08290d2410e4
    - Added the HttpContext.OverrideHeaders property.
    - Added the HttpRequest.SetContextBag and GetContextBag methods.
    - Added the Router.RegisterValueHandler method.
    - Added the RouterModule class.
    - Added the ValueResult class.
    - Rewrite the return type of HttpRequest.SendTo from HttpResponse to object.
    - Rewrite the LogStream.WriteException output format.
    - Rewrite the return type of RouterCallback delegate from HttpResponse to object.
    - Simplified the way HttpRequest obtains the origin IP of the request.

- https://github.com/sisk-http/core/commit/bbb0e84046eeb8684393230dfc4a4baacb062fba, https://github.com/sisk-http/core/commit/6f77f3db71fcdbe435d61db6fe7914f7ecb2ec06, https://github.com/sisk-http/core/commit/d643d718f256e6d1a3df276ac28dced09a5ec627
    - Added an string representation to HttpRequest.ToString().
    - Created the HttpStreamPingPolicy class.
    - Renamed HttpRequestEventSource.KeepAlive -> WaitForFail.
    - Added the HttpWebSocket.MaxAttempts property.
    - Fixed an bug where the WebSocket was throwing an exception when the client didn't terminated the close handshake with the server.