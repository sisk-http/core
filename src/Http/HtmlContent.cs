// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
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
public class HtmlContent : StringContent
{
    /// <summary>
    /// Gets or sets the default encoding which will be used on constructors.
    /// </summary>
    public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Creates an new <see cref="HtmlContent"/> class with given HTML content and encoding.
    /// </summary>
    /// <param name="content">The HTML content string.</param>
    /// <param name="encoding">The encoding which will encode the HTML contents.</param>
    public HtmlContent(string content, Encoding encoding) : base(content, encoding, "text/html")
    {
    }

    /// <summary>
    /// Creates an new <see cref="HtmlContent"/> class with given HTML content, using the <see cref="DefaultEncoding"/> encoding.
    /// </summary>
    /// <param name="content">The HTML content string.</param>
    public HtmlContent(string content) : this(content, DefaultEncoding) { }
}
