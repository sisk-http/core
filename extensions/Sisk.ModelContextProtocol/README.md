# Sisk.ModelContextProtocol

This package is an extension to the Sisk Framework and provides a native implementation to the MCP (Model Context Protocol), which allows creating applications that provide resources for artificial intelligence and agentic models.

Currently, the following MCP features are supported:

| Feature | Description | Status |
| ------- | --------- | -------- |
| Tools | Provides tools for MCP clients. | ⚠️ In progress. |
| Prompts | Provides standardized prompts for MCP clients. | ❌ Not implemented. |
| Resources | Lists and reads registered skill files. | ✅ Implemented for skills. |
| Skills | Announces and serves Agent Skills through `io.modelcontextprotocol/skills`. | ✅ Implemented. |
| Completions | Provides argument completions and suggestions for clients and resources. | ❌ Not implemented. |
| Logging | Provides logs for clients. | ❌ Not implemented. |

### Transport

The implemented transport is [Streamable HTTP](https://modelcontextprotocol.io/specification/2026-07-28/basic/transports/streamable-http), supporting singular request/response messages. The provider supports modern stateless requests for protocol `2026-07-28` and preserves the legacy initialization flow for older clients.

### Publishing skills

Register skills before the provider starts handling requests:

```csharp
provider.Skills.Add(new McpSkill(
    path: "acme/refunds",
    frontmatter: new LightJson.JsonObject
    {
        ["name"] = "refunds",
        ["description"] = "Follow the refund workflow.",
        ["license"] = "MIT"
    },
    body: "# Refunds\nRead references/policy.md before using the refund tools.",
    files: new Dictionary<string, byte[]>
    {
        ["references/policy.md"] = File.ReadAllBytes("skills/refunds/references/policy.md")
    }));
```

The provider advertises `capabilities.resources` and
`capabilities.extensions["io.modelcontextprotocol/skills"]` in `server/discover`
when skills are registered. Clients use `skills/list` or `skills/get` to obtain
complete manifests and `resources/read` to fetch individual files. `resources/list`
also enumerates these files. These methods require the existing stateless MCP
`2026-07-28` request metadata and HTTP headers. Legacy initialization does not
advertise the extension.

`McpSkill` generates `SKILL.md` with JSON-form YAML frontmatter (JSON is valid YAML)
and the supplied Markdown body. All supplied frontmatter fields are preserved.
Pass instructions without an existing frontmatter block; this API does not import
or parse existing YAML files. The final path segment must equal `frontmatter.name`.
Prefix segments accept ASCII letters, digits, `.`, `_`, `~`, and `-`, excluding
`.` and `..`. Supporting file keys are relative paths with `/` separators.

The generated entry and supporting bytes are snapshotted at construction. Each
manifest includes every registered file, its byte size, and its SHA-256 digest.
`SKILL.md` is returned as text; supporting files are returned as base64 blobs,
including supporting text files, preserving their original bytes. No file is
executed or read from disk by the provider itself. Applications must register
unique skill URIs and include every nested skill file in the enclosing skill's
`files`, using identical bytes for shared resources (including the generated nested
`SKILL.md`). Duplicate skill URIs, divergent shared content, or incomplete enclosing
manifests cause skill/resource requests to return `-32603` rather than serve an
inconsistent catalog. Do not mutate the catalog while requests are being handled.

Each skill is limited to 512 files and 16 MiB including `SKILL.md`. Listings return
the entire catalog in one response without `nextCursor`; supplied cursors are
rejected. Results use `ttlMs: 0` and `cacheScope: "private"`. Unknown skill/resource
URIs and invalid parameters return `-32602`. Dynamic skills, directory reads,
subscriptions, and change notifications are not implemented or advertised.

See the [Skills extension specification](https://github.com/modelcontextprotocol/ext-skills/blob/main/specification/stable/skills.mdx).

#### Usage Example

```csharp
internal class Program
{
    static async Task Main(string[] args)
    {
        var mcpProvider = new McpProvider()
        {
            Tools = [
                new McpTool(
                    name: "sum",
                    description: "Sum two numbers",
                    schema: JsonSchema.CreateObjectSchema(
                        properties: new Dictionary<string, JsonSchema>()
                        {
                            { "a", JsonSchema.CreateNumberSchema() },
                            { "b", JsonSchema.CreateNumberSchema() }
                        }),
                    executionHandler: (ctx) => {
                        double a = ctx.Arguments["a"].GetNumber();
                        double b = ctx.Arguments["b"].GetNumber();
                        
                        return Task.FromResult(McpToolResult.CreateText("Result: " + (a + b)));
                    })
            ]
        };
        
        using var host = HttpServer.CreateBuilder()
            .UseListeningPort(19999)
            .UseRouter(router =>
            {
                router.MapAny("/mcp", async (HttpRequest request) =>
                {
                    return await mcpProvider.HandleRequestAsync(request, default);
                });
            })
            .Build();

        await host.StartAsync();
    }
}
```

Or through dependency injection:

```csharp
internal class Program
{
    static async Task Main(string[] args)
    {
        using var host = HttpServer.CreateBuilder()
            .UseListeningPort(19999)
            .UseMcp(provider =>
            {
                provider.Tools.Add(new McpTool(
                    name: "say-hello",
                    description: "Says hello world.",
                    schema: JsonSchema.Empty,
                    executionHandler: (ctx) => Task.FromResult(McpToolResult.CreateText("Hello, world!"))));
            })
            .UseRouter(r =>
            {
                r.SetObject(new ApplicationHandler());
            })
            .Build();

        await host.StartAsync();
    }
}

class ApplicationHandler
{
    [Route(RouteMethod.Get | RouteMethod.Post, "mcp")]
    public async Task<HttpResponse> McpIndex(HttpRequest request)
    {
        return await request.HandleMcpRequestAsync(cancellation: default);
    }
}
```
