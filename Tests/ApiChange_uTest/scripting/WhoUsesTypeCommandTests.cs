
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
    public class WhoUsesTypeCommandTests
    {
        [Test]
        public void Fail_When_No_More_Args_Are_Passed()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[]
            {
                "-whousestype"
            });
            WhoUsesTypeCommand cmd = (WhoUsesTypeCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();

            StringAssert.Contains("Expected: ApiChange -whousesType [typeQuery] [files] -in [files]",output);
            StringAssert.Contains("Error: WhoUsesType the file query to search for type definitions is missing.", output);
            StringAssert.Contains("Error: -in <files> is missing. Search in these files for users of one or more types.", output);
            StringAssert.Contains("rror: The type query is missing.", output);
        }

        [Test]
        public void Find_All_Users_Of_PublicBaseClass()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[]
            {
                "-whousestype", "PublicBaseClass", TestConstants.BaseLibV1,
                "-in", TestConstants.DependantLibV1
            });
            WhoUsesTypeCommand cmd = (WhoUsesTypeCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            string[] lines = output.Split(new char[] { '\n' });
            Assert.IsTrue(lines.Length > 23);
            StringAssert.Contains("m_Base", output);
            StringAssert.Contains("private void ReadFromField()", output);
            StringAssert.Contains("private void WriteToField()", output);
            StringAssert.Contains("private void CallFunctionFromBaseClass()", output);
        }
    }
}
