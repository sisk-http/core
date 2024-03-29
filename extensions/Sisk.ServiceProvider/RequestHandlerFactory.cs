﻿// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   RequestHandlerFactory.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;

namespace Sisk.Core.Routing
{
    /// <summary>
    /// Provides a class that instantiates request handlers.
    /// </summary>
    /// <definition>
    /// public abstract class RequestHandlerFactory
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace> 
    /// Sisk.Core.Routing.Handlers
    /// </namespace>
    public abstract class RequestHandlerFactory
    {
        /// <summary>
        /// Builds and gets the <see cref="IRequestHandler"/> instance objects used later by an <see cref="Route"/>.
        /// </summary>
        /// <definition>
        /// public abstract IRequestHandler[] BuildRequestHandlers();
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public abstract IRequestHandler[] BuildRequestHandlers();

        /// <summary>
        /// Method that is called by the Agirax instantiator with the parameters defined in configuration before calling <see cref="BuildRequestHandlers()"/>.
        /// </summary>
        /// <param name="setupParameters">Parameters that are defined in a configuration file.</param>
        /// <definition>
        /// public abstract void Setup(NameValueCollection setupParameters);
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Routing.Handlers
        /// </namespace>
        public abstract void Setup(NameValueCollection setupParameters);
    }
}