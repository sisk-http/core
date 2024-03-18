// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PortableConfigurationRequireSection.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Http.Hosting;

/// <summary>
/// Defines which sessions are mandatory in the portable configuration file.
/// </summary>
/// <definition>
/// [Flags]
/// public enum PortableConfigurationRequireSection
/// </definition>
/// <type>
/// Enum
/// </type>
[Flags]
public enum PortableConfigurationRequireSection
{
    /// <summary>
    /// Defines that the ServerConfiguration section is mandatory.
    /// </summary>
    ServerConfiguration = 2,

    /// <summary>
    /// Defines that the ListeningHost section is mandatory.
    /// </summary>
    ListeningHost = 4,

    /// <summary>
    /// Defines that the Parameters section is mandatory.
    /// </summary>
    Parameters = 8,

    /// <summary>
    /// Defines that the all sections is mandatory.
    /// </summary>
    All = ServerConfiguration | ListeningHost | Parameters
}
