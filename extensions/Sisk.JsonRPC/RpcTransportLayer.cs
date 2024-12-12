using LightJson;
using LightJson.Serialization;
using Sisk.Core.Http;
using Sisk.Core.Http.Streams;
using Sisk.Core.Routing;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Text;

namespace Sisk.JsonRPC;

/// <summary>
/// Provides transport layers for handling JSON-RPC communications.
/// </summary>
public sealed class RpcTransportLayer
{
    private readonly JsonRpcHandler _handler;

    internal RpcTransportLayer(JsonRpcHandler handler)
    {
        _handler = handler;
    }

    /// <summary>
    /// Gets the event handler for WebSocket message reception.
    /// </summary>
    public WebSocketMessageReceivedEventHandler WebSocket { get => new WebSocketMessageReceivedEventHandler(ImplWebSocket); }

    /// <summary>
    /// Gets the action to handle HTTP POST requests.
    /// </summary>
    public RouteAction HttpPost { get => new RouteAction(ImplTransportPostHttp); }

    /// <summary>
    /// Gets the action to handle HTTP GET requests.
    /// </summary>
    public RouteAction HttpGet { get => new RouteAction(ImplTransportGetHttp); }

    /// <summary>
    /// Gets the action to display general help for available web methods.
    /// </summary>
    public RouteAction HttpDescriptor { get => new RouteAction(ImplDescriptor); }

    void ImplWebSocket(object? sender, WebSocketMessage message)
    {
        JsonRpcRequest? rpcRequest = null;
        JsonRpcResponse response;

        string messageJson = message.GetString();

        if (!JsonValue.TryDeserialize(messageJson, _handler._jsonOptions, out JsonValue jsonRequestObject))
        {
            response = JsonRpcResponse.CreateErrorResponse(JsonValue.Null, new JsonRpcError(JsonErrorCode.InvalidRequest, "Invalid JSON-RPC message received."));

            string responseJson = JsonValue.Serialize(response, _handler._jsonOptions).ToString();
            message.Sender.Send(responseJson);

            return;
        }

        Task.Run(() =>
        {
            rpcRequest = new JsonRpcRequest(jsonRequestObject.GetJsonObject());
            response = HandleRpcRequest(rpcRequest);

            string responseJson = JsonValue.Serialize(response, _handler._jsonOptions).ToString();
            message.Sender.Send(responseJson);
        });
    }

    HttpResponse ImplDescriptor(HttpRequest request)
    {
        JsonValue result = DocumentationDescriptor.GetDocumentationDescriptor(_handler).AsJsonValue();
        JsonRpcResponse response = new JsonRpcResponse(result, null, JsonValue.Null);

        return new HttpResponse()
        {
            Status = HttpStatusInformation.Ok,
            Content = new StringContent(JsonValue.Serialize(response, _handler._jsonOptions).ToString(), Encoding.UTF8, "application/json")
        };
    }

    HttpResponse ImplTransportGetHttp(HttpRequest request)
    {
        JsonRpcRequest? rpcRequest = null;
        JsonRpcResponse response;

        try
        {
            string qmethod = request.Query["method"].GetString();
            string qparameters = request.Query["params"].GetString();
            string qid = request.Query["id"].GetString();

            rpcRequest = new JsonRpcRequest(qmethod, JsonValue.Deserialize(qparameters), qid);
            response = HandleRpcRequest(rpcRequest);
        }
        catch (Exception ex)
        {
            response = new JsonRpcResponse(null,
                new JsonRpcError(JsonErrorCode.InternalError, ex.Message, JsonValue.Null), rpcRequest?.Id ?? "0");
        }

        if (rpcRequest is not null && rpcRequest.Id.IsNull)
        {
            return new HttpResponse()
            {
                Status = HttpStatusInformation.Accepted
            };
        }
        else
        {
            return new HttpResponse()
            {
                Status = HttpStatusInformation.Ok,
                Content = new StringContent(JsonValue.Serialize(response, _handler._jsonOptions).ToString(), Encoding.UTF8, "application/json")
            };
        }
    }

