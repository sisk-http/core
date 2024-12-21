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

internal record class RpcDelegate ( MethodInfo Method, object? Target );
