
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;
using Mono.Cecil;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class AssemblyDifferTests
    { 
        QueryAggregator myQueries;

        [TestFixtureSetUp]
        public void GenerateSelectiveQueries()
        {
            myQueries = new QueryAggregator();
            myQueries.TypeQueries.Add( new TypeQuery(TypeQueryMode.ApiRelevant, "BaseLibrary.ApiChanges"));

            myQueries.MethodQueries.Add( MethodQuery.PublicMethods );
            myQueries.MethodQueries.Add( MethodQuery.ProtectedMethods );

            myQueries.FieldQueries.Add(  FieldQuery.PublicFields );
            myQueries.FieldQueries.Add(  FieldQuery.ProtectedFields );

            myQueries.EventQueries.Add(EventQuery.PublicEvents);
            myQueries.EventQueries.Add(EventQuery.ProtectedEvents);
        }

        [Test]
        public void DiffBaseibraryV2VsV1()
        {
            AssemblyDiffer differ = new AssemblyDiffer(TestConstants.BaseLibV1Assembly, TestConstants.BaseLibV2Assembly);
            AssemblyDiffCollection diff = differ.GenerateTypeDiff(myQueries);
            Assert.AreEqual(4, diff.AddedRemovedTypes.AddedCount, "Added types");
            Assert.AreEqual(4, diff.AddedRemovedTypes.RemovedCount, "Removed types");
            Assert.AreEqual(3, diff.ChangedTypes.Count, "Changed Types");
        }
    }
 
}