    HttpResponse ImplTransportPostHttp(HttpRequest request)
    {
        JsonRpcRequest? rpcRequest = null;
        JsonRpcResponse response;

        if (request.Headers.ContentType?.Contains("application/json") == false)
        {
            response = JsonRpcResponse.CreateErrorResponse(JsonValue.Null, new JsonRpcError(JsonErrorCode.InvalidRequest, "The Content-Type must be 'application/json'."));
            goto sendResponse;
        }

        try
        {
            using var requestReader = new StreamReader(request.GetRequestStream(), request.RequestEncoding);
            using var jsonReader = new JsonReader(requestReader, _handler._jsonOptions);

            var jsonRequestObject = jsonReader.Parse().GetJsonObject();
            rpcRequest = new JsonRpcRequest(jsonRequestObject);

            response = HandleRpcRequest(rpcRequest);
        }
        catch (Exception ex)
        {
            response = new JsonRpcResponse(null,
                new JsonRpcError(JsonErrorCode.InternalError, ex.Message, JsonValue.Null), rpcRequest?.Id ?? "0");
        }

    sendResponse:
        if (rpcRequest is not null && rpcRequest.Id.IsNull)
        {
            return new HttpResponse()
            {
                Status = HttpStatusInformation.Accepted
            };
        }
        else
        {
            return new HttpResponse()
            {
                Status = HttpStatusInformation.Ok,
                Content = new StringContent(JsonValue.Serialize(response, _handler._jsonOptions).ToString(), Encoding.UTF8, "application/json")
            };
        }
    }

    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(Task<>))]
    [DynamicDependency(DynamicallyAccessedMemberTypes.PublicMethods, typeof(TaskAwaiter<>))]
    [System.Diagnostics.CodeAnalysis.SuppressMessage("Trimming", "IL2026:Using dynamic types might cause types or members to be removed by trimmer.", Justification = "<Pendente>")]
    JsonRpcResponse HandleRpcRequest(JsonRpcRequest request)
    {
        JsonRpcResponse response;
        try
        {
            string rpcMethod = request.Method;
            RpcDelegate? method = _handler.Methods.GetMethod(rpcMethod);

            if (method != null)
            {
                var methodInfo = method.Method;
                var methodParameters = methodInfo.GetParameters();

                object?[] methodInvokationParameters = new object?[methodParameters.Length];

                int optionalParameters = methodParameters.Count(c => c.IsOptional);
                int requiredParameterCount = methodInvokationParameters.Length - optionalParameters;
                int totalParameterCount = methodInvokationParameters.Length;

                if (methodParameters.Length == 1 && methodParameters[0].Name == "params")
                {
                    methodInvokationParameters[0] = request.Parameters.Get(methodParameters[0].ParameterType);
                }
                else if (request.Parameters.IsJsonArray)
                {
                    var jsonValueList = request.Parameters.GetJsonArray();

                    if (jsonValueList.Count < requiredParameterCount || jsonValueList.Count > totalParameterCount)
                    {
                        response = new JsonRpcResponse(null,
                            new JsonRpcError(JsonErrorCode.InvalidParams, $"Parameter count mismatch. The Method \"{rpcMethod}\" requires {requiredParameterCount} parameters, but we received {jsonValueList.Count}.", JsonValue.Null), request.Id);

                        return response;
                    }

                    for (int i = 0; i < jsonValueList.Count; i++)
                    {
                        methodInvokationParameters[i] = jsonValueList[i].MaybeNull()?.Get(methodParameters[i].ParameterType);
                    }
                }
                else
                {
                    var jsonValueObject = request.Parameters.GetJsonObject();
                    if (jsonValueObject.Count < requiredParameterCount || jsonValueObject.Count > totalParameterCount)
                    {
                        response = new JsonRpcResponse(null,
                            new JsonRpcError(JsonErrorCode.InvalidParams, $"Parameter count mismatch. The Method \"{rpcMethod}\" requires {requiredParameterCount} parameters, but we received {jsonValueObject.Count}.", JsonValue.Null), request.Id);

                        return response;
                    }

                    for (int i = 0; i < methodInvokationParameters.Length; i++)
                    {
                        var param = methodParameters[i];
                        string? paramName = param.Name;
                        if (paramName is null) continue;

                        var jsonParameter = jsonValueObject[paramName];

                        methodInvokationParameters[i] = jsonParameter.MaybeNull()?.Get(param.ParameterType);
                    }
                }

                object? result = methodInfo.Invoke(method.Target, methodInvokationParameters);

                if (result is Task task)
                {
                    result = ((dynamic)task).GetAwaiter().GetResult();
                }

                JsonValue resultEncoded = JsonValue.Serialize(result, _handler._jsonOptions);

                response = new JsonRpcResponse(resultEncoded, null, request.Id);
            }
            else
            {
                response = new JsonRpcResponse(null,
                    new JsonRpcError(JsonErrorCode.MethodNotFound, $"Method not found.", JsonValue.Null), request.Id);
            }
        }
        catch (JsonRpcException jex)
        {
            response = new JsonRpcResponse(null, jex.AsRpcError(), request.Id);
        }
        catch (Exception ex)
        {
            response = new JsonRpcResponse(null,
               new JsonRpcError(JsonErrorCode.InternalError, ex.Message, JsonValue.Null), request?.Id ?? "0");
        }
        return response;
    }
}
