
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using ApiChange.Infrastructure;

namespace UnitTests.UsageQueries
{
    [TestFixture]
    public class WhoHasFieldOfTypeTests
    {
        [Test]
        public void Can_Find_CompilerGenerated_Generic_Field_Type()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(
                new TypeQuery("BaseLibrary.FieldQuery","PublicClassWithManyEventsAndMethods"));


            TypeDefinition func = TypeQuery.GetTypeByName(TestConstants.SystemCoreAssembly, "System.Func`1");
            new WhoHasFieldOfType(agg, func);

            try
            {
                agg.Analyze(TestConstants.BaseLibV1Assembly);

                Assert.AreEqual(3, agg.FieldMatches.Count, "Field match count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var res in agg.FieldMatches)
                    {
                        Console.WriteLine("Found field: {0}, file: {1}", res.Match.Print(FieldPrintOptions.All), res.SourceFileName);
                    }
                }
            }

        }

        [Test]
        public void Can_Find_Property_Backing_Field()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(
                new TypeQuery("BaseLibrary.FieldQuery", "PublicClassWithManyEventsAndMethods"));


            TypeDefinition func = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Int32");
            new WhoHasFieldOfType(agg, func);

            try
            {
                agg.Analyze(TestConstants.BaseLibV1Assembly);

                Assert.AreEqual(1, agg.FieldMatches.Count, "Field match count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var res in agg.FieldMatches)
                    {
                        Console.WriteLine("Found field: {0}, file: {1}", res.Match.Print(FieldPrintOptions.All), res.SourceFileName);
                    }
                }
            }

        }

        [Test]
        public void Can_Find_Non_Compilergenerated_Generic_Field()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(
                new TypeQuery("BaseLibrary.FieldQuery", "PublicClassWithManyEventsAndMethods"));


            TypeDefinition func = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Collections.Generic.KeyValuePair`2");
            new WhoHasFieldOfType(agg, func);

            try
            {
                agg.Analyze(TestConstants.BaseLibV1Assembly);

                Assert.AreEqual(1, agg.FieldMatches.Count, "Field match count");
                Assert.AreEqual("ProtectedKeyValuePairField", agg.FieldMatches[0].Match.Name, "Field Name");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var res in agg.FieldMatches)
                    {
                        Console.WriteLine("Found field: {0}, file: {1}", res.Match.Print(FieldPrintOptions.All), res.SourceFileName);
                    }
                }
            }
        }

        [Test]
        public void Can_Find_Field_Where_Type_Is_A_Generic_Parameter()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(
                new TypeQuery("BaseLibrary.FieldQuery", "PublicClassWithManyEventsAndMethods"));


            TypeDefinition decimalType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Decimal");
            new WhoHasFieldOfType(agg, decimalType);

            try
            {
                agg.Analyze(TestConstants.BaseLibV1Assembly);

                Assert.AreEqual(1, agg.FieldMatches.Count, "Field match count");
                Assert.AreEqual("myDecimalField", agg.FieldMatches[0].Match.Name, "Field Name");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var res in agg.FieldMatches)
                    {
                        Console.WriteLine("Found field: {0}, file: {1}", res.Match.Print(FieldPrintOptions.All), res.SourceFileName);
                    }
                }
            }
        }
    }
}
