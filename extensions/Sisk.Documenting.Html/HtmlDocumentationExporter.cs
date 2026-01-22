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
using System.Text.Encodings.Web;
using Sisk.Core.Helpers;
using Sisk.Core.Http;
using Sisk.Core.Routing;
using TinyComponents;

namespace Sisk.Documenting.Html;

/// <summary>
/// Represents a class for exporting API documentation to HTML format.
/// </summary>
public class HtmlDocumentationExporter : IApiDocumentationExporter {

    // search-line from Remix Icon
    const string InputSearchIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M18.031 16.6168L22.3137 20.8995L20.8995 22.3137L16.6168 18.031C15.0769 19.263 13.124 20 11 20C6.032 20 2 15.968 2 11C2 6.032 6.032 2 11 2C15.968 2 20 6.032 20 11C20 13.124 19.263 15.0769 18.031 16.6168ZM16.0247 15.8748C17.2475 14.6146 18 12.8956 18 11C18 7.1325 14.8675 4 11 4C7.1325 4 4 7.1325 4 11C4 14.8675 7.1325 18 11 18C12.8956 18 14.6146 17.2475 15.8748 16.0247L16.0247 15.8748Z"></path></svg>
            """;

    // arrow-right-s-line from Remix Icon
    const string MenuArrowIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M13.1717 12.0007L8.22192 7.05093L9.63614 5.63672L16.0001 12.0007L9.63614 18.3646L8.22192 16.9504L13.1717 12.0007Z"></path></svg>
            """;

    // file-copy-line from Remix Icon
    const string CopyIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M6.9998 6V3C6.9998 2.44772 7.44752 2 7.9998 2H19.9998C20.5521 2 20.9998 2.44772 20.9998 3V17C20.9998 17.5523 20.5521 18 19.9998 18H16.9998V20.9991C16.9998 21.5519 16.5499 22 15.993 22H4.00666C3.45059 22 3 21.5554 3 20.9991L3.0026 7.00087C3.0027 6.44811 3.45264 6 4.00942 6H6.9998ZM5.00242 8L5.00019 20H14.9998V8H5.00242ZM8.9998 6H16.9998V16H18.9998V4H8.9998V6Z"></path></svg>
            """;

    // external-link-line from Remix Icon
    const string ExternalLinkIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M10 6V8H5V19H16V14H18V20C18 20.5523 17.5523 21 17 21H4C3.44772 21 3 20.5523 3 20V7C3 6.44772 3.44772 6 4 6H10ZM21 3V11H19L18.9999 6.413L11.2071 14.2071L9.79289 12.7929L17.5849 5H13V3H21Z"></path></svg>
            """;

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
    /// Gets or sets the format string for endpoint query parameters.
    /// </summary>
    public string FormatEndpointQueryParameters { get; set; } = "Query parameters:";

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
    /// Gets or sets whether to include the sidebar navigation. Default is true.
    /// </summary>
    public bool IncludeSidebar { get; set; } = true;

    /// <summary>
    /// Gets or sets the format string for the navigation section title for API Reference.
    /// </summary>
    public string FormatNavApiReference { get; set; } = "API Reference";

    /// <summary>
    /// Gets or sets the format string for the "Example" tab label.
    /// </summary>
    public string FormatTabExample { get; set; } = "Example";

