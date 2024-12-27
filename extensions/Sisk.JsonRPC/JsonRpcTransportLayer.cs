// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   JsonRpcTransportLayer.cs
// Repository:  https://github.com/sisk-http/core

using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;
using LightJson;
using LightJson.Serialization;
using Sisk.Core.Http;
using Sisk.Core.Http.Streams;
using Sisk.Core.Routing;
using Sisk.JsonRPC.Documentation;

namespace Sisk.JsonRPC;

/// <summary>
/// Provides transport layers for handling JSON-RPC communications.
/// </summary>
public sealed class JsonRpcTransportLayer {
    private readonly JsonRpcHandler _handler;

    internal JsonRpcTransportLayer ( JsonRpcHandler handler ) {
        this._handler = handler;
    }

    /// <summary>
    /// Gets the event handler for WebSocket message reception.
    /// </summary>
    public WebSocketMessageReceivedEventHandler WebSocket { get => new WebSocketMessageReceivedEventHandler ( this.ImplWebSocket ); }

    /// <summary>
    /// Gets the action to handle HTTP POST requests.
    /// </summary>
    public RouteAction HttpPost { get => new RouteAction ( this.ImplTransportPostHttp ); }

    /// <summary>
    /// Gets the action to handle HTTP GET requests.
    /// </summary>
    public RouteAction HttpGet { get => new RouteAction ( this.ImplTransportGetHttp ); }

    /// <summary>
    /// Gets the action to display general help for available web methods.
    /// </summary>
    public RouteAction HttpDescriptor { get => new RouteAction ( this.ImplDescriptor ); }

    void ImplWebSocket ( object? sender, WebSocketMessage message ) {
        JsonRpcRequest? rpcRequest = null;
        JsonRpcResponse response;

        string messageJson = message.GetString ();

        if (!JsonValue.TryDeserialize ( messageJson, this._handler._jsonOptions, out JsonValue jsonRequestObject )) {
            response = JsonRpcResponse.CreateErrorResponse ( JsonValue.Null, new JsonRpcError ( JsonErrorCode.InvalidRequest, "Invalid JSON-RPC message received." ) );

            string responseJson = JsonValue.Serialize ( response, this._handler._jsonOptions ).ToString ();
            message.Sender.Send ( responseJson );

            return;
        }

        Task.Run ( () => {
            rpcRequest = new JsonRpcRequest ( jsonRequestObject.GetJsonObject () );
            response = this.HandleRpcRequest ( rpcRequest );

            string responseJson = JsonValue.Serialize ( response, this._handler._jsonOptions ).ToString ();
            message.Sender.Send ( responseJson );
        } );
    }

    HttpResponse ImplDescriptor ( HttpRequest request ) {
        var documentation = DocumentationDescriptor.GetDocumentationDescriptor ( this._handler );
        JsonValue result = new JsonRpcJsonExport ( this._handler._jsonOptions ).EncodeDocumentation ( documentation );
        JsonRpcResponse response = new JsonRpcResponse ( result, null, JsonValue.Null );

        return new HttpResponse () {
            Status = HttpStatusInformation.Ok,
            Content = new StringContent ( JsonValue.Serialize ( response, this._handler._jsonOptions ).ToString (), Encoding.UTF8, "application/json" )
        };
    }

    HttpResponse ImplTransportGetHttp ( HttpRequest request ) {
        JsonRpcRequest? rpcRequest = null;
        JsonRpcResponse response;

        try {
            string qmethod = request.Query [ "method" ].GetString ();
            string qparameters = request.Query [ "params" ].GetString ();
            string qid = request.Query [ "id" ].GetString ();

            rpcRequest = new JsonRpcRequest ( qmethod, JsonValue.Deserialize ( qparameters ), qid );
            response = this.HandleRpcRequest ( rpcRequest );
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
                Content = new StringContent ( JsonValue.Serialize ( response, this._handler._jsonOptions ).ToString (), Encoding.UTF8, "application/json" )
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
            using var jsonReader = new JsonReader ( requestReader, this._handler._jsonOptions );

            var jsonRequestObject = jsonReader.Parse ().GetJsonObject ();
            rpcRequest = new JsonRpcRequest ( jsonRequestObject );

            response = this.HandleRpcRequest ( rpcRequest );
        }
        catch (Exception ex) {
            response = new JsonRpcResponse ( null,
                new JsonRpcError ( JsonErrorCode.InternalError, ex.Message, JsonValue.Null ), rpcRequest?.Id ?? "0" );
        }

sendResponse:
        if (rpcRequest is not null && rpcRequest.Id.IsNull) {
            return new HttpResponse () {
                Status = HttpStatusInformation.Accepted
            };
        }
        else {
            return new HttpResponse () {
                Status = HttpStatusInformation.Ok,
                Content = new StringContent ( JsonValue.Serialize ( response, this._handler._jsonOptions ).ToString (), Encoding.UTF8, "application/json" )
            };
        }
    }

    [DynamicDependency ( DynamicallyAccessedMemberTypes.PublicMethods, typeof ( Task<> ) )]
    [DynamicDependency ( DynamicallyAccessedMemberTypes.PublicMethods, typeof ( TaskAwaiter<> ) )]
    [SuppressMessage ( "Trimming", "IL2026:Using dynamic types might cause types or members to be removed by trimmer.", Justification = "<Pendente>" )]
    [SuppressMessage ( "Trimming", "IL2072:Target parameter argument does not satisfy 'DynamicallyAccessedMembersAttribute' in call to target method. The return value of the source method does not have matching annotations.", Justification = "<Pending>" )]
    JsonRpcResponse HandleRpcRequest ( JsonRpcRequest request ) {
        JsonRpcResponse response;
        try {
            string rpcMethod = request.Method;
            RpcDelegate? method = this._handler.Methods.GetMethod ( rpcMethod );

            if (method != null) {
                var methodInfo = method.Method;
                var methodParameters = methodInfo.GetParameters ();

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
                        methodInvokationParameters [ i ] = jsonValueList [ i ].MaybeNull ()?.Get ( methodParameters [ i ].ParameterType );
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

                        methodInvokationParameters [ i ] = jsonParameter.MaybeNull ()?.Get ( param.ParameterType );
                    }
                }

                object? result = methodInfo.Invoke ( method.Target, methodInvokationParameters );

                if (result is Task task) {
                    result = ((dynamic) task).GetAwaiter ().GetResult ();
                }

                JsonValue resultEncoded = JsonValue.Serialize ( result, this._handler._jsonOptions );

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
        catch (Exception ex) {
            response = new JsonRpcResponse ( null,
               new JsonRpcError ( JsonErrorCode.InternalError, ex.Message, JsonValue.Null ), request?.Id ?? "0" );
        }
        return response;
    }
}
