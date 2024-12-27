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
    /// Gets or sets the CSS styles used in the HTML export.
    /// </summary>
    public string? Style { get; set; } = """
        * { box-sizing: border-box; }
        html, body { margin: 0; background-color: #f4f9ff; font-family: Arial }
        p, li { line-height: 1.6 }
        li + li { margin-top: 1em; }
        .monospaced { font-family: monospace; }

        main {
            background: white;
            max-width: 800px;
            margin: 30px auto 0 auto;
            padding: 20px 40px;
            border-radius: 14px;
            border: 1px solid #cbd3da;
        }

        h1 {
            padding-bottom: .25em;
            border-bottom: 2px solid #aacae7;
            font-size: 1.8em;
            font-weight: medium;
        }

        h2 {
            padding: 1em 0 .25em 0;
            border-bottom: 1px solid #aacae7;
            font-size: 1.4em;
            font-weight: normal;
        }

        .details > span {
            display: inline-block;
            background-color: #e3e8ed;
            padding: 1px 6px;
            font-size: 0.9em;
            font-weight: bold;
        }

        .details > span:not(:first-child) {
            margin-left: 5px; 
        }
        """;

    /// <summary>
    /// Encodes the specified <see cref="JsonRpcDocumentation"/> into a HTML string.
    /// </summary>
    /// <param name="documentation">The <see cref="JsonRpcDocumentation"/> instance.</param>
    protected string EncodeDocumentationHtml ( JsonRpcDocumentation documentation ) {
        HtmlElement html = new HtmlElement ( "html" );

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
                foreach (var category in methodsGrouped) {
                    main += new HtmlElement ( "h1", category.Key ?? "Methods" );
                    foreach (var method in category) {
                        main += new HtmlElement ( "section", section => {
                            section += new HtmlElement ( "h2", method.MethodName );
                            section += new HtmlElement ( "p", method.Description ?? "" );
                            section += new HtmlElement ( "p", $"Returns: {method.ReturnType.FullName}" );
                            section += new HtmlElement ( "ul", ulParams => {
                                foreach (var param in method.Parameters) {
                                    ulParams += new HtmlElement ( "li", li => {
                                        li += new HtmlElement ( "b", param.ParameterName )
                                            .WithClass ( "monospaced" );
                                        li += new HtmlElement ( "div", param.Description ?? "(no description)" );
                                        li += new HtmlElement ( "div", paramDetails => {
                                            paramDetails.WithClass ( "details" );
                                            paramDetails += new HtmlElement ( "span", param.IsOptional ? "Optional" : "Required" );
                                            paramDetails += new HtmlElement ( "span", param.ParameterType.FullName );
                                        } );
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
