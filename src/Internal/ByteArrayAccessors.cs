// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ByteArrayAccessors.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.Core;

class ByteArrayAccessors
{
    [UnsafeAccessor(UnsafeAccessorKind.Field, Name = "_content")]
    public extern static ref byte[] UnsafeGetContent(ByteArrayContent bcontent);
}
