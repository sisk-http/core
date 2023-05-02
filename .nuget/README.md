# Welcome to Sisk

## Sisk is a powerful framework for building powerful web applications for Windows, Linux and Mac.

With Sisk, you can use the full power of .NET to create fast, dynamic and powerful web applications. It's an alternative to Microsoft's ASP.NET Core, which is simpler and easier to understand and setup.

```cs
public class Program
{
    static void Main(string[] args)
    {
        HttpServer http = HttpServer.Emit(
            insecureHttpPort: 5555,
            out HttpServerConfiguration serverConfig,
            out ListeningHost listeningHost,
            out Router mainRouter
        );

        mainRouter += new Route(RouteMethod.Get, "/", request =>
        {
            return new HttpResponse(200) { Content = new StringContent("Hello, world!") };
        });

        http.Start();

        Console.WriteLine($"HTTP server is listening on {http.ListeningPrefixes[0]}");

        Thread.Sleep(-1);
    }
}
```

## Main features:

- 100% open source
- Multi-platform and cross-operating system
- Ultra fast asynchronous response/second average
- Uses the operating system's native HTTP interface listener
- Sustainable and maintainable
- Code-pattern-agnostic
- Easy setup
- Same code implementation for *nix, Mac OS and Windows
- Middlewares, before and/or after request handlers
- Configurable error handling
- Support to log advanced logging with rotating support
- Easy Cross-Origin Resource Sharing setup
- Server Side Events and Web Sockets support
- Multiple listening hosts per service
- Advanced routing system
- Less than 170kb the entire package
- Written in C#

You can learn how to get started with Sisk [here](https://sisk.project-principium.dev/#/docs/getting-started).