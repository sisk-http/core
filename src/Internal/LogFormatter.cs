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
using System.Text;

namespace Sisk.Core.Internal;

internal class LogFormatter
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

    public static string FormatAccessLogEntry(in string format, HttpServerExecutionResult executionResult)
    {
        ReadOnlySpan<char> formatSpan = format.AsSpan();
        StringBuilder sb = new StringBuilder(format.Length);

        int incidences = formatSpan.Count('%');
        Span<Range> formatRanges = stackalloc Range[incidences];
        ExtractIncidences(formatSpan, formatRanges);

        Index lastIndexStart = Index.FromStart(0);
        for (int i = 0; i < incidences; i++)
        {
            ref Range currentRange = ref formatRanges[i];
            ReadOnlySpan<char> term = formatSpan[(currentRange.Start.Value + 1)..currentRange.End];
            string? result;

            if (term is ['{', .., '}'])
            {
                result = executionResult.Request.Headers[new string(term[1..^1])];
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
                    _ => new string(formatSpan[currentRange])
                };
            }

            sb.Append(formatSpan[lastIndexStart..currentRange.Start]);
            sb.Append(result);

            lastIndexStart = currentRange.End;
        }

        return sb.ToString();
    }

    static int ExtractIncidences(ReadOnlySpan<char> input, Span<Range> output)
    {
        int inputLength = input.Length,
            index = 0,
            found = 0;

        while ((index = input.IndexOf('%', index)) >= 0)
        {
            int seek = index++;
            char current = input[seek];
            if (seek == inputLength - 1)//reached end
            {
                return found;
            }
            else if ((current = input[++seek]) == '{')
            {
                while (seek < inputLength - 1 && input[++seek] != '}')
                {
                    ;
                }

                if (seek < inputLength - 1)
                    seek++;
                else if (seek == inputLength - 1 && char.IsLetter(input[seek]))
                    seek++;

                output[found++] = new Range(index - 1, Index.FromStart(seek));
            }
            else if (char.IsLetter(current))
            {
                while (seek < inputLength - 1 && char.IsLetter(current = input[++seek]))
                {
                    ;
                }

                if (seek == inputLength - 1 && char.IsLetter(input[seek]))
                    seek++;

                output[found++] = new Range(index - 1, Index.FromStart(seek));
            }

        }
        return found;
    }
}
