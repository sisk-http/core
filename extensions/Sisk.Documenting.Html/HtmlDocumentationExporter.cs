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
using Sisk.Documenting.Exporters;
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

    // arrow-left-line from Remix Icon
    const string BackArrowIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M7.82843 10.9999H20V12.9999H7.82843L13.1924 18.3638L11.7782 19.778L4 11.9999L11.7782 4.22168L13.1924 5.63589L7.82843 10.9999Z"></path></svg>
            """;

    // computer-line from Remix Icon
    const string SystemThemeIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M4 16H20V5H4V16ZM13 18V20H17V22H7V20H11V18H2.9918C2.44405 18 2 17.5511 2 16.9925V4.00748C2 3.45107 2.45531 3 2.9918 3H21.0082C21.556 3 22 3.44892 22 4.00748V16.9925C22 17.5489 21.5447 18 21.0082 18H13Z"></path></svg>
            """;

    // sun-line from Remix Icon
    const string LightThemeIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M12 18C8.68629 18 6 15.3137 6 12C6 8.68629 8.68629 6 12 6C15.3137 6 18 8.68629 18 12C18 15.3137 15.3137 18 12 18ZM12 16C14.2091 16 16 14.2091 16 12C16 9.79086 14.2091 8 12 8C9.79086 8 8 9.79086 8 12C8 14.2091 9.79086 16 12 16ZM11 1H13V4H11V1ZM11 20H13V23H11V20ZM3.51472 4.92893L4.92893 3.51472L7.05025 5.63604L5.63604 7.05025L3.51472 4.92893ZM16.9497 18.364L18.364 16.9497L20.4853 19.0711L19.0711 20.4853L16.9497 18.364ZM19.0711 3.51472L20.4853 4.92893L18.364 7.05025L16.9497 5.63604L19.0711 3.51472ZM5.63604 16.9497L7.05025 18.364L4.92893 20.4853L3.51472 19.0711L5.63604 16.9497ZM23 11V13H20V11H23ZM4 11V13H1V11H4Z"></path></svg>
            """;

    // moon-line from Remix Icon
    const string DarkThemeIcon = """
            <svg xmlns="http://www.w3.org/2000/svg" viewBox="0 0 24 24" fill="currentColor"><path d="M10 7C10 10.866 13.134 14 17 14C18.9584 14 20.729 13.1957 21.9995 11.8995C22 11.933 22 11.9665 22 12C22 17.5228 17.5228 22 12 22C6.47715 22 2 17.5228 2 12C2 6.47715 6.47715 2 12 2C12.0335 2 12.067 2 12.1005 2.00049C10.8043 3.27098 10 5.04157 10 7ZM4 12C4 16.4183 7.58172 20 12 20C15.0583 20 17.7158 18.2839 19.062 15.7621C18.3945 15.9187 17.7035 16 17 16C12.0294 16 8 11.9706 8 7C8 6.29648 8.08133 5.60547 8.2379 4.938C5.71611 6.28423 4 8.9417 4 12Z"></path></svg>
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
    /// Gets or sets the heading displayed above the situational examples for a request parameter.
    /// </summary>
    public string FormatParameterExamples { get; set; } = "Examples:";

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
    /// Gets or sets the format string for the link that returns from an endpoint page to the endpoint listing.
    /// </summary>
    public string FormatBackToEndpoints { get; set; } = "All endpoints";

    /// <summary>
    /// Gets or sets the label of the button that expands a long request or response example description.
    /// </summary>
    public string FormatShowMore { get; set; } = "Show more";

    /// <summary>
    /// Gets or sets the label of the button that collapses an expanded request or response example description.
    /// </summary>
    public string FormatShowLess { get; set; } = "Show less";

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

                header += new HtmlElement ( "div", title => {
                    title.ClassList.Add ( "sidebar-title" );

                    title += new HtmlElement ( "div", name => {
                        name += new HtmlElement ( "h1", documentation.ApplicationName ?? "API" );
                        name += new HtmlElement ( "span", $"v{documentation.ApiVersion ?? "1.0"}" )
                            .WithClass ( "version" );
                    } );

                    title += WriteThemeSwitcher ();
                } );

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
    /// Writes the color theme switcher, which lets the reader choose between the system, light and dark themes.
    /// </summary>
    /// <returns>The HTML element representing the theme switcher.</returns>
    protected virtual HtmlElement? WriteThemeSwitcher () {
        return new HtmlElement ( "div", switcher => {
            switcher.ClassList.Add ( "theme-switcher" );
            switcher.Attributes [ "role" ] = "group";
            switcher.Attributes [ "aria-label" ] = "Color theme";

            foreach (var (value, label, icon) in new [] {
                ("system", "System theme", SystemThemeIcon),
                ("light", "Light theme", LightThemeIcon),
                ("dark", "Dark theme", DarkThemeIcon) }) {

                switcher += new HtmlElement ( "button", btn => {
                    btn.Attributes [ "type" ] = "button";
                    btn.Attributes [ "data-theme-value" ] = value;
                    btn.Attributes [ "aria-label" ] = label;
                    btn.Attributes [ "title" ] = label;
                    btn += RenderableText.Raw ( icon );
                } );
            }
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

                var mainColumn = new HtmlElement ( "div" ).WithClass ( "endpoint-main" );
                var examplesColumn = new HtmlElement ( "aside" ).WithClass ( "endpoint-examples" );

                content += new HtmlElement ( "div", breadcrumb => {
                    breadcrumb.ClassList.Add ( "endpoint-breadcrumb" );

                    breadcrumb += new HtmlElement ( "a", back => {
                        back.ClassList.Add ( "endpoint-back" );
                        back.Attributes [ "href" ] = "#";
                        back += RenderableText.Raw ( BackArrowIcon );
                        back += new HtmlElement ( "span", FormatBackToEndpoints );
                    } );

                    breadcrumb += new HtmlElement ( "span", endpoint.Group ?? "Requests" );
                } );

                content += new HtmlElement ( "div", header => {
                    header.ClassList.Add ( "endpoint-content-header" );

                    header += new HtmlElement ( "h3", endpoint.Name );

                    header += new HtmlElement ( "div", copyWrapper => {
                        copyWrapper.ClassList.Add ( "copy-button-wrapper" );

                        var request = HttpContext.GetCurrentContext ()?.Request;
                        if (request?.Query [ "embed-target" ].Value is { Length: > 0 }) {
                            copyWrapper += new HtmlElement ( "a", btn => {
                                btn.ClassList.Add ( "open-ext-button" );
                                btn.Attributes [ "href" ] = $"{GetDocumentationUrl ( request )}#{TransformId ( endpoint.Name )}";
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

                mainColumn += CreateParagraphs ( endpoint.Description );

                mainColumn += new HtmlElement ( "div", path => {
                    path.ClassList.Add ( "endpoint-path" );

                    path += new HtmlElement ( "span", endpoint.RouteMethod.ToString ().ToUpper () )
                        .WithClass ( "method-badge" )
                        .WithStyle ( new { backgroundColor = GetRouteMethodHexColor ( endpoint.RouteMethod ) + "35" } );
                    path += new HtmlElement ( "code", endpoint.Path );
                } );

                if (endpoint.Headers.Length > 0) {
                    mainColumn += new HtmlElement ( "div", div => {
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
                    mainColumn += new HtmlElement ( "div", div => {
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
                    mainColumn += new HtmlElement ( "div", div => {
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
                    mainColumn += new HtmlElement ( "div", div => {
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

                                    var parameterExamples = endpoint.ParameterExamples
                                        .Where ( example => string.Equals ( example.ParameterName, param.Name, StringComparison.Ordinal ) );
                                    if (parameterExamples.Any ()) {
                                        li += new HtmlElement ( "div", examplesDiv => {
                                            examplesDiv.ClassList.Add ( "parameter-examples" );
                                            examplesDiv += new HtmlElement ( "p", FormatParameterExamples )
                                                .WithClass ( "parameter-examples-title" );

                                            foreach (var example in parameterExamples) {
                                                examplesDiv += new HtmlElement ( "details", exampleDetails => {
                                                    exampleDetails.ClassList.Add ( "parameter-example" );
                                                    exampleDetails += new HtmlElement ( "summary", example.Title );
                                                    exampleDetails += new HtmlElement ( "div", exampleBody => {
                                                        exampleBody.ClassList.Add ( "parameter-example-body" );
                                                        if (example.Description is { Length: > 0 }) {
                                                            exampleBody += new HtmlElement ( "div", CreateParagraphs ( example.Description ) )
                                                                .WithClass ( "parameter-example-description" );
                                                        }
                                                        if (example.Example is { } exampleValue) {
                                                            exampleBody += CreateCodeBlock ( exampleValue, example.ExampleLanguage );
                                                        }
                                                    } );
                                                } );
                                            }
                                        } );
                                    }
                                } );
                            }
                        } );
                    } );
                }

                if (endpoint.RequestExamples.Length > 0) {
                    examplesColumn += CreateExampleCard (
                        FormatEndpointRequestExamples.TrimEnd ( ':' ),
                        $"req-{TransformId ( endpoint.Name )}",
                        showTabs: false,
                        endpoint.RequestExamples
                            .Select ( ( req, index ) => (
                                Group: "request",
                                GroupClass: (string?) null,
                                Title: GetExampleTitle ( req.Description, $"{FormatTabExample} {index + 1}" ),
                                Description: (string?) req.Description,
                                Content: CreateExampleSchemaTabControl (
                                    req.Example,
                                    req.ExampleLanguage,
                                    req.JsonSchema,
                                    $"req-{TransformId ( endpoint.Name )}-{index}" ) ) )
                            .ToArray () );
                }

                if (endpoint.Responses.Length > 0) {
                    examplesColumn += CreateExampleCard (
                        FormatEndpointResponses.TrimEnd ( ':' ),
                        $"res-{TransformId ( endpoint.Name )}",
                        showTabs: true,
                        endpoint.Responses
                            .Select ( ( res, index ) => (
                                Group: ((int) res.StatusCode).ToString (),
                                GroupClass: (string?) ((int) res.StatusCode switch {
                                    >= 200 and < 300 => "success",
                                    >= 300 and < 400 => "redirect",
                                    _ => "error"
                                }),
                                Title: GetExampleTitle ( res.Description, $"{FormatTabExample} {index + 1}" ),
                                res.Description,
                                Content: CreateExampleSchemaTabControl (
                                    res.Example,
                                    res.ExampleLanguage,
                                    res.JsonSchema,
                                    $"res-{TransformId ( endpoint.Name )}-{index}" ) ) )
                            .ToArray () );
                }

                bool hasExamples = endpoint.RequestExamples.Length > 0 || endpoint.Responses.Length > 0;

                content += new HtmlElement ( "div", layout => {
                    layout.ClassList.Add ( "endpoint-layout" );
                    layout += mainColumn;

                    if (hasExamples) {
                        layout.ClassList.Add ( "has-examples" );
                        layout += examplesColumn;
                    }
                } );
            } );
        } );
    }

    HtmlElement CreateExampleCard ( string title, string tabId, bool showTabs,
        (string Group, string? GroupClass, string Title, string? Description, HtmlElement? Content) [] items ) {

        var groups = items.GroupBy ( item => item.Group ).ToArray ();

        return new HtmlElement ( "div", card => {
            card.ClassList.Add ( "tab-control" );
            card.ClassList.Add ( "example-card" );
            card.Attributes [ "data-tab-id" ] = tabId;

            card += new HtmlElement ( "div", header => {
                header.ClassList.Add ( "tab-header" );
                header.ClassList.Add ( "example-card-header" );

                header += new HtmlElement ( "span", title ).WithClass ( "example-card-title" );

                for (int i = 0; showTabs && i < groups.Length; i++) {
                    var group = groups [ i ];
                    var isFirst = i == 0;

                    header += new HtmlElement ( "button", btn => {
                        btn.ClassList.Add ( "tab-button" );
                        if (group.First ().GroupClass is { } groupClass)
                            btn.ClassList.Add ( groupClass );
                        if (isFirst)
                            btn.ClassList.Add ( "active" );

                        btn.Attributes [ "type" ] = "button";
                        btn.Attributes [ "data-tab" ] = group.Key;
                        btn += group.Key;
                    } );
                }
            } );

            for (int i = 0; i < groups.Length; i++) {
                var variants = groups [ i ].ToArray ();
                var groupKey = groups [ i ].Key;
                var isFirst = i == 0;

                card += new HtmlElement ( "div", panel => {
                    panel.ClassList.Add ( "tab-content" );
                    if (isFirst)
                        panel.ClassList.Add ( "active" );

                    panel.Attributes [ "data-tab-content" ] = groupKey;
                    panel.Attributes [ "data-label" ] = showTabs ? groupKey : null;

                    if (variants.Length > 1) {
                        panel += new HtmlElement ( "select", select => {
                            select.ClassList.Add ( "example-variant-select" );
                            select.Attributes [ "aria-label" ] = title;

                            for (int v = 0; v < variants.Length; v++) {
                                select += new HtmlElement ( "option", variants [ v ].Title )
                                    .WithAttribute ( "value", v.ToString () );
                            }
                        } );
                    }

                    for (int v = 0; v < variants.Length; v++) {
                        var variant = variants [ v ];
                        var variantIndex = v;

                        panel += new HtmlElement ( "div", variantPanel => {
                            variantPanel.ClassList.Add ( "example-variant" );
                            if (variantIndex == 0)
                                variantPanel.ClassList.Add ( "active" );

                            variantPanel.Attributes [ "data-variant" ] = variantIndex.ToString ();
                            variantPanel.Attributes [ "data-title" ] = variants.Length > 1 ? variant.Title : null;

                            if (variant.Description is { Length: > 0 }) {
                                variantPanel += new HtmlElement ( "div", description => {
                                    description.ClassList.Add ( "example-description" );
                                    description += new HtmlElement ( "div", CreateParagraphs ( variant.Description ) )
                                        .WithClass ( "example-description-text" );
                                    description += new HtmlElement ( "button", FormatShowMore )
                                        .WithClass ( "example-description-toggle" )
                                        .WithAttribute ( "type", "button" )
                                        .WithAttribute ( "data-less", FormatShowLess );
                                } );
                            }

                            variantPanel += variant.Content;
                        } );
                    }
                } );
            }
        } );
    }

    static string GetExampleTitle ( string? description, string fallback ) {
        if (string.IsNullOrWhiteSpace ( description ))
            return fallback;

        var firstLine = description.Trim ().Split ( '\n' ) [ 0 ];
        var title = firstLine.IndexOf ( ". ", StringComparison.Ordinal ) switch {
            > 0 and var sentenceEnd => firstLine [ ..sentenceEnd ],
            _ => firstLine
        };

        title = title.Replace ( "`", "" ).Replace ( "*", "" ).Trim ().TrimEnd ( '.', ':' );
        return title.Length > 70 ? title [ ..67 ].TrimEnd () + "..." : title;
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
                cblock += new RenderableText ( code );
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

        return RenderableText.Raw ( CommonMark.CommonMarkConverter.Convert ( text ) );
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
            RouteMethod.Query => "#087ea4",
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
            head += new HtmlElement ( "script", RenderableText.Raw ( Scripts.ThemeScript ) );
            head += new HtmlElement ( "style", RenderableText.Raw ( Style ) );

            if (Head != null) {
                head += Head;
            }
        } );

        html += new HtmlElement ( "body", body => {

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

                    if (!IncludeSidebar) {
                        main += WriteThemeSwitcher ();
                    }

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

            body += new HtmlElement ( "script",
                RenderableText.Raw ( $"({Scripts.DefaultScript})(document, location.href.split('#')[0]);" ) );
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
        if (request?.Query [ "embed-target" ].Value is { Length: > 0 } requestingEndpoint) {

            var endpoint = documentation.Endpoints.FirstOrDefault ( e =>
                string.Equals ( e.Name, requestingEndpoint, StringComparison.OrdinalIgnoreCase ) );

            if (endpoint is null) {
                return new StringContent ( "console.error('Sisk documentation: embedded endpoint not found.');",
                    Encoding.UTF8, "text/javascript" );
            }

            var container = new HtmlElement ( "div", container => {
                container.ClassList.Add ( "sisk-embed" );
                container.Attributes [ "part" ] = "container";

                container += new HtmlElement ( "section", section => {
                    section.ClassList.Add ( "endpoint" );

                    if (WriteEndpointDescription ( endpoint ) is { } endpointElement) {
                        endpointElement.Attributes [ "open" ] = "open";
                        section += endpointElement;
                    }
                } );
            } );

            var shadowStyle = Style
                .Replace ( ":root:not([data-theme=\"light\"])", ":host(:not([data-theme=\"light\"]))" )
                .Replace ( ":root[data-theme=\"dark\"]", ":host([data-theme=\"dark\"])" )
                .Replace ( ":root", ":host" );

            var shadowHtml = $"<style>{shadowStyle}\n{Html.Style.EmbedStyles}</style>{container}";

            return new StringContent ( $$"""
                (() => {
                    const script = document.currentScript;
                    const host = document.createElement('div');
                    host.className = 'sisk-embed-target';
                    if (script.dataset.theme) host.dataset.theme = script.dataset.theme;
                    script.parentNode.insertBefore(host, script);

                    const root = host.attachShadow({ mode: 'open' });
                    root.innerHTML = "{{JavaScriptEncoder.Default.Encode ( shadowHtml )}}";

                    // Prism.js highlights the whole host document and replaces window.Prism when loaded,
                    // so it is forced into manual mode and the host page global is restored afterwards.
                    const hostPrism = window.Prism;
                    window.Prism = { manual: true };
                    {{Script}}
                    const prism = window.Prism;
                    window.Prism = hostPrism;
                    if (prism.highlightAllUnder) prism.highlightAllUnder(root);

                    ({{Scripts.DefaultScript}})(root, "{{JavaScriptEncoder.Default.Encode ( GetDocumentationUrl ( request ) )}}");
                })();
                """, Encoding.UTF8, "text/javascript" );
        }

        return new HtmlContent ( ExportHtml ( documentation ), Encoding.UTF8 );
    }

    static string GetDocumentationUrl ( HttpRequest request ) =>
        Uri.TryCreate ( request.Query [ "r" ].Value, UriKind.Absolute, out var referer ) && referer is { Scheme: "http" or "https" }
            ? referer.GetLeftPart ( UriPartial.Path )
            : request.Uri.GetLeftPart ( UriPartial.Path );
}
