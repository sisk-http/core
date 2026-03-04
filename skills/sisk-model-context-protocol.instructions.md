---
applyTo: '**/Sisk.ModelContextProtocol/**/*.cs'
---

# Sisk.ModelContextProtocol

Implements an MCP (Model Context Protocol) server over **Streamable HTTP** transport, allowing Sisk applications to expose tools that AI/LLM agent clients can call.

```
dotnet add package Sisk.ModelContextProtocol
```

> **Status: Under development.** Only **Tools** are currently supported. Prompts, Resources, Completions, and server-side Logging are not yet implemented.  
> Protocol version: `2025-06-18`

Reference: https://docs.sisk-framework.org/docs/extensions/mcp  
Namespace: `Sisk.ModelContextProtocol`

---

## Quick Setup

### Standalone provider (multiple providers or manual wiring)

```csharp
using Sisk.ModelContextProtocol;
using LightJson.Schema;

var mcp = new McpProvider(
    serverName:    "my-server",
    serverTitle:   "My MCP Server",
    serverVersion: new Version(1, 0));

mcp.Tools.Add(new McpTool(
    name:             "say_hello",
    description:      "Says hello to someone.",
    schema:           JsonSchema.CreateObjectSchema(
        properties: new Dictionary<string, JsonSchema>
        {
            { "name", JsonSchema.CreateStringSchema(description: "Person's name.") }
        },
        requiredProperties: ["name"]),
    executionHandler: async ctx =>
    {
        string name = ctx.Arguments["name"].GetString();
        return McpToolResult.CreateText($"Hello, {name}!");
    }));

using var host = HttpServer.CreateBuilder()
    .UseListeningPort(5000)
    .UseRouter(r =>
    {
        r.MapAny("/mcp", async req => await mcp.HandleRequestAsync(req));
    })
    .Build();

await host.StartAsync();
```

### Singleton via builder (`UseMcp`)

```csharp
using var host = HttpServer.CreateBuilder()
    .UseListeningPort(5000)
    .UseMcp(mcp =>
    {
        mcp.ServerName  = "my-server";
        mcp.ServerTitle = "My MCP Server";

        mcp.Tools.Add(new McpTool(
            name:             "say_hello",
            description:      "Says hello.",
            schema:           JsonSchema.Empty,
            executionHandler: ctx => Task.FromResult(McpToolResult.CreateText("Hello!"))));
    })
    .UseRouter(r =>
    {
        // HandleMcpRequestAsync uses the singleton configured by UseMcp
        r.MapAny("/mcp", async req => await req.HandleMcpRequestAsync());
    })
    .Build();
```

- `UseMcp(McpProvider)` — registers an existing provider instance as the singleton.
- `UseMcp(Action<McpProvider>)` — creates and configures a new provider as the singleton.
- `request.HandleMcpRequestAsync()` — dispatches to the singleton; throws `InvalidOperationException` if `UseMcp` was never called.

---

## McpProvider

| Member | Description |
|---|---|
| `ServerName` | Machine-readable server identifier. Default `"ExampleServer"`. |
| `ServerTitle` | Human-readable display name. Default `"Example Server Display Name"`. |
| `ServerVersion` | `Version` object. Default `1.0`. |
| `Tools` | `IList<McpTool>` — add tools here before the server starts. |
| `PROTOCOL_VERSION` | Const `"2025-06-18"` — the supported MCP spec version. |
| `HandleRequestAsync(HttpRequest, CancellationToken)` | Processes a single MCP HTTP request and returns an `HttpResponse`. |

The endpoint handler must accept both **GET** (initialization) and **POST** (tool calls) — use `RouteMethod.Get | RouteMethod.Post` or `MapAny`.

---

## McpTool

```csharp
new McpTool(
    name:             "tool_name",     // unique, snake_case by convention
    description:      "What it does.", // shown to the LLM
    schema:           JsonSchema.CreateObjectSchema(...),
    executionHandler: async ctx => { ... },
    title:            "Tool Display Name" // optional; defaults to name
)
```

| Property | Type | Description |
|---|---|---|
| `Name` | `string` | Unique tool identifier (shown to the LLM). |
| `Title` | `string?` | Optional display name. Falls back to `Name`. |
| `Description` | `string` | Human/LLM-readable description of what the tool does. |
| `Schema` | `JsonSchema` | Input schema. Use `JsonSchema.Empty` for no arguments. |
| `ExecuteAsync` | `McpToolHandler` | The execution delegate. Can be replaced after construction. |

---

## JSON Schema Builder

Uses `LightJson.Schema.JsonSchema`. All factory methods accept an optional `description` parameter:

```csharp
// object with typed properties
JsonSchema.CreateObjectSchema(
    properties: new Dictionary<string, JsonSchema>
    {
        { "query",   JsonSchema.CreateStringSchema(description: "Search query.") },
        { "limit",   JsonSchema.CreateNumberSchema(description: "Max results.") },
        { "enabled", JsonSchema.CreateBooleanSchema() },
        { "tags",    JsonSchema.CreateArraySchema(
                         itemsSchema: JsonSchema.CreateStringSchema(),
                         minItems: 0,
                         description: "Filter tags.") }
    },
    requiredProperties: ["query"]);   // non-required props may be absent in call

// enum / fixed set of string values
JsonSchema.CreateStringSchema(
    enums: ["asc", "desc"],
    description: "Sort direction.");

// no arguments
JsonSchema.Empty
```

