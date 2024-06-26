﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RouterFactory.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Collections.Specialized;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Provides a class that instantiates a router, capable of porting applications for use with Agirax.
    /// </summary>
    /// <definition>
    /// public abstract class RouterFactory
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Routing
    /// </namespace>
    public abstract class RouterFactory
    {
        /// <summary>
        /// Build and gets a router that will be used later by an <see cref="ListeningHost"/>.
        /// </summary>
        /// <definition>
        /// public abstract Router BuildRouter();
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public abstract Router BuildRouter();

        /// <summary>
        /// Method that is called by the service instantiator with the parameters defined in configuration before calling <see cref="BuildRouter()"/>.
        /// </summary>
        /// <param name="setupParameters">Parameters that are defined in a configuration file.</param>
        /// <definition>
        /// public abstract void Setup(NameValueCollection setupParameters);
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing
        /// </namespace>
        public abstract void Setup(NameValueCollection setupParameters);

        /// <summary>
        /// Method that is called immediately before initializing the service, after all configurations was parsed and set up.
        /// </summary>
        /// <definition>
        /// public abstract void Bootstrap();
        /// </definition>
        /// <type>
        /// Method
        /// </type> 
        public abstract void Bootstrap();
    }
}
