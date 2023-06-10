using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;
using System.Text.RegularExpressions;
using System.Collections.Specialized;
using System.Diagnostics.Tracing;
using System.Collections.Generic;
using Sisk.Core.Http.Streams;
using System.Threading.Tasks;
using System.Net.WebSockets;
using System.Globalization;
using System.Buffers.Text;
using System.Net.Sockets;
using System.Collections;
using System.Diagnostics;
using Sisk.Core.Internal;
using System.Reflection;
using Sisk.Core.Routing;
using Sisk.Core.Entity;
using System.Threading;
using System.Xml.Linq;
using System.Net.Mime;
using System.Net.Http;
using System.Dynamic;
using Sisk.Core.Http;
using System.Text;
using System.Linq;
using System.Web;
using System.Net;
using System.IO;
using System;
/* \Entity\CrossOriginResourceSharingHeaders.cs */
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
        /// Gets an instance of an empty CrossOriginResourceSharingHeaders.
        /// </summary>
        /// <definition>
        /// public static CrossOriginResourceSharingHeaders Empty { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
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
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
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
        public string[]? AllowOrigins { get; set; }

        /// <summary>
        /// Gets or sets the Access-Control-Allow-Methods header specifies the method or methods allowed when accessing the resource. 
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
        /// Gets or sets the Access-Control-Allow-Headers header is used in response to a preflight request to indicate which HTTP headers can be used when making the actual request. 
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
        /// Gets or sets the Access-Control-Max-Age header indicates how long the results of a preflight request can be cached.
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
            ExposeHeaders = Array.Empty<string>();
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
            ExposeHeaders = Array.Empty<string>();
            AllowOrigin = null;
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

/* \Entity\MultipartObject.cs */


namespace Sisk.Core.Entity
{
    /// <summary>
    /// Represents an multipart/form-data object.
    /// </summary>
    /// <definition>
    /// public class MultipartObject
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Entity
    /// </namespace>
    public class MultipartObject
    {
        /// <summary>
        /// Gets or sets the default content encoding for decoding objects contents as strings.
        /// </summary>
        /// <definition>
        /// public static Encoding DefaultContentEncoding { get; set; }
        /// </definition>
        /// <type>
        /// Static property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public static Encoding DefaultContentEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the default content encoding for decoding multipart-form headers names and values.
        /// </summary>
        /// <definition>
        /// public static Encoding DefaultHeadersEncoding { get; set; }
        /// </definition>
        /// <type>
        /// Static property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public static Encoding DefaultHeadersEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// The multipart form data object headers.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Headers { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public NameValueCollection Headers { get; private set; }

        /// <summary>
        /// The name of the file provided by Multipart form data. Null is returned if the object is not a file.
        /// </summary>
        /// <definition>
        /// public string? Filename { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string? Filename { get; private set; }

        /// <summary>
        /// The multipart form data object field name.
        /// </summary>
        /// <definition>
        /// public string Name { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string Name { get; private set; }

        /// <summary>
        /// The multipart form data content bytes.
        /// </summary>
        /// <definition>
        /// public byte[] ContentBytes { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public byte[] ContentBytes { get; private set; }

        /// <summary>
        /// The multipart form data content length.
        /// </summary>
        /// <definition>
        /// public int ContentLength { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public int ContentLength { get; private set; }

        /// <summary>
        /// Reads the content bytes with the given encoder.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string? ReadContentAsString(Encoding encoder)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string? ReadContentAsString(Encoding encoder)
        {
            if (ContentLength == 0)
                return null;
            return encoder.GetString(ContentBytes);
        }

        /// <summary>
        /// Reads the content bytes using the <see cref="DefaultContentEncoding"/> encoding.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string? ReadContentAsString()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string? ReadContentAsString()
        {
            return ReadContentAsString(DefaultContentEncoding);
        }

        /// <summary>
        /// Determine the image format based in the file header for each image content type.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public MultipartObjectImageFormat GetImageFormat()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public MultipartObjectCommonFormat GetCommonFileFormat()
        {
            IEnumerable<byte> len8 = ContentBytes.Take(8);
            IEnumerable<byte> len4 = ContentBytes.Take(4);
            IEnumerable<byte> len3 = ContentBytes.Take(3);

            if (len8.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            {
                return MultipartObjectCommonFormat.PNG;
            }
            else if (len4.SequenceEqual(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }))
            {
                return MultipartObjectCommonFormat.WEBP;
            }
            else if (len4.SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }))
            {
                return MultipartObjectCommonFormat.PDF;
            }
            else if (len3.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
            {
                return MultipartObjectCommonFormat.JPEG;
            }
            else if (len3.SequenceEqual(new byte[] { 73, 73, 42 }))
            {
                return MultipartObjectCommonFormat.TIFF;
            }
            else if (len3.SequenceEqual(new byte[] { 77, 77, 42 }))
            {
                return MultipartObjectCommonFormat.TIFF;
            }
            else if (len3.SequenceEqual(new byte[] { 0x42, 0x4D }))
            {
                return MultipartObjectCommonFormat.BMP;
            }
            else if (len3.SequenceEqual(new byte[] { 0x47, 0x46, 0x49 }))
            {
                return MultipartObjectCommonFormat.GIF;
            }
            else
            {
                return MultipartObjectCommonFormat.Unknown;
            }
        }

        internal MultipartObject(NameValueCollection headers, string? filename, string name, byte[]? body)
        {
            Headers = headers;
            Filename = filename;
            Name = name;
            ContentBytes = body ?? Array.Empty<byte>();
            ContentLength = body?.Length ?? 0;
        }

        internal static MultipartObject[] ParseMultipartObjects(HttpRequest req)
        {
            string? contentType = req.Headers["Content-Type"];
            if (contentType is null)
            {
                throw new InvalidOperationException("Content-Type header cannot be null when retriving a multipart form content");
            }

            string[] contentTypePieces = contentType.Split(';');
            string? boundary = null;
            foreach (string obj in contentTypePieces)
            {
                string[] kv = obj.Split("=");
                if (kv.Length != 2)
                { continue; }
                if (kv[0].Trim() == "boundary")
                {
                    boundary = kv[1].Trim();
                }
            }

            if (boundary is null)
            {
                throw new InvalidOperationException("No boundary was specified for this multipart form content.");
            }

            byte[] boundaryBytes = Encoding.UTF8.GetBytes(boundary);

            /////////
            // https://stackoverflow.com/questions/9755090/split-a-byte-array-at-a-delimiter
            byte[][] Separate(byte[] source, byte[] separator)
            {
                var Parts = new List<byte[]>();
                var Index = 0;
                byte[] Part;
                for (var I = 0; I < source.Length; ++I)
                {
                    if (Equals(source, separator, I))
                    {
                        Part = new byte[I - Index];
                        Array.Copy(source, Index, Part, 0, Part.Length);
                        Parts.Add(Part);
                        Index = I + separator.Length;
                        I += separator.Length - 1;
                    }
                }
                Part = new byte[source.Length - Index];
                Array.Copy(source, Index, Part, 0, Part.Length);
                Parts.Add(Part);
                return Parts.ToArray();
            }

            bool Equals(byte[] source, byte[] separator, int index)
            {
                for (int i = 0; i < separator.Length; ++i)
                    if (index + i >= source.Length || source[index + i] != separator[i])
                        return false;
                return true;
            }
            /////////

            byte[][] matchedResults = Separate(req.RawBody, boundaryBytes);

            List<MultipartObject> outputObjects = new List<MultipartObject>();
            for (int i = 1; i < matchedResults.Length - 1; i++)
            {
                byte[]? contentBytes = null;
                NameValueCollection headers = new NameValueCollection();
                byte[] result = matchedResults[i].ToArray();
                int resultLength = result.Length - 4;
                //string content = Encoding.ASCII.GetString(result);

                int spaceLength = 0;
                bool parsingContent = false;
                bool headerNameParsed = false;
                bool headerValueParsed = false;
                List<byte> headerNameBytes = new();
                List<byte> headerValueBytes = new();

                int headerSize = 0;
                for (int j = 0; j < resultLength; j++)
                {
                    byte J = result[j];
                    if (spaceLength == 2 && headerNameParsed && !headerValueParsed)
                    {
                        string headerName = DefaultHeadersEncoding.GetString(headerNameBytes.ToArray());
                        string headerValue = DefaultHeadersEncoding.GetString(headerValueBytes.ToArray());

                        headers.Add(headerName, headerValue.Trim());
                        headerNameParsed = false;
                        headerValueParsed = false;
                    }
                    else if (spaceLength == 4 && !parsingContent)
                    {
                        headerSize = j;
                        contentBytes = new byte[resultLength - headerSize];
                        parsingContent = true;
                    }
                    if ((J == 0x0A || J == 0x0D) && !parsingContent)
                    {
                        spaceLength++;
                        continue;
                    }
                    else
                    {
                        spaceLength = 0;
                    }
                    if (!parsingContent)
                    {
                        if (!headerNameParsed)
                        {
                            if (J == 58)
                            {
                                headerNameParsed = true;
                            }
                            else
                            {
                                headerNameBytes.Add(J);
                            }
                        }
                        else if (!headerValueParsed)
                        {
                            headerValueBytes.Add(J);
                        }
                    }
                    else
                    {
                        contentBytes![j - headerSize] = J;
                    }
                }

                // parse field name
                string[] val = headers["Content-Disposition"]?.Split(';') ?? Array.Empty<string>();
                string? fieldName = null;
                string? fieldFilename = null;

                foreach (string valueAttribute in val)
                {
                    string[] valAttributeParts = valueAttribute.Trim().Split("=");
                    if (valAttributeParts.Length != 2)
                        continue;
                    if (valAttributeParts[0] == "name")
                    {
                        fieldName = valAttributeParts[1].Trim('"');
                    }
                    else if (valAttributeParts[0] == "filename")
                    {
                        fieldFilename = valAttributeParts[1].Trim('"');
                    }
                }

                if (fieldName == null)
                {
                    throw new InvalidOperationException($"Content-part object position {i} cannot have an empty field name.");
                }

                MultipartObject newObject = new MultipartObject(headers, fieldFilename, fieldName, contentBytes?.ToArray());
                outputObjects.Add(newObject);
            }

            return outputObjects.ToArray();
        }
    }

    /// <summary>
    /// Represents an image format for Multipart objects.
    /// </summary>
    /// <definition>
    /// public enum MultipartObjectImageFormat
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Entity
    /// </namespace>
    public enum MultipartObjectCommonFormat
    {
        /// <summary>
        /// Represents that the object is not a recognized image.
        /// </summary>
        /// <definition>
        /// Unknown = 0
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        Unknown = 0,

        /// <summary>
        /// Represents an JPEG/JPG image.
        /// </summary>
        /// <definition>
        /// JPEG = 100
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        JPEG = 100,

        /// <summary>
        /// Represents an GIF image.
        /// </summary>
        /// <definition>
        /// GIF = 101
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        GIF = 101,

        /// <summary>
        /// Represents an PNG image.
        /// </summary>
        /// <definition>
        /// PNG = 102
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        PNG = 102,

        /// <summary>
        /// Represents an TIFF image.
        /// </summary>
        /// <definition>
        /// TIFF = 103
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        TIFF = 103,

        /// <summary>
        /// Represents an bitmap image.
        /// </summary>
        /// <definition>
        /// BMP = 104
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        BMP = 104,

        /// <summary>
        /// Represents an WebP image.
        /// </summary>
        /// <definition>
        /// WEBP = 105
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        WEBP = 105,

        /// <summary>
        /// Represents an PDF file.
        /// </summary>
        /// <definition>
        /// PDF = 200
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        PDF = 200
    }
}

/* \Http\HttpContext.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an context for Http requests.
    /// </summary>
    /// <definition>
    /// public class HttpContext
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpContext
    {
        /// <summary>
        /// Gets or sets a managed object that is accessed and modified by request handlers.
        /// </summary>
        /// <definition>
        /// public Dictionary&lt;string, object?&gt; RequestBag { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Dictionary<string, object?> RequestBag { get; set; }

        /// <summary>
        /// Gets the context Http Server instance.
        /// </summary>
        /// <definition>
        /// public HttpServer? HttpServer { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpServer? HttpServer { get; private set; }

        /// <summary>
        /// Gets or sets the HTTP response for this context. This property is only not null when a post-executing <see cref="IRequestHandler"/> was executed for this router context.
        /// </summary>
        /// <definition>
        /// public HttpResponse? RouterResponse { get; } 
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse? RouterResponse { get; internal set; } = null!;

        /// <summary>
        /// Gets the matched Http Route object from the Router.
        /// </summary>
        /// <definition>
        /// public Route? MatchedRoute { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Route? MatchedRoute { get; internal set; }

        internal HttpContext(Dictionary<string, object?> requestBag, HttpServer? httpServer, Route? matchedRoute)
        {
            RequestBag = requestBag;
            HttpServer = httpServer;
            MatchedRoute = matchedRoute;
        }
    }
}

/* \Http\HttpRequest.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an exception that is thrown while a request is being interpreted by the HTTP server.
    /// </summary>
    /// <definition>
    /// public class HttpRequestException : Exception
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpRequestException : Exception
    {
        internal HttpRequestException(string message) : base(message) { }
    }

    /// <summary>
    /// Represents an HTTP request received by a Sisk server.
    /// </summary>
    /// <definition>
    /// public sealed class HttpRequest
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public sealed class HttpRequest
    {
        internal HttpServer baseServer;
        internal ListeningHost hostContext;
        private HttpServerConfiguration contextServerConfiguration;
        private HttpListenerResponse listenerResponse;
        private HttpListenerRequest listenerRequest;
        private HttpListenerContext context;
        private byte[]? contentBytes;
        internal bool isStreaming;
        private HttpRequestEventSource? activeEventSource;
        private bool isContentAvailable = false;
        private NameValueCollection headers = new NameValueCollection();

        internal HttpRequest(
            HttpListenerRequest listenerRequest,
            HttpListenerResponse listenerResponse,
            HttpServer server,
            ListeningHost host,
            HttpListenerContext context)
        {
            this.baseServer = server;
            this.contextServerConfiguration = baseServer.ServerConfiguration;
            this.listenerResponse = listenerResponse;
            this.listenerRequest = listenerRequest;
            this.hostContext = host;
            this.RequestedAt = DateTime.Now;
            this.Query = listenerRequest.QueryString;
            this.RequestId = Guid.NewGuid();

            IPAddress requestRealAddress = new IPAddress(listenerRequest.LocalEndPoint.Address.GetAddressBytes());
            this.Origin = requestRealAddress;

            if (contextServerConfiguration.ResolveForwardedOriginAddress)
            {
                string? forwardedIp = listenerRequest.Headers["X-Forwarded-For"];
                if (forwardedIp != null)
                {
                    /*
                     * the first entry from the header value is the real client ip.
                     * source: https://datatracker.ietf.org/doc/html/rfc2616#section-4.2
                     */
                    string forwardedIpLiteralStr = forwardedIp.Contains(',') ? forwardedIp.Substring(0, forwardedIp.IndexOf(',')) : forwardedIp;
                    bool ok = IPAddress.TryParse(forwardedIpLiteralStr, out IPAddress? forwardedAddress);
                    if (!ok || forwardedAddress == null)
                    {
                        throw new HttpRequestException("The forwarded IP address is invalid.");
                    }
                    else
                    {
                        this.Origin = forwardedAddress;
                    }
                }
            }

            string? cookieHeader = listenerRequest.Headers["cookie"];
            if (cookieHeader != null)
            {
                string[] cookieParts = cookieHeader.Split(';');
                foreach (string cookieExpression in cookieParts)
                {
                    int eqPos = cookieExpression.IndexOf("=");
                    if (eqPos < 0)
                    {
                        throw new HttpRequestException("The cookie header is invalid or is it has an malformed syntax.");
                    }
                    string key = cookieExpression.Substring(0, eqPos).Trim();
                    string value = cookieExpression.Substring(eqPos + 1).Trim();

                    if (string.IsNullOrEmpty(key))
                    {
                        throw new HttpRequestException("The cookie header is invalid or is it has an malformed syntax.");
                    }

                    this.Cookies[key] = WebUtility.UrlDecode(value);
                }
            }

            // normalize headers encoding
            if (contextServerConfiguration.Flags.NormalizeHeadersEncodings)
            {
                Encoding entryCodepage = Encoding.GetEncoding("ISO-8859-1");
                foreach (string headerName in listenerRequest.Headers)
                {
                    string headerValue = listenerRequest.Headers[headerName]!;
                    headers.Add(
                        headerName,
                        mbConvertCodepage(headerValue, entryCodepage, listenerRequest.ContentEncoding)
                    );
                }
            }
            else
            {
                headers = listenerRequest.Headers;
            }

            this.context = context;
        }

        internal string mbConvertCodepage(string input, Encoding inEnc, Encoding outEnc)
        {
            byte[] tempBytes;
            tempBytes = inEnc.GetBytes(input);
            return outEnc.GetString(tempBytes);
        }

#pragma warning disable
        ~HttpRequest()
        {
            this.contentBytes = null;
            this.listenerRequest = null;
            this.listenerResponse = null;
            this.contextServerConfiguration = null;
        }
