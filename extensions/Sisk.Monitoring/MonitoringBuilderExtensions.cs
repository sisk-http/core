using Sisk.Core.Http.Hosting;

namespace Sisk.Monitoring;

/// <summary>
/// Provides extension methods to register the Sisk monitoring dashboard within an <see cref="HttpServerHostContextBuilder"/>.
/// </summary>
public static class MonitoringBuilderExtensions {

    /// <summary>
    /// Registers a custom <see cref="ApplicationMonitor"/> implementation at the specified base path.
    /// </summary>
    /// <typeparam name="TApplicationMonitor">The concrete monitor type to instantiate.</typeparam>
    /// <param name="builder">The host context builder.</param>
    /// <param name="basePath">The URL prefix for monitoring routes.</param>
    /// <param name="factory">A factory function that creates the monitor instance.</param>
    /// <returns>The same <paramref name="builder"/> for method chaining.</returns>
    public static HttpServerHostContextBuilder UseMonitoring<TApplicationMonitor> ( this HttpServerHostContextBuilder builder, string basePath, Func<TApplicationMonitor> factory ) where TApplicationMonitor : ApplicationMonitor {
        var monitor = factory ();
        builder.UseRouter ( r => {
            foreach (var route in monitor.GetRoutes ( basePath ))
                r.SetRoute ( route );
        } );
        return builder;
    }

    /// <summary>
    /// Registers a custom <see cref="ApplicationMonitor"/> implementation at the default path "/monitoring".
    /// </summary>
    /// <typeparam name="TApplicationMonitor">The concrete monitor type to instantiate.</typeparam>
    /// <param name="builder">The host context builder.</param>
    /// <param name="factory">A factory function that creates the monitor instance.</param>
    /// <returns>The same <paramref name="builder"/> for method chaining.</returns>
    public static HttpServerHostContextBuilder UseMonitoring<TApplicationMonitor> ( this HttpServerHostContextBuilder builder, Func<TApplicationMonitor> factory ) where TApplicationMonitor : ApplicationMonitor {
        return UseMonitoring<TApplicationMonitor> ( builder, "/monitoring", factory );
    }

    /// <summary>
    /// Registers a default <see cref="ApplicationMonitor"/> instance configured via an action at the specified base path.
    /// </summary>
    /// <param name="builder">The host context builder.</param>
    /// <param name="basePath">The URL prefix for monitoring routes.</param>
    /// <param name="factory">An action that configures the monitor instance.</param>
    /// <returns>The same <paramref name="builder"/> for method chaining.</returns>
    public static HttpServerHostContextBuilder UseMonitoring ( this HttpServerHostContextBuilder builder, string basePath, Action<ApplicationMonitor> factory ) {
        var monitor = new ApplicationMonitor ();
        factory ( monitor );
        builder.UseRouter ( r => {
            foreach (var route in monitor.GetRoutes ( basePath ))
                r.SetRoute ( route );
        } );
        return builder;
    }

    /// <summary>
    /// Registers a default <see cref="ApplicationMonitor"/> instance configured via an action at the default path "/monitoring".
    /// </summary>
    /// <param name="builder">The host context builder.</param>
    /// <param name="factory">An action that configures the monitor instance.</param>
    /// <returns>The same <paramref name="builder"/> for method chaining.</returns>
    public static HttpServerHostContextBuilder UseMonitoring ( this HttpServerHostContextBuilder builder, Action<ApplicationMonitor> factory ) {
        return UseMonitoring ( builder, "/monitoring", factory );
    }
}