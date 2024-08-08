using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.IniConfiguration.Parser;

/// <summary>
/// Provides an INI-document parser.
/// </summary>
public sealed class IniParser : IDisposable
{
    internal static readonly StringComparer IniNamingComparer = StringComparer.InvariantCultureIgnoreCase;

    private TextReader reader;
    private bool disposedValue;

    /// <summary>
    /// Creates an new <see cref="IniParser"/> with the specified text reader.
    /// </summary>
    /// <param name="reader">The <see cref="TextReader"/> instace to read the INI document.</param>
    public IniParser(TextReader reader)
    {
        this.reader = reader;
    }

    const char SECTION_START = '[';
    const char SECTION_END = ']';
    const char COMMENT_1 = '#';
    const char COMMENT_2 = ';';
    const char STRING_QUOTE_1 = '\'';
    const char STRING_QUOTE_2 = '\"';
    const char PROPERTY_DELIMITER = '=';
    const char NEW_LINE = '\n';

    /// <summary>
    /// Reads the INI document from the input stream.
    /// </summary>
    /// <returns>An <see cref="IniDocument"/> file containing all properties and data from the input stream.</returns>
    public IniDocument Parse()
    {
        ThrowIfDisposed();

        string lastSectionName = "__GLOBAL__";
        List<(string, string)> items = new List<(string, string)>();
        List<IniSection> creatingSections = new List<IniSection>();

        int read = 0;
        while ((read = reader.Peek()) >= 0)
        {
            char c = (char)read;

            if (c == SECTION_START)
            {
                reader.Read();
                string? sectionName = ReadUntil(new char[] { SECTION_END })?.Trim();

                if (sectionName is null)
                    break;

                var closingSection = new IniSection(lastSectionName, items.ToArray());
                creatingSections.Add(closingSection);

                items.Clear();
                lastSectionName = sectionName;

                SkipWhiteSpace();
            }
            else if (c == COMMENT_1 || c == COMMENT_2)
            {
                SkipUntilNewLine();
            }
            else
            {
                string? propertyName = ReadUntil(new char[] { PROPERTY_DELIMITER }, true);
                if (propertyName is null)
                    break;

                string? propertyValue = ReadValue();
                if (propertyValue is null)
                    break;

                if ((string.IsNullOrWhiteSpace(propertyName) && string.IsNullOrWhiteSpace(propertyValue)) == false)
                    items.Add((propertyName.Trim(), propertyValue));

                SkipWhiteSpace();
            }
        }

        if (items.Count > 0)
        {
            var closingSection = new IniSection(lastSectionName, items.ToArray());
            creatingSections.Add(closingSection);
        }

        return new IniDocument(creatingSections.ToArray());
    }

    void SkipUntilNewLine()
    {
        ReadUntil(new char[] { NEW_LINE }, false);
    }

    void SkipWhiteSpace()
    {
        int read = 0;
        while ((read = reader.Peek()) >= 0)
        {
            char c = (char)read;
            if (!char.IsWhiteSpace(c))
            {
                break;
            }
            else reader.Read();
        }
    }

    string? ReadValue()
    {
    readNext:
        int read = reader.Read();
        if (read < 0)
        {
            return "";
        }
        else
        {
            char c = (char)read;

            if (c == ' ' || c == '\t')
            {
                goto readNext;
            }
            else if (c == '\r' || c == '\n')
            {
                return "";
            }
            if (c == STRING_QUOTE_1)
            {
                return ReadUntil(new char[] { STRING_QUOTE_1 }, false);
            }
            else if (c == STRING_QUOTE_2)
            {
                return ReadUntil(new char[] { STRING_QUOTE_2 }, false);
            }
            else
            {
                return (c + ReadUntil(new char[] { NEW_LINE }, true, true)).Trim();
            }
        }
    }

    string? ReadUntil(in char[] until, bool canExplode = false, bool breakOnComment = false)
    {
        StringBuilder sb = new StringBuilder();
        int read = 0;
        while ((read = reader.Read()) >= 0)
        {
            char c = (char)read;

            if (until.Contains(c))
            {
                return sb.ToString();
            }
            else if (breakOnComment && (c == COMMENT_1 || c == COMMENT_2))
            {
                var s = sb.ToString();
                SkipUntilNewLine();
                return s;
            }
            else
            {
                sb.Append(c);
            }
        }

        if (canExplode)
        {
            return sb.ToString();
        }
        else
        {
            return null;
        }
    }

    void ThrowIfDisposed()
    {
        if (disposedValue)
            throw new ObjectDisposedException(nameof(IniParser));
    }

    void Dispose(bool disposing)
    {
        if (!disposedValue)
        {
            if (disposing)
            {
                reader.Dispose();
            }

            disposedValue = true;
        }
    }

    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
