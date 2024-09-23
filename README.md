<div align="center" style="display:grid;place-items:center;">
  <p>
      <a href="https://sisk.proj.pw/" target="_blank"><img width="160" src="./.github/Icon.png"></a>
  </p>
  <h1>Sisk Framework</h1>

  [Discover Sisk](https://www.sisk-framework.org/) 
  | [Documentation](https://docs.sisk-framework.org/) 
  | [Blog](https://blog.sisk-framework.org/) 
  | [Changelogs](https://github.com/sisk-http/archive/tree/master/changelogs) 
  | [Benchmarks](https://github.com/sisk-http/benchmarks)

  <div>

  <a href="">[![Nuget](https://img.shields.io/nuget/dt/Sisk.HttpServer?logo=nuget)](https://www.nuget.org/packages/Sisk.HttpServer/)</a>
  <a href="">[![Nuget](https://img.shields.io/nuget/v/Sisk.HttpServer?label=last%20version)](https://www.nuget.org/packages/Sisk.HttpServer/)</a>
  <a href="">[![Nuget](https://img.shields.io/github/license/sisk-http/core)](https://github.com/sisk-http/core/blob/master/LICENSE.txt)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/.net_version-8_|_9-purple?logo=dotnet)](#)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/platform-win%20|%20unix%20|%20osx-orange.svg)](#)</a>

  </div>

  **Sisk** is a set of libraries for web development that is lightweight, agnostic, easy, simple, and robust. The perfect choice for your next project.

</div>

------


### Documentation

You can get started with Sisk [here](https://docs.sisk-framework.org/) or build the documentation repository [here](https://github.com/sisk-http/docs).

For information about release notes, changelogs and API breaking changes, please refer to [changelogs archive](https://github.com/sisk-http/archive/tree/master/changelogs).

### Installing

You can install the latest release from [Nuget packages](https://www.nuget.org/packages/Sisk.HttpServer/):

```
dotnet add package Sisk.HttpServer
```

### Packages

In this repository, you have the source code of:

  - [Sisk.HttpServer](src): the Sisk Framework mainframe and core functions.
  - [Sisk.SslProxy](extensions/Sisk.SslProxy): the HTTPS provider for Sisk.
  - [Sisk.BasicAuth](extensions/Sisk.BasicAuth): the basic authentication package which provides helper request handlers for handling authentication.
  - [Sisk.ServiceProvider](extensions/Sisk.ServiceProvider): (obsolete) the Service Providers utility package for porting your Sisk app between environments. This package is indeed to work with version 0.15 and olders, as 0.16 has it implemented on it's [core package](https://github.com/sisk-http/docs/blob/master/archive/0.16/service-providers-migration.md).

## Getting started

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

You can learn more about Sisk on its [website](https://www.sisk-framework.org/).

## Main features

Sisk can do web development the way you want. Create MVC, MVVC, SOLID applications, or any other design pattern you're interested in.

- **Lightweight:** robust projects tested in small, low-cost, low-performance environments and got good results.
- **Open-source:** the entire Sisk ecosystem is open source, and all the libraries and technologies we use must be open source as well. Sisk is entirely distributed under the MIT License, which allows the commercial development.
- **Sustainable:** you are the one who makes the project, Sisk gives you the tools. Because it is open source, the community (including you) can maintain, fix bugs and improve Sisk over time.

## Stargazers over time

[![Stargazers over time](https://starchart.cc/sisk-http/core.svg?variant=light)](https://starchart.cc/sisk-http/core)

## License

The entire Sisk ecosystem is licensed under the [MIT License](https://sisk.project-principium.dev/license).