#pragma warning restore

        internal void ImportContents(Stream listenerRequest)
        {
            using (var memoryStream = new MemoryStream())
            {
                listenerRequest.CopyTo(memoryStream);
                this.contentBytes = memoryStream.ToArray();
                isContentAvailable = true;
            }
        }

        /// <summary>
        /// Gets a unique random ID for this request.
        /// </summary>
        /// <definition>
        /// public string RequestId { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Guid RequestId { get; private set; }

        /// <summary>
        /// Gets a boolean indicating whether this request was made by an secure transport context (SSL/TLS) or not.
        /// </summary>
        /// <definition>
        /// public bool IsSecure { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IsSecure { get => listenerRequest.IsSecureConnection; }

        /// <summary>
        /// Gets a boolean indicating whether the content of this request has been processed by the server.
        /// </summary>
        /// <definition>
        /// public bool IsContentAvailable { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IsContentAvailable { get => isContentAvailable; }

        /// <summary>
        /// Gets a boolean indicating whether this request has contents.
        /// </summary>
        /// <definition>
        /// public bool HasContents { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool HasContents { get => this.ContentLength > 0; }

        /// <summary>
        /// Gets the HTTP request headers.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Headers { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Headers
        {
            get => headers;
        }

        /// <summary>
        /// Gets an <see cref="NameValueCollection"/> object with all cookies set in this request.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Cookies { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Cookies { get; private set; } = new NameValueCollection();

        /// <summary>
        /// Get the requested host header (without port) from this HTTP request.
        /// </summary>
        /// <definition>
        /// public string Host { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string Host
        {
            get => listenerRequest.Url!.Host;
        }

        /// <summary>
        /// Get the requested host header with the port from this HTTP request.
        /// </summary>
        /// <definition>
        /// public string Authority { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string Authority
        {
            get => listenerRequest.Url!.Authority;
        }

        /// <summary>
        /// Gets the HTTP request path without the query string.
        /// </summary>
        /// <definition>
        /// public string Path { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string Path
        {
            get => listenerRequest.Url?.AbsolutePath ?? "/";
        }

        /// <summary>
        /// Gets the full HTTP request path with the query string.
        /// </summary>
        /// <definition>
        /// public string FullPath { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string FullPath
        {
            get => listenerRequest.RawUrl ?? "/";
        }

        /// <summary>
        /// Gets the full URL for this request, with scheme, host, port (if any), path and query.
        /// </summary>
        /// <definition>
        /// public string FullUrl { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string FullUrl
        {
            get => listenerRequest.Url!.ToString();
        }

        /// <summary>
        /// Gets the Encoding used in the request.
        /// </summary>
        /// <definition>
        /// public Encoding RequestEncoding { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Encoding RequestEncoding
        {
            get => listenerRequest.ContentEncoding;
        }

        /// <summary>
        /// Gets the HTTP request method.
        /// </summary>
        /// <definition>
        /// public HttpMethod Method { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpMethod Method
        {
            get => new HttpMethod(listenerRequest.HttpMethod);
        }

        /// <summary>
        /// Gets the HTTP request body as string, decoded by the request content encoding.
        /// </summary>
        /// <definition>
        /// public string Body { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string Body
        {
            get => listenerRequest.ContentEncoding.GetString(RawBody);
        }

        /// <summary>
        /// Gets the HTTP request body as a byte array.
        /// </summary>
        /// <definition>
        /// public byte[] RawBody { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public byte[] RawBody
        {
            get => contentBytes ?? Array.Empty<byte>();
        }

        /// <summary>
        /// Gets the content length in bytes.
        /// </summary>
        /// <definition>
        /// public long ContentLength { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public long ContentLength
        {
            get => listenerRequest.ContentLength64;
        }

        /// <summary>
        /// Gets the HTTP request query extracted from the path string. This property also contains routing parameters.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Query { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Query { get; internal set; }

        /// <summary>
        /// Gets the HTTP request URL raw query string.
        /// </summary>
        /// <definition>
        /// public string? QueryString { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string? QueryString { get => listenerRequest.Url?.Query; }

        /// <summary>
        /// Gets the incoming IP address from the request.
        /// </summary>
        /// <definition>
        /// public IPAddress Origin { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public IPAddress Origin
        {
            get; internal set;
        }

        /// <summary>
        /// Gets the moment which the request was received by the server.
        /// </summary>
        /// <definition>
        /// public DateTime RequestedAt { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public DateTime RequestedAt { get; private init; }

        /// <summary>
        /// Gets the HttpContext for this request.
        /// </summary>
        /// <definition>
        /// public HttpContext? Context { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpContext? Context { get; internal set; }

        /// <summary>
        /// Gets the multipart form content for this request.
        /// </summary>
        /// <returns></returns>
        /// <exception cref="InvalidOperationException"></exception>
        /// <definition>
        /// public MultipartObject[] GetMultipartFormContent()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public MultipartObject[] GetMultipartFormContent()
        {
            return MultipartObject.ParseMultipartObjects(this);
        }

        /// <summary>
        /// Gets the values sent by a form in this request.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public NameValueCollection GetFormContent()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection GetFormContent()
        {
            return HttpUtility.ParseQueryString(Body);
        }

        /// <summary>
        /// Gets the raw HTTP request message from the socket.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string GetRawHttpRequest(bool includeBody = true)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string GetRawHttpRequest(bool includeBody = true)
        {
            StringBuilder sb = new StringBuilder();
            // Method and path
            sb.Append(Method.ToString().ToUpper() + " ");
            sb.Append(Path + " ");
            sb.Append("HTTP/");
            sb.Append(listenerRequest.ProtocolVersion.Major + ".");
            sb.Append(listenerRequest.ProtocolVersion.Minor + "\n");

            // Headers
            foreach (string hName in Headers)
            {
                string hValue = Headers[hName]!;
                sb.AppendLine($"{hName}: {hValue}");
            }
            sb.AppendLine();

            // Content
            if (includeBody)
            {
                sb.Append(Body);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Gets a query value using an case-insensitive search.
        /// </summary>
        /// <param name="queryKeyName">The query value name.</param>
        /// <returns></returns>
        /// <definition>
        /// public string? GetQueryValue(string queryKeyName)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string? GetQueryValue(string queryKeyName) => Query[queryKeyName];

        /// <summary>
        /// Gets a header value using a case-insensitive search.
        /// </summary>
        /// <param name="headerName">The header name.</param>
        /// <returns></returns>
        /// <definition>
        /// public string? GetHeader(string headerName)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string? GetHeader(string headerName) => Headers[headerName];

        /// <summary>
        /// Closes this HTTP request and their connection with the remote client without sending any response.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public HttpResponse Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse Close()
        {
            return new HttpResponse(HttpResponse.HTTPRESPONSE_CLOSE);
        }

        /// <summary>
        /// Gets an Event Source interface for this request. Calling this method will put this <see cref="HttpRequest"/> instance in it's
        /// event source listening state.
        /// </summary>
        /// <param name="identifier">Optional. Defines an label to the EventStream connection, useful for finding this connection's reference later.</param>
        /// <definition>
        /// public HttpRequestEventSource GetEventSource(string? identifier = null)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http 
        /// </namespace>
        public HttpRequestEventSource GetEventSource(string? identifier = null)
        {
            if (isStreaming)
            {
                throw new InvalidOperationException("This HTTP request is already in streaming mode.");
            }
            isStreaming = true;
            activeEventSource = new HttpRequestEventSource(identifier, listenerResponse, listenerRequest, this);
            return activeEventSource;
        }

        /// <summary>
        /// Accepts and acquires a websocket for this request. Calling this method will put this <see cref="HttpRequest"/> instance in
        /// streaming state.
        /// </summary>
        /// <param name="subprotocol">Optional. Determines the sub-protocol to plug the websocket in.</param>
        /// <param name="identifier">Optional. Defines an label to the Web Socket connection, useful for finding this connection's reference later.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket GetWebSocket(string? subprotocol = null)
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        /// <exception cref="InvalidOperationException"></exception>
        public HttpWebSocket GetWebSocket(string? subprotocol = null, string? identifier = null)
        {
            if (isStreaming)
            {
                throw new InvalidOperationException("This HTTP request is already in streaming mode.");
            }
            isStreaming = true;
            var accept = context.AcceptWebSocketAsync(subprotocol).Result;
            return new HttpWebSocket(accept, this, identifier);
        }

        internal long CalcRequestSize()
        {
            long l = 0;
            l += listenerRequest.ContentLength64;
            l += RequestEncoding.GetByteCount(GetRawHttpRequest(false));
            return l;
        }
    }
}

