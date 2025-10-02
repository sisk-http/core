// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseExtensions.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Net;
using System.Text;
using Sisk.Core.Entity;
using Sisk.Core.Helpers;

namespace Sisk.Core.Http;

/// <summary>
/// Provides useful extensions for <see cref="HttpResponse"/> objects.
/// </summary>
public static class HttpResponseExtensions {
#if NET10_0_OR_GREATER
    extension<THttpResponse> ( THttpResponse response ) where THttpResponse : HttpResponse {

        /// <summary>
        /// Sets the content of the HTTP response to a string.
        /// </summary>
        /// <param name="content">The string content to set.</param>
        /// <returns>The modified HTTP response.</returns>
        public THttpResponse WithContent ( string content ) {
            response.Content = new StringContent ( content );
            return response;
        }

        /// <summary>
        /// Sets the content of the HTTP response to a string with a specified encoding and MIME type.
        /// </summary>
        /// <param name="content">The string content to set.</param>
        /// <param name="encoding">The encoding to use for the content. Can be <see langword="null"/>.</param>
        /// <param name="mimeType">The MIME type of the content.</param>
        /// <returns>The modified HTTP response.</returns>
        public THttpResponse WithContent ( string content, Encoding? encoding, string mimeType ) {
            response.Content = new StringContent ( content, encoding, mimeType );
            return response;
        }

        /// <summary>
        /// Sets the content of the HTTP response to a specified <see cref="HttpContent"/>.
        /// </summary>
        /// <param name="content">The <see cref="HttpContent"/> to set.</param>
        /// <returns>The modified HTTP response.</returns>
        public THttpResponse WithContent ( HttpContent content ) {
            response.Content = content;
            return response;
        }

        /// <summary>
        /// Adds a header to the HTTP response.
        /// </summary>
        /// <param name="headerName">The name of the header.</param>
        /// <param name="headerValue">The value of the header.</param>
        /// <returns>The modified HTTP response.</returns>
        public THttpResponse WithHeader ( string headerName, string headerValue ) {
            response.Headers.Add ( headerName, headerValue );
            return response;
        }

        /// <summary>
        /// Adds multiple headers to the HTTP response from a dictionary.
        /// </summary>
        /// <param name="headers">A dictionary of header names and their values.</param>
        /// <returns>The modified HTTP response.</returns>
        public THttpResponse WithHeaders ( IDictionary<string, string?> headers ) {
            foreach (string key in headers.Keys) {
                response.Headers.Add ( key, headers [ key ] ?? string.Empty );
            }

            return response;
        }

        /// <summary>
        /// Adds multiple headers to the HTTP response from a <see cref="StringKeyStoreCollection"/>.
        /// </summary>
        /// <param name="headers">A <see cref="StringKeyStoreCollection"/> of header names and their values.</param>
        /// <returns>The modified HTTP response.</returns>
        public THttpResponse WithHeaders ( StringKeyStoreCollection headers ) {
            foreach (string key in headers.Keys) {
                response.Headers.Add ( key, headers [ key ] ?? string.Empty );
            }

            return response;
        }

        /// <summary>
        /// Sets the status code of the HTTP response using an integer.
        /// </summary>
        /// <param name="httpStatusCode">The integer HTTP status code.</param>
        /// <returns>The modified HTTP response.</returns>
        public HttpResponse WithStatus ( int httpStatusCode ) {
            response.Status = httpStatusCode;
            return response;
        }

        /// <summary>
        /// Sets the status code of the HTTP response using an <see cref="HttpStatusCode"/> enum.
        /// </summary>
        /// <param name="statusCode">The <see cref="HttpStatusCode"/> to set.</param>
        /// <returns>The modified HTTP response.</returns>
        public HttpResponse WithStatus ( HttpStatusCode statusCode ) {
            response.Status = statusCode;
            return response;
        }

        /// <summary>
        /// Sets the status of the HTTP response using <see cref="HttpStatusInformation"/>.
        /// </summary>
        /// <param name="statusInformation">The <see cref="HttpStatusInformation"/> to set.</param>
        /// <returns>The modified HTTP response.</returns>
        public HttpResponse WithStatus ( in HttpStatusInformation statusInformation ) {
            response.Status = statusInformation;
            return response;
        }

        /// <summary>
        /// Adds a cookie to the HTTP response with specified parameters.
        /// </summary>
        /// <param name="name">The name of the cookie.</param>
        /// <param name="value">The value of the cookie.</param>
        /// <param name="expires">The expiration date and time of the cookie. Can be <see langword="null"/>.</param>
        /// <param name="maxAge">The maximum age of the cookie in seconds. Can be <see langword="null"/>.</param>
        /// <param name="domain">The domain for which the cookie is valid. Can be <see langword="null"/>.</param>
        /// <param name="path">The path for which the cookie is valid. Can be <see langword="null"/>.</param>
        /// <param name="secure">A value indicating whether the cookie should only be sent over HTTPS. Can be <see langword="null"/>.</param>
        /// <param name="httpOnly">A value indicating whether the cookie should be inaccessible to client-side scripts. Can be <see langword="null"/>.</param>
        /// <param name="sameSite">The SameSite attribute for the cookie. Can be <see langword="null"/>.</param>
        /// <returns>The modified HTTP response.</returns>
        public HttpResponse WithCookie (
            string name,
            string value,
            DateTime? expires = null,
            TimeSpan? maxAge = null,
            string? domain = null,
            string? path = null,
            bool? secure = null,
            bool? httpOnly = null,
            string? sameSite = null ) {

            response.Headers.Add ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( name, value, expires, maxAge, domain, path, secure, httpOnly, sameSite ) );
            return response;
        }

