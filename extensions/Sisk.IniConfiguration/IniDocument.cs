// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IniDocument.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.IniConfiguration.Serializer;
using System.Text;

namespace Sisk.IniConfiguration;

/// <summary>
/// Represents an INI configuration document.
/// </summary>
public sealed class IniDocument
{
    /// <summary>
    /// Gets all INI sections defined in this INI document.
    /// </summary>
    public IList<IniSection> Sections { get; }

    /// <summary>
    /// Gets the global INI section, which is the primary section in the document.
    /// </summary>
    public IniSection Global { get => this.Sections[0]; }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// string, reading it as an UTF-8 string.
    /// </summary>
    /// <param name="iniConfiguration">The UTF-8 string.</param>
    public static IniDocument FromString(string iniConfiguration)
    {
        using TextReader reader = new StringReader(iniConfiguration);
        using IniReader parser = new IniReader(reader);
        return parser.Read();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// file using the specified encoding.
    /// </summary>
    /// <param name="filePath">The absolute or relative file path to the INI document.</param>
    /// <param name="encoding">Optional. The encoding used to read the file. Defaults to UTF-8.</param>
    public static IniDocument FromFile(string filePath, Encoding? encoding = null)
    {
        using TextReader reader = new StreamReader(filePath, encoding ?? Encoding.UTF8);
        using IniReader parser = new IniReader(reader);
        return parser.Read();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// stream using the specified encoding.
    /// </summary>
    /// <param name="stream">The input stream where the INI document is.</param>
    /// <param name="encoding">Optional. The encoding used to read the stream. Defaults to UTF-8.</param>
    public static IniDocument FromStream(Stream stream, Encoding? encoding = null)
    {
        using TextReader reader = new StreamReader(stream, encoding ?? Encoding.UTF8);
        using IniReader parser = new IniReader(reader);
        return parser.Read();
    }

    /// <summary>
    /// Creates an new <see cref="IniDocument"/> document from the specified
    /// <see cref="TextReader"/>.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> instance.</param>
    public static IniDocument FromStream(TextReader reader)
    {
        using IniReader parser = new IniReader(reader);
        return parser.Read();
    }

    internal IniDocument(IniSection[] sections)
    {
        this.Sections = IniSection.MergeIniSections(sections);
    }

    /// <summary>
    /// Gets an defined INI section from this document. The search is case-insensitive.
    /// </summary>
    /// <param name="sectionName">The section name.</param>
    /// <returns>The <see cref="IniSection"/> object if found, or null if not defined.</returns>
    public IniSection? GetSection(string sectionName)
    {
        for (int i = 0; i < this.Sections.Count; i++)
        {
            IniSection section = this.Sections[i];
            if (IniReader.IniNamingComparer.Compare(section.Name, sectionName) == 0)
                return section;
        }
        return null;
    }
}
