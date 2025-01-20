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
        public bool IsPathRoot { get => this.Path != "/" && this.Path != ""; }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with default parameters.
        /// </summary>
        public ListeningPort () {
            this.Hostname = "localhost";
            this.Port = 80;
            this.Secure = false;
            this.Path = "/";
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on. If this port is the default HTTPS port (443), the class will have the property <see cref="Secure"/> to true.</param>
        public ListeningPort ( ushort port ) {
            this.Hostname = "localhost";
            this.Port = port;
            this.Secure = this.Port == 443;
            this.Path = "/";
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port and secure context at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        public ListeningPort ( ushort port, bool secure ) {
            this.Hostname = "localhost";
            this.Port = port;
            this.Secure = secure;
            this.Path = "/";
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port, secure context and hostname.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        /// <param name="hostname">The hostname DNS pattern the server will listen to.</param>
        public ListeningPort ( bool secure, string hostname, ushort port ) {
            this.Hostname = hostname;
            this.Port = port;
            this.Secure = secure;
            this.Path = "/";
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
            this.Hostname = hostname;
            this.Port = port;
            this.Secure = secure;
            this.Path = path;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified URI.
        /// </summary>
        /// <param name="uri">The URI component that will be parsed to the listening port format.</param>
        public ListeningPort ( string uri ) {
            if (ushort.TryParse ( uri, out ushort port )) {
                this.Hostname = "localhost";
                this.Port = port;
                this.Secure = port == 443;
                this.Path = "/";
            }
            else if (Uri.TryCreate ( uri, UriKind.RelativeOrAbsolute, out var uriResult )) {
                if (uriResult.Scheme != "http" && uriResult.Scheme != "https") {
                    throw new ArgumentException ( SR.ListeningPort_Parser_InvalidInput );
                }

                this.Hostname = uriResult.Host;
                this.Port = (ushort) uriResult.Port;
                this.Secure = string.Compare ( uriResult.Scheme, "https", true ) == 0;
                this.Path = uriResult.AbsolutePath;
            }
            else {
                throw new ArgumentException ( SR.ListeningPort_Parser_InvalidInput );
            }
        }

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        public override bool Equals ( object? obj ) {
            if (obj is ListeningPort p) {
                return this.Equals ( p );
            }
            return false;
        }

        /// <summary>
        /// Determines if this <see cref="ListeningPort"/> is equals to another <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="other">The another object which will be used to compare.</param>
        public bool Equals ( ListeningPort other ) {
            return this.GetHashCode ().Equals ( other.GetHashCode () );
        }

        /// <summary>
        /// Gets the hash code for this listening port.
        /// </summary>
        public override int GetHashCode () {
            return HashCode.Combine ( this.Hostname, this.Port, this.Secure, this.Path );
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
            return this.ToString ( true );
        }

        /// <summary>
        /// Gets an string representation of this <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="includePath">Optional. Defines whether the path should be included in the result string.</param>
        public string ToString ( bool includePath = true ) {
            if (includePath) {
                return $"{(this.Secure ? "https" : "http")}://{this.Hostname}:{this.Port}{this.Path.TrimEnd ( '/' )}/";
            }
            else {
                return $"{(this.Secure ? "https" : "http")}://{this.Hostname}:{this.Port}/";
            }
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
            try {
                result = Parse ( s, provider );
                return true;
            }
            catch {
                result = default;
                return false;
            }
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
