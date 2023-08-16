// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteDefinition.cs
// Repository:  https://github.com/sisk-http/core

using System.Reflection;

namespace Sisk.Core.Routing;

internal class RouteDefinition
{
    public RouteMethod Method { get; set; }
    public string Path { get; set; }

    public RouteDefinition(RouteMethod method, String path)
    {
        Method = method;
        Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public static RouteDefinition GetFromCallback(RouterCallback callback)
    {
        RouteAttribute? callbackType = callback.GetMethodInfo().GetCustomAttribute<RouteAttribute>(true);
        if (callbackType == null)
        {
            throw new InvalidOperationException("No route definition was found for the given callback. It may be possible that the " +
                "informed method does not implement the RouteAttribute attribute.");
        }
        else
        {
            return new RouteDefinition(callbackType.Method, callbackType.Path);
        }
    }
}
