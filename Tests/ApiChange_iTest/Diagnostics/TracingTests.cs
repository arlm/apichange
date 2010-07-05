
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using UnitTests;
using ApiChange.Infrastructure;
using System.Threading;
using System.Globalization;

namespace ApiChange.IntegrationTests.Diagnostics
{
    [TestFixture]
    public class TracingTests
    {
        static TypeHashes myType = new TypeHashes(typeof(TracingTests));

        TraceReset myReset;

        [SetUp]
        public void SaveTrace()
        {
            myReset = new TraceReset(null);
            Thread.CurrentThread.CurrentUICulture = CultureInfo.InvariantCulture;
        }

        [TearDown]
        public void ResetTrace()
        {
            myReset.Dispose();
        }


        void GenerateTraces()
        {
            using (Tracer t = new Tracer(myType, "GenerateTraces"))
            {
                t.Info("Infoxx message: {0}", "Some other info string");
                t.Warning("Warning message: {0}", "Some other warning string");
                t.Error("Error message {0}", "Some other error string");
            }
        }

        void GenerateStaticTraces()
        {
            string Method = "GenerateStaticTraces";

            Tracer.Info(Level.L1, myType, Method, "Enter method");
            Tracer.Info(Level.L1, myType,Method, "Infoxx message: {0}", "Some other info string");
            Tracer.Warning(Level.L1, myType,Method, "Warning message: {0}", "Some other warning string");
            Tracer.Error(Level.L1, myType,Method, "Error message {0}", "Some other error string");
            Tracer.Info(Level.L1, myType, Method, "Leave method");

        }

        void GenerateTraces(object n)
        {
            for (int i = 0; i < (int)n; i++)
            {
                GenerateTraces();
            }
        }

        [Test]
        public void Performance_Tracing_Is_Off()
        {
            TracerConfig.Reset(null);

            Action acc = GenerateTraces;
            acc.Profile(5 * 1000 * 1000, "Could trace {0} with {frequency} traces/s in {time}s");
        }

        [Test]
        public void Performance_Tracing_To_Null_Device()
        {
            TracerConfig.Reset("null; * *");
            Action acc = GenerateTraces;
            acc.Profile(500 * 1000, "Could trace {0} with {frequency} traces/s in {time}s");
        }

        [Test]
        public void Performance_Tracing_To_Null_Device_Not_Matching()
        {
            TracerConfig.Reset("null; xxx *");
            Action acc = GenerateTraces;
            acc.Profile(500 * 1000, "Could trace {0} with {frequency} traces/s in {time}s");
        }


        [Test]
        public void Performance_Tracing_To_File()
        {
            TracerConfig.Reset( @"file c:\temp\trace_uTest.txt; * *");
            Action acc = GenerateTraces;
            acc.Profile(50 * 1000, "Could trace {0} with {frequency} traces/s in {time}s");
        }

        [Test]
        public void Performance_Tracing_To_DebugOutput()
        {
            TracerConfig.Reset(@"DebugOutput; * *");
            Action acc = GenerateTraces;
            acc.Profile(50 * 1000, "Could trace {0} with {frequency} traces/s in {time}s");
        }

        [Test]
        public void Performance_Static_Traces_To_Null_Device()
        {
            TracerConfig.Reset("null; * *");
            Action acc = GenerateStaticTraces;
            acc.Profile(500 * 1000, "Could trace {0} with {frequency} traces/s in {time}s");

        }


        [Test]
        public void Performance_Of_Action_Delegate()
        {
            int called =0;
            TracerConfig.Reset("null;* Level1");
            Action prof = () =>
                {
                    Tracer.Execute(MessageTypes.Info, Level.L1, myType, () => called++);
                };
            const int Runs = 1000*1000;
            prof.Profile(Runs, "Could execute {0} trace callbacks with {frequency} calls/s in {time}s");
            Assert.AreEqual(Runs, called);
            Console.WriteLine("Executed {0}", called);
        }

        [Test]
        public void Trace_From_ManyThreads()
        {
            TracerConfig.Reset("null;* *");
            StringListTraceListener stringTracer = new StringListTraceListener(); 
            TracerConfig.Listeners.Add(stringTracer);

            List<Thread> threads = new List<Thread>();
            int Threads = 5;
            int GenerateCount = 10 * 1000;
            int LinesPerGenerate = 5;
            for (int i = 0; i < Threads; i++)
            {
                Thread t = new Thread(new ParameterizedThreadStart(GenerateTraces));
                t.IsBackground = true;
                t.Start(GenerateCount);
                threads.Add(t);
            }

            threads.ForEach((t) => t.Join());

            Assert.AreEqual(Threads * GenerateCount * LinesPerGenerate, stringTracer.Messages.Count);
            HashSet<int> threadIds = new HashSet<int>();
            foreach(var line in stringTracer.Messages)
            {
                threadIds.Add(int.Parse(line.Substring(20, 4)));
            }

            Assert.AreEqual(Threads, threadIds.Count);
        }

        [Test]
        [ExpectedException(typeof(NotImplementedException))]
        public void Demo_Show_Leaving_Trace_With_Exception()
        {
            TracerConfig.Reset("console");
            
            SomeMethod();
        }

        void SomeMethod()
        {
            using (Tracer t = new Tracer(myType, "SomeMethod"))
            {
                SomeOtherMethod();
            }
        }

        private void SomeOtherMethod()
        {
            using (Tracer t = new Tracer(myType, "SomeOtherMethod"))
            {
                FaultyMethod();
            }
        }

        private void FaultyMethod()
        {
            throw new NotImplementedException("Hi this a fault");
        }

        [Test]
        public void Demo_Alternate_Flow()
        {
            using(Tracer t = new Tracer(myType, "Demo_Alternate_Flow"))
            {
                t.InfoExecute(()=>
                    {
                        Console.WriteLine("Execute code only when info level is enabled");
                    });
            }
        }
    }
}
