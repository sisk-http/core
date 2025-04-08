// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   MultipartObject.cs
// Repository:  https://github.com/sisk-http/core

using System.Collections.Specialized;
using System.Text;
using Sisk.Core.Http;

namespace Sisk.Core.Entity {
    /// <summary>
    /// Represents an multipart/form-data object.
    /// </summary>
    public sealed class MultipartObject : IEquatable<MultipartObject> {
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
        public byte [] ContentBytes { get; private set; }

        /// <summary>
        /// Gets this <see cref="MultipartObject"/> form data content length in byte count.
        /// </summary>
        public int ContentLength { get => ContentBytes.Length; }

        /// <summary>
        /// Gets an boolean indicating if this <see cref="MultipartObject"/> has contents or not.
        /// </summary>
        public bool HasContents { get => ContentLength > 0; }

        /// <summary>
        /// Gets an boolean indicating if this <see cref="MultipartObject"/> is a file or not.
        /// </summary>
        public bool IsFile { get => HasContents && !string.IsNullOrEmpty ( Filename ); }

        /// <summary>
        /// Reads the content bytes with the given encoder.
        /// </summary>
        public string ReadContentAsString ( Encoding encoder ) {
            if (ContentLength == 0)
                return string.Empty;
            return encoder.GetString ( ContentBytes );
        }

        /// <summary>
        /// Reads the content bytes using the HTTP request content-encoding.
        /// </summary>
        public string ReadContentAsString () {
            return ReadContentAsString ( _baseEncoding );
        }

        /// <summary>
        /// Determines the image format based in the file header for each image content type.
        /// </summary>
        public MultipartObjectCommonFormat GetCommonFileFormat () {
            int byteLen = ContentBytes.Length;

            if (byteLen >= 8) {
                Span<byte> len8 = ContentBytes.AsSpan ( 0, 8 );

                if (len8.SequenceEqual ( MultipartObjectCommonFormatByteMark.PNG )) {
                    return MultipartObjectCommonFormat.PNG;
                }
            }
            if (byteLen >= 4) {
                Span<byte> len4 = ContentBytes.AsSpan ( 0, 4 );

                if (len4.SequenceEqual ( MultipartObjectCommonFormatByteMark.WEBP )) {
                    return MultipartObjectCommonFormat.WEBP;
                }
                else if (len4.SequenceEqual ( MultipartObjectCommonFormatByteMark.PDF )) {
                    return MultipartObjectCommonFormat.PDF;
                }
                else if (len4.SequenceEqual ( MultipartObjectCommonFormatByteMark.TIFF )) {
                    return MultipartObjectCommonFormat.TIFF;
                }
            }
            if (byteLen >= 3) {
                Span<byte> len3 = ContentBytes.AsSpan ( 0, 3 );

                if (len3.SequenceEqual ( MultipartObjectCommonFormatByteMark.JPEG )) {
                    return MultipartObjectCommonFormat.JPEG;
                }
                else if (len3.SequenceEqual ( MultipartObjectCommonFormatByteMark.GIF )) {
                    return MultipartObjectCommonFormat.GIF;
                }
            }
            if (byteLen >= 2) {
                Span<byte> len2 = ContentBytes.AsSpan ( 0, 2 );

                if (len2.SequenceEqual ( MultipartObjectCommonFormatByteMark.BMP )) {
                    return MultipartObjectCommonFormat.BMP;
                }
            }

            return MultipartObjectCommonFormat.Unknown;
        }

        internal MultipartObject ( NameValueCollection headers, string? filename, string name, byte []? body, Encoding encoding ) {
            Headers = new HttpHeaderCollection ();
            Headers.ImportNameValueCollection ( headers );
            Headers.MakeReadOnly ();

            Filename = filename;
            Name = name;
            ContentBytes = body ?? Array.Empty<byte> ();
            _baseEncoding = encoding;
        }

        internal static MultipartFormCollection ParseMultipartObjects ( HttpRequest req, byte [] body, CancellationToken cancellation = default ) {
            if (body.Length == 0)
                return new MultipartFormCollection ( Enumerable.Empty<MultipartObject> () );

            string? contentType = req.Headers [ HttpKnownHeaderNames.ContentType ]
                ?? throw new InvalidOperationException ( SR.MultipartObject_ContentTypeMissing );

            string [] contentTypePieces = contentType.Split ( ';' );
            string? boundary = null;
            for (int i = 0; i < contentTypePieces.Length; i++) {
                cancellation.ThrowIfCancellationRequested ();
                string obj = contentTypePieces [ i ];
                string [] kv = obj.Split ( "=" );
                if (kv.Length != 2) { continue; }
                if (kv [ 0 ].Trim () == "boundary") {
                    boundary = kv [ 1 ].Trim ();
                }
            }

            if (boundary is null) {
                throw new InvalidOperationException ( SR.MultipartObject_BoundaryMissing );
            }

            byte [] boundaryBytes = req.RequestEncoding.GetBytes ( boundary );
            MultipartFormReader reader = new MultipartFormReader ( body, boundaryBytes, req.RequestEncoding, req.baseServer.ServerConfiguration.ThrowExceptions );
            var objects = reader.Read ( cancellation );

            return new MultipartFormCollection ( objects );
        }

        /// <inheritdoc/>
        public override int GetHashCode () {
            return HashCode.Combine ( Name.GetHashCode (), ContentLength.GetHashCode (), Filename?.GetHashCode () ?? 0 );
        }

        /// <inheritdoc/>
        public override bool Equals ( object? obj ) {
            if (obj is MultipartObject mo)
                return Equals ( mo );
            return false;
        }

        /// <inheritdoc/>
        public bool Equals ( MultipartObject? other ) {
            return GetHashCode () == other?.GetHashCode ();
        }
    }
}
