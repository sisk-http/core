using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;

namespace tests;

[TestClass]
public sealed class Server {
    public static HttpServerHostContext Instance = null!;

    public static HttpClient GetHttpClient () => new HttpClient () { BaseAddress = new Uri ( Instance.HttpServer.ListeningPrefixes [ 0 ] ) };

    [AssemblyInitialize]
    public static void AssemblyInit ( TestContext testContext ) {

        Instance = HttpServer.CreateBuilder ()
            .UseRouter ( router => {

                router.MapGet ( "/tests/plaintext", delegate ( HttpRequest request ) {
                    return new HttpResponse () {
                        Content = new StringContent ( "Hello, world!", Encoding.UTF8, "text/plain" ),
                        Status = HttpStatusInformation.Ok
                    };
                } );
                router.MapGet ( "/tests/plaintext/chunked", delegate ( HttpRequest request ) {
                    return new HttpResponse () {
                        Content = new StringContent ( "Hello, world!", Encoding.UTF8, "text/plain" ),
                        Status = HttpStatusInformation.Ok,
                        SendChunked = true
                    };
                } );

                router.SetRoute(HttpMethod.Get, "/tests/bytearray", (req) => {
                    byte[] byteArray = Encoding.UTF8.GetBytes("This is a Sisk byte array response.");
                    // Use Sisk.Core.Http.ByteArrayContent
                    var content = new Sisk.Core.Http.ByteArrayContent(byteArray, "application/custom-binary");
                    return new HttpResponse(content);
                });

                router.SetRoute(HttpMethod.Get, "/tests/bytearray/defaulted", (req) => {
                    byte[] byteArray = Encoding.UTF8.GetBytes("Defaulted Sisk byte array.");
                    // Use Sisk.Core.Http.ByteArrayContent
                    var content = new Sisk.Core.Http.ByteArrayContent(byteArray); // Defaults to application/octet-stream
                    return new HttpResponse(content);
                });

                router.SetRoute(HttpMethod.Get, "/tests/htmlcontent", (req) => {
                    string htmlString = "<html><body><h1>Hello from Sisk HtmlContent</h1></body></html>";
                    // Use Sisk.Core.Http.HtmlContent
                    var content = new Sisk.Core.Http.HtmlContent(htmlString);
                    // HtmlContent should default to text/html; charset=utf-8
                    return new HttpResponse(content);
                });

                router.SetRoute(HttpMethod.Get, "/tests/htmlcontent/customchar", (req) => {
                    string htmlString = "<html><body><h1>Custom Charset</h1></body></html>";
                    // Use Sisk.Core.Http.HtmlContent, specifying a different encoding
                    var content = new Sisk.Core.Http.HtmlContent(htmlString, System.Text.Encoding.ASCII);
                    // The Content-Type header should reflect this: text/html; charset=us-ascii
                    return new HttpResponse(content);
                });

                router.SetRoute(HttpMethod.Get, "/tests/streamcontent/seekable", (req) => {
                    string streamData = "This is data from a seekable stream.";
                    byte[] streamBytes = Encoding.UTF8.GetBytes(streamData);
                    var memoryStream = new System.IO.MemoryStream(streamBytes); // MemoryStream is seekable
                    var content = new Sisk.Core.Http.StreamContent(memoryStream, "text/plain; charset=utf-8");
                    return new HttpResponse(content);
                });

                router.SetRoute(HttpMethod.Get, "/tests/streamcontent/nonseekable", (req) => {
                    string streamData = "Data from a non-seekable stream.";
                    byte[] streamBytes = Encoding.UTF8.GetBytes(streamData);
                    var memoryStream = new System.IO.MemoryStream(streamBytes);
                    var nonSeekableStream = new NonSeekableStreamWrapper(memoryStream);
                    var content = new Sisk.Core.Http.StreamContent(nonSeekableStream, "application/octet-stream");
                    return new HttpResponse(content);
                });

                router.SetRoute(HttpMethod.Get, "/tests/streamcontent/predefinedlength", (req) => {
                    string streamData = "Data from stream with predefined length.";
                    byte[] streamBytes = Encoding.UTF8.GetBytes(streamData);
                    var memoryStream = new System.IO.MemoryStream(streamBytes);
                    var content = new Sisk.Core.Http.StreamContent(memoryStream, (long)streamBytes.Length, "text/example");
                    return new HttpResponse(content);
                });

                router.SetRoute(HttpMethod.Get, "/tests/responsestream/simple", (req) => {
                    // Mark the request as handled to prevent the server from sending a default response,
                    // as we are writing directly to the stream.
                    req.Context.Result = Sisk.Core.Routing.RequestHandlerResult.CreateAsHandled();

                    Sisk.Core.Http.HttpResponseStreamManager responseStreamManager = req.GetResponseStream();

                    string responseBody = "Hello from GetResponseStream!";
                    byte[] responseBytes = Encoding.UTF8.GetBytes(responseBody);

                    var headers = new Sisk.Core.Entity.HttpHeaderCollection();
                    headers.Add(Sisk.Core.Http.HttpKnownHeaderNames.ContentType, "text/plain; charset=utf-8");
                    headers.Add(Sisk.Core.Http.HttpKnownHeaderNames.ContentLength, responseBytes.Length.ToString());

                    responseStreamManager.WriteHead(System.Net.HttpStatusCode.OK, headers);

                    // Write directly to the HttpListenerResponse's output stream.
                    responseStreamManager.UnderlyingResponse.OutputStream.Write(responseBytes, 0, responseBytes.Length);

                    // Close the output stream to finalize the response.
                    responseStreamManager.UnderlyingResponse.OutputStream.Close();

                    // This return is important if the router expects an object;
                    // otherwise, if it's void/Task, direct stream manipulation is enough
                    // once marked as handled. Given current router setup, this is good practice.
                    return Sisk.Core.Routing.RouteAction.RequestHandledIndicator;
                });

                router.SetRoute(HttpMethod.Get, "/tests/responsestream/chunked", (req) => {
                    req.Context.Result = Sisk.Core.Routing.RequestHandlerResult.CreateAsHandled();
                    Sisk.Core.Http.HttpResponseStreamManager responseStreamManager = req.GetResponseStream();

                    var headers = new Sisk.Core.Entity.HttpHeaderCollection();
                    headers.Add(Sisk.Core.Http.HttpKnownHeaderNames.ContentType, "text/plain; charset=utf-8");
                    // No Content-Length for chunked responses.
                    // HttpResponseStreamManager.Write will handle chunking.

                    responseStreamManager.WriteHead(System.Net.HttpStatusCode.OK, headers);

                    byte[] chunk1Bytes = Encoding.UTF8.GetBytes("This is the first chunk. ");
                    responseStreamManager.Write(chunk1Bytes, false); // isLast = false

                    byte[] chunk2Bytes = Encoding.UTF8.GetBytes("This is the second chunk. ");
                    responseStreamManager.Write(chunk2Bytes, false); // isLast = false

                    byte[] chunk3Bytes = Encoding.UTF8.GetBytes("This is the final chunk.");
                    responseStreamManager.Write(chunk3Bytes, true);  // isLast = true, this will send the 0-length final chunk and close.

                    return Sisk.Core.Routing.RouteAction.RequestHandledIndicator;
                });

                // Routes for HttpRequest body reading tests
                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getBodyContents", (req) => {
                    // Server-side: Use GetBodyContents() to read the request body
                    byte[] bodyBytes = req.GetBodyContents();
                    // Echo the body back in the response
                    return new HttpResponse(new Sisk.Core.Http.ByteArrayContent(bodyBytes, req.Headers[Sisk.Core.Http.HttpKnownHeaderNames.ContentType] ?? "application/octet-stream"));
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getBodyContentsAsync", async (req) => {
                    // Server-side: Use GetBodyContentsAsync() to read the request body
                    Memory<byte> bodyMemory = await req.GetBodyContentsAsync();
                    // Echo the body back in the response
                    return new HttpResponse(new Sisk.Core.Http.ByteArrayContent(bodyMemory.ToArray(), req.Headers[Sisk.Core.Http.HttpKnownHeaderNames.ContentType] ?? "application/octet-stream"));
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/rawBody", (req) => {
                    // Server-side: Use RawBody property to read the request body
                    byte[] bodyBytes = req.RawBody;
                    // Echo the body back in the response
                    return new HttpResponse(new Sisk.Core.Http.ByteArrayContent(bodyBytes, req.Headers[Sisk.Core.Http.HttpKnownHeaderNames.ContentType] ?? "application/octet-stream"));
                });

                // Routes for HttpRequest JSON content reading tests
                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getJsonContent", (req) => {
                    try {
                        var poco = req.GetJsonContent<tests.Tests.TestPoco>();
                        if (poco != null) {
                            return new HttpResponse(new Sisk.Core.Http.JsonContent(poco));
                        }
                        // Sisk's GetJsonContent returns null for empty or non-JSON content if deserialization fails without throwing for some types.
                        // Let's ensure a JsonContent(null) is returned for client to check.
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(null));
                    } catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException: {ex.Message}");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getJsonContentAsync", async (req) => {
                    try {
                        var poco = await req.GetJsonContentAsync<tests.Tests.TestPoco>();
                        if (poco != null) {
                            return new HttpResponse(new Sisk.Core.Http.JsonContent(poco));
                        }
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(null));
                    } catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException: {ex.Message}");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getJsonContentWithOptions", (req) => {
                    try {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var poco = req.GetJsonContent<tests.Tests.TestPoco>(options);
                        if (poco != null) {
                            return new HttpResponse(new Sisk.Core.Http.JsonContent(poco));
                        }
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(null));
                    } catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException (options): {ex.Message}");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getJsonContentAsyncWithOptions", async (req) => {
                    try {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var poco = await req.GetJsonContentAsync<tests.Tests.TestPoco>(options);
                        if (poco != null) {
                            return new HttpResponse(new Sisk.Core.Http.JsonContent(poco));
                        }
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(null));
                    } catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"JsonException (options, async): {ex.Message}");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest form content reading tests
                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getFormContent", (req) => {
                    try {
                        Sisk.Core.Entity.StringKeyStoreCollection form = req.GetFormContent();
                        var dictionary = new System.Collections.Generic.Dictionary<string, string?>();
                        if (form != null) {
                            foreach (string? key in form.AllKeys) {
                                if (key != null) {
                                    dictionary[key] = form[key]?.Value; // Access .Value from StringValue
                                }
                            }
                        }
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(dictionary));
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getFormContentAsync", async (req) => {
                    try {
                        Sisk.Core.Entity.StringKeyStoreCollection form = await req.GetFormContentAsync();
                        var dictionary = new System.Collections.Generic.Dictionary<string, string?>();
                        if (form != null) {
                            foreach (string? key in form.AllKeys) {
                                if (key != null) {
                                    dictionary[key] = form[key]?.Value; // Access .Value from StringValue
                                }
                            }
                        }
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(dictionary));
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest multipart form content reading tests
                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getMultipartFormContent", (req) => {
                    try {
                        Sisk.Core.Entity.MultipartFormCollection multipartCollection = req.GetMultipartFormContent();
                        var simplifiedResult = multipartCollection
                            .Select(mpo => new SimpleMultipartObjectInfo {
                                Name = mpo.Name,
                                Value = mpo.IsFile ? null : mpo.Value,
                                FileName = mpo.IsFile ? mpo.FileName : null,
                                ContentType = mpo.ContentType,
                                Length = mpo.ContentBytes?.Length ?? 0,
                                ContentPreview = mpo.IsFile && (mpo.ContentType?.StartsWith("text/") == true) && mpo.ContentBytes != null
                                                 ? Encoding.UTF8.GetString(mpo.ContentBytes.Take(100).ToArray())
                                                 : (mpo.IsFile ? null : mpo.Value)
                            })
                            .ToList();
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(simplifiedResult));
                    } catch (Sisk.Core.Http.HttpRequestException ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {ex.Message}");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getMultipartFormContentAsync", async (req) => {
                    try {
                        Sisk.Core.Entity.MultipartFormCollection multipartCollection = await req.GetMultipartFormContentAsync();
                        var simplifiedResult = multipartCollection
                            .Select(mpo => new SimpleMultipartObjectInfo {
                                Name = mpo.Name,
                                Value = mpo.IsFile ? null : mpo.Value,
                                FileName = mpo.IsFile ? mpo.FileName : null,
                                ContentType = mpo.ContentType,
                                Length = mpo.ContentBytes?.Length ?? 0,
                                ContentPreview = mpo.IsFile && (mpo.ContentType?.StartsWith("text/") == true) && mpo.ContentBytes != null
                                                 ? Encoding.UTF8.GetString(mpo.ContentBytes.Take(100).ToArray())
                                                 : (mpo.IsFile ? null : mpo.Value)
                            })
                            .ToList();
                        return new HttpResponse(new Sisk.Core.Http.JsonContent(simplifiedResult));
                    } catch (Sisk.Core.Http.HttpRequestException ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {ex.Message}");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest GetRequestStream tests
                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getRequestStream/read", async (req) => {
                    try {
                        using (System.IO.Stream requestStream = req.GetRequestStream())
                        using (var reader = new System.IO.StreamReader(requestStream, Encoding.UTF8))
                        {
                            string content = await reader.ReadToEndAsync();
                            // Echo back the content read from the stream
                            return new HttpResponse(new Sisk.Core.Http.StringContent(content, Encoding.UTF8));
                        }
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getRequestStream/empty", async (req) => {
                    try {
                        using (System.IO.Stream requestStream = req.GetRequestStream())
                        using (var reader = new System.IO.StreamReader(requestStream, Encoding.UTF8))
                        {
                            string content = await reader.ReadToEndAsync();
                            if (string.IsNullOrEmpty(content)) {
                                return new HttpResponse(System.Net.HttpStatusCode.OK, "Stream was empty as expected.");
                            }
                            return new HttpResponse(System.Net.HttpStatusCode.BadRequest, "Stream was not empty.");
                        }
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}");
                    }
                });

                // Routes for HttpRequest GetRequestStream tests (after body consumption)
                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getRequestStream/afterGetBodyContents", (req) => {
                    try {
                        _ = req.GetBodyContents();
                        req.GetRequestStream();
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, "GetRequestStream did not throw after GetBodyContents.");
                    } catch (InvalidOperationException) {
                        return new HttpResponse(System.Net.HttpStatusCode.OK, "Caught InvalidOperationException as expected.");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getRequestStream/afterRawBody", (req) => {
                    try {
                        _ = req.RawBody;
                        req.GetRequestStream();
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, "GetRequestStream did not throw after RawBody access.");
                    } catch (InvalidOperationException) {
                        return new HttpResponse(System.Net.HttpStatusCode.OK, "Caught InvalidOperationException as expected.");
                    } catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}");
                    }
                });

                router.SetRoute(HttpMethod.Post, "/tests/httprequest/getRequestStream/afterGetJsonContent", (req) => {
                    try {
                        // Assuming tests.Tests.TestPoco is accessible or a compatible DTO is used.
                        _ = req.GetJsonContent<tests.Tests.TestPoco>();

                        using (System.IO.Stream requestStream = req.GetRequestStream()) {
                            byte[] buffer = new byte[10];
                            int bytesRead = requestStream.Read(buffer, 0, buffer.Length);
                            if (bytesRead == 0) {
                                var contentBytesField = typeof(Sisk.Core.Http.HttpRequest).GetField("contentBytes",
                                    System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance);
                                object? contentBytesValue = contentBytesField?.GetValue(req);

                                if (contentBytesValue == null) {
                                     return new HttpResponse(System.Net.HttpStatusCode.OK, "Stream returned and was consumed (0 bytes read), contentBytes is null.");
                                } else {
                                    return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, "Stream returned and was consumed, but contentBytes was not null.");
                                }
                            }
                            return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Stream returned but was not fully consumed (read {bytesRead} bytes).");
                        }
                    } catch (InvalidOperationException ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected InvalidOperationException: {ex.Message}");
                    } catch (System.Text.Json.JsonException jEx){
                         return new HttpResponse(System.Net.HttpStatusCode.BadRequest, $"Json body error: {jEx.Message}");
                    }
                    catch (Exception ex) {
                        return new HttpResponse(System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}");
                    }
                });
            } )
            .Build ();

        Instance.Start ( verbose: false, preventHault: false );
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup () {
        Instance.Dispose ();
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
    public string? ContentPreview { get; set; } // Optional: small preview for text-based files
}
