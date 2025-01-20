// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ByteArrayAccessors.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Core.Internal;

class ByteArrayAccessors {
    [UnsafeAccessor ( UnsafeAccessorKind.Field, Name = "_content" )]
    public extern static ref byte [] UnsafeGetContent ( ByteArrayContent bcontent );

    [UnsafeAccessor ( UnsafeAccessorKind.Field, Name = "_offset" )]
    public extern static ref int UnsafeGetOffset ( ByteArrayContent bcontent );

    [UnsafeAccessor ( UnsafeAccessorKind.Field, Name = "_count" )]
    public extern static ref int UnsafeGetCount ( ByteArrayContent bcontent );
}
