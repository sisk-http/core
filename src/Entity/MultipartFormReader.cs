// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormReader.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Internal;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Entity;

internal sealed class MultipartFormReader
{
    byte[] boundaryBytes;
    byte[] bytes;
    byte[] nlbytes;
    int position = 0;
    Encoding encoder;
    bool debugEnabled;

    public MultipartFormReader(byte[] inputBytes, byte[] boundaryBytes, Encoding baseEncoding, bool debugEnabled)
    {
        this.boundaryBytes = boundaryBytes;
        this.encoder = baseEncoding;
        this.bytes = inputBytes;
        this.position = 0;
        this.nlbytes = baseEncoding.GetBytes("\r\n");
        this.debugEnabled = debugEnabled;
    }

    void ThrowDataException(string message)
    {
        if (debugEnabled)
        {
            throw new InvalidDataException(SR.Format(SR.MultipartFormReader_InvalidData, position, message));
        }
    }

    bool CanRead { get => position < bytes.Length; }

    int ReadByte()
    {
        if (CanRead)
            return bytes[position++];

        return -1;
    }

    void ReadNewLine()
    {
        position += nlbytes.Length;
    }

    int Read(Span<byte> buffer)
    {
        int read = 0;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (ReadByte() is > 0 and int b)
            {
                buffer[read++] = (byte)b;
            }
            else break;
        }
        return read;
    }

    public MultipartObject[] Read()
    {
        List<MultipartObject> objects = new List<MultipartObject>();
        while (this.CanRead)
        {
            ReadNextBoundary();
            NameValueCollection headers = ReadHeaders();

            if (!CanRead)
                break;

            byte[] content = ReadContent().ToArray();

            ReadNewLine();

            string? contentDisposition = headers[HttpKnownHeaderNames.ContentDisposition];
            if (contentDisposition is null)
            {
                ThrowDataException("The Content-Disposition header is empty or missing.");
                continue;
            }

            NameValueCollection cdispositionValues = CookieParser.ParseCookieString(contentDisposition);

            string? formItemName = cdispositionValues["name"]?.Trim(SharedChars.DoubleQuote);
            string? formFilename = cdispositionValues["filename"]?.Trim(SharedChars.DoubleQuote);

            if (string.IsNullOrEmpty(formItemName))
            {
                ThrowDataException("The Content-Disposition \"name\" parameter is empty or missing.");
                continue;
            }

            MultipartObject resultObj = new MultipartObject(headers, formFilename, formItemName, content, encoder);

            objects.Add(resultObj);
        }

        return objects.ToArray();
    }

    string ReadLine()
    {
        Span<byte> line = stackalloc byte[2048];
        int read,
            n = 0,
            lnbytelen = nlbytes.Length;

        while ((read = ReadByte()) > 0)
        {
            if (n == line.Length)
            {
                ThrowDataException($"Header line was too long (> {line.Length} bytes allocated).");
                break;
            }

            line[n++] = (byte)read;

            if (n >= lnbytelen)
            {
                if (line[(n - lnbytelen)..n].SequenceEqual(nlbytes))
                {
                    break;
                }
            }
        }

        return encoder.GetString(line[0..n]);
    }

    Span<byte> ReadContent()
    {
        int boundaryLen = boundaryBytes.Length;
        int istart = position;

        while (CanRead)
        {
            position++;

            if ((position - istart) > boundaryLen)
            {
                if (bytes[(position - boundaryLen)..position].SequenceEqual(boundaryBytes))
                {
                    break;
                }
            }
        }

        position -= boundaryLen + nlbytes.Length + 2 /* the boundary "--" construct */;

        return bytes[istart..position];
    }

    NameValueCollection ReadHeaders()
    {
        NameValueCollection headers = new NameValueCollection();
        string? line;
        while (!string.IsNullOrEmpty(line = ReadLine()))
        {
            int sepIndex = line.IndexOf(':');
            if (sepIndex == -1)
                break;

            string hname = line.Substring(0, sepIndex);
            string hvalue = line.Substring(sepIndex + 1).Trim();

            headers.Add(hname, hvalue);
        }

        return headers;
    }

    unsafe void ReadNextBoundary()
    {
        Span<byte> boundaryBlock = stackalloc byte[boundaryBytes.Length + 2];
        int nextLine = Read(boundaryBlock);

        ReadNewLine();

        if (nextLine != boundaryBlock.Length)
        {
            ThrowDataException($"Boundary expected at byte {position}.");
        }
        if (!boundaryBlock[2..].SequenceEqual(boundaryBytes))
        {
            ThrowDataException($"The provided boundary string does not match the request boundary string.");
        }
    }
}