// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IniReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.IniConfiguration.Core.Serialization;

/// <summary>
/// Provides an INI-document reader and parser.
/// </summary>
public sealed class IniReader : IDisposable {

    /// <summary>
    /// Gets or sets the default <see cref="StringComparer"/> used by the INI reader and instances
    /// to compare key names.
    /// </summary>
    public static StringComparer IniNamingComparer { get; set; } = StringComparer.InvariantCultureIgnoreCase;

    internal const string INITIAL_SECTION_NAME = "__SISKINIGLOBAL__";

    private readonly TextReader reader;
    private bool disposedValue;

    /// <summary>
    /// Gets the <see cref="TextReader"/> which is providing data to this INI reader.
    /// </summary>
    public TextReader Reader { get => reader; }

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
        ThrowIfDisposed ();

        string lastSectionName = INITIAL_SECTION_NAME;
        List<KeyValuePair<string, string>> items = new ();
        List<IniSection> creatingSections = new ();

        string? line;
        while ((line = reader.ReadLine ()) != null) {

            if (string.IsNullOrWhiteSpace ( line ))
                continue;

            string lineTrimmed = line.TrimStart ();
            if (lineTrimmed.Length == 0)
                continue;

            char assertingChar = lineTrimmed [ 0 ];

            // assert comments
            if (assertingChar == Token.COMMENT_1 || assertingChar == Token.COMMENT_2) {
                continue;
            }

            // assert section start
            else if (assertingChar == Token.SECTION_START) {
                string? sectionName = ParseSectionName ( lineTrimmed );

                if (sectionName is null)
                    continue;

                // Commit the previous section before starting a new one
                if (items.Count > 0) {
                    var commitingSection = new IniSection ( lastSectionName, items );
                    creatingSections.Add ( commitingSection );
                    items = new List<KeyValuePair<string, string>> ();
                }

                lastSectionName = sectionName;
            }

            // assert property
            else {
                var property = ParsePropertyName ( lineTrimmed );
                items.Add ( new KeyValuePair<string, string> ( property.propertyName ?? string.Empty, property.propertyValue ?? string.Empty ) );
            }
        }

        if (items.Count > 0) {
            var commitingSection = new IniSection ( lastSectionName, items );
            creatingSections.Add ( commitingSection );
        }

        return new IniDocument ( creatingSections );
    }

    (string? propertyName, string? propertyValue) ParsePropertyName ( in ReadOnlySpan<char> line ) {

        int propertySeparator = line.IndexOfAny ( [ Token.PROPERTY_DELIMITER, Token.NEW_LINE, Token.RETURN ] );
        if (propertySeparator < 0) {

            return (new string ( line ).Trim (), null);
        }

        if (line [ propertySeparator ] == Token.PROPERTY_DELIMITER) {

            string propertyName = new string ( line [ 0..propertySeparator ] ).Trim ();
            string valueInitial = new string ( line [ (propertySeparator + 1).. ] ).Trim ();

            if (string.IsNullOrEmpty ( valueInitial )) {
                return (propertyName, null);
            }

            if (valueInitial [ 0 ] == Token.STRING_QUOTE_1) {
                if (valueInitial.Length > 1 && valueInitial.EndsWith ( Token.STRING_QUOTE_1 )) {
                    return (propertyName, valueInitial [ 1..^1 ]);
                }
                else {
                    string valueParsed = ParseValue ( valueInitial, Token.STRING_QUOTE_1 );
                    return (propertyName, valueParsed);
                }
            }
            else if (valueInitial [ 0 ] == Token.STRING_QUOTE_2) {
                if (valueInitial.Length > 1 && valueInitial.EndsWith ( Token.STRING_QUOTE_2 )) {
                    return (propertyName, valueInitial [ 1..^1 ]);
                }
                else {
                    string valueParsed = ParseValue ( valueInitial, Token.STRING_QUOTE_2 );
                    return (propertyName, valueParsed);
                }
            }
            else if (valueInitial.StartsWith ( "<<<" )) {
                string delimiter = valueInitial [ 3.. ].Trim ();
                if (!string.IsNullOrEmpty ( delimiter )) {
                    return (propertyName, ParseHeredocValue ( delimiter ));
                }

                return (propertyName, valueInitial);
            }
            else {
                if (valueInitial.AsSpan ().IndexOfAny ( [ Token.COMMENT_1, Token.COMMENT_2 ] ) is int commentIndex and >= 0) {
                    valueInitial = valueInitial [ 0..commentIndex ].TrimEnd ();
                }

                return (propertyName, valueInitial);
            }
        }
        else {
            return (new string ( line ).Trim (), null);
        }
    }

    string ParseHeredocValue ( string delimiter ) {
        StringBuilder sb = new StringBuilder ();
        string? line;
        while ((line = reader.ReadLine ()) != null) {
            if (line.Trim ().Equals ( delimiter, StringComparison.Ordinal )) {
                break;
            }
            sb.AppendLine ( line );
        }
        if (sb.Length > 0) {
            int len = sb.Length;
            if (len > 0 && sb [ len - 1 ] == '\n') {
                len--;
                if (len > 0 && sb [ len - 1 ] == '\r') {
                    len--;
                }
                sb.Length = len;
            }
        }
        return sb.ToString ();
    }


    string ParseValue ( string initialValue, char eof ) {

        if (initialValue.StartsWith ( eof ) &&
            initialValue.IndexOf ( eof, 1 ) is { } lastIndex and >= 0) {

            return initialValue [ 1..lastIndex ];
        }

        StringBuilder sb = new StringBuilder ();
        sb.AppendLine ( initialValue );

        string? line;
        while ((line = reader.ReadLine ()) != null) {

            if (line.IndexOf ( eof ) is { } endOfLine and >= 0) {
                sb.AppendLine ( line [ ..endOfLine ] );
                break;
            }
            else {
                sb.AppendLine ( line );
            }
        }

        string result = sb.ToString ().Trim ();
        if (result.StartsWith ( eof )) {
            result = result [ 1.. ];
        }
        if (result.EndsWith ( eof )) {
            result = result [ ..^1 ];
        }

        return result;
    }

    string? ParseSectionName ( in ReadOnlySpan<char> line ) {
        int sectionStartPosition = line.IndexOf ( Token.SECTION_START );
        int sectionEndPosition = line.IndexOf ( Token.SECTION_END );

        if (sectionEndPosition < 0 || sectionStartPosition < 0) {
            return null;
        }

        ReadOnlySpan<char> part = line [ (sectionStartPosition + 1)..sectionEndPosition ];
        return new string ( part ).Trim ();
    }

    void ThrowIfDisposed () {
        if (disposedValue)
            throw new ObjectDisposedException ( nameof ( IniReader ) );
    }

    void Dispose ( bool disposing ) {
        if (!disposedValue) {
            if (disposing) {
                reader.Dispose ();
            }

            disposedValue = true;
        }
    }

    /// <inheritdoc/>
    public void Dispose () {
        Dispose ( disposing: true );
        GC.SuppressFinalize ( this );
    }
}