
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using System.IO;
using ApiChange.Infrastructure;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class TypeQueryTests
    {
        const string TypeQueryNS = "BaseLibrary.TypeQuery";

        [Test]
        public void Load_Internal_Interfaces()
        {
            TypeQuery query = new TypeQuery(TypeQueryMode.Internal | TypeQueryMode.Interface, TypeQueryNS);
            List<TypeDefinition> types = query.GetTypes(TestConstants.BaseLibV1Assembly);
            Assert.AreEqual(1, types.Count);
            Assert.AreEqual(TestConstants.BaseLibV1Interface1Internal, types[0].FullName);
        }

        [Test]
        public void Load_Internal_Classes()
        {
            TypeQuery query = new TypeQuery(TypeQueryMode.Internal | TypeQueryMode.Class, TypeQueryNS);
            List<TypeDefinition> types = query.GetTypes(TestConstants.BaseLibV1Assembly).SortByTypeName();

            try
            {
                Assert.AreEqual(2, types.Count);
                Assert.AreEqual("BaseLibrary.TypeQuery.ClassInternal", types[0].FullName);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var t in types)
                        Console.WriteLine("Got {0}", t.FullName);
                }
            }
        }

        [Test]
        public void Load_Internal_NotCompilerGenerated_Classes_Without_NameSpace()
        {
            TypeQuery query = new TypeQuery(
                TypeQueryMode.Internal | TypeQueryMode.Class ,
                "");

            var types = query.GetTypes(TestConstants.BaseLibV1Assembly);
            try
            {
                Assert.AreEqual(1, types.Count);
                Assert.AreEqual("<Module>", types[0].FullName);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var t in types) Console.WriteLine("Got: {0}", t);
                }
            }

            query.SearchMode |= TypeQueryMode.NotCompilerGenerated;

            var typesWithOutCompilerGeneratedClasses = query.GetTypes(TestConstants.BaseLibV1Assembly);

            try
            {
                Assert.AreEqual(0, typesWithOutCompilerGeneratedClasses.Count);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var t in typesWithOutCompilerGeneratedClasses) Console.WriteLine("Got: {0}", t);
                }
            }
        }

        [Test]
        public void Load_Internal_Valuetypes_NotCompilierGenerated()
        {
            TypeQuery query = new TypeQuery(
                TypeQueryMode.Internal | TypeQueryMode.ValueType | TypeQueryMode.NotCompilerGenerated | TypeQueryMode.Enum,
                TypeQueryNS);

            var types = query.GetTypes(TestConstants.BaseLibV1Assembly).SortByTypeName();
            Assert.AreEqual(2, types.Count);
            Assert.AreEqual("BaseLibrary.TypeQuery.EnumInternal", types[0].FullName);
            Assert.AreEqual("BaseLibrary.TypeQuery.StructInternal", types[1].FullName);
        }

        [Test]
        public void Load_Public_Interfaces_From_Namespace()
        {
            var query = new TypeQuery(TypeQueryMode.Public|TypeQueryMode.Interface, TypeQueryNS+"*");
            List<TypeDefinition> types = query.GetTypes(TestConstants.BaseLibV1Assembly);
            Assert.AreEqual(2, types.Count);
        }

        [Test]
        public void Get_Generic_Type_With_Three_GenericArguments()
        {
            var query = new TypeQuery(TestConstants.TypeEquivalenceNS, "Class1<A,B,C>");
            var genericTypes = query.GetTypes(TestConstants.BaseLibV1Assembly);
            Assert.AreEqual(1, genericTypes.Count, "Should get one instance");
        }

        [Test]
        public void Get_Generic_Type_With_One_Generic_Argument()
        {
            var query = new TypeQuery(TestConstants.TypeEquivalenceNS, "Class1<A>");
            var genericTypes = query.GetTypes(TestConstants.BaseLibV1Assembly);
            Assert.AreEqual(1, genericTypes.Count, "Should get one instance");
        }

        [Test]
        public void Should_Get_All_Generic_And_NonGeneric_Types()
        {
            var query = new TypeQuery(TestConstants.TypeEquivalenceNS, "Class1*");
            var genericTypes = query.GetTypes(TestConstants.BaseLibV1Assembly);
            Assert.AreEqual(3, genericTypes.Count, "Should get all instances");
        }


        [Test]
        public void Load_Public_Interface_With_Exact_Namespace_Match()
        {
            var query = new TypeQuery(TypeQueryMode.Public | TypeQueryMode.Interface, TypeQueryNS + ".SubNs");
            AssemblyDefinition assembly = TestConstants.BaseLibV1Assembly;

            List<TypeDefinition> types = query.GetTypes(assembly);
            Assert.AreEqual(1, types.Count);
            Assert.AreEqual("BaseLibrary.TypeQuery.SubNs.PublicInterfaceInLowerNS", types[0].FullName);
        }

        [Test]
        public void Load_API_Relevant_Types()
        {
            var publicTypes = new TypeQuery(TypeQueryMode.ApiRelevant).GetTypes(TestConstants.BaseLibV1Assembly);

            Console.WriteLine("Got {0} types", publicTypes.Count);
            foreach (var type in publicTypes)
            {
                Console.WriteLine("\t{0}", type);
            }

            Assert.AreEqual(21, publicTypes.Count, "BaseLibraryV1 should only contain these public types");

            int interfaceCount = publicTypes.Count( (type) => type.IsInterface );
            Console.WriteLine("Got {0} public interfaces", interfaceCount);
            Assert.AreEqual(4, interfaceCount, "BaseLibraryV1 has only these public interfaces");
        }

        [Test]
        public void Can_Query_For_Nested_Type()
        {
            var query = new TypeQuery("BaseLibrary.TypeQuery", "*NestedClass*");
            var nestedTypes = query.GetTypes(TestConstants.BaseLibV1Assembly);
            Assert.AreEqual(1, nestedTypes.Count, "Should get nested type.");

        }
    }
}
