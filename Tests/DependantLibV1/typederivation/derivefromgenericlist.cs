
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV1.TypeDerivation
{
    class DeriveFromGenericList<T> : List<T>
    {
    }

    class DeriveFromGenericStringList : List<string>
    {
    }
}