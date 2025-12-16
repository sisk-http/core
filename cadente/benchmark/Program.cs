using Sisk.Cadente;

var host = new HttpHost ( 5555 ) {
    UsePipelines = false  // Disable pipelines for testing
};

host.UseHandler ( async ctx => {
    ctx.Response.StatusCode = 200;
    ctx.Response.Headers.Set ( new HttpHeader ( "Content-Length", "13" ) );
    using var stream = await ctx.Response.GetResponseStreamAsync ( chunked: false );
    await stream.WriteAsync ( "Hello, World!"u8.ToArray () );
} );

host.Start ();
Console.WriteLine ( $"Server listening on http://localhost:5555" );
Console.WriteLine ( "Press Ctrl+C to stop..." );
Thread.Sleep ( -1 );
