using Sisk.Core.Http;
using Sisk.Core.Routing;

const string PREFIX = "http://localhost:5555/";

Router router = new Router();
router += new Route(RouteMethod.Get, "/", request =>
{
    var content = new System.Net.Http.StringContent("<h1>Hello world from Sisk!</h1>", System.Text.Encoding.UTF8, "text/html");
    return new HttpResponse(content);
});

ListeningHost host = new ListeningHost(PREFIX, router);

HttpServerConfiguration config = new HttpServerConfiguration();
config.ListeningHosts.Add(host);
config.AccessLogsStream = null;

HttpServer server = new HttpServer(config);

server.Start();
System.Console.WriteLine("HTTP Server is listening at " + PREFIX);
System.Threading.Thread.Sleep(-1);