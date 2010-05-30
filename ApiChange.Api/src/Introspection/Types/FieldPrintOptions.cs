
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Introspection
{
    [Flags]
    public enum FieldPrintOptions
    {
        Visibility = 1,
        Modifiers = 2 ,
        SimpleType = 4,
        Value = 8,
        All = Visibility | Modifiers | SimpleType | Value
    }
}