Available factory methods:
- `JsonSchema.CreateObjectSchema(properties, requiredProperties, description)`
- `JsonSchema.CreateArraySchema(itemsSchema, minItems, description)`
- `JsonSchema.CreateStringSchema(enums, description)`
- `JsonSchema.CreateNumberSchema(description)`
- `JsonSchema.CreateBooleanSchema(description)`
- `JsonSchema.Empty`

The schema is automatically validated against tool call arguments before the handler is invoked. Validation failures return a descriptive error text to the LLM without calling the handler.

---

## McpToolContext

Provided to the execution handler:

```csharp
executionHandler: async (McpToolContext ctx) =>
{
    // required argument
    string query = ctx.Arguments["query"].GetString();

    // optional argument (may be absent/null)
    int limit = ctx.Arguments["limit"].MaybeNull()?.GetInteger() ?? 10;

    // raw JSON object access
    JsonObject raw = ctx.Arguments["options"].GetJsonObject();

    // array
    double[] numbers = ctx.Arguments["numbers"].GetJsonArray().ToArray<double>();

    // metadata from the LLM client (rarely needed)
    JsonObject meta = ctx.Metadata;

    // cancel long-running work on client disconnect
    ctx.Cancellation.ThrowIfCancellationRequested();

    return McpToolResult.CreateText("done");
}
```

| Property | Type | Description |
|---|---|---|
| `Arguments` | `JsonObject` | Validated input arguments from the tool call. |
| `Metadata` | `JsonObject` | Optional `_meta` field from the protocol request. |
| `ToolName` | `string` | Name of the invoked tool. |
| `Server` | `McpProvider` | The owning `McpProvider` instance. |
| `Request` | `HttpRequest` | The raw Sisk `HttpRequest` (access headers, IP, etc.). |
| `Cancellation` | `CancellationToken` | Raised on client disconnect or timeout. |

**Reading argument values (LightJson API):**

| Method | Returns |
|---|---|
| `.GetString()` | `string` — throws if null |
| `.GetNumber()` | `double` |
| `.GetInteger()` | `int` |
| `.GetBoolean()` | `bool` |
| `.GetJsonObject()` | `JsonObject` |
| `.GetJsonArray()` | `JsonArray` |
| `.MaybeNull()` | `JsonValue?` — returns `null` if value is JSON null |

---

## McpToolResult

```csharp
// text (most common)
McpToolResult.CreateText("The answer is 42.");

// image — bytes are base64-encoded automatically
byte[] png = await screenshot.CaptureAsync();
McpToolResult.CreateImage(png, "image/png");

// audio
byte[] wav = await audio.RecordAsync();
McpToolResult.CreateAudio(wav, "audio/wav");

// combine multiple content pieces into one response
McpToolResult.Combine(
    McpToolResult.CreateText("Here is the screenshot:"),
    McpToolResult.CreateImage(png, "image/png"),
    McpToolResult.CreateText("Analysis complete.")
);
```

- `Combine` with a single argument returns it directly (no wrapping).
- `Combine` with multiple arguments merges them into a JSON array.
- Errors thrown inside the handler are caught automatically and returned to the LLM as a text error result with `isError: true`.

---

## Handled MCP Methods

The following JSON-RPC methods are handled by `HandleRequestAsync`:

| Method | Notes |
|---|---|
| `GET` (HTTP) | Returns an `initialize` response (capabilities). |
| `initialize` | Handshake — returns server info and capabilities. Protocol version mismatch is tolerated. |
| `tools/list` | Returns the registered `Tools` list. |
| `tools/call` | Validates schema, invokes `ExecuteAsync`, returns result. |
| `ping` | Returns empty `{}`. |
| `notifications/*` | Accepted silently (`202 Accepted`). |
| anything else | Returns JSON-RPC error `-32000 "Method not found or not supported"`. |

---

## Error Handling

Exceptions thrown inside a tool handler are **caught automatically** — the LLM receives a text content response with `isError: true` describing the exception message. You do not need to `try/catch` unless you want to format the error yourself:

```csharp
executionHandler: async ctx =>
{
    try
    {
        var result = await DoWorkAsync(ctx.Arguments, ctx.Cancellation);
        return McpToolResult.CreateText(result);
    }
    catch (TimeoutException)
    {
        return McpToolResult.CreateText("Operation timed out. Try with a smaller dataset.");
    }
}
```

---

## Notes

- The MCP endpoint path (`/mcp`) is arbitrary — choose any path.
- Tools are registered once at startup; the tool list does not hot-reload (`listChanged: false`).
- Use `McpProvider.HandleRequestAsync` directly when you need multiple MCP providers on different routes.
- Use `request.HandleMcpRequestAsync()` shorthand only when a single provider per app is sufficient.
- `ctx.Arguments` has already been schema-validated before the handler is called.
- For AOT/trimming support, be aware that `JsonSchema` validation uses reflection internally.
