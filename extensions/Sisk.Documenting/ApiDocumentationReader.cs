// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiDocumentationReader.cs
// Repository:  https://github.com/sisk-http/core

using System.Reflection;
using Sisk.Core.Routing;
using Sisk.Documenting.Annotations;

namespace Sisk.Documenting;

internal class ApiDocumentationReader {

    public static ApiDocumentation ReadDocumentation ( ApiIdentifier identifier, Router router ) {
        var routes = router.GetDefinedRoutes ();

        List<ApiEndpoint> endpoints = new ( routes.Length );

        foreach (var route in routes) {
            var routeMethod = route.Action?.Method;
            if (routeMethod is null)
                continue;

            var apiEndpointAttr = routeMethod.GetCustomAttribute<ApiEndpointAttribute> ();
            if (apiEndpointAttr is null)
                continue;

            List<ApiEndpointResponse> responses = new List<ApiEndpointResponse> ();
            List<ApiEndpointParameter> parameters = new List<ApiEndpointParameter> ();
            List<ApiEndpointHeader> headers = new List<ApiEndpointHeader> ();
            List<ApiEndpointPathParameter> pathParameters = new List<ApiEndpointPathParameter> ();
            List<ApiEndpointRequestExample> requests = new List<ApiEndpointRequestExample> ();

            foreach (var requestHandler in route.RequestHandlers) {
                MethodInfo? rhMethod = ExtractRhExecute ( requestHandler );
                if (rhMethod is null)
                    continue;

                var rhAttrs = ExtractAttributesFromMethod ( rhMethod );
                foreach (var apiResAttr in rhAttrs.Item1)
                    responses.Add ( apiResAttr.GetApiEndpointObject () );
                foreach (var apiParamAttr in rhAttrs.Item2)
                    parameters.Add ( apiParamAttr.GetApiEndpointObject () );
                foreach (var apiHeaderAttr in rhAttrs.Item3)
                    headers.Add ( apiHeaderAttr.GetApiEndpointObject () );
                foreach (var apiPathParam in rhAttrs.Item4)
                    pathParameters.Add ( apiPathParam.GetApiEndpointObject () );
                foreach (var apiReq in rhAttrs.Item5)
                    requests.Add ( apiReq.GetApiEndpointObject () );
            }

            var attrs = ExtractAttributesFromMethod ( routeMethod );
            foreach (var apiResAttr in attrs.Item1)
                responses.Add ( apiResAttr.GetApiEndpointObject () );
            foreach (var apiParamAttr in attrs.Item2)
                parameters.Add ( apiParamAttr.GetApiEndpointObject () );
            foreach (var apiHeaderAttr in attrs.Item3)
                headers.Add ( apiHeaderAttr.GetApiEndpointObject () );
            foreach (var apiPathParam in attrs.Item4)
                pathParameters.Add ( apiPathParam.GetApiEndpointObject () );
            foreach (var apiReqParam in attrs.Item5)
                requests.Add ( apiReqParam.GetApiEndpointObject () );

            ApiEndpoint endpoint = new ApiEndpoint () {
                Description = apiEndpointAttr.Description,
                Group = apiEndpointAttr.Group,
                Name = apiEndpointAttr.Name,
                Path = route.Path,
                RouteMethod = route.Method,
                Headers = headers.ToArray (),
                Parameters = parameters.ToArray (),
                Responses = responses.ToArray (),
                PathParameters = pathParameters.ToArray (),
                RequestExamples = requests.ToArray ()
            };

            endpoints.Add ( endpoint );
        }

        return new ApiDocumentation () {
            ApiVersion = identifier.ApplicationVersion,
            ApplicationDescription = identifier.ApplicationDescription,
            ApplicationName = identifier.ApplicationName,
            Endpoints = endpoints.ToArray (),
        };
    }

    static (ApiResponseAttribute [], ApiParameterAttribute [], ApiHeaderAttribute [], ApiPathParameterAttribute [], ApiRequestAttribute []) ExtractAttributesFromMethod ( MethodInfo method ) {
        var apiResponsesAttrs = method.GetCustomAttributes<ApiResponseAttribute> ().ToArray ();
        var apiParametersAttrs = method.GetCustomAttributes<ApiParameterAttribute> ().ToArray ();
        var apiHeadersAttrs = method.GetCustomAttributes<ApiHeaderAttribute> ().ToArray ();
        var apiPathParamsAttrs = method.GetCustomAttributes<ApiPathParameterAttribute> ().ToArray ();
        var apiRequestsAttrs = method.GetCustomAttributes<ApiRequestAttribute> ().ToArray ();
        return (apiResponsesAttrs, apiParametersAttrs, apiHeadersAttrs, apiPathParamsAttrs, apiRequestsAttrs);
    }

    static MethodInfo? ExtractRhExecute ( IRequestHandler rh ) {
        var rhType = rh.GetType ();
        if (rh is AsyncRequestHandler) {
            return rhType.GetMethod ( "ExecuteAsync", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
        }
        else {
            return rhType.GetMethod ( "Execute", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance );
        }
    }
}
