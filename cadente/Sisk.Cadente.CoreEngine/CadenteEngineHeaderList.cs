// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteEngineHeaderList.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Sisk.Core.Http.Engine;

namespace Sisk.Cadente.CoreEngine;

internal sealed class CadenteEngineHeaderList ( HttpHostContext.HttpResponse response ) : IHttpEngineHeaderList {
    readonly HttpHostContext.HttpResponse _response = response;

    public int Count => _response.Headers.Count;

    public string [] DefinedHeaderNames => _response.Headers.Select ( h => h.Name ).ToArray ();

    public void AppendHeader ( string name, string value ) {
        _response.Headers.Add ( new HttpHeader ( name, value ) );
    }

    public void Clear () {
        _response.Headers.Clear ();
    }

    public bool Contains ( string name ) {
        return _response.Headers.Any ( h => h.Name.Equals ( name, StringComparison.OrdinalIgnoreCase ) );
    }

    public string [] GetHeader ( string name ) {
        return _response.Headers
            .Where ( h => h.Name.Equals ( name, StringComparison.OrdinalIgnoreCase ) )
            .Select ( h => h.Value )
            .ToArray ();
    }

    public void SetHeader ( string name, string value ) {
        _response.Headers.Set ( new HttpHeader ( name, value ) );
    }
}
