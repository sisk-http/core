# Sisk.ModelContextProtocol

This package is an extension to the Sisk Framework and provides a native implementation to the MCP (Model Context Protocol), which allows creating applications that provide resources for artificial intelligence and agentic models.

Currently, the following MCP features are supported:

| Feature | Description | Status |
| ------- | --------- | -------- |
| Tools | Provides tools for MCP clients. | ⚠️ In progress. |
| Prompts | Provides standardized prompts for MCP clients. | ❌ Not implemented. |
| Resources | Provides resources and files for MCP clients. | ❌ Not implemented. |
| Completions | Provides argument completions and suggestions for clients and resources. | ❌ Not implemented. |
| Logging | Provides logs for clients. | ❌ Not implemented. |

### Transport

The implemented transport is by [Streamable HTTP](https://modelcontextprotocol.io/specification/2025-06-18/basic/transports#streamable-http), supporting only singular messages.

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
