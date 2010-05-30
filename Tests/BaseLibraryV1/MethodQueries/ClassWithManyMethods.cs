
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.MethodQueries
{
    class ClassWithManyMethods
    {
        public void PublicVoid1() { }
        public void PublicVoid2() { }
        internal void InternalVoid1() {}
        internal void InternalVoid2() { }
        protected void ProtectedVoid1() {}
        protected void ProtectedVoid2() { }
        private void PrivateVoid1() {}
        private void PrivateVoid2() { }
        protected internal void ProtectedInteralVoid1() {}
        protected internal void ProtectedInteralVoid2() { }
        public static void PublicStaticVoid1() {}
        public static void PublicStaticVoid2() { }
        internal static void InternalStaticVoid1() { }
        internal static void InternalStaticVoid2() { }
        private static void PrivateStaticVoid1() { }
        private static void PrivateStaticVoid2() { }
        public virtual void PublicVirtualVoid1() { }
        public virtual void PublicVirtualVoid2() { }
        protected virtual void ProtectedVirtualVoid1() {}
        protected virtual void ProtectedVirtualVoid2() { }
        protected internal virtual void ProtectedInteralVirtualVoid1() { }
        protected internal virtual void ProtectedInteralVirtualVoid2() { }

        string GetString(byte[] bytes)
        {
            return null;
        }

        public Func<int> GenericMethod<T>(T a, int b) { return null; }

        void DisposeToolsHandlers(IList<Exception> exceptions_in_out)
        {

        }

        IList<Exception> MethodWithGenericReturnType()
        {
            return null;
        }
    }
}
