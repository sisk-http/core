// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PrefixedLogStream.cs
// Repository:  https://github.com/sisk-http/core


namespace Sisk.Core.Http;

/// <summary>
/// Represents a log stream that prefixes log messages with a custom string.
/// </summary>
[System.Diagnostics.CodeAnalysis.SuppressMessage ( "Naming", "CA1711:Identifiers should not have incorrect suffix",
    Justification = "Breaking change." )]
public sealed class PrefixedLogStream : LogStream {

    /// <summary>
    /// Gets or sets a function that returns the prefix to be added to log messages.
    /// </summary>
    /// <remarks>The assigned function should return the desired prefix as a string. If the function returns
    /// null, it is treated as an empty string in most scenarios.</remarks>
    public Func<string?> PrefixFunction { get; set; } = delegate { return string.Empty; };

    /// <summary>
    /// Gets or sets the function that provides a suffix string value.
    /// </summary>
    /// <remarks>The assigned function should return the desired suffix as a string. If the function returns
    /// null, it is treated as an empty string in most scenarios.</remarks>
    public Func<string?> SuffixFunction { get; set; } = delegate { return string.Empty; };

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixedLogStream"/> class with the specified prefix function.
    /// </summary>
    /// <param name="prefixFunction">A function that returns the prefix to be added to log messages.</param>
    public PrefixedLogStream ( Func<string> prefixFunction ) {
        PrefixFunction = prefixFunction;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixedLogStream"/> class with the specified prefix function and text writer.
    /// </summary>
    /// <param name="prefixFunction">A function that returns the prefix to be added to log messages.</param>
    /// <param name="tw">The text writer to write log messages to.</param>
    public PrefixedLogStream ( Func<string> prefixFunction, TextWriter tw ) : base ( tw ) {
        PrefixFunction = prefixFunction;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixedLogStream"/> class with the specified prefix function and file name.
    /// </summary>
    /// <param name="prefixFunction">A function that returns the prefix to be added to log messages.</param>
    /// <param name="filename">The name of the file to write log messages to.</param>
    public PrefixedLogStream ( Func<string> prefixFunction, string filename ) : base ( filename ) {
        PrefixFunction = prefixFunction;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="PrefixedLogStream"/> class with the specified prefix function, file name, and text writer.
    /// </summary>
    /// <param name="prefixFunction">A function that returns the prefix to be added to log messages.</param>
    /// <param name="filename">The name of the file to write log messages to, or <c>null</c> to write to the text writer.</param>
    /// <param name="tw">The text writer to write log messages to, or <c>null</c> to write to the file.</param>
    public PrefixedLogStream ( Func<string> prefixFunction, string? filename, TextWriter? tw ) : base ( filename, tw ) {
        PrefixFunction = prefixFunction;
    }

    /// <inheritdoc/>
    protected override void WriteLineInternal ( string line ) {
        string? prefix = PrefixFunction ();
        string? suffix = SuffixFunction ();
        base.WriteLineInternal ( string.Concat ( prefix, line, suffix ) );
    }

    /// <inheritdoc/>
    protected override ValueTask WriteLineInternalAsync ( string line ) {
        string? prefix = PrefixFunction ();
        string? suffix = SuffixFunction ();
        return base.WriteLineInternalAsync ( string.Concat ( prefix, line, suffix ) );
    }
}
