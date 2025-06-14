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
  
  <a href="">[![Tests](https://github.com/sisk-http/core/actions/workflows/dotnet-tests.yml/badge.svg)](https://github.com/sisk-http/core/actions/workflows/dotnet-tests.yml)</a>
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
- [Sisk.IniConfiguration](extensions/Sisk.IniConfiguration): an INI-document configuration provider for Sisk.
- [Sisk.BasicAuth](extensions/Sisk.BasicAuth): the basic authentication package which provides helper request handlers for handling authentication.
- [Sisk.Cadente](extensions/Sisk.Cadente): an experimental "ultrafast" implementation of the HTTP/1.1 protocol in pure C#.

## Getting started

The Sisk core idea is to create a service that runs on the internet and follows the pattern you define. Moreover, Sisk is a framework that adapts to how you want it to work, not the other way around.

Due to its explicit nature, its behavior is fully predictable. The main differentiator from ASP.NET is that Sisk can be built and run in very few lines of code, avoiding unnecessary configurations, and requiring the minimum setup to get your server working. Additionally, it does not demand any additional .NET SDK packages to work with, as the base package of .NET is sufficient to start your development with Sisk.

You can build applications that are not necessarily a complete web application, but that have an web module, such as receiving an OAuth authorization token, hosting a UI to control your application, monitoring logs, etc. Sisk is very flexible and abstract.

```c#
class Program
{
    static async Task Main(string[] args)
    {
        using var app = HttpServer.CreateBuilder()
            .UseListeningPort("http://localhost:5000/")
            .Build();
        
        app.Router.MapGet("/", request =>
        {
            return new HttpResponse()
            {
                Status = 200,
                Content = new StringContent("Hello, world!")
            };
        });
        
        await app.StartAsync();
    }
}
```

You can learn more about Sisk on it's [website](https://www.sisk-framework.org/).

## Main features

Sisk can do web development the way you want. Create MVC, MVVC, SOLID applications, or any other design pattern you're interested in.

- **Flexible**: you're not limited to build an full-featured web application, but it can be an module to your existing project.
- **Lightweight:** robust projects can be runned in small, low-cost environments and provide a nice performance. The Sisk core server also uses zero dependencies on it's core library. 
- **Open-source:** the entire Sisk ecosystem is open source, and all the libraries and technologies we use must be open source as well. Sisk is entirely distributed under the MIT License, which allows the commercial development.
- **Sustainable:** the project adapts to the pattern you want. Write MVC, MVVM, Clean-Architecture, OOP, Functional applications... you are in control.

## Stargazers over time

[![Stargazers over time](https://starchart.cc/sisk-http/core.svg?variant=light)](https://starchart.cc/sisk-http/core)

## Contributors

Special thanks to all the contributors who have helped Sisk to grow and improve.

- [Khalid Abuhakmeh](https://github.com/khalidabuhakmeh)
- [Sascha-L](https://github.com/Sascha-L)

## License

The entire Sisk ecosystem is licensed under the [MIT License](https://sisk.project-principium.dev/license).
