// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   IConfigurationReader.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal.ServiceProvider;

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents an interface that reads and applies settings from a settings file.
/// </summary>
public interface IConfigurationReader
{
    /// <summary>
    /// Represents the method that reads and applies settings from a settings file.
    /// </summary>
    /// <param name="context">The configuration context object.</param>
    public void ReadConfiguration(ConfigurationContext context);
}
