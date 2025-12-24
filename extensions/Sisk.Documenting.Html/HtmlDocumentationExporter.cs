// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HtmlDocumentationExporter.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Routing;
using TinyComponents;

namespace Sisk.Documenting.Html;

/// <summary>
/// Represents a class for exporting API documentation to HTML format.
/// </summary>
public class HtmlDocumentationExporter : IApiDocumentationExporter {

    /// <summary>
    /// Creates an new instance of the <see cref="HtmlDocumentationExporter"/> class.
    /// </summary>
    public HtmlDocumentationExporter () {
    }

    /// <summary>
    /// Gets or sets the title of the HTML page.
    /// </summary>
    public string PageTitle { get; set; } = "API documentation";

    /// <summary>
    /// Gets or sets the CSS styles of the HTML page.
    /// </summary>
    public string Style { get; set; } = Html.Style.DefaultStyles;

    /// <summary>
    /// Gets or sets the JavaScript script to be included in the HTML page.
    /// </summary>
    public string? Script { get; set; }

    /// <summary>
    /// Gets or sets the format string for the main title service version.
    /// </summary>
    public string FormatMainTitleServiceVersion { get; set; } = "Service version: {0}";

    /// <summary>
    /// Gets or sets the format string for endpoint headers.
    /// </summary>
    public string FormatEndpointHeaders { get; set; } = "Headers:";

    /// <summary>
    /// Gets or sets the format string for endpoint path parameters.
    /// </summary>
    public string FormatEndpointPathParameters { get; set; } = "Path parameters:";

    /// <summary>
    /// Gets or sets the format string for endpoint request parameters.
    /// </summary>
    public string FormatEndpointParameters { get; set; } = "Request parameters:";

    /// <summary>
    /// Gets or sets the format string for endpoint responses.
    /// </summary>
    public string FormatEndpointResponses { get; set; } = "Responses:";

    /// <summary>
    /// Gets or sets the format string for endpoint request examples.
    /// </summary>
    public string FormatEndpointRequestExamples { get; set; } = "Request examples:";

    /// <summary>
    /// Gets or sets the format string for required text.
    /// </summary>
    public string FormatRequiredText { get; set; } = "Required";

    /// <summary>
    /// Gets or sets an optional object to append after the generated contents, right at the end of the <c>&lt;main&gt;</c>
    /// tag of the generated page.
    /// </summary>
    public object? Footer { get; set; }

    /// <summary>
    /// Gets or sets an optional object to append after the main title, at the beginning of the <c>&lt;main&gt;</c>
    /// tag of the generated page.
    /// </summary>
    public object? Header { get; set; }

    /// <summary>
    /// Gets or sets an optional object to append inside the <c>&lt;head&gt;</c>
    /// tag of the generated page.
    /// </summary>
    public object? Head { get; set; }

    /// <summary>
    /// Includes the Prism.js syntax-highlighter library by appending its JavaScript and CSS to the current
    /// <see cref="Script"/> and <see cref="Style"/> properties.
    /// </summary>
    public void IncludePrismJs () {
        Script += $"(()=>{{{PrismJs.Js}}})();";
        Style += "\n\n" + PrismJs.Css;
    }

    /// <summary>
    /// Writes the main title of the API documentation.
    /// </summary>
    /// <param name="documentation">The API documentation to write the title for.</param>
    /// <returns>The HTML element representing the main title.</returns>
    protected virtual HtmlElement? WriteMainTitle ( ApiDocumentation documentation ) {
        return HtmlElement.Fragment ( fragment => {
            fragment += new HtmlElement ( "h1", documentation.ApplicationName ?? "Application name" )
                .WithClass ( "app-title" );
            fragment += CreateParagraphs ( documentation.ApplicationDescription );
            fragment += new HtmlElement ( "p", string.Format ( FormatMainTitleServiceVersion, documentation.ApiVersion ?? "1.0" ) );
        } );
    }

