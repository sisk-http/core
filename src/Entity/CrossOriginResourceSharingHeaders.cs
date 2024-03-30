// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CrossOriginResourceSharingHeaders.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Entity
{
    /// <summary>
    /// Provides a class to provide Cross Origin response headers for when communicating with a browser.
    /// </summary>
    /// <definition>
    /// public sealed class CrossOriginResourceSharingHeaders
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class CrossOriginResourceSharingHeaders
    {
        /// <summary>
        /// Gets an instance of an empty CrossOriginResourceSharingHeaders.
        /// </summary>
        /// <definition>
        /// public static CrossOriginResourceSharingHeaders Empty { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public static CrossOriginResourceSharingHeaders Empty { get => new(); }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Credentials header indicates whether or not the response to the request can be exposed when the credentials flag is true. When used as part of a
        /// response to a preflight request, this indicates whether or not the actual request can be made using credentials.
        /// </summary>
        /// <definition>
        /// public bool AllowCredentials { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool? AllowCredentials { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Expose-Headers header adds the specified headers to the allowlist that JavaScript in browsers is allowed to access.
        /// </summary>
        /// <definition>
        /// public string[] ExposeHeaders { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
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
        public string? AllowOrigin { get; set; }

        /// <summary>
        /// Gets or sets domains which will define the source header according to one of the domains present below.
        /// </summary>
        /// <remarks>
        /// This property makes the server compare the origin of the request and associate the domain that corresponds to it.
        /// </remarks>
        /// <definition>
        /// public string[] AllowOrigins { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string[] AllowOrigins { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Methods header specifies the method or methods allowed when accessing the resource. 
        /// </summary>
        /// <definition>
        /// public string[] AllowMethods { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string[] AllowMethods { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Headers header is used in response to a preflight request to indicate which HTTP headers can be used when making the actual request. 
        /// </summary>
        /// <definition>
        /// public string[] AllowHeaders { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string[] AllowHeaders { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Max-Age header indicates how long the results of a preflight request can be cached.
        /// </summary>
        /// <definition>
        /// public TimeSpan MaxAge { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public TimeSpan MaxAge { get; set; }

        /// <summary>
        /// Creates an empty <see cref="CrossOriginResourceSharingHeaders"/> instance with no predefined CORS headers.
        /// </summary>
        /// <definition>
        /// public CrossOriginResourceSharingHeaders()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public CrossOriginResourceSharingHeaders()
        {
            ExposeHeaders = Array.Empty<string>();
            AllowOrigin = null;
            AllowOrigins = Array.Empty<string>();
            AllowMethods = Array.Empty<string>();
            AllowHeaders = Array.Empty<string>();
            MaxAge = TimeSpan.Zero;
        }

        /// <summary>
        /// Create an instance of Cross-Origin Resource Sharing that allows any origin, any method and any header in the request.
        /// </summary>
        /// <definition>
        /// public static CrossOriginResourceSharingHeaders CreatePublicContext()
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static CrossOriginResourceSharingHeaders CreatePublicContext() => new()
        {
            AllowHeaders = new string[] { "*" },
            AllowOrigin = "*",
            AllowMethods = new string[] { "*" },
            AllowCredentials = true,
            MaxAge = TimeSpan.FromSeconds(3600)
        };
    }
}
