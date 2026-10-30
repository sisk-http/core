// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SR.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;

namespace Sisk.IniConfiguration;

internal static class SR {
    public const string IniConfigurationReader_ReadPropertyFailed = "Caught exception while trying to read the property {0}: {1}";

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    public static string Format ( string format, params object? [] items ) {
        return String.Format ( provider: null, format, items );
    }
}
