Sisk is a powerful framework for building powerful web applications. It is written in .NET 6 and uses Microsoft-HTTPAPI/2.0 as their main http Engine.

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

> You can use Sisk with HTTPS, HTTP/2 and HTTP/3 QUIC if you follow [this Microsoft tutorial](https://learn.microsoft.com/en-us/iis/manage/configuring-security/how-to-set-up-ssl-on-iis). Requires installation of IIS on Windows.

## Documentation

The specification is complete, however, tutorials are yet to come. By the way, you can access the Sisk documentation [here](https://sisk-http.github.io/docs/static/#/).

## Installation

You can install the latest release from Nuget:

```
PM> dotnet add package Sisk.HttpServer
```