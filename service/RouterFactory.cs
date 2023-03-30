using Sisk.Core.Http;
using System.Collections.Specialized;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Provides a class that instantiates a router, capable of porting applications for use with Agirax.
    /// </summary>
    /// <definition>
    /// public abstract class RouterFactory
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public abstract class RouterFactory
    {
        /// <summary>
        /// Build and gets a router that will be used later by an <see cref="ListeningHost"/>.
        /// </summary>
        /// <definition>
        /// public abstract Router BuildRouter();
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public abstract Router BuildRouter();

        /// <summary>
        /// Method that is called by the Agirax instantiator with the parameters defined in configuration before calling <see cref="BuildRouter()"/>.
        /// </summary>
        /// <param name="setupParameters">Parameters that are defined in a configuration file.</param>
        /// <definition>
        /// public abstract void Setup(NameValueCollection setupParameters);
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public abstract void Setup(NameValueCollection setupParameters);
    }
}
