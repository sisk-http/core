// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CrossOriginResourceSharingHeaders.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Entity {
    /// <summary>
    /// Provides a class to provide Cross Origin response headers for when communicating with a browser.
    /// </summary>
    public sealed class CrossOriginResourceSharingHeaders {

        /// <summary>
        /// When applied to the <see cref="AllowOrigin"/> property, the HTTP server automatically applies
        /// the incoming request Origin header value to the Access-Control-Allow-Origin header.
        /// </summary>
        public const string AutoAllowOrigin = "<SISK_AUTO_ALLOW_ORIGIN_NAME>";

        /// <summary>
        /// Gets an instance of an empty CrossOriginResourceSharingHeaders.
        /// </summary>
        public static CrossOriginResourceSharingHeaders Empty { get => new (); }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Credentials header indicates whether or not the response to the request can be exposed when the credentials flag is true. When used as part of a
        /// response to a preflight request, this indicates whether or not the actual request can be made using credentials.
        /// </summary>
        public bool? AllowCredentials { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Expose-Headers header adds the specified headers to the allowlist that JavaScript in browsers is allowed to access.
        /// </summary>
        public string [] ExposeHeaders { get; set; }

        /// <summary>
        /// From MDN: Access-Control-Allow-Origin specifies either a single origin which tells browsers to allow that origin to access the resource; or else — for requests
        /// without credentials — the "*" wildcard tells browsers to allow any origin to access the resource.
        /// </summary>
        public string? AllowOrigin { get; set; }

        /// <summary>
        /// Gets or sets domains which will define the source header according to one of the domains present below.
        /// </summary>
        /// <remarks>
        /// This property makes the server compare the origin of the request and associate the domain that corresponds to it.
        /// </remarks>
        public string [] AllowOrigins { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Methods header specifies the method or methods allowed when accessing the resource. 
        /// </summary>
        public string [] AllowMethods { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Headers header is used in response to a preflight request to indicate which HTTP headers can be used when making the actual request. 
        /// </summary>
        public string [] AllowHeaders { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Max-Age header indicates how long the results of a preflight request can be cached.
        /// </summary>
        public TimeSpan MaxAge { get; set; }

        /// <summary>
        /// Creates an empty <see cref="CrossOriginResourceSharingHeaders"/> instance with no predefined CORS headers.
        /// </summary>
        public CrossOriginResourceSharingHeaders () {
            ExposeHeaders = Array.Empty<string> ();
            AllowOrigins = Array.Empty<string> ();
            AllowMethods = Array.Empty<string> ();
            AllowHeaders = Array.Empty<string> ();
            AllowOrigin = null;
            MaxAge = TimeSpan.Zero;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CrossOriginResourceSharingHeaders"/> class with the specified CORS headers.
        /// </summary>
        /// <param name="allowOrigin">The value of the Access-Control-Allow-Origin header.</param>
        /// <param name="allowOrigins">The values of the Access-Control-Allow-Origin header.</param>
        /// <param name="allowMethods">The values of the Access-Control-Allow-Methods header.</param>
        /// <param name="allowHeaders">The values of the Access-Control-Allow-Headers header.</param>
        /// <param name="exposeHeaders">The values of the Access-Control-Expose-Headers header.</param>
        /// <param name="maxAge">The value of the Access-Control-Max-Age header.</param>
        /// <param name="allowCredentials">The value of the Access-Control-Allow-Credentials header.</param>
        public CrossOriginResourceSharingHeaders (
            string? allowOrigin = null,
            string []? allowOrigins = null,
            string []? allowMethods = null,
            string []? allowHeaders = null,
            string []? exposeHeaders = null,
            TimeSpan? maxAge = null,
            bool allowCredentials = false ) {

            ExposeHeaders = exposeHeaders ?? Array.Empty<string> ();
            AllowOrigins = allowOrigins ?? Array.Empty<string> ();
            AllowHeaders = allowHeaders ?? Array.Empty<string> ();
            AllowMethods = allowMethods ?? Array.Empty<string> ();
            AllowOrigin = allowOrigin;
            MaxAge = maxAge ?? TimeSpan.Zero;
            AllowCredentials = allowCredentials;
        }

        /// <summary>
        /// Create an instance of Cross-Origin Resource Sharing that allows any origin, any method and any header in the request.
        /// </summary>
        public static CrossOriginResourceSharingHeaders CreatePublicContext () => new () {
            AllowHeaders = new string [] { "*" },
            AllowOrigin = "*",
            AllowMethods = new string [] { "*" },
            AllowCredentials = true,
            MaxAge = TimeSpan.FromSeconds ( 3600 )
        };
    }
}
