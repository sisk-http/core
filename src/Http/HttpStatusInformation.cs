// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpStatusInformation.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System.Diagnostics.CodeAnalysis;
using System.Net;
using System.Runtime.CompilerServices;

namespace Sisk.Core.Http
{
    /// <summary>
    /// Represents a structure that holds an HTTP response status information, with it's status code and description.
    /// </summary>
    public struct HttpStatusInformation : IComparable, IEquatable<HttpStatusInformation>
    {
        private int __statusCode;
        private string __description;

        /// <summary>
        /// Gets or sets the short description of the HTTP message.
        /// </summary>
        /// <remarks>
        /// Custom status descriptions is only supported for plain HTTP/1.1 and 1.0 transfers.
        /// </remarks>
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
        /// Creates an new <see cref="HttpStatusInformation"/> with default parameters (200 OK) status.
        /// </summary>
        public HttpStatusInformation()
        {
            __statusCode = 200;
            __description = "OK";
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        public HttpStatusInformation(int statusCode)
        {
            ValidateStatusCode(statusCode);
            __statusCode = statusCode;
            __description = GetStatusCodeDescription(statusCode);
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        public HttpStatusInformation(HttpStatusCode statusCode)
        {
            int s = (int)statusCode;
            ValidateStatusCode(s);
            __statusCode = s;
            __description = GetStatusCodeDescription(statusCode);
        }

        /// <summary>
        /// Creates an new <see cref="HttpStatusInformation"/> instance with given parameters.
        /// </summary>
        /// <remarks>
        /// Custom status descriptions is only supported for plain HTTP/1.1 and 1.0 transfers.
        /// </remarks>
        /// <param name="description">Sets the short description of the HTTP message.</param>
        /// <param name="statusCode">Sets the numeric HTTP status code of the HTTP message.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public HttpStatusInformation(int statusCode, string description)
        {
            ValidateStatusCode(statusCode);
            ValidateDescription(description);
            __statusCode = statusCode;
            __description = description;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static void ValidateStatusCode(int st)
        {
            if (st < 100 || st > 999)
                throw new ProtocolViolationException(SR.HttpStatusCode_IllegalStatusCode);
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
        public static string GetStatusCodeDescription(int statusCode)
        {
            ValidateStatusCode(statusCode);
            return HttpStatusDescription.Get(statusCode);
        }

        /// <summary>
        /// Gets the description of the HTTP status based on its description.
        /// </summary>
        /// <param name="statusCode">The HTTP status code.</param>
        public static string GetStatusCodeDescription(HttpStatusCode statusCode)
        {
            return GetStatusCodeDescription((int)statusCode);
        }

        /// <summary>
        /// Gets an <see cref="HttpStatusCode"/> corresponding to this instance, or null if the HTTP status does not match any value.
        /// </summary>
        /// <returns>
        /// An <see cref="HttpStatusCode"/> or null if the HTTP status matches no entry on it.
        /// </returns>
        public HttpStatusCode? GetHttpStatusCode()
        {
            HttpStatusCode s = (HttpStatusCode)__statusCode;
            if (Enum.IsDefined(s))
            {
                return s;
            }
            return null;
        }

        /// <inheritdoc/>
        /// <exclude/>
        public readonly bool Equals(HttpStatusInformation other)
        {
            return other.__statusCode.Equals(this.__statusCode) && other.__description.Equals(this.__description);
        }

        /// <inheritdoc/>
        /// <exclude/>
        public int CompareTo(object? obj)
        {
            if (obj is HttpStatusInformation other)
            {
                if (other.__statusCode == this.__statusCode) return 0;
                if (other.__statusCode > this.__statusCode) return 1;
                if (other.__statusCode < this.__statusCode) return -1;
            }
            return -1;
        }

        /// <inheritdoc/>
        /// <exclude/>
        public override bool Equals([NotNullWhen(true)] object? obj)
        {
            if (obj is HttpStatusInformation other)
            {
                return Equals(other);
            }
            return false;
        }

        /// <inheritdoc/>
        /// <exclude/>
        public override int GetHashCode()
        {
            return this.__statusCode.GetHashCode() ^ this.__description.GetHashCode();
        }

        /// <summary>
        /// Gets an string representation of this HTTP Status Code.
        /// </summary>
        public override string ToString()
        {
            return $"{__statusCode} {__description}";
        }
    }
}
