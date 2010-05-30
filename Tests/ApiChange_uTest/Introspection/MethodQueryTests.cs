
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.Cecil;
using ApiChange.Api.Introspection;
using System.Text.RegularExpressions;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class MethodQueryTests
    {
        AssemblyDefinition myAssembly;
        TypeDefinition myTestClass;
        TypeDefinition myClassWithMethodsAndEvents;

        [TestFixtureSetUp]
        public void LoadAssembly()
        {
            myAssembly = TestConstants.BaseLibV1Assembly;
            myTestClass = TypeQuery.GetTypeByName(myAssembly, "BaseLibrary.MethodQueries.ClassWithManyMethods");
            myClassWithMethodsAndEvents = TypeQuery.GetTypeByName(myAssembly, "BaseLibrary.MethodQueries.ClassWithMethodsAndEvents");
        }

        [Test]
        public void Parse_Args_With_No_Generics()
        {
            var match = new MethodQuery("void Func( int a, System.Int32 b, string c)");
            Assert.AreEqual(3, match.ArgumentFilters.Count);
            Assert.IsTrue(match.ArgumentFilters[0].Key.IsMatch("xxxSystem.Int32"));
            Assert.AreEqual("*a", match.ArgumentFilters[0].Value);
            Assert.IsTrue(match.ArgumentFilters[1].Key.IsMatch("System.Int32"));
            Assert.AreEqual("*b", match.ArgumentFilters[1].Value);
            Assert.IsTrue(match.ArgumentFilters[2].Key.IsMatch("System.String"));
            Assert.AreEqual("*c", match.ArgumentFilters[2].Value);
        }

        [Test]
        public void Parse_Args_With_Spaces()
        {
            MethodQuery query = new MethodQuery("void Function( int a ,  short  b, string  c   )");
            Assert.AreEqual(3, query.ArgumentFilters.Count, "Argument Filters");
            Assert.IsTrue(query.ArgumentFilters[0].Key.IsMatch("System.Int32"));
            Assert.AreEqual("*a", query.ArgumentFilters[0].Value);
            Assert.IsTrue(query.ArgumentFilters[1].Key.IsMatch("xxxSystem.Int16"));
            Assert.AreEqual("*b", query.ArgumentFilters[1].Value);
            Assert.IsTrue(query.ArgumentFilters[2].Key.IsMatch("System.String"));
            Assert.AreEqual("*c", query.ArgumentFilters[2].Value);
        }

        [Test]
        public void Parse_Args_With_Arrays()
        {
            MethodQuery query = new MethodQuery("int [] Function( int [] a ,  short  []b, string  [][]  c   )");
            Assert.AreEqual(3, query.ArgumentFilters.Count, "Argument Filters");
            Assert.IsTrue(query.ArgumentFilters[0].Key.IsMatch(".*System.Int32[]"));
            Assert.AreEqual("*a", query.ArgumentFilters[0].Value);
            Assert.IsTrue(query.ArgumentFilters[1].Key.IsMatch(".*System.Int16[]"));
            Assert.AreEqual("*b", query.ArgumentFilters[1].Value);
            Assert.IsTrue(query.ArgumentFilters[2].Key.IsMatch(".*System.String[][]"));
            Assert.AreEqual("*c", query.ArgumentFilters[2].Value);
            Assert.AreEqual(".*int\\[]", query.ReturnTypeFilter.ToString());
        }

        [Test]
        public void Parse_Args_With_Nested_Generics()
        {
            MethodQuery query = new MethodQuery("void Func<int>(Func<int> a, Func<Func<int , bool >   , byte > b, Func<int,Func<int,int>> c )");
            Assert.AreEqual(3, query.ArgumentFilters.Count, "Argument Filters");
            Assert.IsTrue(query.ArgumentFilters[0].Key.IsMatch("Func`1<System.Int32>"));
            Assert.AreEqual("*a", query.ArgumentFilters[0].Value);
            Assert.IsTrue(query.ArgumentFilters[1].Key.IsMatch("*.Func`2<.*Func`2<.*System.Int32,.*System.Boolean>,.*System.Byte>"));
            Assert.AreEqual("*b", query.ArgumentFilters[1].Value);
            Assert.IsTrue(query.ArgumentFilters[2].Key.IsMatch(".*Func`2<.*System.Int32,Func`2<.*System.Int32,.*System.Int32>>"));
            Assert.AreEqual("*c", query.ArgumentFilters[2].Value);
            Assert.AreEqual("Func", query.NameFilter);
        }

        bool IsMatch(Match m, string groupName)
        {
            return m.Groups[groupName].Success;
        }

        string Value(Match m, string groupName)
        {
            return m.Groups[groupName].Value;
        }

        MethodQuery query = new MethodQuery();

        [Test]
        public void Parse_Method_With_No_Generics()
        {
            var match = MethodQuery.MethodDefParser.Match("public static void Function(  int a , System.Int32  base, string c )  ");
            Assert.IsTrue(match.Success, "Method Parser regex should match");

            Assert.IsTrue(MethodQuery.AllMethods.Captures(match, "public").Value, "public should match");
            Assert.IsTrue(MethodQuery.AllMethods.Captures(match, "static").Value, "static shold match");
            Assert.AreEqual("void", Value(match, "retType"), "Return type should be void");
            Assert.AreEqual("Function", Value(match, "funcName"), "Function name should match");
            Assert.AreEqual("int a , System.Int32  base, string c", Value(match, "args"), "Function arguments");
        }

        [Test]
        public void Parse_Method_With_Nested_Generics()
        {
            var match = MethodQuery.MethodDefParser.Match("public static Func< int, System.Int32, Func<bool>> Function<bool>(  Func<int, int> a , Action<Action<Action<int>>>  base, string c )  ");
            Assert.IsTrue(match.Success, "Method Parser regex should match");

            Assert.IsTrue(MethodQuery.AllMethods.Captures(match, "public").Value, "public should match");
            Assert.IsTrue(MethodQuery.AllMethods.Captures(match, "static").Value, "static shold match");
            Assert.AreEqual("Func< int, System.Int32, Func<bool>>", Value(match, "retType"), "Return type should be Func< int, System.Int32, Func<bool>>");
            Assert.AreEqual("Function<bool>", Value(match, "funcName"), "Function name should match");
            Assert.AreEqual("Func<int, int> a , Action<Action<Action<int>>>  base, string c", Value(match, "args"), "Function arguments");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid_MethodQuery()
        {
            var query = new MethodQuery("blahh");
        }

        [Test]
        public void Query_For_Public_Methods()
        {
            MethodQuery query = new MethodQuery("public * *( * )"); // match all public methods");

            Assert.AreEqual("*", query.NameFilter, "Name filter is none");
            Assert.AreEqual(".*", query.ReturnTypeFilter.ToString(), "Return filter is none");
            Assert.IsNull(query.ArgumentFilters, "Argument filter is none");
        }

        [Test]
        public void Query_For_Protected_Methods()
        {
            // match all protected methods with one parameter
            MethodQuery query = new MethodQuery("protected * *(* a)");

            Assert.AreEqual("*", query.NameFilter, "NameFilter");
            Assert.AreEqual(".*", query.ReturnTypeFilter.ToString(), "ReturnTypeFilter");
            Assert.AreEqual(1, query.ArgumentFilters.Count, "Argument filter");
            Assert.AreEqual(".*", query.ArgumentFilters[0].Key.ToString(), "Argument type filter");
            Assert.AreEqual("*a", query.ArgumentFilters[0].Value, "Argument name filter");
        }

        [Test]
        public void Get_Public_Void_Methods()
        {
            MethodQuery query = new MethodQuery("public void *()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(7, matches.Count);
            Assert.AreEqual("PublicVoid1", matches[0].Name);
        }

        [Test]
        public void Get_Interal_Void_Methods()
        {
            MethodQuery query = new MethodQuery("internal void *()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(4, matches.Count);
            Assert.AreEqual("InternalVoid1", matches[0].Name);
        }


        [Test]
        public void Get_Method_With_ArgumentArray_And_Return_Type()
        {
            MethodQuery query = new MethodQuery("string GetString(byte[] bytes)");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("GetString", matches[0].Name);
        }

        [Test]
        public void Get_Method_With_GenericArgument_With_Partial_TypeNames()
        {
            MethodQuery query = new MethodQuery("void DisposeToolsHandlers(IList<Exception> exceptions_in_out)");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("DisposeToolsHandlers", matches[0].Name);

        }

        [Test]
        public void Get_Method_With_GenericReturnType_With_Partial_TypeNames()
        {
            MethodQuery query = new MethodQuery("IList<Exception> MethodWithGenericReturnType()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("MethodWithGenericReturnType", matches[0].Name);
        }

        [Test]
        public void Get_Protected_Void_Methods()
        {
            MethodQuery query = new MethodQuery("protected void *()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(4, matches.Count);
            Assert.AreEqual("ProtectedVoid1", matches[0].Name);
        }

        [Test]
        public void Get_Private_Void_Methods()
        {
            MethodQuery query = new MethodQuery("private void *()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(4, matches.Count);
            Assert.AreEqual("PrivateVoid1", matches[0].Name);
        }

        [Test]
        public void Get_Protected_Interal_Void_Methods()
        {
            MethodQuery query = new MethodQuery("protected internal void *()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(4, matches.Count);
            Assert.AreEqual("ProtectedInteralVoid1", matches[0].Name);
        }

        [Test]
        public void Get_Public_Static_Void_Methods()
        {
            MethodQuery query = new MethodQuery("public static void *()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("PublicStaticVoid1", matches[0].Name);
        }

        [Test]
        public void Get_Internal_Static_Void_Methods()
        {
            MethodQuery query = new MethodQuery("internal static void *()");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("InternalStaticVoid1", matches[0].Name);
        }

        [Test]
        public void Get_Private_Static_Void1()
        {
            MethodQuery query = new MethodQuery(" private  static  void *( ) ");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("PrivateStaticVoid1", matches[0].Name);
        }

        [Test]
        public void Get_PublicVirtual_Void_Methods()
        {
            MethodQuery query = new MethodQuery("public virtual void * ( ) ");
            var matches = query.GetMethods(myTestClass);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("PublicVirtualVoid1", matches[0].Name);
        }

        [Test]
        public void Get_ProtectedVirtual_Void_Methods()
        {
            MethodQuery query = new MethodQuery("protected virtual void *() ");
            var matches = query.GetMethods(myTestClass);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("ProtectedVirtualVoid1", matches[0].Name);
        }

        [Test]
        public void Get_ProtectedInternalVirtual_Void_Methods()
        {
            MethodQuery query = new MethodQuery("protected internal virtual void *() ");
            var matches = query.GetMethods(myTestClass);
            Assert.AreEqual(2, matches.Count);
            Assert.AreEqual("ProtectedInteralVirtualVoid1", matches[0].Name);
        }

        [Test]
        public void Get_Generic_Method()
        {
            MethodQuery query = new MethodQuery("public Func<int> GenericMethod<T>(T a, int b)");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(1, matches.Count);
            Assert.AreEqual("GenericMethod", matches[0].Name);
        }

        [Test]
        public void Get_Generic_Method_Fails_If_Type_Is_Different()
        {
            MethodQuery query = new MethodQuery("public Func<int> GenericMethod<T>(T a, string b)");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(0, matches.Count);
        }


        [Test]
        public void Get_Generic_Method_Fails_If_Generic_Type_Is_Different()
        {
            MethodQuery query = new MethodQuery("public Func<int> GenericMethod<T>(B a, int b)");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void Get_Generic_Method_Fails_If_ParameterName_Is_Different()
        {
            MethodQuery query = new MethodQuery("public Func<int> GenericMethod<T>(T b, int b)");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void Get_Generic_Method_Fails_If_ParameterCount_Is_Different()
        {
            MethodQuery query = new MethodQuery("public Func<int> GenericMethod<T>(T a)");
            var matches = query.GetMethods(myTestClass);

            Assert.AreEqual(0, matches.Count);
        }

        [Test]
        public void Get_All_Non_Public_Methods()
        {
            MethodQuery query = new MethodQuery("!public * *( * )");
            var matches = query.GetMethods(myTestClass);
            Assert.AreEqual(19, matches.Count, "Non public methods");
        }

        [Test]
        public void Get_Method_By_Name()
        {
            MethodQuery query = new MethodQuery("* * ProtectedVoid1( * )");
            var matches = query.GetMethods(myTestClass);
            Assert.AreEqual(1, matches.Count);
        }

        [Test]
        public void Get_All_PublicNonVirtual_Methods()
        {
            MethodQuery query = new MethodQuery("public !virtual * *( * )");
            var matches = query.GetMethods(myTestClass);
            Assert.AreEqual(6, matches.Count, "All Public, Non Virtual methods");
        }

        [Test]
        public void Do_Not_Get_EventMethods_y()
        {
            var methods = MethodQuery.AllMethods.GetMethods(myClassWithMethodsAndEvents);
            Assert.AreEqual(4, methods.Count, "No events should show up in the method queries");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invalid_Empty_Throws()
        {
            new MethodQuery("");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Invalid_Null_Throws()
        {
            new MethodQuery(null);
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid_Unbalanced_Braces_Throws()
        {
            new MethodQuery("* F(");
        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Invalid_Missing_Return_Type_Throws()
        {
            var query = new MethodQuery("Func()");
        }

        [Test]
        public void Can_Get_Constructor_By_Name()
        {
            MethodQuery query = new MethodQuery("* ClassWithManyMethods()");
            var matches = query.GetMethods(myTestClass);
            Assert.AreEqual(1, matches.Count, "Should be able to get default ctor");
        }
    }
}
