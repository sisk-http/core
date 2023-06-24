using System.Collections.Specialized;
using System.Net;
using System.Text;
using System.Text.RegularExpressions;
using System.Web;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an HTTP Response.
    /// </summary>
    /// <definition>
    /// public sealed class HttpResponse : CookieHelper
    /// </definition> 
    /// <type>
    /// Class
    /// </type>
    public sealed class HttpResponse : CookieHelper
    {
        internal const byte HTTPRESPONSE_EMPTY = 2;
        internal const byte HTTPRESPONSE_SERVER_CLOSE = 4;
        internal const byte HTTPRESPONSE_CLIENT_CLOSE = 32;
        internal const byte HTTPRESPONSE_ERROR = 8;
        internal int CalculedLength = 0;

        /// <summary>
        /// Creates an new empty <see cref="HttpResponse"/> with no status code or contents. This will cause to the HTTP server to close the
        /// connection between the server and the client and don't deliver any response.
        /// </summary>
        /// <definition>
        /// public static HttpResponse CreateEmptyResponse()
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static HttpResponse CreateEmptyResponse()
        {
            return new HttpResponse(HTTPRESPONSE_EMPTY);
        }

        /// <summary>
        /// Creates an new redirect <see cref="HttpResponse"/> with given location header.
        /// </summary>
        /// <param name="location">The absolute or relative URL path which the client must be redirected to.</param>
        /// <definition>
        /// public static HttpResponse CreateRedirectResponse(string location)
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static HttpResponse CreateRedirectResponse(string location)
        {
            HttpResponse res = new HttpResponse();
            res.Status = System.Net.HttpStatusCode.MovedPermanently;
            res.Headers.Add("Location", location);

            return res;
        }

        /// <summary>
        /// Gets or sets an custom HTTP status code and description for this HTTP response. If this property ins't null, it will overwrite
        /// the <see cref="Status"/> property in this class.
        /// </summary>
        /// <definition>
        /// public HttpStatusInformation? CustomStatus { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpStatusInformation? CustomStatus { get; set; } = null;

        /// <summary>
        /// Gets or sets the HTTP response status code.
        /// </summary>
        /// <definition>
        /// public HttpStatusCode Status { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets a <see cref="NameValueCollection"/> instance of the HTTP response headers.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Headers { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public NameValueCollection Headers { get; private set; } = new NameValueCollection();

        /// <summary>
        /// Gets or sets the HTTP response body contents.
        /// </summary>
        /// <definition>
        /// public HttpContent? Content { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpContent? Content { get; set; }

        /// <summary>
        /// Gets or sets whether the HTTP response can be sent chunked.
        /// </summary>
        /// <definition>
        /// public bool SendChunked { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool SendChunked { get; set; } = false;

        internal byte internalStatus = 0;

        internal HttpResponse(byte internalStatus)
        {
            this.internalStatus = internalStatus;
        }

        internal HttpResponse(HttpListenerResponse res)
        {
            this.Status = (HttpStatusCode)res.StatusCode;
            this.Headers.Add(res.Headers);
        }

        /// <summary>
        /// Gets the raw HTTP response message.
        /// </summary>
        /// <param name="includeBody">Determines whether the message content will also be included in the return from this function.</param>
        /// <returns></returns>
        /// <definition>
        /// public string GetRawHttpResponse(bool includeBody = true)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public string GetRawHttpResponse(bool includeBody = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"HTTP/1.1 {(int)Status}");
            foreach (string header in this.Headers)
            {
                sb.Append(header + ": ");
                sb.Append(this.Headers[header]);
                sb.Append('\n');
            }
            sb.Append('\n');

            if (includeBody)
            {
                sb.Append(Content?.ReadAsStringAsync().Result);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with HTTP OK status code and no content.
        /// </summary>
        /// <definition>
        /// public HttpResponse()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse()
        {
            this.Status = HttpStatusCode.OK;
            this.Content = null;
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <definition>
        /// public HttpResponse(HttpStatusCode status)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(HttpStatusCode status) : this(status, null) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <definition>
        /// public HttpResponse(int status) 
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(int status) : this((HttpStatusCode)status, null) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP content.
        /// </summary>
        /// <definition>
        /// public HttpResponse(int status, HttpContent? content)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(int status, HttpContent? content) : this((HttpStatusCode)status, content) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given HTTP content, with default status code as 200 OK.
        /// </summary>
        /// <definition>
        /// public HttpResponse(HttpContent? content)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(HttpContent? content) : this(HttpStatusCode.OK, content) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP contents.
        /// </summary>
        /// <definition>
        /// public HttpResponse(HttpStatusCode status, HttpContent content)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(HttpStatusCode status, HttpContent? content)
        {
            this.Status = status;
            this.Content = content;
        }

        internal string? GetHeader(string headerName)
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

        internal override void SetCookieHeader(String name, String value)
        {
            this.Headers.Set(name, value);
        }
    }
}
