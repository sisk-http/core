// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.JsonRPC;

internal static class SR {
    public const string JsonRpcError_DefaultMessage = "An exception was thrown.";

    public const string JsonRpcRequest_MethodEmpty = "The JSON-RPC request Method cannot be null or an empty string.";
    public const string JsonRpcRequest_ParametersInvalid = "The JSON-RPC request parameters must be an array or an object.";
    public const string JsonRpcRequest_IdInvalid = "The JSON-RPC request id must be an string, number or null.";
    public const string JsonRpcRequest_VersionInvalid = "The JSON-RPC request version must be \"2.0\".";

    public const string JsonRpcTransportLayer_InvalidMessage = "Invalid JSON-RPC message received.";
    public const string JsonRpcTransportLayer_InvalidContentType = "The Content-Type must be 'application/json'.";
    public const string JsonRpcTransportLayer_ParameterCountMismatch = "Parameter count mismatch. The Method \"{0}\" requires at least {1} parameters, but we received {2}.";
    public const string JsonRpcTransportLayer_ParameterRequired = "Parameter \"{0}\" is required.";
    public const string JsonRpcTransportLayer_MethodNotFound = "Method not found.";

    public const string MethodScanner_ValueTypeReturn = "Defining web methods which their return type is an value type is not supported. Encapsulate it with ValueResult<T>.";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
