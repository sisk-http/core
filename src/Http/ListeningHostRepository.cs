﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ListeningHostRepository.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using System.Runtime.InteropServices;
using Sisk.Core.Internal;

namespace Sisk.Core.Http;

/// <summary>
/// Represents an fluent repository of <see cref="ListeningHost"/> that can add, modify, or remove listening hosts while an <see cref="HttpServer"/> is running.
/// </summary>
public sealed class ListeningHostRepository : IList<ListeningHost> {
    private readonly List<ListeningHost> _hosts = new List<ListeningHost> ();

    /// <summary>
    /// Creates a new instance of an empty <see cref="ListeningHostRepository"/>.
    /// </summary>
    public ListeningHostRepository () {
    }


    /// <summary>
    /// Creates a new instance of an <see cref="ListeningHostRepository"/> copying the items from another collection of <see cref="ListeningHost"/>.
    /// </summary>
    /// <param name="hosts">The collection which stores the <see cref="ListeningHost"/> which will be copied to this repository.</param>
    public ListeningHostRepository ( IEnumerable<ListeningHost> hosts ) {
        _hosts.AddRange ( hosts );
    }

    /// <summary>
    /// Gets the number of elements contained in this <see cref="ListeningHostRepository"/>.
    /// </summary>
    public int Count => _hosts.Count;

    /// <summary>
    /// Gets an boolean indicating if this <see cref="ListeningHostRepository"/> is read only. This property always returns <see langword="false"></see>.
    /// </summary>
    public bool IsReadOnly => false;

    /// <summary>
    /// Returns the first <see cref="ListeningHost"/> in the repository, or creates and adds a new one if the repository is empty.
    /// </summary>
    /// <returns>
    /// The first <see cref="ListeningHost"/> in the repository, or a new instance if none exist.
    /// </returns>
    public ListeningHost FirstOrNew () {
        if (_hosts.Count == 0) {
            ListeningHost host = new ListeningHost ();
            _hosts.Add ( host );
            return host;
        }
        else {
            return _hosts [ 0 ];
        }
    }

    /// <summary>
    /// Adds a listeninghost to this repository. If this listeninghost already exists in this class, an exception will be thrown.
    /// </summary>
    /// <param name="item">The <see cref="ListeningHost"/> to add to this collection.</param>
    public void Add ( ListeningHost item ) {
        if (Contains ( item )) {
            throw new ArgumentException ( SR.ListeningHostRepository_Duplicate, nameof ( item ) );
        }

        _hosts.Add ( item );
    }

    /// <summary>
    /// Removes all listeninghosts from this repository.
    /// </summary>
    public void Clear () {
        _hosts.Clear ();
    }

    /// <summary>
    /// Determines if an <see cref="ListeningHost"/> is present in this repository.
    /// </summary>
    /// <param name="item">The <see cref="ListeningHost"/> to check if is present in this repository.</param>
    public bool Contains ( ListeningHost item ) {
        return _hosts.Contains ( item );
    }

    /// <summary>
    /// Copies all elements from this repository to another compatible repository.
    /// </summary>
    /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied.</param>
    /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
    public void CopyTo ( ListeningHost [] array, int arrayIndex ) {
        _hosts.CopyTo ( array, arrayIndex );
    }

    /// <summary>
    /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
    /// </summary>
    public IEnumerator<ListeningHost> GetEnumerator () {
        return _hosts.GetEnumerator ();
    }

    /// <summary>
    /// Try to remove a <see cref="ListeningHost"/> from this repository. If the item is removed, this methods returns <see langword="true"></see>.
    /// </summary>
    /// <param name="item">The <see cref="ListeningHost"/> to be removed.</param>
    public bool Remove ( ListeningHost item ) {
        return _hosts.Remove ( item );
    }

    /// <summary>
    /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
    /// </summary>
    IEnumerator IEnumerable.GetEnumerator () {
        return GetEnumerator ();
    }

    /// <inheritdoc/>
    public ListeningHost this [ int index ] { get => _hosts [ index ]; set => _hosts [ index ] = value; }

    internal ListeningHost? GetRequestMatchingListeningHost ( string incomingHost, string path, int incomingPort ) {
        lock (_hosts) {
            Span<ListeningHost> hosts = CollectionsMarshal.AsSpan ( _hosts );
            for (int H = 0; H < hosts.Length; H++) {
                ListeningHost h = hosts [ H ];

                Span<ListeningPort> ports = CollectionsMarshal.AsSpan ( h._ports );
                for (int P = 0; P < ports.Length; P++) {
                    ref ListeningPort p = ref ports [ P ];

                    if (p.Port == incomingPort
                        && HttpStringInternals.IsDnsMatch ( p.Hostname, incomingHost )
                        && path.StartsWith ( p.Path, StringComparison.OrdinalIgnoreCase )) {
                        return h;
                    }
                }
            }
        }
        return null;
    }

    /// <inheritdoc/>
    public int IndexOf ( ListeningHost item ) {
        return _hosts.IndexOf ( item );
    }

    /// <inheritdoc/>
    public void Insert ( int index, ListeningHost item ) {
        _hosts.Insert ( index, item );
    }

    /// <inheritdoc/>
    public void RemoveAt ( int index ) {
        _hosts.RemoveAt ( index );
    }
}
