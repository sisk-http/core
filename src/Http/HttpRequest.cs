// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequest.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Http.Streams;
using Sisk.Core.Internal;
using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;
using System.Web;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an exception that is thrown while a request is being interpreted by the HTTP server.
    /// </summary>
    public class HttpRequestException : Exception
    {
        internal HttpRequestException(string message) : base(message) { }
    }

    /// <summary>
    /// Represents an HTTP request received by a Sisk server.
    /// </summary>
    public sealed class HttpRequest
    {
        internal HttpServer baseServer;
        private readonly HttpServerConfiguration contextServerConfiguration;
        private readonly HttpListenerResponse listenerResponse;
        private readonly HttpListenerRequest listenerRequest;
        private readonly HttpListenerContext context;
        private byte[]? contentBytes;
        internal bool isStreaming;
        private HttpRequestEventSource? activeEventSource;
        private HttpHeaderCollection? headers = null;
        private NameValueCollection? cookies = null;
        private StringValueCollection? query = null;
        private StringValueCollection? form = null;

        private IPAddress? remoteAddr;

        private int currentFrame = 0;

        internal HttpRequest(
            HttpServer server,
            HttpListenerContext context)
        {
            this.context = context;
            baseServer = server;
            contextServerConfiguration = baseServer.ServerConfiguration;
            listenerResponse = context.Response;
            listenerRequest = context.Request;
            RequestedAt = DateTime.Now;
        }

        internal string mbConvertCodepage(string input, Encoding inEnc, Encoding outEnc)
        {
            byte[] tempBytes;
            tempBytes = inEnc.GetBytes(input);
            return outEnc.GetString(tempBytes);
        }

        void ReadRequestStreamContents()
        {
            if (contentBytes is null)
            {
                if (ContentLength > 0)
                {
                    using (var memoryStream = new MemoryStream(ContentLength))
                    {
                        listenerRequest.InputStream.CopyTo(memoryStream);
                        contentBytes = memoryStream.ToArray();
                    }
                }
                else if (ContentLength < 0)
                {
                    contentBytes = Array.Empty<byte>();
                    throw new HttpRequestException(SR.HttpRequest_NoContentLength);
                }
                else
                {
                    contentBytes = Array.Empty<byte>();
                }
            }
        }

        /// <summary>
        /// Gets a unique random ID for this request.
        /// </summary>
        public Guid RequestId { get => listenerRequest.RequestTraceIdentifier; }

        /// <summary>
        /// Gets a boolean indicating whether this request was made by an secure
        /// transport context (SSL/TLS) or not.
        /// </summary>
        public bool IsSecure
        {
            get
            {
                if (contextServerConfiguration.ForwardingResolver is { } fr)
                {
                    return fr.OnResolveSecureConnection(this, listenerRequest.IsSecureConnection);
                }
                else
                {
                    return listenerRequest.IsSecureConnection;
                }
            }
        }

        /// <summary>
        /// Gets a boolean indicating whether the body content of this request has been processed by the server.
        /// </summary>
        public bool IsContentAvailable { get => contentBytes is not null; }

        /// <summary>
        /// Gets a boolean indicating whether this request has body contents.
        /// </summary>
        public bool HasContents { get => ContentLength > 0; }

        /// <summary>
        /// Gets the HTTP request headers.
        /// </summary>
        public HttpHeaderCollection Headers
        {
            get
            {
                if (headers is null)
                {
                    if (contextServerConfiguration.Flags.NormalizeHeadersEncodings)
                    {
                        headers = new HttpHeaderCollection();
                        Encoding entryCodepage = Encoding.GetEncoding("ISO-8859-1");
                        foreach (string headerName in listenerRequest.Headers)
                        {
                            string headerValue = listenerRequest.Headers[headerName]!;
                            headers.Add(
                                headerName,
                                mbConvertCodepage(headerValue, entryCodepage, listenerRequest.ContentEncoding)
                            );
                        }
                    }
                    else
                    {
                        headers = new HttpHeaderCollection(listenerRequest.Headers);
                    }
                }

                return headers;
            }
        }

        /// <summary>
        /// Gets an <see cref="NameValueCollection"/> object with all cookies set in this request.
        /// </summary>
        public NameValueCollection Cookies
        {
            get
            {
                if (cookies is null)
                {
                    cookies = new NameValueCollection();
                    string? cookieHeader = listenerRequest.Headers[HttpKnownHeaderNames.Cookie];

                    if (!string.IsNullOrWhiteSpace(cookieHeader))
                    {
                        string[] cookiePairs = cookieHeader.Split(';');

                        for (int i = 0; i < cookiePairs.Length; i++)
                        {
                            string cookieExpression = cookiePairs[i];

                            int eqPos = cookieExpression.IndexOf('=');
                            if (eqPos < 0)
                            {
                                throw new HttpRequestException(SR.HttpRequest_InvalidCookieSyntax);
                            }

                            string cookieName = cookieExpression.Substring(0, eqPos).Trim();
                            string cookieValue = cookieExpression.Substring(eqPos + 1).Trim();

                            if (string.IsNullOrWhiteSpace(cookieName))
                            {
                                throw new HttpRequestException(SR.HttpRequest_InvalidCookieSyntax);
                            }

                            cookies[cookieName] = WebUtility.UrlDecode(cookieValue);
                        }
                    }
                }

                return cookies;
            }
        }

        /// <summary>
        /// Get the requested host header (without port) from this HTTP request.
        /// </summary>
        public string? Host { get; internal set; }

        /// <summary>
        /// Gets the managed object which holds data for an entire HTTP session.
        /// </summary>
        /// <remarks>
        /// This property is an shortcut for <see cref="HttpContext.RequestBag"/> property.
        /// </remarks>
        public HttpContextBagRepository Bag => Context.RequestBag;

        /// <summary>
        /// Get the requested host header with the port from this HTTP request.
        /// </summary>
        public string Authority
        {
            get => listenerRequest.Url!.Authority;
        }

        /// <summary>
        /// Gets the HTTP request path without the query string.
        /// </summary>
        public string Path
        {
            get => listenerRequest.Url?.AbsolutePath ?? "/";
        }

        /// <summary>
        /// Gets the full HTTP request path with the query string.
        /// </summary>
        public string FullPath
        {
            get => listenerRequest.RawUrl ?? "/";
        }

        /// <summary>
        /// Gets the full URL for this request, with scheme, host, port, path and query.
        /// </summary>
        public string FullUrl
        {
            get => listenerRequest.Url!.ToString();
        }

        /// <summary>
        /// Gets an string <see cref="Encoding"/> that can be used to decode text in this HTTP request.
        /// </summary>
        public Encoding RequestEncoding
        {
            get => listenerRequest.ContentEncoding;
        }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        public HttpMethod Method
        {
            get => new HttpMethod(listenerRequest.HttpMethod);
        }

        /// <summary>
        /// Gets the HTTP request body as string, decoded by the request content encoding.
        /// </summary>
        public string Body
        {
            get => listenerRequest.ContentEncoding.GetString(RawBody);
        }

        /// <summary>
        /// Gets the HTTP request body as a byte array.
        /// </summary>
        public byte[] RawBody
        {
            get
            {
                ReadRequestStreamContents();
                return contentBytes!;
            }
        }

        /// <summary>
        /// Gets the content length in bytes count.
        /// </summary>
        /// <remarks>
        /// The value can be negative if the content length is unknown.
        /// </remarks>
        public int ContentLength
        {
            get => (int)listenerRequest.ContentLength64;
        }

        /// <summary>
        /// Gets the HTTP request query extracted from the path string. This property also contains routing parameters.
        /// </summary>
        public StringValueCollection Query
        {
            get
            {
                if (query is null)
                {
                    query = StringValueCollection.FromNameValueCollection("query parameter", listenerRequest.QueryString);
                }
                return query;
            }
        }

        /// <summary>
        /// Gets the HTTP request URL raw query string.
        /// </summary>
        public string? QueryString { get => listenerRequest.Url?.Query; }

        /// <summary>
        /// Gets the incoming IP address from the request.
        /// </summary>
        public IPAddress RemoteAddress
        {
            get
            {
                if (contextServerConfiguration.ForwardingResolver is { } fr)
                {
                    remoteAddr = fr.OnResolveClientAddress(this, listenerRequest.RemoteEndPoint);
                }
                else
                {
                    remoteAddr = new IPAddress(listenerRequest.RemoteEndPoint.Address.GetAddressBytes());
                }

                return remoteAddr;
            }
        }

        /// <summary>
        /// Gets the moment which the request was received by the server.
        /// </summary>
        public DateTime RequestedAt { get; private init; }

        /// <summary>
        /// Gets the HttpContext for this request.
        /// </summary>
        public HttpContext Context { get; internal set; } = null!;

        /// <summary>
        /// Gets the multipart form content for this request.
        /// </summary>
        public MultipartFormCollection GetMultipartFormContent()
        {
            return MultipartObject.ParseMultipartObjects(this);
        }

        /// <summary>
        /// Gets the values sent by a form in this request.
        /// </summary>
        public StringValueCollection GetFormContent()
        {
            if (form is null)
            {
                form = StringValueCollection.FromNameValueCollection("form", HttpUtility.ParseQueryString(Body));
            }
            return form;
        }

        /// <summary>
        /// Gets the raw HTTP request message from the socket.
        /// </summary>
        public string GetRawHttpRequest(bool includeBody = true, bool appendExtraInfo = false)
        {
            StringBuilder sb = new StringBuilder();
            // Method and path
            sb.Append(Method.ToString().ToUpper() + " ");
            sb.Append(Path + " ");
            sb.Append("HTTP/");
            sb.Append(listenerRequest.ProtocolVersion.Major + ".");
            sb.Append(listenerRequest.ProtocolVersion.Minor + "\n");

            // Headers
            if (appendExtraInfo)
            {
                sb.AppendLine($":remote-ip: {RemoteAddress} (was {listenerRequest.RemoteEndPoint})");
                sb.AppendLine($":host: {Host} (was {listenerRequest.UserHostName})");
                sb.AppendLine($":date: {RequestedAt:s}");
                sb.AppendLine($":request-id: {RequestId}");
                sb.AppendLine($":request-proto: {(IsSecure ? "https" : "http")}");
            }
            foreach (string hName in Headers)
            {
                string hValue = Headers[hName]!;
                sb.AppendLine($"{hName}: {hValue}");
            }
            sb.AppendLine();

            // Content
            if (includeBody)
            {
                sb.Append(Body);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates and stores a managed object in HTTP context bag through it's type.
        /// </summary>
        /// <typeparam name="T">The type of object that will be stored in the HTTP context bag.</typeparam>
        /// <returns>Returns the stored object.</returns>
        [Obsolete("This method is deprecated and should be removed in future Sisk versions. Please, use Bag property instead.")]
        public T SetContextBag<T>() where T : notnull, new()
        {
            return Context.RequestBag.Set<T>();
        }

        /// <summary>
        /// Stores a managed object in HTTP context bag through it's type.
        /// </summary>
        /// <typeparam name="T">The type of object that will be stored in the HTTP context bag.</typeparam>
        /// <param name="contextObject">The object which will be stored.</param>
        /// <returns>Returns the stored object.</returns>
        [Obsolete("This method is deprecated and should be removed in future Sisk versions. Please, use Bag property instead.")]
        public T SetContextBag<T>(T contextObject) where T : notnull
        {
            return Context.RequestBag.Set<T>(contextObject);
        }

        /// <summary>
        /// Gets an managed object from the HTTP context bag through it's type.
        /// </summary>
        /// <typeparam name="T">The type of object which is stored in the HTTP context bag.</typeparam>
        [Obsolete("This method is deprecated and should be removed in future Sisk versions. Please, use Bag property instead.")]
        public T GetContextBag<T>() where T : notnull
        {
            return Context.RequestBag.Get<T>();
        }

        /// <summary>
        /// Gets a query value using an case-insensitive search.
        /// </summary>
        /// <param name="queryKeyName">The query value name.</param>
        public string? GetQueryValue(string queryKeyName) => Query[queryKeyName].Value;

        /// <summary>
        /// Gets the value stored from the <see cref="Query"/> and converts it to the given type.
        /// </summary>
        /// <typeparam name="T">The parseable type which will be converted to.</typeparam>
        /// <param name="queryKeyName">The name of the URL parameter. The search is ignore-case.</param>
        /// <param name="defaultValue">The default value that will be returned if the item is not found in the query.</param>
        public T GetQueryValue<T>(string queryKeyName, T defaultValue = default) where T : struct
        {
            StringValue value = Query[queryKeyName];
            if (value.IsNull) return defaultValue;

            try
            {
                return value.Get<T>();
            }
            catch (Exception)
            {
                throw new InvalidCastException(string.Format(SR.HttpRequest_GetQueryValue_CastException, queryKeyName, typeof(T).FullName));
            }
        }

        /// <summary>
        /// Calls another handler for this request, preserving the current call-stack frame, and then returns the response from
        /// it. This method manages to prevent possible stack overflows.
        /// </summary>
        /// <param name="otherCallback">Defines the <see cref="RouteAction"/> method which will handle this request.</param>
        public object SendTo(RouteAction otherCallback)
        {
            Interlocked.Increment(ref currentFrame);
            if (currentFrame > 64)
            {
                throw new OverflowException(SR.HttpRequest_SendTo_MaxRedirects);
            }
            return otherCallback(this);
        }

        /// <summary>
        /// Closes this HTTP request and their connection with the remote client without sending any response.
        /// </summary>
        public HttpResponse Close()
        {
            return new HttpResponse(HttpResponse.HTTPRESPONSE_SERVER_REFUSE);
        }

        /// <summary>
        /// Gets the HTTP request content stream. This property is only available while the
        /// content has not been imported by the HTTP server and will invalidate the body content 
        /// cached in this object.
        /// </summary>
        public Stream GetRequestStream()
        {
            if (contentBytes is not null)
            {
                throw new InvalidOperationException(SR.HttpRequest_InputStreamAlreadyLoaded);
            }
            return listenerRequest.InputStream;
        }

        /// <summary>
        /// Gets an HTTP response stream for this HTTP request.
        /// </summary>
        public HttpResponseStream GetResponseStream()
        {
            if (isStreaming)
            {
                throw new InvalidOperationException(SR.HttpRequest_AlreadyInStreamingState);
            }
            isStreaming = true;
            return new HttpResponseStream(listenerResponse, listenerRequest, this);
        }

        /// <summary>
        /// Gets an Event Source interface for this request. Calling this method will put this <see cref="HttpRequest"/> instance in it's
        /// event source listening state.
        /// </summary>
        /// <param name="identifier">Optional. Defines an label to the EventStream connection, useful for finding this connection's reference later.</param>
        public HttpRequestEventSource GetEventSource(string? identifier = null)
        {
            if (isStreaming)
            {
                throw new InvalidOperationException(SR.HttpRequest_AlreadyInStreamingState);
            }
            isStreaming = true;
            activeEventSource = new HttpRequestEventSource(identifier, listenerResponse, listenerRequest, this);
            return activeEventSource;
        }

        /// <summary>
        /// Accepts and acquires a websocket for this request. Calling this method will put this <see cref="HttpRequest"/> instance in
        /// streaming state.
        /// </summary>
        /// <param name="subprotocol">Optional. Determines the sub-protocol to plug the websocket in.</param>
        /// <param name="identifier">Optional. Defines an label to the Web Socket connection, useful for finding this connection's reference later.</param>
        public HttpWebSocket GetWebSocket(string? subprotocol = null, string? identifier = null)
        {
            if (isStreaming)
            {
                throw new InvalidOperationException(SR.HttpRequest_AlreadyInStreamingState);
            }
            isStreaming = true;
            var accept = context.AcceptWebSocketAsync(subprotocol).Result;
            return new HttpWebSocket(accept, this, identifier);
        }

        /// <summary>
        /// Gets an string representation of this <see cref="HttpRequest"/> object.
        /// </summary>
        public override string ToString()
        {
            return $"{Method} {FullPath}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal long CalcRequestSize() => listenerRequest.ContentLength64;
    }
}
