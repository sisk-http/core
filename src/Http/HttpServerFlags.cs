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
        /// Creates an new <see cref="HttpServerFlags"/> instance with default flags values.
        /// </summary>
        public HttpServerFlags()
        {
        }
    }
}