    /// <summary>
    /// Writes the description of an API endpoint.
    /// </summary>
    /// <param name="endpoint">The API endpoint to write the description for.</param>
    /// <returns>The HTML element representing the endpoint description, or null if no description is available.</returns>
    protected virtual HtmlElement? WriteEndpointDescription ( ApiEndpoint endpoint ) {
        return new HtmlElement ( "details", details => {
            details.ClassList.Add ( "endpoint-description" );

            details += new HtmlElement ( "summary", summary => {
                summary.Id = TransformId ( endpoint.Name );

                summary += new HtmlElement ( "span", endpoint.RouteMethod ).WithStyle ( new { backgroundColor = GetRouteMethodHexColor ( endpoint.RouteMethod ) + "40" } );
                summary += new HtmlElement ( "span", endpoint.Path );
                summary += new HtmlElement ( "span", $" - {endpoint.Name}" ).WithClass ( "muted" );
            } );

            details += new HtmlElement ( "h3", endpoint.Name );
            details += CreateParagraphs ( endpoint.Description );

            details += CreateCodeBlock ( $"{endpoint.RouteMethod.ToString ().ToUpper ()} {endpoint.Path}", null );

            if (endpoint.Headers.Length > 0) {
                details += new HtmlElement ( "div", div => {
                    div += new HtmlElement ( "p", FormatEndpointHeaders );

                    div += new HtmlElement ( "ul", ul => {
                        foreach (var header in endpoint.Headers) {
                            ul += new HtmlElement ( "li", li => {
                                li.ClassList.Add ( "item-description" );

                                li += new HtmlElement ( "code", header.HeaderName );
                                li += new HtmlElement ( "span", header.IsRequired ? FormatRequiredText : "" ).WithClass ( "at", "ml3" );
                                li += new HtmlElement ( "div", CreateParagraphs ( header.Description ) );
                            } );
                        }
                    } );
                } );
            }

            if (endpoint.PathParameters.Length > 0) {
                details += new HtmlElement ( "div", div => {
                    div += new HtmlElement ( "p", FormatEndpointPathParameters );

                    div += new HtmlElement ( "ul", ul => {
                        foreach (var pathParam in endpoint.PathParameters) {
                            ul += new HtmlElement ( "li", li => {
                                li.ClassList.Add ( "item-description" );

                                li += new HtmlElement ( "code", pathParam.Name );
                                li += new HtmlElement ( "span", pathParam.Type ).WithClass ( "muted", "ml3" );
                                li += new HtmlElement ( "div", CreateParagraphs ( pathParam.Description ) );
                            } );
                        }
                    } );
                } );
            }

            if (endpoint.Parameters.Length > 0) {
                details += new HtmlElement ( "div", div => {
                    div += new HtmlElement ( "p", FormatEndpointParameters );

                    div += new HtmlElement ( "ul", ul => {
                        foreach (var param in endpoint.Parameters) {
                            ul += new HtmlElement ( "li", li => {
                                li.ClassList.Add ( "item-description" );

                                li += new HtmlElement ( "code", param.Name );
                                li += new HtmlElement ( "span", param.TypeName ).WithClass ( "muted", "ml3" );
                                li += new HtmlElement ( "span", param.IsRequired ? FormatRequiredText : "" ).WithClass ( "at", "ml3" );
                                li += new HtmlElement ( "div", CreateParagraphs ( param.Description ) );
                            } );
                        }
                    } );
                } );
            }

            if (endpoint.RequestExamples.Length > 0) {
                details += new HtmlElement ( "div", div => {
                    div += new HtmlElement ( "p", FormatEndpointRequestExamples );

                    div += new HtmlElement ( "ul", ul => {
                        foreach (var req in endpoint.RequestExamples) {
                            ul += new HtmlElement ( "li", li => {
                                li.ClassList.Add ( "item-description" );

                                li += CreateParagraphs ( req.Description );

                                if (req.Example != null) {
                                    li += new HtmlElement ( "div", exampleDiv => {
                                        exampleDiv += CreateCodeBlock ( req.Example, req.ExampleLanguage );
                                    } );
                                }
                            } );
                        }
                    } );
                } );
            }

            if (endpoint.Responses.Length > 0) {
                details += new HtmlElement ( "div", div => {
                    div += new HtmlElement ( "p", FormatEndpointResponses );

                    div += new HtmlElement ( "ul", ul => {
                        foreach (var res in endpoint.Responses) {
                            ul += new HtmlElement ( "li", li => {
                                li.ClassList.Add ( "item-description" );

                                li += new HtmlElement ( "code", (int) res.StatusCode );
                                li += new HtmlElement ( "div", CreateParagraphs ( res.Description ) );

                                if (res.Example != null) {
                                    li += new HtmlElement ( "div", exampleDiv => {
                                        exampleDiv += CreateCodeBlock ( res.Example, res.ExampleLanguage );
                                    } );
                                }
                            } );
                        }
                    } );
                } );
            }
        } );
    }

    /// <summary>
    /// Creates an HTML code block element from the provided code and language.
    /// </summary>
    /// <param name="code">The code to display in the code block.</param>
    /// <param name="language">The programming language of the code, or null for no language highlighting.</param>
    /// <returns>The HTML element representing the code block, or null if no code is provided.</returns>
    protected virtual HtmlElement? CreateCodeBlock ( string code, string? language ) {
        return new HtmlElement ( "pre", pre => {
            pre += new HtmlElement ( "code", cblock => {
                if (language != null) {
                    cblock.ClassList.Add ( $"language-{language}" );
                    cblock.ClassList.Add ( $"lang-{language}" );
                }
                cblock += code;
            } );
        } );
    }

