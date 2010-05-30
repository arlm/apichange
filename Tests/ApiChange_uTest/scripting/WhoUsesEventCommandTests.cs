
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
    public class WhoUsesEventCommandTests
    {
        [Test]
        public void Fails_When_No_More_Args_Are_Passed()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[]
            {
                "-whousesevent"
            });
            WhoUsesEventCommand cmd = (WhoUsesEventCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();

            StringAssert.Contains("Error: Command -whousesevent <files> is", output);
        }

        [Test]
        public void Can_Find_Event_SubScriber_And_UnSubscriber()
        {
            CommandParser parser = new CommandParser();
            CommandData data = parser.Parse(new string[]
            {
                "-whousesevent", "*(*)", TestConstants.BaseLibV1,
                "-in", TestConstants.DependantLibV1
            });
            WhoUsesEventCommand cmd = (WhoUsesEventCommand)data.GetCommand();
            cmd.Out = new StringWriter();
            cmd.Execute();
            string output = cmd.Out.ToString();
            StringAssert.Contains("public void UnRegisterFromPublicStaticEvent", output);
            StringAssert.Contains("RegisterToPublicStaticEvent", output);
            StringAssert.Contains("RegisterToPublicEvent", output);
            StringAssert.Contains("AddEvent",output);
            StringAssert.Contains("RemoveEvent", output);
            StringAssert.Contains("ClassWhichUsesMethods.cs; 60", output);
            StringAssert.Contains("ClassWhichUsesMethods.cs; 65", output);
            StringAssert.Contains("ClassWhichUsesMethods.cs; 70", output);

        }



    }
}
