// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IniDocument.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using Sisk.IniConfiguration.Core.Serialization;

namespace Sisk.IniConfiguration.Core;

/// <summary>
/// Represents an INI document.
/// </summary>
public sealed class IniDocument : IEquatable<IniDocument> {

    /// <summary>
    /// Gets all INI sections defined in this INI document.
    /// </summary>
    public IniSectionCollection Sections { get; }

    /// <summary>
    /// Gets the global INI section, which is the primary section in the document.
    /// </summary>
    public IniSection Global {
        get => Sections.GetGlobal ();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// string, reading it as an UTF-8 string.
    /// </summary>
    /// <param name="iniConfiguration">The UTF-8 string.</param>
    public static IniDocument FromString ( string iniConfiguration ) {
        using TextReader reader = new StringReader ( iniConfiguration );
        using IniReader parser = new IniReader ( reader );
        return parser.Read ();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// file using the specified encoding.
    /// </summary>
    /// <param name="filePath">The absolute or relative file path to the INI document.</param>
    /// <param name="encoding">Optional. The encoding used to read the file. Defaults to UTF-8.</param>
    /// <param name="throwIfNotExists">Optional. Defines whether this method should throw if the specified file doens't exists or return an empty INI document.</param>
    public static IniDocument FromFile ( string filePath, Encoding? encoding = null, bool throwIfNotExists = true ) {
        if (!throwIfNotExists && !File.Exists ( filePath ))
            return new IniDocument ();

        using TextReader reader = new StreamReader ( filePath, encoding ?? Encoding.Default );
        using IniReader parser = new IniReader ( reader );
        return parser.Read ();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// stream using the specified encoding.
    /// </summary>
    /// <param name="stream">The input stream where the INI document is.</param>
    /// <param name="encoding">Optional. The encoding used to read the stream. Defaults to UTF-8.</param>
    public static IniDocument FromStream ( Stream stream, Encoding? encoding = null ) {
        using TextReader reader = new StreamReader ( stream, encoding ?? Encoding.Default );
        using IniReader parser = new IniReader ( reader );
        return parser.Read ();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> instance.</param>
    public static IniDocument FromStream ( TextReader reader ) {
        using IniReader parser = new IniReader ( reader );
        return parser.Read ();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> instance from the
    /// specified <see cref="IniSection"/> collection.
    /// </summary>
    /// <param name="sections">The list of <see cref="IniSection"/>.</param>
    public IniDocument ( IEnumerable<IniSection> sections ) {
        Sections = new IniSectionCollection ( sections );
    }

    /// <summary>
    /// Creates an new empty <see cref="IniDocument"/> instance with no
    /// INI sections added to it.
    /// </summary>
    public IniDocument () {
        Sections = new IniSectionCollection ();
    }

    /// <summary>
    /// Gets an defined INI section from this document. The search is case-insensitive.
    /// </summary>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The <see cref="IniSection"/> object if found, or null if not defined.</returns>
    public IniSection? GetSection ( string sectionName ) {
        for (int i = 0; i < Sections.Count; i++) {
            IniSection section = Sections [ i ];
            if (IniReader.IniNamingComparer.Compare ( section.Name, sectionName ) == 0)
                return section;
        }
        return null;
    }

    /// <summary>
    /// Retrieves all entries in the INI document.
    /// </summary>
    /// <returns>An enumerable collection of key-value pairs, where each key is the entry name and each value is an array of strings representing the entry values.</returns>
    public IEnumerable<KeyValuePair<string, string []>> GetEntries () {
        foreach (var section in Sections) {
            foreach (var entry in section) {

                string name;
                if (section.Name != IniReader.INITIAL_SECTION_NAME) {
                    name = $"{section.Name}.{entry.Key}";
                }
                else {
                    name = entry.Key;
                }

                yield return new KeyValuePair<string, string []> ( name, entry.Value );
            }
        }
    }

    /// <summary>
    /// Retrieves the values of a specific entry in the INI document.
    /// </summary>
    /// <param name="name">The name of the entry to retrieve.</param>
    /// <param name="stringComparison">The string comparison to use when searching for the entry. Defaults to <see cref="StringComparison.OrdinalIgnoreCase"/>.</param>
    /// <returns>An array of strings representing the values of the entry, or an empty array if the entry is not found.</returns>
    public string [] GetEntry ( string name, StringComparison stringComparison = StringComparison.OrdinalIgnoreCase ) {
        foreach (var entry in GetEntries ()) {
            if (entry.Key.Equals ( name, stringComparison )) {
                return entry.Value;
            }
        }
        return Array.Empty<string> ();
    }

    /// <summary>
    /// Saves the INI document to a file.
    /// </summary>
    /// <param name="filePath">The path to the file where the INI document will be saved.</param>
    /// <param name="encoding">The encoding to use when writing the file. Defaults to <see cref="Encoding.Default"/>.</param>
    public void SaveTo ( string filePath, Encoding? encoding = default ) {
        using var fs = File.OpenWrite ( filePath );
        using var sw = new StreamWriter ( fs, encoding ?? Encoding.Default );
        using var iw = new IniWriter ( sw );

        iw.Write ( this );
        sw.Flush ();
    }

    /// <summary>
    /// Saves the INI document to a stream.
    /// </summary>
    /// <param name="stream">The stream to which the INI document will be written.</param>
    /// <param name="encoding">The encoding to use when writing to the stream. Defaults to <see cref="Encoding.Default"/>.</param>
    public void SaveTo ( Stream stream, Encoding? encoding = default ) {
        using var sw = new StreamWriter ( stream, encoding ?? Encoding.Default );
        using var iw = new IniWriter ( sw );

        iw.Write ( this );
        sw.Flush ();
    }

    /// <summary>
    /// Gets the INI document string from this <see cref="IniDocument"/>.
    /// </summary>
    public override string ToString () {
        using (var sw = new StringWriter ())
        using (var iw = new IniWriter ( sw )) {
            iw.Write ( this );
            return sw.ToString ();
        }
    }

    /// <inheritdoc/>
    public bool Equals ( IniDocument? other ) {

        if (other is null)
            return false;

        if (Sections.Count != other.Sections.Count)
            return false;

        for (int i = 0; i < Sections.Count; i++) {
            var a = Sections [ i ];
            var b = other.Sections [ i ];

            if (!a.Equals ( b ))
                return false;
        }

        return true;
    }

    /// <inheritdoc/>
    public override int GetHashCode () {
        var hcode = new HashCode ();

        foreach (var section in Sections) {
            hcode.Add ( section );
        }

        return hcode.ToHashCode ();
    }

    /// <inheritdoc/>
    public override bool Equals ( object? obj ) {
        return obj is IniDocument doc && Equals ( doc );
    }
}
