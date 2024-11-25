using System.Text;

namespace Sisk.ManagedHttpListener.HttpSerializer;

internal static class HttpResponseSerializer
{
    public static bool TryWriteHttp1Response(
        Stream outgoingStream,
        int statusCode,
        string statusDescription,
        List<(string, string)> headers)
    {
        try
        {
            using var sw = new StreamWriter(outgoingStream, Encoding.Latin1, leaveOpen: true) { NewLine = "\r\n" };
            sw.WriteLine($"HTTP/1.1 {statusCode} {statusDescription}");

            for (int i = 0; i < headers.Count; i++)
            {
                (string name, string value) header = headers[i];
                sw.WriteLine($"{header.name}: {header.value}");
            }

            sw.WriteLine();
            return true;
        }
        catch (Exception ex)
        {
            Logger.LogInformation($"Couldn't write HTTP response to {outgoingStream.GetType().Name}: {ex.Message}");
            return false;
        }
    }
}
