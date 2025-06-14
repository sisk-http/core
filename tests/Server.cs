using Sisk.Core.Entity; // Added for MultipartFormCollection and MultipartObject
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Http.Streams; // Added for HttpRequestEventSource
using Sisk.Core.Routing;
using System.Collections.Generic; // Added for List and Dictionary
using System.Linq;
using System.Net.Http.Json;
using System.Text;
using System.Threading; // Added for Thread.Sleep
using System.Threading.Tasks; // Added for async Task and Task.Delay
using Sisk.Core.WebSockets; // Added for WebSocket support
using System.Net.WebSockets; // Added for WebSocketMessageType
using System.Security.Cryptography; // Added for SHA256

namespace tests;

[TestClass]
public sealed class Server
{
    public static HttpServerHostContext Instance = null!;

    public static HttpClient GetHttpClient() => new HttpClient() { BaseAddress = new Uri(Instance.HttpServer.ListeningPrefixes[0]) };

    [AssemblyInitialize]
    public static void AssemblyInit(TestContext testContext)
    {
        Instance = HttpServer.CreateBuilder()
            .UseCors(new CrossOriginResourceSharingHeaders(allowOrigin: "*", allowMethods: ["GET", "POST", "PUT"]))
            .UseRouter(router =>
            {
                router.MapGet("/tests/plaintext", delegate (HttpRequest request)
                {
                    return new HttpResponse()
                    {
                        Content = new StringContent("Hello, world!", Encoding.UTF8, "text/plain"),
                        Status = HttpStatusInformation.Ok
                    };
                });
                router.MapGet("/tests/plaintext/chunked", delegate (HttpRequest request)
                {
                    return new HttpResponse()
                    {
                        Content = new StringContent("Hello, world!", Encoding.UTF8, "text/plain"),
                        Status = HttpStatusInformation.Ok,
                        SendChunked = true
                    };
                });

                router.SetRoute(RouteMethod.Get, "/tests/bytearray", (req) =>
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes("This is a Sisk byte array response.");
                    // Use Sisk.Core.Http.ByteArrayContent
                    var content = new ByteArrayContent(byteArray);
                    return new HttpResponse(content) { Headers = new() { ContentType = "application/custom-binary" } };
                });

                router.SetRoute(RouteMethod.Get, "/tests/bytearray/defaulted", (req) =>
                {
                    byte[] byteArray = Encoding.UTF8.GetBytes("Defaulted Sisk byte array.");
                    // Use Sisk.Core.Http.ByteArrayContent
                    var content = new ByteArrayContent(byteArray); // Defaults to application/octet-stream
                    return new HttpResponse(content);
                });

                router.SetRoute(RouteMethod.Get, "/tests/htmlcontent", (req) =>
                {
                    string htmlString = "<html><body><h1>Hello from Sisk HtmlContent</h1></body></html>";
                    // Use Sisk.Core.Http.HtmlContent
                    var content = new Sisk.Core.Http.HtmlContent(htmlString);
                    // HtmlContent should default to text/html; charset=utf-8
                    return new HttpResponse(content);
                });

                router.SetRoute(RouteMethod.Get, "/tests/htmlcontent/customchar", (req) =>
                {
                    string htmlString = "<html><body><h1>Custom Charset</h1></body></html>";
                    // Use Sisk.Core.Http.HtmlContent, specifying a different encoding
                    var content = new HtmlContent(htmlString, System.Text.Encoding.ASCII);
                    // The Content-Type header should reflect this: text/html; charset=us-ascii
                    return new HttpResponse(content);
                });

                router.SetRoute(RouteMethod.Get, "/tests/streamcontent/seekable", (req) =>
                {
                    string streamData = "This is data from a seekable stream.";
                    byte[] streamBytes = Encoding.UTF8.GetBytes(streamData);
                    var memoryStream = new System.IO.MemoryStream(streamBytes); // MemoryStream is seekable
                    var content = new StreamContent(memoryStream);
                    return new HttpResponse(content);
                });

                router.SetRoute(RouteMethod.Get, "/tests/streamcontent/nonseekable", (req) =>
                {
                    string streamData = "Data from a non-seekable stream.";
                    byte[] streamBytes = Encoding.UTF8.GetBytes(streamData);
                    var memoryStream = new System.IO.MemoryStream(streamBytes);
                    var nonSeekableStream = new NonSeekableStreamWrapper(memoryStream);
                    var content = new StreamContent(nonSeekableStream);
                    return new HttpResponse(content);
                });

                router.SetRoute(RouteMethod.Get, "/tests/streamcontent/predefinedlength", (req) =>
                {
                    string streamData = "Data from stream with predefined length.";
                    byte[] streamBytes = Encoding.UTF8.GetBytes(streamData);
                    var memoryStream = new System.IO.MemoryStream(streamBytes);
                    var content = new StreamContent(memoryStream);
                    return new HttpResponse(content) { Headers = new() { ContentType = "text/example" } };
                });

                router.SetRoute(RouteMethod.Get, "/tests/responsestream/simple", (req) =>
                {
                    var responseStreamManager = req.GetResponseStream();

                    string responseBody = "Hello from GetResponseStream!";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseBody);

                    responseStreamManager.SetStatus(System.Net.HttpStatusCode.OK);
                    responseStreamManager.SetHeader(HttpKnownHeaderNames.ContentType, "text/plain; charset=utf-8");
                    responseStreamManager.SetContentLength(responseBytes.Length);

                    // Write directly to the HttpListenerResponse's output stream.
                    responseStreamManager.Write(responseBytes, 0, responseBytes.Length);

                    return responseStreamManager.Close();
                });

                router.SetRoute(RouteMethod.Get, "/tests/responsestream/chunked", (req) =>
                {
                    var responseStreamManager = req.GetResponseStream();

                    responseStreamManager.SetStatus(System.Net.HttpStatusCode.OK);
                    responseStreamManager.SetHeader(HttpKnownHeaderNames.ContentType, "text/plain; charset=utf-8");

                    byte[] chunk1Bytes = Encoding.UTF8.GetBytes("This is the first chunk. ");
                    responseStreamManager.Write(chunk1Bytes); // isLast = false

                    byte[] chunk2Bytes = Encoding.UTF8.GetBytes("This is the second chunk. ");
                    responseStreamManager.Write(chunk2Bytes); // isLast = false

                    byte[] chunk3Bytes = Encoding.UTF8.GetBytes("This is the final chunk.");
                    responseStreamManager.Write(chunk3Bytes);  // isLast = true, this will send the 0-length final chunk and close.

                    return responseStreamManager.Close();
                });

                // Routes for HttpRequest body reading tests
                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getBodyContents", (req) =>
                {
                    // Server-side: Use GetBodyContents() to read the request body
                    byte[] bodyBytes = req.GetBodyContents();
                    // Echo the body back in the response
                    return new HttpResponse(new ByteArrayContent(bodyBytes));
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getBodyContentsAsync", async (HttpRequest req) =>
                {
                    // Server-side: Use GetBodyContentsAsync() to read the request body
                    Memory<byte> bodyMemory = await req.GetBodyContentsAsync();
                    // Echo the body back in the response
                    return new HttpResponse(new ByteArrayContent(bodyMemory.ToArray()));
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/rawBody", (req) =>
                {
                    // Server-side: Use RawBody property to read the request body
                    byte[] bodyBytes = req.RawBody;
                    // Echo the body back in the response
                    return new HttpResponse(new ByteArrayContent(bodyBytes));
                });

                // Routes for HttpRequest JSON content reading tests
                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getJsonContent", (req) =>
                {
                    try
                    {
                        var poco = req.GetJsonContent<tests.Tests.TestPoco>();
                        if (poco != null)
                        {
                            return new HttpResponse(JsonContent.Create(poco));
                        }
                        return new HttpResponse("null");
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getJsonContentAsync", async (HttpRequest req) =>
                {
                    try
                    {
                        var poco = await req.GetJsonContentAsync<tests.Tests.TestPoco>();
                        if (poco != null)
                        {
                            return new HttpResponse(JsonContent.Create(poco));
                        }
                        return new HttpResponse("null");
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getJsonContentWithOptions", (req) =>
                {
                    try
                    {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var poco = req.GetJsonContent<tests.Tests.TestPoco>(options);
                        if (poco != null)
                        {
                            return new HttpResponse(JsonContent.Create(poco));
                        }
                        return new HttpResponse("null");
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException (options): {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getJsonContentAsyncWithOptions", async (HttpRequest req) =>
                {
                    try
                    {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var poco = await req.GetJsonContentAsync<tests.Tests.TestPoco>(options);
                        if (poco != null)
                        {
                            return new HttpResponse(JsonContent.Create(poco));
                        }
                        return new HttpResponse("null");
                    }
                    catch (System.Text.Json.JsonException ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException (options, async): {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest form content reading tests
                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getFormContent", (req) =>
                {
                    try
                    {
                        Sisk.Core.Entity.StringKeyStoreCollection form = req.GetFormContent();
                        var dictionary = new System.Collections.Generic.Dictionary<string, string?>();
                        if (form != null)
                        {
                            foreach (string? key in form.Keys)
                            {
                                if (key != null)
                                {
                                    dictionary[key] = form[key];// Access .Value from StringValue
                                }
                            }
                        }
                        return new HttpResponse(JsonContent.Create(dictionary));
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getFormContentAsync", async (HttpRequest req) =>
                {
                    try
                    {
                        Sisk.Core.Entity.StringKeyStoreCollection form = await req.GetFormContentAsync();
                        var dictionary = new System.Collections.Generic.Dictionary<string, string?>();
                        if (form != null)
                        {
                            foreach (string? key in form.Keys)
                            {
                                if (key != null)
                                {
                                    dictionary[key] = form[key];
                                }
                            }
                        }
                        return new HttpResponse(JsonContent.Create(dictionary));
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest multipart form content reading tests
                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getMultipartFormContent", (req) =>
                {
                    try
                    {
                        Sisk.Core.Entity.MultipartFormCollection multipartCollection = req.GetMultipartFormContent();
                        var simplifiedResult = multipartCollection
                            .Values.Select(mpo => new SimpleMultipartObjectInfo
                            {
                                Name = mpo.Name,
                                Value = mpo.IsFile ? null : mpo.ReadContentAsString(),
                                FileName = mpo.IsFile ? mpo.Filename : null,
                                ContentType = mpo.ContentType,
                                Length = mpo.ContentBytes?.Length ?? 0,
                                ContentPreview = mpo.IsFile && (mpo.ContentType?.StartsWith("text/") == true) && mpo.ContentBytes != null
                                                 ? Encoding.UTF8.GetString(mpo.ContentBytes.Take(100).ToArray())
                                                 : (mpo.IsFile ? null : mpo.ReadContentAsString())
                            })
                            .ToList();
                        return new HttpResponse(JsonContent.Create(simplifiedResult));
                    }
                    catch (Sisk.Core.Http.HttpRequestException ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getMultipartFormContentAsync", async (HttpRequest req) =>
                {
                    try
                    {
                        Sisk.Core.Entity.MultipartFormCollection multipartCollection = await req.GetMultipartFormContentAsync();
                        var simplifiedResult = multipartCollection
                            .Values.Select(mpo => new SimpleMultipartObjectInfo
                            {
                                Name = mpo.Name,
                                Value = mpo.IsFile ? null : mpo.ReadContentAsString(),
                                FileName = mpo.IsFile ? mpo.Filename : null,
                                ContentType = mpo.ContentType,
                                Length = mpo.ContentBytes?.Length ?? 0,
                                ContentPreview = mpo.IsFile && (mpo.ContentType?.StartsWith("text/") == true) && mpo.ContentBytes != null
                                                 ? Encoding.UTF8.GetString(mpo.ContentBytes.Take(100).ToArray())
                                                 : (mpo.IsFile ? null : mpo.ReadContentAsString())
                            })
                            .ToList();
                        return new HttpResponse(JsonContent.Create(simplifiedResult));
                    }
                    catch (Sisk.Core.Http.HttpRequestException ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {ex.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest GetRequestStream tests
                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getRequestStream/read", async (HttpRequest req) =>
                {
                    try
                    {
                        using (System.IO.Stream requestStream = req.GetRequestStream())
                        using (var reader = new System.IO.StreamReader(requestStream, Encoding.UTF8))
                        {
                            string content = await reader.ReadToEndAsync();
                            // Echo back the content read from the stream
                            return new HttpResponse(new StringContent(content, Encoding.UTF8));
                        }
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getRequestStream/empty", async (HttpRequest req) =>
                {
                    try
                    {
                        using (System.IO.Stream requestStream = req.GetRequestStream())
                        using (var reader = new System.IO.StreamReader(requestStream, Encoding.UTF8))
                        {
                            string content = await reader.ReadToEndAsync();
                            if (string.IsNullOrEmpty(content))
                            {
                                return new HttpResponse(System.Net.HttpStatusCode.OK, "Stream was empty as expected.");
                            }
                            return new HttpResponse(System.Net.HttpStatusCode.BadRequest, "Stream was not empty.");
                        }
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest GetRequestStream tests (after body consumption)
                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getRequestStream/afterGetBodyContents", (req) =>
                {
                    try
                    {
                        _ = req.GetBodyContents();
                        req.GetRequestStream();
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, "GetRequestStream did not throw after GetBodyContents.");
                    }
                    catch (InvalidOperationException)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.OK, "Caught InvalidOperationException as expected.");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getRequestStream/afterRawBody", (req) =>
                {
                    try
                    {
                        _ = req.RawBody;
                        req.GetRequestStream();
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, "GetRequestStream did not throw after RawBody access.");
                    }
                    catch (InvalidOperationException)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.OK, "Caught InvalidOperationException as expected.");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/httprequest/getRequestStream/afterGetJsonContent", (req) =>
                {
                    try
                    {
                        // Assuming tests.Tests.TestPoco is accessible or a compatible DTO is used.
                        _ = req.GetJsonContent<tests.Tests.TestPoco>();

                        using (System.IO.Stream requestStream = req.GetRequestStream())
                        {
                            byte[] buffer = new byte[10];
                            int bytesRead = requestStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0)
                            {
                                var contentBytesField = typeof(Sisk.Core.Http.HttpRequest).GetField("contentBytes",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                object? contentBytesValue = contentBytesField?.GetValue(req);

                                if (contentBytesValue == null)
                                {
                                    return new HttpResponse(System.Net.HttpStatusCode.OK, "Stream returned and was consumed (0 bytes read), contentBytes is null.");
                                }
                                else
                                {
                                    return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, "Stream returned and was consumed, but contentBytes was not null.");
                                }
                            }
                            return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Stream returned but was not fully consumed (read {bytesRead} bytes).");
                        }
                    }
                    catch (InvalidOperationException ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected InvalidOperationException: {ex.Message}");
                    }
                    catch (System.Text.Json.JsonException jEx)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"Json body error: {jEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}");
                    }
                });

                router.SetRoute(RouteMethod.Post, "/tests/multipart/echo", async (HttpRequest req) =>
                {
                    try
                    {
                        Sisk.Core.Entity.MultipartFormCollection multipartCollection = await req.GetMultipartFormContentAsync();
                        var result = new List<SimpleMultipartObjectInfo>();
                        if (multipartCollection == null)
                        {
                            // This case might not be hit if GetMultipartFormContentAsync throws an exception
                            // for non-multipart content or other issues. Included for completeness.
                            return new HttpResponse(System.Net.HttpStatusCode.BadRequest, "No multipart content found or error during parsing.");
                        }

                        foreach (var mpo in multipartCollection.Values)
                        {
                            var info = new SimpleMultipartObjectInfo
                            {
                                Name = mpo.Name,
                                FileName = mpo.Filename,
                                ContentType = mpo.ContentType,
                                Length = mpo.ContentBytes?.Length ?? 0,

                                PartHeaders = []
                            };
                            if (mpo.Headers != null)
                            {
                                foreach (var headerKey in mpo.Headers.Keys)
                                {
                                    if (headerKey != null)
                                    {
                                        info.PartHeaders[headerKey] = mpo.Headers[headerKey];
                                    }
                                }
                            }

                            Encoding partEncoding = Encoding.UTF8;
                            string? charset = null;
                            if (!string.IsNullOrEmpty(mpo.ContentType))
                            {
                                try
                                {
                                    var mediaType = new System.Net.Mime.ContentType(mpo.ContentType);
                                    if (!string.IsNullOrEmpty(mediaType.CharSet))
                                    {
                                        charset = mediaType.CharSet;
                                        partEncoding = Encoding.GetEncoding(charset);
                                    }
                                }
                                catch (Exception) { /* Ignore invalid ContentType or charset, use default UTF-8 */ }
                            }
                            else if (!string.IsNullOrEmpty(mpo.ContentType))
                            {
                                try
                                {
                                    var mediaType = new System.Net.Mime.ContentType(mpo.ContentType);
                                    if (!string.IsNullOrEmpty(mediaType.CharSet))
                                    {
                                        charset = mediaType.CharSet;
                                        partEncoding = Encoding.GetEncoding(charset);
                                    }
                                }
                                catch (Exception) { /* Ignore invalid ContentType or charset, use default UTF-8 */ }
                            }

                            if (mpo.IsFile)
                            {
                                info.Value = null;
                                if (mpo.ContentBytes != null)
                                {
                                    bool isTextContent = false;
                                    if (charset != null)
                                    {
                                        isTextContent = true;
                                    }
                                    else if (mpo.ContentType != null)
                                    {
                                        string cTypeLower = mpo.ContentType.ToLowerInvariant();
                                        isTextContent = cTypeLower.StartsWith("text/") ||
                                                        cTypeLower.Contains("json") ||
                                                        cTypeLower.Contains("xml") ||
                                                        cTypeLower.Contains("html") ||
                                                        cTypeLower.Contains("javascript");
                                    }

                                    if (isTextContent)
                                    {
                                        info.ContentPreview = partEncoding.GetString(mpo.ContentBytes);
                                    }
                                    else
                                    {
                                        info.ContentPreview = Convert.ToBase64String(mpo.ContentBytes);
                                    }
                                }
                            }
                            else
                            {
                                info.Value = mpo.ReadContentAsString(partEncoding);
                                info.ContentPreview = info.Value;
                            }
                            result.Add(info);
                        }
                        return new HttpResponse(JsonContent.Create(result));
                    }
                    catch (Sisk.Core.Http.HttpRequestException httpReqEx)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {httpReqEx.Message}");
                    }
                    catch (Exception ex)
                    {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error processing multipart request: {ex.Message}");
                    }
                });

                // SSE Routes
                router.SetRoute(RouteMethod.Get, "/tests/sse/sync", (req) =>
                {
                    var eventSource = req.GetEventSource();
                    eventSource.AppendHeader("X-Test-SSE", "sync");
                    eventSource.Send("message 1 part 1");
                    eventSource.Send("message 1 part 2");
                    eventSource.Send("message 2 part 1", fieldName: "customSync");
                    eventSource.Send("message 2 part 2", fieldName: "customSync");
                    return eventSource.Close();
                });

                router.SetRoute(RouteMethod.Get, "/tests/sse/async", async (HttpRequest req) =>
                {
                    var eventSource = await req.GetEventSourceAsync();
                    eventSource.AppendHeader("X-Test-SSE", "async");
                    await eventSource.SendAsync("async message 1");
                    await Task.Delay(50);
                    await eventSource.SendAsync("async message 2", fieldName: "customAsync");
                    await Task.Delay(50);
                    await eventSource.SendAsync("async message 3");
                    return eventSource.Close();
                });

                router.SetRoute(RouteMethod.Get, "/tests/sse/cors", (req) =>
                {
                    var eventSource = req.GetEventSource();
                    eventSource.AppendHeader("X-Test-SSE", "cors");
                    eventSource.Send("cors message 1");
                    return eventSource.Close();
                });

                router.SetRoute(RouteMethod.Get, "/tests/sse/empty", (req) =>
                {
                    var eventSource = req.GetEventSource();
                    eventSource.Send(""); // Default event name "message", empty data
                    eventSource.Send(null); // Default event name "message", null data (becomes empty)
                    eventSource.Send("", fieldName: "customEmpty"); // Custom event name, empty data
                    eventSource.Send(null, fieldName: "customNull"); // Custom event name, null data (becomes empty)
                    return eventSource.Close();
                });

                // WebSocket routes
                router.MapWebSocket("/tests/ws/echo", async (HttpRequest request, WebSocket client) =>
                {
                    await client.SendTextAsync($"Connected to /tests/ws/echo. Your headers: {string.Join(", ", request.Headers.Select(h => $"{h.Key}={h.Value}"))}");
                    WebSocketReceiveResult? result;
                    do
                    {
                        var buffer = new byte[1024 * 4];
                        result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client initiated close", CancellationToken.None);
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            await client.SendTextAsync($"Echo: {receivedMessage}");
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            var binaryData = new ArraySegment<byte>(buffer, 0, result.Count);
                            await client.SendBinaryAsync(binaryData);
                        }
                    } while (!result.CloseStatus.HasValue);
                });

                router.MapWebSocket("/tests/ws/checksum", async (HttpRequest request, WebSocket client) =>
                {
                    await client.SendTextAsync("Connected to /tests/ws/checksum. Send data as binary, then checksum as text (SHA256 hex).");
                    byte[]? lastReceivedBinary = null;

                    WebSocketReceiveResult? result;
                    do
                    {
                        var buffer = new byte[1024 * 8]; // Increased buffer for potentially larger data
                        result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client initiated close", CancellationToken.None);
                        }
                        else if (result.MessageType == WebSocketMessageType.Binary)
                        {
                            lastReceivedBinary = new byte[result.Count];
                            Array.Copy(buffer, 0, lastReceivedBinary, 0, result.Count);
                            await client.SendTextAsync($"Received {result.Count} bytes. Send SHA256 checksum as text.");
                        }
                        else if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string receivedChecksum = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            if (lastReceivedBinary != null)
                            {
                                using (var sha256 = SHA256.Create())
                                {
                                    byte[] hashBytes = sha256.ComputeHash(lastReceivedBinary);
                                    string computedChecksum = BitConverter.ToString(hashBytes).Replace("-", "").ToLowerInvariant();
                                    if (computedChecksum == receivedChecksum.ToLowerInvariant())
                                    {
                                        await client.SendTextAsync("Checksum VALID");
                                    }
                                    else
                                    {
                                        await client.SendTextAsync($"Checksum INVALID. Expected: {computedChecksum}, Got: {receivedChecksum}");
                                    }
                                }
                                lastReceivedBinary = null; // Reset for next binary message
                            }
                            else
                            {
                                await client.SendTextAsync("Please send binary data first before sending a checksum.");
                            }
                        }
                    } while (!result.CloseStatus.HasValue);
                });

                router.MapWebSocket("/tests/ws/queue", async (HttpRequest request, WebSocket client) =>
                {
                    await client.SendTextAsync("Connected to /tests/ws/queue. Messages will be processed in order.");
                    var messageQueue = new Queue<string>();
                    var cts = new CancellationTokenSource();
                    bool processing = false;

                    // Start a processing task
                    _ = Task.Run(async () =>
                    {
                        processing = true;
                        while (!cts.Token.IsCancellationRequested || messageQueue.Count > 0)
                        {
                            if (messageQueue.TryDequeue(out string? messageContent))
                            {
                                await client.SendTextAsync($"Processing: {messageContent}");
                                await Task.Delay(100, cts.Token); // Simulate work
                                await client.SendTextAsync($"Processed: {messageContent}");
                            }
                            else
                            {
                                await Task.Delay(50, cts.Token); // Wait for more messages
                            }
                        }
                        processing = false;
                    }, cts.Token);

                    WebSocketReceiveResult? result;
                    do
                    {
                        var buffer = new byte[1024 * 4];
                        result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            if (receivedMessage.Equals("STOP_PROCESSING", StringComparison.OrdinalIgnoreCase))
                            {
                                cts.Cancel();
                                await client.SendTextAsync("Stopping message processing queue.");
                            }
                            else
                            {
                                messageQueue.Enqueue(receivedMessage);
                                await client.SendTextAsync($"Queued: {receivedMessage}");
                            }
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            cts.Cancel();
                            // Wait a bit for the processing task to finish up if it's active
                            for(int i=0; i < 10 && processing; ++i) await Task.Delay(100);
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client initiated close", CancellationToken.None);
                        }
                    } while (!result.CloseStatus.HasValue && !cts.IsCancellationRequested);

                    // Ensure remaining messages are processed if client didn't explicitly stop
                    if (cts.IsCancellationRequested && messageQueue.Count > 0)
                    {
                       await client.SendTextAsync($"Client disconnected, processing {messageQueue.Count} remaining messages...");
                       while (messageQueue.TryDequeue(out string? messageContent))
                       {
                           await client.SendTextAsync($"Processing: {messageContent}");
                           await Task.Delay(100); // Simulate work
                           await client.SendTextAsync($"Processed: {messageContent}");
                       }
                    }
                    if (!client.CloseStatus.HasValue)
                    {
                        await client.CloseOutputAsync(WebSocketCloseStatus.NormalClosure, "Queue processing finished.", CancellationToken.None);
                    }
                });

                router.MapWebSocket("/tests/ws/headers", async (HttpRequest request, WebSocket client) =>
                {
                    var headersDict = new Dictionary<string, string>();
                    foreach (var header in request.Headers)
                    {
                        if (header.Value != null)
                           headersDict[header.Key] = header.Value;
                    }
                    // Sisk.Core.Http.JsonContent requires a concrete type for serialization or an object.
                    // Using Dictionary<string, string> is fine.
                    var jsonContent = JsonContent.Create(headersDict);
                    string headersJson = Encoding.UTF8.GetString(jsonContent.WriteToByteArray());
                    await client.SendTextAsync(headersJson);
                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Headers sent", CancellationToken.None);
                });

                router.MapWebSocket("/tests/ws/async-server", async (HttpRequest request, WebSocket client) =>
                {
                    await client.SendTextAsync("Connected to /tests/ws/async-server. Server will handle messages asynchronously.");

                    WebSocketReceiveResult? result;
                    do
                    {
                        var buffer = new byte[1024 * 4];
                        result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);

                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            // Don't await this task, let it run in the background
                            _ = Task.Run(async () => {
                                await Task.Delay(500); // Simulate async work
                                await client.SendTextAsync($"Async response to: {receivedMessage}");
                            });
                        }
                        else if (result.MessageType == WebSocketMessageType.Close)
                        {
                            await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client initiated close", CancellationToken.None);
                        }
                    } while (!result.CloseStatus.HasValue);
                });

                router.MapWebSocket("/tests/ws/disconnect", async (HttpRequest request, WebSocket client) =>
                {
                    // This handler is primarily for testing server-side behavior on client disconnect.
                    // We can log or set a flag that a test could later verify.
                    // For now, just acknowledge connection and wait for disconnect.
                    Console.WriteLine($"[WebSocket /tests/ws/disconnect] Client {client.GetHashCode()} connected. Waiting for disconnect.");
                    try
                    {
                         WebSocketReceiveResult? result;
                         var buffer = new byte[1024];
                         do
                         {
                             result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                             if (result.MessageType == WebSocketMessageType.Text) {
                                 // Optional: echo back to confirm connection is live
                                 await client.SendTextAsync("Still connected...");
                             }
                         } while (!result.CloseStatus.HasValue);
                         Console.WriteLine($"[WebSocket /tests/ws/disconnect] Client {client.GetHashCode()} initiated disconnect with status: {result.CloseStatus}, Description: {result.CloseStatusDescription}");
                    }
                    catch (WebSocketException wsex) when (wsex.WebSocketErrorCode == WebSocketError.ConnectionClosedPrematurely || wsex.WebSocketErrorCode == WebSocketError.OperationAborted)
                    {
                        // This is where you'd handle unexpected disconnects
                        Console.WriteLine($"[WebSocket /tests/ws/disconnect] Client {client.GetHashCode()} disconnected unexpectedly. Error: {wsex.Message}");
                        // Perform cleanup tasks here if necessary
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"[WebSocket /tests/ws/disconnect] Error for client {client.GetHashCode()}: {ex.Message}");
                    }
                    finally
                    {
                         Console.WriteLine($"[WebSocket /tests/ws/disconnect] Client {client.GetHashCode()} session ended.");
                         // Ensure connection is closed if not already
                         if (client.State != WebSocketState.Closed && client.State != WebSocketState.Aborted)
                         {
                            await client.CloseAsync(WebSocketCloseStatus.InternalServerError, "Server finalizing session.", CancellationToken.None);
                         }
                    }
                });

                router.MapWebSocket("/tests/ws/subprotocol", async (HttpRequest request, WebSocket client) =>
                {
                    // Server logic to select a sub-protocol
                    string? selectedProtocol = null;
                    if (request.Headers.TryGetValue("Sec-WebSocket-Protocol", out string? clientProtocolsHeader))
                    {
                        var clientProtocols = clientProtocolsHeader?.Split(',').Select(p => p.Trim());
                        // Example: server supports "chat.v1" and "chat.v2"
                        string[] supportedServerProtocols = { "chat.v1", "chat.v2", "custom.protocol" };
                        if (clientProtocols != null)
                        {
                            foreach (var pName in clientProtocols)
                            {
                                if (supportedServerProtocols.Contains(pName, StringComparer.OrdinalIgnoreCase))
                                {
                                    selectedProtocol = pName; // Select the first supported protocol
                                    break;
                                }
                            }
                        }
                    }

                    // Important: The actual sub-protocol negotiation is handled by Sisk itself
                    // when you pass `requestedSubProtocol` to `HttpServer.UpgradeToWebSocketAsync`.
                    // Sisk's MapWebSocket does this internally if a match is found.
                    // This handler just needs to know *which* protocol was selected to behave accordingly.
                    // The selected protocol is available in `client.SubProtocol`.

                    if (!string.IsNullOrEmpty(client.SubProtocol))
                    {
                        await client.SendTextAsync($"Sub-protocol '{client.SubProtocol}' negotiated and selected.");
                    }
                    else
                    {
                        await client.SendTextAsync("No common sub-protocol negotiated, or client did not request one.");
                    }

                    // Echo functionality for testing with the negotiated protocol
                    WebSocketReceiveResult? result;
                    do
                    {
                        var buffer = new byte[1024 * 4];
                        result = await client.ReceiveAsync(new ArraySegment<byte>(buffer), CancellationToken.None);
                        if (result.MessageType == WebSocketMessageType.Text)
                        {
                            string receivedMessage = Encoding.UTF8.GetString(buffer, 0, result.Count);
                            await client.SendTextAsync($"({client.SubProtocol ?? "no-protocol"}): {receivedMessage}");
                        }
                    } while (!result.CloseStatus.HasValue);

                    await client.CloseAsync(WebSocketCloseStatus.NormalClosure, "Sub-protocol test finished", CancellationToken.None);
                });
            })
            .Build();

        Instance.Start(verbose: false, preventHault: false);
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup()
    {
        Instance.Dispose();
    }
}

// Helper class for a non-seekable stream wrapper
public class NonSeekableStreamWrapper : System.IO.Stream
{
    private readonly System.IO.Stream _innerStream;
    public NonSeekableStreamWrapper(System.IO.Stream innerStream) { _innerStream = innerStream; }
    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => false; // The key difference
    public override bool CanWrite => _innerStream.CanWrite;
    // Length and Position might throw NotSupportedException if CanSeek is false for many stream consumers.
    // HttpContent's behavior with these for non-seekable streams is what we are implicitly testing.
    public override long Length => _innerStream.CanSeek ? _innerStream.Length : throw new NotSupportedException();
    public override long Position { get => _innerStream.CanSeek ? _innerStream.Position : throw new NotSupportedException(); set { if (!_innerStream.CanSeek) throw new NotSupportedException(); _innerStream.Position = value; } }
    public override void Flush() => _innerStream.Flush();
    public override int Read(byte[] buffer, int offset, int count) => _innerStream.Read(buffer, offset, count);
    public override long Seek(long offset, System.IO.SeekOrigin origin) => throw new NotSupportedException();
    public override void SetLength(long value) => throw new NotSupportedException();
    public override void Write(byte[] buffer, int offset, int count) => _innerStream.Write(buffer, offset, count);
    protected override void Dispose(bool disposing) { if (disposing) { _innerStream.Dispose(); } base.Dispose(disposing); }
}

// Helper DTO for echoing multipart data, as MultipartObject itself might be complex to serialize directly for client assertion.
// This DTO is defined here for Server.cs to use. A compatible one is in HttpRequestTests.cs for the client.
public class SimpleMultipartObjectInfo
{
    public string? Name { get; set; }
    public string? Value { get; set; } // For text fields
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long Length { get; set; } // Length of the content bytes
    public string? ContentPreview { get; set; } // Optional: small preview for text-based files or Base64 for binary
    public Dictionary<string, string?>? PartHeaders { get; set; }
}
