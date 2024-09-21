// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningHostRepository.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Collections;
using System.Runtime.InteropServices;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an fluent repository of <see cref="ListeningHost"/> that can add, modify, or remove listening hosts while an <see cref="HttpServer"/> is running.
    /// </summary>
    public sealed class ListeningHostRepository : ICollection<ListeningHost>, IEnumerable<ListeningHost>
    {
        private readonly List<ListeningHost> _hosts = new List<ListeningHost>();

        /// <summary>
        /// Creates a new instance of an empty <see cref="ListeningHostRepository"/>.
        /// </summary>
        public ListeningHostRepository()
        {
        }


        /// <summary>
        /// Creates a new instance of an <see cref="ListeningHostRepository"/> copying the items from another collection of <see cref="ListeningHost"/>.
        /// </summary>
        /// <param name="hosts">The collection which stores the <see cref="ListeningHost"/> which will be copied to this repository.</param>
        public ListeningHostRepository(IEnumerable<ListeningHost> hosts)
        {
            this._hosts.AddRange(hosts);
        }

        /// <summary>
        /// Gets the number of elements contained in this <see cref="ListeningHostRepository"/>.
        /// </summary>
        public int Count => this._hosts.Count;

        /// <summary>
        /// Gets an boolean indicating if this <see cref="ListeningHostRepository"/> is read only. This property always returns <c>true</c>.
        /// </summary>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a listeninghost to this repository. If this listeninghost already exists in this class, an exception will be thrown.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to add to this collection.</param>
        public void Add(ListeningHost item)
        {
            if (this.Contains(item)) throw new ArgumentOutOfRangeException(SR.ListeningHostRepository_Duplicate);
            this._hosts.Add(item);
        }

        /// <summary>
        /// Removes all listeninghosts from this repository.
        /// </summary>
        public void Clear()
        {
            this._hosts.Clear();
        }

        /// <summary>
        /// Determines if an <see cref="ListeningHost"/> is present in this repository.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to check if is present in this repository.</param>
        public bool Contains(ListeningHost item)
        {
            return this._hosts.Contains(item);
        }

        /// <summary>
        /// Copies all elements from this repository to another compatible repository.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        public void CopyTo(ListeningHost[] array, int arrayIndex)
        {
            this._hosts.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
        /// </summary>
        public IEnumerator<ListeningHost> GetEnumerator()
        {
            return this._hosts.GetEnumerator();
        }

        /// <summary>
        /// Try to remove a <see cref="ListeningHost"/> from this repository. If the item is removed, this methods returns <c>true</c>.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to be removed.</param>
        public bool Remove(ListeningHost item)
        {
            return this._hosts.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
        /// </summary>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets or sets a listening host through its index.
        /// </summary>
        /// <param name="index">The Listening Host index</param>
        public ListeningHost this[int index] { get => this._hosts[index]; set => this._hosts[index] = value; }

        internal ListeningHost? GetRequestMatchingListeningHost(string incomingHost, in int incomingPort)
        {
            lock (this._hosts)
            {
                Span<ListeningHost> hosts = CollectionsMarshal.AsSpan(this._hosts);
                for (int H = 0; H < hosts.Length; H++)
                {
                    ListeningHost h = hosts[H];

                    Span<ListeningPort> ports = h.Ports.AsSpan();
                    for (int P = 0; P < ports.Length; P++)
                    {
                        ref ListeningPort p = ref ports[P];

                        if (p.Port == incomingPort && HttpStringInternals.IsDnsMatch(p.Hostname, incomingHost))
                        {
                            return h;
                        }
                    }
                }
            }
            return null;
        }
    }
}
