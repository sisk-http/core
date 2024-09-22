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
        public record PathMatchResult(bool IsMatched, NameValueCollection? Query);

        public static bool PathRouteMatch(ReadOnlySpan<char> routeA, ReadOnlySpan<char> routeB, bool ignoreCase)
        {
            const char SEPARATOR = '/';
            const string ROUTE_GROUP_START = "<";
            const string ROUTE_GROUP_END = ">";

            if (routeA == Route.AnyPath || routeB == Route.AnyPath)
            {
                return true;
            }

            int pathPatternSepCount = routeA.Count(SEPARATOR);
            int reqsPatternSepCount = routeB.Count(SEPARATOR);

            Span<Range> aPrt = stackalloc Range[pathPatternSepCount];
            Span<Range> bPrt = stackalloc Range[reqsPatternSepCount];

            int splnA = routeA.Split(aPrt, SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            int splnB = routeB.Split(bPrt, SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            if (splnA != splnB)
            {
                return false;
            }

            int matchCount = 0;
            for (int i = 0; i < splnA; i++)
            {
                Range aRng = aPrt[i];
                Range bRng = bPrt[i];

                ReadOnlySpan<char> aPtt = routeA[aRng.Start..aRng.End].Trim(SEPARATOR);
                ReadOnlySpan<char> bPtt = routeB[bRng.Start..bRng.End].Trim(SEPARATOR);

                bool isPathAQuery = aPtt.StartsWith(ROUTE_GROUP_START) && aPtt.EndsWith(ROUTE_GROUP_END);
                bool isPathBQuery = bPtt.StartsWith(ROUTE_GROUP_START) && bPtt.EndsWith(ROUTE_GROUP_END);

                if (isPathAQuery || isPathBQuery)
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

        [MethodImpl(MethodImplOptions.AggressiveOptimization)]
        public static PathMatchResult IsPathMatch(ReadOnlySpan<char> pathPattern, ReadOnlySpan<char> requestPath, bool ignoreCase)
        {
            const char SEPARATOR = '/';
            const string ROUTE_GROUP_START = "<";
            const string ROUTE_GROUP_END = ">";

            if (pathPattern == Route.AnyPath)
            {
                return new PathMatchResult(true, null);
            }

            NameValueCollection? query = null;

            int pathPatternSepCount = pathPattern.Count(SEPARATOR);
            int reqsPatternSepCount = requestPath.Count(SEPARATOR);

            Span<Range> pathPatternParts = stackalloc Range[pathPatternSepCount];
            Span<Range> requestPathParts = stackalloc Range[reqsPatternSepCount];

            int splnA = pathPattern.Split(pathPatternParts, SEPARATOR, StringSplitOptions.RemoveEmptyEntries);
            int splnB = requestPath.Split(requestPathParts, SEPARATOR, StringSplitOptions.RemoveEmptyEntries);

            if (splnA != splnB)
            {
                return new PathMatchResult(false, query);
            }

            for (int i = 0; i < splnA; i++)
            {
                Range pathRng = pathPatternParts[i];
                Range reqsRng = requestPathParts[i];

                ReadOnlySpan<char> pathPtt = pathPattern[pathRng.Start..pathRng.End].Trim(SEPARATOR);
                ReadOnlySpan<char> reqsPtt = requestPath[reqsRng.Start..reqsRng.End].Trim(SEPARATOR);

                if (pathPtt.StartsWith(ROUTE_GROUP_START) && pathPtt.EndsWith(ROUTE_GROUP_END))
                {
                    if (query is null) query = new NameValueCollection();
                    string queryValueName = new string(pathPtt[new Range(1, pathPtt.Length - 1)]);
                    query.Add(queryValueName, new string(reqsPtt));
                }
                else
                {
                    if (pathPtt.CompareTo(reqsPtt, ignoreCase ? StringComparison.InvariantCultureIgnoreCase : StringComparison.InvariantCulture) != 0)
                    {
                        return new PathMatchResult(false, query);
                    }
                }
            }

            return new PathMatchResult(true, query);
        }

        public static bool IsDnsMatch(in string wildcardPattern, string subject)
        {
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;

            int portSeparatorPosition = subject.IndexOf(':');
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
                    return isWildcardMatchRgx(wildcardPattern, subject, comparer);
                }
            }
            else
            {
                return isWildcardMatchRgx(wildcardPattern, subject, comparer);
            }
        }

        private static bool isWildcardMatchRgx(in string pattern, in string subject, StringComparison comparer)
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
    }
}
