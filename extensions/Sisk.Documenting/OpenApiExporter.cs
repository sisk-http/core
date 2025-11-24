// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   OpenApiExporter.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Text;
using LightJson;
using Sisk.Core.Routing;

namespace Sisk.Documenting;

/// <summary>
/// Exports API documentation to OpenAPI 3.0 format.
/// </summary>
public sealed class OpenApiExporter : IApiDocumentationExporter {

    /// <summary>
    /// Gets or sets the OpenAPI version. Default is "3.0.0".
    /// </summary>
    public string OpenApiVersion { get; set; } = "3.0.0";

    /// <summary>
    /// Gets or sets the server URLs for the API.
    /// </summary>
    public string [] ServerUrls { get; set; } = Array.Empty<string> ();

    /// <summary>
    /// Gets or sets the contact information for the API.
    /// </summary>
    public OpenApiContact? Contact { get; set; }

    /// <summary>
    /// Gets or sets the license information for the API.
    /// </summary>
    public OpenApiLicense? License { get; set; }

    /// <summary>
    /// Gets or sets the terms of service URL for the API.
    /// </summary>
    public string? TermsOfService { get; set; }

    /// <summary>
    /// Initializes a new instance of the <see cref="OpenApiExporter"/> class.
    /// </summary>
    public OpenApiExporter () {
    }

    /// <summary>
    /// Exports the specified API documentation content to OpenAPI format.
    /// </summary>
    /// <param name="documentation">The API documentation to export.</param>
    /// <returns>An <see cref="HttpContent"/> representing the exported documentation in JSON format.</returns>
    public HttpContent ExportDocumentationContent ( ApiDocumentation documentation ) {
        var openApiDoc = BuildOpenApiDocument ( documentation );
        var options = new JsonOptions { WriteIndented = true };
        var json = openApiDoc.ToString ( options );

        return new StringContent ( json, Encoding.UTF8, "application/json" );
    }

    private JsonObject BuildOpenApiDocument ( ApiDocumentation documentation ) {
        var doc = new JsonObject {
            [ "openapi" ] = OpenApiVersion,
            [ "info" ] = BuildInfo ( documentation ),
            [ "paths" ] = BuildPaths ( documentation )
        };

        if (ServerUrls.Length > 0) {
            var servers = new JsonArray ();
            foreach (var url in ServerUrls) {
                servers.Add ( new JsonObject { [ "url" ] = url } );
            }
            doc [ "servers" ] = servers;
        }

        return doc;
    }

    private JsonObject BuildInfo ( ApiDocumentation documentation ) {
        var info = new JsonObject ();

        if (!string.IsNullOrEmpty ( documentation.ApplicationName )) {
            info [ "title" ] = documentation.ApplicationName;
        }
        else {
            info [ "title" ] = "API Documentation";
        }

        if (!string.IsNullOrEmpty ( documentation.ApplicationDescription )) {
            info [ "description" ] = documentation.ApplicationDescription;
        }

        if (!string.IsNullOrEmpty ( documentation.ApiVersion )) {
            info [ "version" ] = documentation.ApiVersion;
        }
        else {
            info [ "version" ] = "1.0.0";
        }

        if (!string.IsNullOrEmpty ( TermsOfService )) {
            info [ "termsOfService" ] = TermsOfService;
        }

        if (Contact != null) {
            var contact = new JsonObject ();
            if (!string.IsNullOrEmpty ( Contact.Name ))
                contact [ "name" ] = Contact.Name;
            if (!string.IsNullOrEmpty ( Contact.Email ))
                contact [ "email" ] = Contact.Email;
            if (!string.IsNullOrEmpty ( Contact.Url ))
                contact [ "url" ] = Contact.Url;
            if (contact.Count > 0)
                info [ "contact" ] = contact;
        }

        if (License != null) {
            var license = new JsonObject ();
            if (!string.IsNullOrEmpty ( License.Name ))
                license [ "name" ] = License.Name;
            if (!string.IsNullOrEmpty ( License.Url ))
                license [ "url" ] = License.Url;
            if (license.Count > 0)
                info [ "license" ] = license;
        }

        return info;
    }

    private JsonObject BuildPaths ( ApiDocumentation documentation ) {
        var paths = new JsonObject ();

        foreach (var endpoint in documentation.Endpoints) {
            var path = ConvertSiskPathToOpenApi ( endpoint.Path );

            JsonObject pathItem;
            if (paths.ContainsKey ( path )) {
                pathItem = paths [ path ].GetJsonObject ();
            }
            else {
                pathItem = new JsonObject ();
                paths [ path ] = pathItem;
            }

            var methods = GetHttpMethods ( endpoint.RouteMethod );

            foreach (var method in methods) {
                pathItem [ method.ToLowerInvariant () ] = BuildOperation ( endpoint );
            }
        }

        return paths;
    }

