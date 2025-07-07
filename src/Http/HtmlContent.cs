// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HtmlContent.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;

namespace Sisk.Core.Http;

/// <summary>
/// Provides HTTP content based on HTML contents.
/// </summary>
public class HtmlContent : StringContent {

    /// <summary>
    /// Creates an new <see cref="HtmlContent"/> class with given HTML content and encoding.
    /// </summary>
    /// <param name="content">The HTML content string.</param>
    /// <param name="encoding">The encoding which will encode the HTML contents.</param>
    public HtmlContent ( string content, Encoding encoding ) : base ( content, encoding, "text/html" ) {
    }

    /// <summary>
    /// Creates an new <see cref="HtmlContent"/> class with given HTML content, using the environment default encoding.
    /// </summary>
    /// <param name="content">The HTML content string.</param>
    public HtmlContent ( string content ) : this ( content, Encoding.Default ) { }

    /// <summary>
    /// Creates a new <see cref="HtmlContent"/> class with given HTML content as a byte span and encoding.
    /// </summary>
    /// <param name="contents">The HTML content as a byte span.</param>
    /// <param name="encoding">The encoding which will decode the HTML contents.</param>
    public HtmlContent ( ReadOnlySpan<byte> contents, Encoding encoding ) : base ( encoding.GetString ( contents ), encoding, "text/html" ) {
    }

    /// <summary>
    /// Creates a new <see cref="HtmlContent"/> class with given HTML content as a UTF-8 encoded byte span.
    /// </summary>
    /// <param name="utf8Contents">The HTML content as a UTF-8 encoded byte span.</param>
    public HtmlContent ( ReadOnlySpan<byte> utf8Contents ) : this ( utf8Contents, Encoding.UTF8 ) {
    }
}
