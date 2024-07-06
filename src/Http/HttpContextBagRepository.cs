// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpContextBagRepository.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Entity;

namespace Sisk.Core.Http;

/// <summary>
/// Represents a repository of information stored over the lifetime of a request.
/// </summary>
public sealed class HttpContextBagRepository : TypedValueDictionary
{
}
