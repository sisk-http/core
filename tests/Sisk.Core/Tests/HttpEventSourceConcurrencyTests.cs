using System.Collections.Concurrent;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Text;
using Sisk.Core.Http;
using Sisk.Core.Http.Engine;
using Sisk.Core.Http.Streams;

namespace tests.Tests;

[TestClass]
public sealed class HttpEventSourceConcurrencyTests {
    [DataTestMethod]
    [DataRow ( false, false )]
    [DataRow ( false, true )]
    [DataRow ( true, false )]
    [DataRow ( true, true )]
    public async Task ConcurrentSends_KeepFlushSerialized ( bool firstAsync, bool secondAsync ) {
        using var stream = new ControlledStream { BlockFirstFlush = true };
        using var source = CreateSource ( stream );
        Task<bool> first = Task.Run ( async () => firstAsync
            ? await source.SendAsync ( "A" )
            : source.Send ( "A" ) );
        Task<bool>? second = null;
        try {
            await stream.FlushEntered.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );
            var flushLock = (SemaphoreSlim) typeof ( HttpRequestEventSource )
                .GetField ( "flushLock", BindingFlags.Instance | BindingFlags.NonPublic )!.GetValue ( source )!;

            Assert.AreEqual ( 0, flushLock.CurrentCount, "The flush must remain inside the send critical section." );
            second = Task.Run ( async () => secondAsync
                ? await source.SendAsync ( "B" )
                : source.Send ( "B" ) );
        }
        finally {
            stream.ReleaseFlush.TrySetResult ();
            await first.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );
            if (second is not null)
                await second.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );
        }

        CollectionAssert.AreEqual ( new [] { "data: A\n\n", "data: B\n\n" }, stream.Writes.ToArray () );
    }

    [DataTestMethod]
    [DataRow ( false, false )]
    [DataRow ( false, true )]
    [DataRow ( true, false )]
    [DataRow ( true, true )]
    public async Task FlushFailure_DisposesSource ( bool asynchronous, bool closing ) {
        using var stream = new ControlledStream { FailFlush = true };
        using var source = CreateSource ( stream );
        Task terminated = source.WaitForFailAsync ( CancellationToken.None );

        if (closing) {
            if (asynchronous)
                await source.CloseAsync ();
            else
                source.Close ();
        }
        else {
            bool sent = asynchronous ? await source.SendAsync ( "A" ) : source.Send ( "A" );
            Assert.IsFalse ( sent );
        }

        Assert.IsFalse ( source.IsActive );
        await terminated.WaitAsync ( TimeSpan.FromSeconds ( 5 ) );
        Assert.IsFalse ( await source.SendAsync ( "after failure" ) );
        Assert.IsFalse ( source.Send ( "after failure" ) );
    }

    private static HttpRequestEventSource CreateSource ( Stream stream ) {
        var source = (HttpRequestEventSource) RuntimeHelpers.GetUninitializedObject ( typeof ( HttpRequestEventSource ) );
        var ping = Activator.CreateInstance ( typeof ( HttpStreamPingPolicy ),
            BindingFlags.Instance | BindingFlags.NonPublic, null, [ source ], null )!;

        foreach (var (name, value) in new (string, object) [] {
            ("res", new FakeResponse ( stream )),
            ("flushLock", new SemaphoreSlim ( 1, 1 )),
            ("sendQueue", new ConcurrentQueue<string> ()),
            ("disposeLock", new object ()),
            ("sseTerminationSource", new TaskCompletionSource ()),
            ("pingPolicy", ping),
            ("hostServer", Server.Instance.HttpServer)
        }) {
            typeof ( HttpRequestEventSource ).GetField ( name, BindingFlags.Instance | BindingFlags.NonPublic )!
                .SetValue ( source, value );
        }

        return source;
    }

    private sealed class ControlledStream : MemoryStream {
        public bool BlockFirstFlush { get; init; }
        public bool FailFlush { get; init; }
        public TaskCompletionSource FlushEntered { get; } = new ( TaskCreationOptions.RunContinuationsAsynchronously );
        public TaskCompletionSource ReleaseFlush { get; } = new ( TaskCreationOptions.RunContinuationsAsynchronously );
        public ConcurrentQueue<string> Writes { get; } = new ();
        private int flushCount;

        public override void Write ( byte [] buffer, int offset, int count ) => Write ( buffer.AsSpan ( offset, count ) );

        public override void Write ( ReadOnlySpan<byte> buffer ) => Writes.Enqueue ( Encoding.UTF8.GetString ( buffer ) );

        public override ValueTask WriteAsync ( ReadOnlyMemory<byte> buffer, CancellationToken cancellationToken = default ) {
            Write ( buffer.Span );
            return ValueTask.CompletedTask;
        }

        public override void Flush () => FlushAsync ( CancellationToken.None ).GetAwaiter ().GetResult ();

        public override async Task FlushAsync ( CancellationToken cancellationToken ) {
            if (FailFlush)
                throw new IOException ( "Injected flush failure." );

            if (Interlocked.Increment ( ref flushCount ) == 1 && BlockFirstFlush) {
                FlushEntered.TrySetResult ();
                await ReleaseFlush.Task.WaitAsync ( TimeSpan.FromSeconds ( 5 ), cancellationToken );
            }
        }
    }

    private sealed class FakeResponse ( Stream stream ) : HttpServerEngineContextResponse {
        public override int StatusCode { get; set; } = 200;
        public override string StatusDescription { get; set; } = "OK";
        public override bool KeepAlive { get; set; }
        public override bool SendChunked { get; set; }
        public override long ContentLength64 { get; set; }
        public override string? ContentType { get; set; }
        public override IHttpEngineHeaderList Headers => throw new NotSupportedException ();
        public override Stream OutputStream => stream;
        public override void AppendHeader ( string name, string value ) { }
        public override void Abort () { }
        public override void Close () { }
        public override void Dispose () { }
    }
}
