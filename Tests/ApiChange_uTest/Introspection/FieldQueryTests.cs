
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;
using Mono.Cecil;
using System.Text.RegularExpressions;
using ApiChange.Infrastructure;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class FieldQueryTests
    {
        TypeDefinition myFieldClass;
        TypeDefinition myClassWithManyEventsAndMethods;

        [TestFixtureSetUp]
        public void GetTestClass()
        {
            myFieldClass = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.FieldQuery.PublicClassWithManyFields");
            myClassWithManyEventsAndMethods = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.FieldQuery.PublicClassWithManyEventsAndMethods");
        }

        string Value(Match m, string groupName)
        {
            return m.Groups[groupName].Value;
        }

        [Test]
        public void Parse_Static_Query()
        {
            var match = FieldQuery.FieldQueryParser.Match("static * m");

            Assert.IsTrue(match.Success, "Regex did not match");
            Assert.IsTrue(FieldQuery.AllFields.Captures(match, "static").Value, "static mach");
            Assert.AreEqual("*", Value(match, "fieldType"), "fieldType");
            Assert.AreEqual("m", Value(match, "fieldName"), "fieldName");
        }

        [Test]
        public void Parse_Static_ReadOnly_Query()
        {
            var match = FieldQuery.FieldQueryParser.Match(" static  readonly *  m_Member ");

            Assert.IsTrue(match.Success, "Regex did not match");
            Assert.IsTrue(FieldQuery.AllFields.Captures(match, "static").Value, "static match");
            Assert.IsTrue(FieldQuery.AllFields.Captures(match, "readonly").Value, "readonly match");
            Assert.AreEqual("*", Value(match, "fieldType"));
            Assert.AreEqual("m_Member", Value(match, "fieldName"), "fieldName");
        }

        [Test]
        public void Parse_Protected_Const_Query()
        {
            var match = FieldQuery.FieldQueryParser.Match(" protected const Func< int , bool > m_Member ");

            Assert.IsTrue(match.Success, "Regex did not match");
            Assert.IsTrue(FieldQuery.AllFields.Captures(match, "protected").Value, "protected match");
            Assert.IsTrue(FieldQuery.AllFields.Captures(match, "const").Value, "const match");
            Assert.AreEqual("Func< int , bool >", Value(match, "fieldType"));
            Assert.AreEqual("m_Member", Value(match, "fieldName"), "fieldName");
        }

        [Test]
        public void Parse_Private_Generic_Query()
        {
            var match = FieldQuery.FieldQueryParser.Match("private Func<Func<int>> m1_Member ");

            Assert.IsTrue(match.Success, "Regex did not match");
            Assert.IsTrue(FieldQuery.AllFields.Captures(match, "private").Value, "private match");
            Assert.AreEqual("Func<Func<int>>", Value(match, "fieldType"));
            Assert.AreEqual("m1_Member", Value(match, "fieldName"), "fieldName");
        }

        [Test]
        public void Parse_Public_Generic_Query()
        {
            var match = FieldQuery.FieldQueryParser.Match("public Func< Func<int> , Func<bool> > m1_Member ");

            Assert.IsTrue(match.Success, "Regex did not match");
            Assert.IsTrue(FieldQuery.AllFields.Captures(match, "public").Value, "public match");
            Assert.AreEqual("Func< Func<int> , Func<bool> >", Value(match, "fieldType"));
            Assert.AreEqual("m1_Member", Value(match, "fieldName"), "fieldName");
        }

        [Test]
        public void Get_All_Fields()
        {
            var query = new FieldQuery();
            var fields = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(14, fields.Count, "Field Count");
        }

        [Test]
        public void Get_All_Public_Fields()
        {
            var query = new FieldQuery("public * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(3, matches.Count, "Field count");
        }

        [Test]
        public void Get_All_Private_Fields()
        {
            var query = new FieldQuery("private * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(4, matches.Count, "Field count");
        }

        [Test]
        public void Get_All_Internal_Fields()
        {
            var query = new FieldQuery("internal * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(1, matches.Count, "Field count");
        }

        [Test]
        public void Get_All_ProtectedInternal_Fields()
        {
            var query = new FieldQuery("protected internal * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(3, matches.Count, "Field count");
        }

        [Test]
        public void Get_All_Static_Fields()
        {
            var query = new FieldQuery("static * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(4, matches.Count, "Field count");
        }

        [Test]
        public void Get_All_ReadOnly_Fields()
        {
            var query = new FieldQuery("readonly * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(3, matches.Count, "Field count");
        }

        [Test]
        public void Get_All_Const_Fields()
        {
            var query = new FieldQuery("const * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(4, matches.Count, "Field count");
        }

        [Test]
        public void Get_All_Static_Readonly_Fields()
        {
            var query = new FieldQuery("static readonly * *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(2, matches.Count, "Field count");
            Assert.AreEqual("protectedstaticreadonlyField", matches[0].Name, "Field name");
        }

        [Test]
        public void Get_All_GenericFields()
        {
            var query = new FieldQuery(" *> *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(2, matches.Count, "Field count");
        }

        [Test]
        public void Get_Public_Static_Fields_With_Partial_Type_Name()
        {
            var query = new FieldQuery("public static *Tim* *");
            var matches = query.GetMatchingFields(myFieldClass);

            Assert.AreEqual(1, matches.Count, "Field Count");
            Assert.AreEqual("publicStaticDateTimeField", matches[0].Name, "Field Name");
        }

        [Test]
        public void Get_Public_Static_Fields_With_Partial_Field_Name()
        {
            var query = new FieldQuery("public static * *Staticint");
            var matches = query.GetMatchingFields(myFieldClass);
            Assert.AreEqual(1, matches.Count, "Field Count");
            Assert.AreEqual("publicReadOnlyStaticint", matches[0].Name, "Field Name");
        }

        [Test]
        public void Get_All_NotPublic_Static_Fields()
        {
            var query = new FieldQuery("!public static * *");
            var matches = query.GetMatchingFields(myFieldClass);
            Assert.AreEqual(2, matches.Count, "Field Count");
        }

        [Test]
        public void Get_All_NotReadOnly_NotProtected_Fields()
        {
            var query = new FieldQuery("!readonly !protected * *");
            var matches = query.GetMatchingFields(myFieldClass);
            try
            {
                Assert.AreEqual(9, matches.Count, "Field Count");
            }
            finally
            {
                foreach (var field in matches)
                {
                    Console.WriteLine("Field: {0}", field.Print(FieldPrintOptions.All));
                }
            }
        }

        [Test]
        public void Get_All_Not_Const_Fields()
        {
            var query = new FieldQuery("!const * *");
            var matches = query.GetMatchingFields(myFieldClass);
            try
            {
                Assert.AreEqual(10, matches.Count, "Field Count");
            }
            finally
            {
                foreach (var field in matches)
                {
                    Console.WriteLine("Field: {0}", field.Print(FieldPrintOptions.All));
                }
            }

        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void Fail_On_Invalid_FieldQuery()
        {
            new FieldQuery("afdgd");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Fail_On_InvalidEmptyQuery()
        {
            new FieldQuery("");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void Fail_On_InvalidNullQuery()
        {
            new FieldQuery(null);
        }

        [Test]
        public void Ensure_That_Event_And_Property_Fields_Are_Not_Returned()
        {
            var matches = FieldQuery.AllFields.GetMatchingFields(myClassWithManyEventsAndMethods);
            try
            {
                Assert.AreEqual(2, matches.Count, "Fields.Count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var field in matches)
                    {
                        Console.WriteLine("{0}", field.Print(FieldPrintOptions.All));
                    }
                }
            }

        }
    }
}
