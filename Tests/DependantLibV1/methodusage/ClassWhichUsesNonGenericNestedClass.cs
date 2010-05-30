
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.MethodUsage
{
    class ClassWhichUsesNonGenericNestedClass
    {
        void UsingMethod()
        {
            BaseLibrary.MethodQueries.NestedNonGenericClass.NestedInnerNonGenericClass.NestedInnerInnerNonGenericClass cl = new BaseLibrary.MethodQueries.NestedNonGenericClass.NestedInnerNonGenericClass.NestedInnerInnerNonGenericClass();
            cl.MethodOfInnerMostNonGenericClass();
        }
    }
}
