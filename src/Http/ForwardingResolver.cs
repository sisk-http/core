// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ForwardingResolver.cs
// Repository:  https://github.com/sisk-http/core

using System.Net;

namespace Sisk.Core.Http;

/// <summary>
/// Provides HTTP forwarding resolving methods that can be used to resolving the client remote
/// address, host and protocol of a proxy, load balancer or CDN, through the HTTP request.
/// </summary>
public abstract class ForwardingResolver {
    /// <summary>
    /// Method that is called when resolving the IP address of the client in the request.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/> object which contains parameters of the request.</param>
    /// <param name="connectingEndpoint">The original connecting endpoint.</param>
    /// <returns></returns>
    public virtual IPAddress OnResolveClientAddress ( HttpRequest request, IPEndPoint connectingEndpoint ) {
        return connectingEndpoint.Address;
    }

    /// <summary>
    /// Method that is called when resolving the client request host.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/> object which contains parameters of the request.</param>
    /// <param name="requestedHost">The original requested host.</param>
    /// <returns></returns>
    public virtual string OnResolveRequestHost ( HttpRequest request, string requestedHost ) {
        return requestedHost;
    }

    /// <summary>
    /// Method that is called when resolving whether the HTTP request is using HTTPS or HTTP.
    /// </summary>
    /// <param name="request">The <see cref="HttpRequest"/> object which contains parameters of the request.</param>
    /// <param name="isSecure">The original security state of the request.</param>
    /// <returns></returns>
    public virtual bool OnResolveSecureConnection ( HttpRequest request, bool isSecure ) {
        return isSecure;
    }
}
