**Sisk** is a **web development framework** that is lightweight, agnostic, easy, simple, and robust. The perfect choice for your next project.

- [Discover Sisk](https://www.sisk-framework.org/)
- [Documentation](https://docs.sisk-framework.org/)
- [Changelogs](https://github.com/sisk-http/archive/tree/master/changelogs)
- [Benchmarks](https://github.com/sisk-http/benchmarks)

## Documentation

You can get started with Sisk [here](https://md.proj.pw/sisk-http/docs-v2/main/) or build the documentation repository [here](https://github.com/sisk-http/docs-v2).

For information about release notes, changelogs and API breaking changes, see [docs/Changelog.md](https://github.com/sisk-http/docs/blob/master/Changelog.md).

The Sisk core idea is to create a service that runs on the internet and follows the pattern you define. Moreover, Sisk is a framework that adapts to how you want it to work, not the other way around.

Due to its explicit nature, its behavior is predictable. The main differentiator from ASP.NET is that Sisk can be up and running in very few lines of code, avoiding unnecessary configurations, and requiring the minimum setup to get your server working. Additionally, it does not demand any additional .NET SDK packages to develop, as the base package of .NET 6 is sufficient to start your development with Sisk.

It can handle multiple requests asynchronously, provides useful tools to manage and accelerate web development.

```c#
using Sisk.Core.Http;

class Program
{
    static async Task Main(string[] args)
    {
        using var app = HttpServer.CreateBuilder(5555).Build();

        app.Router.MapGet("/", request =>
        {
            return new HttpResponse("Hello, world!");
        });

        await app.StartAsync(); // 🚀 app is listening on http://localhost:5555/
    }
}
```

## Main features

Sisk can do web development the way you want. Create MVC, MVVC, SOLID applications, or any other design pattern you're interested in.

- **Lightweight:** robust projects tested in small, low-cost, low-performance environments and got good results.
- **Open-source:** the entire Sisk ecosystem is open source, and all the libraries and technologies we use must be open source as well. Sisk is entirely distributed under the MIT License, which allows the commercial development.
- **Sustainable:** you are the one who makes the project, Sisk gives you the tools. Because it is open source, the community (including you) can maintain, fix bugs and improve Sisk over time.

## Read More

Read more about Sisk at it's [repository](https://github.com/sisk-http/) or [website](https://www.sisk-framework.org/).**Sisk** is a **web development framework** that is lightweight, agnostic, easy, simple, and robust. The perfect choice for your next project.

- [Discover Sisk](https://www.sisk-framework.org/)
- [Documentation](https://docs.sisk-framework.org/)
- [Changelogs](https://github.com/sisk-http/archive/tree/master/changelogs)
- [Benchmarks](https://github.com/sisk-http/benchmarks)

## Documentation

You can get started with Sisk [here](https://md.proj.pw/sisk-http/docs-v2/main/) or build the documentation repository [here](https://github.com/sisk-http/docs-v2).

For information about release notes, changelogs and API breaking changes, see [docs/Changelog.md](https://github.com/sisk-http/docs/blob/master/Changelog.md).

The Sisk core idea is to create a service that runs on the internet and follows the pattern you define. Moreover, Sisk is a framework that adapts to how you want it to work, not the other way around.

Due to its explicit nature, its behavior is predictable. The main differentiator from ASP.NET is that Sisk can be up and running in very few lines of code, avoiding unnecessary configurations, and requiring the minimum setup to get your server working. Additionally, it does not demand any additional .NET SDK packages to develop, as the base package of .NET 6 is sufficient to start your development with Sisk.

It can handle multiple requests asynchronously, provides useful tools to manage and accelerate web development.

```c#
using Sisk.Core.Http;
using Sisk.Core.Routing;

namespace myProgram;

class Program
{
    static void Main(string[] args)
    {
        var app = HttpServer.CreateBuilder();

        app.Router += new Route(RouteMethod.Get, "/", request =>
        {
            return new HttpResponse(200)
                .WithContent("Hello, world!");
        });

        app.Start();
    }
}
```

## Main features

Sisk can do web development the way you want. Create MVC, MVVC, SOLID applications, or any other design pattern you're interested in.

- **Lightweight:** robust projects tested in small, low-cost, low-performance environments and got good results.
- **Open-source:** the entire Sisk ecosystem is open source, and all the libraries and technologies we use must be open source as well. Sisk is entirely distributed under the MIT License, which allows the commercial development.
- **Sustainable:** you are the one who makes the project, Sisk gives you the tools. Because it is open source, the community (including you) can maintain, fix bugs and improve Sisk over time.

## Read More

Read more about Sisk at it's [repository](https://github.com/sisk-http/) or [website](https://www.sisk-framework.org/).