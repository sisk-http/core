---
applyTo: '**/Sisk.Documenting*/**/*.cs'
---

# Sisk.Documenting + Sisk.Documenting.Html

`Sisk.Documenting` generates API documentation automatically from your routes and annotation attributes. It supports OpenAPI 3.0 export out of the box, and `Sisk.Documenting.Html` adds a self-contained interactive HTML exporter.

> **Status: Not yet published on NuGet.** Reference the project directly from source: `https://github.com/sisk-http/core/tree/main/extensions/Sisk.Documenting`

Reference: https://docs.sisk-framework.org/docs/extensions/api-documentation  
Namespaces: `Sisk.Documenting`, `Sisk.Documenting.Annotations`, `Sisk.Documenting.Html`

---

## Setup

Register the documentation middleware on the builder:

```csharp
using Sisk.Documenting;
using Sisk.Documenting.Annotations;

using var host = HttpServer.CreateBuilder(5555)
    .UseApiDocumentation(
        context: new ApiGenerationContext
        {
            ApplicationName        = "My API",
            ApplicationDescription = "Description of my API.",
            ApplicationVersion     = "1.0.0",
            BodyExampleTypeHandler      = new JsonContentTypeHandler(),
            ParameterExampleTypeHandler = new JsonContentTypeHandler(),
        },
        routerPath: "/api/docs",
        exporter: new OpenApiExporter
        {
            ServerUrls = ["http://localhost:5555/"]
        })
    .UseRouter(r => r.SetObject(new MyController()))
    .Build();

await host.StartAsync();
```

- `routerPath` defaults to `"/api/docs"` when not provided.
- If `exporter` is `null`, a default `OpenApiExporter` is used.
- `UseApiDocumentation()` with no arguments uses an empty `ApiGenerationContext` and default exporter.

---

## Annotation Attributes

All attributes live in `Sisk.Documenting.Annotations`. Only methods decorated with `[ApiEndpoint]` are included in the generated documentation.

### `[ApiEndpoint]`
Marks a route method as a documented endpoint. **Required** for the method to appear in docs.

```csharp
[RouteGet("/users")]
[ApiEndpoint("List Users", Description = "Returns all users.", Group = "Users", Order = 1)]
public HttpResponse List(HttpRequest req) { ... }
```

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Display name (ctor or property). |
| `Description` | `string?` | Human-readable description. |
| `Group` | `string?` | Groups endpoints in the output (e.g., by controller). |
| `Order` | `int` | Sorting position within its group. Default `0`. |
| `InheritDescriptionFromXmlDocumentation` | `bool` | Falls back to XML `<summary>` if `Description` is not set. Default `true`. |

### `[ApiQueryParameter]`
Documents a query string parameter (`?key=value`). **Multiple allowed.**

```csharp
[ApiQueryParameter("page",     Type = "int",    IsRequired = false, Description = "Page number.")]
[ApiQueryParameter("pageSize", Type = "int",    IsRequired = false, Description = "Items per page.")]
```

### `[ApiPathParameter]`
Documents a path variable (e.g., `<id>` in `/users/<id>`). **Multiple allowed.**

```csharp
[ApiPathParameter("id", Type = "guid", Description = "The user identifier.")]
```

### `[ApiHeader]`
Documents a required or optional request header. **Multiple allowed.**

```csharp
[ApiHeader("Authorization", IsRequired = true, Description = "Bearer token.")]
[ApiHeader("X-Tenant-Id",   IsRequired = false)]
```

### `[ApiParameter]`
Documents a generic body/form parameter. **Multiple allowed.**

```csharp
[ApiParameter("username", "string", IsRequired = true,  Description = "Login username.")]
[ApiParameter("password", "string", IsRequired = true,  Description = "Login password.")]
```

### `[ApiParametersFrom]`
Reflects all public properties of a type and generates parameters from them via `ParameterExampleTypeHandler`. **Multiple allowed.**

```csharp
[ApiParametersFrom(typeof(CreateUserDto))]
```

Requires `ApiGenerationContext.ParameterExampleTypeHandler` to be set.

### `[ApiRequest]`
Documents the expected request body. **Multiple allowed** (e.g., multiple content types).

```csharp
[ApiRequest("Create user payload",
    PayloadType     = typeof(CreateUserDto),   // auto-generates example
    ExampleLanguage = "json")]

// or with a raw string example
[ApiRequest("Create user payload",
    Example         = """{"name":"Alice","email":"alice@example.com"}""",
    ExampleLanguage = "json")]
```

| Property | Description |
|---|---|
| `Description` | Required. Short description of the request body. |
| `PayloadType` | Type used to auto-generate example via `BodyExampleTypeHandler`. |
| `Example` | Raw example string (used if `PayloadType` is not set or handler returns null). |
| `ExampleLanguage` | Language hint for syntax highlighting (e.g., `"json"`, `"xml"`). |
| `JsonSchema` | Raw JSON schema string. Auto-generated from `PayloadType` if `ContentSchemaTypeHandler` is set. |

