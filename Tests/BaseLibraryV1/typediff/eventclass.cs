
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    public class EventClass
    {
        public event Func<int> PublicEventA;
        public event Func<int> PublicEventB;
        public event Func<int> PublicEventC;
        public event Func<int> PublicEventD;        
        public static event Func<bool> PublicStaticEvent;
        public virtual event Func<bool> PublicVirtualEvent;
        protected event Action ProtectedEvent;
        protected event Action ProtectedEventB;
        internal event Action InternalEvent;
        protected internal Action ProtectedInternalEvent;
    }
}