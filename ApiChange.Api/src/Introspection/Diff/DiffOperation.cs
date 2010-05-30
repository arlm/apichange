
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace ApiChange.Api.Introspection
{
    public class DiffOperation
    {
        public bool IsAdded { get; private set; }
        public bool IsRemoved { get { return !IsAdded; } }

        public DiffOperation(bool isAdded)
        {
            IsAdded = isAdded;
        }
    }


}
