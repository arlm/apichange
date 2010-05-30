
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
    public class WhoImplementsInterfaceCommandTests
    {
        [Test]
        public void Can_Find_Implementing_Assembly()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[]
            {
                "-whoimplementsinterface", "IDisposable", @"GAC:\mscorlib.dll",
                "-in", TestConstants.BaseLibV1
            });

            WhoImplementsInterfaceCommand cmd = (WhoImplementsInterfaceCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("public class BaseLibrary.ApiChanges.PublicBaseClass", cmd.Out.ToString());
            StringAssert.Contains("internal class BaseLibrary.TypeDiff.ClassWithInterfacesAndBaseClass", cmd.Out.ToString());
        }

        [Test]
        public void Can_Find_Generic_Interface_Implementation()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[]
            {
                "-whoimplementsinterface", "IEnumerable<bool>", @"GAC:\mscorlib.dll",
                "-in", TestConstants.BaseLibV1
            });

            WhoImplementsInterfaceCommand cmd = (WhoImplementsInterfaceCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("public interface BaseLibrary.ApiChanges.IGenericInteface", cmd.Out.ToString());
            StringAssert.Contains("public class BaseLibrary.ApiChanges.PublicDerivedClass1", cmd.Out.ToString());
        }

        [Test]
        public void Fail_When_No_More_Args_Are_Passed()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[]
            {
                "-whoimplementsinterface"
            });

            WhoImplementsInterfaceCommand cmd = (WhoImplementsInterfaceCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();

            string output = cmd.Out.ToString();

            StringAssert.Contains("WhoImplementsInterface <files> is missing. This are the files in which for the ", output);
            StringAssert.Contains("-in <files> is missing. Search in these files for implementers of this interface", output);
            StringAssert.Contains("The interface query is missing. ", output);
        }
    }
}
