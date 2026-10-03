// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouteMatchingTests.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;
using Sisk.Core.Routing;

namespace tests.Tests;

[TestClass]
public sealed class RouteMatchingTests {

    [TestMethod]
    public async Task OptionsPreflightOnGetRouteReturnsOk () {
        using var client = Server.GetHttpClient ();
        var request = new HttpRequestMessage ( HttpMethod.Options, "tests/plaintext" );
        var response = await client.SendAsync ( request );

        Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );
    }

    [TestMethod]
    public async Task OptionsOnAnyRouteExecutesAction () {
        using var client = Server.GetHttpClient ();
        var request = new HttpRequestMessage ( HttpMethod.Options, "tests/routing/any" );
        var response = await client.SendAsync ( request );

        Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );
        Assert.IsTrue ( response.Headers.TryGetValues ( "X-Route-Action", out var values ) );
        Assert.AreEqual ( "any", values.Single () );
    }

    [TestMethod]
    public async Task RegexRouteMatchesMethodAndCapturesGroups () {
        using var client = Server.GetHttpClient ();

        var getResponse = await client.GetAsync ( "tests/routing/regex/42" );
        Assert.AreEqual ( HttpStatusCode.OK, getResponse.StatusCode );
        Assert.AreEqual ( "42", await getResponse.Content.ReadAsStringAsync () );

        var postResponse = await client.PostAsync ( "tests/routing/regex/42", null );
        Assert.AreEqual ( HttpStatusCode.MethodNotAllowed, postResponse.StatusCode );
    }

    [TestMethod]
    public void RouterLookupKeepsMethodAndCaseSemantics () {
        var router = new Router () { MatchRoutesIgnoreCase = true };
        var postRoute = new Route ( RouteMethod.Post, "/users/<id>", ( HttpRequest req ) => new HttpResponse () );
        var regexRoute = new RegexRoute ( RouteMethod.Get, @"^/files/(?<name>[a-z]+)$", req => new HttpResponse () );

        router.Map ( postRoute );
        router.Map ( regexRoute );

        Assert.AreSame ( postRoute, router.GetRouteFromPath ( RouteMethod.Any, "/USERS/<id>" ) );
        Assert.IsTrue ( router.IsDefined ( RouteMethod.Post, "/users/123" ) );

        RouteMatch match = regexRoute.Match ( "/FILES/ABC", router );
        Assert.IsTrue ( match.Success );
        Assert.AreEqual ( "ABC", match.Parameters? [ "name" ] );
    }

    [TestMethod]
    public void RegexRouteCacheFollowsRouterCaseOption () {
        var regexRoute = new RegexRoute ( RouteMethod.Get, @"^/files$", req => new HttpResponse () );

        Assert.IsFalse ( regexRoute.Match ( "/FILES", new Router () ).Success );
        Assert.IsTrue ( regexRoute.Match ( "/FILES", new Router () { MatchRoutesIgnoreCase = true } ).Success );

        regexRoute.Path = @"^/other$";
        Assert.IsTrue ( regexRoute.Match ( "/other", new Router () ).Success );
    }

    [TestMethod]
    public void PrefixRouteMatchesWholeSegments () {
        var router = new Router ();
        var route = new PrefixRoute ( RouteMethod.Get, "/static/", null, req => new HttpResponse (), null );

        Assert.IsTrue ( route.Match ( "/static", router ).Success );
        Assert.IsTrue ( route.Match ( "/static/", router ).Success );
        Assert.IsTrue ( route.Match ( "/static/css/site.css", router ).Success );
        Assert.IsFalse ( route.Match ( "/staticfoo", router ).Success );
        Assert.IsFalse ( route.Match ( "/STATIC/a", router ).Success );
        Assert.IsTrue ( route.Match ( "/STATIC/a", new Router () { MatchRoutesIgnoreCase = true } ).Success );
    }

    [TestMethod]
    public void RootPrefixRouteMatchesAnyPath () {
        var router = new Router ();
        var route = new PrefixRoute ( RouteMethod.Get, "/", null, req => new HttpResponse (), null );

        Assert.IsTrue ( route.Match ( "/", router ).Success );
        Assert.IsTrue ( route.Match ( "/a/b", router ).Success );
    }

    [TestMethod]
    public void OnlyStaticRoutesAreCheckedForCollisions () {
        AssertStart ( r => {
            r.MapGet ( "/users/<id>", ( HttpRequest req ) => new HttpResponse () );
            r.MapGet ( "/users/<name>", ( HttpRequest req ) => new HttpResponse () );
        }, shouldThrow: true );

        AssertStart ( r => {
            r.Map ( new PrefixRoute ( RouteMethod.Get, "/", null, req => new HttpResponse (), null ) );
            r.MapGet ( "/", ( HttpRequest req ) => new HttpResponse () );
            r.Map ( new PrefixRoute ( RouteMethod.Get, "/static", null, req => new HttpResponse (), null ) );
            r.Map ( new RegexRoute ( RouteMethod.Get, "^/.*$", req => new HttpResponse () ) );
            r.MapGet ( "/static/x", ( HttpRequest req ) => new HttpResponse () );
        }, shouldThrow: false );
    }

    [TestMethod]
    public void StaticRoutesTakePrecedenceOverDynamicRoutes () {
        var router = new Router ();
        var regexRoute = new RegexRoute ( RouteMethod.Get, "^/users/me$", req => new HttpResponse () );
        var prefixRoute = new PrefixRoute ( RouteMethod.Get, "/", null, req => new HttpResponse (), null );
        var userRoute = new Route ( RouteMethod.Get, "/users/<id>", ( HttpRequest req ) => new HttpResponse () );
        var rootRoute = new Route ( RouteMethod.Get, "/", ( HttpRequest req ) => new HttpResponse () );

        router.Map ( regexRoute );
        router.Map ( prefixRoute );
        router.Map ( userRoute );
        router.Map ( rootRoute );

        AssertStart ( router, shouldThrow: false );

        CollectionAssert.AreEqual (
            new Route [] { userRoute, rootRoute, regexRoute, prefixRoute },
            router.GetDefinedRoutes () );
    }

    [TestMethod]
    public void RouterPrefixAppliesOnlyToStaticRoutes () {
        var router = new Router () { Prefix = "/api" };
        var staticRoute = new Route ( RouteMethod.Get, "/users", ( HttpRequest req ) => new HttpResponse () );
        var prefixRoute = new PrefixRoute ( RouteMethod.Get, "/files", null, req => new HttpResponse (), null );
        var regexRoute = new RegexRoute ( RouteMethod.Get, "^/regex$", req => new HttpResponse () );

        router.Map ( staticRoute );
        router.Map ( prefixRoute );
        router.Map ( regexRoute );

        Assert.AreEqual ( "/api/users", staticRoute.Path );
        Assert.AreEqual ( "/files", prefixRoute.Path );
        Assert.AreEqual ( "^/regex$", regexRoute.Path );
    }

    [TestMethod]
    public void DocumentedCustomRouteMatchesAndIsDynamic () {
        var router = new Router ();
        var route = new ExtensionRoute ( ".csv", req => new HttpResponse () );

        RouteMatch match = route.Match ( "/reports/2024.csv", router );

        Assert.IsTrue ( match.Success );
        Assert.AreEqual ( "/reports/2024", match.Parameters? [ "file" ] );
        Assert.IsFalse ( route.Match ( "/reports/2024.txt", router ).Success );
        Assert.IsFalse ( route.IsStatic );
    }

    sealed class ExtensionRoute : Route {
        public ExtensionRoute ( string extension, RouteAction action ) : base ( RouteMethod.Get, extension, action ) {
        }

        public override bool IsStatic => false;

        public override bool AllowRewrites => false;

        public override RouteMatch Match ( string requestPath, Router router ) {
            StringComparison comparison = router.MatchRoutesIgnoreCase
                ? StringComparison.OrdinalIgnoreCase
                : StringComparison.Ordinal;

            if (!requestPath.EndsWith ( Path, comparison ))
                return RouteMatch.NotMatched;

            var parameters = new System.Collections.Specialized.NameValueCollection {
                [ "file" ] = requestPath [ ..^Path.Length ]
            };
            return new RouteMatch ( true, parameters );
        }
    }

    [TestMethod]
    public void DefaultRouteMatchUsesTemplateSemantics () {
        var route = new Route ( RouteMethod.Get, "/users/<id>/posts", ( HttpRequest req ) => new HttpResponse () );

        RouteMatch match = route.Match ( "/users/7/posts", new Router () );
        Assert.IsTrue ( match.Success );
        Assert.AreEqual ( "7", match.Parameters? [ "id" ] );

        Assert.IsFalse ( route.Match ( "/users/7", new Router () ).Success );
        Assert.IsFalse ( route.Match ( "/USERS/7/posts", new Router () ).Success );
        Assert.IsTrue ( route.Match ( "/USERS/7/posts", new Router () { MatchRoutesIgnoreCase = true } ).Success );
        Assert.IsTrue ( route.IsStatic );
        Assert.IsTrue ( route.AllowRewrites );
    }

    [TestMethod]
    public void RouteMatchPublicApi () {
        Assert.IsFalse ( RouteMatch.NotMatched.Success );
        Assert.IsNull ( RouteMatch.NotMatched.Parameters );

        var parameters = new System.Collections.Specialized.NameValueCollection { [ "a" ] = "1" };
        var match = new RouteMatch ( true, parameters );

        Assert.IsTrue ( match.Success );
        Assert.AreSame ( parameters, match.Parameters );
    }

    [TestMethod]
    public void OnPathModifiedReceivesOldAndNewPaths () {
        var route = new DelegateRoute ( "/a", isStatic: true, ( path, router ) => RouteMatch.NotMatched );
        route.PathChanges.Clear ();

        route.Path = "/b";
        route.Path = "/c";

        CollectionAssert.AreEqual ( new [] { "/a -> /b", "/b -> /c" }, route.PathChanges );
    }

    [TestMethod]
    public void RouterPrefixAppliesToCustomStaticRoutesOnly () {
        var router = new Router () { Prefix = "/api" };
        var staticRoute = new DelegateRoute ( "/items", isStatic: true, ( path, r ) => RouteMatch.NotMatched );
        var dynamicRoute = new DelegateRoute ( "/items", isStatic: false, ( path, r ) => RouteMatch.NotMatched );

        router.Map ( staticRoute );
        router.Map ( dynamicRoute );

        Assert.AreEqual ( "/api/items", staticRoute.Path );
        Assert.AreEqual ( "/items", dynamicRoute.Path );
    }

    [TestMethod]
    public void RegexRouteAttributesCreateRegexRoutesWithoutPrefix () {
        var router = new Router ();
        router.MapType<AttributeRoutes> ();

        Route [] routes = router.GetDefinedRoutes ();

        Route regexRoute = routes.Single ( r => r.Name == "regex" );
        Assert.IsInstanceOfType<RegexRoute> ( regexRoute );
        Assert.AreEqual ( @"^/regex/(?<id>\d+)$", regexRoute.Path );
        Assert.AreEqual ( "5", regexRoute.Match ( "/regex/5", router ).Parameters? [ "id" ] );

        Route useRegexRoute = routes.Single ( r => r.Name == "use-regex" );
        Assert.IsInstanceOfType<RegexRoute> ( useRegexRoute );

        Route templateRoute = routes.Single ( r => r.Name == "template" );
        Assert.AreEqual ( typeof ( Route ), templateRoute.GetType () );
        Assert.AreEqual ( "/attr/template", templateRoute.Path );

#pragma warning disable CS0618
        Assert.IsTrue ( regexRoute.UseRegex );
        Assert.IsFalse ( templateRoute.UseRegex );
#pragma warning restore CS0618
    }

    [TestMethod]
    public async Task CustomRouteParametersAreDecodedIntoRequest () {
        using var host = StartHost ( r => {
            r.Map ( new DelegateRoute ( "/custom", isStatic: false,
                ( path, router ) => path.StartsWith ( "/custom/", StringComparison.Ordinal )
                    ? new RouteMatch ( true, new System.Collections.Specialized.NameValueCollection {
                        [ "value" ] = "a%20b",
                        [ "empty" ] = ""
                    } )
                    : RouteMatch.NotMatched,
                req => new HttpResponse ( $"{req.RouteParameters [ "value" ].GetString ()}|{req.RouteParameters.ContainsKey ( "empty" )}" ) ) );
        } );
        using var client = CreateClient ( host );

        var response = await client.GetAsync ( "custom/anything" );

        Assert.AreEqual ( HttpStatusCode.OK, response.StatusCode );
        Assert.AreEqual ( "a b|False", await response.Content.ReadAsStringAsync () );
        Assert.AreEqual ( HttpStatusCode.NotFound, ( await client.GetAsync ( "other" ) ).StatusCode );
    }

    [TestMethod]
    public async Task StaticRoutesWinOverEarlierRegisteredCustomRoutes () {
        using var host = StartHost ( r => {
            r.Map ( new DelegateRoute ( "/", isStatic: false, ( path, router ) => new RouteMatch ( true, null ),
                req => new HttpResponse ( "custom" ) ) );
            r.MapGet ( "/exact", ( HttpRequest req ) => new HttpResponse ( "static" ) );
        } );
        using var client = CreateClient ( host );

        Assert.AreEqual ( "static", await client.GetStringAsync ( "exact" ) );
        Assert.AreEqual ( "custom", await client.GetStringAsync ( "anything/else" ) );
    }

    [TestMethod]
    public async Task DynamicRoutesAreMatchedInRegistrationOrder () {
        using var host = StartHost ( r => {
            r.Map ( new DelegateRoute ( "/first", isStatic: false, ( path, router ) => new RouteMatch ( true, null ),
                req => new HttpResponse ( "first" ) ) );
            r.Map ( new DelegateRoute ( "/second", isStatic: false, ( path, router ) => new RouteMatch ( true, null ),
                req => new HttpResponse ( "second" ) ) );
        } );
        using var client = CreateClient ( host );

        Assert.AreEqual ( "first", await client.GetStringAsync ( "whatever" ) );
    }

    [TestMethod]
    public async Task CustomRouteMethodMismatchReturnsMethodNotAllowedOrOptionsOk () {
        using var host = StartHost ( r => {
            r.Map ( new DelegateRoute ( "/m", isStatic: false, ( path, router ) => new RouteMatch ( path == "/m", null ),
                req => new HttpResponse ( "ok" ) ) );
        } );
        using var client = CreateClient ( host );

        Assert.AreEqual ( HttpStatusCode.OK, ( await client.GetAsync ( "m" ) ).StatusCode );
        Assert.AreEqual ( HttpStatusCode.MethodNotAllowed, ( await client.PostAsync ( "m", null ) ).StatusCode );
        Assert.AreEqual ( HttpStatusCode.OK, ( await client.SendAsync ( new HttpRequestMessage ( HttpMethod.Options, "m" ) ) ).StatusCode );
    }

    [TestMethod]
    public async Task ForceTrailingSlashRespectsAllowRewrites () {
        using var host = StartHost ( r => {
            r.MapGet ( "/static", ( HttpRequest req ) => new HttpResponse ( "static" ) );
            r.Map ( new DelegateRoute ( "/dynamic", isStatic: false, ( path, router ) => new RouteMatch ( path.StartsWith ( "/dynamic", StringComparison.Ordinal ), null ),
                req => new HttpResponse ( "dynamic" ) ) );
            r.Map ( new PrefixRoute ( RouteMethod.Get, "/prefix", null, req => new HttpResponse ( "prefix" ), null ) );
        }, config => config.ForceTrailingSlash = true );
        using var client = CreateClient ( host );

        var staticResponse = await client.GetAsync ( "static" );
        Assert.AreEqual ( HttpStatusCode.TemporaryRedirect, staticResponse.StatusCode );
        Assert.AreEqual ( "/static/", staticResponse.Headers.Location?.OriginalString );

        var dynamicResponse = await client.GetAsync ( "dynamic" );
        Assert.AreEqual ( HttpStatusCode.OK, dynamicResponse.StatusCode );
        Assert.AreEqual ( "dynamic", await dynamicResponse.Content.ReadAsStringAsync () );

        var prefixResponse = await client.GetAsync ( "prefix" );
        Assert.AreEqual ( HttpStatusCode.OK, prefixResponse.StatusCode );
    }

    sealed class DelegateRoute : Route {
        readonly Func<string, Router, RouteMatch> matcher;
        readonly bool isStatic;

        public List<string> PathChanges { get; } = new ();

        public DelegateRoute ( string path, bool isStatic, Func<string, Router, RouteMatch> matcher, RouteAction? action = null )
            : base ( RouteMethod.Get, path, action ?? ( req => new HttpResponse () ) ) {
            this.matcher = matcher;
            this.isStatic = isStatic;
        }

        public override bool IsStatic => isStatic;

        public override bool AllowRewrites => isStatic;

        public override RouteMatch Match ( string requestPath, Router router ) => matcher ( requestPath, router );

        protected override void OnPathModified ( string oldPath, string newPath ) {
            PathChanges.Add ( $"{oldPath} -> {newPath}" );
        }
    }

    [RoutePrefix ( "/attr" )]
    sealed class AttributeRoutes {
        [RegexRoute ( RouteMethod.Get, @"^/regex/(?<id>\d+)$", Name = "regex" )]
        public static HttpResponse Regex ( HttpRequest req ) => new HttpResponse ();

        [Route ( RouteMethod.Get, @"^/use-regex$", Name = "use-regex", UseRegex = true )]
        public static HttpResponse UseRegex ( HttpRequest req ) => new HttpResponse ();

        [RouteGet ( "/template", Name = "template" )]
        public static HttpResponse Template ( HttpRequest req ) => new HttpResponse ();
    }

    static HttpServerHostContext StartHost ( Action<Router> setup, Action<HttpServerConfiguration>? configure = null ) {
        var router = new Router ();
        setup ( router );

        var host = HttpServer.CreateBuilder ( ListeningPort.GetRandomPort ().Port )
            .UseConfiguration ( config => configure?.Invoke ( config ) )
            .UseRouter ( router )
            .Build ();

        host.Start ( verbose: false, preventHault: false );
        return host;
    }

    static HttpClient CreateClient ( HttpServerHostContext host ) => new HttpClient ( new HttpClientHandler { AllowAutoRedirect = false } ) {
        BaseAddress = new Uri ( host.HttpServer.ListeningPrefixes [ 0 ] ),
        Timeout = TimeSpan.FromSeconds ( 20 )
    };

    static void AssertStart ( Action<Router> setup, bool shouldThrow ) {
        var router = new Router ();
        setup ( router );
        AssertStart ( router, shouldThrow );
    }

    static void AssertStart ( Router router, bool shouldThrow ) {
        using var host = HttpServer.CreateBuilder ( ListeningPort.GetRandomPort ().Port ).UseRouter ( router ).Build ();

        if (shouldThrow) {
            Assert.ThrowsException<ArgumentException> ( () => host.Start ( verbose: false, preventHault: false ) );
        }
        else {
            host.Start ( verbose: false, preventHault: false );
        }
    }

    [TestMethod]
    public async Task FileServerServesOnlyItsPrefix () {
        using var client = Server.GetHttpClient ();

        var fileResponse = await client.GetAsync ( "tests/routing/files/sample.txt" );
        Assert.AreEqual ( HttpStatusCode.OK, fileResponse.StatusCode );
        Assert.AreEqual ( "prefix", await fileResponse.Content.ReadAsStringAsync () );

        var siblingResponse = await client.GetAsync ( "tests/routing/filesfoo/sample.txt" );
        Assert.AreEqual ( HttpStatusCode.NotFound, siblingResponse.StatusCode );

        var staticResponse = await client.GetAsync ( "tests/routing/files/static" );
        Assert.AreEqual ( HttpStatusCode.OK, staticResponse.StatusCode );
        Assert.AreEqual ( "static", await staticResponse.Content.ReadAsStringAsync () );
    }
}
