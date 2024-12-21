// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DocumentationDescriptor.cs
// Repository:  https://github.com/sisk-http/core

using System.Reflection;
using LightJson;
using Sisk.JsonRPC.Annotations;

namespace Sisk.JsonRPC;
internal class DocumentationDescriptor {
    internal static JsonArray GetDocumentationDescriptor ( JsonRpcHandler handler ) {

        JsonArray arr = new JsonArray ();

        foreach (var method in handler.Methods.methods) {
            var methodDocs = method.Value.Method.GetCustomAttribute<MethodDescriptionAttribute> ();
            var paramsDocs = method.Value.Method.GetCustomAttributes<ParamDescriptionAttribute> ();

            var item = new {
                name = method.Key,
                description = methodDocs?.Description,
                returns = method.Value.Method.ReturnType.Name,
                parameters = method.Value
                    .Method.GetParameters ()
                    .Select ( p => new {
                        name = p.Name,
                        typeName = p.ParameterType.Name,
                        description = paramsDocs.FirstOrDefault ( f => f.ParameterName == p.Name )?.Description,
                        isOptional = p.IsOptional
                    } )
                    .ToArray ()
            };

            arr.Add ( JsonValue.Serialize ( item, handler.JsonSerializerOptions ) );
        }

        return arr;
    }
}
