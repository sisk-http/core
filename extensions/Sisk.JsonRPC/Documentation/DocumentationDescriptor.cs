// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   DocumentationDescriptor.cs
// Repository:  https://github.com/sisk-http/core

using System.Reflection;
using Sisk.JsonRPC.Annotations;

namespace Sisk.JsonRPC.Documentation;

internal class DocumentationDescriptor {
    internal static JsonRpcDocumentation GetDocumentationDescriptor ( JsonRpcHandler handler, JsonRpcDocumentationMetadata? metadata ) {

        List<JsonRpcDocumentationMethod> methods = new List<JsonRpcDocumentationMethod> ();

        foreach (var method in handler.Methods.methods) {
            var methodDocs = method.Value.Method.GetCustomAttribute<MethodDescriptionAttribute> ();
            var paramsDocs = method.Value.Method.GetCustomAttributes<ParamDescriptionAttribute> ();

            methods.Add ( new JsonRpcDocumentationMethod (
                methodName: method.Key,
                category: methodDocs?.Category,
                description: methodDocs?.Description,
                returnType: method.Value.Method.ReturnType,
                parameters: method.Value.Method.GetParameters ()
                    .Select ( p => new JsonRpcDocumentationParameter (
                        parameterName: p.Name!,
                        parameterType: p.ParameterType,
                        description: paramsDocs.FirstOrDefault ( f => f.ParameterName == p.Name )?.Description,
                        isOptional: p.IsOptional
                    ) )
                    .ToArray () ) );
        }

        return new JsonRpcDocumentation ( methods.OrderBy ( m => m.MethodName ).ToArray (), metadata );
    }
}
