<p align="center">
  <img width="100%" height="auto" src="./.github/Header.png">
</p>

<div align="center">

  <a href="">[![Nuget](https://img.shields.io/nuget/dt/Sisk.HttpServer?logo=nuget)](https://www.nuget.org/packages/Sisk.HttpServer/)</a>
  <a href="">[![Nuget](https://img.shields.io/github/license/sisk-http/core)](https://github.com/sisk-http/core/blob/master/LICENSE.txt)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/.net%20version-%206%20|%207%20|%208-purple.svg?logo=dotnet)](#)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/platform-win%20|%20unix%20|%20osx-orange.svg)](#)</a>
</div>

------

**Sisk** is a **web development framework** that is lightweight, agnostic, easy, simple, and robust. The perfect choice for your next project.

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
  - [Sisk.ServiceProvider](extensions/Sisk.ServiceProvider): the Service Providers utility package for porting your Sisk app between environments.
  - [Sisk.BasicAuth](extensions/Sisk.BasicAuth): the basic authentication package which provides helper request handlers for handling authentication.

## Getting started

The Sisk core idea is to create a service that runs on the internet and follows the pattern you define. Moreover, Sisk is a framework that adapts to how you want it to work, not the other way around.

Due to its explicit nature, its behavior is predictable. The main differentiator from ASP.NET is that Sisk can be up and running in very few lines of code, avoiding unnecessary configurations, and requiring the minimum setup to get your server working. Additionally, it does not demand any additional .NET SDK packages to develop, as the base package of .NET 6 is sufficient to start your development with Sisk.

It can handle multiple requests asynchronously, provides useful tools to manage and accelerate web development.

```c#
using Sisk.Core.Http;
using Sisk.Core.Routing;

namespace myProgram;

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

## Main features

Sisk can do web development the way you want. Create MVC, MVVC, SOLID applications, or any other design pattern you're interested in.

- **Lightweight:** robust projects tested in small, low-cost, low-performance environments and got good results. The entire Sisk ecosystem is less than 500kb in size!
- **Open-source:** the entire Sisk ecosystem is open source, and all the libraries and technologies we use must be open source as well. Sisk is entirely distributed under the MIT License, which allows the commercial development.
- **Sustainable:** you are the one who makes the project, Sisk gives you the tools. Because it is open source, the community (including you) can maintain, fix bugs and improve Sisk over time.

## What is Sisk for?

You can create Restful applications, gRPC, Websockets, file servers, GraphQL, Entity Framework, and more - basically whatever you want. Sisk is an extremely modular and sustainable framework. Furthermore, its current development is intense, and there's much more to be added to Sisk, but the focus is to keep it a simple, easy-to-maintain, and enjoyable framework for developers to start projects of any size.

Sisk was also been tested in low-performance environments, like machines with less than 1GB of RAM, and it can process over twenty thousand requests per second. The code, from arrival on the server to the response, is extremely concise, with very few steps before reaching the client.

One of the pillars of developing with Sisk is compatibility with any machine that supports .NET, including those that do not require Native AOT. Some additional implementations are also provided to the Sisk ecosystem, such as porting projects to other machines with configuration files, a view-engine based on LISP, among others, served with packages beyond the Sisk core package. By design, Sisk is built to work with routers, but don't worry, you are not obligated to use them. Sisk will provide you with all the necessary infrastructure to create a secure application that doesn't obfuscate your code.

There's no need for excessive ceremony, fluff, or spending hours on boring documentation. Sisk is simple and elegant in its syntax, facilitating the development of fast and complex systems.

## But why not just use ASP.NET?

ASP.NET is an great and well-established web framework, and many features present in Sisk were inspired by it. However, Sisk focuses on simpler and more performant development, eliminating the need for installing additional components in your system, project, editor, etc. Sisk was designed to be straightforward and robust, enabling the creation of anything you desire.

Moreover, its development model allows you to choose how you want your development to be. You handle requests in a simple, efficient, explicit, and fast manner. Knowledge and understanding of HTTP are required if you want to do everything manually, and even then, Sisk can greatly simplify things with all the functions it provides in its core package.

Getting started with Sisk is easy. Those who already have experience with web development typically learn Sisk in one or two days. Our documentation is… let's say… very well-documented. You can find everything you need here. Additionally, our source code is open, so you have access to it.