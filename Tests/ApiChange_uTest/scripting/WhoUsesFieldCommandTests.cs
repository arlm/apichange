
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
    class WhoUsesFieldCommandTests
    {
        [Test]
        public void Fail_When_No_More_Args_Are_Passed()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[]
            {
                "-whousesfield"
            });

            WhoUsesFieldCommand cmd =  (WhoUsesFieldCommand) data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Error: Command -whousesfield <files> is missing", cmd.Out.ToString());
        }

        [Test]
        public void Can_Find_FieldUsers_With_Correct_Line_Info()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[]
            {
                "-whousesfield", "PublicBaseClass(*)", TestConstants.BaseLibV1,
                "-in", TestConstants.DependantLibV1
            });

            WhoUsesFieldCommand cmd = (WhoUsesFieldCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("ClassWhichAccessesFields.cs; 25", cmd.Out.ToString());
            StringAssert.Contains("ClassWhichAccessesFields.cs; 26", cmd.Out.ToString());
            StringAssert.Contains("ClassWhichAccessesFields.cs; 27", cmd.Out.ToString());
        }
    }
}
