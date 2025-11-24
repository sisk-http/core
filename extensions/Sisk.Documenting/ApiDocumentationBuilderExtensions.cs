// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ApiDocumentationBuilderExtensions.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Handlers;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Routing;

namespace Sisk.Documenting;

/// <summary>
/// Provides extension methods for configuring API documentation generation and exposure on an HTTP server host.
/// </summary>
public static class ApiDocumentationBuilderExtensions {

    class ApiDocumentationRouteServerHandler ( ApiGenerationContext context, string routerPath, IApiDocumentationExporter exporter ) : HttpServerHandler {

        public override int Priority => 100;

        public ApiGenerationContext Context { get; } = context;
        public string RouterPath { get; } = routerPath;
        public IApiDocumentationExporter Exporter { get; } = exporter;

        protected override void OnSetupRouter ( Router router ) {
            var documentation = ApiDocumentation.Generate ( router, Context );

            router.MapGet ( RouterPath, request => {
                return new HttpResponse () {
                    Content = Exporter.ExportDocumentationContent ( documentation )
                };
            } );
        }
    }

    /// <summary>
    /// Configures the application to expose generated API documentation at the specified router path.
    /// </summary>
    /// <param name="builder">The <see cref="HttpServerHostContextBuilder"/> to configure.</param>
    /// <param name="context">The <see cref="ApiGenerationContext"/> used to generate the documentation.</param>
    /// <param name="routerPath">The route at which the documentation will be served. Defaults to "/api/docs".</param>
    /// <param name="exporter">
    /// The <see cref="IApiDocumentationExporter"/> used to export the documentation content.
    /// If <see langword="null"/>, a default <see cref="OpenApiExporter"/> is used.
    /// </param>
    /// <returns>The same <see cref="HttpServerHostContextBuilder"/> instance for method chaining.</returns>
    public static HttpServerHostContextBuilder UseApiDocumentation ( this HttpServerHostContextBuilder builder, ApiGenerationContext context, string routerPath = "/api/docs", IApiDocumentationExporter? exporter = null ) {
        builder.UseHandler ( new ApiDocumentationRouteServerHandler ( context, routerPath, exporter ?? new OpenApiExporter () ) );
        return builder;
    }

    /// <summary>
    /// Configures the application to expose generated API documentation at the default router path "/api/docs"
    /// using an empty <see cref="ApiGenerationContext"/>.
    /// </summary>
    /// <param name="builder">The <see cref="HttpServerHostContextBuilder"/> to configure.</param>
    /// <returns>The same <see cref="HttpServerHostContextBuilder"/> instance for method chaining.</returns>
    public static HttpServerHostContextBuilder UseApiDocumentation ( this HttpServerHostContextBuilder builder ) {
        return UseApiDocumentation ( builder, new ApiGenerationContext () );
    }
}