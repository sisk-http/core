using Sisk.Core.Http;
using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal
{
    internal class LoggingFormatter
    {
        TimeSpan currentTimezoneDiff = TimeZoneInfo.Local.GetUtcOffset(DateTime.Now);

        HttpServerExecutionResult? res;
        DateTime d;
        Uri? bReqUri;
        IPAddress? bReqIpAddr;
        NameValueCollection? reqHeaders;
        int bResStatusCode;
        string? bResStatusDescr;
        float? incomingSize;
        float? outcomingSize;
        long execTime;

        public LoggingFormatter(
            HttpServerExecutionResult? res,
            DateTime d, Uri? bReqUri,
            IPAddress? bReqIpAddr,
            NameValueCollection? reqHeaders,
            int bResStatusCode,
            string? bResStatusDescr,
            float? incomingSize,
            float? outcomingSize,
            long execTime)
        {
            this.res = res;
            this.d = d;
            this.bReqUri = bReqUri;
            this.bReqIpAddr = bReqIpAddr;
            this.reqHeaders = reqHeaders;
            this.bResStatusCode = bResStatusCode;
            this.bResStatusDescr = bResStatusDescr;
            this.incomingSize = incomingSize;
            this.outcomingSize = outcomingSize;
            this.execTime = execTime;
        }

        private static string? dd(LoggingFormatter lc) => $"{lc.d.Day:D2}";
        private static string? dmmm(LoggingFormatter lc) => $"{lc.d:MMMM}";
        private static string? dmm(LoggingFormatter lc) => $"{lc.d:MMM}";
        private static string? dm(LoggingFormatter lc) => $"{lc.d.Month:D2}";
        private static string? dy(LoggingFormatter lc) => $"{lc.d.Year:D4}";
        private static string? th(LoggingFormatter lc) => $"{lc.d:hh}";
        private static string? tH(LoggingFormatter lc) => $"{lc.d:HH}";
        private static string? ti(LoggingFormatter lc) => $"{lc.d.Minute:D2}";
        private static string? ts(LoggingFormatter lc) => $"{lc.d.Second:D2}";
        private static string? tm(LoggingFormatter lc) => $"{lc.d.Millisecond:D3}";
        private static string? tz(LoggingFormatter lc) => $"{lc.currentTimezoneDiff.TotalHours:00}00";
        private static string? ri(LoggingFormatter lc) => lc.bReqIpAddr?.ToString();
        private static string? rs(LoggingFormatter lc) => lc.bReqUri?.Scheme;
        private static string? ra(LoggingFormatter lc) => lc.bReqUri?.Authority;
        private static string? rh(LoggingFormatter lc) => lc.bReqUri?.Host;
        private static string? rp(LoggingFormatter lc) => lc.bReqUri?.Port.ToString();
        private static string? rz(LoggingFormatter lc) => lc.bReqUri?.AbsolutePath ?? "/";
        private static string? rq(LoggingFormatter lc) => lc.bReqUri?.Query;
        private static string? sc(LoggingFormatter lc) => lc.bResStatusCode.ToString();
        private static string? sd(LoggingFormatter lc) => lc.bResStatusDescr;
        private static string? lin(LoggingFormatter lc) => HttpServer.HumanReadableSize(lc.incomingSize);
        private static string? lou(LoggingFormatter lc) => HttpServer.HumanReadableSize(lc.outcomingSize);
        private static string? lms(LoggingFormatter lc) => lc.execTime.ToString();
        private static string? ls(LoggingFormatter lc) => lc.res?.Status.ToString();

        private static MethodInfo[] Callers = typeof(LoggingFormatter).GetMethods(BindingFlags.Static | BindingFlags.NonPublic);

        private void replaceEntities(ref string format)
        {
            foreach (var m in Callers)
            {
                string literal = "%" + m.Name;
                if (format.Contains(literal))
                {
                    string? invokeResult = (string?)m.Invoke(null, new object?[] { this });
                    format = format.Replace(literal, invokeResult);
                }
            }
        }

        private void replaceHeaders(ref string format)
        {
            int pos = 0;
            while ((pos = format.IndexOf("%{")) >= 0)
            {
                int end = format.IndexOf('}');
                string headerName = format.Substring(pos + 2, end - pos - 2);
                string? headerValue = reqHeaders?[headerName];
                format = format.Replace($"%{{{headerName}}}", headerValue);
            }
        }

        public void Format(ref string format)
        {
            replaceHeaders(ref format);
            replaceEntities(ref format);
        }
    }
}
