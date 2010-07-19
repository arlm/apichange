

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Infrastructure
{
    /// <summary>
    /// Severity of trace messages
    /// </summary>
    [Flags]
    public enum MessageTypes
    {
        None = 0,
        Info = 1,
        Instrument = 2,
        Warning = 4,
        Error = 8,
        InOut = 16,
        Exception = 32,
        All = InOut | Info | Instrument | Warning | Error | Exception
    }

    /// <summary>
    /// Trace levels where 1 is the one with only high level traces whereas 5 is the level with
    /// highest details. Trace levels can be combined together so you can look for all high level messages only 
    /// and all errors at all levels. 
    /// </summary>
    [Flags]
    public enum Level
    {
        None = 0,
        L1 = 1,
        L2 = 2,
        L3 = 4,
        L4 = 8,
        L5 = 16,
        Dispose = 32,
        All = L1 | L2 | L3 | L4 | L5 | Dispose
    }
}
