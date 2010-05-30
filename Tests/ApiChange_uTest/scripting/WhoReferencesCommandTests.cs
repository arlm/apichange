
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
    class WhoReferencesCommandTests : CommandTestBase
    {
        [Test]
        public void Can_Find_BaseLib_Reference()
        {
            WhoReferencesCommand cmd = new WhoReferencesCommand(new CommandData()
            {
              Command = ApiChange.Api.Scripting.Commands.WhoReferences,
              Queries1 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV1) },
              Queries2 = new List<FileQuery> { new FileQuery(TestConstants.DependantLibV1)}
            });

            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();

            StringAssert.Contains("DependantLibV1.dll".ToLower(),output.ToLower());
            StringAssert.Contains("reference baselibraryv1.dll", output.ToLower());
        }

        [Test]
        public void Can_Find_BaseLib_Ref_With_Parsed_Command()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse( new string []
            {
                "-whoreferences", TestConstants.BaseLibV1,
                "-in", TestConstants.DependantLibV1
            });

            WhoReferencesCommand cmd = (WhoReferencesCommand) data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("DependantLibV1.dll".ToLower(), output.ToLower());
            StringAssert.Contains("reference baselibraryv1.dll", output.ToLower());
        }

        [Test]
        public void Fail_When_No_Additional_Args_Present()
        {
            WhoReferencesCommand cmd = new WhoReferencesCommand(new CommandData()
            {
                Command = ApiChange.Api.Scripting.Commands.WhoReferences,
            });

            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("Command -whoreferences needs a file. This is the file which is referenced by the others.", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_From_Is_Missing()
        {
            WhoReferencesCommand cmd = new WhoReferencesCommand(new CommandData()
            {
                Command = ApiChange.Api.Scripting.Commands.WhoReferences,
                Queries2 = new List<FileQuery> { new FileQuery(TestConstants.DependantLibV1) }

            });

            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("Command -whoreferences needs a file.", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_In_Is_Missing()
        {
            WhoReferencesCommand cmd = new WhoReferencesCommand(new CommandData()
            {
                Command = ApiChange.Api.Scripting.Commands.WhoReferences,
                Queries1 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV1) },
            });

            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("Error: Command -whoreferences needs a -in <files> query.", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_Refrence_File_Has_No_File()
        {
            WhoReferencesCommand cmd = new WhoReferencesCommand(new CommandData()
            {
                Command = ApiChange.Api.Scripting.Commands.WhoReferences,
                Queries1 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV1+"dd") },
                Queries2 = new List<FileQuery> { new FileQuery(TestConstants.DependantLibV1) }
            });

            cmd.Out = new StringWriter();
            cmd.Execute();

            StringAssert.Contains("The referenced file", GetErrorsAndWarnings(cmd));
            StringAssert.Contains("BaseLibraryV1.dlldd did not match any files.", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_From_Has_More_Than_One_File()
        {
            WhoReferencesCommand cmd = new WhoReferencesCommand(new CommandData()
            {
                Command = ApiChange.Api.Scripting.Commands.WhoReferences,
                Queries1 = new List<FileQuery> { new FileQuery(
                    Path.Combine(Path.GetDirectoryName(TestConstants.DependantLibV1),"*.dll") ) },
                Queries2 = new List<FileQuery> { new FileQuery(TestConstants.DependantLibV1) }
            });

            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Error: Command -whoreferences ", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_In_Has_No_File()
        {
            WhoReferencesCommand cmd = new WhoReferencesCommand(new CommandData()
            {
                Command = ApiChange.Api.Scripting.Commands.WhoReferences,
                Queries1 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV1) },
                Queries2 = new List<FileQuery> { new FileQuery(TestConstants.DependantLibV1+"ddd") }
            });

            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Error: Command -whoreferences -in", GetErrorsAndWarnings(cmd));
        }
    }
}
