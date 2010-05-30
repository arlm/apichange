
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.FieldQuery
{
    public class PublicClassWithManyEventsAndMethods
    {
        internal event Func<bool> InternalEvent;
        public event Func<bool> PublicEvent;
        private event Func<bool> PrivateEvent;

        public int PublicProperty
        {
            get;
            set;
        }

        private Func<decimal,decimal> myDecimalField;

        protected KeyValuePair<char, long> ProtectedKeyValuePairField;

        public PublicClassWithManyEventsAndMethods()
        {
        }

        protected PublicClassWithManyEventsAndMethods(string a)
        {
        }

        protected internal PublicClassWithManyEventsAndMethods(string a, int b)
        {

        }

        public Func<int, bool> GenericFunc<T>(T a, int b)
        {
            return null;
        }

        public Func<int, bool> GenericFunc(int a, int b)
        {
            return null;
        }

        internal void InteralVoidFunc()
        {

        }

        private void PrivateVoidFunc()
        {

        }

        public string PublicFuncWithTwoArgs(int a, Func<bool> b)
        {
            return null;
        }
    }
}