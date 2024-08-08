// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningHost.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;
using Sisk.Core.Routing;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a structure to contain the fields needed by an http server host.
    /// </summary>
    public sealed class ListeningHost
    {
        private ListeningPort[] _ports = null!;
        internal ushort[] _numericPorts = null!;

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        public override bool Equals(object? obj)
        {
            if (obj is ListeningHost other)
            {
                if (other is null) return false;
                if (other._ports.Length != _ports.Length) return false;

                for (int i = 0; i < _ports.Length; i++)
                {
                    ListeningPort A = _ports[i];
                    ListeningPort B = other._ports[i];
                    if (!A.Equals(B)) return false;
                }
                return true;
            }
            else
            {
                return false;
            }
        }

        /// <summary>
        /// Gets the hash code for this listening host.
        /// </summary>
        public override int GetHashCode()
        {
            int hashCode = 9999;
            foreach (var port in _ports)
            {
                hashCode ^= port.GetHashCode();
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
            = new CrossOriginResourceSharingHeaders();

        /// <summary>
        /// Gets or sets a label for this Listening Host.
        /// </summary>
        public string? Label { get; set; } = null;

        /// <summary>
        /// Gets or sets the ports that this host will listen on.
        /// </summary>
        public ListeningPort[] Ports
        {
            get
            {
                return _ports;
            }
            set
            {
                _ports = value;
                _numericPorts = value.Select(p => p.Port).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Sisk.Core.Routing.Router"/> for this <see cref="ListeningHost"/> instance.
        /// </summary>
        public Router? Router { get; set; }

        /// <summary>
        /// Creates an new empty <see cref="ListeningHost"/> instance.
        /// </summary>
        public ListeningHost()
        {
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> instance with given URL.
        /// </summary>
        /// <param name="uri">The well formatted URL with scheme, hostname and port.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        public ListeningHost(string uri, Router r)
        {
            Ports = new ListeningPort[] { new ListeningPort(uri) };
            Router = r;
        }
    }
}
