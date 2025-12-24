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

    public static ApiDocumentation ReadDocumentation ( ApiGenerationContext context, Router router ) {
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
            List<ApiEndpointQueryParameter> queryParameters = new List<ApiEndpointQueryParameter> ();

            foreach (var requestHandler in route.RequestHandlers) {
                MethodInfo? rhMethod = ExtractRhExecute ( requestHandler );
                if (rhMethod is null)
                    continue;

                var rhAttrs = ExtractAttributesFromMethod ( rhMethod );
                foreach (var apiResAttr in rhAttrs.Item1)
                    responses.Add ( apiResAttr.GetApiEndpointObject ( context ) );
                foreach (var apiParamAttr in rhAttrs.Item2)
                    parameters.Add ( apiParamAttr.GetApiEndpointObject () );
                foreach (var apiParamFromAttr in rhAttrs.Item3)
                    parameters.AddRange ( apiParamFromAttr.GetParameters ( context ) );
                foreach (var apiHeaderAttr in rhAttrs.Item4)
                    headers.Add ( apiHeaderAttr.GetApiEndpointObject () );
                foreach (var apiPathParam in rhAttrs.Item5)
                    pathParameters.Add ( apiPathParam.GetApiEndpointObject () );
                foreach (var apiReq in rhAttrs.Item6)
                    requests.Add ( apiReq.GetApiEndpointObject ( context ) );
                foreach (var apiReq in rhAttrs.Item7)
                    queryParameters.Add ( apiReq.GetApiEndpointObject () );
            }

            var attrs = ExtractAttributesFromMethod ( routeMethod );
            foreach (var apiResAttr in attrs.Item1)
                responses.Add ( apiResAttr.GetApiEndpointObject ( context ) );
            foreach (var apiParamAttr in attrs.Item2)
                parameters.Add ( apiParamAttr.GetApiEndpointObject () );
            foreach (var apiParamFromAttr in attrs.Item3)
                parameters.AddRange ( apiParamFromAttr.GetParameters ( context ) );
            foreach (var apiHeaderAttr in attrs.Item4)
                headers.Add ( apiHeaderAttr.GetApiEndpointObject () );
            foreach (var apiPathParam in attrs.Item5)
                pathParameters.Add ( apiPathParam.GetApiEndpointObject () );
            foreach (var apiReqParam in attrs.Item6)
                requests.Add ( apiReqParam.GetApiEndpointObject ( context ) );
            foreach (var apiReqParam in attrs.Item7)
                queryParameters.Add ( apiReqParam.GetApiEndpointObject () );

            string endpointName = apiEndpointAttr.Name;
            if (string.IsNullOrEmpty ( endpointName ))
                endpointName = route.Name ?? route.Action?.Method.Name ?? "(untitled endpoint)";


            ApiEndpoint endpoint = new ApiEndpoint () {
                Description = apiEndpointAttr.Description,
                Group = apiEndpointAttr.Group,
                Name = endpointName,
                Path = route.Path,
                RouteMethod = route.Method,
                Headers = headers.ToArray (),
                Parameters = parameters.ToArray (),
                Responses = responses.ToArray (),
                PathParameters = pathParameters.ToArray (),
                RequestExamples = requests.ToArray (),
                QueryParameters = queryParameters.ToArray (),
                Order = apiEndpointAttr.Order
            };

            endpoints.Add ( endpoint );
        }

        return new ApiDocumentation () {
            ApiVersion = context.ApplicationVersion,
            ApplicationDescription = context.ApplicationDescription,
            ApplicationName = context.ApplicationName,
            Endpoints = endpoints
                .OrderBy ( e => e.Order )
                .ThenByDescending ( e => e.Path )
                .ToArray (),
        };
    }

    static (ApiResponseAttribute [], ApiParameterAttribute [], ApiParametersFromAttribute [], ApiHeaderAttribute [], ApiPathParameterAttribute [], ApiRequestAttribute [], ApiQueryParameterAttribute []) ExtractAttributesFromMethod ( MethodInfo method ) {
        var apiResponsesAttrs = method.GetCustomAttributes<ApiResponseAttribute> ().ToArray ();
        var apiParametersAttrs = method.GetCustomAttributes<ApiParameterAttribute> ().ToArray ();
        var apiParametersFromAttrs = method.GetCustomAttributes<ApiParametersFromAttribute> ().ToArray ();
        var apiHeadersAttrs = method.GetCustomAttributes<ApiHeaderAttribute> ().ToArray ();
        var apiPathParamsAttrs = method.GetCustomAttributes<ApiPathParameterAttribute> ().ToArray ();
        var apiRequestsAttrs = method.GetCustomAttributes<ApiRequestAttribute> ().ToArray ();
        var apiQueryParamsAttrs = method.GetCustomAttributes<ApiQueryParameterAttribute> ().ToArray ();
        return (apiResponsesAttrs, apiParametersAttrs, apiParametersFromAttrs, apiHeadersAttrs, apiPathParamsAttrs, apiRequestsAttrs, apiQueryParamsAttrs);
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
