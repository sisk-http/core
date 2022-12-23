using Sisk.Core.Routing.Handlers;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Specifies that the method, when used on this attribute, will instantiate the type and call the <see cref="IRequestHandler"/> with given parameters.
    /// </summary>
    /// <definition>
    /// [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    /// public class RequestHandlerAttribute : Attribute
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = true)]
    public class RequestHandlerAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the type that implements <see cref="IRequestHandler"/> which will be instantiated.
        /// </summary>
        /// <definition>
        /// public Type RequestHandlerType { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Type RequestHandlerType { get; set; }

        /// <summary>
        /// Specifies parameters for the given type's constructor.
        /// </summary>
        /// <definition>
        /// public object?[] ConstructorArguments { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public object?[] ConstructorArguments { get; set; }

        /// <summary>
        /// Creates a new instance of this attribute with the informed parameters.
        /// </summary>
        /// <param name="handledBy">The type that implements <see cref="IRequestHandler"/> which will be instantiated.</param>
        /// <definition>
        /// public RequestHandlerAttribute(Type handledBy)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public RequestHandlerAttribute(Type handledBy)
        {
            RequestHandlerType = handledBy;
            ConstructorArguments = new object?[] { };
        }
    }
}
