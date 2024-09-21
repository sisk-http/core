// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartObjectCommonFormatByteMark.cs
// Repository:  https://github.com/sisk-http/core

namespace Sisk.Core.Entity;

internal static class MultipartObjectCommonFormatByteMark
{
    public static readonly byte[] PNG = new byte[] { 0x89, 0x50, 0x4E, 0x47, 0x0D, 0x0A, 0x1A, 0x0A };

    public static readonly byte[] PDF = new byte[] { 0x25, 0x50, 0x44, 0x46 };
    public static readonly byte[] WEBP = new byte[] { 0x52, 0x49, 0x46, 0x46 };
    public static readonly byte[] TIFF = new byte[] { 0x4D, 0x4D, 0x00, 0x2A };
    public static readonly byte[] WEBM = new byte[] { 0x1A, 0x45, 0xDF, 0xA3 };

    public static readonly byte[] JPEG = new byte[] { 0xFF, 0xD8, 0xFF };
    public static readonly byte[] GIF = new byte[] { 0x47, 0x46, 0x49 };

    public static readonly byte[] BMP = new byte[] { 0x42, 0x4D };
}
