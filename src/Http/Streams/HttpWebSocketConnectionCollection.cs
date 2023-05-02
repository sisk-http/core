using System;
using System.Collections.Generic;
using System.Diagnostics.Tracing;
using System.Linq;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// Provides a managed object to manage <see cref="HttpWebSocket"/> connections.
    /// </summary>
    /// <definition>
    /// public sealed class HttpWebSocketConnectionCollection
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class HttpWebSocketConnectionCollection
    {
        internal List<HttpWebSocket> _ws = new List<HttpWebSocket>();

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpWebSocket"/> is registered in this collection.
        /// </summary>
        /// <definition>
        /// public event WebSocketRegistrationHandler? OnRegister;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event WebSocketRegistrationHandler? OnWebSocketRegister;

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpWebSocket"/> is closed and removed from this collection.
        /// </summary>
        /// <definition>
        /// public event EventSourceUnregistrationHandler? OnEventSourceUnregistration;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event WebSocketRegistrationHandler? OnWebSocketUnregister;

        internal HttpWebSocketConnectionCollection() { }

        internal void RegisterWebSocket([In, Out] HttpWebSocket src)
        {
            if (src.identifier != null && !_ws.Contains(src))
            {
                _ws.Add(src);
                if (OnWebSocketRegister != null)
                    OnWebSocketRegister(this, src);
            }
        }

        internal void UnregisterWebSocket([In] HttpWebSocket ws)
        {
            if (_ws.Remove(ws) && OnWebSocketUnregister != null)
            {
                OnWebSocketUnregister(this, ws);
            }
        }

        /// <summary>
        /// Gets the Web Sockect connection for the specified identifier.
        /// </summary>
        /// <param name="identifier">The Web Socket identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket? GetByIdentifier(string identifier)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpWebSocket? GetByIdentifier(string identifier)
        {
            HttpWebSocket? src = _ws.Where(es => !es.isClosed && es.Identifier == identifier).FirstOrDefault();
            return src;
        }

        /// <summary>
        /// Gets all actives <see cref="HttpWebSocket"/> instances that matches their identifier predicate.
        /// </summary>
        /// <param name="predicate">The expression on the an non-empty Web Socket identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket[] Find(Func&lt;string, bool&gt; predicate)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpWebSocket[] Find(Func<string, bool> predicate)
        {
            return _ws.Where(e =>
            {
                if (e.isClosed || e.Identifier == null) return false;
                return predicate(e.Identifier);
            }).ToArray();
        }

        /// <summary>
        /// Gets all actives <see cref="HttpWebSocket"/> instances.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket[] All()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpWebSocket[] All()
        {
            return _ws.Where(e => !e.isClosed).ToArray();
        }

        /// <summary>
        /// Closes all registered and active <see cref="HttpWebSocket"/> in this collections.
        /// </summary>
        /// <definition>
        /// public void DropAll()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void DropAll()
        {
            foreach (HttpWebSocket es in _ws) es.Close();
        }
    }

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpWebSocketConnectionCollection"/> registers an new web socket connection.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpWebSocketConnectionCollection"/> object.</param>
    /// <param name="ws">Represents the registered <see cref="HttpWebSocket"/> web socket connection.</param>
    /// <definition>
    /// public delegate void WebSocketRegistrationHandler(object sender, HttpWebSocket ws);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void WebSocketRegistrationHandler(object sender, HttpWebSocket ws);

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpWebSocketConnectionCollection"/> is removed and had it's connection closed.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpWebSocketConnectionCollection"/> object.</param>
    /// <param name="ws">Represents the closed <see cref="HttpWebSocket"/> web socket connection.</param>
    /// <definition>
    /// public delegate void WebSocketUnregistrationHandler(object sender, HttpWebSocket ws);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void WebSocketUnregistrationHandler(object sender, HttpWebSocket ws);
}
