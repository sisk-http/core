using Sisk.Core.Entity;
using Sisk.Core.Http.Streams;
using System.Collections.Specialized;
using System.Net;
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
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
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
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
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
        private bool isContentAvailable = false;
        private NameValueCollection headers = new NameValueCollection();

        internal HttpRequest(
            HttpListenerRequest listenerRequest,
            HttpListenerResponse listenerResponse,
            HttpServer server,
            ListeningHost host,
            HttpListenerContext context)
        {
            this.baseServer = server;
            this.contextServerConfiguration = baseServer.ServerConfiguration;
            this.listenerResponse = listenerResponse;
            this.listenerRequest = listenerRequest;
            this.hostContext = host;
            this.RequestedAt = DateTime.Now;
            this.Query = listenerRequest.QueryString;
            this.RequestId = Guid.NewGuid();

            IPAddress requestRealAddress = new IPAddress(listenerRequest.LocalEndPoint.Address.GetAddressBytes());
            this.Origin = requestRealAddress;

            if (contextServerConfiguration.ResolveForwardedOriginAddress)
            {
                string? forwardedIp = listenerRequest.Headers["X-Forwarded-For"];
                if (forwardedIp != null)
                {
                    /*
                     * the first entry from the header value is the real client ip.
                     * source: https://datatracker.ietf.org/doc/html/rfc2616#section-4.2
                     */
                    string forwardedIpLiteralStr = forwardedIp.Contains(',') ? forwardedIp.Substring(0, forwardedIp.IndexOf(',')) : forwardedIp;
                    bool ok = IPAddress.TryParse(forwardedIpLiteralStr, out IPAddress? forwardedAddress);
                    if (!ok || forwardedAddress == null)
                    {
                        throw new HttpRequestException("The forwarded IP address is invalid.");
                    }
                    else
                    {
                        this.Origin = forwardedAddress;
                    }
                }
            }

            string? cookieHeader = listenerRequest.Headers["cookie"];
            if (cookieHeader != null)
            {
                string[] cookieParts = cookieHeader.Split(';');
                foreach (string cookieExpression in cookieParts)
                {
                    int eqPos = cookieExpression.IndexOf("=");
                    if (eqPos < 0)
                    {
                        throw new HttpRequestException("The cookie header is invalid or is it has an malformed syntax.");
                    }
                    string key = cookieExpression.Substring(0, eqPos).Trim();
                    string value = cookieExpression.Substring(eqPos + 1).Trim();

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new HttpRequestException("The cookie header is invalid or is it has an malformed syntax.");
                    }

                    this.Cookies[key] = WebUtility.UrlDecode(value);
                }
            }

            // normalize headers encoding
            if (contextServerConfiguration.Flags.NormalizeHeadersEncodings)
            {
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

            this.context = context;
        }

        internal string mbConvertCodepage(string input, Encoding inEnc, Encoding outEnc)
        {
            byte[] tempBytes;
            tempBytes = inEnc.GetBytes(input);
            return outEnc.GetString(tempBytes);
        }

#pragma warning disable
        ~HttpRequest()
        {
            this.contentBytes = null;
            this.listenerRequest = null;
            this.listenerResponse = null;
            this.contextServerConfiguration = null;
        }
#pragma warning restore

        internal void ImportContents(Stream listenerRequest)
        {
            using (var memoryStream = new MemoryStream())
            {
                listenerRequest.CopyTo(memoryStream);
                this.contentBytes = memoryStream.ToArray();
                isContentAvailable = true;
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Guid RequestId { get; private set; }

        /// <summary>
        /// Gets a boolean indicating whether this request was made by an secure transport context (SSL/TLS) or not.
        /// </summary>
        /// <definition>
        /// public bool IsSecure { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IsContentAvailable { get => isContentAvailable; }

        /// <summary>
        /// Gets a boolean indicating whether this request has contents.
        /// </summary>
        /// <definition>
        /// public bool HasContents { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Headers
        {
            get => headers;
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Cookies { get; private set; } = new NameValueCollection();

        /// <summary>
        /// Get the requested host header (without port) from this HTTP request.
        /// </summary>
        /// <definition>
        /// public string Host { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string Host
        {
            get => listenerRequest.Url!.Host;
        }

        /// <summary>
        /// Get the requested host header with the port from this HTTP request.
        /// </summary>
        /// <definition>
        /// public string Authority { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public byte[] RawBody
        {
            get => contentBytes ?? Array.Empty<byte>();
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public long ContentLength
        {
            get => listenerRequest.ContentLength64;
        }

        /// <summary>
        /// Gets the HTTP request query extracted from the path string. This property also contains routing parameters.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Query { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Query { get; internal set; }

        /// <summary>
        /// Gets the HTTP request URL raw query string.
        /// </summary>
        /// <definition>
        /// public string? QueryString { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public IPAddress Origin
        {
            get; internal set;
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public DateTime RequestedAt { get; private init; }

        /// <summary>
        /// Gets the HttpContext for this request.
        /// </summary>
        /// <definition>
        /// public HttpContext? Context { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpContext? Context { get; internal set; }

        /// <summary>
        /// Gets the multipart form content for this request.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <definition>
        /// public MultipartObject[] GetMultipartFormContent()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public MultipartObject[] GetMultipartFormContent()
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection GetFormContent()
        {
            return HttpUtility.ParseQueryString(Body);
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string? GetQueryValue(string queryKeyName) => Query[queryKeyName];

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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string? GetHeader(string headerName) => Headers[headerName];

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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse Close()
        {
            return new HttpResponse(HttpResponse.HTTPRESPONSE_SERVER_CLOSE);
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
                throw new InvalidOperationException("This HTTP request is already in streaming mode.");
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
                throw new InvalidOperationException("This HTTP request is already in streaming mode.");
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
                throw new InvalidOperationException("This HTTP request is already in streaming mode.");
            }
            isStreaming = true;
            var accept = context.AcceptWebSocketAsync(subprotocol).Result;
            return new HttpWebSocket(accept, this, identifier);
        }

        internal long CalcRequestSize()
        {
            long l = 0;
            l += listenerRequest.ContentLength64;
            l += RequestEncoding.GetByteCount(GetRawHttpRequest(false));
            return l;
        }
    }
}
