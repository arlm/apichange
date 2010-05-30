
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BaseLibrary.ApiChanges;

namespace DependantLibV1.WhoInstantiatesQueries
{
    class ClassWhichInstantiatesReferenceType
    {
        public void Creator()
        {
            Func<string> f = () => "";

            new PublicGenericClass<string>(f);
        }
    }
}