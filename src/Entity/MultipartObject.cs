using Sisk.Core.Http;
using System.Collections.Specialized;
using System.Text;

namespace Sisk.Core.Entity
{
    /// <summary>
    /// Represents an multipart/form-data object.
    /// </summary>
    /// <definition>
    /// public class MultipartObject
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Entity
    /// </namespace>
    public class MultipartObject
    {
        /// <summary>
        /// The multipart form data object headers.
        /// </summary>
        /// <definition>
        /// public NameValueCollection Headers { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public NameValueCollection Headers { get; private set; }

        /// <summary>
        /// The name of the file provided by Multipart form data. Null is returned if the object is not a file.
        /// </summary>
        /// <definition>
        /// public string? Filename { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string? Filename { get; private set; }

        /// <summary>
        /// The multipart form data object field name.
        /// </summary>
        /// <definition>
        /// public string Name { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string Name { get; private set; }

        /// <summary>
        /// The multipart form data content bytes.
        /// </summary>
        /// <definition>
        /// public byte[] ContentBytes { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public byte[] ContentBytes { get; private set; }

        /// <summary>
        /// The multipart form data content length.
        /// </summary>
        /// <definition>
        /// public int ContentLength { get; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public int ContentLength { get; private set; }

        /// <summary>
        /// Reads the content bytes with the given encoder.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string? ReadContentAsString(Encoding encoder)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string? ReadContentAsString(Encoding encoder)
        {
            if (ContentLength == 0)
                return null;
            return encoder.GetString(ContentBytes);
        }

        /// <summary>
        /// Reads the content bytes as an ASCII string.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public string? ReadContentAsString()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public string? ReadContentAsString()
        {
            return ReadContentAsString(Encoding.ASCII);
        }

        /// <summary>
        /// Determine the image format based in the file header for each image content type.
        /// </summary>
        /// <returns></returns>
        /// <definition>
        /// public MultipartObjectImageFormat GetImageFormat()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        public MultipartObjectImageFormat GetImageFormat()
        {
            IEnumerable<byte> len8 = ContentBytes.Take(8);
            IEnumerable<byte> len4 = ContentBytes.Take(4);
            IEnumerable<byte> len3 = ContentBytes.Take(3);

            if (len8.SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }))
            {
                return MultipartObjectImageFormat.PNG;
            }
            else if (len3.SequenceEqual(new byte[] { 0xFF, 0xD8, 0xFF }))
            {
                return MultipartObjectImageFormat.JPEG;
            }
            else if (len3.SequenceEqual(new byte[] { 73, 73, 42 }))
            {
                return MultipartObjectImageFormat.TIFF;
            }
            else if (len3.SequenceEqual(new byte[] { 77, 77, 42 }))
            {
                return MultipartObjectImageFormat.TIFF;
            }
            else if (len3.SequenceEqual(new byte[] { 0x42, 0x4D }))
            {
                return MultipartObjectImageFormat.BMP;
            }
            else if (len3.SequenceEqual(new byte[] { 0x47, 0x46, 0x49 }))
            {
                return MultipartObjectImageFormat.GIF;
            }
            else if (len4.SequenceEqual(new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F' }))
            {
                return MultipartObjectImageFormat.WEBP;
            }
            else
            {
                return MultipartObjectImageFormat.Unknown;
            }
        }

        internal MultipartObject(NameValueCollection headers, string? filename, string name, byte[]? body)
        {
            Headers = headers;
            Filename = filename;
            Name = name;
            ContentBytes = body ?? new byte[] { };
            ContentLength = body?.Length ?? 0;
        }

        internal static MultipartObject[] ParseMultipartObjects(HttpRequest req)
        {
            string? contentType = req.Headers["Content-Type"];
            if (contentType is null)
            {
                throw new InvalidOperationException("Content-Type header cannot be null when retriving a multipart form content");
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
                throw new InvalidOperationException("No boundary was specified for this multipart form content.");
            }

            byte[] bodyBytes = req.RawBody.Skip(2).ToArray();
            byte[] boundaryBytes = Encoding.ASCII.GetBytes(boundary);
            long contentLength = bodyBytes.Length;

            int boundaryLength = boundaryBytes.Length;

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

            byte[] matchingContent = new byte[0xFFFFFF];
            byte[][] matchedResults = Separate(req.RawBody, boundaryBytes);

            List<MultipartObject> outputObjects = new List<MultipartObject>();
            for (int i = 1; i < matchedResults.Length - 1; i++)
            {
                byte[]? contentBytes = null;
                NameValueCollection headers = new NameValueCollection();
                byte[] result = matchedResults[i].ToArray();
                int resultLength = result.Length - 4;
                string content = Encoding.ASCII.GetString(result);

                int spaceLength = 0;
                bool parsingContent = false;
                bool headerNameParsed = false;
                bool headerValueParsed = false;
                string headerName = "";
                string headerValue = "";

                int headerSize = 0;
                for (int j = 0; j < resultLength; j++)
                {
                    byte J = result[j];
                    if (spaceLength == 2 && headerNameParsed && !headerValueParsed)
                    {
                        headerValueParsed = true;
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
                                headerName += (char)J;
                            }
                        }
                        else if (!headerValueParsed)
                        {
                            headerValue += (char)J;
                        }
                    }
                    else
                    {
                        contentBytes![j - headerSize] = J;
                    }
                }

                // parse field name
                string[] val = headers["Content-Disposition"]?.Split(';') ?? new string[] { };
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
                    throw new InvalidOperationException($"Content-part object position {i} cannot have an empty field name.");
                }

                MultipartObject newObject = new MultipartObject(headers, fieldFilename, fieldName, contentBytes?.ToArray());
                outputObjects.Add(newObject);
            }

            return outputObjects.ToArray();
        }
    }

    /// <summary>
    /// Represents an image format for Multipart objects.
    /// </summary>
    /// <definition>
    /// public enum MultipartObjectImageFormat
    /// </definition>
    /// <type>
    /// Enum
    /// </type>
    /// <namespace>
    /// Sisk.Core.Entity
    /// </namespace>
    public enum MultipartObjectImageFormat
    {
        /// <summary>
        /// Represents that the object is not a recognized image.
        /// </summary>
        /// <definition>
        /// Unknown = 0
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        Unknown = 0,

        /// <summary>
        /// Represents an JPEG/JPG image.
        /// </summary>
        /// <definition>
        /// JPEG = 100
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        JPEG = 100,

        /// <summary>
        /// Represents an GIF image.
        /// </summary>
        /// <definition>
        /// GIF = 101
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        GIF = 101,

        /// <summary>
        /// Represents an PNG image.
        /// </summary>
        /// <definition>
        /// PNG = 102
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        PNG = 102,

        /// <summary>
        /// Represents an TIFF image.
        /// </summary>
        /// <definition>
        /// TIFF = 103
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        TIFF = 103,

        /// <summary>
        /// Represents an bitmap image.
        /// </summary>
        /// <definition>
        /// BMP = 104
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        BMP = 104,

        /// <summary>
        /// Represents an WebP image.
        /// </summary>
        /// <definition>
        /// WEBP = 105
        /// </definition>
        /// <type>
        /// Enum Value
        /// </type>
        /// <namespace>
        /// Sisk.Core.Entity
        /// </namespace>
        WEBP = 105
    }
}
