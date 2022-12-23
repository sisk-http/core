namespace Sisk.Core.Entity
{
    /// <summary>
    /// Provides a class to provide Cross Origin response headers for when communicating with a browser.
    /// </summary>
    /// <definition>
    /// public class CrossOriginResourceSharingHeaders
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Entity
    /// </namespace>
    public class CrossOriginResourceSharingHeaders
    {
        /// <summary>
        /// Gets an instance of CrossOriginResourceSharingHeaders that does not allow CORS by default.
        /// </summary>
        /// <definition>
        /// public static CrossOriginResourceSharingHeaders Empty { get; }
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public static CrossOriginResourceSharingHeaders Empty = new CrossOriginResourceSharingHeaders();

        /// <summary>
        /// The origin hostnames allowed by the browser.
        /// </summary>
        /// <definition>
        /// public string[] AllowOrigins { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string[] AllowOrigins { get; set; }

        /// <summary>
        /// The allowed HTTP request methods.
        /// </summary>
        /// <definition>
        /// public string[] AllowMethods { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string[] AllowMethods { get; set; }

        /// <summary>
        /// The allowed HTTP request headers.
        /// </summary>
        /// <definition>
        /// public string[] AllowHeaders { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string[] AllowHeaders { get; set; }

        /// <summary>
        /// Defines the Max-Age cache expirity.
        /// </summary>
        /// <definition>
        /// public TimeSpan MaxAge { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public TimeSpan MaxAge { get; set; }

        /// <summary>
        /// Create a new <see cref="CrossOriginResourceSharingHeaders"/> class instance with given parameters.
        /// </summary>
        /// <param name="allowOrigins">The origin hostnames allowed by the browser.</param>
        /// <param name="allowMethods">The allowed HTTP request methods.</param>
        /// <param name="allowHeaders">The allowed HTTP request headers.</param>
        /// <param name="maxAge">Defines the max-age cache expirity time.</param>
        /// <definition>
        /// public CrossOriginResourceSharingHeaders(string[] allowOrigins, string[] allowMethods, string[] allowHeaders, TimeSpan maxAge)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public CrossOriginResourceSharingHeaders(string[] allowOrigins, string[] allowMethods, string[] allowHeaders, TimeSpan maxAge)
        {
            AllowOrigins = allowOrigins;
            AllowMethods = allowMethods;
            AllowHeaders = allowHeaders;
            MaxAge = maxAge;
        }

        /// <summary>
        /// Creates an empty <see cref="CrossOriginResourceSharingHeaders"/> instance with no predefined CORS headers.
        /// </summary>
        /// <definition>
        /// public CrossOriginResourceSharingHeaders()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public CrossOriginResourceSharingHeaders()
        {
            AllowOrigins = new string[0];
            AllowMethods = new string[0];
            AllowHeaders = new string[0];
            MaxAge = TimeSpan.Zero;
        }

        /// <summary>
        /// Get the Cross-Origin Resource Sharing header for the allowed origins.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string GetAllowOriginsHeader()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string GetAllowOriginsHeader() => string.Join(",", AllowOrigins);

        /// <summary>
        /// Get the Cross-Origin Resource Sharing header for the allowed request methods.
        /// </summary>
        /// <definition>
        /// public string GetAllowMethodsHeader()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string GetAllowMethodsHeader() => string.Join(",", AllowMethods);

        /// <summary>
        /// Get the Cross-Origin Resource Sharing header for the allowed request headers.
        /// </summary>
        /// <definition>
        /// public string GetAllowHeadersHeader()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string GetAllowHeadersHeader() => string.Join(",", AllowHeaders);

        /// <summary>
        /// Get the total of seconds in the Max-Age property as a request header.
        /// </summary>
        /// <definition>
        /// public string GetMaxAgeHeader()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string GetMaxAgeHeader() => MaxAge.TotalSeconds.ToString();

        /// <summary>
        /// Create an instance of Cross-Origin Resource Sharing that allows any origin, any method, and any header in the request.
        /// </summary>
        /// <definition>
        /// public static CrossOriginResourceSharingHeaders CreatePublicContext()
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        /// <static>True</static>
        public static CrossOriginResourceSharingHeaders CreatePublicContext()
        {
            return new CrossOriginResourceSharingHeaders(new string[] { "*" }, new string[] { "*" }, new string[] { "*" }, TimeSpan.FromMinutes(10));
        }
    }
}
