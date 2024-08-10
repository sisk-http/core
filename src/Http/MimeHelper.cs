// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MimeHelper.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;

namespace Sisk.Core.Http;

/// <summary>
/// Provides useful helper methods for resolving mime-types from common formats.
/// </summary>
public static class MimeHelper
{
    /// <summary>
    /// Gets the content mime-type from the specified file extension.
    /// </summary>
    /// <param name="fileExtension">The file extension, with or without the initial dot.</param>
    /// <param name="defaultMime">The default mime-type when the file is not found.</param>
    /// <returns>The best matched mime-type, or the default if no mime-type was matched with the specified extension.</returns>
    public static string GetMimeType(string fileExtension, string defaultMime = "application/octet-stream")
    {
        return MimeTypeList.ResolveMimeType(fileExtension.TrimStart('.').ToLower()) ?? defaultMime;
    }
}
