
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using ApiChange.Api.Scripting;

namespace UnitTests.Scripting
{
    public class CommandTestBase
    {
        internal string GetErrorsAndWarnings(CommandBase cmd)
        {
            string[] lines = cmd.Out.ToString().Split(Environment.NewLine.ToCharArray(), StringSplitOptions.RemoveEmptyEntries);
            StringBuilder sb = new StringBuilder();
            foreach (string line in lines)
            {
                if (line.StartsWith("Error") ||
                    line.StartsWith("Warning"))
                    sb.AppendLine(line);
            }

            return sb.ToString();
        }
    }
}