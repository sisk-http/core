using System.Reflection;
using Microsoft.OpenApi.Models;
using Microsoft.OpenApi.Writers;
using Sisk.Core.Routing;

namespace Sisk.Helpers.Swagger;

public class OpenApiGenerator {
    public static void GenerateOpenApiDocument ( Router router ) {
        var routes = router.GetDefinedRoutes ();
        var openApiDocument = new OpenApiDocument ();
        openApiDocument.Paths = new OpenApiPaths ();
        openApiDocument.Info = new OpenApiInfo () {
            Version = "1.0.0",
            Title = "Teste"
        };

        var routeGrouped = routes.GroupBy ( g => g.Path );
        foreach (var routeGroup in routeGrouped) {
            string path = routeGroup.Key;
            Dictionary<OperationType, OpenApiOperation> operations = new Dictionary<OperationType, OpenApiOperation> ( routeGroup.Count () );

            foreach (var route in routeGroup) {
                var method = route.Action?.Method;
                if (method is null)
                    continue;

                var apiDefinitionAttr = method.GetCustomAttribute<ApiDefinitionAttribute> ();
                var apiResponseAttrs = method.GetCustomAttributes<ApiResponseAttribute> ();

                var description = apiDefinitionAttr?.Description;

                var operationType = route.Method switch {
                    RouteMethod.Get => OperationType.Get,
                    RouteMethod.Post => OperationType.Post,
                    RouteMethod.Put => OperationType.Put,
                    RouteMethod.Patch => OperationType.Patch,
                    RouteMethod.Delete => OperationType.Delete,
                    RouteMethod.Head => OperationType.Head,
                    RouteMethod.Options => OperationType.Options,
                    _ => OperationType.Get // should ignore/throw? need handle this
                };

                OpenApiResponses responses = new OpenApiResponses ();
                foreach (var apiResponseAttr in apiResponseAttrs) {
                    responses.Add ( apiResponseAttr.StatusCode.ToString (), new OpenApiResponse () {
                        Description = apiResponseAttr?.Description,
                        Content = new Dictionary<string, OpenApiMediaType> () {
                        }
                    } );
                }

                operations.Add ( operationType, new OpenApiOperation () {
                    Description = description,
                    Responses = new OpenApiResponses () {
                        [ "200" ] = new OpenApiResponse () {
                            Description = "200",

                        }
                    }
                } );
            }

            openApiDocument.Paths.Add ( path, new OpenApiPathItem () {
                Operations = operations
            } );
        }

        using var sw = new StringWriter ();
        openApiDocument.SerializeAsV3 ( new OpenApiJsonWriter ( sw ) );

        string json = sw.ToString ();

        ;

        //var document = new OpenApiDocument
        //{
        //    Info = new OpenApiInfo
        //    {
        //        Version = "1.0.0",
        //        Title = "Swagger Petstore (Simple)",
        //    },
        //    Servers = new List<OpenApiServer>
        //    {
        //        new OpenApiServer { Url = "http://petstore.swagger.io/api" }
        //    },
        //    Paths = new OpenApiPaths
        //    {
        //        ["/pets"] = new OpenApiPathItem
        //        {
        //            Operations = new Dictionary<OperationType, OpenApiOperation>
        //            {
        //                [OperationType.Get] = new OpenApiOperation
        //                {
        //                    Description = "Returns all pets from the system that the user has access to",
        //                    Responses = new OpenApiResponses
        //                    {
        //                        ["200"] = new OpenApiResponse
        //                        {
        //                            Description = "OK"
        //                        }
        //                    }
        //                }
        //            }
        //        }
        //    }
        //};
    }
}
