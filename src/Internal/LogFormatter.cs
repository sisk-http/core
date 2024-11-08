// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogFormatter.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Helpers;
using Sisk.Core.Http;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.RegularExpressions;

namespace Sisk.Core.Internal;

internal partial class LogFormatter
{
    public static string FormatExceptionEntr(HttpServerExecutionResult executionResult)
    {
        StringBuilder errLineBuilder = new StringBuilder(128);
        errLineBuilder.Append('[');
        errLineBuilder.Append(executionResult.Request.RequestedAt.ToString("G"));
        errLineBuilder.Append(']');
        errLineBuilder.Append(' ');
        errLineBuilder.Append(executionResult.Request.Method.Method);
        errLineBuilder.Append(' ');
        errLineBuilder.Append(executionResult.Request.FullPath);
        errLineBuilder.AppendLine();
        errLineBuilder.AppendLine(executionResult.ServerException?.ToString());
        if (executionResult.ServerException?.InnerException is { } iex)
        {
            errLineBuilder.AppendLine("[inner exception]");
            errLineBuilder.AppendLine(iex.ToString());
        }

        errLineBuilder.AppendLine();
        return errLineBuilder.ToString();
    }

    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    [GeneratedRegex(@"%([a-z]+|\{[^\}]+\})", RegexOptions.IgnoreCase)]
    internal static partial Regex EntryMatchRegex();

    public static string FormatAccessLogEntry(in string format, HttpServerExecutionResult executionResult)
    {
        ReadOnlySpan<char> formatSpan = format;
        StringBuilder sb = new StringBuilder(format.Length);
        MatchCollection matches = EntryMatchRegex().Matches(format);

        int lastIndex = 0;
        for (int i = 0; i < matches.Count; i++)
        {
            Match match = matches[i];
            string term = match.Groups[1].Value;
            string? result;

            if (term is ['{', .., '}'])
            {
                result = executionResult.Request.Headers[term[1..^1]];
            }
            else
            {
                result = term switch
                {
                    "dd" => executionResult.Request.RequestedAt.Day.ToString("D2"),
                    "dmmm" => executionResult.Request.RequestedAt.ToString("MMMM"),
                    "dmm" => executionResult.Request.RequestedAt.ToString("MMM"),
                    "dm" => executionResult.Request.RequestedAt.Month.ToString("D2"),
                    "dy" => executionResult.Request.RequestedAt.Year.ToString("D4"),

                    "th" => executionResult.Request.RequestedAt.ToString("hh"),
                    "tH" => executionResult.Request.RequestedAt.ToString("HH"),
                    "ti" => executionResult.Request.RequestedAt.ToString("MM"),
                    "ts" => executionResult.Request.RequestedAt.ToString("ss"),
                    "tm" => executionResult.Request.RequestedAt.Millisecond.ToString("D3"),
                    "tz" => $"{TimeZoneInfo.Local.GetUtcOffset(executionResult.Request.RequestedAt).TotalHours:00}00",

                    "ri" => executionResult.Request.RemoteAddress.ToString(),
                    "rm" => executionResult.Request.Method.Method.ToUpper(),
                    "rs" => executionResult.Request.Uri.Scheme,
                    "ra" => executionResult.Request.Authority,
                    "rh" => executionResult.Request.Host,
                    "rp" => executionResult.Request.Uri.Port.ToString(),
                    "rz" => executionResult.Request.Path,
                    "rq" => executionResult.Request.QueryString,

                    "sc" => executionResult.Response?.Status.StatusCode.ToString(),
                    "sd" => executionResult.Response?.Status.Description.ToString(),

                    "lin" => SizeHelper.HumanReadableSize(executionResult.RequestSize),
                    "linr" => executionResult.RequestSize.ToString(),

                    "lou" => SizeHelper.HumanReadableSize(executionResult.ResponseSize),
                    "lour" => executionResult.ResponseSize.ToString(),

                    "lms" => executionResult.Elapsed.TotalMilliseconds.ToString("N0"),
                    "ls" => executionResult.Status.ToString(),
                    _ => match.Value
                };
            }

            sb.Append(formatSpan[lastIndex..match.Index]);
            sb.Append(result);

            lastIndex = match.Index + match.Length;
        }

        return sb.ToString();
    }
}
