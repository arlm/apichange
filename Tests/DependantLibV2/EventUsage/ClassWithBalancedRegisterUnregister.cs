
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace DependantLibV2.EventUsage
{
    class BaseClass
    {
        public event Action PublicBaseEvent;
        public static event Func<string> PublicStaticBaseEvent;
        protected Action ProtectedBaseEvent;
    }

    class ClassWithBalancedRegisterUnregister : BaseClass
    {
        void Register(BaseLibrary.EventQueries.ClassWithManyEvents cl)
        {
            cl.PublicEvent += new Func<int>(cl_PublicEvent);
            cl.PublicEvent2 += new Func<bool>(cl_PublicEvent2);
            cl.PublicVirtualEvent += new Func<int>(cl_PublicVirtualEvent);
        }

        void RegisterFromBaseClass()
        {
            base.PublicBaseEvent += new Action(ClassWithBalancedRegisterUnregister_PublicBaseEvent);
            BaseClass.PublicStaticBaseEvent += new Func<string>(BaseClass_PublicStaticBaseEvent);
            base.ProtectedBaseEvent +=  new Action(ClassWithBalancedRegisterUnregister_PublicBaseEvent);
        }

        int RegisterFromOtherClassInstance(string a, string b, string c, int d, BaseClass other)
        {
            other.PublicBaseEvent += new Action(Action_CallBack);
            return d+5;
        }

        void Action_CallBack()
        {
        }

        string BaseClass_PublicStaticBaseEvent()
        {
            throw new NotImplementedException();
        }

        void ClassWithBalancedRegisterUnregister_PublicBaseEvent()
        {
            throw new NotImplementedException();
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
            cl.PublicVirtualEvent -= cl_PublicVirtualEvent;
        }

        static event Action StaticEvent;
        event Action InstanceEvent;
        event Func<int, int> InstanceEventWithParameters;

        void RegisterInstanceAndStaticEvents()
        {
            StaticEvent += new Action(ClassWithBalancedRegisterUnregister_StaticEvent);
            InstanceEvent += new Action(ClassWithBalancedRegisterUnregister_InstanceEvent);
            InstanceEventWithParameters += new Func<int, int>(ClassWithBalancedRegisterUnregister_InstanceEventWithParameters);
        }

        int ClassWithBalancedRegisterUnregister_InstanceEventWithParameters(int arg)
        {
            throw new NotImplementedException();
        }

        void ClassWithBalancedRegisterUnregister_InstanceEvent()
        {
            throw new NotImplementedException();
        }

        void ClassWithBalancedRegisterUnregister_StaticEvent()
        {
            throw new NotImplementedException();
        }
    }
}