        /// <summary>
        /// Adds a cookie to the HTTP response using a <see cref="Cookie"/> object.
        /// </summary>
        /// <param name="cookie">The <see cref="Cookie"/> object to add.</param>
        /// <returns>The modified HTTP response.</returns>
        public HttpResponse WithCookie ( Cookie cookie ) {

            response.Headers.Add ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( cookie ) );
            return response;
        }
    }
#else
    /// <summary>
    /// Sets an UTF-8 string as the HTTP response content in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="content">The UTF-8 string containing the response body.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithContent<THttpResponse> ( this THttpResponse response, string content ) where THttpResponse : HttpResponse {
        response.Content = new StringContent ( content );
        return response;
    }

    /// <summary>
    /// Sets an string as the HTTP response content in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="content">The string containing the response body.</param>
    /// <param name="encoding">The encoding to encode the string message.</param>
    /// <param name="mimeType">The mime-type of the response content.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithContent<THttpResponse> ( this THttpResponse response, string content, Encoding? encoding, string mimeType ) where THttpResponse : HttpResponse {
        response.Content = new StringContent ( content, encoding, mimeType );
        return response;
    }

    /// <summary>
    /// Sets an <see cref="HttpContent"/> as the HTTP content body in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="content">The HTTP content object.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithContent<THttpResponse> ( this THttpResponse response, HttpContent content ) where THttpResponse : HttpResponse {
        response.Content = content;
        return response;
    }

    /// <summary>
    /// Sets an HTTP header in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="headerName">The name of the header.</param>
    /// <param name="headerValue">The header value.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithHeader<THttpResponse> ( this THttpResponse response, string headerName, string headerValue ) where THttpResponse : HttpResponse {
        response.Headers.Add ( headerName, headerValue );
        return response;
    }

    /// <summary>
    /// Sets an list of HTTP headers in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="headers">The collection of HTTP headers.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithHeader<THttpResponse> ( this THttpResponse response, NameValueCollection headers ) where THttpResponse : HttpResponse {
        foreach (string key in headers.Keys)
            response.Headers.Add ( key, headers [ key ] ?? string.Empty );
        return response;
    }

    /// <summary>
    /// Sets an list of HTTP headers in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="headers">The collection of HTTP headers.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithHeader<THttpResponse> ( this THttpResponse response, StringKeyStoreCollection headers ) where THttpResponse : HttpResponse {
        headers.AddRange ( headers );
        return response;
    }

    /// <summary>
    /// Sets the HTTP status code of this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithStatus<THttpResponse> ( this THttpResponse response, int httpStatusCode ) where THttpResponse : HttpResponse {
        response.Status = httpStatusCode;
        return response;
    }

    /// <summary>
    /// Sets the HTTP status code of this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithStatus<THttpResponse> ( this THttpResponse response, HttpStatusCode httpStatusCode ) where THttpResponse : HttpResponse {
        response.Status = httpStatusCode;
        return response;
    }

    /// <summary>
    /// Sets the HTTP status code of this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="statusInformation">The HTTP status information.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithStatus<THttpResponse> ( this THttpResponse response, in HttpStatusInformation statusInformation ) where THttpResponse : HttpResponse {
        response.Status = statusInformation;
        return response;
    }

    /// <summary>
    /// Sets a cookie and sends it in the response to be set by the client.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="name">The cookie name.</param>
    /// <param name="value">The cookie value.</param>
    /// <param name="expires">The cookie expirity date.</param>
    /// <param name="maxAge">The cookie max duration after being set.</param>
    /// <param name="domain">The domain where the cookie will be valid.</param>
    /// <param name="path">The path where the cookie will be valid.</param>
    /// <param name="secure">Determines if the cookie will only be stored in an secure context.</param>
    /// <param name="httpOnly">Determines if the cookie will be only available in the HTTP context.</param>
    /// <param name="sameSite">The cookie SameSite parameter.</param>
    public static THttpResponse WithCookie<THttpResponse> ( this THttpResponse response,
        string name,
        string value,
        DateTime? expires = null,
        TimeSpan? maxAge = null,
        string? domain = null,
        string? path = null,
        bool? secure = null,
        bool? httpOnly = null,
        string? sameSite = null )
        where THttpResponse : HttpResponse {
        response.Headers.Add ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( name, value, expires, maxAge, domain, path, secure, httpOnly, sameSite ) );
        return response;
    }

    /// <summary>
    /// Sets a cookie and sends it in the response to be set by the client.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="cookie">The cookie object.</param>
    public static THttpResponse WithCookie<THttpResponse> ( this THttpResponse response, Cookie cookie ) where THttpResponse : HttpResponse {
        response.Headers.Add ( HttpKnownHeaderNames.SetCookie, CookieHelper.BuildCookieHeaderValue ( cookie ) );
        return response;
    }
#endif
}
