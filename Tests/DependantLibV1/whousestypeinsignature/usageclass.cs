
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class UsageClass
    {
        public void UseGenericMethod()
        {
            ClassWithGenericTypeArguments cl = new ClassWithGenericTypeArguments();
            cl.GenericMethod<decimal, sbyte>(0, 1);
        }
    }
}