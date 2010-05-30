
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeEquivalence
{
    class Class1<A,B,C>
    {
        // GenericInstanceType in Cecil
        Func<string, string, Class1<string,int,Exception>> GenericDelegate;

        // GenericInstanceType in Cecil
        Class1<string, int, Func<string, Func<int>>> m_Field;

        // GenericInstanceType in Cecil
        Func<A, B, int,Func<Func<char,sbyte>,byte>> m_partialGenericField;

        // TypeReference in Cecil
        int m_NonGenericField;
    }

    class Class1<A>
    {

    }

    class Class1
    {
 
    }

}