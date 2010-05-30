
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class TypeQueryFactoryTests
    {
        [Test]
        public void Empty_Type_Query_Throws_ArgumentException()
        {
            Assert.Throws<ArgumentException>(() => new TypeQueryFactory().GetQueries(""));
        }

        [Test]
        public void Null_Type_Query_Throws_ArgumentNullException()
        {
            Assert.Throws<ArgumentNullException>(() => new TypeQueryFactory().GetQueries(null));
        }

        [Test]
        public void Can_Split_Queries()
        {
            var queries = new TypeQueryFactory().GetQueries(" public class someClass;interface blah", TypeQueryMode.ApiRelevant);
            Assert.AreEqual(2, queries.Count);
        }

        [Test]
        public void One_QueryString_Results_In_Only_One_QueryObject()
        {
            var queries = new TypeQueryFactory().GetQueries(" public interface  struct class   someClass.NameSpace.*  ");
            Assert.AreEqual(1, queries.Count);
        }

        [Test]
        public void Split_Type_NS_Is_Null_When_Type_Has_No_Prefix()
        {
            TypeQueryFactory fac = new TypeQueryFactory();
            KeyValuePair<string, string> nsType = fac.SplitNameSpaceAndType("TypeName");
            Assert.IsNull(nsType.Key, "Namespace should be null");
            Assert.AreEqual("TypeName", nsType.Value, "Splitted value should be there");
        }

        [Test]
        public void Split_Type_Can_Split_Full_Qualified_Type_Into_NS_And_Type()
        {
            TypeQueryFactory fac = new TypeQueryFactory();
            KeyValuePair<string, string> nsType = fac.SplitNameSpaceAndType("System.Diagnostics.Debug");
            Assert.AreEqual("System.Diagnostics", nsType.Key);
            Assert.AreEqual("Debug", nsType.Value);
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Split_Type_Throws_On_Empty_Type()
        {
            TypeQueryFactory fac = new TypeQueryFactory();
            fac.SplitNameSpaceAndType("");
        }

        [Test]
        public void When_Only_TypeName_Is_Given_Filter_For_All_Types()
        {
            TypeQueryFactory fac = new TypeQueryFactory();
            var queries = fac.GetQueries("typeName");
            Assert.AreEqual(1, queries.Count);
            TypeQuery tq = queries[0];
            Assert.IsNull(tq.NamespaceFilter);
            Assert.AreEqual(TypeQueryMode.All, tq.SearchMode);
            Assert.AreEqual("typeName", tq.TypeNameFilter);
        }

        [Test]
        public void Fail_When_Invalid_Modifier_With_TypeName_IsEntered()
        {
            TypeQueryFactory fac = new TypeQueryFactory();
            Assert.Throws<ArgumentException>(() => fac.GetQueries("xxx typeName"));
        }

        [Test]
        public void Can_Query_For_Generic_Type()
        {
            TypeQueryFactory fac = new TypeQueryFactory();
            TypeQuery tq = fac.GetQueries("IEnumerable<string>")[0];
            var matchingTypes = tq.GetTypes(TestConstants.MscorlibAssembly);
        }
    }
}