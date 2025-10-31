// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   Server.cs
// Repository:  https://github.com/sisk-http/core

using System.Net.WebSockets;
using System.Security.Cryptography;
using System.Text;
using Sisk.Cadente.CoreEngine;
using Sisk.Core.Entity;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Http.Streams;
using Sisk.Core.Routing;

namespace tests;

[TestClass]
public sealed class Server {
    public static HttpServerHostContext Instance = null!;

    public static HttpClient GetHttpClient () => new HttpClient () {
        BaseAddress = new Uri ( Instance.HttpServer.ListeningPrefixes [ 0 ] ),
        Timeout = TimeSpan.FromMinutes ( 10 )
    };

    [AssemblyInitialize]
    public static void AssemblyInit ( TestContext testContext ) {
        var builder = HttpServer.CreateBuilder ()
            .UseCors ( new CrossOriginResourceSharingHeaders ( allowOrigin: "*", allowMethods: [ "GET", "POST", "PUT" ] ) );

        if (string.Equals ( Environment.GetEnvironmentVariable ( "SISK_TEST_ENGINE" ), "Cadente", StringComparison.OrdinalIgnoreCase )) {
            Console.WriteLine ( "Using Cadente test engine." );
            builder.UseEngine ( new CadenteHttpServerEngine () );
        }

        Instance = builder
            .UseConfiguration ( config => {
                config.ThrowExceptions = true;
                config.ErrorsLogsStream = LogStream.ConsoleOutput;
            } )
            .UseRouter ( router => {
                // Original HTTP Routes
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

                router.SetRoute ( RouteMethod.Get, "/tests/bytearray", ( req ) => {
                    byte [] byteArray = Encoding.UTF8.GetBytes ( "This is a Sisk byte array response." );
                    var content = new ByteArrayContent ( byteArray );
                    return new HttpResponse ( content ) { Headers = new () { ContentType = "application/custom-binary" } };
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/bytearray/defaulted", ( req ) => {
                    byte [] byteArray = Encoding.UTF8.GetBytes ( "Defaulted Sisk byte array." );
                    var content = new ByteArrayContent ( byteArray );
                    return new HttpResponse ( content );
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/htmlcontent", ( req ) => {
                    string htmlString = "<html><body><h1>Hello from Sisk HtmlContent</h1></body></html>";
                    var content = new Sisk.Core.Http.HtmlContent ( htmlString );
                    return new HttpResponse ( content );
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/htmlcontent/customchar", ( req ) => {
                    string htmlString = "<html><body><h1>Custom Charset</h1></body></html>";
                    var content = new HtmlContent ( htmlString, System.Text.Encoding.ASCII );
                    return new HttpResponse ( content );
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/streamcontent/seekable", ( req ) => {
                    string streamData = "This is data from a seekable stream.";
                    byte [] streamBytes = Encoding.UTF8.GetBytes ( streamData );
                    var memoryStream = new System.IO.MemoryStream ( streamBytes );
                    var content = new StreamContent ( memoryStream );
                    return new HttpResponse ( content );
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/streamcontent/nonseekable", ( req ) => {
                    string streamData = "Data from a non-seekable stream.";
                    byte [] streamBytes = Encoding.UTF8.GetBytes ( streamData );
                    var memoryStream = new System.IO.MemoryStream ( streamBytes );
                    var nonSeekableStream = new NonSeekableStreamWrapper ( memoryStream );
                    var content = new StreamContent ( nonSeekableStream );
                    return new HttpResponse ( content );
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/streamcontent/predefinedlength", ( req ) => {
                    string streamData = "Data from stream with predefined length.";
                    byte [] streamBytes = Encoding.UTF8.GetBytes ( streamData );
                    var memoryStream = new System.IO.MemoryStream ( streamBytes );
                    var content = new StreamContent ( memoryStream );
                    return new HttpResponse ( content ) { Headers = new () { ContentType = "text/example" } };
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/responsestream/simple", ( req ) => {
                    var responseStreamManager = req.GetResponseStream ();
                    string responseBody = "Hello from GetResponseStream!";
                    byte [] responseBytes = Encoding.UTF8.GetBytes ( responseBody );
                    responseStreamManager.SetStatus ( System.Net.HttpStatusCode.OK );
                    responseStreamManager.SetHeader ( HttpKnownHeaderNames.ContentType, "text/plain; charset=utf-8" );
                    responseStreamManager.SetContentLength ( responseBytes.Length );
                    responseStreamManager.Write ( responseBytes, 0, responseBytes.Length );
                    return responseStreamManager.Close ();
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/responsestream/chunked", ( req ) => {
                    var responseStreamManager = req.GetResponseStream ();
                    responseStreamManager.SetStatus ( System.Net.HttpStatusCode.OK );
                    responseStreamManager.SetHeader ( HttpKnownHeaderNames.ContentType, "text/plain; charset=utf-8" );
                    byte [] chunk1Bytes = Encoding.UTF8.GetBytes ( "This is the first chunk. " );
                    responseStreamManager.Write ( chunk1Bytes );
                    byte [] chunk2Bytes = Encoding.UTF8.GetBytes ( "This is the second chunk. " );
                    responseStreamManager.Write ( chunk2Bytes );
                    byte [] chunk3Bytes = Encoding.UTF8.GetBytes ( "This is the final chunk." );
                    responseStreamManager.Write ( chunk3Bytes );
                    return responseStreamManager.Close ();
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getBodyContents", ( req ) => {
                    byte [] bodyBytes = req.GetBodyContents ();
                    return new HttpResponse ( new ByteArrayContent ( bodyBytes ) );
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getBodyContentsAsync", async ( HttpRequest req ) => {
                    Memory<byte> bodyMemory = await req.GetBodyContentsAsync ();
                    return new HttpResponse ( new ByteArrayContent ( bodyMemory.ToArray () ) );
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/rawBody", ( req ) => {
                    byte [] bodyBytes = req.RawBody;
                    return new HttpResponse ( new ByteArrayContent ( bodyBytes ) );
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getJsonContent", ( req ) => {
                    try {
                        var poco = req.GetJsonContent<tests.Tests.TestPoco> ();
                        if (poco != null) {
                            string jsonString = System.Text.Json.JsonSerializer.Serialize ( poco );
                            return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                        }
                        return new HttpResponse ( "null" );
                    }
                    catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"JsonException: {ex.Message}" );
                    }
                    catch (Exception ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" );
                    }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getJsonContentAsync", async ( HttpRequest req ) => {
                    try {
                        var poco = await req.GetJsonContentAsync<tests.Tests.TestPoco> ();
                        if (poco != null) {
                            string jsonString = System.Text.Json.JsonSerializer.Serialize ( poco );
                            return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                        }
                        return new HttpResponse ( "null" );
                    }
                    catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"JsonException: {ex.Message}" );
                    }
                    catch (Exception ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" );
                    }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getJsonContentWithOptions", ( req ) => {
                    try {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var poco = req.GetJsonContent<tests.Tests.TestPoco> ( options );
                        if (poco != null) {
                            string jsonString = System.Text.Json.JsonSerializer.Serialize ( poco, options );
                            return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                        }
                        return new HttpResponse ( "null" );
                    }
                    catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"JsonException (options): {ex.Message}" );
                    }
                    catch (Exception ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" );
                    }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getJsonContentAsyncWithOptions", async ( HttpRequest req ) => {
                    try {
                        var options = new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true };
                        var poco = await req.GetJsonContentAsync<tests.Tests.TestPoco> ( options );
                        if (poco != null) {
                            string jsonString = System.Text.Json.JsonSerializer.Serialize ( poco, options );
                            return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                        }
                        return new HttpResponse ( "null" );
                    }
                    catch (System.Text.Json.JsonException ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"JsonException (options, async): {ex.Message}" );
                    }
                    catch (Exception ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" );
                    }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getFormContent", ( req ) => {
                    try {
                        Sisk.Core.Entity.StringKeyStoreCollection form = req.GetFormContent ();
                        var dictionary = form.ToDictionary ( kv => kv.Key, kv => kv.Value.FirstOrDefault () );
                        string jsonString = System.Text.Json.JsonSerializer.Serialize ( dictionary );
                        return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                    }
                    catch (Exception ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" );
                    }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getFormContentAsync", async ( HttpRequest req ) => {
                    try {
                        Sisk.Core.Entity.StringKeyStoreCollection form = await req.GetFormContentAsync ();
                        var dictionary = form.ToDictionary ( kv => kv.Key, kv => kv.Value.FirstOrDefault () );
                        string jsonString = System.Text.Json.JsonSerializer.Serialize ( dictionary );
                        return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                    }
                    catch (Exception ex) {
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" );
                    }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getMultipartFormContent", ( req ) => {
                    try {
                        Sisk.Core.Entity.MultipartFormCollection multipartCollection = req.GetMultipartFormContent ();
                        var simplifiedResult = multipartCollection.Values.Select ( mpo => new SimpleMultipartObjectInfo { Name = mpo.Name, Value = mpo.IsFile ? null : mpo.ReadContentAsString (), FileName = mpo.IsFile ? mpo.Filename : null, ContentType = mpo.ContentType, Length = mpo.ContentBytes?.Length ?? 0, ContentPreview = mpo.IsFile && (mpo.ContentType?.StartsWith ( "text/" ) == true) && mpo.ContentBytes != null ? Encoding.UTF8.GetString ( mpo.ContentBytes.Take ( 100 ).ToArray () ) : (mpo.IsFile ? null : mpo.ReadContentAsString ()) } ).ToList ();
                        string jsonString = System.Text.Json.JsonSerializer.Serialize ( simplifiedResult );
                        return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                    }
                    catch (Sisk.Core.Http.HttpRequestException ex) { return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {ex.Message}" ); }
                    catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" ); }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getMultipartFormContentAsync", async ( HttpRequest req ) => {
                    try {
                        Sisk.Core.Entity.MultipartFormCollection multipartCollection = await req.GetMultipartFormContentAsync ();
                        var simplifiedResult = multipartCollection.Values.Select ( mpo => new SimpleMultipartObjectInfo { Name = mpo.Name, Value = mpo.IsFile ? null : mpo.ReadContentAsString (), FileName = mpo.IsFile ? mpo.Filename : null, ContentType = mpo.ContentType, Length = mpo.ContentBytes?.Length ?? 0, ContentPreview = mpo.IsFile && (mpo.ContentType?.StartsWith ( "text/" ) == true) && mpo.ContentBytes != null ? Encoding.UTF8.GetString ( mpo.ContentBytes.Take ( 100 ).ToArray () ) : (mpo.IsFile ? null : mpo.ReadContentAsString ()) } ).ToList ();
                        string jsonString = System.Text.Json.JsonSerializer.Serialize ( simplifiedResult );
                        return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) );
                    }
                    catch (Sisk.Core.Http.HttpRequestException ex) { return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {ex.Message}" ); }
                    catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" ); }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getRequestStream/read", async ( HttpRequest req ) => {
                    try {
                        using (System.IO.Stream requestStream = req.GetRequestStream ())
                        using (var reader = new System.IO.StreamReader ( requestStream, Encoding.UTF8 )) {
                            string content = await reader.ReadToEndAsync ();
                            return new HttpResponse ( new StringContent ( content, Encoding.UTF8 ) );
                        }
                    }
                    catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" ); }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getRequestStream/empty", async ( HttpRequest req ) => {
                    try {
                        using (System.IO.Stream requestStream = req.GetRequestStream ())
                        using (var reader = new System.IO.StreamReader ( requestStream, Encoding.UTF8 )) {
                            
                            string content = await reader.ReadToEndAsync ();
                            if (string.IsNullOrEmpty ( content ))
                                return new HttpResponse ( System.Net.HttpStatusCode.OK, "Stream was empty as expected." );
                            return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, "Stream was not empty." );
                        }
                    }
                    catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error: {ex.Message}" ); }
                } );

                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getRequestStream/afterGetBodyContents", ( req ) => { try { _ = req.GetBodyContents (); req.GetRequestStream (); return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, "GetRequestStream did not throw after GetBodyContents." ); } catch (InvalidOperationException) { return new HttpResponse ( System.Net.HttpStatusCode.OK, "Caught InvalidOperationException as expected." ); } catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}" ); } } );
                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getRequestStream/afterRawBody", ( req ) => { try { _ = req.RawBody; req.GetRequestStream (); return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, "GetRequestStream did not throw after RawBody access." ); } catch (InvalidOperationException) { return new HttpResponse ( System.Net.HttpStatusCode.OK, "Caught InvalidOperationException as expected." ); } catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}" ); } } );
                router.SetRoute ( RouteMethod.Post, "/tests/httprequest/getRequestStream/afterGetJsonContent", ( req ) => { try { _ = req.GetJsonContent<tests.Tests.TestPoco> (); using (System.IO.Stream requestStream = req.GetRequestStream ()) { byte [] buffer = new byte [ 10 ]; int bytesRead = requestStream.Read ( buffer, 0, buffer.Length ); if (bytesRead == 0) { var contentBytesField = typeof ( Sisk.Core.Http.HttpRequest ).GetField ( "contentBytes", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance ); object? contentBytesValue = contentBytesField?.GetValue ( req ); if (contentBytesValue == null) return new HttpResponse ( System.Net.HttpStatusCode.OK, "Stream returned and was consumed (0 bytes read), contentBytes is null." ); else return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, "Stream returned and was consumed, but contentBytes was not null." ); } return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Stream returned but was not fully consumed (read {bytesRead} bytes)." ); } } catch (InvalidOperationException ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Unexpected InvalidOperationException: {ex.Message}" ); } catch (System.Text.Json.JsonException jEx) { return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"Json body error: {jEx.Message}" ); } catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Unexpected server error: {ex.Message}" ); } } );
                router.SetRoute ( RouteMethod.Post, "/tests/multipart/echo", async ( HttpRequest req ) => { try { Sisk.Core.Entity.MultipartFormCollection multipartCollection = await req.GetMultipartFormContentAsync (); var result = new List<SimpleMultipartObjectInfo> (); if (multipartCollection == null) return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, "No multipart content found or error during parsing." ); foreach (var mpo in multipartCollection.Values) { var info = new SimpleMultipartObjectInfo { Name = mpo.Name, FileName = mpo.Filename, ContentType = mpo.ContentType, Length = mpo.ContentBytes?.Length ?? 0, PartHeaders = [] }; if (mpo.Headers != null) foreach (var headerKey in mpo.Headers.Keys) if (headerKey != null && mpo.Headers [ headerKey ] != null) info.PartHeaders [ headerKey ] = string.Join ( ", ", mpo.Headers.GetValues ( headerKey ) ?? [] ); Encoding partEncoding = Encoding.UTF8; string? charset = null; if (!string.IsNullOrEmpty ( mpo.ContentType )) try { var mediaType = new System.Net.Mime.ContentType ( mpo.ContentType ); if (!string.IsNullOrEmpty ( mediaType.CharSet )) { charset = mediaType.CharSet; partEncoding = Encoding.GetEncoding ( charset ); } } catch { } if (mpo.IsFile) { info.Value = null; if (mpo.ContentBytes != null) { bool isTextContent = charset != null || (mpo.ContentType != null && (mpo.ContentType.ToLowerInvariant ().StartsWith ( "text/" ) || mpo.ContentType.ToLowerInvariant ().Contains ( "json" ) || mpo.ContentType.ToLowerInvariant ().Contains ( "xml" ) || mpo.ContentType.ToLowerInvariant ().Contains ( "html" ) || mpo.ContentType.ToLowerInvariant ().Contains ( "javascript" ))); info.ContentPreview = isTextContent ? partEncoding.GetString ( mpo.ContentBytes ) : Convert.ToBase64String ( mpo.ContentBytes ); } } else { info.Value = mpo.ReadContentAsString ( partEncoding ); info.ContentPreview = info.Value; } result.Add ( info ); } string jsonString = System.Text.Json.JsonSerializer.Serialize ( result ); return new HttpResponse ( new StringContent ( jsonString, Encoding.UTF8, "application/json" ) ); } catch (Sisk.Core.Http.HttpRequestException httpReqEx) { return new HttpResponse ( System.Net.HttpStatusCode.BadRequest, $"HttpRequestException: {httpReqEx.Message}" ); } catch (Exception ex) { return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, $"Server error processing multipart request: {ex.Message}" ); } } );

                // SSE Routes
                router.SetRoute ( RouteMethod.Get, "/tests/sse/sync", ( req ) => { var es = req.GetEventSource (); es.AppendHeader ( "X-Test-SSE", "sync" ); es.Send ( "message 1 part 1" ); es.Send ( "message 1 part 2" ); es.Send ( "message 2 part 1", fieldName: "customSync" ); es.Send ( "message 2 part 2", fieldName: "customSync" ); return es.Close (); } );
                router.SetRoute ( RouteMethod.Get, "/tests/sse/async", async ( HttpRequest req ) => { var es = await req.GetEventSourceAsync (); es.AppendHeader ( "X-Test-SSE", "async" ); await es.SendAsync ( "async message 1" ); await Task.Delay ( 50 ); await es.SendAsync ( "async message 2", fieldName: "customAsync" ); await Task.Delay ( 50 ); await es.SendAsync ( "async message 3" ); return es.Close (); } );
                router.SetRoute ( RouteMethod.Get, "/tests/sse/cors", ( req ) => { var es = req.GetEventSource (); es.AppendHeader ( "X-Test-SSE", "cors" ); es.Send ( "cors message 1" ); return es.Close (); } );
                router.SetRoute ( RouteMethod.Get, "/tests/sse/empty", ( req ) => { var es = req.GetEventSource (); es.Send ( "" ); es.Send ( null ); es.Send ( "", fieldName: "customEmpty" ); es.Send ( null, fieldName: "customNull" ); return es.Close (); } );

                // WebSocket routes
                router.SetRoute ( RouteMethod.Get, "/tests/ws/echo", async ( HttpRequest request ) => {
                    HttpWebSocket client = await request.GetWebSocketAsync ();
                    // h.Value is KeyValuePair<string, string[]> -> h.Value.Value is not valid
                    // Use string.Join to handle potential multiple values for a header key
                    await client.SendAsync ( $"Connected to /tests/ws/echo. Your headers: {string.Join ( ", ", request.Headers.Select ( h => $"{h.Key}={string.Join ( "; ", h.Value )}" ) )}" );
                    WebSocketMessage? msg;
                    do {
                        msg = await client.ReceiveMessageAsync ();
                        if (msg == null)
                            break;
                        await client.SendAsync ( msg.MessageBytes );
                    } while (msg != null);
                    return await client.CloseAsync ();
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/ws/checksum", async ( HttpRequest request ) => {
                    HttpWebSocket client = await request.GetWebSocketAsync ();
                    await client.SendAsync ( "Connected to /tests/ws/checksum. Send data as binary, then checksum as text (SHA256 hex)." );
                    byte []? lastReceivedBinary = null;
                    WebSocketMessage? msg;
                    do {
                        msg = await client.ReceiveMessageAsync ();
                        if (msg == null)
                            break;
                        if (lastReceivedBinary == null) {
                            lastReceivedBinary = msg.MessageBytes;
                            await client.SendAsync ( $"Received {lastReceivedBinary?.Length ?? 0} bytes. Send SHA256 checksum as text." );
                        }
                        else {
                            string receivedChecksum = msg.GetString ( request.RequestEncoding );
                            using (var sha256 = SHA256.Create ()) {
                                byte [] hashBytes = sha256.ComputeHash ( lastReceivedBinary );
                                string computedChecksum = BitConverter.ToString ( hashBytes ).Replace ( "-", "" ).ToLowerInvariant ();
                                await client.SendAsync ( computedChecksum == receivedChecksum.ToLowerInvariant () ? "Checksum VALID" : $"Checksum INVALID. Expected: {computedChecksum}, Got: {receivedChecksum}" );
                            }
                            lastReceivedBinary = null;
                        }
                    } while (msg != null);
                    return await client.CloseAsync ();
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/ws/headers", async ( HttpRequest request ) => {
                    HttpWebSocket client = await request.GetWebSocketAsync ();
                    // h.Value is string[] here when iterating request.Headers
                    var headersDict = request.Headers.ToDictionary ( h => h.Key, h => string.Join ( "; ", h.Value ) );
                    byte [] headersJsonBytes = System.Text.Json.JsonSerializer.SerializeToUtf8Bytes ( headersDict );
                    await client.SendAsync ( headersJsonBytes );
                    return await client.CloseAsync ();
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/ws/async-server", async ( HttpRequest request ) => {
                    HttpWebSocket client = await request.GetWebSocketAsync ();
                    await client.SendAsync ( "Connected to /tests/ws/async-server. Server will handle messages asynchronously." );
                    WebSocketMessage? msg;
                    do {
                        msg = await client.ReceiveMessageAsync ();
                        if (msg == null)
                            break;
                        var receivedMessage = msg.GetString ( request.RequestEncoding );
                        _ = Task.Run ( async () => {
                            await Task.Delay ( 500 );
                            if (!client.IsClosed)
                                await client.SendAsync ( $"Async response to: {receivedMessage}" );
                        } );
                    } while (msg != null);
                    return await client.CloseAsync ();
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/ws/disconnect", async ( HttpRequest request ) => {
                    HttpWebSocket? client = null;
                    string clientHash = "N/A";
                    try {
                        client = await request.GetWebSocketAsync ();
                        clientHash = client.GetHashCode ().ToString ();
                        Console.WriteLine ( $"[WebSocket /tests/ws/disconnect] Client {clientHash} connected. Waiting for disconnect." );
                        WebSocketMessage? msg;
                        do {
                            msg = await client.ReceiveMessageAsync ();
                            if (msg == null) { Console.WriteLine ( $"[WebSocket /tests/ws/disconnect] Client {clientHash} disconnected unexpectedly (ReceiveMessageAsync returned null)." ); break; }
                            if (!client.IsClosed)
                                await client.SendAsync ( "Still connected..." );
                        } while (msg != null);
                        return await client.CloseAsync ();
                    }
                    catch (WebSocketException wsex) {
                        Console.WriteLine ( $"[WebSocket /tests/ws/disconnect] Client {clientHash} disconnected with WebSocketException. Error: {wsex.Message}, WebSocketError: {wsex.WebSocketErrorCode}" );
                        if (client != null && !client.IsClosed)
                            return await client.CloseAsync ();
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, "WebSocket session ended due to a client error." );
                    }
                    catch (Exception ex) {
                        Console.WriteLine ( $"[WebSocket /tests/ws/disconnect] Error for client {clientHash}: {ex.Message}" );
                        if (client != null && !client.IsClosed)
                            return await client.CloseAsync ();
                        return new HttpResponse ( System.Net.HttpStatusCode.InternalServerError, "WebSocket session ended due to a server error." );
                    }
                    finally {
                        Console.WriteLine ( $"[WebSocket /tests/ws/disconnect] Client {clientHash} session ended." );
                        if (client != null && !client.IsClosed)
                            try { await client.CloseAsync (); }
                            catch { }
                    }
                } );

                router.SetRoute ( RouteMethod.Get, "/tests/ws/subprotocol", async ( HttpRequest request ) => {
                    string? selectedProtocol = null;
                    // The indexer on HttpHeaderCollection (derived from StringKeyStoreCollection) seems to resolve to `string?` not `StringValue`
                    // despite StringValueCollection having a 'new StringValue this[]'. The compiler error CS0030 suggests this.
                    string? clientProtocolsHeaderValue = request.Headers [ "Sec-WebSocket-Protocol" ];

                    if (!string.IsNullOrEmpty ( clientProtocolsHeaderValue )) {
                        var clientProtocols = clientProtocolsHeaderValue.Split ( ',' )
                            .Select ( p => p.Trim () )
                            .Where ( p => !string.IsNullOrEmpty ( p ) );

                        if (clientProtocols.Any ()) {
                            string [] supportedServerProtocols = { "custom.protocol" };
                            foreach (var pName in clientProtocols) {
                                if (supportedServerProtocols.Contains ( pName, StringComparer.OrdinalIgnoreCase )) {
                                    selectedProtocol = pName;
                                    break;
                                }
                            }
                        }
                    }
                    HttpWebSocket client = await request.GetWebSocketAsync ( subprotocol: selectedProtocol );
                    await client.SendAsync ( !string.IsNullOrEmpty ( selectedProtocol )
                        ? $"Sub-protocol '{selectedProtocol}' negotiated and selected."
                        : "No common sub-protocol negotiated, or client did not request one." );
                    WebSocketMessage? msg;
                    do {
                        msg = await client.ReceiveMessageAsync ();
                        if (msg == null)
                            break;
                        await client.SendAsync ( $"({selectedProtocol ?? "no-protocol"}): {msg.GetString ( request.RequestEncoding )}" );
                    } while (msg != null);
                    return await client.CloseAsync ();
                } );
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
public class NonSeekableStreamWrapper : System.IO.Stream {
    private readonly System.IO.Stream _innerStream;
    public NonSeekableStreamWrapper ( System.IO.Stream innerStream ) { _innerStream = innerStream; }
    public override bool CanRead => _innerStream.CanRead;
    public override bool CanSeek => false;
    public override bool CanWrite => _innerStream.CanWrite;
    public override long Length => _innerStream.CanSeek ? _innerStream.Length : throw new NotSupportedException ();
    public override long Position { get => _innerStream.CanSeek ? _innerStream.Position : throw new NotSupportedException (); set { if (!_innerStream.CanSeek) throw new NotSupportedException (); _innerStream.Position = value; } }
    public override void Flush () => _innerStream.Flush ();
    public override int Read ( byte [] buffer, int offset, int count ) => _innerStream.Read ( buffer, offset, count );
    public override long Seek ( long offset, System.IO.SeekOrigin origin ) => throw new NotSupportedException ();
    public override void SetLength ( long value ) => throw new NotSupportedException ();
    public override void Write ( byte [] buffer, int offset, int count ) => _innerStream.Write ( buffer, offset, count );
    protected override void Dispose ( bool disposing ) { if (disposing) { _innerStream.Dispose (); } base.Dispose ( disposing ); }
}

// Helper DTO for echoing multipart data
public class SimpleMultipartObjectInfo {
    public string? Name { get; set; }
    public string? Value { get; set; }
    public string? FileName { get; set; }
    public string? ContentType { get; set; }
    public long Length { get; set; }
    public string? ContentPreview { get; set; }
    public Dictionary<string, string?>? PartHeaders { get; set; }
}
