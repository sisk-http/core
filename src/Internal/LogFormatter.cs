﻿// The Sisk Framework source code
// Copyright (c) 2024- PROJECT PRINCIPIUM and all Sisk contributors
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   LogFormatter.cs
// Repository:  https://github.com/sisk-http/core

using System.Globalization;
using System.Runtime.CompilerServices;
using System.Text;
using Sisk.Core.Helpers;
using Sisk.Core.Http;

namespace Sisk.Core.Internal;

static class LogFormatter {
    public static string FormatExceptionEntr ( HttpServerExecutionResult executionResult ) {
        StringBuilder errLineBuilder = new StringBuilder ( 128 );
        errLineBuilder.Append ( '[' );
        errLineBuilder.Append ( executionResult.Request.RequestedAt.ToString ( "G", CultureInfo.CurrentCulture.DateTimeFormat ) );
        errLineBuilder.Append ( ']' );
        errLineBuilder.Append ( ' ' );
        errLineBuilder.Append ( executionResult.Request.Method.Method );
        errLineBuilder.Append ( ' ' );
        errLineBuilder.Append ( executionResult.Request.FullPath );
        errLineBuilder.AppendLine ();
        errLineBuilder.AppendLine ( executionResult.ServerException?.ToString () );
        if (executionResult.ServerException?.InnerException is { } iex) {
            errLineBuilder.AppendLine ( "[inner exception]" );
            errLineBuilder.AppendLine ( iex.ToString () );
        }

        errLineBuilder.AppendLine ();
        return errLineBuilder.ToString ();
    }

    public static string FormatAccessLogEntry ( string format, HttpServerExecutionResult executionResult ) {
        ReadOnlySpan<char> formatSpan = format.AsSpan ();
        StringBuilder sb = new StringBuilder ( format.Length * 2 );

        int incidences = formatSpan.Count ( '%' );
        Span<Range> formatRanges = stackalloc Range [ incidences ];
        ExtractIncidences ( formatSpan, formatRanges );

        Index lastIndexStart = Index.FromStart ( 0 );

        for (int i = 0; i < incidences; i++) {

            ref Range currentRange = ref formatRanges [ i ];
            ReadOnlySpan<char> term = formatSpan [ new Range ( currentRange.Start.Value + 1, currentRange.End ) ];
            string result;

            if (term is [ '{', .., '}' ]) {
                string headerName = new string ( term [ 1..^1 ] );
                if (headerName.StartsWith ( ':' )) {
                    result = executionResult.Response?.GetHeaderValue ( headerName [ 1.. ] )
                        ?? string.Empty;
                }
                else {
                    result = executionResult.Request.Headers [ headerName ]
                        ?? string.Empty;
                }
            }
            else {
                result = MatchTermExpression ( term, executionResult )
                    ?? string.Empty;
            }

            sb.Append ( formatSpan [ lastIndexStart..currentRange.Start ] );
            sb.Append ( result );

            lastIndexStart = currentRange.End;
        }

        if (lastIndexStart.Value < formatSpan.Length) {
            // add remainder
            sb.Append ( formatSpan [ lastIndexStart.. ] );
        }

        return sb.ToString ();
    }

    [MethodImpl ( MethodImplOptions.AggressiveInlining )]
    static string? MatchTermExpression ( in ReadOnlySpan<char> term, HttpServerExecutionResult executionResult )
        => term switch {
            "dd" => executionResult.Request.RequestedAt.Day.ToString ( "D2", provider: null ),
            "dmmm" => executionResult.Request.RequestedAt.ToString ( "MMMM", provider: null ),
            "dmm" => executionResult.Request.RequestedAt.ToString ( "MMM", provider: null ),
            "dm" => executionResult.Request.RequestedAt.Month.ToString ( "D2", provider: null ),
            "dy" => executionResult.Request.RequestedAt.Year.ToString ( "D4", provider: null ),

            "th" => executionResult.Request.RequestedAt.ToString ( "hh", provider: null ),
            "tH" => executionResult.Request.RequestedAt.ToString ( "HH", provider: null ),
            "ti" => executionResult.Request.RequestedAt.ToString ( "mm", provider: null ),
            "ts" => executionResult.Request.RequestedAt.ToString ( "ss", provider: null ),
            "tm" => executionResult.Request.RequestedAt.Millisecond.ToString ( "D3", provider: null ),
            "tz" => $"{HttpServer.environmentUtcOffset.TotalHours:00}00",

            "ri" => executionResult.Request.RemoteAddress.ToString (),
            "rm" => executionResult.Request.Method.Method.ToUpperInvariant (),
            "rs" => executionResult.Request.Uri.Scheme,
            "ra" => executionResult.Request.Authority,
            "rh" => executionResult.Request.Host,
            "rp" => executionResult.Request.Uri.Port.ToString ( provider: null ),
            "rz" => executionResult.Request.Path,
            "rq" => executionResult.Request.QueryString,

            "sc" => executionResult.Response?.Status.StatusCode.ToString ( provider: null ),
            "sd" => executionResult.Response?.Status.Description.ToString (),

            "lin" => SizeHelper.HumanReadableSize ( executionResult.RequestSize ),
            "linr" => executionResult.RequestSize.ToString ( provider: null ),

            "lou" => SizeHelper.HumanReadableSize ( executionResult.ResponseSize ),
            "lour" => executionResult.ResponseSize.ToString ( provider: null ),

            "lms" => executionResult.Elapsed.TotalMilliseconds.ToString ( "N0", provider: null ),
            "ls" => executionResult.Status.ToString (),
            _ => null
        };

    static int ExtractIncidences ( ReadOnlySpan<char> input, Span<Range> output ) {
        int inputLength = input.Length,

            // 0 = find next % ocurrence
            // 1 = find next non-letter ocurrence
            // 2 = find next } ocurrence
            mode = 0,

            rangeStart = -1,
            rangeEnd = -1,

            found = 0;

        for (int i = 0; i < inputLength; i++) {
            char current = input [ i ];
            if (current == '%') {
                if (mode == 1) {
                    output [ found++ ] = new Range ( rangeStart, i );
                }
                rangeStart = i;
                rangeEnd = -1;
                mode = 1;
            }
            else if (mode == 1) {
                if (current == '{') {
                    mode = 2;
                }
                else if (!char.IsLetter ( current )) {
                    rangeEnd = i;
                    mode = 0;
                    output [ found++ ] = new Range ( rangeStart, rangeEnd );
                }
            }
            else if (mode == 2 && current == '}') {
                rangeEnd = i;
                mode = 0;
                output [ found++ ] = new Range ( rangeStart, rangeEnd + 1 );
            }
        }

        if (mode == 1) {
            output [ found++ ] = new Range ( rangeStart, inputLength );
        }

        return found;
    }
}
