// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpWebSocketConnectionCollection.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// Provides a managed object to manage <see cref="HttpWebSocket"/> connections.
    /// </summary>
    public class HttpWebSocketConnectionCollection
    {
        internal List<HttpWebSocket> _ws = new List<HttpWebSocket>();

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpWebSocket"/> is registered in this collection.
        /// </summary>
        public event WebSocketRegistrationHandler? OnWebSocketRegister;

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpWebSocket"/> is closed and removed from this collection.
        /// </summary>
        public event WebSocketRegistrationHandler? OnWebSocketUnregister;

        internal HttpWebSocketConnectionCollection() { }

        internal void RegisterWebSocket(HttpWebSocket src)
        {
            if (src.identifier != null)
            {
                lock (_ws)
                {
                    // close another websockets with same identifier
                    HttpWebSocket[] wsId = Find(s => s == src.identifier);
                    foreach (HttpWebSocket ws in wsId)
                    {
                        ws.Close();
                    }
                    _ws.Add(src);
                }
                if (OnWebSocketRegister != null)
                    OnWebSocketRegister(this, src);
            }
        }

        internal void UnregisterWebSocket(HttpWebSocket ws)
        {
            lock (_ws)
            {
                if (_ws.Remove(ws) && OnWebSocketUnregister != null)
                {
                    OnWebSocketUnregister(this, ws);
                }
            }
        }

        /// <summary>
        /// Gets the Web Sockect connection for the specified identifier.
        /// </summary>
        /// <param name="identifier">The Web Socket identifier.</param>
        public HttpWebSocket? GetByIdentifier(string identifier)
        {
            lock (_ws)
            {
                HttpWebSocket? src = _ws.Where(es => !es.isClosed && es.Identifier == identifier).FirstOrDefault();
                return src;
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpWebSocket"/> instances that matches their identifier predicate.
        /// </summary>
        /// <param name="predicate">The expression on the an non-empty Web Socket identifier.</param>
        public HttpWebSocket[] Find(Func<string, bool> predicate)
        {
            lock (_ws)
            {
                return _ws.Where(e => e.Identifier != null && predicate(e.Identifier)).ToArray();
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpWebSocket"/> instances.
        /// </summary>
        public HttpWebSocket[] All()
        {
            lock (_ws)
            {
                return _ws.ToArray();
            }
        }

        /// <summary>
        /// Gets an number indicating the amount of active web socket connections.
        /// </summary>
        public int ActiveConnections { get => _ws.Count; }

        /// <summary>
        /// Closes all registered and active <see cref="HttpWebSocket"/> in this collections.
        /// </summary>
        public void DropAll()
        {
            lock (_ws)
            {
                foreach (HttpWebSocket es in _ws) es.Close();
            }
        }
    }

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpWebSocketConnectionCollection"/> registers an new web socket connection.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpWebSocketConnectionCollection"/> object.</param>
    /// <param name="ws">Represents the registered <see cref="HttpWebSocket"/> web socket connection.</param>
    public delegate void WebSocketRegistrationHandler(object sender, HttpWebSocket ws);

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpWebSocketConnectionCollection"/> is removed and had it's connection closed.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpWebSocketConnectionCollection"/> object.</param>
    /// <param name="ws">Represents the closed <see cref="HttpWebSocket"/> web socket connection.</param>
    public delegate void WebSocketUnregistrationHandler(object sender, HttpWebSocket ws);
}
