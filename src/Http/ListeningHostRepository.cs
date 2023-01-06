using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

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
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHostRepository()
        {
        }


        /// <summary>
        /// Creates a new instance of an <see cref="ListeningHostRepository"/> copying the items from another collection of <see cref="ListeningHost"/>.
        /// </summary>
        /// <param name="hosts">The collection which stores the <see cref="ListeningHost"/> which will be copied to this repository.</param>
        /// <definition>
        /// public ListeningHostRepository(IEnumerable&lt;ListeningHost&gt; hosts)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Contains(ListeningHost item)
        {
            foreach (ListeningHost h in this._hosts)
            {
                if (CompareListeningHost(item, h))
                    return true;
            }
            return false;
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Remove(ListeningHost item)
        {
            for (int i = 0; i < _hosts.Count; i++)
            {
                ListeningHost h = _hosts[i];
                if (CompareListeningHost(h, item))
                {
                    _hosts.RemoveAt(i);
                    return true;
                }
            }
            return false;
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
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        internal ListeningHost? GetRequestMatchingListeningHost(string dnsSafeHost, int port)
        {
            foreach (ListeningHost h in this._hosts)
            {
                if (isWildcardMatch(h.Hostname, dnsSafeHost) && h._numericPorts.Contains(port))
                {
                    return h;
                }
            }
            return null;
        }

        private bool CompareListeningHost(ListeningHost a, ListeningHost b)
        {
            return a.Handle == b.Handle;
        }

        private bool isWildcardMatch(string wildcardPattern, string subject)
        {
            if (string.IsNullOrWhiteSpace(wildcardPattern))
            {
                return false;
            }

            int wildcardCount = wildcardPattern.Count(x => x.Equals('*'));
            if (wildcardCount <= 0)
            {
                return subject.Equals(wildcardPattern, StringComparison.CurrentCultureIgnoreCase);
            }
            else if (wildcardCount == 1)
            {
                string newWildcardPattern = wildcardPattern.Replace("*", "");

                if (wildcardPattern.StartsWith("*"))
                {
                    return subject.EndsWith(newWildcardPattern, StringComparison.CurrentCultureIgnoreCase);
                }
                else if (wildcardPattern.EndsWith("*"))
                {
                    return subject.StartsWith(newWildcardPattern, StringComparison.CurrentCultureIgnoreCase);
                }
                else
                {
                    return isWildcardMatchRgx(wildcardPattern, subject);
                }
            }
            else
            {
                return isWildcardMatchRgx(wildcardPattern, subject);
            }
        }

        private bool isWildcardMatchRgx(string pattern, string subject)
        {
            string[] parts = pattern.Split('*');
            if (parts.Length <= 1)
            {
                return subject.Equals(pattern, StringComparison.CurrentCultureIgnoreCase);
            }

            int pos = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                if (i <= 0)
                {
                    // first
                    pos = subject.IndexOf(parts[i], pos, StringComparison.CurrentCultureIgnoreCase);
                    if (pos != 0)
                    {
                        return false;
                    }
                }
                else if (i >= (parts.Length - 1))
                {
                    // last
                    if (!subject.EndsWith(parts[i], StringComparison.CurrentCultureIgnoreCase))
                    {
                        return false;
                    }
                }
                else
                {
                    pos = subject.IndexOf(parts[i], pos, StringComparison.CurrentCultureIgnoreCase);
                    if (pos < 0)
                    {
                        return false;
                    }

                    pos += parts[i].Length;
                }
            }

            return true;
        }
    }
}
