// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MethodScanner.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Reflection;
using Sisk.JsonRPC.Annotations;

namespace Sisk.JsonRPC;

internal static class MethodScanner {

    public static IEnumerable<RpcDelegate> ScanMethods ( [DynamicallyAccessedMembers ( DynamicallyAccessedMemberTypes.All )] Type type, bool prefixTypes, object? target ) {
        const BindingFlags Flags = BindingFlags.Public | BindingFlags.Instance | BindingFlags.Static;

        var customTypeName = type.GetCustomAttribute<WebNameAttribute> ()?.Name;
        var methods = type.GetMethods ( Flags );
        for (int i = 0; i < methods.Length; i++) {
            var method = methods [ i ];
            WebMethodAttribute? methodName = method.GetCustomAttribute<WebMethodAttribute> ();

            if (methodName is not null) {

                string typeName = customTypeName ?? type.Name;
                string name = methodName.Name ?? method.Name;

                if (prefixTypes)
                    name = typeName + "." + name;

                yield return ParseDelegate ( method, target, name );
            }
        }
    }

    public static RpcDelegate ParseDelegate ( MethodInfo method, object? target, string? name = null ) {
        CheckIfMethodIsEligible ( method );

        name ??= method.Name;
        var parameters = GetParameters ( method ).ToArray ();

        bool _isAsyncTask = false, _isAsyncEnumerable = false;
        var retType = method.ReturnType;
        if (retType.IsValueType) {
            throw new NotSupportedException ( "Defining web methods which their return type is an value type is not supported. Encapsulate it with ValueResult<T>." );
        }
        else if (retType.IsAssignableTo ( typeof ( Task ) )) {
            _isAsyncTask = true;
            if (CheckAsyncReturnParameters ( retType, method ) is Exception rex) {
                throw rex;
            }
        }
        else if (retType.IsGenericType && retType.GetGenericTypeDefinition () == typeof ( IAsyncEnumerable<> )) {
            _isAsyncEnumerable = true;
            if (CheckAsyncReturnParameters ( retType, method ) is Exception rex) {
                throw rex;
            }
        }

        var returnInformation = new RpcDelegateMethodReturnInformation ( retType, _isAsyncEnumerable, _isAsyncTask );

        return new RpcDelegate ( name, method, parameters, returnInformation, target );
    }

    public static IEnumerable<RpcDelegateMethodParameter> GetParameters ( MethodInfo method ) {
        return method.GetParameters ()
            .Select ( p => new RpcDelegateMethodParameter ( p.ParameterType, p.Name, p.IsOptional, p.DefaultValue ) );
    }

    static void CheckIfMethodIsEligible ( MethodInfo method ) {
        var retType = method.ReturnType;
        if (retType.IsValueType) {
            throw new NotSupportedException ( "Defining web methods which their return type is an value type is not supported. Encapsulate it with ValueResult<T>." );
        }
        else if (retType.IsAssignableTo ( typeof ( Task ) )) {
            if (CheckAsyncReturnParameters ( retType, method ) is Exception rex) {
                throw rex;
            }
        }
        else if (retType.IsGenericType && retType.GetGenericTypeDefinition () == typeof ( IAsyncEnumerable<> )) {
            if (CheckAsyncReturnParameters ( retType, method ) is Exception rex) {
                throw rex;
            }
        }
    }

    static Exception? CheckAsyncReturnParameters ( Type asyncOutType, MethodInfo method ) {
        if (asyncOutType.GenericTypeArguments.Length == 1) {
            Type genericAssignType = asyncOutType.GenericTypeArguments [ 0 ];
            if (genericAssignType.IsValueType) {
                return new NotSupportedException ( "Defining web methods which their return type is an value type is not supported. Encapsulate it with ValueResult<T>." );
            }
        }
        return null;
    }
}
