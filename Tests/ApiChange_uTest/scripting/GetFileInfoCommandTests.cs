
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using NUnit.Framework;
using ApiChange.Api.Scripting;
using ApiChange.ExternalData;
using System.IO;

namespace UnitTests.Scripting
{
    [TestFixture]
    public class GetFileInfoCommandTests
    {
        [Test]
        public void Do_Show_Help_If_No_More_Args_Are_Passed()
        {
            CommandParser parser = new CommandParser();
            var data = parser.Parse(new string[] 
            { 
                "-getfileinfo"
            });
            GetFileInfoCommand cmd = (GetFileInfoCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            StringAssert.Contains("Missing file name", cmd.Out.ToString());
            StringAssert.Contains("The file  was not found", cmd.Out.ToString());
        }
    }
}