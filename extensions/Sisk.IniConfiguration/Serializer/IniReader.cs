// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IniReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.IniConfiguration.Serializer;

/// <summary>
/// Provides an INI-document reader and parser.
/// </summary>
public sealed class IniReader : IDisposable {
    internal static readonly StringComparer IniNamingComparer = StringComparer.InvariantCultureIgnoreCase;

    internal const string INITIAL_SECTION_NAME = "__SISKINIGLOBAL__";

    private readonly TextReader reader;
    private bool disposedValue;

    /// <summary>
    /// Gets the <see cref="TextReader"/> which is providing data to this INI reader.
    /// </summary>
    public TextReader Reader { get => this.reader; }

    /// <summary>
    /// Creates an new <see cref="IniReader"/> with the specified text reader.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> instace to read the INI document.</param>
    public IniReader ( TextReader reader ) {
        this.reader = reader;
    }

    /// <summary>
    /// Reads the INI document from the input stream.
    /// </summary>
    /// <returns>An <see cref="IniDocument"/> file containing all properties and data from the input stream.</returns>
    public IniDocument Read () {
        this.ThrowIfDisposed ();

        string lastSectionName = INITIAL_SECTION_NAME;
        List<(string, string)> items = new List<(string, string)> ();
        List<IniSection> creatingSections = new List<IniSection> ();

        int read = 0;
        while ((read = this.reader.Peek ()) >= 0) {
            char c = (char) read;

            if (c is Token.SECTION_START) {
                this.reader.Read ();
                string? sectionName = this.ReadUntil ( new char [] { Token.SECTION_END } )?.Trim ();

                if (sectionName is null)
                    break;

                var closingSection = new IniSection ( lastSectionName, items.ToArray () );
                creatingSections.Add ( closingSection );

                items.Clear ();
                lastSectionName = sectionName;

                this.SkipUntilNewLine ();
            }
            else if (c is Token.COMMENT_1 or Token.COMMENT_2) {
                this.SkipUntilNewLine ();
            }
            else {
                string? propertyName = this.ReadUntil ( new char [] { Token.PROPERTY_DELIMITER, Token.NEW_LINE, Token.RETURN }, true );
                if (propertyName is null)
                    break;

                string? propertyValue = this.ReadValue ();
                if (propertyValue is null)
                    break;

                if ((string.IsNullOrWhiteSpace ( propertyName ) && string.IsNullOrWhiteSpace ( propertyValue )) == false)
                    items.Add ( (propertyName.Trim (), propertyValue) );

                this.SkipWhiteSpace ();
            }
        }

        if (items.Count > 0) {
            var closingSection = new IniSection ( lastSectionName, items.ToArray () );
            creatingSections.Add ( closingSection );
        }

        return new IniDocument ( creatingSections.ToArray () );
    }

    void SkipUntilNewLine () {
        this.ReadUntil ( new char [] { Token.NEW_LINE }, false );
    }

    void SkipWhiteSpace () {
        int read = 0;
        while ((read = this.reader.Peek ()) >= 0) {
            char c = (char) read;
            if (!char.IsWhiteSpace ( c )) {
                break;
            }
            else
                this.reader.Read ();
        }
    }

    string? ReadValue () {
readNext:
        int read = this.reader.Read ();
        if (read < 0) {
            return string.Empty;
        }
        else {
            char c = (char) read;

            if (c is Token.SPACE or Token.TAB) {
                goto readNext;
            }
            else if (c is Token.RETURN or Token.NEW_LINE) {
                return string.Empty;
            }
            if (c == Token.STRING_QUOTE_1) {
                return this.ReadUntil ( new char [] { Token.STRING_QUOTE_1 }, false );
            }
            else if (c == Token.STRING_QUOTE_2) {
                return this.ReadUntil ( new char [] { Token.STRING_QUOTE_2 }, false );
            }
            else {
                return (c + this.ReadUntil ( new char [] { Token.NEW_LINE }, true, true )).Trim ();
            }
        }
    }

    string? ReadUntil ( in char [] until, bool canExplode = false, bool breakOnComment = false ) {
        StringBuilder sb = new StringBuilder ();
        int read = 0;
        while ((read = this.reader.Read ()) >= 0) {
            char c = (char) read;

            if (until.Contains ( c )) {
                return sb.ToString ();
            }
            else if (breakOnComment && (c is Token.COMMENT_1 or Token.COMMENT_2)) {
                var s = sb.ToString ();
                this.SkipUntilNewLine ();
                return s;
            }
            else {
                sb.Append ( c );
            }
        }

        if (canExplode) {
            return sb.ToString ();
        }
        else {
            return null;
        }
    }

    void ThrowIfDisposed () {
        if (this.disposedValue)
            throw new ObjectDisposedException ( nameof ( IniReader ) );
    }

    void Dispose ( bool disposing ) {
        if (!this.disposedValue) {
            if (disposing) {
                this.reader.Dispose ();
            }

            this.disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose () {
        this.Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}
