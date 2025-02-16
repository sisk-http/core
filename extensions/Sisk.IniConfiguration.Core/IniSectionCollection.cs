using System.Collections;
using Sisk.IniConfiguration.Core.Serialization;

namespace Sisk.IniConfiguration.Core;

/// <summary>
/// Represents an collection of <see cref="IniSection"/>.
/// </summary>
public sealed class IniSectionCollection : IList<IniSection> {
    private List<IniSection> inner;

    internal IniSectionCollection () {
        this.inner = new List<IniSection> ();
    }

    internal IniSectionCollection ( IEnumerable<IniSection> p ) {
        this.inner = new List<IniSection> ( p );
    }

    /// <summary>
    /// Gets the global <see cref="IniSection"/> in this collection or creates a new one if it
    /// doens't exists.
    /// </summary>
    /// <returns>The global <see cref="IniSection"/>.</returns>
    public IniSection GetGlobal () {

        IniSection global;
        if (this.inner.Count == 0) {
            global = new IniSection ( IniReader.INITIAL_SECTION_NAME );
            this.inner.Add ( global );
        }
        else {
            if (this.inner [ 0 ].Name == IniReader.INITIAL_SECTION_NAME) {
                global = this.inner [ 0 ];
            }
            else {
                global = new IniSection ( IniReader.INITIAL_SECTION_NAME );
                this.inner.Insert ( 0, global );
            }
        }

        return global;
    }

    /// <inheritdoc/>
    public IniSection this [ int index ] {
        get => ((IList<IniSection>) this.inner) [ index ];
        set {
            ((IList<IniSection>) this.inner) [ index ] = value;
            this.MergeIniSections ();
        }
    }

    /// <inheritdoc/>
    public int Count => ((ICollection<IniSection>) this.inner).Count;

    /// <inheritdoc/>
    public bool IsReadOnly => ((ICollection<IniSection>) this.inner).IsReadOnly;

    /// <inheritdoc/>
    public void Add ( IniSection item ) {
        ((ICollection<IniSection>) this.inner).Add ( item );
        this.MergeIniSections ();
    }

    /// <inheritdoc/>
    public void Clear () {
        ((ICollection<IniSection>) this.inner).Clear ();
    }

    /// <inheritdoc/>
    public bool Contains ( IniSection item ) {
        return ((ICollection<IniSection>) this.inner).Contains ( item );
    }

    /// <inheritdoc/>
    public void CopyTo ( IniSection [] array, int arrayIndex ) {
        ((ICollection<IniSection>) this.inner).CopyTo ( array, arrayIndex );
    }

    /// <inheritdoc/>
    public IEnumerator<IniSection> GetEnumerator () {
        return ((IEnumerable<IniSection>) this.inner).GetEnumerator ();
    }

    /// <inheritdoc/>
    public int IndexOf ( IniSection item ) {
        return ((IList<IniSection>) this.inner).IndexOf ( item );
    }

    /// <inheritdoc/>
    public void Insert ( int index, IniSection item ) {
        ((IList<IniSection>) this.inner).Insert ( index, item );
        this.MergeIniSections ();
    }

    /// <inheritdoc/>
    public bool Remove ( IniSection item ) {
        bool result = ((ICollection<IniSection>) this.inner).Remove ( item );
        this.MergeIniSections ();
        return result;
    }

    /// <inheritdoc/>
    public void RemoveAt ( int index ) {
        ((IList<IniSection>) this.inner).RemoveAt ( index );
        this.MergeIniSections ();
    }

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator () {
        return ((IEnumerable) this.inner).GetEnumerator ();
    }

    IList<IniSection> MergeIniSections () {
        lock (this.inner) {
            var sectionNames = this.inner
            .DistinctBy ( s => s.Name, IniReader.IniNamingComparer )
            .Select ( s => s.Name )
            .ToArray ();

            List<IniSection> result = new List<IniSection> ( sectionNames.Length );
            for (int i = 0; i < sectionNames.Length; i++) {
                string currentName = sectionNames [ i ];
                List<KeyValuePair<string, string>> allProperties = new ();

                for (int j = 0; j < this.inner.Count; j++) {
                    IniSection s = this.inner [ j ];
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
