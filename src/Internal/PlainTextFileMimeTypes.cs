// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   PlainTextFileMimeTypes.cs
// Repository:  https://github.com/sisk-http/core

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal;

static class PlainTextFileMimeTypes {

    public static readonly string [] PlainTextMimeTypes = [
        "application/json",
        "application/javascript",
        "application/xml",
        "application/ld+json",
        "application/yaml",
        "application/graphql",
        "application/sql",
        "application/atom+xml",
        "application/rss+xml",
        "application/manifest+json"
    ];
}
