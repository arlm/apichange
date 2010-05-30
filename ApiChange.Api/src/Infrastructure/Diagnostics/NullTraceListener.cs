
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ApiChange.Infrastructure
{
    class NullTraceListener : TraceListener
    {
        public override void Write(string message)
        {
            
        }

        public override void WriteLine(object o)
        {
            
        }

        public override void WriteLine(string message)
        {
            
        }
    }
}
