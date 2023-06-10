using Sisk.Core.Routing;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides advanced fields for Sisk server behavior.
    /// </summary>
    public class HttpServerFlags
    {
        /// <summary>
        /// Determines if the HTTP server should drop requests which has content body in GET, OPTIONS, HEAD and TRACE methods.
        /// </summary>
        public bool ThrowContentOnNonSemanticMethods = true;

        /// <summary>
        /// Determines the HTTP header name of the request ID.
        /// </summary>
        public string HeaderNameRequestId = "X-Request-Id";

        /// <summary>
        /// Determines if the HTTP server automatically should send CORS headers if set.
        /// </summary>
        public bool SendCorsHeaders = true;

        /// <summary>
        /// Determines if the HTTP server should automatically send HTTP headers of an pre-processed GET response if the request is using HEAD method.
        /// </summary>
        public bool TreatHeadAsGetMethod = true;

        /// <summary>
        /// Determines if the HTTP server should write log to OPTIONS requests.
        /// </summary>
        public LogOutput OptionsLogMode = LogOutput.Both;

        /// <summary>
        /// Determines if the HTTP server should send the X-Powered-By header in all responses.
        /// </summary>
        public bool SendSiskHeader = true;

        /// <summary>
        /// Determines the WebSocket buffer initial and max length.
        /// </summary>
        public int WebSocketBufferSize = 1024;

        /// <summary>
        /// Determines if the HTTP server should convert request headers encoding to the content encoding.
        /// </summary>
        public bool NormalizeHeadersEncodings = true;

        /// <summary>
        /// Determines if the HTTP server should automatically rewrite paths to end with /. Does not works with Regex routes.
        /// </summary>
        public bool ForceTrailingSlash = false;

        /// <summary>
        /// Creates an new <see cref="HttpServerFlags"/> instance with default flags values.
        /// </summary>
        public HttpServerFlags()
        {
        }
    }
}
