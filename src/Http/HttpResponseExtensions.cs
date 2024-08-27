// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpResponseExtensions.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Net;
using System.Text;

namespace Sisk.Core.Http;

/// <summary>
/// Provides useful extensions for <see cref="HttpResponse"/> objects.
/// </summary>
public static class HttpResponseExtensions
{
    /// <summary>
    /// Sets an UTF-8 string as the HTTP response content in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="content">The UTF-8 string containing the response body.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithContent<THttpResponse>(this THttpResponse response, string content) where THttpResponse : HttpResponse
    {
        response.Content = new StringContent(content);
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
    public static THttpResponse WithContent<THttpResponse>(this THttpResponse response, string content, Encoding? encoding, string mimeType) where THttpResponse : HttpResponse
    {
        response.Content = new StringContent(content, encoding, mimeType);
        return response;
    }

    /// <summary>
    /// Sets an <see cref="HttpContent"/> as the HTTP content body in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="content">The HTTP content object.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithContent<THttpResponse>(this THttpResponse response, HttpContent content) where THttpResponse : HttpResponse
    {
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
    public static THttpResponse WithHeader<THttpResponse>(this THttpResponse response, string headerName, string headerValue) where THttpResponse : HttpResponse
    {
        response.Headers.Add(headerName, headerValue);
        return response;
    }

    /// <summary>
    /// Sets an list of HTTP headers in this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="headers">The collection of HTTP headers.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithHeader<THttpResponse>(this THttpResponse response, NameValueCollection headers) where THttpResponse : HttpResponse
    {
        foreach (string key in headers.Keys)
            response.Headers.Add(key, headers[key]);
        return response;
    }

    /// <summary>
    /// Sets the HTTP status code of this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithStatus<THttpResponse>(this THttpResponse response, int httpStatusCode) where THttpResponse : HttpResponse
    {
        response.StatusInformation = httpStatusCode;
        return response;
    }

    /// <summary>
    /// Sets the HTTP status code of this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="httpStatusCode">The HTTP status code.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithStatus<THttpResponse>(this THttpResponse response, HttpStatusCode httpStatusCode) where THttpResponse : HttpResponse
    {
        response.StatusInformation = httpStatusCode;
        return response;
    }

    /// <summary>
    /// Sets the HTTP status code of this <see cref="HttpResponse"/>.
    /// </summary>
    /// <typeparam name="THttpResponse">The type which implements <see cref="HttpResponse"/>.</typeparam>
    /// <param name="response">The <see cref="HttpResponse"/> object.</param>
    /// <param name="statusInformation">The HTTP status information.</param>
    /// <returns>The self <typeparamref name="THttpResponse"/> object.</returns>
    public static THttpResponse WithStatus<THttpResponse>(this THttpResponse response, in HttpStatusInformation statusInformation) where THttpResponse : HttpResponse
    {
        response.StatusInformation = statusInformation;
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
    public static THttpResponse WithCookie<THttpResponse>(this THttpResponse response,
        string name,
        string value,
        DateTime? expires = null,
        TimeSpan? maxAge = null,
        string? domain = null,
        string? path = null,
        bool? secure = null,
        bool? httpOnly = null,
        string? sameSite = null)
        where THttpResponse : HttpResponse
    {
        response.SetCookie(name, value, expires, maxAge, domain, path, secure, httpOnly, sameSite);
        return response;
    }
}
