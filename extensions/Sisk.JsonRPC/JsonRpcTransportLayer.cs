// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcTransportLayer.cs
// Repository:  https://github.com/sisk-http/core

using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using LightJson;
using LightJson.Serialization;
using Sisk.Core.Http;
using Sisk.Core.Http.Streams;
using Sisk.Core.Routing;

namespace Sisk.JsonRPC;

/// <summary>
/// Provides transport layers for handling JSON-RPC communications.
/// </summary>
public sealed class JsonRpcTransportLayer {
    private readonly JsonRpcHandler _handler;

    internal JsonRpcTransportLayer ( JsonRpcHandler handler ) {
        _handler = handler;
    }

    /// <summary>
    /// Gets the event handler for WebSocket message reception.
    /// </summary>
    public RouteAction WebSocket { get => new RouteAction ( ImplWebSocket ); }

    /// <summary>
    /// Gets the action to handle HTTP POST requests.
    /// </summary>
    public RouteAction HttpPost { get => new RouteAction ( ImplTransportPostHttp ); }

    /// <summary>
    /// Gets the action to handle HTTP GET requests.
    /// </summary>
    public RouteAction HttpGet { get => new RouteAction ( ImplTransportGetHttp ); }

    async Task ImplWebSocket ( HttpRequest request ) {
        var websocket = await request.GetWebSocketAsync ();

        while (await websocket.ReceiveMessageAsync ( TimeSpan.FromMinutes ( 30 ) ) is { } message) {

            JsonRpcRequest? rpcRequest = null;
            JsonRpcResponse response;

            string messageJson = message.GetString ();

            if (await _handler._jsonOptions.TryDeserializeAsync ( messageJson ) is { Success: true } jsonRequestObject) {
                rpcRequest = new JsonRpcRequest ( jsonRequestObject.Result.GetJsonObject () );
                response = HandleRpcRequest ( rpcRequest );

                string responseJson = _handler._jsonOptions.SerializeJson ( response );
                await message.Sender.SendAsync ( responseJson );
            }
            else {
                response = JsonRpcResponse.CreateErrorResponse ( JsonValue.Null, new JsonRpcError ( JsonErrorCode.InvalidRequest, "Invalid JSON-RPC message received." ) );

                string responseJson = _handler._jsonOptions.SerializeJson ( response );
                await message.Sender.SendAsync ( responseJson );
            }
        }
    }

    HttpResponse ImplTransportGetHttp ( HttpRequest request ) {
        JsonRpcRequest? rpcRequest = null;
        JsonRpcResponse response;

        try {
            string qmethod = request.Query [ "method" ].GetString ();
            string qparameters = request.Query [ "params" ].GetString ();
            string qid = request.Query [ "id" ].GetString ();

            rpcRequest = new JsonRpcRequest ( qmethod, JsonValue.Deserialize ( qparameters ), qid );
            response = HandleRpcRequest ( rpcRequest );
        }
        catch (Exception ex) {
            response = new JsonRpcResponse ( null,
                new JsonRpcError ( JsonErrorCode.InternalError, ex.Message, JsonValue.Null ), rpcRequest?.Id ?? "0" );
        }

        if (rpcRequest is not null && rpcRequest.Id.IsNull) {
            return new HttpResponse () {
                Status = HttpStatusInformation.Accepted
            };
        }
        else {
            return new HttpResponse () {
                Status = HttpStatusInformation.Ok,
                Content = new StringContent ( JsonValue.Serialize ( response, _handler._jsonOptions ).ToString (), Encoding.UTF8, "application/json" )
            };
        }
    }

    HttpResponse ImplTransportPostHttp ( HttpRequest request ) {
        JsonRpcRequest? rpcRequest = null;
        JsonRpcResponse response;

        if (request.Headers.ContentType?.Contains ( "application/json" ) == false) {
            response = JsonRpcResponse.CreateErrorResponse ( JsonValue.Null, new JsonRpcError ( JsonErrorCode.InvalidRequest, "The Content-Type must be 'application/json'." ) );
            goto sendResponse;
        }

        try {
            using var requestReader = new StreamReader ( request.GetRequestStream (), request.RequestEncoding );
            using var jsonReader = new JsonReader ( requestReader, _handler._jsonOptions );

            var jsonRequestObject = jsonReader.Parse ().GetJsonObject ();
            rpcRequest = new JsonRpcRequest ( jsonRequestObject );

            response = HandleRpcRequest ( rpcRequest );
        }
        catch (Exception ex) {
            if (_handler._server.ServerConfiguration.ThrowExceptions) {
                throw;
            }
            else {
                response = new JsonRpcResponse ( null,
                    new JsonRpcError ( JsonErrorCode.InternalError, ex.Message, JsonValue.Null ), rpcRequest?.Id ?? "0" );
            }
        }

sendResponse:
        if (rpcRequest is not null && rpcRequest.Id.IsNull && response.Error is null) {
            return new HttpResponse () {
                Status = HttpStatusInformation.Accepted
            };
        }
        else {
            return new HttpResponse () {
                Status = HttpStatusInformation.Ok,
                Content = new StringContent ( JsonValue.Serialize ( response, _handler._jsonOptions ).ToString (), Encoding.UTF8, "application/json" )
            };
        }
    }

