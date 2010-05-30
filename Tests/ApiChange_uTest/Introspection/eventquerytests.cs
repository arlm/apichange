
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using Mono.Cecil;
using ApiChange.Api.Introspection;
using ApiChange.Infrastructure;

namespace UnitTests.Introspection
{
    [TestFixture]
    public class EventQueryTests
    {
        TypeDefinition myClassWithManyEvents;

        [TestFixtureSetUp]
        public void LoadTestClass()
        {
            myClassWithManyEvents = TypeQuery.GetTypeByName(TestConstants.BaseLibV1Assembly,
                                            "BaseLibrary.EventQueries.ClassWithManyEvents");
        }

        [Test]
        public void GetAllPublicEvents()
        {
            EventQuery query = new EventQuery("public * *");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            try
            {
                Assert.AreEqual(4, events.Count, "Event count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var ev in events)
                    {
                        Console.WriteLine("{0}", ev.Print());
                    }
                }
            }
        }

        [Test]
        public void GetAllNonStaticEvents()
        {
            EventQuery query = new EventQuery("!static * *");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            try
            {
                Assert.AreEqual(7, events.Count, "Event count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var ev in events)
                    {
                        Console.WriteLine("{0}", ev.Print());
                    }
                }
            }
        }

        [Test]
        public void GetAllProtectedEvents()
        {
            EventQuery query = new EventQuery("protected * *");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            try
            {
                Assert.AreEqual(1, events.Count, "Event count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var ev in events)
                    {
                        Console.WriteLine("{0}", ev.Print());
                    }
                }
            }
        }

        [Test]
        public void GetEventByName()
        {
            EventQuery query = new EventQuery("* PublicEvent2");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            try
            {
                Assert.AreEqual(1, events.Count, "Event count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var ev in events)
                    {
                        Console.WriteLine("{0}", ev.Print());
                    }
                }
            }
        }

        [Test]
        public void GetEventByType()
        {
            EventQuery query = new EventQuery("Func<bool> *");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            try
            {
                Assert.AreEqual(1, events.Count, "Event count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var ev in events)
                    {
                        Console.WriteLine("{0}", ev.Print());
                    }
                }
            }
        }

        [Test]
        public void GetEvent_By_Type_And_Name()
        {
            EventQuery query = new EventQuery("EventHandler<EventArgs> SceneChanged");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            Assert.AreEqual(1, events.Count);

        }

        [Test]
        [ExpectedException(typeof(ArgumentException))]
        public void InvalidEventQuery()
        {
            new EventQuery("adfasd");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void EmptyEventQuery()
        {
            new EventQuery("");
        }

        [Test]
        [ExpectedException(typeof(ArgumentNullException))]
        public void NullEventQuery()
        {
            new EventQuery(null);
        }

        [Test]
        public void GetVirtualEvents()
        {
            EventQuery query = new EventQuery("virtual * *");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            try
            {
                Assert.AreEqual(1, events.Count, "Event count");
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var ev in events)
                    {
                        Console.WriteLine("{0}", ev.Print());
                    }
                }
            }
        }

        [Test]
        public void Only_Return_Public_Events_When_Only_Public_Is_Entered()
        {
            EventQuery query = new EventQuery("public event * *");
            var events = query.GetMatchingEvents(myClassWithManyEvents);
            try
            {
                Assert.AreEqual(4, events.Count, "Event count");
                Assert.IsTrue(query.myIsPublic.Value);
                Assert.IsNull(query.myIsInternal);
                Assert.IsNull(query.myIsPrivate);
                Assert.IsNull(query.myIsProtected);
                Assert.IsNull(query.myIsProtectedInernal);
                Assert.IsNull(query.myIsStatic);
                Assert.IsNull(query.myIsVirtual);
            }
            finally
            {
                if (ExceptionHelper.InException)
                {
                    foreach (var ev in events)
                    {
                        Console.WriteLine("{0}", ev.Print());
                    }
                }
            }
        }
    }
}