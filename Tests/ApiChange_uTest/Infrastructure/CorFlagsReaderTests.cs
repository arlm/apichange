
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Infrastructure;
using ApiChange.Api.Introspection;
using System.IO;
using System.Reflection;

namespace UnitTests.Infrastructure
{
    [TestFixture]
    public class CorFlagsReaderTests
    {
        [Test]
        public void ReadMscorlibAssembly()
        {
            string mscorlib = new FileQuery("GAC:\\mscorlib.dll").Files.Single();
            CorFlagsReader data = null;
            using(var fStream = new FileStream(mscorlib, FileMode.Open, FileAccess.Read))
            {
                data = CorFlagsReader.ReadAssemblyMetadata(fStream);
            }
            Assert.IsNotNull(data);
            Assert.IsTrue(data.IsPureIL);
            Assert.IsTrue(data.IsSigned);
            Assert.AreEqual(ProcessorArchitecture.X86, data.ProcessorArchitecture);
            Assert.IsTrue(data.MajorRuntimeVersion >= 2);
        }

        [Test]
        public void ReadSystemAssembly()
        {
            string system = new FileQuery("GAC:\\System.dll").Files.Single();
            CorFlagsReader data = null;
            using(var fStream = new FileStream(system, FileMode.Open, FileAccess.Read))
            {
                data = CorFlagsReader.ReadAssemblyMetadata(new FileStream(system, FileMode.Open, FileAccess.Read));
            }

            Assert.IsNotNull(data);
            Assert.IsTrue(data.IsPureIL);
            Assert.IsTrue(data.IsSigned);
            Assert.AreEqual(ProcessorArchitecture.MSIL, data.ProcessorArchitecture);
            Assert.IsTrue(data.MajorRuntimeVersion >= 2);
        }

        [Test]
        public void Read_Unmanaged_Dll()
        {
            var data = CorFlagsReader.ReadAssemblyMetadata(new FileQuery("%WINDIR%\\System32\\kernel32.dll").Files.Single());
            Assert.IsNotNull(data);
            Assert.AreEqual(0, data.MajorRuntimeVersion);
            Assert.AreEqual(ProcessorArchitecture.X86, data.ProcessorArchitecture);
        }

        [Test]
        public void Read_Non_PE_File()
        {
            var data = CorFlagsReader.ReadAssemblyMetadata(new FileQuery(@"%WINDIR%\Microsoft.NET\Framework\v2.0.50727\aspnet_perf.ini").Files.Single());
            Assert.IsNull(data);
        }
    }
}
