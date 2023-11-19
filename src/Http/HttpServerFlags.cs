// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerFlags.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;
using System.Net;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides advanced fields for Sisk server behavior.
    /// </summary>
    /// <definition>
    /// public class HttpServerFlags
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class HttpServerFlags
    {
        /// <summary>
        /// Determines if the HTTP server should drop requests which has content body in GET, OPTIONS, HEAD and TRACE methods.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>true</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool ThrowContentOnNonSemanticMethods;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public bool ThrowContentOnNonSemanticMethods = true;

        /// <summary>
        /// Determines the HTTP header name of the request ID.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>"X-Request-Id"</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool HeaderNameRequestId;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public string HeaderNameRequestId = "X-Request-Id";

        /// <summary>
        /// Determines if the HTTP server automatically should send CORS headers if set.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>true</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool SendCorsHeaders;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public bool SendCorsHeaders = true;

        /// <summary>
        /// Determines if the HTTP server should automatically send HTTP headers of an pre-processed GET response if the request is using HEAD method.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>true</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool TreatHeadAsGetMethod;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public bool TreatHeadAsGetMethod = true;

        /// <summary>
        /// Determines if the HTTP server should write log to OPTIONS requests.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>LogOutput.Both</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public LogOutput OptionsLogMode;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public LogOutput OptionsLogMode = LogOutput.Both;

        /// <summary>
        /// Determines if the HTTP server should send the X-Powered-By header in all responses.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>true</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool SendSiskHeader;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public bool SendSiskHeader = true;

        /// <summary>
        /// Determines the WebSocket buffer initial and max length.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>1024</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public int WebSocketBufferSize;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public int WebSocketBufferSize = 1024;

        /// <summary>
        /// Determines if the HTTP server should convert request headers encoding to the content encoding.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>true</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool NormalizeHeadersEncodings;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public bool NormalizeHeadersEncodings = true;

        /// <summary>
        /// Determines if the HTTP server should automatically rewrite paths to end with /. Does not works with Regex routes.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>false</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool ForceTrailingSlash;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public bool ForceTrailingSlash = false;

        /// <summary>
        /// Determines the maximum amount of time an connection can keep alive without sending or receiving any
        /// data.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>TimeSpan.FromSeconds(120)</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public TimeSpan IdleConnectionTimeout;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public TimeSpan IdleConnectionTimeout = TimeSpan.FromSeconds(120);

        /// <summary>
        /// Determines the maximum amount of time an route can process an request, including running request handlers,
        /// reading body and executing the route action. Specify zero for no limit.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>TimeSpan.Zero</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public TimeSpan RouteActionTimeout;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public TimeSpan RouteActionTimeout = TimeSpan.Zero;

        /// <summary>
        /// Determines if the request input stream should be read into an byte array after running all <see cref="RequestHandlerExecutionMode.BeforeContents"/>
        /// request handlers.
        /// </summary>
        /// <docs>
        ///     <p>
        ///         Default value: <code>true</code>
        ///     </p>
        /// </docs>
        /// <definition>
        /// public bool AutoReadRequestStream;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public bool AutoReadRequestStream = true;

        /// <summary>
        /// Creates an new <see cref="HttpServerFlags"/> instance with default flags values.
        /// </summary>
        /// <definition>
        /// public HttpServerFlags()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpServerFlags()
        {
        }
    }
}
