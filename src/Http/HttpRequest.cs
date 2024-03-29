﻿// The Sisk Framework source code
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
    /// <definition>
    /// public class HttpRequestException : Exception
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class HttpRequestException : Exception
    {
        internal HttpRequestException(string message) : base(message) { }
    }

    /// <summary>
    /// Represents an HTTP request received by a Sisk server.
    /// </summary>
    /// <definition>
    /// public sealed class HttpRequest
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class HttpRequest
    {
        internal HttpServer baseServer;
        internal ListeningHost hostContext;
        private HttpServerConfiguration contextServerConfiguration;
        private HttpListenerResponse listenerResponse;
        private HttpListenerRequest listenerRequest;
        private HttpListenerContext context;
        private byte[]? contentBytes;
        internal bool isStreaming;
        private HttpRequestEventSource? activeEventSource;
        private NameValueCollection? headers = null;
        private NameValueCollection? cookies = null;
        private StringValueCollection? query = null;
        private StringValueCollection? form = null;
        private int currentFrame = 0;

        internal HttpRequest(
            HttpServer server,
            ListeningHost host,
            HttpListenerContext context)
        {
            this.context = context;
            this.baseServer = server;
            this.contextServerConfiguration = baseServer.ServerConfiguration;
            this.listenerResponse = context.Response;
            this.listenerRequest = context.Request;
            this.RequestedAt = DateTime.Now;
            this.hostContext = host;
        }

        internal string mbConvertCodepage(string input, Encoding inEnc, Encoding outEnc)
        {
            byte[] tempBytes;
            tempBytes = inEnc.GetBytes(input);
            return outEnc.GetString(tempBytes);
        }

        void ReadRequestStreamContents()
        {
            if (this.contentBytes == null)
            {
                using (var memoryStream = new MemoryStream())
                {
                    listenerRequest.InputStream.CopyTo(memoryStream);
                    this.contentBytes = memoryStream.ToArray();
                }
            }
        }

        /// <summary>
        /// Gets a unique random ID for this request.
        /// </summary>
        /// <definition>
        /// public string RequestId { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Guid RequestId { get => listenerRequest.RequestTraceIdentifier; }

        /// <summary>
        /// Gets a boolean indicating whether this request was made by an secure transport context (SSL/TLS) or not.
        /// </summary>
        /// <definition>
        /// public bool IsSecure { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsSecure { get => listenerRequest.IsSecureConnection; }

        /// <summary>
        /// Gets a boolean indicating whether the content of this request has been processed by the server.
        /// </summary>
        /// <definition>
        /// public bool IsContentAvailable { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsContentAvailable { get => contentBytes != null; }

        /// <summary>
        /// Gets a boolean indicating whether this request has contents.
        /// </summary>
        /// <definition>
        /// public bool HasContents { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool HasContents { get => this.ContentLength > 0; }

        /// <summary>
        /// Gets the HTTP request headers.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Headers { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public NameValueCollection Headers
        {
            get
            {
                if (headers == null)
                {
                    if (contextServerConfiguration.Flags.NormalizeHeadersEncodings)
                    {
                        headers = new NameValueCollection();
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
                        headers = listenerRequest.Headers;
                    }
                }

                return headers;
            }
        }

        /// <summary>
        /// Gets an <see cref="NameValueCollection"/> object with all cookies set in this request.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Cookies { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public NameValueCollection Cookies
        {
            get
            {
                if (cookies == null)
                {
                    cookies = new NameValueCollection();
                    string? cookieHeader = listenerRequest.Headers[HttpKnownHeaderNames.Cookie];
                    if (cookieHeader != null)
                    {
                        string[] cookieParts = cookieHeader.Split(';');
                        foreach (string cookieExpression in cookieParts)
                        {
                            int eqPos = cookieExpression.IndexOf("=");
                            if (eqPos < 0)
                            {
                                throw new HttpRequestException(SR.HttpRequest_InvalidCookieSyntax);
                            }
                            string key = cookieExpression.Substring(0, eqPos).Trim();
                            string value = cookieExpression.Substring(eqPos + 1).Trim();

                            if (string.IsNullOrEmpty(key))
                            {
                                throw new HttpRequestException(SR.HttpRequest_InvalidCookieSyntax);
                            }

                            cookies[key] = WebUtility.UrlDecode(value);
                        }
                    }
                }

                return cookies;
            }
        }

        /// <summary>
        /// Get the requested host header (without port) from this HTTP request.
        /// </summary>
        /// <definition>
        /// public string Host { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string Host
        {
            get => listenerRequest.Url!.Host;
        }

        /// <summary>
        /// Gets the managed object which holds data for an entire HTTP session.
        /// </summary>
        /// <remarks>
        /// This property is an shortcut for <see cref="HttpContext.RequestBag"/> property.
        /// </remarks>
        /// <definition>
        /// public HttpContextBagRepository Bag { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpContextBagRepository Bag { get => Context.RequestBag; }

        /// <summary>
        /// Get the requested host header with the port from this HTTP request.
        /// </summary>
        /// <definition>
        /// public string Authority { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string Authority
        {
            get => listenerRequest.Url!.Authority;
        }

        /// <summary>
        /// Gets the HTTP request path without the query string.
        /// </summary>
        /// <definition>
        /// public string Path { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string Path
        {
            get => listenerRequest.Url?.AbsolutePath ?? "/";
        }

        /// <summary>
        /// Gets the full HTTP request path with the query string.
        /// </summary>
        /// <definition>
        /// public string FullPath { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string FullPath
        {
            get => listenerRequest.RawUrl ?? "/";
        }

        /// <summary>
        /// Gets the full URL for this request, with scheme, host, port (if any), path and query.
        /// </summary>
        /// <definition>
        /// public string FullUrl { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string FullUrl
        {
            get => listenerRequest.Url!.ToString();
        }

        /// <summary>
        /// Gets the Encoding used in the request.
        /// </summary>
        /// <definition>
        /// public Encoding RequestEncoding { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Encoding RequestEncoding
        {
            get => listenerRequest.ContentEncoding;
        }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        /// <definition>
        /// public HttpMethod Method { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpMethod Method
        {
            get => new HttpMethod(listenerRequest.HttpMethod);
        }


        /// <summary>
        /// Gets the HTTP request body as string, decoded by the request content encoding.
        /// </summary>
        /// <definition>
        /// public string Body { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string Body
        {
            get => listenerRequest.ContentEncoding.GetString(RawBody);
        }

        /// <summary>
        /// Gets the HTTP request body as a byte array.
        /// </summary>
        /// <definition>
        /// public byte[] RawBody { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public byte[] RawBody
        {
            get
            {
                ReadRequestStreamContents();
                return contentBytes!;
            }
        }

        /// <summary>
        /// Gets the content length in bytes.
        /// </summary>
        /// <definition>
        /// public long ContentLength { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public long ContentLength
        {
            get => listenerRequest.ContentLength64;
        }

        /// <summary>
        /// Gets the HTTP request query extracted from the path string. This property also contains routing parameters.
        /// </summary>
        /// <definition>
        /// public StringValueCollection Query { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public StringValueCollection Query
        {
            get
            {
                if (query == null)
                {
                    query = StringValueCollection.FromNameValueCollection("query parameter", listenerRequest.QueryString);
                }
                return query;
            }
        }

        /// <summary>
        /// Gets the HTTP request URL raw query string.
        /// </summary>
        /// <definition>
        /// public string? QueryString { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? QueryString { get => listenerRequest.Url?.Query; }


        /// <summary>
        /// Gets the incoming IP address from the request.
        /// </summary>
        /// <definition>
        /// public IPAddress Origin { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        [Obsolete("This property is deprecated. Use HttpRequest.RemoteAddress instead.")]
        public IPAddress Origin { get => RemoteAddress; }

        /// <summary>
        /// Gets the incoming IP address from the request.
        /// </summary>
        /// <definition>
        /// public IPAddress Origin { get; }
        /// </definition>
        /// <type>
        /// Property 
        /// </type>
        public IPAddress RemoteAddress
        {
            get
            {
                if (contextServerConfiguration.ResolveForwardedOriginAddress)
                {
                    string? forwardedIp = listenerRequest.Headers[HttpKnownHeaderNames.XForwardedFor];
                    if (forwardedIp != null)
                    {
                        string forwardedIpLiteralStr = forwardedIp.Contains(',') ? forwardedIp.Substring(forwardedIp.IndexOf(',') + 1) : forwardedIp;
                        bool ok = IPAddress.TryParse(forwardedIpLiteralStr, out IPAddress? forwardedAddress);
                        if (!ok || forwardedAddress == null)
                        {
                            throw new HttpRequestException(SR.HttpRequest_InvalidForwardedIpAddress);
                        }
                        else
                        {
                            return forwardedAddress;
                        }
                    }
                }

                return listenerRequest.RemoteEndPoint.Address;
            }
        }

        /// <summary>
        /// Gets the moment which the request was received by the server.
        /// </summary>
        /// <definition>
        /// public DateTime RequestedAt { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public DateTime RequestedAt { get; private init; }

        /// <summary>
        /// Gets the HttpContext for this request.
        /// </summary>
        /// <definition>
        /// public HttpContext Context { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpContext Context { get; internal set; } = null!;

        /// <summary>
        /// Gets the multipart form content for this request.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public MultipartFormCollection GetMultipartFormContent()
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public MultipartFormCollection GetMultipartFormContent()
        {
            return MultipartObject.ParseMultipartObjects(this);
        }

        /// <summary>
        /// Gets the values sent by a form in this request.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public NameValueCollection GetFormContent()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public StringValueCollection GetFormContent()
        {
            if (form == null)
            {
                form = StringValueCollection.FromNameValueCollection("form", HttpUtility.ParseQueryString(Body));
            }
            return form;
        }

        /// <summary>
        /// Gets the raw HTTP request message from the socket.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string GetRawHttpRequest(bool includeBody = true)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public string GetRawHttpRequest(bool includeBody = true)
        {
            StringBuilder sb = new StringBuilder();
            // Method and path
            sb.Append(Method.ToString().ToUpper() + " ");
            sb.Append(Path + " ");
            sb.Append("HTTP/");
            sb.Append(listenerRequest.ProtocolVersion.Major + ".");
            sb.Append(listenerRequest.ProtocolVersion.Minor + "\n");

            // Headers
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
        /// <definition>
        /// public T SetContextBag{{T}}() where T : notnull, new()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public T SetContextBag<T>() where T : notnull, new()
        {
            return this.Context.RequestBag.Set<T>();
        }

        /// <summary>
        /// Stores a managed object in HTTP context bag through it's type.
        /// </summary>
        /// <typeparam name="T">The type of object that will be stored in the HTTP context bag.</typeparam>
        /// <param name="contextObject">The object which will be stored.</param>
        /// <returns>Returns the stored object.</returns>
        /// <definition>
        /// public T SetContextBag{{T}}(T contextObject) where T : notnull
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public T SetContextBag<T>(T contextObject) where T : notnull
        {
            return this.Context.RequestBag.Set<T>(contextObject);
        }

        /// <summary>
        /// Gets an managed object from the HTTP context bag through it's type.
        /// </summary>
        /// <typeparam name="T">The type of object which is stored in the HTTP context bag.</typeparam>
        /// <definition>
        /// public T GetContextBag{{T}}(T contextObject) where T : notnull
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public T GetContextBag<T>() where T : notnull
        {
            return this.Context.RequestBag.Get<T>();
        }

        /// <summary>
        /// Gets a query value using an case-insensitive search.
        /// </summary>
        /// <param name="queryKeyName">The query value name.</param>
        /// <returns></returns>
        /// <definition>
        /// public string? GetQueryValue(string queryKeyName)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        [Obsolete("This method is deprecated and will be removed in later versions. Use Query[queryKeyName] instead.")]
        public string? GetQueryValue(string queryKeyName) => Query[queryKeyName];

        /// <summary>
        /// Gets the value stored from the <see cref="Query"/> and converts it to the given type.
        /// </summary>
        /// <typeparam name="T">The parseable type which will be converted to.</typeparam>
        /// <param name="queryKeyName">The name of the URL parameter. The search is ignore-case.</param>
        /// <param name="defaultValue">The default value that will be returned if the item is not found in the query.</param>
        /// <definition>
        /// // in .NET 6
        /// public T GetQueryValue{{T}}(string queryKeyName, T defaultValue = default) where T : struct
        /// 
        /// // in .NET 7+
        /// public T GetQueryValue{{T}}(string queryKeyName, T defaultValue = default) where T : struct, IParsable{{T}}
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        [Obsolete("This method is deprecated and will be removed in later versions. Use Query[queryKeyName] instead.")]
#if NET6_0
        public T GetQueryValue<T>(string queryKeyName, T defaultValue = default) where T : struct
#elif NET7_0_OR_GREATER
        public T GetQueryValue<T>(string queryKeyName, T defaultValue = default) where T : struct, IParsable<T>
#endif
        {
            string? value = Query[queryKeyName].MaybeNull()?.GetString();
            if (value == null) return defaultValue;

            try
            {
#if NET6_0
                return (T)Parseable.ParseInternal<T>(value);
#elif NET7_0_OR_GREATER
                return T.Parse(value, null);
#endif
            }
            catch (Exception)
            {
                throw new InvalidCastException(string.Format(SR.HttpRequest_GetQueryValue_CastException, queryKeyName, typeof(T).FullName));
            }
        }

        /// <summary>
        /// Gets a header value using a case-insensitive search.
        /// </summary>
        /// <param name="headerName">The header name.</param>
        /// <returns></returns>
        /// <definition>
        /// public string? GetHeader(string headerName)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        [Obsolete("This method is deprecated and will be removed in later versions. Use Headers[headerName] instead.")]
        public string? GetHeader(string headerName) => Headers[headerName];

        /// <summary>
        /// Calls another handler for this request, preserving the current call-stack frame, and then returns the response from
        /// it. This method manages to prevent possible stack overflows.
        /// </summary>
        /// <param name="otherCallback">Defines the <see cref="RouteAction"/> method which will handle this request.</param>
        /// <definition>
        /// public object SendTo(RouterCallback otherCallback)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
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
        /// <returns></returns>
        /// <definition>
        /// public HttpResponse Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpResponse Close()
        {
            return new HttpResponse(HttpResponse.HTTPRESPONSE_SERVER_REFUSE);
        }

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
        public Stream GetRequestStream()
        {
            if (contentBytes != null)
            {
                throw new InvalidOperationException(SR.HttpRequest_InputStreamAlreadyLoaded);
            }
            return listenerRequest.InputStream;
        }

        /// <summary>
        /// Gets an HTTP response stream for this HTTP request.
        /// </summary>
        /// <definition>
        /// public HttpResponseStream GetResponseStream()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
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
        /// <definition>
        /// public HttpRequestEventSource GetEventSource(string? identifier = null)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
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
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket GetWebSocket(string? subprotocol = null)
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        /// <exception cref="InvalidOperationException"></exception>
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
        /// <definition>
        /// public override String ToString()
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public override String ToString()
        {
            return $"{Method} {FullPath}";
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal long CalcRequestSize() => listenerRequest.ContentLength64;
    }
}
