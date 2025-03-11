// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IniSectionCollection.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections;
using Sisk.IniConfiguration.Core.Serialization;

namespace Sisk.IniConfiguration.Core;

/// <summary>
/// Represents an collection of <see cref="IniSection"/>.
/// </summary>
public sealed class IniSectionCollection : IList<IniSection> {
    private List<IniSection> inner;

    internal IniSectionCollection () {
        inner = new List<IniSection> ();
    }

    internal IniSectionCollection ( IEnumerable<IniSection> p ) {
        inner = new List<IniSection> ( p );
    }

    /// <summary>
    /// Gets the global <see cref="IniSection"/> in this collection or creates a new one if it
    /// doens't exists.
    /// </summary>
    /// <returns>The global <see cref="IniSection"/>.</returns>
    public IniSection GetGlobal () {

        IniSection global;
        if (inner.Count == 0) {
            global = new IniSection ( IniReader.INITIAL_SECTION_NAME );
            inner.Add ( global );
        }
        else {
            if (inner [ 0 ].Name == IniReader.INITIAL_SECTION_NAME) {
                global = inner [ 0 ];
            }
            else {
                global = new IniSection ( IniReader.INITIAL_SECTION_NAME );
                inner.Insert ( 0, global );
            }
        }

        return global;
    }

    /// <inheritdoc/>
    public IniSection this [ int index ] {
        get => ((IList<IniSection>) inner) [ index ];
        set {
            ((IList<IniSection>) inner) [ index ] = value;
            MergeIniSections ();
        }
    }

    /// <inheritdoc/>
    public int Count => ((ICollection<IniSection>) inner).Count;

    /// <inheritdoc/>
    public bool IsReadOnly => ((ICollection<IniSection>) inner).IsReadOnly;

    /// <inheritdoc/>
    public void Add ( IniSection item ) {
        ((ICollection<IniSection>) inner).Add ( item );
        MergeIniSections ();
    }

    /// <inheritdoc/>
    public void Clear () {
        ((ICollection<IniSection>) inner).Clear ();
    }

    /// <inheritdoc/>
    public bool Contains ( IniSection item ) {
        return ((ICollection<IniSection>) inner).Contains ( item );
    }

    /// <inheritdoc/>
    public void CopyTo ( IniSection [] array, int arrayIndex ) {
        ((ICollection<IniSection>) inner).CopyTo ( array, arrayIndex );
    }

    /// <inheritdoc/>
    public IEnumerator<IniSection> GetEnumerator () {
        return ((IEnumerable<IniSection>) inner).GetEnumerator ();
    }

    /// <inheritdoc/>
    public int IndexOf ( IniSection item ) {
        return ((IList<IniSection>) inner).IndexOf ( item );
    }

    /// <inheritdoc/>
    public void Insert ( int index, IniSection item ) {
        ((IList<IniSection>) inner).Insert ( index, item );
        MergeIniSections ();
    }

    /// <inheritdoc/>
    public bool Remove ( IniSection item ) {
        bool result = ((ICollection<IniSection>) inner).Remove ( item );
        MergeIniSections ();
        return result;
    }

    /// <inheritdoc/>
    public void RemoveAt ( int index ) {
        ((IList<IniSection>) inner).RemoveAt ( index );
        MergeIniSections ();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator () {
        return ((IEnumerable) inner).GetEnumerator ();
    }

    IList<IniSection> MergeIniSections () {
        lock (inner) {
            var sectionNames = inner
            .DistinctBy ( s => s.Name, IniReader.IniNamingComparer )
            .Select ( s => s.Name )
            .ToArray ();

            List<IniSection> result = new List<IniSection> ( sectionNames.Length );
            for (int i = 0; i < sectionNames.Length; i++) {
                string currentName = sectionNames [ i ];
                List<KeyValuePair<string, string>> allProperties = new ();

                for (int j = 0; j < inner.Count; j++) {
                    IniSection s = inner [ j ];
                    if (IniReader.IniNamingComparer.Compare ( s.Name, currentName ) == 0) {
                        allProperties.AddRange ( s.items );
                    }
                }

                result.Add ( new IniSection ( currentName, allProperties.ToArray () ) );
            }

            return result.ToList ();
        }
    }
}