/* \Http\HttpResponse.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an HTTP Response.
    /// </summary>
    /// <definition>
    /// public class HttpResponse
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpResponse
    {
        internal const byte HTTPRESPONSE_EMPTY = 2;
        internal const byte HTTPRESPONSE_STREAM_CLOSE = 4;
        internal const byte HTTPRESPONSE_ERROR = 8;
        internal const byte HTTPRESPONSE_CLOSE = 16;
        internal int CalculedLength = 0;

        /// <summary>
        /// Creates an new empty <see cref="HttpResponse"/> with no status code or contents. This will cause to the HTTP server to close the
        /// connection between the server and the client and don't deliver any response.
        /// </summary>
        /// <definition>
        /// public static HttpResponse CreateEmptyResponse()
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static HttpResponse CreateEmptyResponse()
        {
            return new HttpResponse(HTTPRESPONSE_EMPTY);
        }

        /// <summary>
        /// Creates an new redirect <see cref="HttpResponse"/> with given location header.
        /// </summary>
        /// <param name="location">The absolute or relative URL path which the client must be redirected to.</param>
        /// <definition>
        /// public static HttpResponse CreateRedirectResponse(string location)
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static HttpResponse CreateRedirectResponse(string location)
        {
            HttpResponse res = new HttpResponse();
            res.Status = System.Net.HttpStatusCode.MovedPermanently;
            res.Headers.Add("Location", location);

            return res;
        }


        /// <summary>
        /// Gets or sets an custom HTTP status code and description for this HTTP response. If this property ins't null, it will overwrite
        /// the <see cref="Status"/> property in this class.
        /// </summary>
        /// <definition>
        /// public HttpStatusInformation? CustomStatus { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpStatusInformation? CustomStatus { get; set; } = null;

        /// <summary>
        /// Gets or sets the HTTP response status code.
        /// </summary>
        /// <definition>
        /// public HttpStatusCode Status { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpStatusCode Status { get; set; } = HttpStatusCode.OK;

        /// <summary>
        /// Gets a <see cref="NameValueCollection"/> instance of the HTTP response headers.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Headers { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public NameValueCollection Headers { get; private set; } = new NameValueCollection();

        /// <summary>
        /// Gets or sets the HTTP response body contents.
        /// </summary>
        /// <definition>
        /// public HttpContent? Content { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpContent? Content { get; set; }

        /// <summary>
        /// Gets or sets the default encoding when creating new HttpResponse instances.
        /// </summary>
        /// <definition>
        /// public static Encoding DefaultEncoding { get; set; }
        /// </definition>
        /// <type>
        /// Static Property
        /// </type>
        /// <remarks>
        /// This property is no longer useful and ins't used anywhere. Please, avoid using it.
        /// </remarks>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        /// <static>True</static>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This property is deprecated and ins't used anywhere. Please, avoid using it.")]
        public static Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets whether the HTTP response can be sent chunked.
        /// </summary>
        /// <definition>
        /// public bool SendChunked { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool SendChunked { get; set; } = false;

        internal byte internalStatus = 0;

        internal HttpResponse(byte internalStatus)
        {
            this.internalStatus = internalStatus;
        }

        internal HttpResponse(HttpListenerResponse res)
        {
            this.Status = (HttpStatusCode)res.StatusCode;
            this.Headers.Add(res.Headers);
        }

        /// <summary>
        /// Gets the raw HTTP response message.
        /// </summary>
        /// <param name="includeBody">Determines whether the message content will also be included in the return from this function.</param>
        /// <returns></returns>
        /// <definition>
        /// public string GetRawHttpResponse(bool includeBody = true)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string GetRawHttpResponse(bool includeBody = true)
        {
            StringBuilder sb = new StringBuilder();
            sb.AppendLine($"HTTP/1.1 {(int)Status}");
            foreach (string header in this.Headers)
            {
                sb.Append(header + ": ");
                sb.Append(this.Headers[header]);
                sb.Append('\n');
            }
            sb.Append('\n');

            if (includeBody)
            {
                sb.Append(Content?.ReadAsStringAsync().Result);
            }

            return sb.ToString();
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with HTTP OK status code and no content.
        /// </summary>
        /// <definition>
        /// public HttpResponse()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse()
        {
            this.Status = HttpStatusCode.OK;
            this.Content = null;
        }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <definition>
        /// public HttpResponse(HttpStatusCode status)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse(HttpStatusCode status) : this(status, null) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code.
        /// </summary>
        /// <definition>
        /// public HttpResponse(int status) 
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(int status) : this((HttpStatusCode)status, null) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP content.
        /// </summary>
        /// <definition>
        /// public HttpResponse(int status, HttpContent? content)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(int status, HttpContent? content) : this((HttpStatusCode)status, content) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given HTTP content, with default status code as 200 OK.
        /// </summary>
        /// <definition>
        /// public HttpResponse(HttpContent? content)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(HttpContent? content) : this(HttpStatusCode.OK, content) { }

        /// <summary>
        /// Creates an new <see cref="HttpResponse"/> instance with given status code and HTTP contents.
        /// </summary>
        /// <definition>
        /// public HttpResponse(HttpStatusCode status, HttpContent content)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpResponse(HttpStatusCode status, HttpContent? content)
        {
            this.Status = status;
            this.Content = content;
        }

        /// <summary>
        /// Sets a cookie and sends it in the response to be set by the client.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        /// <definition>
        /// public void SetCookie(string name, string value)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void SetCookie(string name, string value)
        {
            Headers.Add("Set-Cookie", $"{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}");
        }

        /// <summary>
        /// Sets a cookie and sends it in the response to be set by the client.
        /// </summary>
        /// <param name="name">The cookie name.</param>
        /// <param name="value">The cookie value.</param>
        /// <param name="expires">The cookie expirity date.</param>
        /// <param name="maxAge">The cookie max duration after being set.</param>
        /// <param name="domain">The domain where the cookie will be valid.</param>
        /// <param name="path">The path where the cookie will be valid.</param>
        /// <param name="secure">Determines if the cookie will only be stored in an secure context.</param>
        /// <param name="httpOnly">Determines if the cookie will be only available in the HTTP context.</param>
        /// <param name="sameSite">The cookie SameSite parameter.</param>
        /// <definition>
        /// public void SetCookie(string name, string value, DateTime? expires, TimeSpan? maxAge, string? domain, string? path, bool? secure, bool? httpOnly)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void SetCookie(string name, string value, DateTime? expires, TimeSpan? maxAge, string? domain, string? path, bool? secure, bool? httpOnly, string? sameSite)
        {
            List<string> syntax = new List<string>();
            syntax.Add($"{HttpUtility.UrlEncode(name)}={HttpUtility.UrlEncode(value)}");
            if (expires != null)
            {
                syntax.Add($"Expires={expires.Value.ToUniversalTime():r}");
            }
            if (maxAge != null)
            {
                syntax.Add($"Max-Age={maxAge.Value.TotalSeconds}");
            }
            if (domain != null)
            {
                syntax.Add($"Domain={domain}");
            }
            if (path != null)
            {
                syntax.Add($"Path={path}");
            }
            if (secure == true)
            {
                syntax.Add($"Secure");
            }
            if (httpOnly == true)
            {
                syntax.Add($"HttpOnly");
            }
            if (sameSite != null)
            {
                syntax.Add($"SameSite={sameSite}");
            }

            Headers.Add("Set-Cookie", String.Join("; ", syntax));
        }

        internal string? GetHeader(string headerName)
        {
            foreach (string header in Headers.Keys)
            {
                if (string.Compare(header, headerName, StringComparison.OrdinalIgnoreCase) == 0)
                {
                    return Headers[header];
                }
            }
            return null;
        }
    }
}

/* \Http\HttpServer.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides an lightweight HTTP server powered by Sisk.
    /// </summary>
    /// <definition>
    /// public class HttpServer : IDisposable
    /// </definition> 
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpServer : IDisposable
    {
        private bool _isListening = false;
        private bool _isDisposing = false;
        private HttpListener httpListener = new HttpListener();
        internal static string poweredByHeader = "";
        internal HttpEventSourceCollection _eventCollection = new HttpEventSourceCollection();
        internal HttpWebSocketConnectionCollection _wsCollection = new HttpWebSocketConnectionCollection();
        internal List<string>? listeningPrefixes;

        static HttpServer()
        {
            Version assVersion = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version!;
            poweredByHeader = $"Sisk/{assVersion.Major}.{assVersion.Minor}";
        }

        /// <summary>
        /// Outputs an non-listening HTTP server with configuration, listening host, and router.
        /// </summary>
        /// <remarks>This method is not appropriate to running production servers.</remarks>
        /// <param name="insecureHttpPort">The insecure port where the HTTP server will listen.</param>
        /// <param name="configuration">The <see cref="HttpServerConfiguration"/> object issued from this method.</param>
        /// <param name="host">The <see cref="ListeningHost"/> object issued from this method.</param>
        /// <param name="router">The <see cref="Router"/> object issued from this method.</param>
        /// <returns></returns>
        /// <definition>
        /// public static HttpServer Emit(in int insecureHttpPort, out HttpServerConfiguration configuration, out ListeningHost host, out Router router)
        /// </definition>
        /// <type>
        /// Static method 
        /// </type>
        public static HttpServer Emit(
                in int insecureHttpPort,
                out HttpServerConfiguration configuration,
                out ListeningHost host,
                out Router router
            )
        {
            router = new Router();
            if (insecureHttpPort == 0)
            {
                host = new ListeningHost();
                host.Router = router;
                host.Ports = new ListeningPort[]
                {
                    ListeningPort.GetRandomPort()
                };
            }
            else
            {
                host = new ListeningHost();
                host.Router = router;
                host.Ports = new ListeningPort[]
                {
                    new ListeningPort(false, "localhost", insecureHttpPort)
                };
            }
            configuration = new HttpServerConfiguration();
            configuration.ListeningHosts.Add(host);

            HttpServer server = new HttpServer(configuration);
            return server;
        }

        /// <summary>
        /// Gets or sets the Server Configuration object.
        /// </summary>
        /// <definition>
        /// public HttpServerConfiguration ServerConfiguration { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpServerConfiguration ServerConfiguration { get; set; } = new HttpServerConfiguration();

        /// <summary>
        /// Gets an boolean indicating if this HTTP server is running and listening.
        /// </summary>
        /// <definition>
        /// public bool IsListening { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IsListening { get => _isListening && !_isDisposing; }

        /// <summary>
        /// Gets an string array containing all URL prefixes which this HTTP server is listening to.
        /// </summary>
        /// <definition>
        /// public string ListeningPrefixes { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string[] ListeningPrefixes => listeningPrefixes?.ToArray() ?? Array.Empty<string>();

        /// <summary>
        /// Gets an <see cref="HttpEventSourceCollection"/> with active event source connections in this HTTP server.
        /// </summary>
        /// <definition>
        /// public HttpEventSourceCollection EventSources { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpEventSourceCollection EventSources { get => _eventCollection; }

        /// <summary>
        /// Gets an <see cref="HttpWebSocketConnectionCollection"/> with active Web Sockets connections in this HTTP server.
        /// </summary>
        /// <definition>
        /// public HttpWebSocketConnectionCollection WebSockets { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpWebSocketConnectionCollection WebSockets { get => _wsCollection; }

        /// <summary>
        /// Event that is called when this <see cref="HttpServer"/> computes an request and it's response.
        /// </summary>
        /// <definition>
        /// public event ServerExecutionEventHandler? OnConnectionClose;
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public event ServerExecutionEventHandler? OnConnectionClose;

        /// <summary>
        /// Event that is called when this <see cref="HttpServer"/> receives an request.
        /// </summary>
        /// <definition>
        /// public event ReceiveRequestEventHandler? OnConnectionOpen;
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public event ReceiveRequestEventHandler? OnConnectionOpen;

        /// <summary>
        /// Get Sisk version label.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string GetVersion()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string GetVersion() => poweredByHeader;

        /// <summary>
        /// Creates a new default configuration <see cref="Sisk.Core.Http.HttpServer"/> instance with the given Route and server configuration.
        /// </summary>
        /// <param name="configuration">The configuration object of the server.</param>
        /// <definition>
        /// public HttpServer(HttpServerConfiguration configuration)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpServer(HttpServerConfiguration configuration)
        {
            this.ServerConfiguration = configuration;
        }

        /// <summary>
        /// Restarts this HTTP server, sending all processing responses and starting them again, reading the listening ports again.
        /// </summary>
        /// <definition>
        /// public void Restart()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Restart()
        {
            Stop();
            Start();
        }

        /// <summary>
        /// Starts listening to the set port and handling requests on this server.
        /// </summary>
        /// <definition>
        /// public void Start()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Start()
        {
            if (this.ServerConfiguration.ListeningHosts is null)
            {
                throw new InvalidOperationException("Cannot start the HTTP server with no listening hosts.");
            }

            listeningPrefixes = new List<string>();
            foreach (ListeningHost listeningHost in this.ServerConfiguration.ListeningHosts)
            {
                foreach (ListeningPort port in listeningHost.Ports)
                {
                    string prefix = port.ToString();
                    if (!listeningPrefixes.Contains(prefix)) listeningPrefixes.Add(prefix);
                }
            }

            httpListener.Prefixes.Clear();
            foreach (string prefix in listeningPrefixes)
                httpListener.Prefixes.Add(prefix);

            _isListening = true;
            httpListener.Start();
            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
        }

        /// <summary>
        /// Stops the server from listening and stops the request handler.
        /// </summary>
        /// <definition>
        /// public void Stop()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Stop()
        {
            _isListening = false;
            httpListener.Stop();
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal static string HumanReadableSize(float? size)
        {
            if (size == null) return "";
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            int order = 0;
            while (size >= 1024 && order < sizes.Length - 1)
            {
                order++;
                size = size / 1024;
            }
            return string.Format("{0:0.##}{1}", size, sizes[order]);
        }

        private void TryCloseStream(HttpListenerResponse response)
        {
            try
            {
                response.Close();
            }
            catch (Exception)
            {
                ;
            }
        }

        internal static void SetCorsHeaders(HttpListenerRequest baseRequest, CrossOriginResourceSharingHeaders cors, HttpListenerResponse baseResponse)
        {
            if (cors.AllowHeaders.Length > 0) baseResponse.Headers.Set("Access-Control-Allow-Headers", string.Join(", ", cors.AllowHeaders));
            if (cors.AllowMethods.Length > 0) baseResponse.Headers.Set("Access-Control-Allow-Methods", string.Join(", ", cors.AllowMethods));
            if (cors.AllowOrigin != null) baseResponse.Headers.Set("Access-Control-Allow-Origin", cors.AllowOrigin);
            if (cors.AllowOrigins?.Length > 0)
            {
                string? origin = baseRequest.Headers["Origin"];
                if (origin != null)
                {
                    foreach (var definedOrigin in cors.AllowOrigins)
                    {
                        if (string.Compare(definedOrigin, origin, true) == 0)
                        {
                            baseResponse.Headers.Set("Access-Control-Allow-Origin", origin);
                            break;
                        }
                    }
                }
            }
            if (cors.AllowCredentials != null) baseResponse.Headers.Set("Access-Control-Allow-Credentials", cors.AllowCredentials.ToString()!.ToLower());
            if (cors.ExposeHeaders.Length > 0) baseResponse.Headers.Set("Access-Control-Expose-Headers", string.Join(", ", cors.ExposeHeaders));
            if (cors.MaxAge.TotalSeconds > 0) baseResponse.Headers.Set("Access-Control-Max-Age", cors.MaxAge.TotalSeconds.ToString());
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        private void ListenerCallback(IAsyncResult result)
        {
            if (_isDisposing || !_isListening)
                return;

            httpListener.BeginGetContext(new AsyncCallback(ListenerCallback), httpListener);
            HttpListenerContext context;
            HttpRequest request = null!;
            HttpResponse? response = null;

            try
            {
                context = httpListener.EndGetContext(result);
            }
            catch (Exception)
            {
                return;
            }

            HttpServerFlags flag = ServerConfiguration.Flags;
            Stopwatch sw = new Stopwatch();
            HttpListenerResponse baseResponse = context.Response;
            HttpListenerRequest baseRequest = context.Request;
            long incomingSize = 0;
            long outcomingSize = 0;
            bool closeStream = true;
            bool useCors = false;
            bool hasAccessLogging = ServerConfiguration.AccessLogsStream != null;
            bool hasErrorLogging = ServerConfiguration.ErrorsLogsStream != null;
            LogOutput logMode = LogOutput.Both;
            IPAddress otherParty = baseRequest.RemoteEndPoint.Address;
            Uri? connectingUri = baseRequest.Url;
            int responseStatus = 0;
            string responseDescription = "";
            NameValueCollection? reqHeaders = null;

            if (ServerConfiguration.DefaultCultureInfo != null)
            {
                Thread.CurrentThread.CurrentCulture = ServerConfiguration.DefaultCultureInfo;
                Thread.CurrentThread.CurrentUICulture = ServerConfiguration.DefaultCultureInfo;
            }

            HttpServerExecutionResult? executionResult = new HttpServerExecutionResult()
            {
                Request = request,
                Response = response,
                Status = HttpServerExecutionStatus.NoResponse
            };

            try
            {
                sw.Start();

                if (connectingUri is null)
                {
                    baseResponse.StatusCode = 400;
                    executionResult.Status = HttpServerExecutionStatus.DnsFailed;
                    return;
                }

                string dnsSafeHost = connectingUri.DnsSafeHost;
                string? forwardedHost = baseRequest.Headers["X-Forwarded-Host"];
                if (ServerConfiguration.ResolveForwardedOriginHost && forwardedHost != null)
                {
                    dnsSafeHost = forwardedHost;
                }

                // detect the listening host for this listener
                ListeningHost? matchedListeningHost = ServerConfiguration.ListeningHosts
                    .GetRequestMatchingListeningHost(dnsSafeHost, baseRequest.LocalEndPoint.Port);

                if (matchedListeningHost is null)
                {
                    baseResponse.StatusCode = 400; // Bad Request
                    executionResult.Status = HttpServerExecutionStatus.DnsUnknownHost;
                    return;
                }
                else
                {
                    request = new HttpRequest(baseRequest, baseResponse, this, matchedListeningHost, context);
                    reqHeaders = baseRequest.Headers;
                    if (ServerConfiguration.ResolveForwardedOriginAddress || ServerConfiguration.ResolveForwardedOriginHost)
                    {
                        otherParty = request.Origin;
                    }
                }

                if (matchedListeningHost.Router == null || !matchedListeningHost.CanListen)
                {
                    baseResponse.StatusCode = 503; // Service Unavailable
                    executionResult.Status = HttpServerExecutionStatus.ListeningHostNotReady;
                    return;
                }

                if (ServerConfiguration.IncludeRequestIdHeader)
                {
                    baseResponse.Headers.Set(flag.HeaderNameRequestId, request.RequestId.ToString());
                }

                if (OnConnectionOpen != null)
                    OnConnectionOpen(this, request);

                long requestMaxSize = ServerConfiguration.MaximumContentLength;
                if (requestMaxSize > 0 && baseRequest.ContentLength64 > requestMaxSize)
                {
                    executionResult.Status = HttpServerExecutionStatus.ContentTooLarge;
                    baseResponse.StatusCode = 413;
                    return;
                }

                incomingSize += request.CalcRequestSize();

                // check for illegal body content requests
                if (flag.ThrowContentOnNonSemanticMethods && (
                       request.Method == HttpMethod.Get
                    || request.Method == HttpMethod.Options
                    || request.Method == HttpMethod.Head
                    || request.Method == HttpMethod.Trace
                    ) && context.Request.ContentLength64 > 0)
                {
                    executionResult.Status = HttpServerExecutionStatus.ContentServedOnIllegalMethod;
                    baseResponse.StatusCode = 400;
                    return;
                }

                // bind
                matchedListeningHost.Router.BindServer(this);

                // aditional before-router flags
                if (flag.SendSiskHeader)
                    baseResponse.Headers.Set("X-Powered-By", poweredByHeader);

                // get response
                var routerResult = matchedListeningHost.Router.Execute(request, baseRequest);
                response = routerResult.Response;
                logMode = routerResult.Route?.LogMode ?? LogOutput.Both;
                useCors = routerResult.Route?.UseCors ?? true;

                if (response is null)
                {
                    executionResult.Status = HttpServerExecutionStatus.NoResponse;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_STREAM_CLOSE)
                {
                    executionResult.Status = HttpServerExecutionStatus.StreamClosed;
                    baseResponse.StatusCode = (int)response.Status;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_EMPTY)
                {
                    executionResult.Status = HttpServerExecutionStatus.NoResponse;
                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_ERROR)
                {
                    executionResult.Status = HttpServerExecutionStatus.UncaughtExceptionThrown;
                    executionResult.ServerException = routerResult.Exception;
                    baseResponse.StatusCode = 500;

                    return;
                }
                else if (response?.internalStatus == HttpResponse.HTTPRESPONSE_CLOSE)
                {
                    executionResult.Status = HttpServerExecutionStatus.ClosedStream;
                    baseResponse.Close();
                    return;
                }

                if (useCors && flag.SendCorsHeaders)
                {
                    SetCorsHeaders(baseRequest, matchedListeningHost.CrossOriginResourceSharingPolicy, baseResponse);
                }
                if (routerResult.Result == RouteMatchResult.OptionsMatched)
                {
                    logMode = flag.OptionsLogMode;
                }

                byte[] responseBytes = response!.Content?.ReadAsByteArrayAsync().Result ?? new byte[] { };

                if (response.CustomStatus != null)
                {
                    baseResponse.StatusCode = response.CustomStatus.Value.StatusCode;
                    baseResponse.StatusDescription = response.CustomStatus.Value.Description;
                    responseStatus = response.CustomStatus.Value.StatusCode;
                    responseDescription = response.CustomStatus.Value.Description;
                }
                else
                {
                    baseResponse.StatusCode = (int)response.Status;
                    responseStatus = baseResponse.StatusCode;
                    responseDescription = baseResponse.StatusDescription;
                }
                baseResponse.SendChunked = response.SendChunked;

                NameValueCollection resHeaders = new NameValueCollection
                {
                    response.Headers
                };

                foreach (string incameHeader in resHeaders)
                {
                    baseResponse.AddHeader(incameHeader, resHeaders[incameHeader] ?? "");
                }

                if (responseBytes.Length > 0)
                {
                    baseResponse.ContentType = resHeaders["Content-Type"] ?? response.Content?.Headers.ContentType?.ToString();

                    if (resHeaders["Content-Encoding"] != null)
                    {
                        baseResponse.ContentEncoding = Encoding.GetEncoding(resHeaders["Content-Encoding"]!);
                    }
                    else
                    {
                        baseResponse.ContentEncoding = ServerConfiguration.DefaultEncoding;
                    }

                    if (!response.SendChunked)
                    {
                        baseResponse.ContentLength64 = responseBytes.Length;
                    }

                    if (context.Request.HttpMethod != "HEAD")
                    {
                        baseResponse.OutputStream.Write(responseBytes);
                        outcomingSize += responseBytes.Length;
                    }
                }

                string httpStatusVerbose = $"{baseResponse.StatusCode} {baseResponse.StatusDescription}";

                executionResult.RequestSize = incomingSize;
                executionResult.ResponseSize = outcomingSize;
                executionResult.Response = response;

                sw.Stop();
                baseResponse.Close();
                baseRequest.InputStream.Close();

                closeStream = false;
                executionResult.Status = HttpServerExecutionStatus.Executed;
            }
            catch (ObjectDisposedException objException)
            {
                executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
                executionResult.ServerException = objException;
            }
            catch (HttpListenerException netException)
            {
                executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
                executionResult.ServerException = netException;
            }
            catch (HttpRequestException requestException)
            {
                baseResponse.StatusCode = 400;
                executionResult.Status = HttpServerExecutionStatus.MalformedRequest;
                executionResult.ServerException = requestException;
            }
            catch (Exception ex)
            {
                if (!ServerConfiguration.ThrowExceptions)
                {
                    executionResult.ServerException = ex;
                    executionResult.Status = HttpServerExecutionStatus.ExceptionThrown;
                }
                else
                {
                    throw;
                }
            }
            finally
            {
                if (closeStream)
                {
                    baseRequest.InputStream.Close();
                    TryCloseStream(baseResponse);
                }

                if (OnConnectionClose != null)
                {
                    // the "Request" variable was pointing to an null value before
                    // this line
                    executionResult.Request = request;
                    OnConnectionClose(this, executionResult);
                }

                bool canAccessLog = logMode.HasFlag(LogOutput.AccessLog) && hasAccessLogging;
                bool canErrorLog = logMode.HasFlag(LogOutput.ErrorLog) && hasErrorLogging;

                if (executionResult.ServerException != null && canErrorLog)
                {
                    StringBuilder exceptionStr = new StringBuilder();
                    exceptionStr.AppendLine($"Exception thrown at {DateTime.Now:R}");
                    exceptionStr.AppendLine($"-------------\nRequest:");
                    exceptionStr.AppendLine(request.GetRawHttpRequest(false));
                    exceptionStr.AppendLine($"\n-------------\nError contents:");
                    exceptionStr.AppendLine(executionResult.ServerException.ToString());

                    if (executionResult.ServerException.InnerException != null)
                    {
                        exceptionStr.AppendLine($"\n-------------\nInner exception:");
                        exceptionStr.AppendLine(executionResult.ServerException.InnerException.ToString());
                    }

                    ServerConfiguration.ErrorsLogsStream?.WriteLine(exceptionStr.ToString());
                }
                if (canAccessLog)
                {
                    var formatter = new LoggingFormatter(
                        executionResult,
                        DateTime.Now,
                        connectingUri,
                        otherParty,
                        reqHeaders,
                        responseStatus,
                        responseDescription,
                        incomingSize,
                        outcomingSize,
                        sw.ElapsedMilliseconds);

                    string line = ServerConfiguration.AccessLogsFormat;
                    formatter.Format(ref line);

                    ServerConfiguration.AccessLogsStream?.WriteLine(line);
                }
            }
        }

        /// <summary>
        /// Invalidates this class and releases the resources used by it, and permanently closes the HTTP server.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Dispose()
        {
            _isDisposing = true;
            this.httpListener.Close();
            this.ServerConfiguration.Dispose();
        }

        private enum StreamMethodCallback
        {
            Nothing,
            Abort,
            Close
        }
    }
}

/* \Http\HttpServerConfiguration.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides execution parameters for an <see cref="HttpServer"/>.
    /// </summary>
    /// <definition>
    /// public class HttpServerConfiguration : IDisposable
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpServerConfiguration : IDisposable
    {
        /// <summary>
        /// Gets or sets advanced flags and configuration settings for the HTTP server.
        /// </summary>
        public HttpServerFlags Flags { get; set; } = new HttpServerFlags();

        /// <summary>
        /// Gets or sets the access logging format for incoming HTTP requests.
        /// </summary>
        /// <definition>
        /// public string AccessLogsFormat { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string AccessLogsFormat { get; set; } = "%dd-%dm-%dy %th:%ti:%ts %ls %ri %rs://%ra%rz%rq [%sc %sd] %lin -> %lou in %lmsms";

        /// <summary>
        /// Gets or sets the default <see cref="CultureInfo"/> object which the HTTP server will apply to the request handlers and callbacks thread.
        /// </summary>
        /// <definition>
        /// public CultureInfo? DefaultCultureInfo { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public CultureInfo? DefaultCultureInfo { get; set; }

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> object which the HTTP server will write HTTP server access messages to.
        /// </summary>
        /// <remarks>
        /// This property defaults to Console.Out.
        /// </remarks> 
        /// <definition>
        /// public TextWriter? AccessLogsStream { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public LogStream? AccessLogsStream { get; set; } = LogStream.ConsoleOutput;

        /// <summary>
        /// Gets or sets the <see cref="TextWriter"/> object which the HTTP server will write HTTP server error transcriptions to.
        /// </summary>
        /// <remarks>
        /// This stream can be empty if ThrowExceptions is true.
        /// </remarks>
        /// <definition>
        /// public TextWriter? ErrorsLogsStream { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public LogStream? ErrorsLogsStream { get; set; }

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve remote (IP) addresses by the X-Forwarded-For header. This option is useful if you are using Sisk through a reverse proxy.
        /// </summary>
        /// <definition>
        /// public bool ResolveForwardedOriginAddress { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool ResolveForwardedOriginAddress { get; set; } = false;

        /// <summary>
        /// Gets or sets whether the HTTP server should resolve remote forwarded hosts by the header X-Forwarded-Host.
        /// </summary>
        /// <definition>
        /// public bool ResolveForwardedOriginHost { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool ResolveForwardedOriginHost { get; set; } = false;

        /// <summary>
        /// Gets or sets the default encoding for sending and decoding messages.
        /// </summary>
        /// <definition>
        /// public Encoding DefaultEncoding { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Encoding DefaultEncoding { get; set; } = Encoding.UTF8;

        /// <summary>
        /// Gets or sets the maximum size of a request body before it is closed by the socket.
        /// </summary>
        /// <definition>
        /// public long MaximumContentLength { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        /// <remarks>
        /// Leave it as "0" to set the maximum content length to unlimited.
        /// </remarks> 
        public long MaximumContentLength { get; set; } = 0;

        /// <summary>
        /// Gets or sets the message level the console will write. This property is now deprecated. Use <see cref="AccessLogsStream"/> or <see cref="ErrorsLogsStream"/> instead.
        /// </summary>
        /// <definition>
        /// [Obsolete]
        /// [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        /// public VerboseMode Verbose { get; set; }
        /// </definition>
        /// <remarks>
        /// Since Sisk 0.8.1, this property was deprecated. The defaults for the <see cref="AccessLogsStream"/> it's the <see cref="VerboseMode.Detailed"/> verbose mode.
        /// </remarks>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete("This property is deprecated and ins't used anymore. Please, use AccessLogsStream and ErrorLogsStream instead.")]
        public VerboseMode Verbose { get; set; } = VerboseMode.Normal;

        /// <summary>
        /// Gets or sets whether the server should include the "X-Request-Id" header in response headers.
        /// </summary>
        /// <definition>
        /// public bool IncludeRequestIdHeader { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IncludeRequestIdHeader { get; set; } = false;

        /// <summary>
        /// Gets or sets the listening hosts repository that the <see cref="HttpServer"/> instance will listen to.
        /// </summary>
        /// <definition>
        /// public ListeningHostRepository ListeningHosts { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHostRepository ListeningHosts { get; set; } = new ListeningHostRepository();

        /// <summary>
        /// Gets or sets whether the server should throw exceptions instead of reporting it on <see cref="HttpServerExecutionStatus"/> if any is thrown while processing requests.
        /// </summary>
        /// <definition>
        /// public bool ThrowExceptions { get; set; } = false;
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool ThrowExceptions { get; set; } = false;

        /// <summary>
        /// Creates an new <see cref="HttpServerConfiguration"/> instance with no parameters.
        /// </summary>
        /// <definition>
        /// public HttpServerConfiguration()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpServerConfiguration()
        {
        }

        /// <summary>
        /// Frees the resources and invalidates this instance.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Dispose()
        {
            ListeningHosts.Clear();
            AccessLogsStream?.Close();
            ErrorsLogsStream?.Close();
        }
    }

    /// <summary>
    /// Specifies the message level the <see cref="HttpServer"/> should display.
    /// </summary>
    /// <definition>
    /// public enum VerboseMode
    /// </definition> 
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public enum VerboseMode
    {
        /// <summary>
        /// No message will be written.
        /// </summary>
        Silent,

        /// <summary>
        /// Only summary messages will be written.
        /// </summary>
        Normal,

        /// <summary>
        /// Detailed messages will be written.
        /// </summary>
        Detailed
    }
}

/* \Http\HttpServerExecutionResult.cs */
namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents the function that is called when a server receives and computes a request.
    /// </summary>
    /// <param name="sender">The <see cref="HttpServer"/> calling the function.</param>
    /// <param name="e">Server request and operation information.</param>
    /// <definition>
    /// public delegate void ServerExecutionEventHandler(object sender, HttpServerExecutionResult e);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public delegate void ServerExecutionEventHandler(object sender, HttpServerExecutionResult e);

    /// <summary>
    /// Represents a function that is called when the server receives an HTTP request.
    /// </summary>
    /// <param name="sender">The <see cref="HttpServer"/> calling the function.</param>
    /// <param name="request">The received request.</param>
    /// <definition>
    /// public delegate void ReceiveRequestEventHandler(object sender, HttpRequest request);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public delegate void ReceiveRequestEventHandler(object sender, HttpRequest request);

    /// <summary>
    /// Represents the results of executing a request on the server.
    /// </summary>
    /// <definition>
    /// public class HttpServerExecutionResult
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpServerExecutionResult
    {
        /// <summary>
        /// Represents the request received in this diagnosis.
        /// </summary>
        /// <definition>
        /// public HttpRequest Request { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpRequest Request { get; internal set; } = null!;

        /// <summary>
        /// Represents the response sent by the server.
        /// </summary>
        /// <definition>
        /// public HttpResponse? Response { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse? Response { get; internal set; }

        /// <summary>
        /// Represents the status of server operation.
        /// </summary>
        /// <definition>
        /// public HttpServerExecutionStatus Status { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpServerExecutionStatus Status { get; internal set; }

        /// <summary>
        /// Gets the exception that was thrown when executing the route, if any.
        /// </summary>
        /// <definition>
        /// public Exception? ServerException { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Exception? ServerException { get; internal set; }

        /// <summary>
        /// Gets an boolean indicating if this execution status is an success status.
        /// </summary>
        /// <definition>
        /// public bool IsSuccessStatus { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IsSuccessStatus { get => Status == HttpServerExecutionStatus.Executed || Status == HttpServerExecutionStatus.StreamClosed; }

        /// <summary>
        /// Gets the request size in bytes.
        /// </summary>
        /// <definition>
        /// public long RequestSize { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public long RequestSize { get; internal set; }

        /// <summary>
        /// Gets the response size in bytes, if any.
        /// </summary>
        /// <definition>
        /// public long ResponseSize { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public long ResponseSize { get; internal set; }

        internal HttpServerExecutionResult() { }
    }

    /// <summary>
    /// Represents the status of an execution of a request on an <see cref="HttpServer"/>.
    /// </summary>
    /// <definition>
    /// public enum HttpServerExecutionStatus
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public enum HttpServerExecutionStatus
    {
        /// <summary>
        /// Represents that the request was executed by a router and its response was delivered.
        /// </summary>
        Executed,

        /// <summary>
        /// Represents that the request has sent an request body with an with a HTTP method that is not indicated for
        /// receiving request contents.
        /// </summary>
        ContentServedOnIllegalMethod,

        /// <summary>
        /// This enum value is deprecated. Use <see cref="ContentServedOnIllegalMethod"/> instead.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete]
        ContentServedOnNotSupportedMethod = ContentServedOnIllegalMethod,

        /// <summary>
        /// Represents that the content of the request is too large than what was configured on the server.
        /// </summary>
        ContentTooLarge,

        /// <summary>
        /// Represents that the connection stream was closed.
        /// </summary>
        StreamClosed,

        /// <summary>
        /// This enum value is deprecated. Use <see cref="StreamClosed"/> instead.
        /// </summary>
        [System.ComponentModel.EditorBrowsable(System.ComponentModel.EditorBrowsableState.Never)]
        [Obsolete]
        EventSourceClosed = StreamClosed,

        /// <summary>
        /// Represents that the router did not deliver a response to the received request.
        /// </summary>
        NoResponse,

        /// <summary>
        /// Represents that the client did not correctly specify a host in the request.
        /// </summary>
        DnsFailed,

        /// <summary>
        /// Represents that the client requested an host that's not been set up on this server.
        /// </summary>
        DnsUnknownHost,

        /// <summary>
        /// Indicates that the server encountered an exception while processing the request.
        /// </summary>
        ExceptionThrown,

        /// <summary>
        /// Indicates that the router encontered an uncaught exception while calling it's callback function.
        /// </summary>
        UncaughtExceptionThrown,

        /// <summary>
        /// Indicates that the DNS was successful, however the matched <see cref="ListeningHost"/> does not have an valid initialized router .
        /// </summary>
        ListeningHostNotReady,

        /// <summary>
        /// Indicates that the server cannot or will not process the request due to something that is perceived to be a client error.
        /// </summary>
        MalformedRequest,

        /// <summary>
        /// Indicates that the server closed the connection with the client.
        /// </summary>
        ClosedStream
    }
}

