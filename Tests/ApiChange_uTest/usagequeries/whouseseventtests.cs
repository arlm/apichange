
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;
using Mono.Cecil;

namespace UnitTests.UsageQueries
{
    [TestFixture]
    public class WhoUsesEventTests
    {
        [Test]
        public void Can_Find_All_Subscribers_To_Static_And_Instance_Events()
        {
            TypeDefinition type = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicBaseClass");
            List<EventDefinition> evs = new EventQuery().GetMatchingEvents(type);
            Assert.AreEqual(2, evs.Count, "Class has events defined");

            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.MethodUsage"));
            new WhoUsesEvents(agg, evs);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(3, agg.MethodMatches.Count);
        }
    }
}
