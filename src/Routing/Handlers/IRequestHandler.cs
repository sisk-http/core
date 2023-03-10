using Sisk.Core.Http;

namespace Sisk.Core.Routing.Handlers
{
    /// <summary>
    /// Represents an interface that is executed before a request.
    /// </summary>
    /// <definition>
    /// public interface IRequestHandler
    /// </definition>
    /// <type>
    /// Interface
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing.Handlers
    /// </namespace>
    public interface IRequestHandler
    {
        /// <summary>
        /// This method is called by the <see cref="Router"/> before executing a request when the <see cref="Route"/> instantiates an object that implements this interface. If it returns
        /// a <see cref="HttpResponse"/> object, the route callback is not called and all execution of the route is stopped. If it returns "null", the execution is continued.
        /// </summary>
        /// <param name="request">The entry HTTP request.</param>
        /// <param name="context">The HTTP request context. It may contain information from other <see cref="IRequestHandler"/>.</param>
        /// <returns></returns>
        /// <definition>
        /// HttpResponse? Execute(HttpRequest request, HttpContext context);
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        HttpResponse? Execute(HttpRequest request, HttpContext context);

        /// <summary>
        /// Gets or sets the unique identifier of the instance of this interface.
        /// </summary>
        /// <definition>
        /// string Identifier { get; init; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        string Identifier { get; init; }

        /// <summary>
        /// Gets or sets when this RequestHandler should run.
        /// </summary>
        /// <definition>
        /// RequestHandlerExecutionMode ExecutionMode { get; init; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        RequestHandlerExecutionMode ExecutionMode { get; init; }
    }

    /// <summary>
    /// Defines when the <see cref="IRequestHandler"/> object should be executed.
    /// </summary>
    /// <definition>
    /// public enum RequestHandlerExecutionMode
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing.Handlers
    /// </namespace>
    public enum RequestHandlerExecutionMode
    {
        /// <summary>
        /// Indicates that the handler must be executed before the router calls the route callback.
        /// </summary>
        BeforeResponse,

        /// <summary>
        /// Indicates that the handler must be executed after the route callback execution.
        /// </summary>
        AfterResponse
    }
}
