namespace Sisk.IniConfiguration.Core.Serialization;

/// <summary>
/// Specifies the behavior for writing new lines in an INI file.
/// </summary>
[Flags]
public enum IniWritingNewLineBehavior {
    /// <summary>
    /// Quotes the value when writing new lines.
    /// </summary>
    Quote = 1 << 0,
    /// <summary>
    /// Splits the value into multiple lines when writing new lines.
    /// </summary>
    Split = 1 << 1,
    /// <summary>
    /// Escapes the new line characters when writing new lines.
    /// </summary>
    Escape = 1 << 2
}

/// <summary>
/// Represents a writer for INI files.
/// </summary>
public sealed class IniWriter : IDisposable {

    private readonly TextWriter writer;

    /// <summary>
    /// Gets or sets the behavior for writing new lines.
    /// </summary>
    public IniWritingNewLineBehavior NewLineBehavior { get; set; } = IniWritingNewLineBehavior.Quote;

    /// <summary>
    /// Gets or sets the default comment character.
    /// </summary>
    public char CommentChar { get; set; } = Token.COMMENT_2;

    /// <summary>
    /// Gets the underlying text writer.
    /// </summary>
    public TextWriter Writer {
        get => this.writer;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="IniWriter"/> class.
    /// </summary>
    /// <param name="writer">The underlying text writer.</param>
    public IniWriter ( TextWriter writer ) {
        this.writer = writer;
    }

    /// <summary>
    /// Writes a comment to the INI file.
    /// </summary>
    /// <param name="commentString">The comment to write.</param>
    public void WriteComment ( string commentString ) {
        foreach (var line in commentString.Split ( '\n', '\r' )) {
            this.writer.WriteLine ( $"{this.CommentChar} {line.TrimEnd ()}" );
        }
    }

    /// <summary>
    /// Writes a key-value pair to the INI file.
    /// </summary>
    /// <param name="key">The key to write.</param>
    /// <param name="value">The value to write.</param>
    public void Write ( string key, string? value ) {
        if (value is null) {
            this.writer.WriteLine ( $"{key.Trim ()} = " );
        }
        else {
            string carry = value;

            bool quoteValue = false;
            bool splitNewLineEndings = false;
            bool escapeNewLineEndings = false;

            if (value.Contains ( Token.COMMENT_1 ) || value.Contains ( Token.COMMENT_2 )) {
                quoteValue = true;
            }

            if (value.Contains ( '\n' )) {
                if (this.NewLineBehavior.HasFlag ( IniWritingNewLineBehavior.Quote ))
                    quoteValue = true;
                if (this.NewLineBehavior.HasFlag ( IniWritingNewLineBehavior.Split ))
                    splitNewLineEndings = true;
                if (this.NewLineBehavior.HasFlag ( IniWritingNewLineBehavior.Escape ))
                    escapeNewLineEndings = true;
            }

            if (!quoteValue) {
                var carrySpan = carry.AsSpan ();

                if (carrySpan.Count ( Token.STRING_QUOTE_1 ) % 2 != 0 || carrySpan.Count ( Token.STRING_QUOTE_2 ) % 2 != 0) {
                    quoteValue = true;
                }
            }

            if (escapeNewLineEndings)
                carry = carry.Replace ( "\n", "\\n" );

            if (quoteValue)
                carry = carry.Contains ( Token.STRING_QUOTE_1 ) ?
                    $"{Token.STRING_QUOTE_2}{carry}{Token.STRING_QUOTE_2}" :
                    $"{Token.STRING_QUOTE_1}{carry}{Token.STRING_QUOTE_1}";

            if (splitNewLineEndings) {
                foreach (var line in carry.Split ( '\n', '\r' )) {
                    this.Write ( key, line );
                }
            }
            else {
                this.Writer.WriteLine ( $"{key.Trim ()} = {carry}" );
            }
        }
    }

    /// <summary>
    /// Writes a key-value pair to the INI file, where the value is an array of strings.
    /// </summary>
    /// <param name="value">The key-value pair to write.</param>
    public void Write ( in KeyValuePair<string, string []> value ) {
        foreach (var line in value.Value)
            this.Write ( value.Key, line );
    }

    /// <summary>
    /// Writes an INI section to the INI file.
    /// </summary>
    /// <param name="section">The section to write.</param>
    public void Write ( IniSection section ) {
        if (section.Name != IniReader.INITIAL_SECTION_NAME) {
            this.writer.Write ( Token.SECTION_START );
            this.writer.Write ( section.Name.Trim () );
            this.writer.WriteLine ( Token.SECTION_END );
        }

        foreach (var entry in section)
            this.Write ( entry );

        this.writer.WriteLine ();
    }

    /// <summary>
    /// Writes an INI document to the INI file.
    /// </summary>
    /// <param name="document">The document to write.</param>
    public void Write ( IniDocument document ) {
        foreach (var section in document.Sections)
            this.Write ( section );
    }

    /// <summary>
    /// Releases all resources used by the <see cref="IniWriter"/> object.
    /// </summary>
    public void Dispose () {
        this.writer.Dispose ();
    }
}
