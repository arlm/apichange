
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.EventQueries
{
    class ClassWithManyEvents
    {
        public event Func<int> PublicEvent;
        public event Func<bool> PublicEvent2;
        protected event Func<int> ProtectedEvent;
        internal event Func<int> InternalEvent;
        private event Action PrivateEvent;
        public virtual event Func<int> PublicVirtualEvent;
        public static event Action PublicStaticEvent;
        private event EventHandler<EventArgs> SceneChanged;
    }
}