
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.MethodQueries
{
    public class NestedClass<T>
    {
        public class InnerClass<V>
        {
            public class InnerInnerClass
            {
                public void MethodOfNestedClass(T arg1, V arg2)
                {
                  
                }
            }
        }
    }
}