### `[ApiResponse]`
Documents a possible HTTP response. **Multiple allowed.**

```csharp
[ApiResponse(HttpStatusCode.OK,        PayloadType = typeof(UserDto),  Description = "User found.")]
[ApiResponse(HttpStatusCode.NotFound,  Description = "User not found.")]
[ApiResponse(HttpStatusCode.BadRequest, Example = """{"error":"Invalid input"}""", ExampleLanguage = "json")]
```

| Property | Description |
|---|---|
| `StatusCode` | Required (ctor). The HTTP status code. |
| `Description` | Describes when this response is returned. |
| `PayloadType` | Auto-generates response body example via `BodyExampleTypeHandler`. |
| `Example` | Raw example string. |
| `ExampleLanguage` | Language hint. |
| `JsonSchema` | Raw JSON schema. Auto-generated from `PayloadType` if `ContentSchemaTypeHandler` is set. |

---

## Full Annotated Example

```csharp
[RoutePrefix("/api/users")]
public class UsersController : RouterModule
{
    [RouteGet]
    [ApiEndpoint("List Users", Description = "Returns a paginated list of users.", Group = "Users")]
    [ApiQueryParameter("page",     Type = "int", IsRequired = false, Description = "Page number (1-based).")]
    [ApiQueryParameter("pageSize", Type = "int", IsRequired = false, Description = "Items per page.")]
    [ApiResponse(HttpStatusCode.OK,  PayloadType = typeof(UserDto[]), Description = "Paginated results.")]
    public HttpResponse List(HttpRequest req) { ... }

    [RouteGet("/<id>")]
    [ApiEndpoint("Get User", Group = "Users")]
    [ApiPathParameter("id", Type = "guid", Description = "User ID.")]
    [ApiResponse(HttpStatusCode.OK,       PayloadType = typeof(UserDto))]
    [ApiResponse(HttpStatusCode.NotFound, Description = "User not found.")]
    public HttpResponse Get(HttpRequest req) { ... }

    [RoutePost]
    [ApiEndpoint("Create User", Group = "Users")]
    [ApiRequest("User data", PayloadType = typeof(CreateUserDto))]
    [ApiResponse(HttpStatusCode.Created,    PayloadType = typeof(UserDto))]
    [ApiResponse(HttpStatusCode.BadRequest, Description = "Validation error.")]
    public HttpResponse Create(HttpRequest req) { ... }
}
```

---

## ApiGenerationContext

Metadata and handler configuration for the documentation engine:

```csharp
var context = new ApiGenerationContext
{
    ApplicationName        = "My API",
    ApplicationDescription = "Full description shown in docs.",
    ApplicationVersion     = "2.1.0",

    // generates JSON body examples for [ApiRequest] / [ApiResponse] with PayloadType
    BodyExampleTypeHandler = new JsonContentTypeHandler(new JsonSerializerOptions
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    }),

    // generates parameter rows for [ApiParametersFrom]
    ParameterExampleTypeHandler = new JsonContentTypeHandler(),

    // generates JSON Schema strings for PayloadType
    ContentSchemaTypeHandler = new JsonContentTypeHandler(),
};
```

All three handlers can be the same `JsonContentTypeHandler` instance.

---

## JsonContentTypeHandler

Built-in handler implementing `IExampleBodyTypeHandler`, `IExampleParameterTypeHandler`, and `IContentSchemaTypeHandler`:

```csharp
// default options (requires dynamic code / reflection)
new JsonContentTypeHandler()

// custom serializer options
new JsonContentTypeHandler(new JsonSerializerOptions { ... })

// AOT-safe: provide your own IJsonTypeInfoResolver
new JsonContentTypeHandler(myTypeInfoResolver, serializerOptions)
```

- `EnumerationExampleCount` — number of items generated in array/list examples. Default `1`.
- `IncludeDescriptionAnnotations` — include XML doc summaries as annotations. Default `true`.
- `JsonContentTypeHandler.Shared` — lazy shared singleton (reflection-based).

> For AOT / trimming, prefer the constructor overload that takes an `IJsonTypeInfoResolver`.

---

## Exporters

### OpenApiExporter

Produces an OpenAPI 3.0-compatible JSON document:

```csharp
new OpenApiExporter
{
    OpenApiVersion = "3.0.0",
    ServerUrls     = ["https://api.example.com", "http://localhost:5555"],
    TermsOfService = "https://example.com/terms",
    Contact = new OpenApiContact
    {
        Name  = "Support",
        Email = "support@example.com",
        Url   = "https://example.com/support"
    },
    License = new OpenApiLicense
    {
        Name = "MIT",
        Url  = "https://opensource.org/licenses/MIT"
    }
}
```

The generated JSON is served at `routerPath` with `Content-Type: application/json`.

### HtmlDocumentationExporter (`Sisk.Documenting.Html`)

Produces a fully self-contained HTML page with syntax highlighting (PrismJS) and a search bar:

