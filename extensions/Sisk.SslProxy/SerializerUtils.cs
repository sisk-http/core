// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   SerializerUtils.cs
// Repository:  https://github.com/sisk-http/core

using System.Runtime.CompilerServices;
using System.Text;

namespace Sisk.Ssl;

static class SerializerUtils
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static string DecodeString(ReadOnlySpan<byte> data) => Encoding.UTF8.GetString(data);

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static byte[] EncodeString(string data) => Encoding.UTF8.GetBytes(data);

    public static byte[] ReadUntil(Stream inputStream, int intercept, int allocSize)
    {
        byte[] buffer = new byte[allocSize];
        int current, size = 0;
        while (inputStream.CanRead && ((current = inputStream.ReadByte()) >= 0))
        {
            if (current == intercept)
            {
                return buffer[0..size];
            }
            else
            {
                buffer[size] = (byte)current;
            }
            size++;
        }
        throw new InvalidDataException();
    }

    public static void CopyStream(Stream input, Stream output, int bytes)
    {
        byte[] buffer = new byte[81920];
        int read;
        while (bytes > 0 && (read = input.Read(buffer, 0, Math.Min(buffer.Length, bytes))) > 0)
        {
            output.Write(buffer, 0, read);
            bytes -= read;
        }
    }

    public static void CopyUntil(Stream input, Stream output, byte[] eof)
    {
        Span<byte> buffer = stackalloc byte[81920];
        int read;
        while ((read = input.Read(buffer)) > 0)
        {
            output.Write(buffer);
            if (buffer.EndsWith(eof))
            {
                break;
            }
        }
    }
}
