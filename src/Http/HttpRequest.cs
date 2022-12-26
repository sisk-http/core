using Sisk.Core.Entity;
using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Web;

namespace Sisk.Core.Http
{
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
        private HttpServerConfiguration contextServerConfiguration;
        private HttpListenerResponse listenerResponse;
        private HttpListenerRequest listenerRequest;
        private byte[]? contentBytes;
        internal bool isServingEventSourceEvents;
        private HttpRequestEventSource? activeEventSource;
        private bool isContentAvailable = false;
        private bool hasContents = false;

        internal HttpRequest(
            ref HttpListenerRequest listenerRequest,
            ref HttpListenerResponse listenerResponse,
            HttpServerConfiguration contextServerConfiguration)
        {
            this.contextServerConfiguration = contextServerConfiguration;
            this.listenerResponse = listenerResponse;
            this.listenerRequest = listenerRequest;
            this.RequestedAt = DateTime.Now;
            this.Query = listenerRequest.QueryString;
            this.Origin = new IPAddress(listenerRequest.LocalEndPoint.Address.GetAddressBytes());
            hasContents = listenerRequest.ContentLength64 > 0;
        }

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
        /// Gets a unique random ID for this request that is generated on server input.
        /// </summary>
        /// <definition>
        /// public Guid RequestId { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Guid RequestId { get; init; } = Guid.NewGuid();

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
        public bool HasContents { get => hasContents; }

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
            get => listenerRequest.Headers;
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
        /// Gets the HTTP request body as string.
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
            get => contentBytes ?? new byte[] { };
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
            get; private init;
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
        public string? GetHeader(string headerName)
        {
            foreach (string header in Headers.Keys)
            {
                if (string.Compare(header, headerName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Headers[header];
                }
            }
            return null;
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
        public string? GetQueryValue(string queryKeyName)
        {
            foreach (string q in Query.Keys)
            {
                if (string.Compare(q, queryKeyName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Query[q];
                }
            }
            return null;
        }

        /// <summary>
        /// Create an HTTP response with code 200 OK without any body.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public HttpResponse CreateHeadResponse()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse CreateHeadResponse()
        {
            HttpResponse res = new HttpResponse();
            res.Status = System.Net.HttpStatusCode.OK;
            return res;
        }

        /// <summary>
        /// Creates an HttpResponse object with given status code and body content.
        /// </summary>
        /// <param name="statusCode">The Http response status code.</param>
        /// <param name="content">The body content.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpResponse CreateResponse(HttpStatusCode statusCode, string? content)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse CreateResponse(HttpStatusCode statusCode, string? content)
        {
            HttpResponse res = new HttpResponse();
            res.Status = statusCode;
            if (content != null)
                res.Content = new StringContent(content, listenerRequest.ContentEncoding, "text/plain");
            return res;
        }

        /// <summary>
        /// Creates an HttpResponse object with given status code.
        /// </summary>
        /// <param name="statusCode">The Http response status code.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpResponse CreateResponse(HttpStatusCode statusCode)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse CreateResponse(HttpStatusCode statusCode)
        {
            HttpResponse res = new HttpResponse();
            res.Status = statusCode;
            return res;
        }

        /// <summary>
        /// Creates an HttpResponse object with status code 200 OK and given content.
        /// </summary>
        /// <param name="content">The string content.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpResponse CreateOkResponse(string? content)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse CreateOkResponse(string? content)
        {
            HttpResponse res = new HttpResponse();
            res.Status = System.Net.HttpStatusCode.OK;
            if (content != null)
                res.Content = new StringContent(content, listenerRequest.ContentEncoding, "text/plain");

            return res;
        }

        /// <summary>
        /// Creates an HTTP 301 response code for the given location.
        /// </summary>
        /// <param name="location">The header value for the new location.</param>
        /// <param name="permanent">Determines if the response is HTTP 301 or HTTP 302.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpResponse CreateRedirectResponse(string location, bool permanent)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse CreateRedirectResponse(string location, bool permanent)
        {
            HttpResponse res = new HttpResponse();
            res.Status = permanent ? System.Net.HttpStatusCode.MovedPermanently : System.Net.HttpStatusCode.Moved;
            res.Headers.Add("Location", location);

            return res;
        }

        /// <summary>
        /// Gets an Event Source interface for this request. Calling this method will put this <see cref="HttpRequest"/> instance in it's
        /// event source listening state.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public HttpRequestEventSource GetEventSource()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpRequestEventSource GetEventSource()
        {
            if (isServingEventSourceEvents)
            {
                throw new InvalidOperationException("This HTTP request is already listening to Event Sources.");
            }
            isServingEventSourceEvents = true;
            activeEventSource = new HttpRequestEventSource(listenerResponse, listenerRequest, contextServerConfiguration);
            return activeEventSource;
        }
    }
}
