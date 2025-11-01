// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningPort.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Net.Sockets;

namespace Sisk.Core.Http {
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
    public readonly struct ListeningPort : IEquatable<ListeningPort>, IParsable<ListeningPort> {
        /// <summary>
        /// Gets the DNS hostname pattern where this listening port will refer.
        /// </summary>
        public string Hostname { get; }

        /// <summary>
        /// Gets the port where this listening port will refer.
        /// </summary>
        public ushort Port { get; }

        /// <summary>
        /// Gets whether the server should listen to this port securely (SSL).
        /// </summary>
        public bool Secure { get; }

        /// <summary>
        /// Gets where this listening port prefix is listening to.
        /// </summary>
        public string Path { get; }

        /// <summary>
        /// Gets an boolean indicating if this listening port has an non-rooted path.
        /// </summary>
        public bool IsPathRoot { get => Path != "/" && Path != ""; }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with default parameters.
        /// </summary>
        public ListeningPort () {
            Hostname = "localhost";
            Port = 80;
            Secure = false;
            Path = "/";
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on. If this port is the default HTTPS port (443), the class will have the property <see cref="Secure"/> to true.</param>
        public ListeningPort ( ushort port ) {
            Hostname = "localhost";
            Port = port;
            Secure = Port == 443;
            Path = "/";
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port and secure context at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        public ListeningPort ( ushort port, bool secure ) {
            Hostname = "localhost";
            Port = port;
            Secure = secure;
            Path = "/";
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port, secure context and hostname.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        /// <param name="hostname">The hostname DNS pattern the server will listen to.</param>
        public ListeningPort ( bool secure, string hostname, ushort port ) {
            Hostname = hostname;
            Port = port;
            Secure = secure;
            Path = "/";
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port, secure context, hostname
        /// and path.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        /// <param name="hostname">The hostname DNS pattern the server will listen to.</param>
        /// <param name="path">The prefix path.</param>
        public ListeningPort ( bool secure, string hostname, ushort port, string path ) {
            Hostname = hostname;
            Port = port;
            Secure = secure;
            Path = path;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified URI.
        /// </summary>
        /// <param name="uri">The URI component that will be parsed to the listening port format.</param>
        public ListeningPort ( string uri ) {
            if (ParseCore ( uri ) is { } result) {
                Secure = result.secure;
                Hostname = result.hostname;
                Port = result.port;
                Path = result.path;
            }
            else {
                throw new ArgumentException ( SR.ListeningPort_Parser_InvalidInput );
            }
        }

        /// <summary>
        /// Resolves the listening IP address from the hostname.
        /// </summary>
        /// <returns>The resolved <see cref="IPAddress"/>.</returns>
        /// <exception cref="SocketException">Thrown when DNS resolution fails.</exception>
        public IPAddress ResolveListeningIPAddress () {
            if (IPAddress.TryParse ( Hostname, out var ipaddress )) {
                return ipaddress;
            }
            else {
                IPHostEntry entry = Dns.GetHostEntry ( Hostname );
                return entry.AddressList.First ( a => a.AddressFamily == AddressFamily.InterNetwork );
            }
        }

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        public override bool Equals ( object? obj ) {
            if (obj is ListeningPort p) {
                return Equals ( p );
            }
            return false;
        }

        /// <summary>
        /// Determines if this <see cref="ListeningPort"/> is equals to another <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="other">The another object which will be used to compare.</param>
        public bool Equals ( ListeningPort other ) {
            return GetHashCode ().Equals ( other.GetHashCode () );
        }

        /// <summary>
        /// Gets the hash code for this listening port.
        /// </summary>
        public override int GetHashCode () {
            return HashCode.Combine ( Hostname, Port, Secure, Path );
        }

        /// <summary>
        /// Gets an <see cref="ListeningPort"/> object with an random insecure port at the default loopback address.
        /// </summary>
        public static ListeningPort GetRandomPort () {
            TcpListener l = new TcpListener ( IPAddress.Loopback, 0 );
            l.Start ();
            ushort port = (ushort) ((IPEndPoint) l.LocalEndpoint).Port;
            l.Stop ();
            return new ListeningPort ( port, false );
        }

        /// <summary>
        /// Gets an string representation of this <see cref="ListeningPort"/>.
        /// </summary>
        public override string ToString () {
            return ToString ( true );
        }

        /// <summary>
        /// Gets an string representation of this <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="includePath">Optional. Defines whether the path should be included in the result string.</param>
        public string ToString ( bool includePath = true ) {
            if (includePath) {
                return $"{(Secure ? "https" : "http")}://{Hostname}:{Port}{Path.TrimEnd ( '/' )}/";
            }
            else {
                return $"{(Secure ? "https" : "http")}://{Hostname}:{Port}/";
            }
        }

        /// <summary>
        /// Parses a string into a <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        public static ListeningPort Parse ( string s ) {
            return new ListeningPort ( s );
        }

        /// <summary>
        /// Parses a string into a <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about s.</param>
        public static ListeningPort Parse ( string s, IFormatProvider? provider ) {
            return new ListeningPort ( s );
        }

        /// <summary>
        /// Tries to parse a string into a <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="s">The string to parse.</param>
        /// <param name="provider">An object that provides culture-specific formatting information about s.</param>
        /// <param name="result">When this method returns, contains the result of successfully parsing s or an undefined value on failure.</param>
        public static bool TryParse ( [NotNullWhen ( true )] string? s, IFormatProvider? provider, [MaybeNullWhen ( false )] out ListeningPort result ) {
            if (s is null) {
                result = default;
                return false;
            }
            if (ParseCore ( s ) is { } n) {
                result = new ListeningPort ( n.secure, n.hostname, n.port, n.path );
                return true;
            }
            else {
                result = default;
                return false;
            }
        }

        static (bool secure, string hostname, ushort port, string path)? ParseCore ( string? s ) {

            if (string.IsNullOrEmpty ( s ))
                return null;

            if (ushort.TryParse ( s, out ushort nport ))
                return (false, "localhost", nport, "/");

            if (s.StartsWith ( "http", StringComparison.Ordinal )) {
                int schemeIndex = s.IndexOf ( ':' );
                if (schemeIndex <= 0)
                    return null;

                string scheme = s [ ..schemeIndex ];
                if (scheme != "http" && scheme != "https")
                    return null;

                string hostname;
                ushort port;

                int pathIndex = s.IndexOf ( '/', schemeIndex + 3 );
                int portIndex = s.IndexOf ( ':', schemeIndex + 3 );
                if (portIndex < 0) {
                    // does not includes a port
                    hostname = s.Substring ( schemeIndex + 3, pathIndex - schemeIndex - 3 );
                    port = scheme == "http" ? (ushort) 80 : (ushort) 443;
                }
                else {
                    // does includes a port
                    hostname = s.Substring ( schemeIndex + 3, portIndex - schemeIndex - 3 );
                    string portStr = pathIndex switch {
                        -1 => s [ (portIndex + 1).. ],
                        _ => s.Substring ( portIndex + 1, pathIndex - portIndex - 1 )
                    };
                    if (!ushort.TryParse ( portStr, out port )) {
                        return null;
                    }
                }

                string path;
                if (pathIndex < 0) {
                    path = "/";
                }
                else {
                    path = s [ pathIndex.. ];
                }

                return (scheme == "https", hostname, port, path);
            }

            if (s.Contains ( ':' )) {
                string [] parts = s.Split ( ':' );
                if (parts.Length != 2)
                    return null;
                if (!ushort.TryParse ( parts [ 1 ], out ushort port ))
                    return null;
                return (false, parts [ 0 ], port, "/");
            }

            return null;
        }

        /// <inheritdoc/>
        public static bool operator == ( ListeningPort left, ListeningPort right ) {
            return left.Equals ( right );
        }

        /// <inheritdoc/>
        public static bool operator != ( ListeningPort left, ListeningPort right ) {
            return !(left == right);
        }
    }
}
