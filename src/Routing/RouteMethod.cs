// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
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
    [Flags]
    public enum RouteMethod : int
    {
        /// <summary>
        /// Represents the HTTP GET method.
        /// </summary>
        Get = 2 << 0,

        /// <summary>
        /// Represents the HTTP POST method.
        /// </summary>
        Post = 2 << 1,

        /// <summary>
        /// Represents the HTTP PUT method.
        /// </summary>
        Put = 2 << 2,

        /// <summary>
        /// Represents the HTTP PATCH method.
        /// </summary>
        Patch = 2 << 3,

        /// <summary>
        /// Represents the HTTP DELETE method.
        /// </summary>
        Delete = 2 << 4,

        /// <summary>
        /// Represents the HTTP HEAD method.
        /// </summary>
        Head = 2 << 6,

        /// <summary>
        /// Represents the HTTP OPTIONS method.
        /// </summary>
        Options = 2 << 7,

        /// <summary>
        /// Represents any HTTP method.
        /// </summary>
        Any = Get | Post | Put | Patch | Delete | Head | Options
    }
}
