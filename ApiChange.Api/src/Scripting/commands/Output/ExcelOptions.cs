
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Scripting
{
    [Flags]
    enum ExcelOptions
    {
        None = 0,
        Visible = 1,
        CloseOnExit = 2
    }
}
