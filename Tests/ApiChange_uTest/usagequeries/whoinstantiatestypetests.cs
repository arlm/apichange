
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;

namespace UnitTests.UsageQueries
{
    [TestFixture]
    public class WhoInstantiatesTypeTests
    {
        [Test]
        public void Can_Find_Reference_Type_Instantiations()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoInstantiatesQueries", "ClassWhichInstantiatesReferenceType"));
            new WhoInstantiatesType(agg, TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicGenericClass`1"));
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "Should get ctor call");
        }

        [Test]
        public void Can_Find_Value_Type_Instantiations()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoInstantiatesQueries", "ClassWhichInstantiatesValueType"));
            new WhoInstantiatesType(agg, TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.DateTime"));
            new WhoInstantiatesType(agg, TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Threading.AsyncFlowControl"));
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(3, agg.MethodMatches.Count, "Should get ctor call");
        }

    }
}
