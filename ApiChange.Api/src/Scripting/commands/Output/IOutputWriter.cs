
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Scripting
{
    interface IOutputWriter : IDisposable
    {
        void PrintRow(string fmtString,Func<List<string>> additionalColumnDataProvider, params object[] args);
        void SetCurrentSheet(SheetInfo header);
    }
}
