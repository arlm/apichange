
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;
using ApiChange.Infrastructure;

namespace UnitTests.UsageQueries
{
    [TestFixture]
    public class WhoImplementsInterfaceTests
    {
        [Test]
        public void Can_Find_IDisposable_Implementer()
        {
            var iDisposable = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IDisposable");

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoImplementsInterface(agg, iDisposable);

            try
            {
                agg.Analyze(TestConstants.DependandLibV1Assembly);
                var results = agg.TypeMatches;
                Assert.AreEqual(2, results.Count);
                Assert.AreEqual("public interface System.IDisposable", agg.TypeMatches[0].Annotations[MatchContext.MatchItem]);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var typeMatch in agg.TypeMatches)
                    {
                        Console.WriteLine("IDisposable is implemeted by {0}: {1}", typeMatch.Match.Print(), typeMatch.SourceFileName);
                    }
                }
            }
        }

        [Test]
        public void Can_Find_IDisposable_Implementer_Without_Pdb()
        {
            var iDisposable = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IDisposable");

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoImplementsInterface(agg, iDisposable);

            try
            {
                agg.Analyze(TestConstants.DependandLibV1Assembly);
                var results = agg.TypeMatches;
                Assert.AreEqual(2, results.Count);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var typeMatch in agg.TypeMatches)
                    {
                        Console.WriteLine("IDisposable is implemeted by {0}: {1}", typeMatch.Match.Print(), typeMatch.SourceFileName);
                    }
                }
            }
        }

        [Test]
        public void Can_Differentiate_Between_Generic_Interface_Implementations()
        {
            var iGenericIComparable = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IComparable`1");
            var iIComparable = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IComparable");

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoImplementsInterface(agg, iGenericIComparable);
            new WhoImplementsInterface(agg, iIComparable);

            agg.Analyze(TestConstants.DependandLibV1Assembly);
            var results = agg.TypeMatches;
            Assert.AreEqual(2, results.Count);
        }
    }
}
