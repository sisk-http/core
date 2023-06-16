using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Http;

/// <summary>
/// Provides HTTP content based on HTML file.
/// </summary>
/// <definition>
/// public class HtmlContent : ByteArrayContent
/// </definition>
/// <type>
/// Class
/// </type>
public class HtmlContent : ByteArrayContent
{
    /// <summary>
    /// Gets or sets the default encoding which will be used on constructors.
    /// </summary>
    /// <definition>
    /// public static Encoding DefaultEncoding { get; set; }
    /// </definition>
    /// <type>
    /// Static property
    /// </type>
    public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

    /// <summary>
    /// Creates an new <see cref="HtmlContent"/> class with given HTML content and encoding.
    /// </summary>
    /// <param name="content">The HTML content string.</param>
    /// <param name="encoding">The encoding which will encode the HTML contents.</param>
    /// <definition>
    /// public HtmlContent(string content, Encoding? encoding)
    /// </definition>
    /// <type>
    /// Constructor
    /// </type>
    public HtmlContent(string content, Encoding? encoding) : base(GetContentBytes(content, encoding))
    {
        Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue("text/html");
    }

    /// <summary>
    /// Creates an new <see cref="HtmlContent"/> class with given HTML content, using the <see cref="DefaultEncoding"/> encoding.
    /// </summary>
    /// <param name="content">The HTML content string.</param>
    /// <definition>
    /// public HtmlContent(string content)
    /// </definition>
    /// <type>
    /// Constructor
    /// </type>
    public HtmlContent(string content) : this(content, DefaultEncoding) { }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    private static byte[] GetContentBytes(string content, Encoding? encoder)
    {
        ArgumentNullException.ThrowIfNull(content, nameof(content));
        return (encoder ?? Encoding.UTF8).GetBytes(content);
    }
}
