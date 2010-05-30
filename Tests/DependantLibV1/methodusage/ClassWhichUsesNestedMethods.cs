
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibrary.MethodQueries;

namespace DependantLibV1.MethodUsage
{
    class ClassWhichUsesNestedMethods
    {
        void NowItBecomesInteresting()
        {
            NestedClass<string>.InnerClass<int>.InnerInnerClass nested = 
                new NestedClass<string>.InnerClass<int>.InnerInnerClass();

            nested.MethodOfNestedClass("a", -1);
        }
    }
}
