// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IContentSchemaTypeHandler.cs
// Repository:  https://github.com/sisk-http/core

using LightJson.Schema;

namespace Sisk.Documenting;

/// <summary>
/// Defines a handler that provides JSON schema information for types.
/// </summary>
public interface IContentSchemaTypeHandler {
    /// <summary>
    /// Generates a JSON schema that describes the specified type.
    /// </summary>
    /// <param name="type">The <see cref="Type"/> for which to generate the schema.</param>
    /// <returns>A <see cref="JsonSchema"/> that represents the structure of <paramref name="type"/>.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="type"/> is <see langword="null"/>.</exception>
    public JsonSchema GetJsonSchemaForType ( Type type );
}