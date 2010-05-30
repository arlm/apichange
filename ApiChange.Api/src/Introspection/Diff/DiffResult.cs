
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Diagnostics;

namespace ApiChange.Api.Introspection
{
    public class DiffResult<T> 
    {
        public DiffOperation Operation  { get; private set;   }
        public T ObjectV1   { get; private set; }

        public DiffResult(T v1, DiffOperation diffType)
        {
            ObjectV1 = v1;
            Operation = diffType;
        }

        public override string ToString()
        {
            return String.Format("{0}, {1}", ObjectV1, Operation.IsAdded ? "added" : "removed");
        }
    }
}
