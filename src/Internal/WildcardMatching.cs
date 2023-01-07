using System;
using System.Collections.Generic;
using System.Collections.Specialized;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Sisk.Core.Internal
{
    internal class WildcardMatching
    {
        public record PathMatchResult(bool IsMatched, NameValueCollection Query);

        public PathMatchResult IsPathMatch(string pathPattern, string requestPath, bool ignoreCase)
        {
            NameValueCollection query = new NameValueCollection();
            pathPattern = pathPattern.TrimEnd('/');
            requestPath = requestPath.TrimEnd('/');

            string[] pathPatternParts = pathPattern.Split('/');
            string[] requestPathParts = requestPath.Split('/');

            if (pathPatternParts.Length != requestPathParts.Length)
            {
                return new PathMatchResult(false, query);
            }

            for (int i = 0; i < pathPatternParts.Length; i++)
            {
                string pathPtt = pathPatternParts[i];
                string reqsPtt = requestPathParts[i];

                if (pathPtt.StartsWith('<') && pathPtt.EndsWith('>'))
                {
                    string queryValueName = pathPtt.Substring(1, pathPtt.Length - 2);
                    query.Add(queryValueName, reqsPtt);
                }
                else
                {
                    if (string.Compare(pathPtt, reqsPtt, ignoreCase) != 0)
                    {
                        return new PathMatchResult(false, query);
                    }
                }
            }

            return new PathMatchResult(true, query);
        }

        public bool IsDnsMatch(string wildcardPattern, string subject)
        {
            StringComparison comparer = StringComparison.OrdinalIgnoreCase;
            wildcardPattern = wildcardPattern.Trim();
            subject = subject.Trim();

            if (string.IsNullOrWhiteSpace(wildcardPattern))
            {
                return false;
            }

            if (subject.StartsWith(wildcardPattern.Replace("*.", "")))
            {
                return true;
            }

            int wildcardCount = wildcardPattern.Count(x => x.Equals('*'));
            if (wildcardCount <= 0)
            {
                return subject.Equals(wildcardPattern, comparer);
            }
            else if (wildcardCount == 1)
            {
                string newWildcardPattern = wildcardPattern.Replace("*", "");

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

        private bool isWildcardMatchRgx(string pattern, string subject, StringComparison comparer)
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
