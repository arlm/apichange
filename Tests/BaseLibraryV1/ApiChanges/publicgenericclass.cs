
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.ApiChanges
{
    public class PublicGenericClass<T>
    {
        public PublicGenericClass(Func<T> arg)
        {

        }

        public PublicGenericClass(Func<int> arg)
        {

        }

        public void GenericFunction<V>(V arg)
        {

        }

        public Func<U> GenericFunction<U, V>(U arg1, V arg2)
        {
            return (Func < U >) null;
        }
    }
}