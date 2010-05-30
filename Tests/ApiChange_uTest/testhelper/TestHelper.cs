
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Mono.Cecil;

namespace UnitTests
{
    static class TestHelper
    {
        public static List<TypeDefinition> SortByTypeName(this List<TypeDefinition> types)
        {
            types.Sort((a, b) => String.Compare(a.FullName, b.FullName));
            return types;
        }
    }
}