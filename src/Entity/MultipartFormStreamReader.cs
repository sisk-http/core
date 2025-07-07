// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormStreamReader.cs
// Repository:  https://github.com/sisk-http/core

//using System;
//using System.Buffers;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Linq;
//using System.Reflection.PortableExecutable;
//using System.Text;
//using System.Threading.Tasks;

//namespace Sisk.Core.Entity;
//internal class MultipartFormStreamReader {

//    Stream baseStream;
//    Encoding encoding;

//    public MultipartFormStreamReader ( Stream stream, Encoding encoding ) {
//        baseStream = stream ?? throw new ArgumentNullException ( nameof ( stream ), "The stream cannot be null." );
//        this.encoding = encoding ?? throw new ArgumentNullException ( nameof ( encoding ), "The encoding cannot be null." );
//    }

//    const byte CR = 0x0D;
//    const byte LF = 0x0A;
//    const byte COLON = 0x3A;
//    const byte SPACE = 0x20;

//    public async IAsyncEnumerable<MultipartObject> StreamObjectsAsync ( CancellationToken cancellation ) {

//        using var bufferOwnership = MemoryPool<byte>.Shared.Rent ( 65536 );
//        var memory = bufferOwnership.Memory;
//        while (await baseStream.ReadAsync ( memory, cancellation ) is { } read and >= 0) {

//            // read memory lines
//            int crlfindex = 0;
//            while ((crlfindex = memory.Span [ crlfindex.. ].IndexOfAny ( [ CR, LF ] )) >= 0) {

//                Memory<byte> line = memory [ 0..crlfindex ];

//            }
//        }
//    }
//}