    /// <summary>
    /// Creates an HTML badge element for an API endpoint.
    /// </summary>
    /// <param name="method">The HTTP method of the endpoint (e.g. GET, POST, PUT, etc.).</param>
    /// <param name="path">The path of the endpoint, or null for no path display.</param>
    /// <returns>The HTML element representing the endpoint badge, or null if no badge is applicable.</returns>
    protected virtual HtmlElement? CreateEndpointBadge ( RouteMethod method, string? path ) {

        string spanColor = GetRouteMethodHexColor ( method );

        return new HtmlElement ( "span22", span => {
            span.ClassList.Add ( "endpoint-badge" );
            span += new HtmlElement ( "span", method ).WithStyle ( new { backgroundColor = $"{spanColor}43" } );
            span += new HtmlElement ( "span", path );
        } );
    }

    /// <summary>
    /// Creates one or more HTML paragraph elements from the provided text.
    /// </summary>
    /// <param name="text">The text to display in the paragraphs, or null for no paragraphs.</param>
    /// <returns>The HTML element representing the paragraphs, or null if no text is provided.</returns>
    [return: NotNullIfNotNull ( nameof ( text ) )]
    protected virtual object? CreateParagraphs ( string? text ) {
        if (text is null)
            return null;

        return new MarkdownText ( text );
    }

    /// <summary>
    /// Gets the hex color code associated with the specified route method.
    /// </summary>
    /// <param name="rm">The route method to get the color for.</param>
    /// <returns>The hex color code as a string.</returns>
    protected virtual string GetRouteMethodHexColor ( RouteMethod rm ) {
        return rm switch {
            RouteMethod.Get => "#0bc70f",
            RouteMethod.Post => "#f26710",
            RouteMethod.Put => "#3210f2",
            RouteMethod.Patch => "#6319c4",
            RouteMethod.Delete => "#c41919",
            _ => "#549696"
        };
    }

    /// <summary>
    /// Transforms an unsafe ID into a safe and valid HTML ID.
    /// </summary>
    /// <param name="unsafeId">The ID to transform, which may contain invalid characters.</param>
    /// <returns>The transformed ID, which is safe for use in HTML.</returns>
    protected string TransformId ( string unsafeId ) {
        return new string ( unsafeId.Where ( char.IsLetterOrDigit ).ToArray () );
    }

    /// <summary>
    /// Truncates a string to the specified size, appending an ellipsis if necessary.
    /// </summary>
    /// <param name="s">The string to truncate, or null for no truncation.</param>
    /// <param name="size">The maximum length of the string, including the ellipsis.</param>
    /// <returns>The truncated string, or the original string if it is already within the size limit, or null if the input string is null.</returns>
    [return: NotNullIfNotNull ( nameof ( s ) )]
    protected string? Ellipsis ( string? s, int size ) {
        if (string.IsNullOrEmpty ( s )) {
            return s;
        }
        if (s.Length <= size) {
            return s;
        }
        if (size <= 3) {
            return s.Substring ( 0, size ) + "...";
        }
        return s.Substring ( 0, size - 3 ) + "...";
    }

    /// <summary>
    /// Exports the API documentation as an HTML string.
    /// </summary>
    /// <param name="d">The API documentation to export.</param>
    /// <returns>The exported HTML string.</returns>
    public string ExportHtml ( ApiDocumentation d ) {
        HtmlElement html = new HtmlElement ( "html" );

        html += new HtmlElement ( "head", head => {
            head += new HtmlElement ( "meta" )
                .WithAttribute ( "charset", "UTF-8" )
                .SelfClosed ();
            head += new HtmlElement ( "meta" )
                .WithAttribute ( "name", "viewport" )
                .WithAttribute ( "content", "width=device-width, initial-scale=1.0" )
                .SelfClosed ();

            head += new HtmlElement ( "title", PageTitle );
            head += new HtmlElement ( "style", RenderableText.Raw ( Style ) );
            head += new HtmlElement ( "script", RenderableText.Raw ( Script ) );
        } );

        html += new HtmlElement ( "body", body => {
            body += new HtmlElement ( "main", main => {

                main += WriteMainTitle ( d );

                main += Header;

                var groups = d.Endpoints.GroupBy ( e => e.Group );
                foreach (var item in groups) {

                    main += new HtmlElement ( "h2", $"{item.Key ?? "Requests"}:" );

                    foreach (var endpoint in item.OrderBy ( i => i.Path.Length )) {
                        main += new HtmlElement ( "section", section => {
                            section.ClassList.Add ( "endpoint" );

                            section += WriteEndpointDescription ( endpoint );
                        } );
                    }
                }

                main += Footer;
            } );
        } );

        return html.ToString ();
    }

    /// <summary>
    /// Exports the API documentation as HTTP content.
    /// </summary>
    /// <param name="documentation">The API documentation to export.</param>
    /// <returns>The exported API documentation as HTTP content.</returns>
    public HttpContent ExportDocumentationContent ( ApiDocumentation documentation ) {
        return new HtmlContent ( ExportHtml ( documentation ), Encoding.UTF8 );
    }
}
