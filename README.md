<p align="center">
  <img width="100%" height="auto" src="./.github/Header.png">
</p>

<div align="center">

  <a href="">[![Nuget](https://img.shields.io/nuget/dt/Sisk.HttpServer)](https://www.nuget.org/packages/Sisk.HttpServer/)</a>
  <a href="">[![Nuget](https://img.shields.io/github/license/sisk-http/core)](https://github.com/sisk-http/core/blob/master/LICENSE.txt)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/.net%20version-.NET%206-blue.svg)](#)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/platform-win%20|%20unix%20|%20osx-orange.svg)](#)</a>
</div>

------

Sisk is an cross-platform web framework for building powerful web applications and cloud services. It is written in C# with .NET 6 and uses Microsoft-HTTPAPI/2.0 as their main HTTP engine. Compatible with Native AOT and .NET runtime.

It can handle multiple requests asynchronously, provides useful tools to manage and accelerate web development.

```c#
using Sisk.Core.Http;
using Sisk.Core.Routing;

namespace myProgram;

class Program
{
    static void Main(string[] args)
    {
        Router router = new Router();
        HttpServerConfiguration config = new HttpServerConfiguration();
        config.ListeningHosts.Add(new ListeningHost("localhost", 5555, router));
        HttpServer server = new HttpServer(config);

        router.SetRoute(RouteMethod.Get, "/", (req) =>
        {
            return req.CreateOkResponse("Hello, world!");
        });

        server.Start();
        Thread.Sleep(-1); // prevent hauting
    }
}
```

## Features

- Multi-platform and cross-operating system
- Ultra fast response/second average
- Support to operating system's native HTTP interface listener
- Sustainable and maintainable
- Database-agnostic
- Same code implementation for *nix, Mac OS and Windows
- Asynchronous request handling
- Middlewares, before and/or after request handlers
- Configurable error handling
- Support to log access/error logs
- Easy Cross-Origin Resource Sharing setup
- Written in C#

## Documentation

You can access the Sisk documentation [here](https://sisk.proj.pw/) or access it's repository [here](https://github.com/sisk-http/docs).

## Installation

You can install the latest release from Nuget packages:

```
PM> dotnet add package Sisk.HttpServer
```
