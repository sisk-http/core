// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ViewManager.cs
// Repository:  https://github.com/sisk-http/core

using HandlebarsDotNet;
using Sisk.Core.Http;
using System.Reflection;
using System.Text;

namespace Sisk.Mvc;

public static class ViewManager
{
    public static void InitializePartials()
    {
        var a = Assembly.GetEntryAssembly()!;
        var resources = a.GetManifestResourceNames();

        foreach (var resource in resources)
        {
            if (resource.EndsWith(".hbs", StringComparison.OrdinalIgnoreCase))
            {
                using Stream resourceStream = a.GetManifestResourceStream(resource)!;
                using StreamReader sr = new StreamReader(resourceStream, Encoding.Default);

                string partialSource = sr.ReadToEnd();
                string partialName = resource[(resource.IndexOf('.') + 1)..(resource.LastIndexOf('.'))];

                Handlebars.RegisterTemplate(partialName, partialSource);
            }
        }
    }

    public static ViewResponseHandler<TModel> CreateView<TModel>(string viewPath)
    {
        using Stream? resourceStream = AssemblyResourceHelper.GetAssemblyResourceStream(viewPath);
        if (resourceStream is null)
        {
            throw new FileNotFoundException("The specified view path was not found.");
        }

        using var sr = new StreamReader(resourceStream, Encoding.Default);
        string viewText = sr.ReadToEnd();

        var h = Handlebars.Compile(viewText);

        return new ViewResponseHandler<TModel>(delegate (ModelBag<TModel> model)
        {
            ModelBag<TModel> _model = model;
            if (_model is null)
            {
                _model = ModelBag<TModel>.Empty;
            }

            string html = h(new
            {
                Bag = _model
            });

            return new HttpResponse()
                .WithContent(new HtmlContent(html));
        });
    }
}

public delegate HttpResponse ViewResponseHandler<TModel>(ModelBag<TModel> model = default!);