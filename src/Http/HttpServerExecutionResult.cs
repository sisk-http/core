// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerExecutionResult.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents the results of an request execution on the HTTP server.
    /// </summary>
    public sealed class HttpServerExecutionResult
    {
        /// <summary>
        /// Gets the <see cref="HttpRequest"/> received in this diagnosis.
        /// </summary>
        public HttpRequest Request { get; internal set; } = null!;

        /// <summary>
        /// Gets the resulted <see cref="HttpResponse"/> send by the router, if any. This object can be null if the
        /// server didn't sent any response to the client.
        /// </summary>
        public HttpResponse? Response { get; internal set; }

        /// <summary>
        /// Gets the <see cref="HttpContext"/> of this execution result.
        /// </summary>
        public HttpContext Context { get; internal set; } = null!;

        /// <summary>
        /// Gets the status of server operation.
        /// </summary>
        public HttpServerExecutionStatus Status { get; internal set; }

        /// <summary>
        /// Gets the exception that was thrown when executing the route, if any.
        /// </summary>
        public Exception? ServerException { get; internal set; }

        /// <summary>
        /// Gets an boolean indicating if this execution status is an success status.
        /// </summary>
        public bool IsSuccessStatus { get => this.Status == HttpServerExecutionStatus.Executed || this.Status == HttpServerExecutionStatus.ConnectionClosed; }

        /// <summary>
        /// Gets the estimated request size in bytes.
        /// </summary>
        public long RequestSize { get; internal set; }

        /// <summary>
        /// Gets the estimated response size in bytes, if any.
        /// </summary>
        public long ResponseSize { get; internal set; }

        /// <summary>
        /// Gets the total processing time of the HTTP session.
        /// </summary>
        public TimeSpan Elapsed { get; internal set; }

        internal HttpServerExecutionResult() { }
    }

    /// <summary>
    /// Represents the status of an execution of a request on an <see cref="HttpServer"/>.
    /// </summary>
    public enum HttpServerExecutionStatus
    {
        /// <summary>
        /// Represents that the request was closed by the HTTP server and executed by a router and its response was succesfully delivered.
        /// </summary>
        Executed,

        /// <summary>
        /// Represents that the request has sent an request body with an with a HTTP method that is not indicated for
        /// receiving request contents.
        /// </summary>
        ContentServedOnIllegalMethod,

        /// <summary>
        /// Represents that the content of the request is too large than what was configured on the server, or it's bigger than the max supported size (2GB).
        /// </summary>
        ContentTooLarge,

        /// <summary>
        /// Represents that the connection stream was closed by the client.
        /// </summary>
        ConnectionClosed,

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
        /// Indicates that the router encontered an uncaught exception while calling it's action function.
        /// </summary>
        UncaughtExceptionThrown,

        /// <summary>
        /// Indicates that the DNS was successful, however the matched <see cref="ListeningHost"/> does
        /// not have an valid initialized router.
        /// </summary>
        ListeningHostNotReady,

        /// <summary>
        /// Indicates that the server cannot or will not process the request due to something that is perceived to be a client error.
        /// </summary>
        MalformedRequest,

        /// <summary>
        /// Indicates that the HTTP server closed an unwanted remote connection.
        /// </summary>
        RemoteRequestDropped
    }
}
