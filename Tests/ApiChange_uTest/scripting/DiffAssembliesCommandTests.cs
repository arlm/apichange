
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
    public class DiffAssembliesCommandTests : CommandTestBase
    {
        [Test]
        public void Diff_BaseLibV1_Vs_V2_And_Check_Output()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            {
               "-diff",
               "-new", TestConstants.BaseLibV2,
               "-old", TestConstants.BaseLibV1,
            });

            DiffAssembliesCommand command = (DiffAssembliesCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();

            string output = command.Out.ToString();
            StringAssert.Contains("Removed 15 public type/s", output);
            StringAssert.Contains("- public class BaseLibrary.ApiChanges.PublicGenericClass<T>", output);
            StringAssert.Contains("Added 5 public type/s", output);
            StringAssert.Contains("From 1 assemblies were 15 types removed and 6 changed.", output);
        }

        [Test]
        public void Fail_When_New_Query_Is_Missing()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            {
               "-diff",
               "-old", TestConstants.BaseLibV1,
            });

            DiffAssembliesCommand command = (DiffAssembliesCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            StringAssert.Contains("Error: -new <filequery> is missing.", GetErrorsAndWarnings(command));
        }

        [Test]
        public void Fail_When_Old_Query_Is_Missing()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            {
              "-diff",
              "-new", TestConstants.BaseLibV2,
            });

            DiffAssembliesCommand command = (DiffAssembliesCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            StringAssert.Contains("Error: -old <filequery> is missing.", GetErrorsAndWarnings(command));
        }

        [Test]
        public void Fail_When_Old_Query_Does_Not_Match_Any_File()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            {
               "-diff",
               "-new", TestConstants.BaseLibV2,
               "-old", TestConstants.BaseLibV1+"xxx",
            });
            DiffAssembliesCommand command = (DiffAssembliesCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            string ewarn = GetErrorsAndWarnings(command);
            StringAssert.Contains("Error: The -old query", ewarn);
            StringAssert.Contains("did not match any files", ewarn);
        }

        [Test]
        public void Fail_When_New_Query_Does_Not_Match_Any_File()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            {
               "-diff",
               "-new", TestConstants.BaseLibV2+"xxx",
               "-old", TestConstants.BaseLibV1,
            });
            DiffAssembliesCommand command = (DiffAssembliesCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            string ewarn = GetErrorsAndWarnings(command);
            StringAssert.Contains("Error: The -new query", ewarn);
            StringAssert.Contains("did not match any files", ewarn);
        }

        [Test]
        public void Fail_When_No_Other_Parameters_Given()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            {
               "-diff",
            });

            DiffAssembliesCommand command = (DiffAssembliesCommand)data.GetCommand();
            command.Out = new StringWriter();
            command.Execute();
            string ewarn = GetErrorsAndWarnings(command);
            StringAssert.Contains("Error: -new <filequery> is missing", ewarn);
        }
    }
}
