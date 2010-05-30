
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;

namespace UnitTests.UsageQueries
{
    [TestFixture]
    class WhoDerivesFromTypeTests
    {
        [Test]
        public void CanFindTypesThatDeriveFromGenericBaseClasses()
        {
            var genList = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Collections.Generic.List`1");

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoDerivesFromType(agg, genList);

            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(2, agg.TypeMatches.Count);
        }
    }
}
