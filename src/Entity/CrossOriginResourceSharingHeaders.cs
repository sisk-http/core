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
        /// public static CrossOriginResourceSharingHeaders Empty 
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public static CrossOriginResourceSharingHeaders Empty = new CrossOriginResourceSharingHeaders();

        /// <summary>
        /// From MDN: The Access-Control-Allow-Credentials header indicates whether or not the response to the request can be exposed when the credentials flag is true. When used as part of a
        /// response to a preflight request, this indicates whether or not the actual request can be made using credentials.
        /// </summary>
        /// <definition>
        /// public bool AllowCredentials { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public bool? AllowCredentials { get; set; }

        /// <summary>
        /// From MDN: The Access-Control-Expose-Headers header adds the specified headers to the allowlist that JavaScript in browsers is allowed to access.
        /// </summary>
        /// <definition>
        /// public string[] ExposeHeaders { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string[] ExposeHeaders { get; set; }

        /// <summary>
        /// From MDN: Access-Control-Allow-Origin specifies either a single origin which tells browsers to allow that origin to access the resource; or else — for requests
        /// without credentials — the "*" wildcard tells browsers to allow any origin to access the resource.
        /// </summary>
        /// <definition>
        /// public string? AllowOrigin { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string? AllowOrigin { get; set; }

        /// <summary>
        /// From MDN: The Access-Control-Allow-Methods header specifies the method or methods allowed when accessing the resource. 
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
        /// From MDN: The Access-Control-Allow-Headers header is used in response to a preflight request to indicate which HTTP headers can be used when making the actual request. 
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
        /// From MDN: The Access-Control-Max-Age header indicates how long the results of a preflight request can be cached.
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
        /// <param name="allowOrigin">The origin hostname allowed by the browser.</param>
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
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This constructor is obsolete. Please, create an new class instance without any parameter instead.")]
        public CrossOriginResourceSharingHeaders(string allowOrigin, string[] allowMethods, string[] allowHeaders, TimeSpan maxAge)
        {
            AllowOrigin = allowOrigin;
            AllowMethods = allowMethods;
            AllowHeaders = allowHeaders;
            MaxAge = maxAge;
            ExposeHeaders = new string[] { };
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
            ExposeHeaders = new string[0];
            AllowOrigin = null;
            AllowMethods = new string[0];
            AllowHeaders = new string[0];
            MaxAge = TimeSpan.Zero;
        }

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
            return new CrossOriginResourceSharingHeaders()
            {
                AllowHeaders = new string[] { "*" },
                AllowOrigin = "*",
                AllowMethods = new string[] { "*" },
                AllowCredentials = true,
                MaxAge = TimeSpan.FromSeconds(3600)
            };
        }
    }
}
