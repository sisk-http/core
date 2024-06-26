﻿// The Sisk Framework source code
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
    public class MultipartObject
    {
        private readonly Encoding _baseEncoding;

        /// <summary>
        /// The multipart form data object headers.
        /// </summary>
        public NameValueCollection Headers { get; private set; }

        /// <summary>
        /// The name of the file provided by Multipart form data. Null is returned if the object is not a file.
        /// </summary>
        public string? Filename { get; private set; }

        /// <summary>
        /// The multipart form data object field name.
        /// </summary>
        public string Name { get; private set; }

        /// <summary>
        /// The multipart form data content bytes.
        /// </summary>
        public byte[] ContentBytes { get; private set; }

        /// <summary>
        /// The multipart form data content length.
        /// </summary>
        public int ContentLength { get; private set; }

        /// <summary>
        /// Reads the content bytes with the given encoder.
        /// </summary>
        public string? ReadContentAsString(Encoding encoder)
        {
            if (ContentLength == 0)
                return null;
            return encoder.GetString(ContentBytes);
        }

        /// <summary>
        /// Reads the content bytes using the Http request content-encoding.
        /// </summary>
        public string? ReadContentAsString()
        {
            return ReadContentAsString(_baseEncoding);
        }

        /// <summary>
        /// Determine the image format based in the file header for each image content type.
        /// </summary>
        public MultipartObjectCommonFormat GetCommonFileFormat()
        {
            IEnumerable<byte> len8 = ContentBytes.Take(8);
            IEnumerable<byte> len4 = ContentBytes.Take(4);
            IEnumerable<byte> len3 = ContentBytes.Take(3);

            if (len8.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            {
                return MultipartObjectCommonFormat.PNG;
            }
            else if (len4.SequenceEqual(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }))
            {
                return MultipartObjectCommonFormat.WEBP;
            }
            else if (len4.SequenceEqual(new byte[] { 0x25, 0x50, 0x44, 0x46 }))
            {
                return MultipartObjectCommonFormat.PDF;
            }
            else if (len3.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
            {
                return MultipartObjectCommonFormat.JPEG;
            }
            else if (len3.SequenceEqual(new byte[] { 73, 73, 42 }))
            {
                return MultipartObjectCommonFormat.TIFF;
            }
            else if (len3.SequenceEqual(new byte[] { 77, 77, 42 }))
            {
                return MultipartObjectCommonFormat.TIFF;
            }
            else if (len3.SequenceEqual(new byte[] { 0x42, 0x4D }))
            {
                return MultipartObjectCommonFormat.BMP;
            }
            else if (len3.SequenceEqual(new byte[] { 0x47, 0x46, 0x49 }))
            {
                return MultipartObjectCommonFormat.GIF;
            }
            else
            {
                return MultipartObjectCommonFormat.Unknown;
            }
        }

        internal MultipartObject(NameValueCollection headers, string? filename, string name, byte[]? body, Encoding encoding)
        {
            Headers = headers;
            Filename = filename;
            Name = name;
            ContentBytes = body ?? Array.Empty<byte>();
            ContentLength = body?.Length ?? 0;
            _baseEncoding = encoding;
        }

        internal static MultipartFormCollection ParseMultipartObjects(HttpRequest req)
        {
            string? contentType = req.Headers["Content-Type"];
            if (contentType is null)
            {
                throw new InvalidOperationException(SR.MultipartObject_ContentTypeMissing);
            }

            string[] contentTypePieces = contentType.Split(';');
            string? boundary = null;
            foreach (string obj in contentTypePieces)
            {
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
                string[] val = headers["Content-Disposition"]?.Split(';') ?? Array.Empty<string>();
                string? fieldName = null;
                string? fieldFilename = null;

                foreach (string valueAttribute in val)
                {
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

                if (fieldName == null)
                {
                    throw new InvalidOperationException(string.Format(SR.MultipartObject_EmptyFieldName, i));
                }

                MultipartObject newObject = new MultipartObject(headers, fieldFilename, fieldName, contentBytes?.ToArray(), req.RequestEncoding);
                outputObjects.Add(newObject);
            }

            return new MultipartFormCollection(outputObjects);
        }
    }

    /// <summary>
    /// Represents an image format for Multipart objects.
    /// </summary>
    public enum MultipartObjectCommonFormat
    {
        /// <summary>
        /// Represents that the object is not a recognized image.
        /// </summary>
        Unknown = 0,

        /// <summary>
        /// Represents an JPEG/JPG image.
        /// </summary>
        JPEG = 100,

        /// <summary>
        /// Represents an GIF image.
        /// </summary>
        GIF = 101,

        /// <summary>
        /// Represents an PNG image.
        /// </summary>
        PNG = 102,

        /// <summary>
        /// Represents an TIFF image.
        /// </summary>
        TIFF = 103,

        /// <summary>
        /// Represents an bitmap image.
        /// </summary>
        BMP = 104,

        /// <summary>
        /// Represents an WebP image.
        /// </summary>
        WEBP = 105,

        /// <summary>
        /// Represents an PDF file.
        /// </summary>
        PDF = 200
    }
}
