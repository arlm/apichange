
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
    class NoneCommandTests
    {
        [Test]
        public void Show_Help_When_No_Arg_Passed()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[0]);
            NoneCommand command = (NoneCommand) data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            StringAssert.Contains("Options: ", command.Out.ToString());
        }

        [Test]
        public void Show_Help_When_UnkownArg_Passed()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse( new string [] { "-invalidarg" });
            NoneCommand command = (NoneCommand) data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            StringAssert.Contains("Error: The command line switch -invalidarg is unknown", command.Out.ToString());
        }
    }
}