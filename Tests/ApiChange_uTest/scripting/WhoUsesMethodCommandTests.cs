
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using System.IO;
using ApiChange.Infrastructure;

namespace UnitTests.Scripting
{
    [TestFixture]
    class MethodUsageCommandTests : CommandTestBase
    {
        [Test]
        public void Can_Find_Used_Methods()
        {
            WhoUsesMethodCommand cmd = new WhoUsesMethodCommand(new CommandData() 
            { Command = ApiChange.Api.Scripting.Commands.MethodUsage,
                TypeAndInnerQuery = "PublicBaseClass(public static void StaticMethod())",
              Queries1 = new List<FileQuery>() { new FileQuery(TestConstants.BaseLibV1) },
              Queries2 = new List<FileQuery>() { new FileQuery(TestConstants.BaseLibV1) }
            });

            var writer = new StringWriter();
            cmd.Out = writer;
            cmd.Execute();

            string output = writer.ToString();
            Console.WriteLine(output);  
            StringAssert.Contains("BaseLibrary.ApiChanges.PublicBaseClass", output);
            StringAssert.Contains("public void DoSomeThing(List<float> l)", output); 
        }

        [Test]
        public void Fail_When_Query_Does_Not_Find_Any_Matching_Methods_To_Query_For()
        {
            WhoUsesMethodCommand cmd = new WhoUsesMethodCommand(new CommandData()
            {
                Command = ApiChange.Api.Scripting.Commands.MethodUsage,
                TypeAndInnerQuery = "PublicBaseClass(public static void StaticMethodddddd())",
                Queries1 = new List<FileQuery>() { new FileQuery(TestConstants.BaseLibV1) },
                Queries2 = new List<FileQuery>() { new FileQuery(TestConstants.BaseLibV1) }
            });

            var writer = new StringWriter();
            cmd.Out = writer;
            cmd.Execute();

            string output = writer.ToString();
            StringAssert.Contains("Error: No methods to query found. Aborting query.", output); 
        }

        [Test]
        public void Fail_When_DefiningAssembly_Is_Missing()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[]
            {
                "-whousesmethod",
                "-in", TestConstants.BaseLibV1
            });

            WhoUsesMethodCommand command = (WhoUsesMethodCommand) data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            StringAssert.Contains("Error: Command -whousesmethod <files>",
                         GetErrorsAndWarnings(command));
            StringAssert.Contains("Error: The method query was empty.",
                         GetErrorsAndWarnings(command));
        }

        [Test]
        public void Fail_When_In_Is_Missing()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[]
            {
                "-whousesmethod", TestConstants.BaseLibV1
            });

            WhoUsesMethodCommand command = (WhoUsesMethodCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            StringAssert.Contains("Command -whousesmethod <files>",
                         GetErrorsAndWarnings(command));
            StringAssert.Contains("Error: The method query was empty.",
                         GetErrorsAndWarnings(command));
        }

        [Test]
        public void Fail_When_No_More_Arguments_Passed()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[]
            {
                "-whousesmethod",
            });

            WhoUsesMethodCommand command = (WhoUsesMethodCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();

            StringAssert.Contains("Command -whousesmethod <files>",
               GetErrorsAndWarnings(command));
            StringAssert.Contains("Error: The method query was empty.",
                         GetErrorsAndWarnings(command));
        }
    }
}
