
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
    class WhoUsesTypeTests : Trace_
    {
        [Test]
        public void Can_Find_Genericparameters_Of_Base_Interface()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("BaseLibrary.ApiChanges","IGenericInteface"));

            var floatType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Single");

            new WhoUsesType(agg, floatType);
            agg.Analyze(TestConstants.BaseLibV1Assembly);

            Assert.AreEqual(3, agg.TypeMatches.Count, "Type match count");
            Assert.AreEqual("BaseLibrary.ApiChanges.IGenericInteface", agg.TypeMatches[0].Match.FullName);
        }

        [Test]
        public void Can_Find_GenericParameters_Of_Base_Type()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassWhichDerivesFromGenericBaseClass"));

            var decimalType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Decimal");

            new WhoUsesType(agg, decimalType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.TypeMatches.Count, "Type match count");
            Assert.AreEqual("DependantLibV1.WhoUsesTypeInSignature.ClassWhichDerivesFromGenericBaseClass", agg.TypeMatches[0].Match.FullName);
        }

        [Test]
        public void Can_Find_GenericMethodInvocations_With_Type_Parameters()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "UsageClass"));

            var decimalType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Decimal");

            new WhoUsesType(agg, decimalType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(2, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual("UseGenericMethod", agg.MethodMatches[0].Match.Name);
            Assert.AreEqual("UseGenericMethod", agg.MethodMatches[1].Match.Name);
        }

        [Test]
        public void Can_Find_Type_As_Base_Class()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassDerivesFromException"));

            var exType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Exception");

            new WhoUsesType(agg, exType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.TypeMatches.Count, "Type match count");
            Assert.AreEqual("ClassDerivesFromException", agg.TypeMatches[0].Match.Name);
        }

        [Test]
        public void Can_Find_Type_As_Base_Interface()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassImplementsInterface"));

            var iDisposable = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IDisposable");

            new WhoUsesType(agg, iDisposable);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.TypeMatches.Count, "Type match count");
            Assert.AreEqual("ClassImplementsInterface", agg.TypeMatches[0].Match.Name);
        }

        [Test]
        public void Can_Find_Type_As_Field_Type()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassWithSearchedFieldType"));

            var charType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Char");

            new WhoUsesType(agg, charType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.FieldMatches.Count, "field match count");
            Assert.AreEqual("CharEvent", agg.FieldMatches[0].Match.Name);
        }

        [Test]
        public void Can_Find_Type_In_Method_Return_Type_In_Interface()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassWithGenericTypeArguments"));

            var dateTimeType = TypeQuery.GetTypeByName(
                TestConstants.MscorlibAssembly,
                "System.DateTime");

            new WhoUsesType(agg, dateTimeType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual("public virtual IEnumerator<DateTime> GetEnumerator()", agg.MethodMatches[0].Match.Print(MethodPrintOption.Full));
        }

        [Test]
        public void Can_Find_Type_In_Method_Argument_List()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "StructWithFunctionWithSearchedParameter"));

            var byteType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Byte");

            new WhoUsesType(agg, byteType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "method match count");
            Assert.AreEqual("FuncWithByteParamter", agg.MethodMatches[0].Match.Name);
        }

        [Test]
        public void Can_Find_Type_In_Method_Argument_List_Used_As_Generic_Parameter()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassWithFunctionWithGenericArguments"));

            var byteType = TypeQuery.GetTypeByName(TestConstants.DependandLibV1Assembly, "DependantLibV1.WhoUsesTypeInSignature.StructWithFunctionWithSearchedParameter");

            new WhoUsesType(agg, byteType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(1, agg.MethodMatches.Count, "method match count");
            Assert.AreEqual("FuncWithGenricMethodArgs", agg.MethodMatches[0].Match.Name);
        }

        [Test]
        public void Can_Find_Type_In_NonGeneric_Calls()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassWithUsingStatement"));

            var idisposable = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IDisposable");

            new WhoUsesType(agg, idisposable);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(2, agg.MethodMatches.Count, "method match count");
            Assert.AreEqual("FunctionWithUsingStatement", agg.MethodMatches[0].Match.Name);
            Assert.AreEqual("UsingDisposeableStruct", agg.MethodMatches[1].Match.Name);
        }

        List<QueryResult<MethodDefinition>> RemoveMatches(List<QueryResult<MethodDefinition>> list, string notReason)
        {
            return (from item in list
             where item.Annotations.Reason != notReason
             select item).ToList();
        }

        [Test]
        public void Can_Find_InstanceType_ConstructorCalls()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "InstantiateValueType"));

            var datetimetype = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.DateTime");
            var asyncflowcontroltype = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.Threading.AsyncFlowControl");

            new WhoUsesType(agg, new List<TypeDefinition> {datetimetype,asyncflowcontroltype});
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            List<QueryResult<MethodDefinition>> filtered = RemoveMatches(agg.MethodMatches, WhoUsesType.LocalVariableReason);

            Assert.AreEqual(3, filtered.Count, "method match count");
            Assert.AreEqual("InstantiateDateTime", filtered[0].Match.Name);
            Assert.AreEqual("InstantiateTypeInsideUsingStateMent", filtered[1].Match.Name);
        }

        [Test]
        public void Can_Find_Type_In_Constrained_Calls()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassWithUsingStatement"));

            var byteType = TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IDisposable");

            new WhoUsesType(agg, byteType);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(2, agg.MethodMatches.Count, "method match count");
            Assert.AreEqual("FunctionWithUsingStatement", agg.MethodMatches[0].Match.Name);
            Assert.AreEqual("UsingDisposeableStruct", agg.MethodMatches[1].Match.Name);
        }

        [Test]
        public void Can_Detect_Field_Read_Write_MethodCall()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "ClassWithUsingStatement"));

            new WhoUsesType(agg, TestConstants.PublicBaseClassTypeV1);
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(3, agg.MethodMatches.Count, "method match count");
            Assert.AreEqual("WriteToField", agg.MethodMatches[0].Match.Name);
            Assert.AreEqual("ReadFromField", agg.MethodMatches[1].Match.Name);
            Assert.AreEqual("CallFunctionFromBaseClass", agg.MethodMatches[2].Match.Name);
        }

        [Test]
        public void Can_Detect_Enum_Usage_In_Switch_Case()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("BaseLibrary.TypeUsageQuery", "SwitchOfEnumValues"));

            new WhoUsesType(agg, TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.StringSplitOptions"));
            agg.Analyze(TestConstants.BaseLibV1Assembly);

            Assert.AreEqual(2, agg.MethodMatches.Count, "method match count");
            Assert.AreEqual("UsingSwitchWithEnum", agg.MethodMatches[0].Match.Name);
        }

        [Test]
        public void Can_Detect_Type_Usage_In_Interface()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("BaseLibrary.TypeUsageQuery", "ITestInterface"));

            new WhoUsesType(agg, TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.String"));
            agg.Analyze(TestConstants.BaseLibV1Assembly);

            Assert.AreEqual(2, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual(WhoUsesType.UsedAsMethodReturnType, agg.MethodMatches[0].Annotations.Reason);
            Assert.AreEqual(WhoUsesType.UsedAsMethodParameterReason, agg.MethodMatches[1].Annotations.Reason);
        }

        [Test]
        public void Can_Find_TypeOfAndCast_Calls()
        {
            UsageQueryAggregator agg = new UsageQueryAggregator(new TypeQuery("DependantLibV1.WhoUsesTypeInSignature", "CastToTypeAndTypeof"));

            new WhoUsesType(agg, TypeQuery.GetTypeByName(TestConstants.MscorlibAssembly, "System.IDisposable"));
            agg.Analyze(TestConstants.DependandLibV1Assembly);

            Assert.AreEqual(2, agg.MethodMatches.Count, "Method match count");
            Assert.AreEqual(WhoUsesType.CastReason, agg.MethodMatches[0].Annotations.Reason);
            Assert.AreEqual(WhoUsesType.TypeOfReason, agg.MethodMatches[1].Annotations.Reason);
        }
    }
}
