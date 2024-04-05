// The Sisk Framework source code
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
    /// <definition>
    /// public class HttpServerConfiguration : IDisposable
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class HttpServerConfiguration : IDisposable
    {
        /// <summary>
        /// Gets or sets advanced flags and configuration settings for the HTTP server.
        /// </summary>
        /// <definition>
        /// public HttpServerFlags Flags { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpServerFlags Flags { get; set; } = new HttpServerFlags();

        /// <summary>
        /// Gets or sets the access logging format for incoming HTTP requests.
        /// </summary>
        /// <definition>
        /// public string AccessLogsFormat { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string AccessLogsFormat { get; set; } = "%dd-%dm-%dy %th:%ti:%ts %ls %ri %rm %rs://%ra%rz%rq [%sc %sd] %lin -> %lou in %lmsms";

        /// <summary>
        /// Gets or sets the default <see cref="CultureInfo"/> object which the HTTP server will apply to the request handlers and callbacks thread.
        /// </summary>
        /// <definition>
        /// public CultureInfo? DefaultCultureInfo { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public CultureInfo? DefaultCultureInfo { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="LogStream"/> object which the HTTP server will write HTTP server access messages to.
        /// </summary>
        /// <definition>
        /// public TextWriter? AccessLogsStream { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public LogStream? AccessLogsStream { get; set; } = null;

        /// <summary>
        /// Gets or sets the <see cref="LogStream"/> object which the HTTP server will write HTTP server error transcriptions to.
        /// </summary>
        /// <definition>
        /// public TextWriter? ErrorsLogsStream { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public LogStream? ErrorsLogsStream { get; set; } = LogStream.ConsoleOutput;

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve remote (IP) addresses by the X-Forwarded-For header. This option is useful if you are using Sisk through a reverse proxy.
        /// </summary>
        /// <definition>
        /// public bool ResolveForwardedOriginAddress { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool ResolveForwardedOriginAddress { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve remote forwarded hosts by the header X-Forwarded-Host.
        /// </summary>
        /// <definition>
        /// public bool ResolveForwardedOriginHost { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool ResolveForwardedOriginHost { get; set; } = false;

        /// <summary>
        /// Gets or sets the default encoding for sending and decoding messages.
        /// </summary>
        /// <definition>
        /// public Encoding DefaultEncoding { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the maximum size of a request body before it is closed by the socket.
        /// </summary>
        /// <definition>
        /// public long MaximumContentLength { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <remarks>
        /// Leave it as "0" to set the maximum content length to unlimited.
        /// </remarks> 
        public long MaximumContentLength { get; set; } = 0;

        /// <summary>
        /// Gets or sets whether the server should include the "X-Request-Id" header in response headers.
        /// </summary>
        /// <definition>
        /// public bool IncludeRequestIdHeader { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IncludeRequestIdHeader { get; set; } = false;

        /// <summary>
        /// Gets or sets the listening hosts repository that the <see cref="HttpServer"/> instance will listen to.
        /// </summary>
        /// <definition>
        /// public ListeningHostRepository ListeningHosts { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public ListeningHostRepository ListeningHosts { get; set; } = new ListeningHostRepository();

        /// <summary>
        /// Gets or sets whether the server should throw exceptions instead of reporting it on <see cref="HttpServerExecutionStatus"/> if any is thrown while processing requests.
        /// </summary>
        /// <definition>
        /// public bool ThrowExceptions { get; set; } = false;
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool ThrowExceptions { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the client should mantain an persistent connection
        /// with the HTTP server.
        /// </summary>
        /// <definition>
        /// public bool KeepAlive { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool KeepAlive { get; set; } = true;

        /// <summary>
        /// Creates an new <see cref="HttpServerConfiguration"/> instance with no parameters.
        /// </summary>
        /// <definition>
        /// public HttpServerConfiguration()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpServerConfiguration()
        {
        }

        /// <summary>
        /// Frees the resources and invalidates this instance.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Dispose()
        {
            ListeningHosts.Clear();
            AccessLogsStream?.Close();
            ErrorsLogsStream?.Close();
        }
    }
}
