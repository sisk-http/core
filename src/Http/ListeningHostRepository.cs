// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningHostRepository.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Collections;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an fluent repository of <see cref="ListeningHost"/> that can add, modify, or remove listening hosts while an <see cref="HttpServer"/> is running.
    /// </summary>
    /// <definition>
    /// public class ListeningHostRepository : ICollection&lt;ListeningHost&gt;, IEnumerable&lt;ListeningHost&gt;
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class ListeningHostRepository : ICollection<ListeningHost>, IEnumerable<ListeningHost>
    {
        private List<ListeningHost> _hosts = new List<ListeningHost>();

        /// <summary>
        /// Creates a new instance of an empty <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <definition>
        /// public ListeningHostRepository()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningHostRepository()
        {
        }


        /// <summary>
        /// Creates a new instance of an <see cref="ListeningHostRepository"/> copying the items from another collection of <see cref="ListeningHost"/>.
        /// </summary>
        /// <param name="hosts">The collection which stores the <see cref="ListeningHost"/> which will be copied to this repository.</param>
        /// <definition>
        /// public ListeningHostRepository(IEnumerable{{ListeningHost}} hosts)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningHostRepository(IEnumerable<ListeningHost> hosts)
        {
            _hosts.AddRange(hosts);
        }

        /// <summary>
        /// Gets the number of elements contained in this <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <definition>
        /// public int Count { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int Count => _hosts.Count;

        /// <summary>
        /// Gets an boolean indicating if this <see cref="ListeningHostRepository"/> is read only. This property always returns <c>true</c>.
        /// </summary>
        /// <definition>
        /// public bool IsReadOnly { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a listeninghost to this repository. If this listeninghost already exists in this class, an exception will be thrown.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to add to this collection.</param>
        /// <definition>
        /// public bool IsReadOnly { get; }
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Add(ListeningHost item)
        {
            if (this.Contains(item)) throw new ArgumentOutOfRangeException("This ListeningHost has already been defined in this collection with identical definitions.");
            _hosts.Add(item);
        }

        /// <summary>
        /// Removes all listeninghosts from this repository.
        /// </summary>
        /// <definition>
        /// public void Clear()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Clear()
        {
            _hosts.Clear();
        }

        /// <summary>
        /// Determines if an <see cref="ListeningHost"/> is present in this repository.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to check if is present in this repository.</param>
        /// <returns></returns>
        /// <definition>
        /// public bool Contains(ListeningHost item)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public bool Contains(ListeningHost item)
        {
            return _hosts.Contains(item);
        }

        /// <summary>
        /// Copies all elements from this repository to another compatible repository.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <definition>
        /// public void CopyTo(ListeningHost[] array, int arrayIndex)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void CopyTo(ListeningHost[] array, int arrayIndex)
        {
            _hosts.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public IEnumerator&lt;ListeningHost&gt; GetEnumerator()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public IEnumerator<ListeningHost> GetEnumerator()
        {
            return _hosts.GetEnumerator();
        }

        /// <summary>
        /// Try to remove a <see cref="ListeningHost"/> from this repository. If the item is removed, this methods returns <c>true</c>.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to be removed.</param>
        /// <returns></returns>
        /// <definition>
        /// public bool Remove(ListeningHost item)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public bool Remove(ListeningHost item)
        {
            return _hosts.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// IEnumerator IEnumerable.GetEnumerator()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets a listening host through its index.
        /// </summary>
        /// <param name="index">The Listening Host index</param>
        /// <returns></returns>
        /// <definition>
        /// public ListeningHost this[int index]
        /// </definition>
        /// <type>
        /// Indexer
        /// </type>
        public ListeningHost this[int index] { get => _hosts[index]; }

        internal ListeningHost? GetRequestMatchingListeningHost(string dnsSafeHost, int port)
        {
            foreach (ListeningHost h in this._hosts)
            {
                foreach (ListeningPort p in h.Ports)
                {
                    if (p.Port == port && HttpStringInternals.IsDnsMatch(p.Hostname, dnsSafeHost))
                    {
                        return h;
                    }
                }
            }
            return null;
        }
    }
}
