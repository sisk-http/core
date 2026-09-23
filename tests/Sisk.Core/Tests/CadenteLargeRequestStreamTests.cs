using System.Reflection;
using Sisk.Cadente;

namespace tests.Tests;

[TestClass]
public sealed class CadenteLargeRequestStreamTests {

    [DataTestMethod]
    [DataRow ( 2147483647L, 0 )]
    [DataRow ( 2147483648L, 0 )]
    [DataRow ( 2147483649L, 4096 )]
    [DataRow ( 4831838208L, 4096 )]
    public async Task ContentLengthAboveInt32_ReturnsEofWithoutConsumingNextRequest ( long contentLength, int bufferedBytes ) {
        using var transport = new SyntheticRequestStream ( contentLength - bufferedBytes );
        using Stream input = CreateRequestStream ( transport, contentLength, bufferedBytes );
        byte [] buffer = new byte [ 81920 ];
        long received = 0;

        while (received < contentLength) {
            int read = input.Read ( buffer, 0, buffer.Length );
            Assert.IsTrue ( read > 0 );
            received += read;
        }

        Assert.AreEqual ( contentLength, received );
        Assert.AreEqual ( contentLength, input.Position );
        Assert.AreEqual ( 0, input.Read ( buffer, 0, buffer.Length ) );
        Assert.AreEqual ( 0, await input.ReadAsync ( buffer.AsMemory () ) );
        Assert.AreEqual ( 0, transport.ReadsPastBody );
        Assert.AreEqual ( (int) 'G', transport.ReadByte () );
    }

    [DataTestMethod]
    [DataRow ( 2147483648L, 0 )]
    [DataRow ( 4831838208L, 4096 )]
    public async Task ContentLengthAboveInt32_DrainsRemainingBodyWithoutConsumingNextRequest ( long contentLength, int bufferedBytes ) {
        using var transport = new SyntheticRequestStream ( contentLength - bufferedBytes );
        using Stream input = CreateRequestStream ( transport, contentLength, bufferedBytes );
        byte [] buffer = new byte [ 81920 ];
        long received = 0;

        while (received < contentLength - buffer.Length) {
            received += await input.ReadAsync ( buffer.AsMemory () );
        }

        MethodInfo drain = input.GetType ().GetMethod ( "DrainAsync" )!;
        bool drained = await (Task<bool>) drain.Invoke ( input, [ CancellationToken.None ] )!;

        Assert.IsTrue ( drained );
        Assert.AreEqual ( contentLength, input.Position );
        Assert.AreEqual ( 0, transport.ReadsPastBody );
        Assert.AreEqual ( 0, input.Read ( buffer, 0, buffer.Length ) );
        Assert.AreEqual ( (int) 'G', transport.ReadByte () );
    }

    private static Stream CreateRequestStream ( Stream transport, long contentLength, int bufferedBytes ) {
        Assembly assembly = typeof ( HttpHost ).Assembly;
        Type requestType = assembly.GetType ( "Sisk.Cadente.HttpSerializer.HttpRequestBase", throwOnError: true )!;
        object request = Activator.CreateInstance ( requestType )!;
        requestType.GetField ( "ContentLength" )!.SetValue ( request, contentLength );
        requestType.GetField ( "BufferedContent" )!.SetValue ( request, new ReadOnlyMemory<byte> ( new byte [ bufferedBytes ] ) );

        Type streamType = assembly.GetType ( "Sisk.Cadente.Streams.HttpRequestStream", throwOnError: true )!;
        return (Stream) Activator.CreateInstance ( streamType, transport, request )!;
    }

    private sealed class SyntheticRequestStream ( long remaining ) : Stream {
        public int ReadsPastBody { get; private set; }
        public override bool CanRead => true;
        public override bool CanSeek => false;
        public override bool CanWrite => false;
        public override long Length => throw new NotSupportedException ();
        public override long Position { get => throw new NotSupportedException (); set => throw new NotSupportedException (); }

        public override int Read ( byte [] buffer, int offset, int count ) {
            if (remaining == 0) {
                ReadsPastBody++;
                Assert.AreEqual ( 1, ReadsPastBody, "Request stream read beyond Content-Length." );
                buffer [ offset ] = (byte) 'G';
                return 1;
            }

            int read = (int) Math.Min ( count, remaining );
            buffer.AsSpan ( offset, read ).Clear ();
            remaining -= read;
            return read;
        }

        public override void Flush () => throw new NotSupportedException ();
        public override long Seek ( long offset, SeekOrigin origin ) => throw new NotSupportedException ();
        public override void SetLength ( long value ) => throw new NotSupportedException ();
        public override void Write ( byte [] buffer, int offset, int count ) => throw new NotSupportedException ();
    }
}