    private string ConvertSiskPathToOpenApi ( string siskPath ) {
        // Converte parâmetros de path do formato Sisk para OpenAPI
        // Ex: /users/<id> -> /users/{id}
        return siskPath.Replace ( "<", "{" ).Replace ( ">", "}" );
    }

    private string [] GetHttpMethods ( RouteMethod routeMethod ) {
        var methods = new List<string> ();

        if (routeMethod.HasFlag ( RouteMethod.Get ))
            methods.Add ( "GET" );
        if (routeMethod.HasFlag ( RouteMethod.Post ))
            methods.Add ( "POST" );
        if (routeMethod.HasFlag ( RouteMethod.Put ))
            methods.Add ( "PUT" );
        if (routeMethod.HasFlag ( RouteMethod.Delete ))
            methods.Add ( "DELETE" );
        if (routeMethod.HasFlag ( RouteMethod.Patch ))
            methods.Add ( "PATCH" );
        if (routeMethod.HasFlag ( RouteMethod.Head ))
            methods.Add ( "HEAD" );
        if (routeMethod.HasFlag ( RouteMethod.Options ))
            methods.Add ( "OPTIONS" );

        return methods.ToArray ();
    }

    private JsonObject BuildOperation ( ApiEndpoint endpoint ) {
        var operation = new JsonObject {
            [ "summary" ] = endpoint.Name
        };

        if (!string.IsNullOrEmpty ( endpoint.Description )) {
            operation [ "description" ] = endpoint.Description;
        }

        if (!string.IsNullOrEmpty ( endpoint.Group )) {
            var tags = new JsonArray { endpoint.Group };
            operation [ "tags" ] = tags;
        }

        // Parameters
        var parameters = new JsonArray ();

        // Path parameters
        foreach (var pathParam in endpoint.PathParameters) {
            var parameter = new JsonObject {
                [ "name" ] = pathParam.Name,
                [ "in" ] = "path",
                [ "required" ] = true,
                [ "schema" ] = new JsonObject {
                    [ "type" ] = MapTypeToOpenApi ( pathParam.Type )
                },
                [ "description" ] = pathParam.Description ?? ""
            };
            parameters.Add ( parameter );
        }

        // Query parameters
        foreach (var queryParam in endpoint.QueryParameters) {
            var parameter = new JsonObject {
                [ "name" ] = queryParam.Name,
                [ "in" ] = "query",
                [ "required" ] = queryParam.IsRequired,
                [ "schema" ] = new JsonObject {
                    [ "type" ] = MapTypeToOpenApi ( queryParam.Type )
                },
                [ "description" ] = queryParam.Description ?? ""
            };
            parameters.Add ( parameter );
        }

        // Headers
        foreach (var header in endpoint.Headers) {
            var parameter = new JsonObject {
                [ "name" ] = header.HeaderName,
                [ "in" ] = "header",
                [ "required" ] = header.IsRequired,
                [ "schema" ] = new JsonObject {
                    [ "type" ] = "string"
                },
                [ "description" ] = header.Description ?? ""
            };
            parameters.Add ( parameter );
        }

        if (parameters.Count > 0) {
            operation [ "parameters" ] = parameters;
        }

        // Request body
        if (endpoint.Parameters.Length > 0 || endpoint.RequestExamples.Length > 0) {
            var requestBody = new JsonObject ();
            var content = new JsonObject ();

            // Build schema from Parameters (body parameters)
            if (endpoint.Parameters.Length > 0) {
                var schema = new JsonObject {
                    [ "type" ] = "object",
                    [ "properties" ] = new JsonObject ()
                };

                var properties = schema [ "properties" ].GetJsonObject ();
                var required = new JsonArray ();

                foreach (var param in endpoint.Parameters) {
                    var propertySchema = new JsonObject {
                        [ "type" ] = MapTypeToOpenApi ( param.TypeName )
                    };

                    if (!string.IsNullOrEmpty ( param.Description )) {
                        propertySchema [ "description" ] = param.Description;
                    }

                    properties [ param.Name ] = propertySchema;

                    if (param.IsRequired) {
                        required.Add ( param.Name );
                    }
                }

                if (required.Count > 0) {
                    schema [ "required" ] = required;
                }

                // Default to JSON if no examples specify otherwise
                var defaultMediaType = endpoint.RequestExamples.Length > 0
                    ? DetermineMediaType ( endpoint.RequestExamples [ 0 ].ExampleLanguage )
                    : "application/json";

                content [ defaultMediaType ] = new JsonObject {
                    [ "schema" ] = schema
                };
            }

            // Add examples from RequestExamples
            foreach (var example in endpoint.RequestExamples) {
                var mediaType = DetermineMediaType ( example.ExampleLanguage );

                if (!content.ContainsKey ( mediaType )) {
                    content [ mediaType ] = new JsonObject {
                        [ "schema" ] = new JsonObject {
                            [ "type" ] = "object"
                        }
                    };
                }

                if (!string.IsNullOrEmpty ( example.Example )) {
                    var mediaTypeObj = content [ mediaType ].GetJsonObject ();
                    if (!mediaTypeObj.ContainsKey ( "example" )) {
                        mediaTypeObj [ "example" ] = example.Example;
                    }
                }
            }

            if (content.Count > 0) {
                requestBody [ "content" ] = content;
                operation [ "requestBody" ] = requestBody;
            }
        }

        // Responses
        var responses = new JsonObject ();

        if (endpoint.Responses.Length > 0) {
            foreach (var response in endpoint.Responses) {
                var statusCode = ((int) response.StatusCode).ToString ();
                var responseObj = new JsonObject {
                    [ "description" ] = response.Description ?? GetDefaultStatusDescription ( response.StatusCode )
                };

                if (!string.IsNullOrEmpty ( response.Example )) {
                    var mediaType = DetermineMediaType ( response.ExampleLanguage );
                    var contentObj = new JsonObject {
                        [ mediaType ] = new JsonObject {
                            [ "schema" ] = new JsonObject {
                                [ "type" ] = "object"
                            },
                            [ "example" ] = response.Example
                        }
                    };
                    responseObj [ "content" ] = contentObj;
                }

                responses [ statusCode ] = responseObj;
            }
        }
        else {
            // Default response
            responses [ "200" ] = new JsonObject {
                [ "description" ] = "Successful response"
            };
        }

        operation [ "responses" ] = responses;

        return operation;
    }

