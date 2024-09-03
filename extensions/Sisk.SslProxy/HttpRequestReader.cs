// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpRequestReader.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Diagnostics.CodeAnalysis;

namespace Sisk.Ssl;

static class HttpRequestReader
{
    public static bool TryReadHttp1Request(Stream inboundStream,
                            string? replaceHostName,
        [NotNullWhen(true)] out string? method,
        [NotNullWhen(true)] out string? path,
        [NotNullWhen(true)] out string? proto,
                            out int contentLength,
                            out List<(string, string)> headers)
    {
        contentLength = 0;
        try
        {
            byte[] _method = SerializerUtils.ReadUntil(inboundStream, Constants.CH_SPACE, 16);
            method = SerializerUtils.DecodeString(_method);

            byte[] _path = SerializerUtils.ReadUntil(inboundStream, Constants.CH_SPACE, 2048);
            path = SerializerUtils.DecodeString(_path);

            byte[] _protocol = SerializerUtils.ReadUntil(inboundStream, Constants.CH_RETURN, 8);
            proto = SerializerUtils.DecodeString(_protocol);

            inboundStream.ReadByte(); // \n

            List<(string, string)> headerList = new List<(string, string)>();
            while (inboundStream.CanRead)
            {
                char? firstReadChar;
                int c = inboundStream.ReadByte();
                if (c == Constants.CH_RETURN)
                {
                    inboundStream.ReadByte(); // \n
                    break;
                }
                else
                {
                    firstReadChar = (char)c;
                }

                byte[] headerName = SerializerUtils.ReadUntil(inboundStream, Constants.CH_HSEP, 256);
                byte[] headerValue = SerializerUtils.ReadUntil(inboundStream, Constants.CH_RETURN, 2048);

                inboundStream.ReadByte(); // \n

                string hName = firstReadChar + SerializerUtils.DecodeString(headerName).Trim();
                string hValue = SerializerUtils.DecodeString(headerValue).TrimStart();

                if (string.Compare(hName, HttpKnownHeaderNames.ContentLength, true) == 0)
                {
                    contentLength = int.Parse(hValue);
                }
                else if (string.Compare(hName, HttpKnownHeaderNames.Host, true) == 0 && replaceHostName is not null)
                {
                    hValue = replaceHostName;
                }

                headerList.Add((hName, hValue));
            }

            headers = headerList;
            return true;
        }
        catch
        {
            method = null;
            path = null;
            proto = null;
            headers = new();
            return false;
        }
    }


}
