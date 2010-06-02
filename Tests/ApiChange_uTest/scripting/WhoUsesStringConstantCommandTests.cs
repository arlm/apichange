
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
    public class WhoUsesStringConstantCommandTests
    {
        [Test]
        public void Can_Find_Simple_Const_Comparison()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "-whousesstring","Global A",
                "-in", TestConstants.DependantLibV1
            });

            WhoUsesStringConstantCommand cmd = (WhoUsesStringConstantCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();

            string output = cmd.Out.ToString();

            StringAssert.Contains("DependantLibV1.WhoUsesStringConstants.UsingStringConstants", output);
            StringAssert.Contains("private void CompareAgainstString(String input)", output);
            StringAssert.Contains("Global A string", output);
            StringAssert.Contains("UsingStringConstants.cs; 19", output);

            StringAssert.Contains("private void CreateCompoundString(String str)", output);
            StringAssert.Contains("; 27", output);

            StringAssert.Contains("public void .ctor()", output);
            StringAssert.Contains("; 15",output);

            StringAssert.Contains("private const string ConstCompoundString", output);
        }

        [Test]
        public void Fail_When_WhoUsesString_Is_Alone()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "-whousesstring"
            });

            WhoUsesStringConstantCommand cmd = (WhoUsesStringConstantCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();

            StringAssert.Contains("Error: Command whousesstring expects a search string.", output);
            StringAssert.Contains("Error: -in <files> is missing.", output);
        }

        [Test]
        public void Fail_When_In_Query_Is_Missing()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "-whousesstring", "teststring"
            });

            WhoUsesStringConstantCommand cmd = (WhoUsesStringConstantCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("Error: -in <files> is missing.", output);
        }

        [Test]
        public void Can_Add_Defining_Assemblies()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "-whousesstring", "Global A", TestConstants.BaseLibV1,
                "-in", TestConstants.DependantLibV1
            });

            WhoUsesStringConstantCommand cmd = (WhoUsesStringConstantCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("private void CompareAgainstString(String input", output);
        }
    }
}
