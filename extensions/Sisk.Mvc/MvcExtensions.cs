// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MvcExtensions.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http.Hosting;

namespace Sisk.Mvc;

public static class MvcExtensions
{
    public static HttpServerHostContextBuilder UseMvc(this HttpServerHostContextBuilder builder)
    {
        return builder.UseHandler<MvcServerHandler>();
    }
}