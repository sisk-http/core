// The Sisk Framework source code
// Copyright (c) 2023 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartObject.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Http;
using System.Collections.Specialized;
using System.Text;

namespace Sisk.Core.Entity
{
    /// <summary>
    /// Represents an multipart/form-data object.
    /// </summary>
    public sealed class MultipartObject
    {
        private readonly Encoding _baseEncoding;

        /// <summary>
        /// Gets this <see cref="MultipartObject"/> headers.
        /// </summary>
        public HttpHeaderCollection Headers { get; private set; }

        /// <summary>
        /// Gets this <see cref="MultipartObject"/> provided file name. If this object ins't disposing a file,
        /// nothing is returned.
        /// </summary>
        public string? Filename { get; private set; }

        /// <summary>
        /// Gets this <see cref="MultipartObject"/> field name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// Gets this <see cref="MultipartObject"/> form data content in bytes.
        /// </summary>
        public byte[] ContentBytes { get; private set; }

        /// <summary>
        /// Gets this <see cref="MultipartObject"/> form data content length in byte count.
        /// </summary>
        public int ContentLength { get; private set; }

        /// <summary>
        /// Gets an booolean indicating if this <see cref="MultipartObject"/> has contents or not.
        /// </summary>
        public bool HasContents { get => this.ContentLength > 0; }

        /// <summary>
        /// Reads the content bytes with the given encoder.
        /// </summary>
        public string ReadContentAsString(Encoding encoder)
        {
            if (this.ContentLength == 0)
                return string.Empty;
            return encoder.GetString(this.ContentBytes);
        }

        /// <summary>
        /// Reads the content bytes using the HTTP request content-encoding.
        /// </summary>
        public string ReadContentAsString()
        {
            return this.ReadContentAsString(this._baseEncoding);
        }

        /// <summary>
        /// Determines the image format based in the file header for each image content type.
        /// </summary>
        public MultipartObjectCommonFormat GetCommonFileFormat()
        {
            int byteLen = this.ContentBytes.Length;

            if (byteLen >= 8)
            {
                Span<byte> len8 = this.ContentBytes.AsSpan(0, 8);

                if (len8.SequenceEqual(MultipartObjectCommonFormatByteMark.PNG))
                {
                    return MultipartObjectCommonFormat.PNG;
                }
            }
            if (byteLen >= 4)
            {
                Span<byte> len4 = this.ContentBytes.AsSpan(0, 4);

                if (len4.SequenceEqual(MultipartObjectCommonFormatByteMark.WEBP))
                {
                    return MultipartObjectCommonFormat.WEBP;
                }
                else if (len4.SequenceEqual(MultipartObjectCommonFormatByteMark.PDF))
                {
                    return MultipartObjectCommonFormat.PDF;
                }
                else if (len4.SequenceEqual(MultipartObjectCommonFormatByteMark.TIFF))
                {
                    return MultipartObjectCommonFormat.TIFF;
                }
            }
            if (byteLen >= 3)
            {
                Span<byte> len3 = this.ContentBytes.AsSpan(0, 3);

                if (len3.SequenceEqual(MultipartObjectCommonFormatByteMark.JPEG))
                {
                    return MultipartObjectCommonFormat.JPEG;
                }
                else if (len3.SequenceEqual(MultipartObjectCommonFormatByteMark.GIF))
                {
                    return MultipartObjectCommonFormat.GIF;
                }
            }
            if (byteLen >= 2)
            {
                Span<byte> len2 = this.ContentBytes.AsSpan(0, 2);

                if (len2.SequenceEqual(MultipartObjectCommonFormatByteMark.BMP))
                {
                    return MultipartObjectCommonFormat.BMP;
                }
            }

            return MultipartObjectCommonFormat.Unknown;
        }

        internal MultipartObject(NameValueCollection headers, string? filename, string name, byte[]? body, Encoding encoding)
        {
            this.Headers = new HttpHeaderCollection();
            this.Headers.ImportNameValueCollection(headers);
            this.Headers.MakeReadOnly();

            this.Filename = filename;
            this.Name = name;
            this.ContentBytes = body ?? Array.Empty<byte>();
            this.ContentLength = body?.Length ?? 0;
            this._baseEncoding = encoding;
        }