    /// <summary>
    /// Gets or sets the format string for the "Schema" tab label.
    /// </summary>
    public string FormatTabSchema { get; set; } = "Schema";

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
        return new HtmlElement ( "div", div => {
            div.ClassList.Add ( "content-header" );

            div += new HtmlElement ( "h1", documentation.ApplicationName ?? "Application name" );

            if (!string.IsNullOrEmpty ( documentation.ApplicationDescription )) {
                div += new HtmlElement ( "p", CreateParagraphs ( documentation.ApplicationDescription ) )
                    .WithClass ( "description" );
            }

            div += new HtmlElement ( "p", string.Format ( FormatMainTitleServiceVersion, documentation.ApiVersion ?? "1.0" ) )
                .WithClass ( "muted" );
        } );
    }

    /// <summary>
    /// Writes the sidebar navigation for the API documentation.
    /// </summary>
    /// <param name="documentation">The API documentation to generate navigation for.</param>
    /// <returns>The HTML element representing the sidebar navigation.</returns>
    protected virtual HtmlElement? WriteSidebarNavigation ( ApiDocumentation documentation ) {
        return new HtmlElement ( "nav", nav => {
            nav.ClassList.Add ( "sidebar" );

            nav += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "sidebar-header" );

                header += new HtmlElement ( "h1", documentation.ApplicationName ?? "API" );
                header += new HtmlElement ( "span", $"v{documentation.ApiVersion ?? "1.0"}" )
                    .WithClass ( "version" );

                header += new HtmlElement ( "button", searchBtn => {
                    searchBtn.ClassList.Add ( "search-trigger" );
                    searchBtn.Attributes [ "type" ] = "button";
                    searchBtn += RenderableText.Raw ( InputSearchIcon );
                    searchBtn += new HtmlElement ( "span", "Search..." );
                    searchBtn += new HtmlElement ( "kbd", "Ctrl+K" );
                } );
            } );

            nav += new HtmlElement ( "div", section => {
                section.ClassList.Add ( "nav-section" );

                section += new HtmlElement ( "div", FormatNavApiReference )
                    .WithClass ( "nav-section-title" );

                var groups = documentation.Endpoints.DistinctBy ( e => $"{e.RouteMethod} {e.Name}" ).GroupBy ( e => e.Group );
                foreach (var group in groups) {
                    section += new HtmlElement ( "div", navGroup => {
                        navGroup.ClassList.Add ( "nav-group" );
                        navGroup.ClassList.Add ( "open" );

                        navGroup += new HtmlElement ( "div", groupHeader => {
                            groupHeader.ClassList.Add ( "nav-group-header" );

                            groupHeader += new HtmlElement ( "span", RenderableText.Raw ( MenuArrowIcon ) ).WithClass ( "arrow" );
                            groupHeader += new HtmlElement ( "span", group.Key ?? "Requests" );
                        } );

                        navGroup += new HtmlElement ( "div", navItems => {
                            navItems.ClassList.Add ( "nav-items" );

                            foreach (var endpoint in group.OrderBy ( i => i.Path.Length )) {
                                navItems += new HtmlElement ( "a", navItem => {
                                    navItem.ClassList.Add ( "nav-item" );
                                    navItem.Attributes [ "href" ] = $"#{TransformId ( endpoint.Name )}";

                                    navItem += new HtmlElement ( "span", endpoint.RouteMethod.ToString ().ToUpper () )
                                        .WithClass ( "method-badge" )
                                        .WithStyle ( new { backgroundColor = GetRouteMethodHexColor ( endpoint.RouteMethod ) + "30" } );

                                    navItem += new HtmlElement ( "span", Ellipsis ( endpoint.Name, 25 ) );
                                } );
                            }
                        } );
                    } );
                }
            } );
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

                summary += new HtmlElement ( "span", endpoint.RouteMethod )
                    .WithStyle ( new { backgroundColor = GetRouteMethodHexColor ( endpoint.RouteMethod ) + "35" } );
                summary += new HtmlElement ( "span", endpoint.Path );
                summary += new HtmlElement ( "span", endpoint.Name ).WithClass ( "muted" );
            } );

            details += new HtmlElement ( "div", content => {
                content.ClassList.Add ( "endpoint-content" );

                content += new HtmlElement ( "div", header => {
                    header.ClassList.Add ( "endpoint-content-header" );

                    header += new HtmlElement ( "h3", endpoint.Name );

                    header += new HtmlElement ( "div", copyWrapper => {
                        copyWrapper.ClassList.Add ( "copy-button-wrapper" );

                        var context = HttpContext.GetCurrentContext ();
                        if (context?.Request.Query [ "embed" ] == "iframe") {
                            copyWrapper += new HtmlElement ( "a", btn => {
                                btn.ClassList.Add ( "open-ext-button" );
                                btn.Attributes [ "href" ] = $"{context.Request.Path}#{TransformId ( endpoint.Name )}";
                                btn.Attributes [ "target" ] = "_blank";
                                btn.Attributes [ "aria-label" ] = "Open in new tab";
                                btn += RenderableText.Raw ( ExternalLinkIcon );
                            } );
                        }

                        copyWrapper += new HtmlElement ( "button", btn => {
                            btn.ClassList.Add ( "copy-button" );
                            btn.Attributes [ "type" ] = "button";
                            btn.Attributes [ "aria-label" ] = "Copy options";
                            btn += RenderableText.Raw ( CopyIcon );
                        } );

                        copyWrapper += new HtmlElement ( "div", dropdown => {
                            dropdown.ClassList.Add ( "dropdown-menu" );

                            dropdown += new HtmlElement ( "button", item => {
                                item.ClassList.Add ( "dropdown-item" );
                                item.Attributes [ "data-action" ] = "copy-link";
                                item.Attributes [ "data-target-id" ] = TransformId ( endpoint.Name );
                                item.Attributes [ "type" ] = "button";
                                item += "Copy link";
                            } );

                            dropdown += new HtmlElement ( "button", item => {
                                item.ClassList.Add ( "dropdown-item" );
                                item.Attributes [ "data-action" ] = "copy-markdown";
                                item.Attributes [ "data-name" ] = endpoint.Name;
                                item.Attributes [ "data-target-id" ] = TransformId ( endpoint.Name );
                                item.Attributes [ "type" ] = "button";
                                item += "Copy markdown";
                            } );

                            dropdown += new HtmlElement ( "button", item => {
                                item.ClassList.Add ( "dropdown-item" );
                                item.Attributes [ "data-action" ] = "copy-embed";
                                item.Attributes [ "data-endpoint-name" ] = endpoint.Name;
                                item.Attributes [ "type" ] = "button";
                                item += "Copy embed";
                            } );
                        } );
                    } );
                } );

                content += CreateParagraphs ( endpoint.Description );

                content += CreateCodeBlock ( $"{endpoint.RouteMethod.ToString ().ToUpper ()} {endpoint.Path}", null );

                if (endpoint.Headers.Length > 0) {
                    content += new HtmlElement ( "div", div => {
                        div.ClassList.Add ( "params-section" );

                        div += new HtmlElement ( "p", FormatEndpointHeaders );

                        div += new HtmlElement ( "ul", ul => {
                            ul.ClassList.Add ( "params-list" );

                            foreach (var header in endpoint.Headers) {
                                ul += new HtmlElement ( "li", li => {
                                    li.ClassList.Add ( "param-item" );

                                    li += new HtmlElement ( "div", headerDiv => {
                                        headerDiv.ClassList.Add ( "param-header" );

                                        headerDiv += new HtmlElement ( "span", header.HeaderName ).WithClass ( "param-name" );
                                        if (header.IsRequired) {
                                            headerDiv += new HtmlElement ( "span", FormatRequiredText ).WithClass ( "param-required" );
                                        }
                                    } );

                                    li += new HtmlElement ( "div", CreateParagraphs ( header.Description ) )
                                        .WithClass ( "param-description" );
                                } );
                            }
                        } );
                    } );
                }

                if (endpoint.PathParameters.Length > 0) {
                    content += new HtmlElement ( "div", div => {
                        div.ClassList.Add ( "params-section" );

                        div += new HtmlElement ( "p", FormatEndpointPathParameters );

                        div += new HtmlElement ( "ul", ul => {
                            ul.ClassList.Add ( "params-list" );

                            foreach (var pathParam in endpoint.PathParameters) {
                                ul += new HtmlElement ( "li", li => {
                                    li.ClassList.Add ( "param-item" );

                                    li += new HtmlElement ( "div", headerDiv => {
                                        headerDiv.ClassList.Add ( "param-header" );

                                        headerDiv += new HtmlElement ( "span", pathParam.Name ).WithClass ( "param-name" );
                                        headerDiv += new HtmlElement ( "span", pathParam.Type ).WithClass ( "param-type" );
                                    } );

                                    li += new HtmlElement ( "div", CreateParagraphs ( pathParam.Description ) )
                                        .WithClass ( "param-description" );
                                } );
                            }
                        } );
                    } );
                }

                if (endpoint.QueryParameters.Length > 0) {
                    content += new HtmlElement ( "div", div => {
                        div.ClassList.Add ( "params-section" );

                        div += new HtmlElement ( "p", FormatEndpointQueryParameters );

                        div += new HtmlElement ( "ul", ul => {
                            ul.ClassList.Add ( "params-list" );

                            foreach (var queryParam in endpoint.QueryParameters) {
                                ul += new HtmlElement ( "li", li => {
                                    li.ClassList.Add ( "param-item" );

                                    li += new HtmlElement ( "div", headerDiv => {
                                        headerDiv.ClassList.Add ( "param-header" );

                                        headerDiv += new HtmlElement ( "span", queryParam.Name ).WithClass ( "param-name" );
                                        headerDiv += new HtmlElement ( "span", queryParam.Type ).WithClass ( "param-type" );
                                        if (queryParam.IsRequired) {
                                            headerDiv += new HtmlElement ( "span", FormatRequiredText ).WithClass ( "param-required" );
                                        }
                                    } );

                                    li += new HtmlElement ( "div", CreateParagraphs ( queryParam.Description ) )
                                        .WithClass ( "param-description" );
                                } );
                            }
                        } );
                    } );
                }

                if (endpoint.Parameters.Length > 0) {
                    content += new HtmlElement ( "div", div => {
                        div.ClassList.Add ( "params-section" );

                        div += new HtmlElement ( "p", FormatEndpointParameters );

                        div += new HtmlElement ( "ul", ul => {
                            ul.ClassList.Add ( "params-list" );

                            foreach (var param in endpoint.Parameters) {
                                ul += new HtmlElement ( "li", li => {
                                    li.ClassList.Add ( "param-item" );

                                    li += new HtmlElement ( "div", headerDiv => {
                                        headerDiv.ClassList.Add ( "param-header" );

                                        headerDiv += new HtmlElement ( "span", param.Name ).WithClass ( "param-name" );
                                        headerDiv += new HtmlElement ( "span", param.TypeName ).WithClass ( "param-type" );
                                        if (param.IsRequired) {
                                            headerDiv += new HtmlElement ( "span", FormatRequiredText ).WithClass ( "param-required" );
                                        }
                                    } );

                                    li += new HtmlElement ( "div", CreateParagraphs ( param.Description ) )
                                        .WithClass ( "param-description" );
                                } );
                            }
                        } );
                    } );
                }

                if (endpoint.RequestExamples.Length > 0) {
                    int reqIndex = 0;
                    content += new HtmlElement ( "div", div => {
                        div.ClassList.Add ( "params-section" );

                        div += new HtmlElement ( "p", FormatEndpointRequestExamples );

                        div += new HtmlElement ( "ul", ul => {
                            ul.ClassList.Add ( "params-list" );

                            foreach (var req in endpoint.RequestExamples) {
                                int currentReqIndex = reqIndex++;
                                ul += new HtmlElement ( "li", li => {
                                    li.ClassList.Add ( "param-item" );

                                    li += CreateParagraphs ( req.Description );

                                    var tabControl = CreateExampleSchemaTabControl (
                                        req.Example,
                                        req.ExampleLanguage,
                                        req.JsonSchema,
                                        $"req-{TransformId ( endpoint.Name )}-{currentReqIndex}"
                                    );
                                    if (tabControl != null) {
                                        li += tabControl;
                                    }
                                } );
                            }
                        } );
                    } );
                }

                if (endpoint.Responses.Length > 0) {
                    int resIndex = 0;
                    content += new HtmlElement ( "div", div => {
                        div.ClassList.Add ( "params-section" );

                        div += new HtmlElement ( "p", FormatEndpointResponses );

                        div += new HtmlElement ( "ul", ul => {
                            ul.ClassList.Add ( "params-list" );

                            foreach (var res in endpoint.Responses) {
                                int currentResIndex = resIndex++;
                                ul += new HtmlElement ( "li", li => {
                                    li.ClassList.Add ( "param-item" );

                                    string statusClass = (int) res.StatusCode switch {
                                        >= 200 and < 300 => "success",
                                        >= 300 and < 400 => "redirect",
                                        _ => "error"
                                    };

                                    li += new HtmlElement ( "span", (int) res.StatusCode )
                                        .WithClass ( "status-code", statusClass );

                                    li += new HtmlElement ( "div", CreateParagraphs ( res.Description ) )
                                        .WithClass ( "param-description" );

                                    var tabControl = CreateExampleSchemaTabControl (
                                        res.Example,
                                        res.ExampleLanguage,
                                        res.JsonSchema,
                                        $"res-{TransformId ( endpoint.Name )}-{currentResIndex}"
                                    );
                                    if (tabControl != null) {
                                        li += tabControl;
                                    }
                                } );
                            }
                        } );
                    } );
                }
            } );
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
    /// Creates a tab control with Example and Schema tabs when both are available.
    /// If only one is available, it renders just that content without tabs.
    /// </summary>
    /// <param name="example">The example content, or null if not available.</param>
    /// <param name="exampleLanguage">The language for syntax highlighting of the example.</param>
    /// <param name="jsonSchema">The JSON schema content, or null if not available.</param>
    /// <param name="tabId">A unique identifier for this tab control instance.</param>
    /// <returns>The HTML element representing the tab control or single content block.</returns>
    protected virtual HtmlElement? CreateExampleSchemaTabControl ( string? example, string? exampleLanguage, string? jsonSchema, string tabId ) {
        bool hasExample = !string.IsNullOrEmpty ( example );
        bool hasSchema = !string.IsNullOrEmpty ( jsonSchema );

        if (!hasExample && !hasSchema) {
            return null;
        }

        if (hasExample && !hasSchema) {
            return CreateCodeBlock ( example!, exampleLanguage );
        }

        if (!hasExample && hasSchema) {
            return CreateCodeBlock ( jsonSchema!, "json" );
        }

        return new HtmlElement ( "div", tabControl => {
            tabControl.ClassList.Add ( "tab-control" );
            tabControl.Attributes [ "data-tab-id" ] = tabId;

            tabControl += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "tab-header" );

                header += new HtmlElement ( "button", btn => {
                    btn.ClassList.Add ( "tab-button" );
                    btn.ClassList.Add ( "active" );
                    btn.Attributes [ "data-tab" ] = "example";
                    btn += FormatTabExample;
                } );

                header += new HtmlElement ( "button", btn => {
                    btn.ClassList.Add ( "tab-button" );
                    btn.Attributes [ "data-tab" ] = "schema";
                    btn += FormatTabSchema;
                } );
            } );

            tabControl += new HtmlElement ( "div", content => {
                content.ClassList.Add ( "tab-content" );
                content.ClassList.Add ( "active" );
                content.Attributes [ "data-tab-content" ] = "example";
                content += CreateCodeBlock ( example!, exampleLanguage );
            } );

            tabControl += new HtmlElement ( "div", content => {
                content.ClassList.Add ( "tab-content" );
                content.Attributes [ "data-tab-content" ] = "schema";
                content += CreateCodeBlock ( jsonSchema!, "json" );
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

        var request = HttpContext.GetCurrentContext ()?.Request;

        HtmlElement html = new HtmlElement ( "html" );
        html.Attributes [ "lang" ] = "en";

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

            if (Head != null) {
                head += Head;
            }
        } );

        html += new HtmlElement ( "body", body => {

            if (request?.Query [ "embed" ] == "iframe" &&
                request.Query [ "embed-endpoint" ].Value is { Length: > 0 } requestingEndpoint) {

                var endpoint = d.Endpoints.FirstOrDefault ( e =>
                    string.Equals ( e.Name, requestingEndpoint, StringComparison.OrdinalIgnoreCase ) );

                if (endpoint != null) {
                    body.ClassList.Add ( "iframe-mode" );
                    html.ClassList.Add ( "iframe-mode" );

                    body += new HtmlElement ( "style", RenderableText.Raw ( Html.Style.EmbedStyles ) );

                    body += new HtmlElement ( "div", container => {
                        container.ClassList.Add ( "iframe-container" );

                        container += new HtmlElement ( "section", section => {
                            section.ClassList.Add ( "endpoint" );

                            var endpointElement = WriteEndpointDescription ( endpoint );
                            if (endpointElement != null) {
                                endpointElement.Attributes [ "open" ] = "open";
                                section += endpointElement;
                            }
                        } );
                    } );

                    if (!string.IsNullOrEmpty ( Script )) {
                        body += new HtmlElement ( "script", RenderableText.Raw ( Script ) );
                    }
                    body += new HtmlElement ( "script", RenderableText.Raw ( Scripts.DefaultScript ) );
                    return;
                }
            }

            body += new HtmlElement ( "div", wrapper => {
                wrapper.ClassList.Add ( "page-wrapper" );

                if (IncludeSidebar) {
                    body += new HtmlElement ( "div", overlay => {
                        overlay.ClassList.Add ( "sidebar-overlay" );
                    } );

                    body += new HtmlElement ( "div", searchOverlay => {
                        searchOverlay.ClassList.Add ( "search-overlay" );

                        searchOverlay += new HtmlElement ( "div", dialog => {
                            dialog.ClassList.Add ( "search-dialog" );

                            dialog += new HtmlElement ( "div", inputWrapper => {
                                inputWrapper.ClassList.Add ( "search-input-wrapper" );
                                inputWrapper += RenderableText.Raw ( MenuArrowIcon );
                                inputWrapper += new HtmlElement ( "input", input => {
                                    input.ClassList.Add ( "search-input" );
                                    input.Attributes [ "type" ] = "text";
                                    input.Attributes [ "placeholder" ] = "Search endpoints...";
                                    input.Attributes [ "autocomplete" ] = "off";
                                } ).SelfClosed ();
                            } );

                            dialog += new HtmlElement ( "div", results => {
                                results.ClassList.Add ( "search-results" );
                            } );
                        } );
                    } );

                    wrapper += WriteSidebarNavigation ( d );

                    body += new HtmlElement ( "button", menuBtn => {
                        menuBtn.ClassList.Add ( "mobile-menu-btn" );
                        menuBtn.Attributes [ "aria-label" ] = "Open navigation menu";
                        menuBtn += "☰";
                    } );
                }

                wrapper += new HtmlElement ( "main", main => {

                    main += WriteMainTitle ( d );

                    main += Header;

                    var groups = d.Endpoints
                        .DistinctBy ( e => $"{e.RouteMethod} {e.Name}" )
                        .GroupBy ( e => e.Group );

                    foreach (var item in groups) {

                        main += new HtmlElement ( "div", groupSection => {
                            groupSection.ClassList.Add ( "group-section" );
                            groupSection.Id = TransformId ( item.Key ?? "Requests" );

                            groupSection += new HtmlElement ( "h2", $"{item.Key ?? "Requests"}" );

                            foreach (var endpoint in item.OrderBy ( i => i.Path.Length )) {
                                groupSection += new HtmlElement ( "section", section => {
                                    section.ClassList.Add ( "endpoint" );

                                    section += WriteEndpointDescription ( endpoint );
                                } );
                            }
                        } );
                    }

                    main += Footer;
                } );
            } );

            if (!string.IsNullOrEmpty ( Script )) {
                body += new HtmlElement ( "script", RenderableText.Raw ( Script ) );
            }

            body += new HtmlElement ( "script", RenderableText.Raw ( Scripts.DefaultScript ) );
        } );

        return "<!DOCTYPE html>\n" + html.ToString ();
    }

    /// <summary>
    /// Exports the API documentation as HTTP content.
    /// </summary>
    /// <param name="documentation">The API documentation to export.</param>
    /// <returns>The exported API documentation as HTTP content.</returns>
    public HttpContent ExportDocumentationContent ( ApiDocumentation documentation ) {

        var request = HttpContext.GetCurrentContext ()?.Request;
        if (request?.Query [ "embed-target" ].Value is { Length: > 0 } requestingIframe) {

            var uri = new UrlBuilder ( request.Query [ "r" ].GetString () )
                .ClearQuery ()
                .AddQuery ( "embed", "iframe" )
                .AddQuery ( "embed-endpoint", requestingIframe );

            return new StringContent ( $"""
                document.write(`<style>{JavaScriptEncoder.Default.Encode ( Html.Style.EmbedTargetStyles )}</style>`);
                document.write(`<iframe class="sisk-embed-target" src="{uri.Uri.ToString ()}"></iframe>`);
                """, Encoding.UTF8, "text/javascript" );
        }

        return new HtmlContent ( ExportHtml ( documentation ), Encoding.UTF8 );
    }
}
