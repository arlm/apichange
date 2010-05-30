
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Introspection;
using ApiChange.Infrastructure;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class TypeDiffTests
    {
        QueryAggregator myQueries;

        [TestFixtureSetUp]
        public void GenerateSelectiveQueries()
        {
            myQueries = new QueryAggregator();

            myQueries.MethodQueries.Add(MethodQuery.PublicMethods);
            myQueries.MethodQueries.Add(MethodQuery.ProtectedMethods);

            myQueries.FieldQueries.Add(FieldQuery.PublicFields);
            myQueries.FieldQueries.Add(FieldQuery.ProtectedFields);

            myQueries.EventQueries.Add(EventQuery.PublicEvents);
            myQueries.EventQueries.Add(EventQuery.ProtectedEvents);
        }


        [Test]
        public void DiffTypeWithNoChanges()
        {
            var simpleV1 = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.TypeDiff.SimpleFieldClass");
            var simpleV2 = TypeQuery.GetTypeByName(TestConstants.BaseLibV2Assembly, "BaseLibrary.TypeDiff.SimpleFieldClass");

            TypeDiff diff = TypeDiff.GenerateDiff(simpleV1, simpleV1, myQueries);
            Assert.AreEqual(TypeDiff.None, diff, "None object should be returned for empty diff");
            Assert.IsFalse(diff.HasChangedBaseType, "No Base Type change");
            Assert.AreEqual(0, diff.Events.Count, "Event count");
            Assert.AreEqual(0, diff.Fields.RemovedCount, "Field remove count");
            Assert.AreEqual(0, diff.Fields.AddedCount, "Field add count");
            Assert.AreEqual(0, diff.Interfaces.Count, "Interface changes");
            Assert.AreEqual(0, diff.Methods.Count, "Method changes");
        }

        [Test]
        public void DiffTypeWithOnlyFieldChanges()
        {
            var simpleV1 = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.TypeDiff.SimpleFieldClass");
            var simpleV2 = TypeQuery.GetTypeByName(TestConstants.BaseLibV2Assembly, "BaseLibrary.TypeDiff.SimpleFieldClass");

            TypeDiff diff = TypeDiff.GenerateDiff(simpleV1, simpleV2, myQueries);
            Assert.IsFalse(diff.HasChangedBaseType, "No Base Type change");
            Assert.AreEqual(0, diff.Events.Count, "Event count");
            Assert.AreEqual(5, diff.Fields.RemovedCount, "Field remove count");
            Assert.AreEqual(6, diff.Fields.AddedCount, "Field add count");
            Assert.AreEqual(0, diff.Interfaces.Count, "Interface changes");
            Assert.AreEqual(0, diff.Methods.Count, "Method changes");
        }

        [Test]
        public void DiffMethods()
        {
            var simpleV1 = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.TypeDiff.MethodClass");
            var simpleV2 = TypeQuery.GetTypeByName(TestConstants.BaseLibV2Assembly, "BaseLibrary.TypeDiff.MethodClass");

            TypeDiff diff = TypeDiff.GenerateDiff(simpleV1, simpleV2, myQueries);
            try
            {
                Assert.AreEqual(12, diff.Methods.RemovedCount, "Removed methods count");
                Assert.AreEqual(7, diff.Methods.AddedCount, "Added methods");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    Console.WriteLine("Removed methods");
                    foreach (var method in diff.Methods.Removed)
                    {
                        Console.WriteLine("{0}", method.ObjectV1.Print(MethodPrintOption.Full));
                    }

                    Console.WriteLine("Added methods");
                    foreach (var method in diff.Methods.Added)
                    {
                        Console.WriteLine("{0}", method.ObjectV1.Print(MethodPrintOption.Full));
                    }
                }
            }
        }

        [Test]
        public void DiffEvents()
        {
            var simpleV1 = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.TypeDiff.EventClass");
            var simpleV2 = TypeQuery.GetTypeByName(TestConstants.BaseLibV2Assembly, "BaseLibrary.TypeDiff.EventClass");

            TypeDiff diff = TypeDiff.GenerateDiff(simpleV1, simpleV2, myQueries);
            try
            {
                Assert.AreEqual(7, diff.Events.RemovedCount, "Removed events");
                Assert.AreEqual(10, diff.Events.AddedCount, "Added events");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    Console.WriteLine("Removed events");
                    foreach (var ev in diff.Events.Removed)
                    {
                        Console.WriteLine("{0}", ev.ObjectV1.Print());
                    }

                    Console.WriteLine("Added events");
                    foreach (var ev in diff.Events.Added)
                    {
                        Console.WriteLine("{0}", ev.ObjectV1.Print());
                    }
                }
            }
        }

        [Test]
        public void DiffBaseClassAndInmplementedInterfaces()
        {
            var simpleV1 = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.TypeDiff.ClassWithInterfacesAndBaseClass");
            var simpleV2 = TypeQuery.GetTypeByName(TestConstants.BaseLibV2Assembly, "BaseLibrary.TypeDiff.ClassWithInterfacesAndBaseClass");

            TypeDiff diff = TypeDiff.GenerateDiff(simpleV1, simpleV2, myQueries);
            try
            {
                Assert.IsTrue(diff.HasChangedBaseType, "Base class type has changed");
                Assert.AreEqual("EventArgs", diff.TypeV1.BaseType.Name, "Base Type V1");
                Assert.AreEqual("ResolveEventArgs", diff.TypeV2.BaseType.Name, "Base Type V2");

                Assert.AreEqual(2, diff.Interfaces.RemovedCount, "Removed interfaces");
                Assert.AreEqual(1, diff.Interfaces.AddedCount, "Added interfaces");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    Console.WriteLine("Removed interfaces");
                    foreach (var remItf in diff.Interfaces.Removed)
                    {
                        Console.WriteLine("{0}", remItf.ObjectV1.FullName);
                    }
                    Console.WriteLine("Added interfaces");
                    foreach (var addItf in diff.Interfaces.Added)
                    {
                        Console.WriteLine("{0}", addItf.ObjectV1.FullName);
                    }

                }
            }
        }

        [Test]
        public void DiffBaseClassWithChangeInGenericTypeArgs()
        {
            var simpleV1 = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.TypeDiff.ClassWithGenericBase");
            var simpleV2 = TypeQuery.GetTypeByName(TestConstants.BaseLibV2Assembly, "BaseLibrary.TypeDiff.ClassWithGenericBase");

            TypeDiff diff = TypeDiff.GenerateDiff(simpleV1, simpleV2, myQueries);
            Assert.IsTrue(diff.HasChangedBaseType, "Base Type has changed generic argument");
        }

        [Test]
        public void DiffWithItselfMustReturnEmptyDiff()
        {
            var simpleV1 = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly, "BaseLibrary.TypeDiff.ClassWithGenericBase");

            TypeDiff diff = TypeDiff.GenerateDiff(simpleV1, simpleV1, myQueries);
            Assert.IsFalse(diff.HasChangedBaseType, "Base Type has changed generic argument");
            Assert.AreEqual(0, diff.Events.Count);
            Assert.AreEqual(0, diff.Fields.Count);
            Assert.AreEqual(0, diff.Interfaces.Count);
            Assert.AreEqual(0, diff.Methods.Count);
        }

    }
}