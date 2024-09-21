// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteDefinition.cs
// Repository:  https://github.com/sisk-http/core

using System.Reflection;

namespace Sisk.Core.Routing;

// TODO: remove this class in future
internal class RouteDefinition
{
    public RouteMethod Method { get; set; }
    public string Path { get; set; }

    public RouteDefinition(RouteMethod method, String path)
    {
        this.Method = method;
        this.Path = path ?? throw new ArgumentNullException(nameof(path));
    }

    public static RouteDefinition GetFromCallback(RouteAction action)
    {
        RouteAttribute? callbackType = action.GetMethodInfo().GetCustomAttribute<RouteAttribute>(true);
        if (callbackType is null)
        {
            throw new InvalidOperationException(SR.Router_RouteDefinitionNotFound);
        }
        else
        {
            return new RouteDefinition(callbackType.Method, callbackType.Path);
        }
    }
}
