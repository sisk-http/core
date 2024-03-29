﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpStatusCode.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Net;
using System.Runtime.CompilerServices;
using System.Text;

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
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <definition>
        /// public HttpStatusInformation(int statusCode)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpStatusInformation(int statusCode)
        {
            StatusCode = statusCode;
            Description = GetStatusCodeDescription(statusCode);
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        /// <definition>
        /// public HttpStatusInformation(HttpStatusCode statusCode)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpStatusInformation(HttpStatusCode statusCode)
        {
            StatusCode = (int)statusCode;
            Description = GetStatusCodeDescription(StatusCode);
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <param name="description">Sets the short description of the HTTP message.</param>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        /// <exception cref="ArgumentNullException"></exception>
        /// <definition>
        /// public HttpStatusInformation(int statusCode, string description)
        /// </definition>
        /// <type>
        /// Constructor
        /// </type>
        public HttpStatusInformation(int statusCode, string description)
        {
            Description = description ?? throw new ArgumentNullException(nameof(description));
            StatusCode = statusCode;
            ValidateStatusCode(statusCode);
            ValidateDescription(description);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateStatusCode(int st)
        {
            if (st < 100 || st > 999) throw new ProtocolViolationException(SR.HttpStatusCode_IllegalStatusCode);
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateDescription(string s)
        {
            if (s.Length > 8192) throw new ProtocolViolationException(SR.HttpStatusCode_IllegalStatusReason);
        }

        /// <summary>
        /// Gets the description of the HTTP status based on its description.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        /// <definition>
        /// public static string GetStatusCodeDescription(int statusCode)
        /// </definition>
        /// <type>
        /// Static method
        /// </type>
        public static string GetStatusCodeDescription(int statusCode)
        {
            ValidateStatusCode(statusCode);
            return HttpStatusDescription.Get(statusCode);
        }
    }
}
