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
        /// Determines if the HTTP server should write URL query information to the access/error logs.
        /// </summary>
        public bool IncludeFullPathOnLog = false;

        /// <summary>
        /// Determines if the HTTP server should send the X-Powered-By header in all responses.
        /// </summary>
        public bool SendSiskHeader = true;

        /// <summary>
        /// Creates an new <see cref="HttpServerFlags"/> instance with default flags values.
        /// </summary>
        public HttpServerFlags()
        {
        }
    }
}
