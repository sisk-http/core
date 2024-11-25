// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartFormReader.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using Sisk.Core.Internal;
using System.Collections.Specialized;
using System.Text;

namespace Sisk.Core.Entity;

internal sealed class MultipartFormReader
{
    readonly byte[] boundaryBytes;
    readonly byte[] bytes;
    readonly byte[] nlbytes;
    int position = 0;
    readonly Encoding encoder;
    readonly bool debugEnabled;

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
        if (this.debugEnabled)
        {
            throw new InvalidDataException(SR.Format(SR.MultipartFormReader_InvalidData, this.position, message));
        }
    }

    bool CanRead { get => this.position < this.bytes.Length; }

    int ReadByte()
    {
        if (this.CanRead)
            return this.bytes[this.position++];

        return -1;
    }

    void ReadNewLine()
    {
        this.position += this.nlbytes.Length;
    }

    int Read(Span<byte> buffer)
    {
        int read = 0;
        for (int i = 0; i < buffer.Length; i++)
        {
            if (this.ReadByte() is > 0 and int b)
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
            this.ReadNextBoundary();
            NameValueCollection headers = this.ReadHeaders();

            if (!this.CanRead)
                break;

            byte[] content = this.ReadContent().ToArray();

            this.ReadNewLine();

            string? contentDisposition = headers[HttpKnownHeaderNames.ContentDisposition];
            if (contentDisposition is null)
            {
                this.ThrowDataException("The Content-Disposition header is empty or missing.");
                continue;
            }

            var cdispositionValues = StringKeyStore.FromCookieString(contentDisposition);

            string? formItemName = cdispositionValues["name"]?.Trim(SharedChars.DoubleQuote);
            string? formFilename = cdispositionValues["filename"]?.Trim(SharedChars.DoubleQuote);

            if (string.IsNullOrEmpty(formItemName))
            {
                this.ThrowDataException("The Content-Disposition \"name\" parameter is empty or missing.");
                continue;
            }

            MultipartObject resultObj = new MultipartObject(headers, formFilename, formItemName, content, this.encoder);

            objects.Add(resultObj);
        }

        return objects.ToArray();
    }

    string ReadLine()
    {
        Span<byte> line = stackalloc byte[2048];
        int read,
            n = 0,
            lnbytelen = this.nlbytes.Length;

        while ((read = this.ReadByte()) > 0)
        {
            if (n == line.Length)
            {
                this.ThrowDataException($"Header line was too long (> {line.Length} bytes allocated).");
                break;
            }

            line[n++] = (byte)read;

            if (n >= lnbytelen)
            {
                if (line[(n - lnbytelen)..n].SequenceEqual(this.nlbytes))
                {
                    break;
                }
            }
        }

        return this.encoder.GetString(line[0..n]);
    }

    Span<byte> ReadContent()
    {
        var boundarySpan = this.boundaryBytes.AsSpan();
        int boundaryLen = this.boundaryBytes.Length;
        int istart = this.position;

        while (this.CanRead)
        {
            this.position++;

            if ((this.position - istart) > boundaryLen)
            {
                if (this.bytes[(this.position - boundaryLen)..this.position].AsSpan().SequenceCompareTo(boundarySpan) == 0)
                {
                    break;
                }
            }
        }

        this.position -= boundaryLen + this.nlbytes.Length + 2 /* +2 represents the boundary "--" construct */;

        return this.bytes.AsSpan()[istart..this.position];
    }

    NameValueCollection ReadHeaders()
    {
        NameValueCollection headers = new NameValueCollection();
        string? line;
        while (!string.IsNullOrEmpty(line = this.ReadLine()))
        {
            int sepIndex = line.IndexOf(':', StringComparison.Ordinal);
            if (sepIndex == -1)
                break;

            string hname = line[..sepIndex];
            string hvalue = line[(sepIndex + 1)..].Trim();

            headers.Add(hname, hvalue);
        }

        return headers;
    }

    void ReadNextBoundary()
    {
        Span<byte> boundaryBlock = stackalloc byte[this.boundaryBytes.Length + 2];
        int nextLine = this.Read(boundaryBlock);

        this.ReadNewLine();

        if (nextLine != boundaryBlock.Length)
        {
            this.ThrowDataException($"Boundary expected at byte {this.position}.");
        }
        if (!boundaryBlock[2..].SequenceEqual(this.boundaryBytes))
        {
            this.ThrowDataException($"The provided boundary string does not match the request boundary string.");
        }
    }
}