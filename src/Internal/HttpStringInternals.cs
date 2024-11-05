// The Sisk Framework source code
// Copyright (c) 2024 PROJECT PRINCIPIUM
//
// The code below is licensed under the MIT license as
// of the date of its publication, available at
//
// File name:   HttpStringInternals.cs
// Repository:  https://github.com/sisk-http/core

using Sisk.Core.Routing;
using System.Collections.Specialized;
using System.Runtime.CompilerServices;

namespace Sisk.Core.Internal
{
    internal static class HttpStringInternals
    {
        public record struct PathMatchResult(bool IsMatched, NameValueCollection? Query);

        const char ROUTE_SEPARATOR = '/';
        const char ROUTE_GROUP_START = '<';
        const char ROUTE_GROUP_END = '>';

        static readonly char[] ROUTE_TRIM_CHARS = [ROUTE_SEPARATOR, ' ', '\t'];

        /// <summary>
        /// Asserts if the specified route pattern is valid or not. (NOT FOR REGEX ROUTES)
        /// </summary>
        public static void AssertRoute(ReadOnlySpan<char> route)
        {
            if (route == Route.AnyPath)
            {
                return;
            }
            if (route.Length == 0 || route[0] != ROUTE_SEPARATOR)
            {
                throw new FormatException(SR.Router_Set_InvalidRouteStart);
            }

            int pathPatternSepCount = route.Count(ROUTE_SEPARATOR);
            Span<Range> parts = stackalloc Range[pathPatternSepCount];

            int partCount = route.Split(parts, ROUTE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            for (int i = 0; i < partCount; i++)
            {
                var partRng = parts[i];
                var part = route[partRng].Trim(ROUTE_SEPARATOR);

                if (part.Length == 0)
                {
                    if (i != partCount - 1)
                    {
                        throw new FormatException(SR.Router_EmptyRouterPathPart);
                    }
                }
                else if (part[0] == ROUTE_GROUP_START)
                {
                    if (part.Length < 2)
                    {
                        throw new FormatException(SR.Router_NameExpected);
                    }
                    if (part[^1] != ROUTE_GROUP_END)
                    {
                        throw new FormatException(SR.Router_GtExpected);
                    }
                }
            }
        }

        /// <summary>
        /// Test if two routes matches their patterns, by comparing two <see cref="Route"/>s.
        /// </summary>
        public static bool IsRoutePatternMatch(ReadOnlySpan<char> routeA, ReadOnlySpan<char> routeB, bool ignoreCase)
        {
            if (IsRouteAnyPath(routeA) || IsRouteAnyPath(routeB))
            {
                return true;
            }

            int pathPatternSepCount = routeA.Count(ROUTE_SEPARATOR);
            int reqsPatternSepCount = routeB.Count(ROUTE_SEPARATOR);

            Span<Range> aPrt = stackalloc Range[pathPatternSepCount];
            Span<Range> bPrt = stackalloc Range[reqsPatternSepCount];

            int splnA = routeA.Split(aPrt, ROUTE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            int splnB = routeB.Split(bPrt, ROUTE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            if (splnA != splnB)
            {
                return false;
            }

            int matchCount = 0;
            for (int i = 0; i < splnA; i++)
            {
                Range aRng = aPrt[i];
                Range bRng = bPrt[i];

                ReadOnlySpan<char> aPtt = routeA[aRng].Trim(ROUTE_TRIM_CHARS);
                ReadOnlySpan<char> bPtt = routeB[bRng].Trim(ROUTE_TRIM_CHARS);

                bool isPathAQuery = aPtt is [ROUTE_GROUP_START, .., ROUTE_GROUP_END];
                bool isPathBQuery = bPtt is [ROUTE_GROUP_START, .., ROUTE_GROUP_END];

                if (isPathAQuery && bPtt.Length > 0)
                {
                    matchCount++;
                }
                else if (isPathBQuery && aPtt.Length > 0)
                {
                    matchCount++;
                }
                else if (aPtt.CompareTo(bPtt, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) == 0)
                {
                    matchCount++;
                }
            }

            return matchCount == splnA;
        }

        /// <summary>
        /// Tests if one request path matches the specified route pattern, by comparing one ROUTE PATH and one REQUEST PATH. This
        /// method allocates route parameters.
        /// </summary>
        public static PathMatchResult IsReqPathMatch(ReadOnlySpan<char> pathPattern, ReadOnlySpan<char> requestPath, bool ignoreCase)
        {
            if (IsRouteAnyPath(pathPattern))
            {
                return new PathMatchResult(true, null);
            }

            NameValueCollection? pathParams = null;

            int pathPatternSepCount = pathPattern.Count(ROUTE_SEPARATOR);
            int reqsPatternSepCount = requestPath.Count(ROUTE_SEPARATOR);

            Span<Range> pathPatternParts = stackalloc Range[pathPatternSepCount];
            Span<Range> requestPathParts = stackalloc Range[reqsPatternSepCount];

            int splnA = pathPattern.Split(pathPatternParts, ROUTE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            int splnB = requestPath.Split(requestPathParts, ROUTE_SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            if (splnA != splnB)
            {
                return new PathMatchResult(false, pathParams);
            }

            for (int i = 0; i < splnA; i++)
            {
                Range pathRng = pathPatternParts[i];
                Range reqsRng = requestPathParts[i];

                ReadOnlySpan<char> pathPtt = pathPattern[pathRng].Trim(ROUTE_TRIM_CHARS);
                ReadOnlySpan<char> reqsPtt = requestPath[reqsRng].Trim(ROUTE_TRIM_CHARS);

                if (pathPtt is [ROUTE_GROUP_START, .., ROUTE_GROUP_END] && reqsPtt.Length > 0)
                {
                    pathParams ??= new NameValueCollection();
                    string queryValueName = new string(pathPtt[new Range(1, pathPtt.Length - 1)]);
                    pathParams.Add(queryValueName, new string(reqsPtt));
                }
                else
                {
                    if (pathPtt.CompareTo(reqsPtt, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) != 0)
                    {
                        return new PathMatchResult(false, pathParams);
                    }
                }
            }

            return new PathMatchResult(true, pathParams);
        }

        public static bool IsDnsMatch(in string wildcardPattern, string subject)
        {
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;

            int portSeparatorPosition = subject.IndexOf(':', StringComparison.Ordinal);
            if (portSeparatorPosition > 0)
            {
                subject = subject[0..portSeparatorPosition];
            }

            int wildcardCount = wildcardPattern.Count(x => x == '*');
            if (wildcardPattern.StartsWith("*.") && wildcardCount == 1)
            {
                if (string.Compare(subject, wildcardPattern[2..], comparer) == 0)
                    return true;
            }

            if (wildcardCount <= 0)
            {
                return subject.Equals(wildcardPattern, comparer);
            }
            else if (wildcardCount == 1)
            {
                string newWildcardPattern = wildcardPattern.Replace("*", string.Empty);

                if (wildcardPattern.StartsWith("*"))
                {
                    return subject.EndsWith(newWildcardPattern, comparer);
                }
                else if (wildcardPattern.EndsWith("*"))
                {
                    return subject.StartsWith(newWildcardPattern, comparer);
                }
                else
                {
                    return WildcardMatch(wildcardPattern, subject, comparer);
                }
            }
            else
            {
                return WildcardMatch(wildcardPattern, subject, comparer);
            }
        }

        private static bool WildcardMatch(in string pattern, in string subject, StringComparison comparer)
        {
            string[] parts = pattern.Split('*');
            if (parts.Length <= 1)
            {
                return subject.Equals(pattern, comparer);
            }

            int pos = 0;

            for (int i = 0; i < parts.Length; i++)
            {
                if (i <= 0)
                {
                    // first
                    pos = subject.IndexOf(parts[i], pos, comparer);
                    if (pos != 0)
                    {
                        return false;
                    }
                }
                else if (i >= (parts.Length - 1))
                {
                    // last
                    if (!subject.EndsWith(parts[i], comparer))
                    {
                        return false;
                    }
                }
                else
                {
                    pos = subject.IndexOf(parts[i], pos, comparer);
                    if (pos < 0)
                    {
                        return false;
                    }

                    pos += parts[i].Length;
                }
            }

            return true;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        private static bool IsRouteAnyPath(ReadOnlySpan<char> path)
            => MemoryExtensions.Equals(path, Route.AnyPath, StringComparison.Ordinal);
    }
}
