<p align="center">
  <img width="100%" height="auto" src="./.github/Header.png">
</p>

<div align="center">

  <a href="">[![Nuget](https://img.shields.io/nuget/dt/Sisk.HttpServer)](https://www.nuget.org/packages/Sisk.HttpServer/)</a>
  <a href="">[![Nuget](https://img.shields.io/github/license/CypherPotato/Sisk)](https://github.com/CypherPotato/Sisk/blob/master/LICENSE.txt)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/.net%20version-.NET%206-blue.svg)](#)</a>
  <a href="">[![Nuget](https://img.shields.io/badge/platform-win%20|%20unix%20|%20osx-orange.svg)](https://github.com/CypherPotato/Sisk/blob/master/LICENSE.txt)</a>
</div>

------

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

You can access the Sisk documentation [here](https://sisk-http.github.io/docs/static/#/) or access it's repository [here](https://github.com/sisk-http/docs).
The specification is complete, however, tutorials are yet to come.

## Installation

You can install the latest release from Nuget:

```
PM> dotnet add package Sisk.HttpServer
```