    [System.Diagnostics.CodeAnalysis.SuppressMessage ( "Trimming",
        "IL2072:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<Pending>" )]
    JsonRpcResponse HandleRpcRequest ( JsonRpcRequest request ) {
        JsonRpcResponse response;
        try {
            string rpcMethod = request.Method;
            RpcDelegate? method = _handler.Methods.GetMethod ( rpcMethod );

            if (method != null) {
                var methodInfo = method.Method;
                var methodParameters = method.Parameters;

                object? [] methodInvokationParameters = new object? [ methodParameters.Length ];

                int optionalParameters = methodParameters.Count ( c => c.IsOptional );
                int requiredParameterCount = methodInvokationParameters.Length - optionalParameters;
                int totalParameterCount = methodInvokationParameters.Length;

                if (methodParameters.Length == 1 && methodParameters [ 0 ].Name == "params") {
                    methodInvokationParameters [ 0 ] = request.Parameters.Get ( methodParameters [ 0 ].ParameterType );
                }
                else if (request.Parameters.IsJsonArray) {
                    var jsonValueList = request.Parameters.GetJsonArray ();

                    if (jsonValueList.Count < requiredParameterCount || jsonValueList.Count > totalParameterCount) {
                        response = new JsonRpcResponse ( null,
                            new JsonRpcError ( JsonErrorCode.InvalidParams, $"Parameter count mismatch. The Method \"{rpcMethod}\" requires at least {requiredParameterCount} parameters, but we received {jsonValueList.Count}.", JsonValue.Null ), request.Id );

                        return response;
                    }

                    for (int i = 0; i < jsonValueList.Count; i++) {
                        methodInvokationParameters [ i ] = jsonValueList [ i ].MaybeNull ()?.Get ( methodParameters [ i ].ParameterType ) ?? methodParameters [ i ].DefaultValue;
                    }
                }
                else {
                    var jsonValueObject = request.Parameters.GetJsonObject ();
                    if (jsonValueObject.Count < requiredParameterCount || jsonValueObject.Count > totalParameterCount) {
                        response = new JsonRpcResponse ( null,
                            new JsonRpcError ( JsonErrorCode.InvalidParams, $"Parameter count mismatch. The Method \"{rpcMethod}\" requires at least {requiredParameterCount} parameters, but we received {jsonValueObject.Count}.", JsonValue.Null ), request.Id );

                        return response;
                    }

                    for (int i = 0; i < methodInvokationParameters.Length; i++) {
                        var param = methodParameters [ i ];
                        string? paramName = param.Name;
                        if (paramName is null)
                            continue;

                        var jsonParameter = jsonValueObject [ paramName ];

                        if (jsonParameter.IsNull && !param.IsOptional && Nullable.GetUnderlyingType ( param.ParameterType ) == null) {
                            response = new JsonRpcResponse ( null,
                                new JsonRpcError ( JsonErrorCode.InvalidParams, $"Parameter \"{paramName}\" is required.", JsonValue.Null ), request.Id );
                            return response;
                        }

                        methodInvokationParameters [ i ] = jsonParameter.MaybeNull ()?.Get ( param.ParameterType ) ?? param.DefaultValue;
                    }
                }

                object? result = methodInfo.Invoke ( method.Target, methodInvokationParameters );

                if (result is not null) {
                    if (method.ReturnInformation.IsAsyncTask) {
                        ref Task<object> actionTask = ref Unsafe.As<object, Task<object>> ( ref result );
                        result = actionTask.GetAwaiter ().GetResult ();
                    }
                    else if (method.ReturnInformation.IsAsyncEnumerable) {
                        ref IAsyncEnumerable<object> asyncEnumerable = ref Unsafe.As<object, IAsyncEnumerable<object>> ( ref result );
                        result = asyncEnumerable.ToBlockingEnumerable ();
                    }
                }

                JsonValue resultEncoded = JsonValue.Serialize ( result, _handler._jsonOptions );
                response = new JsonRpcResponse ( resultEncoded, null, request.Id );
            }
            else {
                response = new JsonRpcResponse ( null,
                    new JsonRpcError ( JsonErrorCode.MethodNotFound, $"Method not found.", JsonValue.Null ), request.Id );
            }
        }
        catch (JsonRpcException jex) {
            response = new JsonRpcResponse ( null, jex.AsRpcError (), request.Id );
        }
        catch (Exception erx) {
            Exception ex = erx;
            if (erx is TargetInvocationException)
                ex = ex.InnerException ?? ex;

            if (_handler._server.ServerConfiguration.ThrowExceptions) {
                throw;
            }
            else {
                _handler._server.ServerConfiguration.ErrorsLogsStream?.WriteException ( erx );
                response = new JsonRpcResponse ( null,
                    new JsonRpcError ( JsonErrorCode.InternalError, ex.Message, JsonValue.Null ), request?.Id ?? "0" );
            }
        }
        return response;
    }
}
