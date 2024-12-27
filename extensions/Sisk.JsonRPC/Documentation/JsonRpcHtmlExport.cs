// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcHtmlExport.cs
// Repository:  https://github.com/sisk-http/core

using System.Text;
using TinyComponents;

namespace Sisk.JsonRPC.Documentation;

/// <summary>
/// Provides an HTML-based <see cref="IJsonRpcDocumentationExporter"/>.
/// </summary>
public class JsonRpcHtmlExport : IJsonRpcDocumentationExporter {

    /// <summary>
    /// Gets or sets an boolean indicating if an summary should be exported in the HTML.
    /// </summary>
    public bool ExportSummary { get; set; } = true;

    /// <summary>
    /// Gets or sets an optional object to append to the header of the
    /// exported HTML.
    /// </summary>
    public object? Header { get; set; }

    /// <summary>
    /// Gets or sets the CSS styles used in the HTML export.
    /// </summary>
    public string? Style { get; set; } = """
        * { box-sizing: border-box; }        
        p, li { line-height: 1.6 }

        html, body { 
            margin: 0;
            background-color: white;
            font-size: 16px;
            font-family: -apple-system,BlinkMacSystemFont,"Segoe UI","Noto Sans",Helvetica,Arial,sans-serif,"Apple Color Emoji","Segoe UI Emoji"
        }

        main {
            background: white;
            max-width: 900px;
            width: 90vw;
            margin: 30px auto 0 auto;
            padding: 20px 40px;
            border-radius: 14px;
            border: 1px solid #d1d9e0;
        }

        h1, h2, h3 {
            margin-top: 2.5rem;
            margin-bottom: 1rem;
            font-weight: 600;
            line-height: 1.25;
        }

        h1, h2 {
            padding-bottom: .3em;
            border-bottom: 2px solid #d1d9e0b3;
        }

        h1 {                    
            font-size: 2em;                
        }

        h2 {
            font-size: 1.5em;
        }

        h1 a,
        h2 a {
            opacity: 0;   
            color: #000;
            text-decoration: none !important;
            user-select: none;
        }

        h1:hover a,
        h2:hover a {
            opacity: 0.3;
        }

        h1 a:hover,
        h2 a:hover {
            opacity: 1;
        }

        .paramlist {
            padding-left: 0;
            list-style-type: none;
        }

        .paramtitle {
            display: flex;
            gap: 15px;
            margin-bottom: 5px;
        }

        .paramtitle b {
            padding: .2em .4em;
            margin: 0;
            font-family: ui-monospace,SFMono-Regular,SF Mono,Menlo,Consolas,Liberation Mono,monospace;
            font-size: 14px;
            font-weight: 600;
            white-space: break-spaces;
            background-color: #eff1f3;
            border-radius: 6px;
        }

        .paramlist li + li {
            margin-top: 1em;
            border-top: 1px solid #d8dee4;
            padding-top: 1.25em;
        }

        .muted {
            color: #656d76;
        }

        .at {
            color: #9a6700;
        }

        a {
            color: #0969da;
            text-decoration: none;
        }

        a:hover {
            text-decoration: underline;
        }
        """;

    /// <summary>
    /// Encodes the specified <see cref="JsonRpcDocumentation"/> into a HTML string.
    /// </summary>
    /// <param name="documentation">The <see cref="JsonRpcDocumentation"/> instance.</param>
    protected string EncodeDocumentationHtml ( JsonRpcDocumentation documentation ) {
        HtmlElement html = new HtmlElement ( "html" );

        string GetMethodIdName ( string name ) {
            return new string ( name.Where ( char.IsLetterOrDigit ).ToArray () );
        }

        html += new HtmlElement ( "head", head => {
            head += new HtmlElement ( "title", "JSON-RPC 2.0 Application Documentation" );
            if (this.Style != null) {
                head += new HtmlElement ( "style", RenderableText.Raw ( this.Style ) );
            }
        } );

        html += new HtmlElement ( "body", body => {

            var methodsGrouped = documentation.Methods
                .GroupBy ( g => g.Category );

            body += new HtmlElement ( "main", main => {

                if (this.Header is not null) {
                    main += this.Header;
                }

                if (this.ExportSummary) {
                    main += new HtmlElement ( "h1", "Summary" );

                    foreach (var category in methodsGrouped) {
                        main += new HtmlElement ( "p", (category.Key ?? "Methods") + ":" );
                        main += new HtmlElement ( "ul", ul => {
                            foreach (var method in documentation.Methods) {
                                ul += new HtmlElement ( "li", li => {
                                    li += new HtmlElement ( "a", method.MethodName )
                                        .WithAttribute ( "href", $"#{GetMethodIdName ( method.MethodName )}" );
                                    if (!string.IsNullOrEmpty ( method.Description )) {
                                        li += new HtmlElement ( "span", $" - {method.Description}" )
                                            .WithClass ( "muted" );
                                    }
                                } );
                            }
                        } );
                    }
                }

                foreach (var category in methodsGrouped) {
                    main += new HtmlElement ( "h1", category.Key ?? "Methods" );
                    foreach (var method in category) {
                        main += new HtmlElement ( "section", section => {

                            section.Id = GetMethodIdName ( method.MethodName );

                            section += new HtmlElement ( "h2", h2 => {
                                h2 += method.MethodName;
                                h2 += new HtmlElement ( "a", "🔗" )
                                    .WithAttribute ( "href", $"#{section.Id}" );
                            } );
                            section += new HtmlElement ( "p", method.Description ?? "" );
                            section += new HtmlElement ( "p", $"Returns: {method.ReturnType.FullName}" );
                            section += new HtmlElement ( "ul", ulParams => {
                                ulParams.ClassList.Add ( "paramlist" );
                                foreach (var param in method.Parameters) {
                                    ulParams += new HtmlElement ( "li", li => {
                                        li += new HtmlElement ( "div", div => {
                                            div.ClassList.Add ( "paramtitle" );

                                            div += new HtmlElement ( "b", param.ParameterName );

                                            div += new HtmlElement ( "span", param.ParameterType.FullName )
                                                    .WithClass ( "muted" );

                                            if (!param.IsOptional)
                                                div += new HtmlElement ( "span", "Required" ).WithClass ( "at" );

                                        } );
                                        li += new HtmlElement ( "div", param.Description ?? "" );
                                    } );
                                }
                            } );
                        } );
                    }
                }
            } );
        } );

        return html.ToString ();
    }

    /// <inheritdoc/>
    public byte [] ExportDocumentBytes ( JsonRpcDocumentation documentation ) {
        return Encoding.UTF8.GetBytes ( this.EncodeDocumentationHtml ( documentation ) );
    }
}