/* \Http\HttpServerFlags.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides advanced fields for Sisk server behavior.
    /// </summary>
    public class HttpServerFlags
    {
        /// <summary>
        /// Determines if the HTTP server should drop requests which has content body in GET, OPTIONS, HEAD and TRACE methods.
        /// </summary>
        public bool ThrowContentOnNonSemanticMethods = true;

        /// <summary>
        /// Determines the HTTP header name of the request ID.
        /// </summary>
        public string HeaderNameRequestId = "X-Request-Id";

        /// <summary>
        /// Determines if the HTTP server automatically should send CORS headers if set.
        /// </summary>
        public bool SendCorsHeaders = true;

        /// <summary>
        /// Determines if the HTTP server should automatically send HTTP headers of an pre-processed GET response if the request is using HEAD method.
        /// </summary>
        public bool TreatHeadAsGetMethod = true;

        /// <summary>
        /// Determines if the HTTP server should write log to OPTIONS requests.
        /// </summary>
        public LogOutput OptionsLogMode = LogOutput.Both;

        /// <summary>
        /// Determines if the HTTP server should send the X-Powered-By header in all responses.
        /// </summary>
        public bool SendSiskHeader = true;

        /// <summary>
        /// Determines the WebSocket buffer initial and max length.
        /// </summary>
        public int WebSocketBufferSize = 1024;

        /// <summary>
        /// Determines if the HTTP server should convert request headers encoding to the content encoding.
        /// </summary>
        public bool NormalizeHeadersEncodings = true;

        /// <summary>
        /// Determines if the HTTP server should automatically rewrite paths to end with /. Does not works with Regex routes.
        /// </summary>
        public bool ForceTrailingSlash = false;

        /// <summary>
        /// Creates an new <see cref="HttpServerFlags"/> instance with default flags values.
        /// </summary>
        public HttpServerFlags()
        {
        }
    }
}

/* \Http\HttpStatusCode.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents a structure that holds an HTTP response status information, with its code and description.
    /// </summary>
    /// <definition>
    /// public struct HttpStatusInformation
    /// </definition>
    /// <type>
    /// Structure
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public struct HttpStatusInformation
    {
        private int __statusCode;
        private string __description;

        /// <summary>
        /// Gets or sets the short description of the HTTP message.
        /// </summary>
        public string Description
        {
            get => __description;
            set
            {
                ValidateDescription(value);
                __description = value;
            }
        }

        /// <summary>
        /// Gets or sets the numeric HTTP status code of the HTTP message.
        /// </summary>
        public int StatusCode
        {
            get => __statusCode;
            set
            {
                ValidateStatusCode(value);
                __statusCode = value;
            }
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <param name="description">Sets the short description of the HTTP message.</param>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpStatusInformation(int statusCode, string description)
        {
            __description = description ?? throw new ArgumentNullException(nameof(description));
            __statusCode = statusCode;
            ValidateStatusCode(statusCode);
            ValidateDescription(description);
        }

        private void ValidateStatusCode(int st)
        {
            if (Math.Ceiling(Math.Log10(st)) != 3) throw new ArgumentException("The HTTP status code must be three-digits long.");
        }

        private void ValidateDescription(string s)
        {
            if (s.Length > 8192) throw new ArgumentException("The HTTP reason phrase must be equal or smaller than 8192 bytes.");
        }
    }
}

/* \Http\ListeningHost.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a structure to contain the fields needed by an http server host.
    /// </summary>
    /// <definition>
    /// public class ListeningHost
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class ListeningHost
    {
        private ListeningPort[] _ports = null!;
        internal int[] _numericPorts = null!;

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        /// <returns></returns>
        /// <definition>
        /// public override bool Equals(object? obj)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override bool Equals(object? obj)
        {
            ListeningHost? other = (obj as ListeningHost);
            if (other == null) return false;
            if (other._ports.Length != _ports.Length) return false;
            for (int i = 0; i < _ports.Length; i++)
            {
                ListeningPort A = this._ports[i];
                ListeningPort B = other._ports[i];
                if (!A.Equals(B)) return false;
            }
            return true;
        }

        /// <summary>
        /// Gets the hash code for this listening host.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public override int GetHashCode()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override int GetHashCode()
        {
            int hashCode = 0;
            foreach (var port in _ports)
            {
                hashCode ^= port.GetHashCode();
            }
            return hashCode;
        }

        /// <summary>
        /// Gets whether this <see cref="ListeningHost"/> can be listened by it's host <see cref="HttpServer"/>.
        /// </summary>
        /// <definition>
        /// public bool CanListen { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool CanListen { get => Router is not null; }

        /// <summary>
        /// Gets or sets the CORS sharing policy object.
        /// </summary>
        /// <definition>
        /// public Entity.CrossOriginResourceSharingHeaders? CrossOriginResourceSharingPolicy { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Entity.CrossOriginResourceSharingHeaders CrossOriginResourceSharingPolicy { get; set; } = CrossOriginResourceSharingHeaders.Empty;

        /// <summary>
        /// Gets or sets a label for this Listening Host.
        /// </summary>
        /// <definition>
        /// public string? Label { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public string? Label { get; set; } = null;

        /// <summary>
        /// Gets or sets the ports that this host will listen on.
        /// </summary>
        /// <definition>
        /// public ListeningPort[] Ports { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningPort[] Ports
        {
            get
            {
                return _ports;
            }
            set
            {
                _ports = value;
                _numericPorts = value.Select(p => p.Port).ToArray();
            }
        }

        /// <summary>
        /// Gets or sets the <see cref="Sisk.Core.Routing.Router"/> for this <see cref="ListeningHost"/> instance.
        /// </summary>
        /// <definition>
        /// public Router? Router { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public Router? Router { get; set; }

        /// <summary>
        /// Creates an new empty <see cref="ListeningHost"/> instance.
        /// </summary>
        /// <definition>
        /// public ListeningHost()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningHost()
        {
        }

        /// <summary>
        /// Creates an new <see cref="ListeningHost"/> instance with given URL.
        /// </summary>
        /// <param name="uri">The well formatted URL with scheme, hostname and port.</param>
        /// <param name="r">The router which will handle this listener requests.</param>
        /// <definition>
        /// public ListeningHost(string uri, Router r)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningHost(string uri, Router r)
        {
            this.Ports = new ListeningPort[] { new ListeningPort(uri) };
            this.Router = r;
        }
    }
}

/* \Http\ListeningHostRepository.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents an fluent repository of <see cref="ListeningHost"/> that can add, modify, or remove listening hosts while an <see cref="HttpServer"/> is running.
    /// </summary>
    /// <definition>
    /// public class ListeningHostRepository : ICollection&lt;ListeningHost&gt;, IEnumerable&lt;ListeningHost&gt;
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class ListeningHostRepository : ICollection<ListeningHost>, IEnumerable<ListeningHost>
    {
        private List<ListeningHost> _hosts = new List<ListeningHost>();

        /// <summary>
        /// Creates a new instance of an empty <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <definition>
        /// public ListeningHostRepository()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHostRepository()
        {
        }


        /// <summary>
        /// Creates a new instance of an <see cref="ListeningHostRepository"/> copying the items from another collection of <see cref="ListeningHost"/>.
        /// </summary>
        /// <param name="hosts">The collection which stores the <see cref="ListeningHost"/> which will be copied to this repository.</param>
        /// <definition>
        /// public ListeningHostRepository(IEnumerable&lt;ListeningHost&gt; hosts)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHostRepository(IEnumerable<ListeningHost> hosts)
        {
            _hosts.AddRange(hosts);
        }

        /// <summary>
        /// Gets the number of elements contained in this <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <definition>
        /// public int Count { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public int Count => _hosts.Count;

        /// <summary>
        /// Gets an boolean indicating if this <see cref="ListeningHostRepository"/> is read only. This property always returns <c>true</c>.
        /// </summary>
        /// <definition>
        /// public bool IsReadOnly { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool IsReadOnly => false;

        /// <summary>
        /// Adds a listeninghost to this repository. If this listeninghost already exists in this class, an exception will be thrown.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to add to this collection.</param>
        /// <definition>
        /// public bool IsReadOnly { get; }
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Add(ListeningHost item)
        {
            if (this.Contains(item)) throw new ArgumentOutOfRangeException("This ListeningHost has already been defined in this collection with identical definitions.");
            _hosts.Add(item);
        }

        /// <summary>
        /// Removes all listeninghosts from this repository.
        /// </summary>
        /// <definition>
        /// public void Clear()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Clear()
        {
            _hosts.Clear();
        }

        /// <summary>
        /// Determines if an <see cref="ListeningHost"/> is present in this repository.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to check if is present in this repository.</param>
        /// <returns></returns>
        /// <definition>
        /// public bool Contains(ListeningHost item)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Contains(ListeningHost item)
        {
            return _hosts.Contains(item);
        }

        /// <summary>
        /// Copies all elements from this repository to another compatible repository.
        /// </summary>
        /// <param name="array">The one-dimensional System.Array that is the destination of the elements copied.</param>
        /// <param name="arrayIndex">The zero-based index in array at which copying begins.</param>
        /// <definition>
        /// public void CopyTo(ListeningHost[] array, int arrayIndex)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void CopyTo(ListeningHost[] array, int arrayIndex)
        {
            _hosts.CopyTo(array, arrayIndex);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public IEnumerator&lt;ListeningHost&gt; GetEnumerator()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public IEnumerator<ListeningHost> GetEnumerator()
        {
            return _hosts.GetEnumerator();
        }

        /// <summary>
        /// Try to remove a <see cref="ListeningHost"/> from this repository. If the item is removed, this methods returns <c>true</c>.
        /// </summary>
        /// <param name="item">The <see cref="ListeningHost"/> to be removed.</param>
        /// <returns></returns>
        /// <definition>
        /// public bool Remove(ListeningHost item)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Remove(ListeningHost item)
        {
            return _hosts.Remove(item);
        }

        /// <summary>
        /// Returns an enumerator that iterates through this <see cref="ListeningHostRepository"/>.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// IEnumerator IEnumerable.GetEnumerator()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return this.GetEnumerator();
        }

        /// <summary>
        /// Gets a listening host through its index.
        /// </summary>
        /// <param name="index">The Listening Host index</param>
        /// <returns></returns>
        /// <definition>
        /// public ListeningHost this[int index]
        /// </definition>
        /// <type>
        /// Indexer
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public ListeningHost this[int index] { get => _hosts[index]; }

        internal ListeningHost? GetRequestMatchingListeningHost(string dnsSafeHost, int port)
        {
            foreach (ListeningHost h in this._hosts)
            {
                foreach (ListeningPort p in h.Ports)
                {
                    if (p.Port == port && WildcardMatching.IsDnsMatch(p.Hostname, dnsSafeHost))
                    {
                        return h;
                    }
                }
            }
            return null;
        }
    }
}

/* \Http\ListeningPort.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a structure to contain a listener port for an <see cref="ListeningHost"/> instance.
    /// </summary>
    /// <docs>
    ///     <p>
    ///         A listener port represents an access point on the HTTP server.
    ///         It consists of an indicator that it should use a secure connection (HTTPS), its hostname and port.
    ///     </p>
    ///     <p>
    ///         It must start with https:// or http://, and must terminate with an /.
    ///     </p>
    ///     <p>
    ///         It is represented by the syntax:
    ///     </p>
    ///     <pre><code class="lang-none">
    ///         [http|https]://[hostname]:[port]/
    ///     </code></pre>
    ///     <p>
    ///         Examples:
    ///     </p>
    ///     <pre><code class="lang-none">
    ///         http://localhost:80/
    ///         https://subdomain.domain.net:443/
    ///         http://182.32.112.223:5251/
    ///     </code></pre>
    /// </docs>
    /// <definition>
    /// public struct ListeningPort
    /// </definition>
    /// <type>
    /// Struct
    /// </type>
    public struct ListeningPort
    {
        /// <summary>
        /// Gets or sets the DNS hostname pattern where this listening port will refer.
        /// </summary>
        /// <definition>
        /// public string Hostname { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string Hostname { get; set; }

        /// <summary>
        /// Gets or sets the port where this listening port will refer.
        /// </summary>
        /// <definition>
        /// public int Port { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int Port { get; set; }

        /// <summary>
        /// Gets or sets whether the server should listen to this port securely (SSL).
        /// </summary>
        /// <definition>
        /// public bool Secure { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool Secure { get; set; }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on. If this port is the default HTTPS port (443), the class will have the property <see cref="Secure"/> to true.</param>
        /// <definition>
        /// public ListeningPort(int port)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningPort(int port)
        {
            this.Hostname = "localhost";
            this.Port = port;
            this.Secure = Port == 443;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port and secure context at the loopback host.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        /// <definition>
        /// public ListeningPort(int port, bool secure)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningPort(int port, bool secure)
        {
            this.Hostname = "localhost";
            this.Port = port;
            this.Secure = secure;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified port, secure context and hostname.
        /// </summary>
        /// <param name="port">The port the server will listen on.</param>
        /// <param name="secure">Indicates whether the server should listen to this port securely (SSL).</param>
        /// <param name="hostname">The hostname DNS pattern the server will listen to.</param>
        /// <definition>
        /// public ListeningPort(bool secure, string hostname, int port)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningPort(bool secure, string hostname, int port)
        {
            this.Hostname = hostname;
            this.Port = port;
            this.Secure = secure;
        }

        /// <summary>
        /// Creates an new <see cref="ListeningPort"/> instance with the specified URI.
        /// </summary>
        /// <param name="uri">The URI component that will be parsed to the listening port format.</param>
        /// <definition>
        /// public ListeningPort(string uri)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public ListeningPort(string uri)
        {
            int schemeIndex = uri.IndexOf(":");
            if (schemeIndex == -1) throw new ArgumentException("Scheme was not defined in the URI.");
            int portIndex = uri.IndexOf(":", schemeIndex + 3);
            if (portIndex == -1) throw new ArgumentException("The URI port must be explicitly defined.");
            int endIndex = uri.IndexOf("/", schemeIndex + 3);
            if (endIndex == -1 || !uri.EndsWith('/')) throw new ArgumentException("The URI must terminate with /.");

            string schemePart = uri.Substring(0, schemeIndex);
            string hostnamePart = uri.Substring(schemeIndex + 3, portIndex - (schemeIndex + 3));
            string portPart = uri.Substring(portIndex + 1, endIndex - (portIndex + 1));

            if (schemePart == "http")
            {
                this.Secure = false;
            }
            else if (schemePart == "https")
            {
                this.Secure = true;
            }
            else
            {
                throw new ArgumentException("The URI scheme must be http or https.");
            }

            if (!Int32.TryParse(portPart, out int port)) throw new ArgumentException("The URI port is invalid.");

            this.Port = port;
            this.Hostname = hostnamePart;
        }

        /// <summary>
        /// Determines if another object is equals to this class instance.
        /// </summary>
        /// <param name="obj">The another object which will be used to compare.</param>
        /// <returns></returns>
        /// <definition>
        /// public override bool Equals(object? obj)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override bool Equals(object? obj)
        {
            if (obj == null) return false;
            ListeningPort p = (ListeningPort)obj;
            return p.Secure == this.Secure && p.Port == this.Port && p.Hostname == this.Hostname;
        }

        /// <summary>
        /// Gets the hash code for this listening port.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public override int GetHashCode()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override int GetHashCode()
        {
            return (this.Secure.GetHashCode()) ^ (this.Port.GetHashCode()) ^ (this.Hostname.GetHashCode());
        }

        /// <summary>
        /// Gets an <see cref="ListeningPort"/> object with an random insecure port.
        /// </summary>
        /// <definition>
        /// public static ListeningPort GetRandomPort()
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static ListeningPort GetRandomPort()
        {
            TcpListener l = new TcpListener(IPAddress.Loopback, 0);
            l.Start();
            int port = ((IPEndPoint)l.LocalEndpoint).Port;
            l.Stop();
            return new ListeningPort(port, false);
        }

        /// <summary>
        /// Gets an string representation of this <see cref="ListeningPort"/>.
        /// </summary>
        /// <definition>
        /// public override string ToString()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override string ToString()
        {
            return $"{(Secure ? "https" : "http")}://{Hostname}:{Port}/";
        }
    }
}

/* \Http\LogStream.cs */


