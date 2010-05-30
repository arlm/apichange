
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Introspection
{
    [Flags]
    public enum MethodPrintOption
    {
        ShortNames =1 ,
        Visiblity = 2,
        Modifier  = 4 ,
        ReturnType = 8,
        Parameters = 16,
        ParamNames = 32,
        Full = ShortNames | Visiblity | Modifier | ReturnType | Parameters | ParamNames
    }
}
