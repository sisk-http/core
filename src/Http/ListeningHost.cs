// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningHost.cs
// Repository:  https://github.com/sisk-http/core

using System.Security.Cryptography.X509Certificates;
using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http {
    /// <summary>
    /// Provides a type to contain the fields needed by an HTTP server virtual host.
    /// </summary>
    public sealed class ListeningHost {
        internal List<ListeningPort> _ports = new List<ListeningPort> ();

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        public override bool Equals ( object? obj ) {
            if (obj is ListeningHost other) {
                return other.GetHashCode () == GetHashCode ();
            }
            else {
                return false;
            }
        }

        /// <summary>
        /// Gets the hash code for this listening host.
        /// </summary>
        public override int GetHashCode () {
            int hashCode = 9999;
            foreach (var port in _ports) {
                hashCode ^= port.GetHashCode ();
            }
            return hashCode;
        }

        /// <summary>
        /// Gets whether this <see cref="ListeningHost"/> can be listened by it's host <see cref="HttpServer"/>.
        /// </summary>
        public bool CanListen { get => Router is not null; }

        /// <summary>
        /// Gets or sets the CORS sharing policy object.
        /// </summary>
        public Entity.CrossOriginResourceSharingHeaders CrossOriginResourceSharingPolicy { get; set; }
            = new CrossOriginResourceSharingHeaders ();

        /// <summary>
        /// Gets or sets a label for this Listening Host.
        /// </summary>
        public string? Label { get; set; }

        /// <summary>
        /// Gets or sets the SSL options for this <see cref="ListeningHost"/>.
        /// </summary>
        public ListeningHostSslOptions? SslOptions { get; set; }

        /// <summary>
        /// Gets or sets the list of <see cref="ListeningPort"/> that this host will listen on.
        /// </summary>
        public IList<ListeningPort> Ports {
            get {
                return _ports;
            }
            set {
                _ports = new List<ListeningPort> ( value );
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Sisk.Core.Routing.Router"/> for this <see cref="ListeningHost"/> instance.
        /// </summary>
        public Router? Router { get; set; }

        /// <summary>
        /// Creates an new empty <see cref="ListeningHost"/> instance.
        /// </summary>
        public ListeningHost () {
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> instance with given array of <see cref="ListeningPort"/>.
        /// </summary>
        /// <param name="ports">The array of <see cref="ListeningPort"/> to listen in the <see cref="ListeningHost"/>.</param>
        public ListeningHost ( params ListeningPort [] ports ) {
            _ports = ports.ToList ();
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> instance with given URL.
        /// </summary>
        /// <param name="uri">The well formatted URL with scheme, hostname and port.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        public ListeningHost ( string uri, Router r ) {
            Ports = [ new ListeningPort ( uri ) ];
            Router = r;
        }

        internal void EnsureReady () {
            // The router does not need to be defined to start the server.
            ;
            if (_ports.Count == 0) {
                throw new InvalidOperationException ( SR.ListeningHost_NotReady_EmptyPorts );
            }

            string firstPath = _ports [ 0 ].Path;
            for (int i = 0; i < _ports.Count; i++) {
                ListeningPort port = _ports [ i ];
                if (!port.Path.StartsWith ( '/' )) {
                    throw new InvalidOperationException ( SR.ListeningHost_NotReady_InvalidPath );
                }
                if (!port.Path.Equals ( firstPath, StringComparison.OrdinalIgnoreCase )) {
                    throw new InvalidOperationException ( SR.ListeningHost_NotReady_DifferentPath );
                }
            }
        }
    }
}
