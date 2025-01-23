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
        public int ContentLength { get => this.ContentBytes.Length; }

        /// <summary>
        /// Gets an booolean indicating if this <see cref="MultipartObject"/> has contents or not.
        /// </summary>
        public bool HasContents { get => this.ContentLength > 0; }

        /// <summary>
        /// Reads the content bytes with the given encoder.
        /// </summary>
        public string ReadContentAsString ( Encoding encoder ) {
            if (this.ContentLength == 0)
                return string.Empty;
            return encoder.GetString ( this.ContentBytes );
        }

        /// <summary>
        /// Reads the content bytes using the HTTP request content-encoding.
        /// </summary>
        public string ReadContentAsString () {
            return this.ReadContentAsString ( this._baseEncoding );
        }

        /// <summary>
        /// Determines the image format based in the file header for each image content type.
        /// </summary>
        public MultipartObjectCommonFormat GetCommonFileFormat () {
            int byteLen = this.ContentBytes.Length;

            if (byteLen >= 8) {
                Span<byte> len8 = this.ContentBytes.AsSpan ( 0, 8 );

                if (len8.SequenceEqual ( MultipartObjectCommonFormatByteMark.PNG )) {
                    return MultipartObjectCommonFormat.PNG;
                }
            }
            if (byteLen >= 4) {
                Span<byte> len4 = this.ContentBytes.AsSpan ( 0, 4 );

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
                Span<byte> len3 = this.ContentBytes.AsSpan ( 0, 3 );

                if (len3.SequenceEqual ( MultipartObjectCommonFormatByteMark.JPEG )) {
                    return MultipartObjectCommonFormat.JPEG;
                }
                else if (len3.SequenceEqual ( MultipartObjectCommonFormatByteMark.GIF )) {
                    return MultipartObjectCommonFormat.GIF;
                }
            }
            if (byteLen >= 2) {
                Span<byte> len2 = this.ContentBytes.AsSpan ( 0, 2 );

                if (len2.SequenceEqual ( MultipartObjectCommonFormatByteMark.BMP )) {
                    return MultipartObjectCommonFormat.BMP;
                }
            }

            return MultipartObjectCommonFormat.Unknown;
        }

        internal MultipartObject ( NameValueCollection headers, string? filename, string name, byte []? body, Encoding encoding ) {
            this.Headers = new HttpHeaderCollection ();
            this.Headers.ImportNameValueCollection ( headers );
            this.Headers.MakeReadOnly ();

            this.Filename = filename;
            this.Name = name;
            this.ContentBytes = body ?? Array.Empty<byte> ();
            this._baseEncoding = encoding;
        }

        //
        // we should rewrite it using Spans<>, but there are so many code and it would take
        // days...
        // edit: we did it! but we'll mantain this method for at least one year
        internal static MultipartFormCollection ParseMultipartObjects ( HttpRequest req ) {
            string? contentType = req.Headers [ HttpKnownHeaderNames.ContentType ]
                ?? throw new InvalidOperationException ( SR.MultipartObject_ContentTypeMissing );

            string [] contentTypePieces = contentType.Split ( ';' );
            string? boundary = null;
            for (int i = 0; i < contentTypePieces.Length; i++) {
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
            MultipartFormReader reader = new MultipartFormReader ( req.RawBody, boundaryBytes, req.RequestEncoding, req.baseServer.ServerConfiguration.ThrowExceptions );
            var objects = reader.Read ();

            return new MultipartFormCollection ( objects );
        }

        /// <inheritdoc/>
        public override int GetHashCode () {
            return HashCode.Combine ( this.Name.GetHashCode (), this.ContentLength.GetHashCode (), this.Filename?.GetHashCode () ?? 0 );
        }

        /// <inheritdoc/>
        public override bool Equals ( object? obj ) {
            if (obj is MultipartObject mo)
                return this.Equals ( mo );
            return false;
        }

        /// <inheritdoc/>
        public bool Equals ( MultipartObject? other ) {
            return this.GetHashCode () == other?.GetHashCode ();
        }
    }
}
