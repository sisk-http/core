// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpStatusCode.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using System.Runtime.CompilerServices;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents a structure that holds an HTTP response status information, with its code and description.
    /// </summary>
    /// <definition>
    /// public struct HttpStatusInformation
    /// </definition>
    /// <type>
    /// Struct
    /// </type>
    public struct HttpStatusInformation
    {
        private int __statusCode = 100;
        private string __description = "Continue";

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
            Description = description ?? throw new ArgumentNullException(nameof(description));
            StatusCode = statusCode;
            ValidateStatusCode(statusCode);
            ValidateDescription(description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateStatusCode(int st)
        {
            if (st < 100 || st > 999) throw new ProtocolViolationException("The HTTP status code must be three-digits long.");
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private void ValidateDescription(string s)
        {
            if (s.Length > 8192) throw new ProtocolViolationException("The HTTP reason phrase must be equal or smaller than 8192 characters.");
        }
    }
}
