// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ProviderContext.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Http.Hosting;

namespace Sisk.Core.Internal.ServiceProvider;

internal class ProviderContext
{
    public string ConfigurationFile;
    public HttpServerHostContext Host = null!;
    public ListeningHost TargetListeningHost = null!;
    public bool CreateConfigurationFileIfDoenstExists = false;
    public PortableConfigurationRequireSection _requiredSections = default;

    public ProviderContext(String configurationFile)
    {
        ConfigurationFile = configurationFile;
    }
}