namespace Sisk.Core.Http
{
    /// <summary>
    /// Provides a managed, asynchronous log writer which supports writing safe data to log files or streams.
    /// </summary>
    /// <definition>
    /// public class LogStream : IDisposable
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http 
    /// </namespace>
    public class LogStream : IDisposable
    {
        private Queue<object?> logQueue = new Queue<object?>();
        private ManualResetEvent watcher = new ManualResetEvent(false);
        private ManualResetEvent waiter = new ManualResetEvent(false);
        private ManualResetEvent terminate = new ManualResetEvent(false);
        private Thread loggingThread;
        private bool isBlocking = false;

        /// <summary>
        /// Represents a LogStream that writes its output to the <see cref="Console.Out"/> stream.
        /// </summary>
        /// <definition>
        /// public static LogStream ConsoleOutput;
        /// </definition>
        /// <type>
        /// Field
        /// </type>
        public static LogStream ConsoleOutput = new LogStream(Console.Out);

        /// <summary>
        /// Gets the absolute path to the file where the log is being written to.
        /// </summary>
        /// <definition>
        /// public string? FilePath { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? FilePath { get; private set; } = null;

        /// <summary>
        /// Gets the <see cref="TextWriter"/> object where the log is being written to.
        /// </summary>
        /// <definition>
        /// public TextWriter? TextWriter { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public TextWriter? TextWriter { get; private set; } = null;

        /// <summary>
        /// Gets or sets the encoding used for writting data to the output file. This property is only appliable if
        /// this instance is using an file-based output.
        /// </summary>
        /// <definition>
        /// public Encoding Encoding { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public Encoding Encoding { get; set; } = Encoding.UTF8;

        private LogStream()
        {
            loggingThread = new Thread(new ThreadStart(ProcessQueue));
            loggingThread.IsBackground = true;
            loggingThread.Start();
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given TextWriter object.
        /// </summary>
        /// <param name="tw">Represents the writer which this instance will write log to.</param>
        /// <definition>
        /// public LogStream(TextWriter tw)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public LogStream(TextWriter tw) : this()
        {
            TextWriter = tw;
        }

        /// <summary>
        /// Creates an new <see cref="LogStream"/> instance with the given relative or absolute file path.
        /// </summary>
        /// <param name="filename">The file path where this instance will write log to.</param>
        /// <definition>
        /// public LogStream(string filename)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public LogStream(string filename) : this()
        {
            FilePath = Path.GetFullPath(filename);
        }

        /// <summary>
        /// Reads the last few lines of the linked log file.
        /// </summary>
        /// <param name="lines">The amount of lines to be read from the file.</param>
        /// <returns></returns>
        /// <exception cref="NotSupportedException">Thrown when used with a log file for a stream, textwriter, or other non-file structure.</exception>
        /// <definition>
        /// public string[] Peek(int lines)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public string[] Peek(int lines)
        {
            if (this.FilePath == null)
            {
                throw new NotSupportedException("This method only works when the LogStream is appending content to an file.");
            }

            string[] output = Array.Empty<string>();
            if (File.Exists(this.FilePath))
            {
                output = File.ReadLines(this.FilePath, this.Encoding).TakeLast(lines).ToArray();
            }

            return output;
        }

        /// <summary>
        /// Waits for the log to finish writing the current queue state.
        /// </summary>
        /// <param name="blocking">Block next writings until that instance is released by the <see cref="Set"/> method.</param>
        /// <definition>
        /// public void Wait(bool blocking = false)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Wait(bool blocking = false)
        {
            if (blocking)
            {
                watcher.Reset();
                isBlocking = true;
            }
            waiter.WaitOne();
        }

        /// <summary>
        /// Releases the execution of the queue.
        /// </summary>
        /// <definition>
        /// public void Set()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Set()
        {
            watcher.Set();
            isBlocking = false;
        }

        private void setWatcher()
        {
            if (!isBlocking)
                watcher.Set();
        }

        private void ProcessQueue()
        {
            while (true)
            {
                waiter.Set();
                int i = WaitHandle.WaitAny(new WaitHandle[] { watcher, terminate });
                if (i == 1) return; // terminate

                //Console.WriteLine("{0,20}{1,20} {2}", "", "queue ++", logQueue.Count);

                watcher.Reset();
                waiter.Reset();

                object?[] copy;
                lock (logQueue)
                {
                    copy = logQueue.ToArray();
                    logQueue.Clear();
                }

                foreach (object? line in copy)
                {
                    if (FilePath != null)
                    {
                        File.AppendAllText(FilePath!, line?.ToString(), Encoding);
                    }
                    else if (TextWriter != null)
                    {
                        TextWriter?.Write(line);
                        TextWriter?.Flush();
                    }
                    else
                    {
                        throw new InvalidOperationException("There is no valid output for log writing.");
                    }
                }

                //Console.WriteLine("{0,20}{1,20}", "", "queue --");
            }
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        /// <definition>
        /// public void Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Close() => Dispose();

        /// <summary>
        /// Writes an exception description in the log.
        /// </summary>
        /// <param name="exp">The exception which will be written.</param>
        /// <definition>
        /// public void WriteException(Exception exp)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteException(Exception exp)
        {
            StringBuilder exceptionStr = new StringBuilder();
            exceptionStr.AppendLine($"Exception thrown at {DateTime.Now:R}");
            exceptionStr.AppendLine(exp.ToString());

            if (exp.InnerException != null)
            {
                exceptionStr.AppendLine($"\n-------------\nInner exception:");
                exceptionStr.AppendLine(exp.InnerException.ToString());
            }
            WriteLine(exceptionStr);
        }

        /// <summary>
        /// Writes an line-break at the end of the output.
        /// </summary>
        /// <definition>
        /// public void WriteLine()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine()
        {
            lock (logQueue)
            {
                logQueue.Enqueue("\n");
                setWatcher();
            }
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        /// <definition>
        /// public void WriteLine(object? message)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine(object? message)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(message?.ToString() + "\n");
                setWatcher();
            }
        }

        /// <summary>
        /// Writes the text and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="message">The text that will be written in the output.</param>
        /// <definition>
        /// public void WriteLine(string message)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine(string message)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(message + "\n");
                setWatcher();
            }
        }

        /// <summary>
        /// Writes the text format and arguments and concats an line-break at the end into the output.
        /// </summary>
        /// <param name="format">The string format that represents the arguments positions.</param>
        /// <param name="args">An array of objects that represents the string format slots values.</param>
        /// <definition>
        /// public void WriteLine(string format, params object?[] args)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WriteLine(string format, params object?[] args)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(string.Format(format, args) + "\n");
                setWatcher();
            }
        }

        /// <summary>
        /// Writes the text into the output.
        /// </summary>
        /// <param name="value">The text which will be inserted at the output.</param>
        /// <definition>
        /// public void Write(object? value)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Write(object? value)
        {
            lock (logQueue)
            {
                logQueue.Enqueue(value);
                setWatcher();
            }
        }

        /// <summary>
        /// Writes all pending logs from the queue and closes all resources used by this object.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Dispose()
        {
            terminate.Set();
            loggingThread.Join();
            logQueue.Clear();
            TextWriter?.Flush();
            TextWriter?.Close();
        }
    }
}

/* \Http\Streams\HttpEventSourceCollection.cs */


namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// Provides a managed object to manage <see cref="HttpRequestEventSource"/> connections.
    /// </summary>
    /// <definition>
    /// public sealed class HttpEventSourceCollection
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class HttpEventSourceCollection
    {
        internal List<HttpRequestEventSource> _eventSources = new List<HttpRequestEventSource>();

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpRequestEventSource"/> is registered in this collection.
        /// </summary>
        /// <definition>
        /// public event EventSourceRegistrationHandler? OnEventSourceRegistered;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event EventSourceRegistrationHandler? OnEventSourceRegistered;

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpRequestEventSource"/> is closed and removed from this collection.
        /// </summary>
        /// <definition>
        /// public event EventSourceUnregistrationHandler? OnEventSourceUnregistration;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event EventSourceUnregistrationHandler? OnEventSourceUnregistration;

        internal HttpEventSourceCollection()
        {
        }

        internal void UnregisterEventSource(HttpRequestEventSource eventSource)
        {
            lock (_eventSources)
            {
                if (_eventSources.Remove(eventSource) && OnEventSourceUnregistration != null)
                {
                    OnEventSourceUnregistration(this, eventSource);
                }
            }
        }

        internal void RegisterEventSource(HttpRequestEventSource src)
        {
            if (src.Identifier != null)
            {
                lock (_eventSources)
                {
                    HttpRequestEventSource[] toClose = Find(p => p == src.Identifier);
                    foreach (HttpRequestEventSource ev in toClose)
                    {
                        ev.Close();
                    }
                    _eventSources.Add(src);
                }
                if (OnEventSourceRegistered != null)
                    OnEventSourceRegistered(this, src);
            }
        }

        /// <summary>
        /// Gets an number indicating the amount of active event source connections.
        /// </summary>
        /// <definition>
        /// public int ActiveConnections { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int ActiveConnections { get => _eventSources.Count(ev => ev.IsActive); }

        /// <summary>
        /// Gets the event source connection for the specified identifier.
        /// </summary>
        /// <param name="identifier">The event source identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpRequestEventSource? GetByIdentifier(string identifier)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpRequestEventSource? GetByIdentifier(string identifier)
        {
            lock (_eventSources)
            {
                HttpRequestEventSource? src = _eventSources.Where(es => es.Identifier == identifier).FirstOrDefault();
                return src;
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpRequestEventSource"/> instances that matches their identifier predicate.
        /// </summary>
        /// <param name="predicate">The expression on the an non-empty event source identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpRequestEventSource[] Find(Func&lt;string, bool&gt; predicate)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpRequestEventSource[] Find(Func<string, bool> predicate)
        {
            lock (_eventSources)
            {
                return _eventSources.Where(e =>
                {
                    if (!e.IsActive || e.Identifier == null) return false;
                    return predicate(e.Identifier);
                }).ToArray();
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpRequestEventSource"/> instances.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public HttpRequestEventSource[] All()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpRequestEventSource[] All()
        {
            lock (_eventSources)
            {
                return _eventSources.Where(e => e.IsActive).ToArray();
            }
        }

        /// <summary>
        /// Closes and disposes all registered and active <see cref="HttpRequestEventSource"/> in this collections.
        /// </summary>
        /// <definition>
        /// public void DropAll()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void DropAll()
        {
            lock (_eventSources)
            {
                foreach (HttpRequestEventSource es in _eventSources) es.Dispose();
            }
        }
    }

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpEventSourceCollection"/> registers an new event source connection.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpEventSourceCollection"/> object.</param>
    /// <param name="eventSource">Represents the registered <see cref="HttpRequestEventSource"/> event source connection.</param>
    /// <definition>
    /// public delegate void EventSourceRegistrationHandler(object sender, HttpRequestEventSource eventSource);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void EventSourceRegistrationHandler(object sender, HttpRequestEventSource eventSource);

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpEventSourceCollection"/> is removed and had their connection closed.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpEventSourceCollection"/> object.</param>
    /// <param name="eventSource">Represents the closed <see cref="HttpRequestEventSource"/> event source connection.</param>
    /// <definition>
    /// public delegate void EventSourceUnregistrationHandler(object sender, HttpRequestEventSource eventSource);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void EventSourceUnregistrationHandler(object sender, HttpRequestEventSource eventSource);
}

/* \Http\Streams\HttpRequestEventSource.cs */


namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// An <see cref="HttpRequestEventSource"/> instance opens a persistent connection to the request, which sends events in text/event-stream format.
    /// </summary>
    /// <definition>
    /// public class HttpRequestEventSource : IDisposable
    /// </definition>
    /// <type>
    /// Class 
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpRequestEventSource : IDisposable
    {
        private ManualResetEvent terminatingMutex = new ManualResetEvent(false);
        private HttpListenerResponse res;
        private HttpListenerRequest req;
        private HttpRequest reqObj;
        private HttpServer hostServer;
        private List<string> sendQueue = new List<string>();
        bool hasSentData = false;
        int length = 0;
        TimeSpan keepAlive = TimeSpan.Zero;
        DateTime lastSuccessfullMessage = DateTime.Now;

        // 
        // isClosed determines if this instance has some connection or not
        // isDisposed determines if this object was removed from their collection but wasnt collected by gc yet
        // 

        private bool isClosed = false;
        private bool isDisposed = false;

        /// <summary>
        /// Gets the <see cref="Http.HttpRequest"/> object which created this Event Source instance.
        /// </summary>
        /// <definition>
        /// public HttpRequest HttpRequest { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpRequest HttpRequest => reqObj;

        /// <summary>
        /// Gets an integer indicating the total bytes sent by this instance to the client.
        /// </summary>
        /// <definition>
        /// public int SentContentLength { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int SentContentLength { get => length; }

        /// <summary>
        /// Gets an unique identifier label to this EventStream connection, useful for finding this connection's reference later.
        /// </summary>
        /// <definition>
        /// public string? Identifier { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? Identifier { get; private set; }

        /// <summary>
        /// Gets an boolean indicating if this connection is open and this instance can send messages.
        /// </summary>
        /// <definition>
        /// public bool IsDisposed { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsActive { get; private set; }

        internal HttpRequestEventSource(string? identifier, HttpListenerResponse res, HttpListenerRequest req, HttpRequest host)
        {
            this.res = res ?? throw new ArgumentNullException(nameof(res));
            this.req = req ?? throw new ArgumentNullException(nameof(req));
            Identifier = identifier;
            hostServer = host.baseServer;
            reqObj = host;

            hostServer._eventCollection.RegisterEventSource(this);

            IsActive = true;

            res.AddHeader("Cache-Control", "no-store, no-cache");
            res.AddHeader("Content-Type", "text/event-stream");
            res.AddHeader("X-Powered-By", HttpServer.poweredByHeader);
            HttpServer.SetCorsHeaders(req, host.hostContext.CrossOriginResourceSharingPolicy, res);
        }

        private void keepAliveTask()
        {
            while (IsActive)
            {
                if (lastSuccessfullMessage < DateTime.Now - keepAlive)
                {
                    Dispose();
                    break;
                }
                else
                {
                    Thread.Sleep(1000);
                }
            }
        }

        /// <summary>
        /// Sends an header to the streaming context.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <definition>
        /// public void AppendHeader(string name, string value)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void AppendHeader(string name, string value)
        {
            if (hasSentData)
            {
                throw new InvalidOperationException("It's not possible to set headers after a message has been sent in this instance.");
            }
            res.AddHeader(name, value);
        }

        /// <summary>
        /// Writes a event message with their data to the event listener and returns an boolean indicating if the message was delivered to the client.
        /// </summary>
        /// <param name="data">The message text.</param>
        /// <definition>
        /// public bool Send(string data)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Send(string data)
        {
            if (!IsActive)
            {
                return false;
            }
            hasSentData = true;
            sendQueue.Add($"data: {data}\n\n");
            Flush();
            return true;
        }

        /// <summary>
        /// Writes a event message with their data to the event listener and returns an boolean indicating if the message was delivered to the client.
        /// </summary>
        /// <param name="data">The message object.</param>
        /// <definition>
        /// public bool Send(object? data)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public bool Send(object? data)
        {
            if (!IsActive)
            {
                return false;
            }
            hasSentData = true;
            sendQueue.Add($"data: {data?.ToString()}\n\n");
            Flush();
            return true;
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution. This method
        /// is released when either the client or the server reaches an sending failure.
        /// </summary>
        /// <definition>
        /// public void KeepAlive()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void KeepAlive()
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("Cannot keep alive an instance that has it's connection disposed.");
            }
            terminatingMutex.WaitOne();
        }

        /// <summary>
        /// Asynchronously waits for the connection to close before continuing execution with
        /// an maximum keep alive timeout. This method is released when either the client or the server reaches an sending failure.
        /// </summary>
        /// <param name="maximumIdleTolerance">The maximum timeout interval for an idle connection to automatically release this method.</param>
        /// <definition>
        /// public void KeepAlive()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void KeepAlive(TimeSpan maximumIdleTolerance)
        {
            if (!IsActive)
            {
                throw new InvalidOperationException("Cannot keep alive an instance that has it's connection disposed.");
            }
            keepAlive = maximumIdleTolerance;
            new Task(keepAliveTask).Start();
            terminatingMutex.WaitOne();
        }

        /// <summary>
        /// Closes the event listener and it's connection.
        /// </summary>
        /// <definition>
        /// public HttpResponse Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse Close()
        {
            if (!isClosed)
            {
                isClosed = true;
                Flush();
                Dispose();
                hostServer._eventCollection.UnregisterEventSource(this);
            }
            return new HttpResponse(HttpResponse.HTTPRESPONSE_STREAM_CLOSE)
            {
                CalculedLength = length
            };
        }

        /// <summary>
        /// Cancels the sending queue from sending pending messages and clears the queue.
        /// </summary>
        /// <definition>
        /// public void Cancel()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Cancel()
        {
            sendQueue.Clear();
        }

        private void Flush()
        {
            for (int i = 0; i < sendQueue.Count; i++)
            {
                if (isClosed)
                {
                    return;
                }
                string item = sendQueue[i];
                byte[] itemBytes = req.ContentEncoding.GetBytes(item);
                try
                {
                    res.OutputStream.Write(itemBytes);
                    length += itemBytes.Length;
                    sendQueue.RemoveAt(0);
                    lastSuccessfullMessage = DateTime.Now;
                }
                catch (Exception)
                {
                    Dispose();
                }
            }
        }

        /// <summary>
        /// Flushes and releases the used resources of this class instance.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method 
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Dispose()
        {
            if (isDisposed) return;
            Close();
            sendQueue.Clear();
            terminatingMutex.Set();
            IsActive = false;
            isDisposed = true;
        }
    }
}

/* \Http\Streams\HttpWebSocket.cs */


namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// Provides an persistent bi-directional socket between the client and the HTTP server.
    /// </summary>
    /// <definition>
    /// public sealed class HttpWebSocket
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class HttpWebSocket
    {
        internal string? identifier = null;
        internal HttpListenerWebSocketContext ctx;
        internal HttpRequest request;
        bool isListening = true;
        internal bool isClosed = false;
        internal TimeSpan closeTimeout = TimeSpan.Zero;
        internal bool isWaitingNext = false;

        internal WebSocketMessage? lastMessage = null;
        internal CancellationTokenSource asyncListenerToken = null!;
        internal ManualResetEvent closeEvent = new ManualResetEvent(false);
        internal ManualResetEvent waitNextEvent = new ManualResetEvent(false);
        internal Thread receiveThread;

        int bufferLength = 0;

        /// <summary>
        /// Gets or sets an object linked with this <see cref="WebSocket"/> session.
        /// </summary>
        /// <definition>
        /// public object? State { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public object? State { get; set; }

        /// <summary>
        /// Gets the <see cref="Sisk.Core.Http.HttpRequest"/> object which created this Web Socket instance.
        /// </summary>
        /// <definition>
        /// public HttpRequest HttpRequest { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpRequest HttpRequest => request;

        /// <summary>
        /// Gets an boolean indicating if this Web Socket connection is closed.
        /// </summary>
        /// <definition>
        /// public bool IsClosed { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsClosed => isClosed;

        /// <summary>
        /// Gets an unique identifier label to this Web Socket connection, useful for finding this connection's reference later.
        /// </summary>
        /// <definition>
        /// public string? Identifier { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? Identifier => identifier;

        /// <summary>
        /// Represents the event which is called when this websocket receives an message from
        /// remote origin.
        /// </summary>
        /// <definition>
        /// public event WebSocketMessageReceivedEventHandler? OnReceive;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event WebSocketMessageReceivedEventHandler? OnReceive = null;

        internal HttpWebSocket(HttpListenerWebSocketContext ctx, HttpRequest req, string? identifier)
        {
            this.ctx = ctx;
            request = req;
            bufferLength = request.baseServer.ServerConfiguration.Flags.WebSocketBufferSize;
            this.identifier = identifier;

            if (identifier != null)
            {
                req.baseServer._wsCollection.RegisterWebSocket(this);
            }

            RecreateAsyncToken();

            receiveThread = new Thread(new ThreadStart(ReceiveTask));
            receiveThread.IsBackground = true;
            receiveThread.Start();
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        void RecreateAsyncToken()
        {
            asyncListenerToken = new CancellationTokenSource();
            if (closeTimeout.TotalMilliseconds > 0)
                asyncListenerToken.CancelAfter(closeTimeout);
            asyncListenerToken.Token.ThrowIfCancellationRequested();
        }

        void TrimMessage(WebSocketReceiveResult result, WebSocketMessage message)
        {
            if (result.Count < message.Length)
            {
                byte[] trimmed = new byte[result.Count];
                for (int i = 0; i < trimmed.Length; i++)
                {
                    trimmed[i] = message.MessageBytes[i];
                }
                message.__msgBytes = trimmed;
            }
            message.IsClose = result.MessageType == WebSocketMessageType.Close;
            message.IsEnd = result.EndOfMessage;

            if (result.MessageType == WebSocketMessageType.Close)
            {
                isClosed = true;
                isListening = false;
                closeEvent.Set();
            }
        }

        internal async void ReceiveTask()
        {
            while (isListening)
            {
                WebSocketMessage message = new WebSocketMessage(this, bufferLength);

                var arrSegment = new ArraySegment<byte>(message.__msgBytes);
                WebSocketReceiveResult result;

                try
                {
                    result = await ctx.WebSocket.ReceiveAsync(arrSegment, asyncListenerToken.Token);
                }
                catch (Exception)
                {
                    continue;
                }

                TrimMessage(result, message);
                if (isWaitingNext)
                {
                    isWaitingNext = false;
                    lastMessage = message;
                    waitNextEvent.Set();
                }
                else
                {
                    if (OnReceive != null) OnReceive(this, message);
                }
            }
        }

        /// <summary>
        /// Sends an text message to the remote point.
        /// </summary>
        /// <param name="message">The target message which will be as an encoded UTF-8 string.</param>
        /// <definition>
        /// public void Send(string message)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(string message)
        {
            byte[] messageBytes = Encoding.UTF8.GetBytes(message);
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte>(messageBytes);
            SendInternal(span, WebSocketMessageType.Text);
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        /// <definition>
        /// public void Send(byte[] buffer)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(byte[] buffer)
        {
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte>(buffer);
            SendInternal(span, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte array.</param>
        /// <param name="start">The index at which to begin the memory.</param>
        /// <param name="length">The number of items in the memory.</param>
        /// <definition>
        /// public void Send(byte[] buffer, int start, int length)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(byte[] buffer, int start, int length)
        {
            ReadOnlyMemory<byte> span = new ReadOnlyMemory<byte>(buffer, start, length);
            SendInternal(span, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// Sends an binary message to the remote point.
        /// </summary>
        /// <param name="buffer">The target byte memory.</param>
        /// <definition>
        /// public void Send(ReadOnlyMemory&lt;byte&gt; buffer)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void Send(ReadOnlyMemory<byte> buffer)
        {
            SendInternal(buffer, WebSocketMessageType.Binary);
        }

        /// <summary>
        /// Closes the connection between the client and the server and returns an Http resposne indicating that the connection has been terminated.
        /// This method will not throw an exception if the connection is already closed.
        /// </summary>
        /// <definition>
        /// public HttpResponse Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpResponse Close()
        {
            if (!isClosed)
            {
                ctx.WebSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, null, CancellationToken.None).Wait();
                isListening = false;
                isClosed = true;
                closeEvent.Set();
            }
            request.baseServer._wsCollection.UnregisterWebSocket(this);
            return new HttpResponse(HttpResponse.HTTPRESPONSE_STREAM_CLOSE);
        }

        private void SendInternal(ReadOnlyMemory<byte> buffer, WebSocketMessageType msgType)
        {
            if (isClosed) { return; }

            int totalLength = buffer.Length;
            int chunks = Math.Max(totalLength / bufferLength, 1);

            for (int i = 0; i < chunks; i++)
            {
                int ca = i * bufferLength;
                int cb = Math.Min(ca + bufferLength, buffer.Length);

                ReadOnlyMemory<byte> chunk = buffer[ca..cb];

                ctx.WebSocket.SendAsync(chunk, msgType, i + 1 == chunks, CancellationToken.None);
            }
        }

        /// <summary>
        /// Blocks the current call stack until the connection is terminated by the client or the server, limited to the maximum
        /// timeout.
        /// </summary>
        /// <param name="timeout">Defines the timeout timer before the connection expires without any message.</param>
        /// <definition>
        /// public void WaitForClose(TimeSpan timeout)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WaitForClose(TimeSpan timeout)
        {
            this.closeTimeout = timeout;
            closeEvent.WaitOne();
        }

        /// <summary>
        /// Blocks the current call stack until the connection is terminated by either the client or the server.
        /// </summary>
        /// <definition>
        /// public void WaitForClose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void WaitForClose()
        {
            closeEvent.WaitOne();
        }

        /// <summary>
        /// Blocks the current thread and waits the next incoming message from this web socket instance.
        /// </summary>
        /// <remarks>
        /// Null is returned if a connection error is thrown.
        /// </remarks>
        /// <definition>
        /// public WebSocketMessage? WaitNext()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public WebSocketMessage? WaitNext()
        {
            waitNextEvent.Reset();
            isWaitingNext = true;
            waitNextEvent.WaitOne();
            return lastMessage;
        }
    }

    /// <summary>
    /// Represents the void that is called when the Web Socket receives an message.
    /// </summary>
    /// <param name="sender">The <see cref="HttpWebSocket"/> object which fired the event.</param>
    /// <param name="message">The Web Socket message information.</param>
    /// <definition>
    /// public delegate void WebSocketMessageReceivedEventHandler(object? sender, WebSocketMessage message);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void WebSocketMessageReceivedEventHandler(object? sender, WebSocketMessage message);

    /// <summary>
    /// Represents an websocket request message received by an websocket server.
    /// </summary>
    /// <definition>
    /// public sealed class WebSocketMessage
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public sealed class WebSocketMessage
    {
        internal byte[] __msgBytes;

        /// <summary>
        /// Gets an boolean indicating that this message is the last chunk of the message.
        /// </summary>
        /// <definition>
        /// public bool IsEnd { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsEnd { get; internal set; }

        /// <summary>
        /// Gets an boolean indicating that this message is an remote closing message.
        /// </summary>
        /// <definition>
        /// public bool IsClose { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public bool IsClose { get; internal set; }

        /// <summary>
        /// Gets an byte array with the message contents.
        /// </summary>
        /// <definition>
        /// public byte[] MessageBytes { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public byte[] MessageBytes => __msgBytes;

        /// <summary>
        /// Gets the message length in byte count.
        /// </summary>
        /// <definition>
        /// public int Length { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public int Length => __msgBytes.Length;

        /// <summary>
        /// Gets the sender <see cref="HttpWebSocket"/> object instance which received this message.
        /// </summary>
        /// <definition>
        /// public HttpWebSocket Sender { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public HttpWebSocket Sender { get; internal set; }

        /// <summary>
        /// Reads the message bytes as string using the specified encoding.
        /// </summary>
        /// <param name="encoder">The encoding which will be used to decode the message.</param>
        /// <definition>
        /// public string GetString(System.Text.Encoding encoder)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public string GetString(System.Text.Encoding encoder)
        {
            return encoder.GetString(MessageBytes);
        }

        /// <summary>
        /// Reads the message bytes as string using the UTF-8 text encoding.
        /// </summary>
        /// <definition>
        /// public string GetString()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public string GetString()
        {
            return GetString(Encoding.UTF8);
        }

        internal WebSocketMessage(HttpWebSocket httpws, int bufferLen)
        {
            Sender = httpws;
            __msgBytes = new byte[bufferLen];
        }
    }
}

/* \Http\Streams\HttpWebSocketConnectionCollection.cs */


namespace Sisk.Core.Http.Streams
{
    /// <summary>
    /// Provides a managed object to manage <see cref="HttpWebSocket"/> connections.
    /// </summary>
    /// <definition>
    /// public class HttpWebSocketConnectionCollection
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    public class HttpWebSocketConnectionCollection
    {
        internal List<HttpWebSocket> _ws = new List<HttpWebSocket>();

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpWebSocket"/> is registered in this collection.
        /// </summary>
        /// <definition>
        /// public event WebSocketRegistrationHandler? OnRegister;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event WebSocketRegistrationHandler? OnWebSocketRegister;

        /// <summary>
        /// Represents an event that is fired when an <see cref="HttpWebSocket"/> is closed and removed from this collection.
        /// </summary>
        /// <definition>
        /// public event EventSourceUnregistrationHandler? OnEventSourceUnregistration;
        /// </definition>
        /// <type>
        /// Event
        /// </type>
        public event WebSocketRegistrationHandler? OnWebSocketUnregister;

        internal HttpWebSocketConnectionCollection() { }

        internal void RegisterWebSocket(HttpWebSocket src)
        {
            if (src.identifier != null)
            {
                lock (_ws)
                {
                    // close another websockets with same identifier
                    HttpWebSocket[] wsId = Find(s => s == src.identifier);
                    foreach (HttpWebSocket ws in wsId)
                    {
                        ws.Close();
                    }
                    _ws.Add(src);
                }
                if (OnWebSocketRegister != null)
                    OnWebSocketRegister(this, src);
            }
        }

        internal void UnregisterWebSocket(HttpWebSocket ws)
        {
            lock (_ws)
            {
                if (_ws.Remove(ws) && OnWebSocketUnregister != null)
                {
                    OnWebSocketUnregister(this, ws);
                }
            }
        }

        /// <summary>
        /// Gets the Web Sockect connection for the specified identifier.
        /// </summary>
        /// <param name="identifier">The Web Socket identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket? GetByIdentifier(string identifier)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpWebSocket? GetByIdentifier(string identifier)
        {
            lock (_ws)
            {
                HttpWebSocket? src = _ws.Where(es => !es.isClosed && es.Identifier == identifier).FirstOrDefault();
                return src;
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpWebSocket"/> instances that matches their identifier predicate.
        /// </summary>
        /// <param name="predicate">The expression on the an non-empty Web Socket identifier.</param>
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket[] Find(Func&lt;string, bool&gt; predicate)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpWebSocket[] Find(Func<string, bool> predicate)
        {
            lock (_ws)
            {
                return _ws.Where(e => e.Identifier != null && predicate(e.Identifier)).ToArray();
            }
        }

        /// <summary>
        /// Gets all actives <see cref="HttpWebSocket"/> instances.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public HttpWebSocket[] All()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public HttpWebSocket[] All()
        {
            lock (_ws)
            {
                return _ws.ToArray();
            }
        }

        /// <summary>
        /// Closes all registered and active <see cref="HttpWebSocket"/> in this collections.
        /// </summary>
        /// <definition>
        /// public void DropAll()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public void DropAll()
        {
            lock (_ws)
            {
                foreach (HttpWebSocket es in _ws) es.Close();
            }
        }
    }

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpWebSocketConnectionCollection"/> registers an new web socket connection.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpWebSocketConnectionCollection"/> object.</param>
    /// <param name="ws">Represents the registered <see cref="HttpWebSocket"/> web socket connection.</param>
    /// <definition>
    /// public delegate void WebSocketRegistrationHandler(object sender, HttpWebSocket ws);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void WebSocketRegistrationHandler(object sender, HttpWebSocket ws);

    /// <summary>
    /// Represents an function that is called when an <see cref="HttpWebSocketConnectionCollection"/> is removed and had it's connection closed.
    /// </summary>
    /// <param name="sender">Represents the caller <see cref="HttpWebSocketConnectionCollection"/> object.</param>
    /// <param name="ws">Represents the closed <see cref="HttpWebSocket"/> web socket connection.</param>
    /// <definition>
    /// public delegate void WebSocketUnregistrationHandler(object sender, HttpWebSocket ws);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    public delegate void WebSocketUnregistrationHandler(object sender, HttpWebSocket ws);
}

/* \Internal\LoggingConstants.cs */


namespace Sisk.Core.Internal
{
    internal class LoggingFormatter
    {
        TimeSpan currentTimezoneDiff = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

        HttpServerExecutionResult? res;
        DateTime d;
        Uri? bReqUri;
        IPAddress? bReqIpAddr;
        NameValueCollection? reqHeaders;
        int bResStatusCode;
        string? bResStatusDescr;
        float? incomingSize;
        float? outcomingSize;
        long execTime;

        public LoggingFormatter(
            HttpServerExecutionResult? res,
            DateTime d, Uri? bReqUri,
            IPAddress? bReqIpAddr,
            NameValueCollection? reqHeaders,
            int bResStatusCode,
            string? bResStatusDescr,
            float? incomingSize,
            float? outcomingSize,
            long execTime)
        {
            this.res = res;
            this.d = d;
            this.bReqUri = bReqUri;
            this.bReqIpAddr = bReqIpAddr;
            this.reqHeaders = reqHeaders;
            this.bResStatusCode = bResStatusCode;
            this.bResStatusDescr = bResStatusDescr;
            this.incomingSize = incomingSize;
            this.outcomingSize = outcomingSize;
            this.execTime = execTime;
        }

        private static string? dd(LoggingFormatter lc) => $"{lc.d.Day:D2}";
        private static string? dmmm(LoggingFormatter lc) => $"{lc.d:MMMM}";
        private static string? dmm(LoggingFormatter lc) => $"{lc.d:MMM}";
        private static string? dm(LoggingFormatter lc) => $"{lc.d.Month:D2}";
        private static string? dy(LoggingFormatter lc) => $"{lc.d.Year:D4}";
        private static string? th(LoggingFormatter lc) => $"{lc.d:hh}";
        private static string? tH(LoggingFormatter lc) => $"{lc.d:HH}";
        private static string? ti(LoggingFormatter lc) => $"{lc.d.Minute:D2}";
        private static string? ts(LoggingFormatter lc) => $"{lc.d.Second:D2}";
        private static string? tm(LoggingFormatter lc) => $"{lc.d.Millisecond:D3}";
        private static string? tz(LoggingFormatter lc) => $"{lc.currentTimezoneDiff.TotalHours:00}00";
        private static string? ri(LoggingFormatter lc) => lc.bReqIpAddr?.ToString();
        private static string? rs(LoggingFormatter lc) => lc.bReqUri?.Scheme;
        private static string? ra(LoggingFormatter lc) => lc.bReqUri?.Authority;
        private static string? rh(LoggingFormatter lc) => lc.bReqUri?.Host;
        private static string? rp(LoggingFormatter lc) => lc.bReqUri?.Port.ToString();
        private static string? rz(LoggingFormatter lc) => lc.bReqUri?.AbsolutePath ?? "/";
        private static string? rq(LoggingFormatter lc) => lc.bReqUri?.Query;
        private static string? sc(LoggingFormatter lc) => lc.bResStatusCode.ToString();
        private static string? sd(LoggingFormatter lc) => lc.bResStatusDescr;
        private static string? lin(LoggingFormatter lc) => HttpServer.HumanReadableSize(lc.incomingSize);
        private static string? lou(LoggingFormatter lc) => HttpServer.HumanReadableSize(lc.outcomingSize);
        private static string? lms(LoggingFormatter lc) => lc.execTime.ToString();
        private static string? ls(LoggingFormatter lc) => lc.res?.Status.ToString();

        private static MethodInfo[] Callers = typeof(LoggingFormatter).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

        private void replaceEntities(ref string format)
        {
            foreach (var m in Callers)
            {
                string literal = "%" + m.Name;
                if (format.Contains(literal))
                {
                    string? invokeResult = (string?)m.Invoke(null, new object?[] { this });
                    format = format.Replace(literal, invokeResult);
                }
            }
        }

        private void replaceHeaders(ref string format)
        {
            int pos = 0;
            while ((pos = format.IndexOf("%{")) >= 0)
            {
                int end = format.IndexOf('}');
                string headerName = format.Substring(pos + 2, end - pos - 2);
                string? headerValue = reqHeaders?[headerName];
                format = format.Replace($"%{{{headerName}}}", headerValue);
            }
        }

        public void Format(ref string format)
        {
            replaceHeaders(ref format);
            replaceEntities(ref format);
        }
    }
}

/* \Internal\WildcardMatching.cs */


namespace Sisk.Core.Internal
{
    internal static class WildcardMatching
    {
        public record PathMatchResult(bool IsMatched, NameValueCollection Query);

        public static string StripRouteParameters(string routePath)
        {
            bool state = false;
            StringBuilder sb = new StringBuilder();
            foreach (char c in routePath)
            {
                if (c == '<' && !state)
                {
                    state = true;
                    sb.Append("arg");
                }
                else if (c == '<' && state)
                {
                    throw new InvalidOperationException("A route parameter was initialized but not terminated.");
                }
                else if (c == '>' && !state)
                {
                    throw new InvalidOperationException("A route parameter was terminated but no parameter was initialized.");
                }
                else if (c == '>' && state)
                {
                    state = false;
                }
                else if (!state)
                {
                    sb.Append(c);
                }
            }
            if (state)
            {
                throw new InvalidOperationException("A route parameter was initialized but not terminated.");
            }
            else
            {
                return sb.ToString();
            }
        }

        public static PathMatchResult IsPathMatch(string pathPattern, string requestPath, bool ignoreCase)
        {
            NameValueCollection query = new NameValueCollection();
            pathPattern = pathPattern.TrimEnd('/');
            requestPath = requestPath.TrimEnd('/');

            /*
             * normalize by rfc3986
             */
            string[] pathPatternParts = pathPattern.Split('/', StringSplitOptions.RemoveEmptyEntries);
            string[] requestPathParts = requestPath.Split('/', StringSplitOptions.RemoveEmptyEntries);

            if (pathPatternParts.Length != requestPathParts.Length)
            {
                return new PathMatchResult(false, query);
            }

            for (int i = 0; i < pathPatternParts.Length; i++)
            {
                string pathPtt = pathPatternParts[i];
                string reqsPtt = requestPathParts[i];

                if (pathPtt.StartsWith('<') && pathPtt.EndsWith('>'))
                {
                    string queryValueName = pathPtt.Substring(1, pathPtt.Length - 2);
                    query.Add(queryValueName, reqsPtt);
                }
                else
                {
                    if (string.Compare(pathPtt, reqsPtt, ignoreCase) != 0)
                    {
                        return new PathMatchResult(false, query);
                    }
                }
            }

            return new PathMatchResult(true, query);
        }

        public static bool IsDnsMatch(string wildcardPattern, string subject)
        {
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;
            wildcardPattern = wildcardPattern.Trim();
            subject = subject.Trim();

            if (string.IsNullOrWhiteSpace(wildcardPattern))
            {
                return false;
            }

            if (subject.StartsWith(wildcardPattern.Replace("*.", "")))
            {
                return true;
            }

            int wildcardCount = wildcardPattern.Count(x => x.Equals('*'));
            if (wildcardCount <= 0)
            {
                return subject.Equals(wildcardPattern, comparer);
            }
            else if (wildcardCount == 1)
            {
                string newWildcardPattern = wildcardPattern.Replace("*", "");

                if (wildcardPattern.StartsWith("*"))
                {
                    return subject.EndsWith(newWildcardPattern, comparer);
                }
                else if (wildcardPattern.EndsWith("*"))
                {
                    return subject.StartsWith(newWildcardPattern, comparer);
                }
                else
                {
                    return isWildcardMatchRgx(wildcardPattern, subject, comparer);
                }
            }
            else
            {
                return isWildcardMatchRgx(wildcardPattern, subject, comparer);
            }
        }

        private static bool isWildcardMatchRgx(string pattern, string subject, StringComparison comparer)
        {
            string[] parts = pattern.Split('*');
            if (parts.Length <= 1)
            {
                return subject.Equals(pattern, comparer);
            }

            int pos = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                if (i <= 0)
                {
                    // first
                    pos = subject.IndexOf(parts[i], pos, comparer);
                    if (pos != 0)
                    {
                        return false;
                    }
                }
                else if (i >= (parts.Length - 1))
                {
                    // last
                    if (!subject.EndsWith(parts[i], comparer))
                    {
                        return false;
                    }
                }
                else
                {
                    pos = subject.IndexOf(parts[i], pos, comparer);
                    if (pos < 0)
                    {
                        return false;
                    }

                    pos += parts[i].Length;
                }
            }

            return true;
        }
    }
}

/* \Routing\IRequestHandler.cs */


namespace Sisk.Core.Routing
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
        /// Indicates that the handler must be executed before the router calls the route callback and before the request content is available.
        /// </summary>
        BeforeContents,

        /// <summary>
        /// Indicates that the handler must be executed before the router calls the route callback and after request contents is loaded.
        /// </summary>
        BeforeResponse,

        /// <summary>
        /// Indicates that the handler must be executed after the route callback execution.
        /// </summary>
        AfterResponse
    }
}

/* \Routing\RequestCallback.cs */


namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents the function that is called after the route is matched with the request.
    /// </summary>
    /// <param name="request">The received request on the router.</param>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse RouterCallback(HttpRequest request);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public delegate HttpResponse RouterCallback(HttpRequest request);

    /// <summary>
    /// Represents the function that is called after no route is matched with the request.
    /// </summary>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse NoMatchedRouteErrorCallback();
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public delegate HttpResponse NoMatchedRouteErrorCallback();

    /// <summary>
    /// Represents the function that is called after the route callback threw an exception.
    /// </summary>
    /// <returns></returns>
    /// <definition>
    /// public delegate HttpResponse ExceptionErrorCallback(Exception ex, HttpRequest request);
    /// </definition>
    /// <type>
    /// Delegate
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing 
    /// </namespace>
    public delegate HttpResponse ExceptionErrorCallback(Exception ex, HttpRequest request);
}

/* \Routing\RequestHandledAttribute.cs */


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
        [DynamicallyAccessedMembers(
              DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )]
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
        public RequestHandlerAttribute([DynamicallyAccessedMembers(
              DynamicallyAccessedMemberTypes.PublicProperties
            | DynamicallyAccessedMemberTypes.PublicFields
            | DynamicallyAccessedMemberTypes.PublicConstructors
            | DynamicallyAccessedMemberTypes.PublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicMethods
            | DynamicallyAccessedMemberTypes.NonPublicFields
            | DynamicallyAccessedMemberTypes.NonPublicConstructors
        )] Type handledBy)
        {
            RequestHandlerType = handledBy;
            ConstructorArguments = new object?[] { };
        }
    }
}

/* \Routing\Route.cs */
namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an HTTP route to be matched by an <see cref="Router"/> object.
    /// </summary>
    /// <definition>
    /// public class Route
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public class Route
    {
        /// <summary>
        /// Gets or sets how this route can write messages to log files on the server.
        /// </summary>
        /// <definition>
        /// public LogOutput LogMode { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace> 
        /// Sisk.Core.Routing
        /// </namespace>
        public LogOutput LogMode { get; set; } = LogOutput.Both;

        /// <summary>
        /// Get or sets if this route should use regex to be interpreted instead of predefined templates.
        /// </summary>
        /// <definition>
        /// public bool UseRegex { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public bool UseRegex { get; set; } = false;

        /// <summary>
        /// Gets or sets whether this route should send Cross-Origin Resource Sharing headers in the response.
        /// </summary>
        /// <definition>
        /// public bool UseCors { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public bool UseCors { get; set; } = true;

        /// <summary>
        /// Gets or sets the matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.
        /// </summary>
        /// <definition>
        /// public RouteMethod Method { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public RouteMethod Method { get; set; }

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        /// <definition>
        /// public string Path { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public string Path { get; set; } = "";

        /// <summary>
        /// Gets or sets the route name. It allows it to be found by other routes and makes it easier to create links.
        /// </summary>
        /// <definition>
        /// public string? Name { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets the function that is called after the route is matched with the request.
        /// </summary>
        /// <definition>
        /// public RouterCallback? Callback { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public RouterCallback? Callback { get; set; }

        /// <summary>
        /// Gets or sets the RequestHandlers to run before the route's Callback.
        /// </summary>
        /// <definition>
        /// public IRequestHandler[]? RequestHandlers { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public IRequestHandler[]? RequestHandlers { get; set; }

        /// <summary>
        /// Gets or sets the global request handlers that will not run on this route. The verification is given by the identifier of the instance of an <see cref="IRequestHandler"/>.
        /// </summary>
        /// <definition>
        /// public IRequestHandler[]? BypassGlobalRequestHandlers { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public IRequestHandler[]? BypassGlobalRequestHandlers { get; set; }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="callback">The function that is called after the route is matched with the request.</param>
        /// <definition>
        /// public Route(RouteMethod method, string path, RouterCallback callback)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public Route(RouteMethod method, string path, RouterCallback callback)
        {
            Method = method;
            Path = path;
            Callback = callback;
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with given parameters.
        /// </summary>
        /// <param name="method">The matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.</param>
        /// <param name="path">The path expression that will be interpreted by the router and validated by the requests.</param>
        /// <param name="name">The route name. It allows it to be found by other routes and makes it easier to create links.</param>
        /// <param name="callback">The function that is called after the route is matched with the request.</param>
        /// <param name="beforeCallback">The RequestHandlers to run before the route's Callback.</param>
        /// <definition>
        /// public Route(RouteMethod method, string path, string? name, RouterCallback callback, IRequestHandler[]? beforeCallback)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Route(RouteMethod method, string path, string? name, RouterCallback callback, IRequestHandler[]? beforeCallback)
        {
            Method = method;
            Path = path;
            Name = name;
            Callback = callback;
            RequestHandlers = beforeCallback;
        }

        /// <summary>
        /// Creates an new <see cref="Route"/> instance with no parameters.
        /// </summary>
        /// <definition>
        /// public Route()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Route()
        {
        }

        /// <summary>
        /// Gets an string notation for this <see cref="Route"/> object.
        /// </summary>
        /// <definition>
        /// public override string ToString()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        public override string ToString()
        {
            if (string.IsNullOrEmpty(Name))
            {
                return $"[Method={Method}, Path={Path}]";
            }
            else
            {
                return $"[Method={Method}, Name={Name}]";
            }
        }
    }

    /// <summary>
    /// Determines the way the server can write log messages. This enumerator is for giving permissions for certain contexts to be able or not to write to the logs.
    /// </summary>
    /// <definition>
    /// public enum LogOutput
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    [Flags]
    public enum LogOutput
    {
        /// <summary>
        /// Determines that the context or the route can write log messages only to the access logs.
        /// </summary>
        AccessLog = 1,
        /// <summary>
        /// Determines that the context or the route can write error messages only to the error logs.
        /// </summary>
        ErrorLog = 2,
        /// <summary>
        /// Determines that the context or the route can write log messages to both error and access logs.
        /// </summary>
        Both = AccessLog | ErrorLog,
        /// <summary>
        /// Determines that the context or the route cannot write any log messages.
        /// </summary>
        None = 0
    }
}

/* \Routing\RouteAttribute.cs */
namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an class that, when applied to a method, will be recognized by a router as a route.
    /// </summary>
    /// <definition>
    /// [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    /// public class RouteAttribute : Attribute
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class RouteAttribute : Attribute
    {
        /// <summary>
        /// Gets or sets the matching HTTP method. If it is "Any", the route will just use the path expression to be matched, not the HTTP method.
        /// </summary>
        /// <definition>
        /// public RouteMethod Method { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public RouteMethod Method { get; set; } = RouteMethod.Any;

        /// <summary>
        /// Gets or sets the path expression that will be interpreted by the router and validated by the requests.
        /// </summary>
        /// <definition>
        /// public string Path { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public string Path { get; set; } = null!;

        /// <summary>
        /// Gets or sets the route name. It allows it to be found by other routes and makes it easier to create links.
        /// </summary>
        /// <definition>
        /// public string? Name { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public string? Name { get; set; }

        /// <summary>
        /// Gets or sets whether this route should send Cross-Origin Resource Sharing headers in the response.
        /// </summary>
        /// <definition>
        /// public bool UseCors { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public bool UseCors { get; set; } = true;

        /// <summary>
        /// Gets or sets how this route can write messages to log files on the server.
        /// </summary>
        /// <definition>
        /// public LogOutput LogMode { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public LogOutput LogMode { get; set; } = LogOutput.Both;

        /// <summary>
        /// Creates an new <see cref="RouteAttribute"/> instance with given route method and path pattern.
        /// </summary>
        /// <param name="method">The route entry point method.</param>
        /// <param name="path">The route path.</param>
        /// <definition>
        /// public RouteAttribute(RouteMethod method, string path)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public RouteAttribute(RouteMethod method, string path)
        {
            this.Method = method;
            this.Path = path;
        }
    }
}

