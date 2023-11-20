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
    /// <definition>
    /// public class ListeningHost
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class ListeningHost
    {
        private ListeningPort[] _ports = null!;
        internal int[] _numericPorts = null!;

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        /// <returns></returns>
        /// <definition>
        /// public override bool Equals(object? obj)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override bool Equals(object? obj)
        {
            if (obj is ListeningHost other)
            {
                if (other == null) return false;
                if (other._ports.Length != _ports.Length) return false;

                for (int i = 0; i < _ports.Length; i++)
                {
                    ListeningPort A = this._ports[i];
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
        /// <returns></returns>
        /// <definition>
        /// public override int GetHashCode()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var port in _ports)
            {
                hashCode ^= port.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Gets whether this <see cref="ListeningHost"/> can be listened by it's host <see cref="HttpServer"/>.
        /// </summary>
        /// <definition>
        /// public bool CanListen { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool CanListen { get => Router is not null; }

        /// <summary>
        /// Gets or sets the CORS sharing policy object.
        /// </summary>
        /// <definition>
        /// public Entity.CrossOriginResourceSharingHeaders? CrossOriginResourceSharingPolicy { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Entity.CrossOriginResourceSharingHeaders? CrossOriginResourceSharingPolicy { get; set; }

        /// <summary>
        /// Gets or sets a label for this Listening Host.
        /// </summary>
        /// <definition>
        /// public string? Label { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? Label { get; set; } = null;

        /// <summary>
        /// Gets or sets the ports that this host will listen on.
        /// </summary>
        /// <definition>
        /// public ListeningPort[] Ports { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
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
        /// <definition>
        /// public Router? Router { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Router? Router { get; set; }

        /// <summary>
        /// Creates an new empty <see cref="ListeningHost"/> instance.
        /// </summary>
        /// <definition>
        /// public ListeningHost()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningHost()
        {
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> instance with given URL.
        /// </summary>
        /// <param name="uri">The well formatted URL with scheme, hostname and port.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        /// <definition>
        /// public ListeningHost(string uri, Router r)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningHost(string uri, Router r)
        {
            this.Ports = new ListeningPort[] { new ListeningPort(uri) };
            this.Router = r;
        }
    }
}
