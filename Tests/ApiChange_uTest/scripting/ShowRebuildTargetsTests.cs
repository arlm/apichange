
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
    public class ShowRebuildTargetsTests : CommandTestBase
    {
        [Test]
        public void Can_Find_Rebuild_Targets_In_Simple_Case()
        {
            ShowRebuildTargetsCommand cmd = new ShowRebuildTargetsCommand(
                new CommandData()
                {
                    Command = ApiChange.Api.Scripting.Commands.ShowRebuildTargets,
                    Queries1 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV1) },
                    Queries2 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV2) },
                    SearchInQuery = new List<FileQuery>() { new FileQuery(
                       Path.GetDirectoryName(TestConstants.DependantLibV1) + "\\*.dll") }
                }
                );

            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Detected 2 assemblies which need a recompilation", cmd.Out.ToString());
            StringAssert.Contains("BaseLibraryV1.dll".ToLower(), cmd.Out.ToString().ToLower());
            StringAssert.Contains("DependantLibV1.dll".ToLower(), cmd.Out.ToString().ToLower());
        }

        [Test]
        public void Can_Find_Rebuild_Targets_When_Only_Old2_Query_Has_Matches()
        {
            ShowRebuildTargetsCommand cmd = new ShowRebuildTargetsCommand(
                          new CommandData()
                          {
                              Command = ApiChange.Api.Scripting.Commands.ShowRebuildTargets,
                              Queries1 = new List<FileQuery>(),
                              OldFiles2 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV1) },
                              Queries2 = new List<FileQuery> { new FileQuery(TestConstants.BaseLibV2) },
                              SearchInQuery = new List<FileQuery>() { new FileQuery(
                                    Path.GetDirectoryName(TestConstants.DependantLibV1) + "\\*.dll") }
                          }
                          );

            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Detected 2 assemblies which need a recompilation", cmd.Out.ToString());
            StringAssert.Contains("BaseLibraryV1.dll".ToLower(), cmd.Out.ToString().ToLower());
            StringAssert.Contains("DependantLibV1.dll".ToLower(), cmd.Out.ToString().ToLower());
        }

        [Test]
        public void Warn_When_No_Previous_Version_Can_Be_Found()
        {
            CommandParser parser = new CommandParser();
            CommandData data  = parser.Parse(new string[] { "-showrebuildtargets",
                "-old", @"%windir%\*.exe",
                "-new", @"%windir%\*.dll", 
                "-searchin", @"%windir%\*.dll" });
            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand) data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("seems to be a new file", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_No_Old_Query_Given()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] { "-showrebuildtargets",
                "-new", @"%windir%\*.dll", 
                "-searchin", @"%windir%\*.dll" });

            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("-old/-old2 <filequery> is missing", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_No_New_Query_Given()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] { "-showrebuildtargets",
                "-old", @"%windir%\*.exe",
                "-searchin", @"%windir%\*.dll" });

            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("-new <filequery> is missing", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_No_SearchIn_Query_Given()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] { "-showrebuildtargets",
                "-old", @"%windir%\*.exe",
                "-new", @"%windir%\*.dll", 
                });

            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("-searchin <filequery> is missing", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Fail_When_Old_Query_Has_No_Matches()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] { "-showrebuildtargets",
                "-old", @"%windir%\*.alois",
                "-new", @"%windir%\*.dll", 
                 "-searchin", @"%windir%\*.dll"
                });

            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("The -old/-old2 query ", GetErrorsAndWarnings(cmd));
            StringAssert.Contains("*.alois did not match any files", GetErrorsAndWarnings(cmd));
        }

        [Test]
        public void Succeed_When_Files_Match_Only_In_Old2_Query()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] { "-showrebuildtargets",
                "-old", @"%windir%\*.alois",
                "-old2", @"%windir%\*.dll",
                "-new", @"%windir%\*.dll", 
                 "-searchin", @"%windir%\*.dll"
                });

            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.AreEqualIgnoringCase("", GetErrorsAndWarnings(cmd));
        }



        [Test]
        public void Fail_When_New_Query_Has_No_Matches()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] { "-showrebuildtargets",
                "-old", @"%windir%\*.dll",
                "-new", @"%windir%\*.alois", 
                 "-searchin", @"%windir%\*.dll"
                });

            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("The -new query", GetErrorsAndWarnings(cmd));
            StringAssert.Contains("*.alois did not match any files.", GetErrorsAndWarnings(cmd));
        }


        [Test]
        public void Fail_When_Searchin_Query_Has_No_Matches()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] { "-showrebuildtargets",
                "-old", @"%windir%\*.dll",
                "-new", @"%windir%\*.dll", 
                 "-searchin", @"%windir%\*.alois"
                });

            ShowRebuildTargetsCommand cmd = (ShowRebuildTargetsCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("The -searchin query", GetErrorsAndWarnings(cmd));
            StringAssert.Contains("*.alois did not match any files.", GetErrorsAndWarnings(cmd));
        }
    }
}
