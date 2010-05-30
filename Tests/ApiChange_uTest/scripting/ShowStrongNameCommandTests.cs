
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.IO;

namespace UnitTests.Scripting
{
    [TestFixture]
    class ShowStrongNameCommandTests : CommandTestBase
    {
        [Test]
        public void Can_Display_StrongName_Of_DependantLibV1()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "showstrongname",
                TestConstants.DependantLibV1
            });

            ShowStrongNameCommand cmd = (ShowStrongNameCommand) data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.AreEqualIgnoringCase("DependantLibV1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null", cmd.Out.ToString().Trim());
        }

        [Test]
        public void Fail_When_No_AdditionalArg_Present()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "showstrongname",
            });

            ShowStrongNameCommand cmd = (ShowStrongNameCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Error: Command -showstrongname expects a file query to display their strong name.", cmd.Out.ToString());
        }

        [Test]
        public void Fail_When_File_Query_Does_Not_Find_Any_File()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "showstrongname",
                Path.Combine(Environment.GetEnvironmentVariable("TEMP"),"*.dllll")
            });

            ShowStrongNameCommand cmd = (ShowStrongNameCommand) data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Error: Command -showstrongname. The query", GetErrorsAndWarnings(cmd));
            StringAssert.Contains("did not match any files.", GetErrorsAndWarnings(cmd));
        }
    }
}