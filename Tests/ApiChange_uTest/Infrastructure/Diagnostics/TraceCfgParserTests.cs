
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Infrastructure;

namespace UnitTests.Infrastructure.Diagnostics
{
    [TestFixture]
    public class TraceCfgParserTests
    {
        public static TypeHashes ApiChange = new TypeHashes("ApiChange");
        public static TypeHashes ApiChange_Infrastructure = new TypeHashes("ApiChange.Infrastructure");
        public static TypeHashes ApiChange_Infrastructure_Internal = new TypeHashes("ApiChange.Infrastructure.Internal");
        public static TypeHashes ApiChange1 = new TypeHashes("ApiChange1");
        public static TypeHashes ApiChange_Infrastructure1 = new TypeHashes("ApiChange.Infrastructure1");
        public static TypeHashes ApiChange_Infrastructure_Internal1 = new TypeHashes("ApiChange.Infrastructure.Internal1");

        TraceReset myReset;

        [SetUp]
        public void SaveTrace()
        {
            myReset = new TraceReset(null);    
        }

        [TearDown]
        public void ResetTrace()
        {
            myReset.Dispose();
        }

        [Test]
        public void Do_Not_Throw_On_Empty_Traceconfig()
        {
            new TraceCfgParser(null);
            var p = new TraceCfgParser("");
            Assert.AreEqual(null, p.Filters);
            Assert.AreEqual(null, p.NotFilters);
            Assert.AreEqual(null, p.OutDevice);
        }

        [Test]
        public void DoNotThrow_When_OutputDevice_Is_Invalid()
        {
            var p = new TraceCfgParser("sxxdd");
            Assert.IsNotNull(p.Filters);
            Assert.IsNull(p.NotFilters);
            Assert.IsNull(p.OutDevice);
        }

        [Test]
        public void Can_Enable_Null_Device()
        {
            var p = new TraceCfgParser("null");
            Assert.IsNotNull(p.OutDevice);
            Assert.AreEqual("NullTraceListener", p.OutDevice.GetType().Name);
            Assert.IsNull(p.NotFilters);
            Assert.IsNotNull(p.Filters);
        }

        [Test]
        public void When_Only_OutputDevice_IsConfigure_Enable_Full_Tracing()
        {
            var p = new TraceCfgParser("null");
            Assert.IsNotNull(p.OutDevice);
            Assert.IsNull(p.NotFilters);
            Assert.IsNotNull(p.Filters);
            Assert.AreEqual(typeof(TraceFilterMatchAll).Name, p.Filters.GetType().Name);
        }

        [Test]
        public void Configure_DebugOutput_With_Inclusion_And_Exclusion_Filter()
        {
            var p = new TraceCfgParser("debugoutput;ApiChange.* all;!ApiChange.Infrastructure.* all");
            Assert.IsNotNull(p.OutDevice);
            Assert.IsNotNull(p.Filters);
            Assert.IsNotNull(p.NotFilters);
            Assert.AreEqual(typeof(TraceFilter).Name, p.Filters.GetType().Name);

            Assert.IsTrue(p.Filters.IsMatch(ApiChange, MessageTypes.All, Level.L1));
            Assert.IsTrue(p.Filters.IsMatch(ApiChange, MessageTypes.Error, Level.L2));

            Assert.IsTrue(p.Filters.IsMatch(ApiChange_Infrastructure, MessageTypes.Info, Level.L3));
            Assert.IsTrue(p.Filters.IsMatch(ApiChange_Infrastructure_Internal, MessageTypes.InOut, Level.L4));

            Assert.IsFalse(p.NotFilters.IsMatch(ApiChange, MessageTypes.Warning, Level.L5));
            Assert.IsTrue(p.NotFilters.IsMatch(ApiChange_Infrastructure, MessageTypes.InOut, Level.Dispose));
            Assert.IsFalse(p.NotFilters.IsMatch(ApiChange_Infrastructure1, MessageTypes.Info, Level.L1));
            Assert.IsTrue(p.NotFilters.IsMatch(ApiChange_Infrastructure_Internal, MessageTypes.Error, Level.L1));
            Assert.IsFalse(p.NotFilters.IsMatch(ApiChange_Infrastructure1, MessageTypes.All, Level.L1));
        }
    }
}