        //
        // we should rewrite it using Spans<>, but there are so many code and it would take
        // days...
        internal static MultipartFormCollection ParseMultipartObjects(HttpRequest req)
        {
            string? contentType = req.Headers[HttpKnownHeaderNames.ContentType];
            if (contentType is null)
            {
                throw new InvalidOperationException(SR.MultipartObject_ContentTypeMissing);
            }

            string[] contentTypePieces = contentType.Split(';');
            string? boundary = null;
            for (int i = 0; i < contentTypePieces.Length; i++)
            {
                string obj = contentTypePieces[i];
                string[] kv = obj.Split("=");
                if (kv.Length != 2)
                { continue; }
                if (kv[0].Trim() == "boundary")
                {
                    boundary = kv[1].Trim();
                }
            }

            if (boundary is null)
            {
                throw new InvalidOperationException(SR.MultipartObject_BoundaryMissing);
            }

            byte[] boundaryBytes = Encoding.UTF8.GetBytes(boundary);

            if (req.baseServer.ServerConfiguration.Flags.EnableNewMultipartFormReader == true)
            {
                MultipartFormReader reader = new MultipartFormReader(req.RawBody, boundaryBytes, req.RequestEncoding, req.baseServer.ServerConfiguration.ThrowExceptions);
                var objects = reader.Read();

                return new MultipartFormCollection(objects);
            }

            /////////
            // https://stackoverflow.com/questions/9755090/split-a-byte-array-at-a-delimiter
            byte[][] Separate(byte[] source, byte[] separator)
            {
                var Parts = new List<byte[]>();
                var Index = 0;
                byte[] Part;
                for (var I = 0; I < source.Length; ++I)
                {
                    if (Equals(source, separator, I))
                    {
                        Part = new byte[I - Index];
                        Array.Copy(source, Index, Part, 0, Part.Length);
                        Parts.Add(Part);
                        Index = I + separator.Length;
                        I += separator.Length - 1;
                    }
                }
                Part = new byte[source.Length - Index];
                Array.Copy(source, Index, Part, 0, Part.Length);
                Parts.Add(Part);
                return Parts.ToArray();
            }

            bool Equals(byte[] source, byte[] separator, int index)
            {
                for (int i = 0; i < separator.Length; ++i)
                    if (index + i >= source.Length || source[index + i] != separator[i])
                        return false;
                return true;
            }
            /////////

            byte[][] matchedResults = Separate(req.RawBody, boundaryBytes);

            List<MultipartObject> outputObjects = new List<MultipartObject>();
            for (int i = 1; i < matchedResults.Length - 1; i++)
            {
                byte[]? contentBytes = null;
                NameValueCollection headers = new NameValueCollection();
                byte[] result = matchedResults[i].ToArray();
                int resultLength = result.Length - 4;
                //string content = Encoding.ASCII.GetString(result);

                int spaceLength = 0;
                bool parsingContent = false;
                bool headerNameParsed = false;
                bool headerValueParsed = false;
                List<byte> headerNameBytes = new();
                List<byte> headerValueBytes = new();

                int headerSize = 0;
                for (int j = 0; j < resultLength; j++)
                {
                    byte J = result[j];
                    if (spaceLength == 2 && headerNameParsed && !headerValueParsed)
                    {
                        string headerName = Encoding.UTF8.GetString(headerNameBytes.ToArray());
                        string headerValue = Encoding.UTF8.GetString(headerValueBytes.ToArray());

                        headers.Add(headerName, headerValue.Trim());
                        headerNameParsed = false;
                        headerValueParsed = false;
                    }
                    else if (spaceLength == 4 && !parsingContent)
                    {
                        headerSize = j;
                        contentBytes = new byte[resultLength - headerSize];
                        parsingContent = true;
                    }
                    if ((J == 0x0A || J == 0x0D) && !parsingContent)
                    {
                        spaceLength++;
                        continue;
                    }
                    else
                    {
                        spaceLength = 0;
                    }
                    if (!parsingContent)
                    {
                        if (!headerNameParsed)
                        {
                            if (J == 58)
                            {
                                headerNameParsed = true;
                            }
                            else
                            {
                                headerNameBytes.Add(J);
                            }
                        }
                        else if (!headerValueParsed)
                        {
                            headerValueBytes.Add(J);
                        }
                    }
                    else
                    {
                        contentBytes![j - headerSize] = J;
                    }
                }

                // parse field name
                string[] val = headers[HttpKnownHeaderNames.ContentDisposition]?.Split(';') ?? Array.Empty<string>();
                string? fieldName = null;
                string? fieldFilename = null;

                for (int k = 0; k < val.Length; k++)
                {
                    string valueAttribute = val[k];
                    string[] valAttributeParts = valueAttribute.Trim().Split("=");
                    if (valAttributeParts.Length != 2)
                        continue;
                    if (valAttributeParts[0] == "name")
                    {
                        fieldName = valAttributeParts[1].Trim('"');
                    }
                    else if (valAttributeParts[0] == "filename")
                    {
                        fieldFilename = valAttributeParts[1].Trim('"');
                    }
                }

                if (fieldName is null)
                {
                    throw new InvalidOperationException(string.Format(SR.MultipartObject_EmptyFieldName, i));
                }

                MultipartObject newObject = new MultipartObject(headers, fieldFilename, fieldName, contentBytes?.ToArray(), req.RequestEncoding);
                outputObjects.Add(newObject);
            }

            return new MultipartFormCollection(outputObjects);
        }
    }
}
