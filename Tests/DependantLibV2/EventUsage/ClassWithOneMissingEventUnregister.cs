
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV2.EventUsage
{
    class ClassWithOneMissingEventUnregister
    {
        void Register(BaseLibrary.EventQueries.ClassWithManyEvents cl)
        {
            cl.PublicEvent += new Func<int>(cl_PublicEvent);
            cl.PublicEvent2 += new Func<bool>(cl_PublicEvent2);
            cl.PublicVirtualEvent += new Func<int>(cl_PublicVirtualEvent);
        }

        int cl_PublicVirtualEvent()
        {
            throw new NotImplementedException();
        }

        bool cl_PublicEvent2()
        {
            throw new NotImplementedException();
        }

        int cl_PublicEvent()
        {
            throw new NotImplementedException();
        }

        void Unregister(BaseLibrary.EventQueries.ClassWithManyEvents cl)
        {
            cl.PublicEvent -= cl_PublicEvent;
            cl.PublicEvent2 -= cl_PublicEvent2;
            // cl.PublicVirtualEvent -= cl_PublicVirtualEvent;
        }
    }
}
