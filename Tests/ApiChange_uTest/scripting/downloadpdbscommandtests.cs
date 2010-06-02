
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.IO;
using ApiChange.Infrastructure;
using ApiChange.Api.Introspection;

namespace UnitTests.Scripting
{
    [TestFixture]
    public class DownLoadPdbsCommandTests : CommandTestBase
    {
        [Test]
        public void Can_Download_Pdb_With_Symget()
        {
            CommandData data = new CommandData();
            data.Queries1.Add(new FileQuery(typeof(object).Assembly.Location));

            DowndLoadPdbsCommand cmd = new DowndLoadPdbsCommand(data);
            cmd.Out = new StringWriter();
            cmd.Execute();
            if (!SymChkExecutor.bCanStartSymChk)
                Assert.Ignore("Cannot test since symcheck.exe is not in path");

            Assert.AreEqual(0, cmd.myloader.FailedPdbs.Count, "Pdb download must have succeeded");
            Assert.AreEqual(1, cmd.myloader.SucceededPdbCount, "Exactly one pdb for mscorlib.dll should have been downloaded");
            Assert.AreEqual(0, cmd.myloader.FailedPdbs.Count);
        }

        [Test]
        public void FailedPdbCount_Is_One_When_Pdb_Could_Not_Be_Found()
        {
            CommandData data = new CommandData();
            data.SymbolServer = "";
            data.Queries1.Add(new FileQuery(@"%windir%\system32\kernel32.dll"));
            DowndLoadPdbsCommand cmd = new DowndLoadPdbsCommand(data);
            cmd.Out = new StringWriter();
            cmd.Execute();
            Assert.AreEqual(1, cmd.myloader.FailedPdbs.Count, "Pdb download must have failed");
            Assert.AreEqual(0, cmd.myloader.SucceededPdbCount, "No pdb should have been downloaded");
            Assert.AreEqual("kernel32.dll", cmd.myloader.FailedPdbs[0]);
        }

        [Test]
        public void Fail_More_Arguments_Than_Needed()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[] { "-getpdbs", @"%windir%\system32\..\system32\kernel32.dll", "downloaddir", "additionalarg" });
            DowndLoadPdbsCommand cmd = (DowndLoadPdbsCommand) data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Error: The argument", cmd.Out.ToString());
        }

        [Test]
        public void Fail_When_No_More_Argments_Passed()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[] { "-getpdbs" });
            DowndLoadPdbsCommand cmd = (DowndLoadPdbsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("File query for the dll/exe file is not present. E.g. *.dll to download", cmd.Out.ToString());
        }

        [Test]
        public void Fail_When_NotExisting_Directory_Passed()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[] { "-getpdbs", @"c:\notexistingDir\*.dll" });
            DowndLoadPdbsCommand cmd = (DowndLoadPdbsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Invalid directory in", cmd.Out.ToString());
        }
    }
}
