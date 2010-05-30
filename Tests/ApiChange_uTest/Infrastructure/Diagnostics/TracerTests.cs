
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Infrastructure;
using System.Diagnostics;
using System.Threading;

namespace UnitTests.Infrastructure.Diagnostics
{
    [TestFixture]
    public class TracerTests
    {
        SetReset<string> myReset;
        static TypeHandle myType = new TypeHandle(typeof(TracerTests));

        [SetUp]
        public void SaveTraceSettings()
        {
            myReset = new SetReset<string>(
                (value) => Environment.SetEnvironmentVariable(TracerConfig.TraceEnvVarName, value),
                () => Environment.GetEnvironmentVariable(TracerConfig.TraceEnvVarName),
                "file");
        }

        [TearDown]
        public void RestoreTraceSettings()
        {
            myReset.Dispose();
        }

        [Test]
        public void Can_Trace_Info()
        {
            TracerConfig.Reset("null;* *");
            Tracer t = new Tracer();
            t.Info("Hello world {0}", "test");
            t.Warning("");
            t.Error("");
            t.Dispose();
        }



        [Test]
        public void Trace_Instance_Messages_To_DebugOutput()
        {
            TracerConfig.Reset("debugoutput");

            using (Tracer t = new Tracer(myType, "Test_Method"))
            {
                t.Info("Info message");
                t.Warning("Warning message");
                t.Error("Error message");
            }
        }

        [Test]
        public void Trace_Static_Messages_To_DebugOutput()
        {
            TracerConfig.Reset("debugoutput");
            const string MethodName = "Trace_Static_Messages_To_DebugOutput";

            Tracer.Info(Level.L1, myType, MethodName, "Info {0}", "world");
            Tracer.Warning(Level.L2, myType, MethodName, "Warning {0}", "world");
            Tracer.Error(Level.L3, myType, MethodName, "Error {0}", "world");
            Tracer.Error(Level.L4, myType, MethodName, new Exception("Test Exception"));
            Tracer.Error(Level.L5, myType, MethodName, new Exception("Other Test Exception"), "Error {0}", "world");
        }

        [Test]
        public void Leave_Method_WithException()
        {
            TracerConfig.Reset("debugoutput");
            try
            {
                using(Tracer t = new Tracer(myType, "Leave_Method_WithException"))
                {
                    throw new InvalidCastException("test ex");
                }
            }
            catch (Exception)
            {

            }
        }

        void GenerateLevelTraces(Level l)
        {
            string MethodName = "GenerateTraces";

            using (Tracer t = new Tracer(l, myType, MethodName))
            {
                t.Info(l, "Info trace");
                t.Warning(l, "Warning trace");
                t.Error(l, "Error trace");
                t.InfoExecute(() => t.Info("Execute trace"));

                Tracer.Info(l, myType, MethodName, "Static Info");
                Tracer.Warning(l, myType, MethodName, "Static Warning");
                Tracer.Error(l, myType, MethodName, "Static Error");
                Tracer.Error(l, myType, MethodName, new Exception());
                Tracer.Error(l, myType, MethodName, new Exception(), "Static Error Excetion Trace");
                Tracer.Execute(MessageTypes.Info, l, myType, () => Tracer.Info(l, myType, "MethodName", "Static trace execute"));
            }
        }


        [Test]
        public void When_Level1_IsEnabled_AllFlagsAreEnabled()
        {
            TracerConfig.Reset("null;");
            StringListTraceListener stringTracer = new StringListTraceListener();
            TracerConfig.Listeners.Add(stringTracer);

            GenerateLevelTraces(Level.L1);

            try
            {
                Assert.AreEqual(12, stringTracer.Messages.Count);
            }
            finally
            {
                ExceptionHelper.WhenException(() => stringTracer.Messages.ForEach((str) => Console.Write(str)));
            }
        }

        [Test]
        public void When_Level2_IsEnabled_NoOtherTracesArrive()
        {
            TracerConfig.Reset("null;* level2");
            StringListTraceListener stringTracer = new StringListTraceListener();
            TracerConfig.Listeners.Add(stringTracer);

            GenerateLevelTraces(Level.L1);

            try
            {
                Assert.AreEqual(0, stringTracer.Messages.Count);
            }
            finally
            {
                ExceptionHelper.WhenException(() => stringTracer.Messages.ForEach((str) => Console.Write(str)));
            }
        }

        [Test]
        public void When_Level1_And_Error_IsEnabled_NothingElse_Must_Pass()
        {
            TracerConfig.Reset("null;* l1+error");
            StringListTraceListener stringTracer = new StringListTraceListener();
            TracerConfig.Listeners.Add(stringTracer);

            GenerateLevelTraces(Level.L1);

            try
            {
                Assert.AreEqual(4, stringTracer.Messages.Count);
            }
            finally
            {
                ExceptionHelper.WhenException(() => stringTracer.Messages.ForEach((str) => Console.Write(str)));
            }
        }
    }
}
