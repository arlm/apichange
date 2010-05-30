
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;

namespace DependantLibV1.WhoUsesTypeInSignature
{
    class InstantiateValueType
    {
        void InstantiateDateTime()
        {
            DateTime time = new DateTime(0, 0, 0);
        }

        void InstantiateTypeInsideUsingStateMent()
        {
            using (AsyncFlowControl flow = new AsyncFlowControl())
            {
                Console.Write("inside async flow control");
            }
        }
    }
}
