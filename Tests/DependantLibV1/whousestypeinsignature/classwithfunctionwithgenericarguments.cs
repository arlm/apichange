
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class ClassWithFunctionWithGenericArguments
    {
        protected void FuncWithGenricMethodArgs(bool arg1, KeyValuePair<int,Func<StructWithFunctionWithSearchedParameter>> arg2)
        {

        }
    }
}