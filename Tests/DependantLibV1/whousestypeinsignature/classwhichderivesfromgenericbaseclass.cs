
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class ClassWhichDerivesFromGenericBaseClass : LinkedList<Func<decimal>>
    {
    }
}