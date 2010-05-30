
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DependantLibV1.WhoInstantiatesQueries
{
    class ClassWhichInstantiatesValueType
    {
        public ClassWhichInstantiatesValueType()
        {
            using (AsyncFlowControl control = new AsyncFlowControl())
            {
                DateTime t1 = new DateTime();
                DateTime t2 = new DateTime(1, 0, 0);
            }
        }
    }
}
