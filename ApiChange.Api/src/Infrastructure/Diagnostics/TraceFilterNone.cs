
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Infrastructure
{
    class TraceFilterMatchNone : TraceFilter
    {
        public TraceFilterMatchNone()
        {
        }

        public override bool IsMatch(TypeHashes type, MessageTypes msgTypeFilter, Level level)
        {
            return false;
        }
    }
}
