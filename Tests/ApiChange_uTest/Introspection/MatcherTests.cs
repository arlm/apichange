

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class MatcherTests
    {
        [Test]
        public void Null_Filter_Matches_Everything()
        {
            Assert.IsTrue(Matcher.MatchWithWildcards(null, "", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards(null, null, StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards(null, "xxx", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Empty_Filter_Matches_Only_Empty_String()
        {
            Assert.IsTrue(Matcher.MatchWithWildcards("", "", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("", "xxx", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("", null, StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("", "xxx", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Star_Filter_Matches_Everything()
        {
            Assert.IsTrue(Matcher.MatchWithWildcards("*", "xxx", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards("*", "", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards("*", null, StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void EndsWith_Filter_MatchesOnlyEnd()
        {
            Assert.IsTrue(Matcher.MatchWithWildcards("*a", "a", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards("*a", "xxxa", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("*a", null, StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("*a", "b", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("*a", "ab", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void StartsWith_Filter_MatchesOnlyStart()
        {
            Assert.IsTrue(Matcher.MatchWithWildcards("a*", "a", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards("a*", "ab", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*", "xxxa", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*", null, StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*", "b", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*", "baaaa", StringComparison.OrdinalIgnoreCase));
        }

        [Test]
        public void Regex_Filter_Does_Match_Not_Greedy()
        {
            Assert.IsTrue(Matcher.MatchWithWildcards("a*b*c", "abc", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards("a*b*c", "abc", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards("a*b*c", "aaabbbbcccc", StringComparison.OrdinalIgnoreCase));
            Assert.IsTrue(Matcher.MatchWithWildcards("a*b*c", "ab4tq3grawc", StringComparison.OrdinalIgnoreCase));


            Assert.IsFalse(Matcher.MatchWithWildcards("a*b*c", "ac", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*b*c", "a", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*b*c", "ab", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*b*c", "bc", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*b*c", "abca", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*b*c", "", StringComparison.OrdinalIgnoreCase));
            Assert.IsFalse(Matcher.MatchWithWildcards("a*b*c", null, StringComparison.OrdinalIgnoreCase));
        }
    }
}
