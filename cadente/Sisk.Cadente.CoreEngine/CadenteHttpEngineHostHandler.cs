
// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   CadenteHttpEngineHostHandler.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Cadente.CoreEngine
{
    internal class CadenteHttpEngineHostHandler : HttpHostHandler
    {
        private readonly CadenteHttpServerEngine _engine;

        public CadenteHttpEngineHostHandler(CadenteHttpServerEngine engine)
        {
            _engine = engine;
        }

        public override async Task OnContextCreatedAsync(HttpHost sender, HttpHostContext context)
        {
            var request = new CadenteHttpServerEngineRequest(context.Request, context);
            var response = new CadenteHttpServerEngineResponse(context.Response, context);
            var engineContext = new CadenteHttpServerEngineContext(request, response);

            _engine.EnqueueContext(engineContext);

            await engineContext.ProcessingTask;
        }
    }
}
