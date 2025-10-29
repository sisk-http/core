// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   BasicAuthenticateRequestHandler.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Routing;

namespace Sisk.BasicAuth;

/// <summary>
/// Gets a <see cref="IRequestHandler"/> that serves as an authenticator for the Basic Authentication scheme, which can validate a user id and password.
/// </summary>
public class BasicAuthenticateRequestHandler : IRequestHandler {

    const string DefaultRealm = "Access to the protected webpage";
    private Func<BasicAuthenticationCredentials, HttpContext, HttpResponse?> validateDefault;

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticateRequestHandler"/> class with default settings.
    /// </summary>
    public BasicAuthenticateRequestHandler () : this ( ( a, b ) => null, null ) {
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticateRequestHandler"/> class with a custom authentication function and realm message.
    /// </summary>
    /// <param name="authenticateRequest">A function that validates the <see cref="BasicAuthenticationCredentials"/> against the provided <see cref="HttpContext"/>. It should return an <see cref="HttpResponse"/> if authentication fails, otherwise <see langword="null"/>.</param>
    /// <param name="realmMessage">The realm message to be used in the Basic Authentication challenge.</param>
    public BasicAuthenticateRequestHandler ( Func<BasicAuthenticationCredentials, HttpContext, HttpResponse?> authenticateRequest, string? realmMessage = null ) {
        Realm = realmMessage ?? DefaultRealm;
        validateDefault = authenticateRequest;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="BasicAuthenticateRequestHandler"/> class with an asynchronous custom authentication function and realm message.
    /// </summary>
    /// <param name="authenticateRequestAsync">An asynchronous function that validates the <see cref="BasicAuthenticationCredentials"/> against the provided <see cref="HttpContext"/>. It should return an <see cref="HttpResponse"/> if authentication fails, otherwise <see langword="null"/>.</param>
    /// <param name="asyncCancellation">A <see cref="CancellationToken"/> to observe while waiting for the task to complete. If the token is already cancelled, this method returns immediately without waiting for the task.</param>
    /// <param name="realmMessage">The realm message to be used in the Basic Authentication challenge. If <see langword="null"/>, <see cref="DefaultRealm"/> is used.</param>
    public BasicAuthenticateRequestHandler ( Func<BasicAuthenticationCredentials, HttpContext, Task<HttpResponse?>> authenticateRequestAsync, CancellationToken asyncCancellation = default, string? realmMessage = null ) {
        Realm = realmMessage ?? DefaultRealm;
        validateDefault = delegate ( BasicAuthenticationCredentials key, HttpContext ctx ) {

            var task = authenticateRequestAsync ( key, ctx );
            task.Wait ( asyncCancellation );

            return task.Result;
        };
    }

    /// <summary>
    /// Gets or sets when this RequestHandler should run.
    /// </summary>
    public RequestHandlerExecutionMode ExecutionMode { get; init; } = RequestHandlerExecutionMode.BeforeResponse;

    /// <summary>
    /// Gets or sets a message to show the client which protection scope it needs to authenticate to.
    /// </summary>
    public string Realm { get; set; }

    /// <summary>
    /// Indicates the method that is called to validate a request with client credentials. When returning an <see cref="HttpResponse"/>,
    /// it will be sent immediately to the client and the rest of the stack will not be executed. If the return is null, it
    /// is interpretable that the authentication was successful and the execution should continue.
    /// </summary>
    /// <param name="credentials">Represents the credentials sent by the client, already decoded and ready for use.</param>
    /// <param name="context">Represents the Http context.</param>
    public virtual HttpResponse? OnValidating ( BasicAuthenticationCredentials credentials, HttpContext context ) {
        return validateDefault.Invoke ( credentials, context );
    }

    /// <summary>
    /// This method is called by the <see cref="Router"/> before executing a request when the <see cref="Route"/> instantiates an object that implements this interface. If it returns
    /// a <see cref="HttpResponse"/> object, the route callback is not called and all execution of the route is stopped. If it returns "null", the execution is continued.
    /// </summary>
    public HttpResponse? Execute ( HttpRequest request, HttpContext context ) {
        string? authorization = request.Headers.Authorization;
        if (authorization == null) {
            return CreateUnauthorizedResponse ();
        }
        else {
            try {
                var auth = ParseAuth ( authorization );
                if (auth == null) {
                    return DefaultMessagePage.Instance.CreateMessageHtml ( HttpStatusInformation.BadRequest, "Invalid Authorization Header" );
                }
                var res = OnValidating ( auth, context );
                return res;
            }
            catch (Exception) {
                return CreateUnauthorizedResponse ();
            }
        }
    }

    /// <summary>
    /// Creates an empty HTTP response with the WWW-Authenticate header and an custom realm message.
    /// </summary>
    /// <param name="realm">Defines the realm message to send back to the client.</param>
    public HttpResponse CreateUnauthorizedResponse ( string realm ) {
        var unauth = new HttpResponse ( System.Net.HttpStatusCode.Unauthorized );
        unauth.Headers.Add ( "WWW-Authenticate", $"Basic realm=\"{realm}\"" );
        return unauth;
    }

    /// <summary>
    /// Creates an empty HTTP response with the WWW-Authenticate header and with the realm message defined in this class instance.
    /// </summary>
    public HttpResponse CreateUnauthorizedResponse () {
        return new HttpResponse () {
            Status = HttpStatusInformation.Unauthorized,
            Headers = new () {
                WWWAuthenticate = $"Basic realm=\"{Realm}\""
            }
        };
    }

    private BasicAuthenticationCredentials? ParseAuth ( string s ) {

        var schemeSeparator = s.IndexOf ( ' ' );
        if (schemeSeparator == -1)
            return null;

        var scheme = s [ 0..schemeSeparator ];
        if (scheme.Equals ( "Basic", StringComparison.OrdinalIgnoreCase ) == false)
            return null;

        var credentialsParts = s [ (schemeSeparator + 1).. ].Split ( ':' );
        if (credentialsParts.Length != 2)
            return null;

        return new BasicAuthenticationCredentials ( credentialsParts [ 0 ], credentialsParts [ 1 ] );
    }
}