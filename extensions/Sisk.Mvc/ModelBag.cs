// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   ModelBag.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Mvc;

public sealed record class ModelBag<TModel>(TModel? Model)
{
    public static readonly ModelBag<TModel> Empty = new ModelBag<TModel>(default(TModel));

    public string PageTitle { get; set; } = "Document";
    public string DebugId { get; set; } = Guid.NewGuid().ToString()[0..8];
}
