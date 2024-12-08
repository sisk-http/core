using System.Diagnostics.CodeAnalysis;
using System.Text;

namespace Sisk.ManagedHttpListener.HttpSerializer;

internal static class HttpRequestSerializer
{
    static ReadOnlySpan<byte> ReadUntil(Span<byte> readBuffer, Streams.HttpBufferedReadStream bufferStream, byte intercept, out bool found)
    {
        int accumulatedPosition = (int)bufferStream.Position;
        while (bufferStream.Read(readBuffer) > 0)
        {
            int interceptPosition = bufferStream.BufferedBytes[accumulatedPosition..].IndexOf(intercept);
            if (interceptPosition >= 0)
            {
                // trim buffer stream
                bufferStream.Position = accumulatedPosition + interceptPosition;
                found = true;
                return readBuffer[0..interceptPosition];
            }
            else
            {
                accumulatedPosition += readBuffer.Length;
            }
        }

        found = false;
        return Array.Empty<byte>();
    }

    public static bool TryReadHttp1Request(
                            Streams.HttpBufferedReadStream inboundStream,
                     scoped Span<byte> lineMemory,
        [NotNullWhen(true)] out string? method,
        [NotNullWhen(true)] out string? path,
                            out long contentLength,
                            out int messageEnd,
                            out List<(string, string)> headers,
                            out bool expectContinue)
    {
        contentLength = 0;
        messageEnd = 0;
        expectContinue = false;

        //try
        //{
        ReadOnlySpan<byte> _requestLine = ReadUntil(lineMemory, inboundStream, Constants.CH_RETURN, out bool _foundRequestLine);
        if (_requestLine.Length == 0 || !_foundRequestLine)
            goto ret;

        ReadOnlySpan<char> _requestLineString = Encoding.Latin1.GetString(_requestLine);
        Span<Range> _requestLineParts = stackalloc Range[3];

        if (MemoryExtensions.Split(_requestLineString, _requestLineParts, ' ') != 3)
        {
            goto ret;
        }

        method = new string(_requestLineString[_requestLineParts[0]]);
        path = new string(_requestLineString[_requestLineParts[1]]);

        inboundStream.Position = _requestLine.Length + 2; // +2 includes \r\n

        List<(string, string)> headerList = new List<(string, string)>(Constants.HEADER_LINE_ALLOCATION);
        while (inboundStream.CanRead)
        {
            ReadOnlySpan<byte> _headerLine = ReadUntil(lineMemory, inboundStream, Constants.CH_RETURN, out bool _foundHeaderDiv);
            if (_headerLine.Length == 0)
            {
                if (_foundHeaderDiv)
                {
                    inboundStream.Position += 2; // \r\n
                    break;
                }
                else
                {
                    goto ret;
                }
            }

            inboundStream.Position += 2; // \r\n

            ReadOnlySpan<char> _headerLineString = Encoding.Latin1.GetString(_headerLine);
            int separatorIndex = _headerLineString.IndexOf(':');
            if (separatorIndex == -1)
                break;

            ReadOnlySpan<char> hName = _headerLineString[0..separatorIndex];
            ReadOnlySpan<char> hValue = _headerLineString[(separatorIndex + 1)..];

            if (hName.CompareTo("Content-Length", StringComparison.OrdinalIgnoreCase) == 0)
            {
                contentLength = long.Parse(hValue);
            }
            else if ((hName.CompareTo("Expect", StringComparison.OrdinalIgnoreCase) == 0 && hValue == "100-continue"))
            {
                expectContinue = true;
            }

            headerList.Add((new string(hName), new string(hValue)));
        }

        messageEnd = (int)inboundStream.Position;
        headers = headerList;
        return true;
    //}
    //catch (Exception ex)
    //{
    //    Logger.LogInformation($"Couldn't read HTTP request from {inboundStream.GetType().Name}: {ex.Message}");
    //}

    ret:
        method = null;
        path = null;
        headers = new();
        return false;
    }
}
