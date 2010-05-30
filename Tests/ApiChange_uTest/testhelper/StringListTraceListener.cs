
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace UnitTests
{
    class StringListTraceListener : TraceListener
    {
        List<string> myTraces = new List<string>();

        public void Clear()
        {
            lock (myTraces)
                myTraces.Clear();
        }

        public List<string> Messages
        {
            get
            {
                return GetMessages(null);
            }
        }

        public List<string> GetMessages(Predicate<string> filter)
        {
            lock(myTraces)
            {
                if (filter == null)
                    filter = (str) => true;

                return myTraces.FindAll(filter);
            }
        }


        public override void WriteLine(string message)
        {
            lock (myTraces)
            {
                myTraces.Add(message);
            }
        }

        public override void Write(string message)
        {
            WriteLine(message);
        }

        public override void Write(object o)
        {
            WriteLine(o.ToString());
        }

        public override void WriteLine(object o)
        {
            WriteLine(o.ToString());
        }
    }
}
