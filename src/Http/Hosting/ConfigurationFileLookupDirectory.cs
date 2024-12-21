// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ConfigurationFileLookupDirectory.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Represents the base directory where the <see cref="IConfigurationReader"/> should search for the configuration
/// file.
/// </summary>
[Flags]
public enum ConfigurationFileLookupDirectory {
    /// <summary>
    /// The <see cref="IConfigurationReader"/> should search in the process current/base directory.
    /// </summary>
    CurrentDirectory = 1 << 1,

    /// <summary>
    /// The <see cref="IConfigurationReader"/> should search in the executable base directory.
    /// </summary>
    AppDirectory = 1 << 2
}
