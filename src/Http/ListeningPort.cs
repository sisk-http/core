// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningPort.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Net.Sockets;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a structure to contain a listener port for an <see cref="ListeningHost"/> instance.
    /// </summary>
    /// <example>
    ///     <para>
    ///         A listener port represents an access point on the HTTP server.
    ///         It consists of an indicator that it should use a secure connection (HTTPS), its hostname and port.
    ///     </para>
    ///     <para>
    ///         It must start with https:// or http://, and must terminate with an /.
    ///     </para>
    ///     <para>
    ///         It is represented by the syntax:
    ///     </para>
    ///     <pre><code class="lang-none">
    ///         [http|https]://[hostname]:[port]/
    ///     </code></pre>
    ///     <para>
    ///         Examples:
    ///     </para>
    ///     <code>
    ///         http://localhost:80/
    ///         https://subdomain.domain.net:443/
    ///         http://182.32.112.223:5251/
    ///     </code>
    /// </example>
    public struct ListeningPort
    {
        /// <summary>
        /// Gets or sets the DNS hostname pattern where this listening port will refer.
        /// </summary>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the port where this listening port will refer.
        /// </summary>
        public ushort Port { get; set; }

        /// <summary>
        /// Gets or sets whether the server should listen to this port securely (SSL).
        /// </summary>
        public bool Secure { get; set; }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with default parameters.
        /// </summary>
        public ListeningPort()
        {
            Hostname = "localhost";
            Port = 80;
            Secure = false;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on. If this port is the default HTTPS port (443), the class will have the property <see cref="Secure"/> to true.</param>
        public ListeningPort(ushort port)
        {
            Hostname = "localhost";
            Port = port;
            Secure = Port == 443;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port and secure context at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        public ListeningPort(ushort port, bool secure)
        {
            Hostname = "localhost";
            Port = port;
            Secure = secure;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port, secure context and hostname.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        /// <param name="hostname">The hostname DNS pattern the server will listen to.</param>
        public ListeningPort(bool secure, string hostname, ushort port)
        {
            Hostname = hostname;
            Port = port;
            Secure = secure;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified URI.
        /// </summary>
        /// <param name="uri">The URI component that will be parsed to the listening port format.</param>
        public ListeningPort(string uri)
        {
            int schemeIndex = uri.IndexOf(":");
            if (schemeIndex == -1) throw new ArgumentException(SR.ListeningPort_Parser_UndefinedScheme);
            int portIndex = uri.IndexOf(":", schemeIndex + 3);
            if (portIndex == -1) throw new ArgumentException(SR.ListeningPort_Parser_UndefinedPort);
            int endIndex = uri.IndexOf("/", schemeIndex + 3);
            if (endIndex == -1 || !uri.EndsWith('/')) throw new ArgumentException(SR.ListeningPort_Parser_UriNotTerminatedSlash);

            string schemePart = uri.Substring(0, schemeIndex);
            string hostnamePart = uri.Substring(schemeIndex + 3, portIndex - (schemeIndex + 3));
            string portPart = uri.Substring(portIndex + 1, endIndex - (portIndex + 1));

            if (schemePart == "http")
            {
                Secure = false;
            }
            else if (schemePart == "https")
            {
                Secure = true;
            }
            else
            {
                throw new ArgumentException(SR.ListeningPort_Parser_InvalidScheme);
            }

            if (!ushort.TryParse(portPart, out ushort port)) throw new ArgumentException(SR.ListeningPort_Parser_InvalidPort);

            Port = port;
            Hostname = hostnamePart;
        }

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            ListeningPort p = (ListeningPort)obj;
            return p.Secure == Secure && p.Port == Port && p.Hostname == Hostname;
        }

        /// <summary>
        /// Gets the hash code for this listening port.
        /// </summary>
        public override int GetHashCode()
        {
            return (Secure.GetHashCode()) ^ (Port.GetHashCode()) ^ (Hostname.GetHashCode());
        }

        /// <summary>
        /// Gets an <see cref="ListeningPort"/> object with an random insecure port.
        /// </summary>
        public static ListeningPort GetRandomPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            ushort port = (ushort)((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return new ListeningPort(port, false);
        }

        /// <summary>
        /// Gets an string representation of this <see cref="ListeningPort"/>.
        /// </summary>
        public override string ToString()
        {
            return $"{(Secure ? "https" : "http")}://{Hostname}:{Port}/";
        }
    }
}