/* \Routing\RouteMethod.cs */
namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents an HTTP method to be matched in an <see cref="Route"/>.
    /// </summary>
    /// <definition>
    /// [Flags]
    /// public enum RouteMethod : int
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    [Flags]
    public enum RouteMethod : int
    {
        /// <summary>
        /// Represents the HTTP GET method.
        /// </summary>
        /// <definition>
        /// Get = 2 &lt;&lt; 0
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Get = 2 << 0,

        /// <summary>
        /// Represents the HTTP POST method.
        /// </summary>
        /// <definition>
        /// Post = 2 &lt;&lt; 1
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Post = 2 << 1,

        /// <summary>
        /// Represents the HTTP PUT method.
        /// </summary>
        /// <definition>
        /// Put = 2 &lt;&lt; 2
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Put = 2 << 2,

        /// <summary>
        /// Represents the HTTP PATCH method.
        /// </summary>
        /// <definition>
        /// Patch = 2 &lt;&lt; 3
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Patch = 2 << 3,

        /// <summary>
        /// Represents the HTTP DELETE method.
        /// </summary>
        /// <definition>
        /// Delete = 2 &lt;&lt; 4
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Delete = 2 << 4,

        /// <summary>
        /// Represents the HTTP COPY method.
        /// </summary>
        /// <definition>
        /// Copy = 2 &lt;&lt; 5
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Copy = 2 << 5,

        /// <summary>
        /// Represents the HTTP HEAD method.
        /// </summary>
        /// <definition>
        /// Head = 2 &lt;&lt; 6
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Head = 2 << 6,

        /// <summary>
        /// Represents the HTTP OPTIONS method.
        /// </summary>
        /// <definition>
        /// Options = 2 &lt;&lt; 7
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Options = 2 << 7,

        /// <summary>
        /// Represents the HTTP LINK method.
        /// </summary>
        /// <definition>
        /// Link = 2 &lt;&lt; 8
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Link = 2 << 8,

        /// <summary>
        /// Represents the HTTP UNLINK method.
        /// </summary>
        /// <definition>
        /// Unlink = 2 &lt;&lt; 9
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Unlink = 2 << 9,

        /// <summary>
        /// Represents the HTTP VIEW method.
        /// </summary>
        /// <definition>
        /// View = 2 &lt;&lt; 10
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        View = 2 << 10,

        /// <summary>
        /// Represents the HTTP TRACE method.
        /// </summary>
        /// <definition>
        /// Trace = 2 &lt;&lt; 11
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Trace = 2 << 11,


        /// <summary>
        /// Represents any HTTP method.
        /// </summary>
        /// <definition>
        /// Any = 0
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        Any = 0
    }
}

