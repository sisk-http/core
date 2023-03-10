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
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpServerConfiguration : IDisposable
    {
        /// <summary>
        /// Gets or sets advanced flags and configuration settings for the HTTP server.
        /// </summary>
        public HttpServerFlags Flags { get; set; } = new HttpServerFlags();

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> object which the HTTP server will write HTTP server access messages to.
        /// </summary>
        /// <remarks>
        /// This property defaults to Console.Out. By setting this property to null, no output will be written, completely ignoring the `Verbose` property.
        /// </remarks>
        /// <definition>
        /// public TextWriter? AccessLogsStream { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public TextWriter? AccessLogsStream { get; set; } = Console.Out;

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> object which the HTTP server will write HTTP server error transcriptions to.
        /// </summary>
        /// <remarks>
        /// This stream could be empty if ThrowExceptions is true.
        /// </remarks>
        /// <definition>
        /// public TextWriter? ErrorsLogsStream { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public TextWriter? ErrorsLogsStream { get; set; }

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve remote (IP) addresses by the X-Forwarded-For header. This option is useful if you are using Sisk through a reverse proxy.
        /// </summary>
        /// <definition>
        /// public bool ResolveForwardedOriginAddress { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        /// <remarks>
        /// Leave it as "0" to set the maximum content length to unlimited.
        /// </remarks>
        public long MaximumContentLength { get; set; } = 0;

        /// <summary>
        /// Gets or sets the message level the console will write. This property is now deprecated. Use <see cref="AccessLogsStream"/> or <see cref="ErrorsLogsStream"/> instead.
        /// </summary>
        /// <definition>
        /// [Obsolete]
        /// [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        /// public VerboseMode Verbose { get; set; }
        /// </definition>
        /// <remarks>
        /// Since Sisk 0.8.1, this property was deprecated. The defaults for the <see cref="AccessLogsStream"/> it's the <see cref="VerboseMode.Detailed"/> verbose mode.
        /// </remarks>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This property is deprecated and ins't used anymore. Please, use AccessLogsStream and ErrorLogsStream instead.")]
        public VerboseMode Verbose { get; set; } = VerboseMode.Normal;

        /// <summary>
        /// Gets or sets whether the server should include the "X-Request-Id" header in response headers.
        /// </summary>
        /// <definition>
        /// public bool IncludeRequestIdHeader { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool ThrowExceptions { get; set; } = false;

        /// <summary>
        /// Creates an new <see cref="HttpServerConfiguration"/> instance with no parameters.
        /// </summary>
        /// <definition>
        /// public HttpServerConfiguration()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Dispose()
        {
            ListeningHosts.Clear();
            AccessLogsStream?.Close();
            ErrorsLogsStream?.Close();
        }
    }

    /// <summary>
    /// Specifies the message level the <see cref="HttpServer"/> should display.
    /// </summary>
    /// <definition>
    /// public enum VerboseMode
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public enum VerboseMode
    {
        /// <summary>
        /// No message will be written.
        /// </summary>
        Silent,

        /// <summary>
        /// Only summary messages will be written.
        /// </summary>
        Normal,

        /// <summary>
        /// Detailed messages will be written.
        /// </summary>
        Detailed
    }
}
