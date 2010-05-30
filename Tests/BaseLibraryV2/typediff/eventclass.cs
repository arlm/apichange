
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    public class EventClass
    {
        public event Func<int> PublicEventD;        // Unchanged event

        // Type Change
        public event Func<bool> PublicEventA; // Changed delegate type

        // Virtual Static 
        public virtual event Func<int> PublicEventB; // Made virtual
        public event Func<bool> PublicStaticEvent; // removed static
        public static event Func<bool> PublicVirtualEvent; // changed from virtual to static 

        // Visibility changes
        protected event Func<int> PublicEventC;  // Changed visibility public -> protected
        public event Action ProtectedEvent;    // Changed visibility protected -> public
        private event Action ProtectedEventB;    // Changed visibility protected -> private
        public event Action InternalEvent;     // changed visibility internal -> public
        public event Action ProtectedInternalEvent; // changed visiblity protected internal -> public

        // New events
        public event Action NewPublicEvent;         // Added new event
        protected event Action NewProtectedEvent;   // Added new protected event
    }
}