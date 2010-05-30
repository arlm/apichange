
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
    public class WhoUsesMethodTests
    {
        [Test]
        public void Validate_That_Simple_Method_Calls_From_Local_Variable_Is_Found()
        {
            TypeDefinition baseClassMethod = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicBaseClass");
            var baseMethods = MethodQuery.AllMethods.GetMethods(baseClassMethod);

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoUsesMethod(agg,baseMethods);

            try
            {
                agg.Analyze(TestConstants.DependandLibV1Assembly);
                var results = agg.MethodMatches;
                Assert.AreEqual(8, results.Count);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var res in agg.MethodMatches)
                    {
                        Console.WriteLine("Got method call at {0} {1} {2}", res.Match.Print(MethodPrintOption.Full), 
                            res.SourceFileName, res.LineNumber);
                    }
                }

                agg.Dispose();
            }
        }

        [Test]
        public void Can_Differentiate_Methods_With_GenericParameters()
        {
            TypeDefinition baseClassMethod = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicBaseClass");
            var baseMethods = new MethodQuery("public void DoSomeThing(System.Collections.Generic.List<int> l)").GetMethods(baseClassMethod);

            Assert.AreEqual(1, baseMethods.Count, "Should get only one method with generic parameter");

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoUsesMethod(agg, baseMethods);

            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(2, agg.MethodMatches.Count, "Method match count");
            HashSet<string> methods = new HashSet<string>(from m in agg.MethodMatches
                                                          select m.Match.Print(MethodPrintOption.Full));
            Assert.IsTrue(methods.Contains("public void CallGenericIntFunc(PublicBaseClass cl)"));

            var methodWithFloatAsGenericParam = new MethodQuery("public void DoSomeThing(System.Collections.Generic.List<float> l)").GetMethods(baseClassMethod);
            Assert.AreEqual(1, methodWithFloatAsGenericParam.Count, "Did not find long function");

            agg.Dispose();

            agg = new UsageQueryAggregator();
            new WhoUsesMethod(agg, methodWithFloatAsGenericParam);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual("public void CallGenericFloatFunc(PublicBaseClass cl)", agg.MethodMatches[0].Match.Print(MethodPrintOption.Full));
        }

        [Test]
        public void Can_Find_Method_With_Generic_Type_Arguments()
        {
            TypeDefinition baseClassMethod = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicGenericClass`1");
            var baseMethods = new MethodQuery("public void GenericFunction<V>(V arg)").GetMethods(baseClassMethod);

            Assert.AreEqual(1, baseMethods.Count, "Should get only one method with generic parameter");

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoUsesMethod(agg, baseMethods);

            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual("public void CallRealGenericFunc()", agg.MethodMatches[0].Match.Print(MethodPrintOption.Full));
            agg.Dispose();
        }

        [Test]
        public void Can_Find_Method_With_Generic_ReturnType_And_Generic_Arguments()
        {
            TypeDefinition baseClassMethod = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicGenericClass`1");
            var baseMethods = new MethodQuery("public Func<U> GenericFunction<U, V>(U arg1, V arg2)").GetMethods(baseClassMethod);

            Assert.AreEqual(1, baseMethods.Count, "Should get only one method with generic parameter");

            UsageQueryAggregator agg = new UsageQueryAggregator();
            new WhoUsesMethod(agg, baseMethods);

            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual("public void CallGenericFuncWithGenericReturnType()", agg.MethodMatches[0].Match.Print(MethodPrintOption.Full));
            agg.Dispose();
        }


        [Test]
        public void Can_Find_Method_Of_Nested_Type()
        {
            TypeDefinition baseClassMethod = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.MethodQueries.NestedClass`1/InnerClass`1/InnerInnerClass");
            var baseMethods = new MethodQuery("* MethodOfNestedClass(*)").GetMethods(baseClassMethod);

            Assert.AreEqual(1, baseMethods.Count, "Should get only one method with generic parameter");

            UsageQueryAggregator agg = new UsageQueryAggregator(
                new TypeQuery("DependantLibV1.MethodUsage", "ClassWhichUsesNestedMethods"));
            new WhoUsesMethod(agg, baseMethods);

            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual("private void NowItBecomesInteresting()", agg.MethodMatches[0].Match.Print(MethodPrintOption.Full));
            agg.Dispose();
        }


        [Test]
        public void Can_Find_Method_Of_NonGeneric_Nested_Type()
        {
            TypeDefinition baseClassMethod = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.MethodQueries.NestedNonGenericClass/NestedInnerNonGenericClass/NestedInnerInnerNonGenericClass");
            var baseMethods = new MethodQuery("* MethodOfInnerMostNonGenericClass(*)").GetMethods(baseClassMethod);

            Assert.AreEqual(1, baseMethods.Count, "Should get only one method from nested class");

            UsageQueryAggregator agg = new UsageQueryAggregator(
                new TypeQuery("DependantLibV1.MethodUsage", "ClassWhichUsesNonGenericNestedClass"));
            new WhoUsesMethod(agg, baseMethods);

            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual("private void UsingMethod()", agg.MethodMatches[0].Match.Print(MethodPrintOption.Full));
            agg.Dispose();
        }


        [Test]
        public void Can_Find_Method_With_Out_Parameters()
        {
            TypeDefinition publicBaseClassType = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.ApiChanges.PublicBaseClass");
            MethodDefinition method = new MethodQuery("public bool CheckForUpdates(*)").GetSingleMethod(publicBaseClassType);

            using (UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.MethodUsage", "ClassWhichUsesMethods")))
            {
                new WhoUsesMethod(agg, new List<MethodDefinition> { method });
                agg.Analyze(TestConstants.DependandLibV1Assembly);

                Assert.AreEqual(1, agg.MethodMatches.Count);
                Assert.AreEqual("CallCheckForUpdates", agg.MethodMatches[0].Match.Name);
            }
        }
    }
}
