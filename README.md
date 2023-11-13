<div align="center" style="display:grid;place-items:center;">
  <p>
      <a href="https://sisk.proj.pw/" target="_blank"><img width="160" src="./.github/Icon.png" alt="V logo"></a>
  </p>
  <h1>Sisk Framework</h1>

  [Discover Sisk](https://sisk.project-principium.dev/) | [Documentation](https://sisk.project-principium.dev/read?q=/contents/docs/welcome.md) | [Changelog](https://github.com/sisk-http/docs/blob/master/Changelog.md) | [Specification](https://sisk.project-principium.dev/read?q=/contents/spec/index.md) | [Benchmark](https://github.com/sisk-http/benchmarks) | [Roadmap](https://github.com/orgs/sisk-http/projects/1)

  <div>

  <a href="">[![Nuget](https://img.shields.io/nuget/dt/Sisk.HttpServer?logo=nuget)](https://www.nuget.org/packages/Sisk.HttpServer/)</a>
  <a href="">[![Nuget](https://img.shields.io/github/license/sisk-http/core)](https://github.com/sisk-http/core/blob/master/LICENSE.txt)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/.net%20version-%206%20|%207%20|%208-purple.svg?logo=dotnet)](#)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/platform-win%20|%20unix%20|%20osx-orange.svg)](#)</a>

  </div>

  **Sisk** is a **web development framework** that is lightweight, agnostic, easy, simple, and robust. The perfect choice for your next project.

  **You can do more with less.**

</div>

------


### Documentation

You can get started with Sisk [here](https://sisk.project-principium.dev/read?q=/contents/docs/getting-started.md) or build the documentation repository [here](https://github.com/sisk-http/docs).

For information about release notes, changelogs and API breaking changes, see [docs/Changelog.md](https://github.com/sisk-http/docs/blob/master/Changelog.md).

### Installing

You can install the latest release from [Nuget packages](https://www.nuget.org/packages/Sisk.HttpServer/):

```
PM> NuGet\Install-Package Sisk.HttpServer
```

### Packages

In this repository, you have the source code of:

  - [Sisk.HttpServer](src): the Sisk Framework mainframe and core functions.
  - [Sisk.ServiceProvider](extensions/Sisk.ServiceProvider): the Service Providers utility package for porting your Sisk app between environments. This package is indeed to work with version 0.15 and olders, as [0.16 has it implemented on it's [core package](https://github.com/sisk-http/docs/blob/master/archive/0.16/service-providers-migration.md).
  - [Sisk.BasicAuth](extensions/Sisk.BasicAuth): the basic authentication package which provides helper request handlers for handling authentication.

## Getting started

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

## Why use Sisk?

Sisk is a highly modular and sustainable framework designed for creating a variety of applications, including Restful applications, gRPC, Websockets, file servers, GraphQL, Entity Framework, and more. Its development is ongoing, with a focus on simplicity, easy maintenance, and an enjoyable experience for developers. Sisk is known for its efficiency, even in low-performance environments, and it can handle over twenty thousand requests per second. The framework is compatible with any machine supporting .NET, including those that do not require Native AOT. Sisk also offers additional implementations and packages for extended functionality.

While Sisk draws inspiration from ASP.NET, it aims for simpler and more performant development without the need for installing additional components. It provides a straightforward and robust development model, allowing developers to handle requests efficiently and explicitly. Sisk simplifies HTTP-related tasks and offers comprehensive documentation and open-source access to its source code, making it accessible and easy to learn for web developers.