
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
    public class ExtensionTests
    {
        [Test]
        public void PrintVariousFields()
        {
            TypeDefinition publicClassWithManyFields = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, TestConstants.PublicClassWithManyFields);

            List<string> myFieldDefinitions = new List<string>
            {
                "private int privateIntField",
                "private List<string> privateListField",
                "private const int privateConstant - Value: 9",
                "internal readonly int internalReadOnyInt",
                "public static readonly int publicReadOnlyStaticint",
                "public static int publicStaticIntField",
                "public const int publicConstStaticInt - Value: 999",
                "protected int protectedIntField",
                "protected internal int protectedInternalField",
                "protected const int protectedstaticconstField - Value: 879",
                "private List<string> privateListField1",
                "private List<string> privateListField2",
                "protected static readonly int protectedstaticreadonlyField",
                "protected internal int protectedInternalField",
                "protected internal static int protectedInternalStaticField",
                "public static System.DateTime publicStaticDateTimeField",
                "protected internal const string protectedInternalConstString - Value: Test Value"
            };

            foreach (FieldDefinition field in publicClassWithManyFields.Fields)
            {
                string fieldStr = field.Print(FieldPrintOptions.All);
                Console.WriteLine(fieldStr);
                Assert.IsTrue(myFieldDefinitions.Contains(fieldStr),
                    String.Format("Got field string: #{0}# which is not part of required string list", fieldStr));
            }
        }

        [Test]
        public void PrintGenericType()
        {
            TypeDefinition publicClassWithManyFields = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, TestConstants.PublicClassWithManyFields);

            Console.WriteLine("Generic Type: {0} Print: {1}", publicClassWithManyFields.ToString(), publicClassWithManyFields.Print());
        }

        [Test]
        public void PrintGenericFieldType()
        {
            TypeDefinition publicClassWithManyFields = TestConstants.GetGenericClass(TestConstants.BaseLibV1Assembly);

            foreach (FieldDefinition f in publicClassWithManyFields.Fields)
            {
                Console.WriteLine("Field Cecil Type: {0}", f.FieldType.GetType());
            }

            var field = publicClassWithManyFields.Fields.GetField("m_Field");

            Console.WriteLine("Generic Type {0} with field m_Field: {1}", publicClassWithManyFields.Print(), field.Print(FieldPrintOptions.All));

            field = publicClassWithManyFields.Fields.GetField("m_partialGenericField");
            Console.WriteLine("Generic Type {0} with field m_Field: {1}", publicClassWithManyFields.Print(), field.Print(FieldPrintOptions.All));

        }

        [Test]
        public void PrintVariousEvents()
        {
            TypeDefinition classWithManyEvents = TypeQuery.GetTypeByName(
                TestConstants.BaseLibV1Assembly,
                "BaseLibrary.EventQueries.ClassWithManyEvents");

            string[] evDefinitions = new string[] 
            {
                "public event Func<int> PublicEvent",
                "public event Func<bool> PublicEvent2",
                "protected event Func<int> ProtectedEvent",
                "internal event Func<int> InternalEvent",
                "private event System.Action PrivateEvent",
                "public virtual event Func<int> PublicVirtualEvent",
                "public static event System.Action PublicStaticEvent",
                "private event EventHandler<EventArgs> SceneChanged"
            };

            foreach (EventDefinition ev in classWithManyEvents.Events)
            {
                string evStr = ev.Print();
                Console.WriteLine("{0}", evStr);
                Assert.IsTrue(evDefinitions.Contains(evStr), "Event string should be part of list");
            }
        }

        [Test]
        public void PrintVariousMethods()
        {
            TypeDefinition classWithMetods = TypeQuery.GetTypeByName(
                TestConstants.BaseLibV1Assembly,
                "BaseLibrary.Extensions.ClassWithGenericMethodArgs");

            foreach (MethodDefinition method in classWithMetods.Methods)
            {
                Console.WriteLine("{0}", method.Print(MethodPrintOption.Full));
            }

        }
    }
}