/* \Routing\Router.cs */


namespace Sisk.Core.Routing
{
    /// <summary>
    /// Represents a collection of Routes and main executor of callbacks in an <see cref="HttpServer"/>.
    /// </summary>
    /// <definition>
    /// public class Router
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public class Router
    {
        internal record RouterExecutionResult(HttpResponse? Response, Route? Route, RouteMatchResult Result, Exception? Exception);
        private List<Route> _routes = new List<Route>();
        internal HttpServer? ParentServer { get; private set; }
        private bool throwException = false;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        internal void BindServer(HttpServer server)
        {
            if (object.ReferenceEquals(server, this.ParentServer)) return;
            this.ParentServer = server;
            this.throwException = server.ServerConfiguration.ThrowExceptions;
        }

        /// <summary>
        /// Gets or sets whether this <see cref="Router"/> will match routes ignoring case.
        /// </summary>
        /// <definition>
        /// public bool MatchRoutesIgnoreCase { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public bool MatchRoutesIgnoreCase { get; set; } = false;

        /// <summary>
        /// Creates an new <see cref="Router"/> instance with default properties values.
        /// </summary>
        /// <definition>
        /// public Router()
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Router() { }

        /// <summary>
        /// Gets or sets the global requests handlers that will be executed in all matched routes.
        /// </summary>
        /// <definition>
        /// public IRequestHandler[]? GlobalRequestHandlers { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public IRequestHandler[]? GlobalRequestHandlers { get; set; }

        /// <summary>
        /// Gets or sets the Router callback exception handler.
        /// </summary>
        /// <definition>
        /// public ExceptionErrorCallback? CallbackErrorHandler { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public ExceptionErrorCallback? CallbackErrorHandler { get; set; }

        /// <summary>
        /// Gets or sets the Router "404 Not Found" handler.
        /// </summary>
        /// <definition>
        /// public NoMatchedRouteErrorCallback? NotFoundErrorHandler { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public NoMatchedRouteErrorCallback? NotFoundErrorHandler { get; set; } = new NoMatchedRouteErrorCallback(
            () => new HttpResponse(System.Net.HttpStatusCode.NotFound));

        /// <summary>
        /// Gets or sets the Router "405 Method Not Allowed" handler.
        /// </summary>
        /// <definition>
        /// public NoMatchedRouteErrorCallback? MethodNotAllowedErrorHandler { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public NoMatchedRouteErrorCallback? MethodNotAllowedErrorHandler { get; set; } = new NoMatchedRouteErrorCallback(
            () => new HttpResponse(System.Net.HttpStatusCode.MethodNotAllowed));

        /// <summary>
        /// Gets all routes defined on this router instance.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public Route[] GetDefinedRoutes()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Route[] GetDefinedRoutes() => _routes.ToArray();

        #region "Route setters"
        /// <summary>
        /// Defines an route to an router.
        /// </summary>
        /// <param name="r">The router instance which the route is being set.</param>
        /// <param name="route">The route to be defined in the router.</param>
        /// <definition>
        /// public static Router operator +(Router r, Route route)
        /// </definition>
        /// <type>
        /// Operator
        /// </type>
        public static Router operator +(Router r, Route route)
        {
            r.SetRoute(route);
            return r;
        }

        /// <summary>
        /// Gets an route object by their name that is defined in this Router.
        /// </summary>
        /// <param name="name">The route name.</param>
        /// <returns></returns>
        /// <definition>
        /// public Route? GetRouteFromName(string name)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public Route? GetRouteFromName(string name)
        {
            foreach (Route r in this._routes)
            {
                if (r.Name == name)
                {
                    return r;
                }
            }
            return null;
        }

        /// <summary>
        /// Defines an route with their method, path and callback function.
        /// </summary>
        /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
        /// <param name="path">The route path.</param>
        /// <param name="callback">The route function to be called after matched.</param>
        /// <definition>
        /// public void SetRoute(RouteMethod method, string path, RouterCallback callback)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing 
        /// </namespace>
        public void SetRoute(RouteMethod method, string path, RouterCallback callback)
        {
            Route newRoute = new Route(method, path, null, callback, null);
            Route? collisonRoute;
            if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(newRoute);
        }

        /// <summary>
        /// Defines an route with their method, path, callback function and name.
        /// </summary>
        /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
        /// <param name="path">The route path.</param>
        /// <param name="callback">The route function to be called after matched.</param>
        /// <param name="name">The route name.</param>
        /// <definition>
        /// public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name)
        {
            Route newRoute = new Route(method, path, name, callback, null);
            Route? collisonRoute;
            if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(newRoute);
        }

        /// <summary>
        /// Defines an route with their method, path, callback function, name and request handlers.
        /// </summary>
        /// <param name="method">The route method to be matched. "Any" means any method that matches their path.</param>
        /// <param name="path">The route path.</param>
        /// <param name="callback">The route function to be called after matched.</param>
        /// <param name="name">The route name.</param>
        /// <param name="middlewares">Handlers that run before calling your route callback.</param>
        /// <definition>
        /// public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name, IRequestHandler[] middlewares)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetRoute(RouteMethod method, string path, RouterCallback callback, string? name, IRequestHandler[] middlewares)
        {
            Route newRoute = new Route(method, path, name, callback, middlewares);
            Route? collisonRoute;
            if ((collisonRoute = GetCollisionRoute(newRoute.Method, newRoute.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {newRoute} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(newRoute);
        }

        /// <summary>
        /// Defines an route in this Router instance.
        /// </summary>
        /// <param name="r">The route to be defined in the Router.</param>
        /// <definition>
        /// public void SetRoute(Route r)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetRoute(Route r)
        {
            Route? collisonRoute;
            if (!r.UseRegex && (collisonRoute = GetCollisionRoute(r.Method, r.Path)) != null)
            {
                throw new ArgumentException($"A possible route collision could happen between route {r} and route {collisonRoute}. Please review the methods and paths of these routes.");
            }
            _routes.Add(r);
        }

        /// <summary>
        /// Searches the object instance for methods with attribute <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates routes from them.
        /// </summary>
        /// <param name="attrClassInstance">The instance of the class where the instance methods are. The routing methods must be instance methods and marked with <see cref="RouteAttribute"/>.</param>
        /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
        /// <definition>
        /// public void SetObject(object attrClassInstance)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetObject(object attrClassInstance)
        {
            Type attrClassType = attrClassInstance.GetType();
            MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public);
            SetInternal(methods, attrClassInstance);
        }

        /// <summary>
        /// Searches the object instance for methods with attribute <see cref="RouteAttribute"/> and optionals <see cref="RequestHandlerAttribute"/>, and creates routes from them.
        /// </summary>
        /// <param name="attrClassType">The type of the class where the static methods are. The routing methods must be static and marked with <see cref="RouteAttribute"/>.</param>
        /// <exception cref="Exception">An exception is thrown when a method has an erroneous signature.</exception>
        /// <definition>
        /// public void SetObject(Type attrClassType)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public void SetObject([DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.All)] Type attrClassType)
        {
            MethodInfo[] methods = attrClassType.GetMethods(BindingFlags.Static | BindingFlags.NonPublic | BindingFlags.Public);
            SetInternal(methods, null);
        }

        private void SetInternal(MethodInfo[] methods, object? instance)
        {
            foreach (var method in methods)
            {
                RouteAttribute? atrInstance = method.GetCustomAttribute<RouteAttribute>();
                IEnumerable<RequestHandlerAttribute> handlersInstances = method.GetCustomAttributes<RequestHandlerAttribute>();

                if (atrInstance != null)
                {
                    List<IRequestHandler> methodHandlers = new List<IRequestHandler>();
                    if (handlersInstances.Count() > 0)
                    {
                        foreach (RequestHandlerAttribute atr in handlersInstances)
                        {
                            IRequestHandler rhandler = (IRequestHandler)Activator.CreateInstance(atr.RequestHandlerType, atr.ConstructorArguments)!;
                            methodHandlers.Add(rhandler);
                        }
                    }

                    try
                    {
                        RouterCallback r;

                        if (instance == null)
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), method);
                        }
                        else
                        {
                            r = (RouterCallback)Delegate.CreateDelegate(typeof(RouterCallback), instance, method);
                        }

                        Route route = new Route()
                        {
                            Method = atrInstance.Method,
                            Path = atrInstance.Path,
                            Callback = r,
                            Name = atrInstance.Name,
                            RequestHandlers = methodHandlers.ToArray(),
                            LogMode = atrInstance.LogMode,
                            UseCors = atrInstance.UseCors
                        };

                        Route? collisonRoute;
                        if ((collisonRoute = GetCollisionRoute(route.Method, route.Path)) != null)
                        {
                            throw new ArgumentException($"A possible route collision could happen between the route {route} at {method.Name} with route {collisonRoute}. Please review the methods and paths of these routes.");
                        }

                        SetRoute(route);
                    }
                    catch (Exception ex)
                    {
                        throw new Exception($"Couldn't set method {method.Name} as an route. See inner exception.", ex);
                    }
                }
            }
        }
        #endregion

        private bool IsMethodMatching(string ogRqMethod, RouteMethod method)
        {
            Enum.TryParse(typeof(RouteMethod), ogRqMethod, true, out object? ogRqParsedObj);
            if (ogRqParsedObj is null)
            {
                return false;
            }
            RouteMethod ogRqParsed = (RouteMethod)ogRqParsedObj!;
            return method.HasFlag(ogRqParsed);
        }

        private Internal.WildcardMatching.PathMatchResult TestRouteMatchUsingRegex(string routePath, string requestPath)
        {
            return new Internal.WildcardMatching.PathMatchResult
                (Regex.IsMatch(requestPath, routePath, MatchRoutesIgnoreCase ? RegexOptions.IgnoreCase : RegexOptions.None), new System.Collections.Specialized.NameValueCollection());
        }

        private Route? GetCollisionRoute(RouteMethod method, string path)
        {
            if (!path.StartsWith('/'))
            {
                throw new ArgumentException("Route paths must start with /.");
            }
            foreach (Route r in this._routes)
            {
                bool methodMatch = method != RouteMethod.Any && method == r.Method;
                bool pathMatch = WildcardMatching.IsPathMatch(r.Path, path, MatchRoutesIgnoreCase).IsMatched;

                if (methodMatch && pathMatch)
                {
                    return r;
                }
            }
            return null;
        }

        internal HttpResponse? InvokeHandler(IRequestHandler handler, HttpRequest request, HttpContext context, IRequestHandler[]? bypass)
        {
            HttpResponse? result = null;
            if (bypass != null)
            {
                bool isBypassed = false;
                foreach (IRequestHandler bypassed in bypass)
                {
                    if (object.ReferenceEquals(bypassed, handler))
                    {
                        isBypassed = true;
                        break;
                    }
                }
                if (isBypassed) return null;
            }

            try
            {
                result = handler.Execute(request, context);
            }
            catch (Exception ex)
            {
                if (!throwException)
                {
                    if (CallbackErrorHandler is not null)
                    {
                        result = CallbackErrorHandler(ex, request);
                    }
                }
                else throw;
            }

            return result;
        }

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        internal RouterExecutionResult Execute(HttpRequest request, HttpListenerRequest baseRequest)
        {
            Route? matchedRoute = null;
            RouteMatchResult matchResult = RouteMatchResult.NotMatched;
            HttpContext? context = new HttpContext(new Dictionary<string, object?>(), this.ParentServer, matchedRoute);
            HttpServerFlags flag = ParentServer!.ServerConfiguration.Flags;
            request.Context = context;
            bool hasGlobalHandlers = this.GlobalRequestHandlers?.Length > 0;

            foreach (Route route in _routes)
            {
                // test path
                Internal.WildcardMatching.PathMatchResult pathTest;
                if (route.UseRegex)
                {
                    pathTest = TestRouteMatchUsingRegex(route.Path, request.Path);
                }
                else
                {
                    pathTest = WildcardMatching.IsPathMatch(route.Path, request.Path, MatchRoutesIgnoreCase);
                }

                if (!pathTest.IsMatched)
                {
                    continue;
                }

                matchResult = RouteMatchResult.PathMatched;
                bool isMethodMatched = false;

                // test method
                if (route.Method == RouteMethod.Any)
                {
                    isMethodMatched = true;
                }
                else if (request.Method == HttpMethod.Options)
                {
                    matchResult = RouteMatchResult.OptionsMatched;
                    break;
                }
                else if (flag.TreatHeadAsGetMethod && (request.Method == HttpMethod.Head && route.Method == RouteMethod.Get))
                {
                    isMethodMatched = true;
                }
                else if (IsMethodMatching(request.Method.Method, route.Method))
                {
                    isMethodMatched = true;
                }

                if (isMethodMatched)
                {
                    foreach (string routeParam in pathTest.Query)
                    {
                        request.Query.Add(routeParam, HttpUtility.UrlDecode(pathTest.Query[routeParam]));
                    }
                    matchResult = RouteMatchResult.FullyMatched;
                    matchedRoute = route;
                    break;
                }
            }

            if (matchResult == RouteMatchResult.NotMatched && NotFoundErrorHandler is not null)
            {
                return new RouterExecutionResult(NotFoundErrorHandler(), null, matchResult, null);
            }
            else if (matchResult == RouteMatchResult.OptionsMatched)
            {
                HttpResponse corsResponse = new HttpResponse();
                corsResponse.Status = System.Net.HttpStatusCode.OK;

                return new RouterExecutionResult(corsResponse, null, matchResult, null);
            }
            else if (matchResult == RouteMatchResult.PathMatched && MethodNotAllowedErrorHandler is not null)
            {
                return new RouterExecutionResult(MethodNotAllowedErrorHandler(), matchedRoute, matchResult, null);
            }
            else if (matchResult == RouteMatchResult.FullyMatched && matchedRoute != null)
            {
                context.MatchedRoute = matchedRoute;
                HttpResponse? result = null;

                if (flag.ForceTrailingSlash && !matchedRoute.UseRegex && !request.Path.EndsWith('/'))
                {
                    HttpResponse res = new HttpResponse();
                    res.Status = HttpStatusCode.MovedPermanently;
                    res.Headers.Add("Location", request.Path + "/" + (request.QueryString ?? ""));
                    return new RouterExecutionResult(res, matchedRoute, matchResult, null);
                }

                #region Before-contents global handlers
                if (hasGlobalHandlers)
                {
                    foreach (IRequestHandler handler in this.GlobalRequestHandlers!.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeContents))
                    {
                        var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                        }
                    }
                }
                #endregion

                #region Before-contents route-specific handlers
                if (matchedRoute!.RequestHandlers?.Length > 0)
                {
                    foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeContents))
                    {
                        var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                        }
                    }
                }
                #endregion

                request.ImportContents(baseRequest.InputStream);

                #region Before-response global handlers
                if (hasGlobalHandlers)
                {
                    foreach (IRequestHandler handler in this.GlobalRequestHandlers!.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeResponse))
                    {
                        var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                        }
                    }
                }
                #endregion

                #region Before-response route-specific handlers
                if (matchedRoute!.RequestHandlers?.Length > 0)
                {
                    foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.BeforeResponse))
                    {
                        var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                        }
                    }
                }
                #endregion

                #region Route callback

                if (matchedRoute.Callback is null)
                {
                    throw new ArgumentNullException("No route callback was defined to the route " + matchedRoute.ToString());
                }

                try
                {
                    result = matchedRoute.Callback(request);
                }
                catch (Exception ex)
                {
                    if (!throwException)
                    {
                        if (CallbackErrorHandler is not null)
                        {
                            result = CallbackErrorHandler(ex, request);
                        }
                        else
                        {
                            return new RouterExecutionResult(new HttpResponse(HttpResponse.HTTPRESPONSE_ERROR), matchedRoute, matchResult, ex);
                        }
                    }
                    else throw;
                }
                finally
                {
                    context.RouterResponse = result;
                }
                #endregion

                #region After-response global handlers
                if (hasGlobalHandlers)
                {
                    foreach (IRequestHandler handler in this.GlobalRequestHandlers!.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.AfterResponse))
                    {
                        var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                        }
                    }
                }
                #endregion

                #region After-response route-specific handlers
                if (matchedRoute!.RequestHandlers?.Length > 0)
                {
                    foreach (IRequestHandler handler in matchedRoute.RequestHandlers.Where(r => r.ExecutionMode == RequestHandlerExecutionMode.AfterResponse))
                    {
                        var handlerResponse = InvokeHandler(handler, request, context, matchedRoute.BypassGlobalRequestHandlers);
                        if (handlerResponse is not null)
                        {
                            return new RouterExecutionResult(handlerResponse, matchedRoute, matchResult, null);
                        }
                    }
                }
                #endregion
            }

            return new RouterExecutionResult(context?.RouterResponse, matchedRoute, matchResult, null);
        }
    }

    internal enum RouteMatchResult
    {
        FullyMatched,
        PathMatched,
        OptionsMatched,
        HeadMatched,
        NotMatched
    }
}

