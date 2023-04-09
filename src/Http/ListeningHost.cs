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
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public unsafe class ListeningHost
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
            ListeningHost? other = (obj as ListeningHost);
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Entity.CrossOriginResourceSharingHeaders CrossOriginResourceSharingPolicy { get; set; } = CrossOriginResourceSharingHeaders.Empty;

        /// <summary>
        /// Gets or sets a label for this Listening Host.
        /// </summary>
        /// <definition>
        /// public string? Label { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string? Label { get; set; } = null;

        /// <summary>
        /// Gets or sets the hostname (without the port) that this host will listen on the local machine.
        /// </summary>
        /// <definition>
        /// public string Hostname { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the ports that this host will listen on.
        /// </summary>
        /// <definition>
        /// public ListeningPort[] Ports { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Router? Router { get; set; }

        private ListeningHost()
        {
            Hostname = null!;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> value with given parameters.
        /// </summary>
        /// <param name="hostname">The hostname (without the port) that this host will listen on the local machine.</param>
        /// <param name="port">The port this host will listen on.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <definition>
        /// public ListeningHost(ListeningHostProtocol protocol, string hostname, int port, Router r)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHost(string hostname, int port, Router r) : this()
        {
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            Ports = new ListeningPort[] { new ListeningPort(port) };
            Router = r;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> value with given parameters.
        /// </summary>
        /// <param name="hostname">The hostname (without the port) that this host will listen on the local machine.</param>
        /// <param name="port">The port this host will listen on.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <definition>
        /// public ListeningHost(string hostname, ListeningPort port, Router r)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHost(string hostname, ListeningPort port, Router r) : this()
        {
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            Ports = new ListeningPort[] { port };
            Router = r;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> value with given parameters.
        /// </summary>
        /// <param name="hostname">The hostname (without the port) that this host will listen on the local machine.</param>
        /// <param name="ports">The ports which this host will listen on.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <definition>
        /// public ListeningHost(ListeningHostProtocol protocol, string hostname, int[] ports, Router r)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHost(string hostname, int[] ports, Router r) : this()
        {
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            Router = r;

            this.Ports = new ListeningPort[ports.Length];
            for (int i = 0; i < ports.Length; i++)
            {
                this.Ports[i] = new ListeningPort(ports[i]);
            }
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> value with given parameters.
        /// </summary>
        /// <param name="hostname">The hostname (without the port) that this host will listen on the local machine.</param>
        /// <param name="ports">The ports which this host will listen on.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <definition>
        /// public ListeningHost(ListeningHostProtocol protocol, string hostname, ListeningPort[] ports, Router r)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHost(string hostname, ListeningPort[] ports, Router r) : this()
        {
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            Router = r;
            Ports = ports;
        }

        /// <summary>
        /// Creates the instance of a routerless listener host without any <see cref="Sisk.Core.Routing.Router"/>. This instance will not be listened until it has a router.
        /// </summary>
        /// <param name="hostname">The hostname (without the port) that this host will listen on the local machine.</param>
        /// <param name="ports">The ports which this host will listen on.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <definition>
        /// public ListeningHost(string hostname, ListeningPort[] ports)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHost(string hostname, ListeningPort[] ports) : this()
        {
            Hostname = hostname ?? throw new ArgumentNullException(nameof(hostname));
            Ports = ports;
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHost(string uri, Router r) : this()
        {
            Uri uriInstance = new Uri(uri);
            this.Hostname = uriInstance.Host;
            this.Ports = new ListeningPort[] { new ListeningPort(uriInstance.Port, uriInstance.Scheme == "https") };
            this.Router = r;
        }
    }
}
