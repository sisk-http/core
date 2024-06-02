﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpServerConfiguration.cs
// Repository:  https://github.com/sisk-http/core

using System.Globalization;
using System.Text;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides execution parameters for an <see cref="HttpServer"/>.
    /// </summary>
    public class HttpServerConfiguration : IDisposable
    {
        /// <summary>
        /// Gets or sets advanced flags and configuration settings for the HTTP server.
        /// </summary>
        public HttpServerFlags Flags { get; set; } = new HttpServerFlags();

        /// <summary>
        /// Gets or sets the access logging format for incoming HTTP requests.
        /// </summary>
        public string AccessLogsFormat { get; set; } = "%dd-%dm-%dy %th:%ti:%ts %ls %ri %rm %rs://%ra%rz%rq [%sc %sd] %lin -> %lou in %lmsms";

        /// <summary>
        /// Gets or sets the default <see cref="CultureInfo"/> object which the HTTP server will apply to the request handlers and callbacks thread.
        /// </summary>
        public CultureInfo? DefaultCultureInfo { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LogStream"/> object which the HTTP server will write HTTP server access messages to.
        /// </summary>
        public LogStream? AccessLogsStream { get; set; } = null;

        /// <summary>
        /// Gets or sets the <see cref="LogStream"/> object which the HTTP server will write HTTP server error transcriptions to.
        /// </summary>
        public LogStream? ErrorsLogsStream { get; set; } = LogStream.ConsoleOutput;

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve remote (IP) addresses by the X-Forwarded-For header. This option is useful if you are using
        /// Sisk through a reverse proxy or load balancer.
        /// </summary>
        public bool ResolveForwardedOriginAddress { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve remote forwarded hosts by the header X-Forwarded-Host.
        /// </summary>
        public bool ResolveForwardedOriginHost { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve the protocol (HTTP or HTTPS) used by the
        /// client to reach the current HTTP server through an proxy or load balancer.
        /// </summary>
        public bool ResolveForwardedProtocol { get; set; } = false;

        /// <summary>
        /// Gets or sets the default encoding for sending and decoding messages.
        /// </summary>
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the maximum size of a request body before it is closed by the socket.
        /// </summary>
        /// <remarks>
        /// Leave it as "0" to set the maximum content length to unlimited.
        /// </remarks> 
        public long MaximumContentLength { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether the server should include the "X-Request-Id" header in response headers.
        /// </summary>
        public bool IncludeRequestIdHeader { get; set; } = false;

        /// <summary>
        /// Gets or sets the listening hosts repository that the <see cref="HttpServer"/> instance will listen to.
        /// </summary>
        public ListeningHostRepository ListeningHosts { get; set; } = new ListeningHostRepository();

        /// <summary>
        /// Gets or sets whether the server should throw exceptions instead of reporting it on <see cref="HttpServerExecutionStatus"/> if any is thrown while processing requests.
        /// </summary>
        public bool ThrowExceptions { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the client should mantain an persistent connection
        /// with the HTTP server.
        /// </summary>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// Creates an new <see cref="HttpServerConfiguration"/> instance with no parameters.
        /// </summary>
        public HttpServerConfiguration()
        {
        }

        /// <summary>
        /// Frees the resources and invalidates this instance.
        /// </summary>
        public void Dispose()
        {
            ListeningHosts.Clear();
            AccessLogsStream?.Close();
            ErrorsLogsStream?.Close();
        }
    }
}
