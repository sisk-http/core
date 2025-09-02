// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   McpSerializerContext.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Sisk.ModelContextProtocol;

[JsonSerializable ( typeof ( JsonRpcResponse ) )]
internal partial class McpSerializerContext : JsonSerializerContext {
}
