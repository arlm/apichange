
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Scripting
{
    class SheetInfo
    {
        public List<ColumnInfo> Columns
        {
            get;
            set;
        }

        public string SheetName
        {
            get;
            set;
        }

        public int HeaderRow
        {
            get;
            set;
        }

        public SheetInfo()
        {
            HeaderRow = 4;
        }
    }
}