    private string MapTypeToOpenApi ( string? typeName ) {
        if (string.IsNullOrEmpty ( typeName )) {
            return "string";
        }

        return typeName.ToLowerInvariant () switch {
            "int" or "int32" or "integer" => "integer",
            "long" or "int64" => "integer",
            "float" or "single" => "number",
            "double" or "decimal" => "number",
            "bool" or "boolean" => "boolean",
            "string" => "string",
            "datetime" or "date" => "string",
            "guid" or "uuid" => "string",
            _ => "string"
        };
    }

    private string DetermineMediaType ( string? exampleLanguage ) {
        if (string.IsNullOrEmpty ( exampleLanguage )) {
            return "application/json";
        }

        return exampleLanguage.ToLowerInvariant () switch {
            "json" => "application/json",
            "xml" => "application/xml",
            "html" => "text/html",
            "text" or "plain" => "text/plain",
            _ => "application/json"
        };
    }

    private string GetDefaultStatusDescription ( HttpStatusCode statusCode ) {
        return statusCode switch {
            HttpStatusCode.OK => "Successful response",
            HttpStatusCode.Created => "Resource created",
            HttpStatusCode.Accepted => "Request accepted",
            HttpStatusCode.NoContent => "No content",
            HttpStatusCode.BadRequest => "Bad request",
            HttpStatusCode.Unauthorized => "Unauthorized",
            HttpStatusCode.Forbidden => "Forbidden",
            HttpStatusCode.NotFound => "Resource not found",
            HttpStatusCode.MethodNotAllowed => "Method not allowed",
            HttpStatusCode.Conflict => "Conflict",
            HttpStatusCode.InternalServerError => "Internal server error",
            HttpStatusCode.NotImplemented => "Not implemented",
            HttpStatusCode.BadGateway => "Bad gateway",
            HttpStatusCode.ServiceUnavailable => "Service unavailable",
            _ => statusCode.ToString ()
        };
    }
}

/// <summary>
/// Represents contact information for the OpenAPI documentation.
/// </summary>
public sealed class OpenApiContact {

    /// <summary>
    /// Gets or sets the name of the contact person/organization.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the email address of the contact person/organization.
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Gets or sets the URL pointing to the contact information.
    /// </summary>
    public string? Url { get; set; }
}

/// <summary>
/// Represents license information for the OpenAPI documentation.
/// </summary>
public sealed class OpenApiLicense {

    /// <summary>
    /// Gets or sets the name of the license.
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Gets or sets the URL to the license.
    /// </summary>
    public string? Url { get; set; }
}
