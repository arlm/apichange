
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
    public class WhoReferencesAssemblyTests
    {
        [Test]
        public void Can_Find_MsCorlib_Reference()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoReferencesAssembly(agg, TestConstants.BaseLibV1);
            agg.Analyze(TestConstants.DependandLibV1Assembly);
            var assemblyReference = agg.AssemblyMatches.ToList();
            try
            {
                Assert.AreEqual(1, assemblyReference.Count);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (string reference in assemblyReference)
                    {
                        Console.WriteLine("The assembly {0} references {1}", TestConstants.DependandLibV1Assembly, reference);
                    }
                }
            }
        }

        [Test]
        public void Do_Not_Return_Wrong_References()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoReferencesAssembly(agg, "SomeNotExistingAssembly.dll");
            agg.Analyze(TestConstants.DependandLibV1Assembly);
            var assemblyReference = agg.AssemblyMatches.ToList();
            try
            {
                Assert.AreEqual(0, assemblyReference.Count);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (string reference in assemblyReference)
                    {
                        Console.WriteLine("The assembly {0} references {1}", TestConstants.DependandLibV1Assembly, reference);
                    }
                }
            }
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Do_Not_Accept_InvalidQuery()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoReferencesAssembly(agg, null);
        }
    }
}