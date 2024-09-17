// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerFlags.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides advanced fields for Sisk server behavior.
    /// </summary>
    public sealed class HttpServerFlags
    {
        /// <summary>
        /// Determines if the HTTP server should drop requests which has content body in GET, OPTIONS, HEAD and TRACE methods.
        ///     <para>
        ///         Default value: <c>true</c>
        ///     </para>
        /// </summary>
        public bool ThrowContentOnNonSemanticMethods = true;

        /// <summary>
        /// Determines if the HTTP server should matches route after URL decoding the request path.
        ///     <para>
        ///         Default value: <c>true</c>
        ///     </para>
        /// </summary>
        [Obsolete("This flag has expired and will be removed in future Sisk versions.")]
        public bool UnescapedRouteMatching = true;

        /// <summary>
        /// Determines if the HTTP server should handle requests asynchronously or if
        /// it should limit the request processing to one request per time.
        ///     <para>
        ///         Default value: <c>false</c>
        ///     </para>
        /// </summary>
        public bool AsyncRequestProcessing = true;

        /// <summary>
        /// Determines the HTTP header name of the request ID.
        ///     <para>
        ///         Default value: <c>"X-Request-Id"</c>
        ///     </para>
        /// </summary>
        public string HeaderNameRequestId = "X-Request-Id";

        /// <summary>
        /// Determines if the HTTP server automatically should send CORS headers if set.
        ///     <para>
        ///         Default value: <c>true</c>
        ///     </para>
        /// </summary>
        public bool SendCorsHeaders = true;

        /// <summary>
        /// Determines if the HTTP server should automatically send HTTP headers of an pre-processed GET response
        /// if the request is using HEAD method.
        ///     <para>
        ///         Default value: <c>true</c>
        ///     </para>
        /// </summary>

        public bool TreatHeadAsGetMethod = true;

        /// <summary>
        /// Determines if the HTTP server should write log to OPTIONS requests.
        ///     <para>
        ///         Default value: <c>LogOutput.Both</c>
        ///     </para>
        /// </summary>
        public LogOutput OptionsLogMode = LogOutput.Both;

        /// <summary>
        /// Determines if the HTTP server should send the X-Powered-By header in all responses.
        ///     <para>
        ///         Default value: <c>true</c>
        ///     </para>
        /// </summary>
        public bool SendSiskHeader = true;

        /// <summary>
        /// Determines the WebSocket buffer initial and max length.
        ///     <para>
        ///         Default value: <c>1024</c>
        ///     </para>
        /// </summary>
        public int WebSocketBufferSize = 1024;

        /// <summary>
        /// Specifies the size, in bytes, of the copy buffer of both streams (inbound and outgoing)
        /// of the response stream.
        ///     <para>
        ///         Default value: <c>81920</c>
        ///     </para>
        /// </summary>
        public int RequestStreamCopyBufferSize = 81920;

        /// <summary>
        /// Determines if the HTTP server should convert request headers encoding to the content encoding.
        ///     <para>
        ///         Default value: <c>false</c>
        ///     </para>
        /// </summary>
        public bool NormalizeHeadersEncodings = false;

        /// <summary>
        /// Determines if the HTTP server should automatically rewrite GET requests to end their path with /. This is 
        /// non-applyable to Regex routes.
        ///     <para>
        ///         Default value: <c>false</c>
        ///     </para>
        /// </summary>
        public bool ForceTrailingSlash = false;

        /// <summary>
        /// Determines the maximum amount of time an connection can keep alive without sending or receiving any
        /// data.
        ///     <para>
        ///         Default value: <c>TimeSpan.FromSeconds(120)</c>
        ///     </para>
        /// </summary>
        public TimeSpan IdleConnectionTimeout = TimeSpan.FromSeconds(120);

        /// <summary>
        /// Determines if the new span-based multipart form reader should be used. This is an experimental
        /// feature and may not be stable for production usage.
        /// <para>
        ///     Default value: <c>false</c>
        /// </para>
        /// </summary>
        public bool EnableNewMultipartFormReader = false;

        /// <summary>
        /// Creates an new <see cref="HttpServerFlags"/> instance with default flags values.
        /// </summary>
        public HttpServerFlags()
        {
        }
    }
}
