// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteMethod.cs
// Repository:  https://github.com/sisk-http/core

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
        [Obsolete("This HTTP method is not defined in RFC 7231, Session 4, making it specific to a specific technology. To use custom HTTP methods, consider using the HttpResponse.CustomStatus or RouterMethod.Any property for routes." +
            " This enum value will be removed in a future Sisk update.")]
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
        [Obsolete("This HTTP method is not defined in RFC 7231, Session 4, making it specific to a specific technology. To use custom HTTP methods, consider using the HttpResponse.CustomStatus or RouterMethod.Any property for routes." +
            " This enum value will be removed in a future Sisk update.")]
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
        [Obsolete("This HTTP method is not defined in RFC 7231, Session 4, making it specific to a specific technology. To use custom HTTP methods, consider using the HttpResponse.CustomStatus or RouterMethod.Any property for routes." +
            " This enum value will be removed in a future Sisk update.")]
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
        [Obsolete("This HTTP method is not defined in RFC 7231, Session 4, making it specific to a specific technology. To use custom HTTP methods, consider using the HttpResponse.CustomStatus or RouterMethod.Any property for routes." +
            " This enum value will be removed in a future Sisk update.")]
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
        Trace = 2 << 11,

        /// <summary>
        /// Represents any HTTP method.
        /// </summary>
        /// <definition>
        /// Any = Get | Post | Put | Patch | Delete | Copy | Head | Options | Link | Unlink | View | Trace
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
#pragma warning disable CS0618
        Any = Get | Post | Put | Patch | Delete | Copy | Head | Options | Link | Unlink | View | Trace
#pragma warning restore CS0618
    }
}
