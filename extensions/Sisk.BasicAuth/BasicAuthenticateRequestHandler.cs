// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   BasicAuthenticateRequestHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Routing;
using System.Text;

namespace Sisk.BasicAuth;

/// <summary>
/// Gets a <see cref="IRequestHandler"/> that serves as an authenticator for the Basic Authentication scheme, which can validate a user id and password.
/// </summary>
/// <definition>
/// public class BasicAuthenticateRequestHandler : IRequestHandler
/// </definition>
/// <type>
/// Class
/// </type>
public class BasicAuthenticateRequestHandler : IRequestHandler
{
    /// <summary>
    /// Gets or sets when this RequestHandler should run.
    /// </summary>
    /// <definition>
    /// public RequestHandlerExecutionMode ExecutionMode { get; init; }
    /// </definition> 
    /// <type>
    /// Property
    /// </type>
    public RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeResponse;

    /// <summary>
    /// Gets or sets a message to show the client which protection scope it needs to authenticate to.
    /// </summary>
    /// <definition>
    /// public string Realm { get; set; }
    /// </definition> 
    /// <type>
    /// Property
    /// </type>
    public string Realm { get; set; } = "Access to the protected webpage";

    /// <summary>
    /// Indicates the method that is called to validate a request with client credentials. When returning an <see cref="HttpResponse"/>,
    /// it will be sent immediately to the client and the rest of the stack will not be executed. If the return is null, it
    /// is interpretable that the authentication was successful and the execution should continue.
    /// </summary>
    /// <param name="credentials">Represents the credentials sent by the client, already decoded and ready for use.</param>
    /// <param name="context">Represents the Http context.</param>
    /// <definition>
    /// public virtual HttpResponse? OnValidating(BasicAuthenticationCredentials credentials, HttpContext context)
    /// </definition> 
    /// <type>
    /// Method
    /// </type>
    public virtual HttpResponse? OnValidating(BasicAuthenticationCredentials credentials, HttpContext context)
    {
        return null;
    }

    /// <summary>
    /// This method is called by the <see cref="Router"/> before executing a request when the <see cref="Route"/> instantiates an object that implements this interface. If it returns
    /// a <see cref="HttpResponse"/> object, the route callback is not called and all execution of the route is stopped. If it returns "null", the execution is continued.
    /// </summary>
    /// <definition>
    /// public HttpResponse? Execute(HttpRequest request, HttpContext context)
    /// </definition> 
    /// <type>
    /// Method
    /// </type>
    public HttpResponse? Execute(HttpRequest request, HttpContext context)
    {
        string? authorization = request.Headers["Authorization"];
        if (authorization == null)
        {
            return CreateUnauthorizedResponse();
        }
        else
        {
            try
            {
                var auth = ParseAuth(authorization);
                if (auth == null)
                {
                    throw new Exception();
                }
                var res = OnValidating(auth, context);
                return res;
            }
            catch (Exception)
            {
                return CreateUnauthorizedResponse();
            }
        }
    }

    /// <summary>
    /// Creates an empty HTTP response with the WWW-Authenticate header and an custom realm message.
    /// </summary>
    /// <param name="realm">Defines the realm message to send back to the client.</param>
    /// <definition>
    /// public HttpResponse CreateUnauthorizedResponse(string realm)
    /// </definition> 
    /// <type>
    /// Method
    /// </type>
    public HttpResponse CreateUnauthorizedResponse(string realm)
    {
        var unauth = new HttpResponse(System.Net.HttpStatusCode.Unauthorized);
        unauth.Headers.Add("WWW-Authenticate", $"Basic realm=\"{realm}\"");
        return unauth;
    }

    /// <summary>
    /// Creates an empty HTTP response with the WWW-Authenticate header and with the realm message defined in this class instance.
    /// </summary>
    /// <definition>
    /// public HttpResponse CreateUnauthorizedResponse()
    /// </definition> 
    /// <type>
    /// Method
    /// </type>
    public HttpResponse CreateUnauthorizedResponse()
    {
        var unauth = new HttpResponse(System.Net.HttpStatusCode.Unauthorized);
        unauth.Headers.Add("WWW-Authenticate", $"Basic realm=\"{Realm}\"");
        return unauth;
    }

    private BasicAuthenticationCredentials? ParseAuth(string s)
    {
        try
        {
            s = s.Substring(s.IndexOf(' ') + 1);
            byte[] authBytes = Convert.FromBase64String(s);
            string authString = Encoding.UTF8.GetString(authBytes);

            int colonIndex = authString.IndexOf(':');
            if (colonIndex == -1) return null;

            string userid = authString.Substring(0, colonIndex);
            string pass = authString.Substring(colonIndex + 1);

            return new BasicAuthenticationCredentials(userid, pass);
        }
        catch (Exception)
        {
            return null;
        }
    }
}