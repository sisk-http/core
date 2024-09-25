using System.Text;

namespace Sisk.IniConfiguration.Serializer;

/// <summary>
/// Provides an INI-document reader and parser.
/// </summary>
public sealed class IniReader : IDisposable
{
    internal static readonly StringComparer IniNamingComparer = StringComparer.InvariantCultureIgnoreCase;

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
    public IniReader(TextReader reader)
    {
        this.reader = reader;
    }

    /// <summary>
    /// Reads the INI document from the input stream.
    /// </summary>
    /// <returns>An <see cref="IniDocument"/> file containing all properties and data from the input stream.</returns>
    public IniDocument Read()
    {
        ThrowIfDisposed();

        string lastSectionName = "__SISKINIGLOBAL__";
        List<(string, string)> items = new List<(string, string)>();
        List<IniSection> creatingSections = new List<IniSection>();

        int read = 0;
        while ((read = reader.Peek()) >= 0)
        {
            char c = (char)read;

            if (c is Token.SECTION_START)
            {
                reader.Read();
                string? sectionName = ReadUntil(new char[] { Token.SECTION_END })?.Trim();

                if (sectionName is null)
                    break;

                var closingSection = new IniSection(lastSectionName, items.ToArray());
                creatingSections.Add(closingSection);

                items.Clear();
                lastSectionName = sectionName;

                SkipUntilNewLine();
            }
            else if (c is Token.COMMENT_1 or Token.COMMENT_2)
            {
                SkipUntilNewLine();
            }
            else
            {
                string? propertyName = ReadUntil(new char[] { Token.PROPERTY_DELIMITER, Token.NEW_LINE, Token.RETURN }, true);
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
        ReadUntil(new char[] { Token.NEW_LINE }, false);
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
            return string.Empty;
        }
        else
        {
            char c = (char)read;

            if (c is Token.SPACE or Token.TAB)
            {
                goto readNext;
            }
            else if (c is Token.RETURN or Token.NEW_LINE)
            {
                return string.Empty;
            }
            if (c == Token.STRING_QUOTE_1)
            {
                return ReadUntil(new char[] { Token.STRING_QUOTE_1 }, false);
            }
            else if (c == Token.STRING_QUOTE_2)
            {
                return ReadUntil(new char[] { Token.STRING_QUOTE_2 }, false);
            }
            else
            {
                return (c + ReadUntil(new char[] { Token.NEW_LINE }, true, true)).Trim();
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
            else if (breakOnComment && (c is Token.COMMENT_1 or Token.COMMENT_2))
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
            throw new ObjectDisposedException(nameof(IniReader));
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

    /// <inheritdoc/>
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }
}
