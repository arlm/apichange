
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
        static TypeHashes myType = new TypeHashes(typeof(TracerTests));

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

        [Test]
        public void Every_Thread_Prints_His_Exception_Only_Once()
        {
            TracerConfig.Reset("null");
            StringListTraceListener stringTracer = new StringListTraceListener();
            TracerConfig.Listeners.Add(stringTracer);
            ThreadStart method = () =>
                {
                    try
                    {
                        using(Tracer tr1 = new Tracer(myType, "Enclosing Method"))
                        {
                            using (Tracer tracer = new Tracer(myType, "Thread Method"))
                            {
                                throw new NotImplementedException(Thread.CurrentThread.Name);
                            }
                        }
                    }
                    catch (Exception)
                    { }
                };

            List<Thread> threads = new List<Thread>();
            const int ThreadCount = 3;
            List<string> threadNames = new List<string>();
            for(int i=0;i<ThreadCount;i++)
            {
                Thread t = new Thread(method);
                string threadName = "Tracer Thread " + i;
                t.Name = threadName;
                threadNames.Add(threadName);
                t.Start();
                threads.Add(t);
            }

            threads.ForEach(t => t.Join());

            var exLines = stringTracer.GetMessages(line => line.Contains("Exception"));
            Assert.AreEqual(ThreadCount, exLines.Count);
            for(int i=0;i<threadNames.Count;i++)
            {
                Assert.IsTrue(exLines.Any( traceLine => traceLine.Contains(threadNames[i])), 
                    String.Format("Thread with name {0} not found in output", exLines[i]));
            }
            Assert.AreEqual(ThreadCount * 5, stringTracer.Messages.Count);
        }

        [Test]
        public void Trace_Only_Exceptions()
        {
            TracerConfig.Reset("null;* Exception");
            StringListTraceListener stringTracer = new StringListTraceListener();
            TracerConfig.Listeners.Add(stringTracer);

            const int Runs = 20;
            for (int i = 0; i < Runs; i++)
            {
                string exStr = "Ex Nr"+i;
                try
                {
                    using (Tracer tr1 = new Tracer(myType, "Enclosing Method"))
                    {
                        using (Tracer tracer = new Tracer(myType, "Thread Method"))
                        {
                            throw new NotImplementedException(exStr);
                        }
                    }
                }
                catch (Exception)
                { }
                Assert.AreEqual(i+1, stringTracer.Messages.Count);
                Assert.IsTrue(stringTracer.Messages[i].Contains(exStr),
                    String.Format("Got {0} but did not find substring {1}", stringTracer.Messages[i], exStr));
            }
        }

        [Test]
        public void Can_Inject_Faults()
        {
            TracerConfig.Reset("null");
            GenerateLevelTraces(Level.All);

            Tracer.TraceEvent += (msgType, typemethod, time, msg) =>
                {
                    if (msgType == Tracer.MsgType.Error)
                        throw new InvalidOperationException("Injectect Fault");
                };

            Assert.Throws<InvalidOperationException>(() => GenerateLevelTraces(Level.All));

            TracerConfig.Reset(null);
            GenerateLevelTraces(Level.All);
        }

        [Test]
        public void Events_Can_Survive_TraceConfig_Reset()
        {
            TracerConfig.Reset("null");

            Tracer.TraceEvent += (msgType, typemethod, time, msg) =>
            {
                if (msg == "Error trace")
                    throw new InvalidOperationException("Injectect Fault");
            };

            Assert.Throws<InvalidOperationException>(() => GenerateLevelTraces(Level.All));
            TracerConfig.Reset("null", false);
            Assert.Throws<InvalidOperationException>(() => GenerateLevelTraces(Level.All));
            TracerConfig.Reset("null", true);
            GenerateLevelTraces(Level.All);
        }
    } 

}
