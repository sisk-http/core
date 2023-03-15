using System.Net;

namespace Sisk.Core.Http
{
    /// <summary>
    /// An <see cref="HttpRequestEventSource"/> instance opens a persistent connection to the request, which sends events in text/event-stream format.
    /// </summary>
    /// <definition>
    /// public class HttpRequestEventSource : IDisposable
    /// </definition>
    /// <type>
    /// Class
    /// </type>
    /// <namespace>
    /// Sisk.Core.Http
    /// </namespace>
    public class HttpRequestEventSource : IDisposable
    {
        private HttpListenerResponse res;
        private HttpListenerRequest req;

        private List<string> sendQueue = new List<string>();
        private int failedSentResponses = 0;
        private bool isClosed = false;
        internal int Length = 0;
        bool hasSentData = false;

        /// <summary>
        /// Maximum attempts to resend a message if it fails. Leave it as -1 to never stop trying to resent failed messages.
        /// </summary>
        /// <definition>
        /// public int MaximumErrorAttempts { get; set; }
        /// </definition>
        /// <type>
        /// Property
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public int MaximumErrorAttempts { get; set; } = 3;

        internal HttpRequestEventSource(HttpListenerResponse res, HttpListenerRequest req, HttpRequest host)
        {
            this.res = res ?? throw new ArgumentNullException(nameof(res));
            this.req = req ?? throw new ArgumentNullException(nameof(req));
            res.AddHeader("Cache-Control", "no-store");
            res.AddHeader("Content-Type", "text/event-stream");
            res.AddHeader("X-Powered-By", HttpServer.poweredByHeader);
            HttpServer.SetCorsHeaders(host.hostContext.CrossOriginResourceSharingPolicy, res);
        }

        /// <summary>
        /// Sends an header to the streaming context.
        /// </summary>
        /// <param name="name">The header name.</param>
        /// <param name="value">The header value.</param>
        /// <definition>
        /// public void AppendHeader(string name, string value)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void AppendHeader(string name, string value)
        {
            if (hasSentData)
            {
                throw new InvalidOperationException("You cannot set headers after a message has been sent in this context.");
            }
            this.res.AddHeader(name, value);
        }

        /// <summary>
        /// Writes a event message with their data to the event listener.
        /// </summary>
        /// <param name="data">The message text.</param>
        /// <definition>
        /// public void Send(string data)
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Send(string data)
        {
            hasSentData = true;
            sendQueue.Add($"data: {data}\n\n");
            Flush();
        }

        /// <summary>
        /// Closes the event listener and it's connection.
        /// </summary>
        /// <definition>
        /// public HttpResponse Close()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public HttpResponse Close()
        {
            isClosed = true;
            Flush();
            return new HttpResponse(HttpResponse.HTTPRESPONSE_EVENTSOURCE_CLOSE)
            {
                CalculedLength = Length
            };
        }

        /// <summary>
        /// Cancels the sending queue from sending pending messages and clears the queue.
        /// </summary>
        /// <definition>
        /// public void Cancel()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Cancel()
        {
            sendQueue.Clear();
        }

        private void Flush()
        {
            for (int i = 0; i < sendQueue.Count; i++)
            {
                if (isClosed)
                {
                    return;
                }
                string item = sendQueue[i];
                byte[] itemBytes = req.ContentEncoding.GetBytes(item);
                try
                {
                    res.OutputStream.Write(itemBytes);
                    Length += itemBytes.Length;
                    sendQueue.RemoveAt(0);
                }
                catch (Exception)
                {
                    if (MaximumErrorAttempts >= 0 && failedSentResponses >= MaximumErrorAttempts)
                    {
                        throw new Exception($"More than {failedSentResponses} error attempts exceeded.");
                    }
                    failedSentResponses++;
                    Flush();
                }
            }
        }

        /// <summary>
        /// Flushes and releases the used resources of this class instance.
        /// </summary>
        /// <definition>
        /// public void Dispose()
        /// </definition>
        /// <type>
        /// Method
        /// </type>
        /// <namespace>
        /// Sisk.Core.Http
        /// </namespace>
        public void Dispose()
        {
            Close();
            sendQueue.Clear();
        }
    }
}
