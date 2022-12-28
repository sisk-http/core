using System.Collections.Specialized;

namespace Sisk.Core.Routing.Handlers
{
    /// <summary>
    /// Provides a class that instantiates request handlers, capable of porting them to Agirax.
    /// </summary>
    /// <definition>
    /// public abstract class RequestHandlerFactory
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing.Handlers
    /// </namespace>
    public abstract class RequestHandlerFactory
    {
        /// <summary>
        /// Builds and gets the <see cref="IRequestHandler"/> instance objects used later by an <see cref="Route"/>.
        /// </summary>
        /// <definition>
        /// public abstract IRequestHandler[] BuildRequestHandlers();
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public abstract IRequestHandler[] BuildRequestHandlers();

        /// <summary>
        /// Method that is called by the Agirax instantiator with the parameters defined in configuration before calling <see cref="BuildRequestHandlers()"/>.
        /// </summary>
        /// <param name="setupParameters">Parameters that are defined in a configuration file.</param>
        /// <definition>
        /// public abstract void Setup(NameValueCollection setupParameters);
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public abstract void Setup(NameValueCollection setupParameters);
    }
}
