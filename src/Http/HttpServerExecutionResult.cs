namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents the function that is called when a server receives and computes a request.
    /// </summary>
    /// <param name="sender">The <see cref="HttpServer"/> calling the function.</param>
    /// <param name="e">Server request and operation information.</param>
    /// <definition>
    /// public delegate void ServerExecutionEventHandler(object sender, HttpServerExecutionResult e);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public delegate void ServerExecutionEventHandler(object sender, HttpServerExecutionResult e);

    /// <summary>
    /// Represents a function that is called when the server receives an HTTP request.
    /// </summary>
    /// <param name="sender">The <see cref="HttpServer"/> calling the function.</param>
    /// <param name="request">The received request.</param>
    /// <definition>
    /// public delegate void ReceiveRequestEventHandler(object sender, HttpRequest request);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public delegate void ReceiveRequestEventHandler(object sender, HttpRequest request);

    /// <summary>
    /// Represents the results of executing a request on the server.
    /// </summary>
    /// <definition>
    /// public class HttpServerExecutionResult
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpServerExecutionResult
    {
        /// <summary>
        /// Represents the request received in this diagnosis.
        /// </summary>
        /// <definition>
        /// public HttpRequest Request { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpRequest Request { get; internal set; } = null!;

        /// <summary>
        /// Represents the response sent by the server.
        /// </summary>
        /// <definition>
        /// public HttpResponse? Response { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse? Response { get; internal set; }

        /// <summary>
        /// Represents the status of server operation.
        /// </summary>
        /// <definition>
        /// public HttpServerExecutionStatus Status { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpServerExecutionStatus Status { get; internal set; }

        /// <summary>
        /// Gets the exception that was thrown when executing the route, if any.
        /// </summary>
        /// <definition>
        /// public Exception? ServerException { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Exception? ServerException { get; internal set; }

        /// <summary>
        /// Gets an boolean indicating if this execution status is an success status.
        /// </summary>
        /// <definition>
        /// public bool IsSuccessStatus { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IsSuccessStatus { get => Status == HttpServerExecutionStatus.Executed || Status == HttpServerExecutionStatus.EventSourceClosed; }

        /// <summary>
        /// Gets the request size in bytes.
        /// </summary>
        /// <definition>
        /// public long RequestSize { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public long RequestSize { get; internal set; }

        /// <summary>
        /// Gets the response size in bytes, if any.
        /// </summary>
        /// <definition>
        /// public long ResponseSize { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public long ResponseSize { get; internal set; }

        internal HttpServerExecutionResult() { }
    }

    /// <summary>
    /// Represents the status of an execution of a request on an <see cref="HttpServer"/>.
    /// </summary>
    /// <definition>
    /// public enum HttpServerExecutionStatus
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public enum HttpServerExecutionStatus
    {
        /// <summary>
        /// Represents that the request was executed by a router and its response was delivered.
        /// </summary>
        Executed,

        /// <summary>
        /// Represents that the request sent an request body, but it's method does not allow sending of contents.
        /// </summary>
        ContentServedOnNotSupportedMethod,

        /// <summary>
        /// Represents that the content of the request is too large than what was configured on the server.
        /// </summary>
        ContentTooLarge,

        /// <summary>
        /// Represents that an Event Source Events connection has been closed on the HTTP server.
        /// </summary>
        EventSourceClosed,

        /// <summary>
        /// Represents that the router did not deliver a response to the received request.
        /// </summary>
        NoResponse,

        /// <summary>
        /// Represents that the client did not correctly specify a host in the request.
        /// </summary>
        DnsFailed,

        /// <summary>
        /// Represents that the client requested an host that's not been set up on this server.
        /// </summary>
        DnsUnknownHost,

        /// <summary>
        /// Indicates that the server encountered an exception while processing the request.
        /// </summary>
        ExceptionThrown,

        /// <summary>
        /// Indicates that the router encontered an uncaught exception while calling it's callback function.
        /// </summary>
        UncaughtExceptionThrown,

        /// <summary>
        /// Indicates that the DNS was successful, however the matched <see cref="ListeningHost"/> does not have an valid initialized router .
        /// </summary>
        ListeningHostNotReady
    }
}
