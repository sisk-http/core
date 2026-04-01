// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LlmsApiExporter.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Text;
using Sisk.Core.Helpers;
using Sisk.Core.Http;

namespace Sisk.Documenting;

/// <summary>
/// Exports API documentation in a simple markdown format that can be easily rendered by LLMs or other tools.
/// </summary>
public sealed class LlmsApiExporter : IApiDocumentationExporter {

    static string TransformId ( string unsafeId ) {
        return new string ( unsafeId.Where ( c => char.IsLetterOrDigit ( c ) || c == '_' || c == '-' ).ToArray () );
    }

    [return: NotNullIfNotNull ( nameof ( description ) )]
    static string? TruncateDescription ( string? description, int maxLength = 100 ) {
        if (description == null) {
            return null;
        }
        if (description.Length <= maxLength) {
            return description;
        }
        return description.Substring ( 0, maxLength ) + "...";
    }

    HttpContent PrepareEndpointListing ( ApiDocumentation documentation ) {
        StringBuilder sb = new StringBuilder ();

        sb.AppendLine ( $"# {documentation.ApplicationName ?? "API documentation"}" );
        sb.AppendLine ( $"{documentation.ApplicationDescription ?? "(no description available)"}" );
        sb.AppendLine ( $"\nAPI Version: {documentation.ApiVersion ?? "1.0"}" );
        sb.AppendLine ();

        var currentRequest = HttpContext.GetCurrentContext ()?.Request;
        var currentUrl = currentRequest?.FullUrl ?? "/";

        if (currentRequest?.Query [ "endpoint" ].MaybeNullOrEmpty () is { } requestedEndpoint) {

            var endpoint = documentation.Endpoints.FirstOrDefault ( e => TransformId ( e.Name ) == requestedEndpoint );
            if (endpoint != null) {
                sb.AppendLine ( $"# {endpoint.Name}" );
                sb.AppendLine ( $"{endpoint.Description ?? "(no description available)"}" );
                sb.AppendLine ();
                sb.AppendLine ( $"- Method: {endpoint.RouteMethod}" );
                sb.AppendLine ( $"- Path: {endpoint.Path}" );
                sb.AppendLine ();
                if (endpoint.Headers.Any ()) {
                    sb.AppendLine ( "## Headers" );
                    foreach (var _h in endpoint.Headers) {
                        sb.Append ( $"- {_h.HeaderName}" );
                        if (_h.IsRequired) {
                            sb.Append ( " (required)" );
                        }
                        sb.Append ( ": " );
                        if (_h.Description != null) {
                            sb.Append ( _h.Description.ReplaceLineEndings ( " " ) );
                        }
                        else {
                            sb.Append ( "(no description available)" );
                        }
                        sb.AppendLine ();
                    }
                }
                if (endpoint.PathParameters.Any ()) {
                    sb.AppendLine ( "\n## Path Parameters" );
                    foreach (var _p in endpoint.PathParameters) {
                        sb.Append ( $"- {_p.Name}" );
                        if (_p.Type != null) {
                            sb.Append ( $" [{_p.Type}]" );
                        }
                        sb.Append ( ": " );
                        if (_p.Description != null) {
                            sb.Append ( _p.Description.ReplaceLineEndings ( " " ) );
                        }
                        else {
                            sb.Append ( "(no description available)" );
                        }
                        sb.AppendLine ();
                    }
                }
                if (endpoint.QueryParameters.Any ()) {
                    sb.AppendLine ( "\n## Query Parameters" );
                    foreach (var _q in endpoint.QueryParameters) {
                        sb.Append ( $"- {_q.Name}" );
                        if (_q.Type != null) {
                            sb.Append ( $" [{_q.Type}]" );
                        }
                        if (_q.IsRequired) {
                            sb.Append ( " (required)" );
                        }
                        sb.Append ( ": " );
                        if (_q.Description != null) {
                            sb.Append ( _q.Description.ReplaceLineEndings ( " " ) );
                        }
                        else {
                            sb.Append ( "(no description available)" );
                        }
                        sb.AppendLine ();
                    }
                }
                if (endpoint.Parameters.Any ()) {
                    sb.AppendLine ( "\n## Body Parameters" );
                    foreach (var _b in endpoint.Parameters) {
                        sb.Append ( $"- {_b.Name}" );
                        if (_b.TypeName != null) {
                            sb.Append ( $" [{_b.TypeName}]" );
                        }
                        if (_b.IsRequired) {
                            sb.Append ( " (required)" );
                        }
                        sb.Append ( ": " );
                        if (_b.Description != null) {
                            sb.Append ( _b.Description.ReplaceLineEndings ( " " ) );
                        }
                        else {
                            sb.Append ( "(no description available)" );
                        }
                        sb.AppendLine ();
                    }
                }
                if (endpoint.RequestExamples.Any ()) {
                    sb.AppendLine ( "## Request Examples" );
                    foreach (var _e in endpoint.RequestExamples) {
                        sb.AppendLine ( $"- {_e.Description}" );
                        if (_e.Example != null) {
                            sb.AppendLine ( $"### Example request body" );
                            sb.AppendLine ( $"```{_e.ExampleLanguage}" );
                            sb.AppendLine ( _e.Example );
                            sb.AppendLine ( "```" );
                            sb.AppendLine ();
                        }
                        if (_e.JsonSchema != null) {
                            sb.AppendLine ( "### JSON Schema" );
                            sb.AppendLine ( $"```json" );
                            sb.AppendLine ( _e.JsonSchema );
                            sb.AppendLine ( "```" );
                            sb.AppendLine ();
                        }
                    }
                }
                if (endpoint.Responses.Any ()) {
                    sb.AppendLine ( "\n## Responses" );
                    foreach (var _r in endpoint.Responses) {
                        sb.Append ( $"- {_r.StatusCode}: {_r.Description}" );

                        if (_r.Example != null) {
                            sb.AppendLine ( $"### Example return body" );
                            sb.AppendLine ( $"```{_r.ExampleLanguage}" );
                            sb.AppendLine ( _r.Example );
                            sb.AppendLine ( "```" );
                            sb.AppendLine ();
                        }
                        if (_r.JsonSchema != null) {
                            sb.AppendLine ( "### JSON Schema" );
                            sb.AppendLine ( $"```json" );
                            sb.AppendLine ( _r.JsonSchema );
                            sb.AppendLine ( "```" );
                            sb.AppendLine ();
                        }

                        sb.AppendLine ();
                    }
                }

                return new StringContent ( sb.ToString () );
            }
        }

        var groups = documentation.Endpoints.DistinctBy ( e => $"{e.RouteMethod} {e.Name}" ).GroupBy ( e => e.Group );
        foreach (var group in groups) {

            sb.AppendLine ( $"## {group.Key ?? "Ungrouped endpoints"}" );
            foreach (var endpoint in group.OrderBy ( i => i.Order )) {

                var endpointId = TransformId ( $"{endpoint.Name}" );
                var endpointPath = new UrlBuilder ( currentUrl )
                    .ClearQuery ()
                    .SetQuery ( "endpoint", endpointId );

                if (currentRequest?.Headers.Origin is { } _origin)
                    endpointPath.SetAuthority ( _origin );
                else if (currentRequest?.Host is { } _host)
                    endpointPath.SetAuthority ( _host );

                sb.AppendLine ( $"- [{endpoint.Name}]({endpointPath}): {TruncateDescription ( endpoint.Description ) ?? "(no description available)"}" );
            }
            sb.AppendLine ();
        }

        return new StringContent ( sb.ToString () );
    }

    /// <inheritdoc/>
    public HttpContent ExportDocumentationContent ( ApiDocumentation documentation ) {
        return PrepareEndpointListing ( documentation );
    }
}
