

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
    public class ShowReferencesCommandTests
    {
        [Test]
        public void Fail_When_ShowReferences_Is_Passed_Alone()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "-sr"
            });

            ShowReferencesCommand cmd = (ShowReferencesCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();

            StringAssert.Contains("Error: Command -showreferences expects at least one file", output);
        }

        [Test]
        public void Can_Show_References_Of_DependantLib()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[] 
            { 
                "-sr", TestConstants.DependantLibV1
            });

            ShowReferencesCommand cmd = (ShowReferencesCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("DependantLibV1.dll ->" , output);
            StringAssert.Contains("mscorlib, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" , output);
            StringAssert.Contains("BaseLibraryV1, Version=1.0.0.0, Culture=neutral, PublicKeyToken=null" , output);
            StringAssert.Contains("System, Version=2.0.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" , output);
            StringAssert.Contains("System.Core, Version=3.5.0.0, Culture=neutral, PublicKeyToken=b77a5c561934e089" , output);
        }
    }
}
