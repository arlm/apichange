
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Infrastructure;

namespace UnitTests
{
    [TestFixture]
    public class Trace_
    {
        [Test]
        [Explicit]
        public void AA_Enable_DebugOutput()
        {
            TracerConfig.Reset("debugoutput");
        }

        [Test]
        [Explicit]
        public void AA_Enable_File()
        {
            TracerConfig.Reset("file");
        }

        [Test]
        [Explicit]
        public void AAZ_Disable()
        {
            TracerConfig.Reset(null);
        }
    }
}
