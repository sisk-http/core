// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RpcDelegate.cs
// Repository:  https://github.com/sisk-http/core

using System.Reflection;

namespace Sisk.JsonRPC;

internal record class RpcDelegate ( string WebMethodName, MethodInfo Method, RpcDelegateMethodParameter [] Parameters, RpcDelegateMethodReturnInformation ReturnInformation, object? Target );
internal record class RpcDelegateMethodParameter ( Type ParameterType, string? Name, bool IsOptional, object? DefaultValue );
internal record class RpcDelegateMethodReturnInformation ( Type ReturnType, bool IsAsyncEnumerable, bool IsAsyncTask );