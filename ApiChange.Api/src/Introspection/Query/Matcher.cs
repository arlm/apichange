
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Text.RegularExpressions;

namespace ApiChange.Api.Introspection
{

    /// <summary>
    /// Partial String matcher class which supports wildcards
    /// </summary>
    internal static class Matcher
    {
        static char[] myNsTrimChars = new char[] { ' ', '*', '\t' };
        const string EscapedStar = "magic_star";

        // Cache filter string regular expressions for later reuse
        internal static Dictionary<string, Regex> myFilter2Regex = new Dictionary<string, Regex>();

        /// <summary>
        /// Check if a given test string does match the pattern specified by the filterString. Besides
        /// normal string comparisons for the patterns *xxx, xxx*, *xxx* which are mapped to String.EndsWith,
        /// String.StartsWith and String.Contains are regular expressions used if the pattern is more complex
        /// like *xx*bbb.
        /// </summary>
        /// <param name="filterString">Filter string. A filter string of null or * will match any testString. If the teststring is null it will never match anything.</param>
        /// <param name="testString">String to check</param>
        /// <param name="compMode">String Comparision mode</param>
        /// <returns>true if the teststring does match, false otherwise.</returns>
        public static bool MatchWithWildcards(string filterString, string testString, StringComparison compMode)
        {
            if (filterString == null || filterString == "*")
                return true;

            if (testString == null)
            {
                return false;
            }


            if (IsRegexMatchNecessary(filterString))
            {
                return IsMatch(filterString, testString, compMode);
            }

            bool bMatchEnd = false;
            if (filterString.StartsWith("*", compMode))
            {
                bMatchEnd = true;
            }

            bool bMatchStart = false;
            if (filterString.EndsWith("*", compMode))
            {
                bMatchStart = true;
            }

            string filterSubstring = filterString.Trim(myNsTrimChars);

            if (bMatchStart && bMatchEnd)
            {
                if (compMode == StringComparison.OrdinalIgnoreCase ||
                    compMode == StringComparison.InvariantCultureIgnoreCase)
                {
                    return testString.ToLower().Contains(filterSubstring.ToLower());
                }
                else
                {
                    return testString.Contains(filterSubstring);
                }
            }

            if (bMatchStart)
            {
                return testString.StartsWith(filterSubstring,compMode);
            }

            if (bMatchEnd)
            {
                return testString.EndsWith(filterSubstring,compMode);
            }

            return String.Compare(testString,filterSubstring, compMode) == 0;
        }

  
        // Check if * occurs inside filter string and not only at start or end
        internal static bool IsRegexMatchNecessary(string filter)
        {
            int start = Math.Min(1, Math.Max(filter.Length-1,0));
            int len = Math.Max(filter.Length - 2, 0);
            return filter.IndexOf("*", start, len) != -1;
        }

        static internal bool IsMatch(string filter, string testString, StringComparison compMode)
        {
            Regex filterRex = GenerateRegexFromFilter(filter, compMode);
            return filterRex.IsMatch(testString);
        }


        internal static Regex GenerateRegexFromFilter(string filter, StringComparison mode)
        {
            Regex lret = null;

            if (!myFilter2Regex.TryGetValue(filter, out lret))
            {
                string rex = Regex.Escape(filter.Replace("*", EscapedStar));
                rex = "^" + rex + "$";
                lret = new Regex(rex.Replace(EscapedStar, ".*?"), 
                                (   mode == StringComparison.CurrentCultureIgnoreCase || 
                                    mode == StringComparison.InvariantCultureIgnoreCase ||
                                    mode == StringComparison.OrdinalIgnoreCase
                                 ) ? RegexOptions.IgnoreCase : RegexOptions.None);
                myFilter2Regex[filter] = lret;
            }

            return lret;
        }
    }
}
