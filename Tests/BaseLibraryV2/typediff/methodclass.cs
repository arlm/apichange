
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BaseLibrary.TypeDiff
{
    public class MethodClass  
    {
        private MethodClass()
        {

        }

        internal int IntProperty //changed visiblity public -> internal
        {
            get;
            private set;  // changed visibility public -> private
        }

        public static int StaticStringProperty // changed return type
        {
            get;
            set;
        }

        internal void VoidFunction()  // Changed visibility -> public -> internal
        {

        }

        public Func<Func<int>> FunctionWithGenericReturnValue() // changed generic parameter of return type
        {
            return null;
        }

        public void FunctionWithOneParameter(Func<int> b) // renamed method paramerter
        {

        }

        public void FunctionWithTwoParameters(string a, Func<int> b) // changed first argument type
        {

        }

        protected IDisposable ProtectedFunction(List<string> args)  // Changed generic parameter argument
        {
            return null;
        }

        public void StaticFunc() // changed function modifier static -> instance
        {

        }

        public void PublicFuncWithOutParameter(int val) // Changed parameter from out to normal by value semantic
        {
            val = 0;
        }

    }
}