namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a structure to contain a listener port for an <see cref="ListeningHost"/> instance.
    /// </summary>
    /// <definition>
    /// public struct ListeningPort
    /// </definition>
    /// <type>
    /// Struct
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public struct ListeningPort
    {
        /// <summary>
        /// Gets or sets the port where the server will listen.
        /// </summary>
        /// <definition>
        /// public int Port { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets whether the server should listen to this port securely (SSL).
        /// </summary>
        /// <definition>
        /// public bool Secure { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Secure { get; set; }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port.
        /// </summary>
        /// <param name="port">The port the server will listen on. If this port is the default HTTPS port (443), the class will have the property <see cref="Secure"/> to true.</param>
        /// <definition>
        /// public ListeningPort(int port)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningPort(int port)
        {
            this.Port = port;
            this.Secure = Port == 443;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port and secure context.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        /// <definition>
        /// public ListeningPort(int port, bool secure)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningPort(int port, bool secure)
        {
            this.Port = port;
            this.Secure = secure;
        }
    }
}
