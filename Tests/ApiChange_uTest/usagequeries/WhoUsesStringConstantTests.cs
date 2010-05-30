
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;

namespace UnitTests.UsageQueries
{
    [TestFixture]
    public class WhoUsesStringConstantTests
    {
        [Test]
        public void Throw_When_Null_String_IsPassed()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            agg.AddVisitScope("test");
            Assert.Throws<ArgumentException>(() => new WhoUsesStringConstant(agg, null));
        }

        [Test]
        public void Throw_When_Empty_Stirng_IsPassed()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            agg.AddVisitScope("test");
            Assert.Throws<ArgumentException>(() => new WhoUsesStringConstant(agg, ""));
        }

        [Test]
        public void Can_Find_StringConstant_Usage()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            agg.AddVisitScope(TestConstants.BaseLibV1);
            new WhoUsesStringConstant(agg, "Global A string");
            agg.Analyze(TestConstants.DependandLibV1Assembly);
            Assert.AreEqual(3, agg.MethodMatches.Count);
            Assert.AreEqual(1, agg.FieldMatches.Count);
        }

        [Test]
        public void Can_Find_Substring()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            agg.AddVisitScope(TestConstants.BaseLibV1);
            new WhoUsesStringConstant(agg, "Global A");
            agg.Analyze(TestConstants.DependandLibV1Assembly);
            Assert.AreEqual(3, agg.MethodMatches.Count);
            Assert.AreEqual(1, agg.FieldMatches.Count);

        }

        [Test]
        public void Can_Find_Word_Case_InsenstiveSensitive()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            agg.AddVisitScope(TestConstants.BaseLibV1);
            new WhoUsesStringConstant(agg, "GLOBAL A STRING", true, StringComparison.OrdinalIgnoreCase);
            agg.Analyze(TestConstants.DependandLibV1Assembly);
            Assert.AreEqual(2, agg.MethodMatches.Count);
        }

        [Test]
        public void Cannot_Find_Word_If_Casing_Is_Different()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            agg.AddVisitScope(TestConstants.BaseLibV1);
            new WhoUsesStringConstant(agg, "GLOBAL A STRING", true, StringComparison.Ordinal);
            agg.Analyze(TestConstants.DependandLibV1Assembly);
            Assert.AreEqual(0, agg.MethodMatches.Count);
        }
    }

}