using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;

namespace tests;

[TestClass]
public sealed class Server {
    public static HttpServerHostContext Instance = null!;

    public static HttpClient GetHttpClient () => new HttpClient () { BaseAddress = new Uri ( Instance.HttpServer.ListeningPrefixes [ 0 ] ) };

    [AssemblyInitialize]
    public static void AssemblyInit ( TestContext testContext ) {

        Instance = HttpServer.CreateBuilder ()
            .UseRouter ( router => {

                router.MapGet ( "/tests/plaintext", delegate ( HttpRequest request ) {
                    return new HttpResponse () {
                        Content = new StringContent ( "Hello, world!", Encoding.UTF8, "text/plain" ),
                        Status = HttpStatusInformation.Ok
                    };
                } );
            } )
            .Build ();

        Instance.Start ( verbose: false, preventHault: false );
    }

    [AssemblyCleanup]
    public static void AssemblyCleanup () {
        Instance.Dispose ();
    }
}
