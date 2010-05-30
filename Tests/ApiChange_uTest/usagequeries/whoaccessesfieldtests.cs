
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
    class WhoAccessesFieldTests
    {
        [Test]
        public void Can_Find_Instance_Accesses_In_TypeInitializer_And_Other_Function()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.FieldQueries","ClassWhichAccessesFields"));
            new WhoAccessesField(agg, new FieldQuery().GetMatchingFields(TestConstants.PublicBaseClassTypeV1));
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(9, agg.MethodMatches.Count, "Should have at least found these accesses to fields of a base class");

            HashSet<string> methods = new HashSet<string>();
            foreach (var match in agg.MethodMatches)
            {
                methods.Add(match.Match.Name);
            }

            Assert.AreEqual(3, methods.Count);
            Assert.IsTrue(methods.Contains("PassFieldsAsFunctionArguments"));
            Assert.IsTrue(methods.Contains("AssignFields"));
            Assert.IsTrue(methods.Contains(".cctor"));
        }
    }
}
