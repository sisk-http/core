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
    /// public class HttpResponse
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpResponse
    {
        internal const byte HTTPRESPONSE_EMPTY = 2;
        internal const byte HTTPRESPONSE_EVENTSOURCE_CLOSE = 4;
        internal const byte HTTPRESPONSE_ERROR = 8;
        internal const byte HTTPRESPONSE_CLOSE = 16;
        internal int CalculedLength = 0;

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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Headers { get; } = new NameValueCollection();

        /// <summary>
        /// Gets or sets the HTTP response body contents.
        /// </summary>
        /// <definition>
        /// public HttpContent? Content { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpContent? Content { get; set; }

        /// <summary>
        /// Gets or sets the default encoding when creating new HttpResponse instances.
        /// </summary>
        /// <definition>
        /// public static Encoding DefaultEncoding { get; set; }
        /// </definition>
        /// <type>
        /// Static Property
        /// </type>
        /// <remarks>
        /// This property is no longer useful and ins't used anywhere. Please, avoid using it.
        /// </remarks>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        /// <static>True</static>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This property is deprecated and ins't used anywhere. Please, avoid using it.")]
        public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets whether the HTTP response can be sent chunked.
        /// </summary>
        /// <definition>
        /// public bool SendChunked { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string GetRawHttpResponse(bool includeBody = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"HTTP/1.1 {(int)Status} {Regex.Replace(Status.ToString(), "(?<=[a-z])([A-Z])", " $1", RegexOptions.Compiled)}");
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse(HttpStatusCode status)
        {
            this.Status = status;
            this.Content = null;
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <definition>
        /// public HttpResponse(int status)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse(int status)
        {
            this.Status = (HttpStatusCode)status;
            this.Content = null;
        }

        /// <summary>
        /// Sets a cookie and sends it in the response to be set by the client.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        /// <definition>
        /// public void SetCookie(string name, string value)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void SetCookie(string name, string value)
        {
            Headers.Add("Set-Cookie", $"{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}");
        }

        /// <summary>
        /// Sets a cookie and sends it in the response to be set by the client.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        /// <param name="expires">The cookie expirity date.</param>
        /// <param name="maxAge">The cookie max duration after being set.</param>
        /// <param name="domain">The domain where the cookie will be valid.</param>
        /// <param name="path">The path where the cookie will be valid.</param>
        /// <param name="secure">Determines if the cookie will only be stored in an secure context.</param>
        /// <param name="httpOnly">Determines if the cookie will be only available in the HTTP context.</param>
        /// <param name="sameSite">The cookie SameSite parameter.</param>
        /// <definition>
        /// public void SetCookie(string name, string value, DateTime? expires, TimeSpan? maxAge, string? domain, string? path, bool? secure, bool? httpOnly)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void SetCookie(string name, string value, DateTime? expires, TimeSpan? maxAge, string? domain, string? path, bool? secure, bool? httpOnly, string? sameSite)
        {
            List<string> syntax = new List<string>();
            syntax.Add($"{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}");
            if (expires != null)
            {
                syntax.Add($"Expires={expires.Value.ToUniversalTime():r}");
            }
            if (maxAge != null)
            {
                syntax.Add($"Max-Age={maxAge.Value.TotalSeconds}");
            }
            if (domain != null)
            {
                syntax.Add($"Domain={domain}");
            }
            if (path != null)
            {
                syntax.Add($"Path={path}");
            }
            if (secure == true)
            {
                syntax.Add($"Secure");
            }
            if (httpOnly == true)
            {
                syntax.Add($"HttpOnly");
            }
            if (sameSite != null)
            {
                syntax.Add($"SameSite=${sameSite}");
            }

            Headers.Add("Set-Cookie", String.Join("; ", syntax));
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

        internal long CalcHeadersSize()
        {
            long l = 0;
            // RFC-5987 tells that headers should use UTF-8 characters.
            l += Encoding.UTF8.GetByteCount(GetRawHttpResponse(false));
            return l;
        }
    }
}
