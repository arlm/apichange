
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class ClassWithSearchedFieldType
    {
        event Func<char> CharEvent;
    }
}