```csharp
using Sisk.Documenting.Html;

host.UseApiDocumentation(
    context:    context,
    routerPath: "/docs",
    exporter:   new HtmlDocumentationExporter
    {
        PageTitle = "My API Docs",
        // Style and Script are pre-bundled and can be replaced with custom CSS/JS strings
    });
```

| Property | Default | Description |
|---|---|---|
| `PageTitle` | `"API documentation"` | HTML `<title>` and page heading. |
| `Style` | Built-in CSS | Full CSS string. Replace to apply custom theme. |
| `Script` | Built-in JS | JavaScript string. Replace to customise behaviour. |
| `FormatMainTitleServiceVersion` | `"Service version: {0}"` | Format string for version badge. |
| `FormatEndpointHeaders` | `"Headers:"` | Section heading. Override for i18n. |
| `FormatEndpointPathParameters` | `"Path parameters:"` | Section heading. |
| `FormatEndpointQueryParameters` | `"Query parameters:"` | Section heading. |
| `FormatEndpointParameters` | `"Request parameters:"` | Section heading. |
| `FormatEndpointResponses` | `"Responses:"` | Section heading. |
| `FormatEndpointRequestExamples` | `"Request examples:"` | Section heading. |

### Custom Exporter

Implement `IApiDocumentationExporter` to output any format (Markdown, Postman, etc.):

```csharp
public class MarkdownExporter : IApiDocumentationExporter
{
    public HttpContent ExportDocumentationContent(ApiDocumentation documentation)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"# {documentation.ApplicationName}");

        foreach (var endpoint in documentation.Endpoints)
        {
            sb.AppendLine($"## {endpoint.RouteMethod} {endpoint.Path}");
            if (endpoint.Description is { } desc)
                sb.AppendLine(desc);
        }

        return new StringContent(sb.ToString(), Encoding.UTF8, "text/markdown");
    }
}
```

---

## ApiDocumentation Object

If you need to generate docs programmatically (e.g., write to disk):

```csharp
var router = new Router();
// ... define routes ...

var docs = ApiDocumentation.Generate(router, context);

// docs.ApplicationName / docs.ApplicationDescription / docs.ApiVersion
// docs.Endpoints → ApiEndpoint[]
//   .Name / .Description / .Group / .RouteMethod / .Path / .Order
//   .Headers           → ApiEndpointHeader[]
//   .Parameters        → ApiEndpointParameter[]
//   .QueryParameters   → ApiEndpointQueryParameter[]
//   .PathParameters    → ApiEndpointPathParameter[]
//   .RequestExamples   → ApiEndpointRequestExample[]
//   .Responses         → ApiEndpointResponse[]

var exporter = new OpenApiExporter { ServerUrls = ["https://api.example.com"] };
HttpContent content = exporter.ExportDocumentationContent(docs);
string json = await content.ReadAsStringAsync();
File.WriteAllText("openapi.json", json);
```

---

## Custom Type Handlers

### `IExampleBodyTypeHandler`

Generates body example strings for `[ApiRequest]` / `[ApiResponse]` with `PayloadType`:

```csharp
public class XmlBodyHandler : IExampleBodyTypeHandler
{
    public BodyExampleResult? GetBodyExampleForType(Type type)
    {
        string xml = MyXmlGenerator.Serialize(type);
        return new BodyExampleResult(xml, "xml");
    }
}
```

### `IExampleParameterTypeHandler`

Generates parameter rows for `[ApiParametersFrom]`:

```csharp
public class ReflectionParameterHandler : IExampleParameterTypeHandler
{
    public ParameterExampleResult[] GetParameterExamplesForType(Type type)
        => type.GetProperties()
               .Select(p => new ParameterExampleResult(
                   name:        p.Name,
                   typeName:    p.PropertyType.Name,
                   isRequired:  true,
                   description: null))
               .ToArray();
}
```

### `IContentSchemaTypeHandler`

Generates JSON Schema strings for `PayloadType` in `[ApiRequest]` / `[ApiResponse]` when `JsonSchema` is not manually set:

```csharp
public class MySchemaHandler : IContentSchemaTypeHandler
{
    public JsonObject? GetJsonSchemaForType(Type type)
    {
        // return a LightJson JsonObject representing the schema
    }
}
```

---

## Notes

- Only methods decorated with `[ApiEndpoint]` are discovered — routes without it are ignored in docs.
- `InheritDescriptionFromXmlDocumentation = true` (default) reads the XML `<summary>` if `Description` is not provided; this requires the project to emit XML docs (`<GenerateDocumentationFile>true</GenerateDocumentationFile>`).
- Documentation is generated **once** at startup via `OnSetupRouter` in the internal `HttpServerHandler`. Adding routes after `Build()` will not update the docs.
- `[ApiResponse]` and `[ApiRequest]` are `AllowMultiple = true` — stack them freely.
- When both `PayloadType` and `Example` are set, `PayloadType` takes precedence if the handler returns a result.
