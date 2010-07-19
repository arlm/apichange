using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Infrastructure;
using System.IO;

namespace UnitTests.Infrastructure.Diagnostics
{
    [TestFixture]
    class TracerUseCases
    {
        static TypeHashes myType = new TypeHashes(typeof(TracerUseCases));

        const string tmpFile = "C:\\Test_Fault.txt";

        [TestFixtureSetUp]
        public void WriteFile()
        {
            File.WriteAllLines(tmpFile, new string [] { "Line 1", "Line 2", "Line 3" });
        }

        [TestFixtureTearDown]
        public void DeleteFile()
        {
            File.Delete(tmpFile);
        }


        void DoSomeLogic()
        {
            using (Tracer t = new Tracer(myType, "DoSomeLogic"))
            {
                using (var stream = File.Open(tmpFile, FileMode.Open, FileAccess.Read))
                {
                    t.Instrument("1");
                    using (StreamReader reader = new StreamReader(stream))
                    {
                        while (true)
                        {
                            string line = reader.ReadLine();
                            if (line == null)
                                break;

                            t.Info(Level.L3, "Got line from file {0}", line);
                        }
                    }
                }
            }
        }

        [Test]
        public void Inject_Fault_After_File_Open()
        {
            DoSomeLogic();
            TracerConfig.Reset("null");
            Tracer.TraceEvent += (severity, typemethod, time, message) =>
                {
                    if (severity == Tracer.MsgType.Instrument)
                        throw new IOException("Hi this is an injected fault");
                };

            Assert.Throws<IOException>(() => DoSomeLogic());
        }

        [Test]
        public void Inject_Fault_During_Stream_Read()
        {
            TracerConfig.Reset("null");

            DoSomeLogic();

            int lineCount = 0;
            Tracer.TraceEvent += (severity, typemethod, time, message) =>
            {
                if (severity == Tracer.MsgType.Information && message.Contains("Got line from"))
                {
                    lineCount++;
                    if (lineCount == 2)
                        throw new IOException("Exception during read of second line");
                }
            };

            Assert.Throws<IOException>(() => DoSomeLogic());
        }
    }
}
